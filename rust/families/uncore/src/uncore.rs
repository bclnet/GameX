// PORT-SOURCE: Families/GameX.Uncore/Uncore.cs
// PORT-SHA: cf108586ac269047
// PORT-STATUS: done
//
// PARTIAL PORT: AssetFactory's `.dds` (Binary_Dds) and `.start` (Binary_Engine)
// arms return (0, null), since formats/binary.rs doesn't port those types yet,
// so those files fall back to MetaManager's text-or-hex content.
//
// C# finds UncoreArchive by name through Type.GetType; `register` adds it to
// gamex's type registry under the same name, and an app calls it at startup.

use std::io::{self, Error, ErrorKind};
use std::sync::Arc;
use gamex::archiveSource;
use gamex::binary::{newArchive, AssetFactory, Archive, Binary, BinaryState, Stream, Task};
use gamex::family::{registerType, ArchiveFactory, FamilyGame};
use gamex::meta::{extension, FileSource, Tag};
use crate::formats::binary::{Binary_Img, Binary_Pcx, Binary_Snd, Binary_Tga, Binary_TestTri, Binary_Txt};

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
        match extension(&source.path).to_lowercase().as_str() {
            ".txt" | ".ini" | ".cfg" | ".csv" | ".xml" => (Tag::Int(0), Some(Binary_Txt::factory)),
            ".wav" | ".mp3" => (Tag::Int(0), Some(Binary_Snd::factory)),
            ".bmp" | ".jpg" | ".png" | ".gif" | ".tiff" => (Tag::Int(0), Some(Binary_Img::factory)), // Exif
            ".pcx" => (Tag::Int(0), Some(Binary_Pcx::factory)),
            ".tga" => (Tag::Int(0), Some(Binary_Tga::factory)),
            ".dds" => (Tag::Int(0), None), // Binary_Dds: not ported
            ".start" => (Tag::Int(0), None), // Binary_Engine: not ported
            _ => match source.path.as_str() {
                "testtri.gfx" => (Tag::Int(0), Some(Binary_TestTri::factory)),
                _ => (Tag::Int(0), None),
            }
        }
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
