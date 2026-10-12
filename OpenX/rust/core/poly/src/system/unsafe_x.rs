// PORT-SOURCE: Core/OpenX.Poly/System/UnsafeX.cs
// PORT-SHA: 127398fe3d47d79b
// PORT-STATUS: partial
//
// Ported: the `MarshalP` / `MarshalS` read side, which `BinaryReaderExt::readS`
// and its array forms use, and `FixedAStringScan`, which turns a struct's
// `[u8; N]` (C#'s `ByValTStr` field) into a string. Not ported: `Memcpy` (`copy_from_slice` does it),
// `MarshalS` / `MarshalT` writes, `Shape<T>.StructM` (the `Dictionary<int,
// string>` form of `Struct`, picked by `sizeOf`), and the rest of the file.
//
// C# finds a struct's `static (string, int) Struct` field by reflection. Here
// the struct implements `IStruct`, and `bytemuck::Pod` stands in for
// `Marshal.PtrToStructure` (ADR 0002: struct blitting goes to bytemuck).
//
// C#'s `MarshalSArray` calls `MarshalSApply(bytes, p)` with the default
// `count = 1`, so only the first element of a big-endian array is swapped.
// Kept as is; see rust/_/PORTING.md.

use bytemuck::Pod;
use std::io::{Error, ErrorKind, Result};
use std::mem::size_of;

