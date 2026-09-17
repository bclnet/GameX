//! `gamex-app-explorer` — 1:1 port of .NET project `GameX.App.Explorer`.
//!
//! Module layout mirrors the C# folder/file layout exactly so the two trees
//! can be diffed and updated in parallel. See PORT_MAP.tsv and PORTING.md.

pub mod app_shell_xaml;
pub mod app_xaml;
pub mod controls;
pub mod resources;
pub mod views;
