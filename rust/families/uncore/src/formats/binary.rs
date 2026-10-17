// PORT-SOURCE: Families/GameX.Uncore/Formats/Binary.cs
// PORT-SHA: 26b9cbb734746358
// PORT-STATUS: done
//
// Ported: Binary_Bik, Binary_Dds, Binary_Fsb, Binary_Iif, Binary_Img, Binary_Msg, Binary_Pal, Binary_Pcx,
// Binary_Plist, Binary_Raw, Binary_Snd, Binary_Tga, Binary_Txt, Binary_Xga, Binary_Zip, Binary_TestTri,
// Binary_Engine.
//
// Binary_Engine reads six ASCII fields (`archive:query:cell:interior:position:rotation`).
// A byte above 127 becomes `?`, as `Encoding.ASCII` does. `pr` of length `<= 3` is
// `Quaternion.Identity`; a longer list uses the first four components. `GetArchive` of a
// name that is not one archive is a `MultiArchive`, and the `BinaryArchive` cast then fails.
// `IQueryFunc` is `openx::manager::IQueryFunc`. `ArcBinary::asQueryFunc` is the cast; the
// default is the failure. Bethesda's `Binary_Esm` is the implementor and is still unported,
// so a real query archive fails `InvalidCastException: IQueryFunc`. The `MetaContent` value
// is `Arc<dyn CellDatabase + Send + Sync>` inside `Tag::Any`. `GetInfoNodes` omits
// `CellInterior` and `PlayerRotation`. `{Query}` prints `IQuery` (`Box<dyn Query>` has no
// C# runtime type name). `{Archive}` prints `GameX.BinaryArchive`. `int`/`float` parse as
// invariant (trim, `.` decimal), not the current culture. Uncore wires `.start`.
//
// Python's `Binary_Engine` is behind and was left as it is: `bool("False")` is true,
// identity is `len(pr) == 1` (C# is `<= 3`, so length 0, 2, or 3 throws there), and
// `getQuery` is called with no `IQueryFunc` cast.
//
// Binary_Zip: `zip` 4.2.0 (`ZipArchive`) stands in for both `System.IO.Compression.ZipArchive`
// (null key) and SharpZipLib `ZipFile` (any other key). The bytes are copied out of the borrowed
// reader. C# `new ZipArchive(stream, Read)` takes that stream (`leaveOpen` is false) and disposes
// it with the archive. A string key is `ZipFile.Password`, passed as UTF-8 to `by_index_decrypt`.
// A `byte[]` key is SharpZipLib's private `Key` (the traditional cipher key, not a password). The
// crate cannot set that, so an encrypted entry then returns `NotSupportedException: ZipFile.Key`
// instead of decrypting. `!CanRead` is not reachable: an opened `zip` reader implements `Read`.
// The commented `catch` and the commented `Write` / `WriteData` stay out. Uncore does not register
// `.zip`. Python's `Binary_Zip` is a smaller `zipfile` reader: no UseSystem split, no flags, a
// `bytes` key raises `NotImplementedError`, and the `match` tests the name `key` rather than `self.key`.
//
// Binary_Plist reads `PlistReader` (`formats/apple/plist_reader.rs`). Uncore does not register a
// `.plist` extension, so this type is not wired into `UncoreArchive`. Python has no Uncore twin;
// Capcom's `Binary_Plist` is a different unimplemented type.
//
// Binary_Dds: `DDS_HEADER.read` parses the file. `EngineX.Caps.ReadDds` still returns the raw
// bytes as `TextureAsDds`. An empty mip assigns C# `-1..`, and `System.Index` rejects a negative
// start, so that path returns `ArgumentOutOfRangeException`. Python stores `range(-1, 0)`.
//
// Shapes:
// - A C# `Factory` returning `Task.FromResult(new Binary_X(...))` is an associated fn `factory`
//   (an `AssetFactory`) that builds the object synchronously, as C# does, and returns a ready task.
// - `Binary_Msg.Factory(string)` and `Binary_Raw.FactoryMethod(...)` return a C# `Func` that
//   captures its arguments. `AssetFactory` is a plain `fn`, so they return a boxed closure of the
//   same signature; a caller wraps it in its own `fn` to use it as an `AssetFactory`.
// - `MetaContent { Value = this }`: each such type keeps a `me: Weak<Self>`, set when its factory
//   builds the `Arc`, and the content's value is that `Arc` as `Tag::Any` (same object, as in C#),
//   or `Tag::Texture` for a texture, which the engine view takes as `ITexture`.
// - `ITexture.Create<T>(string platform, Func<object, T> func)`: each texture has an inherent
//   `create` passing the `TextureAsBytes` as `Box<dyn Any>`, returning `io::Result` where C# can
//   throw, and `ITexture::create` forwards to it.
// - Nested C# types are at module level under their own name; where two would collide they are
//   prefixed with the enclosing type (`Binary_PcxX_Header`, `Binary_TgaX_Header`, `Binary_RawTag`).
// - C# exceptions are `io::Error`s. `Log.Info` is `openx_poly::log::Log::info`.
//
// Stand-ins:
// - Binary_Img: the `image` crate stands in for System.Drawing's `Bitmap`. It hands back tightly
//   packed RGB/RGBA bytes (GDI+'s `LockBits` gives BGR/BGRA rows padded to `Stride`) and expands
//   palettes itself, so `Raster.BlitByPalette` isn't needed. Its color type maps to the
//   `TextureFormat` C#'s `PixelFormat` switch picks: L16 is Format16bppGrayScale, L8 is
//   Format8bppIndexed (GDI+ loads 8-bit gray as indexed with a gray palette; expanded to RGB here,
//   as the blit does), Rgb8 is Format24bppRgb, Rgba8 is Format32bppArgb. `image` has nothing like
//   Format32bppRgb. GIFs decode to Rgba8 (ARGB32) where GDI+ gives Format8bppIndexed (RGB24).
//   C# keeps the `Bitmap` in an `IDisposable Image` field; here the decoded image is dropped.
// - Binary_Iif: `MidiEvent` is a C# class in a linked list, so it is `Arc<Mutex<MidiEvent>>`.
//   C#'s `BinaryWriter` `Write`/`WriteE` are `write_all` of `to_le_bytes`/`to_be_bytes`.
// - Binary_Xga: `Decode1` returns null; `TextureAsBytes.bytes` is a `Vec`, so it is empty.

use std::any::Any;
use std::io::{self, Cursor, Error, ErrorKind, Read, Seek, SeekFrom, Write};
use std::ops::Range;
use std::sync::atomic::Ordering;
use std::sync::{Arc, LazyLock, Mutex, Weak};
use bytemuck::{Pod, Zeroable};
use gamex::binary::{ArcBinary, Archive, ArchiveExt, BinaryArchive, Stream, Task};
use openx_vfx::vfx::VfsRead;
use crate::formats::apple::plist_reader::{PlistObject, PlistReader};
use gamex::family::FamilyGame;
use gamex::meta::{fileName, extension, FileSource, IHaveMetaInfo, MetaContent, MetaInfo, MetaManager, Tag};
use glam::{Quat, Vec3};
use openx::manager::{CellDatabase, SharedQuery};
use openx_poly::core::SourceRef;
use openx_poly::poly::Int3;
use openx::engine::EngineX::{self, Caps};
use openx_poly::log::Log;
use openx_gfx::gfx::{AnyArc, ITexture, TextureAsBytes, TextureAsDds};
use openx_gfx::gfx_texture::{DdsFormat, TextureFlags, TextureFormat, TexturePixel, DDSCAPS, DDSCAPS2, DDSD, DDPF, DDS_HEADER, DDS_PIXELFORMAT, FourCC};
use openx_poly::prelude::{BinaryReaderExt, IStruct};
use zip::ZipArchive;

/// C# `Task.FromResult((object)value)`.
fn fromResult<'a>(value: io::Result<Tag>) -> Task<'a, Tag> { Box::pin(std::future::ready(value)) }

/// Builds the `Arc` a factory returns, giving the object its `me`.
fn arc<T: IHaveMetaInfo>(mut s: T, me: fn(&mut T) -> &mut Weak<T>) -> Tag { Tag::Info(Arc::new_cyclic(|w| { *me(&mut s) = w.clone(); s })) }

/// C# `Value = this`.
fn this<T: Any + Send + Sync>(me: &Weak<T>) -> Tag { me.upgrade().map_or(Tag::None, |s| Tag::Any(s)) }
/// `Value = this` for a texture: the engine view takes it as `ITexture`.
fn thisTexture<T: ITexture + 'static>(me: &Weak<T>) -> Tag { me.upgrade().map_or(Tag::None, |s| Tag::Texture(s)) }

/// C# `Path.GetFileName(file.Path)`.
fn name(file: Option<&Arc<FileSource>>) -> String { file.map_or(String::new(), |s| fileName(&s.path)) }

fn node(name: String) -> MetaInfo { MetaInfo::new(Some(name), Tag::None, vec![], false) }
fn content(type_: &str, file: Option<&Arc<FileSource>>, value: Tag) -> MetaInfo { MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent { type_: type_.into(), name: name(file), value, ..Default::default() })), vec![], false) }

/// C# `Func<BinaryReader, FileSource, Archive, Task<object>>`, as a closure.
pub type FuncAsset = Box<dyn for<'a> Fn(&'a mut Stream, Arc<FileSource>, &'a dyn Archive) -> Task<'a, Tag> + Send + Sync>;
fn funcAsset<F: for<'a> Fn(&'a mut Stream, Arc<FileSource>, &'a dyn Archive) -> Task<'a, Tag> + Send + Sync + 'static>(f: F) -> FuncAsset { Box::new(f) }

// #region Binary_Bik

/// The file format for Bik Video
/// see https://en.wikipedia.org/wiki/Bink_Video
pub struct Binary_Bik {
    pub data: Vec<u8>,
}

impl Binary_Bik {
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { fromResult(Binary_Bik::new(r, f.fileSize as i32).map(|s| Tag::Info(Arc::new(s)))) }

    pub fn new(r: &mut Stream, fileSize: i32) -> io::Result<Self> { Ok(Self { data: r.readBytes(fileSize as usize)? }) }
}

impl IHaveMetaInfo for Binary_Bik {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Text", file, Tag::Str("BIK Video".into())),
    ] }
}

// #endregion

// #region Binary_Dds

// https://github.com/paroj/nv_dds/blob/master/nv_dds.cpp
/// The file format for DirectDraw Surface
/// see https://en.wikipedia.org/wiki/DirectDraw
pub struct Binary_Dds {
    capsReadDds: bool,
    bytes: Vec<u8>,
    spans: Vec<Range<usize>>,
    format: DdsFormat,
    pub width: i32,
    pub height: i32,
    pub depth: i32,
    pub mipMaps: i32,
    pub texFlags: TextureFlags,
    me: Weak<Binary_Dds>,
}

impl Binary_Dds {
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { fromResult(Binary_Dds::new(r, &f, true).map(|s| arc(s, |s| &mut s.me))) }

    pub fn new(r: &mut Stream, f: &FileSource, readMagic: bool) -> io::Result<Self> {
        let capsReadDds = EngineX::Current().caps.contains(Caps::ReadDds);
        if capsReadDds {
            let bytes = r.readBytes(f.fileSize as usize)?;
            return Ok(Self { capsReadDds, bytes, spans: vec![], format: DdsFormat { type_: String::new(), blockSize: 0, value: (TextureFormat::Unknown, TexturePixel::Unknown) }, width: 0, height: 0, depth: 0, mipMaps: 0, texFlags: TextureFlags::empty(), me: Weak::new() });
        }
        let (header, _dxt10, format, bytes) = DDS_HEADER::read(r, readMagic)?;
        let mipMaps = 1.max(header.dwMipMapCount as i32);
        let (width, height) = (header.dwWidth as i32, header.dwHeight as i32);
        let mut spans = Vec::with_capacity(mipMaps as usize);
        let mut offset = 0i32;
        for i in 0..mipMaps {
            let (w, h) = (width >> i, height >> i);
            let size = ((w + 3) / 4).wrapping_mul((h + 3) / 4).wrapping_mul(format.blockSize);
            let remains = size.min((bytes.len() as i32).wrapping_sub(offset));
            // C# `-1..` goes through `System.Index`, which rejects a negative start.
            if w == 0 || h == 0 || remains <= 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException: Non-negative number required.")); }
            spans.push(offset as usize..(offset + remains) as usize);
            offset += remains;
        }
        Ok(Self { capsReadDds, bytes, spans, format, width, height, depth: 0, mipMaps, texFlags: TextureFlags::empty(), me: Weak::new() })
    }

    // #region ITexture

    pub fn create<T>(&self, platform: &str, func: impl FnOnce(Box<dyn Any>) -> T) -> io::Result<T> {
        Ok(if self.capsReadDds { func(Box::new(TextureAsDds { bytes: self.bytes.clone() })) }
        else { func(Box::new(TextureAsBytes { bytes: self.bytes.clone(), format: Arc::new(self.format.value) as AnyArc, spans: self.spans.clone() })) })
    }

    // #endregion
}

impl ITexture for Binary_Dds {
    fn width(&self) -> i32 { self.width }
    fn height(&self) -> i32 { self.height }
    fn depth(&self) -> i32 { self.depth }
    fn mipMaps(&self) -> i32 { self.mipMaps }
    fn texFlags(&self) -> TextureFlags { self.texFlags }
    fn create(&self, platform: &str, func: &mut dyn FnMut(&dyn Any)) -> io::Result<()> { Self::create(self, platform, |x| func(&*x)) }
}

impl IHaveMetaInfo for Binary_Dds {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Texture", file, thisTexture(&self.me)),
        MetaInfo::new(Some("Texture".into()), Tag::None, vec![
            node(format!("Format: {}", self.format.type_)),
            node(format!("Width: {}", self.width)),
            node(format!("Height: {}", self.height)),
            node(format!("Mipmaps: {}", self.mipMaps)),
        ], false),
    ] }
}

// #endregion

// #region Binary_Fsb

/// The file format for FSB Audio from FMOD sound fx archives
/// see https://en.wikipedia.org/wiki/FMOD
pub struct Binary_Fsb {
    pub data: Vec<u8>,
}

impl Binary_Fsb {
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { fromResult(Binary_Fsb::new(r, f.fileSize as i32).map(|s| Tag::Info(Arc::new(s)))) }

    pub fn new(r: &mut Stream, fileSize: i32) -> io::Result<Self> { Ok(Self { data: r.readBytes(fileSize as usize)? }) }
}

impl IHaveMetaInfo for Binary_Fsb {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Text", file, Tag::Str("FSB B_Audio".into())),
    ] }
}

// #endregion

// #region Binary_Iif

// https://en.wikipedia.org/wiki/Interchange_File_Format

