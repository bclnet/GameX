// PORT-SOURCE: Families/GameX.Bioware/Bioware.cs
// PORT-SHA: 97d80253b9dba2a1
// PORT-STATUS: partial
//
// PARTIAL: readers live in formats/binary.rs. ITransformAsset is not wired
// because UnknownTransform is a stub. assetFactory sends .dlg/.qdb/.qst to
// Binary_Gff when that type is in crate::formats::binary, else Uncore.

use std::io::{self, Error, ErrorKind};
use std::sync::Arc;
use gamex::binary::{newArchive, ArcBinary, Archive, AssetFactory, BinaryArchive, BinaryState};
use gamex::family::{registerType, ArchiveFactory, FamilyGame};
use gamex::meta::{extension, FileSource, Tag};
use gamex_uncore::formats::binary::Binary_Zip;
use gamex_uncore::uncore::UncoreArchive;
use crate::formats::binary::{Binary_Aurora, Binary_Gff, Binary_Key, Binary_Myp};

pub fn register() {
    registerType::<ArchiveFactory>("GameX.Bioware.BiowareArchive, GameX.Bioware", |parent, state| Ok(BiowareArchive::new(parent, state)? as Arc<dyn Archive>));
}

/// BiowareArchive
pub struct BiowareArchive;

impl BiowareArchive {
    pub fn new(parent: Option<Arc<dyn Archive>>, state: BinaryState) -> io::Result<Arc<BinaryArchive>> {
        let arcBinary = Self::getArcBinary(&state.game, &extension(&state.path).to_lowercase())?;
        let mut s = BinaryArchive::new(parent, state, Some(arcBinary));
        s.base.assetFactoryFunc = Some(Self::assetFactory);
        Ok(newArchive(s))
    }

    // #region Factories

    fn getArcBinary(game: &FamilyGame, extension: &str) -> io::Result<Arc<dyn ArcBinary>> {
        let engine = game.engine.0.as_deref();
        Ok(match (engine, extension) {
            (Some("Infinity"), ".key" | ".bif") => Binary_Key::current(),
            (Some("Aurora" | "Odyssey"), ".key" | ".bif") => Binary_Aurora::current(),
            (Some("Omen" | "Aurora"), ".zip") => Binary_Zip::getArcBinary(game),
            (Some("Hero"), ".tor") => Binary_Myp::current(),
            _ => return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")),
        })
    }

    pub fn assetFactory(source: &FileSource, game: &FamilyGame) -> (Tag, Option<AssetFactory>) {
        match extension(&source.path).to_lowercase().as_str() {
            ".dlg" | ".qdb" | ".qst" => (Tag::Int(0), Some(Binary_Gff::factory)),
            _ => UncoreArchive::assetFactory(source, game),
        }
    }

    // #endregion
}
