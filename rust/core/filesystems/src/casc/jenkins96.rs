// PORT-SOURCE: Core/GameX.FileSystems/Casc/Jenkins96.cs
// PORT-SHA: 5d7d4ce8ec085d84
// PORT-STATUS: done
//
// Bob Jenkins' `hashlittle2` (lookup3), the 64-bit filename hash CASC uses to
// key files in a WoW root manifest.
//
// ===================== THREE C#-SIDE BUGS ================================
//
//   1. **`HashCore` ignores `ibStart` and `cbSize`.**
//
//          protected override unsafe void HashCore(byte[] array, int ibStart, int cbSize) {
//              uint length = (uint)array.Length;   // <- not cbSize
//
//      Both parameters are declared and neither is read: it hashes the *whole
//      array* from index 0. `HashAlgorithm`'s contract is to hash `cbSize`
//      bytes from `ibStart`, which is what `TransformBlock` and
//      `ComputeHash(buffer, offset, count)` rely on. So this class produces
//      correct results only through its own
//      `ComputeHash(string)` helper, and silently wrong ones through the base
//      class's streaming API it inherits. Deriving from `HashAlgorithm` and
//      then not honouring its contract is the whole problem — the port is a
//      plain function.
//
//   2. **`hashBytes` is `static` on a class with instance state.**
//      `private static byte[] hashBytes = new byte[0];` is what `HashFinal`
//      returns, so every instance's `Hash` property yields the same empty
//      array while `hashValue` is per-instance. Two threads hashing different
//      paths share it.
//
//   3. **`Array.Resize(ref array, newLen)` inside `HashCore`.** The padding to
//      a 12-byte multiple allocates a copy, so the caller's array is untouched
//      — but only because `ref` on a parameter is local. It reads as though it
//      mutates the input, and a reviewer has to know C#'s parameter semantics
//      to see that it does not. It also means every hash allocates.
//
// Ported as a function over `&[u8]`, which makes 1 and 2 unrepresentable.
//
// VERIFIED: the empty-input case is checkable from the algorithm's own
// definition — with no bytes, `a = b = c = 0xdeadbeef + 0`, so the result is
// `0xdeadbeef_deadbeef`. That pins the initial constants and the
// `(c << 32) | b` packing. The mixing rounds are asserted for their documented
// properties (case-insensitivity, slash normalisation, order sensitivity)
// rather than against published vectors, which I could not obtain here.

/// C# `Jenkins96.ComputeHash(byte[])` — `hashlittle2`, packed as `(c << 32) | b`.
pub fn hash(data: &[u8]) -> u64 {
    let len = data.len();
    let init = 0xdead_beefu32.wrapping_add(len as u32);
    let (mut a, mut b, mut c) = (init, init, init);
    if len == 0 {
        return ((c as u64) << 32) | b as u64;
    }

    // C# pads the buffer up to a multiple of 12 with zeroes, then reads
    // 12-byte blocks as three little-endian u32. Done without allocating by
    // reading past-the-end bytes as zero.
    let padded = len + (12 - len % 12) % 12;
    let word = |i: usize| -> u32 {
        let mut v = [0u8; 4];
        for (k, slot) in v.iter_mut().enumerate() {
            *slot = data.get(i * 4 + k).copied().unwrap_or(0);
        }
        u32::from_le_bytes(v)
    };

    let rot = |x: u32, k: u32| x.rotate_left(k);
    let mut j = 0usize;
    while j + 12 < padded {
        a = a.wrapping_add(word(j / 4));
        b = b.wrapping_add(word(j / 4 + 1));
        c = c.wrapping_add(word(j / 4 + 2));
        a = a.wrapping_sub(c); a ^= rot(c, 4);  c = c.wrapping_add(b);
        b = b.wrapping_sub(a); b ^= rot(a, 6);  a = a.wrapping_add(c);
        c = c.wrapping_sub(b); c ^= rot(b, 8);  b = b.wrapping_add(a);
        a = a.wrapping_sub(c); a ^= rot(c, 16); c = c.wrapping_add(b);
        b = b.wrapping_sub(a); b ^= rot(a, 19); a = a.wrapping_add(c);
        c = c.wrapping_sub(b); c ^= rot(b, 4);  b = b.wrapping_add(a);
        j += 12;
    }

    let i = padded - 12;
    a = a.wrapping_add(word(i / 4));
    b = b.wrapping_add(word(i / 4 + 1));
    c = c.wrapping_add(word(i / 4 + 2));
    c ^= b; c = c.wrapping_sub(rot(b, 14));
    a ^= c; a = a.wrapping_sub(rot(c, 11));
    b ^= a; b = b.wrapping_sub(rot(a, 25));
    c ^= b; c = c.wrapping_sub(rot(b, 16));
    a ^= c; a = a.wrapping_sub(rot(c, 4));
    b ^= a; b = b.wrapping_sub(rot(a, 14));
    c ^= b; c = c.wrapping_sub(rot(b, 24));
    ((c as u64) << 32) | b as u64
}