#[repr(C, packed)]
#[derive(Debug, Clone, Copy, Default, Pod, Zeroable)]
pub struct Chunk {
    pub id: u32,        // 'FORM' | 'CAT '
    pub size: i32,      // Size of the chunk
}

impl Chunk {
    pub const FORM: u32 = 0x4d524f46;
    pub const CAT_: u32 = 0x20544143;
    pub const XDIR: u32 = 0x52494458;
    pub const XMID: u32 = 0x44494d58;
    pub const INFO: u32 = 0x4f464e49;
    pub const TIMB: u32 = 0x424d4954;
    pub const EVNT: u32 = 0x544e5645;
    pub fn actualSize(&self) -> i32 { let size = self.size; size + (size & 1) }
}
impl IStruct for Chunk { const Struct: (&'static str, usize) = (">4xI", std::mem::size_of::<Chunk>()); }

#[repr(C, packed)]
#[derive(Debug, Clone, Copy, Default, Pod, Zeroable)]
pub struct TIMB {
    pub patchNumber: u8,
    pub timbreBank: u8,
}
impl IStruct for TIMB { const Struct: (&'static str, usize) = ("<2x", std::mem::size_of::<TIMB>()); }

/// C# `class MidiEvent`, shared as the linked list's nodes.
pub type MidiEventRef = Arc<Mutex<MidiEvent>>;

#[derive(Default)]
pub struct MidiEvent {
    pub time: i32,
    pub status: u8,
    pub data0: u8,
    pub data1: u8,
    pub length: i32,
    pub stream: Option<Vec<u8>>,
    pub next: Option<MidiEventRef>,
}

impl MidiEvent {
    pub fn new(time: i32) -> MidiEventRef { Arc::new(Mutex::new(MidiEvent { time, ..Default::default() })) }
}

/// C# `enum EV : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum EV {
    NOTE_OFF = 0x80,
    NOTE_ON = 0x90,
    POLY_PRESS = 0xa0,
    CONTROL = 0xb0,
    PROGRAM = 0xc0,
    CHAN_PRESS = 0xd0,
    PITCH = 0xe0,
    SYSEX = 0xf0,
    ESC = 0xf7,
    META = 0xff, // MetaEvent
}

impl EV {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn fromRaw(v: u8) -> Option<Self> {
        Some(match v {
            0x80 => Self::NOTE_OFF,
            0x90 => Self::NOTE_ON,
            0xa0 => Self::POLY_PRESS,
            0xb0 => Self::CONTROL,
            0xc0 => Self::PROGRAM,
            0xd0 => Self::CHAN_PRESS,
            0xe0 => Self::PITCH,
            0xf0 => Self::SYSEX,
            0xf7 => Self::ESC,
            0xff => Self::META,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn toRaw(self) -> u8 {
        self as u8
    }
}

/// C# `enum EVT : byte`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
enum EVT {
    OUTPUT_CABLE = 0x21,
    EOT = 0x2f,
    TEMPO = 0x51,
    TIME_SIG = 0x58,
    KEYSIG = 0x59,
}

impl EVT {
    const TRACK_END: EVT = EVT::EOT;
    const SET_TEMPO: EVT = EVT::TEMPO;
}

/// The file format for Interchange File Format
#[derive(Default)]
pub struct Binary_Iif {
    pub tracks: i32,
    pub events: Option<Vec<Option<MidiEventRef>>>,
    pub timing: Option<Vec<i16>>,
    pub timbres: Option<Vec<Vec<TIMB>>>,
    pub theList: Option<MidiEventRef>,
    curTrack: i32, // used during load of multi-track XMI's (e.g. syngame.xmi)
    curEvent: Option<MidiEventRef>,
    curEventList: Option<MidiEventRef>,
    me: Weak<Binary_Iif>,
}

impl Binary_Iif {
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { fromResult(Binary_Iif::new(r, f.fileSize as i32).map(|s| arc(s, |s| &mut s.me))) }

    pub fn new(r: &mut Stream, fileSize: i32) -> io::Result<Self> { let mut s = Self::default(); s.read(r, fileSize)?; Ok(s) }

    // #region Read

    fn allocData(&mut self) {
        if self.events.is_some() { return; }
        self.events = Some(vec![None; self.tracks as usize]);
        self.timing = Some(vec![0; self.tracks as usize]);
        self.timbres = Some(vec![Vec::new(); self.tracks as usize]);
    }

    fn read(&mut self, r: &mut Stream, fileSize: i32) -> io::Result<bool> {
        self.tracks = 1; // default to 1 track, in case there is no XDIR chunk
        loop {
            let chunk: Chunk = r.readS()?;
            let position = r.tell()? as i64;
            let result = match chunk.id {
                Chunk::FORM => self.handleChunkFORM(r, chunk.size)?,
                Chunk::CAT_ => self.handleChunkCAT(r, chunk.size)?,
                _ => false,
            };
            if !result { return Ok(false); } // something failed

            // seek
            let newPosition = position + chunk.actualSize() as i64;
            if r.tell()? as i64 != newPosition { r.seek(SeekFrom::Start((position + chunk.actualSize() as i64) as u64))?; }
            if !((r.tell()? as i64) < fileSize as i64) { break; }
        }
        Ok(true)
    }

    fn handleChunkFORM(&mut self, r: &mut Stream, chunkSize: i32) -> io::Result<bool> {
        Ok(match r.readUInt32()? {
            Chunk::XDIR => self.handleChunkXDIR(r, chunkSize - 4)?,
            Chunk::XMID => self.handleChunkXMID(r, chunkSize - 4)?,
            _ => false,
        })
    }

    fn handleChunkCAT(&mut self, r: &mut Stream, chunkSize: i32) -> io::Result<bool> {
        let basePosition = r.tell()? as i64;
        let endPosition = r.tell()? as i64 + chunkSize as i64;
        loop {
            let mut chunk: Chunk = r.readS()?;
            let mut position = r.tell()? as i64;
            if chunk.id == Chunk::XMID {
                r.skip(-4)?; position -= 4;
                chunk.size = (chunkSize as i64 - (position - basePosition)) as i32;
            }
            let result = match chunk.id {
                Chunk::FORM => self.handleChunkFORM(r, chunk.size)?,
                Chunk::XMID => self.handleChunkXMID(r, chunk.size)?,
                _ => false,
            };
            if !result { return Ok(false); } // something failed

            // seek
            let newPosition = position + chunk.actualSize() as i64;
            if r.tell()? as i64 != newPosition { r.seek(SeekFrom::Start((position + chunk.actualSize() as i64) as u64))?; }
            if !((r.tell()? as i64) < endPosition) { break; }
        }
        Ok(true)
    }

    fn handleChunkXDIR(&mut self, r: &mut Stream, chunkSize: i32) -> io::Result<bool> {
        if chunkSize != 10 { return Ok(false); }
        let chunk: Chunk = r.readS()?;
        let size = chunk.size;
        if size as i64 == Chunk::INFO as i64 || size != 2 { return Ok(false); }
        self.tracks = r.readUInt16()? as i32;
        self.allocData();
        Ok(true)
    }

    fn handleChunkXMID(&mut self, r: &mut Stream, chunkSize: i32) -> io::Result<bool> {
        let endPosition = r.tell()? as i64 + chunkSize as i64;
        loop {
            let chunk: Chunk = r.readS()?;
            let position = r.tell()? as i64;
            let result = match chunk.id {
                Chunk::FORM => self.handleChunkFORM(r, chunk.size)?,
                Chunk::TIMB => self.handleChunkTIMB(r, chunk.size)?,
                Chunk::EVNT => self.handleChunkEVNT(r, chunk.size)?,
                _ => false,
            };
            if !result { return Ok(false); } // something failed

            // seek
            let newPosition = position + chunk.actualSize() as i64;
            if r.tell()? as i64 != newPosition { r.seek(SeekFrom::Start((position + chunk.actualSize() as i64) as u64))?; }
            if !((r.tell()? as i64) < endPosition) { break; }
        }
        Ok(true)
    }

    fn handleChunkTIMB(&mut self, r: &mut Stream, chunkSize: i32) -> io::Result<bool> {
        let numTimbres = r.readUInt16()?;
        if ((numTimbres as i32) << 1) + 2 != chunkSize { return Ok(false); }
        self.timbres.as_mut().unwrap()[self.curTrack as usize] = r.readSArray::<TIMB>(numTimbres as usize, 0)?;
        Ok(true)
    }

    fn handleChunkEVNT(&mut self, r: &mut Stream, chunkSize: i32) -> io::Result<bool> {
        self.allocData(); // precaution, in case XDIR wasn't found
        self.curEventList = None;
        let timing = self.readEventList(r)?;
        if timing == 0 { Log::info("Unable to convert data\n"); return Ok(false); }
        self.timing.as_mut().unwrap()[self.curTrack as usize] = timing;
        self.events.as_mut().unwrap()[self.curTrack as usize] = self.curEventList.clone();
        self.curTrack += 1;
        Ok(true) // Return how many tracks were converted
    }

    fn createNewEvent(&mut self, time: i32) -> MidiEventRef {
        let Some(list) = &self.curEventList else { let z = MidiEvent::new(time); self.curEvent = Some(z.clone()); self.curEventList = Some(z.clone()); return z; };
        let list = list.clone();
        let mut cur = self.curEvent.clone().unwrap();
        if cur.lock().unwrap().time > time { cur = list; }
        loop {
            let Some(next) = cur.lock().unwrap().next.clone() else { break };
            if next.lock().unwrap().time > time {
                let z = MidiEvent::new(time); z.lock().unwrap().next = Some(next);
                cur.lock().unwrap().next = Some(z.clone());
                self.curEvent = Some(z.clone());
                return z;
            }
            cur = next;
        }
        let z = MidiEvent::new(time);
        cur.lock().unwrap().next = Some(z.clone());
        self.curEvent = Some(z.clone());
        z
    }

    fn convertEvent(&mut self, r: &mut Stream, time: i32, status: u8, size: i32) -> io::Result<()> {
        let mut current = self.createNewEvent(time);
        current.lock().unwrap().status = status;
        current.lock().unwrap().data0 = r.readByte()?; if size == 1 { return Ok(()); }
        current.lock().unwrap().data1 = r.readByte()?; if size == 2 { return Ok(()); }
        // save old
        let prev = current;
        let delta = Self::getVariableLengthQuantity(r)?;
        current = self.createNewEvent(time + delta * 3);
        { let mut c = current.lock().unwrap(); c.status = status; c.data0 = prev.lock().unwrap().data0; c.data1 = 0; }
        self.curEvent = Some(prev); // restore old
        Ok(())
    }

    fn convertSystemMessage(&mut self, r: &mut Stream, time: i32, status: u8) -> io::Result<()> {
        let current = self.createNewEvent(time);
        current.lock().unwrap().status = status;
        // handling of Meta events
        let curEvent = self.curEvent.clone().unwrap();
        if current.lock().unwrap().status == EV::META as u8 { curEvent.lock().unwrap().data0 = r.readByte()?; }
        let length = Self::getVariableLengthQuantity(r)?; curEvent.lock().unwrap().length = length;
        if length == 0 { return Ok(()); }
        curEvent.lock().unwrap().stream = Some(r.readBytes(length as usize)?);
        Ok(())
    }

    fn readEventList(&mut self, r: &mut Stream) -> io::Result<i16> {
        let mut time = 0;
        let mut tempo = 500000;
        let mut tempoSet = false;
        loop {
            let status = r.readByte()?;
            match EV::fromRaw(status & 0xF0) {
                // Note On/Off
                Some(EV::NOTE_OFF) => { Log::info("ERROR: Note off not valid in XMidiFile\n"); return Ok(0); }
                Some(EV::NOTE_ON) => self.convertEvent(r, time, status, 3)?,
                // 2 byte data, Aftertouch, Controller and Pitch Wheel
                Some(EV::POLY_PRESS) | Some(EV::CONTROL) | Some(EV::PITCH) => self.convertEvent(r, time, status, 2)?,
                // 1 byte data, Program Change and Channel Pressure
                Some(EV::PROGRAM) | Some(EV::CHAN_PRESS) => self.convertEvent(r, time, status, 1)?,
                // SysEx
                Some(EV::SYSEX) => {
                    if status == EV::META as u8 {
                        let evt = r.readByte()?;
                        match evt {
                            z if z == EVT::OUTPUT_CABLE as u8 => {}
                            z if z == EVT::TRACK_END as u8 => return Ok(((tempo * 9) / 25000) as i16), // End Of Track
                            z if z == EVT::SET_TEMPO as u8 => if !tempoSet { tempoSet = true; r.skip(1)?; tempo = (r.readByte()? as i32) << 16 | (r.readByte()? as i32) << 8 | r.readByte()? as i32; r.skip(-4)?; }, // Tempo. Need it for PPQN
                            z if z == EVT::TIME_SIG as u8 => {}
                            z if z == EVT::KEYSIG as u8 => {}
                            _ => {}
                        }
                        r.skip(-1)?;
                    }
                    self.convertSystemMessage(r, time, status)?;
                }
                _ => { // Delta T, also known as interval count
                    r.skip(-1)?;
                    let delta = Self::getVariableLengthQuantity2(r)?;
                    time += delta * 3;
                }
            }
        }
    }

    // #endregion

    // #region Write

    pub fn writeAll(&self) {
        for i in 0..self.tracks {
        }
    }

    fn writeMidi<W: Write + Seek>(&self, w: &mut W, track: i32) -> io::Result<bool> {
        const MIDI_MThd: u32 = 0x6468544d;
        let Some(events) = &self.events else { return Ok(false) };
        if track > self.tracks { return Ok(false); }

        // write header
        w.write_all(&MIDI_MThd.to_le_bytes())?;
        w.write_all(&6i32.to_be_bytes())?;
        w.write_all(&0i16.to_be_bytes())?;
        w.write_all(&1i16.to_be_bytes())?;
        w.write_all(&self.timing.as_ref().unwrap()[track as usize].to_be_bytes())?;

        // write tracks
        Self::writeMidiMTrk(w, events[track as usize].clone())
    }

    fn writeMidiMTrk<W: Write + Seek>(w: &mut W, evnts: Option<MidiEventRef>) -> io::Result<bool> {
        const MIDI_MTrk: u32 = 0x6b72544d;
        const XMIDI_CONTROLLER_NEXT_BREAK: u8 = 117;

        let mut delta; let mut time = 0;
        let mut lastStatus = 0u8;

        // This is set true to make the song end when an XMidiFile break is hit.
        let mut sshockBreak = false;
        w.write_all(&MIDI_MTrk.to_le_bytes())?;
        let sizePosition = w.stream_position()?;
        w.write_all(&0i32.to_le_bytes())?;
        let mut evnt = evnts;
        while let Some(e) = evnt {
            let evnt_ = e.lock().unwrap();
            // If sshock_break is set, the delta is only 0
            delta = if sshockBreak { 0 } else { evnt_.time - time };
            time = evnt_.time;

            // write delta
            Self::putVariableLengthQuantity(w, delta)?;

            // write status
            if (evnt_.status != lastStatus) || (evnt_.status >= EV::SYSEX as u8) { w.write_all(&[evnt_.status])?; }

            // write event
            lastStatus = evnt_.status;
            match EV::fromRaw(evnt_.status & 0xF0) {
                // 2 bytes data, Note off, Note on, Aftertouch and Pitch Wheel
                Some(EV::NOTE_OFF) | // invalid in XMID
                Some(EV::NOTE_ON) |
                Some(EV::POLY_PRESS) |
                Some(EV::PITCH) => { w.write_all(&[evnt_.data0])?; w.write_all(&[evnt_.data1])?; }
                // Controller, we need to catch XMIXI Breaks
                Some(EV::CONTROL) => {
                    w.write_all(&[evnt_.data0])?; w.write_all(&[evnt_.data1])?;
                    if evnt_.data0 == XMIDI_CONTROLLER_NEXT_BREAK { sshockBreak = true; } // XMidiFile Break
                }
                // 1 bytes data, Program Change and Channel Pressure
                Some(EV::PROGRAM) |
                Some(EV::CHAN_PRESS) => w.write_all(&[evnt_.data0])?,
                // Variable length, SysEx
                Some(EV::SYSEX) => {
                    if evnt_.status == EV::META as u8 { w.write_all(&[evnt_.data0])?; }
                    Self::putVariableLengthQuantity(w, evnt_.length)?;
                    if evnt_.length != 0 { w.write_all(evnt_.stream.as_deref().unwrap_or_default())?; }
                }
                // Never occur
                _ => Log::info("Not supposed to see this"),
            }
            evnt = evnt_.next.clone();
        }

        // write size
        let position = w.stream_position()?;
        let size = position - sizePosition;
        w.seek(SeekFrom::Start(sizePosition))?;
        w.write_all(&(size as u32).to_be_bytes())?;
        w.seek(SeekFrom::Start(position))?;
        Ok(true)
    }

    // #endregion

    // #region Utils

    // Get the MIDI variable-length quantity. A string of 7-bits/byte, terminated by a byte not having MSB set
    fn getVariableLengthQuantity(r: &mut Stream) -> io::Result<i32> {
        let mut value = 0i32;
        for i in 0..4 {
            value <<= 7;
            let b = r.readByte()?;
            value |= (b & 0x7F) as i32;
            if (b & 0x80) == 0 { break; }
        }
        Ok(value)
    }

    // Instead of treating consecutive delta/interval counts as separate counts, just sum them up until we hit a MIDI event.
    fn getVariableLengthQuantity2(r: &mut Stream) -> io::Result<i32> {
        let mut value = 0i32;
        let mut i = 0;
        while i < 4 { let b = r.readByte()?; if (b & 0x80) != 0 { break; } value += b as i32; i += 1; }
        r.skip(-1)?;
        Ok(value)
    }

    // Write a MIDI variable-length quantity (see getVlc) into 'stream'.
    // Returns # of bytes used to store 'value'.
    // Note: stream can be NULL (useful to count how much space a value would need)
    fn putVariableLengthQuantity<W: Write>(w: &mut W, mut value: i32) -> io::Result<()> {
        let mut buffer = value & 0x7F;
        loop {
            value >>= 7;
            if value == 0 { break; }
            buffer <<= 8;
            buffer |= (value & 0x7F) | 0x80;
        }
        loop {
            w.write_all(&[(buffer & 0xFF) as u8])?;
            if (buffer & 0x80) != 0 { buffer >>= 8; }
            else { break; }
        }
        Ok(())
    }

    // #endregion
}

impl IHaveMetaInfo for Binary_Iif {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Text", file, this(&self.me)),
    ] }
}

// #endregion

// #region Binary_Img

#[repr(C, packed)]
#[derive(Debug, Clone, Copy, Default, Pod, Zeroable)]
pub struct BmpHeader {
    pub type_: u16,             // 'BM'
    pub size: u32,              // File size in bytes
    pub reserved: u32,          // unused (=0)
    pub offBits: u32,           // Offset from beginning of file to the beginning of the bitmap data
    pub info: BmpInfoHeader,
}
impl IStruct for BmpHeader { const Struct: (&'static str, usize) = ("<H3i", std::mem::size_of::<BmpHeader>()); }

#[repr(C, packed)]
#[derive(Debug, Clone, Copy, Default, Pod, Zeroable)]
pub struct BmpInfoHeader {
    pub size: u32,              // Size of InfoHeader =40
    pub width: u32,             // Horizontal width of bitmap in pixels
    pub height: u32,            // Vertical height of bitmap in pixels
    pub planes: u16,            // Number of Planes (=1)
    pub bitCount: u16,          // Bits per Pixel used to store palette entry information.
    pub compression: u32,       // Type of Compression: 0 = BI_RGB no compression, 1 = BI_RLE8 8bit RLE encoding, 2 = BI_RLE4 4bit RLE encoding
    pub sizeImage: u32,         // (compressed) Size of Image - It is valid to set this =0 if Compression = 0
    pub xPixelsPerM: u32,       // orizontal resolution: Pixels/meter
    pub yPixelsPerM: u32,       // vertical resolution: Pixels/meter
    pub colorsUsed: u32,        // Number of actually used colors. For a 8-bit / pixel bitmap this will be 100h or 256.
    pub colorsImportant: u32,   // Number of important colors
}
impl IStruct for BmpInfoHeader { const Struct: (&'static str, usize) = ("<3I2H6I", std::mem::size_of::<BmpInfoHeader>()); }

/// The file format for Image
/// see https://en.wikipedia.org/wiki/Image
pub struct Binary_Img {
    capsDrawing: bool,
    bytes: Vec<u8>,
    // ITexture
    format: (String, (TextureFormat, TexturePixel)),
    pub width: i32,
    pub height: i32,
    pub depth: i32,
    pub mipMaps: i32,
    pub texFlags: TextureFlags,
    me: Weak<Binary_Img>,
}

impl Binary_Img {
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { fromResult(Binary_Img::new(r, &f).map(|s| arc(s, |s| &mut s.me))) }

    pub fn new(r: &mut Stream, f: &FileSource) -> io::Result<Self> {
        let capsDrawing = EngineX::Current().caps.contains(Caps::Drawing);
        if capsDrawing {
            let data = r.readBytes(f.fileSize as usize)?;
            let rawFormat = image::guess_format(&data).map_err(|e| Error::new(ErrorKind::InvalidData, e))?;
            let image = image::load_from_memory_with_format(&data, rawFormat).map_err(|e| Error::new(ErrorKind::InvalidData, e))?;
            let (width, height) = (image.width() as i32, image.height() as i32);
            let rawFormat = format!("{rawFormat:?}");
            let palette = image.color() == image::ColorType::L8;
            let format = match image.color() {
                image::ColorType::L16 => (rawFormat, (TextureFormat::L8, TexturePixel::Unknown)),
                image::ColorType::L8 => (rawFormat, (TextureFormat::RGB24, TexturePixel::Unknown)),
                image::ColorType::Rgb8 => (rawFormat, (TextureFormat::RGB24, TexturePixel::Unknown)),
                image::ColorType::Rgba8 => (rawFormat, (TextureFormat::ARGB32, TexturePixel::Unknown)),
                z => return Err(Error::new(ErrorKind::InvalidInput, format!("Unknown format: {z:?}"))),
            };
            // decode
            let bytes = if !palette { image.into_bytes() }
            else { image.to_rgb8().into_raw() };
            Ok(Self { capsDrawing, bytes, format, width, height, depth: 0, mipMaps: 1, texFlags: TextureFlags::empty(), me: Weak::new() })
        }
        else {
            Err(Error::new(ErrorKind::Unsupported, "Binary_Img"))
        }
    }

    // #region ITexture

    pub fn create<T>(&self, platform: &str, func: impl FnOnce(Box<dyn Any>) -> T) -> io::Result<T> { Ok(func(Box::new(TextureAsBytes { bytes: self.bytes.clone(), format: Arc::new(self.format.1) as AnyArc, spans: vec![] }))) }

    // #endregion
}

impl ITexture for Binary_Img {
    fn width(&self) -> i32 { self.width }
    fn height(&self) -> i32 { self.height }
    fn depth(&self) -> i32 { self.depth }
    fn mipMaps(&self) -> i32 { self.mipMaps }
    fn texFlags(&self) -> TextureFlags { self.texFlags }
    fn create(&self, platform: &str, func: &mut dyn FnMut(&dyn Any)) -> io::Result<()> { Self::create(self, platform, |x| func(&*x)) }
}

impl IHaveMetaInfo for Binary_Img {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Texture", file, thisTexture(&self.me)),
        MetaInfo::new(Some("Binary_Img".into()), Tag::None, vec![
            node(format!("Format: {}", self.format.0)),
            node(format!("Width: {}", self.width)),
            node(format!("Height: {}", self.height)),
        ], false),
    ] }
}

// #endregion

// #region Binary_Msg

/// The file format for MSG
pub struct Binary_Msg {
    pub message: String,
}

impl Binary_Msg {
    /// C# returns a `Func` capturing `message`; see the header.
    pub fn factory(message: String) -> FuncAsset { funcAsset(move |r, f, s| fromResult(Ok(Tag::Info(Arc::new(Binary_Msg::new(message.clone())))))) }

    pub fn new(message: String) -> Self { Self { message } }
}

impl IHaveMetaInfo for Binary_Msg {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Text", file, Tag::Str(self.message.clone())),
    ] }
}

// #endregion

// #region Binary_Pal

#[repr(C, packed)]
#[derive(Debug, Clone, Copy, Default, Pod, Zeroable)]
struct RGB { r: u8, g: u8, b: u8 }

/// The file format for Palette (computing)
/// see https://en.wikipedia.org/wiki/Palette_(computing)
pub struct Binary_Pal {
    pub bpp: u8,
    pub records: Vec<Vec<u8>>,
}

impl Binary_Pal {
    pub fn factory_3<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { fromResult(Binary_Pal::new(r, 3).map(|s| Tag::Info(Arc::new(s)))) }
    pub fn factory_4<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { fromResult(Binary_Pal::new(r, 4).map(|s| Tag::Info(Arc::new(s)))) }

    pub fn new(r: &mut Stream, bpp: u8) -> io::Result<Self> {
        let records = match bpp {
            3 => r.readPArray::<RGB>("3B", 256)?.into_iter().map(|s| vec![s.r, s.g, s.b, 255u8]).collect(),
            4 => r.readPArray::<u32>("I", 256)?.into_iter().map(|s| s.to_le_bytes().to_vec()).collect(),
            _ => return Err(Error::new(ErrorKind::InvalidInput, format!("bpp: {bpp}"))),
        };
        Ok(Self { bpp, records })
    }

    pub fn convertVgaPalette(&mut self) -> &mut Self {
        match self.bpp {
            3 => {
                for i in 0..256 {
                    let p = &mut self.records[i];
                    p[0] = (p[0] << 2) | (p[0] >> 4);
                    p[1] = (p[1] << 2) | (p[1] >> 4);
                    p[2] = (p[2] << 2) | (p[2] >> 4);
                }
            }
            _ => {}
        }
        self
    }
}

// IHaveMetaInfo
impl IHaveMetaInfo for Binary_Pal {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Text", file, Tag::Str("Pallet".into())),
        MetaInfo::new(Some("Pallet".into()), Tag::None, vec![
            node(format!("Records: {}", self.records.len())),
        ], false),
    ] }
}

// #endregion

// #region Binary_Pcx
// https://github.com/warpdesign/pcx-js/blob/master/js/pcx.js

#[repr(C, packed)]
#[derive(Debug, Clone, Copy, Pod, Zeroable)]
struct Binary_PcxX_Header {
    manufacturer: u8,       // Fixed header field valued at a hexadecimal
    version: u8,            // Version number referring to the Paintbrush software release
    encoding: u8,           // Method used for encoding the image data
    bpp: u8,                // Number of bits constituting one plane
    xMin: u16,              // Minimum x co-ordinate of the image position
    yMin: u16,              // Minimum y co-ordinate of the image position
    xMax: u16,              // Maximum x co-ordinate of the image position
    yMax: u16,              // Maximum y co-ordinate of the image position
    hDpi: u16,              // Horizontal image resolution in DPI
    vDpi: u16,              // Vertical image resolution in DPI
    palette: [u8; 48],      // EGA palette for 16-color images
    reserved1: u8,          // First reserved field
    numPlanes: u8,          // Number of color planes constituting the pixel data
    bpl: u16,               // Number of bytes of one color plane representing a single scan line
    mode: u16,              // Mode in which to construe the palette
    hRes: u16,              // horizontal resolution of the source system's screen
    vRes: u16,              // vertical resolution of the source system's screen
    reserved2: [u8; 54],    // Second reserved field, intended for future extension
}
impl IStruct for Binary_PcxX_Header { const Struct: (&'static str, usize) = ("<4B6H48s2B4H54s", 128); }

/// The file format for PiCture eXchange
/// see https://en.wikipedia.org/wiki/PCX
pub struct Binary_Pcx {
    header: Binary_PcxX_Header,
    planes: i32,
    body: Vec<u8>,
    // ITexture
    pub width: i32,
    pub height: i32,
    pub depth: i32,
    pub mipMaps: i32,
    pub texFlags: TextureFlags,
    me: Weak<Binary_Pcx>,
}

impl Binary_Pcx {
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { fromResult(Binary_Pcx::new(r).map(|s| arc(s, |s| &mut s.me))) }

    pub fn new(r: &mut Stream) -> io::Result<Self> {
        let header: Binary_PcxX_Header = r.readS()?;
        if header.manufacturer != 0x0a { return Err(Error::new(ErrorKind::InvalidData, "BAD MAGIC")); }
        else if header.encoding == 0 { return Err(Error::new(ErrorKind::InvalidData, "NO COMPRESSION")); }
        let body = r.readToEnd()?;
        let planes = header.numPlanes as i32;
        let width = header.xMax as i32 - header.xMin as i32 + 1;
        let height = header.yMax as i32 - header.yMin as i32 + 1;
        Ok(Self { header, planes, body, width, height, depth: 0, mipMaps: 1, texFlags: TextureFlags::empty(), me: Weak::new() })
    }

    // #region ITexture

    const Format: (TextureFormat, TexturePixel) = (TextureFormat::RGBA32, TexturePixel::Unknown);

    /// Gets the palette either from the header (< 8 bit) or at the bottom of the file (8bit)
    pub fn getPalette(&self) -> io::Result<&[u8]> {
        if self.header.bpp == 8 && self.body[self.body.len() - 769] == 12 { Ok(&self.body[self.body.len() - 768..]) }
        else if self.header.bpp == 1 { Ok(&self.header.palette[..48]) }
        else { Err(Error::new(ErrorKind::InvalidData, "Could not find 256 color palette.")) }
    }

    /// Set a color using palette index
    #[inline]
    fn setPixel(palette: &[u8], pixels: &mut [u8], pos: usize, index: usize) {
        let start = index * 3;
        pixels[pos + 0] = palette[start];
        pixels[pos + 1] = palette[start + 1];
        pixels[pos + 2] = palette[start + 2];
        pixels[pos + 3] = 255; // alpha channel
    }

    /// Returns true if the 2 most-significant bits are set
    fn rle(body: &[u8], offset: usize) -> bool { (body[offset] >> 6) == 3 }

    /// Returns the length of the RLE run.
    fn rleLength(body: &[u8], offset: usize) -> i32 { (body[offset] & 63) as i32 }

    pub fn create<T>(&self, platform: &str, func: impl FnOnce(Box<dyn Any>) -> T) -> io::Result<T> {
        let (width, height, planes, bpl) = (self.width as usize, self.height as usize, self.planes as usize, self.header.bpl as usize);

        // decodes 4bpp pixel data
        let decode4bpp = || -> io::Result<Vec<u8>> {
            let palette = self.getPalette()?;
            let mut temp = vec![0u8; width * height];
            let mut pixels = vec![0u8; width * height * 4];
            let (mut offset, mut pos, mut length, mut val) = (0usize, 0usize, 0i32, 0i32);

            // Simple RLE decoding: if 2 msb == 1 then we have to mask out count and repeat following byte count times
            let b = &self.body;
            for y in 0..height {
                for p in 0..planes {
                    // bpr holds the number of bytes needed to decode a row of plane: we keep on decoding until the buffer is full
                    pos = width * y;
                    for i_ in 0..bpl {
                        if length == 0 {
                            if Self::rle(b, offset) { length = Self::rleLength(b, offset); val = b[offset + 1] as i32; offset += 2; }
                            else { length = 1; val = b[offset] as i32; offset += 1; }
                        }
                        length -= 1;

                        // Since there may, or may not be blank data at the end of each scanline, we simply check we're not out of bounds
                        if (i_ * 8) < width {
                            for i in 0..8 {
                                let bit = (val >> (7 - i)) & 1;
                                temp[pos + i] |= (bit << p) as u8;
                                // we have all planes: we may set color using the palette
                                if p == planes - 1 { Self::setPixel(palette, &mut pixels, (pos + i) * 4, temp[pos + i] as usize); }
                            }
                            pos += 8;
                        }
                    }
                }
            }
            Ok(pixels)
        };

        // decodes 8bpp (depth = 8/24bit) data
        let decode8bpp = || -> io::Result<Vec<u8>> {
            let palette = if planes == 1 { Some(self.getPalette()?) } else { None };
            let mut pixels = vec![0u8; width * height * 4];
            let (mut offset, mut pos, mut length, mut val) = (0usize, 0usize, 0i32, 0i32);

            // Simple RLE decoding: if 2 msb == 1 then we have to mask out count and repeat following byte count times
            let b = &self.body;
            for y in 0..height {
                for p in 0..planes {
                    // bpr holds the number of bytes needed to decode a row of plane: we keep on decoding until the buffer is full
                    pos = 4 * width * y + p;
                    for i_ in 0..bpl {
                        if length == 0 {
                            if Self::rle(b, offset) { length = Self::rleLength(b, offset); val = b[offset + 1] as i32; offset += 2; }
                            else { length = 1; val = b[offset] as i32; offset += 1; }
                        }
                        length -= 1;

                        // Since there may, or may not be blank data at the end of each scanline, we simply check we're not out of bounds
                        if i_ < width {
                            if planes == 3 {
                                pixels[pos] = val as u8;
                                if p == planes - 1 { pixels[pos + 1] = 255; } // add alpha channel
                            }
                            else { Self::setPixel(palette.ok_or_else(|| Error::new(ErrorKind::InvalidData, "palette is null"))?, &mut pixels, pos, val as usize); }
                            pos += 4;
                        }
                    }
                }
            }
            Ok(pixels)
        };

        let bytes = match self.header.bpp {
            8 => decode8bpp()?,
            1 => decode4bpp()?,
            z => return Err(Error::new(ErrorKind::InvalidData, format!("Unknown bpp: {z}"))),
        };
        Ok(func(Box::new(TextureAsBytes { bytes, format: Arc::new(Self::Format) as AnyArc, spans: vec![] })))
    }

    // #endregion
}

impl ITexture for Binary_Pcx {
    fn width(&self) -> i32 { self.width }
    fn height(&self) -> i32 { self.height }
    fn depth(&self) -> i32 { self.depth }
    fn mipMaps(&self) -> i32 { self.mipMaps }
    fn texFlags(&self) -> TextureFlags { self.texFlags }
    fn create(&self, platform: &str, func: &mut dyn FnMut(&dyn Any)) -> io::Result<()> { Self::create(self, platform, |x| func(&*x)) }
}

impl IHaveMetaInfo for Binary_Pcx {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Texture", file, thisTexture(&self.me)),
        MetaInfo::new(Some("Binary_Pcx".into()), Tag::None, vec![
            node(format!("Width: {}", self.width)),
            node(format!("Height: {}", self.height)),
        ], false),
    ] }
}

// #endregion

// #region Binary_Plist

/// The file format for XXX
pub struct Binary_Plist;

impl Binary_Plist {
    /// C# `ArcBinary<Binary_Plist>.Current`.
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_Plist));
        CURRENT.clone()
    }
}

