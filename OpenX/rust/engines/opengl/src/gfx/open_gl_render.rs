// PORT-SOURCE: Engines/OpenX.Engine.OpenGL/Gfx/OpenGL_Render.cs
// PORT-SHA: d4101965a5beaf8b
// PORT-STATUS: partial
//
// Ported: `TestTriRenderer`, `TextureRenderer`, `ObjectRenderer`,
// `MaterialRenderer`, `GridRenderer`, `WorldRenderer`, `ParticleRenderer`
// (including `SpritesRenderer`, `TrailsRenderer`, `ChildRenderer` and
// `ParticleControllerFactory`, which C# puts in `Gl_Render.cs` — it lives here
// so the factory and the renderers it builds don't form a module cycle) and
// `EngineRenderer`.
//
// `SpritesRenderer` / `TrailsRenderer` ask for `vrf.particle.sprite` and
// `vrf.particle.trail`. Those shaders are not embedded (only `plane` and
// `testtri` are), so constructing one fails the same way a grid shader does.
// The explorer's Particle tab builds the outer `ParticleRenderer`, which has
// no `Start`/`Update`/`Render` in C# either; `ChildRenderer` is what Valve's
// `ParticleSceneNode` drives, and that scene node isn't ported.
// `EngineRenderer` needs an `Arc<dyn CellDatabase>` boxed inside the `Any`.
// `Binary_Engine` produces one when the query archive implements `IQueryFunc`.
// That implementor is Bethesda's `Binary_Esm`, still unported, so a real file
// still fails the cast. The cell coroutine isn't ported, so a started engine
// loads empty containers.
//
// `ObjectRenderer` passes the loaded object as `SourcePath::Obj`. Nothing is
// registered in `BuildersByType` until `Binary_Nif`'s builder is ported, so
// `Start` logs the missing builder and the view stays empty, as C# does.
//
// A renderer gets C#'s `IOpenGfx[]` as `IOpenGfxs` and downcasts the model
// slot, as C# casts `(OpenGLGfxModel)gfx[GfX.XModel]`. C#'s `.Result` on a
// manager task is `result`. `TextureRenderer` takes the texture as
// `Arc<dyn ITexture>` and caches it under its own address, as C# keys the
// cache on the object. Frame stepping and `Select` go through `ITexture`'s
// default methods (see `gfx.rs`): `Tag::Texture` can't see `ITextureFrames`.
// `Select(0)` runs here, after the upload. C# calls it from `OpenGLControl`
// on `Value`, and the control doesn't have that value; `Id` starts at 0 and
// the key handler that would change it is commented out.

use std::any::Any;
use std::collections::HashMap;
use std::future::Future;
use std::io::{Error, ErrorKind};
use std::ops::Range;
use std::sync::{Arc, Mutex};
use std::task::{Context, Poll, Wake, Waker};
use glam::{Mat4, Vec3};
use glow::HasContext;
use openx::manager::CellDatabase;
use openx_gfx::gfx::{GfX, GfxShader, IOpenGfxModel, IOpenGfxs, ITexture, MaterialProp};
use openx_gfx::gfx_render::{Pass, Renderer};
use openx_gfx_egin::egin_particle::{BasicMovement, ColorInterpolate, ContinuousEmitter, CreateWithinSphere, Decay, FadeAndKill, FadeInSimple, FadeOutSimple, IParticleRenderer, InitialVelocityNoise, InstantaneousEmitter, InterpolateRadius, OffsetVectorToVector, OscillateScalar, Particle, ParticleBag, ParticleEmitter, ParticleInitializer, ParticleOperator, ParticleSystemRenderState, PositionOffset, RandomAlpha, RandomColor, RandomLifeTime, RandomRadius, RandomRotation, RandomRotationSpeed, RandomSequence, RandomTrailLength, RemapParticleCountToScalar, RingWave, SpinUpdate};
use openx_gfx_egin::egin_render::{Aabb, Camera, EginRenderer, IParticleSystem};
use openx_poly::core::{ISourceExt, SourcePath, SourceRef};
use openx_poly::pool::KV;
use crate::egin::gl_render::{program, tex, uniform, GLRenderMaterial};
use crate::engine_open_gl::{OpenGLGfxApi, OpenGLGfxModel};
use crate::gfx::open_gl::{OpenGLCellBuilder, GL};
use crate::gfx::open_gl_open_engine::OpenGLOpenEngine;

/// C# `task.Result`.
fn result<F: Future>(f: F) -> F::Output {
    struct Unpark(std::thread::Thread);
    impl Wake for Unpark { fn wake(self: Arc<Self>) { self.0.unpark(); } }
    let waker = Waker::from(Arc::new(Unpark(std::thread::current())));
    let mut cx = Context::from_waker(&waker);
    let mut f = std::pin::pin!(f);
    loop {
        if let Poll::Ready(v) = f.as_mut().poll(&mut cx) { return v; }
        std::thread::park();
    }
}

/// C# `(OpenGLGfxModel)gfx[GfX.XModel]`.
fn gfxModel(gfx: &IOpenGfxs) -> Result<Arc<Mutex<OpenGLGfxModel>>, Error> {
    gfx.get(GfX::XMODEL as usize).cloned().flatten().and_then(|s| s.downcast::<Mutex<OpenGLGfxModel>>().ok()).ok_or_else(|| Error::new(ErrorKind::InvalidData, "InvalidCastException: gfx[GfX.XModel]"))
}

/// C# `GL.BufferData` + `GL.VertexAttribPointer` for the named attributes, in order.
unsafe fn setupAttributes(shader: &GfxShader, attributes: &[(&str, i32)]) {
    let gl = GL();
    gl.enable_vertex_attrib_array(0);
    let (mut offset, stride) = (0, 4 * attributes.iter().map(|x| x.1).sum::<i32>());
    for (name, size) in attributes {
        let location = shader.attribLocation(name);
        if location != u32::MAX { gl.enable_vertex_attrib_array(location); gl.vertex_attrib_pointer_f32(location, *size, glow::FLOAT, false, stride, offset); }
        offset += 4 * size;
    }
}

// #region TestTriRenderer

/// TestTriRenderer
pub struct TestTriRenderer {
    gfxModel: Arc<Mutex<OpenGLGfxModel>>,
    shader: Arc<Mutex<GfxShader>>,
    vao: glow::VertexArray,
}

impl TestTriRenderer {
    pub fn boundingBox(&self) -> Aabb { Aabb::new(glam::Vec3::splat(-1.), glam::Vec3::splat(1.)) }

    pub fn new(gfx: &IOpenGfxs, source: &SourceRef, obj: &dyn std::any::Any) -> Result<Self, Error> {
        let gfxModel = gfxModel(gfx)?;
        let shader = gfxModel.lock().unwrap().createShader(source, &"testtri".into(), None)?;
        let vao = Self::setupVao(&shader.lock().unwrap())?;
        Ok(Self { gfxModel, shader, vao })
    }

    fn setupVao(shader: &GfxShader) -> Result<glow::VertexArray, Error> {
        let gl = GL();
        unsafe {
            gl.use_program(program(shader.program));

            // create and bind vao
            let vao = gl.create_vertex_array().map_err(|e| Error::new(ErrorKind::Other, e))?; gl.bind_vertex_array(Some(vao));
            let vbo = gl.create_buffer().map_err(|e| Error::new(ErrorKind::Other, e))?; gl.bind_buffer(glow::ARRAY_BUFFER, Some(vbo));
            let vertices: [f32; 18] = [
               // xyz           :rgb
               -0.5, -0.5, 0.0,  1.0, 0.0, 0.0,
                0.5, -0.5, 0.0,  0.0, 1.0, 0.0,
                0.0,  0.5, 0.0,  0.0, 0.0, 1.0,
            ];
            gl.buffer_data_u8_slice(glow::ARRAY_BUFFER, bytemuck::cast_slice(&vertices), glow::STATIC_DRAW);

            // attributes
            gl.enable_vertex_attrib_array(0); gl.vertex_attrib_pointer_f32(0, 3, glow::FLOAT, false, 24, 0);
            gl.enable_vertex_attrib_array(1); gl.vertex_attrib_pointer_f32(1, 3, glow::FLOAT, false, 24, 12);
            gl.bind_vertex_array(None); // unbind vao
            Ok(vao)
        }
    }
}

