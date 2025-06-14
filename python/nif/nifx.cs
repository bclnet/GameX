using MathNet.Numerics;
using SharpCompress.Common;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using static OpenStack.Debug;
#pragma warning disable CS9113 // Parameter is unread.

namespace GameX.Bethesda.Formats.Nif;

#region X

static class X<T> {
    public static T Read(BinaryReader r) {
        if (typeof(T) == typeof(float)) { return (T)(object)r.ReadSingle(); }
        else if (typeof(T) == typeof(byte)) { return (T)(object)r.ReadByte(); }
        else if (typeof(T) == typeof(string)) { return (T)(object)r.ReadL32Encoding(); }
        else if (typeof(T) == typeof(Vector3)) { return (T)(object)r.ReadVector3(); }
        else if (typeof(T) == typeof(Quaternion)) { return (T)(object)r.ReadQuaternionWFirst(); }
        else if (typeof(T) == typeof(Color4)) { return (T)(object)new Color4(r); }
        else throw new NotImplementedException("Tried to read an unsupported type.");
    }
    public static string Str(BinaryReader r) => r.ReadL32Encoding();
    // Refers to an object before the current one in the hierarchy.
    public static int? Ptr(BinaryReader r) { int v; return (v = r.ReadInt32()) < 0 ? null : v; } //:M
    // Refers to an object after the current one in the hierarchy.
    public static int? Ref(BinaryReader r) { int v; return (v = r.ReadInt32()) < 0 ? null : v; } //:M
}

static class Y {
    public static string String(BinaryReader r) => r.ReadL32Encoding();
    public static string StringRef(BinaryReader r, int? p) => default;
}

public enum Flags : ushort { }

#endregion

#region Enums

/// <summary>
/// Describes the options for the accum root on NiControllerSequence.
/// </summary>
[Flags]
public enum AccumFlags {
    ACCUM_X_TRANS = 0,
    ACCUM_Y_TRANS = 1,
    ACCUM_Z_TRANS = 2,
    ACCUM_X_ROT = 3,
    ACCUM_Y_ROT = 4,
    ACCUM_Z_ROT = 5,
    ACCUM_X_FRONT = 6,
    ACCUM_Y_FRONT = 7,
    ACCUM_Z_FRONT = 8,
    ACCUM_NEG_FRONT = 9,
}

/// <summary>
/// Describes how the vertex colors are blended with the filtered texture color.
/// </summary>
public enum ApplyMode {
    APPLY_REPLACE = 0,
    APPLY_DECAL = 1,
    APPLY_MODULATE = 2,
    APPLY_HILIGHT = 3,
    APPLY_HILIGHT2 = 4,
}

/// <summary>
/// The type of texture.
/// </summary>
public enum TexType {
    BASE_MAP = 0,
    DARK_MAP = 1,
    DETAIL_MAP = 2,
    GLOSS_MAP = 3,
    GLOW_MAP = 4,
    BUMP_MAP = 5,
    NORMAL_MAP = 6,
    PARALLAX_MAP = 7,
    DECAL_0_MAP = 8,
    DECAL_1_MAP = 9,
    DECAL_2_MAP = 10,
    DECAL_3_MAP = 11,
}

/// <summary>
/// The type of animation interpolation (blending) that will be used on the associated key frames.
/// </summary>
public enum KeyType {
    LINEAR_KEY = 1,
    QUADRATIC_KEY = 2,
    TBC_KEY = 3,
    XYZ_ROTATION_KEY = 4,
    CONST_KEY = 5,
}

/// <summary>
/// Bethesda Havok. Material descriptor for a Havok shape in Oblivion.
/// </summary>
public enum OblivionHavokMaterial {
    OB_HAV_MAT_STONE = 0,
    OB_HAV_MAT_CLOTH = 1,
    OB_HAV_MAT_DIRT = 2,
    OB_HAV_MAT_GLASS = 3,
    OB_HAV_MAT_GRASS = 4,
    OB_HAV_MAT_METAL = 5,
    OB_HAV_MAT_ORGANIC = 6,
    OB_HAV_MAT_SKIN = 7,
    OB_HAV_MAT_WATER = 8,
    OB_HAV_MAT_WOOD = 9,
    OB_HAV_MAT_HEAVY_STONE = 10,
    OB_HAV_MAT_HEAVY_METAL = 11,
    OB_HAV_MAT_HEAVY_WOOD = 12,
    OB_HAV_MAT_CHAIN = 13,
    OB_HAV_MAT_SNOW = 14,
    OB_HAV_MAT_STONE_STAIRS = 15,
    OB_HAV_MAT_CLOTH_STAIRS = 16,
    OB_HAV_MAT_DIRT_STAIRS = 17,
    OB_HAV_MAT_GLASS_STAIRS = 18,
    OB_HAV_MAT_GRASS_STAIRS = 19,
    OB_HAV_MAT_METAL_STAIRS = 20,
    OB_HAV_MAT_ORGANIC_STAIRS = 21,
    OB_HAV_MAT_SKIN_STAIRS = 22,
    OB_HAV_MAT_WATER_STAIRS = 23,
    OB_HAV_MAT_WOOD_STAIRS = 24,
    OB_HAV_MAT_HEAVY_STONE_STAIRS = 25,
    OB_HAV_MAT_HEAVY_METAL_STAIRS = 26,
    OB_HAV_MAT_HEAVY_WOOD_STAIRS = 27,
    OB_HAV_MAT_CHAIN_STAIRS = 28,
    OB_HAV_MAT_SNOW_STAIRS = 29,
    OB_HAV_MAT_ELEVATOR = 30,
    OB_HAV_MAT_RUBBER = 31,
}

/// <summary>
/// Bethesda Havok. Material descriptor for a Havok shape in Fallout 3 and Fallout NV.
/// </summary>
public enum Fallout3HavokMaterial {
    FO_HAV_MAT_STONE = 0,
    FO_HAV_MAT_CLOTH = 1,
    FO_HAV_MAT_DIRT = 2,
    FO_HAV_MAT_GLASS = 3,
    FO_HAV_MAT_GRASS = 4,
    FO_HAV_MAT_METAL = 5,
    FO_HAV_MAT_ORGANIC = 6,
    FO_HAV_MAT_SKIN = 7,
    FO_HAV_MAT_WATER = 8,
    FO_HAV_MAT_WOOD = 9,
    FO_HAV_MAT_HEAVY_STONE = 10,
    FO_HAV_MAT_HEAVY_METAL = 11,
    FO_HAV_MAT_HEAVY_WOOD = 12,
    FO_HAV_MAT_CHAIN = 13,
    FO_HAV_MAT_BOTTLECAP = 14,
    FO_HAV_MAT_ELEVATOR = 15,
    FO_HAV_MAT_HOLLOW_METAL = 16,
    FO_HAV_MAT_SHEET_METAL = 17,
    FO_HAV_MAT_SAND = 18,
    FO_HAV_MAT_BROKEN_CONCRETE = 19,
    FO_HAV_MAT_VEHICLE_BODY = 20,
    FO_HAV_MAT_VEHICLE_PART_SOLID = 21,
    FO_HAV_MAT_VEHICLE_PART_HOLLOW = 22,
    FO_HAV_MAT_BARREL = 23,
    FO_HAV_MAT_BOTTLE = 24,
    FO_HAV_MAT_SODA_CAN = 25,
    FO_HAV_MAT_PISTOL = 26,
    FO_HAV_MAT_RIFLE = 27,
    FO_HAV_MAT_SHOPPING_CART = 28,
    FO_HAV_MAT_LUNCHBOX = 29,
    FO_HAV_MAT_BABY_RATTLE = 30,
    FO_HAV_MAT_RUBBER_BALL = 31,
    FO_HAV_MAT_STONE_PLATFORM = 32,
    FO_HAV_MAT_CLOTH_PLATFORM = 33,
    FO_HAV_MAT_DIRT_PLATFORM = 34,
    FO_HAV_MAT_GLASS_PLATFORM = 35,
    FO_HAV_MAT_GRASS_PLATFORM = 36,
    FO_HAV_MAT_METAL_PLATFORM = 37,
    FO_HAV_MAT_ORGANIC_PLATFORM = 38,
    FO_HAV_MAT_SKIN_PLATFORM = 39,
    FO_HAV_MAT_WATER_PLATFORM = 40,
    FO_HAV_MAT_WOOD_PLATFORM = 41,
    FO_HAV_MAT_HEAVY_STONE_PLATFORM = 42,
    FO_HAV_MAT_HEAVY_METAL_PLATFORM = 43,
    FO_HAV_MAT_HEAVY_WOOD_PLATFORM = 44,
    FO_HAV_MAT_CHAIN_PLATFORM = 45,
    FO_HAV_MAT_BOTTLECAP_PLATFORM = 46,
    FO_HAV_MAT_ELEVATOR_PLATFORM = 47,
    FO_HAV_MAT_HOLLOW_METAL_PLATFORM = 48,
    FO_HAV_MAT_SHEET_METAL_PLATFORM = 49,
    FO_HAV_MAT_SAND_PLATFORM = 50,
    FO_HAV_MAT_BROKEN_CONCRETE_PLATFORM = 51,
    FO_HAV_MAT_VEHICLE_BODY_PLATFORM = 52,
    FO_HAV_MAT_VEHICLE_PART_SOLID_PLATFORM = 53,
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_PLATFORM = 54,
    FO_HAV_MAT_BARREL_PLATFORM = 55,
    FO_HAV_MAT_BOTTLE_PLATFORM = 56,
    FO_HAV_MAT_SODA_CAN_PLATFORM = 57,
    FO_HAV_MAT_PISTOL_PLATFORM = 58,
    FO_HAV_MAT_RIFLE_PLATFORM = 59,
    FO_HAV_MAT_SHOPPING_CART_PLATFORM = 60,
    FO_HAV_MAT_LUNCHBOX_PLATFORM = 61,
    FO_HAV_MAT_BABY_RATTLE_PLATFORM = 62,
    FO_HAV_MAT_RUBBER_BALL_PLATFORM = 63,
    FO_HAV_MAT_STONE_STAIRS = 64,
    FO_HAV_MAT_CLOTH_STAIRS = 65,
    FO_HAV_MAT_DIRT_STAIRS = 66,
    FO_HAV_MAT_GLASS_STAIRS = 67,
    FO_HAV_MAT_GRASS_STAIRS = 68,
    FO_HAV_MAT_METAL_STAIRS = 69,
    FO_HAV_MAT_ORGANIC_STAIRS = 70,
    FO_HAV_MAT_SKIN_STAIRS = 71,
    FO_HAV_MAT_WATER_STAIRS = 72,
    FO_HAV_MAT_WOOD_STAIRS = 73,
    FO_HAV_MAT_HEAVY_STONE_STAIRS = 74,
    FO_HAV_MAT_HEAVY_METAL_STAIRS = 75,
    FO_HAV_MAT_HEAVY_WOOD_STAIRS = 76,
    FO_HAV_MAT_CHAIN_STAIRS = 77,
    FO_HAV_MAT_BOTTLECAP_STAIRS = 78,
    FO_HAV_MAT_ELEVATOR_STAIRS = 79,
    FO_HAV_MAT_HOLLOW_METAL_STAIRS = 80,
    FO_HAV_MAT_SHEET_METAL_STAIRS = 81,
    FO_HAV_MAT_SAND_STAIRS = 82,
    FO_HAV_MAT_BROKEN_CONCRETE_STAIRS = 83,
    FO_HAV_MAT_VEHICLE_BODY_STAIRS = 84,
    FO_HAV_MAT_VEHICLE_PART_SOLID_STAIRS = 85,
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_STAIRS = 86,
    FO_HAV_MAT_BARREL_STAIRS = 87,
    FO_HAV_MAT_BOTTLE_STAIRS = 88,
    FO_HAV_MAT_SODA_CAN_STAIRS = 89,
    FO_HAV_MAT_PISTOL_STAIRS = 90,
    FO_HAV_MAT_RIFLE_STAIRS = 91,
    FO_HAV_MAT_SHOPPING_CART_STAIRS = 92,
    FO_HAV_MAT_LUNCHBOX_STAIRS = 93,
    FO_HAV_MAT_BABY_RATTLE_STAIRS = 94,
    FO_HAV_MAT_RUBBER_BALL_STAIRS = 95,
    FO_HAV_MAT_STONE_STAIRS_PLATFORM = 96,
    FO_HAV_MAT_CLOTH_STAIRS_PLATFORM = 97,
    FO_HAV_MAT_DIRT_STAIRS_PLATFORM = 98,
    FO_HAV_MAT_GLASS_STAIRS_PLATFORM = 99,
    FO_HAV_MAT_GRASS_STAIRS_PLATFORM = 100,
    FO_HAV_MAT_METAL_STAIRS_PLATFORM = 101,
    FO_HAV_MAT_ORGANIC_STAIRS_PLATFORM = 102,
    FO_HAV_MAT_SKIN_STAIRS_PLATFORM = 103,
    FO_HAV_MAT_WATER_STAIRS_PLATFORM = 104,
    FO_HAV_MAT_WOOD_STAIRS_PLATFORM = 105,
    FO_HAV_MAT_HEAVY_STONE_STAIRS_PLATFORM = 106,
    FO_HAV_MAT_HEAVY_METAL_STAIRS_PLATFORM = 107,
    FO_HAV_MAT_HEAVY_WOOD_STAIRS_PLATFORM = 108,
    FO_HAV_MAT_CHAIN_STAIRS_PLATFORM = 109,
    FO_HAV_MAT_BOTTLECAP_STAIRS_PLATFORM = 110,
    FO_HAV_MAT_ELEVATOR_STAIRS_PLATFORM = 111,
    FO_HAV_MAT_HOLLOW_METAL_STAIRS_PLATFORM = 112,
    FO_HAV_MAT_SHEET_METAL_STAIRS_PLATFORM = 113,
    FO_HAV_MAT_SAND_STAIRS_PLATFORM = 114,
    FO_HAV_MAT_BROKEN_CONCRETE_STAIRS_PLATFORM = 115,
    FO_HAV_MAT_VEHICLE_BODY_STAIRS_PLATFORM = 116,
    FO_HAV_MAT_VEHICLE_PART_SOLID_STAIRS_PLATFORM = 117,
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_STAIRS_PLATFORM = 118,
    FO_HAV_MAT_BARREL_STAIRS_PLATFORM = 119,
    FO_HAV_MAT_BOTTLE_STAIRS_PLATFORM = 120,
    FO_HAV_MAT_SODA_CAN_STAIRS_PLATFORM = 121,
    FO_HAV_MAT_PISTOL_STAIRS_PLATFORM = 122,
    FO_HAV_MAT_RIFLE_STAIRS_PLATFORM = 123,
    FO_HAV_MAT_SHOPPING_CART_STAIRS_PLATFORM = 124,
    FO_HAV_MAT_LUNCHBOX_STAIRS_PLATFORM = 125,
    FO_HAV_MAT_BABY_RATTLE_STAIRS_PLATFORM = 126,
    FO_HAV_MAT_RUBBER_BALL_STAIRS_PLATFORM = 127,
}

