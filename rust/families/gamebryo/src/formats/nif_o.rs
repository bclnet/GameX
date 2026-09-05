// PORT-SOURCE: Families/GameX.Gamebryo/Formats/Nif.o.cs
// PORT-SHA: b862973ab9639351
// PORT-STATUS: done
// PORT-GENERATED: gen_enums.py — do not hand-edit; regenerate instead.
//
// 55 enum(s), 739 members, generated from the C#
// rather than transcribed. At this size a one-digit typo would mis-identify a
// game asset in a way no test would obviously catch.
//
// C# allows duplicate discriminants within an enum; Rust does not. Where that
// happens the first member becomes the variant and the rest become associated
// consts pointing at it, so every C# name still resolves.
//
// Per enum:
//   Flags                              u16   enum          1 members
//   AccumFlags                         u32   bitflags     10 members
//   ApplyMode                          u32   enum          5 members
//   KeyType                            u32   enum          5 members
//   OblivionHavokMaterial              u32   enum         32 members
//   Fallout3HavokMaterial              u32   enum        128 members
//   SkyrimHavokMaterial                u32   enum         61 members
//   OblivionLayer                      u8    enum         58 members
//   Fallout3Layer                      u8    enum         44 members
//   SkyrimLayer                        u8    enum         48 members
//   MoppDataBuildType                  u8    enum          3 members
//   PixelFormat                        u32   enum         17 members
//   PixelTiling                        u32   enum          4 members
//   PixelComponent                     u32   enum         20 members
//   PixelRepresentation                u32   enum          7 members
//   PixelLayout                        u32   enum         17 members
//   MipMapFormat                       u32   enum          3 members
//   AlphaFormat                        u32   enum          4 members
//   TexClampMode                       u32   enum          4 members
//   TexFilterMode                      u32   enum          7 members
//   VertMode                           u32   enum          3 members
//   LightMode                          u32   enum          2 members
//   CycleType                          u32   enum          3 members
//   FieldType                          u32   enum          2 members
//   BillboardMode                      u16   enum          7 members
//   ZCompareMode                       u32   enum          8 members
//   hkMotionType                       u8    enum         10 members
//   hkDeactivatorType                  u8    enum          3 members
//   hkSolverDeactivation               u8    enum          6 members
//   hkQualityType                      u8    enum         10 members
//   DecayType                          u32   enum          3 members
//   SymmetryType                       u32   enum          3 members
//   TextureType                        u32   enum          4 members
//   CoordGenType                       u32   enum          5 members
//   EndianType                         u8    enum          2 members
//   MaterialColor                      u16   enum          4 members
//   ConsistencyType                    u16   enum          3 members
//   BoundVolumeType                    u32   enum          6 members
//   hkResponseType                     u8    enum          4 members
//   BSLightingShaderPropertyShaderType u32   enum         21 members
//   TransformMethod                    u32   enum          3 members
//   VertexFlags                        u16   bitflags     11 members
//   FurnitureEntryPoints               u16   bitflags      5 members
//   AnimationType                      u16   enum          3 members
//   ImageType                          u32   enum          2 members
//   BroadPhaseType                     u8    enum          4 members
//   PathFlags                          u16   bitflags      7 members
//   InterpBlendFlags                   u8    enum          1 members
//   bhkCOFlags                         u16   bitflags      9 members
//   VectorFlags                        u16   bitflags     16 members
//   BSVectorFlags                      u16   bitflags     16 members
//   BSShaderType                       u32   enum          8 members
//   BSShaderFlags                      u32   bitflags     32 members
//   BSShaderFlags2                     u32   bitflags     32 members
//   AnimNoteType                       u32   enum          3 members

#![allow(non_camel_case_types, non_upper_case_globals)]

/// C# `enum Flags : u16`.
#[repr(u16)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Flags {
    Hidden = 0x1,
}

