// PORT-SOURCE: Engines/GameX.Engine.Unity/Unity_Render.cs
// PORT-SHA: 11b1a2e1f72e1bc5
// PORT-STATUS: partial
//
// Ported: `createRenderer` for "Object" and C#'s default arm. That returns
// `UnityObjectHolder`. `Binary_Ftl` and `Binary_Fts` are registered on
// `BuildersByType` (`TypeId`) and build a summary (name, vertex count,
// submesh count, group names), not a Unity `GameObject`.
//
// Not ported: `Binary_Nif` does not exist in Rust, so it is not registered.
// UnityEngine (`ViewInfo`, `TestTriRenderer`, `TextureRenderer`, `EngineRenderer`,
// `ObjectRenderer`) is absent. OpenX's Rust `UnityX` has no `BuildersByType`,
// so the map lives here. `gfx == null` and `obj is IHaveSource` are not applied.

use std::any::{Any, TypeId};
use std::collections::HashMap;
use std::io::Error;
use std::sync::{Arc, LazyLock, Mutex, Once};

use gamex::meta::Tag;
use gamex_arkane::formats::danae::binary::{Binary_Ftl, Binary_Fts};

/// Stand-in for the mesh a Unity `GameObject` would carry.
#[derive(Clone, Debug, PartialEq)]
pub struct UnityObjectSummary {
    pub Name: String,
    pub VertexCount: usize,
    pub SubmeshCount: usize,
    pub GroupNames: Vec<String>,
}

/// C# `ObjectRenderer` is not ported. This holds the summary when a builder ran.
#[derive(Clone, Debug, PartialEq)]
pub struct UnityObjectHolder {
    pub TypeName: String,
    /// False when the value is not `Binary_Ftl` or `Binary_Fts`.
    pub GeometryAvailable: bool,
    pub Summary: Option<UnityObjectSummary>,
}

/// C# `Func<ISource, object, bool, MaterialManager<...>, Task<GameObject>>`.
/// `ISource`, `isStatic` and the material manager are dropped: there is no UnityEngine.
pub type BuilderByType = fn(&(dyn Any + Send + Sync)) -> Result<UnityObjectSummary, Error>;

fn builders() -> &'static Mutex<HashMap<TypeId, BuilderByType>> {
    static BuildersByType: LazyLock<Mutex<HashMap<TypeId, BuilderByType>>> = LazyLock::new(Default::default);
    &BuildersByType
}

/// C# `UnityX.BuildersByType`, keyed by the path object's type.
/// Filled on first use with `Binary_Ftl` and `Binary_Fts` only.
pub fn BuildersByType() -> &'static Mutex<HashMap<TypeId, BuilderByType>> {
    register();
    builders()
}

fn register() {
    static ONCE: Once = Once::new();
    ONCE.call_once(|| {
        let mut map = builders().lock().unwrap();
        map.insert(TypeId::of::<Binary_Ftl>(), crate::unity_arkane::unity_ftl_object_builder::buildObject);
        map.insert(TypeId::of::<Binary_Fts>(), crate::unity_arkane::unity_fts_object_builder::buildObject);
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

fn objectHolder(obj: &Tag) -> Result<UnityObjectHolder, Error> {
    let Some(any) = asPath(obj) else {
        return Ok(UnityObjectHolder { TypeName: "unknown".into(), GeometryAvailable: false, Summary: None });
    };
    let id = any.as_ref().type_id();
    let typeName = if id == TypeId::of::<Binary_Ftl>() { "Binary_Ftl" }
        else if id == TypeId::of::<Binary_Fts>() { "Binary_Fts" }
        else { "unknown" };
    let build = builders().lock().unwrap().get(&id).copied();
    let summary = match build {
        Some(build) => Some(build(any.as_ref())?),
        None => None,
    };
    Ok(UnityObjectHolder { TypeName: typeName.into(), GeometryAvailable: summary.is_some(), Summary: summary })
}

/// UnityRenderer
pub struct UnityRenderer;

impl UnityRenderer {
    /// C# `UnityRenderer.CreateRenderer`. "Object" and the default arm return a holder.
    /// "TestTri", "Texture", "VideoTexture" and "Engine" need UnityEngine and return `None`.
    pub fn createRenderer(obj: &Tag, type_: &str) -> Result<Option<UnityObjectHolder>, Error> {
        register();
        match type_ {
            "TestTri" | "Texture" | "VideoTexture" | "Engine" => Ok(None),
            _ => Ok(Some(objectHolder(obj)?)),
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
        let holder = UnityRenderer::createRenderer(&Tag::Info(info), "Object").unwrap().unwrap();
        assert_eq!(holder.TypeName, "Binary_Ftl");
        assert!(holder.GeometryAvailable);
        assert_eq!(holder.Summary.unwrap(), UnityObjectSummary {
            Name: "crate.ftl".into(),
            VertexCount: 3,
            SubmeshCount: 1,
            GroupNames: vec!["head".into(), "group".into()],
        });
        assert!(UnityRenderer::createRenderer(&Tag::None, "Texture").unwrap().is_none());
        assert!(UnityRenderer::createRenderer(&Tag::None, "Engine").unwrap().is_none());
        let unknown = UnityRenderer::createRenderer(&Tag::Any(Arc::new("nope".to_string())), "Object").unwrap().unwrap();
        assert_eq!(unknown.TypeName, "unknown");
        assert!(!unknown.GeometryAvailable);
        assert!(unknown.Summary.is_none());
    }
}
