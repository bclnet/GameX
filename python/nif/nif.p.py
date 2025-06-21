import os
from io import BytesIO
from enum import Enum, Flag
from typing import TypeVar, Generic
from gamex import FileSource, PakBinaryT, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo
from gamex.compression import decompressLz4, decompressZlib
from gamex.Bethesda.formats.records import FormType, Header

T = TypeVar('T')

# types
type Vector3 = np.ndarray
type Vector4 = np.ndarray
type Matrix4x4 = np.ndarray

# typedefs
class Reader: pass
class Color: pass

#region X

class X(Generic[T]):
    @staticmethod
    def read(type: type, r: Reader) -> object:
        if type == float: return r.readSingle()
        elif type == byte: return r.readByte()
        elif type == str: return r.readL32Encoding()
        elif type == Vector3: return r.readVector3()
        elif type == Quaternion: return r.readQuaternionWFirst()
        elif type == Color4: return Color4(r)
        else: raise NotImplementedError('Tried to read an unsupported type.')
    # Refers to an object before the current one in the hierarchy.
    @staticmethod
    def ptr(r: Reader): return None if (v := r.readInt32()) < 0 else v #:M
    # Refers to an object after the current one in the hierarchy.
    @staticmethod
    def ref(r: Reader): return None if (v := r.readInt32()) < 0 else v #:M

class Y:
    @staticmethod
    def string(r: Reader) -> str: return r.readL32Encoding()
    @staticmethod
    def stringRef(r: Reader, p: int) -> str: return None
    @staticmethod
    def isVersionSupported(v: int) -> bool: return True
    @staticmethod
    def parseHeaderStr(s: str) -> tuple:
        p = s.indexOf('Version')
        if p >= 0:
            v = s
            v = v[(p + 8):]
            for i in range(len(v)):
                if v[i].isdigit() or v[i] == '.': continue
                else: v = v[:i]
            ver = Header.ver2Num(v)
            if not Header.isVersionSupported(ver): raise Exception(f'Version {Header.ver2Str(ver)} ({ver}) is not supported.')
            return (s, ver)
        elif s.startsWith('NS'): return (s, 0x0a010000); # Dodgy version for NeoSteam
        raise Exception('Invalid header string')
    @staticmethod
    def ver2Str(v: int) -> str:
        if v == 0: return ''
        elif v < 0x0303000D:
            # this is an old-style 2-number version with one period
            s = f'{(v >> 24) & 0xff}.{(v >> 16) & 0xff}'
            sub_num1 = (v >> 8) & 0xff; sub_num2 = v & 0xff
            if sub_num1 > 0 or sub_num2 > 0: s += f'{sub_num1}'
            if sub_num2 > 0: s += f'{sub_num2}'
            return s
        # this is a new-style 4-number version with 3 periods
        else: return f'{(v >> 24) & 0xff}.{(v >> 16) & 0xff}.{(v >> 8) & 0xff}.{v & 0xff}'
    @staticmethod
    def ver2Num(s: str) -> int:
        if not s: return 0
        if '.' in s:
            l = s.split('.')
            v = 0
            if len(l) > 4: return 0 # Version # has more than 3 dots in it.
            elif len(l) == 2:
                # this is an old style version number.
                v += int(l[0]) << (3 * 8)
                if len(l[1]) >= 1: v += int(l[1][0:1]) << (2 * 8)
                if len(l[1]) >= 2: v += int(l[1][1:2]) << (1 * 8)
                if len(l[1]) >= 3: v += int(l[1][2:])
                return v
            # this is a new style version number with dots separating the digits
            for i in range(min(4, len(l))): v += int(l[i]) << ((3 - i) * 8)
            return v
        return int(s)

class Flags(Flag):
    pass

#endregion

#region Enums

# Describes the options for the accum root on NiControllerSequence.
class AccumFlags(Flag):
    ACCUM_X_TRANS = 0,              # X Translation will be accumulated.
    ACCUM_Y_TRANS = 1 << 1,         # Y Translation will be accumulated.
    ACCUM_Z_TRANS = 1 << 2,         # Z Translation will be accumulated.
    ACCUM_X_ROT = 1 << 3,           # X Rotation will be accumulated.
    ACCUM_Y_ROT = 1 << 4,           # Y Rotation will be accumulated.
    ACCUM_Z_ROT = 1 << 5,           # Z Rotation will be accumulated.
    ACCUM_X_FRONT = 1 << 6,         # +X is front facing. (Default)
    ACCUM_Y_FRONT = 1 << 7,         # +Y is front facing.
    ACCUM_Z_FRONT = 1 << 8,         # +Z is front facing.
    ACCUM_NEG_FRONT = 1 << 9        # -X is front facing.

# Describes how the vertex colors are blended with the filtered texture color.
class ApplyMode(Enum):
    APPLY_REPLACE = 0,              # Replaces existing color
    APPLY_DECAL = 1,                # For placing images on the object like stickers.
    APPLY_MODULATE = 2,             # Modulates existing color. (Default)
    APPLY_HILIGHT = 3,              # PS2 Only.  Function Unknown.
    APPLY_HILIGHT2 = 4              # Parallax Flag in some Oblivion meshes.

# The type of texture.
class TexType(Enum):
    BASE_MAP = 0,                   # The basic texture used by most meshes.
    DARK_MAP = 1,                   # Used to darken the model with false lighting.
    DETAIL_MAP = 2,                 # Combined with base map for added detail.  Usually tiled over the mesh many times for close-up view.
    GLOSS_MAP = 3,                  # Allows the specularity (glossyness) of an object to differ across its surface.
    GLOW_MAP = 4,                   # Creates a glowing effect.  Basically an incandescence map.
    BUMP_MAP = 5,                   # Used to make the object appear to have more detail than it really does.
    NORMAL_MAP = 6,                 # Used to make the object appear to have more detail than it really does.
    PARALLAX_MAP = 7,               # Parallax map.
    DECAL_0_MAP = 8,                # For placing images on the object like stickers.
    DECAL_1_MAP = 9,                # For placing images on the object like stickers.
    DECAL_2_MAP = 10,               # For placing images on the object like stickers.
    DECAL_3_MAP = 11                # For placing images on the object like stickers.

# The type of animation interpolation (blending) that will be used on the associated key frames.
class KeyType(Enum):
    LINEAR_KEY = 1,                 # Use linear interpolation.
    QUADRATIC_KEY = 2,              # Use quadratic interpolation.  Forward and back tangents will be stored.
    TBC_KEY = 3,                    # Use Tension Bias Continuity interpolation.  Tension, bias, and continuity will be stored.
    XYZ_ROTATION_KEY = 4,           # For use only with rotation data.  Separate X, Y, and Z keys will be stored instead of using quaternions.
    CONST_KEY = 5                   # Step function. Used for visibility keys in NiBoolData.

# Bethesda Havok. Material descriptor for a Havok shape in Oblivion.
class OblivionHavokMaterial(Enum):
    OB_HAV_MAT_STONE = 0,           # Stone
    OB_HAV_MAT_CLOTH = 1,           # Cloth
    OB_HAV_MAT_DIRT = 2,            # Dirt
    OB_HAV_MAT_GLASS = 3,           # Glass
    OB_HAV_MAT_GRASS = 4,           # Grass
    OB_HAV_MAT_METAL = 5,           # Metal
    OB_HAV_MAT_ORGANIC = 6,         # Organic
    OB_HAV_MAT_SKIN = 7,            # Skin
    OB_HAV_MAT_WATER = 8,           # Water
    OB_HAV_MAT_WOOD = 9,            # Wood
    OB_HAV_MAT_HEAVY_STONE = 10,    # Heavy Stone
    OB_HAV_MAT_HEAVY_METAL = 11,    # Heavy Metal
    OB_HAV_MAT_HEAVY_WOOD = 12,     # Heavy Wood
    OB_HAV_MAT_CHAIN = 13,          # Chain
    OB_HAV_MAT_SNOW = 14,           # Snow
    OB_HAV_MAT_STONE_STAIRS = 15,   # Stone Stairs
    OB_HAV_MAT_CLOTH_STAIRS = 16,   # Cloth Stairs
    OB_HAV_MAT_DIRT_STAIRS = 17,    # Dirt Stairs
    OB_HAV_MAT_GLASS_STAIRS = 18,   # Glass Stairs
    OB_HAV_MAT_GRASS_STAIRS = 19,   # Grass Stairs
    OB_HAV_MAT_METAL_STAIRS = 20,   # Metal Stairs
    OB_HAV_MAT_ORGANIC_STAIRS = 21, # Organic Stairs
    OB_HAV_MAT_SKIN_STAIRS = 22,    # Skin Stairs
    OB_HAV_MAT_WATER_STAIRS = 23,   # Water Stairs
    OB_HAV_MAT_WOOD_STAIRS = 24,    # Wood Stairs
    OB_HAV_MAT_HEAVY_STONE_STAIRS = 25, # Heavy Stone Stairs
    OB_HAV_MAT_HEAVY_METAL_STAIRS = 26, # Heavy Metal Stairs
    OB_HAV_MAT_HEAVY_WOOD_STAIRS = 27, # Heavy Wood Stairs
    OB_HAV_MAT_CHAIN_STAIRS = 28,   # Chain Stairs
    OB_HAV_MAT_SNOW_STAIRS = 29,    # Snow Stairs
    OB_HAV_MAT_ELEVATOR = 30,       # Elevator
    OB_HAV_MAT_RUBBER = 31          # Rubber

# Bethesda Havok. Material descriptor for a Havok shape in Fallout 3 and Fallout NV.
class Fallout3HavokMaterial(Enum):
    FO_HAV_MAT_STONE = 0,           # Stone
    FO_HAV_MAT_CLOTH = 1,           # Cloth
    FO_HAV_MAT_DIRT = 2,            # Dirt
    FO_HAV_MAT_GLASS = 3,           # Glass
    FO_HAV_MAT_GRASS = 4,           # Grass
    FO_HAV_MAT_METAL = 5,           # Metal
    FO_HAV_MAT_ORGANIC = 6,         # Organic
    FO_HAV_MAT_SKIN = 7,            # Skin
    FO_HAV_MAT_WATER = 8,           # Water
    FO_HAV_MAT_WOOD = 9,            # Wood
    FO_HAV_MAT_HEAVY_STONE = 10,    # Heavy Stone
    FO_HAV_MAT_HEAVY_METAL = 11,    # Heavy Metal
    FO_HAV_MAT_HEAVY_WOOD = 12,     # Heavy Wood
    FO_HAV_MAT_CHAIN = 13,          # Chain
    FO_HAV_MAT_BOTTLECAP = 14,      # Bottlecap
    FO_HAV_MAT_ELEVATOR = 15,       # Elevator
    FO_HAV_MAT_HOLLOW_METAL = 16,   # Hollow Metal
    FO_HAV_MAT_SHEET_METAL = 17,    # Sheet Metal
    FO_HAV_MAT_SAND = 18,           # Sand
    FO_HAV_MAT_BROKEN_CONCRETE = 19,# Broken Concrete
    FO_HAV_MAT_VEHICLE_BODY = 20,   # Vehicle Body
    FO_HAV_MAT_VEHICLE_PART_SOLID = 21, # Vehicle Part Solid
    FO_HAV_MAT_VEHICLE_PART_HOLLOW = 22, # Vehicle Part Hollow
    FO_HAV_MAT_BARREL = 23,         # Barrel
    FO_HAV_MAT_BOTTLE = 24,         # Bottle
    FO_HAV_MAT_SODA_CAN = 25,       # Soda Can
    FO_HAV_MAT_PISTOL = 26,         # Pistol
    FO_HAV_MAT_RIFLE = 27,          # Rifle
    FO_HAV_MAT_SHOPPING_CART = 28,  # Shopping Cart
    FO_HAV_MAT_LUNCHBOX = 29,       # Lunchbox
    FO_HAV_MAT_BABY_RATTLE = 30,    # Baby Rattle
    FO_HAV_MAT_RUBBER_BALL = 31,    # Rubber Ball
    FO_HAV_MAT_STONE_PLATFORM = 32, # Stone
    FO_HAV_MAT_CLOTH_PLATFORM = 33, # Cloth
    FO_HAV_MAT_DIRT_PLATFORM = 34,  # Dirt
    FO_HAV_MAT_GLASS_PLATFORM = 35, # Glass
    FO_HAV_MAT_GRASS_PLATFORM = 36, # Grass
    FO_HAV_MAT_METAL_PLATFORM = 37, # Metal
    FO_HAV_MAT_ORGANIC_PLATFORM = 38, # Organic
    FO_HAV_MAT_SKIN_PLATFORM = 39,  # Skin
    FO_HAV_MAT_WATER_PLATFORM = 40, # Water
    FO_HAV_MAT_WOOD_PLATFORM = 41,  # Wood
    FO_HAV_MAT_HEAVY_STONE_PLATFORM = 42, # Heavy Stone
    FO_HAV_MAT_HEAVY_METAL_PLATFORM = 43, # Heavy Metal
    FO_HAV_MAT_HEAVY_WOOD_PLATFORM = 44, # Heavy Wood
    FO_HAV_MAT_CHAIN_PLATFORM = 45, # Chain
    FO_HAV_MAT_BOTTLECAP_PLATFORM = 46, # Bottlecap
    FO_HAV_MAT_ELEVATOR_PLATFORM = 47, # Elevator
    FO_HAV_MAT_HOLLOW_METAL_PLATFORM = 48, # Hollow Metal
    FO_HAV_MAT_SHEET_METAL_PLATFORM = 49, # Sheet Metal
    FO_HAV_MAT_SAND_PLATFORM = 50,  # Sand
    FO_HAV_MAT_BROKEN_CONCRETE_PLATFORM = 51, # Broken Concrete
    FO_HAV_MAT_VEHICLE_BODY_PLATFORM = 52, # Vehicle Body
    FO_HAV_MAT_VEHICLE_PART_SOLID_PLATFORM = 53, # Vehicle Part Solid
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_PLATFORM = 54, # Vehicle Part Hollow
    FO_HAV_MAT_BARREL_PLATFORM = 55,# Barrel
    FO_HAV_MAT_BOTTLE_PLATFORM = 56,# Bottle
    FO_HAV_MAT_SODA_CAN_PLATFORM = 57, # Soda Can
    FO_HAV_MAT_PISTOL_PLATFORM = 58,# Pistol
    FO_HAV_MAT_RIFLE_PLATFORM = 59, # Rifle
    FO_HAV_MAT_SHOPPING_CART_PLATFORM = 60, # Shopping Cart
    FO_HAV_MAT_LUNCHBOX_PLATFORM = 61, # Lunchbox
    FO_HAV_MAT_BABY_RATTLE_PLATFORM = 62, # Baby Rattle
    FO_HAV_MAT_RUBBER_BALL_PLATFORM = 63, # Rubber Ball
    FO_HAV_MAT_STONE_STAIRS = 64,   # Stone
    FO_HAV_MAT_CLOTH_STAIRS = 65,   # Cloth
    FO_HAV_MAT_DIRT_STAIRS = 66,    # Dirt
    FO_HAV_MAT_GLASS_STAIRS = 67,   # Glass
    FO_HAV_MAT_GRASS_STAIRS = 68,   # Grass
    FO_HAV_MAT_METAL_STAIRS = 69,   # Metal
    FO_HAV_MAT_ORGANIC_STAIRS = 70, # Organic
    FO_HAV_MAT_SKIN_STAIRS = 71,    # Skin
    FO_HAV_MAT_WATER_STAIRS = 72,   # Water
    FO_HAV_MAT_WOOD_STAIRS = 73,    # Wood
    FO_HAV_MAT_HEAVY_STONE_STAIRS = 74, # Heavy Stone
    FO_HAV_MAT_HEAVY_METAL_STAIRS = 75, # Heavy Metal
    FO_HAV_MAT_HEAVY_WOOD_STAIRS = 76, # Heavy Wood
    FO_HAV_MAT_CHAIN_STAIRS = 77,   # Chain
    FO_HAV_MAT_BOTTLECAP_STAIRS = 78, # Bottlecap
    FO_HAV_MAT_ELEVATOR_STAIRS = 79,# Elevator
    FO_HAV_MAT_HOLLOW_METAL_STAIRS = 80, # Hollow Metal
    FO_HAV_MAT_SHEET_METAL_STAIRS = 81, # Sheet Metal
    FO_HAV_MAT_SAND_STAIRS = 82,    # Sand
    FO_HAV_MAT_BROKEN_CONCRETE_STAIRS = 83, # Broken Concrete
    FO_HAV_MAT_VEHICLE_BODY_STAIRS = 84, # Vehicle Body
    FO_HAV_MAT_VEHICLE_PART_SOLID_STAIRS = 85, # Vehicle Part Solid
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_STAIRS = 86, # Vehicle Part Hollow
    FO_HAV_MAT_BARREL_STAIRS = 87,  # Barrel
    FO_HAV_MAT_BOTTLE_STAIRS = 88,  # Bottle
    FO_HAV_MAT_SODA_CAN_STAIRS = 89,# Soda Can
    FO_HAV_MAT_PISTOL_STAIRS = 90,  # Pistol
    FO_HAV_MAT_RIFLE_STAIRS = 91,   # Rifle
    FO_HAV_MAT_SHOPPING_CART_STAIRS = 92, # Shopping Cart
    FO_HAV_MAT_LUNCHBOX_STAIRS = 93,# Lunchbox
    FO_HAV_MAT_BABY_RATTLE_STAIRS = 94, # Baby Rattle
    FO_HAV_MAT_RUBBER_BALL_STAIRS = 95, # Rubber Ball
    FO_HAV_MAT_STONE_STAIRS_PLATFORM = 96, # Stone
    FO_HAV_MAT_CLOTH_STAIRS_PLATFORM = 97, # Cloth
    FO_HAV_MAT_DIRT_STAIRS_PLATFORM = 98, # Dirt
    FO_HAV_MAT_GLASS_STAIRS_PLATFORM = 99, # Glass
    FO_HAV_MAT_GRASS_STAIRS_PLATFORM = 100, # Grass
    FO_HAV_MAT_METAL_STAIRS_PLATFORM = 101, # Metal
    FO_HAV_MAT_ORGANIC_STAIRS_PLATFORM = 102, # Organic
    FO_HAV_MAT_SKIN_STAIRS_PLATFORM = 103, # Skin
    FO_HAV_MAT_WATER_STAIRS_PLATFORM = 104, # Water
    FO_HAV_MAT_WOOD_STAIRS_PLATFORM = 105, # Wood
    FO_HAV_MAT_HEAVY_STONE_STAIRS_PLATFORM = 106, # Heavy Stone
    FO_HAV_MAT_HEAVY_METAL_STAIRS_PLATFORM = 107, # Heavy Metal
    FO_HAV_MAT_HEAVY_WOOD_STAIRS_PLATFORM = 108, # Heavy Wood
    FO_HAV_MAT_CHAIN_STAIRS_PLATFORM = 109, # Chain
    FO_HAV_MAT_BOTTLECAP_STAIRS_PLATFORM = 110, # Bottlecap
    FO_HAV_MAT_ELEVATOR_STAIRS_PLATFORM = 111, # Elevator
    FO_HAV_MAT_HOLLOW_METAL_STAIRS_PLATFORM = 112, # Hollow Metal
    FO_HAV_MAT_SHEET_METAL_STAIRS_PLATFORM = 113, # Sheet Metal
    FO_HAV_MAT_SAND_STAIRS_PLATFORM = 114, # Sand
    FO_HAV_MAT_BROKEN_CONCRETE_STAIRS_PLATFORM = 115, # Broken Concrete
    FO_HAV_MAT_VEHICLE_BODY_STAIRS_PLATFORM = 116, # Vehicle Body
    FO_HAV_MAT_VEHICLE_PART_SOLID_STAIRS_PLATFORM = 117, # Vehicle Part Solid
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_STAIRS_PLATFORM = 118, # Vehicle Part Hollow
    FO_HAV_MAT_BARREL_STAIRS_PLATFORM = 119, # Barrel
    FO_HAV_MAT_BOTTLE_STAIRS_PLATFORM = 120, # Bottle
    FO_HAV_MAT_SODA_CAN_STAIRS_PLATFORM = 121, # Soda Can
    FO_HAV_MAT_PISTOL_STAIRS_PLATFORM = 122, # Pistol
    FO_HAV_MAT_RIFLE_STAIRS_PLATFORM = 123, # Rifle
    FO_HAV_MAT_SHOPPING_CART_STAIRS_PLATFORM = 124, # Shopping Cart
    FO_HAV_MAT_LUNCHBOX_STAIRS_PLATFORM = 125, # Lunchbox
    FO_HAV_MAT_BABY_RATTLE_STAIRS_PLATFORM = 126, # Baby Rattle
    FO_HAV_MAT_RUBBER_BALL_STAIRS_PLATFORM = 127 # Rubber Ball

