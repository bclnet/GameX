// PORT-SOURCE: Families/GameX.ID/Formats/Binary.cs
// PORT-SHA: b289df2afadc91c2
// PORT-STATUS: partial
//
// Ported: `Binary_Pak`, `Binary_Wad` (the `.wad` child `Binary_Pak` opens),
// `Binary_Lmp`, `Binary_Spr`, and `Binary_Mdl` (its constructor is empty, as in C#).
// `Binary_BspX`'s constructor and the three header `Read`s it calls (`X_DHeader_H1`,
// `X_DHeader_S2`, `X_DHeader_3`), plus `CopyLump` and the lump structs those reads
// load. Each `Read` returns null, so `h1` / `s2` / `x3` stay null after a success.
// Not ported: `Binary_BspY`, `Ent`, the miptex lumps, `PLANE` / `AMBIENT_H1` /
// `CONTENTS_*`, and `X_LumpNO` / `X_LumpNO2` / `X_Lump2NO`. `X_DModel_S2` is only
// its size (48): S2 loads `X_DModel_H1` with that count. PORT_MAP stays `todo`.
// No Python twin of `Binary_BspX`.
//
// `Binary_Lmp` keeps C#'s process-wide `Palette` and `Colormap`. A picture
// with no palette throws `NotImplementedException`. `ToLightPixel(32 << 3, index)`
// uses colormap row 0. The stored pixels are RGB plus a 0 byte, and the format
// is still `BGRA32`. A palette lump's `Pixels` is null in C#; here it is empty.
// `Binary_Spr.NextFrame` always returns true and then steps past the last frame.
// Python has no `Binary_Lmp`. Python's `Binary_Spr` sizes `bytes` with `<< 4`
// (C# is `<< 2`) and its `create` reads the name `format`.

use std::any::Any;
use std::io::{self, Cursor, Error, ErrorKind, Seek, SeekFrom};
use std::mem::size_of;
use std::sync::{Arc, LazyLock, Mutex, Weak};
use bytemuck::{Pod, Zeroable};
use gamex::binary::{newArchive, ArcBinary, Archive, BinaryArchive, BinaryState, Stream, SubArchive, Task};
use gamex::meta::{fileName, FileSource, IHaveMetaInfo, MetaContent, MetaInfo, MetaManager, Tag};
use glam::{I16Vec2, I16Vec3, IVec2, IVec3, U16Vec2, Vec3};
use openx_gfx::gfx::{AnyArc, ITexture, ITextureFrames, TextureAsBytes};
use openx_gfx::gfx_render::blitByPalette;
use openx_gfx::gfx_texture::{TextureFlags, TextureFormat, TexturePixel};
use openx_poly::prelude::{BinaryReaderExt, IStruct};
use openx_poly::system::unsafe_x::FixedAStringScan;
use openx_vfx::vfx::VfsRead;

fn fromResult<'a>(value: io::Result<Tag>) -> Task<'a, Tag> { Box::pin(std::future::ready(value)) }
fn reader<'a>(r: Option<&'a mut dyn VfsRead>) -> io::Result<&'a mut dyn VfsRead> { r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "r")) }
/// C# `Path.GetFileNameWithoutExtension`.
fn fileStem(path: &str) -> String { let name = fileName(path); name.rsplit_once('.').map(|(s, _)| s.to_string()).unwrap_or(name) }
fn node(name: String) -> MetaInfo { MetaInfo::new(Some(name), Tag::None, vec![], false) }
fn content(type_: &str, file: Option<&Arc<FileSource>>, value: Tag) -> MetaInfo {
    MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent { type_: type_.into(), name: file.map_or(String::new(), |s| fileName(&s.path)), value, ..Default::default() })), vec![], false)
}
fn thisTexture<T: ITexture + 'static>(me: &Weak<T>) -> Tag { me.upgrade().map_or(Tag::None, |s| Tag::Texture(s)) }
fn endsWithIgnoreCase(path: &str, suffix: &str) -> bool {
    path.len() >= suffix.len() && path[path.len() - suffix.len()..].eq_ignore_ascii_case(suffix)
}

// #region Binary_BspX

/// C# `System.Numerics.Vector4` is 16 bytes aligned to 4. glam's `Vec4` is aligned to 16.
#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct Vector4 { pub X: f32, pub Y: f32, pub Z: f32, pub W: f32 }

/// C# `(int)r.BaseStream.Length`. The position is put back.
fn baseLength(r: &mut Stream) -> io::Result<i32> {
    let pos = r.stream_position()?;
    let len = r.seek(SeekFrom::End(0))?;
    r.seek(SeekFrom::Start(pos))?;
    Ok(len as i32)
}

/// C# `Encoding.ASCII.GetString`. A null array is `ArgumentNullException`. Bytes above 127 become `?`.
fn asciiGetString(bytes: Option<Vec<u8>>) -> io::Result<String> {
    let bytes = bytes.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "ArgumentNullException"))?;
    Ok(bytes.into_iter().map(|c| if c < 0x80 { c as char } else { '?' }).collect())
}

/// Shared prefix of C# `CopyLump`. `Ok(None)` is `return default` (a null array).
fn copyLumpCount(r: &mut Stream, fileLength: i32, lump: X_LumpON, size: i32, maxSize: i32) -> io::Result<Option<i32>> {
    let mut length = lump.Num;
    let ofs = lump.Offset;
    if length % size != 0 { return Err(Error::new(ErrorKind::InvalidData, "LoadBSPFile: odd lump size")); }
    if length / size > maxSize {
        println!("WARNING: exceeded max size for lump %d size %d > maxSize %d\n");
        length = maxSize.wrapping_mul(size);
    }
    if ofs.wrapping_add(length) > fileLength {
        println!("WARNING: exceeded file length for lump GameX.ID.Formats.X_LumpON\n");
        length = fileLength.wrapping_sub(ofs);
        if length <= 0 { return Ok(None); }
    }
    if ofs < 0 { return Err(Error::new(ErrorKind::InvalidInput, "IOException")); }
    r.seek(SeekFrom::Start(ofs as u64))?;
    Ok(Some(length / size))
}

fn copyLumpS<T: IStruct>(r: &mut Stream, fileLength: i32, lump: X_LumpON, size: i32, maxSize: i32) -> io::Result<Option<Vec<T>>> {
    match copyLumpCount(r, fileLength, lump, size, maxSize)? {
        None => Ok(None),
        Some(count) if count > 0 => Ok(Some(r.readSArray::<T>(count as usize, 0)?)),
        Some(_) => Ok(Some(Vec::new())),
    }
}

fn copyLumpP<T: Pod>(r: &mut Stream, fileLength: i32, lump: X_LumpON, pat: &str, size: i32, maxSize: i32) -> io::Result<Option<Vec<T>>> {
    match copyLumpCount(r, fileLength, lump, size, maxSize)? {
        None => Ok(None),
        Some(count) if count > 0 => Ok(Some(r.readPArray::<T>(pat, count as usize)?)),
        Some(_) => Ok(Some(Vec::new())),
    }
}

