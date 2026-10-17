// PORT-SOURCE: Gfx/OpenX.Gfx.Egin/Egin_Particle.cs
// PORT-SHA: 50cef39679b48ca4
// PORT-STATUS: done
//
// A data-driven particle system: emitters produce particles, initializers set
// their starting state, operators advance them per frame. Every concrete type
// is configured from an `IDictionary<string, object>` of `m_fl*` keys — Valve
// particle-system KV blobs — which is what `openx_poly`' `KV` enum
// exists for, so it is used here instead of an untyped map.
//
// The concrete initializers and operators are ported. `System.Random` is a
// small xorshift (`Rng` below): `openx-phy2`'s `LazyRandom` grows without bound,
// which this crate was told not to copy. Each `Rng::new` takes the next seed,
// so two initializers built together don't share a sequence. `IParticleRenderer`
// is the draw trait; the OpenGL sprite and trail renderers implement it.
//
// ===================== FIVE C#-SIDE BUGS ==================================
//
//   1. **`IParticleInitializer.Initialize` both mutates and returns.**
//
//          Particle Initialize(ref Particle particle, ParticleSystemRenderState state);
//
//      It takes `ref` *and* returns a `Particle`. Implementations vary in which
//      they actually use, so a caller that reads the return value gets
//      different results from one that reads the `ref` argument — and nothing
//      in the signature says which is authoritative. The trait here mutates
//      only.
//
//   2. **`ParticleBag` with `initialCapacity: 0` and `growable: true` hands out
//      an out-of-bounds index.** `Add` computes `newSize = _particles.Length * 2`
//      = 0, copies nothing, leaves the array empty, and returns `Count++` = 0.
//      The caller then indexes element 0 of a zero-length array — or throws
//      constructing the `LiveParticles` span, whose `Count` now exceeds the
//      backing array.
//
//   3. **`FadeAndKill` divides by `ConstantLifetime` with no zero check**, so a
//      particle whose lifetime was initialised to 0 (which `RandomLifeTime`
//      can do) yields infinity or NaN, and the NaN propagates into `Alpha`.
//
//   4. **`FadeAndKill`'s fade windows divide by `end - start`**, which is zero
//      whenever a KV blob sets the two equal. The defaults avoid it; data does
//      not have to.
//
//   5. **`Decay` and `FadeAndKill` both decrement `Lifetime`.** A KV blob
//      listing both operators — nothing prevents it — ages particles at twice
//      the intended rate. Documented rather than changed, since which one
//      should own the decrement is a data question.
//
// Also: `ContinuousEmitter` computes `_emitInterval = 1 / emitRate` once in its
// constructor, so an `m_flEmitRate` of 0 gives an infinite interval and the
// emitter silently never fires; and `Particle.GetRotationMatrix` composes only
// `Rotation.Z` and `Rotation.Y`, ignoring `Rotation.X` entirely.

use std::collections::HashMap;
use std::sync::atomic::{AtomicU32, Ordering};
use glam::{Mat4, Vec3};
use openx_poly::pool::KV;

/// C# `Particle.Particle(IDictionary<string, object>)` default radius.
const DEFAULT_CONSTANT_RADIUS: f32 = 5.0;

/// C# `struct Particle`.
///
/// The C# declares 17 auto-properties and sets all of them in its constructor;
/// `Default` here is the same initial state.
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Particle {
    pub particleCount: i32,
    pub constantAlpha: f32,
    pub constantColor: Vec3,
    pub constantLifetime: f32,
    pub constantRadius: f32,
    pub alpha: f32,
    pub alphaAlternate: f32,
    pub color: Vec3,
    pub lifetime: f32,
    pub position: Vec3,
    pub positionPrevious: Vec3,
    pub radius: f32,
    pub trailLength: f32,
    pub rotation: Vec3,
    pub rotationSpeed: Vec3,
    pub sequence: i32,
    pub velocity: Vec3,
}

impl Default for Particle {
    fn default() -> Self {
        Self {
            particleCount: 0,
            constantAlpha: 1.0,
            constantColor: Vec3::ONE,
            constantLifetime: 1.0,
            constantRadius: DEFAULT_CONSTANT_RADIUS,
            alpha: 1.0,
            alphaAlternate: 1.0,
            color: Vec3::ONE,
            lifetime: 1.0,
            position: Vec3::ZERO,
            positionPrevious: Vec3::ZERO,
            radius: DEFAULT_CONSTANT_RADIUS,
            trailLength: 1.0,
            rotation: Vec3::ZERO,
            rotationSpeed: Vec3::ZERO,
            sequence: 0,
            velocity: Vec3::ZERO,
        }
    }
}

impl Particle {
    /// C# `Particle(IDictionary<string, object> baseProperties)`.
    pub fn fromKv(baseProperties: &KV) -> Self {
        let mut p = Self::default();
        p.constantRadius = baseProperties
            .get("m_flConstantRadius")
            .and_then(KV::asF32)
            .unwrap_or(DEFAULT_CONSTANT_RADIUS);
        // C#: `new Vector3(v[0], v[1], v[2]) / 255f` from an int64 array.
        p.constantColor = baseProperties
            .get("m_ConstantColor")
            .and_then(KV::asI64Array)
            .filter(|v| v.len() >= 3)
            .map(|v| Vec3::new(v[0] as f32, v[1] as f32, v[2] as f32) / 255.0)
            .unwrap_or(Vec3::ONE);
        p.constantLifetime = baseProperties
            .get("m_flConstantLifespan")
            .and_then(KV::asF32)
            .unwrap_or(1.0);
        p.color = p.constantColor;
        p.lifetime = p.constantLifetime;
        p.radius = p.constantRadius;
        p
    }

    /// C# `GetTransformationMatrix()` — scale then translate.
    ///
    /// C# `Multiply(scale, translation)` is row-vector order; glam reverses it.
    pub fn transformationMatrix(&self) -> Mat4 {
        Mat4::from_translation(self.position) * Mat4::from_scale(Vec3::splat(self.radius))
    }

    /// C# `GetRotationMatrix()`.
    ///
    /// Composes only Z then Y — `Rotation.X` is ignored, as in the C#.
    pub fn rotationMatrix(&self) -> Mat4 {
        Mat4::from_rotation_y(self.rotation.y) * Mat4::from_rotation_z(self.rotation.z)
    }

    /// Fraction of the particle's life elapsed, in `0..=1`.
    ///
    /// `None` when `constantLifetime` is zero — the C# divides anyway (bug 3).
    pub fn ageFraction(&self) -> Option<f32> {
        if self.constantLifetime.abs() < f32::EPSILON {
            return None;
        }
        Some((1.0 - self.lifetime / self.constantLifetime).clamp(0.0, 1.0))
    }

    /// C# `GetScalar`. An unlisted field is `0`.
    pub fn getScalar(&self, field: ParticleField) -> f32 {
        match field {
            ParticleField::Alpha => self.alpha,
            ParticleField::AlphaAlternate => self.alphaAlternate,
            ParticleField::Radius => self.radius,
            _ => 0.0,
        }
    }

