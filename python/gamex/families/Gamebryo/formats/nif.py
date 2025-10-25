import os
from io import BytesIO
from enum import Enum, Flag, IntFlag
from typing import TypeVar, Generic
from numpy import ndarray, array
from openstk.poly import Reader, log
from gamex import FileSource, PakBinaryT, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo
from gamex.core.globalx import Color3, Color4
from gamex.core.desser import DesSer

T = TypeVar('T')

# types
type byte = int
type Vector3 = ndarray
type Vector4 = ndarray
type Matrix2x2 = ndarray
type Matrix3x4 = ndarray
type Matrix4x4 = ndarray
type Quaternion = ndarray

# typedefs
class Color: pass
class UnionBV: pass
class NiReader: pass
class NiObject: pass

#region X

class Ref(Generic[T]):
    def __init__(self, r: NiReader, v: int): self.v: int = v; self.val: T = None
    def value() -> T: return self.val
class X(Generic[T]):
    @staticmethod # Refers to an object before the current one in the hierarchy.
    def ptr(r: Reader): return None if (v := r.readInt32()) < 0 else Ref(r, v)
    @staticmethod # Refers to an object after the current one in the hierarchy.
    def ref(r: Reader): return None if (v := r.readInt32()) < 0 else Ref(r, v)
class Z:
    @staticmethod
    def readBlocks(s, r: NiReader) -> list[object]:
        pass
    @staticmethod
    def read(s, r: NiReader) -> object:
        match s.t:
            case '[float]': return r.readSingle()
            case '[byte]': return r.readByte()
            case '[str]': return r.readL32AString()
            case '[Vector3]': return r.readVector3()
            case '[Quaternion]': return r.readQuaternionWFirst()
            case '[Color4]': return Color4(r)
            case _: raise NotImplementedError(f'Tried to read an unsupported type: {s.t}')
    @staticmethod
    def readBool8(r: NiReader) -> int: r.readByte() if r.v > 0x04000002 else r.readUInt32()
    @staticmethod
    def readBool(r: NiReader) -> bool: r.readByte() != 0 if r.v > 0x04000002 else r.readUInt32() != 0
    @staticmethod
    def string(r: NiReader) -> str: return r.readL32AString() if r.v < 0x14010003 else None
    @staticmethod
    def stringRef(r: NiReader, p: int) -> str: return None
    @staticmethod
    def isVersionSupported(v: int) -> bool: return True
    @staticmethod
    def parseHeaderStr(s: str) -> tuple:
        p = s.index('Version')
        if p >= 0:
            v = s
            v = v[(p + 8):]
            for i in range(len(v)):
                if v[i].isdigit() or v[i] == '.': continue
                else: v = v[:i]
            ver = Z.ver2Num(v)
            if not Z.isVersionSupported(ver): raise Exception(f'Version {Z.ver2Str(ver)} ({ver}) is not supported.')
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

class Flags(IntFlag):
    Hidden = 0x1

def RefJsonConverter(s): return f'{s.v}'
def TexCoordJsonConverter(s): return f'{s.u:.9g} {s.v:.9g}'
def TriangleJsonConverter(s): return f'{s.v1} {s.v2} {s.v3}'
DesSer.add({'Ref':RefJsonConverter, 'TexCoord':TexCoordJsonConverter, 'Triangle':TriangleJsonConverter})

#endregion

#region Enums

# Describes the options for the accum root on NiControllerSequence.
class AccumFlags(Flag): # Z
    ACCUM_X_TRANS = 0               # X Translation will be accumulated.
    ACCUM_Y_TRANS = 1 << 1          # Y Translation will be accumulated.
    ACCUM_Z_TRANS = 1 << 2          # Z Translation will be accumulated.
    ACCUM_X_ROT = 1 << 3            # X Rotation will be accumulated.
    ACCUM_Y_ROT = 1 << 4            # Y Rotation will be accumulated.
    ACCUM_Z_ROT = 1 << 5            # Z Rotation will be accumulated.
    ACCUM_X_FRONT = 1 << 6          # +X is front facing. (Default)
    ACCUM_Y_FRONT = 1 << 7          # +Y is front facing.
    ACCUM_Z_FRONT = 1 << 8          # +Z is front facing.
    ACCUM_NEG_FRONT = 1 << 9        # -X is front facing.

# Describes how the vertex colors are blended with the filtered texture color.
class ApplyMode(Enum): # X
    APPLY_REPLACE = 0               # Replaces existing color
    APPLY_DECAL = 1                 # For placing images on the object like stickers.
    APPLY_MODULATE = 2              # Modulates existing color. (Default)
    APPLY_HILIGHT = 3               # PS2 Only.  Function Unknown.
    APPLY_HILIGHT2 = 4              # Parallax Flag in some Oblivion meshes.

# The type of animation interpolation (blending) that will be used on the associated key frames.
class KeyType(Enum): # X
    LINEAR_KEY = 1                  # Use linear interpolation.
    QUADRATIC_KEY = 2               # Use quadratic interpolation.  Forward and back tangents will be stored.
    TBC_KEY = 3                     # Use Tension Bias Continuity interpolation.  Tension, bias, and continuity will be stored.
    XYZ_ROTATION_KEY = 4            # For use only with rotation data.  Separate X, Y, and Z keys will be stored instead of using quaternions.
    CONST_KEY = 5                   # Step function. Used for visibility keys in NiBoolData.

# Bethesda Havok. Material descriptor for a Havok shape in Oblivion.
class OblivionHavokMaterial(Enum): # Z
    OB_HAV_MAT_STONE = 0            # Stone
    OB_HAV_MAT_CLOTH = 1            # Cloth
    OB_HAV_MAT_DIRT = 2             # Dirt
    OB_HAV_MAT_GLASS = 3            # Glass
    OB_HAV_MAT_GRASS = 4            # Grass
    OB_HAV_MAT_METAL = 5            # Metal
    OB_HAV_MAT_ORGANIC = 6          # Organic
    OB_HAV_MAT_SKIN = 7             # Skin
    OB_HAV_MAT_WATER = 8            # Water
    OB_HAV_MAT_WOOD = 9             # Wood
    OB_HAV_MAT_HEAVY_STONE = 10     # Heavy Stone
    OB_HAV_MAT_HEAVY_METAL = 11     # Heavy Metal
    OB_HAV_MAT_HEAVY_WOOD = 12      # Heavy Wood
    OB_HAV_MAT_CHAIN = 13           # Chain
    OB_HAV_MAT_SNOW = 14            # Snow
    OB_HAV_MAT_STONE_STAIRS = 15    # Stone Stairs
    OB_HAV_MAT_CLOTH_STAIRS = 16    # Cloth Stairs
    OB_HAV_MAT_DIRT_STAIRS = 17     # Dirt Stairs
    OB_HAV_MAT_GLASS_STAIRS = 18    # Glass Stairs
    OB_HAV_MAT_GRASS_STAIRS = 19    # Grass Stairs
    OB_HAV_MAT_METAL_STAIRS = 20    # Metal Stairs
    OB_HAV_MAT_ORGANIC_STAIRS = 21  # Organic Stairs
    OB_HAV_MAT_SKIN_STAIRS = 22     # Skin Stairs
    OB_HAV_MAT_WATER_STAIRS = 23    # Water Stairs
    OB_HAV_MAT_WOOD_STAIRS = 24     # Wood Stairs
    OB_HAV_MAT_HEAVY_STONE_STAIRS = 25 # Heavy Stone Stairs
    OB_HAV_MAT_HEAVY_METAL_STAIRS = 26 # Heavy Metal Stairs
    OB_HAV_MAT_HEAVY_WOOD_STAIRS = 27 # Heavy Wood Stairs
    OB_HAV_MAT_CHAIN_STAIRS = 28    # Chain Stairs
    OB_HAV_MAT_SNOW_STAIRS = 29     # Snow Stairs
    OB_HAV_MAT_ELEVATOR = 30        # Elevator
    OB_HAV_MAT_RUBBER = 31          # Rubber

# Bethesda Havok. Material descriptor for a Havok shape in Fallout 3 and Fallout NV.
class Fallout3HavokMaterial(Enum): # Z
    FO_HAV_MAT_STONE = 0            # Stone
    FO_HAV_MAT_CLOTH = 1            # Cloth
    FO_HAV_MAT_DIRT = 2             # Dirt
    FO_HAV_MAT_GLASS = 3            # Glass
    FO_HAV_MAT_GRASS = 4            # Grass
    FO_HAV_MAT_METAL = 5            # Metal
    FO_HAV_MAT_ORGANIC = 6          # Organic
    FO_HAV_MAT_SKIN = 7             # Skin
    FO_HAV_MAT_WATER = 8            # Water
    FO_HAV_MAT_WOOD = 9             # Wood
    FO_HAV_MAT_HEAVY_STONE = 10     # Heavy Stone
    FO_HAV_MAT_HEAVY_METAL = 11     # Heavy Metal
    FO_HAV_MAT_HEAVY_WOOD = 12      # Heavy Wood
    FO_HAV_MAT_CHAIN = 13           # Chain
    FO_HAV_MAT_BOTTLECAP = 14       # Bottlecap
    FO_HAV_MAT_ELEVATOR = 15        # Elevator
    FO_HAV_MAT_HOLLOW_METAL = 16    # Hollow Metal
    FO_HAV_MAT_SHEET_METAL = 17     # Sheet Metal
    FO_HAV_MAT_SAND = 18            # Sand
    FO_HAV_MAT_BROKEN_CONCRETE = 19 # Broken Concrete
    FO_HAV_MAT_VEHICLE_BODY = 20    # Vehicle Body
    FO_HAV_MAT_VEHICLE_PART_SOLID = 21 # Vehicle Part Solid
    FO_HAV_MAT_VEHICLE_PART_HOLLOW = 22 # Vehicle Part Hollow
    FO_HAV_MAT_BARREL = 23          # Barrel
    FO_HAV_MAT_BOTTLE = 24          # Bottle
    FO_HAV_MAT_SODA_CAN = 25        # Soda Can
    FO_HAV_MAT_PISTOL = 26          # Pistol
    FO_HAV_MAT_RIFLE = 27           # Rifle
    FO_HAV_MAT_SHOPPING_CART = 28   # Shopping Cart
    FO_HAV_MAT_LUNCHBOX = 29        # Lunchbox
    FO_HAV_MAT_BABY_RATTLE = 30     # Baby Rattle
    FO_HAV_MAT_RUBBER_BALL = 31     # Rubber Ball
    FO_HAV_MAT_STONE_PLATFORM = 32  # Stone
    FO_HAV_MAT_CLOTH_PLATFORM = 33  # Cloth
    FO_HAV_MAT_DIRT_PLATFORM = 34   # Dirt
    FO_HAV_MAT_GLASS_PLATFORM = 35  # Glass
    FO_HAV_MAT_GRASS_PLATFORM = 36  # Grass
    FO_HAV_MAT_METAL_PLATFORM = 37  # Metal
    FO_HAV_MAT_ORGANIC_PLATFORM = 38# Organic
    FO_HAV_MAT_SKIN_PLATFORM = 39   # Skin
    FO_HAV_MAT_WATER_PLATFORM = 40  # Water
    FO_HAV_MAT_WOOD_PLATFORM = 41   # Wood
    FO_HAV_MAT_HEAVY_STONE_PLATFORM = 42 # Heavy Stone
    FO_HAV_MAT_HEAVY_METAL_PLATFORM = 43 # Heavy Metal
    FO_HAV_MAT_HEAVY_WOOD_PLATFORM = 44 # Heavy Wood
    FO_HAV_MAT_CHAIN_PLATFORM = 45  # Chain
    FO_HAV_MAT_BOTTLECAP_PLATFORM = 46 # Bottlecap
    FO_HAV_MAT_ELEVATOR_PLATFORM = 47 # Elevator
    FO_HAV_MAT_HOLLOW_METAL_PLATFORM = 48 # Hollow Metal
    FO_HAV_MAT_SHEET_METAL_PLATFORM = 49 # Sheet Metal
    FO_HAV_MAT_SAND_PLATFORM = 50   # Sand
    FO_HAV_MAT_BROKEN_CONCRETE_PLATFORM = 51 # Broken Concrete
    FO_HAV_MAT_VEHICLE_BODY_PLATFORM = 52 # Vehicle Body
    FO_HAV_MAT_VEHICLE_PART_SOLID_PLATFORM = 53 # Vehicle Part Solid
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_PLATFORM = 54 # Vehicle Part Hollow
    FO_HAV_MAT_BARREL_PLATFORM = 55 # Barrel
    FO_HAV_MAT_BOTTLE_PLATFORM = 56 # Bottle
    FO_HAV_MAT_SODA_CAN_PLATFORM = 57 # Soda Can
    FO_HAV_MAT_PISTOL_PLATFORM = 58 # Pistol
    FO_HAV_MAT_RIFLE_PLATFORM = 59  # Rifle
    FO_HAV_MAT_SHOPPING_CART_PLATFORM = 60 # Shopping Cart
    FO_HAV_MAT_LUNCHBOX_PLATFORM = 61 # Lunchbox
    FO_HAV_MAT_BABY_RATTLE_PLATFORM = 62 # Baby Rattle
    FO_HAV_MAT_RUBBER_BALL_PLATFORM = 63 # Rubber Ball
    FO_HAV_MAT_STONE_STAIRS = 64    # Stone
    FO_HAV_MAT_CLOTH_STAIRS = 65    # Cloth
    FO_HAV_MAT_DIRT_STAIRS = 66     # Dirt
    FO_HAV_MAT_GLASS_STAIRS = 67    # Glass
    FO_HAV_MAT_GRASS_STAIRS = 68    # Grass
    FO_HAV_MAT_METAL_STAIRS = 69    # Metal
    FO_HAV_MAT_ORGANIC_STAIRS = 70  # Organic
    FO_HAV_MAT_SKIN_STAIRS = 71     # Skin
    FO_HAV_MAT_WATER_STAIRS = 72    # Water
    FO_HAV_MAT_WOOD_STAIRS = 73     # Wood
    FO_HAV_MAT_HEAVY_STONE_STAIRS = 74 # Heavy Stone
    FO_HAV_MAT_HEAVY_METAL_STAIRS = 75 # Heavy Metal
    FO_HAV_MAT_HEAVY_WOOD_STAIRS = 76 # Heavy Wood
    FO_HAV_MAT_CHAIN_STAIRS = 77    # Chain
    FO_HAV_MAT_BOTTLECAP_STAIRS = 78# Bottlecap
    FO_HAV_MAT_ELEVATOR_STAIRS = 79 # Elevator
    FO_HAV_MAT_HOLLOW_METAL_STAIRS = 80 # Hollow Metal
    FO_HAV_MAT_SHEET_METAL_STAIRS = 81 # Sheet Metal
    FO_HAV_MAT_SAND_STAIRS = 82     # Sand
    FO_HAV_MAT_BROKEN_CONCRETE_STAIRS = 83 # Broken Concrete
    FO_HAV_MAT_VEHICLE_BODY_STAIRS = 84 # Vehicle Body
    FO_HAV_MAT_VEHICLE_PART_SOLID_STAIRS = 85 # Vehicle Part Solid
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_STAIRS = 86 # Vehicle Part Hollow
    FO_HAV_MAT_BARREL_STAIRS = 87   # Barrel
    FO_HAV_MAT_BOTTLE_STAIRS = 88   # Bottle
    FO_HAV_MAT_SODA_CAN_STAIRS = 89 # Soda Can
    FO_HAV_MAT_PISTOL_STAIRS = 90   # Pistol
    FO_HAV_MAT_RIFLE_STAIRS = 91    # Rifle
    FO_HAV_MAT_SHOPPING_CART_STAIRS = 92 # Shopping Cart
    FO_HAV_MAT_LUNCHBOX_STAIRS = 93 # Lunchbox
    FO_HAV_MAT_BABY_RATTLE_STAIRS = 94 # Baby Rattle
    FO_HAV_MAT_RUBBER_BALL_STAIRS = 95 # Rubber Ball
    FO_HAV_MAT_STONE_STAIRS_PLATFORM = 96 # Stone
    FO_HAV_MAT_CLOTH_STAIRS_PLATFORM = 97 # Cloth
    FO_HAV_MAT_DIRT_STAIRS_PLATFORM = 98 # Dirt
    FO_HAV_MAT_GLASS_STAIRS_PLATFORM = 99 # Glass
    FO_HAV_MAT_GRASS_STAIRS_PLATFORM = 100 # Grass
    FO_HAV_MAT_METAL_STAIRS_PLATFORM = 101 # Metal
    FO_HAV_MAT_ORGANIC_STAIRS_PLATFORM = 102 # Organic
    FO_HAV_MAT_SKIN_STAIRS_PLATFORM = 103 # Skin
    FO_HAV_MAT_WATER_STAIRS_PLATFORM = 104 # Water
    FO_HAV_MAT_WOOD_STAIRS_PLATFORM = 105 # Wood
    FO_HAV_MAT_HEAVY_STONE_STAIRS_PLATFORM = 106 # Heavy Stone
    FO_HAV_MAT_HEAVY_METAL_STAIRS_PLATFORM = 107 # Heavy Metal
    FO_HAV_MAT_HEAVY_WOOD_STAIRS_PLATFORM = 108 # Heavy Wood
    FO_HAV_MAT_CHAIN_STAIRS_PLATFORM = 109 # Chain
    FO_HAV_MAT_BOTTLECAP_STAIRS_PLATFORM = 110 # Bottlecap
    FO_HAV_MAT_ELEVATOR_STAIRS_PLATFORM = 111 # Elevator
    FO_HAV_MAT_HOLLOW_METAL_STAIRS_PLATFORM = 112 # Hollow Metal
    FO_HAV_MAT_SHEET_METAL_STAIRS_PLATFORM = 113 # Sheet Metal
    FO_HAV_MAT_SAND_STAIRS_PLATFORM = 114 # Sand
    FO_HAV_MAT_BROKEN_CONCRETE_STAIRS_PLATFORM = 115 # Broken Concrete
    FO_HAV_MAT_VEHICLE_BODY_STAIRS_PLATFORM = 116 # Vehicle Body
    FO_HAV_MAT_VEHICLE_PART_SOLID_STAIRS_PLATFORM = 117 # Vehicle Part Solid
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_STAIRS_PLATFORM = 118 # Vehicle Part Hollow
    FO_HAV_MAT_BARREL_STAIRS_PLATFORM = 119 # Barrel
    FO_HAV_MAT_BOTTLE_STAIRS_PLATFORM = 120 # Bottle
    FO_HAV_MAT_SODA_CAN_STAIRS_PLATFORM = 121 # Soda Can
    FO_HAV_MAT_PISTOL_STAIRS_PLATFORM = 122 # Pistol
    FO_HAV_MAT_RIFLE_STAIRS_PLATFORM = 123 # Rifle
    FO_HAV_MAT_SHOPPING_CART_STAIRS_PLATFORM = 124 # Shopping Cart
    FO_HAV_MAT_LUNCHBOX_STAIRS_PLATFORM = 125 # Lunchbox
    FO_HAV_MAT_BABY_RATTLE_STAIRS_PLATFORM = 126 # Baby Rattle
    FO_HAV_MAT_RUBBER_BALL_STAIRS_PLATFORM = 127 # Rubber Ball

# Bethesda Havok. Material descriptor for a Havok shape in Skyrim.
class SkyrimHavokMaterial(Enum): # Z
    SKY_HAV_MAT_BROKEN_STONE = 131151687 # Broken Stone
    SKY_HAV_MAT_LIGHT_WOOD = 365420259 # Light Wood
    SKY_HAV_MAT_SNOW = 398949039    # Snow
    SKY_HAV_MAT_GRAVEL = 428587608  # Gravel
    SKY_HAV_MAT_MATERIAL_CHAIN_METAL = 438912228 # Material Chain Metal
    SKY_HAV_MAT_BOTTLE = 493553910  # Bottle
    SKY_HAV_MAT_WOOD = 500811281    # Wood
    SKY_HAV_MAT_SKIN = 591247106    # Skin
    SKY_HAV_MAT_UNKNOWN_617099282 = 617099282 # Unknown in Creation Kit v1.9.32.0. Found in Dawnguard DLC in meshes\dlc01\clutter\dlc01deerskin.nif.
    SKY_HAV_MAT_BARREL = 732141076  # Barrel
    SKY_HAV_MAT_MATERIAL_CERAMIC_MEDIUM = 781661019 # Material Ceramic Medium
    SKY_HAV_MAT_MATERIAL_BASKET = 790784366 # Material Basket
    SKY_HAV_MAT_ICE = 873356572     # Ice
    SKY_HAV_MAT_STAIRS_STONE = 899511101 # Stairs Stone
    SKY_HAV_MAT_WATER = 1024582599  # Water
    SKY_HAV_MAT_UNKNOWN_1028101969 = 1028101969 # Unknown in Creation Kit v1.6.89.0. Found in actors\draugr\character assets\skeletons.nif.
    SKY_HAV_MAT_MATERIAL_BLADE_1HAND = 1060167844 # Material Blade 1 Hand
    SKY_HAV_MAT_MATERIAL_BOOK = 1264672850 # Material Book
    SKY_HAV_MAT_MATERIAL_CARPET = 1286705471 # Material Carpet
    SKY_HAV_MAT_SOLID_METAL = 1288358971 # Solid Metal
    SKY_HAV_MAT_MATERIAL_AXE_1HAND = 1305674443 # Material Axe 1Hand
    SKY_HAV_MAT_UNKNOWN_1440721808 = 1440721808 # Unknown in Creation Kit v1.6.89.0. Found in armor\draugr\draugrbootsfemale_go.nif or armor\amuletsandrings\amuletgnd.nif.
    SKY_HAV_MAT_STAIRS_WOOD = 1461712277 # Stairs Wood
    SKY_HAV_MAT_MUD = 1486385281    # Mud
    SKY_HAV_MAT_MATERIAL_BOULDER_SMALL = 1550912982 # Material Boulder Small
    SKY_HAV_MAT_STAIRS_SNOW = 1560365355 # Stairs Snow
    SKY_HAV_MAT_HEAVY_STONE = 1570821952 # Heavy Stone
    SKY_HAV_MAT_UNKNOWN_1574477864 = 1574477864 # Unknown in Creation Kit v1.6.89.0. Found in actors\dragon\character assets\skeleton.nif.
    SKY_HAV_MAT_UNKNOWN_1591009235 = 1591009235 # Unknown in Creation Kit v1.6.89.0. Found in trap objects or clutter\displaycases\displaycaselgangled01.nif or actors\deer\character assets\skeleton.nif.
    SKY_HAV_MAT_MATERIAL_BOWS_STAVES = 1607128641 # Material Bows Staves
    SKY_HAV_MAT_MATERIAL_WOOD_AS_STAIRS = 1803571212 # Material Wood As Stairs
    SKY_HAV_MAT_GRASS = 1848600814  # Grass
    SKY_HAV_MAT_MATERIAL_BOULDER_LARGE = 1885326971 # Material Boulder Large
    SKY_HAV_MAT_MATERIAL_STONE_AS_STAIRS = 1886078335 # Material Stone As Stairs
    SKY_HAV_MAT_MATERIAL_BLADE_2HAND = 2022742644 # Material Blade 2Hand
    SKY_HAV_MAT_MATERIAL_BOTTLE_SMALL = 2025794648 # Material Bottle Small
    SKY_HAV_MAT_SAND = 2168343821   # Sand
    SKY_HAV_MAT_HEAVY_METAL = 2229413539 # Heavy Metal
    SKY_HAV_MAT_UNKNOWN_2290050264 = 2290050264 # Unknown in Creation Kit v1.9.32.0. Found in Dawnguard DLC in meshes\dlc01\clutter\dlc01sabrecatpelt.nif.
    SKY_HAV_MAT_DRAGON = 2518321175 # Dragon
    SKY_HAV_MAT_MATERIAL_BLADE_1HAND_SMALL = 2617944780 # Material Blade 1Hand Small
    SKY_HAV_MAT_MATERIAL_SKIN_SMALL = 2632367422 # Material Skin Small
    SKY_HAV_MAT_STAIRS_BROKEN_STONE = 2892392795 # Stairs Broken Stone
    SKY_HAV_MAT_MATERIAL_SKIN_LARGE = 2965929619 # Material Skin Large
    SKY_HAV_MAT_ORGANIC = 2974920155# Organic
    SKY_HAV_MAT_MATERIAL_BONE = 3049421844 # Material Bone
    SKY_HAV_MAT_HEAVY_WOOD = 3070783559 # Heavy Wood
    SKY_HAV_MAT_MATERIAL_CHAIN = 3074114406 # Material Chain
    SKY_HAV_MAT_DIRT = 3106094762   # Dirt
    SKY_HAV_MAT_MATERIAL_ARMOR_LIGHT = 3424720541 # Material Armor Light
    SKY_HAV_MAT_MATERIAL_SHIELD_LIGHT = 3448167928 # Material Shield Light
    SKY_HAV_MAT_MATERIAL_COIN = 3589100606 # Material Coin
    SKY_HAV_MAT_MATERIAL_SHIELD_HEAVY = 3702389584 # Material Shield Heavy
    SKY_HAV_MAT_MATERIAL_ARMOR_HEAVY = 3708432437 # Material Armor Heavy
    SKY_HAV_MAT_MATERIAL_ARROW = 3725505938 # Material Arrow
    SKY_HAV_MAT_GLASS = 3739830338  # Glass
    SKY_HAV_MAT_STONE = 3741512247  # Stone
    SKY_HAV_MAT_CLOTH = 3839073443  # Cloth
    SKY_HAV_MAT_MATERIAL_BLUNT_2HAND = 3969592277 # Material Blunt 2Hand
    SKY_HAV_MAT_UNKNOWN_4239621792 = 4239621792 # Unknown in Creation Kit v1.9.32.0. Found in Dawnguard DLC in meshes\dlc01\prototype\dlc1protoswingingbridge.nif.
    SKY_HAV_MAT_MATERIAL_BOULDER_MEDIUM = 4283869410 # Material Boulder Medium

