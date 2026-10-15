// PORT-SOURCE: Engines/OpenX.Engine.Mg/Engine_Mg.cs
// PORT-SHA: e7f90c00696ab7fd
// PORT-STATUS: partial
//
// Ported: `MgEngine`'s registration: id, name and caps.
// Not ported: the rest of the file, the Mg backend. So `GfxFactory`
// (C#: `[null, new MgGfxSprite2D(), null, null, null, null]`) and `SfxFactory` (C#: `[new MgSfx()]`) are left at
// `Engine`'s default, no factory, until those types exist in Rust.

use std::sync::LazyLock;
use openx::engine::{Engine, EngineX};

/// MgEngine
pub struct MgEngine;

impl MgEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::Drawing, ..Engine::new("MG", "MonoGame") });
        &This
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn registrationMatchesTheCSharp() {
        let s = MgEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("MG", "MonoGame", EngineX::Caps::Drawing));
    }
}
