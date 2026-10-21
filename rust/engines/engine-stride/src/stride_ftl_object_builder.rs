// PORT-SOURCE: Engines/GameX.Engine.Stride/StrideFtlObjectBuilder.cs
// PORT-SHA: 8bd24dae167cafce
// PORT-STATUS: partial
//
// Not ported: C# returns Stride.Engine.Entity named for the mesh. Rust returns the holder.
// An FTL kept as the shared mesh. Danae is Y-up, so positions stay as stored.
// materialManager may be null. Texture paths are not uploaded.

use std::any::Any;

use gamex_arkane::engines::ftl_object_builder;
use gamex_arkane::formats::danae::binary::Binary_Ftl;

use crate::stride_object_holder::StrideObjectHolder;

/// C# `StrideFtlObjectBuilder.BuildObject`.
pub fn buildObject(path: &dyn Any, isStatic: bool, materialManager: Option<&dyn Any>) -> StrideObjectHolder {
    let ftl = path.downcast_ref::<Binary_Ftl>().unwrap_or_else(|| panic!("InvalidCastException: Binary_Ftl"));
    let obj = &ftl.Obj;
    let name = if obj.File.is_empty() { "ftl".to_string() } else { obj.File.clone() };
    for texturePath in ftl_object_builder::getTexturePaths(obj) {
        if materialManager.is_some() { let _ = texturePath; }
    }
    let mesh = ftl_object_builder::toGeometry(obj);
    let mut materials = Vec::with_capacity(mesh.Submeshes.len());
    for sub in &mesh.Submeshes {
        let tex = obj.Textures.as_ref().and_then(|ts| {
            if sub.TexId >= 0 && (sub.TexId as usize) < ts.len() { Some(&ts[sub.TexId as usize]) } else { None }
        });
        materials.push(ftl_object_builder::toMaterialProp(tex));
    }
    StrideObjectHolder {
        Name: name,
        IsStatic: isStatic,
        Mesh: Box::new(mesh),
        Materials: materials,
        Groups: ftl_object_builder::groupPositions(obj),
    }
}
