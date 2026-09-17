// PORT-SOURCE: Core/GameX.FileSystems/Casc/LocalIndexHandler.cs
// PORT-SHA: 439d35aa667b8c43
// PORT-STATUS: done
//
// Parses the 16 local `.idx` files that map a truncated 9-byte encoded key to
// its location in the `data.NNN` archives. This is the lookup every local file
// read goes through.
//
// ===================== FIVE C#-SIDE BUGS ================================
//
//   1. **"Latest index" is whichever file the OS lists last.**
//
//          var files = Directory.EnumerateFiles(dir, $"{i:x2}*.idx");
//          if (files.Any()) latestIdx.Add(files.Last());
//
//      `.idx` files are versioned (`0000000042.idx`, `0000000043.idx`) and the
//      newest must win — an older index points at archive offsets that have
//      since been rewritten. `EnumerateFiles` returns filesystem order, which
//      is **not** sorted: NTFS happens to return B-tree order (usually
//      lexicographic), ext4 returns hash order. So this works on Windows by
//      accident and picks an arbitrary index on Linux. **Fix in the C#** by
//      sorting. The port sorts explicitly.
//
//      `files.Any()` also enumerates the directory a second time.
//
//   2. **The 16-byte alignment masks are 32-bit.**
//      `padPos = (8 + HeaderHashSize + 0x0F) & 0xFFFFFFF0` and
//      `(EntriesSize + 0x0FFF) & 0xFFFFF000` are applied to `long`s, so every
//      bit above 32 is cleared — a position at or past 4 GiB wraps to near
//      zero and the parse silently restarts mid-file. `.idx` files are small,
//      so this does not bite today; the mask is still wrong, and the intent
//      (`(x + 15) & !15`) is expressible without a literal.
//
//   3. **`HeaderHashSize` is read from the file and used to allocate,
//      unchecked.** `br.ReadBytes(HeaderHashSize)` on a corrupt or hostile
//      index allocates whatever it says.
//
//   4. **Both checksums are read and never verified.** `HeaderHash` and
//      `EntriesHash` are assigned to locals that nothing reads — so the
//      integrity fields exist in the format, are parsed out, and are ignored.
//
//   5. **`ContainsKey` then `Add` hashes every key twice.** `TryAdd` does it
//      once, on the hottest parse loop in the system (hundreds of thousands of
//      entries across 16 files).
//
// Note the entry layout mixes endianness — `indexLow` is big-endian and `Size`
// is little-endian, in the same 18-byte record. That is the format, not a bug,
// and is preserved.

use std::collections::HashMap;

use super::casc_key::Md5Hash;

/// Bytes per `.idx` entry: 9-byte key + 1 + 4 (location) + 4 (size).
pub const ENTRY_SIZE: usize = 18;
/// C# `CASC_INDEX_COUNT` — one index per key bucket.
pub const INDEX_COUNT: usize = 0x10;

/// C# `IndexEntry`.
#[derive(Debug, Clone, Copy, Default, PartialEq, Eq)]
pub struct IndexEntry {
    /// Which `data.NNN` archive.
    pub index: u32,
    /// Byte offset within it.
    pub offset: u32,
    /// Encoded size.
    pub size: u32,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum IndexError {
    NoIdxFiles,
    /// The header declares a hash size the file cannot hold (bug 3).
    BadHeaderHashSize(u32),
    Truncated { at: usize, need: usize },
}

impl std::fmt::Display for IndexError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::NoIdxFiles => write!(f, "idx files are missing"),
            Self::BadHeaderHashSize(n) => write!(f, "idx header hash size {n} is implausible"),
            Self::Truncated { at, need } => {
                write!(f, "idx truncated at {at}, need {need} more bytes")
            }
        }
    }
}

impl std::error::Error for IndexError {}

/// Round up to a multiple of `align`. The C# uses 32-bit literal masks on
/// 64-bit values (bug 2).
#[inline]
fn align_up(v: u64, align: u64) -> u64 {
    (v + align - 1) & !(align - 1)
}

/// C# `LocalIndexHandler`.
#[derive(Debug, Clone, Default)]
pub struct LocalIndexHandler {
    /// Keyed on the 9-byte prefix, as `MD5HashComparer9` does.
    data: HashMap<[u8; 9], IndexEntry>,
}

impl LocalIndexHandler {
    pub fn len(&self) -> usize {
        self.data.len()
    }

    pub fn is_empty(&self) -> bool {
        self.data.is_empty()
    }

    /// Look up by full or truncated encoded key.
    pub fn get(&self, ekey: &Md5Hash) -> Option<&IndexEntry> {
        let mut k = [0u8; 9];
        k.copy_from_slice(ekey.ekey_prefix());
        self.data.get(&k)
    }

