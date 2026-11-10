// PORT-SOURCE: Families/GameX.IW/Formats/FastFile+AssetType.cs
// PORT-SHA: 2ca6331db37cfe96
// PORT-STATUS: done
// PORT-GENERATED: gen_enums.py — do not hand-edit; regenerate instead.
//
// 4 enum(s), 170 members, generated from the C#
// rather than transcribed. At this size a one-digit typo would mis-identify a
// game asset in a way no test would obviously catch.
//
// C# allows duplicate discriminants within an enum; Rust does not. Where that
// happens the first member becomes the variant and the rest become associated
// consts pointing at it, so every C# name still resolves.
//
// Per enum:
//   IW3XAssetType                      i32   enum         34 members
//   IW4XAssetType                      i32   enum         44 members
//   IW5XAssetType                      i32   enum         48 members
//   Oth_AssetType                      i32   enum         44 members

#![allow(non_camel_case_types, non_upper_case_globals)]

/// C# `enum IW3XAssetType : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum IW3XAssetType {
    xmodelpieces = 0x0,
    physpreset = 0x1,
    xanim = 0x2,
    xmodel = 0x3,
    material = 0x4,
    techset = 0x5,
    image = 0x6,
    sound = 0x7,
    sndcurve = 0x8,
    loaded_sound = 0x9,
    col_map_sp = 0xa,
    col_map_mp = 0xb,
    com_map = 0xc,
    game_map_sp = 0xd,
    game_map_mp = 0xe,
    map_ents = 0xf,
    gfx_map = 0x10,
    lightdef = 0x11,
    ui_map = 0x12,
    font = 0x13,
    menufile = 0x14,
    menu = 0x15,
    localize = 0x16,
    weapon = 0x17,
    snddriverglobals = 0x18,
    fx = 0x19,
    impactfx = 0x1a,
    aitype = 0x1b,
    mptype = 0x1c,
    character = 0x1d,
    xmodelalias = 0x1e,
    rawfile = 0x1f,
    stringtable = 0x20,
    max = 0x21,
}

impl IW3XAssetType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn fromRaw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::xmodelpieces,
            0x1 => Self::physpreset,
            0x2 => Self::xanim,
            0x3 => Self::xmodel,
            0x4 => Self::material,
            0x5 => Self::techset,
            0x6 => Self::image,
            0x7 => Self::sound,
            0x8 => Self::sndcurve,
            0x9 => Self::loaded_sound,
            0xa => Self::col_map_sp,
            0xb => Self::col_map_mp,
            0xc => Self::com_map,
            0xd => Self::game_map_sp,
            0xe => Self::game_map_mp,
            0xf => Self::map_ents,
            0x10 => Self::gfx_map,
            0x11 => Self::lightdef,
            0x12 => Self::ui_map,
            0x13 => Self::font,
            0x14 => Self::menufile,
            0x15 => Self::menu,
            0x16 => Self::localize,
            0x17 => Self::weapon,
            0x18 => Self::snddriverglobals,
            0x19 => Self::fx,
            0x1a => Self::impactfx,
            0x1b => Self::aitype,
            0x1c => Self::mptype,
            0x1d => Self::character,
            0x1e => Self::xmodelalias,
            0x1f => Self::rawfile,
            0x20 => Self::stringtable,
            0x21 => Self::max,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn toRaw(self) -> i32 {
        self as i32
    }
}

/// C# `enum IW4XAssetType : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum IW4XAssetType {
    physpreset = 0x0,
    phys_collmap = 0x1,
    xanim = 0x2,
    xmodelsurfs = 0x3,
    xmodel = 0x4,
    material = 0x5,
    pixelshader = 0x6,
    vertexshader = 0x7,
    vertexdecl = 0x8,
    techset = 0x9,
    image = 0xa,
    sound = 0xb,
    sndcurve = 0xc,
    loaded_sound = 0xd,
    col_map_sp = 0xe,
    col_map_mp = 0xf,
    com_map = 0x10,
    game_map_sp = 0x11,
    game_map_mp = 0x12,
    map_ents = 0x13,
    fx_map = 0x14,
    gfx_map = 0x15,
    lightdef = 0x16,
    ui_map = 0x17,
    font = 0x18,
    menufile = 0x19,
    menu = 0x1a,
    localize = 0x1b,
    weapon = 0x1c,
    snddriverglobals = 0x1d,
    fx = 0x1e,
    impactfx = 0x1f,
    aitype = 0x20,
    mptype = 0x21,
    character = 0x22,
    xmodelalias = 0x23,
    rawfile = 0x24,
    stringtable = 0x25,
    leaderboarddef = 0x26,
    structureddatadef = 0x27,
    tracer = 0x28,
    vehicle = 0x29,
    addon_map_ents = 0x2a,
    max = 0x2b,
}