# Bethesda Havok. Describes the collision layer a body belongs to in Oblivion.
class OblivionLayer(Enum): # Z
    OL_UNIDENTIFIED = 0             # Unidentified (white)
    OL_STATIC = 1                   # Static (red)
    OL_ANIM_STATIC = 2              # AnimStatic (magenta)
    OL_TRANSPARENT = 3              # Transparent (light pink)
    OL_CLUTTER = 4                  # Clutter (light blue)
    OL_WEAPON = 5                   # Weapon (orange)
    OL_PROJECTILE = 6               # Projectile (light orange)
    OL_SPELL = 7                    # Spell (cyan)
    OL_BIPED = 8                    # Biped (green) Seems to apply to all creatures/NPCs
    OL_TREES = 9                    # Trees (light brown)
    OL_PROPS = 10                   # Props (magenta)
    OL_WATER = 11                   # Water (cyan)
    OL_TRIGGER = 12                 # Trigger (light grey)
    OL_TERRAIN = 13                 # Terrain (light yellow)
    OL_TRAP = 14                    # Trap (light grey)
    OL_NONCOLLIDABLE = 15           # NonCollidable (white)
    OL_CLOUD_TRAP = 16              # CloudTrap (greenish grey)
    OL_GROUND = 17                  # Ground (none)
    OL_PORTAL = 18                  # Portal (green)
    OL_STAIRS = 19                  # Stairs (white)
    OL_CHAR_CONTROLLER = 20         # CharController (yellow)
    OL_AVOID_BOX = 21               # AvoidBox (dark yellow)
    OL_UNKNOWN1 = 22                # ? (white)
    OL_UNKNOWN2 = 23                # ? (white)
    OL_CAMERA_PICK = 24             # CameraPick (white)
    OL_ITEM_PICK = 25               # ItemPick (white)
    OL_LINE_OF_SIGHT = 26           # LineOfSight (white)
    OL_PATH_PICK = 27               # PathPick (white)
    OL_CUSTOM_PICK_1 = 28           # CustomPick1 (white)
    OL_CUSTOM_PICK_2 = 29           # CustomPick2 (white)
    OL_SPELL_EXPLOSION = 30         # SpellExplosion (white)
    OL_DROPPING_PICK = 31           # DroppingPick (white)
    OL_OTHER = 32                   # Other (white)
    OL_HEAD = 33                    # Head
    OL_BODY = 34                    # Body
    OL_SPINE1 = 35                  # Spine1
    OL_SPINE2 = 36                  # Spine2
    OL_L_UPPER_ARM = 37             # LUpperArm
    OL_L_FOREARM = 38               # LForeArm
    OL_L_HAND = 39                  # LHand
    OL_L_THIGH = 40                 # LThigh
    OL_L_CALF = 41                  # LCalf
    OL_L_FOOT = 42                  # LFoot
    OL_R_UPPER_ARM = 43             # RUpperArm
    OL_R_FOREARM = 44               # RForeArm
    OL_R_HAND = 45                  # RHand
    OL_R_THIGH = 46                 # RThigh
    OL_R_CALF = 47                  # RCalf
    OL_R_FOOT = 48                  # RFoot
    OL_TAIL = 49                    # Tail
    OL_SIDE_WEAPON = 50             # SideWeapon
    OL_SHIELD = 51                  # Shield
    OL_QUIVER = 52                  # Quiver
    OL_BACK_WEAPON = 53             # BackWeapon
    OL_BACK_WEAPON2 = 54            # BackWeapon (?)
    OL_PONYTAIL = 55                # PonyTail
    OL_WING = 56                    # Wing
    OL_NULL = 57                    # Null

# Bethesda Havok. Describes the collision layer a body belongs to in Fallout 3 and Fallout NV.
class Fallout3Layer(Enum): # Z
    FOL_UNIDENTIFIED = 0            # Unidentified (white)
    FOL_STATIC = 1                  # Static (red)
    FOL_ANIM_STATIC = 2             # AnimStatic (magenta)
    FOL_TRANSPARENT = 3             # Transparent (light pink)
    FOL_CLUTTER = 4                 # Clutter (light blue)
    FOL_WEAPON = 5                  # Weapon (orange)
    FOL_PROJECTILE = 6              # Projectile (light orange)
    FOL_SPELL = 7                   # Spell (cyan)
    FOL_BIPED = 8                   # Biped (green) Seems to apply to all creatures/NPCs
    FOL_TREES = 9                   # Trees (light brown)
    FOL_PROPS = 10                  # Props (magenta)
    FOL_WATER = 11                  # Water (cyan)
    FOL_TRIGGER = 12                # Trigger (light grey)
    FOL_TERRAIN = 13                # Terrain (light yellow)
    FOL_TRAP = 14                   # Trap (light grey)
    FOL_NONCOLLIDABLE = 15          # NonCollidable (white)
    FOL_CLOUD_TRAP = 16             # CloudTrap (greenish grey)
    FOL_GROUND = 17                 # Ground (none)
    FOL_PORTAL = 18                 # Portal (green)
    FOL_DEBRIS_SMALL = 19           # DebrisSmall (white)
    FOL_DEBRIS_LARGE = 20           # DebrisLarge (white)
    FOL_ACOUSTIC_SPACE = 21         # AcousticSpace (white)
    FOL_ACTORZONE = 22              # Actorzone (white)
    FOL_PROJECTILEZONE = 23         # Projectilezone (white)
    FOL_GASTRAP = 24                # GasTrap (yellowish green)
    FOL_SHELLCASING = 25            # ShellCasing (white)
    FOL_TRANSPARENT_SMALL = 26      # TransparentSmall (white)
    FOL_INVISIBLE_WALL = 27         # InvisibleWall (white)
    FOL_TRANSPARENT_SMALL_ANIM = 28 # TransparentSmallAnim (white)
    FOL_DEADBIP = 29                # Dead Biped (green)
    FOL_CHARCONTROLLER = 30         # CharController (yellow)
    FOL_AVOIDBOX = 31               # Avoidbox (orange)
    FOL_COLLISIONBOX = 32           # Collisionbox (white)
    FOL_CAMERASPHERE = 33           # Camerasphere (white)
    FOL_DOORDETECTION = 34          # Doordetection (white)
    FOL_CAMERAPICK = 35             # Camerapick (white)
    FOL_ITEMPICK = 36               # Itempick (white)
    FOL_LINEOFSIGHT = 37            # LineOfSight (white)
    FOL_PATHPICK = 38               # Pathpick (white)
    FOL_CUSTOMPICK1 = 39            # Custompick1 (white)
    FOL_CUSTOMPICK2 = 40            # Custompick2 (white)
    FOL_SPELLEXPLOSION = 41         # SpellExplosion (white)
    FOL_DROPPINGPICK = 42           # Droppingpick (white)
    FOL_NULL = 43                   # Null (white)

# Bethesda Havok. Describes the collision layer a body belongs to in Skyrim.
class SkyrimLayer(Enum): # Z
    SKYL_UNIDENTIFIED = 0           # Unidentified
    SKYL_STATIC = 1                 # Static
    SKYL_ANIMSTATIC = 2             # Anim Static
    SKYL_TRANSPARENT = 3            # Transparent
    SKYL_CLUTTER = 4                # Clutter. Object with this layer will float on water surface.
    SKYL_WEAPON = 5                 # Weapon
    SKYL_PROJECTILE = 6             # Projectile
    SKYL_SPELL = 7                  # Spell
    SKYL_BIPED = 8                  # Biped. Seems to apply to all creatures/NPCs
    SKYL_TREES = 9                  # Trees
    SKYL_PROPS = 10                 # Props
    SKYL_WATER = 11                 # Water
    SKYL_TRIGGER = 12               # Trigger
    SKYL_TERRAIN = 13               # Terrain
    SKYL_TRAP = 14                  # Trap
    SKYL_NONCOLLIDABLE = 15         # NonCollidable
    SKYL_CLOUD_TRAP = 16            # CloudTrap
    SKYL_GROUND = 17                # Ground. It seems that produces no sound when collide.
    SKYL_PORTAL = 18                # Portal
    SKYL_DEBRIS_SMALL = 19          # Debris Small
    SKYL_DEBRIS_LARGE = 20          # Debris Large
    SKYL_ACOUSTIC_SPACE = 21        # Acoustic Space
    SKYL_ACTORZONE = 22             # Actor Zone
    SKYL_PROJECTILEZONE = 23        # Projectile Zone
    SKYL_GASTRAP = 24               # Gas Trap
    SKYL_SHELLCASING = 25           # Shell Casing
    SKYL_TRANSPARENT_SMALL = 26     # Transparent Small
    SKYL_INVISIBLE_WALL = 27        # Invisible Wall
    SKYL_TRANSPARENT_SMALL_ANIM = 28# Transparent Small Anim
    SKYL_WARD = 29                  # Ward
    SKYL_CHARCONTROLLER = 30        # Char Controller
    SKYL_STAIRHELPER = 31           # Stair Helper
    SKYL_DEADBIP = 32               # Dead Bip
    SKYL_BIPED_NO_CC = 33           # Biped No CC
    SKYL_AVOIDBOX = 34              # Avoid Box
    SKYL_COLLISIONBOX = 35          # Collision Box
    SKYL_CAMERASHPERE = 36          # Camera Sphere
    SKYL_DOORDETECTION = 37         # Door Detection
    SKYL_CONEPROJECTILE = 38        # Cone Projectile
    SKYL_CAMERAPICK = 39            # Camera Pick
    SKYL_ITEMPICK = 40              # Item Pick
    SKYL_LINEOFSIGHT = 41           # Line of Sight
    SKYL_PATHPICK = 42              # Path Pick
    SKYL_CUSTOMPICK1 = 43           # Custom Pick 1
    SKYL_CUSTOMPICK2 = 44           # Custom Pick 2
    SKYL_SPELLEXPLOSION = 45        # Spell Explosion
    SKYL_DROPPINGPICK = 46          # Dropping Pick
    SKYL_NULL = 47                  # Null

# Bethesda Havok.
# A byte describing if MOPP Data is organized into chunks (PS3) or not (PC)
class MoppDataBuildType(Enum): # Z
    BUILT_WITH_CHUNK_SUBDIVISION = 0# Organized in chunks for PS3.
    BUILT_WITHOUT_CHUNK_SUBDIVISION = 1 # Not organized in chunks for PC. (Default)
    BUILD_NOT_SET = 2               # Build type not set yet.

# Describes the pixel format used by the NiPixelData object to store a texture.
class PixelFormat(Enum): # Y
    FMT_RGB = 0                     # 24-bit RGB. 8 bits per red, blue, and green component.
    FMT_RGBA = 1                    # 32-bit RGB with alpha. 8 bits per red, blue, green, and alpha component.
    FMT_PAL = 2                     # 8-bit palette index.
    FMT_PALA = 3                    # 8-bit palette index with alpha.
    FMT_DXT1 = 4                    # DXT1 compressed texture.
    FMT_DXT3 = 5                    # DXT3 compressed texture.
    FMT_DXT5 = 6                    # DXT5 compressed texture.
    FMT_RGB24NONINT = 7             # (Deprecated) 24-bit noninterleaved texture, an old PS2 format.
    FMT_BUMP = 8                    # Uncompressed dU/dV gradient bump map.
    FMT_BUMPLUMA = 9                # Uncompressed dU/dV gradient bump map with luma channel representing shininess.
    FMT_RENDERSPEC = 10             # Generic descriptor for any renderer-specific format not described by other formats.
    FMT_1CH = 11                    # Generic descriptor for formats with 1 component.
    FMT_2CH = 12                    # Generic descriptor for formats with 2 components.
    FMT_3CH = 13                    # Generic descriptor for formats with 3 components.
    FMT_4CH = 14                    # Generic descriptor for formats with 4 components.
    FMT_DEPTH_STENCIL = 15          # Indicates the NiPixelFormat is meant to be used on a depth/stencil surface.
    FMT_UNKNOWN = 16

# Describes whether pixels have been tiled from their standard row-major format to a format optimized for a particular platform.
class PixelTiling(Enum): # Y
    TILE_NONE = 0
    TILE_XENON = 1
    TILE_WII = 2
    TILE_NV_SWIZZLED = 3

# Describes the pixel format used by the NiPixelData object to store a texture.
class PixelComponent(Enum): # Y
    COMP_RED = 0
    COMP_GREEN = 1
    COMP_BLUE = 2
    COMP_ALPHA = 3
    COMP_COMPRESSED = 4
    COMP_OFFSET_U = 5
    COMP_OFFSET_V = 6
    COMP_OFFSET_W = 7
    COMP_OFFSET_Q = 8
    COMP_LUMA = 9
    COMP_HEIGHT = 10
    COMP_VECTOR_X = 11
    COMP_VECTOR_Y = 12
    COMP_VECTOR_Z = 13
    COMP_PADDING = 14
    COMP_INTENSITY = 15
    COMP_INDEX = 16
    COMP_DEPTH = 17
    COMP_STENCIL = 18
    COMP_EMPTY = 19

# Describes how each pixel should be accessed on NiPixelFormat.
class PixelRepresentation(Enum): # Y
    REP_NORM_INT = 0
    REP_HALF = 1
    REP_FLOAT = 2
    REP_INDEX = 3
    REP_COMPRESSED = 4
    REP_UNKNOWN = 5
    REP_INT = 6

# Describes the color depth in an NiTexture.
class PixelLayout(Enum): # X
    LAY_PALETTIZED_8 = 0            # Texture is in 8-bit palettized format.
    LAY_HIGH_COLOR_16 = 1           # Texture is in 16-bit high color format.
    LAY_TRUE_COLOR_32 = 2           # Texture is in 32-bit true color format.
    LAY_COMPRESSED = 3              # Texture is compressed.
    LAY_BUMPMAP = 4                 # Texture is a grayscale bump map.
    LAY_PALETTIZED_4 = 5            # Texture is in 4-bit palettized format.
    LAY_DEFAULT = 6                 # Use default setting.
    LAY_SINGLE_COLOR_8 = 7
    LAY_SINGLE_COLOR_16 = 8
    LAY_SINGLE_COLOR_32 = 9
    LAY_DOUBLE_COLOR_32 = 10
    LAY_DOUBLE_COLOR_64 = 11
    LAY_FLOAT_COLOR_32 = 12
    LAY_FLOAT_COLOR_64 = 13
    LAY_FLOAT_COLOR_128 = 14
    LAY_SINGLE_COLOR_4 = 15
    LAY_DEPTH_24_X8 = 16

# Describes how mipmaps are handled in an NiTexture.
class MipMapFormat(Enum): # X
    MIP_FMT_NO = 0                  # Texture does not use mip maps.
    MIP_FMT_YES = 1                 # Texture uses mip maps.
    MIP_FMT_DEFAULT = 2             # Use default setting.

# Describes how transparency is handled in an NiTexture.
class AlphaFormat(Enum): # X
    ALPHA_NONE = 0                  # No alpha.
    ALPHA_BINARY = 1                # 1-bit alpha.
    ALPHA_SMOOTH = 2                # Interpolated 4- or 8-bit alpha.
    ALPHA_DEFAULT = 3               # Use default setting.

# Describes the availiable texture clamp modes, i.e. the behavior of UV mapping outside the [0,1] range.
class TexClampMode(Enum): # X
    CLAMP_S_CLAMP_T = 0             # Clamp in both directions.
    CLAMP_S_WRAP_T = 1              # Clamp in the S(U) direction but wrap in the T(V) direction.
    WRAP_S_CLAMP_T = 2              # Wrap in the S(U) direction but clamp in the T(V) direction.
    WRAP_S_WRAP_T = 3               # Wrap in both directions.

# Describes the availiable texture filter modes, i.e. the way the pixels in a texture are displayed on screen.
class TexFilterMode(Enum): # X
    FILTER_NEAREST = 0              # Nearest neighbor. Uses nearest texel with no mipmapping.
    FILTER_BILERP = 1               # Bilinear. Linear interpolation with no mipmapping.
    FILTER_TRILERP = 2              # Trilinear. Linear intepolation between 8 texels (4 nearest texels between 2 nearest mip levels).
    FILTER_NEAREST_MIPNEAREST = 3   # Nearest texel on nearest mip level.
    FILTER_NEAREST_MIPLERP = 4      # Linear interpolates nearest texel between two nearest mip levels.
    FILTER_BILERP_MIPNEAREST = 5    # Linear interpolates on nearest mip level.
    FILTER_ANISOTROPIC = 6          # Anisotropic filtering. One or many trilinear samples depending on anisotropy.

# Describes how to apply vertex colors for NiVertexColorProperty.
class VertMode(Enum): # X
    VERT_MODE_SRC_IGNORE = 0        # Emissive, ambient, and diffuse colors are all specified by the NiMaterialProperty.
    VERT_MODE_SRC_EMISSIVE = 1      # Emissive colors are specified by the source vertex colors. Ambient+Diffuse are specified by the NiMaterialProperty.
    VERT_MODE_SRC_AMB_DIF = 2       # Ambient+Diffuse colors are specified by the source vertex colors. Emissive is specified by the NiMaterialProperty. (Default)

# Describes which lighting equation components influence the final vertex color for NiVertexColorProperty.
class LightMode(Enum): # X
    LIGHT_MODE_EMISSIVE = 0         # Emissive.
    LIGHT_MODE_EMI_AMB_DIF = 1      # Emissive + Ambient + Diffuse. (Default)

# The animation cyle behavior.
class CycleType(Enum): # Z
    CYCLE_LOOP = 0                  # Loop
    CYCLE_REVERSE = 1               # Reverse
    CYCLE_CLAMP = 2                 # Clamp

# The force field type.
class FieldType(Enum): # X
    FIELD_WIND = 0                  # Wind (fixed direction)
    FIELD_POINT = 1                 # Point (fixed origin)

# Determines the way the billboard will react to the camera.
# Billboard mode is stored in lowest 3 bits although Oblivion vanilla nifs uses values higher than 7.
class BillboardMode(Enum): # Y
    ALWAYS_FACE_CAMERA = 0          # Align billboard and camera forward vector. Minimized rotation.
    ROTATE_ABOUT_UP = 1             # Align billboard and camera forward vector while allowing rotation around the up axis.
    RIGID_FACE_CAMERA = 2           # Align billboard and camera forward vector. Non-minimized rotation.
    ALWAYS_FACE_CENTER = 3          # Billboard forward vector always faces camera ceneter. Minimized rotation.
    RIGID_FACE_CENTER = 4           # Billboard forward vector always faces camera ceneter. Non-minimized rotation.
    BSROTATE_ABOUT_UP = 5           # The billboard will only rotate around its local Z axis (it always stays in its local X-Y plane).
    ROTATE_ABOUT_UP2 = 9            # The billboard will only rotate around the up axis (same as ROTATE_ABOUT_UP?).

# Describes stencil buffer test modes for NiStencilProperty.
class StencilCompareMode(Enum): # Z
    TEST_NEVER = 0                  # Always false. Ref value is ignored.
    TEST_LESS = 1                   # VRef ‹ VBuf
    TEST_EQUAL = 2                  # VRef = VBuf
    TEST_LESS_EQUAL = 3             # VRef ≤ VBuf
    TEST_GREATER = 4                # VRef › VBuf
    TEST_NOT_EQUAL = 5              # VRef ≠ VBuf
    TEST_GREATER_EQUAL = 6          # VRef ≥ VBuf
    TEST_ALWAYS = 7                 # Always true. Buffer is ignored.

# Describes the actions which can occur as a result of tests for NiStencilProperty.
class StencilAction(Enum): # Z
    ACTION_KEEP = 0                 # Keep the current value in the stencil buffer.
    ACTION_ZERO = 1                 # Write zero to the stencil buffer.
    ACTION_REPLACE = 2              # Write the reference value to the stencil buffer.
    ACTION_INCREMENT = 3            # Increment the value in the stencil buffer.
    ACTION_DECREMENT = 4            # Decrement the value in the stencil buffer.
    ACTION_INVERT = 5               # Bitwise invert the value in the stencil buffer.

# Describes the face culling options for NiStencilProperty.
class StencilDrawMode(Enum): # Z
    DRAW_CCW_OR_BOTH = 0            # Application default, chooses between DRAW_CCW or DRAW_BOTH.
    DRAW_CCW = 1                    # Draw only the triangles whose vertices are ordered CCW with respect to the viewer. (Standard behavior)
    DRAW_CW = 2                     # Draw only the triangles whose vertices are ordered CW with respect to the viewer. (Effectively flips faces)
    DRAW_BOTH = 3                   # Draw all triangles, regardless of orientation. (Effectively force double-sided)

# Describes Z-buffer test modes for NiZBufferProperty.
# "Less than" = closer to camera, "Greater than" = further from camera.
class ZCompareMode(Enum): # Y
    ZCOMP_ALWAYS = 0                # Always true. Buffer is ignored.
    ZCOMP_LESS = 1                  # VRef ‹ VBuf
    ZCOMP_EQUAL = 2                 # VRef = VBuf
    ZCOMP_LESS_EQUAL = 3            # VRef ≤ VBuf
    ZCOMP_GREATER = 4               # VRef › VBuf
    ZCOMP_NOT_EQUAL = 5             # VRef ≠ VBuf
    ZCOMP_GREATER_EQUAL = 6         # VRef ≥ VBuf
    ZCOMP_NEVER = 7                 # Always false. Ref value is ignored.

# Bethesda Havok, based on hkpMotion::MotionType. Motion type of a rigid body determines what happens when it is simulated.
class hkMotionType(Enum): # Z
    MO_SYS_INVALID = 0              # Invalid
    MO_SYS_DYNAMIC = 1              # A fully-simulated, movable rigid body. At construction time the engine checks the input inertia and selects MO_SYS_SPHERE_INERTIA or MO_SYS_BOX_INERTIA as appropriate.
    MO_SYS_SPHERE_INERTIA = 2       # Simulation is performed using a sphere inertia tensor.
    MO_SYS_SPHERE_STABILIZED = 3    # This is the same as MO_SYS_SPHERE_INERTIA, except that simulation of the rigid body is "softened".
    MO_SYS_BOX_INERTIA = 4          # Simulation is performed using a box inertia tensor.
    MO_SYS_BOX_STABILIZED = 5       # This is the same as MO_SYS_BOX_INERTIA, except that simulation of the rigid body is "softened".
    MO_SYS_KEYFRAMED = 6            # Simulation is not performed as a normal rigid body. The keyframed rigid body has an infinite mass when viewed by the rest of the system. (used for creatures)
    MO_SYS_FIXED = 7                # This motion type is used for the static elements of a game scene, e.g. the landscape. Faster than MO_SYS_KEYFRAMED at velocity 0. (used for weapons)
    MO_SYS_THIN_BOX = 8             # A box inertia motion which is optimized for thin boxes and has less stability problems
    MO_SYS_CHARACTER = 9            # A specialized motion used for character controllers

