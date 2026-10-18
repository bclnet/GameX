// PORT-SOURCE: Families/GameX.ID/Formats/Binary.cs
// PORT-SHA: b289df2afadc91c2
// PORT-STATUS: partial
//
// Ported: `Binary_Pak`, `Binary_Wad` (the `.wad` child `Binary_Pak` opens),
// `Binary_Lmp`, `Binary_Spr`, and `Binary_Mdl` (its constructor is empty, as in C#).
// Factory slot only: `Binary_BspX`. The rest of this file is not ported.
// PORT_MAP stays `todo`.
//
// `Binary_Lmp` keeps C#'s process-wide `Palette` and `Colormap`. A picture
// with no palette throws `NotImplementedException`. `ToLightPixel(32 << 3, index)`
// uses colormap row 0. The stored pixels are RGB plus a 0 byte, and the format
// is still `BGRA32`. A palette lump's `Pixels` is null in C#; here it is empty.
// `Binary_Spr.NextFrame` always returns true and then steps past the last frame.
// Python has no `Binary_Lmp`. Python's `Binary_Spr` sizes `bytes` with `<< 4`
// (C# is `<< 2`) and its `create` reads the name `format`.

use std::any::Any;
use std::io::{self, Cursor, Error, ErrorKind, SeekFrom};
use std::mem::size_of;
use std::sync::{Arc, LazyLock, Mutex, Weak};
use gamex::binary::{newArchive, ArcBinary, Archive, BinaryArchive, BinaryState, Stream, SubArchive, Task};
use gamex::meta::{fileName, FileSource, IHaveMetaInfo, MetaContent, MetaInfo, MetaManager, Tag};
use openx_gfx::gfx::{AnyArc, ITexture, ITextureFrames, TextureAsBytes};
use openx_gfx::gfx_render::blitByPalette;
use openx_gfx::gfx_texture::{TextureFlags, TextureFormat, TexturePixel};
use openx_poly::prelude::{BinaryReaderExt, IStruct};
use openx_poly::system::unsafe_x::FixedAStringScan;
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

unportedFactory!(Binary_BspX);

fn fromResult<'a>(value: io::Result<Tag>) -> Task<'a, Tag> { Box::pin(std::future::ready(value)) }
fn reader<'a>(r: Option<&'a mut dyn VfsRead>) -> io::Result<&'a mut dyn VfsRead> { r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "r")) }
/// C# `Path.GetFileNameWithoutExtension`.
fn fileStem(path: &str) -> String { let name = fileName(path); name.rsplit_once('.').map(|(s, _)| s.to_string()).unwrap_or(name) }
fn node(name: String) -> MetaInfo { MetaInfo::new(Some(name), Tag::None, vec![], false) }
fn content(type_: &str, file: Option<&Arc<FileSource>>, value: Tag) -> MetaInfo {
    MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent { type_: type_.into(), name: file.map_or(String::new(), |s| fileName(&s.path)), value, ..Default::default() })), vec![], false)
}
fn thisTexture<T: ITexture + 'static>(me: &Weak<T>) -> Tag { me.upgrade().map_or(Tag::None, |s| Tag::Texture(s)) }
fn endsWithIgnoreCase(path: &str, suffix: &str) -> bool {
    path.len() >= suffix.len() && path[path.len() - suffix.len()..].eq_ignore_ascii_case(suffix)
}

// #region Binary_Lmp

/// Quake lump picture, palette, or colormap. `Palette` and `Colormap` are process-wide, as in C#.
pub struct Binary_Lmp {
    pub paletteRecords: Option<Vec<Vec<u8>>>,
    pub colormapRecords: Option<Vec<Vec<u8>>>,
    pixels: Vec<u8>,
    pub width: i32,
    pub height: i32,
    me: Weak<Binary_Lmp>,
}

static PALETTE: Mutex<Option<Arc<Binary_Lmp>>> = Mutex::new(None);
static COLORMAP: Mutex<Option<Arc<Binary_Lmp>>> = Mutex::new(None);

