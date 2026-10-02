// PORT-SOURCE: Gfx/OpenX.Gfx/Gfx.cs
// PORT-SHA: 0116b31de21c0a94
// PORT-STATUS: done
//
// ======================= HOW `(ISource, object path)` KEYS PORT =======================
//
// Every C# manager caches on `var key = (source, path)`, where `source` is an
// `ISource` reference. A C# reference compares by identity, so the key means
// "this source object and this path". `dyn ISource` cannot sit inside a Rust
// tuple key — it is unsized and has no identity of its own — which is where the
// earlier attempt stalled. The port uses `openx_poly::core::SourceRef`, an
// `Arc<dyn ISource>` that hashes and compares by pointer, so the key is the
// same identity the C# had.
//
// The same `Arc` fixes the lifetime problem on the preload tasks. C# stores
// `Task<T>` in a dictionary; the Rust future returned by `ISource::getAsset`
// borrows the source and the path, so it cannot be stored past the call. Each
// stored task therefore owns its `SourceRef` and its `SourcePath` (`load_asset`
// below) and is `'static`.
//
// Other deliberate departures from the C#:
//
//   * The C# caches are `static`, shared by every manager instance of a type.
//     Here each manager owns its cache, so two backends cannot see each other's
//     handles. `clear()` is provided; the C# caches only ever grew.
//   * C# `Preload` starts a hot `Task`, so the load runs in the background.
//     A Rust future is lazy: `preload` registers the load, and it runs when
//     `create` awaits it. A caller that wants background loading spawns the
//     `create` on its runtime; the managers themselves are runtime-agnostic.
//   * A preload task is removed from `tasks` before it is awaited, not after.
//     The C# left a faulted task wedged in the dictionary, so a failed load
//     could never be retried.
//   * `Reload` in the C# discards the handle the builder returns. Here the
//     cache is updated with it, so a backend that hands back a fresh handle
//     does not lose the reload.
//   * `path is ITexture z` — the C# lets an already-decoded asset stand in for
//     the path. `SourcePath` is a real path type, so that case is a separate
//     method (`createFrom`) that takes the decoded asset and a path to cache
//     it under.
//   * `ITexture.Create<T>(string platform, Func<object, T>)` and its siblings
//     on `ISprite` / `IMaterial` are not object-safe generics and have no
//     callers in the ported tree; they are dropped from the traits.
//
// Sources hand assets back through `ISource::getAssetAny` as `Box<dyn Any + Send + Sync>`.
// The managers downcast that to `Box<dyn ITexture>`, `Box<dyn ISprite>` or
// `MaterialProp`, so a source that produces a texture stores a
// `Box<dyn ITexture>` in the `Any` — the same contract as the C#
// `GetAsset<ITexture>` cast.

use std::any::Any;
use std::collections::HashMap;
use std::hash::{Hash, Hasher};
use std::io::Error;
use std::ops::Range;
use std::sync::Arc;
use std::sync::atomic::AtomicI32;

use glam::{Mat4, Quat, Vec2, Vec3, Vec4};
use openx_poly::core::{BoxFuture, ISourceExt, SourceKey, SourcePath, SourceRef};
use crate::gfx_texture::TextureFlags;

/// A boxed, thread-safe value — the C# `object` slots (`tag`, `mesh`, `data`).
pub type AnyBox = Box<dyn Any + Send + Sync>;
/// A shared `object` — what the caches hand back as `tag`.
pub type AnyArc = Arc<dyn Any + Send + Sync>;
/// A stored preload — C# `Dictionary<object, Task<T>>` value.
type LoadTask<T> = BoxFuture<'static, Result<T, Error>>;

/// C# `System.Drawing.Color`, as RGBA bytes.
pub type Color = [u8; 4];

//#region GfX

pub mod GfX {
    use super::AtomicI32;
    pub const XAPI: i32 = 0;
    pub const XSPRITE2D: i32 = 1;
    pub const XSPRITE3D: i32 = 2;
    pub const XMODEL: i32 = 3;
    pub const XLIGHT: i32 = 4;
    pub const XTERRAIN: i32 = 5;
    /// C# `public static int MaxTextureMaxAnisotropy`.
    #[allow(non_upper_case_globals)]
    pub static MaxTextureMaxAnisotropy: AtomicI32 = AtomicI32::new(0);
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum GfxAttach { Find, Transform, All, AllCenter }

/// GfxAlphaMode (glAlphaFunc)
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum GfxAlphaMode { Never = 0x0200, Less = 0x0201, Equal = 0x0202, LEqual = 0x0203, Greater = 0x0204, NotEqual = 0x0205, GEqual = 0x0206, Always = 0x0207 }

/// GfxBlendMode (glBlendFunc)
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum GfxBlendMode { Zero = 0, One = 1, SrcColor = 0x0300, OneMinusSrcColor = 0x0301, SrcAlpha = 0x0302, OneMinusSrcAlpha = 0x0303, DstAlpha = 0x0304, OneMinusDstAlpha = 0x0305, DstColor = 0x0306, OneMinusDstColor = 0x0307, SrcAlphaSaturate = 0x0308 }

/// The backend's handle types — the `B_Object`, `B_Material`, ... type
/// parameters the C# threads through every builder and manager.
pub trait Backend {
    type Object: Clone;
    type Material: Clone;
    type Texture: Clone + Eq + Hash;
    type Shader: Clone;
    type Sprite: Clone;
}

/// C# `source.GetAsset<T>(path)` as an owned, storable future.
fn load_asset<T: Any + Send + Sync>(source: &SourceRef, path: &SourcePath) -> LoadTask<T> {
    let source = source.clone();
    let path = path.clone();
    Box::pin(async move { source.getAsset::<T>(&path, None).await })
}

/// C# `source.GetAsset<object>(path)` as an owned, storable future.
fn load_any(source: &SourceRef, path: &SourcePath) -> LoadTask<AnyBox> {
    let source = source.clone();
    let path = path.clone();
    Box::pin(async move { source.getAssetAny(&path, None).await })
}

fn key(source: &SourceRef, path: &SourcePath) -> SourceKey { (source.clone(), path.clone()) }

//#endregion

//#region ObjectSprite

/// C# `abstract class ObjectSpriteBuilder<B_Object, B_Sprite>`.
pub trait ObjectSpriteBuilder<B: Backend> {
    fn ensure(&mut self);
    fn instance(&mut self, src: &B::Object, parent: Option<&B::Object>) -> B::Object;
    fn create(&mut self, src: &(dyn Any + Send + Sync)) -> B::Object;
}

/// C# `class ObjectSpriteManager<B_Object, B_Sprite>`.
pub struct ObjectSpriteManager<B: Backend, OB: ObjectSpriteBuilder<B>> {
    builder: OB,
    tasks: HashMap<SourceKey, LoadTask<AnyBox>>,
    cached: HashMap<SourceKey, (B::Object, AnyArc)>,
}

impl<B: Backend, OB: ObjectSpriteBuilder<B>> ObjectSpriteManager<B, OB> {
    pub fn new(builder: OB) -> Self {
        Self { builder, tasks: HashMap::new(), cached: HashMap::new() }
    }