# Bethesda Havok, based on hkpRigidBodyDeactivator::DeactivatorType.
# Deactivator Type determines which mechanism Havok will use to classify the body as deactivated.
class hkDeactivatorType(Enum): # Z
    DEACTIVATOR_INVALID = 0         # Invalid
    DEACTIVATOR_NEVER = 1           # This will force the rigid body to never deactivate.
    DEACTIVATOR_SPATIAL = 2         # Tells Havok to use a spatial deactivation scheme. This makes use of high and low frequencies of positional motion to determine when deactivation should occur.

# Bethesda Havok, based on hkpRigidBodyCinfo::SolverDeactivation.
# A list of possible solver deactivation settings. This value defines how aggressively the solver deactivates objects.
# Note: Solver deactivation does not save CPU, but reduces creeping of movable objects in a pile quite dramatically.
class hkSolverDeactivation(Enum): # Z
    SOLVER_DEACTIVATION_INVALID = 0 # Invalid
    SOLVER_DEACTIVATION_OFF = 1     # No solver deactivation.
    SOLVER_DEACTIVATION_LOW = 2     # Very conservative deactivation, typically no visible artifacts.
    SOLVER_DEACTIVATION_MEDIUM = 3  # Normal deactivation, no serious visible artifacts in most cases.
    SOLVER_DEACTIVATION_HIGH = 4    # Fast deactivation, visible artifacts.
    SOLVER_DEACTIVATION_MAX = 5     # Very fast deactivation, visible artifacts.

# Bethesda Havok, based on hkpCollidableQualityType. Describes the priority and quality of collisions for a body,
#     e.g. you may expect critical game play objects to have solid high-priority collisions so that they never sink into ground,
#     or may allow penetrations for visual debris objects.
# Notes:
#     - Fixed and keyframed objects cannot interact with each other.
#     - Debris can interpenetrate but still responds to Bullet hits.
#     - Critical objects are forced to not interpenetrate.
#     - Moving objects can interpenetrate slightly with other Moving or Debris objects but nothing else.
class hkQualityType(Enum): # Z
    MO_QUAL_INVALID = 0             # Automatically assigned to MO_QUAL_FIXED, MO_QUAL_KEYFRAMED or MO_QUAL_DEBRIS
    MO_QUAL_FIXED = 1               # Static body.
    MO_QUAL_KEYFRAMED = 2           # Animated body with infinite mass.
    MO_QUAL_DEBRIS = 3              # Low importance bodies adding visual detail.
    MO_QUAL_MOVING = 4              # Moving bodies which should not penetrate or leave the world, but can.
    MO_QUAL_CRITICAL = 5            # Gameplay critical bodies which cannot penetrate or leave the world under any circumstance.
    MO_QUAL_BULLET = 6              # Fast-moving bodies, such as projectiles.
    MO_QUAL_USER = 7                # For user.
    MO_QUAL_CHARACTER = 8           # For use with rigid body character controllers.
    MO_QUAL_KEYFRAMED_REPORT = 9    # Moving bodies with infinite mass which should report contact points and TOI collisions against all other bodies.

# Describes the decay function of bomb forces.
class DecayType(Enum): # X
    DECAY_NONE = 0                  # No decay.
    DECAY_LINEAR = 1                # Linear decay.
    DECAY_EXPONENTIAL = 2           # Exponential decay.

# Describes the symmetry type of bomb forces.
class SymmetryType(Enum): # Y
    SPHERICAL_SYMMETRY = 0          # Spherical Symmetry.
    CYLINDRICAL_SYMMETRY = 1        # Cylindrical Symmetry.
    PLANAR_SYMMETRY = 2             # Planar Symmetry.

# The type of information that is stored in a texture used by an NiTextureEffect.
class TextureType(Enum): # X
    TEX_PROJECTED_LIGHT = 0         # Apply a projected light texture. Each light effect is summed before multiplying by the base texture.
    TEX_PROJECTED_SHADOW = 1        # Apply a projected shadow texture. Each shadow effect is multiplied by the base texture.
    TEX_ENVIRONMENT_MAP = 2         # Apply an environment map texture. Added to the base texture and light/shadow/decal maps.
    TEX_FOG_MAP = 3                 # Apply a fog map texture. Alpha channel is used to blend the color channel with the base texture.

# Determines the way that UV texture coordinates are generated.
class CoordGenType(Enum): # X
    CG_WORLD_PARALLEL = 0           # Use planar mapping.
    CG_WORLD_PERSPECTIVE = 1        # Use perspective mapping.
    CG_SPHERE_MAP = 2               # Use spherical mapping.
    CG_SPECULAR_CUBE_MAP = 3        # Use specular cube mapping. For NiSourceCubeMap only.
    CG_DIFFUSE_CUBE_MAP = 4         # Use diffuse cube mapping. For NiSourceCubeMap only.

class EndianType(Enum): # X
    ENDIAN_BIG = 0                  # The numbers are stored in big endian format, such as those used by PowerPC Mac processors.
    ENDIAN_LITTLE = 1               # The numbers are stored in little endian format, such as those used by Intel and AMD x86 processors.

# Used by NiMaterialColorControllers to select which type of color in the controlled object that will be animated.
class MaterialColor(Enum): # Y
    TC_AMBIENT = 0                  # Control the ambient color.
    TC_DIFFUSE = 1                  # Control the diffuse color.
    TC_SPECULAR = 2                 # Control the specular color.
    TC_SELF_ILLUM = 3               # Control the self illumination color.

# Used by NiGeometryData to control the volatility of the mesh.
# Consistency Type is masked to only the upper 4 bits (0xF000). Dirty mask is the lower 12 (0x0FFF) but only used at runtime.
class ConsistencyType(Enum): # Y
    CT_MUTABLE = 0x0000             # Mutable Mesh
    CT_STATIC = 0x4000              # Static Mesh
    CT_VOLATILE = 0x8000            # Volatile Mesh

class BoundVolumeType(Enum): # X
    BASE_BV = 0xffffffff            # Default
    SPHERE_BV = 0                   # Sphere
    BOX_BV = 1                      # Box
    CAPSULE_BV = 2                  # Capsule
    UNION_BV = 4                    # Union
    HALFSPACE_BV = 5                # Half Space

# Bethesda Havok.
class hkResponseType(Enum): # Z
    RESPONSE_INVALID = 0            # Invalid Response
    RESPONSE_SIMPLE_CONTACT = 1     # Do normal collision resolution
    RESPONSE_REPORTING = 2          # No collision resolution is performed but listeners are called
    RESPONSE_NONE = 3               # Do nothing, ignore all the results.

# Values for configuring the shader type in a BSLightingShaderProperty
class BSLightingShaderPropertyShaderType(Enum): # Y
    Default = 0
    Environment_Map = 1             # Enables EnvMap Mask(TS6), EnvMap Scale
    Glow_Shader = 2                 # Enables Glow(TS3)
    Parallax = 3                    # Enables Height(TS4)
    Face_Tint = 4                   # Enables Detail(TS4), Tint(TS7)
    Skin_Tint = 5                   # Enables Skin Tint Color
    Hair_Tint = 6                   # Enables Hair Tint Color
    Parallax_Occ = 7                # Enables Height(TS4), Max Passes, Scale. Unimplemented.
    Multitexture_Landscape = 8
    LOD_Landscape = 9
    Snow = 10
    MultiLayer_Parallax = 11        # Enables EnvMap Mask(TS6), Layer(TS7), Parallax Layer Thickness, Parallax Refraction Scale, Parallax Inner Layer U Scale, Parallax Inner Layer V Scale, EnvMap Scale
    Tree_Anim = 12
    LOD_Objects = 13
    Sparkle_Snow = 14               # Enables SparkleParams
    LOD_Objects_HD = 15
    Eye_Envmap = 16                 # Enables EnvMap Mask(TS6), Eye EnvMap Scale
    Cloud = 17
    LOD_Landscape_Noise = 18
    Multitexture_Landscape_LOD_Blend = 19
    FO4_Dismemberment = 20

#endregion

#region Compounds

# Color3 -> Color3(r)
# Color4 -> Color4(r)
# The NIF file footer.
class Footer: # X
    def __init__(self, r: NiReader):
        self.roots: list[Ref[NiObject]] = r.readL32FArray(X[NiObject].ref) if r.v >= 0x0303000D else None # List of root NIF objects. If there is a camera, for 1st person view, then this NIF object is referred to as well in this list, even if it is not a root object (usually we want the camera to be attached to the Bip Head node).

# Group of vertex indices of vertices that match.
class MatchGroup: # X
    def __init__(self, r: NiReader):
        self.vertexIndices: list[int] = r.readL16PArray(None, 'H') # The vertex indices.

# Description of a mipmap within an NiPixelData object.
class MipMap: # X
    _struct = ('<3i', 12)
    def __init__(self, r: NiReader):
        if isinstance(r, tuple): self.width,self.height,self.offset=r; return
        self.width: int = r.readUInt32()                # Width of the mipmap image.
        self.height: int = r.readUInt32()               # Height of the mipmap image.
        self.offset: int = r.readUInt32()               # Offset into the pixel data array where this mipmap starts.

# NiSkinData::BoneVertData. A vertex and its weight.
class BoneVertData: # X
    _struct = ('<Hf', 6)
    def __init__(self, r: NiReader, half: bool = None):
        if isinstance(r, tuple): self.index,self.weight=r; return
        if half: self.index = r.readUInt16(); self.weight = r.readHalf(); return
        self.index: int = r.readUInt16()                # The vertex index, in the mesh.
        self.weight: float = r.readSingle() if full else r.readHalf() # The vertex weight - between 0.0 and 1.0

# Used in NiDefaultAVObjectPalette.
class AVObject: # Z
    def __init__(self, r: NiReader):
        self.name: str = r.readL32AString()             # Object name.
        self.avObject: Ref[NiObject] = X[NiAVObject].ptr(r) # Object reference.

# In a .kf file, this links to a controllable object, via its name (or for version 10.2.0.0 and up, a link and offset to a NiStringPalette that contains the name), and a sequence of interpolators that apply to this controllable object, via links.
# For Controller ID, NiInterpController::GetCtlrID() virtual function returns a string formatted specifically for the derived type.
# For Interpolator ID, NiInterpController::GetInterpolatorID() virtual function returns a string formatted specifically for the derived type.
# The string formats are documented on the relevant niobject blocks.
class ControlledBlock: # Z
    targetName: str = None                              # Name of a controllable object in another NIF file.
    # NiControllerSequence::InterpArrayItem
    interpolator: Ref[NiObject] = None
    controller: Ref[NiObject] = None
    blendInterpolator: Ref[NiObject] = None
    blendIndex: int = None
    # Bethesda-only
    priority: int = 0                                   # Idle animations tend to have low values for this, and high values tend to correspond with the important parts of the animations.
    # NiControllerSequence::IDTag, post-10.1.0.104 only
    nodeName: str = None                                # The name of the animated NiAVObject.
    propertyType: str = None                            # The RTTI type of the NiProperty the controller is attached to, if applicable.
    controllerType: str = None                          # The RTTI type of the NiTimeController.
    controllerId: str = None                            # An ID that can uniquely identify the controller among others of the same type on the same NiObjectNET.
    interpolatorId: str = None                          # An ID that can uniquely identify the interpolator among others of the same type on the same NiObjectNET.
    stringPalette: Ref[NiObject] = None                 # Refers to the NiStringPalette which contains the name of the controlled NIF object.
    nodeNameOffset: int = None                          # Offset in NiStringPalette to the name of the animated NiAVObject.
    propertyTypeOffset: int = None                      # Offset in NiStringPalette to the RTTI type of the NiProperty the controller is attached to, if applicable.
    controllerTypeOffset: int = None                    # Offset in NiStringPalette to the RTTI type of the NiTimeController.
    controllerIdOffset: int = None                      # Offset in NiStringPalette to an ID that can uniquely identify the controller among others of the same type on the same NiObjectNET.
    interpolatorIdOffset: int = None                    # Offset in NiStringPalette to an ID that can uniquely identify the interpolator among others of the same type on the same NiObjectNET.

    def __init__(self, r: NiReader):
        if r.v <= 0x0A010067: self.targetName = Z.string(r)
        # NiControllerSequence::InterpArrayItem
        if r.v >= 0x0A01006A: self.interpolator = X[NiInterpolator].ref(r)
        if r.v <= 0x14050000: self.controller = X[NiTimeController].ref(r)
        if r.v >= 0x0A010068 and r.v <= 0x0A01006E:
            self.blendInterpolator = X[NiBlendInterpolator].ref(r)
            self.blendIndex = r.readUInt16()
        # Bethesda-only
        if r.v >= 0x0A01006A and (r.uv2 > 0): self.priority = r.readByte()
        # NiControllerSequence::IDTag, post-10.1.0.104 only
        if r.v >= 0x0A010068 and r.v <= 0x0A010071:
            self.nodeName = Z.string(r)
            self.propertyType = Z.string(r)
            self.controllerType = Z.string(r)
            self.controllerId = Z.string(r)
            self.interpolatorId = Z.string(r)
        if r.v >= 0x0A020000 and r.v <= 0x14010000:
            self.stringPalette = X[NiStringPalette].ref(r)
            self.nodeNameOffset = r.readUInt32()
            self.propertyTypeOffset = r.readUInt32()
            self.controllerTypeOffset = r.readUInt32()
            self.controllerIdOffset = r.readUInt32()
            self.interpolatorIdOffset = r.readUInt32()
        if r.v >= 0x14010001:
            self.nodeName = Z.string(r)
            self.propertyType = Z.string(r)
            self.controllerType = Z.string(r)
            self.controllerId = Z.string(r)
            self.interpolatorId = Z.string(r)

# Information about how the file was exported
class ExportInfo: # X
    def __init__(self, r: NiReader):
        self.author: str = r.readL8AString()
        self.processScript: str = r.readL8AString()
        self.exportScript: str = r.readL8AString()

# The NIF file header.
class NiReader(Reader): # X
    headerString: str = None                            # 'NetImmerse File Format x.x.x.x' (versions <= 10.0.1.2) or 'Gamebryo File Format x.x.x.x' (versions >= 10.1.0.0), with x.x.x.x the version written out. Ends with a newline character (0x0A).
    copyright: list[str] = None
    v: int = 0x04000002                                 # The NIF version, in hexadecimal notation: 0x04000002, 0x0401000C, 0x04020002, 0x04020100, 0x04020200, 0x0A000100, 0x0A010000, 0x0A020000, 0x14000004, ...
    endianType: EndianType = EndianType.ENDIAN_LITTLE   # Determines the endianness of the data in the file.
    uv: int = 0                                         # An extra version number, for companies that decide to modify the file format.
    numBlocks: int = None                               # Number of file objects.
    uv2: int = 0
    exportInfo: ExportInfo = None
    maxFilepath: str = None
    metadata: bytearray = None
    blockTypes: list[str] = None                        # List of all object types used in this NIF file.
    blockTypeHashes: list[int] = None                   # List of all object types used in this NIF file.
    blockTypeIndex: list[int] = None                    # Maps file objects on their corresponding type: first file object is of type object_types[object_type_index[0]], the second of object_types[object_type_index[1]], etc.
    blockSize: list[int] = None                         # Array of block sizes?
    numStrings: int = 0                                 # Number of strings.
    maxStringLength: int = 0                            # Maximum string length.
    strings: list[str] = None                           # Strings.
    groups: list[int] = None
    # read blocks
    blocks: list[NiObject]
    roots: list[Ref[NiObject]]

    def __init__(self, b: Reader):
        super().__init__(b.f)
        (self.headerString, self.v) = Z.parseHeaderStr(b.readVAString(128, b'\x0A')); r = self
        if r.v <= 0x03010000: self.copyright = [r.readL8AString(), r.readL8AString(), r.readL8AString()]
        if r.v >= 0x03010001: self.v = r.readUInt32()
        if r.v >= 0x14000003: self.endianType = EndianType(r.readByte())
        if r.v >= 0x0A000108: self.uv = r.readUInt32()
        if r.v >= 0x03010001: self.numBlocks = r.readUInt32()
        if ((r.v == 0x14020007) or (r.v == 0x14000005) or ((r.v >= 0x0A000102) and (r.v <= 0x14000004) and (r.uv <= 11))) and (r.uv >= 3):
            self.uv2 = r.readUInt32()
            self.exportInfo = ExportInfo(r)
        if r.uv2 == 130: self.maxFilepath = r.readL8AString()
        if r.v >= 0x1E000000: self.metadata = r.readL32Bytes()
        if r.v >= 0x05000001 and r.v != 0x14030102: self.blockTypes = r.readL16FArray(lambda z: r.readL32AString())
        if r.v == 0x14030102: self.blockTypeHashes = r.readL16PArray(None, 'I')
        if r.v >= 0x05000001: self.blockTypeIndex = r.readPArray(None, 'H', self.numBlocks)
        if r.v >= 0x14020005: self.blockSize = r.readPArray(None, 'I', self.numBlocks)
        if r.v >= 0x14010001:
            self.numStrings = r.readUInt32()
            self.maxStringLength = r.readUInt32()
            self.strings = r.readFArray(lambda z: r.readL32AString(), self.numStrings)
        if r.v >= 0x05000006: self.groups = r.readL32PArray(None, 'I')
        # read blocks
        self.blocks: list[NiObject] = [None]*self.numBlocks
        self.roots = Footer(r).roots

# A list of \\0 terminated strings.
class StringPalette: # Z
    def __init__(self, r: NiReader):
        self.palette: list[str] = r.readL32AString().split('0x00') # A bunch of 0x00 seperated strings.
        self.length: int = r.readUInt32()               # Length of the palette string is repeated here.

# Tension, bias, continuity.
class TBC: # X
    _struct = ('<3f', 12)
    def __init__(self, r: NiReader):
        if isinstance(r, tuple): self.t,self.b,self.c=r; return
        self.t: float = r.readSingle()                  # Tension.
        self.b: float = r.readSingle()                  # Bias.
        self.c: float = r.readSingle()                  # Continuity.

# A generic key with support for interpolation. Type 1 is normal linear interpolation, type 2 has forward and backward tangents, and type 3 has tension, bias and continuity arguments. Note that color4 and byte always seem to be of type 1.
class Key[T]: # X
    time: float = None                                  # Time of the key.
    value: T = None                                     # The key value.
    forward: T = None                                   # Key forward tangent.
    backward: T = None                                  # The key backward tangent.
    tbc: TBC = None                                     # The TBC of the key.

    def __init__(self, t: str, r: NiReader, keyType: KeyType):
        self.t = t
        self.time = r.readSingle()
        self.value = Z.read(self, r)
        if keyType == KeyType.QUADRATIC_KEY:
            self.forward = Z.read(self, r)
            self.backward = Z.read(self, r)
        elif keyType == KeyType.TBC_KEY: self.tbc = r.readS(TBC)

# Array of vector keys (anything that can be interpolated, except rotations).
class KeyGroup[T]: # X
    numKeys: int = 0                                    # Number of keys in the array.
    interpolation: KeyType = 0                          # The key type.
    keys: list[Key[T]] = None                           # The keys.

    def __init__(self, t: str, r: NiReader):
        self.t = t
        self.numKeys = r.readUInt32()
        if self.numKeys != 0: self.interpolation = KeyType(r.readUInt32())
        self.keys = r.readFArray(lambda z: Key[T]('[T]', r, Interpolation), self.numKeys)

# A special version of the key type used for quaternions.  Never has tangents.
class QuatKey[T]: # X
    time: float = None                                  # Time the key applies.
    value: T = None                                     # Value of the key.
    tbc: TBC = None                                     # The TBC of the key.

    def __init__(self, t: str, r: NiReader, keyType: KeyType):
        self.t = t
        if r.v <= 0x0A010000: self.time = r.readSingle()
        if keyType != KeyType.XYZ_ROTATION_KEY:
            if r.v >= 0x0A01006A: self.time = r.readSingle()
            self.value = Z.read(self, r)
        if keyType == KeyType.TBC_KEY: self.tbc = r.readS(TBC)

# Texture coordinates (u,v). As in OpenGL; image origin is in the lower left corner.
class TexCoord: # X
    _struct = ('<2f', 8)
    u: float = None                                     # First coordinate.
    v: float = None                                     # Second coordinate.

    def __init__(self, r: NiReader, half: bool = None):
        if isinstance(r, tuple): self.u,self.v=r; return
        if isinstance(r, float): self.u = r; self.v = half; return
        elif half: self.u = r.readHalf(); self.v = r.readHalf(); return
        self.u = r.readSingle()
        self.v = r.readSingle()

# Describes the order of scaling and rotation matrices. Translate, Scale, Rotation, Center are from TexDesc.
# Back = inverse of Center. FromMaya = inverse of the V axis with a positive translation along V of 1 unit.
class TransformMethod(Enum): # X
    Maya_Deprecated = 0             # Center * Rotation * Back * Translate * Scale
    Max = 1                         # Center * Scale * Rotation * Translate * Back
    Maya = 2                        # Center * Rotation * Back * FromMaya * Translate * Scale

# NiTexturingProperty::Map. Texture description.
class TexDesc: # X
    image: Ref[NiObject] = None                         # Link to the texture image.
    source: Ref[NiObject] = None                        # NiSourceTexture object index.
    clampMode: TexClampMode = TexClampMode.WRAP_S_WRAP_T# 0=clamp S clamp T, 1=clamp S wrap T, 2=wrap S clamp T, 3=wrap S wrap T
    filterMode: TexFilterMode = TexFilterMode.FILTER_TRILERP # 0=nearest, 1=bilinear, 2=trilinear, 3=..., 4=..., 5=...
    flags: Flags = None                                 # Texture mode flags; clamp and filter mode stored in upper byte with 0xYZ00 = clamp mode Y, filter mode Z.
    maxAnisotropy: int = None
    uvSet: int = 0                                      # The texture coordinate set in NiGeometryData that this texture slot will use.
    pS2L: int = 0                                       # L can range from 0 to 3 and are used to specify how fast a texture gets blurry.
    pS2K: int = -75                                     # K is used as an offset into the mipmap levels and can range from -2047 to 2047. Positive values push the mipmap towards being blurry and negative values make the mipmap sharper.
    unknown1: int = None                                # Unknown, 0 or 0x0101?
    # NiTextureTransform
    translation: TexCoord = None                        # The UV translation.
    scale: TexCoord = TexCoord(1.0, 1.0)                # The UV scale.
    rotation: float = 0.0                               # The W axis rotation in texture space.
    transformMethod: TransformMethod = 0                # Depending on the source, scaling can occur before or after rotation.
    center: TexCoord = None                             # The origin around which the texture rotates.

    def __init__(self, r: NiReader):
        if r.v <= 0x03010000: self.image = X[NiImage].ref(r)
        if r.v >= 0x0303000D: self.source = X[NiSourceTexture].ref(r)
        if r.v <= 0x14000005:
            self.clampMode = TexClampMode(r.readUInt32())
            self.filterMode = TexFilterMode(r.readUInt32())
        if r.v >= 0x14010003: self.flags = Flags(r.readUInt16())
        if r.v >= 0x14050004: self.maxAnisotropy = r.readUInt16()
        if r.v <= 0x14000005: self.uvSet = r.readUInt32()
        if r.v <= 0x0A040001:
            self.pS2L = r.readInt16()
            self.pS2K = r.readInt16()
        if r.v <= 0x0401000C: self.unknown1 = r.readUInt16()
        # NiTextureTransform
        if r.v >= 0x0A010000 and Z.readBool(r):
            self.translation = r.readS(TexCoord)
            self.scale = r.readS(TexCoord)
            self.rotation = r.readSingle()
            self.transformMethod = TransformMethod(r.readUInt32())
            self.center = r.readS(TexCoord)