    /// C# `GetVector`. An unlisted field is `Vector3.Zero`.
    pub fn getVector(&self, field: ParticleField) -> Vec3 {
        match field {
            ParticleField::Position => self.position,
            ParticleField::PositionPrevious => self.positionPrevious,
            _ => Vec3::ZERO,
        }
    }
}

/// C# `class ParticleBag(int initialCapacity, bool growable)`.
///
/// A `Vec` with an optional cap, rather than a manually grown array. The
/// zero-capacity growth bug (2) cannot occur: `Vec::push` allocates correctly
/// from empty.
#[derive(Debug, Clone)]
pub struct ParticleBag {
    particles: Vec<Particle>,
    capacity: usize,
    growable: bool,
}

impl ParticleBag {
    /// C# `ParticleBag(int, bool)`.
    pub fn new(initialCapacity: usize, growable: bool) -> Self {
        Self {
            particles: Vec::with_capacity(initialCapacity),
            capacity: initialCapacity,
            growable,
        }
    }

    /// C# `Count`.
    #[inline]
    pub fn len(&self) -> usize {
        self.particles.len()
    }

    #[inline]
    pub fn isEmpty(&self) -> bool {
        self.particles.is_empty()
    }

    /// C# `LiveParticles`.
    ///
    /// The C# returns a `Span` over the backing array, which is invalidated by
    /// the next `Add` that grows it — a dangling view the compiler there cannot
    /// catch. Rust's borrow checker makes holding this across an `add` a
    /// compile error.
    #[inline]
    pub fn liveParticles(&mut self) -> &mut [Particle] {
        &mut self.particles
    }

    #[inline]
    pub fn asSlice(&self) -> &[Particle] {
        &self.particles
    }

    /// C# `Add()` — index of the new particle, or `None` when full and not
    /// growable. The C#'s growth policy (double below 1024, then +1024) is
    /// preserved as the reserve hint.
    pub fn add(&mut self, particle: Particle) -> Option<usize> {
        if self.particles.len() >= self.capacity {
            if !self.growable {
                return None; // C# returns -1
            }
            self.capacity = if self.capacity < 1024 {
                (self.capacity * 2).max(1) // `.max(1)` is what fixes bug 2
            } else {
                self.capacity + 1024
            };
            self.particles.reserve(self.capacity - self.particles.len());
        }
        self.particles.push(particle);
        Some(self.particles.len() - 1)
    }

    /// C# `PruneExpired()` — swap-remove anything with `Lifetime <= 0`.
    pub fn pruneExpired(&mut self) {
        let mut i = 0;
        while i < self.particles.len() {
            if self.particles[i].lifetime <= 0.0 {
                self.particles.swap_remove(i);
            } else {
                i += 1;
            }
        }
    }

    /// C# `Clear()`.
    pub fn clear(&mut self) {
        self.particles.clear();
    }
}

/// C# `class ParticleSystemRenderState`.
#[derive(Debug, Clone, Default, PartialEq)]
pub struct ParticleSystemRenderState {
    pub lifetime: f32,
    controlPoints: HashMap<i32, Vec3>,
}

impl ParticleSystemRenderState {
    /// C# `GetControlPoint`. A missing point is `Vector3.Zero`.
    pub fn getControlPoint(&self, cp: i32) -> Vec3 {
        self.controlPoints.get(&cp).copied().unwrap_or(Vec3::ZERO)
    }

    /// C# `SetControlPoint`, which returns `this` for chaining.
    pub fn setControlPoint(&mut self, cp: i32, value: Vec3) -> &mut Self {
        self.controlPoints.insert(cp, value);
        self
    }
}

/// C# `interface INumberProvider`.
pub trait NumberProvider {
    /// C# `NextNumber()` — returns `double` there.
    fn nextNumber(&mut self) -> f64;
}

/// C# `class LiteralNumberProvider`.
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct LiteralNumberProvider(pub f64);

impl NumberProvider for LiteralNumberProvider {
    #[inline]
    fn nextNumber(&mut self) -> f64 {
        self.0
    }
}

/// C# `interface IVectorProvider`.
pub trait VectorProvider {
    fn nextVector(&mut self) -> Vec3;
}

/// C# `class LiteralVectorProvider`.
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct LiteralVectorProvider(pub Vec3);

impl VectorProvider for LiteralVectorProvider {
    #[inline]
    fn nextVector(&mut self) -> Vec3 {
        self.0
    }
}

/// C# `interface IParticleEmitter`.
///
/// The C# `Start(Action particleEmitCallback)` stores a callback and invokes it
/// per particle. `update` returns the count to emit instead, so the emitter does
/// not need to own a closure over the system that owns it.
pub trait ParticleEmitter: Send {
    fn start(&mut self);
    fn stop(&mut self);
    /// How many particles to emit this frame.
    fn update(&mut self, frameTime: f32) -> usize;
    fn isFinished(&self) -> bool;
}

/// C# `class ContinuousEmitter`.
pub struct ContinuousEmitter {
    isFinished: bool,
    emissionDuration: f64,
    startTime: f64,
    emitRate: f64,
    emitInterval: f32,
    time: f32,
    lastEmissionTime: f32,
}

impl ContinuousEmitter {
    /// C# `ContinuousEmitter(baseProperties, keyValues)`.
    ///
    /// A zero `m_flEmitRate` gives the C# an infinite `_emitInterval` and an
    /// emitter that never fires; it falls back to the default rate here.
    pub fn fromKv(keyValues: &KV) -> Self {
        let num = |k: &str, d: f64| keyValues.get(k).and_then(KV::asF64).unwrap_or(d);
        let emitRate = num("m_flEmitRate", 100.0);
        let emitRate = if emitRate <= 0.0 { 100.0 } else { emitRate };
        Self {
            isFinished: false,
            emissionDuration: num("m_flEmissionDuration", 0.0),
            startTime: num("m_flStartTime", 0.0),
            emitRate,
            emitInterval: (1.0 / emitRate) as f32,
            time: 0.0,
            lastEmissionTime: 0.0,
        }
    }
}

impl ParticleEmitter for ContinuousEmitter {
    fn start(&mut self) {
        self.time = 0.0;
        self.lastEmissionTime = 0.0;
        self.isFinished = false;
    }

    fn stop(&mut self) {
        self.isFinished = true;
    }

    /// C# `Update(float frameTime)`.
    ///
    /// The burst cap (`min(5 * rate, numToEmit)`) and the fact that
    /// `_lastEmissionTime` advances by the *uncapped* count — so capped
    /// particles are dropped rather than deferred — are both preserved; the
    /// comment in the C# says that is deliberate ("in case of refocus").
    fn update(&mut self, frameTime: f32) -> usize {
        if self.isFinished {
            return 0;
        }
        self.time += frameTime;
        let start = self.startTime as f32;
        let duration = self.emissionDuration as f32;
        if self.time < start || (duration != 0.0 && self.time > start + duration) {
            return 0;
        }
        let numToEmit = ((self.time - self.lastEmissionTime) / self.emitInterval).floor();
        if numToEmit <= 0.0 {
            return 0;
        }
        let cap = 5.0 * self.emitRate as f32;
        let emitCount = numToEmit.min(cap).max(0.0) as usize;
        self.lastEmissionTime += numToEmit * self.emitInterval;
        emitCount
    }

