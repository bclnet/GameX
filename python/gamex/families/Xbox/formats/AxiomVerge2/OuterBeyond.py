from __future__ import annotations
import os
from enum import Enum, IntFlag
from openstk.core.typex import *
from gamex.families.Xbox.formats.xna import TypeReader

class THTileMap:
    def __init__(self, r: BinaryReader):
        self.count: int = r.readInt32()
        self.tileSetName: str = r.readString()
        self.widthTiles: int = r.readInt32(); self.heightTiles = r.readInt32()
        self.tileSetWidthTiles: int = r.readInt32(); self.tileSetHeightTiles = r.readInt32()
        self.tileMapGroups: list[THTileMapGroup] = r.readL32FArray(lambda z: THTileMapGroup(r))

class THWorldLayerType(Enum):
    Breach = 'Breach'
    Inside = 'Inside'
    Outside = 'Outside'
    COUNT = 'COUNT'
    NONE = 'NONE'

class THTileFlags(IntFlag):
    FLAG_FLIPH = 2147483648 # 0x80000000
    FLAG_FLIPV = 1073741824 # 0x40000000
    MASK_FLIP = FLAG_FLIPV | FLAG_FLIPH # 0xC0000000
    MASK_DAMAGE_FLAGS = 117440512 # 0x07000000
    FLAG_GLITCHABLE = 67108864 # 0x04000000
    FLAG_TRIGGERABLE = 33554432 # 0x02000000
    FLAG_DAMAGEABLE = 16777216 # 0x01000000
    FLAG_UNCLIMBABLE = 8192 # 0x00002000
    FLAG_DAMAGEABLE_FRINGE = 4096 # 0x00001000
    FLAG_CASTS_SHADOWS = 2048 # 0x00000800
    FLAG_HANDLES_DAMAGE_AS_NPC = 1024 # 0x00000400
    FLAG_LEDGE = 512 # 0x00000200
    FLAG_STAIR = 256 # 0x00000100
    FLAG_CHAIN_DESTRUCTS = 128 # 0x00000080
    FLAG_FIELD = 64 # 0x00000040
    FLAG_SURFACE = 32 # 0x00000020
    FLAG_ALTERNATE_RENDER = 16 # 0x00000010
    FLAG_COLLISION = 8
    FLAG_HIDDEN = 4
    MASK_COLLISION_SHAPE = 3
    FLAG_EMPTY_TILE = 4294967295 # 0xFFFFFFFF
    FLAG_NONE = 0

class THMapObjectType(Enum):
    ApocalypseUrn = 0
    Boss = 1
    BreachPortal = 2
    Damageable = 3
    DamageTrigger = 4
    ExteriorDoor = 5
    ForceField = 6
    Generic = 7
    GlitchableTile = 8
    GenericRegion = 9
    Item = 10
    Lattice = 11
    NPC = 12
    ParticleObject = 13
    Room = 14
    RoomAction = 15
    RoomTransition = 16
    PasscodeAction = 17
    SavePoint = 18
    SecretWorld = 19
    SecretWorldEntrance = 20
    SecretWorldItem = 21
    TileNPC = 22
    TriggerRegion = 23
    UdugDoor = 24

class THProperties:
    def __init__(self):
        self.type: THMapObjectType = THMapObjectType.Generic
        self.properties: dict[str, str] = {}

class THCollisionTile:
    def __init__(self, flags: THTileFlags, properties: THProperties):
        self.flags: THTileFlags = flags
        self.properties: THProperties = properties

class THTileMapGroup:
    def __init__(self, r: BinaryReader):
        self.name: str = r.readString()
        # self.worldLayerType: THWorldLayerType = (THWorldLayerType)Enum.Parse(typeof(THWorldLayerType), self.name, True)
        self.widthTiles: int = r.readInt32(); self.heightTiles: int = r.readInt32()
        self.pixelBounds: RectangleF = RectangleF(0., 0., self.widthTiles * 16, self.heightTiles * 16)
        self.tileSetWidthTiles = r.readInt32(); self.tileSetHeightTiles = r.readInt32()
        self.collisionTiles = [THCollisionTile(THTileFlags(s)) for s in r.readFArray(lambda z: r.readPArray(None, 'I', self.widthTiles), self.heightTiles)]
        # TODO

@RType('OuterBeyond.THTileMapReader')
@RAssembly('AxiomVerge2')
class THTileMapReader(TypeReader[THTileMap]):
    def __init__(self, t: type): super().__init__(t)
    def read(self, r: ContentReader, o: THTileMap) -> THTileMap: return THTileMap(r)