impl Flags {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u16) -> Option<Self> {
        Some(match v {
            0x1 => Self::Hidden,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u16 {
        self as u16
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum AccumFlags : u32`.
    /// Describes the options for the accum root on NiControllerSequence.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct AccumFlags: u32 {
        const ACCUM_X_TRANS = 0x0;
        const ACCUM_Y_TRANS = 0x2;
        const ACCUM_Z_TRANS = 0x4;
        const ACCUM_X_ROT = 0x8;
        const ACCUM_Y_ROT = 0x10;
        const ACCUM_Z_ROT = 0x20;
        const ACCUM_X_FRONT = 0x40;
        const ACCUM_Y_FRONT = 0x80;
        const ACCUM_Z_FRONT = 0x100;
        const ACCUM_NEG_FRONT = 0x200; // -X is front facing.
    }
}

/// Describes how the vertex colors are blended with the filtered texture color.
/// C# `enum ApplyMode : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum ApplyMode {
    APPLY_REPLACE = 0x0,
    APPLY_DECAL = 0x1,
    APPLY_MODULATE = 0x2,
    APPLY_HILIGHT = 0x3,
    APPLY_HILIGHT2 = 0x4, // Parallax Flag in some Oblivion meshes.
}

impl ApplyMode {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::APPLY_REPLACE,
            0x1 => Self::APPLY_DECAL,
            0x2 => Self::APPLY_MODULATE,
            0x3 => Self::APPLY_HILIGHT,
            0x4 => Self::APPLY_HILIGHT2,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// The type of animation interpolation (blending) that will be used on the associated key frames.
/// C# `enum KeyType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum KeyType {
    LINEAR_KEY = 0x1,
    QUADRATIC_KEY = 0x2,
    TBC_KEY = 0x3,
    XYZ_ROTATION_KEY = 0x4,
    CONST_KEY = 0x5, // Step function. Used for visibility keys in NiBoolData.
}

impl KeyType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x1 => Self::LINEAR_KEY,
            0x2 => Self::QUADRATIC_KEY,
            0x3 => Self::TBC_KEY,
            0x4 => Self::XYZ_ROTATION_KEY,
            0x5 => Self::CONST_KEY,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Bethesda Havok. Material descriptor for a Havok shape in Oblivion.
/// C# `enum OblivionHavokMaterial : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum OblivionHavokMaterial {
    OB_HAV_MAT_STONE = 0x0,
    OB_HAV_MAT_CLOTH = 0x1,
    OB_HAV_MAT_DIRT = 0x2,
    OB_HAV_MAT_GLASS = 0x3,
    OB_HAV_MAT_GRASS = 0x4,
    OB_HAV_MAT_METAL = 0x5,
    OB_HAV_MAT_ORGANIC = 0x6,
    OB_HAV_MAT_SKIN = 0x7,
    OB_HAV_MAT_WATER = 0x8,
    OB_HAV_MAT_WOOD = 0x9,
    OB_HAV_MAT_HEAVY_STONE = 0xa,
    OB_HAV_MAT_HEAVY_METAL = 0xb,
    OB_HAV_MAT_HEAVY_WOOD = 0xc,
    OB_HAV_MAT_CHAIN = 0xd,
    OB_HAV_MAT_SNOW = 0xe,
    OB_HAV_MAT_STONE_STAIRS = 0xf,
    OB_HAV_MAT_CLOTH_STAIRS = 0x10,
    OB_HAV_MAT_DIRT_STAIRS = 0x11,
    OB_HAV_MAT_GLASS_STAIRS = 0x12,
    OB_HAV_MAT_GRASS_STAIRS = 0x13,
    OB_HAV_MAT_METAL_STAIRS = 0x14,
    OB_HAV_MAT_ORGANIC_STAIRS = 0x15,
    OB_HAV_MAT_SKIN_STAIRS = 0x16,
    OB_HAV_MAT_WATER_STAIRS = 0x17,
    OB_HAV_MAT_WOOD_STAIRS = 0x18,
    OB_HAV_MAT_HEAVY_STONE_STAIRS = 0x19,
    OB_HAV_MAT_HEAVY_METAL_STAIRS = 0x1a,
    OB_HAV_MAT_HEAVY_WOOD_STAIRS = 0x1b,
    OB_HAV_MAT_CHAIN_STAIRS = 0x1c,
    OB_HAV_MAT_SNOW_STAIRS = 0x1d,
    OB_HAV_MAT_ELEVATOR = 0x1e,
    OB_HAV_MAT_RUBBER = 0x1f, // Rubber
}

impl OblivionHavokMaterial {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::OB_HAV_MAT_STONE,
            0x1 => Self::OB_HAV_MAT_CLOTH,
            0x2 => Self::OB_HAV_MAT_DIRT,
            0x3 => Self::OB_HAV_MAT_GLASS,
            0x4 => Self::OB_HAV_MAT_GRASS,
            0x5 => Self::OB_HAV_MAT_METAL,
            0x6 => Self::OB_HAV_MAT_ORGANIC,
            0x7 => Self::OB_HAV_MAT_SKIN,
            0x8 => Self::OB_HAV_MAT_WATER,
            0x9 => Self::OB_HAV_MAT_WOOD,
            0xa => Self::OB_HAV_MAT_HEAVY_STONE,
            0xb => Self::OB_HAV_MAT_HEAVY_METAL,
            0xc => Self::OB_HAV_MAT_HEAVY_WOOD,
            0xd => Self::OB_HAV_MAT_CHAIN,
            0xe => Self::OB_HAV_MAT_SNOW,
            0xf => Self::OB_HAV_MAT_STONE_STAIRS,
            0x10 => Self::OB_HAV_MAT_CLOTH_STAIRS,
            0x11 => Self::OB_HAV_MAT_DIRT_STAIRS,
            0x12 => Self::OB_HAV_MAT_GLASS_STAIRS,
            0x13 => Self::OB_HAV_MAT_GRASS_STAIRS,
            0x14 => Self::OB_HAV_MAT_METAL_STAIRS,
            0x15 => Self::OB_HAV_MAT_ORGANIC_STAIRS,
            0x16 => Self::OB_HAV_MAT_SKIN_STAIRS,
            0x17 => Self::OB_HAV_MAT_WATER_STAIRS,
            0x18 => Self::OB_HAV_MAT_WOOD_STAIRS,
            0x19 => Self::OB_HAV_MAT_HEAVY_STONE_STAIRS,
            0x1a => Self::OB_HAV_MAT_HEAVY_METAL_STAIRS,
            0x1b => Self::OB_HAV_MAT_HEAVY_WOOD_STAIRS,
            0x1c => Self::OB_HAV_MAT_CHAIN_STAIRS,
            0x1d => Self::OB_HAV_MAT_SNOW_STAIRS,
            0x1e => Self::OB_HAV_MAT_ELEVATOR,
            0x1f => Self::OB_HAV_MAT_RUBBER,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Bethesda Havok. Material descriptor for a Havok shape in Fallout 3 and Fallout NV.
/// C# `enum Fallout3HavokMaterial : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Fallout3HavokMaterial {
    FO_HAV_MAT_STONE = 0x0,
    FO_HAV_MAT_CLOTH = 0x1,
    FO_HAV_MAT_DIRT = 0x2,
    FO_HAV_MAT_GLASS = 0x3,
    FO_HAV_MAT_GRASS = 0x4,
    FO_HAV_MAT_METAL = 0x5,
    FO_HAV_MAT_ORGANIC = 0x6,
    FO_HAV_MAT_SKIN = 0x7,
    FO_HAV_MAT_WATER = 0x8,
    FO_HAV_MAT_WOOD = 0x9,
    FO_HAV_MAT_HEAVY_STONE = 0xa,
    FO_HAV_MAT_HEAVY_METAL = 0xb,
    FO_HAV_MAT_HEAVY_WOOD = 0xc,
    FO_HAV_MAT_CHAIN = 0xd,
    FO_HAV_MAT_BOTTLECAP = 0xe,
    FO_HAV_MAT_ELEVATOR = 0xf,
    FO_HAV_MAT_HOLLOW_METAL = 0x10,
    FO_HAV_MAT_SHEET_METAL = 0x11,
    FO_HAV_MAT_SAND = 0x12,
    FO_HAV_MAT_BROKEN_CONCRETE = 0x13,
    FO_HAV_MAT_VEHICLE_BODY = 0x14,
    FO_HAV_MAT_VEHICLE_PART_SOLID = 0x15,
    FO_HAV_MAT_VEHICLE_PART_HOLLOW = 0x16,
    FO_HAV_MAT_BARREL = 0x17,
    FO_HAV_MAT_BOTTLE = 0x18,
    FO_HAV_MAT_SODA_CAN = 0x19,
    FO_HAV_MAT_PISTOL = 0x1a,
    FO_HAV_MAT_RIFLE = 0x1b,
    FO_HAV_MAT_SHOPPING_CART = 0x1c,
    FO_HAV_MAT_LUNCHBOX = 0x1d,
    FO_HAV_MAT_BABY_RATTLE = 0x1e,
    FO_HAV_MAT_RUBBER_BALL = 0x1f,
    FO_HAV_MAT_STONE_PLATFORM = 0x20,
    FO_HAV_MAT_CLOTH_PLATFORM = 0x21,
    FO_HAV_MAT_DIRT_PLATFORM = 0x22,
    FO_HAV_MAT_GLASS_PLATFORM = 0x23,
    FO_HAV_MAT_GRASS_PLATFORM = 0x24,
    FO_HAV_MAT_METAL_PLATFORM = 0x25,
    FO_HAV_MAT_ORGANIC_PLATFORM = 0x26,
    FO_HAV_MAT_SKIN_PLATFORM = 0x27,
    FO_HAV_MAT_WATER_PLATFORM = 0x28,
    FO_HAV_MAT_WOOD_PLATFORM = 0x29,
    FO_HAV_MAT_HEAVY_STONE_PLATFORM = 0x2a,
    FO_HAV_MAT_HEAVY_METAL_PLATFORM = 0x2b,
    FO_HAV_MAT_HEAVY_WOOD_PLATFORM = 0x2c,
    FO_HAV_MAT_CHAIN_PLATFORM = 0x2d,
    FO_HAV_MAT_BOTTLECAP_PLATFORM = 0x2e,
    FO_HAV_MAT_ELEVATOR_PLATFORM = 0x2f,
    FO_HAV_MAT_HOLLOW_METAL_PLATFORM = 0x30,
    FO_HAV_MAT_SHEET_METAL_PLATFORM = 0x31,
    FO_HAV_MAT_SAND_PLATFORM = 0x32,
    FO_HAV_MAT_BROKEN_CONCRETE_PLATFORM = 0x33,
    FO_HAV_MAT_VEHICLE_BODY_PLATFORM = 0x34,
    FO_HAV_MAT_VEHICLE_PART_SOLID_PLATFORM = 0x35,
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_PLATFORM = 0x36,
    FO_HAV_MAT_BARREL_PLATFORM = 0x37,
    FO_HAV_MAT_BOTTLE_PLATFORM = 0x38,
    FO_HAV_MAT_SODA_CAN_PLATFORM = 0x39,
    FO_HAV_MAT_PISTOL_PLATFORM = 0x3a,
    FO_HAV_MAT_RIFLE_PLATFORM = 0x3b,
    FO_HAV_MAT_SHOPPING_CART_PLATFORM = 0x3c,
    FO_HAV_MAT_LUNCHBOX_PLATFORM = 0x3d,
    FO_HAV_MAT_BABY_RATTLE_PLATFORM = 0x3e,
    FO_HAV_MAT_RUBBER_BALL_PLATFORM = 0x3f,
    FO_HAV_MAT_STONE_STAIRS = 0x40,
    FO_HAV_MAT_CLOTH_STAIRS = 0x41,
    FO_HAV_MAT_DIRT_STAIRS = 0x42,
    FO_HAV_MAT_GLASS_STAIRS = 0x43,
    FO_HAV_MAT_GRASS_STAIRS = 0x44,
    FO_HAV_MAT_METAL_STAIRS = 0x45,
    FO_HAV_MAT_ORGANIC_STAIRS = 0x46,
    FO_HAV_MAT_SKIN_STAIRS = 0x47,
    FO_HAV_MAT_WATER_STAIRS = 0x48,
    FO_HAV_MAT_WOOD_STAIRS = 0x49,
    FO_HAV_MAT_HEAVY_STONE_STAIRS = 0x4a,
    FO_HAV_MAT_HEAVY_METAL_STAIRS = 0x4b,
    FO_HAV_MAT_HEAVY_WOOD_STAIRS = 0x4c,
    FO_HAV_MAT_CHAIN_STAIRS = 0x4d,
    FO_HAV_MAT_BOTTLECAP_STAIRS = 0x4e,
    FO_HAV_MAT_ELEVATOR_STAIRS = 0x4f,
    FO_HAV_MAT_HOLLOW_METAL_STAIRS = 0x50,
    FO_HAV_MAT_SHEET_METAL_STAIRS = 0x51,
    FO_HAV_MAT_SAND_STAIRS = 0x52,
    FO_HAV_MAT_BROKEN_CONCRETE_STAIRS = 0x53,
    FO_HAV_MAT_VEHICLE_BODY_STAIRS = 0x54,
    FO_HAV_MAT_VEHICLE_PART_SOLID_STAIRS = 0x55,
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_STAIRS = 0x56,
    FO_HAV_MAT_BARREL_STAIRS = 0x57,
    FO_HAV_MAT_BOTTLE_STAIRS = 0x58,
    FO_HAV_MAT_SODA_CAN_STAIRS = 0x59,
    FO_HAV_MAT_PISTOL_STAIRS = 0x5a,
    FO_HAV_MAT_RIFLE_STAIRS = 0x5b,
    FO_HAV_MAT_SHOPPING_CART_STAIRS = 0x5c,
    FO_HAV_MAT_LUNCHBOX_STAIRS = 0x5d,
    FO_HAV_MAT_BABY_RATTLE_STAIRS = 0x5e,
    FO_HAV_MAT_RUBBER_BALL_STAIRS = 0x5f,
    FO_HAV_MAT_STONE_STAIRS_PLATFORM = 0x60,
    FO_HAV_MAT_CLOTH_STAIRS_PLATFORM = 0x61,
    FO_HAV_MAT_DIRT_STAIRS_PLATFORM = 0x62,
    FO_HAV_MAT_GLASS_STAIRS_PLATFORM = 0x63,
    FO_HAV_MAT_GRASS_STAIRS_PLATFORM = 0x64,
    FO_HAV_MAT_METAL_STAIRS_PLATFORM = 0x65,
    FO_HAV_MAT_ORGANIC_STAIRS_PLATFORM = 0x66,
    FO_HAV_MAT_SKIN_STAIRS_PLATFORM = 0x67,
    FO_HAV_MAT_WATER_STAIRS_PLATFORM = 0x68,
    FO_HAV_MAT_WOOD_STAIRS_PLATFORM = 0x69,
    FO_HAV_MAT_HEAVY_STONE_STAIRS_PLATFORM = 0x6a,
    FO_HAV_MAT_HEAVY_METAL_STAIRS_PLATFORM = 0x6b,
    FO_HAV_MAT_HEAVY_WOOD_STAIRS_PLATFORM = 0x6c,
    FO_HAV_MAT_CHAIN_STAIRS_PLATFORM = 0x6d,
    FO_HAV_MAT_BOTTLECAP_STAIRS_PLATFORM = 0x6e,
    FO_HAV_MAT_ELEVATOR_STAIRS_PLATFORM = 0x6f,
    FO_HAV_MAT_HOLLOW_METAL_STAIRS_PLATFORM = 0x70,
    FO_HAV_MAT_SHEET_METAL_STAIRS_PLATFORM = 0x71,
    FO_HAV_MAT_SAND_STAIRS_PLATFORM = 0x72,
    FO_HAV_MAT_BROKEN_CONCRETE_STAIRS_PLATFORM = 0x73,
    FO_HAV_MAT_VEHICLE_BODY_STAIRS_PLATFORM = 0x74,
    FO_HAV_MAT_VEHICLE_PART_SOLID_STAIRS_PLATFORM = 0x75,
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_STAIRS_PLATFORM = 0x76,
    FO_HAV_MAT_BARREL_STAIRS_PLATFORM = 0x77,
    FO_HAV_MAT_BOTTLE_STAIRS_PLATFORM = 0x78,
    FO_HAV_MAT_SODA_CAN_STAIRS_PLATFORM = 0x79,
    FO_HAV_MAT_PISTOL_STAIRS_PLATFORM = 0x7a,
    FO_HAV_MAT_RIFLE_STAIRS_PLATFORM = 0x7b,
    FO_HAV_MAT_SHOPPING_CART_STAIRS_PLATFORM = 0x7c,
    FO_HAV_MAT_LUNCHBOX_STAIRS_PLATFORM = 0x7d,
    FO_HAV_MAT_BABY_RATTLE_STAIRS_PLATFORM = 0x7e,
    FO_HAV_MAT_RUBBER_BALL_STAIRS_PLATFORM = 0x7f, // Rubber Ball
}

impl Fallout3HavokMaterial {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::FO_HAV_MAT_STONE,
            0x1 => Self::FO_HAV_MAT_CLOTH,
            0x2 => Self::FO_HAV_MAT_DIRT,
            0x3 => Self::FO_HAV_MAT_GLASS,
            0x4 => Self::FO_HAV_MAT_GRASS,
            0x5 => Self::FO_HAV_MAT_METAL,
            0x6 => Self::FO_HAV_MAT_ORGANIC,
            0x7 => Self::FO_HAV_MAT_SKIN,
            0x8 => Self::FO_HAV_MAT_WATER,
            0x9 => Self::FO_HAV_MAT_WOOD,
            0xa => Self::FO_HAV_MAT_HEAVY_STONE,
            0xb => Self::FO_HAV_MAT_HEAVY_METAL,
            0xc => Self::FO_HAV_MAT_HEAVY_WOOD,
            0xd => Self::FO_HAV_MAT_CHAIN,
            0xe => Self::FO_HAV_MAT_BOTTLECAP,
            0xf => Self::FO_HAV_MAT_ELEVATOR,
            0x10 => Self::FO_HAV_MAT_HOLLOW_METAL,
            0x11 => Self::FO_HAV_MAT_SHEET_METAL,
            0x12 => Self::FO_HAV_MAT_SAND,
            0x13 => Self::FO_HAV_MAT_BROKEN_CONCRETE,
            0x14 => Self::FO_HAV_MAT_VEHICLE_BODY,
            0x15 => Self::FO_HAV_MAT_VEHICLE_PART_SOLID,
            0x16 => Self::FO_HAV_MAT_VEHICLE_PART_HOLLOW,
            0x17 => Self::FO_HAV_MAT_BARREL,
            0x18 => Self::FO_HAV_MAT_BOTTLE,
            0x19 => Self::FO_HAV_MAT_SODA_CAN,
            0x1a => Self::FO_HAV_MAT_PISTOL,
            0x1b => Self::FO_HAV_MAT_RIFLE,
            0x1c => Self::FO_HAV_MAT_SHOPPING_CART,
            0x1d => Self::FO_HAV_MAT_LUNCHBOX,
            0x1e => Self::FO_HAV_MAT_BABY_RATTLE,
            0x1f => Self::FO_HAV_MAT_RUBBER_BALL,
            0x20 => Self::FO_HAV_MAT_STONE_PLATFORM,
            0x21 => Self::FO_HAV_MAT_CLOTH_PLATFORM,
            0x22 => Self::FO_HAV_MAT_DIRT_PLATFORM,
            0x23 => Self::FO_HAV_MAT_GLASS_PLATFORM,
            0x24 => Self::FO_HAV_MAT_GRASS_PLATFORM,
            0x25 => Self::FO_HAV_MAT_METAL_PLATFORM,
            0x26 => Self::FO_HAV_MAT_ORGANIC_PLATFORM,
            0x27 => Self::FO_HAV_MAT_SKIN_PLATFORM,
            0x28 => Self::FO_HAV_MAT_WATER_PLATFORM,
            0x29 => Self::FO_HAV_MAT_WOOD_PLATFORM,
            0x2a => Self::FO_HAV_MAT_HEAVY_STONE_PLATFORM,
            0x2b => Self::FO_HAV_MAT_HEAVY_METAL_PLATFORM,
            0x2c => Self::FO_HAV_MAT_HEAVY_WOOD_PLATFORM,
            0x2d => Self::FO_HAV_MAT_CHAIN_PLATFORM,
            0x2e => Self::FO_HAV_MAT_BOTTLECAP_PLATFORM,
            0x2f => Self::FO_HAV_MAT_ELEVATOR_PLATFORM,
            0x30 => Self::FO_HAV_MAT_HOLLOW_METAL_PLATFORM,
            0x31 => Self::FO_HAV_MAT_SHEET_METAL_PLATFORM,
            0x32 => Self::FO_HAV_MAT_SAND_PLATFORM,
            0x33 => Self::FO_HAV_MAT_BROKEN_CONCRETE_PLATFORM,
            0x34 => Self::FO_HAV_MAT_VEHICLE_BODY_PLATFORM,
            0x35 => Self::FO_HAV_MAT_VEHICLE_PART_SOLID_PLATFORM,
            0x36 => Self::FO_HAV_MAT_VEHICLE_PART_HOLLOW_PLATFORM,
            0x37 => Self::FO_HAV_MAT_BARREL_PLATFORM,
            0x38 => Self::FO_HAV_MAT_BOTTLE_PLATFORM,
            0x39 => Self::FO_HAV_MAT_SODA_CAN_PLATFORM,
            0x3a => Self::FO_HAV_MAT_PISTOL_PLATFORM,
            0x3b => Self::FO_HAV_MAT_RIFLE_PLATFORM,
            0x3c => Self::FO_HAV_MAT_SHOPPING_CART_PLATFORM,
            0x3d => Self::FO_HAV_MAT_LUNCHBOX_PLATFORM,
            0x3e => Self::FO_HAV_MAT_BABY_RATTLE_PLATFORM,
            0x3f => Self::FO_HAV_MAT_RUBBER_BALL_PLATFORM,
            0x40 => Self::FO_HAV_MAT_STONE_STAIRS,
            0x41 => Self::FO_HAV_MAT_CLOTH_STAIRS,
            0x42 => Self::FO_HAV_MAT_DIRT_STAIRS,
            0x43 => Self::FO_HAV_MAT_GLASS_STAIRS,
            0x44 => Self::FO_HAV_MAT_GRASS_STAIRS,
            0x45 => Self::FO_HAV_MAT_METAL_STAIRS,
            0x46 => Self::FO_HAV_MAT_ORGANIC_STAIRS,
            0x47 => Self::FO_HAV_MAT_SKIN_STAIRS,
            0x48 => Self::FO_HAV_MAT_WATER_STAIRS,
            0x49 => Self::FO_HAV_MAT_WOOD_STAIRS,
            0x4a => Self::FO_HAV_MAT_HEAVY_STONE_STAIRS,
            0x4b => Self::FO_HAV_MAT_HEAVY_METAL_STAIRS,
            0x4c => Self::FO_HAV_MAT_HEAVY_WOOD_STAIRS,
            0x4d => Self::FO_HAV_MAT_CHAIN_STAIRS,
            0x4e => Self::FO_HAV_MAT_BOTTLECAP_STAIRS,
            0x4f => Self::FO_HAV_MAT_ELEVATOR_STAIRS,
            0x50 => Self::FO_HAV_MAT_HOLLOW_METAL_STAIRS,
            0x51 => Self::FO_HAV_MAT_SHEET_METAL_STAIRS,
            0x52 => Self::FO_HAV_MAT_SAND_STAIRS,
            0x53 => Self::FO_HAV_MAT_BROKEN_CONCRETE_STAIRS,
            0x54 => Self::FO_HAV_MAT_VEHICLE_BODY_STAIRS,
            0x55 => Self::FO_HAV_MAT_VEHICLE_PART_SOLID_STAIRS,
            0x56 => Self::FO_HAV_MAT_VEHICLE_PART_HOLLOW_STAIRS,
            0x57 => Self::FO_HAV_MAT_BARREL_STAIRS,
            0x58 => Self::FO_HAV_MAT_BOTTLE_STAIRS,
            0x59 => Self::FO_HAV_MAT_SODA_CAN_STAIRS,
            0x5a => Self::FO_HAV_MAT_PISTOL_STAIRS,
            0x5b => Self::FO_HAV_MAT_RIFLE_STAIRS,
            0x5c => Self::FO_HAV_MAT_SHOPPING_CART_STAIRS,
            0x5d => Self::FO_HAV_MAT_LUNCHBOX_STAIRS,
            0x5e => Self::FO_HAV_MAT_BABY_RATTLE_STAIRS,
            0x5f => Self::FO_HAV_MAT_RUBBER_BALL_STAIRS,
            0x60 => Self::FO_HAV_MAT_STONE_STAIRS_PLATFORM,
            0x61 => Self::FO_HAV_MAT_CLOTH_STAIRS_PLATFORM,
            0x62 => Self::FO_HAV_MAT_DIRT_STAIRS_PLATFORM,
            0x63 => Self::FO_HAV_MAT_GLASS_STAIRS_PLATFORM,
            0x64 => Self::FO_HAV_MAT_GRASS_STAIRS_PLATFORM,
            0x65 => Self::FO_HAV_MAT_METAL_STAIRS_PLATFORM,
            0x66 => Self::FO_HAV_MAT_ORGANIC_STAIRS_PLATFORM,
            0x67 => Self::FO_HAV_MAT_SKIN_STAIRS_PLATFORM,
            0x68 => Self::FO_HAV_MAT_WATER_STAIRS_PLATFORM,
            0x69 => Self::FO_HAV_MAT_WOOD_STAIRS_PLATFORM,
            0x6a => Self::FO_HAV_MAT_HEAVY_STONE_STAIRS_PLATFORM,
            0x6b => Self::FO_HAV_MAT_HEAVY_METAL_STAIRS_PLATFORM,
            0x6c => Self::FO_HAV_MAT_HEAVY_WOOD_STAIRS_PLATFORM,
            0x6d => Self::FO_HAV_MAT_CHAIN_STAIRS_PLATFORM,
            0x6e => Self::FO_HAV_MAT_BOTTLECAP_STAIRS_PLATFORM,
            0x6f => Self::FO_HAV_MAT_ELEVATOR_STAIRS_PLATFORM,
            0x70 => Self::FO_HAV_MAT_HOLLOW_METAL_STAIRS_PLATFORM,
            0x71 => Self::FO_HAV_MAT_SHEET_METAL_STAIRS_PLATFORM,
            0x72 => Self::FO_HAV_MAT_SAND_STAIRS_PLATFORM,
            0x73 => Self::FO_HAV_MAT_BROKEN_CONCRETE_STAIRS_PLATFORM,
            0x74 => Self::FO_HAV_MAT_VEHICLE_BODY_STAIRS_PLATFORM,
            0x75 => Self::FO_HAV_MAT_VEHICLE_PART_SOLID_STAIRS_PLATFORM,
            0x76 => Self::FO_HAV_MAT_VEHICLE_PART_HOLLOW_STAIRS_PLATFORM,
            0x77 => Self::FO_HAV_MAT_BARREL_STAIRS_PLATFORM,
            0x78 => Self::FO_HAV_MAT_BOTTLE_STAIRS_PLATFORM,
            0x79 => Self::FO_HAV_MAT_SODA_CAN_STAIRS_PLATFORM,
            0x7a => Self::FO_HAV_MAT_PISTOL_STAIRS_PLATFORM,
            0x7b => Self::FO_HAV_MAT_RIFLE_STAIRS_PLATFORM,
            0x7c => Self::FO_HAV_MAT_SHOPPING_CART_STAIRS_PLATFORM,
            0x7d => Self::FO_HAV_MAT_LUNCHBOX_STAIRS_PLATFORM,
            0x7e => Self::FO_HAV_MAT_BABY_RATTLE_STAIRS_PLATFORM,
            0x7f => Self::FO_HAV_MAT_RUBBER_BALL_STAIRS_PLATFORM,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Bethesda Havok. Material descriptor for a Havok shape in Skyrim.
/// C# `enum SkyrimHavokMaterial : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum SkyrimHavokMaterial {
    SKY_HAV_MAT_BROKEN_STONE = 0x7d13747,
    SKY_HAV_MAT_LIGHT_WOOD = 0x15c7dee3,
    SKY_HAV_MAT_SNOW = 0x17c77aaf,
    SKY_HAV_MAT_GRAVEL = 0x198bba58,
    SKY_HAV_MAT_MATERIAL_CHAIN_METAL = 0x1a2944e4,
    SKY_HAV_MAT_BOTTLE = 0x1d6b08f6,
    SKY_HAV_MAT_WOOD = 0x1dd9c611,
    SKY_HAV_MAT_SKIN = 0x233db702,
    SKY_HAV_MAT_UNKNOWN_617099282 = 0x24c83012,
    SKY_HAV_MAT_BARREL = 0x2ba39614,
    SKY_HAV_MAT_MATERIAL_CERAMIC_MEDIUM = 0x2e97335b,
    SKY_HAV_MAT_MATERIAL_BASKET = 0x2f22696e,
    SKY_HAV_MAT_ICE = 0x340e5d1c,
    SKY_HAV_MAT_STAIRS_STONE = 0x359d733d,
    SKY_HAV_MAT_WATER = 0x3d11e3c7,
    SKY_HAV_MAT_UNKNOWN_1028101969 = 0x3d479751,
    SKY_HAV_MAT_MATERIAL_BLADE_1HAND = 0x3f30e0a4,
    SKY_HAV_MAT_MATERIAL_BOOK = 0x4b616052,
    SKY_HAV_MAT_MATERIAL_CARPET = 0x4cb1913f,
    SKY_HAV_MAT_SOLID_METAL = 0x4ccacc3b,
    SKY_HAV_MAT_MATERIAL_AXE_1HAND = 0x4dd302cb,
    SKY_HAV_MAT_UNKNOWN_1440721808 = 0x55dfab90,
    SKY_HAV_MAT_STAIRS_WOOD = 0x571ff595,
    SKY_HAV_MAT_MUD = 0x58987081,
    SKY_HAV_MAT_MATERIAL_BOULDER_SMALL = 0x5c710dd6,
    SKY_HAV_MAT_STAIRS_SNOW = 0x5d01492b,
    SKY_HAV_MAT_HEAVY_STONE = 0x5da0d740,
    SKY_HAV_MAT_UNKNOWN_1574477864 = 0x5dd8a028,
    SKY_HAV_MAT_UNKNOWN_1591009235 = 0x5ed4dfd3,
    SKY_HAV_MAT_MATERIAL_BOWS_STAVES = 0x5fcad641,
    SKY_HAV_MAT_MATERIAL_WOOD_AS_STAIRS = 0x6b80500c,
    SKY_HAV_MAT_GRASS = 0x6e2f68ee,
    SKY_HAV_MAT_MATERIAL_BOULDER_LARGE = 0x705fce7b,
    SKY_HAV_MAT_MATERIAL_STONE_AS_STAIRS = 0x706b457f,
    SKY_HAV_MAT_MATERIAL_BLADE_2HAND = 0x78909a74,
    SKY_HAV_MAT_MATERIAL_BOTTLE_SMALL = 0x78bf2c58,
    SKY_HAV_MAT_SAND = 0x813e4d0d,
    SKY_HAV_MAT_HEAVY_METAL = 0x84e226a3,
    SKY_HAV_MAT_UNKNOWN_2290050264 = 0x887f64d8,
    SKY_HAV_MAT_DRAGON = 0x961a8817,
    SKY_HAV_MAT_MATERIAL_BLADE_1HAND_SMALL = 0x9c0aaacc,
    SKY_HAV_MAT_MATERIAL_SKIN_SMALL = 0x9ce6bd3e,
    SKY_HAV_MAT_STAIRS_BROKEN_STONE = 0xac66695b,
    SKY_HAV_MAT_MATERIAL_SKIN_LARGE = 0xb0c87e93,
    SKY_HAV_MAT_ORGANIC = 0xb151addb,
    SKY_HAV_MAT_MATERIAL_BONE = 0xb5c27c14,
    SKY_HAV_MAT_HEAVY_WOOD = 0xb7087047,
    SKY_HAV_MAT_MATERIAL_CHAIN = 0xb73b4366,
    SKY_HAV_MAT_DIRT = 0xb9233eaa,
    SKY_HAV_MAT_MATERIAL_ARMOR_LIGHT = 0xcc21169d,
    SKY_HAV_MAT_MATERIAL_SHIELD_LIGHT = 0xcd86ddf8,
    SKY_HAV_MAT_MATERIAL_COIN = 0xd5ed543e,
    SKY_HAV_MAT_MATERIAL_SHIELD_HEAVY = 0xdcadfb50,
    SKY_HAV_MAT_MATERIAL_ARMOR_HEAVY = 0xdd0a3035,
    SKY_HAV_MAT_MATERIAL_ARROW = 0xde0eb592,
    SKY_HAV_MAT_GLASS = 0xdee94842,
    SKY_HAV_MAT_STONE = 0xdf02f237,
    SKY_HAV_MAT_CLOTH = 0xe4d39ca3,
    SKY_HAV_MAT_MATERIAL_BLUNT_2HAND = 0xec9b2bd5,
    SKY_HAV_MAT_UNKNOWN_4239621792 = 0xfcb37ea0,
    SKY_HAV_MAT_MATERIAL_BOULDER_MEDIUM = 0xff56a8e2, // Material Boulder Medium
}

impl SkyrimHavokMaterial {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x7d13747 => Self::SKY_HAV_MAT_BROKEN_STONE,
            0x15c7dee3 => Self::SKY_HAV_MAT_LIGHT_WOOD,
            0x17c77aaf => Self::SKY_HAV_MAT_SNOW,
            0x198bba58 => Self::SKY_HAV_MAT_GRAVEL,
            0x1a2944e4 => Self::SKY_HAV_MAT_MATERIAL_CHAIN_METAL,
            0x1d6b08f6 => Self::SKY_HAV_MAT_BOTTLE,
            0x1dd9c611 => Self::SKY_HAV_MAT_WOOD,
            0x233db702 => Self::SKY_HAV_MAT_SKIN,
            0x24c83012 => Self::SKY_HAV_MAT_UNKNOWN_617099282,
            0x2ba39614 => Self::SKY_HAV_MAT_BARREL,
            0x2e97335b => Self::SKY_HAV_MAT_MATERIAL_CERAMIC_MEDIUM,
            0x2f22696e => Self::SKY_HAV_MAT_MATERIAL_BASKET,
            0x340e5d1c => Self::SKY_HAV_MAT_ICE,
            0x359d733d => Self::SKY_HAV_MAT_STAIRS_STONE,
            0x3d11e3c7 => Self::SKY_HAV_MAT_WATER,
            0x3d479751 => Self::SKY_HAV_MAT_UNKNOWN_1028101969,
            0x3f30e0a4 => Self::SKY_HAV_MAT_MATERIAL_BLADE_1HAND,
            0x4b616052 => Self::SKY_HAV_MAT_MATERIAL_BOOK,
            0x4cb1913f => Self::SKY_HAV_MAT_MATERIAL_CARPET,
            0x4ccacc3b => Self::SKY_HAV_MAT_SOLID_METAL,
            0x4dd302cb => Self::SKY_HAV_MAT_MATERIAL_AXE_1HAND,
            0x55dfab90 => Self::SKY_HAV_MAT_UNKNOWN_1440721808,
            0x571ff595 => Self::SKY_HAV_MAT_STAIRS_WOOD,
            0x58987081 => Self::SKY_HAV_MAT_MUD,
            0x5c710dd6 => Self::SKY_HAV_MAT_MATERIAL_BOULDER_SMALL,
            0x5d01492b => Self::SKY_HAV_MAT_STAIRS_SNOW,
            0x5da0d740 => Self::SKY_HAV_MAT_HEAVY_STONE,
            0x5dd8a028 => Self::SKY_HAV_MAT_UNKNOWN_1574477864,
            0x5ed4dfd3 => Self::SKY_HAV_MAT_UNKNOWN_1591009235,
            0x5fcad641 => Self::SKY_HAV_MAT_MATERIAL_BOWS_STAVES,
            0x6b80500c => Self::SKY_HAV_MAT_MATERIAL_WOOD_AS_STAIRS,
            0x6e2f68ee => Self::SKY_HAV_MAT_GRASS,
            0x705fce7b => Self::SKY_HAV_MAT_MATERIAL_BOULDER_LARGE,
            0x706b457f => Self::SKY_HAV_MAT_MATERIAL_STONE_AS_STAIRS,
            0x78909a74 => Self::SKY_HAV_MAT_MATERIAL_BLADE_2HAND,
            0x78bf2c58 => Self::SKY_HAV_MAT_MATERIAL_BOTTLE_SMALL,
            0x813e4d0d => Self::SKY_HAV_MAT_SAND,
            0x84e226a3 => Self::SKY_HAV_MAT_HEAVY_METAL,
            0x887f64d8 => Self::SKY_HAV_MAT_UNKNOWN_2290050264,
            0x961a8817 => Self::SKY_HAV_MAT_DRAGON,
            0x9c0aaacc => Self::SKY_HAV_MAT_MATERIAL_BLADE_1HAND_SMALL,
            0x9ce6bd3e => Self::SKY_HAV_MAT_MATERIAL_SKIN_SMALL,
            0xac66695b => Self::SKY_HAV_MAT_STAIRS_BROKEN_STONE,
            0xb0c87e93 => Self::SKY_HAV_MAT_MATERIAL_SKIN_LARGE,
            0xb151addb => Self::SKY_HAV_MAT_ORGANIC,
            0xb5c27c14 => Self::SKY_HAV_MAT_MATERIAL_BONE,
            0xb7087047 => Self::SKY_HAV_MAT_HEAVY_WOOD,
            0xb73b4366 => Self::SKY_HAV_MAT_MATERIAL_CHAIN,
            0xb9233eaa => Self::SKY_HAV_MAT_DIRT,
            0xcc21169d => Self::SKY_HAV_MAT_MATERIAL_ARMOR_LIGHT,
            0xcd86ddf8 => Self::SKY_HAV_MAT_MATERIAL_SHIELD_LIGHT,
            0xd5ed543e => Self::SKY_HAV_MAT_MATERIAL_COIN,
            0xdcadfb50 => Self::SKY_HAV_MAT_MATERIAL_SHIELD_HEAVY,
            0xdd0a3035 => Self::SKY_HAV_MAT_MATERIAL_ARMOR_HEAVY,
            0xde0eb592 => Self::SKY_HAV_MAT_MATERIAL_ARROW,
            0xdee94842 => Self::SKY_HAV_MAT_GLASS,
            0xdf02f237 => Self::SKY_HAV_MAT_STONE,
            0xe4d39ca3 => Self::SKY_HAV_MAT_CLOTH,
            0xec9b2bd5 => Self::SKY_HAV_MAT_MATERIAL_BLUNT_2HAND,
            0xfcb37ea0 => Self::SKY_HAV_MAT_UNKNOWN_4239621792,
            0xff56a8e2 => Self::SKY_HAV_MAT_MATERIAL_BOULDER_MEDIUM,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Bethesda Havok. Describes the collision layer a body belongs to in Oblivion.
/// C# `enum OblivionLayer : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum OblivionLayer {
    OL_UNIDENTIFIED = 0x0,
    OL_STATIC = 0x1,
    OL_ANIM_STATIC = 0x2,
    OL_TRANSPARENT = 0x3,
    OL_CLUTTER = 0x4,
    OL_WEAPON = 0x5,
    OL_PROJECTILE = 0x6,
    OL_SPELL = 0x7,
    OL_BIPED = 0x8,
    OL_TREES = 0x9,
    OL_PROPS = 0xa,
    OL_WATER = 0xb,
    OL_TRIGGER = 0xc,
    OL_TERRAIN = 0xd,
    OL_TRAP = 0xe,
    OL_NONCOLLIDABLE = 0xf,
    OL_CLOUD_TRAP = 0x10,
    OL_GROUND = 0x11,
    OL_PORTAL = 0x12,
    OL_STAIRS = 0x13,
    OL_CHAR_CONTROLLER = 0x14,
    OL_AVOID_BOX = 0x15,
    OL_UNKNOWN1 = 0x16,
    OL_UNKNOWN2 = 0x17,
    OL_CAMERA_PICK = 0x18,
    OL_ITEM_PICK = 0x19,
    OL_LINE_OF_SIGHT = 0x1a,
    OL_PATH_PICK = 0x1b,
    OL_CUSTOM_PICK_1 = 0x1c,
    OL_CUSTOM_PICK_2 = 0x1d,
    OL_SPELL_EXPLOSION = 0x1e,
    OL_DROPPING_PICK = 0x1f,
    OL_OTHER = 0x20,
    OL_HEAD = 0x21,
    OL_BODY = 0x22,
    OL_SPINE1 = 0x23,
    OL_SPINE2 = 0x24,
    OL_L_UPPER_ARM = 0x25,
    OL_L_FOREARM = 0x26,
    OL_L_HAND = 0x27,
    OL_L_THIGH = 0x28,
    OL_L_CALF = 0x29,
    OL_L_FOOT = 0x2a,
    OL_R_UPPER_ARM = 0x2b,
    OL_R_FOREARM = 0x2c,
    OL_R_HAND = 0x2d,
    OL_R_THIGH = 0x2e,
    OL_R_CALF = 0x2f,
    OL_R_FOOT = 0x30,
    OL_TAIL = 0x31,
    OL_SIDE_WEAPON = 0x32,
    OL_SHIELD = 0x33,
    OL_QUIVER = 0x34,
    OL_BACK_WEAPON = 0x35,
    OL_BACK_WEAPON2 = 0x36,
    OL_PONYTAIL = 0x37,
    OL_WING = 0x38,
    OL_NULL = 0x39, // Null
}

impl OblivionLayer {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::OL_UNIDENTIFIED,
            0x1 => Self::OL_STATIC,
            0x2 => Self::OL_ANIM_STATIC,
            0x3 => Self::OL_TRANSPARENT,
            0x4 => Self::OL_CLUTTER,
            0x5 => Self::OL_WEAPON,
            0x6 => Self::OL_PROJECTILE,
            0x7 => Self::OL_SPELL,
            0x8 => Self::OL_BIPED,
            0x9 => Self::OL_TREES,
            0xa => Self::OL_PROPS,
            0xb => Self::OL_WATER,
            0xc => Self::OL_TRIGGER,
            0xd => Self::OL_TERRAIN,
            0xe => Self::OL_TRAP,
            0xf => Self::OL_NONCOLLIDABLE,
            0x10 => Self::OL_CLOUD_TRAP,
            0x11 => Self::OL_GROUND,
            0x12 => Self::OL_PORTAL,
            0x13 => Self::OL_STAIRS,
            0x14 => Self::OL_CHAR_CONTROLLER,
            0x15 => Self::OL_AVOID_BOX,
            0x16 => Self::OL_UNKNOWN1,
            0x17 => Self::OL_UNKNOWN2,
            0x18 => Self::OL_CAMERA_PICK,
            0x19 => Self::OL_ITEM_PICK,
            0x1a => Self::OL_LINE_OF_SIGHT,
            0x1b => Self::OL_PATH_PICK,
            0x1c => Self::OL_CUSTOM_PICK_1,
            0x1d => Self::OL_CUSTOM_PICK_2,
            0x1e => Self::OL_SPELL_EXPLOSION,
            0x1f => Self::OL_DROPPING_PICK,
            0x20 => Self::OL_OTHER,
            0x21 => Self::OL_HEAD,
            0x22 => Self::OL_BODY,
            0x23 => Self::OL_SPINE1,
            0x24 => Self::OL_SPINE2,
            0x25 => Self::OL_L_UPPER_ARM,
            0x26 => Self::OL_L_FOREARM,
            0x27 => Self::OL_L_HAND,
            0x28 => Self::OL_L_THIGH,
            0x29 => Self::OL_L_CALF,
            0x2a => Self::OL_L_FOOT,
            0x2b => Self::OL_R_UPPER_ARM,
            0x2c => Self::OL_R_FOREARM,
            0x2d => Self::OL_R_HAND,
            0x2e => Self::OL_R_THIGH,
            0x2f => Self::OL_R_CALF,
            0x30 => Self::OL_R_FOOT,
            0x31 => Self::OL_TAIL,
            0x32 => Self::OL_SIDE_WEAPON,
            0x33 => Self::OL_SHIELD,
            0x34 => Self::OL_QUIVER,
            0x35 => Self::OL_BACK_WEAPON,
            0x36 => Self::OL_BACK_WEAPON2,
            0x37 => Self::OL_PONYTAIL,
            0x38 => Self::OL_WING,
            0x39 => Self::OL_NULL,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

/// Bethesda Havok. Describes the collision layer a body belongs to in Fallout 3 and Fallout NV.
/// C# `enum Fallout3Layer : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Fallout3Layer {
    FOL_UNIDENTIFIED = 0x0,
    FOL_STATIC = 0x1,
    FOL_ANIM_STATIC = 0x2,
    FOL_TRANSPARENT = 0x3,
    FOL_CLUTTER = 0x4,
    FOL_WEAPON = 0x5,
    FOL_PROJECTILE = 0x6,
    FOL_SPELL = 0x7,
    FOL_BIPED = 0x8,
    FOL_TREES = 0x9,
    FOL_PROPS = 0xa,
    FOL_WATER = 0xb,
    FOL_TRIGGER = 0xc,
    FOL_TERRAIN = 0xd,
    FOL_TRAP = 0xe,
    FOL_NONCOLLIDABLE = 0xf,
    FOL_CLOUD_TRAP = 0x10,
    FOL_GROUND = 0x11,
    FOL_PORTAL = 0x12,
    FOL_DEBRIS_SMALL = 0x13,
    FOL_DEBRIS_LARGE = 0x14,
    FOL_ACOUSTIC_SPACE = 0x15,
    FOL_ACTORZONE = 0x16,
    FOL_PROJECTILEZONE = 0x17,
    FOL_GASTRAP = 0x18,
    FOL_SHELLCASING = 0x19,
    FOL_TRANSPARENT_SMALL = 0x1a,
    FOL_INVISIBLE_WALL = 0x1b,
    FOL_TRANSPARENT_SMALL_ANIM = 0x1c,
    FOL_DEADBIP = 0x1d,
    FOL_CHARCONTROLLER = 0x1e,
    FOL_AVOIDBOX = 0x1f,
    FOL_COLLISIONBOX = 0x20,
    FOL_CAMERASPHERE = 0x21,
    FOL_DOORDETECTION = 0x22,
    FOL_CAMERAPICK = 0x23,
    FOL_ITEMPICK = 0x24,
    FOL_LINEOFSIGHT = 0x25,
    FOL_PATHPICK = 0x26,
    FOL_CUSTOMPICK1 = 0x27,
    FOL_CUSTOMPICK2 = 0x28,
    FOL_SPELLEXPLOSION = 0x29,
    FOL_DROPPINGPICK = 0x2a,
    FOL_NULL = 0x2b, // Null (white)
}

impl Fallout3Layer {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::FOL_UNIDENTIFIED,
            0x1 => Self::FOL_STATIC,
            0x2 => Self::FOL_ANIM_STATIC,
            0x3 => Self::FOL_TRANSPARENT,
            0x4 => Self::FOL_CLUTTER,
            0x5 => Self::FOL_WEAPON,
            0x6 => Self::FOL_PROJECTILE,
            0x7 => Self::FOL_SPELL,
            0x8 => Self::FOL_BIPED,
            0x9 => Self::FOL_TREES,
            0xa => Self::FOL_PROPS,
            0xb => Self::FOL_WATER,
            0xc => Self::FOL_TRIGGER,
            0xd => Self::FOL_TERRAIN,
            0xe => Self::FOL_TRAP,
            0xf => Self::FOL_NONCOLLIDABLE,
            0x10 => Self::FOL_CLOUD_TRAP,
            0x11 => Self::FOL_GROUND,
            0x12 => Self::FOL_PORTAL,
            0x13 => Self::FOL_DEBRIS_SMALL,
            0x14 => Self::FOL_DEBRIS_LARGE,
            0x15 => Self::FOL_ACOUSTIC_SPACE,
            0x16 => Self::FOL_ACTORZONE,
            0x17 => Self::FOL_PROJECTILEZONE,
            0x18 => Self::FOL_GASTRAP,
            0x19 => Self::FOL_SHELLCASING,
            0x1a => Self::FOL_TRANSPARENT_SMALL,
            0x1b => Self::FOL_INVISIBLE_WALL,
            0x1c => Self::FOL_TRANSPARENT_SMALL_ANIM,
            0x1d => Self::FOL_DEADBIP,
            0x1e => Self::FOL_CHARCONTROLLER,
            0x1f => Self::FOL_AVOIDBOX,
            0x20 => Self::FOL_COLLISIONBOX,
            0x21 => Self::FOL_CAMERASPHERE,
            0x22 => Self::FOL_DOORDETECTION,
            0x23 => Self::FOL_CAMERAPICK,
            0x24 => Self::FOL_ITEMPICK,
            0x25 => Self::FOL_LINEOFSIGHT,
            0x26 => Self::FOL_PATHPICK,
            0x27 => Self::FOL_CUSTOMPICK1,
            0x28 => Self::FOL_CUSTOMPICK2,
            0x29 => Self::FOL_SPELLEXPLOSION,
            0x2a => Self::FOL_DROPPINGPICK,
            0x2b => Self::FOL_NULL,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

/// Bethesda Havok. Describes the collision layer a body belongs to in Skyrim.
/// C# `enum SkyrimLayer : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum SkyrimLayer {
    SKYL_UNIDENTIFIED = 0x0,
    SKYL_STATIC = 0x1,
    SKYL_ANIMSTATIC = 0x2,
    SKYL_TRANSPARENT = 0x3,
    SKYL_CLUTTER = 0x4,
    SKYL_WEAPON = 0x5,
    SKYL_PROJECTILE = 0x6,
    SKYL_SPELL = 0x7,
    SKYL_BIPED = 0x8,
    SKYL_TREES = 0x9,
    SKYL_PROPS = 0xa,
    SKYL_WATER = 0xb,
    SKYL_TRIGGER = 0xc,
    SKYL_TERRAIN = 0xd,
    SKYL_TRAP = 0xe,
    SKYL_NONCOLLIDABLE = 0xf,
    SKYL_CLOUD_TRAP = 0x10,
    SKYL_GROUND = 0x11,
    SKYL_PORTAL = 0x12,
    SKYL_DEBRIS_SMALL = 0x13,
    SKYL_DEBRIS_LARGE = 0x14,
    SKYL_ACOUSTIC_SPACE = 0x15,
    SKYL_ACTORZONE = 0x16,
    SKYL_PROJECTILEZONE = 0x17,
    SKYL_GASTRAP = 0x18,
    SKYL_SHELLCASING = 0x19,
    SKYL_TRANSPARENT_SMALL = 0x1a,
    SKYL_INVISIBLE_WALL = 0x1b,
    SKYL_TRANSPARENT_SMALL_ANIM = 0x1c,
    SKYL_WARD = 0x1d,
    SKYL_CHARCONTROLLER = 0x1e,
    SKYL_STAIRHELPER = 0x1f,
    SKYL_DEADBIP = 0x20,
    SKYL_BIPED_NO_CC = 0x21,
    SKYL_AVOIDBOX = 0x22,
    SKYL_COLLISIONBOX = 0x23,
    SKYL_CAMERASHPERE = 0x24,
    SKYL_DOORDETECTION = 0x25,
    SKYL_CONEPROJECTILE = 0x26,
    SKYL_CAMERAPICK = 0x27,
    SKYL_ITEMPICK = 0x28,
    SKYL_LINEOFSIGHT = 0x29,
    SKYL_PATHPICK = 0x2a,
    SKYL_CUSTOMPICK1 = 0x2b,
    SKYL_CUSTOMPICK2 = 0x2c,
    SKYL_SPELLEXPLOSION = 0x2d,
    SKYL_DROPPINGPICK = 0x2e,
    SKYL_NULL = 0x2f, // Null
}

impl SkyrimLayer {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::SKYL_UNIDENTIFIED,
            0x1 => Self::SKYL_STATIC,
            0x2 => Self::SKYL_ANIMSTATIC,
            0x3 => Self::SKYL_TRANSPARENT,
            0x4 => Self::SKYL_CLUTTER,
            0x5 => Self::SKYL_WEAPON,
            0x6 => Self::SKYL_PROJECTILE,
            0x7 => Self::SKYL_SPELL,
            0x8 => Self::SKYL_BIPED,
            0x9 => Self::SKYL_TREES,
            0xa => Self::SKYL_PROPS,
            0xb => Self::SKYL_WATER,
            0xc => Self::SKYL_TRIGGER,
            0xd => Self::SKYL_TERRAIN,
            0xe => Self::SKYL_TRAP,
            0xf => Self::SKYL_NONCOLLIDABLE,
            0x10 => Self::SKYL_CLOUD_TRAP,
            0x11 => Self::SKYL_GROUND,
            0x12 => Self::SKYL_PORTAL,
            0x13 => Self::SKYL_DEBRIS_SMALL,
            0x14 => Self::SKYL_DEBRIS_LARGE,
            0x15 => Self::SKYL_ACOUSTIC_SPACE,
            0x16 => Self::SKYL_ACTORZONE,
            0x17 => Self::SKYL_PROJECTILEZONE,
            0x18 => Self::SKYL_GASTRAP,
            0x19 => Self::SKYL_SHELLCASING,
            0x1a => Self::SKYL_TRANSPARENT_SMALL,
            0x1b => Self::SKYL_INVISIBLE_WALL,
            0x1c => Self::SKYL_TRANSPARENT_SMALL_ANIM,
            0x1d => Self::SKYL_WARD,
            0x1e => Self::SKYL_CHARCONTROLLER,
            0x1f => Self::SKYL_STAIRHELPER,
            0x20 => Self::SKYL_DEADBIP,
            0x21 => Self::SKYL_BIPED_NO_CC,
            0x22 => Self::SKYL_AVOIDBOX,
            0x23 => Self::SKYL_COLLISIONBOX,
            0x24 => Self::SKYL_CAMERASHPERE,
            0x25 => Self::SKYL_DOORDETECTION,
            0x26 => Self::SKYL_CONEPROJECTILE,
            0x27 => Self::SKYL_CAMERAPICK,
            0x28 => Self::SKYL_ITEMPICK,
            0x29 => Self::SKYL_LINEOFSIGHT,
            0x2a => Self::SKYL_PATHPICK,
            0x2b => Self::SKYL_CUSTOMPICK1,
            0x2c => Self::SKYL_CUSTOMPICK2,
            0x2d => Self::SKYL_SPELLEXPLOSION,
            0x2e => Self::SKYL_DROPPINGPICK,
            0x2f => Self::SKYL_NULL,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

/// Bethesda Havok.
/// A byte describing if MOPP Data is organized into chunks (PS3) or not (PC)
/// C# `enum MoppDataBuildType : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum MoppDataBuildType {
    BUILT_WITH_CHUNK_SUBDIVISION = 0x0,
    BUILT_WITHOUT_CHUNK_SUBDIVISION = 0x1,
    BUILD_NOT_SET = 0x2, // Build type not set yet.
}

impl MoppDataBuildType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::BUILT_WITH_CHUNK_SUBDIVISION,
            0x1 => Self::BUILT_WITHOUT_CHUNK_SUBDIVISION,
            0x2 => Self::BUILD_NOT_SET,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

/// Describes the pixel format used by the NiPixelData object to store a texture.
/// C# `enum PixelFormat : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum PixelFormat {
    FMT_RGB = 0x0,
    FMT_RGBA = 0x1,
    FMT_PAL = 0x2,
    FMT_PALA = 0x3,
    FMT_DXT1 = 0x4,
    FMT_DXT3 = 0x5,
    FMT_DXT5 = 0x6,
    FMT_RGB24NONINT = 0x7,
    FMT_BUMP = 0x8,
    FMT_BUMPLUMA = 0x9,
    FMT_RENDERSPEC = 0xa,
    FMT_1CH = 0xb,
    FMT_2CH = 0xc,
    FMT_3CH = 0xd,
    FMT_4CH = 0xe,
    FMT_DEPTH_STENCIL = 0xf,
    FMT_UNKNOWN = 0x10,
}

impl PixelFormat {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::FMT_RGB,
            0x1 => Self::FMT_RGBA,
            0x2 => Self::FMT_PAL,
            0x3 => Self::FMT_PALA,
            0x4 => Self::FMT_DXT1,
            0x5 => Self::FMT_DXT3,
            0x6 => Self::FMT_DXT5,
            0x7 => Self::FMT_RGB24NONINT,
            0x8 => Self::FMT_BUMP,
            0x9 => Self::FMT_BUMPLUMA,
            0xa => Self::FMT_RENDERSPEC,
            0xb => Self::FMT_1CH,
            0xc => Self::FMT_2CH,
            0xd => Self::FMT_3CH,
            0xe => Self::FMT_4CH,
            0xf => Self::FMT_DEPTH_STENCIL,
            0x10 => Self::FMT_UNKNOWN,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Describes whether pixels have been tiled from their standard row-major format to a format optimized for a particular platform.
/// C# `enum PixelTiling : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum PixelTiling {
    TILE_NONE = 0x0,
    TILE_XENON = 0x1,
    TILE_WII = 0x2,
    TILE_NV_SWIZZLED = 0x3,
}

impl PixelTiling {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::TILE_NONE,
            0x1 => Self::TILE_XENON,
            0x2 => Self::TILE_WII,
            0x3 => Self::TILE_NV_SWIZZLED,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Describes the pixel format used by the NiPixelData object to store a texture.
/// C# `enum PixelComponent : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum PixelComponent {
    COMP_RED = 0x0,
    COMP_GREEN = 0x1,
    COMP_BLUE = 0x2,
    COMP_ALPHA = 0x3,
    COMP_COMPRESSED = 0x4,
    COMP_OFFSET_U = 0x5,
    COMP_OFFSET_V = 0x6,
    COMP_OFFSET_W = 0x7,
    COMP_OFFSET_Q = 0x8,
    COMP_LUMA = 0x9,
    COMP_HEIGHT = 0xa,
    COMP_VECTOR_X = 0xb,
    COMP_VECTOR_Y = 0xc,
    COMP_VECTOR_Z = 0xd,
    COMP_PADDING = 0xe,
    COMP_INTENSITY = 0xf,
    COMP_INDEX = 0x10,
    COMP_DEPTH = 0x11,
    COMP_STENCIL = 0x12,
    COMP_EMPTY = 0x13,
}

impl PixelComponent {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::COMP_RED,
            0x1 => Self::COMP_GREEN,
            0x2 => Self::COMP_BLUE,
            0x3 => Self::COMP_ALPHA,
            0x4 => Self::COMP_COMPRESSED,
            0x5 => Self::COMP_OFFSET_U,
            0x6 => Self::COMP_OFFSET_V,
            0x7 => Self::COMP_OFFSET_W,
            0x8 => Self::COMP_OFFSET_Q,
            0x9 => Self::COMP_LUMA,
            0xa => Self::COMP_HEIGHT,
            0xb => Self::COMP_VECTOR_X,
            0xc => Self::COMP_VECTOR_Y,
            0xd => Self::COMP_VECTOR_Z,
            0xe => Self::COMP_PADDING,
            0xf => Self::COMP_INTENSITY,
            0x10 => Self::COMP_INDEX,
            0x11 => Self::COMP_DEPTH,
            0x12 => Self::COMP_STENCIL,
            0x13 => Self::COMP_EMPTY,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Describes how each pixel should be accessed on NiPixelFormat.
/// C# `enum PixelRepresentation : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum PixelRepresentation {
    REP_NORM_INT = 0x0,
    REP_HALF = 0x1,
    REP_FLOAT = 0x2,
    REP_INDEX = 0x3,
    REP_COMPRESSED = 0x4,
    REP_UNKNOWN = 0x5,
    REP_INT = 0x6,
}

impl PixelRepresentation {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::REP_NORM_INT,
            0x1 => Self::REP_HALF,
            0x2 => Self::REP_FLOAT,
            0x3 => Self::REP_INDEX,
            0x4 => Self::REP_COMPRESSED,
            0x5 => Self::REP_UNKNOWN,
            0x6 => Self::REP_INT,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Describes the color depth in an NiTexture.
/// C# `enum PixelLayout : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum PixelLayout {
    LAY_PALETTIZED_8 = 0x0,
    LAY_HIGH_COLOR_16 = 0x1,
    LAY_TRUE_COLOR_32 = 0x2,
    LAY_COMPRESSED = 0x3,
    LAY_BUMPMAP = 0x4,
    LAY_PALETTIZED_4 = 0x5,
    LAY_DEFAULT = 0x6,
    LAY_SINGLE_COLOR_8 = 0x7,
    LAY_SINGLE_COLOR_16 = 0x8,
    LAY_SINGLE_COLOR_32 = 0x9,
    LAY_DOUBLE_COLOR_32 = 0xa,
    LAY_DOUBLE_COLOR_64 = 0xb,
    LAY_FLOAT_COLOR_32 = 0xc,
    LAY_FLOAT_COLOR_64 = 0xd,
    LAY_FLOAT_COLOR_128 = 0xe,
    LAY_SINGLE_COLOR_4 = 0xf,
    LAY_DEPTH_24_X8 = 0x10,
}

impl PixelLayout {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::LAY_PALETTIZED_8,
            0x1 => Self::LAY_HIGH_COLOR_16,
            0x2 => Self::LAY_TRUE_COLOR_32,
            0x3 => Self::LAY_COMPRESSED,
            0x4 => Self::LAY_BUMPMAP,
            0x5 => Self::LAY_PALETTIZED_4,
            0x6 => Self::LAY_DEFAULT,
            0x7 => Self::LAY_SINGLE_COLOR_8,
            0x8 => Self::LAY_SINGLE_COLOR_16,
            0x9 => Self::LAY_SINGLE_COLOR_32,
            0xa => Self::LAY_DOUBLE_COLOR_32,
            0xb => Self::LAY_DOUBLE_COLOR_64,
            0xc => Self::LAY_FLOAT_COLOR_32,
            0xd => Self::LAY_FLOAT_COLOR_64,
            0xe => Self::LAY_FLOAT_COLOR_128,
            0xf => Self::LAY_SINGLE_COLOR_4,
            0x10 => Self::LAY_DEPTH_24_X8,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Describes how mipmaps are handled in an NiTexture.
/// C# `enum MipMapFormat : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum MipMapFormat {
    MIP_FMT_NO = 0x0,
    MIP_FMT_YES = 0x1,
    MIP_FMT_DEFAULT = 0x2, // Use default setting.
}

impl MipMapFormat {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::MIP_FMT_NO,
            0x1 => Self::MIP_FMT_YES,
            0x2 => Self::MIP_FMT_DEFAULT,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Describes how transparency is handled in an NiTexture.
/// C# `enum AlphaFormat : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum AlphaFormat {
    ALPHA_NONE = 0x0,
    ALPHA_BINARY = 0x1,
    ALPHA_SMOOTH = 0x2,
    ALPHA_DEFAULT = 0x3, // Use default setting.
}

impl AlphaFormat {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::ALPHA_NONE,
            0x1 => Self::ALPHA_BINARY,
            0x2 => Self::ALPHA_SMOOTH,
            0x3 => Self::ALPHA_DEFAULT,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Describes the availiable texture clamp modes, i.e. the behavior of UV mapping outside the [0,1] range.
/// C# `enum TexClampMode : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum TexClampMode {
    CLAMP_S_CLAMP_T = 0x0,
    CLAMP_S_WRAP_T = 0x1,
    WRAP_S_CLAMP_T = 0x2,
    WRAP_S_WRAP_T = 0x3, // Wrap in both directions.
}

impl TexClampMode {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::CLAMP_S_CLAMP_T,
            0x1 => Self::CLAMP_S_WRAP_T,
            0x2 => Self::WRAP_S_CLAMP_T,
            0x3 => Self::WRAP_S_WRAP_T,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Describes the availiable texture filter modes, i.e. the way the pixels in a texture are displayed on screen.
/// C# `enum TexFilterMode : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum TexFilterMode {
    FILTER_NEAREST = 0x0,
    FILTER_BILERP = 0x1,
    FILTER_TRILERP = 0x2,
    FILTER_NEAREST_MIPNEAREST = 0x3,
    FILTER_NEAREST_MIPLERP = 0x4,
    FILTER_BILERP_MIPNEAREST = 0x5,
    FILTER_ANISOTROPIC = 0x6, // Anisotropic filtering. One or many trilinear samples depending on anisotropy.
}

impl TexFilterMode {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::FILTER_NEAREST,
            0x1 => Self::FILTER_BILERP,
            0x2 => Self::FILTER_TRILERP,
            0x3 => Self::FILTER_NEAREST_MIPNEAREST,
            0x4 => Self::FILTER_NEAREST_MIPLERP,
            0x5 => Self::FILTER_BILERP_MIPNEAREST,
            0x6 => Self::FILTER_ANISOTROPIC,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Describes how to apply vertex colors for NiVertexColorProperty.
/// C# `enum VertMode : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum VertMode {
    VERT_MODE_SRC_IGNORE = 0x0,
    VERT_MODE_SRC_EMISSIVE = 0x1,
    VERT_MODE_SRC_AMB_DIF = 0x2, // Ambient+Diffuse colors are specified by the source vertex colors. Emissive is specified by the NiMaterialProperty. (Default)
}

impl VertMode {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::VERT_MODE_SRC_IGNORE,
            0x1 => Self::VERT_MODE_SRC_EMISSIVE,
            0x2 => Self::VERT_MODE_SRC_AMB_DIF,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Describes which lighting equation components influence the final vertex color for NiVertexColorProperty.
/// C# `enum LightMode : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum LightMode {
    LIGHT_MODE_EMISSIVE = 0x0,
    LIGHT_MODE_EMI_AMB_DIF = 0x1, // Emissive + Ambient + Diffuse. (Default)
}

impl LightMode {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::LIGHT_MODE_EMISSIVE,
            0x1 => Self::LIGHT_MODE_EMI_AMB_DIF,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// The animation cyle behavior.
/// C# `enum CycleType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum CycleType {
    CYCLE_LOOP = 0x0,
    CYCLE_REVERSE = 0x1,
    CYCLE_CLAMP = 0x2, // Clamp
}

impl CycleType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::CYCLE_LOOP,
            0x1 => Self::CYCLE_REVERSE,
            0x2 => Self::CYCLE_CLAMP,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// The force field type.
/// C# `enum FieldType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum FieldType {
    FIELD_WIND = 0x0,
    FIELD_POINT = 0x1, // Point (fixed origin)
}

impl FieldType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::FIELD_WIND,
            0x1 => Self::FIELD_POINT,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Determines the way the billboard will react to the camera.
/// Billboard mode is stored in lowest 3 bits although Oblivion vanilla nifs uses values higher than 7.
/// C# `enum BillboardMode : u16`.
#[repr(u16)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum BillboardMode {
    ALWAYS_FACE_CAMERA = 0x0,
    ROTATE_ABOUT_UP = 0x1,
    RIGID_FACE_CAMERA = 0x2,
    ALWAYS_FACE_CENTER = 0x3,
    RIGID_FACE_CENTER = 0x4,
    BSROTATE_ABOUT_UP = 0x5,
    ROTATE_ABOUT_UP2 = 0x9, // The billboard will only rotate around the up axis (same as ROTATE_ABOUT_UP?).
}