    fn isFinished(&self) -> bool {
        self.isFinished
    }
}

/// C# `class InstantaneousEmitter` — one burst, then done.
pub struct InstantaneousEmitter {
    particlesToEmit: usize,
    startTime: f32,
    time: f32,
    emitted: bool,
    isFinished: bool,
}

impl InstantaneousEmitter {
    pub fn fromKv(keyValues: &KV) -> Self {
        Self {
            particlesToEmit: keyValues
                .get("m_nParticlesToEmit")
                .and_then(KV::asU64)
                .unwrap_or(1) as usize,
            startTime: keyValues
                .get("m_flStartTime")
                .and_then(KV::asF32)
                .unwrap_or(0.0),
            time: 0.0,
            emitted: false,
            isFinished: false,
        }
    }
}

impl ParticleEmitter for InstantaneousEmitter {
    fn start(&mut self) {
        self.time = 0.0;
        self.emitted = false;
        self.isFinished = false;
    }

    fn stop(&mut self) {
        self.isFinished = true;
    }

    fn update(&mut self, frameTime: f32) -> usize {
        if self.isFinished || self.emitted {
            return 0;
        }
        self.time += frameTime;
        if self.time < self.startTime {
            return 0;
        }
        self.emitted = true;
        self.isFinished = true;
        self.particlesToEmit
    }

    fn isFinished(&self) -> bool {
        self.isFinished
    }
}

/// C# `interface IParticleInitializer`.
///
/// Mutates only — see bug 1 for why the C#'s dual mutate-and-return signature
/// is not reproduced.
pub trait ParticleInitializer: Send {
    fn initialize(&mut self, particle: &mut Particle, state: &ParticleSystemRenderState);
}

/// C# `interface IParticleOperator`.
pub trait ParticleOperator: Send {
    fn update(
        &mut self,
        particles: &mut [Particle],
        frameTime: f32,
        state: &ParticleSystemRenderState,
    );
}

/// C# `class Decay` — the sole job is ageing particles.
#[derive(Debug, Clone, Copy, Default)]
pub struct Decay;

impl ParticleOperator for Decay {
    fn update(&mut self, particles: &mut [Particle], frameTime: f32, _s: &ParticleSystemRenderState) {
        for p in particles {
            p.lifetime -= frameTime;
        }
    }
}

/// C# `class BasicMovement`.
#[derive(Debug, Clone, Copy)]
pub struct BasicMovement {
    pub gravity: Vec3,
    pub drag: f32,
}

impl BasicMovement {
    pub fn fromKv(keyValues: &KV) -> Self {
        Self {
            gravity: keyValues
                .get("m_Gravity")
                .and_then(KV::asVec3)
                .unwrap_or(Vec3::ZERO),
            drag: keyValues.get("m_fDrag").and_then(KV::asF32).unwrap_or(0.0),
        }
    }
}

impl ParticleOperator for BasicMovement {
    fn update(&mut self, particles: &mut [Particle], frameTime: f32, _s: &ParticleSystemRenderState) {
        let acceleration = self.gravity * frameTime;
        for p in particles {
            p.velocity *= 1.0 - self.drag * 30.0 * frameTime;
            p.velocity += acceleration;
            p.positionPrevious = p.position;
            p.position += p.velocity * frameTime;
        }
    }
}

/// C# `class FadeAndKill`.
#[derive(Debug, Clone, Copy)]
pub struct FadeAndKill {
    pub startFadeInTime: f32,
    pub endFadeInTime: f32,
    pub startFadeOutTime: f32,
    pub endFadeOutTime: f32,
    pub startAlpha: f32,
    pub endAlpha: f32,
}

impl FadeAndKill {
    /// C# defaults: 0, .5, .5, 1, 1, 0.
    pub fn fromKv(keyValues: &KV) -> Self {
        let f = |k: &str, d: f32| keyValues.get(k).and_then(KV::asF32).unwrap_or(d);
        Self {
            startFadeInTime: f("m_flStartFadeInTime", 0.0),
            endFadeInTime: f("m_flEndFadeInTime", 0.5),
            startFadeOutTime: f("m_flStartFadeOutTime", 0.5),
            endFadeOutTime: f("m_flEndFadeOutTime", 1.0),
            startAlpha: f("m_flStartAlpha", 1.0),
            endAlpha: f("m_flEndAlpha", 0.0),
        }
    }
}

/// Safe fraction across a window; `None` when the window has zero width, which
/// the C# divides through anyway (bug 4).
fn windowT(time: f32, start: f32, end: f32) -> Option<f32> {
    if time < start || time > end {
        return None;
    }
    let span = end - start;
    if span.abs() < f32::EPSILON {
        return None;
    }
    Some((time - start) / span)
}

impl ParticleOperator for FadeAndKill {
    fn update(&mut self, particles: &mut [Particle], frameTime: f32, _s: &ParticleSystemRenderState) {
        for p in particles {
            // `ageFraction` returns None for a zero lifespan, where the C#
            // produces infinity and then NaN alpha (bug 3).
            if let Some(time) = p.ageFraction() {
                if let Some(t) = windowT(time, self.startFadeInTime, self.endFadeInTime) {
                    p.alpha = (1.0 - t) * self.startAlpha + t * p.constantAlpha;
                }
                if let Some(t) = windowT(time, self.startFadeOutTime, self.endFadeOutTime) {
                    p.alpha = (1.0 - t) * p.constantAlpha + t * self.endAlpha;
                }
            }
            p.lifetime -= frameTime;
        }
    }
}

/// C# `enum ParticleField`.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
#[repr(i64)]
pub enum ParticleField {
    Position = 0,
    PositionPrevious = 2,
    Radius = 3,
    Roll = 4,
    Alpha = 7,
    Yaw = 12,
    AlphaAlternate = 16,
}

impl ParticleField {
    fn fromRaw(v: i64) -> Option<Self> {
        Some(match v {
            0 => Self::Position,
            2 => Self::PositionPrevious,
            3 => Self::Radius,
            4 => Self::Roll,
            7 => Self::Alpha,
            12 => Self::Yaw,
            16 => Self::AlphaAlternate,
            _ => return None,
        })
    }
}

/// Stand-in for `System.Random`. `NextDouble` is `[0, 1)`. `nextInt` is C#
/// `Random.Next(min, max)`: `max` is exclusive, and `min >= max` throws.
struct Rng(u32);

impl Rng {
    fn new() -> Self {
        static NEXT: AtomicU32 = AtomicU32::new(0xA3C59AC3);
        let x = NEXT.fetch_add(0x9E3779B9, Ordering::Relaxed);
        Self(if x == 0 { 1 } else { x })
    }

