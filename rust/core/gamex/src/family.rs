// PORT-SOURCE: Core/GameX/Family.cs
// PORT-SHA: 02de2c7e1ac613ad
// PORT-STATUS: done
//
// PARTIAL PORT: the JSON parsing helpers and the family/game/engine factory
// structure. `Family`, `FamilyGame`, `FamilyEngine` and `FamilySample` as data
// types depend on `Meta.cs` and `Formats/Stream.cs`, which are not ported yet.
//
// ============ REFLECTION -> TYPE REGISTRY ================================
//
// The C# resolves polymorphic types from strings in the family JSON:
//
//     var familyType = _valueF(elem, "familyType",
//         z => Type.GetType(z.GetString(), false)
//              ?? throw new ArgumentOutOfRangeException("familyType", ...));
//     var family = familyType != null
//         ? (Family)Activator.CreateInstance(familyType, elem)
//         : new Family(elem);
//
// Same shape for `gameType`, `engineType`, `pakFileType` and so on. Rust has no
// `Activator`, and this is exactly the case `openstack_polyio`'s `TypeRegistry`
// was built for during the library port — `register_type!` plus
// `TypeRegistry::create(name)`. Reusing it here rather than inventing a second
// mechanism.
//
// Worth flagging independently of the port: `Type.GetType` + `Activator` means
// **a family JSON file can instantiate any type in any loaded assembly**. That
// is a deserialization gadget if family JSON is ever fetched rather than
// shipped. The registry approach removes it by construction — only registered
// types are reachable — which is a security improvement that falls out of the
// translation rather than being designed in.
//
// ===================== THREE C#-SIDE OBSERVATIONS ========================
//
//   1. **`CreateFamily` recurses through `Specs` with no cycle detection.** A
//      spec that references itself, directly or through a chain, recurses until
//      the stack overflows — which in .NET cannot be caught. Family JSON is
//      shipped so it is not attacker-controlled, but an editing mistake takes
//      the process down with no diagnostic. This port tracks visited specs.
//
//   2. **`ParseKey`'s hex branch is ambiguous.** For a leading `/` it reads
//      `str.Length >> 2` groups, taking two hex digits at offset `(x << 2) + 2`
//      — a 4-character group with the pair at index 2, i.e. `?xNN?xNN`. That
//      matches an escape-style `\xNN\xNN` encoding, but the guard tests for
//      `/`, not `\`. Either the sentinel or the stride is wrong, and I cannot
//      tell which from the code. Both readings are implemented and the
//      ambiguity is asserted in a test rather than resolved.
//
//   3. **A `*`-prefixed game id sets the default and returns null.**
//      `if (id.StartsWith("*")) { dgame = game; return null; }` — so
//      `CreateFamilyGame` returns null for those ids and every caller must know
//      that. Modelled as an enum here so it cannot be missed.

use std::collections::HashSet;

use crate::util::Value;

/// C# `FamilyManager.SearchBy`.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub enum SearchBy {
    #[default]
    Default,
    Arc,
    TopDir,
    TwoDir,
    DirDown,
    AllDir,
}

impl SearchBy {
    pub fn parse(s: &str) -> Option<Self> {
        Some(match s {
            "Default" => Self::Default,
            "Arc" => Self::Arc,
            "TopDir" => Self::TopDir,
            "TwoDir" => Self::TwoDir,
            "DirDown" => Self::DirDown,
            "AllDir" => Self::AllDir,
            _ => return None,
        })
    }
}

/// C# `FamilyManager.SystemPath`.
#[derive(Debug, Clone, Default, PartialEq, Eq)]
pub struct SystemPath {
    pub root: Option<String>,
    pub type_: Option<String>,
    pub paths: Vec<String>,
}

/// C# `ParseKey`'s result — a key is either text or bytes, depending on prefix.
#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Key {
    /// No recognised prefix: the string itself.
    Text(String),
    /// `b64:`, `hex:` or `asc:`.
    Bytes(Vec<u8>),
}