impl BillboardMode {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u16) -> Option<Self> {
        Some(match v {
            0x0 => Self::ALWAYS_FACE_CAMERA,
            0x1 => Self::ROTATE_ABOUT_UP,
            0x2 => Self::RIGID_FACE_CAMERA,
            0x3 => Self::ALWAYS_FACE_CENTER,
            0x4 => Self::RIGID_FACE_CENTER,
            0x5 => Self::BSROTATE_ABOUT_UP,
            0x9 => Self::ROTATE_ABOUT_UP2,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u16 {
        self as u16
    }
}

/// Describes Z-buffer test modes for NiZBufferProperty.
/// "Less than" = closer to camera, "Greater than" = further from camera.
/// C# `enum ZCompareMode : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum ZCompareMode {
    ZCOMP_ALWAYS = 0x0,
    ZCOMP_LESS = 0x1,
    ZCOMP_EQUAL = 0x2,
    ZCOMP_LESS_EQUAL = 0x3,
    ZCOMP_GREATER = 0x4,
    ZCOMP_NOT_EQUAL = 0x5,
    ZCOMP_GREATER_EQUAL = 0x6,
    ZCOMP_NEVER = 0x7, // Always false. Ref value is ignored.
}

impl ZCompareMode {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::ZCOMP_ALWAYS,
            0x1 => Self::ZCOMP_LESS,
            0x2 => Self::ZCOMP_EQUAL,
            0x3 => Self::ZCOMP_LESS_EQUAL,
            0x4 => Self::ZCOMP_GREATER,
            0x5 => Self::ZCOMP_NOT_EQUAL,
            0x6 => Self::ZCOMP_GREATER_EQUAL,
            0x7 => Self::ZCOMP_NEVER,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Bethesda Havok, based on hkpMotion::MotionType. Motion type of a rigid body determines what happens when it is simulated.
/// C# `enum hkMotionType : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum hkMotionType {
    MO_SYS_INVALID = 0x0,
    MO_SYS_DYNAMIC = 0x1,
    MO_SYS_SPHERE_INERTIA = 0x2,
    MO_SYS_SPHERE_STABILIZED = 0x3,
    MO_SYS_BOX_INERTIA = 0x4,
    MO_SYS_BOX_STABILIZED = 0x5,
    MO_SYS_KEYFRAMED = 0x6,
    MO_SYS_FIXED = 0x7,
    MO_SYS_THIN_BOX = 0x8,
    MO_SYS_CHARACTER = 0x9, // A specialized motion used for character controllers
}

impl hkMotionType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::MO_SYS_INVALID,
            0x1 => Self::MO_SYS_DYNAMIC,
            0x2 => Self::MO_SYS_SPHERE_INERTIA,
            0x3 => Self::MO_SYS_SPHERE_STABILIZED,
            0x4 => Self::MO_SYS_BOX_INERTIA,
            0x5 => Self::MO_SYS_BOX_STABILIZED,
            0x6 => Self::MO_SYS_KEYFRAMED,
            0x7 => Self::MO_SYS_FIXED,
            0x8 => Self::MO_SYS_THIN_BOX,
            0x9 => Self::MO_SYS_CHARACTER,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

/// Bethesda Havok, based on hkpRigidBodyDeactivator::DeactivatorType.
/// Deactivator Type determines which mechanism Havok will use to classify the body as deactivated.
/// C# `enum hkDeactivatorType : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum hkDeactivatorType {
    DEACTIVATOR_INVALID = 0x0,
    DEACTIVATOR_NEVER = 0x1,
    DEACTIVATOR_SPATIAL = 0x2, // Tells Havok to use a spatial deactivation scheme. This makes use of high and low frequencies of positional motion to determine when deactivation should occur.
}

