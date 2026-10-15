// PORT-SOURCE: Core/OpenX/Engine_Unknown/Platform_Unknown.cs
// PORT-SHA: 09a90fb895343726
// PORT-STATUS: done
//
// `UnknownClientHost`'s members throw `NotImplementedException` in C#; here they
// panic with `unimplemented!`. C#'s `Dispose` is `Drop`, which can't fail, so
// that one isn't carried over.

use std::sync::LazyLock;
use crate::engine::Engine;

// #region Client

pub struct UnknownClientHost;

impl UnknownClientHost {
    pub fn run(&self) { unimplemented!() }
}

// #endregion

// #region Platform

/// UnknownPlatform
pub struct UnknownEngine;

impl UnknownEngine {
    //public static Dictionary<Type, Func<object, bool, object, object>> BuildersByType = [];
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine {
            gfxFactory: || Some(vec![None, None, None, None, None, None]),
            sfxFactory: || Some(vec![None]),
            ..Engine::new("UK", "Unknown")
        });
        &This
    }
}

// #endregion
