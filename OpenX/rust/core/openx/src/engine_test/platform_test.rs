// PORT-SOURCE: Core/OpenX/Engine_Test/Platform_Test.cs
// PORT-SHA: fe0749a90d3e5f8a
// PORT-STATUS: done
//
// Every member of `TestClientHost`, `TestGfxApi`, `TestGfxSprite`,
// `TestGfxModel`, `TestGfxLight` and `TestGfxTerrain` throws in C#
// (`NotImplementedException` / `NotSupportedException`). So here they are the
// types alone, filling `TestEngine`'s slots: implementing openx-gfx's traits
// would only add backend type parameters to members that can't be called.
// `TestClientHost.Run` panics with `unimplemented!`.

use std::sync::{Arc, LazyLock};
use openx_gfx::gfx::IOpenGfx;
use openx_sfx::sfx::IOpenSfx;
use crate::engine::Engine;

// #region Client

pub struct TestClientHost;

impl TestClientHost {
    pub fn run(&self) { unimplemented!() }
}

// #endregion

// #region Platform

pub struct TestGfxApi;
impl IOpenGfx for TestGfxApi {}

pub struct TestGfxSprite;
impl IOpenGfx for TestGfxSprite {}

pub struct TestGfxModel;
impl IOpenGfx for TestGfxModel {}

pub struct TestGfxLight;
impl IOpenGfx for TestGfxLight {}

pub struct TestGfxTerrain;
impl IOpenGfx for TestGfxTerrain {}

pub struct TestSfx;
impl IOpenSfx for TestSfx {}

/// TestPlatform
pub struct TestEngine;

impl TestEngine {
    //public static Dictionary<Type, Func<object, bool, object, object>> BuildersByType = [];
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine {
            // as C#: two sprite slots, no light, and terrain in the light slot
            gfxFactory: || Some(vec![Some(Arc::new(TestGfxApi)), Some(Arc::new(TestGfxSprite)), Some(Arc::new(TestGfxSprite)), Some(Arc::new(TestGfxModel)), Some(Arc::new(TestGfxTerrain))]),
            sfxFactory: || Some(vec![Some(Arc::new(TestSfx))]),
            ..Engine::new("TT", "Test")
        });
        &This
    }
}

// #endregion
