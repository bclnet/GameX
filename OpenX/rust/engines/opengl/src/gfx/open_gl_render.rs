// PORT-SOURCE: Engines/OpenX.Engine.OpenGL/Gfx/OpenGL_Render.cs
// PORT-SHA: d4101965a5beaf8b
// PORT-STATUS: partial
//
// Ported: `TestTriRenderer`, `TextureRenderer`, `MaterialRenderer`,
// `GridRenderer` and `WorldRenderer`. Not ported:
//   * `ObjectRenderer`: C# passes the loaded object as the manager's `path`, and
//     the archive hands it back; Rust's `SourcePath` is only a string or an int.
//   * `ParticleRenderer`: needs egin's particle system (Egin_Particle.cs) and
//     scene types.
//   * `EngineRenderer`: needs `OpenGLOpenEngine`, which needs `CellManager`.
//
// A renderer gets C#'s `IOpenGfx[]` as `IOpenGfxs` and downcasts the model
// slot, as C# casts `(OpenGLGfxModel)gfx[GfX.XModel]`. C#'s `.Result` on a
// manager task is `result`. `TextureRenderer` takes the texture as
// `Arc<dyn ITexture>` and caches it under its own address, as C# keys the
// cache on the object; it can't ask for `ITextureFrames` through that, so
// `Update`'s frame stepping isn't wired.

use std::future::Future;
use std::io::{Error, ErrorKind};
use std::ops::Range;
use std::sync::{Arc, Mutex};
use std::task::{Context, Poll, Wake, Waker};
use glam::Mat4;
use glow::HasContext;
use openx_gfx::gfx::{GfX, GfxShader, IOpenGfxModel, IOpenGfxs, ITexture, MaterialProp};
use openx_gfx::gfx_render::{Pass, Renderer};
use openx_gfx_egin::egin_render::{Aabb, Camera, EginRenderer};
use openx_poly::core::{SourcePath, SourceRef};
use crate::egin::gl_render::{program, tex, uniform, GLRenderMaterial};
use crate::engine_open_gl::OpenGLGfxModel;
use crate::gfx::open_gl::GL;

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
    frameDelay: i32,
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
        Ok(Self { gfxModel, source: source.clone(), obj, level, tex, shader, vao, background, frameDelay: 0 })
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
        // C# steps an `ITextureFrames` here; see the header.
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

// #region WorldRenderer

/// WorldRenderer
pub struct WorldRenderer;

impl WorldRenderer {
    pub fn new(gfx: &IOpenGfxs, source: &SourceRef, obj: &dyn std::any::Any) -> Self { Self }
}

impl Renderer for WorldRenderer {}
impl EginRenderer for WorldRenderer {}

// #endregion
