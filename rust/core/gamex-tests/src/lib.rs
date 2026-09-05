//! `gamex-tests` — 1:1 port of .NET project `GameXTests`.
//!
//! Module layout mirrors the C# folder/file layout exactly so the two trees
//! can be diffed and updated in parallel. See PORT_MAP.tsv and PORTING.md.

pub mod assembly;
pub mod base;
pub mod bethesada;
pub mod blizzard;
pub mod cig;
pub mod crytek;
pub mod exports;
pub mod file_data_tests;
pub mod file_manager_tests;
pub mod formats;
pub mod iw;
pub mod red;
pub mod resource_tests;
pub mod test_helper;
pub mod uncore;
pub mod unity;
pub mod valve;
pub mod wb;
