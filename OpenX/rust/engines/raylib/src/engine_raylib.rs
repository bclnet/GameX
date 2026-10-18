// PORT-SOURCE: Engines/OpenX.Engine.Raylib/Engine_Raylib.cs
// PORT-STATUS: partial
//
// Ported: `RaylibEngine` registration (id, name, caps) and `RaylibShellEngine`.
// Not ported: host, sprite, model, and sfx. Factories stay at `Engine`'s default.
// `RaylibEngine.Version` is not ported. The `raylib` crate 6.0.0 is edition 2024
// and rust-version 1.88. This workspace is edition 2021 and rust-version 1.75.

use std::sync::LazyLock;
use openx::engine::{Engine, EngineX};

/// RaylibEngine
pub struct RaylibEngine;

impl RaylibEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::Drawing, ..Engine::new("RL", "Raylib") });
        &This
    }
}

/// RaylibShellEngine
pub struct RaylibShellEngine;

impl RaylibShellEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine::new("RL", "Raylib"));
        &This
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn registrationMatchesTheCSharp() {
        let s = RaylibEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("RL", "Raylib", EngineX::Caps::Drawing));
    }
}
