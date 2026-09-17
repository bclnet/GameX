//! `gamex-origin` — 1:1 port of .NET project `GameX.Origin`.
//!
//! Module layout mirrors the C# folder/file layout exactly so the two trees
//! can be diffed and updated in parallel. See PORT_MAP.tsv and PORTING.md.

pub mod clients;
pub mod formats;
pub mod origin;
pub mod renderers;
pub mod structs;