/// <summary>
/// Bethesda Havok. Material descriptor for a Havok shape in Skyrim.
/// </summary>
public enum SkyrimHavokMaterial {
    SKY_HAV_MAT_BROKEN_STONE = 131151687,
    SKY_HAV_MAT_LIGHT_WOOD = 365420259,
    SKY_HAV_MAT_SNOW = 398949039,
    SKY_HAV_MAT_GRAVEL = 428587608,
    SKY_HAV_MAT_MATERIAL_CHAIN_METAL = 438912228,
    SKY_HAV_MAT_BOTTLE = 493553910,
    SKY_HAV_MAT_WOOD = 500811281,
    SKY_HAV_MAT_SKIN = 591247106,
    SKY_HAV_MAT_UNKNOWN_617099282 = 617099282,
    SKY_HAV_MAT_BARREL = 732141076,
    SKY_HAV_MAT_MATERIAL_CERAMIC_MEDIUM = 781661019,
    SKY_HAV_MAT_MATERIAL_BASKET = 790784366,
    SKY_HAV_MAT_ICE = 873356572,
    SKY_HAV_MAT_STAIRS_STONE = 899511101,
    SKY_HAV_MAT_WATER = 1024582599,
    SKY_HAV_MAT_UNKNOWN_1028101969 = 1028101969,
    SKY_HAV_MAT_MATERIAL_BLADE_1HAND = 1060167844,
    SKY_HAV_MAT_MATERIAL_BOOK = 1264672850,
    SKY_HAV_MAT_MATERIAL_CARPET = 1286705471,
    SKY_HAV_MAT_SOLID_METAL = 1288358971,
    SKY_HAV_MAT_MATERIAL_AXE_1HAND = 1305674443,
    SKY_HAV_MAT_UNKNOWN_1440721808 = 1440721808,
    SKY_HAV_MAT_STAIRS_WOOD = 1461712277,
    SKY_HAV_MAT_MUD = 1486385281,
    SKY_HAV_MAT_MATERIAL_BOULDER_SMALL = 1550912982,
    SKY_HAV_MAT_STAIRS_SNOW = 1560365355,
    SKY_HAV_MAT_HEAVY_STONE = 1570821952,
    SKY_HAV_MAT_UNKNOWN_1574477864 = 1574477864,
    SKY_HAV_MAT_UNKNOWN_1591009235 = 1591009235,
    SKY_HAV_MAT_MATERIAL_BOWS_STAVES = 1607128641,
    SKY_HAV_MAT_MATERIAL_WOOD_AS_STAIRS = 1803571212,
    SKY_HAV_MAT_GRASS = 1848600814,
    SKY_HAV_MAT_MATERIAL_BOULDER_LARGE = 1885326971,
    SKY_HAV_MAT_MATERIAL_STONE_AS_STAIRS = 1886078335,
    SKY_HAV_MAT_MATERIAL_BLADE_2HAND = 2022742644,
    SKY_HAV_MAT_MATERIAL_BOTTLE_SMALL = 2025794648,
    SKY_HAV_MAT_SAND = 2168343821,
    SKY_HAV_MAT_HEAVY_METAL = 2229413539,
    SKY_HAV_MAT_UNKNOWN_2290050264 = 2290050264,
    SKY_HAV_MAT_DRAGON = 2518321175,
    SKY_HAV_MAT_MATERIAL_BLADE_1HAND_SMALL = 2617944780,
    SKY_HAV_MAT_MATERIAL_SKIN_SMALL = 2632367422,
    SKY_HAV_MAT_STAIRS_BROKEN_STONE = 2892392795,
    SKY_HAV_MAT_MATERIAL_SKIN_LARGE = 2965929619,
    SKY_HAV_MAT_ORGANIC = 2974920155,
    SKY_HAV_MAT_MATERIAL_BONE = 3049421844,
    SKY_HAV_MAT_HEAVY_WOOD = 3070783559,
    SKY_HAV_MAT_MATERIAL_CHAIN = 3074114406,
    SKY_HAV_MAT_DIRT = 3106094762,
    SKY_HAV_MAT_MATERIAL_ARMOR_LIGHT = 3424720541,
    SKY_HAV_MAT_MATERIAL_SHIELD_LIGHT = 3448167928,
    SKY_HAV_MAT_MATERIAL_COIN = 3589100606,
    SKY_HAV_MAT_MATERIAL_SHIELD_HEAVY = 3702389584,
    SKY_HAV_MAT_MATERIAL_ARMOR_HEAVY = 3708432437,
    SKY_HAV_MAT_MATERIAL_ARROW = 3725505938,
    SKY_HAV_MAT_GLASS = 3739830338,
    SKY_HAV_MAT_STONE = 3741512247,
    SKY_HAV_MAT_CLOTH = 3839073443,
    SKY_HAV_MAT_MATERIAL_BLUNT_2HAND = 3969592277,
    SKY_HAV_MAT_UNKNOWN_4239621792 = 4239621792,
    SKY_HAV_MAT_MATERIAL_BOULDER_MEDIUM = 4283869410,
}

/// <summary>
/// Bethesda Havok. Describes the collision layer a body belongs to in Oblivion.
/// </summary>
public enum OblivionLayer {
    OL_UNIDENTIFIED = 0,
    OL_STATIC = 1,
    OL_ANIM_STATIC = 2,
    OL_TRANSPARENT = 3,
    OL_CLUTTER = 4,
    OL_WEAPON = 5,
    OL_PROJECTILE = 6,
    OL_SPELL = 7,
    OL_BIPED = 8,
    OL_TREES = 9,
    OL_PROPS = 10,
    OL_WATER = 11,
    OL_TRIGGER = 12,
    OL_TERRAIN = 13,
    OL_TRAP = 14,
    OL_NONCOLLIDABLE = 15,
    OL_CLOUD_TRAP = 16,
    OL_GROUND = 17,
    OL_PORTAL = 18,
    OL_STAIRS = 19,
    OL_CHAR_CONTROLLER = 20,
    OL_AVOID_BOX = 21,
    OL_UNKNOWN1 = 22,
    OL_UNKNOWN2 = 23,
    OL_CAMERA_PICK = 24,
    OL_ITEM_PICK = 25,
    OL_LINE_OF_SIGHT = 26,
    OL_PATH_PICK = 27,
    OL_CUSTOM_PICK_1 = 28,
    OL_CUSTOM_PICK_2 = 29,
    OL_SPELL_EXPLOSION = 30,
    OL_DROPPING_PICK = 31,
    OL_OTHER = 32,
    OL_HEAD = 33,
    OL_BODY = 34,
    OL_SPINE1 = 35,
    OL_SPINE2 = 36,
    OL_L_UPPER_ARM = 37,
    OL_L_FOREARM = 38,
    OL_L_HAND = 39,
    OL_L_THIGH = 40,
    OL_L_CALF = 41,
    OL_L_FOOT = 42,
    OL_R_UPPER_ARM = 43,
    OL_R_FOREARM = 44,
    OL_R_HAND = 45,
    OL_R_THIGH = 46,
    OL_R_CALF = 47,
    OL_R_FOOT = 48,
    OL_TAIL = 49,
    OL_SIDE_WEAPON = 50,
    OL_SHIELD = 51,
    OL_QUIVER = 52,
    OL_BACK_WEAPON = 53,
    OL_BACK_WEAPON2 = 54,
    OL_PONYTAIL = 55,
    OL_WING = 56,
    OL_NULL = 57,
}

/// <summary>
/// Bethesda Havok. Describes the collision layer a body belongs to in Fallout 3 and Fallout NV.
/// </summary>
public enum Fallout3Layer {
    FOL_UNIDENTIFIED = 0,
    FOL_STATIC = 1,
    FOL_ANIM_STATIC = 2,
    FOL_TRANSPARENT = 3,
    FOL_CLUTTER = 4,
    FOL_WEAPON = 5,
    FOL_PROJECTILE = 6,
    FOL_SPELL = 7,
    FOL_BIPED = 8,
    FOL_TREES = 9,
    FOL_PROPS = 10,
    FOL_WATER = 11,
    FOL_TRIGGER = 12,
    FOL_TERRAIN = 13,
    FOL_TRAP = 14,
    FOL_NONCOLLIDABLE = 15,
    FOL_CLOUD_TRAP = 16,
    FOL_GROUND = 17,
    FOL_PORTAL = 18,
    FOL_DEBRIS_SMALL = 19,
    FOL_DEBRIS_LARGE = 20,
    FOL_ACOUSTIC_SPACE = 21,
    FOL_ACTORZONE = 22,
    FOL_PROJECTILEZONE = 23,
    FOL_GASTRAP = 24,
    FOL_SHELLCASING = 25,
    FOL_TRANSPARENT_SMALL = 26,
    FOL_INVISIBLE_WALL = 27,
    FOL_TRANSPARENT_SMALL_ANIM = 28,
    FOL_DEADBIP = 29,
    FOL_CHARCONTROLLER = 30,
    FOL_AVOIDBOX = 31,
    FOL_COLLISIONBOX = 32,
    FOL_CAMERASPHERE = 33,
    FOL_DOORDETECTION = 34,
    FOL_CAMERAPICK = 35,
    FOL_ITEMPICK = 36,
    FOL_LINEOFSIGHT = 37,
    FOL_PATHPICK = 38,
    FOL_CUSTOMPICK1 = 39,
    FOL_CUSTOMPICK2 = 40,
    FOL_SPELLEXPLOSION = 41,
    FOL_DROPPINGPICK = 42,
    FOL_NULL = 43,
}

/// <summary>
/// Bethesda Havok. Describes the collision layer a body belongs to in Skyrim.
/// </summary>
public enum SkyrimLayer {
    SKYL_UNIDENTIFIED = 0,
    SKYL_STATIC = 1,
    SKYL_ANIMSTATIC = 2,
    SKYL_TRANSPARENT = 3,
    SKYL_CLUTTER = 4,
    SKYL_WEAPON = 5,
    SKYL_PROJECTILE = 6,
    SKYL_SPELL = 7,
    SKYL_BIPED = 8,
    SKYL_TREES = 9,
    SKYL_PROPS = 10,
    SKYL_WATER = 11,
    SKYL_TRIGGER = 12,
    SKYL_TERRAIN = 13,
    SKYL_TRAP = 14,
    SKYL_NONCOLLIDABLE = 15,
    SKYL_CLOUD_TRAP = 16,
    SKYL_GROUND = 17,
    SKYL_PORTAL = 18,
    SKYL_DEBRIS_SMALL = 19,
    SKYL_DEBRIS_LARGE = 20,
    SKYL_ACOUSTIC_SPACE = 21,
    SKYL_ACTORZONE = 22,
    SKYL_PROJECTILEZONE = 23,
    SKYL_GASTRAP = 24,
    SKYL_SHELLCASING = 25,
    SKYL_TRANSPARENT_SMALL = 26,
    SKYL_INVISIBLE_WALL = 27,
    SKYL_TRANSPARENT_SMALL_ANIM = 28,
    SKYL_WARD = 29,
    SKYL_CHARCONTROLLER = 30,
    SKYL_STAIRHELPER = 31,
    SKYL_DEADBIP = 32,
    SKYL_BIPED_NO_CC = 33,
    SKYL_AVOIDBOX = 34,
    SKYL_COLLISIONBOX = 35,
    SKYL_CAMERASHPERE = 36,
    SKYL_DOORDETECTION = 37,
    SKYL_CONEPROJECTILE = 38,
    SKYL_CAMERAPICK = 39,
    SKYL_ITEMPICK = 40,
    SKYL_LINEOFSIGHT = 41,
    SKYL_PATHPICK = 42,
    SKYL_CUSTOMPICK1 = 43,
    SKYL_CUSTOMPICK2 = 44,
    SKYL_SPELLEXPLOSION = 45,
    SKYL_DROPPINGPICK = 46,
    SKYL_NULL = 47,
}

/// <summary>
/// Bethesda Havok.
/// A byte describing if MOPP Data is organized into chunks (PS3) or not (PC)
/// </summary>
public enum MoppDataBuildType {
    BUILT_WITH_CHUNK_SUBDIVISION = 0,
    BUILT_WITHOUT_CHUNK_SUBDIVISION = 1,
    BUILD_NOT_SET = 2,
}

/// <summary>
/// Target platform for NiPersistentSrcTextureRendererData (later than 30.1).
/// </summary>
public enum PlatformID {
    ANY = 0,
    XENON = 1,
    PS3 = 2,
    DX9 = 3,
    WII = 4,
    D3D10 = 5,
}

/// <summary>
/// Target renderer for NiPersistentSrcTextureRendererData (until 30.1).
/// </summary>
public enum RendererID {
    XBOX360 = 0,
    PS3 = 1,
    DX9 = 2,
    D3D10 = 3,
    WII = 4,
    GENERIC = 5,
    D3D11 = 6,
}

/// <summary>
/// Describes the pixel format used by the NiPixelData object to store a texture.
/// </summary>
public enum PixelFormat {
    FMT_RGB = 0,
    FMT_RGBA = 1,
    FMT_PAL = 2,
    FMT_PALA = 3,
    FMT_DXT1 = 4,
    FMT_DXT3 = 5,
    FMT_DXT5 = 6,
    FMT_RGB24NONINT = 7,
    FMT_BUMP = 8,
    FMT_BUMPLUMA = 9,
    FMT_RENDERSPEC = 10,
    FMT_1CH = 11,
    FMT_2CH = 12,
    FMT_3CH = 13,
    FMT_4CH = 14,
    FMT_DEPTH_STENCIL = 15,
    FMT_UNKNOWN = 16,
}

/// <summary>
/// Describes whether pixels have been tiled from their standard row-major format to a format optimized for a particular platform.
/// </summary>
public enum PixelTiling {
    TILE_NONE = 0,
    TILE_XENON = 1,
    TILE_WII = 2,
    TILE_NV_SWIZZLED = 3,
}

/// <summary>
/// Describes the pixel format used by the NiPixelData object to store a texture.
/// </summary>
public enum PixelComponent {
    COMP_RED = 0,
    COMP_GREEN = 1,
    COMP_BLUE = 2,
    COMP_ALPHA = 3,
    COMP_COMPRESSED = 4,
    COMP_OFFSET_U = 5,
    COMP_OFFSET_V = 6,
    COMP_OFFSET_W = 7,
    COMP_OFFSET_Q = 8,
    COMP_LUMA = 9,
    COMP_HEIGHT = 10,
    COMP_VECTOR_X = 11,
    COMP_VECTOR_Y = 12,
    COMP_VECTOR_Z = 13,
    COMP_PADDING = 14,
    COMP_INTENSITY = 15,
    COMP_INDEX = 16,
    COMP_DEPTH = 17,
    COMP_STENCIL = 18,
    COMP_EMPTY = 19,
}

/// <summary>
/// Describes how each pixel should be accessed on NiPixelFormat.
/// </summary>
public enum PixelRepresentation {
    REP_NORM_INT = 0,
    REP_HALF = 1,
    REP_FLOAT = 2,
    REP_INDEX = 3,
    REP_COMPRESSED = 4,
    REP_UNKNOWN = 5,
    REP_INT = 6,
}

/// <summary>
/// Describes the color depth in an NiTexture.
/// </summary>
public enum PixelLayout {
    LAY_PALETTIZED_8 = 0,
    LAY_HIGH_COLOR_16 = 1,
    LAY_TRUE_COLOR_32 = 2,
    LAY_COMPRESSED = 3,
    LAY_BUMPMAP = 4,
    LAY_PALETTIZED_4 = 5,
    LAY_DEFAULT = 6,
    LAY_SINGLE_COLOR_8 = 7,
    LAY_SINGLE_COLOR_16 = 8,
    LAY_SINGLE_COLOR_32 = 9,
    LAY_DOUBLE_COLOR_32 = 10,
    LAY_DOUBLE_COLOR_64 = 11,
    LAY_FLOAT_COLOR_32 = 12,
    LAY_FLOAT_COLOR_64 = 13,
    LAY_FLOAT_COLOR_128 = 14,
    LAY_SINGLE_COLOR_4 = 15,
    LAY_DEPTH_24_X8 = 16,
}

/// <summary>
/// Describes how mipmaps are handled in an NiTexture.
/// </summary>
public enum MipMapFormat {
    MIP_FMT_NO = 0,
    MIP_FMT_YES = 1,
    MIP_FMT_DEFAULT = 2,
}

/// <summary>
/// Describes how transparency is handled in an NiTexture.
/// </summary>
public enum AlphaFormat {
    ALPHA_NONE = 0,
    ALPHA_BINARY = 1,
    ALPHA_SMOOTH = 2,
    ALPHA_DEFAULT = 3,
}

/// <summary>
/// Describes the availiable texture clamp modes, i.e. the behavior of UV mapping outside the [0,1] range.
/// </summary>
public enum TexClampMode {
    CLAMP_S_CLAMP_T = 0,
    CLAMP_S_WRAP_T = 1,
    WRAP_S_CLAMP_T = 2,
    WRAP_S_WRAP_T = 3,
}

