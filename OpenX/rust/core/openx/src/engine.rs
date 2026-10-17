// PORT-SOURCE: Core/OpenX/Engine.cs
// PORT-SHA: d2e3f8af10f10234
// PORT-STATUS: done
//
// C#'s `abstract class Engine` is a struct: its subclasses only set fields in
// their constructors, so each is a function returning its one `&'static Engine`
// (`UnknownEngine::This()`, `OpenGLEngine::This()`). Engines compare by
// address, as C# compares references. `EngineX`, a static class, is a module;
// its mutable statics sit behind one lock.
//
// `Engine.Activate` sets `Log.Func = LogFunc`. `Log` starts as stderr, which is
// also the default `logFunc`, so a log before activate still prints. `InTestHost` sniffs
// for a `testhost` assembly in C#; Rust has no test host to find, so it is
// `cfg!(test)`, true only in this crate's own tests.
//
// Kept as C# has it: `Epsilon` loops one halving too far, so it is half of
// machine epsilon (2^-24, not `f32::EPSILON`'s 2^-23).

use std::sync::{Arc, LazyLock, Mutex, MutexGuard};
use openx_gfx::gfx::IOpenGfxs;
use openx_sfx::sfx::IOpenSfxs;

// #region Engine

/// Gets the engine.
pub struct Engine {
    /// Gets the engine enabled.
    pub enabled: bool,
    /// Gets the engine caps
    pub caps: EngineX::Caps,
    /// Gets the engine id.
    pub id: &'static str,
    /// Gets the engine name.
    pub name: &'static str,
    /// Gets the engine tag.
    pub tag: Option<String>,
    /// Gets the engine gfx factory.
    pub gfxFactory: fn() -> Option<IOpenGfxs>,
    /// Gets the engine sfx factory.
    pub sfxFactory: fn() -> Option<IOpenSfxs>,
    /// Gets the engine log func.
    pub logFunc: fn(&str),
}

impl Engine {
    pub fn new(id: &'static str, name: &'static str) -> Self {
        Self { enabled: true, caps: EngineX::Caps::None_, id, name, tag: None, gfxFactory: || None, sfxFactory: || None, logFunc: |a| eprintln!("{a}") }
    }

    /// Gets the engine name.
    pub fn displayName(&self) -> &str { self.name }

    /// Activates the engine.
    pub fn activate(&self) {
        openx_poly::log::Log::setFunc(self.logFunc);
    }

    /// Deactivates the engine.
    pub fn deactivate(&self) {}
}

impl std::fmt::Debug for Engine {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result { write!(f, "Engine({})", self.id) }
}

/// EngineX
pub mod EngineX {
    use super::*;
    use crate::engine_test::platform_test::TestEngine;
    use crate::engine_unknown::platform_unknown::UnknownEngine;
    use crate::util::{decodePath, YamlDict};

    pub static Epsilon: LazyLock<f32> = LazyLock::new(getEpsilon);
    fn getEpsilon() -> f32 { let (mut epsilon, mut comparison) = (1f32, 0f32); loop { epsilon *= 0.5; comparison = 1.0 + epsilon; if !(comparison > 1.0) { break; } } epsilon }
    //pub static Hook: ...

    bitflags::bitflags! {
        /// The platform Caps.
        #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
        pub struct Caps: i32 {
            const None_ = 0x0;
            const ReadDds = 0x1;
            const Drawing = 0x2;
        }
    }

    /// The platform OS.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub enum OS { #[default] Unknown, Windows, OSX, Linux, Android }

    /// Gets the platform os.
    pub const PlatformOS: OS = if cfg!(target_os = "windows") { OS::Windows }
        else if cfg!(target_os = "macos") { OS::OSX }
        else if cfg!(target_os = "android") { OS::Android }
        else if cfg!(target_os = "linux") { OS::Linux }
        else { OS::Unknown };

    /// Determines if in a test host.
    pub const InTestHost: bool = cfg!(test);

    /// Gets the application Path
    pub static ApplicationPath: LazyLock<String> = LazyLock::new(|| std::env::current_exe().ok().and_then(|s| s.parent().map(|s| s.to_string_lossy().into_owned())).unwrap_or_default());

    /// Gets the platform startups.
    pub static Options: LazyLock<Mutex<YamlDict>> = LazyLock::new(|| Mutex::new(YamlDict::new("~/.gamex.yaml")));

