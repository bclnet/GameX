// PORT-SOURCE: Engines/GameX.Engine.Ogre/OgreFtsObjectBuilder.cs
// PORT-SHA: e18a680a1c9b6a95
// PORT-STATUS: done
//
// An FTS map kept as one mesh. Danae is Y-up, so positions stay as stored.
// materialManager may be null. Texture paths are not uploaded.

use std::any::Any;

use gamex_arkane::engines::fts_object_builder;
use gamex_arkane::formats::danae::binary::Binary_Fts;

use crate::ogre_object_holder::OgreObjectHolder;

/// C# `OgreFtsObjectBuilder.BuildObject`.
pub fn buildObject(path: &dyn Any, isStatic: bool, materialManager: Option<&dyn Any>) -> OgreObjectHolder {
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
    OgreObjectHolder {
        Name: name,
        IsStatic: isStatic,
        Mesh: Box::new(mesh),
        Materials: materials,
        Groups: fts_object_builder::scenePositions(level),
    }
}
