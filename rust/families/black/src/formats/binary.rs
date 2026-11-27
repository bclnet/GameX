// PORT-SOURCE: Families/GameX.Black/Formats/Binary.cs
// PORT-SHA: unstamped
// PORT-STATUS: partial
//
// Ported: Binary_Dat (Fallout magic 1, Fallout2 magic 0x11), Binary_Frm,
// Binary_Pal2, Binary_Rix, and Binary_Map (Fallout MAP elevations; the C#
// file does not have that reader yet). Any other game id leaves files unset.
//
// `gamex::vendor::compression::lzss` is still a stub, and uncore has no
// `decompressLzss`. F1 calls the C# `Lzss` algorithm (big-endian block size,
// 4096 window, match 3..18). It is not a different codec.
// Frm reads `Header.FrameOffset` before `Header` is assigned in C# and ignores
// it. This reader does not seek. `HasFrames` and `NextFrame` stay false.
// `ITexture::select` does not retarget: `Select(0)` already filled Bytes.
// `toSheet` packs RGBA8 cells. FrmFrame has no delay (0.1s). Six facings are
// ne, e, se, sw, w, nw when the frame count is 6 * FramesPerDirection.

use std::io::{self, Cursor, Error, ErrorKind, Read, Seek, SeekFrom};
use std::sync::atomic::Ordering;
use std::sync::{Arc, LazyLock, Mutex, Weak};
use bytemuck::{Pod, Zeroable};
use gamex::binary::{ArcBinary, Archive, BinaryArchive, Stream, Task};
use gamex::meta::{fileName, FileSource, IHaveMetaInfo, MetaContent, MetaInfo, MetaManager, Tag};
use gamex_uncore::formats::compression::Compression;
use openx_gfx::gfx::{AnyArc, GfxSpriteSheet, ITexture, TextureAsBytes};
use openx_gfx::gfx_texture::{TextureFlags, TextureFormat, TexturePixel};
use openx_poly::prelude::{BinaryReaderExt, IStruct};
use openx_vfx::vfx::VfsRead;

fn fromResult<'a>(value: io::Result<Tag>) -> Task<'a, Tag> { Box::pin(std::future::ready(value)) }
fn node(name: String) -> MetaInfo { MetaInfo::new(Some(name), Tag::None, vec![], false) }
fn content(type_: &str, file: Option<&Arc<FileSource>>, value: Tag) -> MetaInfo {
    MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent { type_: type_.into(), name: file.map_or(String::new(), |s| fileName(&s.path)), value, ..Default::default() })), vec![], false)
}
fn thisAny<T: std::any::Any + Send + Sync + 'static>(me: &Weak<T>) -> Tag { me.upgrade().map_or(Tag::None, |s| Tag::Any(s)) }
fn thisTexture<T: ITexture + 'static>(me: &Weak<T>) -> Tag { me.upgrade().map_or(Tag::None, |s| Tag::Texture(s)) }

fn streamLen(r: &mut dyn VfsRead) -> io::Result<u64> {
    let pos = r.stream_position()?;
    let len = r.seek(SeekFrom::End(0))?;
    r.seek(SeekFrom::Start(pos))?;
    Ok(len)
}

fn slash(s: String) -> String { s.replace('\\', "/") }

/// C# `ReadL8UString`. A zero length is null there; `Replace` would throw, so this is empty.
fn readL8UString(r: &mut dyn VfsRead) -> io::Result<String> {
    let n = r.readByte()? as usize;
    if n == 0 { return Ok(String::new()); }
    let b = r.readBytes(n)?;
    Ok(String::from_utf8_lossy(&b).trim_end_matches('\0').to_string())
}

fn peekU16(r: &mut dyn VfsRead) -> io::Result<u16> {
    let pos = r.stream_position()?;
    let v = r.readUInt16()?;
    r.seek(SeekFrom::Start(pos))?;
    Ok(v)
}

/// VGA 6-bit channels shifted by 2. `clear0` leaves index 0 transparent for FRM.
fn shiftColor(b0: u8, b1: u8, b2: u8, alpha: u32) -> u32 {
    (alpha << 24) | ((b2 as u32) << 18) | ((b1 as u32) << 10) | ((b0 as u32) << 2)
}

fn readPalette(r: &mut dyn VfsRead, clear0: bool) -> io::Result<[u32; 256]> {
    let rgb = r.readBytes(256 * 3)?;
    let mut rgba = [0u32; 256];
    for i in 0..256 {
        let alpha = if clear0 && i == 0 { 0 } else { 0xFF };
        rgba[i] = shiftColor(rgb[i * 3], rgb[i * 3 + 1], rgb[i * 3 + 2], alpha);
    }
    Ok(rgba)
}

