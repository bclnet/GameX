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
    FO_HAV_MAT_BROKEN_CONCRETE = 19, # Broken Concrete
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
    FO_HAV_MAT_BARREL_PLATFORM = 55, # Barrel
    FO_HAV_MAT_BOTTLE_PLATFORM = 56, # Bottle
    FO_HAV_MAT_SODA_CAN_PLATFORM = 57, # Soda Can
    FO_HAV_MAT_PISTOL_PLATFORM = 58, # Pistol
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
    FO_HAV_MAT_ELEVATOR_STAIRS = 79, # Elevator
    FO_HAV_MAT_HOLLOW_METAL_STAIRS = 80, # Hollow Metal
    FO_HAV_MAT_SHEET_METAL_STAIRS = 81, # Sheet Metal
    FO_HAV_MAT_SAND_STAIRS = 82,    # Sand
    FO_HAV_MAT_BROKEN_CONCRETE_STAIRS = 83, # Broken Concrete
    FO_HAV_MAT_VEHICLE_BODY_STAIRS = 84, # Vehicle Body
    FO_HAV_MAT_VEHICLE_PART_SOLID_STAIRS = 85, # Vehicle Part Solid
    FO_HAV_MAT_VEHICLE_PART_HOLLOW_STAIRS = 86, # Vehicle Part Hollow
    FO_HAV_MAT_BARREL_STAIRS = 87,  # Barrel
    FO_HAV_MAT_BOTTLE_STAIRS = 88,  # Bottle
    FO_HAV_MAT_SODA_CAN_STAIRS = 89, # Soda Can
    FO_HAV_MAT_PISTOL_STAIRS = 90,  # Pistol
    FO_HAV_MAT_RIFLE_STAIRS = 91,   # Rifle
    FO_HAV_MAT_SHOPPING_CART_STAIRS = 92, # Shopping Cart
    FO_HAV_MAT_LUNCHBOX_STAIRS = 93, # Lunchbox
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
    SKY_HAV_MAT_DRAGON = 2518321175, # Dragon
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
    FOL_TRANSPARENT_SMALL_ANIM = 28, # TransparentSmallAnim (white)
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
    SOLVER_DEACTIVATION_INVALID = 0, # Invalid
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
    SBP_56_MOD_CHEST_SECONDARY = 56, # Chest secondary or undergarment
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
    BP_TORSOSECTION_LEFTARM2 = 4000, # Torso Section | Left Arm 2
    BP_TORSOSECTION_RIGHTARM = 5000, # Torso Section | Right Arm
    BP_TORSOSECTION_RIGHTARM2 = 6000, # Torso Section | Right Arm 2
    BP_TORSOSECTION_LEFTLEG = 7000, # Torso Section | Left Leg
    BP_TORSOSECTION_LEFTLEG2 = 8000, # Torso Section | Left Leg 2
    BP_TORSOSECTION_LEFTLEG3 = 9000, # Torso Section | Left Leg 3
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
    roots: list[int] = r.readL32FArray(X[NiObject].ref) if h.v >= 0x0303000D else None # List of root NIF objects. If there is a camera, for 1st person view, then this NIF object is referred to as well in this list, even if it is not a root object (usually we want the camera to be attached to the Bip Head node).

# The distance range where a specific level of detail applies.
class LODRange:
    nearExtent: float = r.readSingle()                  # Begining of range.
    farExtent: float = r.readSingle()                   # End of Range.
    unknownInts: list[int] = r.readUInt32() if h.v <= 50397184 else None # Unknown (0,0,0).

# Group of vertex indices of vertices that match.
class MatchGroup:
    vertexIndices: list[int] = r.readL16FArray(lambda r: r.readUInt16()) # The vertex indices.

# ByteVector3 -> Vector3(r.readByte(), r.readByte(), r.readByte())
# HalfVector3 -> r.readHalf()
# Vector3 -> r.readVector3()
# Vector4 -> r.readVector4()
# Quaternion -> r.readQuaternion()
# hkQuaternion -> r.readQuaternionWFirst()
# Matrix22 -> r.readMatrix2x2()
# Matrix33 -> r.readMatrix3x3()
# Matrix34 -> r.readMatrix3x4()
# Matrix44 -> r.readMatrix4x4()
# hkMatrix3 -> r.readMatrix3x4()
# MipMap -> ??
# NodeSet -> ??
# ShortString -> r.readL8AString()
# NiBoneLODController::SkinInfo. Reference to shape and skin instance.
class SkinInfo:
    shape: int = X[NiTriBasedGeom].ptr(r)
    skinInstance: int = X[NiSkinInstance].ref(r)

# A set of NiBoneLODController::SkinInfo.
class SkinInfoSet:
    skinInfo: list[SkinInfo] = r.readL32FArray(lambda r: SkinInfo(r))

# NiSkinData::BoneVertData. A vertex and its weight.
class BoneVertData:
    index: int = r.readUInt16()                         # The vertex index, in the mesh.
    weight: float = r.readSingle()                      # The vertex weight - between 0.0 and 1.0

# NiSkinData::BoneVertData. A vertex and its weight.
class BoneVertDataHalf:
    index: int = r.readUInt16()                         # The vertex index, in the mesh.
    weight: float = r.readHalf()                        # The vertex weight - between 0.0 and 1.0

# Used in NiDefaultAVObjectPalette.
class AVObject:
    name: str = r.readL32AString()                      # Object name.
    avObject: int = X[NiAVObject].ptr(r)                # Object reference.

# In a .kf file, this links to a controllable object, via its name (or for version 10.2.0.0 and up, a link and offset to a NiStringPalette that contains the name), and a sequence of interpolators that apply to this controllable object, via links.
# For Controller ID, NiInterpController::GetCtlrID() virtual function returns a string formatted specifically for the derived type.
# For Interpolator ID, NiInterpController::GetInterpolatorID() virtual function returns a string formatted specifically for the derived type.
# The string formats are documented on the relevant niobject blocks.
class ControlledBlock:
    targetName: str = Y.string(r) if h.v <= 0x0A010067 else None # Name of a controllable object in another NIF file.
    interpolator: int = X[NiInterpolator].ref(r) if h.v >= 0x0A01006A else None
    controller: int = X[NiTimeController].ref(r) if h.v <= 0x14050000 else None
    blendInterpolator: int = X[NiBlendInterpolator].ref(r) if h.v >= 0x0A010068 and h.v <= 0x0A01006E else None
    blendIndex: int = r.readUInt16() if h.v >= 0x0A010068 and h.v <= 0x0A01006E else None
    priority: int = r.readByte() if h.v >= 0x0A01006A and  else None # Idle animations tend to have low values for this, and high values tend to correspond with the important parts of the animations.
    nodeName: str = Y.string(r) if h.v >= 0x14010001 else None # The name of the animated NiAVObject.
    propertyType: str = Y.string(r) if h.v >= 0x14010001 else None # The RTTI type of the NiProperty the controller is attached to, if applicable.
    controllerType: str = Y.string(r) if h.v >= 0x14010001 else None # The RTTI type of the NiTimeController.
    controllerId: str = Y.string(r) if h.v >= 0x14010001 else None # An ID that can uniquely identify the controller among others of the same type on the same NiObjectNET.
    interpolatorId: str = Y.string(r) if h.v >= 0x14010001 else None # An ID that can uniquely identify the interpolator among others of the same type on the same NiObjectNET.
    stringPalette: int = X[NiStringPalette].ref(r) if h.v >= 0x0A020000 and h.v <= 0x14010000 else None # Refers to the NiStringPalette which contains the name of the controlled NIF object.
    nodeNameOffset: int = r.readUInt32() if h.v >= 0x0A020000 and h.v <= 0x14010000 else None # Offset in NiStringPalette to the name of the animated NiAVObject.
    propertyTypeOffset: int = r.readUInt32() if h.v >= 0x0A020000 and h.v <= 0x14010000 else None # Offset in NiStringPalette to the RTTI type of the NiProperty the controller is attached to, if applicable.
    controllerTypeOffset: int = r.readUInt32() if h.v >= 0x0A020000 and h.v <= 0x14010000 else None # Offset in NiStringPalette to the RTTI type of the NiTimeController.
    controllerIdOffset: int = r.readUInt32() if h.v >= 0x0A020000 and h.v <= 0x14010000 else None # Offset in NiStringPalette to an ID that can uniquely identify the controller among others of the same type on the same NiObjectNET.
    interpolatorIdOffset: int = r.readUInt32() if h.v >= 0x0A020000 and h.v <= 0x14010000 else None # Offset in NiStringPalette to an ID that can uniquely identify the interpolator among others of the same type on the same NiObjectNET.

# Information about how the file was exported
class ExportInfo:
    author: str = r.readL8AString()
    processScript: str = r.readL8AString()
    exportScript: str = r.readL8AString()

# The NIF file header.
class Header:
    headerString: str = ??                              # 'NetImmerse File Format x.x.x.x' (versions <= 10.0.1.2) or 'Gamebryo File Format x.x.x.x' (versions >= 10.1.0.0), with x.x.x.x the version written out. Ends with a newline character (0x0A).
    copyright: list[str] = ?? if h.v <= 0x03010000 else None
    version: int = r.readUInt32() if h.v >= 0x03010001 else None # The NIF version, in hexadecimal notation: 0x04000002, 0x0401000C, 0x04020002, 0x04020100, 0x04020200, 0x0A000100, 0x0A010000, 0x0A020000, 0x14000004, ...
    endianType: EndianType = EndianType(r.readByte()) if h.v >= 0x14000003 else None # Determines the endianness of the data in the file.
    userVersion: int = r.readUInt32() if h.v >= 0x0A000108 else None # An extra version number, for companies that decide to modify the file format.
    numBlocks: int = r.readUInt32() if h.v >= 0x03010001 else None # Number of file objects.
    userVersion2: int = r.readUInt32() if ((Version == 20.2.0.7) or (Version == 20.0.0.5) or ((Version >= 10.0.1.2) and (Version <= 20.0.0.4) and (h.userVersion <= 11))) and (h.userVersion >= 3) else None
    exportInfo: ExportInfo = ExportInfo(r) if               ((Version == 20.2.0.7) or (Version == 20.0.0.5) or ((Version >= 10.0.1.2) and (Version <= 20.0.0.4) and (h.userVersion <= 11))) and (h.userVersion >= 3) else None
    maxFilepath: str = r.readL8AString() if (h.userVersion2 == 130) else None
    metadata: bytearray = r.readL8Bytes() if h.v >= 0x1E000000 else None
    numBlockTypes: int = r.readUInt16() if h.v >= 0x05000001 else None # Number of object types in this NIF file.
    blockTypes: list[str] = r.readL32AString() if Version != 20.3.1.2 and h.v >= 0x05000001 else None # List of all object types used in this NIF file.
    blockTypeHashes: list[int] = r.readUInt32() if h.v >= 0x14030102 and h.v <= 0x14030102 else None # List of all object types used in this NIF file.
    blockTypeIndex: list[int] = r.readUInt16() if h.v >= 0x05000001 else None # Maps file objects on their corresponding type: first file object is of type object_types[object_type_index[0]], the second of object_types[object_type_index[1]], etc.
    blockSize: list[int] = r.readUInt32() if h.v >= 0x14020005 else None # Array of block sizes?
    numStrings: int = r.readUInt32() if h.v >= 0x14010001 else None # Number of strings.
    maxStringLength: int = r.readUInt32() if h.v >= 0x14010001 else None # Maximum string length.
    strings: list[str] = r.readL32AString() if h.v >= 0x14010001 else None # Strings.
    groups: list[int] = r.readL32FArray(lambda r: r.readUInt32()) if h.v >= 0x05000006 else None

# A list of \\0 terminated strings.
class StringPalette:
    palette: str = r.readL32AString()                   # A bunch of 0x00 seperated strings.
    length: int = r.readUInt32()                        # Length of the palette string is repeated here.

# Tension, bias, continuity.
class TBC:
    t: float = r.readSingle()                           # Tension.
    b: float = r.readSingle()                           # Bias.
    c: float = r.readSingle()                           # Continuity.

# A generic key with support for interpolation. Type 1 is normal linear interpolation, type 2 has forward and backward tangents, and type 3 has tension, bias and continuity arguments. Note that color4 and byte always seem to be of type 1.
class Key:
    time: float = r.readSingle()                        # Time of the key.
    value: T = ??                                       # The key value.
    forward: T = ?? if ARG == 2 else None               # Key forward tangent.
    backward: T = ?? if ARG == 2 else None              # The key backward tangent.
    tbc: TBC = TBC(r) if ARG == 3 else None             # The TBC of the key.

# Array of vector keys (anything that can be interpolated, except rotations).
class KeyGroup:
    numKeys: int = r.readUInt32()                       # Number of keys in the array.
    interpolation: KeyType = KeyType(r.readUInt32()) if Num Keys != 0 else None # The key type.
    keys: list[Key] = Key(r, h)                         # The keys.

# A special version of the key type used for quaternions.  Never has tangents.
class QuatKey:
    time: float = r.readSingle() if ARG != 4 and h.v >= 0x0A01006A else None # Time the key applies.
    value: T = ?? if ARG != 4 else None                 # Value of the key.
    tbc: TBC = TBC(r) if ARG == 3 else None             # The TBC of the key.

# Texture coordinates (u,v). As in OpenGL; image origin is in the lower left corner.
class TexCoord:
    u: float = r.readSingle()                           # First coordinate.
    v: float = r.readSingle()                           # Second coordinate.

# Texture coordinates (u,v).
class HalfTexCoord:
    u: float = r.readHalf()                             # First coordinate.
    v: float = r.readHalf()                             # Second coordinate.

# Describes the order of scaling and rotation matrices. Translate, Scale, Rotation, Center are from TexDesc.
# Back = inverse of Center. FromMaya = inverse of the V axis with a positive translation along V of 1 unit.
class TransformMethod(Enum):
    Maya_Deprecated = 0,            # Center * Rotation * Back * Translate * Scale
    Max = 1,                        # Center * Scale * Rotation * Translate * Back
    Maya = 2                        # Center * Rotation * Back * FromMaya * Translate * Scale

# NiTexturingProperty::Map. Texture description.
class TexDesc:
    image: int = X[NiImage].ref(r) if h.v <= 50397184 else None # Link to the texture image.
    source: int = X[NiSourceTexture].ref(r) if h.v >= 0x0303000D else None # NiSourceTexture object index.
    clampMode: TexClampMode = TexClampMode(r.readUInt32()) if h.v <= 0x14000005 else None # 0=clamp S clamp T, 1=clamp S wrap T, 2=wrap S clamp T, 3=wrap S wrap T
    filterMode: TexFilterMode = TexFilterMode(r.readUInt32()) if h.v <= 0x14000005 else None # 0=nearest, 1=bilinear, 2=trilinear, 3=..., 4=..., 5=...
    flags: Flags = Flags(r.readUInt16()) if h.v >= 0x14010003 else None # Texture mode flags; clamp and filter mode stored in upper byte with 0xYZ00 = clamp mode Y, filter mode Z.
    maxAnisotropy: int = r.readUInt16() if h.v >= 0x14050004 else None
    uvSet: int = r.readUInt32() if h.v <= 0x14000005 else None # The texture coordinate set in NiGeometryData that this texture slot will use.
    pS2L: int = r.readInt16() if h.v <= 0x0A040001 else None # L can range from 0 to 3 and are used to specify how fast a texture gets blurry.
    pS2K: int = r.readInt16() if h.v <= 0x0A040001 else None # K is used as an offset into the mipmap levels and can range from -2047 to 2047. Positive values push the mipmap towards being blurry and negative values make the mipmap sharper.
    unknown1: int = r.readUInt16() if h.v <= 0x0401000C else None # Unknown, 0 or 0x0101?
    hasTextureTransform: bool = r.readBool32() if h.v >= 0x0A010000 else None # Whether or not the texture coordinates are transformed.
    translation: TexCoord = TexCoord(r) if Has Texture Transform and h.v >= 0x0A010000 else None # The UV translation.
    scale: TexCoord = TexCoord(r) if Has Texture Transform and h.v >= 0x0A010000 else None # The UV scale.
    rotation: float = r.readSingle() if Has Texture Transform and h.v >= 0x0A010000 else None # The W axis rotation in texture space.
    transformMethod: TransformMethod = TransformMethod(r.readUInt32()) if Has Texture Transform and h.v >= 0x0A010000 else None # Depending on the source, scaling can occur before or after rotation.
    center: TexCoord = TexCoord(r) if Has Texture Transform and h.v >= 0x0A010000 else None # The origin around which the texture rotates.

# NiTexturingProperty::ShaderMap. Shader texture description.
class ShaderTexDesc:
    hasMap: bool = r.readBool32()
    map: TexDesc = TexDesc(r, h) if Has Map else None
    mapId: int = r.readUInt32() if Has Map else None    # Unique identifier for the Gamebryo shader system.

# List of three vertex indices.
class Triangle:
    v1: int = r.readUInt16()                            # First vertex index.
    v2: int = r.readUInt16()                            # Second vertex index.
    v3: int = r.readUInt16()                            # Third vertex index.

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
    vertex: Vector3 = r.readVector3() if ((ARG & 16) != 0) and ((ARG & 16384) != 0) else None
    bitangentX: float = r.readSingle() if ((ARG & 16) != 0) and ((ARG & 256) != 0) and ((ARG & 16384) != 0) else None
    unknownShort: int = r.readUInt16() if ((ARG & 16) != 0) and ((ARG & 256) == 0) and ((ARG & 16384) == 0) else None
    unknownInt: int = r.readUInt32() if ((ARG & 16) != 0) and ((ARG & 256) == 0) and ((ARG & 16384) != 0) else None
    uv: HalfTexCoord = HalfTexCoord(r) if ((ARG & 32) != 0) else None
    normal: Vector3 = Vector3(r.readByte(), r.readByte(), r.readByte()) if (ARG & 128) != 0 else None
    bitangentY: int = r.readByte() if (ARG & 128) != 0 else None
    tangent: Vector3 = Vector3(r.readByte(), r.readByte(), r.readByte()) if ((ARG & 128) != 0) and ((ARG & 256) != 0) else None
    bitangentZ: int = r.readByte() if ((ARG & 128) != 0) and ((ARG & 256) != 0) else None
    vertexColors: ByteColor4 = Color4Byte(r) if (ARG & 512) != 0 else None
    boneWeights: list[float] = r.readHalf() if (ARG & 1024) != 0 else None
    boneIndices: list[int] = r.readByte() if (ARG & 1024) != 0 else None
    eyeData: float = r.readSingle() if (ARG & 4096) != 0 else None

