// PORT-SOURCE: Engines/GameX.Engine.Unreal/Unreal_Arkane.cs
// PORT-SHA: 87f5a3d78b334eaf
// PORT-STATUS: done
//
// An FTL kept as the shared mesh. Danae is Y-up, so positions stay as stored.
// materialManager may be null. Texture paths are not uploaded.
//
// An FTS map kept as one mesh. Danae is Y-up, so positions stay as stored.
// materialManager may be null. Texture paths are not uploaded.

pub mod unreal_ftl_object_builder {
    use std::any::Any;

    use gamex_arkane::engines::ftl_object_builder;
    use gamex_arkane::formats::danae::binary::Binary_Ftl;

    use crate::unreal_object_holder::UnrealObjectHolder;

    /// C# `UnrealFtlObjectBuilder.BuildObject`.
    pub fn buildObject(path: &dyn Any, isStatic: bool, materialManager: Option<&dyn Any>) -> UnrealObjectHolder {
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
        UnrealObjectHolder {
            Name: name,
            IsStatic: isStatic,
            Mesh: Box::new(mesh),
            Materials: materials,
            Groups: ftl_object_builder::groupPositions(obj),
        }
    }
}

pub mod unreal_fts_object_builder {
    use std::any::Any;

    use gamex_arkane::engines::fts_object_builder;
    use gamex_arkane::formats::danae::binary::Binary_Fts;

    use crate::unreal_object_holder::UnrealObjectHolder;

    /// C# `UnrealFtsObjectBuilder.BuildObject`.
    pub fn buildObject(path: &dyn Any, isStatic: bool, materialManager: Option<&dyn Any>) -> UnrealObjectHolder {
        let fts = path.downcast_ref::<Binary_Fts>().unwrap_or_else(|| panic!("InvalidCastException: Binary_Fts"));
        let level = &fts.Level;
        let name = "fts".to_string();
        for texturePath in fts_object_builder::getTexturePaths(level) {
            if materialManager.is_some() { let _ = texturePath; }
        }
        let mesh = fts_object_builder::toGeometry(level);
        let mut materials = Vec::with_capacity(mesh.Submeshes.len());
        for sub in &mesh.Submeshes {
            let tex = level.Textures.as_ref().and_then(|ts| {
                if sub.TexId >= 0 && (sub.TexId as usize) < ts.len() { Some(&ts[sub.TexId as usize]) } else { None }
            });
            materials.push(fts_object_builder::toMaterialProp(tex, sub.Type));
        }
        UnrealObjectHolder {
            Name: name,
            IsStatic: isStatic,
            Mesh: Box::new(mesh),
            Materials: materials,
            Groups: fts_object_builder::scenePositions(level),
        }
    }
}
