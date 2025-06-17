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
public class Footer(BinaryReader r, Header h) { //:M
    public int?[] Roots = h.V >= 0x0303000D ? r.ReadL32FArray(X<NiNode>.Ref) : []; // List of root NIF objects. If there is a camera, for 1st person view, then this NIF object is referred to as well in this list, even if it is not a root object (usually we want the camera to be attached to the Bip Head node).
}

/// <summary>
/// The distance range where a specific level of detail applies.
/// </summary>
public class LODRange(BinaryReader r, Header h) { //:X
    public float NearExtent = r.ReadSingle();   // Begining of range.
    public float FarExtent = r.ReadSingle();    // End of Range.
    public uint[] UnknownInts = h.V <= 0x03010000 ? r.ReadPArray<uint>("I", 3) : default; // Unknown (0,0,0).
}

/// <summary>
/// Group of vertex indices of vertices that match.
/// </summary>
public class MatchGroup(BinaryReader r) { //:M
    public ushort[] VertexIndices = r.ReadL16PArray<ushort>("h"); // The vertex indices.
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
    public int? Shape;
    public int? SkinInstance;
}

/// <summary>
/// A set of NiBoneLODController::SkinInfo.
/// </summary>
public class SkinInfoSet(BinaryReader r) {
    public SkinInfo[] SkinInfo;
}

/// <summary>
/// NiSkinData::BoneVertData. A vertex and its weight.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct BoneVertData { //:M #Marshal
    public static (string, int) Struct = ("<Hf", sizeof(BoneVertData));
    public ushort Index;            // The vertex index, in the mesh.
    public float Weight;            // The vertex weight - between 0.0 and 1.0

    public BoneVertData(BinaryReader r, bool full) {
        Index = r.ReadUInt16();
        Weight = full ? r.ReadSingle() : r.ReadHalf();
    }
}
// use:BoneVertDataHalf -> BoneVertData

/// <summary>
/// Used in NiDefaultAVObjectPalette.
/// </summary>
public class AVObject(BinaryReader r) { //:X
    public string Name = r.ReadL32AString();            // Object name.
    public int? AVObject_ = X<NiAVObject>.Ref(r);       // Object reference.
}

/// <summary>
/// In a .kf file, this links to a controllable object, via its name (or for version 10.2.0.0 and up, a link and offset to a NiStringPalette that contains the name), and a sequence of interpolators that apply to this controllable object, via links.
/// For Controller ID, NiInterpController::GetCtlrID() virtual function returns a string formatted specifically for the derived type.
/// For Interpolator ID, NiInterpController::GetInterpolatorID() virtual function returns a string formatted specifically for the derived type.
/// The string formats are documented on the relevant niobject blocks.
/// </summary>
public class ControlledBlock {
    public string TargetName;       // Name of a controllable object in another NIF file.
    // NiControllerSequence::InterpArrayItem
    public int? Interpolator;
    public int? Controller;
    public int? BlendInterpolator;
    public ushort BlendIndex;
    // Bethesda-only
    public byte Priority;           // Idle animations tend to have low values for this, and high values tend to correspond with the important parts of the animations.
    // NiControllerSequence::IDTag, post-10.1.0.104 only
    public string NodeName;         // The name of the animated NiAVObject.
    public string PropertyType;     // The RTTI type of the NiProperty the controller is attached to, if applicable.
    public string ControllerType;   // The RTTI type of the NiTimeController.
    public string ControllerID;     // An ID that can uniquely identify the controller among others of the same type on the same NiObjectNET.
    public string InterpolatorID;   // An ID that can uniquely identify the interpolator among others of the same type on the same NiObjectNET.

    public ControlledBlock(BinaryReader r, Header h) {
        if (h.V <= 0x0A010067) TargetName = Y.String(r);
        if (h.V <= 0x14050000) Interpolator = X<NiInterpolator>.Ref(r);
        Controller = X<NiTimeController>.Ref(r);
        if (h.V >= 0x0A010068 && h.V <= 0x0A01006E) {
            BlendInterpolator = X<NiBlendInterpolator>.Ref(r);
            BlendIndex = r.ReadUInt16();
        }
        if (h.V <= 0x0A01006A && h.UserVersion2 > 0) Priority = r.ReadByte();
        // Until 10.2
        if (h.V >= 0x0A010068 && h.V <= 0x0A010071) {
            NodeName = Y.String(r);
            PropertyType = Y.String(r);
            ControllerType = Y.String(r);
            ControllerID = Y.String(r);
            InterpolatorID = Y.String(r);
        }
        // From 10.2 to 20.1
        else if (h.V >= 0x0A020000 && h.V <= 0x14010000) {
            var stringPalette = X<NiStringPalette>.Ref(r);
            NodeName = Y.StringRef(r, stringPalette);
            PropertyType = Y.StringRef(r, stringPalette);
            ControllerType = Y.StringRef(r, stringPalette);
            ControllerID = Y.StringRef(r, stringPalette);
            InterpolatorID = Y.StringRef(r, stringPalette);
        }
        // After 20.1
        else if (h.V >= 0x14010001) {
            NodeName = Y.String(r);
            PropertyType = Y.String(r);
            ControllerType = Y.String(r);
            ControllerID = Y.String(r);
            InterpolatorID = Y.String(r);
        }
    }
}

// use:ExportInfo -> []

/// <summary>
/// The NIF file header.
/// </summary>
public class Header {
    public uint V;
    public string HeaderString;     // 'NetImmerse File Format x.x.x.x' (versions &lt;= 10.0.1.2) or 'Gamebryo File Format x.x.x.x' (versions &gt;= 10.1.0.0), with x.x.x.x the version written out. Ends with a newline character (0x0A).
    public string[] Copyright;
    public uint Version;            // The NIF version, in hexadecimal notation
    public EndianType EndianType;   // Determines the endianness of the data in the file.
    public uint UserVersion;        // An extra version number, for companies that decide to modify the file format.
    public uint NumBlocks;          // Number of file objects.
    public uint UserVersion2;
    public string[] ExportInfo;
    public string MaxFilePath;
    public byte[] Metadata;
    public string[] BlockTypes;     // List of all object types used in this NIF file.
    public uint[] BlockTypeHashes;  // List of all object types used in this NIF file.
    public ushort[] BlockTypeIndex; // Maps file objects on their corresponding type: first file object is of type object_types[object_type_index[0]], the second of object_types[object_type_index[1]], etc.
    public uint[] BlockSize;        // Array of block sizes?
    public string[] Strings;        // Strings.
    public uint[] Groups;

    public Header(BinaryReader r) {
        (HeaderString, V) = ParseHeaderStr(r.ReadVAString(0x80, 0xA)); var v = V;
        if (v < 0x03010000) Copyright = [r.ReadL8AString(), r.ReadL8AString(), r.ReadL8AString()];
        Version = v >= 0x03010001 ? r.ReadUInt32() : 0x04000002;
        EndianType = v >= 0x14000003 ? (EndianType)r.ReadByte() : EndianType.ENDIAN_LITTLE;
        if (v >= 0x0A000108) UserVersion = r.ReadUInt32();
        if (v >= 0x03010001) NumBlocks = r.ReadUInt32();
        if ((v == 0x14020007 || v == 0x14000005 || (v >= 0x0A000102 && v <= 0x14000004 && UserVersion <= 11)) && UserVersion >= 3) {
            UserVersion2 = r.ReadUInt32();
            ExportInfo = [r.ReadL8AString(), r.ReadL8AString(), r.ReadL8AString()];
        }
        if (UserVersion2 == 130) MaxFilePath = r.ReadL8AString(0x80);
        if (v >= 0x1E000000) Metadata = r.ReadL8Bytes();
        if (v >= 0x05000001) {
            var numBlockTypes = r.ReadUInt16(); // Number of object types in this NIF file.
            if (v != 0x14030102) BlockTypes = r.ReadFArray(r => r.ReadL32AString(), numBlockTypes);
            else BlockTypeHashes = r.ReadFArray(r => r.ReadUInt32(), numBlockTypes);
            BlockTypeIndex = r.ReadFArray(r => r.ReadUInt16(), (int)NumBlocks);
        }
        if (v >= 0x14020005) BlockSize = r.ReadFArray(r => r.ReadUInt32(), (int)NumBlocks);
        if (v >= 0x14010001) {
            var numStrings = r.ReadUInt32(); // Number of strings.
            var maxStringLength = r.ReadUInt32(); // Maximum string length.
            Strings = r.ReadFArray(r => r.ReadL32AString((int)maxStringLength), (int)numStrings);
        }
        if (v >= 0x05000006) Groups = r.ReadL32PArray<uint>("I");
    }

    static bool IsVersionSupported(uint v) => true;

    static (string, uint) ParseHeaderStr(string s) {
        var p = s.IndexOf("Version");
        if (p >= 0) {
            var v = s;
            v = v[(p + 8)..];
            for (var i = 0; i < v.Length; i++)
                if (char.IsDigit(v[i]) || v[i] == '.') continue;
                else v = v[..i];
            var ver = Ver2Num(v);
            if (!IsVersionSupported(ver)) throw new Exception($"Version {Ver2Str(ver)} ({ver}) is not supported.");
            return (s, ver);
        }
        else if (s.StartsWith("NS")) return (s, 0x0a010000); // Dodgy version for NeoSteam
        throw new Exception("Invalid header string");
    }

    static string Ver2Str(uint v) {
        if (v == 0) return "";
        else if (v < 0x0303000D) {
            // this is an old-style 2-number version with one period
            var s = $"{(v >> 24) & 0xff}.{(v >> 16) & 0xff}";
            uint sub_num1 = (v >> 8) & 0xff, sub_num2 = v & 0xff;
            if (sub_num1 > 0 || sub_num2 > 0) s += $"{sub_num1}";
            if (sub_num2 > 0) s += $"{sub_num2}";
            return s;
        }
        // this is a new-style 4-number version with 3 periods
        else return $"{(v >> 24) & 0xff}.{(v >> 16) & 0xff}.{(v >> 8) & 0xff}.{v & 0xff}";
    }

    static uint Ver2Num(string s) {
        if (string.IsNullOrEmpty(s)) return 0;
        if (s.Contains('.')) {
            var l = s.Split(".");
            var v = 0U;
            if (l.Length > 4) return 0; // Version # has more than 3 dots in it.
            else if (l.Length == 2) {
                // this is an old style version number.
                v += uint.Parse(l[0]) << (3 * 8);
                if (l[1].Length >= 1) v += uint.Parse(l[1][0..1]) << (2 * 8);
                if (l[1].Length >= 2) v += uint.Parse(l[1][1..2]) << (1 * 8);
                if (l[1].Length >= 3) v += uint.Parse(l[1][2..]);
                return v;
            }
            // this is a new style version number with dots separating the digits
            for (var i = 0; i < 4 && i < l.Length; i++) v += uint.Parse(l[i]) << ((3 - i) * 8);
            return v;
        }
        return uint.Parse(s);
    }
}

public class StringPalette(BinaryReader r) { //:X
    public string[] Palette = r.ReadL32AString().Split((char)0); // A bunch of 0x00 seperated strings.
    public uint Length = r.ReadUInt32();       // Length of the palette string is repeated here.
}

/// <summary>
/// Tension, bias, continuity.
/// </summary>
public struct TBC(BinaryReader r) { //:M
    public float T = r.ReadSingle(); // Tension.
    public float B = r.ReadSingle(); // Bias.
    public float C = r.ReadSingle(); // Continuity.
}

/// <summary>
/// A generic key with support for interpolation. Type 1 is normal linear interpolation, type 2 has forward and backward tangents, and type 3 has tension, bias and continuity arguments. Note that color4 and byte always seem to be of type 1.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Key<T> {
    public float Time;              // Time of the key.
    public T Value;                 // The key value.
    public T Forward;               // Key forward tangent.
    public T Backward;              // The key backward tangent.
    public TBC TBC;                 // The TBC of the key.

    public Key(BinaryReader r, KeyType keyType) { //:M
        Time = r.ReadSingle();
        Value = X<T>.Read(r);
        if (keyType == KeyType.QUADRATIC_KEY) {
            Forward = X<T>.Read(r);
            Backward = X<T>.Read(r);
        }
        else if (keyType == KeyType.TBC_KEY) TBC = new TBC(r);
    }
}

/// <summary>
/// Array of vector keys (anything that can be interpolated, except rotations).
/// </summary>
/// <typeparam name="T"></typeparam>
public class KeyGroup<T> { //:M
    public uint NumKeys;            // Number of keys in the array.
    public KeyType Interpolation;   // The key type.
    public Key<T>[] Keys;           // The keys.

    public KeyGroup(BinaryReader r) {
        NumKeys = r.ReadUInt32();
        if (NumKeys != 0) Interpolation = (KeyType)r.ReadUInt32();
        Keys = r.ReadFArray(r => new Key<T>(r, Interpolation), (int)NumKeys);
    }
}

/// <summary>
/// A special version of the key type used for quaternions.  Never has tangents.
/// </summary>
/// <typeparam name="T"></typeparam>
public class QuatKey<T> { //:M
    public float Time;              // Time the key applies.
    public T Value;                 // Value of the key.
    public TBC TBC;                 // The TBC of the key.

    public QuatKey(BinaryReader r, Header h, KeyType keyType) {
        if (h.V <= 0x0A010000 || keyType != KeyType.XYZ_ROTATION_KEY) Time = r.ReadSingle();
        if (keyType != KeyType.XYZ_ROTATION_KEY) Value = X<T>.Read(r);
        if (keyType == KeyType.TBC_KEY) TBC = new TBC(r);
    }
}

/// <summary>
/// Texture coordinates (u,v). As in OpenGL; image origin is in the lower left corner.
/// </summary>
public struct TexCoord { //:M
    public float U; // First coordinate.
    public float V; // Second coordinate.

    public TexCoord(float u, float v) { U = u; V = v; }
    public TexCoord(BinaryReader r) { U = r.ReadSingle(); V = r.ReadSingle(); }
    public TexCoord(BinaryReader r, bool full) { U = full ? r.ReadSingle() : r.ReadHalf(); V = full ? r.ReadSingle() : r.ReadHalf(); }
}
// use:HalfTexCoord -> TexCoord

/// <summary>
/// Describes the order of scaling and rotation matrices. Translate, Scale, Rotation, Center are from TexDesc.
/// Back = inverse of Center. FromMaya = inverse of the V axis with a positive translation along V of 1 unit.
/// </summary>
public enum TransformMethod : uint { //:X
    MayaDeprecated = 0,             // Center * Rotation * Back * Translate * Scale
    Max = 1,                        // Center * Scale * Rotation * Translate * Back
    Maya = 2,                       // Center * Rotation * Back * FromMaya * Translate * Scale
}

/// <summary>
/// NiTexturingProperty::Map. Texture description.
/// </summary>
public class TexDesc { //:M
    public int? Image;                      // Link to the texture image.
    public int? Source;                     // NiSourceTexture object index.
    public TexClampMode ClampMode;          // 0=clamp S clamp T, 1=clamp S wrap T, 2=wrap S clamp T, 3=wrap S wrap T
    public TexFilterMode FilterMode;        // 0=nearest, 1=bilinear, 2=trilinear, 3=..., 4=..., 5=...
    public ushort MaxAnisotropy;
    public uint UVSet;                      // The texture coordinate set in NiGeometryData that this texture slot will use.
    public short PS2L;                      // L can range from 0 to 3 and are used to specify how fast a texture gets blurry.
    public short PS2K;                      // K is used as an offset into the mipmap levels and can range from -2047 to 2047. Positive values push the mipmap towards being blurry and negative values make the mipmap sharper.
    public ushort Unknown1;                 // Unknown, 0 or 0x0101?
    // NiTextureTransform
    public bool HasTextureTransform;        // Whether or not the texture coordinates are transformed.
    public TexCoord Translation;            // The UV translation.
    public TexCoord Scale;                  // The UV scale.
    public float Rotation;                  // The W axis rotation in texture space.
    public TransformMethod TransformMethod; // Depending on the source, scaling can occur before or after rotation.
    public TexCoord Center;                 // The origin around which the texture rotates.

    public TexDesc(BinaryReader r, Header h) {
        if (h.V <= 0x03010000) Image = X<NiImage>.Ref(r);
        if (h.V >= 0x0303000D) Source = X<NiSourceTexture>.Ref(r);
        ClampMode = h.V <= 0x14000005 ? (TexClampMode)r.ReadUInt32() : TexClampMode.WRAP_S_WRAP_T;
        FilterMode = h.V <= 0x14000005 ? (TexFilterMode)r.ReadUInt32() : TexFilterMode.FILTER_TRILERP;
        MaxAnisotropy = h.V <= 0x14050004 ? r.ReadUInt16() : (ushort)0;
        UVSet = h.V <= 0x14000005 ? r.ReadUInt32() : 0;
        PS2L = h.V <= 0x0A040001 ? r.ReadInt16() : (short)0;
        PS2K = h.V <= 0x0A040001 ? r.ReadInt16() : (short)-75;
        if (h.V >= 0x0401010C) Unknown1 = r.ReadUInt16();
        // NiTextureTransform
        HasTextureTransform = h.V >= 0x0A010000 && r.ReadBool32();
        if (!HasTextureTransform) { Scale = new TexCoord(1f, 1f); return; }
        Translation = new TexCoord(r);
        Scale = new TexCoord(r);
        Rotation = r.ReadSingle();
        TransformMethod = (TransformMethod)r.ReadUInt32();
        Center = new TexCoord(r);
    }
}

/// <summary>
/// NiTexturingProperty::ShaderMap. Shader texture description.
/// </summary>
public class ShaderTexDesc { //:X
    public TexDesc Map;
    public uint MapID;

    public ShaderTexDesc(BinaryReader r, Header h) {
        if (!r.ReadBool32()) return;
        Map = new TexDesc(r, h);
        MapID = r.ReadUInt32();
    }
}

/// <summary>
/// List of three vertex indices.
/// </summary>
/// <param name="r"></param>
public struct Triangle(BinaryReader r) { //:M
    public ushort V1 = r.ReadUInt16(); // First vertex index.
    public ushort V2 = r.ReadUInt16(); // Second vertex index.
    public ushort V3 = r.ReadUInt16(); // Third vertex index.
}

[Flags]
public enum VertexFlags : ushort { //:X
    // First 4 bits are unused
    Vertex = 1 << 4,                // & 16
    UVs = 1 << 5,                   // & 32
    UVs_2 = 1 << 6,                 // & 64
    Normals = 1 << 7,               // & 128
    Tangents = 1 << 8,              // & 256
    Vertex_Colors = 1 << 9,         // & 512
    Skinned = 1 << 10,              // & 1024
    Land_Data = 1 << 11,            // & 2048
    Eye_Data = 1 << 12,             // & 4096
    Instance = 1 << 13,             // & 8192
    Full_Precision = 1 << 14        // & 16384
    // Last bit unused
}