impl Binary_Lmp {
    const FORMAT: (TextureFormat, TexturePixel) = (TextureFormat::BGRA32, TexturePixel::Unknown);

    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Self::new(r, &f).map(|s| Tag::Info(s)))
    }

    pub fn new(r: &mut Stream, f: &FileSource) -> io::Result<Arc<Self>> {
        match fileStem(&f.path).as_str() {
            "palette" => {
                let mut paletteRecords = Vec::with_capacity(256);
                for _ in 0..256 {
                    let mut rgb = r.readBytes(3)?;
                    rgb.push(0);
                    paletteRecords.push(rgb);
                }
                let this = Arc::new_cyclic(|w| Self { paletteRecords: Some(paletteRecords), colormapRecords: None, pixels: Vec::new(), width: 0, height: 0, me: w.clone() });
                *PALETTE.lock().unwrap() = Some(this.clone());
                Ok(this)
            }
            "colormap" => {
                let mut colormapRecords = Vec::with_capacity(32);
                for _ in 0..32 { colormapRecords.push(r.readBytes(256)?); }
                let this = Arc::new_cyclic(|w| Self { paletteRecords: None, colormapRecords: Some(colormapRecords), pixels: Vec::new(), width: 0, height: 0, me: w.clone() });
                *COLORMAP.lock().unwrap() = Some(this.clone());
                Ok(this)
            }
            _ => {
                if PALETTE.lock().unwrap().is_none() { return Err(Error::new(ErrorKind::Unsupported, "NotImplementedException")); }
                let width = r.readInt32()?;
                let height = r.readInt32()?;
                let count = width.wrapping_mul(height);
                if count < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")); }
                let indices = r.readBytes(count as usize)?;
                let mut pixels = Vec::with_capacity(indices.len() * 4);
                for &index in &indices { pixels.extend_from_slice(&Self::toLightPixel(32 << 3, index as usize)); }
                Ok(Arc::new_cyclic(|w| Self { paletteRecords: None, colormapRecords: None, pixels, width, height, me: w.clone() }))
            }
        }
    }

    /// C# `ToLightPixel`. `(light >> 3) & 0x1F` selects the colormap row.
    pub fn toLightPixel(light: i32, pixel: usize) -> Vec<u8> {
        let palette = PALETTE.lock().unwrap().clone().expect("NullReferenceException: Palette");
        let colormap = COLORMAP.lock().unwrap().clone().expect("NullReferenceException: Colormap");
        let row = ((light >> 3) & 0x1F) as usize;
        let mapped = colormap.colormapRecords.as_ref().unwrap()[row][pixel] as usize;
        palette.paletteRecords.as_ref().unwrap()[mapped].clone()
    }

    pub fn create<T>(&self, _platform: &str, func: impl FnOnce(Box<dyn Any>) -> T) -> io::Result<T> {
        Ok(func(Box::new(TextureAsBytes { bytes: self.pixels.clone(), format: Arc::new(Self::FORMAT) as AnyArc, spans: vec![] })))
    }
}

impl ITexture for Binary_Lmp {
    fn width(&self) -> i32 { self.width }
    fn height(&self) -> i32 { self.height }
    fn depth(&self) -> i32 { 0 }
    fn mipMaps(&self) -> i32 { 1 }
    fn texFlags(&self) -> TextureFlags { TextureFlags::empty() }
    fn create(&self, platform: &str, func: &mut dyn FnMut(&dyn Any)) -> io::Result<()> { Self::create(self, platform, |x| func(&*x)) }
}

impl IHaveMetaInfo for Binary_Lmp {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        content("TextureName", file, thisTexture(&self.me)),
        MetaInfo::new(Some("TextureName".into()), Tag::None, vec![
            node(format!("Width: {}", self.width)),
            node(format!("Height: {}", self.height)),
        ], false),
    ] }
}

// #endregion

// #region Binary_Mdl

/// C# `Binary_Mdl` reads nothing. `GetInfoNodes` says `UnknownFileModel`.
pub struct Binary_Mdl { me: Weak<Binary_Mdl> }

impl Binary_Mdl {
    pub fn factory<'a>(r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new_cyclic(|w| { let mut s = Self::new(r); s.me = w.clone(); s }))))
    }

    pub fn new(_r: &mut Stream) -> Self { Self { me: Weak::new() } }
}

impl IHaveMetaInfo for Binary_Mdl {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Text", file, Tag::Str("UnknownFileModel File".into())),
        MetaInfo::new(Some("UnknownFileModel".into()), Tag::None, vec![node("Records: 1".into())], false),
    ] }
}

// #endregion

// #region Binary_Pak

const P_MAGIC: u32 = 0x4b434150; // PACK

