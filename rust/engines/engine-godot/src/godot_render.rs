// PORT-SOURCE: Engines/GameX.Engine.Godot/Godot_Render.cs
// PORT-SHA: 96b0e796ac8c89d9
// PORT-STATUS: partial
//
// Ported: `createRenderer` for "Object". That returns `GodotObjectHolder`.
// `Binary_Ftl` and `Binary_Fts` are registered on `BuildersByType` (`TypeId`).
// Each builder returns `GodotBuiltNode` (name, vertex count, submesh count, children):
// one child per submesh, then group or scene children. There is no Godot `Mesh`.
//
// Not ported: `Binary_Nif` does not exist in Rust, so it is not registered.
// `ViewInfo`, `TestTriRenderer`, `TextureRenderer` and `ObjectRenderer` are not in
// this crate (OpenX's Rust Godot gfx is a stub). OpenX rust godot has no
// `BuildersByType`, so the map lives here. `obj is IHaveSource` is not applied.
//
// PARTIAL PORT: the C# static constructor also registers `Binary_Nif`. Rust does not.

use std::any::{Any, TypeId};
use std::collections::HashMap;
use std::io::Error;
use std::sync::{Arc, LazyLock, Mutex, Once};

use gamex::meta::Tag;
use gamex_arkane::formats::danae::binary::{Binary_Ftl, Binary_Fts};

/// Stand-in for a Godot `Node3D` tree. Mesh children repeat the shared vertex count.
/// Danae positions are not stored: there is no `Node3D`.
#[derive(Clone, Debug, PartialEq)]
pub struct GodotBuiltNode {
    pub Name: String,
    pub VertexCount: usize,
    pub SubmeshCount: usize,
    pub Children: Vec<GodotBuiltNode>,
}

/// C# `ObjectRenderer` is not ported (`GodotGfxModel` is not in the Rust Godot crate).
/// This holds the built node when a builder ran.
#[derive(Clone, Debug, PartialEq)]
pub struct GodotObjectHolder {
    pub TypeName: String,
    /// False when the value is not `Binary_Ftl` or `Binary_Fts`.
    pub GeometryAvailable: bool,
    pub Node: Option<GodotBuiltNode>,
}

/// C# `Func<ISource, object, bool, MaterialManager<Material, Texture2D>, Task<Node>>`.
/// `ISource`, `isStatic` and the material manager are dropped: there is no Godot mesh type.
pub type BuilderByType = fn(&(dyn Any + Send + Sync)) -> Result<GodotBuiltNode, Error>;

fn builders() -> &'static Mutex<HashMap<TypeId, BuilderByType>> {
    static BuildersByType: LazyLock<Mutex<HashMap<TypeId, BuilderByType>>> = LazyLock::new(Default::default);
    &BuildersByType
}

/// C# `GodotX.BuildersByType`, keyed by the path object's type.
/// Filled on first use with `Binary_Ftl` and `Binary_Fts` only.
pub fn BuildersByType() -> &'static Mutex<HashMap<TypeId, BuilderByType>> {
    register();
    builders()
}

fn register() {
    static ONCE: Once = Once::new();
    ONCE.call_once(|| {
        let mut map = builders().lock().unwrap();
        map.insert(TypeId::of::<Binary_Ftl>(), crate::godot_ftl_object_builder::buildObject);
        map.insert(TypeId::of::<Binary_Fts>(), crate::godot_fts_object_builder::buildObject);
    });
}

fn asPath(obj: &Tag) -> Option<Arc<dyn Any + Send + Sync>> {
    match obj {
        Tag::Any(z) => Some(z.clone()),
        Tag::Info(z) => {
            let z = z.clone();
            let obj: Arc<dyn Any + Send + Sync> = z;
            Some(obj)
        }
        _ => None,
    }
}

fn objectHolder(obj: &Tag) -> Result<GodotObjectHolder, Error> {
    let Some(any) = asPath(obj) else {
        return Ok(GodotObjectHolder { TypeName: "unknown".into(), GeometryAvailable: false, Node: None });
    };
    let id = any.as_ref().type_id();
    let typeName = if id == TypeId::of::<Binary_Ftl>() { "Binary_Ftl" }
        else if id == TypeId::of::<Binary_Fts>() { "Binary_Fts" }
        else { "unknown" };
    let build = builders().lock().unwrap().get(&id).copied();
    let node = match build {
        Some(build) => Some(build(any.as_ref())?),
        None => None,
    };
    Ok(GodotObjectHolder { TypeName: typeName.into(), GeometryAvailable: node.is_some(), Node: node })
}