impl hkDeactivatorType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::DEACTIVATOR_INVALID,
            0x1 => Self::DEACTIVATOR_NEVER,
            0x2 => Self::DEACTIVATOR_SPATIAL,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

/// Bethesda Havok, based on hkpRigidBodyCinfo::SolverDeactivation.
/// A list of possible solver deactivation settings. This value defines how aggressively the solver deactivates objects.
/// Note: Solver deactivation does not save CPU, but reduces creeping of movable objects in a pile quite dramatically.
/// C# `enum hkSolverDeactivation : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum hkSolverDeactivation {
    SOLVER_DEACTIVATION_INVALID = 0x0,
    SOLVER_DEACTIVATION_OFF = 0x1,
    SOLVER_DEACTIVATION_LOW = 0x2,
    SOLVER_DEACTIVATION_MEDIUM = 0x3,
    SOLVER_DEACTIVATION_HIGH = 0x4,
    SOLVER_DEACTIVATION_MAX = 0x5, // Very fast deactivation, visible artifacts.
}

impl hkSolverDeactivation {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::SOLVER_DEACTIVATION_INVALID,
            0x1 => Self::SOLVER_DEACTIVATION_OFF,
            0x2 => Self::SOLVER_DEACTIVATION_LOW,
            0x3 => Self::SOLVER_DEACTIVATION_MEDIUM,
            0x4 => Self::SOLVER_DEACTIVATION_HIGH,
            0x5 => Self::SOLVER_DEACTIVATION_MAX,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

/// Bethesda Havok, based on hkpCollidableQualityType. Describes the priority and quality of collisions for a body,
/// e.g. you may expect critical game play objects to have solid high-priority collisions so that they never sink into ground,
/// or may allow penetrations for visual debris objects.
/// Notes:
/// - Fixed and keyframed objects cannot interact with each other.
/// - Debris can interpenetrate but still responds to Bullet hits.
/// - Critical objects are forced to not interpenetrate.
/// - Moving objects can interpenetrate slightly with other Moving or Debris objects but nothing else.
/// C# `enum hkQualityType : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum hkQualityType {
    MO_QUAL_INVALID = 0x0,
    MO_QUAL_FIXED = 0x1,
    MO_QUAL_KEYFRAMED = 0x2,
    MO_QUAL_DEBRIS = 0x3,
    MO_QUAL_MOVING = 0x4,
    MO_QUAL_CRITICAL = 0x5,
    MO_QUAL_BULLET = 0x6,
    MO_QUAL_USER = 0x7,
    MO_QUAL_CHARACTER = 0x8,
    MO_QUAL_KEYFRAMED_REPORT = 0x9, // Moving bodies with infinite mass which should report contact points and TOI collisions against all other bodies.
}

