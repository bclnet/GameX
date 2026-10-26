// PORT-SOURCE: Engines/GameX.Engine.OpenGL/OpenGL_Render.cs
// PORT-SHA: 85e7fd691556ba6d
// PORT-STATUS: partial
//
// PARTIAL PORT: `Binary_Ftl` and `Binary_Fts` are registered on the first
// `createRenderer` (`open_gl_arkane::open_gl_ftl_object_builder`, `open_gl_arkane::open_gl_fts_object_builder`).
// FTS is registered. `Binary_Nif` stays unregistered: Gamebryo's Nif is
// unported (`OpenGL_Gamebryo.cs`), so an "Object" view of a NIF still
// has no builder. The file is not `done` while NIF is unregistered.
// "Particle" builds the outer `ParticleRenderer`, which draws nothing.
// "Engine" needs the value to be an `Arc<dyn CellDatabase>` inside the `Any`.
// `Binary_Engine` supplies that when the query archive implements `IQueryFunc`.
// Bethesda's `Binary_Esm` is that implementor and is still unported.
//
// C#'s `object obj` is the MetaContent's `Tag`: "Texture" takes a
// `Tag::Texture`, "Material" a `Tag::Any` holding a `MaterialProp`, and
// `obj is IHaveSource z` has no Rust form yet.

use std::any::{Any, TypeId};
use std::io::{Error, ErrorKind};
use std::sync::{Arc, Once};
use gamex::meta::Tag;
use gamex_arkane::formats::danae::binary::{Binary_Ftl, Binary_Fts};
use openx_engine_opengl::gfx::open_gl::{setObjectDraw, OpenGLX};
use openx_engine_opengl::gfx::open_gl_render::{drawPositions, EngineRenderer, MaterialRenderer, ObjectRenderer, ParticleRenderer, TestTriRenderer, TextureRenderer};
use openx_gfx::gfx::{IOpenGfxs, MaterialProp};
use openx_gfx_egin::egin_render::EginRenderer;
use openx_poly::core::{SourcePath, SourceRef};

pub struct OpenGLRenderer;

impl OpenGLRenderer {
    /// C# `OpenGLRenderer`'s static constructor. FTS is registered. NIF stays unregistered.
    fn register() {
        static ONCE: Once = Once::new();
        ONCE.call_once(|| {
            OpenGLX::BuildersByType().lock().unwrap().insert(TypeId::of::<Binary_Ftl>(), crate::open_gl_arkane::open_gl_ftl_object_builder::buildObject);
            OpenGLX::BuildersByType().lock().unwrap().insert(TypeId::of::<Binary_Fts>(), crate::open_gl_arkane::open_gl_fts_object_builder::buildObject);
            setObjectDraw(drawBuilt);
        });
    }

    pub fn createRenderer(parent: Option<&dyn std::any::Any>, gfx: &IOpenGfxs, source: &SourceRef, obj: &Tag, type_: &str) -> Result<Option<Box<dyn EginRenderer + Send>>, Error> {
        Self::register();
        let badObj = || Error::new(ErrorKind::InvalidInput, format!("InvalidCastException: {type_}: {obj:?}"));
        Ok(Some(match type_ {
            "TestTri" => Box::new(TestTriRenderer::new(gfx, source, &())?),
            "Texture" | "VideoTexture" => match obj { Tag::Texture(z) => Box::new(TextureRenderer::new(gfx, source, z.clone(), 0..i32::MAX, false)?), _ => return Err(badObj()) },
            "Object" => match obj {
                Tag::Any(z) => Box::new(ObjectRenderer::new(gfx, source, z.clone())?),
                Tag::Info(z) => {
                    let z = z.clone();
                    let obj: Arc<dyn Any + Send + Sync> = z;
                    Box::new(ObjectRenderer::new(gfx, source, obj)?)
                }
                _ => return Err(badObj()),
            },
            "Material" => match obj.asAny().and_then(|s| s.downcast_ref::<MaterialProp>()) {
                Some(z) => Box::new(MaterialRenderer::new(gfx, source, &SourcePath::Int(z as *const MaterialProp as u64), z.clone())?),
                None => return Err(badObj()),
            },
            "Particle" => Box::new(ParticleRenderer::new(gfx, source, &())?),
            //"World" => Box::new(OpenGLWorldRenderer::new(gfx, source, obj)),
            "Engine" => {
                let any = match obj {
                    Tag::Any(z) => z.clone(),
                    Tag::Info(z) => { let z = z.clone(); let obj: Arc<dyn Any + Send + Sync> = z; obj }
                    _ => return Err(badObj()),
                };
                Box::new(EngineRenderer::fromAny(gfx, source, &any)?)
            }
            _ => return Ok(None),
        }))
    }
}

fn drawBuilt(obj: &(dyn std::any::Any + Send + Sync)) {
    let key = dataAddr(obj);
    if let Some(o) = obj.downcast_ref::<crate::open_gl_arkane::open_gl_ftl_object_builder::OpenGLFtlObject>() {
        let (positions, indices) = pack(&o.Data.Vertices, o.Data.Submeshes.iter().map(|s| s.Triangles.as_slice()));
        drawPositions(key, &positions, &indices);
    } else if let Some(o) = obj.downcast_ref::<crate::open_gl_arkane::open_gl_fts_object_builder::OpenGLFtsObject>() {
        let (positions, indices) = pack(&o.Data.Vertices, o.Data.Submeshes.iter().map(|s| s.Triangles.as_slice()));
        drawPositions(key, &positions, &indices);
    }
}

fn dataAddr(obj: &(dyn std::any::Any + Send + Sync)) -> usize {
    let raw = obj as *const dyn std::any::Any;
    unsafe { std::mem::transmute_copy::<_, usize>(&raw) }
}

fn pack<'a>(vertices: &[glam::Vec3], subs: impl Iterator<Item = &'a [i32]>) -> (Vec<[f32; 3]>, Vec<u32>) {
    let positions = vertices.iter().map(|v| [v.x, v.y, v.z]).collect();
    let mut indices = Vec::new();
    for tris in subs { for i in tris { indices.push(*i as u32); } }
    (positions, indices)
}