# Bethesda Havok. Material descriptor for a Havok shape in Skyrim.
class SkyrimHavokMaterial(Enum):
    SKY_HAV_MAT_BROKEN_STONE = 131151687, # Broken Stone
    SKY_HAV_MAT_LIGHT_WOOD = 365420259, # Light Wood
    SKY_HAV_MAT_SNOW = 398949039,   # Snow
    SKY_HAV_MAT_GRAVEL = 428587608, # Gravel
    SKY_HAV_MAT_MATERIAL_CHAIN_METAL = 438912228, # Material Chain Metal
    SKY_HAV_MAT_BOTTLE = 493553910, # Bottle
    SKY_HAV_MAT_WOOD = 500811281,   # Wood
    SKY_HAV_MAT_SKIN = 591247106,   # Skin
    SKY_HAV_MAT_UNKNOWN_617099282 = 617099282, # Unknown in Creation Kit v1.9.32.0. Found in Dawnguard DLC in meshes\dlc01\clutter\dlc01deerskin.nif.
    SKY_HAV_MAT_BARREL = 732141076, # Barrel
    SKY_HAV_MAT_MATERIAL_CERAMIC_MEDIUM = 781661019, # Material Ceramic Medium
    SKY_HAV_MAT_MATERIAL_BASKET = 790784366, # Material Basket
    SKY_HAV_MAT_ICE = 873356572,    # Ice
    SKY_HAV_MAT_STAIRS_STONE = 899511101, # Stairs Stone
    SKY_HAV_MAT_WATER = 1024582599, # Water
    SKY_HAV_MAT_UNKNOWN_1028101969 = 1028101969, # Unknown in Creation Kit v1.6.89.0. Found in actors\draugr\character assets\skeletons.nif.
    SKY_HAV_MAT_MATERIAL_BLADE_1HAND = 1060167844, # Material Blade 1 Hand
    SKY_HAV_MAT_MATERIAL_BOOK = 1264672850, # Material Book
    SKY_HAV_MAT_MATERIAL_CARPET = 1286705471, # Material Carpet
    SKY_HAV_MAT_SOLID_METAL = 1288358971, # Solid Metal
    SKY_HAV_MAT_MATERIAL_AXE_1HAND = 1305674443, # Material Axe 1Hand
    SKY_HAV_MAT_UNKNOWN_1440721808 = 1440721808, # Unknown in Creation Kit v1.6.89.0. Found in armor\draugr\draugrbootsfemale_go.nif or armor\amuletsandrings\amuletgnd.nif.
    SKY_HAV_MAT_STAIRS_WOOD = 1461712277, # Stairs Wood
    SKY_HAV_MAT_MUD = 1486385281,   # Mud
    SKY_HAV_MAT_MATERIAL_BOULDER_SMALL = 1550912982, # Material Boulder Small
    SKY_HAV_MAT_STAIRS_SNOW = 1560365355, # Stairs Snow
    SKY_HAV_MAT_HEAVY_STONE = 1570821952, # Heavy Stone
    SKY_HAV_MAT_UNKNOWN_1574477864 = 1574477864, # Unknown in Creation Kit v1.6.89.0. Found in actors\dragon\character assets\skeleton.nif.
    SKY_HAV_MAT_UNKNOWN_1591009235 = 1591009235, # Unknown in Creation Kit v1.6.89.0. Found in trap objects or clutter\displaycases\displaycaselgangled01.nif or actors\deer\character assets\skeleton.nif.
    SKY_HAV_MAT_MATERIAL_BOWS_STAVES = 1607128641, # Material Bows Staves
    SKY_HAV_MAT_MATERIAL_WOOD_AS_STAIRS = 1803571212, # Material Wood As Stairs
    SKY_HAV_MAT_GRASS = 1848600814, # Grass
    SKY_HAV_MAT_MATERIAL_BOULDER_LARGE = 1885326971, # Material Boulder Large
    SKY_HAV_MAT_MATERIAL_STONE_AS_STAIRS = 1886078335, # Material Stone As Stairs
    SKY_HAV_MAT_MATERIAL_BLADE_2HAND = 2022742644, # Material Blade 2Hand
    SKY_HAV_MAT_MATERIAL_BOTTLE_SMALL = 2025794648, # Material Bottle Small
    SKY_HAV_MAT_SAND = 2168343821,  # Sand
    SKY_HAV_MAT_HEAVY_METAL = 2229413539, # Heavy Metal
    SKY_HAV_MAT_UNKNOWN_2290050264 = 2290050264, # Unknown in Creation Kit v1.9.32.0. Found in Dawnguard DLC in meshes\dlc01\clutter\dlc01sabrecatpelt.nif.
    SKY_HAV_MAT_DRAGON = 2518321175,# Dragon
    SKY_HAV_MAT_MATERIAL_BLADE_1HAND_SMALL = 2617944780, # Material Blade 1Hand Small
    SKY_HAV_MAT_MATERIAL_SKIN_SMALL = 2632367422, # Material Skin Small
    SKY_HAV_MAT_STAIRS_BROKEN_STONE = 2892392795, # Stairs Broken Stone
    SKY_HAV_MAT_MATERIAL_SKIN_LARGE = 2965929619, # Material Skin Large
    SKY_HAV_MAT_ORGANIC = 2974920155, # Organic
    SKY_HAV_MAT_MATERIAL_BONE = 3049421844, # Material Bone
    SKY_HAV_MAT_HEAVY_WOOD = 3070783559, # Heavy Wood
    SKY_HAV_MAT_MATERIAL_CHAIN = 3074114406, # Material Chain
    SKY_HAV_MAT_DIRT = 3106094762,  # Dirt
    SKY_HAV_MAT_MATERIAL_ARMOR_LIGHT = 3424720541, # Material Armor Light
    SKY_HAV_MAT_MATERIAL_SHIELD_LIGHT = 3448167928, # Material Shield Light
    SKY_HAV_MAT_MATERIAL_COIN = 3589100606, # Material Coin
    SKY_HAV_MAT_MATERIAL_SHIELD_HEAVY = 3702389584, # Material Shield Heavy
    SKY_HAV_MAT_MATERIAL_ARMOR_HEAVY = 3708432437, # Material Armor Heavy
    SKY_HAV_MAT_MATERIAL_ARROW = 3725505938, # Material Arrow
    SKY_HAV_MAT_GLASS = 3739830338, # Glass
    SKY_HAV_MAT_STONE = 3741512247, # Stone
    SKY_HAV_MAT_CLOTH = 3839073443, # Cloth
    SKY_HAV_MAT_MATERIAL_BLUNT_2HAND = 3969592277, # Material Blunt 2Hand
    SKY_HAV_MAT_UNKNOWN_4239621792 = 4239621792, # Unknown in Creation Kit v1.9.32.0. Found in Dawnguard DLC in meshes\dlc01\prototype\dlc1protoswingingbridge.nif.
    SKY_HAV_MAT_MATERIAL_BOULDER_MEDIUM = 4283869410 # Material Boulder Medium

# Bethesda Havok. Describes the collision layer a body belongs to in Oblivion.
class OblivionLayer(Enum):
    OL_UNIDENTIFIED = 0,            # Unidentified (white)
    OL_STATIC = 1,                  # Static (red)
    OL_ANIM_STATIC = 2,             # AnimStatic (magenta)
    OL_TRANSPARENT = 3,             # Transparent (light pink)
    OL_CLUTTER = 4,                 # Clutter (light blue)
    OL_WEAPON = 5,                  # Weapon (orange)
    OL_PROJECTILE = 6,              # Projectile (light orange)
    OL_SPELL = 7,                   # Spell (cyan)
    OL_BIPED = 8,                   # Biped (green) Seems to apply to all creatures/NPCs
    OL_TREES = 9,                   # Trees (light brown)
    OL_PROPS = 10,                  # Props (magenta)
    OL_WATER = 11,                  # Water (cyan)
    OL_TRIGGER = 12,                # Trigger (light grey)
    OL_TERRAIN = 13,                # Terrain (light yellow)
    OL_TRAP = 14,                   # Trap (light grey)
    OL_NONCOLLIDABLE = 15,          # NonCollidable (white)
    OL_CLOUD_TRAP = 16,             # CloudTrap (greenish grey)
    OL_GROUND = 17,                 # Ground (none)
    OL_PORTAL = 18,                 # Portal (green)
    OL_STAIRS = 19,                 # Stairs (white)
    OL_CHAR_CONTROLLER = 20,        # CharController (yellow)
    OL_AVOID_BOX = 21,              # AvoidBox (dark yellow)
    OL_UNKNOWN1 = 22,               # ? (white)
    OL_UNKNOWN2 = 23,               # ? (white)
    OL_CAMERA_PICK = 24,            # CameraPick (white)
    OL_ITEM_PICK = 25,              # ItemPick (white)
    OL_LINE_OF_SIGHT = 26,          # LineOfSight (white)
    OL_PATH_PICK = 27,              # PathPick (white)
    OL_CUSTOM_PICK_1 = 28,          # CustomPick1 (white)
    OL_CUSTOM_PICK_2 = 29,          # CustomPick2 (white)
    OL_SPELL_EXPLOSION = 30,        # SpellExplosion (white)
    OL_DROPPING_PICK = 31,          # DroppingPick (white)
    OL_OTHER = 32,                  # Other (white)
    OL_HEAD = 33,                   # Head
    OL_BODY = 34,                   # Body
    OL_SPINE1 = 35,                 # Spine1
    OL_SPINE2 = 36,                 # Spine2
    OL_L_UPPER_ARM = 37,            # LUpperArm
    OL_L_FOREARM = 38,              # LForeArm
    OL_L_HAND = 39,                 # LHand
    OL_L_THIGH = 40,                # LThigh
    OL_L_CALF = 41,                 # LCalf
    OL_L_FOOT = 42,                 # LFoot
    OL_R_UPPER_ARM = 43,            # RUpperArm
    OL_R_FOREARM = 44,              # RForeArm
    OL_R_HAND = 45,                 # RHand
    OL_R_THIGH = 46,                # RThigh
    OL_R_CALF = 47,                 # RCalf
    OL_R_FOOT = 48,                 # RFoot
    OL_TAIL = 49,                   # Tail
    OL_SIDE_WEAPON = 50,            # SideWeapon
    OL_SHIELD = 51,                 # Shield
    OL_QUIVER = 52,                 # Quiver
    OL_BACK_WEAPON = 53,            # BackWeapon
    OL_BACK_WEAPON2 = 54,           # BackWeapon (?)
    OL_PONYTAIL = 55,               # PonyTail
    OL_WING = 56,                   # Wing
    OL_NULL = 57                    # Null

# Bethesda Havok. Describes the collision layer a body belongs to in Fallout 3 and Fallout NV.
class Fallout3Layer(Enum):
    FOL_UNIDENTIFIED = 0,           # Unidentified (white)
    FOL_STATIC = 1,                 # Static (red)
    FOL_ANIM_STATIC = 2,            # AnimStatic (magenta)
    FOL_TRANSPARENT = 3,            # Transparent (light pink)
    FOL_CLUTTER = 4,                # Clutter (light blue)
    FOL_WEAPON = 5,                 # Weapon (orange)
    FOL_PROJECTILE = 6,             # Projectile (light orange)
    FOL_SPELL = 7,                  # Spell (cyan)
    FOL_BIPED = 8,                  # Biped (green) Seems to apply to all creatures/NPCs
    FOL_TREES = 9,                  # Trees (light brown)
    FOL_PROPS = 10,                 # Props (magenta)
    FOL_WATER = 11,                 # Water (cyan)
    FOL_TRIGGER = 12,               # Trigger (light grey)
    FOL_TERRAIN = 13,               # Terrain (light yellow)
    FOL_TRAP = 14,                  # Trap (light grey)
    FOL_NONCOLLIDABLE = 15,         # NonCollidable (white)
    FOL_CLOUD_TRAP = 16,            # CloudTrap (greenish grey)
    FOL_GROUND = 17,                # Ground (none)
    FOL_PORTAL = 18,                # Portal (green)
    FOL_DEBRIS_SMALL = 19,          # DebrisSmall (white)
    FOL_DEBRIS_LARGE = 20,          # DebrisLarge (white)
    FOL_ACOUSTIC_SPACE = 21,        # AcousticSpace (white)
    FOL_ACTORZONE = 22,             # Actorzone (white)
    FOL_PROJECTILEZONE = 23,        # Projectilezone (white)
    FOL_GASTRAP = 24,               # GasTrap (yellowish green)
    FOL_SHELLCASING = 25,           # ShellCasing (white)
    FOL_TRANSPARENT_SMALL = 26,     # TransparentSmall (white)
    FOL_INVISIBLE_WALL = 27,        # InvisibleWall (white)
    FOL_TRANSPARENT_SMALL_ANIM = 28,# TransparentSmallAnim (white)
    FOL_DEADBIP = 29,               # Dead Biped (green)
    FOL_CHARCONTROLLER = 30,        # CharController (yellow)
    FOL_AVOIDBOX = 31,              # Avoidbox (orange)
    FOL_COLLISIONBOX = 32,          # Collisionbox (white)
    FOL_CAMERASPHERE = 33,          # Camerasphere (white)
    FOL_DOORDETECTION = 34,         # Doordetection (white)
    FOL_CAMERAPICK = 35,            # Camerapick (white)
    FOL_ITEMPICK = 36,              # Itempick (white)
    FOL_LINEOFSIGHT = 37,           # LineOfSight (white)
    FOL_PATHPICK = 38,              # Pathpick (white)
    FOL_CUSTOMPICK1 = 39,           # Custompick1 (white)
    FOL_CUSTOMPICK2 = 40,           # Custompick2 (white)
    FOL_SPELLEXPLOSION = 41,        # SpellExplosion (white)
    FOL_DROPPINGPICK = 42,          # Droppingpick (white)
    FOL_NULL = 43                   # Null (white)

# Bethesda Havok. Describes the collision layer a body belongs to in Skyrim.
class SkyrimLayer(Enum):
    SKYL_UNIDENTIFIED = 0,          # Unidentified
    SKYL_STATIC = 1,                # Static
    SKYL_ANIMSTATIC = 2,            # Anim Static
    SKYL_TRANSPARENT = 3,           # Transparent
    SKYL_CLUTTER = 4,               # Clutter. Object with this layer will float on water surface.
    SKYL_WEAPON = 5,                # Weapon
    SKYL_PROJECTILE = 6,            # Projectile
    SKYL_SPELL = 7,                 # Spell
    SKYL_BIPED = 8,                 # Biped. Seems to apply to all creatures/NPCs
    SKYL_TREES = 9,                 # Trees
    SKYL_PROPS = 10,                # Props
    SKYL_WATER = 11,                # Water
    SKYL_TRIGGER = 12,              # Trigger
    SKYL_TERRAIN = 13,              # Terrain
    SKYL_TRAP = 14,                 # Trap
    SKYL_NONCOLLIDABLE = 15,        # NonCollidable
    SKYL_CLOUD_TRAP = 16,           # CloudTrap
    SKYL_GROUND = 17,               # Ground. It seems that produces no sound when collide.
    SKYL_PORTAL = 18,               # Portal
    SKYL_DEBRIS_SMALL = 19,         # Debris Small
    SKYL_DEBRIS_LARGE = 20,         # Debris Large
    SKYL_ACOUSTIC_SPACE = 21,       # Acoustic Space
    SKYL_ACTORZONE = 22,            # Actor Zone
    SKYL_PROJECTILEZONE = 23,       # Projectile Zone
    SKYL_GASTRAP = 24,              # Gas Trap
    SKYL_SHELLCASING = 25,          # Shell Casing
    SKYL_TRANSPARENT_SMALL = 26,    # Transparent Small
    SKYL_INVISIBLE_WALL = 27,       # Invisible Wall
    SKYL_TRANSPARENT_SMALL_ANIM = 28, # Transparent Small Anim
    SKYL_WARD = 29,                 # Ward
    SKYL_CHARCONTROLLER = 30,       # Char Controller
    SKYL_STAIRHELPER = 31,          # Stair Helper
    SKYL_DEADBIP = 32,              # Dead Bip
    SKYL_BIPED_NO_CC = 33,          # Biped No CC
    SKYL_AVOIDBOX = 34,             # Avoid Box
    SKYL_COLLISIONBOX = 35,         # Collision Box
    SKYL_CAMERASHPERE = 36,         # Camera Sphere
    SKYL_DOORDETECTION = 37,        # Door Detection
    SKYL_CONEPROJECTILE = 38,       # Cone Projectile
    SKYL_CAMERAPICK = 39,           # Camera Pick
    SKYL_ITEMPICK = 40,             # Item Pick
    SKYL_LINEOFSIGHT = 41,          # Line of Sight
    SKYL_PATHPICK = 42,             # Path Pick
    SKYL_CUSTOMPICK1 = 43,          # Custom Pick 1
    SKYL_CUSTOMPICK2 = 44,          # Custom Pick 2
    SKYL_SPELLEXPLOSION = 45,       # Spell Explosion
    SKYL_DROPPINGPICK = 46,         # Dropping Pick
    SKYL_NULL = 47                  # Null

# Bethesda Havok.
# A byte describing if MOPP Data is organized into chunks (PS3) or not (PC)
class MoppDataBuildType(Enum):
    BUILT_WITH_CHUNK_SUBDIVISION = 0, # Organized in chunks for PS3.
    BUILT_WITHOUT_CHUNK_SUBDIVISION = 1, # Not organized in chunks for PC. (Default)
    BUILD_NOT_SET = 2               # Build type not set yet.

# Target platform for NiPersistentSrcTextureRendererData (later than 30.1).
class PlatformID(Enum):
    ANY = 0,
    XENON = 1,
    PS3 = 2,
    DX9 = 3,
    WII = 4,
    D3D10 = 5

# Target renderer for NiPersistentSrcTextureRendererData (until 30.1).
class RendererID(Enum):
    XBOX360 = 0,
    PS3 = 1,
    DX9 = 2,
    D3D10 = 3,
    WII = 4,
    GENERIC = 5,
    D3D11 = 6

# Describes the pixel format used by the NiPixelData object to store a texture.
class PixelFormat(Enum):
    FMT_RGB = 0,                    # 24-bit RGB. 8 bits per red, blue, and green component.
    FMT_RGBA = 1,                   # 32-bit RGB with alpha. 8 bits per red, blue, green, and alpha component.
    FMT_PAL = 2,                    # 8-bit palette index.
    FMT_PALA = 3,                   # 8-bit palette index with alpha.
    FMT_DXT1 = 4,                   # DXT1 compressed texture.
    FMT_DXT3 = 5,                   # DXT3 compressed texture.
    FMT_DXT5 = 6,                   # DXT5 compressed texture.
    FMT_RGB24NONINT = 7,            # (Deprecated) 24-bit noninterleaved texture, an old PS2 format.
    FMT_BUMP = 8,                   # Uncompressed dU/dV gradient bump map.
    FMT_BUMPLUMA = 9,               # Uncompressed dU/dV gradient bump map with luma channel representing shininess.
    FMT_RENDERSPEC = 10,            # Generic descriptor for any renderer-specific format not described by other formats.
    FMT_1CH = 11,                   # Generic descriptor for formats with 1 component.
    FMT_2CH = 12,                   # Generic descriptor for formats with 2 components.
    FMT_3CH = 13,                   # Generic descriptor for formats with 3 components.
    FMT_4CH = 14,                   # Generic descriptor for formats with 4 components.
    FMT_DEPTH_STENCIL = 15,         # Indicates the NiPixelFormat is meant to be used on a depth/stencil surface.
    FMT_UNKNOWN = 16

# Describes whether pixels have been tiled from their standard row-major format to a format optimized for a particular platform.
class PixelTiling(Enum):
    TILE_NONE = 0,
    TILE_XENON = 1,
    TILE_WII = 2,
    TILE_NV_SWIZZLED = 3

# Describes the pixel format used by the NiPixelData object to store a texture.
class PixelComponent(Enum):
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

