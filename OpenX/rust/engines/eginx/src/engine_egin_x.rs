// PORT-SOURCE: Engines/OpenX.Engine.EginX/Engine_EginX.cs
// PORT-SHA: 2cc4d7dab4ed765c
// PORT-STATUS: partial
//
// Ported: `EginXEngine`'s registration: id, name and caps.
// Not ported: the rest of the file, the EginX backend. So `GfxFactory`
// (C#: `[null, new EginXGfxSprite2D(), null, null, null, null]`) and `SfxFactory` (C#: `[new SystemSfx()]`) are left at
// `Engine`'s default, no factory, until those types exist in Rust.

use std::sync::LazyLock;
use openx::engine::{Engine, EngineX};

/// EginXEngine
pub struct EginXEngine;

impl EginXEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::Drawing, ..Engine::new("EX", "EginX") });
        &This
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn registrationMatchesTheCSharp() {
        let s = EginXEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("EX", "EginX", EngineX::Caps::Drawing));
    }
}
