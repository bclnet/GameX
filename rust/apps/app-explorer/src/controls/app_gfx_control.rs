// PORT-SOURCE: Apps/GameX.App.Explorer/Controls/AppGfxControl.cs
// PORT-SHA: 8cdb9ab75bc3c650
// PORT-STATUS: done
//
// PARTIAL PORT: only `AppOpenGLControl`. The Godot, O3de, Ogre, Sdl, Stride,
// Unity and Unreal controls wait on their OpenX engines, none of which has a
// Rust host yet.
//
// C#'s `OpenGLRenderer.CreateRenderer(this, EngineX.Gfx, Source, Value, Type)`
// reads the control's bound properties; here they're the arguments, and
// `OpenGLEngine::gfx()` stands in for `EngineX.Gfx` (see OpenX's engine_open_gl.rs).
// C#'s `ShellState.Create` argument has no use in the OpenGL control.

use std::sync::{Arc, Mutex};
use gamex::meta::Tag;
use gamex_engine_opengl::open_gl_render::OpenGLRenderer;
use openx_engine_opengl::engine_open_gl::OpenGLEngine;
use openx_poly::core::SourceRef;
use openx_wpf_control::open_gl_control::OpenGLControl;

/// AppOpenGLControl
pub struct AppOpenGLControl;

impl AppOpenGLControl {
    pub fn new(source: SourceRef, value: Tag, type_: String) -> Arc<Mutex<OpenGLControl>> {
        OpenGLControl::new(Box::new(move || OpenGLRenderer::createRenderer(None, OpenGLEngine::gfx(), &source, &value, &type_)))
    }
}
