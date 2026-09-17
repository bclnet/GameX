//! `gamex-iw` — 1:1 port of .NET project `GameX.IW`.
//!
//! Module layout mirrors the C# folder/file layout exactly so the two trees
//! can be diffed and updated in parallel. See PORT_MAP.tsv and PORTING.md.

pub mod formats;
pub mod iw;
pub mod zone;
