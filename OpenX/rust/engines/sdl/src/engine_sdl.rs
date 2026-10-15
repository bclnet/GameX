// PORT-SOURCE: Engines/OpenX.Engine.Sdl/Engine_Sdl.cs
// PORT-SHA: 1cad4d5840d53621
// PORT-STATUS: partial
//
// Ported: `SdlEngine`'s registration: id, name and caps.
// Not ported: the rest of the file, the Sdl backend. So `GfxFactory`
// (C#: `[new SdlGfxSprite2D(), null, null, null, null, null]`) and `SfxFactory` (C#: `[new SdlSfx()]`) are left at
// `Engine`'s default, no factory, until those types exist in Rust.

use std::sync::LazyLock;
use openx::engine::{Engine, EngineX};

/// SdlEngine
pub struct SdlEngine;

impl SdlEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::Drawing, ..Engine::new("SD", "SDL 3") });
        &This
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn registrationMatchesTheCSharp() {
        let s = SdlEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("SD", "SDL 3", EngineX::Caps::Drawing));
    }
}
