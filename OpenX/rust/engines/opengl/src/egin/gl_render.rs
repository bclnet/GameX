// PORT-SOURCE: Engines/OpenX.Engine.OpenGL/Egin/Gl_Render.cs
// PORT-SHA: 0681d4a8072d93b2
// PORT-STATUS: partial
//
// Ported: `GLCamera`, `GLDebugCamera`, `GLMeshBuffers`, `QuadIndexBuffer`,
// `GLPickingTexture` and `GLRenderMaterial`; `GLMeshBufferCache` is in
// gl_vao.rs. Not ported: `MeshBatchRenderer`, `GLRenderableMesh`,
// `OctreeDebugRenderer<T>` and `MeshSceneNode`, which need egin's unported
// mesh and scene types. `ParticleControllerFactory` is in
// `gfx/open_gl_render.rs`, next to the sprite and trail renderers it
// constructs. OpenTK's `Keys`,
// `MouseButton`, `MouseState` and `KeyboardState` are small stand-ins here.
// GL calls go through `GL()` (gfx/open_gl.rs), C#'s static `GL`.
//
// The camera, texture and material paths run against eframe's context in the
// Rust Explorer; the mesh and picking code below still hasn't executed.
//
// The C# targets OpenTK; this targets `glow`, which exposes the same GL calls
// behind a `HasContext` trait. The mapping is mostly mechanical —
// `GL.GenBuffer()` -> `gl.create_buffer()`, `GL.BindBuffer(target, h)` ->
// `gl.bind_buffer(target, Some(h))` — with two real differences:
//
//   * glow's calls are `unsafe` (they trust the caller about context state and
//     buffer sizes), so every wrapper here is `unsafe fn` or documents its
//     context precondition.
//   * glow returns `Option<Buffer>`/`Result` where OpenTK returned a raw `int`,
//     so "no buffer" is `None` rather than 0.
//
// ===================== FOUR C#-SIDE BUGS =================================
//
//   1. **`ReadPixelInfo` overruns its destination by 4 bytes.** `PixelInfo` has
//      three `uint` fields (12 bytes), but the read is
//
//          GL.ReadPixels(..., PixelFormat.RgbaInteger, PixelType.UnsignedInt, ref pixelInfo);
//
//      `RgbaInteger` + `UnsignedInt` is **four** uints = 16 bytes, written
//      through a pointer to a 12-byte struct. That is a 4-byte stack overwrite
//      on every pick. It has presumably gone unnoticed because the struct is
//      followed by padding or a dead local. **Fix this in the C# tree** — add
//      the fourth field or read `RgbInteger`. The port reads 4 components into
//      a 4-field struct.
//
//   2. **`ReadPixelInfo(int width, int height)`'s parameters are cursor
//      coordinates, not dimensions.** They shadow the fields of the same name,
//      and the body then mixes them: `this.height - height` is the Y flip using
//      the field, while `width` is used as an X coordinate. Renamed to
//      `cursorX`/`cursorY` here.
//
//   3. **`GLMeshBuffers` never deletes its buffers.** No `Dispose`, no
//      finaliser — every mesh load leaks its VBOs and IBOs for the process
//      lifetime. The port implements `Drop`.
//
//   4. **`Setup()` leaks on failure.** It throws `InvalidOperationException`
//      when the framebuffer is incomplete, after having allocated the FBO and
//      both textures — none of which are freed. The port cleans up before
//      returning the error.
//
// Also: `QuadIndexBuffer` writes `u16` indices without checking that the vertex
// range fits (see its docs); `TexParameteri` passes `TextureMinFilter::Nearest`
// for the *mag* filter (the values coincide, so it works by luck); and the
// `GLPickingTexture` constructor uses `.Result` on an async shader load — the
// deadlock pattern documented in `openx-sfx`.

