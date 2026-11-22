// PORT-SOURCE: Families/GameX.ID/ID.cs
// PORT-SHA: c3dec055c01efbc2
// PORT-STATUS: partial
//
// Ported: `IDArchive` constructor, `getArcBinary`, `assetFactory`, and
// `register()`. `register()` exists. The Explorer must call it, or opening ID
// fails with "unknown ArchiveType". C# finds the archive by reflection.
// The Q database does not continue. `QGame.Loaded`'s `Games.Q.Database` call
// is intentionally not ported. `family.rs` records `gameType` and builds the
// base `FamilyGame`; it does not construct `QGame`.
// `ITransformAsset` is not wired: `UnknownTransform` is a stub, the same
// choice as Arkane.
// `.lmp` / `.tex` is `Binary_Lmp`, `.spr` is `Binary_Spr`, `.mdl` is the empty
// `Binary_Mdl`. `.bsp` is `Binary_BspX::factory`. `.pak` uses `Binary_Pak`,
// the same arm as `.arc`.

use std::io::{self, Error, ErrorKind};
use std::sync::Arc;
use gamex::binary::{newArchive, ArcBinary, Archive, AssetFactory, BinaryArchive, BinaryState};
use gamex::family::{registerType, ArchiveFactory, FamilyGame};
use gamex::meta::{extension, FileSource, Tag};
use gamex_uncore::formats::binary::Binary_Zip;
use gamex_uncore::uncore::UncoreArchive;
use crate::formats::binary::{Binary_BspX, Binary_Lmp, Binary_Mdl, Binary_Pak, Binary_Spr, Binary_Wad};

pub fn register() {
    registerType::<ArchiveFactory>("GameX.ID.IDArchive, GameX.ID", |parent, state| Ok(IDArchive::new(parent, state)? as Arc<dyn Archive>));
}

/// IDArchive
pub struct IDArchive;

impl IDArchive {
    pub fn new(parent: Option<Arc<dyn Archive>>, state: BinaryState) -> io::Result<Arc<BinaryArchive>> {
        let extension = extension(&state.path).to_lowercase();
        let arcBinary = Self::getArcBinary(&state.game, &extension)?;
        let mut s = BinaryArchive::new(parent, state, arcBinary);
        s.base.assetFactoryFunc = Some(Self::assetFactory);
        Ok(newArchive(s))
    }

    // #region Factories

    fn getArcBinary(game: &FamilyGame, extension: &str) -> io::Result<Option<Arc<dyn ArcBinary>>> {
        Ok(match extension {
            "" => None,
            ".pk3" | ".pk4" | ".zip" => Some(Binary_Zip::getArcBinary(game)),
            ".arc" | ".pak" => Some(Binary_Pak::current()),
            ".wad" => Some(Binary_Wad::current()),
            _ => return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")),
        })
    }

    pub fn assetFactory(source: &FileSource, game: &FamilyGame) -> (Tag, Option<AssetFactory>) {
        match game.engine.0.as_deref() {
            Some("idTech") => match extension(&source.path).to_lowercase().as_str() {
                ".lmp" | ".tex" => (Tag::Int(0), Some(Binary_Lmp::factory)),
                ".bsp" => (Tag::Int(0), Some(Binary_BspX::factory)),
                ".mdl" => (Tag::Int(0), Some(Binary_Mdl::factory)),
                ".spr" => (Tag::Int(0), Some(Binary_Spr::factory)),
                _ => UncoreArchive::assetFactory(source, game),
            },
            _ => panic!("ArgumentOutOfRangeException: game.Engine"),
        }
    }

    // #endregion
}