public class BSVertexData { //:X
    public Vector3 Vertex;
    public float BitangentX;
    public uint UnknownInt;
    public TexCoord UV;
    public Vector3<byte> Normal;
    public byte BitangentY;
    public Vector3<byte> Tangent;
    public byte BitangentZ;
    public ByteColor4 VertexColors;
    public float[] BoneWeights;
    public byte[] BoneIndices;
    public float EyeData;

    public BSVertexData(BinaryReader r, Header h, VertexFlags arg, bool sse) {
        var full = sse || arg.HasFlag(VertexFlags.Full_Precision);
        var tangents = arg.HasFlag(VertexFlags.Tangents);
        if (arg.HasFlag(VertexFlags.Vertex)) {
            Vertex = full ? r.ReadVector3() : r.ReadHalfVector3();
            if (tangents) BitangentX = full ? r.ReadSingle() : r.ReadHalf();
            else UnknownInt = full ? r.ReadUInt32() : r.ReadUInt16();
        }
        if (arg.HasFlag(VertexFlags.UVs)) UV = new TexCoord(r, false);
        if (arg.HasFlag(VertexFlags.Normals)) {
            Normal = new Vector3<byte>(r.ReadByte(), r.ReadByte(), r.ReadByte());
            BitangentY = r.ReadByte();
            if (tangents) Tangent = new Vector3<byte>(r.ReadByte(), r.ReadByte(), r.ReadByte());
            if (tangents) BitangentZ = r.ReadByte();
        }
        if (arg.HasFlag(VertexFlags.Vertex_Colors)) VertexColors = new ByteColor4(r);
        if (arg.HasFlag(VertexFlags.Skinned)) {
            BoneWeights = [r.ReadHalf(), r.ReadHalf(), r.ReadHalf(), r.ReadHalf()];
            BoneIndices = [r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte()];
        }
        if (arg.HasFlag(VertexFlags.Eye_Data)) EyeData = r.ReadSingle();
    }
}
// use:BSVertexDataSSE -> BSVertexData

public struct BSVertexDesc(BinaryReader r) { //:X #Marshal
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
public class SkinPartition { //:X
    public ushort NumVertices;          // Number of vertices in this submesh.
    public ushort NumTriangles;         // Number of triangles in this submesh.
    public ushort NumBones;             // Number of bones influencing this submesh.
    public ushort NumStrips;            // Number of strips in this submesh (zero if not stripped).
    public ushort NumWeightsPerVertex;  // Number of weight coefficients per vertex. The Gamebryo engine seems to work well only if this number is equal to 4, even if there are less than 4 influences per vertex.
    public ushort[] Bones;              // List of bones.
    public ushort[] VertexMap;          // Maps the weight/influence lists in this submesh to the vertices in the shape being skinned.
    public float[][] VertexWeights;     // The vertex weights.
    public ushort StripLengths;         // The strip lengths.
    public ushort[][] Strips;           // The strips.
    public Triangle[] Triangles;        // The triangles.
    public byte[][] BoneIndices;        // Bone indices, they index into 'Bones'.
    public ushort UnknownShort;         // Unknown
    public BSVertexDesc VertexDesc;
    public Triangle[] TrianglesCopy;

    public SkinPartition(BinaryReader r, Header h) {
        NumVertices = r.ReadUInt16();
        NumTriangles = (ushort)(NumVertices / 3); // #calculated?
        NumBones = r.ReadUInt16();
        NumStrips = r.ReadUInt16();
        NumWeightsPerVertex = r.ReadUInt16();
        Bones = r.ReadPArray<ushort>("H", NumBones);
        if (h.V <= 0x0A000102) {
            VertexMap = r.ReadPArray<ushort>("H", NumVertices);
            VertexWeights = r.ReadFArray(r => r.ReadPArray<float>("f", NumWeightsPerVertex), NumVertices);
            StripLengths = r.ReadUInt16();
            if (NumStrips != 0) Strips = r.ReadFArray(r => r.ReadPArray<ushort>("H", StripLengths), NumStrips);
            else Triangles = r.ReadFArray(r => new Triangle(r), NumTriangles);
        }
        else if (h.V >= 0x0A010000) {
            uint hasVertexWeights;
            if (r.ReadBool32()) VertexMap = r.ReadPArray<ushort>("H", NumVertices);
            if ((hasVertexWeights = r.ReadUInt32()) != 0)
                VertexWeights = hasVertexWeights == 1 ? r.ReadFArray(r => r.ReadPArray<float>("f", NumWeightsPerVertex), NumVertices)
                : hasVertexWeights == 15 ? r.ReadFArray(r => r.ReadFArray(k => k.ReadHalf(), NumWeightsPerVertex), NumVertices)
                : default;
            StripLengths = r.ReadUInt16();
            if (r.ReadBool32())
                if (NumStrips != 0) Strips = r.ReadFArray(r => r.ReadPArray<ushort>("H", StripLengths), NumStrips);
                else Triangles = r.ReadFArray(r => new Triangle(r), NumTriangles);
        }
        if (r.ReadBool32()) BoneIndices = r.ReadFArray(r => r.ReadBytes(NumWeightsPerVertex), NumVertices);
        if (h.UserVersion2 > 34) UnknownShort = r.ReadUInt16();
        if (h.V >= 0x14020007 && h.UserVersion <= 100) {
            VertexDesc = new BSVertexDesc(r);
            TrianglesCopy = r.ReadFArray(r => new Triangle(r), NumTriangles);
        }
    }
}

/// <summary>
/// A plane.
/// </summary>
public struct NiPlane(BinaryReader r) { //:X #Marshal
    public Vector3 Normal = r.ReadVector3();    // The plane normal.
    public float Constant = r.ReadSingle();     // The plane constant.
}

/// <summary>
/// A sphere.
/// </summary>
public struct NiBound(BinaryReader r) { //:X #Marshal
    public Vector3 Center = r.ReadVector3();    // The sphere's center.
    public float Radius = r.ReadSingle();       // The sphere's radius.
}

public struct NiQuatTransform(BinaryReader r, Header h) { //:X
    public Vector3 Translation = r.ReadVector3();
    public Quaternion Rotation = r.ReadQuaternion();
    public float Scale = r.ReadSingle();
    public bool[] TRSValid = h.V <= 0x0A01006D ? [r.ReadBool32(), r.ReadBool32(), r.ReadBool32()] : [true, true, true]; // Whether each transform component is valid.
}

public struct NiTransform(BinaryReader r) { //:M #Marshal
    public Matrix4x4 Rotation = r.ReadMatrix3x3As4x4();  // The rotation part of the transformation matrix. #Modified
    public Vector3 Translation = r.ReadVector3();   // The translation vector.
    public float Scale = r.ReadSingle();            // Scaling part (only uniform scaling is supported).
}

/// <summary>
/// Bethesda Animation. Furniture entry points. It specifies the direction(s) from where the actor is able to enter (and leave) the position.
/// </summary>
[Flags]
public enum FurnitureEntryPoints : ushort { //:X
    Front = 0,                      // front entry point
    Behind = 1 << 1,                // behind entry point
    Right = 1 << 2,                 // right entry point
    Left = 1 << 3,                  // left entry point
    Up = 1 << 4                     // up entry point - unknown function. Used on some beds in Skyrim, probably for blocking of sleeping position.
}

/// <summary>
/// Bethesda Animation. Animation type used on this position. This specifies the function of this position.
/// </summary>
public enum AnimationType : ushort { //:X
    Sit = 1,                        // Actor use sit animation.
    Sleep = 2,                      // Actor use sleep animation.
    Lean = 4,                       // Used for lean animations?
}

/// <summary>
/// Bethesda Animation. Describes a furniture position?
/// </summary>
public class FurniturePosition { //:X
    public Vector3 Offset;                  // Offset of furniture marker.
    public ushort Orientation;              // Furniture marker orientation.
    public byte PositionRef1;               // Refers to a furnituremarkerxx.nif file. Always seems to be the same as Position Ref 2.
    public byte PositionRef2;               // Refers to a furnituremarkerxx.nif file. Always seems to be the same as Position Ref 1.
    public float Heading;                   // Similar to Orientation, in float form.
    public AnimationType AnimationType;     // Unknown
    public FurnitureEntryPoints EntryProperties; // Unknown/unused in nif?

    public FurniturePosition(BinaryReader r, Header h) {
        Offset = r.ReadVector3();
        if (h.UserVersion2 <= 34) {
            Orientation = r.ReadUInt16();
            PositionRef1 = r.ReadByte();
            PositionRef2 = r.ReadByte();
        }
        else {
            Heading = r.ReadSingle();
            AnimationType = (AnimationType)r.ReadUInt16();
            EntryProperties = (FurnitureEntryPoints)r.ReadUInt16();
        }
    }
}

/// <summary>
/// Bethesda Havok. A triangle with extra data used for physics.
/// </summary>
public class TriangleData(BinaryReader r, Header h) { //:X
    public Triangle Triangle = new Triangle(r);             // The triangle.
    public ushort WeldingInfo = r.ReadUInt16();             // Additional havok information on how triangles are welded.
    public Vector3 Normal = h.V <= 0x14000005 ? r.ReadVector3() : default; // This is the triangle's normal.
}

/// <summary>
/// Geometry morphing data component.
/// </summary>
public class Morph { //:M
    public string FrameName;                // Name of the frame.
    public KeyType Interpolation;           // Unlike most objects, the presense of this value is not conditional on there being keys.
    public Key<float>[] Keys;               // The morph key frames.
    public float LegacyWeight;
    public Vector3[] Vectors;               // Morph vectors.

    public Morph(BinaryReader r, Header h, uint numVertices) {
        if (h.V >= 0x0A01006A) FrameName = Y.String(r);
        if (h.V <= 0x0A010000) {
            var numKeys = r.ReadUInt32();
            Interpolation = (KeyType)r.ReadUInt32();
            Keys = r.ReadFArray(r => new Key<float>(r, Interpolation), (int)numKeys);
        }
        if (h.V >= 0x0A010068 && h.V <= 0x14010002) LegacyWeight = r.ReadSingle();
        Vectors = r.ReadFArray(r => r.ReadVector3(), (int)numVertices);
    }
}

/// <summary>
/// particle array entry
/// </summary>
/// <param name="r"></param>
public struct Particle(BinaryReader r) { //:M #Marshal
    public Vector3 Velocity = r.ReadVector3();          // Particle velocity
    public Vector3 UnknownVector = r.ReadVector3();     // Unknown
    public float Lifetime = r.ReadSingle();             // The particle age.
    public float Lifespan = r.ReadSingle();             // Maximum age of the particle.
    public float Timestamp = r.ReadSingle();            // Timestamp of the last update.
    public ushort UnknownShort = r.ReadUInt16();        // Unknown short
    public ushort VertexId = r.ReadUInt16();            // Particle/vertex index matches array index
}

/// <summary>
/// NiSkinData::BoneData. Skinning data component.
/// </summary>
public class BoneData { //:M
    public NiTransform SkinTransform;                   // Offset of the skin from this bone in bind position.
    public Vector3 BoundingSphereOffset;                // Translation offset of a bounding sphere holding all vertices. (Note that its a Sphere Containing Axis Aligned Box not a minimum volume Sphere)
    public float BoundingSphereRadius;                  // Radius for bounding sphere holding all vertices.
    public ushort[] Unknown13Shorts;                    // Unknown, always 0?
    public BoneVertData[] VertexWeights;                // The vertex weights.

    public BoneData(BinaryReader r, Header h, int arg) {
        SkinTransform = new NiTransform(r);
        BoundingSphereOffset = r.ReadVector3();
        BoundingSphereRadius = r.ReadSingle();
        if (h.V == 0x14030009 && (h.UserVersion == 0x20000 || h.UserVersion == 0x30000)) Unknown13Shorts = r.ReadPArray<ushort>("H", 13);
        VertexWeights = h.V <= 0x04020100 ? r.ReadL16SArray<BoneVertData>()
            : h.V >= 0x04020200 && arg == 1 ? r.ReadL16SArray<BoneVertData>()
            : h.V >= 0x14030101 && arg == 15 ? r.ReadL16FArray(r => new BoneVertData(r, false))
            : default;
    }
}

/// <summary>
/// Bethesda Havok. Collision filter info representing Layer, Flags, Part Number, and Group all combined into one uint.
/// </summary>
public class HavokFilter(BinaryReader r, Header h) { //:X
    public OblivionLayer Layer_OB = h.V <= 0x14000005 && h.UserVersion2 < 16 ? (OblivionLayer)r.ReadByte() : OblivionLayer.OL_STATIC;       // The layer the collision belongs to.
    public Fallout3Layer Layer_FO = h.V == 0x14002007 && h.UserVersion2 <= 34 ? (Fallout3Layer)r.ReadByte() : Fallout3Layer.FOL_STATIC;     // The layer the collision belongs to.
    public SkyrimLayer Layer_SK = h.V == 0x14002007 && h.UserVersion2 > 34 ? (SkyrimLayer)r.ReadByte() : SkyrimLayer.SKYL_STATIC;           // The layer the collision belongs to.
    public byte FlagsAndPartNumber = r.ReadByte();                                                                                          // FLAGS are stored in highest 3 bits:
    public ushort Group = r.ReadUInt16();
}

/// <summary>
/// Bethesda Havok. Material wrapper for varying material enums by game.
/// </summary>
public class HavokMaterial(BinaryReader r, Header h) { //:X
    public uint UnknownInt = h.V <= 0x0A000102 ? r.ReadUInt32() : default;
    public OblivionHavokMaterial Material_OB = h.V == 0x14000005 && h.UserVersion2 < 16 ? (OblivionHavokMaterial)r.ReadUInt32() : default;  // The material of the shape.
    public Fallout3HavokMaterial Material_FO = h.V == 0x14002007 && h.UserVersion2 <= 34 ? (Fallout3HavokMaterial)r.ReadUInt32() : default; // The material of the shape.
    public SkyrimHavokMaterial Material_SK = h.V == 0x14002007 && h.UserVersion2 > 34 ? (SkyrimHavokMaterial)r.ReadUInt32() : default;      // The material of the shape.
}

/// <summary>
/// Bethesda Havok. Havok Information for packed TriStrip shapes.
/// </summary>
public class OblivionSubShape(BinaryReader r, Header h) { //:X
    public HavokFilter HavokFilter = new HavokFilter(r, h);
    public uint NumVertices = r.ReadUInt32();                   // The number of vertices that form this sub shape.
    public HavokMaterial Material = new HavokMaterial(r, h);    // The material of the subshape.
}

public class bhkPositionConstraintMotor(BinaryReader r) { //:X #Marshal
    public float MinForce = r.ReadSingle();                     // Minimum motor force
    public float MaxForce = r.ReadSingle();                     // Maximum motor force
    public float Tau = r.ReadSingle();                          // Relative stiffness
    public float Damping = r.ReadSingle();                      // Motor damping value
    public float ProportionalRecoveryVelocity = r.ReadSingle(); // A factor of the current error to calculate the recovery velocity
    public float ConstantRecoveryVelocity = r.ReadSingle();     // A constant velocity which is used to recover from errors
    public bool MotorEnabled = r.ReadBool32();                  // Is Motor enabled
}

public class bhkVelocityConstraintMotor(BinaryReader r) { //:X #Marshal
    public float MinForce = r.ReadSingle();                     // Minimum motor force
    public float MaxForce = r.ReadSingle();                     // Maximum motor force
    public float Tau = r.ReadSingle();                          // Relative stiffness
    public float TargetVelocity = r.ReadSingle();
    public bool UseVelocityTarget = r.ReadBool32();
    public bool MotorEnabled = r.ReadBool32();                  // Is Motor enabled
}

public class bhkSpringDamperConstraintMotor(BinaryReader r) { //:X #Marshal
    public float MinForce = r.ReadSingle();                     // Minimum motor force
    public float MaxForce = r.ReadSingle();                     // Maximum motor force
    public float SpringConstant = r.ReadSingle();               // The spring constant in N/m
    public float SpringDamping = r.ReadSingle();                // The spring damping in Nsec/m
    public bool MotorEnabled = r.ReadBool32();                  // Is Motor enabled
}

public enum MotorType : byte { //:X
    MOTOR_NONE = 0,
    MOTOR_POSITION = 1,
    MOTOR_VELOCITY = 2,
    MOTOR_SPRING = 3
}

public class MotorDescriptor { //:X
    public MotorType Type;
    public bhkPositionConstraintMotor PositionMotor;
    public bhkVelocityConstraintMotor VelocityMotor;
    public bhkSpringDamperConstraintMotor SpringDamperMotor;

    public MotorDescriptor(BinaryReader r) {
        Type = (MotorType)r.ReadByte();
        switch (Type) {
            case MotorType.MOTOR_POSITION: PositionMotor = new bhkPositionConstraintMotor(r); break;
            case MotorType.MOTOR_VELOCITY: VelocityMotor = new bhkVelocityConstraintMotor(r); break;
            case MotorType.MOTOR_SPRING: SpringDamperMotor = new bhkSpringDamperConstraintMotor(r); break;
        }
    }
}

/// <summary>
/// This constraint defines a cone in which an object can rotate. The shape of the cone can be controlled in two (orthogonal) directions.
/// </summary>
public class RagdollDescriptor { //:X
    public Vector4 TwistA;                  // Central directed axis of the cone in which the object can rotate. Orthogonal on Plane A.
    public Vector4 PlaneA;                  // Defines the orthogonal plane in which the body can move, the orthogonal directions in which the shape can be controlled (the direction orthogonal on this one and Twist A).
    public Vector4 MotorA;                  // Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    public Vector4 PivotA;                  // Point around which the object will rotate. Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    public Vector4 TwistB;                  // Central directed axis of the cone in which the object can rotate. Orthogonal on Plane B.
    public Vector4 PlaneB;                  // Defines the orthogonal plane in which the body can move, the orthogonal directions in which the shape can be controlled (the direction orthogonal on this one and Twist A).
    public Vector4 MotorB;                  // Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    public Vector4 PivotB;                  // Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    public float ConeMaxAngle;              // Maximum angle the object can rotate around the vector orthogonal on Plane A and Twist A relative to the Twist A vector. Note that Cone Min Angle is not stored, but is simply minus this angle.
    public float PlaneMinAngle;             // Minimum angle the object can rotate around Plane A, relative to Twist A.
    public float PlaneMaxAngle;             // Maximum angle the object can rotate around Plane A, relative to Twist A.
    public float TwistMinAngle;             // Minimum angle the object can rotate around Twist A, relative to Plane A.
    public float TwistMaxAngle;             // Maximum angle the object can rotate around Twist A, relative to Plane A.
    public float MaxFriction;               // Maximum friction, typically 0 or 10. In Fallout 3, typically 100.
    public MotorDescriptor Motor;

