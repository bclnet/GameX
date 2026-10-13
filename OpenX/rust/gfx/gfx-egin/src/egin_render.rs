// PORT-SOURCE: Gfx/OpenX.Gfx.Egin/Egin_Render.cs
// PORT-SHA: 8ed891fd57d72b60
// PORT-STATUS: partial
//
// Ported: `AABB`, `Camera` (verified against the C# test suite, see the tests at
// the bottom), `RenderMaterial` and `EginRenderer`. Not ported: `IPickingTexture`,
// `IVBIB`, `IEginModel`, `Frustum`, `IMesh`, `DrawCall`, `RenderableMesh`,
// `MeshBatchRequest`, `IMeshCollection`, `Octree`, `Scene`, `SceneNode` and
// `IParticleSystem`, which the model and particle renderers need.
//
// ===================== MATRIX CONVENTION ==================================
//
// This is the thing to get right in this file. `System.Numerics.Matrix4x4` is
// **row-major with a row-vector convention** (`v * M`); `glam::Mat4` is
// **column-major with a column-vector convention** (`M * v`). So:
//
//   * C# `A * B` (apply A then B)  ==  glam `B * A`
//   * C# `M.M[r][c]`               ==  element (r,c) of the *transpose* of the
//                                      equivalent glam matrix
//
// Both are handled at the boundary: `viewProjectionMatrix` composes in glam
// order, and `csElement(r, c)` reads out in the C#'s indexing so the two trees
// can be compared entry by entry. The tests pin all six entries the C# test
// asserts.
//
// `Camera::new()` calls `lookAt(Vec3::ZERO)` from a default location of
// `(1,1,1)`, which is what produces the C#'s documented initial pitch/yaw.

use std::collections::HashMap;
use glam::{Mat4, Vec2, Vec3, Vec4};
use openx_gfx::gfx::{MaterialPropKind, MaterialShaderProp};

/// C# `Camera.CAMERASPEED` — units per second.
pub const CAMERA_SPEED: f32 = 300.0;
/// C# `Camera.FOV` = `MathX.PiOver4`.
pub const FOV: f32 = std::f32::consts::FRAC_PI_4;

const PI_OVER_2: f32 = std::f32::consts::FRAC_PI_2;

/// C# `struct AABB`.
#[derive(Debug, Clone, Copy, PartialEq, Default)]
pub struct Aabb {
    pub min: Vec3,
    pub max: Vec3,
}

impl Aabb {
    pub const fn new(min: Vec3, max: Vec3) -> Self {
        Self { min, max }
    }

    #[inline]
    pub fn size(&self) -> Vec3 {
        self.max - self.min
    }

    #[inline]
    pub fn center(&self) -> Vec3 {
        (self.min + self.max) * 0.5
    }

    /// C# `Contains(object source)` for the `Vector3` case.
    ///
    /// The C# takes `object` and `switch`es on the runtime type, throwing
    /// `ArgumentOutOfRangeException` for anything but `Vector3` or `AABB` — so
    /// a caller passing a `Vector4` finds out at runtime. Split into two typed
    /// methods here.
    ///
    /// Note the bounds are **half-open**: `>= min` but `< max`, so a point on
    /// the max face is outside. That is deliberate for voxel-style indexing and
    /// differs from `openx_gfx`'s `BoundingBox::contains`, which is closed
    /// on both sides.
    #[inline]
    pub fn containsPoint(&self, p: Vec3) -> bool {
        p.x >= self.min.x
            && p.x < self.max.x
            && p.y >= self.min.y
            && p.y < self.max.y
            && p.z >= self.min.z
            && p.z < self.max.z
    }

    /// C# `Contains(object source)` for the `AABB` case — full containment,
    /// and here the comparison is closed (`<= max`), unlike the point case.
    #[inline]
    pub fn containsAabb(&self, o: &Self) -> bool {
        o.min.x >= self.min.x
            && o.max.x <= self.max.x
            && o.min.y >= self.min.y
            && o.max.y <= self.max.y
            && o.min.z >= self.min.z
            && o.max.z <= self.max.z
    }

    /// C# `Intersects(AABB other)`.
    #[inline]
    pub fn intersects(&self, o: &Self) -> bool {
        o.max.x >= self.min.x
            && o.min.x < self.max.x
            && o.max.y >= self.min.y
            && o.min.y < self.max.y
            && o.max.z >= self.min.z
            && o.min.z < self.max.z
    }