impl Renderer for TestTriRenderer {}

impl EginRenderer for TestTriRenderer {
    fn render(&mut self, camera: &Camera, pass: Pass) {
        let gl = GL();
        unsafe {
            gl.use_program(program(self.shader.lock().unwrap().program));
            gl.bind_vertex_array(Some(self.vao));
            gl.enable_vertex_attrib_array(0);
            // C# draws 6 vertices from a 3-vertex buffer. Kept.
            gl.draw_arrays(glow::TRIANGLE_STRIP, 0, 6);
            gl.bind_vertex_array(None); // unbind vao
            gl.use_program(None); // unbind program
        }
    }
}

// #endregion

// #region TextureRenderer

/// TextureRenderer
pub struct TextureRenderer {
    gfxModel: Arc<Mutex<OpenGLGfxModel>>,
    source: SourceRef,
    obj: Arc<dyn ITexture>,
    level: Range<i32>,
    tex: i32,
    shader: Arc<Mutex<GfxShader>>,
    vao: glow::VertexArray,
    pub background: bool,
    frameDelay: f32,
}

impl TextureRenderer {
    const FACTOR: i32 = 1;

    pub fn boundingBox(&self) -> Aabb { Aabb::new(glam::Vec3::splat(-1.), glam::Vec3::splat(1.)) }

    /// The cache key C# makes of the object itself.
    fn path(obj: &Arc<dyn ITexture>) -> SourcePath { SourcePath::Int(Arc::as_ptr(obj) as *const () as u64) }

    pub fn new(gfx: &IOpenGfxs, source: &SourceRef, obj: Arc<dyn ITexture>, level: Range<i32>, background: bool) -> Result<Self, Error> {
        let gfxModel = gfxModel(gfx)?;
        let (tex, shader) = {
            let mut m = gfxModel.lock().unwrap();
            let path = Self::path(&obj);
            m.textureManager().delete(source, &path);
            let (tex, _) = m.textureManager().createFrom(source, &path, obj.clone(), Some(level.clone()))?;
            (tex, m.createShader(source, &"plane".into(), None)?)
        };
        let vao = Self::setupVao(&shader.lock().unwrap())?;
        // C# `OnSourceChanged`: `if (Value is ITextureSelect z2) z2.Select(Id)` with `Id` still 0.
        obj.select(0);
        Ok(Self { gfxModel, source: source.clone(), obj, level, tex, shader, vao, background, frameDelay: 0. })
    }

    fn setupVao(shader: &GfxShader) -> Result<glow::VertexArray, Error> {
        let gl = GL();
        unsafe {
            gl.use_program(program(shader.program));

            // create and bind vao
            let vao = gl.create_vertex_array().map_err(|e| Error::new(ErrorKind::Other, e))?; gl.bind_vertex_array(Some(vao));
            let vbo = gl.create_buffer().map_err(|e| Error::new(ErrorKind::Other, e))?; gl.bind_buffer(glow::ARRAY_BUFFER, Some(vbo));
            let vertices: [f32; 44] = [
                // position     :normal        :texcoord  :tangent
                -1., -1., 0.,   0., 0., 1.,    0., 1.,    1., 0., 0.,
                -1., 1., 0.,    0., 0., 1.,    0., 0.,    1., 0., 0.,
                1., -1., 0.,    0., 0., 1.,    1., 1.,    1., 0., 0.,
                1., 1., 0.,     0., 0., 1.,    1., 0.,    1., 0., 0.,
            ];
            gl.buffer_data_u8_slice(glow::ARRAY_BUFFER, bytemuck::cast_slice(&vertices), glow::STATIC_DRAW);

            // attributes
            setupAttributes(shader, &[("vPOSITION", 3), ("vNORMAL", 3), ("vTEXCOORD", 2), ("vTANGENT", 3)]);
            gl.bind_vertex_array(None); // unbind vao
            Ok(vao)
        }
    }
}

impl Renderer for TextureRenderer {
    fn update(&mut self, deltaTime: f32) {
        // C# `if (Obj is not ITextureFrames obj || GfxModel == null || !obj.HasFrames) return`.
        if !self.obj.hasFrames() { return; }
        self.frameDelay += deltaTime;
        if self.frameDelay <= self.obj.fps() as f32 || !self.obj.nextFrame() { return; }
        self.frameDelay = 0.; // reset delay between frames
        let path = Self::path(&self.obj);
        // C# `Reload` reuses the GL name and ignores the builder's return. The
        // Rust manager stores whatever the builder hands back, so keep that name.
        match self.gfxModel.lock().unwrap().textureManager().reload(&self.source, &path, Some(self.level.clone())) {
            Some(Ok((tex, _))) => self.tex = tex,
            Some(Err(e)) => eprintln!("ERROR: {e}"),
            None => {}
        }
    }
}

impl EginRenderer for TextureRenderer {
    fn getViewport(&self, size: (i32, i32)) -> Option<(i32, i32)> {
        let o = &self.obj;
        Some(if o.width() > 1024 || o.height() > 1024 || false { size } else { (o.width() << Self::FACTOR, o.height() << Self::FACTOR) })
    }

    fn render(&mut self, camera: &Camera, pass: Pass) {
        let gl = GL();
        unsafe {
            if self.background { gl.clear_color(1., 1., 1., 1.); gl.clear(glow::COLOR_BUFFER_BIT); }
            gl.use_program(program(self.shader.lock().unwrap().program));
            gl.bind_vertex_array(Some(self.vao));
            gl.enable_vertex_attrib_array(0);
            if self.tex > -1 { gl.active_texture(glow::TEXTURE0); gl.bind_texture(glow::TEXTURE_2D, tex(self.tex)); }
            gl.draw_arrays(glow::TRIANGLE_STRIP, 0, 4);
            gl.bind_vertex_array(None); // unbind vao
            gl.use_program(None); // unbind program
        }
    }
}

// #endregion

// #region ObjectRenderer

/// ObjectRenderer
pub struct ObjectRenderer {
    gfxModel: Arc<Mutex<OpenGLGfxModel>>,
    source: SourceRef,
    obj: Arc<dyn Any + Send + Sync>,
}

impl ObjectRenderer {
    pub fn new(gfx: &IOpenGfxs, source: &SourceRef, obj: Arc<dyn Any + Send + Sync>) -> Result<Self, Error> {
        Ok(Self { gfxModel: gfxModel(gfx)?, source: source.clone(), obj })
    }
}

impl Renderer for ObjectRenderer {
    fn start(&mut self) {
        let path = SourcePath::Obj(self.obj.clone());
        // C# `ObjectManager.Create` catches and logs. The Rust manager returns the error.
        if let Err(e) = result(self.gfxModel.lock().unwrap().objectManager().create(&self.source, &path, true, None)) {
            eprintln!("ERROR: {e}");
        }
    }
}

impl EginRenderer for ObjectRenderer {}

// #endregion

// #region MaterialRenderer

/// MaterialRenderer
pub struct MaterialRenderer {
    gfxModel: Arc<Mutex<OpenGLGfxModel>>,
    material: Arc<GLRenderMaterial>,
    shader: Arc<Mutex<GfxShader>>,
    vao: glow::VertexArray,
}

