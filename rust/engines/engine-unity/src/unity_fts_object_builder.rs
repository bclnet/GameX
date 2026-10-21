// PORT-SOURCE: Engines/GameX.Engine.Unity/UnityFtsObjectBuilder.cs
// PORT-SHA: 646757e91cf0f50c
// PORT-STATUS: partial
//
// Not ported: UnityEngine (`Mesh`, `MeshFilter`, `MeshRenderer`, `GameObject`) and materials.
// `buildObject` returns a summary from `gamex_arkane::engines::fts_object_builder`
// (`toGeometry`, `scenePositions`). Danae is Y-up, so components are copied and not swapped.
// Texture preload is skipped: there is no `TextureManager`. `isStatic` is not stored.
// The name is C#'s `"fts"`. Scene positions are the group names (`player`, `mscene`).

use std::any::Any;
use std::io::{Error, ErrorKind};

use gamex_arkane::engines::fts_object_builder;
use gamex_arkane::formats::danae::binary::Binary_Fts;

use crate::unity_render::UnityObjectSummary;

/// C# `UnityFtsObjectBuilder.BuildObject`, without a Unity `GameObject`.
pub fn buildObject(path: &(dyn Any + Send + Sync)) -> Result<UnityObjectSummary, Error> {
    let fts = path.downcast_ref::<Binary_Fts>().ok_or_else(|| Error::new(ErrorKind::InvalidInput, "InvalidCastException: Binary_Fts"))?;
    let level = &fts.Level;
    let data = fts_object_builder::toGeometry(level);
    let groups = fts_object_builder::scenePositions(level);
    Ok(UnityObjectSummary {
        Name: "fts".to_string(),
        VertexCount: data.Vertices.len(),
        SubmeshCount: data.Submeshes.len(),
        GroupNames: groups.into_iter().map(|(groupName, _)| groupName).collect(),
    })
}
