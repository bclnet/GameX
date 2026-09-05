// PORT-SOURCE: Core/GameX/Util.cs
// PORT-SHA: 8f7ab9954542a057
// PORT-STATUS: done
//
// JSON config helpers and file-format sniffing.
//
// The 112-entry magic-number tables below were **extracted from the C# by
// script**, not transcribed. At that size a single mistyped nibble would
// mis-identify a game asset in a way no test would obviously catch — the same
// reasoning as the enum tables and the Huffman table in the library port.
//
// ===================== FIVE C#-SIDE BUGS =================================
//
//   1. **The same magic gives two different answers.** `0x75B22630` maps to
//      `.asf` on the `fast` path and `.mov` on the slow path, so the extension
//      depends on a bool the caller passes rather than on the bytes. That magic
//      is the ASF header GUID prefix (`30 26 B2 75` little-endian), so `.asf`
//      is the correct answer and the slow table is wrong. **Fix in the C#.**
//
//   2. **The fallback builds an extension from raw bytes.** Both paths end in
//      `$".{Encoding.ASCII.GetString(buf.AsSpan(0, 3)).ToLowerInvariant()}"`,
//      which for non-text input yields a nonsense extension containing control
//      characters or U+FFFD. It cannot fail, so an unknown format is
//      indistinguishable from a recognised one.
//
//   3. **`_valueV` throws on booleans and nulls.** Its `switch` covers Number,
//      String, Array and Object, and `JsonValueKind.True`/`False`/`Null` fall
//      to `_ => throw new ArgumentOutOfRangeException`. A `true` anywhere in a
//      config file aborts the parse.
//
//   4. **`_valueV` reads every number as `Int32`.** A float or a value beyond
//      `int` range throws from `GetInt32()`.
//
//   5. **`_random` is an unsynchronised lazily-created `System.Random`.**
//      `_random ??= new Random()` from two threads can construct two
//      instances, and `Random` itself is not thread-safe — concurrent `Next`
//      calls can corrupt its internal state and return 0 indefinitely.
//
// Also: `_randomValue(low, high)` is **inclusive** of `high`
// (`_random.Next(low, high + 1)`), unlike every other range API in either
// language. Preserved, and asserted in a test.

use std::collections::HashMap;

/// A JSON value, as the C# handled via `System.Text.Json.JsonElement`.
///
/// `Bool` and `Null` are present because the C# threw on them (bug 3), and
/// `Float` because it read every number as `i32` (bug 4).
#[derive(Debug, Clone, PartialEq)]
pub enum Value {
    Null,
    Bool(bool),
    Int(i64),
    Float(f64),
    Str(String),
    Array(Vec<Value>),
    Object(HashMap<String, Value>),
}

impl Value {
    /// C# `_value(elem, key, default)`.
    pub fn str_or<'a>(&'a self, key: &str, default: &'a str) -> &'a str {
        self.get(key).and_then(Value::as_str).unwrap_or(default)
    }

    /// C# `_valueBool(elem, key, default)`.
    pub fn bool_or(&self, key: &str, default: bool) -> bool {
        self.get(key).and_then(Value::as_bool).unwrap_or(default)
    }

    /// C# `_valueF(elem, key, func, default)` — apply `f` when the key exists.
    pub fn map_or<T>(&self, key: &str, f: impl FnOnce(&Value) -> T, default: T) -> T {
        match self.get(key) {
            Some(v) => f(v),
            None => default,
        }
    }

    /// C# `_list(elem, key, func, default)` / `_listV`.
    ///
    /// A scalar is promoted to a one-element list, as in the C#. Unlike the C#
    /// this returns `None` rather than throwing for a kind it cannot flatten.
    pub fn list(&self, key: &str) -> Option<Vec<String>> {
        self.get(key).and_then(Value::flatten)
    }

    /// C# `_listV(elem)`.
    pub fn flatten(&self) -> Option<Vec<String>> {
        Some(match self {
            Value::Int(n) => vec![n.to_string()],
            Value::Float(n) => vec![n.to_string()],
            Value::Str(s) => vec![s.clone()],
            Value::Bool(b) => vec![b.to_string()],
            Value::Array(a) => a.iter().filter_map(Value::as_str).map(str::to_string).collect(),
            _ => return None,
        })
    }

