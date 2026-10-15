// PORT-SOURCE: Engines/OpenX.Engine.Stride/Engine_Stride.cs
// PORT-SHA: e6c2a42bd59f7534
// PORT-STATUS: partial
//
// Ported: `StrideEngine`'s registration: id, name and caps.
// Not ported: the rest of the file, the Stride backend. So `GfxFactory`
// (C#: `[null, null, new StrideGfxSprite3D(), new StrideGfxModel(), null, null]`) and `SfxFactory` (C#: `[new StrideSfx()]`) are left at
// `Engine`'s default, no factory, until those types exist in Rust.

use std::sync::LazyLock;
use openx::engine::{Engine, EngineX};

/// StrideEngine
pub struct StrideEngine;

impl StrideEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::Drawing, ..Engine::new("ST", "Stride") });
        &This
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn registrationMatchesTheCSharp() {
        let s = StrideEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("ST", "Stride", EngineX::Caps::Drawing));
    }
}
