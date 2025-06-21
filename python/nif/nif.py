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

    def __init__(self, r: Reader):
        self.index = r.readUInt16()
        self.weight = r.readSingle()

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
    nodeName: str                                       # The name of the animated NiAVObject.
    propertyType: str                                   # The RTTI type of the NiProperty the controller is attached to, if applicable.
    controllerType: str                                 # The RTTI type of the NiTimeController.
    controllerId: str                                   # An ID that can uniquely identify the controller among others of the same type on the same NiObjectNET.
    interpolatorId: str                                 # An ID that can uniquely identify the interpolator among others of the same type on the same NiObjectNET.
    stringPalette: int                                  # Refers to the NiStringPalette which contains the name of the controlled NIF object.
    nodeNameOffset: int                                 # Offset in NiStringPalette to the name of the animated NiAVObject.
    propertyTypeOffset: int                             # Offset in NiStringPalette to the RTTI type of the NiProperty the controller is attached to, if applicable.
    controllerTypeOffset: int                           # Offset in NiStringPalette to the RTTI type of the NiTimeController.
    controllerIdOffset: int                             # Offset in NiStringPalette to an ID that can uniquely identify the controller among others of the same type on the same NiObjectNET.
    interpolatorIdOffset: int                           # Offset in NiStringPalette to an ID that can uniquely identify the interpolator among others of the same type on the same NiObjectNET.

    def __init__(self, r: Reader, h: Header):
        self.targetName = if h.v <= 0x0A010067: Y.string(r)
        self.interpolator = if h.v >= 0x0A01006A: X[NiInterpolator].ref(r)
        self.controller = if h.v <= 0x14050000: X[NiTimeController].ref(r)
        self.blendInterpolator = if h.v >= 0x0A010068 and h.v <= 0x0A01006E: X[NiBlendInterpolator].ref(r)
        self.blendIndex = if h.v >= 0x0A010068 and h.v <= 0x0A01006E: r.readUInt16()
        self.priority = if h.v >= 0x0A01006A and : r.readByte()
        self.nodeName = if h.v >= 0x0A010068 and h.v <= 0x0A010071: Y.string(r)
        self.propertyType = if h.v >= 0x0A010068 and h.v <= 0x0A010071: Y.string(r)
        self.controllerType = if h.v >= 0x0A010068 and h.v <= 0x0A010071: Y.string(r)
        self.controllerId = if h.v >= 0x0A010068 and h.v <= 0x0A010071: Y.string(r)
        self.interpolatorId = if h.v >= 0x0A010068 and h.v <= 0x0A010071: Y.string(r)
        self.stringPalette = if h.v >= 0x0A020000 and h.v <= 0x14010000: X[NiStringPalette].ref(r)
        self.nodeNameOffset = if h.v >= 0x0A020000 and h.v <= 0x14010000: r.readUInt32()
        self.propertyTypeOffset = if h.v >= 0x0A020000 and h.v <= 0x14010000: r.readUInt32()
        self.controllerTypeOffset = if h.v >= 0x0A020000 and h.v <= 0x14010000: r.readUInt32()
        self.controllerIdOffset = if h.v >= 0x0A020000 and h.v <= 0x14010000: r.readUInt32()
        self.interpolatorIdOffset = if h.v >= 0x0A020000 and h.v <= 0x14010000: r.readUInt32()
        self.nodeName = if h.v >= 0x14010001: Y.string(r)
        self.propertyType = if h.v >= 0x14010001: Y.string(r)
        self.controllerType = if h.v >= 0x14010001: Y.string(r)
        self.controllerId = if h.v >= 0x14010001: Y.string(r)
        self.interpolatorId = if h.v >= 0x14010001: Y.string(r)

# Information about how the file was exported
class ExportInfo:
    def __init__(self, r: Reader):
        self.author: str = r.readL8AString()
        self.processScript: str = r.readL8AString()
        self.exportScript: str = r.readL8AString()

# The NIF file header.
class Header:
    headerString: str                                   # 'NetImmerse File Format x.x.x.x' (versions <= 10.0.1.2) or 'Gamebryo File Format x.x.x.x' (versions >= 10.1.0.0), with x.x.x.x the version written out. Ends with a newline character (0x0A).
    copyright: list[str]
    version: int                                        # The NIF version, in hexadecimal notation: 0x04000002, 0x0401000C, 0x04020002, 0x04020100, 0x04020200, 0x0A000100, 0x0A010000, 0x0A020000, 0x14000004, ...
    endianType: EndianType                              # Determines the endianness of the data in the file.
    userVersion: int                                    # An extra version number, for companies that decide to modify the file format.
    numBlocks: int                                      # Number of file objects.
    userVersion2: int
    exportInfo: ExportInfo
    maxFilepath: str
    metadata: bytearray
    numBlockTypes: int                                  # Number of object types in this NIF file.
    blockTypes: list[str]                               # List of all object types used in this NIF file.
    blockTypeHashes: list[int]                          # List of all object types used in this NIF file.
    blockTypeIndex: list[int]                           # Maps file objects on their corresponding type: first file object is of type object_types[object_type_index[0]], the second of object_types[object_type_index[1]], etc.
    blockSize: list[int]                                # Array of block sizes?
    numStrings: int                                     # Number of strings.
    maxStringLength: int                                # Maximum string length.
    strings: list[str]                                  # Strings.
    groups: list[int]

    def __init__(self, r: Reader, h: Header):
        self.headerString = ??
        self.copyright = if h.v <= 0x03010000: r.readFArray(lambda r: LineString(r), 3)
        self.version = if h.v >= 0x03010001: r.readUInt32()
        self.endianType = if h.v >= 0x14000003: EndianType(r.readByte())
        self.userVersion = if h.v >= 0x0A000108: r.readUInt32()
        self.numBlocks = if h.v >= 0x03010001: r.readUInt32()
        self.userVersion2 = if ((h.v == 0x14020007) or (h.v == 0x14000005) or ((h.v >= 0x0A000102) and (h.v <= 0x14000004) and (Uh.v <= 11))) and (Uh.v >= 3): r.readUInt32()
        self.exportInfo = if ((h.v == 0x14020007) or (h.v == 0x14000005) or ((h.v >= 0x0A000102) and (h.v <= 0x14000004) and (Uh.v <= 11))) and (Uh.v >= 3): ExportInfo(r)
        self.maxFilepath = if (h.uv2 == 130): r.readL8AString()
        self.metadata = if h.v >= 0x1E000000: r.readL8Bytes()
        self.numBlockTypes = if h.v >= 0x05000001: r.readUInt16()
        self.blockTypes = if h.v != 0x14030102 and h.v >= 0x05000001: r.readFArray(lambda r: SizedString(r), NumBlockTypes)
        self.blockTypeHashes = if h.v >= 0x14030102 and h.v <= 0x14030102: r.readPArray(None, 'I', NumBlockTypes)
        self.blockTypeIndex = if h.v >= 0x05000001: r.readFArray(lambda r: BlockTypeIndex(r), NumBlocks)
        self.blockSize = if h.v >= 0x14020005: r.readPArray(None, 'I', NumBlocks)
        self.numStrings = if h.v >= 0x14010001: r.readUInt32()
        self.maxStringLength = if h.v >= 0x14010001: r.readUInt32()
        self.strings = if h.v >= 0x14010001: r.readFArray(lambda r: SizedString(r), NumStrings)
        self.groups = if h.v >= 0x05000006: r.readL32PArray(None, 'I')

# A list of \\0 terminated strings.
class StringPalette:
    def __init__(self, r: Reader):
        self.palette: str = r.readL32AString()          # A bunch of 0x00 seperated strings.
        self.length: int = r.readUInt32()               # Length of the palette string is repeated here.

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
class Key:
    def __init__(self, r: Reader, h: Header):
        self.time: float = r.readSingle()               # Time of the key.
        self.value: T = ??                              # The key value.
        self.forward: T = ?? if ARG == 2 else None      # Key forward tangent.
        self.backward: T = ?? if ARG == 2 else None     # The key backward tangent.
        self.tbc: TBC = TBC(r) if ARG == 3 else None    # The TBC of the key.

# Array of vector keys (anything that can be interpolated, except rotations).
class KeyGroup:
    def __init__(self, r: Reader, h: Header):
        self.numKeys: int = r.readUInt32()              # Number of keys in the array.
        self.interpolation: KeyType = KeyType(r.readUInt32()) if Num Keys != 0 else None # The key type.
        self.keys: list[Key[T]] = r.readFArray(lambda r: Key[T](r, h), NumKeys) # The keys.

# A special version of the key type used for quaternions.  Never has tangents.
class QuatKey:
    def __init__(self, r: Reader, h: Header):
        self.time: float = r.readSingle() if ARG != 4 and h.v >= 0x0A01006A else None # Time the key applies.
        self.value: T = ?? if ARG != 4 else None        # Value of the key.
        self.tbc: TBC = TBC(r) if ARG == 3 else None    # The TBC of the key.

# Texture coordinates (u,v). As in OpenGL; image origin is in the lower left corner.
class TexCoord:
    struct = ('<2f', 8)
    u: float                                            # First coordinate.
    v: float                                            # Second coordinate.

    def __init__(self, r: Reader):
        self.u = r.readSingle()
        self.v = r.readSingle()

# Texture coordinates (u,v).
class HalfTexCoord:
    def __init__(self, r: Reader):
        self.u: float = r.readHalf()                    # First coordinate.
        self.v: float = r.readHalf()                    # Second coordinate.

# Describes the order of scaling and rotation matrices. Translate, Scale, Rotation, Center are from TexDesc.
# Back = inverse of Center. FromMaya = inverse of the V axis with a positive translation along V of 1 unit.
class TransformMethod(Enum):
    Maya_Deprecated = 0,            # Center * Rotation * Back * Translate * Scale
    Max = 1,                        # Center * Scale * Rotation * Translate * Back
    Maya = 2                        # Center * Rotation * Back * FromMaya * Translate * Scale

# NiTexturingProperty::Map. Texture description.
class TexDesc:
    def __init__(self, r: Reader, h: Header):
        self.image: int = X[NiImage].ref(r) if h.v <= 0x03010000 else None # Link to the texture image.
        self.source: int = X[NiSourceTexture].ref(r) if h.v >= 0x0303000D else None # NiSourceTexture object index.
        self.clampMode: TexClampMode = TexClampMode(r.readUInt32()) if h.v <= 0x14000005 else None # 0=clamp S clamp T, 1=clamp S wrap T, 2=wrap S clamp T, 3=wrap S wrap T
        self.filterMode: TexFilterMode = TexFilterMode(r.readUInt32()) if h.v <= 0x14000005 else None # 0=nearest, 1=bilinear, 2=trilinear, 3=..., 4=..., 5=...
        self.flags: Flags = Flags(r.readUInt16()) if h.v >= 0x14010003 else None # Texture mode flags; clamp and filter mode stored in upper byte with 0xYZ00 = clamp mode Y, filter mode Z.
        self.maxAnisotropy: int = r.readUInt16() if h.v >= 0x14050004 else None
        self.uvSet: int = r.readUInt32() if h.v <= 0x14000005 else None # The texture coordinate set in NiGeometryData that this texture slot will use.
        self.pS2L: int = r.readInt16() if h.v <= 0x0A040001 else None # L can range from 0 to 3 and are used to specify how fast a texture gets blurry.
        self.pS2K: int = r.readInt16() if h.v <= 0x0A040001 else None # K is used as an offset into the mipmap levels and can range from -2047 to 2047. Positive values push the mipmap towards being blurry and negative values make the mipmap sharper.
        self.unknown1: int = r.readUInt16() if h.v <= 0x0401000C else None # Unknown, 0 or 0x0101?
        self.hasTextureTransform: bool = r.readBool32() if h.v >= 0x0A010000 else None # Whether or not the texture coordinates are transformed.
        self.translation: TexCoord = TexCoord(r) if Has Texture Transform and h.v >= 0x0A010000 else None # The UV translation.
        self.scale: TexCoord = TexCoord(r) if Has Texture Transform and h.v >= 0x0A010000 else None # The UV scale.
        self.rotation: float = r.readSingle() if Has Texture Transform and h.v >= 0x0A010000 else None # The W axis rotation in texture space.
        self.transformMethod: TransformMethod = TransformMethod(r.readUInt32()) if Has Texture Transform and h.v >= 0x0A010000 else None # Depending on the source, scaling can occur before or after rotation.
        self.center: TexCoord = TexCoord(r) if Has Texture Transform and h.v >= 0x0A010000 else None # The origin around which the texture rotates.

# NiTexturingProperty::ShaderMap. Shader texture description.
class ShaderTexDesc:
    def __init__(self, r: Reader, h: Header):
        self.hasMap: bool = r.readBool32()
        self.map: TexDesc = TexDesc(r, h) if Has Map else None
        self.mapId: int = r.readUInt32() if Has Map else None # Unique identifier for the Gamebryo shader system.

# List of three vertex indices.
class Triangle:
    struct = ('<3H', 18)
    v1: int                                             # First vertex index.
    v2: int                                             # Second vertex index.
    v3: int                                             # Third vertex index.

    def __init__(self, r: Reader):
        self.v1 = r.readUInt16()
        self.v2 = r.readUInt16()
        self.v3 = r.readUInt16()

class VertexFlags(Flag):
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

class BSVertexData:
    def __init__(self, r: Reader, h: Header):
        self.vertex: Vector3 = r.readVector3() if ((ARG & 16) != 0) and ((ARG & 16384) != 0) else None
        self.bitangentX: float = r.readSingle() if ((ARG & 16) != 0) and ((ARG & 256) != 0) and ((ARG & 16384) != 0) else None
        self.unknownShort: int = r.readUInt16() if ((ARG & 16) != 0) and ((ARG & 256) == 0) and ((ARG & 16384) == 0) else None
        self.unknownInt: int = r.readUInt32() if ((ARG & 16) != 0) and ((ARG & 256) == 0) and ((ARG & 16384) != 0) else None
        self.uv: HalfTexCoord = HalfTexCoord(r) if ((ARG & 32) != 0) else None
        self.normal: Vector3 = Vector3(r.readByte(), r.readByte(), r.readByte()) if (ARG & 128) != 0 else None
        self.bitangentY: int = r.readByte() if (ARG & 128) != 0 else None
        self.tangent: Vector3 = Vector3(r.readByte(), r.readByte(), r.readByte()) if ((ARG & 128) != 0) and ((ARG & 256) != 0) else None
        self.bitangentZ: int = r.readByte() if ((ARG & 128) != 0) and ((ARG & 256) != 0) else None
        self.vertexColors: ByteColor4 = Color4Byte(r) if (ARG & 512) != 0 else None
        self.boneWeights: list[float] = r.readFArray(lambda r: r.readHalf(), 4) if (ARG & 1024) != 0 else None
        self.boneIndices: list[int] = r.readBytes(4) if (ARG & 1024) != 0 else None
        self.eyeData: float = r.readSingle() if (ARG & 4096) != 0 else None

class BSVertexDataSSE:
    def __init__(self, r: Reader, h: Header):
        self.vertex: Vector3 = r.readVector3() if ((ARG & 16) != 0) else None
        self.bitangentX: float = r.readSingle() if ((ARG & 16) != 0) and ((ARG & 256) != 0) else None
        self.unknownInt: int = r.readInt32() if ((ARG & 16) != 0) and (ARG & 256) == 0 else None
        self.uv: HalfTexCoord = HalfTexCoord(r) if ((ARG & 32) != 0) else None
        self.normal: Vector3 = Vector3(r.readByte(), r.readByte(), r.readByte()) if (ARG & 128) != 0 else None
        self.bitangentY: int = r.readByte() if (ARG & 128) != 0 else None
        self.tangent: Vector3 = Vector3(r.readByte(), r.readByte(), r.readByte()) if ((ARG & 128) != 0) and ((ARG & 256) != 0) else None
        self.bitangentZ: int = r.readByte() if ((ARG & 128) != 0) and ((ARG & 256) != 0) else None
        self.vertexColors: ByteColor4 = Color4Byte(r) if (ARG & 512) != 0 else None
        self.boneWeights: list[float] = r.readFArray(lambda r: r.readHalf(), 4) if (ARG & 1024) != 0 else None
        self.boneIndices: list[int] = r.readBytes(4) if (ARG & 1024) != 0 else None
        self.eyeData: float = r.readSingle() if (ARG & 4096) != 0 else None

class BSVertexDesc:
    struct = ('<5bHb', 8)
    vF1: int
    vF2: int
    vF3: int
    vF4: int
    vF5: int
    vertexAttributes: VertexFlags
    vF8: int

    def __init__(self, r: Reader):
        self.vF1 = r.readByte()
        self.vF2 = r.readByte()
        self.vF3 = r.readByte()
        self.vF4 = r.readByte()
        self.vF5 = r.readByte()
        self.vertexAttributes = VertexFlags(r.readUInt16())
        self.vF8 = r.readByte()

# Skinning data for a submesh, optimized for hardware skinning. Part of NiSkinPartition.
class SkinPartition:
    def __init__(self, r: Reader, h: Header):
        self.numVertices: int = r.readUInt16()          # Number of vertices in this submesh.
        self.numTriangles: int = r.readUInt16()         # Number of triangles in this submesh.
        self.numBones: int = r.readUInt16()             # Number of bones influencing this submesh.
        self.numStrips: int = r.readUInt16()            # Number of strips in this submesh (zero if not stripped).
        self.numWeightsPerVertex: int = r.readUInt16()  # Number of weight coefficients per vertex. The Gamebryo engine seems to work well only if this number is equal to 4, even if there are less than 4 influences per vertex.
        self.bones: list[int] = r.readPArray(None, 'H', NumBones) # List of bones.
        self.hasVertexMap: bool = r.readBool32() if h.v >= 0x0A010000 else None # Do we have a vertex map?
        self.vertexMap: list[int] = r.readPArray(None, 'H', NumVertices) if Has Vertex Map and h.v >= 0x0A010000 else None # Maps the weight/influence lists in this submesh to the vertices in the shape being skinned.
        self.hasVertexWeights: bool = r.readBool32() if h.v >= 0x0A010000 else None # Do we have vertex weights?
        self.vertexWeights: list[list[float]] = r.readFArray(lambda k: r.readFArray(lambda r: r.readHalf(), NumVertices), NumWeightsPerVertex) if Has Vertex Weights == 15 and h.v >= 0x14030101 else None # The vertex weights.
        self.stripLengths: list[int] = r.readPArray(None, 'H', NumStrips) # The strip lengths.
        self.hasFaces: bool = r.readBool32() if h.v >= 0x0A010000 else None # Do we have triangle or strip data?
        self.strips: list[list[int]] = r.readFArray(lambda k: r.readPArray(None, 'H', NumStrips), StripLengths) if (Has Faces) and (Num Strips != 0) and h.v >= 0x0A010000 else None # The strips.
        self.triangles: list[Triangle] = r.readFArray(lambda r: Triangle(r), NumTriangles) if (Has Faces) and (Num Strips == 0) and h.v >= 0x0A010000 else None # The triangles.
        self.hasBoneIndices: bool = r.readBool32()      # Do we have bone indices?
        self.boneIndices: list[list[int]] = r.readFArray(lambda k: r.readBytes(NumVertices), NumWeightsPerVertex) if Has Bone Indices else None # Bone indices, they index into 'Bones'.
        self.unknownShort: int = r.readUInt16()         # Unknown
        self.vertexDesc: BSVertexDesc = BSVertexDesc(r) if (h.uv2 == 100) else None
        self.trianglesCopy: list[Triangle] = r.readFArray(lambda r: Triangle(r), NumTriangles) if (h.uv2 == 100) else None

# A plane.
class NiPlane:
    struct = ('<4f', 24)
    normal: Vector3                                     # The plane normal.
    constant: float                                     # The plane constant.

    def __init__(self, r: Reader):
        self.normal = r.readVector3()
        self.constant = r.readSingle()

# A sphere.
class NiBound:
    struct = ('<4f', 24)
    center: Vector3                                     # The sphere's center.
    radius: float                                       # The sphere's radius.

    def __init__(self, r: Reader):
        self.center = r.readVector3()
        self.radius = r.readSingle()

class NiQuatTransform:
    translation: Vector3
    rotation: Quaternion
    scale: float
    trsValid: list[bool]                                # Whether each transform component is valid.

    def __init__(self, r: Reader, h: Header):
        self.translation = r.readVector3()
        self.rotation = r.readQuaternion()
        self.scale = r.readSingle()
        self.trsValid = if h.v <= 0x0A01006D: r.readBytes(3)

class NiTransform:
    rotation: Matrix3x3                                 # The rotation part of the transformation matrix.
    translation: Vector3                                # The translation vector.
    scale: float                                        # Scaling part (only uniform scaling is supported).

    def __init__(self, r: Reader):
        self.rotation = r.readMatrix3x3()
        self.translation = r.readVector3()
        self.scale = r.readSingle()

# Bethesda Animation. Furniture entry points. It specifies the direction(s) from where the actor is able to enter (and leave) the position.
class FurnitureEntryPoints(Flag):
    Front = 0,                      # front entry point
    Behind = 1 << 1,                # behind entry point
    Right = 1 << 2,                 # right entry point
    Left = 1 << 3,                  # left entry point
    Up = 1 << 4                     # up entry point - unknown function. Used on some beds in Skyrim, probably for blocking of sleeping position.

# Bethesda Animation. Animation type used on this position. This specifies the function of this position.
class AnimationType(Enum):
    Sit = 1,                        # Actor use sit animation.
    Sleep = 2,                      # Actor use sleep animation.
    Lean = 4                        # Used for lean animations?

# Bethesda Animation. Describes a furniture position?
class FurniturePosition:
    def __init__(self, r: Reader):
        self.offset: Vector3 = r.readVector3()          # Offset of furniture marker.
        self.orientation: int = r.readUInt16()          # Furniture marker orientation.
        self.positionRef1: int = r.readByte()           # Refers to a furnituremarkerxx.nif file. Always seems to be the same as Position Ref 2.
        self.positionRef2: int = r.readByte()           # Refers to a furnituremarkerxx.nif file. Always seems to be the same as Position Ref 1.
        self.heading: float = r.readSingle()            # Similar to Orientation, in float form.
        self.animationType: AnimationType = AnimationType(r.readUInt16()) # Unknown
        self.entryProperties: FurnitureEntryPoints = FurnitureEntryPoints(r.readUInt16()) # Unknown/unused in nif?

# Bethesda Havok. A triangle with extra data used for physics.
class TriangleData:
    def __init__(self, r: Reader, h: Header):
        self.triangle: Triangle = Triangle(r)           # The triangle.
        self.weldingInfo: int = r.readUInt16()          # Additional havok information on how triangles are welded.
        self.normal: Vector3 = r.readVector3() if h.v <= 0x14000005 else None # This is the triangle's normal.

# Geometry morphing data component.
class Morph:
    def __init__(self, r: Reader, h: Header):
        self.frameName: str = Y.string(r) if h.v >= 0x0A01006A else None # Name of the frame.
        self.numKeys: int = r.readUInt32() if h.v <= 0x0A010000 else None # The number of morph keys that follow.
        self.interpolation: KeyType = KeyType(r.readUInt32()) if h.v <= 0x0A010000 else None # Unlike most objects, the presense of this value is not conditional on there being keys.
        self.keys: list[Key[T]] = r.readFArray(lambda r: Key[T](r, h), NumKeys) if h.v <= 0x0A010000 else None # The morph key frames.
        self.legacyWeight: float = r.readSingle() if h.v >= 0x0A010068 and h.v <= 0x14010002 and  else None
        self.vectors: list[Vector3] = r.readFArray(lambda r: r.readVector3(), ARG) # Morph vectors.

# particle array entry
class Particle:
    struct = ('<9f2H', 40)
    velocity: Vector3                                   # Particle velocity
    unknownVector: Vector3                              # Unknown
    lifetime: float                                     # The particle age.
    lifespan: float                                     # Maximum age of the particle.
    timestamp: float                                    # Timestamp of the last update.
    unknownShort: int                                   # Unknown short
    vertexId: int                                       # Particle/vertex index matches array index

    def __init__(self, r: Reader):
        self.velocity = r.readVector3()
        self.unknownVector = r.readVector3()
        self.lifetime = r.readSingle()
        self.lifespan = r.readSingle()
        self.timestamp = r.readSingle()
        self.unknownShort = r.readUInt16()
        self.vertexId = r.readUInt16()

# NiSkinData::BoneData. Skinning data component.
class BoneData:
    def __init__(self, r: Reader, h: Header):
        self.skinTransform: NiTransform = NiTransform(r)# Offset of the skin from this bone in bind position.
        self.boundingSphereOffset: Vector3 = r.readVector3() # Translation offset of a bounding sphere holding all vertices. (Note that its a Sphere Containing Axis Aligned Box not a minimum volume Sphere)
        self.boundingSphereRadius: float = r.readSingle() # Radius for bounding sphere holding all vertices.
        self.unknown13Shorts: list[int] = r.readPArray(None, 'h', 13) if h.v >= 0x14030009 and h.v <= 0x14030009 and  else None # Unknown, always 0?
        self.numVertices: int = r.readUInt16()          # Number of weighted vertices.
        self.vertexWeights: list[BoneVertData] = r.readFArray(lambda r: BoneVertData(r, true), NumVertices) if ARG == 15 and h.v >= 0x14030101 else None # The vertex weights.

# Bethesda Havok. Collision filter info representing Layer, Flags, Part Number, and Group all combined into one uint.
class HavokFilter:
    def __init__(self, r: Reader, h: Header):
        self.layer: SkyrimLayer = SkyrimLayer(r.readByte()) # The layer the collision belongs to.
        self.flagsandPartNumber: int = r.readByte()     # FLAGS are stored in highest 3 bits:
                                                        # 	Bit 7: sets the LINK property and controls whether this body is physically linked to others.
                                                        # 	Bit 6: turns collision off (not used for Layer BIPED).
                                                        # 	Bit 5: sets the SCALED property.
                                                        # 
                                                        # 	PART NUMBER is stored in bits 0-4. Used only when Layer is set to BIPED.
                                                        # 
                                                        # 	Part Numbers for Oblivion, Fallout 3, Skyrim:
                                                        # 	0 - OTHER
                                                        # 	1 - HEAD
                                                        # 	2 - BODY
                                                        # 	3 - SPINE1
                                                        # 	4 - SPINE2
                                                        # 	5 - LUPPERARM
                                                        # 	6 - LFOREARM
                                                        # 	7 - LHAND
                                                        # 	8 - LTHIGH
                                                        # 	9 - LCALF
                                                        # 	10 - LFOOT
                                                        # 	11 - RUPPERARM
                                                        # 	12 - RFOREARM
                                                        # 	13 - RHAND
                                                        # 	14 - RTHIGH
                                                        # 	15 - RCALF
                                                        # 	16 - RFOOT
                                                        # 	17 - TAIL
                                                        # 	18 - SHIELD
                                                        # 	19 - QUIVER
                                                        # 	20 - WEAPON
                                                        # 	21 - PONYTAIL
                                                        # 	22 - WING
                                                        # 	23 - PACK
                                                        # 	24 - CHAIN
                                                        # 	25 - ADDONHEAD
                                                        # 	26 - ADDONCHEST
                                                        # 	27 - ADDONARM
                                                        # 	28 - ADDONLEG
                                                        # 	29-31 - NULL
        self.group: int = r.readUInt16()

# Bethesda Havok. Material wrapper for varying material enums by game.
class HavokMaterial:
    def __init__(self, r: Reader, h: Header):
        self.unknownInt: int = r.readUInt32() if h.v <= 0x0A000102 else None
        self.material: SkyrimHavokMaterial = SkyrimHavokMaterial(r.readUInt32()) # The material of the shape.

# Bethesda Havok. Havok Information for packed TriStrip shapes.
class OblivionSubShape:
    def __init__(self, r: Reader):
        self.havokFilter: HavokFilter = HavokFilter(r, h)
        self.numVertices: int = r.readUInt32()          # The number of vertices that form this sub shape.
        self.material: HavokMaterial = HavokMaterial(r, h) # The material of the subshape.

class bhkPositionConstraintMotor:
    def __init__(self, r: Reader):
        self.minForce: float = r.readSingle()           # Minimum motor force
        self.maxForce: float = r.readSingle()           # Maximum motor force
        self.tau: float = r.readSingle()                # Relative stiffness
        self.damping: float = r.readSingle()            # Motor damping value
        self.proportionalRecoveryVelocity: float = r.readSingle() # A factor of the current error to calculate the recovery velocity
        self.constantRecoveryVelocity: float = r.readSingle() # A constant velocity which is used to recover from errors
        self.motorEnabled: bool = r.readBool32()        # Is Motor enabled

class bhkVelocityConstraintMotor:
    def __init__(self, r: Reader):
        self.minForce: float = r.readSingle()           # Minimum motor force
        self.maxForce: float = r.readSingle()           # Maximum motor force
        self.tau: float = r.readSingle()                # Relative stiffness
        self.targetVelocity: float = r.readSingle()
        self.useVelocityTarget: bool = r.readBool32()
        self.motorEnabled: bool = r.readBool32()        # Is Motor enabled

class bhkSpringDamperConstraintMotor:
    def __init__(self, r: Reader):
        self.minForce: float = r.readSingle()           # Minimum motor force
        self.maxForce: float = r.readSingle()           # Maximum motor force
        self.springConstant: float = r.readSingle()     # The spring constant in N/m
        self.springDamping: float = r.readSingle()      # The spring damping in Nsec/m
        self.motorEnabled: bool = r.readBool32()        # Is Motor enabled

class MotorType(Enum):
    MOTOR_NONE = 0,
    MOTOR_POSITION = 1,
    MOTOR_VELOCITY = 2,
    MOTOR_SPRING = 3

class MotorDescriptor:
    def __init__(self, r: Reader, h: Header):
        self.type: MotorType = MotorType(r.readByte())
        self.positionMotor: bhkPositionConstraintMotor = bhkPositionConstraintMotor(r) if Type == 1 else None
        self.velocityMotor: bhkVelocityConstraintMotor = bhkVelocityConstraintMotor(r) if Type == 2 else None
        self.springDamperMotor: bhkSpringDamperConstraintMotor = bhkSpringDamperConstraintMotor(r) if Type == 3 else None

# This constraint defines a cone in which an object can rotate. The shape of the cone can be controlled in two (orthogonal) directions.
class RagdollDescriptor:
    def __init__(self, r: Reader, h: Header):
        self.pivotA: Vector4 = r.readVector4()          # Point around which the object will rotate. Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
        self.planeA: Vector4 = r.readVector4()          # Defines the orthogonal plane in which the body can move, the orthogonal directions in which the shape can be controlled (the direction orthogonal on this one and Twist A).
        self.twistA: Vector4 = r.readVector4()          # Central directed axis of the cone in which the object can rotate. Orthogonal on Plane A.
        self.pivotB: Vector4 = r.readVector4()          # Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
        self.planeB: Vector4 = r.readVector4()          # Defines the orthogonal plane in which the body can move, the orthogonal directions in which the shape can be controlled (the direction orthogonal on this one and Twist A).
        self.twistB: Vector4 = r.readVector4()          # Central directed axis of the cone in which the object can rotate. Orthogonal on Plane B.
        self.motorA: Vector4 = r.readVector4()          # Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
        self.motorB: Vector4 = r.readVector4()          # Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
        self.coneMaxAngle: float = r.readSingle()       # Maximum angle the object can rotate around the vector orthogonal on Plane A and Twist A relative to the Twist A vector. Note that Cone Min Angle is not stored, but is simply minus this angle.
        self.planeMinAngle: float = r.readSingle()      # Minimum angle the object can rotate around Plane A, relative to Twist A.
        self.planeMaxAngle: float = r.readSingle()      # Maximum angle the object can rotate around Plane A, relative to Twist A.
        self.twistMinAngle: float = r.readSingle()      # Minimum angle the object can rotate around Twist A, relative to Plane A.
        self.twistMaxAngle: float = r.readSingle()      # Maximum angle the object can rotate around Twist A, relative to Plane A.
        self.maxFriction: float = r.readSingle()        # Maximum friction, typically 0 or 10. In Fallout 3, typically 100.
        self.motor: MotorDescriptor = MotorDescriptor(r, h) if h.v >= 0x14020007 and  else None

# This constraint allows rotation about a specified axis, limited by specified boundaries.
class LimitedHingeDescriptor:
    def __init__(self, r: Reader, h: Header):
        self.pivotA: Vector4 = r.readVector4()          # Pivot point around which the object will rotate.
        self.axleA: Vector4 = r.readVector4()           # Axis of rotation.
        self.perp2AxleInA1: Vector4 = r.readVector4()   # Vector in the rotation plane which defines the zero angle.
        self.perp2AxleInA2: Vector4 = r.readVector4()   # Vector in the rotation plane, orthogonal on the previous one, which defines the positive direction of rotation. This is always the vector product of Axle A and Perp2 Axle In A1.
        self.pivotB: Vector4 = r.readVector4()          # Pivot A in second entity coordinate system.
        self.axleB: Vector4 = r.readVector4()           # Axle A in second entity coordinate system.
        self.perp2AxleInB2: Vector4 = r.readVector4()   # Perp2 Axle In A2 in second entity coordinate system.
        self.perp2AxleInB1: Vector4 = r.readVector4()   # Perp2 Axle In A1 in second entity coordinate system.
        self.minAngle: float = r.readSingle()           # Minimum rotation angle.
        self.maxAngle: float = r.readSingle()           # Maximum rotation angle.
        self.maxFriction: float = r.readSingle()        # Maximum friction, typically either 0 or 10. In Fallout 3, typically 100.
        self.motor: MotorDescriptor = MotorDescriptor(r, h) if h.v >= 0x14020007 and  else None

# This constraint allows rotation about a specified axis.
class HingeDescriptor:
    def __init__(self, r: Reader, h: Header):
        self.pivotA: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Pivot point around which the object will rotate.
        self.perp2AxleInA1: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Vector in the rotation plane which defines the zero angle.
        self.perp2AxleInA2: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Vector in the rotation plane, orthogonal on the previous one, which defines the positive direction of rotation. This is always the vector product of Axle A and Perp2 Axle In A1.
        self.pivotB: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Pivot A in second entity coordinate system.
        self.axleB: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Axle A in second entity coordinate system.
        self.axleA: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Axis of rotation.
        self.perp2AxleInB1: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Perp2 Axle In A1 in second entity coordinate system.
        self.perp2AxleInB2: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Perp2 Axle In A2 in second entity coordinate system.

class BallAndSocketDescriptor:
    def __init__(self, r: Reader):
        self.pivotA: Vector4 = r.readVector4()          # Pivot point in the local space of entity A.
        self.pivotB: Vector4 = r.readVector4()          # Pivot point in the local space of entity B.

class PrismaticDescriptor:
    def __init__(self, r: Reader, h: Header):
        self.pivotA: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Pivot.
        self.rotationA: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Rotation axis.
        self.planeA: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Plane normal. Describes the plane the object is able to move on.
        self.slidingA: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Describes the axis the object is able to travel along. Unit vector.
        self.pivotB: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Pivot in B coordinates.
        self.rotationB: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Rotation axis.
        self.planeB: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Plane normal. Describes the plane the object is able to move on in B coordinates.
        self.slidingB: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Describes the axis the object is able to travel along in B coordinates. Unit vector.
        self.minDistance: float = r.readSingle()        # Describe the min distance the object is able to travel.
        self.maxDistance: float = r.readSingle()        # Describe the max distance the object is able to travel.
        self.friction: float = r.readSingle()           # Friction.
        self.motor: MotorDescriptor = MotorDescriptor(r, h) if h.v >= 0x14020007 and  else None

class StiffSpringDescriptor:
    def __init__(self, r: Reader):
        self.pivotA: Vector4 = r.readVector4()
        self.pivotB: Vector4 = r.readVector4()
        self.length: float = r.readSingle()

# Used to store skin weights in NiTriShapeSkinController.
class OldSkinData:
    def __init__(self, r: Reader):
        self.vertexWeight: float = r.readSingle()       # The amount that this bone affects the vertex.
        self.vertexIndex: int = r.readUInt16()          # The index of the vertex that this weight applies to.
        self.unknownVector: Vector3 = r.readVector3()   # Unknown.  Perhaps some sort of offset?

# Determines how the raw image data is stored in NiRawImageData.
class ImageType(Enum):
    RGB = 1,                        # Colors store red, blue, and green components.
    RGBA = 2                        # Colors store red, blue, green, and alpha components.

# Box Bounding Volume
class BoxBV:
    def __init__(self, r: Reader):
        self.center: Vector3 = r.readVector3()
        self.axis: list[Vector3] = r.readFArray(lambda r: r.readVector3(), 3)
        self.extent: Vector3 = r.readVector3()

# Capsule Bounding Volume
class CapsuleBV:
    def __init__(self, r: Reader):
        self.center: Vector3 = r.readVector3()
        self.origin: Vector3 = r.readVector3()
        self.extent: float = r.readSingle()
        self.radius: float = r.readSingle()

class HalfSpaceBV:
    def __init__(self, r: Reader):
        self.plane: NiPlane = NiPlane(r)
        self.center: Vector3 = r.readVector3()

class BoundingVolume:
    def __init__(self, r: Reader, h: Header):
        self.collisionType: BoundVolumeType = BoundVolumeType(r.readUInt32()) # Type of collision data.
        self.sphere: NiBound = NiBound(r) if Collision Type == 0 else None
        self.box: BoxBV = BoxBV(r) if Collision Type == 1 else None
        self.capsule: CapsuleBV = CapsuleBV(r) if Collision Type == 2 else None
        self.union: UnionBV = UnionBV(r) if Collision Type == 4 else None
        self.halfSpace: HalfSpaceBV = HalfSpaceBV(r) if Collision Type == 5 else None

class UnionBV:
    def __init__(self, r: Reader):
        self.boundingVolumes: list[BoundingVolume] = r.readL32FArray(lambda r: BoundingVolume(r, h))

class MorphWeight:
    def __init__(self, r: Reader):
        self.interpolator: int = X[NiInterpolator].ref(r)
        self.weight: float = r.readSingle()

# Transformation data for the bone at this index in bhkPoseArray.
class BoneTransform:
    def __init__(self, r: Reader):
        self.translation: Vector3 = r.readVector3()
        self.rotation: Quaternion = r.readQuaternionWFirst()
        self.scale: Vector3 = r.readVector3()

# A list of transforms for each bone in bhkPoseArray.
class BonePose:
    def __init__(self, r: Reader):
        self.transforms: list[BoneTransform] = r.readL32FArray(lambda r: BoneTransform(r))

# Array of Vectors for Decal placement in BSDecalPlacementVectorExtraData.
class DecalVectorArray:
    def __init__(self, r: Reader):
        self.numVectors: int = r.readInt16()
        self.points: list[Vector3] = r.readFArray(lambda r: r.readVector3(), NumVectors) # Vector XYZ coords
        self.normals: list[Vector3] = r.readFArray(lambda r: r.readVector3(), NumVectors) # Vector Normals

# Editor flags for the Body Partitions.
class BSPartFlag(Flag):
    PF_EDITOR_VISIBLE = 0,          # Visible in Editor
    PF_START_NET_BONESET = 1 << 8   # Start a new shared boneset.  It is expected this BoneSet and the following sets in the Skin Partition will have the same bones.

# Body part list for DismemberSkinInstance
class BodyPartList:
    def __init__(self, r: Reader):
        self.partFlag: BSPartFlag = BSPartFlag(r.readUInt16()) # Flags related to the Body Partition
        self.bodyPart: BSDismemberBodyPartType = BSDismemberBodyPartType(r.readUInt16()) # Body Part Index

# Stores Bone Level of Detail info in a BSBoneLODExtraData
class BoneLOD:
    def __init__(self, r: Reader):
        self.distance: int = r.readUInt32()
        self.boneName: str = Y.string(r)

# Per-chunk material, used in bhkCompressedMeshShapeData
class bhkCMSDMaterial:
    def __init__(self, r: Reader):
        self.material: SkyrimHavokMaterial = SkyrimHavokMaterial(r.readUInt32())
        self.filter: HavokFilter = HavokFilter(r, h)

# Triangle indices used in pair with "Big Verts" in a bhkCompressedMeshShapeData.
class bhkCMSDBigTris:
    def __init__(self, r: Reader):
        self.triangle1: int = r.readUInt16()
        self.triangle2: int = r.readUInt16()
        self.triangle3: int = r.readUInt16()
        self.material: int = r.readUInt32()             # Always 0?
        self.weldingInfo: int = r.readUInt16()

# A set of transformation data: translation and rotation
class bhkCMSDTransform:
    def __init__(self, r: Reader):
        self.translation: Vector4 = r.readVector4()     # A vector that moves the chunk by the specified amount. W is not used.
        self.rotation: Quaternion = r.readQuaternionWFirst() # Rotation. Reference point for rotation is bhkRigidBody translation.

# Defines subshape chunks in bhkCompressedMeshShapeData
class bhkCMSDChunk:
    def __init__(self, r: Reader):
        self.translation: Vector4 = r.readVector4()
        self.materialIndex: int = r.readUInt32()        # Index of material in bhkCompressedMeshShapeData::Chunk Materials
        self.reference: int = r.readUInt16()            # Always 65535?
        self.transformIndex: int = r.readUInt16()       # Index of transformation in bhkCompressedMeshShapeData::Chunk Transforms
        self.vertices: list[int] = r.readL32PArray(None, 'H')
        self.indices: list[int] = r.readL32PArray(None, 'H')
        self.strips: list[int] = r.readL32PArray(None, 'H')
        self.weldingInfo: list[int] = r.readL32PArray(None, 'H')

class MalleableDescriptor:
    def __init__(self, r: Reader, h: Header):
        self.type: hkConstraintType = hkConstraintType(r.readUInt32()) # Type of constraint.
        self.numEntities: int = r.readUInt32()          # Always 2 (Hardcoded). Number of bodies affected by this constraint.
        self.entityA: int = X[bhkEntity].ptr(r)         # Usually NONE. The entity affected by this constraint.
        self.entityB: int = X[bhkEntity].ptr(r)         # Usually NONE. The entity affected by this constraint.
        self.priority: int = r.readUInt32()             # Usually 1. Higher values indicate higher priority of this constraint?
        self.ballandSocket: BallAndSocketDescriptor = BallAndSocketDescriptor(r) if Type == 0 else None
        self.hinge: HingeDescriptor = HingeDescriptor(r, h) if Type == 1 else None
        self.limitedHinge: LimitedHingeDescriptor = LimitedHingeDescriptor(r, h) if Type == 2 else None
        self.prismatic: PrismaticDescriptor = PrismaticDescriptor(r, h) if Type == 6 else None
        self.ragdoll: RagdollDescriptor = RagdollDescriptor(r, h) if Type == 7 else None
        self.stiffSpring: StiffSpringDescriptor = StiffSpringDescriptor(r) if Type == 8 else None
        self.tau: float = r.readSingle() if h.v <= 0x14000005 else None
        self.damping: float = r.readSingle() if h.v <= 0x14000005 else None
        self.strength: float = r.readSingle() if h.v >= 0x14020007 else None

class ConstraintData:
    def __init__(self, r: Reader, h: Header):
        self.type: hkConstraintType = hkConstraintType(r.readUInt32()) # Type of constraint.
        self.numEntities2: int = r.readUInt32()         # Always 2 (Hardcoded). Number of bodies affected by this constraint.
        self.entityA: int = X[bhkEntity].ptr(r)         # Usually NONE. The entity affected by this constraint.
        self.entityB: int = X[bhkEntity].ptr(r)         # Usually NONE. The entity affected by this constraint.
        self.priority: int = r.readUInt32()             # Usually 1. Higher values indicate higher priority of this constraint?
        self.ballandSocket: BallAndSocketDescriptor = BallAndSocketDescriptor(r) if Type == 0 else None
        self.hinge: HingeDescriptor = HingeDescriptor(r, h) if Type == 1 else None
        self.limitedHinge: LimitedHingeDescriptor = LimitedHingeDescriptor(r, h) if Type == 2 else None
        self.prismatic: PrismaticDescriptor = PrismaticDescriptor(r, h) if Type == 6 else None
        self.ragdoll: RagdollDescriptor = RagdollDescriptor(r, h) if Type == 7 else None
        self.stiffSpring: StiffSpringDescriptor = StiffSpringDescriptor(r) if Type == 8 else None
        self.malleable: MalleableDescriptor = MalleableDescriptor(r, h) if Type == 13 else None

#endregion

#region NIF Objects

# Abstract object type.
class NiObject:

    def __init__(self, r: Reader, h: Header):

# Unknown.
class Ni3dsAlphaAnimator(NiObject):
    unknown1: list[int]                                 # Unknown.
    parent: int                                         # The parent?
    num1: int                                           # Unknown.
    num2: int                                           # Unknown.
    unknown2: list[list[int]]                           # Unknown.

    def __init__(self, r: Reader, h: Header):
        self.unknown1 = r.readBytes(40)
        self.parent = X[NiObject].ref(r)
        self.num1 = r.readUInt32()
        self.num2 = r.readUInt32()
        self.unknown2 = r.readFArray(lambda k: r.readPArray(None, 'I', Num1), Num2)

# Unknown. Only found in 2.3 nifs.
class Ni3dsAnimationNode(NiObject):
    name: str                                           # Name of this object.
    hasData: bool                                       # Unknown.
    unknownFloats1: list[float]                         # Unknown. Matrix?
    unknownShort: int                                   # Unknown.
    child: int                                          # Child?
    unknownFloats2: list[float]                         # Unknown.
    count: int                                          # A count.
    unknownArray: list[list[int]]                       # Unknown.

    def __init__(self, r: Reader, h: Header):
        self.name = Y.string(r)
        self.hasData = r.readBool32()
        self.unknownFloats1 = if Has Data: r.readPArray(None, 'f', 21)
        self.unknownShort = if Has Data: r.readUInt16()
        self.child = if Has Data: X[NiObject].ref(r)
        self.unknownFloats2 = if Has Data: r.readPArray(None, 'f', 12)
        self.count = if Has Data: r.readUInt32()
        self.unknownArray = if Has Data: r.readFArray(lambda k: r.readBytes(Count), 5)

# Unknown!
class Ni3dsColorAnimator(NiObject):
    unknown1: list[int]                                 # Unknown.

    def __init__(self, r: Reader, h: Header):
        self.unknown1 = r.readBytes(184)

# Unknown!
class Ni3dsMorphShape(NiObject):
    unknown1: list[int]                                 # Unknown.

    def __init__(self, r: Reader, h: Header):
        self.unknown1 = r.readBytes(14)

# Unknown!
class Ni3dsParticleSystem(NiObject):
    unknown1: list[int]                                 # Unknown.

    def __init__(self, r: Reader, h: Header):
        self.unknown1 = r.readBytes(14)

# Unknown!
class Ni3dsPathController(NiObject):
    unknown1: list[int]                                 # Unknown.

    def __init__(self, r: Reader, h: Header):
        self.unknown1 = r.readBytes(20)

# LEGACY (pre-10.1). Abstract base class for particle system modifiers.
class NiParticleModifier(NiObject):
    nextModifier: int                                   # Next particle modifier.
    controller: int                                     # Points to the particle system controller parent.

    def __init__(self, r: Reader, h: Header):
        self.nextModifier = X[NiParticleModifier].ref(r)
        self.controller = if h.v >= 0x04000002: X[NiParticleSystemController].ptr(r)

# Particle system collider.
class NiPSysCollider(NiObject):
    bounce: float                                       # Amount of bounce for the collider.
    spawnonCollide: bool                                # Spawn particles on impact?
    dieonCollide: bool                                  # Kill particles on impact?
    spawnModifier: int                                  # Spawner to use for the collider.
    parent: int                                         # Link to parent.
    nextCollider: int                                   # The next collider.
    colliderObject: int                                 # The object whose position and orientation are the basis of the collider.

    def __init__(self, r: Reader, h: Header):
        self.bounce = r.readSingle()
        self.spawnonCollide = r.readBool32()
        self.dieonCollide = r.readBool32()
        self.spawnModifier = X[NiPSysSpawnModifier].ref(r)
        self.parent = X[NiPSysColliderManager].ptr(r)
        self.nextCollider = X[NiPSysCollider].ref(r)
        self.colliderObject = X[NiAVObject].ptr(r)

class BroadPhaseType(Enum):
    BROAD_PHASE_INVALID = 0,
    BROAD_PHASE_ENTITY = 1,
    BROAD_PHASE_PHANTOM = 2,
    BROAD_PHASE_BORDER = 3

class hkWorldObjCinfoProperty:
    def __init__(self, r: Reader):
        self.data: int = r.readUInt32()
        self.size: int = r.readUInt32()
        self.capacityandFlags: int = r.readUInt32()

# The base type of most Bethesda-specific Havok-related NIF objects.
class bhkRefObject(NiObject):

    def __init__(self, r: Reader, h: Header):

# Havok objects that can be saved and loaded from disk?
class bhkSerializable(bhkRefObject):

    def __init__(self, r: Reader, h: Header):

# Havok objects that have a position in the world?
class bhkWorldObject(bhkSerializable):
    shape: int                                          # Link to the body for this collision object.
    unknownInt: int
    havokFilter: HavokFilter
    unused: list[int]                                   # Garbage data from memory.
    broadPhaseType: BroadPhaseType
    unusedBytes: list[int]
    cinfoProperty: hkWorldObjCinfoProperty

    def __init__(self, r: Reader, h: Header):
        self.shape = X[bhkShape].ref(r)
        self.unknownInt = if h.v <= 0x0A000100: r.readUInt32()
        self.havokFilter = HavokFilter(r, h)
        self.unused = r.readBytes(4)
        self.broadPhaseType = BroadPhaseType(r.readByte())
        self.unusedBytes = r.readBytes(3)
        self.cinfoProperty = hkWorldObjCinfoProperty(r)

# Havok object that do not react with other objects when they collide (causing deflection, etc.) but still trigger collision notifications to the game.  Possible uses are traps, portals, AI fields, etc.
class bhkPhantom(bhkWorldObject):

    def __init__(self, r: Reader, h: Header):

# A Havok phantom that uses a Havok shape object for its collision volume instead of just a bounding box.
class bhkShapePhantom(bhkPhantom):

    def __init__(self, r: Reader, h: Header):

# Unknown shape.
class bhkSimpleShapePhantom(bhkShapePhantom):
    unused2: list[int]                                  # Garbage data from memory.
    transform: Matrix4x4

    def __init__(self, r: Reader, h: Header):
        self.unused2 = r.readBytes(8)
        self.transform = r.readMatrix4x4()

# A havok node, describes physical properties.
class bhkEntity(bhkWorldObject):

    def __init__(self, r: Reader, h: Header):

# This is the default body type for all "normal" usable and static world objects. The "T" suffix
# marks this body as active for translation and rotation, a normal bhkRigidBody ignores those
# properties. Because the properties are equal, a bhkRigidBody may be renamed into a bhkRigidBodyT and vice-versa.
class bhkRigidBody(bhkEntity):
    collisionResponse: hkResponseType                   # How the body reacts to collisions. See hkResponseType for hkpWorld default implementations.
    unusedByte1: int                                    # Skipped over when writing Collision Response and Callback Delay.
    processContactCallbackDelay: int                    # Lowers the frequency for processContactCallbacks. A value of 5 means that a callback is raised every 5th frame. The default is once every 65535 frames.
    unknownInt1: int                                    # Unknown.
    havokFilterCopy: HavokFilter                        # Copy of Havok Filter
    unused2: list[int]                                  # Garbage data from memory. Matches previous Unused value.
    unknownInt2: int
    collisionResponse2: hkResponseType
    unusedByte2: int                                    # Skipped over when writing Collision Response and Callback Delay.
    processContactCallbackDelay2: int
    translation: Vector4                                # A vector that moves the body by the specified amount. Only enabled in bhkRigidBodyT objects.
    rotation: Quaternion                                # The rotation Yaw/Pitch/Roll to apply to the body. Only enabled in bhkRigidBodyT objects.
    linearVelocity: Vector4                             # Linear velocity.
    angularVelocity: Vector4                            # Angular velocity.
    inertiaTensor: Matrix3x4                            # Defines how the mass is distributed among the body, i.e. how difficult it is to rotate around any given axis.
    center: Vector4                                     # The body's center of mass.
    mass: float                                         # The body's mass in kg. A mass of zero represents an immovable object.
    linearDamping: float                                # Reduces the movement of the body over time. A value of 0.1 will remove 10% of the linear velocity every second.
    angularDamping: float                               # Reduces the movement of the body over time. A value of 0.05 will remove 5% of the angular velocity every second.
    timeFactor: float
    gravityFactor: float
    friction: float                                     # How smooth its surfaces is and how easily it will slide along other bodies.
    rollingFrictionMultiplier: float
    restitution: float                                  # How "bouncy" the body is, i.e. how much energy it has after colliding. Less than 1.0 loses energy, greater than 1.0 gains energy.
                                                        #     If the restitution is not 0.0 the object will need extra CPU for all new collisions.
    maxLinearVelocity: float                            # Maximal linear velocity.
    maxAngularVelocity: float                           # Maximal angular velocity.
    penetrationDepth: float                             # The maximum allowed penetration for this object.
                                                        #     This is a hint to the engine to see how much CPU the engine should invest to keep this object from penetrating.
                                                        #     A good choice is 5% - 20% of the smallest diameter of the object.
    motionSystem: hkMotionType                          # Motion system? Overrides Quality when on Keyframed?
    deactivatorType: hkDeactivatorType                  # The initial deactivator type of the body.
    enableDeactivation: bool
    solverDeactivation: hkSolverDeactivation            # How aggressively the engine will try to zero the velocity for slow objects. This does not save CPU.
    qualityType: hkQualityType                          # The type of interaction with other objects.
    unknownFloat1: float
    unknownBytes1: list[int]                            # Unknown.
    unknownBytes2: list[int]                            # Unknown. Skyrim only.
    constraints: list[int]
    bodyFlags: int                                      # 1 = respond to wind

    def __init__(self, r: Reader, h: Header):
        self.collisionResponse = hkResponseType(r.readByte())
        self.unusedByte1 = r.readByte()
        self.processContactCallbackDelay = r.readUInt16()
        self.unknownInt1 = if h.v >= 0x0A010000: r.readUInt32()
        self.havokFilterCopy = if h.v >= 0x0A010000: HavokFilter(r, h)
        self.unused2 = if h.v >= 0x0A010000: r.readBytes(4)
        self.unknownInt2 = if h.v >= 0x0A010000 and : r.readUInt32()
        self.collisionResponse2 = if h.v >= 0x0A010000: hkResponseType(r.readByte())
        self.unusedByte2 = if h.v >= 0x0A010000: r.readByte()
        self.processContactCallbackDelay2 = if h.v >= 0x0A010000: r.readUInt16()
        self.unknownInt2 = r.readUInt32()
        self.translation = r.readVector4()
        self.rotation = r.readQuaternionWFirst()
        self.linearVelocity = r.readVector4()
        self.angularVelocity = r.readVector4()
        self.inertiaTensor = r.readMatrix3x4()
        self.center = r.readVector4()
        self.mass = r.readSingle()
        self.linearDamping = r.readSingle()
        self.angularDamping = r.readSingle()
        self.timeFactor = r.readSingle()
        self.gravityFactor = r.readSingle()
        self.friction = r.readSingle()
        self.rollingFrictionMultiplier = r.readSingle()
        self.restitution = r.readSingle()
        self.maxLinearVelocity = if h.v >= 0x0A010000: r.readSingle()
        self.maxAngularVelocity = if h.v >= 0x0A010000: r.readSingle()
        self.penetrationDepth = if h.v >= 0x0A010000 and : r.readSingle()
        self.motionSystem = hkMotionType(r.readByte())
        self.deactivatorType = hkDeactivatorType(r.readByte())
        self.enableDeactivation = r.readBool32()
        self.solverDeactivation = hkSolverDeactivation(r.readByte())
        self.qualityType = hkQualityType(r.readByte())
        self.penetrationDepth = r.readSingle()
        self.unknownFloat1 = r.readSingle()
        self.unknownBytes1 = r.readBytes(12)
        self.unknownBytes2 = r.readBytes(4)
        self.constraints = r.readL32FArray(X[bhkSerializable].ref)
        self.bodyFlags = r.readUInt32()
        self.bodyFlags = r.readUInt16()

# The "T" suffix marks this body as active for translation and rotation.
class bhkRigidBodyT(bhkRigidBody):

    def __init__(self, r: Reader, h: Header):

# Describes a physical constraint.
class bhkConstraint(bhkSerializable):
    entities: list[int]                                 # The entities affected by this constraint.
    priority: int                                       # Usually 1. Higher values indicate higher priority of this constraint?

    def __init__(self, r: Reader, h: Header):
        self.entities = r.readL32FArray(X[bhkEntity].ptr)
        self.priority = r.readUInt32()

# Hinge constraint.
class bhkLimitedHingeConstraint(bhkConstraint):
    limitedHinge: LimitedHingeDescriptor                # Describes a limited hinge constraint

    def __init__(self, r: Reader, h: Header):
        self.limitedHinge = LimitedHingeDescriptor(r, h)

# A malleable constraint.
class bhkMalleableConstraint(bhkConstraint):
    malleable: MalleableDescriptor                      # Constraint within constraint.

    def __init__(self, r: Reader, h: Header):
        self.malleable = MalleableDescriptor(r, h)

# A spring constraint.
class bhkStiffSpringConstraint(bhkConstraint):
    stiffSpring: StiffSpringDescriptor                  # Stiff Spring constraint.

    def __init__(self, r: Reader, h: Header):
        self.stiffSpring = StiffSpringDescriptor(r)

# Ragdoll constraint.
class bhkRagdollConstraint(bhkConstraint):
    ragdoll: RagdollDescriptor                          # Ragdoll constraint.

    def __init__(self, r: Reader, h: Header):
        self.ragdoll = RagdollDescriptor(r, h)

# A prismatic constraint.
class bhkPrismaticConstraint(bhkConstraint):
    prismatic: PrismaticDescriptor                      # Describes a prismatic constraint

    def __init__(self, r: Reader, h: Header):
        self.prismatic = PrismaticDescriptor(r, h)

# A hinge constraint.
class bhkHingeConstraint(bhkConstraint):
    hinge: HingeDescriptor                              # Hinge constraing.

    def __init__(self, r: Reader, h: Header):
        self.hinge = HingeDescriptor(r, h)

# A Ball and Socket Constraint.
class bhkBallAndSocketConstraint(bhkConstraint):
    ballandSocket: BallAndSocketDescriptor              # Describes a ball and socket constraint

    def __init__(self, r: Reader, h: Header):
        self.ballandSocket = BallAndSocketDescriptor(r)

# Two Vector4 for pivot in A and B.
class ConstraintInfo:
    def __init__(self, r: Reader):
        self.pivotInA: Vector4 = r.readVector4()
        self.pivotInB: Vector4 = r.readVector4()

# A Ball and Socket Constraint chain.
class bhkBallSocketConstraintChain(bhkSerializable):
    numPivots: int                                      # Number of pivot points. Divide by 2 to get the number of constraints.
    pivots: list[ConstraintInfo]                        # Two pivot points A and B for each constraint.
    tau: float                                          # High values are harder and more reactive, lower values are smoother.
    damping: float                                      # Defines damping strength for the current velocity.
    constraintForceMixing: float                        # Restitution (amount of elasticity) of constraints. Added to the diagonal of the constraint matrix. A value of 0.0 can result in a division by zero with some chain configurations.
    maxErrorDistance: float                             # Maximum distance error in constraints allowed before stabilization algorithm kicks in. A smaller distance causes more resistance.
    entitiesA: list[int]
    numEntities: int                                    # Hardcoded to 2. Don't change.
    entityA: int
    entityB: int
    priority: int

    def __init__(self, r: Reader, h: Header):
        self.numPivots = r.readUInt32()
        self.pivots = r.readFArray(lambda r: ConstraintInfo(r), NumPivots/2)
        self.tau = r.readSingle()
        self.damping = r.readSingle()
        self.constraintForceMixing = r.readSingle()
        self.maxErrorDistance = r.readSingle()
        self.entitiesA = r.readL32FArray(X[bhkRigidBody].ptr)
        self.numEntities = r.readUInt32()
        self.entityA = X[bhkRigidBody].ptr(r)
        self.entityB = X[bhkRigidBody].ptr(r)
        self.priority = r.readUInt32()

# A Havok Shape?
class bhkShape(bhkSerializable):

    def __init__(self, r: Reader, h: Header):

# Transforms a shape.
class bhkTransformShape(bhkShape):
    shape: int                                          # The shape that this object transforms.
    material: HavokMaterial                             # The material of the shape.
    radius: float
    unused: list[int]                                   # Garbage data from memory.
    transform: Matrix4x4                                # A transform matrix.

    def __init__(self, r: Reader, h: Header):
        self.shape = X[bhkShape].ref(r)
        self.material = HavokMaterial(r, h)
        self.radius = r.readSingle()
        self.unused = r.readBytes(8)
        self.transform = r.readMatrix4x4()

# A havok shape, perhaps with a bounding sphere for quick rejection in addition to more detailed shape data?
class bhkSphereRepShape(bhkShape):
    material: HavokMaterial                             # The material of the shape.
    radius: float                                       # The radius of the sphere that encloses the shape.

    def __init__(self, r: Reader, h: Header):
        self.material = HavokMaterial(r, h)
        self.radius = r.readSingle()

# A havok shape.
class bhkConvexShape(bhkSphereRepShape):

    def __init__(self, r: Reader, h: Header):

# A sphere.
class bhkSphereShape(bhkConvexShape):

    def __init__(self, r: Reader, h: Header):

# A capsule.
class bhkCapsuleShape(bhkConvexShape):
    unused: list[int]                                   # Not used. The following wants to be aligned at 16 bytes.
    firstPoint: Vector3                                 # First point on the capsule's axis.
    radius1: float                                      # Matches first capsule radius.
    secondPoint: Vector3                                # Second point on the capsule's axis.
    radius2: float                                      # Matches second capsule radius.

    def __init__(self, r: Reader, h: Header):
        self.unused = r.readBytes(8)
        self.firstPoint = r.readVector3()
        self.radius1 = r.readSingle()
        self.secondPoint = r.readVector3()
        self.radius2 = r.readSingle()

# A box.
class bhkBoxShape(bhkConvexShape):
    unused: list[int]                                   # Not used. The following wants to be aligned at 16 bytes.
    dimensions: Vector3                                 # A cube stored in Half Extents. A unit cube (1.0, 1.0, 1.0) would be stored as 0.5, 0.5, 0.5.
    unusedFloat: float                                  # Unused as Havok stores the Half Extents as hkVector4 with the W component unused.

    def __init__(self, r: Reader, h: Header):
        self.unused = r.readBytes(8)
        self.dimensions = r.readVector3()
        self.unusedFloat = r.readSingle()

# A convex shape built from vertices. Note that if the shape is used in
# a non-static object (such as clutter), then they will simply fall
# through ground when they are under a bhkListShape.
class bhkConvexVerticesShape(bhkConvexShape):
    verticesProperty: hkWorldObjCinfoProperty
    normalsProperty: hkWorldObjCinfoProperty
    vertices: list[Vector4]                             # Vertices. Fourth component is 0. Lexicographically sorted.
    normals: list[Vector4]                              # Half spaces as determined by the set of vertices above. First three components define the normal pointing to the exterior, fourth component is the signed distance of the separating plane to the origin: it is minus the dot product of v and n, where v is any vertex on the separating plane, and n is the normal. Lexicographically sorted.

    def __init__(self, r: Reader, h: Header):
        self.verticesProperty = hkWorldObjCinfoProperty(r)
        self.normalsProperty = hkWorldObjCinfoProperty(r)
        self.vertices = r.readL32FArray(lambda r: r.readL32Vector4())
        self.normals = r.readL32FArray(lambda r: r.readL32Vector4())

# A convex transformed shape?
class bhkConvexTransformShape(bhkTransformShape):

    def __init__(self, r: Reader, h: Header):

class bhkConvexSweepShape(bhkShape):
    shape: int
    material: HavokMaterial
    radius: float
    unknown: Vector3

    def __init__(self, r: Reader, h: Header):
        self.shape = X[bhkShape].ref(r)
        self.material = HavokMaterial(r, h)
        self.radius = r.readSingle()
        self.unknown = r.readVector3()

# Unknown.
class bhkMultiSphereShape(bhkSphereRepShape):
    unknownFloat1: float                                # Unknown.
    unknownFloat2: float                                # Unknown.
    spheres: list[NiBound]                              # This array holds the spheres which make up the multi sphere shape.

    def __init__(self, r: Reader, h: Header):
        self.unknownFloat1 = r.readSingle()
        self.unknownFloat2 = r.readSingle()
        self.spheres = r.readL32FArray(lambda r: NiBound(r))

# A tree-like Havok data structure stored in an assembly-like binary code?
class bhkBvTreeShape(bhkShape):

    def __init__(self, r: Reader, h: Header):

# Memory optimized partial polytope bounding volume tree shape (not an entity).
class bhkMoppBvTreeShape(bhkBvTreeShape):
    shape: int                                          # The shape.
    unused: list[int]                                   # Garbage data from memory. Referred to as User Data, Shape Collection, and Code.
    shapeScale: float                                   # Scale.
    moppDataSize: int                                   # Number of bytes for MOPP data.
    origin: Vector3                                     # Origin of the object in mopp coordinates. This is the minimum of all vertices in the packed shape along each axis, minus 0.1.
    scale: float                                        # The scaling factor to quantize the MOPP: the quantization factor is equal to 256*256 divided by this number. In Oblivion files, scale is taken equal to 256*256*254 / (size + 0.2) where size is the largest dimension of the bounding box of the packed shape.
    buildType: MoppDataBuildType                        # Tells if MOPP Data was organized into smaller chunks (PS3) or not (PC)
    moppData: list[int]                                 # The tree of bounding volume data.

    def __init__(self, r: Reader, h: Header):
        self.shape = X[bhkShape].ref(r)
        self.unused = r.readPArray(None, 'I', 3)
        self.shapeScale = r.readSingle()
        self.moppDataSize = r.readUInt32()
        self.origin = if h.v >= 0x0A000102: r.readVector3()
        self.scale = if h.v >= 0x0A000102: r.readSingle()
        self.buildType = MoppDataBuildType(r.readByte())
        self.moppData = r.readBytes(MOPPDataSize)

# Havok collision object that uses multiple shapes?
class bhkShapeCollection(bhkShape):

    def __init__(self, r: Reader, h: Header):

# A list of shapes.
# 
# Do not put a bhkPackedNiTriStripsShape in the Sub Shapes. Use a
# separate collision nodes without a list shape for those.
# 
# Also, shapes collected in a bhkListShape may not have the correct
# walking noise, so only use it for non-walkable objects.
class bhkListShape(bhkShapeCollection):
    subShapes: list[int]                                # List of shapes.
    material: HavokMaterial                             # The material of the shape.
    childShapeProperty: hkWorldObjCinfoProperty
    childFilterProperty: hkWorldObjCinfoProperty
    unknownInts: list[int]                              # Unknown.

    def __init__(self, r: Reader, h: Header):
        self.subShapes = r.readL32FArray(X[bhkShape].ref)
        self.material = HavokMaterial(r, h)
        self.childShapeProperty = hkWorldObjCinfoProperty(r)
        self.childFilterProperty = hkWorldObjCinfoProperty(r)
        self.unknownInts = r.readL32PArray(None, 'I')

class bhkMeshShape(bhkShape):
    unknowns: list[int]
    radius: float
    unused2: list[int]
    scale: Vector4
    shapeProperties: list[hkWorldObjCinfoProperty]
    unknown2: list[int]
    stripsData: list[int]                               # Refers to a bunch of NiTriStripsData objects that make up this shape.

    def __init__(self, r: Reader, h: Header):
        self.unknowns = r.readPArray(None, 'I', 2)
        self.radius = r.readSingle()
        self.unused2 = r.readBytes(8)
        self.scale = r.readVector4()
        self.shapeProperties = r.readL32FArray(lambda r: hkWorldObjCinfoProperty(r))
        self.unknown2 = r.readPArray(None, 'i', 3)
        self.stripsData = if h.v <= 0x0A000100: r.readL32FArray(X[NiTriStripsData].ref)

# A shape constructed from strips data.
class bhkPackedNiTriStripsShape(bhkShapeCollection):
    subShapes: list[OblivionSubShape]
    userData: int
    unused1: int                                        # Looks like a memory pointer and may be garbage.
    radius: float
    unused2: int                                        # Looks like a memory pointer and may be garbage.
    scale: Vector4
    radiusCopy: float                                   # Same as radius
    scaleCopy: Vector4                                  # Same as scale.
    data: int

    def __init__(self, r: Reader, h: Header):
        self.subShapes = if h.v <= 0x14000005: r.readL16FArray(lambda r: OblivionSubShape(r))
        self.userData = r.readUInt32()
        self.unused1 = r.readUInt32()
        self.radius = r.readSingle()
        self.unused2 = r.readUInt32()
        self.scale = r.readVector4()
        self.radiusCopy = r.readSingle()
        self.scaleCopy = r.readVector4()
        self.data = X[hkPackedNiTriStripsData].ref(r)

# A shape constructed from a bunch of strips.
class bhkNiTriStripsShape(bhkShapeCollection):
    material: HavokMaterial                             # The material of the shape.
    radius: float
    unused: list[int]                                   # Garbage data from memory though the last 3 are referred to as maxSize, size, and eSize.
    growBy: int
    scale: Vector4                                      # Scale. Usually (1.0, 1.0, 1.0, 0.0).
    stripsData: list[int]                               # Refers to a bunch of NiTriStripsData objects that make up this shape.
    dataLayers: list[HavokFilter]                       # Havok Layers for each strip data.

    def __init__(self, r: Reader, h: Header):
        self.material = HavokMaterial(r, h)
        self.radius = r.readSingle()
        self.unused = r.readPArray(None, 'I', 5)
        self.growBy = r.readUInt32()
        self.scale = if h.v >= 0x0A010000: r.readVector4()
        self.stripsData = r.readL32FArray(X[NiTriStripsData].ref)
        self.dataLayers = r.readL32FArray(lambda r: HavokFilter(r, h))

# A generic extra data object.
class NiExtraData(NiObject):
    name: str                                           # Name of this object.
    nextExtraData: int                                  # Block number of the next extra data object.

    def __init__(self, r: Reader, h: Header):
        self.name = if !BSExtraData and h.v >= 0x0A000100: Y.string(r)
        self.nextExtraData = if h.v <= 0x04020200: X[NiExtraData].ref(r)

# Abstract base class for all interpolators of bool, float, NiQuaternion, NiPoint3, NiColorA, and NiQuatTransform data.
class NiInterpolator(NiObject):

    def __init__(self, r: Reader, h: Header):

# Abstract base class for interpolators that use NiAnimationKeys (Key, KeyGrp) for interpolation.
class NiKeyBasedInterpolator(NiInterpolator):

    def __init__(self, r: Reader, h: Header):

# Uses NiFloatKeys to animate a float value over time.
class NiFloatInterpolator(NiKeyBasedInterpolator):
    value: float                                        # Pose value if lacking NiFloatData.
    data: int

    def __init__(self, r: Reader, h: Header):
        self.value = r.readSingle()
        self.data = X[NiFloatData].ref(r)

# An interpolator for transform keyframes.
class NiTransformInterpolator(NiKeyBasedInterpolator):
    transform: NiQuatTransform
    data: int

    def __init__(self, r: Reader, h: Header):
        self.transform = NiQuatTransform(r, h)
        self.data = X[NiTransformData].ref(r)

# Uses NiPosKeys to animate an NiPoint3 value over time.
class NiPoint3Interpolator(NiKeyBasedInterpolator):
    value: Vector3                                      # Pose value if lacking NiPosData.
    data: int

    def __init__(self, r: Reader, h: Header):
        self.value = r.readVector3()
        self.data = X[NiPosData].ref(r)

class PathFlags(Flag):
    CVDataNeedsUpdate = 0,
    CurveTypeOpen = 1 << 1,
    AllowFlip = 1 << 2,
    Bank = 1 << 3,
    ConstantVelocity = 1 << 4,
    Follow = 1 << 5,
    Flip = 1 << 6

# Used to make an object follow a predefined spline path.
class NiPathInterpolator(NiKeyBasedInterpolator):
    flags: PathFlags
    bankDir: int                                        # -1 = Negative, 1 = Positive
    maxBankAngle: float                                 # Max angle in radians.
    smoothing: float
    followAxis: int                                     # 0, 1, or 2 representing X, Y, or Z.
    pathData: int
    percentData: int

    def __init__(self, r: Reader, h: Header):
        self.flags = PathFlags(r.readUInt16())
        self.bankDir = r.readInt32()
        self.maxBankAngle = r.readSingle()
        self.smoothing = r.readSingle()
        self.followAxis = r.readInt16()
        self.pathData = X[NiPosData].ref(r)
        self.percentData = X[NiFloatData].ref(r)

# Uses NiBoolKeys to animate a bool value over time.
class NiBoolInterpolator(NiKeyBasedInterpolator):
    value: bool                                         # Pose value if lacking NiBoolData.
    data: int

    def __init__(self, r: Reader, h: Header):
        self.value = r.readBool32()
        self.data = X[NiBoolData].ref(r)

# Uses NiBoolKeys to animate a bool value over time.
# Unlike NiBoolInterpolator, it ensures that keys have not been missed between two updates.
class NiBoolTimelineInterpolator(NiBoolInterpolator):

    def __init__(self, r: Reader, h: Header):

class InterpBlendFlags(Enum):
    MANAGER_CONTROLLED = 1          # MANAGER_CONTROLLED

# Interpolator item for array in NiBlendInterpolator.
class InterpBlendItem:
    def __init__(self, r: Reader, h: Header):
        self.interpolator: int = X[NiInterpolator].ref(r) # Reference to an interpolator.
        self.weight: float = r.readSingle()
        self.normalizedWeight: float = r.readSingle()
        self.priority: int = r.readByte() if h.v >= 0x0A01006E else None
        self.easeSpinner: float = r.readSingle()

# Abstract base class for all NiInterpolators that blend the results of sub-interpolators together to compute a final weighted value.
class NiBlendInterpolator(NiInterpolator):
    flags: InterpBlendFlags
    arraySize: int
    arrayGrowBy: int
    weightThreshold: float
    interpCount: int
    singleIndex: int
    highPriority: int
    nextHighPriority: int
    singleTime: float
    highWeightsSum: float
    nextHighWeightsSum: float
    highEaseSpinner: float
    interpArrayItems: list[InterpBlendItem]
    managedControlled: bool
    onlyUseHighestWeight: bool
    singleInterpolator: int

    def __init__(self, r: Reader, h: Header):
        self.flags = if h.v >= 0x0A010070: InterpBlendFlags(r.readByte())
        self.arraySize = if h.v <= 0x0A01006D: r.readUInt16()
        self.arrayGrowBy = if h.v <= 0x0A01006D: r.readUInt16()
        self.arraySize = if h.v >= 0x0A01006E: r.readByte()
        self.weightThreshold = if h.v >= 0x0A010070: r.readSingle()
        self.interpCount = if (Flags & 1) == 0 and h.v >= 0x0A010070: r.readByte()
        self.singleIndex = if (Flags & 1) == 0 and h.v >= 0x0A010070: r.readByte()
        self.highPriority = if (Flags & 1) == 0 and h.v >= 0x0A010070: r.readSByte()
        self.nextHighPriority = if (Flags & 1) == 0 and h.v >= 0x0A010070: r.readSByte()
        self.singleTime = if (Flags & 1) == 0 and h.v >= 0x0A010070: r.readSingle()
        self.highWeightsSum = if (Flags & 1) == 0 and h.v >= 0x0A010070: r.readSingle()
        self.nextHighWeightsSum = if (Flags & 1) == 0 and h.v >= 0x0A010070: r.readSingle()
        self.highEaseSpinner = if (Flags & 1) == 0 and h.v >= 0x0A010070: r.readSingle()
        self.interpArrayItems = if (Flags & 1) == 0 and h.v >= 0x0A010070: r.readFArray(lambda r: InterpBlendItem(r, h), ArraySize)
        self.interpArrayItems = if h.v <= 0x0A01006F: r.readFArray(lambda r: InterpBlendItem(r, h), ArraySize)
        self.managedControlled = if h.v <= 0x0A01006F: r.readBool32()
        self.weightThreshold = if h.v <= 0x0A01006F: r.readSingle()
        self.onlyUseHighestWeight = if h.v <= 0x0A01006F: r.readBool32()
        self.interpCount = if h.v <= 0x0A01006D: r.readUInt16()
        self.singleIndex = if h.v <= 0x0A01006D: r.readUInt16()
        self.interpCount = if h.v >= 0x0A01006E and h.v <= 0x0A01006F: r.readByte()
        self.singleIndex = if h.v >= 0x0A01006E and h.v <= 0x0A01006F: r.readByte()
        self.singleInterpolator = if h.v >= 0x0A01006C and h.v <= 0x0A01006F: X[NiInterpolator].ref(r)
        self.singleTime = if h.v >= 0x0A01006C and h.v <= 0x0A01006F: r.readSingle()
        self.highPriority = if h.v <= 0x0A01006D: r.readInt32()
        self.nextHighPriority = if h.v <= 0x0A01006D: r.readInt32()
        self.highPriority = if h.v >= 0x0A01006E and h.v <= 0x0A01006F: r.readByte()
        self.nextHighPriority = if h.v >= 0x0A01006E and h.v <= 0x0A01006F: r.readByte()

# Abstract base class for interpolators storing data via a B-spline.
class NiBSplineInterpolator(NiInterpolator):
    startTime: float                                    # Animation start time.
    stopTime: float                                     # Animation stop time.
    splineData: int
    basisData: int

    def __init__(self, r: Reader, h: Header):
        self.startTime = r.readSingle()
        self.stopTime = r.readSingle()
        self.splineData = X[NiBSplineData].ref(r)
        self.basisData = X[NiBSplineBasisData].ref(r)

# Abstract base class for NiObjects that support names, extra data, and time controllers.
class NiObjectNET(NiObject):
    skyrimShaderType: BSLightingShaderPropertyShaderType# Configures the main shader path
    name: str                                           # Name of this controllable object, used to refer to the object in .kf files.
    hasOldExtraData: bool                               # Extra data for pre-3.0 versions.
    oldExtraPropName: str                               # (=NiStringExtraData)
    oldExtraInternalId: int                             # ref
    oldExtraString: str                                 # Extra string data.
    unknownByte: int                                    # Always 0.
    extraData: int                                      # Extra data object index. (The first in a chain)
    extraDataList: list[int]                            # List of extra data indices.
    controller: int                                     # Controller object index. (The first in a chain)

    def __init__(self, r: Reader, h: Header):
        self.skyrimShaderType = if BSLightingShaderProperty: BSLightingShaderPropertyShaderType(r.readUInt32())
        self.name = Y.string(r)
        self.hasOldExtraData = if h.v <= 0x02030000: r.readBool32()
        self.oldExtraPropName = if Has Old Extra Data and h.v <= 0x02030000: Y.string(r)
        self.oldExtraInternalId = if Has Old Extra Data and h.v <= 0x02030000: r.readUInt32()
        self.oldExtraString = if Has Old Extra Data and h.v <= 0x02030000: Y.string(r)
        self.unknownByte = if h.v <= 0x02030000: r.readByte()
        self.extraData = if h.v >= 0x03000000 and h.v <= 0x04020200: X[NiExtraData].ref(r)
        self.extraDataList = if h.v >= 0x0A000100: r.readL32FArray(X[NiExtraData].ref)
        self.controller = if h.v >= 0x03000000: X[NiTimeController].ref(r)

# This is the most common collision object found in NIF files. It acts as a real object that
# is visible and possibly (if the body allows for it) interactive. The node itself
# is simple, it only has three properties.
# For this type of collision object, bhkRigidBody or bhkRigidBodyT is generally used.
class NiCollisionObject(NiObject):
    target: int                                         # Index of the AV object referring to this collision object.

    def __init__(self, r: Reader, h: Header):
        self.target = X[NiAVObject].ptr(r)

# Collision box.
class NiCollisionData(NiCollisionObject):
    propagationMode: PropagationMode
    collisionMode: CollisionMode
    useAbv: int                                         # Use Alternate Bounding Volume.
    boundingVolume: BoundingVolume

    def __init__(self, r: Reader, h: Header):
        self.propagationMode = PropagationMode(r.readUInt32())
        self.collisionMode = if h.v >= 0x0A010000: CollisionMode(r.readUInt32())
        self.useAbv = r.readByte()
        self.boundingVolume = if Use ABh.v == 1: BoundingVolume(r, h)

# bhkNiCollisionObject flags. The flags 0x2, 0x100, and 0x200 are not seen in any NIF nor get/set by the engine.
class bhkCOFlags(Flag):
    ACTIVE = 0,
    NOTIFY = 1 << 2,
    SET_LOCAL = 1 << 3,
    DBG_DISPLAY = 1 << 4,
    USE_VEL = 1 << 5,
    RESET = 1 << 6,
    SYNC_ON_UPDATE = 1 << 7,
    ANIM_TARGETED = 1 << 10,
    DISMEMBERED_LIMB = 1 << 11

# Havok related collision object?
class bhkNiCollisionObject(NiCollisionObject):
    flags: bhkCOFlags                                   # Set to 1 for most objects, and to 41 for animated objects (ANIM_STATIC). Bits: 0=Active 2=Notify 3=Set Local 6=Reset.
    body: int

    def __init__(self, r: Reader, h: Header):
        self.flags = bhkCOFlags(r.readUInt16())
        self.body = X[bhkWorldObject].ref(r)

# Havok related collision object?
class bhkCollisionObject(bhkNiCollisionObject):

    def __init__(self, r: Reader, h: Header):

# Unknown.
class bhkBlendCollisionObject(bhkCollisionObject):
    heirGain: float
    velGain: float
    unkFloat1: float
    unkFloat2: float

    def __init__(self, r: Reader, h: Header):
        self.heirGain = r.readSingle()
        self.velGain = r.readSingle()
        self.unkFloat1 = r.readSingle()
        self.unkFloat2 = r.readSingle()

# Unknown.
class bhkPCollisionObject(bhkNiCollisionObject):

    def __init__(self, r: Reader, h: Header):

# Unknown.
class bhkSPCollisionObject(bhkPCollisionObject):

    def __init__(self, r: Reader, h: Header):

# Abstract audio-visual base class from which all of Gamebryo's scene graph objects inherit.
class NiAVObject(NiObjectNET):
    flags: Flags                                        # Basic flags for AV objects; commonly 0x000C or 0x000A.
    translation: Vector3                                # The translation vector.
    rotation: Matrix3x3                                 # The rotation part of the transformation matrix.
    scale: float                                        # Scaling part (only uniform scaling is supported).
    velocity: Vector3                                   # Unknown function. Always seems to be (0, 0, 0)
    properties: list[int]                               # All rendering properties attached to this object.
    unknown1: list[int]                                 # Always 2,0,2,0.
    unknown2: int                                       # 0 or 1.
    hasBoundingVolume: bool
    boundingVolume: BoundingVolume
    collisionObject: int

    def __init__(self, r: Reader, h: Header):
        self.flags = r.readUInt32()
        self.flags = if h.v >= 0x03000000 and : Flags(r.readUInt16())
        self.translation = r.readVector3()
        self.rotation = r.readMatrix3x3()
        self.scale = r.readSingle()
        self.velocity = if h.v <= 0x04020200: r.readVector3()
        self.properties = r.readL32FArray(X[NiProperty].ref)
        self.unknown1 = if h.v <= 0x02030000: r.readPArray(None, 'I', 4)
        self.unknown2 = if h.v <= 0x02030000: r.readByte()
        self.hasBoundingVolume = if h.v >= 0x03000000 and h.v <= 0x04020200: r.readBool32()
        self.boundingVolume = if Has Bounding Volume and h.v >= 0x03000000 and h.v <= 0x04020200: BoundingVolume(r, h)
        self.collisionObject = if h.v >= 0x0A000100: X[NiCollisionObject].ref(r)

# Abstract base class for dynamic effects such as NiLights or projected texture effects.
class NiDynamicEffect(NiAVObject):
    switchState: bool                                   # If true, then the dynamic effect is applied to affected nodes during rendering.
    numAffectedNodes: int
    affectedNodes: list[int]                            # If a node appears in this list, then its entire subtree will be affected by the effect.
    affectedNodePointers: list[int]                     # As of 4.0 the pointer hash is no longer stored alongside each NiObject on disk, yet this node list still refers to the pointer hashes. Cannot leave the type as Ptr because the link will be invalid.

    def __init__(self, r: Reader, h: Header):
        self.switchState = if h.v >= 0x0A01006A and : r.readBool32()
        self.numAffectedNodes = if h.v <= 0x04000002: r.readUInt32()
        self.affectedNodes = if h.v <= 0x0303000D: r.readFArray(X[NiNode].ptr, NumAffectedNodes)
        self.affectedNodePointers = if h.v >= 0x04000000 and h.v <= 0x04000002: r.readPArray(None, 'I', NumAffectedNodes)
        self.numAffectedNodes = if h.v >= 0x0A010000 and : r.readUInt32()
        self.affectedNodes = if h.v >= 0x0A010000 and : r.readFArray(X[NiNode].ptr, NumAffectedNodes)

# Abstract base class that represents light sources in a scene graph.
# For Bethesda Stream 130 (FO4), NiLight now directly inherits from NiAVObject.
class NiLight(NiDynamicEffect):
    dimmer: float                                       # Scales the overall brightness of all light components.
    ambientColor: Color3
    diffuseColor: Color3
    specularColor: Color3

    def __init__(self, r: Reader, h: Header):
        self.dimmer = r.readSingle()
        self.ambientColor = Color3(r)
        self.diffuseColor = Color3(r)
        self.specularColor = Color3(r)

# Abstract base class representing all rendering properties. Subclasses are attached to NiAVObjects to control their rendering.
class NiProperty(NiObjectNET):

    def __init__(self, r: Reader, h: Header):

# Unknown
class NiTransparentProperty(NiProperty):
    unknown: list[int]                                  # Unknown.

    def __init__(self, r: Reader, h: Header):
        self.unknown = r.readBytes(6)

# Abstract base class for all particle system modifiers.
class NiPSysModifier(NiObject):
    name: str                                           # Used to locate the modifier.
    order: int                                          # Modifier ID in the particle modifier chain (always a multiple of 1000)?
    target: int                                         # NiParticleSystem parent of this modifier.
    active: bool                                        # Whether or not the modifier is active.

    def __init__(self, r: Reader, h: Header):
        self.name = Y.string(r)
        self.order = r.readUInt32()
        self.target = X[NiParticleSystem].ptr(r)
        self.active = r.readBool32()

# Abstract base class for all particle system emitters.
class NiPSysEmitter(NiPSysModifier):
    speed: float                                        # Speed / Inertia of particle movement.
    speedVariation: float                               # Adds an amount of randomness to Speed.
    declination: float                                  # Declination / First axis.
    declinationVariation: float                         # Declination randomness / First axis.
    planarAngle: float                                  # Planar Angle / Second axis.
    planarAngleVariation: float                         # Planar Angle randomness / Second axis .
    initialColor: Color4                                # Defines color of a birthed particle.
    initialRadius: float                                # Size of a birthed particle.
    radiusVariation: float                              # Particle Radius randomness.
    lifeSpan: float                                     # Duration until a particle dies.
    lifeSpanVariation: float                            # Adds randomness to Life Span.

    def __init__(self, r: Reader, h: Header):
        self.speed = r.readSingle()
        self.speedVariation = r.readSingle()
        self.declination = r.readSingle()
        self.declinationVariation = r.readSingle()
        self.planarAngle = r.readSingle()
        self.planarAngleVariation = r.readSingle()
        self.initialColor = Color4(r)
        self.initialRadius = r.readSingle()
        self.radiusVariation = if h.v >= 0x0A040001: r.readSingle()
        self.lifeSpan = r.readSingle()
        self.lifeSpanVariation = r.readSingle()

# Abstract base class for particle emitters that emit particles from a volume.
class NiPSysVolumeEmitter(NiPSysEmitter):
    emitterObject: int                                  # Node parent of this modifier?

    def __init__(self, r: Reader, h: Header):
        self.emitterObject = if h.v >= 0x0A010000: X[NiNode].ptr(r)

# Abstract base class that provides the base timing and update functionality for all the Gamebryo animation controllers.
class NiTimeController(NiObject):
    nextController: int                                 # Index of the next controller.
    flags: Flags                                        # Controller flags.
                                                        #     Bit 0 : Anim type, 0=APP_TIME 1=APP_INIT
                                                        #     Bit 1-2 : Cycle type, 00=Loop 01=Reverse 10=Clamp
                                                        #     Bit 3 : Active
                                                        #     Bit 4 : Play backwards
                                                        #     Bit 5 : Is manager controlled
                                                        #     Bit 6 : Always seems to be set in Skyrim and Fallout NIFs, unknown function
    frequency: float                                    # Frequency (is usually 1.0).
    phase: float                                        # Phase (usually 0.0).
    startTime: float                                    # Controller start time.
    stopTime: float                                     # Controller stop time.
    target: int                                         # Controller target (object index of the first controllable ancestor of this object).
    unknownInteger: int                                 # Unknown integer.

    def __init__(self, r: Reader, h: Header):
        self.nextController = X[NiTimeController].ref(r)
        self.flags = Flags(r.readUInt16())
        self.frequency = r.readSingle()
        self.phase = r.readSingle()
        self.startTime = r.readSingle()
        self.stopTime = r.readSingle()
        self.target = if h.v >= 0x0303000D: X[NiObjectNET].ptr(r)
        self.unknownInteger = if h.v <= 0x03010000: r.readUInt32()

# Abstract base class for all NiTimeController objects using NiInterpolator objects to animate their target objects.
class NiInterpController(NiTimeController):
    managerControlled: bool

    def __init__(self, r: Reader, h: Header):
        self.managerControlled = if h.v >= 0x0A010068 and h.v <= 0x0A01006C: r.readBool32()

# DEPRECATED (20.6)
class NiMultiTargetTransformController(NiInterpController):
    extraTargets: list[int]                             # NiNode Targets to be controlled.

    def __init__(self, r: Reader, h: Header):
        self.extraTargets = r.readL16FArray(X[NiAVObject].ptr)

# DEPRECATED (20.5), replaced by NiMorphMeshModifier.
# Time controller for geometry morphing.
class NiGeomMorpherController(NiInterpController):
    extraFlags: Flags                                   # 1 = UPDATE NORMALS
    data: int                                           # Geometry morphing data index.
    alwaysUpdate: int
    numInterpolators: int
    interpolators: list[int]
    interpolatorWeights: list[MorphWeight]
    unknownInts: list[int]                              # Unknown.

    def __init__(self, r: Reader, h: Header):
        self.extraFlags = if h.v >= 0x0A000102: Flags(r.readUInt16())
        self.data = X[NiMorphData].ref(r)
        self.alwaysUpdate = if h.v >= 0x04000001: r.readByte()
        self.numInterpolators = if h.v >= 0x0A01006A: r.readUInt32()
        self.interpolators = if h.v >= 0x0A01006A and h.v <= 0x14000005: r.readFArray(X[NiInterpolator].ref, NumInterpolators)
        self.interpolatorWeights = if h.v >= 0x14010003: r.readFArray(lambda r: MorphWeight(r), NumInterpolators)
        self.unknownInts = if h.v >= 0x0A020000 and h.v <= 0x14000005 and : r.readL32PArray(None, 'I')

# Unknown! Used by Daoc->'healing.nif'.
class NiMorphController(NiInterpController):

    def __init__(self, r: Reader, h: Header):

# Unknown! Used by Daoc.
class NiMorpherController(NiInterpController):
    data: int                                           # This controller's data.

    def __init__(self, r: Reader, h: Header):
        self.data = X[NiMorphData].ref(r)

# Uses a single NiInterpolator to animate its target value.
class NiSingleInterpController(NiInterpController):
    interpolator: int

    def __init__(self, r: Reader, h: Header):
        self.interpolator = if h.v >= 0x0A010068: X[NiInterpolator].ref(r)

# DEPRECATED (10.2), RENAMED (10.2) to NiTransformController
# A time controller object for animation key frames.
class NiKeyframeController(NiSingleInterpController):
    data: int

    def __init__(self, r: Reader, h: Header):
        self.data = if h.v <= 0x0A010067: X[NiKeyframeData].ref(r)

# NiTransformController replaces NiKeyframeController.
class NiTransformController(NiKeyframeController):

    def __init__(self, r: Reader, h: Header):

# A particle system modifier controller.
# NiInterpController::GetCtlrID() string format:
#     '%s'
# Where %s = Value of "Modifier Name"
class NiPSysModifierCtlr(NiSingleInterpController):
    modifierName: str                                   # Used to find the modifier pointer.

    def __init__(self, r: Reader, h: Header):
        self.modifierName = Y.string(r)

# Particle system emitter controller.
# NiInterpController::GetInterpolatorID() string format:
#     ['BirthRate', 'EmitterActive'] (for "Interpolator" and "Visibility Interpolator" respectively)
class NiPSysEmitterCtlr(NiPSysModifierCtlr):
    visibilityInterpolator: int
    data: int

    def __init__(self, r: Reader, h: Header):
        self.visibilityInterpolator = if h.v >= 0x0A010000: X[NiInterpolator].ref(r)
        self.data = if h.v <= 0x0A010067: X[NiPSysEmitterCtlrData].ref(r)

# A particle system modifier controller that animates a boolean value for particles.
class NiPSysModifierBoolCtlr(NiPSysModifierCtlr):

    def __init__(self, r: Reader, h: Header):

# A particle system modifier controller that animates active/inactive state for particles.
class NiPSysModifierActiveCtlr(NiPSysModifierBoolCtlr):
    data: int

    def __init__(self, r: Reader, h: Header):
        self.data = if h.v <= 0x0A010067: X[NiVisData].ref(r)

# A particle system modifier controller that animates a floating point value for particles.
class NiPSysModifierFloatCtlr(NiPSysModifierCtlr):
    data: int

    def __init__(self, r: Reader, h: Header):
        self.data = if h.v <= 0x0A010067: X[NiFloatData].ref(r)

# Animates the declination value on an NiPSysEmitter object.
class NiPSysEmitterDeclinationCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the declination variation value on an NiPSysEmitter object.
class NiPSysEmitterDeclinationVarCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the size value on an NiPSysEmitter object.
class NiPSysEmitterInitialRadiusCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the lifespan value on an NiPSysEmitter object.
class NiPSysEmitterLifeSpanCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the speed value on an NiPSysEmitter object.
class NiPSysEmitterSpeedCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the strength value of an NiPSysGravityModifier.
class NiPSysGravityStrengthCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target float value.
class NiFloatInterpController(NiSingleInterpController):

    def __init__(self, r: Reader, h: Header):

# Changes the image a Map (TexDesc) will use. Uses a float interpolator to animate the texture index.
# Often used for performing flipbook animation.
class NiFlipController(NiFloatInterpController):
    textureSlot: TexType                                # Target texture slot (0=base, 4=glow).
    startTime: float
    delta: float                                        # Time between two flips.
                                                        #     delta = (start_time - stop_time) / sources.num_indices
    numSources: int
    sources: list[int]                                  # The texture sources.
    images: list[int]                                   # The image sources

    def __init__(self, r: Reader, h: Header):
        self.textureSlot = TexType(r.readUInt32())
        self.startTime = if h.v >= 0x0303000D and h.v <= 0x0A010067: r.readSingle()
        self.delta = if h.v <= 0x0A010067: r.readSingle()
        self.numSources = r.readUInt32()
        self.sources = if h.v >= 0x04000000: r.readFArray(X[NiSourceTexture].ref, NumSources)
        self.images = if h.v <= 0x03010000: r.readFArray(X[NiImage].ref, NumSources)

# Animates the alpha value of a property using an interpolator.
class NiAlphaController(NiFloatInterpController):
    data: int

    def __init__(self, r: Reader, h: Header):
        self.data = if h.v <= 0x0A010067: X[NiFloatData].ref(r)

# Used to animate a single member of an NiTextureTransform.
# NiInterpController::GetCtlrID() string formats:
#     ['%1-%2-TT_TRANSLATE_U', '%1-%2-TT_TRANSLATE_V', '%1-%2-TT_ROTATE', '%1-%2-TT_SCALE_U', '%1-%2-TT_SCALE_V']
# (Depending on "Operation" enumeration, %1 = Value of "Shader Map", %2 = Value of "Texture Slot")
class NiTextureTransformController(NiFloatInterpController):
    shaderMap: bool                                     # Is the target map a shader map?
    textureSlot: TexType                                # The target texture slot.
    operation: TransformMember                          # Controls which aspect of the texture transform to modify.
    data: int

    def __init__(self, r: Reader, h: Header):
        self.shaderMap = r.readBool32()
        self.textureSlot = TexType(r.readUInt32())
        self.operation = TransformMember(r.readUInt32())
        self.data = if h.v <= 0x0A010067: X[NiFloatData].ref(r)

# Unknown controller.
class NiLightDimmerController(NiFloatInterpController):

    def __init__(self, r: Reader, h: Header):

# Abstract base class for all NiInterpControllers that use a NiInterpolator to animate their target boolean value.
class NiBoolInterpController(NiSingleInterpController):

    def __init__(self, r: Reader, h: Header):

# Animates the visibility of an NiAVObject.
class NiVisController(NiBoolInterpController):
    data: int

    def __init__(self, r: Reader, h: Header):
        self.data = if h.v <= 0x0A010067: X[NiVisData].ref(r)

# Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target NiPoint3 value.
class NiPoint3InterpController(NiSingleInterpController):

    def __init__(self, r: Reader, h: Header):

# Time controller for material color. Flags are used for color selection in versions below 10.1.0.0.
# Bits 4-5: Target Color (00 = Ambient, 01 = Diffuse, 10 = Specular, 11 = Emissive)
# NiInterpController::GetCtlrID() string formats:
#     ['AMB', 'DIFF', 'SPEC', 'SELF_ILLUM'] (Depending on "Target Color")
class NiMaterialColorController(NiPoint3InterpController):
    targetColor: MaterialColor                          # Selects which color to control.
    data: int

    def __init__(self, r: Reader, h: Header):
        self.targetColor = if h.v >= 0x0A010000: MaterialColor(r.readUInt16())
        self.data = if h.v <= 0x0A010067: X[NiPosData].ref(r)

# Animates the ambient, diffuse and specular colors of an NiLight.
# NiInterpController::GetCtlrID() string formats:
#     ['Diffuse', 'Ambient'] (Depending on "Target Color")
class NiLightColorController(NiPoint3InterpController):
    targetColor: LightColor
    data: int

    def __init__(self, r: Reader, h: Header):
        self.targetColor = if h.v >= 0x0A010000: LightColor(r.readUInt16())
        self.data = if h.v <= 0x0A010067: X[NiPosData].ref(r)

# Abstract base class for all extra data controllers.
# NiInterpController::GetCtlrID() string format:
#     '%s'
# Where %s = Value of "Extra Data Name"
class NiExtraDataController(NiSingleInterpController):
    extraDataName: str

    def __init__(self, r: Reader, h: Header):
        self.extraDataName = if h.v >= 0x0A020000: Y.string(r)

# Animates an NiFloatExtraData object attached to an NiAVObject.
# NiInterpController::GetCtlrID() string format is same as parent.
class NiFloatExtraDataController(NiExtraDataController):
    numExtraBytes: int                                  # Number of extra bytes.
    unknownBytes: list[int]                             # Unknown.
    unknownExtraBytes: list[int]                        # Unknown.
    data: int

    def __init__(self, r: Reader, h: Header):
        self.numExtraBytes = if h.v <= 0x0A010000: r.readByte()
        self.unknownBytes = if h.v <= 0x0A010000: r.readBytes(7)
        self.unknownExtraBytes = if h.v <= 0x0A010000: r.readBytes(NumExtraBytes)
        self.data = if h.v <= 0x0A010067: X[NiFloatData].ref(r)

# Animates an NiFloatsExtraData object attached to an NiAVObject.
# NiInterpController::GetCtlrID() string format:
#     '%s[%d]'
# Where %s = Value of "Extra Data Name", %d = Value of "Floats Extra Data Index"
class NiFloatsExtraDataController(NiExtraDataController):
    floatsExtraDataIndex: int
    data: int

    def __init__(self, r: Reader, h: Header):
        self.floatsExtraDataIndex = r.readInt32()
        self.data = if h.v <= 0x0A010067: X[NiFloatData].ref(r)

# Animates an NiFloatsExtraData object attached to an NiAVObject.
# NiInterpController::GetCtlrID() string format:
#     '%s[%d]'
# Where %s = Value of "Extra Data Name", %d = Value of "Floats Extra Data Index"
class NiFloatsExtraDataPoint3Controller(NiExtraDataController):
    floatsExtraDataIndex: int

    def __init__(self, r: Reader, h: Header):
        self.floatsExtraDataIndex = r.readInt32()

# DEPRECATED (20.5), Replaced by NiSkinningLODController.
# Level of detail controller for bones.  Priority is arranged from low to high.
class NiBoneLODController(NiTimeController):
    lod: int                                            # Unknown.
    numLoDs: int                                        # Number of LODs.
    numNodeGroups: int                                  # Number of node arrays.
    nodeGroups: list[NodeSet]                           # A list of node sets (each set a sequence of bones).
    numShapeGroups: int                                 # Number of shape groups.
    shapeGroups1: list[SkinInfoSet]                     # List of shape groups.
    numShapeGroups2: int                                # The size of the second list of shape groups.
    shapeGroups2: list[int]                             # Group of NiTriShape indices.
    unknownInt2: int                                    # Unknown.
    unknownInt3: int                                    # Unknown.

    def __init__(self, r: Reader, h: Header):
        self.lod = r.readUInt32()
        self.numLoDs = r.readUInt32()
        self.numNodeGroups = r.readUInt32()
        self.nodeGroups = r.readFArray(lambda r: NodeSet(r), NumLODs)
        self.numShapeGroups = if h.v >= 0x04020200 and (Uh.v == 0): r.readUInt32()
        self.numShapeGroups = if h.v >= 0x0A020000 and h.v <= 0x0A020000 and (Uh.v == 1): r.readUInt32()
        self.shapeGroups1 = if h.v >= 0x04020200 and (Uh.v == 0): r.readFArray(lambda r: SkinInfoSet(r), NumShapeGroups)
        self.shapeGroups1 = if h.v >= 0x0A020000 and h.v <= 0x0A020000 and (Uh.v == 1): r.readFArray(lambda r: SkinInfoSet(r), NumShapeGroups)
        self.numShapeGroups2 = if h.v >= 0x04020200 and (Uh.v == 0): r.readUInt32()
        self.numShapeGroups2 = if h.v >= 0x0A020000 and h.v <= 0x0A020000 and (Uh.v == 1): r.readUInt32()
        self.shapeGroups2 = if h.v >= 0x04020200 and (Uh.v == 0): r.readFArray(X[NiTriBasedGeom].ref, NumShapeGroups2)
        self.shapeGroups2 = if h.v >= 0x0A020000 and h.v <= 0x0A020000 and (Uh.v == 1): r.readFArray(X[NiTriBasedGeom].ref, NumShapeGroups2)
        self.unknownInt2 = if h.v >= 0x14030009 and h.v <= 0x14030009 and : r.readInt32()
        self.unknownInt3 = if h.v >= 0x14030009 and h.v <= 0x14030009 and : r.readInt32()

# A simple LOD controller for bones.
class NiBSBoneLODController(NiBoneLODController):

    def __init__(self, r: Reader, h: Header):

class MaterialData:
    def __init__(self, r: Reader, h: Header):
        self.hasShader: bool = r.readBool32() if h.v >= 0x0A000100 and h.v <= 0x14010003 else None # Shader.
        self.shaderName: str = Y.string(r) if Has Shader and h.v >= 0x0A000100 and h.v <= 0x14010003 else None # The shader name.
        self.shaderExtraData: int = r.readInt32() if Has Shader and h.v >= 0x0A000100 and h.v <= 0x14010003 else None # Extra data associated with the shader. A value of -1 means the shader is the default implementation.
        self.numMaterials: int = r.readUInt32() if h.v >= 0x14020005 else None
        self.materialName: list[str] = r.readFArray(lambda r: Y.string(r), NumMaterials) if h.v >= 0x14020005 else None # The name of the material.
        self.materialExtraData: list[int] = r.readPArray(None, 'i', NumMaterials) if h.v >= 0x14020005 else None # Extra data associated with the material. A value of -1 means the material is the default implementation.
        self.activeMaterial: int = r.readInt32() if h.v >= 0x14020005 else None # The index of the currently active material.
        self.unknownByte: int = r.readByte() if h.v >= 0x0A020000 and h.v <= 0x0A020000 and (Uh.v == 1) else None # Cyanide extension (only in version 10.2.0.0?).
        self.unknownInteger2: int = r.readInt32() if h.v >= 0x0A040001 and h.v <= 0x0A040001 else None # Unknown.
        self.materialNeedsUpdate: bool = r.readBool32() if h.v >= 0x14020007 else None # Whether the materials for this object always needs to be updated before rendering with them.

# Describes a visible scene element with vertices like a mesh, a particle system, lines, etc.
class NiGeometry(NiAVObject):
    bound: NiBound
    skin: int
    data: int                                           # Data index (NiTriShapeData/NiTriStripData).
    skinInstance: int
    materialData: MaterialData
    shaderProperty: int
    alphaProperty: int

    def __init__(self, r: Reader, h: Header):
        self.bound = if NiParticleSystem: NiBound(r)
        self.skin = if NiParticleSystem: X[NiObject].ref(r)
        self.data = X[NiGeometryData].ref(r)
        self.data = if !NiParticleSystem: X[NiGeometryData].ref(r)
        self.skinInstance = if h.v >= 0x0303000D and : X[NiSkinInstance].ref(r)
        self.skinInstance = if !NiParticleSystem: X[NiSkinInstance].ref(r)
        self.materialData = if h.v >= 0x0A000100 and : MaterialData(r, h)
        self.materialData = if !NiParticleSystem and h.v >= 0x0A000100 and : MaterialData(r, h)
        self.shaderProperty = if h.v >= 0x14020007 and (Uh.v == 12): X[BSShaderProperty].ref(r)
        self.alphaProperty = if h.v >= 0x14020007 and (Uh.v == 12): X[NiAlphaProperty].ref(r)

# Describes a mesh, built from triangles.
class NiTriBasedGeom(NiGeometry):

    def __init__(self, r: Reader, h: Header):

class VectorFlags(Flag):
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

class BSVectorFlags(Flag):
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

# Mesh data: vertices, vertex normals, etc.
class NiGeometryData(NiObject):
    groupId: int                                        # Always zero.
    numVertices: int                                    # Number of vertices.
    bsMaxVertices: int                                  # Bethesda uses this for max number of particles in NiPSysData.
    keepFlags: int                                      # Used with NiCollision objects when OBB or TRI is set.
    compressFlags: int                                  # Unknown.
    hasVertices: bool                                   # Is the vertex array present? (Always non-zero.)
    vertices: list[Vector3]                             # The mesh vertices.
    vectorFlags: VectorFlags
    bsVectorFlags: BSVectorFlags
    materialCrc: int
    hasNormals: bool                                    # Do we have lighting normals? These are essential for proper lighting: if not present, the model will only be influenced by ambient light.
    normals: list[Vector3]                              # The lighting normals.
    tangents: list[Vector3]                             # Tangent vectors.
    bitangents: list[Vector3]                           # Bitangent vectors.
    hasUnkFloats: bool
    unkFloats: list[float]
    center: Vector3                                     # Center of the bounding box (smallest box that contains all vertices) of the mesh.
    radius: float                                       # Radius of the mesh: maximal Euclidean distance between the center and all vertices.
    unknown13shorts: list[int]                          # Unknown, always 0?
    hasVertexColors: bool                               # Do we have vertex colors? These are usually used to fine-tune the lighting of the model.
                                                        # 
                                                        #     Note: how vertex colors influence the model can be controlled by having a NiVertexColorProperty object as a property child of the root node. If this property object is not present, the vertex colors fine-tune lighting.
                                                        # 
                                                        #     Note 2: set to either 0 or 0xFFFFFFFF for NifTexture compatibility.
    vertexColors: list[ByteColor4]                      # The vertex colors.
    numUvSets: int                                      # The lower 6 (or less?) bits of this field represent the number of UV texture sets. The other bits are probably flag bits. For versions 10.1.0.0 and up, if bit 12 is set then extra vectors are present after the normals.
    hasUv: bool                                         # Do we have UV coordinates?
                                                        # 
                                                        #     Note: for compatibility with NifTexture, set this value to either 0x00000000 or 0xFFFFFFFF.
    uvSets: list[list[HalfTexCoord]]                    # The UV texture coordinates. They follow the OpenGL standard: some programs may require you to flip the second coordinate.
    consistencyFlags: ConsistencyType                   # Consistency Flags
    additionalData: int                                 # Unknown.

    def __init__(self, r: Reader, h: Header):
        self.groupId = if h.v >= 0x0A010072: r.readInt32()
        self.numVertices = if !NiPSysData: r.readUInt16()
        self.numVertices = if NiPSysData: r.readUInt16()
        self.bsMaxVertices = if NiPSysData: r.readUInt16()
        self.keepFlags = if h.v >= 0x0A010000: r.readByte()
        self.compressFlags = if h.v >= 0x0A010000: r.readByte()
        self.hasVertices = r.readBool32()
        self.vertices = if (Has Vertices > 0) and (Has Vertices != 15): r.readFArray(lambda r: r.readVector3(), NumVertices)
        self.vertices = if Has Vertices == 15 and h.v >= 0x14030101: r.readFArray(lambda r: Vector3(r.readHalf(), r.readHalf(), r.readHalf()), NumVertices)
        self.vectorFlags = if h.v >= 0x0A000100 and : VectorFlags(r.readUInt16())
        self.bsVectorFlags = BSVectorFlags(r.readUInt16())
        self.materialCrc = if h.v >= 0x14020007 and h.v <= 0x14020007 and (Uh.v == 12): r.readUInt32()
        self.hasNormals = r.readBool32()
        self.normals = if (Has Normals > 0) and (Has Normals != 6): r.readFArray(lambda r: r.readVector3(), NumVertices)
        self.normals = if Has Normals == 6 and h.v >= 0x14030101: r.readFArray(lambda r: Vector3(r.readByte(), r.readByte(), r.readByte()), NumVertices)
        self.tangents = if (Has Normals) and ((Vector Flags | BS Vector Flags) & 4096) and h.v >= 0x0A010000: r.readFArray(lambda r: r.readVector3(), NumVertices)
        self.bitangents = if (Has Normals) and ((Vector Flags | BS Vector Flags) & 4096) and h.v >= 0x0A010000: r.readFArray(lambda r: r.readVector3(), NumVertices)
        self.hasUnkFloats = if h.v >= 0x14030009 and h.v <= 0x14030009 and : r.readBool32()
        self.unkFloats = if Has Unk Floats and h.v >= 0x14030009 and h.v <= 0x14030009 and : r.readPArray(None, 'f', NumVertices)
        self.center = r.readVector3()
        self.radius = r.readSingle()
        self.unknown13shorts = if h.v >= 0x14030009 and h.v <= 0x14030009 and : r.readPArray(None, 'h', 13)
        self.hasVertexColors = r.readBool32()
        self.vertexColors = if (Has Vertex Colors > 0) and (Has Vertex Colors != 7): r.readFArray(lambda r: Color4(r), NumVertices)
        self.vertexColors = if Has Vertex Colors == 7 and h.v >= 0x14030101: r.readFArray(lambda r: Color4Byte(r), NumVertices)
        self.numUvSets = if h.v <= 0x04020200: r.readUInt16()
        self.hasUv = if h.v <= 0x04000002: r.readBool32()
        self.uvSets = if (Has Vertices > 0) and (Has Vertices != 15): r.readFArray(lambda k: r.readFArray(lambda r: TexCoord(r), ((NumUVSets&63)|(VectorFlags&63)|(BSVectorFlags&1))), NumVertices)
        self.uvSets = if Has Vertices == 15 and h.v >= 0x14030101: r.readFArray(lambda k: r.readFArray(lambda r: HalfTexCoord(r), ((NumUVSets&63)|(VectorFlags&63)|(BSVectorFlags&1))), NumVertices)
        self.consistencyFlags = if h.v >= 0x0A000100: ConsistencyType(r.readUInt16())
        self.additionalData = if h.v >= 0x14000004: X[AbstractAdditionalGeometryData].ref(r)

class AbstractAdditionalGeometryData(NiObject):

    def __init__(self, r: Reader, h: Header):

# Describes a mesh, built from triangles.
class NiTriBasedGeomData(NiGeometryData):
    numTriangles: int                                   # Number of triangles.

    def __init__(self, r: Reader, h: Header):
        self.numTriangles = r.readUInt16()

# Unknown. Is apparently only used in skeleton.nif files.
class bhkBlendController(NiTimeController):
    keys: int                                           # Seems to be always zero.

    def __init__(self, r: Reader, h: Header):
        self.keys = r.readUInt32()

# Bethesda-specific collision bounding box for skeletons.
class BSBound(NiExtraData):
    center: Vector3                                     # Center of the bounding box.
    dimensions: Vector3                                 # Dimensions of the bounding box from center.

    def __init__(self, r: Reader, h: Header):
        self.center = r.readVector3()
        self.dimensions = r.readVector3()

# Unknown. Marks furniture sitting positions?
class BSFurnitureMarker(NiExtraData):
    positions: list[FurniturePosition]

    def __init__(self, r: Reader, h: Header):
        self.positions = r.readL32FArray(lambda r: FurniturePosition(r))

# Particle modifier that adds a blend of object space translation and rotation to particles born in world space.
class BSParentVelocityModifier(NiPSysModifier):
    damping: float                                      # Amount of blending?

    def __init__(self, r: Reader, h: Header):
        self.damping = r.readSingle()

# Particle emitter that uses a node, its children and subchildren to emit from.  Emission will be evenly spread along points from nodes leading to their direct parents/children only.
class BSPSysArrayEmitter(NiPSysVolumeEmitter):

    def __init__(self, r: Reader, h: Header):

# Particle Modifier that uses the wind value from the gamedata to alter the path of particles.
class BSWindModifier(NiPSysModifier):
    strength: float                                     # The amount of force wind will have on particles.

    def __init__(self, r: Reader, h: Header):
        self.strength = r.readSingle()

# NiTriStripsData for havok data?
class hkPackedNiTriStripsData(bhkShapeCollection):
    triangles: list[TriangleData]
    numVertices: int
    unknownByte1: int                                   # Unknown.
    vertices: list[Vector3]
    subShapes: list[OblivionSubShape]                   # The subparts.

    def __init__(self, r: Reader, h: Header):
        self.triangles = r.readL32FArray(lambda r: TriangleData(r, h))
        self.numVertices = r.readUInt32()
        self.unknownByte1 = if h.v >= 0x14020007: r.readByte()
        self.vertices = r.readFArray(lambda r: r.readVector3(), NumVertices)
        self.subShapes = if h.v >= 0x14020007: r.readL16FArray(lambda r: OblivionSubShape(r))

# Transparency. Flags 0x00ED.
class NiAlphaProperty(NiProperty):
    flags: Flags                                        # Bit 0 : alpha blending enable
                                                        #     Bits 1-4 : source blend mode
                                                        #     Bits 5-8 : destination blend mode
                                                        #     Bit 9 : alpha test enable
                                                        #     Bit 10-12 : alpha test mode
                                                        #     Bit 13 : no sorter flag ( disables triangle sorting )
                                                        # 
                                                        #     blend modes (glBlendFunc):
                                                        #     0000 GL_ONE
                                                        #     0001 GL_ZERO
                                                        #     0010 GL_SRC_COLOR
                                                        #     0011 GL_ONE_MINUS_SRC_COLOR
                                                        #     0100 GL_DST_COLOR
                                                        #     0101 GL_ONE_MINUS_DST_COLOR
                                                        #     0110 GL_SRC_ALPHA
                                                        #     0111 GL_ONE_MINUS_SRC_ALPHA
                                                        #     1000 GL_DST_ALPHA
                                                        #     1001 GL_ONE_MINUS_DST_ALPHA
                                                        #     1010 GL_SRC_ALPHA_SATURATE
                                                        # 
                                                        #     test modes (glAlphaFunc):
                                                        #     000 GL_ALWAYS
                                                        #     001 GL_LESS
                                                        #     010 GL_EQUAL
                                                        #     011 GL_LEQUAL
                                                        #     100 GL_GREATER
                                                        #     101 GL_NOTEQUAL
                                                        #     110 GL_GEQUAL
                                                        #     111 GL_NEVER
    threshold: int                                      # Threshold for alpha testing (see: glAlphaFunc)
    unknownShort1: int                                  # Unknown
    unknownInt2: int                                    # Unknown

    def __init__(self, r: Reader, h: Header):
        self.flags = Flags(r.readUInt16())
        self.threshold = r.readByte()
        self.unknownShort1 = if h.v <= 0x02030000: r.readUInt16()
        self.unknownShort1 = if h.v >= 0x14030101 and h.v <= 0x14030102: r.readUInt16()
        self.unknownInt2 = if h.v <= 0x02030000: r.readUInt32()

# Ambient light source.
class NiAmbientLight(NiLight):

    def __init__(self, r: Reader, h: Header):

# Generic rotating particles data object.
class NiParticlesData(NiGeometryData):
    numParticles: int                                   # The maximum number of particles (matches the number of vertices).
    particleRadius: float                               # The particles' size.
    hasRadii: bool                                      # Is the particle size array present?
    radii: list[float]                                  # The individual particle sizes.
    numActive: int                                      # The number of active particles at the time the system was saved. This is also the number of valid entries in the following arrays.
    hasSizes: bool                                      # Is the particle size array present?
    sizes: list[float]                                  # The individual particle sizes.
    hasRotations: bool                                  # Is the particle rotation array present?
    rotations: list[Quaternion]                         # The individual particle rotations.
    hasRotationAngles: bool                             # Are the angles of rotation present?
    rotationAngles: list[float]                         # Angles of rotation
    hasRotationAxes: bool                               # Are axes of rotation present?
    rotationAxes: list[Vector3]                         # Axes of rotation.
    hasTextureIndices: bool
    numSubtextureOffsets: int                           # How many quads to use in BSPSysSubTexModifier for texture atlasing
    subtextureOffsets: list[Vector4]                    # Defines UV offsets
    aspectRatio: float                                  # Sets aspect ratio for Subtexture Offset UV quads
    aspectFlags: int
    speedtoAspectAspect2: float
    speedtoAspectSpeed1: float
    speedtoAspectSpeed2: float

    def __init__(self, r: Reader, h: Header):
        self.numParticles = if h.v <= 0x04000002: r.readUInt16()
        self.particleRadius = if h.v <= 0x0A000100: r.readSingle()
        self.hasRadii = if h.v >= 0x0A010000: r.readBool32()
        self.radii = if Has Radii and h.v >= 0x0A010000 and : r.readPArray(None, 'f', NumVertices)
        self.numActive = r.readUInt16()
        self.hasSizes = r.readBool32()
        self.sizes = if Has Sizes: r.readPArray(None, 'f', NumVertices)
        self.hasRotations = if h.v >= 0x0A000100: r.readBool32()
        self.rotations = if Has Rotations and h.v >= 0x0A000100 and : r.readFArray(lambda r: r.readQuaternion(), NumVertices)
        self.hasRotationAngles = if h.v >= 0x14000004: r.readBool32()
        self.rotationAngles = if Has Rotation Angles: r.readPArray(None, 'f', NumVertices)
        self.hasRotationAxes = if h.v >= 0x14000004: r.readBool32()
        self.rotationAxes = if Has Rotation Axes and h.v >= 0x14000004 and : r.readFArray(lambda r: r.readVector3(), NumVertices)
        self.hasTextureIndices = r.readBool32()
        self.numSubtextureOffsets = r.readUInt32()
        self.subtextureOffsets = r.readL8FArray(lambda r: r.readL8Vector4())
        self.aspectRatio = r.readSingle()
        self.aspectFlags = r.readUInt16()
        self.speedtoAspectAspect2 = r.readSingle()
        self.speedtoAspectSpeed1 = r.readSingle()
        self.speedtoAspectSpeed2 = r.readSingle()

# Rotating particles data object.
class NiRotatingParticlesData(NiParticlesData):
    hasRotations2: bool                                 # Is the particle rotation array present?
    rotations2: list[Quaternion]                        # The individual particle rotations.

    def __init__(self, r: Reader, h: Header):
        self.hasRotations2 = if h.v <= 0x04020200: r.readBool32()
        self.rotations2 = if Has Rotations 2 and h.v <= 0x04020200: r.readFArray(lambda r: r.readQuaternion(), NumVertices)

# Particle system data object (with automatic normals?).
class NiAutoNormalParticlesData(NiParticlesData):

    def __init__(self, r: Reader, h: Header):

# Particle Description.
class ParticleDesc:
    def __init__(self, r: Reader, h: Header):
        self.translation: Vector3 = r.readVector3()     # Unknown.
        self.unknownFloats1: list[float] = r.readPArray(None, 'f', 3) if h.v <= 0x0A040001 else None # Unknown.
        self.unknownFloat1: float = r.readSingle()      # Unknown.
        self.unknownFloat2: float = r.readSingle()      # Unknown.
        self.unknownFloat3: float = r.readSingle()      # Unknown.
        self.unknownInt1: int = r.readInt32()           # Unknown.

# Particle system data.
class NiPSysData(NiParticlesData):
    particleDescriptions: list[ParticleDesc]
    hasRotationSpeeds: bool
    rotationSpeeds: list[float]
    numAddedParticles: int
    addedParticlesBase: int

    def __init__(self, r: Reader, h: Header):
        self.particleDescriptions = r.readFArray(lambda r: ParticleDesc(r, h), NumVertices)
        self.hasRotationSpeeds = if h.v >= 0x14000002: r.readBool32()
        self.rotationSpeeds = if Has Rotation Speeds and h.v >= 0x14000002 and : r.readPArray(None, 'f', NumVertices)
        self.numAddedParticles = r.readUInt16()
        self.addedParticlesBase = r.readUInt16()

# Particle meshes data.
class NiMeshPSysData(NiPSysData):
    defaultPoolSize: int
    fillPoolsOnLoad: bool
    generations: list[int]
    particleMeshes: int

    def __init__(self, r: Reader, h: Header):
        self.defaultPoolSize = if h.v >= 0x0A020000: r.readUInt32()
        self.fillPoolsOnLoad = if h.v >= 0x0A020000: r.readBool32()
        self.generations = if h.v >= 0x0A020000: r.readL32PArray(None, 'I')
        self.particleMeshes = X[NiNode].ref(r)

# Binary extra data object. Used to store tangents and bitangents in Oblivion.
class NiBinaryExtraData(NiExtraData):
    binaryData: bytearray                               # The binary data.

    def __init__(self, r: Reader, h: Header):
        self.binaryData = r.readL8Bytes()

# Voxel extra data object.
class NiBinaryVoxelExtraData(NiExtraData):
    unknownInt: int                                     # Unknown.  0?
    data: int                                           # Link to binary voxel data.

    def __init__(self, r: Reader, h: Header):
        self.unknownInt = r.readUInt32()
        self.data = X[NiBinaryVoxelData].ref(r)

# Voxel data object.
class NiBinaryVoxelData(NiObject):
    unknownShort1: int                                  # Unknown.
    unknownShort2: int                                  # Unknown.
    unknownShort3: int                                  # Unknown. Is this^3 the Unknown Bytes 1 size?
    unknown7Floats: list[float]                         # Unknown.
    unknownBytes1: list[list[int]]                      # Unknown. Always a multiple of 7.
    unknownVectors: list[Vector4]                       # Vectors on the unit sphere.
    unknownBytes2: list[int]                            # Unknown.
    unknown5Ints: list[int]                             # Unknown.

    def __init__(self, r: Reader, h: Header):
        self.unknownShort1 = r.readUInt16()
        self.unknownShort2 = r.readUInt16()
        self.unknownShort3 = r.readUInt16()
        self.unknown7Floats = r.readPArray(None, 'f', 7)
        self.unknownBytes1 = r.readFArray(lambda k: r.readBytes(7), 12)
        self.unknownVectors = r.readL32FArray(lambda r: r.readL32Vector4())
        self.unknownBytes2 = r.readL32Bytes(L32)
        self.unknown5Ints = r.readPArray(None, 'I', 5)

# Blends bool values together.
class NiBlendBoolInterpolator(NiBlendInterpolator):
    value: int                                          # The pose value. Invalid if using data.

    def __init__(self, r: Reader, h: Header):
        self.value = r.readByte()

# Blends float values together.
class NiBlendFloatInterpolator(NiBlendInterpolator):
    value: float                                        # The pose value. Invalid if using data.

    def __init__(self, r: Reader, h: Header):
        self.value = r.readSingle()

# Blends NiPoint3 values together.
class NiBlendPoint3Interpolator(NiBlendInterpolator):
    value: Vector3                                      # The pose value. Invalid if using data.

    def __init__(self, r: Reader, h: Header):
        self.value = r.readVector3()

# Blends NiQuatTransform values together.
class NiBlendTransformInterpolator(NiBlendInterpolator):
    value: NiQuatTransform

    def __init__(self, r: Reader, h: Header):
        self.value = if h.v <= 0x0A01006D: NiQuatTransform(r, h)

# Wrapper for boolean animation keys.
class NiBoolData(NiObject):
    data: KeyGroup[T]                                   # The boolean keys.

    def __init__(self, r: Reader, h: Header):
        self.data = KeyGroup[T](r, h)

# Boolean extra data.
class NiBooleanExtraData(NiExtraData):
    booleanData: int                                    # The boolean extra data value.

    def __init__(self, r: Reader, h: Header):
        self.booleanData = r.readByte()

# Contains an NiBSplineBasis for use in interpolation of open, uniform B-Splines.
class NiBSplineBasisData(NiObject):
    numControlPoints: int                               # The number of control points of the B-spline (number of frames of animation plus degree of B-spline minus one).

    def __init__(self, r: Reader, h: Header):
        self.numControlPoints = r.readUInt32()

# Uses B-Splines to animate a float value over time.
class NiBSplineFloatInterpolator(NiBSplineInterpolator):
    value: float                                        # Base value when curve not defined.
    handle: int                                         # Handle into the data. (USHRT_MAX for invalid handle.)

    def __init__(self, r: Reader, h: Header):
        self.value = r.readSingle()
        self.handle = r.readUInt32()

# NiBSplineFloatInterpolator plus the information required for using compact control points.
class NiBSplineCompFloatInterpolator(NiBSplineFloatInterpolator):
    floatOffset: float
    floatHalfRange: float

    def __init__(self, r: Reader, h: Header):
        self.floatOffset = r.readSingle()
        self.floatHalfRange = r.readSingle()

# Uses B-Splines to animate an NiPoint3 value over time.
class NiBSplinePoint3Interpolator(NiBSplineInterpolator):
    value: Vector3                                      # Base value when curve not defined.
    handle: int                                         # Handle into the data. (USHRT_MAX for invalid handle.)

    def __init__(self, r: Reader, h: Header):
        self.value = r.readVector3()
        self.handle = r.readUInt32()

# NiBSplinePoint3Interpolator plus the information required for using compact control points.
class NiBSplineCompPoint3Interpolator(NiBSplinePoint3Interpolator):
    positionOffset: float
    positionHalfRange: float

    def __init__(self, r: Reader, h: Header):
        self.positionOffset = r.readSingle()
        self.positionHalfRange = r.readSingle()

# Supports the animation of position, rotation, and scale using an NiQuatTransform.
# The NiQuatTransform can be an unchanging pose or interpolated from B-Spline control point channels.
class NiBSplineTransformInterpolator(NiBSplineInterpolator):
    transform: NiQuatTransform
    translationHandle: int                              # Handle into the translation data. (USHRT_MAX for invalid handle.)
    rotationHandle: int                                 # Handle into the rotation data. (USHRT_MAX for invalid handle.)
    scaleHandle: int                                    # Handle into the scale data. (USHRT_MAX for invalid handle.)

    def __init__(self, r: Reader, h: Header):
        self.transform = NiQuatTransform(r, h)
        self.translationHandle = r.readUInt32()
        self.rotationHandle = r.readUInt32()
        self.scaleHandle = r.readUInt32()

# NiBSplineTransformInterpolator plus the information required for using compact control points.
class NiBSplineCompTransformInterpolator(NiBSplineTransformInterpolator):
    translationOffset: float
    translationHalfRange: float
    rotationOffset: float
    rotationHalfRange: float
    scaleOffset: float
    scaleHalfRange: float

    def __init__(self, r: Reader, h: Header):
        self.translationOffset = r.readSingle()
        self.translationHalfRange = r.readSingle()
        self.rotationOffset = r.readSingle()
        self.rotationHalfRange = r.readSingle()
        self.scaleOffset = r.readSingle()
        self.scaleHalfRange = r.readSingle()

class BSRotAccumTransfInterpolator(NiTransformInterpolator):

    def __init__(self, r: Reader, h: Header):

# Contains one or more sets of control points for use in interpolation of open, uniform B-Splines, stored as either float or compact.
class NiBSplineData(NiObject):
    floatControlPoints: list[float]                     # Float values representing the control data.
    compactControlPoints: list[int]                     # Signed shorts representing the data from 0 to 1 (scaled by SHRT_MAX).

    def __init__(self, r: Reader, h: Header):
        self.floatControlPoints = r.readL32PArray(None, 'f')
        self.compactControlPoints = r.readL32PArray(None, 'h')

# Camera object.
class NiCamera(NiAVObject):
    cameraFlags: int                                    # Obsolete flags.
    frustumLeft: float                                  # Frustrum left.
    frustumRight: float                                 # Frustrum right.
    frustumTop: float                                   # Frustrum top.
    frustumBottom: float                                # Frustrum bottom.
    frustumNear: float                                  # Frustrum near.
    frustumFar: float                                   # Frustrum far.
    useOrthographicProjection: bool                     # Determines whether perspective is used.  Orthographic means no perspective.
    viewportLeft: float                                 # Viewport left.
    viewportRight: float                                # Viewport right.
    viewportTop: float                                  # Viewport top.
    viewportBottom: float                               # Viewport bottom.
    lodAdjust: float                                    # Level of detail adjust.
    scene: int
    numScreenPolygons: int                              # Deprecated. Array is always zero length on disk write.
    numScreenTextures: int                              # Deprecated. Array is always zero length on disk write.
    unknownInt3: int                                    # Unknown.

    def __init__(self, r: Reader, h: Header):
        self.cameraFlags = if h.v >= 0x0A010000: r.readUInt16()
        self.frustumLeft = r.readSingle()
        self.frustumRight = r.readSingle()
        self.frustumTop = r.readSingle()
        self.frustumBottom = r.readSingle()
        self.frustumNear = r.readSingle()
        self.frustumFar = r.readSingle()
        self.useOrthographicProjection = if h.v >= 0x0A010000: r.readBool32()
        self.viewportLeft = r.readSingle()
        self.viewportRight = r.readSingle()
        self.viewportTop = r.readSingle()
        self.viewportBottom = r.readSingle()
        self.lodAdjust = r.readSingle()
        self.scene = X[NiAVObject].ref(r)
        self.numScreenPolygons = r.readUInt32()
        self.numScreenTextures = if h.v >= 0x04020100: r.readUInt32()
        self.unknownInt3 = if h.v <= 0x03010000: r.readUInt32()

# Wrapper for color animation keys.
class NiColorData(NiObject):
    data: KeyGroup[T]                                   # The color keys.

    def __init__(self, r: Reader, h: Header):
        self.data = KeyGroup[T](r, h)

# Extra data in the form of NiColorA (red, green, blue, alpha).
class NiColorExtraData(NiExtraData):
    data: Color4                                        # RGBA Color?

    def __init__(self, r: Reader, h: Header):
        self.data = Color4(r)

# Controls animation sequences on a specific branch of the scene graph.
class NiControllerManager(NiTimeController):
    cumulative: bool                                    # Whether transformation accumulation is enabled. If accumulation is not enabled, the manager will treat all sequence data on the accumulation root as absolute data instead of relative delta values.
    controllerSequences: list[int]
    objectPalette: int

    def __init__(self, r: Reader, h: Header):
        self.cumulative = r.readBool32()
        self.controllerSequences = r.readL32FArray(X[NiControllerSequence].ref)
        self.objectPalette = X[NiDefaultAVObjectPalette].ref(r)

# Root node in NetImmerse .kf files (until version 10.0).
class NiSequence(NiObject):
    name: str                                           # The sequence name by which the animation system finds and manages this sequence.
    accumRootName: str                                  # The name of the NiAVObject serving as the accumulation root. This is where all accumulated translations, scales, and rotations are applied.
    textKeys: int
    unknownInt4: int                                    # Divinity 2
    unknownInt5: int                                    # Divinity 2
    numControlledBlocks: int
    arrayGrowBy: int
    controlledBlocks: list[ControlledBlock]

    def __init__(self, r: Reader, h: Header):
        self.name = Y.string(r)
        self.accumRootName = if h.v <= 0x0A010067: Y.string(r)
        self.textKeys = if h.v <= 0x0A010067: X[NiTextKeyExtraData].ref(r)
        self.unknownInt4 = if h.v >= 0x14030009 and h.v <= 0x14030009 and : r.readInt32()
        self.unknownInt5 = if h.v >= 0x14030009 and h.v <= 0x14030009 and : r.readInt32()
        self.numControlledBlocks = r.readUInt32()
        self.arrayGrowBy = if h.v >= 0x0A01006A: r.readUInt32()
        self.controlledBlocks = r.readFArray(lambda r: ControlledBlock(r, h), NumControlledBlocks)

# Root node in Gamebryo .kf files (version 10.0.1.0 and up).
class NiControllerSequence(NiSequence):
    weight: float                                       # The weight of a sequence describes how it blends with other sequences at the same priority.
    textKeys: int
    cycleType: CycleType
    frequency: float
    phase: float
    startTime: float
    stopTime: float
    playBackwards: bool
    manager: int                                        # The owner of this sequence.
    accumRootName: str                                  # The name of the NiAVObject serving as the accumulation root. This is where all accumulated translations, scales, and rotations are applied.
    accumFlags: AccumFlags
    stringPalette: int
    animNotes: int
    animNoteArrays: list[int]

    def __init__(self, r: Reader, h: Header):
        self.weight = if h.v >= 0x0A01006A: r.readSingle()
        self.textKeys = if h.v >= 0x0A01006A: X[NiTextKeyExtraData].ref(r)
        self.cycleType = if h.v >= 0x0A01006A: CycleType(r.readUInt32())
        self.frequency = if h.v >= 0x0A01006A: r.readSingle()
        self.phase = if h.v >= 0x0A01006A and h.v <= 0x0A040001: r.readSingle()
        self.startTime = if h.v >= 0x0A01006A: r.readSingle()
        self.stopTime = if h.v >= 0x0A01006A: r.readSingle()
        self.playBackwards = if h.v >= 0x0A01006A and h.v <= 0x0A01006A: r.readBool32()
        self.manager = if h.v >= 0x0A01006A: X[NiControllerManager].ptr(r)
        self.accumRootName = if h.v >= 0x0A01006A: Y.string(r)
        self.accumFlags = if h.v >= 0x14030008: AccumFlags(r.readUInt32())
        self.stringPalette = if h.v >= 0x0A010071 and h.v <= 0x14010000: X[NiStringPalette].ref(r)
        self.animNotes = if h.v >= 0x14020007 and : X[BSAnimNotes].ref(r)
        self.animNoteArrays = if h.v >= 0x14020007 and : r.readL16FArray(X[BSAnimNotes].ref)

# Abstract base class for indexing NiAVObject by name.
class NiAVObjectPalette(NiObject):

    def __init__(self, r: Reader, h: Header):

# NiAVObjectPalette implementation. Used to quickly look up objects by name.
class NiDefaultAVObjectPalette(NiAVObjectPalette):
    scene: int                                          # Scene root of the object palette.
    objs: list[AVObject]                                # The objects.

    def __init__(self, r: Reader, h: Header):
        self.scene = X[NiAVObject].ptr(r)
        self.objs = r.readL32FArray(lambda r: AVObject(r))

# Directional light source.
class NiDirectionalLight(NiLight):

    def __init__(self, r: Reader, h: Header):

# NiDitherProperty allows the application to turn the dithering of interpolated colors and fog values on and off.
class NiDitherProperty(NiProperty):
    flags: Flags                                        # 1 = Enable dithering

    def __init__(self, r: Reader, h: Header):
        self.flags = Flags(r.readUInt16())

# DEPRECATED (10.2), REMOVED (20.5). Replaced by NiTransformController and NiLookAtInterpolator.
class NiRollController(NiSingleInterpController):
    data: int                                           # The data for the controller.

    def __init__(self, r: Reader, h: Header):
        self.data = X[NiFloatData].ref(r)

# Wrapper for 1D (one-dimensional) floating point animation keys.
class NiFloatData(NiObject):
    data: KeyGroup[T]                                   # The keys.

    def __init__(self, r: Reader, h: Header):
        self.data = KeyGroup[T](r, h)

# Extra float data.
class NiFloatExtraData(NiExtraData):
    floatData: float                                    # The float data.

    def __init__(self, r: Reader, h: Header):
        self.floatData = r.readSingle()

# Extra float array data.
class NiFloatsExtraData(NiExtraData):
    data: list[float]                                   # Float data.

    def __init__(self, r: Reader, h: Header):
        self.data = r.readL32PArray(None, 'f')

# NiFogProperty allows the application to enable, disable and control the appearance of fog.
class NiFogProperty(NiProperty):
    flags: Flags                                        # Bit 0: Enables Fog
                                                        #     Bit 1: Sets Fog Function to FOG_RANGE_SQ
                                                        #     Bit 2: Sets Fog Function to FOG_VERTEX_ALPHA
                                                        # 
                                                        #     If Bit 1 and Bit 2 are not set, but fog is enabled, Fog function is FOG_Z_LINEAR.
    fogDepth: float                                     # Depth of the fog in normalized units. 1.0 = begins at near plane. 0.5 = begins halfway between the near and far planes.
    fogColor: Color3                                    # The color of the fog.

    def __init__(self, r: Reader, h: Header):
        self.flags = Flags(r.readUInt16())
        self.fogDepth = r.readSingle()
        self.fogColor = Color3(r)

# LEGACY (pre-10.1) particle modifier. Applies a gravitational field on the particles.
class NiGravity(NiParticleModifier):
    unknownFloat1: float                                # Unknown.
    force: float                                        # The strength/force of this gravity.
    type: FieldType                                     # The force field type.
    position: Vector3                                   # The position of the mass point relative to the particle system.
    direction: Vector3                                  # The direction of the applied acceleration.

    def __init__(self, r: Reader, h: Header):
        self.unknownFloat1 = if h.v >= 0x0303000D: r.readSingle()
        self.force = r.readSingle()
        self.type = FieldType(r.readUInt32())
        self.position = r.readVector3()
        self.direction = r.readVector3()

# Extra integer data.
class NiIntegerExtraData(NiExtraData):
    integerData: int                                    # The value of the extra data.

    def __init__(self, r: Reader, h: Header):
        self.integerData = r.readUInt32()

# Controls animation and collision.  Integer holds flags:
# Bit 0 : enable havok, bAnimated(Skyrim)
# Bit 1 : enable collision, bHavok(Skyrim)
# Bit 2 : is skeleton nif?, bRagdoll(Skyrim)
# Bit 3 : enable animation, bComplex(Skyrim)
# Bit 4 : FlameNodes present, bAddon(Skyrim)
# Bit 5 : EditorMarkers present, bEditorMarker(Skyrim)
# Bit 6 : bDynamic(Skyrim)
# Bit 7 : bArticulated(Skyrim)
# Bit 8 : bIKTarget(Skyrim)/needsTransformUpdates
# Bit 9 : bExternalEmit(Skyrim)
# Bit 10: bMagicShaderParticles(Skyrim)
# Bit 11: bLights(Skyrim)
# Bit 12: bBreakable(Skyrim)
# Bit 13: bSearchedBreakable(Skyrim) .. Runtime only?
class BSXFlags(NiIntegerExtraData):

    def __init__(self, r: Reader, h: Header):

# Extra integer array data.
class NiIntegersExtraData(NiExtraData):
    data: list[int]                                     # Integers.

    def __init__(self, r: Reader, h: Header):
        self.data = r.readL32PArray(None, 'I')

# An extended keyframe controller.
class BSKeyframeController(NiKeyframeController):
    data2: int                                          # A link to more keyframe data.

    def __init__(self, r: Reader, h: Header):
        self.data2 = X[NiKeyframeData].ref(r)

# DEPRECATED (10.2), RENAMED (10.2) to NiTransformData.
# Wrapper for transformation animation keys.
class NiKeyframeData(NiObject):
    numRotationKeys: int                                # The number of quaternion rotation keys. If the rotation type is XYZ (type 4) then this *must* be set to 1, and in this case the actual number of keys is stored in the XYZ Rotations field.
    rotationType: KeyType                               # The type of interpolation to use for rotation.  Can also be 4 to indicate that separate X, Y, and Z values are used for the rotation instead of Quaternions.
    quaternionKeys: list[QuatKey[T]]                    # The rotation keys if Quaternion rotation is used.
    order: float
    xyzRotations: list[KeyGroup[T]]                     # Individual arrays of keys for rotating X, Y, and Z individually.
    translations: KeyGroup[T]                           # Translation keys.
    scales: KeyGroup[T]                                 # Scale keys.

    def __init__(self, r: Reader, h: Header):
        self.numRotationKeys = r.readUInt32()
        self.rotationType = if Num Rotation Keys != 0: KeyType(r.readUInt32())
        self.quaternionKeys = if Rotation Type != 4: r.readFArray(lambda r: QuatKey[T](r, h), NumRotationKeys)
        self.order = if Rotation Type == 4 and h.v <= 0x0A010000: r.readSingle()
        self.xyzRotations = if Rotation Type == 4: r.readFArray(lambda r: KeyGroup[T](r, h), 3)
        self.translations = KeyGroup[T](r, h)
        self.scales = KeyGroup[T](r, h)

class LookAtFlags(Flag):
    LOOK_FLIP = 0,                  # Flip
    LOOK_Y_AXIS = 1 << 1,           # Y-Axis
    LOOK_Z_AXIS = 1 << 2            # Z-Axis

# DEPRECATED (10.2), REMOVED (20.5)
# Replaced by NiTransformController and NiLookAtInterpolator.
class NiLookAtController(NiTimeController):
    flags: LookAtFlags
    lookAt: int

    def __init__(self, r: Reader, h: Header):
        self.flags = if h.v >= 0x0A010000: LookAtFlags(r.readUInt16())
        self.lookAt = X[NiNode].ptr(r)

# NiLookAtInterpolator rotates an object so that it always faces a target object.
class NiLookAtInterpolator(NiInterpolator):
    flags: LookAtFlags
    lookAt: int
    lookAtName: str
    transform: NiQuatTransform
    interpolatorTranslation: int
    interpolatorRoll: int
    interpolatorScale: int

    def __init__(self, r: Reader, h: Header):
        self.flags = LookAtFlags(r.readUInt16())
        self.lookAt = X[NiNode].ptr(r)
        self.lookAtName = Y.string(r)
        self.transform = if h.v <= 0x1404000C: NiQuatTransform(r, h)
        self.interpolatorTranslation = X[NiPoint3Interpolator].ref(r)
        self.interpolatorRoll = X[NiFloatInterpolator].ref(r)
        self.interpolatorScale = X[NiFloatInterpolator].ref(r)

# Describes the surface properties of an object e.g. translucency, ambient color, diffuse color, emissive color, and specular color.
class NiMaterialProperty(NiProperty):
    flags: Flags                                        # Property flags.
    ambientColor: Color3                                # How much the material reflects ambient light.
    diffuseColor: Color3                                # How much the material reflects diffuse light.
    specularColor: Color3                               # How much light the material reflects in a specular manner.
    emissiveColor: Color3                               # How much light the material emits.
    glossiness: float                                   # The material glossiness.
    alpha: float                                        # The material transparency (1=non-transparant). Refer to a NiAlphaProperty object in this material's parent NiTriShape object, when alpha is not 1.
    emissiveMult: float

    def __init__(self, r: Reader, h: Header):
        self.flags = if h.v >= 0x03000000 and h.v <= 0x0A000102: Flags(r.readUInt16())
        self.ambientColor = Color3(r)
        self.diffuseColor = Color3(r)
        self.specularColor = Color3(r)
        self.emissiveColor = Color3(r)
        self.glossiness = r.readSingle()
        self.alpha = r.readSingle()
        self.emissiveMult = r.readSingle()

# DEPRECATED (20.5), replaced by NiMorphMeshModifier.
# Geometry morphing data.
class NiMorphData(NiObject):
    numMorphs: int                                      # Number of morphing object.
    numVertices: int                                    # Number of vertices.
    relativeTargets: int                                # This byte is always 1 in all official files.
    morphs: list[Morph]                                 # The geometry morphing objects.

    def __init__(self, r: Reader, h: Header):
        self.numMorphs = r.readUInt32()
        self.numVertices = r.readUInt32()
        self.relativeTargets = r.readByte()
        self.morphs = r.readFArray(lambda r: Morph(r, h), NumMorphs)

# Generic node object for grouping.
class NiNode(NiAVObject):
    children: list[int]                                 # List of child node object indices.
    effects: list[int]                                  # List of node effects. ADynamicEffect?

    def __init__(self, r: Reader, h: Header):
        self.children = r.readL32FArray(X[NiAVObject].ref)
        self.effects = r.readL32FArray(X[NiDynamicEffect].ref)

# A NiNode used as a skeleton bone?
class NiBone(NiNode):

    def __init__(self, r: Reader, h: Header):

# Morrowind specific.
class AvoidNode(NiNode):

    def __init__(self, r: Reader, h: Header):

# Firaxis-specific UI widgets?
class FxWidget(NiNode):
    unknown3: int                                       # Unknown.
    unknown292Bytes: list[int]                          # Looks like 9 links and some string data.

    def __init__(self, r: Reader, h: Header):
        self.unknown3 = r.readByte()
        self.unknown292Bytes = r.readBytes(292)

# Unknown.
class FxButton(FxWidget):

    def __init__(self, r: Reader, h: Header):

# Unknown.
class FxRadioButton(FxWidget):
    unknownInt1: int                                    # Unknown.
    unknownInt2: int                                    # Unknown.
    unknownInt3: int                                    # Unknown.
    buttons: list[int]                                  # Unknown pointers to other buttons.  Maybe other buttons in a group so they can be switch off if this one is switched on?

    def __init__(self, r: Reader, h: Header):
        self.unknownInt1 = r.readUInt32()
        self.unknownInt2 = r.readUInt32()
        self.unknownInt3 = r.readUInt32()
        self.buttons = r.readL32FArray(X[FxRadioButton].ptr)

# These nodes will always be rotated to face the camera creating a billboard effect for any attached objects.
# 
# In pre-10.1.0.0 the Flags field is used for BillboardMode.
# Bit 0: hidden
# Bits 1-2: collision mode
# Bit 3: unknown (set in most official meshes)
# Bits 5-6: billboard mode
# 
# Collision modes:
# 00 NONE
# 01 USE_TRIANGLES
# 10 USE_OBBS
# 11 CONTINUE
# 
# Billboard modes:
# 00 ALWAYS_FACE_CAMERA
# 01 ROTATE_ABOUT_UP
# 10 RIGID_FACE_CAMERA
# 11 ALWAYS_FACE_CENTER
class NiBillboardNode(NiNode):
    billboardMode: BillboardMode                        # The way the billboard will react to the camera.

    def __init__(self, r: Reader, h: Header):
        self.billboardMode = if h.v >= 0x0A010000: BillboardMode(r.readUInt16())

# Bethesda-specific extension of Node with animation properties stored in the flags, often 42?
class NiBSAnimationNode(NiNode):

    def __init__(self, r: Reader, h: Header):

# Unknown.
class NiBSParticleNode(NiNode):

    def __init__(self, r: Reader, h: Header):

# Flags for NiSwitchNode.
class NiSwitchFlags(Flag):
    UpdateOnlyActiveChild = 0,      # Update Only Active Child
    UpdateControllers = 1 << 1      # Update Controllers

# Represents groups of multiple scenegraph subtrees, only one of which (the "active child") is drawn at any given time.
class NiSwitchNode(NiNode):
    switchNodeFlags: NiSwitchFlags
    index: int

    def __init__(self, r: Reader, h: Header):
        self.switchNodeFlags = if h.v >= 0x0A010000: NiSwitchFlags(r.readUInt16())
        self.index = r.readUInt32()

# Level of detail selector. Links to different levels of detail of the same model, used to switch a geometry at a specified distance.
class NiLODNode(NiSwitchNode):
    lodCenter: Vector3
    lodLevels: list[LODRange]
    lodLevelData: int

    def __init__(self, r: Reader, h: Header):
        self.lodCenter = if h.v >= 0x04000002 and h.v <= 0x0A000100: r.readVector3()
        self.lodLevels = if h.v <= 0x0A000100: r.readL32FArray(lambda r: LODRange(r, h))
        self.lodLevelData = if h.v >= 0x0A010000: X[NiLODData].ref(r)

# NiPalette objects represent mappings from 8-bit indices to 24-bit RGB or 32-bit RGBA colors.
class NiPalette(NiObject):
    hasAlpha: int
    numEntries: int                                     # The number of palette entries. Always 256 but can also be 16.
    palette: list[ByteColor4]                           # The color palette.

    def __init__(self, r: Reader, h: Header):
        self.hasAlpha = r.readByte()
        self.numEntries = r.readUInt32()
        self.palette = if Num Entries == 16: r.readFArray(lambda r: Color4Byte(r), 16)
        self.palette = if Num Entries != 16: r.readFArray(lambda r: Color4Byte(r), 256)

# LEGACY (pre-10.1) particle modifier.
class NiParticleBomb(NiParticleModifier):
    decay: float
    duration: float
    deltaV: float
    start: float
    decayType: DecayType
    symmetryType: SymmetryType
    position: Vector3                                   # The position of the mass point relative to the particle system?
    direction: Vector3                                  # The direction of the applied acceleration?

    def __init__(self, r: Reader, h: Header):
        self.decay = r.readSingle()
        self.duration = r.readSingle()
        self.deltaV = r.readSingle()
        self.start = r.readSingle()
        self.decayType = DecayType(r.readUInt32())
        self.symmetryType = if h.v >= 0x0401000C: SymmetryType(r.readUInt32())
        self.position = r.readVector3()
        self.direction = r.readVector3()

# LEGACY (pre-10.1) particle modifier.
class NiParticleColorModifier(NiParticleModifier):
    colorData: int

    def __init__(self, r: Reader, h: Header):
        self.colorData = X[NiColorData].ref(r)

# LEGACY (pre-10.1) particle modifier.
class NiParticleGrowFade(NiParticleModifier):
    grow: float                                         # The time from the beginning of the particle lifetime during which the particle grows.
    fade: float                                         # The time from the end of the particle lifetime during which the particle fades.

    def __init__(self, r: Reader, h: Header):
        self.grow = r.readSingle()
        self.fade = r.readSingle()

# LEGACY (pre-10.1) particle modifier.
class NiParticleMeshModifier(NiParticleModifier):
    particleMeshes: list[int]

    def __init__(self, r: Reader, h: Header):
        self.particleMeshes = r.readL32FArray(X[NiAVObject].ref)

# LEGACY (pre-10.1) particle modifier.
class NiParticleRotation(NiParticleModifier):
    randomInitialAxis: int
    initialAxis: Vector3
    rotationSpeed: float

    def __init__(self, r: Reader, h: Header):
        self.randomInitialAxis = r.readByte()
        self.initialAxis = r.readVector3()
        self.rotationSpeed = r.readSingle()

# Generic particle system node.
class NiParticles(NiGeometry):
    vertexDesc: BSVertexDesc

    def __init__(self, r: Reader, h: Header):
        self.vertexDesc = BSVertexDesc(r)

# LEGACY (pre-10.1). NiParticles which do not house normals and generate them at runtime.
class NiAutoNormalParticles(NiParticles):

    def __init__(self, r: Reader, h: Header):

# LEGACY (pre-10.1). Particle meshes.
class NiParticleMeshes(NiParticles):

    def __init__(self, r: Reader, h: Header):

# LEGACY (pre-10.1). Particle meshes data.
class NiParticleMeshesData(NiRotatingParticlesData):
    unknownLink2: int                                   # Refers to the mesh that makes up a particle?

    def __init__(self, r: Reader, h: Header):
        self.unknownLink2 = X[NiAVObject].ref(r)

# A particle system.
class NiParticleSystem(NiParticles):
    farBegin: int
    farEnd: int
    nearBegin: int
    nearEnd: int
    data: int
    worldSpace: bool                                    # If true, Particles are birthed into world space.  If false, Particles are birthed into object space.
    modifiers: list[int]                                # The list of particle modifiers.

    def __init__(self, r: Reader, h: Header):
        self.farBegin = r.readUInt16()
        self.farEnd = r.readUInt16()
        self.nearBegin = r.readUInt16()
        self.nearEnd = r.readUInt16()
        self.data = X[NiPSysData].ref(r)
        self.worldSpace = if h.v >= 0x0A010000: r.readBool32()
        self.modifiers = if h.v >= 0x0A010000: r.readL32FArray(X[NiPSysModifier].ref)

# Particle system.
class NiMeshParticleSystem(NiParticleSystem):

    def __init__(self, r: Reader, h: Header):

# A generic particle system time controller object.
class NiParticleSystemController(NiTimeController):
    oldSpeed: int                                       # Particle speed in old files
    speed: float                                        # Particle speed
    speedRandom: float                                  # Particle random speed modifier
    verticalDirection: float                            # vertical emit direction [radians]
                                                        #     0.0 : up
                                                        #     1.6 : horizontal
                                                        #     3.1416 : down
    verticalAngle: float                                # emitter's vertical opening angle [radians]
    horizontalDirection: float                          # horizontal emit direction
    horizontalAngle: float                              # emitter's horizontal opening angle
    unknownNormal: Vector3                              # Unknown.
    unknownColor: Color4                                # Unknown.
    size: float                                         # Particle size
    emitStartTime: float                                # Particle emit start time
    emitStopTime: float                                 # Particle emit stop time
    unknownByte: int                                    # Unknown byte, (=0)
    oldEmitRate: int                                    # Particle emission rate in old files
    emitRate: float                                     # Particle emission rate (particles per second)
    lifetime: float                                     # Particle lifetime
    lifetimeRandom: float                               # Particle lifetime random modifier
    emitFlags: int                                      # Bit 0: Emit Rate toggle bit (0 = auto adjust, 1 = use Emit Rate value)
    startRandom: Vector3                                # Particle random start translation vector
    emitter: int                                        # This index targets the particle emitter object (TODO: find out what type of object this refers to).
    unknownShort2: int                                  # ? short=0 ?
    unknownFloat13: float                               # ? float=1.0 ?
    unknownInt1: int                                    # ? int=1 ?
    unknownInt2: int                                    # ? int=0 ?
    unknownShort3: int                                  # ? short=0 ?
    particleVelocity: Vector3                           # Particle velocity
    particleUnknownVector: Vector3                      # Unknown
    particleLifetime: float                             # The particle's age.
    particleLink: int
    particleTimestamp: int                              # Timestamp of the last update.
    particleUnknownShort: int                           # Unknown short
    particleVertexId: int                               # Particle/vertex index matches array index
    numParticles: int                                   # Size of the following array. (Maximum number of simultaneous active particles)
    numValid: int                                       # Number of valid entries in the following array. (Number of active particles at the time the system was saved)
    particles: list[Particle]                           # Individual particle modifiers?
    unknownLink: int                                    # unknown int (=0xffffffff)
    particleExtra: int                                  # Link to some optional particle modifiers (NiGravity, NiParticleGrowFade, NiParticleBomb, ...)
    unknownLink2: int                                   # Unknown int (=0xffffffff)
    trailer: int                                        # Trailing null byte
    colorData: int
    unknownFloat1: float
    unknownFloats2: list[float]

    def __init__(self, r: Reader, h: Header):
        self.oldSpeed = if h.v <= 0x03010000: r.readUInt32()
        self.speed = if h.v >= 0x0303000D: r.readSingle()
        self.speedRandom = r.readSingle()
        self.verticalDirection = r.readSingle()
        self.verticalAngle = r.readSingle()
        self.horizontalDirection = r.readSingle()
        self.horizontalAngle = r.readSingle()
        self.unknownNormal = r.readVector3()
        self.unknownColor = Color4(r)
        self.size = r.readSingle()
        self.emitStartTime = r.readSingle()
        self.emitStopTime = r.readSingle()
        self.unknownByte = if h.v >= 0x04000002: r.readByte()
        self.oldEmitRate = if h.v <= 0x03010000: r.readUInt32()
        self.emitRate = if h.v >= 0x0303000D: r.readSingle()
        self.lifetime = r.readSingle()
        self.lifetimeRandom = r.readSingle()
        self.emitFlags = if h.v >= 0x04000002: r.readUInt16()
        self.startRandom = r.readVector3()
        self.emitter = X[NiObject].ptr(r)
        self.unknownShort2 = if h.v >= 0x04000002: r.readUInt16()
        self.unknownFloat13 = if h.v >= 0x04000002: r.readSingle()
        self.unknownInt1 = if h.v >= 0x04000002: r.readUInt32()
        self.unknownInt2 = if h.v >= 0x04000002: r.readUInt32()
        self.unknownShort3 = if h.v >= 0x04000002: r.readUInt16()
        self.particleVelocity = if h.v <= 0x03010000: r.readVector3()
        self.particleUnknownVector = if h.v <= 0x03010000: r.readVector3()
        self.particleLifetime = if h.v <= 0x03010000: r.readSingle()
        self.particleLink = if h.v <= 0x03010000: X[NiObject].ref(r)
        self.particleTimestamp = if h.v <= 0x03010000: r.readUInt32()
        self.particleUnknownShort = if h.v <= 0x03010000: r.readUInt16()
        self.particleVertexId = if h.v <= 0x03010000: r.readUInt16()
        self.numParticles = if h.v >= 0x04000002: r.readUInt16()
        self.numValid = if h.v >= 0x04000002: r.readUInt16()
        self.particles = if h.v >= 0x04000002: r.readFArray(lambda r: Particle(r), NumParticles)
        self.unknownLink = if h.v >= 0x04000002: X[NiObject].ref(r)
        self.particleExtra = X[NiParticleModifier].ref(r)
        self.unknownLink2 = X[NiObject].ref(r)
        self.trailer = if h.v >= 0x04000002: r.readByte()
        self.colorData = if h.v <= 0x03010000: X[NiColorData].ref(r)
        self.unknownFloat1 = if h.v <= 0x03010000: r.readSingle()
        self.unknownFloats2 = if h.v <= 0x03010000: r.readPArray(None, 'f', ParticleUnknownShort)

# A particle system controller, used by BS in conjunction with NiBSParticleNode.
class NiBSPArrayController(NiParticleSystemController):

    def __init__(self, r: Reader, h: Header):

# DEPRECATED (10.2), REMOVED (20.5). Replaced by NiTransformController and NiPathInterpolator.
# Time controller for a path.
class NiPathController(NiTimeController):
    pathFlags: PathFlags
    bankDir: int                                        # -1 = Negative, 1 = Positive
    maxBankAngle: float                                 # Max angle in radians.
    smoothing: float
    followAxis: int                                     # 0, 1, or 2 representing X, Y, or Z.
    pathData: int
    percentData: int

    def __init__(self, r: Reader, h: Header):
        self.pathFlags = if h.v >= 0x0A010000: PathFlags(r.readUInt16())
        self.bankDir = r.readInt32()
        self.maxBankAngle = r.readSingle()
        self.smoothing = r.readSingle()
        self.followAxis = r.readInt16()
        self.pathData = X[NiPosData].ref(r)
        self.percentData = X[NiFloatData].ref(r)

class PixelFormatComponent:
    def __init__(self, r: Reader):
        self.type: PixelComponent = PixelComponent(r.readUInt32()) # Component Type
        self.convention: PixelRepresentation = PixelRepresentation(r.readUInt32()) # Data Storage Convention
        self.bitsPerChannel: int = r.readByte()         # Bits per component
        self.isSigned: bool = r.readBool32()

class NiPixelFormat(NiObject):
    pixelFormat: PixelFormat                            # The format of the pixels in this internally stored image.
    redMask: int                                        # 0x000000ff (for 24bpp and 32bpp) or 0x00000000 (for 8bpp)
    greenMask: int                                      # 0x0000ff00 (for 24bpp and 32bpp) or 0x00000000 (for 8bpp)
    blueMask: int                                       # 0x00ff0000 (for 24bpp and 32bpp) or 0x00000000 (for 8bpp)
    alphaMask: int                                      # 0xff000000 (for 32bpp) or 0x00000000 (for 24bpp and 8bpp)
    bitsPerPixel: int                                   # Bits per pixel, 0 (Compressed), 8, 24 or 32.
    oldFastCompare: list[int]                           # [96,8,130,0,0,65,0,0] if 24 bits per pixel
                                                        #     [129,8,130,32,0,65,12,0] if 32 bits per pixel
                                                        #     [34,0,0,0,0,0,0,0] if 8 bits per pixel
                                                        #     [X,0,0,0,0,0,0,0] if 0 (Compressed) bits per pixel where X = PixelFormat
    tiling: PixelTiling
    rendererHint: int
    extraData: int
    flags: int
    sRgbSpace: bool
    channels: list[PixelFormatComponent]                # Channel Data

    def __init__(self, r: Reader, h: Header):
        self.pixelFormat = PixelFormat(r.readUInt32())
        self.redMask = if h.v <= 0x0A030002: r.readUInt32()
        self.greenMask = if h.v <= 0x0A030002: r.readUInt32()
        self.blueMask = if h.v <= 0x0A030002: r.readUInt32()
        self.alphaMask = if h.v <= 0x0A030002: r.readUInt32()
        self.bitsPerPixel = if h.v <= 0x0A030002: r.readUInt32()
        self.oldFastCompare = if h.v <= 0x0A030002: r.readBytes(8)
        self.tiling = if h.v >= 0x0A010000 and h.v <= 0x0A030002: PixelTiling(r.readUInt32())
        self.bitsPerPixel = if h.v >= 0x0A030003: r.readByte()
        self.rendererHint = if h.v >= 0x0A030003: r.readUInt32()
        self.extraData = if h.v >= 0x0A030003: r.readUInt32()
        self.flags = if h.v >= 0x0A030003: r.readByte()
        self.tiling = if h.v >= 0x0A030003: PixelTiling(r.readUInt32())
        self.sRgbSpace = if h.v >= 0x14030004: r.readBool32()
        self.channels = if h.v >= 0x0A030003: r.readFArray(lambda r: PixelFormatComponent(r), 4)

class NiPersistentSrcTextureRendererData(NiPixelFormat):
    palette: int
    numMipmaps: int
    bytesPerPixel: int
    mipmaps: list[MipMap]
    numPixels: int
    padNumPixels: int
    numFaces: int
    platform: PlatformID
    renderer: RendererID
    pixelData: list[int]

    def __init__(self, r: Reader, h: Header):
        self.palette = X[NiPalette].ref(r)
        self.numMipmaps = r.readUInt32()
        self.bytesPerPixel = r.readUInt32()
        self.mipmaps = r.readFArray(lambda r: MipMap(r), NumMipmaps)
        self.numPixels = r.readUInt32()
        self.padNumPixels = if h.v >= 0x14020006: r.readUInt32()
        self.numFaces = r.readUInt32()
        self.platform = if h.v <= 0x1E010000: PlatformID(r.readUInt32())
        self.renderer = if h.v >= 0x1E010001: RendererID(r.readUInt32())
        self.pixelData = r.readBytes(NumPixels*NumFaces)

# A texture.
class NiPixelData(NiPixelFormat):
    palette: int
    numMipmaps: int
    bytesPerPixel: int
    mipmaps: list[MipMap]
    numPixels: int
    numFaces: int
    pixelData: list[int]

    def __init__(self, r: Reader, h: Header):
        self.palette = X[NiPalette].ref(r)
        self.numMipmaps = r.readUInt32()
        self.bytesPerPixel = r.readUInt32()
        self.mipmaps = r.readFArray(lambda r: MipMap(r), NumMipmaps)
        self.numPixels = r.readUInt32()
        self.numFaces = if h.v >= 0x0A030006: r.readUInt32()
        self.pixelData = if h.v <= 0x0A030005: r.readBytes(NumPixels)
        self.pixelData = if h.v >= 0x0A030006: r.readBytes(NumPixels*NumFaces)

# LEGACY (pre-10.1) particle modifier.
class NiPlanarCollider(NiParticleModifier):
    unknownShort: int                                   # Usually 0?
    unknownFloat1: float                                # Unknown.
    unknownFloat2: float                                # Unknown.
    unknownShort2: int                                  # Unknown.
    unknownFloat3: float                                # Unknown.
    unknownFloat4: float                                # Unknown.
    unknownFloat5: float                                # Unknown.
    unknownFloat6: float                                # Unknown.
    unknownFloat7: float                                # Unknown.
    unknownFloat8: float                                # Unknown.
    unknownFloat9: float                                # Unknown.
    unknownFloat10: float                               # Unknown.
    unknownFloat11: float                               # Unknown.
    unknownFloat12: float                               # Unknown.
    unknownFloat13: float                               # Unknown.
    unknownFloat14: float                               # Unknown.
    unknownFloat15: float                               # Unknown.
    unknownFloat16: float                               # Unknown.

    def __init__(self, r: Reader, h: Header):
        self.unknownShort = if h.v >= 0x0A000100: r.readUInt16()
        self.unknownFloat1 = r.readSingle()
        self.unknownFloat2 = r.readSingle()
        self.unknownShort2 = if h.v >= 0x04020200 and h.v <= 0x04020200: r.readUInt16()
        self.unknownFloat3 = r.readSingle()
        self.unknownFloat4 = r.readSingle()
        self.unknownFloat5 = r.readSingle()
        self.unknownFloat6 = r.readSingle()
        self.unknownFloat7 = r.readSingle()
        self.unknownFloat8 = r.readSingle()
        self.unknownFloat9 = r.readSingle()
        self.unknownFloat10 = r.readSingle()
        self.unknownFloat11 = r.readSingle()
        self.unknownFloat12 = r.readSingle()
        self.unknownFloat13 = r.readSingle()
        self.unknownFloat14 = r.readSingle()
        self.unknownFloat15 = r.readSingle()
        self.unknownFloat16 = r.readSingle()

# A point light.
class NiPointLight(NiLight):
    constantAttenuation: float
    linearAttenuation: float
    quadraticAttenuation: float

    def __init__(self, r: Reader, h: Header):
        self.constantAttenuation = r.readSingle()
        self.linearAttenuation = r.readSingle()
        self.quadraticAttenuation = r.readSingle()

# Abstract base class for dynamic effects such as NiLights or projected texture effects.
class NiDeferredDynamicEffect(NiAVObject):
    switchState: bool                                   # If true, then the dynamic effect is applied to affected nodes during rendering.

    def __init__(self, r: Reader, h: Header):
        self.switchState = if h.v >= 0x0A01006A and : r.readBool32()

# Abstract base class that represents light sources in a scene graph.
# For Bethesda Stream 130 (FO4), NiLight now directly inherits from NiAVObject.
class NiDeferredLight(NiDeferredDynamicEffect):
    dimmer: float                                       # Scales the overall brightness of all light components.
    ambientColor: Color3
    diffuseColor: Color3
    specularColor: Color3

    def __init__(self, r: Reader, h: Header):
        self.dimmer = r.readSingle()
        self.ambientColor = Color3(r)
        self.diffuseColor = Color3(r)
        self.specularColor = Color3(r)

# A deferred point light. Custom (Twin Saga).
class NiDeferredPointLight(NiDeferredLight):
    constantAttenuation: float
    linearAttenuation: float
    quadraticAttenuation: float

    def __init__(self, r: Reader, h: Header):
        self.constantAttenuation = r.readSingle()
        self.linearAttenuation = r.readSingle()
        self.quadraticAttenuation = r.readSingle()

# Wrapper for position animation keys.
class NiPosData(NiObject):
    data: KeyGroup[T]

    def __init__(self, r: Reader, h: Header):
        self.data = KeyGroup[T](r, h)

# Wrapper for rotation animation keys.
class NiRotData(NiObject):
    numRotationKeys: int
    rotationType: KeyType
    quaternionKeys: list[QuatKey[T]]
    xyzRotations: list[KeyGroup[T]]

    def __init__(self, r: Reader, h: Header):
        self.numRotationKeys = r.readUInt32()
        self.rotationType = if Num Rotation Keys != 0: KeyType(r.readUInt32())
        self.quaternionKeys = if Rotation Type != 4: r.readFArray(lambda r: QuatKey[T](r, h), NumRotationKeys)
        self.xyzRotations = if Rotation Type == 4: r.readFArray(lambda r: KeyGroup[T](r, h), 3)

# Particle modifier that controls and updates the age of particles in the system.
class NiPSysAgeDeathModifier(NiPSysModifier):
    spawnonDeath: bool                                  # Should the particles spawn on death?
    spawnModifier: int                                  # The spawner to use on death.

    def __init__(self, r: Reader, h: Header):
        self.spawnonDeath = r.readBool32()
        self.spawnModifier = X[NiPSysSpawnModifier].ref(r)

# Particle modifier that applies an explosive force to particles.
class NiPSysBombModifier(NiPSysModifier):
    bombObject: int                                     # The object whose position and orientation are the basis of the force.
    bombAxis: Vector3                                   # The local direction of the force.
    decay: float                                        # How the bomb force will decrease with distance.
    deltaV: float                                       # The acceleration the bomb will apply to particles.
    decayType: DecayType
    symmetryType: SymmetryType

    def __init__(self, r: Reader, h: Header):
        self.bombObject = X[NiNode].ptr(r)
        self.bombAxis = r.readVector3()
        self.decay = r.readSingle()
        self.deltaV = r.readSingle()
        self.decayType = DecayType(r.readUInt32())
        self.symmetryType = SymmetryType(r.readUInt32())

# Particle modifier that creates and updates bound volumes.
class NiPSysBoundUpdateModifier(NiPSysModifier):
    updateSkip: int                                     # Optimize by only computing the bound of (1 / Update Skip) of the total particles each frame.

    def __init__(self, r: Reader, h: Header):
        self.updateSkip = r.readUInt16()

# Particle emitter that uses points within a defined Box shape to emit from.
class NiPSysBoxEmitter(NiPSysVolumeEmitter):
    width: float
    height: float
    depth: float

    def __init__(self, r: Reader, h: Header):
        self.width = r.readSingle()
        self.height = r.readSingle()
        self.depth = r.readSingle()

# Particle modifier that adds a defined shape to act as a collision object for particles to interact with.
class NiPSysColliderManager(NiPSysModifier):
    collider: int

    def __init__(self, r: Reader, h: Header):
        self.collider = X[NiPSysCollider].ref(r)

# Particle modifier that adds keyframe data to modify color/alpha values of particles over time.
class NiPSysColorModifier(NiPSysModifier):
    data: int

    def __init__(self, r: Reader, h: Header):
        self.data = X[NiColorData].ref(r)

# Particle emitter that uses points within a defined Cylinder shape to emit from.
class NiPSysCylinderEmitter(NiPSysVolumeEmitter):
    radius: float
    height: float

    def __init__(self, r: Reader, h: Header):
        self.radius = r.readSingle()
        self.height = r.readSingle()

# Particle modifier that applies a linear drag force to particles.
class NiPSysDragModifier(NiPSysModifier):
    dragObject: int                                     # The object whose position and orientation are the basis of the force.
    dragAxis: Vector3                                   # The local direction of the force.
    percentage: float                                   # The amount of drag to apply to particles.
    range: float                                        # The distance up to which particles are fully affected.
    rangeFalloff: float                                 # The distance at which particles cease to be affected.

    def __init__(self, r: Reader, h: Header):
        self.dragObject = X[NiAVObject].ptr(r)
        self.dragAxis = r.readVector3()
        self.percentage = r.readSingle()
        self.range = r.readSingle()
        self.rangeFalloff = r.readSingle()

# DEPRECATED (10.2). Particle system emitter controller data.
class NiPSysEmitterCtlrData(NiObject):
    birthRateKeys: KeyGroup[T]
    activeKeys: list[Key[T]]

    def __init__(self, r: Reader, h: Header):
        self.birthRateKeys = KeyGroup[T](r, h)
        self.activeKeys = r.readL32FArray(lambda r: Key[T](r, h))

# Particle modifier that applies a gravitational force to particles.
class NiPSysGravityModifier(NiPSysModifier):
    gravityObject: int                                  # The object whose position and orientation are the basis of the force.
    gravityAxis: Vector3                                # The local direction of the force.
    decay: float                                        # How the force diminishes by distance.
    strength: float                                     # The acceleration of the force.
    forceType: ForceType                                # The type of gravitational force.
    turbulence: float                                   # Adds a degree of randomness.
    turbulenceScale: float                              # Scale for turbulence.
    worldAligned: bool

    def __init__(self, r: Reader, h: Header):
        self.gravityObject = X[NiAVObject].ptr(r)
        self.gravityAxis = r.readVector3()
        self.decay = r.readSingle()
        self.strength = r.readSingle()
        self.forceType = ForceType(r.readUInt32())
        self.turbulence = r.readSingle()
        self.turbulenceScale = r.readSingle()
        self.worldAligned = r.readBool32()

# Particle modifier that controls the time it takes to grow and shrink a particle.
class NiPSysGrowFadeModifier(NiPSysModifier):
    growTime: float                                     # The time taken to grow from 0 to their specified size.
    growGeneration: int                                 # Specifies the particle generation to which the grow effect should be applied. This is usually generation 0, so that newly created particles will grow.
    fadeTime: float                                     # The time taken to shrink from their specified size to 0.
    fadeGeneration: int                                 # Specifies the particle generation to which the shrink effect should be applied. This is usually the highest supported generation for the particle system.
    baseScale: float                                    # A multiplier on the base particle scale.

    def __init__(self, r: Reader, h: Header):
        self.growTime = r.readSingle()
        self.growGeneration = r.readUInt16()
        self.fadeTime = r.readSingle()
        self.fadeGeneration = r.readUInt16()
        self.baseScale = r.readSingle()

# Particle emitter that uses points on a specified mesh to emit from.
class NiPSysMeshEmitter(NiPSysEmitter):
    emitterMeshes: list[int]                            # The meshes which are emitted from.
    initialVelocityType: VelocityType                   # The method by which the initial particle velocity will be computed.
    emissionType: EmitFrom                              # The manner in which particles are emitted from the Emitter Meshes.
    emissionAxis: Vector3                               # The emission axis if VELOCITY_USE_DIRECTION.

    def __init__(self, r: Reader, h: Header):
        self.emitterMeshes = r.readL32FArray(X[NiAVObject].ptr)
        self.initialVelocityType = VelocityType(r.readUInt32())
        self.emissionType = EmitFrom(r.readUInt32())
        self.emissionAxis = r.readVector3()

# Particle modifier that updates mesh particles using the age of each particle.
class NiPSysMeshUpdateModifier(NiPSysModifier):
    meshes: list[int]

    def __init__(self, r: Reader, h: Header):
        self.meshes = r.readL32FArray(X[NiAVObject].ref)

class BSPSysInheritVelocityModifier(NiPSysModifier):
    target: int
    chanceToInherit: float
    velocityMultiplier: float
    velocityVariation: float

    def __init__(self, r: Reader, h: Header):
        self.target = X[NiNode].ptr(r)
        self.chanceToInherit = r.readSingle()
        self.velocityMultiplier = r.readSingle()
        self.velocityVariation = r.readSingle()

class BSPSysHavokUpdateModifier(NiPSysModifier):
    nodes: list[int]
    modifier: int

    def __init__(self, r: Reader, h: Header):
        self.nodes = r.readL32FArray(X[NiNode].ref)
        self.modifier = X[NiPSysModifier].ref(r)

class BSPSysRecycleBoundModifier(NiPSysModifier):
    boundOffset: Vector3
    boundExtent: Vector3
    target: int

    def __init__(self, r: Reader, h: Header):
        self.boundOffset = r.readVector3()
        self.boundExtent = r.readVector3()
        self.target = X[NiNode].ptr(r)

# Similar to a Flip Controller, this handles particle texture animation on a single texture atlas
class BSPSysSubTexModifier(NiPSysModifier):
    startFrame: int                                     # Starting frame/position on atlas
    startFrameFudge: float                              # Random chance to start on a different frame?
    endFrame: float                                     # Ending frame/position on atlas
    loopStartFrame: float                               # Frame to start looping
    loopStartFrameFudge: float
    frameCount: float
    frameCountFudge: float

    def __init__(self, r: Reader, h: Header):
        self.startFrame = r.readUInt32()
        self.startFrameFudge = r.readSingle()
        self.endFrame = r.readSingle()
        self.loopStartFrame = r.readSingle()
        self.loopStartFrameFudge = r.readSingle()
        self.frameCount = r.readSingle()
        self.frameCountFudge = r.readSingle()

# Particle Collider object which particles will interact with.
class NiPSysPlanarCollider(NiPSysCollider):
    width: float                                        # Width of the plane along the X Axis.
    height: float                                       # Height of the plane along the Y Axis.
    xAxis: Vector3                                      # Axis defining a plane, relative to Collider Object.
    yAxis: Vector3                                      # Axis defining a plane, relative to Collider Object.

    def __init__(self, r: Reader, h: Header):
        self.width = r.readSingle()
        self.height = r.readSingle()
        self.xAxis = r.readVector3()
        self.yAxis = r.readVector3()

# Particle Collider object which particles will interact with.
class NiPSysSphericalCollider(NiPSysCollider):
    radius: float

    def __init__(self, r: Reader, h: Header):
        self.radius = r.readSingle()

# Particle modifier that updates the particle positions based on velocity and last update time.
class NiPSysPositionModifier(NiPSysModifier):

    def __init__(self, r: Reader, h: Header):

# Particle modifier that calls reset on a target upon looping.
class NiPSysResetOnLoopCtlr(NiTimeController):

    def __init__(self, r: Reader, h: Header):

# Particle modifier that adds rotations to particles.
class NiPSysRotationModifier(NiPSysModifier):
    rotationSpeed: float                                # Initial Rotation Speed in radians per second.
    rotationSpeedVariation: float                       # Distributes rotation speed over the range [Speed - Variation, Speed + Variation].
    rotationAngle: float                                # Initial Rotation Angle in radians.
    rotationAngleVariation: float                       # Distributes rotation angle over the range [Angle - Variation, Angle + Variation].
    randomRotSpeedSign: bool                            # Randomly negate the initial rotation speed?
    randomAxis: bool                                    # Assign a random axis to new particles?
    axis: Vector3                                       # Initial rotation axis.

    def __init__(self, r: Reader, h: Header):
        self.rotationSpeed = r.readSingle()
        self.rotationSpeedVariation = if h.v >= 0x14000002: r.readSingle()
        self.rotationAngle = if h.v >= 0x14000002: r.readSingle()
        self.rotationAngleVariation = if h.v >= 0x14000002: r.readSingle()
        self.randomRotSpeedSign = if h.v >= 0x14000002: r.readBool32()
        self.randomAxis = r.readBool32()
        self.axis = r.readVector3()

# Particle modifier that spawns additional copies of a particle.
class NiPSysSpawnModifier(NiPSysModifier):
    numSpawnGenerations: int                            # Number of allowed generations for spawning. Particles whose generations are >= will not be spawned.
    percentageSpawned: float                            # The likelihood of a particular particle being spawned. Must be between 0.0 and 1.0.
    minNumtoSpawn: int                                  # The minimum particles to spawn for any given original particle.
    maxNumtoSpawn: int                                  # The maximum particles to spawn for any given original particle.
    unknownInt: int                                     # WorldShift
    spawnSpeedVariation: float                          # How much the spawned particle speed can vary.
    spawnDirVariation: float                            # How much the spawned particle direction can vary.
    lifeSpan: float                                     # Lifespan assigned to spawned particles.
    lifeSpanVariation: float                            # The amount the lifespan can vary.

    def __init__(self, r: Reader, h: Header):
        self.numSpawnGenerations = r.readUInt16()
        self.percentageSpawned = r.readSingle()
        self.minNumtoSpawn = r.readUInt16()
        self.maxNumtoSpawn = r.readUInt16()
        self.unknownInt = if h.v >= 0x0A040001 and h.v <= 0x0A040001: r.readInt32()
        self.spawnSpeedVariation = r.readSingle()
        self.spawnDirVariation = r.readSingle()
        self.lifeSpan = r.readSingle()
        self.lifeSpanVariation = r.readSingle()

# Particle emitter that uses points within a sphere shape to emit from.
class NiPSysSphereEmitter(NiPSysVolumeEmitter):
    radius: float

    def __init__(self, r: Reader, h: Header):
        self.radius = r.readSingle()

# Particle system controller, tells the system to update its simulation.
class NiPSysUpdateCtlr(NiTimeController):

    def __init__(self, r: Reader, h: Header):

# Base for all force field particle modifiers.
class NiPSysFieldModifier(NiPSysModifier):
    fieldObject: int                                    # The object whose position and orientation are the basis of the field.
    magnitude: float                                    # Magnitude of the force.
    attenuation: float                                  # How the magnitude diminishes with distance from the Field Object.
    useMaxDistance: bool                                # Whether or not to use a distance from the Field Object after which there is no effect.
    maxDistance: float                                  # Maximum distance after which there is no effect.

    def __init__(self, r: Reader, h: Header):
        self.fieldObject = X[NiAVObject].ref(r)
        self.magnitude = r.readSingle()
        self.attenuation = r.readSingle()
        self.useMaxDistance = r.readBool32()
        self.maxDistance = r.readSingle()

# Particle system modifier, implements a vortex field force for particles.
class NiPSysVortexFieldModifier(NiPSysFieldModifier):
    direction: Vector3                                  # Direction of the vortex field in Field Object's space.

    def __init__(self, r: Reader, h: Header):
        self.direction = r.readVector3()

# Particle system modifier, implements a gravity field force for particles.
class NiPSysGravityFieldModifier(NiPSysFieldModifier):
    direction: Vector3                                  # Direction of the gravity field in Field Object's space.

    def __init__(self, r: Reader, h: Header):
        self.direction = r.readVector3()

# Particle system modifier, implements a drag field force for particles.
class NiPSysDragFieldModifier(NiPSysFieldModifier):
    useDirection: bool                                  # Whether or not the drag force applies only in the direction specified.
    direction: Vector3                                  # Direction in which the force applies if Use Direction is true.

    def __init__(self, r: Reader, h: Header):
        self.useDirection = r.readBool32()
        self.direction = r.readVector3()

# Particle system modifier, implements a turbulence field force for particles.
class NiPSysTurbulenceFieldModifier(NiPSysFieldModifier):
    frequency: float                                    # How many turbulence updates per second.

    def __init__(self, r: Reader, h: Header):
        self.frequency = r.readSingle()

class BSPSysLODModifier(NiPSysModifier):
    lodBeginDistance: float
    lodEndDistance: float
    endEmitScale: float
    endSize: float

    def __init__(self, r: Reader, h: Header):
        self.lodBeginDistance = r.readSingle()
        self.lodEndDistance = r.readSingle()
        self.endEmitScale = r.readSingle()
        self.endSize = r.readSingle()

class BSPSysScaleModifier(NiPSysModifier):
    scales: list[float]

    def __init__(self, r: Reader, h: Header):
        self.scales = r.readL32PArray(None, 'f')

# Particle system controller for force field magnitude.
class NiPSysFieldMagnitudeCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Particle system controller for force field attenuation.
class NiPSysFieldAttenuationCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Particle system controller for force field maximum distance.
class NiPSysFieldMaxDistanceCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Particle system controller for air field air friction.
class NiPSysAirFieldAirFrictionCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Particle system controller for air field inherit velocity.
class NiPSysAirFieldInheritVelocityCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Particle system controller for air field spread.
class NiPSysAirFieldSpreadCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Particle system controller for emitter initial rotation speed.
class NiPSysInitialRotSpeedCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Particle system controller for emitter initial rotation speed variation.
class NiPSysInitialRotSpeedVarCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Particle system controller for emitter initial rotation angle.
class NiPSysInitialRotAngleCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Particle system controller for emitter initial rotation angle variation.
class NiPSysInitialRotAngleVarCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Particle system controller for emitter planar angle.
class NiPSysEmitterPlanarAngleCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Particle system controller for emitter planar angle variation.
class NiPSysEmitterPlanarAngleVarCtlr(NiPSysModifierFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Particle system modifier, updates the particle velocity to simulate the effects of air movements like wind, fans, or wake.
class NiPSysAirFieldModifier(NiPSysFieldModifier):
    direction: Vector3                                  # Direction of the particle velocity
    airFriction: float                                  # How quickly particles will accelerate to the magnitude of the air field.
    inheritVelocity: float                              # How much of the air field velocity will be added to the particle velocity.
    inheritRotation: bool
    componentOnly: bool
    enableSpread: bool
    spread: float                                       # The angle of the air field cone if Enable Spread is true.

    def __init__(self, r: Reader, h: Header):
        self.direction = r.readVector3()
        self.airFriction = r.readSingle()
        self.inheritVelocity = r.readSingle()
        self.inheritRotation = r.readBool32()
        self.componentOnly = r.readBool32()
        self.enableSpread = r.readBool32()
        self.spread = r.readSingle()

# Guild 2-Specific node
class NiPSysTrailEmitter(NiPSysEmitter):
    unknownInt1: int                                    # Unknown
    unknownFloat1: float                                # Unknown
    unknownFloat2: float                                # Unknown
    unknownFloat3: float                                # Unknown
    unknownInt2: int                                    # Unknown
    unknownFloat4: float                                # Unknown
    unknownInt3: int                                    # Unknown
    unknownFloat5: float                                # Unknown
    unknownInt4: int                                    # Unknown
    unknownFloat6: float                                # Unknown
    unknownFloat7: float                                # Unknown

    def __init__(self, r: Reader, h: Header):
        self.unknownInt1 = r.readInt32()
        self.unknownFloat1 = r.readSingle()
        self.unknownFloat2 = r.readSingle()
        self.unknownFloat3 = r.readSingle()
        self.unknownInt2 = r.readInt32()
        self.unknownFloat4 = r.readSingle()
        self.unknownInt3 = r.readInt32()
        self.unknownFloat5 = r.readSingle()
        self.unknownInt4 = r.readInt32()
        self.unknownFloat6 = r.readSingle()
        self.unknownFloat7 = r.readSingle()

# Unknown controller
class NiLightIntensityController(NiFloatInterpController):

    def __init__(self, r: Reader, h: Header):

# Particle system modifier, updates the particle velocity to simulate the effects of point gravity.
class NiPSysRadialFieldModifier(NiPSysFieldModifier):
    radialType: float                                   # If zero, no attenuation.

    def __init__(self, r: Reader, h: Header):
        self.radialType = r.readSingle()

# Abstract class used for different types of LOD selections.
class NiLODData(NiObject):

    def __init__(self, r: Reader, h: Header):

# NiRangeLODData controls switching LOD levels based on Z depth from the camera to the NiLODNode.
class NiRangeLODData(NiLODData):
    lodCenter: Vector3
    lodLevels: list[LODRange]

    def __init__(self, r: Reader, h: Header):
        self.lodCenter = r.readVector3()
        self.lodLevels = r.readL32FArray(lambda r: LODRange(r, h))

# NiScreenLODData controls switching LOD levels based on proportion of the screen that a bound would include.
class NiScreenLODData(NiLODData):
    bound: NiBound
    worldBound: NiBound
    proportionLevels: list[float]

    def __init__(self, r: Reader, h: Header):
        self.bound = NiBound(r)
        self.worldBound = NiBound(r)
        self.proportionLevels = r.readL32PArray(None, 'f')

# Unknown.
class NiRotatingParticles(NiParticles):

    def __init__(self, r: Reader, h: Header):

# DEPRECATED (pre-10.1), REMOVED (20.3).
# Keyframe animation root node, in .kf files.
class NiSequenceStreamHelper(NiObjectNET):

    def __init__(self, r: Reader, h: Header):

# Determines whether flat shading or smooth shading is used on a shape.
class NiShadeProperty(NiProperty):
    flags: Flags                                        # Bit 0: Enable smooth phong shading on this shape. Otherwise, hard-edged flat shading will be used on this shape.

    def __init__(self, r: Reader, h: Header):
        self.flags = Flags(r.readUInt16())

# Skinning data.
class NiSkinData(NiObject):
    skinTransform: NiTransform                          # Offset of the skin from this bone in bind position.
    numBones: int                                       # Number of bones.
    skinPartition: int                                  # This optionally links a NiSkinPartition for hardware-acceleration information.
    hasVertexWeights: int                               # Enables Vertex Weights for this NiSkinData.
    boneList: list[BoneData]                            # Contains offset data for each node that this skin is influenced by.

    def __init__(self, r: Reader, h: Header):
        self.skinTransform = NiTransform(r)
        self.numBones = r.readUInt32()
        self.skinPartition = if h.v >= 0x04000002 and h.v <= 0x0A010000: X[NiSkinPartition].ref(r)
        self.hasVertexWeights = if h.v >= 0x04020100: r.readByte()
        self.boneList = r.readFArray(lambda r: BoneData(r, h), NumBones)

# Skinning instance.
class NiSkinInstance(NiObject):
    data: int                                           # Skinning data reference.
    skinPartition: int                                  # Refers to a NiSkinPartition objects, which partitions the mesh such that every vertex is only influenced by a limited number of bones.
    skeletonRoot: int                                   # Armature root node.
    bones: list[int]                                    # List of all armature bones.

    def __init__(self, r: Reader, h: Header):
        self.data = X[NiSkinData].ref(r)
        self.skinPartition = if h.v >= 0x0A010065: X[NiSkinPartition].ref(r)
        self.skeletonRoot = X[NiNode].ptr(r)
        self.bones = r.readL32FArray(X[NiNode].ptr)

# Old version of skinning instance.
class NiTriShapeSkinController(NiTimeController):
    numBones: int                                       # The number of node bones referenced as influences.
    vertexCounts: list[int]                             # The number of vertex weights stored for each bone.
    bones: list[int]                                    # List of all armature bones.
    boneData: list[list[OldSkinData]]                   # Contains skin weight data for each node that this skin is influenced by.

    def __init__(self, r: Reader, h: Header):
        self.numBones = r.readUInt32()
        self.vertexCounts = r.readPArray(None, 'I', NumBones)
        self.bones = r.readFArray(X[NiBone].ptr, NumBones)
        self.boneData = r.readFArray(lambda k: r.readFArray(lambda r: OldSkinData(r), NumBones), VertexCounts)

# A copy of NISkinInstance for use with NiClod meshes.
class NiClodSkinInstance(NiSkinInstance):

    def __init__(self, r: Reader, h: Header):

# Skinning data, optimized for hardware skinning. The mesh is partitioned in submeshes such that each vertex of a submesh is influenced only by a limited and fixed number of bones.
class NiSkinPartition(NiObject):
    numSkinPartitionBlocks: int
    skinPartitionBlocks: list[SkinPartition]            # Skin partition objects.
    dataSize: int
    vertexSize: int
    vertexDesc: BSVertexDesc
    vertexData: list[BSVertexDataSSE]
    partition: list[SkinPartition]

    def __init__(self, r: Reader, h: Header):
        self.numSkinPartitionBlocks = r.readUInt32()
        self.skinPartitionBlocks = r.readFArray(lambda r: SkinPartition(r, h), NumSkinPartitionBlocks)
        self.dataSize = if (h.uv2 == 100): r.readUInt32()
        self.vertexSize = if (h.uv2 == 100): r.readUInt32()
        self.vertexDesc = if (h.uv2 == 100): BSVertexDesc(r)
        self.vertexData = if Data Size > 0 and (h.uv2 == 100): r.readFArray(lambda r: BSVertexDataSSE(r, h), DataSize/VertexSize)
        self.partition = if (h.uv2 == 100): r.readFArray(lambda r: SkinPartition(r, h), NumSkinPartitionBlocks)

# A texture.
class NiTexture(NiObjectNET):

    def __init__(self, r: Reader, h: Header):

# NiTexture::FormatPrefs. These preferences are a request to the renderer to use a format the most closely matches the settings and may be ignored.
class FormatPrefs:
    def __init__(self, r: Reader):
        self.pixelLayout: PixelLayout = PixelLayout(r.readUInt32()) # Requests the way the image will be stored.
        self.useMipmaps: MipMapFormat = MipMapFormat(r.readUInt32()) # Requests if mipmaps are used or not.
        self.alphaFormat: AlphaFormat = AlphaFormat(r.readUInt32()) # Requests no alpha, 1-bit alpha, or

# Describes texture source and properties.
class NiSourceTexture(NiTexture):
    useExternal: int                                    # Is the texture external?
    fileName: ??                                        # The original source filename of the image embedded by the referred NiPixelData object.
    unknownLink: int                                    # Unknown.
    unknownByte: int                                    # Unknown. Seems to be set if Pixel Data is present?
    pixelData: int                                      # NiPixelData or NiPersistentSrcTextureRendererData
    formatPrefs: FormatPrefs                            # A set of preferences for the texture format. They are a request only and the renderer may ignore them.
    isStatic: int                                       # If set, then the application cannot assume that any dynamic changes to the pixel data will show in the rendered image.
    directRender: bool                                  # A hint to the renderer that the texture can be loaded directly from a texture file into a renderer-specific resource, bypassing the NiPixelData object.
    persistRenderData: bool                             # Pixel Data is NiPersistentSrcTextureRendererData instead of NiPixelData.

    def __init__(self, r: Reader, h: Header):
        self.useExternal = r.readByte()
        self.fileName = if Use External == 1: ??
        self.unknownLink = if Use External == 1 and h.v >= 0x0A010000: X[NiObject].ref(r)
        self.unknownByte = if Use External == 0 and h.v <= 0x0A000100: r.readByte()
        self.fileName = if Use External == 0 and h.v >= 0x0A010000: ??
        self.pixelData = if Use External == 0: X[NiPixelFormat].ref(r)
        self.formatPrefs = FormatPrefs(r)
        self.isStatic = r.readByte()
        self.directRender = if h.v >= 0x0A010067: r.readBool32()
        self.persistRenderData = if h.v >= 0x14020004: r.readBool32()

# Gives specularity to a shape. Flags 0x0001.
class NiSpecularProperty(NiProperty):
    flags: Flags                                        # Bit 0 = Enable specular lighting on this shape.

    def __init__(self, r: Reader, h: Header):
        self.flags = Flags(r.readUInt16())

# LEGACY (pre-10.1) particle modifier.
class NiSphericalCollider(NiParticleModifier):
    unknownFloat1: float                                # Unknown.
    unknownShort1: int                                  # Unknown.
    unknownFloat2: float                                # Unknown.
    unknownShort2: int                                  # Unknown.
    unknownFloat3: float                                # Unknown.
    unknownFloat4: float                                # Unknown.
    unknownFloat5: float                                # Unknown.

    def __init__(self, r: Reader, h: Header):
        self.unknownFloat1 = r.readSingle()
        self.unknownShort1 = r.readUInt16()
        self.unknownFloat2 = r.readSingle()
        self.unknownShort2 = if h.v <= 0x04020002: r.readUInt16()
        self.unknownFloat3 = if h.v >= 0x04020100: r.readSingle()
        self.unknownFloat4 = r.readSingle()
        self.unknownFloat5 = r.readSingle()

# A spot.
class NiSpotLight(NiPointLight):
    outerSpotAngle: float
    innerSpotAngle: float
    exponent: float                                     # Describes the distribution of light. (see: glLight)

    def __init__(self, r: Reader, h: Header):
        self.outerSpotAngle = r.readSingle()
        self.innerSpotAngle = if h.v >= 0x14020005: r.readSingle()
        self.exponent = r.readSingle()

# Allows control of stencil testing.
class NiStencilProperty(NiProperty):
    flags: Flags                                        # Property flags:
                                                        #     Bit 0: Stencil Enable
                                                        #     Bits 1-3: Fail Action
                                                        #     Bits 4-6: Z Fail Action
                                                        #     Bits 7-9: Pass Action
                                                        #     Bits 10-11: Draw Mode
                                                        #     Bits 12-14: Stencil Function
    stencilEnabled: int                                 # Enables or disables the stencil test.
    stencilFunction: StencilCompareMode                 # Selects the compare mode function (see: glStencilFunc).
    stencilRef: int
    stencilMask: int                                    # A bit mask. The default is 0xffffffff.
    failAction: StencilAction
    zFailAction: StencilAction
    passAction: StencilAction
    drawMode: StencilDrawMode                           # Used to enabled double sided faces. Default is 3 (DRAW_BOTH).

    def __init__(self, r: Reader, h: Header):
        self.flags = if h.v <= 0x0A000102: Flags(r.readUInt16())
        self.stencilEnabled = if h.v <= 0x14000005: r.readByte()
        self.stencilFunction = if h.v <= 0x14000005: StencilCompareMode(r.readUInt32())
        self.stencilRef = if h.v <= 0x14000005: r.readUInt32()
        self.stencilMask = if h.v <= 0x14000005: r.readUInt32()
        self.failAction = if h.v <= 0x14000005: StencilAction(r.readUInt32())
        self.zFailAction = if h.v <= 0x14000005: StencilAction(r.readUInt32())
        self.passAction = if h.v <= 0x14000005: StencilAction(r.readUInt32())
        self.drawMode = if h.v <= 0x14000005: StencilDrawMode(r.readUInt32())
        self.flags = if h.v >= 0x14010003: Flags(r.readUInt16())
        self.stencilRef = if h.v >= 0x14010003: r.readUInt32()
        self.stencilMask = if h.v >= 0x14010003: r.readUInt32()

# Apparently commands for an optimizer instructing it to keep things it would normally discard.
# Also refers to NiNode objects (through their name) in animation .kf files.
class NiStringExtraData(NiExtraData):
    bytesRemaining: int                                 # The number of bytes left in the record.  Equals the length of the following string + 4.
    stringData: str                                     # The string.

    def __init__(self, r: Reader, h: Header):
        self.bytesRemaining = if h.v <= 0x04020200: r.readUInt32()
        self.stringData = Y.string(r)

# List of 0x00-seperated strings, which are names of controlled objects and controller types. Used in .kf files in conjunction with NiControllerSequence.
class NiStringPalette(NiObject):
    palette: StringPalette                              # A bunch of 0x00 seperated strings.

    def __init__(self, r: Reader, h: Header):
        self.palette = StringPalette(r)

# List of strings; for example, a list of all bone names.
class NiStringsExtraData(NiExtraData):
    data: list[str]                                     # The strings.

    def __init__(self, r: Reader, h: Header):
        self.data = r.readL32FArray(lambda r: SizedString(r))

# Extra data, used to name different animation sequences.
class NiTextKeyExtraData(NiExtraData):
    unknownInt1: int                                    # Unknown.  Always equals zero in all official files.
    textKeys: list[Key[T]]                              # List of textual notes and at which time they take effect. Used for designating the start and stop of animations and the triggering of sounds.

    def __init__(self, r: Reader, h: Header):
        self.unknownInt1 = if h.v <= 0x04020200: r.readUInt32()
        self.textKeys = r.readL32FArray(lambda r: Key[T](r, h))

# Represents an effect that uses projected textures such as projected lights (gobos), environment maps, and fog maps.
class NiTextureEffect(NiDynamicEffect):
    modelProjectionMatrix: Matrix3x3                    # Model projection matrix.  Always identity?
    modelProjectionTransform: Vector3                   # Model projection transform.  Always (0,0,0)?
    textureFiltering: TexFilterMode                     # Texture Filtering mode.
    maxAnisotropy: int
    textureClamping: TexClampMode                       # Texture Clamp mode.
    textureType: TextureType                            # The type of effect that the texture is used for.
    coordinateGenerationType: CoordGenType              # The method that will be used to generate UV coordinates for the texture effect.
    image: int                                          # Image index.
    sourceTexture: int                                  # Source texture index.
    enablePlane: int                                    # Determines whether a clipping plane is used.
    plane: NiPlane
    pS2L: int
    pS2K: int
    unknownShort: int                                   # Unknown: 0.

    def __init__(self, r: Reader, h: Header):
        self.modelProjectionMatrix = r.readMatrix3x3()
        self.modelProjectionTransform = r.readVector3()
        self.textureFiltering = TexFilterMode(r.readUInt32())
        self.maxAnisotropy = if h.v >= 0x14050004: r.readUInt16()
        self.textureClamping = TexClampMode(r.readUInt32())
        self.textureType = TextureType(r.readUInt32())
        self.coordinateGenerationType = CoordGenType(r.readUInt32())
        self.image = if h.v <= 0x03010000: X[NiImage].ref(r)
        self.sourceTexture = if h.v >= 0x04000000: X[NiSourceTexture].ref(r)
        self.enablePlane = r.readByte()
        self.plane = NiPlane(r)
        self.pS2L = if h.v <= 0x0A020000: r.readInt16()
        self.pS2K = if h.v <= 0x0A020000: r.readInt16()
        self.unknownShort = if h.v <= 0x0401000C: r.readUInt16()

# LEGACY (pre-10.1)
class NiTextureModeProperty(NiProperty):
    unknownInts: list[int]
    unknownShort: int                                   # Unknown. Either 210 or 194.
    pS2L: int                                           # 0?
    pS2K: int                                           # -75?

    def __init__(self, r: Reader, h: Header):
        self.unknownInts = if h.v <= 0x02030000: r.readPArray(None, 'I', 3)
        self.unknownShort = if h.v >= 0x03000000: r.readInt16()
        self.pS2L = if h.v >= 0x03010000 and h.v <= 0x0A020000: r.readInt16()
        self.pS2K = if h.v >= 0x03010000 and h.v <= 0x0A020000: r.readInt16()

# LEGACY (pre-10.1)
class NiImage(NiObject):
    useExternal: int                                    # 0 if the texture is internal to the NIF file.
    fileName: ??                                        # The filepath to the texture.
    imageData: int                                      # Link to the internally stored image data.
    unknownInt: int                                     # Unknown.  Often seems to be 7. Perhaps m_uiMipLevels?
    unknownFloat: float                                 # Unknown.  Perhaps fImageScale?

    def __init__(self, r: Reader, h: Header):
        self.useExternal = r.readByte()
        self.fileName = if Use External != 0: ??
        self.imageData = if Use External == 0: X[NiRawImageData].ref(r)
        self.unknownInt = r.readUInt32()
        self.unknownFloat = if h.v >= 0x03010000: r.readSingle()

# LEGACY (pre-10.1)
class NiTextureProperty(NiProperty):
    unknownInts1: list[int]                             # Property flags.
    flags: Flags                                        # Property flags.
    image: int                                          # Link to the texture image.
    unknownInts2: list[int]                             # Unknown.  0?

    def __init__(self, r: Reader, h: Header):
        self.unknownInts1 = if h.v <= 0x02030000: r.readPArray(None, 'I', 2)
        self.flags = if h.v >= 0x03000000: Flags(r.readUInt16())
        self.image = X[NiImage].ref(r)
        self.unknownInts2 = if h.v >= 0x03000000 and h.v <= 0x03000300: r.readPArray(None, 'I', 2)

# Describes how a fragment shader should be configured for a given piece of geometry.
class NiTexturingProperty(NiProperty):
    flags: Flags                                        # Property flags.
    applyMode: ApplyMode                                # Determines how the texture will be applied.  Seems to have special functions in Oblivion.
    textureCount: int                                   # Number of textures.
    hasBaseTexture: bool                                # Do we have a base texture?
    baseTexture: TexDesc                                # The base texture.
    hasDarkTexture: bool                                # Do we have a dark texture?
    darkTexture: TexDesc                                # The dark texture.
    hasDetailTexture: bool                              # Do we have a detail texture?
    detailTexture: TexDesc                              # The detail texture.
    hasGlossTexture: bool                               # Do we have a gloss texture?
    glossTexture: TexDesc                               # The gloss texture.
    hasGlowTexture: bool                                # Do we have a glow texture?
    glowTexture: TexDesc                                # The glowing texture.
    hasBumpMapTexture: bool                             # Do we have a bump map texture?
    bumpMapTexture: TexDesc                             # The bump map texture.
    bumpMapLumaScale: float
    bumpMapLumaOffset: float
    bumpMapMatrix: Matrix2x2
    hasNormalTexture: bool                              # Do we have a normal texture?
    normalTexture: TexDesc                              # Normal texture.
    hasParallaxTexture: bool
    parallaxTexture: TexDesc
    parallaxOffset: float
    hasDecal0Texture: bool
    decal0Texture: TexDesc                              # The decal texture.
    hasDecal1Texture: bool
    decal1Texture: TexDesc                              # Another decal texture.
    hasDecal2Texture: bool
    decal2Texture: TexDesc                              # Another decal texture.
    hasDecal3Texture: bool
    decal3Texture: TexDesc                              # Another decal texture.
    shaderTextures: list[ShaderTexDesc]                 # Shader textures.

    def __init__(self, r: Reader, h: Header):
        self.flags = if h.v <= 0x0A000102: Flags(r.readUInt16())
        self.flags = if h.v >= 0x14010002: Flags(r.readUInt16())
        self.applyMode = if h.v >= 0x0303000D and h.v <= 0x14010001: ApplyMode(r.readUInt32())
        self.textureCount = r.readUInt32()
        self.hasBaseTexture = r.readBool32()
        self.baseTexture = if Has Base Texture: TexDesc(r, h)
        self.hasDarkTexture = r.readBool32()
        self.darkTexture = if Has Dark Texture: TexDesc(r, h)
        self.hasDetailTexture = r.readBool32()
        self.detailTexture = if Has Detail Texture: TexDesc(r, h)
        self.hasGlossTexture = r.readBool32()
        self.glossTexture = if Has Gloss Texture: TexDesc(r, h)
        self.hasGlowTexture = r.readBool32()
        self.glowTexture = if Has Glow Texture: TexDesc(r, h)
        self.hasBumpMapTexture = if Texture Count > 5 and h.v >= 0x0303000D: r.readBool32()
        self.bumpMapTexture = if Has Bump Map Texture: TexDesc(r, h)
        self.bumpMapLumaScale = if Has Bump Map Texture: r.readSingle()
        self.bumpMapLumaOffset = if Has Bump Map Texture: r.readSingle()
        self.bumpMapMatrix = if Has Bump Map Texture: r.readMatrix2x2()
        self.hasNormalTexture = if Texture Count > 6 and h.v >= 0x14020005: r.readBool32()
        self.normalTexture = if Has Normal Texture: TexDesc(r, h)
        self.hasParallaxTexture = if Texture Count > 7 and h.v >= 0x14020005: r.readBool32()
        self.parallaxTexture = if Has Parallax Texture: TexDesc(r, h)
        self.parallaxOffset = if Has Parallax Texture: r.readSingle()
        self.hasDecal0Texture = if Texture Count > 6 and h.v <= 0x14020004: r.readBool32()
        self.hasDecal0Texture = if Texture Count > 8 and h.v >= 0x14020005: r.readBool32()
        self.decal0Texture = if Has Decal 0 Texture: TexDesc(r, h)
        self.hasDecal1Texture = if Texture Count > 7 and h.v <= 0x14020004: r.readBool32()
        self.hasDecal1Texture = if Texture Count > 9 and h.v >= 0x14020005: r.readBool32()
        self.decal1Texture = if Has Decal 1 Texture: TexDesc(r, h)
        self.hasDecal2Texture = if Texture Count > 8 and h.v <= 0x14020004: r.readBool32()
        self.hasDecal2Texture = if Texture Count > 10 and h.v >= 0x14020005: r.readBool32()
        self.decal2Texture = if Has Decal 2 Texture: TexDesc(r, h)
        self.hasDecal3Texture = if Texture Count > 9 and h.v <= 0x14020004: r.readBool32()
        self.hasDecal3Texture = if Texture Count > 11 and h.v >= 0x14020005: r.readBool32()
        self.decal3Texture = if Has Decal 3 Texture: TexDesc(r, h)
        self.shaderTextures = if h.v >= 0x0A000100: r.readL32FArray(lambda r: ShaderTexDesc(r, h))

class NiMultiTextureProperty(NiTexturingProperty):

    def __init__(self, r: Reader, h: Header):

# Wrapper for transformation animation keys.
class NiTransformData(NiKeyframeData):

    def __init__(self, r: Reader, h: Header):

# A shape node that refers to singular triangle data.
class NiTriShape(NiTriBasedGeom):

    def __init__(self, r: Reader, h: Header):

# Holds mesh data using a list of singular triangles.
class NiTriShapeData(NiTriBasedGeomData):
    numTrianglePoints: int                              # Num Triangles times 3.
    hasTriangles: bool                                  # Do we have triangle data?
    triangles: list[Triangle]                           # Triangle face data.
    matchGroups: list[MatchGroup]                       # The shared normals.

    def __init__(self, r: Reader, h: Header):
        self.numTrianglePoints = r.readUInt32()
        self.hasTriangles = if h.v >= 0x0A010000: r.readBool32()
        self.triangles = if h.v <= 0x0A000102: r.readFArray(lambda r: Triangle(r), NumTriangles)
        self.triangles = if Has Triangles and h.v >= 0x0A000103: r.readFArray(lambda r: Triangle(r), NumTriangles)
        self.matchGroups = if h.v >= 0x03010000: r.readL16FArray(lambda r: MatchGroup(r))

# A shape node that refers to data organized into strips of triangles
class NiTriStrips(NiTriBasedGeom):

    def __init__(self, r: Reader, h: Header):

# Holds mesh data using strips of triangles.
class NiTriStripsData(NiTriBasedGeomData):
    numStrips: int                                      # Number of OpenGL triangle strips that are present.
    stripLengths: list[int]                             # The number of points in each triangle strip.
    hasPoints: bool                                     # Do we have strip point data?
    points: list[list[int]]                             # The points in the Triangle strips. Size is the sum of all entries in Strip Lengths.

    def __init__(self, r: Reader, h: Header):
        self.numStrips = r.readUInt16()
        self.stripLengths = r.readPArray(None, 'H', NumStrips)
        self.hasPoints = if h.v >= 0x0A000103: r.readBool32()
        self.points = if h.v <= 0x0A000102: r.readFArray(lambda k: r.readPArray(None, 'H', NumStrips), StripLengths)
        self.points = if Has Points and h.v >= 0x0A000103: r.readFArray(lambda k: r.readPArray(None, 'H', NumStrips), StripLengths)

# Unknown
class NiEnvMappedTriShape(NiObjectNET):
    unknown1: int                                       # unknown (=4 - 5)
    unknownMatrix: Matrix4x4                            # unknown
    children: list[int]                                 # List of child node object indices.
    child2: int                                         # unknown
    child3: int                                         # unknown

    def __init__(self, r: Reader, h: Header):
        self.unknown1 = r.readUInt16()
        self.unknownMatrix = r.readMatrix4x4()
        self.children = r.readL32FArray(X[NiAVObject].ref)
        self.child2 = X[NiObject].ref(r)
        self.child3 = X[NiObject].ref(r)

# Holds mesh data using a list of singular triangles.
class NiEnvMappedTriShapeData(NiTriShapeData):

    def __init__(self, r: Reader, h: Header):

# LEGACY (pre-10.1)
# Sub data of NiBezierMesh
class NiBezierTriangle4(NiObject):
    unknown1: list[int]                                 # unknown
    unknown2: int                                       # unknown
    matrix: Matrix3x3                                   # unknown
    vector1: Vector3                                    # unknown
    vector2: Vector3                                    # unknown
    unknown3: list[int]                                 # unknown
    unknown4: int                                       # unknown
    unknown5: int                                       # unknown
    unknown6: list[int]                                 # unknown

    def __init__(self, r: Reader, h: Header):
        self.unknown1 = r.readPArray(None, 'I', 6)
        self.unknown2 = r.readUInt16()
        self.matrix = r.readMatrix3x3()
        self.vector1 = r.readVector3()
        self.vector2 = r.readVector3()
        self.unknown3 = r.readPArray(None, 'h', 4)
        self.unknown4 = r.readByte()
        self.unknown5 = r.readUInt32()
        self.unknown6 = r.readPArray(None, 'h', 24)

# LEGACY (pre-10.1)
# Unknown
class NiBezierMesh(NiAVObject):
    bezierTriangle: list[int]                           # unknown
    unknown3: int                                       # Unknown.
    count1: int                                         # Data count.
    unknown4: int                                       # Unknown.
    points1: list[Vector3]                              # data.
    unknown5: int                                       # Unknown (illegal link?).
    points2: list[list[float]]                          # data.
    unknown6: int                                       # unknown
    count2: int                                         # data count 2.
    data2: list[list[int]]                              # data count.

    def __init__(self, r: Reader, h: Header):
        self.bezierTriangle = r.readL32FArray(X[NiBezierTriangle4].ref)
        self.unknown3 = r.readUInt32()
        self.count1 = r.readUInt16()
        self.unknown4 = r.readUInt16()
        self.points1 = r.readFArray(lambda r: r.readVector3(), Count1)
        self.unknown5 = r.readUInt32()
        self.points2 = r.readFArray(lambda k: r.readPArray(None, 'f', Count1), 2)
        self.unknown6 = r.readUInt32()
        self.count2 = r.readUInt16()
        self.data2 = r.readFArray(lambda k: r.readPArray(None, 'H', Count2), 4)

# A shape node that holds continuous level of detail information.
# Seems to be specific to Freedom Force.
class NiClod(NiTriBasedGeom):

    def __init__(self, r: Reader, h: Header):

# Holds mesh data for continuous level of detail shapes.
# Pesumably a progressive mesh with triangles specified by edge splits.
# Seems to be specific to Freedom Force.
# The structure of this is uncertain and highly experimental at this point.
class NiClodData(NiTriBasedGeomData):
    unknownShorts: int
    unknownCount1: int
    unknownCount2: int
    unknownCount3: int
    unknownFloat: float
    unknownShort: int
    unknownClodShorts1: list[list[int]]
    unknownClodShorts2: list[int]
    unknownClodShorts3: list[list[int]]

    def __init__(self, r: Reader, h: Header):
        self.unknownShorts = r.readUInt16()
        self.unknownCount1 = r.readUInt16()
        self.unknownCount2 = r.readUInt16()
        self.unknownCount3 = r.readUInt16()
        self.unknownFloat = r.readSingle()
        self.unknownShort = r.readUInt16()
        self.unknownClodShorts1 = r.readFArray(lambda k: r.readPArray(None, 'H', UnknownCount1), 6)
        self.unknownClodShorts2 = r.readPArray(None, 'H', UnknownCount2)
        self.unknownClodShorts3 = r.readFArray(lambda k: r.readPArray(None, 'H', UnknownCount3), 6)

# DEPRECATED (pre-10.1), REMOVED (20.3).
# Time controller for texture coordinates.
class NiUVController(NiTimeController):
    unknownShort: int                                   # Always 0?
    data: int                                           # Texture coordinate controller data index.

    def __init__(self, r: Reader, h: Header):
        self.unknownShort = r.readUInt16()
        self.data = X[NiUVData].ref(r)

# DEPRECATED (pre-10.1), REMOVED (20.3)
# Texture coordinate data.
class NiUVData(NiObject):
    uvGroups: list[KeyGroup[T]]                         # Four UV data groups. Appear to be U translation, V translation, U scaling/tiling, V scaling/tiling.

    def __init__(self, r: Reader, h: Header):
        self.uvGroups = r.readFArray(lambda r: KeyGroup[T](r, h), 4)

# DEPRECATED (20.5).
# Extra data in the form of a vector (as x, y, z, w components).
class NiVectorExtraData(NiExtraData):
    vectorData: Vector4                                 # The vector data.

    def __init__(self, r: Reader, h: Header):
        self.vectorData = r.readVector4()

# Property of vertex colors. This object is referred to by the root object of the NIF file whenever some NiTriShapeData object has vertex colors with non-default settings; if not present, vertex colors have vertex_mode=2 and lighting_mode=1.
class NiVertexColorProperty(NiProperty):
    flags: Flags                                        # Bits 0-2: Unknown
                                                        #     Bit 3: Lighting Mode
                                                        #     Bits 4-5: Vertex Mode
    vertexMode: VertMode                                # In Flags from 20.1.0.3 on.
    lightingMode: LightMode                             # In Flags from 20.1.0.3 on.

    def __init__(self, r: Reader, h: Header):
        self.flags = Flags(r.readUInt16())
        self.vertexMode = if h.v <= 0x14000005: VertMode(r.readUInt32())
        self.lightingMode = if h.v <= 0x14000005: LightMode(r.readUInt32())

# DEPRECATED (10.x), REMOVED (?)
# Not used in skinning.
# Unsure of use - perhaps for morphing animation or gravity.
class NiVertWeightsExtraData(NiExtraData):
    numBytes: int                                       # Number of bytes in this data object.
    weight: list[float]                                 # The vertex weights.

    def __init__(self, r: Reader, h: Header):
        self.numBytes = r.readUInt32()
        self.weight = r.readL16PArray(None, 'f')

# DEPRECATED (10.2), REMOVED (?), Replaced by NiBoolData.
# Visibility data for a controller.
class NiVisData(NiObject):
    keys: list[Key[T]]

    def __init__(self, r: Reader, h: Header):
        self.keys = r.readL32FArray(lambda r: Key[T](r, h))

# Allows applications to switch between drawing solid geometry or wireframe outlines.
class NiWireframeProperty(NiProperty):
    flags: Flags                                        # Property flags.
                                                        #     0 - Wireframe Mode Disabled
                                                        #     1 - Wireframe Mode Enabled

    def __init__(self, r: Reader, h: Header):
        self.flags = Flags(r.readUInt16())

# Allows applications to set the test and write modes of the renderer's Z-buffer and to set the comparison function used for the Z-buffer test.
class NiZBufferProperty(NiProperty):
    flags: Flags                                        # Bit 0 enables the z test
                                                        #     Bit 1 controls wether the Z buffer is read only (0) or read/write (1)
    function: ZCompareMode                              # Z-Test function (see: glDepthFunc). In Flags from 20.1.0.3 on.

    def __init__(self, r: Reader, h: Header):
        self.flags = Flags(r.readUInt16())
        self.function = if h.v >= 0x0401000C and h.v <= 0x14000005: ZCompareMode(r.readUInt32())

# Morrowind-specific node for collision mesh.
class RootCollisionNode(NiNode):

    def __init__(self, r: Reader, h: Header):

# LEGACY (pre-10.1)
# Raw image data.
class NiRawImageData(NiObject):
    width: int                                          # Image width
    height: int                                         # Image height
    imageType: ImageType                                # The format of the raw image data.
    rgbImageData: list[list[ByteColor3]]                # Image pixel data.
    rgbaImageData: list[list[ByteColor4]]               # Image pixel data.

    def __init__(self, r: Reader, h: Header):
        self.width = r.readUInt32()
        self.height = r.readUInt32()
        self.imageType = ImageType(r.readUInt32())
        self.rgbImageData = if Image Type == 1: r.readFArray(lambda k: r.readFArray(lambda r: ByteColor3(r), Width), Height)
        self.rgbaImageData = if Image Type == 2: r.readFArray(lambda k: r.readFArray(lambda r: Color4Byte(r), Width), Height)

class NiAccumulator(NiObject):

    def __init__(self, r: Reader, h: Header):

# Used to turn sorting off for individual subtrees in a scene. Useful if objects must be drawn in a fixed order.
class NiSortAdjustNode(NiNode):
    sortingMode: SortingMode                            # Sorting
    accumulator: int

    def __init__(self, r: Reader, h: Header):
        self.sortingMode = SortingMode(r.readUInt32())
        self.accumulator = if h.v <= 0x14000003: X[NiAccumulator].ref(r)

# Represents cube maps that are created from either a set of six image files, six blocks of pixel data, or a single pixel data with six faces.
class NiSourceCubeMap(NiSourceTexture):

    def __init__(self, r: Reader, h: Header):

# A PhysX prop which holds information about PhysX actors in a Gamebryo scene
class NiPhysXProp(NiObjectNET):
    physXtoWorldScale: float
    sources: list[int]
    dests: list[int]
    modifiedMeshes: list[int]
    tempName: str
    keepMeshes: bool
    propDescription: int

    def __init__(self, r: Reader, h: Header):
        self.physXtoWorldScale = r.readSingle()
        self.sources = r.readL32FArray(X[NiObject].ref)
        self.dests = r.readL32FArray(X[NiPhysXDest].ref)
        self.modifiedMeshes = if h.v >= 0x14040000: r.readL32FArray(X[NiMesh].ref)
        self.tempName = if h.v >= 0x1E010002 and h.v <= 0x1E020002: Y.string(r)
        self.keepMeshes = r.readBool32()
        self.propDescription = X[NiPhysXPropDesc].ref(r)

class PhysXMaterialRef:
    def __init__(self, r: Reader):
        self.key: int = r.readUInt16()
        self.materialDesc: int = X[NiPhysXMaterialDesc].ref(r)

class PhysXStateName:
    def __init__(self, r: Reader):
        self.name: str = Y.string(r)
        self.index: int = r.readUInt32()

# For serialization of PhysX objects and to attach them to the scene.
class NiPhysXPropDesc(NiObject):
    actors: list[int]
    joints: list[int]
    clothes: list[int]
    materials: list[PhysXMaterialRef]
    numStates: int
    stateNames: list[PhysXStateName]
    flags: int

    def __init__(self, r: Reader, h: Header):
        self.actors = r.readL32FArray(X[NiPhysXActorDesc].ref)
        self.joints = r.readL32FArray(X[NiPhysXJointDesc].ref)
        self.clothes = if h.v >= 0x14030005: r.readL32FArray(X[NiObject].ref)
        self.materials = r.readL32FArray(lambda r: PhysXMaterialRef(r))
        self.numStates = r.readUInt32()
        self.stateNames = if h.v >= 0x14040000: r.readL32FArray(lambda r: PhysXStateName(r))
        self.flags = if h.v >= 0x14040000: r.readByte()

# For serializing NxActor objects.
class NiPhysXActorDesc(NiObject):
    actorName: str
    poses: list[Matrix3x4]
    bodyDesc: int
    density: float
    actorFlags: int
    actorGroup: int
    dominanceGroup: int
    contactReportFlags: int
    forceFieldMaterial: int
    dummy: int
    shapeDescriptions: list[int]
    actorParent: int
    source: int
    dest: int

    def __init__(self, r: Reader, h: Header):
        self.actorName = Y.string(r)
        self.poses = r.readL32FArray(lambda r: r.readL32Matrix3x4())
        self.bodyDesc = X[NiPhysXBodyDesc].ref(r)
        self.density = r.readSingle()
        self.actorFlags = r.readUInt32()
        self.actorGroup = r.readUInt16()
        self.dominanceGroup = if h.v >= 0x14040000: r.readUInt16()
        self.contactReportFlags = if h.v >= 0x14040000: r.readUInt32()
        self.forceFieldMaterial = if h.v >= 0x14040000: r.readUInt16()
        self.dummy = if h.v >= 0x14030001 and h.v <= 0x14030005: r.readUInt32()
        self.shapeDescriptions = r.readL32FArray(X[NiPhysXShapeDesc].ref)
        self.actorParent = X[NiPhysXActorDesc].ref(r)
        self.source = X[NiPhysXRigidBodySrc].ref(r)
        self.dest = X[NiPhysXRigidBodyDest].ref(r)

class PhysXBodyStoredVels:
    def __init__(self, r: Reader, h: Header):
        self.linearVelocity: Vector3 = r.readVector3()
        self.angularVelocity: Vector3 = r.readVector3()
        self.sleep: bool = r.readBool32() if h.v >= 0x1E020003 else None

# For serializing NxBodyDesc objects.
class NiPhysXBodyDesc(NiObject):
    localPose: Matrix3x4
    spaceInertia: Vector3
    mass: float
    vels: list[PhysXBodyStoredVels]
    wakeUpCounter: float
    linearDamping: float
    angularDamping: float
    maxAngularVelocity: float
    ccdMotionThreshold: float
    flags: int
    sleepLinearVelocity: float
    sleepAngularVelocity: float
    solverIterationCount: int
    sleepEnergyThreshold: float
    sleepDamping: float
    contactReportThreshold: float

    def __init__(self, r: Reader, h: Header):
        self.localPose = r.readMatrix3x4()
        self.spaceInertia = r.readVector3()
        self.mass = r.readSingle()
        self.vels = r.readL32FArray(lambda r: PhysXBodyStoredVels(r, h))
        self.wakeUpCounter = r.readSingle()
        self.linearDamping = r.readSingle()
        self.angularDamping = r.readSingle()
        self.maxAngularVelocity = r.readSingle()
        self.ccdMotionThreshold = r.readSingle()
        self.flags = r.readUInt32()
        self.sleepLinearVelocity = r.readSingle()
        self.sleepAngularVelocity = r.readSingle()
        self.solverIterationCount = r.readUInt32()
        self.sleepEnergyThreshold = if h.v >= 0x14030000: r.readSingle()
        self.sleepDamping = if h.v >= 0x14030000: r.readSingle()
        self.contactReportThreshold = if h.v >= 0x14040000: r.readSingle()

class NxJointType(Enum):
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

class NxD6JointMotion(Enum):
    NX_D6JOINT_MOTION_LOCKED = 0,
    NX_D6JOINT_MOTION_LIMITED = 1,
    NX_D6JOINT_MOTION_FREE = 2

class NxD6JointDriveType(Enum):
    NX_D6JOINT_DRIVE_POSITION = 1,
    NX_D6JOINT_DRIVE_VELOCITY = 2

class NxJointProjectionMode(Enum):
    NX_JPM_NONE = 0,
    NX_JPM_POINT_MINDIST = 1,
    NX_JPM_LINEAR_MINDIST = 2

class NiPhysXJointActor:
    def __init__(self, r: Reader):
        self.actor: int = X[NiPhysXActorDesc].ref(r)
        self.localNormal: Vector3 = r.readVector3()
        self.localAxis: Vector3 = r.readVector3()
        self.localAnchor: Vector3 = r.readVector3()

class NxJointLimitSoftDesc:
    def __init__(self, r: Reader):
        self.value: float = r.readSingle()
        self.restitution: float = r.readSingle()
        self.spring: float = r.readSingle()
        self.damping: float = r.readSingle()

class NxJointDriveDesc:
    def __init__(self, r: Reader):
        self.driveType: NxD6JointDriveType = NxD6JointDriveType(r.readUInt32())
        self.restitution: float = r.readSingle()
        self.spring: float = r.readSingle()
        self.damping: float = r.readSingle()

class NiPhysXJointLimit:
    def __init__(self, r: Reader, h: Header):
        self.limitPlaneNormal: Vector3 = r.readVector3()
        self.limitPlaneD: float = r.readSingle()
        self.limitPlaneR: float = r.readSingle() if h.v >= 0x14040000 else None

# A PhysX Joint abstract base class.
class NiPhysXJointDesc(NiObject):
    jointType: NxJointType
    jointName: str
    actors: list[NiPhysXJointActor]
    maxForce: float
    maxTorque: float
    solverExtrapolationFactor: float
    useAccelerationSpring: int
    jointFlags: int
    limitPoint: Vector3
    limits: list[NiPhysXJointLimit]

    def __init__(self, r: Reader, h: Header):
        self.jointType = NxJointType(r.readUInt32())
        self.jointName = Y.string(r)
        self.actors = r.readFArray(lambda r: NiPhysXJointActor(r), 2)
        self.maxForce = r.readSingle()
        self.maxTorque = r.readSingle()
        self.solverExtrapolationFactor = if h.v >= 0x14050003: r.readSingle()
        self.useAccelerationSpring = if h.v >= 0x14050003: r.readUInt32()
        self.jointFlags = r.readUInt32()
        self.limitPoint = r.readVector3()
        self.limits = r.readL32FArray(lambda r: NiPhysXJointLimit(r, h))

# A 6DOF (6 degrees of freedom) joint.
class NiPhysXD6JointDesc(NiPhysXJointDesc):
    xMotion: NxD6JointMotion
    yMotion: NxD6JointMotion
    zMotion: NxD6JointMotion
    swing1Motion: NxD6JointMotion
    swing2Motion: NxD6JointMotion
    twistMotion: NxD6JointMotion
    linearLimit: NxJointLimitSoftDesc
    swing1Limit: NxJointLimitSoftDesc
    swing2Limit: NxJointLimitSoftDesc
    twistLowLimit: NxJointLimitSoftDesc
    twistHighLimit: NxJointLimitSoftDesc
    xDrive: NxJointDriveDesc
    yDrive: NxJointDriveDesc
    zDrive: NxJointDriveDesc
    swingDrive: NxJointDriveDesc
    twistDrive: NxJointDriveDesc
    slerpDrive: NxJointDriveDesc
    drivePosition: Vector3
    driveOrientation: Quaternion
    driveLinearVelocity: Vector3
    driveAngularVelocity: Vector3
    projectionMode: NxJointProjectionMode
    projectionDistance: float
    projectionAngle: float
    gearRatio: float
    flags: int

    def __init__(self, r: Reader, h: Header):
        self.xMotion = NxD6JointMotion(r.readUInt32())
        self.yMotion = NxD6JointMotion(r.readUInt32())
        self.zMotion = NxD6JointMotion(r.readUInt32())
        self.swing1Motion = NxD6JointMotion(r.readUInt32())
        self.swing2Motion = NxD6JointMotion(r.readUInt32())
        self.twistMotion = NxD6JointMotion(r.readUInt32())
        self.linearLimit = NxJointLimitSoftDesc(r)
        self.swing1Limit = NxJointLimitSoftDesc(r)
        self.swing2Limit = NxJointLimitSoftDesc(r)
        self.twistLowLimit = NxJointLimitSoftDesc(r)
        self.twistHighLimit = NxJointLimitSoftDesc(r)
        self.xDrive = NxJointDriveDesc(r)
        self.yDrive = NxJointDriveDesc(r)
        self.zDrive = NxJointDriveDesc(r)
        self.swingDrive = NxJointDriveDesc(r)
        self.twistDrive = NxJointDriveDesc(r)
        self.slerpDrive = NxJointDriveDesc(r)
        self.drivePosition = r.readVector3()
        self.driveOrientation = r.readQuaternion()
        self.driveLinearVelocity = r.readVector3()
        self.driveAngularVelocity = r.readVector3()
        self.projectionMode = NxJointProjectionMode(r.readUInt32())
        self.projectionDistance = r.readSingle()
        self.projectionAngle = r.readSingle()
        self.gearRatio = r.readSingle()
        self.flags = r.readUInt32()

class NxShapeType(Enum):
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

class NxPlane:
    def __init__(self, r: Reader):
        self.val1: float = r.readSingle()
        self.point1: Vector3 = r.readVector3()

class NxCapsule:
    def __init__(self, r: Reader):
        self.val1: float = r.readSingle()
        self.val2: float = r.readSingle()
        self.capsuleFlags: int = r.readUInt32()

# For serializing NxShapeDesc objects
class NiPhysXShapeDesc(NiObject):
    shapeType: NxShapeType
    localPose: Matrix3x4
    shapeFlags: int
    collisionGroup: int
    materialIndex: int
    density: float
    mass: float
    skinWidth: float
    shapeName: str
    nonInteractingCompartmentTypes: int
    collisionBits: list[int]
    plane: NxPlane
    sphereRadius: float
    boxHalfExtents: Vector3
    capsule: NxCapsule
    mesh: int

    def __init__(self, r: Reader, h: Header):
        self.shapeType = NxShapeType(r.readUInt32())
        self.localPose = r.readMatrix3x4()
        self.shapeFlags = r.readUInt32()
        self.collisionGroup = r.readUInt16()
        self.materialIndex = r.readUInt16()
        self.density = r.readSingle()
        self.mass = r.readSingle()
        self.skinWidth = r.readSingle()
        self.shapeName = Y.string(r)
        self.nonInteractingCompartmentTypes = if h.v >= 0x14040000: r.readUInt32()
        self.collisionBits = r.readPArray(None, 'I', 4)
        self.plane = if Shape Type == 0: NxPlane(r)
        self.sphereRadius = if Shape Type == 1: r.readSingle()
        self.boxHalfExtents = if Shape Type == 2: r.readVector3()
        self.capsule = if Shape Type == 3: NxCapsule(r)
        self.mesh = if (Shape Type == 5) or (Shape Type == 6): X[NiPhysXMeshDesc].ref(r)

# Holds mesh data for streaming.
class NiPhysXMeshDesc(NiObject):
    isConvex: bool
    meshName: str
    meshData: list[int]
    meshSize: int
    meshFlags: int
    meshPagingMode: int
    isHardware: bool
    flags: int

    def __init__(self, r: Reader, h: Header):
        self.isConvex = if h.v <= 0x14030004: r.readBool32()
        self.meshName = Y.string(r)
        self.meshData = r.readL8Bytes()
        self.meshSize = if h.v >= 0x14030005 and h.v <= 0x1E020002: r.readUInt16()
        self.meshData = if h.v >= 0x14030005 and h.v <= 0x1E020002: r.readPArray(None, 'H', MeshSize)
        self.meshFlags = r.readUInt32()
        self.meshPagingMode = if h.v >= 0x14030001: r.readUInt32()
        self.isHardware = if h.v >= 0x14030002 and h.v <= 0x14030004: r.readBool32()
        self.flags = if h.v >= 0x14030005: r.readByte()

class NxMaterialFlag(Flag):
    NX_MF_ANISOTROPIC = 1 << 1,
    NX_MF_DUMMY1 = 1 << 2,
    NX_MF_DUMMY2 = 1 << 3,
    NX_MF_DUMMY3 = 1 << 4,
    NX_MF_DISABLE_FRICTION = 1 << 5,
    NX_MF_DISABLE_STRONG_FRICTION = 1 << 6

class NxSpringDesc:
    def __init__(self, r: Reader):
        self.spring: float = r.readSingle()
        self.damper: float = r.readSingle()
        self.targetValue: float = r.readSingle()

class NxCombineMode(Enum):
    NX_CM_AVERAGE = 0,
    NX_CM_MIN = 1,
    NX_CM_MULTIPLY = 2,
    NX_CM_MAX = 3

class NxMaterialDesc:
    def __init__(self, r: Reader, h: Header):
        self.dynamicFriction: float = r.readSingle()
        self.staticFriction: float = r.readSingle()
        self.restitution: float = r.readSingle()
        self.dynamicFrictionV: float = r.readSingle()
        self.staticFrictionV: float = r.readSingle()
        self.directionofAnisotropy: Vector3 = r.readVector3()
        self.flags: NxMaterialFlag = NxMaterialFlag(r.readUInt32())
        self.frictionCombineMode: NxCombineMode = NxCombineMode(r.readUInt32())
        self.restitutionCombineMode: NxCombineMode = NxCombineMode(r.readUInt32())
        self.hasSpring: bool = r.readBool32() if h.v <= 0x14020300 else None
        self.spring: NxSpringDesc = NxSpringDesc(r) if Has Spring and h.v <= 0x14020300 else None

# For serializing NxMaterialDesc objects.
class NiPhysXMaterialDesc(NiObject):
    index: int
    materialDescs: list[NxMaterialDesc]

    def __init__(self, r: Reader, h: Header):
        self.index = r.readUInt16()
        self.materialDescs = r.readL32FArray(lambda r: NxMaterialDesc(r, h))

# A destination is a link between a PhysX actor and a Gamebryo object being driven by the physics.
class NiPhysXDest(NiObject):
    active: bool
    interpolate: bool

    def __init__(self, r: Reader, h: Header):
        self.active = r.readBool32()
        self.interpolate = r.readBool32()

# Base for destinations that set a rigid body state.
class NiPhysXRigidBodyDest(NiPhysXDest):

    def __init__(self, r: Reader, h: Header):

# Connects PhysX rigid body actors to a scene node.
class NiPhysXTransformDest(NiPhysXRigidBodyDest):
    target: int

    def __init__(self, r: Reader, h: Header):
        self.target = X[NiAVObject].ptr(r)

# A source is a link between a Gamebryo object and a PhysX actor.
class NiPhysXSrc(NiObject):
    active: bool
    interpolate: bool

    def __init__(self, r: Reader, h: Header):
        self.active = r.readBool32()
        self.interpolate = r.readBool32()

# Sets state of a rigid body PhysX actor.
class NiPhysXRigidBodySrc(NiPhysXSrc):
    source: int

    def __init__(self, r: Reader, h: Header):
        self.source = X[NiAVObject].ptr(r)

# Sets state of kinematic PhysX actor.
class NiPhysXKinematicSrc(NiPhysXRigidBodySrc):

    def __init__(self, r: Reader, h: Header):

# Sends Gamebryo scene state to a PhysX dynamic actor.
class NiPhysXDynamicSrc(NiPhysXRigidBodySrc):

    def __init__(self, r: Reader, h: Header):

# Wireframe geometry.
class NiLines(NiTriBasedGeom):

    def __init__(self, r: Reader, h: Header):

# Wireframe geometry data.
class NiLinesData(NiGeometryData):
    lines: list[bool]                                   # Is vertex connected to other (next?) vertex?

    def __init__(self, r: Reader, h: Header):
        self.lines = r.readBytes(NumVertices)

# Two dimensional screen elements.
class Polygon:
    def __init__(self, r: Reader):
        self.numVertices: int = r.readUInt16()
        self.vertexOffset: int = r.readUInt16()         # Offset in vertex array.
        self.numTriangles: int = r.readUInt16()
        self.triangleOffset: int = r.readUInt16()       # Offset in indices array.

# DEPRECATED (20.5), functionality included in NiMeshScreenElements.
# Two dimensional screen elements.
class NiScreenElementsData(NiTriShapeData):
    maxPolygons: int
    polygons: list[Polygon]
    polygonIndices: list[int]
    polygonGrowBy: int
    numPolygons: int
    maxVertices: int
    verticesGrowBy: int
    maxIndices: int
    indicesGrowBy: int

    def __init__(self, r: Reader, h: Header):
        self.maxPolygons = r.readUInt16()
        self.polygons = r.readFArray(lambda r: Polygon(r), MaxPolygons)
        self.polygonIndices = r.readPArray(None, 'H', MaxPolygons)
        self.polygonGrowBy = r.readUInt16()
        self.numPolygons = r.readUInt16()
        self.maxVertices = r.readUInt16()
        self.verticesGrowBy = r.readUInt16()
        self.maxIndices = r.readUInt16()
        self.indicesGrowBy = r.readUInt16()

# DEPRECATED (20.5), replaced by NiMeshScreenElements.
# Two dimensional screen elements.
class NiScreenElements(NiTriShape):

    def __init__(self, r: Reader, h: Header):

# NiRoomGroup represents a set of connected rooms i.e. a game level.
class NiRoomGroup(NiNode):
    shell: int                                          # Object that represents the room group as seen from the outside.
    rooms: list[int]

    def __init__(self, r: Reader, h: Header):
        self.shell = X[NiNode].ptr(r)
        self.rooms = r.readL32FArray(X[NiRoom].ptr)

# NiRoom objects represent cells in a cell-portal culling system.
class NiRoom(NiNode):
    wallPlanes: list[NiPlane]
    inPortals: list[int]                                # The portals which see into the room.
    outPortals: list[int]                               # The portals which see out of the room.
    fixtures: list[int]                                 # All geometry associated with the room.

    def __init__(self, r: Reader, h: Header):
        self.wallPlanes = r.readL32FArray(lambda r: NiPlane(r))
        self.inPortals = r.readL32FArray(X[NiPortal].ptr)
        self.outPortals = r.readL32FArray(X[NiPortal].ptr)
        self.fixtures = r.readL32FArray(X[NiAVObject].ptr)

# NiPortal objects are grouping nodes that support aggressive visibility culling.
# They represent flat polygonal regions through which a part of a scene graph can be viewed.
class NiPortal(NiAVObject):
    portalFlags: int
    planeCount: int                                     # Unused in 20.x, possibly also 10.x.
    vertices: list[Vector3]
    adjoiner: int                                       # Root of the scenegraph which is to be seen through this portal.

    def __init__(self, r: Reader, h: Header):
        self.portalFlags = r.readUInt16()
        self.planeCount = r.readUInt16()
        self.vertices = r.readL16FArray(lambda r: r.readL16Vector3())
        self.adjoiner = X[NiNode].ptr(r)

# Bethesda-specific fade node.
class BSFadeNode(NiNode):

    def __init__(self, r: Reader, h: Header):

# The type of animation interpolation (blending) that will be used on the associated key frames.
class BSShaderType(Enum):
    SHADER_TALL_GRASS = 0,          # Tall Grass Shader
    SHADER_DEFAULT = 1,             # Standard Lighting Shader
    SHADER_SKY = 10,                # Sky Shader
    SHADER_SKIN = 14,               # Skin Shader
    SHADER_WATER = 17,              # Water Shader
    SHADER_LIGHTING30 = 29,         # Lighting 3.0 Shader
    SHADER_TILE = 32,               # Tiled Shader
    SHADER_NOLIGHTING = 33          # No Lighting Shader

# Shader Property Flags
class BSShaderFlags(Flag):
    Specular = 0,                   # Enables Specularity
    Skinned = 1 << 1,               # Required For Skinned Meshes
    LowDetail = 1 << 2,             # Lowddetail (seems to use standard diff/norm/spec shader)
    Vertex_Alpha = 1 << 3,          # Vertex Alpha
    Unknown_1 = 1 << 4,             # Unknown
    Single_Pass = 1 << 5,           # Single Pass
    Empty = 1 << 6,                 # Unknown
    Environment_Mapping = 1 << 7,   # Environment mapping (uses Envmap Scale)
    Alpha_Texture = 1 << 8,         # Alpha Texture Requires NiAlphaProperty to Enable
    Unknown_2 = 1 << 9,             # Unknown
    FaceGen = 1 << 10,              # FaceGen
    Parallax_Shader_Index_15 = 1 << 11, # Parallax
    Unknown_3 = 1 << 12,            # Unknown/Crash
    Non_Projective_Shadows = 1 << 13, # Non-Projective Shadows
    Unknown_4 = 1 << 14,            # Unknown/Crash
    Refraction = 1 << 15,           # Refraction (switches on refraction power)
    Fire_Refraction = 1 << 16,      # Fire Refraction (switches on refraction power/period)
    Eye_Environment_Mapping = 1 << 17, # Eye Environment Mapping (does not use envmap light fade or envmap scale)
    Hair = 1 << 18,                 # Hair
    Dynamic_Alpha = 1 << 19,        # Dynamic Alpha
    Localmap_Hide_Secret = 1 << 20, # Localmap Hide Secret
    Window_Environment_Mapping = 1 << 21, # Window Environment Mapping
    Tree_Billboard = 1 << 22,       # Tree Billboard
    Shadow_Frustum = 1 << 23,       # Shadow Frustum
    Multiple_Textures = 1 << 24,    # Multiple Textures (base diff/norm become null)
    Remappable_Textures = 1 << 25,  # usually seen w/texture animation
    Decal_Single_Pass = 1 << 26,    # Decal
    Dynamic_Decal_Single_Pass = 1 << 27, # Dynamic Decal
    Parallax_Occulsion = 1 << 28,   # Parallax Occlusion
    External_Emittance = 1 << 29,   # External Emittance
    Shadow_Map = 1 << 30,           # Shadow Map
    ZBuffer_Test = 1 << 31          # ZBuffer Test (1=on)

# Shader Property Flags 2
class BSShaderFlags2(Flag):
    ZBuffer_Write = 0,              # ZBuffer Write
    LOD_Landscape = 1 << 1,         # LOD Landscape
    LOD_Building = 1 << 2,          # LOD Building
    No_Fade = 1 << 3,               # No Fade
    Refraction_Tint = 1 << 4,       # Refraction Tint
    Vertex_Colors = 1 << 5,         # Has Vertex Colors
    Unknown1 = 1 << 6,              # Unknown
    1st_Light_is_Point_Light = 1 << 7, # 1st Light is Point Light
    2nd_Light = 1 << 8,             # 2nd Light
    3rd_Light = 1 << 9,             # 3rd Light
    Vertex_Lighting = 1 << 10,      # Vertex Lighting
    Uniform_Scale = 1 << 11,        # Uniform Scale
    Fit_Slope = 1 << 12,            # Fit Slope
    Billboard_and_Envmap_Light_Fade = 1 << 13, # Billboard and Envmap Light Fade
    No_LOD_Land_Blend = 1 << 14,    # No LOD Land Blend
    Envmap_Light_Fade = 1 << 15,    # Envmap Light Fade
    Wireframe = 1 << 16,            # Wireframe
    VATS_Selection = 1 << 17,       # VATS Selection
    Show_in_Local_Map = 1 << 18,    # Show in Local Map
    Premult_Alpha = 1 << 19,        # Premult Alpha
    Skip_Normal_Maps = 1 << 20,     # Skip Normal Maps
    Alpha_Decal = 1 << 21,          # Alpha Decal
    No_Transparecny_Multisampling = 1 << 22, # No Transparency MultiSampling
    Unknown2 = 1 << 23,             # Unknown
    Unknown3 = 1 << 24,             # Unknown
    Unknown4 = 1 << 25,             # Unknown
    Unknown5 = 1 << 26,             # Unknown
    Unknown6 = 1 << 27,             # Unknown
    Unknown7 = 1 << 28,             # Unknown
    Unknown8 = 1 << 29,             # Unknown
    Unknown9 = 1 << 30,             # Unknown
    Unknown10 = 1 << 31             # Unknown

# Bethesda-specific property.
class BSShaderProperty(NiShadeProperty):
    shaderType: BSShaderType
    shaderFlags: BSShaderFlags
    shaderFlags2: BSShaderFlags2
    environmentMapScale: float                          # Scales the intensity of the environment/cube map.

    def __init__(self, r: Reader, h: Header):
        self.shaderType = BSShaderType(r.readUInt32())
        self.shaderFlags = BSShaderFlags(r.readUInt32())
        self.shaderFlags2 = BSShaderFlags2(r.readUInt32())
        self.environmentMapScale = r.readSingle()

# Bethesda-specific property.
class BSShaderLightingProperty(BSShaderProperty):
    textureClampMode: TexClampMode                      # How to handle texture borders.

    def __init__(self, r: Reader, h: Header):
        self.textureClampMode = TexClampMode(r.readUInt32())

# Bethesda-specific property.
class BSShaderNoLightingProperty(BSShaderLightingProperty):
    fileName: str                                       # The texture glow map.
    falloffStartAngle: float                            # At this cosine of angle falloff will be equal to Falloff Start Opacity
    falloffStopAngle: float                             # At this cosine of angle falloff will be equal to Falloff Stop Opacity
    falloffStartOpacity: float                          # Alpha falloff multiplier at start angle
    falloffStopOpacity: float                           # Alpha falloff multiplier at end angle

    def __init__(self, r: Reader, h: Header):
        self.fileName = r.readL32AString()
        self.falloffStartAngle = r.readSingle()
        self.falloffStopAngle = r.readSingle()
        self.falloffStartOpacity = r.readSingle()
        self.falloffStopOpacity = r.readSingle()

# Bethesda-specific property.
class BSShaderPPLightingProperty(BSShaderLightingProperty):
    textureSet: int                                     # Texture Set
    refractionStrength: float                           # The amount of distortion. **Not based on physically accurate refractive index** (0=none) (0-1)
    refractionFirePeriod: int                           # Rate of texture movement for refraction shader.
    parallaxMaxPasses: float                            # The number of passes the parallax shader can apply.
    parallaxScale: float                                # The strength of the parallax.
    emissiveColor: Color4                               # Glow color and alpha

    def __init__(self, r: Reader, h: Header):
        self.textureSet = X[BSShaderTextureSet].ref(r)
        self.refractionStrength = r.readSingle()
        self.refractionFirePeriod = r.readInt32()
        self.parallaxMaxPasses = r.readSingle()
        self.parallaxScale = r.readSingle()
        self.emissiveColor = Color4(r)

# This controller is used to animate float variables in BSEffectShaderProperty.
class BSEffectShaderPropertyFloatController(NiFloatInterpController):
    typeofControlledVariable: EffectShaderControlledVariable # Which float variable in BSEffectShaderProperty to animate:

    def __init__(self, r: Reader, h: Header):
        self.typeofControlledVariable = EffectShaderControlledVariable(r.readUInt32())

# This controller is used to animate colors in BSEffectShaderProperty.
class BSEffectShaderPropertyColorController(NiPoint3InterpController):
    typeofControlledColor: EffectShaderControlledColor  # Which color in BSEffectShaderProperty to animate:

    def __init__(self, r: Reader, h: Header):
        self.typeofControlledColor = EffectShaderControlledColor(r.readUInt32())

# This controller is used to animate float variables in BSLightingShaderProperty.
class BSLightingShaderPropertyFloatController(NiFloatInterpController):
    typeofControlledVariable: LightingShaderControlledVariable # Which float variable in BSLightingShaderProperty to animate:

    def __init__(self, r: Reader, h: Header):
        self.typeofControlledVariable = LightingShaderControlledVariable(r.readUInt32())

# This controller is used to animate colors in BSLightingShaderProperty.
class BSLightingShaderPropertyColorController(NiPoint3InterpController):
    typeofControlledColor: LightingShaderControlledColor# Which color in BSLightingShaderProperty to animate:

    def __init__(self, r: Reader, h: Header):
        self.typeofControlledColor = LightingShaderControlledColor(r.readUInt32())

class BSNiAlphaPropertyTestRefController(NiFloatInterpController):

    def __init__(self, r: Reader, h: Header):

# Skyrim, Paired with dummy TriShapes, this controller generates lightning shapes for special effects.
#     First interpolator controls Generation.
class BSProceduralLightningController(NiTimeController):
    interpolator1Generation: int                        # References generation interpolator.
    interpolator2Mutation: int                          # References interpolator for Mutation of strips
    interpolator3Subdivision: int                       # References subdivision interpolator.
    interpolator4NumBranches: int                       # References branches interpolator.
    interpolator5NumBranchesVar: int                    # References branches variation interpolator.
    interpolator6Length: int                            # References length interpolator.
    interpolator7LengthVar: int                         # References length variation interpolator.
    interpolator8Width: int                             # References width interpolator.
    interpolator9ArcOffset: int                         # References interpolator for amplitude control. 0=straight, 50=wide
    subdivisions: int
    numBranches: int
    numBranchesVariation: int
    length: float                                       # How far lightning will stretch to.
    lengthVariation: float                              # How far lightning variation will stretch to.
    width: float                                        # How wide the bolt will be.
    childWidthMult: float                               # Influences forking behavior with a multiplier.
    arcOffset: float
    fadeMainBolt: bool
    fadeChildBolts: bool
    animateArcOffset: bool
    shaderProperty: int                                 # Reference to a shader property.

    def __init__(self, r: Reader, h: Header):
        self.interpolator1Generation = X[NiInterpolator].ref(r)
        self.interpolator2Mutation = X[NiInterpolator].ref(r)
        self.interpolator3Subdivision = X[NiInterpolator].ref(r)
        self.interpolator4NumBranches = X[NiInterpolator].ref(r)
        self.interpolator5NumBranchesVar = X[NiInterpolator].ref(r)
        self.interpolator6Length = X[NiInterpolator].ref(r)
        self.interpolator7LengthVar = X[NiInterpolator].ref(r)
        self.interpolator8Width = X[NiInterpolator].ref(r)
        self.interpolator9ArcOffset = X[NiInterpolator].ref(r)
        self.subdivisions = r.readUInt16()
        self.numBranches = r.readUInt16()
        self.numBranchesVariation = r.readUInt16()
        self.length = r.readSingle()
        self.lengthVariation = r.readSingle()
        self.width = r.readSingle()
        self.childWidthMult = r.readSingle()
        self.arcOffset = r.readSingle()
        self.fadeMainBolt = r.readBool32()
        self.fadeChildBolts = r.readBool32()
        self.animateArcOffset = r.readBool32()
        self.shaderProperty = X[BSShaderProperty].ref(r)

# Bethesda-specific Texture Set.
class BSShaderTextureSet(NiObject):
    textures: list[str]                                 # Textures.
                                                        #     0: Diffuse
                                                        #     1: Normal/Gloss
                                                        #     2: Glow(SLSF2_Glow_Map)/Skin/Hair/Rim light(SLSF2_Rim_Lighting)
                                                        #     3: Height/Parallax
                                                        #     4: Environment
                                                        #     5: Environment Mask
                                                        #     6: Subsurface for Multilayer Parallax
                                                        #     7: Back Lighting Map (SLSF2_Back_Lighting)

    def __init__(self, r: Reader, h: Header):
        self.textures = r.readL32FArray(lambda r: SizedString(r))

# Bethesda-specific property. Found in Fallout3
class WaterShaderProperty(BSShaderProperty):

    def __init__(self, r: Reader, h: Header):

# Sets what sky function this object fulfills in BSSkyShaderProperty or SkyShaderProperty.
class SkyObjectType(Enum):
    BSSM_SKY_TEXTURE = 0,           # BSSM_Sky_Texture
    BSSM_SKY_SUNGLARE = 1,          # BSSM_Sky_Sunglare
    BSSM_SKY = 2,                   # BSSM_Sky
    BSSM_SKY_CLOUDS = 3,            # BSSM_Sky_Clouds
    BSSM_SKY_STARS = 5,             # BSSM_Sky_Stars
    BSSM_SKY_MOON_STARS_MASK = 7    # BSSM_Sky_Moon_Stars_Mask

# Bethesda-specific property. Found in Fallout3
class SkyShaderProperty(BSShaderLightingProperty):
    fileName: str                                       # The texture.
    skyObjectType: SkyObjectType                        # Sky Object Type

    def __init__(self, r: Reader, h: Header):
        self.fileName = r.readL32AString()
        self.skyObjectType = SkyObjectType(r.readUInt32())

# Bethesda-specific property.
class TileShaderProperty(BSShaderLightingProperty):
    fileName: str                                       # Texture file name

    def __init__(self, r: Reader, h: Header):
        self.fileName = r.readL32AString()

# Bethesda-specific property.
class DistantLODShaderProperty(BSShaderProperty):

    def __init__(self, r: Reader, h: Header):

# Bethesda-specific property.
class BSDistantTreeShaderProperty(BSShaderProperty):

    def __init__(self, r: Reader, h: Header):

# Bethesda-specific property.
class TallGrassShaderProperty(BSShaderProperty):
    fileName: str                                       # Texture file name

    def __init__(self, r: Reader, h: Header):
        self.fileName = r.readL32AString()

# Bethesda-specific property.
class VolumetricFogShaderProperty(BSShaderProperty):

    def __init__(self, r: Reader, h: Header):

# Bethesda-specific property.
class HairShaderProperty(BSShaderProperty):

    def __init__(self, r: Reader, h: Header):

# Bethesda-specific property.
class Lighting30ShaderProperty(BSShaderPPLightingProperty):

    def __init__(self, r: Reader, h: Header):

# Skyrim Shader Property Flags 1
class SkyrimShaderPropertyFlags1(Flag):
    Specular = 0,                   # Enables Specularity
    Skinned = 1 << 1,               # Required For Skinned Meshes.
    Temp_Refraction = 1 << 2,
    Vertex_Alpha = 1 << 3,          # Enables using alpha component of vertex colors.
    Greyscale_To_PaletteColor = 1 << 4, # in EffectShaderProperty
    Greyscale_To_PaletteAlpha = 1 << 5, # in EffectShaderProperty
    Use_Falloff = 1 << 6,           # Use Falloff value in EffectShaderProperty
    Environment_Mapping = 1 << 7,   # Environment mapping (uses Envmap Scale).
    Recieve_Shadows = 1 << 8,       # Object can recieve shadows.
    Cast_Shadows = 1 << 9,          # Can cast shadows
    Facegen_Detail_Map = 1 << 10,   # Use a face detail map in the 4th texture slot.
    Parallax = 1 << 11,             # Unused?
    Model_Space_Normals = 1 << 12,  # Use Model space normals and an external Specular Map.
    Non_Projective_Shadows = 1 << 13,
    Landscape = 1 << 14,
    Refraction = 1 << 15,           # Use normal map for refraction effect.
    Fire_Refraction = 1 << 16,
    Eye_Environment_Mapping = 1 << 17, # Eye Environment Mapping (Must use the Eye shader and the model must be skinned)
    Hair_Soft_Lighting = 1 << 18,   # Keeps from going too bright under lights (hair shader only)
    Screendoor_Alpha_Fade = 1 << 19,
    Localmap_Hide_Secret = 1 << 20, # Object and anything it is positioned above will not render on local map view.
    FaceGen_RGB_Tint = 1 << 21,     # Use tintmask for Face.
    Own_Emit = 1 << 22,             # Provides its own emittance color. (will not absorb light/ambient color?)
    Projected_UV = 1 << 23,         # Used for decalling?
    Multiple_Textures = 1 << 24,
    Remappable_Textures = 1 << 25,
    Decal = 1 << 26,
    Dynamic_Decal = 1 << 27,
    Parallax_Occlusion = 1 << 28,
    External_Emittance = 1 << 29,
    Soft_Effect = 1 << 30,
    ZBuffer_Test = 1 << 31          # ZBuffer Test (1=on)

# Skyrim Shader Property Flags 2
class SkyrimShaderPropertyFlags2(Flag):
    ZBuffer_Write = 0,              # Enables writing to the Z-Buffer
    LOD_Landscape = 1 << 1,
    LOD_Objects = 1 << 2,
    No_Fade = 1 << 3,
    Double_Sided = 1 << 4,          # Double-sided rendering.
    Vertex_Colors = 1 << 5,         # Has Vertex Colors.
    Glow_Map = 1 << 6,              # Use Glow Map in the third texture slot.
    Assume_Shadowmask = 1 << 7,
    Packed_Tangent = 1 << 8,
    Multi_Index_Snow = 1 << 9,
    Vertex_Lighting = 1 << 10,
    Uniform_Scale = 1 << 11,
    Fit_Slope = 1 << 12,
    Billboard = 1 << 13,
    No_LOD_Land_Blend = 1 << 14,
    EnvMap_Light_Fade = 1 << 15,
    Wireframe = 1 << 16,            # Wireframe (Seems to only work on particles)
    Weapon_Blood = 1 << 17,         # Used for blood decals on weapons.
    Hide_On_Local_Map = 1 << 18,    # Similar to hide secret, but only for self?
    Premult_Alpha = 1 << 19,        # Has Premultiplied Alpha
    Cloud_LOD = 1 << 20,
    Anisotropic_Lighting = 1 << 21, # Hair only?
    No_Transparency_Multisampling = 1 << 22,
    Unused01 = 1 << 23,             # Unused?
    Multi_Layer_Parallax = 1 << 24, # Use Multilayer (inner-layer) Map
    Soft_Lighting = 1 << 25,        # Use Soft Lighting Map
    Rim_Lighting = 1 << 26,         # Use Rim Lighting Map
    Back_Lighting = 1 << 27,        # Use Back Lighting Map
    Unused02 = 1 << 28,             # Unused?
    Tree_Anim = 1 << 29,            # Enables Vertex Animation, Flutter Animation
    Effect_Lighting = 1 << 30,
    HD_LOD_Objects = 1 << 31

# Fallout 4 Shader Property Flags 1
class Fallout4ShaderPropertyFlags1(Flag):
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

# Fallout 4 Shader Property Flags 2
class Fallout4ShaderPropertyFlags2(Flag):
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

# Bethesda shader property for Skyrim and later.
class BSLightingShaderProperty(BSShaderProperty):
    shaderFlags1: Fallout4ShaderPropertyFlags1          # Fallout 4 Shader Flags. Mostly overridden if "Name" is a path to a BGSM/BGEM file.
    shaderFlags2: Fallout4ShaderPropertyFlags2          # Fallout 4 Shader Flags. Mostly overridden if "Name" is a path to a BGSM/BGEM file.
    uvOffset: TexCoord                                  # Offset UVs
    uvScale: TexCoord                                   # Offset UV Scale to repeat tiling textures, see above.
    textureSet: int                                     # Texture Set, can have override in an esm/esp
    emissiveColor: Color3                               # Glow color and alpha
    emissiveMultiple: float                             # Multiplied emissive colors
    wetMaterial: str
    textureClampMode: TexClampMode                      # How to handle texture borders.
    alpha: float                                        # The material opacity (1=non-transparent).
    refractionStrength: float                           # The amount of distortion. **Not based on physically accurate refractive index** (0=none) (0-1)
    glossiness: float                                   # The material specular power, or glossiness (0-999).
    smoothness: float                                   # The base roughness (0.0-1.0), multiplied by the smoothness map.
    specularColor: Color3                               # Adds a colored highlight.
    specularStrength: float                             # Brightness of specular highlight. (0=not visible) (0-999)
    lightingEffect1: float                              # Controls strength for envmap/backlight/rim/softlight lighting effect?
    lightingEffect2: float                              # Controls strength for envmap/backlight/rim/softlight lighting effect?
    subsurfaceRolloff: float
    rimlightPower: float
    backlightPower: float
    grayscaletoPaletteScale: float
    fresnelPower: float
    wetnessSpecScale: float
    wetnessSpecPower: float
    wetnessMinVar: float
    wetnessEnvMapScale: float
    wetnessFresnelPower: float
    wetnessMetalness: float
    environmentMapScale: float                          # Scales the intensity of the environment/cube map. (0-1)
    unknownEnvMapShort: int
    skinTintColor: Color3                               # Tints the base texture. Overridden by game settings.
    unknownSkinTintInt: int
    hairTintColor: Color3                               # Tints the base texture. Overridden by game settings.
    maxPasses: float                                    # Max Passes
    scale: float                                        # Scale
    parallaxInnerLayerThickness: float                  # How far from the surface the inner layer appears to be.
    parallaxRefractionScale: float                      # Depth of inner parallax layer effect.
    parallaxInnerLayerTextureScale: TexCoord            # Scales the inner parallax layer texture.
    parallaxEnvmapStrength: float                       # How strong the environment/cube map is. (0-??)
    sparkleParameters: Vector4                          # CK lists "snow material" when used.
    eyeCubemapScale: float                              # Eye cubemap scale
    leftEyeReflectionCenter: Vector3                    # Offset to set center for left eye cubemap
    rightEyeReflectionCenter: Vector3                   # Offset to set center for right eye cubemap

    def __init__(self, r: Reader, h: Header):
        self.shaderFlags1 = SkyrimShaderPropertyFlags1(r.readUInt32())
        self.shaderFlags2 = SkyrimShaderPropertyFlags2(r.readUInt32())
        self.shaderFlags1 = Fallout4ShaderPropertyFlags1(r.readUInt32())
        self.shaderFlags2 = Fallout4ShaderPropertyFlags2(r.readUInt32())
        self.uvOffset = TexCoord(r)
        self.uvScale = TexCoord(r)
        self.textureSet = X[BSShaderTextureSet].ref(r)
        self.emissiveColor = Color3(r)
        self.emissiveMultiple = r.readSingle()
        self.wetMaterial = Y.string(r)
        self.textureClampMode = TexClampMode(r.readUInt32())
        self.alpha = r.readSingle()
        self.refractionStrength = r.readSingle()
        self.glossiness = r.readSingle()
        self.smoothness = r.readSingle()
        self.specularColor = Color3(r)
        self.specularStrength = r.readSingle()
        self.lightingEffect1 = r.readSingle()
        self.lightingEffect2 = r.readSingle()
        self.subsurfaceRolloff = r.readSingle()
        self.rimlightPower = r.readSingle()
        self.backlightPower = if Rimlight Power == 0x7F7FFFFF: r.readSingle()
        self.grayscaletoPaletteScale = r.readSingle()
        self.fresnelPower = r.readSingle()
        self.wetnessSpecScale = r.readSingle()
        self.wetnessSpecPower = r.readSingle()
        self.wetnessMinVar = r.readSingle()
        self.wetnessEnvMapScale = r.readSingle()
        self.wetnessFresnelPower = r.readSingle()
        self.wetnessMetalness = r.readSingle()
        self.environmentMapScale = if Skyrim Shader Type == 1: r.readSingle()
        self.unknownEnvMapShort = if Skyrim Shader Type == 1: r.readUInt16()
        self.skinTintColor = if Skyrim Shader Type == 5: Color3(r)
        self.unknownSkinTintInt = if Skyrim Shader Type == 5: r.readUInt32()
        self.hairTintColor = if Skyrim Shader Type == 6: Color3(r)
        self.maxPasses = if Skyrim Shader Type == 7: r.readSingle()
        self.scale = if Skyrim Shader Type == 7: r.readSingle()
        self.parallaxInnerLayerThickness = if Skyrim Shader Type == 11: r.readSingle()
        self.parallaxRefractionScale = if Skyrim Shader Type == 11: r.readSingle()
        self.parallaxInnerLayerTextureScale = if Skyrim Shader Type == 11: TexCoord(r)
        self.parallaxEnvmapStrength = if Skyrim Shader Type == 11: r.readSingle()
        self.sparkleParameters = if Skyrim Shader Type == 14: r.readVector4()
        self.eyeCubemapScale = if Skyrim Shader Type == 16: r.readSingle()
        self.leftEyeReflectionCenter = if Skyrim Shader Type == 16: r.readVector3()
        self.rightEyeReflectionCenter = if Skyrim Shader Type == 16: r.readVector3()

# Bethesda effect shader property for Skyrim and later.
class BSEffectShaderProperty(BSShaderProperty):
    shaderFlags1: Fallout4ShaderPropertyFlags1
    shaderFlags2: Fallout4ShaderPropertyFlags2
    uvOffset: TexCoord                                  # Offset UVs
    uvScale: TexCoord                                   # Offset UV Scale to repeat tiling textures
    sourceTexture: str                                  # points to an external texture.
    textureClampMode: int                               # How to handle texture borders.
    lightingInfluence: int
    envMapMinLod: int
    unknownByte: int
    falloffStartAngle: float                            # At this cosine of angle falloff will be equal to Falloff Start Opacity
    falloffStopAngle: float                             # At this cosine of angle falloff will be equal to Falloff Stop Opacity
    falloffStartOpacity: float                          # Alpha falloff multiplier at start angle
    falloffStopOpacity: float                           # Alpha falloff multiplier at end angle
    emissiveColor: Color4                               # Emissive color
    emissiveMultiple: float                             # Multiplier for Emissive Color (RGB part)
    softFalloffDepth: float
    greyscaleTexture: str                               # Points to an external texture, used as palette for SLSF1_Greyscale_To_PaletteColor/SLSF1_Greyscale_To_PaletteAlpha.
    envMapTexture: str
    normalTexture: str
    envMaskTexture: str
    environmentMapScale: float

    def __init__(self, r: Reader, h: Header):
        self.shaderFlags1 = SkyrimShaderPropertyFlags1(r.readUInt32())
        self.shaderFlags2 = SkyrimShaderPropertyFlags2(r.readUInt32())
        self.shaderFlags1 = Fallout4ShaderPropertyFlags1(r.readUInt32())
        self.shaderFlags2 = Fallout4ShaderPropertyFlags2(r.readUInt32())
        self.uvOffset = TexCoord(r)
        self.uvScale = TexCoord(r)
        self.sourceTexture = r.readL32AString()
        self.textureClampMode = r.readByte()
        self.lightingInfluence = r.readByte()
        self.envMapMinLod = r.readByte()
        self.unknownByte = r.readByte()
        self.falloffStartAngle = r.readSingle()
        self.falloffStopAngle = r.readSingle()
        self.falloffStartOpacity = r.readSingle()
        self.falloffStopOpacity = r.readSingle()
        self.emissiveColor = Color4(r)
        self.emissiveMultiple = r.readSingle()
        self.softFalloffDepth = r.readSingle()
        self.greyscaleTexture = r.readL32AString()
        self.envMapTexture = r.readL32AString()
        self.normalTexture = r.readL32AString()
        self.envMaskTexture = r.readL32AString()
        self.environmentMapScale = r.readSingle()

# Skyrim water shader property flags
class SkyrimWaterShaderFlags(Flag):
    SWSF1_UNKNOWN0 = 0,             # Unknown
    SWSF1_Bypass_Refraction_Map = 1 << 1, # Bypasses refraction map when set to 1
    SWSF1_Water_Toggle = 1 << 2,    # Main water Layer on/off
    SWSF1_UNKNOWN3 = 1 << 3,        # Unknown
    SWSF1_UNKNOWN4 = 1 << 4,        # Unknown
    SWSF1_UNKNOWN5 = 1 << 5,        # Unknown
    SWSF1_Highlight_Layer_Toggle = 1 << 6, # Reflection layer 2 on/off. (is this scene reflection?)
    SWSF1_Enabled = 1 << 7          # Water layer on/off

# Skyrim water shader property, different from "WaterShaderProperty" seen in Fallout.
class BSWaterShaderProperty(BSShaderProperty):
    shaderFlags1: SkyrimShaderPropertyFlags1
    shaderFlags2: SkyrimShaderPropertyFlags2
    uvOffset: TexCoord                                  # Offset UVs. Seems to be unused, but it fits with the other Skyrim shader properties.
    uvScale: TexCoord                                   # Offset UV Scale to repeat tiling textures, see above.
    waterShaderFlags: SkyrimWaterShaderFlags            # Defines attributes for the water shader (will use SkyrimWaterShaderFlags)
    waterDirection: int                                 # A bitflag, only the first/second bit controls water flow positive or negative along UVs.
    unknownShort3: int                                  # Unknown, flag?

    def __init__(self, r: Reader, h: Header):
        self.shaderFlags1 = SkyrimShaderPropertyFlags1(r.readUInt32())
        self.shaderFlags2 = SkyrimShaderPropertyFlags2(r.readUInt32())
        self.uvOffset = TexCoord(r)
        self.uvScale = TexCoord(r)
        self.waterShaderFlags = SkyrimWaterShaderFlags(r.readByte())
        self.waterDirection = r.readByte()
        self.unknownShort3 = r.readUInt16()

# Skyrim Sky shader block.
class BSSkyShaderProperty(BSShaderProperty):
    shaderFlags1: SkyrimShaderPropertyFlags1
    shaderFlags2: SkyrimShaderPropertyFlags2
    uvOffset: TexCoord                                  # Offset UVs. Seems to be unused, but it fits with the other Skyrim shader properties.
    uvScale: TexCoord                                   # Offset UV Scale to repeat tiling textures, see above.
    sourceTexture: str                                  # points to an external texture.
    skyObjectType: SkyObjectType

    def __init__(self, r: Reader, h: Header):
        self.shaderFlags1 = SkyrimShaderPropertyFlags1(r.readUInt32())
        self.shaderFlags2 = SkyrimShaderPropertyFlags2(r.readUInt32())
        self.uvOffset = TexCoord(r)
        self.uvScale = TexCoord(r)
        self.sourceTexture = r.readL32AString()
        self.skyObjectType = SkyObjectType(r.readUInt32())

# Bethesda-specific skin instance.
class BSDismemberSkinInstance(NiSkinInstance):
    partitions: list[BodyPartList]

    def __init__(self, r: Reader, h: Header):
        self.partitions = r.readL32FArray(lambda r: BodyPartList(r))

# Bethesda-specific extra data. Lists locations and normals on a mesh that are appropriate for decal placement.
class BSDecalPlacementVectorExtraData(NiFloatExtraData):
    vectorBlocks: list[DecalVectorArray]

    def __init__(self, r: Reader, h: Header):
        self.vectorBlocks = r.readL16FArray(lambda r: DecalVectorArray(r))

# Bethesda-specific particle modifier.
class BSPSysSimpleColorModifier(NiPSysModifier):
    fadeInPercent: float
    fadeoutPercent: float
    color1EndPercent: float
    color1StartPercent: float
    color2EndPercent: float
    color2StartPercent: float
    colors: list[Color4]

    def __init__(self, r: Reader, h: Header):
        self.fadeInPercent = r.readSingle()
        self.fadeoutPercent = r.readSingle()
        self.color1EndPercent = r.readSingle()
        self.color1StartPercent = r.readSingle()
        self.color2EndPercent = r.readSingle()
        self.color2StartPercent = r.readSingle()
        self.colors = r.readFArray(lambda r: Color4(r), 3)

# Flags for BSValueNode.
class BSValueNodeFlags(Flag):
    BillboardWorldZ = 0,
    UsePlayerAdjust = 1 << 1

# Bethesda-specific node. Found on fxFire effects
class BSValueNode(NiNode):
    value: int
    valueNodeFlags: BSValueNodeFlags

    def __init__(self, r: Reader, h: Header):
        self.value = r.readUInt32()
        self.valueNodeFlags = BSValueNodeFlags(r.readByte())

# Bethesda-Specific (mesh?) Particle System.
class BSStripParticleSystem(NiParticleSystem):

    def __init__(self, r: Reader, h: Header):

# Bethesda-Specific (mesh?) Particle System Data.
class BSStripPSysData(NiPSysData):
    maxPointCount: int
    startCapSize: float
    endCapSize: float
    doZPrepass: bool

    def __init__(self, r: Reader, h: Header):
        self.maxPointCount = r.readUInt16()
        self.startCapSize = r.readSingle()
        self.endCapSize = r.readSingle()
        self.doZPrepass = r.readBool32()

# Bethesda-Specific (mesh?) Particle System Modifier.
class BSPSysStripUpdateModifier(NiPSysModifier):
    updateDeltaTime: float

    def __init__(self, r: Reader, h: Header):
        self.updateDeltaTime = r.readSingle()

# Bethesda-Specific time controller.
class BSMaterialEmittanceMultController(NiFloatInterpController):

    def __init__(self, r: Reader, h: Header):

# Bethesda-Specific particle system.
class BSMasterParticleSystem(NiNode):
    maxEmitterObjects: int
    particleSystems: list[int]

    def __init__(self, r: Reader, h: Header):
        self.maxEmitterObjects = r.readUInt16()
        self.particleSystems = r.readL32FArray(X[NiAVObject].ref)

# Particle system (multi?) emitter controller.
class BSPSysMultiTargetEmitterCtlr(NiPSysEmitterCtlr):
    maxEmitters: int
    masterParticleSystem: int

    def __init__(self, r: Reader, h: Header):
        self.maxEmitters = r.readUInt16()
        self.masterParticleSystem = X[BSMasterParticleSystem].ptr(r)

# Bethesda-Specific time controller.
class BSRefractionStrengthController(NiFloatInterpController):

    def __init__(self, r: Reader, h: Header):

# Bethesda-Specific node.
class BSOrderedNode(NiNode):
    alphaSortBound: Vector4
    staticBound: bool

    def __init__(self, r: Reader, h: Header):
        self.alphaSortBound = r.readVector4()
        self.staticBound = r.readBool32()

# Bethesda-Specific node.
class BSRangeNode(NiNode):
    min: int
    max: int
    current: int

    def __init__(self, r: Reader, h: Header):
        self.min = r.readByte()
        self.max = r.readByte()
        self.current = r.readByte()

# Bethesda-Specific node.
class BSBlastNode(BSRangeNode):

    def __init__(self, r: Reader, h: Header):

# Bethesda-Specific node.
class BSDamageStage(BSBlastNode):

    def __init__(self, r: Reader, h: Header):

# Bethesda-specific time controller.
class BSRefractionFirePeriodController(NiTimeController):
    interpolator: int

    def __init__(self, r: Reader, h: Header):
        self.interpolator = if h.v >= 0x14020007: X[NiInterpolator].ref(r)

# A havok shape.
# A list of convex shapes.
# 
# Do not put a bhkPackedNiTriStripsShape in the Sub Shapes. Use a
# separate collision nodes without a list shape for those.
# 
# Also, shapes collected in a bhkListShape may not have the correct
# walking noise, so only use it for non-walkable objects.
class bhkConvexListShape(bhkShape):
    subShapes: list[int]                                # List of shapes.
    material: HavokMaterial                             # The material of the shape.
    radius: float
    unknownInt1: int
    unknownFloat1: float
    childShapeProperty: hkWorldObjCinfoProperty
    unknownByte1: int
    unknownFloat2: float

    def __init__(self, r: Reader, h: Header):
        self.subShapes = r.readL32FArray(X[bhkConvexShape].ref)
        self.material = HavokMaterial(r, h)
        self.radius = r.readSingle()
        self.unknownInt1 = r.readUInt32()
        self.unknownFloat1 = r.readSingle()
        self.childShapeProperty = hkWorldObjCinfoProperty(r)
        self.unknownByte1 = r.readByte()
        self.unknownFloat2 = r.readSingle()

# Bethesda-specific compound.
class BSTreadTransform:
    def __init__(self, r: Reader):
        self.name: str = Y.string(r)
        self.transform1: NiQuatTransform = NiQuatTransform(r, h)
        self.transform2: NiQuatTransform = NiQuatTransform(r, h)

# Bethesda-specific interpolator.
class BSTreadTransfInterpolator(NiInterpolator):
    treadTransforms: list[BSTreadTransform]
    data: int

    def __init__(self, r: Reader, h: Header):
        self.treadTransforms = r.readL32FArray(lambda r: BSTreadL32Transform(r))
        self.data = X[NiFloatData].ref(r)

# Anim note types.
class AnimNoteType(Enum):
    ANT_INVALID = 0,                # ANT_INVALID
    ANT_GRABIK = 1,                 # ANT_GRABIK
    ANT_LOOKIK = 2                  # ANT_LOOKIK

# Bethesda-specific object.
class BSAnimNote(NiObject):
    type: AnimNoteType                                  # Type of this note.
    time: float                                         # Location in time.
    arm: int                                            # Unknown.
    gain: float                                         # Unknown.
    state: int                                          # Unknown.

    def __init__(self, r: Reader, h: Header):
        self.type = AnimNoteType(r.readUInt32())
        self.time = r.readSingle()
        self.arm = if Type == 1: r.readUInt32()
        self.gain = if Type == 2: r.readSingle()
        self.state = if Type == 2: r.readUInt32()

# Bethesda-specific object.
class BSAnimNotes(NiObject):
    animNotes: list[int]                                # BSAnimNote objects.

    def __init__(self, r: Reader, h: Header):
        self.animNotes = r.readL16FArray(X[BSAnimNote].ref)

# Bethesda-specific Havok serializable.
class bhkLiquidAction(bhkSerializable):
    userData: int
    unknownInt2: int                                    # Unknown
    unknownInt3: int                                    # Unknown
    initialStickForce: float
    stickStrength: float
    neighborDistance: float
    neighborStrength: float

    def __init__(self, r: Reader, h: Header):
        self.userData = r.readUInt32()
        self.unknownInt2 = r.readInt32()
        self.unknownInt3 = r.readInt32()
        self.initialStickForce = r.readSingle()
        self.stickStrength = r.readSingle()
        self.neighborDistance = r.readSingle()
        self.neighborStrength = r.readSingle()

# Culling modes for multi bound nodes.
class BSCPCullingType(Enum):
    BSCP_CULL_NORMAL = 0,           # Normal
    BSCP_CULL_ALLPASS = 1,          # All Pass
    BSCP_CULL_ALLFAIL = 2,          # All Fail
    BSCP_CULL_IGNOREMULTIBOUNDS = 3,# Ignore Multi Bounds
    BSCP_CULL_FORCEMULTIBOUNDSNOUPDATE = 4 # Force Multi Bounds No Update

# Bethesda-specific node.
class BSMultiBoundNode(NiNode):
    multiBound: int
    cullingMode: BSCPCullingType

    def __init__(self, r: Reader, h: Header):
        self.multiBound = X[BSMultiBound].ref(r)
        self.cullingMode = BSCPCullingType(r.readUInt32())

# Bethesda-specific object.
class BSMultiBound(NiObject):
    data: int

    def __init__(self, r: Reader, h: Header):
        self.data = X[BSMultiBoundData].ref(r)

# Abstract base type for bounding data.
class BSMultiBoundData(NiObject):

    def __init__(self, r: Reader, h: Header):

# Oriented bounding box.
class BSMultiBoundOBB(BSMultiBoundData):
    center: Vector3                                     # Center of the box.
    size: Vector3                                       # Size of the box along each axis.
    rotation: Matrix3x3                                 # Rotation of the bounding box.

    def __init__(self, r: Reader, h: Header):
        self.center = r.readVector3()
        self.size = r.readVector3()
        self.rotation = r.readMatrix3x3()

# Bethesda-specific object.
class BSMultiBoundSphere(BSMultiBoundData):
    center: Vector3
    radius: float

    def __init__(self, r: Reader, h: Header):
        self.center = r.readVector3()
        self.radius = r.readSingle()

# This is only defined because of recursion issues.
class BSGeometrySubSegment:
    def __init__(self, r: Reader):
        self.startIndex: int = r.readUInt32()
        self.numPrimitives: int = r.readUInt32()
        self.parentArrayIndex: int = r.readUInt32()
        self.unused: int = r.readUInt32()

# Bethesda-specific. Describes groups of triangles either segmented in a grid (for LOD) or by body part for skinned FO4 meshes.
class BSGeometrySegmentData:
    def __init__(self, r: Reader, h: Header):
        self.flags: int = r.readByte()
        self.index: int = r.readUInt32()                # Index = previous Index + previous Num Tris in Segment * 3
        self.numTrisinSegment: int = r.readUInt32()     # The number of triangles belonging to this segment
        self.startIndex: int = r.readUInt32() if (h.uv2 == 130) else None
        self.numPrimitives: int = r.readUInt32() if (h.uv2 == 130) else None
        self.parentArrayIndex: int = r.readUInt32() if (h.uv2 == 130) else None
        self.subSegment: list[BSGeometrySubSegment] = r.readL32FArray(lambda r: BSGeometrySubSegment(r)) if (h.uv2 == 130) else None

# Bethesda-specific AV object.
class BSSegmentedTriShape(NiTriShape):
    segment: list[BSGeometrySegmentData]                # Configuration of each segment

    def __init__(self, r: Reader, h: Header):
        self.segment = r.readL32FArray(lambda r: BSGeometrySegmentData(r, h))

# Bethesda-specific object.
class BSMultiBoundAABB(BSMultiBoundData):
    position: Vector3                                   # Position of the AABB's center
    extent: Vector3                                     # Extent of the AABB in all directions

    def __init__(self, r: Reader, h: Header):
        self.position = r.readVector3()
        self.extent = r.readVector3()

class AdditionalDataInfo:
    def __init__(self, r: Reader):
        self.dataType: int = r.readInt32()              # Type of data in this channel
        self.numChannelBytesPerElement: int = r.readInt32() # Number of bytes per element of this channel
        self.numChannelBytes: int = r.readInt32()       # Total number of bytes of this channel (num vertices times num bytes per element)
        self.numTotalBytesPerElement: int = r.readInt32() # Number of bytes per element in all channels together. Sum of num channel bytes per element over all block infos.
        self.blockIndex: int = r.readInt32()            # Unsure. The block in which this channel is stored? Usually there is only one block, and so this is zero.
        self.channelOffset: int = r.readInt32()         # Offset (in bytes) of this channel. Sum of all num channel bytes per element of all preceeding block infos.
        self.unknownByte1: int = r.readByte()           # Unknown, usually equal to 2.

class AdditionalDataBlock:
    def __init__(self, r: Reader, h: Header):
        self.hasData: bool = r.readBool32()             # Has data
        self.blockSize: int = r.readInt32() if Has Data else None # Size of Block
        self.blockOffsets: list[int] = r.readL32PArray(None, 'i') if Has Data else None
        self.numData: int = r.readInt32() if Has Data else None
        self.dataSizes: list[int] = r.readPArray(None, 'i', NumData) if Has Data else None
        self.data: list[list[int]] = r.readFArray(lambda k: r.readBytes(NumData), BlockSize) if Has Data else None

class BSPackedAdditionalDataBlock:
    def __init__(self, r: Reader, h: Header):
        self.hasData: bool = r.readBool32()             # Has data
        self.numTotalBytes: int = r.readInt32() if Has Data else None # Total number of bytes (over all channels and all elements, equals num total bytes per element times num vertices).
        self.blockOffsets: list[int] = r.readL32PArray(None, 'i') if Has Data else None # Block offsets in the data? Usually equal to zero.
        self.atomSizes: list[int] = r.readL32PArray(None, 'i') if Has Data else None # The sum of all of these equal num total bytes per element, so this probably describes how each data element breaks down into smaller chunks (i.e. atoms).
        self.data: list[int] = r.readBytes(NumTotalBytes) if Has Data else None
        self.unknownInt1: int = r.readInt32()
        self.numTotalBytesPerElement: int = r.readInt32() # Unsure, but this seems to correspond again to the number of total bytes per element.

class NiAdditionalGeometryData(AbstractAdditionalGeometryData):
    numVertices: int                                    # Number of vertices
    blockInfos: list[AdditionalDataInfo]                # Number of additional data blocks
    blocks: list[AdditionalDataBlock]                   # Number of additional data blocks

    def __init__(self, r: Reader, h: Header):
        self.numVertices = r.readUInt16()
        self.blockInfos = r.readL32FArray(lambda r: AdditionalDataInfo(r))
        self.blocks = r.readL32FArray(lambda r: AdditionalDataBlock(r, h))

class BSPackedAdditionalGeometryData(AbstractAdditionalGeometryData):
    numVertices: int
    blockInfos: list[AdditionalDataInfo]                # Number of additional data blocks
    blocks: list[BSPackedAdditionalDataBlock]           # Number of additional data blocks

    def __init__(self, r: Reader, h: Header):
        self.numVertices = r.readUInt16()
        self.blockInfos = r.readL32FArray(lambda r: AdditionalDataInfo(r))
        self.blocks = r.readL32FArray(lambda r: BSPackedAdditionalDataBlock(r, h))

# Bethesda-specific extra data.
class BSWArray(NiExtraData):
    items: list[int]

    def __init__(self, r: Reader, h: Header):
        self.items = r.readL32PArray(None, 'i')

# Bethesda-specific Havok serializable.
class bhkAabbPhantom(bhkShapePhantom):
    unused: list[int]
    aabbMin: Vector4
    aabbMax: Vector4

    def __init__(self, r: Reader, h: Header):
        self.unused = r.readBytes(8)
        self.aabbMin = r.readVector4()
        self.aabbMax = r.readVector4()

# Bethesda-specific time controller.
class BSFrustumFOVController(NiFloatInterpController):

    def __init__(self, r: Reader, h: Header):

# Bethesda-Specific node.
class BSDebrisNode(BSRangeNode):

    def __init__(self, r: Reader, h: Header):

# A breakable constraint.
class bhkBreakableConstraint(bhkConstraint):
    constraintData: ConstraintData                      # Constraint within constraint.
    threshold: float                                    # Amount of force to break the rigid bodies apart?
    removeWhenBroken: bool                              # No: Constraint stays active. Yes: Constraint gets removed when breaking threshold is exceeded.

    def __init__(self, r: Reader, h: Header):
        self.constraintData = ConstraintData(r, h)
        self.threshold = r.readSingle()
        self.removeWhenBroken = r.readBool32()

# Bethesda-Specific Havok serializable.
class bhkOrientHingedBodyAction(bhkSerializable):
    body: int
    unknownInt1: int
    unknownInt2: int
    unused1: list[int]
    hingeAxisLs: Vector4
    forwardLs: Vector4
    strength: float
    damping: float
    unused2: list[int]

    def __init__(self, r: Reader, h: Header):
        self.body = X[bhkRigidBody].ptr(r)
        self.unknownInt1 = r.readUInt32()
        self.unknownInt2 = r.readUInt32()
        self.unused1 = r.readBytes(8)
        self.hingeAxisLs = r.readVector4()
        self.forwardLs = r.readVector4()
        self.strength = r.readSingle()
        self.damping = r.readSingle()
        self.unused2 = r.readBytes(8)

# Found in Fallout 3 .psa files, extra ragdoll info for NPCs/creatures. (usually idleanims\deathposes.psa)
# Defines different kill poses. The game selects the pose randomly and applies it to a skeleton immediately upon ragdolling.
# Poses can be previewed in GECK Object Window-Actor Data-Ragdoll and selecting Pose Matching tab.
class bhkPoseArray(NiObject):
    bones: list[str]
    poses: list[BonePose]

    def __init__(self, r: Reader, h: Header):
        self.bones = r.readL32FArray(lambda r: Y.string(r))
        self.poses = r.readL32FArray(lambda r: BonePose(r))

# Found in Fallout 3, more ragdoll info?  (meshes\ragdollconstraint\*.rdt)
class bhkRagdollTemplate(NiExtraData):
    bones: list[int]

    def __init__(self, r: Reader, h: Header):
        self.bones = r.readL32FArray(X[NiObject].ref)

# Data for bhkRagdollTemplate
class bhkRagdollTemplateData(NiObject):
    name: str
    mass: float
    restitution: float
    friction: float
    radius: float
    material: HavokMaterial
    constraint: list[ConstraintData]

    def __init__(self, r: Reader, h: Header):
        self.name = Y.string(r)
        self.mass = r.readSingle()
        self.restitution = r.readSingle()
        self.friction = r.readSingle()
        self.radius = r.readSingle()
        self.material = HavokMaterial(r, h)
        self.constraint = r.readL32FArray(lambda r: ConstraintData(r, h))

# A range of indices, which make up a region (such as a submesh).
class Region:
    def __init__(self, r: Reader):
        self.startIndex: int = r.readUInt32()
        self.numIndices: int = r.readUInt32()

# Sets how objects are to be cloned.
class CloningBehavior(Enum):
    CLONING_SHARE = 0,              # Share this object pointer with the newly cloned scene.
    CLONING_COPY = 1,               # Create an exact duplicate of this object for use with the newly cloned scene.
    CLONING_BLANK_COPY = 2          # Create a copy of this object for use with the newly cloned stream, leaving some of the data to be written later.

# The data format of components.
class ComponentFormat(Enum):
    F_UNKNOWN = 0x00000000,         # Unknown, or don't care, format.
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

# Determines how a data stream is used?
class DataStreamUsage(Enum):
    USAGE_VERTEX_INDEX = 0,
    USAGE_VERTEX = 1,
    USAGE_SHADER_CONSTANT = 2,
    USAGE_USER = 3

# Determines how the data stream is accessed?
class DataStreamAccess(Flag):
    CPU_Read = 0,
    CPU_Write_Static = 1 << 1,
    CPU_Write_Mutable = 1 << 2,
    CPU_Write_Volatile = 1 << 3,
    GPU_Read = 1 << 4,
    GPU_Write = 1 << 5,
    CPU_Write_Static_Inititialized = 1 << 6

class NiDataStream(NiObject):
    usage: DataStreamUsage
    access: DataStreamAccess
    numBytes: int                                       # The size in bytes of this data stream.
    cloningBehavior: CloningBehavior
    regions: list[Region]                               # The regions in the mesh. Regions can be used to mark off submeshes which are independent draw calls.
    componentFormats: list[ComponentFormat]             # The format of each component in this data stream.
    data: list[int]
    streamable: bool

    def __init__(self, r: Reader, h: Header):
        self.usage = DataStreamUsage(r.readUInt32())
        self.access = DataStreamAccess(r.readUInt32())
        self.numBytes = r.readUInt32()
        self.cloningBehavior = CloningBehavior(r.readUInt32())
        self.regions = r.readL32FArray(lambda r: Region(r))
        self.componentFormats = r.readL32FArray(lambda r: ComponentFormat(r.readL32UInt32()))
        self.data = r.readBytes(NumBytes)
        self.streamable = r.readBool32()

class SemanticData:
    def __init__(self, r: Reader):
        self.name: str = Y.string(r)                    # Type of data (POSITION, POSITION_BP, INDEX, NORMAL, NORMAL_BP,
                                                        #     TEXCOORD, BLENDINDICES, BLENDWEIGHT, BONE_PALETTE, COLOR, DISPLAYLIST,
                                                        #     MORPH_POSITION, BINORMAL_BP, TANGENT_BP).
        self.index: int = r.readUInt32()                # An extra index of the data. For example, if there are 3 uv maps,
                                                        #     then the corresponding TEXCOORD data components would have indices
                                                        #     0, 1, and 2, respectively.

class DataStreamRef:
    def __init__(self, r: Reader):
        self.stream: int = X[NiDataStream].ref(r)       # Reference to a data stream object which holds the data used by
                                                        #     this reference.
        self.isPerInstance: bool = r.readBool32()       # Sets whether this stream data is per-instance data for use in
                                                        #     hardware instancing.
        self.submeshToRegionMap: list[int] = r.readL16PArray(None, 'H') # A lookup table that maps submeshes to regions.
        self.componentSemantics: list[SemanticData] = r.readL32FArray(lambda r: SemanticData(r)) # Describes the semantic of each component.

# An object that can be rendered.
class NiRenderObject(NiAVObject):
    materialData: MaterialData                          # Per-material data.

    def __init__(self, r: Reader, h: Header):
        self.materialData = MaterialData(r, h)

# Describes the type of primitives stored in a mesh object.
class MeshPrimitiveType(Enum):
    MESH_PRIMITIVE_TRIANGLES = 0,   # Triangle primitive type.
    MESH_PRIMITIVE_TRISTRIPS = 1,   # Triangle strip primitive type.
    MESH_PRIMITIVE_LINES = 2,       # Lines primitive type.
    MESH_PRIMITIVE_LINESTRIPS = 3,  # Line strip primitive type.
    MESH_PRIMITIVE_QUADS = 4,       # Quadrilateral primitive type.
    MESH_PRIMITIVE_POINTS = 5       # Point primitive type.

# A sync point corresponds to a particular stage in per-frame processing.
class SyncPoint(Enum):
    SYNC_ANY = 0x8000,              # Synchronize for any sync points that the modifier supports.
    SYNC_UPDATE = 0x8010,           # Synchronize when an object is updated.
    SYNC_POST_UPDATE = 0x8020,      # Synchronize when an entire scene graph has been updated.
    SYNC_VISIBLE = 0x8030,          # Synchronize when an object is determined to be potentially visible.
    SYNC_RENDER = 0x8040,           # Synchronize when an object is rendered.
    SYNC_PHYSICS_SIMULATE = 0x8050, # Synchronize when a physics simulation step is about to begin.
    SYNC_PHYSICS_COMPLETED = 0x8060,# Synchronize when a physics simulation step has produced results.
    SYNC_REFLECTIONS = 0x8070       # Synchronize after all data necessary to calculate reflections is ready.

# Base class for mesh modifiers.
class NiMeshModifier(NiObject):
    submitPoints: list[SyncPoint]                       # The sync points supported by this mesh modifier for SubmitTasks.
    completePoints: list[SyncPoint]                     # The sync points supported by this mesh modifier for CompleteTasks.

    def __init__(self, r: Reader, h: Header):
        self.submitPoints = r.readL32FArray(lambda r: SyncPoint(r.readL32UInt16()))
        self.completePoints = r.readL32FArray(lambda r: SyncPoint(r.readL32UInt16()))

class ExtraMeshDataEpicMickey:
    def __init__(self, r: Reader):
        self.unknownInt1: int = r.readInt32()
        self.unknownInt2: int = r.readInt32()
        self.unknownInt3: int = r.readInt32()
        self.unknownInt4: float = r.readSingle()
        self.unknownInt5: float = r.readSingle()
        self.unknownInt6: float = r.readSingle()

class ExtraMeshDataEpicMickey2:
    def __init__(self, r: Reader):
        self.start: int = r.readInt32()
        self.end: int = r.readInt32()
        self.unknownShorts: list[int] = r.readPArray(None, 'h', 10)

class NiMesh(NiRenderObject):
    primitiveType: MeshPrimitiveType                    # The primitive type of the mesh, such as triangles or lines.
    unknown51: int
    unknown52: int
    unknown53: int
    unknown54: int
    unknown55: float
    unknown56: int
    numSubmeshes: int                                   # The number of submeshes contained in this mesh.
    instancingEnabled: bool                             # Sets whether hardware instancing is being used.
    bound: NiBound                                      # The combined bounding volume of all submeshes.
    datastreams: list[DataStreamRef]
    modifiers: list[int]
    unknown100: int                                     # Unknown.
    unknown101: int                                     # Unknown.
    unknown102: int                                     # Size of additional data.
    unknown103: list[float]
    unknown200: int
    unknown201: list[ExtraMeshDataEpicMickey]
    unknown250: int
    unknown251: list[int]
    unknown300: int
    unknown301: int
    unknown302: int
    unknown303: list[int]
    unknown350: int
    unknown351: list[ExtraMeshDataEpicMickey2]
    unknown400: int

    def __init__(self, r: Reader, h: Header):
        self.primitiveType = MeshPrimitiveType(r.readUInt32())
        self.unknown51 = if (Uh.v == 15): r.readInt32()
        self.unknown52 = if (Uh.v == 15): r.readInt32()
        self.unknown53 = if (Uh.v == 15): r.readInt32()
        self.unknown54 = if (Uh.v == 15): r.readInt32()
        self.unknown55 = if (Uh.v == 15): r.readSingle()
        self.unknown56 = if (Uh.v == 15): r.readInt32()
        self.numSubmeshes = r.readUInt16()
        self.instancingEnabled = r.readBool32()
        self.bound = NiBound(r)
        self.datastreams = r.readL32FArray(lambda r: DataStreamRef(r))
        self.modifiers = r.readL32FArray(X[NiMeshModifier].ref)
        self.unknown100 = if (Uh.v == 15): r.readByte()
        self.unknown101 = if (Uh.v == 15): r.readInt32()
        self.unknown102 = if (Uh.v == 15): r.readUInt32()
        self.unknown103 = if (Uh.v == 15): r.readPArray(None, 'f', Unknown102)
        self.unknown200 = if (Uh.v == 15): r.readInt32()
        self.unknown201 = if (Uh.v == 15): r.readFArray(lambda r: ExtraMeshDataEpicMickey(r), Unknown200)
        self.unknown250 = if (Uh.v == 15): r.readInt32()
        self.unknown251 = if (Uh.v == 15): r.readPArray(None, 'i', Unknown250)
        self.unknown300 = if (Uh.v == 15): r.readInt32()
        self.unknown301 = if (Uh.v == 15): r.readInt16()
        self.unknown302 = if (Uh.v == 15): r.readInt32()
        self.unknown303 = if (Uh.v == 15): r.readBytes(Unknown302)
        self.unknown350 = if (Uh.v == 15): r.readInt32()
        self.unknown351 = if (Uh.v == 15): r.readFArray(lambda r: ExtraMeshDataEpicMickey2(r), Unknown350)
        self.unknown400 = if (Uh.v == 15): r.readInt32()

# Manipulates a mesh with the semantic MORPHWEIGHTS using an NiMorphMeshModifier.
class NiMorphWeightsController(NiInterpController):
    count: int
    interpolators: list[int]
    targetNames: list[str]

    def __init__(self, r: Reader, h: Header):
        self.count = r.readUInt32()
        self.interpolators = r.readL32FArray(X[NiInterpolator].ref)
        self.targetNames = r.readL32FArray(lambda r: Y.string(r))

class ElementReference:
    def __init__(self, r: Reader):
        self.semantic: SemanticData = SemanticData(r)   # The element semantic.
        self.normalizeFlag: int = r.readUInt32()        # Whether or not to normalize the data.

# Performs linear-weighted blending between a set of target data streams.
class NiMorphMeshModifier(NiMeshModifier):
    flags: int                                          # FLAG_RELATIVETARGETS = 0x01
                                                        #     FLAG_UPDATENORMALS   = 0x02
                                                        #     FLAG_NEEDSUPDATE     = 0x04
                                                        #     FLAG_ALWAYSUPDATE    = 0x08
                                                        #     FLAG_NEEDSCOMPLETION = 0x10
                                                        #     FLAG_SKINNED = 0x20
                                                        #     FLAG_SWSKINNED       = 0x40
    numTargets: int                                     # The number of morph targets.
    elements: list[ElementReference]                    # Semantics and normalization of the morphing data stream elements.

    def __init__(self, r: Reader, h: Header):
        self.flags = r.readByte()
        self.numTargets = r.readUInt16()
        self.elements = r.readL32FArray(lambda r: ElementReference(r))

class NiSkinningMeshModifier(NiMeshModifier):
    flags: int                                          # USE_SOFTWARE_SKINNING = 0x0001
                                                        #     RECOMPUTE_BOUNDS = 0x0002
    skeletonRoot: int                                   # The root bone of the skeleton.
    skeletonTransform: NiTransform                      # The transform that takes the root bone parent coordinate system into the skin coordinate system.
    numBones: int                                       # The number of bones referenced by this mesh modifier.
    bones: list[int]                                    # Pointers to the bone nodes that affect this skin.
    boneTransforms: list[NiTransform]                   # The transforms that go from bind-pose space to bone space.
    boneBounds: list[NiBound]                           # The bounds of the bones.  Only stored if the RECOMPUTE_BOUNDS bit is set.

    def __init__(self, r: Reader, h: Header):
        self.flags = r.readUInt16()
        self.skeletonRoot = X[NiAVObject].ptr(r)
        self.skeletonTransform = NiTransform(r)
        self.numBones = r.readUInt32()
        self.bones = r.readFArray(X[NiAVObject].ptr, NumBones)
        self.boneTransforms = r.readFArray(lambda r: NiTransform(r), NumBones)
        self.boneBounds = if (Flags & 2)!=0: r.readFArray(lambda r: NiBound(r), NumBones)

# An instance of a hardware-instanced mesh in a scene graph.
class NiMeshHWInstance(NiAVObject):
    masterMesh: int                                     # The instanced mesh this object represents.
    meshModifier: int

    def __init__(self, r: Reader, h: Header):
        self.masterMesh = X[NiMesh].ref(r)
        self.meshModifier = X[NiInstancingMeshModifier].ref(r)

# Mesh modifier that provides per-frame instancing capabilities in Gamebryo.
class NiInstancingMeshModifier(NiMeshModifier):
    hasInstanceNodes: bool
    perInstanceCulling: bool
    hasStaticBounds: bool
    affectedMesh: int
    bound: NiBound
    instanceNodes: list[int]

    def __init__(self, r: Reader, h: Header):
        self.hasInstanceNodes = r.readBool32()
        self.perInstanceCulling = r.readBool32()
        self.hasStaticBounds = r.readBool32()
        self.affectedMesh = X[NiMesh].ref(r)
        self.bound = if Has Static Bounds: NiBound(r)
        self.instanceNodes = if Has Instance Nodes: r.readL32FArray(X[NiMeshHWInstance].ref)

class LODInfo:
    def __init__(self, r: Reader):
        self.numBones: int = r.readUInt32()
        self.skinIndices: list[int] = r.readL32PArray(None, 'I')

# Defines the levels of detail for a given character and dictates the character's current LOD.
class NiSkinningLODController(NiTimeController):
    currentLod: int
    bones: list[int]
    skins: list[int]
    loDs: list[LODInfo]

    def __init__(self, r: Reader, h: Header):
        self.currentLod = r.readUInt32()
        self.bones = r.readL32FArray(X[NiNode].ref)
        self.skins = r.readL32FArray(X[NiMesh].ref)
        self.loDs = r.readL32FArray(lambda r: LODInfo(r))

class PSSpawnRateKey:
    def __init__(self, r: Reader):
        self.value: float = r.readSingle()
        self.time: float = r.readSingle()

# Describes the various methods that may be used to specify the orientation of the particles.
class AlignMethod(Enum):
    ALIGN_INVALID = 0,
    ALIGN_PER_PARTICLE = 1,
    ALIGN_LOCAL_FIXED = 2,
    ALIGN_LOCAL_POSITION = 5,
    ALIGN_LOCAL_VELOCITY = 9,
    ALIGN_CAMERA = 16

# Represents a particle system.
class NiPSParticleSystem(NiMesh):
    simulator: int
    generator: int
    emitters: list[int]
    spawners: list[int]
    deathSpawner: int
    maxNumParticles: int
    hasColors: bool
    hasRotations: bool
    hasRotationAxes: bool
    hasAnimatedTextures: bool
    worldSpace: bool
    normalMethod: AlignMethod
    normalDirection: Vector3
    upMethod: AlignMethod
    upDirection: Vector3
    livingSpawner: int
    spawnRateKeys: list[PSSpawnRateKey]
    preRpi: bool

    def __init__(self, r: Reader, h: Header):
        self.simulator = X[NiPSSimulator].ref(r)
        self.generator = X[NiPSBoundUpdater].ref(r)
        self.emitters = r.readL32FArray(X[NiPSEmitter].ref)
        self.spawners = r.readL32FArray(X[NiPSSpawner].ref)
        self.deathSpawner = X[NiPSSpawner].ref(r)
        self.maxNumParticles = r.readUInt32()
        self.hasColors = r.readBool32()
        self.hasRotations = r.readBool32()
        self.hasRotationAxes = r.readBool32()
        self.hasAnimatedTextures = if h.v >= 0x14060100: r.readBool32()
        self.worldSpace = r.readBool32()
        self.normalMethod = if h.v >= 0x14060100: AlignMethod(r.readUInt32())
        self.normalDirection = if h.v >= 0x14060100: r.readVector3()
        self.upMethod = if h.v >= 0x14060100: AlignMethod(r.readUInt32())
        self.upDirection = if h.v >= 0x14060100: r.readVector3()
        self.livingSpawner = if h.v >= 0x14060100: X[NiPSSpawner].ref(r)
        self.spawnRateKeys = if h.v >= 0x14060100: r.readL8FArray(lambda r: PSSpawnRateKey(r))
        self.preRpi = if h.v >= 0x14060100: r.readBool32()

# Represents a particle system that uses mesh particles instead of sprite-based particles.
class NiPSMeshParticleSystem(NiPSParticleSystem):
    masterParticles: list[int]
    poolSize: int
    autoFillPools: bool

    def __init__(self, r: Reader, h: Header):
        self.masterParticles = r.readL32FArray(X[NiAVObject].ref)
        self.poolSize = r.readUInt32()
        self.autoFillPools = r.readBool32()

# A mesh modifier that uses particle system data to generate camera-facing quads.
class NiPSFacingQuadGenerator(NiMeshModifier):

    def __init__(self, r: Reader, h: Header):

# A mesh modifier that uses particle system data to generate aligned quads for each particle.
class NiPSAlignedQuadGenerator(NiMeshModifier):
    scaleAmountU: float
    scaleLimitU: float
    scaleRestU: float
    scaleAmountV: float
    scaleLimitV: float
    scaleRestV: float
    centerU: float
    centerV: float
    uvScrolling: bool
    numFramesAcross: int
    numFramesDown: int
    pingPong: bool
    initialFrame: int
    initialFrameVariation: float
    numFrames: int
    numFramesVariation: float
    initialTime: float
    finalTime: float

    def __init__(self, r: Reader, h: Header):
        self.scaleAmountU = r.readSingle()
        self.scaleLimitU = r.readSingle()
        self.scaleRestU = r.readSingle()
        self.scaleAmountV = r.readSingle()
        self.scaleLimitV = r.readSingle()
        self.scaleRestV = r.readSingle()
        self.centerU = r.readSingle()
        self.centerV = r.readSingle()
        self.uvScrolling = r.readBool32()
        self.numFramesAcross = r.readUInt16()
        self.numFramesDown = r.readUInt16()
        self.pingPong = r.readBool32()
        self.initialFrame = r.readUInt16()
        self.initialFrameVariation = r.readSingle()
        self.numFrames = r.readUInt16()
        self.numFramesVariation = r.readSingle()
        self.initialTime = r.readSingle()
        self.finalTime = r.readSingle()

# The mesh modifier that performs all particle system simulation.
class NiPSSimulator(NiMeshModifier):
    simulationSteps: list[int]

    def __init__(self, r: Reader, h: Header):
        self.simulationSteps = r.readL32FArray(X[NiPSSimulatorStep].ref)

# Abstract base class for a single step in the particle system simulation process.  It has no seralized data.
class NiPSSimulatorStep(NiObject):

    def __init__(self, r: Reader, h: Header):

class PSLoopBehavior(Enum):
    PS_LOOP_CLAMP_BIRTH = 0,        # Key times map such that the first key occurs at the birth of the particle, and times later than the last key get the last key value.
    PS_LOOP_CLAMP_DEATH = 1,        # Key times map such that the last key occurs at the death of the particle, and times before the initial key time get the value of the initial key.
    PS_LOOP_AGESCALE = 2,           # Scale the animation to fit the particle lifetime, so that the first key is age zero, and the last key comes at the particle death.
    PS_LOOP_LOOP = 3,               # The time is converted to one within the time range represented by the keys, as if the key sequence loops forever in the past and future.
    PS_LOOP_REFLECT = 4             # The time is reflection looped, as if the keys played forward then backward the forward then backward etc for all time.

# Encapsulates a floodgate kernel that updates particle size, colors, and rotations.
class NiPSSimulatorGeneralStep(NiPSSimulatorStep):
    sizeKeys: list[Key[T]]                              # The particle size keys.
    sizeLoopBehavior: PSLoopBehavior                    # The loop behavior for the size keys.
    colorKeys: list[Key[T]]                             # The particle color keys.
    colorLoopBehavior: PSLoopBehavior                   # The loop behavior for the color keys.
    rotationKeys: list[QuatKey[T]]                      # The particle rotation keys.
    rotationLoopBehavior: PSLoopBehavior                # The loop behavior for the rotation keys.
    growTime: float                                     # The the amount of time over which a particle's size is ramped from 0.0 to 1.0 in seconds
    shrinkTime: float                                   # The the amount of time over which a particle's size is ramped from 1.0 to 0.0 in seconds
    growGeneration: int                                 # Specifies the particle generation to which the grow effect should be applied. This is usually generation 0, so that newly created particles will grow.
    shrinkGeneration: int                               # Specifies the particle generation to which the shrink effect should be applied. This is usually the highest supported generation for the particle system, so that particles will shrink immediately before getting killed.

    def __init__(self, r: Reader, h: Header):
        self.sizeKeys = if h.v >= 0x14060100: r.readL8FArray(lambda r: Key[T](r, h))
        self.sizeLoopBehavior = if h.v >= 0x14060100: PSLoopBehavior(r.readUInt32())
        self.colorKeys = r.readL8FArray(lambda r: Key[T](r, h))
        self.colorLoopBehavior = if h.v >= 0x14060100: PSLoopBehavior(r.readUInt32())
        self.rotationKeys = if h.v >= 0x14060100: r.readL8FArray(lambda r: QuatKey[T](r, h))
        self.rotationLoopBehavior = if h.v >= 0x14060100: PSLoopBehavior(r.readUInt32())
        self.growTime = r.readSingle()
        self.shrinkTime = r.readSingle()
        self.growGeneration = r.readUInt16()
        self.shrinkGeneration = r.readUInt16()

# Encapsulates a floodgate kernel that simulates particle forces.
class NiPSSimulatorForcesStep(NiPSSimulatorStep):
    forces: list[int]                                   # The forces affecting the particle system.

    def __init__(self, r: Reader, h: Header):
        self.forces = r.readL32FArray(X[NiPSForce].ref)

# Encapsulates a floodgate kernel that simulates particle colliders.
class NiPSSimulatorCollidersStep(NiPSSimulatorStep):
    colliders: list[int]                                # The colliders affecting the particle system.

    def __init__(self, r: Reader, h: Header):
        self.colliders = r.readL32FArray(X[NiPSCollider].ref)

# Encapsulates a floodgate kernel that updates mesh particle alignment and transforms.
class NiPSSimulatorMeshAlignStep(NiPSSimulatorStep):
    rotationKeys: list[QuatKey[T]]                      # The particle rotation keys.
    rotationLoopBehavior: PSLoopBehavior                # The loop behavior for the rotation keys.

    def __init__(self, r: Reader, h: Header):
        self.rotationKeys = r.readL8FArray(lambda r: QuatKey[T](r, h))
        self.rotationLoopBehavior = PSLoopBehavior(r.readUInt32())

# Encapsulates a floodgate kernel that updates particle positions and ages. As indicated by its name, this step should be attached last in the NiPSSimulator mesh modifier.
class NiPSSimulatorFinalStep(NiPSSimulatorStep):

    def __init__(self, r: Reader, h: Header):

# Updates the bounding volume for an NiPSParticleSystem object.
class NiPSBoundUpdater(NiObject):
    updateSkip: int                                     # Number of particle bounds to skip updating every frame. Higher = more updates each frame.

    def __init__(self, r: Reader, h: Header):
        self.updateSkip = r.readUInt16()

# This is used by the Floodgate kernel to determine which NiPSForceHelpers functions to call.
class PSForceType(Enum):
    FORCE_BOMB = 0,
    FORCE_DRAG = 1,
    FORCE_AIR_FIELD = 2,
    FORCE_DRAG_FIELD = 3,
    FORCE_GRAVITY_FIELD = 4,
    FORCE_RADIAL_FIELD = 5,
    FORCE_TURBULENCE_FIELD = 6,
    FORCE_VORTEX_FIELD = 7,
    FORCE_GRAVITY = 8

# Abstract base class for all particle forces.
class NiPSForce(NiObject):
    name: str
    type: PSForceType
    active: bool

    def __init__(self, r: Reader, h: Header):
        self.name = Y.string(r)
        self.type = PSForceType(r.readUInt32())
        self.active = r.readBool32()

# Applies a linear drag force to particles.
class NiPSDragForce(NiPSForce):
    dragAxis: Vector3
    percentage: float
    range: float
    rangeFalloff: float
    dragObject: int

    def __init__(self, r: Reader, h: Header):
        self.dragAxis = r.readVector3()
        self.percentage = r.readSingle()
        self.range = r.readSingle()
        self.rangeFalloff = r.readSingle()
        self.dragObject = X[NiAVObject].ptr(r)

# Applies a gravitational force to particles.
class NiPSGravityForce(NiPSForce):
    gravityAxis: Vector3
    decay: float
    strength: float
    forceType: ForceType
    turbulence: float
    turbulenceScale: float
    gravityObject: int

    def __init__(self, r: Reader, h: Header):
        self.gravityAxis = r.readVector3()
        self.decay = r.readSingle()
        self.strength = r.readSingle()
        self.forceType = ForceType(r.readUInt32())
        self.turbulence = r.readSingle()
        self.turbulenceScale = r.readSingle()
        self.gravityObject = X[NiAVObject].ptr(r)

# Applies an explosive force to particles.
class NiPSBombForce(NiPSForce):
    bombAxis: Vector3
    decay: float
    deltaV: float
    decayType: DecayType
    symmetryType: SymmetryType
    bombObject: int

    def __init__(self, r: Reader, h: Header):
        self.bombAxis = r.readVector3()
        self.decay = r.readSingle()
        self.deltaV = r.readSingle()
        self.decayType = DecayType(r.readUInt32())
        self.symmetryType = SymmetryType(r.readUInt32())
        self.bombObject = X[NiAVObject].ptr(r)

# Abstract base class for all particle emitters.
class NiPSEmitter(NiObject):
    name: str
    speed: float
    speedVar: float
    speedFlipRatio: float
    declination: float
    declinationVar: float
    planarAngle: float
    planarAngleVar: float
    color: ByteColor4
    size: float
    sizeVar: float
    lifespan: float
    lifespanVar: float
    rotationAngle: float
    rotationAngleVar: float
    rotationSpeed: float
    rotationSpeedVar: float
    rotationAxis: Vector3
    randomRotSpeedSign: bool
    randomRotAxis: bool
    unknown: bool

    def __init__(self, r: Reader, h: Header):
        self.name = Y.string(r)
        self.speed = r.readSingle()
        self.speedVar = r.readSingle()
        self.speedFlipRatio = if h.v >= 0x14060100: r.readSingle()
        self.declination = r.readSingle()
        self.declinationVar = r.readSingle()
        self.planarAngle = r.readSingle()
        self.planarAngleVar = r.readSingle()
        self.color = if h.v <= 0x14060000: Color4Byte(r)
        self.size = r.readSingle()
        self.sizeVar = r.readSingle()
        self.lifespan = r.readSingle()
        self.lifespanVar = r.readSingle()
        self.rotationAngle = r.readSingle()
        self.rotationAngleVar = r.readSingle()
        self.rotationSpeed = r.readSingle()
        self.rotationSpeedVar = r.readSingle()
        self.rotationAxis = r.readVector3()
        self.randomRotSpeedSign = r.readBool32()
        self.randomRotAxis = r.readBool32()
        self.unknown = if h.v >= 0x1E000000 and h.v <= 0x1E000001: r.readBool32()

# Abstract base class for particle emitters that emit particles from a volume.
class NiPSVolumeEmitter(NiPSEmitter):
    emitterObject: int

    def __init__(self, r: Reader, h: Header):
        self.emitterObject = X[NiAVObject].ptr(r)

# A particle emitter that emits particles from a rectangular volume.
class NiPSBoxEmitter(NiPSVolumeEmitter):
    emitterWidth: float
    emitterHeight: float
    emitterDepth: float

    def __init__(self, r: Reader, h: Header):
        self.emitterWidth = r.readSingle()
        self.emitterHeight = r.readSingle()
        self.emitterDepth = r.readSingle()

# A particle emitter that emits particles from a spherical volume.
class NiPSSphereEmitter(NiPSVolumeEmitter):
    emitterRadius: float

    def __init__(self, r: Reader, h: Header):
        self.emitterRadius = r.readSingle()

# A particle emitter that emits particles from a cylindrical volume.
class NiPSCylinderEmitter(NiPSVolumeEmitter):
    emitterRadius: float
    emitterHeight: float

    def __init__(self, r: Reader, h: Header):
        self.emitterRadius = r.readSingle()
        self.emitterHeight = r.readSingle()

# Emits particles from one or more NiMesh objects. A random mesh emitter is selected for each particle emission.
class NiPSMeshEmitter(NiPSEmitter):
    meshEmitters: list[int]
    emitAxis: Vector3
    emitterObject: int
    meshEmissionType: EmitFrom
    initialVelocityType: VelocityType

    def __init__(self, r: Reader, h: Header):
        self.meshEmitters = r.readL32FArray(X[NiMesh].ptr)
        self.emitAxis = if h.v <= 0x14060000: r.readVector3()
        self.emitterObject = if h.v >= 0x14060100: X[NiAVObject].ptr(r)
        self.meshEmissionType = EmitFrom(r.readUInt32())
        self.initialVelocityType = VelocityType(r.readUInt32())

# Abstract base class for all particle emitter time controllers.
class NiPSEmitterCtlr(NiSingleInterpController):
    emitterName: str

    def __init__(self, r: Reader, h: Header):
        self.emitterName = Y.string(r)

# Abstract base class for controllers that animate a floating point value on an NiPSEmitter object.
class NiPSEmitterFloatCtlr(NiPSEmitterCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates particle emission and birth rate.
class NiPSEmitParticlesCtlr(NiPSEmitterCtlr):
    emitterActiveInterpolator: int

    def __init__(self, r: Reader, h: Header):
        self.emitterActiveInterpolator = X[NiInterpolator].ref(r)

# Abstract base class for all particle force time controllers.
class NiPSForceCtlr(NiSingleInterpController):
    forceName: str

    def __init__(self, r: Reader, h: Header):
        self.forceName = Y.string(r)

# Abstract base class for controllers that animate a Boolean value on an NiPSForce object.
class NiPSForceBoolCtlr(NiPSForceCtlr):

    def __init__(self, r: Reader, h: Header):

# Abstract base class for controllers that animate a floating point value on an NiPSForce object.
class NiPSForceFloatCtlr(NiPSForceCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates whether or not an NiPSForce object is active.
class NiPSForceActiveCtlr(NiPSForceBoolCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the strength value of an NiPSGravityForce object.
class NiPSGravityStrengthCtlr(NiPSForceFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the speed value on an NiPSEmitter object.
class NiPSEmitterSpeedCtlr(NiPSEmitterFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the size value on an NiPSEmitter object.
class NiPSEmitterRadiusCtlr(NiPSEmitterFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the declination value on an NiPSEmitter object.
class NiPSEmitterDeclinationCtlr(NiPSEmitterFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the declination variation value on an NiPSEmitter object.
class NiPSEmitterDeclinationVarCtlr(NiPSEmitterFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the planar angle value on an NiPSEmitter object.
class NiPSEmitterPlanarAngleCtlr(NiPSEmitterFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the planar angle variation value on an NiPSEmitter object.
class NiPSEmitterPlanarAngleVarCtlr(NiPSEmitterFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the rotation angle value on an NiPSEmitter object.
class NiPSEmitterRotAngleCtlr(NiPSEmitterFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the rotation angle variation value on an NiPSEmitter object.
class NiPSEmitterRotAngleVarCtlr(NiPSEmitterFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the rotation speed value on an NiPSEmitter object.
class NiPSEmitterRotSpeedCtlr(NiPSEmitterFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the rotation speed variation value on an NiPSEmitter object.
class NiPSEmitterRotSpeedVarCtlr(NiPSEmitterFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Animates the lifespan value on an NiPSEmitter object.
class NiPSEmitterLifeSpanCtlr(NiPSEmitterFloatCtlr):

    def __init__(self, r: Reader, h: Header):

# Calls ResetParticleSystem on an NiPSParticleSystem target upon looping.
class NiPSResetOnLoopCtlr(NiTimeController):

    def __init__(self, r: Reader, h: Header):

# This is used by the Floodgate kernel to determine which NiPSColliderHelpers functions to call.
class ColliderType(Enum):
    COLLIDER_PLANAR = 0,
    COLLIDER_SPHERICAL = 1

# Abstract base class for all particle colliders.
class NiPSCollider(NiObject):
    spawner: int
    type: ColliderType
    active: bool
    bounce: float
    spawnonCollide: bool
    dieonCollide: bool

    def __init__(self, r: Reader, h: Header):
        self.spawner = X[NiPSSpawner].ref(r)
        self.type = ColliderType(r.readUInt32())
        self.active = r.readBool32()
        self.bounce = r.readSingle()
        self.spawnonCollide = r.readBool32()
        self.dieonCollide = r.readBool32()

# A planar collider for particles.
class NiPSPlanarCollider(NiPSCollider):
    width: float
    height: float
    xAxis: Vector3
    yAxis: Vector3
    colliderObject: int

    def __init__(self, r: Reader, h: Header):
        self.width = r.readSingle()
        self.height = r.readSingle()
        self.xAxis = r.readVector3()
        self.yAxis = r.readVector3()
        self.colliderObject = X[NiAVObject].ptr(r)

# A spherical collider for particles.
class NiPSSphericalCollider(NiPSCollider):
    radius: float
    colliderObject: int

    def __init__(self, r: Reader, h: Header):
        self.radius = r.readSingle()
        self.colliderObject = X[NiAVObject].ptr(r)

# Creates a new particle whose initial parameters are based on an existing particle.
class NiPSSpawner(NiObject):
    masterParticleSystem: int
    percentageSpawned: float
    spawnSpeedFactor: float
    spawnSpeedFactorVar: float
    spawnDirChaos: float
    lifeSpan: float
    lifeSpanVar: float
    numSpawnGenerations: int
    mintoSpawn: int
    maxtoSpawn: int

    def __init__(self, r: Reader, h: Header):
        self.masterParticleSystem = if h.v >= 0x14060100: X[NiPSParticleSystem].ptr(r)
        self.percentageSpawned = r.readSingle()
        self.spawnSpeedFactor = if h.v >= 0x14060100: r.readSingle()
        self.spawnSpeedFactorVar = r.readSingle()
        self.spawnDirChaos = r.readSingle()
        self.lifeSpan = r.readSingle()
        self.lifeSpanVar = r.readSingle()
        self.numSpawnGenerations = r.readUInt16()
        self.mintoSpawn = r.readUInt32()
        self.maxtoSpawn = r.readUInt32()

class NiEvaluator(NiObject):
    nodeName: str                                       # The name of the animated NiAVObject.
    propertyType: str                                   # The RTTI type of the NiProperty the controller is attached to, if applicable.
    controllerType: str                                 # The RTTI type of the NiTimeController.
    controllerId: str                                   # An ID that can uniquely identify the controller among others of the same type on the same NiObjectNET.
    interpolatorId: str                                 # An ID that can uniquely identify the interpolator among others of the same type on the same NiObjectNET.
    channelTypes: list[int]                             # Channel Indices are BASE/POS = 0, ROT = 1, SCALE = 2, FLAG = 3
                                                        #     Channel Types are:
                                                        #      INVALID = 0, COLOR, BOOL, FLOAT, POINT3, ROT = 5
                                                        #     Any channel may be | 0x40 which means POSED
                                                        #     The FLAG (3) channel flags affects the whole evaluator:
                                                        #      REFERENCED = 0x1, TRANSFORM = 0x2, ALWAYSUPDATE = 0x4, SHUTDOWN = 0x8

    def __init__(self, r: Reader, h: Header):
        self.nodeName = Y.string(r)
        self.propertyType = Y.string(r)
        self.controllerType = Y.string(r)
        self.controllerId = Y.string(r)
        self.interpolatorId = Y.string(r)
        self.channelTypes = r.readBytes(4)

class NiKeyBasedEvaluator(NiEvaluator):

    def __init__(self, r: Reader, h: Header):

class NiBoolEvaluator(NiKeyBasedEvaluator):
    data: int

    def __init__(self, r: Reader, h: Header):
        self.data = X[NiBoolData].ref(r)

class NiBoolTimelineEvaluator(NiBoolEvaluator):

    def __init__(self, r: Reader, h: Header):

class NiColorEvaluator(NiKeyBasedEvaluator):
    data: int

    def __init__(self, r: Reader, h: Header):
        self.data = X[NiColorData].ref(r)

class NiFloatEvaluator(NiKeyBasedEvaluator):
    data: int

    def __init__(self, r: Reader, h: Header):
        self.data = X[NiFloatData].ref(r)

class NiPoint3Evaluator(NiKeyBasedEvaluator):
    data: int

    def __init__(self, r: Reader, h: Header):
        self.data = X[NiPosData].ref(r)

class NiQuaternionEvaluator(NiKeyBasedEvaluator):
    data: int

    def __init__(self, r: Reader, h: Header):
        self.data = X[NiRotData].ref(r)

class NiTransformEvaluator(NiKeyBasedEvaluator):
    value: NiQuatTransform
    data: int

    def __init__(self, r: Reader, h: Header):
        self.value = NiQuatTransform(r, h)
        self.data = X[NiTransformData].ref(r)

class NiConstBoolEvaluator(NiEvaluator):
    value: float

    def __init__(self, r: Reader, h: Header):
        self.value = r.readSingle()

class NiConstColorEvaluator(NiEvaluator):
    value: Color4

    def __init__(self, r: Reader, h: Header):
        self.value = Color4(r)

class NiConstFloatEvaluator(NiEvaluator):
    value: float

    def __init__(self, r: Reader, h: Header):
        self.value = r.readSingle()

class NiConstPoint3Evaluator(NiEvaluator):
    value: Vector3

    def __init__(self, r: Reader, h: Header):
        self.value = r.readVector3()

class NiConstQuaternionEvaluator(NiEvaluator):
    value: Quaternion

    def __init__(self, r: Reader, h: Header):
        self.value = r.readQuaternion()

class NiConstTransformEvaluator(NiEvaluator):
    value: NiQuatTransform

    def __init__(self, r: Reader, h: Header):
        self.value = NiQuatTransform(r, h)

class NiBSplineEvaluator(NiEvaluator):
    startTime: float
    endTime: float
    data: int
    basisData: int

    def __init__(self, r: Reader, h: Header):
        self.startTime = r.readSingle()
        self.endTime = r.readSingle()
        self.data = X[NiBSplineData].ref(r)
        self.basisData = X[NiBSplineBasisData].ref(r)

class NiBSplineColorEvaluator(NiBSplineEvaluator):
    handle: int                                         # Handle into the data. (USHRT_MAX for invalid handle.)

    def __init__(self, r: Reader, h: Header):
        self.handle = r.readUInt32()

class NiBSplineCompColorEvaluator(NiBSplineColorEvaluator):
    offset: float
    halfRange: float

    def __init__(self, r: Reader, h: Header):
        self.offset = r.readSingle()
        self.halfRange = r.readSingle()

class NiBSplineFloatEvaluator(NiBSplineEvaluator):
    handle: int                                         # Handle into the data. (USHRT_MAX for invalid handle.)

    def __init__(self, r: Reader, h: Header):
        self.handle = r.readUInt32()

class NiBSplineCompFloatEvaluator(NiBSplineFloatEvaluator):
    offset: float
    halfRange: float

    def __init__(self, r: Reader, h: Header):
        self.offset = r.readSingle()
        self.halfRange = r.readSingle()

class NiBSplinePoint3Evaluator(NiBSplineEvaluator):
    handle: int                                         # Handle into the data. (USHRT_MAX for invalid handle.)

    def __init__(self, r: Reader, h: Header):
        self.handle = r.readUInt32()

class NiBSplineCompPoint3Evaluator(NiBSplinePoint3Evaluator):
    offset: float
    halfRange: float

    def __init__(self, r: Reader, h: Header):
        self.offset = r.readSingle()
        self.halfRange = r.readSingle()

class NiBSplineTransformEvaluator(NiBSplineEvaluator):
    transform: NiQuatTransform
    translationHandle: int                              # Handle into the translation data. (USHRT_MAX for invalid handle.)
    rotationHandle: int                                 # Handle into the rotation data. (USHRT_MAX for invalid handle.)
    scaleHandle: int                                    # Handle into the scale data. (USHRT_MAX for invalid handle.)

    def __init__(self, r: Reader, h: Header):
        self.transform = NiQuatTransform(r, h)
        self.translationHandle = r.readUInt32()
        self.rotationHandle = r.readUInt32()
        self.scaleHandle = r.readUInt32()

class NiBSplineCompTransformEvaluator(NiBSplineTransformEvaluator):
    translationOffset: float
    translationHalfRange: float
    rotationOffset: float
    rotationHalfRange: float
    scaleOffset: float
    scaleHalfRange: float

    def __init__(self, r: Reader, h: Header):
        self.translationOffset = r.readSingle()
        self.translationHalfRange = r.readSingle()
        self.rotationOffset = r.readSingle()
        self.rotationHalfRange = r.readSingle()
        self.scaleOffset = r.readSingle()
        self.scaleHalfRange = r.readSingle()

class NiLookAtEvaluator(NiEvaluator):
    flags: LookAtFlags
    lookAtName: str
    drivenName: str
    interpolatorTranslation: int
    interpolatorRoll: int
    interpolatorScale: int

    def __init__(self, r: Reader, h: Header):
        self.flags = LookAtFlags(r.readUInt16())
        self.lookAtName = Y.string(r)
        self.drivenName = Y.string(r)
        self.interpolatorTranslation = X[NiPoint3Interpolator].ref(r)
        self.interpolatorRoll = X[NiFloatInterpolator].ref(r)
        self.interpolatorScale = X[NiFloatInterpolator].ref(r)

class NiPathEvaluator(NiKeyBasedEvaluator):
    flags: PathFlags
    bankDir: int                                        # -1 = Negative, 1 = Positive
    maxBankAngle: float                                 # Max angle in radians.
    smoothing: float
    followAxis: int                                     # 0, 1, or 2 representing X, Y, or Z.
    pathData: int
    percentData: int

    def __init__(self, r: Reader, h: Header):
        self.flags = PathFlags(r.readUInt16())
        self.bankDir = r.readInt32()
        self.maxBankAngle = r.readSingle()
        self.smoothing = r.readSingle()
        self.followAxis = r.readInt16()
        self.pathData = X[NiPosData].ref(r)
        self.percentData = X[NiFloatData].ref(r)

# Root node in Gamebryo .kf files (20.5.0.1 and up).
# For 20.5.0.0, "NiSequenceData" is an alias for "NiControllerSequence" and this is not handled in nifxml.
# This was not found in any 20.5.0.0 KFs available and they instead use NiControllerSequence directly.
class NiSequenceData(NiObject):
    name: str
    numControlledBlocks: int
    arrayGrowBy: int
    controlledBlocks: list[ControlledBlock]
    evaluators: list[int]
    textKeys: int
    duration: float
    cycleType: CycleType
    frequency: float
    accumRootName: str                                  # The name of the NiAVObject serving as the accumulation root. This is where all accumulated translations, scales, and rotations are applied.
    accumFlags: AccumFlags

    def __init__(self, r: Reader, h: Header):
        self.name = Y.string(r)
        self.numControlledBlocks = if h.v <= 0x14050001: r.readUInt32()
        self.arrayGrowBy = if h.v <= 0x14050001: r.readUInt32()
        self.controlledBlocks = if h.v <= 0x14050001: r.readFArray(lambda r: ControlledBlock(r, h), NumControlledBlocks)
        self.evaluators = if h.v >= 0x14050002: r.readL32FArray(X[NiEvaluator].ref)
        self.textKeys = X[NiTextKeyExtraData].ref(r)
        self.duration = r.readSingle()
        self.cycleType = CycleType(r.readUInt32())
        self.frequency = r.readSingle()
        self.accumRootName = Y.string(r)
        self.accumFlags = AccumFlags(r.readUInt32())

# An NiShadowGenerator object is attached to an NiDynamicEffect object to inform the shadowing system that the effect produces shadows.
class NiShadowGenerator(NiObject):
    name: str
    flags: int
    shadowCasters: list[int]
    shadowReceivers: list[int]
    target: int
    depthBias: float
    sizeHint: int
    nearClippingDistance: float
    farClippingDistance: float
    directionalLightFrustumWidth: float

    def __init__(self, r: Reader, h: Header):
        self.name = Y.string(r)
        self.flags = r.readUInt16()
        self.shadowCasters = r.readL32FArray(X[NiNode].ref)
        self.shadowReceivers = r.readL32FArray(X[NiNode].ref)
        self.target = X[NiDynamicEffect].ptr(r)
        self.depthBias = r.readSingle()
        self.sizeHint = r.readUInt16()
        self.nearClippingDistance = if h.v >= 0x14030007: r.readSingle()
        self.farClippingDistance = if h.v >= 0x14030007: r.readSingle()
        self.directionalLightFrustumWidth = if h.v >= 0x14030007: r.readSingle()

class NiFurSpringController(NiTimeController):
    unknownFloat: float
    unknownFloat2: float
    bones: list[int]                                    # List of all armature bones.
    bones2: list[int]                                   # List of all armature bones.

    def __init__(self, r: Reader, h: Header):
        self.unknownFloat = r.readSingle()
        self.unknownFloat2 = r.readSingle()
        self.bones = r.readL32FArray(X[NiNode].ptr)
        self.bones2 = r.readL32FArray(X[NiNode].ptr)

class CStreamableAssetData(NiObject):
    root: int
    unknownBytes: list[int]

    def __init__(self, r: Reader, h: Header):
        self.root = X[NiNode].ref(r)
        self.unknownBytes = r.readBytes(5)

# Compressed collision mesh.
class bhkCompressedMeshShape(bhkShape):
    target: int                                         # Points to root node?
    userData: int                                       # Unknown.
    radius: float                                       # A shell that is added around the shape.
    unknownFloat1: float                                # Unknown.
    scale: Vector4                                      # Scale
    radiusCopy: float                                   # A shell that is added around the shape.
    scaleCopy: Vector4                                  # Scale
    data: int                                           # The collision mesh data.

    def __init__(self, r: Reader, h: Header):
        self.target = X[NiAVObject].ptr(r)
        self.userData = r.readUInt32()
        self.radius = r.readSingle()
        self.unknownFloat1 = r.readSingle()
        self.scale = r.readVector4()
        self.radiusCopy = r.readSingle()
        self.scaleCopy = r.readVector4()
        self.data = X[bhkCompressedMeshShapeData].ref(r)

# A compressed mesh shape for collision in Skyrim.
class bhkCompressedMeshShapeData(bhkRefObject):
    bitsPerIndex: int                                   # Number of bits in the shape-key reserved for a triangle index
    bitsPerWIndex: int                                  # Number of bits in the shape-key reserved for a triangle index and its winding
    maskWIndex: int                                     # Mask used to get the triangle index and winding from a shape-key (common: 262143 = 0x3ffff)
    maskIndex: int                                      # Mask used to get the triangle index from a shape-key (common: 131071 = 0x1ffff)
    error: float                                        # The radius of the storage mesh shape? Quantization error?
    boundsMin: Vector4                                  # The minimum boundary of the AABB (the coordinates of the corner with the lowest numerical values)
    boundsMax: Vector4                                  # The maximum boundary of the AABB (the coordinates of the corner with the highest numerical values)
    weldingType: int
    materialType: int
    materials32: list[int]                              # Does not appear to be used.
    materials16: list[int]                              # Does not appear to be used.
    materials8: list[int]                               # Does not appear to be used.
    chunkMaterials: list[bhkCMSDMaterial]               # Table (array) with sets of materials. Chunks refers to this table by index.
    numNamedMaterials: int
    chunkTransforms: list[bhkCMSDTransform]             # Table (array) with sets of transformations. Chunks refers to this table by index.
    bigVerts: list[Vector4]                             # Compressed Vertices?
    bigTris: list[bhkCMSDBigTris]
    chunks: list[bhkCMSDChunk]
    numConvexPieceA: int                                # Does not appear to be used. Needs array.

    def __init__(self, r: Reader, h: Header):
        self.bitsPerIndex = r.readUInt32()
        self.bitsPerWIndex = r.readUInt32()
        self.maskWIndex = r.readUInt32()
        self.maskIndex = r.readUInt32()
        self.error = r.readSingle()
        self.boundsMin = r.readVector4()
        self.boundsMax = r.readVector4()
        self.weldingType = r.readByte()
        self.materialType = r.readByte()
        self.materials32 = r.readL32PArray(None, 'I')
        self.materials16 = r.readL32PArray(None, 'I')
        self.materials8 = r.readL32PArray(None, 'I')
        self.chunkMaterials = r.readL32FArray(lambda r: bhkCMSDMaterial(r))
        self.numNamedMaterials = r.readUInt32()
        self.chunkTransforms = r.readL32FArray(lambda r: bhkCMSDTransform(r))
        self.bigVerts = r.readL32FArray(lambda r: r.readL32Vector4())
        self.bigTris = r.readL32FArray(lambda r: bhkCMSDBigTris(r))
        self.chunks = r.readL32FArray(lambda r: bhkCMSDChunk(r))
        self.numConvexPieceA = r.readUInt32()

# Orientation marker for Skyrim's inventory view.
# How to show the nif in the player's inventory.
# Typically attached to the root node of the nif tree.
# If not present, then Skyrim will still show the nif in inventory,
# using the default values.
# Name should be 'INV' (without the quotes).
# For rotations, a short of "4712" appears as "4.712" but "959" appears as "0.959"  meshes\weapons\daedric\daedricbowskinned.nif
class BSInvMarker(NiExtraData):
    rotationX: int
    rotationY: int
    rotationZ: int
    zoom: float                                         # Zoom factor.

    def __init__(self, r: Reader, h: Header):
        self.rotationX = r.readUInt16()
        self.rotationY = r.readUInt16()
        self.rotationZ = r.readUInt16()
        self.zoom = r.readSingle()

# Unknown
class BSBoneLODExtraData(NiExtraData):
    boneLodCount: int                                   # Number of bone entries
    boneLodInfo: list[BoneLOD]                          # Bone Entry

    def __init__(self, r: Reader, h: Header):
        self.boneLodCount = r.readUInt32()
        self.boneLodInfo = r.readFArray(lambda r: BoneLOD(r), BoneLODCount)

# Links a nif with a Havok Behavior .hkx animation file
class BSBehaviorGraphExtraData(NiExtraData):
    behaviourGraphFile: str                             # Name of the hkx file.
    controlsBaseSkeleton: bool                          # Unknown, has to do with blending appended bones onto an actor.

    def __init__(self, r: Reader, h: Header):
        self.behaviourGraphFile = Y.string(r)
        self.controlsBaseSkeleton = r.readBool32()

# A controller that trails a bone behind an actor.
class BSLagBoneController(NiTimeController):
    linearVelocity: float                               # How long it takes to rotate about an actor back to rest position.
    linearRotation: float                               # How the bone lags rotation
    maximumDistance: float                              # How far bone will tail an actor.

    def __init__(self, r: Reader, h: Header):
        self.linearVelocity = r.readSingle()
        self.linearRotation = r.readSingle()
        self.maximumDistance = r.readSingle()

# A variation on NiTriShape, for visibility control over vertex groups.
class BSLODTriShape(NiTriBasedGeom):
    loD0Size: int
    loD1Size: int
    loD2Size: int

    def __init__(self, r: Reader, h: Header):
        self.loD0Size = r.readUInt32()
        self.loD1Size = r.readUInt32()
        self.loD2Size = r.readUInt32()

# Furniture Marker for actors
class BSFurnitureMarkerNode(BSFurnitureMarker):

    def __init__(self, r: Reader, h: Header):

# Unknown, related to trees.
class BSLeafAnimNode(NiNode):

    def __init__(self, r: Reader, h: Header):

# Node for handling Trees, Switches branch configurations for variation?
class BSTreeNode(NiNode):
    bones1: list[int]                                   # Unknown
    bones: list[int]                                    # Unknown

    def __init__(self, r: Reader, h: Header):
        self.bones1 = r.readL32FArray(X[NiNode].ref)
        self.bones = r.readL32FArray(X[NiNode].ref)

# Fallout 4 Tri Shape
class BSTriShape(NiAVObject):
    boundingSphere: NiBound
    skin: int
    shaderProperty: int
    alphaProperty: int
    vertexDesc: BSVertexDesc
    numTriangles: int
    numVertices: int
    dataSize: int
    vertexData: list[BSVertexDataSSE]
    triangles: list[Triangle]
    particleDataSize: int
    vertices: list[Vector3]
    trianglesCopy: list[Triangle]

    def __init__(self, r: Reader, h: Header):
        self.boundingSphere = NiBound(r)
        self.skin = X[NiObject].ref(r)
        self.shaderProperty = X[BSShaderProperty].ref(r)
        self.alphaProperty = X[NiAlphaProperty].ref(r)
        self.vertexDesc = BSVertexDesc(r)
        self.numTriangles = if (h.uv2 == 130): r.readUInt32()
        self.numTriangles = r.readUInt16()
        self.numVertices = r.readUInt16()
        self.dataSize = r.readUInt32()
        self.vertexData = if Data Size > 0 and (h.uv2 == 130): r.readFArray(lambda r: BSVertexData(r, h), NumVertices)
        self.vertexData = if Data Size > 0 and (h.uv2 == 100): r.readFArray(lambda r: BSVertexDataSSE(r, h), NumVertices)
        self.triangles = if Data Size > 0: r.readFArray(lambda r: Triangle(r), NumTriangles)
        self.particleDataSize = if (h.uv2 == 100): r.readUInt32()
        self.vertices = if Particle Data Size > 0 and (h.uv2 == 100): r.readFArray(lambda r: r.readVector3(), NumVertices)
        self.trianglesCopy = if Particle Data Size > 0 and (h.uv2 == 100): r.readFArray(lambda r: Triangle(r), NumTriangles)

# Fallout 4 LOD Tri Shape
class BSMeshLODTriShape(BSTriShape):
    loD0Size: int
    loD1Size: int
    loD2Size: int

    def __init__(self, r: Reader, h: Header):
        self.loD0Size = r.readUInt32()
        self.loD1Size = r.readUInt32()
        self.loD2Size = r.readUInt32()

class BSGeometryPerSegmentSharedData:
    def __init__(self, r: Reader):
        self.userIndex: int = r.readUInt32()            # If Bone ID is 0xffffffff, this value refers to the Segment at the listed index. Otherwise this is the "Biped Object", which is like the body part types in Skyrim and earlier.
        self.boneId: int = r.readUInt32()               # A hash of the bone name string.
        self.cutOffsets: list[float] = r.readL32PArray(None, 'f')

class BSGeometrySegmentSharedData:
    def __init__(self, r: Reader):
        self.numSegments: int = r.readUInt32()
        self.totalSegments: int = r.readUInt32()
        self.segmentStarts: list[int] = r.readPArray(None, 'I', NumSegments)
        self.perSegmentData: list[BSGeometryPerSegmentSharedData] = r.readFArray(lambda r: BSGeometryPerSegmentSharedData(r), TotalSegments)
        self.ssfLength: int = r.readUInt16()
        self.ssfFile: list[int] = r.readBytes(SSFLength)

# Fallout 4 Sub-Index Tri Shape
class BSSubIndexTriShape(BSTriShape):
    numPrimitives: int
    numSegments: int
    totalSegments: int
    segment: list[BSGeometrySegmentData]
    segmentData: BSGeometrySegmentSharedData

    def __init__(self, r: Reader, h: Header):
        self.numPrimitives = if Data Size > 0 and (h.uv2 == 130): r.readUInt32()
        self.numSegments = if Data Size > 0 and (h.uv2 == 130): r.readUInt32()
        self.totalSegments = if Data Size > 0 and (h.uv2 == 130): r.readUInt32()
        self.segment = if Data Size > 0 and (h.uv2 == 130): r.readFArray(lambda r: BSGeometrySegmentData(r, h), NumSegments)
        self.segmentData = if (Num Segments < Total Segments) and (Data Size > 0) and (h.uv2 == 130): BSGeometrySegmentSharedData(r)
        self.numSegments = if (h.uv2 == 100): r.readUInt32()
        self.segment = if (h.uv2 == 100): r.readFArray(lambda r: BSGeometrySegmentData(r, h), NumSegments)

# Fallout 4 Physics System
class bhkSystem(NiObject):

    def __init__(self, r: Reader, h: Header):

# Fallout 4 Collision Object
class bhkNPCollisionObject(NiCollisionObject):
    flags: int                                          # Due to inaccurate reporting in the CK the Reset and Sync On Update positions are a guess.
                                                        #     Bits: 0=Reset, 2=Notify, 3=SetLocal, 7=SyncOnUpdate, 10=AnimTargeted
    data: int
    bodyId: int

    def __init__(self, r: Reader, h: Header):
        self.flags = r.readUInt16()
        self.data = X[bhkSystem].ref(r)
        self.bodyId = r.readUInt32()

# Fallout 4 Collision System
class bhkPhysicsSystem(bhkSystem):
    binaryData: bytearray

    def __init__(self, r: Reader, h: Header):
        self.binaryData = r.readL8Bytes()

# Fallout 4 Ragdoll System
class bhkRagdollSystem(bhkSystem):
    binaryData: bytearray

    def __init__(self, r: Reader, h: Header):
        self.binaryData = r.readL8Bytes()

# Fallout 4 Extra Data
class BSExtraData(NiExtraData):

    def __init__(self, r: Reader, h: Header):

# Fallout 4 Cloth data
class BSClothExtraData(BSExtraData):
    binaryData: bytearray

    def __init__(self, r: Reader, h: Header):
        self.binaryData = r.readL8Bytes()

# Fallout 4 Bone Transform
class BSSkinBoneTrans:
    def __init__(self, r: Reader):
        self.boundingSphere: NiBound = NiBound(r)
        self.rotation: Matrix3x3 = r.readMatrix3x3()
        self.translation: Vector3 = r.readVector3()
        self.scale: float = r.readSingle()

# Fallout 4 Skin Instance
class BSSkin::Instance(NiObject):
    skeletonRoot: int
    data: int
    bones: list[int]
    unknown: list[Vector3]

    def __init__(self, r: Reader, h: Header):
        self.skeletonRoot = X[NiAVObject].ptr(r)
        self.data = X[BSSkin::BoneData].ref(r)
        self.bones = r.readL32FArray(X[NiNode].ptr)
        self.unknown = r.readL32FArray(lambda r: r.readL32Vector3())

# Fallout 4 Bone Data
class BSSkin::BoneData(NiObject):
    boneList: list[BSSkinBoneTrans]

    def __init__(self, r: Reader, h: Header):
        self.boneList = r.readL32FArray(lambda r: BSSkinBoneTrans(r))

# Fallout 4 Positional Data
class BSPositionData(NiExtraData):
    data: list[float]

    def __init__(self, r: Reader, h: Header):
        self.data = r.readL32FArray(lambda r: r.readL32Half())

class BSConnectPoint:
    def __init__(self, r: Reader):
        self.parent: str = r.readL32AString()
        self.name: str = r.readL32AString()
        self.rotation: Quaternion = r.readQuaternion()
        self.translation: Vector3 = r.readVector3()
        self.scale: float = r.readSingle()

# Fallout 4 Item Slot Parent
class BSConnectPoint::Parents(NiExtraData):
    connectPoints: list[BSConnectPoint]

    def __init__(self, r: Reader, h: Header):
        self.connectPoints = r.readL32FArray(lambda r: BSConnectPoint(r))

# Fallout 4 Item Slot Child
class BSConnectPoint::Children(NiExtraData):
    skinned: bool
    name: list[str]

    def __init__(self, r: Reader, h: Header):
        self.skinned = r.readBool32()
        self.name = r.readL32FArray(lambda r: SizedString(r))

# Fallout 4 Eye Center Data
class BSEyeCenterExtraData(NiExtraData):
    data: list[float]

    def __init__(self, r: Reader, h: Header):
        self.data = r.readL32PArray(None, 'f')

class BSPackedGeomDataCombined:
    def __init__(self, r: Reader):
        self.grayscaletoPaletteScale: float = r.readSingle()
        self.transform: NiTransform = NiTransform(r)
        self.boundingSphere: NiBound = NiBound(r)

class BSPackedGeomData:
    def __init__(self, r: Reader, h: Header):
        self.numVerts: int = r.readUInt32()
        self.lodLevels: int = r.readUInt32()
        self.triCountLoD0: int = r.readUInt32()
        self.triOffsetLoD0: int = r.readUInt32()
        self.triCountLoD1: int = r.readUInt32()
        self.triOffsetLoD1: int = r.readUInt32()
        self.triCountLoD2: int = r.readUInt32()
        self.triOffsetLoD2: int = r.readUInt32()
        self.combined: list[BSPackedGeomDataCombined] = r.readL32FArray(lambda r: BSPackedGeomDataCombined(r))
        self.vertexDesc: BSVertexDesc = BSVertexDesc(r)
        self.vertexData: list[BSVertexData] = r.readFArray(lambda r: BSVertexData(r, h), NumVerts) if !BSPackedCombinedSharedGeomDataExtra else None
        self.triangles: list[Triangle] = r.readFArray(lambda r: Triangle(r), TriCountLOD0+TriCountLOD1+TriCountLOD2) if !BSPackedCombinedSharedGeomDataExtra else None

# This appears to be a 64-bit hash but nif.xml does not have a 64-bit type.
class BSPackedGeomObject:
    def __init__(self, r: Reader):
        self.shapeID1: int = r.readUInt32()
        self.shapeID2: int = r.readUInt32()

# Fallout 4 Packed Combined Geometry Data.
# Geometry is baked into the file and given a list of transforms to position each copy.
class BSPackedCombinedGeomDataExtra(NiExtraData):
    vertexDesc: BSVertexDesc
    numVertices: int
    numTriangles: int
    unknownFlags1: int
    unknownFlags2: int
    numData: int
    object: list[BSPackedGeomObject]
    objectData: list[BSPackedGeomData]

    def __init__(self, r: Reader, h: Header):
        self.vertexDesc = BSVertexDesc(r)
        self.numVertices = r.readUInt32()
        self.numTriangles = r.readUInt32()
        self.unknownFlags1 = r.readUInt32()
        self.unknownFlags2 = r.readUInt32()
        self.numData = r.readUInt32()
        self.object = if BSPackedCombinedSharedGeomDataExtra: r.readFArray(lambda r: BSPackedGeomObject(r), NumData)
        self.objectData = r.readFArray(lambda r: BSPackedGeomData(r, h), NumData)

# Fallout 4 Packed Combined Shared Geometry Data.
# Geometry is NOT baked into the file. It is instead a reference to the shape via a Shape ID (currently undecoded)
# which loads the geometry via the STAT form for the NIF.
class BSPackedCombinedSharedGeomDataExtra(BSPackedCombinedGeomDataExtra):

    def __init__(self, r: Reader, h: Header):

class NiLightRadiusController(NiFloatInterpController):

    def __init__(self, r: Reader, h: Header):

class BSDynamicTriShape(BSTriShape):
    vertexDataSize: int
    vertices: list[Vector4]

    def __init__(self, r: Reader, h: Header):
        self.vertexDataSize = r.readUInt32()
        self.vertices = if Vertex Data Size > 0: r.readFArray(lambda r: r.readVector4(), NumVertices)

#endregion

# Large ref flag.
class BSDistantObjectLargeRefExtraData(NiExtraData):
    largeRef: bool

    def __init__(self, r: Reader, h: Header):
        self.largeRef = r.readBool32()

