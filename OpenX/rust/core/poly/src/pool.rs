// PORT-SOURCE: Core/OpenX.Polyfills/System.Collections.Generic/KVExtensions.cs
// PORT-SHA: 81620d1bd889ef5f
// PORT-STATUS: done
//
// Home per OpenX/Polyfills.md: C# `System.Collections.Generic/` is
// `openx_poly::pool`. Only `KV` has a caller so far (`openx-gfx-egin`'s
// `egin_particle`); `path` stays parked in `_/system_collections_generic/`.
//
// Typed accessors over `IDictionary<string, object>` — the loosely-typed
// key-value trees that Valve-style format parsers produce. C# casts the boxed
// `object` at each access and lets `InvalidCastException` escape.
//
// Rust has no boxed-primitive `object`, so the port introduces an explicit
// `KV` value enum. That turns every cast into a checked conversion: the
// `as*` family returns `Option`, so a missing key and a wrong-typed value are
// both ordinary results instead of exceptions.
//
// The C# also silently returns `default(T)` for a missing key in some overloads
// and throws in others, which callers could not tell apart. Here it is uniform.

use std::collections::HashMap;

use glam::{Vec3, Vec4};

pub use crate::generic_pool::{GenericPool, StaticPool};

/// A loosely-typed value from a parsed key-value tree.
#[derive(Debug, Clone, PartialEq)]
pub enum KV {
    Null,
    Bool(bool),
    Int(i64),
    UInt(u64),
    Float(f64),
    Str(String),
    Array(Vec<KV>),
    Map(HashMap<String, KV>),
}

impl KV {
    /// C# `GetInt32` / `GetInt64`. Accepts an integer, or a string that parses
    /// as one, matching the C#'s tolerance for both.
    pub fn asI64(&self) -> Option<i64> {
        match self {
            KV::Int(v) => Some(*v),
            KV::UInt(v) => i64::try_from(*v).ok(),
            KV::Float(v) => Some(*v as i64),
            KV::Bool(b) => Some(*b as i64),
            KV::Str(s) => s.trim().parse().ok(),
            _ => None,
        }
    }

    /// C# `GetUInt32` / `GetUInt64`.
    pub fn asU64(&self) -> Option<u64> {
        match self {
            KV::UInt(v) => Some(*v),
            KV::Int(v) => u64::try_from(*v).ok(),
            KV::Float(v) if *v >= 0.0 => Some(*v as u64),
            KV::Bool(b) => Some(*b as u64),
            KV::Str(s) => s.trim().parse().ok(),
            _ => None,
        }
    }

    /// C# `GetDouble` / `GetFloat`.
    pub fn asF64(&self) -> Option<f64> {
        match self {
            KV::Float(v) => Some(*v),
            KV::Int(v) => Some(*v as f64),
            KV::UInt(v) => Some(*v as f64),
            KV::Str(s) => s.trim().parse().ok(),
            _ => None,
        }
    }

    pub fn asF32(&self) -> Option<f32> {
        self.asF64().map(|v| v as f32)
    }

    pub fn asBool(&self) -> Option<bool> {
        match self {
            KV::Bool(b) => Some(*b),
            KV::Int(v) => Some(*v != 0),
            KV::UInt(v) => Some(*v != 0),
            KV::Str(s) => match s.trim().to_ascii_lowercase().as_str() {
                "true" | "1" => Some(true),
                "false" | "0" => Some(false),
                _ => None,
            },
            _ => None,
        }
    }

    pub fn asStr(&self) -> Option<&str> {
        match self {
            KV::Str(s) => Some(s),
            _ => None,
        }
    }

    /// C# `GetArray` / `GetMap`.
    pub fn asArray(&self) -> Option<&[KV]> {
        match self {
            KV::Array(v) => Some(v),
            _ => None,
        }
    }

    /// C# `GetSub(string)`.
    pub fn asMap(&self) -> Option<&HashMap<String, KV>> {
        match self {
            KV::Map(m) => Some(m),
            _ => None,
        }
    }

    /// C# `Get<TKey, TValue>(dict, key)` — look up a child by key.
    pub fn get(&self, key: &str) -> Option<&KV> {
        self.asMap()?.get(key).filter(|v| **v != KV::Null)
    }

