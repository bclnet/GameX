// PORT-SOURCE: Engines/OpenX.Engine.Vulkan/Engine_Vulkan.cs
// PORT-STATUS: partial
//
// Ported: `VulkanEngine` registration (id, name, caps) and `createInstance`,
// plus `VulkanShellEngine`.
// Not ported: `VulkanClientHost`, `VulkanGfxApi`, `VulkanGfxSprite3D`, `VulkanGfxModel`,
// `VulkanSfx`, `GfxFactory`, and `SfxFactory`. Those stay at `Engine`'s default.

use std::sync::LazyLock;
use openx::engine::{Engine, EngineX};
use crate::gfx::vulkan::VulkanInstance;

/// VulkanEngine
pub struct VulkanEngine;

impl VulkanEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::Drawing, ..Engine::new("VK", "Vulkan") });
        &This
    }

    /// Creates a Vulkan instance. No window and no swapchain.
    /// `Err` when the loader or an ICD is missing. Does not panic.
    pub fn createInstance() -> Result<VulkanInstance, String> { VulkanInstance::createInstance() }
}

/// VulkanShellEngine
pub struct VulkanShellEngine;

impl VulkanShellEngine {
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
        let s = VulkanEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("VK", "Vulkan", EngineX::Caps::Drawing));
        let shell = VulkanShellEngine::This();
        assert_eq!((shell.id, shell.name), ("VK", "Vulkan"));
    }

    #[test]
    fn createInstanceDoesNotPanic() {
        match VulkanEngine::createInstance() {
            Ok(instance) => drop(instance),
            Err(err) => assert!(!err.is_empty()),
        }
    }
}
