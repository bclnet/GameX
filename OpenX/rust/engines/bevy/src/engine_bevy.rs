// Bevy is Rust-only. There is no C# or Python project, and no `BV` id there.
// PORT-STATUS: partial
//
// Registered: id `BV`, name `Bevy`, caps Drawing, plus `BevyShellEngine`.
// Not ported: a Bevy app. The workspace rust-version is 1.75, and current Bevy
// needs a newer compiler, so this crate does not depend on `bevy`.

use std::sync::LazyLock;
use openx::engine::{Engine, EngineX};

/// BevyEngine
pub struct BevyEngine;

impl BevyEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::Drawing, ..Engine::new("BV", "Bevy") });
        &This
    }
}

/// BevyShellEngine
pub struct BevyShellEngine;

impl BevyShellEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine::new("BV", "Bevy"));
        &This
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn registrationIsBevy() {
        let s = BevyEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("BV", "Bevy", EngineX::Caps::Drawing));
    }
}
