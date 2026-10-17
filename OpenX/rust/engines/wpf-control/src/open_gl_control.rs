// PORT-SOURCE: Engines/OpenX.Wpf.Control/OpenGLControl.cs
// PORT-SHA: 0d0b23538eca4996
// PORT-STATUS: done
//
// The WPF dependency properties (`Source`, `Source2`, `Path`, `Value`, `Type`)
// are what C#'s abstract `CreateRenderer` reads; a subclass passes them in its
// `createRenderer` closure instead, as `AppOpenGLControl` does. C# creates the
// renderer when a property changes; a GL call needs eframe's context, so here
// `onSourceChanged` runs on the first frame drawn. `Value is ITextureSelect`
// can't be asked of the closure's value; `TextureRenderer` calls `select(0)`
// itself, which is the only live call (`Id` starts at 0). `HandleInput` is
// commented out in the C#, as here.

use std::io::Error;
use std::sync::{Arc, Mutex};
use openx_gfx_egin::egin_render::{Camera, EginRenderer};
use crate::ui::gl_wpf_control::{GLWpfControl, GLWpfControlX};

/// C# `abstract Renderer CreateRenderer()`.
pub type CreateRenderer = Box<dyn FnMut() -> Result<Option<Box<dyn EginRenderer + Send>>, Error> + Send>;

/// OpenGLControl
pub struct OpenGLControl {
    pub base: GLWpfControl,
    id: i32,
    // Binding
    pub renderer: Option<Box<dyn EginRenderer + Send>>,
    createRenderer: CreateRenderer,
    sourceChanged: bool,
}

impl OpenGLControl {
    pub fn new(createRenderer: CreateRenderer) -> Arc<Mutex<Self>> {
        Arc::new(Mutex::new(Self { base: GLWpfControl::new(), id: 0, renderer: None, createRenderer, sourceChanged: true }))
    }

    fn onSourceChanged(&mut self) {
        self.renderer = match (self.createRenderer)() {
            Ok(z) => z,
            Err(e) => { eprintln!("ERROR: {e}"); None }
        };
        if let Some(z) = &mut self.renderer { z.start(); }
        //if (Value is ITextureSelect z2) z2.Select(Id);
        //Camera.SetLocation(new Vector3(200));
        //Camera.LookAt(new Vector3(0));
    }

    pub fn ui(this: &Arc<Mutex<Self>>, ui: &mut egui::Ui) { GLWpfControl::ui(this, ui) }
}

impl GLWpfControlX for OpenGLControl {
    fn base(&mut self) -> &mut GLWpfControl { &mut self.base }

    fn setViewport(&mut self, x: i32, y: i32, width: i32, height: i32) {
        let p = self.renderer.as_ref().and_then(|s| s.getViewport((width, height))).unwrap_or((width, height));
        self.base.setViewport(x, y, p.0, p.1);
    }

    fn renderCamera(&mut self, camera: &Camera, frameTime: f32) {
        if let Some(z) = &mut self.renderer { z.render(camera, Default::default()); }
    }

    fn tick(&mut self, deltaTime: f32) {
        if self.sourceChanged { self.sourceChanged = false; self.onSourceChanged(); }
        self.base.tick(deltaTime);
        if let Some(z) = &mut self.renderer { z.update(deltaTime); }
        let camera = self.base.camera.base.base.clone();
        self.renderCamera(&camera, 0.);
    }
}