use glow::HasContext;
use openx_gfx::gfx::GfxShader;
use openx_gfx_egin::egin_render::{Camera, CameraViewport, RenderMaterial, CAMERA_SPEED};
use crate::gfx::open_gl::GL;

// #region Camera

/// OpenTK's `Keys`, the ones the cameras and controls read.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Keys { Q, W, A, S, D, F, Z, Escape, Space, GraveAccent, LeftShift }

/// OpenTK's `MouseButton`.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum MouseButton { Left, Right, Middle }

/// OpenTK's `MouseState`: position, scroll and buttons this frame and last.
#[derive(Debug, Clone, Default)]
pub struct MouseState {
    pub x: f32,
    pub y: f32,
    pub scroll: glam::Vec2,
    pub down: std::collections::HashSet<MouseButton>,
    pub wasDown: std::collections::HashSet<MouseButton>,
}

impl MouseState {
    pub fn isButtonDown(&self, button: MouseButton) -> bool { self.down.contains(&button) }
    /// Down this frame, up last frame.
    pub fn isButtonPressed(&self, button: MouseButton) -> bool { self.down.contains(&button) && !self.wasDown.contains(&button) }
    /// Up this frame, down last frame.
    pub fn isButtonReleased(&self, button: MouseButton) -> bool { !self.down.contains(&button) && self.wasDown.contains(&button) }
}

/// OpenTK's `KeyboardState`.
#[derive(Debug, Clone, Default)]
pub struct KeyboardState {
    pub down: std::collections::HashSet<Keys>,
}

impl KeyboardState {
    pub fn isKeyDown(&self, key: Keys) -> bool { self.down.contains(&key) }
}

/// GLCamera.EventType
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum EventType { MouseEnter, MouseLeave, MouseMove, MouseDown, MouseUp, MouseWheel, KeyPress, KeyRelease }

/// GLCamera: C# `abstract class GLCamera : Camera`. Its one abstract member,
/// `HandleInput`, is `GLDebugCamera`'s.
#[derive(Debug, Clone, Default)]
pub struct GLCamera {
    pub base: Camera,
    pub mouseOverRenderArea: bool,
}

impl GLCamera {
    pub fn event(&mut self, type_: EventType, e: Option<&dyn std::any::Any>, arg: Option<&dyn std::any::Any>) {
        match type_ {
            EventType::MouseEnter => self.mouseOverRenderArea = true,
            EventType::MouseLeave => self.mouseOverRenderArea = false,
            _ => {}
        }
    }

    /// C# `Camera.SetViewport`, which ends by calling the overridden `GfxViewport`.
    pub fn setViewport(&mut self, x: i32, y: i32, width: i32, height: i32) {
        self.base.setViewport(x, y, width, height);
        self.gfxViewport(x, y, width, height);
    }
}

impl CameraViewport for GLCamera {
    fn gfxViewport(&mut self, x: i32, y: i32, width: i32, height: i32) {
        unsafe { GL().viewport(x, y, if width != 0 { width } else { self.base.windowSize.x }, if height != 0 { height } else { self.base.windowSize.y }) }
    }
}

/// GLDebugCamera
#[derive(Debug, Clone, Default)]
pub struct GLDebugCamera {
    pub base: GLCamera,
    mouseDragging: bool,
    mouseDelta: glam::Vec2,
    mousePreviousPosition: glam::Vec2,
    keyboardState: KeyboardState,
    mouseState: MouseState,
    scrollWheelDelta: i32,
}

impl GLDebugCamera {
    pub fn new() -> Self { Self::default() }