impl MaterialRenderer {
    pub fn boundingBox(&self) -> Aabb { Aabb::new(glam::Vec3::splat(-1.), glam::Vec3::splat(1.)) }

    /// `path` is the cache key C# makes of `obj`.
    pub fn new(gfx: &IOpenGfxs, source: &SourceRef, path: &SourcePath, obj: MaterialProp) -> Result<Self, Error> {
        let gfxModel = gfxModel(gfx)?;
        let (material, shader) = {
            let mut m = gfxModel.lock().unwrap();
            m.textureManager().delete(source, path);
            let (material, _) = result(m.materialManager().createFrom(source, path, obj))?;
            let (name, args) = (material.base.shader().shaderName.clone(), material.base.shader().shaderArgs.clone());
            (material, m.createShader(source, &name.into(), Some(&args))?)
        };
        let vao = Self::setupVao(&shader.lock().unwrap())?;
        Ok(Self { gfxModel, material, shader, vao })
    }

    fn setupVao(shader: &GfxShader) -> Result<glow::VertexArray, Error> {
        let gl = GL();
        unsafe {
            gl.use_program(program(shader.program));

            // create and bind vao
            let vao = gl.create_vertex_array().map_err(|e| Error::new(ErrorKind::Other, e))?; gl.bind_vertex_array(Some(vao));
            let vbo = gl.create_buffer().map_err(|e| Error::new(ErrorKind::Other, e))?; gl.bind_buffer(glow::ARRAY_BUFFER, Some(vbo));
            let vertices: [f32; 84] = [
                // position    normal            texcordr  tangent           blendindices      blendweight
                -1., -1., 0.,  0., 0., 0., 1.,   0., 1.,   1., 0., 0., 0.,   0., 0., 0., 0.,   0., 0., 0., 0.,
                -1., 1., 0.,   0., 0., 0., 1.,   0., 0.,   1., 0., 0., 0.,   0., 0., 0., 0.,   0., 0., 0., 0.,
                1., -1., 0.,   0., 0., 0., 1.,   1., 1.,   1., 0., 0., 0.,   0., 0., 0., 0.,   0., 0., 0., 0.,
                1., 1., 0.,    0., 0., 0., 1.,   1., 0.,   1., 0., 0., 0.,   0., 0., 0., 0.,   0., 0., 0., 0.,
            ];
            gl.buffer_data_u8_slice(glow::ARRAY_BUFFER, bytemuck::cast_slice(&vertices), glow::STATIC_DRAW);

            // attributes
            setupAttributes(shader, &[("vPOSITION", 3), ("vNORMAL", 4), ("vTEXCOORD", 2), ("vTANGENT", 4), ("vBLENDINDICES", 4), ("vBLENDWEIGHT", 4)]);
            gl.bind_vertex_array(None); // unbind vao
            Ok(vao)
        }
    }
}

impl Renderer for MaterialRenderer {}

impl EginRenderer for MaterialRenderer {
    fn render(&mut self, camera: &Camera, pass: Pass) {
        let gl = GL();
        let identity = Mat4::IDENTITY.to_cols_array();
        let mut shader = self.shader.lock().unwrap();
        unsafe {
            gl.use_program(program(shader.program));
            gl.bind_vertex_array(Some(self.vao));
            gl.enable_vertex_attrib_array(0);
            let mut location = shader.uniformLocation("m_vTintColorSceneObject");
            if location > -1 { gl.uniform_4_f32(uniform(location).as_ref(), 1., 1., 1., 1.); }
            location = shader.uniformLocation("m_vTintColorDrawCall");
            if location > -1 { gl.uniform_3_f32(uniform(location).as_ref(), 1., 1., 1.); }
            location = shader.uniformLocation("uProjectionViewMatrix");
            if location > -1 { gl.uniform_matrix_4_f32_slice(uniform(location).as_ref(), false, &identity); }
            location = shader.uniformLocation("transform");
            if location > -1 { gl.uniform_matrix_4_f32_slice(uniform(location).as_ref(), false, &identity); }
            self.material.render(&mut shader);
            gl.draw_arrays(glow::TRIANGLE_STRIP, 0, 4);
            self.material.postRender();
            gl.bind_vertex_array(None); // unbind vao
            gl.use_program(None); // unbind program
        }
    }
}

// #endregion

// #region GridRenderer

/// GridRenderer
pub struct GridRenderer {
    gfxModel: Arc<Mutex<OpenGLGfxModel>>,
    shader: Arc<Mutex<GfxShader>>,
    vao: glow::VertexArray,
    vertexCount: i32,
    pub boundingBox: Aabb,
}

impl GridRenderer {
    pub fn new(gfx: &IOpenGfxs, source: &SourceRef, cellWidth: f32, gridWidthInCells: i32) -> Result<Self, Error> {
        let gfxModel = gfxModel(gfx)?;
        let half = cellWidth * 0.5 * gridWidthInCells as f32;
        let boundingBox = Aabb::new(glam::Vec3::new(-half, -half, 0.), glam::Vec3::new(half, half, 0.));
        let shader = gfxModel.lock().unwrap().createShader(source, &"vrf.grid".into(), None)?;
        let (vao, vertexCount) = Self::setupVao(&shader.lock().unwrap(), cellWidth, gridWidthInCells)?;
        Ok(Self { gfxModel, shader, vao, vertexCount, boundingBox })
    }

    fn generateGridVertexBuffer(cellWidth: f32, gridWidthInCells: i32) -> Vec<f32> {
        let mut vertices = Vec::new();
        let width = cellWidth * gridWidthInCells as f32;
        let color = [1.0f32, 1.0, 1.0, 1.0];
        for i in 0..=gridWidthInCells {
            vertices.extend([width, i as f32 * cellWidth, 0.]);
            vertices.extend(color);
            vertices.extend([-width, i as f32 * cellWidth, 0.]);
            vertices.extend(color);
        }
        for i in 1..=gridWidthInCells {
            vertices.extend([width, -i as f32 * cellWidth, 0.]);
            vertices.extend(color);
            vertices.extend([-width, -i as f32 * cellWidth, 0.]);
            vertices.extend(color);
        }
        for i in 0..=gridWidthInCells {
            vertices.extend([i as f32 * cellWidth, width, 0.]);
            vertices.extend(color);
            vertices.extend([i as f32 * cellWidth, -width, 0.]);
            vertices.extend(color);
        }
        for i in 1..=gridWidthInCells {
            vertices.extend([-i as f32 * cellWidth, width, 0.]);
            vertices.extend(color);
            vertices.extend([-i as f32 * cellWidth, -width, 0.]);
            vertices.extend(color);
        }
        vertices
    }

    fn setupVao(shader: &GfxShader, cellWidth: f32, gridWidthInCells: i32) -> Result<(glow::VertexArray, i32), Error> {
        const STRIDE: i32 = 4 * 7;
        let gl = GL();
        unsafe {
            gl.use_program(program(shader.program));

            // create and bind vao
            let vao = gl.create_vertex_array().map_err(|e| Error::new(ErrorKind::Other, e))?; gl.bind_vertex_array(Some(vao));
            let vbo = gl.create_buffer().map_err(|e| Error::new(ErrorKind::Other, e))?; gl.bind_buffer(glow::ARRAY_BUFFER, Some(vbo));
            let vertices = Self::generateGridVertexBuffer(cellWidth, gridWidthInCells);
            // C# divides by 3 though each vertex is 7 floats. Kept.
            let vertexCount = vertices.len() as i32 / 3; // number of vertices in our buffer
            gl.buffer_data_u8_slice(glow::ARRAY_BUFFER, bytemuck::cast_slice(&vertices), glow::STATIC_DRAW);

            // attributes
            gl.enable_vertex_attrib_array(0);
            let mut location = shader.attribLocation("aVertexPosition");
            if location != u32::MAX { gl.enable_vertex_attrib_array(location); gl.vertex_attrib_pointer_f32(location, 3, glow::FLOAT, false, STRIDE, 0); }
            location = shader.attribLocation("aVertexColor");
            if location != u32::MAX { gl.enable_vertex_attrib_array(location); gl.vertex_attrib_pointer_f32(location, 4, glow::FLOAT, false, STRIDE, 4 * 3); }
            gl.bind_vertex_array(None); // unbind vao
            gl.use_program(None); // unbind program
            Ok((vao, vertexCount))
        }
    }
}

