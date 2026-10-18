//! `gamex-origin-uo` — thin asset slot for the ClassicUO Ultima Online shell.
//! Not the process entry. C# `UOGameClient.LoadContent` is the bootstrap.
//! Resource parsers stay in `gamex-origin`. Renderer home later is GameX.Engine.Mg
//! (OpenX engine code MG), not FNA copied out of ClassicUO.
//!
//! WORKSPACE: not added yet

pub mod client;