    /// C# `_related(elem, key, func)` — an object read as a keyed map.
    pub fn related(&self, key: &str) -> HashMap<String, Value> {
        match self.get(key) {
            Some(Value::Object(m)) => m.clone(),
            _ => HashMap::new(),
        }
    }

    /// C# `_dictTrim(source)` — drop null-valued entries.
    pub fn dict_trim(source: &HashMap<String, Value>) -> HashMap<String, Value> {
        source
            .iter()
            .filter(|(_, v)| **v != Value::Null)
            .map(|(k, v)| (k.clone(), v.clone()))
            .collect()
    }

    pub fn get(&self, key: &str) -> Option<&Value> {
        match self {
            Value::Object(m) => m.get(key),
            _ => None,
        }
    }

    pub fn as_str(&self) -> Option<&str> {
        match self {
            Value::Str(s) => Some(s),
            _ => None,
        }
    }

    pub fn as_bool(&self) -> Option<bool> {
        match self {
            Value::Bool(b) => Some(*b),
            _ => None,
        }
    }

    pub fn as_i64(&self) -> Option<i64> {
        match self {
            Value::Int(n) => Some(*n),
            Value::Float(f) => Some(*f as i64),
            _ => None,
        }
    }
}

/// C# `_randomValue(int low, int high)`.
///
/// **Inclusive of `high`** — the C# calls `Next(low, high + 1)`. Preserved.
/// Takes its state by argument rather than using a shared unsynchronised
/// static (bug 5); the caller owns the generator.
pub fn random_value(rng: &mut impl FnMut() -> u64, low: i32, high: i32) -> i32 {
    if high <= low {
        return low;
    }
    let span = (high as i64 - low as i64 + 1) as u64;
    (low as i64 + (rng() % span) as i64) as i32
}

/// C# `_guessExtension(buf, fast: true)` — the one-entry fast table.
static FAST_MAGICS: &[(u32, &str)] = &[
    (0x75B22630, ".asf"),
];

/// C# slow path, matched against bytes 0..4.
static PRIMARY_MAGICS: &[(u32, &str)] = &[
    (0x000001D8, ".motlist"),
    (0x00424956, ".vib"),
    (0x00444957, ".wid"),
    (0x00444F4C, ".lod"),
    (0x00444252, ".rbd"),
    (0x004C4452, ".rdl"),
    (0x00424650, ".pfb"),
    (0x00464453, ".mmtr"),
    (0x0046444D, ".mdf2"),
    (0x004C4F46, ".fol"),
    (0x004E4353, ".scn"),
    (0x004F4C43, ".clo"),
    (0x00504D4C, ".lmp"),
    (0x00535353, ".sss"),
    (0x00534549, ".ies"),
    (0x00530040, ".wel"),
    (0x00584554, ".tex"),
    (0x00525355, ".user"),
    (0x005A5352, ".wcc"),
    (0x04034B50, ".zip"),
    (0x4D534C43, ".clsm"),
    (0x54414D2E, ".mat"),
    (0x54464453, ".sdft"),
    (0x44424453, ".sdbd"),
    (0x52554653, ".sfur"),
    (0x464E4946, ".finf"),
    (0x4D455241, ".arem"),
    (0x21545353, ".sst"),
    (0x204D4252, ".rbm"),
    (0x4D534648, ".hfsm"),
    (0x59444F42, ".rdd"),
    (0x20464544, ".def"),
    (0x4252504E, ".nprb"),
    (0x44484B42, ".bnk"),
    (0x75B22630, ".mov"),
    (0x4853454D, ".mesh"),
    (0x4B504B41, ".pck"),
    (0x50534552, ".spmdl"),
    (0x54564842, ".fsmv2"),
    (0x4C4F4352, ".rcol"),
    (0x5556532E, ".uvs"),
    (0x4C494643, ".cfil"),
    (0x54504E47, ".gnpt"),
    (0x54414D43, ".cmat"),
    (0x44545254, ".trtd"),
    (0x50494C43, ".clip"),
    (0x564D4552, ".mov"),
    (0x414D4941, ".aimapattr"),
    (0x504D4941, ".aimp"),
    (0x72786665, ".efx"),
    (0x736C6375, ".ucls"),
    (0x54435846, ".fxct"),
    (0x58455452, ".rtex"),
    (0x4F464246, ".oft"),
    (0x4C4F434D, ".mcol"),
    (0x46454443, ".cdef"),
    (0x504F5350, ".psop"),
    (0x454D414D, ".mame"),
    (0x43414D4D, ".mameac"),
    (0x544C5346, ".fslt"),
    (0x64637273, ".srcd"),
    (0x68637273, ".asrc"),
    (0x4F525541, ".auto"),
    (0x7261666C, ".lfar"),
    (0x52524554, ".terr"),
    (0x736E636A, ".jcns"),
    (0x6C626C74, ".tmlbld"),
    (0x54455343, ".cset"),
    (0x726D6565, ".eemr"),
    (0x434C4244, ".dblc"),
    (0x384D5453, ".stmesh"),
    (0x32736674, ".tmlfsm2"),
    (0x45555141, ".aque"),
    (0x46554247, ".gbuf"),
    (0x4F4C4347, ".gclo"),
    (0x44525453, ".srtd"),
    (0x544C4946, ".filt"),
];

