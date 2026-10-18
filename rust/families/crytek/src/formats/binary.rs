// PORT-SOURCE: Families/GameX.Crytek/Formats/Binary.cs
// PORT-SHA: d2ed882b020ae856
// PORT-STATUS: partial
//
// Ported: `Binary_Cry3`'s constructors and `Current`. `Read` sets `UseReader`
// false and assigns an empty file list, then fails: the listing is
// `ZipArchiveX(..., ZipKind.Cry3)` in `_LIB/System.IO.Compression.cs`, which
// is still the NOT PORTED placeholder. `ReadData` fails the same way.
// Commented `Write` / `WriteData` stay commented.
//
// Factory slots only: `Binary_CryXml`, `Binary_CryFile`, and the Dunia readers
// `Binary_Map`, `Binary_Xbt`, `Binary_Xbg`, `Binary_Fcb` (those four live in
// `Formats/Dunia/Binary.cs`, which has no Rust twin yet). PORT_MAP stays `todo`.

use std::io::{Error, ErrorKind};
use std::sync::atomic::Ordering;
use std::sync::{Arc, LazyLock};
use gamex::binary::{ArcBinary, Archive, BinaryArchive, Stream, Task};
use gamex::meta::{FileSource, Tag};
use openx_vfx::vfx::VfsRead;

macro_rules! unportedFactory {
    ($name:ident) => {
        pub struct $name;
        impl $name {
            pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
                Box::pin(async { Err(Error::new(ErrorKind::Unsupported, concat!(stringify!($name), " isn't ported"))) })
            }
        }
    };
}

unportedFactory!(Binary_CryXml);
unportedFactory!(Binary_CryFile);
unportedFactory!(Binary_Map);
unportedFactory!(Binary_Xbt);
unportedFactory!(Binary_Xbg);
unportedFactory!(Binary_Fcb);

/// Binary_Cry3
pub struct Binary_Cry3 {
    key: Option<Vec<u8>>,
}

impl Binary_Cry3 {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_Cry3 { key: None }));
        CURRENT.clone()
    }

    /// C# `Binary_Cry3(byte[] key = null)`.
    pub fn new(key: Option<Vec<u8>>) -> Self { Self { key } }
}

impl ArcBinary for Binary_Cry3 {
    fn read<'a>(&'a self, source: &'a BinaryArchive, _r: Option<&'a mut dyn VfsRead>, _tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let _key = &self.key;
            source.useReader.store(false, Ordering::Relaxed);
            source.setFiles(Some(Vec::new()));
            Err(Error::new(ErrorKind::Unsupported, "ZipArchiveX ZipKind.Cry3 isn't ported"))
        })
    }

    fn readData<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, option: &'a Tag) -> Task<'a, Option<gamex::binary::Stream>> {
        let _ = (source, r, file, option);
        Box::pin(async { Err(Error::new(ErrorKind::Unsupported, "ZipArchiveX ZipKind.Cry3 isn't ported")) })
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use gamex::binary::{getResult, BinaryArchive, BinaryState};
    use gamex::family::FamilyGame;
    use openx_vfx::vfx::{DirectoryFileSystem, VfsRead};
    use std::io::Cursor;
    use std::sync::atomic::Ordering;

    #[test]
    fn cry3ClearsUseReaderAndFailsOnZipArchiveX() {
        let state = BinaryState::new(Arc::new(DirectoryFileSystem::new("", None)), FamilyGame::empty(), None, Some("a.pak"), Tag::None);
        let arc = BinaryArchive::new(None, state, Some(Binary_Cry3::current()));
        assert!(arc.useReader.load(Ordering::Relaxed));
        let binary = Binary_Cry3::new(Some(vec![1, 2, 3]));
        let mut cursor = Cursor::new(Vec::<u8>::new());
        let err = getResult(binary.read(&arc, Some(&mut cursor as &mut dyn VfsRead), &Tag::None)).unwrap_err();
        assert!(err.to_string().contains("ZipArchiveX"), "{err}");
        assert!(!arc.useReader.load(Ordering::Relaxed));
        assert!(arc.files().unwrap().is_empty());
    }
}
