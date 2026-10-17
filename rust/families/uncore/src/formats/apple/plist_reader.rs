// PORT-SOURCE: Families/GameX.Uncore/Formats/Apple/PlistReader.cs
// PORT-SHA: 88b27724927ec82e
// PORT-STATUS: done
//
// Apple binary plist (`bplist00`). `ReadObject` returns the last object-table
// entry, which must be a dictionary. Nested arrays and dictionaries are
// flattened there. Python has no twin of this file (ADR 0001).
//
// Kept from C# (see PORTING.md):
// - `ReadObject<T>(string)` calls itself. `readObjectTPath` does the same.
// - The root is `objectTable[^1]`, not `topLevelObjectOffset`. The trailer
//   sanity checks stay commented out.
// - A non-seekable stream is copied, then `Length` and `Position` are still
//   taken from the original stream. `Read + Seek` has no such stream.
// - `new BinaryReader(stream)` disposes that stream. A borrowed reader is not closed.
// - `PlistArray.ToString` prints `ObjectReference[objectRef]` (the int), not the item.
// - `ReadUnicodeString` increments `i` twice. On a little-endian host that yields UTF-16 LE.
// - An 8-byte integer is a signed reinterpret of the `u64`. A UID is `Dictionary<string, ulong>`.
// - Fill nibble 15 is omitted, so later object refs no longer match table slots.
// - A self-cycle is omitted. A cycle between two containers still recurses.
//
// `DateTime.ToString` is the current culture's "G" format. `PlistDate`'s text is
// `yyyy-MM-dd HH:mm:ss` in local time (UTC off Windows). `double.ToString` is Rust's
// display. A short read is `UnexpectedEof`, where C# throws `EndOfStreamException`.
// Byte arrays and dictionaries used as keys compare by value; C# uses reference equality.

use std::io::{self, Error, ErrorKind, Read, Seek, SeekFrom};

use openx_poly::prelude::BinaryReaderExt;

/// Apple's reference date, 2001-01-01 UTC, as a Unix second.
const AppleUnix: i64 = 978_307_200;
const TicksPerSecond: f64 = 10_000_000.0;

/// C# `DateTime` after `ReferenceDate.AddSeconds(seconds).ToLocalTime()`.
#[derive(Clone, Copy)]
pub struct PlistDate {
    /// Seconds since 2001-01-01 UTC, before the local conversion.
    pub seconds: f64,
}

impl PlistDate {
    /// C# `DateTime.ToString()` stand-in: local `yyyy-MM-dd HH:mm:ss`.
    pub fn toString(&self) -> String { formatDate(self.seconds) }
}

impl PartialEq for PlistDate {
    fn eq(&self, other: &Self) -> bool { self.seconds == other.seconds }
}

impl std::fmt::Debug for PlistDate {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result { write!(f, "{}", self.toString()) }
}

/// One plist value after `ToArray` / `ToDictionary`.
///
/// Integers keep C#'s boxed width: `Int16` below 4 bytes, `Int32` below 8, otherwise `Int64`.
/// `UniqueId` is the single `CF$UID` entry of `Dictionary<string, ulong>`.
/// `Container` is a `PlistArray` or `PlistDictionary` left unflattened because it was a key.
#[derive(Clone)]
pub enum PlistObject {
    Null,
    Bool(bool),
    Int16(i16),
    Int32(i32),
    Int64(i64),
    Real(f64),
    Data(Vec<u8>),
    Str(String),
    Date(PlistDate),
    UniqueId(Vec<(String, u64)>),
    Array(Vec<PlistObject>),
    Dict(PlistDict),
    Container { id: u32 },
}

impl PartialEq for PlistObject {
    fn eq(&self, other: &Self) -> bool {
        match (self, other) {
            (PlistObject::Null, PlistObject::Null) => true,
            (PlistObject::Bool(a), PlistObject::Bool(b)) => a == b,
            (PlistObject::Int16(a), PlistObject::Int16(b)) => a == b,
            (PlistObject::Int32(a), PlistObject::Int32(b)) => a == b,
            (PlistObject::Int64(a), PlistObject::Int64(b)) => a == b,
            (PlistObject::Real(a), PlistObject::Real(b)) => realBits(*a) == realBits(*b),
            (PlistObject::Data(a), PlistObject::Data(b)) => a == b,
            (PlistObject::Str(a), PlistObject::Str(b)) => a == b,
            (PlistObject::Date(a), PlistObject::Date(b)) => a == b,
            (PlistObject::UniqueId(a), PlistObject::UniqueId(b)) => a == b,
            (PlistObject::Array(a), PlistObject::Array(b)) => a == b,
            (PlistObject::Dict(a), PlistObject::Dict(b)) => a == b,
            (PlistObject::Container { id: a }, PlistObject::Container { id: b }) => a == b,
            _ => false,
        }
    }
}

impl std::fmt::Debug for PlistObject {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            PlistObject::Null => write!(f, "null"),
            PlistObject::Bool(true) => write!(f, "True"),
            PlistObject::Bool(false) => write!(f, "False"),
            PlistObject::Int16(v) => write!(f, "{v}"),
            PlistObject::Int32(v) => write!(f, "{v}"),
            PlistObject::Int64(v) => write!(f, "{v}"),
            PlistObject::Real(v) => write!(f, "{v}"),
            PlistObject::Data(v) => write!(f, "System.Byte[]({})", v.len()),
            PlistObject::Str(v) => write!(f, "{v:?}"),
            PlistObject::Date(v) => write!(f, "{v:?}"),
            PlistObject::UniqueId(v) => write!(f, "CF$UID={v:?}"),
            PlistObject::Array(v) => write!(f, "{v:?}"),
            PlistObject::Dict(v) => write!(f, "{v:?}"),
            PlistObject::Container { id } => write!(f, "container({id})"),
        }
    }
}

