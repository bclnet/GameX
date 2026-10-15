// PORT-SOURCE: Engines/OpenX.Engine.Unity/Engine_Unity.cs
// PORT-SHA: 252a8d9738f4f2dd
// PORT-STATUS: partial
//
// Ported: `UnityXEngine`'s registration and `UnityShellEngine`: id, name and caps.
// Not ported: the rest of the file, the Unity backend. So `GfxFactory`
// (C#: `[new UnityGfxApi(), new UnityGfxSprite2D(), new UnityGfxSprite3D(), new UnityGfxModel(), new UnityGfxLight(), new UnityGfxTerrain()]`) and `SfxFactory` (C#: `[new UnitySfx()]`) are left at
// `Engine`'s default, no factory, until those types exist in Rust.

use std::sync::LazyLock;
use openx::engine::{Engine, EngineX};

/// UnityXEngine
pub struct UnityXEngine;

impl UnityXEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::None_, ..Engine::new("UN", "Unity") });
        &This
    }
}

/// UnityShellEngine
pub struct UnityShellEngine;

impl UnityShellEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine::new("UN", "Unity"));
        &This
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn registrationMatchesTheCSharp() {
        let s = UnityXEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("UN", "Unity", EngineX::Caps::None_));
    }
}