    fn nextU32(&mut self) -> u32 {
        let mut x = self.0;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        self.0 = if x == 0 { 1 } else { x };
        self.0
    }

    fn nextDouble(&mut self) -> f32 {
        (self.nextU32() as f64 / (u32::MAX as f64 + 1.0)) as f32
    }

    fn nextInt(&mut self, min: i32, max: i32) -> i32 {
        if min >= max { panic!("ArgumentOutOfRangeException: {min} >= {max}"); }
        let span = (max - min) as u32;
        min + (self.nextU32() % span) as i32
    }
}

fn kvF32(kv: &KV, key: &str, default: f32) -> f32 { kv.get(key).and_then(KV::asF32).unwrap_or(default) }
fn kvI64(kv: &KV, key: &str, default: i64) -> i64 { kv.get(key).and_then(KV::asI64).unwrap_or(default) }
fn kvBool(kv: &KV, key: &str, default: bool) -> bool { kv.get(key).and_then(KV::asBool).unwrap_or(default) }
fn kvVec3(kv: &KV, key: &str, default: Vec3) -> Vec3 { kv.get(key).and_then(KV::asVec3).unwrap_or(default) }

fn kvColor(kv: &KV, key: &str, default: Vec3) -> Vec3 {
    kv.get(key).and_then(KV::asI64Array).filter(|v| v.len() >= 3)
        .map(|v| Vec3::new(v[0] as f32, v[1] as f32, v[2] as f32) / 255.0)
        .unwrap_or(default)
}

/// C# `InitialVelocityNoise.Simplex1D` and its two helpers. The integer literal
/// is multiplied as a float, so the low bits are already gone in C#.
fn pseudoRandom(t: f32) -> f32 { ((1013904223517.0f32 * t) % 1664525.0) / 1664525.0 }
fn cosineInterpolate(start: f32, end: f32, mu: f32) -> f32 {
    let mu2 = (1.0 - (mu * std::f32::consts::PI).cos()) / 2.0;
    start * (1.0 - mu2) + end * mu2
}
fn simplex1D(t: f32) -> f32 {
    cosineInterpolate(pseudoRandom(t.floor()), pseudoRandom(t.ceil()), t % 1.0)
}

const PI_OVER_180: f32 = std::f32::consts::PI / 180.0;

/// C# `CreateWithinSphere`.
pub struct CreateWithinSphere {
    random: Rng,
    radiusMin: f32, radiusMax: f32, speedMin: f32, speedMax: f32,
    localMin: Vec3, localMax: Vec3,
}
impl CreateWithinSphere {
    pub fn fromKv(kv: &KV) -> Self {
        Self {
            random: Rng::new(),
            radiusMin: kvF32(kv, "m_fRadiusMin", 0.0), radiusMax: kvF32(kv, "m_fRadiusMax", 0.0),
            speedMin: kvF32(kv, "m_fSpeedMin", 0.0), speedMax: kvF32(kv, "m_fSpeedMax", 0.0),
            localMin: kvVec3(kv, "m_LocalCoordinateSystemSpeedMin", Vec3::ZERO),
            localMax: kvVec3(kv, "m_LocalCoordinateSystemSpeedMax", Vec3::ZERO),
        }
    }
}
impl ParticleInitializer for CreateWithinSphere {
    fn initialize(&mut self, particle: &mut Particle, _state: &ParticleSystemRenderState) {
        let randomVector = Vec3::new(self.random.nextDouble() * 2.0 - 1.0, self.random.nextDouble() * 2.0 - 1.0, self.random.nextDouble() * 2.0 - 1.0);
        let direction = randomVector / randomVector.length();
        let distance = self.radiusMin + (self.random.nextDouble() * (self.radiusMax - self.radiusMin));
        let speed = self.speedMin + (self.random.nextDouble() * (self.speedMax - self.speedMin));
        // One random for the whole local-speed vector, as in the C#.
        let local = self.localMin + (self.random.nextDouble() * (self.localMax - self.localMin));
        particle.position += direction * distance;
        particle.velocity = (direction * speed) + local;
    }
}

/// C# `InitialVelocityNoise`. Only literal providers exist; `NextNumber` on a
/// literal is the value, so it is read once here.
pub struct InitialVelocityNoise { outputMin: Vec3, outputMax: Vec3, noiseScale: f32 }
impl InitialVelocityNoise {
    pub fn fromKv(kv: &KV) -> Self {
        Self {
            outputMin: kvVec3(kv, "m_vecOutputMin", Vec3::ZERO),
            outputMax: kvVec3(kv, "m_vecOutputMax", Vec3::ONE),
            noiseScale: kvF32(kv, "m_flNoiseScale", 1.0),
        }
    }
}
impl ParticleInitializer for InitialVelocityNoise {
    fn initialize(&mut self, particle: &mut Particle, state: &ParticleSystemRenderState) {
        let t = state.lifetime * self.noiseScale;
        let r = Vec3::new(simplex1D(t), simplex1D(t + 101723.0), simplex1D(t + 555557.0));
        particle.velocity = self.outputMin + (r * (self.outputMax - self.outputMin));
    }
}

/// C# `OffsetVectorToVector`.
pub struct OffsetVectorToVector { random: Rng, inputField: ParticleField, outputField: ParticleField, offsetMin: Vec3, offsetMax: Vec3 }
impl OffsetVectorToVector {
    pub fn fromKv(kv: &KV) -> Self {
        Self {
            random: Rng::new(),
            inputField: ParticleField::fromRaw(kvI64(kv, "m_nFieldInput", 0)).unwrap_or(ParticleField::Position),
            outputField: ParticleField::fromRaw(kvI64(kv, "m_nFieldOutput", 0)).unwrap_or(ParticleField::Position),
            offsetMin: kvVec3(kv, "m_vecOutputMin", Vec3::ZERO),
            offsetMax: kvVec3(kv, "m_vecOutputMax", Vec3::ONE),
        }
    }
}
impl ParticleInitializer for OffsetVectorToVector {
    fn initialize(&mut self, particle: &mut Particle, _state: &ParticleSystemRenderState) {
        let input = particle.getVector(self.inputField);
        let offset = Vec3::new(
            self.offsetMin.x + self.random.nextDouble() * (self.offsetMax.x - self.offsetMin.x),
            self.offsetMin.y + self.random.nextDouble() * (self.offsetMax.y - self.offsetMin.y),
            self.offsetMin.z + self.random.nextDouble() * (self.offsetMax.z - self.offsetMin.z),
        );
        let sum = input + offset;
        if self.outputField == ParticleField::Position { particle.position += sum; }
        else if self.outputField == ParticleField::PositionPrevious { particle.positionPrevious = sum; }
    }
}

