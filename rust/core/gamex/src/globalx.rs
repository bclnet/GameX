// PORT-SOURCE: Core/GameX/Globalx.cs
// PORT-SHA: 307f2118fdbdfacb
// PORT-STATUS: done
//
// Four colour structs with on-disk layouts: `Color3` (`<3f`, 12 bytes),
// `ByteColor3` (`<3c`, 3), `Color4` (`<4f`, 16), `ByteColor4` (`<4c`, 4).
//
// The `Struct` tuples are Python `struct`-module format strings — these types
// are read by both the C# and a Python tool, and the string is the shared
// contract. Preserved as constants so that stays visible.
//
// ===================== THREE C#-SIDE BUGS ================================
//
//   1. **`Color3(byte[] s)` assigns raw bytes to float components.**
//
//          public Color3(byte[] s) { R = s[0]; G = s[1]; B = s[2]; }
//
//      `R`, `G`, `B` are `float` in 0..1 everywhere else in the type — the
//      `AsColor` property multiplies by 255 — so a byte of 255 becomes 255.0,
//      not 1.0, and `AsColor` then computes 255 * 255. This is the same defect
//      as `Colorf(uint, Format.ARGB32)` in the OpenStack port. **Fix in the
//      C#**: divide by 255.
//
//   2. **`AsColor` casts without clamping.** `(int)(R * 255f)` on a component
//      above 1.0 (which bug 1 produces routinely) overflows the 0..255 range
//      and `Color.FromArgb` throws `ArgumentException`.
//
//   3. **`public Color3() { }` leaves all three components at 0** — a
//      parameterless struct constructor that produces opaque black, where the
//      surrounding code treats a default `Color3` as "unset". Nothing
//      distinguishes them.

use std::io::{self, Read};

/// Python `struct` format and byte width, as the C# `Struct` tuples record.
pub const COLOR3_STRUCT: (&str, usize) = ("<3f", 12);
pub const BYTE_COLOR3_STRUCT: (&str, usize) = ("<3c", 3);
pub const COLOR4_STRUCT: (&str, usize) = ("<4f", 16);
pub const BYTE_COLOR4_STRUCT: (&str, usize) = ("<4c", 4);

/// C# `Color3` — three floats, nominally 0..1.
#[derive(Debug, Clone, Copy, Default, PartialEq)]
pub struct Color3 {
    pub r: f32,
    pub g: f32,
    pub b: f32,
}

impl Color3 {
    /// C# `Color3(double, double, double)`.
    pub const fn new(r: f32, g: f32, b: f32) -> Self {
        Self { r, g, b }
    }

    /// C# `Color3(byte[] s)`, with the division the C# omits (bug 1).
    pub fn from_bytes(s: [u8; 3]) -> Self {
        Self {
            r: s[0] as f32 / 255.0,
            g: s[1] as f32 / 255.0,
            b: s[2] as f32 / 255.0,
        }
    }

    /// The C#'s literal behaviour, for reading data it wrote.
    #[deprecated(note = "mirrors a C#-side bug: assigns 0..255 to a 0..1 float")]
    pub fn from_bytes_bug_compat(s: [u8; 3]) -> Self {
        Self { r: s[0] as f32, g: s[1] as f32, b: s[2] as f32 }
    }

    /// C# `Color3(BinaryReader r)` — three little-endian f32.
    pub fn read<R: Read>(r: &mut R) -> io::Result<Self> {
        let mut b = [0u8; 12];
        r.read_exact(&mut b)?;
        let f = |i: usize| f32::from_le_bytes([b[i], b[i + 1], b[i + 2], b[i + 3]]);
        Ok(Self { r: f(0), g: f(4), b: f(8) })
    }

    /// C# `AsColor` — clamped, which the C# is not (bug 2).
    pub fn as_rgb8(&self) -> [u8; 3] {
        let c = |x: f32| (x.clamp(0.0, 1.0) * 255.0).round() as u8;
        [c(self.r), c(self.g), c(self.b)]
    }
}

impl std::fmt::Display for Color3 {
    /// C# `ToString()` — `{R:g9} {G:g9} {B:g9}`.
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{:.9} {:.9} {:.9}", self.r, self.g, self.b)
    }
}

/// C# `ByteColor3` — three bytes as stored.
#[derive(Debug, Clone, Copy, Default, PartialEq, Eq)]
pub struct ByteColor3 {
    pub r: u8,
    pub g: u8,
    pub b: u8,
}

impl ByteColor3 {
    pub fn read<R: Read>(r: &mut R) -> io::Result<Self> {
        let mut b = [0u8; 3];
        r.read_exact(&mut b)?;
        Ok(Self { r: b[0], g: b[1], b: b[2] })
    }

    pub fn to_color3(self) -> Color3 {
        Color3::from_bytes([self.r, self.g, self.b])
    }
}

/// C# `Color4`.
#[derive(Debug, Clone, Copy, Default, PartialEq)]
pub struct Color4 {
    pub r: f32,
    pub g: f32,
    pub b: f32,
    pub a: f32,
}