impl hkQualityType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::MO_QUAL_INVALID,
            0x1 => Self::MO_QUAL_FIXED,
            0x2 => Self::MO_QUAL_KEYFRAMED,
            0x3 => Self::MO_QUAL_DEBRIS,
            0x4 => Self::MO_QUAL_MOVING,
            0x5 => Self::MO_QUAL_CRITICAL,
            0x6 => Self::MO_QUAL_BULLET,
            0x7 => Self::MO_QUAL_USER,
            0x8 => Self::MO_QUAL_CHARACTER,
            0x9 => Self::MO_QUAL_KEYFRAMED_REPORT,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

/// Describes the decay function of bomb forces.
/// C# `enum DecayType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum DecayType {
    DECAY_NONE = 0x0,
    DECAY_LINEAR = 0x1,
    DECAY_EXPONENTIAL = 0x2, // Exponential decay.
}

impl DecayType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::DECAY_NONE,
            0x1 => Self::DECAY_LINEAR,
            0x2 => Self::DECAY_EXPONENTIAL,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Describes the symmetry type of bomb forces.
/// C# `enum SymmetryType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum SymmetryType {
    SPHERICAL_SYMMETRY = 0x0,
    CYLINDRICAL_SYMMETRY = 0x1,
    PLANAR_SYMMETRY = 0x2, // Planar Symmetry.
}

