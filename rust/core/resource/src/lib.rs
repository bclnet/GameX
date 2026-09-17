//! `gamex-resource` — 1:1 port of .NET project `GameX.Resource`.
//!
//! Module layout mirrors the C# folder/file layout exactly so the two trees
//! can be diffed and updated in parallel. See PORT_MAP.tsv and PORTING.md.

pub mod algorithms;
pub mod bioware;
pub mod bullfrog;
pub mod capcom;
pub mod core;
pub mod crytek;
pub mod red;
