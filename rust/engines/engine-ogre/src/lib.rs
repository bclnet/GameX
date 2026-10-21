//! `gamex-engine-ogre` — 1:1 port of .NET project `GameX.Engine.Ogre`.
//!
//! Module layout mirrors the C# folder/file layout exactly so the two trees
//! can be diffed and updated in parallel. See PORT_MAP.tsv and PORTING.md.

pub mod ogre_ftl_object_builder;
pub mod ogre_fts_object_builder;
pub mod ogre_object_holder;
pub mod ogre_render;