class BSVertexDataSSE:
    vertex: Vector3 = r.readVector3() if ((ARG & 16) != 0) else None
    bitangentX: float = r.readSingle() if ((ARG & 16) != 0) and ((ARG & 256) != 0) else None
    unknownInt: int = r.readInt32() if ((ARG & 16) != 0) and (ARG & 256) == 0 else None
    uv: HalfTexCoord = HalfTexCoord(r) if ((ARG & 32) != 0) else None
    normal: Vector3 = Vector3(r.readByte(), r.readByte(), r.readByte()) if (ARG & 128) != 0 else None
    bitangentY: int = r.readByte() if (ARG & 128) != 0 else None
    tangent: Vector3 = Vector3(r.readByte(), r.readByte(), r.readByte()) if ((ARG & 128) != 0) and ((ARG & 256) != 0) else None
    bitangentZ: int = r.readByte() if ((ARG & 128) != 0) and ((ARG & 256) != 0) else None
    vertexColors: ByteColor4 = Color4Byte(r) if (ARG & 512) != 0 else None
    boneWeights: list[float] = r.readHalf() if (ARG & 1024) != 0 else None
    boneIndices: list[int] = r.readByte() if (ARG & 1024) != 0 else None
    eyeData: float = r.readSingle() if (ARG & 4096) != 0 else None

class BSVertexDesc:
    vF1: int = r.readByte()
    vF2: int = r.readByte()
    vF3: int = r.readByte()
    vF4: int = r.readByte()
    vF5: int = r.readByte()
    vertexAttributes: VertexFlags = VertexFlags(r.readUInt16())
    vF8: int = r.readByte()

# Skinning data for a submesh, optimized for hardware skinning. Part of NiSkinPartition.
class SkinPartition:
    numVertices: int = r.readUInt16()                   # Number of vertices in this submesh.
    numTriangles: int = r.readUInt16()                  # Number of triangles in this submesh.
    numBones: int = r.readUInt16()                      # Number of bones influencing this submesh.
    numStrips: int = r.readUInt16()                     # Number of strips in this submesh (zero if not stripped).
    numWeightsPerVertex: int = r.readUInt16()           # Number of weight coefficients per vertex. The Gamebryo engine seems to work well only if this number is equal to 4, even if there are less than 4 influences per vertex.
    bones: list[int] = r.readUInt16()                   # List of bones.
    hasVertexMap: bool = r.readBool32() if h.v >= 0x0A010000 else None # Do we have a vertex map?
    vertexMap: list[int] = r.readUInt16() if Has Vertex Map and h.v >= 0x0A010000 else None # Maps the weight/influence lists in this submesh to the vertices in the shape being skinned.
    hasVertexWeights: bool = r.readBool32() if h.v >= 0x0A010000 else None # Do we have vertex weights?
    vertexWeights: list[list[float]] = r.readHalf() if Has Vertex Weights == 15 and h.v >= 0x14030101 else None # The vertex weights.
    stripLengths: list[int] = r.readUInt16()            # The strip lengths.
    hasFaces: bool = r.readBool32() if h.v >= 0x0A010000 else None # Do we have triangle or strip data?
    strips: list[list[int]] = r.readUInt16() if (Has Faces) and (Num Strips != 0) and h.v >= 0x0A010000 else None # The strips.
    triangles: list[Triangle] = Triangle(r) if (Has Faces) and (Num Strips == 0) and h.v >= 0x0A010000 else None # The triangles.
    hasBoneIndices: bool = r.readBool32()               # Do we have bone indices?
    boneIndices: list[list[int]] = r.readByte() if Has Bone Indices else None # Bone indices, they index into 'Bones'.
    unknownShort: int = r.readUInt16()                  # Unknown
    vertexDesc: BSVertexDesc = BSVertexDesc(r) if (h.userVersion2 == 100) else None
    trianglesCopy: list[Triangle] = Triangle(r) if (h.userVersion2 == 100) else None

# A plane.
class NiPlane:
    normal: Vector3 = r.readVector3()                   # The plane normal.
    constant: float = r.readSingle()                    # The plane constant.

# A sphere.
class NiBound:
    center: Vector3 = r.readVector3()                   # The sphere's center.
    radius: float = r.readSingle()                      # The sphere's radius.

class NiQuatTransform:
    translation: Vector3 = r.readVector3()
    rotation: Quaternion = r.readQuaternion()
    scale: float = r.readSingle()
    trsValid: list[bool] = r.readBool32() if h.v <= 0x0A01006D else None # Whether each transform component is valid.

class NiTransform:
    rotation: Matrix3x3 = r.readMatrix3x3()             # The rotation part of the transformation matrix.
    translation: Vector3 = r.readVector3()              # The translation vector.
    scale: float = r.readSingle()                       # Scaling part (only uniform scaling is supported).

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
    offset: Vector3 = r.readVector3()                   # Offset of furniture marker.
    orientation: int = r.readUInt16()                   # Furniture marker orientation.
    positionRef1: int = r.readByte()                    # Refers to a furnituremarkerxx.nif file. Always seems to be the same as Position Ref 2.
    positionRef2: int = r.readByte()                    # Refers to a furnituremarkerxx.nif file. Always seems to be the same as Position Ref 1.
    heading: float = r.readSingle()                     # Similar to Orientation, in float form.
    animationType: AnimationType = AnimationType(r.readUInt16()) # Unknown
    entryProperties: FurnitureEntryPoints = FurnitureEntryPoints(r.readUInt16()) # Unknown/unused in nif?

# Bethesda Havok. A triangle with extra data used for physics.
class TriangleData:
    triangle: Triangle = Triangle(r)                    # The triangle.
    weldingInfo: int = r.readUInt16()                   # Additional havok information on how triangles are welded.
    normal: Vector3 = r.readVector3() if h.v <= 0x14000005 else None # This is the triangle's normal.

# Geometry morphing data component.
class Morph:
    frameName: str = Y.string(r) if h.v >= 0x0A01006A else None # Name of the frame.
    numKeys: int = r.readUInt32() if h.v <= 0x0A010000 else None # The number of morph keys that follow.
    interpolation: KeyType = KeyType(r.readUInt32()) if h.v <= 0x0A010000 else None # Unlike most objects, the presense of this value is not conditional on there being keys.
    keys: list[Key] = Key(r, h) if h.v <= 0x0A010000 else None # The morph key frames.
    legacyWeight: float = r.readSingle() if h.v >= 0x0A010068 and h.v <= 0x14010002 and  else None
    vectors: list[Vector3] = r.readVector3()            # Morph vectors.

# particle array entry
class Particle:
    velocity: Vector3 = r.readVector3()                 # Particle velocity
    unknownVector: Vector3 = r.readVector3()            # Unknown
    lifetime: float = r.readSingle()                    # The particle age.
    lifespan: float = r.readSingle()                    # Maximum age of the particle.
    timestamp: float = r.readSingle()                   # Timestamp of the last update.
    unknownShort: int = r.readUInt16()                  # Unknown short
    vertexId: int = r.readUInt16()                      # Particle/vertex index matches array index

# NiSkinData::BoneData. Skinning data component.
class BoneData:
    skinTransform: NiTransform = NiTransform(r)         # Offset of the skin from this bone in bind position.
    boundingSphereOffset: Vector3 = r.readVector3()     # Translation offset of a bounding sphere holding all vertices. (Note that its a Sphere Containing Axis Aligned Box not a minimum volume Sphere)
    boundingSphereRadius: float = r.readSingle()        # Radius for bounding sphere holding all vertices.
    unknown13Shorts: list[int] = r.readInt16() if h.v >= 0x14030009 and h.v <= 0x14030009 and  else None # Unknown, always 0?
    numVertices: int = r.readUInt16()                   # Number of weighted vertices.
    vertexWeights: list[BoneVertDataHalf] = BoneVertDataHalf(r) if ARG == 15 and h.v >= 0x14030101 else None # The vertex weights.

# Bethesda Havok. Collision filter info representing Layer, Flags, Part Number, and Group all combined into one uint.
class HavokFilter:
    layer: SkyrimLayer = SkyrimLayer(r.readByte())      # The layer the collision belongs to.
    flagsandPartNumber: int = r.readByte()              # FLAGS are stored in highest 3 bits:
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
    group: int = r.readUInt16()

# Bethesda Havok. Material wrapper for varying material enums by game.
class HavokMaterial:
    unknownInt: int = r.readUInt32() if h.v <= 0x0A000102 else None
    material: SkyrimHavokMaterial = SkyrimHavokMaterial(r.readUInt32()) # The material of the shape.

# Bethesda Havok. Havok Information for packed TriStrip shapes.
class OblivionSubShape:
    havokFilter: HavokFilter = HavokFilter(r, h)
    numVertices: int = r.readUInt32()                   # The number of vertices that form this sub shape.
    material: HavokMaterial = HavokMaterial(r, h)       # The material of the subshape.

class bhkPositionConstraintMotor:
    minForce: float = r.readSingle()                    # Minimum motor force
    maxForce: float = r.readSingle()                    # Maximum motor force
    tau: float = r.readSingle()                         # Relative stiffness
    damping: float = r.readSingle()                     # Motor damping value
    proportionalRecoveryVelocity: float = r.readSingle() # A factor of the current error to calculate the recovery velocity
    constantRecoveryVelocity: float = r.readSingle()    # A constant velocity which is used to recover from errors
    motorEnabled: bool = r.readBool32()                 # Is Motor enabled

class bhkVelocityConstraintMotor:
    minForce: float = r.readSingle()                    # Minimum motor force
    maxForce: float = r.readSingle()                    # Maximum motor force
    tau: float = r.readSingle()                         # Relative stiffness
    targetVelocity: float = r.readSingle()
    useVelocityTarget: bool = r.readBool32()
    motorEnabled: bool = r.readBool32()                 # Is Motor enabled

class bhkSpringDamperConstraintMotor:
    minForce: float = r.readSingle()                    # Minimum motor force
    maxForce: float = r.readSingle()                    # Maximum motor force
    springConstant: float = r.readSingle()              # The spring constant in N/m
    springDamping: float = r.readSingle()               # The spring damping in Nsec/m
    motorEnabled: bool = r.readBool32()                 # Is Motor enabled

class MotorType(Enum):
    MOTOR_NONE = 0,
    MOTOR_POSITION = 1,
    MOTOR_VELOCITY = 2,
    MOTOR_SPRING = 3

class MotorDescriptor:
    type: MotorType = MotorType(r.readByte())
    positionMotor: bhkPositionConstraintMotor = bhkPositionConstraintMotor(r) if Type == 1 else None
    velocityMotor: bhkVelocityConstraintMotor = bhkVelocityConstraintMotor(r) if Type == 2 else None
    springDamperMotor: bhkSpringDamperConstraintMotor = bhkSpringDamperConstraintMotor(r) if Type == 3 else None

# This constraint defines a cone in which an object can rotate. The shape of the cone can be controlled in two (orthogonal) directions.
class RagdollDescriptor:
    pivotA: Vector4 = r.readVector4()                   # Point around which the object will rotate. Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    planeA: Vector4 = r.readVector4()                   # Defines the orthogonal plane in which the body can move, the orthogonal directions in which the shape can be controlled (the direction orthogonal on this one and Twist A).
    twistA: Vector4 = r.readVector4()                   # Central directed axis of the cone in which the object can rotate. Orthogonal on Plane A.
    pivotB: Vector4 = r.readVector4()                   # Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    planeB: Vector4 = r.readVector4()                   # Defines the orthogonal plane in which the body can move, the orthogonal directions in which the shape can be controlled (the direction orthogonal on this one and Twist A).
    twistB: Vector4 = r.readVector4()                   # Central directed axis of the cone in which the object can rotate. Orthogonal on Plane B.
    motorA: Vector4 = r.readVector4()                   # Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    motorB: Vector4 = r.readVector4()                   # Defines the orthogonal directions in which the shape can be controlled (namely in this direction, and in the direction orthogonal on this one and Twist A).
    coneMaxAngle: float = r.readSingle()                # Maximum angle the object can rotate around the vector orthogonal on Plane A and Twist A relative to the Twist A vector. Note that Cone Min Angle is not stored, but is simply minus this angle.
    planeMinAngle: float = r.readSingle()               # Minimum angle the object can rotate around Plane A, relative to Twist A.
    planeMaxAngle: float = r.readSingle()               # Maximum angle the object can rotate around Plane A, relative to Twist A.
    twistMinAngle: float = r.readSingle()               # Minimum angle the object can rotate around Twist A, relative to Plane A.
    twistMaxAngle: float = r.readSingle()               # Maximum angle the object can rotate around Twist A, relative to Plane A.
    maxFriction: float = r.readSingle()                 # Maximum friction, typically 0 or 10. In Fallout 3, typically 100.
    motor: MotorDescriptor = MotorDescriptor(r, h) if h.v >= 0x14020007 and  else None

# This constraint allows rotation about a specified axis, limited by specified boundaries.
class LimitedHingeDescriptor:
    pivotA: Vector4 = r.readVector4()                   # Pivot point around which the object will rotate.
    axleA: Vector4 = r.readVector4()                    # Axis of rotation.
    perp2AxleInA1: Vector4 = r.readVector4()            # Vector in the rotation plane which defines the zero angle.
    perp2AxleInA2: Vector4 = r.readVector4()            # Vector in the rotation plane, orthogonal on the previous one, which defines the positive direction of rotation. This is always the vector product of Axle A and Perp2 Axle In A1.
    pivotB: Vector4 = r.readVector4()                   # Pivot A in second entity coordinate system.
    axleB: Vector4 = r.readVector4()                    # Axle A in second entity coordinate system.
    perp2AxleInB2: Vector4 = r.readVector4()            # Perp2 Axle In A2 in second entity coordinate system.
    perp2AxleInB1: Vector4 = r.readVector4()            # Perp2 Axle In A1 in second entity coordinate system.
    minAngle: float = r.readSingle()                    # Minimum rotation angle.
    maxAngle: float = r.readSingle()                    # Maximum rotation angle.
    maxFriction: float = r.readSingle()                 # Maximum friction, typically either 0 or 10. In Fallout 3, typically 100.
    motor: MotorDescriptor = MotorDescriptor(r, h) if h.v >= 0x14020007 and  else None

# This constraint allows rotation about a specified axis.
class HingeDescriptor:
    pivotA: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Pivot point around which the object will rotate.
    perp2AxleInA1: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Vector in the rotation plane which defines the zero angle.
    perp2AxleInA2: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Vector in the rotation plane, orthogonal on the previous one, which defines the positive direction of rotation. This is always the vector product of Axle A and Perp2 Axle In A1.
    pivotB: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Pivot A in second entity coordinate system.
    axleB: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Axle A in second entity coordinate system.
    axleA: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Axis of rotation.
    perp2AxleInB1: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Perp2 Axle In A1 in second entity coordinate system.
    perp2AxleInB2: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Perp2 Axle In A2 in second entity coordinate system.

class BallAndSocketDescriptor:
    pivotA: Vector4 = r.readVector4()                   # Pivot point in the local space of entity A.
    pivotB: Vector4 = r.readVector4()                   # Pivot point in the local space of entity B.

class PrismaticDescriptor:
    pivotA: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Pivot.
    rotationA: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Rotation axis.
    planeA: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Plane normal. Describes the plane the object is able to move on.
    slidingA: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Describes the axis the object is able to travel along. Unit vector.
    pivotB: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Pivot in B coordinates.
    rotationB: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Rotation axis.
    planeB: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Plane normal. Describes the plane the object is able to move on in B coordinates.
    slidingB: Vector4 = r.readVector4() if h.v >= 0x14020007 else None # Describes the axis the object is able to travel along in B coordinates. Unit vector.
    minDistance: float = r.readSingle()                 # Describe the min distance the object is able to travel.
    maxDistance: float = r.readSingle()                 # Describe the max distance the object is able to travel.
    friction: float = r.readSingle()                    # Friction.
    motor: MotorDescriptor = MotorDescriptor(r, h) if h.v >= 0x14020007 and  else None

class StiffSpringDescriptor:
    pivotA: Vector4 = r.readVector4()
    pivotB: Vector4 = r.readVector4()
    length: float = r.readSingle()

# Used to store skin weights in NiTriShapeSkinController.
class OldSkinData:
    vertexWeight: float = r.readSingle()                # The amount that this bone affects the vertex.
    vertexIndex: int = r.readUInt16()                   # The index of the vertex that this weight applies to.
    unknownVector: Vector3 = r.readVector3()            # Unknown.  Perhaps some sort of offset?

# Determines how the raw image data is stored in NiRawImageData.
class ImageType(Enum):
    RGB = 1,                        # Colors store red, blue, and green components.
    RGBA = 2                        # Colors store red, blue, green, and alpha components.

# Box Bounding Volume
class BoxBV:
    center: Vector3 = r.readVector3()
    axis: list[Vector3] = r.readVector3()
    extent: Vector3 = r.readVector3()

# Capsule Bounding Volume
class CapsuleBV:
    center: Vector3 = r.readVector3()
    origin: Vector3 = r.readVector3()
    extent: float = r.readSingle()
    radius: float = r.readSingle()

class HalfSpaceBV:
    plane: NiPlane = NiPlane(r)
    center: Vector3 = r.readVector3()

class BoundingVolume:
    collisionType: BoundVolumeType = BoundVolumeType(r.readUInt32()) # Type of collision data.
    sphere: NiBound = NiBound(r) if Collision Type == 0 else None
    box: BoxBV = BoxBV(r) if Collision Type == 1 else None
    capsule: CapsuleBV = CapsuleBV(r) if Collision Type == 2 else None
    union: UnionBV = UnionBV(r) if Collision Type == 4 else None
    halfSpace: HalfSpaceBV = HalfSpaceBV(r) if Collision Type == 5 else None

class UnionBV:
    boundingVolumes: list[BoundingVolume] = r.readL32FArray(lambda r: BoundingVolume(r, h))