    pub async fn create(&mut self, source: &SourceRef, path: &SourcePath, parent: Option<&B::Object>) -> Result<(B::Object, AnyArc), Error> {
        let key = key(source, path);
        if !self.cached.contains_key(&key) {
            let loaded = self.load(&key).await?;
            self.cached.insert(key.clone(), loaded);
        }
        let (obj, tag) = &self.cached[&key];
        Ok((self.builder.instance(obj, parent), tag.clone()))
    }

    pub fn preload(&mut self, source: &SourceRef, path: &SourcePath) {
        let key = key(source, path);
        if self.cached.contains_key(&key) { return; }
        self.tasks.entry(key).or_insert_with(|| load_any(source, path));
    }

    async fn load(&mut self, key: &SourceKey) -> Result<(B::Object, AnyArc), Error> {
        debug_assert!(!self.cached.contains_key(key));
        self.builder.ensure();
        self.preload(&key.0, &key.1);
        let task = self.tasks.remove(key).expect("preload registers the task");
        let obj = task.await?;
        let created = self.builder.create(&*obj);
        Ok((created, Arc::from(obj)))
    }

    pub fn clear(&mut self) { self.tasks.clear(); self.cached.clear(); }
}

//#endregion

//#region ObjectModel

/// C# `interface IObjectModel` — see the module header for why `Create<T>` is dropped.
pub trait IObjectModel: Send + Sync {}

/// C# `abstract class ObjectModelBuilder<B_Object, B_Material, B_Texture>`.
pub trait ObjectModelBuilder<B: Backend, TB: TextureBuilder<B>, MB: MaterialBuilder<B, TB>> {
    fn ensure(&mut self);
    fn instance(&mut self, src: &B::Object, parent: Option<&B::Object>) -> B::Object;
    fn create<'a>(&'a mut self, source: &'a SourceRef, src: &'a (dyn Any + Send + Sync), static_: bool, materialManager: &'a mut MaterialManager<B, TB, MB>) -> BoxFuture<'a, Result<B::Object, Error>>;
}

/// C# `class ObjectModelManager<B_Object, B_Material, B_Texture>`.
///
/// The C# `Create` swallows every exception into a log line and returns
/// `(default, null)`; the port returns the error.
pub struct ObjectModelManager<B: Backend, TB: TextureBuilder<B>, MB: MaterialBuilder<B, TB>, OB: ObjectModelBuilder<B, TB, MB>> {
    builder: OB,
    pub materialManager: MaterialManager<B, TB, MB>,
    tasks: HashMap<SourceKey, LoadTask<AnyBox>>,
    cached: HashMap<SourceKey, (B::Object, AnyArc)>,
}

impl<B: Backend, TB: TextureBuilder<B>, MB: MaterialBuilder<B, TB>, OB: ObjectModelBuilder<B, TB, MB>> ObjectModelManager<B, TB, MB, OB> {
    pub fn new(materialManager: MaterialManager<B, TB, MB>, builder: OB) -> Self {
        Self { builder, materialManager, tasks: HashMap::new(), cached: HashMap::new() }
    }

    pub async fn create(&mut self, source: &SourceRef, path: &SourcePath, static_: bool, parent: Option<&B::Object>) -> Result<(B::Object, AnyArc), Error> {
        let key = key(source, path);
        if !self.cached.contains_key(&key) {
            let loaded = self.load(&key, static_).await?;
            self.cached.insert(key.clone(), loaded);
        }
        let (obj, tag) = &self.cached[&key];
        Ok((self.builder.instance(obj, parent), tag.clone()))
    }

    pub fn preload(&mut self, source: &SourceRef, path: &SourcePath) {
        let key = key(source, path);
        if self.cached.contains_key(&key) { return; }
        self.tasks.entry(key).or_insert_with(|| load_any(source, path));
    }

    async fn load(&mut self, key: &SourceKey, static_: bool) -> Result<(B::Object, AnyArc), Error> {
        debug_assert!(!self.cached.contains_key(key));
        self.builder.ensure();
        self.preload(&key.0, &key.1);
        let task = self.tasks.remove(key).expect("preload registers the task");
        let obj = task.await?;
        let created = self.builder.create(&key.0, &*obj, static_, &mut self.materialManager).await?;
        Ok((created, Arc::from(obj)))
    }