/// C# `Dictionary<object, object>`. A repeated key keeps its first slot and the last value.
#[derive(Clone, Default, PartialEq)]
pub struct PlistDict {
    entries: Vec<(PlistObject, PlistObject)>,
}

impl PlistDict {
    pub fn new() -> Self { Self { entries: Vec::new() } }
    pub fn len(&self) -> usize { self.entries.len() }
    pub fn isEmpty(&self) -> bool { self.entries.is_empty() }
    pub fn entries(&self) -> &[(PlistObject, PlistObject)] { &self.entries }

    pub fn insert(&mut self, key: PlistObject, value: PlistObject) {
        if let Some(slot) = self.entries.iter_mut().find(|(k, _)| k == &key) { slot.1 = value; }
        else { self.entries.push((key, value)); }
    }

    pub fn get(&self, key: &PlistObject) -> Option<&PlistObject> {
        self.entries.iter().find(|(k, _)| k == key).map(|(_, v)| v)
    }
}

impl std::fmt::Debug for PlistDict {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result { write!(f, "{:?}", self.entries) }
}

impl IntoIterator for PlistDict {
    type Item = (PlistObject, PlistObject);
    type IntoIter = std::vec::IntoIter<(PlistObject, PlistObject)>;
    fn into_iter(self) -> Self::IntoIter { self.entries.into_iter() }
}

/// C# `PlistReader.ISerializable`.
pub trait ISerializable {
    fn fromDictionary(&mut self, plist: &PlistDict);
    fn toDictionary(&self) -> PlistDict;
}

#[derive(Clone)]
enum ItemValue {
    Null,
    Bool(bool),
    Int16(i16),
    Int32(i32),
    Int64(i64),
    Real(f64),
    Data(Vec<u8>),
    Str(String),
    Date(PlistDate),
    UniqueId(Vec<(String, u64)>),
    Array(usize),
    Dict(usize),
}

#[derive(Clone, Copy, PartialEq)]
enum ContainerId { Array(usize), Dict(usize) }

/// C# `PlistItem`. `byteValue` and `marker` stay empty: the reader never fills them.
struct PlistItem {
    byteValue: Vec<u8>,
    isArray: bool,
    isDictionary: bool,
    marker: Vec<u8>,
    value: ItemValue,
}

impl PlistItem {
    fn new(value: ItemValue) -> Self { Self { byteValue: Vec::new(), isArray: false, isDictionary: false, marker: Vec::new(), value } }

    fn setByteValue(&mut self, buffer: Option<&[u8]>) {
        self.byteValue.clear();
        if let Some(buffer) = buffer { self.byteValue.extend_from_slice(buffer); }
    }

    fn size(&self) -> i32 { (self.marker.len() + self.byteValue.len()) as i32 }
}

/// C# `PlistArray`. The object table lives on the reader; this holds the refs.
struct PlistArray {
    objectReference: Vec<i32>,
}

impl PlistArray {
    /// C# `PlistArray(objectTable)`, which calls the two-argument constructor with size 0.
    fn new() -> Self { Self::withSize(0).unwrap() }
    /// C# `PlistArray(objectTable, size)`. A negative size is `ArgumentOutOfRangeException`.
    fn withSize(size: i32) -> io::Result<Self> {
        if size < 0 { return Err(arg("ArgumentOutOfRangeException: Non-negative number required.")); }
        Ok(Self { objectReference: Vec::with_capacity(size as usize) })
    }
}

/// C# `PlistDictionary`.
struct PlistDictionary {
    keyReference: Vec<i32>,
    objectReference: Vec<i32>,
}

impl PlistDictionary {
    fn withSize(size: i32) -> io::Result<Self> {
        if size < 0 { return Err(arg("ArgumentOutOfRangeException: Non-negative number required.")); }
        Ok(Self { keyReference: Vec::with_capacity(size as usize), objectReference: Vec::with_capacity(size as usize) })
    }
}

/// Performs de-serialization of binary plists.
#[derive(Default)]
pub struct PlistReader {
    objectTable: Vec<PlistItem>,
    offsetTable: Vec<i32>,
    offsetIntSize: i32,
    objectRefSize: i32,
    objectCount: i32,
    topLevelObjectOffset: i32,
    offsetTableOffset: i32,
    arrays: Vec<PlistArray>,
    dicts: Vec<PlistDictionary>,
}

impl PlistReader {
    /// Gets the magic number value used in a binary plist header.
    const HeaderMagicNumber: i32 = 0x62706c69;
    /// Gets the version number value used in a binary plist header.
    const HeaderVersionNumber: i32 = 0x73743030;

    pub fn new() -> Self { Self::default() }

    /// Reads a binary plist from the given file path into a dictionary.
    pub fn readObjectPath(&mut self, path: &str) -> io::Result<PlistDict> {
        let mut file = std::fs::File::open(path)?;
        self.readObject(&mut file)
    }