    public RagdollDescriptor(BinaryReader r, Header h) {
        // Oblivion and Fallout 3, Havok 550
        if (h.UserVersion2 <= 16) {
            PivotA = r.ReadVector4();
            PlaneA = r.ReadVector4();
            TwistA = r.ReadVector4();
            PivotB = r.ReadVector4();
            PlaneB = r.ReadVector4();
            TwistB = r.ReadVector4();
        }
        // Fallout 3 and later, Havok 660 and 2010
        else {
            TwistA = r.ReadVector4();
            PlaneA = r.ReadVector4();
            MotorA = r.ReadVector4();
            PivotA = r.ReadVector4();
            TwistB = r.ReadVector4();
            PlaneB = r.ReadVector4();
            MotorB = r.ReadVector4();
            PivotB = r.ReadVector4();
        }
        ConeMaxAngle = r.ReadSingle();
        PlaneMinAngle = r.ReadSingle();
        PlaneMaxAngle = r.ReadSingle();
        TwistMinAngle = r.ReadSingle();
        TwistMaxAngle = r.ReadSingle();
        MaxFriction = r.ReadSingle();
        if (h.V >= 0x14020007 && h.UserVersion2 > 16) Motor = new MotorDescriptor(r);
    }
}

/// <summary>
/// This constraint allows rotation about a specified axis, limited by specified boundaries.
/// </summary>
public class LimitedHingeDescriptor { //:X
    public Vector4 AxleA;                   // Axis of rotation.
    public Vector4 Perp2AxleInA1;           // Vector in the rotation plane which defines the zero angle.
    public Vector4 Perp2AxleInA2;           // Vector in the rotation plane, orthogonal on the previous one, which defines the positive direction of rotation. This is always the vector product of Axle A and Perp2 Axle In A1.
    public Vector4 PivotA;                  // Pivot point around which the object will rotate.
    public Vector4 AxleB;                   // Axle A in second entity coordinate system.
    public Vector4 Perp2AxleInB1;           // Perp2 Axle In A1 in second entity coordinate system.
    public Vector4 Perp2AxleInB2;           // Perp2 Axle In A2 in second entity coordinate system.
    public Vector4 PivotB;                  // Pivot A in second entity coordinate system.
    public float MinAngle;                  // Minimum rotation angle.
    public float MaxAngle;                  // Maximum rotation angle.
    public float MaxFriction;               // Maximum friction, typically either 0 or 10. In Fallout 3, typically 100.
    public MotorDescriptor Motor;

    public LimitedHingeDescriptor(BinaryReader r, Header h) {
        // Oblivion and Fallout 3, Havok 550
        if (h.UserVersion2 <= 16) {
            PivotA = r.ReadVector4();
            AxleA = r.ReadVector4();
            Perp2AxleInA1 = r.ReadVector4();
            Perp2AxleInA2 = r.ReadVector4();
            PivotB = r.ReadVector4();
            AxleB = r.ReadVector4();
            Perp2AxleInB1 = r.ReadVector4();
            Perp2AxleInB2 = r.ReadVector4();
        }
        // Fallout 3 and later, Havok 660 and 2010
        else {
            AxleA = r.ReadVector4();
            Perp2AxleInA1 = r.ReadVector4();
            Perp2AxleInA2 = r.ReadVector4();
            PivotA = r.ReadVector4();
            AxleB = r.ReadVector4();
            Perp2AxleInB1 = r.ReadVector4();
            Perp2AxleInB2 = r.ReadVector4();
            PivotB = r.ReadVector4();
        }
        MinAngle = r.ReadSingle();
        MaxAngle = r.ReadSingle();
        MaxFriction = r.ReadSingle();
        if (h.V >= 0x14020007 && h.UserVersion2 > 16) Motor = new MotorDescriptor(r);
    }
}

/// <summary>
/// This constraint allows rotation about a specified axis.
/// </summary>
public class HingeDescriptor { //:X
    public Vector4 AxleA;                   // Axis of rotation.
    public Vector4 Perp2AxleInA1;           // Vector in the rotation plane which defines the zero angle.
    public Vector4 Perp2AxleInA2;           // Vector in the rotation plane, orthogonal on the previous one, which defines the positive direction of rotation. This is always the vector product of Axle A and Perp2 Axle In A1.
    public Vector4 PivotA;                  // Pivot point around which the object will rotate.
    public Vector4 AxleB;                   // Axle A in second entity coordinate system.
    public Vector4 Perp2AxleInB1;           // Perp2 Axle In A1 in second entity coordinate system.
    public Vector4 Perp2AxleInB2;           // Perp2 Axle In A2 in second entity coordinate system.
    public Vector4 PivotB;                  // Pivot A in second entity coordinate system.

    public HingeDescriptor(BinaryReader r, Header h) {
        // Oblivion
        if (h.V <= 0x14000005) {
            PivotA = r.ReadVector4();
            Perp2AxleInA1 = r.ReadVector4();
            Perp2AxleInA2 = r.ReadVector4();
            PivotB = r.ReadVector4();
            AxleB = r.ReadVector4();
        }
        // Fallout 3
        else if (h.V >= 0x14020007) {
            AxleA = r.ReadVector4();
            Perp2AxleInA1 = r.ReadVector4();
            Perp2AxleInA2 = r.ReadVector4();
            PivotA = r.ReadVector4();
            AxleB = r.ReadVector4();
            Perp2AxleInB1 = r.ReadVector4();
            Perp2AxleInB2 = r.ReadVector4();
            PivotB = r.ReadVector4();
        }
    }
}

public class BallAndSocketDescriptor(BinaryReader r) { //:X
    public Vector4 PivotA = r.ReadVector4(); // Pivot point in the local space of entity A.
    public Vector4 PivotB = r.ReadVector4(); // Pivot point in the local space of entity B.
}

/// <summary>
/// In reality Havok loads these as Transform A and Transform B using hkTransform
/// </summary>
public class PrismaticDescriptor { //:X
    public Vector4 SlidingA;                // Describes the axis the object is able to travel along. Unit vector.
    public Vector4 RotationA;               // Rotation axis.
    public Vector4 PlaneA;                  // Plane normal. Describes the plane the object is able to move on.
    public Vector4 PivotA;                  // Pivot.
    public Vector4 SlidingB;                // Describes the axis the object is able to travel along in B coordinates. Unit vector.
    public Vector4 RotationB;               // Rotation axis.
    public Vector4 PlaneB;                  // Plane normal. Describes the plane the object is able to move on in B coordinates.
    public Vector4 PivotB;                  // Pivot in B coordinates.
    public float MinDistance;               // Describe the min distance the object is able to travel.
    public float MaxDistance;               // Describe the max distance the object is able to travel.
    public float Friction;                  // Friction.
    public MotorDescriptor Motor;

    public PrismaticDescriptor(BinaryReader r, Header h) {
        // Oblivion (Order is a guess)
        if (h.V <= 0x14000005) {
            PivotA = r.ReadVector4();
            RotationA = r.ReadVector4();
            PlaneA = r.ReadVector4();
            SlidingA = r.ReadVector4();
            PivotB = r.ReadVector4();
            RotationB = r.ReadVector4();
            PlaneB = r.ReadVector4();
            SlidingB = r.ReadVector4();
        }
        // Fallout 3
        else if (h.V >= 0x14020007) {
            SlidingA = r.ReadVector4();
            RotationA = r.ReadVector4();
            PlaneA = r.ReadVector4();
            PivotA = r.ReadVector4();
            SlidingB = r.ReadVector4();
            RotationB = r.ReadVector4();
            PlaneB = r.ReadVector4();
            PivotB = r.ReadVector4();
        }
        MinDistance = r.ReadSingle();
        MaxDistance = r.ReadSingle();
        Friction = r.ReadSingle();
        if (h.V >= 0x14020007 && h.UserVersion2 > 16) Motor = new MotorDescriptor(r);
    }
}

public class StiffSpringDescriptor(BinaryReader r) { //:X
    public Vector4 PivotA = r.ReadVector4();
    public Vector4 PivotB = r.ReadVector4();
    public float Length = r.ReadSingle();
}

/// <summary>
/// Used to store skin weights in NiTriShapeSkinController.
/// </summary>
public class OldSkinData(BinaryReader r) { //:X
    public float VertexWeight = r.ReadSingle();        // The amount that this bone affects the vertex.
    public ushort VertexIndex = r.ReadUInt16();        // The index of the vertex that this weight applies to.
    public Vector3 UnknownVector = r.ReadVector3();      // Unknown.  Perhaps some sort of offset?
}

/// <summary>
/// Determines how the raw image data is stored in NiRawImageData.
/// </summary>
public enum ImageType : uint { //:X
    RGB = 0,                        // Colors store red, blue, and green components.
    RGBA = 1                        // Colors store red, blue, green, and alpha components.
}

/// <summary>
/// Box Bounding Volume
/// </summary>
public class BoxBV(BinaryReader r) { //:X
    public Vector3 Center = r.ReadVector3();        //was:Translation
    public Matrix4x4 Axis = r.ReadMatrix3x3As4x4(); //was:Rotation
    public Vector3 Extent = r.ReadVector3();        //was:Radius
}

/// <summary>
/// Capsule Bounding Volume
/// </summary>
public class CapsuleBV(BinaryReader r) { //:X
    public Vector3 Center = r.ReadVector3();
    public Vector3 Origin = r.ReadVector3();
    public float Extent = r.ReadSingle();
    public float Radius = r.ReadSingle();
}

public class HalfSpaceBV(BinaryReader r) { //:X
    public NiPlane Plane = new NiPlane(r);
    public Vector3 Center = r.ReadVector3();
}

public class BoundingVolume { //:M BoundingVolume.BoxBV was:BoundingBox 
    public BoundVolumeType CollisionType;
    public NiBound Sphere;
    public BoxBV Box;
    public CapsuleBV Capsule;
    public UnionBV Union;
    public HalfSpaceBV HalfSpace;

    public BoundingVolume(BinaryReader r) {
        CollisionType = (BoundVolumeType)r.ReadUInt32();
        switch (CollisionType) {
            case BoundVolumeType.SPHERE_BV: Sphere = new NiBound(r); break;
            case BoundVolumeType.BOX_BV: Box = new BoxBV(r); break;
            case BoundVolumeType.CAPSULE_BV: Capsule = new CapsuleBV(r); break;
            case BoundVolumeType.UNION_BV: Union = new UnionBV(r); break;
            case BoundVolumeType.HALFSPACE_BV: HalfSpace = new HalfSpaceBV(r); break;
        }
    }
}

public class UnionBV(BinaryReader r) { //:X
    public BoundingVolume[] BoundingVolumes = r.ReadL32FArray(r => new BoundingVolume(r));
}

public class MorphWeight(BinaryReader r) { //:X
    public int? Interpolator = X<NiInterpolator>.Ref(r);
    public float Weight = r.ReadSingle();
}

/// <summary>
/// A list of transforms for each bone in bhkPoseArray.
/// </summary>
public class BoneTransform(BinaryReader r) { //:X
    public BoneTransform[] Transforms = r.ReadL32FArray(r => new BoneTransform(r));
}

/// <summary>
/// Array of Vectors for Decal placement in BSDecalPlacementVectorExtraData.
/// </summary>
public class DecalVectorArray { //:X
    public short NumVectors;
    public Vector3[] Points;        // Vector XYZ coords
    public Vector3[] Normals;       // Vector Normals

    public DecalVectorArray(BinaryReader r) {
        NumVectors = r.ReadInt16();
        Points = r.ReadPArray<Vector3>("3f", NumVectors);
        Normals = r.ReadPArray<Vector3>("3f", NumVectors);
    }
}

/// <summary>
/// Editor flags for the Body Partitions.
/// </summary>
[Flags]
public enum BSPartFlag : ushort { //:X
    PF_EDITOR_VISIBLE = 0,                      // Visible in Editor
    PF_START_NET_BONESET = 1 << 8               // Start a new shared boneset.  It is expected this BoneSet and the following sets in the Skin Partition will have the same bones.
}

/// <summary>
/// Body part list for DismemberSkinInstance
/// </summary>
public class BodyPartList(BinaryReader r) { //:X
    public BSPartFlag PartFlag = (BSPartFlag)r.ReadUInt16();
    public BSDismemberBodyPartType BodyPart = (BSDismemberBodyPartType)r.ReadUInt16();
}

/// <summary>
/// Stores Bone Level of Detail info in a BSBoneLODExtraData
/// </summary>
public class BoneLOD(BinaryReader r) { //:X
    public uint Distance = r.ReadUInt32();
    public string BoneName = Y.String(r);
}

/// <summary>
/// Per-chunk material, used in bhkCompressedMeshShapeData
/// </summary>
public class bhkCMSDMaterial(BinaryReader r, Header h) { //:X
    public SkyrimHavokMaterial Material = (SkyrimHavokMaterial)r.ReadUInt32();
    public HavokFilter Filter = new HavokFilter(r, h);
}

/// <summary>
/// Triangle indices used in pair with "Big Verts" in a bhkCompressedMeshShapeData.
/// </summary>
public class bhkCMSDBigTris(BinaryReader r, Header h) { //:X
    public ushort Triangle1 = r.ReadUInt16();
    public ushort Triangle2 = r.ReadUInt16();
    public ushort Triangle3 = r.ReadUInt16();
    public uint Material = r.ReadUInt32(); // Always 0?
    public ushort WeldingInfo = r.ReadUInt16();
}

/// <summary>
/// A set of transformation data: translation and rotation
/// </summary>
public class bhkCMSDTransform(BinaryReader r, Header h) { //:X
    public Vector4 Translation = r.ReadVector4();           // A vector that moves the chunk by the specified amount. W is not used.
    public Quaternion Rotation = r.ReadQuaternionWFirst();  // Rotation. Reference point for rotation is bhkRigidBody translation.
}

/// <summary>
/// Defines subshape chunks in bhkCompressedMeshShapeData
/// </summary>
public class bhkCMSDChunk(BinaryReader r) { //:X
    public Vector4 Translation = r.ReadVector4();
    public uint MaterialIndex = r.ReadUInt32();             // Index of material in bhkCompressedMeshShapeData::Chunk Materials
    public ushort Reference = r.ReadUInt16();               // Always 65535?
    public ushort TransformIndex = r.ReadUInt16();          // Index of transformation in bhkCompressedMeshShapeData::Chunk Transforms
    public ushort[] Vertices = r.ReadL32PArray<ushort>("H");
    public ushort[] Indices = r.ReadL32PArray<ushort>("H");
    public ushort[] Strips = r.ReadL32PArray<ushort>("H");
    public ushort[] WeldingInfo = r.ReadL32PArray<ushort>("H");
}

public class MalleableDescriptor { //:X
    public hkConstraintType Type;   // Type of constraint
    public uint NumEntities;        // Always 2 (Hardcoded). Number of bodies affected by this constraint.
    public int? EntityA;            // Usually NONE. The entity affected by this constraint.
    public int? EntityB;            // Usually NONE. The entity affected by this constraint.
    public uint Priority;           // Usually 1. Higher values indicate higher priority of this constraint?
    public BallAndSocketDescriptor BallAndSocket;
    public HingeDescriptor Hinge;
    public LimitedHingeDescriptor LimitedHinge;
    public PrismaticDescriptor Prismatic;
    public RagdollDescriptor Ragdoll;
    public StiffSpringDescriptor StiffSpring;
    public float Tau;               // not in Fallout 3 or Skyrim
    public float Damping;           // In TES CS described as Damping
    public float Strength;          // In GECK and Creation Kit described as Strength

    public MalleableDescriptor(BinaryReader r, Header h) {
        Type = (hkConstraintType)r.ReadUInt32();
        NumEntities = r.ReadUInt32();
        EntityA = X<bhkEntity>.Ref(r);
        EntityB = X<bhkEntity>.Ref(r);
        Priority = r.ReadUInt32();
        switch (Type) {
            case hkConstraintType.BallAndSocket: BallAndSocket = new BallAndSocketDescriptor(r); break;
            case hkConstraintType.Hinge: Hinge = new HingeDescriptor(r, h); break;
            case hkConstraintType.LimitedHinge: LimitedHinge = new LimitedHingeDescriptor(r, h); break;
            case hkConstraintType.Prismatic: Prismatic = new PrismaticDescriptor(r, h); break;
            case hkConstraintType.Ragdoll: Ragdoll = new RagdollDescriptor(r, h); break;
            case hkConstraintType.StiffSpring: StiffSpring = new StiffSpringDescriptor(r); break;
        }
        if (h.V <= 0x14000005) {
            Tau = r.ReadSingle();
            Damping = r.ReadSingle();
        }
        else if (h.V >= 0x14020007)
            Strength = r.ReadSingle();
    }
}

public class ConstraintData { //:X
    public hkConstraintType Type;   // Type of constraint.
    public uint NumEntities;        // Always 2 (Hardcoded). Number of bodies affected by this constraint.
    public int? EntityA;            // Usually NONE. The entity affected by this constraint.
    public int? EntityB;            // Usually NONE. The entity affected by this constraint.
    public uint Priority;           // Usually 1. Higher values indicate higher priority of this constraint?
    public BallAndSocketDescriptor BallAndSocket;
    public HingeDescriptor Hinge;
    public LimitedHingeDescriptor LimitedHinge;
    public PrismaticDescriptor Prismatic;
    public RagdollDescriptor Ragdoll;
    public StiffSpringDescriptor StiffSpring;
    public MalleableDescriptor Malleable;

