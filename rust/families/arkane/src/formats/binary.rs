// PORT-SOURCE: Families/GameX.Arkane/Formats/Binary.cs
// PORT-SHA: 0a45a0a3dc4309a2
// PORT-STATUS: done
//
// C#'s `ArcBinary<Self>.Current` is `current()`. `Binary_Danae` decrypts the FAT
// in place through a pointer; here it indexes the byte vector. `Binary_Void`'s
// `(string, string, string)` file tag is a `Tag::Any` holding that tuple.

use std::collections::HashSet;
use std::io::{self, Cursor, Error, ErrorKind, Seek, SeekFrom};
use std::sync::{Arc, LazyLock};
use gamex::binary::{ArcBinary, BinaryArchive, Stream, SubArchive, Task};
use gamex::meta::{FileSource, Tag};
use gamex_uncore::formats::compression::Compression;
use openx_poly::prelude::{BinaryReaderExt, IStruct};
use openx_vfx::vfx::VfsRead;

fn reader<'a>(r: Option<&'a mut dyn VfsRead>) -> io::Result<&'a mut dyn VfsRead> { r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "r")) }

// #region Binary_Danae

pub struct Binary_Danae;

impl Binary_Danae {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_Danae));
        CURRENT.clone()
    }
}

impl ArcBinary for Binary_Danae {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let r = reader(r)?;

            // #region Encryption

            let key = source.base.game.key.asBytes().ok_or_else(|| Error::new(ErrorKind::InvalidData, "NullReferenceException: Key"))?;
            let (keyLength, mut keyIndex) = (key.len(), 0usize);
            fn readInt32(b: &mut usize, end: usize, fat: &mut [u8], key: &[u8], keyIndex: &mut usize) -> io::Result<i32> {
                if *b + 4 > end { return Err(Error::new(ErrorKind::InvalidData, "truncated FAT")); }
                let p = *b;
                for i in 0..4 { fat[p + i] ^= key[*keyIndex]; *keyIndex += 1; if *keyIndex >= key.len() { *keyIndex = 0; } }
                *b += 4;
                Ok(i32::from_le_bytes(fat[p..p + 4].try_into().unwrap()))
            }
            fn readString(b: &mut usize, end: usize, fat: &mut [u8], key: &[u8], keyIndex: &mut usize) -> io::Result<String> {
                let mut p = *b;
                loop {
                    if p >= end { return Err(Error::new(ErrorKind::InvalidData, "truncated FAT")); }
                    fat[p] ^= key[*keyIndex]; *keyIndex += 1; if *keyIndex >= key.len() { *keyIndex = 0; }
                    if fat[p] == 0 { break; }
                    p += 1;
                }
                let r: String = fat[*b..p].iter().map(|&c| if c < 0x80 { c as char } else { '?' }).collect();
                *b = p + 1;
                Ok(r)
            }

            // #endregion

            let mut files = Vec::new();

            // move to fat table
            let offset = r.readUInt32()?;
            r.seek(SeekFrom::Start(offset as u64))?;
            let fatSize = r.readUInt32()? as usize;
            let mut fatBytes = r.readBytes(fatSize)?;

            // while there are bytes
            let (mut c, end) = (0usize, fatBytes.len());
            while c < end {
                let dirPath = readString(&mut c, end, &mut fatBytes, key, &mut keyIndex)?.replace('\\', "/");
                let numFiles = readInt32(&mut c, end, &mut fatBytes, key, &mut keyIndex)?;
                for _ in 0..numFiles {
                    // get file
                    let mut file = FileSource {
                        path: dirPath.clone() + &readString(&mut c, end, &mut fatBytes, key, &mut keyIndex)?.replace('\\', "/"),
                        offset: readInt32(&mut c, end, &mut fatBytes, key, &mut keyIndex)? as i64,
                        compressed: readInt32(&mut c, end, &mut fatBytes, key, &mut keyIndex)?,
                        fileSize: readInt32(&mut c, end, &mut fatBytes, key, &mut keyIndex)? as i64,
                        packedSize: readInt32(&mut c, end, &mut fatBytes, key, &mut keyIndex)? as i64,
                        ..Default::default()
                    };
                    // special case
                    if file.path.to_lowercase().ends_with(".ftl") { file.compressed = 1; }
                    else if file.compressed == 0 { file.fileSize = file.packedSize; }
                    // add file
                    files.push(Arc::new(file));
                }
            }
            source.setFiles(Some(files));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async move {
            let r = reader(r)?;
            r.seek(SeekFrom::Start(file.offset as u64))?;
            Ok(Some(Box::new(Cursor::new(if (file.compressed & 1) != 0
                { r.decompressBlast(file.packedSize as i32, file.fileSize as i32)? }
                else { r.readBytes(file.packedSize as usize)? })) as Stream))
        })
    }
}

// #endregion

// #region Binary_Void

pub struct Binary_Void;

// #region Headers

#[repr(C, packed)]
#[derive(Debug, Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
struct V_File {
    pub Offset: u64,
    pub FileSize: u32,
    pub PackedSize: u32,
    pub Unknown1: u32,
    pub Flags: u32,
    pub Flags2: u16,
}
impl IStruct for V_File { const Struct: (&'static str, usize) = (">Q4IH", 26); }

// #endregion

impl Binary_Void {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_Void));
        CURRENT.clone()
    }
}

