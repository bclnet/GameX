// PORT-SOURCE: Families/GameX.Uncore/Formats/Binary.cs
// PORT-SHA: 26b9cbb734746358
// PORT-STATUS: partial
//
// Ported: Binary_Bik, Binary_Fsb, Binary_Iif, Binary_Img, Binary_Msg, Binary_Pal, Binary_Pcx,
// Binary_Raw, Binary_Snd, Binary_Tga, Binary_Txt, Binary_Xga, Binary_TestTri.
//
// Not ported:
// - Binary_Dds: needs `DDS_HEADER.Read`, still commented out in openx-gfx's gfx_texture.rs.
// - Binary_Plist: needs `formats/apple/plist_reader.rs` (a todo stub) and an `ArcBinary`.
// - Binary_Zip: needs a zip crate (System.IO.Compression + SharpZipLib with its `Key` property).
// - Binary_Engine: needs `ICellDatabase` and `CellManager.IQueryFunc`, not in gamex's Rust port.
//
// Shapes:
// - A C# `Factory` returning `Task.FromResult(new Binary_X(...))` is an associated fn `factory`
//   (an `AssetFactory`) that builds the object synchronously, as C# does, and returns a ready task.
// - `Binary_Msg.Factory(string)` and `Binary_Raw.FactoryMethod(...)` return a C# `Func` that
//   captures its arguments. `AssetFactory` is a plain `fn`, so they return a boxed closure of the
//   same signature; a caller wraps it in its own `fn` to use it as an `AssetFactory`.
// - `MetaContent { Value = this }`: each such type keeps a `me: Weak<Self>`, set when its factory
//   builds the `Arc`, and the content's value is that `Arc` as `Tag::Any` (same object, as in C#).
// - `ITexture.Create<T>(string platform, Func<object, T> func)` isn't on Rust's `ITexture` (see
//   openx-gfx's gfx.rs header), so each texture has an inherent `create` passing the
//   `TextureAsBytes` as `Box<dyn Any>`, returning `io::Result` where C# can throw.
// - Nested C# types are at module level under their own name; where two would collide they are
//   prefixed with the enclosing type (`Binary_PcxX_Header`, `Binary_TgaX_Header`, `Binary_RawTag`).
// - C# exceptions are `io::Error`s; `Log.Info` is `eprintln!`.
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
use std::sync::{Arc, Mutex, Weak};
use bytemuck::{Pod, Zeroable};
use gamex::binary::{Archive, Stream, Task};
use gamex::family::FamilyGame;
use gamex::meta::{fileName, extension, FileSource, IHaveMetaInfo, MetaContent, MetaInfo, MetaManager, Tag};
use openx::platform::{current_caps, Caps};
use openx_gfx::gfx::{AnyArc, ITexture, TextureAsBytes};
use openx_gfx::gfx_texture::{TextureFlags, TextureFormat, TexturePixel};
use openx_poly::prelude::{BinaryReaderExt, IStruct};

/// C# `Task.FromResult((object)value)`.
fn fromResult<'a>(value: io::Result<Tag>) -> Task<'a, Tag> { Box::pin(std::future::ready(value)) }

/// Builds the `Arc` a factory returns, giving the object its `me`.
fn arc<T: IHaveMetaInfo>(mut s: T, me: fn(&mut T) -> &mut Weak<T>) -> Tag { Tag::Info(Arc::new_cyclic(|w| { *me(&mut s) = w.clone(); s })) }

/// C# `Value = this`.
fn this<T: Any + Send + Sync>(me: &Weak<T>) -> Tag { me.upgrade().map_or(Tag::None, |s| Tag::Any(s)) }

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
        if timing == 0 { eprintln!("Unable to convert data\n"); return Ok(false); }
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
                Some(EV::NOTE_OFF) => { eprintln!("ERROR: Note off not valid in XMidiFile\n"); return Ok(0); }
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
                _ => eprintln!("Not supposed to see this"),
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
        let capsDrawing = current_caps().contains(Caps::DRAWING);
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
}

impl IHaveMetaInfo for Binary_Img {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Texture", file, this(&self.me)),
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
}

impl IHaveMetaInfo for Binary_Pcx {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Texture", file, this(&self.me)),
        MetaInfo::new(Some("Binary_Pcx".into()), Tag::None, vec![
            node(format!("Width: {}", self.width)),
            node(format!("Height: {}", self.height)),
        ], false),
    ] }
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
}

impl IHaveMetaInfo for Binary_Raw {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Texture", file, this(&self.me)),
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
}

impl IHaveMetaInfo for Binary_Tga {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Texture", file, this(&self.me)),
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
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { fromResult(Binary_Xga::new(r, &s.base().tag).map(|s| arc(s, |s| &mut s.me))) }

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
}

impl IHaveMetaInfo for Binary_Xga {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Texture", file, this(&self.me)),
        MetaInfo::new(Some("Binary_Xga".into()), Tag::None, vec![
            node(format!("Type: {}", self.type_)),
            node(format!("Width: {}", self.width)),
            node(format!("Height: {}", self.height)),
        ], false),
    ] }
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
}