    /// C# `ParseIndex(string idx)`, over bytes.
    pub fn parse_into(&mut self, data: &[u8]) -> Result<usize, IndexError> {
        let need = |at: usize, n: usize| -> Result<(), IndexError> {
            if data.len() < at + n {
                Err(IndexError::Truncated { at, need: n })
            } else {
                Ok(())
            }
        };
        need(0, 8)?;
        let header_hash_size = u32::from_le_bytes(data[0..4].try_into().unwrap());
        // The C# passes this straight to ReadBytes (bug 3).
        if header_hash_size as usize > data.len() {
            return Err(IndexError::BadHeaderHashSize(header_hash_size));
        }
        // `HeaderHash` at [4..8] is read and never verified by the C# (bug 4).

        let pad = align_up(8 + header_hash_size as u64, 0x10) as usize;
        need(pad, 8)?;
        let entries_size = u32::from_le_bytes(data[pad..pad + 4].try_into().unwrap()) as usize;
        // `EntriesHash` at [pad+4..pad+8] likewise unverified.
        let body = pad + 8;
        let num = entries_size / ENTRY_SIZE;
        // Integer division drops a trailing partial entry silently in the C#.
        need(body, num * ENTRY_SIZE)?;

        let mut added = 0;
        for i in 0..num {
            let at = body + i * ENTRY_SIZE;
            let mut key = [0u8; 9];
            key.copy_from_slice(&data[at..at + 9]);
            let index_high = data[at + 9] as u32;
            // Big-endian, unlike `size` below — that is the format.
            let index_low = u32::from_be_bytes(data[at + 10..at + 14].try_into().unwrap());
            let entry = IndexEntry {
                index: (index_high << 2) | ((index_low & 0xC000_0000) >> 30),
                offset: index_low & 0x3FFF_FFFF,
                // Little-endian in the same record.
                size: u32::from_le_bytes(data[at + 14..at + 18].try_into().unwrap()),
            };
            // C#: `if (!ContainsKey(key)) Add(key, info)` — first key wins,
            // two hashes per entry. `or_insert` does it in one.
            if !self.data.contains_key(&key) {
                self.data.insert(key, entry);
                added += 1;
            }
        }
        Ok(added)
    }

