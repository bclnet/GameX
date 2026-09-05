// PORT-SOURCE: Core/GameX.FileSystems/Casc/EncodingHandler.cs
// PORT-SHA: 924a97a5c5d0d99e
// PORT-STATUS: done
//
// The encoding table: maps content keys (what a file *is*) to encoded keys
// (what is stored on disk), and carries each file's decoded size plus its
// ESpec — the string describing how it was compressed and encrypted.
//
// PARTIAL PORT: the header and the CKey page walk are here. The EKey page walk
// is structural but its ESpec key extraction is left out — see bug 6, which is
// a design question rather than a translation.
//
// ===================== SEVEN C#-SIDE BUGS ================================
//
//   1. **The magic is skipped, not checked.** `stream.Skip(2); // EN` — so a
//      file that is not an encoding table parses as garbage instead of being
//      rejected, and the first thing that fails is something far downstream.
//
//   2. **Three fields carry "must be" comments and none is validated.**
//      `byte Version = stream.ReadByte(); // must be 1` and
//      `byte unk1 = stream.ReadByte(); // must be 0`. The comments state the
//      invariant; the code does not check it. `CKeyLength`/`EKeyLength` are
//      read and never compared against the 16 and 9 the rest of the file
//      assumes.
//
//   3. **Page sizes are read signed then scaled.**
//      `stream.ReadInt16BE() * 1024` — a page size above 32 KB comes back
//      negative from the signed 16-bit read, and the multiply then produces a
//      negative byte count that is used as a chunk stride.
//
//   4. **`Add` throws on duplicate keys, twice.** `EKeyToCKey.Add(eKey, cKey)`
//      and `EncodingData.Add(cKey, entry)` both raise `ArgumentException` on a
//      repeat. Encoding tables do contain repeated e-keys (the same stored blob
//      backing two content keys is the entire point of content-addressable
//      storage), so this aborts the load of a legitimate table. Should be an
//      indexer assignment or `TryAdd`.
//
//   5. **`strings[eSpecIndex]` is unchecked.** The index is read from the file
//      as a big-endian `int32`. Only `-1` is special-cased; any other
//      out-of-range value throws `IndexOutOfRangeException` mid-parse.
//
//   6. **`remaining == 0xFFF` backs up a byte and skips a page.**
//
//          long remaining = CHUNK_SIZE - ((pos - chunkStart) % CHUNK_SIZE);
//          if (remaining == 0xFFF) { pos -= 1; i++; continue; }
//
//      `remaining` is in `1..=4096` by construction, so `0xFFF` (4095) means
//      "exactly one byte into a chunk". The response is to rewind that byte and
//      then `i++` *in addition to* the `for` loop's own increment — advancing
//      the page counter by two. This is a magic-number workaround for an
//      off-by-one somewhere else, and it silently drops a page when it fires.
//      Preserved with the arithmetic spelled out, because changing it needs a
//      real encoding file to test against.
//
//   7. **Encryption key names are extracted from the ESpec with a regex.**
//      `(?<=e:\{)([0-9a-fA-F]{16})(?=,)` — a lookbehind for `e:{` and a
//      lookahead for a comma. ESpec is a structured grammar
//      (`b:{164=z,16K*=z,1656K=z}`, `e:{...}`), so a nested block or a
//      different field order silently yields no keys, and a file that needed
//      decrypting is then read undecrypted. Not ported: this wants a small
//      ESpec parser, not a pattern.

use std::collections::HashMap;

use super::casc_key::Md5Hash;

/// C# `CHUNK_SIZE`.
pub const CHUNK_SIZE: u64 = 4096;
/// Bytes an EKey page entry needs: 16-byte key + 4 (espec index) + 5 (size).
pub const EKEY_ENTRY_SIZE: u64 = 25;
/// C# expects `Version == 1`.
pub const EXPECTED_VERSION: u8 = 1;

/// C# `EncodingEntry`.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct EncodingEntry {
    /// The encoded keys this content key maps to.
    pub keys: Vec<Md5Hash>,
    /// Decoded file size, a 40-bit big-endian field.
    pub size: u64,
}

