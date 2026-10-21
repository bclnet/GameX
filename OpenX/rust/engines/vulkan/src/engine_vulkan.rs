// PORT-SOURCE: Engines/OpenX.Engine.Vulkan/Engine_Vulkan.cs
// PORT-STATUS: partial
//
// Ported: `VulkanEngine` registration (id, name, caps) and `createInstance`,
// `VulkanShellEngine`, `VulkanTexture`, `VulkanTextureBuilder`, and
// `VulkanGfxModel`'s `TextureManager`.
// Not ported: `VulkanClientHost`, `VulkanGfxApi`, `VulkanGfxSprite3D`, `VulkanSfx`,
// `GfxFactory`, `SfxFactory`, and the material, object, and shader managers.
// Those stay unconstructed, as in C#. The texture keeps bytes. It is not a device image.

use std::any::Any;
use std::io::{Error, ErrorKind};
use std::ops::Range;
use std::sync::atomic::{AtomicI32, Ordering};
use std::sync::{Arc, LazyLock, Mutex, OnceLock};
use openx::engine::{Engine, EngineX};
use openx_gfx::gfx::{ITexture, TextureAsBytes, TextureBuilder, TextureManager};
use openx_gfx::gfx_texture::{TextureFlags, TextureFormat, TexturePixel};
use openx_poly::core::{BoxFuture, SourcePath, SourceRef};
use crate::gfx::vulkan::VulkanInstance;

fn outOfRange(name: &str, value: impl std::fmt::Debug) -> Error { Error::new(ErrorKind::InvalidInput, format!("ArgumentOutOfRangeException: {name}: {value:?}")) }

/// The pixels `VulkanGfxModel.TextureManager` keeps. No device image yet.
#[derive(Clone)]
pub struct VulkanTexture {
    id: i32,
    data: Arc<Mutex<VulkanTextureData>>,
}

struct VulkanTextureData {
    width: i32,
    height: i32,
    mipMaps: i32,
    format: Option<(TextureFormat, TexturePixel)>,
    bytes: Vec<u8>,
    levelStart: i32,
    levelStop: i32,
    clampS: bool,
    clampT: bool,
    deleted: bool,
}

impl PartialEq for VulkanTexture {
    fn eq(&self, other: &Self) -> bool { self.id == other.id }
}
impl Eq for VulkanTexture {}
impl std::hash::Hash for VulkanTexture {
    fn hash<H: std::hash::Hasher>(&self, state: &mut H) { self.id.hash(state); }
}

impl VulkanTexture {
    fn new(data: VulkanTextureData) -> Self {
        static NEXT: AtomicI32 = AtomicI32::new(1);
        Self { id: NEXT.fetch_add(1, Ordering::Relaxed), data: Arc::new(Mutex::new(data)) }
    }

    fn empty() -> Self {
        static NONE: OnceLock<VulkanTexture> = OnceLock::new();
        NONE.get_or_init(|| Self { id: 0, data: Arc::new(Mutex::new(VulkanTextureData {
            width: 0, height: 0, mipMaps: 1, format: None, bytes: Vec::new(), levelStart: 0, levelStop: 1, clampS: false, clampT: false, deleted: false,
        })) }).clone()
    }
}

/// VulkanTextureBuilder. Accepts the same `TextureAsBytes` path as OpenGL and keeps the bytes.
pub struct VulkanTextureBuilder;

impl VulkanTextureBuilder {
    fn checker() -> VulkanTexture {
        Self::solid(4, 4, &[
            0.9, 0.2, 0.8, 1., 0., 0.9, 0., 1., 0.9, 0.2, 0.8, 1., 0., 0.9, 0., 1.,
            0., 0.9, 0., 1., 0.9, 0.2, 0.8, 1., 0., 0.9, 0., 1., 0.9, 0.2, 0.8, 1.,
            0.9, 0.2, 0.8, 1., 0., 0.9, 0., 1., 0.9, 0.2, 0.8, 1., 0., 0.9, 0., 1.,
            0., 0.9, 0., 1., 0.9, 0.2, 0.8, 1., 0., 0.9, 0., 1., 0.9, 0.2, 0.8, 1.,
        ])
    }

    fn solid(width: i32, height: i32, rgbas: &[f32]) -> VulkanTexture {
        let mut bytes = Vec::with_capacity(rgbas.len() * 4);
        for f in rgbas { bytes.extend_from_slice(&f.to_le_bytes()); }
        VulkanTexture::new(VulkanTextureData {
            width, height, mipMaps: 1, format: Some((TextureFormat::RGBA32, TexturePixel::Float)), bytes, levelStart: 0, levelStop: 1, clampS: false, clampT: false, deleted: false,
        })
    }

    fn defaultTexture() -> VulkanTexture {
        static DEFAULT: OnceLock<VulkanTexture> = OnceLock::new();
        DEFAULT.get_or_init(Self::checker).clone()
    }
}

impl TextureBuilder<VulkanBackend> for VulkanTextureBuilder {
    fn default(&self) -> VulkanTexture { Self::defaultTexture() }

    fn createNormalMap(&mut self, _src: &VulkanTexture, _strength: f32) -> VulkanTexture { VulkanTexture::empty() }

    fn createSolid(&mut self, width: i32, height: i32, rgbas: &[f32]) -> VulkanTexture { Self::solid(width, height, rgbas) }

