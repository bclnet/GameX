// PORT-SOURCE: Engines/OpenX.Engine.Godot/Engine_Godot.cs
// PORT-SHA: 0fff6dc8d2b111ed
// PORT-STATUS: partial
//
// Ported: `GodotEngine`'s registration and `GodotShellEngine`: id, name and caps.
// Not ported: the rest of the file, the Godot backend. So `GfxFactory`
// (C#: `[new GodotGfxApi(), new GodotGfxSprite2D(), new GodotGfxSprite3D(), new GodotGfxModel(), null, null]`) and `SfxFactory` (C#: `[new GodotSfx()]`) are left at
// `Engine`'s default, no factory, until those types exist in Rust.

use std::sync::LazyLock;
use openx::engine::{Engine, EngineX};

/// GodotEngine
pub struct GodotEngine;

impl GodotEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::Drawing, ..Engine::new("GD", "Godot") });
        &This
    }
}

/// GodotShellEngine
pub struct GodotShellEngine;

impl GodotShellEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine::new("GD", "Godot"));
        &This
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn registrationMatchesTheCSharp() {
        let s = GodotEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("GD", "Godot", EngineX::Caps::Drawing));
    }
}
