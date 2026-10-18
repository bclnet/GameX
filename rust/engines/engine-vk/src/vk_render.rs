// PORT-SOURCE: Engines/GameX.Engine.Vk/Vk_Render.cs
// PORT-STATUS: partial
//
// Ported: `VkRenderer.createRenderer` (TestTri, Texture, default).
// Not ported: `ViewInfo`. `obj is IHaveSource` has no Rust form yet.
// Renderer bodies do not call `VkGfxModel` (not ported).

use openx_engine_vk::gfx::vk_render::{TestTriRenderer, TextureRenderer};

/// VkRenderer
pub struct VkRenderer;

impl VkRenderer {
    pub fn createRenderer(type_: &str) -> Option<VkBuiltRenderer> {
        match type_ {
            "TestTri" => Some(VkBuiltRenderer::TestTri(TestTriRenderer::new())),
            "Texture" => Some(VkBuiltRenderer::Texture(TextureRenderer::new())),
            _ => None,
        }
    }
}

/// Nearest equivalent of C# `Renderer` for the two Vulkan cases.
pub enum VkBuiltRenderer {
    TestTri(TestTriRenderer),
    Texture(TextureRenderer),
}