#[repr(C, packed)]
#[derive(Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct P_Header {
    pub Magic: u32,
    pub DirOffset: i32,
    pub DirLength: i32,
}
impl IStruct for P_Header { const Struct: (&'static str, usize) = ("<I2i", 12); }

#[repr(C, packed)]
#[derive(Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct P_File {
    pub Path: [u8; 56],
    pub Offset: i32,
    pub FileSize: i32,
}
impl IStruct for P_File { const Struct: (&'static str, usize) = ("<56s2i", 64); }

pub struct Binary_Pak;

impl Binary_Pak {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_Pak));
        CURRENT.clone()
    }
}

impl ArcBinary for Binary_Pak {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, _tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let r = reader(r)?;
            let header = r.readS::<P_Header>()?;
            if header.Magic != P_MAGIC { return Err(Error::new(ErrorKind::InvalidData, "BAD MAGIC")); }
            if header.DirLength < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException: DirLength")); }
            if header.DirOffset < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException: DirOffset")); }
            let numFiles = (header.DirLength as usize) / size_of::<P_File>();
            r.seek(SeekFrom::Start(header.DirOffset as u64))?;
            let entries = r.readSArray::<P_File>(numFiles, size_of::<P_File>() as i32)?;
            let mut files = Vec::with_capacity(entries.len());
            for s in entries {
                let path = FixedAStringScan(&s.Path, 56).replace('\\', "/");
                let offset = s.Offset;
                let fileSize = s.FileSize;
                let mut file = FileSource { path, offset: offset as i64, fileSize: fileSize as i64, ..Default::default() };
                if endsWithIgnoreCase(&file.path, ".wad") {
                    let parent = source.base.this().downcastBinaryArchive().ok_or_else(|| Error::new(ErrorKind::InvalidData, "source"))?;
                    let child = Arc::new(file.clone());
                    file.arc = Some(SubArchive::new(&parent, Some(child), &file.path, Tag::None, Binary_Wad::current()));
                }
                files.push(Arc::new(file));
            }
            source.setFiles(Some(files));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, _source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, _option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async move {
            let r = reader(r)?;
            if file.offset < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException: Offset")); }
            if file.fileSize < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException: FileSize")); }
            r.seek(SeekFrom::Start(file.offset as u64))?;
            Ok(Some(Box::new(Cursor::new(r.readBytes(file.fileSize as usize)?)) as Stream))
        })
    }
}

// #endregion

// #region Binary_Spr

#[repr(i32)]
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Binary_SprType { VP_PARALLEL_UPRIGHT = 0, FACING_UPRIGHT = 1, VP_PARALLEL = 2, ORIENTED = 3, VP_PARALLEL_ORIENTED = 4 }

#[repr(i32)]
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Binary_SprTextFormat { SPR_NORMAL = 0, SPR_ADDITIVE = 1, SPR_INDEXALPHA = 2, SPR_ALPHTEST = 3 }

#[repr(i32)]
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Binary_SprSynchType { Synchronized = 0, Random = 1 }

#[repr(C)]
#[derive(Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct X_Header {
    pub Magic: u32,
    pub Version: i32,
    pub Type: i32,
    pub TextFormat: i32,
    pub BoundingRadius: f32,
    pub MaxWidth: i32,
    pub MaxHeight: i32,
    pub NumFrames: i32,
    pub BeamLen: f32,
    pub SynchType: i32,
}
impl IStruct for X_Header { const Struct: (&'static str, usize) = ("<I3if3ifi", 40); }

#[repr(C)]
#[derive(Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct X_Frame {
    pub Group: i32,
    pub OriginX: i32,
    pub OriginY: i32,
    pub Width: i32,
    pub Height: i32,
}
impl IStruct for X_Frame { const Struct: (&'static str, usize) = ("<5i", 20); }

struct SprState { frame: usize, bytes: Vec<u8> }

/// Half-Life / Quake sprite (`IDSP`). `NextFrame` always returns true.
pub struct Binary_Spr {
    frames: Vec<X_Frame>,
    pixels: Vec<Vec<u8>>,
    palette: Vec<u8>,
    state: Mutex<SprState>,
    me: Weak<Binary_Spr>,
}

impl Binary_Spr {
    const FORMAT: (TextureFormat, TexturePixel) = (TextureFormat::RGBA32, TexturePixel::Unknown);

    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Self::new(r, &f).map(|s| Tag::Info(s)))
    }