fn paint(indices: &[u8], rgba: &[u32; 256], width: usize, height: usize) -> io::Result<Vec<u8>> {
    let pixels = width.checked_mul(height).ok_or_else(|| Error::new(ErrorKind::InvalidInput, "OverflowException"))?;
    if indices.len() > pixels { return Err(Error::new(ErrorKind::InvalidData, "IndexOutOfRangeException")); }
    let mut image = vec![0u8; pixels * 4];
    for (j, &ix) in indices.iter().enumerate() {
        let px = rgba[ix as usize].to_le_bytes();
        image[j * 4..j * 4 + 4].copy_from_slice(&px);
    }
    Ok(image)
}

// #region Binary_Dat

const F1_HEADER_FILEID: u32 = 0x00000001;
const F2_HEADER_FILEID: u32 = 0x00000011;

#[repr(C, packed)]
#[derive(Clone, Copy, Pod, Zeroable)]
struct F1_Header {
    DirectoryCount: u32,
    Unknown1: u32,
    Unknown2: u32,
    Unknown3: u32,
}
impl IStruct for F1_Header { const Struct: (&'static str, usize) = (">4I", 16); }

#[repr(C, packed)]
#[derive(Clone, Copy, Pod, Zeroable)]
struct F1_Directory {
    FileCount: u32,
    Unknown1: u32,
    Unknown2: u32,
    Unknown3: u32,
}
impl IStruct for F1_Directory { const Struct: (&'static str, usize) = (">4I", 16); }

#[repr(C, packed)]
#[derive(Clone, Copy, Pod, Zeroable)]
struct F1_File {
    Attributes: u32,
    Offset: u32,
    Size: u32,
    PackedSize: u32,
}
impl IStruct for F1_File { const Struct: (&'static str, usize) = (">4I", 16); }

#[repr(C, packed)]
#[derive(Clone, Copy, Pod, Zeroable)]
struct F2_Header {
    TreeSize: u32,
    DataSize: u32,
}
impl IStruct for F2_Header { const Struct: (&'static str, usize) = ("<2I", 8); }

#[repr(C, packed)]
#[derive(Clone, Copy, Pod, Zeroable)]
struct F2_File {
    Type: u8,
    RealSize: u32,
    PackedSize: u32,
    Offset: u32,
}
impl IStruct for F2_File { const Struct: (&'static str, usize) = ("<B3I", 13); }

const _: () = assert!(std::mem::size_of::<F2_File>() == 13);

pub struct Binary_Dat;

impl Binary_Dat {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_Dat));
        CURRENT.clone()
    }
}

impl ArcBinary for Binary_Dat {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, _tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let r = r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "NullReferenceException: r"))?;
            readDat(source, r)
        })
    }

    fn readData<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, _option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async move {
            let r = r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "NullReferenceException: r"))?;
            let bytes = readDatData(source, r, file)?;
            Ok(Some(Box::new(Cursor::new(bytes)) as Stream))
        })
    }
}

fn readDat(source: &BinaryArchive, r: &mut dyn VfsRead) -> io::Result<()> {
    let gameId = source.base.game.id.as_str();
    // Fallout
    if gameId == "Fallout" {
        source.magic.store(F1_HEADER_FILEID, Ordering::Relaxed);
        let header = r.readS::<F1_Header>()?;
        let directoryCount = header.DirectoryCount as usize;
        let mut directoryPaths = Vec::with_capacity(directoryCount);
        for _ in 0..directoryCount { directoryPaths.push(slash(readL8UString(r)?)); }
        let mut files = Vec::new();
        for i in 0..directoryCount {
            let directory = r.readS::<F1_Directory>()?;
            let directoryPath = if directoryPaths[i] != "." { format!("{}/", directoryPaths[i]) } else { String::new() };
            for _ in 0..directory.FileCount {
                let path = directoryPath.clone() + &slash(readL8UString(r)?);
                let file = r.readS::<F1_File>()?;
                files.push(Arc::new(FileSource {
                    path,
                    compressed: (file.Attributes & 0x40) as i32,
                    offset: file.Offset as i64,
                    fileSize: file.Size as i64,
                    packedSize: file.PackedSize as i64,
                    ..Default::default()
                }));
            }
        }
        source.setFiles(Some(files));
    }
    // Fallout2
    else if gameId == "Fallout2" {
        source.magic.store(F2_HEADER_FILEID, Ordering::Relaxed);
        let len = streamLen(r)?;
        r.seek(SeekFrom::Start(len - 8))?;
        let header = r.readS::<F2_Header>()?;
        if header.DataSize as u64 != len { return Err(Error::new(ErrorKind::InvalidData, "File is not a valid bsa archive.")); }
        let at = header.DataSize.wrapping_sub(header.TreeSize).wrapping_sub(8) as u64;
        r.seek(SeekFrom::Start(at))?;
        let count = r.readInt32()?;
        if count < 0 { return Err(Error::new(ErrorKind::InvalidInput, "OverflowException")); }
        let mut files = Vec::with_capacity(count as usize);
        for _ in 0..count {
            let path = slash(r.readL32UString(0, false)?.unwrap_or_default());
            let file = r.readS::<F2_File>()?;
            files.push(Arc::new(FileSource {
                path,
                compressed: file.Type as i32,
                fileSize: file.RealSize as i64,
                packedSize: file.PackedSize as i64,
                offset: file.Offset as i64,
                ..Default::default()
            }));
        }
        source.setFiles(Some(files));
    }
    Ok(())
}