    public ConstraintData(BinaryReader r, Header h) {
        Type = (hkConstraintType)r.ReadUInt32();
        NumEntities = r.ReadUInt32();
        EntityA = X<bhkEntity>.Ref(r);
        EntityB = X<bhkEntity>.Ref(r);
        Priority = r.ReadUInt32();
        switch (Type) {
            case hkConstraintType.BallAndSocket: BallAndSocket = new BallAndSocketDescriptor(r); break;
            case hkConstraintType.Hinge: Hinge = new HingeDescriptor(r, h); break;
            case hkConstraintType.LimitedHinge: LimitedHinge = new LimitedHingeDescriptor(r, h); break;
            case hkConstraintType.Prismatic: Prismatic = new PrismaticDescriptor(r, h); break;
            case hkConstraintType.Ragdoll: Ragdoll = new RagdollDescriptor(r, h); break;
            case hkConstraintType.StiffSpring: StiffSpring = new StiffSpringDescriptor(r); break;
            case hkConstraintType.Malleable: Malleable = new MalleableDescriptor(r, h); break;
        }
    }
}

#endregion

#region NIF Objects
// These are the main units of data that NIF files are arranged in.
// They are like C classes and can contain many pieces of data.
// The only differences between these and compounds is that these are treated as object types by the NIF format and can inherit from other classes.

/// <summary>
/// Abstract object type.
/// </summary>
[JsonDerivedType(typeof(NiNode), typeDiscriminator: nameof(NiNode))]
[JsonDerivedType(typeof(NiTriShape), typeDiscriminator: nameof(NiTriShape))]
[JsonDerivedType(typeof(NiTexturingProperty), typeDiscriminator: nameof(NiTexturingProperty))]
[JsonDerivedType(typeof(NiSourceTexture), typeDiscriminator: nameof(NiSourceTexture))]
[JsonDerivedType(typeof(NiMaterialProperty), typeDiscriminator: nameof(NiMaterialProperty))]
[JsonDerivedType(typeof(NiMaterialColorController), typeDiscriminator: nameof(NiMaterialColorController))]
[JsonDerivedType(typeof(NiTriShapeData), typeDiscriminator: nameof(NiTriShapeData))]
[JsonDerivedType(typeof(RootCollisionNode), typeDiscriminator: nameof(RootCollisionNode))]
[JsonDerivedType(typeof(NiStringExtraData), typeDiscriminator: nameof(NiStringExtraData))]
[JsonDerivedType(typeof(NiSkinInstance), typeDiscriminator: nameof(NiSkinInstance))]
[JsonDerivedType(typeof(NiSkinData), typeDiscriminator: nameof(NiSkinData))]
[JsonDerivedType(typeof(NiAlphaProperty), typeDiscriminator: nameof(NiAlphaProperty))]
[JsonDerivedType(typeof(NiZBufferProperty), typeDiscriminator: nameof(NiZBufferProperty))]
[JsonDerivedType(typeof(NiVertexColorProperty), typeDiscriminator: nameof(NiVertexColorProperty))]
[JsonDerivedType(typeof(NiBSAnimationNode), typeDiscriminator: nameof(NiBSAnimationNode))]
[JsonDerivedType(typeof(NiBSParticleNode), typeDiscriminator: nameof(NiBSParticleNode))]
[JsonDerivedType(typeof(NiParticles), typeDiscriminator: nameof(NiParticles))]
[JsonDerivedType(typeof(NiParticlesData), typeDiscriminator: nameof(NiParticlesData))]
[JsonDerivedType(typeof(NiRotatingParticles), typeDiscriminator: nameof(NiRotatingParticles))]
[JsonDerivedType(typeof(NiRotatingParticlesData), typeDiscriminator: nameof(NiRotatingParticlesData))]
[JsonDerivedType(typeof(NiAutoNormalParticles), typeDiscriminator: nameof(NiAutoNormalParticles))]
[JsonDerivedType(typeof(NiAutoNormalParticlesData), typeDiscriminator: nameof(NiAutoNormalParticlesData))]
[JsonDerivedType(typeof(NiUVController), typeDiscriminator: nameof(NiUVController))]
[JsonDerivedType(typeof(NiUVData), typeDiscriminator: nameof(NiUVData))]
[JsonDerivedType(typeof(NiTextureEffect), typeDiscriminator: nameof(NiTextureEffect))]
[JsonDerivedType(typeof(NiTextKeyExtraData), typeDiscriminator: nameof(NiTextKeyExtraData))]
[JsonDerivedType(typeof(NiVertWeightsExtraData), typeDiscriminator: nameof(NiVertWeightsExtraData))]
[JsonDerivedType(typeof(NiParticleSystemController), typeDiscriminator: nameof(NiParticleSystemController))]
[JsonDerivedType(typeof(NiBSPArrayController), typeDiscriminator: nameof(NiBSPArrayController))]
[JsonDerivedType(typeof(NiGravity), typeDiscriminator: nameof(NiGravity))]
[JsonDerivedType(typeof(NiParticleBomb), typeDiscriminator: nameof(NiParticleBomb))]
[JsonDerivedType(typeof(NiParticleColorModifier), typeDiscriminator: nameof(NiParticleColorModifier))]
[JsonDerivedType(typeof(NiParticleGrowFade), typeDiscriminator: nameof(NiParticleGrowFade))]
[JsonDerivedType(typeof(NiParticleMeshModifier), typeDiscriminator: nameof(NiParticleMeshModifier))]
[JsonDerivedType(typeof(NiParticleRotation), typeDiscriminator: nameof(NiParticleRotation))]
[JsonDerivedType(typeof(NiKeyframeController), typeDiscriminator: nameof(NiKeyframeController))]
[JsonDerivedType(typeof(NiKeyframeData), typeDiscriminator: nameof(NiKeyframeData))]
[JsonDerivedType(typeof(NiColorData), typeDiscriminator: nameof(NiColorData))]
[JsonDerivedType(typeof(NiGeomMorpherController), typeDiscriminator: nameof(NiGeomMorpherController))]
[JsonDerivedType(typeof(NiMorphData), typeDiscriminator: nameof(NiMorphData))]
[JsonDerivedType(typeof(AvoidNode), typeDiscriminator: nameof(AvoidNode))]
[JsonDerivedType(typeof(NiVisController), typeDiscriminator: nameof(NiVisController))]
[JsonDerivedType(typeof(NiVisData), typeDiscriminator: nameof(NiVisData))]
[JsonDerivedType(typeof(NiAlphaController), typeDiscriminator: nameof(NiAlphaController))]
[JsonDerivedType(typeof(NiFloatData), typeDiscriminator: nameof(NiFloatData))]
[JsonDerivedType(typeof(NiPosData), typeDiscriminator: nameof(NiPosData))]
[JsonDerivedType(typeof(NiBillboardNode), typeDiscriminator: nameof(NiBillboardNode))]
[JsonDerivedType(typeof(NiShadeProperty), typeDiscriminator: nameof(NiShadeProperty))]
[JsonDerivedType(typeof(NiWireframeProperty), typeDiscriminator: nameof(NiWireframeProperty))]
[JsonDerivedType(typeof(NiCamera), typeDiscriminator: nameof(NiCamera))]
[JsonDerivedType(typeof(NiExtraData), typeDiscriminator: nameof(NiExtraData))]
[JsonDerivedType(typeof(NiSkinPartition), typeDiscriminator: nameof(NiSkinPartition))]
public abstract class NiObject(BinaryReader r, Header h) {
    public static NiObject Read(BinaryReader r, Header h) {
        var nodeType = r.ReadL32AString(0x40);
        switch (nodeType) {
            case "NiNode": return new NiNode(r, h);
            case "NiTriShape": return new NiTriShape(r, h);
            case "NiTexturingProperty": return new NiTexturingProperty(r, h);
            case "NiSourceTexture": return new NiSourceTexture(r, h);
            case "NiMaterialProperty": return new NiMaterialProperty(r, h);
            case "NiMaterialColorController": return new NiMaterialColorController(r, h);
            case "NiTriShapeData": return new NiTriShapeData(r, h);
            case "RootCollisionNode": return new RootCollisionNode(r, h);
            case "NiStringExtraData": return new NiStringExtraData(r, h);
            case "NiSkinInstance": return new NiSkinInstance(r, h);
            case "NiSkinData": return new NiSkinData(r, h);
            case "NiAlphaProperty": return new NiAlphaProperty(r, h);
            case "NiZBufferProperty": return new NiZBufferProperty(r, h);
            case "NiVertexColorProperty": return new NiVertexColorProperty(r, h);
            case "NiBSAnimationNode": return new NiBSAnimationNode(r, h);
            case "NiBSParticleNode": return new NiBSParticleNode(r, h);
            case "NiParticles": return new NiParticles(r, h);
            case "NiParticlesData": return new NiParticlesData(r, h);
            case "NiRotatingParticles": return new NiRotatingParticles(r, h);
            case "NiRotatingParticlesData": return new NiRotatingParticlesData(r, h);
            case "NiAutoNormalParticles": return new NiAutoNormalParticles(r, h);
            case "NiAutoNormalParticlesData": return new NiAutoNormalParticlesData(r, h);
            case "NiUVController": return new NiUVController(r, h);
            case "NiUVData": return new NiUVData(r, h);
            case "NiTextureEffect": return new NiTextureEffect(r, h);
            case "NiTextKeyExtraData": return new NiTextKeyExtraData(r, h);
            case "NiVertWeightsExtraData": return new NiVertWeightsExtraData(r, h);
            case "NiParticleSystemController": return new NiParticleSystemController(r, h);
            case "NiBSPArrayController": return new NiBSPArrayController(r, h);
            case "NiGravity": return new NiGravity(r, h);
            case "NiParticleBomb": return new NiParticleBomb(r, h);
            case "NiParticleColorModifier": return new NiParticleColorModifier(r, h);
            case "NiParticleGrowFade": return new NiParticleGrowFade(r, h);
            case "NiParticleMeshModifier": return new NiParticleMeshModifier(r, h);
            case "NiParticleRotation": return new NiParticleRotation(r, h);
            case "NiKeyframeController": return new NiKeyframeController(r, h);
            case "NiKeyframeData": return new NiKeyframeData(r, h);
            case "NiColorData": return new NiColorData(r, h);
            case "NiGeomMorpherController": return new NiGeomMorpherController(r, h);
            case "NiMorphData": return new NiMorphData(r, h);
            case "AvoidNode": return new AvoidNode(r, h);
            case "NiVisController": return new NiVisController(r, h);
            case "NiVisData": return new NiVisData(r, h);
            case "NiAlphaController": return new NiAlphaController(r, h);
            case "NiFloatData": return new NiFloatData(r, h);
            case "NiPosData": return new NiPosData(r, h);
            case "NiBillboardNode": return new NiBillboardNode(r, h);
            case "NiShadeProperty": return new NiShadeProperty(r, h);
            case "NiWireframeProperty": return new NiWireframeProperty(r, h);
            case "NiCamera": return new NiCamera(r, h);
            default: { Log($"Tried to read an unsupported NiObject type ({nodeType})."); return null; }
        }
    }
} //:M

/// <summary>
/// Unknown.
/// </summary>
public class Ni3dsAlphaAnimator : NiObject { //:X
    public byte[] Unknown1;         // Unknown.
    public int? Parent;             // The parent?
    public uint Num1;               // Unknown.
    public uint Num2;               // Unknown.
    public uint[] Unknown2;         // Unknown.

    public Ni3dsAlphaAnimator(BinaryReader r, Header h) : base(r, h) {
        Unknown1 = r.ReadBytes(40);
        Parent = X<NiObject>.Ref(r);
        Num1 = r.ReadUInt32();
        Num2 = r.ReadUInt32();
        Unknown2 = r.ReadPArray<uint>("I", (int)(Num1 * Num2 * 2));
    }
}

/// <summary>
/// Unknown. Only found in 2.3 nifs.
/// </summary>
public class Ni3dsAnimationNode : NiObject { //:X
    public string Name;             // Name of this object.
    public float[] UnknownFloats1;  // Unknown. Matrix?
    public ushort UnknownShort;     // Unknown.
    public int? Child;              // Child?
    public float[] UnknownFloats2;  // Unknown.
    public byte[] UnknownArray;     // Unknown.

    public Ni3dsAnimationNode(BinaryReader r, Header h) : base(r, h) {
        Name = Y.String(r);
        if (!r.ReadBool32()) return;
        UnknownFloats1 = r.ReadPArray<float>("f", 21);
        UnknownShort = r.ReadUInt16();
        Child = X<NiObject>.Ref(r);
        UnknownFloats2 = r.ReadPArray<float>("f", 12);
        UnknownArray = r.ReadL32Bytes();
    }
}

/// <summary>
/// Unknown!
/// </summary>
public class Ni3dsColorAnimator : NiObject { //:X
    public byte[] Unknown1;          // Unknown.

    public Ni3dsColorAnimator(BinaryReader r, Header h) : base(r, h) {
        Unknown1 = r.ReadBytes(184);
    }
}

/// <summary>
/// Unknown!
/// </summary>
public class Ni3dsMorphShape : NiObject { //:X
    public byte[] Unknown1;           // Unknown.

    public Ni3dsMorphShape(BinaryReader r, Header h) : base(r, h) {
        Unknown1 = r.ReadBytes(14);
    }
}

/// <summary>
/// Unknown!
/// </summary>
public class Ni3dsParticleSystem : NiObject { //:X
    public byte[] Unknown1;           // Unknown.

    public Ni3dsParticleSystem(BinaryReader r, Header h) : base(r, h) {
        Unknown1 = r.ReadBytes(14);
    }
}

/// <summary>
/// Unknown!
/// </summary>
public class Ni3dsPathController : NiObject { //:X
    public byte[] Unknown1;         // Unknown.

    public Ni3dsPathController(BinaryReader r, Header h) : base(r, h) {
        Unknown1 = r.ReadBytes(20);
    }
}

/// <summary>
/// LEGACY (pre-10.1). Abstract base class for particle system modifiers.
/// </summary>
public abstract class NiParticleModifier : NiObject {
    public int? NextModifier;   // Next particle modifier.
    public int? Controller;     // Points to the particle system controller parent.

    public NiParticleModifier(BinaryReader r, Header h) : base(r, h) {
        NextModifier = X<NiParticleModifier>.Ref(r);
        Controller = h.V >= 0x04000002 ? X<NiParticleSystemController>.Ptr(r) : default;
    }
}

/// <summary>
/// Particle system collider.
/// </summary>
public abstract class NiPSysCollider : NiObject {
    public float Bounce;                // Amount of bounce for the collider.
    public bool SpawnOnCollide;         // Spawn particles on impact?
    public bool DieOnCollide;           // Kill particles on impact?
    public int? SpawnModifier;          // Spawner to use for the collider.
    public int? Parent;                 // Link to parent.
    public int? NextCollider;           // The next collider.
    public int? ColliderObject;         // The object whose position and orientation are the basis of the collider.

    public NiPSysCollider(BinaryReader r, Header h) : base(r, h) {
        Bounce = r.ReadSingle();
        SpawnOnCollide = r.ReadBool32();
        DieOnCollide = r.ReadBool32();
        SpawnModifier = X<NiPSysSpawnModifier>.Ref(r);
        Parent = X<NiPSysColliderManager>.Ptr(r);
        NextCollider = X<NiPSysCollider>.Ref(r);
        ColliderObject = X<NiAVObject>.Ptr(r);
    }
}

public enum BroadPhaseType : byte { //:X
    BROAD_PHASE_INVALID = 0,
    BROAD_PHASE_ENTITY = 1,
    BROAD_PHASE_PHANTOM = 2,
    BROAD_PHASE_BORDER = 3
}

public class hkWorldObjCinfoProperty(BinaryReader r) {
    public uint Data = r.ReadUInt32();
    public uint Size = r.ReadUInt32();
    public uint CapacityAndFlags = r.ReadUInt32();
}

/// <summary>
/// The base type of most Bethesda-specific Havok-related NIF objects.
/// </summary>
public abstract class bhkRefObject(BinaryReader r, Header h) : NiObject(r, h) { }

/// <summary>
/// Havok objects that can be saved and loaded from disk?
/// </summary>
public abstract class bhkSerializable(BinaryReader r, Header h) : bhkRefObject(r, h) { }

/// <summary>
/// Havok objects that have a position in the world?
/// </summary>
public abstract class bhkWorldObject : bhkSerializable {
    public int? Shape;              // Link to the body for this collision object.
    public uint UnknownInt;
    public HavokFilter HavokFilter;
    public byte[] Unused;           // Garbage data from memory.
    public BroadPhaseType BroadPhaseType;
    public byte[] UnusedBytes;
    public hkWorldObjCinfoProperty CinfoProperty;

    public bhkWorldObject(BinaryReader r, Header h) : base(r, h) {
        Shape = X<bhkShape>.Ref(r);
        if (h.V <= 0x0A000100) UnknownInt = r.ReadUInt32();
        HavokFilter = new HavokFilter(r, h);
        Unused = r.ReadBytes(4);
        BroadPhaseType = (BroadPhaseType)r.ReadUInt32();
        UnusedBytes = r.ReadBytes(3);
        CinfoProperty = new hkWorldObjCinfoProperty(r);
    }
}

/// <summary>
/// Havok object that do not react with other objects when they collide (causing deflection, etc.) but still trigger collision notifications to the game.  Possible uses are traps, portals, AI fields, etc.
/// </summary>
public abstract class bhkPhantom(BinaryReader r, Header h) : bhkWorldObject(r, h) { }

/// <summary>
/// A Havok phantom that uses a Havok shape object for its collision volume instead of just a bounding box.
/// </summary>
public abstract class bhkShapePhantom(BinaryReader r, Header h) : bhkPhantom(r, h) { }

/// <summary>
/// Unknown shape.
/// </summary>
public class bhkSimpleShapePhantom : bhkShapePhantom {
    public byte[] Unused2; // Garbage data from memory.
    public Matrix4x4 Transform;

    public bhkSimpleShapePhantom(BinaryReader r, Header h) : base(r, h) {
        Unused2 = r.ReadBytes(8);
        Transform = r.ReadMatrix4x4();
    }
}

/// <summary>
/// A havok node, describes physical properties.
/// </summary>
public abstract class bhkEntity(BinaryReader r, Header h) : bhkWorldObject(r, h) { }

/// <summary>
/// This is the default body type for all "normal" usable and static world objects. The "T" suffix marks this body as active for translation and rotation, a normal bhkRigidBody ignores those
/// properties. Because the properties are equal, a bhkRigidBody may be renamed into a bhkRigidBodyT and vice-versa.
/// </summary>
public class bhkRigidBody : bhkEntity {
    public hkResponseType CollisionResponse;    // How the body reacts to collisions. See hkResponseType for hkpWorld default implementations.
    public byte UnusedByte1;                    // Skipped over when writing Collision Response and Callback Delay.
    public ushort ProcessContactCallbackDelay;  // Lowers the frequency for processContactCallbacks. A value of 5 means that a callback is raised every 5th frame. The default is once every 65535 frames.
    public uint UnknownInt1;                    // Unknown.
    public HavokFilter HavokFilterCopy;         // Copy of Havok Filter
    public byte[] Unused2;                      // Garbage data from memory. Matches previous Unused value.
    public uint UnknownInt2;                    // Unknown.
    public hkResponseType CollisionResponse2;
    public byte UnusedByte2;                    // Skipped over when writing Collision Response and Callback Delay.
    public ushort ProcessContactCallbackDelay2; // Lowers the frequency for processContactCallbacks. A value of 5 means that a callback is raised every 5th frame. The default is once every 65535 frames.
    public Vector4 Translation;                 // A vector that moves the body by the specified amount. Only enabled in bhkRigidBodyT objects.
    public Quaternion Rotation;                 // The rotation Yaw/Pitch/Roll to apply to the body. Only enabled in bhkRigidBodyT objects.
    public Vector4 LinearVelocity;              // Linear velocity.
    public Vector4 AngularVelocity;             // Angular velocity.
    public Matrix4x3 InertiaTensor;             // Defines how the mass is distributed among the body, i.e. how difficult it is to rotate around any given axis.
    public Vector4 Center;                      // The body's center of mass.
    public float Mass;                          // The body's mass in kg. A mass of zero represents an immovable object.
    public float LinearDamping;                 // Reduces the movement of the body over time. A value of 0.1 will remove 10% of the linear velocity every second.
    public float AngularDamping;                // Reduces the movement of the body over time. A value of 0.05 will remove 5% of the angular velocity every second.
    public float TimeFactor;
    public float GravityFactor;
    public float Friction;
    public float RollingFrictionMultiplier;
    public float Restitution;                   // How "bouncy" the body is, i.e. how much energy it has after colliding. Less than 1.0 loses energy, greater than 1.0 gains energy.
    public float MaxLinearVelocity;
    public float MaxAngularVelocity;
    public float PenetrationDepth;              // The maximum allowed penetration for this object.
    public hkMotionType MotionSystem;           // Motion system? Overrides Quality when on Keyframed?
    public hkDeactivatorType DeactivatorType;   // The initial deactivator type of the body.
    public bool EnableDeactivation;
    public hkSolverDeactivation SolverDeactivation; // How aggressively the engine will try to zero the velocity for slow objects. This does not save CPU.
    public hkQualityType QualityType;           // The type of interaction with other objects.
    public float UnknownFloat1;
    public byte[] UnknownBytes1;                // Unknown.
    public byte[] UnknownBytes2;                // Unknown. Skyrim only.
    public int?[] Constraints;
    public uint BodyFlags;                      // 1 = respond to wind