    pub fn clear(&mut self) { self.tasks.clear(); self.cached.clear(); }
}

//#endregion

//#region Shader

/// C# `class GfxShader(Func<int, string, int> uniformLocation, Func<int, string, int> attribLocation)`.
pub struct GfxShader {
    _uniformLocation: Box<dyn Fn(i32, &str) -> i32 + Send + Sync>,
    _attribLocation: Box<dyn Fn(i32, &str) -> i32 + Send + Sync>,
    pub name: String,
    pub program: i32,
    pub parameters: HashMap<String, bool>,
    pub renderModes: Vec<String>,
    _uniforms: HashMap<String, i32>,
}

impl GfxShader {
    pub fn new(uniformLocation: impl Fn(i32, &str) -> i32 + Send + Sync + 'static, attribLocation: impl Fn(i32, &str) -> i32 + Send + Sync + 'static) -> Self {
        Self {
            _uniformLocation: Box::new(uniformLocation),
            _attribLocation: Box::new(attribLocation),
            name: String::new(),
            program: 0,
            parameters: HashMap::new(),
            renderModes: Vec::new(),
            _uniforms: HashMap::new(),
        }
    }

    pub fn uniformLocation(&mut self, name: &str) -> i32 {
        if let Some(&v) = self._uniforms.get(name) { return v; }
        let v = (self._uniformLocation)(self.program, name);
        self._uniforms.insert(name.to_string(), v);
        v
    }

    pub fn attribLocation(&self, name: &str) -> u32 { (self._attribLocation)(self.program, name) as u32 }
}

/// C# `abstract class ShaderBuilder<B_Shader>`.
pub trait ShaderBuilder<B: Backend> {
    fn create(&mut self, path: &SourcePath, args: &HashMap<String, bool>) -> B::Shader;
}

/// C# `class ShaderManager<B_Shader>`. The C# `Create` is `async` but never
/// awaits, and its `tag` is always `null`, so this is synchronous and returns
/// the shader alone.
pub struct ShaderManager<B: Backend, SB: ShaderBuilder<B>> {
    builder: SB,
    cached: HashMap<(SourceKey, String), B::Shader>,
}

impl<B: Backend, SB: ShaderBuilder<B>> ShaderManager<B, SB> {
    pub fn new(builder: SB) -> Self {
        Self { builder, cached: HashMap::new() }
    }

    pub fn create(&mut self, source: &SourceRef, path: &SourcePath, args: Option<&HashMap<String, bool>>) -> B::Shader {
        let argx = args.map_or(String::new(), |a| {
            let mut on: Vec<&str> = a.iter().filter(|(_, &v)| v).map(|(k, _)| k.as_str()).collect();
            on.sort_unstable(); // C# joins in dictionary order, which is not stable across runs
            on.join(",")
        });
        let key = (key(source, path), argx);
        if let Some(s) = self.cached.get(&key) { return s.clone(); }
        let empty = HashMap::new();
        let s = self.builder.create(path, args.unwrap_or(&empty));
        self.cached.insert(key, s.clone());
        s
    }

    pub fn clear(&mut self) { self.cached.clear(); }
}

//#endregion

//#region Sprite

/// C# `interface ISprite`.
pub trait ISprite: Send + Sync {
    fn width(&self) -> i32;
    fn height(&self) -> i32;
}

/// C# `abstract class SpriteBuilder<B_Sprite>`.
pub trait SpriteBuilder<B: Backend> {
    fn default(&self) -> B::Sprite;
    fn create(&mut self, spr: &dyn ISprite) -> B::Sprite;
    fn delete(&mut self, spr: &B::Sprite);
}

/// C# `class SpriteManager<B_Sprite>`.
pub struct SpriteManager<B: Backend, SB: SpriteBuilder<B>> {
    builder: SB,
    tasks: HashMap<SourceKey, LoadTask<Box<dyn ISprite>>>,
    cached: HashMap<SourceKey, (B::Sprite, Arc<dyn ISprite>)>,
}

impl<B: Backend, SB: SpriteBuilder<B>> SpriteManager<B, SB> {
    pub fn new(builder: SB) -> Self {
        Self { builder, tasks: HashMap::new(), cached: HashMap::new() }
    }

    pub fn default(&self) -> B::Sprite { self.builder.default() }

    pub async fn create(&mut self, source: &SourceRef, path: &SourcePath) -> Result<(B::Sprite, Arc<dyn ISprite>), Error> {
        let key = key(source, path);
        if let Some(c) = self.cached.get(&key) { return Ok(c.clone()); }
        let tag: Arc<dyn ISprite> = Arc::from(self.load(&key).await?);
        Ok(self.insert(key, tag))
    }

    /// C# `Create` with `path is ISprite z` — an already-decoded sprite, cached under `path`.
    pub fn createFrom(&mut self, source: &SourceRef, path: &SourcePath, spr: Arc<dyn ISprite>) -> (B::Sprite, Arc<dyn ISprite>) {
        let key = key(source, path);
        if let Some(c) = self.cached.get(&key) { return c.clone(); }
        self.insert(key, spr)
    }

    fn insert(&mut self, key: SourceKey, tag: Arc<dyn ISprite>) -> (B::Sprite, Arc<dyn ISprite>) {
        let obj = self.builder.create(&*tag);
        self.cached.insert(key, (obj.clone(), tag.clone()));
        (obj, tag)
    }

    pub fn preload(&mut self, source: &SourceRef, path: &SourcePath) {
        let key = key(source, path);
        if self.cached.contains_key(&key) { return; }
        self.tasks.entry(key).or_insert_with(|| load_asset::<Box<dyn ISprite>>(source, path));
    }

    pub fn delete(&mut self, source: &SourceRef, path: &SourcePath) {
        let key = key(source, path);
        let Some((spr, _)) = self.cached.remove(&key) else { return; };
        self.builder.delete(&spr);
    }

    async fn load(&mut self, key: &SourceKey) -> Result<Box<dyn ISprite>, Error> {
        debug_assert!(!self.cached.contains_key(key));
        self.preload(&key.0, &key.1);
        let task = self.tasks.remove(key).expect("preload registers the task");
        task.await
    }

    pub fn clear(&mut self) {
        for (spr, _) in self.cached.values() { self.builder.delete(spr); }
        self.tasks.clear();
        self.cached.clear();
    }
}

//#endregion

//#region Texture

/// C# `struct TextureAsDds(byte[] bytes)`.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct TextureAsDds {
    pub bytes: Vec<u8>,
}

