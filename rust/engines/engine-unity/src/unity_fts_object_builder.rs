// PORT-SOURCE: Engines/GameX.Engine.Unity/UnityFtsObjectBuilder.cs
// PORT-SHA: 646757e91cf0f50c
// PORT-STATUS: partial
//
// Not ported: UnityEngine (`Mesh`, `MeshFilter`, `MeshRenderer`, `GameObject`) is not in this crate.
// The shared mesh is `gamex_arkane::engines::fts_object_builder` (`toGeometry`, `toMaterialProp`, `scenePositions`).
// Danae is Y-up, so the C# builder copies components and does not call `ToUnity`.