class MorphWeight:
    interpolator: int = X[NiInterpolator].ref(r)
    weight: float = r.readSingle()

# Transformation data for the bone at this index in bhkPoseArray.
class BoneTransform:
    translation: Vector3 = r.readVector3()
    rotation: Quaternion = r.readQuaternionWFirst()
    scale: Vector3 = r.readVector3()

# A list of transforms for each bone in bhkPoseArray.
class BonePose:
    transforms: list[BoneTransform] = r.readL32FArray(lambda r: BoneTransform(r))

# Array of Vectors for Decal placement in BSDecalPlacementVectorExtraData.
class DecalVectorArray:
    numVectors: int = r.readInt16()
    points: list[Vector3] = r.readVector3()             # Vector XYZ coords
    normals: list[Vector3] = r.readVector3()            # Vector Normals

# Editor flags for the Body Partitions.
class BSPartFlag(Flag):
    PF_EDITOR_VISIBLE = 0,          # Visible in Editor
    PF_START_NET_BONESET = 1 << 8   # Start a new shared boneset.  It is expected this BoneSet and the following sets in the Skin Partition will have the same bones.

# Body part list for DismemberSkinInstance
class BodyPartList:
    partFlag: BSPartFlag = BSPartFlag(r.readUInt16())   # Flags related to the Body Partition
    bodyPart: BSDismemberBodyPartType = BSDismemberBodyPartType(r.readUInt16()) # Body Part Index

# Stores Bone Level of Detail info in a BSBoneLODExtraData
class BoneLOD:
    distance: int = r.readUInt32()
    boneName: str = Y.string(r)

# Per-chunk material, used in bhkCompressedMeshShapeData
class bhkCMSDMaterial:
    material: SkyrimHavokMaterial = SkyrimHavokMaterial(r.readUInt32())
    filter: HavokFilter = HavokFilter(r, h)

# Triangle indices used in pair with "Big Verts" in a bhkCompressedMeshShapeData.
class bhkCMSDBigTris:
    triangle1: int = r.readUInt16()
    triangle2: int = r.readUInt16()
    triangle3: int = r.readUInt16()
    material: int = r.readUInt32()                      # Always 0?
    weldingInfo: int = r.readUInt16()

# A set of transformation data: translation and rotation
class bhkCMSDTransform:
    translation: Vector4 = r.readVector4()              # A vector that moves the chunk by the specified amount. W is not used.
    rotation: Quaternion = r.readQuaternionWFirst()     # Rotation. Reference point for rotation is bhkRigidBody translation.

# Defines subshape chunks in bhkCompressedMeshShapeData
class bhkCMSDChunk:
    translation: Vector4 = r.readVector4()
    materialIndex: int = r.readUInt32()                 # Index of material in bhkCompressedMeshShapeData::Chunk Materials
    reference: int = r.readUInt16()                     # Always 65535?
    transformIndex: int = r.readUInt16()                # Index of transformation in bhkCompressedMeshShapeData::Chunk Transforms
    vertices: list[int] = r.readL32FArray(lambda r: r.readUInt16())
    indices: list[int] = r.readL32FArray(lambda r: r.readUInt16())
    strips: list[int] = r.readL32FArray(lambda r: r.readUInt16())
    weldingInfo: list[int] = r.readL32FArray(lambda r: r.readUInt16())

class MalleableDescriptor:
    type: hkConstraintType = hkConstraintType(r.readUInt32()) # Type of constraint.
    numEntities: int = r.readUInt32()                   # Always 2 (Hardcoded). Number of bodies affected by this constraint.
    entityA: int = X[bhkEntity].ptr(r)                  # Usually NONE. The entity affected by this constraint.
    entityB: int = X[bhkEntity].ptr(r)                  # Usually NONE. The entity affected by this constraint.
    priority: int = r.readUInt32()                      # Usually 1. Higher values indicate higher priority of this constraint?
    ballandSocket: BallAndSocketDescriptor = BallAndSocketDescriptor(r) if Type == 0 else None
    hinge: HingeDescriptor = HingeDescriptor(r, h) if Type == 1 else None
    limitedHinge: LimitedHingeDescriptor = LimitedHingeDescriptor(r, h) if Type == 2 else None
    prismatic: PrismaticDescriptor = PrismaticDescriptor(r, h) if Type == 6 else None
    ragdoll: RagdollDescriptor = RagdollDescriptor(r, h) if Type == 7 else None
    stiffSpring: StiffSpringDescriptor = StiffSpringDescriptor(r) if Type == 8 else None
    tau: float = r.readSingle() if h.v <= 0x14000005 else None
    damping: float = r.readSingle() if h.v <= 0x14000005 else None
    strength: float = r.readSingle() if h.v >= 0x14020007 else None

class ConstraintData:
    type: hkConstraintType = hkConstraintType(r.readUInt32()) # Type of constraint.
    numEntities2: int = r.readUInt32()                  # Always 2 (Hardcoded). Number of bodies affected by this constraint.
    entityA: int = X[bhkEntity].ptr(r)                  # Usually NONE. The entity affected by this constraint.
    entityB: int = X[bhkEntity].ptr(r)                  # Usually NONE. The entity affected by this constraint.
    priority: int = r.readUInt32()                      # Usually 1. Higher values indicate higher priority of this constraint?
    ballandSocket: BallAndSocketDescriptor = BallAndSocketDescriptor(r) if Type == 0 else None
    hinge: HingeDescriptor = HingeDescriptor(r, h) if Type == 1 else None
    limitedHinge: LimitedHingeDescriptor = LimitedHingeDescriptor(r, h) if Type == 2 else None
    prismatic: PrismaticDescriptor = PrismaticDescriptor(r, h) if Type == 6 else None
    ragdoll: RagdollDescriptor = RagdollDescriptor(r, h) if Type == 7 else None
    stiffSpring: StiffSpringDescriptor = StiffSpringDescriptor(r) if Type == 8 else None
    malleable: MalleableDescriptor = MalleableDescriptor(r, h) if Type == 13 else None

#endregion

#region NIF Objects

# Abstract object type.
class NiObject:

# Unknown.
class Ni3dsAlphaAnimator(NiObject):
    unknown1: list[int]                                 # Unknown.
    parent: int                                         # The parent?
    num1: int                                           # Unknown.
    num2: int                                           # Unknown.
    unknown2: list[list[int]]                           # Unknown.

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

# Unknown!
class Ni3dsColorAnimator(NiObject):
    unknown1: list[int]                                 # Unknown.

# Unknown!
class Ni3dsMorphShape(NiObject):
    unknown1: list[int]                                 # Unknown.

# Unknown!
class Ni3dsParticleSystem(NiObject):
    unknown1: list[int]                                 # Unknown.

# Unknown!
class Ni3dsPathController(NiObject):
    unknown1: list[int]                                 # Unknown.

# LEGACY (pre-10.1). Abstract base class for particle system modifiers.
class NiParticleModifier(NiObject):
    nextModifier: int                                   # Next particle modifier.
    controller: int                                     # Points to the particle system controller parent.

# Particle system collider.
class NiPSysCollider(NiObject):
    bounce: float                                       # Amount of bounce for the collider.
    spawnonCollide: bool                                # Spawn particles on impact?
    dieonCollide: bool                                  # Kill particles on impact?
    spawnModifier: int                                  # Spawner to use for the collider.
    parent: int                                         # Link to parent.
    nextCollider: int                                   # The next collider.
    colliderObject: int                                 # The object whose position and orientation are the basis of the collider.

class BroadPhaseType(Enum):
    BROAD_PHASE_INVALID = 0,
    BROAD_PHASE_ENTITY = 1,
    BROAD_PHASE_PHANTOM = 2,
    BROAD_PHASE_BORDER = 3

class hkWorldObjCinfoProperty:
    data: int = r.readUInt32()
    size: int = r.readUInt32()
    capacityandFlags: int = r.readUInt32()

# The base type of most Bethesda-specific Havok-related NIF objects.
class bhkRefObject(NiObject):

# Havok objects that can be saved and loaded from disk?
class bhkSerializable(bhkRefObject):

# Havok objects that have a position in the world?
class bhkWorldObject(bhkSerializable):
    shape: int                                          # Link to the body for this collision object.
    unknownInt: int
    havokFilter: HavokFilter
    unused: list[int]                                   # Garbage data from memory.
    broadPhaseType: BroadPhaseType
    unusedBytes: list[int]
    cinfoProperty: hkWorldObjCinfoProperty

# Havok object that do not react with other objects when they collide (causing deflection, etc.) but still trigger collision notifications to the game.  Possible uses are traps, portals, AI fields, etc.
class bhkPhantom(bhkWorldObject):

# A Havok phantom that uses a Havok shape object for its collision volume instead of just a bounding box.
class bhkShapePhantom(bhkPhantom):

# Unknown shape.
class bhkSimpleShapePhantom(bhkShapePhantom):
    unused2: list[int]                                  # Garbage data from memory.
    transform: Matrix4x4

# A havok node, describes physical properties.
class bhkEntity(bhkWorldObject):

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

# The "T" suffix marks this body as active for translation and rotation.
class bhkRigidBodyT(bhkRigidBody):

# Describes a physical constraint.
class bhkConstraint(bhkSerializable):
    entities: list[int]                                 # The entities affected by this constraint.
    priority: int                                       # Usually 1. Higher values indicate higher priority of this constraint?

# Hinge constraint.
class bhkLimitedHingeConstraint(bhkConstraint):
    limitedHinge: LimitedHingeDescriptor                # Describes a limited hinge constraint

# A malleable constraint.
class bhkMalleableConstraint(bhkConstraint):
    malleable: MalleableDescriptor                      # Constraint within constraint.

# A spring constraint.
class bhkStiffSpringConstraint(bhkConstraint):
    stiffSpring: StiffSpringDescriptor                  # Stiff Spring constraint.

# Ragdoll constraint.
class bhkRagdollConstraint(bhkConstraint):
    ragdoll: RagdollDescriptor                          # Ragdoll constraint.

# A prismatic constraint.
class bhkPrismaticConstraint(bhkConstraint):
    prismatic: PrismaticDescriptor                      # Describes a prismatic constraint

# A hinge constraint.
class bhkHingeConstraint(bhkConstraint):
    hinge: HingeDescriptor                              # Hinge constraing.

# A Ball and Socket Constraint.
class bhkBallAndSocketConstraint(bhkConstraint):
    ballandSocket: BallAndSocketDescriptor              # Describes a ball and socket constraint

# Two Vector4 for pivot in A and B.
class ConstraintInfo:
    pivotInA: Vector4 = r.readVector4()
    pivotInB: Vector4 = r.readVector4()

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

# A Havok Shape?
class bhkShape(bhkSerializable):

# Transforms a shape.
class bhkTransformShape(bhkShape):
    shape: int                                          # The shape that this object transforms.
    material: HavokMaterial                             # The material of the shape.
    radius: float
    unused: list[int]                                   # Garbage data from memory.
    transform: Matrix4x4                                # A transform matrix.

# A havok shape, perhaps with a bounding sphere for quick rejection in addition to more detailed shape data?
class bhkSphereRepShape(bhkShape):
    material: HavokMaterial                             # The material of the shape.
    radius: float                                       # The radius of the sphere that encloses the shape.

# A havok shape.
class bhkConvexShape(bhkSphereRepShape):

# A sphere.
class bhkSphereShape(bhkConvexShape):

# A capsule.
class bhkCapsuleShape(bhkConvexShape):
    unused: list[int]                                   # Not used. The following wants to be aligned at 16 bytes.
    firstPoint: Vector3                                 # First point on the capsule's axis.
    radius1: float                                      # Matches first capsule radius.
    secondPoint: Vector3                                # Second point on the capsule's axis.
    radius2: float                                      # Matches second capsule radius.

# A box.
class bhkBoxShape(bhkConvexShape):
    unused: list[int]                                   # Not used. The following wants to be aligned at 16 bytes.
    dimensions: Vector3                                 # A cube stored in Half Extents. A unit cube (1.0, 1.0, 1.0) would be stored as 0.5, 0.5, 0.5.
    unusedFloat: float                                  # Unused as Havok stores the Half Extents as hkVector4 with the W component unused.

# A convex shape built from vertices. Note that if the shape is used in
# a non-static object (such as clutter), then they will simply fall
# through ground when they are under a bhkListShape.
class bhkConvexVerticesShape(bhkConvexShape):
    verticesProperty: hkWorldObjCinfoProperty
    normalsProperty: hkWorldObjCinfoProperty
    vertices: list[Vector4]                             # Vertices. Fourth component is 0. Lexicographically sorted.
    normals: list[Vector4]                              # Half spaces as determined by the set of vertices above. First three components define the normal pointing to the exterior, fourth component is the signed distance of the separating plane to the origin: it is minus the dot product of v and n, where v is any vertex on the separating plane, and n is the normal. Lexicographically sorted.

# A convex transformed shape?
class bhkConvexTransformShape(bhkTransformShape):

class bhkConvexSweepShape(bhkShape):
    shape: int
    material: HavokMaterial
    radius: float
    unknown: Vector3

# Unknown.
class bhkMultiSphereShape(bhkSphereRepShape):
    unknownFloat1: float                                # Unknown.
    unknownFloat2: float                                # Unknown.
    spheres: list[NiBound]                              # This array holds the spheres which make up the multi sphere shape.

# A tree-like Havok data structure stored in an assembly-like binary code?
class bhkBvTreeShape(bhkShape):

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

# Havok collision object that uses multiple shapes?
class bhkShapeCollection(bhkShape):

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

class bhkMeshShape(bhkShape):
    unknowns: list[int]
    radius: float
    unused2: list[int]
    scale: Vector4
    shapeProperties: list[hkWorldObjCinfoProperty]
    unknown2: list[int]
    stripsData: list[int]                               # Refers to a bunch of NiTriStripsData objects that make up this shape.

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

# A shape constructed from a bunch of strips.
class bhkNiTriStripsShape(bhkShapeCollection):
    material: HavokMaterial                             # The material of the shape.
    radius: float
    unused: list[int]                                   # Garbage data from memory though the last 3 are referred to as maxSize, size, and eSize.
    growBy: int
    scale: Vector4                                      # Scale. Usually (1.0, 1.0, 1.0, 0.0).
    stripsData: list[int]                               # Refers to a bunch of NiTriStripsData objects that make up this shape.
    dataLayers: list[HavokFilter]                       # Havok Layers for each strip data.

# A generic extra data object.
class NiExtraData(NiObject):
    name: str                                           # Name of this object.
    nextExtraData: int                                  # Block number of the next extra data object.

# Abstract base class for all interpolators of bool, float, NiQuaternion, NiPoint3, NiColorA, and NiQuatTransform data.
class NiInterpolator(NiObject):

# Abstract base class for interpolators that use NiAnimationKeys (Key, KeyGrp) for interpolation.
class NiKeyBasedInterpolator(NiInterpolator):

# Uses NiFloatKeys to animate a float value over time.
class NiFloatInterpolator(NiKeyBasedInterpolator):
    value: float                                        # Pose value if lacking NiFloatData.
    data: int

# An interpolator for transform keyframes.
class NiTransformInterpolator(NiKeyBasedInterpolator):
    transform: NiQuatTransform
    data: int

# Uses NiPosKeys to animate an NiPoint3 value over time.
class NiPoint3Interpolator(NiKeyBasedInterpolator):
    value: Vector3                                      # Pose value if lacking NiPosData.
    data: int

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

# Uses NiBoolKeys to animate a bool value over time.
class NiBoolInterpolator(NiKeyBasedInterpolator):
    value: bool                                         # Pose value if lacking NiBoolData.
    data: int

# Uses NiBoolKeys to animate a bool value over time.
# Unlike NiBoolInterpolator, it ensures that keys have not been missed between two updates.
class NiBoolTimelineInterpolator(NiBoolInterpolator):

class InterpBlendFlags(Enum):
    MANAGER_CONTROLLED = 1          # MANAGER_CONTROLLED

# Interpolator item for array in NiBlendInterpolator.
class InterpBlendItem:
    interpolator: int = X[NiInterpolator].ref(r)        # Reference to an interpolator.
    weight: float = r.readSingle()
    normalizedWeight: float = r.readSingle()
    priority: int = r.readByte() if h.v >= 0x0A01006E else None
    easeSpinner: float = r.readSingle()

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

# Abstract base class for interpolators storing data via a B-spline.
class NiBSplineInterpolator(NiInterpolator):
    startTime: float                                    # Animation start time.
    stopTime: float                                     # Animation stop time.
    splineData: int
    basisData: int

# Abstract base class for NiObjects that support names, extra data, and time controllers.
class NiObjectNET(NiObject):
    skyrimShaderType: BSLightingShaderPropertyShaderType # Configures the main shader path
    name: str                                           # Name of this controllable object, used to refer to the object in .kf files.
    hasOldExtraData: bool                               # Extra data for pre-3.0 versions.
    oldExtraPropName: str                               # (=NiStringExtraData)
    oldExtraInternalId: int                             # ref
    oldExtraString: str                                 # Extra string data.
    unknownByte: int                                    # Always 0.
    extraData: int                                      # Extra data object index. (The first in a chain)
    extraDataList: list[int]                            # List of extra data indices.
    controller: int                                     # Controller object index. (The first in a chain)

# This is the most common collision object found in NIF files. It acts as a real object that
# is visible and possibly (if the body allows for it) interactive. The node itself
# is simple, it only has three properties.
# For this type of collision object, bhkRigidBody or bhkRigidBodyT is generally used.
class NiCollisionObject(NiObject):
    target: int                                         # Index of the AV object referring to this collision object.

# Collision box.
class NiCollisionData(NiCollisionObject):
    propagationMode: PropagationMode
    collisionMode: CollisionMode
    useAbv: int                                         # Use Alternate Bounding Volume.
    boundingVolume: BoundingVolume

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

# Havok related collision object?
class bhkCollisionObject(bhkNiCollisionObject):

# Unknown.
class bhkBlendCollisionObject(bhkCollisionObject):
    heirGain: float
    velGain: float
    unkFloat1: float
    unkFloat2: float

# Unknown.
class bhkPCollisionObject(bhkNiCollisionObject):

# Unknown.
class bhkSPCollisionObject(bhkPCollisionObject):

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

