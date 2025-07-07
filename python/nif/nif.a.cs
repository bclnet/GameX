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
    public static bool IsVersionSupported(uint v) => true;
    public static (string, uint) ParseHeaderStr(string s) {
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
    public static string Ver2Str(uint v) {
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
    public static uint Ver2Num(string s) {
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
public enum ApplyMode : uint { // X
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
public enum KeyType : uint { // X
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
    FO_HAV_MAT_BROKEN_CONCRETE = 19,// Broken Concrete
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
    FO_HAV_MAT_BARREL_PLATFORM = 55,// Barrel
    FO_HAV_MAT_BOTTLE_PLATFORM = 56,// Bottle
    FO_HAV_MAT_SODA_CAN_PLATFORM = 57, // Soda Can
    FO_HAV_MAT_PISTOL_PLATFORM = 58,// Pistol
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
    FO_HAV_MAT_ELEVATOR_STAIRS = 79,// Elevator
    FO_HAV_MAT_HOLLOW_METAL_STAIRS = 80, // Hollow Metal
    FO_HAV_MAT_SHEET_METAL_STAIRS = 81, // Sheet Metal
    FO_HAV_MAT_SAND_STAIRS = 82,    // Sand
    FO_HAV_MAT_BROKEN_CONCRETE_STAIRS = 83, // Broken Concrete
    FO_HAV_MAT_VEHICLE_BODY_STAIRS = 84, // Vehicle Body
    FO_HAV_MAT_VEHICLE_PART_SOLID_STAIRS = 85, // Vehicle Part Solid
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_STAIRS = 86, // Vehicle Part Hollow
    FO_HAV_MAT_BARREL_STAIRS = 87,  // Barrel
    FO_HAV_MAT_BOTTLE_STAIRS = 88,  // Bottle
    FO_HAV_MAT_SODA_CAN_STAIRS = 89,// Soda Can
    FO_HAV_MAT_PISTOL_STAIRS = 90,  // Pistol
    FO_HAV_MAT_RIFLE_STAIRS = 91,   // Rifle
    FO_HAV_MAT_SHOPPING_CART_STAIRS = 92, // Shopping Cart
    FO_HAV_MAT_LUNCHBOX_STAIRS = 93,// Lunchbox
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
    SKY_HAV_MAT_DRAGON = 2518321175,// Dragon
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
    FOL_TRANSPARENT_SMALL_ANIM = 28,// TransparentSmallAnim (white)
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
public enum PixelLayout : uint { // X
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
public enum MipMapFormat : uint { // X
    MIP_FMT_NO = 0,                 // Texture does not use mip maps.
    MIP_FMT_YES = 1,                // Texture uses mip maps.
    MIP_FMT_DEFAULT = 2             // Use default setting.
}

/// <summary>
/// Describes how transparency is handled in an NiTexture.
/// </summary>
public enum AlphaFormat : uint { // X
    ALPHA_NONE = 0,                 // No alpha.
    ALPHA_BINARY = 1,               // 1-bit alpha.
    ALPHA_SMOOTH = 2,               // Interpolated 4- or 8-bit alpha.
    ALPHA_DEFAULT = 3               // Use default setting.
}

/// <summary>
/// Describes the availiable texture clamp modes, i.e. the behavior of UV mapping outside the [0,1] range.
/// </summary>
public enum TexClampMode : uint { // X
    CLAMP_S_CLAMP_T = 0,            // Clamp in both directions.
    CLAMP_S_WRAP_T = 1,             // Clamp in the S(U) direction but wrap in the T(V) direction.
    WRAP_S_CLAMP_T = 2,             // Wrap in the S(U) direction but clamp in the T(V) direction.
    WRAP_S_WRAP_T = 3               // Wrap in both directions.
}

/// <summary>
/// Describes the availiable texture filter modes, i.e. the way the pixels in a texture are displayed on screen.
/// </summary>
public enum TexFilterMode : uint { // X
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
public enum VertMode : uint { // X
    VERT_MODE_SRC_IGNORE = 0,       // Emissive, ambient, and diffuse colors are all specified by the NiMaterialProperty.
    VERT_MODE_SRC_EMISSIVE = 1,     // Emissive colors are specified by the source vertex colors. Ambient+Diffuse are specified by the NiMaterialProperty.
    VERT_MODE_SRC_AMB_DIF = 2       // Ambient+Diffuse colors are specified by the source vertex colors. Emissive is specified by the NiMaterialProperty. (Default)
}

/// <summary>
/// Describes which lighting equation components influence the final vertex color for NiVertexColorProperty.
/// </summary>
public enum LightMode : uint { // X
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
public enum FieldType : uint { // X
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
    SOLVER_DEACTIVATION_INVALID = 0,// Invalid
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
public enum DecayType : uint { // X
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
public enum TextureType : uint { // X
    TEX_PROJECTED_LIGHT = 0,        // Apply a projected light texture. Each light effect is summed before multiplying by the base texture.
    TEX_PROJECTED_SHADOW = 1,       // Apply a projected shadow texture. Each shadow effect is multiplied by the base texture.
    TEX_ENVIRONMENT_MAP = 2,        // Apply an environment map texture. Added to the base texture and light/shadow/decal maps.
    TEX_FOG_MAP = 3                 // Apply a fog map texture. Alpha channel is used to blend the color channel with the base texture.
}

/// <summary>
/// Determines the way that UV texture coordinates are generated.
/// </summary>
public enum CoordGenType : uint { // X
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
    SBP_56_MOD_CHEST_SECONDARY = 56,// Chest secondary or undergarment
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
    BP_TORSOSECTION_LEFTARM2 = 4000,// Torso Section | Left Arm 2
    BP_TORSOSECTION_RIGHTARM = 5000,// Torso Section | Right Arm
    BP_TORSOSECTION_RIGHTARM2 = 6000, // Torso Section | Right Arm 2
    BP_TORSOSECTION_LEFTLEG = 7000, // Torso Section | Left Leg
    BP_TORSOSECTION_LEFTLEG2 = 8000,// Torso Section | Left Leg 2
    BP_TORSOSECTION_LEFTLEG3 = 9000,// Torso Section | Left Leg 3
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
public class Footer(BinaryReader r, Header h) { // X
    public int?[] Roots = h.V >= 0x0303000D ? r.ReadL32FArray(X<NiObject>.Ref) : default; // List of root NIF objects. If there is a camera, for 1st person view, then this NIF object is referred to as well in this list, even if it is not a root object (usually we want the camera to be attached to the Bip Head node).
}

/// <summary>
/// The distance range where a specific level of detail applies.
/// </summary>
public class LODRange(BinaryReader r, Header h) {
    public float NearExtent = r.ReadSingle();           // Begining of range.
    public float FarExtent = r.ReadSingle();            // End of Range.
    public uint[] UnknownInts = h.V <= 0x03010000 ? r.ReadPArray<uint>("I", 3) : default; // Unknown (0,0,0).
}

/// <summary>
/// Group of vertex indices of vertices that match.
/// </summary>
public class MatchGroup(BinaryReader r) { // X
    public ushort[] VertexIndices = r.ReadL16PArray<ushort>("H"); // The vertex indices.
}

// ByteVector3 -> new Vector3<byte>(r.ReadByte(), r.ReadByte(), r.ReadByte())
// HalfVector3 -> new Vector3(r.ReadHalf(), r.ReadHalf(), r.ReadHalf())
// Vector3 -> r.ReadVector3()
// Vector4 -> r.ReadVector4()
// Quaternion -> r.ReadQuaternion()
// hkQuaternion -> r.ReadQuaternionWFirst()
// Matrix22 -> r.ReadMatrix2x2()
// Matrix33 -> r.ReadMatrix3x3()
// Matrix34 -> r.ReadMatrix3x4()
// Matrix44 -> r.ReadMatrix4x4()
// hkMatrix3 -> r.ReadMatrix3x4()
// MipMap -> new MipMap(r)
// NodeSet -> new NodeSet(r)
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
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BoneVertData { // X
    public static (string, int) Struct = ("<Hf", 7);
    public ushort Index;                                // The vertex index, in the mesh.
    public float Weight;                                // The vertex weight - between 0.0 and 1.0

    public BoneVertData() {}
    public BoneVertData(BinaryReader r) {
        Index = r.ReadUInt16();
        Weight = r.ReadSingle();
    }
    public BoneVertData(BinaryReader r, bool half) { Index = r.ReadUInt16(); Weight = r.ReadHalf(); }
}

// BoneVertDataHalf -> new BoneVertData(r, true)

/// <summary>
/// Used in NiDefaultAVObjectPalette.
/// </summary>
public class AVObject(BinaryReader r) {
    public string Name = r.ReadL32AString();            // Object name.
    public int? AVObject_ = X<NiAVObject>.Ptr(r);       // Object reference.
}

/// <summary>
/// In a .kf file, this links to a controllable object, via its name (or for version 10.2.0.0 and up, a link and offset to a NiStringPalette that contains the name), and a sequence of interpolators that apply to this controllable object, via links.
/// For Controller ID, NiInterpController::GetCtlrID() virtual function returns a string formatted specifically for the derived type.
/// For Interpolator ID, NiInterpController::GetInterpolatorID() virtual function returns a string formatted specifically for the derived type.
/// The string formats are documented on the relevant niobject blocks.
/// </summary>
public class ControlledBlock {
    public string TargetName;                           // Name of a controllable object in another NIF file.
    // NiControllerSequence::InterpArrayItem
    public int? Interpolator;
    public int? Controller;
    public int? BlendInterpolator;
    public ushort BlendIndex;
    // Bethesda-only
    public byte Priority;                               // Idle animations tend to have low values for this, and high values tend to correspond with the important parts of the animations.
    // NiControllerSequence::IDTag, post-10.1.0.104 only
    public string NodeName;                             // The name of the animated NiAVObject.
    public string PropertyType;                         // The RTTI type of the NiProperty the controller is attached to, if applicable.
    public string ControllerType;                       // The RTTI type of the NiTimeController.
    public string ControllerID;                         // An ID that can uniquely identify the controller among others of the same type on the same NiObjectNET.
    public string InterpolatorID;                       // An ID that can uniquely identify the interpolator among others of the same type on the same NiObjectNET.

    public ControlledBlock(BinaryReader r, Header h) {
        if (h.V <= 0x0A010067) TargetName = Y.String(r);
        // NiControllerSequence::InterpArrayItem
        if (h.V >= 0x0A01006A) Interpolator = X<NiInterpolator>.Ref(r);
        if (h.V <= 0x14050000) Controller = X<NiTimeController>.Ref(r);
        if (h.V >= 0x0A010068 && h.V <= 0x0A01006E) {
            BlendInterpolator = X<NiBlendInterpolator>.Ref(r);
            BlendIndex = r.ReadUInt16();
        }
        // Bethesda-only
        if (h.V >= 0x0A01006A && (h.UV2 > 0)) Priority = r.ReadByte();
        // NiControllerSequence::IDTag, post-10.1.0.104 only
        if (h.V >= 0x0A010068 && h.V <= 0x0A010071) {
            NodeName = Y.String(r);
            PropertyType = Y.String(r);
            ControllerType = Y.String(r);
            ControllerID = Y.String(r);
            InterpolatorID = Y.String(r);
        }
        else if (h.V >= 0x0A020000 && h.V <= 0x14010000) {
            var stringPalette = X<NiStringPalette>.Ref(r);
            NodeName = Y.StringRef(r, stringPalette);
            PropertyType = Y.StringRef(r, stringPalette);
            ControllerType = Y.StringRef(r, stringPalette);
            ControllerID = Y.StringRef(r, stringPalette);
            InterpolatorID = Y.StringRef(r, stringPalette);
        }
        else if (h.V >= 0x14010001) {
            NodeName = Y.String(r);
            PropertyType = Y.String(r);
            ControllerType = Y.String(r);
            ControllerID = Y.String(r);
            InterpolatorID = Y.String(r);
        }
    }
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
public class Header { // X
    public string HeaderString;                         // 'NetImmerse File Format x.x.x.x' (versions <= 10.0.1.2) or 'Gamebryo File Format x.x.x.x' (versions >= 10.1.0.0), with x.x.x.x the version written out. Ends with a newline character (0x0A).
    public string[] Copyright;
    public uint V = 0x04000002;                         // The NIF version, in hexadecimal notation: 0x04000002, 0x0401000C, 0x04020002, 0x04020100, 0x04020200, 0x0A000100, 0x0A010000, 0x0A020000, 0x14000004, ...
    public EndianType EndianType = ENDIAN_LITTLE;       // Determines the endianness of the data in the file.
    public uint UV;                                     // An extra version number, for companies that decide to modify the file format.
    public uint NumBlocks;                              // Number of file objects.
    public uint UV2 = 0;
    public ExportInfo ExportInfo;
    public string MaxFilepath;
    public byte[] Metadata;
    public string[] BlockTypes;                         // List of all object types used in this NIF file.
    public uint[] BlockTypeHashes;                      // List of all object types used in this NIF file.
    public ushort[] BlockTypeIndex;                     // Maps file objects on their corresponding type: first file object is of type object_types[object_type_index[0]], the second of object_types[object_type_index[1]], etc.
    public uint[] BlockSize;                            // Array of block sizes?
    public uint NumStrings;                             // Number of strings.
    public uint MaxStringLength;                        // Maximum string length.
    public string[] Strings;                            // Strings.
    public uint[] Groups;

    public Header(BinaryReader r) {
        (HeaderString, V) = Y.ParseHeaderStr(r.ReadVAString(0x80, 0xA)); var h = this;
        if (h.V <= 0x03010000) Copyright = [r.ReadL8AString(), r.ReadL8AString(), r.ReadL8AString()];
        if (h.V >= 0x03010001) V = r.ReadUInt32();
        if (h.V >= 0x14000003) EndianType = (EndianType)r.ReadByte();
        if (h.V >= 0x0A000108) UV = r.ReadUInt32();
        if (h.V >= 0x03010001) NumBlocks = r.ReadUInt32();
        if (((h.V == 0x14020007) || (h.V == 0x14000005) || ((h.V >= 0x0A000102) && (h.V <= 0x14000004) && (h.UV <= 11))) && (h.UV >= 3)) {
            UV2 = r.ReadUInt32();
            ExportInfo = new ExportInfo(r);
        }
        if ((h.UV2 == 130)) MaxFilepath = r.ReadL8AString();
        if (h.V >= 0x1E000000) Metadata = r.ReadL8Bytes();
        if (h.V != 0x14030102 && h.V >= 0x05000001) BlockTypes = r.ReadL16FArray(r => r.ReadL16L32AString());
        if (h.V == 0x14030102) BlockTypeHashes = r.ReadL16PArray<uint>("I");
        if (h.V >= 0x05000001) BlockTypeIndex = r.ReadPArray<ushort>("H", NumBlocks);
        if (h.V >= 0x14020005) BlockSize = r.ReadPArray<uint>("I", NumBlocks);
        if (h.V >= 0x14010001) {
            NumStrings = r.ReadUInt32();
            MaxStringLength = r.ReadUInt32();
            Strings = r.ReadFArray(r => r.ReadL32AString(), NumStrings);
        }
        if (h.V >= 0x05000006) Groups = r.ReadL32PArray<uint>("I");
    }
}

/// <summary>
/// A list of \\0 terminated strings.
/// </summary>
public class StringPalette(BinaryReader r) {
    public string[] Palette = r.ReadL32AString().Split((char)0); // A bunch of 0x00 seperated strings.
    public uint Length = r.ReadUInt32();                // Length of the palette string is repeated here.
}

/// <summary>
/// Tension, bias, continuity.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TBC { // X
    public static (string, int) Struct = ("<3f", 12);
    public float t;                                     // Tension.
    public float b;                                     // Bias.
    public float c;                                     // Continuity.

    public TBC(BinaryReader r) {
        t = r.ReadSingle();
        b = r.ReadSingle();
        c = r.ReadSingle();
    }
}

/// <summary>
/// A generic key with support for interpolation. Type 1 is normal linear interpolation, type 2 has forward and backward tangents, and type 3 has tension, bias and continuity arguments. Note that color4 and byte always seem to be of type 1.
/// </summary>
public class Key<T> { // X
    public float Time;                                  // Time of the key.
    public T Value;                                     // The key value.
    public T Forward;                                   // Key forward tangent.
    public T Backward;                                  // The key backward tangent.
    public TBC TBC;                                     // The TBC of the key.

    public Key(BinaryReader r, KeyType keyType) {
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
public class KeyGroup<T> { // X
    public uint NumKeys;                                // Number of keys in the array.
    public KeyType Interpolation;                       // The key type.
    public Key<T>[] Keys;                               // The keys.

    public KeyGroup(BinaryReader r) {
        NumKeys = r.ReadUInt32();
        if (NumKeys != 0) Interpolation = (KeyType)r.ReadUInt32();
        Keys = r.ReadFArray(r => new Key<T>(r, Interpolation), NumKeys);
    }
}

/// <summary>
/// A special version of the key type used for quaternions.  Never has tangents.
/// </summary>
public class QuatKey<T> { // X
    public float Time;                                  // Time the key applies.
    public T Value;                                     // Value of the key.
    public TBC TBC;                                     // The TBC of the key.

    public QuatKey(BinaryReader r, Header h, KeyType keyType) {
        if (h.V <= 0x0A010000) Time = r.ReadSingle();
        if (keyType != KeyType.XYZ_ROTATION_KEY) {
            if (h.V >= 0x0A01006A) Time = r.ReadSingle();
            Value = X<T>.Read(r);
        }
        else if (keyType == KeyType.TBC_KEY) TBC = new TBC(r);
    }
}

/// <summary>
/// Texture coordinates (u,v). As in OpenGL; image origin is in the lower left corner.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TexCoord { // X
    public static (string, int) Struct = ("<2f", 8);
    public float u;                                     // First coordinate.
    public float v;                                     // Second coordinate.

    public TexCoord(BinaryReader r) {
        u = r.ReadSingle();
        v = r.ReadSingle();
    }
    public TexCoord(BinaryReader r, bool half) { u = r.ReadHalf(); v = r.ReadHalf(); }
}

// HalfTexCoord -> new TexCoord(r, true)
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
public class TexDesc { // X
    public int? Image;                                  // Link to the texture image.
    public int? Source;                                 // NiSourceTexture object index.
    public TexClampMode ClampMode = WRAP_S_WRAP_T;      // 0=clamp S clamp T, 1=clamp S wrap T, 2=wrap S clamp T, 3=wrap S wrap T
    public TexFilterMode FilterMode = FILTER_TRILERP;   // 0=nearest, 1=bilinear, 2=trilinear, 3=..., 4=..., 5=...
    public Flags Flags;                                 // Texture mode flags; clamp and filter mode stored in upper byte with 0xYZ00 = clamp mode Y, filter mode Z.
    public ushort MaxAnisotropy;
    public uint UVSet = 0;                              // The texture coordinate set in NiGeometryData that this texture slot will use.
    public short PS2L = 0;                              // L can range from 0 to 3 and are used to specify how fast a texture gets blurry.
    public short PS2K = -75;                            // K is used as an offset into the mipmap levels and can range from -2047 to 2047. Positive values push the mipmap towards being blurry and negative values make the mipmap sharper.
    public ushort Unknown1;                             // Unknown, 0 or 0x0101?
    // NiTextureTransform
    public TexCoord Translation;                        // The UV translation.
    public TexCoord Scale = new TexCord(1.0, 1.0);      // The UV scale.
    public float Rotation = 0.0f;                       // The W axis rotation in texture space.
    public TransformMethod TransformMethod = 0;         // Depending on the source, scaling can occur before or after rotation.
    public TexCoord Center;                             // The origin around which the texture rotates.

    public TexDesc(BinaryReader r, Header h) {
        if (h.V <= 0x03010000) Image = X<NiImage>.Ref(r);
        if (h.V >= 0x0303000D) Source = X<NiSourceTexture>.Ref(r);
        if (h.V <= 0x14000005) {
            ClampMode = (TexClampMode)r.ReadUInt32();
            FilterMode = (TexFilterMode)r.ReadUInt32();
        }
        if (h.V >= 0x14010003) Flags = (Flags)r.ReadUInt16();
        if (h.V >= 0x14050004) MaxAnisotropy = r.ReadUInt16();
        if (h.V <= 0x14000005) UVSet = r.ReadUInt32();
        if (h.V <= 0x0A040001) {
            PS2L = r.ReadInt16();
            PS2K = r.ReadInt16();
        }
        if (h.V <= 0x0401000C) Unknown1 = r.ReadUInt16();
        // NiTextureTransform
        if (r.ReadBool32() && h.V >= 0x0A010000) {
            Translation = new TexCoord(r);
            Scale = new TexCoord(r);
            Rotation = r.ReadSingle();
            TransformMethod = (TransformMethod)r.ReadUInt32();
            Center = new TexCoord(r);
        }
    }
}

/// <summary>
/// NiTexturingProperty::ShaderMap. Shader texture description.
/// </summary>
public class ShaderTexDesc {
    public TexDesc Map;
    public uint MapID;                                  // Unique identifier for the Gamebryo shader system.

    public ShaderTexDesc(BinaryReader r) {
        if (r.ReadBool32()) {
            Map = new TexDesc(r, h);
            MapID = r.ReadUInt32();
        }
    }
}

/// <summary>
/// List of three vertex indices.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Triangle { // X
    public static (string, int) Struct = ("<3H", 18);
    public ushort v1;                                   // First vertex index.
    public ushort v2;                                   // Second vertex index.
    public ushort v3;                                   // Third vertex index.

    public Triangle(BinaryReader r) {
        v1 = r.ReadUInt16();
        v2 = r.ReadUInt16();
        v3 = r.ReadUInt16();
    }
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

public class BSVertexData {
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

    public BSVertexData(BinaryReader r, VertexFlags arg, bool sse) {
        var full = sse || arg.HasFlag(VertexFlags.Full_Precision);
        var tangents = arg.HasFlag(VertexFlags.Tangents);
        if (arg.HasFlag(VertexFlags.Vertex)) {
            Vertex = full ? r.ReadVector3() : r.ReadHalfVector3();
            if (tangents) BitangentX = full ? r.ReadSingle() : r.ReadHalf();
            else UnknownInt = full ? r.ReadUInt32() : r.ReadUInt16();
        }
        if ((arg.HasFlag(VertexFlags.UVs))) UV = new TexCoord(r, true);
        if (arg.HasFlag(VertexFlags.Normals)) {
            Normal = new Vector3<byte>(r.ReadByte(), r.ReadByte(), r.ReadByte());
            BitangentY = r.ReadByte();
            if (true) {
                if (tangents) Tangent = new Vector3<byte>(r.ReadByte(), r.ReadByte(), r.ReadByte());
                if (tangents) BitangentZ = r.ReadByte();
            }
        }
        if (arg.HasFlag(VertexFlags.Vertex_Colors)) VertexColors = new Color4Byte(r);
        if (arg.HasFlag(VertexFlags.Skinned)) {
            BoneWeights = [r.ReadHalf(), r.ReadHalf(), r.ReadHalf(), r.ReadHalf()];
            BoneIndices = r.ReadBytes(4);
        }
        if (arg.HasFlag(VertexFlags.Eye_Data)) EyeData = r.ReadSingle();
    }
}

// BSVertexDataSSE -> new BSVertexData(r, true)

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BSVertexDesc {
    public static (string, int) Struct = ("<5bHb", 8);
    public byte VF1;
    public byte VF2;
    public byte VF3;
    public byte VF4;
    public byte VF5;
    public VertexFlags VertexAttributes;
    public byte VF8;

    public BSVertexDesc(BinaryReader r) {
        VF1 = r.ReadByte();
        VF2 = r.ReadByte();
        VF3 = r.ReadByte();
        VF4 = r.ReadByte();
        VF5 = r.ReadByte();
        VertexAttributes = (VertexFlags)r.ReadUInt16();
        VF8 = r.ReadByte();
    }
}

/// <summary>
/// Skinning data for a submesh, optimized for hardware skinning. Part of NiSkinPartition.
/// </summary>
public class SkinPartition {
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
        NumTriangles = (ushort)(NumVertices / 3); // calculated
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
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NiPlane {
    public static (string, int) Struct = ("<4f", 24);
    public Vector3 Normal;                              // The plane normal.
    public float Constant;                              // The plane constant.

    public NiPlane(BinaryReader r) {
        Normal = r.ReadVector3();
        Constant = r.ReadSingle();
    }
}

/// <summary>
/// A sphere.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NiBound {
    public static (string, int) Struct = ("<4f", 24);
    public Vector3 Center;                              // The sphere's center.
    public float Radius;                                // The sphere's radius.

    public NiBound(BinaryReader r) {
        Center = r.ReadVector3();
        Radius = r.ReadSingle();
    }
}

public struct NiQuatTransform {
    public Vector3 Translation;
    public Quaternion Rotation;
    public float Scale = 1.0f;
    public bool[] TRSValid;                             // Whether each transform component is valid.

    public NiQuatTransform(BinaryReader r, Header h) {
        Translation = r.ReadVector3();
        Rotation = r.ReadQuaternion();
        Scale = r.ReadSingle();
        if (h.V <= 0x0A01006D) TRSValid = [r.ReadBool32(), r.ReadBool32(), r.ReadBool32()];
    }
}

public struct NiTransform { // X
    public Matrix3x3 Rotation;                          // The rotation part of the transformation matrix.
    public Vector3 Translation;                         // The translation vector.
    public float Scale = 1.0f;                          // Scaling part (only uniform scaling is supported).

    public NiTransform(BinaryReader r) {
        Rotation = r.ReadMatrix3x3();
        Translation = r.ReadVector3();
        Scale = r.ReadSingle();
    }
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
public class FurniturePosition {
    public Vector3 Offset;                              // Offset of furniture marker.
    public ushort Orientation;                          // Furniture marker orientation.
    public byte PositionRef1;                           // Refers to a furnituremarkerxx.nif file. Always seems to be the same as Position Ref 2.
    public byte PositionRef2;                           // Refers to a furnituremarkerxx.nif file. Always seems to be the same as Position Ref 1.
    public float Heading;                               // Similar to Orientation, in float form.
    public AnimationType AnimationType;                 // Unknown
    public FurnitureEntryPoints EntryProperties;        // Unknown/unused in nif?

    public FurniturePosition(BinaryReader r, Header h) {
        Offset = r.ReadVector3();
        if (h.UV2 <= 34) {
            Orientation = r.ReadUInt16();
            PositionRef1 = r.ReadByte();
            PositionRef2 = r.ReadByte();
        }
        else if (h.UV2 > 34) {
            Heading = r.ReadSingle();
            AnimationType = (AnimationType)r.ReadUInt16();
            EntryProperties = (FurnitureEntryPoints)r.ReadUInt16();
        }
    }
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
public class Morph { // X
    public string FrameName;                            // Name of the frame.
    public uint NumKeys;                                // The number of morph keys that follow.
    public KeyType Interpolation;                       // Unlike most objects, the presense of this value is not conditional on there being keys.
    public Key<float>[] Keys;                           // The morph key frames.
    public float LegacyWeight;
    public Vector3[] Vectors;                           // Morph vectors.

    public Morph(BinaryReader r, Header h, uint numVertices) {
        if (h.V >= 0x0A01006A) FrameName = Y.String(r);
        if (h.V <= 0x0A010000) {
            NumKeys = r.ReadUInt32();
            Interpolation = (KeyType)r.ReadUInt32();
            Keys = r.ReadFArray(r => new Key<float>(r, Interpolation), NumKeys);
        }
        if (h.V >= 0x0A010068 && h.V <= 0x14010002 && h.UV2 < 10) LegacyWeight = r.ReadSingle();
        Vectors = r.ReadPArray<Vector3>("3f", numVertices);
    }
}

/// <summary>
/// particle array entry
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Particle { // X
    public static (string, int) Struct = ("<9f2H", 40);
    public Vector3 Velocity;                            // Particle velocity
    public Vector3 UnknownVector;                       // Unknown
    public float Lifetime;                              // The particle age.
    public float Lifespan;                              // Maximum age of the particle.
    public float Timestamp;                             // Timestamp of the last update.
    public ushort UnknownShort = 0;                     // Unknown short
    public ushort VertexID;                             // Particle/vertex index matches array index

    public Particle(BinaryReader r) {
        Velocity = r.ReadVector3();
        UnknownVector = r.ReadVector3();
        Lifetime = r.ReadSingle();
        Lifespan = r.ReadSingle();
        Timestamp = r.ReadSingle();
        UnknownShort = r.ReadUInt16();
        VertexID = r.ReadUInt16();
    }
}

/// <summary>
/// NiSkinData::BoneData. Skinning data component.
/// </summary>
public class BoneData { // X
    public NiTransform SkinTransform;                   // Offset of the skin from this bone in bind position.
    public Vector3 BoundingSphereOffset;                // Translation offset of a bounding sphere holding all vertices. (Note that its a Sphere Containing Axis Aligned Box not a minimum volume Sphere)
    public float BoundingSphereRadius;                  // Radius for bounding sphere holding all vertices.
    public short[] Unknown13Shorts;                     // Unknown, always 0?
    public BoneVertData[] VertexWeights;                // The vertex weights.

    public BoneData(BinaryReader r, Header h, int arg) {
        SkinTransform = new NiTransform(r);
        BoundingSphereOffset = r.ReadVector3();
        BoundingSphereRadius = r.ReadSingle();
        if (h.V == 0x14030009 && (h.UV == 0x20000 || h.UV == 0x30000)) Unknown13Shorts = r.ReadPArray<ushort>("H", 13);
        VertexWeights = h.V <= 0x04020100 ? r.ReadL16SArray<BoneVertData>()
            : h.V >= 0x04020200 && arg == 1 ? r.ReadL16SArray<BoneVertData>()
            : h.V >= 0x14030101 && arg == 15 ? r.ReadL16FArray(r => new BoneVertData(r, false))
            : default;
    }
}

/// <summary>
/// Bethesda Havok. Collision filter info representing Layer, Flags, Part Number, and Group all combined into one uint.
/// </summary>
public class HavokFilter(BinaryReader r, Header h) {
    public OblivionLayer Layer_OB = h.V <= 0x14000005 && (h.UV2 < 16) ? (OblivionLayer)r.ReadByte() : OL_STATIC; // The layer the collision belongs to.
    public Fallout3Layer Layer_FO = (h.V == 0x14020007) && (h.UV2 <= 34) ? (Fallout3Layer)r.ReadByte() : FOL_STATIC; // The layer the collision belongs to.
    public SkyrimLayer Layer_SK = (h.V == 0x14020007) && (h.UV2 > 34) ? (SkyrimLayer)r.ReadByte() : SKYL_STATIC; // The layer the collision belongs to.
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
    public OblivionHavokMaterial Material_OB = h.V <= 0x14000005 && (h.UV2 < 16) ? (OblivionHavokMaterial)r.ReadUInt32() : default; // The material of the shape.
    public Fallout3HavokMaterial Material_FO = (h.V == 0x14020007) && (h.UV2 <= 34) ? (Fallout3HavokMaterial)r.ReadUInt32() : default; // The material of the shape.
    public SkyrimHavokMaterial Material_SK = (h.V == 0x14020007) && (h.UV2 > 34) ? (SkyrimHavokMaterial)r.ReadUInt32() : default; // The material of the shape.
}

/// <summary>
/// Bethesda Havok. Havok Information for packed TriStrip shapes.
/// </summary>
public class OblivionSubShape(BinaryReader r, Header h) {
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

public class MotorDescriptor {
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
public class RagdollDescriptor {
    public Vector4 PivotA;                              // Point around which the object will rotate. Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    public Vector4 PlaneA;                              // Defines the orthogonal plane in which the body can move, the orthogonal directions in which the shape can be controlled (the direction orthogonal on this one and Twist A).
    public Vector4 TwistA;                              // Central directed axis of the cone in which the object can rotate. Orthogonal on Plane A.
    public Vector4 PivotB;                              // Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    public Vector4 PlaneB;                              // Defines the orthogonal plane in which the body can move, the orthogonal directions in which the shape can be controlled (the direction orthogonal on this one and Twist A).
    public Vector4 TwistB;                              // Central directed axis of the cone in which the object can rotate. Orthogonal on Plane B.
    public Vector4 MotorA;                              // Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    public Vector4 MotorB;                              // Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    public float ConeMaxAngle;                          // Maximum angle the object can rotate around the vector orthogonal on Plane A and Twist A relative to the Twist A vector. Note that Cone Min Angle is not stored, but is simply minus this angle.
    public float PlaneMinAngle;                         // Minimum angle the object can rotate around Plane A, relative to Twist A.
    public float PlaneMaxAngle;                         // Maximum angle the object can rotate around Plane A, relative to Twist A.
    public float TwistMinAngle;                         // Minimum angle the object can rotate around Twist A, relative to Plane A.
    public float TwistMaxAngle;                         // Maximum angle the object can rotate around Twist A, relative to Plane A.
    public float MaxFriction;                           // Maximum friction, typically 0 or 10. In Fallout 3, typically 100.
    public MotorDescriptor Motor;

    public RagdollDescriptor(BinaryReader r, Header h) {
        // Oblivion and Fallout 3, Havok 550
        if (h.UV2 <= 16) {
            PivotA = r.ReadVector4();
            PlaneA = r.ReadVector4();
            TwistA = r.ReadVector4();
            PivotB = r.ReadVector4();
            PlaneB = r.ReadVector4();
            TwistB = r.ReadVector4();
        }
        // Fallout 3 and later, Havok 660 and 2010
        else if (h.UV2 > 16){
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
        if (h.V >= 0x14020007 && h.UV2 > 16) Motor = new MotorDescriptor(r);
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
        if (h.UV2 <= 16) {
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
        if (h.V >= 0x14020007 && h.UV2 > 16) Motor = new MotorDescriptor(r);
    }
}

/// <summary>
/// This constraint allows rotation about a specified axis.
/// </summary>
public class HingeDescriptor {
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

public class BallAndSocketDescriptor(BinaryReader r) {
    public Vector4 PivotA = r.ReadVector4();            // Pivot point in the local space of entity A.
    public Vector4 PivotB = r.ReadVector4();            // Pivot point in the local space of entity B.
}

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
        if (h.V >= 0x14020007 && h.UV2 > 16) Motor = new MotorDescriptor(r);
    }
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
    public Vector3 Center = r.ReadVector3();        //was:Translation
    public Matrix4x4 Axis = r.ReadMatrix3x3As4x4(); //was:Rotation
    public Vector3 Extent = r.ReadVector3();        //was:Radius
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

public class BoundingVolume {
    public BoundVolumeType CollisionType;               // Type of collision data.
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

public class UnionBV(BinaryReader r) {
    public BoundingVolume[] BoundingVolumes = r.ReadL32FArray(r => new BoundingVolume(r));
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
    public ushort[] Vertices = r.ReadL32PArray<ushort>("H");
    public ushort[] Indices = r.ReadL32PArray<ushort>("H");
    public ushort[] Strips = r.ReadL32PArray<ushort>("H");
    public ushort[] WeldingInfo = r.ReadL32PArray<ushort>("H");
}

public class MalleableDescriptor {
    public hkConstraintType Type;                       // Type of constraint.
    public uint NumEntities = 2;                        // Always 2 (Hardcoded). Number of bodies affected by this constraint.
    public int? EntityA;                                // Usually NONE. The entity affected by this constraint.
    public int? EntityB;                                // Usually NONE. The entity affected by this constraint.
    public uint Priority = 1;                           // Usually 1. Higher values indicate higher priority of this constraint?
    public BallAndSocketDescriptor BallandSocket;
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
        EntityA = X<bhkEntity>.Ptr(r);
        EntityB = X<bhkEntity>.Ptr(r);
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
        else if (h.V >= 0x14020007) Strength = r.ReadSingle();
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

    public Ni3dsAlphaAnimator(BinaryReader r, Header h) : base(r, h) {
        Unknown1 = r.ReadBytes(40);
        Parent = X<NiObject>.Ref(r);
        Num1 = r.ReadUInt32();
        Num2 = r.ReadUInt32();
        Unknown2 = r.ReadFArray(k => r.ReadPArray<uint>("I", Num1), Num2);
    }
}

/// <summary>
/// Unknown. Only found in 2.3 nifs.
/// </summary>
public class Ni3dsAnimationNode : NiObject {
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
public class Ni3dsColorAnimator : NiObject {
    public byte[] Unknown1;                             // Unknown.

    public Ni3dsColorAnimator(BinaryReader r, Header h) : base(r, h) {
        Unknown1 = r.ReadBytes(184);
    }
}

/// <summary>
/// Unknown!
/// </summary>
public class Ni3dsMorphShape : NiObject {
    public byte[] Unknown1;                             // Unknown.

    public Ni3dsMorphShape(BinaryReader r, Header h) : base(r, h) {
        Unknown1 = r.ReadBytes(14);
    }
}

/// <summary>
/// Unknown!
/// </summary>
public class Ni3dsParticleSystem : NiObject {
    public byte[] Unknown1;                             // Unknown.

    public Ni3dsParticleSystem(BinaryReader r, Header h) : base(r, h) {
        Unknown1 = r.ReadBytes(14);
    }
}

/// <summary>
/// Unknown!
/// </summary>
public class Ni3dsPathController : NiObject {
    public byte[] Unknown1;                             // Unknown.

    public Ni3dsPathController(BinaryReader r, Header h) : base(r, h) {
        Unknown1 = r.ReadBytes(20);
    }
}

/// <summary>
/// LEGACY (pre-10.1). Abstract base class for particle system modifiers.
/// </summary>
public abstract class NiParticleModifier : NiObject { // X
    public int? NextModifier;                           // Next particle modifier.
    public int? Controller;                             // Points to the particle system controller parent.

    public NiParticleModifier(BinaryReader r, Header h) : base(r, h) {
        NextModifier = X<NiParticleModifier>.Ref(r);
        if (h.V >= 0x04000002) Controller = X<NiParticleSystemController>.Ptr(r);
    }
}

/// <summary>
/// Particle system collider.
/// </summary>
public abstract class NiPSysCollider : NiObject {
    public float Bounce = 1.0f;                         // Amount of bounce for the collider.
    public bool SpawnonCollide;                         // Spawn particles on impact?
    public bool DieonCollide;                           // Kill particles on impact?
    public int? SpawnModifier;                          // Spawner to use for the collider.
    public int? Parent;                                 // Link to parent.
    public int? NextCollider;                           // The next collider.
    public int? ColliderObject;                         // The object whose position and orientation are the basis of the collider.

    public NiPSysCollider(BinaryReader r, Header h) : base(r, h) {
        Bounce = r.ReadSingle();
        SpawnonCollide = r.ReadBool32();
        DieonCollide = r.ReadBool32();
        SpawnModifier = X<NiPSysSpawnModifier>.Ref(r);
        Parent = X<NiPSysColliderManager>.Ptr(r);
        NextCollider = X<NiPSysCollider>.Ref(r);
        ColliderObject = X<NiAVObject>.Ptr(r);
    }
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
public abstract class bhkRefObject(BinaryReader r, Header h) : NiObject(r, h) {
}

/// <summary>
/// Havok objects that can be saved and loaded from disk?
/// </summary>
public abstract class bhkSerializable(BinaryReader r, Header h) : bhkRefObject(r, h) {
}

/// <summary>
/// Havok objects that have a position in the world?
/// </summary>
public abstract class bhkWorldObject : bhkSerializable {
    public int? Shape;                                  // Link to the body for this collision object.
    public uint UnknownInt;
    public HavokFilter HavokFilter;
    public byte[] Unused;                               // Garbage data from memory.
    public BroadPhaseType BroadPhaseType = 1;
    public byte[] UnusedBytes;
    public hkWorldObjCinfoProperty CinfoProperty;

    public bhkWorldObject(BinaryReader r, Header h) : base(r, h) {
        Shape = X<bhkShape>.Ref(r);
        if (h.V <= 0x0A000100) UnknownInt = r.ReadUInt32();
        HavokFilter = new HavokFilter(r, h);
        Unused = r.ReadBytes(4);
        BroadPhaseType = (BroadPhaseType)r.ReadByte();
        UnusedBytes = r.ReadBytes(3);
        CinfoProperty = new hkWorldObjCinfoProperty(r);
    }
}

/// <summary>
/// Havok object that do not react with other objects when they collide (causing deflection, etc.) but still trigger collision notifications to the game.  Possible uses are traps, portals, AI fields, etc.
/// </summary>
public abstract class bhkPhantom(BinaryReader r, Header h) : bhkWorldObject(r, h) {
}

/// <summary>
/// A Havok phantom that uses a Havok shape object for its collision volume instead of just a bounding box.
/// </summary>
public abstract class bhkShapePhantom(BinaryReader r, Header h) : bhkPhantom(r, h) {
}

/// <summary>
/// Unknown shape.
/// </summary>
public class bhkSimpleShapePhantom : bhkShapePhantom {
    public byte[] Unused2;                              // Garbage data from memory.
    public Matrix4x4 Transform;

    public bhkSimpleShapePhantom(BinaryReader r, Header h) : base(r, h) {
        Unused2 = r.ReadBytes(8);
        Transform = r.ReadMatrix4x4();
    }
}

/// <summary>
/// A havok node, describes physical properties.
/// </summary>
public abstract class bhkEntity(BinaryReader r, Header h) : bhkWorldObject(r, h) {
}

/// <summary>
/// This is the default body type for all "normal" usable and static world objects. The "T" suffix
/// marks this body as active for translation and rotation, a normal bhkRigidBody ignores those
/// properties. Because the properties are equal, a bhkRigidBody may be renamed into a bhkRigidBodyT and vice-versa.
/// </summary>
public class bhkRigidBody : bhkEntity {
    public hkResponseType CollisionResponse = RESPONSE_SIMPLE_CONTACT; // How the body reacts to collisions. See hkResponseType for hkpWorld default implementations.
    public byte UnusedByte1;                            // Skipped over when writing Collision Response and Callback Delay.
    public ushort ProcessContactCallbackDelay = 0xffff; // Lowers the frequency for processContactCallbacks. A value of 5 means that a callback is raised every 5th frame. The default is once every 65535 frames.
    public uint UnknownInt1;                            // Unknown.
    public HavokFilter HavokFilterCopy;                 // Copy of Havok Filter
    public byte[] Unused2;                              // Garbage data from memory. Matches previous Unused value.
    public uint UnknownInt2;
    public hkResponseType CollisionResponse2 = RESPONSE_SIMPLE_CONTACT;
    public byte UnusedByte2;                            // Skipped over when writing Collision Response and Callback Delay.
    public ushort ProcessContactCallbackDelay2 = 0xffff;
    public Vector4 Translation;                         // A vector that moves the body by the specified amount. Only enabled in bhkRigidBodyT objects.
    public Quaternion Rotation;                         // The rotation Yaw/Pitch/Roll to apply to the body. Only enabled in bhkRigidBodyT objects.
    public Vector4 LinearVelocity;                      // Linear velocity.
    public Vector4 AngularVelocity;                     // Angular velocity.
    public Matrix3x4 InertiaTensor;                     // Defines how the mass is distributed among the body, i.e. how difficult it is to rotate around any given axis.
    public Vector4 Center;                              // The body's center of mass.
    public float Mass = 1.0f;                           // The body's mass in kg. A mass of zero represents an immovable object.
    public float LinearDamping = 0.1f;                  // Reduces the movement of the body over time. A value of 0.1 will remove 10% of the linear velocity every second.
    public float AngularDamping = 0.05f;                // Reduces the movement of the body over time. A value of 0.05 will remove 5% of the angular velocity every second.
    public float TimeFactor = 1.0f;
    public float GravityFactor = 1.0f;
    public float Friction = 0.5f;                       // How smooth its surfaces is and how easily it will slide along other bodies.
    public float RollingFrictionMultiplier;
    public float Restitution = 0.4f;                    // How "bouncy" the body is, i.e. how much energy it has after colliding. Less than 1.0 loses energy, greater than 1.0 gains energy.
                                                        //     If the restitution is not 0.0 the object will need extra CPU for all new collisions.
    public float MaxLinearVelocity = 104.4f;            // Maximal linear velocity.
    public float MaxAngularVelocity = 31.57f;           // Maximal angular velocity.
    public float PenetrationDepth = 0.15f;              // The maximum allowed penetration for this object.
                                                        //     This is a hint to the engine to see how much CPU the engine should invest to keep this object from penetrating.
                                                        //     A good choice is 5% - 20% of the smallest diameter of the object.
    public hkMotionType MotionSystem = MO_SYS_DYNAMIC;  // Motion system? Overrides Quality when on Keyframed?
    public hkDeactivatorType DeactivatorType = DEACTIVATOR_NEVER; // The initial deactivator type of the body.
    public bool EnableDeactivation = 1;
    public hkSolverDeactivation SolverDeactivation = SOLVER_DEACTIVATION_OFF; // How aggressively the engine will try to zero the velocity for slow objects. This does not save CPU.
    public hkQualityType QualityType = MO_QUAL_FIXED;   // The type of interaction with other objects.
    public float UnknownFloat1;
    public byte[] UnknownBytes1;                        // Unknown.
    public byte[] UnknownBytes2;                        // Unknown. Skyrim only.
    public int?[] Constraints;
    public ushort BodyFlags;                            // 1 = respond to wind

    public bhkRigidBody(BinaryReader r, Header h) : base(r, h) {
        CollisionResponse = (hkResponseType)r.ReadByte();
        UnusedByte1 = r.ReadByte();
        ProcessContactCallbackDelay = r.ReadUInt16();
        if (h.V >= 0x0A010000) {
            UnknownInt1 = r.ReadUInt32();
            HavokFilterCopy = new HavokFilter(r, h);
            Unused2 = r.ReadBytes(4);
            if (h.UV2 > 34) UnknownInt2 = r.ReadUInt32();
            CollisionResponse2 = (hkResponseType)r.ReadByte();
            UnusedByte2 = r.ReadByte();
            ProcessContactCallbackDelay2 = r.ReadUInt16();
            if (h.UV2 <= 34) UnknownInt2 = r.ReadUInt32();
        }
        else {
            CollisionResponse2 = hkResponseType.RESPONSE_SIMPLE_CONTACT;
            ProcessContactCallbackDelay2 = 0xffff;
        }
        Translation = r.ReadVector4();
        Rotation = r.ReadQuaternionWFirst();
        LinearVelocity = r.ReadVector4();
        AngularVelocity = r.ReadVector4();
        InertiaTensor = r.ReadMatrix3x4();
        Center = r.ReadVector4();
        Mass = r.ReadSingle();
        LinearDamping = r.ReadSingle();
        AngularDamping = r.ReadSingle();
        if (h.UV2 > 34) {
            TimeFactor = r.ReadSingle();
            if (h.UV2 != 130) GravityFactor = r.ReadSingle();
        }
        Friction = r.ReadSingle();
        if (h.UV2 > 34) RollingFrictionMultiplier = r.ReadSingle();
        Restitution = r.ReadSingle();
        if (h.V >= 0x0A010000) {
            MaxLinearVelocity = r.ReadSingle();
            MaxAngularVelocity = r.ReadSingle();
            if (h.UV2 != 130) PenetrationDepth = r.ReadSingle();
        }
        MotionSystem = (hkMotionType)r.ReadByte();
        if ((h.UV2 <= 34)) DeactivatorType = (hkDeactivatorType)r.ReadByte();
        if ((h.UV2 > 34)) EnableDeactivation = r.ReadBool32();
        SolverDeactivation = (hkSolverDeactivation)r.ReadByte();
        QualityType = (hkQualityType)r.ReadByte();
        if ((h.UV2 == 130)) {
            PenetrationDepth = r.ReadSingle();
            UnknownFloat1 = r.ReadSingle();
        }
        UnknownBytes1 = r.ReadBytes(12);
        if ((h.UV2 > 34)) UnknownBytes2 = r.ReadBytes(4);
        Constraints = r.ReadL32FArray(X<bhkSerializable>.Ref);
        BodyFlags = h.UserVersion2 < 76 ? r.ReadUInt32() : r.ReadUInt16();
    }
}

/// <summary>
/// The "T" suffix marks this body as active for translation and rotation.
/// </summary>
public class bhkRigidBodyT(BinaryReader r, Header h) : bhkRigidBody(r, h) {
}

/// <summary>
/// Describes a physical constraint.
/// </summary>
public abstract class bhkConstraint : bhkSerializable {
    public int?[] Entities;                             // The entities affected by this constraint.
    public uint Priority = 1;                           // Usually 1. Higher values indicate higher priority of this constraint?

    public bhkConstraint(BinaryReader r, Header h) : base(r, h) {
        Entities = r.ReadL32FArray(X<bhkEntity>.Ptr);
        Priority = r.ReadUInt32();
    }
}

/// <summary>
/// Hinge constraint.
/// </summary>
public class bhkLimitedHingeConstraint : bhkConstraint {
    public LimitedHingeDescriptor LimitedHinge;         // Describes a limited hinge constraint

    public bhkLimitedHingeConstraint(BinaryReader r, Header h) : base(r, h) {
        LimitedHinge = new LimitedHingeDescriptor(r, h);
    }
}

/// <summary>
/// A malleable constraint.
/// </summary>
public class bhkMalleableConstraint : bhkConstraint {
    public MalleableDescriptor Malleable;               // Constraint within constraint.

    public bhkMalleableConstraint(BinaryReader r, Header h) : base(r, h) {
        Malleable = new MalleableDescriptor(r, h);
    }
}

/// <summary>
/// A spring constraint.
/// </summary>
public class bhkStiffSpringConstraint : bhkConstraint {
    public StiffSpringDescriptor StiffSpring;           // Stiff Spring constraint.

    public bhkStiffSpringConstraint(BinaryReader r, Header h) : base(r, h) {
        StiffSpring = new StiffSpringDescriptor(r);
    }
}

/// <summary>
/// Ragdoll constraint.
/// </summary>
public class bhkRagdollConstraint : bhkConstraint {
    public RagdollDescriptor Ragdoll;                   // Ragdoll constraint.

    public bhkRagdollConstraint(BinaryReader r, Header h) : base(r, h) {
        Ragdoll = new RagdollDescriptor(r, h);
    }
}

/// <summary>
/// A prismatic constraint.
/// </summary>
public class bhkPrismaticConstraint : bhkConstraint {
    public PrismaticDescriptor Prismatic;               // Describes a prismatic constraint

    public bhkPrismaticConstraint(BinaryReader r, Header h) : base(r, h) {
        Prismatic = new PrismaticDescriptor(r, h);
    }
}

/// <summary>
/// A hinge constraint.
/// </summary>
public class bhkHingeConstraint : bhkConstraint {
    public HingeDescriptor Hinge;                       // Hinge constraing.

    public bhkHingeConstraint(BinaryReader r, Header h) : base(r, h) {
        Hinge = new HingeDescriptor(r, h);
    }
}

/// <summary>
/// A Ball and Socket Constraint.
/// </summary>
public class bhkBallAndSocketConstraint : bhkConstraint {
    public BallAndSocketDescriptor BallandSocket;       // Describes a ball and socket constraint

    public bhkBallAndSocketConstraint(BinaryReader r, Header h) : base(r, h) {
        BallandSocket = new BallAndSocketDescriptor(r);
    }
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
    public float Tau = 1.0f;                            // High values are harder and more reactive, lower values are smoother.
    public float Damping = 0.6f;                        // Defines damping strength for the current velocity.
    public float ConstraintForceMixing = 1.1920929e-08f;// Restitution (amount of elasticity) of constraints. Added to the diagonal of the constraint matrix. A value of 0.0 can result in a division by zero with some chain configurations.
    public float MaxErrorDistance = 0.1f;               // Maximum distance error in constraints allowed before stabilization algorithm kicks in. A smaller distance causes more resistance.
    public int?[] EntitiesA;
    public uint NumEntities = 2;                        // Hardcoded to 2. Don't change.
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
public abstract class bhkShape(BinaryReader r, Header h) : bhkSerializable(r, h) {
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
    public HavokMaterial Material;                      // The material of the shape.
    public float Radius;                                // The radius of the sphere that encloses the shape.

    public bhkSphereRepShape(BinaryReader r, Header h) : base(r, h) {
        Material = new HavokMaterial(r, h);
        Radius = r.ReadSingle();
    }
}

/// <summary>
/// A havok shape.
/// </summary>
public abstract class bhkConvexShape(BinaryReader r, Header h) : bhkSphereRepShape(r, h) {
}

/// <summary>
/// A sphere.
/// </summary>
public class bhkSphereShape(BinaryReader r, Header h) : bhkConvexShape(r, h) {
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
    public byte[] Unused;                               // Not used. The following wants to be aligned at 16 bytes.
    public Vector3 Dimensions;                          // A cube stored in Half Extents. A unit cube (1.0, 1.0, 1.0) would be stored as 0.5, 0.5, 0.5.
    public float UnusedFloat;                           // Unused as Havok stores the Half Extents as hkVector4 with the W component unused.

    public bhkBoxShape(BinaryReader r, Header h) : base(r, h) {
        Unused = r.ReadBytes(8);
        Dimensions = r.ReadVector3();
        UnusedFloat = r.ReadSingle();
    }
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

    public bhkConvexVerticesShape(BinaryReader r, Header h) : base(r, h) {
        VerticesProperty = new hkWorldObjCinfoProperty(r);
        NormalsProperty = new hkWorldObjCinfoProperty(r);
        Vertices = r.ReadL32PArray<Vector4>("4f");
        Normals = r.ReadL32PArray<Vector4>("4f");
    }
}

/// <summary>
/// A convex transformed shape?
/// </summary>
public class bhkConvexTransformShape(BinaryReader r, Header h) : bhkTransformShape(r, h) {
}

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
    public float UnknownFloat1;                         // Unknown.
    public float UnknownFloat2;                         // Unknown.
    public NiBound[] Spheres;                           // This array holds the spheres which make up the multi sphere shape.

    public bhkMultiSphereShape(BinaryReader r, Header h) : base(r, h) {
        UnknownFloat1 = r.ReadSingle();
        UnknownFloat2 = r.ReadSingle();
        Spheres = r.ReadL32FArray(r => new NiBound(r));
    }
}

/// <summary>
/// A tree-like Havok data structure stored in an assembly-like binary code?
/// </summary>
public abstract class bhkBvTreeShape(BinaryReader r, Header h) : bhkShape(r, h) {
}

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
        if (h.UV2 > 34) BuildType = (MoppDataBuildType)r.ReadByte();
        MOPPData = r.ReadBytes(MOPPDataSize);
    }
}

/// <summary>
/// Havok collision object that uses multiple shapes?
/// </summary>
public abstract class bhkShapeCollection(BinaryReader r, Header h) : bhkShape(r, h) {
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

    public bhkListShape(BinaryReader r, Header h) : base(r, h) {
        SubShapes = r.ReadL32FArray(X<bhkShape>.Ref);
        Material = new HavokMaterial(r, h);
        ChildShapeProperty = new hkWorldObjCinfoProperty(r);
        ChildFilterProperty = new hkWorldObjCinfoProperty(r);
        UnknownInts = r.ReadL32PArray<uint>("I");
    }
}

public class bhkMeshShape : bhkShape {
    public uint[] Unknowns;
    public float Radius;
    public byte[] Unused2;
    public Vector4 Scale;
    public hkWorldObjCinfoProperty[] ShapeProperties;
    public int[] Unknown2;
    public int?[] StripsData;                           // Refers to a bunch of NiTriStripsData objects that make up this shape.

    public bhkMeshShape(BinaryReader r, Header h) : base(r, h) {
        Unknowns = r.ReadPArray<uint>("I", 2);
        Radius = r.ReadSingle();
        Unused2 = r.ReadBytes(8);
        Scale = r.ReadVector4();
        ShapeProperties = r.ReadL32FArray(r => new hkWorldObjCinfoProperty(r));
        Unknown2 = r.ReadPArray<uint>("i", 3);
        if (h.V <= 0x0A000100) StripsData = r.ReadL32FArray(X<NiTriStripsData>.Ref);
    }
}

/// <summary>
/// A shape constructed from strips data.
/// </summary>
public class bhkPackedNiTriStripsShape : bhkShapeCollection {
    public OblivionSubShape[] SubShapes;
    public uint UserData = 0;
    public uint Unused1;                                // Looks like a memory pointer and may be garbage.
    public float Radius = 0.1;
    public uint Unused2;                                // Looks like a memory pointer and may be garbage.
    public Vector4 Scale = 1.0, 1.0, 1.0, 0.0;
    public float RadiusCopy = 0.1;                      // Same as radius
    public Vector4 ScaleCopy = 1.0, 1.0, 1.0, 0.0;      // Same as scale.
    public int? Data;

    public bhkPackedNiTriStripsShape(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x14000005) SubShapes = r.ReadL16FArray(r => new OblivionSubShape(r, h));
        UserData = r.ReadUInt32();
        Unused1 = r.ReadUInt32();
        Radius = r.ReadSingle();
        Unused2 = r.ReadUInt32();
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
    public HavokMaterial Material;                      // The material of the shape.
    public float Radius = 0.1;
    public uint[] Unused;                               // Garbage data from memory though the last 3 are referred to as maxSize, size, and eSize.
    public uint GrowBy = 1;
    public Vector4 Scale = 1.0, 1.0, 1.0, 0.0;          // Scale. Usually (1.0, 1.0, 1.0, 0.0).
    public int?[] StripsData;                           // Refers to a bunch of NiTriStripsData objects that make up this shape.
    public HavokFilter[] DataLayers;                    // Havok Layers for each strip data.

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
public class NiExtraData : NiObject { // X
    public string Name;                                 // Name of this object.
    public int? NextExtraData;                          // Block number of the next extra data object.

    public NiExtraData(BinaryReader r, Header h) : base(r, h) {
        if (!BSExtraData && h.V >= 0x0A000100) Name = Y.String(r);
        if (h.V <= 0x04020200) NextExtraData = X<NiExtraData>.Ref(r);
    }
}

/// <summary>
/// Abstract base class for all interpolators of bool, float, NiQuaternion, NiPoint3, NiColorA, and NiQuatTransform data.
/// </summary>
public abstract class NiInterpolator(BinaryReader r, Header h) : NiObject(r, h) {
}

/// <summary>
/// Abstract base class for interpolators that use NiAnimationKeys (Key, KeyGrp) for interpolation.
/// </summary>
public abstract class NiKeyBasedInterpolator(BinaryReader r, Header h) : NiInterpolator(r, h) {
}

/// <summary>
/// Uses NiFloatKeys to animate a float value over time.
/// </summary>
public class NiFloatInterpolator : NiKeyBasedInterpolator {
    public float Value = -3.402823466e+38;              // Pose value if lacking NiFloatData.
    public int? Data;

    public NiFloatInterpolator(BinaryReader r, Header h) : base(r, h) {
        Value = r.ReadSingle();
        Data = X<NiFloatData>.Ref(r);
    }
}

/// <summary>
/// An interpolator for transform keyframes.
/// </summary>
public class NiTransformInterpolator : NiKeyBasedInterpolator {
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
public class NiPoint3Interpolator : NiKeyBasedInterpolator {
    public Vector3 Value = -3.402823466e+38, -3.402823466e+38, -3.402823466e+38; // Pose value if lacking NiPosData.
    public int? Data;

    public NiPoint3Interpolator(BinaryReader r, Header h) : base(r, h) {
        Value = r.ReadVector3();
        Data = X<NiPosData>.Ref(r);
    }
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
    public PathFlags Flags = 3;
    public int BankDir = 1;                             // -1 = Negative, 1 = Positive
    public float MaxBankAngle;                          // Max angle in radians.
    public float Smoothing;
    public short FollowAxis;                            // 0, 1, or 2 representing X, Y, or Z.
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
public class NiBoolInterpolator : NiKeyBasedInterpolator {
    public bool Value = 2;                              // Pose value if lacking NiBoolData.
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
public class NiBoolTimelineInterpolator(BinaryReader r, Header h) : NiBoolInterpolator(r, h) {
}

public enum InterpBlendFlags : byte {
    MANAGER_CONTROLLED = 1          // MANAGER_CONTROLLED
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
public abstract class NiBSplineInterpolator : NiInterpolator {
    public float StartTime = 3.402823466e+38;           // Animation start time.
    public float StopTime = -3.402823466e+38;           // Animation stop time.
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
        if (BSLightingShaderProperty && h.UV2 >= 83) SkyrimShaderType = (BSLightingShaderPropertyShaderType)r.ReadUInt32();
        Name = Y.String(r);
        if (h.V <= 0x02030000) {
            if (r.ReadBool32()) OldExtra = (Y.String(r), r.ReadUInt32(), Y.String(r));
            r.Skip(1); // Unknown Byte, Always 0.
        }
        if (h.V >= 0x03000000 && h.V <= 0x04020200) ExtraData = X<NiExtraData>.Ref(r);
        if (h.V >= 0x0A000100) ExtraDataList = r.ReadL32FArray(X<NiExtraData>.Ref);
        if (h.V >= 0x03000000) Controller = X<NiTimeController>.Ref(r);
    }
}

/// <summary>
/// This is the most common collision object found in NIF files. It acts as a real object that
/// is visible and possibly (if the body allows for it) interactive. The node itself
/// is simple, it only has three properties.
/// For this type of collision object, bhkRigidBody or bhkRigidBodyT is generally used.
/// </summary>
public class NiCollisionObject : NiObject {
    public int? Target;                                 // Index of the AV object referring to this collision object.

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
    public bhkCOFlags Flags = 1;                        // Set to 1 for most objects, and to 41 for animated objects (ANIM_STATIC). Bits: 0=Active 2=Notify 3=Set Local 6=Reset.
    public int? Body;

    public bhkNiCollisionObject(BinaryReader r, Header h) : base(r, h) {
        Flags = (bhkCOFlags)r.ReadUInt16();
        Body = X<bhkWorldObject>.Ref(r);
    }
}

/// <summary>
/// Havok related collision object?
/// </summary>
public class bhkCollisionObject(BinaryReader r, Header h) : bhkNiCollisionObject(r, h) {
}

/// <summary>
/// Unknown.
/// </summary>
public class bhkBlendCollisionObject : bhkCollisionObject {
    public float HeirGain;
    public float VelGain;
    public float UnkFloat1;
    public float UnkFloat2;

    public bhkBlendCollisionObject(BinaryReader r, Header h) : base(r, h) {
        HeirGain = r.ReadSingle();
        VelGain = r.ReadSingle();
        if (h.UV2 < 9) {
            UnkFloat1 = r.ReadSingle();
            UnkFloat2 = r.ReadSingle();
        }
    }
}

/// <summary>
/// Unknown.
/// </summary>
public class bhkPCollisionObject(BinaryReader r, Header h) : bhkNiCollisionObject(r, h) {
}

/// <summary>
/// Unknown.
/// </summary>
public class bhkSPCollisionObject(BinaryReader r, Header h) : bhkPCollisionObject(r, h) {
}

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
    public float Dimmer = 1.0;                          // Scales the overall brightness of all light components.
    public Color3 AmbientColor = 0.0, 0.0, 0.0;
    public Color3 DiffuseColor = 0.0, 0.0, 0.0;
    public Color3 SpecularColor = 0.0, 0.0, 0.0;

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
public abstract class NiProperty(BinaryReader r, Header h) : NiObjectNET(r, h) { // X
}

/// <summary>
/// Unknown
/// </summary>
public class NiTransparentProperty : NiProperty {
    public byte[] Unknown;                              // Unknown.

    public NiTransparentProperty(BinaryReader r, Header h) : base(r, h) {
        Unknown = r.ReadBytes(6);
    }
}

/// <summary>
/// Abstract base class for all particle system modifiers.
/// </summary>
public abstract class NiPSysModifier : NiObject {
    public string Name;                                 // Used to locate the modifier.
    public uint Order;                                  // Modifier ID in the particle modifier chain (always a multiple of 1000)?
    public int? Target;                                 // NiParticleSystem parent of this modifier.
    public bool Active = 1;                             // Whether or not the modifier is active.

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
    public float Speed;                                 // Speed / Inertia of particle movement.
    public float SpeedVariation;                        // Adds an amount of randomness to Speed.
    public float Declination;                           // Declination / First axis.
    public float DeclinationVariation;                  // Declination randomness / First axis.
    public float PlanarAngle;                           // Planar Angle / Second axis.
    public float PlanarAngleVariation;                  // Planar Angle randomness / Second axis .
    public Color4 InitialColor;                         // Defines color of a birthed particle.
    public float InitialRadius = 1.0;                   // Size of a birthed particle.
    public float RadiusVariation;                       // Particle Radius randomness.
    public float LifeSpan;                              // Duration until a particle dies.
    public float LifeSpanVariation;                     // Adds randomness to Life Span.

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
    public int? EmitterObject;                          // Node parent of this modifier?

    public NiPSysVolumeEmitter(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010000) EmitterObject = X<NiNode>.Ptr(r);
    }
}

/// <summary>
/// Abstract base class that provides the base timing and update functionality for all the Gamebryo animation controllers.
/// </summary>
public abstract class NiTimeController : NiObject { // X
    public int? NextController;                         // Index of the next controller.
    public Flags Flags;                                 // Controller flags.
                                                        //     Bit 0 : Anim type, 0=APP_TIME 1=APP_INIT
                                                        //     Bit 1-2 : Cycle type, 00=Loop 01=Reverse 10=Clamp
                                                        //     Bit 3 : Active
                                                        //     Bit 4 : Play backwards
                                                        //     Bit 5 : Is manager controlled
                                                        //     Bit 6 : Always seems to be set in Skyrim and Fallout NIFs, unknown function
    public float Frequency = 1.0;                       // Frequency (is usually 1.0).
    public float Phase;                                 // Phase (usually 0.0).
    public float StartTime = 3.402823466e+38;           // Controller start time.
    public float StopTime = -3.402823466e+38;           // Controller stop time.
    public int? Target;                                 // Controller target (object index of the first controllable ancestor of this object).
    public uint UnknownInteger;                         // Unknown integer.

    public NiTimeController(BinaryReader r, Header h) : base(r, h) {
        NextController = X<NiTimeController>.Ref(r);
        Flags = (Flags)r.ReadUInt16();
        Frequency = r.ReadSingle();
        Phase = r.ReadSingle();
        StartTime = r.ReadSingle();
        StopTime = r.ReadSingle();
        if (h.V >= 0x0303000D) Target = X<NiObjectNET>.Ptr(r);
        else if (h.V <= 0x03010000) UnknownInteger = r.ReadUInt32();
    }
}

/// <summary>
/// Abstract base class for all NiTimeController objects using NiInterpolator objects to animate their target objects.
/// </summary>
public abstract class NiInterpController : NiTimeController { // X
    public bool ManagerControlled;

    public NiInterpController(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010068 && h.V <= 0x0A01006C) ManagerControlled = r.ReadBool32();
    }
}

/// <summary>
/// DEPRECATED (20.6)
/// </summary>
public class NiMultiTargetTransformController : NiInterpController {
    public int?[] ExtraTargets;                         // NiNode Targets to be controlled.

    public NiMultiTargetTransformController(BinaryReader r, Header h) : base(r, h) {
        ExtraTargets = r.ReadL16FArray(X<NiAVObject>.Ptr);
    }
}

/// <summary>
/// DEPRECATED (20.5), replaced by NiMorphMeshModifier.
/// Time controller for geometry morphing.
/// </summary>
public class NiGeomMorpherController : NiInterpController { // X
    public Flags ExtraFlags;                            // 1 = UPDATE NORMALS
    public int? Data;                                   // Geometry morphing data index.
    public byte AlwaysUpdate;
    public int?[] Interpolators;
    public MorphWeight[] InterpolatorWeights;
    public uint[] UnknownInts;                          // Unknown.

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
public class NiMorphController(BinaryReader r, Header h) : NiInterpController(r, h) {
}

/// <summary>
/// Unknown! Used by Daoc.
/// </summary>
public class NiMorpherController : NiInterpController {
    public int? Data;                                   // This controller's data.

    public NiMorpherController(BinaryReader r, Header h) : base(r, h) {
        Data = X<NiMorphData>.Ref(r);
    }
}

/// <summary>
/// Uses a single NiInterpolator to animate its target value.
/// </summary>
public abstract class NiSingleInterpController : NiInterpController { // X
    public int? Interpolator;

    public NiSingleInterpController(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010068) Interpolator = X<NiInterpolator>.Ref(r);
    }
}

/// <summary>
/// DEPRECATED (10.2), RENAMED (10.2) to NiTransformController
/// A time controller object for animation key frames.
/// </summary>
public class NiKeyframeController : NiSingleInterpController { // X
    public int? Data;

    public NiKeyframeController(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x0A010067) Data = X<NiKeyframeData>.Ref(r);
    }
}

/// <summary>
/// NiTransformController replaces NiKeyframeController.
/// </summary>
public class NiTransformController(BinaryReader r, Header h) : NiKeyframeController(r, h) {
}

/// <summary>
/// A particle system modifier controller.
/// NiInterpController::GetCtlrID() string format:
///     '%s'
/// Where %s = Value of "Modifier Name"
/// </summary>
public abstract class NiPSysModifierCtlr : NiSingleInterpController {
    public string ModifierName;                         // Used to find the modifier pointer.

    public NiPSysModifierCtlr(BinaryReader r, Header h) : base(r, h) {
        ModifierName = Y.String(r);
    }
}

/// <summary>
/// Particle system emitter controller.
/// NiInterpController::GetInterpolatorID() string format:
///     ['BirthRate', 'EmitterActive'] (for "Interpolator" and "Visibility Interpolator" respectively)
/// </summary>
public class NiPSysEmitterCtlr : NiPSysModifierCtlr {
    public int? VisibilityInterpolator;
    public int? Data;

    public NiPSysEmitterCtlr(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010000) VisibilityInterpolator = X<NiInterpolator>.Ref(r);
        if (h.V <= 0x0A010067) Data = X<NiPSysEmitterCtlrData>.Ref(r);
    }
}

/// <summary>
/// A particle system modifier controller that animates a boolean value for particles.
/// </summary>
public abstract class NiPSysModifierBoolCtlr(BinaryReader r, Header h) : NiPSysModifierCtlr(r, h) {
}

/// <summary>
/// A particle system modifier controller that animates active/inactive state for particles.
/// </summary>
public class NiPSysModifierActiveCtlr : NiPSysModifierBoolCtlr {
    public int? Data;

    public NiPSysModifierActiveCtlr(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x0A010067) Data = X<NiVisData>.Ref(r);
    }
}

/// <summary>
/// A particle system modifier controller that animates a floating point value for particles.
/// </summary>
public abstract class NiPSysModifierFloatCtlr : NiPSysModifierCtlr {
    public int? Data;

    public NiPSysModifierFloatCtlr(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x0A010067) Data = X<NiFloatData>.Ref(r);
    }
}

/// <summary>
/// Animates the declination value on an NiPSysEmitter object.
/// </summary>
public class NiPSysEmitterDeclinationCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Animates the declination variation value on an NiPSysEmitter object.
/// </summary>
public class NiPSysEmitterDeclinationVarCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Animates the size value on an NiPSysEmitter object.
/// </summary>
public class NiPSysEmitterInitialRadiusCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Animates the lifespan value on an NiPSysEmitter object.
/// </summary>
public class NiPSysEmitterLifeSpanCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Animates the speed value on an NiPSysEmitter object.
/// </summary>
public class NiPSysEmitterSpeedCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Animates the strength value of an NiPSysGravityModifier.
/// </summary>
public class NiPSysGravityStrengthCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target float value.
/// </summary>
public abstract class NiFloatInterpController(BinaryReader r, Header h) : NiSingleInterpController(r, h) { // X
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

    public NiFlipController(BinaryReader r, Header h) : base(r, h) {
        TextureSlot = (TexType)r.ReadUInt32();
        if (h.V >= 0x0303000D && h.V <= 0x0A010067) StartTime = r.ReadSingle();
        if (h.V <= 0x0A010067) Delta = r.ReadSingle();
        if (h.V >= 0x04000000) Sources = r.ReadL32FArray(X<NiSourceTexture>.Ref);
        else if (h.V <= 0x03010000) Images = r.ReadL32FArray(X<NiImage>.Ref);
        else r.ReadUInt32();
    }
}

/// <summary>
/// Animates the alpha value of a property using an interpolator.
/// </summary>
public class NiAlphaController : NiFloatInterpController { // X
    public int? Data;

    public NiAlphaController(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x0A010067) Data = X<NiFloatData>.Ref(r);
    }
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

    public NiTextureTransformController(BinaryReader r, Header h) : base(r, h) {
        ShaderMap = r.ReadBool32();
        TextureSlot = (TexType)r.ReadUInt32();
        Operation = (TransformMember)r.ReadUInt32();
        if (h.V <= 0x0A010067) Data = X<NiFloatData>.Ref(r);
    }
}

/// <summary>
/// Unknown controller.
/// </summary>
public class NiLightDimmerController(BinaryReader r, Header h) : NiFloatInterpController(r, h) {
}

/// <summary>
/// Abstract base class for all NiInterpControllers that use a NiInterpolator to animate their target boolean value.
/// </summary>
public abstract class NiBoolInterpController(BinaryReader r, Header h) : NiSingleInterpController(r, h) { // X
}

/// <summary>
/// Animates the visibility of an NiAVObject.
/// </summary>
public class NiVisController : NiBoolInterpController { // X
    public int? Data;

    public NiVisController(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x0A010067) Data = X<NiVisData>.Ref(r);
    }
}

/// <summary>
/// Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target NiPoint3 value.
/// </summary>
public abstract class NiPoint3InterpController(BinaryReader r, Header h) : NiSingleInterpController(r, h) { // X
}

/// <summary>
/// Time controller for material color. Flags are used for color selection in versions below 10.1.0.0.
/// Bits 4-5: Target Color (00 = Ambient, 01 = Diffuse, 10 = Specular, 11 = Emissive)
/// NiInterpController::GetCtlrID() string formats:
///     ['AMB', 'DIFF', 'SPEC', 'SELF_ILLUM'] (Depending on "Target Color")
/// </summary>
public class NiMaterialColorController : NiPoint3InterpController { // X
    public MaterialColor TargetColor;                   // Selects which color to control.
    public int? Data;

    public NiMaterialColorController(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010000) TargetColor = (MaterialColor)r.ReadUInt16();
        if (h.V <= 0x0A010067) Data = X<NiPosData>.Ref(r);
    }
}

/// <summary>
/// Animates the ambient, diffuse and specular colors of an NiLight.
/// NiInterpController::GetCtlrID() string formats:
///     ['Diffuse', 'Ambient'] (Depending on "Target Color")
/// </summary>
public class NiLightColorController : NiPoint3InterpController {
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
///     '%s'
/// Where %s = Value of "Extra Data Name"
/// </summary>
public abstract class NiExtraDataController : NiSingleInterpController {
    public string ExtraDataName;

    public NiExtraDataController(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A020000) ExtraDataName = Y.String(r);
    }
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
///     '%s[%d]'
/// Where %s = Value of "Extra Data Name", %d = Value of "Floats Extra Data Index"
/// </summary>
public class NiFloatsExtraDataController : NiExtraDataController {
    public int FloatsExtraDataIndex;
    public int? Data;

    public NiFloatsExtraDataController(BinaryReader r, Header h) : base(r, h) {
        FloatsExtraDataIndex = r.ReadInt32();
        if (h.V <= 0x0A010067) Data = X<NiFloatData>.Ref(r);
    }
}

/// <summary>
/// Animates an NiFloatsExtraData object attached to an NiAVObject.
/// NiInterpController::GetCtlrID() string format:
///     '%s[%d]'
/// Where %s = Value of "Extra Data Name", %d = Value of "Floats Extra Data Index"
/// </summary>
public class NiFloatsExtraDataPoint3Controller : NiExtraDataController {
    public int FloatsExtraDataIndex;

    public NiFloatsExtraDataPoint3Controller(BinaryReader r, Header h) : base(r, h) {
        FloatsExtraDataIndex = r.ReadInt32();
    }
}

/// <summary>
/// DEPRECATED (20.5), Replaced by NiSkinningLODController.
/// Level of detail controller for bones.  Priority is arranged from low to high.
/// </summary>
public class NiBoneLODController : NiTimeController {
    public uint LOD;                                    // Unknown.
    public uint NumLODs;                                // Number of LODs.
    public uint NumNodeGroups;                          // Number of node arrays.
    public NodeSet[] NodeGroups;                        // A list of node sets (each set a sequence of bones).
    public uint NumShapeGroups;                         // Number of shape groups.
    public SkinInfoSet[] ShapeGroups1;                  // List of shape groups.
    public uint NumShapeGroups2;                        // The size of the second list of shape groups.
    public int?[] ShapeGroups2;                         // Group of NiTriShape indices.
    public int UnknownInt2;                             // Unknown.
    public int UnknownInt3;                             // Unknown.

    public NiBoneLODController(BinaryReader r, Header h) : base(r, h) {
        LOD = r.ReadUInt32();
        NumLODs = r.ReadUInt32();
        NumNodeGroups = r.ReadUInt32();
        NodeGroups = r.ReadFArray(r => new NodeSet(r), NumLODs);
        if (h.V >= 0x04020200 && (Uh.V == 0)) NumShapeGroups = r.ReadUInt32();
        if (h.V >= 0x0A020000 && h.V <= 0x0A020000 && (Uh.V == 1)) NumShapeGroups = r.ReadUInt32();
        if (h.V >= 0x04020200 && (Uh.V == 0)) ShapeGroups1 = r.ReadFArray(r => new SkinInfoSet(r), NumShapeGroups);
        if (h.V >= 0x0A020000 && h.V <= 0x0A020000 && (Uh.V == 1)) ShapeGroups1 = r.ReadFArray(r => new SkinInfoSet(r), NumShapeGroups);
        if (h.V >= 0x04020200 && (Uh.V == 0)) NumShapeGroups2 = r.ReadUInt32();
        if (h.V >= 0x0A020000 && h.V <= 0x0A020000 && (Uh.V == 1)) NumShapeGroups2 = r.ReadUInt32();
        if (h.V >= 0x04020200 && (Uh.V == 0)) ShapeGroups2 = r.ReadFArray(X<NiTriBasedGeom>.Ref, NumShapeGroups2);
        if (h.V >= 0x0A020000 && h.V <= 0x0A020000 && (Uh.V == 1)) ShapeGroups2 = r.ReadFArray(X<NiTriBasedGeom>.Ref, NumShapeGroups2);
        if (h.V >= 0x14030009 && h.V <= 0x14030009 && (Uh.V == 0x20000) || (Uh.V == 0x30000)) {
            UnknownInt2 = r.ReadInt32();
            UnknownInt3 = r.ReadInt32();
        }
    }
}

/// <summary>
/// A simple LOD controller for bones.
/// </summary>
public class NiBSBoneLODController(BinaryReader r, Header h) : NiBoneLODController(r, h) {
}

public class MaterialData {
    public bool HasShader;                              // Shader.
    public string ShaderName;                           // The shader name.
    public int ShaderExtraData;                         // Extra data associated with the shader. A value of -1 means the shader is the default implementation.
    public uint NumMaterials;
    public string[] MaterialName;                       // The name of the material.
    public int[] MaterialExtraData;                     // Extra data associated with the material. A value of -1 means the material is the default implementation.
    public int ActiveMaterial = -1;                     // The index of the currently active material.
    public byte UnknownByte = 255;                      // Cyanide extension (only in version 10.2.0.0?).
    public int UnknownInteger2;                         // Unknown.
    public bool MaterialNeedsUpdate;                    // Whether the materials for this object always needs to be updated before rendering with them.

    public MaterialData(BinaryReader r, Header h) {
        if (HasShader && h.V >= 0x0A000100 && h.V <= 0x14010003) ShaderExtraData = r.ReadInt32();
        if (h.V >= 0x14020005) {
            NumMaterials = r.ReadUInt32();
            MaterialName = r.ReadFArray(r => Y.String(r), NumMaterials);
            MaterialExtraData = r.ReadPArray<uint>("i", NumMaterials);
            ActiveMaterial = r.ReadInt32();
        }
        if (h.V >= 0x0A020000 && h.V <= 0x0A020000 && (Uh.V == 1)) UnknownByte = r.ReadByte();
        if (h.V >= 0x0A040001 && h.V <= 0x0A040001) UnknownInteger2 = r.ReadInt32();
        if (h.V >= 0x14020007) MaterialNeedsUpdate = r.ReadBool32();
    }
}

/// <summary>
/// Describes a visible scene element with vertices like a mesh, a particle system, lines, etc.
/// </summary>
public abstract class NiGeometry : NiAVObject { // X
    public NiBound Bound;
    public int? Skin;
    public int? Data;                                   // Data index (NiTriShapeData/NiTriStripData).
    public int? SkinInstance;
    public MaterialData MaterialData;
    public int? ShaderProperty;
    public int? AlphaProperty;

    public NiGeometry(BinaryReader r, Header h) : base(r, h) {
        if (NiParticleSystem && (h.UV2 >= 100)) Skin = X<NiObject>.Ref(r);
        if ((h.UV2 < 100)) Data = X<NiGeometryData>.Ref(r);
        if (!NiParticleSystem && (h.UV2 >= 100)) Data = X<NiGeometryData>.Ref(r);
        if (h.V >= 0x0303000D && (h.UV2 < 100)) SkinInstance = X<NiSkinInstance>.Ref(r);
        if (!NiParticleSystem && (h.UV2 >= 100)) SkinInstance = X<NiSkinInstance>.Ref(r);
        if (h.V >= 0x0A000100 && (h.UV2 < 100)) MaterialData = new MaterialData(r, h);
        if (!NiParticleSystem && h.V >= 0x0A000100 && (h.UV2 >= 100)) MaterialData = new MaterialData(r, h);
        if (h.V >= 0x14020007 && (Uh.V == 12)) {
            ShaderProperty = X<BSShaderProperty>.Ref(r);
            AlphaProperty = X<NiAlphaProperty>.Ref(r);
        }
    }
}

/// <summary>
/// Describes a mesh, built from triangles.
/// </summary>
public abstract class NiTriBasedGeom(BinaryReader r, Header h) : NiGeometry(r, h) { // X
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

public abstract class AbstractAdditionalGeometryData(BinaryReader r, Header h) : NiObject(r, h) {
}

/// <summary>
/// Describes a mesh, built from triangles.
/// </summary>
public abstract class NiTriBasedGeomData : NiGeometryData { // X
    public ushort NumTriangles;                         // Number of triangles.

    public NiTriBasedGeomData(BinaryReader r, Header h) : base(r, h) {
        NumTriangles = r.ReadUInt16();
    }
}

/// <summary>
/// Unknown. Is apparently only used in skeleton.nif files.
/// </summary>
public class bhkBlendController : NiTimeController {
    public uint Keys;                                   // Seems to be always zero.

    public bhkBlendController(BinaryReader r, Header h) : base(r, h) {
        Keys = r.ReadUInt32();
    }
}

/// <summary>
/// Bethesda-specific collision bounding box for skeletons.
/// </summary>
public class BSBound : NiExtraData {
    public Vector3 Center;                              // Center of the bounding box.
    public Vector3 Dimensions;                          // Dimensions of the bounding box from center.

    public BSBound(BinaryReader r, Header h) : base(r, h) {
        Center = r.ReadVector3();
        Dimensions = r.ReadVector3();
    }
}

/// <summary>
/// Unknown. Marks furniture sitting positions?
/// </summary>
public class BSFurnitureMarker : NiExtraData {
    public FurniturePosition[] Positions;

    public BSFurnitureMarker(BinaryReader r, Header h) : base(r, h) {
        Positions = r.ReadL32FArray(r => new FurniturePosition(r, h));
    }
}

/// <summary>
/// Particle modifier that adds a blend of object space translation and rotation to particles born in world space.
/// </summary>
public class BSParentVelocityModifier : NiPSysModifier {
    public float Damping;                               // Amount of blending?

    public BSParentVelocityModifier(BinaryReader r, Header h) : base(r, h) {
        Damping = r.ReadSingle();
    }
}

/// <summary>
/// Particle emitter that uses a node, its children and subchildren to emit from.  Emission will be evenly spread along points from nodes leading to their direct parents/children only.
/// </summary>
public class BSPSysArrayEmitter(BinaryReader r, Header h) : NiPSysVolumeEmitter(r, h) {
}

/// <summary>
/// Particle Modifier that uses the wind value from the gamedata to alter the path of particles.
/// </summary>
public class BSWindModifier : NiPSysModifier {
    public float Strength;                              // The amount of force wind will have on particles.

    public BSWindModifier(BinaryReader r, Header h) : base(r, h) {
        Strength = r.ReadSingle();
    }
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

    public hkPackedNiTriStripsData(BinaryReader r, Header h) : base(r, h) {
        Triangles = r.ReadL32FArray(r => new TriangleData(r, h));
        NumVertices = r.ReadUInt32();
        if (h.V >= 0x14020007) UnknownByte1 = r.ReadByte();
        Vertices = r.ReadPArray<Vector3>("3f", NumVertices);
        if (h.V >= 0x14020007) SubShapes = r.ReadL16FArray(r => new OblivionSubShape(r, h));
    }
}

/// <summary>
/// Transparency. Flags 0x00ED.
/// </summary>
public class NiAlphaProperty : NiProperty { // X
    public Flags Flags = 4844;                          // Bit 0 : alpha blending enable
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
    public byte Threshold = 128;                        // Threshold for alpha testing (see: glAlphaFunc)
    public ushort UnknownShort1;                        // Unknown
    public uint UnknownInt2;                            // Unknown

    public NiAlphaProperty(BinaryReader r, Header h) : base(r, h) {
        Flags = (Flags)r.ReadUInt16();
        Threshold = r.ReadByte();
        if (h.V <= 0x02030000) UnknownShort1 = r.ReadUInt16();
        if (h.V >= 0x14030101 && h.V <= 0x14030102) UnknownShort1 = r.ReadUInt16();
        if (h.V <= 0x02030000) UnknownInt2 = r.ReadUInt32();
    }
}

/// <summary>
/// Ambient light source.
/// </summary>
public class NiAmbientLight(BinaryReader r, Header h) : NiLight(r, h) {
}

/// <summary>
/// Generic rotating particles data object.
/// </summary>
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

/// <summary>
/// Rotating particles data object.
/// </summary>
public class NiRotatingParticlesData : NiParticlesData { // X
    public Quaternion[] Rotations;

    public NiRotatingParticlesData(BinaryReader r, Header h) : base(r, h) {
        Rotations = r.ReadBool32() ? r.ReadFArray(r => r.ReadQuaternionWFirst(), NumVertices) : [];
    }
}

/// <summary>
/// Particle system data object (with automatic normals?).
/// </summary>
public class NiAutoNormalParticlesData(BinaryReader r, Header h) : NiParticlesData(r, h) { // X
}

/// <summary>
/// Particle Description.
/// </summary>
public class ParticleDesc(BinaryReader r, Header h) {
    public Vector3 Translation = r.ReadVector3();       // Unknown.
    public float[] UnknownFloats1 = h.V <= 0x0A040001 ? r.ReadPArray<float>("f", 3) : default; // Unknown.
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

    public NiPSysData(BinaryReader r, Header h) : base(r, h) {
        if (!((h.V == 0x14020007) && (h.UV2 > 0))) ParticleDescriptions = r.ReadFArray(r => new ParticleDesc(r, h), NumVertices);
        if (h.V >= 0x14000002) HasRotationSpeeds = r.ReadBool32();
        if (HasRotationSpeeds && h.V >= 0x14000002 && !((h.V == 0x14020007) && (h.UV2 > 0))) RotationSpeeds = r.ReadPArray<float>("f", NumVertices);
        if (!((h.V == 0x14020007) && (h.UV2 > 0))) {
            NumAddedParticles = r.ReadUInt16();
            AddedParticlesBase = r.ReadUInt16();
        }
    }
}

/// <summary>
/// Particle meshes data.
/// </summary>
public class NiMeshPSysData : NiPSysData {
    public uint DefaultPoolSize;
    public bool FillPoolsOnLoad;
    public uint[] Generations;
    public int? ParticleMeshes;

    public NiMeshPSysData(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A020000) Generations = r.ReadL32PArray<uint>("I");
        ParticleMeshes = X<NiNode>.Ref(r);
    }
}

/// <summary>
/// Binary extra data object. Used to store tangents and bitangents in Oblivion.
/// </summary>
public class NiBinaryExtraData : NiExtraData {
    public byte[] BinaryData;                           // The binary data.

    public NiBinaryExtraData(BinaryReader r, Header h) : base(r, h) {
        BinaryData = r.ReadL8Bytes();
    }
}

/// <summary>
/// Voxel extra data object.
/// </summary>
public class NiBinaryVoxelExtraData : NiExtraData {
    public uint UnknownInt = 0;                         // Unknown.  0?
    public int? Data;                                   // Link to binary voxel data.

    public NiBinaryVoxelExtraData(BinaryReader r, Header h) : base(r, h) {
        UnknownInt = r.ReadUInt32();
        Data = X<NiBinaryVoxelData>.Ref(r);
    }
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

    public NiBinaryVoxelData(BinaryReader r, Header h) : base(r, h) {
        UnknownShort1 = r.ReadUInt16();
        UnknownShort2 = r.ReadUInt16();
        UnknownShort3 = r.ReadUInt16();
        Unknown7Floats = r.ReadPArray<float>("f", 7);
        UnknownBytes1 = r.ReadFArray(k => r.ReadBytes(7), 12);
        UnknownVectors = r.ReadL32PArray<Vector4>("4f");
        UnknownBytes2 = r.ReadL32Bytes(L32);
        Unknown5Ints = r.ReadPArray<uint>("I", 5);
    }
}

/// <summary>
/// Blends bool values together.
/// </summary>
public class NiBlendBoolInterpolator : NiBlendInterpolator {
    public byte Value = 2;                              // The pose value. Invalid if using data.

    public NiBlendBoolInterpolator(BinaryReader r, Header h) : base(r, h) {
        Value = r.ReadByte();
    }
}

/// <summary>
/// Blends float values together.
/// </summary>
public class NiBlendFloatInterpolator : NiBlendInterpolator {
    public float Value = -3.402823466e+38f;             // The pose value. Invalid if using data.

    public NiBlendFloatInterpolator(BinaryReader r, Header h) : base(r, h) {
        Value = r.ReadSingle();
    }
}

/// <summary>
/// Blends NiPoint3 values together.
/// </summary>
public class NiBlendPoint3Interpolator : NiBlendInterpolator {
    public Vector3 Value = new Vector3(-3.402823466e+38, -3.402823466e+38, -3.402823466e+38); // The pose value. Invalid if using data.

    public NiBlendPoint3Interpolator(BinaryReader r, Header h) : base(r, h) {
        Value = r.ReadVector3();
    }
}

/// <summary>
/// Blends NiQuatTransform values together.
/// </summary>
public class NiBlendTransformInterpolator : NiBlendInterpolator {
    public NiQuatTransform Value;

    public NiBlendTransformInterpolator(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x0A01006D) Value = new NiQuatTransform(r, h);
    }
}

/// <summary>
/// Wrapper for boolean animation keys.
/// </summary>
public class NiBoolData : NiObject {
    public KeyGroup<byte> Data;                         // The boolean keys.

    public NiBoolData(BinaryReader r, Header h) : base(r, h) {
        Data = new KeyGroup<byte>(r);
    }
}

/// <summary>
/// Boolean extra data.
/// </summary>
public class NiBooleanExtraData : NiExtraData {
    public byte BooleanData;                            // The boolean extra data value.

    public NiBooleanExtraData(BinaryReader r, Header h) : base(r, h) {
        BooleanData = r.ReadByte();
    }
}

/// <summary>
/// Contains an NiBSplineBasis for use in interpolation of open, uniform B-Splines.
/// </summary>
public class NiBSplineBasisData : NiObject {
    public uint NumControlPoints;                       // The number of control points of the B-spline (number of frames of animation plus degree of B-spline minus one).

    public NiBSplineBasisData(BinaryReader r, Header h) : base(r, h) {
        NumControlPoints = r.ReadUInt32();
    }
}

/// <summary>
/// Uses B-Splines to animate a float value over time.
/// </summary>
public abstract class NiBSplineFloatInterpolator : NiBSplineInterpolator {
    public float Value = -3.402823466e+38f;             // Base value when curve not defined.
    public uint Handle = 0xFFFF;                        // Handle into the data. (USHRT_MAX for invalid handle.)

    public NiBSplineFloatInterpolator(BinaryReader r, Header h) : base(r, h) {
        Value = r.ReadSingle();
        Handle = r.ReadUInt32();
    }
}

/// <summary>
/// NiBSplineFloatInterpolator plus the information required for using compact control points.
/// </summary>
public class NiBSplineCompFloatInterpolator : NiBSplineFloatInterpolator {
    public float FloatOffset = 3.402823466e+38f;
    public float FloatHalfRange = 3.402823466e+38f;

    public NiBSplineCompFloatInterpolator(BinaryReader r, Header h) : base(r, h) {
        FloatOffset = r.ReadSingle();
        FloatHalfRange = r.ReadSingle();
    }
}

/// <summary>
/// Uses B-Splines to animate an NiPoint3 value over time.
/// </summary>
public abstract class NiBSplinePoint3Interpolator : NiBSplineInterpolator {
    public Vector3 Value = new Vector3(-3.402823466e+38, -3.402823466e+38, -3.402823466e+38); // Base value when curve not defined.
    public uint Handle = 0xFFFF;                        // Handle into the data. (USHRT_MAX for invalid handle.)

    public NiBSplinePoint3Interpolator(BinaryReader r, Header h) : base(r, h) {
        Value = r.ReadVector3();
        Handle = r.ReadUInt32();
    }
}

/// <summary>
/// NiBSplinePoint3Interpolator plus the information required for using compact control points.
/// </summary>
public class NiBSplineCompPoint3Interpolator : NiBSplinePoint3Interpolator {
    public float PositionOffset = 3.402823466e+38f;
    public float PositionHalfRange = 3.402823466e+38f;

    public NiBSplineCompPoint3Interpolator(BinaryReader r, Header h) : base(r, h) {
        PositionOffset = r.ReadSingle();
        PositionHalfRange = r.ReadSingle();
    }
}

/// <summary>
/// Supports the animation of position, rotation, and scale using an NiQuatTransform.
/// The NiQuatTransform can be an unchanging pose or interpolated from B-Spline control point channels.
/// </summary>
public class NiBSplineTransformInterpolator : NiBSplineInterpolator {
    public NiQuatTransform Transform;
    public uint TranslationHandle = 0xFFFF;             // Handle into the translation data. (USHRT_MAX for invalid handle.)
    public uint RotationHandle = 0xFFFF;                // Handle into the rotation data. (USHRT_MAX for invalid handle.)
    public uint ScaleHandle = 0xFFFF;                   // Handle into the scale data. (USHRT_MAX for invalid handle.)

    public NiBSplineTransformInterpolator(BinaryReader r, Header h) : base(r, h) {
        Transform = new NiQuatTransform(r, h);
        TranslationHandle = r.ReadUInt32();
        RotationHandle = r.ReadUInt32();
        ScaleHandle = r.ReadUInt32();
    }
}

/// <summary>
/// NiBSplineTransformInterpolator plus the information required for using compact control points.
/// </summary>
public class NiBSplineCompTransformInterpolator : NiBSplineTransformInterpolator {
    public float TranslationOffset = 3.402823466e+38f;
    public float TranslationHalfRange = 3.402823466e+38f;
    public float RotationOffset = 3.402823466e+38f;
    public float RotationHalfRange = 3.402823466e+38f;
    public float ScaleOffset = 3.402823466e+38f;
    public float ScaleHalfRange = 3.402823466e+38f;

    public NiBSplineCompTransformInterpolator(BinaryReader r, Header h) : base(r, h) {
        TranslationOffset = r.ReadSingle();
        TranslationHalfRange = r.ReadSingle();
        RotationOffset = r.ReadSingle();
        RotationHalfRange = r.ReadSingle();
        ScaleOffset = r.ReadSingle();
        ScaleHalfRange = r.ReadSingle();
    }
}

public class BSRotAccumTransfInterpolator(BinaryReader r, Header h) : NiTransformInterpolator(r, h) {
}

/// <summary>
/// Contains one or more sets of control points for use in interpolation of open, uniform B-Splines, stored as either float or compact.
/// </summary>
public class NiBSplineData : NiObject {
    public float[] FloatControlPoints;                  // Float values representing the control data.
    public short[] CompactControlPoints;                // Signed shorts representing the data from 0 to 1 (scaled by SHRT_MAX).

    public NiBSplineData(BinaryReader r, Header h) : base(r, h) {
        FloatControlPoints = r.ReadL32PArray<float>("f");
        CompactControlPoints = r.ReadL32PArray<short>("h");
    }
}

/// <summary>
/// Camera object.
/// </summary>
public class NiCamera : NiAVObject { // X
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
    public uint NumScreenPolygons = 0;                  // Deprecated. Array is always zero length on disk write.
    public uint NumScreenTextures = 0;                  // Deprecated. Array is always zero length on disk write.
    public uint UnknownInt3;                            // Unknown.

    public NiCamera(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010000) CameraFlags = r.ReadUInt16();
        FrustumLeft = r.ReadSingle();
        FrustumRight = r.ReadSingle();
        FrustumTop = r.ReadSingle();
        FrustumBottom = r.ReadSingle();
        FrustumNear = r.ReadSingle();
        FrustumFar = r.ReadSingle();
        if (h.V >= 0x0A010000) UseOrthographicProjection = r.ReadBool32();
        ViewportLeft = r.ReadSingle();
        ViewportRight = r.ReadSingle();
        ViewportTop = r.ReadSingle();
        ViewportBottom = r.ReadSingle();
        LODAdjust = r.ReadSingle();
        Scene = X<NiAVObject>.Ref(r);
        NumScreenPolygons = r.ReadUInt32();
        if (h.V >= 0x04020100) NumScreenTextures = r.ReadUInt32();
        if (h.V <= 0x03010000) UnknownInt3 = r.ReadUInt32();
    }
}

/// <summary>
/// Wrapper for color animation keys.
/// </summary>
public class NiColorData : NiObject { // X
    public KeyGroup<Color4> Data;                       // The color keys.

    public NiColorData(BinaryReader r, Header h) : base(r, h) {
        Data = new KeyGroup<Color4>(r);
    }
}

/// <summary>
/// Extra data in the form of NiColorA (red, green, blue, alpha).
/// </summary>
public class NiColorExtraData : NiExtraData {
    public Color4 Data;                                 // RGBA Color?

    public NiColorExtraData(BinaryReader r, Header h) : base(r, h) {
        Data = new Color4(r);
    }
}

/// <summary>
/// Controls animation sequences on a specific branch of the scene graph.
/// </summary>
public class NiControllerManager : NiTimeController {
    public bool Cumulative;                             // Whether transformation accumulation is enabled. If accumulation is not enabled, the manager will treat all sequence data on the accumulation root as absolute data instead of relative delta values.
    public int?[] ControllerSequences;
    public int? ObjectPalette;

    public NiControllerManager(BinaryReader r, Header h) : base(r, h) {
        Cumulative = r.ReadBool32();
        ControllerSequences = r.ReadL32FArray(X<NiControllerSequence>.Ref);
        ObjectPalette = X<NiDefaultAVObjectPalette>.Ref(r);
    }
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

    public NiSequence(BinaryReader r, Header h) : base(r, h) {
        Name = Y.String(r);
        if (h.V <= 0x0A010067) {
            AccumRootName = Y.String(r);
            TextKeys = X<NiTextKeyExtraData>.Ref(r);
        }
        if (h.V == 0x14030009 && (h.UV == 0x20000) || (h.UV == 0x30000)) {
            UnknownInt4 = r.ReadInt32();
            UnknownInt5 = r.ReadInt32();
        }
        NumControlledBlocks = r.ReadUInt32();
        if (h.V >= 0x0A01006A) ArrayGrowBy = r.ReadUInt32();
        ControlledBlocks = r.ReadFArray(r => new ControlledBlock(r, h), NumControlledBlocks);
    }
}

/// <summary>
/// Root node in Gamebryo .kf files (version 10.0.1.0 and up).
/// </summary>
public class NiControllerSequence : NiSequence {
    public float Weight = 1.0f;                         // The weight of a sequence describes how it blends with other sequences at the same priority.
    public int? TextKeys;
    public CycleType CycleType;
    public float Frequency = 1.0f;
    public float Phase;
    public float StartTime = 3.402823466e+38f;
    public float StopTime = -3.402823466e+38f;
    public bool PlayBackwards;
    public int? Manager;                                // The owner of this sequence.
    public string AccumRootName;                        // The name of the NiAVObject serving as the accumulation root. This is where all accumulated translations, scales, and rotations are applied.
    public AccumFlags AccumFlags = ACCUM_X_FRONT;
    public int? StringPalette;
    public int? AnimNotes;
    public int?[] AnimNoteArrays;

    public NiControllerSequence(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A01006A) Frequency = r.ReadSingle();
        if (h.V >= 0x0A01006A && h.V <= 0x0A040001) Phase = r.ReadSingle();
        if (h.V >= 0x0A01006A) {
            StartTime = r.ReadSingle();
            StopTime = r.ReadSingle();
        }
        if (h.V == 0x0A01006A) PlayBackwards = r.ReadBool32();
        if (h.V >= 0x0A01006A) {
            Manager = X<NiControllerManager>.Ptr(r);
            AccumRootName = Y.String(r);
        }
        if (h.V >= 0x14030008) AccumFlags = (AccumFlags)r.ReadUInt32();
        if (h.V >= 0x0A010071 && h.V <= 0x14010000) StringPalette = X<NiStringPalette>.Ref(r);
        if (h.V >= 0x14020007 && (h.UV2 >= 24) && (h.UV2 <= 28)) AnimNotes = X<BSAnimNotes>.Ref(r);
        if (h.V >= 0x14020007 && (h.UV2 > 28)) AnimNoteArrays = r.ReadL16FArray(X<BSAnimNotes>.Ref);
    }
}

/// <summary>
/// Abstract base class for indexing NiAVObject by name.
/// </summary>
public abstract class NiAVObjectPalette(BinaryReader r, Header h) : NiObject(r, h) {
}

/// <summary>
/// NiAVObjectPalette implementation. Used to quickly look up objects by name.
/// </summary>
public class NiDefaultAVObjectPalette : NiAVObjectPalette {
    public int? Scene;                                  // Scene root of the object palette.
    public AVObject[] Objs;                             // The objects.

    public NiDefaultAVObjectPalette(BinaryReader r, Header h) : base(r, h) {
        Scene = X<NiAVObject>.Ptr(r);
        Objs = r.ReadL32FArray(r => new AVObject(r));
    }
}

/// <summary>
/// Directional light source.
/// </summary>
public class NiDirectionalLight(BinaryReader r, Header h) : NiLight(r, h) {
}

/// <summary>
/// NiDitherProperty allows the application to turn the dithering of interpolated colors and fog values on and off.
/// </summary>
public class NiDitherProperty : NiProperty {
    public Flags Flags;                                 // 1 = Enable dithering

    public NiDitherProperty(BinaryReader r, Header h) : base(r, h) {
        Flags = (Flags)r.ReadUInt16();
    }
}

/// <summary>
/// DEPRECATED (10.2), REMOVED (20.5). Replaced by NiTransformController and NiLookAtInterpolator.
/// </summary>
public class NiRollController : NiSingleInterpController {
    public int? Data;                                   // The data for the controller.

    public NiRollController(BinaryReader r, Header h) : base(r, h) {
        Data = X<NiFloatData>.Ref(r);
    }
}

/// <summary>
/// Wrapper for 1D (one-dimensional) floating point animation keys.
/// </summary>
public class NiFloatData : NiObject { // X
    public KeyGroup<float> Data;                        // The keys.

    public NiFloatData(BinaryReader r, Header h) : base(r, h) {
        Data = new KeyGroup<float>(r);
    }
}

/// <summary>
/// Extra float data.
/// </summary>
public class NiFloatExtraData : NiExtraData {
    public float FloatData;                             // The float data.

    public NiFloatExtraData(BinaryReader r, Header h) : base(r, h) {
        FloatData = r.ReadSingle();
    }
}

/// <summary>
/// Extra float array data.
/// </summary>
public class NiFloatsExtraData : NiExtraData {
    public float[] Data;                                // Float data.

    public NiFloatsExtraData(BinaryReader r, Header h) : base(r, h) {
        Data = r.ReadL32PArray<float>("f");
    }
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
    public float FogDepth = 1.0f;                       // Depth of the fog in normalized units. 1.0 = begins at near plane. 0.5 = begins halfway between the near and far planes.
    public Color3 FogColor;                             // The color of the fog.

    public NiFogProperty(BinaryReader r, Header h) : base(r, h) {
        Flags = (Flags)r.ReadUInt16();
        FogDepth = r.ReadSingle();
        FogColor = new Color3(r);
    }
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier. Applies a gravitational field on the particles.
/// </summary>
public class NiGravity : NiParticleModifier { // X
    public float UnknownFloat1;                         // Unknown.
    public float Force;                                 // The strength/force of this gravity.
    public FieldType Type;                              // The force field type.
    public Vector3 Position;                            // The position of the mass point relative to the particle system.
    public Vector3 Direction;                           // The direction of the applied acceleration.

    public NiGravity(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0303000D) UnknownFloat1 = r.ReadSingle();
        Force = r.ReadSingle();
        Type = (FieldType)r.ReadUInt32();
        Position = r.ReadVector3();
        Direction = r.ReadVector3();
    }
}

/// <summary>
/// Extra integer data.
/// </summary>
public class NiIntegerExtraData : NiExtraData {
    public uint IntegerData;                            // The value of the extra data.

    public NiIntegerExtraData(BinaryReader r, Header h) : base(r, h) {
        IntegerData = r.ReadUInt32();
    }
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
public class BSXFlags(BinaryReader r, Header h) : NiIntegerExtraData(r, h) {
}

/// <summary>
/// Extra integer array data.
/// </summary>
public class NiIntegersExtraData : NiExtraData {
    public uint[] Data;                                 // Integers.

    public NiIntegersExtraData(BinaryReader r, Header h) : base(r, h) {
        Data = r.ReadL32PArray<uint>("I");
    }
}

/// <summary>
/// An extended keyframe controller.
/// </summary>
public class BSKeyframeController : NiKeyframeController {
    public int? Data2;                                  // A link to more keyframe data.

    public BSKeyframeController(BinaryReader r, Header h) : base(r, h) {
        Data2 = X<NiKeyframeData>.Ref(r);
    }
}

/// <summary>
/// DEPRECATED (10.2), RENAMED (10.2) to NiTransformData.
/// Wrapper for transformation animation keys.
/// </summary>
public class NiKeyframeData : NiObject { // X
    public uint NumRotationKeys;                        // The number of quaternion rotation keys. If the rotation type is XYZ (type 4) then this *must* be set to 1, and in this case the actual number of keys is stored in the XYZ Rotations field.
    public KeyType RotationType;                        // The type of interpolation to use for rotation.  Can also be 4 to indicate that separate X, Y, and Z values are used for the rotation instead of Quaternions.
    public QuatKey<Quaternion>[] QuaternionKeys;        // The rotation keys if Quaternion rotation is used.
    public float Order;
    public KeyGroup<float>[] XYZRotations;              // Individual arrays of keys for rotating X, Y, and Z individually.
    public KeyGroup<Vector3> Translations;              // Translation keys.
    public KeyGroup<float> Scales;                      // Scale keys.

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

    public NiLookAtController(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010000) Flags = (LookAtFlags)r.ReadUInt16();
        LookAt = X<NiNode>.Ptr(r);
    }
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

    public NiLookAtInterpolator(BinaryReader r, Header h) : base(r, h) {
        Flags = (LookAtFlags)r.ReadUInt16();
        LookAt = X<NiNode>.Ptr(r);
        LookAtName = Y.String(r);
        if (h.V <= 0x1404000C) Transform = new NiQuatTransform(r, h);
        Interpolator:Translation = X<NiPoint3Interpolator>.Ref(r);
        Interpolator:Roll = X<NiFloatInterpolator>.Ref(r);
        Interpolator:Scale = X<NiFloatInterpolator>.Ref(r);
    }
}

/// <summary>
/// Describes the surface properties of an object e.g. translucency, ambient color, diffuse color, emissive color, and specular color.
/// </summary>
public class NiMaterialProperty : NiProperty { // X
    public Flags Flags;                                 // Property flags.
    public Color3 AmbientColor = 1.0, 1.0, 1.0;         // How much the material reflects ambient light.
    public Color3 DiffuseColor = 1.0, 1.0, 1.0;         // How much the material reflects diffuse light.
    public Color3 SpecularColor = 1.0, 1.0, 1.0;        // How much light the material reflects in a specular manner.
    public Color3 EmissiveColor = 0.0, 0.0, 0.0;        // How much light the material emits.
    public float Glossiness = 10.0f;                    // The material glossiness.
    public float Alpha = 1.0f;                          // The material transparency (1=non-transparant). Refer to a NiAlphaProperty object in this material's parent NiTriShape object, when alpha is not 1.
    public float EmissiveMult = 1.0f;

    public NiMaterialProperty(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x03000000 && h.V <= 0x0A000102) Flags = (Flags)r.ReadUInt16();
        if ((h.UV2 < 26)) {
            AmbientColor = new Color3(r);
            DiffuseColor = new Color3(r);
        }
        SpecularColor = new Color3(r);
        EmissiveColor = new Color3(r);
        Glossiness = r.ReadSingle();
        Alpha = r.ReadSingle();
        if ((h.UV2 > 21)) EmissiveMult = r.ReadSingle();
    }
}

/// <summary>
/// DEPRECATED (20.5), replaced by NiMorphMeshModifier.
/// Geometry morphing data.
/// </summary>
public class NiMorphData : NiObject { // X
    public uint NumMorphs;                              // Number of morphing object.
    public uint NumVertices;                            // Number of vertices.
    public byte RelativeTargets = 1;                    // This byte is always 1 in all official files.
    public Morph[] Morphs;                              // The geometry morphing objects.

    public NiMorphData(BinaryReader r, Header h) : base(r, h) {
        NumMorphs = r.ReadUInt32();
        NumVertices = r.ReadUInt32();
        RelativeTargets = r.ReadByte();
        Morphs = r.ReadFArray(r => new Morph(r, NumVertices), NumMorphs);
    }
}

/// <summary>
/// Generic node object for grouping.
/// </summary>
public class NiNode : NiAVObject { // X
    public int?[] Children;                             // List of child node object indices.
    public int?[] Effects;                              // List of node effects. ADynamicEffect?

    public NiNode(BinaryReader r, Header h) : base(r, h) {
        Children = r.ReadL32FArray(X<NiAVObject>.Ref);
        if (h.UV2 < 130) Effects = r.ReadL32FArray(X<NiDynamicEffect>.Ref);
    }
}

/// <summary>
/// A NiNode used as a skeleton bone?
/// </summary>
public class NiBone(BinaryReader r, Header h) : NiNode(r, h) {
}

/// <summary>
/// Morrowind specific.
/// </summary>
public class AvoidNode(BinaryReader r, Header h) : NiNode(r, h) { // X
}

/// <summary>
/// Firaxis-specific UI widgets?
/// </summary>
public class FxWidget : NiNode {
    public byte Unknown3;                               // Unknown.
    public byte[] Unknown292Bytes;                      // Looks like 9 links and some string data.

    public FxWidget(BinaryReader r, Header h) : base(r, h) {
        Unknown3 = r.ReadByte();
        Unknown292Bytes = r.ReadBytes(292);
    }
}

/// <summary>
/// Unknown.
/// </summary>
public class FxButton(BinaryReader r, Header h) : FxWidget(r, h) {
}

/// <summary>
/// Unknown.
/// </summary>
public class FxRadioButton : FxWidget {
    public uint UnknownInt1;                            // Unknown.
    public uint UnknownInt2;                            // Unknown.
    public uint UnknownInt3;                            // Unknown.
    public int?[] Buttons;                              // Unknown pointers to other buttons.  Maybe other buttons in a group so they can be switch off if this one is switched on?

    public FxRadioButton(BinaryReader r, Header h) : base(r, h) {
        UnknownInt1 = r.ReadUInt32();
        UnknownInt2 = r.ReadUInt32();
        UnknownInt3 = r.ReadUInt32();
        Buttons = r.ReadL32FArray(X<FxRadioButton>.Ptr);
    }
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
public class NiBillboardNode : NiNode { // X
    public BillboardMode BillboardMode;                 // The way the billboard will react to the camera.

    public NiBillboardNode(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010000) BillboardMode = (BillboardMode)r.ReadUInt16();
    }
}

/// <summary>
/// Bethesda-specific extension of Node with animation properties stored in the flags, often 42?
/// </summary>
public class NiBSAnimationNode(BinaryReader r, Header h) : NiNode(r, h) { // X
}

/// <summary>
/// Unknown.
/// </summary>
public class NiBSParticleNode(BinaryReader r, Header h) : NiNode(r, h) { // X
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

    public NiSwitchNode(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010000) SwitchNodeFlags = (NiSwitchFlags)r.ReadUInt16();
        Index = r.ReadUInt32();
    }
}

/// <summary>
/// Level of detail selector. Links to different levels of detail of the same model, used to switch a geometry at a specified distance.
/// </summary>
public class NiLODNode : NiSwitchNode {
    public Vector3 LODCenter;
    public LODRange[] LODLevels;
    public int? LODLevelData;

    public NiLODNode(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x04000002 && h.V <= 0x0A000100) LODCenter = r.ReadVector3();
        if (h.V <= 0x0A000100) LODLevels = r.ReadL32FArray(r => new LODRange(r, h));
        if (h.V >= 0x0A010000) LODLevelData = X<NiLODData>.Ref(r);
    }
}

/// <summary>
/// NiPalette objects represent mappings from 8-bit indices to 24-bit RGB or 32-bit RGBA colors.
/// </summary>
public class NiPalette : NiObject {
    public byte HasAlpha;
    public uint NumEntries = 256;                       // The number of palette entries. Always 256 but can also be 16.
    public ByteColor4[] Palette;                        // The color palette.

    public NiPalette(BinaryReader r, Header h) : base(r, h) {
        HasAlpha = r.ReadByte();
        NumEntries = r.ReadUInt32();
        if (NumEntries == 16) Palette = r.ReadFArray(r => new Color4Byte(r), 16);
        if (NumEntries != 16) Palette = r.ReadFArray(r => new Color4Byte(r), 256);
    }
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiParticleBomb : NiParticleModifier { // X
    public float Decay;
    public float Duration;
    public float DeltaV;
    public float Start;
    public DecayType DecayType;
    public SymmetryType SymmetryType;
    public Vector3 Position;                            // The position of the mass point relative to the particle system?
    public Vector3 Direction;                           // The direction of the applied acceleration?

    public NiParticleBomb(BinaryReader r, Header h) : base(r, h) {
        Decay = r.ReadSingle();
        Duration = r.ReadSingle();
        DeltaV = r.ReadSingle();
        Start = r.ReadSingle();
        DecayType = (DecayType)r.ReadUInt32();
        if (h.V >= 0x0401000C) SymmetryType = (SymmetryType)r.ReadUInt32();
        Position = r.ReadVector3();
        Direction = r.ReadVector3();
    }
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiParticleColorModifier : NiParticleModifier { // X
    public int? ColorData;

    public NiParticleColorModifier(BinaryReader r, Header h) : base(r, h) {
        ColorData = X<NiColorData>.Ref(r);
    }
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiParticleGrowFade : NiParticleModifier { // X
    public float Grow;                                  // The time from the beginning of the particle lifetime during which the particle grows.
    public float Fade;                                  // The time from the end of the particle lifetime during which the particle fades.

    public NiParticleGrowFade(BinaryReader r, Header h) : base(r, h) {
        Grow = r.ReadSingle();
        Fade = r.ReadSingle();
    }
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiParticleMeshModifier : NiParticleModifier { // X
    public int?[] ParticleMeshes;

    public NiParticleMeshModifier(BinaryReader r, Header h) : base(r, h) {
        ParticleMeshes = r.ReadL32FArray(X<NiAVObject>.Ref);
    }
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiParticleRotation : NiParticleModifier { // X
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
/// Generic particle system node.
/// </summary>
public class NiParticles : NiGeometry { // X
    public BSVertexDesc VertexDesc;

    public NiParticles(BinaryReader r, Header h) : base(r, h) {
        if ((h.UV2 >= 100)) VertexDesc = new BSVertexDesc(r);
    }
}

/// <summary>
/// LEGACY (pre-10.1). NiParticles which do not house normals and generate them at runtime.
/// </summary>
public class NiAutoNormalParticles(BinaryReader r, Header h) : NiParticles(r, h) { // X
}

/// <summary>
/// LEGACY (pre-10.1). Particle meshes.
/// </summary>
public class NiParticleMeshes(BinaryReader r, Header h) : NiParticles(r, h) {
}

/// <summary>
/// LEGACY (pre-10.1). Particle meshes data.
/// </summary>
public class NiParticleMeshesData : NiRotatingParticlesData {
    public int? UnknownLink2;                           // Refers to the mesh that makes up a particle?

    public NiParticleMeshesData(BinaryReader r, Header h) : base(r, h) {
        UnknownLink2 = X<NiAVObject>.Ref(r);
    }
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
    public bool WorldSpace = 1;                         // If true, Particles are birthed into world space.  If false, Particles are birthed into object space.
    public int?[] Modifiers;                            // The list of particle modifiers.

    public NiParticleSystem(BinaryReader r, Header h) : base(r, h) {
        if ((h.UV2 >= 83)) NearEnd = r.ReadUInt16();
        if ((h.UV2 >= 100)) Data = X<NiPSysData>.Ref(r);
        if (h.V >= 0x0A010000) {
            WorldSpace = r.ReadBool32();
            Modifiers = r.ReadL32FArray(X<NiPSysModifier>.Ref);
        }
    }
}

/// <summary>
/// Particle system.
/// </summary>
public class NiMeshParticleSystem(BinaryReader r, Header h) : NiParticleSystem(r, h) {
}

/// <summary>
/// A generic particle system time controller object.
/// </summary>
public class NiParticleSystemController : NiTimeController { // X
    public uint OldSpeed;                               // Particle speed in old files
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
        if (h.V <= 0x03010000) OldSpeed = r.ReadUInt32();
        if (h.V >= 0x0303000D) Speed = r.ReadSingle();
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
        if (h.V >= 0x04000002) UnknownByte = r.ReadByte();
        if (h.V <= 0x03010000) OldEmitRate = r.ReadUInt32();
        if (h.V >= 0x0303000D) EmitRate = r.ReadSingle();
        Lifetime = r.ReadSingle();
        LifetimeRandom = r.ReadSingle();
        if (h.V >= 0x04000002) EmitFlags = r.ReadUInt16();
        StartRandom = r.ReadVector3();
        Emitter = X<NiObject>.Ptr(r);
        if (h.V >= 0x04000002) {
            UnknownShort2 = r.ReadUInt16();
            UnknownFloat13 = r.ReadSingle();
            UnknownInt1 = r.ReadUInt32();
            UnknownInt2 = r.ReadUInt32();
            UnknownShort3 = r.ReadUInt16();
        }
        if (h.V <= 0x03010000) {
            ParticleVelocity = r.ReadVector3();
            ParticleUnknownVector = r.ReadVector3();
            ParticleLifetime = r.ReadSingle();
            ParticleLink = X<NiObject>.Ref(r);
            ParticleTimestamp = r.ReadUInt32();
            ParticleUnknownShort = r.ReadUInt16();
            ParticleVertexId = r.ReadUInt16();
        }
        if (h.V >= 0x04000002) {
            NumParticles = r.ReadUInt16();
            NumValid = r.ReadUInt16();
            Particles = r.ReadFArray(r => new Particle(r), NumParticles);
            UnknownLink = X<NiObject>.Ref(r);
        }
        ParticleExtra = X<NiParticleModifier>.Ref(r);
        UnknownLink2 = X<NiObject>.Ref(r);
        if (h.V >= 0x04000002) Trailer = r.ReadByte();
        if (h.V <= 0x03010000) {
            ColorData = X<NiColorData>.Ref(r);
            UnknownFloat1 = r.ReadSingle();
            UnknownFloats2 = r.ReadPArray<float>("f", ParticleUnknownShort);
        }
    }
}

/// <summary>
/// A particle system controller, used by BS in conjunction with NiBSParticleNode.
/// </summary>
public class NiBSPArrayController(BinaryReader r, Header h) : NiParticleSystemController(r, h) { // X
}

/// <summary>
/// DEPRECATED (10.2), REMOVED (20.5). Replaced by NiTransformController and NiPathInterpolator.
/// Time controller for a path.
/// </summary>
public class NiPathController : NiTimeController {
    public PathFlags PathFlags;
    public int BankDir = 1;                             // -1 = Negative, 1 = Positive
    public float MaxBankAngle;                          // Max angle in radians.
    public float Smoothing;
    public short FollowAxis;                            // 0, 1, or 2 representing X, Y, or Z.
    public int? PathData;
    public int? PercentData;

    public NiPathController(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A010000) PathFlags = (PathFlags)r.ReadUInt16();
        BankDir = r.ReadInt32();
        MaxBankAngle = r.ReadSingle();
        Smoothing = r.ReadSingle();
        FollowAxis = r.ReadInt16();
        PathData = X<NiPosData>.Ref(r);
        PercentData = X<NiFloatData>.Ref(r);
    }
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

    public NiPixelFormat(BinaryReader r, Header h) : base(r, h) {
        PixelFormat = (PixelFormat)r.ReadUInt32();
        if (h.V <= 0x0A030002) {
            RedMask = r.ReadUInt32();
            GreenMask = r.ReadUInt32();
            BlueMask = r.ReadUInt32();
            AlphaMask = r.ReadUInt32();
            BitsPerPixel = r.ReadUInt32();
            OldFastCompare = r.ReadBytes(8);
        }
        if (h.V >= 0x0A010000 && h.V <= 0x0A030002) Tiling = (PixelTiling)r.ReadUInt32();
        if (h.V >= 0x0A030003) {
            BitsPerPixel = r.ReadByte();
            RendererHint = r.ReadUInt32();
            ExtraData = r.ReadUInt32();
            Flags = r.ReadByte();
            Tiling = (PixelTiling)r.ReadUInt32();
        }
        if (h.V >= 0x14030004) sRGBSpace = r.ReadBool32();
        if (h.V >= 0x0A030003) Channels = r.ReadFArray(r => new PixelFormatComponent(r), 4);
    }
}

public class NiPersistentSrcTextureRendererData : NiPixelFormat {
    public int? Palette;
    public uint NumMipmaps;
    public uint BytesPerPixel;
    public MipMap[] Mipmaps;
    public uint NumPixels;
    public uint PadNumPixels;
    public uint NumFaces;
    public PlatformID Platform;
    public RendererID Renderer;
    public byte[] PixelData;

    public NiPersistentSrcTextureRendererData(BinaryReader r, Header h) : base(r, h) {
        Palette = X<NiPalette>.Ref(r);
        NumMipmaps = r.ReadUInt32();
        BytesPerPixel = r.ReadUInt32();
        Mipmaps = r.ReadFArray(r => new MipMap(r), NumMipmaps);
        NumPixels = r.ReadUInt32();
        if (h.V >= 0x14020006) PadNumPixels = r.ReadUInt32();
        NumFaces = r.ReadUInt32();
        if (h.V <= 0x1E010000) Platform = (PlatformID)r.ReadUInt32();
        if (h.V >= 0x1E010001) Renderer = (RendererID)r.ReadUInt32();
        PixelData = r.ReadBytes(NumPixels * NumFaces);
    }
}

/// <summary>
/// A texture.
/// </summary>
public class NiPixelData : NiPixelFormat {
    public int? Palette;
    public uint NumMipmaps;
    public uint BytesPerPixel;
    public MipMap[] Mipmaps;
    public uint NumPixels;
    public uint NumFaces = 1;
    public byte[] PixelData;

    public NiPixelData(BinaryReader r, Header h) : base(r, h) {
        Palette = X<NiPalette>.Ref(r);
        NumMipmaps = r.ReadUInt32();
        BytesPerPixel = r.ReadUInt32();
        Mipmaps = r.ReadFArray(r => new MipMap(r), NumMipmaps);
        NumPixels = r.ReadUInt32();
        if (h.V >= 0x0A030006) NumFaces = r.ReadUInt32();
        if (h.V <= 0x0A030005) PixelData = r.ReadBytes(NumPixels);
        if (h.V >= 0x0A030006) PixelData = r.ReadBytes(NumPixels * NumFaces);
    }
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

    public NiPlanarCollider(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A000100) UnknownShort = r.ReadUInt16();
        UnknownFloat1 = r.ReadSingle();
        UnknownFloat2 = r.ReadSingle();
        if (h.V == 0x04020200) UnknownShort2 = r.ReadUInt16();
        UnknownFloat3 = r.ReadSingle();
        UnknownFloat4 = r.ReadSingle();
        UnknownFloat5 = r.ReadSingle();
        UnknownFloat6 = r.ReadSingle();
        UnknownFloat7 = r.ReadSingle();
        UnknownFloat8 = r.ReadSingle();
        UnknownFloat9 = r.ReadSingle();
        UnknownFloat10 = r.ReadSingle();
        UnknownFloat11 = r.ReadSingle();
        UnknownFloat12 = r.ReadSingle();
        UnknownFloat13 = r.ReadSingle();
        UnknownFloat14 = r.ReadSingle();
        UnknownFloat15 = r.ReadSingle();
        UnknownFloat16 = r.ReadSingle();
    }
}

/// <summary>
/// A point light.
/// </summary>
public class NiPointLight : NiLight {
    public float ConstantAttenuation;
    public float LinearAttenuation = 1.0f;
    public float QuadraticAttenuation;

    public NiPointLight(BinaryReader r, Header h) : base(r, h) {
        ConstantAttenuation = r.ReadSingle();
        LinearAttenuation = r.ReadSingle();
        QuadraticAttenuation = r.ReadSingle();
    }
}

/// <summary>
/// Abstract base class for dynamic effects such as NiLights or projected texture effects.
/// </summary>
public abstract class NiDeferredDynamicEffect : NiAVObject {
    public bool SwitchState = 1;                        // If true, then the dynamic effect is applied to affected nodes during rendering.

    public NiDeferredDynamicEffect(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x0A01006A && h.UV2 < 130) SwitchState = r.ReadBool32();
    }
}

/// <summary>
/// Abstract base class that represents light sources in a scene graph.
/// For Bethesda Stream 130 (FO4), NiLight now directly inherits from NiAVObject.
/// </summary>
public abstract class NiDeferredLight : NiDeferredDynamicEffect {
    public float Dimmer = 1.0f;                         // Scales the overall brightness of all light components.
    public Color3 AmbientColor = 0.0, 0.0, 0.0;
    public Color3 DiffuseColor = 0.0, 0.0, 0.0;
    public Color3 SpecularColor = 0.0, 0.0, 0.0;

    public NiDeferredLight(BinaryReader r, Header h) : base(r, h) {
        Dimmer = r.ReadSingle();
        AmbientColor = new Color3(r);
        DiffuseColor = new Color3(r);
        SpecularColor = new Color3(r);
    }
}

/// <summary>
/// A deferred point light. Custom (Twin Saga).
/// </summary>
public class NiDeferredPointLight : NiDeferredLight {
    public float ConstantAttenuation;
    public float LinearAttenuation = 1.0f;
    public float QuadraticAttenuation;

    public NiDeferredPointLight(BinaryReader r, Header h) : base(r, h) {
        ConstantAttenuation = r.ReadSingle();
        LinearAttenuation = r.ReadSingle();
        QuadraticAttenuation = r.ReadSingle();
    }
}

/// <summary>
/// Wrapper for position animation keys.
/// </summary>
public class NiPosData : NiObject { // X
    public KeyGroup<Vector3> Data;

    public NiPosData(BinaryReader r, Header h) : base(r, h) {
        Data = new KeyGroup<Vector3>(r);
    }
}

/// <summary>
/// Wrapper for rotation animation keys.
/// </summary>
public class NiRotData : NiObject {
    public uint NumRotationKeys;
    public KeyType RotationType;
    public QuatKey<Quaternion>[] QuaternionKeys;
    public KeyGroup<float>[] XYZRotations;

    public NiRotData(BinaryReader r, Header h) : base(r, h) {
        NumRotationKeys = r.ReadUInt32();
        if (NumRotationKeys != 0) RotationType = (KeyType)r.ReadUInt32();
        if (RotationType != 4) QuaternionKeys = r.ReadFArray(r => new QuatKey<Quaternion>(r, h), NumRotationKeys);
        if (RotationType == 4) XYZRotations = r.ReadFArray(r => new KeyGroup<float>(r), 3);
    }
}

/// <summary>
/// Particle modifier that controls and updates the age of particles in the system.
/// </summary>
public class NiPSysAgeDeathModifier : NiPSysModifier {
    public bool SpawnonDeath;                           // Should the particles spawn on death?
    public int? SpawnModifier;                          // The spawner to use on death.

    public NiPSysAgeDeathModifier(BinaryReader r, Header h) : base(r, h) {
        SpawnonDeath = r.ReadBool32();
        SpawnModifier = X<NiPSysSpawnModifier>.Ref(r);
    }
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

    public NiPSysBombModifier(BinaryReader r, Header h) : base(r, h) {
        BombObject = X<NiNode>.Ptr(r);
        BombAxis = r.ReadVector3();
        Decay = r.ReadSingle();
        DeltaV = r.ReadSingle();
        DecayType = (DecayType)r.ReadUInt32();
        SymmetryType = (SymmetryType)r.ReadUInt32();
    }
}

/// <summary>
/// Particle modifier that creates and updates bound volumes.
/// </summary>
public class NiPSysBoundUpdateModifier : NiPSysModifier {
    public ushort UpdateSkip;                           // Optimize by only computing the bound of (1 / Update Skip) of the total particles each frame.

    public NiPSysBoundUpdateModifier(BinaryReader r, Header h) : base(r, h) {
        UpdateSkip = r.ReadUInt16();
    }
}

/// <summary>
/// Particle emitter that uses points within a defined Box shape to emit from.
/// </summary>
public class NiPSysBoxEmitter : NiPSysVolumeEmitter {
    public float Width;
    public float Height;
    public float Depth;

    public NiPSysBoxEmitter(BinaryReader r, Header h) : base(r, h) {
        Width = r.ReadSingle();
        Height = r.ReadSingle();
        Depth = r.ReadSingle();
    }
}

/// <summary>
/// Particle modifier that adds a defined shape to act as a collision object for particles to interact with.
/// </summary>
public class NiPSysColliderManager : NiPSysModifier {
    public int? Collider;

    public NiPSysColliderManager(BinaryReader r, Header h) : base(r, h) {
        Collider = X<NiPSysCollider>.Ref(r);
    }
}

/// <summary>
/// Particle modifier that adds keyframe data to modify color/alpha values of particles over time.
/// </summary>
public class NiPSysColorModifier : NiPSysModifier {
    public int? Data;

    public NiPSysColorModifier(BinaryReader r, Header h) : base(r, h) {
        Data = X<NiColorData>.Ref(r);
    }
}

/// <summary>
/// Particle emitter that uses points within a defined Cylinder shape to emit from.
/// </summary>
public class NiPSysCylinderEmitter : NiPSysVolumeEmitter {
    public float Radius;
    public float Height;

    public NiPSysCylinderEmitter(BinaryReader r, Header h) : base(r, h) {
        Radius = r.ReadSingle();
        Height = r.ReadSingle();
    }
}

/// <summary>
/// Particle modifier that applies a linear drag force to particles.
/// </summary>
public class NiPSysDragModifier : NiPSysModifier {
    public int? DragObject;                             // The object whose position and orientation are the basis of the force.
    public Vector3 DragAxis = new Vector3(1.0, 0.0, 0.0); // The local direction of the force.
    public float Percentage = 0.05f;                    // The amount of drag to apply to particles.
    public float Range = 3.402823466e+38f;              // The distance up to which particles are fully affected.
    public float RangeFalloff = 3.402823466e+38f;       // The distance at which particles cease to be affected.

    public NiPSysDragModifier(BinaryReader r, Header h) : base(r, h) {
        DragObject = X<NiAVObject>.Ptr(r);
        DragAxis = r.ReadVector3();
        Percentage = r.ReadSingle();
        Range = r.ReadSingle();
        RangeFalloff = r.ReadSingle();
    }
}

/// <summary>
/// DEPRECATED (10.2). Particle system emitter controller data.
/// </summary>
public class NiPSysEmitterCtlrData : NiObject {
    public KeyGroup<float> BirthRateKeys;
    public Key<byte>[] ActiveKeys;

    public NiPSysEmitterCtlrData(BinaryReader r, Header h) : base(r, h) {
        BirthRateKeys = new KeyGroup<float>(r);
        ActiveKeys = r.ReadL32FArray(r => new Key<byte>(r, Interpolation));
    }
}

/// <summary>
/// Particle modifier that applies a gravitational force to particles.
/// </summary>
public class NiPSysGravityModifier : NiPSysModifier {
    public int? GravityObject;                          // The object whose position and orientation are the basis of the force.
    public Vector3 GravityAxis = new Vector3(1.0, 0.0, 0.0); // The local direction of the force.
    public float Decay;                                 // How the force diminishes by distance.
    public float Strength = 1.0f;                       // The acceleration of the force.
    public ForceType ForceType;                         // The type of gravitational force.
    public float Turbulence;                            // Adds a degree of randomness.
    public float TurbulenceScale = 1.0f;                // Scale for turbulence.
    public bool WorldAligned;

    public NiPSysGravityModifier(BinaryReader r, Header h) : base(r, h) {
        GravityObject = X<NiAVObject>.Ptr(r);
        GravityAxis = r.ReadVector3();
        Decay = r.ReadSingle();
        Strength = r.ReadSingle();
        ForceType = (ForceType)r.ReadUInt32();
        Turbulence = r.ReadSingle();
        TurbulenceScale = r.ReadSingle();
        if (h.UV2 > 16) WorldAligned = r.ReadBool32();
    }
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

    public NiPSysGrowFadeModifier(BinaryReader r, Header h) : base(r, h) {
        GrowTime = r.ReadSingle();
        GrowGeneration = r.ReadUInt16();
        FadeTime = r.ReadSingle();
        FadeGeneration = r.ReadUInt16();
        if (h.UV2 >= 34) BaseScale = r.ReadSingle();
    }
}

/// <summary>
/// Particle emitter that uses points on a specified mesh to emit from.
/// </summary>
public class NiPSysMeshEmitter : NiPSysEmitter {
    public int?[] EmitterMeshes;                        // The meshes which are emitted from.
    public VelocityType InitialVelocityType;            // The method by which the initial particle velocity will be computed.
    public EmitFrom EmissionType;                       // The manner in which particles are emitted from the Emitter Meshes.
    public Vector3 EmissionAxis = new Vector3(1.0, 0.0, 0.0); // The emission axis if VELOCITY_USE_DIRECTION.

    public NiPSysMeshEmitter(BinaryReader r, Header h) : base(r, h) {
        EmitterMeshes = r.ReadL32FArray(X<NiAVObject>.Ptr);
        InitialVelocityType = (VelocityType)r.ReadUInt32();
        EmissionType = (EmitFrom)r.ReadUInt32();
        EmissionAxis = r.ReadVector3();
    }
}

/// <summary>
/// Particle modifier that updates mesh particles using the age of each particle.
/// </summary>
public class NiPSysMeshUpdateModifier : NiPSysModifier {
    public int?[] Meshes;

    public NiPSysMeshUpdateModifier(BinaryReader r, Header h) : base(r, h) {
        Meshes = r.ReadL32FArray(X<NiAVObject>.Ref);
    }
}

public class BSPSysInheritVelocityModifier : NiPSysModifier {
    public int? Target;
    public float ChanceToInherit;
    public float VelocityMultiplier;
    public float VelocityVariation;

    public BSPSysInheritVelocityModifier(BinaryReader r, Header h) : base(r, h) {
        Target = X<NiNode>.Ptr(r);
        ChanceToInherit = r.ReadSingle();
        VelocityMultiplier = r.ReadSingle();
        VelocityVariation = r.ReadSingle();
    }
}

public class BSPSysHavokUpdateModifier : NiPSysModifier {
    public int?[] Nodes;
    public int? Modifier;

    public BSPSysHavokUpdateModifier(BinaryReader r, Header h) : base(r, h) {
        Nodes = r.ReadL32FArray(X<NiNode>.Ref);
        Modifier = X<NiPSysModifier>.Ref(r);
    }
}

public class BSPSysRecycleBoundModifier : NiPSysModifier {
    public Vector3 BoundOffset;
    public Vector3 BoundExtent;
    public int? Target;

    public BSPSysRecycleBoundModifier(BinaryReader r, Header h) : base(r, h) {
        BoundOffset = r.ReadVector3();
        BoundExtent = r.ReadVector3();
        Target = X<NiNode>.Ptr(r);
    }
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

    public BSPSysSubTexModifier(BinaryReader r, Header h) : base(r, h) {
        StartFrame = r.ReadUInt32();
        StartFrameFudge = r.ReadSingle();
        EndFrame = r.ReadSingle();
        LoopStartFrame = r.ReadSingle();
        LoopStartFrameFudge = r.ReadSingle();
        FrameCount = r.ReadSingle();
        FrameCountFudge = r.ReadSingle();
    }
}

/// <summary>
/// Particle Collider object which particles will interact with.
/// </summary>
public class NiPSysPlanarCollider : NiPSysCollider {
    public float Width;                                 // Width of the plane along the X Axis.
    public float Height;                                // Height of the plane along the Y Axis.
    public Vector3 XAxis;                               // Axis defining a plane, relative to Collider Object.
    public Vector3 YAxis;                               // Axis defining a plane, relative to Collider Object.

    public NiPSysPlanarCollider(BinaryReader r, Header h) : base(r, h) {
        Width = r.ReadSingle();
        Height = r.ReadSingle();
        XAxis = r.ReadVector3();
        YAxis = r.ReadVector3();
    }
}

/// <summary>
/// Particle Collider object which particles will interact with.
/// </summary>
public class NiPSysSphericalCollider : NiPSysCollider {
    public float Radius;

    public NiPSysSphericalCollider(BinaryReader r, Header h) : base(r, h) {
        Radius = r.ReadSingle();
    }
}

/// <summary>
/// Particle modifier that updates the particle positions based on velocity and last update time.
/// </summary>
public class NiPSysPositionModifier(BinaryReader r, Header h) : NiPSysModifier(r, h) {
}

/// <summary>
/// Particle modifier that calls reset on a target upon looping.
/// </summary>
public class NiPSysResetOnLoopCtlr(BinaryReader r, Header h) : NiTimeController(r, h) {
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
    public bool RandomAxis = 1;                         // Assign a random axis to new particles?
    public Vector3 Axis = new Vector3(1.0, 0.0, 0.0);   // Initial rotation axis.

    public NiPSysRotationModifier(BinaryReader r, Header h) : base(r, h) {
        RotationSpeed = r.ReadSingle();
        if (h.V >= 0x14000002) {
            RotationSpeedVariation = r.ReadSingle();
            RotationAngle = r.ReadSingle();
            RotationAngleVariation = r.ReadSingle();
            RandomRotSpeedSign = r.ReadBool32();
        }
        RandomAxis = r.ReadBool32();
        Axis = r.ReadVector3();
    }
}

/// <summary>
/// Particle modifier that spawns additional copies of a particle.
/// </summary>
public class NiPSysSpawnModifier : NiPSysModifier {
    public ushort NumSpawnGenerations = 0;              // Number of allowed generations for spawning. Particles whose generations are >= will not be spawned.
    public float PercentageSpawned = 1.0f;              // The likelihood of a particular particle being spawned. Must be between 0.0 and 1.0.
    public ushort MinNumtoSpawn = 1;                    // The minimum particles to spawn for any given original particle.
    public ushort MaxNumtoSpawn = 1;                    // The maximum particles to spawn for any given original particle.
    public int UnknownInt;                              // WorldShift
    public float SpawnSpeedVariation;                   // How much the spawned particle speed can vary.
    public float SpawnDirVariation;                     // How much the spawned particle direction can vary.
    public float LifeSpan;                              // Lifespan assigned to spawned particles.
    public float LifeSpanVariation;                     // The amount the lifespan can vary.

    public NiPSysSpawnModifier(BinaryReader r, Header h) : base(r, h) {
        NumSpawnGenerations = r.ReadUInt16();
        PercentageSpawned = r.ReadSingle();
        MinNumtoSpawn = r.ReadUInt16();
        MaxNumtoSpawn = r.ReadUInt16();
        if (h.V == 0x0A040001) UnknownInt = r.ReadInt32();
        SpawnSpeedVariation = r.ReadSingle();
        SpawnDirVariation = r.ReadSingle();
        LifeSpan = r.ReadSingle();
        LifeSpanVariation = r.ReadSingle();
    }
}

/// <summary>
/// Particle emitter that uses points within a sphere shape to emit from.
/// </summary>
public class NiPSysSphereEmitter : NiPSysVolumeEmitter {
    public float Radius;

    public NiPSysSphereEmitter(BinaryReader r, Header h) : base(r, h) {
        Radius = r.ReadSingle();
    }
}

/// <summary>
/// Particle system controller, tells the system to update its simulation.
/// </summary>
public class NiPSysUpdateCtlr(BinaryReader r, Header h) : NiTimeController(r, h) {
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

    public NiPSysFieldModifier(BinaryReader r, Header h) : base(r, h) {
        FieldObject = X<NiAVObject>.Ref(r);
        Magnitude = r.ReadSingle();
        Attenuation = r.ReadSingle();
        UseMaxDistance = r.ReadBool32();
        MaxDistance = r.ReadSingle();
    }
}

/// <summary>
/// Particle system modifier, implements a vortex field force for particles.
/// </summary>
public class NiPSysVortexFieldModifier : NiPSysFieldModifier {
    public Vector3 Direction;                           // Direction of the vortex field in Field Object's space.

    public NiPSysVortexFieldModifier(BinaryReader r, Header h) : base(r, h) {
        Direction = r.ReadVector3();
    }
}

/// <summary>
/// Particle system modifier, implements a gravity field force for particles.
/// </summary>
public class NiPSysGravityFieldModifier : NiPSysFieldModifier {
    public Vector3 Direction = new Vector3(0.0, -1.0, 0.0); // Direction of the gravity field in Field Object's space.

    public NiPSysGravityFieldModifier(BinaryReader r, Header h) : base(r, h) {
        Direction = r.ReadVector3();
    }
}

/// <summary>
/// Particle system modifier, implements a drag field force for particles.
/// </summary>
public class NiPSysDragFieldModifier : NiPSysFieldModifier {
    public bool UseDirection;                           // Whether or not the drag force applies only in the direction specified.
    public Vector3 Direction;                           // Direction in which the force applies if Use Direction is true.

    public NiPSysDragFieldModifier(BinaryReader r, Header h) : base(r, h) {
        UseDirection = r.ReadBool32();
        Direction = r.ReadVector3();
    }
}

/// <summary>
/// Particle system modifier, implements a turbulence field force for particles.
/// </summary>
public class NiPSysTurbulenceFieldModifier : NiPSysFieldModifier {
    public float Frequency;                             // How many turbulence updates per second.

    public NiPSysTurbulenceFieldModifier(BinaryReader r, Header h) : base(r, h) {
        Frequency = r.ReadSingle();
    }
}

public class BSPSysLODModifier : NiPSysModifier {
    public float LODBeginDistance = 0.1f;
    public float LODEndDistance = 0.7f;
    public float EndEmitScale = 0.2f;
    public float EndSize = 1.0f;

    public BSPSysLODModifier(BinaryReader r, Header h) : base(r, h) {
        LODBeginDistance = r.ReadSingle();
        LODEndDistance = r.ReadSingle();
        EndEmitScale = r.ReadSingle();
        EndSize = r.ReadSingle();
    }
}

public class BSPSysScaleModifier : NiPSysModifier {
    public float[] Scales;

    public BSPSysScaleModifier(BinaryReader r, Header h) : base(r, h) {
        Scales = r.ReadL32PArray<float>("f");
    }
}

/// <summary>
/// Particle system controller for force field magnitude.
/// </summary>
public class NiPSysFieldMagnitudeCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Particle system controller for force field attenuation.
/// </summary>
public class NiPSysFieldAttenuationCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Particle system controller for force field maximum distance.
/// </summary>
public class NiPSysFieldMaxDistanceCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Particle system controller for air field air friction.
/// </summary>
public class NiPSysAirFieldAirFrictionCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Particle system controller for air field inherit velocity.
/// </summary>
public class NiPSysAirFieldInheritVelocityCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Particle system controller for air field spread.
/// </summary>
public class NiPSysAirFieldSpreadCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Particle system controller for emitter initial rotation speed.
/// </summary>
public class NiPSysInitialRotSpeedCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Particle system controller for emitter initial rotation speed variation.
/// </summary>
public class NiPSysInitialRotSpeedVarCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Particle system controller for emitter initial rotation angle.
/// </summary>
public class NiPSysInitialRotAngleCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Particle system controller for emitter initial rotation angle variation.
/// </summary>
public class NiPSysInitialRotAngleVarCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Particle system controller for emitter planar angle.
/// </summary>
public class NiPSysEmitterPlanarAngleCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Particle system controller for emitter planar angle variation.
/// </summary>
public class NiPSysEmitterPlanarAngleVarCtlr(BinaryReader r, Header h) : NiPSysModifierFloatCtlr(r, h) {
}

/// <summary>
/// Particle system modifier, updates the particle velocity to simulate the effects of air movements like wind, fans, or wake.
/// </summary>
public class NiPSysAirFieldModifier : NiPSysFieldModifier {
    public Vector3 Direction = new Vector3(-1.0, 0.0, 0.0); // Direction of the particle velocity
    public float AirFriction;                           // How quickly particles will accelerate to the magnitude of the air field.
    public float InheritVelocity;                       // How much of the air field velocity will be added to the particle velocity.
    public bool InheritRotation;
    public bool ComponentOnly;
    public bool EnableSpread;
    public float Spread;                                // The angle of the air field cone if Enable Spread is true.

    public NiPSysAirFieldModifier(BinaryReader r, Header h) : base(r, h) {
        Direction = r.ReadVector3();
        AirFriction = r.ReadSingle();
        InheritVelocity = r.ReadSingle();
        InheritRotation = r.ReadBool32();
        ComponentOnly = r.ReadBool32();
        EnableSpread = r.ReadBool32();
        Spread = r.ReadSingle();
    }
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

    public NiPSysTrailEmitter(BinaryReader r, Header h) : base(r, h) {
        UnknownInt1 = r.ReadInt32();
        UnknownFloat1 = r.ReadSingle();
        UnknownFloat2 = r.ReadSingle();
        UnknownFloat3 = r.ReadSingle();
        UnknownInt2 = r.ReadInt32();
        UnknownFloat4 = r.ReadSingle();
        UnknownInt3 = r.ReadInt32();
        UnknownFloat5 = r.ReadSingle();
        UnknownInt4 = r.ReadInt32();
        UnknownFloat6 = r.ReadSingle();
        UnknownFloat7 = r.ReadSingle();
    }
}

/// <summary>
/// Unknown controller
/// </summary>
public class NiLightIntensityController(BinaryReader r, Header h) : NiFloatInterpController(r, h) {
}

/// <summary>
/// Particle system modifier, updates the particle velocity to simulate the effects of point gravity.
/// </summary>
public class NiPSysRadialFieldModifier : NiPSysFieldModifier {
    public float RadialType;                            // If zero, no attenuation.

    public NiPSysRadialFieldModifier(BinaryReader r, Header h) : base(r, h) {
        RadialType = r.ReadSingle();
    }
}

/// <summary>
/// Abstract class used for different types of LOD selections.
/// </summary>
public abstract class NiLODData(BinaryReader r, Header h) : NiObject(r, h) {
}

/// <summary>
/// NiRangeLODData controls switching LOD levels based on Z depth from the camera to the NiLODNode.
/// </summary>
public class NiRangeLODData : NiLODData {
    public Vector3 LODCenter;
    public LODRange[] LODLevels;

    public NiRangeLODData(BinaryReader r, Header h) : base(r, h) {
        LODCenter = r.ReadVector3();
        LODLevels = r.ReadL32FArray(r => new LODRange(r, h));
    }
}

/// <summary>
/// NiScreenLODData controls switching LOD levels based on proportion of the screen that a bound would include.
/// </summary>
public class NiScreenLODData : NiLODData {
    public NiBound Bound;
    public NiBound WorldBound;
    public float[] ProportionLevels;

    public NiScreenLODData(BinaryReader r, Header h) : base(r, h) {
        Bound = new NiBound(r);
        WorldBound = new NiBound(r);
        ProportionLevels = r.ReadL32PArray<float>("f");
    }
}

/// <summary>
/// Unknown.
/// </summary>
public class NiRotatingParticles(BinaryReader r, Header h) : NiParticles(r, h) { // X
}

/// <summary>
/// DEPRECATED (pre-10.1), REMOVED (20.3).
/// Keyframe animation root node, in .kf files.
/// </summary>
public class NiSequenceStreamHelper(BinaryReader r, Header h) : NiObjectNET(r, h) {
}

/// <summary>
/// Determines whether flat shading or smooth shading is used on a shape.
/// </summary>
public class NiShadeProperty : NiProperty { // X
    public Flags Flags = 1;                             // Bit 0: Enable smooth phong shading on this shape. Otherwise, hard-edged flat shading will be used on this shape.

    public NiShadeProperty(BinaryReader r, Header h) : base(r, h) {
        if ((h.UV2 <= 34)) Flags = (Flags)r.ReadUInt16();
    }
}

/// <summary>
/// Skinning data.
/// </summary>
public class NiSkinData : NiObject { // X
    public NiTransform SkinTransform;                   // Offset of the skin from this bone in bind position.
    public uint NumBones;                               // Number of bones.
    public int? SkinPartition;                          // This optionally links a NiSkinPartition for hardware-acceleration information.
    public byte HasVertexWeights = 1;                   // Enables Vertex Weights for this NiSkinData.
    public BoneData[] BoneList;                         // Contains offset data for each node that this skin is influenced by.

    public NiSkinData(BinaryReader r, Header h) : base(r, h) {
        SkinTransform = new NiTransform(r);
        NumBones = r.ReadUInt32();
        if (h.V >= 0x04000002 && h.V <= 0x0A010000) SkinPartition = X<NiSkinPartition>.Ref(r);
        if (h.V >= 0x04020100) HasVertexWeights = r.ReadByte();
        BoneList = r.ReadFArray(r => new BoneData(r, h), NumBones);
    }
}

/// <summary>
/// Skinning instance.
/// </summary>
public class NiSkinInstance : NiObject { // X
    public int? Data;                                   // Skinning data reference.
    public int? SkinPartition;                          // Refers to a NiSkinPartition objects, which partitions the mesh such that every vertex is only influenced by a limited number of bones.
    public int? SkeletonRoot;                           // Armature root node.
    public int?[] Bones;                                // List of all armature bones.

    public NiSkinInstance(BinaryReader r, Header h) : base(r, h) {
        Data = X<NiSkinData>.Ref(r);
        if (h.V >= 0x0A010065) SkinPartition = X<NiSkinPartition>.Ref(r);
        SkeletonRoot = X<NiNode>.Ptr(r);
        Bones = r.ReadL32FArray(X<NiNode>.Ptr);
    }
}

/// <summary>
/// Old version of skinning instance.
/// </summary>
public class NiTriShapeSkinController : NiTimeController {
    public uint NumBones;                               // The number of node bones referenced as influences.
    public uint[] VertexCounts;                         // The number of vertex weights stored for each bone.
    public int?[] Bones;                                // List of all armature bones.
    public OldSkinData[][] BoneData;                    // Contains skin weight data for each node that this skin is influenced by.

    public NiTriShapeSkinController(BinaryReader r, Header h) : base(r, h) {
        NumBones = r.ReadUInt32();
        VertexCounts = r.ReadPArray<uint>("I", NumBones);
        Bones = r.ReadFArray(X<NiBone>.Ptr, NumBones);
        BoneData = r.ReadFArray(k => r.ReadFArray(r => new OldSkinData(r), NumBones), VertexCounts);
    }
}

/// <summary>
/// A copy of NISkinInstance for use with NiClod meshes.
/// </summary>
public class NiClodSkinInstance(BinaryReader r, Header h) : NiSkinInstance(r, h) {
}

/// <summary>
/// Skinning data, optimized for hardware skinning. The mesh is partitioned in submeshes such that each vertex of a submesh is influenced only by a limited and fixed number of bones.
/// </summary>
public class NiSkinPartition : NiObject { // X
    public uint NumSkinPartitionBlocks;
    public SkinPartition[] SkinPartitionBlocks;         // Skin partition objects.
    public uint DataSize;
    public uint VertexSize;
    public BSVertexDesc VertexDesc;
    public BSVertexData[] VertexData;
    public SkinPartition[] Partition;

    public NiSkinPartition(BinaryReader r, Header h) : base(r, h) {
        NumSkinPartitionBlocks = r.ReadUInt32();
        if (!((h.V == 0x14020007) && (h.UV2 == 100))) SkinPartitionBlocks = r.ReadFArray(r => new SkinPartition(r, h), NumSkinPartitionBlocks);
        if ((h.UV2 == 100)) {
            DataSize = r.ReadUInt32();
            VertexSize = r.ReadUInt32();
            VertexDesc = new BSVertexDesc(r);
            if (DataSize > 0) VertexData = r.ReadFArray(r => new BSVertexData(r, true), DataSize/VertexSize);
            Partition = r.ReadFArray(r => new SkinPartition(r, h), NumSkinPartitionBlocks);
        }
    }
}

/// <summary>
/// A texture.
/// </summary>
public abstract class NiTexture(BinaryReader r, Header h) : NiObjectNET(r, h) { // X
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
public class NiSourceTexture : NiTexture { // X
    public byte UseExternal = 1;                        // Is the texture external?
    public ?? FileName;                                 // The original source filename of the image embedded by the referred NiPixelData object.
    public int? UnknownLink;                            // Unknown.
    public byte UnknownByte = 1;                        // Unknown. Seems to be set if Pixel Data is present?
    public int? PixelData;                              // NiPixelData or NiPersistentSrcTextureRendererData
    public FormatPrefs FormatPrefs;                     // A set of preferences for the texture format. They are a request only and the renderer may ignore them.
    public byte IsStatic = 1;                           // If set, then the application cannot assume that any dynamic changes to the pixel data will show in the rendered image.
    public bool DirectRender = 1;                       // A hint to the renderer that the texture can be loaded directly from a texture file into a renderer-specific resource, bypassing the NiPixelData object.
    public bool PersistRenderData = 0;                  // Pixel Data is NiPersistentSrcTextureRendererData instead of NiPixelData.

    public NiSourceTexture(BinaryReader r, Header h) : base(r, h) {
        UseExternal = r.ReadByte();
        if (UseExternal == 1 && h.V >= 0x0A010000) {
            FileName = ??;
            UnknownLink = X<NiObject>.Ref(r);
        }
        if (UseExternal == 0) {
            if (h.V <= 0x0A000100) UnknownByte = r.ReadByte();
            if (h.V >= 0x0A010000) FileName = ??;
            PixelData = X<NiPixelFormat>.Ref(r);
        }
        FormatPrefs = new FormatPrefs(r);
        IsStatic = r.ReadByte();
        if (h.V >= 0x0A010067) DirectRender = r.ReadBool32();
        if (h.V >= 0x14020004) PersistRenderData = r.ReadBool32();
    }
}

/// <summary>
/// Gives specularity to a shape. Flags 0x0001.
/// </summary>
public class NiSpecularProperty : NiProperty {
    public Flags Flags;                                 // Bit 0 = Enable specular lighting on this shape.

    public NiSpecularProperty(BinaryReader r, Header h) : base(r, h) {
        Flags = (Flags)r.ReadUInt16();
    }
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

    public NiSphericalCollider(BinaryReader r, Header h) : base(r, h) {
        UnknownFloat1 = r.ReadSingle();
        UnknownShort1 = r.ReadUInt16();
        UnknownFloat2 = r.ReadSingle();
        if (h.V <= 0x04020002) UnknownShort2 = r.ReadUInt16();
        if (h.V >= 0x04020100) UnknownFloat3 = r.ReadSingle();
        UnknownFloat4 = r.ReadSingle();
        UnknownFloat5 = r.ReadSingle();
    }
}

/// <summary>
/// A spot.
/// </summary>
public class NiSpotLight : NiPointLight {
    public float OuterSpotAngle;
    public float InnerSpotAngle;
    public float Exponent = 1.0f;                       // Describes the distribution of light. (see: glLight)

    public NiSpotLight(BinaryReader r, Header h) : base(r, h) {
        OuterSpotAngle = r.ReadSingle();
        if (h.V >= 0x14020005) InnerSpotAngle = r.ReadSingle();
        Exponent = r.ReadSingle();
    }
}

/// <summary>
/// Allows control of stencil testing.
/// </summary>
public class NiStencilProperty : NiProperty {
    public Flags Flags = 19840;                         // Property flags:
                                                        //     Bit 0: Stencil Enable
                                                        //     Bits 1-3: Fail Action
                                                        //     Bits 4-6: Z Fail Action
                                                        //     Bits 7-9: Pass Action
                                                        //     Bits 10-11: Draw Mode
                                                        //     Bits 12-14: Stencil Function
    public byte StencilEnabled;                         // Enables or disables the stencil test.
    public StencilCompareMode StencilFunction;          // Selects the compare mode function (see: glStencilFunc).
    public uint StencilRef;
    public uint StencilMask = 4294967295;               // A bit mask. The default is 0xffffffff.
    public StencilAction FailAction;
    public StencilAction ZFailAction;
    public StencilAction PassAction;
    public StencilDrawMode DrawMode = DRAW_BOTH;        // Used to enabled double sided faces. Default is 3 (DRAW_BOTH).

    public NiStencilProperty(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x0A000102) Flags = (Flags)r.ReadUInt16();
        if (h.V <= 0x14000005) {
            StencilEnabled = r.ReadByte();
            StencilFunction = (StencilCompareMode)r.ReadUInt32();
            StencilRef = r.ReadUInt32();
            StencilMask = r.ReadUInt32();
            FailAction = (StencilAction)r.ReadUInt32();
            ZFailAction = (StencilAction)r.ReadUInt32();
            PassAction = (StencilAction)r.ReadUInt32();
            DrawMode = (StencilDrawMode)r.ReadUInt32();
        }
        if (h.V >= 0x14010003) {
            Flags = (Flags)r.ReadUInt16();
            StencilRef = r.ReadUInt32();
            StencilMask = r.ReadUInt32();
        }
    }
}

/// <summary>
/// Apparently commands for an optimizer instructing it to keep things it would normally discard.
/// Also refers to NiNode objects (through their name) in animation .kf files.
/// </summary>
public class NiStringExtraData : NiExtraData { // X
    public uint BytesRemaining;                         // The number of bytes left in the record.  Equals the length of the following string + 4.
    public string StringData;                           // The string.

    public NiStringExtraData(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x04020200) BytesRemaining = r.ReadUInt32();
        StringData = Y.String(r);
    }
}

/// <summary>
/// List of 0x00-seperated strings, which are names of controlled objects and controller types. Used in .kf files in conjunction with NiControllerSequence.
/// </summary>
public class NiStringPalette : NiObject {
    public StringPalette Palette;                       // A bunch of 0x00 seperated strings.

    public NiStringPalette(BinaryReader r, Header h) : base(r, h) {
        Palette = new StringPalette(r);
    }
}

/// <summary>
/// List of strings; for example, a list of all bone names.
/// </summary>
public class NiStringsExtraData : NiExtraData {
    public string[] Data;                               // The strings.

    public NiStringsExtraData(BinaryReader r, Header h) : base(r, h) {
        Data = r.ReadL32FArray(r => r.ReadL32L32AString());
    }
}

/// <summary>
/// Extra data, used to name different animation sequences.
/// </summary>
public class NiTextKeyExtraData : NiExtraData { // X
    public uint UnknownInt1;                            // Unknown.  Always equals zero in all official files.
    public Key<string>[] TextKeys;                      // List of textual notes and at which time they take effect. Used for designating the start and stop of animations and the triggering of sounds.

    public NiTextKeyExtraData(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x04020200) UnknownInt1 = r.ReadUInt32();
        TextKeys = r.ReadL32FArray(r => new Key<string>(r, Interpolation));
    }
}

/// <summary>
/// Represents an effect that uses projected textures such as projected lights (gobos), environment maps, and fog maps.
/// </summary>
public class NiTextureEffect : NiDynamicEffect { // X
    public Matrix3x3 ModelProjectionMatrix;             // Model projection matrix.  Always identity?
    public Vector3 ModelProjectionTransform;            // Model projection transform.  Always (0,0,0)?
    public TexFilterMode TextureFiltering = FILTER_TRILERP; // Texture Filtering mode.
    public ushort MaxAnisotropy;
    public TexClampMode TextureClamping = WRAP_S_WRAP_T;// Texture Clamp mode.
    public TextureType TextureType = TEX_ENVIRONMENT_MAP; // The type of effect that the texture is used for.
    public CoordGenType CoordinateGenerationType = CG_SPHERE_MAP; // The method that will be used to generate UV coordinates for the texture effect.
    public int? Image;                                  // Image index.
    public int? SourceTexture;                          // Source texture index.
    public byte EnablePlane = 0;                        // Determines whether a clipping plane is used.
    public NiPlane Plane;
    public short PS2L = 0;
    public short PS2K = -75;
    public ushort UnknownShort;                         // Unknown: 0.

    public NiTextureEffect(BinaryReader r, Header h) : base(r, h) {
        ModelProjectionMatrix = r.ReadMatrix3x3();
        ModelProjectionTransform = r.ReadVector3();
        TextureFiltering = (TexFilterMode)r.ReadUInt32();
        if (h.V >= 0x14050004) MaxAnisotropy = r.ReadUInt16();
        TextureClamping = (TexClampMode)r.ReadUInt32();
        TextureType = (TextureType)r.ReadUInt32();
        CoordinateGenerationType = (CoordGenType)r.ReadUInt32();
        if (h.V <= 0x03010000) Image = X<NiImage>.Ref(r);
        if (h.V >= 0x04000000) SourceTexture = X<NiSourceTexture>.Ref(r);
        EnablePlane = r.ReadByte();
        Plane = new NiPlane(r);
        if (h.V <= 0x0A020000) {
            PS2L = r.ReadInt16();
            PS2K = r.ReadInt16();
        }
        if (h.V <= 0x0401000C) UnknownShort = r.ReadUInt16();
    }
}

/// <summary>
/// LEGACY (pre-10.1)
/// </summary>
public class NiTextureModeProperty : NiProperty {
    public uint[] UnknownInts;
    public short UnknownShort;                          // Unknown. Either 210 or 194.
    public short PS2L = 0;                              // 0?
    public short PS2K = -75;                            // -75?

    public NiTextureModeProperty(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x02030000) UnknownInts = r.ReadPArray<uint>("I", 3);
        if (h.V >= 0x03000000) UnknownShort = r.ReadInt16();
        if (h.V >= 0x03010000 && h.V <= 0x0A020000) {
            PS2L = r.ReadInt16();
            PS2K = r.ReadInt16();
        }
    }
}

/// <summary>
/// LEGACY (pre-10.1)
/// </summary>
public class NiImage : NiObject {
    public byte UseExternal;                            // 0 if the texture is internal to the NIF file.
    public ?? FileName;                                 // The filepath to the texture.
    public int? ImageData;                              // Link to the internally stored image data.
    public uint UnknownInt = 7;                         // Unknown.  Often seems to be 7. Perhaps m_uiMipLevels?
    public float UnknownFloat = 128.5f;                 // Unknown.  Perhaps fImageScale?

    public NiImage(BinaryReader r, Header h) : base(r, h) {
        UseExternal = r.ReadByte();
        if (UseExternal != 0) FileName = ??;
        if (UseExternal == 0) ImageData = X<NiRawImageData>.Ref(r);
        UnknownInt = r.ReadUInt32();
        if (h.V >= 0x03010000) UnknownFloat = r.ReadSingle();
    }
}

/// <summary>
/// LEGACY (pre-10.1)
/// </summary>
public class NiTextureProperty : NiProperty {
    public uint[] UnknownInts1;                         // Property flags.
    public Flags Flags;                                 // Property flags.
    public int? Image;                                  // Link to the texture image.
    public uint[] UnknownInts2;                         // Unknown.  0?

    public NiTextureProperty(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x02030000) UnknownInts1 = r.ReadPArray<uint>("I", 2);
        if (h.V >= 0x03000000) Flags = (Flags)r.ReadUInt16();
        Image = X<NiImage>.Ref(r);
        if (h.V >= 0x03000000 && h.V <= 0x03000300) UnknownInts2 = r.ReadPArray<uint>("I", 2);
    }
}

/// <summary>
/// Describes how a fragment shader should be configured for a given piece of geometry.
/// </summary>
public class NiTexturingProperty : NiProperty { // X
    public Flags Flags;                                 // Property flags.
    public ApplyMode ApplyMode = APPLY_MODULATE;        // Determines how the texture will be applied.  Seems to have special functions in Oblivion.
    public uint TextureCount = 7;                       // Number of textures.
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

    public NiTexturingProperty(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x0A000102) Flags = (Flags)r.ReadUInt16();
        if (h.V >= 0x14010002) Flags = (Flags)r.ReadUInt16();
        if (h.V >= 0x0303000D && h.V <= 0x14010001) ApplyMode = (ApplyMode)r.ReadUInt32();
        TextureCount = r.ReadUInt32();
        HasBaseTexture = r.ReadBool32();
        if (HasBaseTexture) BaseTexture = new TexDesc(r, h);
        HasDarkTexture = r.ReadBool32();
        if (HasDarkTexture) DarkTexture = new TexDesc(r, h);
        HasDetailTexture = r.ReadBool32();
        if (HasDetailTexture) DetailTexture = new TexDesc(r, h);
        HasGlossTexture = r.ReadBool32();
        if (HasGlossTexture) GlossTexture = new TexDesc(r, h);
        HasGlowTexture = r.ReadBool32();
        if (HasGlowTexture) GlowTexture = new TexDesc(r, h);
        if (TextureCount > 5 && h.V >= 0x0303000D) HasBumpMapTexture = r.ReadBool32();
        if (HasBumpMapTexture) {
            BumpMapTexture = new TexDesc(r, h);
            BumpMapLumaScale = r.ReadSingle();
            BumpMapLumaOffset = r.ReadSingle();
            BumpMapMatrix = r.ReadMatrix2x2();
        }
        if (TextureCount > 6 && h.V >= 0x14020005) HasNormalTexture = r.ReadBool32();
        if (HasNormalTexture) NormalTexture = new TexDesc(r, h);
        if (TextureCount > 7 && h.V >= 0x14020005) HasParallaxTexture = r.ReadBool32();
        if (HasParallaxTexture) {
            ParallaxTexture = new TexDesc(r, h);
            ParallaxOffset = r.ReadSingle();
        }
        if (TextureCount > 6 && h.V <= 0x14020004) HasDecal0Texture = r.ReadBool32();
        if (TextureCount > 8 && h.V >= 0x14020005) HasDecal0Texture = r.ReadBool32();
        if (HasDecal0Texture) Decal0Texture = new TexDesc(r, h);
        if (TextureCount > 7 && h.V <= 0x14020004) HasDecal1Texture = r.ReadBool32();
        if (TextureCount > 9 && h.V >= 0x14020005) HasDecal1Texture = r.ReadBool32();
        if (HasDecal1Texture) Decal1Texture = new TexDesc(r, h);
        if (TextureCount > 8 && h.V <= 0x14020004) HasDecal2Texture = r.ReadBool32();
        if (TextureCount > 10 && h.V >= 0x14020005) HasDecal2Texture = r.ReadBool32();
        if (HasDecal2Texture) Decal2Texture = new TexDesc(r, h);
        if (TextureCount > 9 && h.V <= 0x14020004) HasDecal3Texture = r.ReadBool32();
        if (TextureCount > 11 && h.V >= 0x14020005) HasDecal3Texture = r.ReadBool32();
        if (HasDecal3Texture) Decal3Texture = new TexDesc(r, h);
        if (h.V >= 0x0A000100) ShaderTextures = r.ReadL32FArray(r => new ShaderTexDesc(r));
    }
}

public class NiMultiTextureProperty(BinaryReader r, Header h) : NiTexturingProperty(r, h) {
}

/// <summary>
/// Wrapper for transformation animation keys.
/// </summary>
public class NiTransformData(BinaryReader r, Header h) : NiKeyframeData(r, h) {
}

/// <summary>
/// A shape node that refers to singular triangle data.
/// </summary>
public class NiTriShape(BinaryReader r, Header h) : NiTriBasedGeom(r, h) { // X
}

/// <summary>
/// Holds mesh data using a list of singular triangles.
/// </summary>
public class NiTriShapeData : NiTriBasedGeomData { // X
    public uint NumTrianglePoints;                      // Num Triangles times 3.
    public bool HasTriangles;                           // Do we have triangle data?
    public Triangle[] Triangles;                        // Triangle face data.
    public MatchGroup[] MatchGroups;                    // The shared normals.

    public NiTriShapeData(BinaryReader r, Header h) : base(r, h) {
        NumTrianglePoints = r.ReadUInt32();
        if (h.V >= 0x0A010000) HasTriangles = r.ReadBool32();
        if (h.V <= 0x0A000102) Triangles = r.ReadFArray(r => new Triangle(r), NumTriangles);
        if (HasTriangles && h.V >= 0x0A000103) Triangles = r.ReadFArray(r => new Triangle(r), NumTriangles);
        if (h.V >= 0x03010000) MatchGroups = r.ReadL16FArray(r => new MatchGroup(r));
    }
}

/// <summary>
/// A shape node that refers to data organized into strips of triangles
/// </summary>
public class NiTriStrips(BinaryReader r, Header h) : NiTriBasedGeom(r, h) {
}

/// <summary>
/// Holds mesh data using strips of triangles.
/// </summary>
public class NiTriStripsData : NiTriBasedGeomData {
    public ushort NumStrips;                            // Number of OpenGL triangle strips that are present.
    public ushort[] StripLengths;                       // The number of points in each triangle strip.
    public bool HasPoints;                              // Do we have strip point data?
    public ushort[][] Points;                           // The points in the Triangle strips. Size is the sum of all entries in Strip Lengths.

    public NiTriStripsData(BinaryReader r, Header h) : base(r, h) {
        NumStrips = r.ReadUInt16();
        StripLengths = r.ReadPArray<ushort>("H", NumStrips);
        if (h.V >= 0x0A000103) HasPoints = r.ReadBool32();
        if (h.V <= 0x0A000102) Points = r.ReadFArray(k => r.ReadPArray<ushort>("H", NumStrips), StripLengths);
        if (HasPoints && h.V >= 0x0A000103) Points = r.ReadFArray(k => r.ReadPArray<ushort>("H", NumStrips), StripLengths);
    }
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

    public NiEnvMappedTriShape(BinaryReader r, Header h) : base(r, h) {
        Unknown1 = r.ReadUInt16();
        UnknownMatrix = r.ReadMatrix4x4();
        Children = r.ReadL32FArray(X<NiAVObject>.Ref);
        Child2 = X<NiObject>.Ref(r);
        Child3 = X<NiObject>.Ref(r);
    }
}

/// <summary>
/// Holds mesh data using a list of singular triangles.
/// </summary>
public class NiEnvMappedTriShapeData(BinaryReader r, Header h) : NiTriShapeData(r, h) {
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

    public NiBezierTriangle4(BinaryReader r, Header h) : base(r, h) {
        Unknown1 = r.ReadPArray<uint>("I", 6);
        Unknown2 = r.ReadUInt16();
        Matrix = r.ReadMatrix3x3();
        Vector1 = r.ReadVector3();
        Vector2 = r.ReadVector3();
        Unknown3 = r.ReadPArray<short>("h", 4);
        Unknown4 = r.ReadByte();
        Unknown5 = r.ReadUInt32();
        Unknown6 = r.ReadPArray<short>("h", 24);
    }
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
    public ushort[][] Data2;                            // data count.

    public NiBezierMesh(BinaryReader r, Header h) : base(r, h) {
        BezierTriangle = r.ReadL32FArray(X<NiBezierTriangle4>.Ref);
        Unknown3 = r.ReadUInt32();
        Count1 = r.ReadUInt16();
        Unknown4 = r.ReadUInt16();
        Points1 = r.ReadPArray<Vector3>("3f", Count1);
        Unknown5 = r.ReadUInt32();
        Points2 = r.ReadFArray(k => r.ReadPArray<float>("f", Count1), 2);
        Unknown6 = r.ReadUInt32();
        Data2 = r.ReadFArray(k => r.ReadL16PArray<ushort>("H"), 4);
    }
}

/// <summary>
/// A shape node that holds continuous level of detail information.
/// Seems to be specific to Freedom Force.
/// </summary>
public class NiClod(BinaryReader r, Header h) : NiTriBasedGeom(r, h) {
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

    public NiClodData(BinaryReader r, Header h) : base(r, h) {
        UnknownShorts = r.ReadUInt16();
        UnknownCount1 = r.ReadUInt16();
        UnknownCount2 = r.ReadUInt16();
        UnknownCount3 = r.ReadUInt16();
        UnknownFloat = r.ReadSingle();
        UnknownShort = r.ReadUInt16();
        UnknownClodShorts1 = r.ReadFArray(k => r.ReadPArray<ushort>("H", UnknownCount1), 6);
        UnknownClodShorts2 = r.ReadPArray<ushort>("H", UnknownCount2);
        UnknownClodShorts3 = r.ReadFArray(k => r.ReadPArray<ushort>("H", UnknownCount3), 6);
    }
}

/// <summary>
/// DEPRECATED (pre-10.1), REMOVED (20.3).
/// Time controller for texture coordinates.
/// </summary>
public class NiUVController : NiTimeController { // X
    public ushort UnknownShort;                         // Always 0?
    public int? Data;                                   // Texture coordinate controller data index.

    public NiUVController(BinaryReader r, Header h) : base(r, h) {
        UnknownShort = r.ReadUInt16();
        Data = X<NiUVData>.Ref(r);
    }
}

/// <summary>
/// DEPRECATED (pre-10.1), REMOVED (20.3)
/// Texture coordinate data.
/// </summary>
public class NiUVData : NiObject { // X
    public KeyGroup<float>[] UVGroups;                  // Four UV data groups. Appear to be U translation, V translation, U scaling/tiling, V scaling/tiling.

    public NiUVData(BinaryReader r, Header h) : base(r, h) {
        UVGroups = r.ReadFArray(r => new KeyGroup<float>(r), 4);
    }
}

/// <summary>
/// DEPRECATED (20.5).
/// Extra data in the form of a vector (as x, y, z, w components).
/// </summary>
public class NiVectorExtraData : NiExtraData {
    public Vector4 VectorData;                          // The vector data.

    public NiVectorExtraData(BinaryReader r, Header h) : base(r, h) {
        VectorData = r.ReadVector4();
    }
}

/// <summary>
/// Property of vertex colors. This object is referred to by the root object of the NIF file whenever some NiTriShapeData object has vertex colors with non-default settings; if not present, vertex colors have vertex_mode=2 and lighting_mode=1.
/// </summary>
public class NiVertexColorProperty : NiProperty { // X
    public Flags Flags;                                 // Bits 0-2: Unknown
                                                        //     Bit 3: Lighting Mode
                                                        //     Bits 4-5: Vertex Mode
    public VertMode VertexMode;                         // In Flags from 20.1.0.3 on.
    public LightMode LightingMode;                      // In Flags from 20.1.0.3 on.

    public NiVertexColorProperty(BinaryReader r, Header h) : base(r, h) {
        Flags = (Flags)r.ReadUInt16();
        if (h.V <= 0x14000005) {
            VertexMode = (VertMode)r.ReadUInt32();
            LightingMode = (LightMode)r.ReadUInt32();
        }
    }
}

/// <summary>
/// DEPRECATED (10.x), REMOVED (?)
/// Not used in skinning.
/// Unsure of use - perhaps for morphing animation or gravity.
/// </summary>
public class NiVertWeightsExtraData : NiExtraData { // X
    public uint NumBytes;                               // Number of bytes in this data object.
    public float[] Weight;                              // The vertex weights.

    public NiVertWeightsExtraData(BinaryReader r, Header h) : base(r, h) {
        NumBytes = r.ReadUInt32();
        Weight = r.ReadL16PArray<float>("f");
    }
}

/// <summary>
/// DEPRECATED (10.2), REMOVED (?), Replaced by NiBoolData.
/// Visibility data for a controller.
/// </summary>
public class NiVisData : NiObject { // X
    public Key<byte>[] Keys;

    public NiVisData(BinaryReader r, Header h) : base(r, h) {
        Keys = r.ReadL32FArray(r => new Key<byte>(r, Interpolation));
    }
}

/// <summary>
/// Allows applications to switch between drawing solid geometry or wireframe outlines.
/// </summary>
public class NiWireframeProperty : NiProperty { // X
    public Flags Flags;                                 // Property flags.
                                                        //     0 - Wireframe Mode Disabled
                                                        //     1 - Wireframe Mode Enabled

    public NiWireframeProperty(BinaryReader r, Header h) : base(r, h) {
        Flags = (Flags)r.ReadUInt16();
    }
}

/// <summary>
/// Allows applications to set the test and write modes of the renderer's Z-buffer and to set the comparison function used for the Z-buffer test.
/// </summary>
public class NiZBufferProperty : NiProperty { // X
    public Flags Flags = 3;                             // Bit 0 enables the z test
                                                        //     Bit 1 controls wether the Z buffer is read only (0) or read/write (1)
    public ZCompareMode Function = ZCOMP_LESS_EQUAL;    // Z-Test function (see: glDepthFunc). In Flags from 20.1.0.3 on.

    public NiZBufferProperty(BinaryReader r, Header h) : base(r, h) {
        Flags = (Flags)r.ReadUInt16();
        if (h.V >= 0x0401000C && h.V <= 0x14000005) Function = (ZCompareMode)r.ReadUInt32();
    }
}

/// <summary>
/// Morrowind-specific node for collision mesh.
/// </summary>
public class RootCollisionNode(BinaryReader r, Header h) : NiNode(r, h) { // X
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

    public NiRawImageData(BinaryReader r, Header h) : base(r, h) {
        Width = r.ReadUInt32();
        Height = r.ReadUInt32();
        ImageType = (ImageType)r.ReadUInt32();
        if (ImageType == 1) RGBImageData = r.ReadFArray(k => r.ReadFArray(r => new ByteColor3(r), Width), Height);
        if (ImageType == 2) RGBAImageData = r.ReadFArray(k => r.ReadFArray(r => new Color4Byte(r), Width), Height);
    }
}

public abstract class NiAccumulator(BinaryReader r, Header h) : NiObject(r, h) {
}

/// <summary>
/// Used to turn sorting off for individual subtrees in a scene. Useful if objects must be drawn in a fixed order.
/// </summary>
public class NiSortAdjustNode : NiNode {
    public SortingMode SortingMode = SORTING_INHERIT;   // Sorting
    public int? Accumulator;

    public NiSortAdjustNode(BinaryReader r, Header h) : base(r, h) {
        SortingMode = (SortingMode)r.ReadUInt32();
        if (h.V <= 0x14000003) Accumulator = X<NiAccumulator>.Ref(r);
    }
}

/// <summary>
/// Represents cube maps that are created from either a set of six image files, six blocks of pixel data, or a single pixel data with six faces.
/// </summary>
public class NiSourceCubeMap(BinaryReader r, Header h) : NiSourceTexture(r, h) {
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

    public NiPhysXProp(BinaryReader r, Header h) : base(r, h) {
        PhysXtoWorldScale = r.ReadSingle();
        Sources = r.ReadL32FArray(X<NiObject>.Ref);
        Dests = r.ReadL32FArray(X<NiPhysXDest>.Ref);
        if (h.V >= 0x14040000) ModifiedMeshes = r.ReadL32FArray(X<NiMesh>.Ref);
        if (h.V >= 0x1E010002 && h.V <= 0x1E020002) TempName = Y.String(r);
        KeepMeshes = r.ReadBool32();
        PropDescription = X<NiPhysXPropDesc>.Ref(r);
    }
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

    public NiPhysXPropDesc(BinaryReader r, Header h) : base(r, h) {
        Actors = r.ReadL32FArray(X<NiPhysXActorDesc>.Ref);
        Joints = r.ReadL32FArray(X<NiPhysXJointDesc>.Ref);
        if (h.V >= 0x14030005) Clothes = r.ReadL32FArray(X<NiObject>.Ref);
        Materials = r.ReadL32FArray(r => new PhysXMaterialRef(r));
        NumStates = r.ReadUInt32();
        if (h.V >= 0x14040000) {
            StateNames = r.ReadL32FArray(r => new PhysXStateName(r));
            Flags = r.ReadByte();
        }
    }
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

    public NiPhysXActorDesc(BinaryReader r, Header h) : base(r, h) {
        ActorName = Y.String(r);
        Poses = r.ReadL32FArray(r => r.ReadL32Matrix3x4());
        BodyDesc = X<NiPhysXBodyDesc>.Ref(r);
        Density = r.ReadSingle();
        ActorFlags = r.ReadUInt32();
        ActorGroup = r.ReadUInt16();
        if (h.V >= 0x14040000) {
            DominanceGroup = r.ReadUInt16();
            ContactReportFlags = r.ReadUInt32();
            ForceFieldMaterial = r.ReadUInt16();
        }
        if (h.V >= 0x14030001 && h.V <= 0x14030005) Dummy = r.ReadUInt32();
        ShapeDescriptions = r.ReadL32FArray(X<NiPhysXShapeDesc>.Ref);
        ActorParent = X<NiPhysXActorDesc>.Ref(r);
        Source = X<NiPhysXRigidBodySrc>.Ref(r);
        Dest = X<NiPhysXRigidBodyDest>.Ref(r);
    }
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

    public NiPhysXBodyDesc(BinaryReader r, Header h) : base(r, h) {
        LocalPose = r.ReadMatrix3x4();
        SpaceInertia = r.ReadVector3();
        Mass = r.ReadSingle();
        Vels = r.ReadL32FArray(r => new PhysXBodyStoredVels(r, h));
        WakeUpCounter = r.ReadSingle();
        LinearDamping = r.ReadSingle();
        AngularDamping = r.ReadSingle();
        MaxAngularVelocity = r.ReadSingle();
        CCDMotionThreshold = r.ReadSingle();
        Flags = r.ReadUInt32();
        SleepLinearVelocity = r.ReadSingle();
        SleepAngularVelocity = r.ReadSingle();
        SolverIterationCount = r.ReadUInt32();
        if (h.V >= 0x14030000) {
            SleepEnergyThreshold = r.ReadSingle();
            SleepDamping = r.ReadSingle();
        }
        if (h.V >= 0x14040000) ContactReportThreshold = r.ReadSingle();
    }
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

    public NiPhysXJointDesc(BinaryReader r, Header h) : base(r, h) {
        JointType = (NxJointType)r.ReadUInt32();
        JointName = Y.String(r);
        Actors = r.ReadFArray(r => new NiPhysXJointActor(r), 2);
        MaxForce = r.ReadSingle();
        MaxTorque = r.ReadSingle();
        if (h.V >= 0x14050003) {
            SolverExtrapolationFactor = r.ReadSingle();
            UseAccelerationSpring = r.ReadUInt32();
        }
        JointFlags = r.ReadUInt32();
        LimitPoint = r.ReadVector3();
        Limits = r.ReadL32FArray(r => new NiPhysXJointLimit(r, h));
    }
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

    public NiPhysXD6JointDesc(BinaryReader r, Header h) : base(r, h) {
        XMotion = (NxD6JointMotion)r.ReadUInt32();
        YMotion = (NxD6JointMotion)r.ReadUInt32();
        ZMotion = (NxD6JointMotion)r.ReadUInt32();
        Swing1Motion = (NxD6JointMotion)r.ReadUInt32();
        Swing2Motion = (NxD6JointMotion)r.ReadUInt32();
        TwistMotion = (NxD6JointMotion)r.ReadUInt32();
        LinearLimit = new NxJointLimitSoftDesc(r);
        Swing1Limit = new NxJointLimitSoftDesc(r);
        Swing2Limit = new NxJointLimitSoftDesc(r);
        TwistLowLimit = new NxJointLimitSoftDesc(r);
        TwistHighLimit = new NxJointLimitSoftDesc(r);
        XDrive = new NxJointDriveDesc(r);
        YDrive = new NxJointDriveDesc(r);
        ZDrive = new NxJointDriveDesc(r);
        SwingDrive = new NxJointDriveDesc(r);
        TwistDrive = new NxJointDriveDesc(r);
        SlerpDrive = new NxJointDriveDesc(r);
        DrivePosition = r.ReadVector3();
        DriveOrientation = r.ReadQuaternion();
        DriveLinearVelocity = r.ReadVector3();
        DriveAngularVelocity = r.ReadVector3();
        ProjectionMode = (NxJointProjectionMode)r.ReadUInt32();
        ProjectionDistance = r.ReadSingle();
        ProjectionAngle = r.ReadSingle();
        GearRatio = r.ReadSingle();
        Flags = r.ReadUInt32();
    }
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

    public NiPhysXShapeDesc(BinaryReader r, Header h) : base(r, h) {
        ShapeType = (NxShapeType)r.ReadUInt32();
        LocalPose = r.ReadMatrix3x4();
        ShapeFlags = r.ReadUInt32();
        CollisionGroup = r.ReadUInt16();
        MaterialIndex = r.ReadUInt16();
        Density = r.ReadSingle();
        Mass = r.ReadSingle();
        SkinWidth = r.ReadSingle();
        ShapeName = Y.String(r);
        if (h.V >= 0x14040000) Non-InteractingCompartmentTypes = r.ReadUInt32();
        CollisionBits = r.ReadPArray<uint>("I", 4);
        if (ShapeType == 0) Plane = new NxPlane(r);
        if (ShapeType == 1) SphereRadius = r.ReadSingle();
        if (ShapeType == 2) BoxHalfExtents = r.ReadVector3();
        if (ShapeType == 3) Capsule = new NxCapsule(r);
        if ((ShapeType == 5) || (ShapeType == 6)) Mesh = X<NiPhysXMeshDesc>.Ref(r);
    }
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

    public NiPhysXMeshDesc(BinaryReader r, Header h) : base(r, h) {
        if (h.V <= 0x14030004) IsConvex = r.ReadBool32();
        MeshName = Y.String(r);
        MeshData = r.ReadL8Bytes();
        if (h.V >= 0x14030005 && h.V <= 0x1E020002) {
            MeshSize = r.ReadUInt16();
            MeshData = r.ReadPArray<ushort>("H", MeshSize);
        }
        MeshFlags = r.ReadUInt32();
        if (h.V >= 0x14030001) MeshPagingMode = r.ReadUInt32();
        if (h.V >= 0x14030002 && h.V <= 0x14030004) IsHardware = r.ReadBool32();
        if (h.V >= 0x14030005) Flags = r.ReadByte();
    }
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

public class NxMaterialDesc {
    public float DynamicFriction;
    public float StaticFriction;
    public float Restitution;
    public float DynamicFrictionV;
    public float StaticFrictionV;
    public Vector3 DirectionofAnisotropy;
    public NxMaterialFlag Flags;
    public NxCombineMode FrictionCombineMode;
    public NxCombineMode RestitutionCombineMode;
    public NxSpringDesc Spring;

    public NxMaterialDesc(BinaryReader r, Header h) {
        DynamicFriction = r.ReadSingle();
        StaticFriction = r.ReadSingle();
        Restitution = r.ReadSingle();
        DynamicFrictionV = r.ReadSingle();
        StaticFrictionV = r.ReadSingle();
        DirectionofAnisotropy = r.ReadVector3();
        Flags = (NxMaterialFlag)r.ReadUInt32();
        FrictionCombineMode = (NxCombineMode)r.ReadUInt32();
        RestitutionCombineMode = (NxCombineMode)r.ReadUInt32();
        if (r.ReadBool32() && h.V <= 0x14020300) Spring = new NxSpringDesc(r);
    }
}

/// <summary>
/// For serializing NxMaterialDesc objects.
/// </summary>
public class NiPhysXMaterialDesc : NiObject {
    public ushort Index;
    public NxMaterialDesc[] MaterialDescs;

    public NiPhysXMaterialDesc(BinaryReader r, Header h) : base(r, h) {
        Index = r.ReadUInt16();
        MaterialDescs = r.ReadL32FArray(r => new NxMaterialDesc(r, h));
    }
}

/// <summary>
/// A destination is a link between a PhysX actor and a Gamebryo object being driven by the physics.
/// </summary>
public abstract class NiPhysXDest : NiObject {
    public bool Active;
    public bool Interpolate;

    public NiPhysXDest(BinaryReader r, Header h) : base(r, h) {
        Active = r.ReadBool32();
        Interpolate = r.ReadBool32();
    }
}

/// <summary>
/// Base for destinations that set a rigid body state.
/// </summary>
public abstract class NiPhysXRigidBodyDest(BinaryReader r, Header h) : NiPhysXDest(r, h) {
}

/// <summary>
/// Connects PhysX rigid body actors to a scene node.
/// </summary>
public class NiPhysXTransformDest : NiPhysXRigidBodyDest {
    public int? Target;

    public NiPhysXTransformDest(BinaryReader r, Header h) : base(r, h) {
        Target = X<NiAVObject>.Ptr(r);
    }
}

/// <summary>
/// A source is a link between a Gamebryo object and a PhysX actor.
/// </summary>
public abstract class NiPhysXSrc : NiObject {
    public bool Active;
    public bool Interpolate;

    public NiPhysXSrc(BinaryReader r, Header h) : base(r, h) {
        Active = r.ReadBool32();
        Interpolate = r.ReadBool32();
    }
}

/// <summary>
/// Sets state of a rigid body PhysX actor.
/// </summary>
public abstract class NiPhysXRigidBodySrc : NiPhysXSrc {
    public int? Source;

    public NiPhysXRigidBodySrc(BinaryReader r, Header h) : base(r, h) {
        Source = X<NiAVObject>.Ptr(r);
    }
}

/// <summary>
/// Sets state of kinematic PhysX actor.
/// </summary>
public class NiPhysXKinematicSrc(BinaryReader r, Header h) : NiPhysXRigidBodySrc(r, h) {
}

/// <summary>
/// Sends Gamebryo scene state to a PhysX dynamic actor.
/// </summary>
public class NiPhysXDynamicSrc(BinaryReader r, Header h) : NiPhysXRigidBodySrc(r, h) {
}

/// <summary>
/// Wireframe geometry.
/// </summary>
public class NiLines(BinaryReader r, Header h) : NiTriBasedGeom(r, h) {
}

/// <summary>
/// Wireframe geometry data.
/// </summary>
public class NiLinesData : NiGeometryData {
    public bool[] Lines;                                // Is vertex connected to other (next?) vertex?

    public NiLinesData(BinaryReader r, Header h) : base(r, h) {
        Lines = r.ReadFArray(r => r.ReadBool32(), NumVertices);
    }
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
    public ushort PolygonGrowBy = 1;
    public ushort NumPolygons;
    public ushort MaxVertices;
    public ushort VerticesGrowBy = 1;
    public ushort MaxIndices;
    public ushort IndicesGrowBy = 1;

    public NiScreenElementsData(BinaryReader r, Header h) : base(r, h) {
        MaxPolygons = r.ReadUInt16();
        Polygons = r.ReadFArray(r => new Polygon(r), MaxPolygons);
        PolygonIndices = r.ReadPArray<ushort>("H", MaxPolygons);
        PolygonGrowBy = r.ReadUInt16();
        NumPolygons = r.ReadUInt16();
        MaxVertices = r.ReadUInt16();
        VerticesGrowBy = r.ReadUInt16();
        MaxIndices = r.ReadUInt16();
        IndicesGrowBy = r.ReadUInt16();
    }
}

/// <summary>
/// DEPRECATED (20.5), replaced by NiMeshScreenElements.
/// Two dimensional screen elements.
/// </summary>
public class NiScreenElements(BinaryReader r, Header h) : NiTriShape(r, h) {
}

/// <summary>
/// NiRoomGroup represents a set of connected rooms i.e. a game level.
/// </summary>
public class NiRoomGroup : NiNode {
    public int? Shell;                                  // Object that represents the room group as seen from the outside.
    public int?[] Rooms;

    public NiRoomGroup(BinaryReader r, Header h) : base(r, h) {
        Shell = X<NiNode>.Ptr(r);
        Rooms = r.ReadL32FArray(X<NiRoom>.Ptr);
    }
}

/// <summary>
/// NiRoom objects represent cells in a cell-portal culling system.
/// </summary>
public class NiRoom : NiNode {
    public NiPlane[] WallPlanes;
    public int?[] InPortals;                            // The portals which see into the room.
    public int?[] OutPortals;                           // The portals which see out of the room.
    public int?[] Fixtures;                             // All geometry associated with the room.

    public NiRoom(BinaryReader r, Header h) : base(r, h) {
        WallPlanes = r.ReadL32FArray(r => new NiPlane(r));
        InPortals = r.ReadL32FArray(X<NiPortal>.Ptr);
        OutPortals = r.ReadL32FArray(X<NiPortal>.Ptr);
        Fixtures = r.ReadL32FArray(X<NiAVObject>.Ptr);
    }
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

    public NiPortal(BinaryReader r, Header h) : base(r, h) {
        PortalFlags = r.ReadUInt16();
        PlaneCount = r.ReadUInt16();
        Vertices = r.ReadL16PArray<Vector3>("3f");
        Adjoiner = X<NiNode>.Ptr(r);
    }
}

/// <summary>
/// Bethesda-specific fade node.
/// </summary>
public class BSFadeNode(BinaryReader r, Header h) : NiNode(r, h) {
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
    public BSShaderType ShaderType = SHADER_DEFAULT;
    public BSShaderFlags ShaderFlags = 0x82000000;
    public BSShaderFlags2 ShaderFlags2 = 1;
    public float EnvironmentMapScale = 1.0f;            // Scales the intensity of the environment/cube map.

    public BSShaderProperty(BinaryReader r, Header h) : base(r, h) {
        if ((h.UV2 <= 34)) EnvironmentMapScale = r.ReadSingle();
    }
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public abstract class BSShaderLightingProperty : BSShaderProperty {
    public TexClampMode TextureClampMode = 3;           // How to handle texture borders.

    public BSShaderLightingProperty(BinaryReader r, Header h) : base(r, h) {
        if ((h.UV2 <= 34)) TextureClampMode = (TexClampMode)r.ReadUInt32();
    }
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class BSShaderNoLightingProperty : BSShaderLightingProperty {
    public string FileName;                             // The texture glow map.
    public float FalloffStartAngle = 1.0f;              // At this cosine of angle falloff will be equal to Falloff Start Opacity
    public float FalloffStopAngle = 0.0f;               // At this cosine of angle falloff will be equal to Falloff Stop Opacity
    public float FalloffStartOpacity = 1.0f;            // Alpha falloff multiplier at start angle
    public float FalloffStopOpacity = 0.0f;             // Alpha falloff multiplier at end angle

    public BSShaderNoLightingProperty(BinaryReader r, Header h) : base(r, h) {
        FileName = r.ReadL32AString();
        if ((h.UV2 > 26)) {
            FalloffStartAngle = r.ReadSingle();
            FalloffStopAngle = r.ReadSingle();
            FalloffStartOpacity = r.ReadSingle();
            FalloffStopOpacity = r.ReadSingle();
        }
    }
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class BSShaderPPLightingProperty : BSShaderLightingProperty {
    public int? TextureSet;                             // Texture Set
    public float RefractionStrength = 0.0f;             // The amount of distortion. **Not based on physically accurate refractive index** (0=none) (0-1)
    public int RefractionFirePeriod = 0;                // Rate of texture movement for refraction shader.
    public float ParallaxMaxPasses = 4.0f;              // The number of passes the parallax shader can apply.
    public float ParallaxScale = 1.0f;                  // The strength of the parallax.
    public Color4 EmissiveColor;                        // Glow color and alpha

    public BSShaderPPLightingProperty(BinaryReader r, Header h) : base(r, h) {
        TextureSet = X<BSShaderTextureSet>.Ref(r);
        if ((h.UV2 > 14)) {
            RefractionStrength = r.ReadSingle();
            RefractionFirePeriod = r.ReadInt32();
        }
        if ((h.UV2 > 24)) {
            ParallaxMaxPasses = r.ReadSingle();
            ParallaxScale = r.ReadSingle();
        }
        if ((h.UV2 > 34)) EmissiveColor = new Color4(r);
    }
}

/// <summary>
/// This controller is used to animate float variables in BSEffectShaderProperty.
/// </summary>
public class BSEffectShaderPropertyFloatController : NiFloatInterpController {
    public EffectShaderControlledVariable TypeofControlledVariable; // Which float variable in BSEffectShaderProperty to animate:

    public BSEffectShaderPropertyFloatController(BinaryReader r, Header h) : base(r, h) {
        TypeofControlledVariable = (EffectShaderControlledVariable)r.ReadUInt32();
    }
}

/// <summary>
/// This controller is used to animate colors in BSEffectShaderProperty.
/// </summary>
public class BSEffectShaderPropertyColorController : NiPoint3InterpController {
    public EffectShaderControlledColor TypeofControlledColor; // Which color in BSEffectShaderProperty to animate:

    public BSEffectShaderPropertyColorController(BinaryReader r, Header h) : base(r, h) {
        TypeofControlledColor = (EffectShaderControlledColor)r.ReadUInt32();
    }
}

/// <summary>
/// This controller is used to animate float variables in BSLightingShaderProperty.
/// </summary>
public class BSLightingShaderPropertyFloatController : NiFloatInterpController {
    public LightingShaderControlledVariable TypeofControlledVariable; // Which float variable in BSLightingShaderProperty to animate:

    public BSLightingShaderPropertyFloatController(BinaryReader r, Header h) : base(r, h) {
        TypeofControlledVariable = (LightingShaderControlledVariable)r.ReadUInt32();
    }
}

/// <summary>
/// This controller is used to animate colors in BSLightingShaderProperty.
/// </summary>
public class BSLightingShaderPropertyColorController : NiPoint3InterpController {
    public LightingShaderControlledColor TypeofControlledColor; // Which color in BSLightingShaderProperty to animate:

    public BSLightingShaderPropertyColorController(BinaryReader r, Header h) : base(r, h) {
        TypeofControlledColor = (LightingShaderControlledColor)r.ReadUInt32();
    }
}

public class BSNiAlphaPropertyTestRefController(BinaryReader r, Header h) : NiFloatInterpController(r, h) {
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

    public BSProceduralLightningController(BinaryReader r, Header h) : base(r, h) {
        Interpolator1:Generation = X<NiInterpolator>.Ref(r);
        Interpolator2:Mutation = X<NiInterpolator>.Ref(r);
        Interpolator3:Subdivision = X<NiInterpolator>.Ref(r);
        Interpolator4:NumBranches = X<NiInterpolator>.Ref(r);
        Interpolator5:NumBranchesVar = X<NiInterpolator>.Ref(r);
        Interpolator6:Length = X<NiInterpolator>.Ref(r);
        Interpolator7:LengthVar = X<NiInterpolator>.Ref(r);
        Interpolator8:Width = X<NiInterpolator>.Ref(r);
        Interpolator9:ArcOffset = X<NiInterpolator>.Ref(r);
        Subdivisions = r.ReadUInt16();
        NumBranches = r.ReadUInt16();
        NumBranchesVariation = r.ReadUInt16();
        Length = r.ReadSingle();
        LengthVariation = r.ReadSingle();
        Width = r.ReadSingle();
        ChildWidthMult = r.ReadSingle();
        ArcOffset = r.ReadSingle();
        FadeMainBolt = r.ReadBool32();
        FadeChildBolts = r.ReadBool32();
        AnimateArcOffset = r.ReadBool32();
        ShaderProperty = X<BSShaderProperty>.Ref(r);
    }
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

    public BSShaderTextureSet(BinaryReader r, Header h) : base(r, h) {
        Textures = r.ReadL32FArray(r => r.ReadL32L32AString());
    }
}

/// <summary>
/// Bethesda-specific property. Found in Fallout3
/// </summary>
public class WaterShaderProperty(BinaryReader r, Header h) : BSShaderProperty(r, h) {
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

    public SkyShaderProperty(BinaryReader r, Header h) : base(r, h) {
        FileName = r.ReadL32AString();
        SkyObjectType = (SkyObjectType)r.ReadUInt32();
    }
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class TileShaderProperty : BSShaderLightingProperty {
    public string FileName;                             // Texture file name

    public TileShaderProperty(BinaryReader r, Header h) : base(r, h) {
        FileName = r.ReadL32AString();
    }
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class DistantLODShaderProperty(BinaryReader r, Header h) : BSShaderProperty(r, h) {
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class BSDistantTreeShaderProperty(BinaryReader r, Header h) : BSShaderProperty(r, h) {
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class TallGrassShaderProperty : BSShaderProperty {
    public string FileName;                             // Texture file name

    public TallGrassShaderProperty(BinaryReader r, Header h) : base(r, h) {
        FileName = r.ReadL32AString();
    }
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class VolumetricFogShaderProperty(BinaryReader r, Header h) : BSShaderProperty(r, h) {
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class HairShaderProperty(BinaryReader r, Header h) : BSShaderProperty(r, h) {
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class Lighting30ShaderProperty(BinaryReader r, Header h) : BSShaderPPLightingProperty(r, h) {
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
    public SkyrimShaderPropertyFlags1 ShaderFlags1_SK = 2185233153; // Skyrim Shader Flags for setting render/shader options.
    public SkyrimShaderPropertyFlags2 ShaderFlags2_SK = 32801; // Skyrim Shader Flags for setting render/shader options.
    public Fallout4ShaderPropertyFlags1 ShaderFlags1_FO4 = 2151678465; // Fallout 4 Shader Flags. Mostly overridden if "Name" is a path to a BGSM/BGEM file.
    public Fallout4ShaderPropertyFlags2 ShaderFlags2_FO4 = 1; // Fallout 4 Shader Flags. Mostly overridden if "Name" is a path to a BGSM/BGEM file.
    public TexCoord UVOffset;                           // Offset UVs
    public TexCoord UVScale = new TexCord(1.0, 1.0);    // Offset UV Scale to repeat tiling textures, see above.
    public int? TextureSet;                             // Texture Set, can have override in an esm/esp
    public Color3 EmissiveColor = 0.0, 0.0, 0.0;        // Glow color and alpha
    public float EmissiveMultiple;                      // Multiplied emissive colors
    public string WetMaterial;
    public TexClampMode TextureClampMode = 3;           // How to handle texture borders.
    public float Alpha = 1.0f;                          // The material opacity (1=non-transparent).
    public float RefractionStrength;                    // The amount of distortion. **Not based on physically accurate refractive index** (0=none) (0-1)
    public float Glossiness = 80f;                      // The material specular power, or glossiness (0-999).
    public float Smoothness = 1.0f;                     // The base roughness (0.0-1.0), multiplied by the smoothness map.
    public Color3 SpecularColor;                        // Adds a colored highlight.
    public float SpecularStrength = 1.0f;               // Brightness of specular highlight. (0=not visible) (0-999)
    public float LightingEffect1 = 0.3f;                // Controls strength for envmap/backlight/rim/softlight lighting effect?
    public float LightingEffect2 = 2.0f;                // Controls strength for envmap/backlight/rim/softlight lighting effect?
    public float SubsurfaceRolloff = 0.3f;
    public float RimlightPower = 3.402823466e+38f;
    public float BacklightPower;
    public float GrayscaletoPaletteScale;
    public float FresnelPower = 5.0f;
    public float WetnessSpecScale = -1.0f;
    public float WetnessSpecPower = -1.0f;
    public float WetnessMinVar = -1.0f;
    public float WetnessEnvMapScale = -1.0f;
    public float WetnessFresnelPower = -1.0f;
    public float WetnessMetalness = -1.0f;
    public float EnvironmentMapScale = 1.0f;            // Scales the intensity of the environment/cube map. (0-1)
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

    public BSLightingShaderProperty(BinaryReader r, Header h) : base(r, h) {
        if ((h.UV2 != 130)) ShaderFlags2_SK = (SkyrimShaderPropertyFlags2)r.ReadUInt32();
        if ((h.UV2 == 130)) {
            ShaderFlags1_FO4 = (Fallout4ShaderPropertyFlags1)r.ReadUInt32();
            ShaderFlags2_FO4 = (Fallout4ShaderPropertyFlags2)r.ReadUInt32();
        }
        UVOffset = new TexCoord(r);
        UVScale = new TexCoord(r);
        TextureSet = X<BSShaderTextureSet>.Ref(r);
        EmissiveColor = new Color3(r);
        EmissiveMultiple = r.ReadSingle();
        if ((h.UV2 == 130)) WetMaterial = Y.String(r);
        TextureClampMode = (TexClampMode)r.ReadUInt32();
        Alpha = r.ReadSingle();
        RefractionStrength = r.ReadSingle();
        if (h.UV2 < 130) Glossiness = r.ReadSingle();
        if ((h.UV2 == 130)) Smoothness = r.ReadSingle();
        SpecularColor = new Color3(r);
        SpecularStrength = r.ReadSingle();
        if (h.UV2 < 130) {
            LightingEffect1 = r.ReadSingle();
            LightingEffect2 = r.ReadSingle();
        }
        if ((h.UV2 == 130)) {
            SubsurfaceRolloff = r.ReadSingle();
            RimlightPower = r.ReadSingle();
            if (RimlightPower == 0x7F7FFFFF) BacklightPower = r.ReadSingle();
            GrayscaletoPaletteScale = r.ReadSingle();
            FresnelPower = r.ReadSingle();
            WetnessSpecScale = r.ReadSingle();
            WetnessSpecPower = r.ReadSingle();
            WetnessMinVar = r.ReadSingle();
            WetnessEnvMapScale = r.ReadSingle();
            WetnessFresnelPower = r.ReadSingle();
            WetnessMetalness = r.ReadSingle();
        }
        if (Skyrim Shader Type == 1) EnvironmentMapScale = r.ReadSingle();
        if (Skyrim Shader Type == 5 && (h.UV2 == 130)) {
            if (Skyrim Shader Type == 1) UnknownEnvMapShort = r.ReadUInt16();
            SkinTintColor = new Color3(r);
            UnknownSkinTintInt = r.ReadUInt32();
        }
        if (Skyrim Shader Type == 6) HairTintColor = new Color3(r);
        if (Skyrim Shader Type == 7) {
            MaxPasses = r.ReadSingle();
            Scale = r.ReadSingle();
        }
        if (Skyrim Shader Type == 11) {
            ParallaxInnerLayerThickness = r.ReadSingle();
            ParallaxRefractionScale = r.ReadSingle();
            ParallaxInnerLayerTextureScale = new TexCoord(r);
            ParallaxEnvmapStrength = r.ReadSingle();
        }
        if (Skyrim Shader Type == 14) SparkleParameters = r.ReadVector4();
        if (Skyrim Shader Type == 16) {
            EyeCubemapScale = r.ReadSingle();
            LeftEyeReflectionCenter = r.ReadVector3();
            RightEyeReflectionCenter = r.ReadVector3();
        }
    }
}

/// <summary>
/// Bethesda effect shader property for Skyrim and later.
/// </summary>
public class BSEffectShaderProperty : BSShaderProperty {
    public SkyrimShaderPropertyFlags1 ShaderFlags1_SK = 2147483648;
    public SkyrimShaderPropertyFlags2 ShaderFlags2_SK = 32;
    public Fallout4ShaderPropertyFlags1 ShaderFlags1_FO4 = 2147483648;
    public Fallout4ShaderPropertyFlags2 ShaderFlags2_FO4 = 32;
    public TexCoord UVOffset;                           // Offset UVs
    public TexCoord UVScale = new TexCord(1.0, 1.0);    // Offset UV Scale to repeat tiling textures
    public string SourceTexture;                        // points to an external texture.
    public byte TextureClampMode;                       // How to handle texture borders.
    public byte LightingInfluence;
    public byte EnvMapMinLOD;
    public byte UnknownByte;
    public float FalloffStartAngle = 1.0f;              // At this cosine of angle falloff will be equal to Falloff Start Opacity
    public float FalloffStopAngle = 1.0f;               // At this cosine of angle falloff will be equal to Falloff Stop Opacity
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

    public BSEffectShaderProperty(BinaryReader r, Header h) : base(r, h) {
        if ((h.UV2 != 130)) ShaderFlags2_SK = (SkyrimShaderPropertyFlags2)r.ReadUInt32();
        if ((h.UV2 == 130)) {
            ShaderFlags1_FO4 = (Fallout4ShaderPropertyFlags1)r.ReadUInt32();
            ShaderFlags2_FO4 = (Fallout4ShaderPropertyFlags2)r.ReadUInt32();
        }
        UVOffset = new TexCoord(r);
        UVScale = new TexCoord(r);
        SourceTexture = r.ReadL32AString();
        TextureClampMode = r.ReadByte();
        LightingInfluence = r.ReadByte();
        EnvMapMinLOD = r.ReadByte();
        UnknownByte = r.ReadByte();
        FalloffStartAngle = r.ReadSingle();
        FalloffStopAngle = r.ReadSingle();
        FalloffStartOpacity = r.ReadSingle();
        FalloffStopOpacity = r.ReadSingle();
        EmissiveColor = new Color4(r);
        EmissiveMultiple = r.ReadSingle();
        SoftFalloffDepth = r.ReadSingle();
        GreyscaleTexture = r.ReadL32AString();
        if ((h.UV2 == 130)) {
            EnvMapTexture = r.ReadL32AString();
            NormalTexture = r.ReadL32AString();
            EnvMaskTexture = r.ReadL32AString();
            EnvironmentMapScale = r.ReadSingle();
        }
    }
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
    public TexCoord UVScale = new TexCord(1.0, 1.0);    // Offset UV Scale to repeat tiling textures, see above.
    public SkyrimWaterShaderFlags WaterShaderFlags;     // Defines attributes for the water shader (will use SkyrimWaterShaderFlags)
    public byte WaterDirection = 3;                     // A bitflag, only the first/second bit controls water flow positive or negative along UVs.
    public ushort UnknownShort3;                        // Unknown, flag?

    public BSWaterShaderProperty(BinaryReader r, Header h) : base(r, h) {
        ShaderFlags1 = (SkyrimShaderPropertyFlags1)r.ReadUInt32();
        ShaderFlags2 = (SkyrimShaderPropertyFlags2)r.ReadUInt32();
        UVOffset = new TexCoord(r);
        UVScale = new TexCoord(r);
        WaterShaderFlags = (SkyrimWaterShaderFlags)r.ReadByte();
        WaterDirection = r.ReadByte();
        UnknownShort3 = r.ReadUInt16();
    }
}

/// <summary>
/// Skyrim Sky shader block.
/// </summary>
public class BSSkyShaderProperty : BSShaderProperty {
    public SkyrimShaderPropertyFlags1 ShaderFlags1;
    public SkyrimShaderPropertyFlags2 ShaderFlags2;
    public TexCoord UVOffset;                           // Offset UVs. Seems to be unused, but it fits with the other Skyrim shader properties.
    public TexCoord UVScale = new TexCord(1.0, 1.0);    // Offset UV Scale to repeat tiling textures, see above.
    public string SourceTexture;                        // points to an external texture.
    public SkyObjectType SkyObjectType;

    public BSSkyShaderProperty(BinaryReader r, Header h) : base(r, h) {
        ShaderFlags1 = (SkyrimShaderPropertyFlags1)r.ReadUInt32();
        ShaderFlags2 = (SkyrimShaderPropertyFlags2)r.ReadUInt32();
        UVOffset = new TexCoord(r);
        UVScale = new TexCoord(r);
        SourceTexture = r.ReadL32AString();
        SkyObjectType = (SkyObjectType)r.ReadUInt32();
    }
}

/// <summary>
/// Bethesda-specific skin instance.
/// </summary>
public class BSDismemberSkinInstance : NiSkinInstance {
    public BodyPartList[] Partitions;

    public BSDismemberSkinInstance(BinaryReader r, Header h) : base(r, h) {
        Partitions = r.ReadL32FArray(r => new BodyPartList(r));
    }
}

/// <summary>
/// Bethesda-specific extra data. Lists locations and normals on a mesh that are appropriate for decal placement.
/// </summary>
public class BSDecalPlacementVectorExtraData : NiFloatExtraData {
    public DecalVectorArray[] VectorBlocks;

    public BSDecalPlacementVectorExtraData(BinaryReader r, Header h) : base(r, h) {
        VectorBlocks = r.ReadL16FArray(r => new DecalVectorArray(r));
    }
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

    public BSPSysSimpleColorModifier(BinaryReader r, Header h) : base(r, h) {
        FadeInPercent = r.ReadSingle();
        FadeoutPercent = r.ReadSingle();
        Color1EndPercent = r.ReadSingle();
        Color1StartPercent = r.ReadSingle();
        Color2EndPercent = r.ReadSingle();
        Color2StartPercent = r.ReadSingle();
        Colors = r.ReadFArray(r => new Color4(r), 3);
    }
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

    public BSValueNode(BinaryReader r, Header h) : base(r, h) {
        Value = r.ReadUInt32();
        ValueNodeFlags = (BSValueNodeFlags)r.ReadByte();
    }
}

/// <summary>
/// Bethesda-Specific (mesh?) Particle System.
/// </summary>
public class BSStripParticleSystem(BinaryReader r, Header h) : NiParticleSystem(r, h) {
}

/// <summary>
/// Bethesda-Specific (mesh?) Particle System Data.
/// </summary>
public class BSStripPSysData : NiPSysData {
    public ushort MaxPointCount;
    public float StartCapSize;
    public float EndCapSize;
    public bool DoZPrepass;

    public BSStripPSysData(BinaryReader r, Header h) : base(r, h) {
        MaxPointCount = r.ReadUInt16();
        StartCapSize = r.ReadSingle();
        EndCapSize = r.ReadSingle();
        DoZPrepass = r.ReadBool32();
    }
}

/// <summary>
/// Bethesda-Specific (mesh?) Particle System Modifier.
/// </summary>
public class BSPSysStripUpdateModifier : NiPSysModifier {
    public float UpdateDeltaTime;

    public BSPSysStripUpdateModifier(BinaryReader r, Header h) : base(r, h) {
        UpdateDeltaTime = r.ReadSingle();
    }
}

/// <summary>
/// Bethesda-Specific time controller.
/// </summary>
public class BSMaterialEmittanceMultController(BinaryReader r, Header h) : NiFloatInterpController(r, h) {
}

/// <summary>
/// Bethesda-Specific particle system.
/// </summary>
public class BSMasterParticleSystem : NiNode {
    public ushort MaxEmitterObjects;
    public int?[] ParticleSystems;

    public BSMasterParticleSystem(BinaryReader r, Header h) : base(r, h) {
        MaxEmitterObjects = r.ReadUInt16();
        ParticleSystems = r.ReadL32FArray(X<NiAVObject>.Ref);
    }
}

/// <summary>
/// Particle system (multi?) emitter controller.
/// </summary>
public class BSPSysMultiTargetEmitterCtlr : NiPSysEmitterCtlr {
    public ushort MaxEmitters;
    public int? MasterParticleSystem;

    public BSPSysMultiTargetEmitterCtlr(BinaryReader r, Header h) : base(r, h) {
        MaxEmitters = r.ReadUInt16();
        MasterParticleSystem = X<BSMasterParticleSystem>.Ptr(r);
    }
}

/// <summary>
/// Bethesda-Specific time controller.
/// </summary>
public class BSRefractionStrengthController(BinaryReader r, Header h) : NiFloatInterpController(r, h) {
}

/// <summary>
/// Bethesda-Specific node.
/// </summary>
public class BSOrderedNode : NiNode {
    public Vector4 AlphaSortBound;
    public bool StaticBound;

    public BSOrderedNode(BinaryReader r, Header h) : base(r, h) {
        AlphaSortBound = r.ReadVector4();
        StaticBound = r.ReadBool32();
    }
}

/// <summary>
/// Bethesda-Specific node.
/// </summary>
public class BSRangeNode : NiNode {
    public byte Min;
    public byte Max;
    public byte Current;

    public BSRangeNode(BinaryReader r, Header h) : base(r, h) {
        Min = r.ReadByte();
        Max = r.ReadByte();
        Current = r.ReadByte();
    }
}

/// <summary>
/// Bethesda-Specific node.
/// </summary>
public class BSBlastNode(BinaryReader r, Header h) : BSRangeNode(r, h) {
}

/// <summary>
/// Bethesda-Specific node.
/// </summary>
public class BSDamageStage(BinaryReader r, Header h) : BSBlastNode(r, h) {
}

/// <summary>
/// Bethesda-specific time controller.
/// </summary>
public class BSRefractionFirePeriodController : NiTimeController {
    public int? Interpolator;

    public BSRefractionFirePeriodController(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x14020007) Interpolator = X<NiInterpolator>.Ref(r);
    }
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

    public bhkConvexListShape(BinaryReader r, Header h) : base(r, h) {
        SubShapes = r.ReadL32FArray(X<bhkConvexShape>.Ref);
        Material = new HavokMaterial(r, h);
        Radius = r.ReadSingle();
        UnknownInt1 = r.ReadUInt32();
        UnknownFloat1 = r.ReadSingle();
        ChildShapeProperty = new hkWorldObjCinfoProperty(r);
        UnknownByte1 = r.ReadByte();
        UnknownFloat2 = r.ReadSingle();
    }
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

    public BSTreadTransfInterpolator(BinaryReader r, Header h) : base(r, h) {
        TreadTransforms = r.ReadL32FArray(r => new BSTreadTransform(r));
        Data = X<NiFloatData>.Ref(r);
    }
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

    public BSAnimNote(BinaryReader r, Header h) : base(r, h) {
        Type = (AnimNoteType)r.ReadUInt32();
        Time = r.ReadSingle();
        if (Type == 1) Arm = r.ReadUInt32();
        if (Type == 2) {
            Gain = r.ReadSingle();
            State = r.ReadUInt32();
        }
    }
}

/// <summary>
/// Bethesda-specific object.
/// </summary>
public class BSAnimNotes : NiObject {
    public int?[] AnimNotes;                            // BSAnimNote objects.

    public BSAnimNotes(BinaryReader r, Header h) : base(r, h) {
        AnimNotes = r.ReadL16FArray(X<BSAnimNote>.Ref);
    }
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

    public bhkLiquidAction(BinaryReader r, Header h) : base(r, h) {
        UserData = r.ReadUInt32();
        UnknownInt2 = r.ReadInt32();
        UnknownInt3 = r.ReadInt32();
        InitialStickForce = r.ReadSingle();
        StickStrength = r.ReadSingle();
        NeighborDistance = r.ReadSingle();
        NeighborStrength = r.ReadSingle();
    }
}

/// <summary>
/// Culling modes for multi bound nodes.
/// </summary>
public enum BSCPCullingType : uint {
    BSCP_CULL_NORMAL = 0,           // Normal
    BSCP_CULL_ALLPASS = 1,          // All Pass
    BSCP_CULL_ALLFAIL = 2,          // All Fail
    BSCP_CULL_IGNOREMULTIBOUNDS = 3,// Ignore Multi Bounds
    BSCP_CULL_FORCEMULTIBOUNDSNOUPDATE = 4 // Force Multi Bounds No Update
}

/// <summary>
/// Bethesda-specific node.
/// </summary>
public class BSMultiBoundNode : NiNode {
    public int? MultiBound;
    public BSCPCullingType CullingMode;

    public BSMultiBoundNode(BinaryReader r, Header h) : base(r, h) {
        MultiBound = X<BSMultiBound>.Ref(r);
        if ((h.UV2 >= 83)) CullingMode = (BSCPCullingType)r.ReadUInt32();
    }
}

/// <summary>
/// Bethesda-specific object.
/// </summary>
public class BSMultiBound : NiObject {
    public int? Data;

    public BSMultiBound(BinaryReader r, Header h) : base(r, h) {
        Data = X<BSMultiBoundData>.Ref(r);
    }
}

/// <summary>
/// Abstract base type for bounding data.
/// </summary>
public class BSMultiBoundData(BinaryReader r, Header h) : NiObject(r, h) {
}

/// <summary>
/// Oriented bounding box.
/// </summary>
public class BSMultiBoundOBB : BSMultiBoundData {
    public Vector3 Center;                              // Center of the box.
    public Vector3 Size;                                // Size of the box along each axis.
    public Matrix3x3 Rotation;                          // Rotation of the bounding box.

    public BSMultiBoundOBB(BinaryReader r, Header h) : base(r, h) {
        Center = r.ReadVector3();
        Size = r.ReadVector3();
        Rotation = r.ReadMatrix3x3();
    }
}

/// <summary>
/// Bethesda-specific object.
/// </summary>
public class BSMultiBoundSphere : BSMultiBoundData {
    public Vector3 Center;
    public float Radius;

    public BSMultiBoundSphere(BinaryReader r, Header h) : base(r, h) {
        Center = r.ReadVector3();
        Radius = r.ReadSingle();
    }
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
public class BSGeometrySegmentData {
    public byte Flags;
    public uint Index;                                  // Index = previous Index + previous Num Tris in Segment * 3
    public uint NumTrisinSegment;                       // The number of triangles belonging to this segment
    public uint StartIndex;
    public uint NumPrimitives;
    public uint ParentArrayIndex;
    public BSGeometrySubSegment[] SubSegment;

    public BSGeometrySegmentData(BinaryReader r, Header h) {
        if ((h.UV2 < 130)) NumTrisinSegment = r.ReadUInt32();
        if ((h.UV2 == 130)) {
            StartIndex = r.ReadUInt32();
            NumPrimitives = r.ReadUInt32();
            ParentArrayIndex = r.ReadUInt32();
            SubSegment = r.ReadL32FArray(r => new BSGeometrySubSegment(r));
        }
    }
}

/// <summary>
/// Bethesda-specific AV object.
/// </summary>
public class BSSegmentedTriShape : NiTriShape {
    public BSGeometrySegmentData[] Segment;             // Configuration of each segment

    public BSSegmentedTriShape(BinaryReader r, Header h) : base(r, h) {
        Segment = r.ReadL32FArray(r => new BSGeometrySegmentData(r, h));
    }
}

/// <summary>
/// Bethesda-specific object.
/// </summary>
public class BSMultiBoundAABB : BSMultiBoundData {
    public Vector3 Position;                            // Position of the AABB's center
    public Vector3 Extent;                              // Extent of the AABB in all directions

    public BSMultiBoundAABB(BinaryReader r, Header h) : base(r, h) {
        Position = r.ReadVector3();
        Extent = r.ReadVector3();
    }
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

public class AdditionalDataBlock {
    public int BlockSize;                               // Size of Block
    public int[] BlockOffsets;
    public int NumData;
    public int[] DataSizes;
    public byte[][] Data;

    public AdditionalDataBlock(BinaryReader r) {
        HasData = r.ReadBool32();
        if (HasData) {
            BlockSize = r.ReadInt32();
            BlockOffsets = r.ReadL32PArray<uint>("i");
            NumData = r.ReadInt32();
            DataSizes = r.ReadPArray<uint>("i", NumData);
            Data = r.ReadFArray(k => r.ReadBytes(NumData), BlockSize);
        }
    }
}

public class BSPackedAdditionalDataBlock {
    public int NumTotalBytes;                           // Total number of bytes (over all channels and all elements, equals num total bytes per element times num vertices).
    public int[] BlockOffsets;                          // Block offsets in the data? Usually equal to zero.
    public int[] AtomSizes;                             // The sum of all of these equal num total bytes per element, so this probably describes how each data element breaks down into smaller chunks (i.e. atoms).
    public byte[] Data;
    public int UnknownInt1;
    public int NumTotalBytesPerElement;                 // Unsure, but this seems to correspond again to the number of total bytes per element.

    public BSPackedAdditionalDataBlock(BinaryReader r) {
        HasData = r.ReadBool32();
        if (HasData) {
            NumTotalBytes = r.ReadInt32();
            BlockOffsets = r.ReadL32PArray<uint>("i");
            AtomSizes = r.ReadL32PArray<uint>("i");
            Data = r.ReadBytes(NumTotalBytes);
        }
        UnknownInt1 = r.ReadInt32();
        NumTotalBytesPerElement = r.ReadInt32();
    }
}

public class NiAdditionalGeometryData : AbstractAdditionalGeometryData {
    public ushort NumVertices;                          // Number of vertices
    public AdditionalDataInfo[] BlockInfos;             // Number of additional data blocks
    public AdditionalDataBlock[] Blocks;                // Number of additional data blocks

    public NiAdditionalGeometryData(BinaryReader r, Header h) : base(r, h) {
        NumVertices = r.ReadUInt16();
        BlockInfos = r.ReadL32FArray(r => new AdditionalDataInfo(r));
        Blocks = r.ReadL32FArray(r => new AdditionalDataBlock(r));
    }
}

public class BSPackedAdditionalGeometryData : AbstractAdditionalGeometryData {
    public ushort NumVertices;
    public AdditionalDataInfo[] BlockInfos;             // Number of additional data blocks
    public BSPackedAdditionalDataBlock[] Blocks;        // Number of additional data blocks

    public BSPackedAdditionalGeometryData(BinaryReader r, Header h) : base(r, h) {
        NumVertices = r.ReadUInt16();
        BlockInfos = r.ReadL32FArray(r => new AdditionalDataInfo(r));
        Blocks = r.ReadL32FArray(r => new BSPackedAdditionalDataBlock(r));
    }
}

/// <summary>
/// Bethesda-specific extra data.
/// </summary>
public class BSWArray : NiExtraData {
    public int[] Items;

    public BSWArray(BinaryReader r, Header h) : base(r, h) {
        Items = r.ReadL32PArray<uint>("i");
    }
}

/// <summary>
/// Bethesda-specific Havok serializable.
/// </summary>
public class bhkAabbPhantom : bhkShapePhantom {
    public byte[] Unused;
    public Vector4 AABBMin;
    public Vector4 AABBMax;

    public bhkAabbPhantom(BinaryReader r, Header h) : base(r, h) {
        Unused = r.ReadBytes(8);
        AABBMin = r.ReadVector4();
        AABBMax = r.ReadVector4();
    }
}

/// <summary>
/// Bethesda-specific time controller.
/// </summary>
public class BSFrustumFOVController(BinaryReader r, Header h) : NiFloatInterpController(r, h) {
}

/// <summary>
/// Bethesda-Specific node.
/// </summary>
public class BSDebrisNode(BinaryReader r, Header h) : BSRangeNode(r, h) {
}

/// <summary>
/// A breakable constraint.
/// </summary>
public class bhkBreakableConstraint : bhkConstraint {
    public ConstraintData ConstraintData;               // Constraint within constraint.
    public float Threshold;                             // Amount of force to break the rigid bodies apart?
    public bool RemoveWhenBroken = 0;                   // No: Constraint stays active. Yes: Constraint gets removed when breaking threshold is exceeded.

    public bhkBreakableConstraint(BinaryReader r, Header h) : base(r, h) {
        ConstraintData = new ConstraintData(r);
        Threshold = r.ReadSingle();
        RemoveWhenBroken = r.ReadBool32();
    }
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

    public bhkOrientHingedBodyAction(BinaryReader r, Header h) : base(r, h) {
        Body = X<bhkRigidBody>.Ptr(r);
        UnknownInt1 = r.ReadUInt32();
        UnknownInt2 = r.ReadUInt32();
        Unused1 = r.ReadBytes(8);
        HingeAxisLS = r.ReadVector4();
        ForwardLS = r.ReadVector4();
        Strength = r.ReadSingle();
        Damping = r.ReadSingle();
        Unused2 = r.ReadBytes(8);
    }
}

/// <summary>
/// Found in Fallout 3 .psa files, extra ragdoll info for NPCs/creatures. (usually idleanims\deathposes.psa)
/// Defines different kill poses. The game selects the pose randomly and applies it to a skeleton immediately upon ragdolling.
/// Poses can be previewed in GECK Object Window-Actor Data-Ragdoll and selecting Pose Matching tab.
/// </summary>
public class bhkPoseArray : NiObject {
    public string[] Bones;
    public BonePose[] Poses;

    public bhkPoseArray(BinaryReader r, Header h) : base(r, h) {
        Bones = r.ReadL32FArray(r => Y.String(r));
        Poses = r.ReadL32FArray(r => new BonePose(r));
    }
}

/// <summary>
/// Found in Fallout 3, more ragdoll info?  (meshes\ragdollconstraint\*.rdt)
/// </summary>
public class bhkRagdollTemplate : NiExtraData {
    public int?[] Bones;

    public bhkRagdollTemplate(BinaryReader r, Header h) : base(r, h) {
        Bones = r.ReadL32FArray(X<NiObject>.Ref);
    }
}

/// <summary>
/// Data for bhkRagdollTemplate
/// </summary>
public class bhkRagdollTemplateData : NiObject {
    public string Name;
    public float Mass = 9.0f;
    public float Restitution = 0.8f;
    public float Friction = 0.3f;
    public float Radius = 1.0f;
    public HavokMaterial Material;
    public ConstraintData[] Constraint;

    public bhkRagdollTemplateData(BinaryReader r, Header h) : base(r, h) {
        Name = Y.String(r);
        Mass = r.ReadSingle();
        Restitution = r.ReadSingle();
        Friction = r.ReadSingle();
        Radius = r.ReadSingle();
        Material = new HavokMaterial(r, h);
        Constraint = r.ReadL32FArray(r => new ConstraintData(r));
    }
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
    public CloningBehavior CloningBehavior = CLONING_SHARE;
    public Region[] Regions;                            // The regions in the mesh. Regions can be used to mark off submeshes which are independent draw calls.
    public ComponentFormat[] ComponentFormats;          // The format of each component in this data stream.
    public byte[] Data;
    public bool Streamable = 1;

    public NiDataStream(BinaryReader r, Header h) : base(r, h) {
        Usage = (DataStreamUsage)r.ReadUInt32();
        Access = (DataStreamAccess)r.ReadUInt32();
        NumBytes = r.ReadUInt32();
        CloningBehavior = (CloningBehavior)r.ReadUInt32();
        Regions = r.ReadL32FArray(r => new Region(r));
        ComponentFormats = r.ReadL32FArray(r => (ComponentFormat)r.ReadL32UInt32());
        Data = r.ReadBytes(NumBytes);
        Streamable = r.ReadBool32();
    }
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
    public ushort[] SubmeshToRegionMap = r.ReadL16PArray<ushort>("H"); // A lookup table that maps submeshes to regions.
    public SemanticData[] ComponentSemantics = r.ReadL32FArray(r => new SemanticData(r)); // Describes the semantic of each component.
}

/// <summary>
/// An object that can be rendered.
/// </summary>
public abstract class NiRenderObject : NiAVObject {
    public MaterialData MaterialData;                   // Per-material data.

    public NiRenderObject(BinaryReader r, Header h) : base(r, h) {
        MaterialData = new MaterialData(r, h);
    }
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
    SYNC_PHYSICS_COMPLETED = 0x8060,// Synchronize when a physics simulation step has produced results.
    SYNC_REFLECTIONS = 0x8070       // Synchronize after all data necessary to calculate reflections is ready.
}

/// <summary>
/// Base class for mesh modifiers.
/// </summary>
public abstract class NiMeshModifier : NiObject {
    public SyncPoint[] SubmitPoints;                    // The sync points supported by this mesh modifier for SubmitTasks.
    public SyncPoint[] CompletePoints;                  // The sync points supported by this mesh modifier for CompleteTasks.

    public NiMeshModifier(BinaryReader r, Header h) : base(r, h) {
        SubmitPoints = r.ReadL32FArray(r => (SyncPoint)r.ReadL32UInt16());
        CompletePoints = r.ReadL32FArray(r => (SyncPoint)r.ReadL32UInt16());
    }
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
    public short[] UnknownShorts = r.ReadPArray<short>("h", 10);
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

    public NiMesh(BinaryReader r, Header h) : base(r, h) {
        PrimitiveType = (MeshPrimitiveType)r.ReadUInt32();
        if ((h.UV == 15)) {
            Unknown51 = r.ReadInt32();
            Unknown52 = r.ReadInt32();
            Unknown53 = r.ReadInt32();
            Unknown54 = r.ReadInt32();
            Unknown55 = r.ReadSingle();
            Unknown56 = r.ReadInt32();
        }
        NumSubmeshes = r.ReadUInt16();
        InstancingEnabled = r.ReadBool32();
        Bound = new NiBound(r);
        Datastreams = r.ReadL32FArray(r => new DataStreamRef(r));
        Modifiers = r.ReadL32FArray(X<NiMeshModifier>.Ref);
        if ((h.UV == 15)) {
            Unknown100 = r.ReadByte();
            Unknown101 = r.ReadInt32();
            Unknown102 = r.ReadUInt32();
            Unknown103 = r.ReadPArray<float>("f", Unknown102);
            Unknown200 = r.ReadInt32();
            Unknown201 = r.ReadFArray(r => new ExtraMeshDataEpicMickey(r), Unknown200);
            Unknown250 = r.ReadInt32();
            Unknown251 = r.ReadPArray<uint>("i", Unknown250);
            Unknown300 = r.ReadInt32();
            Unknown301 = r.ReadInt16();
            Unknown302 = r.ReadInt32();
            Unknown303 = r.ReadBytes(Unknown302);
            Unknown350 = r.ReadInt32();
            Unknown351 = r.ReadFArray(r => new ExtraMeshDataEpicMickey2(r), Unknown350);
            Unknown400 = r.ReadInt32();
        }
    }
}

/// <summary>
/// Manipulates a mesh with the semantic MORPHWEIGHTS using an NiMorphMeshModifier.
/// </summary>
public class NiMorphWeightsController : NiInterpController {
    public uint Count;
    public int?[] Interpolators;
    public string[] TargetNames;

    public NiMorphWeightsController(BinaryReader r, Header h) : base(r, h) {
        Count = r.ReadUInt32();
        Interpolators = r.ReadL32FArray(X<NiInterpolator>.Ref);
        TargetNames = r.ReadL32FArray(r => Y.String(r));
    }
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

    public NiMorphMeshModifier(BinaryReader r, Header h) : base(r, h) {
        Flags = r.ReadByte();
        NumTargets = r.ReadUInt16();
        Elements = r.ReadL32FArray(r => new ElementReference(r));
    }
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

    public NiSkinningMeshModifier(BinaryReader r, Header h) : base(r, h) {
        Flags = r.ReadUInt16();
        SkeletonRoot = X<NiAVObject>.Ptr(r);
        SkeletonTransform = new NiTransform(r);
        NumBones = r.ReadUInt32();
        Bones = r.ReadFArray(X<NiAVObject>.Ptr, NumBones);
        BoneTransforms = r.ReadFArray(r => new NiTransform(r), NumBones);
        if ((Flags & 2)!=0) BoneBounds = r.ReadFArray(r => new NiBound(r), NumBones);
    }
}

/// <summary>
/// An instance of a hardware-instanced mesh in a scene graph.
/// </summary>
public class NiMeshHWInstance : NiAVObject {
    public int? MasterMesh;                             // The instanced mesh this object represents.
    public int? MeshModifier;

    public NiMeshHWInstance(BinaryReader r, Header h) : base(r, h) {
        MasterMesh = X<NiMesh>.Ref(r);
        MeshModifier = X<NiInstancingMeshModifier>.Ref(r);
    }
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

    public NiInstancingMeshModifier(BinaryReader r, Header h) : base(r, h) {
        HasInstanceNodes = r.ReadBool32();
        PerInstanceCulling = r.ReadBool32();
        HasStaticBounds = r.ReadBool32();
        AffectedMesh = X<NiMesh>.Ref(r);
        if (HasStaticBounds) Bound = new NiBound(r);
        if (HasInstanceNodes) InstanceNodes = r.ReadL32FArray(X<NiMeshHWInstance>.Ref);
    }
}

public class LODInfo(BinaryReader r) {
    public uint NumBones = r.ReadUInt32();
    public uint[] SkinIndices = r.ReadL32PArray<uint>("I");
}

/// <summary>
/// Defines the levels of detail for a given character and dictates the character's current LOD.
/// </summary>
public class NiSkinningLODController : NiTimeController {
    public uint CurrentLOD;
    public int?[] Bones;
    public int?[] Skins;
    public LODInfo[] LODs;

    public NiSkinningLODController(BinaryReader r, Header h) : base(r, h) {
        CurrentLOD = r.ReadUInt32();
        Bones = r.ReadL32FArray(X<NiNode>.Ref);
        Skins = r.ReadL32FArray(X<NiMesh>.Ref);
        LODs = r.ReadL32FArray(r => new LODInfo(r));
    }
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

    public NiPSParticleSystem(BinaryReader r, Header h) : base(r, h) {
        Simulator = X<NiPSSimulator>.Ref(r);
        Generator = X<NiPSBoundUpdater>.Ref(r);
        Emitters = r.ReadL32FArray(X<NiPSEmitter>.Ref);
        Spawners = r.ReadL32FArray(X<NiPSSpawner>.Ref);
        DeathSpawner = X<NiPSSpawner>.Ref(r);
        MaxNumParticles = r.ReadUInt32();
        HasColors = r.ReadBool32();
        HasRotations = r.ReadBool32();
        HasRotationAxes = r.ReadBool32();
        if (h.V >= 0x14060100) HasAnimatedTextures = r.ReadBool32();
        WorldSpace = r.ReadBool32();
        if (h.V >= 0x14060100) {
            NormalMethod = (AlignMethod)r.ReadUInt32();
            NormalDirection = r.ReadVector3();
            UpMethod = (AlignMethod)r.ReadUInt32();
            UpDirection = r.ReadVector3();
            LivingSpawner = X<NiPSSpawner>.Ref(r);
            SpawnRateKeys = r.ReadL8FArray(r => new PSSpawnRateKey(r));
            Pre-RPI = r.ReadBool32();
        }
    }
}

/// <summary>
/// Represents a particle system that uses mesh particles instead of sprite-based particles.
/// </summary>
public class NiPSMeshParticleSystem : NiPSParticleSystem {
    public int?[] MasterParticles;
    public uint PoolSize;
    public bool Auto-FillPools;

    public NiPSMeshParticleSystem(BinaryReader r, Header h) : base(r, h) {
        MasterParticles = r.ReadL32FArray(X<NiAVObject>.Ref);
        PoolSize = r.ReadUInt32();
        Auto-FillPools = r.ReadBool32();
    }
}

/// <summary>
/// A mesh modifier that uses particle system data to generate camera-facing quads.
/// </summary>
public class NiPSFacingQuadGenerator(BinaryReader r, Header h) : NiMeshModifier(r, h) {
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

    public NiPSAlignedQuadGenerator(BinaryReader r, Header h) : base(r, h) {
        ScaleAmountU = r.ReadSingle();
        ScaleLimitU = r.ReadSingle();
        ScaleRestU = r.ReadSingle();
        ScaleAmountV = r.ReadSingle();
        ScaleLimitV = r.ReadSingle();
        ScaleRestV = r.ReadSingle();
        CenterU = r.ReadSingle();
        CenterV = r.ReadSingle();
        UVScrolling = r.ReadBool32();
        NumFramesAcross = r.ReadUInt16();
        NumFramesDown = r.ReadUInt16();
        PingPong = r.ReadBool32();
        InitialFrame = r.ReadUInt16();
        InitialFrameVariation = r.ReadSingle();
        NumFrames = r.ReadUInt16();
        NumFramesVariation = r.ReadSingle();
        InitialTime = r.ReadSingle();
        FinalTime = r.ReadSingle();
    }
}

/// <summary>
/// The mesh modifier that performs all particle system simulation.
/// </summary>
public class NiPSSimulator : NiMeshModifier {
    public int?[] SimulationSteps;

    public NiPSSimulator(BinaryReader r, Header h) : base(r, h) {
        SimulationSteps = r.ReadL32FArray(X<NiPSSimulatorStep>.Ref);
    }
}

/// <summary>
/// Abstract base class for a single step in the particle system simulation process.  It has no seralized data.
/// </summary>
public abstract class NiPSSimulatorStep(BinaryReader r, Header h) : NiObject(r, h) {
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
    public Key<float>[] SizeKeys;                       // The particle size keys.
    public PSLoopBehavior SizeLoopBehavior;             // The loop behavior for the size keys.
    public Key<ByteColor4>[] ColorKeys;                 // The particle color keys.
    public PSLoopBehavior ColorLoopBehavior;            // The loop behavior for the color keys.
    public QuatKey<Quaternion>[] RotationKeys;          // The particle rotation keys.
    public PSLoopBehavior RotationLoopBehavior;         // The loop behavior for the rotation keys.
    public float GrowTime;                              // The the amount of time over which a particle's size is ramped from 0.0 to 1.0 in seconds
    public float ShrinkTime;                            // The the amount of time over which a particle's size is ramped from 1.0 to 0.0 in seconds
    public ushort GrowGeneration;                       // Specifies the particle generation to which the grow effect should be applied. This is usually generation 0, so that newly created particles will grow.
    public ushort ShrinkGeneration;                     // Specifies the particle generation to which the shrink effect should be applied. This is usually the highest supported generation for the particle system, so that particles will shrink immediately before getting killed.

    public NiPSSimulatorGeneralStep(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x14060100) SizeLoopBehavior = (PSLoopBehavior)r.ReadUInt32();
        ColorKeys = r.ReadL8FArray(r => new Key<ByteColor4>(r, Interpolation));
        if (h.V >= 0x14060100) {
            ColorLoopBehavior = (PSLoopBehavior)r.ReadUInt32();
            RotationKeys = r.ReadL8FArray(r => new QuatKey<Quaternion>(r, h));
            RotationLoopBehavior = (PSLoopBehavior)r.ReadUInt32();
        }
        GrowTime = r.ReadSingle();
        ShrinkTime = r.ReadSingle();
        GrowGeneration = r.ReadUInt16();
        ShrinkGeneration = r.ReadUInt16();
    }
}

/// <summary>
/// Encapsulates a floodgate kernel that simulates particle forces.
/// </summary>
public class NiPSSimulatorForcesStep : NiPSSimulatorStep {
    public int?[] Forces;                               // The forces affecting the particle system.

    public NiPSSimulatorForcesStep(BinaryReader r, Header h) : base(r, h) {
        Forces = r.ReadL32FArray(X<NiPSForce>.Ref);
    }
}

/// <summary>
/// Encapsulates a floodgate kernel that simulates particle colliders.
/// </summary>
public class NiPSSimulatorCollidersStep : NiPSSimulatorStep {
    public int?[] Colliders;                            // The colliders affecting the particle system.

    public NiPSSimulatorCollidersStep(BinaryReader r, Header h) : base(r, h) {
        Colliders = r.ReadL32FArray(X<NiPSCollider>.Ref);
    }
}

/// <summary>
/// Encapsulates a floodgate kernel that updates mesh particle alignment and transforms.
/// </summary>
public class NiPSSimulatorMeshAlignStep : NiPSSimulatorStep {
    public QuatKey<Quaternion>[] RotationKeys;          // The particle rotation keys.
    public PSLoopBehavior RotationLoopBehavior;         // The loop behavior for the rotation keys.

    public NiPSSimulatorMeshAlignStep(BinaryReader r, Header h) : base(r, h) {
        RotationKeys = r.ReadL8FArray(r => new QuatKey<Quaternion>(r, h));
        RotationLoopBehavior = (PSLoopBehavior)r.ReadUInt32();
    }
}

/// <summary>
/// Encapsulates a floodgate kernel that updates particle positions and ages. As indicated by its name, this step should be attached last in the NiPSSimulator mesh modifier.
/// </summary>
public class NiPSSimulatorFinalStep(BinaryReader r, Header h) : NiPSSimulatorStep(r, h) {
}

/// <summary>
/// Updates the bounding volume for an NiPSParticleSystem object.
/// </summary>
public class NiPSBoundUpdater : NiObject {
    public ushort UpdateSkip;                           // Number of particle bounds to skip updating every frame. Higher = more updates each frame.

    public NiPSBoundUpdater(BinaryReader r, Header h) : base(r, h) {
        UpdateSkip = r.ReadUInt16();
    }
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

    public NiPSForce(BinaryReader r, Header h) : base(r, h) {
        Name = Y.String(r);
        Type = (PSForceType)r.ReadUInt32();
        Active = r.ReadBool32();
    }
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

    public NiPSDragForce(BinaryReader r, Header h) : base(r, h) {
        DragAxis = r.ReadVector3();
        Percentage = r.ReadSingle();
        Range = r.ReadSingle();
        RangeFalloff = r.ReadSingle();
        DragObject = X<NiAVObject>.Ptr(r);
    }
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

    public NiPSGravityForce(BinaryReader r, Header h) : base(r, h) {
        GravityAxis = r.ReadVector3();
        Decay = r.ReadSingle();
        Strength = r.ReadSingle();
        ForceType = (ForceType)r.ReadUInt32();
        Turbulence = r.ReadSingle();
        TurbulenceScale = r.ReadSingle();
        GravityObject = X<NiAVObject>.Ptr(r);
    }
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

    public NiPSBombForce(BinaryReader r, Header h) : base(r, h) {
        BombAxis = r.ReadVector3();
        Decay = r.ReadSingle();
        DeltaV = r.ReadSingle();
        DecayType = (DecayType)r.ReadUInt32();
        SymmetryType = (SymmetryType)r.ReadUInt32();
        BombObject = X<NiAVObject>.Ptr(r);
    }
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

    public NiPSEmitter(BinaryReader r, Header h) : base(r, h) {
        Name = Y.String(r);
        Speed = r.ReadSingle();
        SpeedVar = r.ReadSingle();
        if (h.V >= 0x14060100) SpeedFlipRatio = r.ReadSingle();
        Declination = r.ReadSingle();
        DeclinationVar = r.ReadSingle();
        PlanarAngle = r.ReadSingle();
        PlanarAngleVar = r.ReadSingle();
        if (h.V <= 0x14060000) Color = new Color4Byte(r);
        Size = r.ReadSingle();
        SizeVar = r.ReadSingle();
        Lifespan = r.ReadSingle();
        LifespanVar = r.ReadSingle();
        RotationAngle = r.ReadSingle();
        RotationAngleVar = r.ReadSingle();
        RotationSpeed = r.ReadSingle();
        RotationSpeedVar = r.ReadSingle();
        RotationAxis = r.ReadVector3();
        RandomRotSpeedSign = r.ReadBool32();
        RandomRotAxis = r.ReadBool32();
        if (h.V >= 0x1E000000 && h.V <= 0x1E000001) Unknown = r.ReadBool32();
    }
}

/// <summary>
/// Abstract base class for particle emitters that emit particles from a volume.
/// </summary>
public abstract class NiPSVolumeEmitter : NiPSEmitter {
    public int? EmitterObject;

    public NiPSVolumeEmitter(BinaryReader r, Header h) : base(r, h) {
        EmitterObject = X<NiAVObject>.Ptr(r);
    }
}

/// <summary>
/// A particle emitter that emits particles from a rectangular volume.
/// </summary>
public class NiPSBoxEmitter : NiPSVolumeEmitter {
    public float EmitterWidth;
    public float EmitterHeight;
    public float EmitterDepth;

    public NiPSBoxEmitter(BinaryReader r, Header h) : base(r, h) {
        EmitterWidth = r.ReadSingle();
        EmitterHeight = r.ReadSingle();
        EmitterDepth = r.ReadSingle();
    }
}

/// <summary>
/// A particle emitter that emits particles from a spherical volume.
/// </summary>
public class NiPSSphereEmitter : NiPSVolumeEmitter {
    public float EmitterRadius;

    public NiPSSphereEmitter(BinaryReader r, Header h) : base(r, h) {
        EmitterRadius = r.ReadSingle();
    }
}

/// <summary>
/// A particle emitter that emits particles from a cylindrical volume.
/// </summary>
public class NiPSCylinderEmitter : NiPSVolumeEmitter {
    public float EmitterRadius;
    public float EmitterHeight;

    public NiPSCylinderEmitter(BinaryReader r, Header h) : base(r, h) {
        EmitterRadius = r.ReadSingle();
        EmitterHeight = r.ReadSingle();
    }
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

    public NiPSMeshEmitter(BinaryReader r, Header h) : base(r, h) {
        MeshEmitters = r.ReadL32FArray(X<NiMesh>.Ptr);
        if (h.V <= 0x14060000) EmitAxis = r.ReadVector3();
        if (h.V >= 0x14060100) EmitterObject = X<NiAVObject>.Ptr(r);
        MeshEmissionType = (EmitFrom)r.ReadUInt32();
        InitialVelocityType = (VelocityType)r.ReadUInt32();
    }
}

/// <summary>
/// Abstract base class for all particle emitter time controllers.
/// </summary>
public abstract class NiPSEmitterCtlr : NiSingleInterpController {
    public string EmitterName;

    public NiPSEmitterCtlr(BinaryReader r, Header h) : base(r, h) {
        EmitterName = Y.String(r);
    }
}

/// <summary>
/// Abstract base class for controllers that animate a floating point value on an NiPSEmitter object.
/// </summary>
public abstract class NiPSEmitterFloatCtlr(BinaryReader r, Header h) : NiPSEmitterCtlr(r, h) {
}

/// <summary>
/// Animates particle emission and birth rate.
/// </summary>
public class NiPSEmitParticlesCtlr : NiPSEmitterCtlr {
    public int? EmitterActiveInterpolator;

    public NiPSEmitParticlesCtlr(BinaryReader r, Header h) : base(r, h) {
        EmitterActiveInterpolator = X<NiInterpolator>.Ref(r);
    }
}

/// <summary>
/// Abstract base class for all particle force time controllers.
/// </summary>
public abstract class NiPSForceCtlr : NiSingleInterpController {
    public string ForceName;

    public NiPSForceCtlr(BinaryReader r, Header h) : base(r, h) {
        ForceName = Y.String(r);
    }
}

/// <summary>
/// Abstract base class for controllers that animate a Boolean value on an NiPSForce object.
/// </summary>
public abstract class NiPSForceBoolCtlr(BinaryReader r, Header h) : NiPSForceCtlr(r, h) {
}

/// <summary>
/// Abstract base class for controllers that animate a floating point value on an NiPSForce object.
/// </summary>
public abstract class NiPSForceFloatCtlr(BinaryReader r, Header h) : NiPSForceCtlr(r, h) {
}

/// <summary>
/// Animates whether or not an NiPSForce object is active.
/// </summary>
public class NiPSForceActiveCtlr(BinaryReader r, Header h) : NiPSForceBoolCtlr(r, h) {
}

/// <summary>
/// Animates the strength value of an NiPSGravityForce object.
/// </summary>
public class NiPSGravityStrengthCtlr(BinaryReader r, Header h) : NiPSForceFloatCtlr(r, h) {
}

/// <summary>
/// Animates the speed value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterSpeedCtlr(BinaryReader r, Header h) : NiPSEmitterFloatCtlr(r, h) {
}

/// <summary>
/// Animates the size value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterRadiusCtlr(BinaryReader r, Header h) : NiPSEmitterFloatCtlr(r, h) {
}

/// <summary>
/// Animates the declination value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterDeclinationCtlr(BinaryReader r, Header h) : NiPSEmitterFloatCtlr(r, h) {
}

/// <summary>
/// Animates the declination variation value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterDeclinationVarCtlr(BinaryReader r, Header h) : NiPSEmitterFloatCtlr(r, h) {
}

/// <summary>
/// Animates the planar angle value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterPlanarAngleCtlr(BinaryReader r, Header h) : NiPSEmitterFloatCtlr(r, h) {
}

/// <summary>
/// Animates the planar angle variation value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterPlanarAngleVarCtlr(BinaryReader r, Header h) : NiPSEmitterFloatCtlr(r, h) {
}

/// <summary>
/// Animates the rotation angle value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterRotAngleCtlr(BinaryReader r, Header h) : NiPSEmitterFloatCtlr(r, h) {
}

/// <summary>
/// Animates the rotation angle variation value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterRotAngleVarCtlr(BinaryReader r, Header h) : NiPSEmitterFloatCtlr(r, h) {
}

/// <summary>
/// Animates the rotation speed value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterRotSpeedCtlr(BinaryReader r, Header h) : NiPSEmitterFloatCtlr(r, h) {
}

/// <summary>
/// Animates the rotation speed variation value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterRotSpeedVarCtlr(BinaryReader r, Header h) : NiPSEmitterFloatCtlr(r, h) {
}

/// <summary>
/// Animates the lifespan value on an NiPSEmitter object.
/// </summary>
public class NiPSEmitterLifeSpanCtlr(BinaryReader r, Header h) : NiPSEmitterFloatCtlr(r, h) {
}

/// <summary>
/// Calls ResetParticleSystem on an NiPSParticleSystem target upon looping.
/// </summary>
public class NiPSResetOnLoopCtlr(BinaryReader r, Header h) : NiTimeController(r, h) {
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

    public NiPSCollider(BinaryReader r, Header h) : base(r, h) {
        Spawner = X<NiPSSpawner>.Ref(r);
        Type = (ColliderType)r.ReadUInt32();
        Active = r.ReadBool32();
        Bounce = r.ReadSingle();
        SpawnonCollide = r.ReadBool32();
        DieonCollide = r.ReadBool32();
    }
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

    public NiPSPlanarCollider(BinaryReader r, Header h) : base(r, h) {
        Width = r.ReadSingle();
        Height = r.ReadSingle();
        XAxis = r.ReadVector3();
        YAxis = r.ReadVector3();
        ColliderObject = X<NiAVObject>.Ptr(r);
    }
}

/// <summary>
/// A spherical collider for particles.
/// </summary>
public class NiPSSphericalCollider : NiPSCollider {
    public float Radius;
    public int? ColliderObject;

    public NiPSSphericalCollider(BinaryReader r, Header h) : base(r, h) {
        Radius = r.ReadSingle();
        ColliderObject = X<NiAVObject>.Ptr(r);
    }
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

    public NiPSSpawner(BinaryReader r, Header h) : base(r, h) {
        if (h.V >= 0x14060100) MasterParticleSystem = X<NiPSParticleSystem>.Ptr(r);
        PercentageSpawned = r.ReadSingle();
        if (h.V >= 0x14060100) SpawnSpeedFactor = r.ReadSingle();
        SpawnSpeedFactorVar = r.ReadSingle();
        SpawnDirChaos = r.ReadSingle();
        LifeSpan = r.ReadSingle();
        LifeSpanVar = r.ReadSingle();
        NumSpawnGenerations = r.ReadUInt16();
        MintoSpawn = r.ReadUInt32();
        MaxtoSpawn = r.ReadUInt32();
    }
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

    public NiEvaluator(BinaryReader r, Header h) : base(r, h) {
        NodeName = Y.String(r);
        PropertyType = Y.String(r);
        ControllerType = Y.String(r);
        ControllerID = Y.String(r);
        InterpolatorID = Y.String(r);
        ChannelTypes = r.ReadBytes(4);
    }
}

public abstract class NiKeyBasedEvaluator(BinaryReader r, Header h) : NiEvaluator(r, h) {
}

public class NiBoolEvaluator : NiKeyBasedEvaluator {
    public int? Data;

    public NiBoolEvaluator(BinaryReader r, Header h) : base(r, h) {
        Data = X<NiBoolData>.Ref(r);
    }
}

public class NiBoolTimelineEvaluator(BinaryReader r, Header h) : NiBoolEvaluator(r, h) {
}

public class NiColorEvaluator : NiKeyBasedEvaluator {
    public int? Data;

    public NiColorEvaluator(BinaryReader r, Header h) : base(r, h) {
        Data = X<NiColorData>.Ref(r);
    }
}

public class NiFloatEvaluator : NiKeyBasedEvaluator {
    public int? Data;

    public NiFloatEvaluator(BinaryReader r, Header h) : base(r, h) {
        Data = X<NiFloatData>.Ref(r);
    }
}

public class NiPoint3Evaluator : NiKeyBasedEvaluator {
    public int? Data;

    public NiPoint3Evaluator(BinaryReader r, Header h) : base(r, h) {
        Data = X<NiPosData>.Ref(r);
    }
}

public class NiQuaternionEvaluator : NiKeyBasedEvaluator {
    public int? Data;

    public NiQuaternionEvaluator(BinaryReader r, Header h) : base(r, h) {
        Data = X<NiRotData>.Ref(r);
    }
}

public class NiTransformEvaluator : NiKeyBasedEvaluator {
    public NiQuatTransform Value;
    public int? Data;

    public NiTransformEvaluator(BinaryReader r, Header h) : base(r, h) {
        Value = new NiQuatTransform(r, h);
        Data = X<NiTransformData>.Ref(r);
    }
}

public class NiConstBoolEvaluator : NiEvaluator {
    public float Value = -3.402823466e+38f;

    public NiConstBoolEvaluator(BinaryReader r, Header h) : base(r, h) {
        Value = r.ReadSingle();
    }
}

public class NiConstColorEvaluator : NiEvaluator {
    public Color4 Value = new Color3(-3.402823466e+38, -3.402823466e+38, -3.402823466e+38, -3.402823466e+38);

    public NiConstColorEvaluator(BinaryReader r, Header h) : base(r, h) {
        Value = new Color4(r);
    }
}

public class NiConstFloatEvaluator : NiEvaluator {
    public float Value = -3.402823466e+38f;

    public NiConstFloatEvaluator(BinaryReader r, Header h) : base(r, h) {
        Value = r.ReadSingle();
    }
}

public class NiConstPoint3Evaluator : NiEvaluator {
    public Vector3 Value = new Vector3(-3.402823466e+38, -3.402823466e+38, -3.402823466e+38);

    public NiConstPoint3Evaluator(BinaryReader r, Header h) : base(r, h) {
        Value = r.ReadVector3();
    }
}

public class NiConstQuaternionEvaluator : NiEvaluator {
    public Quaternion Value = -3.402823466e+38, -3.402823466e+38, -3.402823466e+38, -3.402823466e+38;

    public NiConstQuaternionEvaluator(BinaryReader r, Header h) : base(r, h) {
        Value = r.ReadQuaternion();
    }
}

public class NiConstTransformEvaluator : NiEvaluator {
    public NiQuatTransform Value;

    public NiConstTransformEvaluator(BinaryReader r, Header h) : base(r, h) {
        Value = new NiQuatTransform(r, h);
    }
}

public class NiBSplineEvaluator : NiEvaluator {
    public float StartTime = 3.402823466e+38f;
    public float EndTime = -3.402823466e+38f;
    public int? Data;
    public int? BasisData;

    public NiBSplineEvaluator(BinaryReader r, Header h) : base(r, h) {
        StartTime = r.ReadSingle();
        EndTime = r.ReadSingle();
        Data = X<NiBSplineData>.Ref(r);
        BasisData = X<NiBSplineBasisData>.Ref(r);
    }
}

public class NiBSplineColorEvaluator : NiBSplineEvaluator {
    public uint Handle = 0xFFFF;                        // Handle into the data. (USHRT_MAX for invalid handle.)

    public NiBSplineColorEvaluator(BinaryReader r, Header h) : base(r, h) {
        Handle = r.ReadUInt32();
    }
}

public class NiBSplineCompColorEvaluator : NiBSplineColorEvaluator {
    public float Offset = 3.402823466e+38f;
    public float HalfRange = 3.402823466e+38f;

    public NiBSplineCompColorEvaluator(BinaryReader r, Header h) : base(r, h) {
        Offset = r.ReadSingle();
        HalfRange = r.ReadSingle();
    }
}

public class NiBSplineFloatEvaluator : NiBSplineEvaluator {
    public uint Handle = 0xFFFF;                        // Handle into the data. (USHRT_MAX for invalid handle.)

    public NiBSplineFloatEvaluator(BinaryReader r, Header h) : base(r, h) {
        Handle = r.ReadUInt32();
    }
}

public class NiBSplineCompFloatEvaluator : NiBSplineFloatEvaluator {
    public float Offset = 3.402823466e+38f;
    public float HalfRange = 3.402823466e+38f;

    public NiBSplineCompFloatEvaluator(BinaryReader r, Header h) : base(r, h) {
        Offset = r.ReadSingle();
        HalfRange = r.ReadSingle();
    }
}

public class NiBSplinePoint3Evaluator : NiBSplineEvaluator {
    public uint Handle = 0xFFFF;                        // Handle into the data. (USHRT_MAX for invalid handle.)

    public NiBSplinePoint3Evaluator(BinaryReader r, Header h) : base(r, h) {
        Handle = r.ReadUInt32();
    }
}

public class NiBSplineCompPoint3Evaluator : NiBSplinePoint3Evaluator {
    public float Offset = 3.402823466e+38f;
    public float HalfRange = 3.402823466e+38f;

    public NiBSplineCompPoint3Evaluator(BinaryReader r, Header h) : base(r, h) {
        Offset = r.ReadSingle();
        HalfRange = r.ReadSingle();
    }
}

public class NiBSplineTransformEvaluator : NiBSplineEvaluator {
    public NiQuatTransform Transform;
    public uint TranslationHandle = 0xFFFF;             // Handle into the translation data. (USHRT_MAX for invalid handle.)
    public uint RotationHandle = 0xFFFF;                // Handle into the rotation data. (USHRT_MAX for invalid handle.)
    public uint ScaleHandle = 0xFFFF;                   // Handle into the scale data. (USHRT_MAX for invalid handle.)

    public NiBSplineTransformEvaluator(BinaryReader r, Header h) : base(r, h) {
        Transform = new NiQuatTransform(r, h);
        TranslationHandle = r.ReadUInt32();
        RotationHandle = r.ReadUInt32();
        ScaleHandle = r.ReadUInt32();
    }
}

public class NiBSplineCompTransformEvaluator : NiBSplineTransformEvaluator {
    public float TranslationOffset = 3.402823466e+38f;
    public float TranslationHalfRange = 3.402823466e+38f;
    public float RotationOffset = 3.402823466e+38f;
    public float RotationHalfRange = 3.402823466e+38f;
    public float ScaleOffset = 3.402823466e+38f;
    public float ScaleHalfRange = 3.402823466e+38f;

    public NiBSplineCompTransformEvaluator(BinaryReader r, Header h) : base(r, h) {
        TranslationOffset = r.ReadSingle();
        TranslationHalfRange = r.ReadSingle();
        RotationOffset = r.ReadSingle();
        RotationHalfRange = r.ReadSingle();
        ScaleOffset = r.ReadSingle();
        ScaleHalfRange = r.ReadSingle();
    }
}

public class NiLookAtEvaluator : NiEvaluator {
    public LookAtFlags Flags;
    public string LookAtName;
    public string DrivenName;
    public int? Interpolator:Translation;
    public int? Interpolator:Roll;
    public int? Interpolator:Scale;

    public NiLookAtEvaluator(BinaryReader r, Header h) : base(r, h) {
        Flags = (LookAtFlags)r.ReadUInt16();
        LookAtName = Y.String(r);
        DrivenName = Y.String(r);
        Interpolator:Translation = X<NiPoint3Interpolator>.Ref(r);
        Interpolator:Roll = X<NiFloatInterpolator>.Ref(r);
        Interpolator:Scale = X<NiFloatInterpolator>.Ref(r);
    }
}

public class NiPathEvaluator : NiKeyBasedEvaluator {
    public PathFlags Flags = 3;
    public int BankDir = 1;                             // -1 = Negative, 1 = Positive
    public float MaxBankAngle;                          // Max angle in radians.
    public float Smoothing;
    public short FollowAxis;                            // 0, 1, or 2 representing X, Y, or Z.
    public int? PathData;
    public int? PercentData;

    public NiPathEvaluator(BinaryReader r, Header h) : base(r, h) {
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
    public float Frequency = 1.0f;
    public string AccumRootName;                        // The name of the NiAVObject serving as the accumulation root. This is where all accumulated translations, scales, and rotations are applied.
    public AccumFlags AccumFlags = ACCUM_X_FRONT;

    public NiSequenceData(BinaryReader r, Header h) : base(r, h) {
        Name = Y.String(r);
        if (h.V <= 0x14050001) {
            NumControlledBlocks = r.ReadUInt32();
            ArrayGrowBy = r.ReadUInt32();
            ControlledBlocks = r.ReadFArray(r => new ControlledBlock(r, h), NumControlledBlocks);
        }
        if (h.V >= 0x14050002) Evaluators = r.ReadL32FArray(X<NiEvaluator>.Ref);
        TextKeys = X<NiTextKeyExtraData>.Ref(r);
        Duration = r.ReadSingle();
        CycleType = (CycleType)r.ReadUInt32();
        Frequency = r.ReadSingle();
        AccumRootName = Y.String(r);
        AccumFlags = (AccumFlags)r.ReadUInt32();
    }
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
    public float DepthBias = 0.98f;
    public ushort SizeHint;
    public float NearClippingDistance;
    public float FarClippingDistance;
    public float DirectionalLightFrustumWidth;

    public NiShadowGenerator(BinaryReader r, Header h) : base(r, h) {
        Name = Y.String(r);
        Flags = r.ReadUInt16();
        ShadowCasters = r.ReadL32FArray(X<NiNode>.Ref);
        ShadowReceivers = r.ReadL32FArray(X<NiNode>.Ref);
        Target = X<NiDynamicEffect>.Ptr(r);
        DepthBias = r.ReadSingle();
        SizeHint = r.ReadUInt16();
        if (h.V >= 0x14030007) {
            NearClippingDistance = r.ReadSingle();
            FarClippingDistance = r.ReadSingle();
            DirectionalLightFrustumWidth = r.ReadSingle();
        }
    }
}

public class NiFurSpringController : NiTimeController {
    public float UnknownFloat;
    public float UnknownFloat2;
    public int?[] Bones;                                // List of all armature bones.
    public int?[] Bones2;                               // List of all armature bones.

    public NiFurSpringController(BinaryReader r, Header h) : base(r, h) {
        UnknownFloat = r.ReadSingle();
        UnknownFloat2 = r.ReadSingle();
        Bones = r.ReadL32FArray(X<NiNode>.Ptr);
        Bones2 = r.ReadL32FArray(X<NiNode>.Ptr);
    }
}

public class CStreamableAssetData : NiObject {
    public int? Root;
    public byte[] UnknownBytes;

    public CStreamableAssetData(BinaryReader r, Header h) : base(r, h) {
        Root = X<NiNode>.Ref(r);
        UnknownBytes = r.ReadBytes(5);
    }
}

/// <summary>
/// Compressed collision mesh.
/// </summary>
public class bhkCompressedMeshShape : bhkShape {
    public int? Target;                                 // Points to root node?
    public uint UserData;                               // Unknown.
    public float Radius = 0.005f;                       // A shell that is added around the shape.
    public float UnknownFloat1;                         // Unknown.
    public Vector4 Scale = new Vector4(1.0, 1.0, 1.0, 0.0); // Scale
    public float RadiusCopy = 0.005f;                   // A shell that is added around the shape.
    public Vector4 ScaleCopy = new Vector4(1.0, 1.0, 1.0, 0.0); // Scale
    public int? Data;                                   // The collision mesh data.

    public bhkCompressedMeshShape(BinaryReader r, Header h) : base(r, h) {
        Target = X<NiAVObject>.Ptr(r);
        UserData = r.ReadUInt32();
        Radius = r.ReadSingle();
        UnknownFloat1 = r.ReadSingle();
        Scale = r.ReadVector4();
        RadiusCopy = r.ReadSingle();
        ScaleCopy = r.ReadVector4();
        Data = X<bhkCompressedMeshShapeData>.Ref(r);
    }
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

    public bhkCompressedMeshShapeData(BinaryReader r, Header h) : base(r, h) {
        BitsPerIndex = r.ReadUInt32();
        BitsPerWIndex = r.ReadUInt32();
        MaskWIndex = r.ReadUInt32();
        MaskIndex = r.ReadUInt32();
        Error = r.ReadSingle();
        BoundsMin = r.ReadVector4();
        BoundsMax = r.ReadVector4();
        WeldingType = r.ReadByte();
        MaterialType = r.ReadByte();
        Materials32 = r.ReadL32PArray<uint>("I");
        Materials16 = r.ReadL32PArray<uint>("I");
        Materials8 = r.ReadL32PArray<uint>("I");
        ChunkMaterials = r.ReadL32FArray(r => new bhkCMSDMaterial(r));
        NumNamedMaterials = r.ReadUInt32();
        ChunkTransforms = r.ReadL32FArray(r => new bhkCMSDTransform(r));
        BigVerts = r.ReadL32PArray<Vector4>("4f");
        BigTris = r.ReadL32FArray(r => new bhkCMSDBigTris(r));
        Chunks = r.ReadL32FArray(r => new bhkCMSDChunk(r));
        NumConvexPieceA = r.ReadUInt32();
    }
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
    public ushort RotationX = 4712;
    public ushort RotationY = 6283;
    public ushort RotationZ = 0;
    public float Zoom = 1.0f;                           // Zoom factor.

    public BSInvMarker(BinaryReader r, Header h) : base(r, h) {
        RotationX = r.ReadUInt16();
        RotationY = r.ReadUInt16();
        RotationZ = r.ReadUInt16();
        Zoom = r.ReadSingle();
    }
}

/// <summary>
/// Unknown
/// </summary>
public class BSBoneLODExtraData : NiExtraData {
    public uint BoneLODCount;                           // Number of bone entries
    public BoneLOD[] BoneLODInfo;                       // Bone Entry

    public BSBoneLODExtraData(BinaryReader r, Header h) : base(r, h) {
        BoneLODCount = r.ReadUInt32();
        BoneLODInfo = r.ReadFArray(r => new BoneLOD(r), BoneLODCount);
    }
}

/// <summary>
/// Links a nif with a Havok Behavior .hkx animation file
/// </summary>
public class BSBehaviorGraphExtraData : NiExtraData {
    public string BehaviourGraphFile;                   // Name of the hkx file.
    public bool ControlsBaseSkeleton;                   // Unknown, has to do with blending appended bones onto an actor.

    public BSBehaviorGraphExtraData(BinaryReader r, Header h) : base(r, h) {
        BehaviourGraphFile = Y.String(r);
        ControlsBaseSkeleton = r.ReadBool32();
    }
}

/// <summary>
/// A controller that trails a bone behind an actor.
/// </summary>
public class BSLagBoneController : NiTimeController {
    public float LinearVelocity;                        // How long it takes to rotate about an actor back to rest position.
    public float LinearRotation;                        // How the bone lags rotation
    public float MaximumDistance;                       // How far bone will tail an actor.

    public BSLagBoneController(BinaryReader r, Header h) : base(r, h) {
        LinearVelocity = r.ReadSingle();
        LinearRotation = r.ReadSingle();
        MaximumDistance = r.ReadSingle();
    }
}

/// <summary>
/// A variation on NiTriShape, for visibility control over vertex groups.
/// </summary>
public class BSLODTriShape : NiTriBasedGeom {
    public uint LOD0Size;
    public uint LOD1Size;
    public uint LOD2Size;

    public BSLODTriShape(BinaryReader r, Header h) : base(r, h) {
        LOD0Size = r.ReadUInt32();
        LOD1Size = r.ReadUInt32();
        LOD2Size = r.ReadUInt32();
    }
}

/// <summary>
/// Furniture Marker for actors
/// </summary>
public class BSFurnitureMarkerNode(BinaryReader r, Header h) : BSFurnitureMarker(r, h) {
}

/// <summary>
/// Unknown, related to trees.
/// </summary>
public class BSLeafAnimNode(BinaryReader r, Header h) : NiNode(r, h) {
}

/// <summary>
/// Node for handling Trees, Switches branch configurations for variation?
/// </summary>
public class BSTreeNode : NiNode {
    public int?[] Bones1;                               // Unknown
    public int?[] Bones;                                // Unknown

    public BSTreeNode(BinaryReader r, Header h) : base(r, h) {
        Bones1 = r.ReadL32FArray(X<NiNode>.Ref);
        Bones = r.ReadL32FArray(X<NiNode>.Ref);
    }
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
    public BSVertexData[] VertexData;
    public Triangle[] Triangles;
    public uint ParticleDataSize;
    public Vector3[] Vertices;
    public Triangle[] TrianglesCopy;

    public BSTriShape(BinaryReader r, Header h) : base(r, h) {
        BoundingSphere = new NiBound(r);
        Skin = X<NiObject>.Ref(r);
        ShaderProperty = X<BSShaderProperty>.Ref(r);
        AlphaProperty = X<NiAlphaProperty>.Ref(r);
        VertexDesc = new BSVertexDesc(r);
        if ((h.UV2 == 130)) NumTriangles = r.ReadUInt32();
        if (h.UV2 < 130) NumTriangles = r.ReadUInt16();
        NumVertices = r.ReadUInt16();
        DataSize = r.ReadUInt32();
        if (DataSize > 0) {
            if ((h.UV2 == 130)) VertexData = r.ReadFArray(r => new BSVertexData(r), NumVertices);
            if ((h.UV2 == 100)) VertexData = r.ReadFArray(r => new BSVertexData(r, true), NumVertices);
            Triangles = r.ReadFArray(r => new Triangle(r), NumTriangles);
        }
        if (Particle DataSize > 0 && (h.UV2 == 100)) {
            ParticleDataSize = r.ReadUInt32();
            if (Particle DataSize > 0) Vertices = r.ReadPArray<Vector3>("3f", NumVertices);
            TrianglesCopy = r.ReadFArray(r => new Triangle(r), NumTriangles);
        }
    }
}

/// <summary>
/// Fallout 4 LOD Tri Shape
/// </summary>
public class BSMeshLODTriShape : BSTriShape {
    public uint LOD0Size;
    public uint LOD1Size;
    public uint LOD2Size;

    public BSMeshLODTriShape(BinaryReader r, Header h) : base(r, h) {
        LOD0Size = r.ReadUInt32();
        LOD1Size = r.ReadUInt32();
        LOD2Size = r.ReadUInt32();
    }
}

public class BSGeometryPerSegmentSharedData(BinaryReader r) {
    public uint UserIndex = r.ReadUInt32();             // If Bone ID is 0xffffffff, this value refers to the Segment at the listed index. Otherwise this is the "Biped Object", which is like the body part types in Skyrim and earlier.
    public uint BoneID = r.ReadUInt32();                // A hash of the bone name string.
    public float[] CutOffsets = r.ReadL32PArray<float>("f");
}

public class BSGeometrySegmentSharedData(BinaryReader r) {
    public uint NumSegments = r.ReadUInt32();
    public uint TotalSegments = r.ReadUInt32();
    public uint[] SegmentStarts = r.ReadPArray<uint>("I", NumSegments);
    public BSGeometryPerSegmentSharedData[] PerSegmentData = r.ReadFArray(r => new BSGeometryPerSegmentSharedData(r), TotalSegments);
    public ushort SSFLength = r.ReadUInt16();
    public byte[] SSFFile = r.ReadBytes(SSFLength);
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

    public BSSubIndexTriShape(BinaryReader r, Header h) : base(r, h) {
        if ((NumSegments < TotalSegments) && (Data Size > 0) && (h.UV2 == 130)) SegmentData = new BSGeometrySegmentSharedData(r);
        if ((h.UV2 == 100)) {
            NumSegments = r.ReadUInt32();
            Segment = r.ReadFArray(r => new BSGeometrySegmentData(r, h), NumSegments);
        }
    }
}

/// <summary>
/// Fallout 4 Physics System
/// </summary>
public abstract class bhkSystem(BinaryReader r, Header h) : NiObject(r, h) {
}

/// <summary>
/// Fallout 4 Collision Object
/// </summary>
public class bhkNPCollisionObject : NiCollisionObject {
    public ushort Flags;                                // Due to inaccurate reporting in the CK the Reset and Sync On Update positions are a guess.
                                                        //     Bits: 0=Reset, 2=Notify, 3=SetLocal, 7=SyncOnUpdate, 10=AnimTargeted
    public int? Data;
    public uint BodyID;

    public bhkNPCollisionObject(BinaryReader r, Header h) : base(r, h) {
        Flags = r.ReadUInt16();
        Data = X<bhkSystem>.Ref(r);
        BodyID = r.ReadUInt32();
    }
}

/// <summary>
/// Fallout 4 Collision System
/// </summary>
public class bhkPhysicsSystem : bhkSystem {
    public byte[] BinaryData;

    public bhkPhysicsSystem(BinaryReader r, Header h) : base(r, h) {
        BinaryData = r.ReadL8Bytes();
    }
}

/// <summary>
/// Fallout 4 Ragdoll System
/// </summary>
public class bhkRagdollSystem : bhkSystem {
    public byte[] BinaryData;

    public bhkRagdollSystem(BinaryReader r, Header h) : base(r, h) {
        BinaryData = r.ReadL8Bytes();
    }
}

/// <summary>
/// Fallout 4 Extra Data
/// </summary>
public class BSExtraData(BinaryReader r, Header h) : NiExtraData(r, h) {
}

/// <summary>
/// Fallout 4 Cloth data
/// </summary>
public class BSClothExtraData : BSExtraData {
    public byte[] BinaryData;

    public BSClothExtraData(BinaryReader r, Header h) : base(r, h) {
        BinaryData = r.ReadL8Bytes();
    }
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

    public BSSkin::Instance(BinaryReader r, Header h) : base(r, h) {
        SkeletonRoot = X<NiAVObject>.Ptr(r);
        Data = X<BSSkin::BoneData>.Ref(r);
        Bones = r.ReadL32FArray(X<NiNode>.Ptr);
        Unknown = r.ReadL32PArray<Vector3>("3f");
    }
}

/// <summary>
/// Fallout 4 Bone Data
/// </summary>
public class BSSkin::BoneData : NiObject {
    public BSSkinBoneTrans[] BoneList;

    public BSSkin::BoneData(BinaryReader r, Header h) : base(r, h) {
        BoneList = r.ReadL32FArray(r => new BSSkinBoneTrans(r));
    }
}

/// <summary>
/// Fallout 4 Positional Data
/// </summary>
public class BSPositionData : NiExtraData {
    public float[] Data;

    public BSPositionData(BinaryReader r, Header h) : base(r, h) {
        Data = r.ReadL32FArray(r => r.ReadL32Half());
    }
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

    public BSConnectPoint::Parents(BinaryReader r, Header h) : base(r, h) {
        ConnectPoints = r.ReadL32FArray(r => new BSConnectPoint(r));
    }
}

/// <summary>
/// Fallout 4 Item Slot Child
/// </summary>
public class BSConnectPoint::Children : NiExtraData {
    public bool Skinned;
    public string[] Name;

    public BSConnectPoint::Children(BinaryReader r, Header h) : base(r, h) {
        Skinned = r.ReadBool32();
        Name = r.ReadL32FArray(r => r.ReadL32L32AString());
    }
}

/// <summary>
/// Fallout 4 Eye Center Data
/// </summary>
public class BSEyeCenterExtraData : NiExtraData {
    public float[] Data;

    public BSEyeCenterExtraData(BinaryReader r, Header h) : base(r, h) {
        Data = r.ReadL32PArray<float>("f");
    }
}

public class BSPackedGeomDataCombined(BinaryReader r) {
    public float GrayscaletoPaletteScale = r.ReadSingle();
    public NiTransform Transform = new NiTransform(r);
    public NiBound BoundingSphere = new NiBound(r);
}

public class BSPackedGeomData {
    public uint NumVerts;
    public uint LODLevels;
    public uint TriCountLOD0;
    public uint TriOffsetLOD0;
    public uint TriCountLOD1;
    public uint TriOffsetLOD1;
    public uint TriCountLOD2;
    public uint TriOffsetLOD2;
    public BSPackedGeomDataCombined[] Combined;
    public BSVertexDesc VertexDesc;
    public BSVertexData[] VertexData;
    public Triangle[] Triangles;

    public BSPackedGeomData(BinaryReader r) {
        NumVerts = r.ReadUInt32();
        LODLevels = r.ReadUInt32();
        TriCountLOD0 = r.ReadUInt32();
        TriOffsetLOD0 = r.ReadUInt32();
        TriCountLOD1 = r.ReadUInt32();
        TriOffsetLOD1 = r.ReadUInt32();
        TriCountLOD2 = r.ReadUInt32();
        TriOffsetLOD2 = r.ReadUInt32();
        Combined = r.ReadL32FArray(r => new BSPackedGeomDataCombined(r));
        VertexDesc = new BSVertexDesc(r);
        if (!BSPackedCombinedSharedGeomDataExtra) {
            VertexData = r.ReadFArray(r => new BSVertexData(r), NumVerts);
            Triangles = r.ReadFArray(r => new Triangle(r), TriCountLOD0 + TriCountLOD1 + TriCountLOD2);
        }
    }
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

    public BSPackedCombinedGeomDataExtra(BinaryReader r, Header h) : base(r, h) {
        VertexDesc = new BSVertexDesc(r);
        NumVertices = r.ReadUInt32();
        NumTriangles = r.ReadUInt32();
        UnknownFlags1 = r.ReadUInt32();
        UnknownFlags2 = r.ReadUInt32();
        NumData = r.ReadUInt32();
        if (BSPackedCombinedSharedGeomDataExtra) Object = r.ReadFArray(r => new BSPackedGeomObject(r), NumData);
        ObjectData = r.ReadFArray(r => new BSPackedGeomData(r), NumData);
    }
}

/// <summary>
/// Fallout 4 Packed Combined Shared Geometry Data.
/// Geometry is NOT baked into the file. It is instead a reference to the shape via a Shape ID (currently undecoded)
/// which loads the geometry via the STAT form for the NIF.
/// </summary>
public class BSPackedCombinedSharedGeomDataExtra(BinaryReader r, Header h) : BSPackedCombinedGeomDataExtra(r, h) {
}

public class NiLightRadiusController(BinaryReader r, Header h) : NiFloatInterpController(r, h) {
}

public class BSDynamicTriShape : BSTriShape {
    public uint VertexDataSize;
    public Vector4[] Vertices;

    public BSDynamicTriShape(BinaryReader r, Header h) : base(r, h) {
        VertexDataSize = r.ReadUInt32();
        if (VertexDataSize > 0) Vertices = r.ReadPArray<Vector4>("4f", NumVertices);
    }
}

#endregion

/// <summary>
/// Large ref flag.
/// </summary>
public class BSDistantObjectLargeRefExtraData : NiExtraData {
    public bool LargeRef;

    public BSDistantObjectLargeRefExtraData(BinaryReader r, Header h) : base(r, h) {
        LargeRef = r.ReadBool32();
    }
}