impl ArcBinary for Binary_Plist {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, tag: &'a Tag) -> Task<'a, ()> {
        let _ = tag;
        Box::pin(async move {
            let r = r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "NullReferenceException: r"))?;
            let dict = PlistReader::new().readObject(r)?;
            let mut files = Vec::new();
            for (key, value) in dict {
                let path = match key { PlistObject::Str(s) => s, _ => return Err(Error::new(ErrorKind::InvalidData, "InvalidCastException: Key")) };
                let bytes = match value { PlistObject::Data(b) => b, _ => return Err(Error::new(ErrorKind::InvalidData, "InvalidCastException: Value")) };
                files.push(Arc::new(FileSource { path, fileSize: bytes.len() as i64, tag: Tag::Bytes(bytes), ..Default::default() }));
            }
            source.setFiles(Some(files));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, option: &'a Tag) -> Task<'a, Option<Stream>> {
        let _ = (source, r, option);
        Box::pin(async move {
            let bytes = match &file.tag { Tag::Bytes(b) => b.clone(), _ => return Err(Error::new(ErrorKind::InvalidData, "InvalidCastException: Tag")) };
            Ok(Some(Box::new(Cursor::new(bytes)) as Stream))
        })
    }
}

// #endregion

// #region Binary_Raw

/// C# `Binary_Raw.Tag`.
#[derive(Debug, Clone, Default)]
pub struct Binary_RawTag {
    pub palette: Option<String>,
    pub width: i32,
    pub height: i32,
}

/// The file format for XXX
pub struct Binary_Raw {
    pub body: Option<Vec<u8>>,
    paletteData: Vec<Vec<u8>>,
    pub palette: Option<String>,
    // ITexture
    pub width: i32,
    pub height: i32,
    pub depth: i32,
    pub mipMaps: i32,
    pub texFlags: TextureFlags,
    me: Weak<Binary_Raw>,
}

impl Binary_Raw {
    /// C# returns a `Func` capturing `action` and `palleteFunc`; see the header.
    pub fn factoryMethod(
        action: impl Fn(&mut Binary_Raw, &mut Stream, &FileSource) -> io::Result<()> + Send + Sync + 'static,
        palleteFunc: impl Fn(&str, Option<&str>) -> Binary_Pal + Send + Sync + 'static) -> FuncAsset {
        funcAsset(move |r, f, s| fromResult(Binary_Raw::new(r, &s.base().game, &f, &action, &palleteFunc).map(|s| arc(s, |s| &mut s.me))))
    }