# Describes how each pixel should be accessed on NiPixelFormat.
class PixelRepresentation(Enum):
    REP_NORM_INT = 0,
    REP_HALF = 1,
    REP_FLOAT = 2,
    REP_INDEX = 3,
    REP_COMPRESSED = 4,
    REP_UNKNOWN = 5,
    REP_INT = 6

# Describes the color depth in an NiTexture.
class PixelLayout(Enum):
    LAY_PALETTIZED_8 = 0,           # Texture is in 8-bit palettized format.
    LAY_HIGH_COLOR_16 = 1,          # Texture is in 16-bit high color format.
    LAY_TRUE_COLOR_32 = 2,          # Texture is in 32-bit true color format.
    LAY_COMPRESSED = 3,             # Texture is compressed.
    LAY_BUMPMAP = 4,                # Texture is a grayscale bump map.
    LAY_PALETTIZED_4 = 5,           # Texture is in 4-bit palettized format.
    LAY_DEFAULT = 6,                # Use default setting.
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

# Describes how mipmaps are handled in an NiTexture.
class MipMapFormat(Enum):
    MIP_FMT_NO = 0,                 # Texture does not use mip maps.
    MIP_FMT_YES = 1,                # Texture uses mip maps.
    MIP_FMT_DEFAULT = 2             # Use default setting.

# Describes how transparency is handled in an NiTexture.
class AlphaFormat(Enum):
    ALPHA_NONE = 0,                 # No alpha.
    ALPHA_BINARY = 1,               # 1-bit alpha.
    ALPHA_SMOOTH = 2,               # Interpolated 4- or 8-bit alpha.
    ALPHA_DEFAULT = 3               # Use default setting.

# Describes the availiable texture clamp modes, i.e. the behavior of UV mapping outside the [0,1] range.
class TexClampMode(Enum):
    CLAMP_S_CLAMP_T = 0,            # Clamp in both directions.
    CLAMP_S_WRAP_T = 1,             # Clamp in the S(U) direction but wrap in the T(V) direction.
    WRAP_S_CLAMP_T = 2,             # Wrap in the S(U) direction but clamp in the T(V) direction.
    WRAP_S_WRAP_T = 3               # Wrap in both directions.

# Describes the availiable texture filter modes, i.e. the way the pixels in a texture are displayed on screen.
class TexFilterMode(Enum):
    FILTER_NEAREST = 0,             # Nearest neighbor. Uses nearest texel with no mipmapping.
    FILTER_BILERP = 1,              # Bilinear. Linear interpolation with no mipmapping.
    FILTER_TRILERP = 2,             # Trilinear. Linear intepolation between 8 texels (4 nearest texels between 2 nearest mip levels).
    FILTER_NEAREST_MIPNEAREST = 3,  # Nearest texel on nearest mip level.
    FILTER_NEAREST_MIPLERP = 4,     # Linear interpolates nearest texel between two nearest mip levels.
    FILTER_BILERP_MIPNEAREST = 5,   # Linear interpolates on nearest mip level.
    FILTER_ANISOTROPIC = 6          # Anisotropic filtering. One or many trilinear samples depending on anisotropy.

# Describes how to apply vertex colors for NiVertexColorProperty.
class VertMode(Enum):
    VERT_MODE_SRC_IGNORE = 0,       # Emissive, ambient, and diffuse colors are all specified by the NiMaterialProperty.
    VERT_MODE_SRC_EMISSIVE = 1,     # Emissive colors are specified by the source vertex colors. Ambient+Diffuse are specified by the NiMaterialProperty.
    VERT_MODE_SRC_AMB_DIF = 2       # Ambient+Diffuse colors are specified by the source vertex colors. Emissive is specified by the NiMaterialProperty. (Default)

# Describes which lighting equation components influence the final vertex color for NiVertexColorProperty.
class LightMode(Enum):
    LIGHT_MODE_EMISSIVE = 0,        # Emissive.
    LIGHT_MODE_EMI_AMB_DIF = 1      # Emissive + Ambient + Diffuse. (Default)

# The animation cyle behavior.
class CycleType(Enum):
    CYCLE_LOOP = 0,                 # Loop
    CYCLE_REVERSE = 1,              # Reverse
    CYCLE_CLAMP = 2                 # Clamp

# The force field type.
class FieldType(Enum):
    FIELD_WIND = 0,                 # Wind (fixed direction)
    FIELD_POINT = 1                 # Point (fixed origin)

# Determines the way the billboard will react to the camera.
# Billboard mode is stored in lowest 3 bits although Oblivion vanilla nifs uses values higher than 7.
class BillboardMode(Enum):
    ALWAYS_FACE_CAMERA = 0,         # Align billboard and camera forward vector. Minimized rotation.
    ROTATE_ABOUT_UP = 1,            # Align billboard and camera forward vector while allowing rotation around the up axis.
    RIGID_FACE_CAMERA = 2,          # Align billboard and camera forward vector. Non-minimized rotation.
    ALWAYS_FACE_CENTER = 3,         # Billboard forward vector always faces camera ceneter. Minimized rotation.
    RIGID_FACE_CENTER = 4,          # Billboard forward vector always faces camera ceneter. Non-minimized rotation.
    BSROTATE_ABOUT_UP = 5,          # The billboard will only rotate around its local Z axis (it always stays in its local X-Y plane).
    ROTATE_ABOUT_UP2 = 9            # The billboard will only rotate around the up axis (same as ROTATE_ABOUT_UP?).

# Describes stencil buffer test modes for NiStencilProperty.
class StencilCompareMode(Enum):
    TEST_NEVER = 0,                 # Always false. Ref value is ignored.
    TEST_LESS = 1,                  # VRef ‹ VBuf
    TEST_EQUAL = 2,                 # VRef = VBuf
    TEST_LESS_EQUAL = 3,            # VRef ≤ VBuf
    TEST_GREATER = 4,               # VRef › VBuf
    TEST_NOT_EQUAL = 5,             # VRef ≠ VBuf
    TEST_GREATER_EQUAL = 6,         # VRef ≥ VBuf
    TEST_ALWAYS = 7                 # Always true. Buffer is ignored.

# Describes the actions which can occur as a result of tests for NiStencilProperty.
class StencilAction(Enum):
    ACTION_KEEP = 0,                # Keep the current value in the stencil buffer.
    ACTION_ZERO = 1,                # Write zero to the stencil buffer.
    ACTION_REPLACE = 2,             # Write the reference value to the stencil buffer.
    ACTION_INCREMENT = 3,           # Increment the value in the stencil buffer.
    ACTION_DECREMENT = 4,           # Decrement the value in the stencil buffer.
    ACTION_INVERT = 5               # Bitwise invert the value in the stencil buffer.

# Describes the face culling options for NiStencilProperty.
class StencilDrawMode(Enum):
    DRAW_CCW_OR_BOTH = 0,           # Application default, chooses between DRAW_CCW or DRAW_BOTH.
    DRAW_CCW = 1,                   # Draw only the triangles whose vertices are ordered CCW with respect to the viewer. (Standard behavior)
    DRAW_CW = 2,                    # Draw only the triangles whose vertices are ordered CW with respect to the viewer. (Effectively flips faces)
    DRAW_BOTH = 3                   # Draw all triangles, regardless of orientation. (Effectively force double-sided)

# Describes Z-buffer test modes for NiZBufferProperty.
# "Less than" = closer to camera, "Greater than" = further from camera.
class ZCompareMode(Enum):
    ZCOMP_ALWAYS = 0,               # Always true. Buffer is ignored.
    ZCOMP_LESS = 1,                 # VRef ‹ VBuf
    ZCOMP_EQUAL = 2,                # VRef = VBuf
    ZCOMP_LESS_EQUAL = 3,           # VRef ≤ VBuf
    ZCOMP_GREATER = 4,              # VRef › VBuf
    ZCOMP_NOT_EQUAL = 5,            # VRef ≠ VBuf
    ZCOMP_GREATER_EQUAL = 6,        # VRef ≥ VBuf
    ZCOMP_NEVER = 7                 # Always false. Ref value is ignored.

# Bethesda Havok, based on hkpMotion::MotionType. Motion type of a rigid body determines what happens when it is simulated.
class hkMotionType(Enum):
    MO_SYS_INVALID = 0,             # Invalid
    MO_SYS_DYNAMIC = 1,             # A fully-simulated, movable rigid body. At construction time the engine checks the input inertia and selects MO_SYS_SPHERE_INERTIA or MO_SYS_BOX_INERTIA as appropriate.
    MO_SYS_SPHERE_INERTIA = 2,      # Simulation is performed using a sphere inertia tensor.
    MO_SYS_SPHERE_STABILIZED = 3,   # This is the same as MO_SYS_SPHERE_INERTIA, except that simulation of the rigid body is "softened".
    MO_SYS_BOX_INERTIA = 4,         # Simulation is performed using a box inertia tensor.
    MO_SYS_BOX_STABILIZED = 5,      # This is the same as MO_SYS_BOX_INERTIA, except that simulation of the rigid body is "softened".
    MO_SYS_KEYFRAMED = 6,           # Simulation is not performed as a normal rigid body. The keyframed rigid body has an infinite mass when viewed by the rest of the system. (used for creatures)
    MO_SYS_FIXED = 7,               # This motion type is used for the static elements of a game scene, e.g. the landscape. Faster than MO_SYS_KEYFRAMED at velocity 0. (used for weapons)
    MO_SYS_THIN_BOX = 8,            # A box inertia motion which is optimized for thin boxes and has less stability problems
    MO_SYS_CHARACTER = 9            # A specialized motion used for character controllers

# Bethesda Havok, based on hkpRigidBodyDeactivator::DeactivatorType.
# Deactivator Type determines which mechanism Havok will use to classify the body as deactivated.
class hkDeactivatorType(Enum):
    DEACTIVATOR_INVALID = 0,        # Invalid
    DEACTIVATOR_NEVER = 1,          # This will force the rigid body to never deactivate.
    DEACTIVATOR_SPATIAL = 2         # Tells Havok to use a spatial deactivation scheme. This makes use of high and low frequencies of positional motion to determine when deactivation should occur.

# Bethesda Havok, based on hkpRigidBodyCinfo::SolverDeactivation.
# A list of possible solver deactivation settings. This value defines how aggressively the solver deactivates objects.
# Note: Solver deactivation does not save CPU, but reduces creeping of movable objects in a pile quite dramatically.
class hkSolverDeactivation(Enum):
    SOLVER_DEACTIVATION_INVALID = 0,# Invalid
    SOLVER_DEACTIVATION_OFF = 1,    # No solver deactivation.
    SOLVER_DEACTIVATION_LOW = 2,    # Very conservative deactivation, typically no visible artifacts.
    SOLVER_DEACTIVATION_MEDIUM = 3, # Normal deactivation, no serious visible artifacts in most cases.
    SOLVER_DEACTIVATION_HIGH = 4,   # Fast deactivation, visible artifacts.
    SOLVER_DEACTIVATION_MAX = 5     # Very fast deactivation, visible artifacts.

# Bethesda Havok, based on hkpCollidableQualityType. Describes the priority and quality of collisions for a body,
#     e.g. you may expect critical game play objects to have solid high-priority collisions so that they never sink into ground,
#     or may allow penetrations for visual debris objects.
# Notes:
#     - Fixed and keyframed objects cannot interact with each other.
#     - Debris can interpenetrate but still responds to Bullet hits.
#     - Critical objects are forced to not interpenetrate.
#     - Moving objects can interpenetrate slightly with other Moving or Debris objects but nothing else.
class hkQualityType(Enum):
    MO_QUAL_INVALID = 0,            # Automatically assigned to MO_QUAL_FIXED, MO_QUAL_KEYFRAMED or MO_QUAL_DEBRIS
    MO_QUAL_FIXED = 1,              # Static body.
    MO_QUAL_KEYFRAMED = 2,          # Animated body with infinite mass.
    MO_QUAL_DEBRIS = 3,             # Low importance bodies adding visual detail.
    MO_QUAL_MOVING = 4,             # Moving bodies which should not penetrate or leave the world, but can.
    MO_QUAL_CRITICAL = 5,           # Gameplay critical bodies which cannot penetrate or leave the world under any circumstance.
    MO_QUAL_BULLET = 6,             # Fast-moving bodies, such as projectiles.
    MO_QUAL_USER = 7,               # For user.
    MO_QUAL_CHARACTER = 8,          # For use with rigid body character controllers.
    MO_QUAL_KEYFRAMED_REPORT = 9    # Moving bodies with infinite mass which should report contact points and TOI collisions against all other bodies.

# Describes the type of gravitational force.
class ForceType(Enum):
    FORCE_PLANAR = 0,
    FORCE_SPHERICAL = 1,
    FORCE_UNKNOWN = 2

# Describes which aspect of the NiTextureTransform the NiTextureTransformController will modify.
class TransformMember(Enum):
    TT_TRANSLATE_U = 0,             # Control the translation of the U coordinates.
    TT_TRANSLATE_V = 1,             # Control the translation of the V coordinates.
    TT_ROTATE = 2,                  # Control the rotation of the coordinates.
    TT_SCALE_U = 3,                 # Control the scale of the U coordinates.
    TT_SCALE_V = 4                  # Control the scale of the V coordinates.

# Describes the decay function of bomb forces.
class DecayType(Enum):
    DECAY_NONE = 0,                 # No decay.
    DECAY_LINEAR = 1,               # Linear decay.
    DECAY_EXPONENTIAL = 2           # Exponential decay.

# Describes the symmetry type of bomb forces.
class SymmetryType(Enum):
    SPHERICAL_SYMMETRY = 0,         # Spherical Symmetry.
    CYLINDRICAL_SYMMETRY = 1,       # Cylindrical Symmetry.
    PLANAR_SYMMETRY = 2             # Planar Symmetry.

# Controls the way the a particle mesh emitter determines the starting speed and direction of the particles that are emitted.
class VelocityType(Enum):
    VELOCITY_USE_NORMALS = 0,       # Uses the normals of the meshes to determine staring velocity.
    VELOCITY_USE_RANDOM = 1,        # Starts particles with a random velocity.
    VELOCITY_USE_DIRECTION = 2      # Uses the emission axis to determine initial particle direction?

# Controls which parts of the mesh that the particles are emitted from.
class EmitFrom(Enum):
    EMIT_FROM_VERTICES = 0,         # Emit particles from the vertices of the mesh.
    EMIT_FROM_FACE_CENTER = 1,      # Emit particles from the center of the faces of the mesh.
    EMIT_FROM_EDGE_CENTER = 2,      # Emit particles from the center of the edges of the mesh.
    EMIT_FROM_FACE_SURFACE = 3,     # Perhaps randomly emit particles from anywhere on the faces of the mesh?
    EMIT_FROM_EDGE_SURFACE = 4      # Perhaps randomly emit particles from anywhere on the edges of the mesh?

# The type of information that is stored in a texture used by an NiTextureEffect.
class TextureType(Enum):
    TEX_PROJECTED_LIGHT = 0,        # Apply a projected light texture. Each light effect is summed before multiplying by the base texture.
    TEX_PROJECTED_SHADOW = 1,       # Apply a projected shadow texture. Each shadow effect is multiplied by the base texture.
    TEX_ENVIRONMENT_MAP = 2,        # Apply an environment map texture. Added to the base texture and light/shadow/decal maps.
    TEX_FOG_MAP = 3                 # Apply a fog map texture. Alpha channel is used to blend the color channel with the base texture.

# Determines the way that UV texture coordinates are generated.
class CoordGenType(Enum):
    CG_WORLD_PARALLEL = 0,          # Use planar mapping.
    CG_WORLD_PERSPECTIVE = 1,       # Use perspective mapping.
    CG_SPHERE_MAP = 2,              # Use spherical mapping.
    CG_SPECULAR_CUBE_MAP = 3,       # Use specular cube mapping. For NiSourceCubeMap only.
    CG_DIFFUSE_CUBE_MAP = 4         # Use diffuse cube mapping. For NiSourceCubeMap only.

class EndianType(Enum):
    ENDIAN_BIG = 0,                 # The numbers are stored in big endian format, such as those used by PowerPC Mac processors.
    ENDIAN_LITTLE = 1               # The numbers are stored in little endian format, such as those used by Intel and AMD x86 processors.

# Used by NiMaterialColorControllers to select which type of color in the controlled object that will be animated.
class MaterialColor(Enum):
    TC_AMBIENT = 0,                 # Control the ambient color.
    TC_DIFFUSE = 1,                 # Control the diffuse color.
    TC_SPECULAR = 2,                # Control the specular color.
    TC_SELF_ILLUM = 3               # Control the self illumination color.

# Used by NiLightColorControllers to select which type of color in the controlled object that will be animated.
class LightColor(Enum):
    LC_DIFFUSE = 0,                 # Control the diffuse color.
    LC_AMBIENT = 1                  # Control the ambient color.

# Used by NiGeometryData to control the volatility of the mesh.
# Consistency Type is masked to only the upper 4 bits (0xF000). Dirty mask is the lower 12 (0x0FFF) but only used at runtime.
class ConsistencyType(Enum):
    CT_MUTABLE = 0x0000,            # Mutable Mesh
    CT_STATIC = 0x4000,             # Static Mesh
    CT_VOLATILE = 0x8000            # Volatile Mesh

# Describes the way that NiSortAdjustNode modifies the sorting behavior for the subtree below it.
class SortingMode(Enum):
    SORTING_INHERIT = 0,            # Inherit. Acts identical to NiNode.
    SORTING_OFF = 1                 # Disables sort on all geometry under this node.

# The propagation mode controls scene graph traversal during collision detection operations for NiCollisionData.
class PropagationMode(Enum):
    PROPAGATE_ON_SUCCESS = 0,       # Propagation only occurs as a result of a successful collision.
    PROPAGATE_ON_FAILURE = 1,       # (Deprecated) Propagation only occurs as a result of a failed collision.
    PROPAGATE_ALWAYS = 2,           # Propagation always occurs regardless of collision result.
    PROPAGATE_NEVER = 3             # Propagation never occurs regardless of collision result.

# The collision mode controls the type of collision operation that is to take place for NiCollisionData.
class CollisionMode(Enum):
    CM_USE_OBB = 0,                 # Use Bounding Box
    CM_USE_TRI = 1,                 # Use Triangles
    CM_USE_ABV = 2,                 # Use Alternate Bounding Volumes
    CM_NOTEST = 3,                  # Indicates that no collision test should be made.
    CM_USE_NIBOUND = 4              # Use NiBound

class BoundVolumeType(Enum):
    BASE_BV = 0xffffffff,           # Default
    SPHERE_BV = 0,                  # Sphere
    BOX_BV = 1,                     # Box
    CAPSULE_BV = 2,                 # Capsule
    UNION_BV = 4,                   # Union
    HALFSPACE_BV = 5                # Half Space

# Bethesda Havok.
class hkResponseType(Enum):
    RESPONSE_INVALID = 0,           # Invalid Response
    RESPONSE_SIMPLE_CONTACT = 1,    # Do normal collision resolution
    RESPONSE_REPORTING = 2,         # No collision resolution is performed but listeners are called
    RESPONSE_NONE = 3               # Do nothing, ignore all the results.