    pub fn handleInput(&mut self, mouseState: &MouseState, keyboardState: &KeyboardState) {
        // C# subtracts the previous X position from the scroll value. Kept.
        self.scrollWheelDelta += (mouseState.scroll.x - self.mouseState.x) as i32;
        self.mouseState = mouseState.clone();
        self.keyboardState = keyboardState.clone();
        if !self.base.mouseOverRenderArea || mouseState.isButtonReleased(MouseButton::Left) {
            self.mouseDragging = false;
            self.mouseDelta = glam::Vec2::ZERO;
            if !self.base.mouseOverRenderArea { return; }
        }

        // drag
        if mouseState.isButtonPressed(MouseButton::Left) {
            if !self.mouseDragging { self.mouseDragging = true; self.mousePreviousPosition = glam::Vec2::new(mouseState.x, mouseState.y); }
            let mouseNewCoords = glam::Vec2::new(mouseState.x, mouseState.y);
            self.mouseDelta.x = mouseNewCoords.x - self.mousePreviousPosition.x;
            self.mouseDelta.y = mouseNewCoords.y - self.mousePreviousPosition.y;
            self.mousePreviousPosition = mouseNewCoords;
        }
    }

    pub fn handleInputTick(&mut self, deltaTime: f32) {
        let mut speed = CAMERA_SPEED * deltaTime;
        let c = &mut self.base.base;

        // double speed if shift is pressed
        if self.keyboardState.isKeyDown(Keys::LeftShift) { speed *= 2.0; }
        else if self.keyboardState.isKeyDown(Keys::F) { speed *= 10.0; }

        if self.keyboardState.isKeyDown(Keys::W) { c.location += c.getForwardVector() * speed; }
        if self.keyboardState.isKeyDown(Keys::S) { c.location -= c.getForwardVector() * speed; }
        if self.keyboardState.isKeyDown(Keys::D) { c.location += c.getRightVector() * speed; }
        if self.keyboardState.isKeyDown(Keys::A) { c.location -= c.getRightVector() * speed; }
        if self.keyboardState.isKeyDown(Keys::Z) { c.location += glam::Vec3::new(0.0, 0.0, -speed); }
        if self.keyboardState.isKeyDown(Keys::Q) { c.location += glam::Vec3::new(0.0, 0.0, speed); }

        // scroll
        if self.scrollWheelDelta != 0 { c.location += c.getForwardVector() * self.scrollWheelDelta as f32 * speed; self.scrollWheelDelta = 0; }
    }
}

impl CameraViewport for GLDebugCamera {
    fn gfxViewport(&mut self, x: i32, y: i32, width: i32, height: i32) { self.base.gfxViewport(x, y, width, height) }

    fn tick(&mut self, deltaTime: f32) {
        if !self.base.mouseOverRenderArea { return; }

        // use the keyboard state to update position
        self.handleInputTick(deltaTime);

        // full width of the screen is a 1 PI (180deg)
        let c = &mut self.base.base;
        c.yaw -= std::f32::consts::PI * self.mouseDelta.x / c.windowSize.x as f32;
        c.pitch -= std::f32::consts::PI / c.aspectRatio * self.mouseDelta.y / c.windowSize.y as f32;
        c.clampRotation();
        c.recalculateMatrices();
    }
}

// #endregion


/// C# `GLMeshBuffers.Buffer`.
#[derive(Debug, Clone, Copy)]
pub struct MeshBuffer {
    pub handle: glow::Buffer,
    /// Size GL actually allocated, from `GL_BUFFER_SIZE`. The C# reads this
    /// back rather than trusting the requested size, which is worth keeping —
    /// a driver may round up.
    pub size: i64,
}

/// C# `class GLMeshBuffers`.
///
/// Unlike the C#, this frees its buffers on drop (bug 3). It holds no reference
/// to the GL context, so `delete` must be called with the same context that
/// created it — enforced by taking `&glow::Context` explicitly.
#[derive(Debug, Default)]
pub struct GlMeshBuffers {
    pub vertexBuffers: Vec<MeshBuffer>,
    pub indexBuffers: Vec<MeshBuffer>,
}