    /// C# `Union(AABB other)`.
    #[inline]
    pub fn union(&self, o: &Self) -> Self {
        Self { min: self.min.min(o.min), max: self.max.max(o.max) }
    }

    /// C# `Translate(Vector3 offset)`.
    #[inline]
    pub fn translate(&self, offset: Vec3) -> Self {
        Self { min: self.min + offset, max: self.max + offset }
    }

    /// C# `Transform(Matrix4x4 transform)` — transform all eight corners and
    /// re-bound.
    ///
    /// The C# builds a `Vector4[8]` and reduces with `Vector4.Min/Max`,
    /// carrying a `w` component through the comparison that it then discards.
    /// For an affine transform `w` is 1 throughout so it makes no difference;
    /// for a projective one the min/max over `w` is meaningless. This uses
    /// `transform_point3`, which divides by `w` properly.
    pub fn transform(&self, m: Mat4) -> Self {
        let (lo, hi) = (self.min, self.max);
        let corners = [
            Vec3::new(lo.x, lo.y, lo.z),
            Vec3::new(hi.x, lo.y, lo.z),
            Vec3::new(hi.x, hi.y, lo.z),
            Vec3::new(lo.x, hi.y, lo.z),
            Vec3::new(lo.x, hi.y, hi.z),
            Vec3::new(lo.x, lo.y, hi.z),
            Vec3::new(hi.x, lo.y, hi.z),
            Vec3::new(hi.x, hi.y, hi.z),
        ];
        let first = m.transform_point3(corners[0]);
        let (mut min, mut max) = (first, first);
        for c in &corners[1..] {
            let p = m.transform_point3(*c);
            min = min.min(p);
            max = max.max(p);
        }
        Self { min, max }
    }
}

impl std::fmt::Display for Aabb {
    /// C# `ToString()` — note the unbalanced trailing `)`, preserved so log
    /// output matches between the trees.
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "AABB [({},{},{}) -> ({},{},{}))",
            self.min.x, self.min.y, self.min.z, self.max.x, self.max.y, self.max.z
        )
    }
}

/// C# `abstract class RenderMaterial`; a backend's material holds one as `base` and adds
/// `Render(GfxShader)` / `PostRender()`. C#'s `MaterialShaderProp` (or its subclass
/// `MaterialShaderVProp`) is the `Shader` / `ShaderV` case of `MaterialPropKind`.
#[derive(Clone)]
pub struct RenderMaterial {
    pub material: MaterialPropKind,
    pub textures: HashMap<String, i32>,
    pub isBlended: bool,
    pub isToolsMaterial: bool,
    pub alphaTestReference: f32,
    pub isAdditiveBlend: bool,
    pub isRenderBackfaces: bool,
}

impl RenderMaterial {
    pub fn new(material: MaterialPropKind) -> std::io::Result<Self> {
        let mut s = Self { material: material.clone(), textures: HashMap::new(), isBlended: false, isToolsMaterial: false, alphaTestReference: 0.0, isAdditiveBlend: false, isRenderBackfaces: false };
        match &material {
            MaterialPropKind::ShaderV(p) => {
                let int = |k: &str| p.intParams.get(k).copied();
                if int("F_ALPHA_TEST") == Some(1) && p.floatParams.contains_key("g_flAlphaTestReference") { s.alphaTestReference = p.floatParams["g_flAlphaTestReference"]; }
                s.isToolsMaterial = p.intAttributes.contains_key("tools.toolsmaterial");
                s.isBlended = int("F_TRANSLUCENT") == Some(1) || p.intAttributes.contains_key("mapbuilder.water") || p.shader.shaderName == "vr_glass.vfx" || p.shader.shaderName == "tools_sprite.vfx";
                s.isAdditiveBlend = int("F_ADDITIVE_BLEND") == Some(1);
                s.isRenderBackfaces = int("F_RENDER_BACKFACES") == Some(1);
            }
            MaterialPropKind::Shader(_) => {}
            _ => return Err(std::io::Error::new(std::io::ErrorKind::InvalidInput, "ArgumentOutOfRangeException: material")),
        }
        Ok(s)
    }

    /// C#'s `MaterialShaderProp` view of `material`.
    pub fn shader(&self) -> &MaterialShaderProp {
        match &self.material { MaterialPropKind::ShaderV(p) => &p.shader, MaterialPropKind::Shader(p) => p, _ => unreachable!("RenderMaterial::new only accepts shader materials") }
    }
}