impl Renderer for GridRenderer {}

impl EginRenderer for GridRenderer {
    fn render(&mut self, camera: &Camera, pass: Pass) {
        let gl = GL();
        let matrix = camera.viewProjectionMatrix.to_cols_array();
        let mut shader = self.shader.lock().unwrap();
        unsafe {
            gl.enable(glow::BLEND);
            gl.blend_func(glow::SRC_ALPHA, glow::ONE_MINUS_SRC_ALPHA);
            gl.use_program(program(shader.program));
            gl.uniform_matrix_4_f32_slice(uniform(shader.uniformLocation("uProjectionViewMatrix")).as_ref(), false, &matrix);
            gl.bind_vertex_array(Some(self.vao));
            gl.enable_vertex_attrib_array(0);
            gl.draw_arrays(glow::LINES, 0, self.vertexCount);
            gl.bind_vertex_array(None); // unbind vao
            gl.use_program(None); // unbind program
            gl.disable(glow::BLEND);
        }
    }
}

// #endregion

// #region ParticleRenderer

const VERTEX_SIZE: usize = 9;

fn kvClass(kv: &KV) -> Result<&str, Error> {
    kv.get("_class").and_then(KV::asStr).ok_or_else(|| Error::new(ErrorKind::InvalidData, "KeyNotFoundException: _class"))
}

fn kvF32(kv: &KV, key: &str, default: f32) -> f32 { kv.get(key).and_then(KV::asF32).unwrap_or(default) }

fn kvBool(kv: &KV, key: &str) -> bool { kv.get(key).and_then(KV::asBool).unwrap_or(false) }

fn kvI64(kv: &KV, key: &str) -> i64 { kv.get(key).and_then(KV::asI64).unwrap_or(0) }

/// `m_hTexture`, or the first `m_vecTexturesInput[].m_hTexture`.
fn textureName(kv: &KV) -> Option<String> {
    if let Some(s) = kv.get("m_hTexture").and_then(KV::asStr) { return Some(s.to_string()); }
    let first = kv.get("m_vecTexturesInput").and_then(KV::asArray).and_then(|a| a.first())?;
    first.get("m_hTexture").and_then(KV::asStr).map(str::to_string)
}

/// C# `TextureManager.Create`. The tag is an `ITexture`, and sequences are
/// read only when that tag `is IDictionary`, so the sequence branch is dead.
fn loadParticleTexture(gfxModel: &Arc<Mutex<OpenGLGfxModel>>, source: &SourceRef, kv: &KV) -> Result<i32, Error> {
    let Some(name) = textureName(kv) else { return Ok(gfxModel.lock().unwrap().textureManager().default()); };
    let (tex, _) = result(gfxModel.lock().unwrap().createTexture(source, &SourcePath::Str(name), None))?;
    Ok(tex)
}

fn attrib(shader: &GfxShader, name: &str, size: i32, stride: i32, offset: i32) {
    let location = shader.attribLocation(name);
    // C# passes `-1` through. glow's missing location is `u32::MAX`; skip it.
    if location == u32::MAX { return; }
    let gl = GL();
    unsafe { gl.enable_vertex_attrib_array(location); gl.vertex_attrib_pointer_f32(location, size, glow::FLOAT, false, stride, offset); }
}

/// C# `ParticleRenderer.SpritesRenderer`.
pub struct SpritesRenderer {
    shader: Arc<Mutex<GfxShader>>,
    vao: glow::VertexArray,
    texture: i32,
    animationRate: f32,
    additive: bool,
    overbrightFactor: f32,
    orientationType: i64,
    rawVertices: Vec<f32>,
    quadIndices: glow::Buffer,
    vertexBuffer: glow::Buffer,
}

impl SpritesRenderer {
    pub fn new(source: &SourceRef, keyValues: &KV, gfxModel: &Arc<Mutex<OpenGLGfxModel>>) -> Result<Self, Error> {
        let shader = gfxModel.lock().unwrap().createShader(source, &"vrf.particle.sprite".into(), None)?;
        let quadIndices = { let mut m = gfxModel.lock().unwrap(); m.quadIndices()?.handle };
        let (vao, vertexBuffer) = Self::makeVao(&shader.lock().unwrap())?;
        let texture = loadParticleTexture(gfxModel, source, keyValues)?;
        Ok(Self {
            shader, vao, texture,
            animationRate: kvF32(keyValues, "m_flAnimationRate", 0.1),
            additive: kvBool(keyValues, "m_bAdditive"),
            overbrightFactor: kvF32(keyValues, "m_flOverbrightFactor", 1.),
            orientationType: kvI64(keyValues, "m_nOrientationType"),
            rawVertices: Vec::new(),
            quadIndices, vertexBuffer,
        })
    }

    fn makeVao(shader: &GfxShader) -> Result<(glow::VertexArray, glow::Buffer), Error> {
        let gl = GL();
        unsafe {
            gl.use_program(program(shader.program));
            let vao = gl.create_vertex_array().map_err(|e| Error::new(ErrorKind::Other, e))?; gl.bind_vertex_array(Some(vao));
            let vbo = gl.create_buffer().map_err(|e| Error::new(ErrorKind::Other, e))?; gl.bind_buffer(glow::ARRAY_BUFFER, Some(vbo));
            let stride = (4 * VERTEX_SIZE) as i32;
            attrib(shader, "aVertexPosition", 3, stride, 0);
            attrib(shader, "aVertexColor", 4, stride, 4 * 3);
            attrib(shader, "aTexCoords", 2, stride, 4 * 7);
            gl.bind_vertex_array(None);
            Ok((vao, vbo))
        }
    }

    /// C# grows to the next 64-vertex block past the first exact allocation.
    fn ensureSpace(&mut self, count: usize) {
        let numFloats = count * VERTEX_SIZE;
        if self.rawVertices.is_empty() { self.rawVertices.resize(numFloats, 0.); }
        else if self.rawVertices.len() < numFloats {
            self.rawVertices.resize((count / 64 + 1) * 64 * VERTEX_SIZE, 0.);
        }
    }