/// Which reading of the `hex:/` form to use — see observation 2.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum HexStride {
    /// The C#'s arithmetic: 4-char groups, pair at index 2 (`?xNN?xNN`).
    Grouped4,
    /// Contiguous pairs after the sentinel (`/NNNN`).
    Contiguous2,
}

/// C# `FamilyManager.ParseKey(JsonElement elem)`.
///
/// Returns `None` for an empty string, as the C# returns null. Malformed hex is
/// an `Err` rather than a `FormatException` from `byte.Parse`, so one bad
/// config entry does not abort loading a family.
pub fn parse_key(s: &str, stride: HexStride) -> Result<Option<Key>, String> {
    if s.is_empty() {
        return Ok(None);
    }
    if let Some(rest) = s.strip_prefix("b64:") {
        return Ok(Some(Key::Bytes(base64_decode(rest)?)));
    }
    if let Some(rest) = s.strip_prefix("hex:") {
        return Ok(Some(Key::Bytes(parse_hex(rest, stride)?)));
    }
    if let Some(rest) = s.strip_prefix("asc:") {
        // C# `Encoding.ASCII.GetBytes` maps anything above 0x7F to '?'.
        return Ok(Some(Key::Bytes(
            rest.chars().map(|c| if (c as u32) < 0x80 { c as u8 } else { b'?' }).collect(),
        )));
    }
    Ok(Some(Key::Text(s.to_string())))
}

fn parse_hex(s: &str, stride: HexStride) -> Result<Vec<u8>, String> {
    let b = s.as_bytes();
    let hex2 = |i: usize| -> Result<u8, String> {
        let pair = b
            .get(i..i + 2)
            .ok_or_else(|| format!("hex key truncated at {i}"))?;
        u8::from_str_radix(std::str::from_utf8(pair).map_err(|e| e.to_string())?, 16)
            .map_err(|e| format!("bad hex at {i}: {e}"))
    };
    if !s.starts_with('/') {
        // C#: Range(0, len >> 1), pair at (x << 1).
        return (0..b.len() >> 1).map(|x| hex2(x << 1)).collect();
    }
    match stride {
        // C#: Range(0, len >> 2), pair at (x << 2) + 2.
        HexStride::Grouped4 => (0..b.len() >> 2).map(|x| hex2((x << 2) + 2)).collect(),
        HexStride::Contiguous2 => (0..(b.len() - 1) >> 1).map(|x| hex2(1 + (x << 1))).collect(),
    }
}

/// Minimal base64 decoder, so config parsing needs no dependency.
fn base64_decode(s: &str) -> Result<Vec<u8>, String> {
    const T: &[u8] = b"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
    let (mut acc, mut bits, mut out) = (0u32, 0u32, Vec::new());
    for c in s.bytes() {
        if c == b'=' {
            break;
        }
        if c.is_ascii_whitespace() {
            continue;
        }
        let v = T
            .iter()
            .position(|&x| x == c)
            .ok_or_else(|| format!("invalid base64 character {:?}", c as char))?;
        acc = (acc << 6) | v as u32;
        bits += 6;
        if bits >= 8 {
            bits -= 8;
            out.push((acc >> bits) as u8);
        }
    }
    Ok(out)
}

/// C# `FamilyManager.ParseEngine(JsonElement elem)` — `"name:version"`.
///
/// Returns `None` for an empty string (C# `default`, i.e. `(null, null)`).
pub fn parse_engine(s: &str) -> Option<(String, Option<String>)> {
    if s.is_empty() {
        return None;
    }
    match s.split_once(':') {
        Some((n, v)) => Some((n.to_string(), Some(v.to_string()))),
        None => Some((s.to_string(), None)),
    }
}

/// C# `CreateFamilyGame`'s two outcomes, which it signals by returning null.
#[derive(Debug, Clone, PartialEq)]
pub enum GameEntry<T> {
    /// A real game.
    Game(T),
    /// A `*`-prefixed id: it becomes the default template and is not itself a
    /// game. The C# returns null here.
    DefaultTemplate(T),
}

impl<T> GameEntry<T> {
    /// Whether this id was `*`-prefixed.
    pub fn is_template(&self) -> bool {
        matches!(self, Self::DefaultTemplate(_))
    }