/// C# `struct TextureAsBytes(byte[] bytes, object format, Range[] spans)`.
#[derive(Clone)]
pub struct TextureAsBytes {
    pub bytes: Vec<u8>,
    pub format: AnyArc,
    pub spans: Vec<Range<usize>>,
}

/// C# `interface ITexture`.
pub trait ITexture: Send + Sync {
    fn width(&self) -> i32;
    fn height(&self) -> i32;
    fn depth(&self) -> i32;
    fn mipMaps(&self) -> i32;
    fn texFlags(&self) -> TextureFlags;
}

/// C# `interface ITextureSelect : ITexture`.
pub trait ITextureSelect: ITexture {
    fn maxId(&self) -> i32;
    fn select(&mut self, id: i32);
}

/// C# `interface ITextureFrames : ITexture`.
pub trait ITextureFrames: ITexture {
    fn fps(&self) -> i32;
    fn hasFrames(&self) -> bool;
    fn nextFrame(&mut self) -> bool;
}

/// C# `abstract class TextureBuilder<B_Texture>`.
pub trait TextureBuilder<B: Backend> {
    fn default(&self) -> B::Texture;
    fn createNormalMap(&mut self, src: &B::Texture, strength: f32) -> B::Texture;
    fn createSolid(&mut self, width: i32, height: i32, rgbas: &[f32]) -> B::Texture;
    fn create(&mut self, reuse: Option<B::Texture>, src: &dyn ITexture, level: Option<Range<i32>>) -> B::Texture;
    fn delete(&mut self, src: &B::Texture);
}

/// C# `TextureBuilder<B_Texture>.MaxTextureMaxAnisotropy`.
pub fn maxTextureMaxAnisotropy() -> i32 { GfX::MaxTextureMaxAnisotropy.load(std::sync::atomic::Ordering::Relaxed) }

/// C# `class Solid(int width, int height, float[] rgbas)`.
///
/// The C# class has no `Equals`/`GetHashCode`, so its solid cache never hit:
/// every lookup allocated a fresh key that matched nothing. Value equality here.
#[derive(Debug, Clone, PartialEq)]
struct SolidKey {
    width: i32,
    height: i32,
    rgbas: Vec<f32>,
}
impl Eq for SolidKey {}
impl Hash for SolidKey {
    fn hash<H: Hasher>(&self, state: &mut H) {
        self.width.hash(state);
        self.height.hash(state);
        for f in &self.rgbas { f.to_bits().hash(state); }
    }
}

/// C# `const float NormalMapIntensity = 0.75f`.
pub const NORMAL_MAP_INTENSITY: f32 = 0.75;

/// C# `class TextureManager<B_Texture>`.
pub struct TextureManager<B: Backend, TB: TextureBuilder<B>> {
    builder: TB,
    tasks: HashMap<SourceKey, LoadTask<Box<dyn ITexture>>>,
    cachedNormalMaps: HashMap<B::Texture, B::Texture>,
    cachedSolids: HashMap<SolidKey, B::Texture>,
    cached: HashMap<SourceKey, (B::Texture, Arc<dyn ITexture>)>,
}

impl<B: Backend, TB: TextureBuilder<B>> TextureManager<B, TB> {
    pub fn new(builder: TB) -> Self {
        Self { builder, tasks: HashMap::new(), cachedNormalMaps: HashMap::new(), cachedSolids: HashMap::new(), cached: HashMap::new() }
    }

    pub fn default(&self) -> B::Texture { self.builder.default() }

    /// C# `CreateNormalMap(B_Texture src, float strength = -1)`: a negative strength selects the default.
    pub fn createNormalMap(&mut self, src: &B::Texture, strength: f32) -> B::Texture {
        if let Some(t) = self.cachedNormalMaps.get(src) { return t.clone(); }
        let t = self.builder.createNormalMap(src, if strength < 0.0 { NORMAL_MAP_INTENSITY } else { strength });
        self.cachedNormalMaps.insert(src.clone(), t.clone());
        t
    }

    pub fn createSolid(&mut self, width: i32, height: i32, rgbas: &[f32]) -> B::Texture {
        let key = SolidKey { width, height, rgbas: rgbas.to_vec() };
        if let Some(t) = self.cachedSolids.get(&key) { return t.clone(); }
        let t = self.builder.createSolid(width, height, rgbas);
        self.cachedSolids.insert(key, t.clone());
        t
    }

    pub async fn create(&mut self, source: &SourceRef, path: &SourcePath, level: Option<Range<i32>>) -> Result<(B::Texture, Arc<dyn ITexture>), Error> {
        let key = key(source, path);
        if let Some(c) = self.cached.get(&key) { return Ok(c.clone()); }
        let tag: Arc<dyn ITexture> = Arc::from(self.load(&key).await?);
        Ok(self.insert(key, tag, level))
    }

    /// C# `Create` with `path is ITexture z` — an already-decoded texture, cached under `path`.
    pub fn createFrom(&mut self, source: &SourceRef, path: &SourcePath, tex: Arc<dyn ITexture>, level: Option<Range<i32>>) -> (B::Texture, Arc<dyn ITexture>) {
        let key = key(source, path);
        if let Some(c) = self.cached.get(&key) { return c.clone(); }
        self.insert(key, tex, level)
    }

    fn insert(&mut self, key: SourceKey, tag: Arc<dyn ITexture>, level: Option<Range<i32>>) -> (B::Texture, Arc<dyn ITexture>) {
        let obj = self.builder.create(None, &*tag, level);
        self.cached.insert(key, (obj.clone(), tag.clone()));
        (obj, tag)
    }