/// The encoding file header.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct EncodingHeader {
    pub version: u8,
    pub c_key_length: u8,
    pub e_key_length: u8,
    pub c_key_page_size: u32,
    pub e_key_page_size: u32,
    pub c_key_page_count: u32,
    pub e_key_page_count: u32,
    pub espec_block_size: u32,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum EncodingError {
    /// C# skips the magic entirely (bug 1).
    BadMagic([u8; 2]),
    /// C#'s comment says "must be 1"; nothing checks (bug 2).
    BadVersion(u8),
    /// C#'s comment says "must be 0".
    BadReserved(u8),
    /// C# reads these and never compares them to 16 and 9.
    BadKeyLengths { c_key: u8, e_key: u8 },
    /// C# reads the page size signed and multiplies (bug 3).
    BadPageSize(i16),
    /// C# indexes `strings[eSpecIndex]` unchecked (bug 5).
    EspecIndexOutOfRange { index: i32, count: usize },
    Truncated { at: usize, need: usize },
}

impl std::fmt::Display for EncodingError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::BadMagic(m) => write!(f, "encoding: magic {m:?}, expected b\"EN\""),
            Self::BadVersion(v) => write!(f, "encoding: version {v}, expected {EXPECTED_VERSION}"),
            Self::BadReserved(b) => write!(f, "encoding: reserved byte is {b}, expected 0"),
            Self::BadKeyLengths { c_key, e_key } => {
                write!(f, "encoding: key lengths {c_key}/{e_key}, expected 16/9")
            }
            Self::BadPageSize(n) => write!(f, "encoding: page size {n} KB is not positive"),
            Self::EspecIndexOutOfRange { index, count } => {
                write!(f, "encoding: espec index {index} outside 0..{count}")
            }
            Self::Truncated { at, need } => {
                write!(f, "encoding: truncated at {at}, need {need} more bytes")
            }
        }
    }
}

impl std::error::Error for EncodingError {}

impl EncodingHeader {
    /// C#'s header read, with the checks its comments describe.
    pub fn parse(data: &[u8]) -> Result<Self, EncodingError> {
        if data.len() < 22 {
            return Err(EncodingError::Truncated { at: 0, need: 22 });
        }
        let magic = [data[0], data[1]];
        if &magic != b"EN" {
            return Err(EncodingError::BadMagic(magic));
        }
        let version = data[2];
        if version != EXPECTED_VERSION {
            return Err(EncodingError::BadVersion(version));
        }
        let c_key_length = data[3];
        let e_key_length = data[4];
        if c_key_length != 16 || e_key_length != 9 {
            return Err(EncodingError::BadKeyLengths {
                c_key: c_key_length,
                e_key: e_key_length,
            });
        }
        // Signed 16-bit big-endian, as the C# reads it — then checked, which
        // the C# does not (bug 3).
        let page_kb = |o: usize| -> Result<u32, EncodingError> {
            let v = i16::from_be_bytes(data[o..o + 2].try_into().unwrap());
            if v <= 0 {
                return Err(EncodingError::BadPageSize(v));
            }
            Ok(v as u32 * 1024)
        };
        let c_key_page_size = page_kb(5)?;
        let e_key_page_size = page_kb(7)?;
        let c_key_page_count = u32::from_be_bytes(data[9..13].try_into().unwrap());
        let e_key_page_count = u32::from_be_bytes(data[13..17].try_into().unwrap());
        let reserved = data[17];
        if reserved != 0 {
            return Err(EncodingError::BadReserved(reserved));
        }
        let espec_block_size = u32::from_be_bytes(data[18..22].try_into().unwrap());
        Ok(Self {
            version,
            c_key_length,
            e_key_length,
            c_key_page_size,
            e_key_page_size,
            c_key_page_count,
            e_key_page_count,
            espec_block_size,
        })
    }

    /// Where the CKey pages begin: header + ESpec block + the page index table
    /// (32 bytes per page).
    pub fn c_key_pages_at(&self) -> u64 {
        22 + self.espec_block_size as u64 + self.c_key_page_count as u64 * 32
    }
}

/// C# `EncodingHandler`.
#[derive(Debug, Clone, Default)]
pub struct EncodingHandler {
    /// C# `EncodingData`, keyed on the full content key.
    pub by_c_key: HashMap<Md5Hash, EncodingEntry>,
    /// C# `EKeyToCKey`, keyed on the 9-byte encoded-key prefix.
    pub e_key_to_c_key: HashMap<[u8; 9], Md5Hash>,
    /// Pages the `remaining == 0xFFF` branch caused to be skipped (bug 6).
    pub skipped_pages: usize,
    /// Duplicate keys the C# would have thrown on (bug 4).
    pub duplicate_keys: usize,
}