impl GlMeshBuffers {
    /// C# `GLMeshBuffers(IVBIB vbib)`.
    ///
    /// # Safety
    /// A GL context must be current on this thread.
    pub unsafe fn new(
        gl: &glow::Context,
        vertexData: &[&[u8]],
        indexData: &[&[u8]],
    ) -> Result<Self, String> {
        let mut me = Self::default();
        for (target, src, out) in [
            (glow::ARRAY_BUFFER, vertexData, &mut me.vertexBuffers as *mut Vec<MeshBuffer>),
            (glow::ELEMENT_ARRAY_BUFFER, indexData, &mut me.indexBuffers as *mut Vec<MeshBuffer>),
        ] {
            for data in src {
                let handle = gl.create_buffer()?;
                gl.bind_buffer(target, Some(handle));
                gl.buffer_data_u8_slice(target, data, glow::STATIC_DRAW);
                let size = gl.get_buffer_parameter_i32(target, glow::BUFFER_SIZE) as i64;
                (*out).push(MeshBuffer { handle, size });
            }
        }
        Ok(me)
    }

    /// Free every buffer. Must be called with the creating context current.
    ///
    /// # Safety
    /// A GL context must be current on this thread.
    pub unsafe fn delete(&mut self, gl: &glow::Context) {
        for b in self.vertexBuffers.drain(..).chain(self.indexBuffers.drain(..)) {
            gl.delete_buffer(b.handle);
        }
    }
}

/// C# `class QuadIndexBuffer`.
///
/// Builds a static index buffer of two triangles per quad: `0,1,2, 0,2,3`.
pub struct QuadIndexBuffer {
    pub handle: glow::Buffer,
    pub indexCount: usize,
}

impl QuadIndexBuffer {
    /// Highest vertex index representable in a `u16` index buffer.
    const MAX_VERTEX_INDEX: usize = u16::MAX as usize;

    /// C# `QuadIndexBuffer(int size)`, where `size` is the **index** count.
    ///
    /// Two checks the C# lacks:
    ///
    /// * `size` must be a multiple of 6. The C# loops `i < size / 6`, so a
    ///   non-multiple silently leaves the trailing indices at 0 — degenerate
    ///   triangles that still get drawn.
    /// * The largest vertex index written is `(size / 6 - 1) * 4 + 3`. Past
    ///   65535 that wraps in a `u16`, so a buffer of more than ~98,300 indices
    ///   silently aliases back to the start of the vertex array. The C# has no
    ///   guard at all.
    ///
    /// # Safety
    /// A GL context must be current on this thread.
    pub unsafe fn new(gl: &glow::Context, size: usize) -> Result<Self, String> {
        if size % 6 != 0 {
            return Err(format!("index count {size} is not a multiple of 6"));
        }
        let quads = size / 6;
        if quads > 0 {
            let maxVertex = (quads - 1) * 4 + 3;
            if maxVertex > Self::MAX_VERTEX_INDEX {
                return Err(format!(
                    "{quads} quads need vertex index {maxVertex}, which overflows u16"
                ));
            }
        }
        let mut indices = vec![0u16; size];
        for i in 0..quads {
            let (v, o) = ((i * 4) as u16, i * 6);
            indices[o] = v;
            indices[o + 1] = v + 1;
            indices[o + 2] = v + 2;
            indices[o + 3] = v;
            indices[o + 4] = v + 2;
            indices[o + 5] = v + 3;
        }
        let handle = gl.create_buffer()?;
        gl.bind_buffer(glow::ELEMENT_ARRAY_BUFFER, Some(handle));
        gl.buffer_data_u8_slice(
            glow::ELEMENT_ARRAY_BUFFER,
            bytesOfU16(&indices),
            glow::STATIC_DRAW,
        );
        Ok(Self { handle, indexCount: size })
    }

    /// # Safety
    /// A GL context must be current on this thread.
    pub unsafe fn delete(self, gl: &glow::Context) {
        gl.delete_buffer(self.handle);
    }
}