# Biped bodypart data used for visibility control of triangles.  Options are Fallout 3, except where marked for Skyrim (uses SBP prefix)
# Skyrim BP names are listed only for vanilla names, different creatures have different defnitions for naming.
class BSDismemberBodyPartType(Enum):
    BP_TORSO = 0,                   # Torso
    BP_HEAD = 1,                    # Head
    BP_HEAD2 = 2,                   # Head 2
    BP_LEFTARM = 3,                 # Left Arm
    BP_LEFTARM2 = 4,                # Left Arm 2
    BP_RIGHTARM = 5,                # Right Arm
    BP_RIGHTARM2 = 6,               # Right Arm 2
    BP_LEFTLEG = 7,                 # Left Leg
    BP_LEFTLEG2 = 8,                # Left Leg 2
    BP_LEFTLEG3 = 9,                # Left Leg 3
    BP_RIGHTLEG = 10,               # Right Leg
    BP_RIGHTLEG2 = 11,              # Right Leg 2
    BP_RIGHTLEG3 = 12,              # Right Leg 3
    BP_BRAIN = 13,                  # Brain
    SBP_30_HEAD = 30,               # Skyrim, Head(Human), Body(Atronachs,Beasts), Mask(Dragonpriest)
    SBP_31_HAIR = 31,               # Skyrim, Hair(human), Far(Dragon), Mask2(Dragonpriest),SkinnedFX(Spriggan)
    SBP_32_BODY = 32,               # Skyrim, Main body, extras(Spriggan)
    SBP_33_HANDS = 33,              # Skyrim, Hands L/R, BodyToo(Dragonpriest), Legs(Draugr), Arms(Giant)
    SBP_34_FOREARMS = 34,           # Skyrim, Forearms L/R, Beard(Draugr)
    SBP_35_AMULET = 35,             # Skyrim, Amulet
    SBP_36_RING = 36,               # Skyrim, Ring
    SBP_37_FEET = 37,               # Skyrim, Feet L/R
    SBP_38_CALVES = 38,             # Skyrim, Calves L/R
    SBP_39_SHIELD = 39,             # Skyrim, Shield
    SBP_40_TAIL = 40,               # Skyrim, Tail(Argonian/Khajiit), Skeleton01(Dragon), FX01(AtronachStorm),FXMist (Dragonpriest), Spit(Chaurus,Spider),SmokeFins(IceWraith)
    SBP_41_LONGHAIR = 41,           # Skyrim, Long Hair(Human), Skeleton02(Dragon),FXParticles(Dragonpriest)
    SBP_42_CIRCLET = 42,            # Skyrim, Circlet(Human, MouthFireEffect(Dragon)
    SBP_43_EARS = 43,               # Skyrim, Ears
    SBP_44_DRAGON_BLOODHEAD_OR_MOD_MOUTH = 44, # Skyrim, Bloodied dragon head, or NPC face/mouth
    SBP_45_DRAGON_BLOODWINGL_OR_MOD_NECK = 45, # Skyrim, Left Bloodied dragon wing, Saddle(Horse), or NPC cape, scarf, shawl, neck-tie, etc.
    SBP_46_DRAGON_BLOODWINGR_OR_MOD_CHEST_PRIMARY = 46, # Skyrim, Right Bloodied dragon wing, or NPC chest primary or outergarment
    SBP_47_DRAGON_BLOODTAIL_OR_MOD_BACK = 47, # Skyrim, Bloodied dragon tail, or NPC backpack/wings/...
    SBP_48_MOD_MISC1 = 48,          # Anything that does not fit in the list
    SBP_49_MOD_PELVIS_PRIMARY = 49, # Pelvis primary or outergarment
    SBP_50_DECAPITATEDHEAD = 50,    # Skyrim, Decapitated Head
    SBP_51_DECAPITATE = 51,         # Skyrim, Decapitate, neck gore
    SBP_52_MOD_PELVIS_SECONDARY = 52, # Pelvis secondary or undergarment
    SBP_53_MOD_LEG_RIGHT = 53,      # Leg primary or outergarment or right leg
    SBP_54_MOD_LEG_LEFT = 54,       # Leg secondary or undergarment or left leg
    SBP_55_MOD_FACE_JEWELRY = 55,   # Face alternate or jewelry
    SBP_56_MOD_CHEST_SECONDARY = 56,# Chest secondary or undergarment
    SBP_57_MOD_SHOULDER = 57,       # Shoulder
    SBP_58_MOD_ARM_LEFT = 58,       # Arm secondary or undergarment or left arm
    SBP_59_MOD_ARM_RIGHT = 59,      # Arm primary or outergarment or right arm
    SBP_60_MOD_MISC2 = 60,          # Anything that does not fit in the list
    SBP_61_FX01 = 61,               # Skyrim, FX01(Humanoid)
    BP_SECTIONCAP_HEAD = 101,       # Section Cap | Head
    BP_SECTIONCAP_HEAD2 = 102,      # Section Cap | Head 2
    BP_SECTIONCAP_LEFTARM = 103,    # Section Cap | Left Arm
    BP_SECTIONCAP_LEFTARM2 = 104,   # Section Cap | Left Arm 2
    BP_SECTIONCAP_RIGHTARM = 105,   # Section Cap | Right Arm
    BP_SECTIONCAP_RIGHTARM2 = 106,  # Section Cap | Right Arm 2
    BP_SECTIONCAP_LEFTLEG = 107,    # Section Cap | Left Leg
    BP_SECTIONCAP_LEFTLEG2 = 108,   # Section Cap | Left Leg 2
    BP_SECTIONCAP_LEFTLEG3 = 109,   # Section Cap | Left Leg 3
    BP_SECTIONCAP_RIGHTLEG = 110,   # Section Cap | Right Leg
    BP_SECTIONCAP_RIGHTLEG2 = 111,  # Section Cap | Right Leg 2
    BP_SECTIONCAP_RIGHTLEG3 = 112,  # Section Cap | Right Leg 3
    BP_SECTIONCAP_BRAIN = 113,      # Section Cap | Brain
    SBP_130_HEAD = 130,             # Skyrim, Head slot, use on full-face helmets
    SBP_131_HAIR = 131,             # Skyrim, Hair slot 1, use on hoods
    SBP_141_LONGHAIR = 141,         # Skyrim, Hair slot 2, use for longer hair
    SBP_142_CIRCLET = 142,          # Skyrim, Circlet slot 1, use for circlets
    SBP_143_EARS = 143,             # Skyrim, Ear slot
    SBP_150_DECAPITATEDHEAD = 150,  # Skyrim, neck gore on head side
    BP_TORSOCAP_HEAD = 201,         # Torso Cap | Head
    BP_TORSOCAP_HEAD2 = 202,        # Torso Cap | Head 2
    BP_TORSOCAP_LEFTARM = 203,      # Torso Cap | Left Arm
    BP_TORSOCAP_LEFTARM2 = 204,     # Torso Cap | Left Arm 2
    BP_TORSOCAP_RIGHTARM = 205,     # Torso Cap | Right Arm
    BP_TORSOCAP_RIGHTARM2 = 206,    # Torso Cap | Right Arm 2
    BP_TORSOCAP_LEFTLEG = 207,      # Torso Cap | Left Leg
    BP_TORSOCAP_LEFTLEG2 = 208,     # Torso Cap | Left Leg 2
    BP_TORSOCAP_LEFTLEG3 = 209,     # Torso Cap | Left Leg 3
    BP_TORSOCAP_RIGHTLEG = 210,     # Torso Cap | Right Leg
    BP_TORSOCAP_RIGHTLEG2 = 211,    # Torso Cap | Right Leg 2
    BP_TORSOCAP_RIGHTLEG3 = 212,    # Torso Cap | Right Leg 3
    BP_TORSOCAP_BRAIN = 213,        # Torso Cap | Brain
    SBP_230_HEAD = 230,             # Skyrim, Head slot, use for neck on character head
    BP_TORSOSECTION_HEAD = 1000,    # Torso Section | Head
    BP_TORSOSECTION_HEAD2 = 2000,   # Torso Section | Head 2
    BP_TORSOSECTION_LEFTARM = 3000, # Torso Section | Left Arm
    BP_TORSOSECTION_LEFTARM2 = 4000,# Torso Section | Left Arm 2
    BP_TORSOSECTION_RIGHTARM = 5000,# Torso Section | Right Arm
    BP_TORSOSECTION_RIGHTARM2 = 6000, # Torso Section | Right Arm 2
    BP_TORSOSECTION_LEFTLEG = 7000, # Torso Section | Left Leg
    BP_TORSOSECTION_LEFTLEG2 = 8000,# Torso Section | Left Leg 2
    BP_TORSOSECTION_LEFTLEG3 = 9000,# Torso Section | Left Leg 3
    BP_TORSOSECTION_RIGHTLEG = 10000, # Torso Section | Right Leg
    BP_TORSOSECTION_RIGHTLEG2 = 11000, # Torso Section | Right Leg 2
    BP_TORSOSECTION_RIGHTLEG3 = 12000, # Torso Section | Right Leg 3
    BP_TORSOSECTION_BRAIN = 13000   # Torso Section | Brain

# Values for configuring the shader type in a BSLightingShaderProperty
class BSLightingShaderPropertyShaderType(Enum):
    Default = 0,
    Environment_Map = 1,            # Enables EnvMap Mask(TS6), EnvMap Scale
    Glow_Shader = 2,                # Enables Glow(TS3)
    Parallax = 3,                   # Enables Height(TS4)
    Face_Tint = 4,                  # Enables Detail(TS4), Tint(TS7)
    Skin_Tint = 5,                  # Enables Skin Tint Color
    Hair_Tint = 6,                  # Enables Hair Tint Color
    Parallax_Occ = 7,               # Enables Height(TS4), Max Passes, Scale. Unimplemented.
    Multitexture_Landscape = 8,
    LOD_Landscape = 9,
    Snow = 10,
    MultiLayer_Parallax = 11,       # Enables EnvMap Mask(TS6), Layer(TS7), Parallax Layer Thickness, Parallax Refraction Scale, Parallax Inner Layer U Scale, Parallax Inner Layer V Scale, EnvMap Scale
    Tree_Anim = 12,
    LOD_Objects = 13,
    Sparkle_Snow = 14,              # Enables SparkleParams
    LOD_Objects_HD = 15,
    Eye_Envmap = 16,                # Enables EnvMap Mask(TS6), Eye EnvMap Scale
    Cloud = 17,
    LOD_Landscape_Noise = 18,
    Multitexture_Landscape_LOD_Blend = 19,
    FO4_Dismemberment = 20

# An unsigned 32-bit integer, describing which float variable in BSEffectShaderProperty to animate.
class EffectShaderControlledVariable(Enum):
    EmissiveMultiple = 0,           # EmissiveMultiple.
    Falloff_Start_Angle = 1,        # Falloff Start Angle (degrees).
    Falloff_Stop_Angle = 2,         # Falloff Stop Angle (degrees).
    Falloff_Start_Opacity = 3,      # Falloff Start Opacity.
    Falloff_Stop_Opacity = 4,       # Falloff Stop Opacity.
    Alpha_Transparency = 5,         # Alpha Transparency (Emissive alpha?).
    U_Offset = 6,                   # U Offset.
    U_Scale = 7,                    # U Scale.
    V_Offset = 8,                   # V Offset.
    V_Scale = 9                     # V Scale.

# An unsigned 32-bit integer, describing which color in BSEffectShaderProperty to animate.
class EffectShaderControlledColor(Enum):
    Emissive_Color = 0              # Emissive Color.

# An unsigned 32-bit integer, describing which float variable in BSLightingShaderProperty to animate.
class LightingShaderControlledVariable(Enum):
    Refraction_Strength = 0,        # The amount of distortion.
    Environment_Map_Scale = 8,      # Environment Map Scale.
    Glossiness = 9,                 # Glossiness.
    Specular_Strength = 10,         # Specular Strength.
    Emissive_Multiple = 11,         # Emissive Multiple.
    Alpha = 12,                     # Alpha.
    U_Offset = 20,                  # U Offset.
    U_Scale = 21,                   # U Scale.
    V_Offset = 22,                  # V Offset.
    V_Scale = 23                    # V Scale.

# An unsigned 32-bit integer, describing which color in BSLightingShaderProperty to animate.
class LightingShaderControlledColor(Enum):
    Specular_Color = 0,             # Specular Color.
    Emissive_Color = 1              # Emissive Color.

# Bethesda Havok. Describes the type of bhkConstraint.
class hkConstraintType(Enum):
    BallAndSocket = 0,              # A ball and socket constraint.
    Hinge = 1,                      # A hinge constraint.
    Limited_Hinge = 2,              # A limited hinge constraint.
    Prismatic = 6,                  # A prismatic constraint.
    Ragdoll = 7,                    # A ragdoll constraint.
    StiffSpring = 8,                # A stiff spring constraint.
    Malleable = 13                  # A malleable constraint.

#endregion

#region Compounds

# SizedString -> r.readL32AString()
# string -> Y.string(r)
# ByteArray -> r.readL8Bytes()
# ByteMatrix -> ??
# Color3 -> Color3(r)
# ByteColor3 -> ByteColor3(r)
# Color4 -> Color4(r)
# ByteColor4 -> Color4Byte(r)
# FilePath -> ??

# The NIF file footer.
class Footer:
    def __init__(self, r: Reader, h: Header):
        self.roots: list[int] = r.readL32FArray(X[NiObject].ref) if h.v >= 0x0303000D else None # List of root NIF objects. If there is a camera, for 1st person view, then this NIF object is referred to as well in this list, even if it is not a root object (usually we want the camera to be attached to the Bip Head node).

# The distance range where a specific level of detail applies.
class LODRange:
    def __init__(self, r: Reader, h: Header):
        self.nearExtent: float = r.readSingle()         # Begining of range.
        self.farExtent: float = r.readSingle()          # End of Range.
        self.unknownInts: list[int] = r.readPArray(None, 'I', 3) if h.v <= 0x03010000 else None # Unknown (0,0,0).

# Group of vertex indices of vertices that match.
class MatchGroup:
    def __init__(self, r: Reader):
        self.vertexIndices: list[int] = r.readL16PArray(None, 'H') # The vertex indices.

# ByteVector3 -> Vector3(r.readByte(), r.readByte(), r.readByte())
# HalfVector3 -> Vector3(r.readHalf(), r.readHalf(), r.readHalf())
# Vector3 -> r.readVector3()
# Vector4 -> r.readVector4()
# Quaternion -> r.readQuaternion()
# hkQuaternion -> r.readQuaternionWFirst()
# Matrix22 -> r.readMatrix2x2()
# Matrix33 -> r.readMatrix3x3()
# Matrix34 -> r.readMatrix3x4()
# Matrix44 -> r.readMatrix4x4()
# hkMatrix3 -> r.readMatrix3x4()
# MipMap -> MipMap(r)
# NodeSet -> NodeSet(r)
# ShortString -> r.readL8AString()

# NiBoneLODController::SkinInfo. Reference to shape and skin instance.
class SkinInfo:
    def __init__(self, r: Reader):
        self.shape: int = X[NiTriBasedGeom].ptr(r)
        self.skinInstance: int = X[NiSkinInstance].ref(r)

# A set of NiBoneLODController::SkinInfo.
class SkinInfoSet:
    def __init__(self, r: Reader):
        self.skinInfo: list[SkinInfo] = r.readL32FArray(lambda r: SkinInfo(r))

# NiSkinData::BoneVertData. A vertex and its weight.
class BoneVertData:
    struct = ('<Hf', 7)
    index: int                                          # The vertex index, in the mesh.
    weight: float                                       # The vertex weight - between 0.0 and 1.0

    def __init__(self, r: Reader, full: bool):
        self.index = r.readUInt16()
        self.weight = r.readSingle()

    def __init__(self, r: Reader, full: bool):
        self.index = r.readUInt16()
        self.weight = r.readSingle() if full else r.readHalf()
        
# BoneVertDataHalf -> BoneVertData(r, true)

# Used in NiDefaultAVObjectPalette.
class AVObject:
    def __init__(self, r: Reader):
        self.name: str = r.readL32AString()             # Object name.
        self.avObject: int = X[NiAVObject].ptr(r)       # Object reference.

# In a .kf file, this links to a controllable object, via its name (or for version 10.2.0.0 and up, a link and offset to a NiStringPalette that contains the name), and a sequence of interpolators that apply to this controllable object, via links.
# For Controller ID, NiInterpController::GetCtlrID() virtual function returns a string formatted specifically for the derived type.
# For Interpolator ID, NiInterpController::GetInterpolatorID() virtual function returns a string formatted specifically for the derived type.
# The string formats are documented on the relevant niobject blocks.
class ControlledBlock:
    targetName: str                                     # Name of a controllable object in another NIF file.
    # NiControllerSequence::InterpArrayItem
    interpolator: int
    controller: int
    blendInterpolator: int
    blendIndex: int
    # Bethesda-only
    priority: int                                       # Idle animations tend to have low values for this, and high values tend to correspond with the important parts of the animations.
    # NiControllerSequence::IDTag, post-10.1.0.104 only
    nodeName: str           # The name of the animated NiAVObject.
    propertyType: str      # The RTTI type of the NiProperty the controller is attached to, if applicable.
    controllerType: str    # The RTTI type of the NiTimeController.
    controllerID: str      # An ID that can uniquely identify the controller among others of the same type on the same NiObjectNET.
    interpolatorID: str    # An ID that can uniquely identify the interpolator among others of the same type on the same NiObjectNET.

    def __init__(self, r: Reader, h: Header):
        if h.v <= 0x0A010067: self.targetName = Y.string(r)
        if h.v <= 0x14050000: self.interpolator = X[NiInterpolator].ref(r)
        self.controller = X[NiTimeController].ref(r)
        if h.v >= 0x0A010068 and h.v <= 0x0A01006E:
            self.blendInterpolator = X[NiBlendInterpolator].ref(r)
            self.blendIndex = r.readUInt16()
        if h.v <= 0x0A01006A and h.userVersion2 > 0: self.priority = r.readByte()
        # Until 10.2
        if h.v >= 0x0A010068 and h.v <= 0x0A010071:
            self.nodeName = Y.string(r)
            self.propertyType = Y.string(r)
            self.controllerType = Y.string(r)
            self.controllerID = Y.string(r)
            self.interpolatorID = Y.string(r)
        # From 10.2 to 20.1
        elif h.v >= 0x0A020000 and h.v <= 0x14010000:
            stringPalette = X[NiStringPalette].ref(r)
            self.nodeName = Y.stringRef(r, stringPalette)
            self.propertyType = Y.stringRef(r, stringPalette)
            self.controllerType = Y.stringRef(r, stringPalette)
            self.controllerID = Y.stringRef(r, stringPalette)
            self.interpolatorID = Y.stringRef(r, stringPalette)
        # After 20.1
        elif h.v >= 0x14010001:
            self.nodeName = Y.string(r)
            self.propertyType = Y.string(r)
            self.controllerType = Y.string(r)
            self.controllerID = Y.string(r)
            self.interpolatorID = Y.string(r)

# Information about how the file was exported
class ExportInfo:
    def __init__(self, r: Reader):
        self.author: str = r.readL8AString()
        self.processScript: str = r.readL8AString()
        self.exportScript: str = r.readL8AString()

# The NIF file header.
class Header:
    v: int
    headerString: str
    copyright: list[str]
    version: int
    endianType: EndianType
    userVersion: int
    numBlocks: int
    userVersion2: int
    exportInfo: list[str]
    maxFilePath: str
    metadata: bytes
    blockTypes: list[str]
    blockTypeHashes: list[int]
    blockTypeIndex: list[int]
    blockSize: list[int]
    strings: list[str]
    groups: list[int]

    def __init__(self, r: Reader):
        (self.headerString, self.v) = Header.parseHeaderStr(r.readVAString(128, b'\x0A')); v = self.v
        if v < 0x03010000: self.copyright = [r.readL8AString(), r.readL8AString(), r.readL8AString()]
        self.version: int = r.readUInt32() if v >= 0x03010001 else 0x04000002
        self.endianType = EndianType(r.readByte()) if v >= 0x14000003 else 0
        if v >= 0x0A000108: self.userVersion: int = r.readUInt32()
        if v >= 0x03010001: self.numBlocks: int = r.readUInt32()
        if (v == 0x14020007 or v == 0x14000005 or (v >= 0x0A000102 and v <= 0x14000004 and self.userVersion <= 11)) and self.userVersion >= 3:
            self.userVersion2 = r.readUInt32()
            self.exportInfo = [r.readL8AString(), r.readL8AString(), r.readL8AString()]
        if self.userVersion2 == 130: self.maxFilePath = r.readL8AString(0x80)
        if v >= 0x1E000000: self.metadata = r.readL8Bytes()
        if v >= 0x05000001:
            numBlockTypes = r.readUInt16() # Number of object types in this NIF file.
            if v != 0x14030102: self.blockTypes = r.readFArray(lambda r: r.readL32AString(), numBlockTypes)
            else: self.blockTypeHashes = r.readFArray(lambda r: r.readUInt32(r), numBlockTypes)
            self.blockTypeIndex = r.readFArray(lambda r: r.readUInt16(), self.numBlocks)
        if v >= 0x14020005: self.blockSize = r.readFArray(lambda r: r.readUInt32(), numBlocks)
        if v >= 0x14010001:
            numStrings = r.readUInt32() # Number of strings.
            maxStringLength = r.readUInt32() # Maximum string length.
            self.strings = r.readFArray(lambda r: r.readL32AString(maxStringLength), numStrings)
        if v >= 0x05000006: self.groups = r.readL32PArray(None, 'I')