/// <summary>
/// Describes the availiable texture filter modes, i.e. the way the pixels in a texture are displayed on screen.
/// </summary>
public enum TexFilterMode {
    FILTER_NEAREST = 0,
    FILTER_BILERP = 1,
    FILTER_TRILERP = 2,
    FILTER_NEAREST_MIPNEAREST = 3,
    FILTER_NEAREST_MIPLERP = 4,
    FILTER_BILERP_MIPNEAREST = 5,
    FILTER_ANISOTROPIC = 6,
}

/// <summary>
/// Describes how to apply vertex colors for NiVertexColorProperty.
/// </summary>
public enum VertMode {
    VERT_MODE_SRC_IGNORE = 0,
    VERT_MODE_SRC_EMISSIVE = 1,
    VERT_MODE_SRC_AMB_DIF = 2,
}

/// <summary>
/// Describes which lighting equation components influence the final vertex color for NiVertexColorProperty.
/// </summary>
public enum LightMode {
    LIGHT_MODE_EMISSIVE = 0,
    LIGHT_MODE_EMI_AMB_DIF = 1,
}

/// <summary>
/// The animation cyle behavior.
/// </summary>
public enum CycleType {
    CYCLE_LOOP = 0,
    CYCLE_REVERSE = 1,
    CYCLE_CLAMP = 2,
}

/// <summary>
/// The force field type.
/// </summary>
public enum FieldType {
    FIELD_WIND = 0,
    FIELD_POINT = 1,
}

/// <summary>
/// Determines the way the billboard will react to the camera.
/// Billboard mode is stored in lowest 3 bits although Oblivion vanilla nifs uses values higher than 7.
/// </summary>
public enum BillboardMode {
    ALWAYS_FACE_CAMERA = 0,
    ROTATE_ABOUT_UP = 1,
    RIGID_FACE_CAMERA = 2,
    ALWAYS_FACE_CENTER = 3,
    RIGID_FACE_CENTER = 4,
    BSROTATE_ABOUT_UP = 5,
    ROTATE_ABOUT_UP2 = 9,
}

/// <summary>
/// Describes stencil buffer test modes for NiStencilProperty.
/// </summary>
public enum StencilCompareMode {
    TEST_NEVER = 0,
    TEST_LESS = 1,
    TEST_EQUAL = 2,
    TEST_LESS_EQUAL = 3,
    TEST_GREATER = 4,
    TEST_NOT_EQUAL = 5,
    TEST_GREATER_EQUAL = 6,
    TEST_ALWAYS = 7,
}

/// <summary>
/// Describes the actions which can occur as a result of tests for NiStencilProperty.
/// </summary>
public enum StencilAction {
    ACTION_KEEP = 0,
    ACTION_ZERO = 1,
    ACTION_REPLACE = 2,
    ACTION_INCREMENT = 3,
    ACTION_DECREMENT = 4,
    ACTION_INVERT = 5,
}

/// <summary>
/// Describes the face culling options for NiStencilProperty.
/// </summary>
public enum StencilDrawMode {
    DRAW_CCW_OR_BOTH = 0,
    DRAW_CCW = 1,
    DRAW_CW = 2,
    DRAW_BOTH = 3,
}

/// <summary>
/// Describes Z-buffer test modes for NiZBufferProperty.
/// "Less than" = closer to camera, "Greater than" = further from camera.
/// </summary>
public enum ZCompareMode {
    ZCOMP_ALWAYS = 0,
    ZCOMP_LESS = 1,
    ZCOMP_EQUAL = 2,
    ZCOMP_LESS_EQUAL = 3,
    ZCOMP_GREATER = 4,
    ZCOMP_NOT_EQUAL = 5,
    ZCOMP_GREATER_EQUAL = 6,
    ZCOMP_NEVER = 7,
}

/// <summary>
/// Bethesda Havok, based on hkpMotion::MotionType. Motion type of a rigid body determines what happens when it is simulated.
/// </summary>
public enum hkMotionType {
    MO_SYS_INVALID = 0,
    MO_SYS_DYNAMIC = 1,
    MO_SYS_SPHERE_INERTIA = 2,
    MO_SYS_SPHERE_STABILIZED = 3,
    MO_SYS_BOX_INERTIA = 4,
    MO_SYS_BOX_STABILIZED = 5,
    MO_SYS_KEYFRAMED = 6,
    MO_SYS_FIXED = 7,
    MO_SYS_THIN_BOX = 8,
    MO_SYS_CHARACTER = 9,
}

/// <summary>
/// Bethesda Havok, based on hkpRigidBodyDeactivator::DeactivatorType.
/// Deactivator Type determines which mechanism Havok will use to classify the body as deactivated.
/// </summary>
public enum hkDeactivatorType {
    DEACTIVATOR_INVALID = 0,
    DEACTIVATOR_NEVER = 1,
    DEACTIVATOR_SPATIAL = 2,
}

/// <summary>
/// Bethesda Havok, based on hkpRigidBodyCinfo::SolverDeactivation.
/// A list of possible solver deactivation settings. This value defines how aggressively the solver deactivates objects.
/// Note: Solver deactivation does not save CPU, but reduces creeping of movable objects in a pile quite dramatically.
/// </summary>
public enum hkSolverDeactivation {
    SOLVER_DEACTIVATION_INVALID = 0,
    SOLVER_DEACTIVATION_OFF = 1,
    SOLVER_DEACTIVATION_LOW = 2,
    SOLVER_DEACTIVATION_MEDIUM = 3,
    SOLVER_DEACTIVATION_HIGH = 4,
    SOLVER_DEACTIVATION_MAX = 5,
}

/// <summary>
/// Bethesda Havok, based on hkpCollidableQualityType. Describes the priority and quality of collisions for a body,
///     e.g. you may expect critical game play objects to have solid high-priority collisions so that they never sink into ground,
///     or may allow penetrations for visual debris objects.
/// Notes:
///     - Fixed and keyframed objects cannot interact with each other.
///     - Debris can interpenetrate but still responds to Bullet hits.
///     - Critical objects are forced to not interpenetrate.
///     - Moving objects can interpenetrate slightly with other Moving or Debris objects but nothing else.
/// </summary>
public enum hkQualityType {
    MO_QUAL_INVALID = 0,
    MO_QUAL_FIXED = 1,
    MO_QUAL_KEYFRAMED = 2,
    MO_QUAL_DEBRIS = 3,
    MO_QUAL_MOVING = 4,
    MO_QUAL_CRITICAL = 5,
    MO_QUAL_BULLET = 6,
    MO_QUAL_USER = 7,
    MO_QUAL_CHARACTER = 8,
    MO_QUAL_KEYFRAMED_REPORT = 9,
}

/// <summary>
/// Describes the type of gravitational force.
/// </summary>
public enum ForceType {
    FORCE_PLANAR = 0,
    FORCE_SPHERICAL = 1,
    FORCE_UNKNOWN = 2,
}

/// <summary>
/// Describes which aspect of the NiTextureTransform the NiTextureTransformController will modify.
/// </summary>
public enum TransformMember {
    TT_TRANSLATE_U = 0,
    TT_TRANSLATE_V = 1,
    TT_ROTATE = 2,
    TT_SCALE_U = 3,
    TT_SCALE_V = 4,
}

/// <summary>
/// Describes the decay function of bomb forces.
/// </summary>
public enum DecayType {
    DECAY_NONE = 0,
    DECAY_LINEAR = 1,
    DECAY_EXPONENTIAL = 2,
}

/// <summary>
/// Describes the symmetry type of bomb forces.
/// </summary>
public enum SymmetryType {
    SPHERICAL_SYMMETRY = 0,
    CYLINDRICAL_SYMMETRY = 1,
    PLANAR_SYMMETRY = 2,
}

/// <summary>
/// Controls the way the a particle mesh emitter determines the starting speed and direction of the particles that are emitted.
/// </summary>
public enum VelocityType {
    VELOCITY_USE_NORMALS = 0,
    VELOCITY_USE_RANDOM = 1,
    VELOCITY_USE_DIRECTION = 2,
}

/// <summary>
/// Controls which parts of the mesh that the particles are emitted from.
/// </summary>
public enum EmitFrom {
    EMIT_FROM_VERTICES = 0,
    EMIT_FROM_FACE_CENTER = 1,
    EMIT_FROM_EDGE_CENTER = 2,
    EMIT_FROM_FACE_SURFACE = 3,
    EMIT_FROM_EDGE_SURFACE = 4,
}

/// <summary>
/// The type of information that is stored in a texture used by an NiTextureEffect.
/// </summary>
public enum TextureType {
    TEX_PROJECTED_LIGHT = 0,
    TEX_PROJECTED_SHADOW = 1,
    TEX_ENVIRONMENT_MAP = 2,
    TEX_FOG_MAP = 3,
}

/// <summary>
/// Determines the way that UV texture coordinates are generated.
/// </summary>
public enum CoordGenType {
    CG_WORLD_PARALLEL = 0,
    CG_WORLD_PERSPECTIVE = 1,
    CG_SPHERE_MAP = 2,
    CG_SPECULAR_CUBE_MAP = 3,
    CG_DIFFUSE_CUBE_MAP = 4,
}

public enum EndianType {
    ENDIAN_BIG = 0,
    ENDIAN_LITTLE = 1,
}

/// <summary>
/// Used by NiMaterialColorControllers to select which type of color in the controlled object that will be animated.
/// </summary>
public enum MaterialColor {
    TC_AMBIENT = 0,
    TC_DIFFUSE = 1,
    TC_SPECULAR = 2,
    TC_SELF_ILLUM = 3,
}

/// <summary>
/// Used by NiLightColorControllers to select which type of color in the controlled object that will be animated.
/// </summary>
public enum LightColor {
    LC_DIFFUSE = 0,
    LC_AMBIENT = 1,
}

/// <summary>
/// Used by NiGeometryData to control the volatility of the mesh.
/// Consistency Type is masked to only the upper 4 bits (0xF000). Dirty mask is the lower 12 (0x0FFF) but only used at runtime.
/// </summary>
public enum ConsistencyType {
    CT_MUTABLE = 0x0000,
    CT_STATIC = 0x4000,
    CT_VOLATILE = 0x8000,
}

/// <summary>
/// Describes the way that NiSortAdjustNode modifies the sorting behavior for the subtree below it.
/// </summary>
public enum SortingMode {
    SORTING_INHERIT = 0,
    SORTING_OFF = 1,
}

/// <summary>
/// The propagation mode controls scene graph traversal during collision detection operations for NiCollisionData.
/// </summary>
public enum PropagationMode {
    PROPAGATE_ON_SUCCESS = 0,
    PROPAGATE_ON_FAILURE = 1,
    PROPAGATE_ALWAYS = 2,
    PROPAGATE_NEVER = 3,
}

/// <summary>
/// The collision mode controls the type of collision operation that is to take place for NiCollisionData.
/// </summary>
public enum CollisionMode {
    CM_USE_OBB = 0,
    CM_USE_TRI = 1,
    CM_USE_ABV = 2,
    CM_NOTEST = 3,
    CM_USE_NIBOUND = 4,
}

public enum BoundVolumeType {
    BASE_BV = 0xffffffff,
    SPHERE_BV = 0,
    BOX_BV = 1,
    CAPSULE_BV = 2,
    UNION_BV = 4,
    HALFSPACE_BV = 5,
}

/// <summary>
/// Bethesda Havok.
/// </summary>
public enum hkResponseType {
    RESPONSE_INVALID = 0,
    RESPONSE_SIMPLE_CONTACT = 1,
    RESPONSE_REPORTING = 2,
    RESPONSE_NONE = 3,
}

/// <summary>
/// Biped bodypart data used for visibility control of triangles.  Options are Fallout 3, except where marked for Skyrim (uses SBP prefix)
/// Skyrim BP names are listed only for vanilla names, different creatures have different defnitions for naming.
/// </summary>
public enum BSDismemberBodyPartType {
    BP_TORSO = 0,
    BP_HEAD = 1,
    BP_HEAD2 = 2,
    BP_LEFTARM = 3,
    BP_LEFTARM2 = 4,
    BP_RIGHTARM = 5,
    BP_RIGHTARM2 = 6,
    BP_LEFTLEG = 7,
    BP_LEFTLEG2 = 8,
    BP_LEFTLEG3 = 9,
    BP_RIGHTLEG = 10,
    BP_RIGHTLEG2 = 11,
    BP_RIGHTLEG3 = 12,
    BP_BRAIN = 13,
    SBP_30_HEAD = 30,
    SBP_31_HAIR = 31,
    SBP_32_BODY = 32,
    SBP_33_HANDS = 33,
    SBP_34_FOREARMS = 34,
    SBP_35_AMULET = 35,
    SBP_36_RING = 36,
    SBP_37_FEET = 37,
    SBP_38_CALVES = 38,
    SBP_39_SHIELD = 39,
    SBP_40_TAIL = 40,
    SBP_41_LONGHAIR = 41,
    SBP_42_CIRCLET = 42,
    SBP_43_EARS = 43,
    SBP_44_DRAGON_BLOODHEAD_OR_MOD_MOUTH = 44,
    SBP_45_DRAGON_BLOODWINGL_OR_MOD_NECK = 45,
    SBP_46_DRAGON_BLOODWINGR_OR_MOD_CHEST_PRIMARY = 46,
    SBP_47_DRAGON_BLOODTAIL_OR_MOD_BACK = 47,
    SBP_48_MOD_MISC1 = 48,
    SBP_49_MOD_PELVIS_PRIMARY = 49,
    SBP_50_DECAPITATEDHEAD = 50,
    SBP_51_DECAPITATE = 51,
    SBP_52_MOD_PELVIS_SECONDARY = 52,
    SBP_53_MOD_LEG_RIGHT = 53,
    SBP_54_MOD_LEG_LEFT = 54,
    SBP_55_MOD_FACE_JEWELRY = 55,
    SBP_56_MOD_CHEST_SECONDARY = 56,
    SBP_57_MOD_SHOULDER = 57,
    SBP_58_MOD_ARM_LEFT = 58,
    SBP_59_MOD_ARM_RIGHT = 59,
    SBP_60_MOD_MISC2 = 60,
    SBP_61_FX01 = 61,
    BP_SECTIONCAP_HEAD = 101,
    BP_SECTIONCAP_HEAD2 = 102,
    BP_SECTIONCAP_LEFTARM = 103,
    BP_SECTIONCAP_LEFTARM2 = 104,
    BP_SECTIONCAP_RIGHTARM = 105,
    BP_SECTIONCAP_RIGHTARM2 = 106,
    BP_SECTIONCAP_LEFTLEG = 107,
    BP_SECTIONCAP_LEFTLEG2 = 108,
    BP_SECTIONCAP_LEFTLEG3 = 109,
    BP_SECTIONCAP_RIGHTLEG = 110,
    BP_SECTIONCAP_RIGHTLEG2 = 111,
    BP_SECTIONCAP_RIGHTLEG3 = 112,
    BP_SECTIONCAP_BRAIN = 113,
    SBP_130_HEAD = 130,
    SBP_131_HAIR = 131,
    SBP_141_LONGHAIR = 141,
    SBP_142_CIRCLET = 142,
    SBP_143_EARS = 143,
    SBP_150_DECAPITATEDHEAD = 150,
    BP_TORSOCAP_HEAD = 201,
    BP_TORSOCAP_HEAD2 = 202,
    BP_TORSOCAP_LEFTARM = 203,
    BP_TORSOCAP_LEFTARM2 = 204,
    BP_TORSOCAP_RIGHTARM = 205,
    BP_TORSOCAP_RIGHTARM2 = 206,
    BP_TORSOCAP_LEFTLEG = 207,
    BP_TORSOCAP_LEFTLEG2 = 208,
    BP_TORSOCAP_LEFTLEG3 = 209,
    BP_TORSOCAP_RIGHTLEG = 210,
    BP_TORSOCAP_RIGHTLEG2 = 211,
    BP_TORSOCAP_RIGHTLEG3 = 212,
    BP_TORSOCAP_BRAIN = 213,
    SBP_230_HEAD = 230,
    BP_TORSOSECTION_HEAD = 1000,
    BP_TORSOSECTION_HEAD2 = 2000,
    BP_TORSOSECTION_LEFTARM = 3000,
    BP_TORSOSECTION_LEFTARM2 = 4000,
    BP_TORSOSECTION_RIGHTARM = 5000,
    BP_TORSOSECTION_RIGHTARM2 = 6000,
    BP_TORSOSECTION_LEFTLEG = 7000,
    BP_TORSOSECTION_LEFTLEG2 = 8000,
    BP_TORSOSECTION_LEFTLEG3 = 9000,
    BP_TORSOSECTION_RIGHTLEG = 10000,
    BP_TORSOSECTION_RIGHTLEG2 = 11000,
    BP_TORSOSECTION_RIGHTLEG3 = 12000,
    BP_TORSOSECTION_BRAIN = 13000,
}