    /// C# `GetVector3` / `ToVector3` — three consecutive numbers.
    ///
    /// Accepts either an array of three numbers or `{x, y, z}` keys, both of
    /// which appear in the formats this parses.
    pub fn asVec3(&self) -> Option<Vec3> {
        if let Some(a) = self.asArray() {
            if a.len() >= 3 {
                return Some(Vec3::new(
                    a[0].asF32()?,
                    a[1].asF32()?,
                    a[2].asF32()?,
                ));
            }
            return None;
        }
        let m = self.asMap()?;
        Some(Vec3::new(
            m.get("x")?.asF32()?,
            m.get("y")?.asF32()?,
            m.get("z")?.asF32()?,
        ))
    }

    /// C# `GetVector4` / `ToVector4`.
    pub fn asVec4(&self) -> Option<Vec4> {
        if let Some(a) = self.asArray() {
            if a.len() >= 4 {
                return Some(Vec4::new(
                    a[0].asF32()?,
                    a[1].asF32()?,
                    a[2].asF32()?,
                    a[3].asF32()?,
                ));
            }
            return None;
        }
        let m = self.asMap()?;
        Some(Vec4::new(
            m.get("x")?.asF32()?,
            m.get("y")?.asF32()?,
            m.get("z")?.asF32()?,
            m.get("w")?.asF32()?,
        ))
    }

    /// C# `GetInt64Array` / `GetUInt64Array`.
    pub fn asI64Array(&self) -> Option<Vec<i64>> {
        self.asArray()?.iter().map(KV::asI64).collect()
    }

    pub fn asU64Array(&self) -> Option<Vec<u64>> {
        self.asArray()?.iter().map(KV::asU64).collect()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn sample() -> KV {
        let mut inner = HashMap::new();
        inner.insert("count".into(), KV::Int(42));
        inner.insert("name".into(), KV::Str("thing".into()));
        inner.insert("scale".into(), KV::Str("2.5".into()));
        inner.insert(
            "pos".into(),
            KV::Array(vec![KV::Float(1.0), KV::Float(2.0), KV::Float(3.0)]),
        );
        let mut root = HashMap::new();
        root.insert("entity".into(), KV::Map(inner));
        KV::Map(root)
    }

    #[test]
    fn nested_lookup() {
        let kv = sample();
        assert_eq!(kv.get("entity").and_then(|e| e.get("count")).unwrap().asI64(), Some(42));
        assert!(kv.get("entity").and_then(|e| e.get("missing")).is_none());
        assert!(kv.get("nope").and_then(|e| e.get("count")).is_none());
    }

    #[test]
    fn numeric_strings_convert_like_the_c_sharp() {
        let kv = sample();
        let scale = kv.get("entity").and_then(|e| e.get("scale")).unwrap();
        assert_eq!(scale.asF32(), Some(2.5));
        assert_eq!(scale.asI64(), Some(2), "string -> int truncates");
    }

    #[test]
    fn wrong_type_is_none_not_an_exception() {
        let kv = sample();
        let name = kv.get("entity").and_then(|e| e.get("name")).unwrap();
        assert_eq!(name.asI64(), None);
        assert_eq!(name.asStr(), Some("thing"));
    }

    #[test]
    fn vectors_parse_from_arrays_and_from_xyz_maps() {
        let kv = sample();
        let from_array = kv.get("entity").and_then(|e| e.get("pos")).unwrap().asVec3().unwrap();
        assert_eq!(from_array, Vec3::new(1.0, 2.0, 3.0));

        let mut m = HashMap::new();
        m.insert("x".to_string(), KV::Float(4.0));
        m.insert("y".to_string(), KV::Float(5.0));
        m.insert("z".to_string(), KV::Float(6.0));
        assert_eq!(KV::Map(m).asVec3().unwrap(), Vec3::new(4.0, 5.0, 6.0));
    }

    #[test]
    fn short_arrays_do_not_produce_a_partial_vector() {
        let short = KV::Array(vec![KV::Float(1.0), KV::Float(2.0)]);
        assert!(short.asVec3().is_none());
    }

    #[test]
    fn null_values_read_as_missing() {
        let mut m = HashMap::new();
        m.insert("k".to_string(), KV::Null);
        assert!(KV::Map(m).get("k").is_none());
    }

    #[test]
    fn integer_arrays_fail_as_a_whole_on_a_bad_element() {
        let good = KV::Array(vec![KV::Int(1), KV::Int(2)]);
        assert_eq!(good.asI64Array(), Some(vec![1, 2]));
        let bad = KV::Array(vec![KV::Int(1), KV::Str("x".into())]);
        assert_eq!(bad.asI64Array(), None);
    }
}