fn readDatData(source: &BinaryArchive, r: &mut dyn VfsRead, file: &FileSource) -> io::Result<Vec<u8>> {
    let magic = source.magic.load(Ordering::Relaxed);
    let packed = file.packedSize as i32;
    if packed < 0 { return Err(Error::new(ErrorKind::InvalidInput, "OverflowException")); }
    // F1
    if magic == F1_HEADER_FILEID {
        r.seek(SeekFrom::Start(file.offset as u64))?;
        if file.compressed == 0 { return r.readBytes(packed as usize); }
        let size = file.fileSize as i32;
        if size < 0 { return Err(Error::new(ErrorKind::InvalidInput, "OverflowException")); }
        return decompressLzss(r, packed, size);
    }
    // F2
    if magic == F2_HEADER_FILEID {
        r.seek(SeekFrom::Start(file.offset as u64))?;
        if peekU16(r)? == 0xda78 { return r.decompressZlib(packed, -1, false); }
        return r.readBytes(packed as usize);
    }
    Err(Error::new(ErrorKind::InvalidData, "BAD MAGIC"))
}

// C# `Compression.Lzss`. `BinaryReaderE.ReadInt16` is big-endian.
const DICT_SIZE: usize = 4096;
const MIN_MATCH: usize = 3;
const MAX_MATCH: usize = 18;

struct Lzss<'a> {
    src: &'a [u8],
    pos: usize,
    uncompressedSize: usize,
    output: Vec<u8>,
    dictionary: Vec<u8>,
    NR: i32,
    DO: usize,
    DI: usize,
    OI: usize,
}

fn decompressLzss(r: &mut dyn VfsRead, length: i32, newLength: i32) -> io::Result<Vec<u8>> {
    if length < 0 || newLength < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")); }
    let src = r.readBytes(length as usize)?;
    Lzss::decompress(&src, newLength as usize)
}

impl<'a> Lzss<'a> {
    fn decompress(src: &'a [u8], uncompressedSize: usize) -> io::Result<Vec<u8>> {
        let mut z = Lzss {
            src,
            pos: 0,
            uncompressedSize,
            output: vec![0u8; uncompressedSize],
            dictionary: vec![0u8; DICT_SIZE],
            NR: 0,
            DO: 0,
            DI: 0,
            OI: 0,
        };
        while !z.lastByte() {
            let n = z.readInt16()?;
            if n != 0 { z.readBlock(n)?; }
            else { break; }
        }
        Ok(z.output)
    }

    fn lastByte(&self) -> bool { self.pos == self.src.len() }

    fn clearDict(&mut self) {
        self.dictionary.fill(32);
        self.DI = DICT_SIZE - MAX_MATCH;
    }

    fn readByte(&mut self) -> io::Result<u8> {
        self.NR += 1;
        if self.pos >= self.src.len() { return Err(Error::new(ErrorKind::UnexpectedEof, "EndOfStreamException")); }
        let b = self.src[self.pos];
        self.pos += 1;
        Ok(b)
    }

    fn readBytes(&mut self, n: usize) -> io::Result<Vec<u8>> {
        if self.pos + n > self.src.len() { return Err(Error::new(ErrorKind::UnexpectedEof, "EndOfStreamException")); }
        let b = self.src[self.pos..self.pos + n].to_vec();
        self.pos += n;
        Ok(b)
    }

    fn writeByte(&mut self, b: u8) -> io::Result<()> {
        if self.OI >= self.uncompressedSize { return Err(Error::new(ErrorKind::InvalidData, "IndexOutOfRangeException")); }
        self.output[self.OI] = b;
        self.OI += 1;
        self.dictionary[self.DI % DICT_SIZE] = b;
        self.DI += 1;
        Ok(())
    }

