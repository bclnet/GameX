// PORT-SOURCE: Families/GameX.IW/IW.cs
// PORT-SHA: 757584843dc8ca46
// PORT-STATUS: partial
//
// PARTIAL PORT: `IWArchive` constructs `Binary_IW`. `assetFactory` always
// uses Uncore. `.iwi` (`Binary_Iwi`) and `ITransformAsset` are not ported.
// `register` is not called from the Explorer, so opening IW there still
// fails with "unknown ArchiveType".

use std::io;
use std::sync::Arc;
use gamex::binary::{newArchive, ArcBinary, Archive, AssetFactory, BinaryArchive, BinaryState};
use gamex::family::{registerType, ArchiveFactory, FamilyGame};
use gamex::meta::{FileSource, Tag};
use gamex_uncore::uncore::UncoreArchive;
use crate::formats::binary::Binary_IW;

pub fn register() {
    registerType::<ArchiveFactory>("GameX.IW.IWArchive, GameX.IW", |parent, state| Ok(IWArchive::new(parent, state)? as Arc<dyn Archive>));
}

/// IWArchive
pub struct IWArchive;

impl IWArchive {
    pub fn new(parent: Option<Arc<dyn Archive>>, state: BinaryState) -> io::Result<Arc<BinaryArchive>> {
        let arcBinary: Option<Arc<dyn ArcBinary>> = Some(Binary_IW::current());
        let mut s = BinaryArchive::new(parent, state, arcBinary);
        s.base.assetFactoryFunc = Some(Self::assetFactory);
        Ok(newArchive(s))
    }

    pub fn assetFactory(source: &FileSource, game: &FamilyGame) -> (Tag, Option<AssetFactory>) {
        UncoreArchive::assetFactory(source, game)
    }
}