    /// Pick one `.idx` per bucket, newest first.
    ///
    /// C# `GetIdxFiles` takes `files.Last()` of an unordered enumeration
    /// (bug 1). Sorting makes "latest" mean what it says.
    pub fn select_latest(mut names: Vec<String>) -> Vec<String> {
        names.sort();
        let mut out: Vec<String> = Vec::new();
        for bucket in 0..INDEX_COUNT {
            let prefix = format!("{bucket:02x}");
            if let Some(n) = names
                .iter()
                .filter(|n| n.starts_with(&prefix) && n.ends_with(".idx"))
                .next_back()
            {
                out.push(n.clone());
            }
        }
        out
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// Build a minimal `.idx` with the given entries.
    fn idx(entries: &[(u8, u32, u32, u32)]) -> Vec<u8> {
        let mut v = Vec::new();
        v.extend_from_slice(&0u32.to_le_bytes()); // header hash size
        v.extend_from_slice(&0u32.to_le_bytes()); // header hash
        while v.len() < align_up(8, 0x10) as usize {
            v.push(0);
        }
        let entries_size = (entries.len() * ENTRY_SIZE) as u32;
        v.extend_from_slice(&entries_size.to_le_bytes());
        v.extend_from_slice(&0u32.to_le_bytes()); // entries hash
        for (key0, index_high, index_low, size) in entries {
            let mut k = [0u8; 9];
            k[0] = *key0;
            v.extend_from_slice(&k);
            v.push(*index_high as u8);
            v.extend_from_slice(&index_low.to_be_bytes());
            v.extend_from_slice(&size.to_le_bytes());
        }
        v
    }

    #[test]
    fn parses_entries_and_decodes_the_packed_location() {
        // index = high << 2 | top 2 bits of low; offset = low & 0x3FFFFFFF
        let low = (0b10 << 30) | 0x1234;
        let mut h = LocalIndexHandler::default();
        assert_eq!(h.parse_into(&idx(&[(0xAA, 3, low, 999)])).unwrap(), 1);
        let e = h.data.values().next().unwrap();
        assert_eq!(e.index, (3 << 2) | 0b10);
        assert_eq!(e.offset, 0x1234);
        assert_eq!(e.size, 999);
    }

    #[test]
    fn the_offset_is_thirty_bits() {
        let mut h = LocalIndexHandler::default();
        h.parse_into(&idx(&[(1, 0, 0xFFFF_FFFF, 0)])).unwrap();
        let e = h.data.values().next().unwrap();
        assert_eq!(e.offset, 0x3FFF_FFFF, "top two bits belong to the index");
        assert_eq!(e.index, 0b11);
    }

    #[test]
    fn location_is_big_endian_and_size_little_endian() {
        // Mixed within one 18-byte record — that is the format.
        let mut h = LocalIndexHandler::default();
        h.parse_into(&idx(&[(1, 0, 0x0000_0001, 0x0100_0000)])).unwrap();
        let e = h.data.values().next().unwrap();
        assert_eq!(e.offset, 1);
        assert_eq!(e.size, 0x0100_0000);
    }

    #[test]
    fn the_first_key_wins() {
        let mut h = LocalIndexHandler::default();
        let added = h
            .parse_into(&idx(&[(0xAA, 1, 100, 10), (0xAA, 2, 200, 20)]))
            .unwrap();
        assert_eq!(added, 1, "duplicate key skipped");
        assert_eq!(h.data.values().next().unwrap().offset, 100);
    }

    #[test]
    fn lookup_uses_the_nine_byte_prefix() {
        let mut h = LocalIndexHandler::default();
        h.parse_into(&idx(&[(0xAA, 0, 5, 50)])).unwrap();
        // A full 16-byte key whose first 9 bytes match must resolve.
        let mut full = [0u8; 16];
        full[0] = 0xAA;
        full[15] = 0xFF; // differs beyond the prefix
        assert_eq!(h.get(&Md5Hash::from_bytes(full)).unwrap().offset, 5);
    }

    #[test]
    fn a_truncated_file_is_an_error() {
        let mut h = LocalIndexHandler::default();
        assert!(matches!(h.parse_into(&[0u8; 4]), Err(IndexError::Truncated { .. })));
        let mut short = idx(&[(1, 0, 0, 0)]);
        short.truncate(short.len() - 3);
        assert!(matches!(
            LocalIndexHandler::default().parse_into(&short),
            Err(IndexError::Truncated { .. })
        ));
    }

    #[test]
    fn an_implausible_header_hash_size_is_rejected_before_allocating() {
        // The C# passes this straight to ReadBytes.
        let mut v = 0xFFFF_FFFFu32.to_le_bytes().to_vec();
        v.extend_from_slice(&[0u8; 32]);
        assert_eq!(
            LocalIndexHandler::default().parse_into(&v),
            Err(IndexError::BadHeaderHashSize(0xFFFF_FFFF))
        );
    }

    #[test]
    fn alignment_is_correct_past_four_gibibytes() {
        // The C#'s 0xFFFFFFF0 / 0xFFFFF000 masks clear every bit above 32.
        assert_eq!(align_up(8, 0x10), 16);
        assert_eq!(align_up(16, 0x10), 16);
        assert_eq!(align_up(17, 0x10), 32);
        let big = 0x1_0000_0001u64;
        assert_eq!(align_up(big, 0x1000), 0x1_0000_1000);
        assert_ne!(align_up(big, 0x1000), (big + 0xFFF) & 0xFFFF_F000);
    }

    #[test]
    fn latest_index_selection_is_ordered_not_filesystem_dependent() {
        // The C# takes files.Last() of Directory.EnumerateFiles, which is
        // NTFS B-tree order on Windows and hash order on ext4.
        let names = vec![
            "0000000043.idx".to_string(),
            "0000000042.idx".to_string(),
            "0100000007.idx".to_string(),
            "0100000009.idx".to_string(),
        ];
        // Shuffled input must give the same answer.
        let a = LocalIndexHandler::select_latest(names.clone());
        let mut rev = names.clone();
        rev.reverse();
        let b = LocalIndexHandler::select_latest(rev);
        assert_eq!(a, b);
        assert_eq!(a, vec!["0000000043.idx", "0100000009.idx"]);
    }

    #[test]
    fn buckets_without_an_index_are_skipped() {
        let out = LocalIndexHandler::select_latest(vec!["0f00000001.idx".to_string()]);
        assert_eq!(out, vec!["0f00000001.idx"]);
        assert!(LocalIndexHandler::select_latest(vec![]).is_empty());
    }

    #[test]
    fn non_idx_names_are_ignored() {
        let out = LocalIndexHandler::select_latest(vec![
            "0000000001.idx".to_string(),
            "0000000002.tmp".to_string(),
        ]);
        assert_eq!(out, vec!["0000000001.idx"]);
    }

    #[test]
    fn an_empty_index_parses_to_nothing() {
        let mut h = LocalIndexHandler::default();
        assert_eq!(h.parse_into(&idx(&[])).unwrap(), 0);
        assert!(h.is_empty());
    }
}