/// <summary>
/// Values for configuring the shader type in a BSLightingShaderProperty
/// </summary>
public enum BSLightingShaderPropertyShaderType {
    Default = 0,
    Environment Map = 1,
    Glow Shader = 2,
    Parallax = 3,
    Face Tint = 4,
    Skin Tint = 5,
    Hair Tint = 6,
    Parallax Occ = 7,
    Multitexture Landscape = 8,
    LOD Landscape = 9,
    Snow = 10,
    MultiLayer Parallax = 11,
    Tree Anim = 12,
    LOD Objects = 13,
    Sparkle Snow = 14,
    LOD Objects HD = 15,
    Eye Envmap = 16,
    Cloud = 17,
    LOD Landscape Noise = 18,
    Multitexture Landscape LOD Blend = 19,
    FO4 Dismemberment = 20,
}

/// <summary>
/// An unsigned 32-bit integer, describing which float variable in BSEffectShaderProperty to animate.
/// </summary>
public enum EffectShaderControlledVariable {
    EmissiveMultiple = 0,
    Falloff Start Angle = 1,
    Falloff Stop Angle = 2,
    Falloff Start Opacity = 3,
    Falloff Stop Opacity = 4,
    Alpha Transparency = 5,
    U Offset = 6,
    U Scale = 7,
    V Offset = 8,
    V Scale = 9,
}

/// <summary>
/// An unsigned 32-bit integer, describing which color in BSEffectShaderProperty to animate.
/// </summary>
public enum EffectShaderControlledColor {
    Emissive Color = 0,
}

/// <summary>
/// An unsigned 32-bit integer, describing which float variable in BSLightingShaderProperty to animate.
/// </summary>
public enum LightingShaderControlledVariable {
    Refraction Strength = 0,
    Environment Map Scale = 8,
    Glossiness = 9,
    Specular Strength = 10,
    Emissive Multiple = 11,
    Alpha = 12,
    U Offset = 20,
    U Scale = 21,
    V Offset = 22,
    V Scale = 23,
}

/// <summary>
/// An unsigned 32-bit integer, describing which color in BSLightingShaderProperty to animate.
/// </summary>
public enum LightingShaderControlledColor {
    Specular Color = 0,
    Emissive Color = 1,
}

/// <summary>
/// Bethesda Havok. Describes the type of bhkConstraint.
/// </summary>
public enum hkConstraintType {
    BallAndSocket = 0,
    Hinge = 1,
    Limited Hinge = 2,
    Prismatic = 6,
    Ragdoll = 7,
    StiffSpring = 8,
    Malleable = 13,
}


#endregion

#region Compounds

/// <summary>
/// A string of given length.
/// </summary>

/// <summary>
/// A string type.
/// </summary>

/// <summary>
/// An array of bytes.
/// </summary>

/// <summary>
/// An array of bytes.
/// </summary>

/// <summary>
/// A color without alpha (red, green, blue).
/// </summary>

/// <summary>
/// A color without alpha (red, green, blue).
/// </summary>

/// <summary>
/// A color with alpha (red, green, blue, alpha).
/// </summary>

/// <summary>
/// A color with alpha (red, green, blue, alpha).
/// </summary>

/// <summary>
/// A string that contains the path to a file.
/// </summary>

/// <summary>
/// The NIF file footer.
/// </summary>

/// <summary>
/// The distance range where a specific level of detail applies.
/// </summary>

/// <summary>
/// Group of vertex indices of vertices that match.
/// </summary>

/// <summary>
/// A vector in 3D space (x,y,z).
/// </summary>

/// <summary>
/// A vector in 3D space (x,y,z).
/// </summary>

/// <summary>
/// A vector in 3D space (x,y,z).
/// </summary>

/// <summary>
/// A 4-dimensional vector.
/// </summary>

/// <summary>
/// A quaternion.
/// </summary>

/// <summary>
/// A quaternion as it appears in the havok objects.
/// </summary>

/// <summary>
/// A 2x2 matrix of float values.  Stored in OpenGL column-major format.
/// </summary>

/// <summary>
/// A 3x3 rotation matrix; M^T M=identity, det(M)=1.    Stored in OpenGL column-major format.
/// </summary>

/// <summary>
/// A 3x4 transformation matrix.
/// </summary>

/// <summary>
/// A 4x4 transformation matrix.
/// </summary>

/// <summary>
/// A 3x3 Havok matrix stored in 4x3 due to memory alignment.
/// </summary>

/// <summary>
/// Description of a mipmap within an NiPixelData object.
/// </summary>

/// <summary>
/// A set of NiNode references.
/// </summary>

/// <summary>
/// Another string format, for short strings.  Specific to Bethesda-specific header tags.
/// </summary>

/// <summary>
/// NiBoneLODController::SkinInfo. Reference to shape and skin instance.
/// </summary>

/// <summary>
/// A set of NiBoneLODController::SkinInfo.
/// </summary>

/// <summary>
/// NiSkinData::BoneVertData. A vertex and its weight.
/// </summary>

/// <summary>
/// NiSkinData::BoneVertData. A vertex and its weight.
/// </summary>

/// <summary>
/// Used in NiDefaultAVObjectPalette.
/// </summary>

/// <summary>
/// In a .kf file, this links to a controllable object, via its name (or for version 10.2.0.0 and up, a link and offset to a NiStringPalette that contains the name), and a sequence of interpolators that apply to this controllable object, via links.
/// For Controller ID, NiInterpController::GetCtlrID() virtual function returns a string formatted specifically for the derived type.
/// For Interpolator ID, NiInterpController::GetInterpolatorID() virtual function returns a string formatted specifically for the derived type.
/// The string formats are documented on the relevant niobject blocks.
/// </summary>

/// <summary>
/// Information about how the file was exported
/// </summary>

/// <summary>
/// The NIF file header.
/// </summary>

/// <summary>
/// A list of \\0 terminated strings.
/// </summary>

/// <summary>
/// Tension, bias, continuity.
/// </summary>

/// <summary>
/// A generic key with support for interpolation. Type 1 is normal linear interpolation, type 2 has forward and backward tangents, and type 3 has tension, bias and continuity arguments. Note that color4 and byte always seem to be of type 1.
/// </summary>

/// <summary>
/// Array of vector keys (anything that can be interpolated, except rotations).
/// </summary>

/// <summary>
/// A special version of the key type used for quaternions.  Never has tangents.
/// </summary>

/// <summary>
/// Texture coordinates (u,v). As in OpenGL; image origin is in the lower left corner.
/// </summary>

/// <summary>
/// Texture coordinates (u,v).
/// </summary>

/// <summary>
/// Describes the order of scaling and rotation matrices. Translate, Scale, Rotation, Center are from TexDesc.
/// Back = inverse of Center. FromMaya = inverse of the V axis with a positive translation along V of 1 unit.
/// </summary>
public enum TransformMethod {
    Maya Deprecated = 0,
    Max = 1,
    Maya = 2,
}

/// <summary>
/// NiTexturingProperty::Map. Texture description.
/// </summary>

/// <summary>
/// NiTexturingProperty::ShaderMap. Shader texture description.
/// </summary>

/// <summary>
/// List of three vertex indices.
/// </summary>

[Flags]
public enum VertexFlags {
    Vertex = 4,
    UVs = 5,
    UVs_2 = 6,
    Normals = 7,
    Tangents = 8,
    Vertex_Colors = 9,
    Skinned = 10,
    Land_Data = 11,
    Eye_Data = 12,
    Instance = 13,
    Full_Precision = 14,
}




/// <summary>
/// Skinning data for a submesh, optimized for hardware skinning. Part of NiSkinPartition.
/// </summary>

/// <summary>
/// A plane.
/// </summary>

/// <summary>
/// A sphere.
/// </summary>



/// <summary>
/// Bethesda Animation. Furniture entry points. It specifies the direction(s) from where the actor is able to enter (and leave) the position.
/// </summary>
[Flags]
public enum FurnitureEntryPoints {
    Front = 0,
    Behind = 1,
    Right = 2,
    Left = 3,
    Up = 4,
}

/// <summary>
/// Bethesda Animation. Animation type used on this position. This specifies the function of this position.
/// </summary>
public enum AnimationType {
    Sit = 1,
    Sleep = 2,
    Lean = 4,
}

/// <summary>
/// Bethesda Animation. Describes a furniture position?
/// </summary>

/// <summary>
/// Bethesda Havok. A triangle with extra data used for physics.
/// </summary>

/// <summary>
/// Geometry morphing data component.
/// </summary>

/// <summary>
/// particle array entry
/// </summary>

/// <summary>
/// NiSkinData::BoneData. Skinning data component.
/// </summary>

/// <summary>
/// Bethesda Havok. Collision filter info representing Layer, Flags, Part Number, and Group all combined into one uint.
/// </summary>

/// <summary>
/// Bethesda Havok. Material wrapper for varying material enums by game.
/// </summary>

/// <summary>
/// Bethesda Havok. Havok Information for packed TriStrip shapes.
/// </summary>




public enum MotorType {
    MOTOR_NONE = 0,
    MOTOR_POSITION = 1,
    MOTOR_VELOCITY = 2,
    MOTOR_SPRING = 3,
}


/// <summary>
/// This constraint defines a cone in which an object can rotate. The shape of the cone can be controlled in two (orthogonal) directions.
/// </summary>

/// <summary>
/// This constraint allows rotation about a specified axis, limited by specified boundaries.
/// </summary>

/// <summary>
/// This constraint allows rotation about a specified axis.
/// </summary>




/// <summary>
/// Used to store skin weights in NiTriShapeSkinController.
/// </summary>

/// <summary>
/// Determines how the raw image data is stored in NiRawImageData.
/// </summary>
public enum ImageType {
    RGB = 1,
    RGBA = 2,
}

/// <summary>
/// Box Bounding Volume
/// </summary>

/// <summary>
/// Capsule Bounding Volume
/// </summary>





/// <summary>
/// Transformation data for the bone at this index in bhkPoseArray.
/// </summary>

/// <summary>
/// A list of transforms for each bone in bhkPoseArray.
/// </summary>

/// <summary>
/// Array of Vectors for Decal placement in BSDecalPlacementVectorExtraData.
/// </summary>

/// <summary>
/// Editor flags for the Body Partitions.
/// </summary>
[Flags]
public enum BSPartFlag {
    PF_EDITOR_VISIBLE = 0,
    PF_START_NET_BONESET = 8,
}

/// <summary>
/// Body part list for DismemberSkinInstance
/// </summary>

/// <summary>
/// Stores Bone Level of Detail info in a BSBoneLODExtraData
/// </summary>

/// <summary>
/// Per-chunk material, used in bhkCompressedMeshShapeData
/// </summary>

/// <summary>
/// Triangle indices used in pair with "Big Verts" in a bhkCompressedMeshShapeData.
/// </summary>

/// <summary>
/// A set of transformation data: translation and rotation
/// </summary>

/// <summary>
/// Defines subshape chunks in bhkCompressedMeshShapeData
/// </summary>




#endregion

#region NIF Objects

/// <summary>
/// Abstract object type.
/// </summary>

/// <summary>
/// Unknown.
/// </summary>

/// <summary>
/// Unknown. Only found in 2.3 nifs.
/// </summary>

/// <summary>
/// Unknown!
/// </summary>

/// <summary>
/// Unknown!
/// </summary>

/// <summary>
/// Unknown!
/// </summary>

/// <summary>
/// Unknown!
/// </summary>

/// <summary>
/// LEGACY (pre-10.1). Abstract base class for particle system modifiers.
/// </summary>

/// <summary>
/// Particle system collider.
/// </summary>

public enum BroadPhaseType {
    BROAD_PHASE_INVALID = 0,
    BROAD_PHASE_ENTITY = 1,
    BROAD_PHASE_PHANTOM = 2,
    BROAD_PHASE_BORDER = 3,
}


/// <summary>
/// The base type of most Bethesda-specific Havok-related NIF objects.
/// </summary>

/// <summary>
/// Havok objects that can be saved and loaded from disk?
/// </summary>

/// <summary>
/// Havok objects that have a position in the world?
/// </summary>

/// <summary>
/// Havok object that do not react with other objects when they collide (causing deflection, etc.) but still trigger collision notifications to the game.  Possible uses are traps, portals, AI fields, etc.
/// </summary>

/// <summary>
/// A Havok phantom that uses a Havok shape object for its collision volume instead of just a bounding box.
/// </summary>

/// <summary>
/// Unknown shape.
/// </summary>

/// <summary>
/// A havok node, describes physical properties.
/// </summary>

/// <summary>
/// This is the default body type for all "normal" usable and static world objects. The "T" suffix
/// marks this body as active for translation and rotation, a normal bhkRigidBody ignores those
/// properties. Because the properties are equal, a bhkRigidBody may be renamed into a bhkRigidBodyT and vice-versa.
/// </summary>

/// <summary>
/// The "T" suffix marks this body as active for translation and rotation.
/// </summary>

/// <summary>
/// Describes a physical constraint.
/// </summary>

/// <summary>
/// Hinge constraint.
/// </summary>

/// <summary>
/// A malleable constraint.
/// </summary>

/// <summary>
/// A spring constraint.
/// </summary>

/// <summary>
/// Ragdoll constraint.
/// </summary>

/// <summary>
/// A prismatic constraint.
/// </summary>

/// <summary>
/// A hinge constraint.
/// </summary>

/// <summary>
/// A Ball and Socket Constraint.
/// </summary>

/// <summary>
/// Two Vector4 for pivot in A and B.
/// </summary>

/// <summary>
/// A Ball and Socket Constraint chain.
/// </summary>

/// <summary>
/// A Havok Shape?
/// </summary>

/// <summary>
/// Transforms a shape.
/// </summary>

/// <summary>
/// A havok shape, perhaps with a bounding sphere for quick rejection in addition to more detailed shape data?
/// </summary>

/// <summary>
/// A havok shape.
/// </summary>

/// <summary>
/// A sphere.
/// </summary>

/// <summary>
/// A capsule.
/// </summary>

/// <summary>
/// A box.
/// </summary>

/// <summary>
/// A convex shape built from vertices. Note that if the shape is used in
/// a non-static object (such as clutter), then they will simply fall
/// through ground when they are under a bhkListShape.
/// </summary>

/// <summary>
/// A convex transformed shape?
/// </summary>


/// <summary>
/// Unknown.
/// </summary>

/// <summary>
/// A tree-like Havok data structure stored in an assembly-like binary code?
/// </summary>

/// <summary>
/// Memory optimized partial polytope bounding volume tree shape (not an entity).
/// </summary>

/// <summary>
/// Havok collision object that uses multiple shapes?
/// </summary>

/// <summary>
/// A list of shapes.
/// 
/// Do not put a bhkPackedNiTriStripsShape in the Sub Shapes. Use a
/// separate collision nodes without a list shape for those.
/// 
/// Also, shapes collected in a bhkListShape may not have the correct
/// walking noise, so only use it for non-walkable objects.
/// </summary>


/// <summary>
/// A shape constructed from strips data.
/// </summary>

/// <summary>
/// A shape constructed from a bunch of strips.
/// </summary>

/// <summary>
/// A generic extra data object.
/// </summary>

/// <summary>
/// Abstract base class for all interpolators of bool, float, NiQuaternion, NiPoint3, NiColorA, and NiQuatTransform data.
/// </summary>

