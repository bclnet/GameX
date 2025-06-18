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
public enum AccumFlags : uint {
    ACCUM_X_TRANS = 0,              // X Translation will be accumulated.
    ACCUM_Y_TRANS = 1 << 1,         // Y Translation will be accumulated.
    ACCUM_Z_TRANS = 1 << 2,         // Z Translation will be accumulated.
    ACCUM_X_ROT = 1 << 3,           // X Rotation will be accumulated.
    ACCUM_Y_ROT = 1 << 4,           // Y Rotation will be accumulated.
    ACCUM_Z_ROT = 1 << 5,           // Z Rotation will be accumulated.
    ACCUM_X_FRONT = 1 << 6,         // +X is front facing. (Default)
    ACCUM_Y_FRONT = 1 << 7,         // +Y is front facing.
    ACCUM_Z_FRONT = 1 << 8,         // +Z is front facing.
    ACCUM_NEG_FRONT = 1 << 9        // -X is front facing.
}

/// <summary>
/// Describes how the vertex colors are blended with the filtered texture color.
/// </summary>
public enum ApplyMode : uint {
    APPLY_REPLACE = 0,              // Replaces existing color
    APPLY_DECAL = 1,                // For placing images on the object like stickers.
    APPLY_MODULATE = 2,             // Modulates existing color. (Default)
    APPLY_HILIGHT = 3,              // PS2 Only.  Function Unknown.
    APPLY_HILIGHT2 = 4              // Parallax Flag in some Oblivion meshes.
}

/// <summary>
/// The type of texture.
/// </summary>
public enum TexType : uint {
    BASE_MAP = 0,                   // The basic texture used by most meshes.
    DARK_MAP = 1,                   // Used to darken the model with false lighting.
    DETAIL_MAP = 2,                 // Combined with base map for added detail.  Usually tiled over the mesh many times for close-up view.
    GLOSS_MAP = 3,                  // Allows the specularity (glossyness) of an object to differ across its surface.
    GLOW_MAP = 4,                   // Creates a glowing effect.  Basically an incandescence map.
    BUMP_MAP = 5,                   // Used to make the object appear to have more detail than it really does.
    NORMAL_MAP = 6,                 // Used to make the object appear to have more detail than it really does.
    PARALLAX_MAP = 7,               // Parallax map.
    DECAL_0_MAP = 8,                // For placing images on the object like stickers.
    DECAL_1_MAP = 9,                // For placing images on the object like stickers.
    DECAL_2_MAP = 10,               // For placing images on the object like stickers.
    DECAL_3_MAP = 11                // For placing images on the object like stickers.
}

/// <summary>
/// The type of animation interpolation (blending) that will be used on the associated key frames.
/// </summary>
public enum KeyType : uint {
    LINEAR_KEY = 1,                 // Use linear interpolation.
    QUADRATIC_KEY = 2,              // Use quadratic interpolation.  Forward and back tangents will be stored.
    TBC_KEY = 3,                    // Use Tension Bias Continuity interpolation.  Tension, bias, and continuity will be stored.
    XYZ_ROTATION_KEY = 4,           // For use only with rotation data.  Separate X, Y, and Z keys will be stored instead of using quaternions.
    CONST_KEY = 5                   // Step function. Used for visibility keys in NiBoolData.
}

/// <summary>
/// Bethesda Havok. Material descriptor for a Havok shape in Oblivion.
/// </summary>
public enum OblivionHavokMaterial : uint {
    OB_HAV_MAT_STONE = 0,           // Stone
    OB_HAV_MAT_CLOTH = 1,           // Cloth
    OB_HAV_MAT_DIRT = 2,            // Dirt
    OB_HAV_MAT_GLASS = 3,           // Glass
    OB_HAV_MAT_GRASS = 4,           // Grass
    OB_HAV_MAT_METAL = 5,           // Metal
    OB_HAV_MAT_ORGANIC = 6,         // Organic
    OB_HAV_MAT_SKIN = 7,            // Skin
    OB_HAV_MAT_WATER = 8,           // Water
    OB_HAV_MAT_WOOD = 9,            // Wood
    OB_HAV_MAT_HEAVY_STONE = 10,    // Heavy Stone
    OB_HAV_MAT_HEAVY_METAL = 11,    // Heavy Metal
    OB_HAV_MAT_HEAVY_WOOD = 12,     // Heavy Wood
    OB_HAV_MAT_CHAIN = 13,          // Chain
    OB_HAV_MAT_SNOW = 14,           // Snow
    OB_HAV_MAT_STONE_STAIRS = 15,   // Stone Stairs
    OB_HAV_MAT_CLOTH_STAIRS = 16,   // Cloth Stairs
    OB_HAV_MAT_DIRT_STAIRS = 17,    // Dirt Stairs
    OB_HAV_MAT_GLASS_STAIRS = 18,   // Glass Stairs
    OB_HAV_MAT_GRASS_STAIRS = 19,   // Grass Stairs
    OB_HAV_MAT_METAL_STAIRS = 20,   // Metal Stairs
    OB_HAV_MAT_ORGANIC_STAIRS = 21, // Organic Stairs
    OB_HAV_MAT_SKIN_STAIRS = 22,    // Skin Stairs
    OB_HAV_MAT_WATER_STAIRS = 23,   // Water Stairs
    OB_HAV_MAT_WOOD_STAIRS = 24,    // Wood Stairs
    OB_HAV_MAT_HEAVY_STONE_STAIRS = 25, // Heavy Stone Stairs
    OB_HAV_MAT_HEAVY_METAL_STAIRS = 26, // Heavy Metal Stairs
    OB_HAV_MAT_HEAVY_WOOD_STAIRS = 27, // Heavy Wood Stairs
    OB_HAV_MAT_CHAIN_STAIRS = 28,   // Chain Stairs
    OB_HAV_MAT_SNOW_STAIRS = 29,    // Snow Stairs
    OB_HAV_MAT_ELEVATOR = 30,       // Elevator
    OB_HAV_MAT_RUBBER = 31          // Rubber
}

/// <summary>
/// Bethesda Havok. Material descriptor for a Havok shape in Fallout 3 and Fallout NV.
/// </summary>
public enum Fallout3HavokMaterial : uint {
    FO_HAV_MAT_STONE = 0,           // Stone
    FO_HAV_MAT_CLOTH = 1,           // Cloth
    FO_HAV_MAT_DIRT = 2,            // Dirt
    FO_HAV_MAT_GLASS = 3,           // Glass
    FO_HAV_MAT_GRASS = 4,           // Grass
    FO_HAV_MAT_METAL = 5,           // Metal
    FO_HAV_MAT_ORGANIC = 6,         // Organic
    FO_HAV_MAT_SKIN = 7,            // Skin
    FO_HAV_MAT_WATER = 8,           // Water
    FO_HAV_MAT_WOOD = 9,            // Wood
    FO_HAV_MAT_HEAVY_STONE = 10,    // Heavy Stone
    FO_HAV_MAT_HEAVY_METAL = 11,    // Heavy Metal
    FO_HAV_MAT_HEAVY_WOOD = 12,     // Heavy Wood
    FO_HAV_MAT_CHAIN = 13,          // Chain
    FO_HAV_MAT_BOTTLECAP = 14,      // Bottlecap
    FO_HAV_MAT_ELEVATOR = 15,       // Elevator
    FO_HAV_MAT_HOLLOW_METAL = 16,   // Hollow Metal
    FO_HAV_MAT_SHEET_METAL = 17,    // Sheet Metal
    FO_HAV_MAT_SAND = 18,           // Sand
    FO_HAV_MAT_BROKEN_CONCRETE = 19, // Broken Concrete
    FO_HAV_MAT_VEHICLE_BODY = 20,   // Vehicle Body
    FO_HAV_MAT_VEHICLE_PART_SOLID = 21, // Vehicle Part Solid
    FO_HAV_MAT_VEHICLE_PART_HOLLOW = 22, // Vehicle Part Hollow
    FO_HAV_MAT_BARREL = 23,         // Barrel
    FO_HAV_MAT_BOTTLE = 24,         // Bottle
    FO_HAV_MAT_SODA_CAN = 25,       // Soda Can
    FO_HAV_MAT_PISTOL = 26,         // Pistol
    FO_HAV_MAT_RIFLE = 27,          // Rifle
    FO_HAV_MAT_SHOPPING_CART = 28,  // Shopping Cart
    FO_HAV_MAT_LUNCHBOX = 29,       // Lunchbox
    FO_HAV_MAT_BABY_RATTLE = 30,    // Baby Rattle
    FO_HAV_MAT_RUBBER_BALL = 31,    // Rubber Ball
    FO_HAV_MAT_STONE_PLATFORM = 32, // Stone
    FO_HAV_MAT_CLOTH_PLATFORM = 33, // Cloth
    FO_HAV_MAT_DIRT_PLATFORM = 34,  // Dirt
    FO_HAV_MAT_GLASS_PLATFORM = 35, // Glass
    FO_HAV_MAT_GRASS_PLATFORM = 36, // Grass
    FO_HAV_MAT_METAL_PLATFORM = 37, // Metal
    FO_HAV_MAT_ORGANIC_PLATFORM = 38, // Organic
    FO_HAV_MAT_SKIN_PLATFORM = 39,  // Skin
    FO_HAV_MAT_WATER_PLATFORM = 40, // Water
    FO_HAV_MAT_WOOD_PLATFORM = 41,  // Wood
    FO_HAV_MAT_HEAVY_STONE_PLATFORM = 42, // Heavy Stone
    FO_HAV_MAT_HEAVY_METAL_PLATFORM = 43, // Heavy Metal
    FO_HAV_MAT_HEAVY_WOOD_PLATFORM = 44, // Heavy Wood
    FO_HAV_MAT_CHAIN_PLATFORM = 45, // Chain
    FO_HAV_MAT_BOTTLECAP_PLATFORM = 46, // Bottlecap
    FO_HAV_MAT_ELEVATOR_PLATFORM = 47, // Elevator
    FO_HAV_MAT_HOLLOW_METAL_PLATFORM = 48, // Hollow Metal
    FO_HAV_MAT_SHEET_METAL_PLATFORM = 49, // Sheet Metal
    FO_HAV_MAT_SAND_PLATFORM = 50,  // Sand
    FO_HAV_MAT_BROKEN_CONCRETE_PLATFORM = 51, // Broken Concrete
    FO_HAV_MAT_VEHICLE_BODY_PLATFORM = 52, // Vehicle Body
    FO_HAV_MAT_VEHICLE_PART_SOLID_PLATFORM = 53, // Vehicle Part Solid
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_PLATFORM = 54, // Vehicle Part Hollow
    FO_HAV_MAT_BARREL_PLATFORM = 55, // Barrel
    FO_HAV_MAT_BOTTLE_PLATFORM = 56, // Bottle
    FO_HAV_MAT_SODA_CAN_PLATFORM = 57, // Soda Can
    FO_HAV_MAT_PISTOL_PLATFORM = 58, // Pistol
    FO_HAV_MAT_RIFLE_PLATFORM = 59, // Rifle
    FO_HAV_MAT_SHOPPING_CART_PLATFORM = 60, // Shopping Cart
    FO_HAV_MAT_LUNCHBOX_PLATFORM = 61, // Lunchbox
    FO_HAV_MAT_BABY_RATTLE_PLATFORM = 62, // Baby Rattle
    FO_HAV_MAT_RUBBER_BALL_PLATFORM = 63, // Rubber Ball
    FO_HAV_MAT_STONE_STAIRS = 64,   // Stone
    FO_HAV_MAT_CLOTH_STAIRS = 65,   // Cloth
    FO_HAV_MAT_DIRT_STAIRS = 66,    // Dirt
    FO_HAV_MAT_GLASS_STAIRS = 67,   // Glass
    FO_HAV_MAT_GRASS_STAIRS = 68,   // Grass
    FO_HAV_MAT_METAL_STAIRS = 69,   // Metal
    FO_HAV_MAT_ORGANIC_STAIRS = 70, // Organic
    FO_HAV_MAT_SKIN_STAIRS = 71,    // Skin
    FO_HAV_MAT_WATER_STAIRS = 72,   // Water
    FO_HAV_MAT_WOOD_STAIRS = 73,    // Wood
    FO_HAV_MAT_HEAVY_STONE_STAIRS = 74, // Heavy Stone
    FO_HAV_MAT_HEAVY_METAL_STAIRS = 75, // Heavy Metal
    FO_HAV_MAT_HEAVY_WOOD_STAIRS = 76, // Heavy Wood
    FO_HAV_MAT_CHAIN_STAIRS = 77,   // Chain
    FO_HAV_MAT_BOTTLECAP_STAIRS = 78, // Bottlecap
    FO_HAV_MAT_ELEVATOR_STAIRS = 79, // Elevator
    FO_HAV_MAT_HOLLOW_METAL_STAIRS = 80, // Hollow Metal
    FO_HAV_MAT_SHEET_METAL_STAIRS = 81, // Sheet Metal
    FO_HAV_MAT_SAND_STAIRS = 82,    // Sand
    FO_HAV_MAT_BROKEN_CONCRETE_STAIRS = 83, // Broken Concrete
    FO_HAV_MAT_VEHICLE_BODY_STAIRS = 84, // Vehicle Body
    FO_HAV_MAT_VEHICLE_PART_SOLID_STAIRS = 85, // Vehicle Part Solid
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_STAIRS = 86, // Vehicle Part Hollow
    FO_HAV_MAT_BARREL_STAIRS = 87,  // Barrel
    FO_HAV_MAT_BOTTLE_STAIRS = 88,  // Bottle
    FO_HAV_MAT_SODA_CAN_STAIRS = 89, // Soda Can
    FO_HAV_MAT_PISTOL_STAIRS = 90,  // Pistol
    FO_HAV_MAT_RIFLE_STAIRS = 91,   // Rifle
    FO_HAV_MAT_SHOPPING_CART_STAIRS = 92, // Shopping Cart
    FO_HAV_MAT_LUNCHBOX_STAIRS = 93, // Lunchbox
    FO_HAV_MAT_BABY_RATTLE_STAIRS = 94, // Baby Rattle
    FO_HAV_MAT_RUBBER_BALL_STAIRS = 95, // Rubber Ball
    FO_HAV_MAT_STONE_STAIRS_PLATFORM = 96, // Stone
    FO_HAV_MAT_CLOTH_STAIRS_PLATFORM = 97, // Cloth
    FO_HAV_MAT_DIRT_STAIRS_PLATFORM = 98, // Dirt
    FO_HAV_MAT_GLASS_STAIRS_PLATFORM = 99, // Glass
    FO_HAV_MAT_GRASS_STAIRS_PLATFORM = 100, // Grass
    FO_HAV_MAT_METAL_STAIRS_PLATFORM = 101, // Metal
    FO_HAV_MAT_ORGANIC_STAIRS_PLATFORM = 102, // Organic
    FO_HAV_MAT_SKIN_STAIRS_PLATFORM = 103, // Skin
    FO_HAV_MAT_WATER_STAIRS_PLATFORM = 104, // Water
    FO_HAV_MAT_WOOD_STAIRS_PLATFORM = 105, // Wood
    FO_HAV_MAT_HEAVY_STONE_STAIRS_PLATFORM = 106, // Heavy Stone
    FO_HAV_MAT_HEAVY_METAL_STAIRS_PLATFORM = 107, // Heavy Metal
    FO_HAV_MAT_HEAVY_WOOD_STAIRS_PLATFORM = 108, // Heavy Wood
    FO_HAV_MAT_CHAIN_STAIRS_PLATFORM = 109, // Chain
    FO_HAV_MAT_BOTTLECAP_STAIRS_PLATFORM = 110, // Bottlecap
    FO_HAV_MAT_ELEVATOR_STAIRS_PLATFORM = 111, // Elevator
    FO_HAV_MAT_HOLLOW_METAL_STAIRS_PLATFORM = 112, // Hollow Metal
    FO_HAV_MAT_SHEET_METAL_STAIRS_PLATFORM = 113, // Sheet Metal
    FO_HAV_MAT_SAND_STAIRS_PLATFORM = 114, // Sand
    FO_HAV_MAT_BROKEN_CONCRETE_STAIRS_PLATFORM = 115, // Broken Concrete
    FO_HAV_MAT_VEHICLE_BODY_STAIRS_PLATFORM = 116, // Vehicle Body
    FO_HAV_MAT_VEHICLE_PART_SOLID_STAIRS_PLATFORM = 117, // Vehicle Part Solid
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_STAIRS_PLATFORM = 118, // Vehicle Part Hollow
    FO_HAV_MAT_BARREL_STAIRS_PLATFORM = 119, // Barrel
    FO_HAV_MAT_BOTTLE_STAIRS_PLATFORM = 120, // Bottle
    FO_HAV_MAT_SODA_CAN_STAIRS_PLATFORM = 121, // Soda Can
    FO_HAV_MAT_PISTOL_STAIRS_PLATFORM = 122, // Pistol
    FO_HAV_MAT_RIFLE_STAIRS_PLATFORM = 123, // Rifle
    FO_HAV_MAT_SHOPPING_CART_STAIRS_PLATFORM = 124, // Shopping Cart
    FO_HAV_MAT_LUNCHBOX_STAIRS_PLATFORM = 125, // Lunchbox
    FO_HAV_MAT_BABY_RATTLE_STAIRS_PLATFORM = 126, // Baby Rattle
    FO_HAV_MAT_RUBBER_BALL_STAIRS_PLATFORM = 127 // Rubber Ball
}

/// <summary>
/// Bethesda Havok. Material descriptor for a Havok shape in Skyrim.
/// </summary>
public enum SkyrimHavokMaterial : uint {
    SKY_HAV_MAT_BROKEN_STONE = 131151687, // Broken Stone
    SKY_HAV_MAT_LIGHT_WOOD = 365420259, // Light Wood
    SKY_HAV_MAT_SNOW = 398949039,   // Snow
    SKY_HAV_MAT_GRAVEL = 428587608, // Gravel
    SKY_HAV_MAT_MATERIAL_CHAIN_METAL = 438912228, // Material Chain Metal
    SKY_HAV_MAT_BOTTLE = 493553910, // Bottle
    SKY_HAV_MAT_WOOD = 500811281,   // Wood
    SKY_HAV_MAT_SKIN = 591247106,   // Skin
    SKY_HAV_MAT_UNKNOWN_617099282 = 617099282, // Unknown in Creation Kit v1.9.32.0. Found in Dawnguard DLC in meshes\dlc01\clutter\dlc01deerskin.nif.
    SKY_HAV_MAT_BARREL = 732141076, // Barrel
    SKY_HAV_MAT_MATERIAL_CERAMIC_MEDIUM = 781661019, // Material Ceramic Medium
    SKY_HAV_MAT_MATERIAL_BASKET = 790784366, // Material Basket
    SKY_HAV_MAT_ICE = 873356572,    // Ice
    SKY_HAV_MAT_STAIRS_STONE = 899511101, // Stairs Stone
    SKY_HAV_MAT_WATER = 1024582599, // Water
    SKY_HAV_MAT_UNKNOWN_1028101969 = 1028101969, // Unknown in Creation Kit v1.6.89.0. Found in actors\draugr\character assets\skeletons.nif.
    SKY_HAV_MAT_MATERIAL_BLADE_1HAND = 1060167844, // Material Blade 1 Hand
    SKY_HAV_MAT_MATERIAL_BOOK = 1264672850, // Material Book
    SKY_HAV_MAT_MATERIAL_CARPET = 1286705471, // Material Carpet
    SKY_HAV_MAT_SOLID_METAL = 1288358971, // Solid Metal
    SKY_HAV_MAT_MATERIAL_AXE_1HAND = 1305674443, // Material Axe 1Hand
    SKY_HAV_MAT_UNKNOWN_1440721808 = 1440721808, // Unknown in Creation Kit v1.6.89.0. Found in armor\draugr\draugrbootsfemale_go.nif or armor\amuletsandrings\amuletgnd.nif.
    SKY_HAV_MAT_STAIRS_WOOD = 1461712277, // Stairs Wood
    SKY_HAV_MAT_MUD = 1486385281,   // Mud
    SKY_HAV_MAT_MATERIAL_BOULDER_SMALL = 1550912982, // Material Boulder Small
    SKY_HAV_MAT_STAIRS_SNOW = 1560365355, // Stairs Snow
    SKY_HAV_MAT_HEAVY_STONE = 1570821952, // Heavy Stone
    SKY_HAV_MAT_UNKNOWN_1574477864 = 1574477864, // Unknown in Creation Kit v1.6.89.0. Found in actors\dragon\character assets\skeleton.nif.
    SKY_HAV_MAT_UNKNOWN_1591009235 = 1591009235, // Unknown in Creation Kit v1.6.89.0. Found in trap objects or clutter\displaycases\displaycaselgangled01.nif or actors\deer\character assets\skeleton.nif.
    SKY_HAV_MAT_MATERIAL_BOWS_STAVES = 1607128641, // Material Bows Staves
    SKY_HAV_MAT_MATERIAL_WOOD_AS_STAIRS = 1803571212, // Material Wood As Stairs
    SKY_HAV_MAT_GRASS = 1848600814, // Grass
    SKY_HAV_MAT_MATERIAL_BOULDER_LARGE = 1885326971, // Material Boulder Large
    SKY_HAV_MAT_MATERIAL_STONE_AS_STAIRS = 1886078335, // Material Stone As Stairs
    SKY_HAV_MAT_MATERIAL_BLADE_2HAND = 2022742644, // Material Blade 2Hand
    SKY_HAV_MAT_MATERIAL_BOTTLE_SMALL = 2025794648, // Material Bottle Small
    SKY_HAV_MAT_SAND = 2168343821,  // Sand
    SKY_HAV_MAT_HEAVY_METAL = 2229413539, // Heavy Metal
    SKY_HAV_MAT_UNKNOWN_2290050264 = 2290050264, // Unknown in Creation Kit v1.9.32.0. Found in Dawnguard DLC in meshes\dlc01\clutter\dlc01sabrecatpelt.nif.
    SKY_HAV_MAT_DRAGON = 2518321175, // Dragon
    SKY_HAV_MAT_MATERIAL_BLADE_1HAND_SMALL = 2617944780, // Material Blade 1Hand Small
    SKY_HAV_MAT_MATERIAL_SKIN_SMALL = 2632367422, // Material Skin Small
    SKY_HAV_MAT_STAIRS_BROKEN_STONE = 2892392795, // Stairs Broken Stone
    SKY_HAV_MAT_MATERIAL_SKIN_LARGE = 2965929619, // Material Skin Large
    SKY_HAV_MAT_ORGANIC = 2974920155, // Organic
    SKY_HAV_MAT_MATERIAL_BONE = 3049421844, // Material Bone
    SKY_HAV_MAT_HEAVY_WOOD = 3070783559, // Heavy Wood
    SKY_HAV_MAT_MATERIAL_CHAIN = 3074114406, // Material Chain
    SKY_HAV_MAT_DIRT = 3106094762,  // Dirt
    SKY_HAV_MAT_MATERIAL_ARMOR_LIGHT = 3424720541, // Material Armor Light
    SKY_HAV_MAT_MATERIAL_SHIELD_LIGHT = 3448167928, // Material Shield Light
    SKY_HAV_MAT_MATERIAL_COIN = 3589100606, // Material Coin
    SKY_HAV_MAT_MATERIAL_SHIELD_HEAVY = 3702389584, // Material Shield Heavy
    SKY_HAV_MAT_MATERIAL_ARMOR_HEAVY = 3708432437, // Material Armor Heavy
    SKY_HAV_MAT_MATERIAL_ARROW = 3725505938, // Material Arrow
    SKY_HAV_MAT_GLASS = 3739830338, // Glass
    SKY_HAV_MAT_STONE = 3741512247, // Stone
    SKY_HAV_MAT_CLOTH = 3839073443, // Cloth
    SKY_HAV_MAT_MATERIAL_BLUNT_2HAND = 3969592277, // Material Blunt 2Hand
    SKY_HAV_MAT_UNKNOWN_4239621792 = 4239621792, // Unknown in Creation Kit v1.9.32.0. Found in Dawnguard DLC in meshes\dlc01\prototype\dlc1protoswingingbridge.nif.
    SKY_HAV_MAT_MATERIAL_BOULDER_MEDIUM = 4283869410 // Material Boulder Medium
}

/// <summary>
/// Bethesda Havok. Describes the collision layer a body belongs to in Oblivion.
/// </summary>
public enum OblivionLayer : byte {
    OL_UNIDENTIFIED = 0,            // Unidentified (white)
    OL_STATIC = 1,                  // Static (red)
    OL_ANIM_STATIC = 2,             // AnimStatic (magenta)
    OL_TRANSPARENT = 3,             // Transparent (light pink)
    OL_CLUTTER = 4,                 // Clutter (light blue)
    OL_WEAPON = 5,                  // Weapon (orange)
    OL_PROJECTILE = 6,              // Projectile (light orange)
    OL_SPELL = 7,                   // Spell (cyan)
    OL_BIPED = 8,                   // Biped (green) Seems to apply to all creatures/NPCs
    OL_TREES = 9,                   // Trees (light brown)
    OL_PROPS = 10,                  // Props (magenta)
    OL_WATER = 11,                  // Water (cyan)
    OL_TRIGGER = 12,                // Trigger (light grey)
    OL_TERRAIN = 13,                // Terrain (light yellow)
    OL_TRAP = 14,                   // Trap (light grey)
    OL_NONCOLLIDABLE = 15,          // NonCollidable (white)
    OL_CLOUD_TRAP = 16,             // CloudTrap (greenish grey)
    OL_GROUND = 17,                 // Ground (none)
    OL_PORTAL = 18,                 // Portal (green)
    OL_STAIRS = 19,                 // Stairs (white)
    OL_CHAR_CONTROLLER = 20,        // CharController (yellow)
    OL_AVOID_BOX = 21,              // AvoidBox (dark yellow)
    OL_UNKNOWN1 = 22,               // ? (white)
    OL_UNKNOWN2 = 23,               // ? (white)
    OL_CAMERA_PICK = 24,            // CameraPick (white)
    OL_ITEM_PICK = 25,              // ItemPick (white)
    OL_LINE_OF_SIGHT = 26,          // LineOfSight (white)
    OL_PATH_PICK = 27,              // PathPick (white)
    OL_CUSTOM_PICK_1 = 28,          // CustomPick1 (white)
    OL_CUSTOM_PICK_2 = 29,          // CustomPick2 (white)
    OL_SPELL_EXPLOSION = 30,        // SpellExplosion (white)
    OL_DROPPING_PICK = 31,          // DroppingPick (white)
    OL_OTHER = 32,                  // Other (white)
    OL_HEAD = 33,                   // Head
    OL_BODY = 34,                   // Body
    OL_SPINE1 = 35,                 // Spine1
    OL_SPINE2 = 36,                 // Spine2
    OL_L_UPPER_ARM = 37,            // LUpperArm
    OL_L_FOREARM = 38,              // LForeArm
    OL_L_HAND = 39,                 // LHand
    OL_L_THIGH = 40,                // LThigh
    OL_L_CALF = 41,                 // LCalf
    OL_L_FOOT = 42,                 // LFoot
    OL_R_UPPER_ARM = 43,            // RUpperArm
    OL_R_FOREARM = 44,              // RForeArm
    OL_R_HAND = 45,                 // RHand
    OL_R_THIGH = 46,                // RThigh
    OL_R_CALF = 47,                 // RCalf
    OL_R_FOOT = 48,                 // RFoot
    OL_TAIL = 49,                   // Tail
    OL_SIDE_WEAPON = 50,            // SideWeapon
    OL_SHIELD = 51,                 // Shield
    OL_QUIVER = 52,                 // Quiver
    OL_BACK_WEAPON = 53,            // BackWeapon
    OL_BACK_WEAPON2 = 54,           // BackWeapon (?)
    OL_PONYTAIL = 55,               // PonyTail
    OL_WING = 56,                   // Wing
    OL_NULL = 57                    // Null
}

/// <summary>
/// Bethesda Havok. Describes the collision layer a body belongs to in Fallout 3 and Fallout NV.
/// </summary>
public enum Fallout3Layer : byte {
    FOL_UNIDENTIFIED = 0,           // Unidentified (white)
    FOL_STATIC = 1,                 // Static (red)
    FOL_ANIM_STATIC = 2,            // AnimStatic (magenta)
    FOL_TRANSPARENT = 3,            // Transparent (light pink)
    FOL_CLUTTER = 4,                // Clutter (light blue)
    FOL_WEAPON = 5,                 // Weapon (orange)
    FOL_PROJECTILE = 6,             // Projectile (light orange)
    FOL_SPELL = 7,                  // Spell (cyan)
    FOL_BIPED = 8,                  // Biped (green) Seems to apply to all creatures/NPCs
    FOL_TREES = 9,                  // Trees (light brown)
    FOL_PROPS = 10,                 // Props (magenta)
    FOL_WATER = 11,                 // Water (cyan)
    FOL_TRIGGER = 12,               // Trigger (light grey)
    FOL_TERRAIN = 13,               // Terrain (light yellow)
    FOL_TRAP = 14,                  // Trap (light grey)
    FOL_NONCOLLIDABLE = 15,         // NonCollidable (white)
    FOL_CLOUD_TRAP = 16,            // CloudTrap (greenish grey)
    FOL_GROUND = 17,                // Ground (none)
    FOL_PORTAL = 18,                // Portal (green)
    FOL_DEBRIS_SMALL = 19,          // DebrisSmall (white)
    FOL_DEBRIS_LARGE = 20,          // DebrisLarge (white)
    FOL_ACOUSTIC_SPACE = 21,        // AcousticSpace (white)
    FOL_ACTORZONE = 22,             // Actorzone (white)
    FOL_PROJECTILEZONE = 23,        // Projectilezone (white)
    FOL_GASTRAP = 24,               // GasTrap (yellowish green)
    FOL_SHELLCASING = 25,           // ShellCasing (white)
    FOL_TRANSPARENT_SMALL = 26,     // TransparentSmall (white)
    FOL_INVISIBLE_WALL = 27,        // InvisibleWall (white)
    FOL_TRANSPARENT_SMALL_ANIM = 28, // TransparentSmallAnim (white)
    FOL_DEADBIP = 29,               // Dead Biped (green)
    FOL_CHARCONTROLLER = 30,        // CharController (yellow)
    FOL_AVOIDBOX = 31,              // Avoidbox (orange)
    FOL_COLLISIONBOX = 32,          // Collisionbox (white)
    FOL_CAMERASPHERE = 33,          // Camerasphere (white)
    FOL_DOORDETECTION = 34,         // Doordetection (white)
    FOL_CAMERAPICK = 35,            // Camerapick (white)
    FOL_ITEMPICK = 36,              // Itempick (white)
    FOL_LINEOFSIGHT = 37,           // LineOfSight (white)
    FOL_PATHPICK = 38,              // Pathpick (white)
    FOL_CUSTOMPICK1 = 39,           // Custompick1 (white)
    FOL_CUSTOMPICK2 = 40,           // Custompick2 (white)
    FOL_SPELLEXPLOSION = 41,        // SpellExplosion (white)
    FOL_DROPPINGPICK = 42,          // Droppingpick (white)
    FOL_NULL = 43                   // Null (white)
}

/// <summary>
/// Bethesda Havok. Describes the collision layer a body belongs to in Skyrim.
/// </summary>
public enum SkyrimLayer : byte {
    SKYL_UNIDENTIFIED = 0,          // Unidentified
    SKYL_STATIC = 1,                // Static
    SKYL_ANIMSTATIC = 2,            // Anim Static
    SKYL_TRANSPARENT = 3,           // Transparent
    SKYL_CLUTTER = 4,               // Clutter. Object with this layer will float on water surface.
    SKYL_WEAPON = 5,                // Weapon
    SKYL_PROJECTILE = 6,            // Projectile
    SKYL_SPELL = 7,                 // Spell
    SKYL_BIPED = 8,                 // Biped. Seems to apply to all creatures/NPCs
    SKYL_TREES = 9,                 // Trees
    SKYL_PROPS = 10,                // Props
    SKYL_WATER = 11,                // Water
    SKYL_TRIGGER = 12,              // Trigger
    SKYL_TERRAIN = 13,              // Terrain
    SKYL_TRAP = 14,                 // Trap
    SKYL_NONCOLLIDABLE = 15,        // NonCollidable
    SKYL_CLOUD_TRAP = 16,           // CloudTrap
    SKYL_GROUND = 17,               // Ground. It seems that produces no sound when collide.
    SKYL_PORTAL = 18,               // Portal
    SKYL_DEBRIS_SMALL = 19,         // Debris Small
    SKYL_DEBRIS_LARGE = 20,         // Debris Large
    SKYL_ACOUSTIC_SPACE = 21,       // Acoustic Space
    SKYL_ACTORZONE = 22,            // Actor Zone
    SKYL_PROJECTILEZONE = 23,       // Projectile Zone
    SKYL_GASTRAP = 24,              // Gas Trap
    SKYL_SHELLCASING = 25,          // Shell Casing
    SKYL_TRANSPARENT_SMALL = 26,    // Transparent Small
    SKYL_INVISIBLE_WALL = 27,       // Invisible Wall
    SKYL_TRANSPARENT_SMALL_ANIM = 28, // Transparent Small Anim
    SKYL_WARD = 29,                 // Ward
    SKYL_CHARCONTROLLER = 30,       // Char Controller
    SKYL_STAIRHELPER = 31,          // Stair Helper
    SKYL_DEADBIP = 32,              // Dead Bip
    SKYL_BIPED_NO_CC = 33,          // Biped No CC
    SKYL_AVOIDBOX = 34,             // Avoid Box
    SKYL_COLLISIONBOX = 35,         // Collision Box
    SKYL_CAMERASHPERE = 36,         // Camera Sphere
    SKYL_DOORDETECTION = 37,        // Door Detection
    SKYL_CONEPROJECTILE = 38,       // Cone Projectile
    SKYL_CAMERAPICK = 39,           // Camera Pick
    SKYL_ITEMPICK = 40,             // Item Pick
    SKYL_LINEOFSIGHT = 41,          // Line of Sight
    SKYL_PATHPICK = 42,             // Path Pick
    SKYL_CUSTOMPICK1 = 43,          // Custom Pick 1
    SKYL_CUSTOMPICK2 = 44,          // Custom Pick 2
    SKYL_SPELLEXPLOSION = 45,       // Spell Explosion
    SKYL_DROPPINGPICK = 46,         // Dropping Pick
    SKYL_NULL = 47                  // Null
}

/// <summary>
/// Bethesda Havok.
/// A byte describing if MOPP Data is organized into chunks (PS3) or not (PC)
/// </summary>
public enum MoppDataBuildType : byte {
    BUILT_WITH_CHUNK_SUBDIVISION = 0, // Organized in chunks for PS3.
    BUILT_WITHOUT_CHUNK_SUBDIVISION = 1, // Not organized in chunks for PC. (Default)
    BUILD_NOT_SET = 2               // Build type not set yet.
}

/// <summary>
/// Target platform for NiPersistentSrcTextureRendererData (later than 30.1).
/// </summary>
public enum PlatformID : uint {
    ANY = 0,
    XENON = 1,
    PS3 = 2,
    DX9 = 3,
    WII = 4,
    D3D10 = 5
}

/// <summary>
/// Target renderer for NiPersistentSrcTextureRendererData (until 30.1).
/// </summary>
public enum RendererID : uint {
    XBOX360 = 0,
    PS3 = 1,
    DX9 = 2,
    D3D10 = 3,
    WII = 4,
    GENERIC = 5,
    D3D11 = 6
}

/// <summary>
/// Describes the pixel format used by the NiPixelData object to store a texture.
/// </summary>
public enum PixelFormat : uint {
    FMT_RGB = 0,                    // 24-bit RGB. 8 bits per red, blue, and green component.
    FMT_RGBA = 1,                   // 32-bit RGB with alpha. 8 bits per red, blue, green, and alpha component.
    FMT_PAL = 2,                    // 8-bit palette index.
    FMT_PALA = 3,                   // 8-bit palette index with alpha.
    FMT_DXT1 = 4,                   // DXT1 compressed texture.
    FMT_DXT3 = 5,                   // DXT3 compressed texture.
    FMT_DXT5 = 6,                   // DXT5 compressed texture.
    FMT_RGB24NONINT = 7,            // (Deprecated) 24-bit noninterleaved texture, an old PS2 format.
    FMT_BUMP = 8,                   // Uncompressed dU/dV gradient bump map.
    FMT_BUMPLUMA = 9,               // Uncompressed dU/dV gradient bump map with luma channel representing shininess.
    FMT_RENDERSPEC = 10,            // Generic descriptor for any renderer-specific format not described by other formats.
    FMT_1CH = 11,                   // Generic descriptor for formats with 1 component.
    FMT_2CH = 12,                   // Generic descriptor for formats with 2 components.
    FMT_3CH = 13,                   // Generic descriptor for formats with 3 components.
    FMT_4CH = 14,                   // Generic descriptor for formats with 4 components.
    FMT_DEPTH_STENCIL = 15,         // Indicates the NiPixelFormat is meant to be used on a depth/stencil surface.
    FMT_UNKNOWN = 16
}

/// <summary>
/// Describes whether pixels have been tiled from their standard row-major format to a format optimized for a particular platform.
/// </summary>
public enum PixelTiling : uint {
    TILE_NONE = 0,
    TILE_XENON = 1,
    TILE_WII = 2,
    TILE_NV_SWIZZLED = 3
}

/// <summary>
/// Describes the pixel format used by the NiPixelData object to store a texture.
/// </summary>
public enum PixelComponent : uint {
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
    COMP_EMPTY = 19
}

/// <summary>
/// Describes how each pixel should be accessed on NiPixelFormat.
/// </summary>
public enum PixelRepresentation : uint {
    REP_NORM_INT = 0,
    REP_HALF = 1,
    REP_FLOAT = 2,
    REP_INDEX = 3,
    REP_COMPRESSED = 4,
    REP_UNKNOWN = 5,
    REP_INT = 6
}

/// <summary>
/// Describes the color depth in an NiTexture.
/// </summary>
public enum PixelLayout : uint {
    LAY_PALETTIZED_8 = 0,           // Texture is in 8-bit palettized format.
    LAY_HIGH_COLOR_16 = 1,          // Texture is in 16-bit high color format.
    LAY_TRUE_COLOR_32 = 2,          // Texture is in 32-bit true color format.
    LAY_COMPRESSED = 3,             // Texture is compressed.
    LAY_BUMPMAP = 4,                // Texture is a grayscale bump map.
    LAY_PALETTIZED_4 = 5,           // Texture is in 4-bit palettized format.
    LAY_DEFAULT = 6,                // Use default setting.
    LAY_SINGLE_COLOR_8 = 7,
    LAY_SINGLE_COLOR_16 = 8,
    LAY_SINGLE_COLOR_32 = 9,
    LAY_DOUBLE_COLOR_32 = 10,
    LAY_DOUBLE_COLOR_64 = 11,
    LAY_FLOAT_COLOR_32 = 12,
    LAY_FLOAT_COLOR_64 = 13,
    LAY_FLOAT_COLOR_128 = 14,
    LAY_SINGLE_COLOR_4 = 15,
    LAY_DEPTH_24_X8 = 16
}

/// <summary>
/// Describes how mipmaps are handled in an NiTexture.
/// </summary>
public enum MipMapFormat : uint {
    MIP_FMT_NO = 0,                 // Texture does not use mip maps.
    MIP_FMT_YES = 1,                // Texture uses mip maps.
    MIP_FMT_DEFAULT = 2             // Use default setting.
}

/// <summary>
/// Describes how transparency is handled in an NiTexture.
/// </summary>
public enum AlphaFormat : uint {
    ALPHA_NONE = 0,                 // No alpha.
    ALPHA_BINARY = 1,               // 1-bit alpha.
    ALPHA_SMOOTH = 2,               // Interpolated 4- or 8-bit alpha.
    ALPHA_DEFAULT = 3               // Use default setting.
}

/// <summary>
/// Describes the availiable texture clamp modes, i.e. the behavior of UV mapping outside the [0,1] range.
/// </summary>
public enum TexClampMode : uint {
    CLAMP_S_CLAMP_T = 0,            // Clamp in both directions.
    CLAMP_S_WRAP_T = 1,             // Clamp in the S(U) direction but wrap in the T(V) direction.
    WRAP_S_CLAMP_T = 2,             // Wrap in the S(U) direction but clamp in the T(V) direction.
    WRAP_S_WRAP_T = 3               // Wrap in both directions.
}

/// <summary>
/// Describes the availiable texture filter modes, i.e. the way the pixels in a texture are displayed on screen.
/// </summary>
public enum TexFilterMode : uint {
    FILTER_NEAREST = 0,             // Nearest neighbor. Uses nearest texel with no mipmapping.
    FILTER_BILERP = 1,              // Bilinear. Linear interpolation with no mipmapping.
    FILTER_TRILERP = 2,             // Trilinear. Linear intepolation between 8 texels (4 nearest texels between 2 nearest mip levels).
    FILTER_NEAREST_MIPNEAREST = 3,  // Nearest texel on nearest mip level.
    FILTER_NEAREST_MIPLERP = 4,     // Linear interpolates nearest texel between two nearest mip levels.
    FILTER_BILERP_MIPNEAREST = 5,   // Linear interpolates on nearest mip level.
    FILTER_ANISOTROPIC = 6          // Anisotropic filtering. One or many trilinear samples depending on anisotropy.
}

/// <summary>
/// Describes how to apply vertex colors for NiVertexColorProperty.
/// </summary>
public enum VertMode : uint {
    VERT_MODE_SRC_IGNORE = 0,       // Emissive, ambient, and diffuse colors are all specified by the NiMaterialProperty.
    VERT_MODE_SRC_EMISSIVE = 1,     // Emissive colors are specified by the source vertex colors. Ambient+Diffuse are specified by the NiMaterialProperty.
    VERT_MODE_SRC_AMB_DIF = 2       // Ambient+Diffuse colors are specified by the source vertex colors. Emissive is specified by the NiMaterialProperty. (Default)
}

/// <summary>
/// Describes which lighting equation components influence the final vertex color for NiVertexColorProperty.
/// </summary>
public enum LightMode : uint {
    LIGHT_MODE_EMISSIVE = 0,        // Emissive.
    LIGHT_MODE_EMI_AMB_DIF = 1      // Emissive + Ambient + Diffuse. (Default)
}

/// <summary>
/// The animation cyle behavior.
/// </summary>
public enum CycleType : uint {
    CYCLE_LOOP = 0,                 // Loop
    CYCLE_REVERSE = 1,              // Reverse
    CYCLE_CLAMP = 2                 // Clamp
}

/// <summary>
/// The force field type.
/// </summary>
public enum FieldType : uint {
    FIELD_WIND = 0,                 // Wind (fixed direction)
    FIELD_POINT = 1                 // Point (fixed origin)
}

/// <summary>
/// Determines the way the billboard will react to the camera.
/// Billboard mode is stored in lowest 3 bits although Oblivion vanilla nifs uses values higher than 7.
/// </summary>
public enum BillboardMode : ushort {
    ALWAYS_FACE_CAMERA = 0,         // Align billboard and camera forward vector. Minimized rotation.
    ROTATE_ABOUT_UP = 1,            // Align billboard and camera forward vector while allowing rotation around the up axis.
    RIGID_FACE_CAMERA = 2,          // Align billboard and camera forward vector. Non-minimized rotation.
    ALWAYS_FACE_CENTER = 3,         // Billboard forward vector always faces camera ceneter. Minimized rotation.
    RIGID_FACE_CENTER = 4,          // Billboard forward vector always faces camera ceneter. Non-minimized rotation.
    BSROTATE_ABOUT_UP = 5,          // The billboard will only rotate around its local Z axis (it always stays in its local X-Y plane).
    ROTATE_ABOUT_UP2 = 9            // The billboard will only rotate around the up axis (same as ROTATE_ABOUT_UP?).
}

/// <summary>
/// Describes stencil buffer test modes for NiStencilProperty.
/// </summary>
public enum StencilCompareMode : uint {
    TEST_NEVER = 0,                 // Always false. Ref value is ignored.
    TEST_LESS = 1,                  // VRef ‹ VBuf
    TEST_EQUAL = 2,                 // VRef = VBuf
    TEST_LESS_EQUAL = 3,            // VRef ≤ VBuf
    TEST_GREATER = 4,               // VRef › VBuf
    TEST_NOT_EQUAL = 5,             // VRef ≠ VBuf
    TEST_GREATER_EQUAL = 6,         // VRef ≥ VBuf
    TEST_ALWAYS = 7                 // Always true. Buffer is ignored.
}

/// <summary>
/// Describes the actions which can occur as a result of tests for NiStencilProperty.
/// </summary>
public enum StencilAction : uint {
    ACTION_KEEP = 0,                // Keep the current value in the stencil buffer.
    ACTION_ZERO = 1,                // Write zero to the stencil buffer.
    ACTION_REPLACE = 2,             // Write the reference value to the stencil buffer.
    ACTION_INCREMENT = 3,           // Increment the value in the stencil buffer.
    ACTION_DECREMENT = 4,           // Decrement the value in the stencil buffer.
    ACTION_INVERT = 5               // Bitwise invert the value in the stencil buffer.
}

/// <summary>
/// Describes the face culling options for NiStencilProperty.
/// </summary>
public enum StencilDrawMode : uint {
    DRAW_CCW_OR_BOTH = 0,           // Application default, chooses between DRAW_CCW or DRAW_BOTH.
    DRAW_CCW = 1,                   // Draw only the triangles whose vertices are ordered CCW with respect to the viewer. (Standard behavior)
    DRAW_CW = 2,                    // Draw only the triangles whose vertices are ordered CW with respect to the viewer. (Effectively flips faces)
    DRAW_BOTH = 3                   // Draw all triangles, regardless of orientation. (Effectively force double-sided)
}

/// <summary>
/// Describes Z-buffer test modes for NiZBufferProperty.
/// "Less than" = closer to camera, "Greater than" = further from camera.
/// </summary>
public enum ZCompareMode : uint {
    ZCOMP_ALWAYS = 0,               // Always true. Buffer is ignored.
    ZCOMP_LESS = 1,                 // VRef ‹ VBuf
    ZCOMP_EQUAL = 2,                // VRef = VBuf
    ZCOMP_LESS_EQUAL = 3,           // VRef ≤ VBuf
    ZCOMP_GREATER = 4,              // VRef › VBuf
    ZCOMP_NOT_EQUAL = 5,            // VRef ≠ VBuf
    ZCOMP_GREATER_EQUAL = 6,        // VRef ≥ VBuf
    ZCOMP_NEVER = 7                 // Always false. Ref value is ignored.
}

/// <summary>
/// Bethesda Havok, based on hkpMotion::MotionType. Motion type of a rigid body determines what happens when it is simulated.
/// </summary>
public enum hkMotionType : byte {
    MO_SYS_INVALID = 0,             // Invalid
    MO_SYS_DYNAMIC = 1,             // A fully-simulated, movable rigid body. At construction time the engine checks the input inertia and selects MO_SYS_SPHERE_INERTIA or MO_SYS_BOX_INERTIA as appropriate.
    MO_SYS_SPHERE_INERTIA = 2,      // Simulation is performed using a sphere inertia tensor.
    MO_SYS_SPHERE_STABILIZED = 3,   // This is the same as MO_SYS_SPHERE_INERTIA, except that simulation of the rigid body is "softened".
    MO_SYS_BOX_INERTIA = 4,         // Simulation is performed using a box inertia tensor.
    MO_SYS_BOX_STABILIZED = 5,      // This is the same as MO_SYS_BOX_INERTIA, except that simulation of the rigid body is "softened".
    MO_SYS_KEYFRAMED = 6,           // Simulation is not performed as a normal rigid body. The keyframed rigid body has an infinite mass when viewed by the rest of the system. (used for creatures)
    MO_SYS_FIXED = 7,               // This motion type is used for the static elements of a game scene, e.g. the landscape. Faster than MO_SYS_KEYFRAMED at velocity 0. (used for weapons)
    MO_SYS_THIN_BOX = 8,            // A box inertia motion which is optimized for thin boxes and has less stability problems
    MO_SYS_CHARACTER = 9            // A specialized motion used for character controllers
}

/// <summary>
/// Bethesda Havok, based on hkpRigidBodyDeactivator::DeactivatorType.
/// Deactivator Type determines which mechanism Havok will use to classify the body as deactivated.
/// </summary>
public enum hkDeactivatorType : byte {
    DEACTIVATOR_INVALID = 0,        // Invalid
    DEACTIVATOR_NEVER = 1,          // This will force the rigid body to never deactivate.
    DEACTIVATOR_SPATIAL = 2         // Tells Havok to use a spatial deactivation scheme. This makes use of high and low frequencies of positional motion to determine when deactivation should occur.
}

/// <summary>
/// Bethesda Havok, based on hkpRigidBodyCinfo::SolverDeactivation.
/// A list of possible solver deactivation settings. This value defines how aggressively the solver deactivates objects.
/// Note: Solver deactivation does not save CPU, but reduces creeping of movable objects in a pile quite dramatically.
/// </summary>
public enum hkSolverDeactivation : byte {
    SOLVER_DEACTIVATION_INVALID = 0, // Invalid
    SOLVER_DEACTIVATION_OFF = 1,    // No solver deactivation.
    SOLVER_DEACTIVATION_LOW = 2,    // Very conservative deactivation, typically no visible artifacts.
    SOLVER_DEACTIVATION_MEDIUM = 3, // Normal deactivation, no serious visible artifacts in most cases.
    SOLVER_DEACTIVATION_HIGH = 4,   // Fast deactivation, visible artifacts.
    SOLVER_DEACTIVATION_MAX = 5     // Very fast deactivation, visible artifacts.
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
public enum hkQualityType : byte {
    MO_QUAL_INVALID = 0,            // Automatically assigned to MO_QUAL_FIXED, MO_QUAL_KEYFRAMED or MO_QUAL_DEBRIS
    MO_QUAL_FIXED = 1,              // Static body.
    MO_QUAL_KEYFRAMED = 2,          // Animated body with infinite mass.
    MO_QUAL_DEBRIS = 3,             // Low importance bodies adding visual detail.
    MO_QUAL_MOVING = 4,             // Moving bodies which should not penetrate or leave the world, but can.
    MO_QUAL_CRITICAL = 5,           // Gameplay critical bodies which cannot penetrate or leave the world under any circumstance.
    MO_QUAL_BULLET = 6,             // Fast-moving bodies, such as projectiles.
    MO_QUAL_USER = 7,               // For user.
    MO_QUAL_CHARACTER = 8,          // For use with rigid body character controllers.
    MO_QUAL_KEYFRAMED_REPORT = 9    // Moving bodies with infinite mass which should report contact points and TOI collisions against all other bodies.
}

/// <summary>
/// Describes the type of gravitational force.
/// </summary>
public enum ForceType : uint {
    FORCE_PLANAR = 0,
    FORCE_SPHERICAL = 1,
    FORCE_UNKNOWN = 2
}

/// <summary>
/// Describes which aspect of the NiTextureTransform the NiTextureTransformController will modify.
/// </summary>
public enum TransformMember : uint {
    TT_TRANSLATE_U = 0,             // Control the translation of the U coordinates.
    TT_TRANSLATE_V = 1,             // Control the translation of the V coordinates.
    TT_ROTATE = 2,                  // Control the rotation of the coordinates.
    TT_SCALE_U = 3,                 // Control the scale of the U coordinates.
    TT_SCALE_V = 4                  // Control the scale of the V coordinates.
}

/// <summary>
/// Describes the decay function of bomb forces.
/// </summary>
public enum DecayType : uint {
    DECAY_NONE = 0,                 // No decay.
    DECAY_LINEAR = 1,               // Linear decay.
    DECAY_EXPONENTIAL = 2           // Exponential decay.
}

/// <summary>
/// Describes the symmetry type of bomb forces.
/// </summary>
public enum SymmetryType : uint {
    SPHERICAL_SYMMETRY = 0,         // Spherical Symmetry.
    CYLINDRICAL_SYMMETRY = 1,       // Cylindrical Symmetry.
    PLANAR_SYMMETRY = 2             // Planar Symmetry.
}

/// <summary>
/// Controls the way the a particle mesh emitter determines the starting speed and direction of the particles that are emitted.
/// </summary>
public enum VelocityType : uint {
    VELOCITY_USE_NORMALS = 0,       // Uses the normals of the meshes to determine staring velocity.
    VELOCITY_USE_RANDOM = 1,        // Starts particles with a random velocity.
    VELOCITY_USE_DIRECTION = 2      // Uses the emission axis to determine initial particle direction?
}

/// <summary>
/// Controls which parts of the mesh that the particles are emitted from.
/// </summary>
public enum EmitFrom : uint {
    EMIT_FROM_VERTICES = 0,         // Emit particles from the vertices of the mesh.
    EMIT_FROM_FACE_CENTER = 1,      // Emit particles from the center of the faces of the mesh.
    EMIT_FROM_EDGE_CENTER = 2,      // Emit particles from the center of the edges of the mesh.
    EMIT_FROM_FACE_SURFACE = 3,     // Perhaps randomly emit particles from anywhere on the faces of the mesh?
    EMIT_FROM_EDGE_SURFACE = 4      // Perhaps randomly emit particles from anywhere on the edges of the mesh?
}

/// <summary>
/// The type of information that is stored in a texture used by an NiTextureEffect.
/// </summary>
public enum TextureType : uint {
    TEX_PROJECTED_LIGHT = 0,        // Apply a projected light texture. Each light effect is summed before multiplying by the base texture.
    TEX_PROJECTED_SHADOW = 1,       // Apply a projected shadow texture. Each shadow effect is multiplied by the base texture.
    TEX_ENVIRONMENT_MAP = 2,        // Apply an environment map texture. Added to the base texture and light/shadow/decal maps.
    TEX_FOG_MAP = 3                 // Apply a fog map texture. Alpha channel is used to blend the color channel with the base texture.
}

/// <summary>
/// Determines the way that UV texture coordinates are generated.
/// </summary>
public enum CoordGenType : uint {
    CG_WORLD_PARALLEL = 0,          // Use planar mapping.
    CG_WORLD_PERSPECTIVE = 1,       // Use perspective mapping.
    CG_SPHERE_MAP = 2,              // Use spherical mapping.
    CG_SPECULAR_CUBE_MAP = 3,       // Use specular cube mapping. For NiSourceCubeMap only.
    CG_DIFFUSE_CUBE_MAP = 4         // Use diffuse cube mapping. For NiSourceCubeMap only.
}

public enum EndianType : byte {
    ENDIAN_BIG = 0,                 // The numbers are stored in big endian format, such as those used by PowerPC Mac processors.
    ENDIAN_LITTLE = 1               // The numbers are stored in little endian format, such as those used by Intel and AMD x86 processors.
}

/// <summary>
/// Used by NiMaterialColorControllers to select which type of color in the controlled object that will be animated.
/// </summary>
public enum MaterialColor : ushort {
    TC_AMBIENT = 0,                 // Control the ambient color.
    TC_DIFFUSE = 1,                 // Control the diffuse color.
    TC_SPECULAR = 2,                // Control the specular color.
    TC_SELF_ILLUM = 3               // Control the self illumination color.
}

/// <summary>
/// Used by NiLightColorControllers to select which type of color in the controlled object that will be animated.
/// </summary>
public enum LightColor : ushort {
    LC_DIFFUSE = 0,                 // Control the diffuse color.
    LC_AMBIENT = 1                  // Control the ambient color.
}

/// <summary>
/// Used by NiGeometryData to control the volatility of the mesh.
/// Consistency Type is masked to only the upper 4 bits (0xF000). Dirty mask is the lower 12 (0x0FFF) but only used at runtime.
/// </summary>
public enum ConsistencyType : ushort {
    CT_MUTABLE = 0x0000,            // Mutable Mesh
    CT_STATIC = 0x4000,             // Static Mesh
    CT_VOLATILE = 0x8000            // Volatile Mesh
}

/// <summary>
/// Describes the way that NiSortAdjustNode modifies the sorting behavior for the subtree below it.
/// </summary>
public enum SortingMode : uint {
    SORTING_INHERIT = 0,            // Inherit. Acts identical to NiNode.
    SORTING_OFF = 1                 // Disables sort on all geometry under this node.
}

/// <summary>
/// The propagation mode controls scene graph traversal during collision detection operations for NiCollisionData.
/// </summary>
public enum PropagationMode : uint {
    PROPAGATE_ON_SUCCESS = 0,       // Propagation only occurs as a result of a successful collision.
    PROPAGATE_ON_FAILURE = 1,       // (Deprecated) Propagation only occurs as a result of a failed collision.
    PROPAGATE_ALWAYS = 2,           // Propagation always occurs regardless of collision result.
    PROPAGATE_NEVER = 3             // Propagation never occurs regardless of collision result.
}

/// <summary>
/// The collision mode controls the type of collision operation that is to take place for NiCollisionData.
/// </summary>
public enum CollisionMode : uint {
    CM_USE_OBB = 0,                 // Use Bounding Box
    CM_USE_TRI = 1,                 // Use Triangles
    CM_USE_ABV = 2,                 // Use Alternate Bounding Volumes
    CM_NOTEST = 3,                  // Indicates that no collision test should be made.
    CM_USE_NIBOUND = 4              // Use NiBound
}

public enum BoundVolumeType : uint {
    BASE_BV = 0xffffffff,           // Default
    SPHERE_BV = 0,                  // Sphere
    BOX_BV = 1,                     // Box
    CAPSULE_BV = 2,                 // Capsule
    UNION_BV = 4,                   // Union
    HALFSPACE_BV = 5                // Half Space
}

/// <summary>
/// Bethesda Havok.
/// </summary>
public enum hkResponseType : byte {
    RESPONSE_INVALID = 0,           // Invalid Response
    RESPONSE_SIMPLE_CONTACT = 1,    // Do normal collision resolution
    RESPONSE_REPORTING = 2,         // No collision resolution is performed but listeners are called
    RESPONSE_NONE = 3               // Do nothing, ignore all the results.
}

/// <summary>
/// Biped bodypart data used for visibility control of triangles.  Options are Fallout 3, except where marked for Skyrim (uses SBP prefix)
/// Skyrim BP names are listed only for vanilla names, different creatures have different defnitions for naming.
/// </summary>
public enum BSDismemberBodyPartType : ushort {
    BP_TORSO = 0,                   // Torso
    BP_HEAD = 1,                    // Head
    BP_HEAD2 = 2,                   // Head 2
    BP_LEFTARM = 3,                 // Left Arm
    BP_LEFTARM2 = 4,                // Left Arm 2
    BP_RIGHTARM = 5,                // Right Arm
    BP_RIGHTARM2 = 6,               // Right Arm 2
    BP_LEFTLEG = 7,                 // Left Leg
    BP_LEFTLEG2 = 8,                // Left Leg 2
    BP_LEFTLEG3 = 9,                // Left Leg 3
    BP_RIGHTLEG = 10,               // Right Leg
    BP_RIGHTLEG2 = 11,              // Right Leg 2
    BP_RIGHTLEG3 = 12,              // Right Leg 3
    BP_BRAIN = 13,                  // Brain
    SBP_30_HEAD = 30,               // Skyrim, Head(Human), Body(Atronachs,Beasts), Mask(Dragonpriest)
    SBP_31_HAIR = 31,               // Skyrim, Hair(human), Far(Dragon), Mask2(Dragonpriest),SkinnedFX(Spriggan)
    SBP_32_BODY = 32,               // Skyrim, Main body, extras(Spriggan)
    SBP_33_HANDS = 33,              // Skyrim, Hands L/R, BodyToo(Dragonpriest), Legs(Draugr), Arms(Giant)
    SBP_34_FOREARMS = 34,           // Skyrim, Forearms L/R, Beard(Draugr)
    SBP_35_AMULET = 35,             // Skyrim, Amulet
    SBP_36_RING = 36,               // Skyrim, Ring
    SBP_37_FEET = 37,               // Skyrim, Feet L/R
    SBP_38_CALVES = 38,             // Skyrim, Calves L/R
    SBP_39_SHIELD = 39,             // Skyrim, Shield
    SBP_40_TAIL = 40,               // Skyrim, Tail(Argonian/Khajiit), Skeleton01(Dragon), FX01(AtronachStorm),FXMist (Dragonpriest), Spit(Chaurus,Spider),SmokeFins(IceWraith)
    SBP_41_LONGHAIR = 41,           // Skyrim, Long Hair(Human), Skeleton02(Dragon),FXParticles(Dragonpriest)
    SBP_42_CIRCLET = 42,            // Skyrim, Circlet(Human, MouthFireEffect(Dragon)
    SBP_43_EARS = 43,               // Skyrim, Ears
    SBP_44_DRAGON_BLOODHEAD_OR_MOD_MOUTH = 44, // Skyrim, Bloodied dragon head, or NPC face/mouth
    SBP_45_DRAGON_BLOODWINGL_OR_MOD_NECK = 45, // Skyrim, Left Bloodied dragon wing, Saddle(Horse), or NPC cape, scarf, shawl, neck-tie, etc.
    SBP_46_DRAGON_BLOODWINGR_OR_MOD_CHEST_PRIMARY = 46, // Skyrim, Right Bloodied dragon wing, or NPC chest primary or outergarment
    SBP_47_DRAGON_BLOODTAIL_OR_MOD_BACK = 47, // Skyrim, Bloodied dragon tail, or NPC backpack/wings/...
    SBP_48_MOD_MISC1 = 48,          // Anything that does not fit in the list
    SBP_49_MOD_PELVIS_PRIMARY = 49, // Pelvis primary or outergarment
    SBP_50_DECAPITATEDHEAD = 50,    // Skyrim, Decapitated Head
    SBP_51_DECAPITATE = 51,         // Skyrim, Decapitate, neck gore
    SBP_52_MOD_PELVIS_SECONDARY = 52, // Pelvis secondary or undergarment
    SBP_53_MOD_LEG_RIGHT = 53,      // Leg primary or outergarment or right leg
    SBP_54_MOD_LEG_LEFT = 54,       // Leg secondary or undergarment or left leg
    SBP_55_MOD_FACE_JEWELRY = 55,   // Face alternate or jewelry
    SBP_56_MOD_CHEST_SECONDARY = 56, // Chest secondary or undergarment
    SBP_57_MOD_SHOULDER = 57,       // Shoulder
    SBP_58_MOD_ARM_LEFT = 58,       // Arm secondary or undergarment or left arm
    SBP_59_MOD_ARM_RIGHT = 59,      // Arm primary or outergarment or right arm
    SBP_60_MOD_MISC2 = 60,          // Anything that does not fit in the list
    SBP_61_FX01 = 61,               // Skyrim, FX01(Humanoid)
    BP_SECTIONCAP_HEAD = 101,       // Section Cap | Head
    BP_SECTIONCAP_HEAD2 = 102,      // Section Cap | Head 2
    BP_SECTIONCAP_LEFTARM = 103,    // Section Cap | Left Arm
    BP_SECTIONCAP_LEFTARM2 = 104,   // Section Cap | Left Arm 2
    BP_SECTIONCAP_RIGHTARM = 105,   // Section Cap | Right Arm
    BP_SECTIONCAP_RIGHTARM2 = 106,  // Section Cap | Right Arm 2
    BP_SECTIONCAP_LEFTLEG = 107,    // Section Cap | Left Leg
    BP_SECTIONCAP_LEFTLEG2 = 108,   // Section Cap | Left Leg 2
    BP_SECTIONCAP_LEFTLEG3 = 109,   // Section Cap | Left Leg 3
    BP_SECTIONCAP_RIGHTLEG = 110,   // Section Cap | Right Leg
    BP_SECTIONCAP_RIGHTLEG2 = 111,  // Section Cap | Right Leg 2
    BP_SECTIONCAP_RIGHTLEG3 = 112,  // Section Cap | Right Leg 3
    BP_SECTIONCAP_BRAIN = 113,      // Section Cap | Brain
    SBP_130_HEAD = 130,             // Skyrim, Head slot, use on full-face helmets
    SBP_131_HAIR = 131,             // Skyrim, Hair slot 1, use on hoods
    SBP_141_LONGHAIR = 141,         // Skyrim, Hair slot 2, use for longer hair
    SBP_142_CIRCLET = 142,          // Skyrim, Circlet slot 1, use for circlets
    SBP_143_EARS = 143,             // Skyrim, Ear slot
    SBP_150_DECAPITATEDHEAD = 150,  // Skyrim, neck gore on head side
    BP_TORSOCAP_HEAD = 201,         // Torso Cap | Head
    BP_TORSOCAP_HEAD2 = 202,        // Torso Cap | Head 2
    BP_TORSOCAP_LEFTARM = 203,      // Torso Cap | Left Arm
    BP_TORSOCAP_LEFTARM2 = 204,     // Torso Cap | Left Arm 2
    BP_TORSOCAP_RIGHTARM = 205,     // Torso Cap | Right Arm
    BP_TORSOCAP_RIGHTARM2 = 206,    // Torso Cap | Right Arm 2
    BP_TORSOCAP_LEFTLEG = 207,      // Torso Cap | Left Leg
    BP_TORSOCAP_LEFTLEG2 = 208,     // Torso Cap | Left Leg 2
    BP_TORSOCAP_LEFTLEG3 = 209,     // Torso Cap | Left Leg 3
    BP_TORSOCAP_RIGHTLEG = 210,     // Torso Cap | Right Leg
    BP_TORSOCAP_RIGHTLEG2 = 211,    // Torso Cap | Right Leg 2
    BP_TORSOCAP_RIGHTLEG3 = 212,    // Torso Cap | Right Leg 3
    BP_TORSOCAP_BRAIN = 213,        // Torso Cap | Brain
    SBP_230_HEAD = 230,             // Skyrim, Head slot, use for neck on character head
    BP_TORSOSECTION_HEAD = 1000,    // Torso Section | Head
    BP_TORSOSECTION_HEAD2 = 2000,   // Torso Section | Head 2
    BP_TORSOSECTION_LEFTARM = 3000, // Torso Section | Left Arm
    BP_TORSOSECTION_LEFTARM2 = 4000, // Torso Section | Left Arm 2
    BP_TORSOSECTION_RIGHTARM = 5000, // Torso Section | Right Arm
    BP_TORSOSECTION_RIGHTARM2 = 6000, // Torso Section | Right Arm 2
    BP_TORSOSECTION_LEFTLEG = 7000, // Torso Section | Left Leg
    BP_TORSOSECTION_LEFTLEG2 = 8000, // Torso Section | Left Leg 2
    BP_TORSOSECTION_LEFTLEG3 = 9000, // Torso Section | Left Leg 3
    BP_TORSOSECTION_RIGHTLEG = 10000, // Torso Section | Right Leg
    BP_TORSOSECTION_RIGHTLEG2 = 11000, // Torso Section | Right Leg 2
    BP_TORSOSECTION_RIGHTLEG3 = 12000, // Torso Section | Right Leg 3
    BP_TORSOSECTION_BRAIN = 13000   // Torso Section | Brain
}

/// <summary>
/// Values for configuring the shader type in a BSLightingShaderProperty
/// </summary>
public enum BSLightingShaderPropertyShaderType : uint {
    Default = 0,
    Environment_Map = 1,            // Enables EnvMap Mask(TS6), EnvMap Scale
    Glow_Shader = 2,                // Enables Glow(TS3)
    Parallax = 3,                   // Enables Height(TS4)
    Face_Tint = 4,                  // Enables Detail(TS4), Tint(TS7)
    Skin_Tint = 5,                  // Enables Skin Tint Color
    Hair_Tint = 6,                  // Enables Hair Tint Color
    Parallax_Occ = 7,               // Enables Height(TS4), Max Passes, Scale. Unimplemented.
    Multitexture_Landscape = 8,
    LOD_Landscape = 9,
    Snow = 10,
    MultiLayer_Parallax = 11,       // Enables EnvMap Mask(TS6), Layer(TS7), Parallax Layer Thickness, Parallax Refraction Scale, Parallax Inner Layer U Scale, Parallax Inner Layer V Scale, EnvMap Scale
    Tree_Anim = 12,
    LOD_Objects = 13,
    Sparkle_Snow = 14,              // Enables SparkleParams
    LOD_Objects_HD = 15,
    Eye_Envmap = 16,                // Enables EnvMap Mask(TS6), Eye EnvMap Scale
    Cloud = 17,
    LOD_Landscape_Noise = 18,
    Multitexture_Landscape_LOD_Blend = 19,
    FO4_Dismemberment = 20
}

/// <summary>
/// An unsigned 32-bit integer, describing which float variable in BSEffectShaderProperty to animate.
/// </summary>
public enum EffectShaderControlledVariable : uint {
    EmissiveMultiple = 0,           // EmissiveMultiple.
    Falloff_Start_Angle = 1,        // Falloff Start Angle (degrees).
    Falloff_Stop_Angle = 2,         // Falloff Stop Angle (degrees).
    Falloff_Start_Opacity = 3,      // Falloff Start Opacity.
    Falloff_Stop_Opacity = 4,       // Falloff Stop Opacity.
    Alpha_Transparency = 5,         // Alpha Transparency (Emissive alpha?).
    U_Offset = 6,                   // U Offset.
    U_Scale = 7,                    // U Scale.
    V_Offset = 8,                   // V Offset.
    V_Scale = 9                     // V Scale.
}

/// <summary>
/// An unsigned 32-bit integer, describing which color in BSEffectShaderProperty to animate.
/// </summary>
public enum EffectShaderControlledColor : uint {
    Emissive_Color = 0              // Emissive Color.
}

/// <summary>
/// An unsigned 32-bit integer, describing which float variable in BSLightingShaderProperty to animate.
/// </summary>
public enum LightingShaderControlledVariable : uint {
    Refraction_Strength = 0,        // The amount of distortion.
    Environment_Map_Scale = 8,      // Environment Map Scale.
    Glossiness = 9,                 // Glossiness.
    Specular_Strength = 10,         // Specular Strength.
    Emissive_Multiple = 11,         // Emissive Multiple.
    Alpha = 12,                     // Alpha.
    U_Offset = 20,                  // U Offset.
    U_Scale = 21,                   // U Scale.
    V_Offset = 22,                  // V Offset.
    V_Scale = 23                    // V Scale.
}

/// <summary>
/// An unsigned 32-bit integer, describing which color in BSLightingShaderProperty to animate.
/// </summary>
public enum LightingShaderControlledColor : uint {
    Specular_Color = 0,             // Specular Color.
    Emissive_Color = 1              // Emissive Color.
}

/// <summary>
/// Bethesda Havok. Describes the type of bhkConstraint.
/// </summary>
public enum hkConstraintType : uint {
    BallAndSocket = 0,              // A ball and socket constraint.
    Hinge = 1,                      // A hinge constraint.
    Limited_Hinge = 2,              // A limited hinge constraint.
    Prismatic = 6,                  // A prismatic constraint.
    Ragdoll = 7,                    // A ragdoll constraint.
    StiffSpring = 8,                // A stiff spring constraint.
    Malleable = 13                  // A malleable constraint.
}

#endregion

#region Compounds

// SizedString -> r.ReadL32AString()
// string -> Y.String(r)
// ByteArray -> r.ReadL8Bytes()
// ByteMatrix -> ??
// Color3 -> new Color3(r)
// ByteColor3 -> new ByteColor3(r)
// Color4 -> new Color4(r)
// ByteColor4 -> new Color4Byte(r)
// FilePath -> ??

/// <summary>
/// The NIF file footer.
/// </summary>
public class Footer(BinaryReader r, Header h) {
    public int?[] Roots = h.V >= 0x0303000D ? r.ReadL32FArray(X<NiObject>.Ref) : default; // List of root NIF objects. If there is a camera, for 1st person view, then this NIF object is referred to as well in this list, even if it is not a root object (usually we want the camera to be attached to the Bip Head node).
}

/// <summary>
/// The distance range where a specific level of detail applies.
/// </summary>
public class LODRange(BinaryReader r, Header h) {
    public float NearExtent = r.ReadSingle();           // Begining of range.
    public float FarExtent = r.ReadSingle();            // End of Range.
    public uint[] UnknownInts = h.V <= 50397184 ? r.ReadUInt32() : default; // Unknown (0,0,0).
}

/// <summary>
/// Group of vertex indices of vertices that match.
/// </summary>
public class MatchGroup(BinaryReader r) {
    public ushort[] VertexIndices = r.ReadL16FArray(r => r.ReadUInt16()); // The vertex indices.
}

// ByteVector3 -> new Vector3(r.ReadByte(), r.ReadByte(), r.ReadByte())
// HalfVector3 -> r.ReadHalf()
// Vector3 -> r.ReadVector3()
// Vector4 -> r.ReadVector4()
// Quaternion -> r.ReadQuaternion()
// hkQuaternion -> r.ReadQuaternionWFirst()
// Matrix22 -> r.ReadMatrix2x2()
// Matrix33 -> r.ReadMatrix3x3()
// Matrix34 -> r.ReadMatrix3x4()
// Matrix44 -> r.ReadMatrix4x4()
// hkMatrix3 -> r.ReadMatrix3x4()
// MipMap -> ??
// NodeSet -> ??
// ShortString -> r.ReadL8AString()
/// <summary>
/// NiBoneLODController::SkinInfo. Reference to shape and skin instance.
/// </summary>
public class SkinInfo(BinaryReader r) {
    public int? Shape = X<NiTriBasedGeom>.Ptr(r);
    public int? SkinInstance = X<NiSkinInstance>.Ref(r);
}

/// <summary>
/// A set of NiBoneLODController::SkinInfo.
/// </summary>
public class SkinInfoSet(BinaryReader r) {
    public SkinInfo[] SkinInfo = r.ReadL32FArray(r => new SkinInfo(r));
}

/// <summary>
/// NiSkinData::BoneVertData. A vertex and its weight.
/// </summary>
public class BoneVertData(BinaryReader r) {
    public ushort Index = r.ReadUInt16();               // The vertex index, in the mesh.
    public float Weight = r.ReadSingle();               // The vertex weight - between 0.0 and 1.0
}

/// <summary>
/// NiSkinData::BoneVertData. A vertex and its weight.
/// </summary>
public class BoneVertDataHalf(BinaryReader r) {
    public ushort Index = r.ReadUInt16();               // The vertex index, in the mesh.
    public float Weight = r.ReadHalf();                 // The vertex weight - between 0.0 and 1.0
}

/// <summary>
/// Used in NiDefaultAVObjectPalette.
/// </summary>
public class AVObject(BinaryReader r) {
    public string Name = r.ReadL32AString();            // Object name.
    public int? AVObject = X<NiAVObject>.Ptr(r);        // Object reference.
}

/// <summary>
/// In a .kf file, this links to a controllable object, via its name (or for version 10.2.0.0 and up, a link and offset to a NiStringPalette that contains the name), and a sequence of interpolators that apply to this controllable object, via links.
/// For Controller ID, NiInterpController::GetCtlrID() virtual function returns a string formatted specifically for the derived type.
/// For Interpolator ID, NiInterpController::GetInterpolatorID() virtual function returns a string formatted specifically for the derived type.
/// The string formats are documented on the relevant niobject blocks.
/// </summary>
public class ControlledBlock(BinaryReader r, Header h) {
    public string TargetName = h.V <= 0x0A010067 ? Y.String(r) : default; // Name of a controllable object in another NIF file.
    public int? Interpolator = h.V >= 0x0A01006A ? X<NiInterpolator>.Ref(r) : default;
    public int? Controller = h.V <= 0x14050000 ? X<NiTimeController>.Ref(r) : default;
    public int? BlendInterpolator = h.V >= 0x0A010068 && h.V <= 0x0A01006E ? X<NiBlendInterpolator>.Ref(r) : default;
    public ushort BlendIndex = h.V >= 0x0A010068 && h.V <= 0x0A01006E ? r.ReadUInt16() : default;
    public byte Priority = h.V >= 0x0A01006A &&  ? r.ReadByte() : default; // Idle animations tend to have low values for this, and high values tend to correspond with the important parts of the animations.
    public string NodeName = h.V >= 0x14010001 ? Y.String(r) : default; // The name of the animated NiAVObject.
    public string PropertyType = h.V >= 0x14010001 ? Y.String(r) : default; // The RTTI type of the NiProperty the controller is attached to, if applicable.
    public string ControllerType = h.V >= 0x14010001 ? Y.String(r) : default; // The RTTI type of the NiTimeController.
    public string ControllerID = h.V >= 0x14010001 ? Y.String(r) : default; // An ID that can uniquely identify the controller among others of the same type on the same NiObjectNET.
    public string InterpolatorID = h.V >= 0x14010001 ? Y.String(r) : default; // An ID that can uniquely identify the interpolator among others of the same type on the same NiObjectNET.
    public int? StringPalette = h.V >= 0x0A020000 && h.V <= 0x14010000 ? X<NiStringPalette>.Ref(r) : default; // Refers to the NiStringPalette which contains the name of the controlled NIF object.
    public uint NodeNameOffset = h.V >= 0x0A020000 && h.V <= 0x14010000 ? r.ReadUInt32() : default; // Offset in NiStringPalette to the name of the animated NiAVObject.
    public uint PropertyTypeOffset = h.V >= 0x0A020000 && h.V <= 0x14010000 ? r.ReadUInt32() : default; // Offset in NiStringPalette to the RTTI type of the NiProperty the controller is attached to, if applicable.
    public uint ControllerTypeOffset = h.V >= 0x0A020000 && h.V <= 0x14010000 ? r.ReadUInt32() : default; // Offset in NiStringPalette to the RTTI type of the NiTimeController.
    public uint ControllerIDOffset = h.V >= 0x0A020000 && h.V <= 0x14010000 ? r.ReadUInt32() : default; // Offset in NiStringPalette to an ID that can uniquely identify the controller among others of the same type on the same NiObjectNET.
    public uint InterpolatorIDOffset = h.V >= 0x0A020000 && h.V <= 0x14010000 ? r.ReadUInt32() : default; // Offset in NiStringPalette to an ID that can uniquely identify the interpolator among others of the same type on the same NiObjectNET.
}

/// <summary>
/// Information about how the file was exported
/// </summary>
public class ExportInfo(BinaryReader r) {
    public string Author = r.ReadL8AString();
    public string ProcessScript = r.ReadL8AString();
    public string ExportScript = r.ReadL8AString();
}

/// <summary>
/// The NIF file header.
/// </summary>
public class Header(BinaryReader r, Header h) {
    public string HeaderString = ??;                    // 'NetImmerse File Format x.x.x.x' (versions <= 10.0.1.2) or 'Gamebryo File Format x.x.x.x' (versions >= 10.1.0.0), with x.x.x.x the version written out. Ends with a newline character (0x0A).
    public string[] Copyright = h.V <= 0x03010000 ? ?? : default;
    public uint Version = h.V >= 0x03010001 ? r.ReadUInt32() : default; // The NIF version, in hexadecimal notation: 0x04000002, 0x0401000C, 0x04020002, 0x04020100, 0x04020200, 0x0A000100, 0x0A010000, 0x0A020000, 0x14000004, ...
    public EndianType EndianType = h.V >= 0x14000003 ? (EndianType)r.ReadByte() : default; // Determines the endianness of the data in the file.
    public uint UserVersion = h.V >= 0x0A000108 ? r.ReadUInt32() : default; // An extra version number, for companies that decide to modify the file format.
    public uint NumBlocks = h.V >= 0x03010001 ? r.ReadUInt32() : default; // Number of file objects.
    public uint UserVersion2 = ((Version == 20.2.0.7) || (Version == 20.0.0.5) || ((Version >= 10.0.1.2) && (Version <= 20.0.0.4) && (h.UserVersion <= 11))) && (h.UserVersion >= 3) ? r.ReadUInt32() : default;
    public ExportInfo ExportInfo =               ((Version == 20.2.0.7) || (Version == 20.0.0.5) || ((Version >= 10.0.1.2) && (Version <= 20.0.0.4) && (h.UserVersion <= 11))) && (h.UserVersion >= 3) ? new ExportInfo(r) : default;
    public string MaxFilepath = (h.UserVersion2 == 130) ? r.ReadL8AString() : default;
    public byte[] Metadata = h.V >= 0x1E000000 ? r.ReadL8Bytes() : default;
    public ushort NumBlockTypes = h.V >= 0x05000001 ? r.ReadUInt16() : default; // Number of object types in this NIF file.
    public string[] BlockTypes = Version != 20.3.1.2 && h.V >= 0x05000001 ? r.ReadL32AString() : default; // List of all object types used in this NIF file.
    public uint[] BlockTypeHashes = h.V >= 0x14030102 && h.V <= 0x14030102 ? r.ReadUInt32() : default; // List of all object types used in this NIF file.
    public ushort[] BlockTypeIndex = h.V >= 0x05000001 ? r.ReadUInt16() : default; // Maps file objects on their corresponding type: first file object is of type object_types[object_type_index[0]], the second of object_types[object_type_index[1]], etc.
    public uint[] BlockSize = h.V >= 0x14020005 ? r.ReadUInt32() : default; // Array of block sizes?
    public uint NumStrings = h.V >= 0x14010001 ? r.ReadUInt32() : default; // Number of strings.
    public uint MaxStringLength = h.V >= 0x14010001 ? r.ReadUInt32() : default; // Maximum string length.
    public string[] Strings = h.V >= 0x14010001 ? r.ReadL32AString() : default; // Strings.
    public uint[] Groups = h.V >= 0x05000006 ? r.ReadL32FArray(r => r.ReadUInt32()) : default;
}

/// <summary>
/// A list of \\0 terminated strings.
/// </summary>
public class StringPalette(BinaryReader r) {
    public string Palette = r.ReadL32AString();         // A bunch of 0x00 seperated strings.
    public uint Length = r.ReadUInt32();                // Length of the palette string is repeated here.
}

/// <summary>
/// Tension, bias, continuity.
/// </summary>
public class TBC(BinaryReader r) {
    public float t = r.ReadSingle();                    // Tension.
    public float b = r.ReadSingle();                    // Bias.
    public float c = r.ReadSingle();                    // Continuity.
}

/// <summary>
/// A generic key with support for interpolation. Type 1 is normal linear interpolation, type 2 has forward and backward tangents, and type 3 has tension, bias and continuity arguments. Note that color4 and byte always seem to be of type 1.
/// </summary>
public class Key(BinaryReader r, Header h) {
    public float Time = r.ReadSingle();                 // Time of the key.
    public T Value = ??;                                // The key value.
    public T Forward = ARG == 2 ? ?? : default;         // Key forward tangent.
    public T Backward = ARG == 2 ? ?? : default;        // The key backward tangent.
    public TBC TBC = ARG == 3 ? new TBC(r) : default;   // The TBC of the key.
}

/// <summary>
/// Array of vector keys (anything that can be interpolated, except rotations).
/// </summary>
public class KeyGroup(BinaryReader r, Header h) {
    public uint NumKeys = r.ReadUInt32();               // Number of keys in the array.
    public KeyType Interpolation = Num Keys != 0 ? (KeyType)r.ReadUInt32() : default; // The key type.
    public Key[] Keys = new Key(r, h);                  // The keys.
}

/// <summary>
/// A special version of the key type used for quaternions.  Never has tangents.
/// </summary>
public class QuatKey(BinaryReader r, Header h) {
    public float Time = ARG != 4 && h.V >= 0x0A01006A ? r.ReadSingle() : default; // Time the key applies.
    public T Value = ARG != 4 ? ?? : default;           // Value of the key.
    public TBC TBC = ARG == 3 ? new TBC(r) : default;   // The TBC of the key.
}

/// <summary>
/// Texture coordinates (u,v). As in OpenGL; image origin is in the lower left corner.
/// </summary>
public class TexCoord(BinaryReader r) {
    public float u = r.ReadSingle();                    // First coordinate.
    public float v = r.ReadSingle();                    // Second coordinate.
}

/// <summary>
/// Texture coordinates (u,v).
/// </summary>
public class HalfTexCoord(BinaryReader r) {
    public float u = r.ReadHalf();                      // First coordinate.
    public float v = r.ReadHalf();                      // Second coordinate.
}

/// <summary>
/// Describes the order of scaling and rotation matrices. Translate, Scale, Rotation, Center are from TexDesc.
/// Back = inverse of Center. FromMaya = inverse of the V axis with a positive translation along V of 1 unit.
/// </summary>
public enum TransformMethod : uint {
    Maya_Deprecated = 0,            // Center * Rotation * Back * Translate * Scale
    Max = 1,                        // Center * Scale * Rotation * Translate * Back
    Maya = 2                        // Center * Rotation * Back * FromMaya * Translate * Scale
}

/// <summary>
/// NiTexturingProperty::Map. Texture description.
/// </summary>
public class TexDesc(BinaryReader r, Header h) {
    public int? Image = h.V <= 50397184 ? X<NiImage>.Ref(r) : default; // Link to the texture image.
    public int? Source = h.V >= 0x0303000D ? X<NiSourceTexture>.Ref(r) : default; // NiSourceTexture object index.
    public TexClampMode ClampMode = h.V <= 0x14000005 ? (TexClampMode)r.ReadUInt32() : default; // 0=clamp S clamp T, 1=clamp S wrap T, 2=wrap S clamp T, 3=wrap S wrap T
    public TexFilterMode FilterMode = h.V <= 0x14000005 ? (TexFilterMode)r.ReadUInt32() : default; // 0=nearest, 1=bilinear, 2=trilinear, 3=..., 4=..., 5=...
    public Flags Flags = h.V >= 0x14010003 ? (Flags)r.ReadUInt16() : default; // Texture mode flags; clamp and filter mode stored in upper byte with 0xYZ00 = clamp mode Y, filter mode Z.
    public ushort MaxAnisotropy = h.V >= 0x14050004 ? r.ReadUInt16() : default;
    public uint UVSet = h.V <= 0x14000005 ? r.ReadUInt32() : default; // The texture coordinate set in NiGeometryData that this texture slot will use.
    public short PS2L = h.V <= 0x0A040001 ? r.ReadInt16() : default; // L can range from 0 to 3 and are used to specify how fast a texture gets blurry.
    public short PS2K = h.V <= 0x0A040001 ? r.ReadInt16() : default; // K is used as an offset into the mipmap levels and can range from -2047 to 2047. Positive values push the mipmap towards being blurry and negative values make the mipmap sharper.
    public ushort Unknown1 = h.V <= 0x0401000C ? r.ReadUInt16() : default; // Unknown, 0 or 0x0101?
    public bool HasTextureTransform = h.V >= 0x0A010000 ? r.ReadBool32() : default; // Whether or not the texture coordinates are transformed.
    public TexCoord Translation = Has Texture Transform && h.V >= 0x0A010000 ? new TexCoord(r) : default; // The UV translation.
    public TexCoord Scale = Has Texture Transform && h.V >= 0x0A010000 ? new TexCoord(r) : default; // The UV scale.
    public float Rotation = Has Texture Transform && h.V >= 0x0A010000 ? r.ReadSingle() : default; // The W axis rotation in texture space.
    public TransformMethod TransformMethod = Has Texture Transform && h.V >= 0x0A010000 ? (TransformMethod)r.ReadUInt32() : default; // Depending on the source, scaling can occur before or after rotation.
    public TexCoord Center = Has Texture Transform && h.V >= 0x0A010000 ? new TexCoord(r) : default; // The origin around which the texture rotates.
}

/// <summary>
/// NiTexturingProperty::ShaderMap. Shader texture description.
/// </summary>
public class ShaderTexDesc(BinaryReader r, Header h) {
    public bool HasMap = r.ReadBool32();
    public TexDesc Map = Has Map ? new TexDesc(r, h) : default;
    public uint MapID = Has Map ? r.ReadUInt32() : default; // Unique identifier for the Gamebryo shader system.
}

/// <summary>
/// List of three vertex indices.
/// </summary>
public class Triangle(BinaryReader r) {
    public ushort v1 = r.ReadUInt16();                  // First vertex index.
    public ushort v2 = r.ReadUInt16();                  // Second vertex index.
    public ushort v3 = r.ReadUInt16();                  // Third vertex index.
}

[Flags]
public enum VertexFlags : ushort {
    Vertex = 1 << 4,
    UVs = 1 << 5,
    UVs_2 = 1 << 6,
    Normals = 1 << 7,
    Tangents = 1 << 8,
    Vertex_Colors = 1 << 9,
    Skinned = 1 << 10,
    Land_Data = 1 << 11,
    Eye_Data = 1 << 12,
    Instance = 1 << 13,
    Full_Precision = 1 << 14
}

public class BSVertexData(BinaryReader r, Header h) {
    public Vector3 Vertex = ((ARG & 16) != 0) && ((ARG & 16384) != 0) ? r.ReadVector3() : default;
    public float BitangentX = ((ARG & 16) != 0) && ((ARG & 256) != 0) && ((ARG & 16384) != 0) ? r.ReadSingle() : default;
    public ushort UnknownShort = ((ARG & 16) != 0) && ((ARG & 256) == 0) && ((ARG & 16384) == 0) ? r.ReadUInt16() : default;
    public uint UnknownInt = ((ARG & 16) != 0) && ((ARG & 256) == 0) && ((ARG & 16384) != 0) ? r.ReadUInt32() : default;
    public HalfTexCoord UV = ((ARG & 32) != 0) ? new HalfTexCoord(r) : default;
    public Vector3<false> Normal = (ARG & 128) != 0 ? new Vector3(r.ReadByte(), r.ReadByte(), r.ReadByte()) : default;
    public byte BitangentY = (ARG & 128) != 0 ? r.ReadByte() : default;
    public Vector3<false> Tangent = ((ARG & 128) != 0) && ((ARG & 256) != 0) ? new Vector3(r.ReadByte(), r.ReadByte(), r.ReadByte()) : default;
    public byte BitangentZ = ((ARG & 128) != 0) && ((ARG & 256) != 0) ? r.ReadByte() : default;
    public ByteColor4 VertexColors = (ARG & 512) != 0 ? new Color4Byte(r) : default;
    public float[] BoneWeights = (ARG & 1024) != 0 ? r.ReadHalf() : default;
    public byte[] BoneIndices = (ARG & 1024) != 0 ? r.ReadByte() : default;
    public float EyeData = (ARG & 4096) != 0 ? r.ReadSingle() : default;
}

public class BSVertexDataSSE(BinaryReader r, Header h) {
    public Vector3 Vertex = ((ARG & 16) != 0) ? r.ReadVector3() : default;
    public float BitangentX = ((ARG & 16) != 0) && ((ARG & 256) != 0) ? r.ReadSingle() : default;
    public int UnknownInt = ((ARG & 16) != 0) && (ARG & 256) == 0 ? r.ReadInt32() : default;
    public HalfTexCoord UV = ((ARG & 32) != 0) ? new HalfTexCoord(r) : default;
    public Vector3<false> Normal = (ARG & 128) != 0 ? new Vector3(r.ReadByte(), r.ReadByte(), r.ReadByte()) : default;
    public byte BitangentY = (ARG & 128) != 0 ? r.ReadByte() : default;
    public Vector3<false> Tangent = ((ARG & 128) != 0) && ((ARG & 256) != 0) ? new Vector3(r.ReadByte(), r.ReadByte(), r.ReadByte()) : default;
    public byte BitangentZ = ((ARG & 128) != 0) && ((ARG & 256) != 0) ? r.ReadByte() : default;
    public ByteColor4 VertexColors = (ARG & 512) != 0 ? new Color4Byte(r) : default;
    public float[] BoneWeights = (ARG & 1024) != 0 ? r.ReadHalf() : default;
    public byte[] BoneIndices = (ARG & 1024) != 0 ? r.ReadByte() : default;
    public float EyeData = (ARG & 4096) != 0 ? r.ReadSingle() : default;
}

public class BSVertexDesc(BinaryReader r) {
    public byte VF1 = r.ReadByte();
    public byte VF2 = r.ReadByte();
    public byte VF3 = r.ReadByte();
    public byte VF4 = r.ReadByte();
    public byte VF5 = r.ReadByte();
    public VertexFlags VertexAttributes = (VertexFlags)r.ReadUInt16();
    public byte VF8 = r.ReadByte();
}

/// <summary>
/// Skinning data for a submesh, optimized for hardware skinning. Part of NiSkinPartition.
/// </summary>
public class SkinPartition(BinaryReader r, Header h) {
    public ushort NumVertices = r.ReadUInt16();         // Number of vertices in this submesh.
    public ushort NumTriangles = r.ReadUInt16();        // Number of triangles in this submesh.
    public ushort NumBones = r.ReadUInt16();            // Number of bones influencing this submesh.
    public ushort NumStrips = r.ReadUInt16();           // Number of strips in this submesh (zero if not stripped).
    public ushort NumWeightsPerVertex = r.ReadUInt16(); // Number of weight coefficients per vertex. The Gamebryo engine seems to work well only if this number is equal to 4, even if there are less than 4 influences per vertex.
    public ushort[] Bones = r.ReadUInt16();             // List of bones.
    public bool HasVertexMap = h.V >= 0x0A010000 ? r.ReadBool32() : default; // Do we have a vertex map?
    public ushort[] VertexMap = Has Vertex Map && h.V >= 0x0A010000 ? r.ReadUInt16() : default; // Maps the weight/influence lists in this submesh to the vertices in the shape being skinned.
    public bool HasVertexWeights = h.V >= 0x0A010000 ? r.ReadBool32() : default; // Do we have vertex weights?
    public float[][] VertexWeights = Has Vertex Weights == 15 && h.V >= 0x14030101 ? r.ReadHalf() : default; // The vertex weights.
    public ushort[] StripLengths = r.ReadUInt16();      // The strip lengths.
    public bool HasFaces = h.V >= 0x0A010000 ? r.ReadBool32() : default; // Do we have triangle or strip data?
    public ushort[][] Strips = (Has Faces) && (Num Strips != 0) && h.V >= 0x0A010000 ? r.ReadUInt16() : default; // The strips.
    public Triangle[] Triangles = (Has Faces) && (Num Strips == 0) && h.V >= 0x0A010000 ? new Triangle(r) : default; // The triangles.
    public bool HasBoneIndices = r.ReadBool32();        // Do we have bone indices?
    public byte[][] BoneIndices = Has Bone Indices ? r.ReadByte() : default; // Bone indices, they index into 'Bones'.
    public ushort UnknownShort = r.ReadUInt16();        // Unknown
    public BSVertexDesc VertexDesc = (h.UserVersion2 == 100) ? new BSVertexDesc(r) : default;
    public Triangle[] TrianglesCopy = (h.UserVersion2 == 100) ? new Triangle(r) : default;
}

/// <summary>
/// A plane.
/// </summary>
public class NiPlane(BinaryReader r) {
    public Vector3 Normal = r.ReadVector3();            // The plane normal.
    public float Constant = r.ReadSingle();             // The plane constant.
}

/// <summary>
/// A sphere.
/// </summary>
public class NiBound(BinaryReader r) {
    public Vector3 Center = r.ReadVector3();            // The sphere's center.
    public float Radius = r.ReadSingle();               // The sphere's radius.
}

public class NiQuatTransform(BinaryReader r, Header h) {
    public Vector3 Translation = r.ReadVector3();
    public Quaternion Rotation = r.ReadQuaternion();
    public float Scale = r.ReadSingle();
    public bool[] TRSValid = h.V <= 0x0A01006D ? r.ReadBool32() : default; // Whether each transform component is valid.
}

public class NiTransform(BinaryReader r) {
    public Matrix3x3 Rotation = r.ReadMatrix3x3();      // The rotation part of the transformation matrix.
    public Vector3 Translation = r.ReadVector3();       // The translation vector.
    public float Scale = r.ReadSingle();                // Scaling part (only uniform scaling is supported).
}

/// <summary>
/// Bethesda Animation. Furniture entry points. It specifies the direction(s) from where the actor is able to enter (and leave) the position.
/// </summary>
[Flags]
public enum FurnitureEntryPoints : ushort {
    Front = 0,                      // front entry point
    Behind = 1 << 1,                // behind entry point
    Right = 1 << 2,                 // right entry point
    Left = 1 << 3,                  // left entry point
    Up = 1 << 4                     // up entry point - unknown function. Used on some beds in Skyrim, probably for blocking of sleeping position.
}

/// <summary>
/// Bethesda Animation. Animation type used on this position. This specifies the function of this position.
/// </summary>
public enum AnimationType : ushort {
    Sit = 1,                        // Actor use sit animation.
    Sleep = 2,                      // Actor use sleep animation.
    Lean = 4                        // Used for lean animations?
}

/// <summary>
/// Bethesda Animation. Describes a furniture position?
/// </summary>
public class FurniturePosition(BinaryReader r) {
    public Vector3 Offset = r.ReadVector3();            // Offset of furniture marker.
    public ushort Orientation = r.ReadUInt16();         // Furniture marker orientation.
    public byte PositionRef1 = r.ReadByte();            // Refers to a furnituremarkerxx.nif file. Always seems to be the same as Position Ref 2.
    public byte PositionRef2 = r.ReadByte();            // Refers to a furnituremarkerxx.nif file. Always seems to be the same as Position Ref 1.
    public float Heading = r.ReadSingle();              // Similar to Orientation, in float form.
    public AnimationType AnimationType = (AnimationType)r.ReadUInt16(); // Unknown
    public FurnitureEntryPoints EntryProperties = (FurnitureEntryPoints)r.ReadUInt16(); // Unknown/unused in nif?
}

/// <summary>
/// Bethesda Havok. A triangle with extra data used for physics.
/// </summary>
public class TriangleData(BinaryReader r, Header h) {
    public Triangle Triangle = new Triangle(r);         // The triangle.
    public ushort WeldingInfo = r.ReadUInt16();         // Additional havok information on how triangles are welded.
    public Vector3 Normal = h.V <= 0x14000005 ? r.ReadVector3() : default; // This is the triangle's normal.
}

/// <summary>
/// Geometry morphing data component.
/// </summary>
public class Morph(BinaryReader r, Header h) {
    public string FrameName = h.V >= 0x0A01006A ? Y.String(r) : default; // Name of the frame.
    public uint NumKeys = h.V <= 0x0A010000 ? r.ReadUInt32() : default; // The number of morph keys that follow.
    public KeyType Interpolation = h.V <= 0x0A010000 ? (KeyType)r.ReadUInt32() : default; // Unlike most objects, the presense of this value is not conditional on there being keys.
    public Key[] Keys = h.V <= 0x0A010000 ? new Key(r, h) : default; // The morph key frames.
    public float LegacyWeight = h.V >= 0x0A010068 && h.V <= 0x14010002 &&  ? r.ReadSingle() : default;
    public Vector3[] Vectors = r.ReadVector3();         // Morph vectors.
}

/// <summary>
/// particle array entry
/// </summary>
public class Particle(BinaryReader r) {
    public Vector3 Velocity = r.ReadVector3();          // Particle velocity
    public Vector3 UnknownVector = r.ReadVector3();     // Unknown
    public float Lifetime = r.ReadSingle();             // The particle age.
    public float Lifespan = r.ReadSingle();             // Maximum age of the particle.
    public float Timestamp = r.ReadSingle();            // Timestamp of the last update.
    public ushort UnknownShort = r.ReadUInt16();        // Unknown short
    public ushort VertexID = r.ReadUInt16();            // Particle/vertex index matches array index
}

/// <summary>
/// NiSkinData::BoneData. Skinning data component.
/// </summary>
public class BoneData(BinaryReader r, Header h) {
    public NiTransform SkinTransform = new NiTransform(r); // Offset of the skin from this bone in bind position.
    public Vector3 BoundingSphereOffset = r.ReadVector3(); // Translation offset of a bounding sphere holding all vertices. (Note that its a Sphere Containing Axis Aligned Box not a minimum volume Sphere)
    public float BoundingSphereRadius = r.ReadSingle(); // Radius for bounding sphere holding all vertices.
    public short[] Unknown13Shorts = h.V >= 0x14030009 && h.V <= 0x14030009 &&  ? r.ReadInt16() : default; // Unknown, always 0?
    public ushort NumVertices = r.ReadUInt16();         // Number of weighted vertices.
    public BoneVertDataHalf[] VertexWeights = ARG == 15 && h.V >= 0x14030101 ? new BoneVertDataHalf(r) : default; // The vertex weights.
}

/// <summary>
/// Bethesda Havok. Collision filter info representing Layer, Flags, Part Number, and Group all combined into one uint.
/// </summary>
public class HavokFilter(BinaryReader r, Header h) {
    public SkyrimLayer Layer = (SkyrimLayer)r.ReadByte(); // The layer the collision belongs to.
    public byte FlagsandPartNumber = r.ReadByte();      // FLAGS are stored in highest 3 bits:
                                                        // 	Bit 7: sets the LINK property and controls whether this body is physically linked to others.
                                                        // 	Bit 6: turns collision off (not used for Layer BIPED).
                                                        // 	Bit 5: sets the SCALED property.
                                                        // 
                                                        // 	PART NUMBER is stored in bits 0-4. Used only when Layer is set to BIPED.
                                                        // 
                                                        // 	Part Numbers for Oblivion, Fallout 3, Skyrim:
                                                        // 	0 - OTHER
                                                        // 	1 - HEAD
                                                        // 	2 - BODY
                                                        // 	3 - SPINE1
                                                        // 	4 - SPINE2
                                                        // 	5 - LUPPERARM
                                                        // 	6 - LFOREARM
                                                        // 	7 - LHAND
                                                        // 	8 - LTHIGH
                                                        // 	9 - LCALF
                                                        // 	10 - LFOOT
                                                        // 	11 - RUPPERARM
                                                        // 	12 - RFOREARM
                                                        // 	13 - RHAND
                                                        // 	14 - RTHIGH
                                                        // 	15 - RCALF
                                                        // 	16 - RFOOT
                                                        // 	17 - TAIL
                                                        // 	18 - SHIELD
                                                        // 	19 - QUIVER
                                                        // 	20 - WEAPON
                                                        // 	21 - PONYTAIL
                                                        // 	22 - WING
                                                        // 	23 - PACK
                                                        // 	24 - CHAIN
                                                        // 	25 - ADDONHEAD
                                                        // 	26 - ADDONCHEST
                                                        // 	27 - ADDONARM
                                                        // 	28 - ADDONLEG
                                                        // 	29-31 - NULL
    public ushort Group = r.ReadUInt16();
}

/// <summary>
/// Bethesda Havok. Material wrapper for varying material enums by game.
/// </summary>
public class HavokMaterial(BinaryReader r, Header h) {
    public uint UnknownInt = h.V <= 0x0A000102 ? r.ReadUInt32() : default;
    public SkyrimHavokMaterial Material = (SkyrimHavokMaterial)r.ReadUInt32(); // The material of the shape.
}

/// <summary>
/// Bethesda Havok. Havok Information for packed TriStrip shapes.
/// </summary>
public class OblivionSubShape(BinaryReader r) {
    public HavokFilter HavokFilter = new HavokFilter(r, h);
    public uint NumVertices = r.ReadUInt32();           // The number of vertices that form this sub shape.
    public HavokMaterial Material = new HavokMaterial(r, h); // The material of the subshape.
}

public class bhkPositionConstraintMotor(BinaryReader r) {
    public float MinForce = r.ReadSingle();             // Minimum motor force
    public float MaxForce = r.ReadSingle();             // Maximum motor force
    public float Tau = r.ReadSingle();                  // Relative stiffness
    public float Damping = r.ReadSingle();              // Motor damping value
    public float ProportionalRecoveryVelocity = r.ReadSingle(); // A factor of the current error to calculate the recovery velocity
    public float ConstantRecoveryVelocity = r.ReadSingle(); // A constant velocity which is used to recover from errors
    public bool MotorEnabled = r.ReadBool32();          // Is Motor enabled
}

public class bhkVelocityConstraintMotor(BinaryReader r) {
    public float MinForce = r.ReadSingle();             // Minimum motor force
    public float MaxForce = r.ReadSingle();             // Maximum motor force
    public float Tau = r.ReadSingle();                  // Relative stiffness
    public float TargetVelocity = r.ReadSingle();
    public bool UseVelocityTarget = r.ReadBool32();
    public bool MotorEnabled = r.ReadBool32();          // Is Motor enabled
}

public class bhkSpringDamperConstraintMotor(BinaryReader r) {
    public float MinForce = r.ReadSingle();             // Minimum motor force
    public float MaxForce = r.ReadSingle();             // Maximum motor force
    public float SpringConstant = r.ReadSingle();       // The spring constant in N/m
    public float SpringDamping = r.ReadSingle();        // The spring damping in Nsec/m
    public bool MotorEnabled = r.ReadBool32();          // Is Motor enabled
}

public enum MotorType : byte {
    MOTOR_NONE = 0,
    MOTOR_POSITION = 1,
    MOTOR_VELOCITY = 2,
    MOTOR_SPRING = 3
}

public class MotorDescriptor(BinaryReader r, Header h) {
    public MotorType Type = (MotorType)r.ReadByte();
    public bhkPositionConstraintMotor PositionMotor = Type == 1 ? new bhkPositionConstraintMotor(r) : default;
    public bhkVelocityConstraintMotor VelocityMotor = Type == 2 ? new bhkVelocityConstraintMotor(r) : default;
    public bhkSpringDamperConstraintMotor SpringDamperMotor = Type == 3 ? new bhkSpringDamperConstraintMotor(r) : default;
}

/// <summary>
/// This constraint defines a cone in which an object can rotate. The shape of the cone can be controlled in two (orthogonal) directions.
/// </summary>
public class RagdollDescriptor(BinaryReader r, Header h) {
    public Vector4 PivotA = r.ReadVector4();            // Point around which the object will rotate. Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    public Vector4 PlaneA = r.ReadVector4();            // Defines the orthogonal plane in which the body can move, the orthogonal directions in which the shape can be controlled (the direction orthogonal on this one and Twist A).
    public Vector4 TwistA = r.ReadVector4();            // Central directed axis of the cone in which the object can rotate. Orthogonal on Plane A.
    public Vector4 PivotB = r.ReadVector4();            // Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    public Vector4 PlaneB = r.ReadVector4();            // Defines the orthogonal plane in which the body can move, the orthogonal directions in which the shape can be controlled (the direction orthogonal on this one and Twist A).
    public Vector4 TwistB = r.ReadVector4();            // Central directed axis of the cone in which the object can rotate. Orthogonal on Plane B.
    public Vector4 MotorA = r.ReadVector4();            // Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    public Vector4 MotorB = r.ReadVector4();            // Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    public float ConeMaxAngle = r.ReadSingle();         // Maximum angle the object can rotate around the vector orthogonal on Plane A and Twist A relative to the Twist A vector. Note that Cone Min Angle is not stored, but is simply minus this angle.
    public float PlaneMinAngle = r.ReadSingle();        // Minimum angle the object can rotate around Plane A, relative to Twist A.
    public float PlaneMaxAngle = r.ReadSingle();        // Maximum angle the object can rotate around Plane A, relative to Twist A.
    public float TwistMinAngle = r.ReadSingle();        // Minimum angle the object can rotate around Twist A, relative to Plane A.
    public float TwistMaxAngle = r.ReadSingle();        // Maximum angle the object can rotate around Twist A, relative to Plane A.
    public float MaxFriction = r.ReadSingle();          // Maximum friction, typically 0 or 10. In Fallout 3, typically 100.
    public MotorDescriptor Motor = h.V >= 0x14020007 &&  ? new MotorDescriptor(r, h) : default;
}

/// <summary>
/// This constraint allows rotation about a specified axis, limited by specified boundaries.
/// </summary>
public class LimitedHingeDescriptor(BinaryReader r, Header h) {
    public Vector4 PivotA = r.ReadVector4();            // Pivot point around which the object will rotate.
    public Vector4 AxleA = r.ReadVector4();             // Axis of rotation.
    public Vector4 Perp2AxleInA1 = r.ReadVector4();     // Vector in the rotation plane which defines the zero angle.
    public Vector4 Perp2AxleInA2 = r.ReadVector4();     // Vector in the rotation plane, orthogonal on the previous one, which defines the positive direction of rotation. This is always the vector product of Axle A and Perp2 Axle In A1.
    public Vector4 PivotB = r.ReadVector4();            // Pivot A in second entity coordinate system.
    public Vector4 AxleB = r.ReadVector4();             // Axle A in second entity coordinate system.
    public Vector4 Perp2AxleInB2 = r.ReadVector4();     // Perp2 Axle In A2 in second entity coordinate system.
    public Vector4 Perp2AxleInB1 = r.ReadVector4();     // Perp2 Axle In A1 in second entity coordinate system.
    public float MinAngle = r.ReadSingle();             // Minimum rotation angle.
    public float MaxAngle = r.ReadSingle();             // Maximum rotation angle.
    public float MaxFriction = r.ReadSingle();          // Maximum friction, typically either 0 or 10. In Fallout 3, typically 100.
    public MotorDescriptor Motor = h.V >= 0x14020007 &&  ? new MotorDescriptor(r, h) : default;
}

/// <summary>
/// This constraint allows rotation about a specified axis.
/// </summary>
public class HingeDescriptor(BinaryReader r, Header h) {
    public Vector4 PivotA = h.V >= 0x14020007 ? r.ReadVector4() : default; // Pivot point around which the object will rotate.
    public Vector4 Perp2AxleInA1 = h.V >= 0x14020007 ? r.ReadVector4() : default; // Vector in the rotation plane which defines the zero angle.
    public Vector4 Perp2AxleInA2 = h.V >= 0x14020007 ? r.ReadVector4() : default; // Vector in the rotation plane, orthogonal on the previous one, which defines the positive direction of rotation. This is always the vector product of Axle A and Perp2 Axle In A1.
    public Vector4 PivotB = h.V >= 0x14020007 ? r.ReadVector4() : default; // Pivot A in second entity coordinate system.
    public Vector4 AxleB = h.V >= 0x14020007 ? r.ReadVector4() : default; // Axle A in second entity coordinate system.
    public Vector4 AxleA = h.V >= 0x14020007 ? r.ReadVector4() : default; // Axis of rotation.
    public Vector4 Perp2AxleInB1 = h.V >= 0x14020007 ? r.ReadVector4() : default; // Perp2 Axle In A1 in second entity coordinate system.
    public Vector4 Perp2AxleInB2 = h.V >= 0x14020007 ? r.ReadVector4() : default; // Perp2 Axle In A2 in second entity coordinate system.
}

public class BallAndSocketDescriptor(BinaryReader r) {
    public Vector4 PivotA = r.ReadVector4();            // Pivot point in the local space of entity A.
    public Vector4 PivotB = r.ReadVector4();            // Pivot point in the local space of entity B.
}

public class PrismaticDescriptor(BinaryReader r, Header h) {
    public Vector4 PivotA = h.V >= 0x14020007 ? r.ReadVector4() : default; // Pivot.
    public Vector4 RotationA = h.V >= 0x14020007 ? r.ReadVector4() : default; // Rotation axis.
    public Vector4 PlaneA = h.V >= 0x14020007 ? r.ReadVector4() : default; // Plane normal. Describes the plane the object is able to move on.
    public Vector4 SlidingA = h.V >= 0x14020007 ? r.ReadVector4() : default; // Describes the axis the object is able to travel along. Unit vector.
    public Vector4 PivotB = h.V >= 0x14020007 ? r.ReadVector4() : default; // Pivot in B coordinates.
    public Vector4 RotationB = h.V >= 0x14020007 ? r.ReadVector4() : default; // Rotation axis.
    public Vector4 PlaneB = h.V >= 0x14020007 ? r.ReadVector4() : default; // Plane normal. Describes the plane the object is able to move on in B coordinates.
    public Vector4 SlidingB = h.V >= 0x14020007 ? r.ReadVector4() : default; // Describes the axis the object is able to travel along in B coordinates. Unit vector.
    public float MinDistance = r.ReadSingle();          // Describe the min distance the object is able to travel.
    public float MaxDistance = r.ReadSingle();          // Describe the max distance the object is able to travel.
    public float Friction = r.ReadSingle();             // Friction.
    public MotorDescriptor Motor = h.V >= 0x14020007 &&  ? new MotorDescriptor(r, h) : default;
}

public class StiffSpringDescriptor(BinaryReader r) {
    public Vector4 PivotA = r.ReadVector4();
    public Vector4 PivotB = r.ReadVector4();
    public float Length = r.ReadSingle();
}

/// <summary>
/// Used to store skin weights in NiTriShapeSkinController.
/// </summary>
public class OldSkinData(BinaryReader r) {
    public float VertexWeight = r.ReadSingle();         // The amount that this bone affects the vertex.
    public ushort VertexIndex = r.ReadUInt16();         // The index of the vertex that this weight applies to.
    public Vector3 UnknownVector = r.ReadVector3();     // Unknown.  Perhaps some sort of offset?
}

/// <summary>
/// Determines how the raw image data is stored in NiRawImageData.
/// </summary>
public enum ImageType : uint {
    RGB = 1,                        // Colors store red, blue, and green components.
    RGBA = 2                        // Colors store red, blue, green, and alpha components.
}

/// <summary>
/// Box Bounding Volume
/// </summary>
public class BoxBV(BinaryReader r) {
    public Vector3 Center = r.ReadVector3();
    public Vector3[] Axis = r.ReadVector3();
    public Vector3 Extent = r.ReadVector3();
}

/// <summary>
/// Capsule Bounding Volume
/// </summary>
public class CapsuleBV(BinaryReader r) {
    public Vector3 Center = r.ReadVector3();
    public Vector3 Origin = r.ReadVector3();
    public float Extent = r.ReadSingle();
    public float Radius = r.ReadSingle();
}

public class HalfSpaceBV(BinaryReader r) {
    public NiPlane Plane = new NiPlane(r);
    public Vector3 Center = r.ReadVector3();
}

public class BoundingVolume(BinaryReader r, Header h) {
    public BoundVolumeType CollisionType = (BoundVolumeType)r.ReadUInt32(); // Type of collision data.
    public NiBound Sphere = Collision Type == 0 ? new NiBound(r) : default;
    public BoxBV Box = Collision Type == 1 ? new BoxBV(r) : default;
    public CapsuleBV Capsule = Collision Type == 2 ? new CapsuleBV(r) : default;
    public UnionBV Union = Collision Type == 4 ? new UnionBV(r) : default;
    public HalfSpaceBV HalfSpace = Collision Type == 5 ? new HalfSpaceBV(r) : default;
}

public class UnionBV(BinaryReader r) {
    public BoundingVolume[] BoundingVolumes = r.ReadL32FArray(r => new BoundingVolume(r, h));
}

public class MorphWeight(BinaryReader r) {
    public int? Interpolator = X<NiInterpolator>.Ref(r);
    public float Weight = r.ReadSingle();
}

/// <summary>
/// Transformation data for the bone at this index in bhkPoseArray.
/// </summary>
public class BoneTransform(BinaryReader r) {
    public Vector3 Translation = r.ReadVector3();
    public Quaternion Rotation = r.ReadQuaternionWFirst();
    public Vector3 Scale = r.ReadVector3();
}

/// <summary>
/// A list of transforms for each bone in bhkPoseArray.
/// </summary>
public class BonePose(BinaryReader r) {
    public BoneTransform[] Transforms = r.ReadL32FArray(r => new BoneTransform(r));
}

/// <summary>
/// Array of Vectors for Decal placement in BSDecalPlacementVectorExtraData.
/// </summary>
public class DecalVectorArray(BinaryReader r) {
    public short NumVectors = r.ReadInt16();
    public Vector3[] Points = r.ReadVector3();          // Vector XYZ coords
    public Vector3[] Normals = r.ReadVector3();         // Vector Normals
}

/// <summary>
/// Editor flags for the Body Partitions.
/// </summary>
[Flags]
public enum BSPartFlag : ushort {
    PF_EDITOR_VISIBLE = 0,          // Visible in Editor
    PF_START_NET_BONESET = 1 << 8   // Start a new shared boneset.  It is expected this BoneSet and the following sets in the Skin Partition will have the same bones.
}

/// <summary>
/// Body part list for DismemberSkinInstance
/// </summary>
public class BodyPartList(BinaryReader r) {
    public BSPartFlag PartFlag = (BSPartFlag)r.ReadUInt16(); // Flags related to the Body Partition
    public BSDismemberBodyPartType BodyPart = (BSDismemberBodyPartType)r.ReadUInt16(); // Body Part Index
}

/// <summary>
/// Stores Bone Level of Detail info in a BSBoneLODExtraData
/// </summary>
public class BoneLOD(BinaryReader r) {
    public uint Distance = r.ReadUInt32();
    public string BoneName = Y.String(r);
}

/// <summary>
/// Per-chunk material, used in bhkCompressedMeshShapeData
/// </summary>
public class bhkCMSDMaterial(BinaryReader r) {
    public SkyrimHavokMaterial Material = (SkyrimHavokMaterial)r.ReadUInt32();
    public HavokFilter Filter = new HavokFilter(r, h);
}

/// <summary>
/// Triangle indices used in pair with "Big Verts" in a bhkCompressedMeshShapeData.
/// </summary>
public class bhkCMSDBigTris(BinaryReader r) {
    public ushort Triangle1 = r.ReadUInt16();
    public ushort Triangle2 = r.ReadUInt16();
    public ushort Triangle3 = r.ReadUInt16();
    public uint Material = r.ReadUInt32();              // Always 0?
    public ushort WeldingInfo = r.ReadUInt16();
}

/// <summary>
/// A set of transformation data: translation and rotation
/// </summary>
public class bhkCMSDTransform(BinaryReader r) {
    public Vector4 Translation = r.ReadVector4();       // A vector that moves the chunk by the specified amount. W is not used.
    public Quaternion Rotation = r.ReadQuaternionWFirst(); // Rotation. Reference point for rotation is bhkRigidBody translation.
}

/// <summary>
/// Defines subshape chunks in bhkCompressedMeshShapeData
/// </summary>
public class bhkCMSDChunk(BinaryReader r) {
    public Vector4 Translation = r.ReadVector4();
    public uint MaterialIndex = r.ReadUInt32();         // Index of material in bhkCompressedMeshShapeData::Chunk Materials
    public ushort Reference = r.ReadUInt16();           // Always 65535?
    public ushort TransformIndex = r.ReadUInt16();      // Index of transformation in bhkCompressedMeshShapeData::Chunk Transforms
    public ushort[] Vertices = r.ReadL32FArray(r => r.ReadUInt16());
    public ushort[] Indices = r.ReadL32FArray(r => r.ReadUInt16());
    public ushort[] Strips = r.ReadL32FArray(r => r.ReadUInt16());
    public ushort[] WeldingInfo = r.ReadL32FArray(r => r.ReadUInt16());
}

public class MalleableDescriptor(BinaryReader r, Header h) {
    public hkConstraintType Type = (hkConstraintType)r.ReadUInt32(); // Type of constraint.
    public uint NumEntities = r.ReadUInt32();           // Always 2 (Hardcoded). Number of bodies affected by this constraint.
    public int? EntityA = X<bhkEntity>.Ptr(r);          // Usually NONE. The entity affected by this constraint.
    public int? EntityB = X<bhkEntity>.Ptr(r);          // Usually NONE. The entity affected by this constraint.
    public uint Priority = r.ReadUInt32();              // Usually 1. Higher values indicate higher priority of this constraint?
    public BallAndSocketDescriptor BallandSocket = Type == 0 ? new BallAndSocketDescriptor(r) : default;
    public HingeDescriptor Hinge = Type == 1 ? new HingeDescriptor(r, h) : default;
    public LimitedHingeDescriptor LimitedHinge = Type == 2 ? new LimitedHingeDescriptor(r, h) : default;
    public PrismaticDescriptor Prismatic = Type == 6 ? new PrismaticDescriptor(r, h) : default;
    public RagdollDescriptor Ragdoll = Type == 7 ? new RagdollDescriptor(r, h) : default;
    public StiffSpringDescriptor StiffSpring = Type == 8 ? new StiffSpringDescriptor(r) : default;
    public float Tau = h.V <= 0x14000005 ? r.ReadSingle() : default;
    public float Damping = h.V <= 0x14000005 ? r.ReadSingle() : default;
    public float Strength = h.V >= 0x14020007 ? r.ReadSingle() : default;
}

public class ConstraintData(BinaryReader r, Header h) {
    public hkConstraintType Type = (hkConstraintType)r.ReadUInt32(); // Type of constraint.
    public uint NumEntities2 = r.ReadUInt32();          // Always 2 (Hardcoded). Number of bodies affected by this constraint.
    public int? EntityA = X<bhkEntity>.Ptr(r);          // Usually NONE. The entity affected by this constraint.
    public int? EntityB = X<bhkEntity>.Ptr(r);          // Usually NONE. The entity affected by this constraint.
    public uint Priority = r.ReadUInt32();              // Usually 1. Higher values indicate higher priority of this constraint?
    public BallAndSocketDescriptor BallandSocket = Type == 0 ? new BallAndSocketDescriptor(r) : default;
    public HingeDescriptor Hinge = Type == 1 ? new HingeDescriptor(r, h) : default;
    public LimitedHingeDescriptor LimitedHinge = Type == 2 ? new LimitedHingeDescriptor(r, h) : default;
    public PrismaticDescriptor Prismatic = Type == 6 ? new PrismaticDescriptor(r, h) : default;
    public RagdollDescriptor Ragdoll = Type == 7 ? new RagdollDescriptor(r, h) : default;
    public StiffSpringDescriptor StiffSpring = Type == 8 ? new StiffSpringDescriptor(r) : default;
    public MalleableDescriptor Malleable = Type == 13 ? new MalleableDescriptor(r, h) : default;
}

#endregion

#region NIF Objects

/// <summary>
/// Abstract object type.
/// </summary>
public abstract class NiObject {
}

/// <summary>
/// Unknown.
/// </summary>
public class Ni3dsAlphaAnimator : NiObject {
    public byte[] Unknown1;                             // Unknown.
    public int? Parent;                                 // The parent?
    public uint Num1;                                   // Unknown.
    public uint Num2;                                   // Unknown.
    public uint[][] Unknown2;                           // Unknown.
}

/// <summary>
/// Unknown. Only found in 2.3 nifs.
/// </summary>
public class Ni3dsAnimationNode : NiObject {
    public string Name;                                 // Name of this object.
    public bool HasData;                                // Unknown.
    public float[] UnknownFloats1;                      // Unknown. Matrix?
    public ushort UnknownShort;                         // Unknown.
    public int? Child;                                  // Child?
    public float[] UnknownFloats2;                      // Unknown.
    public uint Count;                                  // A count.
    public byte[][] UnknownArray;                       // Unknown.
}

/// <summary>
/// Unknown!
/// </summary>
public class Ni3dsColorAnimator : NiObject {
    public byte[] Unknown1;                             // Unknown.
}

/// <summary>
/// Unknown!
/// </summary>
public class Ni3dsMorphShape : NiObject {
    public byte[] Unknown1;                             // Unknown.
}

/// <summary>
/// Unknown!
/// </summary>
public class Ni3dsParticleSystem : NiObject {
    public byte[] Unknown1;                             // Unknown.
}

/// <summary>
/// Unknown!
/// </summary>
public class Ni3dsPathController : NiObject {
    public byte[] Unknown1;                             // Unknown.
}

/// <summary>
/// LEGACY (pre-10.1). Abstract base class for particle system modifiers.
/// </summary>
public abstract class NiParticleModifier : NiObject {
    public int? NextModifier;                           // Next particle modifier.
    public int? Controller;                             // Points to the particle system controller parent.
}

/// <summary>
/// Particle system collider.
/// </summary>
public abstract class NiPSysCollider : NiObject {
    public float Bounce;                                // Amount of bounce for the collider.
    public bool SpawnonCollide;                         // Spawn particles on impact?
    public bool DieonCollide;                           // Kill particles on impact?
    public int? SpawnModifier;                          // Spawner to use for the collider.
    public int? Parent;                                 // Link to parent.
    public int? NextCollider;                           // The next collider.
    public int? ColliderObject;                         // The object whose position and orientation are the basis of the collider.
}

public enum BroadPhaseType : byte {
    BROAD_PHASE_INVALID = 0,
    BROAD_PHASE_ENTITY = 1,
    BROAD_PHASE_PHANTOM = 2,
    BROAD_PHASE_BORDER = 3
}

public class hkWorldObjCinfoProperty(BinaryReader r) {
    public uint Data = r.ReadUInt32();
    public uint Size = r.ReadUInt32();
    public uint CapacityandFlags = r.ReadUInt32();
}

/// <summary>
/// The base type of most Bethesda-specific Havok-related NIF objects.
/// </summary>
public abstract class bhkRefObject : NiObject {
}

/// <summary>
/// Havok objects that can be saved and loaded from disk?
/// </summary>
public abstract class bhkSerializable : bhkRefObject {
}

/// <summary>
/// Havok objects that have a position in the world?
/// </summary>
public abstract class bhkWorldObject : bhkSerializable {
    public int? Shape;                                  // Link to the body for this collision object.
    public uint UnknownInt;
    public HavokFilter HavokFilter;
    public byte[] Unused;                               // Garbage data from memory.
    public BroadPhaseType BroadPhaseType;
    public byte[] UnusedBytes;
    public hkWorldObjCinfoProperty CinfoProperty;
}

/// <summary>
/// Havok object that do not react with other objects when they collide (causing deflection, etc.) but still trigger collision notifications to the game.  Possible uses are traps, portals, AI fields, etc.
/// </summary>
public abstract class bhkPhantom : bhkWorldObject {
}

/// <summary>
/// A Havok phantom that uses a Havok shape object for its collision volume instead of just a bounding box.
/// </summary>
public abstract class bhkShapePhantom : bhkPhantom {
}

/// <summary>
/// Unknown shape.
/// </summary>
public class bhkSimpleShapePhantom : bhkShapePhantom {
    public byte[] Unused2;                              // Garbage data from memory.
    public Matrix4x4 Transform;
}

/// <summary>
/// A havok node, describes physical properties.
/// </summary>
public abstract class bhkEntity : bhkWorldObject {
}

/// <summary>
/// This is the default body type for all "normal" usable and static world objects. The "T" suffix
/// marks this body as active for translation and rotation, a normal bhkRigidBody ignores those
/// properties. Because the properties are equal, a bhkRigidBody may be renamed into a bhkRigidBodyT and vice-versa.
/// </summary>
public class bhkRigidBody : bhkEntity {
    public hkResponseType CollisionResponse;            // How the body reacts to collisions. See hkResponseType for hkpWorld default implementations.
    public byte UnusedByte1;                            // Skipped over when writing Collision Response and Callback Delay.
    public ushort ProcessContactCallbackDelay;          // Lowers the frequency for processContactCallbacks. A value of 5 means that a callback is raised every 5th frame. The default is once every 65535 frames.
    public uint UnknownInt1;                            // Unknown.
    public HavokFilter HavokFilterCopy;                 // Copy of Havok Filter
    public byte[] Unused2;                              // Garbage data from memory. Matches previous Unused value.
    public uint UnknownInt2;
    public hkResponseType CollisionResponse2;
    public byte UnusedByte2;                            // Skipped over when writing Collision Response and Callback Delay.
    public ushort ProcessContactCallbackDelay2;
    public Vector4 Translation;                         // A vector that moves the body by the specified amount. Only enabled in bhkRigidBodyT objects.
    public Quaternion Rotation;                         // The rotation Yaw/Pitch/Roll to apply to the body. Only enabled in bhkRigidBodyT objects.
    public Vector4 LinearVelocity;                      // Linear velocity.
    public Vector4 AngularVelocity;                     // Angular velocity.
    public Matrix3x4 InertiaTensor;                     // Defines how the mass is distributed among the body, i.e. how difficult it is to rotate around any given axis.
    public Vector4 Center;                              // The body's center of mass.
    public float Mass;                                  // The body's mass in kg. A mass of zero represents an immovable object.
    public float LinearDamping;                         // Reduces the movement of the body over time. A value of 0.1 will remove 10% of the linear velocity every second.
    public float AngularDamping;                        // Reduces the movement of the body over time. A value of 0.05 will remove 5% of the angular velocity every second.
    public float TimeFactor;
    public float GravityFactor;
    public float Friction;                              // How smooth its surfaces is and how easily it will slide along other bodies.
    public float RollingFrictionMultiplier;
    public float Restitution;                           // How "bouncy" the body is, i.e. how much energy it has after colliding. Less than 1.0 loses energy, greater than 1.0 gains energy.
                                                        //     If the restitution is not 0.0 the object will need extra CPU for all new collisions.
    public float MaxLinearVelocity;                     // Maximal linear velocity.
    public float MaxAngularVelocity;                    // Maximal angular velocity.
    public float PenetrationDepth;                      // The maximum allowed penetration for this object.
                                                        //     This is a hint to the engine to see how much CPU the engine should invest to keep this object from penetrating.
                                                        //     A good choice is 5% - 20% of the smallest diameter of the object.
    public hkMotionType MotionSystem;                   // Motion system? Overrides Quality when on Keyframed?
    public hkDeactivatorType DeactivatorType;           // The initial deactivator type of the body.
    public bool EnableDeactivation;
    public hkSolverDeactivation SolverDeactivation;     // How aggressively the engine will try to zero the velocity for slow objects. This does not save CPU.
    public hkQualityType QualityType;                   // The type of interaction with other objects.
    public float UnknownFloat1;
    public byte[] UnknownBytes1;                        // Unknown.
    public byte[] UnknownBytes2;                        // Unknown. Skyrim only.
    public int?[] Constraints;
    public ushort BodyFlags;                            // 1 = respond to wind
}

/// <summary>
/// The "T" suffix marks this body as active for translation and rotation.
/// </summary>
public class bhkRigidBodyT : bhkRigidBody {
}

/// <summary>
/// Describes a physical constraint.
/// </summary>
public abstract class bhkConstraint : bhkSerializable {
    public int?[] Entities;                             // The entities affected by this constraint.
    public uint Priority;                               // Usually 1. Higher values indicate higher priority of this constraint?
}

/// <summary>
/// Hinge constraint.
/// </summary>
public class bhkLimitedHingeConstraint : bhkConstraint {
    public LimitedHingeDescriptor LimitedHinge;         // Describes a limited hinge constraint
}

/// <summary>
/// A malleable constraint.
/// </summary>
public class bhkMalleableConstraint : bhkConstraint {
    public MalleableDescriptor Malleable;               // Constraint within constraint.
}

/// <summary>
/// A spring constraint.
/// </summary>
public class bhkStiffSpringConstraint : bhkConstraint {
    public StiffSpringDescriptor StiffSpring;           // Stiff Spring constraint.
}

/// <summary>
/// Ragdoll constraint.
/// </summary>
public class bhkRagdollConstraint : bhkConstraint {
    public RagdollDescriptor Ragdoll;                   // Ragdoll constraint.
}

/// <summary>
/// A prismatic constraint.
/// </summary>
public class bhkPrismaticConstraint : bhkConstraint {
    public PrismaticDescriptor Prismatic;               // Describes a prismatic constraint
}

/// <summary>
/// A hinge constraint.
/// </summary>
public class bhkHingeConstraint : bhkConstraint {
    public HingeDescriptor Hinge;                       // Hinge constraing.
}

/// <summary>
/// A Ball and Socket Constraint.
/// </summary>
public class bhkBallAndSocketConstraint : bhkConstraint {
    public BallAndSocketDescriptor BallandSocket;       // Describes a ball and socket constraint
}

/// <summary>
/// Two Vector4 for pivot in A and B.
/// </summary>
public class ConstraintInfo(BinaryReader r) {
    public Vector4 PivotInA = r.ReadVector4();
    public Vector4 PivotInB = r.ReadVector4();
}

/// <summary>
/// A Ball and Socket Constraint chain.
/// </summary>
public class bhkBallSocketConstraintChain : bhkSerializable {
    public uint NumPivots;                              // Number of pivot points. Divide by 2 to get the number of constraints.
    public ConstraintInfo[] Pivots;                     // Two pivot points A and B for each constraint.
    public float Tau;                                   // High values are harder and more reactive, lower values are smoother.
    public float Damping;                               // Defines damping strength for the current velocity.
    public float ConstraintForceMixing;                 // Restitution (amount of elasticity) of constraints. Added to the diagonal of the constraint matrix. A value of 0.0 can result in a division by zero with some chain configurations.
    public float MaxErrorDistance;                      // Maximum distance error in constraints allowed before stabilization algorithm kicks in. A smaller distance causes more resistance.
    public int?[] EntitiesA;
    public uint NumEntities;                            // Hardcoded to 2. Don't change.
    public int? EntityA;
    public int? EntityB;
    public uint Priority;
}

/// <summary>
/// A Havok Shape?
/// </summary>
public abstract class bhkShape : bhkSerializable {
}

/// <summary>
/// Transforms a shape.
/// </summary>
public class bhkTransformShape : bhkShape {
    public int? Shape;                                  // The shape that this object transforms.
    public HavokMaterial Material;                      // The material of the shape.
    public float Radius;
    public byte[] Unused;                               // Garbage data from memory.
    public Matrix4x4 Transform;                         // A transform matrix.
}

/// <summary>
/// A havok shape, perhaps with a bounding sphere for quick rejection in addition to more detailed shape data?
/// </summary>
public abstract class bhkSphereRepShape : bhkShape {
    public HavokMaterial Material;                      // The material of the shape.
    public float Radius;                                // The radius of the sphere that encloses the shape.
}

/// <summary>
/// A havok shape.
/// </summary>
public abstract class bhkConvexShape : bhkSphereRepShape {
}

/// <summary>
/// A sphere.
/// </summary>
public class bhkSphereShape : bhkConvexShape {
}

/// <summary>
/// A capsule.
/// </summary>
public class bhkCapsuleShape : bhkConvexShape {
    public byte[] Unused;                               // Not used. The following wants to be aligned at 16 bytes.
    public Vector3 FirstPoint;                          // First point on the capsule's axis.
    public float Radius1;                               // Matches first capsule radius.
    public Vector3 SecondPoint;                         // Second point on the capsule's axis.
    public float Radius2;                               // Matches second capsule radius.
}

/// <summary>
/// A box.
/// </summary>
public class bhkBoxShape : bhkConvexShape {
    public byte[] Unused;                               // Not used. The following wants to be aligned at 16 bytes.
    public Vector3 Dimensions;                          // A cube stored in Half Extents. A unit cube (1.0, 1.0, 1.0) would be stored as 0.5, 0.5, 0.5.
    public float UnusedFloat;                           // Unused as Havok stores the Half Extents as hkVector4 with the W component unused.
}

/// <summary>
/// A convex shape built from vertices. Note that if the shape is used in
/// a non-static object (such as clutter), then they will simply fall
/// through ground when they are under a bhkListShape.
/// </summary>
public class bhkConvexVerticesShape : bhkConvexShape {
    public hkWorldObjCinfoProperty VerticesProperty;
    public hkWorldObjCinfoProperty NormalsProperty;
    public Vector4[] Vertices;                          // Vertices. Fourth component is 0. Lexicographically sorted.
    public Vector4[] Normals;                           // Half spaces as determined by the set of vertices above. First three components define the normal pointing to the exterior, fourth component is the signed distance of the separating plane to the origin: it is minus the dot product of v and n, where v is any vertex on the separating plane, and n is the normal. Lexicographically sorted.
}

/// <summary>
/// A convex transformed shape?
/// </summary>
public class bhkConvexTransformShape : bhkTransformShape {
}

public class bhkConvexSweepShape : bhkShape {
    public int? Shape;
    public HavokMaterial Material;
    public float Radius;
    public Vector3 Unknown;
}

/// <summary>
/// Unknown.
/// </summary>
public class bhkMultiSphereShape : bhkSphereRepShape {
    public float UnknownFloat1;                         // Unknown.
    public float UnknownFloat2;                         // Unknown.
    public NiBound[] Spheres;                           // This array holds the spheres which make up the multi sphere shape.
}

/// <summary>
/// A tree-like Havok data structure stored in an assembly-like binary code?
/// </summary>
public abstract class bhkBvTreeShape : bhkShape {
}

/// <summary>
/// Memory optimized partial polytope bounding volume tree shape (not an entity).
/// </summary>
public class bhkMoppBvTreeShape : bhkBvTreeShape {
    public int? Shape;                                  // The shape.
    public uint[] Unused;                               // Garbage data from memory. Referred to as User Data, Shape Collection, and Code.
    public float ShapeScale;                            // Scale.
    public uint MOPPDataSize;                           // Number of bytes for MOPP data.
    public Vector3 Origin;                              // Origin of the object in mopp coordinates. This is the minimum of all vertices in the packed shape along each axis, minus 0.1.
    public float Scale;                                 // The scaling factor to quantize the MOPP: the quantization factor is equal to 256*256 divided by this number. In Oblivion files, scale is taken equal to 256*256*254 / (size + 0.2) where size is the largest dimension of the bounding box of the packed shape.
    public MoppDataBuildType BuildType;                 // Tells if MOPP Data was organized into smaller chunks (PS3) or not (PC)
    public byte[] MOPPData;                             // The tree of bounding volume data.
}

/// <summary>
/// Havok collision object that uses multiple shapes?
/// </summary>
public abstract class bhkShapeCollection : bhkShape {
}

/// <summary>
/// A list of shapes.
/// 
/// Do not put a bhkPackedNiTriStripsShape in the Sub Shapes. Use a
/// separate collision nodes without a list shape for those.
/// 
/// Also, shapes collected in a bhkListShape may not have the correct
/// walking noise, so only use it for non-walkable objects.
/// </summary>
public class bhkListShape : bhkShapeCollection {
    public int?[] SubShapes;                            // List of shapes.
    public HavokMaterial Material;                      // The material of the shape.
    public hkWorldObjCinfoProperty ChildShapeProperty;
    public hkWorldObjCinfoProperty ChildFilterProperty;
    public uint[] UnknownInts;                          // Unknown.
}

public class bhkMeshShape : bhkShape {
    public uint[] Unknowns;
    public float Radius;
    public byte[] Unused2;
    public Vector4 Scale;
    public hkWorldObjCinfoProperty[] ShapeProperties;
    public int[] Unknown2;
    public int?[] StripsData;                           // Refers to a bunch of NiTriStripsData objects that make up this shape.
}

/// <summary>
/// A shape constructed from strips data.
/// </summary>
public class bhkPackedNiTriStripsShape : bhkShapeCollection {
    public OblivionSubShape[] SubShapes;
    public uint UserData;
    public uint Unused1;                                // Looks like a memory pointer and may be garbage.
    public float Radius;
    public uint Unused2;                                // Looks like a memory pointer and may be garbage.
    public Vector4 Scale;
    public float RadiusCopy;                            // Same as radius
    public Vector4 ScaleCopy;                           // Same as scale.
    public int? Data;
}

/// <summary>
/// A shape constructed from a bunch of strips.
/// </summary>
public class bhkNiTriStripsShape : bhkShapeCollection {
    public HavokMaterial Material;                      // The material of the shape.
    public float Radius;
    public uint[] Unused;                               // Garbage data from memory though the last 3 are referred to as maxSize, size, and eSize.
    public uint GrowBy;
    public Vector4 Scale;                               // Scale. Usually (1.0, 1.0, 1.0, 0.0).
    public int?[] StripsData;                           // Refers to a bunch of NiTriStripsData objects that make up this shape.
    public HavokFilter[] DataLayers;                    // Havok Layers for each strip data.
}

/// <summary>
/// A generic extra data object.
/// </summary>
public class NiExtraData : NiObject {
    public string Name;                                 // Name of this object.
    public int? NextExtraData;                          // Block number of the next extra data object.
}

/// <summary>
/// Abstract base class for all interpolators of bool, float, NiQuaternion, NiPoint3, NiColorA, and NiQuatTransform data.
/// </summary>
public abstract class NiInterpolator : NiObject {
}

/// <summary>
/// Abstract base class for interpolators that use NiAnimationKeys (Key, KeyGrp) for interpolation.
/// </summary>
public abstract class NiKeyBasedInterpolator : NiInterpolator {
}

/// <summary>
/// Uses NiFloatKeys to animate a float value over time.
/// </summary>
public class NiFloatInterpolator : NiKeyBasedInterpolator {
    public float Value;                                 // Pose value if lacking NiFloatData.
    public int? Data;
}

/// <summary>
/// An interpolator for transform keyframes.
/// </summary>
public class NiTransformInterpolator : NiKeyBasedInterpolator {
    public NiQuatTransform Transform;
    public int? Data;
}

/// <summary>
/// Uses NiPosKeys to animate an NiPoint3 value over time.
/// </summary>
public class NiPoint3Interpolator : NiKeyBasedInterpolator {
    public Vector3 Value;                               // Pose value if lacking NiPosData.
    public int? Data;
}

[Flags]
public enum PathFlags : ushort {
    CVDataNeedsUpdate = 0,
    CurveTypeOpen = 1 << 1,
    AllowFlip = 1 << 2,
    Bank = 1 << 3,
    ConstantVelocity = 1 << 4,
    Follow = 1 << 5,
    Flip = 1 << 6
}

/// <summary>
/// Used to make an object follow a predefined spline path.
/// </summary>
public class NiPathInterpolator : NiKeyBasedInterpolator {
    public PathFlags Flags;
    public int BankDir;                                 // -1 = Negative, 1 = Positive
    public float MaxBankAngle;                          // Max angle in radians.
    public float Smoothing;
    public short FollowAxis;                            // 0, 1, or 2 representing X, Y, or Z.
    public int? PathData;
    public int? PercentData;
}

/// <summary>
/// Uses NiBoolKeys to animate a bool value over time.
/// </summary>
public class NiBoolInterpolator : NiKeyBasedInterpolator {
    public bool Value;                                  // Pose value if lacking NiBoolData.
    public int? Data;
}

/// <summary>
/// Uses NiBoolKeys to animate a bool value over time.
/// Unlike NiBoolInterpolator, it ensures that keys have not been missed between two updates.
/// </summary>
public class NiBoolTimelineInterpolator : NiBoolInterpolator {
}

public enum InterpBlendFlags : byte {
    MANAGER_CONTROLLED = 1          // MANAGER_CONTROLLED
}

/// <summary>
/// Interpolator item for array in NiBlendInterpolator.
/// </summary>
public class InterpBlendItem(BinaryReader r, Header h) {
    public int? Interpolator = X<NiInterpolator>.Ref(r); // Reference to an interpolator.
    public float Weight = r.ReadSingle();
    public float NormalizedWeight = r.ReadSingle();
    public byte Priority = h.V >= 0x0A01006E ? r.ReadByte() : default;
    public float EaseSpinner = r.ReadSingle();
}

/// <summary>
/// Abstract base class for all NiInterpolators that blend the results of sub-interpolators together to compute a final weighted value.
/// </summary>
public abstract class NiBlendInterpolator : NiInterpolator {
    public InterpBlendFlags Flags;
    public byte ArraySize;
    public ushort ArrayGrowBy;
    public float WeightThreshold;
    public byte InterpCount;
    public byte SingleIndex;
    public byte HighPriority;
    public byte NextHighPriority;
    public float SingleTime;
    public float HighWeightsSum;
    public float NextHighWeightsSum;
    public float HighEaseSpinner;
    public InterpBlendItem[] InterpArrayItems;
    public bool ManagedControlled;
    public bool OnlyUseHighestWeight;
    public int? SingleInterpolator;
}

/// <summary>
/// Abstract base class for interpolators storing data via a B-spline.
/// </summary>
public abstract class NiBSplineInterpolator : NiInterpolator {
    public float StartTime;                             // Animation start time.
    public float StopTime;                              // Animation stop time.
    public int? SplineData;
    public int? BasisData;
}

/// <summary>
/// Abstract base class for NiObjects that support names, extra data, and time controllers.
/// </summary>
public abstract class NiObjectNET : NiObject {
    public BSLightingShaderPropertyShaderType SkyrimShaderType; // Configures the main shader path
    public string Name;                                 // Name of this controllable object, used to refer to the object in .kf files.
    public bool HasOldExtraData;                        // Extra data for pre-3.0 versions.
    public string OldExtraPropName;                     // (=NiStringExtraData)
    public uint OldExtraInternalId;                     // ref
    public string OldExtraString;                       // Extra string data.
    public byte UnknownByte;                            // Always 0.
    public int? ExtraData;                              // Extra data object index. (The first in a chain)
    public int?[] ExtraDataList;                        // List of extra data indices.
    public int? Controller;                             // Controller object index. (The first in a chain)
}

/// <summary>
/// This is the most common collision object found in NIF files. It acts as a real object that
/// is visible and possibly (if the body allows for it) interactive. The node itself
/// is simple, it only has three properties.
/// For this type of collision object, bhkRigidBody or bhkRigidBodyT is generally used.
/// </summary>
public class NiCollisionObject : NiObject {
    public int? Target;                                 // Index of the AV object referring to this collision object.
}

/// <summary>
/// Collision box.
/// </summary>
public class NiCollisionData : NiCollisionObject {
    public PropagationMode PropagationMode;
    public CollisionMode CollisionMode;
    public byte UseABV;                                 // Use Alternate Bounding Volume.
    public BoundingVolume BoundingVolume;
}

/// <summary>
/// bhkNiCollisionObject flags. The flags 0x2, 0x100, and 0x200 are not seen in any NIF nor get/set by the engine.
/// </summary>
[Flags]
public enum bhkCOFlags : ushort {
    ACTIVE = 0,
    NOTIFY = 1 << 2,
    SET_LOCAL = 1 << 3,
    DBG_DISPLAY = 1 << 4,
    USE_VEL = 1 << 5,
    RESET = 1 << 6,
    SYNC_ON_UPDATE = 1 << 7,
    ANIM_TARGETED = 1 << 10,
    DISMEMBERED_LIMB = 1 << 11
}

/// <summary>
/// Havok related collision object?
/// </summary>
public abstract class bhkNiCollisionObject : NiCollisionObject {
    public bhkCOFlags Flags;                            // Set to 1 for most objects, and to 41 for animated objects (ANIM_STATIC). Bits: 0=Active 2=Notify 3=Set Local 6=Reset.
    public int? Body;
}

/// <summary>
/// Havok related collision object?
/// </summary>
public class bhkCollisionObject : bhkNiCollisionObject {
}

/// <summary>
/// Unknown.
/// </summary>
public class bhkBlendCollisionObject : bhkCollisionObject {
    public float HeirGain;
    public float VelGain;
    public float UnkFloat1;
    public float UnkFloat2;
}

/// <summary>
/// Unknown.
/// </summary>
public class bhkPCollisionObject : bhkNiCollisionObject {
}

/// <summary>
/// Unknown.
/// </summary>
public class bhkSPCollisionObject : bhkPCollisionObject {
}

/// <summary>
/// Abstract audio-visual base class from which all of Gamebryo's scene graph objects inherit.
/// </summary>
public abstract class NiAVObject : NiObjectNET {
    public Flags Flags;                                 // Basic flags for AV objects; commonly 0x000C or 0x000A.
    public Vector3 Translation;                         // The translation vector.
    public Matrix3x3 Rotation;                          // The rotation part of the transformation matrix.
    public float Scale;                                 // Scaling part (only uniform scaling is supported).
    public Vector3 Velocity;                            // Unknown function. Always seems to be (0, 0, 0)
    public int?[] Properties;                           // All rendering properties attached to this object.
    public uint[] Unknown1;                             // Always 2,0,2,0.
    public byte Unknown2;                               // 0 or 1.
    public bool HasBoundingVolume;
    public BoundingVolume BoundingVolume;
    public int? CollisionObject;
}

/// <summary>
/// Abstract base class for dynamic effects such as NiLights or projected texture effects.
/// </summary>
public abstract class NiDynamicEffect : NiAVObject {
    public bool SwitchState;                            // If true, then the dynamic effect is applied to affected nodes during rendering.
    public uint NumAffectedNodes;
    public int?[] AffectedNodes;                        // If a node appears in this list, then its entire subtree will be affected by the effect.
    public uint[] AffectedNodePointers;                 // As of 4.0 the pointer hash is no longer stored alongside each NiObject on disk, yet this node list still refers to the pointer hashes. Cannot leave the type as Ptr because the link will be invalid.
}

/// <summary>
/// Abstract base class that represents light sources in a scene graph.
/// For Bethesda Stream 130 (FO4), NiLight now directly inherits from NiAVObject.
/// </summary>
public abstract class NiLight : NiDynamicEffect {
    public float Dimmer;                                // Scales the overall brightness of all light components.
    public Color3 AmbientColor;
    public Color3 DiffuseColor;
    public Color3 SpecularColor;
}

/// <summary>
/// Abstract base class representing all rendering properties. Subclasses are attached to NiAVObjects to control their rendering.
/// </summary>
public abstract class NiProperty : NiObjectNET {
}

/// <summary>
/// Unknown
/// </summary>
public class NiTransparentProperty : NiProperty {
    public byte[] Unknown;                              // Unknown.
}

/// <summary>
/// Abstract base class for all particle system modifiers.
/// </summary>
public abstract class NiPSysModifier : NiObject {
    public string Name;                                 // Used to locate the modifier.
    public uint Order;                                  // Modifier ID in the particle modifier chain (always a multiple of 1000)?
    public int? Target;                                 // NiParticleSystem parent of this modifier.
    public bool Active;                                 // Whether or not the modifier is active.
}

/// <summary>
/// Abstract base class for all particle system emitters.
/// </summary>
public abstract class NiPSysEmitter : NiPSysModifier {
    public float Speed;                                 // Speed / Inertia of particle movement.
    public float SpeedVariation;                        // Adds an amount of randomness to Speed.
    public float Declination;                           // Declination / First axis.
    public float DeclinationVariation;                  // Declination randomness / First axis.
    public float PlanarAngle;                           // Planar Angle / Second axis.
    public float PlanarAngleVariation;                  // Planar Angle randomness / Second axis .
    public Color4 InitialColor;                         // Defines color of a birthed particle.
    public float InitialRadius;                         // Size of a birthed particle.
    public float RadiusVariation;                       // Particle Radius randomness.
    public float LifeSpan;                              // Duration until a particle dies.
    public float LifeSpanVariation;                     // Adds randomness to Life Span.
}

/// <summary>
/// Abstract base class for particle emitters that emit particles from a volume.
/// </summary>
public abstract class NiPSysVolumeEmitter : NiPSysEmitter {
    public int? EmitterObject;                          // Node parent of this modifier?
}

/// <summary>
/// Abstract base class that provides the base timing and update functionality for all the Gamebryo animation controllers.
/// </summary>
public abstract class NiTimeController : NiObject {
    public int? NextController;                         // Index of the next controller.
    public Flags Flags;                                 // Controller flags.
                                                        //     Bit 0 : Anim type, 0=APP_TIME 1=APP_INIT
                                                        //     Bit 1-2 : Cycle type, 00=Loop 01=Reverse 10=Clamp
                                                        //     Bit 3 : Active
                                                        //     Bit 4 : Play backwards
                                                        //     Bit 5 : Is manager controlled
                                                        //     Bit 6 : Always seems to be set in Skyrim and Fallout NIFs, unknown function
    public float Frequency;                             // Frequency (is usually 1.0).
    public float Phase;                                 // Phase (usually 0.0).
    public float StartTime;                             // Controller start time.
    public float StopTime;                              // Controller stop time.
    public int? Target;                                 // Controller target (object index of the first controllable ancestor of this object).
    public uint UnknownInteger;                         // Unknown integer.
}

/// <summary>
/// Abstract base class for all NiTimeController objects using NiInterpolator objects to animate their target objects.
/// </summary>
public abstract class NiInterpController : NiTimeController {
    public bool ManagerControlled;
}

/// <summary>
/// DEPRECATED (20.6)
/// </summary>
public class NiMultiTargetTransformController : NiInterpController {
    public int?[] ExtraTargets;                         // NiNode Targets to be controlled.
}

/// <summary>
/// DEPRECATED (20.5), replaced by NiMorphMeshModifier.
/// Time controller for geometry morphing.
/// </summary>
public class NiGeomMorpherController : NiInterpController {
    public Flags ExtraFlags;                            // 1 = UPDATE NORMALS
    public int? Data;                                   // Geometry morphing data index.
    public byte AlwaysUpdate;
    public uint NumInterpolators;
    public int?[] Interpolators;
    public MorphWeight[] InterpolatorWeights;
    public uint[] UnknownInts;                          // Unknown.
}

/// <summary>
/// Unknown! Used by Daoc->'healing.nif'.
/// </summary>
public class NiMorphController : NiInterpController {
}

/// <summary>
/// Unknown! Used by Daoc.
/// </summary>
public class NiMorpherController : NiInterpController {
    public int? Data;                                   // This controller's data.
}

/// <summary>
/// Uses a single NiInterpolator to animate its target value.
/// </summary>
public abstract class NiSingleInterpController : NiInterpController {
    public int? Interpolator;
}

/// <summary>
/// DEPRECATED (10.2), RENAMED (10.2) to NiTransformController
/// A time controller object for animation key frames.
/// </summary>
public class NiKeyframeController : NiSingleInterpController {
    public int? Data;
}

/// <summary>
/// NiTransformController replaces NiKeyframeController.
/// </summary>
public class NiTransformController : NiKeyframeController {
}

/// <summary>
/// A particle system modifier controller.
/// NiInterpController::GetCtlrID() string format:
///     '%s'
/// Where %s = Value of "Modifier Name"
/// </summary>
public abstract class NiPSysModifierCtlr : NiSingleInterpController {
    public string ModifierName;                         // Used to find the modifier pointer.
}

/// <summary>
/// Particle system emitter controller.
/// NiInterpController::GetInterpolatorID() string format:
///     ['BirthRate', 'EmitterActive'] (for "Interpolator" and "Visibility Interpolator" respectively)
/// </summary>
public class NiPSysEmitterCtlr : NiPSysModifierCtlr {
    public int? VisibilityInterpolator;
    public int? Data;
}

/// <summary>
/// A particle system modifier controller that animates a boolean value for particles.
/// </summary>
public abstract class NiPSysModifierBoolCtlr : NiPSysModifierCtlr {
}

/// <summary>
/// A particle system modifier controller that animates active/inactive state for particles.
/// </summary>
public class NiPSysModifierActiveCtlr : NiPSysModifierBoolCtlr {
    public int? Data;
}

/// <summary>
/// A particle system modifier controller that animates a floating point value for particles.
/// </summary>
public abstract class NiPSysModifierFloatCtlr : NiPSysModifierCtlr {
    public int? Data;
}

/// <summary>
/// Animates the declination value on an NiPSysEmitter object.
/// </summary>
public class NiPSysEmitterDeclinationCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Animates the declination variation value on an NiPSysEmitter object.
/// </summary>
public class NiPSysEmitterDeclinationVarCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Animates the size value on an NiPSysEmitter object.
/// </summary>
public class NiPSysEmitterInitialRadiusCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Animates the lifespan value on an NiPSysEmitter object.
/// </summary>
public class NiPSysEmitterLifeSpanCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Animates the speed value on an NiPSysEmitter object.
/// </summary>
public class NiPSysEmitterSpeedCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Animates the strength value of an NiPSysGravityModifier.
/// </summary>
public class NiPSysGravityStrengthCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target float value.
/// </summary>
public abstract class NiFloatInterpController : NiSingleInterpController {
}

/// <summary>
/// Changes the image a Map (TexDesc) will use. Uses a float interpolator to animate the texture index.
/// Often used for performing flipbook animation.
/// </summary>
public class NiFlipController : NiFloatInterpController {
    public TexType TextureSlot;                         // Target texture slot (0=base, 4=glow).
    public float StartTime;
    public float Delta;                                 // Time between two flips.
                                                        //     delta = (start_time - stop_time) / sources.num_indices
    public uint NumSources;
    public int?[] Sources;                              // The texture sources.
    public int?[] Images;                               // The image sources
}

/// <summary>
/// Animates the alpha value of a property using an interpolator.
/// </summary>
public class NiAlphaController : NiFloatInterpController {
    public int? Data;
}

/// <summary>
/// Used to animate a single member of an NiTextureTransform.
/// NiInterpController::GetCtlrID() string formats:
///     ['%1-%2-TT_TRANSLATE_U', '%1-%2-TT_TRANSLATE_V', '%1-%2-TT_ROTATE', '%1-%2-TT_SCALE_U', '%1-%2-TT_SCALE_V']
/// (Depending on "Operation" enumeration, %1 = Value of "Shader Map", %2 = Value of "Texture Slot")
/// </summary>
public class NiTextureTransformController : NiFloatInterpController {
    public bool ShaderMap;                              // Is the target map a shader map?
    public TexType TextureSlot;                         // The target texture slot.
    public TransformMember Operation;                   // Controls which aspect of the texture transform to modify.
    public int? Data;
}

/// <summary>
/// Unknown controller.
/// </summary>
public class NiLightDimmerController : NiFloatInterpController {
}

/// <summary>
/// Abstract base class for all NiInterpControllers that use a NiInterpolator to animate their target boolean value.
/// </summary>
public abstract class NiBoolInterpController : NiSingleInterpController {
}

/// <summary>
/// Animates the visibility of an NiAVObject.
/// </summary>
public class NiVisController : NiBoolInterpController {
    public int? Data;
}

/// <summary>
/// Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target NiPoint3 value.
/// </summary>
public abstract class NiPoint3InterpController : NiSingleInterpController {
}

/// <summary>
/// Time controller for material color. Flags are used for color selection in versions below 10.1.0.0.
/// Bits 4-5: Target Color (00 = Ambient, 01 = Diffuse, 10 = Specular, 11 = Emissive)
/// NiInterpController::GetCtlrID() string formats:
///     ['AMB', 'DIFF', 'SPEC', 'SELF_ILLUM'] (Depending on "Target Color")
/// </summary>
public class NiMaterialColorController : NiPoint3InterpController {
    public MaterialColor TargetColor;                   // Selects which color to control.
    public int? Data;
}

/// <summary>
/// Animates the ambient, diffuse and specular colors of an NiLight.
/// NiInterpController::GetCtlrID() string formats:
///     ['Diffuse', 'Ambient'] (Depending on "Target Color")
/// </summary>
public class NiLightColorController : NiPoint3InterpController {
    public LightColor TargetColor;
    public int? Data;
}

/// <summary>
/// Abstract base class for all extra data controllers.
/// NiInterpController::GetCtlrID() string format:
///     '%s'
/// Where %s = Value of "Extra Data Name"
/// </summary>
public abstract class NiExtraDataController : NiSingleInterpController {
    public string ExtraDataName;
}

/// <summary>
/// Animates an NiFloatExtraData object attached to an NiAVObject.
/// NiInterpController::GetCtlrID() string format is same as parent.
/// </summary>
public class NiFloatExtraDataController : NiExtraDataController {
    public byte NumExtraBytes;                          // Number of extra bytes.
    public byte[] UnknownBytes;                         // Unknown.
    public byte[] UnknownExtraBytes;                    // Unknown.
    public int? Data;
}

/// <summary>
/// Animates an NiFloatsExtraData object attached to an NiAVObject.
/// NiInterpController::GetCtlrID() string format:
///     '%s[%d]'
/// Where %s = Value of "Extra Data Name", %d = Value of "Floats Extra Data Index"
/// </summary>
public class NiFloatsExtraDataController : NiExtraDataController {
    public int FloatsExtraDataIndex;
    public int? Data;
}

/// <summary>
/// Animates an NiFloatsExtraData object attached to an NiAVObject.
/// NiInterpController::GetCtlrID() string format:
///     '%s[%d]'
/// Where %s = Value of "Extra Data Name", %d = Value of "Floats Extra Data Index"
/// </summary>
public class NiFloatsExtraDataPoint3Controller : NiExtraDataController {
    public int FloatsExtraDataIndex;
}

/// <summary>
/// DEPRECATED (20.5), Replaced by NiSkinningLODController.
/// Level of detail controller for bones.  Priority is arranged from low to high.
/// </summary>
public class NiBoneLODController : NiTimeController {
    public uint LOD;                                    // Unknown.
    public uint NumLODs;                                // Number of LODs.
    public uint NumNodeGroups;                          // Number of node arrays.
    public ??[] NodeGroups;                             // A list of node sets (each set a sequence of bones).
    public uint NumShapeGroups;                         // Number of shape groups.
    public SkinInfoSet[] ShapeGroups1;                  // List of shape groups.
    public uint NumShapeGroups2;                        // The size of the second list of shape groups.
    public int?[] ShapeGroups2;                         // Group of NiTriShape indices.
    public int UnknownInt2;                             // Unknown.
    public int UnknownInt3;                             // Unknown.
}

/// <summary>
/// A simple LOD controller for bones.
/// </summary>
public class NiBSBoneLODController : NiBoneLODController {
}

public class MaterialData(BinaryReader r, Header h) {
    public bool HasShader = h.V >= 0x0A000100 && h.V <= 0x14010003 ? r.ReadBool32() : default; // Shader.
    public string ShaderName = Has Shader && h.V >= 0x0A000100 && h.V <= 0x14010003 ? Y.String(r) : default; // The shader name.
    public int ShaderExtraData = Has Shader && h.V >= 0x0A000100 && h.V <= 0x14010003 ? r.ReadInt32() : default; // Extra data associated with the shader. A value of -1 means the shader is the default implementation.
    public uint NumMaterials = h.V >= 0x14020005 ? r.ReadUInt32() : default;
    public string[] MaterialName = h.V >= 0x14020005 ? Y.String(r) : default; // The name of the material.
    public int[] MaterialExtraData = h.V >= 0x14020005 ? r.ReadInt32() : default; // Extra data associated with the material. A value of -1 means the material is the default implementation.
    public int ActiveMaterial = h.V >= 0x14020005 ? r.ReadInt32() : default; // The index of the currently active material.
    public byte UnknownByte = h.V >= 0x0A020000 && h.V <= 0x0A020000 && (h.UserVersion == 1) ? r.ReadByte() : default; // Cyanide extension (only in version 10.2.0.0?).
    public int UnknownInteger2 = h.V >= 0x0A040001 && h.V <= 0x0A040001 ? r.ReadInt32() : default; // Unknown.
    public bool MaterialNeedsUpdate = h.V >= 0x14020007 ? r.ReadBool32() : default; // Whether the materials for this object always needs to be updated before rendering with them.
}

/// <summary>
/// Describes a visible scene element with vertices like a mesh, a particle system, lines, etc.
/// </summary>
public abstract class NiGeometry : NiAVObject {
    public NiBound Bound;
    public int? Skin;
    public int? Data;                                   // Data index (NiTriShapeData/NiTriStripData).
    public int? SkinInstance;
    public MaterialData MaterialData;
    public int? ShaderProperty;
    public int? AlphaProperty;
}

/// <summary>
/// Describes a mesh, built from triangles.
/// </summary>
public abstract class NiTriBasedGeom : NiGeometry {
}

[Flags]
public enum VectorFlags : ushort {
    UV_1 = 0,
    UV_2 = 1 << 1,
    UV_4 = 1 << 2,
    UV_8 = 1 << 3,
    UV_16 = 1 << 4,
    UV_32 = 1 << 5,
    Unk64 = 1 << 6,
    Unk128 = 1 << 7,
    Unk256 = 1 << 8,
    Unk512 = 1 << 9,
    Unk1024 = 1 << 10,
    Unk2048 = 1 << 11,
    Has_Tangents = 1 << 12,
    Unk8192 = 1 << 13,
    Unk16384 = 1 << 14,
    Unk32768 = 1 << 15
}

[Flags]
public enum BSVectorFlags : ushort {
    Has_UV = 0,
    Unk2 = 1 << 1,
    Unk4 = 1 << 2,
    Unk8 = 1 << 3,
    Unk16 = 1 << 4,
    Unk32 = 1 << 5,
    Unk64 = 1 << 6,
    Unk128 = 1 << 7,
    Unk256 = 1 << 8,
    Unk512 = 1 << 9,
    Unk1024 = 1 << 10,
    Unk2048 = 1 << 11,
    Has_Tangents = 1 << 12,
    Unk8192 = 1 << 13,
    Unk16384 = 1 << 14,
    Unk32768 = 1 << 15
}

/// <summary>
/// Mesh data: vertices, vertex normals, etc.
/// </summary>
public abstract class NiGeometryData : NiObject {
    public int GroupID;                                 // Always zero.
    public ushort NumVertices;                          // Number of vertices.
    public ushort BSMaxVertices;                        // Bethesda uses this for max number of particles in NiPSysData.
    public byte KeepFlags;                              // Used with NiCollision objects when OBB or TRI is set.
    public byte CompressFlags;                          // Unknown.
    public bool HasVertices;                            // Is the vertex array present? (Always non-zero.)
    public Vector3[] Vertices;                          // The mesh vertices.
    public VectorFlags VectorFlags;
    public BSVectorFlags BSVectorFlags;
    public uint MaterialCRC;
    public bool HasNormals;                             // Do we have lighting normals? These are essential for proper lighting: if not present, the model will only be influenced by ambient light.
    public Vector3<false>[] Normals;                    // The lighting normals.
    public Vector3[] Tangents;                          // Tangent vectors.
    public Vector3[] Bitangents;                        // Bitangent vectors.
    public bool HasUnkFloats;
    public float[] UnkFloats;
    public Vector3 Center;                              // Center of the bounding box (smallest box that contains all vertices) of the mesh.
    public float Radius;                                // Radius of the mesh: maximal Euclidean distance between the center and all vertices.
    public short[] Unknown13shorts;                     // Unknown, always 0?
    public bool HasVertexColors;                        // Do we have vertex colors? These are usually used to fine-tune the lighting of the model.
                                                        // 
                                                        //     Note: how vertex colors influence the model can be controlled by having a NiVertexColorProperty object as a property child of the root node. If this property object is not present, the vertex colors fine-tune lighting.
                                                        // 
                                                        //     Note 2: set to either 0 or 0xFFFFFFFF for NifTexture compatibility.
    public ByteColor4[] VertexColors;                   // The vertex colors.
    public ushort NumUVSets;                            // The lower 6 (or less?) bits of this field represent the number of UV texture sets. The other bits are probably flag bits. For versions 10.1.0.0 and up, if bit 12 is set then extra vectors are present after the normals.
    public bool HasUV;                                  // Do we have UV coordinates?
                                                        // 
                                                        //     Note: for compatibility with NifTexture, set this value to either 0x00000000 or 0xFFFFFFFF.
    public HalfTexCoord[][] UVSets;                     // The UV texture coordinates. They follow the OpenGL standard: some programs may require you to flip the second coordinate.
    public ConsistencyType ConsistencyFlags;            // Consistency Flags
    public int? AdditionalData;                         // Unknown.
}

public abstract class AbstractAdditionalGeometryData : NiObject {
}

/// <summary>
/// Describes a mesh, built from triangles.
/// </summary>
public abstract class NiTriBasedGeomData : NiGeometryData {
    public ushort NumTriangles;                         // Number of triangles.
}

/// <summary>
/// Unknown. Is apparently only used in skeleton.nif files.
/// </summary>
public class bhkBlendController : NiTimeController {
    public uint Keys;                                   // Seems to be always zero.
}

/// <summary>
/// Bethesda-specific collision bounding box for skeletons.
/// </summary>
public class BSBound : NiExtraData {
    public Vector3 Center;                              // Center of the bounding box.
    public Vector3 Dimensions;                          // Dimensions of the bounding box from center.
}

/// <summary>
/// Unknown. Marks furniture sitting positions?
/// </summary>
public class BSFurnitureMarker : NiExtraData {
    public FurniturePosition[] Positions;
}

/// <summary>
/// Particle modifier that adds a blend of object space translation and rotation to particles born in world space.
/// </summary>
public class BSParentVelocityModifier : NiPSysModifier {
    public float Damping;                               // Amount of blending?
}

/// <summary>
/// Particle emitter that uses a node, its children and subchildren to emit from.  Emission will be evenly spread along points from nodes leading to their direct parents/children only.
/// </summary>
public class BSPSysArrayEmitter : NiPSysVolumeEmitter {
}

/// <summary>
/// Particle Modifier that uses the wind value from the gamedata to alter the path of particles.
/// </summary>
public class BSWindModifier : NiPSysModifier {
    public float Strength;                              // The amount of force wind will have on particles.
}

/// <summary>
/// NiTriStripsData for havok data?
/// </summary>
public class hkPackedNiTriStripsData : bhkShapeCollection {
    public TriangleData[] Triangles;
    public uint NumVertices;
    public byte UnknownByte1;                           // Unknown.
    public Vector3[] Vertices;
    public OblivionSubShape[] SubShapes;                // The subparts.
}

/// <summary>
/// Transparency. Flags 0x00ED.
/// </summary>
public class NiAlphaProperty : NiProperty {
    public Flags Flags;                                 // Bit 0 : alpha blending enable
                                                        //     Bits 1-4 : source blend mode
                                                        //     Bits 5-8 : destination blend mode
                                                        //     Bit 9 : alpha test enable
                                                        //     Bit 10-12 : alpha test mode
                                                        //     Bit 13 : no sorter flag ( disables triangle sorting )
                                                        // 
                                                        //     blend modes (glBlendFunc):
                                                        //     0000 GL_ONE
                                                        //     0001 GL_ZERO
                                                        //     0010 GL_SRC_COLOR
                                                        //     0011 GL_ONE_MINUS_SRC_COLOR
                                                        //     0100 GL_DST_COLOR
                                                        //     0101 GL_ONE_MINUS_DST_COLOR
                                                        //     0110 GL_SRC_ALPHA
                                                        //     0111 GL_ONE_MINUS_SRC_ALPHA
                                                        //     1000 GL_DST_ALPHA
                                                        //     1001 GL_ONE_MINUS_DST_ALPHA
                                                        //     1010 GL_SRC_ALPHA_SATURATE
                                                        // 
                                                        //     test modes (glAlphaFunc):
                                                        //     000 GL_ALWAYS
                                                        //     001 GL_LESS
                                                        //     010 GL_EQUAL
                                                        //     011 GL_LEQUAL
                                                        //     100 GL_GREATER
                                                        //     101 GL_NOTEQUAL
                                                        //     110 GL_GEQUAL
                                                        //     111 GL_NEVER
    public byte Threshold;                              // Threshold for alpha testing (see: glAlphaFunc)
    public ushort UnknownShort1;                        // Unknown
    public uint UnknownInt2;                            // Unknown
}

/// <summary>
/// Ambient light source.
/// </summary>
public class NiAmbientLight : NiLight {
}

/// <summary>
/// Generic rotating particles data object.
/// </summary>
public class NiParticlesData : NiGeometryData {
    public ushort NumParticles;                         // The maximum number of particles (matches the number of vertices).
    public float ParticleRadius;                        // The particles' size.
    public bool HasRadii;                               // Is the particle size array present?
    public float[] Radii;                               // The individual particle sizes.
    public ushort NumActive;                            // The number of active particles at the time the system was saved. This is also the number of valid entries in the following arrays.
    public bool HasSizes;                               // Is the particle size array present?
    public float[] Sizes;                               // The individual particle sizes.
    public bool HasRotations;                           // Is the particle rotation array present?
    public Quaternion[] Rotations;                      // The individual particle rotations.
    public bool HasRotationAngles;                      // Are the angles of rotation present?
    public float[] RotationAngles;                      // Angles of rotation
    public bool HasRotationAxes;                        // Are axes of rotation present?
    public Vector3[] RotationAxes;                      // Axes of rotation.
    public bool HasTextureIndices;
    public Vector4[] SubtextureOffsets;                 // Defines UV offsets
    public float AspectRatio;                           // Sets aspect ratio for Subtexture Offset UV quads
    public ushort AspectFlags;
    public float SpeedtoAspectAspect2;
    public float SpeedtoAspectSpeed1;
    public float SpeedtoAspectSpeed2;
}

/// <summary>
/// Rotating particles data object.
/// </summary>
public class NiRotatingParticlesData : NiParticlesData {
    public bool HasRotations2;                          // Is the particle rotation array present?
    public Quaternion[] Rotations2;                     // The individual particle rotations.
}

/// <summary>
/// Particle system data object (with automatic normals?).
/// </summary>
public class NiAutoNormalParticlesData : NiParticlesData {
}

/// <summary>
/// Particle Description.
/// </summary>
public class ParticleDesc(BinaryReader r, Header h) {
    public Vector3 Translation = r.ReadVector3();       // Unknown.
    public float[] UnknownFloats1 = h.V <= 0x0A040001 ? r.ReadSingle() : default; // Unknown.
    public float UnknownFloat1 = r.ReadSingle();        // Unknown.
    public float UnknownFloat2 = r.ReadSingle();        // Unknown.
    public float UnknownFloat3 = r.ReadSingle();        // Unknown.
    public int UnknownInt1 = r.ReadInt32();             // Unknown.
}

/// <summary>
/// Particle system data.
/// </summary>
public class NiPSysData : NiParticlesData {
    public ParticleDesc[] ParticleDescriptions;
    public bool HasRotationSpeeds;
    public float[] RotationSpeeds;
    public ushort NumAddedParticles;
    public ushort AddedParticlesBase;
}

/// <summary>
/// Particle meshes data.
/// </summary>
public class NiMeshPSysData : NiPSysData {
    public uint DefaultPoolSize;
    public bool FillPoolsOnLoad;
    public uint[] Generations;
    public int? ParticleMeshes;
}

/// <summary>
/// Binary extra data object. Used to store tangents and bitangents in Oblivion.
/// </summary>
public class NiBinaryExtraData : NiExtraData {
    public byte[] BinaryData;                           // The binary data.
}

/// <summary>
/// Voxel extra data object.
/// </summary>
public class NiBinaryVoxelExtraData : NiExtraData {
    public uint UnknownInt;                             // Unknown.  0?
    public int? Data;                                   // Link to binary voxel data.
}

/// <summary>
/// Voxel data object.
/// </summary>
public class NiBinaryVoxelData : NiObject {
    public ushort UnknownShort1;                        // Unknown.
    public ushort UnknownShort2;                        // Unknown.
    public ushort UnknownShort3;                        // Unknown. Is this^3 the Unknown Bytes 1 size?
    public float[] Unknown7Floats;                      // Unknown.
    public byte[][] UnknownBytes1;                      // Unknown. Always a multiple of 7.
    public Vector4[] UnknownVectors;                    // Vectors on the unit sphere.
    public byte[] UnknownBytes2;                        // Unknown.
    public uint[] Unknown5Ints;                         // Unknown.
}

/// <summary>
/// Blends bool values together.
/// </summary>
public class NiBlendBoolInterpolator : NiBlendInterpolator {
    public byte Value;                                  // The pose value. Invalid if using data.
}

/// <summary>
/// Blends float values together.
/// </summary>
public class NiBlendFloatInterpolator : NiBlendInterpolator {
    public float Value;                                 // The pose value. Invalid if using data.
}

/// <summary>
/// Blends NiPoint3 values together.
/// </summary>
public class NiBlendPoint3Interpolator : NiBlendInterpolator {
    public Vector3 Value;                               // The pose value. Invalid if using data.
}

/// <summary>
/// Blends NiQuatTransform values together.
/// </summary>
public class NiBlendTransformInterpolator : NiBlendInterpolator {
    public NiQuatTransform Value;
}

/// <summary>
/// Wrapper for boolean animation keys.
/// </summary>
public class NiBoolData : NiObject {
    public KeyGroup Data;                               // The boolean keys.
}

/// <summary>
/// Boolean extra data.
/// </summary>
public class NiBooleanExtraData : NiExtraData {
    public byte BooleanData;                            // The boolean extra data value.
}

/// <summary>
/// Contains an NiBSplineBasis for use in interpolation of open, uniform B-Splines.
/// </summary>
public class NiBSplineBasisData : NiObject {
    public uint NumControlPoints;                       // The number of control points of the B-spline (number of frames of animation plus degree of B-spline minus one).
}

/// <summary>
/// Uses B-Splines to animate a float value over time.
/// </summary>
public abstract class NiBSplineFloatInterpolator : NiBSplineInterpolator {
    public float Value;                                 // Base value when curve not defined.
    public uint Handle;                                 // Handle into the data. (USHRT_MAX for invalid handle.)
}

/// <summary>
/// NiBSplineFloatInterpolator plus the information required for using compact control points.
/// </summary>
public class NiBSplineCompFloatInterpolator : NiBSplineFloatInterpolator {
    public float FloatOffset;
    public float FloatHalfRange;
}

/// <summary>
/// Uses B-Splines to animate an NiPoint3 value over time.
/// </summary>
public abstract class NiBSplinePoint3Interpolator : NiBSplineInterpolator {
    public Vector3 Value;                               // Base value when curve not defined.
    public uint Handle;                                 // Handle into the data. (USHRT_MAX for invalid handle.)
}

/// <summary>
/// NiBSplinePoint3Interpolator plus the information required for using compact control points.
/// </summary>
public class NiBSplineCompPoint3Interpolator : NiBSplinePoint3Interpolator {
    public float PositionOffset;
    public float PositionHalfRange;
}

/// <summary>
/// Supports the animation of position, rotation, and scale using an NiQuatTransform.
/// The NiQuatTransform can be an unchanging pose or interpolated from B-Spline control point channels.
/// </summary>
public class NiBSplineTransformInterpolator : NiBSplineInterpolator {
    public NiQuatTransform Transform;
    public uint TranslationHandle;                      // Handle into the translation data. (USHRT_MAX for invalid handle.)
    public uint RotationHandle;                         // Handle into the rotation data. (USHRT_MAX for invalid handle.)
    public uint ScaleHandle;                            // Handle into the scale data. (USHRT_MAX for invalid handle.)
}

/// <summary>
/// NiBSplineTransformInterpolator plus the information required for using compact control points.
/// </summary>
public class NiBSplineCompTransformInterpolator : NiBSplineTransformInterpolator {
    public float TranslationOffset;
    public float TranslationHalfRange;
    public float RotationOffset;
    public float RotationHalfRange;
    public float ScaleOffset;
    public float ScaleHalfRange;
}

public class BSRotAccumTransfInterpolator : NiTransformInterpolator {
}

/// <summary>
/// Contains one or more sets of control points for use in interpolation of open, uniform B-Splines, stored as either float or compact.
/// </summary>
public class NiBSplineData : NiObject {
    public float[] FloatControlPoints;                  // Float values representing the control data.
    public short[] CompactControlPoints;                // Signed shorts representing the data from 0 to 1 (scaled by SHRT_MAX).
}

/// <summary>
/// Camera object.
/// </summary>
public class NiCamera : NiAVObject {
    public ushort CameraFlags;                          // Obsolete flags.
    public float FrustumLeft;                           // Frustrum left.
    public float FrustumRight;                          // Frustrum right.
    public float FrustumTop;                            // Frustrum top.
    public float FrustumBottom;                         // Frustrum bottom.
    public float FrustumNear;                           // Frustrum near.
    public float FrustumFar;                            // Frustrum far.
    public bool UseOrthographicProjection;              // Determines whether perspective is used.  Orthographic means no perspective.
    public float ViewportLeft;                          // Viewport left.
    public float ViewportRight;                         // Viewport right.
    public float ViewportTop;                           // Viewport top.
    public float ViewportBottom;                        // Viewport bottom.
    public float LODAdjust;                             // Level of detail adjust.
    public int? Scene;
    public uint NumScreenPolygons;                      // Deprecated. Array is always zero length on disk write.
    public uint NumScreenTextures;                      // Deprecated. Array is always zero length on disk write.
    public uint UnknownInt3;                            // Unknown.
}

/// <summary>
/// Wrapper for color animation keys.
/// </summary>
public class NiColorData : NiObject {
    public KeyGroup Data;                               // The color keys.
}

/// <summary>
/// Extra data in the form of NiColorA (red, green, blue, alpha).
/// </summary>
public class NiColorExtraData : NiExtraData {
    public Color4 Data;                                 // RGBA Color?
}

/// <summary>
/// Controls animation sequences on a specific branch of the scene graph.
/// </summary>
public class NiControllerManager : NiTimeController {
    public bool Cumulative;                             // Whether transformation accumulation is enabled. If accumulation is not enabled, the manager will treat all sequence data on the accumulation root as absolute data instead of relative delta values.
    public int?[] ControllerSequences;
    public int? ObjectPalette;
}

/// <summary>
/// Root node in NetImmerse .kf files (until version 10.0).
/// </summary>
public class NiSequence : NiObject {
    public string Name;                                 // The sequence name by which the animation system finds and manages this sequence.
    public string AccumRootName;                        // The name of the NiAVObject serving as the accumulation root. This is where all accumulated translations, scales, and rotations are applied.
    public int? TextKeys;
    public int UnknownInt4;                             // Divinity 2
    public int UnknownInt5;                             // Divinity 2
    public uint NumControlledBlocks;
    public uint ArrayGrowBy;
    public ControlledBlock[] ControlledBlocks;
}

/// <summary>
/// Root node in Gamebryo .kf files (version 10.0.1.0 and up).
/// </summary>
public class NiControllerSequence : NiSequence {
    public float Weight;                                // The weight of a sequence describes how it blends with other sequences at the same priority.
    public int? TextKeys;
    public CycleType CycleType;
    public float Frequency;
    public float Phase;
    public float StartTime;
    public float StopTime;
    public bool PlayBackwards;
    public int? Manager;                                // The owner of this sequence.
    public string AccumRootName;                        // The name of the NiAVObject serving as the accumulation root. This is where all accumulated translations, scales, and rotations are applied.
    public AccumFlags AccumFlags;
    public int? StringPalette;
    public int? AnimNotes;
    public int?[] AnimNoteArrays;
}

/// <summary>
/// Abstract base class for indexing NiAVObject by name.
/// </summary>
public abstract class NiAVObjectPalette : NiObject {
}

/// <summary>
/// NiAVObjectPalette implementation. Used to quickly look up objects by name.
/// </summary>
public class NiDefaultAVObjectPalette : NiAVObjectPalette {
    public int? Scene;                                  // Scene root of the object palette.
    public AVObject[] Objs;                             // The objects.
}

/// <summary>
/// Directional light source.
/// </summary>
public class NiDirectionalLight : NiLight {
}

/// <summary>
/// NiDitherProperty allows the application to turn the dithering of interpolated colors and fog values on and off.
/// </summary>
public class NiDitherProperty : NiProperty {
    public Flags Flags;                                 // 1 = Enable dithering
}

/// <summary>
/// DEPRECATED (10.2), REMOVED (20.5). Replaced by NiTransformController and NiLookAtInterpolator.
/// </summary>
public class NiRollController : NiSingleInterpController {
    public int? Data;                                   // The data for the controller.
}

/// <summary>
/// Wrapper for 1D (one-dimensional) floating point animation keys.
/// </summary>
public class NiFloatData : NiObject {
    public KeyGroup Data;                               // The keys.
}

/// <summary>
/// Extra float data.
/// </summary>
public class NiFloatExtraData : NiExtraData {
    public float FloatData;                             // The float data.
}

/// <summary>
/// Extra float array data.
/// </summary>
public class NiFloatsExtraData : NiExtraData {
    public float[] Data;                                // Float data.
}

/// <summary>
/// NiFogProperty allows the application to enable, disable and control the appearance of fog.
/// </summary>
public class NiFogProperty : NiProperty {
    public Flags Flags;                                 // Bit 0: Enables Fog
                                                        //     Bit 1: Sets Fog Function to FOG_RANGE_SQ
                                                        //     Bit 2: Sets Fog Function to FOG_VERTEX_ALPHA
                                                        // 
                                                        //     If Bit 1 and Bit 2 are not set, but fog is enabled, Fog function is FOG_Z_LINEAR.
    public float FogDepth;                              // Depth of the fog in normalized units. 1.0 = begins at near plane. 0.5 = begins halfway between the near and far planes.
    public Color3 FogColor;                             // The color of the fog.
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier. Applies a gravitational field on the particles.
/// </summary>
public class NiGravity : NiParticleModifier {
    public float UnknownFloat1;                         // Unknown.
    public float Force;                                 // The strength/force of this gravity.
    public FieldType Type;                              // The force field type.
    public Vector3 Position;                            // The position of the mass point relative to the particle system.
    public Vector3 Direction;                           // The direction of the applied acceleration.
}

/// <summary>
/// Extra integer data.
/// </summary>
public class NiIntegerExtraData : NiExtraData {
    public uint IntegerData;                            // The value of the extra data.
}

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
public class BSXFlags : NiIntegerExtraData {
}

/// <summary>
/// Extra integer array data.
/// </summary>
public class NiIntegersExtraData : NiExtraData {
    public uint[] Data;                                 // Integers.
}

/// <summary>
/// An extended keyframe controller.
/// </summary>
public class BSKeyframeController : NiKeyframeController {
    public int? Data2;                                  // A link to more keyframe data.
}

/// <summary>
/// DEPRECATED (10.2), RENAMED (10.2) to NiTransformData.
/// Wrapper for transformation animation keys.
/// </summary>
public class NiKeyframeData : NiObject {
    public uint NumRotationKeys;                        // The number of quaternion rotation keys. If the rotation type is XYZ (type 4) then this *must* be set to 1, and in this case the actual number of keys is stored in the XYZ Rotations field.
    public KeyType RotationType;                        // The type of interpolation to use for rotation.  Can also be 4 to indicate that separate X, Y, and Z values are used for the rotation instead of Quaternions.
    public QuatKey[] QuaternionKeys;                    // The rotation keys if Quaternion rotation is used.
    public float Order;
    public KeyGroup[] XYZRotations;                     // Individual arrays of keys for rotating X, Y, and Z individually.
    public KeyGroup Translations;                       // Translation keys.
    public KeyGroup Scales;                             // Scale keys.
}

[Flags]
public enum LookAtFlags : ushort {
    LOOK_FLIP = 0,                  // Flip
    LOOK_Y_AXIS = 1 << 1,           // Y-Axis
    LOOK_Z_AXIS = 1 << 2            // Z-Axis
}

/// <summary>
/// DEPRECATED (10.2), REMOVED (20.5)
/// Replaced by NiTransformController and NiLookAtInterpolator.
/// </summary>
public class NiLookAtController : NiTimeController {
    public LookAtFlags Flags;
    public int? LookAt;
}

/// <summary>
/// NiLookAtInterpolator rotates an object so that it always faces a target object.
/// </summary>
public class NiLookAtInterpolator : NiInterpolator {
    public LookAtFlags Flags;
    public int? LookAt;
    public string LookAtName;
    public NiQuatTransform Transform;
    public int? Interpolator:Translation;
    public int? Interpolator:Roll;
    public int? Interpolator:Scale;
}

/// <summary>
/// Describes the surface properties of an object e.g. translucency, ambient color, diffuse color, emissive color, and specular color.
/// </summary>
public class NiMaterialProperty : NiProperty {
    public Flags Flags;                                 // Property flags.
    public Color3 AmbientColor;                         // How much the material reflects ambient light.
    public Color3 DiffuseColor;                         // How much the material reflects diffuse light.
    public Color3 SpecularColor;                        // How much light the material reflects in a specular manner.
    public Color3 EmissiveColor;                        // How much light the material emits.
    public float Glossiness;                            // The material glossiness.
    public float Alpha;                                 // The material transparency (1=non-transparant). Refer to a NiAlphaProperty object in this material's parent NiTriShape object, when alpha is not 1.
    public float EmissiveMult;
}

/// <summary>
/// DEPRECATED (20.5), replaced by NiMorphMeshModifier.
/// Geometry morphing data.
/// </summary>
public class NiMorphData : NiObject {
    public uint NumMorphs;                              // Number of morphing object.
    public uint NumVertices;                            // Number of vertices.
    public byte RelativeTargets;                        // This byte is always 1 in all official files.
    public Morph[] Morphs;                              // The geometry morphing objects.
}

/// <summary>
/// Generic node object for grouping.
/// </summary>
public class NiNode : NiAVObject {
    public int?[] Children;                             // List of child node object indices.
    public int?[] Effects;                              // List of node effects. ADynamicEffect?
}

/// <summary>
/// A NiNode used as a skeleton bone?
/// </summary>
public class NiBone : NiNode {
}

/// <summary>
/// Morrowind specific.
/// </summary>
public class AvoidNode : NiNode {
}

/// <summary>
/// Firaxis-specific UI widgets?
/// </summary>
public class FxWidget : NiNode {
    public byte Unknown3;                               // Unknown.
    public byte[] Unknown292Bytes;                      // Looks like 9 links and some string data.
}

/// <summary>
/// Unknown.
/// </summary>
public class FxButton : FxWidget {
}

/// <summary>
/// Unknown.
/// </summary>
public class FxRadioButton : FxWidget {
    public uint UnknownInt1;                            // Unknown.
    public uint UnknownInt2;                            // Unknown.
    public uint UnknownInt3;                            // Unknown.
    public int?[] Buttons;                              // Unknown pointers to other buttons.  Maybe other buttons in a group so they can be switch off if this one is switched on?
}

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
public class NiBillboardNode : NiNode {
    public BillboardMode BillboardMode;                 // The way the billboard will react to the camera.
}

/// <summary>
/// Bethesda-specific extension of Node with animation properties stored in the flags, often 42?
/// </summary>
public class NiBSAnimationNode : NiNode {
}

/// <summary>
/// Unknown.
/// </summary>
public class NiBSParticleNode : NiNode {
}

/// <summary>
/// Flags for NiSwitchNode.
/// </summary>
[Flags]
public enum NiSwitchFlags : ushort {
    UpdateOnlyActiveChild = 0,      // Update Only Active Child
    UpdateControllers = 1 << 1      // Update Controllers
}

/// <summary>
/// Represents groups of multiple scenegraph subtrees, only one of which (the "active child") is drawn at any given time.
/// </summary>
public class NiSwitchNode : NiNode {
    public NiSwitchFlags SwitchNodeFlags;
    public uint Index;
}

/// <summary>
/// Level of detail selector. Links to different levels of detail of the same model, used to switch a geometry at a specified distance.
/// </summary>
public class NiLODNode : NiSwitchNode {
    public Vector3 LODCenter;
    public LODRange[] LODLevels;
    public int? LODLevelData;
}

/// <summary>
/// NiPalette objects represent mappings from 8-bit indices to 24-bit RGB or 32-bit RGBA colors.
/// </summary>
public class NiPalette : NiObject {
    public byte HasAlpha;
    public uint NumEntries;                             // The number of palette entries. Always 256 but can also be 16.
    public ByteColor4[] Palette;                        // The color palette.
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiParticleBomb : NiParticleModifier {
    public float Decay;
    public float Duration;
    public float DeltaV;
    public float Start;
    public DecayType DecayType;
    public SymmetryType SymmetryType;
    public Vector3 Position;                            // The position of the mass point relative to the particle system?
    public Vector3 Direction;                           // The direction of the applied acceleration?
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiParticleColorModifier : NiParticleModifier {
    public int? ColorData;
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiParticleGrowFade : NiParticleModifier {
    public float Grow;                                  // The time from the beginning of the particle lifetime during which the particle grows.
    public float Fade;                                  // The time from the end of the particle lifetime during which the particle fades.
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiParticleMeshModifier : NiParticleModifier {
    public int?[] ParticleMeshes;
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiParticleRotation : NiParticleModifier {
    public byte RandomInitialAxis;
    public Vector3 InitialAxis;
    public float RotationSpeed;
}

/// <summary>
/// Generic particle system node.
/// </summary>
public class NiParticles : NiGeometry {
    public BSVertexDesc VertexDesc;
}

/// <summary>
/// LEGACY (pre-10.1). NiParticles which do not house normals and generate them at runtime.
/// </summary>
public class NiAutoNormalParticles : NiParticles {
}

/// <summary>
/// LEGACY (pre-10.1). Particle meshes.
/// </summary>
public class NiParticleMeshes : NiParticles {
}

/// <summary>
/// LEGACY (pre-10.1). Particle meshes data.
/// </summary>
public class NiParticleMeshesData : NiRotatingParticlesData {
    public int? UnknownLink2;                           // Refers to the mesh that makes up a particle?
}

/// <summary>
/// A particle system.
/// </summary>
public class NiParticleSystem : NiParticles {
    public ushort FarBegin;
    public ushort FarEnd;
    public ushort NearBegin;
    public ushort NearEnd;
    public int? Data;
    public bool WorldSpace;                             // If true, Particles are birthed into world space.  If false, Particles are birthed into object space.
    public int?[] Modifiers;                            // The list of particle modifiers.
}

/// <summary>
/// Particle system.
/// </summary>
public class NiMeshParticleSystem : NiParticleSystem {
}

/// <summary>
/// A generic particle system time controller object.
/// </summary>
public class NiParticleSystemController : NiTimeController {
    public uint OldSpeed;                               // Particle speed in old files
    public float Speed;                                 // Particle speed
    public float SpeedRandom;                           // Particle random speed modifier
    public float VerticalDirection;                     // vertical emit direction [radians]
                                                        //     0.0 : up
                                                        //     1.6 : horizontal
                                                        //     3.1416 : down
    public float VerticalAngle;                         // emitter's vertical opening angle [radians]
    public float HorizontalDirection;                   // horizontal emit direction
    public float HorizontalAngle;                       // emitter's horizontal opening angle
    public Vector3 UnknownNormal?;                      // Unknown.
    public Color4 UnknownColor?;                        // Unknown.
    public float Size;                                  // Particle size
    public float EmitStartTime;                         // Particle emit start time
    public float EmitStopTime;                          // Particle emit stop time
    public byte UnknownByte;                            // Unknown byte, (=0)
    public uint OldEmitRate;                            // Particle emission rate in old files
    public float EmitRate;                              // Particle emission rate (particles per second)
    public float Lifetime;                              // Particle lifetime
    public float LifetimeRandom;                        // Particle lifetime random modifier
    public ushort EmitFlags;                            // Bit 0: Emit Rate toggle bit (0 = auto adjust, 1 = use Emit Rate value)
    public Vector3 StartRandom;                         // Particle random start translation vector
    public int? Emitter;                                // This index targets the particle emitter object (TODO: find out what type of object this refers to).
    public ushort UnknownShort2?;                       // ? short=0 ?
    public float UnknownFloat13?;                       // ? float=1.0 ?
    public uint UnknownInt1?;                           // ? int=1 ?
    public uint UnknownInt2?;                           // ? int=0 ?
    public ushort UnknownShort3?;                       // ? short=0 ?
    public Vector3 ParticleVelocity;                    // Particle velocity
    public Vector3 ParticleUnknownVector;               // Unknown
    public float ParticleLifetime;                      // The particle's age.
    public int? ParticleLink;
    public uint ParticleTimestamp;                      // Timestamp of the last update.
    public ushort ParticleUnknownShort;                 // Unknown short
    public ushort ParticleVertexId;                     // Particle/vertex index matches array index
    public ushort NumParticles;                         // Size of the following array. (Maximum number of simultaneous active particles)
    public ushort NumValid;                             // Number of valid entries in the following array. (Number of active particles at the time the system was saved)
    public Particle[] Particles;                        // Individual particle modifiers?
    public int? UnknownLink;                            // unknown int (=0xffffffff)
    public int? ParticleExtra;                          // Link to some optional particle modifiers (NiGravity, NiParticleGrowFade, NiParticleBomb, ...)
    public int? UnknownLink2;                           // Unknown int (=0xffffffff)
    public byte Trailer;                                // Trailing null byte
    public int? ColorData;
    public float UnknownFloat1;
    public float[] UnknownFloats2;
}

/// <summary>
/// A particle system controller, used by BS in conjunction with NiBSParticleNode.
/// </summary>
public class NiBSPArrayController : NiParticleSystemController {
}

/// <summary>
/// DEPRECATED (10.2), REMOVED (20.5). Replaced by NiTransformController and NiPathInterpolator.
/// Time controller for a path.
/// </summary>
public class NiPathController : NiTimeController {
    public PathFlags PathFlags;
    public int BankDir;                                 // -1 = Negative, 1 = Positive
    public float MaxBankAngle;                          // Max angle in radians.
    public float Smoothing;
    public short FollowAxis;                            // 0, 1, or 2 representing X, Y, or Z.
    public int? PathData;
    public int? PercentData;
}

public class PixelFormatComponent(BinaryReader r) {
    public PixelComponent Type = (PixelComponent)r.ReadUInt32(); // Component Type
    public PixelRepresentation Convention = (PixelRepresentation)r.ReadUInt32(); // Data Storage Convention
    public byte BitsPerChannel = r.ReadByte();          // Bits per component
    public bool IsSigned = r.ReadBool32();
}

public abstract class NiPixelFormat : NiObject {
    public PixelFormat PixelFormat;                     // The format of the pixels in this internally stored image.
    public uint RedMask;                                // 0x000000ff (for 24bpp and 32bpp) or 0x00000000 (for 8bpp)
    public uint GreenMask;                              // 0x0000ff00 (for 24bpp and 32bpp) or 0x00000000 (for 8bpp)
    public uint BlueMask;                               // 0x00ff0000 (for 24bpp and 32bpp) or 0x00000000 (for 8bpp)
    public uint AlphaMask;                              // 0xff000000 (for 32bpp) or 0x00000000 (for 24bpp and 8bpp)
    public byte BitsPerPixel;                           // Bits per pixel, 0 (Compressed), 8, 24 or 32.
    public byte[] OldFastCompare;                       // [96,8,130,0,0,65,0,0] if 24 bits per pixel
                                                        //     [129,8,130,32,0,65,12,0] if 32 bits per pixel
                                                        //     [34,0,0,0,0,0,0,0] if 8 bits per pixel
                                                        //     [X,0,0,0,0,0,0,0] if 0 (Compressed) bits per pixel where X = PixelFormat
    public PixelTiling Tiling;
    public uint RendererHint;
    public uint ExtraData;
    public byte Flags;
    public bool sRGBSpace;
    public PixelFormatComponent[] Channels;             // Channel Data
}

public class NiPersistentSrcTextureRendererData : NiPixelFormat {
    public int? Palette;
    public uint NumMipmaps;
    public uint BytesPerPixel;
    public ??[] Mipmaps;
    public uint NumPixels;
    public uint PadNumPixels;
    public uint NumFaces;
    public PlatformID Platform;
    public RendererID Renderer;
    public byte[] PixelData;
}

/// <summary>
/// A texture.
/// </summary>
public class NiPixelData : NiPixelFormat {
    public int? Palette;
    public uint NumMipmaps;
    public uint BytesPerPixel;
    public ??[] Mipmaps;
    public uint NumPixels;
    public uint NumFaces;
    public byte[] PixelData;
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiPlanarCollider : NiParticleModifier {
    public ushort UnknownShort;                         // Usually 0?
    public float UnknownFloat1;                         // Unknown.
    public float UnknownFloat2;                         // Unknown.
    public ushort UnknownShort2;                        // Unknown.
    public float UnknownFloat3;                         // Unknown.
    public float UnknownFloat4;                         // Unknown.
    public float UnknownFloat5;                         // Unknown.
    public float UnknownFloat6;                         // Unknown.
    public float UnknownFloat7;                         // Unknown.
    public float UnknownFloat8;                         // Unknown.
    public float UnknownFloat9;                         // Unknown.
    public float UnknownFloat10;                        // Unknown.
    public float UnknownFloat11;                        // Unknown.
    public float UnknownFloat12;                        // Unknown.
    public float UnknownFloat13;                        // Unknown.
    public float UnknownFloat14;                        // Unknown.
    public float UnknownFloat15;                        // Unknown.
    public float UnknownFloat16;                        // Unknown.
}

/// <summary>
/// A point light.
/// </summary>
public class NiPointLight : NiLight {
    public float ConstantAttenuation;
    public float LinearAttenuation;
    public float QuadraticAttenuation;
}

/// <summary>
/// Abstract base class for dynamic effects such as NiLights or projected texture effects.
/// </summary>
public abstract class NiDeferredDynamicEffect : NiAVObject {
    public bool SwitchState;                            // If true, then the dynamic effect is applied to affected nodes during rendering.
}

/// <summary>
/// Abstract base class that represents light sources in a scene graph.
/// For Bethesda Stream 130 (FO4), NiLight now directly inherits from NiAVObject.
/// </summary>
public abstract class NiDeferredLight : NiDeferredDynamicEffect {
    public float Dimmer;                                // Scales the overall brightness of all light components.
    public Color3 AmbientColor;
    public Color3 DiffuseColor;
    public Color3 SpecularColor;
}

/// <summary>
/// A deferred point light. Custom (Twin Saga).
/// </summary>
public class NiDeferredPointLight : NiDeferredLight {
    public float ConstantAttenuation;
    public float LinearAttenuation;
    public float QuadraticAttenuation;
}

/// <summary>
/// Wrapper for position animation keys.
/// </summary>
public class NiPosData : NiObject {
    public KeyGroup Data;
}

/// <summary>
/// Wrapper for rotation animation keys.
/// </summary>
public class NiRotData : NiObject {
    public uint NumRotationKeys;
    public KeyType RotationType;
    public QuatKey[] QuaternionKeys;
    public KeyGroup[] XYZRotations;
}

/// <summary>
/// Particle modifier that controls and updates the age of particles in the system.
/// </summary>
public class NiPSysAgeDeathModifier : NiPSysModifier {
    public bool SpawnonDeath;                           // Should the particles spawn on death?
    public int? SpawnModifier;                          // The spawner to use on death.
}

/// <summary>
/// Particle modifier that applies an explosive force to particles.
/// </summary>
public class NiPSysBombModifier : NiPSysModifier {
    public int? BombObject;                             // The object whose position and orientation are the basis of the force.
    public Vector3 BombAxis;                            // The local direction of the force.
    public float Decay;                                 // How the bomb force will decrease with distance.
    public float DeltaV;                                // The acceleration the bomb will apply to particles.
    public DecayType DecayType;
    public SymmetryType SymmetryType;
}

/// <summary>
/// Particle modifier that creates and updates bound volumes.
/// </summary>
public class NiPSysBoundUpdateModifier : NiPSysModifier {
    public ushort UpdateSkip;                           // Optimize by only computing the bound of (1 / Update Skip) of the total particles each frame.
}

/// <summary>
/// Particle emitter that uses points within a defined Box shape to emit from.
/// </summary>
public class NiPSysBoxEmitter : NiPSysVolumeEmitter {
    public float Width;
    public float Height;
    public float Depth;
}

/// <summary>
/// Particle modifier that adds a defined shape to act as a collision object for particles to interact with.
/// </summary>
public class NiPSysColliderManager : NiPSysModifier {
    public int? Collider;
}

/// <summary>
/// Particle modifier that adds keyframe data to modify color/alpha values of particles over time.
/// </summary>
public class NiPSysColorModifier : NiPSysModifier {
    public int? Data;
}

/// <summary>
/// Particle emitter that uses points within a defined Cylinder shape to emit from.
/// </summary>
public class NiPSysCylinderEmitter : NiPSysVolumeEmitter {
    public float Radius;
    public float Height;
}

/// <summary>
/// Particle modifier that applies a linear drag force to particles.
/// </summary>
public class NiPSysDragModifier : NiPSysModifier {
    public int? DragObject;                             // The object whose position and orientation are the basis of the force.
    public Vector3 DragAxis;                            // The local direction of the force.
    public float Percentage;                            // The amount of drag to apply to particles.
    public float Range;                                 // The distance up to which particles are fully affected.
    public float RangeFalloff;                          // The distance at which particles cease to be affected.
}

/// <summary>
/// DEPRECATED (10.2). Particle system emitter controller data.
/// </summary>
public class NiPSysEmitterCtlrData : NiObject {
    public KeyGroup BirthRateKeys;
    public Key[] ActiveKeys;
}

/// <summary>
/// Particle modifier that applies a gravitational force to particles.
/// </summary>
public class NiPSysGravityModifier : NiPSysModifier {
    public int? GravityObject;                          // The object whose position and orientation are the basis of the force.
    public Vector3 GravityAxis;                         // The local direction of the force.
    public float Decay;                                 // How the force diminishes by distance.
    public float Strength;                              // The acceleration of the force.
    public ForceType ForceType;                         // The type of gravitational force.
    public float Turbulence;                            // Adds a degree of randomness.
    public float TurbulenceScale;                       // Scale for turbulence.
    public bool WorldAligned;
}

/// <summary>
/// Particle modifier that controls the time it takes to grow and shrink a particle.
/// </summary>
public class NiPSysGrowFadeModifier : NiPSysModifier {
    public float GrowTime;                              // The time taken to grow from 0 to their specified size.
    public ushort GrowGeneration;                       // Specifies the particle generation to which the grow effect should be applied. This is usually generation 0, so that newly created particles will grow.
    public float FadeTime;                              // The time taken to shrink from their specified size to 0.
    public ushort FadeGeneration;                       // Specifies the particle generation to which the shrink effect should be applied. This is usually the highest supported generation for the particle system.
    public float BaseScale;                             // A multiplier on the base particle scale.
}

/// <summary>
/// Particle emitter that uses points on a specified mesh to emit from.
/// </summary>
public class NiPSysMeshEmitter : NiPSysEmitter {
    public int?[] EmitterMeshes;                        // The meshes which are emitted from.
    public VelocityType InitialVelocityType;            // The method by which the initial particle velocity will be computed.
    public EmitFrom EmissionType;                       // The manner in which particles are emitted from the Emitter Meshes.
    public Vector3 EmissionAxis;                        // The emission axis if VELOCITY_USE_DIRECTION.
}

/// <summary>
/// Particle modifier that updates mesh particles using the age of each particle.
/// </summary>
public class NiPSysMeshUpdateModifier : NiPSysModifier {
    public int?[] Meshes;
}

public class BSPSysInheritVelocityModifier : NiPSysModifier {
    public int? Target;
    public float ChanceToInherit;
    public float VelocityMultiplier;
    public float VelocityVariation;
}

public class BSPSysHavokUpdateModifier : NiPSysModifier {
    public int?[] Nodes;
    public int? Modifier;
}

public class BSPSysRecycleBoundModifier : NiPSysModifier {
    public Vector3 BoundOffset;
    public Vector3 BoundExtent;
    public int? Target;
}

/// <summary>
/// Similar to a Flip Controller, this handles particle texture animation on a single texture atlas
/// </summary>
public class BSPSysSubTexModifier : NiPSysModifier {
    public uint StartFrame;                             // Starting frame/position on atlas
    public float StartFrameFudge;                       // Random chance to start on a different frame?
    public float EndFrame;                              // Ending frame/position on atlas
    public float LoopStartFrame;                        // Frame to start looping
    public float LoopStartFrameFudge;
    public float FrameCount;
    public float FrameCountFudge;
}

/// <summary>
/// Particle Collider object which particles will interact with.
/// </summary>
public class NiPSysPlanarCollider : NiPSysCollider {
    public float Width;                                 // Width of the plane along the X Axis.
    public float Height;                                // Height of the plane along the Y Axis.
    public Vector3 XAxis;                               // Axis defining a plane, relative to Collider Object.
    public Vector3 YAxis;                               // Axis defining a plane, relative to Collider Object.
}

/// <summary>
/// Particle Collider object which particles will interact with.
/// </summary>
public class NiPSysSphericalCollider : NiPSysCollider {
    public float Radius;
}

/// <summary>
/// Particle modifier that updates the particle positions based on velocity and last update time.
/// </summary>
public class NiPSysPositionModifier : NiPSysModifier {
}

/// <summary>
/// Particle modifier that calls reset on a target upon looping.
/// </summary>
public class NiPSysResetOnLoopCtlr : NiTimeController {
}

/// <summary>
/// Particle modifier that adds rotations to particles.
/// </summary>
public class NiPSysRotationModifier : NiPSysModifier {
    public float RotationSpeed;                         // Initial Rotation Speed in radians per second.
    public float RotationSpeedVariation;                // Distributes rotation speed over the range [Speed - Variation, Speed + Variation].
    public float RotationAngle;                         // Initial Rotation Angle in radians.
    public float RotationAngleVariation;                // Distributes rotation angle over the range [Angle - Variation, Angle + Variation].
    public bool RandomRotSpeedSign;                     // Randomly negate the initial rotation speed?
    public bool RandomAxis;                             // Assign a random axis to new particles?
    public Vector3 Axis;                                // Initial rotation axis.
}

/// <summary>
/// Particle modifier that spawns additional copies of a particle.
/// </summary>
public class NiPSysSpawnModifier : NiPSysModifier {
    public ushort NumSpawnGenerations;                  // Number of allowed generations for spawning. Particles whose generations are >= will not be spawned.
    public float PercentageSpawned;                     // The likelihood of a particular particle being spawned. Must be between 0.0 and 1.0.
    public ushort MinNumtoSpawn;                        // The minimum particles to spawn for any given original particle.
    public ushort MaxNumtoSpawn;                        // The maximum particles to spawn for any given original particle.
    public int UnknownInt;                              // WorldShift
    public float SpawnSpeedVariation;                   // How much the spawned particle speed can vary.
    public float SpawnDirVariation;                     // How much the spawned particle direction can vary.
    public float LifeSpan;                              // Lifespan assigned to spawned particles.
    public float LifeSpanVariation;                     // The amount the lifespan can vary.
}

/// <summary>
/// Particle emitter that uses points within a sphere shape to emit from.
/// </summary>
public class NiPSysSphereEmitter : NiPSysVolumeEmitter {
    public float Radius;
}

/// <summary>
/// Particle system controller, tells the system to update its simulation.
/// </summary>
public class NiPSysUpdateCtlr : NiTimeController {
}

/// <summary>
/// Base for all force field particle modifiers.
/// </summary>
public abstract class NiPSysFieldModifier : NiPSysModifier {
    public int? FieldObject;                            // The object whose position and orientation are the basis of the field.
    public float Magnitude;                             // Magnitude of the force.
    public float Attenuation;                           // How the magnitude diminishes with distance from the Field Object.
    public bool UseMaxDistance;                         // Whether or not to use a distance from the Field Object after which there is no effect.
    public float MaxDistance;                           // Maximum distance after which there is no effect.
}

/// <summary>
/// Particle system modifier, implements a vortex field force for particles.
/// </summary>
public class NiPSysVortexFieldModifier : NiPSysFieldModifier {
    public Vector3 Direction;                           // Direction of the vortex field in Field Object's space.
}

/// <summary>
/// Particle system modifier, implements a gravity field force for particles.
/// </summary>
public class NiPSysGravityFieldModifier : NiPSysFieldModifier {
    public Vector3 Direction;                           // Direction of the gravity field in Field Object's space.
}

/// <summary>
/// Particle system modifier, implements a drag field force for particles.
/// </summary>
public class NiPSysDragFieldModifier : NiPSysFieldModifier {
    public bool UseDirection;                           // Whether or not the drag force applies only in the direction specified.
    public Vector3 Direction;                           // Direction in which the force applies if Use Direction is true.
}

/// <summary>
/// Particle system modifier, implements a turbulence field force for particles.
/// </summary>
public class NiPSysTurbulenceFieldModifier : NiPSysFieldModifier {
    public float Frequency;                             // How many turbulence updates per second.
}

public class BSPSysLODModifier : NiPSysModifier {
    public float LODBeginDistance;
    public float LODEndDistance;
    public float EndEmitScale;
    public float EndSize;
}

public class BSPSysScaleModifier : NiPSysModifier {
    public float[] Scales;
}

/// <summary>
/// Particle system controller for force field magnitude.
/// </summary>
public class NiPSysFieldMagnitudeCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Particle system controller for force field attenuation.
/// </summary>
public class NiPSysFieldAttenuationCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Particle system controller for force field maximum distance.
/// </summary>
public class NiPSysFieldMaxDistanceCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Particle system controller for air field air friction.
/// </summary>
public class NiPSysAirFieldAirFrictionCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Particle system controller for air field inherit velocity.
/// </summary>
public class NiPSysAirFieldInheritVelocityCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Particle system controller for air field spread.
/// </summary>
public class NiPSysAirFieldSpreadCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Particle system controller for emitter initial rotation speed.
/// </summary>
public class NiPSysInitialRotSpeedCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Particle system controller for emitter initial rotation speed variation.
/// </summary>
public class NiPSysInitialRotSpeedVarCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Particle system controller for emitter initial rotation angle.
/// </summary>
public class NiPSysInitialRotAngleCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Particle system controller for emitter initial rotation angle variation.
/// </summary>
public class NiPSysInitialRotAngleVarCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Particle system controller for emitter planar angle.
/// </summary>
public class NiPSysEmitterPlanarAngleCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Particle system controller for emitter planar angle variation.
/// </summary>
public class NiPSysEmitterPlanarAngleVarCtlr : NiPSysModifierFloatCtlr {
}

/// <summary>
/// Particle system modifier, updates the particle velocity to simulate the effects of air movements like wind, fans, or wake.
/// </summary>
public class NiPSysAirFieldModifier : NiPSysFieldModifier {
    public Vector3 Direction;                           // Direction of the particle velocity
    public float AirFriction;                           // How quickly particles will accelerate to the magnitude of the air field.
    public float InheritVelocity;                       // How much of the air field velocity will be added to the particle velocity.
    public bool InheritRotation;
    public bool ComponentOnly;
    public bool EnableSpread;
    public float Spread;                                // The angle of the air field cone if Enable Spread is true.
}

/// <summary>
/// Guild 2-Specific node
/// </summary>
public class NiPSysTrailEmitter : NiPSysEmitter {
    public int UnknownInt1;                             // Unknown
    public float UnknownFloat1;                         // Unknown
    public float UnknownFloat2;                         // Unknown
    public float UnknownFloat3;                         // Unknown
    public int UnknownInt2;                             // Unknown
    public float UnknownFloat4;                         // Unknown
    public int UnknownInt3;                             // Unknown
    public float UnknownFloat5;                         // Unknown
    public int UnknownInt4;                             // Unknown
    public float UnknownFloat6;                         // Unknown
    public float UnknownFloat7;                         // Unknown
}

/// <summary>
/// Unknown controller
/// </summary>
public class NiLightIntensityController : NiFloatInterpController {
}

/// <summary>
/// Particle system modifier, updates the particle velocity to simulate the effects of point gravity.
/// </summary>
public class NiPSysRadialFieldModifier : NiPSysFieldModifier {
    public float RadialType;                            // If zero, no attenuation.
}

/// <summary>
/// Abstract class used for different types of LOD selections.
/// </summary>
public abstract class NiLODData : NiObject {
}

/// <summary>
/// NiRangeLODData controls switching LOD levels based on Z depth from the camera to the NiLODNode.
/// </summary>
public class NiRangeLODData : NiLODData {
    public Vector3 LODCenter;
    public LODRange[] LODLevels;
}

/// <summary>
/// NiScreenLODData controls switching LOD levels based on proportion of the screen that a bound would include.
/// </summary>
public class NiScreenLODData : NiLODData {
    public NiBound Bound;
    public NiBound WorldBound;
    public float[] ProportionLevels;
}

/// <summary>
/// Unknown.
/// </summary>
public class NiRotatingParticles : NiParticles {
}

/// <summary>
/// DEPRECATED (pre-10.1), REMOVED (20.3).
/// Keyframe animation root node, in .kf files.
/// </summary>
public class NiSequenceStreamHelper : NiObjectNET {
}

/// <summary>
/// Determines whether flat shading or smooth shading is used on a shape.
/// </summary>
public class NiShadeProperty : NiProperty {
    public Flags Flags;                                 // Bit 0: Enable smooth phong shading on this shape. Otherwise, hard-edged flat shading will be used on this shape.
}

/// <summary>
/// Skinning data.
/// </summary>
public class NiSkinData : NiObject {
    public NiTransform SkinTransform;                   // Offset of the skin from this bone in bind position.
    public uint NumBones;                               // Number of bones.
    public int? SkinPartition;                          // This optionally links a NiSkinPartition for hardware-acceleration information.
    public byte HasVertexWeights;                       // Enables Vertex Weights for this NiSkinData.
    public BoneData[] BoneList;                         // Contains offset data for each node that this skin is influenced by.
}

/// <summary>
/// Skinning instance.
/// </summary>
public class NiSkinInstance : NiObject {
    public int? Data;                                   // Skinning data reference.
    public int? SkinPartition;                          // Refers to a NiSkinPartition objects, which partitions the mesh such that every vertex is only influenced by a limited number of bones.
    public int? SkeletonRoot;                           // Armature root node.
    public int?[] Bones;                                // List of all armature bones.
}

/// <summary>
/// Old version of skinning instance.
/// </summary>
public class NiTriShapeSkinController : NiTimeController {
    public uint NumBones;                               // The number of node bones referenced as influences.
    public uint[] VertexCounts;                         // The number of vertex weights stored for each bone.
    public int?[] Bones;                                // List of all armature bones.
    public OldSkinData[][] BoneData;                    // Contains skin weight data for each node that this skin is influenced by.
}

/// <summary>
/// A copy of NISkinInstance for use with NiClod meshes.
/// </summary>
public class NiClodSkinInstance : NiSkinInstance {
}

/// <summary>
/// Skinning data, optimized for hardware skinning. The mesh is partitioned in submeshes such that each vertex of a submesh is influenced only by a limited and fixed number of bones.
/// </summary>
public class NiSkinPartition : NiObject {
    public uint NumSkinPartitionBlocks;
    public SkinPartition[] SkinPartitionBlocks;         // Skin partition objects.
    public uint DataSize;
    public uint VertexSize;
    public BSVertexDesc VertexDesc;
    public BSVertexDataSSE[] VertexData;
    public SkinPartition[] Partition;
}

/// <summary>
/// A texture.
/// </summary>
public abstract class NiTexture : NiObjectNET {
}

/// <summary>
/// NiTexture::FormatPrefs. These preferences are a request to the renderer to use a format the most closely matches the settings and may be ignored.
/// </summary>
public class FormatPrefs(BinaryReader r) {
    public PixelLayout PixelLayout = (PixelLayout)r.ReadUInt32(); // Requests the way the image will be stored.
    public MipMapFormat UseMipmaps = (MipMapFormat)r.ReadUInt32(); // Requests if mipmaps are used or not.
    public AlphaFormat AlphaFormat = (AlphaFormat)r.ReadUInt32(); // Requests no alpha, 1-bit alpha, or
}

/// <summary>
/// Describes texture source and properties.
/// </summary>
public class NiSourceTexture : NiTexture {
    public byte UseExternal;                            // Is the texture external?
    public ?? FileName;                                 // The original source filename of the image embedded by the referred NiPixelData object.
    public int? UnknownLink;                            // Unknown.
    public byte UnknownByte;                            // Unknown. Seems to be set if Pixel Data is present?
    public int? PixelData;                              // NiPixelData or NiPersistentSrcTextureRendererData
    public FormatPrefs FormatPrefs;                     // A set of preferences for the texture format. They are a request only and the renderer may ignore them.
    public byte IsStatic;                               // If set, then the application cannot assume that any dynamic changes to the pixel data will show in the rendered image.
    public bool DirectRender;                           // A hint to the renderer that the texture can be loaded directly from a texture file into a renderer-specific resource, bypassing the NiPixelData object.
    public bool PersistRenderData;                      // Pixel Data is NiPersistentSrcTextureRendererData instead of NiPixelData.
}

/// <summary>
/// Gives specularity to a shape. Flags 0x0001.
/// </summary>
public class NiSpecularProperty : NiProperty {
    public Flags Flags;                                 // Bit 0 = Enable specular lighting on this shape.
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiSphericalCollider : NiParticleModifier {
    public float UnknownFloat1;                         // Unknown.
    public ushort UnknownShort1;                        // Unknown.
    public float UnknownFloat2;                         // Unknown.
    public ushort UnknownShort2;                        // Unknown.
    public float UnknownFloat3;                         // Unknown.
    public float UnknownFloat4;                         // Unknown.
    public float UnknownFloat5;                         // Unknown.
}

/// <summary>
/// A spot.
/// </summary>
public class NiSpotLight : NiPointLight {
    public float OuterSpotAngle;
    public float InnerSpotAngle;
    public float Exponent;                              // Describes the distribution of light. (see: glLight)
}

/// <summary>
/// Allows control of stencil testing.
/// </summary>
public class NiStencilProperty : NiProperty {
    public Flags Flags;                                 // Property flags:
                                                        //     Bit 0: Stencil Enable
                                                        //     Bits 1-3: Fail Action
                                                        //     Bits 4-6: Z Fail Action
                                                        //     Bits 7-9: Pass Action
                                                        //     Bits 10-11: Draw Mode
                                                        //     Bits 12-14: Stencil Function
    public byte StencilEnabled;                         // Enables or disables the stencil test.
    public StencilCompareMode StencilFunction;          // Selects the compare mode function (see: glStencilFunc).
    public uint StencilRef;
    public uint StencilMask;                            // A bit mask. The default is 0xffffffff.
    public StencilAction FailAction;
    public StencilAction ZFailAction;
    public StencilAction PassAction;
    public StencilDrawMode DrawMode;                    // Used to enabled double sided faces. Default is 3 (DRAW_BOTH).
}

/// <summary>
/// Apparently commands for an optimizer instructing it to keep things it would normally discard.
/// Also refers to NiNode objects (through their name) in animation .kf files.
/// </summary>
public class NiStringExtraData : NiExtraData {
    public uint BytesRemaining;                         // The number of bytes left in the record.  Equals the length of the following string + 4.
    public string StringData;                           // The string.
}

/// <summary>
/// List of 0x00-seperated strings, which are names of controlled objects and controller types. Used in .kf files in conjunction with NiControllerSequence.
/// </summary>
public class NiStringPalette : NiObject {
    public StringPalette Palette;                       // A bunch of 0x00 seperated strings.
}

/// <summary>
/// List of strings; for example, a list of all bone names.
/// </summary>
public class NiStringsExtraData : NiExtraData {
    public string[] Data;                               // The strings.
}

/// <summary>
/// Extra data, used to name different animation sequences.
/// </summary>
public class NiTextKeyExtraData : NiExtraData {
    public uint UnknownInt1;                            // Unknown.  Always equals zero in all official files.
    public Key[] TextKeys;                              // List of textual notes and at which time they take effect. Used for designating the start and stop of animations and the triggering of sounds.
}

/// <summary>
/// Represents an effect that uses projected textures such as projected lights (gobos), environment maps, and fog maps.
/// </summary>
public class NiTextureEffect : NiDynamicEffect {
    public Matrix3x3 ModelProjectionMatrix;             // Model projection matrix.  Always identity?
    public Vector3 ModelProjectionTransform;            // Model projection transform.  Always (0,0,0)?
    public TexFilterMode TextureFiltering;              // Texture Filtering mode.
    public ushort MaxAnisotropy;
    public TexClampMode TextureClamping;                // Texture Clamp mode.
    public TextureType TextureType;                     // The type of effect that the texture is used for.
    public CoordGenType CoordinateGenerationType;       // The method that will be used to generate UV coordinates for the texture effect.
    public int? Image;                                  // Image index.
    public int? SourceTexture;                          // Source texture index.
    public byte EnablePlane;                            // Determines whether a clipping plane is used.
    public NiPlane Plane;
    public short PS2L;
    public short PS2K;
    public ushort UnknownShort;                         // Unknown: 0.
}

/// <summary>
/// LEGACY (pre-10.1)
/// </summary>
public class NiTextureModeProperty : NiProperty {
    public uint[] UnknownInts;
    public short UnknownShort;                          // Unknown. Either 210 or 194.
    public short PS2L;                                  // 0?
    public short PS2K;                                  // -75?
}

/// <summary>
/// LEGACY (pre-10.1)
/// </summary>
public class NiImage : NiObject {
    public byte UseExternal;                            // 0 if the texture is internal to the NIF file.
    public ?? FileName;                                 // The filepath to the texture.
    public int? ImageData;                              // Link to the internally stored image data.
    public uint UnknownInt;                             // Unknown.  Often seems to be 7. Perhaps m_uiMipLevels?
    public float UnknownFloat;                          // Unknown.  Perhaps fImageScale?
}

/// <summary>
/// LEGACY (pre-10.1)
/// </summary>
public class NiTextureProperty : NiProperty {
    public uint[] UnknownInts1;                         // Property flags.
    public Flags Flags;                                 // Property flags.
    public int? Image;                                  // Link to the texture image.
    public uint[] UnknownInts2;                         // Unknown.  0?
}

/// <summary>
/// Describes how a fragment shader should be configured for a given piece of geometry.
/// </summary>
public class NiTexturingProperty : NiProperty {
    public Flags Flags;                                 // Property flags.
    public ApplyMode ApplyMode;                         // Determines how the texture will be applied.  Seems to have special functions in Oblivion.
    public uint TextureCount;                           // Number of textures.
    public bool HasBaseTexture;                         // Do we have a base texture?
    public TexDesc BaseTexture;                         // The base texture.
    public bool HasDarkTexture;                         // Do we have a dark texture?
    public TexDesc DarkTexture;                         // The dark texture.
    public bool HasDetailTexture;                       // Do we have a detail texture?
    public TexDesc DetailTexture;                       // The detail texture.
    public bool HasGlossTexture;                        // Do we have a gloss texture?
    public TexDesc GlossTexture;                        // The gloss texture.
    public bool HasGlowTexture;                         // Do we have a glow texture?
    public TexDesc GlowTexture;                         // The glowing texture.
    public bool HasBumpMapTexture;                      // Do we have a bump map texture?
    public TexDesc BumpMapTexture;                      // The bump map texture.
    public float BumpMapLumaScale;
    public float BumpMapLumaOffset;
    public Matrix2x2 BumpMapMatrix;
    public bool HasNormalTexture;                       // Do we have a normal texture?
    public TexDesc NormalTexture;                       // Normal texture.
    public bool HasParallaxTexture;
    public TexDesc ParallaxTexture;
    public float ParallaxOffset;
    public bool HasDecal0Texture;
    public TexDesc Decal0Texture;                       // The decal texture.
    public bool HasDecal1Texture;
    public TexDesc Decal1Texture;                       // Another decal texture.
    public bool HasDecal2Texture;
    public TexDesc Decal2Texture;                       // Another decal texture.
    public bool HasDecal3Texture;
    public TexDesc Decal3Texture;                       // Another decal texture.
    public ShaderTexDesc[] ShaderTextures;              // Shader textures.
}

public class NiMultiTextureProperty : NiTexturingProperty {
}

/// <summary>
/// Wrapper for transformation animation keys.
/// </summary>
public class NiTransformData : NiKeyframeData {
}

/// <summary>
/// A shape node that refers to singular triangle data.
/// </summary>
public class NiTriShape : NiTriBasedGeom {
}

/// <summary>
/// Holds mesh data using a list of singular triangles.
/// </summary>
public class NiTriShapeData : NiTriBasedGeomData {
    public uint NumTrianglePoints;                      // Num Triangles times 3.
    public bool HasTriangles;                           // Do we have triangle data?
    public Triangle[] Triangles;                        // Triangle face data.
    public MatchGroup[] MatchGroups;                    // The shared normals.
}

/// <summary>
/// A shape node that refers to data organized into strips of triangles
/// </summary>
public class NiTriStrips : NiTriBasedGeom {
}

/// <summary>
/// Holds mesh data using strips of triangles.
/// </summary>
public class NiTriStripsData : NiTriBasedGeomData {
    public ushort NumStrips;                            // Number of OpenGL triangle strips that are present.
    public ushort[] StripLengths;                       // The number of points in each triangle strip.
    public bool HasPoints;                              // Do we have strip point data?
    public ushort[][] Points;                           // The points in the Triangle strips. Size is the sum of all entries in Strip Lengths.
}

/// <summary>
/// Unknown
/// </summary>
public class NiEnvMappedTriShape : NiObjectNET {
    public ushort Unknown1;                             // unknown (=4 - 5)
    public Matrix4x4 UnknownMatrix;                     // unknown
    public int?[] Children;                             // List of child node object indices.
    public int? Child2;                                 // unknown
    public int? Child3;                                 // unknown
}

/// <summary>
/// Holds mesh data using a list of singular triangles.
/// </summary>
public class NiEnvMappedTriShapeData : NiTriShapeData {
}

/// <summary>
/// LEGACY (pre-10.1)
/// Sub data of NiBezierMesh
/// </summary>
public class NiBezierTriangle4 : NiObject {
    public uint[] Unknown1;                             // unknown
    public ushort Unknown2;                             // unknown
    public Matrix3x3 Matrix;                            // unknown
    public Vector3 Vector1;                             // unknown
    public Vector3 Vector2;                             // unknown
    public short[] Unknown3;                            // unknown
    public byte Unknown4;                               // unknown
    public uint Unknown5;                               // unknown
    public short[] Unknown6;                            // unknown
}

/// <summary>
/// LEGACY (pre-10.1)
/// Unknown
/// </summary>
public class NiBezierMesh : NiAVObject {
    public int?[] BezierTriangle;                       // unknown
    public uint Unknown3;                               // Unknown.
    public ushort Count1;                               // Data count.
    public ushort Unknown4;                             // Unknown.
    public Vector3[] Points1;                           // data.
    public uint Unknown5;                               // Unknown (illegal link?).
    public float[][] Points2;                           // data.
    public uint Unknown6;                               // unknown
    public ushort Count2;                               // data count 2.
    public ushort[][] Data2;                            // data count.
}

/// <summary>
/// A shape node that holds continuous level of detail information.
/// Seems to be specific to Freedom Force.
/// </summary>
public class NiClod : NiTriBasedGeom {
}

/// <summary>
/// Holds mesh data for continuous level of detail shapes.
/// Pesumably a progressive mesh with triangles specified by edge splits.
/// Seems to be specific to Freedom Force.
/// The structure of this is uncertain and highly experimental at this point.
/// </summary>
public class NiClodData : NiTriBasedGeomData {
    public ushort UnknownShorts;
    public ushort UnknownCount1;
    public ushort UnknownCount2;
    public ushort UnknownCount3;
    public float UnknownFloat;
    public ushort UnknownShort;
    public ushort[][] UnknownClodShorts1;
    public ushort[] UnknownClodShorts2;
    public ushort[][] UnknownClodShorts3;
}

/// <summary>
/// DEPRECATED (pre-10.1), REMOVED (20.3).
/// Time controller for texture coordinates.
/// </summary>
public class NiUVController : NiTimeController {
    public ushort UnknownShort;                         // Always 0?
    public int? Data;                                   // Texture coordinate controller data index.
}

/// <summary>
/// DEPRECATED (pre-10.1), REMOVED (20.3)
/// Texture coordinate data.
/// </summary>
public class NiUVData : NiObject {
    public KeyGroup[] UVGroups;                         // Four UV data groups. Appear to be U translation, V translation, U scaling/tiling, V scaling/tiling.
}

/// <summary>
/// DEPRECATED (20.5).
/// Extra data in the form of a vector (as x, y, z, w components).
/// </summary>
public class NiVectorExtraData : NiExtraData {
    public Vector4 VectorData;                          // The vector data.
}

/// <summary>
/// Property of vertex colors. This object is referred to by the root object of the NIF file whenever some NiTriShapeData object has vertex colors with non-default settings; if not present, vertex colors have vertex_mode=2 and lighting_mode=1.
/// </summary>
public class NiVertexColorProperty : NiProperty {
    public Flags Flags;                                 // Bits 0-2: Unknown
                                                        //     Bit 3: Lighting Mode
                                                        //     Bits 4-5: Vertex Mode
    public VertMode VertexMode;                         // In Flags from 20.1.0.3 on.
    public LightMode LightingMode;                      // In Flags from 20.1.0.3 on.
}

/// <summary>
/// DEPRECATED (10.x), REMOVED (?)
/// Not used in skinning.
/// Unsure of use - perhaps for morphing animation or gravity.
/// </summary>
public class NiVertWeightsExtraData : NiExtraData {
    public uint NumBytes;                               // Number of bytes in this data object.
    public float[] Weight;                              // The vertex weights.
}

/// <summary>
/// DEPRECATED (10.2), REMOVED (?), Replaced by NiBoolData.
/// Visibility data for a controller.
/// </summary>
public class NiVisData : NiObject {
    public Key[] Keys;
}

/// <summary>
/// Allows applications to switch between drawing solid geometry or wireframe outlines.
/// </summary>
public class NiWireframeProperty : NiProperty {
    public Flags Flags;                                 // Property flags.
                                                        //     0 - Wireframe Mode Disabled
                                                        //     1 - Wireframe Mode Enabled
}

/// <summary>
/// Allows applications to set the test and write modes of the renderer's Z-buffer and to set the comparison function used for the Z-buffer test.
/// </summary>
public class NiZBufferProperty : NiProperty {
    public Flags Flags;                                 // Bit 0 enables the z test
                                                        //     Bit 1 controls wether the Z buffer is read only (0) or read/write (1)
    public ZCompareMode Function;                       // Z-Test function (see: glDepthFunc). In Flags from 20.1.0.3 on.
}

/// <summary>
/// Morrowind-specific node for collision mesh.
/// </summary>
public class RootCollisionNode : NiNode {
}

/// <summary>
/// LEGACY (pre-10.1)
/// Raw image data.
/// </summary>
public class NiRawImageData : NiObject {
    public uint Width;                                  // Image width
    public uint Height;                                 // Image height
    public ImageType ImageType;                         // The format of the raw image data.
    public ByteColor3[][] RGBImageData;                 // Image pixel data.
    public ByteColor4[][] RGBAImageData;                // Image pixel data.
}

public abstract class NiAccumulator : NiObject {
}

/// <summary>
/// Used to turn sorting off for individual subtrees in a scene. Useful if objects must be drawn in a fixed order.
/// </summary>
public class NiSortAdjustNode : NiNode {
    public SortingMode SortingMode;                     // Sorting
    public int? Accumulator;
}

/// <summary>
/// Represents cube maps that are created from either a set of six image files, six blocks of pixel data, or a single pixel data with six faces.
/// </summary>
public class NiSourceCubeMap : NiSourceTexture {
}

/// <summary>
/// A PhysX prop which holds information about PhysX actors in a Gamebryo scene
/// </summary>
public class NiPhysXProp : NiObjectNET {
    public float PhysXtoWorldScale;
    public int?[] Sources;
    public int?[] Dests;
    public int?[] ModifiedMeshes;
    public string TempName;
    public bool KeepMeshes;
    public int? PropDescription;
}

public class PhysXMaterialRef(BinaryReader r) {
    public ushort Key = r.ReadUInt16();
    public int? MaterialDesc = X<NiPhysXMaterialDesc>.Ref(r);
}

public class PhysXStateName(BinaryReader r) {
    public string Name = Y.String(r);
    public uint Index = r.ReadUInt32();
}

/// <summary>
/// For serialization of PhysX objects and to attach them to the scene.
/// </summary>
public class NiPhysXPropDesc : NiObject {
    public int?[] Actors;
    public int?[] Joints;
    public int?[] Clothes;
    public PhysXMaterialRef[] Materials;
    public uint NumStates;
    public PhysXStateName[] StateNames;
    public byte Flags;
}

/// <summary>
/// For serializing NxActor objects.
/// </summary>
public class NiPhysXActorDesc : NiObject {
    public string ActorName;
    public Matrix3x4[] Poses;
    public int? BodyDesc;
    public float Density;
    public uint ActorFlags;
    public ushort ActorGroup;
    public ushort DominanceGroup;
    public uint ContactReportFlags;
    public ushort ForceFieldMaterial;
    public uint Dummy;
    public int?[] ShapeDescriptions;
    public int? ActorParent;
    public int? Source;
    public int? Dest;
}

public class PhysXBodyStoredVels(BinaryReader r, Header h) {
    public Vector3 LinearVelocity = r.ReadVector3();
    public Vector3 AngularVelocity = r.ReadVector3();
    public bool Sleep = h.V >= 0x1E020003 ? r.ReadBool32() : default;
}

/// <summary>
/// For serializing NxBodyDesc objects.
/// </summary>
public class NiPhysXBodyDesc : NiObject {
    public Matrix3x4 LocalPose;
    public Vector3 SpaceInertia;
    public float Mass;
    public PhysXBodyStoredVels[] Vels;
    public float WakeUpCounter;
    public float LinearDamping;
    public float AngularDamping;
    public float MaxAngularVelocity;
    public float CCDMotionThreshold;
    public uint Flags;
    public float SleepLinearVelocity;
    public float SleepAngularVelocity;
    public uint SolverIterationCount;
    public float SleepEnergyThreshold;
    public float SleepDamping;
    public float ContactReportThreshold;
}

public enum NxJointType : uint {
    NX_JOINT_PRISMATIC = 0,
    NX_JOINT_REVOLUTE = 1,
    NX_JOINT_CYLINDRICAL = 2,
    NX_JOINT_SPHERICAL = 3,
    NX_JOINT_POINT_ON_LINE = 4,
    NX_JOINT_POINT_IN_PLANE = 5,
    NX_JOINT_DISTANCE = 6,
    NX_JOINT_PULLEY = 7,
    NX_JOINT_FIXED = 8,
    NX_JOINT_D6 = 9
}

public enum NxD6JointMotion : uint {
    NX_D6JOINT_MOTION_LOCKED = 0,
    NX_D6JOINT_MOTION_LIMITED = 1,
    NX_D6JOINT_MOTION_FREE = 2
}

public enum NxD6JointDriveType : uint {
    NX_D6JOINT_DRIVE_POSITION = 1,
    NX_D6JOINT_DRIVE_VELOCITY = 2
}

public enum NxJointProjectionMode : uint {
    NX_JPM_NONE = 0,
    NX_JPM_POINT_MINDIST = 1,
    NX_JPM_LINEAR_MINDIST = 2
}

public class NiPhysXJointActor(BinaryReader r) {
    public int? Actor = X<NiPhysXActorDesc>.Ref(r);
    public Vector3 LocalNormal = r.ReadVector3();
    public Vector3 LocalAxis = r.ReadVector3();
    public Vector3 LocalAnchor = r.ReadVector3();
}

public class NxJointLimitSoftDesc(BinaryReader r) {
    public float Value = r.ReadSingle();
    public float Restitution = r.ReadSingle();
    public float Spring = r.ReadSingle();
    public float Damping = r.ReadSingle();
}

public class NxJointDriveDesc(BinaryReader r) {
    public NxD6JointDriveType DriveType = (NxD6JointDriveType)r.ReadUInt32();
    public float Restitution = r.ReadSingle();
    public float Spring = r.ReadSingle();
    public float Damping = r.ReadSingle();
}

public class NiPhysXJointLimit(BinaryReader r, Header h) {
    public Vector3 LimitPlaneNormal = r.ReadVector3();
    public float LimitPlaneD = r.ReadSingle();
    public float LimitPlaneR = h.V >= 0x14040000 ? r.ReadSingle() : default;
}

/// <summary>
/// A PhysX Joint abstract base class.
/// </summary>
public abstract class NiPhysXJointDesc : NiObject {
    public NxJointType JointType;
    public string JointName;
    public NiPhysXJointActor[] Actors;
    public float MaxForce;
    public float MaxTorque;
    public float SolverExtrapolationFactor;
    public uint UseAccelerationSpring;
    public uint JointFlags;
    public Vector3 LimitPoint;
    public NiPhysXJointLimit[] Limits;
}

/// <summary>
/// A 6DOF (6 degrees of freedom) joint.
/// </summary>
public class NiPhysXD6JointDesc : NiPhysXJointDesc {
    public NxD6JointMotion XMotion;
    public NxD6JointMotion YMotion;
    public NxD6JointMotion ZMotion;
    public NxD6JointMotion Swing1Motion;
    public NxD6JointMotion Swing2Motion;
    public NxD6JointMotion TwistMotion;
    public NxJointLimitSoftDesc LinearLimit;
    public NxJointLimitSoftDesc Swing1Limit;
    public NxJointLimitSoftDesc Swing2Limit;
    public NxJointLimitSoftDesc TwistLowLimit;
    public NxJointLimitSoftDesc TwistHighLimit;
    public NxJointDriveDesc XDrive;
    public NxJointDriveDesc YDrive;
    public NxJointDriveDesc ZDrive;
    public NxJointDriveDesc SwingDrive;
    public NxJointDriveDesc TwistDrive;
    public NxJointDriveDesc SlerpDrive;
    public Vector3 DrivePosition;
    public Quaternion DriveOrientation;
    public Vector3 DriveLinearVelocity;
    public Vector3 DriveAngularVelocity;
    public NxJointProjectionMode ProjectionMode;
    public float ProjectionDistance;
    public float ProjectionAngle;
    public float GearRatio;
    public uint Flags;
}

public enum NxShapeType : uint {
    NX_SHAPE_PLANE = 0,
    NX_SHAPE_SPHERE = 1,
    NX_SHAPE_BOX = 2,
    NX_SHAPE_CAPSULE = 3,
    NX_SHAPE_WHEEL = 4,
    NX_SHAPE_CONVEX = 5,
    NX_SHAPE_MESH = 6,
    NX_SHAPE_HEIGHTFIELD = 7,
    NX_SHAPE_RAW_MESH = 8,
    NX_SHAPE_COMPOUND = 9
}

public class NxPlane(BinaryReader r) {
    public float Val1 = r.ReadSingle();
    public Vector3 Point1 = r.ReadVector3();
}

public class NxCapsule(BinaryReader r) {
    public float Val1 = r.ReadSingle();
    public float Val2 = r.ReadSingle();
    public uint CapsuleFlags = r.ReadUInt32();
}

/// <summary>
/// For serializing NxShapeDesc objects
/// </summary>
public class NiPhysXShapeDesc : NiObject {
    public NxShapeType ShapeType;
    public Matrix3x4 LocalPose;
    public uint ShapeFlags;
    public ushort CollisionGroup;
    public ushort MaterialIndex;
    public float Density;
    public float Mass;
    public float SkinWidth;
    public string ShapeName;
    public uint Non-InteractingCompartmentTypes;
    public uint[] CollisionBits;
    public NxPlane Plane;
    public float SphereRadius;
    public Vector3 BoxHalfExtents;
    public NxCapsule Capsule;
    public int? Mesh;
}

/// <summary>
/// Holds mesh data for streaming.
/// </summary>
public class NiPhysXMeshDesc : NiObject {
    public bool IsConvex;
    public string MeshName;
    public ushort[] MeshData;
    public ushort MeshSize;
    public uint MeshFlags;
    public uint MeshPagingMode;
    public bool IsHardware;
    public byte Flags;
}

[Flags]
public enum NxMaterialFlag : uint {
    NX_MF_ANISOTROPIC = 1 << 1,
    NX_MF_DUMMY1 = 1 << 2,
    NX_MF_DUMMY2 = 1 << 3,
    NX_MF_DUMMY3 = 1 << 4,
    NX_MF_DISABLE_FRICTION = 1 << 5,
    NX_MF_DISABLE_STRONG_FRICTION = 1 << 6
}

public class NxSpringDesc(BinaryReader r) {
    public float Spring = r.ReadSingle();
    public float Damper = r.ReadSingle();
    public float TargetValue = r.ReadSingle();
}

public enum NxCombineMode : uint {
    NX_CM_AVERAGE = 0,
    NX_CM_MIN = 1,
    NX_CM_MULTIPLY = 2,
    NX_CM_MAX = 3
}

public class NxMaterialDesc(BinaryReader r, Header h) {
    public float DynamicFriction = r.ReadSingle();
    public float StaticFriction = r.ReadSingle();
    public float Restitution = r.ReadSingle();
    public float DynamicFrictionV = r.ReadSingle();
    public float StaticFrictionV = r.ReadSingle();
    public Vector3 DirectionofAnisotropy = r.ReadVector3();
    public NxMaterialFlag Flags = (NxMaterialFlag)r.ReadUInt32();
    public NxCombineMode FrictionCombineMode = (NxCombineMode)r.ReadUInt32();
    public NxCombineMode RestitutionCombineMode = (NxCombineMode)r.ReadUInt32();
    public bool HasSpring = h.V <= 0x14020300 ? r.ReadBool32() : default;
    public NxSpringDesc Spring = Has Spring && h.V <= 0x14020300 ? new NxSpringDesc(r) : default;
}

/// <summary>
/// For serializing NxMaterialDesc objects.
/// </summary>
public class NiPhysXMaterialDesc : NiObject {
    public ushort Index;
    public NxMaterialDesc[] MaterialDescs;
}

/// <summary>
/// A destination is a link between a PhysX actor and a Gamebryo object being driven by the physics.
/// </summary>
public abstract class NiPhysXDest : NiObject {
    public bool Active;
    public bool Interpolate;
}

/// <summary>
/// Base for destinations that set a rigid body state.
/// </summary>
public abstract class NiPhysXRigidBodyDest : NiPhysXDest {
}

/// <summary>
/// Connects PhysX rigid body actors to a scene node.
/// </summary>
public class NiPhysXTransformDest : NiPhysXRigidBodyDest {
    public int? Target;
}

/// <summary>
/// A source is a link between a Gamebryo object and a PhysX actor.
/// </summary>
public abstract class NiPhysXSrc : NiObject {
    public bool Active;
    public bool Interpolate;
}

/// <summary>
/// Sets state of a rigid body PhysX actor.
/// </summary>
public abstract class NiPhysXRigidBodySrc : NiPhysXSrc {
    public int? Source;
}

/// <summary>
/// Sets state of kinematic PhysX actor.
/// </summary>
public class NiPhysXKinematicSrc : NiPhysXRigidBodySrc {
}

/// <summary>
/// Sends Gamebryo scene state to a PhysX dynamic actor.
/// </summary>
public class NiPhysXDynamicSrc : NiPhysXRigidBodySrc {
}

/// <summary>
/// Wireframe geometry.
/// </summary>
public class NiLines : NiTriBasedGeom {
}

/// <summary>
/// Wireframe geometry data.
/// </summary>
public class NiLinesData : NiGeometryData {
    public bool[] Lines;                                // Is vertex connected to other (next?) vertex?
}

/// <summary>
/// Two dimensional screen elements.
/// </summary>
public class Polygon(BinaryReader r) {
    public ushort NumVertices = r.ReadUInt16();
    public ushort VertexOffset = r.ReadUInt16();        // Offset in vertex array.
    public ushort NumTriangles = r.ReadUInt16();
    public ushort TriangleOffset = r.ReadUInt16();      // Offset in indices array.
}

/// <summary>
/// DEPRECATED (20.5), functionality included in NiMeshScreenElements.
/// Two dimensional screen elements.
/// </summary>
public class NiScreenElementsData : NiTriShapeData {
    public ushort MaxPolygons;
    public Polygon[] Polygons;
    public ushort[] PolygonIndices;
    public ushort PolygonGrowBy;
    public ushort NumPolygons;
    public ushort MaxVertices;
    public ushort VerticesGrowBy;
    public ushort MaxIndices;
    public ushort IndicesGrowBy;
}

/// <summary>
/// DEPRECATED (20.5), replaced by NiMeshScreenElements.
/// Two dimensional screen elements.
/// </summary>
public class NiScreenElements : NiTriShape {
}

/// <summary>
/// NiRoomGroup represents a set of connected rooms i.e. a game level.
/// </summary>
public class NiRoomGroup : NiNode {
    public int? Shell;                                  // Object that represents the room group as seen from the outside.
    public int?[] Rooms;
}

/// <summary>
/// NiRoom objects represent cells in a cell-portal culling system.
/// </summary>
public class NiRoom : NiNode {
    public NiPlane[] WallPlanes;
    public int?[] InPortals;                            // The portals which see into the room.
    public int?[] OutPortals;                           // The portals which see out of the room.
    public int?[] Fixtures;                             // All geometry associated with the room.
}

/// <summary>
/// NiPortal objects are grouping nodes that support aggressive visibility culling.
/// They represent flat polygonal regions through which a part of a scene graph can be viewed.
/// </summary>
public class NiPortal : NiAVObject {
    public ushort PortalFlags;
    public ushort PlaneCount;                           // Unused in 20.x, possibly also 10.x.
    public Vector3[] Vertices;
    public int? Adjoiner;                               // Root of the scenegraph which is to be seen through this portal.
}

/// <summary>
/// Bethesda-specific fade node.
/// </summary>
public class BSFadeNode : NiNode {
}

/// <summary>
/// The type of animation interpolation (blending) that will be used on the associated key frames.
/// </summary>
public enum BSShaderType : uint {
    SHADER_TALL_GRASS = 0,          // Tall Grass Shader
    SHADER_DEFAULT = 1,             // Standard Lighting Shader
    SHADER_SKY = 10,                // Sky Shader
    SHADER_SKIN = 14,               // Skin Shader
    SHADER_WATER = 17,              // Water Shader
    SHADER_LIGHTING30 = 29,         // Lighting 3.0 Shader
    SHADER_TILE = 32,               // Tiled Shader
    SHADER_NOLIGHTING = 33          // No Lighting Shader
}

/// <summary>
/// Shader Property Flags
/// </summary>
[Flags]
public enum BSShaderFlags : uint {
    Specular = 0,                   // Enables Specularity
    Skinned = 1 << 1,               // Required For Skinned Meshes
    LowDetail = 1 << 2,             // Lowddetail (seems to use standard diff/norm/spec shader)
    Vertex_Alpha = 1 << 3,          // Vertex Alpha
    Unknown_1 = 1 << 4,             // Unknown
    Single_Pass = 1 << 5,           // Single Pass
    Empty = 1 << 6,                 // Unknown
    Environment_Mapping = 1 << 7,   // Environment mapping (uses Envmap Scale)
    Alpha_Texture = 1 << 8,         // Alpha Texture Requires NiAlphaProperty to Enable
    Unknown_2 = 1 << 9,             // Unknown
    FaceGen = 1 << 10,              // FaceGen
    Parallax_Shader_Index_15 = 1 << 11, // Parallax
    Unknown_3 = 1 << 12,            // Unknown/Crash
    Non_Projective_Shadows = 1 << 13, // Non-Projective Shadows
    Unknown_4 = 1 << 14,            // Unknown/Crash
    Refraction = 1 << 15,           // Refraction (switches on refraction power)
    Fire_Refraction = 1 << 16,      // Fire Refraction (switches on refraction power/period)
    Eye_Environment_Mapping = 1 << 17, // Eye Environment Mapping (does not use envmap light fade or envmap scale)
    Hair = 1 << 18,                 // Hair
    Dynamic_Alpha = 1 << 19,        // Dynamic Alpha
    Localmap_Hide_Secret = 1 << 20, // Localmap Hide Secret
    Window_Environment_Mapping = 1 << 21, // Window Environment Mapping
    Tree_Billboard = 1 << 22,       // Tree Billboard
    Shadow_Frustum = 1 << 23,       // Shadow Frustum
    Multiple_Textures = 1 << 24,    // Multiple Textures (base diff/norm become null)
    Remappable_Textures = 1 << 25,  // usually seen w/texture animation
    Decal_Single_Pass = 1 << 26,    // Decal
    Dynamic_Decal_Single_Pass = 1 << 27, // Dynamic Decal
    Parallax_Occulsion = 1 << 28,   // Parallax Occlusion
    External_Emittance = 1 << 29,   // External Emittance
    Shadow_Map = 1 << 30,           // Shadow Map
    ZBuffer_Test = 1 << 31          // ZBuffer Test (1=on)
}

/// <summary>
/// Shader Property Flags 2
/// </summary>
[Flags]
public enum BSShaderFlags2 : uint {
    ZBuffer_Write = 0,              // ZBuffer Write
    LOD_Landscape = 1 << 1,         // LOD Landscape
    LOD_Building = 1 << 2,          // LOD Building
    No_Fade = 1 << 3,               // No Fade
    Refraction_Tint = 1 << 4,       // Refraction Tint
    Vertex_Colors = 1 << 5,         // Has Vertex Colors
    Unknown1 = 1 << 6,              // Unknown
    1st_Light_is_Point_Light = 1 << 7, // 1st Light is Point Light
    2nd_Light = 1 << 8,             // 2nd Light
    3rd_Light = 1 << 9,             // 3rd Light
    Vertex_Lighting = 1 << 10,      // Vertex Lighting
    Uniform_Scale = 1 << 11,        // Uniform Scale
    Fit_Slope = 1 << 12,            // Fit Slope
    Billboard_and_Envmap_Light_Fade = 1 << 13, // Billboard and Envmap Light Fade
    No_LOD_Land_Blend = 1 << 14,    // No LOD Land Blend
    Envmap_Light_Fade = 1 << 15,    // Envmap Light Fade
    Wireframe = 1 << 16,            // Wireframe
    VATS_Selection = 1 << 17,       // VATS Selection
    Show_in_Local_Map = 1 << 18,    // Show in Local Map
    Premult_Alpha = 1 << 19,        // Premult Alpha
    Skip_Normal_Maps = 1 << 20,     // Skip Normal Maps
    Alpha_Decal = 1 << 21,          // Alpha Decal
    No_Transparecny_Multisampling = 1 << 22, // No Transparency MultiSampling
    Unknown2 = 1 << 23,             // Unknown
    Unknown3 = 1 << 24,             // Unknown
    Unknown4 = 1 << 25,             // Unknown
    Unknown5 = 1 << 26,             // Unknown
    Unknown6 = 1 << 27,             // Unknown
    Unknown7 = 1 << 28,             // Unknown
    Unknown8 = 1 << 29,             // Unknown
    Unknown9 = 1 << 30,             // Unknown
    Unknown10 = 1 << 31             // Unknown
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class BSShaderProperty : NiShadeProperty {
    public BSShaderType ShaderType;
    public BSShaderFlags ShaderFlags;
    public BSShaderFlags2 ShaderFlags2;
    public float EnvironmentMapScale;                   // Scales the intensity of the environment/cube map.
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public abstract class BSShaderLightingProperty : BSShaderProperty {
    public TexClampMode TextureClampMode;               // How to handle texture borders.
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class BSShaderNoLightingProperty : BSShaderLightingProperty {
    public string FileName;                             // The texture glow map.
    public float FalloffStartAngle;                     // At this cosine of angle falloff will be equal to Falloff Start Opacity
    public float FalloffStopAngle;                      // At this cosine of angle falloff will be equal to Falloff Stop Opacity
    public float FalloffStartOpacity;                   // Alpha falloff multiplier at start angle
    public float FalloffStopOpacity;                    // Alpha falloff multiplier at end angle
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class BSShaderPPLightingProperty : BSShaderLightingProperty {
    public int? TextureSet;                             // Texture Set
    public float RefractionStrength;                    // The amount of distortion. **Not based on physically accurate refractive index** (0=none) (0-1)
    public int RefractionFirePeriod;                    // Rate of texture movement for refraction shader.
    public float ParallaxMaxPasses;                     // The number of passes the parallax shader can apply.
    public float ParallaxScale;                         // The strength of the parallax.
    public Color4 EmissiveColor;                        // Glow color and alpha
}

/// <summary>
/// This controller is used to animate float variables in BSEffectShaderProperty.
/// </summary>
public class BSEffectShaderPropertyFloatController : NiFloatInterpController {
    public EffectShaderControlledVariable TypeofControlledVariable; // Which float variable in BSEffectShaderProperty to animate:
}

/// <summary>
/// This controller is used to animate colors in BSEffectShaderProperty.
/// </summary>
public class BSEffectShaderPropertyColorController : NiPoint3InterpController {
    public EffectShaderControlledColor TypeofControlledColor; // Which color in BSEffectShaderProperty to animate:
}

/// <summary>
/// This controller is used to animate float variables in BSLightingShaderProperty.
/// </summary>
public class BSLightingShaderPropertyFloatController : NiFloatInterpController {
    public LightingShaderControlledVariable TypeofControlledVariable; // Which float variable in BSLightingShaderProperty to animate:
}

/// <summary>
/// This controller is used to animate colors in BSLightingShaderProperty.
/// </summary>
public class BSLightingShaderPropertyColorController : NiPoint3InterpController {
    public LightingShaderControlledColor TypeofControlledColor; // Which color in BSLightingShaderProperty to animate:
}

public class BSNiAlphaPropertyTestRefController : NiFloatInterpController {
}

/// <summary>
/// Skyrim, Paired with dummy TriShapes, this controller generates lightning shapes for special effects.
///     First interpolator controls Generation.
/// </summary>
public class BSProceduralLightningController : NiTimeController {
    public int? Interpolator1:Generation;               // References generation interpolator.
    public int? Interpolator2:Mutation;                 // References interpolator for Mutation of strips
    public int? Interpolator3:Subdivision;              // References subdivision interpolator.
    public int? Interpolator4:NumBranches;              // References branches interpolator.
    public int? Interpolator5:NumBranchesVar;           // References branches variation interpolator.
    public int? Interpolator6:Length;                   // References length interpolator.
    public int? Interpolator7:LengthVar;                // References length variation interpolator.
    public int? Interpolator8:Width;                    // References width interpolator.
    public int? Interpolator9:ArcOffset;                // References interpolator for amplitude control. 0=straight, 50=wide
    public ushort Subdivisions;
    public ushort NumBranches;
    public ushort NumBranchesVariation;
    public float Length;                                // How far lightning will stretch to.
    public float LengthVariation;                       // How far lightning variation will stretch to.
    public float Width;                                 // How wide the bolt will be.
    public float ChildWidthMult;                        // Influences forking behavior with a multiplier.
    public float ArcOffset;
    public bool FadeMainBolt;
    public bool FadeChildBolts;
    public bool AnimateArcOffset;
    public int? ShaderProperty;                         // Reference to a shader property.
}

/// <summary>
/// Bethesda-specific Texture Set.
/// </summary>
public class BSShaderTextureSet : NiObject {
    public string[] Textures;                           // Textures.
                                                        //     0: Diffuse
                                                        //     1: Normal/Gloss
                                                        //     2: Glow(SLSF2_Glow_Map)/Skin/Hair/Rim light(SLSF2_Rim_Lighting)
                                                        //     3: Height/Parallax
                                                        //     4: Environment
                                                        //     5: Environment Mask
                                                        //     6: Subsurface for Multilayer Parallax
                                                        //     7: Back Lighting Map (SLSF2_Back_Lighting)
}

/// <summary>
/// Bethesda-specific property. Found in Fallout3
/// </summary>
public class WaterShaderProperty : BSShaderProperty {
}

/// <summary>
/// Sets what sky function this object fulfills in BSSkyShaderProperty or SkyShaderProperty.
/// </summary>
public enum SkyObjectType : uint {
    BSSM_SKY_TEXTURE = 0,           // BSSM_Sky_Texture
    BSSM_SKY_SUNGLARE = 1,          // BSSM_Sky_Sunglare
    BSSM_SKY = 2,                   // BSSM_Sky
    BSSM_SKY_CLOUDS = 3,            // BSSM_Sky_Clouds
    BSSM_SKY_STARS = 5,             // BSSM_Sky_Stars
    BSSM_SKY_MOON_STARS_MASK = 7    // BSSM_Sky_Moon_Stars_Mask
}

/// <summary>
/// Bethesda-specific property. Found in Fallout3
/// </summary>
public class SkyShaderProperty : BSShaderLightingProperty {
    public string FileName;                             // The texture.
    public SkyObjectType SkyObjectType;                 // Sky Object Type
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class TileShaderProperty : BSShaderLightingProperty {
    public string FileName;                             // Texture file name
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class DistantLODShaderProperty : BSShaderProperty {
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class BSDistantTreeShaderProperty : BSShaderProperty {
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class TallGrassShaderProperty : BSShaderProperty {
    public string FileName;                             // Texture file name
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class VolumetricFogShaderProperty : BSShaderProperty {
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class HairShaderProperty : BSShaderProperty {
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class Lighting30ShaderProperty : BSShaderPPLightingProperty {
}

/// <summary>
/// Skyrim Shader Property Flags 1
/// </summary>
[Flags]
public enum SkyrimShaderPropertyFlags1 : uint {
    Specular = 0,                   // Enables Specularity
    Skinned = 1 << 1,               // Required For Skinned Meshes.
    Temp_Refraction = 1 << 2,
    Vertex_Alpha = 1 << 3,          // Enables using alpha component of vertex colors.
    Greyscale_To_PaletteColor = 1 << 4, // in EffectShaderProperty
    Greyscale_To_PaletteAlpha = 1 << 5, // in EffectShaderProperty
    Use_Falloff = 1 << 6,           // Use Falloff value in EffectShaderProperty
    Environment_Mapping = 1 << 7,   // Environment mapping (uses Envmap Scale).
    Recieve_Shadows = 1 << 8,       // Object can recieve shadows.
    Cast_Shadows = 1 << 9,          // Can cast shadows
    Facegen_Detail_Map = 1 << 10,   // Use a face detail map in the 4th texture slot.
    Parallax = 1 << 11,             // Unused?
    Model_Space_Normals = 1 << 12,  // Use Model space normals and an external Specular Map.
    Non_Projective_Shadows = 1 << 13,
    Landscape = 1 << 14,
    Refraction = 1 << 15,           // Use normal map for refraction effect.
    Fire_Refraction = 1 << 16,
    Eye_Environment_Mapping = 1 << 17, // Eye Environment Mapping (Must use the Eye shader and the model must be skinned)
    Hair_Soft_Lighting = 1 << 18,   // Keeps from going too bright under lights (hair shader only)
    Screendoor_Alpha_Fade = 1 << 19,
    Localmap_Hide_Secret = 1 << 20, // Object and anything it is positioned above will not render on local map view.
    FaceGen_RGB_Tint = 1 << 21,     // Use tintmask for Face.
    Own_Emit = 1 << 22,             // Provides its own emittance color. (will not absorb light/ambient color?)
    Projected_UV = 1 << 23,         // Used for decalling?
    Multiple_Textures = 1 << 24,
    Remappable_Textures = 1 << 25,
    Decal = 1 << 26,
    Dynamic_Decal = 1 << 27,
    Parallax_Occlusion = 1 << 28,
    External_Emittance = 1 << 29,
    Soft_Effect = 1 << 30,
    ZBuffer_Test = 1 << 31          // ZBuffer Test (1=on)
}

/// <summary>
/// Skyrim Shader Property Flags 2
/// </summary>
[Flags]
public enum SkyrimShaderPropertyFlags2 : uint {
    ZBuffer_Write = 0,              // Enables writing to the Z-Buffer
    LOD_Landscape = 1 << 1,
    LOD_Objects = 1 << 2,
    No_Fade = 1 << 3,
    Double_Sided = 1 << 4,          // Double-sided rendering.
    Vertex_Colors = 1 << 5,         // Has Vertex Colors.
    Glow_Map = 1 << 6,              // Use Glow Map in the third texture slot.
    Assume_Shadowmask = 1 << 7,
    Packed_Tangent = 1 << 8,
    Multi_Index_Snow = 1 << 9,
    Vertex_Lighting = 1 << 10,
    Uniform_Scale = 1 << 11,
    Fit_Slope = 1 << 12,
    Billboard = 1 << 13,
    No_LOD_Land_Blend = 1 << 14,
    EnvMap_Light_Fade = 1 << 15,
    Wireframe = 1 << 16,            // Wireframe (Seems to only work on particles)
    Weapon_Blood = 1 << 17,         // Used for blood decals on weapons.
    Hide_On_Local_Map = 1 << 18,    // Similar to hide secret, but only for self?
    Premult_Alpha = 1 << 19,        // Has Premultiplied Alpha
    Cloud_LOD = 1 << 20,
    Anisotropic_Lighting = 1 << 21, // Hair only?
    No_Transparency_Multisampling = 1 << 22,
    Unused01 = 1 << 23,             // Unused?
    Multi_Layer_Parallax = 1 << 24, // Use Multilayer (inner-layer) Map
    Soft_Lighting = 1 << 25,        // Use Soft Lighting Map
    Rim_Lighting = 1 << 26,         // Use Rim Lighting Map
    Back_Lighting = 1 << 27,        // Use Back Lighting Map
    Unused02 = 1 << 28,             // Unused?
    Tree_Anim = 1 << 29,            // Enables Vertex Animation, Flutter Animation
    Effect_Lighting = 1 << 30,
    HD_LOD_Objects = 1 << 31
}

/// <summary>
/// Fallout 4 Shader Property Flags 1
/// </summary>
[Flags]
public enum Fallout4ShaderPropertyFlags1 : uint {
    Specular = 0,
    Skinned = 1 << 1,
    Temp_Refraction = 1 << 2,
    Vertex_Alpha = 1 << 3,
    GreyscaleToPalette_Color = 1 << 4,
    GreyscaleToPalette_Alpha = 1 << 5,
    Use_Falloff = 1 << 6,
    Environment_Mapping = 1 << 7,
    RGB_Falloff = 1 << 8,
    Cast_Shadows = 1 << 9,
    Face = 1 << 10,
    UI_Mask_Rects = 1 << 11,
    Model_Space_Normals = 1 << 12,
    Non_Projective_Shadows = 1 << 13,
    Landscape = 1 << 14,
    Refraction = 1 << 15,
    Fire_Refraction = 1 << 16,
    Eye_Environment_Mapping = 1 << 17,
    Hair = 1 << 18,
    Screendoor_Alpha_Fade = 1 << 19,
    Localmap_Hide_Secret = 1 << 20,
    Skin_Tint = 1 << 21,
    Own_Emit = 1 << 22,
    Projected_UV = 1 << 23,
    Multiple_Textures = 1 << 24,
    Tessellate = 1 << 25,
    Decal = 1 << 26,
    Dynamic_Decal = 1 << 27,
    Character_Lighting = 1 << 28,
    External_Emittance = 1 << 29,
    Soft_Effect = 1 << 30,
    ZBuffer_Test = 1 << 31
}

/// <summary>
/// Fallout 4 Shader Property Flags 2
/// </summary>
[Flags]
public enum Fallout4ShaderPropertyFlags2 : uint {
    ZBuffer_Write = 0,
    LOD_Landscape = 1 << 1,
    LOD_Objects = 1 << 2,
    No_Fade = 1 << 3,
    Double_Sided = 1 << 4,
    Vertex_Colors = 1 << 5,
    Glow_Map = 1 << 6,
    Transform_Changed = 1 << 7,
    Dismemberment_Meatcuff = 1 << 8,
    Tint = 1 << 9,
    Grass_Vertex_Lighting = 1 << 10,
    Grass_Uniform_Scale = 1 << 11,
    Grass_Fit_Slope = 1 << 12,
    Grass_Billboard = 1 << 13,
    No_LOD_Land_Blend = 1 << 14,
    Dismemberment = 1 << 15,
    Wireframe = 1 << 16,
    Weapon_Blood = 1 << 17,
    Hide_On_Local_Map = 1 << 18,
    Premult_Alpha = 1 << 19,
    VATS_Target = 1 << 20,
    Anisotropic_Lighting = 1 << 21,
    Skew_Specular_Alpha = 1 << 22,
    Menu_Screen = 1 << 23,
    Multi_Layer_Parallax = 1 << 24,
    Alpha_Test = 1 << 25,
    Gradient_Remap = 1 << 26,
    VATS_Target_Draw_All = 1 << 27,
    Pipboy_Screen = 1 << 28,
    Tree_Anim = 1 << 29,
    Effect_Lighting = 1 << 30,
    Refraction_Writes_Depth = 1 << 31
}

/// <summary>
/// Bethesda shader property for Skyrim and later.
/// </summary>
public class BSLightingShaderProperty : BSShaderProperty {
    public Fallout4ShaderPropertyFlags1 ShaderFlags1;   // Fallout 4 Shader Flags. Mostly overridden if "Name" is a path to a BGSM/BGEM file.
    public Fallout4ShaderPropertyFlags2 ShaderFlags2;   // Fallout 4 Shader Flags. Mostly overridden if "Name" is a path to a BGSM/BGEM file.
    public TexCoord UVOffset;                           // Offset UVs
    public TexCoord UVScale;                            // Offset UV Scale to repeat tiling textures, see above.
    public int? TextureSet;                             // Texture Set, can have override in an esm/esp
    public Color3 EmissiveColor;                        // Glow color and alpha
    public float EmissiveMultiple;                      // Multiplied emissive colors
    public string WetMaterial;
    public TexClampMode TextureClampMode;               // How to handle texture borders.
    public float Alpha;                                 // The material opacity (1=non-transparent).
    public float RefractionStrength;                    // The amount of distortion. **Not based on physically accurate refractive index** (0=none) (0-1)
    public float Glossiness;                            // The material specular power, or glossiness (0-999).
    public float Smoothness;                            // The base roughness (0.0-1.0), multiplied by the smoothness map.
    public Color3 SpecularColor;                        // Adds a colored highlight.
    public float SpecularStrength;                      // Brightness of specular highlight. (0=not visible) (0-999)
    public float LightingEffect1;                       // Controls strength for envmap/backlight/rim/softlight lighting effect?
    public float LightingEffect2;                       // Controls strength for envmap/backlight/rim/softlight lighting effect?
    public float SubsurfaceRolloff;
    public float RimlightPower;
    public float BacklightPower;
    public float GrayscaletoPaletteScale;
    public float FresnelPower;
    public float WetnessSpecScale;
    public float WetnessSpecPower;
    public float WetnessMinVar;
    public float WetnessEnvMapScale;
    public float WetnessFresnelPower;
    public float WetnessMetalness;
    public float EnvironmentMapScale;                   // Scales the intensity of the environment/cube map. (0-1)
    public ushort UnknownEnvMapShort;
    public Color3 SkinTintColor;                        // Tints the base texture. Overridden by game settings.
    public uint UnknownSkinTintInt;
    public Color3 HairTintColor;                        // Tints the base texture. Overridden by game settings.
    public float MaxPasses;                             // Max Passes
    public float Scale;                                 // Scale
    public float ParallaxInnerLayerThickness;           // How far from the surface the inner layer appears to be.
    public float ParallaxRefractionScale;               // Depth of inner parallax layer effect.
    public TexCoord ParallaxInnerLayerTextureScale;     // Scales the inner parallax layer texture.
    public float ParallaxEnvmapStrength;                // How strong the environment/cube map is. (0-??)
    public Vector4 SparkleParameters;                   // CK lists "snow material" when used.
    public float EyeCubemapScale;                       // Eye cubemap scale
    public Vector3 LeftEyeReflectionCenter;             // Offset to set center for left eye cubemap
    public Vector3 RightEyeReflectionCenter;            // Offset to set center for right eye cubemap
}

/// <summary>
/// Bethesda effect shader property for Skyrim and later.
/// </summary>
public class BSEffectShaderProperty : BSShaderProperty {
    public Fallout4ShaderPropertyFlags1 ShaderFlags1;
    public Fallout4ShaderPropertyFlags2 ShaderFlags2;
    public TexCoord UVOffset;                           // Offset UVs
    public TexCoord UVScale;                            // Offset UV Scale to repeat tiling textures
    public string SourceTexture;                        // points to an external texture.
    public byte TextureClampMode;                       // How to handle texture borders.
    public byte LightingInfluence;
    public byte EnvMapMinLOD;
    public byte UnknownByte;
    public float FalloffStartAngle;                     // At this cosine of angle falloff will be equal to Falloff Start Opacity
    public float FalloffStopAngle;                      // At this cosine of angle falloff will be equal to Falloff Stop Opacity
    public float FalloffStartOpacity;                   // Alpha falloff multiplier at start angle
    public float FalloffStopOpacity;                    // Alpha falloff multiplier at end angle
    public Color4 EmissiveColor;                        // Emissive color
    public float EmissiveMultiple;                      // Multiplier for Emissive Color (RGB part)
    public float SoftFalloffDepth;
    public string GreyscaleTexture;                     // Points to an external texture, used as palette for SLSF1_Greyscale_To_PaletteColor/SLSF1_Greyscale_To_PaletteAlpha.
    public string EnvMapTexture;
    public string NormalTexture;
    public string EnvMaskTexture;
    public float EnvironmentMapScale;
}

/// <summary>
/// Skyrim water shader property flags
/// </summary>
[Flags]
public enum SkyrimWaterShaderFlags : byte {
    SWSF1_UNKNOWN0 = 0,             // Unknown
    SWSF1_Bypass_Refraction_Map = 1 << 1, // Bypasses refraction map when set to 1
    SWSF1_Water_Toggle = 1 << 2,    // Main water Layer on/off
    SWSF1_UNKNOWN3 = 1 << 3,        // Unknown
    SWSF1_UNKNOWN4 = 1 << 4,        // Unknown
    SWSF1_UNKNOWN5 = 1 << 5,        // Unknown
    SWSF1_Highlight_Layer_Toggle = 1 << 6, // Reflection layer 2 on/off. (is this scene reflection?)
    SWSF1_Enabled = 1 << 7          // Water layer on/off
}

/// <summary>
/// Skyrim water shader property, different from "WaterShaderProperty" seen in Fallout.
/// </summary>
public class BSWaterShaderProperty : BSShaderProperty {
    public SkyrimShaderPropertyFlags1 ShaderFlags1;
    public SkyrimShaderPropertyFlags2 ShaderFlags2;
    public TexCoord UVOffset;                           // Offset UVs. Seems to be unused, but it fits with the other Skyrim shader properties.
    public TexCoord UVScale;                            // Offset UV Scale to repeat tiling textures, see above.
    public SkyrimWaterShaderFlags WaterShaderFlags;     // Defines attributes for the water shader (will use SkyrimWaterShaderFlags)
    public byte WaterDirection;                         // A bitflag, only the first/second bit controls water flow positive or negative along UVs.
    public ushort UnknownShort3;                        // Unknown, flag?
}

/// <summary>
/// Skyrim Sky shader block.
/// </summary>
public class BSSkyShaderProperty : BSShaderProperty {
    public SkyrimShaderPropertyFlags1 ShaderFlags1;
    public SkyrimShaderPropertyFlags2 ShaderFlags2;
    public TexCoord UVOffset;                           // Offset UVs. Seems to be unused, but it fits with the other Skyrim shader properties.
    public TexCoord UVScale;                            // Offset UV Scale to repeat tiling textures, see above.
    public string SourceTexture;                        // points to an external texture.
    public SkyObjectType SkyObjectType;
}

/// <summary>
/// Bethesda-specific skin instance.
/// </summary>
public class BSDismemberSkinInstance : NiSkinInstance {
    public BodyPartList[] Partitions;
}

/// <summary>
/// Bethesda-specific extra data. Lists locations and normals on a mesh that are appropriate for decal placement.
/// </summary>
public class BSDecalPlacementVectorExtraData : NiFloatExtraData {
    public DecalVectorArray[] VectorBlocks;
}

/// <summary>
/// Bethesda-specific particle modifier.
/// </summary>
public class BSPSysSimpleColorModifier : NiPSysModifier {
    public float FadeInPercent;
    public float FadeoutPercent;
    public float Color1EndPercent;
    public float Color1StartPercent;
    public float Color2EndPercent;
    public float Color2StartPercent;
    public Color4[] Colors;
}

/// <summary>
/// Flags for BSValueNode.
/// </summary>
[Flags]
public enum BSValueNodeFlags : byte {
    BillboardWorldZ = 0,
    UsePlayerAdjust = 1 << 1
}

/// <summary>
/// Bethesda-specific node. Found on fxFire effects
/// </summary>
public class BSValueNode : NiNode {
    public uint Value;
    public BSValueNodeFlags ValueNodeFlags;
}

/// <summary>
/// Bethesda-Specific (mesh?) Particle System.
/// </summary>
public class BSStripParticleSystem : NiParticleSystem {
}

/// <summary>
/// Bethesda-Specific (mesh?) Particle System Data.
/// </summary>
public class BSStripPSysData : NiPSysData {
    public ushort MaxPointCount;
    public float StartCapSize;
    public float EndCapSize;
    public bool DoZPrepass;
}

/// <summary>
/// Bethesda-Specific (mesh?) Particle System Modifier.
/// </summary>
public class BSPSysStripUpdateModifier : NiPSysModifier {
    public float UpdateDeltaTime;
}

/// <summary>
/// Bethesda-Specific time controller.
/// </summary>
public class BSMaterialEmittanceMultController : NiFloatInterpController {
}

/// <summary>
/// Bethesda-Specific particle system.
/// </summary>
public class BSMasterParticleSystem : NiNode {
    public ushort MaxEmitterObjects;
    public int?[] ParticleSystems;
}

/// <summary>
/// Particle system (multi?) emitter controller.
/// </summary>
public class BSPSysMultiTargetEmitterCtlr : NiPSysEmitterCtlr {
    public ushort MaxEmitters;
    public int? MasterParticleSystem;
}

/// <summary>
/// Bethesda-Specific time controller.
/// </summary>
public class BSRefractionStrengthController : NiFloatInterpController {
}

/// <summary>
/// Bethesda-Specific node.
/// </summary>
public class BSOrderedNode : NiNode {
    public Vector4 AlphaSortBound;
    public bool StaticBound;
}

/// <summary>
/// Bethesda-Specific node.
/// </summary>
public class BSRangeNode : NiNode {
    public byte Min;
    public byte Max;
    public byte Current;
}

/// <summary>
/// Bethesda-Specific node.
/// </summary>
public class BSBlastNode : BSRangeNode {
}

/// <summary>
/// Bethesda-Specific node.
/// </summary>
public class BSDamageStage : BSBlastNode {
}

/// <summary>
/// Bethesda-specific time controller.
/// </summary>
public class BSRefractionFirePeriodController : NiTimeController {
    public int? Interpolator;
}

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
public class bhkConvexListShape : bhkShape {
    public int?[] SubShapes;                            // List of shapes.
    public HavokMaterial Material;                      // The material of the shape.
    public float Radius;
    public uint UnknownInt1;
    public float UnknownFloat1;
    public hkWorldObjCinfoProperty ChildShapeProperty;
    public byte UnknownByte1;
    public float UnknownFloat2;
}

/// <summary>
/// Bethesda-specific compound.
/// </summary>
public class BSTreadTransform(BinaryReader r) {
    public string Name = Y.String(r);
    public NiQuatTransform Transform1 = new NiQuatTransform(r, h);
    public NiQuatTransform Transform2 = new NiQuatTransform(r, h);
}

/// <summary>
/// Bethesda-specific interpolator.
/// </summary>
public class BSTreadTransfInterpolator : NiInterpolator {
    public BSTreadTransform[] TreadTransforms;
    public int? Data;
}

/// <summary>
/// Anim note types.
/// </summary>
public enum AnimNoteType : uint {
    ANT_INVALID = 0,                // ANT_INVALID
    ANT_GRABIK = 1,                 // ANT_GRABIK
    ANT_LOOKIK = 2                  // ANT_LOOKIK
}

/// <summary>
/// Bethesda-specific object.
/// </summary>
public class BSAnimNote : NiObject {
    public AnimNoteType Type;                           // Type of this note.
    public float Time;                                  // Location in time.
    public uint Arm;                                    // Unknown.
    public float Gain;                                  // Unknown.
    public uint State;                                  // Unknown.
}

/// <summary>
/// Bethesda-specific object.
/// </summary>
public class BSAnimNotes : NiObject {
    public int?[] AnimNotes;                            // BSAnimNote objects.
}

/// <summary>
/// Bethesda-specific Havok serializable.
/// </summary>
public class bhkLiquidAction : bhkSerializable {
    public uint UserData;
    public int UnknownInt2;                             // Unknown
    public int UnknownInt3;                             // Unknown
    public float InitialStickForce;
    public float StickStrength;
    public float NeighborDistance;
    public float NeighborStrength;
}

/// <summary>
/// Culling modes for multi bound nodes.
/// </summary>
public enum BSCPCullingType : uint {
    BSCP_CULL_NORMAL = 0,           // Normal
    BSCP_CULL_ALLPASS = 1,          // All Pass
    BSCP_CULL_ALLFAIL = 2,          // All Fail
    BSCP_CULL_IGNOREMULTIBOUNDS = 3, // Ignore Multi Bounds
    BSCP_CULL_FORCEMULTIBOUNDSNOUPDATE = 4 // Force Multi Bounds No Update
}

/// <summary>
/// Bethesda-specific node.
/// </summary>
public class BSMultiBoundNode : NiNode {
    public int? MultiBound;
    public BSCPCullingType CullingMode;
}

/// <summary>
/// Bethesda-specific object.
/// </summary>
public class BSMultiBound : NiObject {
    public int? Data;
}

/// <summary>
/// Abstract base type for bounding data.
/// </summary>
public class BSMultiBoundData : NiObject {
}

/// <summary>
/// Oriented bounding box.
/// </summary>
public class BSMultiBoundOBB : BSMultiBoundData {
    public Vector3 Center;                              // Center of the box.
    public Vector3 Size;                                // Size of the box along each axis.
    public Matrix3x3 Rotation;                          // Rotation of the bounding box.
}

/// <summary>
/// Bethesda-specific object.
/// </summary>
public class BSMultiBoundSphere : BSMultiBoundData {
    public Vector3 Center;
    public float Radius;
}

/// <summary>
/// This is only defined because of recursion issues.
/// </summary>
public class BSGeometrySubSegment(BinaryReader r) {
    public uint StartIndex = r.ReadUInt32();
    public uint NumPrimitives = r.ReadUInt32();
    public uint ParentArrayIndex = r.ReadUInt32();
    public uint Unused = r.ReadUInt32();
}

/// <summary>
/// Bethesda-specific. Describes groups of triangles either segmented in a grid (for LOD) or by body part for skinned FO4 meshes.
/// </summary>
public class BSGeometrySegmentData(BinaryReader r, Header h) {
    public byte Flags = r.ReadByte();
    public uint Index = r.ReadUInt32();                 // Index = previous Index + previous Num Tris in Segment * 3
    public uint NumTrisinSegment = r.ReadUInt32();      // The number of triangles belonging to this segment
    public uint StartIndex = (h.UserVersion2 == 130) ? r.ReadUInt32() : default;
    public uint NumPrimitives = (h.UserVersion2 == 130) ? r.ReadUInt32() : default;
    public uint ParentArrayIndex = (h.UserVersion2 == 130) ? r.ReadUInt32() : default;
    public BSGeometrySubSegment[] SubSegment = (h.UserVersion2 == 130) ? r.ReadL32FArray(r => new BSGeometrySubSegment(r)) : default;
}

/// <summary>
/// Bethesda-specific AV object.
/// </summary>
public class BSSegmentedTriShape : NiTriShape {
    public BSGeometrySegmentData[] Segment;             // Configuration of each segment
}

/// <summary>
/// Bethesda-specific object.
/// </summary>
public class BSMultiBoundAABB : BSMultiBoundData {
    public Vector3 Position;                            // Position of the AABB's center
    public Vector3 Extent;                              // Extent of the AABB in all directions
}

public class AdditionalDataInfo(BinaryReader r) {
    public int DataType = r.ReadInt32();                // Type of data in this channel
    public int NumChannelBytesPerElement = r.ReadInt32(); // Number of bytes per element of this channel
    public int NumChannelBytes = r.ReadInt32();         // Total number of bytes of this channel (num vertices times num bytes per element)
    public int NumTotalBytesPerElement = r.ReadInt32(); // Number of bytes per element in all channels together. Sum of num channel bytes per element over all block infos.
    public int BlockIndex = r.ReadInt32();              // Unsure. The block in which this channel is stored? Usually there is only one block, and so this is zero.
    public int ChannelOffset = r.ReadInt32();           // Offset (in bytes) of this channel. Sum of all num channel bytes per element of all preceeding block infos.
    public byte UnknownByte1 = r.ReadByte();            // Unknown, usually equal to 2.
}

public class AdditionalDataBlock(BinaryReader r, Header h) {
    public bool HasData = r.ReadBool32();               // Has data
    public int BlockSize = Has Data ? r.ReadInt32() : default; // Size of Block
    public int[] BlockOffsets = Has Data ? r.ReadL32FArray(r => r.ReadInt32()) : default;
    public int NumData = Has Data ? r.ReadInt32() : default;
    public int[] DataSizes = Has Data ? r.ReadInt32() : default;
    public byte[][] Data = Has Data ? r.ReadByte() : default;
}

public class BSPackedAdditionalDataBlock(BinaryReader r, Header h) {
    public bool HasData = r.ReadBool32();               // Has data
    public int NumTotalBytes = Has Data ? r.ReadInt32() : default; // Total number of bytes (over all channels and all elements, equals num total bytes per element times num vertices).
    public int[] BlockOffsets = Has Data ? r.ReadL32FArray(r => r.ReadInt32()) : default; // Block offsets in the data? Usually equal to zero.
    public int[] AtomSizes = Has Data ? r.ReadL32FArray(r => r.ReadInt32()) : default; // The sum of all of these equal num total bytes per element, so this probably describes how each data element breaks down into smaller chunks (i.e. atoms).
    public byte[] Data = Has Data ? r.ReadByte() : default;
    public int UnknownInt1 = r.ReadInt32();
    public int NumTotalBytesPerElement = r.ReadInt32(); // Unsure, but this seems to correspond again to the number of total bytes per element.
}

public class NiAdditionalGeometryData : AbstractAdditionalGeometryData {
    public ushort NumVertices;                          // Number of vertices
    public AdditionalDataInfo[] BlockInfos;             // Number of additional data blocks
    public AdditionalDataBlock[] Blocks;                // Number of additional data blocks
}

public class BSPackedAdditionalGeometryData : AbstractAdditionalGeometryData {
    public ushort NumVertices;
    public AdditionalDataInfo[] BlockInfos;             // Number of additional data blocks
    public BSPackedAdditionalDataBlock[] Blocks;        // Number of additional data blocks
}

/// <summary>
/// Bethesda-specific extra data.
/// </summary>
public class BSWArray : NiExtraData {
    public int[] Items;
}

/// <summary>
/// Bethesda-specific Havok serializable.
/// </summary>
public class bhkAabbPhantom : bhkShapePhantom {
    public byte[] Unused;
    public Vector4 AABBMin;
    public Vector4 AABBMax;
}

/// <summary>
/// Bethesda-specific time controller.
/// </summary>
public class BSFrustumFOVController : NiFloatInterpController {
}

/// <summary>
/// Bethesda-Specific node.
/// </summary>
public class BSDebrisNode : BSRangeNode {
}

/// <summary>
/// A breakable constraint.
/// </summary>
public class bhkBreakableConstraint : bhkConstraint {
    public ConstraintData ConstraintData;               // Constraint within constraint.
    public float Threshold;                             // Amount of force to break the rigid bodies apart?
    public bool RemoveWhenBroken;                       // No: Constraint stays active. Yes: Constraint gets removed when breaking threshold is exceeded.
}

/// <summary>
/// Bethesda-Specific Havok serializable.
/// </summary>
public class bhkOrientHingedBodyAction : bhkSerializable {
    public int? Body;
    public uint UnknownInt1;
    public uint UnknownInt2;
    public byte[] Unused1;
    public Vector4 HingeAxisLS;
    public Vector4 ForwardLS;
    public float Strength;
    public float Damping;
    public byte[] Unused2;
}

/// <summary>
/// Found in Fallout 3 .psa files, extra ragdoll info for NPCs/creatures. (usually idleanims\deathposes.psa)
/// Defines different kill poses. The game selects the pose randomly and applies it to a skeleton immediately upon ragdolling.
/// Poses can be previewed in GECK Object Window-Actor Data-Ragdoll and selecting Pose Matching tab.
/// </summary>
public class bhkPoseArray : NiObject {
    public string[] Bones;
    public BonePose[] Poses;
}

/// <summary>
/// Found in Fallout 3, more ragdoll info?  (meshes\ragdollconstraint\*.rdt)
/// </summary>
public class bhkRagdollTemplate : NiExtraData {
    public int?[] Bones;
}

/// <summary>
/// Data for bhkRagdollTemplate
/// </summary>
public class bhkRagdollTemplateData : NiObject {
    public string Name;
    public float Mass;
    public float Restitution;
    public float Friction;
    public float Radius;
    public HavokMaterial Material;
    public ConstraintData[] Constraint;
}

/// <summary>
/// A range of indices, which make up a region (such as a submesh).
/// </summary>
public class Region(BinaryReader r) {
    public uint StartIndex = r.ReadUInt32();
    public uint NumIndices = r.ReadUInt32();
}

/// <summary>
/// Sets how objects are to be cloned.
/// </summary>
public enum CloningBehavior : uint {
    CLONING_SHARE = 0,              // Share this object pointer with the newly cloned scene.
    CLONING_COPY = 1,               // Create an exact duplicate of this object for use with the newly cloned scene.
    CLONING_BLANK_COPY = 2          // Create a copy of this object for use with the newly cloned stream, leaving some of the data to be written later.
}

/// <summary>
/// The data format of components.
/// </summary>
public enum ComponentFormat : uint {
    F_UNKNOWN = 0x00000000,         // Unknown, or don't care, format.
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
    F_UINT_10_10_10_2 = 0x0001043E
}

/// <summary>
/// Determines how a data stream is used?
/// </summary>
public enum DataStreamUsage : uint {
    USAGE_VERTEX_INDEX = 0,
    USAGE_VERTEX = 1,
    USAGE_SHADER_CONSTANT = 2,
    USAGE_USER = 3
}

/// <summary>
/// Determines how the data stream is accessed?
/// </summary>
[Flags]
public enum DataStreamAccess : uint {
    CPU_Read = 0,
    CPU_Write_Static = 1 << 1,
    CPU_Write_Mutable = 1 << 2,
    CPU_Write_Volatile = 1 << 3,
    GPU_Read = 1 << 4,
    GPU_Write = 1 << 5,
    CPU_Write_Static_Inititialized = 1 << 6
}

public class NiDataStream : NiObject {
    public DataStreamUsage Usage;
    public DataStreamAccess Access;
    public uint NumBytes;                               // The size in bytes of this data stream.
    public CloningBehavior CloningBehavior;
    public Region[] Regions;                            // The regions in the mesh. Regions can be used to mark off submeshes which are independent draw calls.
    public ComponentFormat[] ComponentFormats;          // The format of each component in this data stream.
    public byte[] Data;
    public bool Streamable;
}

public class SemanticData(BinaryReader r) {
    public string Name = Y.String(r);                   // Type of data (POSITION, POSITION_BP, INDEX, NORMAL, NORMAL_BP,
                                                        //     TEXCOORD, BLENDINDICES, BLENDWEIGHT, BONE_PALETTE, COLOR, DISPLAYLIST,
                                                        //     MORPH_POSITION, BINORMAL_BP, TANGENT_BP).
    public uint Index = r.ReadUInt32();                 // An extra index of the data. For example, if there are 3 uv maps,
                                                        //     then the corresponding TEXCOORD data components would have indices
                                                        //     0, 1, and 2, respectively.
}

public class DataStreamRef(BinaryReader r) {
    public int? Stream = X<NiDataStream>.Ref(r);        // Reference to a data stream object which holds the data used by
                                                        //     this reference.
    public bool IsPerInstance = r.ReadBool32();         // Sets whether this stream data is per-instance data for use in
                                                        //     hardware instancing.
    public ushort[] SubmeshToRegionMap = r.ReadL16FArray(r => r.ReadUInt16()); // A lookup table that maps submeshes to regions.
    public SemanticData[] ComponentSemantics = r.ReadL32FArray(r => new SemanticData(r)); // Describes the semantic of each component.
}

/// <summary>
/// An object that can be rendered.
/// </summary>
public abstract class NiRenderObject : NiAVObject {
    public MaterialData MaterialData;                   // Per-material data.
}

/// <summary>
/// Describes the type of primitives stored in a mesh object.
/// </summary>
public enum MeshPrimitiveType : uint {
    MESH_PRIMITIVE_TRIANGLES = 0,   // Triangle primitive type.
    MESH_PRIMITIVE_TRISTRIPS = 1,   // Triangle strip primitive type.
    MESH_PRIMITIVE_LINES = 2,       // Lines primitive type.
    MESH_PRIMITIVE_LINESTRIPS = 3,  // Line strip primitive type.
    MESH_PRIMITIVE_QUADS = 4,       // Quadrilateral primitive type.
    MESH_PRIMITIVE_POINTS = 5       // Point primitive type.
}

/// <summary>
/// A sync point corresponds to a particular stage in per-frame processing.
/// </summary>
public enum SyncPoint : ushort {
    SYNC_ANY = 0x8000,              // Synchronize for any sync points that the modifier supports.
    SYNC_UPDATE = 0x8010,           // Synchronize when an object is updated.
    SYNC_POST_UPDATE = 0x8020,      // Synchronize when an entire scene graph has been updated.
    SYNC_VISIBLE = 0x8030,          // Synchronize when an object is determined to be potentially visible.
    SYNC_RENDER = 0x8040,           // Synchronize when an object is rendered.
    SYNC_PHYSICS_SIMULATE = 0x8050, // Synchronize when a physics simulation step is about to begin.
    SYNC_PHYSICS_COMPLETED = 0x8060, // Synchronize when a physics simulation step has produced results.
    SYNC_REFLECTIONS = 0x8070       // Synchronize after all data necessary to calculate reflections is ready.
}

/// <summary>
/// Base class for mesh modifiers.
/// </summary>
public abstract class NiMeshModifier : NiObject {
    public SyncPoint[] SubmitPoints;                    // The sync points supported by this mesh modifier for SubmitTasks.
    public SyncPoint[] CompletePoints;                  // The sync points supported by this mesh modifier for CompleteTasks.
}

public class ExtraMeshDataEpicMickey(BinaryReader r) {
    public int UnknownInt1 = r.ReadInt32();
    public int UnknownInt2 = r.ReadInt32();
    public int UnknownInt3 = r.ReadInt32();
    public float UnknownInt4 = r.ReadSingle();
    public float UnknownInt5 = r.ReadSingle();
    public float UnknownInt6 = r.ReadSingle();
}

public class ExtraMeshDataEpicMickey2(BinaryReader r) {
    public int Start = r.ReadInt32();
    public int End = r.ReadInt32();
    public short[] UnknownShorts = r.ReadInt16();
}

public class NiMesh : NiRenderObject {
    public MeshPrimitiveType PrimitiveType;             // The primitive type of the mesh, such as triangles or lines.
    public int Unknown51;
    public int Unknown52;
    public int Unknown53;
    public int Unknown54;
    public float Unknown55;
    public int Unknown56;
    public ushort NumSubmeshes;                         // The number of submeshes contained in this mesh.
    public bool InstancingEnabled;                      // Sets whether hardware instancing is being used.
    public NiBound Bound;                               // The combined bounding volume of all submeshes.
    public DataStreamRef[] Datastreams;
    public int?[] Modifiers;
    public byte Unknown100;                             // Unknown.
    public int Unknown101;                              // Unknown.
    public uint Unknown102;                             // Size of additional data.
    public float[] Unknown103;
    public int Unknown200;
    public ExtraMeshDataEpicMickey[] Unknown201;
    public int Unknown250;
    public int[] Unknown251;
    public int Unknown300;
    public short Unknown301;
    public int Unknown302;
    public byte[] Unknown303;
    public int Unknown350;
    public ExtraMeshDataEpicMickey2[] Unknown351;
    public int Unknown400;
}

/// <summary>
/// Manipulates a mesh with the semantic MORPHWEIGHTS using an NiMorphMeshModifier.
/// </summary>
public class NiMorphWeightsController : NiInterpController {
    public uint Count;
    public int?[] Interpolators;
    public string[] TargetNames;
}

public class ElementReference(BinaryReader r) {
    public SemanticData Semantic = new SemanticData(r); // The element semantic.
    public uint NormalizeFlag = r.ReadUInt32();         // Whether or not to normalize the data.
}

/// <summary>
/// Performs linear-weighted blending between a set of target data streams.
/// </summary>
public class NiMorphMeshModifier : NiMeshModifier {
    public byte Flags;                                  // FLAG_RELATIVETARGETS = 0x01
                                                        //     FLAG_UPDATENORMALS   = 0x02
                                                        //     FLAG_NEEDSUPDATE     = 0x04
                                                        //     FLAG_ALWAYSUPDATE    = 0x08
                                                        //     FLAG_NEEDSCOMPLETION = 0x10
                                                        //     FLAG_SKINNED = 0x20
                                                        //     FLAG_SWSKINNED       = 0x40
    public ushort NumTargets;                           // The number of morph targets.
    public ElementReference[] Elements;                 // Semantics and normalization of the morphing data stream elements.
}

public class NiSkinningMeshModifier : NiMeshModifier {
    public ushort Flags;                                // USE_SOFTWARE_SKINNING = 0x0001
                                                        //     RECOMPUTE_BOUNDS = 0x0002
    public int? SkeletonRoot;                           // The root bone of the skeleton.
    public NiTransform SkeletonTransform;               // The transform that takes the root bone parent coordinate system into the skin coordinate system.
    public uint NumBones;                               // The number of bones referenced by this mesh modifier.
    public int?[] Bones;                                // Pointers to the bone nodes that affect this skin.
    public NiTransform[] BoneTransforms;                // The transforms that go from bind-pose space to bone space.
    public NiBound[] BoneBounds;                        // The bounds of the bones.  Only stored if the RECOMPUTE_BOUNDS bit is set.
}

/// <summary>
/// An instance of a hardware-instanced mesh in a scene graph.
/// </summary>
public class NiMeshHWInstance : NiAVObject {
    public int? MasterMesh;                             // The instanced mesh this object represents.
    public int? MeshModifier;
}

/// <summary>
/// Mesh modifier that provides per-frame instancing capabilities in Gamebryo.
/// </summary>
public class NiInstancingMeshModifier : NiMeshModifier {
    public bool HasInstanceNodes;
    public bool PerInstanceCulling;
    public bool HasStaticBounds;
    public int? AffectedMesh;
    public NiBound Bound;
    public int?[] InstanceNodes;
}

public class LODInfo(BinaryReader r) {
    public uint NumBones = r.ReadUInt32();
    public uint[] SkinIndices = r.ReadL32FArray(r => r.ReadUInt32());
}

/// <summary>
/// Defines the levels of detail for a given character and dictates the character's current LOD.
/// </summary>
public class NiSkinningLODController : NiTimeController {
    public uint CurrentLOD;
    public int?[] Bones;
    public int?[] Skins;
    public LODInfo[] LODs;
}

public class PSSpawnRateKey(BinaryReader r) {
    public float Value = r.ReadSingle();
    public float Time = r.ReadSingle();
}

/// <summary>
/// Describes the various methods that may be used to specify the orientation of the particles.
/// </summary>
public enum AlignMethod : uint {
    ALIGN_INVALID = 0,
    ALIGN_PER_PARTICLE = 1,
    ALIGN_LOCAL_FIXED = 2,
    ALIGN_LOCAL_POSITION = 5,
    ALIGN_LOCAL_VELOCITY = 9,
    ALIGN_CAMERA = 16
}

/// <summary>
/// Represents a particle system.
/// </summary>
public class NiPSParticleSystem : NiMesh {
    public int? Simulator;
    public int? Generator;
    public int?[] Emitters;
    public int?[] Spawners;
    public int? DeathSpawner;
    public uint MaxNumParticles;
    public bool HasColors;
    public bool HasRotations;
    public bool HasRotationAxes;
    public bool HasAnimatedTextures;
    public bool WorldSpace;
    public AlignMethod NormalMethod;
    public Vector3 NormalDirection;
    public AlignMethod UpMethod;
    public Vector3 UpDirection;
    public int? LivingSpawner;
    public PSSpawnRateKey[] SpawnRateKeys;
    public bool Pre-RPI;
}

/// <summary>
/// Represents a particle system that uses mesh particles instead of sprite-based particles.
/// </summary>
public class NiPSMeshParticleSystem : NiPSParticleSystem {
    public int?[] MasterParticles;
    public uint PoolSize;
    public bool Auto-FillPools;
}

/// <summary>
/// A mesh modifier that uses particle system data to generate camera-facing quads.
/// </summary>
public class NiPSFacingQuadGenerator : NiMeshModifier {
}

/// <summary>
/// A mesh modifier that uses particle system data to generate aligned quads for each particle.
/// </summary>
public class NiPSAlignedQuadGenerator : NiMeshModifier {
    public float ScaleAmountU;
    public float ScaleLimitU;
    public float ScaleRestU;
    public float ScaleAmountV;
    public float ScaleLimitV;
    public float ScaleRestV;
    public float CenterU;
    public float CenterV;
    public bool UVScrolling;
    public ushort NumFramesAcross;
    public ushort NumFramesDown;
    public bool PingPong;
    public ushort InitialFrame;
    public float InitialFrameVariation;
    public ushort NumFrames;
    public float NumFramesVariation;
    public float InitialTime;
    public float FinalTime;
}

/// <summary>
/// The mesh modifier that performs all particle system simulation.
/// </summary>
public class NiPSSimulator : NiMeshModifier {
    public int?[] SimulationSteps;
}

/// <summary>
/// Abstract base class for a single step in the particle system simulation process.  It has no seralized data.
/// </summary>
public abstract class NiPSSimulatorStep : NiObject {
}

public enum PSLoopBehavior : uint {
    PS_LOOP_CLAMP_BIRTH = 0,        // Key times map such that the first key occurs at the birth of the particle, and times later than the last key get the last key value.
    PS_LOOP_CLAMP_DEATH = 1,        // Key times map such that the last key occurs at the death of the particle, and times before the initial key time get the value of the initial key.
    PS_LOOP_AGESCALE = 2,           // Scale the animation to fit the particle lifetime, so that the first key is age zero, and the last key comes at the particle death.
    PS_LOOP_LOOP = 3,               // The time is converted to one within the time range represented by the keys, as if the key sequence loops forever in the past and future.
    PS_LOOP_REFLECT = 4             // The time is reflection looped, as if the keys played forward then backward the forward then backward etc for all time.
}

/// <summary>
/// Encapsulates a floodgate kernel that updates particle size, colors, and rotations.
/// </summary>
public class NiPSSimulatorGeneralStep : NiPSSimulatorStep {
    public Key[] SizeKeys;                              // The particle size keys.
    public PSLoopBehavior SizeLoopBehavior;             // The loop behavior for the size keys.
    public Key[] ColorKeys;                             // The particle color keys.
    public PSLoopBehavior ColorLoopBehavior;            // The loop behavior for the color keys.
    public QuatKey[] RotationKeys;                      // The particle rotation keys.
    public PSLoopBehavior RotationLoopBehavior;         // The loop behavior for the rotation keys.
    public float GrowTime;                              // The the amount of time over which a particle's size is ramped from 0.0 to 1.0 in seconds
    public float ShrinkTime;                            // The the amount of time over which a particle's size is ramped from 1.0 to 0.0 in seconds
    public ushort GrowGeneration;                       // Specifies the particle generation to which the grow effect should be applied. This is usually generation 0, so that newly created particles will grow.
    public ushort ShrinkGeneration;                     // Specifies the particle generation to which the shrink effect should be applied. This is usually the highest supported generation for the particle system, so that particles will shrink immediately before getting killed.
}

/// <summary>
/// Encapsulates a floodgate kernel that simulates particle forces.
/// </summary>
public class NiPSSimulatorForcesStep : NiPSSimulatorStep {
    public int?[] Forces;                               // The forces affecting the particle system.
}

/// <summary>
/// Encapsulates a floodgate kernel that simulates particle colliders.
/// </summary>
public class NiPSSimulatorCollidersStep : NiPSSimulatorStep {
    public int?[] Colliders;                            // The colliders affecting the particle system.
}

/// <summary>
/// Encapsulates a floodgate kernel that updates mesh particle alignment and transforms.
/// </summary>
public class NiPSSimulatorMeshAlignStep : NiPSSimulatorStep {
    public QuatKey[] RotationKeys;                      // The particle rotation keys.
    public PSLoopBehavior RotationLoopBehavior;         // The loop behavior for the rotation keys.
}

/// <summary>
/// Encapsulates a floodgate kernel that updates particle positions and ages. As indicated by its name, this step should be attached last in the NiPSSimulator mesh modifier.
/// </summary>
public class NiPSSimulatorFinalStep : NiPSSimulatorStep {
}

/// <summary>
/// Updates the bounding volume for an NiPSParticleSystem object.
/// </summary>
public class NiPSBoundUpdater : NiObject {
    public ushort UpdateSkip;                           // Number of particle bounds to skip updating every frame. Higher = more updates each frame.
}

/// <summary>
/// This is used by the Floodgate kernel to determine which NiPSForceHelpers functions to call.
/// </summary>
public enum PSForceType : uint {
    FORCE_BOMB = 0,
    FORCE_DRAG = 1,
    FORCE_AIR_FIELD = 2,
    FORCE_DRAG_FIELD = 3,
    FORCE_GRAVITY_FIELD = 4,
    FORCE_RADIAL_FIELD = 5,
    FORCE_TURBULENCE_FIELD = 6,
    FORCE_VORTEX_FIELD = 7,
    FORCE_GRAVITY = 8
}

/// <summary>
/// Abstract base class for all particle forces.
/// </summary>
public abstract class NiPSForce : NiObject {
    public string Name;
    public PSForceType Type;
    public bool Active;
}

/// <summary>
/// Applies a linear drag force to particles.
/// </summary>
public class NiPSDragForce : NiPSForce {
    public Vector3 DragAxis;
    public float Percentage;
    public float Range;
    public float RangeFalloff;
    public int? DragObject;
}

/// <summary>
/// Applies a gravitational force to particles.
/// </summary>
public class NiPSGravityForce : NiPSForce {
    public Vector3 GravityAxis;
    public float Decay;
    public float Strength;
    public ForceType ForceType;
    public float Turbulence;
    public float TurbulenceScale;
    public int? GravityObject;
}

/// <summary>
/// Applies an explosive force to particles.
/// </summary>
public class NiPSBombForce : NiPSForce {
    public Vector3 BombAxis;
    public float Decay;
    public float DeltaV;
    public DecayType DecayType;
    public SymmetryType SymmetryType;
    public int? BombObject;
}

/// <summary>
/// Abstract base class for all particle emitters.
/// </summary>
public abstract class NiPSEmitter : NiObject {
    public string Name;
    public float Speed;
    public float SpeedVar;
    public float SpeedFlipRatio;
    public float Declination;
    public float DeclinationVar;
    public float PlanarAngle;
    public float PlanarAngleVar;
    public ByteColor4 Color;
    public float Size;
    public float SizeVar;
    public float Lifespan;
    public float LifespanVar;
    public float RotationAngle;
    public float RotationAngleVar;
    public float RotationSpeed;
    public float RotationSpeedVar;
    public Vector3 RotationAxis;
    public bool RandomRotSpeedSign;
    public bool RandomRotAxis;
    public bool Unknown;
}

/// <summary>
/// Abstract base class for particle emitters that emit particles from a volume.
/// </summary>
public abstract class NiPSVolumeEmitter : NiPSEmitter {
    public int? EmitterObject;
}

/// <summary>
/// A particle emitter that emits particles from a rectangular volume.
/// </summary>
public class NiPSBoxEmitter : NiPSVolumeEmitter {
    public float EmitterWidth;
    public float EmitterHeight;
    public float EmitterDepth;
}

/// <summary>
/// A particle emitter that emits particles from a spherical volume.
/// </summary>
public class NiPSSphereEmitter : NiPSVolumeEmitter {
    public float EmitterRadius;
}

/// <summary>
/// A particle emitter that emits particles from a cylindrical volume.
/// </summary>
public class NiPSCylinderEmitter : NiPSVolumeEmitter {
    public float EmitterRadius;
    public float EmitterHeight;
}

/// <summary>
/// Emits particles from one or more NiMesh objects. A random mesh emitter is selected for each particle emission.
/// </summary>
public class NiPSMeshEmitter : NiPSEmitter {
    public int?[] MeshEmitters;
    public Vector3 EmitAxis;
    public int? EmitterObject;
    public EmitFrom MeshEmissionType;
    public VelocityType InitialVelocityType;
}

/// <summary>
/// Abstract base class for all particle emitter time controllers.
/// </summary>
public abstract class NiPSEmitterCtlr : NiSingleInterpController {
    public string EmitterName;
}

/// <summary>
/// Abstract base class for controllers that animate a floating point value on an NiPSEmitter object.
/// </summary>
public abstract class NiPSEmitterFloatCtlr : NiPSEmitterCtlr {
}

/// <summary>
/// Animates particle emission and birth rate.
/// </summary>
public class NiPSEmitParticlesCtlr : NiPSEmitterCtlr {
    public int? EmitterActiveInterpolator;
}

/// <summary>
/// Abstract base class for all particle force time controllers.
/// </summary>
public abstract class NiPSForceCtlr : NiSingleInterpController {
    public string ForceName;
}

/// <summary>
/// Abstract base class for controllers that animate a Boolean value on an NiPSForce object.
/// </summary>
public abstract class NiPSForceBoolCtlr : NiPSForceCtlr {
}

/// <summary>
/// Abstract base class for controllers that animate a floating point value on an NiPSForce object.
/// </summary>
public abstract class NiPSForceFloatCtlr : NiPSForceCtlr {
}

/// <summary>
/// Animates whether or not an NiPSForce object is active.
/// </summary>
public class NiPSForceActiveCtlr : NiPSForceBoolCtlr {
}

/// <summary>
/// Animates the strength value of an NiPSGravityForce object.
/// </summary>
public class NiPSGravityStrengthCtlr : NiPSForceFloatCtlr {
}

/// <summary>
/// Animates the speed value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterSpeedCtlr : NiPSEmitterFloatCtlr {
}

/// <summary>
/// Animates the size value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterRadiusCtlr : NiPSEmitterFloatCtlr {
}

/// <summary>
/// Animates the declination value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterDeclinationCtlr : NiPSEmitterFloatCtlr {
}

/// <summary>
/// Animates the declination variation value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterDeclinationVarCtlr : NiPSEmitterFloatCtlr {
}

/// <summary>
/// Animates the planar angle value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterPlanarAngleCtlr : NiPSEmitterFloatCtlr {
}

/// <summary>
/// Animates the planar angle variation value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterPlanarAngleVarCtlr : NiPSEmitterFloatCtlr {
}

/// <summary>
/// Animates the rotation angle value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterRotAngleCtlr : NiPSEmitterFloatCtlr {
}

/// <summary>
/// Animates the rotation angle variation value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterRotAngleVarCtlr : NiPSEmitterFloatCtlr {
}

/// <summary>
/// Animates the rotation speed value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterRotSpeedCtlr : NiPSEmitterFloatCtlr {
}

/// <summary>
/// Animates the rotation speed variation value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterRotSpeedVarCtlr : NiPSEmitterFloatCtlr {
}

/// <summary>
/// Animates the lifespan value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterLifeSpanCtlr : NiPSEmitterFloatCtlr {
}

/// <summary>
/// Calls ResetParticleSystem on an NiPSParticleSystem target upon looping.
/// </summary>
public class NiPSResetOnLoopCtlr : NiTimeController {
}

/// <summary>
/// This is used by the Floodgate kernel to determine which NiPSColliderHelpers functions to call.
/// </summary>
public enum ColliderType : uint {
    COLLIDER_PLANAR = 0,
    COLLIDER_SPHERICAL = 1
}

/// <summary>
/// Abstract base class for all particle colliders.
/// </summary>
public class NiPSCollider : NiObject {
    public int? Spawner;
    public ColliderType Type;
    public bool Active;
    public float Bounce;
    public bool SpawnonCollide;
    public bool DieonCollide;
}

/// <summary>
/// A planar collider for particles.
/// </summary>
public class NiPSPlanarCollider : NiPSCollider {
    public float Width;
    public float Height;
    public Vector3 XAxis;
    public Vector3 YAxis;
    public int? ColliderObject;
}

/// <summary>
/// A spherical collider for particles.
/// </summary>
public class NiPSSphericalCollider : NiPSCollider {
    public float Radius;
    public int? ColliderObject;
}

/// <summary>
/// Creates a new particle whose initial parameters are based on an existing particle.
/// </summary>
public class NiPSSpawner : NiObject {
    public int? MasterParticleSystem;
    public float PercentageSpawned;
    public float SpawnSpeedFactor;
    public float SpawnSpeedFactorVar;
    public float SpawnDirChaos;
    public float LifeSpan;
    public float LifeSpanVar;
    public ushort NumSpawnGenerations;
    public uint MintoSpawn;
    public uint MaxtoSpawn;
}

public abstract class NiEvaluator : NiObject {
    public string NodeName;                             // The name of the animated NiAVObject.
    public string PropertyType;                         // The RTTI type of the NiProperty the controller is attached to, if applicable.
    public string ControllerType;                       // The RTTI type of the NiTimeController.
    public string ControllerID;                         // An ID that can uniquely identify the controller among others of the same type on the same NiObjectNET.
    public string InterpolatorID;                       // An ID that can uniquely identify the interpolator among others of the same type on the same NiObjectNET.
    public byte[] ChannelTypes;                         // Channel Indices are BASE/POS = 0, ROT = 1, SCALE = 2, FLAG = 3
                                                        //     Channel Types are:
                                                        //      INVALID = 0, COLOR, BOOL, FLOAT, POINT3, ROT = 5
                                                        //     Any channel may be | 0x40 which means POSED
                                                        //     The FLAG (3) channel flags affects the whole evaluator:
                                                        //      REFERENCED = 0x1, TRANSFORM = 0x2, ALWAYSUPDATE = 0x4, SHUTDOWN = 0x8
}

public abstract class NiKeyBasedEvaluator : NiEvaluator {
}

public class NiBoolEvaluator : NiKeyBasedEvaluator {
    public int? Data;
}

public class NiBoolTimelineEvaluator : NiBoolEvaluator {
}

public class NiColorEvaluator : NiKeyBasedEvaluator {
    public int? Data;
}

public class NiFloatEvaluator : NiKeyBasedEvaluator {
    public int? Data;
}

public class NiPoint3Evaluator : NiKeyBasedEvaluator {
    public int? Data;
}

public class NiQuaternionEvaluator : NiKeyBasedEvaluator {
    public int? Data;
}

public class NiTransformEvaluator : NiKeyBasedEvaluator {
    public NiQuatTransform Value;
    public int? Data;
}

public class NiConstBoolEvaluator : NiEvaluator {
    public float Value;
}

public class NiConstColorEvaluator : NiEvaluator {
    public Color4 Value;
}

public class NiConstFloatEvaluator : NiEvaluator {
    public float Value;
}

public class NiConstPoint3Evaluator : NiEvaluator {
    public Vector3 Value;
}

public class NiConstQuaternionEvaluator : NiEvaluator {
    public Quaternion Value;
}

public class NiConstTransformEvaluator : NiEvaluator {
    public NiQuatTransform Value;
}

public class NiBSplineEvaluator : NiEvaluator {
    public float StartTime;
    public float EndTime;
    public int? Data;
    public int? BasisData;
}

public class NiBSplineColorEvaluator : NiBSplineEvaluator {
    public uint Handle;                                 // Handle into the data. (USHRT_MAX for invalid handle.)
}

public class NiBSplineCompColorEvaluator : NiBSplineColorEvaluator {
    public float Offset;
    public float HalfRange;
}

public class NiBSplineFloatEvaluator : NiBSplineEvaluator {
    public uint Handle;                                 // Handle into the data. (USHRT_MAX for invalid handle.)
}

public class NiBSplineCompFloatEvaluator : NiBSplineFloatEvaluator {
    public float Offset;
    public float HalfRange;
}

public class NiBSplinePoint3Evaluator : NiBSplineEvaluator {
    public uint Handle;                                 // Handle into the data. (USHRT_MAX for invalid handle.)
}

public class NiBSplineCompPoint3Evaluator : NiBSplinePoint3Evaluator {
    public float Offset;
    public float HalfRange;
}

public class NiBSplineTransformEvaluator : NiBSplineEvaluator {
    public NiQuatTransform Transform;
    public uint TranslationHandle;                      // Handle into the translation data. (USHRT_MAX for invalid handle.)
    public uint RotationHandle;                         // Handle into the rotation data. (USHRT_MAX for invalid handle.)
    public uint ScaleHandle;                            // Handle into the scale data. (USHRT_MAX for invalid handle.)
}

public class NiBSplineCompTransformEvaluator : NiBSplineTransformEvaluator {
    public float TranslationOffset;
    public float TranslationHalfRange;
    public float RotationOffset;
    public float RotationHalfRange;
    public float ScaleOffset;
    public float ScaleHalfRange;
}

public class NiLookAtEvaluator : NiEvaluator {
    public LookAtFlags Flags;
    public string LookAtName;
    public string DrivenName;
    public int? Interpolator:Translation;
    public int? Interpolator:Roll;
    public int? Interpolator:Scale;
}

public class NiPathEvaluator : NiKeyBasedEvaluator {
    public PathFlags Flags;
    public int BankDir;                                 // -1 = Negative, 1 = Positive
    public float MaxBankAngle;                          // Max angle in radians.
    public float Smoothing;
    public short FollowAxis;                            // 0, 1, or 2 representing X, Y, or Z.
    public int? PathData;
    public int? PercentData;
}

/// <summary>
/// Root node in Gamebryo .kf files (20.5.0.1 and up).
/// For 20.5.0.0, "NiSequenceData" is an alias for "NiControllerSequence" and this is not handled in nifxml.
/// This was not found in any 20.5.0.0 KFs available and they instead use NiControllerSequence directly.
/// </summary>
public class NiSequenceData : NiObject {
    public string Name;
    public uint NumControlledBlocks;
    public uint ArrayGrowBy;
    public ControlledBlock[] ControlledBlocks;
    public int?[] Evaluators;
    public int? TextKeys;
    public float Duration;
    public CycleType CycleType;
    public float Frequency;
    public string AccumRootName;                        // The name of the NiAVObject serving as the accumulation root. This is where all accumulated translations, scales, and rotations are applied.
    public AccumFlags AccumFlags;
}

/// <summary>
/// An NiShadowGenerator object is attached to an NiDynamicEffect object to inform the shadowing system that the effect produces shadows.
/// </summary>
public class NiShadowGenerator : NiObject {
    public string Name;
    public ushort Flags;
    public int?[] ShadowCasters;
    public int?[] ShadowReceivers;
    public int? Target;
    public float DepthBias;
    public ushort SizeHint;
    public float NearClippingDistance;
    public float FarClippingDistance;
    public float DirectionalLightFrustumWidth;
}

public class NiFurSpringController : NiTimeController {
    public float UnknownFloat;
    public float UnknownFloat2;
    public int?[] Bones;                                // List of all armature bones.
    public int?[] Bones2;                               // List of all armature bones.
}

public class CStreamableAssetData : NiObject {
    public int? Root;
    public byte[] UnknownBytes;
}

/// <summary>
/// Compressed collision mesh.
/// </summary>
public class bhkCompressedMeshShape : bhkShape {
    public int? Target;                                 // Points to root node?
    public uint UserData;                               // Unknown.
    public float Radius;                                // A shell that is added around the shape.
    public float UnknownFloat1;                         // Unknown.
    public Vector4 Scale;                               // Scale
    public float RadiusCopy;                            // A shell that is added around the shape.
    public Vector4 ScaleCopy;                           // Scale
    public int? Data;                                   // The collision mesh data.
}

/// <summary>
/// A compressed mesh shape for collision in Skyrim.
/// </summary>
public class bhkCompressedMeshShapeData : bhkRefObject {
    public uint BitsPerIndex;                           // Number of bits in the shape-key reserved for a triangle index
    public uint BitsPerWIndex;                          // Number of bits in the shape-key reserved for a triangle index and its winding
    public uint MaskWIndex;                             // Mask used to get the triangle index and winding from a shape-key (common: 262143 = 0x3ffff)
    public uint MaskIndex;                              // Mask used to get the triangle index from a shape-key (common: 131071 = 0x1ffff)
    public float Error;                                 // The radius of the storage mesh shape? Quantization error?
    public Vector4 BoundsMin;                           // The minimum boundary of the AABB (the coordinates of the corner with the lowest numerical values)
    public Vector4 BoundsMax;                           // The maximum boundary of the AABB (the coordinates of the corner with the highest numerical values)
    public byte WeldingType;
    public byte MaterialType;
    public uint[] Materials32;                          // Does not appear to be used.
    public uint[] Materials16;                          // Does not appear to be used.
    public uint[] Materials8;                           // Does not appear to be used.
    public bhkCMSDMaterial[] ChunkMaterials;            // Table (array) with sets of materials. Chunks refers to this table by index.
    public uint NumNamedMaterials;
    public bhkCMSDTransform[] ChunkTransforms;          // Table (array) with sets of transformations. Chunks refers to this table by index.
    public Vector4[] BigVerts;                          // Compressed Vertices?
    public bhkCMSDBigTris[] BigTris;
    public bhkCMSDChunk[] Chunks;
    public uint NumConvexPieceA;                        // Does not appear to be used. Needs array.
}

/// <summary>
/// Orientation marker for Skyrim's inventory view.
/// How to show the nif in the player's inventory.
/// Typically attached to the root node of the nif tree.
/// If not present, then Skyrim will still show the nif in inventory,
/// using the default values.
/// Name should be 'INV' (without the quotes).
/// For rotations, a short of "4712" appears as "4.712" but "959" appears as "0.959"  meshes\weapons\daedric\daedricbowskinned.nif
/// </summary>
public class BSInvMarker : NiExtraData {
    public ushort RotationX;
    public ushort RotationY;
    public ushort RotationZ;
    public float Zoom;                                  // Zoom factor.
}

/// <summary>
/// Unknown
/// </summary>
public class BSBoneLODExtraData : NiExtraData {
    public uint BoneLODCount;                           // Number of bone entries
    public BoneLOD[] BoneLODInfo;                       // Bone Entry
}

/// <summary>
/// Links a nif with a Havok Behavior .hkx animation file
/// </summary>
public class BSBehaviorGraphExtraData : NiExtraData {
    public string BehaviourGraphFile;                   // Name of the hkx file.
    public bool ControlsBaseSkeleton;                   // Unknown, has to do with blending appended bones onto an actor.
}

/// <summary>
/// A controller that trails a bone behind an actor.
/// </summary>
public class BSLagBoneController : NiTimeController {
    public float LinearVelocity;                        // How long it takes to rotate about an actor back to rest position.
    public float LinearRotation;                        // How the bone lags rotation
    public float MaximumDistance;                       // How far bone will tail an actor.
}

/// <summary>
/// A variation on NiTriShape, for visibility control over vertex groups.
/// </summary>
public class BSLODTriShape : NiTriBasedGeom {
    public uint LOD0Size;
    public uint LOD1Size;
    public uint LOD2Size;
}

/// <summary>
/// Furniture Marker for actors
/// </summary>
public class BSFurnitureMarkerNode : BSFurnitureMarker {
}

/// <summary>
/// Unknown, related to trees.
/// </summary>
public class BSLeafAnimNode : NiNode {
}

/// <summary>
/// Node for handling Trees, Switches branch configurations for variation?
/// </summary>
public class BSTreeNode : NiNode {
    public int?[] Bones1;                               // Unknown
    public int?[] Bones;                                // Unknown
}

/// <summary>
/// Fallout 4 Tri Shape
/// </summary>
public class BSTriShape : NiAVObject {
    public NiBound BoundingSphere;
    public int? Skin;
    public int? ShaderProperty;
    public int? AlphaProperty;
    public BSVertexDesc VertexDesc;
    public ushort NumTriangles;
    public ushort NumVertices;
    public uint DataSize;
    public BSVertexDataSSE[] VertexData;
    public Triangle[] Triangles;
    public uint ParticleDataSize;
    public Vector3[] Vertices;
    public Triangle[] TrianglesCopy;
}

/// <summary>
/// Fallout 4 LOD Tri Shape
/// </summary>
public class BSMeshLODTriShape : BSTriShape {
    public uint LOD0Size;
    public uint LOD1Size;
    public uint LOD2Size;
}

public class BSGeometryPerSegmentSharedData(BinaryReader r) {
    public uint UserIndex = r.ReadUInt32();             // If Bone ID is 0xffffffff, this value refers to the Segment at the listed index. Otherwise this is the "Biped Object", which is like the body part types in Skyrim and earlier.
    public uint BoneID = r.ReadUInt32();                // A hash of the bone name string.
    public float[] CutOffsets = r.ReadL32FArray(r => r.ReadSingle());
}

public class BSGeometrySegmentSharedData(BinaryReader r) {
    public uint NumSegments = r.ReadUInt32();
    public uint TotalSegments = r.ReadUInt32();
    public uint[] SegmentStarts = r.ReadUInt32();
    public BSGeometryPerSegmentSharedData[] PerSegmentData = new BSGeometryPerSegmentSharedData(r);
    public ushort SSFLength = r.ReadUInt16();
    public byte[] SSFFile = r.ReadByte();
}

/// <summary>
/// Fallout 4 Sub-Index Tri Shape
/// </summary>
public class BSSubIndexTriShape : BSTriShape {
    public uint NumPrimitives;
    public uint NumSegments;
    public uint TotalSegments;
    public BSGeometrySegmentData[] Segment;
    public BSGeometrySegmentSharedData SegmentData;
}

/// <summary>
/// Fallout 4 Physics System
/// </summary>
public abstract class bhkSystem : NiObject {
}

/// <summary>
/// Fallout 4 Collision Object
/// </summary>
public class bhkNPCollisionObject : NiCollisionObject {
    public ushort Flags;                                // Due to inaccurate reporting in the CK the Reset and Sync On Update positions are a guess.
                                                        //     Bits: 0=Reset, 2=Notify, 3=SetLocal, 7=SyncOnUpdate, 10=AnimTargeted
    public int? Data;
    public uint BodyID;
}

/// <summary>
/// Fallout 4 Collision System
/// </summary>
public class bhkPhysicsSystem : bhkSystem {
    public byte[] BinaryData;
}

/// <summary>
/// Fallout 4 Ragdoll System
/// </summary>
public class bhkRagdollSystem : bhkSystem {
    public byte[] BinaryData;
}

/// <summary>
/// Fallout 4 Extra Data
/// </summary>
public class BSExtraData : NiExtraData {
}

/// <summary>
/// Fallout 4 Cloth data
/// </summary>
public class BSClothExtraData : BSExtraData {
    public byte[] BinaryData;
}

/// <summary>
/// Fallout 4 Bone Transform
/// </summary>
public class BSSkinBoneTrans(BinaryReader r) {
    public NiBound BoundingSphere = new NiBound(r);
    public Matrix3x3 Rotation = r.ReadMatrix3x3();
    public Vector3 Translation = r.ReadVector3();
    public float Scale = r.ReadSingle();
}

/// <summary>
/// Fallout 4 Skin Instance
/// </summary>
public class BSSkin::Instance : NiObject {
    public int? SkeletonRoot;
    public int? Data;
    public int?[] Bones;
    public Vector3[] Unknown;
}

/// <summary>
/// Fallout 4 Bone Data
/// </summary>
public class BSSkin::BoneData : NiObject {
    public BSSkinBoneTrans[] BoneList;
}

/// <summary>
/// Fallout 4 Positional Data
/// </summary>
public class BSPositionData : NiExtraData {
    public float[] Data;
}

public class BSConnectPoint(BinaryReader r) {
    public string Parent = r.ReadL32AString();
    public string Name = r.ReadL32AString();
    public Quaternion Rotation = r.ReadQuaternion();
    public Vector3 Translation = r.ReadVector3();
    public float Scale = r.ReadSingle();
}

/// <summary>
/// Fallout 4 Item Slot Parent
/// </summary>
public class BSConnectPoint::Parents : NiExtraData {
    public BSConnectPoint[] ConnectPoints;
}

/// <summary>
/// Fallout 4 Item Slot Child
/// </summary>
public class BSConnectPoint::Children : NiExtraData {
    public bool Skinned;
    public string[] Name;
}

/// <summary>
/// Fallout 4 Eye Center Data
/// </summary>
public class BSEyeCenterExtraData : NiExtraData {
    public float[] Data;
}

public class BSPackedGeomDataCombined(BinaryReader r) {
    public float GrayscaletoPaletteScale = r.ReadSingle();
    public NiTransform Transform = new NiTransform(r);
    public NiBound BoundingSphere = new NiBound(r);
}

public class BSPackedGeomData(BinaryReader r, Header h) {
    public uint NumVerts = r.ReadUInt32();
    public uint LODLevels = r.ReadUInt32();
    public uint TriCountLOD0 = r.ReadUInt32();
    public uint TriOffsetLOD0 = r.ReadUInt32();
    public uint TriCountLOD1 = r.ReadUInt32();
    public uint TriOffsetLOD1 = r.ReadUInt32();
    public uint TriCountLOD2 = r.ReadUInt32();
    public uint TriOffsetLOD2 = r.ReadUInt32();
    public BSPackedGeomDataCombined[] Combined = r.ReadL32FArray(r => new BSPackedGeomDataCombined(r));
    public BSVertexDesc VertexDesc = new BSVertexDesc(r);
    public BSVertexData[] VertexData = !BSPackedCombinedSharedGeomDataExtra ? new BSVertexData(r, h) : default;
    public Triangle[] Triangles = !BSPackedCombinedSharedGeomDataExtra ? new Triangle(r) : default;
}

/// <summary>
/// This appears to be a 64-bit hash but nif.xml does not have a 64-bit type.
/// </summary>
public class BSPackedGeomObject(BinaryReader r) {
    public uint ShapeID1 = r.ReadUInt32();
    public uint ShapeID2 = r.ReadUInt32();
}

/// <summary>
/// Fallout 4 Packed Combined Geometry Data.
/// Geometry is baked into the file and given a list of transforms to position each copy.
/// </summary>
public class BSPackedCombinedGeomDataExtra : NiExtraData {
    public BSVertexDesc VertexDesc;
    public uint NumVertices;
    public uint NumTriangles;
    public uint UnknownFlags1;
    public uint UnknownFlags2;
    public uint NumData;
    public BSPackedGeomObject[] Object;
    public BSPackedGeomData[] ObjectData;
}

/// <summary>
/// Fallout 4 Packed Combined Shared Geometry Data.
/// Geometry is NOT baked into the file. It is instead a reference to the shape via a Shape ID (currently undecoded)
/// which loads the geometry via the STAT form for the NIF.
/// </summary>
public class BSPackedCombinedSharedGeomDataExtra : BSPackedCombinedGeomDataExtra {
}

public class NiLightRadiusController : NiFloatInterpController {
}

public class BSDynamicTriShape : BSTriShape {
    public uint VertexDataSize;
    public Vector4[] Vertices;
}

#endregion

/// <summary>
/// Large ref flag.
/// </summary>
public class BSDistantObjectLargeRefExtraData : NiExtraData {
    public bool LargeRef;
}

