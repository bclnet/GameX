// PORT-SOURCE: Engines/GameX.Engine.Unity/UnityFtlObjectBuilder.cs
// PORT-SHA: 109b7c555d122d79
// PORT-STATUS: partial
//
// Not ported: UnityEngine (`Mesh`, `MeshFilter`, `MeshRenderer`, `GameObject`) and materials.
// `buildObject` returns a summary from `gamex_arkane::engines::ftl_object_builder`
// (`toGeometry`, `groupPositions`). Danae is Y-up, so components are copied and not swapped.
// Texture preload is skipped: there is no `TextureManager`. `isStatic` is not stored.

use std::any::Any;
use std::io::{Error, ErrorKind};

use gamex_arkane::engines::ftl_object_builder;
use gamex_arkane::formats::danae::binary::Binary_Ftl;

use crate::unity_render::UnityObjectSummary;

/// C# `UnityFtlObjectBuilder.BuildObject`, without a Unity `GameObject`.
pub fn buildObject(path: &(dyn Any + Send + Sync)) -> Result<UnityObjectSummary, Error> {
    let ftl = path.downcast_ref::<Binary_Ftl>().ok_or_else(|| Error::new(ErrorKind::InvalidInput, "InvalidCastException: Binary_Ftl"))?;
    let obj = &ftl.Obj;
    let name = if obj.File.is_empty() { "ftl".to_string() } else { obj.File.clone() };
    let data = ftl_object_builder::toGeometry(obj);
    let groups = ftl_object_builder::groupPositions(obj);
    Ok(UnityObjectSummary {
        Name: name,
        VertexCount: data.Vertices.len(),
        SubmeshCount: data.Submeshes.len(),
        GroupNames: groups.into_iter().map(|(groupName, _)| if groupName.is_empty() { "group".to_string() } else { groupName }).collect(),
    })
}