# NiTexturingProperty::ShaderMap. Shader texture description.
class ShaderTexDesc: # Y
    map: TexDesc = None
    mapId: int = 0                                      # Unique identifier for the Gamebryo shader system.

    def __init__(self, r: NiReader):
        if Z.readBool(r):
            self.map = TexDesc(r)
            self.mapId = r.readUInt32()

# List of three vertex indices.
class Triangle: # X
    _struct = ('<3H', 6)
    def __init__(self, r: NiReader):
        if isinstance(r, tuple): self.v1,self.v2,self.v3=r; return
        self.v1: int = r.readUInt16()                   # First vertex index.
        self.v2: int = r.readUInt16()                   # Second vertex index.
        self.v3: int = r.readUInt16()                   # Third vertex index.

class VertexFlags(Flag): # Y
    Vertex = 1 << 4
    UVs = 1 << 5
    UVs_2 = 1 << 6
    Normals = 1 << 7
    Tangents = 1 << 8
    Vertex_Colors = 1 << 9
    Skinned = 1 << 10
    Land_Data = 1 << 11
    Eye_Data = 1 << 12
    Instance = 1 << 13
    Full_Precision = 1 << 14

class BSVertexData: # Z
    vertex: Vector3 = None
    bitangentX: float = None
    unknownInt: int = 0
    uv: TexCoord = None
    normal: Vector3 = None
    bitangentY: int = 0
    tangent: Vector3 = None
    bitangentZ: int = 0
    vertexColors: Color4 = None
    boneWeights: list[float] = None
    boneIndices: bytearray = None
    eyeData: float = None

    def __init__(self, r: NiReader, arg: VertexFlags, sse: bool):
        full = sse or VertexFlags.Full_Precision in arg
        tangents = VertexFlags.Tangents in arg
        if arg.HasFlag(VertexFlags.Vertex):
            self.vertex = r.readVector3() if full else r.readHalfVector3()
            if tangents:
                self.bitangentX = r.readSingle() if full else r.readHalf()
            else:
                self.unknownInt = r.readUInt32() if full else r.readUInt16()
        if (arg.HasFlag(VertexFlags.UVs)): self.uv = TexCoord(r, true)
        if arg.HasFlag(VertexFlags.Normals):
            self.normal = Vector3(r.readByte(), r.readByte(), r.readByte())
            self.bitangentY = r.readByte()
            if tangents: self.tangent = Vector3(r.readByte(), r.readByte(), r.readByte())
            if tangents: self.bitangentZ = r.readByte()
        if arg.HasFlag(VertexFlags.Vertex_Colors): self.vertexColors = Color4(r.readBytes(4))
        if arg.HasFlag(VertexFlags.Skinned):
            self.boneWeights = [r.readHalf(), r.readHalf(), r.readHalf(), r.readHalf()]
            self.boneIndices = r.readBytes(4)
        if arg.HasFlag(VertexFlags.Eye_Data): self.eyeData = r.readSingle()

# BSVertexDataSSE -> BSVertexData(r, ARG, true)

class BSVertexDesc: # Y
    _struct = ('<5bHb', 8)
    def __init__(self, r: NiReader):
        if isinstance(r, tuple): self.vF1,self.vF2,self.vF3,self.vF4,self.vF5,self.vertexAttributes,self.vF8=(r[0],r[1],r[2],r[3],r[4],VertexFlags(r[5]),r[6]); return
        self.vF1: int = r.readByte()
        self.vF2: int = r.readByte()
        self.vF3: int = r.readByte()
        self.vF4: int = r.readByte()
        self.vF5: int = r.readByte()
        self.vertexAttributes: VertexFlags = VertexFlags(r.readUInt16())
        self.vF8: int = r.readByte()

# Skinning data for a submesh, optimized for hardware skinning. Part of NiSkinPartition.
class SkinPartition: # Y
    numVertices: int = None                             # Number of vertices in this submesh.
    numTriangles: int = None                            # Number of triangles in this submesh.
    numBones: int = None                                # Number of bones influencing this submesh.
    numStrips: int = None                               # Number of strips in this submesh (zero if not stripped).
    numWeightsPerVertex: int = None                     # Number of weight coefficients per vertex. The Gamebryo engine seems to work well only if this number is equal to 4, even if there are less than 4 influences per vertex.
    bones: list[int] = None                             # List of bones.
    vertexMap: list[int] = None                         # Maps the weight/influence lists in this submesh to the vertices in the shape being skinned.
    vertexWeights: list[list[float]] = None             # The vertex weights.
    stripLengths: list[int] = None                      # The strip lengths.
    strips: list[list[int]] = None                      # The strips.
    triangles: list[Triangle] = None                    # The triangles.
    boneIndices: list[bytearray] = None                 # Bone indices, they index into 'Bones'.
    unknownShort: int = None                            # Unknown
    vertexDesc: BSVertexDesc = None
    trianglesCopy: list[Triangle] = None

    def __init__(self, r: NiReader):
        self.numVertices = r.readUInt16()
        self.numTriangles = (self.numVertices / 3) # calculated
        self.numBones = r.readUInt16()
        self.numStrips = r.readUInt16()
        self.numWeightsPerVertex = r.readUInt16()
        self.bones = r.readPArray(None, 'H', self.numBones)
        if r.v <= 0x0A000102:
            self.vertexMap = r.readPArray(None, 'H', self.numVertices)
            self.vertexWeights = r.readFArray(lambda k: r.readPArray(None, 'f', self.numWeightsPerVertex), self.numVertices)
            self.stripLengths = r.readPArray(None, 'H', self.numStrips)
            if self.numStrips != 0: self.strips = r.readFArray(lambda k, i: r.readPArray(None, 'H', self.stripLengths[i]), self.numStrips)
            else: self.triangles = r.readSArray(Triangle, self.numTriangles)
        elif r.v >= 0x0A010000:
            if Z.readBool(r): self.vertexMap = r.readPArray(None, 'H', self.numVertices)
            hasVertexWeights = Z.readBool8(r)
            if hasVertexWeights == 1: self.vertexWeights = r.readFArray(lambda k: r.readPArray(None, 'f', self.numWeightsPerVertex), self.numVertices)
            if hasVertexWeights == 15: self.vertexWeights = r.readFArray(lambda k: r.readFArray(lambda z: r.readHalf(), self.numWeightsPerVertex), self.numVertices)
            self.stripLengths = r.readPArray(None, 'H', self.numStrips)
            if Z.readBool(r):
                if self.numStrips != 0: self.strips = r.readFArray(lambda k, i: r.readPArray(None, 'H', self.stripLengths[i]), self.numStrips)
                else: self.triangles = r.readSArray(Triangle, self.numTriangles)
        if Z.readBool(r): self.boneIndices = r.readFArray(lambda k: r.readBytes(self.numWeightsPerVertex), self.numVertices)
        if r.uv2 > 34: self.unknownShort = r.readUInt16()
        if r.uv2 == 100:
            self.vertexDesc = r.readS(BSVertexDesc)
            self.trianglesCopy = r.readSArray(Triangle, self.numTriangles)

# A plane.
class NiPlane: # Y
    _struct = ('<4f', 16)
    def __init__(self, r: NiReader):
        if isinstance(r, tuple): self.normal,self.constant=(array(r[0:3]),r[3]); return
        self.normal: Vector3 = r.readVector3()          # The plane normal.
        self.constant: float = r.readSingle()           # The plane constant.

# A sphere.
class NiBound: # Y
    _struct = ('<4f', 16)
    def __init__(self, r: NiReader):
        if isinstance(r, tuple): self.center,self.radius=(array(r[0:3]),r[3]); return
        self.center: Vector3 = r.readVector3()          # The sphere's center.
        self.radius: float = r.readSingle()             # The sphere's radius.

class NiQuatTransform: # Z
    def __init__(self, r: NiReader):
        self.translation: Vector3 = r.readVector3()
        self.rotation: Quaternion = r.readQuaternionWFirst()
        self.scale: float = r.readSingle()
        self.trsValid: list[bool] = [Z.readBool(r), Z.readBool(r), Z.readBool(r)] if r.v <= 0x0A01006D else None # Whether each transform component is valid.

class NiTransform: # X
    def __init__(self, r: NiReader):
        self.rotation: Matrix4x4 = r.readMatrix3x3As4x4() # The rotation part of the transformation matrix.
        self.translation: Vector3 = r.readVector3()     # The translation vector.
        self.scale: float = r.readSingle()              # Scaling part (only uniform scaling is supported).

# Bethesda Animation. Furniture entry points. It specifies the direction(s) from where the actor is able to enter (and leave) the position.
class FurnitureEntryPoints(Flag): # Z
    Front = 0                       # front entry point
    Behind = 1 << 1                 # behind entry point
    Right = 1 << 2                  # right entry point
    Left = 1 << 3                   # left entry point
    Up = 1 << 4                     # up entry point - unknown function. Used on some beds in Skyrim, probably for blocking of sleeping position.

# Bethesda Animation. Animation type used on this position. This specifies the function of this position.
class AnimationType(Enum): # Z
    Sit = 1                         # Actor use sit animation.
    Sleep = 2                       # Actor use sleep animation.
    Lean = 4                        # Used for lean animations?

# Bethesda Animation. Describes a furniture position?
class FurniturePosition: # Z
    offset: Vector3 = None                              # Offset of furniture marker.
    orientation: int = None                             # Furniture marker orientation.
    positionRef1: int = 0                               # Refers to a furnituremarkerxx.nif file. Always seems to be the same as Position Ref 2.
    positionRef2: int = 0                               # Refers to a furnituremarkerxx.nif file. Always seems to be the same as Position Ref 1.
    heading: float = None                               # Similar to Orientation, in float form.
    animationType: AnimationType = 0                    # Unknown
    entryProperties: FurnitureEntryPoints = 0           # Unknown/unused in nif?

    def __init__(self, r: NiReader):
        self.offset = r.readVector3()
        if r.uv2 <= 34:
            self.orientation = r.readUInt16()
            self.positionRef1 = r.readByte()
            self.positionRef2 = r.readByte()
        else:
            self.heading = r.readSingle()
            self.animationType = AnimationType(r.readUInt16())
            self.entryProperties = FurnitureEntryPoints(r.readUInt16())

# Geometry morphing data component.
class Morph: # X
    frameName: str = None                               # Name of the frame.
    interpolation: KeyType = 0                          # Unlike most objects, the presense of this value is not conditional on there being keys.
    keys: list[Key[float]] = None                       # The morph key frames.
    legacyWeight: float = None
    vectors: list[Vector3] = None                       # Morph vectors.

    def __init__(self, r: NiReader, numVertices: int):
        if r.v >= 0x0A01006A: self.frameName = Z.string(r)
        if r.v <= 0x0A010000:
            numKeys = r.readUInt32()
            self.interpolation = KeyType(r.readUInt32())
            self.keys = r.readFArray(lambda z: Key[float]('[float]', r, Interpolation), self.numKeys)
        if r.v >= 0x0A010068 and r.v <= 0x14010002 and r.uv2 < 10: self.legacyWeight = r.readSingle()
        self.vectors = r.readPArray(array, '3f', numVertices)

# particle array entry
class Particle: # X
    _struct = ('<9f2H', 40)
    def __init__(self, r: NiReader):
        if isinstance(r, tuple): self.velocity,self.unknownVector,self.lifetime,self.lifespan,self.timestamp,self.unknownShort,self.vertexId=(array(r[0:3]),array(r[3:6]),r[6],r[7],r[8],r[9],r[10]); return
        self.velocity: Vector3 = r.readVector3()        # Particle velocity
        self.unknownVector: Vector3 = r.readVector3()   # Unknown
        self.lifetime: float = r.readSingle()           # The particle age.
        self.lifespan: float = r.readSingle()           # Maximum age of the particle.
        self.timestamp: float = r.readSingle()          # Timestamp of the last update.
        self.unknownShort: int = r.readUInt16()         # Unknown short
        self.vertexId: int = r.readUInt16()             # Particle/vertex index matches array index

# NiSkinData::BoneData. Skinning data component.
class BoneData: # X
    skinTransform: NiTransform = None                   # Offset of the skin from this bone in bind position.
    boundingSphereOffset: Vector3 = None                # Translation offset of a bounding sphere holding all vertices. (Note that its a Sphere Containing Axis Aligned Box not a minimum volume Sphere)
    boundingSphereRadius: float = None                  # Radius for bounding sphere holding all vertices.
    unknown13Shorts: list[int] = None                   # Unknown, always 0?
    vertexWeights: list[BoneVertData] = None            # The vertex weights.

    def __init__(self, r: NiReader, arg: int):
        self.skinTransform = NiTransform(r)
        self.boundingSphereOffset = r.readVector3()
        self.boundingSphereRadius = r.readSingle()
        if r.v == 0x14030009 and (r.uv == 0x20000) or (r.uv == 0x30000): self.unknown13Shorts = r.readPArray(None, 'h', 13)
        self.vertexWeights = r.readL16SArray(BoneVertData) if r.v <= 0x04020100 else \
            r.readL16SArray(BoneVertData) if r.v >= 0x04020200 and arg == 1 else \
            r.readL16FArray(lambda z: BoneVertData(r, False)) if r.V >= 0x14030101 and arg == 15 else None

# Bethesda Havok. Collision filter info representing Layer, Flags, Part Number, and Group all combined into one uint.
class HavokFilter: # Z
    def __init__(self, r: NiReader):
        self.layerOb: OblivionLayer = OblivionLayer(r.readByte()) if r.v <= 0x14000005 and (r.uv2 < 16) else OblivionLayer.OL_STATIC # The layer the collision belongs to.
        self.layerFo: Fallout3Layer = Fallout3Layer(r.readByte()) if (r.v == 0x14020007) and (r.uv2 <= 34) else Fallout3Layer.FOL_STATIC # The layer the collision belongs to.
        self.layerSk: SkyrimLayer = SkyrimLayer(r.readByte()) if (r.v == 0x14020007) and (r.uv2 > 34) else SkyrimLayer.SKYL_STATIC # The layer the collision belongs to.
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
class HavokMaterial: # Z
    def __init__(self, r: NiReader):
        self.unknownInt: int = r.readUInt32() if r.v <= 0x0A000102 else 0
        self.materialOb: OblivionHavokMaterial = OblivionHavokMaterial(r.readUInt32()) if r.v <= 0x14000005 and (r.uv2 < 16) else 0 # The material of the shape.
        self.materialFo: Fallout3HavokMaterial = Fallout3HavokMaterial(r.readUInt32()) if (r.v == 0x14020007) and (r.uv2 <= 34) else 0 # The material of the shape.
        self.materialSk: SkyrimHavokMaterial = SkyrimHavokMaterial(r.readUInt32()) if (r.v == 0x14020007) and (r.uv2 > 34) else 0 # The material of the shape.

# Determines how the raw image data is stored in NiRawImageData.
class ImageType(Enum): # Y
    RGB = 1                         # Colors store red, blue, and green components.
    RGBA = 2                        # Colors store red, blue, green, and alpha components.

# Box Bounding Volume
class BoxBV: # X
    def __init__(self, r: NiReader):
        self.center: Vector3 = r.readVector3()
        self.axis: Matrix4x4 = r.readMatrix3x3As4x4()
        self.extent: Vector3 = r.readVector3()

# Capsule Bounding Volume
class CapsuleBV: # Y
    _struct = ('<8f', 32)
    def __init__(self, r: NiReader):
        if isinstance(r, tuple): self.center,self.origin,self.extent,self.radius=(array(r[0:3]),array(r[3:6]),r[6],r[7]); return
        self.center: Vector3 = r.readVector3()
        self.origin: Vector3 = r.readVector3()
        self.extent: float = r.readSingle()
        self.radius: float = r.readSingle()

class HalfSpaceBV: # Y
    _struct = ('<7f', 28)
    def __init__(self, r: NiReader):
        if isinstance(r, tuple): self.plane,self.center=(NiPlane(r[0:4]),array(r[4:7])); return
        self.plane: NiPlane = r.readS(NiPlane)
        self.center: Vector3 = r.readVector3()

class BoundingVolume: # X
    collisionType: BoundVolumeType = 0                  # Type of collision data.
    sphere: NiBound = None
    box: BoxBV = None
    capsule: CapsuleBV = None
    union: UnionBV = None
    halfSpace: HalfSpaceBV = None

    def __init__(self, r: NiReader):
        self.collisionType = BoundVolumeType(r.readUInt32())
        match self.collisionType:
            case BoundVolumeType.SPHERE_BV: self.sphere = r.readS(NiBound)
            case BoundVolumeType.BOX_BV: self.box = BoxBV(r)
            case BoundVolumeType.CAPSULE_BV: self.capsule = r.readS(CapsuleBV)
            case BoundVolumeType.UNION_BV: self.union = UnionBV(r)
            case BoundVolumeType.HALFSPACE_BV: self.halfSpace = r.readS(HalfSpaceBV)

class UnionBV: # Y
    def __init__(self, r: NiReader):
        self.boundingVolumes: list[BoundingVolume] = r.readL32FArray(lambda z: BoundingVolume(r))

class MorphWeight: # Y
    def __init__(self, r: NiReader):
        self.interpolator: Ref[NiObject] = X[NiInterpolator].ref(r)
        self.weight: float = r.readSingle()

#endregion

#region NIF Objects

# These are the main units of data that NIF files are arranged in.
# They are like C classes and can contain many pieces of data.
# The only differences between these and compounds is that these are treated as object types by the NIF format and can inherit from other classes.

# Abstract object type.
class NiObject: # X
    def __init__(self, r: NiReader):
        pass

    @staticmethod
    def read(r: NiReader, nodeType: str) -> NiObject:
        # print(f'{nodeType}: {r.tell()}')
        def type(o: NiObject) -> NiObject: setattr(o, '$type', nodeType); return o;
        match nodeType:
            case 'NiNode': return type(NiNode(r))
            case 'NiTriShape': return type(NiTriShape(r))
            case 'NiTexturingProperty': return type(NiTexturingProperty(r))
            case 'NiSourceTexture': return type(NiSourceTexture(r))
            case 'NiMaterialProperty': return type(NiMaterialProperty(r))
            case 'NiMaterialColorController': return type(NiMaterialColorController(r))
            case 'NiTriShapeData': return type(NiTriShapeData(r))
            case 'RootCollisionNode': return type(RootCollisionNode(r))
            case 'NiStringExtraData': return type(NiStringExtraData(r))
            case 'NiSkinInstance': return type(NiSkinInstance(r))
            case 'NiSkinData': return type(NiSkinData(r))
            case 'NiAlphaProperty': return type(NiAlphaProperty(r))
            case 'NiZBufferProperty': return type(NiZBufferProperty(r))
            case 'NiVertexColorProperty': return type(NiVertexColorProperty(r))
            case 'NiBSAnimationNode': return type(NiBSAnimationNode(r))
            case 'NiBSParticleNode': return type(NiBSParticleNode(r))
            case 'NiParticles': return type(NiParticles(r))
            case 'NiParticlesData': return type(NiParticlesData(r))
            case 'NiRotatingParticles': return type(NiRotatingParticles(r))
            case 'NiRotatingParticlesData': return type(NiRotatingParticlesData(r))
            case 'NiAutoNormalParticles': return type(NiAutoNormalParticles(r))
            case 'NiAutoNormalParticlesData': return type(NiAutoNormalParticlesData(r))
            case 'NiUVController': return type(NiUVController(r))
            case 'NiUVData': return type(NiUVData(r))
            case 'NiTextureEffect': return type(NiTextureEffect(r))
            case 'NiTextKeyExtraData': return type(NiTextKeyExtraData(r))
            case 'NiVertWeightsExtraData': return type(NiVertWeightsExtraData(r))
            case 'NiParticleSystemController': return type(NiParticleSystemController(r))
            case 'NiBSPArrayController': return type(NiBSPArrayController(r))
            case 'NiGravity': return type(NiGravity(r))
            case 'NiParticleBomb': return type(NiParticleBomb(r))
            case 'NiParticleColorModifier': return type(NiParticleColorModifier(r))
            case 'NiParticleGrowFade': return type(NiParticleGrowFade(r))
            case 'NiParticleMeshModifier': return type(NiParticleMeshModifier(r))
            case 'NiParticleRotation': return type(NiParticleRotation(r))
            case 'NiKeyframeController': return type(NiKeyframeController(r))
            case 'NiKeyframeData': return type(NiKeyframeData(r))
            case 'NiColorData': return type(NiColorData(r))
            case 'NiGeomMorpherController': return type(NiGeomMorpherController(r))
            case 'NiMorphData': return type(NiMorphData(r))
            case 'AvoidNode': return type(AvoidNode(r))
            case 'NiVisController': return type(NiVisController(r))
            case 'NiVisData': return type(NiVisData(r))
            case 'NiAlphaController': return type(NiAlphaController(r))
            case 'NiFloatData': return type(NiFloatData(r))
            case 'NiPosData': return type(NiPosData(r))
            case 'NiBillboardNode': return type(NiBillboardNode(r))
            case 'NiShadeProperty': return type(NiShadeProperty(r))
            case 'NiWireframeProperty': return type(NiWireframeProperty(r))
            case 'NiCamera': return type(NiCamera(r))
            case 'NiPathController': return type(NiPathController(r))
            case 'NiPixelData': return type(NiPixelData(r))
            case 'NiBinaryExtraData': return type(NiBinaryExtraData(r))
            case 'NiTriStrips': return type(NiTriStrips(r))
            case 'NiTriStripsData': return type(NiTriStripsData(r))
            case 'BSXFlags': return type(BSXFlags(r))
            case 'bhkNiTriStripsShape': return type(bhkNiTriStripsShape(r))
            case 'bhkMoppBvTreeShape': return type(bhkMoppBvTreeShape(r))
            case 'bhkRigidBody': return type(bhkRigidBody(r))
            case 'bhkCollisionObject': return type(bhkCollisionObject(r))
            case 'bhkRigidBodyT': return type(bhkRigidBodyT(r))
            case 'bhkConvexVerticesShape': return type(bhkConvexVerticesShape(r))
            case 'bhkListShape': return type(bhkListShape(r))
            case 'BSFurnitureMarker': return type(BSFurnitureMarker(r))
            case 'bhkBoxShape': return type(bhkBoxShape(r))
            case 'bhkConvexTransformShape': return type(bhkConvexTransformShape(r))
            case 'NiSpecularProperty': return type(NiSpecularProperty(r))
            case 'NiControllerSequence': return type(NiControllerSequence(r))
            case 'NiControllerManager': return type(NiControllerManager(r))
            case 'NiMultiTargetTransformController': return type(NiMultiTargetTransformController(r))
            case 'NiTransformInterpolator': return type(NiTransformInterpolator(r))
            case 'NiTransformData': return type(NiTransformData(r))
            case 'NiStringPalette': return type(NiStringPalette(r))
            case 'NiDefaultAVObjectPalette': return type(NiDefaultAVObjectPalette(r))
            case 'NiStencilProperty': return type(NiStencilProperty(r))
            case _: log(f'Tried to read an unsupported NiObject type ({nodeType}).'); node = None
        return node

# LEGACY (pre-10.1). Abstract base class for particle system modifiers.
class NiParticleModifier(NiObject): # X
    nextModifier: Ref[NiObject] = None                  # Next particle modifier.
    controller: Ref[NiObject] = None                    # Points to the particle system controller parent.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.nextModifier = X[NiParticleModifier].ref(r)
        if r.v >= 0x04000002: self.controller = X[NiParticleSystemController].ptr(r)

class BroadPhaseType(Enum): # Z
    BROAD_PHASE_INVALID = 0
    BROAD_PHASE_ENTITY = 1
    BROAD_PHASE_PHANTOM = 2
    BROAD_PHASE_BORDER = 3

class hkWorldObjCinfoProperty: # Z
    def __init__(self, r: NiReader):
        self.data: int = r.readUInt32()
        self.size: int = r.readUInt32()
        self.capacityandFlags: int = r.readUInt32()