    /// Reads a binary plist from the given stream into a dictionary.
    pub fn readObject<R: Read + Seek + ?Sized>(&mut self, s: &mut R) -> io::Result<PlistDict> {
        // C# rejects a null stream and a stream that cannot be read. `Read + Seek` is both.
        // C# copies a non-seekable stream, then still uses the original's Length and Position.
        let length = streamLen(s)?;
        self.reset();
        if length > 40 {
            s.seek(SeekFrom::Start(0))?;
            let bpli = s.readInt32E()?;
            let version = s.readInt32E()?;
            if bpli != Self::HeaderMagicNumber || version != Self::HeaderVersionNumber {
                return Err(arg("ArgumentException: The stream data does not start with required 'bplist00' header."));
            }
            // The first six bytes of the trailer are unused, so offset by 26 instead of 32.
            s.seek(SeekFrom::Start(length - 26))?;
            self.offsetIntSize = i32::from(s.readByte()?);
            self.objectRefSize = i32::from(s.readByte()?);
            self.objectCount = s.readInt64E()? as i32;
            self.topLevelObjectOffset = s.readInt64E()? as i32;
            self.offsetTableOffset = s.readInt64E()? as i32;
            let _offsetTableSize = self.offsetIntSize.wrapping_mul(self.objectCount);
            // Trailer sanity stays commented out, as in C#.
            // if offsetIntSize < 1 || offsetIntSize > 8 || objectRefSize < 1 || objectRefSize > 8
            //     || offsetTableOffset < 8 || offsetTableSize + offsetTableOffset + 32 > length { error }
            self.readOffsetTable(s)?;
            self.readObjectTable(s)?;
        }
        else { return Err(arg("ArgumentException: The stream is too short to be a valid binary plist.")); }

        if self.objectTable.is_empty() {
            return Err(arg("ArgumentOutOfRangeException: Index was out of range."));
        }
        let root = self.objectTable.len() - 1;
        let rootDict = match &self.objectTable[root].value { ItemValue::Dict(index) => Some(*index), _ => None };
        match rootDict {
            Some(index) => self.toDictionary(index),
            // GetType() is the item's type, not the value's.
            None => Err(invalid("InvalidOperationException: Unsupported root plist object: GameX.Uncore.Formats.Apple.PlistReader+PlistItem. A dictionary must be the root plist object.")),
        }
    }

    /// C# `ReadObject<T>(string)`. Opens `path`, then calls itself.
    #[allow(unconditional_recursion)]
    pub fn readObjectTPath<T: ISerializable + Default>(&mut self, path: &str) -> io::Result<T> {
        let _file = std::fs::File::open(path)?;
        self.readObjectTPath::<T>(path)
    }

    /// Reads a binary plist from the given stream into a new `ISerializable`.
    pub fn readObjectT<T: ISerializable + Default, R: Read + Seek + ?Sized>(&mut self, s: &mut R) -> io::Result<T> {
        let mut obj = T::default();
        obj.fromDictionary(&self.readObject(s)?);
        Ok(obj)
    }

    fn readArray<R: Read + Seek + ?Sized>(&mut self, r: &mut R, index: i64, size: i32) -> io::Result<usize> {
        let mut array = PlistArray::withSize(size)?;
        for i in 0..size {
            let at = index + i64::from(i.wrapping_mul(self.objectRefSize));
            array.objectReference.push(readInteger(r, at, self.objectRefSize)? as i32);
        }
        self.arrays.push(array);
        Ok(self.arrays.len() - 1)
    }

    fn readDictionary<R: Read + Seek + ?Sized>(&mut self, r: &mut R, index: i64, size: i32) -> io::Result<usize> {
        let mut dictionary = PlistDictionary::withSize(size)?;
        let skip = i64::from(size.wrapping_mul(self.objectRefSize));
        for i in 0..size {
            let step = i64::from(i.wrapping_mul(self.objectRefSize));
            dictionary.keyReference.push(readInteger(r, index + step, self.objectRefSize)? as i32);
            dictionary.objectReference.push(readInteger(r, skip + index + step, self.objectRefSize)? as i32);
        }
        self.dicts.push(dictionary);
        Ok(self.dicts.len() - 1)
    }

    fn readObjectTable<R: Read + Seek + ?Sized>(&mut self, r: &mut R) -> io::Result<()> {
        for i in 0..self.objectCount {
            let at = i64::from(self.offsetTable[i as usize]);
            if at < 0 { return Err(arg("ArgumentOutOfRangeException: Non-negative number required.")); }
            r.seek(SeekFrom::Start(at as u64))?;
            let marker = r.readByte()?;
            match (marker & 0xf0) >> 4 {
                0 => {
                    let mut primitive = None;
                    if readPrimitive(r, at, &mut primitive)? {
                        let value = match primitive { None => ItemValue::Null, Some(v) => ItemValue::Bool(v) };
                        self.objectTable.push(PlistItem::new(value));
                    }
                }
                1 => {
                    let size = 1i32 << u32::from(marker & 0x0f);
                    let pos = r.stream_position()? as i64;
                    let parsed = readInteger(r, pos, size)?;
                    let value = if size < 4 { ItemValue::Int16(parsed as i16) }
                        else if size < 8 { ItemValue::Int32(parsed as i32) }
                        else { ItemValue::Int64(parsed) };
                    self.objectTable.push(PlistItem::new(value));
                }
                2 => {
                    let size = 1i32 << u32::from(marker & 0x0f);
                    let pos = r.stream_position()? as i64;
                    self.objectTable.push(PlistItem::new(ItemValue::Real(readReal(r, pos, size)?)));
                }
                3 => {
                    let size = i32::from(marker & 0x0f);
                    if size == 3 {
                        let pos = r.stream_position()? as i64;
                        self.objectTable.push(PlistItem::new(ItemValue::Date(readDate(r, pos, 8)?)));
                    }
                    else { return Err(invalid(format!("InvalidOperationException: Unsupported date size: {size:b}"))); }
                }
                4 => {
                    let size = countOf(r, marker)?;
                    let pos = r.stream_position()? as i64;
                    let bytes = readData(r, pos, size)?;
                    self.objectTable.push(PlistItem::new(ItemValue::Data(bytes)));
                }
                5 => {
                    let size = countOf(r, marker)?;
                    let pos = r.stream_position()? as i64;
                    let text = readAsciiString(r, pos, size)?;
                    self.objectTable.push(PlistItem::new(ItemValue::Str(text)));
                }
                6 => {
                    let size = countOf(r, marker)?;
                    let pos = r.stream_position()? as i64;
                    let text = readUnicodeString(r, pos, size)?;
                    self.objectTable.push(PlistItem::new(ItemValue::Str(text)));
                }
                8 => {
                    let size = i32::from(marker & 0x0f) + 1;
                    let pos = r.stream_position()? as i64;
                    self.objectTable.push(PlistItem::new(ItemValue::UniqueId(readUniqueId(r, pos, size)?)));
                }
                10 | 12 => {
                    let size = countOf(r, marker)?;
                    let pos = r.stream_position()? as i64;
                    let index = self.readArray(r, pos, size)?;
                    let mut item = PlistItem::new(ItemValue::Array(index));
                    item.isArray = true;
                    self.objectTable.push(item);
                }
                13 => {
                    let size = countOf(r, marker)?;
                    let pos = r.stream_position()? as i64;
                    let index = self.readDictionary(r, pos, size)?;
                    let mut item = PlistItem::new(ItemValue::Dict(index));
                    item.isDictionary = true;
                    self.objectTable.push(item);
                }
                _ => return Err(invalid(format!("InvalidOperationException: An invalid marker was found while reading the object table: {marker:b}"))),
            }
        }
        Ok(())
    }

