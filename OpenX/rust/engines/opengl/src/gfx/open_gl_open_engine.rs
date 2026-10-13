// PORT-SOURCE: Engines/OpenX.Engine.OpenGL/Gfx/OpenGLOpenEngine.cs
// PORT-SHA: 09f8f2907f4bee3a
// PORT-STATUS: todo
//
// Not ported yet: `OpenGLOpenEngine` drives OpenX's `CellManager` through an
// `AsyncCoroutineQueue`, neither of which is in the Rust port. `EngineRenderer`
// (gfx/open_gl_render.rs) is waiting on it.