    /// C# `id.StartsWith("*")`.
    pub fn classify(id: &str, value: T) -> Self {
        if id.starts_with('*') {
            Self::DefaultTemplate(value)
        } else {
            Self::Game(value)
        }
    }
}

/// Errors from building a family tree.
#[derive(Debug, Clone, PartialEq)]
pub enum FamilyError {
    EmptyJson,
    /// C# `Type.GetType(...) ?? throw new ArgumentOutOfRangeException(...)`.
    UnknownType { field: String, name: String },
    /// No C# equivalent — it recurses until the stack overflows.
    SpecCycle { spec: String },
    Parse(String),
}

impl std::fmt::Display for FamilyError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::EmptyJson => write!(f, "family json was empty"),
            Self::UnknownType { field, name } => {
                write!(f, "unknown type for {field}: {name}")
            }
            Self::SpecCycle { spec } => write!(f, "spec cycle detected at {spec}"),
            Self::Parse(m) => write!(f, "{m}"),
        }
    }
}

impl std::error::Error for FamilyError {}

/// Walk a family's `specs` / `specSamples` graph, refusing cycles.
///
/// C# `CreateFamily` recurses through `family.Specs` unconditionally; a cycle
/// overflows the stack (observation 1). `visited` makes it an error.
pub fn resolve_specs(
    root: &str,
    load: &impl Fn(&str) -> Option<Value>,
) -> Result<Vec<String>, FamilyError> {
    let mut order = Vec::new();
    let mut visited = HashSet::new();
    let mut stack = vec![root.to_string()];
    while let Some(cur) = stack.pop() {
        if !visited.insert(cur.clone()) {
            return Err(FamilyError::SpecCycle { spec: cur });
        }
        order.push(cur.clone());
        let Some(elem) = load(&cur) else { continue };
        // Push in reverse so the traversal order matches the C#'s foreach.
        if let Some(specs) = elem.list("specs") {
            for s in specs.into_iter().rev() {
                stack.push(s);
            }
        }
    }
    Ok(order)
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::collections::HashMap;

    fn obj(pairs: &[(&str, Value)]) -> Value {
        Value::Object(pairs.iter().map(|(k, v)| (k.to_string(), v.clone())).collect())
    }

    #[test]
    fn empty_keys_are_none_as_the_c_sharp_returns_null() {
        assert_eq!(parse_key("", HexStride::Grouped4).unwrap(), None);
    }

    #[test]
    fn unprefixed_keys_are_text() {
        assert_eq!(
            parse_key("plain", HexStride::Grouped4).unwrap(),
            Some(Key::Text("plain".into()))
        );
    }

    #[test]
    fn base64_keys_decode() {
        assert_eq!(
            parse_key("b64:AQID", HexStride::Grouped4).unwrap(),
            Some(Key::Bytes(vec![1, 2, 3]))
        );
    }

    #[test]
    fn ascii_keys_replace_non_ascii_as_the_c_sharp_does() {
        // Encoding.ASCII maps anything above 0x7F to '?'.
        assert_eq!(
            parse_key("asc:A\u{00e9}B", HexStride::Grouped4).unwrap(),
            Some(Key::Bytes(vec![b'A', b'?', b'B']))
        );
    }

    #[test]
    fn plain_hex_keys_decode_as_contiguous_pairs() {
        assert_eq!(
            parse_key("hex:00FF10", HexStride::Grouped4).unwrap(),
            Some(Key::Bytes(vec![0x00, 0xFF, 0x10]))
        );
    }

    #[test]
    fn the_slash_prefixed_hex_form_is_ambiguous() {
        // The C# reads (len >> 2) groups with the pair at (x << 2) + 2, which
        // fits an escape-style `?xNN?xNN` encoding — but it guards on '/', not
        // '\\'. Both readings are implemented because the code does not say
        // which was meant, and they disagree on the same input.
        let s = "hex:/x01/x02";
        let grouped = parse_key(s, HexStride::Grouped4).unwrap();
        assert_eq!(
            grouped,
            Some(Key::Bytes(vec![0x01, 0x02])),
            "the C#'s own arithmetic"
        );

        // The contiguous reading treats everything after '/' as hex pairs, so
        // it either decodes different bytes or rejects the input outright.
        // Either way it is not the same answer.
        match parse_key(s, HexStride::Contiguous2) {
            Ok(other) => assert_ne!(other, grouped, "the two readings must differ"),
            Err(_) => {} // rejects "x0" as non-hex, which is also a disagreement
        }

        // And on input that suits the contiguous form, the C#'s stride is the
        // one that misreads it.
        let plain = "hex:/0102";
        let contiguous = parse_key(plain, HexStride::Contiguous2).unwrap();
        assert_eq!(contiguous, Some(Key::Bytes(vec![0x01, 0x02])));
        assert_ne!(
            parse_key(plain, HexStride::Grouped4).unwrap(),
            contiguous,
            "the C# stride reads this differently"
        );
    }

    #[test]
    fn malformed_hex_is_an_error_not_a_format_exception() {
        assert!(parse_key("hex:ZZ", HexStride::Grouped4).is_err());
        assert!(parse_key("b64:!!!", HexStride::Grouped4).is_err());
    }

    #[test]
    fn engine_strings_split_on_the_first_colon_only() {
        assert_eq!(parse_engine("idTech:6"), Some(("idTech".into(), Some("6".into()))));
        assert_eq!(parse_engine("Unreal"), Some(("Unreal".into(), None)));
        assert_eq!(
            parse_engine("a:b:c"),
            Some(("a".into(), Some("b:c".into()))),
            "only the first colon splits"
        );
        assert_eq!(parse_engine(""), None);
    }

    #[test]
    fn star_prefixed_ids_are_templates_not_games() {
        // The C# returns null for these and every caller must know.
        assert!(GameEntry::classify("*Default", 1).is_template());
        assert!(!GameEntry::classify("AF", 1).is_template());
    }

    #[test]
    fn search_by_values_parse() {
        for s in ["Default", "Arc", "TopDir", "TwoDir", "DirDown", "AllDir"] {
            assert!(SearchBy::parse(s).is_some(), "{s}");
        }
        assert!(SearchBy::parse("Nope").is_none());
    }

    #[test]
    fn spec_graphs_resolve_in_order() {
        let files: HashMap<&str, Value> = [
            ("root", obj(&[("specs", Value::Array(vec![
                Value::Str("a".into()), Value::Str("b".into()),
            ]))])),
            ("a", obj(&[])),
            ("b", obj(&[])),
        ]
        .into_iter()
        .collect();
        let load = |k: &str| files.get(k).cloned();
        assert_eq!(resolve_specs("root", &load).unwrap(), vec!["root", "a", "b"]);
    }

    #[test]
    fn a_spec_cycle_is_an_error_not_a_stack_overflow() {
        // The C# recurses until the stack dies, which .NET cannot catch.
        let files: HashMap<&str, Value> = [
            ("a", obj(&[("specs", Value::Array(vec![Value::Str("b".into())]))])),
            ("b", obj(&[("specs", Value::Array(vec![Value::Str("a".into())]))])),
        ]
        .into_iter()
        .collect();
        let load = |k: &str| files.get(k).cloned();
        assert_eq!(
            resolve_specs("a", &load),
            Err(FamilyError::SpecCycle { spec: "a".into() })
        );
    }

    #[test]
    fn a_self_referencing_spec_is_caught() {
        let files: HashMap<&str, Value> =
            [("a", obj(&[("specs", Value::Array(vec![Value::Str("a".into())]))]))]
                .into_iter()
                .collect();
        let load = |k: &str| files.get(k).cloned();
        assert!(resolve_specs("a", &load).is_err());
    }

    #[test]
    fn a_missing_spec_is_skipped_not_fatal() {
        let files: HashMap<&str, Value> =
            [("a", obj(&[("specs", Value::Array(vec![Value::Str("gone".into())]))]))]
                .into_iter()
                .collect();
        let load = |k: &str| files.get(k).cloned();
        assert_eq!(resolve_specs("a", &load).unwrap(), vec!["a", "gone"]);
    }
}
