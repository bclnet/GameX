//! `gamex-engine-opengl` — 1:1 port of .NET project `GameX.Engine.OpenGL`.
//!
//! Module layout mirrors the C# folder/file layout exactly so the two trees
//! can be diffed and updated in parallel. See PORT_MAP.tsv and PORTING.md.

pub mod open_gl_arkane;
pub mod open_gl_gamebryo;
pub mod open_gl_render;