    pub fn new(r: &mut Stream, _f: &FileSource) -> io::Result<Arc<Self>> {
        let header = r.readS::<X_Header>()?;
        if header.Magic != 0x50534449 { return Err(Error::new(ErrorKind::InvalidData, "BAD MAGIC")); }
        let colors = r.readUInt16()? as usize;
        let palette = r.readBytes(colors * 3)?;
        if header.NumFrames < 0 { return Err(Error::new(ErrorKind::InvalidInput, "OverflowException")); }
        let mut frames = Vec::with_capacity(header.NumFrames as usize);
        let mut pixels = Vec::with_capacity(header.NumFrames as usize);
        for _ in 0..header.NumFrames {
            let frame = r.readS::<X_Frame>()?;
            let count = frame.Width.wrapping_mul(frame.Height);
            if count < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")); }
            pixels.push(r.readBytes(count as usize)?);
            frames.push(frame);
        }
        let frame0 = *frames.first().ok_or_else(|| Error::new(ErrorKind::InvalidData, "IndexOutOfRangeException"))?;
        let n = frame0.Width.wrapping_mul(frame0.Height).wrapping_shl(2);
        if n < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")); }
        let bytes = vec![0u8; n as usize];
        Ok(Arc::new_cyclic(|w| Self { frames, pixels, palette, state: Mutex::new(SprState { frame: 0, bytes }), me: w.clone() }))
    }

    fn advance(&self) -> bool {
        let mut state = self.state.lock().unwrap();
        let p = &self.pixels[state.frame];
        let _ = blitByPalette(&mut state.bytes, 4, p, &self.palette, 3, None);
        state.frame += 1;
        true
    }

    pub fn create<T>(&self, _platform: &str, func: impl FnOnce(Box<dyn Any>) -> T) -> io::Result<T> {
        let state = self.state.lock().unwrap();
        Ok(func(Box::new(TextureAsBytes { bytes: state.bytes.clone(), format: Arc::new(Self::FORMAT) as AnyArc, spans: vec![] })))
    }
}

impl ITexture for Binary_Spr {
    fn width(&self) -> i32 { self.frames.first().map(|s| s.Width).unwrap_or(0) }
    fn height(&self) -> i32 { self.frames.first().map(|s| s.Height).unwrap_or(0) }
    fn depth(&self) -> i32 { 0 }
    fn mipMaps(&self) -> i32 { 1 }
    fn texFlags(&self) -> TextureFlags { TextureFlags::empty() }
    fn fps(&self) -> i32 { 60 }
    fn hasFrames(&self) -> bool { self.state.lock().unwrap().frame < self.frames.len() }
    fn nextFrame(&self) -> bool { self.advance() }
    fn create(&self, platform: &str, func: &mut dyn FnMut(&dyn Any)) -> io::Result<()> { Self::create(self, platform, |x| func(&*x)) }
}

impl ITextureFrames for Binary_Spr {
    fn fps(&self) -> i32 { 60 }
    fn hasFrames(&self) -> bool { ITexture::hasFrames(self) }
    fn nextFrame(&mut self) -> bool { self.advance() }
}

impl IHaveMetaInfo for Binary_Spr {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        content("VideoTexture", file, thisTexture(&self.me)),
        MetaInfo::new(Some("Sprite".into()), Tag::None, vec![
            node(format!("Frames: {}", self.frames.len())),
            node(format!("Width: {}", self.width())),
            node(format!("Height: {}", self.height())),
            node(format!("Mipmaps: {}", self.mipMaps())),
        ], false),
    ] }
}

// #endregion

// #region Binary_Wad

const W_MAGIC: u32 = 0x32444157; // WAD2

#[repr(C, packed)]
#[derive(Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct W_Header {
    pub Magic: u32,
    pub LumpCount: i32,
    pub LumpOffset: i32,
}
impl IStruct for W_Header { const Struct: (&'static str, usize) = ("<I2i", 12); }

#[repr(C, packed)]
#[derive(Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct W_Lump {
    pub Offset: i32,
    pub PackedSize: i32,
    pub FileSize: i32,
    pub Type: u8,
    pub Compression: u8,
    pub Padding0: u8,
    pub Padding1: u8,
    pub Path: [u8; 16],
}
impl IStruct for W_Lump { const Struct: (&'static str, usize) = ("<3i4b16s", 32); }

pub struct Binary_Wad;

impl Binary_Wad {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_Wad));
        CURRENT.clone()
    }
}