    fn writeBytes(&mut self, bytes: &[u8]) {
        for &b in bytes {
            if self.OI >= self.uncompressedSize { break; }
            self.output[self.OI] = b;
            self.OI += 1;
        }
    }

    fn readDict(&mut self) -> u8 {
        let b = self.dictionary[self.DO % DICT_SIZE];
        self.DO += 1;
        b
    }

    fn readInt16(&mut self) -> io::Result<i32> {
        if self.pos + 2 > self.src.len() { return Err(Error::new(ErrorKind::UnexpectedEof, "EndOfStreamException")); }
        let n = i16::from_be_bytes([self.src[self.pos], self.src[self.pos + 1]]);
        self.pos += 2;
        Ok(n as i32)
    }

    fn readBlock(&mut self, n: i32) -> io::Result<()> {
        self.NR = 0;
        if n < 0 {
            let bytes = self.readBytes((-n) as usize)?;
            self.writeBytes(&bytes);
            return Ok(());
        }
        self.clearDict();
        while self.NR < n && !self.lastByte() {
            let mut num1 = self.readByte()?;
            if self.NR >= n || self.lastByte() { break; }
            for _ in 0..8 {
                if num1 % 2 == 1 {
                    let b = self.readByte()?;
                    self.writeByte(b)?;
                    if self.NR >= n { return Ok(()); }
                }
                else {
                    if self.NR >= n { return Ok(()); }
                    self.DO = self.readByte()? as usize;
                    if self.NR >= n { return Ok(()); }
                    let num2 = self.readByte()? as usize;
                    self.DO |= (num2 & 0xF0) << 4;
                    let num3 = (num2 & 0x0F) + MIN_MATCH;
                    for _ in 0..num3 { let b = self.readDict(); self.writeByte(b)?; }
                }
                num1 >>= 1;
                if self.lastByte() { return Ok(()); }
            }
        }
        Ok(())
    }
}

// #endregion

// #region Binary_Frm