    public bhkRigidBody(BinaryReader r, Header h) : base(r, h) {
        CollisionResponse = (hkResponseType)r.ReadByte();
        UnusedByte1 = r.ReadByte();
        ProcessContactCallbackDelay = r.ReadUInt16();
        if (h.V >= 0x0A010000) {
            UnknownInt1 = r.ReadUInt32();
            HavokFilterCopy = new HavokFilter(r, h);
            Unused2 = r.ReadBytes(4);
            if (h.UserVersion2 > 34) UnknownInt2 = r.ReadUInt32();
            CollisionResponse2 = (hkResponseType)r.ReadByte();
            UnusedByte2 = r.ReadByte();
            ProcessContactCallbackDelay2 = r.ReadUInt16();
            if (h.UserVersion2 <= 34) UnknownInt2 = r.ReadUInt32();
        }
        else {
            CollisionResponse2 = hkResponseType.RESPONSE_SIMPLE_CONTACT;
            ProcessContactCallbackDelay2 = 0xffff;
        }
        Translation = r.ReadVector4();
        Rotation = r.ReadQuaternion();
        LinearVelocity = r.ReadVector4();
        AngularVelocity = r.ReadVector4();
        InertiaTensor = r.ReadMatrix4x3();
        Center = r.ReadVector4();
        Mass = r.ReadSingle();
        LinearDamping = r.ReadSingle();
        AngularDamping = r.ReadSingle();
        if (h.UserVersion2 > 34) {
            TimeFactor = r.ReadSingle();
            if (h.UserVersion2 != 130) GravityFactor = r.ReadSingle();
        }
        Friction = r.ReadSingle();
        if (h.UserVersion2 > 34) RollingFrictionMultiplier = r.ReadSingle();
        Restitution = r.ReadSingle();
        if (h.V >= 0x0A010000) {
            MaxLinearVelocity = r.ReadSingle();
            MaxAngularVelocity = r.ReadSingle();
            if (h.UserVersion2 != 130) PenetrationDepth = r.ReadSingle();
        }
        MotionSystem = (hkMotionType)r.ReadByte();
        if (h.UserVersion2 <= 34) DeactivatorType = (hkDeactivatorType)r.ReadByte();
        SolverDeactivation = (hkSolverDeactivation)r.ReadByte();
        QualityType = (hkQualityType)r.ReadByte();
        if (h.UserVersion2 == 130) {
            PenetrationDepth = r.ReadSingle();
            UnknownFloat1 = r.ReadSingle();
        }
        UnknownBytes1 = r.ReadBytes(12);
        if (h.UserVersion2 > 34) UnknownBytes2 = r.ReadBytes(4);
        Constraints = r.ReadL32FArray(X<bhkSerializable>.Ref);
        BodyFlags = h.UserVersion2 < 76 ? r.ReadUInt32() : r.ReadUInt16();
    }
}

/// <summary>
/// The "T" suffix marks this body as active for translation and rotation.
/// </summary>
public class bhkRigidBodyT(BinaryReader r, Header h) : bhkRigidBody(r, h) { }

/// <summary>
/// Describes a physical constraint.
/// </summary>
public abstract class bhkConstraint : bhkSerializable {
    public int?[] Entities;         // The entities affected by this constraint.
    public uint Priority;           // Usually 1. Higher values indicate higher priority of this constraint?

    public bhkConstraint(BinaryReader r, Header h) : base(r, h) {
        Entities = r.ReadL32FArray(X<bhkEntity>.Ref);
        Priority = r.ReadUInt32();
    }
}

/// <summary>
/// Hinge constraint.
/// </summary>
public class bhkLimitedHingeConstraint : bhkConstraint {
    public LimitedHingeDescriptor LimitedHinge; // Describes a limited hinge constraint

    public bhkLimitedHingeConstraint(BinaryReader r, Header h) : base(r, h) {
        LimitedHinge = new LimitedHingeDescriptor(r, h);
    }
}

/// <summary>
/// A malleable constraint.
/// </summary>
public class bhkMalleableConstraint : bhkConstraint {
    public MalleableDescriptor Malleable; // Constraint within constraint.

    public bhkMalleableConstraint(BinaryReader r, Header h) : base(r, h) {
        Malleable = new MalleableDescriptor(r, h);
    }
}

/// <summary>
/// A spring constraint.
/// </summary>
public class bhkStiffSpringConstraint : bhkConstraint {
    public StiffSpringDescriptor StiffSpring; // Stiff Spring constraint.

    public bhkStiffSpringConstraint(BinaryReader r, Header h) : base(r, h) {
        StiffSpring = new StiffSpringDescriptor(r);
    }
}

/// <summary>
/// Ragdoll constraint.
/// </summary>
public class bhkRagdollConstraint : bhkConstraint {
    public RagdollDescriptor Ragdoll; // Ragdoll constraint.

    public bhkRagdollConstraint(BinaryReader r, Header h) : base(r, h) {
        Ragdoll = new RagdollDescriptor(r, h);
    }
}

/// <summary>
/// A prismatic constraint.
/// </summary>
public class bhkPrismaticConstraint : bhkConstraint {
    public PrismaticDescriptor Prismatic; // Describes a prismatic constraint

    public bhkPrismaticConstraint(BinaryReader r, Header h) : base(r, h) {
        Prismatic = new PrismaticDescriptor(r, h);
    }
}

/// <summary>
/// A hinge constraint.
/// </summary>
public class bhkHingeConstraint : bhkConstraint {
    public HingeDescriptor Hinge; // Hinge constraing.

    public bhkHingeConstraint(BinaryReader r, Header h) : base(r, h) {
        Hinge = new HingeDescriptor(r, h);
    }
}

/// <summary>
/// A Ball and Socket Constraint.
/// </summary>
public class bhkBallAndSocketConstraint : bhkConstraint {
    public BallAndSocketDescriptor BallAndSocket; // Describes a ball and socket constraint

    public bhkBallAndSocketConstraint(BinaryReader r, Header h) : base(r, h) {
        BallAndSocket = new BallAndSocketDescriptor(r);
    }
}

/// <summary>
/// Two Vector4 for pivot in A and B.
/// </summary>
public class ConstraintInfo(BinaryReader r, Header h) {
    public Vector4 PivotInA = r.ReadVector4();
    public Vector4 PivotInB = r.ReadVector4();
}

/// <summary>
/// A Ball and Socket Constraint chain.
/// </summary>
public class bhkBallSocketConstraintChain : bhkSerializable {
    public ConstraintInfo[] Pivots;             // Two pivot points A and B for each constraint.
    public float Tau;                           // High values are harder and more reactive, lower values are smoother.
    public float Damping;                       // High values are harder and more reactive, lower values are smoother.
    public float ConstraintForceMixing;         // Restitution (amount of elasticity) of constraints. Added to the diagonal of the constraint matrix. A value of 0.0 can result in a division by zero with some chain configurations.
    public float MaxErrorDistance;              // Maximum distance error in constraints allowed before stabilization algorithm kicks in. A smaller distance causes more resistance.
    public int?[] EntitiesA;
    public int NumEntities;                     // Hardcoded to 2. Don't change.
    public int? EntityA;
    public int? EntityB;
    public uint Priority;

    public bhkBallSocketConstraintChain(BinaryReader r, Header h) : base(r, h) {
        Pivots = r.ReadFArray(r => new ConstraintInfo(r, h), (int)r.ReadUInt32() >> 1);
        Tau = r.ReadSingle();
        EntitiesA = r.ReadL32FArray(r => X<bhkRigidBody>.Ptr(r));
        EntityA = X<bhkRigidBody>.Ptr(r);
        EntityB = X<bhkRigidBody>.Ptr(r);
        Priority = r.ReadUInt32();
    }
}

/// <summary>
/// A Havok Shape?
/// </summary>
public abstract class bhkShape(BinaryReader r, Header h) : bhkSerializable(r, h) { }

/// <summary>
/// Transforms a shape.
/// </summary>
public class bhkTransformShape : bhkShape {
    public int? Shape;                      // The shape that this object transforms.
    public HavokMaterial Material;          // The material of the shape.
    public float Radius;
    public byte[] Unused;                   // Garbage data from memory.
    public Matrix4x4 Transform;             // A transform matrix.

    public bhkTransformShape(BinaryReader r, Header h) : base(r, h) {
        Shape = X<bhkShape>.Ref(r);
        Material = new HavokMaterial(r, h);
        Radius = r.ReadSingle();
        Unused = r.ReadBytes(8);
        Transform = r.ReadMatrix4x4();
    }
}

/// <summary>
/// A havok shape, perhaps with a bounding sphere for quick rejection in addition to more detailed shape data?
/// </summary>
public abstract class bhkSphereRepShape : bhkShape {
    public HavokMaterial Material;          // The material of the shape.
    public float Radius;                    // The radius of the sphere that encloses the shape.

    public bhkSphereRepShape(BinaryReader r, Header h) : base(r, h) {
        Material = new HavokMaterial(r, h);
        Radius = r.ReadSingle();
    }
}

/// <summary>
/// A havok shape.
/// </summary>
public abstract class bhkConvexShape(BinaryReader r, Header h) : bhkSphereRepShape(r, h) { }

/// <summary>
/// A sphere.
/// </summary>
public class bhkSphereShape(BinaryReader r, Header h) : bhkConvexShape(r, h) { }

/// <summary>
/// A capsule.
/// </summary>
public class bhkCapsuleShape : bhkConvexShape {
    public byte[] Unused;                       // Not used. The following wants to be aligned at 16 bytes.
    public Vector3 FirstPoint;                  // First point on the capsule's axis.
    public float Radius1;                       // Matches first capsule radius.
    public Vector3 SecondPoint;                 // Second point on the capsule's axis.
    public float Radius2;                       // Matches second capsule radius.

    public bhkCapsuleShape(BinaryReader r, Header h) : base(r, h) {
        Unused = r.ReadBytes(8);
        FirstPoint = r.ReadVector3();
        Radius1 = r.ReadSingle();
        SecondPoint = r.ReadVector3();
        Radius2 = r.ReadSingle();
    }
}

/// <summary>
/// A box.
/// </summary>
public class bhkBoxShape : bhkConvexShape {
    public byte[] Unused;                       // Not used. The following wants to be aligned at 16 bytes.
    public Vector3 Dimensions;                  // A cube stored in Half Extents. A unit cube (1.0, 1.0, 1.0) would be stored as 0.5, 0.5, 0.5.
    public float UnusedFloat;                   // Unused as Havok stores the Half Extents as hkVector4 with the W component unused.

    public bhkBoxShape(BinaryReader r, Header h) : base(r, h) {
        Unused = r.ReadBytes(8);
        Dimensions = r.ReadVector3();
        UnusedFloat = r.ReadSingle();
    }
}

/// <summary>
/// A convex shape built from vertices. Note that if the shape is used in a non-static object (such as clutter), then they will simply fall through ground when they are under a bhkListShape.
/// </summary>
public class bhkConvexVerticesShape : bhkConvexShape {
    public hkWorldObjCinfoProperty VerticesProperty;
    public hkWorldObjCinfoProperty NormalsProperty;
    public Vector4[] Vertices;                          // Vertices. Fourth component is 0. Lexicographically sorted.
    public Vector4[] Normals;                           // Half spaces as determined by the set of vertices above. First three components define the normal pointing to the exterior, fourth component is the signed distance of the separating plane to the origin: it is minus the dot product of v and n, where v is any vertex on the separating plane, and n is the normal. Lexicographically sorted.

    public bhkConvexVerticesShape(BinaryReader r, Header h) : base(r, h) {
        VerticesProperty = new hkWorldObjCinfoProperty(r);
        NormalsProperty = new hkWorldObjCinfoProperty(r);
        Vertices = r.ReadL32PArray<Vector4>("4f");
        Normals = r.ReadL32PArray<Vector4>("4f");
    }
}

/// <summary>
/// A convex transformed shape?
/// Should inherit from bhkConvexShape according to hierarchy, but seems to be exactly the same as bhkTransformShape.
/// </summary>
public class bhkConvexTransformShape(BinaryReader r, Header h) : bhkTransformShape(r, h) { }

public class bhkConvexSweepShape : bhkShape {
    public int? Shape;
    public HavokMaterial Material;
    public float Radius;
    public Vector3 Unknown;

    public bhkConvexSweepShape(BinaryReader r, Header h) : base(r, h) {
        Shape = X<bhkShape>.Ref(r);
        Material = new HavokMaterial(r, h);
        Radius = r.ReadSingle();
        Unknown = r.ReadVector3();
    }
}

/// <summary>
/// Unknown.
/// </summary>
public class bhkMultiSphereShape : bhkSphereRepShape {
    public float UnknownFloat1;             // Unknown.
    public float UnknownFloat2;             // Unknown.
    public NiBound[] Spheres;               // This array holds the spheres which make up the multi sphere shape.

    public bhkMultiSphereShape(BinaryReader r, Header h) : base(r, h) {
        UnknownFloat1 = r.ReadSingle();
        UnknownFloat2 = r.ReadSingle();
        Spheres = r.ReadL32FArray(x => new NiBound(r));
    }
}

/// <summary>
/// A tree-like Havok data structure stored in an assembly-like binary code?
/// </summary>
public abstract class bhkBvTreeShape(BinaryReader r, Header h) : bhkShape(r, h) { }

/// <summary>
/// Memory optimized partial polytope bounding volume tree shape (not an entity).
/// </summary>
public class bhkMoppBvTreeShape : bhkBvTreeShape {
    public int? Shape;                      // The shape.
    public uint[] Unused;                   // Garbage data from memory. Referred to as User Data, Shape Collection, and Code.
    public float ShapeScale;                // Scale.
    public Vector3 Origin;                  // Origin of the object in mopp coordinates. This is the minimum of all vertices in the packed shape along each axis, minus 0.1.
    public float Scale;                     // The scaling factor to quantize the MOPP: the quantization factor is equal to 256*256 divided by this number. In Oblivion files, scale is taken equal to 256*256*254 / (size + 0.2) where size is the largest dimension of the bounding box of the packed shape.
    public MoppDataBuildType BuildType;     // Tells if MOPP Data was organized into smaller chunks (PS3) or not (PC)
    public byte[] MOPPData;                 // The tree of bounding volume data.

    public bhkMoppBvTreeShape(BinaryReader r, Header h) : base(r, h) {
        Shape = X<bhkShape>.Ref(r);
        Unused = r.ReadPArray<uint>("I", 3);
        ShapeScale = r.ReadSingle();
        var moppDataSize = 0; // #calculated
        if (h.V >= 0x0A000102) {
            Origin = r.ReadVector3();
            Scale = r.ReadSingle();
        }
        if (h.UserVersion2 > 34) BuildType = (MoppDataBuildType)r.ReadByte();
        MOPPData = r.ReadBytes((int)moppDataSize);
    }
}

/// <summary>
/// Havok collision object that uses multiple shapes?
/// </summary>
public abstract class bhkShapeCollection(BinaryReader r, Header h) : bhkShape(r, h) { }

/// <summary>
/// A list of shapes.
/// Do not put a bhkPackedNiTriStripsShape in the Sub Shapes. Use a separate collision nodes without a list shape for those.
/// Also, shapes collected in a bhkListShape may not have the correct walking noise, so only use it for non-walkable objects.
/// </summary>
public class bhkListShape : bhkShapeCollection {
    public int?[] SubShapes;                        // List of shapes.
    public HavokMaterial Material;                  // The material of the shape.
    public hkWorldObjCinfoProperty ChildShapeProperty;
    public hkWorldObjCinfoProperty ChildFilterProperty;
    public uint[] UnknownInts;                     // Unknown.

    public bhkListShape(BinaryReader r, Header h) : base(r, h) {
        SubShapes = r.ReadL32FArray(X<bhkShape>.Ref);
        Material = new HavokMaterial(r, h);
        ChildShapeProperty = new hkWorldObjCinfoProperty(r);
        ChildFilterProperty = new hkWorldObjCinfoProperty(r);
        UnknownInts = r.ReadL32PArray<uint>("I");
    }
}

/// <summary>
/// bhkMeshShape appears in some old Oblivion nifs, for instance meshes/architecture/basementsections/ungrdltraphingedoor.nif but only in some distributions of Oblivion
/// XXX not completely decoded, also the 4 dummy separator bytes seem to be missing from nifs that have this block
/// </summary>
public class bhkMeshShape : bhkShape {
    public uint[] Unknowns;
    public float Radius;
    public byte[] Unused2;
    public Vector4 Scale;
    public hkWorldObjCinfoProperty[] ShapeProperties;
    public int[] Unknown2;
    public int?[] StripsData;       // Refers to a bunch of NiTriStripsData objects that make up this shape.

    public bhkMeshShape(BinaryReader r, Header h) : base(r, h) {
        Unknowns = r.ReadPArray<uint>("I", 2);
        Radius = r.ReadSingle();
        Unused2 = r.ReadBytes(8);
        Scale = r.ReadVector4();
        ShapeProperties = r.ReadL32FArray(r => new hkWorldObjCinfoProperty(r));
        Unknown2 = r.ReadPArray<int>("i", 3);
        if (h.V <= 0x0A000100) StripsData = r.ReadL32FArray(X<NiTriStripsData>.Ref);
    }
}

/// <summary>
/// A shape constructed from strips data.
/// </summary>
public class bhkPackedNiTriStripsShape : bhkShapeCollection {
    public OblivionSubShape[] SubShapes;
    public uint UserData;
    public uint Unused1;                // Looks like a memory pointer and may be garbage.
    public float Radius;
    public uint Unused2;                // Looks like a memory pointer and may be garbage.
    public Vector4 Scale;
    public float RadiusCopy;            // Same as radius
    public Vector4 ScaleCopy;           // Same as scale.
    public int? Data;