# The base type of most Bethesda-specific Havok-related NIF objects.
class bhkRefObject(NiObject): # Z
    def __init__(self, r: NiReader):
        super().__init__(r)

# Havok objects that can be saved and loaded from disk?
class bhkSerializable(bhkRefObject): # Z
    def __init__(self, r: NiReader):
        super().__init__(r)

# Havok objects that have a position in the world?
class bhkWorldObject(bhkSerializable): # Z
    shape: Ref[NiObject] = None                         # Link to the body for this collision object.
    unknownInt: int = 0
    havokFilter: HavokFilter = None
    unused: bytearray = None                            # Garbage data from memory.
    broadPhaseType: BroadPhaseType = 1
    unusedBytes: bytearray = None
    cinfoProperty: hkWorldObjCinfoProperty = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.shape = X[bhkShape].ref(r)
        if r.v <= 0x0A000100: self.unknownInt = r.readUInt32()
        self.havokFilter = HavokFilter(r)
        self.unused = r.readBytes(4)
        self.broadPhaseType = BroadPhaseType(r.readByte())
        self.unusedBytes = r.readBytes(3)
        self.cinfoProperty = hkWorldObjCinfoProperty(r)

# A havok node, describes physical properties.
class bhkEntity(bhkWorldObject): # Z
    def __init__(self, r: NiReader):
        super().__init__(r)

# This is the default body type for all "normal" usable and static world objects. The "T" suffix
# marks this body as active for translation and rotation, a normal bhkRigidBody ignores those
# properties. Because the properties are equal, a bhkRigidBody may be renamed into a bhkRigidBodyT and vice-versa.
class bhkRigidBody(bhkEntity): # Z
    collisionResponse: hkResponseType = hkResponseType.RESPONSE_SIMPLE_CONTACT # How the body reacts to collisions. See hkResponseType for hkpWorld default implementations.
    unusedByte1: int = 0                                # Skipped over when writing Collision Response and Callback Delay.
    processContactCallbackDelay: int = 0xffff           # Lowers the frequency for processContactCallbacks. A value of 5 means that a callback is raised every 5th frame. The default is once every 65535 frames.
    unknownInt1: int = 0                                # Unknown.
    havokFilterCopy: HavokFilter = None                 # Copy of Havok Filter
    unused2: bytearray = None                           # Garbage data from memory. Matches previous Unused value.
    unknownInt2: int = 0
    collisionResponse2: hkResponseType = hkResponseType.RESPONSE_SIMPLE_CONTACT
    unusedByte2: int = 0                                # Skipped over when writing Collision Response and Callback Delay.
    processContactCallbackDelay2: int = 0xffff
    translation: Vector4 = None                         # A vector that moves the body by the specified amount. Only enabled in bhkRigidBodyT objects.
    rotation: Quaternion = None                         # The rotation Yaw/Pitch/Roll to apply to the body. Only enabled in bhkRigidBodyT objects.
    linearVelocity: Vector4 = None                      # Linear velocity.
    angularVelocity: Vector4 = None                     # Angular velocity.
    inertiaTensor: Matrix3x4 = None                     # Defines how the mass is distributed among the body, i.e. how difficult it is to rotate around any given axis.
    center: Vector4 = None                              # The body's center of mass.
    mass: float = 1.0                                   # The body's mass in kg. A mass of zero represents an immovable object.
    linearDamping: float = 0.1                          # Reduces the movement of the body over time. A value of 0.1 will remove 10% of the linear velocity every second.
    angularDamping: float = 0.05                        # Reduces the movement of the body over time. A value of 0.05 will remove 5% of the angular velocity every second.
    timeFactor: float = 1.0
    gravityFactor: float = 1.0
    friction: float = 0.5                               # How smooth its surfaces is and how easily it will slide along other bodies.
    rollingFrictionMultiplier: float = None
    restitution: float = 0.4                            # How "bouncy" the body is, i.e. how much energy it has after colliding. Less than 1.0 loses energy, greater than 1.0 gains energy.
                                                        #     If the restitution is not 0.0 the object will need extra CPU for all new collisions.
    maxLinearVelocity: float = 104.4                    # Maximal linear velocity.
    maxAngularVelocity: float = 31.57                   # Maximal angular velocity.
    penetrationDepth: float = 0.15                      # The maximum allowed penetration for this object.
                                                        #     This is a hint to the engine to see how much CPU the engine should invest to keep this object from penetrating.
                                                        #     A good choice is 5% - 20% of the smallest diameter of the object.
    motionSystem: hkMotionType = hkMotionType.MO_SYS_DYNAMIC # Motion system? Overrides Quality when on Keyframed?
    deactivatorType: hkDeactivatorType = hkDeactivatorType.DEACTIVATOR_NEVER # The initial deactivator type of the body.
    enableDeactivation: bool = 1
    solverDeactivation: hkSolverDeactivation = hkSolverDeactivation.SOLVER_DEACTIVATION_OFF # How aggressively the engine will try to zero the velocity for slow objects. This does not save CPU.
    qualityType: hkQualityType = hkQualityType.MO_QUAL_FIXED # The type of interaction with other objects.
    unknownFloat1: float = None
    unknownBytes1: bytearray = None                     # Unknown.
    unknownBytes2: bytearray = None                     # Unknown. Skyrim only.
    constraints: list[Ref[NiObject]] = None
    bodyFlags: int = 0                                  # 1 = respond to wind

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.collisionResponse = hkResponseType(r.readByte())
        self.unusedByte1 = r.readByte()
        self.processContactCallbackDelay = r.readUInt16()
        if r.v >= 0x0A010000:
            self.unknownInt1 = r.readUInt32()
            self.havokFilterCopy = HavokFilter(r)
            self.unused2 = r.readBytes(4)
            if r.uv2 > 34: self.unknownInt2 = r.readUInt32()
            self.collisionResponse2 = hkResponseType(r.readByte())
            self.unusedByte2 = r.readByte()
            self.processContactCallbackDelay2 = r.readUInt16()
        if r.uv2 <= 34: self.unknownInt2 = r.readUInt32()
        self.translation = r.readVector4()
        self.rotation = r.readQuaternion()
        self.linearVelocity = r.readVector4()
        self.angularVelocity = r.readVector4()
        self.inertiaTensor = r.readMatrix3x4()
        self.center = r.readVector4()
        self.mass = r.readSingle()
        self.linearDamping = r.readSingle()
        self.angularDamping = r.readSingle()
        if r.uv2 > 34:
            self.timeFactor = r.readSingle()
            if r.uv2 != 130: self.gravityFactor = r.readSingle()
        self.friction = r.readSingle()
        if r.uv2 > 34: self.rollingFrictionMultiplier = r.readSingle()
        self.restitution = r.readSingle()
        if r.v >= 0x0A010000:
            self.maxLinearVelocity = r.readSingle()
            self.maxAngularVelocity = r.readSingle()
            if r.uv2 != 130: self.penetrationDepth = r.readSingle()
        self.motionSystem = hkMotionType(r.readByte())
        if r.uv2 <= 34: self.deactivatorType = hkDeactivatorType(r.readByte())
        else: self.enableDeactivation = Z.readBool(r)
        self.solverDeactivation = hkSolverDeactivation(r.readByte())
        self.qualityType = hkQualityType(r.readByte())
        if r.uv2 == 130:
            self.penetrationDepth = r.readSingle()
            self.unknownFloat1 = r.readSingle()
        self.unknownBytes1 = r.readBytes(12)
        if r.uv2 > 34: self.unknownBytes2 = r.readBytes(4)
        self.constraints = r.readL32FArray(X[bhkSerializable].ref)
        self.bodyFlags = r.readUInt32() if r.uv2 < 76 else r.readUInt16()

# The "T" suffix marks this body as active for translation and rotation.
class bhkRigidBodyT(bhkRigidBody): # Z
    def __init__(self, r: NiReader):
        super().__init__(r)

# A Havok Shape?
class bhkShape(bhkSerializable): # Z
    def __init__(self, r: NiReader):
        super().__init__(r)

# Transforms a shape.
class bhkTransformShape(bhkShape): # Z
    shape: Ref[NiObject] = None                         # The shape that this object transforms.
    material: HavokMaterial = None                      # The material of the shape.
    radius: float = None
    unused: bytearray = None                            # Garbage data from memory.
    transform: Matrix4x4 = None                         # A transform matrix.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.shape = X[bhkShape].ref(r)
        self.material = HavokMaterial(r)
        self.radius = r.readSingle()
        self.unused = r.readBytes(8)
        self.transform = r.readMatrix4x4()

# A havok shape, perhaps with a bounding sphere for quick rejection in addition to more detailed shape data?
class bhkSphereRepShape(bhkShape): # Z
    material: HavokMaterial = None                      # The material of the shape.
    radius: float = None                                # The radius of the sphere that encloses the shape.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.material = HavokMaterial(r)
        self.radius = r.readSingle()

# A havok shape.
class bhkConvexShape(bhkSphereRepShape): # Z
    def __init__(self, r: NiReader):
        super().__init__(r)

# A box.
class bhkBoxShape(bhkConvexShape): # Z
    unused: bytearray = None                            # Not used. The following wants to be aligned at 16 bytes.
    dimensions: Vector3 = None                          # A cube stored in Half Extents. A unit cube (1.0, 1.0, 1.0) would be stored as 0.5, 0.5, 0.5.
    unusedFloat: float = None                           # Unused as Havok stores the Half Extents as hkVector4 with the W component unused.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.unused = r.readBytes(8)
        self.dimensions = r.readVector3()
        self.unusedFloat = r.readSingle()

# A convex shape built from vertices. Note that if the shape is used in
# a non-static object (such as clutter), then they will simply fall
# through ground when they are under a bhkListShape.
class bhkConvexVerticesShape(bhkConvexShape): # Z
    verticesProperty: hkWorldObjCinfoProperty = None
    normalsProperty: hkWorldObjCinfoProperty = None
    vertices: list[Vector4] = None                      # Vertices. Fourth component is 0. Lexicographically sorted.
    normals: list[Vector4] = None                       # Half spaces as determined by the set of vertices above. First three components define the normal pointing to the exterior, fourth component is the signed distance of the separating plane to the origin: it is minus the dot product of v and n, where v is any vertex on the separating plane, and n is the normal. Lexicographically sorted.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.verticesProperty = hkWorldObjCinfoProperty(r)
        self.normalsProperty = hkWorldObjCinfoProperty(r)
        self.vertices = r.readL32PArray(array, '4f')
        self.normals = r.readL32PArray(array, '4f')

# A convex transformed shape?
class bhkConvexTransformShape(bhkTransformShape): # Z
    def __init__(self, r: NiReader):
        super().__init__(r)

# A tree-like Havok data structure stored in an assembly-like binary code?
class bhkBvTreeShape(bhkShape): # Z
    def __init__(self, r: NiReader):
        super().__init__(r)

# Memory optimized partial polytope bounding volume tree shape (not an entity).
class bhkMoppBvTreeShape(bhkBvTreeShape): # Z
    shape: Ref[NiObject] = None                         # The shape.
    unused: list[int] = None                            # Garbage data from memory. Referred to as User Data, Shape Collection, and Code.
    shapeScale: float = 1.0                             # Scale.
    moppDataSize: int = 0                               # Number of bytes for MOPP data.
    origin: Vector3 = None                              # Origin of the object in mopp coordinates. This is the minimum of all vertices in the packed shape along each axis, minus 0.1.
    scale: float = None                                 # The scaling factor to quantize the MOPP: the quantization factor is equal to 256*256 divided by this number. In Oblivion files, scale is taken equal to 256*256*254 / (size + 0.2) where size is the largest dimension of the bounding box of the packed shape.
    buildType: MoppDataBuildType = 0                    # Tells if MOPP Data was organized into smaller chunks (PS3) or not (PC)
    moppData: bytearray = None                          # The tree of bounding volume data.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.shape = X[bhkShape].ref(r)
        self.unused = r.readPArray(None, 'I', 3)
        self.shapeScale = r.readSingle()
        self.moppDataSize = r.readUInt32() # calculated
        if r.v >= 0x0A000102:
            self.origin = r.readVector3()
            self.scale = r.readSingle()
        if r.uv2 > 34: self.buildType = MoppDataBuildType(r.readByte())
        self.moppData = r.readBytes(self.moppDataSize)

# Havok collision object that uses multiple shapes?
class bhkShapeCollection(bhkShape): # Z
    def __init__(self, r: NiReader):
        super().__init__(r)

# A list of shapes.
# 
# Do not put a bhkPackedNiTriStripsShape in the Sub Shapes. Use a
# separate collision nodes without a list shape for those.
# 
# Also, shapes collected in a bhkListShape may not have the correct
# walking noise, so only use it for non-walkable objects.
class bhkListShape(bhkShapeCollection): # Z
    subShapes: list[Ref[NiObject]] = None               # List of shapes.
    material: HavokMaterial = None                      # The material of the shape.
    childShapeProperty: hkWorldObjCinfoProperty = None
    childFilterProperty: hkWorldObjCinfoProperty = None
    unknownInts: list[int] = None                       # Unknown.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.subShapes = r.readL32FArray(X[bhkShape].ref)
        self.material = HavokMaterial(r)
        self.childShapeProperty = hkWorldObjCinfoProperty(r)
        self.childFilterProperty = hkWorldObjCinfoProperty(r)
        self.unknownInts = r.readL32PArray(None, 'I')

# A shape constructed from a bunch of strips.
class bhkNiTriStripsShape(bhkShapeCollection): # Z
    material: HavokMaterial = None                      # The material of the shape.
    radius: float = 0.1
    unused: list[int] = None                            # Garbage data from memory though the last 3 are referred to as maxSize, size, and eSize.
    growBy: int = 1
    scale: Vector4 = array((1.0, 1.0, 1.0, 0.0))        # Scale. Usually (1.0, 1.0, 1.0, 0.0).
    stripsData: list[Ref[NiObject]] = None              # Refers to a bunch of NiTriStripsData objects that make up this shape.
    dataLayers: list[HavokFilter] = None                # Havok Layers for each strip data.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.material = HavokMaterial(r)
        self.radius = r.readSingle()
        self.unused = r.readPArray(None, 'I', 5)
        self.growBy = r.readUInt32()
        if r.v >= 0x0A010000: self.scale = r.readVector4()
        self.stripsData = r.readL32FArray(X[NiTriStripsData].ref)
        self.dataLayers = r.readL32FArray(lambda z: HavokFilter(r))

# A generic extra data object.
class NiExtraData(NiObject): # X
    name: str = None                                    # Name of this object.
    nextExtraData: Ref[NiObject] = None                 # Block number of the next extra data object.

    def __init__(self, r: NiReader):
        super().__init__(r)
        BSExtraData: bool = False
        if r.v >= 0x0A000100 and not BSExtraData: self.name = Z.string(r)
        if r.v <= 0x04020200: self.nextExtraData = X[NiExtraData].ref(r)

# Abstract base class for all interpolators of bool, float, NiQuaternion, NiPoint3, NiColorA, and NiQuatTransform data.
class NiInterpolator(NiObject): # Y
    def __init__(self, r: NiReader):
        super().__init__(r)

# Abstract base class for interpolators that use NiAnimationKeys (Key, KeyGrp) for interpolation.
class NiKeyBasedInterpolator(NiInterpolator): # Z
    def __init__(self, r: NiReader):
        super().__init__(r)

# An interpolator for transform keyframes.
class NiTransformInterpolator(NiKeyBasedInterpolator): # Z
    transform: NiQuatTransform = None
    data: Ref[NiObject] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.transform = NiQuatTransform(r)
        self.data = X[NiTransformData].ref(r)

class PathFlags(Flag): # X
    CVDataNeedsUpdate = 0
    CurveTypeOpen = 1 << 1
    AllowFlip = 1 << 2
    Bank = 1 << 3
    ConstantVelocity = 1 << 4
    Follow = 1 << 5
    Flip = 1 << 6

class InterpBlendFlags(Enum): # Z
    MANAGER_CONTROLLED = 1          # MANAGER_CONTROLLED

# Interpolator item for array in NiBlendInterpolator.
class InterpBlendItem: # Z
    interpolator: Ref[NiObject] = None                  # Reference to an interpolator.
    weight: float = None
    normalizedWeight: float = None
    priority: int = 0
    easeSpinner: float = None

    def __init__(self, r: NiReader):
        self.interpolator = X[NiInterpolator].ref(r)
        self.weight = r.readSingle()
        self.normalizedWeight = r.readSingle()
        if r.v <= 0x0A01006D: self.priority = r.readInt32()
        elif r.v >= 0x0A01006E: self.priority = r.readByte()
        self.easeSpinner = r.readSingle()

# Abstract base class for all NiInterpolators that blend the results of sub-interpolators together to compute a final weighted value.
class NiBlendInterpolator(NiInterpolator): # Z
    flags: InterpBlendFlags = 0
    arraySize: int = None
    arrayGrowBy: int = None
    weightThreshold: float = None
    # Flags conds
    singleTime: float = -3.402823466e+38
    highWeightsSum: float = -3.402823466e+38
    nextHighWeightsSum: float = -3.402823466e+38
    highEaseSpinner: float = -3.402823466e+38
    interpArrayItems: list[InterpBlendItem] = None
    # end Flags 1 conds
    managedControlled: bool = None
    onlyUseHighestWeight: bool = None
    interpCount: int = None
    singleIndex: int = None
    singleInterpolator: Ref[NiObject] = None
    highPriority: int = 0
    nextHighPriority: int = 0

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A010070: self.flags = InterpBlendFlags(r.readByte())
        if r.v <= 0x0A01006D:
            self.arraySize = r.readUInt16()
            self.arrayGrowBy = r.readUInt16()
        if r.v >= 0x0A01006E: self.arraySize = r.readByte()
        if r.v >= 0x0A010070: self.weightThreshold = r.readSingle()
        # Flags conds
        if r.v >= 0x0A010070 and (self.flags & 1) == 0:
            self.interpCount = r.readByte()
            self.singleIndex = r.readByte()
            self.highPriority = r.readSByte()
            self.nextHighPriority = r.readSByte()
            self.singleTime = r.readSingle()
            self.highWeightsSum = r.readSingle()
            self.nextHighWeightsSum = r.readSingle()
            self.highEaseSpinner = r.readSingle()
            self.interpArrayItems = r.readFArray(lambda z: InterpBlendItem(r), self.arraySize)
        # end Flags 1 conds
        if r.v <= 0x0A01006F:
            self.interpArrayItems = r.readFArray(lambda z: InterpBlendItem(r), self.arraySize)
            self.managedControlled = Z.readBool(r)
            self.weightThreshold = r.readSingle()
            self.onlyUseHighestWeight = Z.readBool(r)
        if r.v <= 0x0A01006D:
            self.interpCount = r.readUInt16()
            self.singleIndex = r.readUInt16()
        if r.v >= 0x0A01006E and r.v <= 0x0A01006F:
            self.interpCount = r.readByte()
            self.singleIndex = r.readByte()
        if r.v >= 0x0A01006C and r.v <= 0x0A01006F:
            self.singleInterpolator = X[NiInterpolator].ref(r)
            self.singleTime = r.readSingle()
        if r.v <= 0x0A01006D:
            self.highPriority = r.readInt32()
            self.nextHighPriority = r.readInt32()
        if r.v >= 0x0A01006E and r.v <= 0x0A01006F:
            self.highPriority = r.readByte()
            self.nextHighPriority = r.readByte()

# Abstract base class for NiObjects that support names, extra data, and time controllers.
class NiObjectNET(NiObject): # X
    skyrimShaderType: BSLightingShaderPropertyShaderType = 0 # Configures the main shader path
    name: str = None                                    # Name of this controllable object, used to refer to the object in .kf files.
    oldExtraPropName: str = None                        # (=NiStringExtraData)
    oldExtraInternalId: int = 0                         # ref
    oldExtraString: str = None                          # Extra string data.
    unknownByte: int = 0                                # Always 0.
    extraData: Ref[NiObject] = None                     # Extra data object index. (The first in a chain)
    extraDataList: list[Ref[NiObject]] = None           # List of extra data indices.
    controller: Ref[NiObject] = None                    # Controller object index. (The first in a chain)

    def __init__(self, r: NiReader):
        super().__init__(r)
        BSLightingShaderProperty: bool = False
        if r.uv2 >= 83 and BSLightingShaderProperty: self.skyrimShaderType = BSLightingShaderPropertyShaderType(r.readUInt32())
        self.name = Z.string(r)
        if r.v <= 0x02030000 and Z.readBool(r):
            self.oldExtraPropName = Z.string(r)
            self.oldExtraInternalId = r.readUInt32()
            self.oldExtraString = Z.string(r)
        if r.v <= 0x02030000: self.unknownByte = r.readByte()
        if r.v >= 0x03000000 and r.v <= 0x04020200: self.extraData = X[NiExtraData].ref(r)
        if r.v >= 0x0A000100: self.extraDataList = r.readL32FArray(X[NiExtraData].ref)
        if r.v >= 0x03000000: self.controller = X[NiTimeController].ref(r)

# This is the most common collision object found in NIF files. It acts as a real object that
# is visible and possibly (if the body allows for it) interactive. The node itself
# is simple, it only has three properties.
# For this type of collision object, bhkRigidBody or bhkRigidBodyT is generally used.
class NiCollisionObject(NiObject): # Y
    target: Ref[NiObject] = None                        # Index of the AV object referring to this collision object.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.target = X[NiAVObject].ptr(r)

# bhkNiCollisionObject flags. The flags 0x2, 0x100, and 0x200 are not seen in any NIF nor get/set by the engine.
class bhkCOFlags(Flag): # Z
    ACTIVE = 0
    NOTIFY = 1 << 2
    SET_LOCAL = 1 << 3
    DBG_DISPLAY = 1 << 4
    USE_VEL = 1 << 5
    RESET = 1 << 6
    SYNC_ON_UPDATE = 1 << 7
    ANIM_TARGETED = 1 << 10
    DISMEMBERED_LIMB = 1 << 11

# Havok related collision object?
class bhkNiCollisionObject(NiCollisionObject): # Z
    flags: bhkCOFlags = 1                               # Set to 1 for most objects, and to 41 for animated objects (ANIM_STATIC). Bits: 0=Active 2=Notify 3=Set Local 6=Reset.
    body: Ref[NiObject] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.flags = bhkCOFlags(r.readUInt16())
        self.body = X[bhkWorldObject].ref(r)

# Havok related collision object?
class bhkCollisionObject(bhkNiCollisionObject): # Z
    def __init__(self, r: NiReader):
        super().__init__(r)

# Abstract audio-visual base class from which all of Gamebryo's scene graph objects inherit.
class NiAVObject(NiObjectNET): # X
    flags: Flags = 14                                   # Basic flags for AV objects. For Bethesda streams above 26 only.
    translation: Vector3 = None                         # The translation vector.
    rotation: Matrix4x4 = None                          # The rotation part of the transformation matrix.
    scale: float = 1.0                                  # Scaling part (only uniform scaling is supported).
    velocity: Vector3 = None                            # Unknown function. Always seems to be (0, 0, 0)
    properties: list[Ref[NiObject]] = None              # All rendering properties attached to this object.
    unknown1: list[int] = None                          # Always 2,0,2,0.
    unknown2: int = 0                                   # 0 or 1.
    boundingVolume: BoundingVolume = None
    collisionObject: Ref[NiObject] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.uv2 > 26: self.flags = Flags(r.readUInt16())
        if r.v >= 0x03000000 and (r.uv2 <= 26): self.flags = Flags(r.readUInt16())
        self.translation = r.readVector3()
        self.rotation = r.readMatrix3x3As4x4()
        self.scale = r.readSingle()
        if r.v <= 0x04020200: self.velocity = r.readVector3()
        if r.uv2 <= 34: self.properties = r.readL32FArray(X[NiProperty].ref)
        if r.v <= 0x02030000:
            self.unknown1 = r.readPArray(None, 'I', 4)
            self.unknown2 = r.readByte()
        if r.v >= 0x03000000 and r.v <= 0x04020200 and Z.readBool(r): self.boundingVolume = BoundingVolume(r)
        if r.v >= 0x0A000100: self.collisionObject = X[NiCollisionObject].ref(r)

