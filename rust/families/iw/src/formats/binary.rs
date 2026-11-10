// PORT-SOURCE: Families/GameX.IW/Formats/Binary.cs
// PORT-SHA: f59ba82df59b0e9c
// PORT-STATUS: partial
//
// PARTIAL PORT: the `.ff` `Read` and `ReadData` for Magic.FF.
// Not ported: CASC, `.iwd` zip, `.pak`, `.ipak`, `.xpak`, and `Binary_Iwi`.
// BO4, BOCW, and Vanguard set magic CASC and list nothing.

use std::io::{self, Cursor, Error, ErrorKind};
use std::sync::atomic::Ordering;
use std::sync::{Arc, LazyLock};
use gamex::binary::{ArcBinary, BinaryArchive, Stream, Task};
use gamex::meta::{extension, FileSource, Tag};
use openx_poly::prelude::BinaryReaderExt;
use openx_vfx::vfx::VfsRead;
use crate::formats::fast_file::{self, A100, F000, F100, IWff, S1ff, TAff, U100};

fn reader<'a>(r: Option<&'a mut dyn VfsRead>) -> io::Result<&'a mut dyn VfsRead> { r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "r")) }

pub struct Binary_IW;

impl Binary_IW {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_IW));
        CURRENT.clone()
    }
}

impl ArcBinary for Binary_IW {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, _tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let r = reader(r)?;
            source.setFiles(Some(Vec::new()));
            let id = source.base.game.id.as_str();
            if id == "BO4" || id == "BOCW" || id == "Vanguard" {
                source.magic.store(0, Ordering::Relaxed);
                return Ok(());
            }
            if !extension(&source.base.binPath).eq_ignore_ascii_case(".ff") { return Ok(()); }
            source.magic.store(2, Ordering::Relaxed);
            let magic = r.readUInt32()?;
            let format_ = r.readUInt32()?;
            let version = r.readUInt32()?;
            if magic != IWff && magic != S1ff && magic != TAff { return Err(Error::new(ErrorKind::InvalidData, format!("Bad magic {magic}"))); }
            if format_ != U100 && format_ != F100 && format_ != A100 && format_ != F000 { return Err(Error::new(ErrorKind::InvalidData, format!("Bad format {format_}"))); }
            let Some((zone, headers)) = fast_file::listFastFile(r, magic, format_, version)? else { return Ok(()); };
            *source.base.tag.lock().unwrap_or_else(|p| p.into_inner()) = match zone {
                Some(z) => Tag::Bytes(z),
                None => Tag::None,
            };
            let files = headers.into_iter().map(|asset| Arc::new(FileSource {
                id: asset.id,
                path: asset.path,
                offset: asset.position,
                fileSize: asset.fileSize,
                data: asset.data,
                ..Default::default()
            })).collect();
            source.setFiles(Some(files));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, source: &'a BinaryArchive, _r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, _option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async move {
            if let Some(data) = &file.data { return Ok(Some(Box::new(Cursor::new(data.clone())) as Stream)); }
            let tag = source.base.tag.lock().unwrap_or_else(|p| p.into_inner()).clone();
            if let Tag::Bytes(zone) = tag {
                if file.fileSize > 0 {
                    if file.offset < 0 { return Err(Error::new(ErrorKind::InvalidInput, "offset")); }
                    let start = file.offset as usize;
                    let end = start + file.fileSize as usize;
                    if end < start || end > zone.len() { return Err(Error::new(ErrorKind::InvalidInput, "offset")); }
                    return Ok(Some(Box::new(Cursor::new(zone[start..end].to_vec())) as Stream));
                }
            }
            Ok(Some(Box::new(Cursor::new(Vec::new())) as Stream))
        })
    }
}