impl SymmetryType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::SPHERICAL_SYMMETRY,
            0x1 => Self::CYLINDRICAL_SYMMETRY,
            0x2 => Self::PLANAR_SYMMETRY,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// The type of information that is stored in a texture used by an NiTextureEffect.
/// C# `enum TextureType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum TextureType {
    TEX_PROJECTED_LIGHT = 0x0,
    TEX_PROJECTED_SHADOW = 0x1,
    TEX_ENVIRONMENT_MAP = 0x2,
    TEX_FOG_MAP = 0x3, // Apply a fog map texture. Alpha channel is used to blend the color channel with the base texture.
}

impl TextureType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::TEX_PROJECTED_LIGHT,
            0x1 => Self::TEX_PROJECTED_SHADOW,
            0x2 => Self::TEX_ENVIRONMENT_MAP,
            0x3 => Self::TEX_FOG_MAP,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Determines the way that UV texture coordinates are generated.
/// C# `enum CoordGenType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum CoordGenType {
    CG_WORLD_PARALLEL = 0x0,
    CG_WORLD_PERSPECTIVE = 0x1,
    CG_SPHERE_MAP = 0x2,
    CG_SPECULAR_CUBE_MAP = 0x3,
    CG_DIFFUSE_CUBE_MAP = 0x4, // Use diffuse cube mapping. For NiSourceCubeMap only.
}