class StringPalette: #:X
    def __init__(self, r: Reader):
        self.palette: list[str] = r.readL32AString().split('0x00') # A bunch of 0x00 seperated strings.
        self.length: int = r.readUInt32()       # Length of the palette string is repeated here.

# Tension, bias, continuity.
class TBC:
    struct = ('<3f', 12)
    t: float                                            # Tension.
    b: float                                            # Bias.
    c: float                                            # Continuity.

    def __init__(self, r: Reader):
        self.t = r.readSingle()
        self.b = r.readSingle()
        self.c = r.readSingle()

# A generic key with support for interpolation. Type 1 is normal linear interpolation, type 2 has forward and backward tangents, and type 3 has tension, bias and continuity arguments. Note that color4 and byte always seem to be of type 1.
class Key: #:M
    time: float                 # Time of the key.
    value: object               # The key value.
    forward: object             # Key forward tangent.
    backward: object            # The key backward tangent.    
    tbc: TBC                    # The TBC of the key.

    def __init__(self, T: type, r: Reader, keyType: KeyType):
        self.time = r.readSingle()
        self.value = X[T].read(r)
        if keyType == KeyType.QUADRATIC_KEY:
            self.forward = X[T].read(r)
            self.backward = X[T].read(r)
        elif keyType == KeyType.TBC_KEY: self.tbc: TBC = TBC(r)

# Array of vector keys (anything that can be interpolated, except rotations).
class KeyGroup: #:M
    numKeys: int                # Number of keys in the array.
    interpolation: KeyType      # The key type.
    keys: list[Key]             # The keys.

    def __init__(self, T: type, r: Reader):
        self.numKeys = r.readUInt32()
        if self.numKeys != 0: self.interpolation = KeyType(r.readUInt32())
        self.keys = r.readFArray(lambda r: Key(r, self.interpolation), self.numKeys)

# A special version of the key type used for quaternions.  Never has tangents.
class QuatKey: #:M
    time: float                 # Time the key applies.
    value: object               # Value of the key.
    tbc: TBC                    # The TBC of the key.

    def __init__(self, T: type, r: Reader, keyType: KeyType):
        self.time = r.readSingle()
        if keyType != KeyType.XYZ_ROTATION_KEY: self.value = X[T].read(r)
        if keyType == KeyType.TBC_KEY: self.tbc = TBC(r)

# Texture coordinates (u,v). As in OpenGL; image origin is in the lower left corner.
class TexCoord: #:M
    u: float                    # First coordinate.
    v: float                    # Second coordinate.

    def __init__(self, r, v):
        if not v: self.u = r.readSingle(); self.v = r.readSingle()
        else: self.u = r; self.v = v
    @classmethod
    def init2(cls, r: Reader, full: bool): return cls(a.readSingle(), a.readSingle()) if full else cls(a.readHalf(), a.readHalf())
# use:HalfTexCoord -> TexCoord

# Describes the order of scaling and rotation matrices. Translate, Scale, Rotation, Center are from TexDesc.
# Back = inverse of Center. FromMaya = inverse of the V axis with a positive translation along V of 1 unit.
class TransformMethod(Enum): #:X
    MayaDeprecated = 0,             # Center * Rotation * Back * Translate * Scale
    Max = 1,                        # Center * Scale * Rotation * Translate * Back
    Maya = 2                        # Center * Rotation * Back * FromMaya * Translate * Scale

# NiTexturingProperty::Map. Texture description.
class TexDesc: #:M
    image: int
    source: int
    clampMode: TexClampMode
    filterMode: TexFilterMode
    maxAnisotropy: int
    uvSet: int
    ps2L: int
    ps2K: int
    unknown1: int
    # NiTextureTransform
    hasTextureTransform: bool
    translation: TexCoord
    scale: TexCoord
    rotation: float
    transformMethod: TransformMethod
    center: TexCoord

    def __init__(self, r: Reader, h: Header):
        if h.v <= 0x03010000: self.image = X[NiImage].ref(r)
        if h.v <= 0x0303000D: self.source = X[NiSourceTexture].ref(r)
        self.clampMode = TexClampMode(r.readUInt32()) if h.v <= 0x14000005 else TexClampMode.WRAP_S_WRAP_T
        self.filterMode = TexFilterMode(r.readUInt32()) if h.v <= 0x14000005 else TexFilterMode.FILTER_TRILERP
        self.maxAnisotropy = r.readUInt16() if h.v <= 0x14050004 else 0
        self.uvSet = r.readUInt32() if h.v <= 0x14000005 else 0
        self.ps2L = r.readInt16() if h.v <= 0x14000005 else 0
        self.ps2K = r.readInt16() if h.v <= 0x14000005 else -75
        if h.v >= 0x0401010C: self.unknown1 = r.readUInt16()
        # NiTextureTransform
        self.hasTextureTransform = h.v >= 0x0A010000 and r.readBool32()
        if not self.hasTextureTransform: self.scale = TexCoord(1., 1.); return
        self.translation = TexCoord(r)
        self.scale = TexCoord(r)
        self.rotation = r.readSingle()
        self.transformMethod = TransformMethod(r.readUInt32())
        self.center = TexCoord(r)

# NiTexturingProperty::ShaderMap. Shader texture description.
class TexDesc: #:X
    map: TexDesc
    mapID: int

    def __init__(self, r: Reader, h: Header):
        if not r.readBool32(): return
        self.map = TexDesc(r, h)
        self.mapID = r.readUInt32()

# List of three vertex indices.
class Triangle: #:M
    def __init__(self, r: Reader):
        self.v1: int = r.readUInt16()   # First vertex index.
        self.v2: int = r.readUInt16()   # Second vertex index.
        self.v3: int = r.readUInt16()   # Third vertex index.

class VertexFlags(Flag): #:X
    # First 4 bits are unused
    Vertex = 1 << 4,                    # & 16
    UVs = 1 << 5,                       # & 32
    UVs_2 = 1 << 6,                     # & 64
    Normals = 1 << 7,                   # & 128
    Tangents = 1 << 8,                  # & 256
    Vertex_Colors = 1 << 9,             # & 512
    Skinned = 1 << 10,                  # & 1024
    Land_Data = 1 << 11,                # & 2048
    Eye_Data = 1 << 12,                 # & 4096
    Instance = 1 << 13,                 # & 8192
    Full_Precision = 1 << 14            # & 16384
    # FLast bit unused

# NiTexturingProperty::Map. Texture description.
class BSVertexData: #:M
    vertex: int
    bitangentX: int
    uknownInt: TexClampMode
    uv: TexFilterMode
    normal: int
    bitangentY: int
    tangent: int
    bitangentZ: int
    vertexColors: int
    boneWeights: bool
    boneIndices: TexCoord
    eyeData: float

    def __init__(self, r: Reader, h: Header, arg: VertexFlags, sse: bool):
        full = sse or VertexFlags.Full_Precision in arg
        tangents = VertexFlags.Tangents in arg
        if VertexFlags.Vertex in arg:
            self.vertex = r.readVector3() if full else r.readHalfVector3()
            if tangents: self.bitangentX = r.readSingle() if full else r.readHalf()
            else: self.unknownInt = r.readUInt32() if full else r.readUInt16()
        if VertexFlags.UVs in arg: UV = TexCoord(r, false)
        if VertexFlags.Normals in arg:
            self.normal = Vector3[byte](r.readByte(), r.readByte(), r.readByte())
            self.bitangentY = r.readByte()
            if tangents: self.tangent = Vector3[byte](r.readByte(), r.readByte(), r.readByte())
            if tangents: self.bitangentZ = r.readByte()
        if VertexFlags.Vertex_Colors in arg: self.vertexColors = ByteColor4(r)
        if VertexFlags.Skinned in arg:
            self.boneWeights = [r.readHalf(), r.readHalf(), r.readHalf(), r.readHalf()]
            self.boneIndices = [r.readByte(), r.readByte(), r.readByte(), r.readByte()]
        if VertexFlags.Eye_Data in arg: self.eyeData = r.readSingle()
# use:BSVertexDataSSE -> BSVertexData

class BSVertexDesc: #:X #Marshal
    def __init__(self, r: Reader):
        self.vf1: int = r.readByte()
        self.vf2: int = r.readByte()
        self.vf3: int = r.readByte()
        self.vf4: int = r.readByte()
        self.vf5: int = r.readByte()
        self.vertexAttributes: VertexFlags = VertexFlags(r.readUInt16())
        self.vf8: int = r.readByte()

# Skinning data for a submesh, optimized for hardware skinning. Part of NiSkinPartition.
class SkinPartition: #:X
    numVertices: int                # Number of vertices in this submesh.
    numTriangles: int               # Number of triangles in this submesh.
    numBones: int                   # Number of bones influencing this submesh.
    numStrips: int                  # Number of strips in this submesh (zero if not stripped).
    numWeightsPerVertex: int        # Number of weight coefficients per vertex. The Gamebryo engine seems to work well only if this number is equal to 4, even if there are less than 4 influences per vertex.
    bones: list[int]                # List of bones.
    vertexMap: list[int]            # Maps the weight/influence lists in this submesh to the vertices in the shape being skinned.
    vertexWeights: list[list[float]] # The vertex weights.
    stripLengths: int               # The strip lengths.
    strips: list[list[int]]         # The strips.
    triangles: list[Triangle]       # The triangles.
    boneIndices: list[bytearray]    # Bone indices, they index into 'Bones'.
    unknownShort: int               # Unknown
    vertexDesc: BSVertexDesc
    trianglesCopy: list[Triangle]

    def __init__(self, r: Reader, h: Header):
        self.numVertices = r.readUInt16()
        self.numTriangles = self.numVertices / 3 # #calculated?
        self.numBones = r.readUInt16()
        self.numStrips = r.readUInt16()
        self.numWeightsPerVertex = r.readUInt16()
        self.bones = r.readPArray(None, 'H', self.numBones)
        if h.v <= 0x0A000102:
            self.vertexMap = r.readPArray(None, 'H', self.numVertices)
            self.vertexWeights = r.readFArray(lambda r: r.readPArray(None, 'f', self.numWeightsPerVertex), self.numVertices)
            self.stripLengths = r.readUInt16()
            if self.numStrips != 0: self.strips = r.readFArray(lambda r: r.readPArray(None, 'H', self.stripLengths), self.numStrips)
            else: self.triangles = r.readFArray(lambda r: Triangle(r), self.numTriangles)
        elif h.v >= 0x0A010000:
            if r.readBool32(): self.vertexMap = r.readPArray(None, 'H', self.numVertices)
            if (hasVertexWeights := r.ReadUInt32()) != 0:
                self.vertexWeights = r.readFArray(lambda r: r.readPArray(None, 'f', self.numWeightsPerVertex), self.numVertices) if hasVertexWeights == 1 else \
                r.readFArray(lambda r: r.readFArray(lambda k: k.readHalf(), self.numWeightsPerVertex), self.numVertices) if hasVertexWeights == 15 else \
                None
            self.stripLengths = r.readUInt16()
            if r.readBool32():
                if self.numStrips != 0: self.strips = r.readFArray(lambda r: r.readPArray(None, 'H', self.stripLengths), self.numStrips)
                else: self.triangles = r.readFArray(lambda r: Triangle(r), self.numTriangles)
        if r.readBool32(): self.boneIndices = r.readFArray(lambda r: r.readBytes(self.numWeightsPerVertex), self.numVertices)
        if h.userVersion2 > 34: self.unknownShort = r.readUInt16()
        if h.v >= 0x14020007 and h.userVersion <= 100:
            self.vertexDesc = BSVertexDesc(r)
            self.trianglesCopy = r.readFArray(lambda r: Triangle(r), self.numTriangles)

# A plane.
class NiPlane: #:X #Marshal
    def __init__(self, r: Reader):
        self.normal: Vector3 = r.readVector3()          # The plane normal.
        self.constant: float = r.readSingle()           # The plane constant.

# A sphere.
class NiBound: #:X #Marshal
    def __init__(self, r: Reader):
        self.center: Vector3 = r.readVector3()          # The sphere's center.
        self.radius: float = r.readSingle()             # The sphere's radius.

class NiQuatTransform: #:X
    def __init__(self, r: Reader):
        self.translation: Vector3 = r.readVector3()
        self.rotation: float = r.readQuaternion()
        self.scale: float = r.readSingle()
        self.trsValid: list[bool] = [r.readBool32(), r.readBool32(), r.readBool32()] if h.v <= 0x0A01006D else [true, true, true] # Whether each transform component is valid.

class NiTransform: #:M #Marshal
    def __init__(self, r: Reader):
        self.rotation: Matrix4x4 = r.readMatrix3x3As4x4()
        self.translation: Vector3 = r.readVector3()
        self.scale: float = r.readSingle()

# Bethesda Animation. Furniture entry points. It specifies the direction(s) from where the actor is able to enter (and leave) the position.
class FurnitureEntryPoints(Flag): #:X
    Front = 0,                      # front entry point
    Behind = 1 << 1,                # behind entry point
    Right = 1 << 2,                 # right entry point
    Left = 1 << 3,                  # left entry point
    Up = 1 << 4                     # up entry point - unknown function. Used on some beds in Skyrim, probably for blocking of sleeping position.
    
# Bethesda Animation. Animation type used on this position. This specifies the function of this position.
class AnimationType(Enum): #:X
    # First 4 bits are unused
    Sit = 1,                        # Actor use sit animation.
    Sleep = 2,                      # Actor use sleep animation.
    Lean = 4                        # Used for lean animations?

# Bethesda Animation. Describes a furniture position?
class FurniturePosition: #:X
    offset: Vector3                 # Offset of furniture marker.
    orientation: int                # Furniture marker orientation.
    positionRef1: int               # Refers to a furnituremarkerxx.nif file. Always seems to be the same as Position Ref 2.
    positionRef2: int               # Refers to a furnituremarkerxx.nif file. Always seems to be the same as Position Ref 1.
    heading: float                  # Similar to Orientation, in float form.
    animationType: AnimationType    # Unknown
    entryProperties: FurnitureEntryPoints # Unknown/unused in nif?

    def __init__(self, r: Reader, h: Header):
        self.offset = r.readVector3()
        if h.userVersion2 <= 34:
            self.orientation = r.readUInt16()
            self.positionRef1 = r.readByte()
            self.positionRef2 = r.readByte()
        else:
            self.heading = r.readSingle()
            self.animationType = AnimationType(r.readUInt16())
            self.entryProperties = FurnitureEntryPoints(r.readUInt16())

# Bethesda Havok. A triangle with extra data used for physics.
class TriangleData: #:X
    def __init__(self, r: Reader, h: Header):
        self.triangle: Triangle = Triangle(r)           # The triangle.
        self.weldingInfo: int = r.readUInt16()          # Additional havok information on how triangles are welded.
        self.normal: Vector3 = r.readVector3() if h.v <= 0x14000005 else None # This is the triangle's normal.

# Geometry morphing data component.
class Morph: #:M
    frameName: str                  # Name of the frame.
    interpolation: KeyType          # Unlike most objects, the presense of this value is not conditional on there being keys.
    keys: list[Key]                 # The morph key frames.
    legacyWeight: float
    vectors: list[Vector3]          # Morph vectors.

    def __init__(self, r: Reader, numVertices: int):
        if h.v >= 0x0A01006A: self.frameName = Y.string(r)
        if h.v <= 0x0A010000:
            numKeys = r.readUInt32()
            self.interpolation = KeyType(r.readUInt32())
            self.keys = r.readFArray(lambda r: Key(r, self.interpolation), numKeys)
        if h.v >= 0x0A010068 and h.v <= 0x14010002: self.legacyWeight = r.readSingle()
        self.vectors = r.readFArray(lambda r: r.readVector3(), numVertices)

# particle array entry
class Particle: #:M #Marshal
    def __init__(self, r: Reader):
        self.velocity: Vector3 = r.readVector3()            # Particle velocity
        self.unknownVector: Vector3 = r.readVector3()       # Unknown
        self.lifetime: float = r.readSingle()               # The particle age.
        self.lifespan: float = r.readSingle()               # Maximum age of the particle.
        self.timestamp: float = r.readSingle()              # Timestamp of the last update.
        self.unknownShort: int = r.readUInt16()             # Unknown short
        self.vertexId: int = r.readUInt16()                 # Particle/vertex index matches array index

# NiSkinData::BoneData. Skinning data component.
class BoneData: #:M
    skinTransform: NiTransform                              # Offset of the skin from this bone in bind position.
    boundingSphereOffset: Vector3                           # Translation offset of a bounding sphere holding all vertices. (Note that its a Sphere Containing Axis Aligned Box not a minimum volume Sphere)
    boundingSphereRadius: float                             # Radius for bounding sphere holding all vertices.
    unknown13Shorts: list[int]                              # Unknown, always 0?
    vertexWeights: list[BoneVertData]                       # The vertex weights.

    def __init__(self, r: Reader):
        self.skinTransform = NiTransform(r) 
        self.boundingSphereOffset = r.readVector3()
        self.boundingSphereRadius = r.readSingle()
        if h.v == 0x14030009 and (h.userVersion == 0x20000 or h.userVersion == 0x30000): self.unknown13Shorts = r.readPArray(None, 'H', 13)
        self.vertexWeights = r.readL16SArray(BoneVertData) if h.v <= 0x04020100 else \
            r.readL16SArray(BoneVertData) if h.v <= 0x04020200 and arg == 1 else \
            r.readL16FArray(lambda r: BoneVertData(r, false)) if h.v <= 0x14030101 and arg == 15 else \
            None
            
# Bethesda Havok. Collision filter info representing Layer, Flags, Part Number, and Group all combined into one uint.
class HavokFilter: #:X
    def __init__(self, r: Reader, h: Header):
        self.layer_OB: OblivionLayer = OblivionLayer(r.readByte()) if h.v <= 0x14000005 and h.userVersion2 < 16 else OblivionLayer.OL_STATIC    # The layer the collision belongs to.
        self.layer_FO: Fallout3Layer = Fallout3Layer(r.readByte()) if h.v == 0x14002007 and h.userVersion2 <= 34 else Fallout3Layer.FOL_STATIC  # The layer the collision belongs to.
        self.layer_SK: SkyrimLayer = SkyrimLayer(r.readByte()) if h.v == 0x14002007 and h.userVersion2 > 34 else SkyrimLayer.SKYL_STATIC        # The layer the collision belongs to.
        self.flagsAndPartNumber: int = r.readByte()                                                                                             # FLAGS are stored in highest 3 bits:
        self.group: int = r.readUInt16()

# Bethesda Havok. Material wrapper for varying material enums by game.
class HavokMaterial: #:X
    def __init__(self, r: Reader, h: Header):
        self.unknownInt: int = r.readUInt32() if h.v <= 0x0A000102 else 0
        self.material_OB: OblivionHavokMaterial = OblivionHavokMaterial(r.readUInt32()) if h.v <= 0x14000005 and h.userVersion2 < 16 else 0     # The material of the shape.
        self.material_FO: Fallout3HavokMaterial = Fallout3HavokMaterial(r.readUInt32()) if h.v == 0x14002007 and h.userVersion2 <= 34 else 0    # The material of the shape.
        self.material_SK: SkyrimHavokMaterial = SkyrimHavokMaterial(r.readUInt32()) if h.v == 0x14002007 and h.userVersion2 > 34 else 0         # The material of the shape.

# Bethesda Havok. Havok Information for packed TriStrip shapes.
class OblivionSubShape: #:X
    def __init__(self, r: Reader, h: Header):
        self.havokFilter: HavokFilter = HavokFilter(r, h)
        self.numVertices: int = r.readUInt32()                              # The number of vertices that form this sub shape.
        self.material: HavokMaterial = HavokMaterial(r, h)                  # The material of the subshape.

class bhkPositionConstraintMotor: #:X #Marshal
    def __init__(self, r: Reader):
        self.minForce: float = r.readSingle()           # Minimum motor force
        self.maxForce: float = r.readSingle()           # Maximum motor force
        self.tau: float = r.readSingle()                # Relative stiffness
        self.damping: float = r.readSingle()            # Motor damping value
        self.proportionalRecoveryVelocity: float = r.readSingle() # A factor of the current error to calculate the recovery velocity
        self.constantRecoveryVelocity: float = r.readSingle() # A constant velocity which is used to recover from errors
        self.motorEnabled: bool = r.readBool32()        # Is Motor enabled

