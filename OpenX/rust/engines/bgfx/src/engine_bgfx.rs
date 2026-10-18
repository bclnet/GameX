// PORT-SOURCE: Engines/OpenX.Engine.Bgfx/Engine_Bgfx.cs
// PORT-STATUS: partial
//
// Ported: `BgfxEngine` registration (id, name, caps) and `BgfxShellEngine`.
// Not ported: host, sprite, model, and sfx. Factories stay at `Engine`'s default.

use std::sync::LazyLock;
use openx::engine::{Engine, EngineX};

/// BgfxEngine
pub struct BgfxEngine;

impl BgfxEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::Drawing, ..Engine::new("BX", "Bgfx") });
        &This
    }
}

/// BgfxShellEngine
pub struct BgfxShellEngine;

impl BgfxShellEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine::new("BX", "Bgfx"));
        &This
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn registrationMatchesTheCSharp() {
        let s = BgfxEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("BX", "Bgfx", EngineX::Caps::Drawing));
    }
}