    pub fn new(r: &mut Stream, game: &FamilyGame, source: &FileSource, action: &dyn Fn(&mut Binary_Raw, &mut Stream, &FileSource) -> io::Result<()>, palleteFunc: &dyn Fn(&str, Option<&str>) -> Binary_Pal) -> io::Result<Self> {
        let mut s = Self { body: None, paletteData: vec![], palette: None, width: 0, height: 0, depth: 0, mipMaps: 1, texFlags: TextureFlags::empty(), me: Weak::new() };
        action(&mut s, r, source)?;
        if s.body.is_none() { s.body = Some(r.readToEnd()?); }
        if let Some(c) = source.tag.asAny().and_then(|z| z.downcast_ref::<Binary_RawTag>()) {
            s.palette = c.palette.clone();
            s.width = c.width;
            s.height = c.height;
        }
        s.paletteData = palleteFunc(&game.id, s.palette.as_deref()).records;
        Ok(s)
    }

    // #region ITexure

    const Format: (TextureFormat, TexturePixel) = (TextureFormat::RGBA32, TexturePixel::Unknown);

    pub fn create<T>(&self, platform: &str, func: impl FnOnce(Box<dyn Any>) -> T) -> io::Result<T> {
        Ok(func(Box::new(TextureAsBytes { bytes: self.body.iter().flatten().flat_map(|&s| self.paletteData[s as usize].iter().copied()).collect(), format: Arc::new(Self::Format) as AnyArc, spans: vec![] })))
    }

    // #endregion
}

impl ITexture for Binary_Raw {
    fn width(&self) -> i32 { self.width }
    fn height(&self) -> i32 { self.height }
    fn depth(&self) -> i32 { self.depth }
    fn mipMaps(&self) -> i32 { self.mipMaps }
    fn texFlags(&self) -> TextureFlags { self.texFlags }
    fn create(&self, platform: &str, func: &mut dyn FnMut(&dyn Any)) -> io::Result<()> { Self::create(self, platform, |x| func(&*x)) }
}

impl IHaveMetaInfo for Binary_Raw {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Texture", file, thisTexture(&self.me)),
        MetaInfo::new(Some("Binary_Raw".into()), Tag::None, vec![
            node(format!("Palette: {}", self.palette.as_deref().unwrap_or(""))),
            node(format!("Width: {}", self.width)),
            node(format!("Height: {}", self.height)),
        ], false),
    ] }
}

// #endregion

// #region Binary_Snd