/// <summary>
/// Abstract base class for interpolators that use NiAnimationKeys (Key, KeyGrp) for interpolation.
/// </summary>

/// <summary>
/// Uses NiFloatKeys to animate a float value over time.
/// </summary>

/// <summary>
/// An interpolator for transform keyframes.
/// </summary>

/// <summary>
/// Uses NiPosKeys to animate an NiPoint3 value over time.
/// </summary>

[Flags]
public enum PathFlags {
    CVDataNeedsUpdate = 0,
    CurveTypeOpen = 1,
    AllowFlip = 2,
    Bank = 3,
    ConstantVelocity = 4,
    Follow = 5,
    Flip = 6,
}

/// <summary>
/// Used to make an object follow a predefined spline path.
/// </summary>

/// <summary>
/// Uses NiBoolKeys to animate a bool value over time.
/// </summary>

/// <summary>
/// Uses NiBoolKeys to animate a bool value over time.
/// Unlike NiBoolInterpolator, it ensures that keys have not been missed between two updates.
/// </summary>

public enum InterpBlendFlags {
    MANAGER_CONTROLLED = 1,
}

/// <summary>
/// Interpolator item for array in NiBlendInterpolator.
/// </summary>

/// <summary>
/// Abstract base class for all NiInterpolators that blend the results of sub-interpolators together to compute a final weighted value.
/// </summary>

/// <summary>
/// Abstract base class for interpolators storing data via a B-spline.
/// </summary>

/// <summary>
/// Abstract base class for NiObjects that support names, extra data, and time controllers.
/// </summary>

/// <summary>
/// This is the most common collision object found in NIF files. It acts as a real object that
/// is visible and possibly (if the body allows for it) interactive. The node itself
/// is simple, it only has three properties.
/// For this type of collision object, bhkRigidBody or bhkRigidBodyT is generally used.
/// </summary>

/// <summary>
/// Collision box.
/// </summary>

/// <summary>
/// bhkNiCollisionObject flags. The flags 0x2, 0x100, and 0x200 are not seen in any NIF nor get/set by the engine.
/// </summary>
[Flags]
public enum bhkCOFlags {
    ACTIVE = 0,
    NOTIFY = 2,
    SET_LOCAL = 3,
    DBG_DISPLAY = 4,
    USE_VEL = 5,
    RESET = 6,
    SYNC_ON_UPDATE = 7,
    ANIM_TARGETED = 10,
    DISMEMBERED_LIMB = 11,
}

/// <summary>
/// Havok related collision object?
/// </summary>

/// <summary>
/// Havok related collision object?
/// </summary>

/// <summary>
/// Unknown.
/// </summary>

/// <summary>
/// Unknown.
/// </summary>

/// <summary>
/// Unknown.
/// </summary>

/// <summary>
/// Abstract audio-visual base class from which all of Gamebryo's scene graph objects inherit.
/// </summary>

/// <summary>
/// Abstract base class for dynamic effects such as NiLights or projected texture effects.
/// </summary>

/// <summary>
/// Abstract base class that represents light sources in a scene graph.
/// For Bethesda Stream 130 (FO4), NiLight now directly inherits from NiAVObject.
/// </summary>

/// <summary>
/// Abstract base class representing all rendering properties. Subclasses are attached to NiAVObjects to control their rendering.
/// </summary>

/// <summary>
/// Unknown
/// </summary>

/// <summary>
/// Abstract base class for all particle system modifiers.
/// </summary>

/// <summary>
/// Abstract base class for all particle system emitters.
/// </summary>

/// <summary>
/// Abstract base class for particle emitters that emit particles from a volume.
/// </summary>

/// <summary>
/// Abstract base class that provides the base timing and update functionality for all the Gamebryo animation controllers.
/// </summary>

/// <summary>
/// Abstract base class for all NiTimeController objects using NiInterpolator objects to animate their target objects.
/// </summary>

/// <summary>
/// DEPRECATED (20.6)
/// </summary>

/// <summary>
/// DEPRECATED (20.5), replaced by NiMorphMeshModifier.
/// Time controller for geometry morphing.
/// </summary>

/// <summary>
/// Unknown! Used by Daoc->'healing.nif'.
/// </summary>

/// <summary>
/// Unknown! Used by Daoc.
/// </summary>

/// <summary>
/// Uses a single NiInterpolator to animate its target value.
/// </summary>

/// <summary>
/// DEPRECATED (10.2), RENAMED (10.2) to NiTransformController
/// A time controller object for animation key frames.
/// </summary>

/// <summary>
/// NiTransformController replaces NiKeyframeController.
/// </summary>

/// <summary>
/// A particle system modifier controller.
/// NiInterpController::GetCtlrID() string format:
///     '%s'
/// Where %s = Value of "Modifier Name"
/// </summary>

/// <summary>
/// Particle system emitter controller.
/// NiInterpController::GetInterpolatorID() string format:
///     ['BirthRate', 'EmitterActive'] (for "Interpolator" and "Visibility Interpolator" respectively)
/// </summary>

/// <summary>
/// A particle system modifier controller that animates a boolean value for particles.
/// </summary>

/// <summary>
/// A particle system modifier controller that animates active/inactive state for particles.
/// </summary>

/// <summary>
/// A particle system modifier controller that animates a floating point value for particles.
/// </summary>

/// <summary>
/// Animates the declination value on an NiPSysEmitter object.
/// </summary>

/// <summary>
/// Animates the declination variation value on an NiPSysEmitter object.
/// </summary>

/// <summary>
/// Animates the size value on an NiPSysEmitter object.
/// </summary>

/// <summary>
/// Animates the lifespan value on an NiPSysEmitter object.
/// </summary>

/// <summary>
/// Animates the speed value on an NiPSysEmitter object.
/// </summary>

/// <summary>
/// Animates the strength value of an NiPSysGravityModifier.
/// </summary>

/// <summary>
/// Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target float value.
/// </summary>

/// <summary>
/// Changes the image a Map (TexDesc) will use. Uses a float interpolator to animate the texture index.
/// Often used for performing flipbook animation.
/// </summary>

/// <summary>
/// Animates the alpha value of a property using an interpolator.
/// </summary>

/// <summary>
/// Used to animate a single member of an NiTextureTransform.
/// NiInterpController::GetCtlrID() string formats:
///     ['%1-%2-TT_TRANSLATE_U', '%1-%2-TT_TRANSLATE_V', '%1-%2-TT_ROTATE', '%1-%2-TT_SCALE_U', '%1-%2-TT_SCALE_V']
/// (Depending on "Operation" enumeration, %1 = Value of "Shader Map", %2 = Value of "Texture Slot")
/// </summary>

/// <summary>
/// Unknown controller.
/// </summary>

/// <summary>
/// Abstract base class for all NiInterpControllers that use a NiInterpolator to animate their target boolean value.
/// </summary>

/// <summary>
/// Animates the visibility of an NiAVObject.
/// </summary>

/// <summary>
/// Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target NiPoint3 value.
/// </summary>

/// <summary>
/// Time controller for material color. Flags are used for color selection in versions below 10.1.0.0.
/// Bits 4-5: Target Color (00 = Ambient, 01 = Diffuse, 10 = Specular, 11 = Emissive)
/// NiInterpController::GetCtlrID() string formats:
///     ['AMB', 'DIFF', 'SPEC', 'SELF_ILLUM'] (Depending on "Target Color")
/// </summary>

/// <summary>
/// Animates the ambient, diffuse and specular colors of an NiLight.
/// NiInterpController::GetCtlrID() string formats:
///     ['Diffuse', 'Ambient'] (Depending on "Target Color")
/// </summary>

/// <summary>
/// Abstract base class for all extra data controllers.
/// NiInterpController::GetCtlrID() string format:
///     '%s'
/// Where %s = Value of "Extra Data Name"
/// </summary>

/// <summary>
/// Animates an NiFloatExtraData object attached to an NiAVObject.
/// NiInterpController::GetCtlrID() string format is same as parent.
/// </summary>

/// <summary>
/// Animates an NiFloatsExtraData object attached to an NiAVObject.
/// NiInterpController::GetCtlrID() string format:
///     '%s[%d]'
/// Where %s = Value of "Extra Data Name", %d = Value of "Floats Extra Data Index"
/// </summary>

/// <summary>
/// Animates an NiFloatsExtraData object attached to an NiAVObject.
/// NiInterpController::GetCtlrID() string format:
///     '%s[%d]'
/// Where %s = Value of "Extra Data Name", %d = Value of "Floats Extra Data Index"
/// </summary>

/// <summary>
/// DEPRECATED (20.5), Replaced by NiSkinningLODController.
/// Level of detail controller for bones.  Priority is arranged from low to high.
/// </summary>

/// <summary>
/// A simple LOD controller for bones.
/// </summary>


/// <summary>
/// Describes a visible scene element with vertices like a mesh, a particle system, lines, etc.
/// </summary>

/// <summary>
/// Describes a mesh, built from triangles.
/// </summary>

[Flags]
public enum VectorFlags {
    UV_1 = 0,
    UV_2 = 1,
    UV_4 = 2,
    UV_8 = 3,
    UV_16 = 4,
    UV_32 = 5,
    Unk64 = 6,
    Unk128 = 7,
    Unk256 = 8,
    Unk512 = 9,
    Unk1024 = 10,
    Unk2048 = 11,
    Has_Tangents = 12,
    Unk8192 = 13,
    Unk16384 = 14,
    Unk32768 = 15,
}

[Flags]
public enum BSVectorFlags {
    Has_UV = 0,
    Unk2 = 1,
    Unk4 = 2,
    Unk8 = 3,
    Unk16 = 4,
    Unk32 = 5,
    Unk64 = 6,
    Unk128 = 7,
    Unk256 = 8,
    Unk512 = 9,
    Unk1024 = 10,
    Unk2048 = 11,
    Has_Tangents = 12,
    Unk8192 = 13,
    Unk16384 = 14,
    Unk32768 = 15,
}

/// <summary>
/// Mesh data: vertices, vertex normals, etc.
/// </summary>


/// <summary>
/// Describes a mesh, built from triangles.
/// </summary>

/// <summary>
/// Unknown. Is apparently only used in skeleton.nif files.
/// </summary>

/// <summary>
/// Bethesda-specific collision bounding box for skeletons.
/// </summary>

/// <summary>
/// Unknown. Marks furniture sitting positions?
/// </summary>

/// <summary>
/// Particle modifier that adds a blend of object space translation and rotation to particles born in world space.
/// </summary>

/// <summary>
/// Particle emitter that uses a node, its children and subchildren to emit from.  Emission will be evenly spread along points from nodes leading to their direct parents/children only.
/// </summary>

/// <summary>
/// Particle Modifier that uses the wind value from the gamedata to alter the path of particles.
/// </summary>

/// <summary>
/// NiTriStripsData for havok data?
/// </summary>

/// <summary>
/// Transparency. Flags 0x00ED.
/// </summary>

/// <summary>
/// Ambient light source.
/// </summary>

/// <summary>
/// Generic rotating particles data object.
/// </summary>

/// <summary>
/// Rotating particles data object.
/// </summary>

/// <summary>
/// Particle system data object (with automatic normals?).
/// </summary>

/// <summary>
/// Particle Description.
/// </summary>

/// <summary>
/// Particle system data.
/// </summary>

/// <summary>
/// Particle meshes data.
/// </summary>

/// <summary>
/// Binary extra data object. Used to store tangents and bitangents in Oblivion.
/// </summary>

/// <summary>
/// Voxel extra data object.
/// </summary>

/// <summary>
/// Voxel data object.
/// </summary>

/// <summary>
/// Blends bool values together.
/// </summary>

/// <summary>
/// Blends float values together.
/// </summary>

/// <summary>
/// Blends NiPoint3 values together.
/// </summary>

/// <summary>
/// Blends NiQuatTransform values together.
/// </summary>

/// <summary>
/// Wrapper for boolean animation keys.
/// </summary>

/// <summary>
/// Boolean extra data.
/// </summary>

/// <summary>
/// Contains an NiBSplineBasis for use in interpolation of open, uniform B-Splines.
/// </summary>

/// <summary>
/// Uses B-Splines to animate a float value over time.
/// </summary>

/// <summary>
/// NiBSplineFloatInterpolator plus the information required for using compact control points.
/// </summary>

/// <summary>
/// Uses B-Splines to animate an NiPoint3 value over time.
/// </summary>

/// <summary>
/// NiBSplinePoint3Interpolator plus the information required for using compact control points.
/// </summary>

/// <summary>
/// Supports the animation of position, rotation, and scale using an NiQuatTransform.
/// The NiQuatTransform can be an unchanging pose or interpolated from B-Spline control point channels.
/// </summary>

/// <summary>
/// NiBSplineTransformInterpolator plus the information required for using compact control points.
/// </summary>


/// <summary>
/// Contains one or more sets of control points for use in interpolation of open, uniform B-Splines, stored as either float or compact.
/// </summary>

/// <summary>
/// Camera object.
/// </summary>

/// <summary>
/// Wrapper for color animation keys.
/// </summary>

/// <summary>
/// Extra data in the form of NiColorA (red, green, blue, alpha).
/// </summary>

/// <summary>
/// Controls animation sequences on a specific branch of the scene graph.
/// </summary>

/// <summary>
/// Root node in NetImmerse .kf files (until version 10.0).
/// </summary>

/// <summary>
/// Root node in Gamebryo .kf files (version 10.0.1.0 and up).
/// </summary>

/// <summary>
/// Abstract base class for indexing NiAVObject by name.
/// </summary>

/// <summary>
/// NiAVObjectPalette implementation. Used to quickly look up objects by name.
/// </summary>

/// <summary>
/// Directional light source.
/// </summary>

/// <summary>
/// NiDitherProperty allows the application to turn the dithering of interpolated colors and fog values on and off.
/// </summary>

/// <summary>
/// DEPRECATED (10.2), REMOVED (20.5). Replaced by NiTransformController and NiLookAtInterpolator.
/// </summary>

/// <summary>
/// Wrapper for 1D (one-dimensional) floating point animation keys.
/// </summary>

/// <summary>
/// Extra float data.
/// </summary>

/// <summary>
/// Extra float array data.
/// </summary>

/// <summary>
/// NiFogProperty allows the application to enable, disable and control the appearance of fog.
/// </summary>

/// <summary>
/// LEGACY (pre-10.1) particle modifier. Applies a gravitational field on the particles.
/// </summary>

/// <summary>
/// Extra integer data.
/// </summary>

/// <summary>
/// Controls animation and collision.  Integer holds flags:
/// Bit 0 : enable havok, bAnimated(Skyrim)
/// Bit 1 : enable collision, bHavok(Skyrim)
/// Bit 2 : is skeleton nif?, bRagdoll(Skyrim)
/// Bit 3 : enable animation, bComplex(Skyrim)
/// Bit 4 : FlameNodes present, bAddon(Skyrim)
/// Bit 5 : EditorMarkers present, bEditorMarker(Skyrim)
/// Bit 6 : bDynamic(Skyrim)
/// Bit 7 : bArticulated(Skyrim)
/// Bit 8 : bIKTarget(Skyrim)/needsTransformUpdates
/// Bit 9 : bExternalEmit(Skyrim)
/// Bit 10: bMagicShaderParticles(Skyrim)
/// Bit 11: bLights(Skyrim)
/// Bit 12: bBreakable(Skyrim)
/// Bit 13: bSearchedBreakable(Skyrim) .. Runtime only?
/// </summary>

/// <summary>
/// Extra integer array data.
/// </summary>

/// <summary>
/// An extended keyframe controller.
/// </summary>

/// <summary>
/// DEPRECATED (10.2), RENAMED (10.2) to NiTransformData.
/// Wrapper for transformation animation keys.
/// </summary>

[Flags]
public enum LookAtFlags {
    LOOK_FLIP = 0,
    LOOK_Y_AXIS = 1,
    LOOK_Z_AXIS = 2,
}

/// <summary>
/// DEPRECATED (10.2), REMOVED (20.5)
/// Replaced by NiTransformController and NiLookAtInterpolator.
/// </summary>