impl CoordGenType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::CG_WORLD_PARALLEL,
            0x1 => Self::CG_WORLD_PERSPECTIVE,
            0x2 => Self::CG_SPHERE_MAP,
            0x3 => Self::CG_SPECULAR_CUBE_MAP,
            0x4 => Self::CG_DIFFUSE_CUBE_MAP,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// C# `enum EndianType : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum EndianType {
    ENDIAN_BIG = 0x0,
    ENDIAN_LITTLE = 0x1, // The numbers are stored in little endian format, such as those used by Intel and AMD x86 processors.
}

impl EndianType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::ENDIAN_BIG,
            0x1 => Self::ENDIAN_LITTLE,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

/// Used by NiMaterialColorControllers to select which type of color in the controlled object that will be animated.
/// C# `enum MaterialColor : u16`.
#[repr(u16)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum MaterialColor {
    TC_AMBIENT = 0x0,
    TC_DIFFUSE = 0x1,
    TC_SPECULAR = 0x2,
    TC_SELF_ILLUM = 0x3, // Control the self illumination color.
}

impl MaterialColor {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u16) -> Option<Self> {
        Some(match v {
            0x0 => Self::TC_AMBIENT,
            0x1 => Self::TC_DIFFUSE,
            0x2 => Self::TC_SPECULAR,
            0x3 => Self::TC_SELF_ILLUM,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u16 {
        self as u16
    }
}

/// Used by NiGeometryData to control the volatility of the mesh.
/// Consistency Type is masked to only the upper 4 bits (0xF000). Dirty mask is the lower 12 (0x0FFF) but only used at runtime.
/// C# `enum ConsistencyType : u16`.
#[repr(u16)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum ConsistencyType {
    CT_MUTABLE = 0x0,
    CT_STATIC = 0x4000,
    CT_VOLATILE = 0x8000, // Volatile Mesh
}

impl ConsistencyType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u16) -> Option<Self> {
        Some(match v {
            0x0 => Self::CT_MUTABLE,
            0x4000 => Self::CT_STATIC,
            0x8000 => Self::CT_VOLATILE,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u16 {
        self as u16
    }
}

/// C# `enum BoundVolumeType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum BoundVolumeType {
    BASE_BV = 0xffffffff,
    SPHERE_BV = 0x0,
    BOX_BV = 0x1,
    CAPSULE_BV = 0x2,
    UNION_BV = 0x4,
    HALFSPACE_BV = 0x5, // Half Space
}

impl BoundVolumeType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0xffffffff => Self::BASE_BV,
            0x0 => Self::SPHERE_BV,
            0x1 => Self::BOX_BV,
            0x2 => Self::CAPSULE_BV,
            0x4 => Self::UNION_BV,
            0x5 => Self::HALFSPACE_BV,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Bethesda Havok.
/// C# `enum hkResponseType : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum hkResponseType {
    RESPONSE_INVALID = 0x0,
    RESPONSE_SIMPLE_CONTACT = 0x1,
    RESPONSE_REPORTING = 0x2,
    RESPONSE_NONE = 0x3, // Do nothing, ignore all the results.
}