class bhkVelocityConstraintMotor: #:X #Marshal
    def __init__(self, r: Reader):
        self.minForce: float = r.readSingle()           # Minimum motor force
        self.maxForce: float = r.readSingle()           # Maximum motor force
        self.tau: float = r.readSingle()                # Relative stiffness
        self.targetVelocity: float = r.readSingle()
        self.useVelocityTarget: bool = r.readBool32()
        self.motorEnabled: bool = r.readBool32()        # Is Motor enabled

class bhkSpringDamperConstraintMotor: #:X #Marshal
    def __init__(self, r: Reader):
        self.minForce: float = r.readSingle()           # Minimum motor force
        self.maxForce: float = r.readSingle()           # Maximum motor force
        self.springConstant: float = r.readSingle()     # The spring constant in N/m
        self.springDamping: float = r.readSingle()      # The spring damping in Nsec/m
        self.motorEnabled: bool = r.readBool32()        # Is Motor enabled

class MotorType(Enum): #:X
    MOTOR_NONE = 0,
    MOTOR_POSITION = 1,
    MOTOR_VELOCITY = 2,
    MOTOR_SPRING = 3

class MotorDescriptor: #:X
    type: MotorType
    positionMotor: bhkPositionConstraintMotor
    velocityMotor: bhkVelocityConstraintMotor
    springDamperMotor: bhkSpringDamperConstraintMotor

    def __init__(self, r: Reader):
        self.type = MotorType(r.readByte())
        match self.type:
            case MotorType.MOTOR_POSITION: self.positionMotor = bhkPositionConstraintMotor(r)
            case MotorType.MOTOR_VELOCITY: self.velocityMotor = bhkVelocityConstraintMotor(r)
            case MotorType.MOTOR_SPRING: self.springDamperMotor = bhkSpringDamperConstraintMotor(r)

# This constraint defines a cone in which an object can rotate. The shape of the cone can be controlled in two (orthogonal) directions.
class RagdollDescriptor: #:X
    twistA: Vector4                 # Central directed axis of the cone in which the object can rotate. Orthogonal on Plane A.
    planeA: Vector4                 # Defines the orthogonal plane in which the body can move, the orthogonal directions in which the shape can be controlled (the direction orthogonal on this one and Twist A).
    motorA: Vector4                 # Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    pivotA: Vector4                 # Point around which the object will rotate. Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    twistB: Vector4                 # Central directed axis of the cone in which the object can rotate. Orthogonal on Plane B.
    planeB: Vector4                 # Defines the orthogonal plane in which the body can move, the orthogonal directions in which the shape can be controlled (the direction orthogonal on this one and Twist A).
    motorB: Vector4                 # Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    pivotB: Vector4                 # Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    coneMaxAngle: float             # Maximum angle the object can rotate around the vector orthogonal on Plane A and Twist A relative to the Twist A vector. Note that Cone Min Angle is not stored, but is simply minus this angle.
    planeMinAngle: float            # Minimum angle the object can rotate around Plane A, relative to Twist A.
    planeMaxAngle: float            # Maximum angle the object can rotate around Plane A, relative to Twist A.
    twistMinAngle: float            # Minimum angle the object can rotate around Twist A, relative to Plane A.
    twistMaxAngle: float            # Maximum angle the object can rotate around Twist A, relative to Plane A.
    maxFriction: float              # Maximum friction, typically 0 or 10. In Fallout 3, typically 100.
    motor: MotorDescriptor

    def __init__(self, r: Reader, h: Header):
        # Oblivion and Fallout 3, Havok 550
        if h.userVersion2 <= 16:
            self.pivotA = r.readVector4()
            self.planeA = r.readVector4()
            self.twistA = r.readVector4()
            self.pivotB = r.readVector4()
            self.planeB = r.readVector4()
            self.twistB = r.readVector4()
        # Fallout 3 and later, Havok 660 and 2010
        else:
            self.twistA = r.readVector4()
            self.planeA = r.readVector4()
            self.motorA = r.readVector4()
            self.pivotA = r.readVector4()
            self.twistB = r.readVector4()
            self.planeB = r.readVector4()
            self.motorB = r.readVector4()
            self.pivotB = r.readVector4()
        self.coneMaxAngle = r.readSingle()
        self.planeMinAngle = r.readSingle()
        self.planeMaxAngle = r.readSingle()
        self.twistMinAngle = r.readSingle()
        self.twistMaxAngle = r.readSingle()
        self.maxFriction = r.readSingle()
        if h.v >= 0x14020007 and h.userVersion2 > 16: self.motor = MotorDescriptor(r)

# This constraint allows rotation about a specified axis, limited by specified boundaries.
class LimitedHingeDescriptor: #:X
    axleA: Vector4                          # Axis of rotation.
    perp2AxleInA1: Vector4                  # Vector in the rotation plane which defines the zero angle.
    perp2AxleInA2: Vector4                  # Vector in the rotation plane, orthogonal on the previous one, which defines the positive direction of rotation. This is always the vector product of Axle A and Perp2 Axle In A1.
    pivotA: Vector4                         # Pivot point around which the object will rotate.
    axleB: Vector4                          # Axle A in second entity coordinate system.
    perp2AxleInB1: Vector4                  # Perp2 Axle In A1 in second entity coordinate system.
    perp2AxleInB2: Vector4                  # Perp2 Axle In A2 in second entity coordinate system.
    pivotB: Vector4                         # Pivot A in second entity coordinate system.
    minAngle: float                         # Minimum rotation angle.
    maxAngle: float                         # Maximum rotation angle.
    maxFriction: float                      # Maximum friction, typically either 0 or 10. In Fallout 3, typically 100.
    motor: MotorDescriptor

    def __init__(self, r: Reader, h: Header):
        # Oblivion and Fallout 3, Havok 550
        if h.userVersion2 <= 16:
            self.pivotA = r.readVector4()
            self.axleA = r.readVector4()
            self.perp2AxleInA1 = r.readVector4()
            self.perp2AxleInA2 = r.readVector4()
            self.pivotB = r.readVector4()
            self.axleB = r.readVector4()
            self.perp2AxleInB1 = r.readVector4()
            self.perp2AxleInB2 = r.readVector4()
        # Fallout 3 and later, Havok 660 and 2010
        else:
            self.axleA = r.readVector4()
            self.perp2AxleInA1 = r.readVector4()
            self.perp2AxleInA2 = r.readVector4()
            self.pivotA = r.readVector4()
            self.axleB = r.readVector4()
            self.perp2AxleInB1 = r.readVector4()
            self.perp2AxleInB2 = r.readVector4()
            self.pivotB = r.readVector4()
        self.minAngle = r.readSingle()
        self.maxAngle = r.readSingle()
        self.maxFriction = r.readSingle()
        if h.v >= 0x14020007 and h.userVersion2 > 16: self.motor = MotorDescriptor(r)


# This constraint allows rotation about a specified axis.
class HingeDescriptor: #:X
    axleA: Vector4                          # Axis of rotation.
    perp2AxleInA1: Vector4                  # Vector in the rotation plane which defines the zero angle.
    perp2AxleInA2: Vector4                  # Vector in the rotation plane, orthogonal on the previous one, which defines the positive direction of rotation. This is always the vector product of Axle A and Perp2 Axle In A1.
    pivotA: Vector4                         # Pivot point around which the object will rotate.
    axleB: Vector4                          # Axle A in second entity coordinate system.
    perp2AxleInB1: Vector4                  # Perp2 Axle In A1 in second entity coordinate system.
    perp2AxleInB2: Vector4                  # Perp2 Axle In A2 in second entity coordinate system.
    pivotB: Vector4                         # Pivot A in second entity coordinate system.

    def __init__(self, r: Reader, h: Header):
        # Oblivion
        if h.v <= 0x14000005:
            self.pivotA = r.readVector4()
            self.perp2AxleInA1 = r.readVector4()
            self.perp2AxleInA2 = r.readVector4()
            self.pivotB = r.readVector4()
            self.axleB = r.readVector4()
        # Fallout 3
        elif h.v >= 0x14020007:
            self.axleA = r.readVector4()
            self.perp2AxleInA1 = r.readVector4()
            self.perp2AxleInA2 = r.readVector4()
            self.pivotA = r.readVector4()
            self.axleB = r.readVector4()
            self.perp2AxleInB1 = r.readVector4()
            self.perp2AxleInB2 = r.readVector4()
            self.pivotB = r.readVector4()

class BallAndSocketDescriptor: #:X
    def __init__(self, r: Reader):
        self.pivotA: Vector4 = r.readVector4() # Pivot point in the local space of entity A.
        self.pivotB: Vector4 = r.readVector4() # Pivot point in the local space of entity B.

# In reality Havok loads these as Transform A and Transform B using hkTransform
class PrismaticDescriptor: #:X
    slidingA: Vector4                       # Describes the axis the object is able to travel along. Unit vector.
    rotationA: Vector4                      # Rotation axis.
    planeA: Vector4                         # Plane normal. Describes the plane the object is able to move on.
    pivotA: Vector4                         # Pivot.
    slidingB: Vector4                       # Describes the axis the object is able to travel along in B coordinates. Unit vector.
    rotationB: Vector4                      # Rotation axis.
    planeB: Vector4                         # Plane normal. Describes the plane the object is able to move on in B coordinates.
    pivotB: Vector4                         # Pivot in B coordinates.
    minDistance: float                      # Describe the min distance the object is able to travel.
    maxDistance: float                      # Describe the max distance the object is able to travel.
    friction: float                         # Friction.
    motor: MotorDescriptor

    def __init__(self, r: Reader, h: Header):
        # Oblivion (Order is a guess)
        if h.v <= 0x14000005:
            self.pivotA = r.readVector4()
            self.rotationA = r.readVector4()
            self.planeA = r.readVector4()
            self.slidingA = r.readVector4()
            self.pivotB = r.readVector4()
            self.rotationB = r.readVector4()
            self.planeB = r.readVector4()
            self.slidingB = r.readVector4()
        # Fallout 3
        elif h.v >= 0x14020007:
            self.slidingA = r.readVector4()
            self.rotationA = r.readVector4()
            self.planeA = r.readVector4()
            self.pivotA = r.readVector4()
            self.slidingB = r.readVector4()
            self.rotationB = r.readVector4()
            self.planeB = r.readVector4()
            self.pivotB = r.readVector4()
        self.minDistance = r.readSingle()
        self.maxDistance = r.readSingle()
        self.friction = r.readSingle()
        if h.v >= 0x14020007 and h.userVersion2 > 16: self.motor = MotorDescriptor(r)

class StiffSpringDescriptor: #:X
    def __init__(self, r: Reader):
        self.pivotA: Vector4 = r.readVector4()
        self.pivotB: Vector4 = r.readVector4()
        self.length: float = r.readSingle()

# Used to store skin weights in NiTriShapeSkinController.
class OldSkinData: #:X
    def __init__(self, r: Reader):
        self.vertexWeight: float = r.readSingle()        # The amount that this bone affects the vertex.
        self.vertexIndex: int = r.readUInt16()        # The index of the vertex that this weight applies to.
        self.unknownVector: Vector3 = r.readVector3()    # Unknown.  Perhaps some sort of offset?

# Determines how the raw image data is stored in NiRawImageData.
class ImageType(Enum): #:X
    RGB = 0,                        # Colors store red, blue, and green components.
    RGBA = 1                        # Colors store red, blue, green, and alpha components.

# Box Bounding Volume
class BoxBV: #:X
    def __init__(self, r: Reader):
        self.center: Vector3 = r.readVector3()        #was:Translation
        self.axis: Matrix4x4 = r.readMatrix3x3As4x4() #was:Rotation
        self.extent: Vector3 = r.readVector3()        #was:Radius

# Capsule Bounding Volume
class CapsuleBV: #:X
    def __init__(self, r: Reader):
        self.center: Vector3 = r.readVector3()
        self.origin: Vector3 = r.readVector3()
        self.extent: float = r.readSingle()
        self.radius: float = r.readSingle()

class HalfSpaceBV: #:X
    def __init__(self, r: Reader):
        self.plane: NiPlane = NiPlane(r)
        self.center: Vector3 = r.readVector3()

class UnionBV: pass
class BoundingVolume: #:X
    collisionType: BoundVolumeType
    sphere: NiBound
    box: BoxBV
    capsule: CapsuleBV
    union: UnionBV
    halfSpace: HalfSpaceBV

    def __init__(self, r: Reader, h: Header):
        self.collisionType = BoundVolumeType(r.readUInt32())
        match self.collisionType:
            case BoundVolumeType.SPHERE_BV: self.sphere = NiBound(r)
            case BoundVolumeType.BOX_BV: self.box = BoxBV(r)
            case BoundVolumeType.CAPSULE_BV: self.capsule = CapsuleBV(r)
            case BoundVolumeType.UNION_BV: self.union = UnionBV(r)
            case BoundVolumeType.HALFSPACE_BV: self.halfSpace = HalfSpaceBV(r)

class UnionBV: #:X
    def __init__(self, r: Reader):
        self.boundingVolumes: list[BoundingVolume] = r.readL32FArray(lambda r: BoundingVolume(r))

class MorphWeight: #:X
    def __init__(self, r: Reader):
        self.interpolator: int = X[NiInterpolator].ref(r)
        self.weight: float = r.readSingle()

# A list of transforms for each bone in bhkPoseArray.
class BoneTransform: #:X
    def __init__(self, r: Reader):
        self.transforms: list[BoneTransform] = r.readL32FArray(lambda r: BoneTransform(r))

# Array of Vectors for Decal placement in BSDecalPlacementVectorExtraData.
class DecalVectorArray: #:X
    numVectors: int
    points: list[Vector3]         # Vector XYZ coords
    normals: list[Vector3]        # Vector Normals

    def __init__(self, r: Reader, h: Header):
        self.numVectors = r.readInt16()
        self.points = r.readPArray(Vector3, '3f', self.numVectors)
        self.normals = r.readPArray(Vector3, '3f', self.numVectors)

# Editor flags for the Body Partitions.
class BSPartFlag(Flag): #:X
    PF_EDITOR_VISIBLE = 0,                      # Visible in Editor
    PF_START_NET_BONESET = 1 << 8               # Start a new shared boneset.  It is expected this BoneSet and the following sets in the Skin Partition will have the same bones.

# Body part list for DismemberSkinInstance
class BodyPartList: #:X
    def __init__(self, r: Reader):
        self.partFlag: BSPartFlag = BSPartFlag(r.readUInt16())
        self.bodyPart: BSDismemberBodyPartType = BSDismemberBodyPartType(r.readUInt16())

# Stores Bone Level of Detail info in a BSBoneLODExtraData
class BoneLOD: #:X
    def __init__(self, r: Reader):
        self.distance: int = r.readUInt32()
        self.boneName: str = Y.string(r)

# Per-chunk material, used in bhkCompressedMeshShapeData
class bhkCMSDMaterial: #:X
    def __init__(self, r: Reader):
        self.material: SkyrimHavokMaterial = SkyrimHavokMaterial(r.readUInt32())
        self.filter: HavokFilter = HavokFilter(r, h)

# Triangle indices used in pair with "Big Verts" in a bhkCompressedMeshShapeData.
class bhkCMSDBigTris: #:X
    def __init__(self, r: Reader):
        self.triangle1: int = r.readUInt16()
        self.triangle2: int = r.readUInt16()
        self.triangle3: int = r.readUInt16()
        self.material: int = r.readUInt32() # Always 0?
        self.weldingInfo: int = r.readUInt16()

# A set of transformation data: translation and rotation
class bhkCMSDTransform: #:X
    def __init__(self, r: Reader):
        self.translation: Vector4 = r.readVector4()           # A vector that moves the chunk by the specified amount. W is not used.
        self.rotation: Quaternion = r.readQuaternionWFirst()  # Rotation. Reference point for rotation is bhkRigidBody translation.

# Defines subshape chunks in bhkCompressedMeshShapeData
class bhkCMSDChunk: #:X
    def __init__(self, r: Reader):
        self.translation: Vector4 = r.readVector4()
        self.materialIndex: int = r.readUInt32()             # Index of material in bhkCompressedMeshShapeData::Chunk Materials
        self.reference: int = r.readUInt16()               # Always 65535?
        self.transformIndex: int = r.readUInt16()          # Index of transformation in bhkCompressedMeshShapeData::Chunk Transforms
        self.vertices: list[int] = r.readL32PArray(None, 'H')
        self.indices: list[int] = r.readL32PArray(None, 'H')
        self.strips: list[int] = r.readL32PArray(None, 'H')
        self.weldingInfo: list[int] = r.readL32PArray(None, 'H')

class MalleableDescriptor: #:X
    type: hkConstraintType      # Type of constraint
    numEntities: int            # Always 2 (Hardcoded). Number of bodies affected by this constraint.
    entityA: int                # Usually NONE. The entity affected by this constraint.
    entityB: int                # Usually NONE. The entity affected by this constraint
    priority: int               # Usually 1. Higher values indicate higher priority of this constraint?
    ballAndSocket: BallAndSocketDescriptor
    hinge: HingeDescriptor
    limitedHinge: LimitedHingeDescriptor
    prismatic: PrismaticDescriptor
    ragdoll: RagdollDescriptor
    stiffSpring: StiffSpringDescriptor
    tau: float                  # not in Fallout 3 or Skyrim
    damping: float              # In TES CS described as Damping
    strength: float             # In GECK and Creation Kit described as Strength

    def __init__(self, r: Reader, h: Header):
        self.type = hkConstraintType(r.readUInt32())
        self.numEntities = r.readUInt32()
        self.entityA = X[bhkEntity].ref(r)
        self.entityB = X[bhkEntity].ref(r)
        self.priority = r.readUInt32()
        match self.type:
            case hkConstraintType.BallAndSocket: self.ballAndSocket = BallAndSocketDescriptor(r)
            case hkConstraintType.Hinge: self.hinge = HingeDescriptor(r, h)
            case hkConstraintType.LimitedHinge: self.limitedHinge = LimitedHingeDescriptor(r, h)
            case hkConstraintType.Prismatic: self.prismatic = PrismaticDescriptor(r, h)
            case hkConstraintType.Ragdoll: self.ragdoll = RagdollDescriptor(r, h)
            case hkConstraintType.StiffSpring: self.stiffSpring = StiffSpringDescriptor(r)
        if h.v <= 0x14000005:
            self.tau = r.readSingle()
            self.damping = r.readSingle()
        elif h.v >= 0x14020007:
            self.strength = r.readSingle()

class ConstraintData: #:X
    type: hkConstraintType      # Type of constraint
    numEntities: int            # Always 2 (Hardcoded). Number of bodies affected by this constraint.
    entityA: int                # Usually NONE. The entity affected by this constraint.
    entityB: int                # Usually NONE. The entity affected by this constraint
    priority: int               # Usually 1. Higher values indicate higher priority of this constraint?
    ballAndSocket: BallAndSocketDescriptor
    hinge: HingeDescriptor
    limitedHinge: LimitedHingeDescriptor
    prismatic: PrismaticDescriptor
    ragdoll: RagdollDescriptor
    stiffSpring: StiffSpringDescriptor
    malleable: MalleableDescriptor

    def __init__(self, r: Reader, h: Header):
        self.type = hkConstraintType(r.readUInt32())
        self.numEntities = r.readUInt32()
        self.entityA = X[bhkEntity].ref(r)
        self.entityB = X[bhkEntity].ref(r)
        self.priority = r.readUInt32()
        match self.type:
            case hkConstraintType.BallAndSocket: self.ballAndSocket = BallAndSocketDescriptor(r)
            case hkConstraintType.Hinge: self.hinge = HingeDescriptor(r, h)
            case hkConstraintType.LimitedHinge: self.limitedHinge = LimitedHingeDescriptor(r, h)
            case hkConstraintType.Prismatic: self.prismatic = PrismaticDescriptor(r, h)
            case hkConstraintType.Ragdoll: self.ragdoll = RagdollDescriptor(r, h)
            case hkConstraintType.StiffSpring: self.stiffSpring = StiffSpringDescriptor(r)
            case hkConstraintType.Malleable: self.malleable = MalleableDescriptor(r, h)

#endregion

#region NIF Objects
# These are the main units of data that NIF files are arranged in.
# They are like C classes and can contain many pieces of data.
# The only differences between these and compounds is that these are treated as object types by the NIF format and can inherit from other classes.

