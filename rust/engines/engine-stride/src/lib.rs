//! `gamex-engine-stride` — 1:1 port of .NET project `GameX.Engine.Stride`.
//!
//! Module layout mirrors the C# folder/file layout exactly so the two trees
//! can be diffed and updated in parallel. See PORT_MAP.tsv and PORTING.md.

pub mod stride_ftl_object_builder;
pub mod stride_fts_object_builder;
pub mod stride_object_holder;
pub mod stride_render;