#[repr(C, packed)]
#[derive(Clone, Copy, Pod, Zeroable)]
pub struct FrmHeader {
    pub Version: u32,
    pub Fps: u16,
    pub ActionFrame: u16,
    pub FramesPerDirection: u16,
    pub PixelShiftX: [i16; 6],
    pub PixelShiftY: [i16; 6],
    pub FrameOffset: [u32; 6],
    pub SizeOfFrame: u32,
}
impl IStruct for FrmHeader { const Struct: (&'static str, usize) = (">I3H6h6h6II", 62); }
const _: () = assert!(std::mem::size_of::<FrmHeader>() == 62);

#[repr(C, packed)]
#[derive(Clone, Copy, Pod, Zeroable)]
pub struct FrmFrame {
    pub Width: u16,
    pub Height: u16,
    pub Size: u32,
    pub PixelShiftX: i16,
    pub PixelShiftY: i16,
}
impl IStruct for FrmFrame { const Struct: (&'static str, usize) = (">2HI2h", 12); }
const _: () = assert!(std::mem::size_of::<FrmFrame>() == 12);

static DEFAULT_PALLET: Mutex<Option<Arc<Binary_Pal2>>> = Mutex::new(None);

pub struct Binary_Frm {
    pub Header: FrmHeader,
    pub Frames: Vec<(FrmFrame, Vec<u8>)>,
    pub Pallet: Option<[u32; 256]>,
    pub Bytes: Vec<u8>,
    pub Width: i32,
    pub Height: i32,
    me: Weak<Binary_Frm>,
}

impl Binary_Frm {
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> {
        Box::pin(async move {
            let pallet = palletFor(s, &f.path).await?;
            let frm = Binary_Frm::read(r, &pallet)?;
            Ok(Tag::Info(Arc::new_cyclic(|w| {
                let mut frm = frm;
                frm.me = w.clone();
                frm
            }) as Arc<dyn IHaveMetaInfo>))
        })
    }

    fn read(r: &mut dyn VfsRead, pallet: &Binary_Pal2) -> io::Result<Self> {
        let rgba32 = &pallet.Rgba32;
        let header = r.readS::<FrmHeader>()?;
        let framesPerDirection = header.FramesPerDirection;
        let mut frames = Vec::new();
        let len = streamLen(r)?;
        let mut i = 0usize;
        let limit = 6usize * framesPerDirection as usize;
        while i < limit && r.stream_position()? < len {
            // C# reads Header.FrameOffset[i] here, before Header is assigned, and ignores it.
            let frame = r.readS::<FrmFrame>()?;
            let data = r.readBytes(frame.Size as usize)?;
            let image = paint(&data, rgba32, frame.Width as usize, frame.Height as usize)?;
            frames.push((frame, image));
            i += 1;
        }
        let (bytes, width, height) = selectFrame(&frames, 0)?;
        Ok(Self { Header: header, Frames: frames, Pallet: Some(pallet.Rgba32), Bytes: bytes, Width: width, Height: height, me: Weak::new() })
    }

    /// Expands each frame to RGBA8 and packs one anim per facing, or `all`.
    pub fn toSheet(&self) -> GfxSpriteSheet {
        const DURATION: f32 = 0.1;
        let mut cells = Vec::with_capacity(self.Frames.len());
        for (frame, pixels) in &self.Frames {
            let rgba = expandFrm(frame, pixels, self.Pallet.as_ref());
            cells.push((rgba, frame.Width as i32, frame.Height as i32, DURATION));
        }
        GfxSpriteSheet::pack(&cells, &frmAnims(self.Header.FramesPerDirection, self.Frames.len()))
    }

    const FORMAT: (TextureFormat, TexturePixel) = (TextureFormat::RGBA32, TexturePixel::Unknown);

    pub fn create<T>(&self, _platform: &str, func: impl FnOnce(Box<dyn std::any::Any>) -> T) -> io::Result<T> {
        Ok(func(Box::new(TextureAsBytes { bytes: self.Bytes.clone(), format: Arc::new(Self::FORMAT) as AnyArc, spans: vec![] })))
    }
}

fn expandFrm(frame: &FrmFrame, pixels: &[u8], pallet: Option<&[u32; 256]>) -> Vec<u8> {
    let n = frame.Width as usize * frame.Height as usize;
    if n == 0 { return Vec::new(); }
    if pixels.len() >= n * 4 { return pixels[..n * 4].to_vec(); }
    let mut rgba = vec![0u8; n * 4];
    for i in 0..n {
        let ix = if i < pixels.len() { pixels[i] } else { 0 };
        let c = match pallet {
            Some(p) => p[ix as usize],
            None if ix == 0 => 0,
            None => 0xFF00_0000 | ((ix as u32) << 16) | ((ix as u32) << 8) | ix as u32,
        };
        rgba[i * 4..i * 4 + 4].copy_from_slice(&c.to_le_bytes());
    }
    rgba
}

// Orientation 0..5. Six anims only when every facing was stored.
fn frmAnims(fpd: u16, n: usize) -> Vec<(String, Vec<i32>)> {
    if fpd > 0 && n == 6 * fpd as usize {
        const FACING: [&str; 6] = ["ne", "e", "se", "sw", "w", "nw"];
        return FACING.iter().enumerate().map(|(d, name)| {
            let start = d * fpd as usize;
            let frames = (0..fpd as usize).map(|i| (start + i) as i32).collect();
            ((*name).to_string(), frames)
        }).collect();
    }
    vec![("all".to_string(), (0..n as i32).collect())]
}

fn selectFrame(frames: &[(FrmFrame, Vec<u8>)], id: usize) -> io::Result<(Vec<u8>, i32, i32)> {
    let (frame, bytes) = frames.get(id).ok_or_else(|| Error::new(ErrorKind::InvalidData, "IndexOutOfRangeException"))?;
    Ok((bytes.clone(), frame.Width as i32, frame.Height as i32))
}

async fn palletFor(s: &dyn Archive, path: &str) -> io::Result<Binary_Pal2> {
    if path.len() < 4 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")); }
    let palletPath = format!("{}.PAL", &path[..path.len() - 4]);
    if s.contains(&Tag::Str(palletPath.clone()))? {
        return readPal(s, &palletPath).await;
    }
    if let Some(p) = DEFAULT_PALLET.lock().unwrap().clone() { return Ok((*p).clone()); }
    let color = Tag::Str("COLOR.PAL".into());
    if s.contains(&color)? {
        let mut p = readPal(s, "COLOR.PAL").await?;
        p.setColors();
        *DEFAULT_PALLET.lock().unwrap() = Some(Arc::new(p.clone()));
        return Ok(p);
    }
    Err(Error::new(ErrorKind::NotFound, "No pallet found"))
}

async fn readPal(s: &dyn Archive, path: &str) -> io::Result<Binary_Pal2> {
    let key = Tag::Str(path.to_string());
    let none = Tag::None;
    let mut data = s.getData(&key, &none, true).await?.ok_or_else(|| Error::new(ErrorKind::NotFound, "No pallet found"))?;
    Binary_Pal2::read(&mut *data)
}

impl ITexture for Binary_Frm {
    fn width(&self) -> i32 { self.Width }
    fn height(&self) -> i32 { self.Height }
    fn depth(&self) -> i32 { 0 }
    fn mipMaps(&self) -> i32 { 1 }
    fn texFlags(&self) -> TextureFlags { TextureFlags::empty() }
    fn fps(&self) -> i32 { self.Header.Fps as i32 }
    fn maxId(&self) -> i32 { if self.Frames.len() == 1 { 1 } else { self.Header.FramesPerDirection as i32 } }
    fn hasFrames(&self) -> bool { false }
    fn nextFrame(&self) -> bool { false }
    fn create(&self, platform: &str, func: &mut dyn FnMut(&dyn std::any::Any)) -> io::Result<()> { Self::create(self, platform, |x| func(&*x)) }
}

impl IHaveMetaInfo for Binary_Frm {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Texture", file, thisTexture(&self.me)),
        MetaInfo::new(Some("Binary_Frm".into()), Tag::None, vec![
            node(format!("Frames: {}", self.Frames.len())),
            node(format!("Width: {}", self.Width)),
            node(format!("Height: {}", self.Height)),
        ], false),
    ] }
}

// #endregion

// #region Binary_Pal2

#[derive(Clone)]
pub struct Binary_Pal2 {
    pub Rgba32: [u32; 256],
    me: Weak<Binary_Pal2>,
}

impl Binary_Pal2 {
    pub fn factory<'a>(r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Self::read(r).map(|s| Tag::Info(Arc::new_cyclic(|w| {
            let mut s = s;
            s.me = w.clone();
            s
        }) as Arc<dyn IHaveMetaInfo>)))
    }

    pub fn read(r: &mut dyn VfsRead) -> io::Result<Self> {
        Ok(Self { Rgba32: readPalette(r, true)?, me: Weak::new() })
    }

    pub fn setColors(&mut self) {
        for i in 229..=232 { self.Rgba32[i] = 0xffff0000; }
        for i in 233..=237 { self.Rgba32[i] = 0xff00ff00; }
        for i in 238..=247 { self.Rgba32[i] = 0xff000000; }
        for i in 248..=254 { self.Rgba32[i] = 0xff00ff00; }
    }
}