# Abstract base class for dynamic effects such as NiLights or projected texture effects.
class NiDynamicEffect(NiAVObject):
    switchState: bool                                   # If true, then the dynamic effect is applied to affected nodes during rendering.
    numAffectedNodes: int
    affectedNodes: list[int]                            # If a node appears in this list, then its entire subtree will be affected by the effect.
    affectedNodePointers: list[int]                     # As of 4.0 the pointer hash is no longer stored alongside each NiObject on disk, yet this node list still refers to the pointer hashes. Cannot leave the type as Ptr because the link will be invalid.

# Abstract base class that represents light sources in a scene graph.
# For Bethesda Stream 130 (FO4), NiLight now directly inherits from NiAVObject.
class NiLight(NiDynamicEffect):
    dimmer: float                                       # Scales the overall brightness of all light components.
    ambientColor: Color3
    diffuseColor: Color3
    specularColor: Color3

# Abstract base class representing all rendering properties. Subclasses are attached to NiAVObjects to control their rendering.
class NiProperty(NiObjectNET):

# Unknown
class NiTransparentProperty(NiProperty):
    unknown: list[int]                                  # Unknown.

# Abstract base class for all particle system modifiers.
class NiPSysModifier(NiObject):
    name: str                                           # Used to locate the modifier.
    order: int                                          # Modifier ID in the particle modifier chain (always a multiple of 1000)?
    target: int                                         # NiParticleSystem parent of this modifier.
    active: bool                                        # Whether or not the modifier is active.

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

# Abstract base class for particle emitters that emit particles from a volume.
class NiPSysVolumeEmitter(NiPSysEmitter):
    emitterObject: int                                  # Node parent of this modifier?

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

# Abstract base class for all NiTimeController objects using NiInterpolator objects to animate their target objects.
class NiInterpController(NiTimeController):
    managerControlled: bool

# DEPRECATED (20.6)
class NiMultiTargetTransformController(NiInterpController):
    extraTargets: list[int]                             # NiNode Targets to be controlled.

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

# Unknown! Used by Daoc->'healing.nif'.
class NiMorphController(NiInterpController):

# Unknown! Used by Daoc.
class NiMorpherController(NiInterpController):
    data: int                                           # This controller's data.

# Uses a single NiInterpolator to animate its target value.
class NiSingleInterpController(NiInterpController):
    interpolator: int

# DEPRECATED (10.2), RENAMED (10.2) to NiTransformController
# A time controller object for animation key frames.
class NiKeyframeController(NiSingleInterpController):
    data: int

# NiTransformController replaces NiKeyframeController.
class NiTransformController(NiKeyframeController):

# A particle system modifier controller.
# NiInterpController::GetCtlrID() string format:
#     '%s'
# Where %s = Value of "Modifier Name"
class NiPSysModifierCtlr(NiSingleInterpController):
    modifierName: str                                   # Used to find the modifier pointer.

# Particle system emitter controller.
# NiInterpController::GetInterpolatorID() string format:
#     ['BirthRate', 'EmitterActive'] (for "Interpolator" and "Visibility Interpolator" respectively)
class NiPSysEmitterCtlr(NiPSysModifierCtlr):
    visibilityInterpolator: int
    data: int

# A particle system modifier controller that animates a boolean value for particles.
class NiPSysModifierBoolCtlr(NiPSysModifierCtlr):

# A particle system modifier controller that animates active/inactive state for particles.
class NiPSysModifierActiveCtlr(NiPSysModifierBoolCtlr):
    data: int

# A particle system modifier controller that animates a floating point value for particles.
class NiPSysModifierFloatCtlr(NiPSysModifierCtlr):
    data: int

# Animates the declination value on an NiPSysEmitter object.
class NiPSysEmitterDeclinationCtlr(NiPSysModifierFloatCtlr):

# Animates the declination variation value on an NiPSysEmitter object.
class NiPSysEmitterDeclinationVarCtlr(NiPSysModifierFloatCtlr):

# Animates the size value on an NiPSysEmitter object.
class NiPSysEmitterInitialRadiusCtlr(NiPSysModifierFloatCtlr):

# Animates the lifespan value on an NiPSysEmitter object.
class NiPSysEmitterLifeSpanCtlr(NiPSysModifierFloatCtlr):

# Animates the speed value on an NiPSysEmitter object.
class NiPSysEmitterSpeedCtlr(NiPSysModifierFloatCtlr):

# Animates the strength value of an NiPSysGravityModifier.
class NiPSysGravityStrengthCtlr(NiPSysModifierFloatCtlr):

# Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target float value.
class NiFloatInterpController(NiSingleInterpController):

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

# Animates the alpha value of a property using an interpolator.
class NiAlphaController(NiFloatInterpController):
    data: int

# Used to animate a single member of an NiTextureTransform.
# NiInterpController::GetCtlrID() string formats:
#     ['%1-%2-TT_TRANSLATE_U', '%1-%2-TT_TRANSLATE_V', '%1-%2-TT_ROTATE', '%1-%2-TT_SCALE_U', '%1-%2-TT_SCALE_V']
# (Depending on "Operation" enumeration, %1 = Value of "Shader Map", %2 = Value of "Texture Slot")
class NiTextureTransformController(NiFloatInterpController):
    shaderMap: bool                                     # Is the target map a shader map?
    textureSlot: TexType                                # The target texture slot.
    operation: TransformMember                          # Controls which aspect of the texture transform to modify.
    data: int

# Unknown controller.
class NiLightDimmerController(NiFloatInterpController):

# Abstract base class for all NiInterpControllers that use a NiInterpolator to animate their target boolean value.
class NiBoolInterpController(NiSingleInterpController):

# Animates the visibility of an NiAVObject.
class NiVisController(NiBoolInterpController):
    data: int

# Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target NiPoint3 value.
class NiPoint3InterpController(NiSingleInterpController):

# Time controller for material color. Flags are used for color selection in versions below 10.1.0.0.
# Bits 4-5: Target Color (00 = Ambient, 01 = Diffuse, 10 = Specular, 11 = Emissive)
# NiInterpController::GetCtlrID() string formats:
#     ['AMB', 'DIFF', 'SPEC', 'SELF_ILLUM'] (Depending on "Target Color")
class NiMaterialColorController(NiPoint3InterpController):
    targetColor: MaterialColor                          # Selects which color to control.
    data: int

# Animates the ambient, diffuse and specular colors of an NiLight.
# NiInterpController::GetCtlrID() string formats:
#     ['Diffuse', 'Ambient'] (Depending on "Target Color")
class NiLightColorController(NiPoint3InterpController):
    targetColor: LightColor
    data: int

# Abstract base class for all extra data controllers.
# NiInterpController::GetCtlrID() string format:
#     '%s'
# Where %s = Value of "Extra Data Name"
class NiExtraDataController(NiSingleInterpController):
    extraDataName: str

# Animates an NiFloatExtraData object attached to an NiAVObject.
# NiInterpController::GetCtlrID() string format is same as parent.
class NiFloatExtraDataController(NiExtraDataController):
    numExtraBytes: int                                  # Number of extra bytes.
    unknownBytes: list[int]                             # Unknown.
    unknownExtraBytes: list[int]                        # Unknown.
    data: int

# Animates an NiFloatsExtraData object attached to an NiAVObject.
# NiInterpController::GetCtlrID() string format:
#     '%s[%d]'
# Where %s = Value of "Extra Data Name", %d = Value of "Floats Extra Data Index"
class NiFloatsExtraDataController(NiExtraDataController):
    floatsExtraDataIndex: int
    data: int

# Animates an NiFloatsExtraData object attached to an NiAVObject.
# NiInterpController::GetCtlrID() string format:
#     '%s[%d]'
# Where %s = Value of "Extra Data Name", %d = Value of "Floats Extra Data Index"
class NiFloatsExtraDataPoint3Controller(NiExtraDataController):
    floatsExtraDataIndex: int

# DEPRECATED (20.5), Replaced by NiSkinningLODController.
# Level of detail controller for bones.  Priority is arranged from low to high.
class NiBoneLODController(NiTimeController):
    lod: int                                            # Unknown.
    numLoDs: int                                        # Number of LODs.
    numNodeGroups: int                                  # Number of node arrays.
    nodeGroups: list[??]                                # A list of node sets (each set a sequence of bones).
    numShapeGroups: int                                 # Number of shape groups.
    shapeGroups1: list[SkinInfoSet]                     # List of shape groups.
    numShapeGroups2: int                                # The size of the second list of shape groups.
    shapeGroups2: list[int]                             # Group of NiTriShape indices.
    unknownInt2: int                                    # Unknown.
    unknownInt3: int                                    # Unknown.

# A simple LOD controller for bones.
class NiBSBoneLODController(NiBoneLODController):

class MaterialData:
    hasShader: bool = r.readBool32() if h.v >= 0x0A000100 and h.v <= 0x14010003 else None # Shader.
    shaderName: str = Y.string(r) if Has Shader and h.v >= 0x0A000100 and h.v <= 0x14010003 else None # The shader name.
    shaderExtraData: int = r.readInt32() if Has Shader and h.v >= 0x0A000100 and h.v <= 0x14010003 else None # Extra data associated with the shader. A value of -1 means the shader is the default implementation.
    numMaterials: int = r.readUInt32() if h.v >= 0x14020005 else None
    materialName: list[str] = Y.string(r) if h.v >= 0x14020005 else None # The name of the material.
    materialExtraData: list[int] = r.readInt32() if h.v >= 0x14020005 else None # Extra data associated with the material. A value of -1 means the material is the default implementation.
    activeMaterial: int = r.readInt32() if h.v >= 0x14020005 else None # The index of the currently active material.
    unknownByte: int = r.readByte() if h.v >= 0x0A020000 and h.v <= 0x0A020000 and (h.userVersion == 1) else None # Cyanide extension (only in version 10.2.0.0?).
    unknownInteger2: int = r.readInt32() if h.v >= 0x0A040001 and h.v <= 0x0A040001 else None # Unknown.
    materialNeedsUpdate: bool = r.readBool32() if h.v >= 0x14020007 else None # Whether the materials for this object always needs to be updated before rendering with them.

# Describes a visible scene element with vertices like a mesh, a particle system, lines, etc.
class NiGeometry(NiAVObject):
    bound: NiBound
    skin: int
    data: int                                           # Data index (NiTriShapeData/NiTriStripData).
    skinInstance: int
    materialData: MaterialData
    shaderProperty: int
    alphaProperty: int

# Describes a mesh, built from triangles.
class NiTriBasedGeom(NiGeometry):

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

class AbstractAdditionalGeometryData(NiObject):

# Describes a mesh, built from triangles.
class NiTriBasedGeomData(NiGeometryData):
    numTriangles: int                                   # Number of triangles.

# Unknown. Is apparently only used in skeleton.nif files.
class bhkBlendController(NiTimeController):
    keys: int                                           # Seems to be always zero.

# Bethesda-specific collision bounding box for skeletons.
class BSBound(NiExtraData):
    center: Vector3                                     # Center of the bounding box.
    dimensions: Vector3                                 # Dimensions of the bounding box from center.

# Unknown. Marks furniture sitting positions?
class BSFurnitureMarker(NiExtraData):
    positions: list[FurniturePosition]

# Particle modifier that adds a blend of object space translation and rotation to particles born in world space.
class BSParentVelocityModifier(NiPSysModifier):
    damping: float                                      # Amount of blending?

# Particle emitter that uses a node, its children and subchildren to emit from.  Emission will be evenly spread along points from nodes leading to their direct parents/children only.
class BSPSysArrayEmitter(NiPSysVolumeEmitter):

# Particle Modifier that uses the wind value from the gamedata to alter the path of particles.
class BSWindModifier(NiPSysModifier):
    strength: float                                     # The amount of force wind will have on particles.

# NiTriStripsData for havok data?
class hkPackedNiTriStripsData(bhkShapeCollection):
    triangles: list[TriangleData]
    numVertices: int
    unknownByte1: int                                   # Unknown.
    vertices: list[Vector3]
    subShapes: list[OblivionSubShape]                   # The subparts.

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

# Ambient light source.
class NiAmbientLight(NiLight):

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
    subtextureOffsets: list[Vector4]                    # Defines UV offsets
    aspectRatio: float                                  # Sets aspect ratio for Subtexture Offset UV quads
    aspectFlags: int
    speedtoAspectAspect2: float
    speedtoAspectSpeed1: float
    speedtoAspectSpeed2: float

# Rotating particles data object.
class NiRotatingParticlesData(NiParticlesData):
    hasRotations2: bool                                 # Is the particle rotation array present?
    rotations2: list[Quaternion]                        # The individual particle rotations.

# Particle system data object (with automatic normals?).
class NiAutoNormalParticlesData(NiParticlesData):

# Particle Description.
class ParticleDesc:
    translation: Vector3 = r.readVector3()              # Unknown.
    unknownFloats1: list[float] = r.readSingle() if h.v <= 0x0A040001 else None # Unknown.
    unknownFloat1: float = r.readSingle()               # Unknown.
    unknownFloat2: float = r.readSingle()               # Unknown.
    unknownFloat3: float = r.readSingle()               # Unknown.
    unknownInt1: int = r.readInt32()                    # Unknown.

# Particle system data.
class NiPSysData(NiParticlesData):
    particleDescriptions: list[ParticleDesc]
    hasRotationSpeeds: bool
    rotationSpeeds: list[float]
    numAddedParticles: int
    addedParticlesBase: int

# Particle meshes data.
class NiMeshPSysData(NiPSysData):
    defaultPoolSize: int
    fillPoolsOnLoad: bool
    generations: list[int]
    particleMeshes: int

# Binary extra data object. Used to store tangents and bitangents in Oblivion.
class NiBinaryExtraData(NiExtraData):
    binaryData: bytearray                               # The binary data.

# Voxel extra data object.
class NiBinaryVoxelExtraData(NiExtraData):
    unknownInt: int                                     # Unknown.  0?
    data: int                                           # Link to binary voxel data.

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

# Blends bool values together.
class NiBlendBoolInterpolator(NiBlendInterpolator):
    value: int                                          # The pose value. Invalid if using data.

# Blends float values together.
class NiBlendFloatInterpolator(NiBlendInterpolator):
    value: float                                        # The pose value. Invalid if using data.

# Blends NiPoint3 values together.
class NiBlendPoint3Interpolator(NiBlendInterpolator):
    value: Vector3                                      # The pose value. Invalid if using data.

# Blends NiQuatTransform values together.
class NiBlendTransformInterpolator(NiBlendInterpolator):
    value: NiQuatTransform

# Wrapper for boolean animation keys.
class NiBoolData(NiObject):
    data: KeyGroup                                      # The boolean keys.

# Boolean extra data.
class NiBooleanExtraData(NiExtraData):
    booleanData: int                                    # The boolean extra data value.

# Contains an NiBSplineBasis for use in interpolation of open, uniform B-Splines.
class NiBSplineBasisData(NiObject):
    numControlPoints: int                               # The number of control points of the B-spline (number of frames of animation plus degree of B-spline minus one).

# Uses B-Splines to animate a float value over time.
class NiBSplineFloatInterpolator(NiBSplineInterpolator):
    value: float                                        # Base value when curve not defined.
    handle: int                                         # Handle into the data. (USHRT_MAX for invalid handle.)

# NiBSplineFloatInterpolator plus the information required for using compact control points.
class NiBSplineCompFloatInterpolator(NiBSplineFloatInterpolator):
    floatOffset: float
    floatHalfRange: float

# Uses B-Splines to animate an NiPoint3 value over time.
class NiBSplinePoint3Interpolator(NiBSplineInterpolator):
    value: Vector3                                      # Base value when curve not defined.
    handle: int                                         # Handle into the data. (USHRT_MAX for invalid handle.)

# NiBSplinePoint3Interpolator plus the information required for using compact control points.
class NiBSplineCompPoint3Interpolator(NiBSplinePoint3Interpolator):
    positionOffset: float
    positionHalfRange: float

# Supports the animation of position, rotation, and scale using an NiQuatTransform.
# The NiQuatTransform can be an unchanging pose or interpolated from B-Spline control point channels.
class NiBSplineTransformInterpolator(NiBSplineInterpolator):
    transform: NiQuatTransform
    translationHandle: int                              # Handle into the translation data. (USHRT_MAX for invalid handle.)
    rotationHandle: int                                 # Handle into the rotation data. (USHRT_MAX for invalid handle.)
    scaleHandle: int                                    # Handle into the scale data. (USHRT_MAX for invalid handle.)

# NiBSplineTransformInterpolator plus the information required for using compact control points.
class NiBSplineCompTransformInterpolator(NiBSplineTransformInterpolator):
    translationOffset: float
    translationHalfRange: float
    rotationOffset: float
    rotationHalfRange: float
    scaleOffset: float
    scaleHalfRange: float

class BSRotAccumTransfInterpolator(NiTransformInterpolator):

# Contains one or more sets of control points for use in interpolation of open, uniform B-Splines, stored as either float or compact.
class NiBSplineData(NiObject):
    floatControlPoints: list[float]                     # Float values representing the control data.
    compactControlPoints: list[int]                     # Signed shorts representing the data from 0 to 1 (scaled by SHRT_MAX).

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

# Wrapper for color animation keys.
class NiColorData(NiObject):
    data: KeyGroup                                      # The color keys.

# Extra data in the form of NiColorA (red, green, blue, alpha).
class NiColorExtraData(NiExtraData):
    data: Color4                                        # RGBA Color?

# Controls animation sequences on a specific branch of the scene graph.
class NiControllerManager(NiTimeController):
    cumulative: bool                                    # Whether transformation accumulation is enabled. If accumulation is not enabled, the manager will treat all sequence data on the accumulation root as absolute data instead of relative delta values.
    controllerSequences: list[int]
    objectPalette: int

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

# Abstract base class for indexing NiAVObject by name.
class NiAVObjectPalette(NiObject):

# NiAVObjectPalette implementation. Used to quickly look up objects by name.
class NiDefaultAVObjectPalette(NiAVObjectPalette):
    scene: int                                          # Scene root of the object palette.
    objs: list[AVObject]                                # The objects.

# Directional light source.
class NiDirectionalLight(NiLight):

# NiDitherProperty allows the application to turn the dithering of interpolated colors and fog values on and off.
class NiDitherProperty(NiProperty):
    flags: Flags                                        # 1 = Enable dithering

