// PORT-SOURCE: Families/GameX.Epic/Epic.cs
// PORT-SHA: d6a6da390e1e0e09
// PORT-STATUS: partial
//
// Ported: `EpicArchive::new` and `assetFactory`. The only factory arm is `_`,
// which is `UncoreArchive::assetFactory`. `Binary_Pck` is the archive binary;
// its read fails until `UPackage` is ported. Transforms are not wired.
// Not registered: `app_xaml.rs` still lists Uncore and Arkane. The spec names
// `GameX.Epic.EpicArchive`.

use std::sync::Arc;
use gamex::binary::{newArchive, AssetFactory, Archive, BinaryArchive, BinaryState};
use gamex::family::FamilyGame;
use gamex::meta::{extension, FileSource, Tag};
use gamex_uncore::uncore::UncoreArchive;
use crate::formats::binary::Binary_Pck;

/// EpicArchive
pub struct EpicArchive;

impl EpicArchive {
    pub fn new(parent: Option<Arc<dyn Archive>>, state: BinaryState) -> Arc<BinaryArchive> {
        let mut s = BinaryArchive::new(parent, state, Some(Binary_Pck::current()));
        s.base.assetFactoryFunc = Some(Self::assetFactory);
        newArchive(s)
    }

    // #region Factories

    pub fn assetFactory(source: &FileSource, game: &FamilyGame) -> (Tag, Option<AssetFactory>) {
        match extension(&source.path).to_lowercase().as_str() {
            _ => UncoreArchive::assetFactory(source, game),
        }
    }

    // #endregion
}
