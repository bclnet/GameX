// PORT-SOURCE: Engines/OpenX.Engine.Diligent/Engine_Diligent.cs
// PORT-STATUS: partial
//
// Ported: `DiligentEngine` registration (id, name, caps) and `DiligentShellEngine`.
// `diligent_shim` links native/diligent_shim.cpp. On this build `linked()` is false
// and `version()` is "not linked".
// Not ported: host, sprite, model, and sfx. Factories stay at `Engine`'s default.

use std::sync::LazyLock;
use openx::engine::{Engine, EngineX};

/// DiligentEngine
pub struct DiligentEngine;

impl DiligentEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::Drawing, ..Engine::new("DN", "Diligent") });
        &This
    }
}

/// DiligentShellEngine
pub struct DiligentShellEngine;

impl DiligentShellEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine::new("DN", "Diligent"));
        &This
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn registrationMatchesTheCSharp() {
        let s = DiligentEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("DN", "Diligent", EngineX::Caps::Drawing));
    }
}