/// C# `abstract class Camera`.
///
/// The C# makes this abstract solely for `GfxViewport`, the one backend hook.
/// That is a trait ([`CameraViewport`]); the state and maths live in this
/// struct, so they are testable without a backend. C#'s `SetViewport` ends by
/// calling `GfxViewport`; here a backend's camera (`GLCamera`) calls
/// `setViewport` and then its own `gfxViewport`. `ViewFrustum` and `Picker`
/// aren't ported (see the end of the file).
#[derive(Debug, Clone)]
pub struct Camera {
    pub location: Vec3,
    pub pitch: f32,
    pub yaw: f32,
    pub scale: f32,
    pub projectionMatrix: Mat4,
    pub cameraViewMatrix: Mat4,
    pub viewProjectionMatrix: Mat4,
    pub windowSize: glam::IVec2,
    pub aspectRatio: f32,
}

impl Default for Camera {
    /// C# `Camera() => LookAt(new Vector3(0))`, from `Location = new(1)`.
    fn default() -> Self {
        let mut c = Self {
            location: Vec3::ONE,
            pitch: 0.0,
            yaw: 0.0,
            scale: 1.0,
            projectionMatrix: Mat4::ZERO,
            cameraViewMatrix: Mat4::ZERO,
            viewProjectionMatrix: Mat4::ZERO,
            windowSize: glam::IVec2::ZERO,
            aspectRatio: 0.0,
        };
        c.lookAt(Vec3::ZERO);
        c
    }
}

impl Camera {
    pub fn new() -> Self { Self::default() }

    /// Reads a matrix entry with **C#'s** `M[row][col]` indexing, so the two
    /// trees can be compared numerically. See the convention note above.
    pub fn csElement(m: Mat4, row: usize, col: usize) -> f32 {
        // C# is the transpose of the glam matrix, so (r,c) there is (c,r) here,
        // and glam's column-major array indexes as [col * 4 + row].
        m.to_cols_array()[row * 4 + col]
    }

    /// C# `RecalculateMatrices()`: `CreateScale(Scale) * CreateLookAt(...)`, then
    /// `CameraViewMatrix * ProjectionMatrix`. Both products reverse in glam.
    pub fn recalculateMatrices(&mut self) {
        let look = Mat4::look_at_rh(self.location, self.location + self.getForwardVector(), Vec3::Z);
        self.cameraViewMatrix = look * Mat4::from_scale(Vec3::splat(self.scale));
        self.viewProjectionMatrix = self.projectionMatrix * self.cameraViewMatrix;
    }

    /// Calculate forward vector from pitch and yaw
    pub fn getForwardVector(&self) -> Vec3 { Vec3::new(self.yaw.cos() * self.pitch.cos(), self.yaw.sin() * self.pitch.cos(), self.pitch.sin()) }

    pub fn getRightVector(&self) -> Vec3 { Vec3::new((self.yaw - PI_OVER_2).cos(), (self.yaw - PI_OVER_2).sin(), 0.0) }

    /// C# `SetViewport(int x, int y, int width, int height)`, without its last
    /// step: the backend calls `CameraViewport::gfxViewport` after this.
    pub fn setViewport(&mut self, x: i32, y: i32, width: i32, height: i32) {
        // store window size and aspect ratio
        self.aspectRatio = width as f32 / height as f32;
        self.windowSize = glam::IVec2::new(width, height);
        // calculate projection matrix
        self.projectionMatrix = Mat4::perspective_rh(FOV, self.aspectRatio, 1.0, 40000.0);
        self.recalculateMatrices();
    }

    /// C# `CopyFrom(Camera fromOther)`. Like the C#, it doesn't copy `Scale`.
    pub fn copyFrom(&mut self, fromOther: &Camera) {
        self.aspectRatio = fromOther.aspectRatio;
        self.windowSize = fromOther.windowSize;
        self.location = fromOther.location;
        self.pitch = fromOther.pitch;
        self.yaw = fromOther.yaw;
        self.projectionMatrix = fromOther.projectionMatrix;
        self.cameraViewMatrix = fromOther.cameraViewMatrix;
        self.viewProjectionMatrix = fromOther.viewProjectionMatrix;
    }

    pub fn setLocation(&mut self, location: Vec3) {
        self.location = location;
        self.recalculateMatrices();
    }