impl EncodingHandler {
    pub fn len(&self) -> usize {
        self.by_c_key.len()
    }

    pub fn is_empty(&self) -> bool {
        self.by_c_key.is_empty()
    }

    /// Resolve a content key to its first encoded key.
    pub fn e_key_for(&self, c_key: &Md5Hash) -> Option<&Md5Hash> {
        self.by_c_key.get(c_key).and_then(|e| e.keys.first())
    }

    /// Reverse lookup, as `EKeyToCKey` provides.
    pub fn c_key_for(&self, e_key: &Md5Hash) -> Option<&Md5Hash> {
        let mut k = [0u8; 9];
        k.copy_from_slice(e_key.ekey_prefix());
        self.e_key_to_c_key.get(&k)
    }

    /// C#'s CKey page walk.
    ///
    /// `data` starts at [`EncodingHeader::c_key_pages_at`]. Duplicate keys are
    /// counted rather than thrown on (bug 4), and the `0xFFF` page skip is
    /// reproduced and counted (bug 6).
    pub fn parse_c_key_pages(
        &mut self,
        header: &EncodingHeader,
        data: &[u8],
    ) -> Result<(), EncodingError> {
        let need = |at: usize, n: usize| -> Result<(), EncodingError> {
            if data.len() < at + n {
                Err(EncodingError::Truncated { at, need: n })
            } else {
                Ok(())
            }
        };
        let mut pos = 0usize;
        let mut page = 0u32;
        while page < header.c_key_page_count {
            loop {
                need(pos, 1)?;
                let keys_count = data[pos];
                pos += 1;
                if keys_count == 0 {
                    break;
                }
                need(pos, 5 + 16)?;
                // 40-bit big-endian size.
                let size = data[pos..pos + 5]
                    .iter()
                    .fold(0u64, |a, &b| (a << 8) | b as u64);
                pos += 5;
                let c_key = Md5Hash::from_slice(&data[pos..pos + 16]).unwrap();
                pos += 16;
                need(pos, keys_count as usize * 16)?;
                let mut keys = Vec::with_capacity(keys_count as usize);
                for _ in 0..keys_count {
                    let e_key = Md5Hash::from_slice(&data[pos..pos + 16]).unwrap();
                    pos += 16;
                    keys.push(e_key);
                    let mut prefix = [0u8; 9];
                    prefix.copy_from_slice(e_key.ekey_prefix());
                    // The C# uses Add here and throws on a repeat.
                    if self.e_key_to_c_key.insert(prefix, c_key).is_some() {
                        self.duplicate_keys += 1;
                    }
                }
                if self
                    .by_c_key
                    .insert(c_key, EncodingEntry { keys, size })
                    .is_some()
                {
                    self.duplicate_keys += 1;
                }
            }
            // C#: remaining = CHUNK_SIZE - (pos % CHUNK_SIZE), in 1..=4096.
            let remaining = CHUNK_SIZE - (pos as u64 % CHUNK_SIZE);
            if remaining == 0xFFF {
                // Exactly one byte into a chunk. The C# rewinds it and then
                // advances the page counter twice — see bug 6.
                pos -= 1;
                page += 2;
                self.skipped_pages += 1;
                continue;
            }
            pos += remaining as usize;
            page += 1;
        }
        Ok(())
    }

    /// Look up an ESpec string by index, checked. C# indexes unchecked (bug 5).
    pub fn espec<'a>(
        strings: &'a [&'a str],
        index: i32,
    ) -> Result<Option<&'a str>, EncodingError> {
        if index == -1 {
            return Ok(None); // the C#'s only special case
        }
        if index < 0 || index as usize >= strings.len() {
            return Err(EncodingError::EspecIndexOutOfRange {
                index,
                count: strings.len(),
            });
        }
        Ok(Some(strings[index as usize]))
    }
}

// NOT PORTED: the EKey page walk's ESpec key extraction. The C# pulls
// encryption key names out with the regex `(?<=e:\{)([0-9a-fA-F]{16})(?=,)`;
// that wants a small ESpec parser instead (bug 7).

#[cfg(test)]
mod tests {
    use super::*;

