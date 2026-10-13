// PORT-SOURCE: Engines/OpenX.Engine.OpenGL/Engine_OpenGL.cs
// PORT-SHA: 96a5e35f1ac093c0
// PORT-STATUS: partial
//
// Not ported: `OpenGLGfxSprite3D`'s sprite manager (C# never creates it either:
// its constructor line is commented out), and `OpenGLEngine`'s `SfxFactory`
// (`SystemSfx` isn't in openx-sfx's Rust port).
//
// The C# managers share one `TextureManager` and one `MaterialManager` by
// reference. Rust's own them: the object manager holds the material manager,
// which holds the texture manager, so `OpenGLGfxModel`'s accessors reach into
// that chain. `OpenGLGfxTerrain` keeps its own pair, as the C# does.
//
// OpenX's Rust `Platform` (Platform.cs, before C# renamed it Engine.cs) has
// no `GfxFactory` and no `EngineX.Gfx`. So `OpenGLEngine::gfx()` builds the
// C# `GfxFactory` array once and hands it out, standing in for `EngineX.Gfx`
// after `EngineX.Activate(OpenGLEngine.This)`.
//
// GL names are C#'s `int`s (0 is none, -1 is unset). Shaders are embedded with
// `include_str!`, as C# embeds `Gfx/Shaders/*` as resources; the csproj embeds
// only `plane` and `testtri`, so the other names fail to load in both.
// Regex is the `regex` crate.

use std::any::Any;
use std::collections::HashMap;
use std::io::{Error, ErrorKind};
use std::ops::Range;
use std::sync::atomic::{AtomicI32, Ordering};
use std::sync::{Arc, LazyLock, Mutex};
use glam::{Mat4, Quat, Vec3, Vec4};
use glow::HasContext;
use openx::platform::{Caps, Platform};
use openx_gfx::gfx::*;
use openx_gfx::gfx_texture::{TextureFlags, TextureFormat, TexturePixel};
use openx_poly::core::{BoxFuture, SourcePath, SourceRef};
use regex::Regex;
use crate::egin::gl_render::{program, tex, GLRenderMaterial, QuadIndexBuffer};
use crate::egin::gl_vao::GlVaoCache;
use crate::gfx::open_gl::{OpenGLX, GL};

fn outOfRange(name: &str, value: impl std::fmt::Debug) -> Error { Error::new(ErrorKind::InvalidInput, format!("ArgumentOutOfRangeException: {name}: {value:?}")) }

// #region Client

/// OpenGLClientHost: both members throw `NotImplementedException` in C#.
pub struct OpenGLClientHost;

impl OpenGLClientHost {
    pub fn run(&self) { unimplemented!() }
}

// #endregion

// #region Engine

/// The C# type arguments `<object, GLRenderMaterial, int, GfxShader>`.
pub struct OpenGLBackend;

impl Backend for OpenGLBackend {
    /// C# `object`, which may be null.
    type Object = Option<AnyArc>;
    type Material = Arc<GLRenderMaterial>;
    type Texture = i32;
    /// `GfxShader` is a class, and its uniform cache mutates.
    type Shader = Arc<Mutex<GfxShader>>;
    type Sprite = i32;
}

pub type OpenGLTextureManager = TextureManager<OpenGLBackend, OpenGLTextureBuilder>;
pub type OpenGLMaterialManager = MaterialManager<OpenGLBackend, OpenGLTextureBuilder, OpenGLMaterialBuilder>;
pub type OpenGLObjectManager = ObjectModelManager<OpenGLBackend, OpenGLTextureBuilder, OpenGLMaterialBuilder, OpenGLObjectBuilder>;
pub type OpenGLShaderManager = ShaderManager<OpenGLBackend, OpenGLShaderBuilder>;

/// OpenGLObjectBuilder
pub struct OpenGLObjectBuilder;

impl ObjectModelBuilder<OpenGLBackend, OpenGLTextureBuilder, OpenGLMaterialBuilder> for OpenGLObjectBuilder {
    fn instance(&mut self, prefab: &Option<AnyArc>, parent: Option<&Option<AnyArc>>) -> Option<AnyArc> {
        Some(Arc::new("clone"))
    }

    fn create<'a>(&'a mut self, source: &'a SourceRef, path: &'a (dyn Any + Send + Sync), isStatic: bool, materialManager: &'a mut OpenGLMaterialManager) -> BoxFuture<'a, Result<Option<AnyArc>, Error>> {
        Box::pin(async move {
            let builder = *OpenGLX::BuildersByType().lock().unwrap().get(&path.type_id()).ok_or_else(|| Error::new(ErrorKind::NotFound, "KeyNotFoundException: BuildersByType"))?;
            let s = builder(source, path, isStatic, materialManager).await?;
            Ok(Some(s))
        })
    }