impl IHaveMetaInfo for Binary_Pal2 {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Null", file, thisAny(&self.me)),
        MetaInfo::new(Some("Binary_Pal2".into()), Tag::None, vec![], false),
    ] }
}

// #endregion

// #region Binary_Rix

#[repr(C, packed)]
#[derive(Clone, Copy, Pod, Zeroable)]
struct RixHeader {
    Magic: u32,
    Width: u16,
    Height: u16,
    PaletteType: u8,
    StorageType: u8,
}
impl IStruct for RixHeader { const Struct: (&'static str, usize) = ("<I2H2B", 10); }
const _: () = assert!(std::mem::size_of::<RixHeader>() == 10);

pub struct Binary_Rix {
    bytes: Vec<u8>,
    pub Width: i32,
    pub Height: i32,
    me: Weak<Binary_Rix>,
}

impl Binary_Rix {
    pub fn factory<'a>(r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Self::read(r).map(|s| Tag::Info(Arc::new_cyclic(|w| {
            let mut s = s;
            s.me = w.clone();
            s
        }) as Arc<dyn IHaveMetaInfo>)))
    }

    pub fn read(r: &mut dyn VfsRead) -> io::Result<Self> {
        let header = r.readS::<RixHeader>()?;
        let rgba = readPalette(r, false)?;
        let width = header.Width as usize;
        let height = header.Height as usize;
        let data = r.readBytes(width * height)?;
        let bytes = paint(&data, &rgba, width, height)?;
        Ok(Self { bytes, Width: header.Width as i32, Height: header.Height as i32, me: Weak::new() })
    }

    const FORMAT: (TextureFormat, TexturePixel) = (TextureFormat::RGBA32, TexturePixel::Unknown);

    pub fn create<T>(&self, _platform: &str, func: impl FnOnce(Box<dyn std::any::Any>) -> T) -> io::Result<T> {
        Ok(func(Box::new(TextureAsBytes { bytes: self.bytes.clone(), format: Arc::new(Self::FORMAT) as AnyArc, spans: vec![] })))
    }
}

impl ITexture for Binary_Rix {
    fn width(&self) -> i32 { self.Width }
    fn height(&self) -> i32 { self.Height }
    fn depth(&self) -> i32 { 0 }
    fn mipMaps(&self) -> i32 { 1 }
    fn texFlags(&self) -> TextureFlags { TextureFlags::empty() }
    fn create(&self, platform: &str, func: &mut dyn FnMut(&dyn std::any::Any)) -> io::Result<()> { Self::create(self, platform, |x| func(&*x)) }
}