    /// C# `Reload(ISource source, object path, Range? level)`. `None` when the path was never loaded.
    pub fn reload(&mut self, source: &SourceRef, path: &SourcePath, level: Option<Range<i32>>) -> Option<(B::Texture, Arc<dyn ITexture>)> {
        let key = key(source, path);
        let (existing, tag) = self.cached.get(&key)?.clone();
        let t = self.builder.create(Some(existing), &*tag, level);
        self.cached.insert(key, (t.clone(), tag.clone()));
        Some((t, tag))
    }

    pub fn preload(&mut self, source: &SourceRef, path: &SourcePath) {
        let key = key(source, path);
        if self.cached.contains_key(&key) { return; }
        self.tasks.entry(key).or_insert_with(|| load_asset::<Box<dyn ITexture>>(source, path));
    }

    pub fn delete(&mut self, source: &SourceRef, path: &SourcePath) {
        let key = key(source, path);
        let Some((tex, _)) = self.cached.remove(&key) else { return; };
        self.builder.delete(&tex);
    }

    async fn load(&mut self, key: &SourceKey) -> Result<Box<dyn ITexture>, Error> {
        debug_assert!(!self.cached.contains_key(key));
        self.preload(&key.0, &key.1);
        let task = self.tasks.remove(key).expect("preload registers the task");
        task.await
    }

    pub fn clear(&mut self) {
        for (t, _) in self.cached.values() { self.builder.delete(t); }
        self.tasks.clear();
        self.cached.clear();
        self.cachedNormalMaps.clear();
        self.cachedSolids.clear();
    }

    pub fn len(&self) -> usize { self.cached.len() }
    pub fn is_empty(&self) -> bool { self.cached.is_empty() }
}

//#endregion

//#region Material

/// C# `interface IMaterial` — see the module header for why `Create<T>` is dropped.
pub trait IMaterial: Send + Sync {}

/// C# `class MaterialStdProp : MaterialProp`.
#[derive(Clone, Default)]
pub struct MaterialStdProp {
    pub textures: HashMap<String, AnyArc>,
    pub alphaBlended: bool,
    pub srcBlendMode: Option<GfxBlendMode>,
    pub dstBlendMode: Option<GfxBlendMode>,
    pub alphaTest: bool,
    pub alphaCutoff: f32,
}

/// C# `class MaterialStd2Prop : MaterialStdProp`.
#[derive(Clone, Default)]
pub struct MaterialStd2Prop {
    pub std: MaterialStdProp,
    pub zWrite: bool,
    pub diffuseColor: Color,
    pub specularColor: Color,
    pub emissiveColor: Color,
    pub glossiness: f32,
    pub alpha: f32,
}

/// C# `class MaterialShaderProp : MaterialProp`.
#[derive(Clone, Default)]
pub struct MaterialShaderProp {
    pub shaderName: String,
    pub shaderArgs: HashMap<String, bool>,
}

/// C# `class MaterialShaderVProp : MaterialShaderProp`.
#[derive(Clone, Default)]
pub struct MaterialShaderVProp {
    pub shader: MaterialShaderProp,
    pub intParams: HashMap<String, i64>,
    pub floatParams: HashMap<String, f32>,
    pub vectorParams: HashMap<String, Vec4>,
    pub textureParams: HashMap<String, String>,
    pub intAttributes: HashMap<String, i64>,
}

/// C# `abstract class MaterialProp { object Tag; }` and its four subclasses.
#[derive(Clone)]
pub enum MaterialPropKind {
    Std(MaterialStdProp),
    Std2(MaterialStd2Prop),
    Shader(MaterialShaderProp),
    ShaderV(MaterialShaderVProp),
}

/// C# `abstract class MaterialProp`.
#[derive(Clone)]
pub struct MaterialProp {
    pub tag: Option<AnyArc>,
    pub kind: MaterialPropKind,
}

/// C# `abstract class MaterialBuilder<B_Material, B_Texture>(TextureManager<B_Texture> textureManager)`.
///
/// The C# builder holds the texture manager it was constructed with; here the
/// manager that owns it is passed to `create`, which is the only place it is used.
pub trait MaterialBuilder<B: Backend, TB: TextureBuilder<B>> {
    fn default(&self) -> B::Material;
    fn terrain(&self) -> B::Material;
    fn create<'a>(&'a mut self, source: &'a SourceRef, prop: &'a MaterialProp, textureManager: &'a mut TextureManager<B, TB>) -> BoxFuture<'a, Result<B::Material, Error>>;
}

/// C# `class MaterialManager<B_Material, B_Texture>` — manages loading and instantiation of materials.
pub struct MaterialManager<B: Backend, TB: TextureBuilder<B>, MB: MaterialBuilder<B, TB>> {
    builder: MB,
    pub textureManager: TextureManager<B, TB>,
    tasks: HashMap<SourceKey, LoadTask<MaterialProp>>,
    cached: HashMap<SourceKey, (B::Material, Option<AnyArc>)>,
}

impl<B: Backend, TB: TextureBuilder<B>, MB: MaterialBuilder<B, TB>> MaterialManager<B, TB, MB> {
    pub fn new(textureManager: TextureManager<B, TB>, builder: MB) -> Self {
        Self { builder, textureManager, tasks: HashMap::new(), cached: HashMap::new() }
    }

    pub fn default(&self) -> B::Material { self.builder.default() }
    pub fn terrain(&self) -> B::Material { self.builder.terrain() }

    pub async fn create(&mut self, source: &SourceRef, path: &SourcePath) -> Result<(B::Material, Option<AnyArc>), Error> {
        let key = key(source, path);
        if let Some(c) = self.cached.get(&key) { return Ok(c.clone()); }
        let prop = self.load(&key).await?;
        self.insert(key, prop).await
    }

    /// C# `Create` with `path is MaterialProp z` — an already-decoded material, cached under `path`.
    pub async fn createFrom(&mut self, source: &SourceRef, path: &SourcePath, prop: MaterialProp) -> Result<(B::Material, Option<AnyArc>), Error> {
        let key = key(source, path);
        if let Some(c) = self.cached.get(&key) { return Ok(c.clone()); }
        self.insert(key, prop).await
    }