# Abstract base class for dynamic effects such as NiLights or projected texture effects.
class NiDynamicEffect(NiAVObject): # X
    switchState: bool = True                            # If true, then the dynamic effect is applied to affected nodes during rendering.
    affectedNodes: list[Ref[NiObject]] = None           # If a node appears in this list, then its entire subtree will be affected by the effect.
    affectedNodePointers: list[int] = None              # As of 4.0 the pointer hash is no longer stored alongside each NiObject on disk, yet this node list still refers to the pointer hashes. Cannot leave the type as Ptr because the link will be invalid.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A01006A and r.uv2 < 130: self.switchState = Z.readBool(r)
        if r.v <= 0x0303000D: self.affectedNodes = r.readL32FArray(X[NiNode].ptr)
        elif r.v >= 0x04000000 and r.v <= 0x04000002: self.affectedNodePointers = r.readL32PArray(None, 'I')
        elif r.v >= 0x0A010000 and r.uv2 < 130: self.affectedNodes = r.readL32FArray(X[NiNode].ptr)

# Abstract base class representing all rendering properties. Subclasses are attached to NiAVObjects to control their rendering.
class NiProperty(NiObjectNET): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# Abstract base class that provides the base timing and update functionality for all the Gamebryo animation controllers.
class NiTimeController(NiObject): # X
    nextController: Ref[NiObject] = None                # Index of the next controller.
    flags: Flags = None                                 # Controller flags.
                                                        #     Bit 0 : Anim type, 0=APP_TIME 1=APP_INIT
                                                        #     Bit 1-2 : Cycle type, 00=Loop 01=Reverse 10=Clamp
                                                        #     Bit 3 : Active
                                                        #     Bit 4 : Play backwards
                                                        #     Bit 5 : Is manager controlled
                                                        #     Bit 6 : Always seems to be set in Skyrim and Fallout NIFs, unknown function
    frequency: float = 1.0                              # Frequency (is usually 1.0).
    phase: float = None                                 # Phase (usually 0.0).
    startTime: float = 3.402823466e+38                  # Controller start time.
    stopTime: float = -3.402823466e+38                  # Controller stop time.
    target: Ref[NiObject] = None                        # Controller target (object index of the first controllable ancestor of this object).
    unknownInteger: int = 0                             # Unknown integer.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.nextController = X[NiTimeController].ref(r)
        self.flags = Flags(r.readUInt16())
        self.frequency = r.readSingle()
        self.phase = r.readSingle()
        self.startTime = r.readSingle()
        self.stopTime = r.readSingle()
        if r.v >= 0x0303000D: self.target = X[NiObjectNET].ptr(r)
        elif r.v <= 0x03010000: self.unknownInteger = r.readUInt32()

# Abstract base class for all NiTimeController objects using NiInterpolator objects to animate their target objects.
class NiInterpController(NiTimeController): # X
    managerControlled: bool = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A010068 and r.v <= 0x0A01006C: self.managerControlled = Z.readBool(r)

# DEPRECATED (20.6)
class NiMultiTargetTransformController(NiInterpController): # Z
    extraTargets: list[Ref[NiObject]] = None            # NiNode Targets to be controlled.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.extraTargets = r.readL16FArray(X[NiAVObject].ptr)

# DEPRECATED (20.5), replaced by NiMorphMeshModifier.
# Time controller for geometry morphing.
class NiGeomMorpherController(NiInterpController): # X
    extraFlags: Flags = None                            # 1 = UPDATE NORMALS
    data: Ref[NiObject] = None                          # Geometry morphing data index.
    alwaysUpdate: int = 0
    interpolators: list[Ref[NiObject]] = None
    interpolatorWeights: list[MorphWeight] = None
    unknownInts: list[int] = None                       # Unknown.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A000102: self.extraFlags = Flags(r.readUInt16())
        self.data = X[NiMorphData].ref(r)
        if r.v >= 0x04000001: self.alwaysUpdate = r.readByte()
        if r.v >= 0x0A01006A and r.v <= 0x14000005: self.interpolators = r.readL32FArray(X[NiInterpolator].ref)
        elif r.v >= 0x14010003: self.interpolatorWeights = r.readL32FArray(lambda z: MorphWeight(r))
        if r.v >= 0x0A020000 and r.v <= 0x14000005 and (r.uv2 > 9): self.unknownInts = r.readL32PArray(None, 'I')

# Uses a single NiInterpolator to animate its target value.
class NiSingleInterpController(NiInterpController): # X
    interpolator: Ref[NiObject] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A010068: self.interpolator = X[NiInterpolator].ref(r)

# DEPRECATED (10.2), RENAMED (10.2) to NiTransformController
# A time controller object for animation key frames.
class NiKeyframeController(NiSingleInterpController): # X
    data: Ref[NiObject] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x0A010067: self.data = X[NiKeyframeData].ref(r)

# Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target float value.
class NiFloatInterpController(NiSingleInterpController): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# Animates the alpha value of a property using an interpolator.
class NiAlphaController(NiFloatInterpController): # X
    data: Ref[NiObject] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x0A010067: self.data = X[NiFloatData].ref(r)

# Abstract base class for all NiInterpControllers that use a NiInterpolator to animate their target boolean value.
class NiBoolInterpController(NiSingleInterpController): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# Animates the visibility of an NiAVObject.
class NiVisController(NiBoolInterpController): # X
    data: Ref[NiObject] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x0A010067: self.data = X[NiVisData].ref(r)

# Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target NiPoint3 value.
class NiPoint3InterpController(NiSingleInterpController): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# Time controller for material color. Flags are used for color selection in versions below 10.1.0.0.
# Bits 4-5: Target Color (00 = Ambient, 01 = Diffuse, 10 = Specular, 11 = Emissive)
# NiInterpController::GetCtlrID() string formats:
#     ['AMB', 'DIFF', 'SPEC', 'SELF_ILLUM'] (Depending on "Target Color")
class NiMaterialColorController(NiPoint3InterpController): # X
    targetColor: MaterialColor = 0                      # Selects which color to control.
    data: Ref[NiObject] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A010000: self.targetColor = MaterialColor(r.readUInt16())
        if r.v <= 0x0A010067: self.data = X[NiPosData].ref(r)

class MaterialData: # Y
    shaderName: str = None                              # The shader name.
    shaderExtraData: int = 0                            # Extra data associated with the shader. A value of -1 means the shader is the default implementation.
    numMaterials: int = 0
    materialName: list[str] = None                      # The name of the material.
    materialExtraData: list[int] = None                 # Extra data associated with the material. A value of -1 means the material is the default implementation.
    activeMaterial: int = -1                            # The index of the currently active material.
    unknownByte: int = 255                              # Cyanide extension (only in version 10.2.0.0?).
    unknownInteger2: int = 0                            # Unknown.
    materialNeedsUpdate: bool = None                    # Whether the materials for this object always needs to be updated before rendering with them.

    def __init__(self, r: NiReader):
        if r.v >= 0x0A000100 and r.v <= 0x14010003 and Z.readBool(r):
            self.shaderName = Z.string(r)
            self.shaderExtraData = r.readInt32()
        if r.v >= 0x14020005:
            self.numMaterials = r.readUInt32()
            self.materialName = r.readFArray(lambda z: Z.string(r), self.numMaterials)
            self.materialExtraData = r.readPArray(None, 'i', self.numMaterials)
            self.activeMaterial = r.readInt32()
        if r.v == 0x0A020000 and (r.uv == 1): self.unknownByte = r.readByte()
        if r.v == 0x0A040001: self.unknownInteger2 = r.readInt32()
        if r.v >= 0x14020007: self.materialNeedsUpdate = Z.readBool(r)

# Describes a visible scene element with vertices like a mesh, a particle system, lines, etc.
class NiGeometry(NiAVObject): # X
    bound: NiBound = None
    skin: Ref[NiObject] = None
    data: Ref[NiObject] = None                          # Data index (NiTriShapeData/NiTriStripData).
    skinInstance: Ref[NiObject] = None
    materialData: MaterialData = None
    shaderProperty: Ref[NiObject] = None
    alphaProperty: Ref[NiObject] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        NiParticleSystem: bool = False
        if (r.uv2 >= 100) and NiParticleSystem:
            self.bound = r.readS(NiBound)
            self.skin = X[NiObject].ref(r)
        if r.uv2 < 100: self.data = X[NiGeometryData].ref(r)
        if (r.uv2 >= 100) and not NiParticleSystem: self.data = X[NiGeometryData].ref(r)
        if r.v >= 0x0303000D and (r.uv2 < 100): self.skinInstance = X[NiSkinInstance].ref(r)
        if (r.uv2 >= 100) and not NiParticleSystem: self.skinInstance = X[NiSkinInstance].ref(r)
        if r.v >= 0x0A000100 and (r.uv2 < 100): self.materialData = MaterialData(r)
        if r.v >= 0x0A000100 and (r.uv2 >= 100) and not NiParticleSystem: self.materialData = MaterialData(r)
        if r.v >= 0x14020007 and (r.uv == 12):
            self.shaderProperty = X[BSShaderProperty].ref(r)
            self.alphaProperty = X[NiAlphaProperty].ref(r)

# Describes a mesh, built from triangles.
class NiTriBasedGeom(NiGeometry): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

class VectorFlags(Flag): # Y
    UV_1 = 0
    UV_2 = 1 << 1
    UV_4 = 1 << 2
    UV_8 = 1 << 3
    UV_16 = 1 << 4
    UV_32 = 1 << 5
    Unk64 = 1 << 6
    Unk128 = 1 << 7
    Unk256 = 1 << 8
    Unk512 = 1 << 9
    Unk1024 = 1 << 10
    Unk2048 = 1 << 11
    Has_Tangents = 1 << 12
    Unk8192 = 1 << 13
    Unk16384 = 1 << 14
    Unk32768 = 1 << 15

class BSVectorFlags(Flag): # Y
    Has_UV = 0
    Unk2 = 1 << 1
    Unk4 = 1 << 2
    Unk8 = 1 << 3
    Unk16 = 1 << 4
    Unk32 = 1 << 5
    Unk64 = 1 << 6
    Unk128 = 1 << 7
    Unk256 = 1 << 8
    Unk512 = 1 << 9
    Unk1024 = 1 << 10
    Unk2048 = 1 << 11
    Has_Tangents = 1 << 12
    Unk8192 = 1 << 13
    Unk16384 = 1 << 14
    Unk32768 = 1 << 15

# Mesh data: vertices, vertex normals, etc.
class NiGeometryData(NiObject): # X
    groupId: int = 0                                    # Always zero.
    numVertices: int = None                             # Number of vertices.
    bsMaxVertices: int = None                           # Bethesda uses this for max number of particles in NiPSysData.
    keepFlags: int = 0                                  # Used with NiCollision objects when OBB or TRI is set.
    compressFlags: int = 0                              # Unknown.
    vertices: list[Vector3] = None                      # The mesh vertices.
    vectorFlags: VectorFlags = 0
    bsVectorFlags: BSVectorFlags = 0
    materialCrc: int = 0
    normals: list[Vector3] = None                       # The lighting normals.
    tangents: list[Vector3] = None                      # Tangent vectors.
    bitangents: list[Vector3] = None                    # Bitangent vectors.
    unkFloats: list[float] = None
    center: Vector3 = None                              # Center of the bounding box (smallest box that contains all vertices) of the mesh.
    radius: float = None                                # Radius of the mesh: maximal Euclidean distance between the center and all vertices.
    unknown13shorts: list[int] = None                   # Unknown, always 0?
    vertexColors: list[Color4] = None                   # The vertex colors.
    numUvSets: int = None                               # The lower 6 (or less?) bits of this field represent the number of UV texture sets. The other bits are probably flag bits. For versions 10.1.0.0 and up, if bit 12 is set then extra vectors are present after the normals.
    uvSets: list[list[TexCoord]] = None                 # The UV texture coordinates. They follow the OpenGL standard: some programs may require you to flip the second coordinate.
    consistencyFlags: ConsistencyType = ConsistencyType.CT_MUTABLE # Consistency Flags
    additionalData: Ref[NiObject] = None                # Unknown.

    def __init__(self, r: NiReader):
        super().__init__(r)
        NiPSysData: bool = False
        if r.v >= 0x0A010072: self.groupId = r.readInt32()
        if not NiPSysData or r.UV2 >= 34: self.numVertices = r.readUInt16()
        if (r.uv2 >= 34) and NiPSysData: self.bsMaxVertices = r.readUInt16()
        if r.v >= 0x0A010000:
            self.keepFlags = r.readByte()
            self.compressFlags = r.readByte()
        hasVertices = Z.readBool8(r)
        if (hasVertices > 0) and (hasVertices != 15): self.vertices = r.readPArray(array, '3f', self.numVertices)
        if r.v >= 0x14030101 and hasVertices == 15: self.vertices = r.readFArray(lambda z: Vector3(r.readHalf(), r.readHalf(), r.readHalf()), self.numVertices)
        if r.v >= 0x0A000100 and not ((r.v == 0x14020007) and (r.uv2 > 0)): self.vectorFlags = VectorFlags(r.readUInt16())
        if ((r.v == 0x14020007) and (r.uv2 > 0)): self.bsVectorFlags = BSVectorFlags(r.readUInt16())
        if r.v == 0x14020007 and (r.uv == 12): self.materialCrc = r.readUInt32()
        hasNormals = Z.readBool8(r)
        if (hasNormals > 0) and (hasNormals != 6): self.normals = r.readPArray(array, '3f', self.numVertices)
        if r.v >= 0x14030101 and hasNormals == 6: self.normals = r.readFArray(lambda z: Vector3(r.readByte(), r.readByte(), r.readByte()), self.numVertices)
        if r.v >= 0x0A010000 and (hasNormals != 0) and ((self.vectorFlags | self.bsVectorFlags) & 4096) != 0:
            self.tangents = r.readPArray(array, '3f', self.numVertices)
            self.bitangents = r.readPArray(array, '3f', self.numVertices)
        if r.v == 0x14030009 and (r.uv == 0x20000) or (r.uv == 0x30000) and Z.readBool(r): self.unkFloats = r.readPArray(None, 'f', self.numVertices)
        self.center = r.readVector3()
        self.radius = r.readSingle()
        if r.v == 0x14030009 and (r.uv == 0x20000) or (r.uv == 0x30000): self.unknown13shorts = r.readPArray(None, 'h', 13)
        hasVertexColors = Z.readBool8(r)
        if (hasVertexColors > 0) and (hasVertexColors != 7): self.vertexColors = r.readFArray(lambda z: Color4(r), self.numVertices)
        if r.v >= 0x14030101 and hasVertexColors == 7: self.vertexColors = r.readFArray(lambda z: Color4(r.readBytes(4)), self.numVertices)
        if r.v <= 0x04020200: self.numUvSets = r.readUInt16()
        hasUv = Z.readBool(r) if r.v <= 0x04000002 else None
        if (hasVertices > 0) and (hasVertices != 15): self.uvSets = r.readFArray(lambda k: r.readSArray(TexCoord, self.numVertices), ((self.numUvSets & 63) | (self.vectorFlags & 63) | (self.bsVectorFlags & 1)))
        if r.v >= 0x14030101 and hasVertices == 15: self.uvSets = r.readFArray(lambda k: r.readFArray(lambda z: TexCoord(r, true), self.numVertices), ((self.numUvSets & 63) | (self.vectorFlags & 63) | (self.bsVectorFlags & 1)))
        if r.v >= 0x0A000100: self.consistencyFlags = ConsistencyType(r.readUInt16())
        if r.v >= 0x14000004: self.additionalData = X[AbstractAdditionalGeometryData].ref(r)

class AbstractAdditionalGeometryData(NiObject): # Y
    def __init__(self, r: NiReader):
        super().__init__(r)

# Describes a mesh, built from triangles.
class NiTriBasedGeomData(NiGeometryData): # X
    numTriangles: int = None                            # Number of triangles.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.numTriangles = r.readUInt16()

# Unknown. Marks furniture sitting positions?
class BSFurnitureMarker(NiExtraData): # Z
    positions: list[FurniturePosition] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.positions = r.readL32FArray(lambda z: FurniturePosition(r))

# Transparency. Flags 0x00ED.
class NiAlphaProperty(NiProperty): # X
    flags: Flags = 4844                                 # Bit 0 : alpha blending enable
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
    threshold: int = 128                                # Threshold for alpha testing (see: glAlphaFunc)
    unknownShort1: int = None                           # Unknown
    unknownInt2: int = 0                                # Unknown

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.flags = Flags(r.readUInt16())
        self.threshold = r.readByte()
        if r.v <= 0x02030000: self.unknownShort1 = r.readUInt16()
        if r.v >= 0x14030101 and r.v <= 0x14030102: self.unknownShort1 = r.readUInt16()
        if r.v <= 0x02030000: self.unknownInt2 = r.readUInt32()

# Generic rotating particles data object.
class NiParticlesData(NiGeometryData): # X
    numParticles: int = None                            # The maximum number of particles (matches the number of vertices).
    particleRadius: float = None                        # The particles' size.
    radii: list[float] = None                           # The individual particle sizes.
    numActive: int = None                               # The number of active particles at the time the system was saved. This is also the number of valid entries in the following arrays.
    sizes: list[float] = None                           # The individual particle sizes.
    rotations: list[Quaternion] = None                  # The individual particle rotations.
    rotationAngles: list[float] = None                  # Angles of rotation
    rotationAxes: list[Vector3] = None                  # Axes of rotation.
    numSubtextureOffsets: int = 0                       # How many quads to use in BSPSysSubTexModifier for texture atlasing
    subtextureOffsets: list[Vector4] = None             # Defines UV offsets
    aspectRatio: float = None                           # Sets aspect ratio for Subtexture Offset UV quads
    aspectFlags: int = None
    speedtoAspectAspect2: float = None
    speedtoAspectSpeed1: float = None
    speedtoAspectSpeed2: float = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x04000002: self.numParticles = r.readUInt16()
        if r.v <= 0x0A000100: self.particleRadius = r.readSingle()
        if r.v >= 0x0A010000 and not ((r.v == 0x14020007) and (r.uv2 > 0)) and Z.readBool(r): self.radii = r.readPArray(None, 'f', self.numVertices)
        self.numActive = r.readUInt16()
        if not ((r.v == 0x14020007) and (r.uv2 > 0)) and Z.readBool(r): self.sizes = r.readPArray(None, 'f', self.numVertices)
        if r.v >= 0x0A000100 and not ((r.v == 0x14020007) and (r.uv2 > 0)) and Z.readBool(r): self.rotations = r.readFArray(lambda z: r.readQuaternionWFirst(), self.numVertices)
        hasRotationAngles = Z.readBool(r) if r.v >= 0x14000004 else None
        if not ((r.v == 0x14020007) and (r.uv2 > 0)) and hasRotationAngles: self.rotationAngles = r.readPArray(None, 'f', self.numVertices)
        if r.v >= 0x14000004 and not ((r.v == 0x14020007) and (r.uv2 > 0)) and Z.readBool(r): self.rotationAxes = r.readPArray(array, '3f', self.numVertices)
        hasTextureIndices = Z.readBool(r) if ((r.v == 0x14020007) and (r.uv2 > 0)) else None
        if r.uv2 > 34: self.numSubtextureOffsets = r.readUInt32()
        if ((r.v == 0x14020007) and (r.uv2 > 0)): self.subtextureOffsets = r.readL8PArray(array, '4f')
        if r.uv2 > 34:
            self.aspectRatio = r.readSingle()
            self.aspectFlags = r.readUInt16()
            self.speedtoAspectAspect2 = r.readSingle()
            self.speedtoAspectSpeed1 = r.readSingle()
            self.speedtoAspectSpeed2 = r.readSingle()

# Rotating particles data object.
class NiRotatingParticlesData(NiParticlesData): # X
    rotations2: list[Quaternion] = None                 # The individual particle rotations.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x04020200 and Z.readBool(r): self.rotations2 = r.readFArray(lambda z: r.readQuaternionWFirst(), self.numVertices)

# Particle system data object (with automatic normals?).
class NiAutoNormalParticlesData(NiParticlesData): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# Binary extra data object. Used to store tangents and bitangents in Oblivion.
class NiBinaryExtraData(NiExtraData): # Z
    binaryData: bytearray = None                        # The binary data.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.binaryData = r.readL32Bytes()

# Camera object.
class NiCamera(NiAVObject): # X
    cameraFlags: int = None                             # Obsolete flags.
    frustumLeft: float = None                           # Frustrum left.
    frustumRight: float = None                          # Frustrum right.
    frustumTop: float = None                            # Frustrum top.
    frustumBottom: float = None                         # Frustrum bottom.
    frustumNear: float = None                           # Frustrum near.
    frustumFar: float = None                            # Frustrum far.
    useOrthographicProjection: bool = None              # Determines whether perspective is used.  Orthographic means no perspective.
    viewportLeft: float = None                          # Viewport left.
    viewportRight: float = None                         # Viewport right.
    viewportTop: float = None                           # Viewport top.
    viewportBottom: float = None                        # Viewport bottom.
    lodAdjust: float = None                             # Level of detail adjust.
    scene: Ref[NiObject] = None
    numScreenPolygons: int = 0                          # Deprecated. Array is always zero length on disk write.
    numScreenTextures: int = 0                          # Deprecated. Array is always zero length on disk write.
    unknownInt3: int = 0                                # Unknown.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A010000: self.cameraFlags = r.readUInt16()
        self.frustumLeft = r.readSingle()
        self.frustumRight = r.readSingle()
        self.frustumTop = r.readSingle()
        self.frustumBottom = r.readSingle()
        self.frustumNear = r.readSingle()
        self.frustumFar = r.readSingle()
        if r.v >= 0x0A010000: self.useOrthographicProjection = Z.readBool(r)
        self.viewportLeft = r.readSingle()
        self.viewportRight = r.readSingle()
        self.viewportTop = r.readSingle()
        self.viewportBottom = r.readSingle()
        self.lodAdjust = r.readSingle()
        self.scene = X[NiAVObject].ref(r)
        self.numScreenPolygons = r.readUInt32()
        if r.v >= 0x04020100: self.numScreenTextures = r.readUInt32()
        if r.v <= 0x03010000: self.unknownInt3 = r.readUInt32()

# Wrapper for color animation keys.
class NiColorData(NiObject): # X
    data: KeyGroup[Color4] = None                       # The color keys.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.data = KeyGroup[Color4]('[Color4]', r)

# Controls animation sequences on a specific branch of the scene graph.
class NiControllerManager(NiTimeController): # Z
    cumulative: bool = None                             # Whether transformation accumulation is enabled. If accumulation is not enabled, the manager will treat all sequence data on the accumulation root as absolute data instead of relative delta values.
    controllerSequences: list[Ref[NiObject]] = None
    objectPalette: Ref[NiObject] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.cumulative = Z.readBool(r)
        self.controllerSequences = r.readL32FArray(X[NiControllerSequence].ref)
        self.objectPalette = X[NiDefaultAVObjectPalette].ref(r)

# Root node in NetImmerse .kf files (until version 10.0).
class NiSequence(NiObject): # Z
    name: str = None                                    # The sequence name by which the animation system finds and manages this sequence.
    accumRootName: str = None                           # The name of the NiAVObject serving as the accumulation root. This is where all accumulated translations, scales, and rotations are applied.
    textKeys: Ref[NiObject] = None
    unknownInt4: int = 0                                # Divinity 2
    unknownInt5: int = 0                                # Divinity 2
    numControlledBlocks: int = 0
    arrayGrowBy: int = 0
    controlledBlocks: list[ControlledBlock] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.name = Z.string(r)
        if r.v <= 0x0A010067:
            self.accumRootName = Z.string(r)
            self.textKeys = X[NiTextKeyExtraData].ref(r)
        if r.v == 0x14030009 and (r.uv == 0x20000) or (r.uv == 0x30000):
            self.unknownInt4 = r.readInt32()
            self.unknownInt5 = r.readInt32()
        self.numControlledBlocks = r.readUInt32()
        if r.v >= 0x0A01006A: self.arrayGrowBy = r.readUInt32()
        self.controlledBlocks = r.readFArray(lambda z: ControlledBlock(r), self.numControlledBlocks)