/// <summary>
/// NiLookAtInterpolator rotates an object so that it always faces a target object.
/// </summary>

/// <summary>
/// Describes the surface properties of an object e.g. translucency, ambient color, diffuse color, emissive color, and specular color.
/// </summary>

/// <summary>
/// DEPRECATED (20.5), replaced by NiMorphMeshModifier.
/// Geometry morphing data.
/// </summary>

/// <summary>
/// Generic node object for grouping.
/// </summary>

/// <summary>
/// A NiNode used as a skeleton bone?
/// </summary>

/// <summary>
/// Morrowind specific.
/// </summary>

/// <summary>
/// Firaxis-specific UI widgets?
/// </summary>

/// <summary>
/// Unknown.
/// </summary>

/// <summary>
/// Unknown.
/// </summary>

/// <summary>
/// These nodes will always be rotated to face the camera creating a billboard effect for any attached objects.
/// 
/// In pre-10.1.0.0 the Flags field is used for BillboardMode.
/// Bit 0: hidden
/// Bits 1-2: collision mode
/// Bit 3: unknown (set in most official meshes)
/// Bits 5-6: billboard mode
/// 
/// Collision modes:
/// 00 NONE
/// 01 USE_TRIANGLES
/// 10 USE_OBBS
/// 11 CONTINUE
/// 
/// Billboard modes:
/// 00 ALWAYS_FACE_CAMERA
/// 01 ROTATE_ABOUT_UP
/// 10 RIGID_FACE_CAMERA
/// 11 ALWAYS_FACE_CENTER
/// </summary>

/// <summary>
/// Bethesda-specific extension of Node with animation properties stored in the flags, often 42?
/// </summary>

/// <summary>
/// Unknown.
/// </summary>

/// <summary>
/// Flags for NiSwitchNode.
/// </summary>
[Flags]
public enum NiSwitchFlags {
    UpdateOnlyActiveChild = 0,
    UpdateControllers = 1,
}

/// <summary>
/// Represents groups of multiple scenegraph subtrees, only one of which (the "active child") is drawn at any given time.
/// </summary>

/// <summary>
/// Level of detail selector. Links to different levels of detail of the same model, used to switch a geometry at a specified distance.
/// </summary>

/// <summary>
/// NiPalette objects represent mappings from 8-bit indices to 24-bit RGB or 32-bit RGBA colors.
/// </summary>

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>

/// <summary>
/// Generic particle system node.
/// </summary>

/// <summary>
/// LEGACY (pre-10.1). NiParticles which do not house normals and generate them at runtime.
/// </summary>

/// <summary>
/// LEGACY (pre-10.1). Particle meshes.
/// </summary>

/// <summary>
/// LEGACY (pre-10.1). Particle meshes data.
/// </summary>

/// <summary>
/// A particle system.
/// </summary>

/// <summary>
/// Particle system.
/// </summary>

/// <summary>
/// A generic particle system time controller object.
/// </summary>

/// <summary>
/// A particle system controller, used by BS in conjunction with NiBSParticleNode.
/// </summary>

/// <summary>
/// DEPRECATED (10.2), REMOVED (20.5). Replaced by NiTransformController and NiPathInterpolator.
/// Time controller for a path.
/// </summary>




/// <summary>
/// A texture.
/// </summary>

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>

/// <summary>
/// A point light.
/// </summary>

/// <summary>
/// Abstract base class for dynamic effects such as NiLights or projected texture effects.
/// </summary>

/// <summary>
/// Abstract base class that represents light sources in a scene graph.
/// For Bethesda Stream 130 (FO4), NiLight now directly inherits from NiAVObject.
/// </summary>

/// <summary>
/// A deferred point light. Custom (Twin Saga).
/// </summary>

/// <summary>
/// Wrapper for position animation keys.
/// </summary>

/// <summary>
/// Wrapper for rotation animation keys.
/// </summary>

/// <summary>
/// Particle modifier that controls and updates the age of particles in the system.
/// </summary>

/// <summary>
/// Particle modifier that applies an explosive force to particles.
/// </summary>

/// <summary>
/// Particle modifier that creates and updates bound volumes.
/// </summary>

/// <summary>
/// Particle emitter that uses points within a defined Box shape to emit from.
/// </summary>

/// <summary>
/// Particle modifier that adds a defined shape to act as a collision object for particles to interact with.
/// </summary>

/// <summary>
/// Particle modifier that adds keyframe data to modify color/alpha values of particles over time.
/// </summary>

/// <summary>
/// Particle emitter that uses points within a defined Cylinder shape to emit from.
/// </summary>

/// <summary>
/// Particle modifier that applies a linear drag force to particles.
/// </summary>

/// <summary>
/// DEPRECATED (10.2). Particle system emitter controller data.
/// </summary>

/// <summary>
/// Particle modifier that applies a gravitational force to particles.
/// </summary>

/// <summary>
/// Particle modifier that controls the time it takes to grow and shrink a particle.
/// </summary>

/// <summary>
/// Particle emitter that uses points on a specified mesh to emit from.
/// </summary>

/// <summary>
/// Particle modifier that updates mesh particles using the age of each particle.
/// </summary>




/// <summary>
/// Similar to a Flip Controller, this handles particle texture animation on a single texture atlas
/// </summary>

/// <summary>
/// Particle Collider object which particles will interact with.
/// </summary>

/// <summary>
/// Particle Collider object which particles will interact with.
/// </summary>

/// <summary>
/// Particle modifier that updates the particle positions based on velocity and last update time.
/// </summary>

/// <summary>
/// Particle modifier that calls reset on a target upon looping.
/// </summary>

/// <summary>
/// Particle modifier that adds rotations to particles.
/// </summary>

/// <summary>
/// Particle modifier that spawns additional copies of a particle.
/// </summary>

/// <summary>
/// Particle emitter that uses points within a sphere shape to emit from.
/// </summary>

/// <summary>
/// Particle system controller, tells the system to update its simulation.
/// </summary>

/// <summary>
/// Base for all force field particle modifiers.
/// </summary>

/// <summary>
/// Particle system modifier, implements a vortex field force for particles.
/// </summary>

/// <summary>
/// Particle system modifier, implements a gravity field force for particles.
/// </summary>

/// <summary>
/// Particle system modifier, implements a drag field force for particles.
/// </summary>

/// <summary>
/// Particle system modifier, implements a turbulence field force for particles.
/// </summary>



/// <summary>
/// Particle system controller for force field magnitude.
/// </summary>

/// <summary>
/// Particle system controller for force field attenuation.
/// </summary>

/// <summary>
/// Particle system controller for force field maximum distance.
/// </summary>

/// <summary>
/// Particle system controller for air field air friction.
/// </summary>

/// <summary>
/// Particle system controller for air field inherit velocity.
/// </summary>

/// <summary>
/// Particle system controller for air field spread.
/// </summary>

/// <summary>
/// Particle system controller for emitter initial rotation speed.
/// </summary>

/// <summary>
/// Particle system controller for emitter initial rotation speed variation.
/// </summary>

/// <summary>
/// Particle system controller for emitter initial rotation angle.
/// </summary>

/// <summary>
/// Particle system controller for emitter initial rotation angle variation.
/// </summary>

/// <summary>
/// Particle system controller for emitter planar angle.
/// </summary>

/// <summary>
/// Particle system controller for emitter planar angle variation.
/// </summary>

/// <summary>
/// Particle system modifier, updates the particle velocity to simulate the effects of air movements like wind, fans, or wake.
/// </summary>

/// <summary>
/// Guild 2-Specific node
/// </summary>

/// <summary>
/// Unknown controller
/// </summary>

/// <summary>
/// Particle system modifier, updates the particle velocity to simulate the effects of point gravity.
/// </summary>

/// <summary>
/// Abstract class used for different types of LOD selections.
/// </summary>

/// <summary>
/// NiRangeLODData controls switching LOD levels based on Z depth from the camera to the NiLODNode.
/// </summary>

/// <summary>
/// NiScreenLODData controls switching LOD levels based on proportion of the screen that a bound would include.
/// </summary>

/// <summary>
/// Unknown.
/// </summary>

/// <summary>
/// DEPRECATED (pre-10.1), REMOVED (20.3).
/// Keyframe animation root node, in .kf files.
/// </summary>

/// <summary>
/// Determines whether flat shading or smooth shading is used on a shape.
/// </summary>

/// <summary>
/// Skinning data.
/// </summary>

/// <summary>
/// Skinning instance.
/// </summary>

/// <summary>
/// Old version of skinning instance.
/// </summary>

/// <summary>
/// A copy of NISkinInstance for use with NiClod meshes.
/// </summary>

/// <summary>
/// Skinning data, optimized for hardware skinning. The mesh is partitioned in submeshes such that each vertex of a submesh is influenced only by a limited and fixed number of bones.
/// </summary>

/// <summary>
/// A texture.
/// </summary>

/// <summary>
/// NiTexture::FormatPrefs. These preferences are a request to the renderer to use a format the most closely matches the settings and may be ignored.
/// </summary>

/// <summary>
/// Describes texture source and properties.
/// </summary>

/// <summary>
/// Gives specularity to a shape. Flags 0x0001.
/// </summary>

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>

/// <summary>
/// A spot.
/// </summary>

/// <summary>
/// Allows control of stencil testing.
/// </summary>

/// <summary>
/// Apparently commands for an optimizer instructing it to keep things it would normally discard.
/// Also refers to NiNode objects (through their name) in animation .kf files.
/// </summary>

/// <summary>
/// List of 0x00-seperated strings, which are names of controlled objects and controller types. Used in .kf files in conjunction with NiControllerSequence.
/// </summary>

/// <summary>
/// List of strings; for example, a list of all bone names.
/// </summary>

/// <summary>
/// Extra data, used to name different animation sequences.
/// </summary>

/// <summary>
/// Represents an effect that uses projected textures such as projected lights (gobos), environment maps, and fog maps.
/// </summary>

/// <summary>
/// LEGACY (pre-10.1)
/// </summary>

/// <summary>
/// LEGACY (pre-10.1)
/// </summary>

/// <summary>
/// LEGACY (pre-10.1)
/// </summary>

/// <summary>
/// Describes how a fragment shader should be configured for a given piece of geometry.
/// </summary>


/// <summary>
/// Wrapper for transformation animation keys.
/// </summary>

/// <summary>
/// A shape node that refers to singular triangle data.
/// </summary>

/// <summary>
/// Holds mesh data using a list of singular triangles.
/// </summary>

/// <summary>
/// A shape node that refers to data organized into strips of triangles
/// </summary>

/// <summary>
/// Holds mesh data using strips of triangles.
/// </summary>

/// <summary>
/// Unknown
/// </summary>

/// <summary>
/// Holds mesh data using a list of singular triangles.
/// </summary>

/// <summary>
/// LEGACY (pre-10.1)
/// Sub data of NiBezierMesh
/// </summary>

/// <summary>
/// LEGACY (pre-10.1)
/// Unknown
/// </summary>

/// <summary>
/// A shape node that holds continuous level of detail information.
/// Seems to be specific to Freedom Force.
/// </summary>

/// <summary>
/// Holds mesh data for continuous level of detail shapes.
/// Pesumably a progressive mesh with triangles specified by edge splits.
/// Seems to be specific to Freedom Force.
/// The structure of this is uncertain and highly experimental at this point.
/// </summary>

/// <summary>
/// DEPRECATED (pre-10.1), REMOVED (20.3).
/// Time controller for texture coordinates.
/// </summary>

/// <summary>
/// DEPRECATED (pre-10.1), REMOVED (20.3)
/// Texture coordinate data.
/// </summary>

/// <summary>
/// DEPRECATED (20.5).
/// Extra data in the form of a vector (as x, y, z, w components).
/// </summary>

/// <summary>
/// Property of vertex colors. This object is referred to by the root object of the NIF file whenever some NiTriShapeData object has vertex colors with non-default settings; if not present, vertex colors have vertex_mode=2 and lighting_mode=1.
/// </summary>

/// <summary>
/// DEPRECATED (10.x), REMOVED (?)
/// Not used in skinning.
/// Unsure of use - perhaps for morphing animation or gravity.
/// </summary>

/// <summary>
/// DEPRECATED (10.2), REMOVED (?), Replaced by NiBoolData.
/// Visibility data for a controller.
/// </summary>

/// <summary>
/// Allows applications to switch between drawing solid geometry or wireframe outlines.
/// </summary>

/// <summary>
/// Allows applications to set the test and write modes of the renderer's Z-buffer and to set the comparison function used for the Z-buffer test.
/// </summary>

/// <summary>
/// Morrowind-specific node for collision mesh.
/// </summary>

/// <summary>
/// LEGACY (pre-10.1)
/// Raw image data.
/// </summary>


/// <summary>
/// Used to turn sorting off for individual subtrees in a scene. Useful if objects must be drawn in a fixed order.
/// </summary>

/// <summary>
/// Represents cube maps that are created from either a set of six image files, six blocks of pixel data, or a single pixel data with six faces.
/// </summary>

/// <summary>
/// A PhysX prop which holds information about PhysX actors in a Gamebryo scene
/// </summary>



/// <summary>
/// For serialization of PhysX objects and to attach them to the scene.
/// </summary>

/// <summary>
/// For serializing NxActor objects.
/// </summary>


/// <summary>
/// For serializing NxBodyDesc objects.
/// </summary>

public enum NxJointType {
    NX_JOINT_PRISMATIC = 0,
    NX_JOINT_REVOLUTE = 1,
    NX_JOINT_CYLINDRICAL = 2,
    NX_JOINT_SPHERICAL = 3,
    NX_JOINT_POINT_ON_LINE = 4,
    NX_JOINT_POINT_IN_PLANE = 5,
    NX_JOINT_DISTANCE = 6,
    NX_JOINT_PULLEY = 7,
    NX_JOINT_FIXED = 8,
    NX_JOINT_D6 = 9,
}

public enum NxD6JointMotion {
    NX_D6JOINT_MOTION_LOCKED = 0,
    NX_D6JOINT_MOTION_LIMITED = 1,
    NX_D6JOINT_MOTION_FREE = 2,
}

public enum NxD6JointDriveType {
    NX_D6JOINT_DRIVE_POSITION = 1,
    NX_D6JOINT_DRIVE_VELOCITY = 2,
}

public enum NxJointProjectionMode {
    NX_JPM_NONE = 0,
    NX_JPM_POINT_MINDIST = 1,
    NX_JPM_LINEAR_MINDIST = 2,
}





/// <summary>
/// A PhysX Joint abstract base class.
/// </summary>

/// <summary>
/// A 6DOF (6 degrees of freedom) joint.
/// </summary>

public enum NxShapeType {
    NX_SHAPE_PLANE = 0,
    NX_SHAPE_SPHERE = 1,
    NX_SHAPE_BOX = 2,
    NX_SHAPE_CAPSULE = 3,
    NX_SHAPE_WHEEL = 4,
    NX_SHAPE_CONVEX = 5,
    NX_SHAPE_MESH = 6,
    NX_SHAPE_HEIGHTFIELD = 7,
    NX_SHAPE_RAW_MESH = 8,
    NX_SHAPE_COMPOUND = 9,
}



/// <summary>
/// For serializing NxShapeDesc objects
/// </summary>

/// <summary>
/// Holds mesh data for streaming.
/// </summary>

[Flags]
public enum NxMaterialFlag {
    NX_MF_ANISOTROPIC = 1,
    NX_MF_DUMMY1 = 2,
    NX_MF_DUMMY2 = 3,
    NX_MF_DUMMY3 = 4,
    NX_MF_DISABLE_FRICTION = 5,
    NX_MF_DISABLE_STRONG_FRICTION = 6,
}


public enum NxCombineMode {
    NX_CM_AVERAGE = 0,
    NX_CM_MIN = 1,
    NX_CM_MULTIPLY = 2,
    NX_CM_MAX = 3,
}


/// <summary>
/// For serializing NxMaterialDesc objects.
/// </summary>

/// <summary>
/// A destination is a link between a PhysX actor and a Gamebryo object being driven by the physics.
/// </summary>

/// <summary>
/// Base for destinations that set a rigid body state.
/// </summary>