#[repr(C, packed)]
#[derive(Debug, Clone, Copy, Default, Pod, Zeroable)]
pub struct WavHeader {
    pub chunkId: u32,               // 'RIFF'
    pub chunkSize: i32,             // Size of the overall file - 8 bytes, in bytes (32-bit integer)
    pub format: u32,                // 'WAVE'
}
impl WavHeader {
    pub const RIFF: i32 = 0x46464952;
    pub const WAVE: i32 = 0x45564157;
}
impl IStruct for WavHeader { const Struct: (&'static str, usize) = ("<3I", std::mem::size_of::<WavHeader>()); }

#[repr(C, packed)]
#[derive(Debug, Clone, Copy, Default, Pod, Zeroable)]
pub struct WavFmt {
    pub chunkId: u32,               // 'fmt '
    pub chunkSize: i32,             // Length of format data (16)
    pub audioFormat: u16,           // Type of format (1 is PCM)
    pub numChannels: u16,           // Number of Channels
    pub sampleRate: u32,            // Sample Rate
    pub byteRate: u32,              // (Sample Rate * BitsPerSample * Channels) / 8
    pub blockAlign: u16,            // (BitsPerSample * Channels) / 8.1 - 8 bit mono2 - 8 bit stereo/16 bit mono4 - 16 bit stereo
    pub bitsPerSample: u16,         // Bits per sample
}
impl WavFmt {
    pub const FMT_: i32 = 0x20746d66;
}
impl IStruct for WavFmt { const Struct: (&'static str, usize) = ("<2I2H2I2H", std::mem::size_of::<WavFmt>()); }

#[repr(C, packed)]
#[derive(Debug, Clone, Copy, Default, Pod, Zeroable)]
pub struct WavData {
    pub chunkId: u32,               // 'data'
    pub chunkSize: i32,             // Size of the data section
}
impl WavData {
    pub const DATA: i32 = 0x61746164;
}
impl IStruct for WavData { const Struct: (&'static str, usize) = ("<3I2H6I", std::mem::size_of::<WavData>()); }

/// The file format for XXX
pub struct Binary_Snd {
    pub data: Vec<u8>,
    pub tag: Tag,
}

impl Binary_Snd {
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { fromResult(Binary_Snd::new(r, f.fileSize as i32, Tag::None).map(|s| Tag::Info(Arc::new(s)))) }
    pub fn factory_Wav<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { fromResult(Binary_Snd::new(r, f.fileSize as i32, Tag::Str(".wav".into())).map(|s| Tag::Info(Arc::new(s)))) }

    pub fn new(r: &mut Stream, fileSize: i32, tag: Tag) -> io::Result<Self> { Ok(Self { data: r.readBytes(fileSize as usize)?, tag }) }
}

impl IHaveMetaInfo for Binary_Snd {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "AudioPlayer".into(), name: name(file), value: Tag::stream(Box::new(Cursor::new(self.data.clone()))),
            tag: if self.tag.isNone() { Tag::Str(file.map_or(String::new(), |s| extension(&s.path))) } else { self.tag.clone() },
            ..Default::default() })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Tga
// https://github.com/cadenji/tgafunc/blob/main/tgafunc.c
// https://www.dca.fee.unicamp.br/~martino/disciplinas/ea978/tgaffs.pdf
// https://www.conholdate.app/viewer/view/rVqTeZPLAL/tga-file-format-specifications.pdf?default=view&preview=

// Image pixel format
// The pixel data are all in little-endian. E.g. a PIXEL_ARGB32 format image, a single pixel is stored in the memory in the order of BBBBBBBB GGGGGGGG RRRRRRRR AAAAAAAA.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum PIXEL {
    BW8,    // Single channel format represents grayscale, 8-bit integer.
    BW16,   // Single channel format represents grayscale, 16-bit integer.
    RGB555, // A 16-bit pixel format. The topmost bit is assumed to an attribute bit, usually ignored. Because of little-endian, this format pixel is stored in the memory in the order of GGGBBBBB ARRRRRGG.
    RGB24,  // RGB color format, 8-bit per channel.
    ARGB32, // RGB color with alpha format, 8-bit per channel.
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum TYPE {
    NO_DATA = 0,
    COLOR_MAPPED = 1,
    TRUE_COLOR = 2,
    GRAYSCALE = 3,
    RLE_COLOR_MAPPED = 9,
    RLE_TRUE_COLOR = 10,
    RLE_GRAYSCALE = 11,
}

// Gets the bytes per pixel by pixel format
#[inline]
fn pixelFormatToPixelSize(format: PIXEL) -> i32 {
    match format {
        PIXEL::BW8 => 1,
        PIXEL::BW16 => 2,
        PIXEL::RGB555 => 2,
        PIXEL::RGB24 => 3,
        PIXEL::ARGB32 => 4,
    }
}

// Convert bits to integer bytes. E.g. 8 bits to 1 byte, 9 bits to 2 bytes.
#[inline] fn bitsToBytes(bits: u8) -> u8 { ((bits as i32 - 1) / 8 + 1) as u8 }

#[derive(Default)]
struct ColorMap {
    firstIndex: u16,
    entryCount: u16,
    bytesPerEntry: u8,
    pixels: Option<Vec<u8>>,
}

#[repr(C, packed)]
#[derive(Debug, Clone, Copy, Default, Pod, Zeroable)]
struct Binary_TgaX_Header {
    idLength: u8,
    mapType: u8,
    imageType: u8, // TYPE
    // color map specification
    mapFirstEntry: u16,
    mapLength: u16,
    mapEntrySize: u8,
    // image specification
    imageXOrigin: u16,
    imageYOrigin: u16,
    imageWidth: u16,
    imageHeight: u16,
    pixelDepth: u8,
    imageDescriptor: u8,
}
impl IStruct for Binary_TgaX_Header { const Struct: (&'static str, usize) = ("<3b2Hb4H2b", std::mem::size_of::<Binary_TgaX_Header>()); }

impl Binary_TgaX_Header {
    fn IS_SUPPORTED_IMAGE_TYPE(&self) -> bool {
        self.imageType == TYPE::COLOR_MAPPED as u8 ||
        self.imageType == TYPE::TRUE_COLOR as u8 ||
        self.imageType == TYPE::GRAYSCALE as u8 ||
        self.imageType == TYPE::RLE_COLOR_MAPPED as u8 ||
        self.imageType == TYPE::RLE_TRUE_COLOR as u8 ||
        self.imageType == TYPE::RLE_GRAYSCALE as u8
    }
    fn IS_COLOR_MAPPED(&self) -> bool {
        self.imageType == TYPE::COLOR_MAPPED as u8 ||
        self.imageType == TYPE::RLE_COLOR_MAPPED as u8
    }
    fn IS_TRUE_COLOR(&self) -> bool {
        self.imageType == TYPE::TRUE_COLOR as u8 ||
        self.imageType == TYPE::RLE_TRUE_COLOR as u8
    }
    fn IS_GRAYSCALE(&self) -> bool {
        self.imageType == TYPE::GRAYSCALE as u8 ||
        self.imageType == TYPE::RLE_GRAYSCALE as u8
    }
    fn IS_RLE(&self) -> bool {
        self.imageType == TYPE::RLE_COLOR_MAPPED as u8 ||
        self.imageType == TYPE::RLE_TRUE_COLOR as u8 ||
        self.imageType == TYPE::RLE_GRAYSCALE as u8
    }

    fn check(&self) -> io::Result<()> {
        const MAX_IMAGE_DIMENSIONS: i32 = 65535;
        let (imageWidth, imageHeight) = (self.imageWidth as i32, self.imageHeight as i32);
        if self.mapType > 1 { Err(Error::new(ErrorKind::InvalidData, "UNSUPPORTED_COLOR_MAP_TYPE")) }
        else if self.imageType == TYPE::NO_DATA as u8 { Err(Error::new(ErrorKind::InvalidData, "NO_DATA")) }
        else if !self.IS_SUPPORTED_IMAGE_TYPE() { Err(Error::new(ErrorKind::InvalidData, "UNSUPPORTED_IMAGE_TYPE")) }
        else if imageWidth <= 0 || imageWidth > MAX_IMAGE_DIMENSIONS || imageHeight <= 0 || imageHeight > MAX_IMAGE_DIMENSIONS { Err(Error::new(ErrorKind::InvalidData, "INVALID_IMAGE_DIMENSIONS")) }
        else { Ok(()) }
    }

    fn getColorMap(&self, r: &mut Stream) -> io::Result<ColorMap> {
        let mapSize = self.mapLength as i32 * bitsToBytes(self.mapEntrySize) as i32;
        let mut s = ColorMap::default();
        if self.IS_COLOR_MAPPED() {
            s.firstIndex = self.mapFirstEntry;
            s.entryCount = self.mapLength;
            s.bytesPerEntry = bitsToBytes(self.mapEntrySize);
            s.pixels = Some(r.readBytes(mapSize as usize)?);
        }
        else if self.mapType == 1 { r.skip(mapSize as i64)?; } // The image is not color mapped at this time, but contains a color map. So skips the color map data block directly.
        Ok(s)
    }

    fn getPixelFormat(&self) -> io::Result<PIXEL> {
        if self.IS_COLOR_MAPPED() {
            if self.pixelDepth == 8 {
                match self.mapEntrySize {
                    15 | 16 => return Ok(PIXEL::RGB555),
                    24 => return Ok(PIXEL::RGB24),
                    32 => return Ok(PIXEL::ARGB32),
                    _ => {}
                }
            }
        }
        else if self.IS_TRUE_COLOR() {
            match self.pixelDepth {
                16 => return Ok(PIXEL::RGB555),
                24 => return Ok(PIXEL::RGB24),
                32 => return Ok(PIXEL::ARGB32),
                _ => {}
            }
        }
        else if self.IS_GRAYSCALE() {
            match self.pixelDepth {
                8 => return Ok(PIXEL::BW8),
                16 => return Ok(PIXEL::BW16),
                _ => {}
            }
        }
        Err(Error::new(ErrorKind::InvalidData, "UNSUPPORTED_PIXEL_FORMAT"))
    }
}

/// C# `MemoryStream.ReadByte()`: -1 at the end.
fn memReadByte(s: &mut Cursor<Vec<u8>>) -> io::Result<i32> { let mut b = [0u8; 1]; Ok(if s.read(&mut b)? == 0 { -1 } else { b[0] as i32 }) }

/// The file format for XXX
/// see https://en.wikipedia.org/wiki/Truevision_TGA
pub struct Binary_Tga {
    header: Binary_TgaX_Header,
    map: Mutex<ColorMap>,
    pixelFormat: PIXEL,
    pixelSize: i32,
    body: Mutex<Cursor<Vec<u8>>>,
    // ITexture
    format: (TextureFormat, TexturePixel),
    pub width: i32,
    pub height: i32,
    pub depth: i32,
    pub mipMaps: i32,
    pub texFlags: TextureFlags,
    me: Weak<Binary_Tga>,
}

impl Binary_Tga {
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { fromResult(Binary_Tga::new(r, &f).map(|s| arc(s, |s| &mut s.me))) }

    pub fn new(r: &mut Stream, f: &FileSource) -> io::Result<Self> {
        let header: Binary_TgaX_Header = r.readS()?;
        header.check()?;
        r.skip(header.idLength as i64)?;
        let map = header.getColorMap(r)?;
        let width = header.imageWidth as i32;
        let height = header.imageHeight as i32;
        let body = Cursor::new(r.readToEnd()?);
        let pixelFormat = header.getPixelFormat()?;
        let pixelSize = pixelFormatToPixelSize(pixelFormat);
        let format = match pixelFormat {
            PIXEL::BW8 => return Err(Error::new(ErrorKind::Unsupported, "NotSupported")),
            PIXEL::BW16 => return Err(Error::new(ErrorKind::Unsupported, "NotSupported")),
            PIXEL::RGB555 => (TextureFormat::RGB565, TexturePixel::Unknown),
            PIXEL::RGB24 => (TextureFormat::RGB24, TexturePixel::Unknown),
            PIXEL::ARGB32 => (TextureFormat::RGBA32, TexturePixel::Unknown),
        };
        Ok(Self { header, map: Mutex::new(map), pixelFormat, pixelSize, body: Mutex::new(body), format, width, height, depth: 0, mipMaps: 1, texFlags: TextureFlags::empty(), me: Weak::new() })
    }

    // #region ITexture

    #[inline]
    fn pixelToMapIndex(pixelPtr: &[u8], offset: usize) -> u16 { pixelPtr[offset] as u16 }

    #[inline]
    fn getColorFromMap(dest: &mut [u8], offset: usize, index: u16, map: &ColorMap) -> io::Result<()> {
        let index = index.wrapping_sub(map.firstIndex);
        if (index as i32) < 0 && index >= map.entryCount { return Err(Error::new(ErrorKind::InvalidData, "COLOR_MAP_INDEX_FAILED")); }
        let pixels = map.pixels.as_ref().ok_or_else(|| Error::new(ErrorKind::InvalidData, "Map.Pixels is null"))?;
        let n = map.bytesPerEntry as usize;
        dest[offset..offset + n].copy_from_slice(&pixels[n * index as usize..n * index as usize + n]);
        Ok(())
    }

    pub fn create<T>(&self, platform: &str, func: impl FnOnce(Box<dyn Any>) -> T) -> io::Result<T> {
        let mut s = self.body.lock().unwrap();
        let mut map = self.map.lock().unwrap();

        // decodeRle
        let decodeRle = |data: &mut [u8], s: &mut Cursor<Vec<u8>>, map: &ColorMap| -> io::Result<()> {
            let isColorMapped = self.header.IS_COLOR_MAPPED();
            let pixelSize = self.pixelSize as usize;
            let mut o = 0usize;
            let mut pixelCount = self.width * self.height;

            let mut isRunLengthPacket = false;
            let mut packetCount = 0;
            let mut pixelBuffer = vec![0u8; if isColorMapped { map.bytesPerEntry as usize } else { pixelSize }];

            while pixelCount > 0 {
                if packetCount == 0 {
                    let repetitionCountField = memReadByte(s)?;
                    isRunLengthPacket = (repetitionCountField & 0x80) != 0;
                    packetCount = (repetitionCountField & 0x7F) + 1;
                    if isRunLengthPacket {
                        s.read(&mut pixelBuffer[0..pixelSize])?;
                        // in color mapped image, the pixel as the index value of the color map. The actual pixel value is found from the color map.
                        if isColorMapped { let index = Self::pixelToMapIndex(&pixelBuffer, o); Self::getColorFromMap(&mut pixelBuffer, 0, index, map)?; }
                    }
                }

                if isRunLengthPacket { data[o..o + pixelSize].copy_from_slice(&pixelBuffer[0..pixelSize]); }
                else {
                    s.read(&mut data[o..o + pixelSize])?;
                    // In color mapped image, the pixel as the index value of the color map. The actual pixel value is found from the color map.
                    if isColorMapped { let index = Self::pixelToMapIndex(data, o); Self::getColorFromMap(data, o, index, map)?; }
                }

                packetCount -= 1;
                o += pixelSize;
                pixelCount -= 1;
            }
            Ok(())
        };

        // decode
        let decode = |data: &mut [u8], s: &mut Cursor<Vec<u8>>, map: &ColorMap| -> io::Result<()> {
            let isColorMapped = self.header.IS_COLOR_MAPPED();
            let pixelSize = self.pixelSize as usize;
            let mut o = 0usize;
            let mut pixelCount = (self.width * self.height) as usize;

            // in color mapped image, the pixel as the index value of the color map. The actual pixel value is found from the color map.
            if isColorMapped {
                while pixelCount > 0 {
                    s.read(&mut data[o..o + pixelSize])?;
                    let index = Self::pixelToMapIndex(data, o); Self::getColorFromMap(data, o, index, map)?;
                    o += map.bytesPerEntry as usize;
                    pixelCount -= 1;
                }
            }
            else { s.read(&mut data[0..pixelCount * pixelSize])?; }
            Ok(())
        };

        let mut bytes = vec![0u8; (self.width * self.height * self.pixelSize) as usize];
        if self.header.IS_RLE() { decodeRle(&mut bytes, &mut s, &map)?; }
        else { decode(&mut bytes, &mut s, &map)?; }
        map.pixels = None;

        // flip the image if necessary, to keep the origin in upper left corner.
        let flipH = (self.header.imageDescriptor & 0x10) != 0;
        let flipV = (self.header.imageDescriptor & 0x20) == 0;
        if flipH { self.flipH(&mut bytes); }
        if flipV { self.flipV(&mut bytes); }

        Ok(func(Box::new(TextureAsBytes { bytes, format: Arc::new(self.format) as AnyArc, spans: vec![] })))
    }

    // Returns the pixel at coordinates (x,y) for reading or writing.
    // If the pixel coordinates are out of bounds (larger than width/height or small than 0), they will be clamped.
    #[inline]
    fn getPixel(&self, mut x: i32, mut y: i32) -> usize {
        if x < 0 { x = 0; }
        else if x >= self.width { x = self.width - 1; }
        if y < 0 { y = 0; }
        else if y >= self.height { y = self.height - 1; }
        ((y * self.width + x) * self.pixelSize) as usize
    }

    fn flipH(&self, data: &mut [u8]) {
        let pixelSize = self.pixelSize as usize;
        let mut temp = vec![0u8; pixelSize];
        let flipNum = self.width / 2;
        for i in 0..flipNum {
            for j in 0..self.height {
                let p1 = self.getPixel(i, j);
                let p2 = self.getPixel(self.width - 1 - i, j);
                // swap two pixels
                temp.copy_from_slice(&data[p1..p1 + pixelSize]);
                data.copy_within(p2..p2 + pixelSize, p1);
                data[p2..p2 + pixelSize].copy_from_slice(&temp);
            }
        }
    }

    fn flipV(&self, data: &mut [u8]) {
        let pixelSize = self.pixelSize as usize;
        let mut temp = vec![0u8; pixelSize];
        let flipNum = self.height / 2;
        for i in 0..flipNum {
            for j in 0..self.width {
                let p1 = self.getPixel(j, i);
                let p2 = self.getPixel(j, self.height - 1 - i);
                // swap two pixels
                temp.copy_from_slice(&data[p1..p1 + pixelSize]);
                data.copy_within(p2..p2 + pixelSize, p1);
                data[p2..p2 + pixelSize].copy_from_slice(&temp);
            }
        }
    }

    // #endregion
}

impl ITexture for Binary_Tga {
    fn width(&self) -> i32 { self.width }
    fn height(&self) -> i32 { self.height }
    fn depth(&self) -> i32 { self.depth }
    fn mipMaps(&self) -> i32 { self.mipMaps }
    fn texFlags(&self) -> TextureFlags { self.texFlags }
    fn create(&self, platform: &str, func: &mut dyn FnMut(&dyn Any)) -> io::Result<()> { Self::create(self, platform, |x| func(&*x)) }
}

impl IHaveMetaInfo for Binary_Tga {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Texture", file, thisTexture(&self.me)),
        MetaInfo::new(Some("Binary_Tga".into()), Tag::None, vec![
            node(format!("Format: {:?}", self.pixelFormat)),
            node(format!("Width: {}", self.width)),
            node(format!("Height: {}", self.height)),
        ], false),
    ] }
}

// #endregion

// #region Binary_Txt

/// The file format for XXX
pub struct Binary_Txt {
    pub data: Option<String>,
}

impl Binary_Txt {
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { fromResult(Binary_Txt::new(r, f.fileSize as i32).map(|s| Tag::Info(Arc::new(s)))) }

    pub fn new(r: &mut Stream, fileSize: i32) -> io::Result<Self> { Ok(Self { data: r.readFUString(fileSize as usize)? }) }
}

impl IHaveMetaInfo for Binary_Txt {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Text", file, self.data.clone().map_or(Tag::None, Tag::Str)),
    ] }
}

// #endregion

// #region Binary_Xga

pub struct Binary_Xga {
    type_: i32,
    body: Vec<u8>,
    // ITexture
    pub width: i32,
    pub height: i32,
    pub depth: i32,
    pub mipMaps: i32,
    pub texFlags: TextureFlags,
    me: Weak<Binary_Xga>,
}

impl Binary_Xga {
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> {
        let tag = s.base().tag.lock().unwrap_or_else(|p| p.into_inner()).clone();
        fromResult(Binary_Xga::new(r, &tag).map(|s| arc(s, |s| &mut s.me)))
    }

    pub fn new(r: &mut Stream, tag: &Tag) -> io::Result<Self> {
        Ok(Self { type_: 1, body: r.readToEnd()?, width: 64, height: 64, depth: 0, mipMaps: 1, texFlags: TextureFlags::empty(), me: Weak::new() })
    }

    // #region ITexture

    const Format: (TextureFormat, TexturePixel) = (TextureFormat::RGBA32, TexturePixel::Unknown);

    pub fn create<T>(&self, platform: &str, func: impl FnOnce(Box<dyn Any>) -> T) -> io::Result<T> {
        fn decode1() -> Vec<u8> { Vec::new() }
        Ok(func(Box::new(TextureAsBytes { bytes: match self.type_ {
            1 => decode1(),
            z => return Err(Error::new(ErrorKind::InvalidData, format!("Unsupported type: {z}"))),
        }, format: Arc::new(Self::Format) as AnyArc, spans: vec![] })))
    }

    // #endregion
}

impl ITexture for Binary_Xga {
    fn width(&self) -> i32 { self.width }
    fn height(&self) -> i32 { self.height }
    fn depth(&self) -> i32 { self.depth }
    fn mipMaps(&self) -> i32 { self.mipMaps }
    fn texFlags(&self) -> TextureFlags { self.texFlags }
    fn create(&self, platform: &str, func: &mut dyn FnMut(&dyn Any)) -> io::Result<()> { Self::create(self, platform, |x| func(&*x)) }
}

impl IHaveMetaInfo for Binary_Xga {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Texture", file, thisTexture(&self.me)),
        MetaInfo::new(Some("Binary_Xga".into()), Tag::None, vec![
            node(format!("Type: {}", self.type_)),
            node(format!("Width: {}", self.width)),
            node(format!("Height: {}", self.height)),
        ], false),
    ] }
}

// #endregion

// #region Binary_Zip

/// The archive `Read` keeps on `source.Tag` so `ReadData` can open entries after the reader is gone.
struct ZipHeld {
    arc: Mutex<ZipArchive<Cursor<Vec<u8>>>>,
    password: Option<String>,
    /// SharpZipLib's private `Key` was set. The `zip` crate has no equivalent.
    rawKey: bool,
}

fn zipError(err: zip::result::ZipError) -> Error { Error::new(ErrorKind::InvalidData, err.to_string()) }

/// C# `!ZipArchiveEntry.FullName.EndsWith('/')`.
fn systemFile(name: &str) -> bool { !name.ends_with('/') }

/// C# `ZipEntry.IsFile`. The name test is `/` or `\`. DOS directory (0x10) and volume (0x08)
/// attributes are not on the `zip` crate's `ZipFile`, so those entries stay.
fn sharpFile(name: &str) -> bool { !name.ends_with('/') && !name.ends_with('\\') }

/// Binary_Zip
pub struct Binary_Zip {
    key: Tag,
}

impl Binary_Zip {
    /// C# `Instance`.
    pub fn instance() -> Arc<dyn ArcBinary> {
        static INSTANCE: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_Zip { key: Tag::None }));
        INSTANCE.clone()
    }

    /// C# `GetArcBinary`. A null key shares `Instance`. `byte[]` keys compare by value; C# `ConcurrentDictionary<object>` uses reference equality for arrays.
    pub fn getArcBinary(game: &FamilyGame) -> Arc<dyn ArcBinary> {
        if game.key.isNone() { return Self::instance(); }
        static ARCBINARYS: LazyLock<Mutex<Vec<(Tag, Arc<dyn ArcBinary>)>>> = LazyLock::new(|| Mutex::new(Vec::new()));
        let mut arcs = ARCBINARYS.lock().unwrap_or_else(|p| p.into_inner());
        if let Some((_, found)) = arcs.iter().find(|(k, _)| k == &game.key) { return found.clone(); }
        let arc: Arc<dyn ArcBinary> = Arc::new(Binary_Zip { key: game.key.clone() });
        arcs.push((game.key.clone(), arc.clone()));
        arc
    }

    fn owned(r: &mut dyn VfsRead) -> io::Result<Vec<u8>> {
        r.seek(SeekFrom::Start(0))?;
        let mut bytes = Vec::new();
        r.read_to_end(&mut bytes)?;
        Ok(bytes)
    }
}