    fn readOffsetTable<R: Read + Seek + ?Sized>(&mut self, r: &mut R) -> io::Result<()> {
        for i in 0..self.objectCount {
            let at = i64::from(self.offsetTableOffset) + i64::from(i.wrapping_mul(self.offsetIntSize));
            self.offsetTable.push(readInteger(r, at, self.offsetIntSize)? as i32);
        }
        Ok(())
    }

    fn reset(&mut self) {
        self.objectRefSize = 0;
        self.objectCount = 0;
        self.offsetIntSize = 0;
        self.offsetTableOffset = 0;
        self.topLevelObjectOffset = 0;
        self.objectTable.clear();
        self.offsetTable.clear();
        self.arrays.clear();
        self.dicts.clear();
    }

    fn isThis(&self, index: usize, this: ContainerId) -> bool {
        match (&self.objectTable[index].value, this) {
            (ItemValue::Array(a), ContainerId::Array(i)) => *a == i,
            (ItemValue::Dict(d), ContainerId::Dict(i)) => *d == i,
            _ => false,
        }
    }

    fn valueObject(&self, index: usize) -> io::Result<PlistObject> {
        enum Kind { Array(usize), Dict(usize), Other(PlistObject) }
        let kind = match &self.objectTable[index].value {
            ItemValue::Array(a) => Kind::Array(*a),
            ItemValue::Dict(d) => Kind::Dict(*d),
            other => Kind::Other(other.toPublic()),
        };
        match kind {
            Kind::Array(a) => Ok(PlistObject::Array(self.toArray(a)?)),
            Kind::Dict(d) => Ok(PlistObject::Dict(self.toDictionary(d)?)),
            Kind::Other(v) => Ok(v),
        }
    }

    /// A container key stays the container. C# does not call `ToDictionary` on keys.
    fn keyObject(&self, index: usize) -> PlistObject {
        match &self.objectTable[index].value {
            ItemValue::Array(a) => PlistObject::Container { id: *a as u32 },
            ItemValue::Dict(d) => PlistObject::Container { id: (*d as u32) | 0x8000_0000 },
            other => other.toPublic(),
        }
    }

    fn toArray(&self, arrayIndex: usize) -> io::Result<Vec<PlistObject>> {
        let refs = self.arrays[arrayIndex].objectReference.clone();
        let mut array = vec![PlistObject::Null; refs.len()];
        for (i, objectRef) in refs.into_iter().enumerate() {
            if objectRef >= 0 && (objectRef as usize) < self.objectTable.len() && !self.isThis(objectRef as usize, ContainerId::Array(arrayIndex)) {
                array[i] = self.valueObject(objectRef as usize)?;
            }
        }
        Ok(array)
    }

    fn toDictionary(&self, dictIndex: usize) -> io::Result<PlistDict> {
        let keys = self.dicts[dictIndex].keyReference.clone();
        let objs = self.dicts[dictIndex].objectReference.clone();
        let mut dictionary = PlistDict::new();
        for i in 0..keys.len() {
            let keyRef = keys[i];
            let objectRef = objs[i];
            if self.refLive(keyRef, ContainerId::Dict(dictIndex)) && self.refLive(objectRef, ContainerId::Dict(dictIndex)) {
                let keyValue = self.keyObject(keyRef as usize);
                if matches!(keyValue, PlistObject::Null) {
                    return Err(arg("ArgumentNullException: Value cannot be null."));
                }
                dictionary.insert(keyValue, self.valueObject(objectRef as usize)?);
            }
        }
        Ok(dictionary)
    }

    fn refLive(&self, objectRef: i32, this: ContainerId) -> bool {
        objectRef >= 0 && (objectRef as usize) < self.objectTable.len() && !self.isThis(objectRef as usize, this)
    }

    fn itemToString(&self, index: usize) -> String {
        match &self.objectTable[index].value {
            ItemValue::Null => "null".into(),
            ItemValue::Bool(true) => "True".into(),
            ItemValue::Bool(false) => "False".into(),
            ItemValue::Int16(v) => v.to_string(),
            ItemValue::Int32(v) => v.to_string(),
            ItemValue::Int64(v) => v.to_string(),
            ItemValue::Real(v) => v.to_string(),
            ItemValue::Data(_) => "System.Byte[]".into(),
            ItemValue::Str(v) => v.clone(),
            ItemValue::Date(v) => v.toString(),
            ItemValue::UniqueId(_) => "System.Collections.Generic.Dictionary`2[System.String,System.UInt64]".into(),
            ItemValue::Array(a) => self.arrayToString(*a),
            ItemValue::Dict(d) => self.dictToString(*d),
        }
    }

    /// C# prints `ObjectReference[objectRef]`, the stored int, not the referenced object.
    fn arrayToString(&self, arrayIndex: usize) -> String {
        let refs = self.arrays[arrayIndex].objectReference.clone();
        let mut b = String::from("[");
        for (i, objectRef) in refs.iter().copied().enumerate() {
            if i > 0 { b.push(','); }
            let inRange = objectRef >= 0 && (objectRef as usize) < self.objectTable.len();
            if objectRef < 0 {
                panic!("ArgumentOutOfRangeException: Index was out of range.");
            }
            else if inRange && !self.isThis(objectRef as usize, ContainerId::Array(arrayIndex)) {
                let n = self.arrays[arrayIndex].objectReference[objectRef as usize];
                b.push_str(&n.to_string());
            }
            else { b.push_str(&format!("*{{{objectRef}}}")); }
        }
        b.push(']');
        b
    }