/// GodotRenderer
pub struct GodotRenderer;

impl GodotRenderer {
    /// C# `GodotRenderer.CreateRenderer`. "Object" returns a holder.
    /// "TestTri" and "Texture" need the OpenX Godot renderers and return `None`.
    /// Any other type is C#'s `default` (`null`).
    pub fn createRenderer(obj: &Tag, type_: &str) -> Result<Option<GodotObjectHolder>, Error> {
        register();
        match type_ {
            "Object" => Ok(Some(objectHolder(obj)?)),
            _ => Ok(None),
        }
    }
}

#[cfg(test)]
mod tests {
    use std::sync::Arc;

    use gamex::meta::{IHaveMetaInfo, Tag};
    use gamex_arkane::formats::danae::binary::Binary_Ftl;
    use gamex_arkane::formats::danae::eerie_types::{E_3DOBJ, E_FACE, E_GROUPLIST, E_TEXTURE, E_VERTEX};
    use glam::{U16Vec3, Vec3};

    use super::*;

    fn vert(x: f32, y: f32, z: f32) -> E_VERTEX {
        E_VERTEX { V: Vec3::new(x, y, z), ..Default::default() }
    }

    fn sample() -> Binary_Ftl {
        Binary_Ftl { Obj: E_3DOBJ {
            File: "crate.ftl".into(),
            Vertexs: Some(vec![vert(0.0, 0.0, 0.0), vert(1.0, 0.0, 0.0), vert(0.0, 1.0, 0.0)]),
            Textures: Some(vec![E_TEXTURE { Path: "a.tex".into(), ..Default::default() }]),
            Faces: Some(vec![E_FACE { FaceType: 0, TexId: 0, Vid: U16Vec3::new(0, 1, 2), ..Default::default() }]),
            Groups: Some(vec![
                E_GROUPLIST { Name: "head".into(), Origin: 1, ..Default::default() },
                E_GROUPLIST { Name: String::new(), Origin: 0, ..Default::default() },
            ]),
            ..Default::default()
        } }
    }

    #[test]
    fn registersFtlAndFtsOnly() {
        let map = BuildersByType().lock().unwrap();
        assert!(map.contains_key(&TypeId::of::<Binary_Ftl>()));
        assert!(map.contains_key(&TypeId::of::<Binary_Fts>()));
        assert_eq!(map.len(), 2);
    }

    #[test]
    fn objectUsesSharedFtlGeometry() {
        let info: Arc<dyn IHaveMetaInfo> = Arc::new(sample());
        let holder = GodotRenderer::createRenderer(&Tag::Info(info), "Object").unwrap().unwrap();
        assert_eq!(holder.TypeName, "Binary_Ftl");
        assert!(holder.GeometryAvailable);
        assert_eq!(holder.Node.unwrap(), GodotBuiltNode {
            Name: "crate.ftl".into(),
            VertexCount: 3,
            SubmeshCount: 1,
            Children: vec![
                GodotBuiltNode { Name: "tex0".into(), VertexCount: 3, SubmeshCount: 1, Children: vec![] },
                GodotBuiltNode { Name: "head".into(), VertexCount: 0, SubmeshCount: 0, Children: vec![] },
                GodotBuiltNode { Name: "group".into(), VertexCount: 0, SubmeshCount: 0, Children: vec![] },
            ],
        });
        assert!(GodotRenderer::createRenderer(&Tag::None, "Texture").unwrap().is_none());
        assert!(GodotRenderer::createRenderer(&Tag::None, "TestTri").unwrap().is_none());
        let unknown = GodotRenderer::createRenderer(&Tag::Any(Arc::new("nope".to_string())), "Object").unwrap().unwrap();
        assert_eq!(unknown.TypeName, "unknown");
        assert!(!unknown.GeometryAvailable);
        assert!(unknown.Node.is_none());
    }
}
