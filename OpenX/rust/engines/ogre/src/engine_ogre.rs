// PORT-SOURCE: Engines/OpenX.Engine.Ogre/Engine_Ogre.cs
// PORT-SHA: 7cb3e0ef00720a6d
// PORT-STATUS: partial
//
// Ported: `OgreEngine`'s registration and `OgreShellEngine`: id, name and caps.
// Not ported: the rest of the file, the Ogre backend. So `GfxFactory`
// (C#: `[null, null, new OgreGfxSprite3D(), new OgreGfxModel(), null, null]`) and `SfxFactory` (C#: `[new OgreSfx()]`) are left at
// `Engine`'s default, no factory, until those types exist in Rust.

use std::sync::LazyLock;
use openx::engine::{Engine, EngineX};

/// OgreEngine
pub struct OgreEngine;

impl OgreEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::Drawing, ..Engine::new("OG", "Ogre") });
        &This
    }
}

/// OgreShellEngine
pub struct OgreShellEngine;

impl OgreShellEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine::new("OG", "Ogre"));
        &This
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn registrationMatchesTheCSharp() {
        let s = OgreEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("OG", "Ogre", EngineX::Caps::Drawing));
    }
}