impl IW4XAssetType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn fromRaw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::physpreset,
            0x1 => Self::phys_collmap,
            0x2 => Self::xanim,
            0x3 => Self::xmodelsurfs,
            0x4 => Self::xmodel,
            0x5 => Self::material,
            0x6 => Self::pixelshader,
            0x7 => Self::vertexshader,
            0x8 => Self::vertexdecl,
            0x9 => Self::techset,
            0xa => Self::image,
            0xb => Self::sound,
            0xc => Self::sndcurve,
            0xd => Self::loaded_sound,
            0xe => Self::col_map_sp,
            0xf => Self::col_map_mp,
            0x10 => Self::com_map,
            0x11 => Self::game_map_sp,
            0x12 => Self::game_map_mp,
            0x13 => Self::map_ents,
            0x14 => Self::fx_map,
            0x15 => Self::gfx_map,
            0x16 => Self::lightdef,
            0x17 => Self::ui_map,
            0x18 => Self::font,
            0x19 => Self::menufile,
            0x1a => Self::menu,
            0x1b => Self::localize,
            0x1c => Self::weapon,
            0x1d => Self::snddriverglobals,
            0x1e => Self::fx,
            0x1f => Self::impactfx,
            0x20 => Self::aitype,
            0x21 => Self::mptype,
            0x22 => Self::character,
            0x23 => Self::xmodelalias,
            0x24 => Self::rawfile,
            0x25 => Self::stringtable,
            0x26 => Self::leaderboarddef,
            0x27 => Self::structureddatadef,
            0x28 => Self::tracer,
            0x29 => Self::vehicle,
            0x2a => Self::addon_map_ents,
            0x2b => Self::max,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn toRaw(self) -> i32 {
        self as i32
    }
}

/// C# `enum IW5XAssetType : i32`.
///
/// 1 C# member(s) share a discriminant with an earlier one, which
/// Rust enums disallow; those are associated consts below.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum IW5XAssetType {
    physpreset = 0x0,
    phys_collmap = 0x1,
    xanim = 0x2,
    xmodelsurfs = 0x3,
    xmodel = 0x4,
    material = 0x5,
    pixelshader = 0x6,
    vertexshader = 0x7,
    vertexdecl = 0x8,
    techset = 0x9,
    image = 0xa,
    sound = 0xb,
    sndcurve = 0xc,
    loaded_sound = 0xd,
    col_map_mp = 0xe,
    com_map = 0xf,
    glass_map = 0x10,
    ai_paths = 0x11,
    vehicle_tracks = 0x12,
    map_ents = 0x13,
    fx_map = 0x14,
    gfx_map = 0x15,
    lightdef = 0x16,
    ui_map = 0x17,
    font = 0x18,
    menufile = 0x19,
    menu = 0x1a,
    localize = 0x1b,
    attachment = 0x1c,
    weapon = 0x1d,
    snddriverglobals = 0x1e,
    fx = 0x1f,
    impactfx = 0x20,
    surfacefx = 0x21,
    aitype = 0x22,
    mptype = 0x23,
    character = 0x24,
    xmodelalias = 0x25,
    rawfile = 0x26,
    scriptfile = 0x27,
    stringtable = 0x28,
    leaderboarddef = 0x29,
    structureddatadef = 0x2a,
    tracer = 0x2b,
    vehicle = 0x2c,
    addon_map_ents = 0x2d,
    max = 0x2e,
}