    fn updateVertices(&mut self, bag: &ParticleBag, modelView: Mat4) {
        let (_scale, rotation, _translation) = modelView.to_scale_rotation_translation();
        // C# `rotation * billboard * transformation` is row-vector order.
        let billboard = Mat4::from_quat(rotation.inverse());
        self.ensureSpace(bag.len() * 4);
        for (i, p) in bag.asSlice().iter().enumerate() {
            let model = if self.orientationType == 0 { p.transformationMatrix() * billboard * p.rotationMatrix() }
            else { p.transformationMatrix() * p.rotationMatrix() };
            let corners = [
                model.transform_point3(Vec3::new(-1., -1., 0.)),
                model.transform_point3(Vec3::new(-1., 1., 0.)),
                model.transform_point3(Vec3::new(1., 1., 0.)),
                model.transform_point3(Vec3::new(1., -1., 0.)),
            ];
            let quad = i * VERTEX_SIZE * 4;
            for (j, c) in corners.iter().enumerate() {
                let o = quad + VERTEX_SIZE * j;
                self.rawVertices[o] = c.x; self.rawVertices[o + 1] = c.y; self.rawVertices[o + 2] = c.z;
                self.rawVertices[o + 3] = p.color.x; self.rawVertices[o + 4] = p.color.y; self.rawVertices[o + 5] = p.color.z; self.rawVertices[o + 6] = p.alpha;
            }
            // Full 0–1 quad. `TextureSequences` is never filled on this path.
            let uvs = [(0., 1.), (0., 0.), (1., 0.), (1., 1.)];
            for (j, (u, v)) in uvs.iter().enumerate() {
                let o = quad + VERTEX_SIZE * j;
                self.rawVertices[o + 7] = *u; self.rawVertices[o + 8] = *v;
            }
        }
        let used = bag.len() * VERTEX_SIZE * 4;
        let gl = GL();
        unsafe { gl.bind_buffer(glow::ARRAY_BUFFER, Some(self.vertexBuffer)); gl.buffer_data_u8_slice(glow::ARRAY_BUFFER, bytemuck::cast_slice(&self.rawVertices[..used]), glow::DYNAMIC_DRAW); }
    }
}

impl IParticleRenderer for SpritesRenderer {
    fn render(&mut self, bag: &ParticleBag, viewProjection: Mat4, modelView: Mat4) {
        if bag.len() == 0 { return; }
        self.updateVertices(bag, modelView);
        let gl = GL();
        let mut shader = self.shader.lock().unwrap();
        let matrix = viewProjection.to_cols_array();
        unsafe {
            gl.enable(glow::BLEND);
            gl.use_program(program(shader.program));
            if self.additive { gl.blend_func(glow::SRC_ALPHA, glow::ONE); }
            else { gl.blend_func(glow::SRC_ALPHA, glow::ONE_MINUS_SRC_ALPHA); }
            gl.bind_vertex_array(Some(self.vao));
            gl.enable_vertex_attrib_array(0);
            gl.active_texture(glow::TEXTURE0);
            gl.bind_texture(glow::TEXTURE_2D, tex(self.texture));
            gl.bind_buffer(glow::ARRAY_BUFFER, Some(self.vertexBuffer));
            gl.uniform_1_u32(uniform(shader.uniformLocation("uTexture")).as_ref(), 0);
            gl.uniform_matrix_4_f32_slice(uniform(shader.uniformLocation("uProjectionViewMatrix")).as_ref(), false, &matrix);
            gl.uniform_1_f32(uniform(shader.uniformLocation("uOverbrightFactor")).as_ref(), self.overbrightFactor);
            gl.disable(glow::CULL_FACE);
            gl.enable(glow::DEPTH_TEST);
            gl.depth_mask(false);
            gl.bind_buffer(glow::ELEMENT_ARRAY_BUFFER, Some(self.quadIndices));
            gl.draw_elements(glow::TRIANGLES, (bag.len() * 6) as i32, glow::UNSIGNED_SHORT, 0);
            gl.bind_buffer(glow::ELEMENT_ARRAY_BUFFER, None);
            gl.bind_buffer(glow::ARRAY_BUFFER, None);
            gl.enable(glow::CULL_FACE);
            gl.disable(glow::DEPTH_TEST);
            gl.depth_mask(true);
            gl.bind_vertex_array(None);
            gl.use_program(None);
            // C# sets the blend equation only on the additive path, after the draw.
            if self.additive { gl.blend_equation(glow::FUNC_ADD); }
            gl.disable(glow::BLEND);
        }
    }

    fn setRenderMode(&mut self, renderMode: Option<&str>) {
        // C# builds `renderMode_{name}` and then does not reload the shader.
        if let Some(mode) = renderMode {
            if self.shader.lock().unwrap().renderModes.iter().any(|m| m == mode) { let _ = format!("renderMode_{mode}"); }
        }
    }

    fn supportedRenderModes(&self) -> Vec<String> { self.shader.lock().unwrap().renderModes.clone() }
}

/// C# `ParticleRenderer.TrailsRenderer`.
pub struct TrailsRenderer {
    shader: Arc<Mutex<GfxShader>>,
    quadVao: glow::VertexArray,
    texture: i32,
    animationRate: f32,
    additive: bool,
    overbrightFactor: f32,
    orientationType: i64,
    finalTextureScaleU: f32,
    finalTextureScaleV: f32,
    maxLength: f32,
    lengthFadeInTime: f32,
}

impl TrailsRenderer {
    pub fn new(source: &SourceRef, keyValues: &KV, gfxModel: &Arc<Mutex<OpenGLGfxModel>>) -> Result<Self, Error> {
        let shader = gfxModel.lock().unwrap().createShader(source, &"vrf.particle.trail".into(), Some(&HashMap::new()))?;
        let quadVao = Self::setupQuadBuffer(&shader.lock().unwrap())?;
        let texture = loadParticleTexture(gfxModel, source, keyValues)?;
        Ok(Self {
            shader, quadVao, texture,
            animationRate: kvF32(keyValues, "m_flAnimationRate", 0.1),
            additive: kvBool(keyValues, "m_bAdditive"),
            overbrightFactor: kvF32(keyValues, "m_flOverbrightFactor", 1.),
            orientationType: kvI64(keyValues, "m_nOrientationType"),
            finalTextureScaleU: kvF32(keyValues, "m_flFinalTextureScaleU", 1.),
            finalTextureScaleV: kvF32(keyValues, "m_flFinalTextureScaleV", 1.),
            maxLength: kvF32(keyValues, "m_flMaxLength", 2000.),
            lengthFadeInTime: kvF32(keyValues, "m_flLengthFadeInTime", 0.),
        })
    }

    fn setupQuadBuffer(shader: &GfxShader) -> Result<glow::VertexArray, Error> {
        let gl = GL();
        unsafe {
            gl.use_program(program(shader.program));
            let vao = gl.create_vertex_array().map_err(|e| Error::new(ErrorKind::Other, e))?; gl.bind_vertex_array(Some(vao));
            let vbo = gl.create_buffer().map_err(|e| Error::new(ErrorKind::Other, e))?; gl.bind_buffer(glow::ARRAY_BUFFER, Some(vbo));
            let vertices: [f32; 12] = [-1., -1., 0., -1., 1., 0., 1., -1., 0., 1., 1., 0.];
            gl.buffer_data_u8_slice(glow::ARRAY_BUFFER, bytemuck::cast_slice(&vertices), glow::STATIC_DRAW);
            gl.enable_vertex_attrib_array(0);
            attrib(shader, "aVertexPosition", 3, 0, 0);
            gl.bind_vertex_array(None);
            Ok(vao)
        }
    }
}