impl hkResponseType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::RESPONSE_INVALID,
            0x1 => Self::RESPONSE_SIMPLE_CONTACT,
            0x2 => Self::RESPONSE_REPORTING,
            0x3 => Self::RESPONSE_NONE,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

/// Values for configuring the shader type in a BSLightingShaderProperty
/// C# `enum BSLightingShaderPropertyShaderType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum BSLightingShaderPropertyShaderType {
    Default = 0x0,
    Environment_Map = 0x1,
    Glow_Shader = 0x2,
    Parallax = 0x3,
    Face_Tint = 0x4,
    Skin_Tint = 0x5,
    Hair_Tint = 0x6,
    Parallax_Occ = 0x7,
    Multitexture_Landscape = 0x8,
    LOD_Landscape = 0x9,
    Snow = 0xa,
    MultiLayer_Parallax = 0xb,
    Tree_Anim = 0xc,
    LOD_Objects = 0xd,
    Sparkle_Snow = 0xe,
    LOD_Objects_HD = 0xf,
    Eye_Envmap = 0x10,
    Cloud = 0x11,
    LOD_Landscape_Noise = 0x12,
    Multitexture_Landscape_LOD_Blend = 0x13,
    FO4_Dismemberment = 0x14,
}

impl BSLightingShaderPropertyShaderType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Default,
            0x1 => Self::Environment_Map,
            0x2 => Self::Glow_Shader,
            0x3 => Self::Parallax,
            0x4 => Self::Face_Tint,
            0x5 => Self::Skin_Tint,
            0x6 => Self::Hair_Tint,
            0x7 => Self::Parallax_Occ,
            0x8 => Self::Multitexture_Landscape,
            0x9 => Self::LOD_Landscape,
            0xa => Self::Snow,
            0xb => Self::MultiLayer_Parallax,
            0xc => Self::Tree_Anim,
            0xd => Self::LOD_Objects,
            0xe => Self::Sparkle_Snow,
            0xf => Self::LOD_Objects_HD,
            0x10 => Self::Eye_Envmap,
            0x11 => Self::Cloud,
            0x12 => Self::LOD_Landscape_Noise,
            0x13 => Self::Multitexture_Landscape_LOD_Blend,
            0x14 => Self::FO4_Dismemberment,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// Describes the order of scaling and rotation matrices. Translate, Scale, Rotation, Center are from TexDesc.
/// Back = inverse of Center. FromMaya = inverse of the V axis with a positive translation along V of 1 unit.
/// C# `enum TransformMethod : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum TransformMethod {
    Maya_Deprecated = 0x0,
    Max = 0x1,
    Maya = 0x2, // Center * Rotation * Back * FromMaya * Translate * Scale
}

impl TransformMethod {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Maya_Deprecated,
            0x1 => Self::Max,
            0x2 => Self::Maya,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum VertexFlags : u16`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct VertexFlags: u16 {
        const Vertex = 0x10;
        const UVs = 0x20;
        const UVs_2 = 0x40;
        const Normals = 0x80;
        const Tangents = 0x100;
        const Vertex_Colors = 0x200;
        const Skinned = 0x400;
        const Land_Data = 0x800;
        const Eye_Data = 0x1000;
        const Instance = 0x2000;
        const Full_Precision = 0x4000;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum FurnitureEntryPoints : u16`.
    /// Bethesda Animation. Furniture entry points. It specifies the direction(s) from where the actor is able to enter (and leave) the position.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct FurnitureEntryPoints: u16 {
        const Front = 0x0;
        const Behind = 0x2;
        const Right = 0x4;
        const Left = 0x8;
        const Up = 0x10; // up entry point - unknown function. Used on some beds in Skyrim, probably for blocking of sleeping position.
    }
}

/// Bethesda Animation. Animation type used on this position. This specifies the function of this position.
/// C# `enum AnimationType : u16`.
#[repr(u16)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum AnimationType {
    Sit = 0x1,
    Sleep = 0x2,
    Lean = 0x4, // Used for lean animations?
}

impl AnimationType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u16) -> Option<Self> {
        Some(match v {
            0x1 => Self::Sit,
            0x2 => Self::Sleep,
            0x4 => Self::Lean,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u16 {
        self as u16
    }
}

/// Determines how the raw image data is stored in NiRawImageData.
/// C# `enum ImageType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum ImageType {
    RGB = 0x1,
    RGBA = 0x2, // Colors store red, blue, green, and alpha components.
}

impl ImageType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x1 => Self::RGB,
            0x2 => Self::RGBA,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// C# `enum BroadPhaseType : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum BroadPhaseType {
    BROAD_PHASE_INVALID = 0x0,
    BROAD_PHASE_ENTITY = 0x1,
    BROAD_PHASE_PHANTOM = 0x2,
    BROAD_PHASE_BORDER = 0x3,
}

impl BroadPhaseType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::BROAD_PHASE_INVALID,
            0x1 => Self::BROAD_PHASE_ENTITY,
            0x2 => Self::BROAD_PHASE_PHANTOM,
            0x3 => Self::BROAD_PHASE_BORDER,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum PathFlags : u16`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct PathFlags: u16 {
        const CVDataNeedsUpdate = 0x0;
        const CurveTypeOpen = 0x2;
        const AllowFlip = 0x4;
        const Bank = 0x8;
        const ConstantVelocity = 0x10;
        const Follow = 0x20;
        const Flip = 0x40;
    }
}

/// C# `enum InterpBlendFlags : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum InterpBlendFlags {
    MANAGER_CONTROLLED = 0x1, // MANAGER_CONTROLLED
}

impl InterpBlendFlags {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x1 => Self::MANAGER_CONTROLLED,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum bhkCOFlags : u16`.
    /// bhkNiCollisionObject flags. The flags 0x2, 0x100, and 0x200 are not seen in any NIF nor get/set by the engine.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct bhkCOFlags: u16 {
        const ACTIVE = 0x0;
        const NOTIFY = 0x4;
        const SET_LOCAL = 0x8;
        const DBG_DISPLAY = 0x10;
        const USE_VEL = 0x20;
        const RESET = 0x40;
        const SYNC_ON_UPDATE = 0x80;
        const ANIM_TARGETED = 0x400;
        const DISMEMBERED_LIMB = 0x800;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum VectorFlags : u16`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct VectorFlags: u16 {
        const UV_1 = 0x0;
        const UV_2 = 0x2;
        const UV_4 = 0x4;
        const UV_8 = 0x8;
        const UV_16 = 0x10;
        const UV_32 = 0x20;
        const Unk64 = 0x40;
        const Unk128 = 0x80;
        const Unk256 = 0x100;
        const Unk512 = 0x200;
        const Unk1024 = 0x400;
        const Unk2048 = 0x800;
        const Has_Tangents = 0x1000;
        const Unk8192 = 0x2000;
        const Unk16384 = 0x4000;
        const Unk32768 = 0x8000;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum BSVectorFlags : u16`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct BSVectorFlags: u16 {
        const Has_UV = 0x0;
        const Unk2 = 0x2;
        const Unk4 = 0x4;
        const Unk8 = 0x8;
        const Unk16 = 0x10;
        const Unk32 = 0x20;
        const Unk64 = 0x40;
        const Unk128 = 0x80;
        const Unk256 = 0x100;
        const Unk512 = 0x200;
        const Unk1024 = 0x400;
        const Unk2048 = 0x800;
        const Has_Tangents = 0x1000;
        const Unk8192 = 0x2000;
        const Unk16384 = 0x4000;
        const Unk32768 = 0x8000;
    }
}

/// The type of animation interpolation (blending) that will be used on the associated key frames.
/// C# `enum BSShaderType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum BSShaderType {
    SHADER_TALL_GRASS = 0x0,
    SHADER_DEFAULT = 0x1,
    SHADER_SKY = 0xa,
    SHADER_SKIN = 0xe,
    SHADER_WATER = 0x11,
    SHADER_LIGHTING30 = 0x1d,
    SHADER_TILE = 0x20,
    SHADER_NOLIGHTING = 0x21, // No Lighting Shader
}

impl BSShaderType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::SHADER_TALL_GRASS,
            0x1 => Self::SHADER_DEFAULT,
            0xa => Self::SHADER_SKY,
            0xe => Self::SHADER_SKIN,
            0x11 => Self::SHADER_WATER,
            0x1d => Self::SHADER_LIGHTING30,
            0x20 => Self::SHADER_TILE,
            0x21 => Self::SHADER_NOLIGHTING,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum BSShaderFlags : u32`.
    /// Shader Property Flags
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct BSShaderFlags: u32 {
        const Specular = 0x0;
        const Skinned = 0x2;
        const LowDetail = 0x4;
        const Vertex_Alpha = 0x8;
        const Unknown_1 = 0x10;
        const Single_Pass = 0x20;
        const Empty = 0x40;
        const Environment_Mapping = 0x80;
        const Alpha_Texture = 0x100;
        const Unknown_2 = 0x200;
        const FaceGen = 0x400;
        const Parallax_Shader_Index_15 = 0x800;
        const Unknown_3 = 0x1000;
        const Non_Projective_Shadows = 0x2000;
        const Unknown_4 = 0x4000;
        const Refraction = 0x8000;
        const Fire_Refraction = 0x10000;
        const Eye_Environment_Mapping = 0x20000;
        const Hair = 0x40000;
        const Dynamic_Alpha = 0x80000;
        const Localmap_Hide_Secret = 0x100000;
        const Window_Environment_Mapping = 0x200000;
        const Tree_Billboard = 0x400000;
        const Shadow_Frustum = 0x800000;
        const Multiple_Textures = 0x1000000;
        const Remappable_Textures = 0x2000000;
        const Decal_Single_Pass = 0x4000000;
        const Dynamic_Decal_Single_Pass = 0x8000000;
        const Parallax_Occulsion = 0x10000000;
        const External_Emittance = 0x20000000;
        const Shadow_Map = 0x40000000;
        const ZBuffer_Test = 0x80000000; // ZBuffer Test (1=on)
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum BSShaderFlags2 : u32`.
    /// Shader Property Flags 2
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct BSShaderFlags2: u32 {
        const ZBuffer_Write = 0x0;
        const LOD_Landscape = 0x2;
        const LOD_Building = 0x4;
        const No_Fade = 0x8;
        const Refraction_Tint = 0x10;
        const Vertex_Colors = 0x20;
        const Unknown1 = 0x40;
        const X1st_Light_is_Point_Light = 0x80;
        const X2nd_Light = 0x100;
        const X3rd_Light = 0x200;
        const Vertex_Lighting = 0x400;
        const Uniform_Scale = 0x800;
        const Fit_Slope = 0x1000;
        const Billboard_and_Envmap_Light_Fade = 0x2000;
        const No_LOD_Land_Blend = 0x4000;
        const Envmap_Light_Fade = 0x8000;
        const Wireframe = 0x10000;
        const VATS_Selection = 0x20000;
        const Show_in_Local_Map = 0x40000;
        const Premult_Alpha = 0x80000;
        const Skip_Normal_Maps = 0x100000;
        const Alpha_Decal = 0x200000;
        const No_Transparecny_Multisampling = 0x400000;
        const Unknown2 = 0x800000;
        const Unknown3 = 0x1000000;
        const Unknown4 = 0x2000000;
        const Unknown5 = 0x4000000;
        const Unknown6 = 0x8000000;
        const Unknown7 = 0x10000000;
        const Unknown8 = 0x20000000;
        const Unknown9 = 0x40000000;
        const Unknown10 = 0x80000000; // Unknown
    }
}

/// Anim note types.
/// C# `enum AnimNoteType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum AnimNoteType {
    ANT_INVALID = 0x0,
    ANT_GRABIK = 0x1,
    ANT_LOOKIK = 0x2, // ANT_LOOKIK
}

impl AnimNoteType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::ANT_INVALID,
            0x1 => Self::ANT_GRABIK,
            0x2 => Self::ANT_LOOKIK,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}
