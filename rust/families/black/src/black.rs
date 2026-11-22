// PORT-SOURCE: Families/GameX.Black/Black.cs
// PORT-SHA: 406137879a6d7e97
// PORT-STATUS: partial
//
// PARTIAL: ITransformAsset stays unwired because UnknownTransform is a stub.
// Readers live in formats/binary.rs. getArcBinary is the commented C# switch:
// only ".dat", otherwise ArgumentOutOfRangeException. These games have no engine.

use std::io::{self, Error, ErrorKind};
use std::sync::Arc;
use gamex::binary::{newArchive, ArcBinary, Archive, AssetFactory, BinaryArchive, BinaryState};
use gamex::family::{registerType, ArchiveFactory, FamilyGame};
use gamex::meta::{extension, FileSource, Tag};
use crate::formats::binary::{Binary_Dat, Binary_Frm, Binary_Map, Binary_Pal2, Binary_Rix};

pub fn register() {
    registerType::<ArchiveFactory>("GameX.Black.BlackArchive, GameX.Black", |parent, state| Ok(BlackArchive::new(parent, state)? as Arc<dyn Archive>));
}

/// BlackArchive
pub struct BlackArchive;

impl BlackArchive {
    pub fn new(parent: Option<Arc<dyn Archive>>, state: BinaryState) -> io::Result<Arc<BinaryArchive>> {
        let arcBinary = Self::getArcBinary(&state.game, &extension(&state.path).to_lowercase())?;
        let mut s = BinaryArchive::new(parent, state, Some(arcBinary));
        s.base.assetFactoryFunc = Some(Self::assetFactory);
        Ok(newArchive(s))
    }

    // #region Factories

    fn getArcBinary(_game: &FamilyGame, extension: &str) -> io::Result<Arc<dyn ArcBinary>> {
        match extension {
            ".dat" => Ok(Binary_Dat::current()),
            _ => Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")),
        }
    }

    pub fn assetFactory(source: &FileSource, _game: &FamilyGame) -> (Tag, Option<AssetFactory>) {
        let ext = extension(&source.path).to_lowercase();
        if ext.starts_with(".fr") { return (Tag::Int(0), Some(Binary_Frm::factory)); }
        match ext.as_str() {
            ".pal" => (Tag::Int(0), Some(Binary_Pal2::factory)),
            ".rix" => (Tag::Int(0), Some(Binary_Rix::factory)),
            ".map" => (Tag::Int(0), Some(Binary_Map::factory)),
            _ => (Tag::Int(0), None),
        }
    }

    // #endregion
}