    fn ensure(&mut self) {}
}

/// OpenGLShaderBuilder
pub struct OpenGLShaderBuilder;

impl OpenGLShaderBuilder {
    const ShaderDirectory: &'static str = "OpenX.Engine.OpenGL.Gfx.Shaders";

    fn getShaderFileByName(name: &str) -> &str {
        match name {
            "plane" => "plane",
            "testtri" => "testtri",
            "vrf.error" => "error",
            "vrf.grid" => "debug_grid",
            "vrf.picking" => "picking",
            "vrf.particle.sprite" => "particle_sprite",
            "vrf.particle.trail" => "particle_trail",
            "tools_sprite.vfx" => "sprite",
            "vr_unlit.vfx" => "vr_unlit",
            "vr_black_unlit.vfx" => "vr_black_unlit",
            "water_dota.vfx" => "water",
            "hero.vfx" | "hero_underlords.vfx" => "dota_hero",
            "multiblend.vfx" => "multiblend",
            _ => {
                if name.starts_with("vr_") { return "vr_standard"; }
                eprintln!("WARN: Unknown shader {name}, defaulting to simple.");
                "simple"
            }
        }
    }

    /// C# `GetManifestResourceStream($"{ShaderDirectory}.{name}")`.
    fn getShaderSource(name: &str) -> Result<String, Error> {
        Ok(match name {
            "plane.vert" => include_str!("gfx/shaders/plane.vert"),
            "plane.frag" => include_str!("gfx/shaders/plane.frag"),
            "testtri.vert" => include_str!("gfx/shaders/testtri.vert"),
            "testtri.frag" => include_str!("gfx/shaders/testtri.frag"),
            _ => return Err(Error::new(ErrorKind::NotFound, format!("ArgumentNullException: stream ({}.{name})", Self::ShaderDirectory))),
        }.to_string())
    }

    /// Preprocess a vertex shader's source to include the #version plus #defines for parameters
    fn preprocessVertexShader(source: &str, args: &HashMap<String, bool>) -> Result<String, Error> { Self::resolveIncludes(&Self::updateDefines(source, args)) }

    /// Update default defines with possible overrides from the model
    fn updateDefines(source: &str, args: &HashMap<String, bool>) -> String {
        // find all #define param_(paramName) (paramValue) using regex
        static DEFINE: LazyLock<Regex> = LazyLock::new(|| Regex::new(r"#define param_(\S*?) (\S*?)\s*?\n").unwrap());
        let mut source = source.to_string();
        let defines: Vec<(String, usize, usize)> = DEFINE.captures_iter(&source.clone()).map(|m| { let g = m.get(2).unwrap(); (m[1].to_string(), g.start(), g.len()) }).collect();
        for (name, index, length) in defines {
            if let Some(&value) = args.get(&name) {
                let end = index + length.min(source.len() - index);
                source.replace_range(index..end, if value { "1" } else { "0" });
            }
        }
        source
    }