/// C# `PositionOffset`.
pub struct PositionOffset { random: Rng, offsetMin: Vec3, offsetMax: Vec3 }
impl PositionOffset {
    pub fn fromKv(kv: &KV) -> Self {
        Self { random: Rng::new(), offsetMin: kvVec3(kv, "m_OffsetMin", Vec3::ZERO), offsetMax: kvVec3(kv, "m_OffsetMax", Vec3::ZERO) }
    }
}
impl ParticleInitializer for PositionOffset {
    fn initialize(&mut self, particle: &mut Particle, _state: &ParticleSystemRenderState) {
        let distance = self.offsetMax - self.offsetMin;
        let offset = self.offsetMin + distance * Vec3::new(self.random.nextDouble(), self.random.nextDouble(), self.random.nextDouble());
        particle.position += offset;
    }
}

/// C# `RandomAlpha`. `Random.Next(min, max)` throws when the two are equal,
/// which the defaults (255, 255) are.
pub struct RandomAlpha { random: Rng, alphaMin: i32, alphaMax: i32 }
impl RandomAlpha {
    pub fn fromKv(kv: &KV) -> Self {
        let mut alphaMin = kvI64(kv, "m_nAlphaMin", 255) as i32;
        let mut alphaMax = kvI64(kv, "m_nAlphaMax", 255) as i32;
        if alphaMin > alphaMax { std::mem::swap(&mut alphaMin, &mut alphaMax); }
        Self { random: Rng::new(), alphaMin, alphaMax }
    }
}
impl ParticleInitializer for RandomAlpha {
    fn initialize(&mut self, particle: &mut Particle, _state: &ParticleSystemRenderState) {
        let alpha = self.random.nextInt(self.alphaMin, self.alphaMax) as f32 / 255.0;
        particle.constantAlpha = alpha;
        particle.alpha = alpha;
    }
}

/// C# `RandomColor`. One `NextDouble` lerps the whole color.
pub struct RandomColor { random: Rng, colorMin: Vec3, colorMax: Vec3 }
impl RandomColor {
    pub fn fromKv(kv: &KV) -> Self {
        Self { random: Rng::new(), colorMin: kvColor(kv, "m_ColorMin", Vec3::ONE), colorMax: kvColor(kv, "m_ColorMax", Vec3::ONE) }
    }
}
impl ParticleInitializer for RandomColor {
    fn initialize(&mut self, particle: &mut Particle, _state: &ParticleSystemRenderState) {
        let t = self.random.nextDouble();
        particle.constantColor = self.colorMin + (t * (self.colorMax - self.colorMin));
        particle.color = particle.constantColor;
    }
}

/// C# `RandomLifeTime`.
pub struct RandomLifeTime { random: Rng, lifetimeMin: f32, lifetimeMax: f32 }
impl RandomLifeTime {
    pub fn fromKv(kv: &KV) -> Self { Self { random: Rng::new(), lifetimeMin: kvF32(kv, "m_fLifetimeMin", 0.0), lifetimeMax: kvF32(kv, "m_fLifetimeMax", 0.0) } }
}
impl ParticleInitializer for RandomLifeTime {
    fn initialize(&mut self, particle: &mut Particle, _state: &ParticleSystemRenderState) {
        let lifetime = self.lifetimeMin + ((self.lifetimeMax - self.lifetimeMin) * self.random.nextDouble());
        particle.constantLifetime = lifetime;
        particle.lifetime = lifetime;
    }
}

/// C# `RandomRadius`.
pub struct RandomRadius { random: Rng, radiusMin: f32, radiusMax: f32 }
impl RandomRadius {
    pub fn fromKv(kv: &KV) -> Self { Self { random: Rng::new(), radiusMin: kvF32(kv, "m_flRadiusMin", 0.0), radiusMax: kvF32(kv, "m_flRadiusMax", 0.0) } }
}
impl ParticleInitializer for RandomRadius {
    fn initialize(&mut self, particle: &mut Particle, _state: &ParticleSystemRenderState) {
        particle.constantRadius = self.radiusMin + (self.random.nextDouble() * (self.radiusMax - self.radiusMin));
        particle.radius = particle.constantRadius;
    }
}

/// C# `RandomRotation`. Field 4 writes roll (Z); field 12 writes yaw (Y).
pub struct RandomRotation { random: Rng, degreesMin: f32, degreesMax: f32, degreesOffset: f32, fieldOutput: i64, randomlyFlipDirection: bool }
impl RandomRotation {
    pub fn fromKv(kv: &KV) -> Self {
        Self {
            random: Rng::new(),
            degreesMin: kvF32(kv, "m_flDegreesMin", 0.0), degreesMax: kvF32(kv, "m_flDegreesMax", 360.0),
            degreesOffset: kvF32(kv, "m_flDegrees", 0.0), fieldOutput: kvI64(kv, "m_nFieldOutput", 4),
            randomlyFlipDirection: kvBool(kv, "m_bRandomlyFlipDirection", false),
        }
    }
}
impl ParticleInitializer for RandomRotation {
    fn initialize(&mut self, particle: &mut Particle, _state: &ParticleSystemRenderState) {
        let mut degrees = self.degreesOffset + self.degreesMin + (self.random.nextDouble() * (self.degreesMax - self.degreesMin));
        if self.randomlyFlipDirection && self.random.nextDouble() > 0.5 { degrees *= -1.0; }
        if self.fieldOutput == 4 { particle.rotation.z = degrees * PI_OVER_180; }
        else if self.fieldOutput == 12 { particle.rotation.y = degrees * PI_OVER_180; }
    }
}

/// C# `RandomRotationSpeed`. Yaw writes X and roll writes Z — not the axes
/// `RandomRotation` uses.
pub struct RandomRotationSpeed { random: Rng, fieldOutput: ParticleField, randomlyFlipDirection: bool, degrees: f32, degreesMin: f32, degreesMax: f32 }
impl RandomRotationSpeed {
    pub fn fromKv(kv: &KV) -> Self {
        Self {
            random: Rng::new(),
            fieldOutput: ParticleField::fromRaw(kvI64(kv, "m_nFieldOutput", ParticleField::Roll as i64)).unwrap_or(ParticleField::Roll),
            randomlyFlipDirection: kvBool(kv, "m_bRandomlyFlipDirection", true),
            degrees: kvF32(kv, "m_flDegrees", 0.0), degreesMin: kvF32(kv, "m_flDegreesMin", 0.0), degreesMax: kvF32(kv, "m_flDegreesMax", 360.0),
        }
    }
}
impl ParticleInitializer for RandomRotationSpeed {
    fn initialize(&mut self, particle: &mut Particle, _state: &ParticleSystemRenderState) {
        let mut value = PI_OVER_180 * (self.degrees + self.degreesMin + (self.random.nextDouble() * (self.degreesMax - self.degreesMin)));
        if self.randomlyFlipDirection && self.random.nextDouble() > 0.5 { value *= -1.0; }
        if self.fieldOutput == ParticleField::Yaw { particle.rotationSpeed = Vec3::new(value, 0.0, 0.0); }
        else if self.fieldOutput == ParticleField::Roll { particle.rotationSpeed = Vec3::new(0.0, 0.0, value); }
    }
}