    pub fn setLocationPitchYaw(&mut self, location: Vec3, pitch: f32, yaw: f32) {
        self.location = location;
        self.pitch = pitch;
        self.yaw = yaw;
        self.recalculateMatrices();
    }

    pub fn lookAt(&mut self, target: Vec3) {
        let dir = (target - self.location).normalize();
        self.yaw = dir.y.atan2(dir.x);
        self.pitch = dir.z.asin();
        self.clampRotation();
        self.recalculateMatrices();
    }

    /// C# `SetFromTransformMatrix(Matrix4x4)`: reads C#'s first basis row
    /// (`M11, M12, M13`) as the view direction, and doesn't clamp.
    pub fn setFromTransformMatrix(&mut self, matrix: Mat4) {
        self.location = matrix.w_axis.truncate();
        // extract view direction from view matrix and use it to calculate pitch and yaw
        let dir = Vec3::new(Self::csElement(matrix, 0, 0), Self::csElement(matrix, 0, 1), Self::csElement(matrix, 0, 2));
        self.yaw = dir.y.atan2(dir.x);
        self.pitch = dir.z.asin();
        self.recalculateMatrices();
    }

    pub fn setScale(&mut self, scale: f32) {
        self.scale = scale;
        self.recalculateMatrices();
    }

    /// Prevent camera from going upside-down
    pub fn clampRotation(&mut self) {
        if self.pitch >= PI_OVER_2 { self.pitch = PI_OVER_2 - 0.001; }
        else if self.pitch <= -PI_OVER_2 { self.pitch = -PI_OVER_2 + 0.001; }
    }
}

/// C# `Camera.GfxViewport`, the one abstract member, and `Camera.Tick`, its one virtual.
pub trait CameraViewport {
    fn gfxViewport(&mut self, x: i32, y: i32, width: i32, height: i32);
    fn tick(&mut self, deltaTime: f32) {}
}

// #region Render

/// C# `abstract class EginRenderer : Renderer`.
pub trait EginRenderer: openx_gfx::gfx_render::Renderer {
    //fn boundingBox(&self) -> Aabb;
    fn getViewport(&self, size: (i32, i32)) -> Option<(i32, i32)> { None }
    fn render(&mut self, camera: &Camera, pass: openx_gfx::gfx_render::Pass) {}
}

// NOT PORTED: `IPickingTexture`, `OnDiskBufferData` (+ `Attribute`,
// `RenderSlotType`), `IVBIB`, `IEginModel`. All are interfaces or plain
// descriptors for GPU buffer uploads; they need a real backend to be shaped
// against, and none exists in the ported tree yet.

#[cfg(test)]
mod tests {
    use super::*;

    // The expected values below come from `OpenX.GfxTests/Egin/Gfx_Render.cs`
    // — the C# test suite's own assertions. Matching them means this port agrees
    // with the C# numerically, not just structurally, and it pins the
    // row-major/column-major mapping documented at the top of this file.

    fn approx(a: f32, b: f32, tol: f32) -> bool {
        (a - b).abs() < tol
    }

    #[test]
    fn initialPitchAndYawMatchTheCSharp() {
        let c = Camera::new();
        assert!(approx(c.pitch, -0.6154797, 1e-6), "pitch {}", c.pitch);
        assert!(approx(c.yaw, -2.3561945, 1e-6), "yaw {}", c.yaw);
    }

    #[test]
    fn forwardVectorMatchesTheCSharp() {
        let v = Camera::new().getForwardVector();
        for (i, got) in [v.x, v.y, v.z].iter().enumerate() {
            assert!(approx(*got, -0.577350259, 1e-6), "component {i} = {got}");
        }
    }

    #[test]
    fn rightVectorMatchesTheCSharp() {
        let v = Camera::new().getRightVector();
        assert!(approx(v.x, -0.707107, 1e-5), "x {}", v.x);
        assert!(approx(v.y, 0.7071066, 1e-5), "y {}", v.y);
        assert_eq!(v.z, 0.0);
    }

    #[test]
    fn projectionMatrixMatchesTheCSharp() {
        let mut c = Camera::new();
        c.setViewport(0, 0, 100, 100);
        assert_eq!(c.aspectRatio, 1.0);
        assert_eq!(c.windowSize, glam::IVec2::new(100, 100));
        let m11 = Camera::csElement(c.projectionMatrix, 0, 0);
        assert!(approx(m11, 2.41421342, 1e-5), "M11 {m11}");
    }

