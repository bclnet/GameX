// PORT-SOURCE: Engines/GameX.Engine.Godot/Godot_Arkane.cs
// PORT-SHA: 2326527b9db8e517
// PORT-STATUS: partial
//
// Not ported: Godot `Node3D`, `MeshInstance3D` and `ArrayMesh` are not in this crate.
// `buildObject` returns `GodotBuiltNode`: one child per submesh (`tex{id}`, shared vertex
// count), then one child per `groupPositions` name. Danae is Y-up, so C# copies
// components and does not swap axes; Rust stores names only, not positions.
// Texture preload and materials are skipped: there is no Godot `MaterialManager`.
// `isStatic` is not stored.
//
// Not ported: Godot `Node3D`, `MeshInstance3D` and `ArrayMesh` are not in this crate.
// `buildObject` returns `GodotBuiltNode`: one child per submesh (`tex{id}`, shared vertex
// count), then one child per `scenePositions` name (`player`, `mscene`). Danae is Y-up,
// so C# copies components and does not swap axes; Rust stores names only, not positions.
// Texture preload and materials are skipped: there is no Godot `MaterialManager`.
// `isStatic` is not stored. The name is C#'s `"fts"`.

pub mod godot_ftl_object_builder {
    use std::any::Any;
    use std::io::{Error, ErrorKind};

    use gamex_arkane::engines::ftl_object_builder;
    use gamex_arkane::formats::danae::binary::Binary_Ftl;

    use crate::godot_render::GodotBuiltNode;

    /// C# `GodotFtlObjectBuilder.BuildObject`, without a Godot `Node`.
    pub fn buildObject(path: &(dyn Any + Send + Sync)) -> Result<GodotBuiltNode, Error> {
        let ftl = path.downcast_ref::<Binary_Ftl>().ok_or_else(|| Error::new(ErrorKind::InvalidInput, "InvalidCastException: Binary_Ftl"))?;
        let obj = &ftl.Obj;
        let name = if obj.File.is_empty() { "ftl".to_string() } else { obj.File.clone() };
        let data = ftl_object_builder::toGeometry(obj);
        let vertexCount = data.Vertices.len();
        let mut children = Vec::with_capacity(data.Submeshes.len());
        for sub in &data.Submeshes {
            children.push(GodotBuiltNode { Name: format!("tex{}", sub.TexId), VertexCount: vertexCount, SubmeshCount: 1, Children: Vec::new() });
        }
        for (groupName, _) in ftl_object_builder::groupPositions(obj) {
            let name = if groupName.is_empty() { "group".to_string() } else { groupName };
            children.push(GodotBuiltNode { Name: name, VertexCount: 0, SubmeshCount: 0, Children: Vec::new() });
        }
        Ok(GodotBuiltNode { Name: name, VertexCount: vertexCount, SubmeshCount: data.Submeshes.len(), Children: children })
    }
}

pub mod godot_fts_object_builder {
    use std::any::Any;
    use std::io::{Error, ErrorKind};

    use gamex_arkane::engines::fts_object_builder;
    use gamex_arkane::formats::danae::binary::Binary_Fts;

    use crate::godot_render::GodotBuiltNode;

    /// C# `GodotFtsObjectBuilder.BuildObject`, without a Godot `Node`.
    pub fn buildObject(path: &(dyn Any + Send + Sync)) -> Result<GodotBuiltNode, Error> {
        let fts = path.downcast_ref::<Binary_Fts>().ok_or_else(|| Error::new(ErrorKind::InvalidInput, "InvalidCastException: Binary_Fts"))?;
        let level = &fts.Level;
        let data = fts_object_builder::toGeometry(level);
        let vertexCount = data.Vertices.len();
        let mut children = Vec::with_capacity(data.Submeshes.len() + 2);
        for sub in &data.Submeshes {
            children.push(GodotBuiltNode { Name: format!("tex{}", sub.TexId), VertexCount: vertexCount, SubmeshCount: 1, Children: Vec::new() });
        }
        for (groupName, _) in fts_object_builder::scenePositions(level) {
            children.push(GodotBuiltNode { Name: groupName, VertexCount: 0, SubmeshCount: 0, Children: Vec::new() });
        }
        Ok(GodotBuiltNode { Name: "fts".to_string(), VertexCount: vertexCount, SubmeshCount: data.Submeshes.len(), Children: children })
    }
}
