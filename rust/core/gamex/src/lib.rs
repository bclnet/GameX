//! `gamex` — 1:1 port of .NET project `GameX`.
//!
//! Module layout mirrors the C# folder/file layout exactly so the two trees
//! can be diffed and updated in parallel. See PORT_MAP.tsv and PORTING.md.

pub mod app;
pub mod assembly;
pub mod blizzard_proto_database;
pub mod client;
pub mod config;
pub mod des_ser;
pub mod family;
pub mod formats;
pub mod globalx;
pub mod manager;
pub mod meta;
pub mod store;
pub mod transforms;
pub mod util;
pub mod vendor;
