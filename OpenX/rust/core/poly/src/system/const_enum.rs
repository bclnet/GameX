// No C# twin: Rust's stand-in for a C# `enum` too large for a Rust enum.
//
// A C# enum is its integer, and rustc's cost for a Rust enum's derives grows
// with the square of its variant count: `#[derive(Debug)]` alone on WB's
// 31,108-member `WeenieClassName` needs about 23 GB to check. So gen_enums.py
// emits big enums as a newtype over the C# base type, with each C# member an
// associated const, and implements `ConstEnum` for the names. Any value, named
// or not, is valid, as in C#; `match` works on the consts.

use std::fmt;

/// A C# enum as a newtype with associated consts.
pub trait ConstEnum: Copy + 'static {
    /// The C# base type (`ushort`, `int`, ...).
    type Raw: Copy + Ord + fmt::LowerHex + 'static;
    /// The members' names, one per value (the first C# member for a shared value), sorted by value.
    const Names: &'static [(Self::Raw, &'static str)];

    fn raw(self) -> Self::Raw;

    /// C# `Enum.GetName`: the member's name, or `None` for an unnamed value.
    fn name(self) -> Option<&'static str> {
        let v = self.raw();
        Self::Names.binary_search_by_key(&v, |s| s.0).ok().map(|i| Self::Names[i].1)
    }

    /// C# `ToString()`: the name, or the value in hex. Generated `Debug` impls call this.
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self.name() {
            Some(n) => f.write_str(n),
            None => write!(f, "{:#x}", self.raw()),
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[derive(Clone, Copy, PartialEq, Eq)]
    struct Color(u8);
    impl Color { const Red: Self = Self(1); const Blue: Self = Self(4); }
    impl ConstEnum for Color { type Raw = u8; const Names: &'static [(u8, &'static str)] = &[(1, "Red"), (4, "Blue")]; fn raw(self) -> u8 { self.0 } }
    impl fmt::Debug for Color { fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { ConstEnum::fmt(self, f) } }

    #[test]
    fn namesAndUnnamedValues() {
        assert_eq!(format!("{:?}", Color::Blue), "Blue");
        assert_eq!(Color(2).name(), None);
        assert_eq!(format!("{:?}", Color(2)), "0x2");
        assert!(matches!(Color(1), Color::Red));
    }
}