    fn dictToString(&self, dictIndex: usize) -> String {
        let keys = self.dicts[dictIndex].keyReference.clone();
        let objs = self.dicts[dictIndex].objectReference.clone();
        let mut b = String::from("{");
        for i in 0..keys.len() {
            if i > 0 { b.push(','); }
            self.appendRef(&mut b, keys[i], ContainerId::Dict(dictIndex));
            b.push(':');
            self.appendRef(&mut b, objs[i], ContainerId::Dict(dictIndex));
        }
        b.push('}');
        b
    }

    fn appendRef(&self, b: &mut String, objectRef: i32, this: ContainerId) {
        if objectRef < 0 || (objectRef as usize) >= self.objectTable.len() { b.push_str(&format!("#{{{objectRef}}}")); }
        else if self.isThis(objectRef as usize, this) { b.push_str(&format!("*{{{objectRef}}}")); }
        else { b.push_str(&self.itemToString(objectRef as usize)); }
    }
}

impl ItemValue {
    fn toPublic(&self) -> PlistObject {
        match self {
            ItemValue::Null => PlistObject::Null,
            ItemValue::Bool(v) => PlistObject::Bool(*v),
            ItemValue::Int16(v) => PlistObject::Int16(*v),
            ItemValue::Int32(v) => PlistObject::Int32(*v),
            ItemValue::Int64(v) => PlistObject::Int64(*v),
            ItemValue::Real(v) => PlistObject::Real(*v),
            ItemValue::Data(v) => PlistObject::Data(v.clone()),
            ItemValue::Str(v) => PlistObject::Str(v.clone()),
            ItemValue::Date(v) => PlistObject::Date(*v),
            ItemValue::UniqueId(v) => PlistObject::UniqueId(v.clone()),
            ItemValue::Array(_) | ItemValue::Dict(_) => PlistObject::Null,
        }
    }
}

fn arg(msg: impl Into<String>) -> Error { Error::new(ErrorKind::InvalidInput, msg.into()) }
fn invalid(msg: impl Into<String>) -> Error { Error::new(ErrorKind::InvalidData, msg.into()) }

fn streamLen<R: Seek + ?Sized>(s: &mut R) -> io::Result<u64> {
    let pos = s.stream_position()?;
    let len = s.seek(SeekFrom::End(0))?;
    s.seek(SeekFrom::Start(pos))?;
    Ok(len)
}

fn realBits(v: f64) -> u64 { if v.is_nan() { f64::NAN.to_bits() } else if v == 0.0 { 0 } else { v.to_bits() } }

fn readAsciiString<R: Read + Seek + ?Sized>(r: &mut R, index: i64, size: i32) -> io::Result<String> {
    let buffer = readData(r, index, size)?;
    Ok(if buffer.is_empty() { String::new() } else { buffer.iter().map(|&c| if c < 0x80 { c as char } else { '?' }).collect() })
}

fn readData<R: Read + Seek + ?Sized>(r: &mut R, index: i64, size: i32) -> io::Result<Vec<u8>> {
    if index < 0 || size < 0 { return Err(arg("ArgumentOutOfRangeException: Non-negative number required.")); }
    r.seek(SeekFrom::Start(index as u64))?;
    let mut buffer = vec![0u8; size as usize];
    let mut bufferIndex = 0usize;
    while bufferIndex < buffer.len() {
        match r.read(&mut buffer[bufferIndex..]) {
            Ok(0) => break,
            Ok(n) => bufferIndex += n,
            Err(e) if e.kind() == ErrorKind::Interrupted => continue,
            Err(e) => return Err(e),
        }
    }
    Ok(buffer)
}

fn readDate<R: Read + Seek + ?Sized>(r: &mut R, index: i64, size: i32) -> io::Result<PlistDate> {
    let seconds = readReal(r, index, size)?;
    if !seconds.is_finite() { return Err(arg("ArgumentOutOfRangeException: value")); }
    Ok(PlistDate { seconds })
}

fn readInteger<R: Read + Seek + ?Sized>(r: &mut R, index: i64, size: i32) -> io::Result<i64> {
    let mut buffer = readData(r, index, size)?;
    if buffer.len() > 1 && cfg!(target_endian = "little") { buffer.reverse(); }
    match size {
        1 => Ok(i64::from(buffer[0])),
        2 => Ok(i64::from(u16::from_le_bytes(buffer[0..2].try_into().unwrap()))),
        4 => Ok(i64::from(u32::from_le_bytes(buffer[0..4].try_into().unwrap()))),
        8 => Ok(u64::from_le_bytes(buffer[0..8].try_into().unwrap()) as i64),
        _ => Err(invalid(format!("InvalidOperationException: Unsupported variable-length integer size: {size}"))),
    }
}

fn readPrimitive<R: Read + Seek + ?Sized>(r: &mut R, index: i64, primitive: &mut Option<bool>) -> io::Result<bool> {
    if index < 0 { return Err(arg("ArgumentOutOfRangeException: Non-negative number required.")); }
    r.seek(SeekFrom::Start(index as u64))?;
    let value = r.readByte()?;
    match value & 0x0f {
        0 => { *primitive = None; Ok(true) }
        8 => { *primitive = Some(false); Ok(true) }
        9 => { *primitive = Some(true); Ok(true) }
        15 => { *primitive = None; Ok(false) }
        _ => Err(invalid(format!("InvalidOperationException: Illegal primitive: {value:b}"))),
    }
}

