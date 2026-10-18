// PORT-SOURCE: Engines/OpenX.Engine.Vk/Engine_Vk.cs
// PORT-STATUS: partial
//
// Ported: `VkEngine` registration (id, name, caps) and `createInstance`,
// plus `VkShellEngine`.
// Not ported: `VkClientHost`, `VkGfxApi`, `VkGfxSprite3D`, `VkGfxModel`,
// `VkSfx`, `GfxFactory`, and `SfxFactory`. Those stay at `Engine`'s default.

use std::sync::LazyLock;
use openx::engine::{Engine, EngineX};
use crate::gfx::vk::VkInstance;

/// VkEngine
pub struct VkEngine;

impl VkEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::Drawing, ..Engine::new("VK", "Vulkan") });
        &This
    }

    /// Creates a Vulkan instance. No window and no swapchain.
    /// `Err` when the loader or an ICD is missing. Does not panic.
    pub fn createInstance() -> Result<VkInstance, String> { VkInstance::createInstance() }
}

/// VkShellEngine
pub struct VkShellEngine;

impl VkShellEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine::new("VK", "Vulkan"));
        &This
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn registrationMatchesTheCSharp() {
        let s = VkEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("VK", "Vulkan", EngineX::Caps::Drawing));
        let shell = VkShellEngine::This();
        assert_eq!((shell.id, shell.name), ("VK", "Vulkan"));
    }

    #[test]
    fn createInstanceDoesNotPanic() {
        match VkEngine::createInstance() {
            Ok(instance) => drop(instance),
            Err(err) => assert!(!err.is_empty()),
        }
    }
}