impl IW5XAssetType {
    /// Alias of [`Self::col_map_mp`].
    pub const col_map_sp: Self = Self::col_map_mp;
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn fromRaw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::physpreset,
            0x1 => Self::phys_collmap,
            0x2 => Self::xanim,
            0x3 => Self::xmodelsurfs,
            0x4 => Self::xmodel,
            0x5 => Self::material,
            0x6 => Self::pixelshader,
            0x7 => Self::vertexshader,
            0x8 => Self::vertexdecl,
            0x9 => Self::techset,
            0xa => Self::image,
            0xb => Self::sound,
            0xc => Self::sndcurve,
            0xd => Self::loaded_sound,
            0xe => Self::col_map_mp,
            0xf => Self::com_map,
            0x10 => Self::glass_map,
            0x11 => Self::ai_paths,
            0x12 => Self::vehicle_tracks,
            0x13 => Self::map_ents,
            0x14 => Self::fx_map,
            0x15 => Self::gfx_map,
            0x16 => Self::lightdef,
            0x17 => Self::ui_map,
            0x18 => Self::font,
            0x19 => Self::menufile,
            0x1a => Self::menu,
            0x1b => Self::localize,
            0x1c => Self::attachment,
            0x1d => Self::weapon,
            0x1e => Self::snddriverglobals,
            0x1f => Self::fx,
            0x20 => Self::impactfx,
            0x21 => Self::surfacefx,
            0x22 => Self::aitype,
            0x23 => Self::mptype,
            0x24 => Self::character,
            0x25 => Self::xmodelalias,
            0x26 => Self::rawfile,
            0x27 => Self::scriptfile,
            0x28 => Self::stringtable,
            0x29 => Self::leaderboarddef,
            0x2a => Self::structureddatadef,
            0x2b => Self::tracer,
            0x2c => Self::vehicle,
            0x2d => Self::addon_map_ents,
            0x2e => Self::max,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn toRaw(self) -> i32 {
        self as i32
    }
}

/// C# `enum Oth_AssetType : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Oth_AssetType {
    PhysPreset = 0x0,
    PhysColMap = 0x1,
    XAnim = 0x2,
    XModelSurfs = 0x3,
    XModel = 0x4,
    Material = 0x5,
    PixelShader = 0x6,
    VertexShader = 0x7,
    VertexDecl = 0x8,
    TechSet = 0x9,
    Image = 0xa,
    Sound = 0xb,
    SndCurve = 0xc,
    LoadedSound = 0xd,
    ColMapSP = 0xe,
    ColMapMP = 0xf,
    ComMap = 0x10,
    GameMapSP = 0x11,
    GameMapMP = 0x12,
    MapEnts = 0x13,
    FXMap = 0x14,
    GfxMap = 0x15,
    LightDef = 0x16,
    UIMap = 0x17,
    Font = 0x18,
    MenuFile = 0x19,
    Menu = 0x1a,
    Localize = 0x1b,
    Weapon = 0x1c,
    SndDriverGlobals = 0x1d,
    FX = 0x1e,
    ImpactFX = 0x1f,
    AIType = 0x20,
    MPType = 0x21,
    Character = 0x22,
    XModelAlias = 0x23,
    RawFile = 0x24,
    StringTable = 0x25,
    LeaderBoardDef = 0x26,
    StructuredDataDef = 0x27,
    Tracer = 0x28,
    Vehical = 0x29,
    AddONMapEnts = 0x2a,
    Max = 0x2b,
}

impl Oth_AssetType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn fromRaw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::PhysPreset,
            0x1 => Self::PhysColMap,
            0x2 => Self::XAnim,
            0x3 => Self::XModelSurfs,
            0x4 => Self::XModel,
            0x5 => Self::Material,
            0x6 => Self::PixelShader,
            0x7 => Self::VertexShader,
            0x8 => Self::VertexDecl,
            0x9 => Self::TechSet,
            0xa => Self::Image,
            0xb => Self::Sound,
            0xc => Self::SndCurve,
            0xd => Self::LoadedSound,
            0xe => Self::ColMapSP,
            0xf => Self::ColMapMP,
            0x10 => Self::ComMap,
            0x11 => Self::GameMapSP,
            0x12 => Self::GameMapMP,
            0x13 => Self::MapEnts,
            0x14 => Self::FXMap,
            0x15 => Self::GfxMap,
            0x16 => Self::LightDef,
            0x17 => Self::UIMap,
            0x18 => Self::Font,
            0x19 => Self::MenuFile,
            0x1a => Self::Menu,
            0x1b => Self::Localize,
            0x1c => Self::Weapon,
            0x1d => Self::SndDriverGlobals,
            0x1e => Self::FX,
            0x1f => Self::ImpactFX,
            0x20 => Self::AIType,
            0x21 => Self::MPType,
            0x22 => Self::Character,
            0x23 => Self::XModelAlias,
            0x24 => Self::RawFile,
            0x25 => Self::StringTable,
            0x26 => Self::LeaderBoardDef,
            0x27 => Self::StructuredDataDef,
            0x28 => Self::Tracer,
            0x29 => Self::Vehical,
            0x2a => Self::AddONMapEnts,
            0x2b => Self::Max,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn toRaw(self) -> i32 {
        self as i32
    }
}
