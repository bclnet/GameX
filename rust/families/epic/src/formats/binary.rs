// PORT-SOURCE: Families/GameX.Epic/Formats/Binary.cs
// PORT-SHA: d88dfb8a790847ac
// PORT-STATUS: partial
//
// `Binary_Pck.Read` builds `Core.UPackage` and walks `header.Exports`. That
// type is still `PORT-STATUS: todo` (`formats/core/u_package.rs` and the
// partials beside it), so `Read` assigns the empty file list and fails there.
// `ReadData` would seek `file.Tag` as the package `BinaryReader`; it fails
// with the same gap. `WriteData` throws `NotImplementedException`, as in C#.
// PORT_MAP stays `todo` until `UPackage` is ported. Do not treat this file as
// the Unreal package reader.

use std::io::{Error, ErrorKind, Write};
use std::sync::{Arc, LazyLock};
use gamex::binary::{ArcBinary, BinaryArchive, Stream, Task};
use gamex::meta::{FileSource, Tag};
use openx_vfx::vfx::VfsRead;

pub struct Binary_Pck;

impl Binary_Pck {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_Pck));
        CURRENT.clone()
    }
}

impl ArcBinary for Binary_Pck {
    fn read<'a>(&'a self, source: &'a BinaryArchive, _r: Option<&'a mut dyn VfsRead>, _tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            source.setFiles(Some(Vec::new()));
            Err(Error::new(ErrorKind::Unsupported, "UPackage isn't ported"))
        })
    }

    fn readData<'a>(&'a self, _source: &'a BinaryArchive, _r: Option<&'a mut dyn VfsRead>, _file: &'a Arc<FileSource>, _option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async { Err(Error::new(ErrorKind::Unsupported, "UPackage isn't ported")) })
    }

    fn writeData<'a>(&'a self, _source: &'a BinaryArchive, _w: Option<&'a mut dyn Write>, _file: &'a Arc<FileSource>, _data: Stream, _option: &'a Tag) -> Task<'a, ()> {
        Box::pin(async { Err(Error::new(ErrorKind::Unsupported, "NotImplementedException")) })
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use gamex::binary::{getResult, BinaryArchive, BinaryState};
    use gamex::family::FamilyGame;
    use openx_vfx::vfx::DirectoryFileSystem;

    #[test]
    fn pckAssignsNoFilesAndFailsOnUPackage() {
        let state = BinaryState::new(Arc::new(DirectoryFileSystem::new("", None)), FamilyGame::empty(), None, Some("a.pck"), Tag::None);
        let arc = BinaryArchive::new(None, state, Some(Binary_Pck::current()));
        let err = getResult(Binary_Pck.read(&arc, None, &Tag::None)).unwrap_err();
        assert!(err.to_string().contains("UPackage"), "{err}");
        assert!(arc.files().unwrap().is_empty());
        let err = getResult(Binary_Pck.writeData(&arc, None, &Arc::new(FileSource::default()), Box::new(std::io::Cursor::new(Vec::<u8>::new())), &Tag::None)).unwrap_err();
        assert!(err.to_string().contains("NotImplementedException"), "{err}");
    }
}