    async fn insert(&mut self, key: SourceKey, prop: MaterialProp) -> Result<(B::Material, Option<AnyArc>), Error> {
        let obj = self.builder.create(&key.0, &prop, &mut self.textureManager).await?;
        let entry = (obj, prop.tag);
        self.cached.insert(key, entry.clone());
        Ok(entry)
    }

    pub fn preload(&mut self, source: &SourceRef, path: &SourcePath) {
        let key = key(source, path);
        if self.cached.contains_key(&key) { return; }
        self.tasks.entry(key).or_insert_with(|| load_asset::<MaterialProp>(source, path));
    }

    async fn load(&mut self, key: &SourceKey) -> Result<MaterialProp, Error> {
        debug_assert!(!self.cached.contains_key(key));
        self.preload(&key.0, &key.1);
        let task = self.tasks.remove(key).expect("preload registers the task");
        task.await
    }

    pub fn clear(&mut self) { self.tasks.clear(); self.cached.clear(); }
}

//#endregion

//#region OpenGfx

/// C# `interface IOpenGfx`.
pub trait IOpenGfx {}

/// C# `interface IOpenGfxApi<B_Object, B_Material>`.
pub trait IOpenGfxApi<B: Backend>: IOpenGfx {
    fn createObject(&mut self, name: &str, tag: Option<&str>, parent: Option<&B::Object>) -> B::Object;
    fn createMesh(&mut self, mesh: &(dyn Any + Send + Sync)) -> AnyBox;
    fn addMeshRenderer(&mut self, src: &B::Object, mesh: &(dyn Any + Send + Sync), material: &B::Material, enabled: bool, isStatic: bool);
    fn addMeshCollider(&mut self, src: &B::Object, mesh: &(dyn Any + Send + Sync), isKinematic: bool, isStatic: bool);
    fn attach(&mut self, method: GfxAttach, src: &B::Object, args: &[&(dyn Any + Send + Sync)]);
    fn parent(&mut self, src: &B::Object, parent: &B::Object);
    fn transform(&mut self, src: &B::Object, position: Vec3, rotation: Quat, localScale: Vec3);
    /// C# `Transform(B_Object src, Vector3 position, Matrix4x4 rotation, Vector3 localScale)`.
    fn transformM(&mut self, src: &B::Object, position: Vec3, rotation: Mat4, localScale: Vec3);
    fn addMissingMeshCollidersRecursively(&mut self, src: &B::Object, isStatic: bool);
    fn setLayerRecursively(&mut self, src: &B::Object, layer: i32);
    fn setVisible(&mut self, src: &B::Object, visible: bool);
    fn destroy(&mut self, src: &B::Object);
}

/// C# `interface IOpenGfxSprite<B_Object, B_Sprite>`.
pub trait IOpenGfxSprite<B: Backend>: IOpenGfx {
    type SpriteBuilder: SpriteBuilder<B>;
    fn spriteManager(&mut self) -> &mut SpriteManager<B, Self::SpriteBuilder>;
    fn preload(&mut self, source: &SourceRef, path: &SourcePath);
    fn create<'a>(&'a mut self, source: &'a SourceRef, path: &'a SourcePath, parent: Option<&'a B::Object>) -> BoxFuture<'a, Result<(B::Sprite, Arc<dyn ISprite>), Error>>;
}

/// C# `interface IOpenGfxModel<B_Object, B_Material, B_Texture, B_Shader>`.
pub trait IOpenGfxModel<B: Backend>: IOpenGfx {
    type TextureBuilder: TextureBuilder<B>;
    type MaterialBuilder: MaterialBuilder<B, Self::TextureBuilder>;
    type ObjectBuilder: ObjectModelBuilder<B, Self::TextureBuilder, Self::MaterialBuilder>;
    type ShaderBuilder: ShaderBuilder<B>;
    fn materialManager(&mut self) -> &mut MaterialManager<B, Self::TextureBuilder, Self::MaterialBuilder>;
    fn objectManager(&mut self) -> &mut ObjectModelManager<B, Self::TextureBuilder, Self::MaterialBuilder, Self::ObjectBuilder>;
    fn shaderManager(&mut self) -> &mut ShaderManager<B, Self::ShaderBuilder>;
    fn textureManager(&mut self) -> &mut TextureManager<B, Self::TextureBuilder>;
    fn preload(&mut self, source: &SourceRef, path: &SourcePath);
    fn preloadTexture(&mut self, source: &SourceRef, path: &SourcePath);
    fn create<'a>(&'a mut self, source: &'a SourceRef, path: &'a SourcePath, isStatic: bool, parent: Option<&'a B::Object>) -> BoxFuture<'a, Result<(B::Object, AnyArc), Error>>;
    fn createShader(&mut self, source: &SourceRef, path: &SourcePath, args: Option<&HashMap<String, bool>>) -> B::Shader;
    fn createTexture<'a>(&'a mut self, source: &'a SourceRef, path: &'a SourcePath, level: Option<Range<i32>>) -> BoxFuture<'a, Result<(B::Texture, Arc<dyn ITexture>), Error>>;
    fn post(&mut self, src: &B::Object, position: Vec3, eulerAngles: Vec3, scale: Option<f32>, parent: Option<&B::Object>);
}

/// C# `interface IOpenGfxLight<B_Object>`.
pub trait IOpenGfxLight<B: Backend>: IOpenGfx {
    fn create(&mut self, name: &str, position: Option<Vec3>, radius: f32, color: Color, indoors: bool, parent: Option<&B::Object>) -> B::Object;
    fn createProbe(&mut self, name: &str, position: Option<Vec3>, parent: Option<&B::Object>) -> B::Object;
}

