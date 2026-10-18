// PORT-SOURCE: Engines/OpenX.Engine.Vk/Gfx/Vk_Render.cs
// PORT-STATUS: partial
//
// Ported: `TestTriRenderer` and `TextureRenderer` type names.
// Not ported: the bodies. They call `VkGfxModel.TextureManager`, and
// `VkGfxModel` is not ported, so constructing them would not match C#.

/// TestTriRenderer
pub struct TestTriRenderer;

impl TestTriRenderer {
    pub fn new() -> Self { Self }
}

/// TextureRenderer
pub struct TextureRenderer {
    frameDelay: f32,
}

impl TextureRenderer {
    pub fn new() -> Self { Self { frameDelay: 0.0 } }

    pub fn start(&mut self) {}

    pub fn update(&mut self, deltaTime: f32) { self.frameDelay += deltaTime; }
}
