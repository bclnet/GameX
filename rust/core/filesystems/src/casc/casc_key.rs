// PORT-SOURCE: Core/GameX.FileSystems/Casc/RootHandlers.cs (MD5Hash / RootEntry / FileDataHash)
// PORT-SHA: SHARED
// PORT-SHARED: yes  (extracted from the source file above, which has its own .rs)
// PORT-STATUS: done
//
// The CASC key types, lifted out of `RootHandlers.cs` where the C# buries them
// at line 4196. Everything in CASC keys on these, so they cannot live inside
// the largest file in the project.
//
// ===================== FOUR C#-SIDE OBSERVATIONS =========================
//
//   1. **`MD5Hash` is two `ulong`s with no byte-order contract.**
//
//          public readonly struct MD5Hash {
//              public readonly ulong lowPart;
//              public readonly ulong highPart;
//          }
//
//      It is populated by reinterpreting 16 bytes read from a file, so its
//      value depends on host endianness — and `MD5HashComparer.GetHashCode`
//      then reinterprets it *again* as four `uint`s via `Unsafe.As`. On a
//      big-endian host both the comparison and the hash change meaning. There
//      is no `FromBytes`/`ToBytes` anywhere; the type is only ever produced by
//      a cast. The port stores the 16 bytes and derives the two words, so the
//      on-disk order is explicit.
//
//   2. **`MD5Hash` has no constructor, no `Equals`, no `GetHashCode`.**
//      Equality lives in a separate `MD5HashComparer` singleton, so
//      `hash1 == hash2` uses the default struct comparison (field-wise, which
//      happens to be right) while `Dictionary<MD5Hash, T>` uses the comparer
//      only if it was passed one. A dictionary built without it silently uses
//      reflection-based `ValueType.GetHashCode` — correct but slow, on the
//      hottest lookup path in the system.
//
//   3. **`FileDataHash.ComputeHash` mixes signed and unsigned.**
//      `0x100000001B3L * (... ^ baseOffset)` multiplies a `long` literal by a
//      `ulong` expression. That pairing has no common type in C#, so either the
//      literal is being coerced in a way worth checking, or this does not
//      compile as written. The algorithm itself is plain FNV-1a over the four
//      little-endian bytes of the id, which is what the port implements.
//
//   4. **`ContentFlagsFilter.Filter` enumerates its input three times.**
//      Two `temp.Any(...)` calls plus the final consumption, over an
//      `IEnumerable` that is a LINQ chain over the root file. For a WoW root
//      with millions of entries that is three full passes where one would do.

/// C# `MD5Hash` — a 16-byte CASC content or encoding key.
///
/// Stored as bytes in on-disk order, with `low_part`/`high_part` derived, so
/// the layout does not depend on host endianness (observation 1).
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default, PartialOrd, Ord)]
pub struct Md5Hash(pub [u8; 16]);

impl Md5Hash {
    /// C# `CASC_CKEY_SIZE`.
    pub const CKEY_SIZE: usize = 0x10;
    /// C# `CASC_EKEY_SIZE` — encoded keys are truncated to 9 bytes in indices.
    pub const EKEY_SIZE: usize = 0x09;

    pub const fn from_bytes(b: [u8; 16]) -> Self {
        Self(b)
    }

    /// `None` when the slice is not 16 bytes. The C# reinterprets whatever is
    /// at the pointer.
    pub fn from_slice(b: &[u8]) -> Option<Self> {
        Some(Self(b.try_into().ok()?))
    }

    /// C# `lowPart` — the first 8 bytes as a little-endian u64.
    #[inline]
    pub fn low_part(&self) -> u64 {
        u64::from_le_bytes(self.0[..8].try_into().unwrap())
    }

    /// C# `highPart` — the last 8 bytes as a little-endian u64.
    #[inline]
    pub fn high_part(&self) -> u64 {
        u64::from_le_bytes(self.0[8..].try_into().unwrap())
    }

    /// The first `EKEY_SIZE` bytes, which is how indices key encoded entries.
    #[inline]
    pub fn ekey_prefix(&self) -> &[u8] {
        &self.0[..Self::EKEY_SIZE]
    }

    /// Whether this key is all zeroes, which CASC uses as "absent".
    #[inline]
    pub fn is_zero(&self) -> bool {
        self.0 == [0u8; 16]
    }

    /// C# `MD5HashComparer.GetHashCode` — FNV-1a over the key's four `uint`s.
    ///
    /// The C# reads them with `Unsafe.As<MD5Hash, uint>`, which assumes the
    /// struct is exactly 16 bytes with no padding and is host-endian. This
    /// reads them from the stored bytes little-endian, so the value is stable
    /// across platforms.
    pub fn fnv1a32(&self) -> u32 {
        const OFFSET: u32 = 2166136261;
        const PRIME: u32 = 16777619;
        let mut hash = OFFSET;
        for i in 0..4 {
            let w = u32::from_le_bytes(self.0[i * 4..i * 4 + 4].try_into().unwrap());
            hash ^= w;
            hash = hash.wrapping_mul(PRIME);
        }
        hash
    }
}

impl std::fmt::Display for Md5Hash {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        for b in self.0 {
            write!(f, "{b:02x}")?;
        }
        Ok(())
    }
}