    /// Remove any #includes from the shader and replace with the included code
    fn resolveIncludes(source: &str) -> Result<String, Error> {
        static INCLUDE: LazyLock<Regex> = LazyLock::new(|| Regex::new(r#"#include "([^"]*?)";?\s*\n"#).unwrap());
        let mut source = source.to_string();
        for define in INCLUDE.captures_iter(&source.clone()) {
            let mut includedCode = Self::getShaderSource(&define[1])?;
            // recursively resolve includes in the included code. (Watch out for cyclic dependencies!)
            includedCode = Self::resolveIncludes(&includedCode)?;
            if !includedCode.ends_with('\n') { includedCode.push('\n'); }
            source = source.replace(&define[0], &includedCode);
        }
        Ok(source)
    }

    fn findDefines(source: &str) -> Vec<String> {
        static DEFINE: LazyLock<Regex> = LazyLock::new(|| Regex::new(r"#define param_(\S+)").unwrap());
        DEFINE.captures_iter(source).map(|m| m[1].to_string()).collect()
    }
}

impl ShaderBuilder<OpenGLBackend> for OpenGLShaderBuilder {
    fn create(&mut self, path: &SourcePath, args: &HashMap<String, bool>) -> Result<Arc<Mutex<GfxShader>>, Error> {
        let SourcePath::Str(name) = path else { return Err(Error::new(ErrorKind::InvalidInput, "InvalidCastException: path")) };
        let shaderFileName = Self::getShaderFileByName(name);
        let gl = GL();
        unsafe {
            // defines
            let mut defines: Vec<String> = Vec::new();

            // vertex shader
            let vertexShader = gl.create_shader(glow::VERTEX_SHADER).map_err(|e| Error::new(ErrorKind::Other, e))?;
            {
                let shaderSource = Self::getShaderSource(&format!("{shaderFileName}.vert"))?;
                gl.shader_source(vertexShader, &Self::preprocessVertexShader(&shaderSource, args)?);
                // defines: find defines supported from source
                defines.extend(Self::findDefines(&shaderSource));
            }
            gl.compile_shader(vertexShader);
            if !gl.get_shader_compile_status(vertexShader) {
                let vsInfo = gl.get_shader_info_log(vertexShader);
                return Err(Error::new(ErrorKind::InvalidData, format!("Error setting up Vertex B_Shader \"{name}\": {vsInfo}")));
            }

            // fragment shader
            let fragmentShader = gl.create_shader(glow::FRAGMENT_SHADER).map_err(|e| Error::new(ErrorKind::Other, e))?;
            {
                let shaderSource = Self::getShaderSource(&format!("{shaderFileName}.frag"))?;
                gl.shader_source(fragmentShader, &Self::updateDefines(&shaderSource, args));
                // defines: find render modes supported from source, take union to avoid duplicates
                for z in Self::findDefines(&shaderSource) { if !defines.contains(&z) { defines.push(z); } }
            }
            gl.compile_shader(fragmentShader);
            if !gl.get_shader_compile_status(fragmentShader) {
                let fsInfo = gl.get_shader_info_log(fragmentShader);
                return Err(Error::new(ErrorKind::InvalidData, format!("Error setting up Fragment B_Shader \"{name}\": {fsInfo}")));
            }

            // defines: find render modes
            const RenderMode: &str = "renderMode_";
            let renderModes: Vec<String> = defines.iter().filter_map(|k| k.strip_prefix(RenderMode).map(str::to_string)).collect();

            // build shader
            let mut shader = GfxShader::new(
                |p, n| unsafe { program(p).and_then(|p| GL().get_uniform_location(p, n)).map_or(-1, |l| l.0 as i32) },
                |p, n| unsafe { program(p).and_then(|p| GL().get_attrib_location(p, n)).map_or(-1, |l| l as i32) });
            shader.name = name.clone();
            shader.parameters = args.clone();
            let shaderProgram = gl.create_program().map_err(|e| Error::new(ErrorKind::Other, e))?;
            shader.program = shaderProgram.0.get() as i32;
            shader.renderModes = renderModes;
            gl.attach_shader(shaderProgram, vertexShader);
            gl.attach_shader(shaderProgram, fragmentShader);
            gl.link_program(shaderProgram);
            gl.validate_program(shaderProgram);
            let linkStatus = gl.get_program_link_status(shaderProgram);
            gl.detach_shader(shaderProgram, vertexShader);
            gl.delete_shader(vertexShader);
            gl.detach_shader(shaderProgram, fragmentShader);
            gl.delete_shader(fragmentShader);
            if !linkStatus {
                let linkInfo = gl.get_program_info_log(shaderProgram);
                return Err(Error::new(ErrorKind::InvalidData, format!("Error linking shaders: {linkInfo} (link status = 0)")));
            }
            //eprintln!("Shader {name}({}) compiled and linked succesfully", args.keys().cloned().collect::<Vec<_>>().join(", "));
            Ok(Arc::new(Mutex::new(shader)))
        }
    }
}

/// OpenGLTextureBuilder
pub struct OpenGLTextureBuilder;

static _defaultTexture: AtomicI32 = AtomicI32::new(-1);

impl OpenGLTextureBuilder {
    pub fn release(&mut self) {
        let z = _defaultTexture.swap(-1, Ordering::Relaxed);
        if z > -1 { unsafe { if let Some(t) = tex(z) { GL().delete_texture(t); } } }
    }

    fn createDefaultTexture() -> i32 {
        Self::createSolidX(4, 4, &[
            0.9, 0.2, 0.8, 1.,
            0., 0.9, 0., 1.,
            0.9, 0.2, 0.8, 1.,
            0., 0.9, 0., 1.,

            0., 0.9, 0., 1.,
            0.9, 0.2, 0.8, 1.,
            0., 0.9, 0., 1.,
            0.9, 0.2, 0.8, 1.,

            0.9, 0.2, 0.8, 1.,
            0., 0.9, 0., 1.,
            0.9, 0.2, 0.8, 1.,
            0., 0.9, 0., 1.,

            0., 0.9, 0., 1.,
            0.9, 0.2, 0.8, 1.,
            0., 0.9, 0., 1.,
            0.9, 0.2, 0.8, 1.,
        ])
    }

    /// C# `Default`: the default texture, created on first use.
    pub fn defaultTexture() -> i32 {
        let z = _defaultTexture.load(Ordering::Relaxed);
        if z > -1 { return z; }
        let z = Self::createDefaultTexture();
        _defaultTexture.store(z, Ordering::Relaxed);
        z
    }

    /// C# `CreateSolid`, which needs no instance state.
    fn createSolidX(width: i32, height: i32, pixels: &[f32]) -> i32 {
        let gl = GL();
        unsafe {
            let Ok(t) = gl.create_texture() else { return 0 };
            gl.bind_texture(glow::TEXTURE_2D, Some(t));
            gl.tex_image_2d(glow::TEXTURE_2D, 0, glow::RGBA32F as i32, width, height, 0, glow::RGBA, glow::FLOAT, glow::PixelUnpackData::Slice(Some(bytemuck::cast_slice(pixels))));
            gl.tex_parameter_i32(glow::TEXTURE_2D, glow::TEXTURE_MAX_LEVEL, 0);
            gl.tex_parameter_i32(glow::TEXTURE_2D, glow::TEXTURE_MIN_FILTER, glow::NEAREST as i32);
            gl.tex_parameter_i32(glow::TEXTURE_2D, glow::TEXTURE_MAG_FILTER, glow::NEAREST as i32);
            gl.tex_parameter_i32(glow::TEXTURE_2D, glow::TEXTURE_WRAP_S, glow::REPEAT as i32);
            gl.tex_parameter_i32(glow::TEXTURE_2D, glow::TEXTURE_WRAP_T, glow::REPEAT as i32);
            gl.bind_texture(glow::TEXTURE_2D, None); // release texture
            t.0.get() as i32
        }
    }
}

impl TextureBuilder<OpenGLBackend> for OpenGLTextureBuilder {
    fn default(&self) -> i32 { Self::defaultTexture() }

    fn createNormalMap(&mut self, src: &i32, strength: f32) -> i32 { 0 }

    fn createSolid(&mut self, width: i32, height: i32, pixels: &[f32]) -> i32 { Self::createSolidX(width, height, pixels) }

    fn create(&mut self, reuse: Option<i32>, src: &dyn ITexture, level2: Option<Range<i32>>) -> Result<i32, Error> {
        let mut result: Result<i32, Error> = Err(outOfRange("x", "null"));
        src.create("GL", &mut |x| result = (|| -> Result<i32, Error> {
            let Some(t) = x.downcast_ref::<TextureAsBytes>() else { return Err(outOfRange("x", "not TextureAsBytes")) };
            let gl = GL();
            unsafe {
                let tex_ = match reuse.and_then(tex) { Some(z) => z, None => gl.create_texture().map_err(|e| Error::new(ErrorKind::Other, e))? };
                let numMipMaps = src.mipMaps().max(1);
                let level = (level2.as_ref().map_or(0, |s| s.start), numMipMaps);
                // bind
                gl.bind_texture(glow::TEXTURE_2D, Some(tex_));
                if level.0 > 0 { gl.tex_parameter_i32(glow::TEXTURE_2D, glow::TEXTURE_BASE_LEVEL, level.0); }
                gl.tex_parameter_i32(glow::TEXTURE_2D, glow::TEXTURE_MAX_LEVEL, level.1 - 1);
                // C# `Spans` null is an empty `spans` here
                let (bytes, fmt, spans) = (&t.bytes, &t.format, &t.spans);
                // decode
                let compressedTexImage2D = |tex: &dyn ITexture, level: (i32, i32), internalFormat: u32| -> bool {
                    let (width, height) = (tex.width(), tex.height());
                    if !spans.is_empty() {
                        for l in level.0..level.1 {
                            let Some(span) = spans.get(l as usize) else { return false };
                            let pixels = &bytes[span.clone()];
                            gl.compressed_tex_image_2d(glow::TEXTURE_2D, l, internalFormat as i32, width >> l, height >> l, 0, pixels.len() as i32, pixels);
                        }
                    }
                    else { gl.compressed_tex_image_2d(glow::TEXTURE_2D, 0, internalFormat as i32, width, height, 0, bytes.len() as i32, bytes); }
                    true
                };
                let texImage2D = |tex: &dyn ITexture, level: (i32, i32), internalFormat: u32, format: u32, type_: u32| -> bool {
                    let (width, height) = (tex.width(), tex.height());
                    if !spans.is_empty() {
                        for l in level.0..level.1 {
                            let Some(span) = spans.get(l as usize) else { return false };
                            let pixels = &bytes[span.clone()];
                            gl.tex_image_2d(glow::TEXTURE_2D, l, internalFormat as i32, width >> l, height >> l, 0, format, type_, glow::PixelUnpackData::Slice(Some(pixels)));
                        }
                    }
                    else { gl.tex_image_2d(glow::TEXTURE_2D, 0, internalFormat as i32, width, height, 0, format, type_, glow::PixelUnpackData::Slice(Some(bytes))); }
                    true
                };
                // process
                if bytes.is_empty() { return Ok(Self::defaultTexture()); }
                else if let Some(&(formatx, pixel)) = fmt.downcast_ref::<(TextureFormat, TexturePixel)>() {
                    let s = (pixel as i32 & TexturePixel::Signed as i32) != 0;
                    let f = (pixel as i32 & TexturePixel::Float as i32) != 0;
                    if (formatx as i32 & TextureFormat::Compressed as i32) != 0 {
                        use TextureFormat::*;
                        let internalFormat = match formatx {
                            DXT1 => if s { glow::COMPRESSED_SRGB_S3TC_DXT1_EXT } else { glow::COMPRESSED_RGB_S3TC_DXT1_EXT },
                            DXT1A => if s { glow::COMPRESSED_SRGB_ALPHA_S3TC_DXT1_EXT } else { glow::COMPRESSED_RGBA_S3TC_DXT1_EXT },
                            DXT3 => if s { glow::COMPRESSED_SRGB_ALPHA_S3TC_DXT3_EXT } else { glow::COMPRESSED_RGBA_S3TC_DXT3_EXT },
                            DXT5 => if s { glow::COMPRESSED_SRGB_ALPHA_S3TC_DXT5_EXT } else { glow::COMPRESSED_RGBA_S3TC_DXT5_EXT },
                            BC4 => if s { glow::COMPRESSED_SIGNED_RED_RGTC1 } else { glow::COMPRESSED_RED_RGTC1 },
                            BC5 => if s { glow::COMPRESSED_SIGNED_RG_RGTC2 } else { glow::COMPRESSED_RG_RGTC2 },
                            BC6H => if s { glow::COMPRESSED_RGB_BPTC_SIGNED_FLOAT } else { glow::COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT },
                            BC7 => if s { glow::COMPRESSED_SRGB_ALPHA_BPTC_UNORM } else { glow::COMPRESSED_RGBA_BPTC_UNORM },
                            ETC2 => if s { glow::COMPRESSED_SRGB8_ETC2 } else { glow::COMPRESSED_RGB8_ETC2 },
                            ETC2_EAC => if s { glow::COMPRESSED_SRGB8_ALPHA8_ETC2_EAC } else { glow::COMPRESSED_RGBA8_ETC2_EAC },
                            _ => return Err(outOfRange("TextureFormat", formatx)),
                        };
                        if internalFormat == 0 || !compressedTexImage2D(src, level, internalFormat) { return Ok(Self::defaultTexture()); }
                    }
                    else {
                        use TextureFormat::*;
                        let (internalFormat, format, type_) = match formatx {
                            I8 => (0x8040 /* INTENSITY8_EXT */, glow::RED, glow::UNSIGNED_BYTE),
                            L8 => (6409, 6409, glow::UNSIGNED_BYTE),
                            R8 => (glow::R8, glow::RED, glow::UNSIGNED_BYTE),
                            R16 => if f { (glow::R16F, glow::RED, glow::FLOAT) } else { (glow::R16, glow::RED, glow::UNSIGNED_SHORT) },
                            RG16 => if f { (glow::RG16F, glow::RED, glow::FLOAT) } else { (glow::RG16, glow::RED, glow::UNSIGNED_SHORT) },
                            RGB24 => (glow::RGB8, glow::RGB, glow::UNSIGNED_BYTE),
                            RGB565 => (glow::RGB5, glow::RGB, glow::UNSIGNED_BYTE), //UnsignedShort565
                            RGBA32 => (glow::RGBA8, glow::RGBA, glow::UNSIGNED_BYTE),
                            ARGB32 => (glow::RGBA, glow::RGB, glow::UNSIGNED_INT_8_8_8_8_REV), //: odd Rgb
                            BGRA32 => (glow::RGBA, glow::BGRA, glow::UNSIGNED_INT_8_8_8_8),
                            BGRA1555 => (glow::RGBA, glow::BGRA, glow::UNSIGNED_SHORT_1_5_5_5_REV),
                            _ => return Err(outOfRange("TextureFormat", formatx)),
                        };
                        if internalFormat == 0 || !texImage2D(src, level, internalFormat, format, type_) { return Ok(Self::defaultTexture()); }
                    }
                }
                else { return Err(outOfRange("fmt", "not (TextureFormat, TexturePixel)")); }
                // texture
                let maxTextureMaxAnisotropy = openx_gfx::gfx::maxTextureMaxAnisotropy();
                if maxTextureMaxAnisotropy >= 4 {
                    // C# passes All.MaxTextureMaxAnisotropyExt (0x84FF, the query) as the parameter name. Kept.
                    gl.tex_parameter_i32(glow::TEXTURE_2D, 0x84FF, maxTextureMaxAnisotropy);
                    gl.tex_parameter_i32(glow::TEXTURE_2D, glow::TEXTURE_MIN_FILTER, glow::LINEAR_MIPMAP_LINEAR as i32);
                    gl.tex_parameter_i32(glow::TEXTURE_2D, glow::TEXTURE_MAG_FILTER, glow::LINEAR as i32);
                }
                else {
                    gl.tex_parameter_i32(glow::TEXTURE_2D, glow::TEXTURE_MIN_FILTER, glow::NEAREST as i32);
                    gl.tex_parameter_i32(glow::TEXTURE_2D, glow::TEXTURE_MAG_FILTER, glow::NEAREST as i32);
                }
                gl.tex_parameter_i32(glow::TEXTURE_2D, glow::TEXTURE_WRAP_S, (if src.texFlags().contains(TextureFlags::SUGGEST_CLAMPS) { glow::CLAMP_TO_EDGE } else { glow::REPEAT }) as i32);
                gl.tex_parameter_i32(glow::TEXTURE_2D, glow::TEXTURE_WRAP_T, (if src.texFlags().contains(TextureFlags::SUGGEST_CLAMPT) { glow::CLAMP_TO_EDGE } else { glow::REPEAT }) as i32);
                gl.bind_texture(glow::TEXTURE_2D, None); // release texture
                Ok(tex_.0.get() as i32)
            }
        })())?;
        result
    }

    fn delete(&mut self, src: &i32) { unsafe { if let Some(t) = tex(*src) { GL().delete_texture(t); } } }
}

/// OpenGLMaterialBuilder
pub struct OpenGLMaterialBuilder;

static _defaultMaterial: LazyLock<Arc<GLRenderMaterial>> = LazyLock::new(|| Arc::new(OpenGLMaterialBuilder::createDefaultMaterial()));
static _terrainMaterial: LazyLock<Arc<GLRenderMaterial>> = LazyLock::new(|| Arc::new(OpenGLMaterialBuilder::createTerrainMaterial()));

impl OpenGLMaterialBuilder {
    fn createDefaultMaterial() -> GLRenderMaterial {
        let mut m = GLRenderMaterial::new(MaterialPropKind::Shader(MaterialShaderProp::default())).unwrap();
        m.base.textures.insert("g_tColor".into(), OpenGLTextureBuilder::defaultTexture());
        if let MaterialPropKind::Shader(z) = &mut m.base.material { z.shaderName = "vrf.error".into(); }
        m
    }

    fn createTerrainMaterial() -> GLRenderMaterial {
        let mut m = GLRenderMaterial::new(MaterialPropKind::Shader(MaterialShaderProp::default())).unwrap();
        if let MaterialPropKind::Shader(z) = &mut m.base.material { z.shaderName = "vrf.error".into(); }
        m
    }
}

impl MaterialBuilder<OpenGLBackend, OpenGLTextureBuilder> for OpenGLMaterialBuilder {
    fn default(&self) -> Arc<GLRenderMaterial> { _defaultMaterial.clone() }
    fn terrain(&self) -> Arc<GLRenderMaterial> { _terrainMaterial.clone() }

    fn create<'a>(&'a mut self, source: &'a SourceRef, path: &'a MaterialProp, textureManager: &'a mut OpenGLTextureManager) -> BoxFuture<'a, Result<Arc<GLRenderMaterial>, Error>> {
        Box::pin(async move {
            let mut m = GLRenderMaterial::new(path.kind.clone())?;
            match &path.kind {
                MaterialPropKind::ShaderV(_) => {
                    let MaterialPropKind::ShaderV(p) = &mut m.base.material else { unreachable!() };
                    let p = p.clone();
                    for tex in &p.textureParams { let (t, _) = textureManager.create(source, &format!("{}_c", tex.1).into(), None).await?; m.base.textures.insert(tex.0.clone(), t); }
                    if p.intParams.get("F_SOLID_COLOR") == Some(&1) {
                        let a = p.vectorParams["g_vColorTint"];
                        m.base.textures.insert("g_tColor".into(), textureManager.createSolid(1, 1, &[a.x, a.y, a.z, a.w]));
                    }
                    if !m.base.textures.contains_key("g_tColor") { m.base.textures.insert("g_tColor".into(), textureManager.default()); }

                    // Since our shaders only use g_tColor, we have to find at least one texture to use here
                    if m.base.textures["g_tColor"] == textureManager.default() {
                        for name in ["g_tColor2", "g_tColor1", "g_tColorA", "g_tColorB", "g_tColorC"] {
                            if let Some(&z) = m.base.textures.get(name) {
                                m.base.textures.insert("g_tColor".into(), z);
                                break;
                            }
                        }
                    }

                    // Set default values for scale and positions (C# sets them on the shared prop; here on the material's copy)
                    let MaterialPropKind::ShaderV(p) = &mut m.base.material else { unreachable!() };
                    p.vectorParams.entry("g_vTexCoordScale".into()).or_insert(Vec4::ONE);
                    p.vectorParams.entry("g_vTexCoordOffset".into()).or_insert(Vec4::ZERO);
                    p.vectorParams.entry("g_vColorTint".into()).or_insert(Vec4::ONE);
                    Ok(Arc::new(m))
                }
                //MaterialPropKind::Shader(_) => Ok(Arc::new(m)),
                _ => Err(outOfRange("path", "not MaterialShaderVProp")),
            }
        })
    }
}

/// OpenGLGfxApi: the members that throw `NotImplementedException` in C# panic with `unimplemented!`.
pub struct OpenGLGfxApi;

impl IOpenGfx for OpenGLGfxApi {}

impl IOpenGfxApi<OpenGLBackend> for OpenGLGfxApi {
    fn addMeshCollider(&mut self, src: &Option<AnyArc>, mesh: &(dyn Any + Send + Sync), isKinematic: bool, isStatic: bool) { unimplemented!() }
    fn addMeshRenderer(&mut self, src: &Option<AnyArc>, mesh: &(dyn Any + Send + Sync), material: &Arc<GLRenderMaterial>, enabled: bool, isStatic: bool) { unimplemented!() }
    fn addMissingMeshCollidersRecursively(&mut self, src: &Option<AnyArc>, isStatic: bool) { unimplemented!() }
    fn attach(&mut self, method: GfxAttach, src: &Option<AnyArc>, args: &[&(dyn Any + Send + Sync)]) {}
    fn createMesh(&mut self, mesh: &(dyn Any + Send + Sync)) -> AnyBox { unimplemented!() }
    fn createObject(&mut self, name: &str, tag: Option<&str>, parent: Option<&Option<AnyArc>>) -> Option<AnyArc> { Some(Arc::new(())) }
    fn setLayerRecursively(&mut self, src: &Option<AnyArc>, layer: i32) { unimplemented!() }
    fn parent(&mut self, src: &Option<AnyArc>, parent: &Option<AnyArc>) { unimplemented!() }
    fn transform(&mut self, src: &Option<AnyArc>, position: Vec3, rotation: Quat, localScale: Vec3) { unimplemented!() }
    fn transformM(&mut self, src: &Option<AnyArc>, position: Vec3, rotation: Mat4, localScale: Vec3) { unimplemented!() }
    fn setVisible(&mut self, src: &Option<AnyArc>, visible: bool) {}
    fn destroy(&mut self, src: &Option<AnyArc>) {}
}

/// OpenGLGfxSprite3D: C#'s `_spriteManager` is never created (its line is
/// commented out), so every member throws `NullReferenceException`. Without a
/// sprite builder there's nothing to put behind `IOpenGfxSprite` here.
pub struct OpenGLGfxSprite3D;

impl IOpenGfx for OpenGLGfxSprite3D {}

/// OpenGLGfxModel
pub struct OpenGLGfxModel {
    _objectManager: OpenGLObjectManager,
    _shaderManager: OpenGLShaderManager,
    // cache
    _quadIndices: Option<QuadIndexBuffer>,
    pub meshBufferCache: GlVaoCache,
}

impl OpenGLGfxModel {
    pub fn new() -> Self {
        let _textureManager = TextureManager::new(OpenGLTextureBuilder);
        let _materialManager = MaterialManager::new(_textureManager, OpenGLMaterialBuilder);
        let _objectManager = ObjectModelManager::new(_materialManager, OpenGLObjectBuilder);
        let _shaderManager = ShaderManager::new(OpenGLShaderBuilder);
        Self { _objectManager, _shaderManager, _quadIndices: None, meshBufferCache: GlVaoCache::new() }
    }

    pub fn quadIndices(&mut self) -> Result<&QuadIndexBuffer, Error> {
        if self._quadIndices.is_none() { self._quadIndices = Some(unsafe { QuadIndexBuffer::new(GL(), 65532) }.map_err(|e| Error::new(ErrorKind::Other, e))?); }
        Ok(self._quadIndices.as_ref().unwrap())
    }
}

impl IOpenGfx for OpenGLGfxModel {}

impl IOpenGfxModel<OpenGLBackend> for OpenGLGfxModel {
    type TextureBuilder = OpenGLTextureBuilder;
    type MaterialBuilder = OpenGLMaterialBuilder;
    type ObjectBuilder = OpenGLObjectBuilder;
    type ShaderBuilder = OpenGLShaderBuilder;
    fn materialManager(&mut self) -> &mut OpenGLMaterialManager { &mut self._objectManager.materialManager }
    fn objectManager(&mut self) -> &mut OpenGLObjectManager { &mut self._objectManager }
    fn shaderManager(&mut self) -> &mut OpenGLShaderManager { &mut self._shaderManager }
    fn textureManager(&mut self) -> &mut OpenGLTextureManager { &mut self._objectManager.materialManager.textureManager }
    fn preload(&mut self, source: &SourceRef, path: &SourcePath) { self._objectManager.preload(source, path) }
    fn preloadTexture(&mut self, source: &SourceRef, path: &SourcePath) { self.textureManager().preload(source, path) }
    fn create<'a>(&'a mut self, source: &'a SourceRef, path: &'a SourcePath, isStatic: bool, parent: Option<&'a Option<AnyArc>>) -> BoxFuture<'a, Result<(Option<AnyArc>, AnyArc), Error>> { Box::pin(self._objectManager.create(source, path, isStatic, parent)) }
    fn createShader(&mut self, source: &SourceRef, path: &SourcePath, args: Option<&HashMap<String, bool>>) -> Result<Arc<Mutex<GfxShader>>, Error> { self._shaderManager.create(source, path, args) }
    fn createTexture<'a>(&'a mut self, source: &'a SourceRef, path: &'a SourcePath, level: Option<Range<i32>>) -> BoxFuture<'a, Result<(i32, Arc<dyn ITexture>), Error>> { Box::pin(self.textureManager().create(source, path, level)) }
    fn post(&mut self, src: &Option<AnyArc>, position: Vec3, eulerAngles: Vec3, scale: Option<f32>, parent: Option<&Option<AnyArc>>) {}
}

/// OpenGLGfxTerrain
pub struct OpenGLGfxTerrain {
    _materialManager: OpenGLMaterialManager,
}

impl OpenGLGfxTerrain {
    pub fn new() -> Self { Self { _materialManager: MaterialManager::new(TextureManager::new(OpenGLTextureBuilder), OpenGLMaterialBuilder) } }
}

impl IOpenGfx for OpenGLGfxTerrain {}

impl IOpenGfxTerrain<OpenGLBackend> for OpenGLGfxTerrain {
    fn createData(&mut self, offset: i32, heights: &[Vec<f32>], heightRange: f32, sampleDistance: f32, layers: &[GfxTerrainLayer<OpenGLBackend>], alphaMap: &[Vec<Vec<f32>>]) -> AnyBox { Box::new(()) }
    fn create(&mut self, name: &str, position: Option<Vec3>, data: &(dyn Any + Send + Sync), parent: Option<&Option<AnyArc>>) -> Option<AnyArc> { None }
}

/// OpenGLEngine
#[derive(Debug, Clone, Copy, Default)]
pub struct OpenGLEngine;

impl OpenGLEngine {
    pub const This: OpenGLEngine = OpenGLEngine;

    /// C# `GfxFactory`.
    pub fn gfxFactory() -> IOpenGfxs {
        vec![Some(Arc::new(Mutex::new(OpenGLGfxApi))), None, Some(Arc::new(Mutex::new(OpenGLGfxSprite3D))), Some(Arc::new(Mutex::new(OpenGLGfxModel::new()))), None, Some(Arc::new(Mutex::new(OpenGLGfxTerrain::new())))]
    }

    /// C# `EngineX.Gfx` once this engine is active: `GfxFactory()`, made once.
    pub fn gfx() -> &'static IOpenGfxs {
        static Gfx: LazyLock<IOpenGfxs> = LazyLock::new(OpenGLEngine::gfxFactory);
        &Gfx
    }
}

impl Platform for OpenGLEngine {
    fn id(&self) -> &str { "GL" }
    fn name(&self) -> &str { "OpenGL" }
    fn caps(&self) -> Caps { Caps::DRAWING }
}

// #endregion