impl ArcBinary for Binary_Void {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let r = reader(r)?;
            if !source.base.binPath.ends_with(".index") { return Err(Error::new(ErrorKind::InvalidData, "must be a .index file")); }

            // master.index file
            if source.base.binPath == "master.index" {
                const MAGIC: u32 = 0x04534552;
                const SubMarker: u32 = 0x18000000;
                const EndMarker: u32 = 0x01000000;

                let mut files = Vec::new();
                let magic = r.readUInt32E()?;
                if magic != MAGIC { return Err(Error::new(ErrorKind::InvalidData, "BAD MAGIC")); }
                r.skip(4)?;
                let mut first = true;
                let parent = source.base.this().downcastBinaryArchive().ok_or_else(|| Error::new(ErrorKind::InvalidData, "source"))?;
                loop {
                    let mut pathSize = r.readUInt32()?;
                    if pathSize == SubMarker { first = false; pathSize = r.readUInt32()?; }
                    else if pathSize == EndMarker { break; }
                    let path = r.readFAString(pathSize as usize)?.unwrap_or_default().replace('\\', "/");
                    let packId = if first { 0 } else { r.readUInt16()? };
                    if !path.ends_with(".index") { continue; }
                    files.push(Arc::new(FileSource {
                        arc: Some(SubArchive::new(&parent, None, &path, Tag::None, Self::current())),
                        path,
                        ..Default::default()
                    }));
                }
                source.setFiles(Some(files));
                return Ok(());
            }

            // find files
            let vfx = &source.base.vfx;
            let binPath = &source.base.binPath;
            let resourcePath = format!("{}.resources", &binPath[..binPath.len() - 6]);
            if !vfx.fileExists(&resourcePath) { return Err(Error::new(ErrorKind::InvalidData, "Unable to find resources extension")); }
            let sharedResourcePath = [
                "shared_2_3.sharedrsc",
                "shared_2_3_4.sharedrsc",
                "shared_1_2_3.sharedrsc",
                "shared_1_2_3_4.sharedrsc"]
                .into_iter().find(|s| vfx.fileExists(s));

            // read
            r.seek(SeekFrom::Start(4))?;
            let mainFileSize = r.readUInt32E()?;
            r.skip(24)?;
            let numFiles = r.readUInt32E()?;
            let mut files = Vec::with_capacity(numFiles as usize);
            for i in 0..numFiles {
                let id = r.readUInt32E()?;
                let tag1 = r.readL32UString(0, false)?;
                let tag2 = r.readL32UString(0, false)?;
                let path = r.readL32UString(0, false)?.unwrap_or_default().replace('\\', "/");
                let file: V_File = r.readS()?;
                let useSharedResources = (file.Flags & 0x20) != 0 && file.Flags2 == 0x8000;
                if useSharedResources && sharedResourcePath.is_none() { return Err(Error::new(ErrorKind::InvalidData, "sharedResourcePath not available")); }
                let newPath = if useSharedResources { sharedResourcePath.unwrap().to_string() } else { resourcePath.clone() };
                files.push(Arc::new(FileSource {
                    id: id as i32,
                    path,
                    compressed: if file.FileSize != file.PackedSize { 1 } else { 0 },
                    fileSize: file.FileSize as i64,
                    packedSize: file.PackedSize as i64,
                    offset: file.Offset as i64,
                    tag: Tag::Any(Arc::new((newPath, tag1, tag2))),
                    ..Default::default()
                }));
            }
            source.setFiles(Some(files));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async move {
            if file.fileSize == 0 || _badPositions.contains(&file.offset) { return Ok(Some(Box::new(Cursor::new(Vec::new())) as Stream)); }
            let (path, tag1, tag2) = file.tag.asAny().and_then(|s| s.downcast_ref::<(String, Option<String>, Option<String>)>()).ok_or_else(|| Error::new(ErrorKind::InvalidData, "InvalidCastException: Tag"))?;
            let mut r2 = source.getReader(Some(path))?.ok_or_else(|| Error::new(ErrorKind::NotFound, path.clone()))?;
            r2.seek(SeekFrom::Start(file.offset as u64))?;
            Ok(Some(Box::new(Cursor::new(if file.compressed != 0
                { r2.decompressZlib(file.packedSize as i32, file.fileSize as i32, false)? }
                else { r2.readBytes(file.packedSize as usize)? })) as Stream))
        })
    }
}

// Bad Positions - Dishonored2
static _badPositions: LazyLock<HashSet<i64>> = LazyLock::new(|| HashSet::from([
    293, //: generated/decls/renderparm/atm/worldfog/artistscatteringcolor.decl
    917004, //: generated/decls/renderparm/ocean/patchtransform.decl
    9923823, //: generated/decls/soundevent/sound_events/bsp/bsp_physmat/bsp_foosteps/bsp_fs_player/emily/fs_e_metal_chandelier/fs_e_metal_chandelier_w.decl
    9924002, //: generated/decls/fx/contactsystem/w.emily_env.metal.chandelier.fx.decl
    32872162, //: generated/image/models/effects/textures/gameplay/blood/blood_leads_05_fl.bimage7
    32966564, //: generated/decls/material/models/effects/materials/gameplay/blood/blood_leads_05_bf.material.decl
    45704814, //: generated/decls/fx/contactsystem/pr.ar.venom_env.tile.fx.decl
]));

// #endregion