impl std::str::FromStr for Md5Hash {
    type Err = String;
    /// Parse a 32-character hex key, as CASC config files and CDN paths carry.
    fn from_str(s: &str) -> Result<Self, Self::Err> {
        if s.len() != 32 {
            return Err(format!("expected 32 hex characters, got {}", s.len()));
        }
        let mut out = [0u8; 16];
        for (i, slot) in out.iter_mut().enumerate() {
            *slot = u8::from_str_radix(&s[i * 2..i * 2 + 2], 16)
                .map_err(|e| format!("bad hex at {}: {e}", i * 2))?;
        }
        Ok(Self(out))
    }
}

/// C# `RootEntry`.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct RootEntry {
    pub c_key: Md5Hash,
    pub content_flags: u32,
    pub locale_flags: u32,
}

/// C# `FileDataHash.ComputeHash(int fileDataId)` — FNV-1a 64 over the four
/// little-endian bytes of the id.
///
/// See observation 3 on the C#'s signed/unsigned mixing. The constants here are
/// the standard FNV-1a 64-bit offset basis and prime.
pub fn file_data_hash(file_data_id: u32) -> u64 {
    const OFFSET: u64 = 0xCBF2_9CE4_8422_2325;
    const PRIME: u64 = 0x0000_0100_0000_01B3;
    let mut hash = OFFSET;
    for i in 0..4 {
        let byte = ((file_data_id >> (8 * i)) & 0xFF) as u64;
        hash = PRIME.wrapping_mul(byte ^ hash);
    }
    hash
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::str::FromStr;

    const KEY: &str = "0123456789abcdef0123456789abcdef";

    #[test]
    fn keys_round_trip_through_hex() {
        let k = Md5Hash::from_str(KEY).unwrap();
        assert_eq!(k.to_string(), KEY);
    }

    #[test]
    fn hex_parsing_is_length_checked() {
        assert!(Md5Hash::from_str("0123").is_err());
        assert!(Md5Hash::from_str(&format!("{KEY}00")).is_err());
        assert!(Md5Hash::from_str("zz234567890abcdef0123456789abcdef").is_err());
    }

    #[test]
    fn the_word_views_are_little_endian_over_the_stored_bytes() {
        // The C# reinterprets the struct, so this is host-dependent there.
        let k = Md5Hash::from_bytes([
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
        ]);
        assert_eq!(k.low_part(), 0x0807_0605_0403_0201);
        assert_eq!(k.high_part(), 0x1817_1615_1413_1211);
    }

    #[test]
    fn from_slice_rejects_the_wrong_length() {
        assert!(Md5Hash::from_slice(&[0u8; 16]).is_some());
        assert!(Md5Hash::from_slice(&[0u8; 9]).is_none());
        assert!(Md5Hash::from_slice(&[0u8; 17]).is_none());
    }

    #[test]
    fn ekey_prefix_is_the_first_nine_bytes() {
        let k = Md5Hash::from_str(KEY).unwrap();
        assert_eq!(k.ekey_prefix().len(), Md5Hash::EKEY_SIZE);
        assert_eq!(k.ekey_prefix(), &k.0[..9]);
        assert_eq!(Md5Hash::CKEY_SIZE, 16);
    }

    #[test]
    fn the_zero_key_is_recognised() {
        assert!(Md5Hash::default().is_zero());
        assert!(!Md5Hash::from_str(KEY).unwrap().is_zero());
    }

    #[test]
    fn fnv1a32_is_order_sensitive_and_stable() {
        let a = Md5Hash::from_str(KEY).unwrap();
        let mut b = a;
        b.0.swap(0, 15);
        assert_ne!(a.fnv1a32(), b.fnv1a32(), "byte order must matter");
        assert_eq!(a.fnv1a32(), a.fnv1a32());
        // A known value, so a platform change shows up as a test failure
        // rather than as cache misses.
        assert_eq!(Md5Hash::default().fnv1a32(), {
            let mut h = 2166136261u32;
            for _ in 0..4 { h ^= 0; h = h.wrapping_mul(16777619); }
            h
        });
    }

    #[test]
    fn file_data_hash_matches_fnv1a_over_four_le_bytes() {
        // Independent reimplementation, to check the loop shape.
        fn reference(id: u32) -> u64 {
            let mut h = 0xCBF2_9CE4_8422_2325u64;
            for b in id.to_le_bytes() {
                h ^= b as u64;
                h = h.wrapping_mul(0x0000_0100_0000_01B3);
            }
            h
        }
        for id in [0u32, 1, 53183, 0xFFFF_FFFF, 1234567] {
            assert_eq!(file_data_hash(id), reference(id), "id {id}");
        }
    }

    #[test]
    fn file_data_hash_distinguishes_ids() {
        assert_ne!(file_data_hash(0), file_data_hash(1));
        assert_ne!(file_data_hash(1), file_data_hash(256), "byte position matters");
    }

    #[test]
    fn keys_are_usable_as_map_keys() {
        use std::collections::HashMap;
        let mut m = HashMap::new();
        let k = Md5Hash::from_str(KEY).unwrap();
        m.insert(k, 42);
        assert_eq!(m.get(&Md5Hash::from_str(KEY).unwrap()), Some(&42));
    }

    #[test]
    fn keys_order_by_their_bytes() {
        // Ord matters: indices are binary-searched by key prefix.
        let a = Md5Hash::from_bytes([0u8; 16]);
        let b = Md5Hash::from_bytes([1u8; 16]);
        assert!(a < b);
    }
}
