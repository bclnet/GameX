// PORT-SOURCE: Engines/OpenX.Engine.Unreal/Engine_Unreal.cs
// PORT-SHA: 0b05e5232d116d80
// PORT-STATUS: partial
// PARTIAL PORT: `unreal_shim` links native/unreal_shim.cpp. On this build `linked()` is false (stub, not an Unreal host).
//
// Ported: `UnrealEngine`'s registration and `UnrealShellEngine`: id, name and caps.
// Not ported: the rest of the file, the Unreal backend. So `GfxFactory`
// (C#: `[new UnrealGfxApi(), null, new UnrealGfxSprite3D(), new UnrealGfxModel(), null, null]`) and `SfxFactory` (C#: `[new UnrealSfx()]`) are left at
// `Engine`'s default, no factory, until those types exist in Rust.

use std::sync::LazyLock;
use openx::engine::{Engine, EngineX};

/// UnrealEngine
pub struct UnrealEngine;

impl UnrealEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::Drawing, ..Engine::new("UR", "Unreal") });
        &This
    }
}

/// UnrealShellEngine
pub struct UnrealShellEngine;

impl UnrealShellEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine::new("UR", "Unreal"));
        &This
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn registrationMatchesTheCSharp() {
        let s = UnrealEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("UR", "Unreal", EngineX::Caps::Drawing));
    }
}