    public bhkPackedNiTriStripsShape(BinaryReader r, Header h) : base(r, h) {
        SubShapes = r.ReadL16FArray(r => new OblivionSubShape(r, h));
        UserData = r.ReadUInt32();
        Unused1 = r.ReadUInt32();
        Radius = r.ReadSingle();
        Scale = r.ReadVector4();
        RadiusCopy = r.ReadSingle();
        ScaleCopy = r.ReadVector4();
        Data = X<hkPackedNiTriStripsData>.Ref(r);
    }
}

/// <summary>
/// A shape constructed from a bunch of strips.
/// </summary>
public class bhkNiTriStripsShape : bhkShapeCollection {
    public HavokMaterial Material;      // The material of the shape.
    public float Radius;
    public uint[] Unused;               // Garbage data from memory though the last 3 are referred to as maxSize, size, and eSize.
    public uint GrowBy;
    public Vector4 Scale;               // Scale. Usually (1.0, 1.0, 1.0, 0.0).
    public int?[] StripsData;           // Refers to a bunch of NiTriStripsData objects that make up this shape.
    public HavokFilter[] DataLayers;    // Havok Layers for each strip data.

    public bhkNiTriStripsShape(BinaryReader r, Header h) : base(r, h) {
        Material = new HavokMaterial(r, h);
        Radius = r.ReadSingle();
        Unused = r.ReadPArray<uint>("I", 5);
        GrowBy = r.ReadUInt32();
        if (h.V >= 0x0A010000) Scale = r.ReadVector4();
        StripsData = r.ReadL32FArray(X<NiTriStripsData>.Ref);
        DataLayers = r.ReadL32FArray(r => new HavokFilter(r, h));
    }
}

/// <summary>
/// A generic extra data object.
/// </summary>
public class NiExtraData : NiObject {
    public string Name;             // Name of this object.
    public int? NextExtraData;      // Block number of the next extra data object.

    public NiExtraData(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x0A000100 && !BSExtraData) Name = Y.String(r);
        if (h.V <= 0x04020200) NextExtraData = X<NiExtraData>.Ref(r);
    }
}

/// <summary>
/// Abstract base class for all interpolators of bool, float, NiQuaternion, NiPoint3, NiColorA, and NiQuatTransform data.
/// </summary>
public abstract class NiInterpolator(BinaryReader r, Header h) : NiObject(r, h) { } //:X

/// <summary>
/// Abstract base class for interpolators that use NiAnimationKeys (Key, KeyGrp) for interpolation.
/// </summary>
public abstract class NiKeyBasedInterpolator(BinaryReader r, Header h) : NiInterpolator(r, h) { } //:X

/// <summary>
/// Uses NiFloatKeys to animate a float value over time.
/// </summary>
public class NiFloatInterpolator : NiKeyBasedInterpolator { //:X
    public float Value;     // Pose value if lacking NiFloatData.
    public int? Data;

    public NiFloatInterpolator(BinaryReader r, Header h) : base(r, h) {
        Value = r.ReadSingle();
        Data = X<NiFloatData>.Ref(r);
    }
}

/// <summary>
/// An interpolator for transform keyframes.
/// </summary>
public class NiTransformInterpolator : NiKeyBasedInterpolator { //:X
    public NiQuatTransform Transform;
    public int? Data;

    public NiTransformInterpolator(BinaryReader r, Header h) : base(r, h) {
        Transform = new NiQuatTransform(r, h);
        Data = X<NiTransformData>.Ref(r);
    }
}

/// <summary>
/// Uses NiPosKeys to animate an NiPoint3 value over time.
/// </summary>
public class NiPoint3Interpolator : NiKeyBasedInterpolator { //:X
    public Vector3 Value;     // Pose value if lacking NiPosData.
    public int? Data;

    public NiPoint3Interpolator(BinaryReader r, Header h) : base(r, h) {
        Value = r.ReadVector3();
        Data = X<NiPosData>.Ref(r);
    }
}

[Flags]
public enum PathFlags : ushort { //:X
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
public class NiPathInterpolator : NiKeyBasedInterpolator { //:X
    public PathFlags Flags;
    public int BankDir;             // -1 = Negative, 1 = Positive
    public float MaxBankAngle;      // Max angle in radians.
    public float Smoothing;
    public short FollowAxis;        // 0, 1, or 2 representing X, Y, or Z.
    public int? PathData;
    public int? PercentData;

    public NiPathInterpolator(BinaryReader r, Header h) : base(r, h) {
        Flags = (PathFlags)r.ReadUInt16();
        BankDir = r.ReadInt32();
        MaxBankAngle = r.ReadSingle();
        Smoothing = r.ReadSingle();
        FollowAxis = r.ReadInt16();
        PathData = X<NiPosData>.Ref(r);
        PercentData = X<NiFloatData>.Ref(r);
    }
}

/// <summary>
/// Uses NiBoolKeys to animate a bool value over time.
/// </summary>
public class NiBoolInterpolator : NiKeyBasedInterpolator { //:X
    public bool Value;          // Pose value if lacking NiBoolData.
    public int? Data;

    public NiBoolInterpolator(BinaryReader r, Header h) : base(r, h) {
        Value = r.ReadBool32();
        Data = X<NiBoolData>.Ref(r);
    }
}

/// <summary>
/// Uses NiBoolKeys to animate a bool value over time.
/// Unlike NiBoolInterpolator, it ensures that keys have not been missed between two updates.
/// </summary>
public class NiBoolTimelineInterpolator(BinaryReader r, Header h) : NiBoolInterpolator(r, h) { } //:X

public enum InterpBlendFlags : byte { //:X
    MANAGER_CONTROLLED = 1      // MANAGER_CONTROLLED
}

/// <summary>
/// Interpolator item for array in NiBlendInterpolator.
/// </summary>
public class InterpBlendItem { //:X
    public int? Interpolator;           // Reference to an interpolator.
    public float Weight;
    public float NormalizedWeight;
    public int Priority;
    public float EaseSpinner;

    public InterpBlendItem(BinaryReader r, Header h) {
        Interpolator = X<NiInterpolator>.Ref(r);
        Weight = r.ReadSingle();
        NormalizedWeight = r.ReadSingle();
        Priority = h.V <= 0x0A01006D ? r.ReadInt32()
            : h.V >= 0x0A01006E ? r.ReadByte()
            : 0;
        EaseSpinner = r.ReadSingle();
    }
}

/// <summary>
/// Abstract base class for all NiInterpolators that blend the results of sub-interpolators together to compute a final weighted value.
/// </summary>
public abstract class NiBlendInterpolator : NiInterpolator { //:X
    public InterpBlendFlags Flags;
    public ushort ArraySize;
    public ushort ArrayGrowBy;
    public float WeightThreshold;
    // Flags conds
    public ushort InterpCount;
    public ushort SingleIndex = 255;
    public int HighPriority = -128;
    public int NextHighPriority = -128;
    public float SingleTime = -3.402823466e+38f;
    public float HighWeightsSum = -3.402823466e+38f;
    public float NextHighWeightsSum = -3.402823466e+38f;
    public float HighEaseSpinner = -3.402823466e+38f;
    public InterpBlendItem[] InterpArrayItems;
    // end
    public bool ManagedControlled;
    public bool OnlyUseHighestWeight;
    public int? SingleInterpolator;

    public NiBlendInterpolator(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x0A010070) Flags = (InterpBlendFlags)r.ReadByte();
        ArraySize = h.V <= 0x0A01006D ? r.ReadUInt16() : r.ReadByte();
        if (h.V >= 0x0A01006D) ArrayGrowBy = r.ReadUInt16();
        if (h.V >= 0x0A010070) {
            WeightThreshold = r.ReadSingle();
            // Flags conds
            if (Flags.HasFlag(InterpBlendFlags.MANAGER_CONTROLLED)) {
                InterpCount = r.ReadByte();
                SingleIndex = r.ReadByte();
                HighPriority = r.ReadSByte();
                NextHighPriority = r.ReadSByte();
                SingleTime = r.ReadSingle();
                HighWeightsSum = r.ReadByte();
                NextHighWeightsSum = r.ReadByte();
                HighEaseSpinner = r.ReadByte();
                InterpArrayItems = r.ReadFArray(r => new InterpBlendItem(r, h), ArraySize);
            }
        }
        else {
            InterpArrayItems = r.ReadFArray(r => new InterpBlendItem(r, h), ArraySize);
            ManagedControlled = r.ReadBool32();
            WeightThreshold = r.ReadSingle();
            OnlyUseHighestWeight = r.ReadBool32();
            InterpCount = h.V <= 0x0A01006D ? r.ReadUInt16() : r.ReadByte();
            SingleIndex = h.V <= 0x0A01006D ? r.ReadUInt16() : r.ReadByte();
            if (h.V >= 0x0A01006C) {
                SingleInterpolator = X<NiInterpolator>.Ref(r);
                SingleTime = r.ReadSingle();
            }
            HighPriority = h.V <= 0x0A01006D ? r.ReadInt32() : r.ReadByte();
            NextHighPriority = h.V <= 0x0A01006D ? r.ReadInt32() : r.ReadByte();
        }
    }
}

/// <summary>
/// Abstract base class for interpolators storing data via a B-spline.
/// </summary>
public abstract class NiBSplineInterpolator : NiInterpolator { //:X
    public float StartTime;     // Animation start time.
    public float StopTime;      // Animation stop time.
    public int? SplineData;
    public int? BasisData;

    public NiBSplineInterpolator(BinaryReader r, Header h) : base(r, h) {
        StartTime = r.ReadSingle();
        StopTime = r.ReadSingle();
        SplineData = X<NiBSplineData>.Ref(r);
        BasisData = X<NiBSplineBasisData>.Ref(r);
    }
}

/// <summary>
/// Abstract base class for NiObjects that support names, extra data, and time controllers.
/// </summary>
public abstract class NiObjectNET : NiObject { //:M
    [JsonPropertyOrder(1)] public BSLightingShaderPropertyShaderType SkyrimShaderType; // Configures the main shader path
    [JsonPropertyOrder(1)] public string Name;                  // Name of this controllable object, used to refer to the object in .kf files.
    [JsonPropertyOrder(1)] public (string, uint, string) OldExtra; // Extra data for pre-3.0 versions.
    [JsonPropertyOrder(1)] public int? ExtraData;               // Extra data object index. (The first in a chain)
    [JsonPropertyOrder(1)] public int?[] ExtraDataList;         // List of extra data indices.
    [JsonPropertyOrder(1)] public int? Controller;              // Controller object index. (The first in a chain)

    public NiObjectNET(BinaryReader r, Header h) : base(r, h) {
        if (h.UserVersion2 >= 83) SkyrimShaderType = (BSLightingShaderPropertyShaderType)r.ReadUInt32();
        Name = Y.String(r);
        if (h.V <= 0x02030000) {
            if (r.ReadBool32()) OldExtra = (Y.String(r), r.ReadUInt32(), Y.String(r));
            r.Skip(1); // Unknown Byte, Always 0.
        }
        if (h.V >= 0x03000000 && h.V <= 0x04020200) ExtraData = X<NiExtraData>.Ref(r);
        if (h.V >= 0x0A000100) ExtraDataList = r.ReadL16FArray(r => X<NiExtraData>.Ref(r));
        if (h.V >= 0x03000000) Controller = X<NiTimeController>.Ref(r);
    }
}

/// <summary>
/// This is the most common collision object found in NIF files. It acts as a real object that is visible and possibly (if the body allows for it) interactive. The node itself
/// is simple, it only has three properties. For this type of collision object, bhkRigidBody or bhkRigidBodyT is generally used.
/// </summary>
public class NiCollisionObject : NiObject {
    public int? Target;             // Index of the AV object referring to this collision object.

    public NiCollisionObject(BinaryReader r, Header h) : base(r, h) {
        Target = X<NiAVObject>.Ptr(r);
    }
}

/// <summary>
/// Collision box.
/// </summary>
public class NiCollisionData : NiCollisionObject {
    public PropagationMode PropagationMode;
    public CollisionMode CollisionMode;
    public BoundingVolume BoundingVolume;

    public NiCollisionData(BinaryReader r, Header h) : base(r, h) {
        PropagationMode = (PropagationMode)r.ReadUInt32();
        if (h.V >= 0x0A010000) CollisionMode = (CollisionMode)r.ReadUInt32();
        BoundingVolume = r.ReadUInt32() == 1 ? new BoundingVolume(r) : default;
    }
}

/// <summary>
/// bhkNiCollisionObject flags. The flags 0x2, 0x100, and 0x200 are not seen in any NIF nor get/set by the engine.
/// </summary>
[Flags]
public enum bhkCOFlags : ushort { //:X
    ACTIVE = 0,
    //UNK1 = 1 << 1,
    NOTIFY = 1 << 2,
    SET_LOCAL = 1 << 3,
    DBG_DISPLAY = 1 << 4,
    USE_VEL = 1 << 5,
    RESET = 1 << 6,
    SYNC_ON_UPDATE = 1 << 7,
    //UNK2 = 1 << 8,
    //UNK3 = 1 << 9,
    ANIM_TARGETED = 1 << 10,
    DISMEMBERED_LIMB = 1 << 11
}

/// <summary>
/// Havok related collision object?
/// </summary>
public abstract class bhkNiCollisionObject : NiCollisionObject {
    public bhkCOFlags Flags;       // Set to 1 for most objects, and to 41 for animated objects (ANIM_STATIC). Bits: 0=Active 2=Notify 3=Set Local 6=Reset.
    public int? Body;

    public bhkNiCollisionObject(BinaryReader r, Header h) : base(r, h) {
        Flags = (bhkCOFlags)r.ReadUInt16();
        Body = X<bhkWorldObject>.Ref(r);
    }
}

/// <summary>
/// Havok related collision object?
/// </summary>
public class bhkCollisionObject(BinaryReader r, Header h) : bhkNiCollisionObject(r, h) { }

/// <summary>
/// Unknown.
/// </summary>
public abstract class bhkBlendCollisionObject : bhkCollisionObject {
    public float HeirGain;
    public float VelGain;
    public float UnkFloat1;
    public float UnkFloat2;

    public bhkBlendCollisionObject(BinaryReader r, Header h) : base(r, h) {
        HeirGain = r.ReadSingle();
        VelGain = r.ReadSingle();
        if (h.UserVersion2 < 9) {
            UnkFloat1 = r.ReadSingle();
            UnkFloat2 = r.ReadSingle();
        }
    }
}

/// <summary>
/// Unknown.
/// </summary>
public abstract class bhkPCollisionObject(BinaryReader r, Header h) : bhkNiCollisionObject(r, h) { }

/// <summary>
/// Unknown.
/// </summary>
public abstract class bhkSPCollisionObject(BinaryReader r, Header h) : bhkPCollisionObject(r, h) { }

/// <summary>
/// Abstract audio-visual base class from which all of Gamebryo's scene graph objects inherit.
/// </summary>
public abstract class NiAVObject : NiObjectNET {
    public enum F {
        NiFlagsHidden = 0x1
    }

    [JsonPropertyOrder(4)] public Flags Flags;              // Basic flags for AV objects.
    [JsonPropertyOrder(5)] public Vector3 Translation;      // The translation vector.
    [JsonPropertyOrder(6)] public Matrix4x4 Rotation;       // The rotation part of the transformation matrix.
    [JsonPropertyOrder(7)] public float Scale;              // Scaling part (only uniform scaling is supported).
    [JsonPropertyOrder(8)] public Vector3 Velocity;         // Unknown function. Always seems to be (0, 0, 0)
    [JsonPropertyOrder(9)] public int?[] Properties;        // All rendering properties attached to this object.
    [JsonPropertyOrder(10)] public uint[] Unknown1;         // Always 2,0,2,0.
    [JsonPropertyOrder(10)] public byte Unknown2;           // 0 or 1.
    [JsonPropertyOrder(10)] public BoundingVolume BoundingVolume;
    [JsonPropertyOrder(11)] public NiCollisionObject CollisionObject;

    public NiAVObject(BinaryReader r, Header h) : base(r, h) {
        Flags = h.V >= 0x03000000 && h.UserVersion2 <= 26 ? (Flags)r.ReadUInt16()
            : h.UserVersion2 > 26 ? (Flags)r.ReadUInt16()
            : (Flags)14;
        Translation = r.ReadVector3();
        Rotation = r.ReadMatrix3x3As4x4();
        Scale = r.ReadSingle();
        if (h.V <= 0x04020200) Velocity = r.ReadVector3();
        if (h.UserVersion2 <= 34) Properties = r.ReadL32FArray(X<NiProperty>.Ref);
        if (h.V <= 0x02030000) {
            Unknown1 = r.ReadPArray<uint>("I", 4);
            Unknown2 = r.ReadByte();
        }
        if (h.V >= 0x03000000 && h.V <= 0x04020200 && r.ReadBool32()) BoundingVolume = new BoundingVolume(r);
        if (h.V >= 0x0A000100) CollisionObject = new NiCollisionObject(r, h);
    }
}

/// <summary>
/// Abstract base class for dynamic effects such as NiLights or projected texture effects.
/// </summary>
public abstract class NiDynamicEffect : NiAVObject {
    public bool SwitchState;               // If true, then the dynamic effect is applied to affected nodes during rendering.
    public int?[] AffectedNodes;            // If a node appears in this list, then its entire subtree will be affected by the effect.
    public uint[] AffectedNodeListPointers;

    public NiDynamicEffect(BinaryReader r, Header h) : base(r, h) {
        SwitchState = h.V >= 0x0A01006A && h.UserVersion2 < 130 ? r.ReadBool32() : true;
        if (h.V <= 0x04000002) {
            var numAffectedNodes = r.ReadUInt32();
            if (h.V <= 0x0303000D) AffectedNodes = r.ReadFArray(X<NiNode>.Ptr, (int)numAffectedNodes);
            else if (h.V >= 0x04000000) AffectedNodeListPointers = r.ReadPArray<uint>("I", (int)numAffectedNodes);
        }
        else if (h.V >= 0x0A010000 && h.UserVersion2 < 130) AffectedNodes = r.ReadL32FArray(X<NiNode>.Ptr);
    }
}

/// <summary>
/// Abstract base class that represents light sources in a scene graph.
/// For Bethesda Stream 130 (FO4), NiLight now directly inherits from NiAVObject.
/// </summary>
public abstract class NiLight : NiDynamicEffect {
    public float Dimmer;             // Scales the overall brightness of all light components.
    public Color3 AmbientColor;
    public Color3 DiffuseColor;
    public Color3 SpecularColor;

    public NiLight(BinaryReader r, Header h) : base(r, h) {
        Dimmer = r.ReadSingle();
        AmbientColor = new Color3(r);
        DiffuseColor = new Color3(r);
        SpecularColor = new Color3(r);
    }
}

/// <summary>
/// Abstract base class representing all rendering properties. Subclasses are attached to NiAVObjects to control their rendering.
/// </summary>
public abstract class NiProperty(BinaryReader r, Header h) : NiObjectNET(r, h) { }

/// <summary>
/// Unknown
/// </summary>
public class NiTransparentProperty : NiProperty {
    public byte[] Unknown;             // Unknown.

    public NiTransparentProperty(BinaryReader r, Header h) : base(r, h) {
        Unknown = r.ReadBytes(6);
    }
}

/// <summary>
/// Abstract base class for all particle system modifiers.
/// </summary>
public abstract class NiPSysModifier : NiObject {
    public string Name;             // Used to locate the modifier.
    public uint Order;              // Modifier ID in the particle modifier chain (always a multiple of 1000)?
    public int? Target;             // NiParticleSystem parent of this modifier.
    public bool Active;             // Whether or not the modifier is active.