impl IParticleRenderer for TrailsRenderer {
    fn render(&mut self, bag: &ParticleBag, viewProjection: Mat4, _modelView: Mat4) {
        let gl = GL();
        let mut shader = self.shader.lock().unwrap();
        let matrix = viewProjection.to_cols_array();
        let modelLocation = shader.uniformLocation("uModelMatrix");
        let colorLocation = shader.uniformLocation("uColor");
        let alphaLocation = shader.uniformLocation("uAlpha");
        let uvOffsetLocation = shader.uniformLocation("uUvOffset");
        let uvScaleLocation = shader.uniformLocation("uUvScale");
        unsafe {
            gl.enable(glow::BLEND);
            gl.use_program(program(shader.program));
            if self.additive { gl.blend_func(glow::SRC_ALPHA, glow::ONE); }
            else { gl.blend_func(glow::SRC_ALPHA, glow::ONE_MINUS_SRC_ALPHA); }
            gl.bind_vertex_array(Some(self.quadVao));
            gl.enable_vertex_attrib_array(0);
            gl.active_texture(glow::TEXTURE0);
            gl.bind_texture(glow::TEXTURE_2D, tex(self.texture));
            gl.uniform_1_u32(uniform(shader.uniformLocation("uTexture")).as_ref(), 0);
            gl.uniform_matrix_4_f32_slice(uniform(shader.uniformLocation("uProjectionViewMatrix")).as_ref(), false, &matrix);
            gl.uniform_1_f32(uniform(shader.uniformLocation("uOverbrightFactor")).as_ref(), self.overbrightFactor);
            for p in bag.asSlice() {
                let difference = p.positionPrevious - p.position;
                let direction = difference.normalize();
                // C# builds `midPoint` and never reads it.
                let length = self.maxLength.min(p.trailLength * difference.length() / 2.);
                let t = 1. - p.lifetime / p.constantLifetime;
                let animatedLength = if t >= self.lengthFadeInTime { length } else { t * length / self.lengthFadeInTime };
                let scale = Mat4::from_scale(Vec3::new(p.radius, animatedLength, 1.));
                let translation = Mat4::from_translation(Vec3::Y * animatedLength);
                let axis = Vec3::Y.cross(direction).normalize();
                let rotation = Mat4::from_axis_angle(axis, direction.y.acos());
                // C# `scale * translation * rotation` is row-vector order.
                let _modelMatrix = if self.orientationType == 0 { rotation * translation * scale } else { p.transformationMatrix() };
                // C# `UniformMatrix4f(modelMatrixLocation, 0, false, ...)` passes a count of 0, so the matrix is not uploaded. Kept.
                let _ = (modelLocation, _modelMatrix);
                // Sequence UVs need the texture tag to be an `IDictionary`. It is an `ITexture`.
                gl.uniform_2_f32(uniform(uvOffsetLocation).as_ref(), 1., 1.);
                gl.uniform_2_f32(uniform(uvScaleLocation).as_ref(), self.finalTextureScaleU, self.finalTextureScaleV);
                gl.uniform_3_f32(uniform(colorLocation).as_ref(), p.color.x, p.color.y, p.color.z);
                gl.uniform_1_f32(uniform(alphaLocation).as_ref(), p.alpha * p.alphaAlternate);
                gl.draw_arrays(glow::TRIANGLE_STRIP, 0, 4);
            }
            gl.bind_vertex_array(None);
            gl.use_program(None);
            if self.additive { gl.blend_equation(glow::FUNC_ADD); }
            gl.disable(glow::BLEND);
        }
    }

    fn setRenderMode(&mut self, renderMode: Option<&str>) {
        if let Some(mode) = renderMode {
            if self.shader.lock().unwrap().renderModes.iter().any(|m| m == mode) { let _ = format!("renderMode_{mode}"); }
        }
    }

    fn supportedRenderModes(&self) -> Vec<String> { self.shader.lock().unwrap().renderModes.clone() }
}

/// C# `ParticleControllerFactory`. It lives next to the renderers it builds.
pub struct ParticleControllerFactory;

impl ParticleControllerFactory {
    pub fn tryCreateEmitter(name: &str, _base: &KV, info: &KV) -> Option<Box<dyn ParticleEmitter>> {
        match name {
            "C_OP_InstantaneousEmitter" => Some(Box::new(InstantaneousEmitter::fromKv(info))),
            "C_OP_ContinuousEmitter" => Some(Box::new(ContinuousEmitter::fromKv(info))),
            _ => None,
        }
    }

    pub fn tryCreateInitializer(name: &str, info: &KV) -> Option<Box<dyn ParticleInitializer>> {
        match name {
            "C_INIT_CreateWithinSphere" => Some(Box::new(CreateWithinSphere::fromKv(info))),
            "C_INIT_InitialVelocityNoise" => Some(Box::new(InitialVelocityNoise::fromKv(info))),
            "C_INIT_OffsetVectorToVector" => Some(Box::new(OffsetVectorToVector::fromKv(info))),
            "C_INIT_PositionOffset" => Some(Box::new(PositionOffset::fromKv(info))),
            "C_INIT_RandomAlpha" => Some(Box::new(RandomAlpha::fromKv(info))),
            "C_INIT_RandomColor" => Some(Box::new(RandomColor::fromKv(info))),
            "C_INIT_RandomLifeTime" => Some(Box::new(RandomLifeTime::fromKv(info))),
            "C_INIT_RandomRadius" => Some(Box::new(RandomRadius::fromKv(info))),
            "C_INIT_RandomRotation" => Some(Box::new(RandomRotation::fromKv(info))),
            "C_INIT_RandomRotationSpeed" => Some(Box::new(RandomRotationSpeed::fromKv(info))),
            "C_INIT_RandomSequence" => Some(Box::new(RandomSequence::fromKv(info))),
            "C_INIT_RandomTrailLength" => Some(Box::new(RandomTrailLength::fromKv(info))),
            "C_INIT_RemapParticleCountToScalar" => Some(Box::new(RemapParticleCountToScalar::fromKv(info))),
            "C_INIT_RingWave" => Some(Box::new(RingWave::fromKv(info))),
            _ => None,
        }
    }

    pub fn tryCreateOperator(name: &str, info: &KV) -> Option<Box<dyn ParticleOperator>> {
        match name {
            "C_OP_Decay" => Some(Box::new(Decay)),
            "C_OP_BasicMovement" => Some(Box::new(BasicMovement::fromKv(info))),
            "C_OP_ColorInterpolate" => Some(Box::new(ColorInterpolate::fromKv(info))),
            "C_OP_InterpolateRadius" => Some(Box::new(InterpolateRadius::fromKv(info))),
            "C_OP_FadeAndKill" => Some(Box::new(FadeAndKill::fromKv(info))),
            "C_OP_FadeInSimple" => Some(Box::new(FadeInSimple::fromKv(info))),
            "C_OP_FadeOutSimple" => Some(Box::new(FadeOutSimple::fromKv(info))),
            "C_OP_OscillateScalar" => Some(Box::new(OscillateScalar::fromKv(info))),
            "C_OP_SpinUpdate" => Some(Box::new(SpinUpdate)),
            _ => None,
        }
    }

    /// A shader that fails to load throws out of C# `TryCreateRender`. Same here.
    pub fn tryCreateRender(name: &str, source: &SourceRef, info: &KV, gfxModel: &Arc<Mutex<OpenGLGfxModel>>) -> Result<Option<Box<dyn IParticleRenderer>>, Error> {
        match name {
            "C_OP_RenderSprites" => Ok(Some(Box::new(SpritesRenderer::new(source, info, gfxModel)?))),
            "C_OP_RenderTrails" => Ok(Some(Box::new(TrailsRenderer::new(source, info, gfxModel)?))),
            _ => Ok(None),
        }
    }
}

fn runOperators(operators: &mut [Box<dyn ParticleOperator>], bag: &mut ParticleBag, frameTime: f32, state: &ParticleSystemRenderState) {
    for op in operators { op.update(bag.liveParticles(), frameTime, state); }
}

fn drawParticles(renderers: &mut [Box<dyn IParticleRenderer>], bag: &ParticleBag, camera: &Camera) {
    for r in renderers { r.render(bag, camera.viewProjectionMatrix, camera.cameraViewMatrix); }
}

