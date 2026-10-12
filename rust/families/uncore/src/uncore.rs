// PORT-SOURCE: Families/GameX.Uncore/Uncore.cs
// PORT-SHA: cf108586ac269047
// PORT-STATUS: done
//
// PARTIAL PORT: AssetFactory's Binary_* factories live in Formats/Binary.cs,
// which isn't ported, so every file gets (0, null) and the Explorer falls back
// to MetaManager's text-or-hex content.
//
// C# finds UncoreArchive by name through Type.GetType; `register` adds it to
// gamex's type registry under the same name, and an app calls it at startup.

use std::io::{self, Error, ErrorKind};
use std::sync::Arc;
use gamex::archiveSource;
use gamex::binary::{newArchive, AssetFactory, Archive, Binary, BinaryState, Stream, Task};
use gamex::family::{registerType, ArchiveFactory, FamilyGame};
use gamex::meta::{FileSource, Tag};

pub fn register() {
    registerType::<ArchiveFactory>("GameX.Uncore.UncoreArchive, GameX.Uncore", |parent, state| Ok(UncoreArchive::new(parent, state)));
}

// UncoreFamily: C#'s is the base Family, which createFamily builds when no familyType is registered.

/// UncoreArchive
pub struct UncoreArchive {
    base: Binary,
}
archiveSource!(UncoreArchive);

impl UncoreArchive {
    pub fn new(parent: Option<Arc<dyn Archive>>, state: BinaryState) -> Arc<UncoreArchive> {
        let mut base = Binary::new(state);
        base.name = "Uncore".into();
        base.assetFactoryFunc = Some(Self::assetFactory);
        newArchive(UncoreArchive { base })
    }

    // Factories

    pub fn assetFactory(source: &FileSource, game: &FamilyGame) -> (Tag, Option<AssetFactory>) {
        (Tag::Int(0), None)
    }
}

impl Archive for UncoreArchive {
    fn base(&self) -> &Binary { &self.base }
    fn closing(&self) -> Task<'_, ()> { Box::pin(async { Ok(()) }) }
    fn opening(&self) -> Task<'_, ()> { Box::pin(async { Ok(()) }) }
    fn contains(&self, path: &Tag) -> io::Result<bool> { Ok(false) }
    fn getSource(&self, path: &Tag, throwOnError: bool) -> io::Result<(Option<Arc<dyn Archive>>, Option<Arc<FileSource>>)> { Err(Error::new(ErrorKind::Unsupported, "NotImplemented")) }
    fn getData<'a>(&'a self, path: &'a Tag, option: &'a Tag, throwOnError: bool) -> Task<'a, Option<Stream>> { Box::pin(async { Err(Error::new(ErrorKind::Unsupported, "NotImplemented")) }) }
    fn getAsset<'a>(&'a self, path: &'a Tag, option: &'a Tag, throwOnError: bool) -> Task<'a, Tag> { Box::pin(async { Err(Error::new(ErrorKind::Unsupported, "NotImplemented")) }) }
}