    fn create(&mut self, reuse: Option<VulkanTexture>, src: &dyn ITexture, level2: Option<Range<i32>>) -> Result<VulkanTexture, Error> {
        let mut result: Result<VulkanTexture, Error> = Err(outOfRange("x", "null"));
        src.create("VK", &mut |x| result = (|| -> Result<VulkanTexture, Error> {
            let Some(t) = x.downcast_ref::<TextureAsBytes>() else { return Err(outOfRange("x", "not TextureAsBytes")); };
            if t.bytes.is_empty() { return Ok(Self::defaultTexture()); }
            let Some(&(formatx, pixel)) = t.format.downcast_ref::<(TextureFormat, TexturePixel)>() else { return Err(outOfRange("fmt", "not (TextureFormat, TexturePixel)")); };
            if !known(formatx) { return Err(outOfRange("TextureFormat", formatx)); }
            let numMipMaps = src.mipMaps().max(1);
            let start = level2.as_ref().map_or(0, |s| s.start);
            badSpan(&t.spans, start, numMipMaps)?;
            let tex = reuse.clone().unwrap_or_else(|| VulkanTexture::new(VulkanTextureData {
                width: 0, height: 0, mipMaps: 1, format: None, bytes: Vec::new(), levelStart: 0, levelStop: 1, clampS: false, clampT: false, deleted: false,
            }));
            {
                let mut d = tex.data.lock().unwrap();
                d.width = src.width();
                d.height = src.height();
                d.mipMaps = numMipMaps;
                d.format = Some((formatx, pixel));
                d.bytes = t.bytes.clone();
                d.levelStart = start;
                d.levelStop = numMipMaps;
                d.clampS = src.texFlags().contains(TextureFlags::SUGGEST_CLAMPS);
                d.clampT = src.texFlags().contains(TextureFlags::SUGGEST_CLAMPT);
                d.deleted = false;
            }
            Ok(tex)
        })())?;
        result
    }

    fn delete(&mut self, src: &VulkanTexture) {
        let mut d = src.data.lock().unwrap();
        d.deleted = true;
        d.bytes.clear();
    }
}

fn known(format: TextureFormat) -> bool {
    use TextureFormat::*;
    matches!(format, I8 | L8 | R8 | R16 | RG16 | RGB24 | RGB565 | RGBA32 | ARGB32 | BGRA32 | BGRA1555 | DXT1 | DXT1A | DXT3 | DXT5 | BC4 | BC5 | BC6H | BC7 | ETC2 | ETC2_EAC)
}

fn badSpan(spans: &[Range<usize>], start: i32, stop: i32) -> Result<(), Error> {
    if spans.is_empty() { return Ok(()); }
    for l in start..stop {
        if spans.get(l as usize).is_none() { return Err(outOfRange("spans", l)); }
    }
    Ok(())
}

/// C# type arguments `<object, object, VulkanTexture, object>`.
pub struct VulkanBackend;

impl openx_gfx::gfx::Backend for VulkanBackend {
    type Object = Option<openx_gfx::gfx::AnyArc>;
    type Material = ();
    type Texture = VulkanTexture;
    type Shader = ();
    type Sprite = ();
}

pub type VulkanTextureManager = TextureManager<VulkanBackend, VulkanTextureBuilder>;

/// VulkanGfxModel. Material, object, and shader managers stay unconstructed.
pub struct VulkanGfxModel {
    textureManager: VulkanTextureManager,
}

impl VulkanGfxModel {
    pub fn new() -> Self { Self { textureManager: TextureManager::new(VulkanTextureBuilder) } }

    pub fn textureManager(&mut self) -> &mut VulkanTextureManager { &mut self.textureManager }

    pub fn preloadTexture(&mut self, source: &SourceRef, path: &SourcePath) { self.textureManager.preload(source, path); }

    pub fn createTexture<'a>(&'a mut self, source: &'a SourceRef, path: &'a SourcePath, level: Option<Range<i32>>) -> BoxFuture<'a, Result<(VulkanTexture, std::sync::Arc<dyn ITexture>), Error>> {
        Box::pin(self.textureManager.create(source, path, level))
    }
}

/// VulkanEngine
pub struct VulkanEngine;

impl VulkanEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine { caps: EngineX::Caps::Drawing, ..Engine::new("VK", "Vulkan") });
        &This
    }

    /// Creates a Vulkan instance. No window and no swapchain.
    /// `Err` when the loader or an ICD is missing. Does not panic.
    pub fn createInstance() -> Result<VulkanInstance, String> { VulkanInstance::createInstance() }
}

/// VulkanShellEngine
pub struct VulkanShellEngine;

impl VulkanShellEngine {
    pub fn This() -> &'static Engine {
        static This: LazyLock<Engine> = LazyLock::new(|| Engine::new("VK", "Vulkan"));
        &This
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn registrationMatchesTheCSharp() {
        let s = VulkanEngine::This();
        assert_eq!((s.id, s.name, s.caps), ("VK", "Vulkan", EngineX::Caps::Drawing));
        let shell = VulkanShellEngine::This();
        assert_eq!((shell.id, shell.name), ("VK", "Vulkan"));
    }

    #[test]
    fn textureManagerBuildsASolid() {
        let mut model = VulkanGfxModel::new();
        let tex = model.textureManager().createSolid(1, 1, &[0.2, 0.3, 0.3, 1.0]);
        let data = tex.data.lock().unwrap();
        assert_eq!((data.width, data.height, data.bytes.len()), (1, 1, 16));
        drop(data);
        let a = model.textureManager().default();
        let b = model.textureManager().default();
        assert_eq!(a.id, b.id);
        assert_ne!(a.id, tex.id);
    }

    #[test]
    fn createInstanceDoesNotPanic() {
        match VulkanEngine::createInstance() {
            Ok(instance) => drop(instance),
            Err(err) => assert!(!err.is_empty()),
        }
    }
}
