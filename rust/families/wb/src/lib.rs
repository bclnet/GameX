//! `gamex-wb` — 1:1 port of .NET project `GameX.WB`.
//!
//! Module layout mirrors the C# folder/file layout exactly so the two trees
//! can be diffed and updated in parallel. See PORT_MAP.tsv and PORTING.md.

pub mod builders;
pub mod data;
pub mod formats;
pub mod games;
pub mod wb;