impl ArcBinary for Binary_Wad {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, _tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let r = reader(r)?;
            let header = r.readS::<W_Header>()?;
            if header.Magic != W_MAGIC { return Err(Error::new(ErrorKind::InvalidData, "BAD MAGIC")); }
            if header.LumpCount < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException: LumpCount")); }
            if header.LumpOffset < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException: LumpOffset")); }
            r.seek(SeekFrom::Start(header.LumpOffset as u64))?;
            let lumps = r.readSArray::<W_Lump>(header.LumpCount as usize, size_of::<W_Lump>() as i32)?;
            let files = lumps.into_iter().map(|s| {
                let name = FixedAStringScan(&s.Path, 16).replace('\\', "/");
                Arc::new(FileSource {
                    path: format!("{name}.tex"),
                    hash: s.Type as u64,
                    offset: s.Offset as i64,
                    packedSize: s.PackedSize as i64,
                    fileSize: s.FileSize as i64,
                    compressed: s.Compression as i32,
                    ..Default::default()
                })
            }).collect();
            source.setFiles(Some(files));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, option: &'a Tag) -> Task<'a, Option<Stream>> {
        // A temporary `Binary_Pak` would not live as long as the returned task.
        static PAK: Binary_Pak = Binary_Pak;
        PAK.readData(source, r, file, option)
    }
}

// #endregion

#[cfg(test)]
mod tests {
    use super::*;
    use gamex::binary::{getResult, BinaryState};
    use gamex::family::FamilyGame;
    use gamex::meta::Tag;
    use openx_gfx::gfx::{ITexture, TextureAsBytes};
    use openx_vfx::vfx::{DirectoryFileSystem, VfsRead};
    use std::io::{Cursor, Read};

    fn archive(binary: Arc<dyn ArcBinary>) -> Arc<BinaryArchive> {
        let state = BinaryState::new(Arc::new(DirectoryFileSystem::new("", None)), FamilyGame::empty(), None, Some("a.pak"), Tag::None);
        newArchive(BinaryArchive::new(None, state, Some(binary)))
    }

    fn pakBytes(files: &[(&str, &[u8])]) -> Vec<u8> {
        let mut data = Vec::new();
        let mut dir = Vec::new();
        for (name, bytes) in files {
            let offset = (12 + data.len()) as i32;
            data.extend_from_slice(bytes);
            let mut path = [0u8; 56];
            path[..name.len()].copy_from_slice(name.as_bytes());
            dir.extend_from_slice(&path);
            dir.extend_from_slice(&offset.to_le_bytes());
            dir.extend_from_slice(&(bytes.len() as i32).to_le_bytes());
        }
        let mut out = Vec::new();
        out.extend_from_slice(&P_MAGIC.to_le_bytes());
        out.extend_from_slice(&((12 + data.len()) as i32).to_le_bytes());
        out.extend_from_slice(&(dir.len() as i32).to_le_bytes());
        out.extend(data);
        out.extend(dir);
        out
    }

    #[test]
    fn structSizesMatchThePatterns() {
        assert_eq!(size_of::<P_Header>(), 12);
        assert_eq!(size_of::<P_File>(), 64);
        assert_eq!(size_of::<W_Header>(), 12);
        assert_eq!(size_of::<W_Lump>(), 32);
    }

    #[test]
    fn pakListsSlashesAndReadsBytesAndOpensAWadChild() {
        let binary = Binary_Pak::current();
        let arc = archive(binary.clone());
        let mut cursor = Cursor::new(pakBytes(&[("a\\b.txt", b"hi"), ("x.WAD", b"wad")]));
        getResult(binary.read(&arc, Some(&mut cursor as &mut dyn VfsRead), &Tag::None)).unwrap();
        let files = arc.files().unwrap();
        assert_eq!(files[0].path, "a/b.txt");
        assert_eq!((files[0].offset, files[0].fileSize), (12, 2));
        assert!(files[0].arc.is_none());
        assert_eq!(files[1].path, "x.WAD");
        assert!(files[1].arc.is_some());
        cursor.set_position(0);
        let mut data = getResult(binary.readData(&arc, Some(&mut cursor as &mut dyn VfsRead), &files[0], &Tag::None)).unwrap().unwrap();
        let mut got = Vec::new();
        data.read_to_end(&mut got).unwrap();
        assert_eq!(got, b"hi");
    }

