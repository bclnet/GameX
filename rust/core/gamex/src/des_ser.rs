// PORT-SOURCE: Core/GameX/DesSer.cs
// PORT-SHA: 899d278ef9371987
// PORT-STATUS: done
//
// NOT PORTED — `System.Text.Json` configuration and 14 custom converters.
//
// The whole file is serializer setup: a `JsonSerializerOptions` with camelCase
// naming, relaxed escaping, `AllowNamedFloatingPointLiterals`, and a
// `TypeInfoResolver` whose one modifier alphabetises properties; plus
// converters for `Color3`, `ByteColor3`, `Color4`, `ByteColor4`, `float`,
// `Vector2`, `Vector3` and the rest.
//
// The Rust equivalent is `serde` with `#[serde(rename_all = "camelCase")]` and
// `impl Serialize`/`Deserialize` on the vector and colour types — declared
// where those types live, not gathered in one options object. So this file has
// no counterpart by design: its content becomes derives and impls spread across
// the types it configures.
//
// Two things to carry across when those impls are written, because both are
// deliberate and easy to lose:
//
//   1. **`AllowNamedFloatingPointLiterals`** means `NaN`, `Infinity` and
//      `-Infinity` are accepted and emitted as JSON *strings*. Serde rejects
//      those by default and writes `null` for non-finite floats, which would
//      silently corrupt any model file containing them — and model data does
//      contain them. A custom float serializer is needed.
//   2. **`AlphabetizeProperties`** sorts properties with
//      `StringComparer.Ordinal` so output is byte-stable across runs. Serde
//      emits fields in declaration order, which is also stable, but *different*
//      — so any committed golden file or checksum over serialized output will
//      not match unless the fields are declared alphabetically.
//
// One observation on the C#: `AlphabetizeProperties` clears
// `typeInfo.Properties` and re-adds them while assigning `Order = i`. Setting
// `Order` and controlling insertion sequence are two mechanisms for the same
// thing; if `Order` is ever honoured before the collection sequence, the two
// disagree. Harmless today, but it is doing the job twice.
//
// Kept as a file so the 1:1 mapping holds and `sync-check.sh` tracks drift.
