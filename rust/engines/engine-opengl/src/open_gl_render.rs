// PORT-SOURCE: Engines/GameX.Engine.OpenGL/OpenGL_Render.cs
// PORT-SHA: 78188c836ed24ae9
// PORT-STATUS: done
//
// PARTIAL PORT: the static constructor registers `Binary_Nif` with
// `OpenGLNifObjectBuilder.BuildObject`, both unported (Gamebryo's Nif.cs and
// OpenGLNifObjectBuilder.cs), so an "Object" view has no builder.
// "Particle" builds the outer `ParticleRenderer`, which draws nothing.
// "Engine" needs the value to be an `Arc<dyn CellDatabase>` inside the `Any`.
// `Binary_Engine` supplies that when the query archive implements `IQueryFunc`.
// Bethesda's `Binary_Esm` is that implementor and is still unported.
//
// C#'s `object obj` is the MetaContent's `Tag`: "Texture" takes a
// `Tag::Texture`, "Material" a `Tag::Any` holding a `MaterialProp`, and
// `obj is IHaveSource z` has no Rust form yet.

use std::any::Any;
use std::io::{Error, ErrorKind};
use std::sync::Arc;
use gamex::meta::Tag;
use openx_engine_opengl::gfx::open_gl_render::{EngineRenderer, MaterialRenderer, ObjectRenderer, ParticleRenderer, TestTriRenderer, TextureRenderer};
use openx_gfx::gfx::{IOpenGfxs, MaterialProp};
use openx_gfx_egin::egin_render::EginRenderer;
use openx_poly::core::{SourcePath, SourceRef};

pub struct OpenGLRenderer;

impl OpenGLRenderer {
    pub fn createRenderer(parent: Option<&dyn std::any::Any>, gfx: &IOpenGfxs, source: &SourceRef, obj: &Tag, type_: &str) -> Result<Option<Box<dyn EginRenderer + Send>>, Error> {
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