/// <summary>
/// Connects PhysX rigid body actors to a scene node.
/// </summary>

/// <summary>
/// A source is a link between a Gamebryo object and a PhysX actor.
/// </summary>

/// <summary>
/// Sets state of a rigid body PhysX actor.
/// </summary>

/// <summary>
/// Sets state of kinematic PhysX actor.
/// </summary>

/// <summary>
/// Sends Gamebryo scene state to a PhysX dynamic actor.
/// </summary>

/// <summary>
/// Wireframe geometry.
/// </summary>

/// <summary>
/// Wireframe geometry data.
/// </summary>

/// <summary>
/// Two dimensional screen elements.
/// </summary>

/// <summary>
/// DEPRECATED (20.5), functionality included in NiMeshScreenElements.
/// Two dimensional screen elements.
/// </summary>

/// <summary>
/// DEPRECATED (20.5), replaced by NiMeshScreenElements.
/// Two dimensional screen elements.
/// </summary>

/// <summary>
/// NiRoomGroup represents a set of connected rooms i.e. a game level.
/// </summary>

/// <summary>
/// NiRoom objects represent cells in a cell-portal culling system.
/// </summary>

/// <summary>
/// NiPortal objects are grouping nodes that support aggressive visibility culling.
/// They represent flat polygonal regions through which a part of a scene graph can be viewed.
/// </summary>

/// <summary>
/// Bethesda-specific fade node.
/// </summary>

/// <summary>
/// The type of animation interpolation (blending) that will be used on the associated key frames.
/// </summary>
public enum BSShaderType {
    SHADER_TALL_GRASS = 0,
    SHADER_DEFAULT = 1,
    SHADER_SKY = 10,
    SHADER_SKIN = 14,
    SHADER_WATER = 17,
    SHADER_LIGHTING30 = 29,
    SHADER_TILE = 32,
    SHADER_NOLIGHTING = 33,
}

/// <summary>
/// Shader Property Flags
/// </summary>
[Flags]
public enum BSShaderFlags {
    Specular = 0,
    Skinned = 1,
    LowDetail = 2,
    Vertex_Alpha = 3,
    Unknown_1 = 4,
    Single_Pass = 5,
    Empty = 6,
    Environment_Mapping = 7,
    Alpha_Texture = 8,
    Unknown_2 = 9,
    FaceGen = 10,
    Parallax_Shader_Index_15 = 11,
    Unknown_3 = 12,
    Non_Projective_Shadows = 13,
    Unknown_4 = 14,
    Refraction = 15,
    Fire_Refraction = 16,
    Eye_Environment_Mapping = 17,
    Hair = 18,
    Dynamic_Alpha = 19,
    Localmap_Hide_Secret = 20,
    Window_Environment_Mapping = 21,
    Tree_Billboard = 22,
    Shadow_Frustum = 23,
    Multiple_Textures = 24,
    Remappable_Textures = 25,
    Decal_Single_Pass = 26,
    Dynamic_Decal_Single_Pass = 27,
    Parallax_Occulsion = 28,
    External_Emittance = 29,
    Shadow_Map = 30,
    ZBuffer_Test = 31,
}

/// <summary>
/// Shader Property Flags 2
/// </summary>
[Flags]
public enum BSShaderFlags2 {
    ZBuffer_Write = 0,
    LOD_Landscape = 1,
    LOD_Building = 2,
    No_Fade = 3,
    Refraction_Tint = 4,
    Vertex_Colors = 5,
    Unknown1 = 6,
    1st_Light_is_Point_Light = 7,
    2nd_Light = 8,
    3rd_Light = 9,
    Vertex_Lighting = 10,
    Uniform_Scale = 11,
    Fit_Slope = 12,
    Billboard_and_Envmap_Light_Fade = 13,
    No_LOD_Land_Blend = 14,
    Envmap_Light_Fade = 15,
    Wireframe = 16,
    VATS_Selection = 17,
    Show_in_Local_Map = 18,
    Premult_Alpha = 19,
    Skip_Normal_Maps = 20,
    Alpha_Decal = 21,
    No_Transparecny_Multisampling = 22,
    Unknown2 = 23,
    Unknown3 = 24,
    Unknown4 = 25,
    Unknown5 = 26,
    Unknown6 = 27,
    Unknown7 = 28,
    Unknown8 = 29,
    Unknown9 = 30,
    Unknown10 = 31,
}

/// <summary>
/// Bethesda-specific property.
/// </summary>

/// <summary>
/// Bethesda-specific property.
/// </summary>

/// <summary>
/// Bethesda-specific property.
/// </summary>

/// <summary>
/// Bethesda-specific property.
/// </summary>

/// <summary>
/// This controller is used to animate float variables in BSEffectShaderProperty.
/// </summary>

/// <summary>
/// This controller is used to animate colors in BSEffectShaderProperty.
/// </summary>

/// <summary>
/// This controller is used to animate float variables in BSLightingShaderProperty.
/// </summary>

/// <summary>
/// This controller is used to animate colors in BSLightingShaderProperty.
/// </summary>


/// <summary>
/// Skyrim, Paired with dummy TriShapes, this controller generates lightning shapes for special effects.
///     First interpolator controls Generation.
/// </summary>

/// <summary>
/// Bethesda-specific Texture Set.
/// </summary>

/// <summary>
/// Bethesda-specific property. Found in Fallout3
/// </summary>

/// <summary>
/// Sets what sky function this object fulfills in BSSkyShaderProperty or SkyShaderProperty.
/// </summary>
public enum SkyObjectType {
    BSSM_SKY_TEXTURE = 0,
    BSSM_SKY_SUNGLARE = 1,
    BSSM_SKY = 2,
    BSSM_SKY_CLOUDS = 3,
    BSSM_SKY_STARS = 5,
    BSSM_SKY_MOON_STARS_MASK = 7,
}

/// <summary>
/// Bethesda-specific property. Found in Fallout3
/// </summary>

/// <summary>
/// Bethesda-specific property.
/// </summary>

/// <summary>
/// Bethesda-specific property.
/// </summary>

/// <summary>
/// Bethesda-specific property.
/// </summary>

/// <summary>
/// Bethesda-specific property.
/// </summary>

/// <summary>
/// Bethesda-specific property.
/// </summary>

/// <summary>
/// Bethesda-specific property.
/// </summary>

/// <summary>
/// Bethesda-specific property.
/// </summary>

/// <summary>
/// Skyrim Shader Property Flags 1
/// </summary>
[Flags]
public enum SkyrimShaderPropertyFlags1 {
    Specular = 0,
    Skinned = 1,
    Temp_Refraction = 2,
    Vertex_Alpha = 3,
    Greyscale_To_PaletteColor = 4,
    Greyscale_To_PaletteAlpha = 5,
    Use_Falloff = 6,
    Environment_Mapping = 7,
    Recieve_Shadows = 8,
    Cast_Shadows = 9,
    Facegen_Detail_Map = 10,
    Parallax = 11,
    Model_Space_Normals = 12,
    Non_Projective_Shadows = 13,
    Landscape = 14,
    Refraction = 15,
    Fire_Refraction = 16,
    Eye_Environment_Mapping = 17,
    Hair_Soft_Lighting = 18,
    Screendoor_Alpha_Fade = 19,
    Localmap_Hide_Secret = 20,
    FaceGen_RGB_Tint = 21,
    Own_Emit = 22,
    Projected_UV = 23,
    Multiple_Textures = 24,
    Remappable_Textures = 25,
    Decal = 26,
    Dynamic_Decal = 27,
    Parallax_Occlusion = 28,
    External_Emittance = 29,
    Soft_Effect = 30,
    ZBuffer_Test = 31,
}

/// <summary>
/// Skyrim Shader Property Flags 2
/// </summary>
[Flags]
public enum SkyrimShaderPropertyFlags2 {
    ZBuffer_Write = 0,
    LOD_Landscape = 1,
    LOD_Objects = 2,
    No_Fade = 3,
    Double_Sided = 4,
    Vertex_Colors = 5,
    Glow_Map = 6,
    Assume_Shadowmask = 7,
    Packed_Tangent = 8,
    Multi_Index_Snow = 9,
    Vertex_Lighting = 10,
    Uniform_Scale = 11,
    Fit_Slope = 12,
    Billboard = 13,
    No_LOD_Land_Blend = 14,
    EnvMap_Light_Fade = 15,
    Wireframe = 16,
    Weapon_Blood = 17,
    Hide_On_Local_Map = 18,
    Premult_Alpha = 19,
    Cloud_LOD = 20,
    Anisotropic_Lighting = 21,
    No_Transparency_Multisampling = 22,
    Unused01 = 23,
    Multi_Layer_Parallax = 24,
    Soft_Lighting = 25,
    Rim_Lighting = 26,
    Back_Lighting = 27,
    Unused02 = 28,
    Tree_Anim = 29,
    Effect_Lighting = 30,
    HD_LOD_Objects = 31,
}

/// <summary>
/// Fallout 4 Shader Property Flags 1
/// </summary>
[Flags]
public enum Fallout4ShaderPropertyFlags1 {
    Specular = 0,
    Skinned = 1,
    Temp_Refraction = 2,
    Vertex_Alpha = 3,
    GreyscaleToPalette_Color = 4,
    GreyscaleToPalette_Alpha = 5,
    Use_Falloff = 6,
    Environment_Mapping = 7,
    RGB_Falloff = 8,
    Cast_Shadows = 9,
    Face = 10,
    UI_Mask_Rects = 11,
    Model_Space_Normals = 12,
    Non_Projective_Shadows = 13,
    Landscape = 14,
    Refraction = 15,
    Fire_Refraction = 16,
    Eye_Environment_Mapping = 17,
    Hair = 18,
    Screendoor_Alpha_Fade = 19,
    Localmap_Hide_Secret = 20,
    Skin_Tint = 21,
    Own_Emit = 22,
    Projected_UV = 23,
    Multiple_Textures = 24,
    Tessellate = 25,
    Decal = 26,
    Dynamic_Decal = 27,
    Character_Lighting = 28,
    External_Emittance = 29,
    Soft_Effect = 30,
    ZBuffer_Test = 31,
}

/// <summary>
/// Fallout 4 Shader Property Flags 2
/// </summary>
[Flags]
public enum Fallout4ShaderPropertyFlags2 {
    ZBuffer_Write = 0,
    LOD_Landscape = 1,
    LOD_Objects = 2,
    No_Fade = 3,
    Double_Sided = 4,
    Vertex_Colors = 5,
    Glow_Map = 6,
    Transform_Changed = 7,
    Dismemberment_Meatcuff = 8,
    Tint = 9,
    Grass_Vertex_Lighting = 10,
    Grass_Uniform_Scale = 11,
    Grass_Fit_Slope = 12,
    Grass_Billboard = 13,
    No_LOD_Land_Blend = 14,
    Dismemberment = 15,
    Wireframe = 16,
    Weapon_Blood = 17,
    Hide_On_Local_Map = 18,
    Premult_Alpha = 19,
    VATS_Target = 20,
    Anisotropic_Lighting = 21,
    Skew_Specular_Alpha = 22,
    Menu_Screen = 23,
    Multi_Layer_Parallax = 24,
    Alpha_Test = 25,
    Gradient_Remap = 26,
    VATS_Target_Draw_All = 27,
    Pipboy_Screen = 28,
    Tree_Anim = 29,
    Effect_Lighting = 30,
    Refraction_Writes_Depth = 31,
}

/// <summary>
/// Bethesda shader property for Skyrim and later.
/// </summary>

/// <summary>
/// Bethesda effect shader property for Skyrim and later.
/// </summary>

/// <summary>
/// Skyrim water shader property flags
/// </summary>
[Flags]
public enum SkyrimWaterShaderFlags {
    SWSF1_UNKNOWN0 = 0,
    SWSF1_Bypass_Refraction_Map = 1,
    SWSF1_Water_Toggle = 2,
    SWSF1_UNKNOWN3 = 3,
    SWSF1_UNKNOWN4 = 4,
    SWSF1_UNKNOWN5 = 5,
    SWSF1_Highlight_Layer_Toggle = 6,
    SWSF1_Enabled = 7,
}

/// <summary>
/// Skyrim water shader property, different from "WaterShaderProperty" seen in Fallout.
/// </summary>

/// <summary>
/// Skyrim Sky shader block.
/// </summary>

/// <summary>
/// Bethesda-specific skin instance.
/// </summary>

/// <summary>
/// Bethesda-specific extra data. Lists locations and normals on a mesh that are appropriate for decal placement.
/// </summary>

/// <summary>
/// Bethesda-specific particle modifier.
/// </summary>

/// <summary>
/// Flags for BSValueNode.
/// </summary>
[Flags]
public enum BSValueNodeFlags {
    BillboardWorldZ = 0,
    UsePlayerAdjust = 1,
}

/// <summary>
/// Bethesda-specific node. Found on fxFire effects
/// </summary>

/// <summary>
/// Bethesda-Specific (mesh?) Particle System.
/// </summary>

/// <summary>
/// Bethesda-Specific (mesh?) Particle System Data.
/// </summary>

/// <summary>
/// Bethesda-Specific (mesh?) Particle System Modifier.
/// </summary>

/// <summary>
/// Bethesda-Specific time controller.
/// </summary>

/// <summary>
/// Bethesda-Specific particle system.
/// </summary>

/// <summary>
/// Particle system (multi?) emitter controller.
/// </summary>

/// <summary>
/// Bethesda-Specific time controller.
/// </summary>

/// <summary>
/// Bethesda-Specific node.
/// </summary>

/// <summary>
/// Bethesda-Specific node.
/// </summary>

/// <summary>
/// Bethesda-Specific node.
/// </summary>

/// <summary>
/// Bethesda-Specific node.
/// </summary>

/// <summary>
/// Bethesda-specific time controller.
/// </summary>

/// <summary>
/// A havok shape.
/// A list of convex shapes.
/// 
/// Do not put a bhkPackedNiTriStripsShape in the Sub Shapes. Use a
/// separate collision nodes without a list shape for those.
/// 
/// Also, shapes collected in a bhkListShape may not have the correct
/// walking noise, so only use it for non-walkable objects.
/// </summary>

/// <summary>
/// Bethesda-specific compound.
/// </summary>

/// <summary>
/// Bethesda-specific interpolator.
/// </summary>

/// <summary>
/// Anim note types.
/// </summary>
public enum AnimNoteType {
    ANT_INVALID = 0,
    ANT_GRABIK = 1,
    ANT_LOOKIK = 2,
}

/// <summary>
/// Bethesda-specific object.
/// </summary>

/// <summary>
/// Bethesda-specific object.
/// </summary>

/// <summary>
/// Bethesda-specific Havok serializable.
/// </summary>

/// <summary>
/// Culling modes for multi bound nodes.
/// </summary>
public enum BSCPCullingType {
    BSCP_CULL_NORMAL = 0,
    BSCP_CULL_ALLPASS = 1,
    BSCP_CULL_ALLFAIL = 2,
    BSCP_CULL_IGNOREMULTIBOUNDS = 3,
    BSCP_CULL_FORCEMULTIBOUNDSNOUPDATE = 4,
}

/// <summary>
/// Bethesda-specific node.
/// </summary>

/// <summary>
/// Bethesda-specific object.
/// </summary>

/// <summary>
/// Abstract base type for bounding data.
/// </summary>

/// <summary>
/// Oriented bounding box.
/// </summary>

/// <summary>
/// Bethesda-specific object.
/// </summary>

/// <summary>
/// This is only defined because of recursion issues.
/// </summary>

/// <summary>
/// Bethesda-specific. Describes groups of triangles either segmented in a grid (for LOD) or by body part for skinned FO4 meshes.
/// </summary>

/// <summary>
/// Bethesda-specific AV object.
/// </summary>

/// <summary>
/// Bethesda-specific object.
/// </summary>






/// <summary>
/// Bethesda-specific extra data.
/// </summary>

/// <summary>
/// Bethesda-specific Havok serializable.
/// </summary>

/// <summary>
/// Bethesda-specific time controller.
/// </summary>