/// C#'s `static (string, int) Struct` field on a marshalled struct.
pub trait IStruct: Pod {
    const Struct: (&'static str, usize);
}

/// C# `MarshalSApply`: byte-swaps each field of a big-endian (`>`) pattern.
pub fn MarshalSApply(source: &mut [u8], p: &str, count: usize) -> Result<()> {
    if p.as_bytes()[0] == b'<' { return Ok(()); }
    let s = p.as_bytes();
    let (mut _i, mut cnt) = (0usize, 0usize);
    for _ in 0..count {
        for &c in &s[1..] {
            if c.is_ascii_digit() { cnt = cnt * 10 + (c - b'0') as usize; continue; }
            else if cnt == 0 { cnt = 1; }
            let size = MarshalPSymbol(c as char)?;
            if size <= 0 { _i += cnt; }
            else { for _ in 0..cnt { source[_i.._i + size as usize].reverse(); _i += size as usize; } }
            cnt = 0;
        }
    }
    Ok(())
}

// #region MarshalP

/// C# `MarshalPSymbol`.
pub fn MarshalPSymbol(c: char) -> Result<i32> {
    Ok(match c.to_ascii_lowercase() {
        'c' | 'x' | 'b' | 's' => 1,
        'h' => 2,
        'i' | 'f' => 4,
        'q' | 'd' => 8,
        _ => return Err(Error::new(ErrorKind::InvalidInput, format!("Unknown PSymbol: {c}"))),
    })
}

/// C# `MarshalPSize`.
pub fn MarshalPSize(p: &str) -> Result<usize> {
    let s = p.as_bytes();
    let c = s[0] as char;
    if s.len() == 1 { return Ok(MarshalPSymbol(c)? as usize); }
    let (mut _i, mut cnt) = (0usize, 0usize);
    for &c in &s[if c == '<' || c == '>' { 1 } else { 0 }..] {
        if c.is_ascii_digit() { cnt = cnt * 10 + (c - b'0') as usize; continue; }
        else if cnt == 0 { cnt = 1; }
        let size = MarshalPSymbol(c as char)?;
        _i += if size <= 0 { cnt } else { size as usize * cnt };
        cnt = 0;
    }
    Ok(_i)
}

/// C# `MarshalP<T>`.
pub fn MarshalP<T: Pod>(pat: &str, bytesFunc: &mut dyn FnMut(usize) -> Result<Vec<u8>>) -> Result<T> {
    let s = MarshalPSize(pat)?;
    let mut bytes = bytesFunc(s)?;
    if pat.as_bytes()[0] == b'>' { MarshalSApply(&mut bytes, pat, 1)?; }
    Ok(bytemuck::pod_read_unaligned(&bytes[..size_of::<T>()]))
}

/// C# `MarshalPArray<T>`.
pub fn MarshalPArray<T: Pod>(pat: &str, bytesFunc: &mut dyn FnMut(usize) -> Result<Vec<u8>>, count: usize) -> Result<Vec<T>> {
    let s = MarshalPSize(pat)?;
    let mut bytes = bytesFunc(s)?;
    if pat.as_bytes()[0] == b'>' { MarshalSApply(&mut bytes, pat, 1)?; }
    let mut result = vec![T::zeroed(); count];
    let n = bytes.len().min(count * size_of::<T>());
    bytemuck::cast_slice_mut::<T, u8>(&mut result)[..n].copy_from_slice(&bytes[..n]);
    Ok(result)
}

// #endregion

// #region MarshalS

/// C# `MarshalS<T>` (read).
pub fn MarshalS<T: IStruct>(bytesFunc: &mut dyn FnMut(usize) -> Result<Vec<u8>>, sizeOf: i32) -> Result<T> {
    let (p, s) = T::Struct;
    if sizeOf > 0 && sizeOf as usize != s { return Err(Error::new(ErrorKind::InvalidData, format!("Sizes are different: {sizeOf}|{s}"))); }
    let mut bytes = bytesFunc(s)?;
    if p.as_bytes()[0] == b'>' { MarshalSApply(&mut bytes, p, 1)?; }
    Ok(bytemuck::pod_read_unaligned(&bytes[..size_of::<T>()]))
}

/// C# `MarshalSArray<T>` (read).
pub fn MarshalSArray<T: IStruct>(bytesFunc: &mut dyn FnMut(usize) -> Result<Vec<u8>>, count: usize, sizeOf: i32) -> Result<Vec<T>> {
    let (p, s) = T::Struct;
    if sizeOf > 0 && sizeOf as usize != s { return Err(Error::new(ErrorKind::InvalidData, format!("Sizes are different: {sizeOf}|{s}"))); }
    let mut bytes = bytesFunc(s * count)?;
    if p.as_bytes()[0] == b'>' { MarshalSApply(&mut bytes, p, 1)?; }
    let mut result = vec![T::zeroed(); count];
    let n = bytes.len().min(count * size_of::<T>());
    bytemuck::cast_slice_mut::<T, u8>(&mut result)[..n].copy_from_slice(&bytes[..n]);
    Ok(result)
}

// #endregion

/// C# `FixedAStringScan(byte* data, int length)`: ASCII up to the first NUL, `?` for bytes over 0x7F.
/// C# reads `data[length]` when there is no NUL; the slice bound stops there instead.
pub fn FixedAStringScan(data: &[u8], mut length: usize) -> String {
    let mut i = 0;
    while i < data.len() && data[i] != 0 && length > 0 { length -= 1; i += 1; }
    if i > 0 { data[..i].iter().map(|&c| if c < 0x80 { c as char } else { '?' }).collect() } else { String::new() }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[repr(C, packed)]
    #[derive(Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
    struct V_File { Offset: u64, FileSize: u32, Flags2: u16 }
    impl IStruct for V_File { const Struct: (&'static str, usize) = (">QIH", 14); }

    #[test]
    fn patternSizes() {
        assert_eq!(MarshalPSize("<6i").unwrap(), 24);
        assert_eq!(MarshalPSize(">Q4IH").unwrap(), 26);
        assert_eq!(MarshalPSize("<256s3if").unwrap(), 256 + 16);
    }

    #[test]
    fn bigEndianStructsAreSwapped() {
        let data: Vec<u8> = vec![0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 2, 0, 3];
        let mut f = |n: usize| Ok(data[..n].to_vec());
        let v: V_File = MarshalS(&mut f, 0).unwrap();
        let (o, s, f2) = (v.Offset, v.FileSize, v.Flags2);
        assert_eq!((o, s, f2), (1, 2, 3));
    }
}
