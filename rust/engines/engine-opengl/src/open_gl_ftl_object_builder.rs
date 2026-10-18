// PORT-SOURCE: Engines/GameX.Engine.OpenGL/OpenGLFtlObjectBuilder.cs
// PORT-SHA: ecdc2346ef0fc784
// PORT-STATUS: done
//
// An FTL kept as the shared mesh. Danae is Y-up, so positions stay as stored.
// OpenGL's material builder only accepts `MaterialShaderVProp`, so the props stay `MaterialStd2Prop`.
// `createMesh` is still `unimplemented!` on `OpenGLGfxApi`.

use std::io::{Error, ErrorKind};
use std::sync::Arc;

use glam::Vec3;
use openx_engine_opengl::engine_open_gl::OpenGLMaterialManager;
use openx_gfx::gfx::{AnyArc, MaterialStd2Prop};
use openx_poly::core::{BoxFuture, SourcePath, SourceRef};

use gamex_arkane::engines::ftl_object_builder::{self, FtlMesh};
use gamex_arkane::formats::danae::binary::Binary_Ftl;

/// C# `OpenGLFtlObject`.
#[derive(Clone)]
pub struct OpenGLFtlObject {
    pub Name: String,
    pub IsStatic: bool,
    pub Data: FtlMesh,
    pub Materials: Vec<MaterialStd2Prop>,
    pub Groups: Vec<(String, Vec3)>,
}

/// C# `OpenGLFtlObjectBuilder.BuildObject`.
pub fn buildObject<'a>(source: &'a SourceRef, path: &'a (dyn std::any::Any + Send + Sync), isStatic: bool, materialManager: &'a mut OpenGLMaterialManager) -> BoxFuture<'a, Result<AnyArc, Error>> {
    Box::pin(async move {
        let ftl = path.downcast_ref::<Binary_Ftl>().ok_or_else(|| Error::new(ErrorKind::InvalidInput, "InvalidCastException: Binary_Ftl"))?;
        let obj = &ftl.Obj;
        let name = if obj.File.is_empty() { "ftl".to_string() } else { obj.File.clone() };
        for texturePath in ftl_object_builder::getTexturePaths(obj) {
            materialManager.textureManager.preload(source, &SourcePath::from(texturePath));
        }
        let data = ftl_object_builder::toGeometry(obj);
        let mut materials = Vec::with_capacity(data.Submeshes.len());
        for sub in &data.Submeshes {
            let tex = obj.Textures.as_ref().and_then(|ts| {
                if sub.TexId >= 0 && (sub.TexId as usize) < ts.len() { Some(&ts[sub.TexId as usize]) } else { None }
            });
            materials.push(ftl_object_builder::toMaterialProp(tex));
        }
        let built = OpenGLFtlObject { Name: name, IsStatic: isStatic, Data: data, Materials: materials, Groups: ftl_object_builder::groupPositions(obj) };
        Ok(Arc::new(built) as AnyArc)
    })
}