    fn header_bytes(c_pages: u32, e_pages: u32, espec: u32) -> Vec<u8> {
        let mut v = b"EN".to_vec();
        v.push(1); // version
        v.push(16); // ckey length
        v.push(9); // ekey length
        v.extend_from_slice(&4i16.to_be_bytes()); // ckey page size, KB
        v.extend_from_slice(&4i16.to_be_bytes()); // ekey page size, KB
        v.extend_from_slice(&c_pages.to_be_bytes());
        v.extend_from_slice(&e_pages.to_be_bytes());
        v.push(0); // reserved
        v.extend_from_slice(&espec.to_be_bytes());
        v
    }

    #[test]
    fn parses_a_valid_header() {
        let h = EncodingHeader::parse(&header_bytes(2, 3, 100)).unwrap();
        assert_eq!(h.version, 1);
        assert_eq!(h.c_key_page_size, 4096);
        assert_eq!(h.e_key_page_size, 4096);
        assert_eq!(h.c_key_page_count, 2);
        assert_eq!(h.e_key_page_count, 3);
        assert_eq!(h.espec_block_size, 100);
    }

    #[test]
    fn the_magic_is_checked_not_skipped() {
        // The C# does `stream.Skip(2)`.
        let mut v = header_bytes(1, 1, 0);
        v[0] = b'X';
        assert_eq!(EncodingHeader::parse(&v), Err(EncodingError::BadMagic([b'X', b'N'])));
    }

    #[test]
    fn the_must_be_fields_are_enforced() {
        // Both carry "// must be" comments in the C# and neither is checked.
        let mut ver = header_bytes(1, 1, 0);
        ver[2] = 2;
        assert_eq!(EncodingHeader::parse(&ver), Err(EncodingError::BadVersion(2)));
        let mut res = header_bytes(1, 1, 0);
        res[17] = 1;
        assert_eq!(EncodingHeader::parse(&res), Err(EncodingError::BadReserved(1)));
    }

    #[test]
    fn key_lengths_are_checked_against_what_the_code_assumes() {
        let mut v = header_bytes(1, 1, 0);
        v[3] = 20;
        assert_eq!(
            EncodingHeader::parse(&v),
            Err(EncodingError::BadKeyLengths { c_key: 20, e_key: 9 })
        );
    }

    #[test]
    fn a_negative_page_size_is_rejected() {
        // The C# reads Int16BE and multiplies by 1024, so a size above 32 KB
        // comes out negative and is used as a stride.
        let mut v = header_bytes(1, 1, 0);
        v[5..7].copy_from_slice(&(-1i16).to_be_bytes());
        assert_eq!(EncodingHeader::parse(&v), Err(EncodingError::BadPageSize(-1)));
        // And 0 is equally unusable.
        let mut z = header_bytes(1, 1, 0);
        z[5..7].copy_from_slice(&0i16.to_be_bytes());
        assert_eq!(EncodingHeader::parse(&z), Err(EncodingError::BadPageSize(0)));
    }

    #[test]
    fn a_short_header_is_an_error() {
        assert!(matches!(
            EncodingHeader::parse(b"EN"),
            Err(EncodingError::Truncated { .. })
        ));
    }

    #[test]
    fn the_page_offset_accounts_for_the_espec_block_and_index_table() {
        let h = EncodingHeader::parse(&header_bytes(3, 0, 500)).unwrap();
        assert_eq!(h.c_key_pages_at(), 22 + 500 + 3 * 32);
    }

    /// One CKey page: a single entry then the 0 terminator.
    fn page(c_key: u8, e_keys: &[u8], size: u64) -> Vec<u8> {
        let mut v = vec![e_keys.len() as u8];
        v.extend_from_slice(&size.to_be_bytes()[3..]); // 40-bit
        let mut ck = [0u8; 16];
        ck[0] = c_key;
        v.extend_from_slice(&ck);
        for e in e_keys {
            let mut ek = [0u8; 16];
            ek[0] = *e;
            v.extend_from_slice(&ek);
        }
        v.push(0); // terminator
        v.resize(CHUNK_SIZE as usize, 0);
        v
    }

    #[test]
    fn parses_a_ckey_page() {
        let h = EncodingHeader::parse(&header_bytes(1, 0, 0)).unwrap();
        let mut e = EncodingHandler::default();
        e.parse_c_key_pages(&h, &page(0xAA, &[0x11, 0x22], 123456)).unwrap();
        assert_eq!(e.len(), 1);
        let mut ck = [0u8; 16];
        ck[0] = 0xAA;
        let entry = e.by_c_key.get(&Md5Hash::from_bytes(ck)).unwrap();
        assert_eq!(entry.size, 123456);
        assert_eq!(entry.keys.len(), 2);
    }

