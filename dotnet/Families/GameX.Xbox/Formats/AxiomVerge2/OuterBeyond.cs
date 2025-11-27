using GameX.Xbox.Formats.Xna;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace GameX.Xbox.Formats.AxiomVerge2.OuterBeyond;

[RType("OuterBeyond.THTileMapReader"), RAssembly("AxiomVerge2, Version=1.0.0.0, Culture=neutral")] class THTileMapReader : TypeReader<THTileMap> { public override THTileMap Read(ContentReader r, THTileMap o) => new(r); }

class THTileMap(BinaryReader r) {
    public int Count = r.ReadInt32();
    public string TileSetName = r.ReadString();
    public int WidthTiles = r.ReadInt32(), HeightTiles = r.ReadInt32();
    public int TileSetWidthTiles = r.ReadInt32(), TileSetHeightTiles = r.ReadInt32();
    public THTileMapGroup[] TileMapGroups = r.ReadL32FArray(z => new THTileMapGroup(r));
}

public enum THWorldLayerType {
    Breach,
    Inside,
    Outside,
    COUNT,
    NONE,
}

[Flags]
public enum THTileFlags : uint {
    FLAG_FLIPH = 2147483648, // 0x80000000
    FLAG_FLIPV = 1073741824, // 0x40000000
    MASK_FLIP = FLAG_FLIPV | FLAG_FLIPH, // 0xC0000000
    MASK_DAMAGE_FLAGS = 117440512, // 0x07000000
    FLAG_GLITCHABLE = 67108864, // 0x04000000
    FLAG_TRIGGERABLE = 33554432, // 0x02000000
    FLAG_DAMAGEABLE = 16777216, // 0x01000000
    FLAG_UNCLIMBABLE = 8192, // 0x00002000
    FLAG_DAMAGEABLE_FRINGE = 4096, // 0x00001000
    FLAG_CASTS_SHADOWS = 2048, // 0x00000800
    FLAG_HANDLES_DAMAGE_AS_NPC = 1024, // 0x00000400
    FLAG_LEDGE = 512, // 0x00000200
    FLAG_STAIR = 256, // 0x00000100
    FLAG_CHAIN_DESTRUCTS = 128, // 0x00000080
    FLAG_FIELD = 64, // 0x00000040
    FLAG_SURFACE = 32, // 0x00000020
    FLAG_ALTERNATE_RENDER = 16, // 0x00000010
    FLAG_COLLISION = 8,
    FLAG_HIDDEN = 4,
    MASK_COLLISION_SHAPE = 3,
    FLAG_EMPTY_TILE = 4294967295, // 0xFFFFFFFF
    FLAG_NONE = 0,
}

public enum THMapObjectType {
    ApocalypseUrn,
    Boss,
    BreachPortal,
    Damageable,
    DamageTrigger,
    ExteriorDoor,
    ForceField,
    Generic,
    GlitchableTile,
    GenericRegion,
    Item,
    Lattice,
    NPC,
    ParticleObject,
    Room,
    RoomAction,
    RoomTransition,
    PasscodeAction,
    SavePoint,
    SecretWorld,
    SecretWorldEntrance,
    SecretWorldItem,
    TileNPC,
    TriggerRegion,
    UdugDoor,
}

public class THProperties {
    public THMapObjectType Type = THMapObjectType.Generic;
    public Dictionary<string, string> Properties;
}

public struct THCollisionTile {
    public THTileFlags Flags;
    public THProperties Properties;
}

class THTileMapGroup {
    public string Name;
    public THWorldLayerType WorldLayerType;
    public int WidthTiles, HeightTiles;
    public RectangleF PixelBounds;
    public int TileSetWidthTiles, TileSetHeightTiles;
    public THCollisionTile[][] CollisionTiles;

    public THTileMapGroup(BinaryReader r) {
        Name = r.ReadString();
        WorldLayerType = (THWorldLayerType)Enum.Parse(typeof(THWorldLayerType), Name, true);
        WidthTiles = r.ReadInt32(); HeightTiles = r.ReadInt32();
        PixelBounds = new(0f, 0f, WidthTiles * 16, HeightTiles * 16);
        TileSetWidthTiles = r.ReadInt32(); TileSetHeightTiles = r.ReadInt32();
        CollisionTiles = r.ReadFArray(z => r.ReadPArray<uint>("I", WidthTiles).Select(s => new THCollisionTile { Flags = (THTileFlags)s }).ToArray(), HeightTiles);
        // TODO
    }
}