impl ArcBinary for Binary_Zip {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, tag: &'a Tag) -> Task<'a, ()> {
        let _ = tag;
        Box::pin(async move {
            source.useReader.store(false, Ordering::Relaxed);
            let r = r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "NullReferenceException: r"))?;
            let bytes = Self::owned(r)?;
            let mut arc = ZipArchive::new(Cursor::new(bytes)).map_err(zipError)?;
            let useSystem = self.key.isNone();
            // C# `if (Key == null) { }` sits in the SharpZipLib branch and never runs: `UseSystem` already returned.
            let (password, rawKey) = if useSystem { (None, false) }
                else { match &self.key {
                    Tag::Str(s) => (Some(s.clone()), false),
                    Tag::Bytes(_) => (None, true),
                    _ => (None, false),
                } };
            let mut files = Vec::new();
            for index in 0..arc.len() {
                // `by_index` refuses an encrypted entry until a password is supplied. C# lists it from the central directory.
                let entry = arc.by_index_raw(index).map_err(zipError)?;
                let raw = entry.name();
                let keep = if useSystem { systemFile(raw) } else { sharpFile(raw) };
                if !keep { continue; }
                let flags = if useSystem { 0 } else if entry.encrypted() { 1 } else { 0 };
                let path = raw.replace('\\', "/");
                let packedSize = entry.compressed_size() as i64;
                let fileSize = entry.size() as i64;
                // C# `Tag` is the `ZipArchiveEntry` or `ZipEntry`. The index is what `by_index` opens.
                files.push(Arc::new(FileSource { path, packedSize, fileSize, flags, tag: Tag::Int(index as i64), ..Default::default() }));
            }
            *source.base.tag.lock().unwrap_or_else(|p| p.into_inner()) = Tag::Any(Arc::new(ZipHeld { arc: Mutex::new(arc), password, rawKey }));
            source.setFiles(Some(files));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, option: &'a Tag) -> Task<'a, Option<Stream>> {
        let _ = (r, option);
        Box::pin(async move {
            let held = {
                let tag = source.base.tag.lock().unwrap_or_else(|p| p.into_inner());
                match &*tag {
                    Tag::Any(z) => z.clone().downcast::<ZipHeld>().map_err(|_| Error::new(ErrorKind::InvalidData, "InvalidCastException: Tag"))?,
                    _ => return Err(Error::new(ErrorKind::InvalidData, "InvalidCastException: Tag")),
                }
            };
            let index = match file.tag.asInt() { Some(i) if i >= 0 => i as usize, _ => return Err(Error::new(ErrorKind::InvalidData, "InvalidCastException: Tag")) };
            // SharpZipLib decrypts with the reflected `Key`. A password is not that key.
            if held.rawKey && file.flags != 0 { return Err(Error::new(ErrorKind::Unsupported, "NotSupportedException: ZipFile.Key")); }
            let mut arc = held.arc.lock().unwrap_or_else(|p| p.into_inner());
            let mut input = match &held.password {
                Some(password) if file.flags != 0 => arc.by_index_decrypt(index, password.as_bytes()).map_err(zipError)?,
                _ => arc.by_index(index).map_err(zipError)?,
            };
            // C# `if (!input.CanRead)` logs and returns `Stream.Null`. A `zip` reader implements `Read`, so that branch is not reachable.
            // The `catch` around this copy is commented out in C#, so a failure here propagates.
            let mut body = Vec::new();
            input.read_to_end(&mut body)?;
            Ok(Some(Box::new(Cursor::new(body)) as Stream))
        })
    }
}

// #endregion

// #region Binary_TestTri

#[derive(Default)]
pub struct Binary_TestTri {
    me: Weak<Binary_TestTri>,
}

impl Binary_TestTri {
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { fromResult(Ok(arc(Binary_TestTri::default(), |s| &mut s.me))) }
}

// IHaveMetaInfo
impl IHaveMetaInfo for Binary_TestTri {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("TestTri", file, this(&self.me)),
    ] }
}

// #endregion

// #region Binary_Engine

/// C# `Encoding.ASCII.GetString`: a byte above 127 is `?`.
fn ascii(bytes: &[u8]) -> String { bytes.iter().map(|&b| if b < 0x80 { b as char } else { '?' }).collect() }

fn field<'a>(values: &'a [String], index: usize) -> io::Result<&'a str> {
    values.get(index).map(String::as_str).ok_or_else(|| Error::new(ErrorKind::InvalidData, "IndexOutOfRangeException"))
}

fn parseBool(s: &str) -> io::Result<bool> {
    if s.eq_ignore_ascii_case("true") { Ok(true) }
    else if s.eq_ignore_ascii_case("false") { Ok(false) }
    else { Err(Error::new(ErrorKind::InvalidData, format!("FormatException: {s}"))) }
}

fn parseInt(s: &str) -> io::Result<i32> {
    s.trim().parse().map_err(|_| Error::new(ErrorKind::InvalidData, format!("FormatException: {s}")))
}

fn parseFloat(s: &str) -> io::Result<f32> {
    s.trim().parse().map_err(|_| Error::new(ErrorKind::InvalidData, format!("FormatException: {s}")))
}

fn ints(s: &str) -> io::Result<Vec<i32>> { s.split(',').map(parseInt).collect() }
fn floats(s: &str) -> io::Result<Vec<f32>> { s.split(',').map(parseFloat).collect() }

fn at<T: Copy>(values: &[T], index: usize) -> io::Result<T> {
    values.get(index).copied().ok_or_else(|| Error::new(ErrorKind::InvalidData, "IndexOutOfRangeException"))
}

/// C# `(BinaryArchive)archive`. A null archive stays null. Anything else that is not a `BinaryArchive` throws.
fn castArchive(archive: Option<Arc<dyn Archive>>) -> io::Result<Option<Arc<BinaryArchive>>> {
    match archive {
        None => Ok(None),
        Some(archive) => archive.downcastBinaryArchive().map(Some).ok_or_else(|| Error::new(ErrorKind::InvalidData, "InvalidCastException: BinaryArchive")),
    }
}

/// C# `Vector3.ToString` with the invariant group separator.
fn vec3String(v: Vec3) -> String { format!("<{}, {}, {}>", v.x, v.y, v.z) }

/// The file format for Engine
pub struct Binary_Engine {
    me: Weak<Binary_Engine>,
    pub archive: Option<Arc<BinaryArchive>>,
    pub queryArchive: Option<Arc<BinaryArchive>>,
    pub query: SharedQuery,
    pub cellId: Int3,
    pub cellInterior: bool,
    pub playerPosition: Vec3,
    pub playerRotation: Quat,
}

impl Binary_Engine {
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Binary_Engine::new(r, s).map(|engine| arc(engine, |s| &mut s.me)))
    }

    pub fn new(r: &mut Stream, s: &dyn Archive) -> io::Result<Self> {
        let values: Vec<String> = ascii(&r.readToEnd()?).split(':').map(str::to_string).collect();
        let ar = field(&values, 0)?.to_string();
        let qa = field(&values, 1)?.to_string();
        let ci = ints(field(&values, 2)?)?;
        let cb = parseBool(field(&values, 3)?)?;
        let pp = floats(field(&values, 4)?)?;
        let pr = floats(field(&values, 5)?)?;
        let archive = castArchive(ArchiveExt::getArchive(s, &Tag::Str(ar), true)?)?;
        let queryArchive = castArchive(ArchiveExt::getArchive(s, &Tag::Str(qa), true)?)?;
        let queryBinary = queryArchive.as_ref().ok_or_else(|| Error::new(ErrorKind::InvalidData, "NullReferenceException: QueryArchive"))?;
        let arcBinary = queryBinary.arcBinary.as_ref().ok_or_else(|| Error::new(ErrorKind::InvalidData, "NullReferenceException: ArcBinary"))?;
        let query = arcBinary.asQueryFunc().ok_or_else(|| Error::new(ErrorKind::InvalidData, "InvalidCastException: IQueryFunc"))?.getQuery();
        Ok(Self {
            me: Weak::new(),
            archive,
            queryArchive,
            query,
            cellId: Int3::new(at(&ci, 0)?, at(&ci, 1)?, at(&ci, 2)?),
            cellInterior: cb,
            playerPosition: Vec3::new(at(&pp, 0)?, at(&pp, 1)?, at(&pp, 2)?),
            playerRotation: if pr.len() <= 3 { Quat::IDENTITY } else { Quat::from_xyzw(at(&pr, 0)?, at(&pr, 1)?, at(&pr, 2)?, at(&pr, 3)?) },
            })
    }

    /// `Arc<dyn CellDatabase + Send + Sync>`, the value `EngineRenderer` downcasts.
    fn database(&self) -> Arc<dyn CellDatabase + Send + Sync> {
        self.me.upgrade().expect("NullReferenceException: Binary_Engine")
    }
}

impl CellDatabase for Binary_Engine {
    fn archive(&self) -> SourceRef {
        let archive = self.archive.clone().expect("NullReferenceException: Archive");
        SourceRef::from(archive as Arc<dyn openx_poly::core::ISource>)
    }
    fn query(&self) -> SharedQuery { self.query.clone() }
    fn cellId(&self) -> Int3 { self.cellId }
    fn cellInterior(&self) -> bool { self.cellInterior }
    fn playerPosition(&self) -> Vec3 { self.playerPosition }
    fn playerRotation(&self) -> Quat { self.playerRotation }
}