# DEPRECATED (10.2), REMOVED (20.5). Replaced by NiTransformController and NiLookAtInterpolator.
class NiRollController(NiSingleInterpController):
    data: int                                           # The data for the controller.

# Wrapper for 1D (one-dimensional) floating point animation keys.
class NiFloatData(NiObject):
    data: KeyGroup                                      # The keys.

# Extra float data.
class NiFloatExtraData(NiExtraData):
    floatData: float                                    # The float data.

# Extra float array data.
class NiFloatsExtraData(NiExtraData):
    data: list[float]                                   # Float data.

# NiFogProperty allows the application to enable, disable and control the appearance of fog.
class NiFogProperty(NiProperty):
    flags: Flags                                        # Bit 0: Enables Fog
                                                        #     Bit 1: Sets Fog Function to FOG_RANGE_SQ
                                                        #     Bit 2: Sets Fog Function to FOG_VERTEX_ALPHA
                                                        # 
                                                        #     If Bit 1 and Bit 2 are not set, but fog is enabled, Fog function is FOG_Z_LINEAR.
    fogDepth: float                                     # Depth of the fog in normalized units. 1.0 = begins at near plane. 0.5 = begins halfway between the near and far planes.
    fogColor: Color3                                    # The color of the fog.

# LEGACY (pre-10.1) particle modifier. Applies a gravitational field on the particles.
class NiGravity(NiParticleModifier):
    unknownFloat1: float                                # Unknown.
    force: float                                        # The strength/force of this gravity.
    type: FieldType                                     # The force field type.
    position: Vector3                                   # The position of the mass point relative to the particle system.
    direction: Vector3                                  # The direction of the applied acceleration.

# Extra integer data.
class NiIntegerExtraData(NiExtraData):
    integerData: int                                    # The value of the extra data.

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

# Extra integer array data.
class NiIntegersExtraData(NiExtraData):
    data: list[int]                                     # Integers.

# An extended keyframe controller.
class BSKeyframeController(NiKeyframeController):
    data2: int                                          # A link to more keyframe data.

# DEPRECATED (10.2), RENAMED (10.2) to NiTransformData.
# Wrapper for transformation animation keys.
class NiKeyframeData(NiObject):
    numRotationKeys: int                                # The number of quaternion rotation keys. If the rotation type is XYZ (type 4) then this *must* be set to 1, and in this case the actual number of keys is stored in the XYZ Rotations field.
    rotationType: KeyType                               # The type of interpolation to use for rotation.  Can also be 4 to indicate that separate X, Y, and Z values are used for the rotation instead of Quaternions.
    quaternionKeys: list[QuatKey]                       # The rotation keys if Quaternion rotation is used.
    order: float
    xyzRotations: list[KeyGroup]                        # Individual arrays of keys for rotating X, Y, and Z individually.
    translations: KeyGroup                              # Translation keys.
    scales: KeyGroup                                    # Scale keys.

class LookAtFlags(Flag):
    LOOK_FLIP = 0,                  # Flip
    LOOK_Y_AXIS = 1 << 1,           # Y-Axis
    LOOK_Z_AXIS = 1 << 2            # Z-Axis

# DEPRECATED (10.2), REMOVED (20.5)
# Replaced by NiTransformController and NiLookAtInterpolator.
class NiLookAtController(NiTimeController):
    flags: LookAtFlags
    lookAt: int

# NiLookAtInterpolator rotates an object so that it always faces a target object.
class NiLookAtInterpolator(NiInterpolator):
    flags: LookAtFlags
    lookAt: int
    lookAtName: str
    transform: NiQuatTransform
    interpolatorTranslation: int
    interpolatorRoll: int
    interpolatorScale: int

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

# DEPRECATED (20.5), replaced by NiMorphMeshModifier.
# Geometry morphing data.
class NiMorphData(NiObject):
    numMorphs: int                                      # Number of morphing object.
    numVertices: int                                    # Number of vertices.
    relativeTargets: int                                # This byte is always 1 in all official files.
    morphs: list[Morph]                                 # The geometry morphing objects.

# Generic node object for grouping.
class NiNode(NiAVObject):
    children: list[int]                                 # List of child node object indices.
    effects: list[int]                                  # List of node effects. ADynamicEffect?

# A NiNode used as a skeleton bone?
class NiBone(NiNode):

# Morrowind specific.
class AvoidNode(NiNode):

# Firaxis-specific UI widgets?
class FxWidget(NiNode):
    unknown3: int                                       # Unknown.
    unknown292Bytes: list[int]                          # Looks like 9 links and some string data.

# Unknown.
class FxButton(FxWidget):

# Unknown.
class FxRadioButton(FxWidget):
    unknownInt1: int                                    # Unknown.
    unknownInt2: int                                    # Unknown.
    unknownInt3: int                                    # Unknown.
    buttons: list[int]                                  # Unknown pointers to other buttons.  Maybe other buttons in a group so they can be switch off if this one is switched on?

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

# Bethesda-specific extension of Node with animation properties stored in the flags, often 42?
class NiBSAnimationNode(NiNode):

# Unknown.
class NiBSParticleNode(NiNode):

# Flags for NiSwitchNode.
class NiSwitchFlags(Flag):
    UpdateOnlyActiveChild = 0,      # Update Only Active Child
    UpdateControllers = 1 << 1      # Update Controllers

# Represents groups of multiple scenegraph subtrees, only one of which (the "active child") is drawn at any given time.
class NiSwitchNode(NiNode):
    switchNodeFlags: NiSwitchFlags
    index: int

# Level of detail selector. Links to different levels of detail of the same model, used to switch a geometry at a specified distance.
class NiLODNode(NiSwitchNode):
    lodCenter: Vector3
    lodLevels: list[LODRange]
    lodLevelData: int

# NiPalette objects represent mappings from 8-bit indices to 24-bit RGB or 32-bit RGBA colors.
class NiPalette(NiObject):
    hasAlpha: int
    numEntries: int                                     # The number of palette entries. Always 256 but can also be 16.
    palette: list[ByteColor4]                           # The color palette.

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

# LEGACY (pre-10.1) particle modifier.
class NiParticleColorModifier(NiParticleModifier):
    colorData: int

# LEGACY (pre-10.1) particle modifier.
class NiParticleGrowFade(NiParticleModifier):
    grow: float                                         # The time from the beginning of the particle lifetime during which the particle grows.
    fade: float                                         # The time from the end of the particle lifetime during which the particle fades.

# LEGACY (pre-10.1) particle modifier.
class NiParticleMeshModifier(NiParticleModifier):
    particleMeshes: list[int]

# LEGACY (pre-10.1) particle modifier.
class NiParticleRotation(NiParticleModifier):
    randomInitialAxis: int
    initialAxis: Vector3
    rotationSpeed: float

# Generic particle system node.
class NiParticles(NiGeometry):
    vertexDesc: BSVertexDesc

# LEGACY (pre-10.1). NiParticles which do not house normals and generate them at runtime.
class NiAutoNormalParticles(NiParticles):

# LEGACY (pre-10.1). Particle meshes.
class NiParticleMeshes(NiParticles):

# LEGACY (pre-10.1). Particle meshes data.
class NiParticleMeshesData(NiRotatingParticlesData):
    unknownLink2: int                                   # Refers to the mesh that makes up a particle?

# A particle system.
class NiParticleSystem(NiParticles):
    farBegin: int
    farEnd: int
    nearBegin: int
    nearEnd: int
    data: int
    worldSpace: bool                                    # If true, Particles are birthed into world space.  If false, Particles are birthed into object space.
    modifiers: list[int]                                # The list of particle modifiers.

# Particle system.
class NiMeshParticleSystem(NiParticleSystem):

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

# A particle system controller, used by BS in conjunction with NiBSParticleNode.
class NiBSPArrayController(NiParticleSystemController):

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

class PixelFormatComponent:
    type: PixelComponent = PixelComponent(r.readUInt32()) # Component Type
    convention: PixelRepresentation = PixelRepresentation(r.readUInt32()) # Data Storage Convention
    bitsPerChannel: int = r.readByte()                  # Bits per component
    isSigned: bool = r.readBool32()

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

class NiPersistentSrcTextureRendererData(NiPixelFormat):
    palette: int
    numMipmaps: int
    bytesPerPixel: int
    mipmaps: list[??]
    numPixels: int
    padNumPixels: int
    numFaces: int
    platform: PlatformID
    renderer: RendererID
    pixelData: list[int]

# A texture.
class NiPixelData(NiPixelFormat):
    palette: int
    numMipmaps: int
    bytesPerPixel: int
    mipmaps: list[??]
    numPixels: int
    numFaces: int
    pixelData: list[int]

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

# A point light.
class NiPointLight(NiLight):
    constantAttenuation: float
    linearAttenuation: float
    quadraticAttenuation: float

# Abstract base class for dynamic effects such as NiLights or projected texture effects.
class NiDeferredDynamicEffect(NiAVObject):
    switchState: bool                                   # If true, then the dynamic effect is applied to affected nodes during rendering.

# Abstract base class that represents light sources in a scene graph.
# For Bethesda Stream 130 (FO4), NiLight now directly inherits from NiAVObject.
class NiDeferredLight(NiDeferredDynamicEffect):
    dimmer: float                                       # Scales the overall brightness of all light components.
    ambientColor: Color3
    diffuseColor: Color3
    specularColor: Color3

# A deferred point light. Custom (Twin Saga).
class NiDeferredPointLight(NiDeferredLight):
    constantAttenuation: float
    linearAttenuation: float
    quadraticAttenuation: float

# Wrapper for position animation keys.
class NiPosData(NiObject):
    data: KeyGroup

# Wrapper for rotation animation keys.
class NiRotData(NiObject):
    numRotationKeys: int
    rotationType: KeyType
    quaternionKeys: list[QuatKey]
    xyzRotations: list[KeyGroup]

# Particle modifier that controls and updates the age of particles in the system.
class NiPSysAgeDeathModifier(NiPSysModifier):
    spawnonDeath: bool                                  # Should the particles spawn on death?
    spawnModifier: int                                  # The spawner to use on death.

# Particle modifier that applies an explosive force to particles.
class NiPSysBombModifier(NiPSysModifier):
    bombObject: int                                     # The object whose position and orientation are the basis of the force.
    bombAxis: Vector3                                   # The local direction of the force.
    decay: float                                        # How the bomb force will decrease with distance.
    deltaV: float                                       # The acceleration the bomb will apply to particles.
    decayType: DecayType
    symmetryType: SymmetryType

# Particle modifier that creates and updates bound volumes.
class NiPSysBoundUpdateModifier(NiPSysModifier):
    updateSkip: int                                     # Optimize by only computing the bound of (1 / Update Skip) of the total particles each frame.

# Particle emitter that uses points within a defined Box shape to emit from.
class NiPSysBoxEmitter(NiPSysVolumeEmitter):
    width: float
    height: float
    depth: float

# Particle modifier that adds a defined shape to act as a collision object for particles to interact with.
class NiPSysColliderManager(NiPSysModifier):
    collider: int

# Particle modifier that adds keyframe data to modify color/alpha values of particles over time.
class NiPSysColorModifier(NiPSysModifier):
    data: int

# Particle emitter that uses points within a defined Cylinder shape to emit from.
class NiPSysCylinderEmitter(NiPSysVolumeEmitter):
    radius: float
    height: float

# Particle modifier that applies a linear drag force to particles.
class NiPSysDragModifier(NiPSysModifier):
    dragObject: int                                     # The object whose position and orientation are the basis of the force.
    dragAxis: Vector3                                   # The local direction of the force.
    percentage: float                                   # The amount of drag to apply to particles.
    range: float                                        # The distance up to which particles are fully affected.
    rangeFalloff: float                                 # The distance at which particles cease to be affected.

# DEPRECATED (10.2). Particle system emitter controller data.
class NiPSysEmitterCtlrData(NiObject):
    birthRateKeys: KeyGroup
    activeKeys: list[Key]

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

# Particle modifier that controls the time it takes to grow and shrink a particle.
class NiPSysGrowFadeModifier(NiPSysModifier):
    growTime: float                                     # The time taken to grow from 0 to their specified size.
    growGeneration: int                                 # Specifies the particle generation to which the grow effect should be applied. This is usually generation 0, so that newly created particles will grow.
    fadeTime: float                                     # The time taken to shrink from their specified size to 0.
    fadeGeneration: int                                 # Specifies the particle generation to which the shrink effect should be applied. This is usually the highest supported generation for the particle system.
    baseScale: float                                    # A multiplier on the base particle scale.

# Particle emitter that uses points on a specified mesh to emit from.
class NiPSysMeshEmitter(NiPSysEmitter):
    emitterMeshes: list[int]                            # The meshes which are emitted from.
    initialVelocityType: VelocityType                   # The method by which the initial particle velocity will be computed.
    emissionType: EmitFrom                              # The manner in which particles are emitted from the Emitter Meshes.
    emissionAxis: Vector3                               # The emission axis if VELOCITY_USE_DIRECTION.

# Particle modifier that updates mesh particles using the age of each particle.
class NiPSysMeshUpdateModifier(NiPSysModifier):
    meshes: list[int]

class BSPSysInheritVelocityModifier(NiPSysModifier):
    target: int
    chanceToInherit: float
    velocityMultiplier: float
    velocityVariation: float

class BSPSysHavokUpdateModifier(NiPSysModifier):
    nodes: list[int]
    modifier: int

class BSPSysRecycleBoundModifier(NiPSysModifier):
    boundOffset: Vector3
    boundExtent: Vector3
    target: int

# Similar to a Flip Controller, this handles particle texture animation on a single texture atlas
class BSPSysSubTexModifier(NiPSysModifier):
    startFrame: int                                     # Starting frame/position on atlas
    startFrameFudge: float                              # Random chance to start on a different frame?
    endFrame: float                                     # Ending frame/position on atlas
    loopStartFrame: float                               # Frame to start looping
    loopStartFrameFudge: float
    frameCount: float
    frameCountFudge: float

# Particle Collider object which particles will interact with.
class NiPSysPlanarCollider(NiPSysCollider):
    width: float                                        # Width of the plane along the X Axis.
    height: float                                       # Height of the plane along the Y Axis.
    xAxis: Vector3                                      # Axis defining a plane, relative to Collider Object.
    yAxis: Vector3                                      # Axis defining a plane, relative to Collider Object.

# Particle Collider object which particles will interact with.
class NiPSysSphericalCollider(NiPSysCollider):
    radius: float

# Particle modifier that updates the particle positions based on velocity and last update time.
class NiPSysPositionModifier(NiPSysModifier):

# Particle modifier that calls reset on a target upon looping.
class NiPSysResetOnLoopCtlr(NiTimeController):

# Particle modifier that adds rotations to particles.
class NiPSysRotationModifier(NiPSysModifier):
    rotationSpeed: float                                # Initial Rotation Speed in radians per second.
    rotationSpeedVariation: float                       # Distributes rotation speed over the range [Speed - Variation, Speed + Variation].
    rotationAngle: float                                # Initial Rotation Angle in radians.
    rotationAngleVariation: float                       # Distributes rotation angle over the range [Angle - Variation, Angle + Variation].
    randomRotSpeedSign: bool                            # Randomly negate the initial rotation speed?
    randomAxis: bool                                    # Assign a random axis to new particles?
    axis: Vector3                                       # Initial rotation axis.

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

# Particle emitter that uses points within a sphere shape to emit from.
class NiPSysSphereEmitter(NiPSysVolumeEmitter):
    radius: float

# Particle system controller, tells the system to update its simulation.
class NiPSysUpdateCtlr(NiTimeController):

# Base for all force field particle modifiers.
class NiPSysFieldModifier(NiPSysModifier):
    fieldObject: int                                    # The object whose position and orientation are the basis of the field.
    magnitude: float                                    # Magnitude of the force.
    attenuation: float                                  # How the magnitude diminishes with distance from the Field Object.
    useMaxDistance: bool                                # Whether or not to use a distance from the Field Object after which there is no effect.
    maxDistance: float                                  # Maximum distance after which there is no effect.

# Particle system modifier, implements a vortex field force for particles.
class NiPSysVortexFieldModifier(NiPSysFieldModifier):
    direction: Vector3                                  # Direction of the vortex field in Field Object's space.

# Particle system modifier, implements a gravity field force for particles.
class NiPSysGravityFieldModifier(NiPSysFieldModifier):
    direction: Vector3                                  # Direction of the gravity field in Field Object's space.

# Particle system modifier, implements a drag field force for particles.
class NiPSysDragFieldModifier(NiPSysFieldModifier):
    useDirection: bool                                  # Whether or not the drag force applies only in the direction specified.
    direction: Vector3                                  # Direction in which the force applies if Use Direction is true.

# Particle system modifier, implements a turbulence field force for particles.
class NiPSysTurbulenceFieldModifier(NiPSysFieldModifier):
    frequency: float                                    # How many turbulence updates per second.

class BSPSysLODModifier(NiPSysModifier):
    lodBeginDistance: float
    lodEndDistance: float
    endEmitScale: float
    endSize: float

class BSPSysScaleModifier(NiPSysModifier):
    scales: list[float]

# Particle system controller for force field magnitude.
class NiPSysFieldMagnitudeCtlr(NiPSysModifierFloatCtlr):

# Particle system controller for force field attenuation.
class NiPSysFieldAttenuationCtlr(NiPSysModifierFloatCtlr):

# Particle system controller for force field maximum distance.
class NiPSysFieldMaxDistanceCtlr(NiPSysModifierFloatCtlr):

# Particle system controller for air field air friction.
class NiPSysAirFieldAirFrictionCtlr(NiPSysModifierFloatCtlr):

# Particle system controller for air field inherit velocity.
class NiPSysAirFieldInheritVelocityCtlr(NiPSysModifierFloatCtlr):

# Particle system controller for air field spread.
class NiPSysAirFieldSpreadCtlr(NiPSysModifierFloatCtlr):

# Particle system controller for emitter initial rotation speed.
class NiPSysInitialRotSpeedCtlr(NiPSysModifierFloatCtlr):

# Particle system controller for emitter initial rotation speed variation.
class NiPSysInitialRotSpeedVarCtlr(NiPSysModifierFloatCtlr):

# Particle system controller for emitter initial rotation angle.
class NiPSysInitialRotAngleCtlr(NiPSysModifierFloatCtlr):