/// C# `ParticleRenderer.ChildRenderer`. Valve's scene node builds this; the
/// explorer's Particle tab does not.
pub struct ChildRenderer {
    source: SourceRef,
    gfxModel: Arc<Mutex<OpenGLGfxModel>>,
    emitters: Vec<Box<dyn ParticleEmitter>>,
    initializers: Vec<Box<dyn ParticleInitializer>>,
    operators: Vec<Box<dyn ParticleOperator>>,
    renderers: Vec<Box<dyn IParticleRenderer>>,
    childRenderers: Vec<ChildRenderer>,
    hasStarted: bool,
    particleBag: ParticleBag,
    particlesEmitted: i32,
    systemRenderState: ParticleSystemRenderState,
    pub boundingBox: Aabb,
}

impl ChildRenderer {
    pub fn new(gfxModel: Arc<Mutex<OpenGLGfxModel>>, source: SourceRef, system: &dyn IParticleSystem, pos: Vec3) -> Result<Self, Error> {
        let mut systemRenderState = ParticleSystemRenderState::default();
        systemRenderState.setControlPoint(0, pos);
        let mut me = Self {
            source, gfxModel,
            emitters: Vec::new(), initializers: Vec::new(), operators: Vec::new(), renderers: Vec::new(), childRenderers: Vec::new(),
            hasStarted: false, particleBag: ParticleBag::new(100, true), particlesEmitted: 0, systemRenderState,
            boundingBox: Aabb::new(pos + Vec3::splat(-32.), pos + Vec3::splat(32.)),
        };
        me.setupEmitters(system.data(), system.emitters())?;
        me.setupInitializers(system.initializers())?;
        me.setupOperators(system.operators())?;
        me.setupRenderers(system.renderers())?;
        me.setupChildParticles(&system.childParticleNames(true))?;
        Ok(me)
    }

    pub fn position(&self) -> Vec3 { self.systemRenderState.getControlPoint(0) }

    pub fn setPosition(&mut self, value: Vec3) {
        self.systemRenderState.setControlPoint(0, value);
        for child in &mut self.childRenderers { child.setPosition(value); }
    }

    pub fn start(&mut self) {
        for emitter in &mut self.emitters { emitter.start(); }
        for child in &mut self.childRenderers { child.start(); }
    }

    fn emitParticle(&mut self) {
        let mut p = Particle::default();
        p.particleCount = self.particlesEmitted;
        p.position = self.systemRenderState.getControlPoint(0);
        for initializer in &mut self.initializers { initializer.initialize(&mut p, &self.systemRenderState); }
        if self.particleBag.add(p).is_none() { eprintln!("Out of space in particle bag"); return; }
        self.particlesEmitted += 1;
    }

    pub fn stop(&mut self) {
        for emitter in &mut self.emitters { emitter.stop(); }
        for child in &mut self.childRenderers { child.stop(); }
    }

    pub fn restart(&mut self) {
        self.stop();
        self.systemRenderState.lifetime = 0.;
        self.particleBag.clear();
        self.start();
        for child in &mut self.childRenderers { child.restart(); }
    }

    pub fn update(&mut self, deltaTime: f32) {
        if !self.hasStarted { self.start(); self.hasStarted = true; }
        self.systemRenderState.lifetime += deltaTime;
        for i in 0..self.emitters.len() {
            let n = self.emitters[i].update(deltaTime);
            for _ in 0..n { self.emitParticle(); }
        }
        runOperators(&mut self.operators, &mut self.particleBag, deltaTime, &self.systemRenderState);
        self.particleBag.pruneExpired();
        let center = self.systemRenderState.getControlPoint(0);
        self.boundingBox = if self.particleBag.len() == 0 { Aabb::new(center, center) } else {
            let (mut minP, mut maxP) = (center, center);
            for p in self.particleBag.asSlice() {
                let r = Vec3::splat(p.radius);
                minP = minP.min(p.position - r);
                maxP = maxP.max(p.position + r);
            }
            Aabb::new(minP, maxP)
        };
        let mut merged = self.boundingBox;
        for child in &mut self.childRenderers {
            child.update(deltaTime);
            merged = merged.union(&child.boundingBox);
        }
        self.boundingBox = merged;
        // An empty emitter list is finished, so an empty system restarts every frame.
        if self.isFinished() { self.restart(); }
    }

    pub fn isFinished(&self) -> bool {
        self.emitters.iter().all(|e| e.isFinished()) && self.particleBag.len() == 0 && self.childRenderers.iter().all(|c| c.isFinished())
    }

    pub fn render(&mut self, camera: &Camera, pass: Pass) {
        // C# returns before children when this bag is empty. Kept.
        if self.particleBag.len() == 0 { return; }
        if pass == Pass::Translucent || pass == Pass::Both { drawParticles(&mut self.renderers, &self.particleBag, camera); }
        for child in &mut self.childRenderers { child.render(camera, Pass::Both); }
    }

    fn setupEmitters(&mut self, base: &KV, data: Vec<KV>) -> Result<(), Error> {
        let mut emitters = Vec::new();
        for info in &data {
            let class = kvClass(info)?;
            match ParticleControllerFactory::tryCreateEmitter(class, base, info) {
                Some(e) => emitters.push(e),
                None => eprintln!("Unsupported emitter class '{class}'."),
            }
        }
        self.emitters = emitters;
        Ok(())
    }

    fn setupInitializers(&mut self, data: Vec<KV>) -> Result<(), Error> {
        let mut initializers = Vec::new();
        for info in &data {
            let class = kvClass(info)?;
            match ParticleControllerFactory::tryCreateInitializer(class, info) {
                Some(i) => initializers.push(i),
                None => eprintln!("Unsupported initializer class '{class}'."),
            }
        }
        self.initializers = initializers;
        Ok(())
    }

    fn setupOperators(&mut self, data: Vec<KV>) -> Result<(), Error> {
        let mut operators = Vec::new();
        for info in &data {
            let class = kvClass(info)?;
            match ParticleControllerFactory::tryCreateOperator(class, info) {
                Some(o) => operators.push(o),
                None => eprintln!("Unsupported operator class '{class}'."),
            }
        }
        self.operators = operators;
        Ok(())
    }

    fn setupRenderers(&mut self, data: Vec<KV>) -> Result<(), Error> {
        let mut renderers = Vec::new();
        for info in &data {
            let class = kvClass(info)?;
            match ParticleControllerFactory::tryCreateRender(class, &self.source, info, &self.gfxModel)? {
                Some(r) => renderers.push(r),
                None => eprintln!("Unsupported renderer class '{class}'."),
            }
        }
        self.renderers = renderers;
        Ok(())
    }

    fn setupChildParticles(&mut self, names: &[String]) -> Result<(), Error> {
        let gfxModel = self.gfxModel.clone();
        let source = self.source.clone();
        let pos = self.position();
        for name in names {
            let path = SourcePath::Str(name.clone());
            let system = match result(source.getAsset::<Arc<dyn IParticleSystem>>(&path, None)) {
                Ok(s) => s,
                Err(e) => {
                    eprintln!("InvalidCastException: {name}: {e}");
                    return Err(Error::new(ErrorKind::InvalidData, format!("InvalidCastException: {name}: {e}")));
                }
            };
            self.childRenderers.push(ChildRenderer::new(gfxModel.clone(), source.clone(), system.as_ref(), pos)?);
        }
        Ok(())
    }
}

/// ParticleRenderer. C# has no `Start`, `Update` or `Render` on this class.
pub struct ParticleRenderer;

impl ParticleRenderer {
    pub fn new(_gfx: &IOpenGfxs, _source: &SourceRef, _obj: &dyn Any) -> Result<Self, Error> { Ok(Self) }
}

impl Renderer for ParticleRenderer {}
impl EginRenderer for ParticleRenderer {}

// #endregion

// #region EngineRenderer

fn gfxApi(gfx: &IOpenGfxs) -> Result<Arc<Mutex<OpenGLGfxApi>>, Error> {
    gfx.get(GfX::XAPI as usize).cloned().flatten().and_then(|s| s.downcast::<Mutex<OpenGLGfxApi>>().ok()).ok_or_else(|| Error::new(ErrorKind::InvalidData, "InvalidCastException: gfx[GfX.XApi]"))
}