# Abstract object type.
class NiObject: pass
class NiObject:
    def __init__(self, r: Reader):
        setattr(self, '$type', type(self).__name__) #:M

    @staticmethod
    def read(r: Reader, h: Header) -> NiObject:
        nodeType: str = r.readL32AString()
        # print(nodeType)
        match nodeType:
            case 'NiNode': return NiNode(r)
            case 'NiTriShape': return NiTriShape(r)
            case 'NiTexturingProperty': return NiTexturingProperty(r)
            case 'NiSourceTexture': return NiSourceTexture(r)
            case 'NiMaterialProperty': return NiMaterialProperty(r)
            case 'NiMaterialColorController': return NiMaterialColorController(r)
            case 'NiTriShapeData': return NiTriShapeData(r)
            case 'RootCollisionNode': return RootCollisionNode(r)
            case 'NiStringExtraData': return NiStringExtraData(r)
            case 'NiSkinInstance': return NiSkinInstance(r)
            case 'NiSkinData': return NiSkinData(r)
            case 'NiAlphaProperty': return NiAlphaProperty(r)
            case 'NiZBufferProperty': return NiZBufferProperty(r)
            case 'NiVertexColorProperty': return NiVertexColorProperty(r)
            case 'NiBSAnimationNode': return NiBSAnimationNode(r)
            case 'NiBSParticleNode': return NiBSParticleNode(r)
            case 'NiParticles': return NiParticles(r)
            case 'NiParticlesData': return NiParticlesData(r)
            case 'NiRotatingParticles': return NiRotatingParticles(r)
            case 'NiRotatingParticlesData': return NiRotatingParticlesData(r)
            case 'NiAutoNormalParticles': return NiAutoNormalParticles(r)
            case 'NiAutoNormalParticlesData': return NiAutoNormalParticlesData(r)
            case 'NiUVController': return NiUVController(r)
            case 'NiUVData': return NiUVData(r)
            case 'NiTextureEffect': return NiTextureEffect(r)
            case 'NiTextKeyExtraData': return NiTextKeyExtraData(r)
            case 'NiVertWeightsExtraData': return NiVertWeightsExtraData(r)
            case 'NiParticleSystemController': return NiParticleSystemController(r)
            case 'NiBSPArrayController': return NiBSPArrayController(r)
            case 'NiGravity': return NiGravity(r)
            case 'NiParticleBomb': return NiParticleBomb(r)
            case 'NiParticleColorModifier': return NiParticleColorModifier(r)
            case 'NiParticleGrowFade': return NiParticleGrowFade(r)
            case 'NiParticleMeshModifier': return NiParticleMeshModifier(r)
            case 'NiParticleRotation': return NiParticleRotation(r)
            case 'NiKeyframeController': return NiKeyframeController(r)
            case 'NiKeyframeData': return NiKeyframeData(r)
            case 'NiColorData': return NiColorData(r)
            case 'NiGeomMorpherController': return NiGeomMorpherController(r)
            case 'NiMorphData': return NiMorphData(r)
            case 'AvoidNode': return AvoidNode(r)
            case 'NiVisController': return NiVisController(r)
            case 'NiVisData': return NiVisData(r)
            case 'NiAlphaController': return NiAlphaController(r)
            case 'NiFloatData': return NiFloatData(r)
            case 'NiPosData': return NiPosData(r)
            case 'NiBillboardNode': return NiBillboardNode(r)
            case 'NiShadeProperty': return NiShadeProperty(r)
            case 'NiWireframeProperty': return NiWireframeProperty(r)
            case 'NiCamera': return NiCamera(r)
            case _: print(f'Tried to read an unsupported NiObject type ({nodeType}).'); return None

# Unknown.
class Ni3dsAlphaAnimator(NiObject): #:X
    unknown1: bytearray         # Unknown.
    parent: int                 # The parent?
    num1: int                   # Unknown.
    num2: int                   # Unknown.
    unknown2: list[int]         # Unknown.

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        self.unknown1 = r.readBytes(40)
        self.parent = X[NiObject].ref(r)
        self.num1 = r.readUInt32()
        self.num2 = r.readUInt32()
        self.unknown2 = r.readPArray(None, 'I', self.num1 * self.num2 * 2)

# Unknown. Only found in 2.3 nifs.
class Ni3dsAnimationNode(NiObject): #:X
    name: str                       # Name of this object.
    unknownFloats1: list[float]     # Unknown. Matrix?
    unknownShort: int               # Unknown.
    child: int                      # Child?
    unknownFloats2: list[float]     # Unknown.
    unknownArray: bytearray         # Unknown.

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        self.name = Y.string(r)
        if not r.readBool32(): return
        self.unknownFloats1 = r.readPArray(None, 'f', 21)
        self.unknownShort = r.readUInt16()
        self.child = X[NiObject].ref(r)
        self.unknownFloats2 = r.readPArray(None, 'f', 12)
        self.unknownArray = r.readL32Bytes()

# Unknown!
class Ni3dsColorAnimator(NiObject): #:X
    unknown1: bytearray         # Unknown.

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        self.unknown1 = r.readBytes(184)

# Unknown!
class Ni3dsMorphShape(NiObject): #:X
    unknown1: bytearray         # Unknown.

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        self.unknown1 = r.readBytes(14) 

# Unknown!
class Ni3dsParticleSystem(NiObject): #:X
    unknown1: bytearray         # Unknown.

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        self.unknown1 = r.readBytes(14)

# Unknown!
class Ni3dsPathController(NiObject): #:X
    unknown1: bytearray         # Unknown.

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        self. = r.readBytes(20)

# LEGACY (pre-10.1). Abstract base class for particle system modifiers.
class NiParticleModifier(NiObject): #:X
    nextModifier: int               # Next particle modifier.
    controller: int                 # Next particle modifier.

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        self.nextModifier: int = X[NiParticleModifier].ref(r)
        self.controller: int = X[NiParticleSystemController].ref(r) if h.v >= 0x04000002 else None

# Particle system collider.
class NiPSysCollider(NiObject): #:X
    bounce: float                   # Amount of bounce for the collider.
    spawnOnCollide: bool            # Spawn particles on impact?
    dieOnCollide: bool              # Kill particles on impact?
    spawnModifier: int              # Spawner to use for the collider.
    parent: int                     # Link to parent.
    nextCollider: int               # The next collider.
    colliderObject: int             # The object whose position and orientation are the basis of the collider.

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        self.bounce = r.readSingle()
        self.spawnOnCollide = r.readBool32()
        self.dieOnCollide = r.readBool32()
        self.spawnModifier = X[NiPSysSpawnModifier].ref(r)
        self.parent = X[NiPSysColliderManager].ptr(r)
        self.nextCollider = X[NiPSysCollider].ref(r)
        self.colliderObject = X[NiAVObject].ptr(r)

class BroadPhaseType(Enum): #:X
    BROAD_PHASE_INVALID = 0,
    BROAD_PHASE_ENTITY = 1,
    BROAD_PHASE_PHANTOM = 2,
    BROAD_PHASE_BORDER = 3

class hkWorldObjCinfoProperty: #:X
    def __init__(self, r: Reader):
        self.data: int = r.readUInt32()
        self.size: int = r.readUInt32()
        self.capacityAndFlags: int = r.readUInt32()

class bhkRefObject(NiObject):
    def __init__(self, r: Reader): super().__init__(r, h)

class bhkSerializable(bhkRefObject):
    def __init__(self, r: Reader): super().__init__(r, h)

# Havok objects that have a position in the world?
class bhkWorldObject(bhkSerializable): #:X
    shape: int                      # Link to the body for this collision object.
    unknownInt: int
    havokFilter: HavokFilter
    unused: bytearray               # Garbage data from memory.
    broadPhaseType: BroadPhaseType
    unusedBytes: bytearray
    cinfoProperty: hkWorldObjCinfoProperty

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        self.shape = X[bhkShape].ref(r)
        if h.v <= 0x0A000100: self.unknownInt = r.readUInt32()
        self.havokFilter = HavokFilter(r, h)
        self.unused = r.readBytes(4)
        self.broadPhaseType = BroadPhaseType(r.readUInt32())
        self.unusedBytes = r.readBytes(3)
        self.cinfoProperty = hkWorldObjCinfoProperty(r)

# Havok object that do not react with other objects when they collide (causing deflection, etc.) but still trigger collision notifications to the game.  Possible uses are traps, portals, AI fields, etc.
class bhkPhantom(bhkWorldObject):
    def __init__(self, r: Reader): super().__init__(r, h)

# A Havok phantom that uses a Havok shape object for its collision volume instead of just a bounding box.
class bhkShapePhantom(bhkPhantom):
    def __init__(self, r: Reader): super().__init__(r, h)

# Unknown shape.
class bhkSimpleShapePhantom(bhkShapePhantom): #:X
    unused2: bytearray                      # Garbage data from memory.
    transform: Matrix4x4

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        self.unused2 = r.readBytes(8)
        self.transform = r.readMatrix4x4()

# A havok node, describes physical properties.
class bhkEntity(bhkWorldObject):
    def __init__(self, r: Reader): super().__init__(r, h)

# This is the default body type for all "normal" usable and static world objects. The "T" suffix marks this body as active for translation and rotation, a normal bhkRigidBody ignores those
# properties. Because the properties are equal, a bhkRigidBody may be renamed into a bhkRigidBodyT and vice-versa.
class bhkRigidBody(bhkEntity): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# The "T" suffix marks this body as active for translation and rotation.
class bhkRigidBodyT(bhkRigidBody):
    def __init__(self, r: Reader): super().__init__(r, h)

# Describes a physical constraint.
class bhkConstraint(bhkSerializable): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Hinge constraint.
class bhkLimitedHingeConstraint(bhkConstraint): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A malleable constraint.
class bhkMalleableConstraint(bhkConstraint): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A spring constraint.
class bhkStiffSpringConstraint(bhkConstraint): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Ragdoll constraint.
class bhkRagdollConstraint(bhkConstraint): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A prismatic constraint.
class bhkPrismaticConstraint(bhkConstraint): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A hinge constraint.
class bhkHingeConstraint(bhkConstraint): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A Ball and Socket Constraint.
class bhkBallAndSocketConstraint(bhkConstraint): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Two Vector4 for pivot in A and B.
class ConstraintInfo: #:X
    def __init__(self, r: Reader, h: Header):
        self.pivotInA: Vector4 = r.readVector4()
        self.pivotInB: Vector4 = r.readVector4()

# A Ball and Socket Constraint chain.
class bhkBallSocketConstraintChain(bhkSerializable): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A Havok Shape?
class bhkShape(bhkSerializable): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Transforms a shape.
class bhkTransformShape(bhkShape): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A havok shape, perhaps with a bounding sphere for quick rejection in addition to more detailed shape data?
class bhkSphereRepShape(bhkShape): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A havok shape.
class bhkConvexShape(bhkSphereRepShape): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# A sphere.
class bhkSphereShape(bhkConvexShape): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# A capsule.
class bhkCapsuleShape(bhkConvexShape): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A box.
class bhkBoxShape(bhkConvexShape): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A convex shape built from vertices. Note that if the shape is used in a non-static object (such as clutter), then they will simply fall through ground when they are under a bhkListShape.
class bhkConvexVerticesShape(bhkConvexShape): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A convex transformed shape?
# Should inherit from bhkConvexShape according to hierarchy, but seems to be exactly the same as bhkTransformShape.
class bhkConvexTransformShape(bhkTransformShape): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

class bhkConvexSweepShape(bhkShape): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Unknown.
class bhkMultiSphereShape(bhkSphereRepShape): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A tree-like Havok data structure stored in an assembly-like binary code?
class bhkBvTreeShape(bhkShape): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Memory optimized partial polytope bounding volume tree shape (not an entity).
class bhkMoppBvTreeShape(bhkBvTreeShape): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Havok collision object that uses multiple shapes?
class bhkShapeCollection(bhkShape): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# A list of shapes.
# Do not put a bhkPackedNiTriStripsShape in the Sub Shapes. Use a separate collision nodes without a list shape for those.
# Also, shapes collected in a bhkListShape may not have the correct walking noise, so only use it for non-walkable objects.
class bhkListShape(bhkShapeCollection): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# bhkMeshShape appears in some old Oblivion nifs, for instance meshes/architecture/basementsections/ungrdltraphingedoor.nif but only in some distributions of Oblivion
# XXX not completely decoded, also the 4 dummy separator bytes seem to be missing from nifs that have this block
class bhkMeshShape(bhkShape): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A shape constructed from strips data.
class bhkPackedNiTriStripsShape(bhkShapeCollection): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A shape constructed from a bunch of strips.
class bhkNiTriStripsShape(bhkShapeCollection): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A generic extra data object.
class NiExtraData(NiObject): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Abstract base class for all interpolators of bool, float, NiQuaternion, NiPoint3, NiColorA, and NiQuatTransform data.
class NiInterpolator(NiObject): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Abstract base class for interpolators that use NiAnimationKeys (Key, KeyGrp) for interpolation.
class NiKeyBasedInterpolator(NiInterpolator): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Uses NiFloatKeys to animate a float value over time.
class NiFloatInterpolator(NiKeyBasedInterpolator): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# An interpolator for transform keyframes.
class NiTransformInterpolator(NiKeyBasedInterpolator): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Uses NiPosKeys to animate an NiPoint3 value over time.
class NiPoint3Interpolator(NiKeyBasedInterpolator): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

class PathFlags(Enum): #:X
    CVDataNeedsUpdate = 0
    CurveTypeOpen = 1 << 1
    AllowFlip = 1 << 2
    Bank = 1 << 3
    ConstantVelocity = 1 << 4
    Follow = 1 << 5
    Flip = 1 << 6

# Used to make an object follow a predefined spline path.
class NiPathInterpolator(NiKeyBasedInterpolator): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Uses NiBoolKeys to animate a bool value over time.
class NiBoolInterpolator(NiKeyBasedInterpolator): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Uses NiBoolKeys to animate a bool value over time.
# Unlike NiBoolInterpolator, it ensures that keys have not been missed between two updates.
class NiBoolTimelineInterpolator(NiBoolInterpolator): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

class InterpBlendFlags(Enum): #:X
    MANAGER_CONTROLLED = 1      # MANAGER_CONTROLLED

# Interpolator item for array in NiBlendInterpolator.
class InterpBlendItem: #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Abstract base class for all NiInterpolators that blend the results of sub-interpolators together to compute a final weighted value.
class NiBlendInterpolator(NiInterpolator): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Abstract base class for interpolators storing data via a B-spline.
class NiBSplineInterpolator(NiInterpolator): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Abstract base class for NiObjects that support names, extra data, and time controllers.
class NiObjectNET(NiObject): #:X
    skyrimShaderType: BSLightingShaderPropertyShaderType # Configures the main shader path
    name: str                                   # Name of this controllable object, used to refer to the object in .kf files.
    oldExtraData: tuple                         # Extra data for pre-3.0 versions.
    extraData: int                              # Extra data object index. (The first in a chain)
    extraDataList: list[int]                    # List of extra data indices.
    controller: int                             # Controller object index. (The first in a chain)

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        if h.UserVersion2 >= 83: self.skyrimShaderType = BSLightingShaderPropertyShaderType(r.readUInt32())
        self.name = Y.string(r)
        if h.v <= 0x02030000:
            if r.readBool32(): self.oldExtra = (Y.string(r), r.readUInt32(), Y.string(r))
            r.skip(1) # Unknown Byte, Always 0.
        if h.v >= 0x03000000 and h.v <= 0x04020200: self.extraData = X[NiExtraData].ref(r)
        if h.v >= 0x0A000100: self.extraDataList = r.readL16FArray(lambda r: X[NiExtraData].ref(r))
        if h.v >= 0x03000000: self.controller = X[NiTimeController].ref(r)

# This is the most common collision object found in NIF files. It acts as a real object that is visible and possibly (if the body allows for it) interactive. The node itself
# is simple, it only has three properties. For this type of collision object, bhkRigidBody or bhkRigidBodyT is generally used.
class NiCollisionObject(NiObject): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Collision box.
class NiCollisionData(NiCollisionObject): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# bhkNiCollisionObject flags. The flags 0x2, 0x100, and 0x200 are not seen in any NIF nor get/set by the engine.
class bhkCOFlags(Enum): #:X
    ACTIVE = 0
    #UNK1 = 1 << 1
    NOTIFY = 1 << 2
    SET_LOCAL = 1 << 3
    DBG_DISPLAY = 1 << 4
    USE_VEL = 1 << 5
    RESET = 1 << 6
    SYNC_ON_UPDATE = 1 << 7
    #UNK2 = 1 << 8
    #UNK3 = 1 << 9
    ANIM_TARGETED = 1 << 10
    DISMEMBERED_LIMB = 1 << 11

# Havok related collision object?
class bhkNiCollisionObject(NiCollisionObject): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Havok related collision object?
class bhkCollisionObject(bhkNiCollisionObject): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Unknown.
class bhkBlendCollisionObject(bhkCollisionObject): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Unknown.
class bhkPCollisionObject(bhkNiCollisionObject): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Unknown.
class bhkSPCollisionObject(bhkPCollisionObject): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)


# Abstract audio-visual base class from which all of Gamebryo's scene graph objects inherit.
class NiAVObject(NiObjectNET): #:M
    class F(Flag):
        Hidden = 0x1

    flags: Flags    # Basic flags for AV objects.
    translation: Vector3
    rotation: Matrix4x4
    scale: float
    velocity: Vector3
    properties: list[int]
    boundingVolume: BoundingVolume
    collisionObject: NiCollisionObject

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        self.flags = Flags(r.readUInt16()) if h.v >= 0x03000000 and h.userVersion2 <= 26 else \
            Flags(r.readUInt16()) if h.userVersion2 > 26 else \
            Flags(14)
        self.translation = r.readVector3()
        self.rotation = r.readMatrix3x3As4x4()
        self.scale = r.readSingle()
        if h.v <= 0x04020200: self.velocity = r.readVector3()
        if h.userVersion2 <= 34: self.properties = r.readL32FArray(lambda r: X[NiProperty].ref(r))
        if h.v <= 0x02030000:
            self.unknown1 = r.readPArray(None, 'I', 4)
            self.unknown2 = r.readByte()
        if h.v >= 0x03000000 and h.v <= 0x04020200 and r.readBool32(): self.boundingVolume = BoundingVolume(r)
        if h.v >= 0x0A000100: self.collisionObject = NiCollisionObject(r, h)

# Abstract base class for dynamic effects such as NiLights or projected texture effects.
class NiDynamicEffect(NiAVObject): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Abstract base class that represents light sources in a scene graph.
# For Bethesda Stream 130 (FO4), NiLight now directly inherits from NiAVObject.
class NiLight(NiDynamicEffect): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Abstract base class representing all rendering properties. Subclasses are attached to NiAVObjects to control their rendering.
class NiProperty(NiObjectNET):
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Unknown
class NiTransparentProperty(NiProperty): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Abstract base class for all particle system modifiers.
class NiPSysModifier(NiObject): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Abstract base class for all particle system emitters.
class NiPSysEmitter(NiPSysModifier): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Abstract base class for particle emitters that emit particles from a volume.
class NiPSysVolumeEmitter(NiPSysEmitter): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

class NiTimeController(NiObject): #:M
    nextController: int             # Index of the next controller.
    flags: Flags                    # Controller flags.
    frequency: float                # Frequency (is usually 1.0).
    phase: float                    # Phase (usually 0.0).
    startTime: float                # Controller start time.
    stopTime: float                 # Controller stop time.
    target: int                     # Controller target (object index of the first controllable ancestor of this object).
    unknownInt: int                 # Unknown integer.
    
    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        self.nextController = X[NiTimeController].ref(r)
        self.flags = r.readUInt16() 
        self.frequency = r.readSingle()
        self.phase = r.readSingle() 
        self.startTime = r.readSingle()
        self.stopTime = r.readSingle()
        if h.v >= 0x0303000D: self.target = X[NiObjectNET].ptr(r)
        elif h.v <= 0x03010000: self.unknownInt = r.readUInt32()