/// C# `RandomSequence`.
pub struct RandomSequence { random: Rng, sequenceMin: i32, sequenceMax: i32, shuffle: bool, counter: i32 }
impl RandomSequence {
    pub fn fromKv(kv: &KV) -> Self {
        Self { random: Rng::new(), sequenceMin: kvI64(kv, "m_nSequenceMin", 0) as i32, sequenceMax: kvI64(kv, "m_nSequenceMax", 0) as i32, shuffle: kvBool(kv, "m_bShuffle", false), counter: 0 }
    }
}
impl ParticleInitializer for RandomSequence {
    fn initialize(&mut self, particle: &mut Particle, _state: &ParticleSystemRenderState) {
        particle.sequence = if self.shuffle { self.random.nextInt(self.sequenceMin, self.sequenceMax + 1) }
        else { self.sequenceMin + if self.sequenceMax > self.sequenceMin { let n = self.counter; self.counter += 1; n % (self.sequenceMax - self.sequenceMin) } else { 0 } };
    }
}

/// C# `RandomTrailLength`.
pub struct RandomTrailLength { random: Rng, minLength: f32, maxLength: f32 }
impl RandomTrailLength {
    pub fn fromKv(kv: &KV) -> Self { Self { random: Rng::new(), minLength: kvF32(kv, "m_flMinLength", 0.1), maxLength: kvF32(kv, "m_flMaxLength", 0.1) } }
}
impl ParticleInitializer for RandomTrailLength {
    fn initialize(&mut self, particle: &mut Particle, _state: &ParticleSystemRenderState) {
        particle.trailLength = self.minLength + (self.random.nextDouble() * (self.maxLength - self.minLength));
    }
}

/// C# `RemapParticleCountToScalar`. Only output field 3 (radius) is handled.
/// A zero input span divides through, as in the C#.
pub struct RemapParticleCountToScalar { fieldOutput: i64, inputMin: i64, inputMax: i64, outputMin: f32, outputMax: f32, scaleInitialRange: bool }
impl RemapParticleCountToScalar {
    pub fn fromKv(kv: &KV) -> Self {
        Self {
            fieldOutput: kvI64(kv, "m_nFieldOutput", 3), inputMin: kvI64(kv, "m_nInputMin", 0), inputMax: kvI64(kv, "m_nInputMax", 10),
            outputMin: kvF32(kv, "m_flOutputMin", 0.0), outputMax: kvF32(kv, "m_flOutputMax", 1.0), scaleInitialRange: kvBool(kv, "m_bScaleInitialRange", false),
        }
    }
}
impl ParticleInitializer for RemapParticleCountToScalar {
    fn initialize(&mut self, particle: &mut Particle, _state: &ParticleSystemRenderState) {
        let particleCount = self.inputMax.min(self.inputMin.max(particle.particleCount as i64));
        let t = (particleCount - self.inputMin) as f32 / (self.inputMax - self.inputMin) as f32;
        let output = self.outputMin + (t * (self.outputMax - self.outputMin));
        if self.fieldOutput == 3 {
            particle.radius = if self.scaleInitialRange { particle.radius * output } else { output };
        }
    }
}

/// C# `RingWave`.
pub struct RingWave { random: Rng, evenDistribution: bool, initialRadius: f32, thickness: f32, particlesPerOrbit: f32, orbitCount: f32 }
impl RingWave {
    pub fn fromKv(kv: &KV) -> Self {
        Self {
            random: Rng::new(), evenDistribution: kvBool(kv, "m_bEvenDistribution", false),
            initialRadius: kvF32(kv, "m_flInitialRadius", 0.0), thickness: kvF32(kv, "m_flThickness", 0.0),
            particlesPerOrbit: kvF32(kv, "m_flParticlesPerOrbit", -1.0), orbitCount: 0.0,
        }
    }
    fn nextAngle(&mut self) -> f32 {
        if self.evenDistribution {
            let offset = self.orbitCount / self.particlesPerOrbit;
            self.orbitCount = (self.orbitCount + 1.0) % self.particlesPerOrbit;
            offset * 2.0 * std::f32::consts::PI
        } else { 2.0 * std::f32::consts::PI * self.random.nextDouble() }
    }
}
impl ParticleInitializer for RingWave {
    fn initialize(&mut self, particle: &mut Particle, _state: &ParticleSystemRenderState) {
        let radius = self.initialRadius + (self.random.nextDouble() * self.thickness);
        let angle = self.nextAngle();
        particle.position += radius * Vec3::new(angle.cos(), angle.sin(), 0.0);
    }
}

/// C# `ColorInterpolate`.
pub struct ColorInterpolate { colorFade: Vec3, fadeStartTime: f32, fadeEndTime: f32 }
impl ColorInterpolate {
    pub fn fromKv(kv: &KV) -> Self { Self { colorFade: kvColor(kv, "m_ColorFade", Vec3::ONE), fadeStartTime: kvF32(kv, "m_flFadeStartTime", 0.0), fadeEndTime: kvF32(kv, "m_flFadeEndTime", 1.0) } }
}
impl ParticleOperator for ColorInterpolate {
    fn update(&mut self, particles: &mut [Particle], _frameTime: f32, _state: &ParticleSystemRenderState) {
        for p in particles.iter_mut() {
            let time = 1.0 - (p.lifetime / p.constantLifetime);
            if time >= self.fadeStartTime && time <= self.fadeEndTime {
                let t = (time - self.fadeStartTime) / (self.fadeEndTime - self.fadeStartTime);
                p.color = ((1.0 - t) * p.constantColor) + (t * self.colorFade);
            }
        }
    }
}

/// C# `FadeInSimple`.
pub struct FadeInSimple { fadeInTime: f32 }
impl FadeInSimple { pub fn fromKv(kv: &KV) -> Self { Self { fadeInTime: kvF32(kv, "m_flFadeInTime", 0.25) } } }
impl ParticleOperator for FadeInSimple {
    fn update(&mut self, particles: &mut [Particle], _frameTime: f32, _state: &ParticleSystemRenderState) {
        for p in particles.iter_mut() {
            let time = 1.0 - (p.lifetime / p.constantLifetime);
            if time <= self.fadeInTime { p.alpha = (time / self.fadeInTime) * p.constantAlpha; }
        }
    }
}

/// C# `FadeOutSimple`.
pub struct FadeOutSimple { fadeOutTime: f32 }
impl FadeOutSimple { pub fn fromKv(kv: &KV) -> Self { Self { fadeOutTime: kvF32(kv, "m_flFadeOutTime", 0.25) } } }
impl ParticleOperator for FadeOutSimple {
    fn update(&mut self, particles: &mut [Particle], _frameTime: f32, _state: &ParticleSystemRenderState) {
        for p in particles.iter_mut() {
            let timeLeft = p.lifetime / p.constantLifetime;
            if timeLeft <= self.fadeOutTime { p.alpha = (timeLeft / self.fadeOutTime) * p.constantAlpha; }
        }
    }
}

