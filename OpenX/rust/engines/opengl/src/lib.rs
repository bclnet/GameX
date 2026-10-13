//! `openx-engine-opengl` — 1:1 port of .NET project `OpenX.Engine.OpenGL`, on `glow`.
//!
//! The host (the egui GL control in `openx-wpf-control`) hands eframe's
//! `glow::Context` to `gfx::open_gl::setGL`; everything else draws through it.
//! See each file's header for what is and isn't ported.

pub mod egin;
pub mod engine_open_gl;
pub mod gfx;
