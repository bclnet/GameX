// PORT-SOURCE: Engines/GameX.Engine.Wgpu/Wgpu_Render.cs
// PORT-STATUS: partial
//
// Ported: `createRenderer` for TestTri, Texture, Object, and the default.
// `Binary_Ftl` and `Binary_Fts` register on `WgpuX::BuildersByType`.
// OpenX has no `TestTriRenderer` or `TextureRenderer` body yet, so those arms are empty shells.
// Not ported: `Binary_Nif` (no Rust type), `ViewInfo`. No GPU upload.
// Builder fn is OpenX `Builder`: `fn(&(dyn Any + Send), bool, &(dyn Any + Send))`.

/// TestTriRenderer. No GPU upload.
pub struct TestTriRenderer;

impl TestTriRenderer {
    pub fn new() -> Self { Self }
    pub fn start(&mut self) {}
    pub fn update(&mut self, _deltaTime: f32) {}
}

/// TextureRenderer. No GPU upload.
pub struct TextureRenderer;

impl TextureRenderer {
    pub fn new() -> Self { Self }
    pub fn start(&mut self) {}
    pub fn update(&mut self, _deltaTime: f32) {}
}


use std::any::{Any, TypeId};
use std::sync::Once;

use glam::Vec3;
use openx_gfx::gfx::MaterialStd2Prop;

use gamex_arkane::engines::ftl_object_builder::{self, FtlMesh};
use gamex_arkane::engines::fts_object_builder::{self, FtsMesh};
use gamex_arkane::formats::danae::binary::{Binary_Ftl, Binary_Fts};
use openx_engine_wgpu::gfx::wgpu::{Builder, WgpuX};
fn mapLock() -> std::sync::MutexGuard<'static, std::collections::HashMap<TypeId, Builder>> {
    WgpuX::buildersByType()
}


/// Texture paths and one prop per submesh. Danae stays Y-up.
pub struct WgpuObjectMaterials {
    pub Paths: Vec<String>,
    pub Props: Vec<MaterialStd2Prop>,
}

pub enum WgpuMesh {
    Ftl(FtlMesh),
    Fts(FtsMesh),
}

/// Holder. Name, IsStatic, mesh, materials, groups. No GPU upload.
pub struct WgpuObject {
    pub Name: String,
    pub IsStatic: bool,
    pub Mesh: WgpuMesh,
    pub Materials: WgpuObjectMaterials,
    pub Groups: Vec<(String, Vec3)>,
}

fn buildFtl(path: &(dyn Any + Send), isStatic: bool, _materialManager: &(dyn Any + Send)) -> Option<Box<dyn Any + Send>> {
    let ftl = path.downcast_ref::<Binary_Ftl>()?;
    let obj = &ftl.Obj;
    let name = if obj.File.is_empty() { "ftl".to_string() } else { obj.File.clone() };
    let mesh = ftl_object_builder::toGeometry(obj);
    let paths = ftl_object_builder::getTexturePaths(obj);
    let mut props = Vec::with_capacity(mesh.Submeshes.len());
    for sub in &mesh.Submeshes {
        let tex = obj.Textures.as_ref().and_then(|ts| if sub.TexId >= 0 { ts.get(sub.TexId as usize) } else { None });
        props.push(ftl_object_builder::toMaterialProp(tex));
    }
    Some(Box::new(WgpuObject {
        Name: name,
        IsStatic: isStatic,
        Mesh: WgpuMesh::Ftl(mesh),
        Materials: WgpuObjectMaterials { Paths: paths, Props: props },
        Groups: ftl_object_builder::groupPositions(obj),
    }))
}

fn buildFts(path: &(dyn Any + Send), isStatic: bool, _materialManager: &(dyn Any + Send)) -> Option<Box<dyn Any + Send>> {
    let fts = path.downcast_ref::<Binary_Fts>()?;
    let level = &fts.Level;
    let mesh = fts_object_builder::toGeometry(level);
    let paths = fts_object_builder::getTexturePaths(level);
    let mut props = Vec::with_capacity(mesh.Submeshes.len());
    for sub in &mesh.Submeshes {
        let tex = level.Textures.as_ref().and_then(|ts| if sub.TexId >= 0 { ts.get(sub.TexId as usize) } else { None });
        props.push(fts_object_builder::toMaterialProp(tex, sub.Type));
    }
    Some(Box::new(WgpuObject {
        Name: "fts".to_string(),
        IsStatic: isStatic,
        Mesh: WgpuMesh::Fts(mesh),
        Materials: WgpuObjectMaterials { Paths: paths, Props: props },
        Groups: fts_object_builder::scenePositions(level),
    }))
}

fn register() {
    static ONCE: Once = Once::new();
    ONCE.call_once(|| {
        let mut map = mapLock();
        map.insert(TypeId::of::<Binary_Ftl>(), buildFtl as Builder);
        map.insert(TypeId::of::<Binary_Fts>(), buildFts as Builder);
    });
}

/// ObjectRenderer. `start` invokes the builder for `obj`'s type. An unknown type does nothing.
pub struct ObjectRenderer {
    obj: Option<Box<dyn Any + Send>>,
    pub Built: Option<Box<dyn Any + Send>>,
}

impl ObjectRenderer {
    pub fn new(obj: Option<Box<dyn Any + Send>>) -> Self { Self { obj, Built: None } }

    pub fn start(&mut self) {
        register();
        let Some(obj) = self.obj.as_deref() else { return };
        let id = obj.type_id();
        let build = {
            let map = mapLock();
            let Some(build) = map.get(&id).copied() else { return };
            build
        };
        let built = build(obj, true, &());
        self.Built = built;
    }

    pub fn update(&mut self, _deltaTime: f32) {}
}

/// Nearest equivalent of C# `Renderer` for the cases this port builds.
pub enum WgpuBuiltRenderer {
    TestTri(TestTriRenderer),
    Texture(TextureRenderer),
    Object(ObjectRenderer),
}

/// WgpuRenderer
pub struct WgpuRenderer;

impl WgpuRenderer {
    /// C# `CreateRenderer`. `obj` is the loaded object. TestTri and Texture ignore it.
    /// `Binary_Nif` is not registered: that type is not ported.
    pub fn createRenderer(type_: &str, obj: Option<Box<dyn Any + Send>>) -> Option<WgpuBuiltRenderer> {
        register();
        match type_ {
            "TestTri" => Some(WgpuBuiltRenderer::TestTri(TestTriRenderer::new())),
            "Texture" => Some(WgpuBuiltRenderer::Texture(TextureRenderer::new())),
            "Object" => Some(WgpuBuiltRenderer::Object(ObjectRenderer::new(obj))),
            _ => None,
        }
    }
}