    public NiPSysModifier(BinaryReader r, Header h) : base(r, h) {
        Name = Y.String(r);
        Order = r.ReadUInt32();
        Target = X<NiParticleSystem>.Ptr(r);
        Active = r.ReadBool32();
    }
}

/// <summary>
/// Abstract base class for all particle system emitters.
/// </summary>
public abstract class NiPSysEmitter : NiPSysModifier {
    public float Speed;             // Speed / Inertia of particle movement.
    public float SpeedVariation;    // Adds an amount of randomness to Speed.
    public float Declination;       // Declination / First axis.
    public float DeclinationVariation; // Declination randomness / First axis.
    public float PlanarAngle;       // Planar Angle / Second axis.
    public float PlanarAngleVariation; // Planar Angle randomness / Second axis.
    public Color4 InitialColor;      // Defines color of a birthed particle.
    public float InitialRadius;     // Size of a birthed particle.
    public float RadiusVariation;   // Particle Radius randomness.
    public float LifeSpan;          // Duration until a particle dies.
    public float LifeSpanVariation; // Adds randomness to Life Span.

    public NiPSysEmitter(BinaryReader r, Header h) : base(r, h) {
        Speed = r.ReadSingle();
        SpeedVariation = r.ReadSingle();
        Declination = r.ReadSingle();
        DeclinationVariation = r.ReadSingle();
        PlanarAngle = r.ReadSingle();
        PlanarAngleVariation = r.ReadSingle();
        InitialColor = new Color4(r);
        InitialRadius = r.ReadSingle();
        if (h.V >= 0x0A040001) RadiusVariation = r.ReadSingle();
        LifeSpan = r.ReadSingle();
        LifeSpanVariation = r.ReadSingle();
    }
}


/// <summary>
/// Abstract base class for particle emitters that emit particles from a volume.
/// </summary>
public abstract class NiPSysVolumeEmitter : NiPSysEmitter {
    public int? EmitterObject;             // Node parent of this modifier?

    public NiPSysVolumeEmitter(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010000) EmitterObject = X<NiNode>.Ptr(r);
    }
}

/// <summary>
/// Abstract base class that provides the base timing and update functionality for all the Gamebryo animation controllers.
/// </summary>
public abstract class NiTimeController : NiObject {
    public int? NextController;             // Index of the next controller.
    public Flags Flags;                     // Controller flags.
    public float Frequency;                 // Frequency (is usually 1.0).
    public float Phase;                     // Phase (usually 0.0).
    public float StartTime;                 // Controller start time.
    public float StopTime;                  // Controller stop time.
    public int? Target;                     // Controller target (object index of the first controllable ancestor of this object).
    public int UnknownInt;                  // Unknown integer.

    public NiTimeController(BinaryReader r, Header h) : base(r, h) {
        NextController = X<NiTimeController>.Ref(r);
        Flags = (Flags)r.ReadUInt16();
        Frequency = r.ReadSingle();
        Phase = r.ReadSingle();
        StartTime = r.ReadSingle();
        StopTime = r.ReadSingle();
        if (h.V >= 0x0303000D) Target = X<NiObjectNET>.Ptr(r);
        else if (h.V <= 0x03010000) UnknownInt = r.ReadInt32();
    }
}

/// <summary>
/// DEPRECATED (20.6)
/// </summary>
public class NiMultiTargetTransformController : NiInterpController {
    public int?[] ExtraTargets;         // NiNode Targets to be controlled.

    public NiMultiTargetTransformController(BinaryReader r, Header h) : base(r, h) {
        ExtraTargets = r.ReadL32FArray(X<NiAVObject>.Ptr);
    }
}

/// <summary>
/// DEPRECATED (20.5), replaced by NiMorphMeshModifier.
/// Time controller for geometry morphing.
/// </summary>
public class NiGeomMorpherController : NiInterpController {
    public Flags ExtraFlags;        // 1 = UPDATE NORMALS
    public int? Data;               // Geometry morphing data index.
    public byte AlwaysUpdate;
    public int?[] Interpolators;
    public MorphWeight[] InterpolatorWeights;
    public uint[] UnknownInts;      // Unknown.

    public NiGeomMorpherController(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A000102) ExtraFlags = (Flags)r.ReadUInt16();
        Data = X<NiMorphData>.Ref(r);
        if (h.V >= 0x04000001) AlwaysUpdate = r.ReadByte();
        if (h.V >= 0x0A01006A) {
            if (h.V <= 0x14000005) Interpolators = r.ReadL32FArray(X<NiInterpolator>.Ref);
            else if (h.V >= 0x14010003) InterpolatorWeights = r.ReadL32FArray(r => new MorphWeight(r));
            else r.ReadUInt32();
        }
        if (h.V >= 0x0A020000 && h.V <= 0x14000005 && h.UserVersion2 < 9) UnknownInts = r.ReadL32PArray<uint>("I");
    }
}

/// <summary>
/// Unknown! Used by Daoc->'healing.nif'.
/// </summary>
public class NiMorphController(BinaryReader r, Header h) : NiInterpController(r, h) { }

/// <summary>
/// Unknown! Used by Daoc.
/// </summary>
public class NiMorpherController : NiInterpController {
    public int? Data;         // This controller's data.

    public NiMorpherController(BinaryReader r, Header h) : base(r, h) {
        Data = X<NiMorphData>.Ptr(r);
    }
}

/// <summary>
/// Uses a single NiInterpolator to animate its target value.
/// </summary>
public abstract class NiSingleInterpController : NiInterpController {
    public int? Interpolator;

    public NiSingleInterpController(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010068) Interpolator = X<NiInterpolator>.Ref(r);
    }
}

/// <summary>
/// DEPRECATED (10.2), RENAMED (10.2) to NiTransformController
/// A time controller object for animation key frames.
/// </summary>
public class NiKeyframeController : NiSingleInterpController {
    public int? Data;

    public NiKeyframeController(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x0A010067) Data = X<NiKeyframeData>.Ref(r);
    }
}

/// <summary>
/// NiTransformController replaces NiKeyframeController.
/// </summary>
public class NiTransformController(BinaryReader r, Header h) : NiKeyframeController(r, h) { }

/// <summary>
/// A particle system modifier controller.
/// NiInterpController::GetCtlrID() string format:
/// '%s' Where %s = Value of "Modifier Name"
/// </summary>
public class NiPSysModifierCtlr : NiSingleInterpController {
    public string ModifierName;

    public NiPSysModifierCtlr(BinaryReader r, Header h) : base(r, h) {
        ModifierName = Y.String(r);         // Used to find the modifier pointer.
    }
}

/// <summary>
/// Particle system emitter controller.
/// NiInterpController::GetInterpolatorID() string format:
/// ['BirthRate', 'EmitterActive'] (for "Interpolator" and "Visibility Interpolator" respectively)
/// </summary>
public class NiPSysEmitterCtlr : NiPSysModifierCtlr {
    public int? VisibilityInterpolator;
    public int? Data;

    public NiPSysEmitterCtlr(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010000) VisibilityInterpolator = X<NiInterpolator>.Ref(r);
        if (h.V >= 0x0A010067) Data = X<NiPSysEmitterCtlrData>.Ref(r);
    }
}

/// <summary>
/// A particle system modifier controller that animates a boolean value for particles.
/// </summary>
public abstract class NiPSysModifierBoolCtlr(BinaryReader r, Header h) : NiPSysModifierCtlr(r, h) { }

/// <summary>
/// A particle system modifier controller that animates active/inactive state for particles.
/// </summary>
public class NiPSysModifierActiveCtlr : NiPSysModifierBoolCtlr {
    public int? Data;

    public NiPSysModifierActiveCtlr(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010067) Data = X<NiVisData>.Ref(r);
    }
}

/// <summary>
/// A particle system modifier controller that animates a floating point value for particles.
/// </summary>
public abstract class NiPSysModifierFloatCtlr : NiPSysModifierCtlr {
    public int? Data;

    public NiPSysModifierFloatCtlr(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010067) Data = X<NiFloatData>.Ref(r);
    }
}

/// <summary>
/// Animates the declination value on an NiPSysEmitter object.
/// </summary>
public class NiPSysEmitterDeclinationCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) { }

/// <summary>
/// Animates the declination variation value on an NiPSysEmitter object.
/// </summary>
public class NiPSysEmitterDeclinationVarCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) { }

/// <summary>
/// Animates the size value on an NiPSysEmitter object.
/// </summary>
public class NiPSysEmitterInitialRadiusCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) { }

/// <summary>
/// Animates the lifespan value on an NiPSysEmitter object.
/// </summary>
public class NiPSysEmitterLifeSpanCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) { }

/// <summary>
/// Animates the speed value on an NiPSysEmitter object.
/// </summary>
public class NiPSysEmitterSpeedCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) { }

/// <summary>
/// Animates the strength value of an NiPSysGravityModifier.
/// </summary>
public class NiPSysGravityStrengthCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) { }

/// <summary>
/// Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target float value.
/// </summary>
public abstract class NiFloatInterpController(BinaryReader r, Header h) : NiSingleInterpController(r, h) { }

/// <summary>
/// Changes the image a Map (TexDesc) will use. Uses a float interpolator to animate the texture index.
/// Often used for performing flipbook animation.
/// </summary>
public class NiFlipController : NiFloatInterpController {
    public TexType TextureSlot;     // Target texture slot (0=base, 4=glow).
    //public float StartTime;
    public float Delta;             // Time between two flips. delta = (start_time - stop_time) / sources.num_indices
    public int?[] Sources;          // The texture sources.
    public int?[] Images;           // The image sources

    public NiFlipController(BinaryReader r, Header h) : base(r, h) {
        TextureSlot = (TexType)r.ReadUInt32();
        if (h.V >= 0x0303000D && h.V <= 0x14010067) StartTime = r.ReadSingle();
        if (h.V <= 0x14010067) Delta = r.ReadSingle();
        if (h.V >= 0x04000000) Sources = r.ReadL32FArray(X<NiSourceTexture>.Ref);
        else if (h.V <= 0x03010000) Images = r.ReadL32FArray(X<NiImage>.Ref);
        else r.ReadUInt32();
    }
}

/// <summary>
/// Animates the alpha value of a property using an interpolator.
/// </summary>
public class NiAlphaController : NiFloatInterpController { //:M
    public int? Data;

    public NiAlphaController(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x0A020067) Data = X<NiFloatData>.Ref(r);
    }
}

// HERE


/// <summary>
/// Used to animate a single member of an NiTextureTransform.
/// NiInterpController::GetCtlrID() string formats:
/// ['%1-%2-TT_TRANSLATE_U', '%1-%2-TT_TRANSLATE_V', '%1-%2-TT_ROTATE', '%1-%2-TT_SCALE_U', '%1-%2-TT_SCALE_V']
/// (Depending on "Operation" enumeration, %1 = Value of "Shader Map", %2 = Value of "Texture Slot")
/// </summary>
public class NiTextureTransformController : NiFloatInterpController { //:X

    public NiTextureTransformController(BinaryReader r, Header h) : base(r, h) {
    }
}

/// <summary>
/// Unknown controller.
/// </summary>
public class NiLightDimmerController(BinaryReader r, Header h) : NiFloatInterpController(r, h) { } //:X

/// <summary>
/// Abstract base class for all NiInterpControllers that use a NiInterpolator to animate their target boolean value.
/// </summary>
/// <param name="r"></param>
/// <param name="h"></param>
public abstract class NiBoolInterpController(BinaryReader r, Header h) : NiSingleInterpController(r, h) { } //:X

/// <summary>
/// Animates the visibility of an NiAVObject.
/// </summary>
public class NiVisController : NiBoolInterpController { //:M
    public int? Data;

    public NiVisController(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x0A010067) Data = X<NiVisData>.Ref(r);
    }
}


/// <summary>
/// Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target NiPoint3 value.
/// </summary>
public abstract class NiPoint3InterpController : NiSingleInterpController { //:M
    //public int? Data;

    public NiPoint3InterpController(BinaryReader r, Header h) : base(r, h) {
        //Data = X<NiPosData>.Ref(r);
    }
}

/// <summary>
/// Time controller for material color. Flags are used for color selection in versions below 10.1.0.0.
/// Bits 4-5: Target Color (00 = Ambient, 01 = Diffuse, 10 = Specular, 11 = Emissive)
/// NiInterpController::GetCtlrID() string formats:
/// ['AMB', 'DIFF', 'SPEC', 'SELF_ILLUM'] (Depending on "Target Color")
/// </summary>
public class NiMaterialColorController : NiPoint3InterpController { //:M
    public MaterialColor TargetColor;       // Selects which color to control.
    public int? Data;

    public NiMaterialColorController(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010000) TargetColor = (MaterialColor)r.ReadUInt16();
        if (h.V <= 0x0A010067) Data = X<NiPosData>.Ref(r);
    }
}

/// <summary>
/// Animates the ambient, diffuse and specular colors of an NiLight.
/// NiInterpController::GetCtlrID() string formats:
/// ['Diffuse', 'Ambient'] (Depending on "Target Color")
/// </summary>
public class NiLightColorController : NiPoint3InterpController { //:X
    public LightColor TargetColor;
    public int? Data;

    public NiLightColorController(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010000) TargetColor = (LightColor)r.ReadUInt16();
        if (h.V <= 0x0A010067) Data = X<NiPosData>.Ref(r);
    }
}

/// <summary>
/// Abstract base class for all extra data controllers.
/// NiInterpController::GetCtlrID() string format:
/// '%s' Where %s = Value of "Extra Data Name"
/// </summary>
public abstract class NiExtraDataController : NiSingleInterpController { //:X
    public string ExtraDataName;

    public NiExtraDataController(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A020000) ExtraDataName = Y.String(r);
    }
}

/// <summary>
/// Animates an NiFloatExtraData object attached to an NiAVObject.
/// NiInterpController::GetCtlrID() string format is same as parent.
/// </summary>
public class NiFloatExtraDataController : NiExtraDataController { //:X
    public byte NumExtraBytes;              // Number of extra bytes.
    public byte[] UnknownBytes;             // Unknown.
    public byte[] UnknownExtraBytes;        // Unknown.
    public int? Data;

    public NiFloatExtraDataController(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x0A010000) {
            NumExtraBytes = r.ReadByte();
            UnknownBytes = r.ReadBytes(7);
            UnknownExtraBytes = r.ReadBytes(NumExtraBytes);
        }
        if (h.V <= 0x0A010067) Data = X<NiFloatData>.Ref(r);
    }
}

/// <summary>
/// Animates an NiFloatsExtraData object attached to an NiAVObject.
/// NiInterpController::GetCtlrID() string format:
/// '%s[%d]' Where %s = Value of "Extra Data Name", %d = Value of "Floats Extra Data Index"
/// </summary>
public class NiFloatsExtraDataController : NiExtraDataController { //:X

    public NiFloatsExtraDataController(BinaryReader r, Header h) : base(r, h) {
    }
}

/// <summary>
/// Animates an NiFloatsExtraData object attached to an NiAVObject.
/// NiInterpController::GetCtlrID() string format:
/// '%s[%d]' Where %s = Value of "Extra Data Name", %d = Value of "Floats Extra Data Index"
/// </summary>
public class NiFloatsExtraDataPoint3Controller : NiExtraDataController { //:X

    public NiFloatsExtraDataPoint3Controller(BinaryReader r, Header h) : base(r, h) {
    }
}

/// <summary>
/// DEPRECATED (20.5), Replaced by NiSkinningLODController.
/// Level of detail controller for bones.  Priority is arranged from low to high.
/// </summary>
public class NiBoneLODController : NiTimeController { //:X

    public NiBoneLODController(BinaryReader r, Header h) : base(r, h) {
    }
}

/// <summary>
/// A simple LOD controller for bones.
/// </summary>
public class NiBSBoneLODController(BinaryReader r, Header h) : NiBoneLODController(r, h) { } //:X

public class MaterialData { //:X

    public MaterialData(BinaryReader r, Header h) {
    }
}

/// <summary>
/// Describes a visible scene element with vertices like a mesh, a particle system, lines, etc.
/// </summary>
public abstract class NiGeometry : NiAVObject {
    public int? Data;
    public int? SkinInstance;

    public NiGeometry(BinaryReader r, Header h) : base(r, h) {
        Data = X<NiGeometryData>.Ref(r);
        SkinInstance = X<NiSkinInstance>.Ref(r);
    }
}

/// <summary>
/// Describes a mesh, built from triangles.
/// </summary>
public abstract class NiTriBasedGeom(BinaryReader r, Header h) : NiGeometry(r, h) { }

public enum VectorFlags : ushort { //:X

}

public enum BSVectorFlags : ushort { //:X

}



















public abstract class NiGeometryData : NiObject {
    public ushort NumVertices;
    public bool HasVertices;
    public Vector3[] Vertices;
    public bool HasNormals;
    public Vector3[] Normals;
    public Vector3 Center;
    public float Radius;
    public bool HasVertexColors;
    public Color4[] VertexColors;
    public ushort NumUVSets;
    public bool HasUV;
    [JsonIgnore] public TexCoord[,] UVSets;

    public NiGeometryData(BinaryReader r, Header h) : base(r, h) {
        NumVertices = r.ReadUInt16();
        HasVertices = r.ReadBool32();
        if (HasVertices) Vertices = r.ReadFArray(r => r.ReadVector3(), NumVertices);
        HasNormals = r.ReadBool32();
        if (HasNormals) Normals = r.ReadFArray(r => r.ReadVector3(), NumVertices);
        Center = r.ReadVector3();
        Radius = r.ReadSingle();
        HasVertexColors = r.ReadBool32();
        if (HasVertexColors) VertexColors = r.ReadFArray(r => new Color4(r), NumVertices);
        NumUVSets = r.ReadUInt16();
        HasUV = r.ReadBool32();
        if (HasUV) {
            UVSets = new TexCoord[NumUVSets, NumVertices];
            for (var i = 0; i < NumUVSets; i++)
                for (var j = 0; j < NumVertices; j++) UVSets[i, j] = new TexCoord(r);
        }
    }
}



public abstract class NiTriBasedGeomData : NiGeometryData {
    public ushort NumTriangles;

    public NiTriBasedGeomData(BinaryReader r, Header h) : base(r, h) {
        NumTriangles = r.ReadUInt16();
    }
}

public class NiTriShape(BinaryReader r, Header h) : NiTriBasedGeom(r, h) { }

public class NiTriShapeData : NiTriBasedGeomData {
    public uint NumTrianglePoints;
    public Triangle[] Triangles;
    public MatchGroup[] MatchGroups;

    public NiTriShapeData(BinaryReader r, Header h) : base(r, h) {
        NumTrianglePoints = r.ReadUInt32();
        Triangles = r.ReadFArray(r => new Triangle(r), NumTriangles);
        MatchGroups = r.ReadL16FArray(r => new MatchGroup(r));
    }
}