/// C# `InterpolateRadius`.
pub struct InterpolateRadius { startTime: f32, endTime: f32, startScale: f32, endScale: f32 }
impl InterpolateRadius {
    pub fn fromKv(kv: &KV) -> Self { Self { startTime: kvF32(kv, "m_flStartTime", 0.0), endTime: kvF32(kv, "m_flEndTime", 1.0), startScale: kvF32(kv, "m_flStartScale", 1.0), endScale: kvF32(kv, "m_flEndScale", 1.0) } }
}
impl ParticleOperator for InterpolateRadius {
    fn update(&mut self, particles: &mut [Particle], _frameTime: f32, _state: &ParticleSystemRenderState) {
        for p in particles.iter_mut() {
            let time = 1.0 - (p.lifetime / p.constantLifetime);
            if time >= self.startTime && time <= self.endTime {
                let t = (time - self.startTime) / (self.endTime - self.startTime);
                p.radius = p.constantRadius * ((self.startScale * (1.0 - t)) + (self.endScale * t));
            }
        }
    }
}

/// C# `OscillateScalar`. The removal of stale rate entries is commented out there.
pub struct OscillateScalar {
    random: Rng, outputField: ParticleField, rateMin: f32, rateMax: f32, frequencyMin: f32, frequencyMax: f32,
    oscillationMultiplier: f32, oscillationOffset: f32, proportional: bool,
    particleRates: HashMap<i32, f32>, particleFrequencies: HashMap<i32, f32>,
}
impl OscillateScalar {
    pub fn fromKv(kv: &KV) -> Self {
        Self {
            random: Rng::new(),
            outputField: ParticleField::fromRaw(kvI64(kv, "m_nField", ParticleField::Alpha as i64)).unwrap_or(ParticleField::Alpha),
            rateMin: kvF32(kv, "m_RateMin", 0.0), rateMax: kvF32(kv, "m_RateMax", 0.0),
            frequencyMin: kvF32(kv, "m_FrequencyMin", 1.0), frequencyMax: kvF32(kv, "m_FrequencyMax", 1.0),
            oscillationMultiplier: kvF32(kv, "m_flOscMult", 2.0), oscillationOffset: kvF32(kv, "m_flOscAdd", 0.5),
            proportional: kvBool(kv, "m_bProportionalOp", true),
            particleRates: HashMap::new(), particleFrequencies: HashMap::new(),
        }
    }
    fn rate(&mut self, id: i32) -> f32 {
        if let Some(v) = self.particleRates.get(&id).copied() { return v; }
        let v = self.rateMin + (self.random.nextDouble() * (self.rateMax - self.rateMin));
        self.particleRates.insert(id, v);
        v
    }
    fn frequency(&mut self, id: i32) -> f32 {
        if let Some(v) = self.particleFrequencies.get(&id).copied() { return v; }
        let v = self.frequencyMin + (self.random.nextDouble() * (self.frequencyMax - self.frequencyMin));
        self.particleFrequencies.insert(id, v);
        v
    }
}
impl ParticleOperator for OscillateScalar {
    fn update(&mut self, particles: &mut [Particle], frameTime: f32, _state: &ParticleSystemRenderState) {
        for p in particles.iter_mut() {
            let rate = self.rate(p.particleCount);
            let frequency = self.frequency(p.particleCount);
            let t = if self.proportional { 1.0 - (p.lifetime / p.constantLifetime) } else { p.lifetime };
            let delta = ((t * frequency * self.oscillationMultiplier) + self.oscillationOffset) * std::f32::consts::PI;
            let delta = delta.sin();
            if self.outputField == ParticleField::Radius { p.radius += delta * rate * frameTime; }
            else if self.outputField == ParticleField::Alpha { p.alpha += delta * rate * frameTime; }
            else if self.outputField == ParticleField::AlphaAlternate { p.alphaAlternate += delta * rate * frameTime; }
        }
    }
}

/// C# `SpinUpdate`.
#[derive(Debug, Clone, Copy, Default)]
pub struct SpinUpdate;
impl ParticleOperator for SpinUpdate {
    fn update(&mut self, particles: &mut [Particle], frameTime: f32, _state: &ParticleSystemRenderState) {
        for p in particles.iter_mut() { p.rotation += p.rotationSpeed * frameTime; }
    }
}

/// C# `interface IParticleRenderer`.
pub trait IParticleRenderer: Send {
    fn render(&mut self, particles: &ParticleBag, viewProjection: Mat4, modelView: Mat4);
    fn setRenderMode(&mut self, renderMode: Option<&str>);
    fn supportedRenderModes(&self) -> Vec<String>;
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::collections::HashMap;

    fn kv(pairs: &[(&str, KV)]) -> KV {
        let mut m = HashMap::new();
        for (k, v) in pairs {
            m.insert(k.to_string(), v.clone());
        }
        KV::Map(m)
    }

    #[test]
    fn particle_defaults_match_the_c_sharp_constructor() {
        let p = Particle::fromKv(&kv(&[]));
        assert_eq!(p.constantRadius, 5.0);
        assert_eq!(p.radius, 5.0);
        assert_eq!(p.constantColor, Vec3::ONE);
        assert_eq!(p.color, Vec3::ONE);
        assert_eq!(p.constantLifetime, 1.0);
        assert_eq!(p.lifetime, 1.0);
        assert_eq!(p.alpha, 1.0);
        assert_eq!(p.trailLength, 1.0);
    }

    #[test]
    fn constant_color_is_read_as_bytes_over_255() {
        let p = Particle::fromKv(&kv(&[(
            "m_ConstantColor",
            KV::Array(vec![KV::Int(255), KV::Int(128), KV::Int(0)]),
        )]));
        assert!((p.constantColor.x - 1.0).abs() < 1e-6);
        assert!((p.constantColor.y - 128.0 / 255.0).abs() < 1e-6);
        assert_eq!(p.constantColor.z, 0.0);
        assert_eq!(p.color, p.constantColor, "Color mirrors ConstantColor");
    }

    #[test]
    fn a_short_color_array_falls_back_rather_than_panicking() {
        let p = Particle::fromKv(&kv(&[(
            "m_ConstantColor",
            KV::Array(vec![KV::Int(255)]),
        )]));
        assert_eq!(p.constantColor, Vec3::ONE, "C# would index out of range");
    }

    #[test]
    fn zero_capacity_growable_bag_works() {
        // The C# computes newSize = 0 * 2 = 0 and returns index 0 into an
        // empty array.
        let mut b = ParticleBag::new(0, true);
        assert_eq!(b.add(Particle::default()), Some(0));
        assert_eq!(b.len(), 1);
        assert_eq!(b.add(Particle::default()), Some(1));
    }

    #[test]
    fn non_growable_bag_refuses_when_full() {
        let mut b = ParticleBag::new(2, false);
        assert_eq!(b.add(Particle::default()), Some(0));
        assert_eq!(b.add(Particle::default()), Some(1));
        assert_eq!(b.add(Particle::default()), None, "C# returns -1");
        assert_eq!(b.len(), 2);
    }