    #[test]
    fn viewProjectionMatrixMatchesAllSixAssertedEntries() {
        // This is the strongest check in the file: it verifies the whole
        // convention mapping at once. A transposed composition or a reversed
        // product order fails here.
        let mut c = Camera::new();
        c.setViewport(0, 0, 100, 100);
        let vp = c.viewProjectionMatrix;
        for (label, r, col, expected) in [
            ("M11", 0, 0, -1.70710671f32),
            ("M12", 0, 1, -0.985598564),
            ("M13", 0, 2, -0.577364743),
            ("M14", 0, 3, -0.5773503),
            ("M43", 3, 2, 0.732069254),
            ("M44", 3, 3, 1.7320509),
        ] {
            let got = Camera::csElement(vp, r, col);
            assert!(
                approx(got, expected, 2e-5),
                "{label}: got {got}, C# asserts {expected}"
            );
        }
    }

    #[test]
    fn copyFromTransfersAspectRatio() {
        // The C# test asserts exactly this.
        let mut a = Camera::new();
        let mut b = Camera::new();
        b.aspectRatio = 0.5;
        a.copyFrom(&b);
        assert_eq!(a.aspectRatio, 0.5);
    }

    #[test]
    fn copyFromDoesNotTransferScale() {
        // Documents the C# omission rather than silently fixing it.
        let mut a = Camera::new();
        let mut b = Camera::new();
        b.setScale(4.0);
        a.copyFrom(&b);
        assert_eq!(a.scale, 1.0, "scale is not copied by the C#");
    }

    #[test]
    fn degenerateViewportGivesNanAsInTheCSharp() {
        // The C# divides by height with no check: an infinite aspect ratio.
        let mut c = Camera::new();
        c.setViewport(0, 0, 100, 0);
        assert!(c.aspectRatio.is_infinite());
    }

    #[test]
    fn pitchIsClampedJustShortOfVertical() {
        let mut c = Camera::new();
        c.location = Vec3::ZERO;
        c.lookAt(Vec3::new(0.0, 0.0, 10.0)); // straight up
        assert!(c.pitch < PI_OVER_2, "pitch {} must stay below pi/2", c.pitch);
        assert!(approx(c.pitch, PI_OVER_2 - 0.001, 1e-6));
    }

    #[test]
    fn aabbPointContainmentIsHalfOpen() {
        let b = Aabb::new(Vec3::ZERO, Vec3::splat(2.0));
        assert!(b.containsPoint(Vec3::ZERO), "min face is inside");
        assert!(!b.containsPoint(Vec3::splat(2.0)), "max face is outside");
        assert!(b.containsPoint(Vec3::splat(1.0)));
    }

    #[test]
    fn aabbUnionAndTranslate() {
        let a = Aabb::new(Vec3::ZERO, Vec3::ONE);
        let b = Aabb::new(Vec3::splat(5.0), Vec3::splat(6.0));
        let u = a.union(&b);
        assert_eq!((u.min, u.max), (Vec3::ZERO, Vec3::splat(6.0)));
        let t = a.translate(Vec3::splat(10.0));
        assert_eq!((t.min, t.max), (Vec3::splat(10.0), Vec3::splat(11.0)));
        assert_eq!(a.size(), Vec3::ONE);
        assert_eq!(a.center(), Vec3::splat(0.5));
    }

    #[test]
    fn aabbTransformReboundsAllCorners() {
        let b = Aabb::new(Vec3::ZERO, Vec3::ONE);
        let r = b.transform(Mat4::from_rotation_z(std::f32::consts::FRAC_PI_2));
        // Rotating the unit cube 90 degrees about Z maps x into -y.
        assert!(approx(r.min.y, 0.0, 1e-5), "min.y {}", r.min.y);
        assert!(approx(r.max.x, 0.0, 1e-5), "max.x {}", r.max.x);
        assert!(approx(r.max.z, 1.0, 1e-5));
    }

    #[test]
    fn aabbIntersectionIsHalfOpenToo() {
        let a = Aabb::new(Vec3::ZERO, Vec3::splat(2.0));
        let touching = Aabb::new(Vec3::splat(2.0), Vec3::splat(4.0));
        let overlapping = Aabb::new(Vec3::ONE, Vec3::splat(3.0));
        assert!(a.intersects(&overlapping));
        assert!(!a.intersects(&touching), "max face does not intersect");
    }
}
