// PORT-SOURCE: Families/GameX.Arkane/Formats/Danae/Binary.cs
// PORT-SHA: 9f89528785e1195b
// PORT-STATUS: done
//
// C#'s nested header structs are module-level here (Rust has no nested types),
// private where C# made them private. Marshalled ones are `bytemuck::Pod` +
// `IStruct`; their `ByValTStr` strings are `[u8; N]`, read with
// `FixedAStringScan` where C# reads them, and their `POLY` fields are the raw
// `i32` (`F_POLY.Type`), converted with `POLY::from_bits_retain`.
//
// Done differently:
//   * `ToString()` / `WriteToStream` are `this.Serialize()` in C#, i.e. JSON
//     through `DesSer`, which has no Rust counterpart yet. Here they write the
//     `{:#?}` debug form of the same public fields. `Value = this` in the
//     "Text" `MetaContent` is that string.
//   * `$"Obj: {Obj}"`, `$"Level: {Level}"`, `$"Bkg: {Bkg}"` and `$"Anim: {Anim}"`:
//     those types don't override `ToString`, so C# prints their type names; so does this.
//   * `using GameX.Bullfrog;` is unused in the C#: nothing to port.

use std::fmt;
use std::io::{self, Cursor, Error, ErrorKind, Read, Seek, SeekFrom, Write};
use std::sync::Arc;

use glam::{I16Vec3, IVec3, Quat, U16Vec3, Vec2, Vec3};

use gamex::binary::{Archive, Stream, Task};
use gamex::meta::{FileSource, IHaveMetaInfo, MetaContent, MetaInfo, MetaManager, Tag};
use gamex_uncore::formats::compression::Compression;
use openx_poly::io::i_stream::IWriteToStream;
use openx_poly::prelude::*;
use openx_poly::system::unsafe_x::FixedAStringScan;

use super::eerie_types::*;

// #region Binary_Ftl
// https://github.com/OpenSourcedGames/Arx-Fatalis/blob/master/Sources/DANAE/ARX_FTL.cpp#L575

#[derive(Debug)]
pub struct Binary_Ftl {
    pub Obj: E_3DOBJ,
}

// #region Headers