    #[test]
    fn pakRejectsABadMagic() {
        let binary = Binary_Pak::current();
        let arc = archive(binary.clone());
        let mut cursor = Cursor::new(vec![0u8; 12]);
        let err = getResult(binary.read(&arc, Some(&mut cursor as &mut dyn VfsRead), &Tag::None)).unwrap_err();
        assert!(err.to_string().contains("BAD MAGIC"), "{err}");
    }

    #[test]
    fn wadAppendsTexAndReadsTheLump() {
        let mut lump = Vec::new();
        lump.extend_from_slice(&44i32.to_le_bytes());
        lump.extend_from_slice(&2i32.to_le_bytes());
        lump.extend_from_slice(&2i32.to_le_bytes());
        lump.extend_from_slice(&[7, 1, 0, 0]);
        let mut name = [0u8; 16];
        name[..4].copy_from_slice(b"door");
        lump.extend_from_slice(&name);
        let mut bytes = Vec::new();
        bytes.extend_from_slice(&W_MAGIC.to_le_bytes());
        bytes.extend_from_slice(&1i32.to_le_bytes());
        bytes.extend_from_slice(&12i32.to_le_bytes());
        bytes.extend_from_slice(&lump);
        bytes.extend_from_slice(b"yz");
        let binary = Binary_Wad::current();
        let arc = archive(binary.clone());
        let mut cursor = Cursor::new(bytes);
        getResult(binary.read(&arc, Some(&mut cursor as &mut dyn VfsRead), &Tag::None)).unwrap();
        let files = arc.files().unwrap();
        assert_eq!(files[0].path, "door.tex");
        assert_eq!((files[0].hash, files[0].compressed, files[0].packedSize, files[0].fileSize), (7, 1, 2, 2));
        cursor.set_position(0);
        let mut data = getResult(binary.readData(&arc, Some(&mut cursor as &mut dyn VfsRead), &files[0], &Tag::None)).unwrap().unwrap();
        let mut got = Vec::new();
        data.read_to_end(&mut got).unwrap();
        assert_eq!(got, b"yz");
    }

    #[test]
    fn unportedFactoriesFailWhenCalled() {
        let empty = <dyn Archive>::empty();
        let file = Arc::new(FileSource::default());
        let mut stream: Stream = Box::new(Cursor::new(Vec::<u8>::new()));
        let err = getResult(Binary_BspX::factory(&mut stream, file, &*empty)).unwrap_err();
        assert!(err.to_string().contains("Binary_BspX isn't ported"), "{err}");
    }

    #[test]
    fn lmpPaintsThroughTheColormap() {
        *PALETTE.lock().unwrap() = None;
        *COLORMAP.lock().unwrap() = None;
        let empty = <dyn Archive>::empty();
        let file = |path: &str| Arc::new(FileSource { path: path.into(), ..Default::default() });
        let mut stream: Stream = Box::new(Cursor::new(Vec::<u8>::new()));
        let err = getResult(Binary_Lmp::factory(&mut stream, file("gfx/box.lmp"), &*empty)).unwrap_err();
        assert!(err.to_string().contains("NotImplementedException"), "{err}");

        let mut pal = vec![0u8; 256 * 3];
        pal[21..24].copy_from_slice(&[1, 2, 3]);
        let mut stream: Stream = Box::new(Cursor::new(pal));
        getResult(Binary_Lmp::factory(&mut stream, file("palette.lmp"), &*empty)).unwrap();
        let mut map = vec![0u8; 32 * 256];
        map[4] = 7;
        let mut stream: Stream = Box::new(Cursor::new(map));
        getResult(Binary_Lmp::factory(&mut stream, file("gfx/colormap.lmp"), &*empty)).unwrap();

        let mut pic = Vec::new();
        pic.extend_from_slice(&1i32.to_le_bytes());
        pic.extend_from_slice(&1i32.to_le_bytes());
        pic.push(4);
        let picFile = file("gfx/bigbox.lmp");
        let mut stream: Stream = Box::new(Cursor::new(pic));
        let tag = getResult(Binary_Lmp::factory(&mut stream, picFile.clone(), &*empty)).unwrap();
        let Tag::Info(info) = tag else { panic!("info"); };
        let nodes = info.getInfoNodes(None, Some(&picFile), &Tag::None);
        assert_eq!(nodes[1].items[0].name.as_deref(), Some("Width: 1"));
        let Tag::MetaContent(content) = &nodes[0].tag else { panic!("content"); };
        let Tag::Texture(tex) = &content.value else { panic!("texture"); };
        let mut got = None;
        tex.create("", &mut |any| { got = any.downcast_ref::<TextureAsBytes>().map(|s| s.bytes.clone()); }).unwrap();
        assert_eq!(got.unwrap(), vec![1, 2, 3, 0]);
        assert_eq!(Binary_Lmp::toLightPixel(32 << 3, 4), vec![1, 2, 3, 0]);
    }