/// C# slow path fallback, matched against bytes 4..8.
static SECONDARY_MAGICS: &[(u32, &str)] = &[
    (0x00766544, ".dev"),
    (0x6E616863, ".chain"),
    (0x6E6C6B73, ".fbxskel"),
    (0x47534D47, ".msg"),
    (0x52495547, ".gui"),
    (0x47464347, ".gcfg"),
    (0x72617675, ".uvar"),
    (0x544E4649, ".ifnt"),
    (0x20746F6D, ".mot"),
    (0x70797466, ".mov"),
    (0x6D61636D, ".mcam"),
    (0x6572746D, ".mtre"),
    (0x6D73666D, ".mfsm"),
    (0x74736C6D, ".motlist"),
    (0x6B6E626D, ".motbank"),
    (0x3273666D, ".motfsm2"),
    (0x74736C63, ".mcamlist"),
    (0x70616D6A, ".jmap"),
    (0x736E636A, ".jcns"),
    (0x4E414554, ".tean"),
    (0x61646B69, ".ikda"),
    (0x736C6B69, ".ikls"),
    (0x72746B69, ".iktr"),
    (0x326C6B69, ".ikl2"),
    (0x72686366, ".fchr"),
    (0x544C5346, ".fslt"),
    (0x6B6E6263, ".cbnk"),
    (0x30474154, ".havokcl"),
    (0x52504347, ".gcpr"),
    (0x74646366, ".fcmndatals"),
    (0x67646C6A, ".jointlodgroup"),
    (0x444E5347, ".gsnd"),
    (0x59545347, ".gsty"),
    (0x3267656C, ".leg2"),
];