/// C# `class GfxTerrainLayer<B_Texture>`.
#[derive(Clone)]
pub struct GfxTerrainLayer<B: Backend> {
    pub texture: B::Texture,
    pub smoothness: f32,
    pub metallic: f32,
    pub specular: Color,
    pub maskMapTexture: Option<B::Texture>,
    pub normalMapTexture: Option<B::Texture>,
    pub tileSize: Vec2,
}

/// C# `interface IOpenGfxTerrain<B_Object, B_Material, B_Texture>`.
///
/// `float[,] heights` is `heights[row][col]`; `float[,,] alphaMap` is `alphaMap[row][col][layer]`.
pub trait IOpenGfxTerrain<B: Backend>: IOpenGfx {
    fn createData(&mut self, offset: i32, heights: &[Vec<f32>], heightRange: f32, sampleDistance: f32, layers: &[GfxTerrainLayer<B>], alphaMap: &[Vec<Vec<f32>>]) -> AnyBox;
    fn create(&mut self, name: &str, position: Option<Vec3>, data: &(dyn Any + Send + Sync), parent: Option<&B::Object>) -> B::Object;
}

//#endregion

#[cfg(test)]
mod tests {
    use super::*;
    use std::future::Future;
    use std::io::ErrorKind;
    use std::sync::atomic::{AtomicU32, Ordering};
    use std::task::{Context, Poll, RawWaker, RawWakerVTable, Waker};
    use openx_poly::core::ISource;

    /// The managers are runtime-agnostic; the tests only need to drive a
    /// future that never actually waits.
    fn block_on<F: Future>(f: F) -> F::Output {
        fn raw() -> RawWaker { RawWaker::new(std::ptr::null(), &VTABLE) }
        static VTABLE: RawWakerVTable = RawWakerVTable::new(|_| raw(), |_| {}, |_| {}, |_| {});
        let waker = unsafe { Waker::from_raw(raw()) };
        let mut cx = Context::from_waker(&waker);
        let mut f = std::pin::pin!(f);
        loop {
            if let Poll::Ready(v) = f.as_mut().poll(&mut cx) { return v; }
        }
    }

    #[derive(Debug, Clone, PartialEq, Eq, Hash)]
    struct H(u32);

    struct Test;
    impl Backend for Test {
        type Object = H;
        type Material = H;
        type Texture = H;
        type Shader = H;
        type Sprite = H;
    }

    struct FakeTex(i32);
    impl ITexture for FakeTex {
        fn width(&self) -> i32 { self.0 }
        fn height(&self) -> i32 { 4 }
        fn depth(&self) -> i32 { 1 }
        fn mipMaps(&self) -> i32 { 1 }
        fn texFlags(&self) -> TextureFlags { TextureFlags::empty() }
    }

    struct FakeSprite;
    impl ISprite for FakeSprite {
        fn width(&self) -> i32 { 8 }
        fn height(&self) -> i32 { 8 }
    }

