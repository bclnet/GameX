// PORT-SOURCE: Engines/OpenX.Engine.Wgpu/Engine_Wgpu.cs
// PORT-STATUS: partial
//
// Ported: `WgpuEngine` registration (id, name, caps), `createInstance`,
// and `WgpuShellEngine`.
// Not ported: `WgpuClientHost`, `WgpuGfxSprite3D`, `WgpuGfxModel`, `WgpuSfx`,
// `GfxFactory`, and `SfxFactory`. Those stay at `Engine`'s default.

use std::sync::LazyLock;
use openx::engine::{Engine, EngineX};
use crate::gfx::wgpu::WgpuInstance;

/// WgpuEngine
pub struct WgpuEngine;

impl WgpuEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::Drawing, ..Engine::new("WG", "Wgpu") });
        &This
    }

    /// Creates a wgpu instance and requests an adapter. No window and no surface.
    /// `Err` when no adapter is found. Does not panic.
    pub fn createInstance() -> Result<WgpuInstance, String> { WgpuInstance::createInstance() }
}

/// WgpuShellEngine
pub struct WgpuShellEngine;

impl WgpuShellEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine::new("WG", "Wgpu"));
        &This
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn registrationMatchesTheCSharp() {
        let s = WgpuEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("WG", "Wgpu", EngineX::Caps::Drawing));
        let shell = WgpuShellEngine::This();
        assert_eq!((shell.id, shell.name), ("WG", "Wgpu"));
    }

    #[test]
    fn createInstanceDoesNotPanic() {
        match WgpuEngine::createInstance() {
            Ok(instance) => drop(instance),
            Err(err) => assert!(!err.is_empty()),
        }
    }
}