# Root node in Gamebryo .kf files (version 10.0.1.0 and up).
class NiControllerSequence(NiSequence): # Z
    weight: float = 1.0                                 # The weight of a sequence describes how it blends with other sequences at the same priority.
    textKeys: Ref[NiObject] = None
    cycleType: CycleType = 0
    frequency: float = 1.0
    phase: float = None
    startTime: float = 3.402823466e+38
    stopTime: float = -3.402823466e+38
    playBackwards: bool = None
    manager: Ref[NiObject] = None                       # The owner of this sequence.
    accumRootName: str = None                           # The name of the NiAVObject serving as the accumulation root. This is where all accumulated translations, scales, and rotations are applied.
    accumFlags: AccumFlags = AccumFlags.ACCUM_X_FRONT
    stringPalette: Ref[NiObject] = None
    animNotes: Ref[NiObject] = None
    animNoteArrays: list[Ref[NiObject]] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A01006A:
            self.weight = r.readSingle()
            self.textKeys = X[NiTextKeyExtraData].ref(r)
            self.cycleType = CycleType(r.readUInt32())
            self.frequency = r.readSingle()
        if r.v >= 0x0A01006A and r.v <= 0x0A040001: self.phase = r.readSingle()
        if r.v >= 0x0A01006A:
            self.startTime = r.readSingle()
            self.stopTime = r.readSingle()
        if r.v == 0x0A01006A: self.playBackwards = Z.readBool(r)
        if r.v >= 0x0A01006A:
            self.manager = X[NiControllerManager].ptr(r)
            self.accumRootName = Z.string(r)
        if r.v >= 0x14030008: self.accumFlags = AccumFlags(r.readUInt32())
        if r.v >= 0x0A010071 and r.v <= 0x14010000: self.stringPalette = X[NiStringPalette].ref(r)
        if r.v >= 0x14020007 and (r.uv2 >= 24) and (r.uv2 <= 28): self.animNotes = X[BSAnimNotes].ref(r)
        if r.v >= 0x14020007 and (r.uv2 > 28): self.animNoteArrays = r.readL16FArray(X[BSAnimNotes].ref)

# Abstract base class for indexing NiAVObject by name.
class NiAVObjectPalette(NiObject): # Z
    def __init__(self, r: NiReader):
        super().__init__(r)

# NiAVObjectPalette implementation. Used to quickly look up objects by name.
class NiDefaultAVObjectPalette(NiAVObjectPalette): # Z
    scene: Ref[NiObject] = None                         # Scene root of the object palette.
    objs: list[AVObject] = None                         # The objects.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.scene = X[NiAVObject].ptr(r)
        self.objs = r.readL32FArray(lambda z: AVObject(r))

# Wrapper for 1D (one-dimensional) floating point animation keys.
class NiFloatData(NiObject): # X
    data: KeyGroup[float] = None                        # The keys.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.data = KeyGroup[float]('[float]', r)

# LEGACY (pre-10.1) particle modifier. Applies a gravitational field on the particles.
class NiGravity(NiParticleModifier): # X
    unknownFloat1: float = None                         # Unknown.
    force: float = None                                 # The strength/force of this gravity.
    type: FieldType = 0                                 # The force field type.
    position: Vector3 = None                            # The position of the mass point relative to the particle system.
    direction: Vector3 = None                           # The direction of the applied acceleration.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0303000D: self.unknownFloat1 = r.readSingle()
        self.force = r.readSingle()
        self.type = FieldType(r.readUInt32())
        self.position = r.readVector3()
        self.direction = r.readVector3()

# Extra integer data.
class NiIntegerExtraData(NiExtraData): # Z
    integerData: int = 0                                # The value of the extra data.

    def __init__(self, r: NiReader):
        super().__init__(r)
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
class BSXFlags(NiIntegerExtraData): # Z
    def __init__(self, r: NiReader):
        super().__init__(r)

# DEPRECATED (10.2), RENAMED (10.2) to NiTransformData.
# Wrapper for transformation animation keys.
class NiKeyframeData(NiObject): # X
    numRotationKeys: int = 0                            # The number of quaternion rotation keys. If the rotation type is XYZ (type 4) then this *must* be set to 1, and in this case the actual number of keys is stored in the XYZ Rotations field.
    rotationType: KeyType = 0                           # The type of interpolation to use for rotation.  Can also be 4 to indicate that separate X, Y, and Z values are used for the rotation instead of Quaternions.
    quaternionKeys: list[QuatKey[Quaternion]] = None    # The rotation keys if Quaternion rotation is used.
    order: float = None
    xyzRotations: list[KeyGroup[float]] = None          # Individual arrays of keys for rotating X, Y, and Z individually.
    translations: KeyGroup[Vector3] = None              # Translation keys.
    scales: KeyGroup[float] = None                      # Scale keys.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.numRotationKeys = r.readUInt32()
        if self.numRotationKeys != 0: self.rotationType = KeyType(r.readUInt32())
        if RotationType != KeyType.XYZ_ROTATION_KEY: self.quaternionKeys = r.readFArray(lambda z: QuatKey[Quaternion]('[Quaternion]', r, self.rotationType), self.numRotationKeys)
        else:
            if r.v <= 0x0A010000: self.order = r.readSingle()
            self.xyzRotations = r.readFArray(lambda z: KeyGroup[float]('[float]', r), 3)
        self.translations = KeyGroup[Vector3]('[Vector3]', r)
        self.scales = KeyGroup[float]('[float]', r)

# Describes the surface properties of an object e.g. translucency, ambient color, diffuse color, emissive color, and specular color.
class NiMaterialProperty(NiProperty): # X
    flags: Flags = None                                 # Property flags.
    ambientColor: Color3 = Color3(1.0, 1.0, 1.0)        # How much the material reflects ambient light.
    diffuseColor: Color3 = Color3(1.0, 1.0, 1.0)        # How much the material reflects diffuse light.
    specularColor: Color3 = Color3(1.0, 1.0, 1.0)       # How much light the material reflects in a specular manner.
    emissiveColor: Color3 = Color3(0.0, 0.0, 0.0)       # How much light the material emits.
    glossiness: float = 10.0                            # The material glossiness.
    alpha: float = 1.0                                  # The material transparency (1=non-transparant). Refer to a NiAlphaProperty object in this material's parent NiTriShape object, when alpha is not 1.
    emissiveMult: float = 1.0

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x03000000 and r.v <= 0x0A000102: self.flags = Flags(r.readUInt16())
        if r.uv2 < 26:
            self.ambientColor = Color3(r)
            self.diffuseColor = Color3(r)
        self.specularColor = Color3(r)
        self.emissiveColor = Color3(r)
        self.glossiness = r.readSingle()
        self.alpha = r.readSingle()
        if r.uv2 > 21: self.emissiveMult = r.readSingle()

# DEPRECATED (20.5), replaced by NiMorphMeshModifier.
# Geometry morphing data.
class NiMorphData(NiObject): # X
    numMorphs: int = 0                                  # Number of morphing object.
    numVertices: int = 0                                # Number of vertices.
    relativeTargets: int = 1                            # This byte is always 1 in all official files.
    morphs: list[Morph] = None                          # The geometry morphing objects.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.numMorphs = r.readUInt32()
        self.numVertices = r.readUInt32()
        self.relativeTargets = r.readByte()
        self.morphs = r.readFArray(lambda z: Morph(r, self.numVertices), self.numMorphs)

# Generic node object for grouping.
class NiNode(NiAVObject): # X
    children: list[Ref[NiObject]] = None                # List of child node object indices.
    effects: list[Ref[NiObject]] = None                 # List of node effects. ADynamicEffect?

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.children = r.readL32FArray(X[NiAVObject].ref)
        if r.uv2 < 130: self.effects = r.readL32FArray(X[NiDynamicEffect].ref)

# Morrowind specific.
class AvoidNode(NiNode): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

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
class NiBillboardNode(NiNode): # X
    billboardMode: BillboardMode = 0                    # The way the billboard will react to the camera.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A010000: self.billboardMode = BillboardMode(r.readUInt16())

# Bethesda-specific extension of Node with animation properties stored in the flags, often 42?
class NiBSAnimationNode(NiNode): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# Unknown.
class NiBSParticleNode(NiNode): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# NiPalette objects represent mappings from 8-bit indices to 24-bit RGB or 32-bit RGBA colors.
class NiPalette(NiObject): # X
    hasAlpha: int = 0
    numEntries: int = 256                               # The number of palette entries. Always 256 but can also be 16.
    palette: list[Color4] = None                        # The color palette.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.hasAlpha = r.readByte()
        self.numEntries = r.readUInt32()
        if self.numEntries == 16: self.palette = r.readFArray(lambda z: Color4(r.readBytes(4)), 16)
        else: self.palette = r.readFArray(lambda z: Color4(r.readBytes(4)), 256)

# LEGACY (pre-10.1) particle modifier.
class NiParticleBomb(NiParticleModifier): # X
    decay: float = None
    duration: float = None
    deltaV: float = None
    start: float = None
    decayType: DecayType = 0
    symmetryType: SymmetryType = 0
    position: Vector3 = None                            # The position of the mass point relative to the particle system?
    direction: Vector3 = None                           # The direction of the applied acceleration?

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.decay = r.readSingle()
        self.duration = r.readSingle()
        self.deltaV = r.readSingle()
        self.start = r.readSingle()
        self.decayType = DecayType(r.readUInt32())
        if r.v >= 0x0401000C: self.symmetryType = SymmetryType(r.readUInt32())
        self.position = r.readVector3()
        self.direction = r.readVector3()

# LEGACY (pre-10.1) particle modifier.
class NiParticleColorModifier(NiParticleModifier): # X
    colorData: Ref[NiObject] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.colorData = X[NiColorData].ref(r)

# LEGACY (pre-10.1) particle modifier.
class NiParticleGrowFade(NiParticleModifier): # X
    grow: float = None                                  # The time from the beginning of the particle lifetime during which the particle grows.
    fade: float = None                                  # The time from the end of the particle lifetime during which the particle fades.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.grow = r.readSingle()
        self.fade = r.readSingle()

# LEGACY (pre-10.1) particle modifier.
class NiParticleMeshModifier(NiParticleModifier): # X
    particleMeshes: list[Ref[NiObject]] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.particleMeshes = r.readL32FArray(X[NiAVObject].ref)

# LEGACY (pre-10.1) particle modifier.
class NiParticleRotation(NiParticleModifier): # X
    randomInitialAxis: int = 0
    initialAxis: Vector3 = None
    rotationSpeed: float = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.randomInitialAxis = r.readByte()
        self.initialAxis = r.readVector3()
        self.rotationSpeed = r.readSingle()

# Generic particle system node.
class NiParticles(NiGeometry): # X
    vertexDesc: BSVertexDesc = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.uv2 >= 100: self.vertexDesc = r.readS(BSVertexDesc)

# LEGACY (pre-10.1). NiParticles which do not house normals and generate them at runtime.
class NiAutoNormalParticles(NiParticles): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# A generic particle system time controller object.
class NiParticleSystemController(NiTimeController): # X
    oldSpeed: int = 0                                   # Particle speed in old files
    speed: float = None                                 # Particle speed
    speedRandom: float = None                           # Particle random speed modifier
    verticalDirection: float = None                     # vertical emit direction [radians]
                                                        #     0.0 : up
                                                        #     1.6 : horizontal
                                                        #     3.1416 : down
    verticalAngle: float = None                         # emitter's vertical opening angle [radians]
    horizontalDirection: float = None                   # horizontal emit direction
    horizontalAngle: float = None                       # emitter's horizontal opening angle
    unknownNormal: Vector3 = None                       # Unknown.
    unknownColor: Color4 = None                         # Unknown.
    size: float = None                                  # Particle size
    emitStartTime: float = None                         # Particle emit start time
    emitStopTime: float = None                          # Particle emit stop time
    unknownByte: int = 0                                # Unknown byte, (=0)
    oldEmitRate: int = 0                                # Particle emission rate in old files
    emitRate: float = None                              # Particle emission rate (particles per second)
    lifetime: float = None                              # Particle lifetime
    lifetimeRandom: float = None                        # Particle lifetime random modifier
    emitFlags: int = None                               # Bit 0: Emit Rate toggle bit (0 = auto adjust, 1 = use Emit Rate value)
    startRandom: Vector3 = None                         # Particle random start translation vector
    emitter: Ref[NiObject] = None                       # This index targets the particle emitter object (TODO: find out what type of object this refers to).
    unknownShort2: int = None                           # ? short=0 ?
    unknownFloat13: float = None                        # ? float=1.0 ?
    unknownInt1: int = 0                                # ? int=1 ?
    unknownInt2: int = 0                                # ? int=0 ?
    unknownShort3: int = None                           # ? short=0 ?
    particleVelocity: Vector3 = None                    # Particle velocity
    particleUnknownVector: Vector3 = None               # Unknown
    particleLifetime: float = None                      # The particle's age.
    particleLink: Ref[NiObject] = None
    particleTimestamp: int = 0                          # Timestamp of the last update.
    particleUnknownShort: int = None                    # Unknown short
    particleVertexId: int = None                        # Particle/vertex index matches array index
    numParticles: int = None                            # Size of the following array. (Maximum number of simultaneous active particles)
    numValid: int = None                                # Number of valid entries in the following array. (Number of active particles at the time the system was saved)
    particles: list[Particle] = None                    # Individual particle modifiers?
    unknownLink: Ref[NiObject] = None                   # unknown int (=0xffffffff)
    particleExtra: Ref[NiObject] = None                 # Link to some optional particle modifiers (NiGravity, NiParticleGrowFade, NiParticleBomb, ...)
    unknownLink2: Ref[NiObject] = None                  # Unknown int (=0xffffffff)
    trailer: int = 0                                    # Trailing null byte
    colorData: Ref[NiObject] = None
    unknownFloat1: float = None
    unknownFloats2: list[float] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x03010000: self.oldSpeed = r.readUInt32()
        if r.v >= 0x0303000D: self.speed = r.readSingle()
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
        if r.v >= 0x04000002: self.unknownByte = r.readByte()
        if r.v <= 0x03010000: self.oldEmitRate = r.readUInt32()
        if r.v >= 0x0303000D: self.emitRate = r.readSingle()
        self.lifetime = r.readSingle()
        self.lifetimeRandom = r.readSingle()
        if r.v >= 0x04000002: self.emitFlags = r.readUInt16()
        self.startRandom = r.readVector3()
        self.emitter = X[NiObject].ptr(r)
        if r.v >= 0x04000002:
            self.unknownShort2 = r.readUInt16()
            self.unknownFloat13 = r.readSingle()
            self.unknownInt1 = r.readUInt32()
            self.unknownInt2 = r.readUInt32()
            self.unknownShort3 = r.readUInt16()
        if r.v <= 0x03010000:
            self.particleVelocity = r.readVector3()
            self.particleUnknownVector = r.readVector3()
            self.particleLifetime = r.readSingle()
            self.particleLink = X[NiObject].ref(r)
            self.particleTimestamp = r.readUInt32()
            self.particleUnknownShort = r.readUInt16()
            self.particleVertexId = r.readUInt16()
        if r.v >= 0x04000002:
            self.numParticles = r.readUInt16()
            self.numValid = r.readUInt16()
            self.particles = r.readSArray(Particle, self.numParticles)
            self.unknownLink = X[NiObject].ref(r)
        self.particleExtra = X[NiParticleModifier].ref(r)
        self.unknownLink2 = X[NiObject].ref(r)
        if r.v >= 0x04000002: self.trailer = r.readByte()
        if r.v <= 0x03010000:
            self.colorData = X[NiColorData].ref(r)
            self.unknownFloat1 = r.readSingle()
            self.unknownFloats2 = r.readPArray(None, 'f', self.particleUnknownShort)

# A particle system controller, used by BS in conjunction with NiBSParticleNode.
class NiBSPArrayController(NiParticleSystemController): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# DEPRECATED (10.2), REMOVED (20.5). Replaced by NiTransformController and NiPathInterpolator.
# Time controller for a path.
class NiPathController(NiTimeController): # X
    pathFlags: PathFlags = 0
    bankDir: int = 1                                    # -1 = Negative, 1 = Positive
    maxBankAngle: float = None                          # Max angle in radians.
    smoothing: float = None
    followAxis: int = None                              # 0, 1, or 2 representing X, Y, or Z.
    pathData: Ref[NiObject] = None
    percentData: Ref[NiObject] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A010000: self.pathFlags = PathFlags(r.readUInt16())
        self.bankDir = r.readInt32()
        self.maxBankAngle = r.readSingle()
        self.smoothing = r.readSingle()
        self.followAxis = r.readInt16()
        self.pathData = X[NiPosData].ref(r)
        self.percentData = X[NiFloatData].ref(r)

class PixelFormatComponent: # Y
    def __init__(self, r: NiReader):
        self.type: PixelComponent = PixelComponent(r.readUInt32()) # Component Type
        self.convention: PixelRepresentation = PixelRepresentation(r.readUInt32()) # Data Storage Convention
        self.bitsPerChannel: int = r.readByte()         # Bits per component
        self.isSigned: bool = Z.readBool(r)

class NiPixelFormat(NiObject): # Y
    pixelFormat: PixelFormat = 0                        # The format of the pixels in this internally stored image.
    redMask: int = 0                                    # 0x000000ff (for 24bpp and 32bpp) or 0x00000000 (for 8bpp)
    greenMask: int = 0                                  # 0x0000ff00 (for 24bpp and 32bpp) or 0x00000000 (for 8bpp)
    blueMask: int = 0                                   # 0x00ff0000 (for 24bpp and 32bpp) or 0x00000000 (for 8bpp)
    alphaMask: int = 0                                  # 0xff000000 (for 32bpp) or 0x00000000 (for 24bpp and 8bpp)
    bitsPerPixel: int = 0                               # Bits per pixel, 0 (Compressed), 8, 24 or 32.
    oldFastCompare: bytearray = None                    # [96,8,130,0,0,65,0,0] if 24 bits per pixel
                                                        #     [129,8,130,32,0,65,12,0] if 32 bits per pixel
                                                        #     [34,0,0,0,0,0,0,0] if 8 bits per pixel
                                                        #     [X,0,0,0,0,0,0,0] if 0 (Compressed) bits per pixel where X = PixelFormat
    tiling: PixelTiling = 0                             # Seems to always be zero.
    rendererHint: int = 0
    extraData: int = 0
    flags: int = 0
    sRgbSpace: bool = None
    channels: list[PixelFormatComponent] = None         # Channel Data

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.pixelFormat = PixelFormat(r.readUInt32())
        if r.v <= 0x0A030002:
            self.redMask = r.readUInt32()
            self.greenMask = r.readUInt32()
            self.blueMask = r.readUInt32()
            self.alphaMask = r.readUInt32()
            self.bitsPerPixel = r.readUInt32()
            self.oldFastCompare = r.readBytes(8)
        if r.v >= 0x0A010000 and r.v <= 0x0A030002: self.tiling = PixelTiling(r.readUInt32())
        if r.v >= 0x0A030003:
            self.bitsPerPixel = r.readByte()
            self.rendererHint = r.readUInt32()
            self.extraData = r.readUInt32()
            self.flags = r.readByte()
            self.tiling = PixelTiling(r.readUInt32())
        if r.v >= 0x14030004: self.sRgbSpace = Z.readBool(r)
        if r.v >= 0x0A030003: self.channels = r.readFArray(lambda z: PixelFormatComponent(r), 4)

# A texture.
class NiPixelData(NiPixelFormat): # X
    palette: Ref[NiObject] = None
    numMipmaps: int = 0
    bytesPerPixel: int = 0
    mipmaps: list[MipMap] = None
    numPixels: int = 0
    numFaces: int = 1
    pixelData: bytearray = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.palette = X[NiPalette].ref(r)
        self.numMipmaps = r.readUInt32()
        self.bytesPerPixel = r.readUInt32()
        self.mipmaps = r.readSArray(MipMap, self.numMipmaps)
        self.numPixels = r.readUInt32()
        if r.v >= 0x0A030006: self.numFaces = r.readUInt32()
        if r.v <= 0x0A030005: self.pixelData = r.readBytes(self.numPixels)
        if r.v >= 0x0A030006: self.pixelData = r.readBytes(self.numPixels * self.numFaces)

# Wrapper for position animation keys.
class NiPosData(NiObject): # X
    data: KeyGroup[Vector3] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.data = KeyGroup[Vector3]('[Vector3]', r)

# Unknown.
class NiRotatingParticles(NiParticles): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# Determines whether flat shading or smooth shading is used on a shape.
class NiShadeProperty(NiProperty): # X
    flags: Flags = 1                                    # Bit 0: Enable smooth phong shading on this shape. Otherwise, hard-edged flat shading will be used on this shape.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.uv2 <= 34: self.flags = Flags(r.readUInt16())

# Skinning data.
class NiSkinData(NiObject): # X
    skinTransform: NiTransform = None                   # Offset of the skin from this bone in bind position.
    numBones: int = 0                                   # Number of bones.
    skinPartition: Ref[NiObject] = None                 # This optionally links a NiSkinPartition for hardware-acceleration information.
    hasVertexWeights: int = 1                           # Enables Vertex Weights for this NiSkinData.
    boneList: list[BoneData] = None                     # Contains offset data for each node that this skin is influenced by.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.skinTransform = NiTransform(r)
        self.numBones = r.readUInt32()
        if r.v >= 0x04000002 and r.v <= 0x0A010000: self.skinPartition = X[NiSkinPartition].ref(r)
        if r.v >= 0x04020100: self.hasVertexWeights = r.readByte()
        self.boneList = r.readFArray(lambda z: BoneData(r, self.hasVertexWeights), self.numBones)

# Skinning instance.
class NiSkinInstance(NiObject): # X
    data: Ref[NiObject] = None                          # Skinning data reference.
    skinPartition: Ref[NiObject] = None                 # Refers to a NiSkinPartition objects, which partitions the mesh such that every vertex is only influenced by a limited number of bones.
    skeletonRoot: Ref[NiObject] = None                  # Armature root node.
    bones: list[Ref[NiObject]] = None                   # List of all armature bones.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.data = X[NiSkinData].ref(r)
        if r.v >= 0x0A010065: self.skinPartition = X[NiSkinPartition].ref(r)
        self.skeletonRoot = X[NiNode].ptr(r)
        self.bones = r.readL32FArray(X[NiNode].ptr)

# Skinning data, optimized for hardware skinning. The mesh is partitioned in submeshes such that each vertex of a submesh is influenced only by a limited and fixed number of bones.
class NiSkinPartition(NiObject): # X
    numSkinPartitionBlocks: int = 0
    skinPartitionBlocks: list[SkinPartition] = None     # Skin partition objects.
    dataSize: int = 0
    vertexSize: int = 0
    vertexDesc: BSVertexDesc = None
    vertexData: list[BSVertexData] = None
    partition: list[SkinPartition] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.numSkinPartitionBlocks = r.readUInt32()
        if not ((r.v == 0x14020007) and (r.uv2 == 100)): self.skinPartitionBlocks = r.readFArray(lambda z: SkinPartition(r), self.numSkinPartitionBlocks)
        if r.uv2 == 100:
            self.dataSize = r.readUInt32()
            self.vertexSize = r.readUInt32()
            self.vertexDesc = r.readS(BSVertexDesc)
            if self.dataSize > 0: self.vertexData = r.readFArray(lambda z: BSVertexData(r, VertexDesc.VertexAttributes, true), self.dataSize / self.vertexSize)
            self.partition = r.readFArray(lambda z: SkinPartition(r), self.numSkinPartitionBlocks)

# A texture.
class NiTexture(NiObjectNET): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# NiTexture::FormatPrefs. These preferences are a request to the renderer to use a format the most closely matches the settings and may be ignored.
class FormatPrefs: # Y
    def __init__(self, r: NiReader):
        self.pixelLayout: PixelLayout = PixelLayout(r.readUInt32()) # Requests the way the image will be stored.
        self.useMipmaps: MipMapFormat = MipMapFormat(r.readUInt32()) # Requests if mipmaps are used or not.
        self.alphaFormat: AlphaFormat = AlphaFormat(r.readUInt32()) # Requests no alpha, 1-bit alpha, or