impl IHaveMetaInfo for Binary_Rix {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Texture", file, thisTexture(&self.me)),
        MetaInfo::new(Some("Binary_Rix".into()), Tag::None, vec![
            node(format!("Width: {}", self.Width)),
            node(format!("Height: {}", self.Height)),
        ], false),
    ] }
}

// #endregion

// #region Binary_Map

pub struct MapLevel {
    pub Elevation: i32,
    pub Floor: Vec<u16>,
    pub Roof: Vec<u16>,
}

pub struct Binary_Map {
    pub Version: i32,
    pub Name: String,
    pub EnteringTile: i32,
    pub EnteringElevation: i32,
    pub EnteringRotation: i32,
    pub Flags: i32,
    pub MapIndex: i32,
    pub Levels: Vec<MapLevel>,
    me: Weak<Binary_Map>,
}

impl Binary_Map {
    pub fn factory<'a>(r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Self::read(r).map(|s| Tag::Info(Arc::new_cyclic(|w| {
            let mut s = s;
            s.me = w.clone();
            s
        }) as Arc<dyn IHaveMetaInfo>)))
    }

    pub fn read(r: &mut dyn VfsRead) -> io::Result<Self> {
        let version = r.readInt32E()?;
        if version != 19 && version != 20 {
            return Err(Error::new(ErrorKind::InvalidData, format!("InvalidOperationException: {version}")));
        }
        let name = asciiName(&r.readBytes(16)?);
        let enteringTile = r.readInt32E()?;
        let enteringElevation = r.readInt32E()?;
        let enteringRotation = r.readInt32E()?;
        let localVarsCount = r.readInt32E()?;
        let _scriptIndex = r.readInt32E()?;
        let flags = r.readInt32E()?;
        let _darkness = r.readInt32E()?;
        let globalVarsCount = r.readInt32E()?;
        let mapIndex = r.readInt32E()?;
        let _lastVisitTime = r.readUInt32E()?;
        r.skip(44 * 4)?;
        skipCount(r, globalVarsCount)?;
        skipCount(r, localVarsCount)?;
        let absent = [0x2, 0x4, 0x8];
        let mut levels = Vec::new();
        for elevation in 0..3 {
            if flags & absent[elevation] != 0 { continue; }
            let mut floor = Vec::with_capacity(10000);
            let mut roof = Vec::with_capacity(10000);
            for _ in 0..10000 {
                let entry = r.readUInt32E()?;
                roof.push(((entry >> 16) & 0x0FFF) as u16);
                floor.push((entry & 0x0FFF) as u16);
            }
            levels.push(MapLevel { Elevation: elevation as i32, Floor: floor, Roof: roof });
        }
        Ok(Self {
            Version: version,
            Name: name,
            EnteringTile: enteringTile,
            EnteringElevation: enteringElevation,
            EnteringRotation: enteringRotation,
            Flags: flags,
            MapIndex: mapIndex,
            Levels: levels,
            me: Weak::new(),
        })
    }
}

fn asciiName(bytes: &[u8]) -> String {
    let mut s = String::new();
    for &c in bytes {
        if c == 0 { break; }
        s.push(if c > 127 { '?' } else { c as char });
    }
    s
}

fn skipCount(r: &mut dyn VfsRead, count: i32) -> io::Result<()> {
    let n = if count < 0 { 0 } else { count as i64 };
    r.skip(n * 4)?;
    Ok(())
}

impl IHaveMetaInfo for Binary_Map {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Object", file, thisAny(&self.me)),
        MetaInfo::new(Some("Binary_Map".into()), Tag::None, vec![
            node(format!("Version: {}", self.Version)),
            node(format!("Name: {}", self.Name)),
            node(format!("Levels: {}", self.Levels.len())),
        ], false),
    ] }
}

// #endregion

#[cfg(test)]
mod tests {
    use super::*;
    use openx_gfx::gfx::{GfxSpriteImage, GfxSpriteSheet};
    use std::any::Any;

    #[test]
    fn mapRejectsOtherVersionsAndSkipsAbsentElevations() {
        let mut bad = Cursor::new(21i32.to_be_bytes().to_vec());
        let Err(err) = Binary_Map::read(&mut bad) else { panic!("version 21"); };
        assert!(err.to_string().contains("21"), "{err}");

        let mut ok = Cursor::new(mapHeader(19, 0xE));
        let map = Binary_Map::read(&mut ok).unwrap();
        assert_eq!(map.Version, 19);
        assert_eq!(map.Flags, 0xE);
        assert!(map.Levels.is_empty());
        assert_eq!(ok.position(), 0xEC);
    }