/// EngineRenderer
pub struct EngineRenderer {
    gfx: IOpenGfxs,
    source: SourceRef,
    db: Arc<dyn CellDatabase + Send + Sync>,
    engine: Option<OpenGLOpenEngine>,
}

impl EngineRenderer {
    /// `obj` must be an `Arc<dyn CellDatabase>` stored inside the `Any`.
    /// A concrete database value does not downcast.
    pub fn fromAny(gfx: &IOpenGfxs, source: &SourceRef, obj: &Arc<dyn Any + Send + Sync>) -> Result<Self, Error> {
        let db = obj.downcast_ref::<Arc<dyn CellDatabase + Send + Sync>>().cloned()
            .ok_or_else(|| Error::new(ErrorKind::InvalidInput, "InvalidCastException: ICellDatabase"))?;
        Ok(Self { gfx: gfx.clone(), source: source.clone(), db, engine: None })
    }
}

impl Renderer for EngineRenderer {
    fn start(&mut self) {
        let api = match gfxApi(&self.gfx) {
            Ok(a) => a,
            Err(e) => { eprintln!("ERROR: {e}"); return; }
        };
        let mut engine = OpenGLOpenEngine::new(self.db.query(), OpenGLCellBuilder::new(self.db.archive(), api), false);
        engine.spawnPlayer(self.db.as_ref());
        self.engine = Some(engine);
    }

    fn update(&mut self, _deltaTime: f32) {
        if let Some(engine) = &mut self.engine { engine.update(); }
    }
}

impl EginRenderer for EngineRenderer {
    fn render(&mut self, camera: &Camera, _pass: Pass) {
        if let Some(engine) = &mut self.engine { engine.camera = Some(camera.clone()); }
    }
}

// #endregion

// #region WorldRenderer

/// WorldRenderer
pub struct WorldRenderer;

impl WorldRenderer {
    pub fn new(gfx: &IOpenGfxs, source: &SourceRef, obj: &dyn std::any::Any) -> Self { Self }
}

impl Renderer for WorldRenderer {}
impl EginRenderer for WorldRenderer {}

// #endregion

#[cfg(test)]
mod tests {
    use std::any::Any;
    use std::collections::HashMap;
    use std::io::{Error, ErrorKind};
    use std::sync::Arc;
    use glam::Vec3;
    use openx::manager::CellDatabase;
    use openx_gfx_egin::egin_render::IParticleSystem;
    use openx_poly::core::{BoxFuture, ISource, SourcePath, SourceRef};
    use openx_poly::pool::KV;
    use openx_poly::poly::Int3;
    use super::*;

    struct NoSource;
    impl ISource for NoSource {
        fn getAssetAny<'a>(&'a self, _path: &'a SourcePath, _option: Option<&'a (dyn Any + Sync)>) -> BoxFuture<'a, Result<Box<dyn Any + Send + Sync>, Error>> {
            Box::pin(async { Err(Error::new(ErrorKind::Other, "no asset")) })
        }
    }

    fn kv(pairs: &[(&str, KV)]) -> KV {
        let mut m = HashMap::new();
        for (k, v) in pairs { m.insert((*k).to_string(), v.clone()); }
        KV::Map(m)
    }

    struct Sys { data: KV, emitters: Vec<KV>, operators: Vec<KV>, children: Vec<String> }
    impl IParticleSystem for Sys {
        fn data(&self) -> &KV { &self.data }
        fn renderers(&self) -> Vec<KV> { Vec::new() }
        fn operators(&self) -> Vec<KV> { self.operators.clone() }
        fn initializers(&self) -> Vec<KV> { Vec::new() }
        fn emitters(&self) -> Vec<KV> { self.emitters.clone() }
        fn childParticleNames(&self, _enabledOnly: bool) -> Vec<String> { self.children.clone() }
    }

    fn child(system: &Sys) -> ChildRenderer {
        ChildRenderer::new(Arc::new(std::sync::Mutex::new(OpenGLGfxModel::new())), SourceRef::new(NoSource), system, Vec3::ZERO).unwrap()
    }

    #[test]
    fn unknown_controller_is_skipped() {
        assert!(ParticleControllerFactory::tryCreateEmitter("nope", &KV::Null, &KV::Null).is_none());
        assert!(ParticleControllerFactory::tryCreateOperator("C_OP_Decay", &KV::Null).is_some());
        assert!(ParticleControllerFactory::tryCreateInitializer("C_INIT_RandomLifeTime", &KV::Null).is_some());
    }

    #[test]
    fn empty_system_restarts_and_clears_its_lifetime() {
        let mut c = child(&Sys { data: KV::Null, emitters: Vec::new(), operators: Vec::new(), children: Vec::new() });
        c.update(0.5);
        assert_eq!(c.systemRenderState.lifetime, 0.);
        assert_eq!(c.particleBag.len(), 0);
        assert!(c.hasStarted);
    }

    #[test]
    fn burst_expires_and_the_emit_counter_stays() {
        let mut c = child(&Sys {
            data: KV::Null,
            emitters: vec![kv(&[("_class", KV::Str("C_OP_InstantaneousEmitter".into())), ("m_nParticlesToEmit", KV::Int(2))])],
            operators: vec![kv(&[("_class", KV::Str("C_OP_Decay".into()))])],
            children: Vec::new(),
        });
        c.update(2.);
        assert_eq!(c.particleBag.len(), 0, "decay and prune empty the bag, then restart clears it");
        assert_eq!(c.particlesEmitted, 2, "restart does not reset the counter");
        assert_eq!(c.systemRenderState.lifetime, 0.);
        assert!(!c.emitters[0].isFinished(), "restart calls start");
    }

    #[test]
    fn a_missing_child_system_fails_the_cast() {
        let system = Sys { data: KV::Null, emitters: Vec::new(), operators: Vec::new(), children: vec!["child".into()] };
        let err = match ChildRenderer::new(Arc::new(std::sync::Mutex::new(OpenGLGfxModel::new())), SourceRef::new(NoSource), &system, Vec3::ZERO) {
            Err(e) => e,
            Ok(_) => panic!("expected the cast to fail"),
        };
        assert!(err.to_string().contains("InvalidCastException"), "{err}");
    }

    struct TestDb;
    impl CellDatabase for TestDb {
        fn archive(&self) -> SourceRef { SourceRef::new(NoSource) }
        fn query(&self) -> openx::manager::SharedQuery { unimplemented!() }
        fn cellId(&self) -> Int3 { Int3::ZERO }
        fn cellInterior(&self) -> bool { false }
        fn playerPosition(&self) -> Vec3 { Vec3::ZERO }
        fn playerRotation(&self) -> glam::Quat { glam::Quat::IDENTITY }
    }

    #[test]
    fn from_any_rejects_a_plain_value() {
        let obj: Arc<dyn Any + Send + Sync> = Arc::new(1u32);
        let err = match EngineRenderer::fromAny(&Vec::new(), &SourceRef::new(NoSource), &obj) {
            Err(e) => e,
            Ok(_) => panic!("expected the cast to fail"),
        };
        assert!(err.to_string().contains("InvalidCastException"), "{err}");
    }

    #[test]
    fn from_any_accepts_a_boxed_database() {
        let db: Arc<dyn CellDatabase + Send + Sync> = Arc::new(TestDb);
        let obj: Arc<dyn Any + Send + Sync> = Arc::new(db);
        assert!(EngineRenderer::fromAny(&Vec::new(), &SourceRef::new(NoSource), &obj).is_ok());
    }
}
