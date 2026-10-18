//! `openx-engine-unreal` — 1:1 port of its .NET project.
//!
//! The engine **registration** is translated: id, name and caps (`engine_*.rs`).
//!
//! The **render** half is not: see the individual modules for why, and
//! PORTING.md for the per-backend viability assessment.
//!
//! `unreal_shim` links a C++ stub. It does not host Unreal Engine.

pub mod engine_unreal;
pub mod gfx;
pub mod unreal_shim;