impl IHaveMetaInfo for Binary_Engine {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> {
        // C# `{Query}` is `object.ToString()`. A `Box<dyn Query>` has no runtime type name.
        let archive = match &self.archive { Some(_) => "GameX.BinaryArchive", None => "" };
        vec![
            content("Engine", file, Tag::Any(Arc::new(self.database()))),
            MetaInfo::new(Some("Binary_Engine".into()), Tag::None, vec![
                node(format!("Archive: {archive}")),
                node("Query: IQuery".into()),
                node(format!("CellId: {},{},{}", self.cellId.x, self.cellId.y, self.cellId.z)),
                node(format!("PlayerPosition: {}", vec3String(self.playerPosition))),
            ], false),
        ]
    }
}

// #endregion

#[cfg(test)]
mod tests {
    use super::*;

    fn stream(bytes: Vec<u8>) -> Stream { Box::new(Cursor::new(bytes)) }

    #[test]
    fn txtReadsText() {
        let mut r = stream(b"hello".to_vec());
        assert_eq!(Binary_Txt::new(&mut r, 5).unwrap().data.as_deref(), Some("hello"));
    }

    #[test]
    fn tgaDecodesAndFlips() {
        // 2x2 uncompressed true color, 24 bpp, bottom-left origin (descriptor 0)
        let mut b = vec![0u8, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2, 0, 24, 0];
        b.extend_from_slice(&[1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4]);
        let mut r = stream(b);
        let tga = Binary_Tga::new(&mut r, &FileSource::default()).unwrap();
        assert_eq!((tga.width, tga.height, tga.pixelFormat), (2, 2, PIXEL::RGB24));
        let bytes = tga.create("", |s| s.downcast::<TextureAsBytes>().unwrap().bytes).unwrap();
        assert_eq!(bytes, [3, 3, 3, 4, 4, 4, 1, 1, 1, 2, 2, 2]);
    }

    #[test]
    fn tgaRejectsNoData() {
        let mut r = stream(vec![0u8; 18]);
        assert!(Binary_Tga::new(&mut r, &FileSource::default()).is_err());
    }

    #[test]
    fn imgNeedsDrawing() {
        let mut r = stream(vec![0u8; 4]);
        let mut f = FileSource::default(); f.fileSize = 4;
        assert_eq!(Binary_Img::new(&mut r, &f).err().unwrap().kind(), ErrorKind::Unsupported);
    }

    fn dxt1Header(width: u32, height: u32, mipMaps: u32) -> DDS_HEADER {
        DDS_HEADER {
            dwSize: DDS_HEADER::SIZE_OF as u32,
            dwFlags: DDSD::HEADER_FLAGS_TEXTURE,
            dwHeight: height,
            dwWidth: width,
            dwPitchOrLinearSize: 0,
            dwDepth: 0,
            dwMipMapCount: mipMaps,
            dwReserved1: [0; 11],
            ddspf: DDS_PIXELFORMAT { dwSize: DDS_PIXELFORMAT::SIZE_OF as u32, dwFlags: DDPF::FOURCC, dwFourCC: FourCC::DXT1, dwRGBBitCount: 0, dwRBitMask: 0, dwGBitMask: 0, dwBBitMask: 0, dwABitMask: 0 },
            dwCaps: DDSCAPS::TEXTURE,
            dwCaps2: DDSCAPS2::empty(),
            dwCaps3: 0, dwCaps4: 0, dwReserved2: 0,
        }
    }

    #[test]
    fn ddsSplitsThePayloadIntoMipSpans() {
        let mut bytes = Vec::new();
        dxt1Header(100, 100, 1).write(&mut bytes, None, &[1, 2, 3], true).unwrap();
        let mut r = stream(bytes);
        let dds = Binary_Dds::new(&mut r, &FileSource::default(), true).unwrap();
        assert_eq!((dds.width, dds.height, dds.mipMaps), (100, 100, 1));
        assert_eq!(dds.format.type_, "DXT1");
        assert_eq!(dds.format.blockSize, 8);
        dds.create("", |s| {
            let t = s.downcast_ref::<TextureAsBytes>().unwrap();
            assert_eq!(t.bytes, vec![1, 2, 3]);
            assert_eq!(t.spans, vec![0..3]);
        }).unwrap();
    }

    #[test]
    fn ddsRejectsAMipThatCollapsesToZero() {
        let mut bytes = Vec::new();
        dxt1Header(1, 1, 2).write(&mut bytes, None, &[0u8; 8], true).unwrap();
        let mut r = stream(bytes);
        let err = match Binary_Dds::new(&mut r, &FileSource::default(), true) { Err(e) => e, Ok(_) => panic!("a collapsed mip is not a texture") };
        assert!(err.to_string().contains("ArgumentOutOfRangeException"), "{err}");
    }

    fn plist(objects: &[Vec<u8>]) -> Vec<u8> {
        let mut out = b"bplist00".to_vec();
        let mut offsets = Vec::new();
        for object in objects {
            offsets.push(out.len());
            out.extend(object);
        }
        let table = out.len() as u64;
        for offset in offsets { out.push(offset as u8); }
        out.extend_from_slice(&[0u8; 6]);
        out.push(1);
        out.push(1);
        out.extend_from_slice(&(objects.len() as u64).to_be_bytes());
        out.extend_from_slice(&0u64.to_be_bytes());
        out.extend_from_slice(&table.to_be_bytes());
        out
    }

    fn asciiObj(text: &str) -> Vec<u8> { let mut v = vec![0x50 | text.len() as u8]; v.extend(text.as_bytes()); v }
    fn dataObj(bytes: &[u8]) -> Vec<u8> { let mut v = vec![0x40 | bytes.len() as u8]; v.extend(bytes); v }
    fn dictObj(keys: &[u8], vals: &[u8]) -> Vec<u8> { let mut v = vec![0xD0 | keys.len() as u8]; v.extend(keys); v.extend(vals); v }

    fn archive() -> BinaryArchive {
        use gamex::binary::BinaryState;
        use gamex::family::FamilyGame;
        use openx_vfx::vfx::DirectoryFileSystem;
        let state = BinaryState::new(Arc::new(DirectoryFileSystem::new("", None)), FamilyGame::empty(), None, None, Tag::None);
        BinaryArchive::new(None, state, Some(Binary_Plist::current()))
    }

    #[test]
    fn plistListsEachDataValueAndReadsItBack() {
        use gamex::binary::getResult;
        use openx_vfx::vfx::VfsRead;
        let bytes = plist(&[asciiObj("a"), dataObj(&[1, 2, 3]), asciiObj("b"), dataObj(&[9]), dictObj(&[0, 2], &[1, 3])]);
        let archive = archive();
        let mut cursor = Cursor::new(bytes);
        getResult(Binary_Plist.read(&archive, Some(&mut cursor as &mut dyn VfsRead), &Tag::None)).unwrap();
        let files = archive.files().unwrap();
        assert_eq!(files.len(), 2);
        assert_eq!((files[0].path.as_str(), files[0].fileSize), ("a", 3));
        assert_eq!(files[0].tag.asBytes(), Some(&[1, 2, 3][..]));
        assert_eq!((files[1].path.as_str(), files[1].fileSize), ("b", 1));
        let mut data = getResult(Binary_Plist.readData(&archive, None, &files[0], &Tag::None)).unwrap().unwrap();
        let mut got = Vec::new();
        data.read_to_end(&mut got).unwrap();
        assert_eq!(got, vec![1, 2, 3]);
    }

    #[test]
    fn plistRejectsAKeyThatIsNotAStringAndAValueThatIsNotBytes() {
        use gamex::binary::getResult;
        use openx_vfx::vfx::VfsRead;
        let archive = archive();
        let key = plist(&[vec![0x10, 1], dataObj(&[1]), dictObj(&[0], &[1])]);
        let mut cursor = Cursor::new(key);
        let err = getResult(Binary_Plist.read(&archive, Some(&mut cursor as &mut dyn VfsRead), &Tag::None)).err().unwrap();
        assert!(err.to_string().contains("InvalidCastException: Key"), "{err}");
        let value = plist(&[asciiObj("k"), vec![0x10, 1], dictObj(&[0], &[1])]);
        let mut cursor = Cursor::new(value);
        let err = getResult(Binary_Plist.read(&archive, Some(&mut cursor as &mut dyn VfsRead), &Tag::None)).err().unwrap();
        assert!(err.to_string().contains("InvalidCastException: Value"), "{err}");
        let mut file = FileSource::default();
        file.tag = Tag::Int(1);
        let err = getResult(Binary_Plist.readData(&archive, None, &Arc::new(file), &Tag::None)).err().unwrap();
        assert!(err.to_string().contains("InvalidCastException: Tag"), "{err}");
    }

    fn crc32(data: &[u8]) -> u32 {
        let mut crc = 0xFFFFFFFFu32;
        for &b in data {
            crc ^= u32::from(b);
            for _ in 0..8 {
                let mask = (crc & 1).wrapping_neg();
                crc = (crc >> 1) ^ (0xEDB88320 & mask);
            }
        }
        !crc
    }

    /// A stored zip. `flags` is the general-purpose bit field (bit 0 = encrypted).
    fn zipStored(entries: &[(&str, &[u8], u16)]) -> Vec<u8> {
        let mut local = Vec::new();
        let mut central = Vec::new();
        for (name, data, flags) in entries {
            let offset = local.len() as u32;
            let crc = crc32(data);
            let name = name.as_bytes();
            let size = data.len() as u32;
            local.extend_from_slice(&0x04034b50u32.to_le_bytes());
            local.extend_from_slice(&20u16.to_le_bytes());
            local.extend_from_slice(&flags.to_le_bytes());
            local.extend_from_slice(&0u16.to_le_bytes());
            local.extend_from_slice(&0u16.to_le_bytes());
            local.extend_from_slice(&0u16.to_le_bytes());
            local.extend_from_slice(&crc.to_le_bytes());
            local.extend_from_slice(&size.to_le_bytes());
            local.extend_from_slice(&size.to_le_bytes());
            local.extend_from_slice(&(name.len() as u16).to_le_bytes());
            local.extend_from_slice(&0u16.to_le_bytes());
            local.extend_from_slice(name);
            local.extend_from_slice(data);
            central.extend_from_slice(&0x02014b50u32.to_le_bytes());
            central.extend_from_slice(&20u16.to_le_bytes());
            central.extend_from_slice(&20u16.to_le_bytes());
            central.extend_from_slice(&flags.to_le_bytes());
            central.extend_from_slice(&0u16.to_le_bytes());
            central.extend_from_slice(&0u16.to_le_bytes());
            central.extend_from_slice(&0u16.to_le_bytes());
            central.extend_from_slice(&crc.to_le_bytes());
            central.extend_from_slice(&size.to_le_bytes());
            central.extend_from_slice(&size.to_le_bytes());
            central.extend_from_slice(&(name.len() as u16).to_le_bytes());
            central.extend_from_slice(&0u16.to_le_bytes());
            central.extend_from_slice(&0u16.to_le_bytes());
            central.extend_from_slice(&0u16.to_le_bytes());
            central.extend_from_slice(&0u16.to_le_bytes());
            central.extend_from_slice(&0u32.to_le_bytes());
            central.extend_from_slice(&offset.to_le_bytes());
            central.extend_from_slice(name);
        }
        let mut out = local;
        let cdOff = out.len() as u32;
        let cdSize = central.len() as u32;
        let count = entries.len() as u16;
        out.extend(central);
        out.extend_from_slice(&0x06054b50u32.to_le_bytes());
        out.extend_from_slice(&0u16.to_le_bytes());
        out.extend_from_slice(&0u16.to_le_bytes());
        out.extend_from_slice(&count.to_le_bytes());
        out.extend_from_slice(&count.to_le_bytes());
        out.extend_from_slice(&cdSize.to_le_bytes());
        out.extend_from_slice(&cdOff.to_le_bytes());
        out.extend_from_slice(&0u16.to_le_bytes());
        out
    }

    fn zipArchive(binary: Arc<dyn ArcBinary>) -> BinaryArchive {
        use gamex::binary::BinaryState;
        use gamex::family::FamilyGame;
        use openx_vfx::vfx::DirectoryFileSystem;
        let state = BinaryState::new(Arc::new(DirectoryFileSystem::new("", None)), FamilyGame::empty(), None, None, Tag::None);
        BinaryArchive::new(None, state, Some(binary))
    }

    fn readZip(binary: &Binary_Zip, bytes: Vec<u8>) -> BinaryArchive {
        use gamex::binary::getResult;
        use openx_vfx::vfx::VfsRead;
        let archive = zipArchive(Arc::new(Binary_Zip { key: binary.key.clone() }));
        let mut cursor = Cursor::new(bytes);
        getResult(binary.read(&archive, Some(&mut cursor as &mut dyn VfsRead), &Tag::None)).unwrap();
        archive
    }

    fn sampleZip() -> Vec<u8> {
        zipStored(&[
            ("dir/", b"", 0),
            ("a\\b.txt", b"hi", 0),
            ("fold\\", b"", 0),
            ("c.txt", b"z", 0),
            ("secret.txt", b"nope", 1),
        ])
    }

    #[test]
    fn zipNullKeySkipsSlashDirsAndReadsTheFile() {
        use gamex::binary::getResult;
        use std::sync::atomic::Ordering;
        let binary = Binary_Zip { key: Tag::None };
        let archive = readZip(&binary, sampleZip());
        assert!(!archive.useReader.load(Ordering::Relaxed));
        let files = archive.files().unwrap();
        let paths: Vec<&str> = files.iter().map(|s| s.path.as_str()).collect();
        assert_eq!(paths, vec!["a/b.txt", "fold/", "c.txt", "secret.txt"]);
        assert!(files.iter().all(|s| s.flags == 0));
        assert_eq!((files[0].packedSize, files[0].fileSize), (2, 2));
        let mut data = getResult(binary.readData(&archive, None, &files[0], &Tag::None)).unwrap().unwrap();
        let mut got = Vec::new();
        data.read_to_end(&mut got).unwrap();
        assert_eq!(got, b"hi");
        let err = getResult(binary.readData(&archive, None, &files[3], &Tag::None)).err().unwrap();
        assert!(!err.to_string().contains("ZipFile.Key"), "{err}");
    }

    #[test]
    fn zipStringKeySetsTheCryptedFlagAndStillReadsAPlainFile() {
        use gamex::binary::getResult;
        let binary = Binary_Zip { key: Tag::Str("secret".into()) };
        let archive = readZip(&binary, sampleZip());
        let files = archive.files().unwrap();
        let paths: Vec<&str> = files.iter().map(|s| s.path.as_str()).collect();
        assert_eq!(paths, vec!["a/b.txt", "c.txt", "secret.txt"]);
        assert_eq!(files.iter().map(|s| s.flags).collect::<Vec<_>>(), vec![0, 0, 1]);
        let mut data = getResult(binary.readData(&archive, None, &files[1], &Tag::None)).unwrap().unwrap();
        let mut got = Vec::new();
        data.read_to_end(&mut got).unwrap();
        assert_eq!(got, b"z");
        let err = getResult(binary.readData(&archive, None, &files[2], &Tag::None)).err().unwrap();
        assert!(!err.to_string().contains("ZipFile.Key"), "{err}");
    }