fn readReal<R: Read + Seek + ?Sized>(r: &mut R, index: i64, size: i32) -> io::Result<f64> {
    let mut buffer = readData(r, index, size)?;
    if cfg!(target_endian = "little") { buffer.reverse(); }
    match size {
        4 => Ok(f64::from(f32::from_le_bytes(buffer[0..4].try_into().unwrap()))),
        8 => Ok(f64::from_le_bytes(buffer[0..8].try_into().unwrap())),
        _ => Err(invalid(format!("InvalidOperationException: Unsupported floating point number size: {size}"))),
    }
}

fn readUnicodeString<R: Read + Seek + ?Sized>(r: &mut R, index: i64, size: i32) -> io::Result<String> {
    if index < 0 { return Err(arg("ArgumentOutOfRangeException: Non-negative number required.")); }
    r.seek(SeekFrom::Start(index as u64))?;
    let size = size.wrapping_mul(2);
    if size < 0 { return Err(arg("ArgumentOutOfRangeException: Non-negative number required.")); }
    let mut buffer = vec![0u8; size as usize];
    let mut i = 0i32;
    while i < size {
        let one = r.readByte()?;
        let two = r.readByte()?;
        if cfg!(target_endian = "little") { buffer[i as usize] = two; i += 1; buffer[i as usize] = one; }
        else { buffer[i as usize] = one; i += 1; buffer[i as usize] = two; }
        i += 1;
    }
    let units: Vec<u16> = buffer.chunks_exact(2).map(|c| u16::from_le_bytes([c[0], c[1]])).collect();
    Ok(String::from_utf16_lossy(&units))
}

fn readUniqueId<R: Read + Seek + ?Sized>(r: &mut R, index: i64, size: i32) -> io::Result<Vec<(String, u64)>> {
    Ok(vec![("CF$UID".into(), readInteger(r, index, size)? as u64)])
}

fn countOf<R: Read + Seek + ?Sized>(r: &mut R, marker: u8) -> io::Result<i32> {
    let mut size = i32::from(marker & 0x0f);
    if size == 15 {
        let intSize = 1i32 << u32::from(r.readByte()? & 0x0f);
        let pos = r.stream_position()? as i64;
        size = readInteger(r, pos, intSize)? as i32;
    }
    Ok(size)
}

fn formatDate(seconds: f64) -> String {
    if !seconds.is_finite() { return String::new(); }
    let adj = (seconds * TicksPerSecond + if seconds >= 0.0 { 0.5 } else { -0.5 }) as i64;
    let unix = AppleUnix + adj.div_euclid(10_000_000);
    let (y, m, d, hh, mm, ss) = civil(localUnix(unix));
    format!("{y:04}-{m:02}-{d:02} {hh:02}:{mm:02}:{ss:02}")
}

fn localUnix(unix: i64) -> i64 { unix + localOffsetSecs(unix) }

fn localOffsetSecs(unix: i64) -> i64 {
    #[cfg(windows)]
    { windowsLocalOffset(unix) }
    #[cfg(not(windows))]
    { let _ = unix; 0 }
}

#[cfg(windows)]
fn windowsLocalOffset(unix: i64) -> i64 {
    #[repr(C)]
    struct SystemTime { year: u16, month: u16, dayOfWeek: u16, day: u16, hour: u16, minute: u16, second: u16, milliseconds: u16 }
    #[link(name = "kernel32")]
    extern "system" {
        fn FileTimeToSystemTime(fileTime: *const i64, systemTime: *mut SystemTime) -> i32;
        fn SystemTimeToFileTime(systemTime: *const SystemTime, fileTime: *mut i64) -> i32;
        fn SystemTimeToTzSpecificLocalTime(timeZone: *const u8, utc: *const SystemTime, local: *mut SystemTime) -> i32;
    }
    const FiletimeUnix: i64 = 116_444_736_000_000_000;
    let ft = FiletimeUnix + unix.saturating_mul(10_000_000);
    unsafe {
        let mut utc: SystemTime = std::mem::zeroed();
        if FileTimeToSystemTime(&ft, &mut utc) == 0 { return 0; }
        let mut local: SystemTime = std::mem::zeroed();
        if SystemTimeToTzSpecificLocalTime(std::ptr::null(), &utc, &mut local) == 0 { return 0; }
        let mut localFt = 0i64;
        if SystemTimeToFileTime(&local, &mut localFt) == 0 { return 0; }
        (localFt - ft) / 10_000_000
    }
}