# Particle system controller for emitter initial rotation angle variation.
class NiPSysInitialRotAngleVarCtlr(NiPSysModifierFloatCtlr):

# Particle system controller for emitter planar angle.
class NiPSysEmitterPlanarAngleCtlr(NiPSysModifierFloatCtlr):

# Particle system controller for emitter planar angle variation.
class NiPSysEmitterPlanarAngleVarCtlr(NiPSysModifierFloatCtlr):

# Particle system modifier, updates the particle velocity to simulate the effects of air movements like wind, fans, or wake.
class NiPSysAirFieldModifier(NiPSysFieldModifier):
    direction: Vector3                                  # Direction of the particle velocity
    airFriction: float                                  # How quickly particles will accelerate to the magnitude of the air field.
    inheritVelocity: float                              # How much of the air field velocity will be added to the particle velocity.
    inheritRotation: bool
    componentOnly: bool
    enableSpread: bool
    spread: float                                       # The angle of the air field cone if Enable Spread is true.

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

# Unknown controller
class NiLightIntensityController(NiFloatInterpController):

# Particle system modifier, updates the particle velocity to simulate the effects of point gravity.
class NiPSysRadialFieldModifier(NiPSysFieldModifier):
    radialType: float                                   # If zero, no attenuation.

# Abstract class used for different types of LOD selections.
class NiLODData(NiObject):

# NiRangeLODData controls switching LOD levels based on Z depth from the camera to the NiLODNode.
class NiRangeLODData(NiLODData):
    lodCenter: Vector3
    lodLevels: list[LODRange]

# NiScreenLODData controls switching LOD levels based on proportion of the screen that a bound would include.
class NiScreenLODData(NiLODData):
    bound: NiBound
    worldBound: NiBound
    proportionLevels: list[float]

# Unknown.
class NiRotatingParticles(NiParticles):

# DEPRECATED (pre-10.1), REMOVED (20.3).
# Keyframe animation root node, in .kf files.
class NiSequenceStreamHelper(NiObjectNET):

# Determines whether flat shading or smooth shading is used on a shape.
class NiShadeProperty(NiProperty):
    flags: Flags                                        # Bit 0: Enable smooth phong shading on this shape. Otherwise, hard-edged flat shading will be used on this shape.

# Skinning data.
class NiSkinData(NiObject):
    skinTransform: NiTransform                          # Offset of the skin from this bone in bind position.
    numBones: int                                       # Number of bones.
    skinPartition: int                                  # This optionally links a NiSkinPartition for hardware-acceleration information.
    hasVertexWeights: int                               # Enables Vertex Weights for this NiSkinData.
    boneList: list[BoneData]                            # Contains offset data for each node that this skin is influenced by.

# Skinning instance.
class NiSkinInstance(NiObject):
    data: int                                           # Skinning data reference.
    skinPartition: int                                  # Refers to a NiSkinPartition objects, which partitions the mesh such that every vertex is only influenced by a limited number of bones.
    skeletonRoot: int                                   # Armature root node.
    bones: list[int]                                    # List of all armature bones.

# Old version of skinning instance.
class NiTriShapeSkinController(NiTimeController):
    numBones: int                                       # The number of node bones referenced as influences.
    vertexCounts: list[int]                             # The number of vertex weights stored for each bone.
    bones: list[int]                                    # List of all armature bones.
    boneData: list[list[OldSkinData]]                   # Contains skin weight data for each node that this skin is influenced by.

# A copy of NISkinInstance for use with NiClod meshes.
class NiClodSkinInstance(NiSkinInstance):

# Skinning data, optimized for hardware skinning. The mesh is partitioned in submeshes such that each vertex of a submesh is influenced only by a limited and fixed number of bones.
class NiSkinPartition(NiObject):
    numSkinPartitionBlocks: int
    skinPartitionBlocks: list[SkinPartition]            # Skin partition objects.
    dataSize: int
    vertexSize: int
    vertexDesc: BSVertexDesc
    vertexData: list[BSVertexDataSSE]
    partition: list[SkinPartition]

# A texture.
class NiTexture(NiObjectNET):

# NiTexture::FormatPrefs. These preferences are a request to the renderer to use a format the most closely matches the settings and may be ignored.
class FormatPrefs:
    pixelLayout: PixelLayout = PixelLayout(r.readUInt32()) # Requests the way the image will be stored.
    useMipmaps: MipMapFormat = MipMapFormat(r.readUInt32()) # Requests if mipmaps are used or not.
    alphaFormat: AlphaFormat = AlphaFormat(r.readUInt32()) # Requests no alpha, 1-bit alpha, or

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

# Gives specularity to a shape. Flags 0x0001.
class NiSpecularProperty(NiProperty):
    flags: Flags                                        # Bit 0 = Enable specular lighting on this shape.

# LEGACY (pre-10.1) particle modifier.
class NiSphericalCollider(NiParticleModifier):
    unknownFloat1: float                                # Unknown.
    unknownShort1: int                                  # Unknown.
    unknownFloat2: float                                # Unknown.
    unknownShort2: int                                  # Unknown.
    unknownFloat3: float                                # Unknown.
    unknownFloat4: float                                # Unknown.
    unknownFloat5: float                                # Unknown.

# A spot.
class NiSpotLight(NiPointLight):
    outerSpotAngle: float
    innerSpotAngle: float
    exponent: float                                     # Describes the distribution of light. (see: glLight)

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

# Apparently commands for an optimizer instructing it to keep things it would normally discard.
# Also refers to NiNode objects (through their name) in animation .kf files.
class NiStringExtraData(NiExtraData):
    bytesRemaining: int                                 # The number of bytes left in the record.  Equals the length of the following string + 4.
    stringData: str                                     # The string.

# List of 0x00-seperated strings, which are names of controlled objects and controller types. Used in .kf files in conjunction with NiControllerSequence.
class NiStringPalette(NiObject):
    palette: StringPalette                              # A bunch of 0x00 seperated strings.

# List of strings; for example, a list of all bone names.
class NiStringsExtraData(NiExtraData):
    data: list[str]                                     # The strings.

# Extra data, used to name different animation sequences.
class NiTextKeyExtraData(NiExtraData):
    unknownInt1: int                                    # Unknown.  Always equals zero in all official files.
    textKeys: list[Key]                                 # List of textual notes and at which time they take effect. Used for designating the start and stop of animations and the triggering of sounds.

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

# LEGACY (pre-10.1)
class NiTextureModeProperty(NiProperty):
    unknownInts: list[int]
    unknownShort: int                                   # Unknown. Either 210 or 194.
    pS2L: int                                           # 0?
    pS2K: int                                           # -75?

# LEGACY (pre-10.1)
class NiImage(NiObject):
    useExternal: int                                    # 0 if the texture is internal to the NIF file.
    fileName: ??                                        # The filepath to the texture.
    imageData: int                                      # Link to the internally stored image data.
    unknownInt: int                                     # Unknown.  Often seems to be 7. Perhaps m_uiMipLevels?
    unknownFloat: float                                 # Unknown.  Perhaps fImageScale?

# LEGACY (pre-10.1)
class NiTextureProperty(NiProperty):
    unknownInts1: list[int]                             # Property flags.
    flags: Flags                                        # Property flags.
    image: int                                          # Link to the texture image.
    unknownInts2: list[int]                             # Unknown.  0?

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

class NiMultiTextureProperty(NiTexturingProperty):

# Wrapper for transformation animation keys.
class NiTransformData(NiKeyframeData):

# A shape node that refers to singular triangle data.
class NiTriShape(NiTriBasedGeom):

# Holds mesh data using a list of singular triangles.
class NiTriShapeData(NiTriBasedGeomData):
    numTrianglePoints: int                              # Num Triangles times 3.
    hasTriangles: bool                                  # Do we have triangle data?
    triangles: list[Triangle]                           # Triangle face data.
    matchGroups: list[MatchGroup]                       # The shared normals.

# A shape node that refers to data organized into strips of triangles
class NiTriStrips(NiTriBasedGeom):

# Holds mesh data using strips of triangles.
class NiTriStripsData(NiTriBasedGeomData):
    numStrips: int                                      # Number of OpenGL triangle strips that are present.
    stripLengths: list[int]                             # The number of points in each triangle strip.
    hasPoints: bool                                     # Do we have strip point data?
    points: list[list[int]]                             # The points in the Triangle strips. Size is the sum of all entries in Strip Lengths.

# Unknown
class NiEnvMappedTriShape(NiObjectNET):
    unknown1: int                                       # unknown (=4 - 5)
    unknownMatrix: Matrix4x4                            # unknown
    children: list[int]                                 # List of child node object indices.
    child2: int                                         # unknown
    child3: int                                         # unknown

# Holds mesh data using a list of singular triangles.
class NiEnvMappedTriShapeData(NiTriShapeData):

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

# A shape node that holds continuous level of detail information.
# Seems to be specific to Freedom Force.
class NiClod(NiTriBasedGeom):

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

# DEPRECATED (pre-10.1), REMOVED (20.3).
# Time controller for texture coordinates.
class NiUVController(NiTimeController):
    unknownShort: int                                   # Always 0?
    data: int                                           # Texture coordinate controller data index.

# DEPRECATED (pre-10.1), REMOVED (20.3)
# Texture coordinate data.
class NiUVData(NiObject):
    uvGroups: list[KeyGroup]                            # Four UV data groups. Appear to be U translation, V translation, U scaling/tiling, V scaling/tiling.

# DEPRECATED (20.5).
# Extra data in the form of a vector (as x, y, z, w components).
class NiVectorExtraData(NiExtraData):
    vectorData: Vector4                                 # The vector data.

# Property of vertex colors. This object is referred to by the root object of the NIF file whenever some NiTriShapeData object has vertex colors with non-default settings; if not present, vertex colors have vertex_mode=2 and lighting_mode=1.
class NiVertexColorProperty(NiProperty):
    flags: Flags                                        # Bits 0-2: Unknown
                                                        #     Bit 3: Lighting Mode
                                                        #     Bits 4-5: Vertex Mode
    vertexMode: VertMode                                # In Flags from 20.1.0.3 on.
    lightingMode: LightMode                             # In Flags from 20.1.0.3 on.

# DEPRECATED (10.x), REMOVED (?)
# Not used in skinning.
# Unsure of use - perhaps for morphing animation or gravity.
class NiVertWeightsExtraData(NiExtraData):
    numBytes: int                                       # Number of bytes in this data object.
    weight: list[float]                                 # The vertex weights.

# DEPRECATED (10.2), REMOVED (?), Replaced by NiBoolData.
# Visibility data for a controller.
class NiVisData(NiObject):
    keys: list[Key]

# Allows applications to switch between drawing solid geometry or wireframe outlines.
class NiWireframeProperty(NiProperty):
    flags: Flags                                        # Property flags.
                                                        #     0 - Wireframe Mode Disabled
                                                        #     1 - Wireframe Mode Enabled

# Allows applications to set the test and write modes of the renderer's Z-buffer and to set the comparison function used for the Z-buffer test.
class NiZBufferProperty(NiProperty):
    flags: Flags                                        # Bit 0 enables the z test
                                                        #     Bit 1 controls wether the Z buffer is read only (0) or read/write (1)
    function: ZCompareMode                              # Z-Test function (see: glDepthFunc). In Flags from 20.1.0.3 on.

# Morrowind-specific node for collision mesh.
class RootCollisionNode(NiNode):

# LEGACY (pre-10.1)
# Raw image data.
class NiRawImageData(NiObject):
    width: int                                          # Image width
    height: int                                         # Image height
    imageType: ImageType                                # The format of the raw image data.
    rgbImageData: list[list[ByteColor3]]                # Image pixel data.
    rgbaImageData: list[list[ByteColor4]]               # Image pixel data.

class NiAccumulator(NiObject):

# Used to turn sorting off for individual subtrees in a scene. Useful if objects must be drawn in a fixed order.
class NiSortAdjustNode(NiNode):
    sortingMode: SortingMode                            # Sorting
    accumulator: int

# Represents cube maps that are created from either a set of six image files, six blocks of pixel data, or a single pixel data with six faces.
class NiSourceCubeMap(NiSourceTexture):

# A PhysX prop which holds information about PhysX actors in a Gamebryo scene
class NiPhysXProp(NiObjectNET):
    physXtoWorldScale: float
    sources: list[int]
    dests: list[int]
    modifiedMeshes: list[int]
    tempName: str
    keepMeshes: bool
    propDescription: int

class PhysXMaterialRef:
    key: int = r.readUInt16()
    materialDesc: int = X[NiPhysXMaterialDesc].ref(r)

class PhysXStateName:
    name: str = Y.string(r)
    index: int = r.readUInt32()

# For serialization of PhysX objects and to attach them to the scene.
class NiPhysXPropDesc(NiObject):
    actors: list[int]
    joints: list[int]
    clothes: list[int]
    materials: list[PhysXMaterialRef]
    numStates: int
    stateNames: list[PhysXStateName]
    flags: int

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

class PhysXBodyStoredVels:
    linearVelocity: Vector3 = r.readVector3()
    angularVelocity: Vector3 = r.readVector3()
    sleep: bool = r.readBool32() if h.v >= 0x1E020003 else None

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
    actor: int = X[NiPhysXActorDesc].ref(r)
    localNormal: Vector3 = r.readVector3()
    localAxis: Vector3 = r.readVector3()
    localAnchor: Vector3 = r.readVector3()

class NxJointLimitSoftDesc:
    value: float = r.readSingle()
    restitution: float = r.readSingle()
    spring: float = r.readSingle()
    damping: float = r.readSingle()

class NxJointDriveDesc:
    driveType: NxD6JointDriveType = NxD6JointDriveType(r.readUInt32())
    restitution: float = r.readSingle()
    spring: float = r.readSingle()
    damping: float = r.readSingle()

class NiPhysXJointLimit:
    limitPlaneNormal: Vector3 = r.readVector3()
    limitPlaneD: float = r.readSingle()
    limitPlaneR: float = r.readSingle() if h.v >= 0x14040000 else None

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
    val1: float = r.readSingle()
    point1: Vector3 = r.readVector3()

class NxCapsule:
    val1: float = r.readSingle()
    val2: float = r.readSingle()
    capsuleFlags: int = r.readUInt32()

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

class NxMaterialFlag(Flag):
    NX_MF_ANISOTROPIC = 1 << 1,
    NX_MF_DUMMY1 = 1 << 2,
    NX_MF_DUMMY2 = 1 << 3,
    NX_MF_DUMMY3 = 1 << 4,
    NX_MF_DISABLE_FRICTION = 1 << 5,
    NX_MF_DISABLE_STRONG_FRICTION = 1 << 6

class NxSpringDesc:
    spring: float = r.readSingle()
    damper: float = r.readSingle()
    targetValue: float = r.readSingle()

class NxCombineMode(Enum):
    NX_CM_AVERAGE = 0,
    NX_CM_MIN = 1,
    NX_CM_MULTIPLY = 2,
    NX_CM_MAX = 3

class NxMaterialDesc:
    dynamicFriction: float = r.readSingle()
    staticFriction: float = r.readSingle()
    restitution: float = r.readSingle()
    dynamicFrictionV: float = r.readSingle()
    staticFrictionV: float = r.readSingle()
    directionofAnisotropy: Vector3 = r.readVector3()
    flags: NxMaterialFlag = NxMaterialFlag(r.readUInt32())
    frictionCombineMode: NxCombineMode = NxCombineMode(r.readUInt32())
    restitutionCombineMode: NxCombineMode = NxCombineMode(r.readUInt32())
    hasSpring: bool = r.readBool32() if h.v <= 0x14020300 else None
    spring: NxSpringDesc = NxSpringDesc(r) if Has Spring and h.v <= 0x14020300 else None

# For serializing NxMaterialDesc objects.
class NiPhysXMaterialDesc(NiObject):
    index: int
    materialDescs: list[NxMaterialDesc]

# A destination is a link between a PhysX actor and a Gamebryo object being driven by the physics.
class NiPhysXDest(NiObject):
    active: bool
    interpolate: bool

# Base for destinations that set a rigid body state.
class NiPhysXRigidBodyDest(NiPhysXDest):

# Connects PhysX rigid body actors to a scene node.
class NiPhysXTransformDest(NiPhysXRigidBodyDest):
    target: int

# A source is a link between a Gamebryo object and a PhysX actor.
class NiPhysXSrc(NiObject):
    active: bool
    interpolate: bool

# Sets state of a rigid body PhysX actor.
class NiPhysXRigidBodySrc(NiPhysXSrc):
    source: int

# Sets state of kinematic PhysX actor.
class NiPhysXKinematicSrc(NiPhysXRigidBodySrc):

# Sends Gamebryo scene state to a PhysX dynamic actor.
class NiPhysXDynamicSrc(NiPhysXRigidBodySrc):

# Wireframe geometry.
class NiLines(NiTriBasedGeom):

# Wireframe geometry data.
class NiLinesData(NiGeometryData):
    lines: list[bool]                                   # Is vertex connected to other (next?) vertex?

# Two dimensional screen elements.
class Polygon:
    numVertices: int = r.readUInt16()
    vertexOffset: int = r.readUInt16()                  # Offset in vertex array.
    numTriangles: int = r.readUInt16()
    triangleOffset: int = r.readUInt16()                # Offset in indices array.

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

# DEPRECATED (20.5), replaced by NiMeshScreenElements.
# Two dimensional screen elements.
class NiScreenElements(NiTriShape):

# NiRoomGroup represents a set of connected rooms i.e. a game level.
class NiRoomGroup(NiNode):
    shell: int                                          # Object that represents the room group as seen from the outside.
    rooms: list[int]

# NiRoom objects represent cells in a cell-portal culling system.
class NiRoom(NiNode):
    wallPlanes: list[NiPlane]
    inPortals: list[int]                                # The portals which see into the room.
    outPortals: list[int]                               # The portals which see out of the room.
    fixtures: list[int]                                 # All geometry associated with the room.

# NiPortal objects are grouping nodes that support aggressive visibility culling.
# They represent flat polygonal regions through which a part of a scene graph can be viewed.
class NiPortal(NiAVObject):
    portalFlags: int
    planeCount: int                                     # Unused in 20.x, possibly also 10.x.
    vertices: list[Vector3]
    adjoiner: int                                       # Root of the scenegraph which is to be seen through this portal.