/// C# `ComputeHash(string str, bool fix = true)`.
///
/// With `fix`, CASC's path convention: `/` becomes `\`, then uppercase, then
/// ASCII bytes. `Encoding.ASCII` maps anything above `0x7F` to `?`, which is
/// reproduced — a non-ASCII path therefore hashes as if it contained `?`, and
/// two paths differing only outside ASCII collide.
pub fn hash_path(s: &str, fix: bool) -> u64 {
    let prepared = if fix {
        s.replace('/', "\\").to_uppercase()
    } else {
        s.to_string()
    };
    let bytes: Vec<u8> = prepared
        .chars()
        .map(|c| if (c as u32) < 0x80 { c as u8 } else { b'?' })
        .collect();
    hash(&bytes)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn the_empty_input_pins_the_initial_constants() {
        // With no bytes a = b = c = 0xdeadbeef + 0, and the result is
        // (c << 32) | b. This checks both the seed and the packing.
        assert_eq!(hash(&[]), 0xdead_beef_dead_beef);
    }

    #[test]
    fn paths_are_normalised_before_hashing() {
        // CASC keys on backslash-separated uppercase paths.
        assert_eq!(hash_path("a/b", true), hash_path("A\\B", true));
        assert_eq!(hash_path("Interface/Icons", true), hash_path("INTERFACE\\ICONS", true));
    }

    #[test]
    fn without_fix_the_string_is_hashed_verbatim() {
        assert_ne!(hash_path("a/b", false), hash_path("a/b", true));
        assert_eq!(hash_path("A\\B", false), hash_path("a/b", true));
    }

    #[test]
    fn the_hash_is_order_sensitive() {
        assert_ne!(hash(b"ab"), hash(b"ba"));
        assert_ne!(hash_path("a/b", true), hash_path("b/a", true));
    }

    #[test]
    fn length_is_mixed_in_so_padding_does_not_collide() {
        // The seed is 0xdeadbeef + length, so inputs differing only by
        // trailing zeroes must not collide despite the 12-byte padding.
        assert_ne!(hash(b"abc"), hash(b"abc\0"));
        assert_ne!(hash(b"abc\0"), hash(b"abc\0\0"));
    }

    #[test]
    fn block_boundaries_are_handled() {
        // Exactly 12, just under, just over — the loop condition and the
        // final-block index are where an off-by-one would show.
        let a = hash(&[1u8; 11]);
        let b = hash(&[1u8; 12]);
        let c = hash(&[1u8; 13]);
        let d = hash(&[1u8; 24]);
        for (x, y) in [(a, b), (b, c), (c, d), (a, d)] {
            assert_ne!(x, y);
        }
    }

    #[test]
    fn long_inputs_are_deterministic() {
        let data: Vec<u8> = (0..1000).map(|i| (i % 251) as u8).collect();
        assert_eq!(hash(&data), hash(&data));
        let mut other = data.clone();
        other[999] ^= 1;
        assert_ne!(hash(&data), hash(&other), "last byte must matter");
        let mut first = data.clone();
        first[0] ^= 1;
        assert_ne!(hash(&data), hash(&first), "first byte must matter");
    }

    #[test]
    fn non_ascii_paths_collapse_to_question_marks_as_the_c_sharp_does() {
        // Encoding.ASCII maps > 0x7F to '?', so these collide in both.
        assert_eq!(hash_path("caf\u{00e9}", true), hash_path("CAF?", true));
        assert_eq!(hash_path("a\u{00e9}", true), hash_path("a\u{00fc}", true));
    }

    #[test]
    fn hashing_a_slice_hashes_only_that_slice() {
        // The C#'s HashCore ignores ibStart/cbSize and hashes the whole array,
        // so this distinction does not exist there.
        let buf = b"XXXXhelloYYYY";
        assert_eq!(hash(&buf[4..9]), hash(b"hello"));
        assert_ne!(hash(&buf[4..9]), hash(buf));
    }
}
