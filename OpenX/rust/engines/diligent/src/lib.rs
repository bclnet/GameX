//! `openx-engine-diligent` — 1:1 port of its .NET project.
//!
//! The engine **registration** is translated: id, name and caps (`engine_*.rs`).
//!
//! `diligent_shim` links a C++ stub. It does not host Diligent Engine.
//! On this build `linked` is false and `version` is "not linked".

pub mod engine_diligent;
pub mod gfx;
pub mod diligent_shim;