    #[test]
    fn zipByteKeyDoesNotDecryptAndDoesNotBecomeAPassword() {
        use gamex::binary::getResult;
        let binary = Binary_Zip { key: Tag::Bytes(vec![9, 9, 9]) };
        let archive = readZip(&binary, sampleZip());
        let files = archive.files().unwrap();
        let plain = files.iter().find(|s| s.path == "c.txt").unwrap();
        let mut data = getResult(binary.readData(&archive, None, plain, &Tag::None)).unwrap().unwrap();
        let mut got = Vec::new();
        data.read_to_end(&mut got).unwrap();
        assert_eq!(got, b"z");
        let secret = files.iter().find(|s| s.path == "secret.txt").unwrap();
        let err = getResult(binary.readData(&archive, None, secret, &Tag::None)).err().unwrap();
        assert!(err.to_string().contains("NotSupportedException: ZipFile.Key"), "{err}");
    }

    #[test]
    fn zipReadsADeflatedEntry() {
        use gamex::binary::getResult;
        use zip::write::SimpleFileOptions;
        use zip::ZipWriter;
        let mut writer = ZipWriter::new(Cursor::new(Vec::new()));
        writer.start_file("a.txt", SimpleFileOptions::default()).unwrap();
        writer.write_all(b"deflated").unwrap();
        let binary = Binary_Zip { key: Tag::None };
        let archive = readZip(&binary, writer.finish().unwrap().into_inner());
        let files = archive.files().unwrap();
        assert_eq!((files.len(), files[0].path.as_str(), files[0].fileSize), (1, "a.txt", 8));
        let mut data = getResult(binary.readData(&archive, None, &files[0], &Tag::None)).unwrap().unwrap();
        let mut got = Vec::new();
        data.read_to_end(&mut got).unwrap();
        assert_eq!(got, b"deflated");
    }

    #[test]
    fn zipRejectsAMissingReaderAndABadArchiveAndAForeignTag() {
        use gamex::binary::getResult;
        use std::sync::atomic::Ordering;
        let binary = Binary_Zip { key: Tag::None };
        let archive = zipArchive(Arc::new(Binary_Zip { key: Tag::None }));
        let err = getResult(binary.read(&archive, None, &Tag::None)).err().unwrap();
        assert!(err.to_string().contains("NullReferenceException"), "{err}");
        assert!(!archive.useReader.load(Ordering::Relaxed));
        let archive = zipArchive(Binary_Zip::instance());
        let mut cursor = Cursor::new(b"not a zip".to_vec());
        let err = getResult(binary.read(&archive, Some(&mut cursor as &mut dyn openx_vfx::vfx::VfsRead), &Tag::None)).err().unwrap();
        assert_eq!(err.kind(), ErrorKind::InvalidData);
        let mut file = FileSource::default();
        file.tag = Tag::Str("nope".into());
        let err = getResult(binary.readData(&archive, None, &Arc::new(file), &Tag::None)).err().unwrap();
        assert!(err.to_string().contains("InvalidCastException: Tag"), "{err}");
    }

    #[test]
    fn zipGetArcBinarySharesTheNullInstanceAndAStringKey() {
        use gamex::family::FamilyGame;
        let empty = FamilyGame::empty();
        assert!(Arc::ptr_eq(&Binary_Zip::getArcBinary(&empty), &Binary_Zip::instance()));
        let one = FamilyGame { key: Tag::Str("pw".into()), ..Default::default() };
        let two = FamilyGame { key: Tag::Str("pw".into()), ..Default::default() };
        let other = FamilyGame { key: Tag::Str("other".into()), ..Default::default() };
        assert!(Arc::ptr_eq(&Binary_Zip::getArcBinary(&one), &Binary_Zip::getArcBinary(&two)));
        assert!(!Arc::ptr_eq(&Binary_Zip::getArcBinary(&one), &Binary_Zip::getArcBinary(&other)));
        // C# `byte[]` keys use reference equality. `Tag::Bytes` compares by value, so equal bytes share one binary.
        let bytes = FamilyGame { key: Tag::Bytes(vec![1, 2]), ..Default::default() };
        let same = FamilyGame { key: Tag::Bytes(vec![1, 2]), ..Default::default() };
        assert!(Arc::ptr_eq(&Binary_Zip::getArcBinary(&bytes), &Binary_Zip::getArcBinary(&same)));
    }

    use gamex::binary::{getResult, newArchive, BinaryState};
    use gamex::family::{registerType, ArchiveFactory, SearchBy};
    use crate::uncore::UncoreArchive;
    use openx::manager::{Cell, IQueryFunc, Land, Ltex, Query};
    use openx_vfx::vfx::{VirtualFileSystem, VfsRead};
    use std::sync::Once;

    struct PlainPak;
    impl ArcBinary for PlainPak {
        fn read<'a>(&'a self, _source: &'a BinaryArchive, _r: Option<&'a mut dyn VfsRead>, _tag: &'a Tag) -> Task<'a, ()> { Box::pin(async { Ok(()) }) }
    }

    struct QueryPak;
    impl ArcBinary for QueryPak {
        fn read<'a>(&'a self, _source: &'a BinaryArchive, _r: Option<&'a mut dyn VfsRead>, _tag: &'a Tag) -> Task<'a, ()> { Box::pin(async { Ok(()) }) }
        fn asQueryFunc(&self) -> Option<&dyn IQueryFunc> { Some(self) }
    }
    impl IQueryFunc for QueryPak {
        fn getQuery(&self) -> SharedQuery { Arc::new(Mutex::new(Box::new(StubQuery))) }
    }

    struct StubQuery;
    impl Query for StubQuery {
        fn meterInUnits(&self) -> f32 { 0. }
        fn cellLengthInMeters(&self) -> f32 { 0. }
        fn loadRadius(&self) -> i32 { 0 }
        fn visibleRadius(&self) -> i32 { 0 }
        fn world(&self) -> i32 { 0 }
        fn setWorld(&mut self, _world: i32) {}
        fn getCellId(&self, _point: Vec3) -> Int3 { Int3::ZERO }
        fn findCell(&self, _cell: Int3) -> Option<Box<dyn Cell>> { None }
        fn findCellByName(&self, _name: &str) -> Option<Box<dyn Cell>> { None }
        fn findLand(&self, _cell: Int3) -> Option<Box<dyn Land>> { None }
        fn findLtex(&self, _index: i32) -> Option<Box<dyn Ltex>> { None }
    }

    fn engineArchive(parent: Option<Arc<dyn Archive>>, state: BinaryState) -> io::Result<Arc<dyn Archive>> {
        let binary: Arc<dyn ArcBinary> = if state.game.id == "Query" { Arc::new(QueryPak) } else { Arc::new(PlainPak) };
        Ok(newArchive(BinaryArchive::new(parent, state, Some(binary))))
    }

    fn registerEngineArchive() {
        static ONCE: Once = Once::new();
        ONCE.call_once(|| registerType::<ArchiveFactory>("GameX.Test.EngineArchive, GameX.Test", engineArchive));
    }

    fn engineParent(id: &str, files: &[(&str, Vec<u8>)]) -> Arc<dyn Archive> {
        registerEngineArchive();
        let game = Arc::new(FamilyGame {
            id: id.into(),
            archiveType: Some("GameX.Test.EngineArchive, GameX.Test".into()),
            searchBy: SearchBy::Arc,
            arcExts: Some(vec![".pak".into()]),
            ..Default::default()
        });
        let mut virtuals = std::collections::HashMap::new();
        for (path, data) in files { virtuals.insert((*path).to_string(), data.clone()); }
        let state = BinaryState::new(Arc::new(VirtualFileSystem::new(virtuals)), game, None, None, Tag::None);
        newArchive(BinaryArchive::new(None, state, None))
    }

    fn openEngine(id: &str, text: &[u8], files: &[(&str, Vec<u8>)]) -> io::Result<Tag> {
        let parent = engineParent(id, files);
        let mut r = stream(text.to_vec());
        let file = Arc::new(FileSource { path: "dir/cell.start".into(), ..Default::default() });
        getResult(Binary_Engine::factory(&mut r, file, &*parent))
    }

    fn paks() -> Vec<(&'static str, Vec<u8>)> { vec![("world.pak", vec![]), ("query.pak", vec![])] }

    fn engineDb(text: &[u8]) -> Arc<dyn CellDatabase + Send + Sync> {
        let Tag::Info(info) = openEngine("Query", text, &paks()).unwrap() else { panic!("factory") };
        let nodes = info.getInfoNodes(None, Some(&Arc::new(FileSource { path: "dir/cell.start".into(), ..Default::default() })), &Tag::None);
        let content = nodes[0].tag.asMetaContent().unwrap();
        assert_eq!((content.type_.as_str(), content.name.as_str()), ("Engine", "cell.start"));
        let db = content.value.asAny().unwrap().downcast_ref::<Arc<dyn CellDatabase + Send + Sync>>().unwrap().clone();
        assert_eq!(nodes[1].name.as_deref(), Some("Binary_Engine"));
        assert_eq!(nodes[1].items.len(), 4);
        assert_eq!(nodes[1].items[0].name.as_deref(), Some("Archive: GameX.BinaryArchive"));
        assert_eq!(nodes[1].items[1].name.as_deref(), Some("Query: IQuery"));
        assert!(nodes[1].items.iter().all(|s| s.name.as_deref().is_some_and(|n| !n.starts_with("CellInterior") && !n.starts_with("PlayerRotation"))));
        db
    }

    #[test]
    fn engineRejectsAShortFileABadBoolAndAShortCell() {
        let err = openEngine("Query", b"a:b", &paks()).err().unwrap();
        assert!(err.to_string().contains("IndexOutOfRangeException"), "{err}");
        let err = openEngine("Query", b"a:b:1,2,3:yes:0,0,0:0", &paks()).err().unwrap();
        assert!(err.to_string().contains("FormatException"), "{err}");
        let err = openEngine("Query", b"a:b:1,2,3: true:0,0,0:0", &paks()).err().unwrap();
        assert!(err.to_string().contains("FormatException"), "{err}");
        let err = openEngine("Query", b"a:b:1,x:True:0,0,0:0", &paks()).err().unwrap();
        assert!(err.to_string().contains("FormatException"), "{err}");
        // C# indexes the cell and the position after `GetArchive`, so the names have to resolve first.
        let err = openEngine("Query", b"world.pak:query.pak:1,2:True:0,0,0:0", &paks()).err().unwrap();
        assert!(err.to_string().contains("IndexOutOfRangeException"), "{err}");
        let err = openEngine("Query", b"world.pak:query.pak:1,2,3:True:1,2:0", &paks()).err().unwrap();
        assert!(err.to_string().contains("IndexOutOfRangeException"), "{err}");
    }

    #[test]
    fn engineCastFailsWhenTheArchiveIsNotABinaryOrTheBinaryIsNotAQuery() {
        let err = openEngine("Query", b"missing.pak:query.pak:1,2,3:True:0,0,0:0", &paks()).err().unwrap();
        assert!(err.to_string().contains("InvalidCastException: BinaryArchive"), "{err}");
        let err = openEngine("Plain", b"world.pak:query.pak:1,2,3:True:0,0,0:0", &paks()).err().unwrap();
        assert!(err.to_string().contains("InvalidCastException: IQueryFunc"), "{err}");
    }

    #[test]
    fn engineReadsTheCellAndKeepsAShortRotationAsIdentity() {
        let db = engineDb(b"world.pak:query.pak: 1, 2 ,3:true:1.5,2,3:0");
        assert!(db.cellInterior());
        assert_eq!(db.cellId(), Int3::new(1, 2, 3));
        let _ = db.archive();
        assert!(db.query().lock().unwrap().world() == 0);
        assert_eq!(db.playerPosition(), Vec3::new(1.5, 2.0, 3.0));
        assert_eq!(db.playerRotation(), Quat::IDENTITY);
        let nodes = {
            let Tag::Info(info) = openEngine("Query", b"world.pak:query.pak: 1, 2 ,3:true:1.5,2,3:0", &paks()).unwrap() else { panic!("factory") };
            info.getInfoNodes(None, Some(&Arc::new(FileSource { path: "dir/cell.start".into(), ..Default::default() })), &Tag::None)
        };
        assert_eq!(nodes[1].items[2].name.as_deref(), Some("CellId: 1,2,3"));
        assert_eq!(nodes[1].items[3].name.as_deref(), Some("PlayerPosition: <1.5, 2, 3>"));

        let db = engineDb(b"world.pak:query.pak:1,2,3:False:0,0,0:1,2,3");
        assert!(!db.cellInterior());
        assert_eq!(db.playerRotation(), Quat::IDENTITY);
        let db = engineDb(b"world.pak:query.pak:9,8,7,6:FALSE:4,5,6:0,1,0,0,9");
        assert_eq!(db.cellId(), Int3::new(9, 8, 7));
        assert_eq!(db.playerPosition(), Vec3::new(4.0, 5.0, 6.0));
        assert_eq!(db.playerRotation(), Quat::from_xyzw(0.0, 1.0, 0.0, 0.0));
    }

    #[test]
    fn engineAsciiReplacesBytesAbove127() {
        let files = [("caf?.pak", vec![]), ("query.pak", vec![])];
        let db = {
            let Tag::Info(info) = openEngine("Query", b"caf\xE9.pak:query.pak:4,5,6:True:0,0,0:0,0,0,1", &files).unwrap() else { panic!("factory") };
            let nodes = info.getInfoNodes(None, None, &Tag::None);
            nodes[0].tag.asMetaContent().unwrap().value.asAny().unwrap().downcast_ref::<Arc<dyn CellDatabase + Send + Sync>>().unwrap().clone()
        };
        assert_eq!((db.cellId(), db.playerRotation()), (Int3::new(4, 5, 6), Quat::IDENTITY));
    }

    #[test]
    fn startUsesBinaryEngine() {
        let parent = engineParent("Query", &[]);
        let run = |path: &str| {
            let (tag, factory) = UncoreArchive::assetFactory(&FileSource { path: path.into(), ..Default::default() }, &FamilyGame::empty());
            let mut r = stream(b"a:b".to_vec());
            let err = getResult(factory.unwrap()(&mut r, Arc::new(FileSource::default()), &*parent)).err().unwrap();
            assert_eq!(tag, Tag::Int(0));
            assert!(err.to_string().contains("IndexOutOfRangeException"), "{err}");
        };
        run("a.start");
        run("a.START");
        let (_, factory) = UncoreArchive::assetFactory(&FileSource { path: "a.zip".into(), ..Default::default() }, &FamilyGame::empty());
        assert!(factory.is_none());
    }
}