/// C# `_guessExtension(byte[] buf, bool fast = true)`.
///
/// Returns `None` where the C# fabricated an extension from raw bytes (bug 2),
/// so "unrecognised" is distinguishable from "recognised".
///
/// The `fast` flag is preserved but note it changes the answer for
/// `0x75B22630` in the C# — see bug 1. Here both paths agree on `.asf`, which
/// is what that magic actually denotes.
pub fn guess_extension(buf: &[u8], fast: bool) -> Option<&'static str> {
    if buf.len() < 4 {
        return None; // C# returns String.Empty
    }
    let at = |o: usize| -> Option<u32> {
        Some(u32::from_le_bytes(buf.get(o..o + 4)?.try_into().ok()?))
    };
    let first = at(0)?;
    let lookup = |t: &'static [(u32, &'static str)], v: u32| {
        t.iter().find(|(m, _)| *m == v).map(|(_, e)| *e)
    };
    if let Some(e) = lookup(FAST_MAGICS, first) {
        return Some(e);
    }
    if fast {
        return None;
    }
    if let Some(e) = lookup(PRIMARY_MAGICS, first) {
        return Some(e);
    }
    lookup(SECONDARY_MAGICS, at(4)?)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn magic_tables_are_the_expected_size() {
        // Guards the extraction: these counts come from the C# source.
        assert_eq!(FAST_MAGICS.len(), 1);
        assert_eq!(PRIMARY_MAGICS.len(), 77);
        assert_eq!(SECONDARY_MAGICS.len(), 34);
    }

    #[test]
    fn no_magic_appears_twice_in_a_table() {
        for t in [FAST_MAGICS, PRIMARY_MAGICS, SECONDARY_MAGICS] {
            let mut seen = std::collections::HashSet::new();
            for (m, e) in t {
                assert!(seen.insert(m), "duplicate magic {m:#010X} -> {e}");
            }
        }
    }

    #[test]
    fn known_magics_resolve() {
        // 0x04034B50 is the ZIP local file header, PK\x03\x04.
        assert_eq!(guess_extension(&[0x50, 0x4B, 0x03, 0x04], false), Some(".zip"));
        // 0x75B22630 is the ASF header GUID prefix.
        assert_eq!(guess_extension(&[0x30, 0x26, 0xB2, 0x75], true), Some(".asf"));
    }

    #[test]
    fn the_fast_flag_no_longer_changes_the_answer() {
        // In the C# this magic is .asf when fast and .mov when not.
        let asf = [0x30, 0x26, 0xB2, 0x75];
        assert_eq!(guess_extension(&asf, true), guess_extension(&asf, false));
    }

    #[test]
    fn unrecognised_input_is_none_not_a_fabricated_extension() {
        // The C# returns ".\u{1}\u{2}\u{3}" or similar here.
        assert_eq!(guess_extension(&[0x01, 0x02, 0x03, 0x04], false), None);
        assert_eq!(guess_extension(&[0xFF; 8], false), None);
    }

    #[test]
    fn short_buffers_are_rejected() {
        assert_eq!(guess_extension(&[], true), None);
        assert_eq!(guess_extension(&[0x50, 0x4B], true), None);
    }

    #[test]
    fn the_secondary_table_needs_eight_bytes() {
        // The C# guards this with `buf.Length < 8 ? 0U : ...`, so a 4-byte
        // buffer looks up magic 0 rather than stopping.
        let m = SECONDARY_MAGICS[0].0.to_le_bytes();
        let mut v = vec![0xAA, 0xAA, 0xAA, 0xAA];
        v.extend_from_slice(&m);
        assert_eq!(guess_extension(&v, false), Some(SECONDARY_MAGICS[0].1));
        assert_eq!(guess_extension(&v[..4], false), None, "no bytes 4..8");
    }

    #[test]
    fn booleans_and_nulls_do_not_abort_the_parse() {
        // The C# `_valueV` throws ArgumentOutOfRangeException for both.
        let mut m = HashMap::new();
        m.insert("on".to_string(), Value::Bool(true));
        m.insert("nil".to_string(), Value::Null);
        let v = Value::Object(m);
        assert!(v.bool_or("on", false));
        assert!(!v.bool_or("missing", false));
        assert_eq!(v.get("nil"), Some(&Value::Null));
    }

    #[test]
    fn scalars_are_promoted_to_lists() {
        let mut m = HashMap::new();
        m.insert("one".to_string(), Value::Str("a".into()));
        m.insert("many".to_string(), Value::Array(vec![
            Value::Str("a".into()), Value::Str("b".into()),
        ]));
        m.insert("num".to_string(), Value::Int(7));
        let v = Value::Object(m);
        assert_eq!(v.list("one"), Some(vec!["a".to_string()]));
        assert_eq!(v.list("many"), Some(vec!["a".to_string(), "b".to_string()]));
        assert_eq!(v.list("num"), Some(vec!["7".to_string()]));
        assert_eq!(v.list("absent"), None);
    }

    #[test]
    fn dict_trim_drops_nulls() {
        let mut m = HashMap::new();
        m.insert("keep".to_string(), Value::Int(1));
        m.insert("drop".to_string(), Value::Null);
        let out = Value::dict_trim(&m);
        assert_eq!(out.len(), 1);
        assert!(out.contains_key("keep"));
    }

    #[test]
    fn random_value_is_inclusive_of_high() {
        // Pins the C#'s Next(low, high + 1).
        let mut seq = [0u64, 1, 2].into_iter().cycle();
        let mut rng = || seq.next().unwrap();
        let vals: Vec<i32> = (0..6).map(|_| random_value(&mut rng, 0, 2)).collect();
        assert!(vals.contains(&2), "high must be reachable: {vals:?}");
        assert!(vals.iter().all(|v| (0..=2).contains(v)));
    }

    #[test]
    fn degenerate_random_ranges_do_not_divide_by_zero() {
        let mut rng = || 0u64;
        assert_eq!(random_value(&mut rng, 5, 5), 5);
        assert_eq!(random_value(&mut rng, 9, 3), 9);
    }
}
