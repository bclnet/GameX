// PORT-SOURCE: Families/GameX.ID/ID.cs
// PORT-SHA: 9e143a7100ea21b5
// PORT-STATUS: partial
//
// Ported: `IDArchive` constructor, `getArcBinary`, and `assetFactory`.
// `QGame` calls `Games.Q.Database`, which is still unported. `ITransformAsset`
// isn't wired: `UnknownTransform` is a stub. Not registered. The spec names
// `GameX.ID.IDArchive`, and `register` waits until `QGame` is ported.
// `.lmp` / `.tex` is `Binary_Lmp`, `.spr` is `Binary_Spr`, `.mdl` is the empty
// `Binary_Mdl`. `.bsp` is `Binary_BspX`: the constructor and the three header
// `Read`s. `Ent`, miptex, and the content enums are still out.

use std::io::{self, Error, ErrorKind};
use std::sync::Arc;
use gamex::binary::{newArchive, ArcBinary, Archive, AssetFactory, BinaryArchive, BinaryState};
use gamex::family::FamilyGame;
use gamex::meta::{extension, FileSource, Tag};
use gamex_uncore::formats::binary::Binary_Zip;
use gamex_uncore::uncore::UncoreArchive;
use crate::formats::binary::{Binary_BspX, Binary_Lmp, Binary_Mdl, Binary_Pak, Binary_Spr, Binary_Wad};

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
            ".arc" => Some(Binary_Pak::current()),
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