/// <summary>
/// Bethesda-Specific node.
/// </summary>

/// <summary>
/// A breakable constraint.
/// </summary>

/// <summary>
/// Bethesda-Specific Havok serializable.
/// </summary>

/// <summary>
/// Found in Fallout 3 .psa files, extra ragdoll info for NPCs/creatures. (usually idleanims\deathposes.psa)
/// Defines different kill poses. The game selects the pose randomly and applies it to a skeleton immediately upon ragdolling.
/// Poses can be previewed in GECK Object Window-Actor Data-Ragdoll and selecting Pose Matching tab.
/// </summary>

/// <summary>
/// Found in Fallout 3, more ragdoll info?  (meshes\ragdollconstraint\*.rdt)
/// </summary>

/// <summary>
/// Data for bhkRagdollTemplate
/// </summary>

/// <summary>
/// A range of indices, which make up a region (such as a submesh).
/// </summary>

/// <summary>
/// Sets how objects are to be cloned.
/// </summary>
public enum CloningBehavior {
    CLONING_SHARE = 0,
    CLONING_COPY = 1,
    CLONING_BLANK_COPY = 2,
}

/// <summary>
/// The data format of components.
/// </summary>
public enum ComponentFormat {
    F_UNKNOWN = 0x00000000,
    F_INT8_1 = 0x00010101,
    F_INT8_2 = 0x00020102,
    F_INT8_3 = 0x00030103,
    F_INT8_4 = 0x00040104,
    F_UINT8_1 = 0x00010105,
    F_UINT8_2 = 0x00020106,
    F_UINT8_3 = 0x00030107,
    F_UINT8_4 = 0x00040108,
    F_NORMINT8_1 = 0x00010109,
    F_NORMINT8_2 = 0x0002010A,
    F_NORMINT8_3 = 0x0003010B,
    F_NORMINT8_4 = 0x0004010C,
    F_NORMUINT8_1 = 0x0001010D,
    F_NORMUINT8_2 = 0x0002010E,
    F_NORMUINT8_3 = 0x0003010F,
    F_NORMUINT8_4 = 0x00040110,
    F_INT16_1 = 0x00010211,
    F_INT16_2 = 0x00020212,
    F_INT16_3 = 0x00030213,
    F_INT16_4 = 0x00040214,
    F_UINT16_1 = 0x00010215,
    F_UINT16_2 = 0x00020216,
    F_UINT16_3 = 0x00030217,
    F_UINT16_4 = 0x00040218,
    F_NORMINT16_1 = 0x00010219,
    F_NORMINT16_2 = 0x0002021A,
    F_NORMINT16_3 = 0x0003021B,
    F_NORMINT16_4 = 0x0004021C,
    F_NORMUINT16_1 = 0x0001021D,
    F_NORMUINT16_2 = 0x0002021E,
    F_NORMUINT16_3 = 0x0003021F,
    F_NORMUINT16_4 = 0x00040220,
    F_INT32_1 = 0x00010421,
    F_INT32_2 = 0x00020422,
    F_INT32_3 = 0x00030423,
    F_INT32_4 = 0x00040424,
    F_UINT32_1 = 0x00010425,
    F_UINT32_2 = 0x00020426,
    F_UINT32_3 = 0x00030427,
    F_UINT32_4 = 0x00040428,
    F_NORMINT32_1 = 0x00010429,
    F_NORMINT32_2 = 0x0002042A,
    F_NORMINT32_3 = 0x0003042B,
    F_NORMINT32_4 = 0x0004042C,
    F_NORMUINT32_1 = 0x0001042D,
    F_NORMUINT32_2 = 0x0002042E,
    F_NORMUINT32_3 = 0x0003042F,
    F_NORMUINT32_4 = 0x00040430,
    F_FLOAT16_1 = 0x00010231,
    F_FLOAT16_2 = 0x00020232,
    F_FLOAT16_3 = 0x00030233,
    F_FLOAT16_4 = 0x00040234,
    F_FLOAT32_1 = 0x00010435,
    F_FLOAT32_2 = 0x00020436,
    F_FLOAT32_3 = 0x00030437,
    F_FLOAT32_4 = 0x00040438,
    F_UINT_10_10_10_L1 = 0x00010439,
    F_NORMINT_10_10_10_L1 = 0x0001043A,
    F_NORMINT_11_11_10 = 0x0001043B,
    F_NORMUINT8_4_BGRA = 0x0004013C,
    F_NORMINT_10_10_10_2 = 0x0001043D,
    F_UINT_10_10_10_2 = 0x0001043E,
}

/// <summary>
/// Determines how a data stream is used?
/// </summary>
public enum DataStreamUsage {
    USAGE_VERTEX_INDEX = 0,
    USAGE_VERTEX = 1,
    USAGE_SHADER_CONSTANT = 2,
    USAGE_USER = 3,
}

/// <summary>
/// Determines how the data stream is accessed?
/// </summary>
[Flags]
public enum DataStreamAccess {
    CPU Read = 0,
    CPU Write Static = 1,
    CPU Write Mutable = 2,
    CPU Write Volatile = 3,
    GPU Read = 4,
    GPU Write = 5,
    CPU Write Static Inititialized = 6,
}




/// <summary>
/// An object that can be rendered.
/// </summary>

/// <summary>
/// Describes the type of primitives stored in a mesh object.
/// </summary>
public enum MeshPrimitiveType {
    MESH_PRIMITIVE_TRIANGLES = 0,
    MESH_PRIMITIVE_TRISTRIPS = 1,
    MESH_PRIMITIVE_LINES = 2,
    MESH_PRIMITIVE_LINESTRIPS = 3,
    MESH_PRIMITIVE_QUADS = 4,
    MESH_PRIMITIVE_POINTS = 5,
}

/// <summary>
/// A sync point corresponds to a particular stage in per-frame processing.
/// </summary>
public enum SyncPoint {
    SYNC_ANY = 0x8000,
    SYNC_UPDATE = 0x8010,
    SYNC_POST_UPDATE = 0x8020,
    SYNC_VISIBLE = 0x8030,
    SYNC_RENDER = 0x8040,
    SYNC_PHYSICS_SIMULATE = 0x8050,
    SYNC_PHYSICS_COMPLETED = 0x8060,
    SYNC_REFLECTIONS = 0x8070,
}

/// <summary>
/// Base class for mesh modifiers.
/// </summary>




/// <summary>
/// Manipulates a mesh with the semantic MORPHWEIGHTS using an NiMorphMeshModifier.
/// </summary>


/// <summary>
/// Performs linear-weighted blending between a set of target data streams.
/// </summary>


/// <summary>
/// An instance of a hardware-instanced mesh in a scene graph.
/// </summary>

/// <summary>
/// Mesh modifier that provides per-frame instancing capabilities in Gamebryo.
/// </summary>


/// <summary>
/// Defines the levels of detail for a given character and dictates the character's current LOD.
/// </summary>


/// <summary>
/// Describes the various methods that may be used to specify the orientation of the particles.
/// </summary>
public enum AlignMethod {
    ALIGN_INVALID = 0,
    ALIGN_PER_PARTICLE = 1,
    ALIGN_LOCAL_FIXED = 2,
    ALIGN_LOCAL_POSITION = 5,
    ALIGN_LOCAL_VELOCITY = 9,
    ALIGN_CAMERA = 16,
}

/// <summary>
/// Represents a particle system.
/// </summary>

/// <summary>
/// Represents a particle system that uses mesh particles instead of sprite-based particles.
/// </summary>

/// <summary>
/// A mesh modifier that uses particle system data to generate camera-facing quads.
/// </summary>

/// <summary>
/// A mesh modifier that uses particle system data to generate aligned quads for each particle.
/// </summary>

/// <summary>
/// The mesh modifier that performs all particle system simulation.
/// </summary>

/// <summary>
/// Abstract base class for a single step in the particle system simulation process.  It has no seralized data.
/// </summary>

public enum PSLoopBehavior {
    PS_LOOP_CLAMP_BIRTH = 0,
    PS_LOOP_CLAMP_DEATH = 1,
    PS_LOOP_AGESCALE = 2,
    PS_LOOP_LOOP = 3,
    PS_LOOP_REFLECT = 4,
}

/// <summary>
/// Encapsulates a floodgate kernel that updates particle size, colors, and rotations.
/// </summary>

/// <summary>
/// Encapsulates a floodgate kernel that simulates particle forces.
/// </summary>

/// <summary>
/// Encapsulates a floodgate kernel that simulates particle colliders.
/// </summary>

/// <summary>
/// Encapsulates a floodgate kernel that updates mesh particle alignment and transforms.
/// </summary>

/// <summary>
/// Encapsulates a floodgate kernel that updates particle positions and ages. As indicated by its name, this step should be attached last in the NiPSSimulator mesh modifier.
/// </summary>

/// <summary>
/// Updates the bounding volume for an NiPSParticleSystem object.
/// </summary>

/// <summary>
/// This is used by the Floodgate kernel to determine which NiPSForceHelpers functions to call.
/// </summary>
public enum PSForceType {
    FORCE_BOMB = 0,
    FORCE_DRAG = 1,
    FORCE_AIR_FIELD = 2,
    FORCE_DRAG_FIELD = 3,
    FORCE_GRAVITY_FIELD = 4,
    FORCE_RADIAL_FIELD = 5,
    FORCE_TURBULENCE_FIELD = 6,
    FORCE_VORTEX_FIELD = 7,
    FORCE_GRAVITY = 8,
}

/// <summary>
/// Abstract base class for all particle forces.
/// </summary>

/// <summary>
/// Applies a linear drag force to particles.
/// </summary>

/// <summary>
/// Applies a gravitational force to particles.
/// </summary>

/// <summary>
/// Applies an explosive force to particles.
/// </summary>

/// <summary>
/// Abstract base class for all particle emitters.
/// </summary>

/// <summary>
/// Abstract base class for particle emitters that emit particles from a volume.
/// </summary>

/// <summary>
/// A particle emitter that emits particles from a rectangular volume.
/// </summary>

/// <summary>
/// A particle emitter that emits particles from a spherical volume.
/// </summary>

/// <summary>
/// A particle emitter that emits particles from a cylindrical volume.
/// </summary>

/// <summary>
/// Emits particles from one or more NiMesh objects. A random mesh emitter is selected for each particle emission.
/// </summary>

/// <summary>
/// Abstract base class for all particle emitter time controllers.
/// </summary>

/// <summary>
/// Abstract base class for controllers that animate a floating point value on an NiPSEmitter object.
/// </summary>

/// <summary>
/// Animates particle emission and birth rate.
/// </summary>

/// <summary>
/// Abstract base class for all particle force time controllers.
/// </summary>

/// <summary>
/// Abstract base class for controllers that animate a Boolean value on an NiPSForce object.
/// </summary>

/// <summary>
/// Abstract base class for controllers that animate a floating point value on an NiPSForce object.
/// </summary>

/// <summary>
/// Animates whether or not an NiPSForce object is active.
/// </summary>

/// <summary>
/// Animates the strength value of an NiPSGravityForce object.
/// </summary>

/// <summary>
/// Animates the speed value on an NiPSEmitter object.
/// </summary>

/// <summary>
/// Animates the size value on an NiPSEmitter object.
/// </summary>

/// <summary>
/// Animates the declination value on an NiPSEmitter object.
/// </summary>

/// <summary>
/// Animates the declination variation value on an NiPSEmitter object.
/// </summary>

/// <summary>
/// Animates the planar angle value on an NiPSEmitter object.
/// </summary>

/// <summary>
/// Animates the planar angle variation value on an NiPSEmitter object.
/// </summary>

/// <summary>
/// Animates the rotation angle value on an NiPSEmitter object.
/// </summary>

/// <summary>
/// Animates the rotation angle variation value on an NiPSEmitter object.
/// </summary>

/// <summary>
/// Animates the rotation speed value on an NiPSEmitter object.
/// </summary>

/// <summary>
/// Animates the rotation speed variation value on an NiPSEmitter object.
/// </summary>

/// <summary>
/// Animates the lifespan value on an NiPSEmitter object.
/// </summary>

/// <summary>
/// Calls ResetParticleSystem on an NiPSParticleSystem target upon looping.
/// </summary>

/// <summary>
/// This is used by the Floodgate kernel to determine which NiPSColliderHelpers functions to call.
/// </summary>
public enum ColliderType {
    COLLIDER_PLANAR = 0,
    COLLIDER_SPHERICAL = 1,
}

/// <summary>
/// Abstract base class for all particle colliders.
/// </summary>

/// <summary>
/// A planar collider for particles.
/// </summary>

/// <summary>
/// A spherical collider for particles.
/// </summary>

/// <summary>
/// Creates a new particle whose initial parameters are based on an existing particle.
/// </summary>



























/// <summary>
/// Root node in Gamebryo .kf files (20.5.0.1 and up).
/// For 20.5.0.0, "NiSequenceData" is an alias for "NiControllerSequence" and this is not handled in nifxml.
/// This was not found in any 20.5.0.0 KFs available and they instead use NiControllerSequence directly.
/// </summary>

/// <summary>
/// An NiShadowGenerator object is attached to an NiDynamicEffect object to inform the shadowing system that the effect produces shadows.
/// </summary>



/// <summary>
/// Compressed collision mesh.
/// </summary>

/// <summary>
/// A compressed mesh shape for collision in Skyrim.
/// </summary>

/// <summary>
/// Orientation marker for Skyrim's inventory view.
/// How to show the nif in the player's inventory.
/// Typically attached to the root node of the nif tree.
/// If not present, then Skyrim will still show the nif in inventory,
/// using the default values.
/// Name should be 'INV' (without the quotes).
/// For rotations, a short of "4712" appears as "4.712" but "959" appears as "0.959"  meshes\weapons\daedric\daedricbowskinned.nif
/// </summary>

/// <summary>
/// Unknown
/// </summary>

/// <summary>
/// Links a nif with a Havok Behavior .hkx animation file
/// </summary>

/// <summary>
/// A controller that trails a bone behind an actor.
/// </summary>

/// <summary>
/// A variation on NiTriShape, for visibility control over vertex groups.
/// </summary>

/// <summary>
/// Furniture Marker for actors
/// </summary>

/// <summary>
/// Unknown, related to trees.
/// </summary>

/// <summary>
/// Node for handling Trees, Switches branch configurations for variation?
/// </summary>

/// <summary>
/// Fallout 4 Tri Shape
/// </summary>

/// <summary>
/// Fallout 4 LOD Tri Shape
/// </summary>



/// <summary>
/// Fallout 4 Sub-Index Tri Shape
/// </summary>

/// <summary>
/// Fallout 4 Physics System
/// </summary>

/// <summary>
/// Fallout 4 Collision Object
/// </summary>

/// <summary>
/// Fallout 4 Collision System
/// </summary>

/// <summary>
/// Fallout 4 Ragdoll System
/// </summary>

/// <summary>
/// Fallout 4 Extra Data
/// </summary>

/// <summary>
/// Fallout 4 Cloth data
/// </summary>

/// <summary>
/// Fallout 4 Bone Transform
/// </summary>

/// <summary>
/// Fallout 4 Skin Instance
/// </summary>

/// <summary>
/// Fallout 4 Bone Data
/// </summary>

/// <summary>
/// Fallout 4 Positional Data
/// </summary>


/// <summary>
/// Fallout 4 Item Slot Parent
/// </summary>

/// <summary>
/// Fallout 4 Item Slot Child
/// </summary>

/// <summary>
/// Fallout 4 Eye Center Data
/// </summary>



/// <summary>
/// This appears to be a 64-bit hash but nif.xml does not have a 64-bit type.
/// </summary>

/// <summary>
/// Fallout 4 Packed Combined Geometry Data.
/// Geometry is baked into the file and given a list of transforms to position each copy.
/// </summary>

/// <summary>
/// Fallout 4 Packed Combined Shared Geometry Data.
/// Geometry is NOT baked into the file. It is instead a reference to the shape via a Shape ID (currently undecoded)
/// which loads the geometry via the STAT form for the NIF.
/// </summary>




#endregion

/// <summary>
/// Large ref flag.
/// </summary>