    /// `Engines`, `Current`, `Gfx` and `Sfx`.
    struct State {
        engines: Vec<&'static Engine>,
        current: Option<&'static Engine>,
        gfx: Option<Arc<IOpenGfxs>>,
        sfx: Option<Arc<IOpenSfxs>>,
    }

    fn state() -> MutexGuard<'static, State> {
        static S: LazyLock<Mutex<State>> = LazyLock::new(|| {
            let mut s = State { engines: vec![UnknownEngine::This()], current: None, gfx: None, sfx: None };
            activate(&mut s, Some(if InTestHost { TestEngine::This() } else { UnknownEngine::This() }));
            Mutex::new(s)
        });
        S.lock().unwrap_or_else(|p| p.into_inner())
    }

    fn activate(s: &mut State, engine: Option<&'static Engine>) -> &'static Engine {
        //Hook?.Invoke(); Hook = null;
        let engine = match engine { Some(z) if z.enabled => z, _ => UnknownEngine::This() };
        if !s.engines.iter().any(|z| std::ptr::eq(*z, engine)) { s.engines.push(engine); }
        let current = s.current;
        if !current.is_some_and(|z| std::ptr::eq(z, engine)) {
            if let Some(z) = current { z.deactivate(); }
            engine.activate();
            s.gfx = (engine.gfxFactory)().map(Arc::new);
            s.sfx = (engine.sfxFactory)().map(Arc::new);
            s.current = Some(engine);
        }
        engine
    }

    /// Gets the platform startups.
    pub fn Engines() -> Vec<&'static Engine> { state().engines.clone() }

    /// C# `Engines.UnionWith(engines)`: adds the ones not there yet.
    pub fn EnginesUnionWith(engines: &[&'static Engine]) {
        let mut s = state();
        for &e in engines { if !s.engines.iter().any(|z| std::ptr::eq(*z, e)) { s.engines.push(e); } }
    }

    /// Gets or sets the current platform.
    pub fn Current() -> &'static Engine { state().current.unwrap() }

    /// Activates an engine.
    pub fn Activate(engine: Option<&'static Engine>) -> &'static Engine { activate(&mut state(), engine) }

    /// Gets the gfx.
    pub fn Gfx() -> Option<Arc<IOpenGfxs>> { state().gfx.clone() }

    /// Gets the sfx.
    pub fn Sfx() -> Option<Arc<IOpenSfxs>> { state().sfx.clone() }

    pub fn DecodePath(path: &str, rootPath: Option<&str>) -> String { decodePath(&ApplicationPath, path, rootPath) }
}

// #endregion

#[cfg(test)]
mod tests {
    use super::*;
    use super::EngineX::{Caps, OS};
    use crate::engine_unknown::platform_unknown::UnknownEngine;

    static Disabled: LazyLock<Engine> = LazyLock::new(|| Engine { enabled: false, ..Engine::new("DISABLED", "Disabled") });
    static Gl: LazyLock<Engine> = LazyLock::new(|| Engine { caps: Caps::ReadDds, gfxFactory: || Some(vec![None; 6]), ..Engine::new("GL", "Gl") });

    #[test]
    fn epsilonIsHalfOfMachineEpsilonAsInTheCSharp() {
        assert_eq!(*EngineX::Epsilon, f32::EPSILON / 2.0);
        assert_eq!(1.0f32 + *EngineX::Epsilon, 1.0f32);
    }

    #[test]
    fn disabledEnginesFallBackToUnknownAndActivateSwitches() {
        assert!(std::ptr::eq(EngineX::Activate(Some(&Disabled)), UnknownEngine::This()));
        EngineX::Activate(Some(&Gl));
        assert_eq!(EngineX::Current().id, "GL");
        assert!(EngineX::Engines().iter().any(|s| s.id == "GL"));
        assert!(EngineX::Current().caps.contains(Caps::ReadDds));
        assert_eq!(EngineX::Gfx().map(|s| s.len()), Some(6));
        EngineX::Activate(Some(UnknownEngine::This())); // restore for other tests
    }

    #[test]
    fn capsComposeAndTest() {
        let c = Caps::ReadDds | Caps::Drawing;
        assert!(c.contains(Caps::ReadDds) && c.contains(Caps::Drawing));
        assert!(!Caps::ReadDds.contains(Caps::Drawing));
    }

    #[test]
    fn osDetectionReturnsSomethingSane() {
        assert!(matches!(EngineX::PlatformOS, OS::Windows | OS::OSX | OS::Linux | OS::Android | OS::Unknown));
    }
}