# Describes texture source and properties.
class NiSourceTexture(NiTexture): # X
    useExternal: int = 1                                # Is the texture external?
    fileName: str = None                                # The external texture file name.
    unknownLink: Ref[NiObject] = None                   # Unknown.
    unknownByte: int = 1                                # Unknown. Seems to be set if Pixel Data is present?
    pixelData: Ref[NiObject] = None                     # NiPixelData or NiPersistentSrcTextureRendererData
    formatPrefs: FormatPrefs = None                     # A set of preferences for the texture format. They are a request only and the renderer may ignore them.
    isStatic: int = 1                                   # If set, then the application cannot assume that any dynamic changes to the pixel data will show in the rendered image.
    directRender: bool = True                           # A hint to the renderer that the texture can be loaded directly from a texture file into a renderer-specific resource, bypassing the NiPixelData object.
    persistRenderData: bool = False                     # Pixel Data is NiPersistentSrcTextureRendererData instead of NiPixelData.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.useExternal = r.readByte()
        if self.useExternal == 1:
            self.fileName = r.readL32AString()
            if r.v >= 0x0A010000: self.unknownLink = X[NiObject].ref(r)
        if self.useExternal == 0:
            if r.v <= 0x0A000100: self.unknownByte = r.readByte()
            if r.v >= 0x0A010000: self.fileName = r.readL32AString()
            self.pixelData = X[NiPixelFormat].ref(r)
        self.formatPrefs = FormatPrefs(r)
        self.isStatic = r.readByte()
        if r.v >= 0x0A010067: self.directRender = Z.readBool(r)
        if r.v >= 0x14020004: self.persistRenderData = Z.readBool(r)

# Gives specularity to a shape. Flags 0x0001.
class NiSpecularProperty(NiProperty): # Z
    flags: Flags = None                                 # Bit 0 = Enable specular lighting on this shape.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.flags = Flags(r.readUInt16())

# Allows control of stencil testing.
class NiStencilProperty(NiProperty): # Z
    flags: Flags = None                                 # Property flags.
    stencilEnabled: int = 0                             # Enables or disables the stencil test.
    stencilFunction: StencilCompareMode = 0             # Selects the compare mode function (see: glStencilFunc).
    stencilRef: int = 0
    stencilMask: int = 4294967295                       # A bit mask. The default is 0xffffffff.
    failAction: StencilAction = 0
    zFailAction: StencilAction = 0
    passAction: StencilAction = 0
    drawMode: StencilDrawMode = StencilDrawMode.DRAW_BOTH # Used to enabled double sided faces. Default is 3 (DRAW_BOTH).

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x0A000102: self.flags = Flags(r.readUInt16())
        if r.v <= 0x14000005:
            self.stencilEnabled = r.readByte()
            self.stencilFunction = StencilCompareMode(r.readUInt32())
            self.stencilRef = r.readUInt32()
            self.stencilMask = r.readUInt32()
            self.failAction = StencilAction(r.readUInt32())
            self.zFailAction = StencilAction(r.readUInt32())
            self.passAction = StencilAction(r.readUInt32())
            self.drawMode = StencilDrawMode(r.readUInt32())
        if r.v >= 0x14010003:
            self.flags = Flags(r.readUInt16())
            self.stencilRef = r.readUInt32()
            self.stencilMask = r.readUInt32()

# Apparently commands for an optimizer instructing it to keep things it would normally discard.
# Also refers to NiNode objects (through their name) in animation .kf files.
class NiStringExtraData(NiExtraData): # X
    bytesRemaining: int = 0                             # The number of bytes left in the record.  Equals the length of the following string + 4.
    stringData: str = None                              # The string.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x04020200: self.bytesRemaining = r.readUInt32()
        self.stringData = Z.string(r)

# List of 0x00-seperated strings, which are names of controlled objects and controller types. Used in .kf files in conjunction with NiControllerSequence.
class NiStringPalette(NiObject): # Z
    palette: StringPalette = None                       # A bunch of 0x00 seperated strings.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.palette = StringPalette(r)

# Extra data, used to name different animation sequences.
class NiTextKeyExtraData(NiExtraData): # X
    unknownInt1: int = 0                                # Unknown.  Always equals zero in all official files.
    textKeys: list[Key[str]] = None                     # List of textual notes and at which time they take effect. Used for designating the start and stop of animations and the triggering of sounds.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x04020200: self.unknownInt1 = r.readUInt32()
        self.textKeys = r.readL32FArray(lambda z: Key[str]('[str]', r, KeyType.LINEAR_KEY))

# Represents an effect that uses projected textures such as projected lights (gobos), environment maps, and fog maps.
class NiTextureEffect(NiDynamicEffect): # X
    modelProjectionMatrix: Matrix4x4 = None             # Model projection matrix.  Always identity?
    modelProjectionTransform: Vector3 = None            # Model projection transform.  Always (0,0,0)?
    textureFiltering: TexFilterMode = TexFilterMode.FILTER_TRILERP # Texture Filtering mode.
    maxAnisotropy: int = None
    textureClamping: TexClampMode = TexClampMode.WRAP_S_WRAP_T # Texture Clamp mode.
    textureType: TextureType = TextureType.TEX_ENVIRONMENT_MAP # The type of effect that the texture is used for.
    coordinateGenerationType: CoordGenType = CoordGenType.CG_SPHERE_MAP # The method that will be used to generate UV coordinates for the texture effect.
    image: Ref[NiObject] = None                         # Image index.
    sourceTexture: Ref[NiObject] = None                 # Source texture index.
    enablePlane: int = 0                                # Determines whether a clipping plane is used.
    plane: NiPlane = None
    pS2L: int = 0
    pS2K: int = -75
    unknownShort: int = None                            # Unknown: 0.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.modelProjectionMatrix = r.readMatrix3x3As4x4()
        self.modelProjectionTransform = r.readVector3()
        self.textureFiltering = TexFilterMode(r.readUInt32())
        if r.v >= 0x14050004: self.maxAnisotropy = r.readUInt16()
        self.textureClamping = TexClampMode(r.readUInt32())
        self.textureType = TextureType(r.readUInt32())
        self.coordinateGenerationType = CoordGenType(r.readUInt32())
        if r.v <= 0x03010000: self.image = X[NiImage].ref(r)
        if r.v >= 0x04000000: self.sourceTexture = X[NiSourceTexture].ref(r)
        self.enablePlane = r.readByte()
        self.plane = r.readS(NiPlane)
        if r.v <= 0x0A020000:
            self.pS2L = r.readInt16()
            self.pS2K = r.readInt16()
        if r.v <= 0x0401000C: self.unknownShort = r.readUInt16()

# LEGACY (pre-10.1)
class NiImage(NiObject): # Y
    useExternal: int = 0                                # 0 if the texture is internal to the NIF file.
    fileName: str = None                                # The filepath to the texture.
    imageData: Ref[NiObject] = None                     # Link to the internally stored image data.
    unknownInt: int = 7                                 # Unknown.  Often seems to be 7. Perhaps m_uiMipLevels?
    unknownFloat: float = 128.5                         # Unknown.  Perhaps fImageScale?

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.useExternal = r.readByte()
        if self.useExternal != 0: self.fileName = r.readL32AString()
        else: self.imageData = X[NiRawImageData].ref(r)
        self.unknownInt = r.readUInt32()
        if r.v >= 0x03010000: self.unknownFloat = r.readSingle()

# Describes how a fragment shader should be configured for a given piece of geometry.
class NiTexturingProperty(NiProperty): # X
    flags: Flags = None                                 # Property flags.
    applyMode: ApplyMode = ApplyMode.APPLY_MODULATE     # Determines how the texture will be applied.  Seems to have special functions in Oblivion.
    textureCount: int = 7                               # Number of textures.
    baseTexture: TexDesc = None                         # The base texture.
    darkTexture: TexDesc = None                         # The dark texture.
    detailTexture: TexDesc = None                       # The detail texture.
    glossTexture: TexDesc = None                        # The gloss texture.
    glowTexture: TexDesc = None                         # The glowing texture.
    bumpMapTexture: TexDesc = None                      # The bump map texture.
    bumpMapLumaScale: float = None
    bumpMapLumaOffset: float = None
    bumpMapMatrix: Matrix2x2 = None
    normalTexture: TexDesc = None                       # Normal texture.
    parallaxTexture: TexDesc = None
    parallaxOffset: float = None
    decal0Texture: TexDesc = None                       # The decal texture.
    decal1Texture: TexDesc = None                       # Another decal texture.
    decal2Texture: TexDesc = None                       # Another decal texture.
    decal3Texture: TexDesc = None                       # Another decal texture.
    shaderTextures: list[ShaderTexDesc] = None          # Shader textures.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x0A000102: self.flags = Flags(r.readUInt16())
        if r.v >= 0x14010002: self.flags = Flags(r.readUInt16())
        if r.v >= 0x0303000D and r.v <= 0x14010001: self.applyMode = ApplyMode(r.readUInt32())
        self.textureCount = r.readUInt32()
        if Z.readBool(r): self.baseTexture = TexDesc(r)
        if Z.readBool(r): self.darkTexture = TexDesc(r)
        if Z.readBool(r): self.detailTexture = TexDesc(r)
        if Z.readBool(r): self.glossTexture = TexDesc(r)
        if Z.readBool(r): self.glowTexture = TexDesc(r)
        hasBumpMapTexture = Z.readBool(r) if r.v >= 0x0303000D and self.textureCount > 5 else None
        if hasBumpMapTexture:
            self.bumpMapTexture = TexDesc(r)
            self.bumpMapLumaScale = r.readSingle()
            self.bumpMapLumaOffset = r.readSingle()
            self.bumpMapMatrix = r.readMatrix2x2()
        hasNormalTexture = Z.readBool(r) if r.v >= 0x14020005 and self.textureCount > 6 else None
        if hasNormalTexture: self.normalTexture = TexDesc(r)
        hasParallaxTexture = Z.readBool(r) if r.v >= 0x14020005 and self.textureCount > 7 else None
        if hasParallaxTexture:
            self.parallaxTexture = TexDesc(r)
            self.parallaxOffset = r.readSingle()
        hasDecal0Texture = Z.readBool(r) if r.v <= 0x14020004 and self.textureCount > 6 else \
            Z.readBool(r) if r.v >= 0x14020005 and self.textureCount > 8 else None
        if hasDecal0Texture: self.decal0Texture = TexDesc(r)
        hasDecal1Texture = Z.readBool(r) if r.v <= 0x14020004 and self.textureCount > 7 else \
            Z.readBool(r) if r.v >= 0x14020005 and self.textureCount > 9 else None
        if hasDecal1Texture: self.decal1Texture = TexDesc(r)
        hasDecal2Texture = Z.readBool(r) if r.v <= 0x14020004 and self.textureCount > 8 else \
            Z.readBool(r) if r.v >= 0x14020005 and self.textureCount > 10 else None
        if hasDecal2Texture: self.decal2Texture = TexDesc(r)
        hasDecal3Texture = Z.readBool(r) if r.v <= 0x14020004 and self.textureCount > 9 else \
            Z.readBool(r) if r.v >= 0x14020005 and self.textureCount > 11 else None
        if hasDecal3Texture: self.decal3Texture = TexDesc(r)
        if r.v >= 0x0A000100: self.shaderTextures = r.readL32FArray(lambda z: ShaderTexDesc(r))

# Wrapper for transformation animation keys.
class NiTransformData(NiKeyframeData): # Z
    def __init__(self, r: NiReader):
        super().__init__(r)

# A shape node that refers to singular triangle data.
class NiTriShape(NiTriBasedGeom): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# Holds mesh data using a list of singular triangles.
class NiTriShapeData(NiTriBasedGeomData): # X
    numTrianglePoints: int = 0                          # Num Triangles times 3.
    triangles: list[Triangle] = None                    # Triangle data.
    matchGroups: list[MatchGroup] = None                # The shared normals.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.numTrianglePoints = r.readUInt32()
        hasTriangles = False if r.v >= 0x0A010000 else None # calculated
        if r.v <= 0x0A000102: self.triangles = r.readSArray(Triangle, self.numTriangles)
        if r.v >= 0x0A000103 and hasTriangles: self.triangles = r.readSArray(Triangle, self.numTriangles)
        if r.v >= 0x03010000: self.matchGroups = r.readL16FArray(lambda z: MatchGroup(r))

# A shape node that refers to data organized into strips of triangles
class NiTriStrips(NiTriBasedGeom): # Z
    def __init__(self, r: NiReader):
        super().__init__(r)

# Holds mesh data using strips of triangles.
class NiTriStripsData(NiTriBasedGeomData): # Z
    numStrips: int = None                               # Number of OpenGL triangle strips that are present.
    stripLengths: list[int] = None                      # The number of points in each triangle strip.
    points: list[list[int]] = None                      # The points in the Triangle strips.  Size is the sum of all entries in Strip Lengths.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.numStrips = r.readUInt16()
        self.stripLengths = r.readPArray(None, 'H', self.numStrips)
        hasPoints = Z.readBool(r) if r.v >= 0x0A000103 else None
        if r.v <= 0x0A000102: self.points = r.readFArray(lambda k, i: r.readPArray(None, 'H', self.stripLengths[i]), self.numStrips)
        if r.v >= 0x0A000103 and hasPoints: self.points = r.readFArray(lambda k, i: r.readPArray(None, 'H', self.stripLengths[i]), self.numStrips)

# DEPRECATED (pre-10.1), REMOVED (20.3).
# Time controller for texture coordinates.
class NiUVController(NiTimeController): # X
    unknownShort: int = None                            # Always 0?
    data: Ref[NiObject] = None                          # Texture coordinate controller data index.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.unknownShort = r.readUInt16()
        self.data = X[NiUVData].ref(r)

# DEPRECATED (pre-10.1), REMOVED (20.3)
# Texture coordinate data.
class NiUVData(NiObject): # X
    uvGroups: list[KeyGroup[float]] = None              # Four UV data groups. Appear to be U translation, V translation, U scaling/tiling, V scaling/tiling.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.uvGroups = r.readFArray(lambda z: KeyGroup[float]('[float]', r), 4)

# Property of vertex colors. This object is referred to by the root object of the NIF file whenever some NiTriShapeData object has vertex colors with non-default settings; if not present, vertex colors have vertex_mode=2 and lighting_mode=1.
class NiVertexColorProperty(NiProperty): # X
    flags: Flags = None                                 # Bits 0-2: Unknown
                                                        #     Bit 3: Lighting Mode
                                                        #     Bits 4-5: Vertex Mode
    vertexMode: VertMode = 0                            # In Flags from 20.1.0.3 on.
    lightingMode: LightMode = 0                         # In Flags from 20.1.0.3 on.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.flags = Flags(r.readUInt16())
        if r.v <= 0x14000005:
            self.vertexMode = VertMode(r.readUInt32())
            self.lightingMode = LightMode(r.readUInt32())

# DEPRECATED (10.x), REMOVED (?)
# Not used in skinning.
# Unsure of use - perhaps for morphing animation or gravity.
class NiVertWeightsExtraData(NiExtraData): # X
    numBytes: int = 0                                   # Number of bytes in this data object.
    weight: list[float] = None                          # The vertex weights.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.numBytes = r.readUInt32()
        self.weight = r.readL16PArray(None, 'f')

# DEPRECATED (10.2), REMOVED (?), Replaced by NiBoolData.
# Visibility data for a controller.
class NiVisData(NiObject): # X
    keys: list[Key[byte]] = None

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.keys = r.readL32FArray(lambda z: Key[byte]('[byte]', r, KeyType.LINEAR_KEY))

# Allows applications to switch between drawing solid geometry or wireframe outlines.
class NiWireframeProperty(NiProperty): # X
    flags: Flags = None                                 # Property flags.
                                                        #     0 - Wireframe Mode Disabled
                                                        #     1 - Wireframe Mode Enabled

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.flags = Flags(r.readUInt16())

# Allows applications to set the test and write modes of the renderer's Z-buffer and to set the comparison function used for the Z-buffer test.
class NiZBufferProperty(NiProperty): # X
    flags: Flags = 3                                    # Bit 0 enables the z test
                                                        #     Bit 1 controls wether the Z buffer is read only (0) or read/write (1)
    function: ZCompareMode = ZCompareMode.ZCOMP_LESS_EQUAL # Z-Test function (see: glDepthFunc). In Flags from 20.1.0.3 on.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.flags = Flags(r.readUInt16())
        if r.v >= 0x0401000C and r.v <= 0x14000005: self.function = ZCompareMode(r.readUInt32())

# Morrowind-specific node for collision mesh.
class RootCollisionNode(NiNode): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# LEGACY (pre-10.1)
# Raw image data.
class NiRawImageData(NiObject): # Y
    width: int = 0                                      # Image width
    height: int = 0                                     # Image height
    imageType: ImageType = 0                            # The format of the raw image data.
    rgbImageData: list[list[Color3]] = None             # Image pixel data.
    rgbaImageData: list[list[Color4]] = None            # Image pixel data.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.width = r.readUInt32()
        self.height = r.readUInt32()
        self.imageType = ImageType(r.readUInt32())
        if self.imageType == ImageType.RGB: self.rgbImageData = r.readFArray(lambda k: r.readFArray(lambda z: Color3(r.readBytes(3)), Height), Width)
        if self.imageType == ImageType.RGBA: self.rgbaImageData = r.readFArray(lambda k: r.readFArray(lambda z: Color4(r.readBytes(4)), Height), Width)

# The type of animation interpolation (blending) that will be used on the associated key frames.
class BSShaderType(Enum): # Y
    SHADER_TALL_GRASS = 0           # Tall Grass Shader
    SHADER_DEFAULT = 1              # Standard Lighting Shader
    SHADER_SKY = 10                 # Sky Shader
    SHADER_SKIN = 14                # Skin Shader
    SHADER_WATER = 17               # Water Shader
    SHADER_LIGHTING30 = 29          # Lighting 3.0 Shader
    SHADER_TILE = 32                # Tiled Shader
    SHADER_NOLIGHTING = 33          # No Lighting Shader

# Shader Property Flags
class BSShaderFlags(Flag): # Y
    Specular = 0                    # Enables Specularity
    Skinned = 1 << 1                # Required For Skinned Meshes
    LowDetail = 1 << 2              # Lowddetail (seems to use standard diff/norm/spec shader)
    Vertex_Alpha = 1 << 3           # Vertex Alpha
    Unknown_1 = 1 << 4              # Unknown
    Single_Pass = 1 << 5            # Single Pass
    Empty = 1 << 6                  # Unknown
    Environment_Mapping = 1 << 7    # Environment mapping (uses Envmap Scale)
    Alpha_Texture = 1 << 8          # Alpha Texture Requires NiAlphaProperty to Enable
    Unknown_2 = 1 << 9              # Unknown
    FaceGen = 1 << 10               # FaceGen
    Parallax_Shader_Index_15 = 1 << 11 # Parallax
    Unknown_3 = 1 << 12             # Unknown/Crash
    Non_Projective_Shadows = 1 << 13# Non-Projective Shadows
    Unknown_4 = 1 << 14             # Unknown/Crash
    Refraction = 1 << 15            # Refraction (switches on refraction power)
    Fire_Refraction = 1 << 16       # Fire Refraction (switches on refraction power/period)
    Eye_Environment_Mapping = 1 << 17 # Eye Environment Mapping (does not use envmap light fade or envmap scale)
    Hair = 1 << 18                  # Hair
    Dynamic_Alpha = 1 << 19         # Dynamic Alpha
    Localmap_Hide_Secret = 1 << 20  # Localmap Hide Secret
    Window_Environment_Mapping = 1 << 21 # Window Environment Mapping
    Tree_Billboard = 1 << 22        # Tree Billboard
    Shadow_Frustum = 1 << 23        # Shadow Frustum
    Multiple_Textures = 1 << 24     # Multiple Textures (base diff/norm become null)
    Remappable_Textures = 1 << 25   # usually seen w/texture animation
    Decal_Single_Pass = 1 << 26     # Decal
    Dynamic_Decal_Single_Pass = 1 << 27 # Dynamic Decal
    Parallax_Occulsion = 1 << 28    # Parallax Occlusion
    External_Emittance = 1 << 29    # External Emittance
    Shadow_Map = 1 << 30            # Shadow Map
    ZBuffer_Test = 1 << 31          # ZBuffer Test (1=on)

# Shader Property Flags 2
class BSShaderFlags2(Flag): # Y
    ZBuffer_Write = 0               # ZBuffer Write
    LOD_Landscape = 1 << 1          # LOD Landscape
    LOD_Building = 1 << 2           # LOD Building
    No_Fade = 1 << 3                # No Fade
    Refraction_Tint = 1 << 4        # Refraction Tint
    Vertex_Colors = 1 << 5          # Has Vertex Colors
    Unknown1 = 1 << 6               # Unknown
    X1st_Light_is_Point_Light = 1 << 7 # 1st Light is Point Light
    X2nd_Light = 1 << 8             # 2nd Light
    X3rd_Light = 1 << 9             # 3rd Light
    Vertex_Lighting = 1 << 10       # Vertex Lighting
    Uniform_Scale = 1 << 11         # Uniform Scale
    Fit_Slope = 1 << 12             # Fit Slope
    Billboard_and_Envmap_Light_Fade = 1 << 13 # Billboard and Envmap Light Fade
    No_LOD_Land_Blend = 1 << 14     # No LOD Land Blend
    Envmap_Light_Fade = 1 << 15     # Envmap Light Fade
    Wireframe = 1 << 16             # Wireframe
    VATS_Selection = 1 << 17        # VATS Selection
    Show_in_Local_Map = 1 << 18     # Show in Local Map
    Premult_Alpha = 1 << 19         # Premult Alpha
    Skip_Normal_Maps = 1 << 20      # Skip Normal Maps
    Alpha_Decal = 1 << 21           # Alpha Decal
    No_Transparecny_Multisampling = 1 << 22 # No Transparency MultiSampling
    Unknown2 = 1 << 23              # Unknown
    Unknown3 = 1 << 24              # Unknown
    Unknown4 = 1 << 25              # Unknown
    Unknown5 = 1 << 26              # Unknown
    Unknown6 = 1 << 27              # Unknown
    Unknown7 = 1 << 28              # Unknown
    Unknown8 = 1 << 29              # Unknown
    Unknown9 = 1 << 30              # Unknown
    Unknown10 = 1 << 31             # Unknown

# Bethesda-specific property.
class BSShaderProperty(NiShadeProperty): # Y
    shaderType: BSShaderType = BSShaderType.SHADER_DEFAULT
    shaderFlags: BSShaderFlags = 0x82000000
    shaderFlags2: BSShaderFlags2 = 1
    environmentMapScale: float = 1.0                    # Scales the intensity of the environment/cube map.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.uv2 <= 34:
            self.shaderType = BSShaderType(r.readUInt32())
            self.shaderFlags = BSShaderFlags(r.readUInt32())
            self.shaderFlags2 = BSShaderFlags2(r.readUInt32())
            self.environmentMapScale = r.readSingle()

# Anim note types.
class AnimNoteType(Enum): # Z
    ANT_INVALID = 0                 # ANT_INVALID
    ANT_GRABIK = 1                  # ANT_GRABIK
    ANT_LOOKIK = 2                  # ANT_LOOKIK

# Bethesda-specific object.
class BSAnimNote(NiObject): # Z
    type: AnimNoteType = 0                              # Type of this note.
    time: float = None                                  # Location in time.
    arm: int = 0                                        # Unknown.
    gain: float = None                                  # Unknown.
    state: int = 0                                      # Unknown.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.type = AnimNoteType(r.readUInt32())
        self.time = r.readSingle()
        if Type == AnimNoteType.ANT_GRABIK: self.arm = r.readUInt32()
        if Type == AnimNoteType.ANT_LOOKIK:
            self.gain = r.readSingle()
            self.state = r.readUInt32()

# Bethesda-specific object.
class BSAnimNotes(NiObject): # Z
    animNotes: list[Ref[NiObject]] = None               # BSAnimNote objects.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.animNotes = r.readL16FArray(X[BSAnimNote].ref)

#endregion