# DEPRECATED (20.6)
class NiMultiTargetTransformController(NiInterpController): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# DEPRECATED (20.5), replaced by NiMorphMeshModifier.
# Time controller for geometry morphing.
class NiGeomMorpherController(NiInterpController): #:M
    extraFlags: Flags               # 1 = UPDATE NORMALS
    data: int
    alwaysUpdate: int
    interpolators: list[int]
    interpolatorWeights: list[MorphWeight]
    unknownInts: list[int]          # Unknown.

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        if h.v >= 0x0A000102: self.extraFlags = Flags(r.readUInt16())
        self.data = X[NiMorphData].ref(r) 
        if h.v >= 0x04000001: self.alwaysUpdate = r.readByte()
        if h.v >= 0x0A01006A:
            if h.v <= 0x14000005: self.interpolators = r.readL32FArray(X[NiInterpolator].ref)
            elif h.v >= 0x14010003: self.interpolatorWeights = r.readL32FArray(lambda r: MorphWeight(r))
            else: r.readUInt32()
        if h.v >= 0x0A020000 and h.v <= 0x14000005 and h.userVersion2 < 9: self.unknownInts = r.readL32PArray(None, 'I')

# Unknown! Used by Daoc->'healing.nif'.
class NiMorphController(NiInterpController): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Unknown! Used by Daoc.
class NiMorpherController(NiInterpController): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Uses a single NiInterpolator to animate its target value.
class NiSingleInterpController(NiInterpController): #:M
    interpolator: int

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        if h.v >= 0x0A010068: self.interpolator = X[NiInterpolator].ref(r)

# DEPRECATED (10.2), RENAMED (10.2) to NiTransformController
# A time controller object for animation key frames.
class NiKeyframeController(NiSingleInterpController):
    data: int

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        if h.v <= 0x0A010067: self.data = X[NiKeyframeData].ref(r) #:M


# Unknown! Used by Daoc->'healing.nif'.
class NiTransformController(NiKeyframeController): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)


# A particle system modifier controller.
# NiInterpController::GetCtlrID() string format:
# '%s' Where %s = Value of "Modifier Name"
class NiPSysModifierCtlr(NiSingleInterpController): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Particle system emitter controller.
# NiInterpController::GetInterpolatorID() string format:
# ['BirthRate', 'EmitterActive'] (for "Interpolator" and "Visibility Interpolator" respectively)
class NiPSysEmitterCtlr(NiPSysModifierCtlr): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A particle system modifier controller that animates a boolean value for particles.
class NiPSysModifierBoolCtlr(NiPSysModifierCtlr): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# A particle system modifier controller that animates active/inactive state for particles.
class NiPSysModifierActiveCtlr(NiPSysModifierBoolCtlr): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# A particle system modifier controller that animates a floating point value for particles.
class NiPSysModifierFloatCtlr(NiPSysModifierCtlr): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Animates the declination value on an NiPSysEmitter object.
class NiPSysEmitterDeclinationCtlr(NiPSysModifierFloatCtlr): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Animates the declination variation value on an NiPSysEmitter object.
class NiPSysEmitterDeclinationVarCtlr(NiPSysModifierFloatCtlr): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Animates the size value on an NiPSysEmitter object.
class NiPSysEmitterInitialRadiusCtlr(NiPSysModifierFloatCtlr): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Animates the lifespan value on an NiPSysEmitter object.
class NiPSysEmitterLifeSpanCtlr(NiPSysModifierFloatCtlr): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Animates the speed value on an NiPSysEmitter object.
class NiPSysEmitterSpeedCtlr(NiPSysModifierFloatCtlr): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Animates the strength value of an NiPSysGravityModifier.
class NiPSysGravityStrengthCtlr(NiPSysModifierFloatCtlr): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target float value.
class NiFloatInterpController(NiSingleInterpController): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)

# Changes the image a Map (TexDesc) will use. Uses a float interpolator to animate the texture index.
# Often used for performing flipbook animation.
class NiFlipController(NiFloatInterpController): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Animates the alpha value of a property using an interpolator.
class NiAlphaController(NiFloatInterpController): #:M
    data: int

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        if h.v <= 0x0A020067: self.data = X[NiFloatData].ref(r)

# Used to animate a single member of an NiTextureTransform.
# NiInterpController::GetCtlrID() string formats:
# ['%1-%2-TT_TRANSLATE_U', '%1-%2-TT_TRANSLATE_V', '%1-%2-TT_ROTATE', '%1-%2-TT_SCALE_U', '%1-%2-TT_SCALE_V']
# (Depending on "Operation" enumeration, %1 = Value of "Shader Map", %2 = Value of "Texture Slot")
class NiTextureTransformController(NiFloatInterpController): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# Unknown controller.
class NiLightDimmerController(NiFloatInterpController): #:X
    def __init__(self, r: Reader, h: Header): super().__init__(r, h)


# Abstract base class for all NiInterpControllers that use a NiInterpolator to animate their target boolean value.
class NiBoolInterpController(NiSingleInterpController):
    def __init__(self, r: Reader): super().__init__(r, h)

# Animates the visibility of an NiAVObject.
class NiVisController(NiBoolInterpController): #:M
    data: int

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        if h.v <= 0x0A010067: self.data: int = X[NiVisData].ref(r)

# xx
class xx(xx): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###


class NiMaterialColorController(NiPoint3InterpController):
    def __init__(self, r: Reader): super().__init__(r, h)

# xx
class xx(xx): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# xx
class xx(xx): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# xx
class xx(xx): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# xx
class xx(xx): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# xx
class xx(xx): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# xx
class xx(xx): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# xx
class xx(xx): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# xx
class xx(xx): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###

# xx
class xx(xx): #:X
    ###

    def __init__(self, r: Reader, h: Header):
        super().__init__(r, h)
        ###























# yy
class yy: #:X
    def __init__(self, r: Reader):
        xx
# xx
class xx: #:X
    xx
    def __init__(self, r: Reader, h: Header):
        xx





# Nodes
class NiNode(NiAVObject):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        # self.numChildren: int
        self.children: list[int] = r.readL32FArray(lambda r: X[NiAVObject].ref(r)) #:M
        #self.numEffects: int
        self.effects: list[int] = r.readL32FArray(lambda r: X[NiDynamicEffect].ref(r)) #:M
class RootCollisionNode(NiNode):
    def __init__(self, r: Reader): super().__init__(r, h)
class NiBSAnimationNode(NiNode):
    def __init__(self, r: Reader): super().__init__(r, h)
class NiBSParticleNode(NiNode):
    def __init__(self, r: Reader): super().__init__(r, h)
class NiBillboardNode(NiNode):
    def __init__(self, r: Reader): super().__init__(r, h)
class AvoidNode(NiNode):
    def __init__(self, r: Reader): super().__init__(r, h)

# Geometry
class NiGeometry(NiAVObject):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.data: int = X[NiGeometryData].ref(r) #:M
        self.skinInstance: int = X[NiSkinInstance].ref(r) #:M

class NiGeometryData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.numVertices: int = r.readUInt16() #:M
        self.hasVertices: bool = r.readBool32() #:M
        if self.hasVertices: self.vertices: list[np.ndarray] = r.readFArray(lambda r: r.readVector3(), self.numVertices) #:M :Vector3
        self.hasNormals: bool = r.readBool32()
        if self.hasNormals: self.normals: list[np.ndarray] = r.readFArray(lambda r: r.readVector3(), self.numVertices) #:M :Vector3
        self.center: np.ndarray = r.readVector3() #:M :Vector3
        self.radius: float = r.readSingle() #:M
        self.hasVertexColors: bool = r.readBool32() #:M
        if self.hasVertexColors: self.vertexColors: list[Color3] = r.readFArray(lambda r: Color4(r), self.numVertices) #:M
        self.numUVSets: int = r.readUInt16() #:M
        self.hasUV: bool = r.readBool32() #:M
        if self.hasUV:
            self.uvSets: list[list[TexCoord]] = [[None for x in range(self.numVertices)] for x in range(self.numUVSets)] #:M :TexCoord[self.numUVSets, self.numVertices]
            for i in range(self.numUVSets):
                for j in range(self.numVertices): self.uvSets[i][j] = TexCoord(r)

class NiTriBasedGeom(NiGeometry):
    def __init__(self, r: Reader): super().__init__(r, h)

class NiTriBasedGeomData(NiGeometryData):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.numTriangles: int = r.readUInt16() #:M

class NiTriShape(NiTriBasedGeom):
    def __init__(self, r: Reader): super().__init__(r, h)

class NiTriShapeData(NiTriBasedGeomData):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.numTrianglePoints: int = r.readUInt32() #:M
        self.triangles: list[Triangle] = r.readFArray(lambda r: Triangle(r), self.numTriangles) #:M
        self.matchGroups: list[MatchGroup] = r.readL16FArray(lambda r: MatchGroup(r)) #:M

# Properties


class NiTexturingProperty(NiProperty):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.flags: NiAVObject.NiFlags = NiReaderUtils.readFlags(r) #:M
        self.applyMode: ApplyMode = r.readUInt32() #:M
        self.textureCount: int = r.readUInt32() #:M
        self.baseTexture: TexDesc = TexDesc(r) if r.readBool32() else None #:M
        self.darkTexture: TexDesc = TexDesc(r) if r.readBool32() else None #:M
        self.detailTexture: TexDesc = TexDesc(r) if r.readBool32() else None #:M
        self.glossTexture: TexDesc = TexDesc(r) if r.readBool32() else None #:M
        self.glowTexture: TexDesc = TexDesc(r) if r.readBool32() else None #:M
        self.bumpMapTexture: TexDesc = TexDesc(r) if r.readBool32() else None #:M
        self.decal0Texture: TexDesc = TexDesc(r) if r.readBool32() else None #:M

class NiAlphaProperty(NiProperty):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.flags: int = r.readUInt16() #:M
        self.threshold: byte = r.readByte() #:M

class NiZBufferProperty(NiProperty):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.flags: int = r.readUInt16() #:M

class NiVertexColorProperty(NiProperty):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.flags: NiAVObject.NiFlags = NiReaderUtils.readFlags(r) #:M
        self.vertexMode: VertMode = r.readUInt32() #:M
        self.lightingMode: LightMode = r.readUInt32() #:M

class NiShadeProperty(NiProperty):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.flags: NiAVObject.NiFlags = NiReaderUtils.readFlags(r) #:M

class NiWireframeProperty(NiProperty):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.flags: NiAVObject.NiFlags = NiReaderUtils.readFlags(r) #:M

class NiCamera(NiAVObject):
    def __init__(self, r: Reader): super().__init__(r, h)

# Data
class NiUVData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.uvGroups: KeyGroup = r.readFArray(lambda r: KeyGroup(float, r), 4) #:M

class NiKeyframeData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.numRotationKeys: int = r.readUInt32() #:M
        if self.numRotationKeys != 0:
            self.rotationType: KeyType = r.readUInt32() #:M
            if self.rotationType != KeyType.XYZ_ROTATION_KEY:
                self.quaternionKeys: list[QuatKey] = r.readFArray(lambda r: QuatKey(Quaternion, r, rotationType), self.numRotationKeys) #:M
            else:
                self.unknownFloat: float = r.readSingle() #:M
                self.xyzRotations: list[KeyGroup] = r.readFArray(lambda r: KeyGroup(float, r), 3) #:M
        self.translations: KeyGroup = KeyGroup(np.ndarray, r) #:M :Vector3
        self.scales: KeyGroup = KeyGroup(float, r) #:M

class NiColorData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.data: KeyGroup = KeyGroup(Color4, r) #:M

class NiMorphData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.numMorphs: int = r.readUInt32() #:M
        self.numVertices: int = r.readUInt32() #:M
        self.relativeTargets: byte = r.readByte() #:M
        self.morphs: list[Morph] = r.readFArray(lambda r: Morph(r, self.numVertices), self.numMorphs) #:M

class NiVisData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.keys: list[Key] = r.readL32FArray(lambda r: Key(byte, r, KeyType.LINEAR_KEY)) #:M

class NiFloatData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.data: KeyGroup = KeyGroup(float, r) #:M

class NiPosData(NiObject): 
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.data: KeyGroup = KeyGroup(np.ndarray, r) #:M

class NiExtraData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.nextExtraData: int = X[NiExtraData].ref(r) #:M

class NiStringExtraData(NiExtraData):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.bytesRemaining: int = r.readUInt32() #:M
        self.str: str = r.readL32Encoding() #:M

class NiTextKeyExtraData(NiExtraData):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.unknownInt1: int = r.readUInt32()
        self.textKeys: list[Key] = r.readL32FArray(lambda r: Key(string, r, KeyType.LINEAR_KEY)) #:M

class NiVertWeightsExtraData(NiExtraData):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.numBytes: int = r.readUInt32() #:M
        self.numVertices: int = r.readUInt16() #:M
        self.weights: list[float] = r.readPArray('f', self.numVertices) #:M

# Controllers


class NiUVController(NiTimeController):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.unknownShort: int = r.readUInt16()
        self.data: int = X[NiUVData].ref(r)

class NiInterpController(NiTimeController):
    def __init__(self, r: Reader): super().__init__(r, h)


# Particles
class NiParticles(NiGeometry):
    def __init__(self, r: Reader): super().__init__(r, h)
class NiParticlesData(NiGeometryData):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.numParticles: int = r.readUInt16() #:M
        self.particleRadius: float = r.readSingle() #:M
        self.numActive: int = r.readUInt16() #:M
        self.sizes: list[float] = r.readPArray('f', self.numVertices) if r.readBool32() else None #:M

class NiRotatingParticles(NiParticles):
    def __init__(self, r: Reader): super().__init__(r, h)
class NiRotatingParticlesData(NiParticlesData):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.rotations: list[Quaternion] = r.readFArray(lambda r: r.readQuaternionWFirst(), self.numVertices) if r.readBool32() else [] #:M

class NiAutoNormalParticles(NiParticles):
    def __init__(self, r: Reader): super().__init__(r, h)
class NiAutoNormalParticlesData(NiParticlesData):
    def __init__(self, r: Reader): super().__init__(r, h)

class NiParticleSystemController(NiTimeController):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.speed: float = r.readSingle() #:M
        self.speedRandom: float = r.readSingle() #:M
        self.verticalDirection: float = r.readSingle() #:M
        self.verticalAngle: float = r.readSingle() #:M
        self.horizontalDirection: float = r.readSingle() #:M
        self.horizontalAngle: float = r.readSingle() #:M
        self.unknownNormal: np.ndarray = r.readVector3() #:M :Vector3
        self.unknownColor: Color4 = Color4(r) #:M
        self.size: float = r.readSingle() #:M
        self.emitStartTime: float = r.readSingle() #:M
        self.emitStopTime: float = r.readSingle() #:M
        self.unknownByte: byte = r.readByte() #:M
        self.emitRate: float = r.readSingle() #:M
        self.lifetime: float = r.readSingle() #:M
        self.lifetimeRandom: float = r.readSingle() #:M
        self.emitFlags: int = r.readUInt16() #:M
        self.startRandom: np.ndarray = r.readVector3() #:M :Vector3
        self.emitter: int = X[NiObject].ptr(r) #:M
        self.unknownShort2: int = r.readUInt16() #:M
        self.unknownFloat13: float = r.readSingle() #:M
        self.unknownInt1: int = r.readUInt32() #:M
        self.unknownInt2: int = r.readUInt32() #:M
        self.unknownShort3: int = r.readUInt16() #:M
        self.numParticles: int = r.readUInt16() #:M
        self.numValid: int = r.readUInt16() #:M
        self.particles: list[Particle] = r.readFArray(lambda r: Particle(r), self.numParticles) #:M
        self.unknownLink: int = X[NiObject].ref(r) #:M
        self.particleExtra: int = X[NiParticleModifier].ref(r) #:M
        self.unknownLink2: int = X[NiObject].ref(r) #:M
        self.trailer: byte = r.readByte() #:M

class NiBSPArrayController(NiParticleSystemController):
    def __init__(self, r: Reader): super().__init__(r, h)

# Particle Modifiers
class NiParticleModifier(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.nextModifier: int = X[NiParticleModifier].ref(r) #:M
        self.controller: int = X[NiParticleSystemController].ptr(r) #:M

class NiGravity(NiParticleModifier):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.unknownFloat1: float = r.readSingle() #:M
        self.force: float = r.readSingle() #:M
        self.type: FieldType = r.readUInt32() #:M
        self.position: np.ndarray = r.ReadVector3() #:M :Vector3
        self.direction: np.ndarray = r.ReadVector3() #:M :Vector3

class NiParticleBomb(NiParticleModifier):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.decay: float = r.readSingle() #:M
        self.duration: float = r.readSingle() #:M
        self.deltaV: float = r.readSingle() #:M
        self.start: float = r.readSingle() #:M
        self.decayType: DecayType = r.readUInt32() #:M
        self.position: np.ndarray = r.readVector3() #:M :Vector3
        self.direction: np.ndarray = r.readVector3() #:M :Vector3

class NiParticleColorModifier(NiParticleModifier):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.colorData: int = X[NiColorData].ref(r) #:M

class NiParticleGrowFade(NiParticleModifier):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.grow: float = r.readSingle() #:M
        self.fade: float = r.readSingle() #:M

class NiParticleMeshModifier(NiParticleModifier):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.particleMeshes: list[int] = r.readL32FArray(lambda r: X[NiAVObject].ref(r)) #:M

class NiParticleRotation(NiParticleModifier):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.randomInitialAxis: byte = r.readByte() #:M
        self.initialAxis: np.ndarray = r.readVector3() #:M :Vector3
        self.rotationSpeed: float = r.readSingle() #:M


# Skin Stuff
class NiSkinInstance(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.data: int = X[NiSkinData].ref(r) #:M
        self.skeletonRoot: int = X[NiNode].ptr(r) #:M
        self.numBones: int = r.readUInt32() #:M
        self.bones: list[int] = r.readFArray(lambda r: X[NiNode].ptr(r), self.numBones) #:M

class NiSkinData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.skinTransform: NiTransform = NiTransform(r) #:M
        self.numBones: int = r.readUInt32() #:M
        self.skinPartition: int = X[NiSkinPartition].ref(r) #:M
        self.boneList: list[BoneData] = r.readFArray(lambda r: BoneData(r), self.numBones) #:M

class NiSkinPartition(NiObject):
    def __init__(self, r: Reader): super().__init__(r, h)

# Miscellaneous
class NiTexture(NiObjectNET):
    def __init__(self, r: Reader): super().__init__(r, h)

class NiSourceTexture(NiTexture):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.useExternal: byte = r.readByte() #:M
        self.fileName: str = r.readL32Encoding() #:M
        self.pixelLayout: PixelLayout = r.readUInt32() #:M
        self.useMipMaps: MipMapFormat = r.readUInt32() #:M
        self.alphaFormat: AlphaFormat = r.readUInt32() #:M
        self.isStatic: byte = r.readByte() #:M

class NiPoint3InterpController(NiSingleInterpController):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.data: int = X[NiPosData].ref(r)

class NiMaterialProperty(NiProperty):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.flags: NiAVObject.NiFlags = NiReaderUtils.readFlags(r) #:M
        self.ambientColor: Color3 = Color3(r) #:M
        self.diffuseColor: Color3 = Color3(r) #:M
        self.specularColor: Color3 = Color3(r) #:M
        self.emissiveColor: Color3 = Color3(r) #:M
        self.glossiness: float = r.readSingle() #:M
        self.alpha: float = r.readSingle() #:M



class NiDynamicEffect(NiAVObject):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.affectedNodeListPointers: list[int] = r.readL32PArray(None, 'I') #:M

class NiTextureEffect(NiDynamicEffect):
    def __init__(self, r: Reader):
        super().__init__(r, h)
        self.modelProjectionMatrix: np.ndarray = r.readMatrix3x3As4x4() #:M :Matrix4x4
        self.modelProjectionTransform: np.ndarray = r.readVector3() #:M :Vector3
        self.textureFiltering: TexFilterMode = r.ReadUInt32() #:M
        self.textureClamping: TexClampMode = r.readUInt32() #:M
        self.textureType: TextureType = r.readUInt32() #:M
        self.coordinateGenerationType: CoordGenType = r.readUInt32() #:M
        self.sourceTexture: int = X[NiSourceTexture].ref(r) #:M
        self.clippingPlane: byte = r.readByte() #:M
        self.unknownVector: np.ndarray = r.readVector3() #:M :Vector3
        self.unknownFloat: float = r.readSingle() #:M
        self.ps2L: int = r.readInt16() #:M
        self.ps2K: int = r.readInt16() #:M
        self.unknownShort: int = r.readUInt16() #:M

#endregion

class NiReaderUtils:
    @staticmethod
    def readFlags(r: Reader) -> NiAVObject.NiFlags: return r.readUInt16()
