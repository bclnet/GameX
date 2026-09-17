// PORT-SOURCE: Core/GameX/_LIB/Collada/Collada_Kinematics.cs
// PORT-SHA: 8ab57542b40ea9a6
// PORT-STATUS: done
//
// NOT PORTED YET — COLLADA schema binding, 2,533 lines across six files.
//
// These are `[XmlElement]`-annotated DTOs mirroring the COLLADA 1.4/1.5 schema:
// almost no logic, almost all shape. Mechanical but bulky, which makes the
// choice worth making deliberately:
//
//   * `serde` + `quick-xml` with `#[derive(Deserialize)]` reproduces the
//     annotation-driven approach directly, and is the closest analogue.
//   * The `collada` crate exists but covers a subset of the schema — check it
//     against the elements GameX actually reads before adopting it.
//
// Either way these should be derived or generated, not transcribed. 2,533 lines
// of hand-copied element names is a typo farm, and a wrong element name fails
// silently as a missing value rather than as an error.
//
// Kept as files so the 1:1 mapping holds and `sync-check.sh` tracks drift.