/// C# `size <= 1` arm: `ReadBytes`. A negative count throws; zero is an empty array.
fn copyLumpBytes(r: &mut Stream, fileLength: i32, lump: X_LumpON, maxSize: i32) -> io::Result<Option<Vec<u8>>> {
    match copyLumpCount(r, fileLength, lump, 1, maxSize)? {
        None => Ok(None),
        Some(count) if count > 0 => Ok(Some(r.readBytes(count as usize)?)),
        Some(0) => Ok(Some(Vec::new())),
        Some(_) => Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")),
    }
}

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_LumpON { pub Offset: i32, pub Num: i32 }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DHeader_H1 {
    pub Version: i32,
    pub ENTITIES: X_LumpON,
    pub PLANES: X_LumpON,
    pub TEXTURES: X_LumpON,
    pub VERTEXES: X_LumpON,
    pub VISIBILITY: X_LumpON,
    pub NODES: X_LumpON,
    pub TEXINFO: X_LumpON,
    pub FACES: X_LumpON,
    pub LIGHTING: X_LumpON,
    pub CLIPNODES: X_LumpON,
    pub LEAFS: X_LumpON,
    pub MARKSURFACES: X_LumpON,
    pub EDGES: X_LumpON,
    pub SURFEDGES: X_LumpON,
    pub MODELS: X_LumpON,
}
impl IStruct for X_DHeader_H1 { const Struct: (&'static str, usize) = ("<31i", size_of::<X_DHeader_H1>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DHeader_S2 {
    pub Magic: i32,
    pub Version: i32,
    pub ENTITIES: X_LumpON,
    pub PLANES: X_LumpON,
    pub VERTEXES: X_LumpON,
    pub TEXTURES: X_LumpON,
    pub VISIBILITY: X_LumpON,
    pub NODES: X_LumpON,
    pub TEXINFO: X_LumpON,
    pub FACES: X_LumpON,
    pub LIGHTING: X_LumpON,
    pub LEAFS: X_LumpON,
    pub LEAFFACES: X_LumpON,
    pub LEAFBRUSHES: X_LumpON,
    pub EDGES: X_LumpON,
    pub SURFEDGES: X_LumpON,
    pub MODELS: X_LumpON,
    pub BRUSHES: X_LumpON,
    pub BRUSHSIDES: X_LumpON,
    pub POP: X_LumpON,
    pub AREAS: X_LumpON,
    pub AREAPORTALS: X_LumpON,
    pub LIGHTINFO_S: X_LumpON,
}
impl IStruct for X_DHeader_S2 { const Struct: (&'static str, usize) = ("<42i", size_of::<X_DHeader_S2>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DHeader_3 {
    pub Magic: i32,
    pub Version: i32,
    pub ENTITIES: X_LumpON,
    pub SHADERS: X_LumpON,
    pub PLANES: X_LumpON,
    pub NODES: X_LumpON,
    pub LEAFS: X_LumpON,
    pub LEAFSURFACES: X_LumpON,
    pub LEAFBRUSHES: X_LumpON,
    pub MODELS: X_LumpON,
    pub BRUSHES: X_LumpON,
    pub BRUSHSIDES: X_LumpON,
    pub DRAWVERTS: X_LumpON,
    pub DRAWINDEXES: X_LumpON,
    pub FOGS: X_LumpON,
    pub SURFACES: X_LumpON,
    pub LIGHTMAPS: X_LumpON,
    pub LIGHTGRID: X_LumpON,
    pub VISIBILITY: X_LumpON,
}
impl IStruct for X_DHeader_3 { const Struct: (&'static str, usize) = ("<36i", size_of::<X_DHeader_3>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DModel_H1 {
    pub Mins: Vec3, pub Maxs: Vec3, pub Origin: Vec3,
    pub HeadNodes: [i32; 4],
    pub Visleafs: i32,
    pub FirstFace: i32, pub NumFaces: i32,
}
impl IStruct for X_DModel_H1 { const Struct: (&'static str, usize) = ("<9f7i", size_of::<X_DModel_H1>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DModel_3 {
    pub Mins: Vec3, pub Maxs: Vec3,
    pub FirstSurface: i32, pub NumSurfaces: i32,
    pub FirstBrush: i32, pub NumBrushes: i32,
}
impl IStruct for X_DModel_3 { const Struct: (&'static str, usize) = ("<6f4i", size_of::<X_DModel_3>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DPlane_H1S2 { pub Normal: Vec3, pub Dist: f32, pub Type: i32 }
impl IStruct for X_DPlane_H1S2 { const Struct: (&'static str, usize) = ("<4fi", size_of::<X_DPlane_H1S2>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DPlane_3 { pub Normal: Vec3, pub Dist: f32 }
impl IStruct for X_DPlane_3 { const Struct: (&'static str, usize) = ("<4f", size_of::<X_DPlane_3>()); }

/// C# `(short l, short r) Children` is `I16Vec2` (`l` then `r`). `Vector3<short>` is `I16Vec3`.
#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DNode_H1S2 {
    pub PlaneNum: i32,
    pub Children: I16Vec2,
    pub Mins: I16Vec3, pub Maxs: I16Vec3,
    pub FirstFace: u16, pub NumFaces: u16,
}
impl IStruct for X_DNode_H1S2 { const Struct: (&'static str, usize) = ("<i8h2H", size_of::<X_DNode_H1S2>()); }

/// C# `(int l, int r) Children` is `IVec2`. `Vector3<int>` is `IVec3`.
#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DNode_3 { pub PlaneNum: i32, pub Children: IVec2, pub Mins: IVec3, pub Maxs: IVec3 }
impl IStruct for X_DNode_3 { const Struct: (&'static str, usize) = ("<9i", size_of::<X_DNode_3>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DClipNode_H1 { pub PlaneNum: i32, pub Children: I16Vec2 }
impl IStruct for X_DClipNode_H1 { const Struct: (&'static str, usize) = ("<I2h", size_of::<X_DClipNode_H1>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_Texinfo_H1 { pub Vecs0: Vector4, pub Vecs1: Vector4, pub Miptex: i32, pub Flags: i32 }
impl IStruct for X_Texinfo_H1 { const Struct: (&'static str, usize) = ("<8f2i", size_of::<X_Texinfo_H1>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_Lightinfo_S {
    pub Value: i32,
    pub Color: Vec3,
    pub Direct: f32, pub Directangle: f32, pub Directstyle: f32,
    pub DirectstyleName: [u8; 32],
}
impl IStruct for X_Lightinfo_S { const Struct: (&'static str, usize) = ("<i6fs32", size_of::<X_Lightinfo_S>()); }

#[repr(C)]
#[derive(Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_Texinfo_S {
    pub Vecs0: Vector4, pub Vecs1: Vector4,
    pub Flags: i32,
    pub Texture: [u8; 64],
    pub NextTexinfo: i32,
    pub TransMag: f32,
    pub TransAngle: i32,
    pub BaseAngle: i32,
    pub Animtime: f32,
    pub Nonlit: f32,
    pub Translucence: f32,
    pub Friction: f32,
    pub Restitution: f32,
    pub Color: Vec3,
    pub GroupName: [u8; 32],
}
impl IStruct for X_Texinfo_S { const Struct: (&'static str, usize) = ("<8f2i", size_of::<X_Texinfo_S>()); }
impl Default for X_Texinfo_S { fn default() -> Self { Zeroable::zeroed() } }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_Texinfo_2 {
    pub Vecs0: Vector4, pub Vecs1: Vector4,
    pub Flags: i32,
    pub Value: i32,
    pub Texture: [u8; 32],
    pub NextTexinfo: i32,
}
impl IStruct for X_Texinfo_2 { const Struct: (&'static str, usize) = ("<8f2i", size_of::<X_Texinfo_2>()); }

/// C# `Vector2<ushort>`.
#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DEdge_H1S2 { pub V: U16Vec2 }
impl IStruct for X_DEdge_H1S2 { const Struct: (&'static str, usize) = ("<2H", size_of::<X_DEdge_H1S2>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DFace_H12 {
    pub PlaneNum: u16,
    pub Side: i16,
    pub FirstEdge: i32,
    pub NumEdges: i16,
    pub Texinfo: i16,
    pub Styles: [u8; 4],
    pub LightOfs: i32,
}
impl IStruct for X_DFace_H12 { const Struct: (&'static str, usize) = ("<Hhi2h4Bi", size_of::<X_DFace_H12>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DFace_S {
    pub PlaneNum: u16,
    pub Side: i16,
    pub FirstEdge: i32,
    pub NumEdges: i16,
    pub Texinfo: i16,
    pub Styles: [u8; 4],
    pub LightOfs: i32,
    pub LightInfo: i32,
}
impl IStruct for X_DFace_S { const Struct: (&'static str, usize) = ("<Hhi2h4B2i", size_of::<X_DFace_S>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DSurface_3 {
    pub ShaderNum: i32, pub FogNum: i32, pub SurfaceType: i32,
    pub FirstVert: i32, pub NumVerts: i32,
    pub FirstIndex: i32, pub NumIndexes: i32,
    pub LightmapNum: i32,
    pub LightmapX: i32, pub LightmapY: i32,
    pub LightmapWidth: i32, pub LightmapHeight: i32,
    pub LightmapOrigin: Vec3,
    pub LightmapVecs0: Vec3, pub LightmapVecs1: Vec3, pub LightmapVecs2: Vec3,
    pub PatchWidth: i32, pub PatchHeight: i32,
}
impl IStruct for X_DSurface_3 { const Struct: (&'static str, usize) = ("<12i6f2i", size_of::<X_DSurface_3>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DLeaf_H1 {
    pub Contents: i32,
    pub VisOfs: i32,
    pub Mins: I16Vec3, pub Maxs: I16Vec3,
    pub FirstMarkSurface: u16, pub NumMarkSurface: u16,
    pub AmbientLevel: [u8; 4],
}
impl IStruct for X_DLeaf_H1 { const Struct: (&'static str, usize) = ("<2i6h2H4B", size_of::<X_DLeaf_H1>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DLeaf_S2 {
    pub Contents: i32,
    pub Cluster: i16, pub Area: i16,
    pub Mins: I16Vec3, pub Maxs: I16Vec3,
    pub FirstLeafFace: u16, pub NumLeafFaces: u16,
    pub FirstLeafBrush: u16, pub NumLeafBrushes: u16,
}
impl IStruct for X_DLeaf_S2 { const Struct: (&'static str, usize) = ("<i8h8H", size_of::<X_DLeaf_S2>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DLeaf_3 {
    pub Cluster: i32, pub Area: i32,
    pub Mins: IVec3, pub Maxs: IVec3,
    pub FirstLeafSurface: i32, pub NumLeafSurfaces: i32,
    pub FirstLeafBrush: i32, pub NumLeafBrushes: i32,
}
impl IStruct for X_DLeaf_3 { const Struct: (&'static str, usize) = ("<12i", size_of::<X_DLeaf_3>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DBrushside_2 { pub PlaneNum: u16, pub Texinfo: i16 }
impl IStruct for X_DBrushside_2 { const Struct: (&'static str, usize) = ("<Hh", size_of::<X_DBrushside_2>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DBrushside_S { pub PlaneNum: u16, pub Texinfo: i16, pub Lightinfo: i32 }
impl IStruct for X_DBrushside_S { const Struct: (&'static str, usize) = ("<Hhi", size_of::<X_DBrushside_S>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DBrushside_3 { pub PlaneNum: i32, pub ShaderNum: i32 }
impl IStruct for X_DBrushside_3 { const Struct: (&'static str, usize) = ("<2i", size_of::<X_DBrushside_3>()); }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DBrush_S23 { pub FirstSide: i32, pub NumSides: i32, pub Contents_ShaderNum: i32 }
impl IStruct for X_DBrush_S23 { const Struct: (&'static str, usize) = ("<3i", size_of::<X_DBrush_S23>()); }

#[repr(C)]
#[derive(Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DShader_3 { pub Shader: [u8; 64], pub SurfaceFlags: i32, pub ContentFlags: i32 }
impl IStruct for X_DShader_3 { const Struct: (&'static str, usize) = ("<64s2i", size_of::<X_DShader_3>()); }
impl Default for X_DShader_3 { fn default() -> Self { Zeroable::zeroed() } }

#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DAreaportal_S2 { pub PortalNum: i32, pub OtherArea: i32 }
impl IStruct for X_DAreaportal_S2 { const Struct: (&'static str, usize) = ("<2i", size_of::<X_DAreaportal_S2>()); }

/// C# `Struct` names `sizeof(X_DAreaportal_S2)`, which is the same 8 bytes.
#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DArea_S2 { pub NumAreaportals: i32, pub FirstAreaportal: i32 }
impl IStruct for X_DArea_S2 { const Struct: (&'static str, usize) = ("<2i", size_of::<X_DArea_S2>()); }

#[repr(C)]
#[derive(Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DFog_3 { pub Shader: [u8; 64], pub BrushNum: i32, pub VisibleSide: i32 }
impl IStruct for X_DFog_3 { const Struct: (&'static str, usize) = ("<64s3i", size_of::<X_DFog_3>()); }
impl Default for X_DFog_3 { fn default() -> Self { Zeroable::zeroed() } }

/// C# `Struct` names `sizeof(X_DBrush_S23)`. The fields are the same 12 bytes.
#[repr(C)]
#[derive(Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct X_DrawVert_3 { pub FirstSide: i32, pub NumSides: i32, pub Contents_ShaderNum: i32 }
impl IStruct for X_DrawVert_3 { const Struct: (&'static str, usize) = ("<3i", size_of::<X_DrawVert_3>()); }

const _: () = assert!(size_of::<X_DHeader_H1>() == 124 && size_of::<X_DHeader_S2>() == 176 && size_of::<X_DHeader_3>() == 144);
const _: () = assert!(size_of::<X_DModel_H1>() == 64 && size_of::<X_DModel_3>() == 40);
const _: () = assert!(size_of::<X_DPlane_H1S2>() == 20 && size_of::<X_DPlane_3>() == 16);
const _: () = assert!(size_of::<X_DNode_H1S2>() == 24 && size_of::<X_DNode_3>() == 36 && size_of::<X_DClipNode_H1>() == 8);
const _: () = assert!(size_of::<X_Texinfo_H1>() == 40 && size_of::<X_Texinfo_S>() == 180 && size_of::<X_Texinfo_2>() == 76 && size_of::<X_Lightinfo_S>() == 60);
const _: () = assert!(size_of::<X_DEdge_H1S2>() == 4 && size_of::<X_DFace_H12>() == 20 && size_of::<X_DFace_S>() == 24 && size_of::<X_DSurface_3>() == 104);
const _: () = assert!(size_of::<X_DLeaf_H1>() == 28 && size_of::<X_DLeaf_S2>() == 28 && size_of::<X_DLeaf_3>() == 48);
const _: () = assert!(size_of::<X_DBrushside_2>() == 4 && size_of::<X_DBrushside_S>() == 8 && size_of::<X_DBrushside_3>() == 8 && size_of::<X_DBrush_S23>() == 12);
const _: () = assert!(size_of::<X_DShader_3>() == 72 && size_of::<X_DAreaportal_S2>() == 8 && size_of::<X_DArea_S2>() == 8 && size_of::<X_DFog_3>() == 72 && size_of::<X_DrawVert_3>() == 12);
const _: () = assert!(size_of::<Vector4>() == 16 && size_of::<Vec3>() == 12);

impl X_DHeader_H1 {
    pub fn read(mut self, r: &mut Stream, id: &str, h1: bool) -> io::Result<Tag> {
        if id == "HL:BS" { std::mem::swap(&mut self.ENTITIES, &mut self.PLANES); }
        if self.Version != if h1 { 30 } else { 29 } { return Err(Error::new(ErrorKind::InvalidData, "BAD VERSION")); }
        let fileLength = baseLength(r)?;
        let dmodels = copyLumpS::<X_DModel_H1>(r, fileLength, self.MODELS, size_of::<X_DModel_H1>() as i32, if h1 { 400 } else { 256 })?;
        let dvertexes = copyLumpP::<Vec3>(r, fileLength, self.VERTEXES, "3f", size_of::<Vec3>() as i32, 65535)?;
        let dplanes = copyLumpS::<X_DPlane_H1S2>(r, fileLength, self.PLANES, size_of::<X_DPlane_H1S2>() as i32, if h1 { 32767 } else { 8192 })?;
        let dleafs = copyLumpS::<X_DLeaf_H1>(r, fileLength, self.LEAFS, size_of::<X_DLeaf_H1>() as i32, if h1 { 8192 } else { 32767 })?;
        let dnodes = copyLumpS::<X_DNode_H1S2>(r, fileLength, self.NODES, size_of::<X_DNode_H1S2>() as i32, 32767)?;
        let texinfo = copyLumpS::<X_Texinfo_H1>(r, fileLength, self.TEXINFO, size_of::<X_Texinfo_H1>() as i32, if h1 { 8192 } else { 4096 })?;
        let dclipnodes = copyLumpS::<X_DClipNode_H1>(r, fileLength, self.CLIPNODES, size_of::<X_DClipNode_H1>() as i32, 32767)?;
        let dfaces = copyLumpS::<X_DFace_H12>(r, fileLength, self.FACES, size_of::<X_DFace_H12>() as i32, 65535)?;
        let dmarksurfaces = copyLumpP::<u16>(r, fileLength, self.MARKSURFACES, "H", size_of::<u16>() as i32, 65535)?;
        let dsurfedges = copyLumpP::<i32>(r, fileLength, self.SURFEDGES, "i", size_of::<i32>() as i32, 512000)?;
        let dedges = copyLumpS::<X_DEdge_H1S2>(r, fileLength, self.EDGES, size_of::<X_DEdge_H1S2>() as i32, 256000)?;
        let dtexdata = copyLumpBytes(r, fileLength, self.TEXTURES, 0x200000)?;
        let dvisdata = copyLumpBytes(r, fileLength, self.VISIBILITY, if h1 { 0x200000 } else { 0x100000 })?;
        let dlightdata = copyLumpBytes(r, fileLength, self.LIGHTING, if h1 { 0x200000 } else { 0x100000 })?;
        let dentdata = asciiGetString(copyLumpBytes(r, fileLength, self.ENTITIES, if h1 { 131072 } else { 65536 })?)?;
        let _ = (dmodels, dvertexes, dplanes, dleafs, dnodes, texinfo, dclipnodes, dfaces, dmarksurfaces, dsurfedges, dedges, dtexdata, dvisdata, dlightdata, dentdata);
        Ok(Tag::None)
    }
}

impl X_DHeader_S2 {
    pub fn read(self, r: &mut Stream, _id: &str, sin: bool) -> io::Result<Tag> {
        if self.Magic != if sin { 0x0 } else { 0x0 } { return Err(Error::new(ErrorKind::InvalidData, "BAD MAGIC")); }
        if self.Version != if sin { 30 } else { 38 } { return Err(Error::new(ErrorKind::InvalidData, "BAD VERSION")); }
        let fileLength = baseLength(r)?;
        // C# passes `sizeof(X_DModel_S2)` (48) and then reads `X_DModel_H1` (64).
        let dmodels = copyLumpS::<X_DModel_H1>(r, fileLength, self.MODELS, 48, 1024)?;
        let dvertexes = copyLumpP::<Vec3>(r, fileLength, self.VERTEXES, "3f", size_of::<Vec3>() as i32, 65535)?;
        let dplanes = copyLumpS::<X_DPlane_H1S2>(r, fileLength, self.PLANES, size_of::<X_DPlane_H1S2>() as i32, 65535)?;
        let dleafs = copyLumpS::<X_DLeaf_S2>(r, fileLength, self.LEAFS, size_of::<X_DLeaf_S2>() as i32, 65535)?;
        let dnodes = copyLumpS::<X_DNode_H1S2>(r, fileLength, self.NODES, size_of::<X_DNode_H1S2>() as i32, 65535)?;
        if sin { copyLumpS::<X_Texinfo_S>(r, fileLength, self.TEXINFO, size_of::<X_Texinfo_S>() as i32, 8192)?; }
        else { copyLumpS::<X_Texinfo_2>(r, fileLength, self.TEXINFO, size_of::<X_Texinfo_2>() as i32, 8192)?; }
        if sin { copyLumpS::<X_DFace_S>(r, fileLength, self.FACES, size_of::<X_DFace_S>() as i32, 65535)?; }
        else { copyLumpS::<X_DFace_H12>(r, fileLength, self.FACES, size_of::<X_DFace_H12>() as i32, 65535)?; }
        let dleaffaces = copyLumpP::<u16>(r, fileLength, self.LEAFFACES, "H", size_of::<u16>() as i32, 65535)?;
        let dleafbrushes = copyLumpP::<u16>(r, fileLength, self.LEAFBRUSHES, "H", size_of::<u16>() as i32, 65535)?;
        let dsurfedges = copyLumpP::<i32>(r, fileLength, self.SURFEDGES, "i", size_of::<i32>() as i32, 256000)?;
        let dedges = copyLumpS::<X_DEdge_H1S2>(r, fileLength, self.EDGES, size_of::<X_DEdge_H1S2>() as i32, 128000)?;
        let dbrushes = copyLumpS::<X_DBrush_S23>(r, fileLength, self.BRUSHES, size_of::<X_DBrush_S23>() as i32, 8192)?;
        if sin { copyLumpS::<X_DBrushside_S>(r, fileLength, self.BRUSHSIDES, size_of::<X_DBrushside_S>() as i32, 65536)?; }
        else { copyLumpS::<X_DBrushside_2>(r, fileLength, self.BRUSHSIDES, size_of::<X_DBrushside_2>() as i32, 65536)?; }
        let dareas = copyLumpS::<X_DArea_S2>(r, fileLength, self.AREAS, size_of::<X_DArea_S2>() as i32, 256)?;
        let dareaportals = copyLumpS::<X_DAreaportal_S2>(r, fileLength, self.AREAPORTALS, size_of::<X_DAreaportal_S2>() as i32, 1024)?;
        let lightinfo = if sin { copyLumpS::<X_Lightinfo_S>(r, fileLength, self.LIGHTINFO_S, size_of::<X_Lightinfo_S>() as i32, 1024)? } else { None };
        let dvisdata = copyLumpBytes(r, fileLength, self.VISIBILITY, 0x280000)?;
        let dlightdata = copyLumpBytes(r, fileLength, self.LIGHTING, if sin { 0x300000 } else { 0x320000 })?;
        let dentdata = asciiGetString(copyLumpBytes(r, fileLength, self.ENTITIES, 0x40000)?)?;
        let _ = (dmodels, dvertexes, dplanes, dleafs, dnodes, dleaffaces, dleafbrushes, dsurfedges, dedges, dbrushes, dareas, dareaportals, lightinfo, dvisdata, dlightdata, dentdata);
        Ok(Tag::None)
    }
}

impl X_DHeader_3 {
    pub fn read(self, r: &mut Stream, id: &str) -> io::Result<Tag> {
        let _ = id;
        if self.Magic != 0x0 { return Err(Error::new(ErrorKind::InvalidData, "BAD MAGIC")); }
        if self.Version != 46 { return Err(Error::new(ErrorKind::InvalidData, "BAD VERSION")); }
        let fileLength = baseLength(r)?;
        let dshaders = copyLumpS::<X_DShader_3>(r, fileLength, self.SHADERS, size_of::<X_DShader_3>() as i32, 0x400)?;
        let dmodels = copyLumpS::<X_DModel_3>(r, fileLength, self.MODELS, size_of::<X_DModel_3>() as i32, 0x400)?;
        let dplanes = copyLumpS::<X_DPlane_3>(r, fileLength, self.PLANES, size_of::<X_DPlane_3>() as i32, 0x20000)?;
        let dleafs = copyLumpS::<X_DLeaf_3>(r, fileLength, self.LEAFS, size_of::<X_DLeaf_3>() as i32, 0x20000)?;
        let dnodes = copyLumpS::<X_DNode_3>(r, fileLength, self.NODES, size_of::<X_DNode_3>() as i32, 0x20000)?;
        let dleafsurfaces = copyLumpP::<i32>(r, fileLength, self.LEAFSURFACES, "i", size_of::<i32>() as i32, 0x20000)?;
        let dleafbrushes = copyLumpP::<i32>(r, fileLength, self.LEAFBRUSHES, "i", size_of::<i32>() as i32, 0x40000)?;
        let dbrushes = copyLumpS::<X_DBrush_S23>(r, fileLength, self.BRUSHES, size_of::<X_DBrush_S23>() as i32, 0x8000)?;
        let dbrushsides = copyLumpS::<X_DBrushside_3>(r, fileLength, self.BRUSHSIDES, size_of::<X_DBrushside_3>() as i32, 0x20000)?;
        let drawVerts = copyLumpS::<X_DrawVert_3>(r, fileLength, self.DRAWVERTS, size_of::<X_DrawVert_3>() as i32, 0x80000)?;
        let drawSurfaces = copyLumpS::<X_DSurface_3>(r, fileLength, self.SURFACES, size_of::<X_DSurface_3>() as i32, 0x20000)?;
        let dfogs = copyLumpS::<X_DFog_3>(r, fileLength, self.FOGS, size_of::<X_DFog_3>() as i32, 0x100)?;
        let drawIndexes = copyLumpP::<i32>(r, fileLength, self.DRAWINDEXES, "i", size_of::<i32>() as i32, 0x80000)?;
        let dvisData = copyLumpBytes(r, fileLength, self.VISIBILITY, 0x200000)?;
        let dlightData = copyLumpBytes(r, fileLength, self.LIGHTMAPS, 0x800000)?;
        let dentData = asciiGetString(copyLumpBytes(r, fileLength, self.ENTITIES, 0x10000)?)?;
        let gridData = copyLumpBytes(r, fileLength, self.LIGHTGRID, 0x800000)?;
        let _ = (dshaders, dmodels, dplanes, dleafs, dnodes, dleafsurfaces, dleafbrushes, dbrushes, dbrushsides, drawVerts, drawSurfaces, dfogs, drawIndexes, dvisData, dlightData, dentData, gridData);
        Ok(Tag::None)
    }
}

/// Quake / Half-Life / Sin / Quake 3 BSP. The three `Read`s return null, as in C#.
#[derive(Debug)]
pub struct Binary_BspX {
    h1: Tag,
    s2: Tag,
    x3: Tag,
    me: Weak<Binary_BspX>,
}

impl Binary_BspX {
    pub fn factory<'a>(r: &'a mut Stream, _f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Self::new(r, s).map(|bsp| Tag::Info(bsp)))
    }

    pub fn new(r: &mut Stream, s: &dyn Archive) -> io::Result<Arc<Self>> {
        let mut flag = false;
        let game = &s.base().game;
        let gameId = game.id.as_str();
        let engineN = game.engine.0.as_deref();
        let engineV = game.engine.1.as_deref();
        // C# `||` skips the flag assignment when the left side is already true.
        let h1 = if engineV == Some("2") || { flag = engineN == Some("GoldSrc"); flag } { r.readS::<X_DHeader_H1>()?.read(r, gameId, flag)? } else { Tag::None };
        let s2 = if engineV == Some("3") || { flag = gameId == "Sin"; flag } { r.readS::<X_DHeader_S2>()?.read(r, gameId, flag)? } else { Tag::None };
        let x3 = if engineV == Some("3") { r.readS::<X_DHeader_3>()?.read(r, gameId)? } else { Tag::None };
        Ok(Arc::new_cyclic(|w| Self { h1, s2, x3, me: w.clone() }))
    }
}

impl IHaveMetaInfo for Binary_BspX {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Text", file, self.me.upgrade().map_or(Tag::None, |s| Tag::Any(s))),
        MetaInfo::new(Some("BSP".into()), Tag::None, vec![], false),
    ] }
}

// #endregion

// #region Binary_Lmp

/// Quake lump picture, palette, or colormap. `Palette` and `Colormap` are process-wide, as in C#.
pub struct Binary_Lmp {
    pub paletteRecords: Option<Vec<Vec<u8>>>,
    pub colormapRecords: Option<Vec<Vec<u8>>>,
    pixels: Vec<u8>,
    pub width: i32,
    pub height: i32,
    me: Weak<Binary_Lmp>,
}

static PALETTE: Mutex<Option<Arc<Binary_Lmp>>> = Mutex::new(None);
static COLORMAP: Mutex<Option<Arc<Binary_Lmp>>> = Mutex::new(None);

impl Binary_Lmp {
    const FORMAT: (TextureFormat, TexturePixel) = (TextureFormat::BGRA32, TexturePixel::Unknown);

    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Self::new(r, &f).map(|s| Tag::Info(s)))
    }

    pub fn new(r: &mut Stream, f: &FileSource) -> io::Result<Arc<Self>> {
        match fileStem(&f.path).as_str() {
            "palette" => {
                let mut paletteRecords = Vec::with_capacity(256);
                for _ in 0..256 {
                    let mut rgb = r.readBytes(3)?;
                    rgb.push(0);
                    paletteRecords.push(rgb);
                }
                let this = Arc::new_cyclic(|w| Self { paletteRecords: Some(paletteRecords), colormapRecords: None, pixels: Vec::new(), width: 0, height: 0, me: w.clone() });
                *PALETTE.lock().unwrap() = Some(this.clone());
                Ok(this)
            }
            "colormap" => {
                let mut colormapRecords = Vec::with_capacity(32);
                for _ in 0..32 { colormapRecords.push(r.readBytes(256)?); }
                let this = Arc::new_cyclic(|w| Self { paletteRecords: None, colormapRecords: Some(colormapRecords), pixels: Vec::new(), width: 0, height: 0, me: w.clone() });
                *COLORMAP.lock().unwrap() = Some(this.clone());
                Ok(this)
            }
            _ => {
                if PALETTE.lock().unwrap().is_none() { return Err(Error::new(ErrorKind::Unsupported, "NotImplementedException")); }
                let width = r.readInt32()?;
                let height = r.readInt32()?;
                let count = width.wrapping_mul(height);
                if count < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")); }
                let indices = r.readBytes(count as usize)?;
                let mut pixels = Vec::with_capacity(indices.len() * 4);
                for &index in &indices { pixels.extend_from_slice(&Self::toLightPixel(32 << 3, index as usize)); }
                Ok(Arc::new_cyclic(|w| Self { paletteRecords: None, colormapRecords: None, pixels, width, height, me: w.clone() }))
            }
        }
    }

    /// C# `ToLightPixel`. `(light >> 3) & 0x1F` selects the colormap row.
    pub fn toLightPixel(light: i32, pixel: usize) -> Vec<u8> {
        let palette = PALETTE.lock().unwrap().clone().expect("NullReferenceException: Palette");
        let colormap = COLORMAP.lock().unwrap().clone().expect("NullReferenceException: Colormap");
        let row = ((light >> 3) & 0x1F) as usize;
        let mapped = colormap.colormapRecords.as_ref().unwrap()[row][pixel] as usize;
        palette.paletteRecords.as_ref().unwrap()[mapped].clone()
    }

    pub fn create<T>(&self, _platform: &str, func: impl FnOnce(Box<dyn Any>) -> T) -> io::Result<T> {
        Ok(func(Box::new(TextureAsBytes { bytes: self.pixels.clone(), format: Arc::new(Self::FORMAT) as AnyArc, spans: vec![] })))
    }
}

impl ITexture for Binary_Lmp {
    fn width(&self) -> i32 { self.width }
    fn height(&self) -> i32 { self.height }
    fn depth(&self) -> i32 { 0 }
    fn mipMaps(&self) -> i32 { 1 }
    fn texFlags(&self) -> TextureFlags { TextureFlags::empty() }
    fn create(&self, platform: &str, func: &mut dyn FnMut(&dyn Any)) -> io::Result<()> { Self::create(self, platform, |x| func(&*x)) }
}

impl IHaveMetaInfo for Binary_Lmp {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        content("TextureName", file, thisTexture(&self.me)),
        MetaInfo::new(Some("TextureName".into()), Tag::None, vec![
            node(format!("Width: {}", self.width)),
            node(format!("Height: {}", self.height)),
        ], false),
    ] }
}

// #endregion

// #region Binary_Mdl

/// C# `Binary_Mdl` reads nothing. `GetInfoNodes` says `UnknownFileModel`.
pub struct Binary_Mdl { me: Weak<Binary_Mdl> }

impl Binary_Mdl {
    pub fn factory<'a>(r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new_cyclic(|w| { let mut s = Self::new(r); s.me = w.clone(); s }))))
    }

    pub fn new(_r: &mut Stream) -> Self { Self { me: Weak::new() } }
}

impl IHaveMetaInfo for Binary_Mdl {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        content("Text", file, Tag::Str("UnknownFileModel File".into())),
        MetaInfo::new(Some("UnknownFileModel".into()), Tag::None, vec![node("Records: 1".into())], false),
    ] }
}

// #endregion

// #region Binary_Pak

const P_MAGIC: u32 = 0x4b434150; // PACK

#[repr(C, packed)]
#[derive(Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct P_Header {
    pub Magic: u32,
    pub DirOffset: i32,
    pub DirLength: i32,
}
impl IStruct for P_Header { const Struct: (&'static str, usize) = ("<I2i", 12); }

#[repr(C, packed)]
#[derive(Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct P_File {
    pub Path: [u8; 56],
    pub Offset: i32,
    pub FileSize: i32,
}
impl IStruct for P_File { const Struct: (&'static str, usize) = ("<56s2i", 64); }

pub struct Binary_Pak;

impl Binary_Pak {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_Pak));
        CURRENT.clone()
    }
}

impl ArcBinary for Binary_Pak {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, _tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let r = reader(r)?;
            let header = r.readS::<P_Header>()?;
            if header.Magic != P_MAGIC { return Err(Error::new(ErrorKind::InvalidData, "BAD MAGIC")); }
            if header.DirLength < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException: DirLength")); }
            if header.DirOffset < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException: DirOffset")); }
            let numFiles = (header.DirLength as usize) / size_of::<P_File>();
            r.seek(SeekFrom::Start(header.DirOffset as u64))?;
            let entries = r.readSArray::<P_File>(numFiles, size_of::<P_File>() as i32)?;
            let mut files = Vec::with_capacity(entries.len());
            for s in entries {
                let path = FixedAStringScan(&s.Path, 56).replace('\\', "/");
                let offset = s.Offset;
                let fileSize = s.FileSize;
                let mut file = FileSource { path, offset: offset as i64, fileSize: fileSize as i64, ..Default::default() };
                if endsWithIgnoreCase(&file.path, ".wad") {
                    let parent = source.base.this().downcastBinaryArchive().ok_or_else(|| Error::new(ErrorKind::InvalidData, "source"))?;
                    let child = Arc::new(file.clone());
                    file.arc = Some(SubArchive::new(&parent, Some(child), &file.path, Tag::None, Binary_Wad::current()));
                }
                files.push(Arc::new(file));
            }
            source.setFiles(Some(files));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, _source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, _option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async move {
            let r = reader(r)?;
            if file.offset < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException: Offset")); }
            if file.fileSize < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException: FileSize")); }
            r.seek(SeekFrom::Start(file.offset as u64))?;
            Ok(Some(Box::new(Cursor::new(r.readBytes(file.fileSize as usize)?)) as Stream))
        })
    }
}

// #endregion

// #region Binary_Spr

#[repr(i32)]
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Binary_SprType { VP_PARALLEL_UPRIGHT = 0, FACING_UPRIGHT = 1, VP_PARALLEL = 2, ORIENTED = 3, VP_PARALLEL_ORIENTED = 4 }

#[repr(i32)]
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Binary_SprTextFormat { SPR_NORMAL = 0, SPR_ADDITIVE = 1, SPR_INDEXALPHA = 2, SPR_ALPHTEST = 3 }

#[repr(i32)]
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Binary_SprSynchType { Synchronized = 0, Random = 1 }

#[repr(C)]
#[derive(Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct X_Header {
    pub Magic: u32,
    pub Version: i32,
    pub Type: i32,
    pub TextFormat: i32,
    pub BoundingRadius: f32,
    pub MaxWidth: i32,
    pub MaxHeight: i32,
    pub NumFrames: i32,
    pub BeamLen: f32,
    pub SynchType: i32,
}
impl IStruct for X_Header { const Struct: (&'static str, usize) = ("<I3if3ifi", 40); }

#[repr(C)]
#[derive(Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct X_Frame {
    pub Group: i32,
    pub OriginX: i32,
    pub OriginY: i32,
    pub Width: i32,
    pub Height: i32,
}
impl IStruct for X_Frame { const Struct: (&'static str, usize) = ("<5i", 20); }

struct SprState { frame: usize, bytes: Vec<u8> }

/// Half-Life / Quake sprite (`IDSP`). `NextFrame` always returns true.
pub struct Binary_Spr {
    frames: Vec<X_Frame>,
    pixels: Vec<Vec<u8>>,
    palette: Vec<u8>,
    state: Mutex<SprState>,
    me: Weak<Binary_Spr>,
}

impl Binary_Spr {
    const FORMAT: (TextureFormat, TexturePixel) = (TextureFormat::RGBA32, TexturePixel::Unknown);

    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Self::new(r, &f).map(|s| Tag::Info(s)))
    }

    pub fn new(r: &mut Stream, _f: &FileSource) -> io::Result<Arc<Self>> {
        let header = r.readS::<X_Header>()?;
        if header.Magic != 0x50534449 { return Err(Error::new(ErrorKind::InvalidData, "BAD MAGIC")); }
        let colors = r.readUInt16()? as usize;
        let palette = r.readBytes(colors * 3)?;
        if header.NumFrames < 0 { return Err(Error::new(ErrorKind::InvalidInput, "OverflowException")); }
        let mut frames = Vec::with_capacity(header.NumFrames as usize);
        let mut pixels = Vec::with_capacity(header.NumFrames as usize);
        for _ in 0..header.NumFrames {
            let frame = r.readS::<X_Frame>()?;
            let count = frame.Width.wrapping_mul(frame.Height);
            if count < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")); }
            pixels.push(r.readBytes(count as usize)?);
            frames.push(frame);
        }
        let frame0 = *frames.first().ok_or_else(|| Error::new(ErrorKind::InvalidData, "IndexOutOfRangeException"))?;
        let n = frame0.Width.wrapping_mul(frame0.Height).wrapping_shl(2);
        if n < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")); }
        let bytes = vec![0u8; n as usize];
        Ok(Arc::new_cyclic(|w| Self { frames, pixels, palette, state: Mutex::new(SprState { frame: 0, bytes }), me: w.clone() }))
    }

    fn advance(&self) -> bool {
        let mut state = self.state.lock().unwrap();
        let p = &self.pixels[state.frame];
        let _ = blitByPalette(&mut state.bytes, 4, p, &self.palette, 3, None);
        state.frame += 1;
        true
    }

    pub fn create<T>(&self, _platform: &str, func: impl FnOnce(Box<dyn Any>) -> T) -> io::Result<T> {
        let state = self.state.lock().unwrap();
        Ok(func(Box::new(TextureAsBytes { bytes: state.bytes.clone(), format: Arc::new(Self::FORMAT) as AnyArc, spans: vec![] })))
    }
}

impl ITexture for Binary_Spr {
    fn width(&self) -> i32 { self.frames.first().map(|s| s.Width).unwrap_or(0) }
    fn height(&self) -> i32 { self.frames.first().map(|s| s.Height).unwrap_or(0) }
    fn depth(&self) -> i32 { 0 }
    fn mipMaps(&self) -> i32 { 1 }
    fn texFlags(&self) -> TextureFlags { TextureFlags::empty() }
    fn fps(&self) -> i32 { 60 }
    fn hasFrames(&self) -> bool { self.state.lock().unwrap().frame < self.frames.len() }
    fn nextFrame(&self) -> bool { self.advance() }
    fn create(&self, platform: &str, func: &mut dyn FnMut(&dyn Any)) -> io::Result<()> { Self::create(self, platform, |x| func(&*x)) }
}

impl ITextureFrames for Binary_Spr {
    fn fps(&self) -> i32 { 60 }
    fn hasFrames(&self) -> bool { ITexture::hasFrames(self) }
    fn nextFrame(&mut self) -> bool { self.advance() }
}

impl IHaveMetaInfo for Binary_Spr {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        content("VideoTexture", file, thisTexture(&self.me)),
        MetaInfo::new(Some("Sprite".into()), Tag::None, vec![
            node(format!("Frames: {}", self.frames.len())),
            node(format!("Width: {}", self.width())),
            node(format!("Height: {}", self.height())),
            node(format!("Mipmaps: {}", self.mipMaps())),
        ], false),
    ] }
}

// #endregion

// #region Binary_Wad

const W_MAGIC: u32 = 0x32444157; // WAD2

#[repr(C, packed)]
#[derive(Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct W_Header {
    pub Magic: u32,
    pub LumpCount: i32,
    pub LumpOffset: i32,
}
impl IStruct for W_Header { const Struct: (&'static str, usize) = ("<I2i", 12); }

#[repr(C, packed)]
#[derive(Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct W_Lump {
    pub Offset: i32,
    pub PackedSize: i32,
    pub FileSize: i32,
    pub Type: u8,
    pub Compression: u8,
    pub Padding0: u8,
    pub Padding1: u8,
    pub Path: [u8; 16],
}
impl IStruct for W_Lump { const Struct: (&'static str, usize) = ("<3i4b16s", 32); }

pub struct Binary_Wad;

impl Binary_Wad {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_Wad));
        CURRENT.clone()
    }
}

impl ArcBinary for Binary_Wad {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, _tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let r = reader(r)?;
            let header = r.readS::<W_Header>()?;
            if header.Magic != W_MAGIC { return Err(Error::new(ErrorKind::InvalidData, "BAD MAGIC")); }
            if header.LumpCount < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException: LumpCount")); }
            if header.LumpOffset < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException: LumpOffset")); }
            r.seek(SeekFrom::Start(header.LumpOffset as u64))?;
            let lumps = r.readSArray::<W_Lump>(header.LumpCount as usize, size_of::<W_Lump>() as i32)?;
            let files = lumps.into_iter().map(|s| {
                let name = FixedAStringScan(&s.Path, 16).replace('\\', "/");
                Arc::new(FileSource {
                    path: format!("{name}.tex"),
                    hash: s.Type as u64,
                    offset: s.Offset as i64,
                    packedSize: s.PackedSize as i64,
                    fileSize: s.FileSize as i64,
                    compressed: s.Compression as i32,
                    ..Default::default()
                })
            }).collect();
            source.setFiles(Some(files));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, option: &'a Tag) -> Task<'a, Option<Stream>> {
        // A temporary `Binary_Pak` would not live as long as the returned task.
        static PAK: Binary_Pak = Binary_Pak;
        PAK.readData(source, r, file, option)
    }
}

// #endregion

#[cfg(test)]
mod tests {
    use super::*;
    use gamex::binary::{getResult, BinaryState};
    use gamex::family::FamilyGame;
    use gamex::meta::Tag;
    use openx_gfx::gfx::{ITexture, TextureAsBytes};
    use openx_vfx::vfx::{DirectoryFileSystem, VfsRead};
    use std::io::{Cursor, Read};

    fn archive(binary: Arc<dyn ArcBinary>) -> Arc<BinaryArchive> {
        let state = BinaryState::new(Arc::new(DirectoryFileSystem::new("", None)), FamilyGame::empty(), None, Some("a.pak"), Tag::None);
        newArchive(BinaryArchive::new(None, state, Some(binary)))
    }

    fn pakBytes(files: &[(&str, &[u8])]) -> Vec<u8> {
        let mut data = Vec::new();
        let mut dir = Vec::new();
        for (name, bytes) in files {
            let offset = (12 + data.len()) as i32;
            data.extend_from_slice(bytes);
            let mut path = [0u8; 56];
            path[..name.len()].copy_from_slice(name.as_bytes());
            dir.extend_from_slice(&path);
            dir.extend_from_slice(&offset.to_le_bytes());
            dir.extend_from_slice(&(bytes.len() as i32).to_le_bytes());
        }
        let mut out = Vec::new();
        out.extend_from_slice(&P_MAGIC.to_le_bytes());
        out.extend_from_slice(&((12 + data.len()) as i32).to_le_bytes());
        out.extend_from_slice(&(dir.len() as i32).to_le_bytes());
        out.extend(data);
        out.extend(dir);
        out
    }

    #[test]
    fn structSizesMatchThePatterns() {
        assert_eq!(size_of::<P_Header>(), 12);
        assert_eq!(size_of::<P_File>(), 64);
        assert_eq!(size_of::<W_Header>(), 12);
        assert_eq!(size_of::<W_Lump>(), 32);
    }

    #[test]
    fn pakListsSlashesAndReadsBytesAndOpensAWadChild() {
        let binary = Binary_Pak::current();
        let arc = archive(binary.clone());
        let mut cursor = Cursor::new(pakBytes(&[("a\\b.txt", b"hi"), ("x.WAD", b"wad")]));
        getResult(binary.read(&arc, Some(&mut cursor as &mut dyn VfsRead), &Tag::None)).unwrap();
        let files = arc.files().unwrap();
        assert_eq!(files[0].path, "a/b.txt");
        assert_eq!((files[0].offset, files[0].fileSize), (12, 2));
        assert!(files[0].arc.is_none());
        assert_eq!(files[1].path, "x.WAD");
        assert!(files[1].arc.is_some());
        cursor.set_position(0);
        let mut data = getResult(binary.readData(&arc, Some(&mut cursor as &mut dyn VfsRead), &files[0], &Tag::None)).unwrap().unwrap();
        let mut got = Vec::new();
        data.read_to_end(&mut got).unwrap();
        assert_eq!(got, b"hi");
    }

    #[test]
    fn pakRejectsABadMagic() {
        let binary = Binary_Pak::current();
        let arc = archive(binary.clone());
        let mut cursor = Cursor::new(vec![0u8; 12]);
        let err = getResult(binary.read(&arc, Some(&mut cursor as &mut dyn VfsRead), &Tag::None)).unwrap_err();
        assert!(err.to_string().contains("BAD MAGIC"), "{err}");
    }

    #[test]
    fn wadAppendsTexAndReadsTheLump() {
        let mut lump = Vec::new();
        lump.extend_from_slice(&44i32.to_le_bytes());
        lump.extend_from_slice(&2i32.to_le_bytes());
        lump.extend_from_slice(&2i32.to_le_bytes());
        lump.extend_from_slice(&[7, 1, 0, 0]);
        let mut name = [0u8; 16];
        name[..4].copy_from_slice(b"door");
        lump.extend_from_slice(&name);
        let mut bytes = Vec::new();
        bytes.extend_from_slice(&W_MAGIC.to_le_bytes());
        bytes.extend_from_slice(&1i32.to_le_bytes());
        bytes.extend_from_slice(&12i32.to_le_bytes());
        bytes.extend_from_slice(&lump);
        bytes.extend_from_slice(b"yz");
        let binary = Binary_Wad::current();
        let arc = archive(binary.clone());
        let mut cursor = Cursor::new(bytes);
        getResult(binary.read(&arc, Some(&mut cursor as &mut dyn VfsRead), &Tag::None)).unwrap();
        let files = arc.files().unwrap();
        assert_eq!(files[0].path, "door.tex");
        assert_eq!((files[0].hash, files[0].compressed, files[0].packedSize, files[0].fileSize), (7, 1, 2, 2));
        cursor.set_position(0);
        let mut data = getResult(binary.readData(&arc, Some(&mut cursor as &mut dyn VfsRead), &files[0], &Tag::None)).unwrap().unwrap();
        let mut got = Vec::new();
        data.read_to_end(&mut got).unwrap();
        assert_eq!(got, b"yz");
    }

    fn putI32(buf: &mut [u8], at: usize, v: i32) { buf[at..at + 4].copy_from_slice(&v.to_le_bytes()); }

    fn gameArc(id: &str, n: Option<&str>, v: Option<&str>) -> Arc<BinaryArchive> {
        let game = Arc::new(FamilyGame { id: id.into(), engine: (n.map(|s| s.to_string()), v.map(|s| s.to_string())), ..Default::default() });
        let state = BinaryState::new(Arc::new(DirectoryFileSystem::new("", None)), game, None, Some("a.bsp"), Tag::None);
        newArchive(BinaryArchive::new(None, state, None))
    }

    fn openBsp(bytes: Vec<u8>, id: &str, n: Option<&str>, v: Option<&str>) -> io::Result<Arc<Binary_BspX>> {
        let arc = gameArc(id, n, v);
        let mut stream: Stream = Box::new(Cursor::new(bytes));
        Binary_BspX::new(&mut stream, &*arc)
    }

    /// Version at `versionAt`, then lumps of `(index, offset, num)` starting at `lumpsAt`.
    fn bspBytes(len: usize, versionAt: usize, version: i32, lumpsAt: usize, lumps: &[(usize, i32, i32)]) -> Vec<u8> {
        let mut buf = vec![0u8; len];
        putI32(&mut buf, versionAt, version);
        for (i, offset, num) in lumps {
            putI32(&mut buf, lumpsAt + i * 8, *offset);
            putI32(&mut buf, lumpsAt + 4 + i * 8, *num);
        }
        buf
    }

    #[test]
    fn bspSkipsEveryHeaderWhenTheEngineDoesNotMatch() {
        let bsp = openBsp(vec![], "Quake", Some("idTech"), None).unwrap();
        assert!(bsp.h1.isNone() && bsp.s2.isNone() && bsp.x3.isNone());
        let file = Arc::new(FileSource { path: "maps/e1m1.bsp".into(), ..Default::default() });
        let nodes = bsp.getInfoNodes(None, Some(&file), &Tag::None);
        assert_eq!(nodes[1].name.as_deref(), Some("BSP"));
        assert!(nodes[1].items.is_empty());
        let Tag::MetaContent(content) = &nodes[0].tag else { panic!("content"); };
        assert_eq!(content.name, "e1m1.bsp");
        assert!(content.value.asAny().is_some());
    }

    #[test]
    fn bspH1ReadsVersion29UnlessGoldSrcOrHlBs() {
        let ok = bspBytes(124, 0, 29, 4, &[]);
        let bsp = openBsp(ok.clone(), "Quake", Some("idTech"), Some("2")).unwrap();
        assert!(bsp.h1.isNone() && bsp.s2.isNone() && bsp.x3.isNone());
        let err = openBsp(bspBytes(124, 0, 1, 4, &[]), "Quake", Some("idTech"), Some("2")).unwrap_err();
        assert!(err.to_string().contains("BAD VERSION"), "{err}");
        // v == "2" short-circuits, so GoldSrc still wants 29.
        openBsp(ok, "Quake", Some("GoldSrc"), Some("2")).unwrap();
        let err = openBsp(bspBytes(124, 0, 30, 4, &[]), "Quake", Some("GoldSrc"), Some("2")).unwrap_err();
        assert!(err.to_string().contains("BAD VERSION"), "{err}");
        // GoldSrc without v 2 wants 30, and does not go on to read S2.
        openBsp(bspBytes(124, 0, 30, 4, &[]), "Quake", Some("GoldSrc"), None).unwrap();
        let err = openBsp(bspBytes(124, 0, 29, 4, &[]), "Quake", Some("GoldSrc"), None).unwrap_err();
        assert!(err.to_string().contains("BAD VERSION"), "{err}");

        // PLANES num 4 is not a multiple of 20. After the HL:BS swap that lump is ENTITIES (bytes) and PLANES is 20.
        let swapped = bspBytes(148, 0, 29, 4, &[(0, 124, 20), (1, 144, 4)]);
        let err = openBsp(swapped.clone(), "Quake", Some("idTech"), Some("2")).unwrap_err();
        assert!(err.to_string().contains("LoadBSPFile: odd lump size"), "{err}");
        openBsp(swapped, "HL:BS", Some("idTech"), Some("2")).unwrap();

        let err = openBsp(bspBytes(124, 0, 29, 4, &[(14, 0, 1)]), "Quake", Some("idTech"), Some("2")).unwrap_err();
        assert!(err.to_string().contains("LoadBSPFile: odd lump size"), "{err}");
        let err = openBsp(bspBytes(124, 0, 29, 4, &[(0, 1000, 4)]), "Quake", Some("idTech"), Some("2")).unwrap_err();
        assert!(err.to_string().contains("ArgumentNullException"), "{err}");
    }

    #[test]
    fn bspSinAndV3ReadS2ThenMaybeX3() {
        openBsp(bspBytes(176, 4, 30, 8, &[]), "Sin", None, None).unwrap();
        // Sin's texinfo lump is `X_Texinfo_S` (180), not `X_Texinfo_2` (76).
        openBsp(bspBytes(176 + 180, 4, 30, 8, &[(6, 176, 180)]), "Sin", None, None).unwrap();
        let err = openBsp(bspBytes(176 + 76, 4, 30, 8, &[(6, 176, 76)]), "Sin", None, None).unwrap_err();
        assert!(err.to_string().contains("LoadBSPFile: odd lump size"), "{err}");
        let err = openBsp(bspBytes(176, 4, 38, 8, &[]), "Sin", None, None).unwrap_err();
        assert!(err.to_string().contains("BAD VERSION"), "{err}");
        // v == "3" is taken before `gameId == "Sin"`, so the flag stays false and S2 wants 38.
        let err = openBsp(bspBytes(176, 4, 30, 8, &[]), "Sin", None, Some("3")).unwrap_err();
        assert!(err.to_string().contains("BAD VERSION"), "{err}");
        // Zero lumps leave the cursor on the S2 header, so X3 sees version 38.
        let err = openBsp(bspBytes(176, 4, 38, 8, &[]), "Quake", Some("idTech"), Some("3")).unwrap_err();
        assert!(err.to_string().contains("BAD VERSION"), "{err}");

        let mut both = bspBytes(176 + 144, 4, 38, 8, &[(0, 176, 0)]);
        putI32(&mut both, 176 + 4, 46);
        let bsp = openBsp(both, "Quake", Some("idTech"), Some("3")).unwrap();
        assert!(bsp.h1.isNone() && bsp.s2.isNone() && bsp.x3.isNone());

        // MODELS is lump 14. Count uses sizeof(X_DModel_S2) == 48, then the read is X_DModel_H1 (64 bytes).
        let short = bspBytes(176 + 48, 4, 38, 8, &[(14, 176, 48)]);
        let err = openBsp(short, "Quake", None, Some("3")).unwrap_err();
        assert!(!err.to_string().contains("BAD VERSION"), "{err}");
        let mut wide = bspBytes(176 + 64 + 144, 4, 38, 8, &[(0, 176 + 64, 0), (14, 176, 48)]);
        putI32(&mut wide, 176 + 64 + 4, 46);
        openBsp(wide, "Quake", None, Some("3")).unwrap();
    }

    #[test]
    fn lmpPaintsThroughTheColormap() {
        *PALETTE.lock().unwrap() = None;
        *COLORMAP.lock().unwrap() = None;
        let empty = <dyn Archive>::empty();
        let file = |path: &str| Arc::new(FileSource { path: path.into(), ..Default::default() });
        let mut stream: Stream = Box::new(Cursor::new(Vec::<u8>::new()));
        let err = getResult(Binary_Lmp::factory(&mut stream, file("gfx/box.lmp"), &*empty)).unwrap_err();
        assert!(err.to_string().contains("NotImplementedException"), "{err}");

        let mut pal = vec![0u8; 256 * 3];
        pal[21..24].copy_from_slice(&[1, 2, 3]);
        let mut stream: Stream = Box::new(Cursor::new(pal));
        getResult(Binary_Lmp::factory(&mut stream, file("palette.lmp"), &*empty)).unwrap();
        let mut map = vec![0u8; 32 * 256];
        map[4] = 7;
        let mut stream: Stream = Box::new(Cursor::new(map));
        getResult(Binary_Lmp::factory(&mut stream, file("gfx/colormap.lmp"), &*empty)).unwrap();

        let mut pic = Vec::new();
        pic.extend_from_slice(&1i32.to_le_bytes());
        pic.extend_from_slice(&1i32.to_le_bytes());
        pic.push(4);
        let picFile = file("gfx/bigbox.lmp");
        let mut stream: Stream = Box::new(Cursor::new(pic));
        let tag = getResult(Binary_Lmp::factory(&mut stream, picFile.clone(), &*empty)).unwrap();
        let Tag::Info(info) = tag else { panic!("info"); };
        let nodes = info.getInfoNodes(None, Some(&picFile), &Tag::None);
        assert_eq!(nodes[1].items[0].name.as_deref(), Some("Width: 1"));
        let Tag::MetaContent(content) = &nodes[0].tag else { panic!("content"); };
        let Tag::Texture(tex) = &content.value else { panic!("texture"); };
        let mut got = None;
        tex.create("", &mut |any| { got = any.downcast_ref::<TextureAsBytes>().map(|s| s.bytes.clone()); }).unwrap();
        assert_eq!(got.unwrap(), vec![1, 2, 3, 0]);
        assert_eq!(Binary_Lmp::toLightPixel(32 << 3, 4), vec![1, 2, 3, 0]);
    }

    #[test]
    fn mdlReadsNothingAndNamesTheModel() {
        let empty = <dyn Archive>::empty();
        let file = Arc::new(FileSource { path: "a.mdl".into(), ..Default::default() });
        let mut stream: Stream = Box::new(Cursor::new(Vec::<u8>::new()));
        let tag = getResult(Binary_Mdl::factory(&mut stream, file.clone(), &*empty)).unwrap();
        let Tag::Info(info) = tag else { panic!("info"); };
        let nodes = info.getInfoNodes(None, Some(&file), &Tag::None);
        let Tag::MetaContent(content) = &nodes[0].tag else { panic!("content"); };
        assert_eq!(content.value.asStr(), Some("UnknownFileModel File"));
        assert_eq!(nodes[1].items[0].name.as_deref(), Some("Records: 1"));
    }

    #[test]
    fn sprBlitsThePaletteAndRejectsABadMagic() {
        let empty = <dyn Archive>::empty();
        let mut bad: Stream = Box::new(Cursor::new(vec![0u8; 40]));
        let err = getResult(Binary_Spr::factory(&mut bad, Arc::new(FileSource::default()), &*empty)).unwrap_err();
        assert!(err.to_string().contains("BAD MAGIC"), "{err}");

        let mut bytes = Vec::new();
        bytes.extend_from_slice(&0x50534449u32.to_le_bytes());
        bytes.extend_from_slice(&1i32.to_le_bytes());
        bytes.extend_from_slice(&0i32.to_le_bytes());
        bytes.extend_from_slice(&0i32.to_le_bytes());
        bytes.extend_from_slice(&0f32.to_le_bytes());
        bytes.extend_from_slice(&1i32.to_le_bytes());
        bytes.extend_from_slice(&1i32.to_le_bytes());
        bytes.extend_from_slice(&1i32.to_le_bytes());
        bytes.extend_from_slice(&0f32.to_le_bytes());
        bytes.extend_from_slice(&0i32.to_le_bytes());
        bytes.extend_from_slice(&1u16.to_le_bytes());
        bytes.extend_from_slice(&[9, 8, 7]);
        for _ in 0..3 { bytes.extend_from_slice(&0i32.to_le_bytes()); }
        bytes.extend_from_slice(&1i32.to_le_bytes());
        bytes.extend_from_slice(&1i32.to_le_bytes());
        bytes.push(0);
        let file = Arc::new(FileSource { path: "a.spr".into(), ..Default::default() });
        let mut stream: Stream = Box::new(Cursor::new(bytes));
        let tag = getResult(Binary_Spr::factory(&mut stream, file.clone(), &*empty)).unwrap();
        let Tag::Info(info) = tag else { panic!("info"); };
        let nodes = info.getInfoNodes(None, Some(&file), &Tag::None);
        assert_eq!(nodes[1].items[0].name.as_deref(), Some("Frames: 1"));
        let Tag::MetaContent(content) = &nodes[0].tag else { panic!("content"); };
        let Tag::Texture(tex) = &content.value else { panic!("texture"); };
        assert!(tex.hasFrames());
        assert_eq!(tex.fps(), 60);
        assert!(tex.nextFrame());
        assert!(!tex.hasFrames());
        let mut got = None;
        tex.create("", &mut |any| { got = any.downcast_ref::<TextureAsBytes>().map(|s| s.bytes.clone()); }).unwrap();
        assert_eq!(got.unwrap(), vec![9, 8, 7, 255]);
    }
}
