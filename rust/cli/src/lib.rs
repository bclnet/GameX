//! `gamex-app-cli` — 1:1 port of .NET project `GameX.App.Cli`.
//!
//! Module layout mirrors the C# folder/file layout exactly so the two trees
//! can be diffed and updated in parallel. See PORT_MAP.tsv and PORTING.md.

pub mod program;
pub mod program_get;
pub mod program_list;
pub mod program_set;
pub mod program_test;