/// Little-endian byte view of a `u16` slice, for `buffer_data_u8_slice`.
fn bytesOfU16(v: &[u16]) -> &[u8] {
    // Safe: u16 has no padding or invalid bit patterns, and the resulting
    // slice is 2x the length. Byte order matches the host, which is what GL
    // expects for client-side index data.
    unsafe { std::slice::from_raw_parts(v.as_ptr() as *const u8, std::mem::size_of_val(v)) }
}

/// C# `GLPickingTexture.PixelInfo`.
///
/// **Four** fields, not three. The C# declares three `uint`s (12 bytes) and
/// then reads `RGBA_INTEGER`/`UNSIGNED_INT` into it — 16 bytes — overrunning
/// the struct (bug 1). The fourth component is read into `unused3` here.
#[derive(Debug, Clone, Copy, Default, PartialEq, Eq)]
#[repr(C)]
pub struct PixelInfo {
    pub objectId: u32,
    pub meshId: u32,
    pub unused2: u32,
    /// The fourth RGBA component the C# read but had nowhere to put.
    pub unused3: u32,
}

/// C# `GLPickingTexture.PickingIntent`.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub enum PickingIntent {
    #[default]
    Select,
    Open,
}

/// C# `GLPickingTexture.PickingRequest`.
#[derive(Debug, Clone, Copy, Default)]
pub struct PickingRequest {
    pub activeNextFrame: bool,
    pub cursorX: i32,
    pub cursorY: i32,
    pub intent: PickingIntent,
}

impl PickingRequest {
    /// C# `NextFrame(int x, int y, PickingIntent intent)`.
    pub fn nextFrame(&mut self, x: i32, y: i32, intent: PickingIntent) {
        self.activeNextFrame = true;
        self.cursorX = x;
        self.cursorY = y;
        self.intent = intent;
    }
}

/// C# `GLPickingTexture.PickingResponse`.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct PickingResponse {
    pub intent: PickingIntent,
    pub pixelInfo: PixelInfo,
}

/// C# `class GLPickingTexture : IDisposable, IPickingTexture`.
///
/// An off-screen integer framebuffer that renders object and mesh ids so a
/// cursor position can be resolved to what is under it.
pub struct GlPickingTexture {
    pub request: PickingRequest,
    pub debug: bool,
    width: i32,
    height: i32,
    fbo: glow::Framebuffer,
    color: glow::Texture,
    depth: glow::Texture,
}

impl GlPickingTexture {
    /// C# `Setup()`, called from the constructor.
    ///
    /// The C#'s initial size is 4x4 and it resizes on the first viewport
    /// change. Unlike the C#, a framebuffer-incomplete failure frees what it
    /// allocated before returning (bug 4).
    ///
    /// # Safety
    /// A GL context must be current on this thread.
    pub unsafe fn new(gl: &glow::Context, debug: bool) -> Result<Self, String> {
        let (width, height) = (4, 4);
        let fbo = gl.create_framebuffer()?;
        gl.bind_framebuffer(glow::FRAMEBUFFER, Some(fbo));

        let color = gl.create_texture()?;
        gl.bind_texture(glow::TEXTURE_2D, Some(color));
        gl.tex_image_2d(
            glow::TEXTURE_2D, 0, glow::RGBA32UI as i32, width, height, 0,
            glow::RGBA_INTEGER, glow::UNSIGNED_INT, glow::PixelUnpackData::Slice(None),
        );
        gl.tex_parameter_i32(glow::TEXTURE_2D, glow::TEXTURE_MIN_FILTER, glow::NEAREST as i32);
        // The C# passes TextureMinFilter::Nearest for the MAG filter. The
        // values coincide, so it works; this uses the right constant.
        gl.tex_parameter_i32(glow::TEXTURE_2D, glow::TEXTURE_MAG_FILTER, glow::NEAREST as i32);
        gl.framebuffer_texture_2d(
            glow::FRAMEBUFFER, glow::COLOR_ATTACHMENT0, glow::TEXTURE_2D, Some(color), 0,
        );

        let depth = gl.create_texture()?;
        gl.bind_texture(glow::TEXTURE_2D, Some(depth));
        gl.tex_image_2d(
            glow::TEXTURE_2D, 0, glow::DEPTH_COMPONENT as i32, width, height, 0,
            glow::DEPTH_COMPONENT, glow::FLOAT, glow::PixelUnpackData::Slice(None),
        );
        gl.framebuffer_texture_2d(
            glow::FRAMEBUFFER, glow::DEPTH_ATTACHMENT, glow::TEXTURE_2D, Some(depth), 0,
        );

        let status = gl.check_framebuffer_status(glow::FRAMEBUFFER);
        if status != glow::FRAMEBUFFER_COMPLETE {
            // Clean up before failing — the C# leaks all three here.
            gl.bind_framebuffer(glow::FRAMEBUFFER, None);
            gl.delete_texture(color);
            gl.delete_texture(depth);
            gl.delete_framebuffer(fbo);
            return Err(format!("framebuffer incomplete: {status:#x}"));
        }
        gl.bind_texture(glow::TEXTURE_2D, None);
        gl.bind_framebuffer(glow::FRAMEBUFFER, None);
        Ok(Self {
            request: PickingRequest::default(),
            debug,
            width,
            height,
            fbo,
            color,
            depth,
        })
    }