# Bethesda-specific fade node.
class BSFadeNode(NiNode):

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

# Bethesda-specific property.
class BSShaderLightingProperty(BSShaderProperty):
    textureClampMode: TexClampMode                      # How to handle texture borders.

# Bethesda-specific property.
class BSShaderNoLightingProperty(BSShaderLightingProperty):
    fileName: str                                       # The texture glow map.
    falloffStartAngle: float                            # At this cosine of angle falloff will be equal to Falloff Start Opacity
    falloffStopAngle: float                             # At this cosine of angle falloff will be equal to Falloff Stop Opacity
    falloffStartOpacity: float                          # Alpha falloff multiplier at start angle
    falloffStopOpacity: float                           # Alpha falloff multiplier at end angle

# Bethesda-specific property.
class BSShaderPPLightingProperty(BSShaderLightingProperty):
    textureSet: int                                     # Texture Set
    refractionStrength: float                           # The amount of distortion. **Not based on physically accurate refractive index** (0=none) (0-1)
    refractionFirePeriod: int                           # Rate of texture movement for refraction shader.
    parallaxMaxPasses: float                            # The number of passes the parallax shader can apply.
    parallaxScale: float                                # The strength of the parallax.
    emissiveColor: Color4                               # Glow color and alpha

# This controller is used to animate float variables in BSEffectShaderProperty.
class BSEffectShaderPropertyFloatController(NiFloatInterpController):
    typeofControlledVariable: EffectShaderControlledVariable # Which float variable in BSEffectShaderProperty to animate:

# This controller is used to animate colors in BSEffectShaderProperty.
class BSEffectShaderPropertyColorController(NiPoint3InterpController):
    typeofControlledColor: EffectShaderControlledColor  # Which color in BSEffectShaderProperty to animate:

# This controller is used to animate float variables in BSLightingShaderProperty.
class BSLightingShaderPropertyFloatController(NiFloatInterpController):
    typeofControlledVariable: LightingShaderControlledVariable # Which float variable in BSLightingShaderProperty to animate:

# This controller is used to animate colors in BSLightingShaderProperty.
class BSLightingShaderPropertyColorController(NiPoint3InterpController):
    typeofControlledColor: LightingShaderControlledColor # Which color in BSLightingShaderProperty to animate:

class BSNiAlphaPropertyTestRefController(NiFloatInterpController):

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

# Bethesda-specific property. Found in Fallout3
class WaterShaderProperty(BSShaderProperty):

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

# Bethesda-specific property.
class TileShaderProperty(BSShaderLightingProperty):
    fileName: str                                       # Texture file name

# Bethesda-specific property.
class DistantLODShaderProperty(BSShaderProperty):

# Bethesda-specific property.
class BSDistantTreeShaderProperty(BSShaderProperty):

# Bethesda-specific property.
class TallGrassShaderProperty(BSShaderProperty):
    fileName: str                                       # Texture file name

# Bethesda-specific property.
class VolumetricFogShaderProperty(BSShaderProperty):

# Bethesda-specific property.
class HairShaderProperty(BSShaderProperty):

# Bethesda-specific property.
class Lighting30ShaderProperty(BSShaderPPLightingProperty):

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

# Skyrim Sky shader block.
class BSSkyShaderProperty(BSShaderProperty):
    shaderFlags1: SkyrimShaderPropertyFlags1
    shaderFlags2: SkyrimShaderPropertyFlags2
    uvOffset: TexCoord                                  # Offset UVs. Seems to be unused, but it fits with the other Skyrim shader properties.
    uvScale: TexCoord                                   # Offset UV Scale to repeat tiling textures, see above.
    sourceTexture: str                                  # points to an external texture.
    skyObjectType: SkyObjectType

# Bethesda-specific skin instance.
class BSDismemberSkinInstance(NiSkinInstance):
    partitions: list[BodyPartList]

# Bethesda-specific extra data. Lists locations and normals on a mesh that are appropriate for decal placement.
class BSDecalPlacementVectorExtraData(NiFloatExtraData):
    vectorBlocks: list[DecalVectorArray]

# Bethesda-specific particle modifier.
class BSPSysSimpleColorModifier(NiPSysModifier):
    fadeInPercent: float
    fadeoutPercent: float
    color1EndPercent: float
    color1StartPercent: float
    color2EndPercent: float
    color2StartPercent: float
    colors: list[Color4]

# Flags for BSValueNode.
class BSValueNodeFlags(Flag):
    BillboardWorldZ = 0,
    UsePlayerAdjust = 1 << 1

# Bethesda-specific node. Found on fxFire effects
class BSValueNode(NiNode):
    value: int
    valueNodeFlags: BSValueNodeFlags

# Bethesda-Specific (mesh?) Particle System.
class BSStripParticleSystem(NiParticleSystem):

# Bethesda-Specific (mesh?) Particle System Data.
class BSStripPSysData(NiPSysData):
    maxPointCount: int
    startCapSize: float
    endCapSize: float
    doZPrepass: bool

# Bethesda-Specific (mesh?) Particle System Modifier.
class BSPSysStripUpdateModifier(NiPSysModifier):
    updateDeltaTime: float

# Bethesda-Specific time controller.
class BSMaterialEmittanceMultController(NiFloatInterpController):

# Bethesda-Specific particle system.
class BSMasterParticleSystem(NiNode):
    maxEmitterObjects: int
    particleSystems: list[int]

# Particle system (multi?) emitter controller.
class BSPSysMultiTargetEmitterCtlr(NiPSysEmitterCtlr):
    maxEmitters: int
    masterParticleSystem: int

# Bethesda-Specific time controller.
class BSRefractionStrengthController(NiFloatInterpController):

# Bethesda-Specific node.
class BSOrderedNode(NiNode):
    alphaSortBound: Vector4
    staticBound: bool

# Bethesda-Specific node.
class BSRangeNode(NiNode):
    min: int
    max: int
    current: int

# Bethesda-Specific node.
class BSBlastNode(BSRangeNode):

# Bethesda-Specific node.
class BSDamageStage(BSBlastNode):

# Bethesda-specific time controller.
class BSRefractionFirePeriodController(NiTimeController):
    interpolator: int

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

# Bethesda-specific compound.
class BSTreadTransform:
    name: str = Y.string(r)
    transform1: NiQuatTransform = NiQuatTransform(r, h)
    transform2: NiQuatTransform = NiQuatTransform(r, h)

# Bethesda-specific interpolator.
class BSTreadTransfInterpolator(NiInterpolator):
    treadTransforms: list[BSTreadTransform]
    data: int

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

# Bethesda-specific object.
class BSAnimNotes(NiObject):
    animNotes: list[int]                                # BSAnimNote objects.

# Bethesda-specific Havok serializable.
class bhkLiquidAction(bhkSerializable):
    userData: int
    unknownInt2: int                                    # Unknown
    unknownInt3: int                                    # Unknown
    initialStickForce: float
    stickStrength: float
    neighborDistance: float
    neighborStrength: float

# Culling modes for multi bound nodes.
class BSCPCullingType(Enum):
    BSCP_CULL_NORMAL = 0,           # Normal
    BSCP_CULL_ALLPASS = 1,          # All Pass
    BSCP_CULL_ALLFAIL = 2,          # All Fail
    BSCP_CULL_IGNOREMULTIBOUNDS = 3, # Ignore Multi Bounds
    BSCP_CULL_FORCEMULTIBOUNDSNOUPDATE = 4 # Force Multi Bounds No Update

# Bethesda-specific node.
class BSMultiBoundNode(NiNode):
    multiBound: int
    cullingMode: BSCPCullingType

# Bethesda-specific object.
class BSMultiBound(NiObject):
    data: int

# Abstract base type for bounding data.
class BSMultiBoundData(NiObject):

# Oriented bounding box.
class BSMultiBoundOBB(BSMultiBoundData):
    center: Vector3                                     # Center of the box.
    size: Vector3                                       # Size of the box along each axis.
    rotation: Matrix3x3                                 # Rotation of the bounding box.

# Bethesda-specific object.
class BSMultiBoundSphere(BSMultiBoundData):
    center: Vector3
    radius: float

# This is only defined because of recursion issues.
class BSGeometrySubSegment:
    startIndex: int = r.readUInt32()
    numPrimitives: int = r.readUInt32()
    parentArrayIndex: int = r.readUInt32()
    unused: int = r.readUInt32()

# Bethesda-specific. Describes groups of triangles either segmented in a grid (for LOD) or by body part for skinned FO4 meshes.
class BSGeometrySegmentData:
    flags: int = r.readByte()
    index: int = r.readUInt32()                         # Index = previous Index + previous Num Tris in Segment * 3
    numTrisinSegment: int = r.readUInt32()              # The number of triangles belonging to this segment
    startIndex: int = r.readUInt32() if (h.userVersion2 == 130) else None
    numPrimitives: int = r.readUInt32() if (h.userVersion2 == 130) else None
    parentArrayIndex: int = r.readUInt32() if (h.userVersion2 == 130) else None
    subSegment: list[BSGeometrySubSegment] = r.readL32FArray(lambda r: BSGeometrySubSegment(r)) if (h.userVersion2 == 130) else None

# Bethesda-specific AV object.
class BSSegmentedTriShape(NiTriShape):
    segment: list[BSGeometrySegmentData]                # Configuration of each segment

# Bethesda-specific object.
class BSMultiBoundAABB(BSMultiBoundData):
    position: Vector3                                   # Position of the AABB's center
    extent: Vector3                                     # Extent of the AABB in all directions

class AdditionalDataInfo:
    dataType: int = r.readInt32()                       # Type of data in this channel
    numChannelBytesPerElement: int = r.readInt32()      # Number of bytes per element of this channel
    numChannelBytes: int = r.readInt32()                # Total number of bytes of this channel (num vertices times num bytes per element)
    numTotalBytesPerElement: int = r.readInt32()        # Number of bytes per element in all channels together. Sum of num channel bytes per element over all block infos.
    blockIndex: int = r.readInt32()                     # Unsure. The block in which this channel is stored? Usually there is only one block, and so this is zero.
    channelOffset: int = r.readInt32()                  # Offset (in bytes) of this channel. Sum of all num channel bytes per element of all preceeding block infos.
    unknownByte1: int = r.readByte()                    # Unknown, usually equal to 2.

class AdditionalDataBlock:
    hasData: bool = r.readBool32()                      # Has data
    blockSize: int = r.readInt32() if Has Data else None # Size of Block
    blockOffsets: list[int] = r.readL32FArray(lambda r: r.readInt32()) if Has Data else None
    numData: int = r.readInt32() if Has Data else None
    dataSizes: list[int] = r.readInt32() if Has Data else None
    data: list[list[int]] = r.readByte() if Has Data else None

class BSPackedAdditionalDataBlock:
    hasData: bool = r.readBool32()                      # Has data
    numTotalBytes: int = r.readInt32() if Has Data else None # Total number of bytes (over all channels and all elements, equals num total bytes per element times num vertices).
    blockOffsets: list[int] = r.readL32FArray(lambda r: r.readInt32()) if Has Data else None # Block offsets in the data? Usually equal to zero.
    atomSizes: list[int] = r.readL32FArray(lambda r: r.readInt32()) if Has Data else None # The sum of all of these equal num total bytes per element, so this probably describes how each data element breaks down into smaller chunks (i.e. atoms).
    data: list[int] = r.readByte() if Has Data else None
    unknownInt1: int = r.readInt32()
    numTotalBytesPerElement: int = r.readInt32()        # Unsure, but this seems to correspond again to the number of total bytes per element.

class NiAdditionalGeometryData(AbstractAdditionalGeometryData):
    numVertices: int                                    # Number of vertices
    blockInfos: list[AdditionalDataInfo]                # Number of additional data blocks
    blocks: list[AdditionalDataBlock]                   # Number of additional data blocks

class BSPackedAdditionalGeometryData(AbstractAdditionalGeometryData):
    numVertices: int
    blockInfos: list[AdditionalDataInfo]                # Number of additional data blocks
    blocks: list[BSPackedAdditionalDataBlock]           # Number of additional data blocks

# Bethesda-specific extra data.
class BSWArray(NiExtraData):
    items: list[int]

# Bethesda-specific Havok serializable.
class bhkAabbPhantom(bhkShapePhantom):
    unused: list[int]
    aabbMin: Vector4
    aabbMax: Vector4

# Bethesda-specific time controller.
class BSFrustumFOVController(NiFloatInterpController):

# Bethesda-Specific node.
class BSDebrisNode(BSRangeNode):

# A breakable constraint.
class bhkBreakableConstraint(bhkConstraint):
    constraintData: ConstraintData                      # Constraint within constraint.
    threshold: float                                    # Amount of force to break the rigid bodies apart?
    removeWhenBroken: bool                              # No: Constraint stays active. Yes: Constraint gets removed when breaking threshold is exceeded.

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

# Found in Fallout 3 .psa files, extra ragdoll info for NPCs/creatures. (usually idleanims\deathposes.psa)
# Defines different kill poses. The game selects the pose randomly and applies it to a skeleton immediately upon ragdolling.
# Poses can be previewed in GECK Object Window-Actor Data-Ragdoll and selecting Pose Matching tab.
class bhkPoseArray(NiObject):
    bones: list[str]
    poses: list[BonePose]

# Found in Fallout 3, more ragdoll info?  (meshes\ragdollconstraint\*.rdt)
class bhkRagdollTemplate(NiExtraData):
    bones: list[int]

# Data for bhkRagdollTemplate
class bhkRagdollTemplateData(NiObject):
    name: str
    mass: float
    restitution: float
    friction: float
    radius: float
    material: HavokMaterial
    constraint: list[ConstraintData]

# A range of indices, which make up a region (such as a submesh).
class Region:
    startIndex: int = r.readUInt32()
    numIndices: int = r.readUInt32()

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

class SemanticData:
    name: str = Y.string(r)                             # Type of data (POSITION, POSITION_BP, INDEX, NORMAL, NORMAL_BP,
                                                        #     TEXCOORD, BLENDINDICES, BLENDWEIGHT, BONE_PALETTE, COLOR, DISPLAYLIST,
                                                        #     MORPH_POSITION, BINORMAL_BP, TANGENT_BP).
    index: int = r.readUInt32()                         # An extra index of the data. For example, if there are 3 uv maps,
                                                        #     then the corresponding TEXCOORD data components would have indices
                                                        #     0, 1, and 2, respectively.

class DataStreamRef:
    stream: int = X[NiDataStream].ref(r)                # Reference to a data stream object which holds the data used by
                                                        #     this reference.
    isPerInstance: bool = r.readBool32()                # Sets whether this stream data is per-instance data for use in
                                                        #     hardware instancing.
    submeshToRegionMap: list[int] = r.readL16FArray(lambda r: r.readUInt16()) # A lookup table that maps submeshes to regions.
    componentSemantics: list[SemanticData] = r.readL32FArray(lambda r: SemanticData(r)) # Describes the semantic of each component.

# An object that can be rendered.
class NiRenderObject(NiAVObject):
    materialData: MaterialData                          # Per-material data.

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
    SYNC_PHYSICS_COMPLETED = 0x8060, # Synchronize when a physics simulation step has produced results.
    SYNC_REFLECTIONS = 0x8070       # Synchronize after all data necessary to calculate reflections is ready.

# Base class for mesh modifiers.
class NiMeshModifier(NiObject):
    submitPoints: list[SyncPoint]                       # The sync points supported by this mesh modifier for SubmitTasks.
    completePoints: list[SyncPoint]                     # The sync points supported by this mesh modifier for CompleteTasks.

class ExtraMeshDataEpicMickey:
    unknownInt1: int = r.readInt32()
    unknownInt2: int = r.readInt32()
    unknownInt3: int = r.readInt32()
    unknownInt4: float = r.readSingle()
    unknownInt5: float = r.readSingle()
    unknownInt6: float = r.readSingle()

class ExtraMeshDataEpicMickey2:
    start: int = r.readInt32()
    end: int = r.readInt32()
    unknownShorts: list[int] = r.readInt16()

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

# Manipulates a mesh with the semantic MORPHWEIGHTS using an NiMorphMeshModifier.
class NiMorphWeightsController(NiInterpController):
    count: int
    interpolators: list[int]
    targetNames: list[str]

class ElementReference:
    semantic: SemanticData = SemanticData(r)            # The element semantic.
    normalizeFlag: int = r.readUInt32()                 # Whether or not to normalize the data.

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

class NiSkinningMeshModifier(NiMeshModifier):
    flags: int                                          # USE_SOFTWARE_SKINNING = 0x0001
                                                        #     RECOMPUTE_BOUNDS = 0x0002
    skeletonRoot: int                                   # The root bone of the skeleton.
    skeletonTransform: NiTransform                      # The transform that takes the root bone parent coordinate system into the skin coordinate system.
    numBones: int                                       # The number of bones referenced by this mesh modifier.
    bones: list[int]                                    # Pointers to the bone nodes that affect this skin.
    boneTransforms: list[NiTransform]                   # The transforms that go from bind-pose space to bone space.
    boneBounds: list[NiBound]                           # The bounds of the bones.  Only stored if the RECOMPUTE_BOUNDS bit is set.

# An instance of a hardware-instanced mesh in a scene graph.
class NiMeshHWInstance(NiAVObject):
    masterMesh: int                                     # The instanced mesh this object represents.
    meshModifier: int

# Mesh modifier that provides per-frame instancing capabilities in Gamebryo.
class NiInstancingMeshModifier(NiMeshModifier):
    hasInstanceNodes: bool
    perInstanceCulling: bool
    hasStaticBounds: bool
    affectedMesh: int
    bound: NiBound
    instanceNodes: list[int]

class LODInfo:
    numBones: int = r.readUInt32()
    skinIndices: list[int] = r.readL32FArray(lambda r: r.readUInt32())

# Defines the levels of detail for a given character and dictates the character's current LOD.
class NiSkinningLODController(NiTimeController):
    currentLod: int
    bones: list[int]
    skins: list[int]
    loDs: list[LODInfo]

class PSSpawnRateKey:
    value: float = r.readSingle()
    time: float = r.readSingle()

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

