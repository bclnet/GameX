//! `openx-engine-godot` — 1:1 port of its .NET project.
//!
//! The engine **registration** is translated: id, name and caps (`engine_*.rs`).
//!
//! The **render** half is not: see the individual modules for why, and
//! PORTING.md for the per-backend viability assessment.

pub mod engine_godot;
pub mod gfx;