    #[test]
    fn the_size_field_is_forty_bits_big_endian() {
        let h = EncodingHeader::parse(&header_bytes(1, 0, 0)).unwrap();
        let mut e = EncodingHandler::default();
        // 2^39 exercises the full width.
        let big = 1u64 << 39;
        e.parse_c_key_pages(&h, &page(1, &[1], big)).unwrap();
        assert_eq!(e.by_c_key.values().next().unwrap().size, big);
    }

    #[test]
    fn both_lookup_directions_work() {
        let h = EncodingHeader::parse(&header_bytes(1, 0, 0)).unwrap();
        let mut e = EncodingHandler::default();
        e.parse_c_key_pages(&h, &page(0xAA, &[0x11], 1)).unwrap();
        let mut ck = [0u8; 16];
        ck[0] = 0xAA;
        let mut ek = [0u8; 16];
        ek[0] = 0x11;
        assert_eq!(e.e_key_for(&Md5Hash::from_bytes(ck)).unwrap().0[0], 0x11);
        assert_eq!(e.c_key_for(&Md5Hash::from_bytes(ek)).unwrap().0[0], 0xAA);
    }

    #[test]
    fn duplicate_keys_are_counted_not_thrown_on() {
        // The C# uses Add for both maps and raises ArgumentException. A repeated
        // e-key is legitimate: one stored blob backing two content keys is the
        // point of content-addressable storage.
        let h = EncodingHeader::parse(&header_bytes(2, 0, 0)).unwrap();
        let mut data = page(0xAA, &[0x11], 1);
        data.extend_from_slice(&page(0xBB, &[0x11], 2)); // same e-key
        let mut e = EncodingHandler::default();
        e.parse_c_key_pages(&h, &data).unwrap();
        assert_eq!(e.len(), 2, "both content keys survive");
        assert_eq!(e.duplicate_keys, 1, "and the collision is reported");
    }

    #[test]
    fn espec_lookup_is_bounds_checked() {
        let strings = ["b:{164=z}", "z"];
        assert_eq!(EncodingHandler::espec(&strings, 0).unwrap(), Some("b:{164=z}"));
        assert_eq!(EncodingHandler::espec(&strings, -1).unwrap(), None, "the C#'s case");
        // The C# indexes unchecked for everything else.
        assert!(matches!(
            EncodingHandler::espec(&strings, 5),
            Err(EncodingError::EspecIndexOutOfRange { index: 5, count: 2 })
        ));
        assert!(EncodingHandler::espec(&strings, -2).is_err());
    }

    #[test]
    fn the_chunk_remainder_is_always_in_range() {
        // `remaining` is CHUNK_SIZE - (pos % CHUNK_SIZE), so 1..=4096 — which
        // is why comparing it to 0xFFF means "one byte into a chunk" and
        // nothing else.
        for pos in [0u64, 1, 4095, 4096, 4097, 8191] {
            let r = CHUNK_SIZE - (pos % CHUNK_SIZE);
            assert!((1..=CHUNK_SIZE).contains(&r), "pos {pos} -> {r}");
            assert_eq!(r == 0xFFF, pos % CHUNK_SIZE == 1, "0xFFF means offset 1");
        }
    }

    #[test]
    fn an_empty_page_terminates_immediately() {
        let h = EncodingHeader::parse(&header_bytes(1, 0, 0)).unwrap();
        let mut e = EncodingHandler::default();
        let mut data = vec![0u8]; // terminator straight away
        data.resize(CHUNK_SIZE as usize, 0);
        e.parse_c_key_pages(&h, &data).unwrap();
        assert!(e.is_empty());
    }

    #[test]
    fn a_truncated_page_is_an_error() {
        let h = EncodingHeader::parse(&header_bytes(1, 0, 0)).unwrap();
        let mut e = EncodingHandler::default();
        // Claims one e-key but the data stops short.
        let data = vec![1u8, 0, 0, 0, 0, 1];
        assert!(matches!(
            e.parse_c_key_pages(&h, &data),
            Err(EncodingError::Truncated { .. })
        ));
    }
}