# Represents a particle system that uses mesh particles instead of sprite-based particles.
class NiPSMeshParticleSystem(NiPSParticleSystem):
    masterParticles: list[int]
    poolSize: int
    autoFillPools: bool

# A mesh modifier that uses particle system data to generate camera-facing quads.
class NiPSFacingQuadGenerator(NiMeshModifier):

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

# The mesh modifier that performs all particle system simulation.
class NiPSSimulator(NiMeshModifier):
    simulationSteps: list[int]

# Abstract base class for a single step in the particle system simulation process.  It has no seralized data.
class NiPSSimulatorStep(NiObject):

class PSLoopBehavior(Enum):
    PS_LOOP_CLAMP_BIRTH = 0,        # Key times map such that the first key occurs at the birth of the particle, and times later than the last key get the last key value.
    PS_LOOP_CLAMP_DEATH = 1,        # Key times map such that the last key occurs at the death of the particle, and times before the initial key time get the value of the initial key.
    PS_LOOP_AGESCALE = 2,           # Scale the animation to fit the particle lifetime, so that the first key is age zero, and the last key comes at the particle death.
    PS_LOOP_LOOP = 3,               # The time is converted to one within the time range represented by the keys, as if the key sequence loops forever in the past and future.
    PS_LOOP_REFLECT = 4             # The time is reflection looped, as if the keys played forward then backward the forward then backward etc for all time.

# Encapsulates a floodgate kernel that updates particle size, colors, and rotations.
class NiPSSimulatorGeneralStep(NiPSSimulatorStep):
    sizeKeys: list[Key]                                 # The particle size keys.
    sizeLoopBehavior: PSLoopBehavior                    # The loop behavior for the size keys.
    colorKeys: list[Key]                                # The particle color keys.
    colorLoopBehavior: PSLoopBehavior                   # The loop behavior for the color keys.
    rotationKeys: list[QuatKey]                         # The particle rotation keys.
    rotationLoopBehavior: PSLoopBehavior                # The loop behavior for the rotation keys.
    growTime: float                                     # The the amount of time over which a particle's size is ramped from 0.0 to 1.0 in seconds
    shrinkTime: float                                   # The the amount of time over which a particle's size is ramped from 1.0 to 0.0 in seconds
    growGeneration: int                                 # Specifies the particle generation to which the grow effect should be applied. This is usually generation 0, so that newly created particles will grow.
    shrinkGeneration: int                               # Specifies the particle generation to which the shrink effect should be applied. This is usually the highest supported generation for the particle system, so that particles will shrink immediately before getting killed.

# Encapsulates a floodgate kernel that simulates particle forces.
class NiPSSimulatorForcesStep(NiPSSimulatorStep):
    forces: list[int]                                   # The forces affecting the particle system.

# Encapsulates a floodgate kernel that simulates particle colliders.
class NiPSSimulatorCollidersStep(NiPSSimulatorStep):
    colliders: list[int]                                # The colliders affecting the particle system.

# Encapsulates a floodgate kernel that updates mesh particle alignment and transforms.
class NiPSSimulatorMeshAlignStep(NiPSSimulatorStep):
    rotationKeys: list[QuatKey]                         # The particle rotation keys.
    rotationLoopBehavior: PSLoopBehavior                # The loop behavior for the rotation keys.

# Encapsulates a floodgate kernel that updates particle positions and ages. As indicated by its name, this step should be attached last in the NiPSSimulator mesh modifier.
class NiPSSimulatorFinalStep(NiPSSimulatorStep):

# Updates the bounding volume for an NiPSParticleSystem object.
class NiPSBoundUpdater(NiObject):
    updateSkip: int                                     # Number of particle bounds to skip updating every frame. Higher = more updates each frame.

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

# Applies a linear drag force to particles.
class NiPSDragForce(NiPSForce):
    dragAxis: Vector3
    percentage: float
    range: float
    rangeFalloff: float
    dragObject: int

# Applies a gravitational force to particles.
class NiPSGravityForce(NiPSForce):
    gravityAxis: Vector3
    decay: float
    strength: float
    forceType: ForceType
    turbulence: float
    turbulenceScale: float
    gravityObject: int

# Applies an explosive force to particles.
class NiPSBombForce(NiPSForce):
    bombAxis: Vector3
    decay: float
    deltaV: float
    decayType: DecayType
    symmetryType: SymmetryType
    bombObject: int

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

# Abstract base class for particle emitters that emit particles from a volume.
class NiPSVolumeEmitter(NiPSEmitter):
    emitterObject: int

# A particle emitter that emits particles from a rectangular volume.
class NiPSBoxEmitter(NiPSVolumeEmitter):
    emitterWidth: float
    emitterHeight: float
    emitterDepth: float

# A particle emitter that emits particles from a spherical volume.
class NiPSSphereEmitter(NiPSVolumeEmitter):
    emitterRadius: float

# A particle emitter that emits particles from a cylindrical volume.
class NiPSCylinderEmitter(NiPSVolumeEmitter):
    emitterRadius: float
    emitterHeight: float

# Emits particles from one or more NiMesh objects. A random mesh emitter is selected for each particle emission.
class NiPSMeshEmitter(NiPSEmitter):
    meshEmitters: list[int]
    emitAxis: Vector3
    emitterObject: int
    meshEmissionType: EmitFrom
    initialVelocityType: VelocityType

# Abstract base class for all particle emitter time controllers.
class NiPSEmitterCtlr(NiSingleInterpController):
    emitterName: str

# Abstract base class for controllers that animate a floating point value on an NiPSEmitter object.
class NiPSEmitterFloatCtlr(NiPSEmitterCtlr):

# Animates particle emission and birth rate.
class NiPSEmitParticlesCtlr(NiPSEmitterCtlr):
    emitterActiveInterpolator: int

# Abstract base class for all particle force time controllers.
class NiPSForceCtlr(NiSingleInterpController):
    forceName: str

# Abstract base class for controllers that animate a Boolean value on an NiPSForce object.
class NiPSForceBoolCtlr(NiPSForceCtlr):

# Abstract base class for controllers that animate a floating point value on an NiPSForce object.
class NiPSForceFloatCtlr(NiPSForceCtlr):

# Animates whether or not an NiPSForce object is active.
class NiPSForceActiveCtlr(NiPSForceBoolCtlr):

# Animates the strength value of an NiPSGravityForce object.
class NiPSGravityStrengthCtlr(NiPSForceFloatCtlr):

# Animates the speed value on an NiPSEmitter object.
class NiPSEmitterSpeedCtlr(NiPSEmitterFloatCtlr):

# Animates the size value on an NiPSEmitter object.
class NiPSEmitterRadiusCtlr(NiPSEmitterFloatCtlr):

# Animates the declination value on an NiPSEmitter object.
class NiPSEmitterDeclinationCtlr(NiPSEmitterFloatCtlr):

# Animates the declination variation value on an NiPSEmitter object.
class NiPSEmitterDeclinationVarCtlr(NiPSEmitterFloatCtlr):

# Animates the planar angle value on an NiPSEmitter object.
class NiPSEmitterPlanarAngleCtlr(NiPSEmitterFloatCtlr):

# Animates the planar angle variation value on an NiPSEmitter object.
class NiPSEmitterPlanarAngleVarCtlr(NiPSEmitterFloatCtlr):

# Animates the rotation angle value on an NiPSEmitter object.
class NiPSEmitterRotAngleCtlr(NiPSEmitterFloatCtlr):

# Animates the rotation angle variation value on an NiPSEmitter object.
class NiPSEmitterRotAngleVarCtlr(NiPSEmitterFloatCtlr):

# Animates the rotation speed value on an NiPSEmitter object.
class NiPSEmitterRotSpeedCtlr(NiPSEmitterFloatCtlr):

# Animates the rotation speed variation value on an NiPSEmitter object.
class NiPSEmitterRotSpeedVarCtlr(NiPSEmitterFloatCtlr):

# Animates the lifespan value on an NiPSEmitter object.
class NiPSEmitterLifeSpanCtlr(NiPSEmitterFloatCtlr):

# Calls ResetParticleSystem on an NiPSParticleSystem target upon looping.
class NiPSResetOnLoopCtlr(NiTimeController):

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

# A planar collider for particles.
class NiPSPlanarCollider(NiPSCollider):
    width: float
    height: float
    xAxis: Vector3
    yAxis: Vector3
    colliderObject: int

# A spherical collider for particles.
class NiPSSphericalCollider(NiPSCollider):
    radius: float
    colliderObject: int

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

class NiKeyBasedEvaluator(NiEvaluator):

class NiBoolEvaluator(NiKeyBasedEvaluator):
    data: int

class NiBoolTimelineEvaluator(NiBoolEvaluator):

class NiColorEvaluator(NiKeyBasedEvaluator):
    data: int

class NiFloatEvaluator(NiKeyBasedEvaluator):
    data: int

class NiPoint3Evaluator(NiKeyBasedEvaluator):
    data: int

class NiQuaternionEvaluator(NiKeyBasedEvaluator):
    data: int

class NiTransformEvaluator(NiKeyBasedEvaluator):
    value: NiQuatTransform
    data: int

class NiConstBoolEvaluator(NiEvaluator):
    value: float

class NiConstColorEvaluator(NiEvaluator):
    value: Color4

class NiConstFloatEvaluator(NiEvaluator):
    value: float

class NiConstPoint3Evaluator(NiEvaluator):
    value: Vector3

class NiConstQuaternionEvaluator(NiEvaluator):
    value: Quaternion

class NiConstTransformEvaluator(NiEvaluator):
    value: NiQuatTransform

class NiBSplineEvaluator(NiEvaluator):
    startTime: float
    endTime: float
    data: int
    basisData: int

class NiBSplineColorEvaluator(NiBSplineEvaluator):
    handle: int                                         # Handle into the data. (USHRT_MAX for invalid handle.)

class NiBSplineCompColorEvaluator(NiBSplineColorEvaluator):
    offset: float
    halfRange: float

class NiBSplineFloatEvaluator(NiBSplineEvaluator):
    handle: int                                         # Handle into the data. (USHRT_MAX for invalid handle.)

class NiBSplineCompFloatEvaluator(NiBSplineFloatEvaluator):
    offset: float
    halfRange: float

class NiBSplinePoint3Evaluator(NiBSplineEvaluator):
    handle: int                                         # Handle into the data. (USHRT_MAX for invalid handle.)

class NiBSplineCompPoint3Evaluator(NiBSplinePoint3Evaluator):
    offset: float
    halfRange: float

class NiBSplineTransformEvaluator(NiBSplineEvaluator):
    transform: NiQuatTransform
    translationHandle: int                              # Handle into the translation data. (USHRT_MAX for invalid handle.)
    rotationHandle: int                                 # Handle into the rotation data. (USHRT_MAX for invalid handle.)
    scaleHandle: int                                    # Handle into the scale data. (USHRT_MAX for invalid handle.)

class NiBSplineCompTransformEvaluator(NiBSplineTransformEvaluator):
    translationOffset: float
    translationHalfRange: float
    rotationOffset: float
    rotationHalfRange: float
    scaleOffset: float
    scaleHalfRange: float

class NiLookAtEvaluator(NiEvaluator):
    flags: LookAtFlags
    lookAtName: str
    drivenName: str
    interpolatorTranslation: int
    interpolatorRoll: int
    interpolatorScale: int

class NiPathEvaluator(NiKeyBasedEvaluator):
    flags: PathFlags
    bankDir: int                                        # -1 = Negative, 1 = Positive
    maxBankAngle: float                                 # Max angle in radians.
    smoothing: float
    followAxis: int                                     # 0, 1, or 2 representing X, Y, or Z.
    pathData: int
    percentData: int

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

class NiFurSpringController(NiTimeController):
    unknownFloat: float
    unknownFloat2: float
    bones: list[int]                                    # List of all armature bones.
    bones2: list[int]                                   # List of all armature bones.

class CStreamableAssetData(NiObject):
    root: int
    unknownBytes: list[int]

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

# Unknown
class BSBoneLODExtraData(NiExtraData):
    boneLodCount: int                                   # Number of bone entries
    boneLodInfo: list[BoneLOD]                          # Bone Entry

# Links a nif with a Havok Behavior .hkx animation file
class BSBehaviorGraphExtraData(NiExtraData):
    behaviourGraphFile: str                             # Name of the hkx file.
    controlsBaseSkeleton: bool                          # Unknown, has to do with blending appended bones onto an actor.

# A controller that trails a bone behind an actor.
class BSLagBoneController(NiTimeController):
    linearVelocity: float                               # How long it takes to rotate about an actor back to rest position.
    linearRotation: float                               # How the bone lags rotation
    maximumDistance: float                              # How far bone will tail an actor.

# A variation on NiTriShape, for visibility control over vertex groups.
class BSLODTriShape(NiTriBasedGeom):
    loD0Size: int
    loD1Size: int
    loD2Size: int

# Furniture Marker for actors
class BSFurnitureMarkerNode(BSFurnitureMarker):

# Unknown, related to trees.
class BSLeafAnimNode(NiNode):

# Node for handling Trees, Switches branch configurations for variation?
class BSTreeNode(NiNode):
    bones1: list[int]                                   # Unknown
    bones: list[int]                                    # Unknown

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

# Fallout 4 LOD Tri Shape
class BSMeshLODTriShape(BSTriShape):
    loD0Size: int
    loD1Size: int
    loD2Size: int

class BSGeometryPerSegmentSharedData:
    userIndex: int = r.readUInt32()                     # If Bone ID is 0xffffffff, this value refers to the Segment at the listed index. Otherwise this is the "Biped Object", which is like the body part types in Skyrim and earlier.
    boneId: int = r.readUInt32()                        # A hash of the bone name string.
    cutOffsets: list[float] = r.readL32FArray(lambda r: r.readSingle())

class BSGeometrySegmentSharedData:
    numSegments: int = r.readUInt32()
    totalSegments: int = r.readUInt32()
    segmentStarts: list[int] = r.readUInt32()
    perSegmentData: list[BSGeometryPerSegmentSharedData] = BSGeometryPerSegmentSharedData(r)
    ssfLength: int = r.readUInt16()
    ssfFile: list[int] = r.readByte()

# Fallout 4 Sub-Index Tri Shape
class BSSubIndexTriShape(BSTriShape):
    numPrimitives: int
    numSegments: int
    totalSegments: int
    segment: list[BSGeometrySegmentData]
    segmentData: BSGeometrySegmentSharedData

# Fallout 4 Physics System
class bhkSystem(NiObject):

# Fallout 4 Collision Object
class bhkNPCollisionObject(NiCollisionObject):
    flags: int                                          # Due to inaccurate reporting in the CK the Reset and Sync On Update positions are a guess.
                                                        #     Bits: 0=Reset, 2=Notify, 3=SetLocal, 7=SyncOnUpdate, 10=AnimTargeted
    data: int
    bodyId: int

# Fallout 4 Collision System
class bhkPhysicsSystem(bhkSystem):
    binaryData: bytearray

# Fallout 4 Ragdoll System
class bhkRagdollSystem(bhkSystem):
    binaryData: bytearray

# Fallout 4 Extra Data
class BSExtraData(NiExtraData):

# Fallout 4 Cloth data
class BSClothExtraData(BSExtraData):
    binaryData: bytearray

# Fallout 4 Bone Transform
class BSSkinBoneTrans:
    boundingSphere: NiBound = NiBound(r)
    rotation: Matrix3x3 = r.readMatrix3x3()
    translation: Vector3 = r.readVector3()
    scale: float = r.readSingle()

# Fallout 4 Skin Instance
class BSSkin::Instance(NiObject):
    skeletonRoot: int
    data: int
    bones: list[int]
    unknown: list[Vector3]

# Fallout 4 Bone Data
class BSSkin::BoneData(NiObject):
    boneList: list[BSSkinBoneTrans]

# Fallout 4 Positional Data
class BSPositionData(NiExtraData):
    data: list[float]

class BSConnectPoint:
    parent: str = r.readL32AString()
    name: str = r.readL32AString()
    rotation: Quaternion = r.readQuaternion()
    translation: Vector3 = r.readVector3()
    scale: float = r.readSingle()

# Fallout 4 Item Slot Parent
class BSConnectPoint::Parents(NiExtraData):
    connectPoints: list[BSConnectPoint]

# Fallout 4 Item Slot Child
class BSConnectPoint::Children(NiExtraData):
    skinned: bool
    name: list[str]

# Fallout 4 Eye Center Data
class BSEyeCenterExtraData(NiExtraData):
    data: list[float]

class BSPackedGeomDataCombined:
    grayscaletoPaletteScale: float = r.readSingle()
    transform: NiTransform = NiTransform(r)
    boundingSphere: NiBound = NiBound(r)

class BSPackedGeomData:
    numVerts: int = r.readUInt32()
    lodLevels: int = r.readUInt32()
    triCountLoD0: int = r.readUInt32()
    triOffsetLoD0: int = r.readUInt32()
    triCountLoD1: int = r.readUInt32()
    triOffsetLoD1: int = r.readUInt32()
    triCountLoD2: int = r.readUInt32()
    triOffsetLoD2: int = r.readUInt32()
    combined: list[BSPackedGeomDataCombined] = r.readL32FArray(lambda r: BSPackedGeomDataCombined(r))
    vertexDesc: BSVertexDesc = BSVertexDesc(r)
    vertexData: list[BSVertexData] = BSVertexData(r, h) if !BSPackedCombinedSharedGeomDataExtra else None
    triangles: list[Triangle] = Triangle(r) if !BSPackedCombinedSharedGeomDataExtra else None

# This appears to be a 64-bit hash but nif.xml does not have a 64-bit type.
class BSPackedGeomObject:
    shapeID1: int = r.readUInt32()
    shapeID2: int = r.readUInt32()

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

# Fallout 4 Packed Combined Shared Geometry Data.
# Geometry is NOT baked into the file. It is instead a reference to the shape via a Shape ID (currently undecoded)
# which loads the geometry via the STAT form for the NIF.
class BSPackedCombinedSharedGeomDataExtra(BSPackedCombinedGeomDataExtra):

class NiLightRadiusController(NiFloatInterpController):

class BSDynamicTriShape(BSTriShape):
    vertexDataSize: int
    vertices: list[Vector4]

#endregion

# Large ref flag.
class BSDistantObjectLargeRefExtraData(NiExtraData):
    largeRef: bool

