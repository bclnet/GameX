// PORT-SOURCE: Engines/OpenX.Wpf.Control/VulkanControl.cs
// PORT-STATUS: partial
//
// Ported: source-change `Start` and `Tick` `Update`.
// Not ported: WPF dependency properties and `CompositionTarget.Rendering`.
// No surface: ADR 0009 leaves the C# loader until a window is written.
// eframe is OpenGL, so this control does not paint. `Select` stays with
// the caller; `id` starts at 0.

use openx_gfx::gfx_render::Renderer;

/// C# `abstract Renderer CreateRenderer()`.
pub type CreateRenderer = Box<dyn FnMut() -> Option<Box<dyn Renderer + Send>> + Send>;

/// VulkanControl
pub struct VulkanControl {
    id: i32,
    pub renderer: Option<Box<dyn Renderer + Send>>,
    createRenderer: CreateRenderer,
    sourceChanged: bool,
}

impl VulkanControl {
    pub fn new(createRenderer: CreateRenderer) -> Self {
        Self { id: 0, renderer: None, createRenderer, sourceChanged: true }
    }

    fn onSourceChanged(&mut self) {
        self.renderer = (self.createRenderer)();
        if let Some(z) = &mut self.renderer { z.start(); }
        let _ = self.id;
    }

    pub fn tick(&mut self, deltaTime: f32) {
        if self.sourceChanged { self.sourceChanged = false; self.onSourceChanged(); }
        if let Some(z) = &mut self.renderer { z.update(deltaTime); }
    }
}
