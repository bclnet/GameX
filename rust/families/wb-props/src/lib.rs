//! `gamex-wb-props` — 1:1 port of .NET project `GameX.WB.Props`.
//!
//! Module layout mirrors the C# folder/file layout exactly so the two trees
//! can be diffed and updated in parallel. See PORT_MAP.tsv and PORTING.md.
//! `gamex-wb` re-exports `formats::ac::props` at its own `formats::ac::props`.

pub mod formats;
