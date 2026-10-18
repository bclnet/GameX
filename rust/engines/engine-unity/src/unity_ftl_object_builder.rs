// PORT-SOURCE: Engines/GameX.Engine.Unity/UnityFtlObjectBuilder.cs
// PORT-SHA: 109b7c555d122d79
// PORT-STATUS: partial
//
// Not ported: UnityEngine (`Mesh`, `MeshFilter`, `MeshRenderer`, `GameObject`) is not in this crate.
// The shared mesh is `gamex_arkane::engines::ftl_object_builder` (`toGeometry`, `toMaterialProp`, `groupPositions`).
// Danae is Y-up, so the C# builder copies components and does not call `ToUnity`.
