// PORT-SOURCE: Core/GameX.FileSystems/Casc/HexConverter.cs
// PORT-SHA: 31ae014e248c4090
// PORT-STATUS: done
//
// Byte-array to hex string and back.
//
// This is **`System.HexConverter` from the .NET runtime, copied verbatim** —
// including its branchless nibble trick:
//
//     uint difference = (((uint)value & 0xF0U) << 4) + ((uint)value & 0x0FU) - 0x8989U;
//     uint packedResult = ((((uint)(-(int)difference) & 0x7070U) >> 4)
//                          + difference + 0xB9B9U) | (uint)casing;
//
// I checked that against plain formatting for **all 256 byte values, both
// casings**: identical output. So it is a micro-optimisation, not a different
// algorithm, and porting the trick verbatim would add unsafe-adjacent
// arithmetic for no behavioural gain. This uses a lookup table instead, which
// is the same output and readable.
//
// Two notes on the C#:
//
//   * **`Casing` is an enum whose values are ORed into packed character
//     bits** — `Upper = 0`, `Lower = 0x2020`. That is the ASCII case bit for
//     two characters at once, so the enum is not a set of options but a
//     pre-shifted bitmask. Any value other than those two produces garbage,
//     and nothing prevents one.
//   * **`ToCharsBuffer` writes `buffer[startingIndex + 1]` before
//     `buffer[startingIndex]`**, and indexes both unchecked. A `startingIndex`
//     one past the end writes out of bounds on the *first* store, so the array
//     is corrupted before the exception.

/// C# `HexConverter.Casing`.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub enum Casing {
    #[default]
    Upper,
    Lower,
}

const UPPER: &[u8; 16] = b"0123456789ABCDEF";
const LOWER: &[u8; 16] = b"0123456789abcdef";

/// C# `ToCharsBuffer(byte value, char[] buffer, int startingIndex, Casing)`.
///
/// Returns the two characters rather than writing through an index, so the
/// out-of-bounds path in observation 2 cannot occur.
#[inline]
pub fn to_chars(value: u8, casing: Casing) -> [u8; 2] {
    let t = match casing {
        Casing::Upper => UPPER,
        Casing::Lower => LOWER,
    };
    [t[(value >> 4) as usize], t[(value & 0x0F) as usize]]
}

/// C# `ToString(byte[] bytes, Casing casing)`.
pub fn to_string(bytes: &[u8], casing: Casing) -> String {
    let mut s = String::with_capacity(bytes.len() * 2);
    for &b in bytes {
        let [hi, lo] = to_chars(b, casing);
        s.push(hi as char);
        s.push(lo as char);
    }
    s
}

/// C# `TryDecodeFromUtf16(string chars, byte[] bytes, out int charsProcessed)`.
///
/// Decodes into `out`, returning how many characters were consumed. `false`
/// when a non-hex character is met or the input is not a whole number of
/// pairs — matching the C#'s contract, which reports partial progress through
/// `charsProcessed`.
pub fn try_decode(chars: &str, out: &mut [u8]) -> (bool, usize) {
    let b = chars.as_bytes();
    if b.len() % 2 != 0 {
        return (false, 0);
    }
    let n = (b.len() / 2).min(out.len());
    for i in 0..n {
        let hi = from_hex(b[i * 2]);
        let lo = from_hex(b[i * 2 + 1]);
        match (hi, lo) {
            (Some(h), Some(l)) => out[i] = (h << 4) | l,
            // The C# reports how far it got, so a caller can locate the bad
            // character; preserved.
            _ => return (false, i * 2),
        }
    }
    (n == b.len() / 2, n * 2)
}

#[inline]
fn from_hex(c: u8) -> Option<u8> {
    Some(match c {
        b'0'..=b'9' => c - b'0',
        b'a'..=b'f' => c - b'a' + 10,
        b'A'..=b'F' => c - b'A' + 10,
        _ => return None,
    })
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn every_byte_matches_plain_formatting_in_both_casings() {
        // This is the check that justified dropping the .NET bit trick: it is
        // an optimisation, not a different algorithm.
        for b in 0u8..=255 {
            let up = to_chars(b, Casing::Upper);
            let lo = to_chars(b, Casing::Lower);
            assert_eq!(
                std::str::from_utf8(&up).unwrap(),
                format!("{b:02X}"),
                "upper {b:#04x}"
            );
            assert_eq!(
                std::str::from_utf8(&lo).unwrap(),
                format!("{b:02x}"),
                "lower {b:#04x}"
            );
        }
    }

    #[test]
    fn known_values() {
        assert_eq!(to_string(&[0x00, 0xAB, 0xFF], Casing::Upper), "00ABFF");
        assert_eq!(to_string(&[0x00, 0xAB, 0xFF], Casing::Lower), "00abff");
        assert_eq!(to_string(&[], Casing::Upper), "");
    }

    #[test]
    fn decoding_round_trips() {
        let data: Vec<u8> = (0..64).map(|i| (i * 7 % 256) as u8).collect();
        for casing in [Casing::Upper, Casing::Lower] {
            let s = to_string(&data, casing);
            let mut out = vec![0u8; data.len()];
            let (ok, used) = try_decode(&s, &mut out);
            assert!(ok);
            assert_eq!(used, s.len());
            assert_eq!(out, data, "{casing:?}");
        }
    }

    #[test]
    fn mixed_case_input_decodes() {
        let mut out = [0u8; 2];
        let (ok, _) = try_decode("aBcD", &mut out);
        assert!(ok);
        assert_eq!(out, [0xAB, 0xCD]);
    }

    #[test]
    fn an_odd_length_input_is_rejected() {
        let mut out = [0u8; 4];
        assert_eq!(try_decode("ABC", &mut out), (false, 0));
    }

    #[test]
    fn a_bad_character_reports_how_far_it_got() {
        // The C# surfaces this through `charsProcessed`.
        let mut out = [0u8; 4];
        let (ok, used) = try_decode("00ZZ4444", &mut out);
        assert!(!ok);
        assert_eq!(used, 2, "one byte consumed before the bad pair");
        assert_eq!(out[0], 0x00);
    }

    #[test]
    fn a_short_destination_stops_early_without_overrunning() {
        // The C# writes through an unchecked index.
        let mut out = [0u8; 1];
        let (ok, used) = try_decode("AABBCC", &mut out);
        assert!(!ok, "not all input consumed");
        assert_eq!(used, 2);
        assert_eq!(out[0], 0xAA);
    }

    #[test]
    fn casing_is_a_closed_set() {
        // The C# enum's values are pre-shifted ASCII case bits (0 and 0x2020)
        // ORed into packed characters, so any other value emits garbage.
        // An enum with two variants cannot hold one.
        assert_ne!(Casing::Upper, Casing::Lower);
        assert_eq!(Casing::default(), Casing::Upper);
    }
}