/// Days and clock of a Unix second, as UTC.
fn civil(unix: i64) -> (i32, i32, i32, i32, i32, i32) {
    let secs = unix.rem_euclid(86_400);
    let days = unix.div_euclid(86_400);
    let z = days + 719_468;
    let era = (if z >= 0 { z } else { z - 146_096 }) / 146_097;
    let doe = z - era * 146_097;
    let yoe = (doe - doe / 1_460 + doe / 36_524 - doe / 146_096) / 365;
    let y = yoe + era * 400;
    let doy = doe - (365 * yoe + yoe / 4 - yoe / 100);
    let mp = (5 * doy + 2) / 153;
    let d = doy - (153 * mp + 2) / 5 + 1;
    let m = if mp < 10 { mp + 3 } else { mp - 9 };
    let y = y + if m <= 2 { 1 } else { 0 };
    (y as i32, m as i32, d as i32, (secs / 3_600) as i32, ((secs % 3_600) / 60) as i32, (secs % 60) as i32)
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::io::Cursor;

    struct Bplist { objects: Vec<Vec<u8>>, top: u64 }

    impl Bplist {
        fn new() -> Self { Self { objects: Vec::new(), top: 0 } }
        fn push(&mut self, object: Vec<u8>) -> u8 { self.objects.push(object); (self.objects.len() - 1) as u8 }
        fn bytes(&self) -> Vec<u8> {
            let mut out = b"bplist00".to_vec();
            let mut offsets = Vec::new();
            for object in &self.objects {
                offsets.push(out.len());
                out.extend(object);
            }
            let table = out.len() as u64;
            for offset in offsets { out.push(offset as u8); }
            out.extend_from_slice(&[0u8; 6]);
            out.push(1);
            out.push(1);
            out.extend_from_slice(&(self.objects.len() as u64).to_be_bytes());
            out.extend_from_slice(&self.top.to_be_bytes());
            out.extend_from_slice(&table.to_be_bytes());
            out
        }
    }

    fn ascii(text: &str) -> Vec<u8> { let mut v = vec![0x50 | text.len() as u8]; v.extend(text.as_bytes()); v }
    fn data(bytes: &[u8]) -> Vec<u8> { let mut v = vec![0x40 | bytes.len() as u8]; v.extend(bytes); v }
    fn dictObj(keys: &[u8], vals: &[u8]) -> Vec<u8> { let mut v = vec![0xD0 | keys.len() as u8]; v.extend(keys); v.extend(vals); v }
    fn array(refs: &[u8]) -> Vec<u8> { let mut v = vec![0xA0 | refs.len() as u8]; v.extend(refs); v }

    fn read(bytes: Vec<u8>) -> io::Result<(PlistReader, PlistDict)> {
        let mut reader = PlistReader::new();
        let dict = reader.readObject(&mut Cursor::new(bytes))?;
        Ok((reader, dict))
    }

    fn valueOf(dict: &PlistDict) -> &PlistObject { &dict.entries()[0].1 }

    fn wrap(value: Vec<u8>) -> Vec<u8> {
        let mut p = Bplist::new();
        let key = p.push(ascii("k"));
        let val = p.push(value);
        p.push(dictObj(&[key], &[val]));
        p.top = 0;
        p.bytes()
    }

    #[test]
    fn dictOfStringAndDataIgnoresTheTopOffset() {
        let mut p = Bplist::new();
        let key = p.push(ascii("a"));
        let val = p.push(data(&[1, 2, 3]));
        p.push(dictObj(&[key], &[val]));
        p.top = 0;
        let (_reader, dict) = read(p.bytes()).unwrap();
        assert_eq!(dict.len(), 1);
        assert_eq!(dict.get(&PlistObject::Str("a".into())), Some(&PlistObject::Data(vec![1, 2, 3])));
    }

    #[test]
    fn badMagicAndAShortStreamFail() {
        let mut long = b"bplist01".to_vec();
        long.extend(std::iter::repeat(0).take(40));
        let magic = read(long).err().unwrap();
        assert!(magic.to_string().contains("bplist00"), "{magic}");
        let short = read(vec![0; 40]).err().unwrap();
        assert!(short.to_string().contains("too short"), "{short}");
    }

    #[test]
    fn aStringRootIsRejected() {
        let mut p = Bplist::new();
        p.push(ascii("hi"));
        let err = read(p.bytes()).err().unwrap();
        assert!(err.to_string().contains("PlistReader+PlistItem"), "{err}");
    }

    #[test]
    fn aFillByteIsOmittedSoTheDictBeforeItStaysTheRoot() {
        let mut p = Bplist::new();
        p.push(dictObj(&[], &[]));
        p.push(vec![0x0f]);
        let (_reader, dict) = read(p.bytes()).unwrap();
        assert!(dict.isEmpty());
    }

    #[test]
    fn onlyFillBytesLeaveTheObjectTableEmpty() {
        let mut p = Bplist::new();
        p.push(vec![0x0f]);
        p.push(vec![0x0f]);
        let err = read(p.bytes()).err().unwrap();
        assert!(err.to_string().contains("Index was out of range"), "{err}");
    }

    #[test]
    fn integersKeepTheirBoxedWidth() {
        assert_eq!(valueOf(&read(wrap(vec![0x10, 0xff])).unwrap().1), &PlistObject::Int16(255));
        assert_eq!(valueOf(&read(wrap({ let mut v = vec![0x11]; v.extend(0xffffu16.to_be_bytes()); v })).unwrap().1), &PlistObject::Int16(-1));
        assert_eq!(valueOf(&read(wrap({ let mut v = vec![0x12]; v.extend(0xffffffffu32.to_be_bytes()); v })).unwrap().1), &PlistObject::Int32(-1));
        assert_eq!(valueOf(&read(wrap({ let mut v = vec![0x13]; v.extend(u64::MAX.to_be_bytes()); v })).unwrap().1), &PlistObject::Int64(-1));
    }

    #[test]
    fn scalarsArraysAndNestedDicts() {
        assert_eq!(valueOf(&read(wrap(vec![0x09])).unwrap().1), &PlistObject::Bool(true));
        assert_eq!(valueOf(&read(wrap(vec![0x08])).unwrap().1), &PlistObject::Bool(false));
        assert_eq!(valueOf(&read(wrap({ let mut v = vec![0x22]; v.extend(1.5f32.to_be_bytes()); v })).unwrap().1), &PlistObject::Real(1.5));
        assert_eq!(valueOf(&read(wrap(vec![0x33, 0, 0, 0, 0, 0, 0, 0, 0])).unwrap().1), &PlistObject::Date(PlistDate { seconds: 0.0 }));
        assert_eq!(valueOf(&read(wrap(vec![0x80, 7])).unwrap().1), &PlistObject::UniqueId(vec![("CF$UID".into(), 7)]));
        let high = wrap({ let mut v = vec![0x52]; v.extend([0x41, 0xff]); v });
        assert_eq!(valueOf(&read(high).unwrap().1), &PlistObject::Str("A?".into()));
        let mut wide = vec![0x5f, 0x10, 15];
        wide.extend(std::iter::repeat(b'b').take(15));
        assert_eq!(valueOf(&read(wrap(wide)).unwrap().1), &PlistObject::Str("b".repeat(15)));
        assert_eq!(valueOf(&read(wrap({ let mut v = vec![0x61]; v.extend(u16::from(b'A').to_be_bytes()); v })).unwrap().1), &PlistObject::Str("A".into()));

        let mut p = Bplist::new();
        let key = p.push(ascii("k"));
        let null = p.push(vec![0x00]);
        let yes = p.push(vec![0x09]);
        let arr = p.push(array(&[null, yes]));
        p.push(dictObj(&[key], &[arr]));
        let (_reader, dict) = read(p.bytes()).unwrap();
        assert_eq!(valueOf(&dict), &PlistObject::Array(vec![PlistObject::Null, PlistObject::Bool(true)]));

        let mut p = Bplist::new();
        let innerKey = p.push(ascii("n"));
        let innerVal = p.push(data(&[9]));
        let inner = p.push(dictObj(&[innerKey], &[innerVal]));
        let key = p.push(ascii("k"));
        p.push(dictObj(&[key], &[inner]));
        let (_reader, dict) = read(p.bytes()).unwrap();
        let mut innerDict = PlistDict::new();
        innerDict.insert(PlistObject::Str("n".into()), PlistObject::Data(vec![9]));
        assert_eq!(valueOf(&dict), &PlistObject::Dict(innerDict));
    }

    #[test]
    fn aSelfCycleIsOmittedAndADuplicateKeyKeepsTheLastValue() {
        let mut p = Bplist::new();
        let key = p.push(ascii("k"));
        p.push(dictObj(&[key], &[1]));
        let (_reader, dict) = read(p.bytes()).unwrap();
        assert!(dict.isEmpty());

        let mut p = Bplist::new();
        let key = p.push(ascii("k"));
        let first = p.push(data(&[1]));
        let second = p.push(data(&[2]));
        p.push(dictObj(&[key, key], &[first, second]));
        let (_reader, dict) = read(p.bytes()).unwrap();
        assert_eq!(dict.len(), 1);
        assert_eq!(dict.get(&PlistObject::Str("k".into())), Some(&PlistObject::Data(vec![2])));
    }

    #[test]
    fn arrayToStringPrintsTheReferenceInteger() {
        let mut p = Bplist::new();
        p.push(ascii("hi"));
        p.push(array(&[0]));
        let key = p.push(ascii("k"));
        p.push(dictObj(&[key], &[1]));
        let (reader, dict) = read(p.bytes()).unwrap();
        assert_eq!(valueOf(&dict), &PlistObject::Array(vec![PlistObject::Str("hi".into())]));
        assert_eq!(reader.arrayToString(0), "[0]");
        assert_eq!(reader.dictToString(0), "{k:[0]}");
    }

    #[test]
    fn anOutOfRangeDictRefIsSkippedAndPrintsAHash() {
        let mut p = Bplist::new();
        p.push(dictObj(&[5], &[5]));
        let (reader, dict) = read(p.bytes()).unwrap();
        assert!(dict.isEmpty());
        assert_eq!(reader.dictToString(0), "{#{5}:#{5}}");
    }

    #[test]
    fn illegalPrimitiveInvalidMarkerAndDateSize() {
        let mut p = Bplist::new();
        p.push(vec![0x01]);
        let err = read(p.bytes()).err().unwrap();
        assert!(err.to_string().contains("Illegal primitive: 1"), "{err}");
        let mut p = Bplist::new();
        p.push(vec![0x70]);
        let err = read(p.bytes()).err().unwrap();
        assert!(err.to_string().contains("invalid marker"), "{err}");
        assert!(err.to_string().contains("1110000"), "{err}");
        let mut p = Bplist::new();
        p.push(vec![0x31]);
        let err = read(p.bytes()).err().unwrap();
        assert!(err.to_string().contains("Unsupported date size: 1"), "{err}");
    }

    #[test]
    fn pathAndSerializableOverloads() {
        let mut p = Bplist::new();
        let key = p.push(ascii("a"));
        let val = p.push(data(&[4]));
        p.push(dictObj(&[key], &[val]));
        let path = std::env::temp_dir().join("gamex-plist-reader-test.bplist");
        let _guard = Tmp(path.clone());
        std::fs::write(&path, p.bytes()).unwrap();
        let dict = PlistReader::new().readObjectPath(path.to_str().unwrap()).unwrap();
        assert_eq!(dict.get(&PlistObject::Str("a".into())), Some(&PlistObject::Data(vec![4])));

        #[derive(Default)]
        struct Rec { n: i32 }
        impl ISerializable for Rec {
            fn fromDictionary(&mut self, plist: &PlistDict) { self.n = plist.len() as i32; }
            fn toDictionary(&self) -> PlistDict { PlistDict::new() }
        }
        let rec = PlistReader::new().readObjectT::<Rec, _>(&mut Cursor::new(p.bytes())).unwrap();
        assert_eq!(rec.n, 1);
    }

    #[test]
    fn itemByteValueAndTheOneArgArrayConstructor() {
        let mut item = PlistItem::new(ItemValue::Null);
        assert_eq!(item.size(), 0);
        item.setByteValue(Some(&[1, 2]));
        item.marker.push(9);
        assert_eq!(item.size(), 3);
        item.setByteValue(None);
        assert_eq!(item.byteValue.len(), 0);
        assert_eq!(item.toStringStandIn(), "null");
        let empty = PlistArray::new();
        assert!(empty.objectReference.is_empty());
        assert!(PlistArray::withSize(-1).is_err());
    }

    #[test]
    fn civilDatesOfTheUnixAndAppleEpochs() {
        assert_eq!(civil(0), (1970, 1, 1, 0, 0, 0));
        assert_eq!(civil(AppleUnix), (2001, 1, 1, 0, 0, 0));
    }

    struct Tmp(std::path::PathBuf);
    impl Drop for Tmp { fn drop(&mut self) { let _ = std::fs::remove_file(&self.0); } }
}

impl PlistItem {
    fn toStringStandIn(&self) -> &'static str { if matches!(self.value, ItemValue::Null) { "null" } else { "value" } }
}