    fn mapHeader(version: i32, flags: i32) -> Vec<u8> {
        let mut b = Vec::new();
        b.extend_from_slice(&version.to_be_bytes());
        b.extend_from_slice(&[0u8; 16]);
        for v in [0, 0, 0, 0, 0, flags, 0, 0, 0] { b.extend_from_slice(&v.to_be_bytes()); }
        b.extend_from_slice(&0u32.to_be_bytes());
        b.extend_from_slice(&[0u8; 44 * 4]);
        assert_eq!(b.len(), 0xEC);
        b
    }

    #[test]
    fn frmToSheetPacksFakeFrames() {
        let gray = bareFrm(2, vec![
            (frmFrame(2, 1), vec![0, 8]),
            (frmFrame(2, 1), vec![0, 16]),
        ], None);
        let sheet = gray.toSheet();
        assert_eq!(sheet.anims.len(), 1);
        assert_eq!(sheet.anims[0].name, "all");
        assert_eq!(sheet.anims[0].frames, vec![0, 1]);
        assert_eq!(sheet.frames[0].duration, 0.1);
        assert_eq!(sheet.frames[1].duration, 0.1);
        let img = sheetImage(&sheet);
        assert_eq!(img.width, 4);
        assert_eq!(img.height, 1);
        assert_eq!(&img.rgba[0..4], &[0, 0, 0, 0]);
        assert_eq!(&img.rgba[4..8], &[8, 8, 8, 255]);
        assert_eq!(&img.rgba[8..12], &[0, 0, 0, 0]);
        assert_eq!(&img.rgba[12..16], &[16, 16, 16, 255]);

        let mut pal = [0u32; 256];
        pal[0] = shiftColor(1, 2, 3, 0);
        pal[1] = shiftColor(10, 20, 30, 0xFF);
        let painted = bareFrm(1, vec![(frmFrame(1, 2), vec![0, 1])], Some(pal));
        let paintedSheet = painted.toSheet();
        assert_eq!(paintedSheet.anims[0].name, "all");
        let paintedImg = sheetImage(&paintedSheet);
        assert_eq!(&paintedImg.rgba[0..4], &shiftColor(1, 2, 3, 0).to_le_bytes());
        assert_eq!(&paintedImg.rgba[4..8], &shiftColor(10, 20, 30, 0xFF).to_le_bytes());

        let kept = bareFrm(1, vec![(frmFrame(1, 1), vec![9, 8, 7, 6])], None);
        assert_eq!(&sheetImage(&kept.toSheet()).rgba[0..4], &[9, 8, 7, 6]);

        let facing = bareFrm(1, (0..6).map(|i| (frmFrame(1, 1), vec![i + 1])).collect(), None);
        let facingSheet = facing.toSheet();
        let names: Vec<&str> = facingSheet.anims.iter().map(|a| a.name.as_str()).collect();
        assert_eq!(names, ["ne", "e", "se", "sw", "w", "nw"]);
        for (d, anim) in facingSheet.anims.iter().enumerate() {
            assert_eq!(anim.frames, vec![d as i32]);
        }
        let facingImg = sheetImage(&facingSheet);
        assert_eq!(facingImg.width, 6);
        assert_eq!(&facingImg.rgba[0..4], &[1, 1, 1, 255]);
        assert_eq!(&facingImg.rgba[20..24], &[6, 6, 6, 255]);
    }

    fn frmFrame(w: u16, h: u16) -> FrmFrame {
        FrmFrame { Width: w, Height: h, Size: w as u32 * h as u32, PixelShiftX: 0, PixelShiftY: 0 }
    }

    fn bareFrm(fpd: u16, frames: Vec<(FrmFrame, Vec<u8>)>, pallet: Option<[u32; 256]>) -> Binary_Frm {
        Binary_Frm {
            Header: FrmHeader {
                Version: 0, Fps: 10, ActionFrame: 0, FramesPerDirection: fpd,
                PixelShiftX: [0; 6], PixelShiftY: [0; 6], FrameOffset: [0; 6], SizeOfFrame: 0,
            },
            Frames: frames,
            Pallet: pallet,
            Bytes: vec![],
            Width: 0,
            Height: 0,
            me: Weak::new(),
        }
    }

    fn sheetImage(sheet: &GfxSpriteSheet) -> GfxSpriteImage {
        let mut found = None;
        sheet.texture.create("", &mut |any| { found = any.downcast_ref::<GfxSpriteImage>().cloned(); }).unwrap();
        found.expect("GfxSpriteImage")
    }
}
