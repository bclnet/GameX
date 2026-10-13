// PORT-SOURCE: Engines/OpenX.Wpf.Control/UI/GLWpfControl.cs
// PORT-SHA: 6365dc211ede5fab
// PORT-STATUS: done
//
// C#'s control derives from OpenTK.Wpf's `GLWpfControl`, which owns a GL 4.6
// compatibility context and calls `Render` every frame. In Rust the host is
// egui (ADR 0005): `ui` draws through an egui paint callback on eframe's own
// glow context, which it hands to `setGL`, and asks for a repaint each frame
// (C#'s `RenderContinuously`). The callback's rectangle is the control: `x`
// and `y` of `SetViewport` are its lower-left corner in GL's pixels.
//
// C#'s virtual `SetViewport`, `Tick` and `Render(Camera, float)` are
// `GLWpfControlX`'s methods, so `OpenGLControl` can override them.

use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::{Arc, Mutex};
use std::time::Instant;
use glow::HasContext;
use openx_engine_opengl::egin::gl_render::{EventType, GLDebugCamera};
use openx_engine_opengl::gfx::open_gl::{setGL, GL};
use openx_gfx::gfx::GfX;
use openx_gfx_egin::egin_render::{Camera, CameraViewport};

static CheckGLCalled: AtomicBool = AtomicBool::new(false);
pub static ShowConsole: AtomicBool = AtomicBool::new(false);

/// GLWpfControl
pub struct GLWpfControl {
    /// C# `GLCamera Camera`, a `GLDebugCamera`.
    pub camera: GLDebugCamera,
    pub viewportChanged: bool,
    initialized: bool,
    elapsed: Option<Instant>,
}

impl Default for GLWpfControl {
    fn default() -> Self { Self { camera: GLDebugCamera::new(), viewportChanged: true, initialized: false, elapsed: None } }
}

impl GLWpfControl {
    pub fn new() -> Self { Self::default() }

    fn onInitialized(&mut self) {
        Self::checkGL();
        self.camera = GLDebugCamera::new();
        unsafe { GL().enable(glow::DEPTH_TEST); }
    }

    fn checkGL() {
        if CheckGLCalled.swap(true, Ordering::Relaxed) { return; }
        let gl = GL();
        unsafe {
            println!("OpenGL version: {}", gl.get_parameter_string(glow::VERSION));
            println!("OpenGL vendor: {}", gl.get_parameter_string(glow::VENDOR));
            println!("GLSL version: {}", gl.get_parameter_string(glow::SHADING_LANGUAGE_VERSION));
        }
        if gl.supported_extensions().contains("GL_EXT_texture_filter_anisotropic") {
            let z = unsafe { gl.get_parameter_i32(glow::MAX_TEXTURE_MAX_ANISOTROPY_EXT) };
            GfX::MaxTextureMaxAnisotropy.store(z, Ordering::Relaxed);
            println!("MaxTextureMaxAnisotropyExt: {z}");
        }
        else { eprintln!("GL_EXT_texture_filter_anisotropic is not supported"); }
    }

    /// Draws the control into `ui`'s remaining space.
    pub fn ui<T: GLWpfControlX + Send + 'static>(this: &Arc<Mutex<T>>, ui: &mut egui::Ui) {
        let (rect, response) = ui.allocate_exact_size(ui.available_size(), egui::Sense::drag());
        {
            let mut s = this.lock().unwrap();
            let camera = &mut s.base().camera.base;
            if response.hovered() != camera.mouseOverRenderArea { camera.event(if response.hovered() { EventType::MouseEnter } else { EventType::MouseLeave }, None, None); }
        }
        let this = this.clone();
        // clipped to the control, so its glClear stays inside it as on WPF's own surface
        ui.painter_at(rect).add(egui::PaintCallback { rect, callback: Arc::new(egui_glow::CallbackFn::new(move |info, painter| {
            setGL(painter.gl().clone());
            let mut s = this.lock().unwrap();
            if !s.base().initialized { s.base().initialized = true; s.base().onInitialized(); }
            let delta = s.base().elapsed.replace(Instant::now()).map_or(0., |z| z.elapsed().as_millis() as f32);
            let vp = info.viewport_in_pixels();
            Self::_render(&mut *s, delta, (vp.left_px, vp.from_bottom_px, vp.width_px, vp.height_px));
        })) });
        ui.ctx().request_repaint();
    }

    fn _render<T: GLWpfControlX + ?Sized>(s: &mut T, delta: f32, (x, y, width, height): (i32, i32, i32, i32)) {
        //if Visibility != Visibility.Visible return;
        s.tick(delta);
        //if ViewportChanged
        s.setViewport(x, y, width, height);
        s.render(delta);
    }

    /// `GLCamera::setViewport` ends in its `GfxViewport`, as C#'s `Camera.SetViewport` does.
    pub fn setViewport(&mut self, x: i32, y: i32, width: i32, height: i32) { self.camera.base.setViewport(x, y, width, height); self.viewportChanged = false; }

    pub fn tick(&mut self, deltaTime: f32) {
        //let mouseState = ...; let keyboardState = ...;
        self.camera.tick(deltaTime);
        //self.camera.handleInput(mouseState, keyboardState);
        //self.handleInput(mouseState, keyboardState);
    }
}

/// C# `GLWpfControl`'s virtual members, which a subclass overrides.
pub trait GLWpfControlX {
    fn base(&mut self) -> &mut GLWpfControl;

    fn setViewport(&mut self, x: i32, y: i32, width: i32, height: i32) { self.base().setViewport(x, y, width, height) }

    fn tick(&mut self, deltaTime: f32) { self.base().tick(deltaTime) }

    fn render(&mut self, deltaTime: f32) {
        unsafe {
            let gl = GL();
            gl.clear_color(0.2, 0.3, 0.3, 1.); // GL.ClearColor(Color4.Blue);
            gl.clear(glow::COLOR_BUFFER_BIT | glow::DEPTH_BUFFER_BIT);
        }
        let camera = self.base().camera.base.base.clone();
        self.renderCamera(&camera, deltaTime);
    }

    /// C# `Render(Camera camera, float deltaTime)`.
    fn renderCamera(&mut self, camera: &Camera, deltaTime: f32) {}
}