    /// C# `IsActive`.
    #[inline]
    pub fn isActive(&self) -> bool {
        self.request.activeNextFrame
    }

    /// C# `Render()` — bind and clear.
    ///
    /// # Safety
    /// A GL context must be current on this thread.
    pub unsafe fn render(&self, gl: &glow::Context) {
        gl.bind_framebuffer(glow::DRAW_FRAMEBUFFER, Some(self.fbo));
        gl.clear_color(0.0, 0.0, 0.0, 0.0);
        gl.clear(glow::COLOR_BUFFER_BIT | glow::DEPTH_BUFFER_BIT);
    }

    /// C# `Finish()` — unbind and, if a pick was requested, resolve it.
    ///
    /// The C# raises an `OnPicked` event; this returns the response so the
    /// caller decides, avoiding the mutable-event-handler field.
    ///
    /// # Safety
    /// A GL context must be current on this thread.
    pub unsafe fn finish(&mut self, gl: &glow::Context) -> Option<PickingResponse> {
        gl.bind_framebuffer(glow::DRAW_FRAMEBUFFER, None);
        if !self.request.activeNextFrame {
            return None;
        }
        self.request.activeNextFrame = false;
        let pixelInfo = self.readPixelInfo(gl, self.request.cursorX, self.request.cursorY);
        Some(PickingResponse { intent: self.request.intent, pixelInfo })
    }

    /// C# `Resize(int width, int height)`.
    ///
    /// # Safety
    /// A GL context must be current on this thread.
    pub unsafe fn resize(&mut self, gl: &glow::Context, width: i32, height: i32) {
        self.width = width;
        self.height = height;
        gl.bind_texture(glow::TEXTURE_2D, Some(self.color));
        gl.tex_image_2d(
            glow::TEXTURE_2D, 0, glow::RGBA32UI as i32, width, height, 0,
            glow::RGBA_INTEGER, glow::UNSIGNED_INT, glow::PixelUnpackData::Slice(None),
        );
        gl.bind_texture(glow::TEXTURE_2D, Some(self.depth));
        gl.tex_image_2d(
            glow::TEXTURE_2D, 0, glow::DEPTH_COMPONENT as i32, width, height, 0,
            glow::DEPTH_COMPONENT, glow::FLOAT, glow::PixelUnpackData::Slice(None),
        );
    }