#[repr(C)]
#[derive(Debug, Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
struct FTL_HEADER {
    Offset3Ddata: i32,              // -1 = no
    OffsetCylinder: i32,            // -1 = no
    OffsetProgressiveData: i32,     // -1 = no
    OffsetClothesData: i32,         // -1 = no
    OffsetCollisionSpheres: i32,    // -1 = no
    OffsetPhysicsBox: i32,          // -1 = no
}
impl IStruct for FTL_HEADER { const Struct: (&'static str, usize) = ("<6i", 24); }

#[repr(C)]
#[derive(Debug, Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct FTL_3DHEADER {
    NumVertex: i32,
    NumFaces: i32,
    NumMaps: i32,
    NumGroups: i32,
    NumAction: i32,
    NumSelections: i32,
    Origin: i32,
    Name: [u8; 256],
}
impl IStruct for FTL_3DHEADER { const Struct: (&'static str, usize) = ("<7i256s", 28 + 256); }

#[repr(C)]
#[derive(Debug, Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
struct FTL_VERTEX {
    Vert: TLVERTEX,
    V: Vec3,
    Norm: Vec3,
}
// C#: $"{TLVERTEX.Struct.Item1}6f"
impl IStruct for FTL_VERTEX { const Struct: (&'static str, usize) = ("<4f2I2f6f", 32 + 24); }

impl FTL_VERTEX {
    fn to(mut s: FTL_VERTEX) -> E_VERTEX {
        s.Vert.Color = 0xFF000000;
        E_VERTEX {
            Vert: s.Vert,
            V: s.V,
            Norm: s.Norm,
            VWorld: Default::default(),
        }
    }
}

#[repr(C)]
#[derive(Debug, Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct FTL_TEXTURE {
    Name: [u8; 256],
}
impl IStruct for FTL_TEXTURE { const Struct: (&'static str, usize) = ("<256s", 256); }

impl FTL_TEXTURE {
    fn to(s: FTL_TEXTURE) -> E_TEXTURE {
        let name = FixedAStringScan(&s.Name, 256);
        let mut poly = POLY::empty();
        if name != "" {
            if name.contains("NPC_") { poly |= POLY::LATE_MIP; }
            if name.contains("nocol") { poly |= POLY::NOCOL; }
            if name.contains("climb") { poly |= POLY::CLIMB; } // change string depending on GFX guys
            if name.contains("fall") { poly |= POLY::FALL; }
            if name.contains("lava") { poly |= POLY::LAVA; }
            if name.contains("water") { poly |= POLY::WATER | POLY::TRANS; }
            else if name.contains("spider_web") { poly |= POLY::WATER | POLY::TRANS; }
            else if name.contains("[metal]") { poly |= POLY::METAL; }
        }
        E_TEXTURE {
            Path: name,
            Poly: poly,
            ..Default::default()
        }
    }
}

#[repr(C)]
#[derive(Debug, Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
struct FTL_FACE {
    FaceType: i32,  // 0 = flat, 1 = text, 2 = Double-Side
    Rgb: IVec3,
    Vid: U16Vec3,
    TexId: i16,
    U: Vec3,
    V: Vec3,
    Ou: I16Vec3,
    Ov: I16Vec3,
    TransVal: f32,
    Norm: Vec3,
    Nrmls0: Vec3, Nrmls1: Vec3, Nrmls2: Vec3,
    Temp: f32,
}
impl IStruct for FTL_FACE { const Struct: (&'static str, usize) = ("<4i3Hh6f6h14f", 116); }

impl FTL_FACE {
    fn to(s: FTL_FACE) -> E_FACE {
        E_FACE {
            FaceType: s.FaceType,
            TexId: s.TexId,
            U: s.U,
            V: s.V,
            Ou: s.Ou,
            Ov: s.Ov,
            TransVal: s.TransVal,
            Norm: s.Norm,
            Nrmls: Some(vec![s.Nrmls0, s.Nrmls1, s.Nrmls2]),
            Temp: s.Temp,
            ..Default::default()
        }
    }
}

#[repr(C)]
#[derive(Debug, Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct FTL_GROUPLIST {
    Name: [u8; 256],
    Origin: i32,
    NumIndex: i32,
    Trash: i32, // Indexes;
    Size: f32,
}
impl IStruct for FTL_GROUPLIST { const Struct: (&'static str, usize) = ("<256s3if", 256 + 16); }

impl FTL_GROUPLIST {
    fn to(s: FTL_GROUPLIST, indexes: Option<Vec<i32>>) -> E_GROUPLIST {
        E_GROUPLIST {
            Name: FixedAStringScan(&s.Name, 256),
            Origin: s.Origin,
            NumIndex: s.NumIndex,
            Indexes: indexes,
            Size: s.Size,
        }
    }
}

#[repr(C)]
#[derive(Debug, Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct FTL_ACTIONLIST {
    Name: [u8; 256],
    Idx: i32, //index vertex;
    Act: i32, //action
    Sfx: i32, //sfx
}
impl IStruct for FTL_ACTIONLIST { const Struct: (&'static str, usize) = ("<256s3i", 256 + 12); }

impl FTL_ACTIONLIST {
    fn to(s: FTL_ACTIONLIST) -> E_ACTIONLIST {
        E_ACTIONLIST {
            Name: FixedAStringScan(&s.Name, 256),
            Idx: s.Idx,
            Act: s.Act,
            Sfx: s.Sfx,
        }
    }
}

#[repr(C)]
#[derive(Debug, Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct FTL_SELECTIONS {
    Name: [u8; 64],
    NumSelected: i32,
    Trash: i32, //Selected;
}
impl IStruct for FTL_SELECTIONS { const Struct: (&'static str, usize) = ("<64s2i", 64 + 8); }

impl FTL_SELECTIONS {
    fn to(s: FTL_SELECTIONS, selected: Option<Vec<i32>>) -> E_SELECTIONS {
        E_SELECTIONS {
            Name: FixedAStringScan(&s.Name, 64),
            NumSelected: s.NumSelected,
            Selected: selected,
        }
    }
}

// #endregion

impl Binary_Ftl {
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { Box::pin(async move { Ok(Tag::Info(Arc::new(Binary_Ftl::new(r)?))) }) }

    const FTL_MAGIC: u32 = 0x004c5446;
    const FTL_VERSION: f32 = 0.83257;

    pub fn new<R: Read + Seek>(r: &mut R) -> io::Result<Self> {
        let mut obj = E_3DOBJ::default();
        let magic = r.readUInt32()?;
        if magic != Self::FTL_MAGIC { return Err(Error::new(ErrorKind::InvalidData, format!("Invalid FTL magic: \"{magic}\"."))); }
        let version = r.readSingle()?;
        if version != Self::FTL_VERSION { return Err(Error::new(ErrorKind::InvalidData, format!("Invalid FLT version: \"{version}\"."))); }
        r.skip(512)?; // skip checksum
        let header = r.readS::<FTL_HEADER>()?;

        // Check For & Load 3D Data
        if header.Offset3Ddata != -1 {
            r.seek(SeekFrom::Start(header.Offset3Ddata as u64))?;
            let s = r.readS::<FTL_3DHEADER>()?;
            obj.NumVertex = s.NumVertex;
            obj.NumFaces = s.NumFaces;
            obj.NumMaps = s.NumMaps;
            obj.NumGroups = s.NumGroups;
            obj.NumAction = s.NumAction;
            obj.NumSelections = s.NumSelections;
            obj.Origin = s.Origin;
            obj.File = FixedAStringScan(&s.Name, 256);
            obj.Vertexs = if s.NumVertex > 0 { Some(r.readSArray::<FTL_VERTEX>(s.NumVertex as usize, 0)?.into_iter().map(FTL_VERTEX::to).collect()) } else { None }; obj.Point0 = if s.NumVertex > 0 { obj.Vertexs.as_ref().unwrap()[obj.Origin as usize].V } else { Default::default() };
            obj.Faces = if s.NumFaces > 0 { Some(r.readSArray::<FTL_FACE>(s.NumFaces as usize, 0)?.into_iter().map(FTL_FACE::to).collect()) } else { None };
            obj.Textures = if s.NumMaps > 0 { Some(r.readSEach::<FTL_TEXTURE>(s.NumMaps as usize)?.into_iter().map(FTL_TEXTURE::to).collect()) } else { None };
            obj.Groups = if s.NumGroups > 0 { Some(r.readSEach::<FTL_GROUPLIST>(s.NumGroups as usize)?.into_iter().map(|s| Ok(FTL_GROUPLIST::to(s, if s.NumIndex > 0 { Some(r.readPArray::<i32>("i", s.NumIndex as usize)?) } else { None }))).collect::<io::Result<_>>()?) } else { None };
            obj.Actions = if s.NumAction > 0 { Some(r.readSEach::<FTL_ACTIONLIST>(s.NumAction as usize)?.into_iter().map(FTL_ACTIONLIST::to).collect()) } else { None };
            obj.Selections = if s.NumSelections > 0 { Some(r.readSEach::<FTL_SELECTIONS>(s.NumSelections as usize)?.into_iter().map(|s| Ok(FTL_SELECTIONS::to(s, Some(r.readPArray::<i32>("i", s.NumSelected as usize)?)))).collect::<io::Result<_>>()?) } else { None };
        }

        // collision spheres
        if header.OffsetCollisionSpheres != -1 {
            r.seek(SeekFrom::Start(header.OffsetCollisionSpheres as u64))?;
            obj.Spheres = Some(r.readL32SArray::<COLLISION_SPHERE>(0, false)?);
        }

        // progressive data
        if header.OffsetProgressiveData != -1 {
            r.seek(SeekFrom::Start(header.OffsetProgressiveData as u64))?;
            let numVertex = r.readInt32()?;
            r.skip((PROGRESSIVE_DATA::SIZEOF * numVertex) as i64)?;
        }

        // clothes data
        if header.OffsetClothesData != -1 {
            r.seek(SeekFrom::Start(header.OffsetClothesData as u64))?;
            let (numCvert, numSprings) = (r.readInt32()?, r.readInt32()?);
            obj.Cdata = Some(CLOTHES_DATA {
                Cvert: Some(r.readSArray::<CLOTHESVERTEX>(numCvert as usize, 0)?),
                Springs: Some(r.readSArray::<E_SPRINGS>(numSprings as usize, 0)?),
            });
        }

        // process
        obj.centerObjectCoordinates();
        obj.createCedricData();
        obj.precomputeFastAccess();
        Ok(Self { Obj: obj })
    }
}

impl IWriteToStream for Binary_Ftl {
    fn writeToStream(&self, stream: &mut dyn Write) -> io::Result<()> { stream.write_all(self.to_string().as_bytes()) }
}

impl fmt::Display for Binary_Ftl {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { write!(f, "{self:#?}") }
}

impl IHaveMetaInfo for Binary_Ftl {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent { type_: "Text".into(), name: "Name".into(), value: Tag::Str(self.to_string()), ..Default::default() })), vec![], false),
        MetaInfo::new(Some("FTL".into()), Tag::None, vec![
            MetaInfo::new(Some(format!("Obj: {}", "GameX.Arkane.Formats.Danae.E_3DOBJ")), Tag::None, vec![], false),
        ], false),
    ] }
}

// #endregion

// #region Binary_Fts
// https://github.com/OpenSourcedGames/Arx-Fatalis/blob/master/Sources/EERIE/EERIEPoly.cpp#L3755

pub struct Binary_Fts {
    pub Level: F_LEVEL,
    Bkg: E_BACKGROUND,
}

// #region Headers

#[derive(Debug, Clone, Default)]
struct ANCHOR_DATA {
    Pos: Vec3,
    NumLinked: i16,
    Flags: i16,
    Linked: Option<Vec<i32>>,
    Radius: f32,
    Height: f32,
}

#[derive(Debug, Clone, Default)]
pub struct E_BKG_INFO {
    pub Treat: u8,
    pub Nothing: bool,
    pub NumPoly: i16,
    pub NumIAnchors: i16,
    pub NumPolyin: i16,
    pub FrustrumMinY: f32,
    pub FrustrumMaxY: f32,
    pub Polydata: Option<Vec<E_POLY>>,
    //pub Polyin: Option<Vec<Vec<E_POLY>>>,
    pub IAnchors: Option<Vec<i32>>, // index on anchors list
    pub Flags: i32,
    pub TileMinY: f32,
    pub TileMaxY: f32,
}

#[derive(Debug, Clone, Copy, Default)]
struct E_SMINMAX {
    Min: i16,
    Max: i16,
}

//const MAX_GOSUB: i32 = 10;
//const MAX_SHORTCUT: i32 = 80;
//const MAX_SCRIPTTIMERS: i32 = 5;
//const FBD_TREAT: i32 = 1;
//const FBD_NOTHING: i32 = 2;

#[derive(Debug, Clone, Default)]
struct F_BKG_DATA {
    Treat: u8,
    Nothing: u8,
    NumPoly: i16,
    NumIAnchors: i16,
    NumPolyin: i16,
    Flags: i32,
    FrustrumMinY: f32,
    FrustrumMaxY: f32,
    Polydata: Option<Vec<E_POLY>>,
    Polyin: Option<Vec<Vec<E_POLY>>>,
    IAnchors: Option<Vec<i32>>, // index on anchors list
}

const MAX_BKGX: i32 = 160; const MAX_BKGZ: i32 = 160; const BKG_SIZX: i32 = 100; const BKG_SIZZ: i32 = 100;

#[derive(Debug, Clone)]
struct E_BACKGROUND {
    fastdata: Vec<Vec<F_BKG_DATA>>,
    exist: i32,
    XSize: i16,
    ZSize: i16,
    Xdiv: i16,
    Zdiv: i16,
    Xmul: f32,
    Zmul: f32,
    Backg: Vec<E_BKG_INFO>,
    Ambient: Vec3,
    Ambient255: Vec3,
    MinMax: Vec<E_SMINMAX>,
    NumAnchors: i32,
    Anchors: Option<Vec<ANCHOR_DATA>>,
    Name: String,
}

impl E_BACKGROUND {
    /// C# defaults: `sx = MAX_BKGX, sz = MAX_BKGZ, xdiv = BKG_SIZX, zdiv = BKG_SIZZ`.
    fn new(sx: i16, sz: i16, mut xdiv: i16, mut zdiv: i16) -> Self {
        if xdiv < 0 { xdiv = 1; }
        if zdiv < 0 { zdiv = 1; }
        let mut Backg = vec![E_BKG_INFO::default(); sx as usize * sz as usize];
        for i in 0..Backg.len() { Backg[i].Nothing = true; }
        let mut MinMax = vec![E_SMINMAX::default(); sz as usize];
        for i in 0..MinMax.len() { MinMax[i].Min = 9999; MinMax[i].Max = -1; }
        Self {
            fastdata: vec![vec![F_BKG_DATA::default(); MAX_BKGZ as usize]; MAX_BKGX as usize],
            exist: 1,
            XSize: sx,
            ZSize: sz,
            Xdiv: xdiv,
            Zdiv: zdiv,
            Xmul: 1.0 / xdiv as f32,
            Zmul: 1.0 / zdiv as f32,
            Backg,
            Ambient: Vec3::ZERO,
            Ambient255: Vec3::ZERO,
            MinMax,
            NumAnchors: 0,
            Anchors: None,
            Name: String::new(),
        }
    }
}

//const NON_PORTAL_VERSION: f32 = 0.136;
//const SIZ_WRK: i32 = 10;

#[repr(C)]
#[derive(Debug, Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct FTS_HEADER {
    Path: [u8; 256],
    Count: i32,
    Version: f32,
    Compressedsize: i32,
    Pad: [i32; 3],
}
impl IStruct for FTS_HEADER { const Struct: (&'static str, usize) = ("<256sifi3i", 256 + 24); }

#[repr(C)]
#[derive(Debug, Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct FTS_HEADER2 {
    Path: [u8; 256],
}
impl IStruct for FTS_HEADER2 { const Struct: (&'static str, usize) = ("<256s", 256); }

#[repr(C)]
#[derive(Debug, Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
struct F_VERTEX {
    //Struct: ("<5f", 20)
    sy: f32,
    ssx: f32,
    ssz: f32,
    stu: f32,
    stv: f32,
}

#[repr(C)]
#[derive(Debug, Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
struct F_POLY {
    V0: F_VERTEX, V1: F_VERTEX, V2: F_VERTEX, V3: F_VERTEX,
    TexPtr: i32,
    Norm: Vec3, Norm2: Vec3,
    Nrml0: Vec3, Nrml1: Vec3, Nrml2: Vec3, Nrml3: Vec3,
    TransVal: f32,
    Area: f32,
    Type: i32, // POLY
    Room: i16,
    Paddy: i16,
}
impl IStruct for F_POLY { const Struct: (&'static str, usize) = ("<20fi20fi2h", 172); }

impl F_POLY {
    fn to(s: F_POLY, textures: &[E_TEXTURE], bkg: &mut E_BACKGROUND) -> E_POLY {
        fn declareEGInfo(bkg: &mut E_BACKGROUND, x: f32, y: f32, z: f32) {
            let posx = (x * bkg.Xmul) as i32;
            if posx < 0 { return; }
            else if posx >= bkg.XSize as i32 { return; }
            let posz = (z * bkg.Zmul) as i32;
            if posz < 0 { return; }
            else if posz >= bkg.ZSize as i32 { return; }
            let eg = &mut bkg.Backg[(posx + posz * bkg.XSize as i32) as usize];
            eg.Nothing = false;
        }

        let texPtr = s.TexPtr;
        let Type = POLY::from_bits_retain(s.Type);
        let mut t = E_POLY {
            Room: s.Room,
            Area: s.Area,
            Norm: s.Norm,
            Norm2: s.Norm2,
            Nrml: Some(vec![s.Nrml0, s.Nrml1, s.Nrml2, s.Nrml3]),
            Tex: if texPtr != 0 { textures.iter().find(|x| x.Id == texPtr).cloned() } else { None },
            TransVal: s.TransVal,
            Type,
            V: Some(vec![
                TLVERTEX { Color: 0xFFFFFFFF, Rhw: 1.0, Specular: 1, S: Vec3::new(s.V0.ssx, s.V0.sy, s.V0.ssz), T: Vec2::new(s.V0.stu, s.V0.stv) },
                TLVERTEX { Color: 0xFFFFFFFF, Rhw: 1.0, Specular: 1, S: Vec3::new(s.V1.ssx, s.V1.sy, s.V1.ssz), T: Vec2::new(s.V1.stu, s.V1.stv) },
                TLVERTEX { Color: 0xFFFFFFFF, Rhw: 1.0, Specular: 1, S: Vec3::new(s.V2.ssx, s.V2.sy, s.V2.ssz), T: Vec2::new(s.V2.stu, s.V2.stv) },
                TLVERTEX { Color: 0xFFFFFFFF, Rhw: 1.0, Specular: 1, S: Vec3::new(s.V3.ssx, s.V3.sy, s.V3.ssz), T: Vec2::new(s.V3.stu, s.V3.stv) },
            ]),
            ..Default::default()
        };

        // clone v
        t.Tv = t.V.clone();
        for kk in 0..4 { t.Tv.as_mut().unwrap()[kk].Color = 0xFF000000; }

        // re-center
        let (to, div): (usize, f32);
        if Type.intersects(POLY::QUAD) { to = 4; div = 0.25; }
        else { to = 3; div = 0.333333333333; }
        t.Center = Vec3::ZERO;
        let V = t.V.as_mut().unwrap();
        for h in 0..to {
            t.Center += V[h].S;
            if h != 0 {
                t.Max.x = t.Max.x.max(V[h].S.x); t.Min.x = t.Min.x.min(V[h].S.x);
                t.Max.y = t.Max.y.max(V[h].S.y); t.Min.y = t.Min.y.min(V[h].S.y);
                t.Max.z = t.Max.z.max(V[h].S.z); t.Min.z = t.Min.z.min(V[h].S.z);
            }
            else { t.Max = V[0].S; t.Min = t.Max; }
        }
        t.Center *= div;

        // distance
        let mut dist = 0f32;
        for h in 0..to { dist = dist.max(V[h].S.distance(t.Center)); }
        V[0].Rhw = dist;

        // declare
        declareEGInfo(bkg, t.Center.x, t.Center.y, t.Center.z);
        declareEGInfo(bkg, V[0].S.x, V[0].S.y, V[0].S.z);
        declareEGInfo(bkg, V[1].S.x, V[1].S.y, V[1].S.z);
        declareEGInfo(bkg, V[2].S.x, V[2].S.y, V[2].S.z);
        if Type.intersects(POLY::QUAD) { declareEGInfo(bkg, V[3].S.x, V[3].S.y, V[3].S.z); }
        t
    }
}

#[repr(C)]
#[derive(Debug, Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
struct F_SCENE_HEADER {
    Version: f32,
    SizeX: i32,
    SizeZ: i32,
    NumTextures: i32,
    NumPolys: i32,
    NumAnchors: i32,
    PlayerPos: Vec3,
    MscenePos: Vec3,
    NumPortals: i32,
    NumRooms: i32,
}
impl IStruct for F_SCENE_HEADER { const Struct: (&'static str, usize) = ("<f5i6f2i", 56); }

#[repr(C)]
#[derive(Debug, Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct F_TEXTURE_CONTAINER {
    TcPtr: i32,
    TempPtr: i32,
    Fic: [u8; 256],
}
impl IStruct for F_TEXTURE_CONTAINER { const Struct: (&'static str, usize) = ("<2i256s", 8 + 256); }

impl F_TEXTURE_CONTAINER {
    fn to(s: F_TEXTURE_CONTAINER) -> E_TEXTURE {
        E_TEXTURE {
            Id: s.TcPtr,
            Path: FixedAStringScan(&s.Fic, 256),
            ..Default::default()
        }
    }
}

#[repr(C)]
#[derive(Debug, Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
struct F_ANCHOR_DATA {
    Pos: Vec3,
    Radius: f32,
    Height: f32,
    NumLinked: i16,
    Flags: i16,
}
impl IStruct for F_ANCHOR_DATA { const Struct: (&'static str, usize) = ("<5f2h", 24); }

#[repr(C)]
#[derive(Debug, Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
struct F_SCENE_INFO {
    NumPoly: i32,
    NumIAnchors: i32,
}
impl IStruct for F_SCENE_INFO { const Struct: (&'static str, usize) = ("<2I", 8); }

#[repr(C)]
#[derive(Debug, Clone, Copy, Default, bytemuck::Pod, bytemuck::Zeroable)]
pub struct ROOM_DIST_DATA { //:includes ROOM_DIST_DATA_SAVE
    pub Distance: f32, // -1 means use truedist
    pub StartPos: Vec3,
    pub EndPos: Vec3,
}
impl IStruct for ROOM_DIST_DATA { const Struct: (&'static str, usize) = ("<7f", 28); }

#[derive(Debug, Clone, Default)]
pub struct F_LEVEL {
    pub PlayerPos: Vec3,
    pub MscenePos: Vec3,
    pub Textures: Option<Vec<E_TEXTURE>>,
    pub Backg: Option<Vec<E_BKG_INFO>>,
    pub Portals: Option<E_PORTAL_DATA>,
    pub NumRoomDistance: i32,
    pub RoomDistance: Option<Vec<ROOM_DIST_DATA>>,
}

// #endregion

impl Binary_Fts {
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { Box::pin(async move { Ok(Tag::Info(Arc::new(Binary_Fts::new(r)?))) }) }

    const FTS_VERSION: f32 = 0.141;

    pub fn new<R: Read + Seek>(r: &mut R) -> io::Result<Self> {
        fn setRoomDistance(level: &mut F_LEVEL, i: i64, j: i64, rdd: ROOM_DIST_DATA) {
            if i < 0 || j < 0 || i >= level.NumRoomDistance as i64 || j >= level.NumRoomDistance as i64 || level.RoomDistance.is_none() { return; }
            level.RoomDistance.as_mut().unwrap()[(i + j * level.NumRoomDistance as i64) as usize] = rdd;
        }

        let header = r.readS::<FTS_HEADER>()?;
        if header.Version != Self::FTS_VERSION { return Err(Error::new(ErrorKind::InvalidData, "BAD MAGIC")); }
        if header.Count > 0 {
            let mut count = 0;
            while count < header.Count {
                r.readS::<FTS_HEADER2>()?;
                r.skip(512)?; // skip check
                count += 1;
                if count > 60 { return Err(Error::new(ErrorKind::InvalidData, "BAD HEADER")); }
            }
        }

        let mut Level = F_LEVEL::default();
        let mut Bkg = E_BACKGROUND::new(MAX_BKGX as i16, MAX_BKGZ as i16, BKG_SIZX as i16, BKG_SIZZ as i16);
        let position = r.tell()?; let length = r.seek(SeekFrom::End(0))?; r.seek(SeekFrom::Start(position))?;
        let s = r.decompressBlast((length - position) as i32, header.Compressedsize)?;
        let mut r2 = Cursor::new(s);
        {
            // read
            let fsh = r2.readS::<F_SCENE_HEADER>()?;
            if fsh.Version != Self::FTS_VERSION { return Err(Error::new(ErrorKind::InvalidData, "BAD MAGIC")); }
            if fsh.SizeX != Bkg.XSize as i32 { return Err(Error::new(ErrorKind::InvalidData, "BAD HEADER")); }
            if fsh.SizeZ != Bkg.ZSize as i32 { return Err(Error::new(ErrorKind::InvalidData, "BAD HEADER")); }
            Level.PlayerPos = fsh.PlayerPos;
            Level.MscenePos = fsh.MscenePos;
            //eprintln!("Header2: {:x}, 56", r2.tell()?);

            // textures
            let textures = &*Level.Textures.insert(r2.readSArray::<F_TEXTURE_CONTAINER>(fsh.NumTextures as usize, 0)?.into_iter().map(F_TEXTURE_CONTAINER::to).collect());
            //eprintln!("Texture: {:x}", r2.tell()?);

            // backg
            for j in 0..fsh.SizeZ {
                for i in 0..fsh.SizeX {
                    let fsi = r2.readS::<F_SCENE_INFO>()?;
                    //if fsi.NumPoly > 0 { eprintln!("F[{j},{i}]: {:x}, {}, {}", r2.tell()?, fsi.NumPoly, fsi.NumIAnchors); }
                    // F_POLY::to writes `Bkg.Backg[..].Nothing`, so the polys are read before `bi` borrows the cell; `Nothing` is set below either way.
                    let Polydata = if fsi.NumPoly > 0 { Some(r2.readSArray::<F_POLY>(fsi.NumPoly as usize, 0)?.into_iter().map(|s| F_POLY::to(s, textures, &mut Bkg)).collect()) } else { None };
                    let bi = &mut Bkg.Backg[(i + j * fsh.SizeX) as usize];
                    bi.NumIAnchors = fsi.NumIAnchors as i16;
                    bi.NumPoly = fsi.NumPoly as i16;
                    bi.Polydata = Polydata;
                    bi.Treat = 0;
                    bi.Nothing = fsi.NumPoly == 0;
                    bi.FrustrumMaxY = -99999999.0;
                    bi.FrustrumMinY = 99999999.0;
                    bi.IAnchors = if fsi.NumIAnchors <= 0 { None } else { Some(r2.readPArray::<i32>("i", fsi.NumIAnchors as usize)?) };
                }
            }
            //eprintln!("Background: {:x}", r2.tell()?);

            // anchors
            Bkg.NumAnchors = fsh.NumAnchors;
            Bkg.Anchors = if fsh.NumAnchors > 0 { Some(vec![ANCHOR_DATA::default(); fsh.NumAnchors as usize]) } else { None };
            for i in 0..fsh.NumAnchors {
                let a = &mut Bkg.Anchors.as_mut().unwrap()[i as usize];
                let fad = r2.readS::<F_ANCHOR_DATA>()?;
                a.Flags = fad.Flags;
                a.Pos = fad.Pos;
                a.NumLinked = fad.NumLinked;
                a.Height = fad.Height;
                a.Radius = fad.Radius;
                a.Linked = if fad.NumLinked > 0 { Some(r2.readPArray::<i32>("i", fad.NumLinked as usize)?) } else { None };
            }
            //eprintln!("Anchors: {:x}", r2.tell()?);

            // rooms
            if fsh.NumRooms > 0 {
                let portals = Level.Portals.insert(E_PORTAL_DATA {
                    NumRooms: fsh.NumRooms,
                    Room: Some(vec![E_ROOM_DATA::default(); (fsh.NumRooms + 1) as usize]),
                    NumTotal: fsh.NumPortals,
                    Portals: Some(r2.readSArray::<E_SAVE_PORTALS>(fsh.NumPortals as usize, 0)?.into_iter().map(E_SAVE_PORTALS::to).collect()),
                });
                for i in 0..(portals.NumRooms + 1) as usize {
                    let x = r2.readS::<E_SAVE_ROOM_DATA>()?;
                    portals.Room.as_mut().unwrap()[i] = E_ROOM_DATA {
                        NumPortals: x.NumPortals,
                        NumPolys: x.NumPolys,
                        Portals: if x.NumPortals > 0 { Some(r2.readPArray::<i32>("i", x.NumPortals as usize)?) } else { None },
                        EpData: if x.NumPolys > 0 { Some(r2.readSArray::<EP_DATA>(x.NumPolys as usize, 0)?) } else { None },
                        ..Default::default()
                    };
                }
            }
            //eprintln!("Portals: {:x}", r2.tell()?);
            if let Some(portals) = &Level.Portals {
                let numRoomDistance = portals.NumRooms + 1; Level.NumRoomDistance = numRoomDistance;
                Level.RoomDistance = Some(vec![ROOM_DIST_DATA::default(); (numRoomDistance * numRoomDistance) as usize]);
                for n in 0..numRoomDistance {
                    for m in 0..numRoomDistance {
                        setRoomDistance(&mut Level, m as i64, n as i64, r2.readS::<ROOM_DIST_DATA>()?);
                    }
                }
            }
            else { Level.NumRoomDistance = 0; Level.RoomDistance = None; }
            //eprintln!("RoomDistance: {:x}", r2.tell()?);
        }
        Self::computePolyIn();
        //PATHFINDER_Create();
        //PORTAL_Blend_Portals_And_Rooms();
        //ComputePortalVertexBuffer();
        Ok(Self { Level, Bkg })
    }

    fn computePolyIn() {
    }
}

impl IWriteToStream for Binary_Fts {
    fn writeToStream(&self, stream: &mut dyn Write) -> io::Result<()> { stream.write_all(self.to_string().as_bytes()) }
}

/// C#'s serializer writes public fields only, so `Bkg` is left out.
impl fmt::Debug for Binary_Fts {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { f.debug_struct("Binary_Fts").field("Level", &self.Level).finish() }
}

impl fmt::Display for Binary_Fts {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { write!(f, "{self:#?}") }
}

impl IHaveMetaInfo for Binary_Fts {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent { type_: "Text".into(), name: "Name".into(), value: Tag::Str(self.to_string()), ..Default::default() })), vec![], false),
        MetaInfo::new(Some("FTS".into()), Tag::None, vec![
            MetaInfo::new(Some(format!("Level: {}", "GameX.Arkane.Formats.Danae.Binary_Fts+F_LEVEL")), Tag::None, vec![], false),
            MetaInfo::new(Some(format!("Bkg: {}", "GameX.Arkane.Formats.Danae.Binary_Fts+E_BACKGROUND")), Tag::None, vec![], false),
        ], false),
    ] }
}

// #endregion

// #region Binary_Tea
// https://github.com/OpenSourcedGames/Arx-Fatalis/blob/master/Sources/EERIE/EERIEobject.cpp#L264

const DIV24: f32 = 0.0416666666666666666667;

#[repr(C)]
#[derive(Debug, Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct THEA_HEADER {
    Identity: [u8; 20],
    Version: u32,
    AnimName: [u8; 256],
    NbFrames: i32,
    NbGroups: i32,
    NbKeyFrames: i32,
}
impl IStruct for THEA_HEADER { const Struct: (&'static str, usize) = ("<20sI256s3i", 292); }

#[repr(C)]
#[derive(Debug, Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct THEA_KEYFRAME {
    NumFrame: i32,
    FlagFrame: i32,
    MasterKeyFrame: i32,
    KeyFrame: i32,
    KeyMove: i32,
    KeyOrient: i32,
    KeyMorph: i32,
    TimeFrame: i32,
}
impl IStruct for THEA_KEYFRAME { const Struct: (&'static str, usize) = ("<8i", 32); }

#[repr(C)]
#[derive(Debug, Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct THEA_KEYFRAME_2015 {
    NumFrame: i32,
    FlagFrame: i32,
    InfoFrame: [u8; 256],
    MasterKeyFrame: i32,
    KeyFrame: i32,
    KeyMove: i32,
    KeyOrient: i32,
    KeyMorph: i32,
    TimeFrame: i32,
}
impl IStruct for THEA_KEYFRAME_2015 { const Struct: (&'static str, usize) = ("<2i256s6i", 288); }

#[repr(C)]
#[derive(Debug, Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct THEA_KEYMOVE { V: Vec3 }
impl IStruct for THEA_KEYMOVE { const Struct: (&'static str, usize) = ("<3f", 12); }

// On disk w, x, y, z. Runtime `Quat` is x, y, z, w.
#[repr(C)]
#[derive(Debug, Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct ArxQuat {
    W: f32,
    X: f32,
    Y: f32,
    Z: f32,
}
impl IStruct for ArxQuat { const Struct: (&'static str, usize) = ("<4f", 16); }

#[repr(C)]
#[derive(Debug, Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct THEO_GROUPANIM {
    KeyGroup: i32,
    Angle: [u8; 8],
    QuatW: f32,
    QuatX: f32,
    QuatY: f32,
    QuatZ: f32,
    Translate: Vec3,
    Zoom: Vec3,
}
impl IStruct for THEO_GROUPANIM { const Struct: (&'static str, usize) = ("<i8s4f3f3f", 52); }

#[repr(C)]
#[derive(Debug, Clone, Copy, bytemuck::Pod, bytemuck::Zeroable)]
struct THEA_SAMPLE {
    SampleName: [u8; 256],
    SampleSize: i32,
}
impl IStruct for THEA_SAMPLE { const Struct: (&'static str, usize) = ("<256si", 260); }

fn getTimeBetweenKeyFrames(ea: &E_ANIM, f1: i32, f2: i32) -> f32 {
    if f1 < 0 { return 0.0; }
    if f1 > ea.NbKeyFrames - 1 { return 0.0; }
    if f2 < 0 { return 0.0; }
    if f2 > ea.NbKeyFrames - 1 { return 0.0; }
    let mut time = 0.0f32;
    let mut kk = f1 + 1;
    while kk <= f2 {
        time += ea.Frames[kk as usize].Time;
        kk += 1;
    }
    time
}

#[derive(Debug)]
pub struct Binary_Tea {
    pub Name: String,
    pub Version: u32,
    pub Anim: E_ANIM,
}

impl Binary_Tea {
    pub fn factory<'a>(r: &'a mut Stream, f: Arc<FileSource>, s: &'a dyn Archive) -> Task<'a, Tag> { Box::pin(async move { Ok(Tag::Info(Arc::new(Binary_Tea::new(r)?))) }) }

    pub fn new<R: Read + Seek>(r: &mut R) -> io::Result<Self> {
        let th = r.readS::<THEA_HEADER>()?;
        if th.Version < 2014 { return Err(Error::new(ErrorKind::InvalidData, format!("Invalid TEA Version: {}", th.Version))); }
        let _identity = FixedAStringScan(&th.Identity, 20);
        let name = FixedAStringScan(&th.AnimName, 256);
        let (nbKey, nbGroups) = (th.NbKeyFrames, th.NbGroups);
        let mut eerie = E_ANIM {
            AnimTime: 0.0,
            Flag: 0,
            NbGroups: nbGroups,
            NbKeyFrames: nbKey,
            Frames: vec![E_FRAME::default(); nbKey as usize],
            Groups: vec![E_GROUP::default(); (nbKey * nbGroups) as usize],
            VoidGroups: vec![0u8; nbGroups as usize],
        };
        let mut i = 0i32;
        while i < nbKey {
            let mut tkf2015 = if th.Version >= 2015 {
                r.readS::<THEA_KEYFRAME_2015>()?
            } else {
                let tkf = r.readS::<THEA_KEYFRAME>()?;
                THEA_KEYFRAME_2015 {
                    NumFrame: tkf.NumFrame,
                    FlagFrame: tkf.FlagFrame,
                    InfoFrame: [0u8; 256],
                    MasterKeyFrame: tkf.MasterKeyFrame,
                    KeyFrame: tkf.KeyFrame,
                    KeyMove: tkf.KeyMove,
                    KeyOrient: tkf.KeyOrient,
                    KeyMorph: tkf.KeyMorph,
                    TimeFrame: tkf.TimeFrame,
                }
            };
            eerie.Frames[i as usize].MasterKeyFrame = tkf2015.MasterKeyFrame;
            eerie.Frames[i as usize].NumFrame = tkf2015.NumFrame;
            eerie.Frames[i as usize].FRotate = tkf2015.KeyOrient as i16;
            eerie.Frames[i as usize].FTranslate = tkf2015.KeyMove as i16;
            tkf2015.TimeFrame = tkf2015.NumFrame * 1000; // file time_frame is discarded
            eerie.Frames[i as usize].Time = tkf2015.TimeFrame as f32 * DIV24;
            eerie.AnimTime += tkf2015.TimeFrame as f32;
            eerie.Frames[i as usize].Flag = tkf2015.FlagFrame;
            if tkf2015.KeyMove == 1 {
                let tkm = r.readS::<THEA_KEYMOVE>()?;
                eerie.Frames[i as usize].Translate = tkm.V;
            }
            if tkf2015.KeyOrient == 1 {
                r.skip(8)?; // THEO_ANGLE
                let quat = r.readS::<ArxQuat>()?;
                eerie.Frames[i as usize].Quat = Quat::from_xyzw(quat.X, quat.Y, quat.Z, quat.W);
            }
            if tkf2015.KeyMorph == 1 { r.skip(16)?; } // THEA_MORPH
            let mut j = 0i32;
            while j < nbGroups {
                let tga = r.readS::<THEO_GROUPANIM>()?;
                let eg = &mut eerie.Groups[(j + i * nbGroups) as usize];
                eg.Key = tga.KeyGroup;
                eg.Quat = Quat::from_xyzw(tga.QuatX, tga.QuatY, tga.QuatZ, tga.QuatW);
                eg.Translate = tga.Translate;
                eg.Zoom = tga.Zoom;
                j += 1;
            }
            let numSample = r.readInt32()?;
            eerie.Frames[i as usize].Sample = -1;
            if numSample != -1 {
                let ts = r.readS::<THEA_SAMPLE>()?;
                r.skip(ts.SampleSize as i64)?;
                eerie.Frames[i as usize].SampleName = Some(FixedAStringScan(&ts.SampleName, 256));
            }
            r.readInt32()?; // num_sfx, discarded
            i += 1;
        }
        i = 0;
        while i < nbKey {
            if eerie.Frames[i as usize].FTranslate == 0 {
                let mut k = i;
                while k >= 0 && eerie.Frames[k as usize].FTranslate == 0 { k -= 1; }
                let mut j = i;
                while j < nbKey && eerie.Frames[j as usize].FTranslate == 0 { j += 1; }
                if j < nbKey && k >= 0 {
                    let mut r1 = getTimeBetweenKeyFrames(&eerie, k, i);
                    let mut r2 = getTimeBetweenKeyFrames(&eerie, i, j);
                    let tot = 1.0 / (r1 + r2);
                    r1 *= tot;
                    r2 *= tot;
                    let tj = eerie.Frames[j as usize].Translate;
                    let tk = eerie.Frames[k as usize].Translate;
                    eerie.Frames[i as usize].Translate = tj * r1 + tk * r2;
                }
            }
            if eerie.Frames[i as usize].FRotate == 0 {
                let mut k = i;
                while k >= 0 && eerie.Frames[k as usize].FRotate == 0 { k -= 1; }
                let mut j = i;
                while j < nbKey && eerie.Frames[j as usize].FRotate == 0 { j += 1; }
                if j < nbKey && k >= 0 {
                    let mut r1 = getTimeBetweenKeyFrames(&eerie, k, i);
                    let mut r2 = getTimeBetweenKeyFrames(&eerie, i, j);
                    let tot = 1.0 / (r1 + r2);
                    r1 *= tot;
                    r2 *= tot;
                    let qj = eerie.Frames[j as usize].Quat;
                    let qk = eerie.Frames[k as usize].Quat;
                    eerie.Frames[i as usize].Quat = Quat::from_xyzw(
                        qj.x * r1 + qk.x * r2,
                        qj.y * r1 + qk.y * r2,
                        qj.z * r1 + qk.z * r2,
                        qj.w * r1 + qk.w * r2,
                    );
                }
            }
            i += 1;
        }
        for fr in &mut eerie.Frames {
            fr.FTranslate = 1;
            fr.FRotate = 1;
        }
        let mut gi = 0i32;
        while gi < eerie.NbGroups {
            let mut voidd = 1i32;
            let mut j = 0i32;
            while j < eerie.NbKeyFrames {
                let g = eerie.Groups[(gi + j * eerie.NbGroups) as usize];
                if g.Quat.x != 0.0 || g.Quat.y != 0.0 || g.Quat.z != 0.0 || g.Quat.w != 1.0
                    || g.Translate.x != 0.0 || g.Translate.y != 0.0 || g.Translate.z != 0.0
                    || g.Zoom.x != 0.0 || g.Zoom.y != 0.0 || g.Zoom.z != 0.0
                {
                    voidd = 0;
                    break;
                }
                j += 1;
            }
            if voidd != 0 { eerie.VoidGroups[gi as usize] = 1; }
            gi += 1;
        }
        eerie.AnimTime = th.NbFrames as f32 * 1000.0 * DIV24;
        if eerie.AnimTime < 1.0 { eerie.AnimTime = 1.0; }
        Ok(Self { Name: name, Version: th.Version, Anim: eerie })
    }
}

impl fmt::Display for Binary_Tea {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { write!(f, "{self:#?}") }
}

impl IHaveMetaInfo for Binary_Tea {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent { type_: "Text".into(), name: "Name".into(), value: Tag::Str(self.to_string()), ..Default::default() })), vec![], false),
        MetaInfo::new(Some("TEA".into()), Tag::None, vec![
            MetaInfo::new(Some(format!("Anim: {}", "GameX.Arkane.Formats.Danae.E_ANIM")), Tag::None, vec![], false),
        ], false),
    ] }
}

// #endregion

#[cfg(test)]
mod tests {
    use super::*;
    use std::mem::size_of;

    #[test]
    fn structSizesMatchPatterns() {
        assert_eq!(size_of::<FTL_HEADER>(), FTL_HEADER::Struct.1);
        assert_eq!(size_of::<FTL_3DHEADER>(), FTL_3DHEADER::Struct.1);
        assert_eq!(size_of::<FTL_VERTEX>(), FTL_VERTEX::Struct.1);
        assert_eq!(size_of::<FTL_TEXTURE>(), FTL_TEXTURE::Struct.1);
        assert_eq!(size_of::<FTL_FACE>(), FTL_FACE::Struct.1);
        assert_eq!(size_of::<FTL_GROUPLIST>(), FTL_GROUPLIST::Struct.1);
        assert_eq!(size_of::<FTL_ACTIONLIST>(), FTL_ACTIONLIST::Struct.1);
        assert_eq!(size_of::<FTL_SELECTIONS>(), FTL_SELECTIONS::Struct.1);
        assert_eq!(size_of::<FTS_HEADER>(), FTS_HEADER::Struct.1);
        assert_eq!(size_of::<FTS_HEADER2>(), FTS_HEADER2::Struct.1);
        assert_eq!(size_of::<F_POLY>(), F_POLY::Struct.1);
        assert_eq!(size_of::<F_SCENE_HEADER>(), F_SCENE_HEADER::Struct.1);
        assert_eq!(size_of::<F_TEXTURE_CONTAINER>(), F_TEXTURE_CONTAINER::Struct.1);
        assert_eq!(size_of::<F_ANCHOR_DATA>(), F_ANCHOR_DATA::Struct.1);
        assert_eq!(size_of::<F_SCENE_INFO>(), F_SCENE_INFO::Struct.1);
        assert_eq!(size_of::<ROOM_DIST_DATA>(), ROOM_DIST_DATA::Struct.1);
        assert_eq!(size_of::<THEA_HEADER>(), THEA_HEADER::Struct.1);
        assert_eq!(size_of::<THEA_KEYFRAME>(), THEA_KEYFRAME::Struct.1);
        assert_eq!(size_of::<THEA_KEYFRAME_2015>(), THEA_KEYFRAME_2015::Struct.1);
        assert_eq!(size_of::<THEO_GROUPANIM>(), THEO_GROUPANIM::Struct.1);
        assert_eq!(size_of::<THEA_HEADER>(), 292);
        assert_eq!(size_of::<THEA_KEYFRAME>(), 32);
        assert_eq!(size_of::<THEA_KEYFRAME_2015>(), 288);
        assert_eq!(size_of::<THEO_GROUPANIM>(), 52);
        assert_eq!(size_of::<THEA_KEYMOVE>(), 12);
        assert_eq!(size_of::<THEA_SAMPLE>(), 260);
        assert_eq!(size_of::<ArxQuat>(), 16);
    }

    /// A hand-built FTL: 3 vertices (origin 0 at the centre), one group holding
    /// vertices 0 and 1, one texture, one action point; no other sections.
    #[test]
    fn ftlReadsAHandBuiltObject() {
        let mut b: Vec<u8> = Vec::new();
        let i32s = |b: &mut Vec<u8>, v: &[i32]| for z in v { b.extend_from_slice(&z.to_le_bytes()); };
        let f32s = |b: &mut Vec<u8>, v: &[f32]| for z in v { b.extend_from_slice(&z.to_le_bytes()); };
        let name = |b: &mut Vec<u8>, s: &str, n: usize| { let mut z = s.as_bytes().to_vec(); z.resize(n, 0); b.extend_from_slice(&z); };
        b.extend_from_slice(&0x004c5446u32.to_le_bytes());
        f32s(&mut b, &[0.83257]);
        b.resize(b.len() + 512, 0);
        let offset3D = (b.len() + 24) as i32;
        i32s(&mut b, &[offset3D, -1, -1, -1, -1, -1]);
        i32s(&mut b, &[3, 0, 1, 1, 1, 0, 0]); name(&mut b, "obj.ftl", 256);
        for v in [[0f32, 0., 0.], [1., 2., 3.], [-1., 0., 0.]] {
            f32s(&mut b, &[v[0], v[1], v[2], 1.0]); i32s(&mut b, &[0, 0]); f32s(&mut b, &[0.0, 0.0]); // TLVERTEX
            f32s(&mut b, &v); f32s(&mut b, &[0.0, 0.0, 1.0]); // V, Norm
        }
        name(&mut b, "graph\\obj3d\\textures\\water_01.bmp", 256);
        name(&mut b, "head", 256); i32s(&mut b, &[0, 2, 0]); f32s(&mut b, &[1.0]);
        i32s(&mut b, &[0, 1]); // group indexes, read before the actions
        name(&mut b, "FIRE", 256); i32s(&mut b, &[1, 0, 0]);

        let ftl = Binary_Ftl::new(&mut Cursor::new(b)).unwrap();
        let obj = &ftl.Obj;
        assert_eq!(obj.File, "obj.ftl");
        assert_eq!(obj.Vertexs.as_ref().unwrap()[1].V, Vec3::new(1.0, 2.0, 3.0));
        assert_eq!(obj.Vertexs.as_ref().unwrap()[1].Vert.Color, 0xFF000000);
        let tex = &obj.Textures.as_ref().unwrap()[0];
        assert_eq!(tex.Poly, POLY::WATER | POLY::TRANS);
        assert_eq!(obj.Groups.as_ref().unwrap()[0].Indexes, Some(vec![0, 1]));
        let bones = obj.Bones.as_ref().unwrap();
        assert_eq!(bones.len(), 1);
        assert_eq!(bones[0].IdxVertices, Some(vec![0, 1, 2])); // the group's, then the lonely vertex
        assert_eq!(obj.FastAccess.HeadGroup, 0);
        assert_eq!(obj.FastAccess.Fire, 1);
        assert_eq!(obj.FastAccess.JawGroup, -1);
        // QuatInit is C#'s default(Quaternion), all zeros, so the local vertices are NaN as in C#.
        assert!(obj.VertexLocal.as_ref().unwrap()[1].x.is_nan());
    }

    fn teaHeader(version: u32, nbFrames: i32, nbGroups: i32, nbKeyFrames: i32) -> Vec<u8> {
        let mut b = Vec::new();
        let mut id = b"THEA".to_vec(); id.resize(20, 0); b.extend_from_slice(&id);
        b.extend_from_slice(&version.to_le_bytes());
        let mut name = b"walk".to_vec(); name.resize(256, 0); b.extend_from_slice(&name);
        for z in [nbFrames, nbGroups, nbKeyFrames] { b.extend_from_slice(&z.to_le_bytes()); }
        b
    }

    /// Hand-built 2014: frame 1 is filled by the swapped-weight blend of frames 0 and 2.
    #[test]
    fn teaDecodesAHandBuilt2014() {
        let mut b = teaHeader(2014, 3, 1, 3);
        let i32s = |b: &mut Vec<u8>, v: &[i32]| for z in v { b.extend_from_slice(&z.to_le_bytes()); };
        let f32s = |b: &mut Vec<u8>, v: &[f32]| for z in v { b.extend_from_slice(&z.to_le_bytes()); };
        let zeros = |b: &mut Vec<u8>, n: usize| b.resize(b.len() + n, 0);
        let name = |b: &mut Vec<u8>, s: &str, n: usize| { let mut z = s.as_bytes().to_vec(); z.resize(n, 0); b.extend_from_slice(&z); };
        // frame 0: move+orient, discarded time 999, group translate 0
        i32s(&mut b, &[0, 0, 0, 1, 1, 1, 0, 999]);
        f32s(&mut b, &[0.0, 0.0, 0.0]); zeros(&mut b, 8); f32s(&mut b, &[1.0, 0.0, 0.0, 0.0]);
        i32s(&mut b, &[1]); zeros(&mut b, 8); f32s(&mut b, &[0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0]);
        i32s(&mut b, &[-1, 0]);
        // frame 1: no move/orient, morph skipped, sample "step", group translate x = 1
        i32s(&mut b, &[1, 0, 0, 0, 0, 0, 1, 0]);
        zeros(&mut b, 16);
        i32s(&mut b, &[0]); zeros(&mut b, 8); f32s(&mut b, &[0.0, 0.0, 0.0, 1.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0]);
        i32s(&mut b, &[0]); name(&mut b, "step", 256); i32s(&mut b, &[4]);
        b.extend_from_slice(&[0x11, 0x22, 0x33, 0x44]);
        i32s(&mut b, &[7]);
        // frame 2: move 3,0,0 and quat wxyz 0,1,0,0
        i32s(&mut b, &[2, 0, 0, 1, 1, 1, 0, 0]);
        f32s(&mut b, &[3.0, 0.0, 0.0]); zeros(&mut b, 8); f32s(&mut b, &[0.0, 1.0, 0.0, 0.0]);
        i32s(&mut b, &[0]); zeros(&mut b, 8); f32s(&mut b, &[0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0]);
        i32s(&mut b, &[-1, 0]);

        let tea = Binary_Tea::new(&mut Cursor::new(b)).unwrap();
        assert_eq!(tea.Version, 2014);
        assert_eq!(tea.Name, "walk");
        let div = 0.0416666666666666666667f32;
        let fr = &tea.Anim.Frames;
        assert_eq!(fr[0].Time.to_bits(), 0f32.to_bits());
        assert_eq!(fr[1].Time.to_bits(), (1000f32 * div).to_bits());
        assert_eq!(fr[2].Time.to_bits(), (2000f32 * div).to_bits());
        let anim = 3f32 * 1000f32 * div;
        assert_eq!(tea.Anim.AnimTime.to_bits(), anim.to_bits());
        assert_ne!(tea.Anim.AnimTime, 1.0);
        let t1 = 1000f32 * div;
        let t2 = 2000f32 * div;
        let tot = 1f32 / (t1 + t2);
        let r1 = t1 * tot;
        let r2 = t2 * tot;
        assert_eq!(fr[1].Translate.x.to_bits(), (3f32 * r1).to_bits());
        let q = fr[1].Quat;
        assert_eq!(q.x.to_bits(), r1.to_bits());
        assert_eq!(q.w.to_bits(), r2.to_bits());
        assert_eq!(q.y, 0.0);
        assert_eq!(q.z, 0.0);
        let len2 = q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
        assert_ne!(len2.to_bits(), 1.0f32.to_bits());
        assert!(fr.iter().all(|f| f.FTranslate == 1 && f.FRotate == 1));
        assert_eq!(fr[1].Sample, -1);
        assert_eq!(fr[1].SampleName.as_deref(), Some("step"));
        assert!(fr[0].SampleName.is_none());
        assert!(fr[2].SampleName.is_none());
        assert_eq!(tea.Anim.VoidGroups[0], 0);
        let nodes = tea.getInfoNodes(None, None, &Tag::None);
        let teaNode = nodes.iter().find(|n| n.name.as_deref() == Some("TEA")).unwrap();
        assert!(teaNode.items[0].name.as_deref().unwrap().contains("GameX.Arkane.Formats.Danae.E_ANIM"));

        let err = Binary_Tea::new(&mut Cursor::new(teaHeader(2013, 0, 0, 0))).unwrap_err();
        assert_eq!(err.kind(), ErrorKind::InvalidData);
        assert!(err.to_string().contains("Invalid TEA Version"));

        let empty = Binary_Tea::new(&mut Cursor::new(teaHeader(2015, 0, 0, 0))).unwrap();
        assert_eq!(empty.Version, 2015);
        assert_eq!(empty.Anim.AnimTime, 1.0);
        assert!(empty.Anim.Frames.is_empty());
        assert!(empty.Anim.Groups.is_empty());
        assert!(empty.Anim.VoidGroups.is_empty());
    }
}
