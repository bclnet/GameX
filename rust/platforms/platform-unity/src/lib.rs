//! `gamex-platform-unity` — 1:1 port of .NET project `GameX.Platform.Unity`.
//!
//! Module layout mirrors the C# folder/file layout exactly so the two trees
//! can be diffed and updated in parallel. See PORT_MAP.tsv and PORTING.md.

pub mod scripts;
pub mod unity_nif_object_builder;
pub mod unity_render;
