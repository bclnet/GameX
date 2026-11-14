//! `gamex-iw` — 1:1 port of .NET project `GameX.IW`.
//!
//! Module layout mirrors the C# folder/file layout. `ZoneUnused` is parked
//! and is not ported, including the asset-type enums and `FastFileUnused`.
//! See PORT_MAP.tsv and PORTING.md.

pub mod formats;
pub mod iw;