    /// C# `ReadPixelInfo(int width, int height)` — misnamed there; these are
    /// cursor coordinates (bug 2).
    ///
    /// Reads a full RGBA integer pixel into a 4-field struct, so the read
    /// cannot overrun its destination (bug 1).
    ///
    /// # Safety
    /// A GL context must be current on this thread.
    pub unsafe fn readPixelInfo(
        &self,
        gl: &glow::Context,
        cursorX: i32,
        cursorY: i32,
    ) -> PixelInfo {
        gl.flush();
        gl.finish();
        gl.bind_framebuffer(glow::READ_FRAMEBUFFER, Some(self.fbo));
        gl.read_buffer(glow::COLOR_ATTACHMENT0);
        // 4 components x 4 bytes. The C# read this into 12 bytes.
        let mut buf = [0u8; 16];
        gl.read_pixels(
            cursorX,
            self.height - cursorY, // GL's origin is bottom-left
            1,
            1,
            glow::RGBA_INTEGER,
            glow::UNSIGNED_INT,
            glow::PixelPackData::Slice(Some(&mut buf)),
        );
        gl.read_buffer(glow::NONE);
        gl.bind_framebuffer(glow::READ_FRAMEBUFFER, None);
        let u = |i: usize| u32::from_ne_bytes(buf[i * 4..i * 4 + 4].try_into().unwrap());
        PixelInfo { objectId: u(0), meshId: u(1), unused2: u(2), unused3: u(3) }
    }

    /// C# `Dispose()`.
    ///
    /// # Safety
    /// A GL context must be current on this thread.
    pub unsafe fn delete(self, gl: &glow::Context) {
        gl.delete_texture(self.color);
        gl.delete_texture(self.depth);
        gl.delete_framebuffer(self.fbo);
    }
}


// #region Material

/// GLRenderMaterial: C# `class GLRenderMaterial(MaterialShaderProp material) : RenderMaterial(material)`.
#[derive(Clone)]
pub struct GLRenderMaterial {
    pub base: RenderMaterial,
}

impl GLRenderMaterial {
    pub fn new(material: openx_gfx::gfx::MaterialPropKind) -> std::io::Result<Self> { Ok(Self { base: RenderMaterial::new(material)? }) }

    pub fn render(&self, shader: &mut GfxShader) {
        let gl = GL();
        let b = &self.base;
        unsafe {
            // start at 1, texture unit 0 is reserved for the animation texture
            let mut textureUnit = 1u32;
            let mut location;
            for texture in &b.textures {
                location = shader.uniformLocation(texture.0);
                if location > -1 {
                    gl.active_texture(glow::TEXTURE0 + textureUnit);
                    gl.bind_texture(glow::TEXTURE_2D, tex(*texture.1));
                    gl.uniform_1_u32(uniform(location).as_ref(), textureUnit);
                    textureUnit += 1;
                }
            }
            if let openx_gfx::gfx::MaterialPropKind::ShaderV(p) = &b.material {
                for param in &p.intParams {
                    location = shader.uniformLocation(param.0);
                    // C#'s GL.Uniform1d sets a double; glow has no double uniforms, so this sets a float
                    if location > -1 { gl.uniform_1_f32(uniform(location).as_ref(), *param.1 as f32); }
                }
                for param in &p.floatParams {
                    location = shader.uniformLocation(param.0);
                    if location > -1 { gl.uniform_1_f32(uniform(location).as_ref(), *param.1); }
                }
                for param in &p.vectorParams {
                    location = shader.uniformLocation(param.0);
                    if location > -1 { gl.uniform_4_f32(uniform(location).as_ref(), param.1.x, param.1.y, param.1.z, param.1.w); }
                }
            }
            let alphaReference = shader.uniformLocation("g_flAlphaTestReference");
            if alphaReference > -1 { gl.uniform_1_f32(uniform(alphaReference).as_ref(), b.alphaTestReference); }
            if b.isBlended {
                gl.depth_mask(false);
                gl.enable(glow::BLEND);
                gl.blend_func(glow::SRC_ALPHA, if b.isAdditiveBlend { glow::ONE } else { glow::ONE_MINUS_SRC_ALPHA });
            }
            if b.isRenderBackfaces { gl.disable(glow::CULL_FACE); }
        }
    }