// Nodes
public class NiNode : NiAVObject {
    [JsonPropertyOrder(12)] public int?[] Children;
    [JsonPropertyOrder(13)] public int?[] Effects;

    public NiNode(BinaryReader r, Header h) : base(r, h) {
        Children = r.ReadL32FArray(X<NiAVObject>.Ref);
        if (h.UserVersion2 < 130) Effects = r.ReadL32FArray(X<NiDynamicEffect>.Ref);
    }
}
public class RootCollisionNode(BinaryReader r, Header h) : NiNode(r, h) { }
public class NiBSAnimationNode(BinaryReader r, Header h) : NiNode(r, h) { }
public class NiBSParticleNode(BinaryReader r, Header h) : NiNode(r, h) { }
public class NiBillboardNode(BinaryReader r, Header h) : NiNode(r, h) { }
public class AvoidNode(BinaryReader r, Header h) : NiNode(r, h) { }

// Geometry

/// <summary>
/// Abstract base class for all NiTimeController objects using NiInterpolator objects to animate their target objects.
/// </summary>
public abstract class NiInterpController : NiTimeController {
    public bool ManagerControlled;

    public NiInterpController(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010068 && h.V <= 0x0A01006C) ManagerControlled = r.ReadBool32();
    }
}

// Properties

public class NiTexturingProperty : NiProperty {
    public ApplyMode ApplyMode;
    public uint TextureCount;
    public TexDesc BaseTexture;
    public TexDesc DarkTexture;
    public TexDesc DetailTexture;
    public TexDesc GlossTexture;
    public TexDesc GlowTexture;
    public TexDesc BumpMapTexture;
    public TexDesc Decal0Texture;

    public NiTexturingProperty(BinaryReader r, Header h) : base(r, h) {
        Flags = NiReaderUtils.ReadFlags(r);
        ApplyMode = (ApplyMode)r.ReadUInt32();
        TextureCount = r.ReadUInt32();
        BaseTexture = r.ReadBool32() ? new TexDesc(r, h) : default;
        DarkTexture = r.ReadBool32() ? new TexDesc(r, h) : default;
        DetailTexture = r.ReadBool32() ? new TexDesc(r, h) : default;
        GlossTexture = r.ReadBool32() ? new TexDesc(r, h) : default;
        GlowTexture = r.ReadBool32() ? new TexDesc(r, h) : default;
        BumpMapTexture = r.ReadBool32() ? new TexDesc(r, h) : default;
        Decal0Texture = r.ReadBool32() ? new TexDesc(r, h) : default;
    }
}

public class NiAlphaProperty : NiProperty {
    public ushort Flags;
    public byte Threshold;

    public NiAlphaProperty(BinaryReader r, Header h) : base(r, h) {
        Flags = r.ReadUInt16();
        Threshold = r.ReadByte();
    }
}

public class NiZBufferProperty : NiProperty {
    public ushort Flags;

    public NiZBufferProperty(BinaryReader r, Header h) : base(r, h) {
        Flags = r.ReadUInt16();
    }
}

public class NiVertexColorProperty : NiProperty {
    public NiAVObject.NiFlags Flags;
    public VertMode VertexMode;
    public LightMode LightingMode;

    public NiVertexColorProperty(BinaryReader r, Header h) : base(r, h) {
        Flags = NiReaderUtils.ReadFlags(r);
        VertexMode = (VertMode)r.ReadUInt32();
        LightingMode = (LightMode)r.ReadUInt32();
    }
}



public class NiWireframeProperty : NiProperty {
    public NiAVObject.NiFlags Flags;

    public NiWireframeProperty(BinaryReader r, Header h) : base(r, h) {
        Flags = NiReaderUtils.ReadFlags(r);
    }
}

public class NiCamera(BinaryReader r, Header h) : NiAVObject(r, h) { }

// Data
public class NiUVData : NiObject {
    public KeyGroup<float>[] UVGroups;

    public NiUVData(BinaryReader r, Header h) : base(r, h) {
        UVGroups = r.ReadFArray(r => new KeyGroup<float>(r), 4);
    }
}

public class NiKeyframeData : NiObject {
    public uint NumRotationKeys;
    public KeyType RotationType;
    public QuatKey<Quaternion>[] QuaternionKeys;
    public float UnknownFloat;
    public KeyGroup<float>[] XYZRotations;
    public KeyGroup<Vector3> Translations;
    public KeyGroup<float> Scales;

    public NiKeyframeData(BinaryReader r, Header h) : base(r, h) {
        NumRotationKeys = r.ReadUInt32();
        if (NumRotationKeys != 0) {
            RotationType = (KeyType)r.ReadUInt32();
            if (RotationType != KeyType.XYZ_ROTATION_KEY)
                QuaternionKeys = r.ReadFArray(r => new QuatKey<Quaternion>(r, RotationType), (int)NumRotationKeys);
            else {

                UnknownFloat = r.ReadSingle();
                XYZRotations = r.ReadFArray(r => new KeyGroup<float>(r), 3);
            }
        }
        Translations = new KeyGroup<Vector3>(r);
        Scales = new KeyGroup<float>(r);
    }
}

public class NiColorData : NiObject {
    public KeyGroup<Color4> Data;

    public NiColorData(BinaryReader r, Header h) : base(r, h) {
        Data = new KeyGroup<Color4>(r);
    }
}

public class NiMorphData : NiObject {
    public uint NumMorphs;
    public uint NumVertices;
    public byte RelativeTargets;
    public Morph[] Morphs;

    public NiMorphData(BinaryReader r, Header h) : base(r, h) {
        NumMorphs = r.ReadUInt32();
        NumVertices = r.ReadUInt32();
        RelativeTargets = r.ReadByte();
        Morphs = r.ReadFArray(r => new Morph(r, NumVertices), (int)NumMorphs);
    }
}

public class NiVisData : NiObject {
    //public uint NumKeys;
    public Key<byte>[] Keys;

    public NiVisData(BinaryReader r, Header h) : base(r, h) {
        Keys = r.ReadL32FArray(r => new Key<byte>(r, KeyType.LINEAR_KEY));
    }
}

public class NiFloatData : NiObject {
    public KeyGroup<float> Data;

    public NiFloatData(BinaryReader r, Header h) : base(r, h) {
        Data = new KeyGroup<float>(r);
    }
}

public class NiPosData : NiObject {
    public KeyGroup<Vector3> Data;

    public NiPosData(BinaryReader r, Header h) : base(r, h) {
        Data = new KeyGroup<Vector3>(r);
    }
}



public class NiStringExtraData : NiExtraData {
    public uint BytesRemaining;
    public string Str;

    public NiStringExtraData(BinaryReader r, Header h) : base(r, h) {
        BytesRemaining = r.ReadUInt32();
        Str = Y.String(r);
    }
}

public class NiTextKeyExtraData : NiExtraData {
    public uint UnknownInt1;
    //public uint NumTextKeys;
    public Key<string>[] TextKeys;

    public NiTextKeyExtraData(BinaryReader r, Header h) : base(r, h) {
        UnknownInt1 = r.ReadUInt32();
        TextKeys = r.ReadL32FArray(r => new Key<string>(r, KeyType.LINEAR_KEY));
    }
}

public class NiVertWeightsExtraData : NiExtraData {
    public uint NumBytes;
    public ushort NumVertices;
    public float[] Weights;

    public NiVertWeightsExtraData(BinaryReader r, Header h) : base(r, h) {
        NumBytes = r.ReadUInt32();
        NumVertices = r.ReadUInt16();
        Weights = r.ReadPArray<float>("f", NumVertices);
    }
}

// Controllers

public class NiUVController : NiTimeController {
    public ushort UnknownShort;
    public int? Data;

    public NiUVController(BinaryReader r, Header h) : base(r, h) {
        UnknownShort = r.ReadUInt16();
        Data = X<NiUVData>.Ref(r);
    }
}


// Particles
public class NiParticles(BinaryReader r, Header h) : NiGeometry(r, h) { }
public class NiParticlesData : NiGeometryData {
    public ushort NumParticles;
    public float ParticleRadius;
    public ushort NumActive;
    public float[] Sizes;

    public NiParticlesData(BinaryReader r, Header h) : base(r, h) {
        NumParticles = r.ReadUInt16();
        ParticleRadius = r.ReadSingle();
        NumActive = r.ReadUInt16();
        Sizes = r.ReadBool32() ? r.ReadPArray<float>("f", NumVertices) : null;
    }
}

public class NiRotatingParticles(BinaryReader r, Header h) : NiParticles(r, h) { }
public class NiRotatingParticlesData : NiParticlesData {
    public Quaternion[] Rotations;

    public NiRotatingParticlesData(BinaryReader r, Header h) : base(r, h) {
        Rotations = r.ReadBool32() ? r.ReadFArray(r => r.ReadQuaternionWFirst(), NumVertices) : [];
    }
}

public class NiAutoNormalParticles(BinaryReader r, Header h) : NiParticles(r, h) { }
public class NiAutoNormalParticlesData(BinaryReader r, Header h) : NiParticlesData(r, h) { }

public class NiParticleSystemController : NiTimeController {
    public float Speed;
    public float SpeedRandom;
    public float VerticalDirection;
    public float VerticalAngle;
    public float HorizontalDirection;
    public float HorizontalAngle;
    public Vector3 UnknownNormal;
    public Color4 UnknownColor;
    public float Size;
    public float EmitStartTime;
    public float EmitStopTime;
    public byte UnknownByte;
    public float EmitRate;
    public float Lifetime;
    public float LifetimeRandom;
    public ushort EmitFlags;
    public Vector3 StartRandom;
    public int? Emitter;
    public ushort UnknownShort2;
    public float UnknownFloat13;
    public uint UnknownInt1;
    public uint UnknownInt2;
    public ushort UnknownShort3;
    public ushort NumParticles;
    public ushort NumValid;
    public Particle[] Particles;
    public int? UnknownLink;
    public int? ParticleExtra;
    public int? UnknownLink2;
    public byte Trailer;

    public NiParticleSystemController(BinaryReader r, Header h) : base(r, h) {
        Speed = r.ReadSingle();
        SpeedRandom = r.ReadSingle();
        VerticalDirection = r.ReadSingle();
        VerticalAngle = r.ReadSingle();
        HorizontalDirection = r.ReadSingle();
        HorizontalAngle = r.ReadSingle();
        UnknownNormal = r.ReadVector3();
        UnknownColor = new Color4(r);
        Size = r.ReadSingle();
        EmitStartTime = r.ReadSingle();
        EmitStopTime = r.ReadSingle();
        UnknownByte = r.ReadByte();
        EmitRate = r.ReadSingle();
        Lifetime = r.ReadSingle();
        LifetimeRandom = r.ReadSingle();
        EmitFlags = r.ReadUInt16();
        StartRandom = r.ReadVector3();
        Emitter = X<NiObject>.Ptr(r);
        UnknownShort2 = r.ReadUInt16();
        UnknownFloat13 = r.ReadSingle();
        UnknownInt1 = r.ReadUInt32();
        UnknownInt2 = r.ReadUInt32();
        UnknownShort3 = r.ReadUInt16();
        NumParticles = r.ReadUInt16();
        NumValid = r.ReadUInt16();
        Particles = r.ReadFArray(r => new Particle(r), NumParticles);
        UnknownLink = X<NiObject>.Ref(r);
        ParticleExtra = X<NiParticleModifier>.Ref(r);
        UnknownLink2 = X<NiObject>.Ref(r);
        Trailer = r.ReadByte();
    }
}

public class NiBSPArrayController(BinaryReader r, Header h) : NiParticleSystemController(r, h) { }

// Particle Modifiers


public class NiGravity : NiParticleModifier {
    public float UnknownFloat1;
    public float Force;
    public FieldType Type;
    public Vector3 Position;
    public Vector3 Direction;

    public NiGravity(BinaryReader r, Header h) : base(r, h) {
        UnknownFloat1 = r.ReadSingle();
        Force = r.ReadSingle();
        Type = (FieldType)r.ReadUInt32();
        Position = r.ReadVector3();
        Direction = r.ReadVector3();
    }
}

public class NiParticleBomb : NiParticleModifier {
    public float Decay;
    public float Duration;
    public float DeltaV;
    public float Start;
    public DecayType DecayType;
    public Vector3 Position;
    public Vector3 Direction;

    public NiParticleBomb(BinaryReader r, Header h) : base(r, h) {
        Decay = r.ReadSingle();
        Duration = r.ReadSingle();
        DeltaV = r.ReadSingle();
        Start = r.ReadSingle();
        DecayType = (DecayType)r.ReadUInt32();
        Position = r.ReadVector3();
        Direction = r.ReadVector3();
    }
}

public class NiParticleColorModifier : NiParticleModifier {
    public int? ColorData;

    public NiParticleColorModifier(BinaryReader r, Header h) : base(r, h) {
        ColorData = X<NiColorData>.Ref(r);
    }
}

public class NiParticleGrowFade : NiParticleModifier {
    public float Grow;
    public float Fade;

    public NiParticleGrowFade(BinaryReader r, Header h) : base(r, h) {
        Grow = r.ReadSingle();
        Fade = r.ReadSingle();
    }
}

public class NiParticleMeshModifier : NiParticleModifier {
    //public uint NumParticleMeshes;
    public int?[] ParticleMeshes;

    public NiParticleMeshModifier(BinaryReader r, Header h) : base(r, h) {
        ParticleMeshes = r.ReadL32FArray(r => X<NiAVObject>.Ref(r));
    }
}

public class NiParticleRotation : NiParticleModifier {
    public byte RandomInitialAxis;
    public Vector3 InitialAxis;
    public float RotationSpeed;

    public NiParticleRotation(BinaryReader r, Header h) : base(r, h) {
        RandomInitialAxis = r.ReadByte();
        InitialAxis = r.ReadVector3();
        RotationSpeed = r.ReadSingle();
    }
}

/// <summary>
/// DEPRECATED (pre-10.1), REMOVED (20.3).
/// Keyframe animation root node, in .kf files.
/// </summary>
public class NiSequenceStreamHelper(BinaryReader r, Header h) : NiObjectNET(r, h) { }

/// <summary>
/// Determines whether flat shading or smooth shading is used on a shape.
/// </summary>
public class NiShadeProperty(BinaryReader r, Header h) : NiProperty(r, h) {
    public NiAVObject.NiFlags Flags = h.UserVersion2 >= 32 ? NiReaderUtils.ReadFlags(r) : (NiAVObject.NiFlags)1; // Bit 0: Enable smooth phong shading on this shape. Otherwise, hard-edged flat shading will be used on this shape.
}

/// <summary>
/// Skinning data.
/// </summary>
public class NiSkinData : NiObject {
    public NiTransform SkinTransform;       // Offset of the skin from this bone in bind position.
    public uint NumBones;                   // Number of bones.
    public int? SkinPartition;              // This optionally links a NiSkinPartition for hardware-acceleration information.
    public BoneData[] BoneList;             // Contains offset data for each node that this skin is influenced by.

    public NiSkinData(BinaryReader r, Header h) : base(r, h) {
        SkinTransform = new NiTransform(r);
        NumBones = r.ReadUInt32();
        if (h.V >= 0x04000002 && h.V <= 0x0A010000) SkinPartition = X<NiSkinPartition>.Ref(r);
        if (h.V < 0x04020100 || r.ReadBool32()) BoneList = r.ReadFArray(r => new BoneData(r), (int)NumBones);
    }
}

/// <summary>
/// Skinning instance.
/// </summary>
public class NiSkinInstance : NiObject {
    public int? Data;                       // Skinning data reference.
    public int? SkinPartition;              // Refers to a NiSkinPartition objects, which partitions the mesh such that every vertex is only influenced by a limited number of bones.
    public int? SkeletonRoot;               // Armature root node.
    public int?[] Bones;                    // List of all armature bones.

    public NiSkinInstance(BinaryReader r, Header h) : base(r, h) {
        Data = X<NiSkinData>.Ref(r);
        if (h.V >= 0x0A010165) SkinPartition = X<NiSkinPartition>.Ptr(r);
        SkeletonRoot = X<NiNode>.Ptr(r);
        Bones = r.ReadL32FArray(X<NiNode>.Ptr);
    }
}

public class NiSkinPartition(BinaryReader r, Header h) : NiObject(r, h) { }

// Miscellaneous
public abstract class NiTexture(BinaryReader r, Header h) : NiObjectNET(r, h) { }

public class NiSourceTexture : NiTexture {
    public byte UseExternal;
    public string FileName;
    public PixelLayout PixelLayout;
    public MipMapFormat UseMipMaps;
    public AlphaFormat AlphaFormat;
    public byte IsStatic;

    public NiSourceTexture(BinaryReader r, Header h) : base(r, h) {
        UseExternal = r.ReadByte();
        FileName = r.ReadL32Encoding();
        PixelLayout = (PixelLayout)r.ReadUInt32();
        UseMipMaps = (MipMapFormat)r.ReadUInt32();
        AlphaFormat = (AlphaFormat)r.ReadUInt32();
        IsStatic = r.ReadByte();
    }
}



public class NiMaterialProperty : NiProperty {
    public NiAVObject.NiFlags Flags;
    public Color3 AmbientColor;
    public Color3 DiffuseColor;
    public Color3 SpecularColor;
    public Color3 EmissiveColor;
    public float Glossiness;
    public float Alpha;

    public NiMaterialProperty(BinaryReader r, Header h) : base(r, h) {
        Flags = NiReaderUtils.ReadFlags(r);
        AmbientColor = new Color3(r);
        DiffuseColor = new Color3(r);
        SpecularColor = new Color3(r);
        EmissiveColor = new Color3(r);
        Glossiness = r.ReadSingle();
        Alpha = r.ReadSingle();
    }
}



public class NiTextureEffect : NiDynamicEffect {
    public Matrix4x4 ModelProjectionMatrix;
    public Vector3 ModelProjectionTransform;
    public TexFilterMode TextureFiltering;
    public TexClampMode TextureClamping;
    public TextureType TextureType;
    public CoordGenType CoordinateGenerationType;
    public int? SourceTexture;
    public byte ClippingPlane;
    public Vector3 UnknownVector;
    public float UnknownFloat;
    public short PS2L;
    public short PS2K;
    public ushort UnknownShort;

    public NiTextureEffect(BinaryReader r, Header h) : base(r, h) {
        ModelProjectionMatrix = r.ReadMatrix3x3As4x4();
        ModelProjectionTransform = r.ReadVector3();
        TextureFiltering = (TexFilterMode)r.ReadUInt32();
        TextureClamping = (TexClampMode)r.ReadUInt32();
        TextureType = (TextureType)r.ReadUInt32();
        CoordinateGenerationType = (CoordGenType)r.ReadUInt32();
        SourceTexture = X<NiSourceTexture>.Ref(r);
        ClippingPlane = r.ReadByte();
        UnknownVector = r.ReadVector3();
        UnknownFloat = r.ReadSingle();
        PS2L = r.ReadInt16();
        PS2K = r.ReadInt16();
        UnknownShort = r.ReadUInt16();
    }
}

#endregion