    #[test]
    fn prune_removes_expired_and_keeps_the_rest() {
        let mut b = ParticleBag::new(4, true);
        for (i, life) in [1.0f32, -1.0, 2.0, 0.0].iter().enumerate() {
            let mut p = Particle::default();
            p.lifetime = *life;
            p.sequence = i as i32;
            b.add(p);
        }
        b.pruneExpired();
        assert_eq!(b.len(), 2);
        assert!(b.asSlice().iter().all(|p| p.lifetime > 0.0));
    }

    #[test]
    fn prune_of_all_expired_empties_the_bag() {
        let mut b = ParticleBag::new(4, true);
        for _ in 0..3 {
            let mut p = Particle::default();
            p.lifetime = -1.0;
            b.add(p);
        }
        b.pruneExpired();
        assert!(b.isEmpty());
    }

    #[test]
    fn decay_ages_particles() {
        let mut b = ParticleBag::new(2, true);
        b.add(Particle::default());
        let s = ParticleSystemRenderState::default();
        Decay.update(b.liveParticles(), 0.25, &s);
        assert_eq!(b.asSlice()[0].lifetime, 0.75);
    }

    #[test]
    fn zero_lifespan_does_not_produce_nan_alpha() {
        // The C# divides by ConstantLifetime unconditionally (bug 3).
        let mut p = Particle::default();
        p.constantLifetime = 0.0;
        assert!(p.ageFraction().is_none());
        let mut ops = FadeAndKill::fromKv(&kv(&[]));
        let mut arr = [p];
        ops.update(&mut arr, 0.1, &ParticleSystemRenderState::default());
        assert!(!arr[0].alpha.is_nan(), "alpha must stay finite");
    }

    #[test]
    fn zero_width_fade_window_is_skipped() {
        // C# divides by (end - start) == 0 (bug 4).
        assert!(windowT(0.5, 0.5, 0.5).is_none());
        assert_eq!(windowT(0.5, 0.0, 1.0), Some(0.5));
        assert!(windowT(2.0, 0.0, 1.0).is_none(), "outside the window");
    }

    #[test]
    fn fade_out_drives_alpha_toward_end_alpha() {
        let mut p = Particle::default();
        p.constantLifetime = 1.0;
        p.lifetime = 0.25; // 75% aged, inside the default 0.5..1.0 fade-out
        let mut arr = [p];
        FadeAndKill::fromKv(&kv(&[])).update(&mut arr, 0.0, &ParticleSystemRenderState::default());
        assert!(arr[0].alpha < 1.0 && arr[0].alpha > 0.0, "alpha {}", arr[0].alpha);
    }

    #[test]
    fn continuous_emitter_respects_its_rate() {
        let mut e = ContinuousEmitter::fromKv(&kv(&[("m_flEmitRate", KV::Float(100.0))]));
        e.start();
        // 0.1s at 100/s should be about 10 particles.
        let n = e.update(0.1);
        assert!((9..=11).contains(&n), "emitted {n}");
    }

    #[test]
    fn zero_emit_rate_still_emits() {
        // The C# gets _emitInterval = 1/0 = infinity and never fires.
        let mut e = ContinuousEmitter::fromKv(&kv(&[("m_flEmitRate", KV::Float(0.0))]));
        e.start();
        assert!(e.update(1.0) > 0, "must fall back to a usable rate");
    }

    #[test]
    fn emitter_waits_for_its_start_time() {
        let mut e = ContinuousEmitter::fromKv(&kv(&[("m_flStartTime", KV::Float(1.0))]));
        e.start();
        assert_eq!(e.update(0.5), 0, "not started yet");
        assert!(e.update(1.0) > 0, "now past start time");
    }

    #[test]
    fn stopped_emitter_emits_nothing() {
        let mut e = ContinuousEmitter::fromKv(&kv(&[]));
        e.start();
        e.stop();
        assert!(e.isFinished());
        assert_eq!(e.update(10.0), 0);
    }

    #[test]
    fn instantaneous_emitter_fires_once() {
        let mut e = InstantaneousEmitter::fromKv(&kv(&[("m_nParticlesToEmit", KV::Int(42))]));
        e.start();
        assert_eq!(e.update(0.1), 42);
        assert_eq!(e.update(0.1), 0, "must not fire twice");
        assert!(e.isFinished());
    }

    #[test]
    fn basic_movement_applies_gravity_and_records_previous_position() {
        let mut b = ParticleBag::new(1, true);
        b.add(Particle::default());
        let mut op = BasicMovement { gravity: Vec3::new(0.0, 0.0, -10.0), drag: 0.0 };
        op.update(b.liveParticles(), 0.1, &ParticleSystemRenderState::default());
        let p = b.asSlice()[0];
        assert!(p.velocity.z < 0.0, "gravity applied");
        assert_eq!(p.positionPrevious, Vec3::ZERO);
        assert!(p.position.z < 0.0, "moved");
    }

    #[test]
    fn transformation_matrix_scales_then_translates() {
        let mut p = Particle::default();
        p.radius = 2.0;
        p.position = Vec3::new(5.0, 0.0, 0.0);
        let m = p.transformationMatrix();
        // A unit point at +X should land at position + radius.
        assert!((m.transform_point3(Vec3::X) - Vec3::new(7.0, 0.0, 0.0)).length() < 1e-5);
    }

    #[test]
    fn spin_update_adds_rotation_speed() {
        let mut p = Particle::default();
        p.rotationSpeed = Vec3::new(0.0, 0.0, 2.0);
        SpinUpdate.update(std::slice::from_mut(&mut p), 0.5, &ParticleSystemRenderState::default());
        assert!((p.rotation.z - 1.0).abs() < 1e-6);
    }

    #[test]
    fn remap_count_writes_radius_for_field_three() {
        let mut op = RemapParticleCountToScalar::fromKv(&kv(&[("m_nInputMax", KV::Int(10)), ("m_flOutputMax", KV::Float(1.0))]));
        let mut p = Particle::default();
        p.particleCount = 10;
        op.initialize(&mut p, &ParticleSystemRenderState::default());
        assert!((p.radius - 1.0).abs() < 1e-6);
    }

    #[test]
    fn control_point_zero_is_the_origin_until_set() {
        let mut s = ParticleSystemRenderState::default();
        assert_eq!(s.getControlPoint(0), Vec3::ZERO);
        s.setControlPoint(0, Vec3::new(1.0, 2.0, 3.0));
        assert_eq!(s.getControlPoint(0), Vec3::new(1.0, 2.0, 3.0));
    }

    #[test]
    fn rotation_matrix_ignores_the_x_component() {
        // Documents the C# omission rather than silently adding X.
        let mut a = Particle::default();
        a.rotation = Vec3::new(1.5, 0.0, 0.0);
        assert!(
            (a.rotationMatrix().to_cols_array()[0] - 1.0).abs() < 1e-6,
            "X rotation has no effect"
        );
    }
}