    #[test]
    fn mdlReadsNothingAndNamesTheModel() {
        let empty = <dyn Archive>::empty();
        let file = Arc::new(FileSource { path: "a.mdl".into(), ..Default::default() });
        let mut stream: Stream = Box::new(Cursor::new(Vec::<u8>::new()));
        let tag = getResult(Binary_Mdl::factory(&mut stream, file.clone(), &*empty)).unwrap();
        let Tag::Info(info) = tag else { panic!("info"); };
        let nodes = info.getInfoNodes(None, Some(&file), &Tag::None);
        let Tag::MetaContent(content) = &nodes[0].tag else { panic!("content"); };
        assert_eq!(content.value.asStr(), Some("UnknownFileModel File"));
        assert_eq!(nodes[1].items[0].name.as_deref(), Some("Records: 1"));
    }

    #[test]
    fn sprBlitsThePaletteAndRejectsABadMagic() {
        let empty = <dyn Archive>::empty();
        let mut bad: Stream = Box::new(Cursor::new(vec![0u8; 40]));
        let err = getResult(Binary_Spr::factory(&mut bad, Arc::new(FileSource::default()), &*empty)).unwrap_err();
        assert!(err.to_string().contains("BAD MAGIC"), "{err}");

        let mut bytes = Vec::new();
        bytes.extend_from_slice(&0x50534449u32.to_le_bytes());
        bytes.extend_from_slice(&1i32.to_le_bytes());
        bytes.extend_from_slice(&0i32.to_le_bytes());
        bytes.extend_from_slice(&0i32.to_le_bytes());
        bytes.extend_from_slice(&0f32.to_le_bytes());
        bytes.extend_from_slice(&1i32.to_le_bytes());
        bytes.extend_from_slice(&1i32.to_le_bytes());
        bytes.extend_from_slice(&1i32.to_le_bytes());
        bytes.extend_from_slice(&0f32.to_le_bytes());
        bytes.extend_from_slice(&0i32.to_le_bytes());
        bytes.extend_from_slice(&1u16.to_le_bytes());
        bytes.extend_from_slice(&[9, 8, 7]);
        for _ in 0..3 { bytes.extend_from_slice(&0i32.to_le_bytes()); }
        bytes.extend_from_slice(&1i32.to_le_bytes());
        bytes.extend_from_slice(&1i32.to_le_bytes());
        bytes.push(0);
        let file = Arc::new(FileSource { path: "a.spr".into(), ..Default::default() });
        let mut stream: Stream = Box::new(Cursor::new(bytes));
        let tag = getResult(Binary_Spr::factory(&mut stream, file.clone(), &*empty)).unwrap();
        let Tag::Info(info) = tag else { panic!("info"); };
        let nodes = info.getInfoNodes(None, Some(&file), &Tag::None);
        assert_eq!(nodes[1].items[0].name.as_deref(), Some("Frames: 1"));
        let Tag::MetaContent(content) = &nodes[0].tag else { panic!("content"); };
        let Tag::Texture(tex) = &content.value else { panic!("texture"); };
        assert!(tex.hasFrames());
        assert_eq!(tex.fps(), 60);
        assert!(tex.nextFrame());
        assert!(!tex.hasFrames());
        let mut got = None;
        tex.create("", &mut |any| { got = any.downcast_ref::<TextureAsBytes>().map(|s| s.bytes.clone()); }).unwrap();
        assert_eq!(got.unwrap(), vec![9, 8, 7, 255]);
    }
}
