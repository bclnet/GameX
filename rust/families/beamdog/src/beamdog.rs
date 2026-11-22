// PORT-SOURCE: Families/GameX.Beamdog/Beamdog.cs
// PORT-SHA: 4e86c9ff36ada907
// PORT-STATUS: partial
//
// PARTIAL PORT: `getArcBinary` dispatches `.key` / `.bif` to `Binary_Key`,
// `.dat` to `Binary_Dat`, and `.gob` / `.lmp` to `Binary_Da2`. Those readers
// are regions in `formats/binary.rs`. `assetFactory` is still `UncoreArchive` for every
// extension. `ITransformAsset` is not wired: `UnknownTransform` is a stub,
// the same choice as Arkane. C# finds `BeamdogArchive` by name through
// Type.GetType; `register` adds it under the same spec string.

use std::io::{self, Error, ErrorKind};
use std::sync::Arc;
use gamex::binary::{newArchive, ArcBinary, Archive, AssetFactory, BinaryArchive, BinaryState};
use gamex::family::{registerType, ArchiveFactory, FamilyGame};
use gamex::meta::{extension, FileSource, Tag};
use gamex_uncore::uncore::UncoreArchive;
use crate::formats::binary::{Binary_Da2, Binary_Dat, Binary_Key};

pub fn register() {
    registerType::<ArchiveFactory>("GameX.Beamdog.BeamdogArchive, GameX.Beamdog", |parent, state| Ok(BeamdogArchive::new(parent, state)? as Arc<dyn Archive>));
}

/// BeamdogArchive
pub struct BeamdogArchive;

impl BeamdogArchive {
    pub fn new(parent: Option<Arc<dyn Archive>>, state: BinaryState) -> io::Result<Arc<BinaryArchive>> {
        let arcBinary = Self::getArcBinary(&state.game, &extension(&state.path).to_lowercase())?;
        let mut s = BinaryArchive::new(parent, state, Some(arcBinary));
        s.base.assetFactoryFunc = Some(Self::assetFactory);
        Ok(newArchive(s))
    }

    // #region Factories

    fn getArcBinary(game: &FamilyGame, extension: &str) -> io::Result<Arc<dyn ArcBinary>> {
        Ok(match extension {
            ".key" | ".bif" => Binary_Key::current(),
            ".dat" => Binary_Dat::current(),
            ".gob" | ".lmp" => Binary_Da2::current(),
            _ => return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")),
        })
    }

    pub fn assetFactory(source: &FileSource, game: &FamilyGame) -> (Tag, Option<AssetFactory>) {
        match extension(&source.path).to_lowercase().as_str() {
            _ => UncoreArchive::assetFactory(source, game),
        }
    }

    // #endregion
}
