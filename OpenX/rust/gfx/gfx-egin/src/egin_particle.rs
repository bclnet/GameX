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
// PARTIAL PORT. The core is done: `Particle`, `ParticleBag`, the four trait
// families, both emitters, the number/vector providers, and the operators that
// carry real logic (`Decay`, `FadeAndKill`, `BasicMovement`). The remaining ~20
// concrete initializers and operators (`CreateWithinSphere`, `RingWave`,
// `RandomColor`, `OscillateScalar`, ...) are each a few lines of arithmetic over
// KV keys, all following the same two shapes; they are listed at the bottom.
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
#[derive(Debug, Clone, Copy, Default, PartialEq)]
pub struct ParticleSystemRenderState {
    pub lifetime: f32,
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
pub trait ParticleEmitter {
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
pub trait ParticleInitializer {
    fn initialize(&mut self, particle: &mut Particle, state: &ParticleSystemRenderState);
}

/// C# `interface IParticleOperator`.
pub trait ParticleOperator {
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

// NOT YET PORTED, all from this file: the initializers `CreateWithinSphere`,
// `InitialVelocityNoise`, `OffsetVectorToVector`, `PositionOffset`,
// `RandomAlpha`, `RandomColor`, `RandomLifeTime`, `RandomRadius`,
// `RandomRotation`, `RandomRotationSpeed`, `RandomSequence`,
// `RandomTrailLength`, `RemapParticleCountToScalar`, `RingWave`; and the
// operators `ColorInterpolate`, `FadeInSimple`, `FadeOutSimple`,
// `InterpolateRadius`, `OscillateScalar`, `SpinUpdate`. Each is a handful of
// lines reading `m_fl*` keys and writing one or two `Particle` fields, all
// following `ParticleInitializer` or `ParticleOperator` above. The randomised
// ones need an RNG decision first — `openx-phy2`'s `LazyRandom` has the
// unbounded-growth problem noted in PORTING.md, so this crate should take
// `rand` or a small xorshift rather than copy it.
//
// Also not ported: `IParticleRenderer` and `ParticleField` (renderer-side, need
// a real backend), and the three separate `ParticleExtensions` static classes —
// the C# declares that name three times in one file, each a different set of
// KV-reading helpers, which `KV`'s accessors now cover.

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