impl Color4 {
    pub const fn new(r: f32, g: f32, b: f32, a: f32) -> Self {
        Self { r, g, b, a }
    }

    pub fn read<R: Read>(r: &mut R) -> io::Result<Self> {
        let mut b = [0u8; 16];
        r.read_exact(&mut b)?;
        let f = |i: usize| f32::from_le_bytes([b[i], b[i + 1], b[i + 2], b[i + 3]]);
        Ok(Self { r: f(0), g: f(4), b: f(8), a: f(12) })
    }

    pub fn as_rgba8(&self) -> [u8; 4] {
        let c = |x: f32| (x.clamp(0.0, 1.0) * 255.0).round() as u8;
        [c(self.r), c(self.g), c(self.b), c(self.a)]
    }
}

/// C# `ByteColor4`.
#[derive(Debug, Clone, Copy, Default, PartialEq, Eq)]
pub struct ByteColor4 {
    pub r: u8,
    pub g: u8,
    pub b: u8,
    pub a: u8,
}

impl ByteColor4 {
    pub fn read<R: Read>(r: &mut R) -> io::Result<Self> {
        let mut b = [0u8; 4];
        r.read_exact(&mut b)?;
        Ok(Self { r: b[0], g: b[1], b: b[2], a: b[3] })
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::io::Cursor;

    #[test]
    fn struct_widths_match_their_format_strings() {
        // "<3f" is 3 little-endian floats = 12 bytes, and so on.
        assert_eq!(COLOR3_STRUCT, ("<3f", 12));
        assert_eq!(BYTE_COLOR3_STRUCT, ("<3c", 3));
        assert_eq!(COLOR4_STRUCT, ("<4f", 16));
        assert_eq!(BYTE_COLOR4_STRUCT, ("<4c", 4));
    }

    #[test]
    fn bytes_are_normalised_to_zero_one() {
        let c = Color3::from_bytes([255, 128, 0]);
        assert_eq!(c.r, 1.0);
        assert!((c.g - 128.0 / 255.0).abs() < 1e-6);
        assert_eq!(c.b, 0.0);
    }

    #[test]
    fn the_c_sharp_byte_constructor_produces_out_of_range_components() {
        #[allow(deprecated)]
        let c = Color3::from_bytes_bug_compat([255, 128, 0]);
        assert_eq!(c.r, 255.0, "a 0..1 component holding 255");
        // And AsColor then computes 255 * 255, which FromArgb rejects.
        assert!((c.r * 255.0) as i32 > 255);
    }

    #[test]
    fn as_rgb8_round_trips_a_normalised_colour() {
        let c = Color3::from_bytes([10, 200, 255]);
        assert_eq!(c.as_rgb8(), [10, 200, 255]);
    }

    #[test]
    fn as_rgb8_clamps_rather_than_overflowing() {
        // The C# casts unclamped and FromArgb throws.
        let c = Color3::new(2.0, -1.0, 0.5);
        assert_eq!(c.as_rgb8(), [255, 0, 128]);
    }

    #[test]
    fn reads_are_little_endian_and_exact_width() {
        let mut v = Vec::new();
        for f in [1.0f32, 0.5, 0.25] {
            v.extend_from_slice(&f.to_le_bytes());
        }
        let c = Color3::read(&mut Cursor::new(&v)).unwrap();
        assert_eq!((c.r, c.g, c.b), (1.0, 0.5, 0.25));
        // A short buffer is an error, not a partial read.
        assert!(Color3::read(&mut Cursor::new(&v[..8])).is_err());
    }

    #[test]
    fn color4_reads_four_components() {
        let mut v = Vec::new();
        for f in [1.0f32, 0.0, 0.5, 0.25] {
            v.extend_from_slice(&f.to_le_bytes());
        }
        let c = Color4::read(&mut Cursor::new(&v)).unwrap();
        assert_eq!(c.as_rgba8(), [255, 0, 128, 64]);
    }

    #[test]
    fn byte_colours_read_their_widths() {
        let c3 = ByteColor3::read(&mut Cursor::new([1u8, 2, 3])).unwrap();
        assert_eq!((c3.r, c3.g, c3.b), (1, 2, 3));
        let c4 = ByteColor4::read(&mut Cursor::new([1u8, 2, 3, 4])).unwrap();
        assert_eq!(c4.a, 4);
        assert!(ByteColor4::read(&mut Cursor::new([1u8, 2])).is_err());
    }

    #[test]
    fn byte_color3_converts_through_the_normalised_path() {
        let c = ByteColor3 { r: 255, g: 0, b: 0 }.to_color3();
        assert_eq!(c.r, 1.0, "not 255.0");
    }

    #[test]
    fn display_matches_the_c_sharp_shape() {
        let s = Color3::new(1.0, 0.5, 0.0).to_string();
        assert_eq!(s.split_whitespace().count(), 3);
    }
}
