//! `gamex-crytek` — 1:1 port of .NET project `GameX.Crytek`.
//!
//! Module layout mirrors the C# folder/file layout exactly so the two trees
//! can be diffed and updated in parallel. See PORT_MAP.tsv and PORTING.md.

pub mod crytek;
pub mod formats;
pub mod polyfills;
pub mod transforms;
