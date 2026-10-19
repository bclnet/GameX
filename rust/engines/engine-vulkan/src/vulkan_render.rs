// PORT-SOURCE: Engines/GameX.Engine.Vulkan/Vulkan_Render.cs
// PORT-STATUS: partial
//
// Ported: `VulkanRenderer.createRenderer` (TestTri, Texture, default).
// Not ported: `ViewInfo`. `obj is IHaveSource` has no Rust form yet.
// Renderer bodies do not call `VulkanGfxModel` (not ported).

use openx_engine_vulkan::gfx::vulkan_render::{TestTriRenderer, TextureRenderer};

/// VulkanRenderer
pub struct VulkanRenderer;

impl VulkanRenderer {
    pub fn createRenderer(type_: &str) -> Option<VulkanBuiltRenderer> {
        match type_ {
            "TestTri" => Some(VulkanBuiltRenderer::TestTri(TestTriRenderer::new())),
            "Texture" => Some(VulkanBuiltRenderer::Texture(TextureRenderer::new())),
            _ => None,
        }
    }
}

/// Nearest equivalent of C# `Renderer` for the two Vulkan cases.
pub enum VulkanBuiltRenderer {
    TestTri(TestTriRenderer),
    Texture(TextureRenderer),
}