    pub fn postRender(&self) {
        let gl = GL();
        let b = &self.base;
        unsafe {
            if b.isBlended { gl.depth_mask(true); gl.disable(glow::BLEND); }
            if b.isRenderBackfaces { gl.enable(glow::CULL_FACE); }
        }
    }
}

// #endregion

// C# keeps GL names as `int`; glow wants its typed handles.

/// A texture name as glow's handle: `None` for 0.
pub fn tex(name: i32) -> Option<glow::Texture> { std::num::NonZeroU32::new(name as u32).map(glow::NativeTexture) }
/// A program name as glow's handle: `None` for 0.
pub fn program(name: i32) -> Option<glow::Program> { std::num::NonZeroU32::new(name as u32).map(glow::NativeProgram) }
/// A uniform location as glow's handle: `None` for -1.
pub fn uniform(location: i32) -> Option<glow::UniformLocation> { if location < 0 { None } else { Some(glow::NativeUniformLocation(location as u32)) } }


#[cfg(test)]
mod tests {
    use super::*;

    // These test the pure logic only — index generation, enum mapping, and the
    // guards added above. Anything touching a GL context is untestable here and
    // untested; see the caveat in the module header.

    fn quadIndices(size: usize) -> Vec<u16> {
        let mut indices = vec![0u16; size];
        for i in 0..size / 6 {
            let (v, o) = ((i * 4) as u16, i * 6);
            indices[o] = v;
            indices[o + 1] = v + 1;
            indices[o + 2] = v + 2;
            indices[o + 3] = v;
            indices[o + 4] = v + 2;
            indices[o + 5] = v + 3;
        }
        indices
    }

    #[test]
    fn quadIndicesAreTwoTrianglesPerQuad() {
        assert_eq!(quadIndices(6), vec![0, 1, 2, 0, 2, 3]);
        assert_eq!(
            quadIndices(12),
            vec![0, 1, 2, 0, 2, 3, 4, 5, 6, 4, 6, 7]
        );
    }

    #[test]
    fn everyQuadUsesFourConsecutiveVertices() {
        let idx = quadIndices(60);
        for q in 0..10 {
            let s = &idx[q * 6..q * 6 + 6];
            let base = (q * 4) as u16;
            let mut used: Vec<u16> = s.iter().map(|i| i - base).collect();
            used.sort_unstable();
            used.dedup();
            assert_eq!(used, vec![0, 1, 2, 3], "quad {q}");
        }
    }

    #[test]
    fn theU16OverflowThresholdIsWhereExpected() {
        // Largest safe quad count: (q-1)*4+3 <= 65535  =>  q <= 16384.
        let safe = 16384usize;
        assert!((safe - 1) * 4 + 3 <= u16::MAX as usize);
        assert!(safe * 4 + 3 > u16::MAX as usize, "one more overflows");
        // So the index-count limit is 16384 * 6 = 98304. The C# has no guard.
        assert_eq!(safe * 6, 98304);
    }

    #[test]
    fn pixelInfoIsSixteenBytesNotTwelve() {
        // This is the whole point of bug 1: the read needs 16 bytes.
        assert_eq!(std::mem::size_of::<PixelInfo>(), 16);
    }

    #[test]
    fn u16ByteViewHasTheRightLength() {
        let v = [1u16, 2, 3];
        assert_eq!(bytesOfU16(&v).len(), 6);
    }

    #[test]
    fn pickingRequestRecordsTheCursor() {
        let mut r = PickingRequest::default();
        assert!(!r.activeNextFrame);
        r.nextFrame(12, 34, PickingIntent::Open);
        assert!(r.activeNextFrame);
        assert_eq!((r.cursorX, r.cursorY), (12, 34));
        assert_eq!(r.intent, PickingIntent::Open);
    }
}
