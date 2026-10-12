// PORT-SOURCE: Core/GameX/Util.cs
// PORT-SHA: 8f7ab9954542a057
// PORT-STATUS: done
//
// JSON config helpers over `serde_json::Value` (C# `JsonElement`), and
// file-format sniffing. C# overloads get their own names: `_list<T>(elem, key,
// func)` is `_listF`, and the array form of `_related` is `_relatedA`.
//
// The 112-entry magic-number tables below were **extracted from the C# by
// script**, not transcribed.
//
// ===================== C#-SIDE BUGS ======================================
//
//   1. **The same magic gives two different answers.** `0x75B22630` maps to
//      `.asf` on the `fast` path and `.mov` on the slow path. That magic is the
//      ASF header GUID prefix, so `.asf` is the correct answer. **Fix in the C#.**
//
//   2. **The fallback builds an extension from raw bytes.** Both paths end in
//      `$".{Encoding.ASCII.GetString(buf.AsSpan(0, 3)).ToLowerInvariant()}"`,
//      so an unknown format is indistinguishable from a recognised one. Here it
//      is `None`.
//
//   3. **`_valueV` throws on booleans and nulls, and reads every number as
//      `Int32`.** Here they are `Tag::Bool`, `Tag::None` and `i64`.
//
//   4. **`_random` is an unsynchronised lazily-created `System.Random`.** The
//      caller passes the generator.
//
// Also: `_randomValue(low, high)` is **inclusive** of `high`
// (`_random.Next(low, high + 1)`), unlike every other range API in either
// language. Preserved, and asserted in a test.

use indexmap::IndexMap;
use serde_json::Value;

use crate::meta::Tag;

// value
pub fn _valueF<T>(elem: &Value, key: &str, func: impl FnOnce(&Value) -> T, default_: T) -> T {
    match elem.get(key) { Some(z) => func(z), None => default_ }
}
pub fn _value(elem: &Value, key: &str) -> Option<String> { elem.get(key).and_then(Value::as_str).map(str::to_string) }
pub fn _valueBool(elem: &Value, key: &str, default_: bool) -> bool { elem.get(key).and_then(Value::as_bool).unwrap_or(default_) }
pub fn _valueV(elem: &Value) -> Tag {
    match elem {
        Value::Number(z) => Tag::Int(z.as_i64().unwrap_or_default()),
        Value::String(z) => Tag::Str(z.clone()),
        Value::Array(z) => Tag::Strs(z.iter().map(|s| s.as_str().unwrap_or_default().to_string()).collect()),
        Value::Object(z) => Tag::Map(z.iter().map(|(k, v)| (k.clone(), _valueV(v))).collect()),
        Value::Bool(z) => Tag::Bool(*z),
        Value::Null => Tag::None,
    }
}

// list
pub fn _listF<T>(elem: &Value, key: &str, func: impl Fn(&str) -> T) -> Option<Vec<T>> { elem.get(key).map(|z| _listV(z).iter().map(|s| func(s)).collect()) }
pub fn _list(elem: &Value, key: &str) -> Option<Vec<String>> { elem.get(key).map(_listV) }
pub fn _listV(elem: &Value) -> Vec<String> {
    match elem {
        Value::Number(z) => vec![z.to_string()],
        Value::String(z) => vec![z.clone()],
        Value::Array(z) => z.iter().map(|s| s.as_str().unwrap_or_default().to_string()).collect(),
        _ => vec![],
    }
}

// related
pub fn _related<T>(elem: &Value, key: &str, mut func: impl FnMut(&str, &Value) -> T) -> IndexMap<String, T> {
    match elem.get(key).and_then(Value::as_object) { Some(z) => z.iter().map(|(k, v)| (k.clone(), func(k, v))).collect(), None => IndexMap::new() }
}
pub fn _relatedA<T>(elem: &Value, key: &str, keyFunc: impl Fn(&Value) -> String, valueFunc: impl Fn(&Value) -> T) -> IndexMap<String, T> {
    match elem.get(key).and_then(Value::as_array) { Some(z) => z.iter().map(|x| (keyFunc(x), valueFunc(x))).collect(), None => IndexMap::new() }
}
pub fn _dictTrim<T>(source: IndexMap<String, Option<T>>) -> IndexMap<String, T> {
    source.into_iter().filter_map(|(k, v)| v.map(|v| (k, v))).collect()
}

/// C# `_randomValue(int low, int high)`.
///
/// **Inclusive of `high`** — the C# calls `Next(low, high + 1)`. Preserved.
/// Takes its state by argument rather than using a shared unsynchronised
/// static (bug 4); the caller owns the generator.
pub fn _randomValue(rng: &mut impl FnMut() -> u64, low: i32, high: i32) -> i32 {
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
pub fn _guessExtension(buf: &[u8], fast: bool) -> Option<&'static str> {
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
    use serde_json::json;

    #[test]
    fn magicTablesAreTheExpectedSize() {
        // Guards the extraction: these counts come from the C# source.
        assert_eq!(FAST_MAGICS.len(), 1);
        assert_eq!(PRIMARY_MAGICS.len(), 77);
        assert_eq!(SECONDARY_MAGICS.len(), 34);
    }

    #[test]
    fn knownMagicsResolve() {
        assert_eq!(_guessExtension(&[0x50, 0x4B, 0x03, 0x04], false), Some(".zip"));
        assert_eq!(_guessExtension(&[0x30, 0x26, 0xB2, 0x75], true), Some(".asf"));
        assert_eq!(_guessExtension(&[0x01, 0x02, 0x03, 0x04], false), None);
        assert_eq!(_guessExtension(&[0x50, 0x4B], true), None);
    }

    #[test]
    fn scalarsPromoteToOneElementLists() {
        let v = json!({ "one": "a", "many": ["a", "b"], "num": 7 });
        assert_eq!(_list(&v, "one"), Some(vec!["a".to_string()]));
        assert_eq!(_list(&v, "many"), Some(vec!["a".to_string(), "b".to_string()]));
        assert_eq!(_list(&v, "num"), Some(vec!["7".to_string()]));
        assert_eq!(_list(&v, "absent"), None);
    }

    #[test]
    fn relatedKeepsJsonOrderAndDictTrimDropsNulls() {
        let v = json!({ "games": { "b": 1, "a": null } });
        let z = _related(&v, "games", |k, v| v.as_i64());
        assert_eq!(z.keys().collect::<Vec<_>>(), ["b", "a"]);
        assert_eq!(_dictTrim(z).len(), 1);
    }

    #[test]
    fn randomValueIsInclusiveOfHigh() {
        // Pins the C#'s Next(low, high + 1).
        let mut seq = [0u64, 1, 2].into_iter().cycle();
        let mut rng = || seq.next().unwrap();
        let vals: Vec<i32> = (0..6).map(|_| _randomValue(&mut rng, 0, 2)).collect();
        assert!(vals.contains(&2), "high must be reachable: {vals:?}");
        assert!(vals.iter().all(|v| (0..=2).contains(v)));
    }
}