    /// A source that serves textures and sprites by path, counting loads.
    #[derive(Default)]
    struct FakeSource { loads: AtomicU32 }
    impl ISource for FakeSource {
        fn getAssetAny<'a>(&'a self, path: &'a SourcePath, _option: Option<&'a (dyn Any + Sync)>) -> BoxFuture<'a, Result<Box<dyn Any + Send + Sync>, Error>> {
            self.loads.fetch_add(1, Ordering::Relaxed);
            Box::pin(async move {
                match path {
                    SourcePath::Str(s) if s.ends_with(".tex") => Ok(Box::new(Box::new(FakeTex(s.len() as i32)) as Box<dyn ITexture>) as Box<dyn Any + Send + Sync>),
                    SourcePath::Str(s) if s.ends_with(".spr") => Ok(Box::new(Box::new(FakeSprite) as Box<dyn ISprite>) as Box<dyn Any + Send + Sync>),
                    _ => Err(Error::new(ErrorKind::NotFound, format!("{path:?}"))),
                }
            })
        }
    }

    #[derive(Default)]
    struct Counter { next: u32, creates: u32, deletes: u32 }
    impl Counter {
        fn issue(&mut self) -> H { self.next += 1; self.creates += 1; H(self.next) }
    }

    struct TexBuilder(Counter);
    impl TextureBuilder<Test> for TexBuilder {
        fn default(&self) -> H { H(0) }
        fn createNormalMap(&mut self, _src: &H, _strength: f32) -> H { self.0.issue() }
        fn createSolid(&mut self, _w: i32, _h: i32, _rgbas: &[f32]) -> H { self.0.issue() }
        fn create(&mut self, _reuse: Option<H>, _src: &dyn ITexture, _level: Option<Range<i32>>) -> H { self.0.issue() }
        fn delete(&mut self, _src: &H) { self.0.deletes += 1; }
    }

    struct SprBuilder(Counter);
    impl SpriteBuilder<Test> for SprBuilder {
        fn default(&self) -> H { H(0) }
        fn create(&mut self, _spr: &dyn ISprite) -> H { self.0.issue() }
        fn delete(&mut self, _spr: &H) { self.0.deletes += 1; }
    }

    fn source() -> SourceRef { SourceRef::new(FakeSource::default()) }
    fn loads(s: &SourceRef) -> u32 {
        // `SourceRef` derefs to `dyn ISource`; the test wants the concrete type.
        let any: &FakeSource = unsafe { &*(Arc::as_ptr(&s.0) as *const FakeSource) };
        any.loads.load(Ordering::Relaxed)
    }

    #[test]
    fn sourceRef_isIdentityNotValue() {
        let a = source();
        let b = source();
        assert_eq!(a, a.clone(), "a clone points at the same source");
        assert_ne!(a, b, "two sources with equal contents are still two sources");
    }

    #[test]
    fn texture_create_cachesBySourceAndPath() {
        let src = source();
        let mut m = TextureManager::<Test, _>::new(TexBuilder(Counter::default()));
        let (a, _) = block_on(m.create(&src, &"a.tex".into(), None)).unwrap();
        let (b, _) = block_on(m.create(&src, &"a.tex".into(), None)).unwrap();
        let (c, _) = block_on(m.create(&src, &"b.tex".into(), None)).unwrap();
        assert_eq!(a, b);
        assert_ne!(a, c);
        assert_eq!(m.builder.0.creates, 2);
        assert_eq!(loads(&src), 2, "the second `a.tex` never hit the source");
    }

    #[test]
    fn texture_samePathOnAnotherSourceIsAnotherEntry() {
        let s1 = source();
        let s2 = source();
        let mut m = TextureManager::<Test, _>::new(TexBuilder(Counter::default()));
        let (a, _) = block_on(m.create(&s1, &"a.tex".into(), None)).unwrap();
        let (b, _) = block_on(m.create(&s2, &"a.tex".into(), None)).unwrap();
        assert_ne!(a, b);
        assert_eq!(m.len(), 2);
    }

    #[test]
    fn texture_preloadThenCreate_loadsOnce() {
        let src = source();
        let mut m = TextureManager::<Test, _>::new(TexBuilder(Counter::default()));
        m.preload(&src, &"a.tex".into());
        m.preload(&src, &"a.tex".into());
        assert_eq!(m.tasks.len(), 1, "a second preload joins the first task");
        assert_eq!(loads(&src), 0, "a Rust future is lazy: nothing has hit the source yet");
        block_on(m.create(&src, &"a.tex".into(), None)).unwrap();
        assert_eq!(loads(&src), 1);
        assert!(m.tasks.is_empty(), "the task is consumed by the load");
    }

    #[test]
    fn texture_failedLoad_isRetryable() {
        let src = source();
        let mut m = TextureManager::<Test, _>::new(TexBuilder(Counter::default()));
        assert!(block_on(m.create(&src, &"missing".into(), None)).is_err());
        assert!(m.tasks.is_empty(), "a faulted task must not stay wedged");
        assert!(block_on(m.create(&src, &"missing".into(), None)).is_err());
        assert_eq!(loads(&src), 2);
    }

    #[test]
    fn texture_createFrom_usesTheDecodedAsset() {
        let src = source();
        let mut m = TextureManager::<Test, _>::new(TexBuilder(Counter::default()));
        let (a, tag) = m.createFrom(&src, &"a.tex".into(), Arc::new(FakeTex(99)), None);
        assert_eq!(tag.width(), 99);
        let (b, _) = block_on(m.create(&src, &"a.tex".into(), None)).unwrap();
        assert_eq!(a, b, "the path is now cached, so create does not load");
        assert_eq!(loads(&src), 0);
    }

    #[test]
    fn texture_reloadKeepsTheHandleTheBuilderReturned() {
        let src = source();
        let mut m = TextureManager::<Test, _>::new(TexBuilder(Counter::default()));
        let (first, _) = block_on(m.create(&src, &"a.tex".into(), None)).unwrap();
        let (reloaded, _) = m.reload(&src, &"a.tex".into(), None).unwrap();
        assert_ne!(first, reloaded, "builder issued a new handle");
        let (again, _) = block_on(m.create(&src, &"a.tex".into(), None)).unwrap();
        assert_eq!(again, reloaded, "cache updated");
        assert!(m.reload(&src, &"never".into(), None).is_none());
    }

    #[test]
    fn texture_createSolid_cacheActuallyHits() {
        let mut m = TextureManager::<Test, _>::new(TexBuilder(Counter::default()));
        let a = m.createSolid(1, 1, &[1.0, 0.0, 0.0, 1.0]);
        let b = m.createSolid(1, 1, &[1.0, 0.0, 0.0, 1.0]);
        m.createSolid(1, 1, &[0.0, 1.0, 0.0, 1.0]);
        assert_eq!(a, b);
        assert_eq!(m.builder.0.creates, 2);
    }

    #[test]
    fn texture_negativeNormalStrengthSelectsTheDefault() {
        let mut m = TextureManager::<Test, _>::new(TexBuilder(Counter::default()));
        m.createNormalMap(&H(99), -1.0);
        m.createNormalMap(&H(99), -1.0);
        assert_eq!(m.builder.0.creates, 1);
    }

    #[test]
    fn texture_deleteAndClear_releaseHandles() {
        let src = source();
        let mut m = TextureManager::<Test, _>::new(TexBuilder(Counter::default()));
        block_on(m.create(&src, &"a.tex".into(), None)).unwrap();
        block_on(m.create(&src, &"b.tex".into(), None)).unwrap();
        m.delete(&src, &"a.tex".into());
        assert_eq!(m.len(), 1);
        m.clear();
        assert!(m.is_empty());
        assert_eq!(m.builder.0.deletes, 2);
    }

    #[test]
    fn sprite_create_cachesBySourceAndPath() {
        let src = source();
        let mut m = SpriteManager::<Test, _>::new(SprBuilder(Counter::default()));
        let (a, tag) = block_on(m.create(&src, &"a.spr".into())).unwrap();
        let (b, _) = block_on(m.create(&src, &"a.spr".into())).unwrap();
        assert_eq!(a, b);
        assert_eq!(tag.width(), 8);
        assert_eq!(loads(&src), 1);
        m.delete(&src, &"a.spr".into());
        assert_eq!(m.builder.0.deletes, 1);
    }

    #[test]
    fn shader_variantsKeyOnTheirDefines() {
        struct SB;
        impl ShaderBuilder<Test> for SB {
            fn create(&mut self, _p: &SourcePath, args: &HashMap<String, bool>) -> H { H(args.values().filter(|v| **v).count() as u32) }
        }
        let src = source();
        let mut m = ShaderManager::<Test, _>::new(SB);
        let mut on = HashMap::new();
        on.insert("HAS_NORMAL".to_string(), true);
        let plain = m.create(&src, &"std".into(), None);
        let with = m.create(&src, &"std".into(), Some(&on));
        let with2 = m.create(&src, &"std".into(), Some(&on));
        assert_ne!(plain, with, "defines must not collide in the cache");
        assert_eq!(with, with2);
    }
}
