// PORT-SOURCE: Engines/OpenX.Engine.O3de/Engine_O3de.cs
// PORT-SHA: 0b8be07a40d2dacd
// PORT-STATUS: partial
//
// Ported: `O3deEngine`'s registration and `O3deShellEngine`: id, name and caps.
// Not ported: the rest of the file, the O3de backend. So `GfxFactory`
// (C#: `[null, null, new O3deGfxSprite3D(), new O3deGfxModel(), null, null]`) and `SfxFactory` (C#: `[new O3deSfx()]`) are left at
// `Engine`'s default, no factory, until those types exist in Rust.

use std::sync::LazyLock;
use openx::engine::{Engine, EngineX};

/// O3deEngine
pub struct O3deEngine;

impl O3deEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::Drawing, ..Engine::new("O3", "O3de") });
        &This
    }
}

/// O3deShellEngine
pub struct O3deShellEngine;

impl O3deShellEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine::new("O3", "O3de"));
        &This
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn registrationMatchesTheCSharp() {
        let s = O3deEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("O3", "O3de", EngineX::Caps::Drawing));
    }
}
