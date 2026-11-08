// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using GameX.Origin.Structs.UO;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.Origin.UO;

/// <summary>One tile the editor is about to place. Offsets are relative to the click.</summary>
public struct CustomBuildObject {
    public CustomBuildObject(ushort graphic) {
        Graphic = graphic;
        X = Y = Z = 0;
    }

    public ushort Graphic;
    public int X, Y, Z;
}

/// <summary>Plot corner. ClassicUO Point, without XNA.</summary>
public struct PlotPoint {
    public int X;
    public int Y;
}

/// <summary>
/// A world click. IsPiece is ClassicUO's place is Multi.
/// There is no GameObject in this tree.
/// </summary>
public struct CustomHouseHit {
    public int X;
    public int Y;
    public int Z;
    public ushort Graphic;
    public bool IsPiece;
}

public abstract class CustomHouseObject {
    public int Category;
    public int FeatureMask;

    public virtual bool Parse(string text) => false;

    public abstract int Contains(ushort graphic);
}

public abstract class CustomHouseObjectCategory<T> where T : CustomHouseObject {
    public int Index;
    public List<T> Items = new List<T>();
}

public class CustomHouseWall : CustomHouseObject {
    public const int GRAPHICS_COUNT = 8;
    public ushort[] Graphics = new ushort[GRAPHICS_COUNT];
    public int Style, TID, South1, South2, South3, Corner, East1, East2, East3, Post, WindowS, AltWindowS, WindowE, AltWindowE, SecondAltWindowS, SecondAltWindowE;
    public ushort[] WindowGraphics = new ushort[GRAPHICS_COUNT];

    public override int Contains(ushort graphic) {
        for (var i = 0; i < GRAPHICS_COUNT; i++)
            if (Graphics[i] == graphic || WindowGraphics[i] == graphic) return i;
        return -1;
    }

    public override bool Parse(string text) {
        var scanf = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var result = false;
        if (scanf.Length >= 18)
            result = int.TryParse(scanf[0], out Category) && int.TryParse(scanf[1], out Style) && int.TryParse(scanf[2], out TID) && int.TryParse(scanf[3], out South1) && int.TryParse(scanf[4], out South2) && int.TryParse(scanf[5], out South3) && int.TryParse(scanf[6], out Corner) && int.TryParse(scanf[7], out East1) && int.TryParse(scanf[8], out East2) && int.TryParse(scanf[9], out East3) && int.TryParse(scanf[10], out Post) && int.TryParse(scanf[11], out WindowS) && int.TryParse(scanf[12], out AltWindowS) && int.TryParse(scanf[13], out WindowE) && int.TryParse(scanf[14], out AltWindowE) && int.TryParse(scanf[15], out SecondAltWindowS) && int.TryParse(scanf[16], out SecondAltWindowE) && int.TryParse(scanf[17], out FeatureMask);
        if (result) {
            WindowGraphics[0] = Graphics[0] = (ushort)South1;
            WindowGraphics[1] = Graphics[1] = (ushort)South2;
            WindowGraphics[2] = Graphics[2] = (ushort)South3;
            WindowGraphics[3] = Graphics[3] = (ushort)Corner;
            WindowGraphics[4] = Graphics[4] = (ushort)East1;
            WindowGraphics[5] = Graphics[5] = (ushort)East2;
            WindowGraphics[6] = Graphics[6] = (ushort)East3;
            WindowGraphics[7] = Graphics[7] = (ushort)Post;
        }
        if (AltWindowE == 0 && WindowE != 0) {
            AltWindowE = WindowE;
            WindowE = 0;
        }
        if (WindowS != 0) WindowGraphics[0] = (ushort)WindowS;
        if (AltWindowS != 0) WindowGraphics[1] = (ushort)AltWindowS;
        if (SecondAltWindowS != 0) WindowGraphics[2] = (ushort)SecondAltWindowS;
        if (WindowE != 0) WindowGraphics[4] = (ushort)WindowE;
        if (AltWindowE != 0) WindowGraphics[5] = (ushort)AltWindowE;
        if (SecondAltWindowE != 0) WindowGraphics[6] = (ushort)SecondAltWindowE;
        return result;
    }
}

public class CustomHouseWallCategory : CustomHouseObjectCategory<CustomHouseWall> { }

public class CustomHouseFloor : CustomHouseObject {
    public const int GRAPHICS_COUNT = 16;
    public int F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16;
    public ushort[] Graphics = new ushort[GRAPHICS_COUNT];

    public override bool Parse(string text) {
        var scanf = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var result = false;
        if (scanf.Length >= 18)
            result = int.TryParse(scanf[0], out Category) && int.TryParse(scanf[1], out F1) && int.TryParse(scanf[2], out F2) && int.TryParse(scanf[3], out F3) && int.TryParse(scanf[4], out F4) && int.TryParse(scanf[5], out F5) && int.TryParse(scanf[6], out F6) && int.TryParse(scanf[7], out F7) && int.TryParse(scanf[8], out F8) && int.TryParse(scanf[9], out F9) && int.TryParse(scanf[10], out F10) && int.TryParse(scanf[11], out F11) && int.TryParse(scanf[12], out F12) && int.TryParse(scanf[13], out F13) && int.TryParse(scanf[14], out F14) && int.TryParse(scanf[15], out F15) && int.TryParse(scanf[16], out F16) && int.TryParse(scanf[17], out FeatureMask);
        if (result) {
            Graphics[0] = (ushort)F1;
            Graphics[1] = (ushort)F2;
            Graphics[2] = (ushort)F3;
            Graphics[3] = (ushort)F4;
            Graphics[4] = (ushort)F5;
            Graphics[5] = (ushort)F6;
            Graphics[6] = (ushort)F7;
            Graphics[7] = (ushort)F8;
            Graphics[8] = (ushort)F9;
            Graphics[9] = (ushort)F10;
            Graphics[10] = (ushort)F11;
            Graphics[11] = (ushort)F12;
            Graphics[12] = (ushort)F13;
            Graphics[13] = (ushort)F14;
            Graphics[14] = (ushort)F15;
            Graphics[15] = (ushort)F16;
        }
        return result;
    }

    public override int Contains(ushort graphic) {
        for (var i = 0; i < GRAPHICS_COUNT; i++)
            if (Graphics[i] == graphic) return i;
        return -1;
    }
}

public class CustomHouseRoof : CustomHouseObject {
    public const int GRAPHICS_COUNT = 16;
    public ushort[] Graphics = new ushort[GRAPHICS_COUNT];
    public int Style, TID, North, East, South, West, NSCrosspiece, EWCrosspiece, NDent, EDent, SDent, WDent, NTPiece, ETPiece, STPiece, WTPiece, XPiece, ExtraPiece;

    public override bool Parse(string text) {
        var scanf = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var result = false;
        if (scanf.Length >= 20)
            result = int.TryParse(scanf[0], out Category) && int.TryParse(scanf[1], out Style) && int.TryParse(scanf[2], out TID) && int.TryParse(scanf[3], out North) && int.TryParse(scanf[4], out East) && int.TryParse(scanf[5], out South) && int.TryParse(scanf[6], out West) && int.TryParse(scanf[7], out NSCrosspiece) && int.TryParse(scanf[8], out EWCrosspiece) && int.TryParse(scanf[9], out NDent) && int.TryParse(scanf[10], out EDent) && int.TryParse(scanf[11], out SDent) && int.TryParse(scanf[12], out WDent) && int.TryParse(scanf[13], out NTPiece) && int.TryParse(scanf[14], out ETPiece) && int.TryParse(scanf[15], out STPiece) && int.TryParse(scanf[16], out WTPiece) && int.TryParse(scanf[17], out XPiece) && int.TryParse(scanf[18], out ExtraPiece) && int.TryParse(scanf[19], out FeatureMask);
        if (result) {
            Graphics[0] = (ushort)North;
            Graphics[1] = (ushort)East;
            Graphics[2] = (ushort)South;
            Graphics[3] = (ushort)West;
            Graphics[4] = (ushort)NSCrosspiece;
            Graphics[5] = (ushort)EWCrosspiece;
            Graphics[6] = (ushort)NDent;
            Graphics[7] = (ushort)EDent;
            Graphics[8] = (ushort)SDent;
            Graphics[9] = (ushort)WDent;
            Graphics[10] = (ushort)NTPiece;
            Graphics[11] = (ushort)ETPiece;
            Graphics[12] = (ushort)STPiece;
            Graphics[13] = (ushort)WTPiece;
            Graphics[14] = (ushort)XPiece;
            Graphics[15] = (ushort)ExtraPiece;
        }
        return result;
    }

    public override int Contains(ushort graphic) {
        for (var i = 0; i < GRAPHICS_COUNT; i++)
            if (Graphics[i] == graphic) return i;
        return -1;
    }
}

public class CustomHouseRoofCategory : CustomHouseObjectCategory<CustomHouseRoof> { }

public class CustomHouseMisc : CustomHouseObject {
    public const int GRAPHICS_COUNT = 8;
    public ushort[] Graphics = new ushort[GRAPHICS_COUNT];
    public int Style, TID, Piece1, Piece2, Piece3, Piece4, Piece5, Piece6, Piece7, Piece8;

    public override bool Parse(string text) {
        var scanf = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var result = false;
        if (scanf.Length >= 12)
            result = int.TryParse(scanf[0], out Category) && int.TryParse(scanf[1], out Style) && int.TryParse(scanf[2], out TID) && int.TryParse(scanf[3], out Piece1) && int.TryParse(scanf[4], out Piece2) && int.TryParse(scanf[5], out Piece3) && int.TryParse(scanf[6], out Piece4) && int.TryParse(scanf[7], out Piece5) && int.TryParse(scanf[8], out Piece6) && int.TryParse(scanf[9], out Piece7) && int.TryParse(scanf[10], out Piece8) && int.TryParse(scanf[11], out FeatureMask);
        if (result) {
            Graphics[0] = (ushort)Piece1;
            Graphics[1] = (ushort)Piece2;
            Graphics[2] = (ushort)Piece3;
            Graphics[3] = (ushort)Piece4;
            Graphics[4] = (ushort)Piece5;
            Graphics[5] = (ushort)Piece6;
            Graphics[6] = (ushort)Piece7;
            Graphics[7] = (ushort)Piece8;
        }
        return result;
    }

    public override int Contains(ushort graphic) {
        for (var i = 0; i < GRAPHICS_COUNT; i++)
            if (Graphics[i] == graphic) return i;
        return -1;
    }
}

public class CustomHouseMiscCategory : CustomHouseObjectCategory<CustomHouseMisc> { }

public class CustomHouseDoor : CustomHouseObject {
    public const int GRAPHICS_COUNT = 8;
    public ushort[] Graphics = new ushort[GRAPHICS_COUNT];
    public int Piece1, Piece2, Piece3, Piece4, Piece5, Piece6, Piece7, Piece8;

    public override bool Parse(string text) {
        var scanf = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var result = false;
        if (scanf.Length >= 10)
            result = int.TryParse(scanf[0], out Category) && int.TryParse(scanf[1], out Piece1) && int.TryParse(scanf[2], out Piece2) && int.TryParse(scanf[3], out Piece3) && int.TryParse(scanf[4], out Piece4) && int.TryParse(scanf[5], out Piece5) && int.TryParse(scanf[6], out Piece6) && int.TryParse(scanf[7], out Piece7) && int.TryParse(scanf[8], out Piece8) && int.TryParse(scanf[9], out FeatureMask);
        if (result) {
            Graphics[0] = (ushort)Piece1;
            Graphics[1] = (ushort)Piece2;
            Graphics[2] = (ushort)Piece3;
            Graphics[3] = (ushort)Piece4;
            Graphics[4] = (ushort)Piece5;
            Graphics[5] = (ushort)Piece6;
            Graphics[6] = (ushort)Piece7;
            Graphics[7] = (ushort)Piece8;
        }
        return result;
    }

    public override int Contains(ushort graphic) {
        for (var i = 0; i < GRAPHICS_COUNT; i++)
            if (Graphics[i] == graphic) return i;
        return -1;
    }
}

public class CustomHouseTeleport : CustomHouseObject {
    public const int GRAPHICS_COUNT = 16;
    public int F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16;
    public ushort[] Graphics = new ushort[GRAPHICS_COUNT];

    public override bool Parse(string text) {
        var scanf = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var result = false;
        if (scanf.Length >= 18)
            result = int.TryParse(scanf[0], out Category) && int.TryParse(scanf[1], out F1) && int.TryParse(scanf[2], out F2) && int.TryParse(scanf[3], out F3) && int.TryParse(scanf[4], out F4) && int.TryParse(scanf[5], out F5) && int.TryParse(scanf[6], out F6) && int.TryParse(scanf[7], out F7) && int.TryParse(scanf[8], out F8) && int.TryParse(scanf[9], out F9) && int.TryParse(scanf[10], out F10) && int.TryParse(scanf[11], out F11) && int.TryParse(scanf[12], out F12) && int.TryParse(scanf[13], out F13) && int.TryParse(scanf[14], out F14) && int.TryParse(scanf[15], out F15) && int.TryParse(scanf[16], out F16) && int.TryParse(scanf[17], out FeatureMask);
        if (result) {
            Graphics[0] = (ushort)F1;
            Graphics[1] = (ushort)F2;
            Graphics[2] = (ushort)F3;
            Graphics[3] = (ushort)F4;
            Graphics[4] = (ushort)F5;
            Graphics[5] = (ushort)F6;
            Graphics[6] = (ushort)F7;
            Graphics[7] = (ushort)F8;
            Graphics[8] = (ushort)F9;
            Graphics[9] = (ushort)F10;
            Graphics[10] = (ushort)F11;
            Graphics[11] = (ushort)F12;
            Graphics[12] = (ushort)F13;
            Graphics[13] = (ushort)F14;
            Graphics[14] = (ushort)F15;
            Graphics[15] = (ushort)F16;
        }
        return result;
    }

    public override int Contains(ushort graphic) {
        for (var i = 0; i < GRAPHICS_COUNT; i++)
            if (Graphics[i] == graphic) return i;
        return -1;
    }
}

public class CustomHouseStair : CustomHouseObject {
    public const int GRAPHICS_COUNT = 9;
    public int Block, North, East, South, West, Squared1, Squared2, Rounded1, Rounded2, MultiNorth, MultiEast, MultiSouth, MultiWest;
    public ushort[] Graphics = new ushort[GRAPHICS_COUNT];

    public override bool Parse(string text) {
        var scanf = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var result = false;
        if (scanf.Length >= 15)
            result = int.TryParse(scanf[0], out Category) && int.TryParse(scanf[1], out Block) && int.TryParse(scanf[2], out North) && int.TryParse(scanf[3], out East) && int.TryParse(scanf[4], out South) && int.TryParse(scanf[5], out West) && int.TryParse(scanf[6], out Squared1) && int.TryParse(scanf[7], out Squared2) && int.TryParse(scanf[8], out Rounded1) && int.TryParse(scanf[9], out Rounded2) && int.TryParse(scanf[10], out MultiNorth) && int.TryParse(scanf[11], out MultiEast) && int.TryParse(scanf[12], out MultiSouth) && int.TryParse(scanf[13], out MultiWest) && int.TryParse(scanf[14], out FeatureMask);
        if (result) {
            Graphics[0] = (ushort)(MultiNorth != 0 ? Squared1 : 0);
            Graphics[1] = (ushort)(MultiEast != 0 ? Squared2 : 0);
            Graphics[2] = (ushort)(MultiSouth != 0 ? Rounded1 : 0);
            Graphics[3] = (ushort)(MultiWest != 0 ? Rounded2 : 0);
            Graphics[4] = (ushort)Block;
            Graphics[5] = (ushort)North;
            Graphics[6] = (ushort)East;
            Graphics[7] = (ushort)South;
            Graphics[8] = (ushort)West;
        }
        return result;
    }

    public override int Contains(ushort graphic) {
        for (var i = 0; i < GRAPHICS_COUNT; i++)
            if (Graphics[i] == graphic) return i;
        return -1;
    }
}

public class CustomHousePlaceInfo : CustomHouseObject {
    public const int GRAPHICS_COUNT = 1;
    public int Graphic, Top, Bottom, AdjUN, AdjLN, AdjUE, AdjLE, AdjUS, AdjLS, AdjUW, AdjLW, DirectSupports, CanGoW, CanGoN, CanGoNWS;
    public ushort[] Graphics = new ushort[GRAPHICS_COUNT];

    public override bool Parse(string text) {
        var scanf = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var result = false;
        if (scanf.Length >= 16)
            result = int.TryParse(scanf[1], out Graphic) && int.TryParse(scanf[2], out Top) && int.TryParse(scanf[3], out Bottom) && int.TryParse(scanf[4], out AdjUN) && int.TryParse(scanf[5], out AdjLN) && int.TryParse(scanf[6], out AdjUE) && int.TryParse(scanf[7], out AdjLE) && int.TryParse(scanf[8], out AdjUS) && int.TryParse(scanf[9], out AdjLS) && int.TryParse(scanf[10], out AdjUW) && int.TryParse(scanf[11], out AdjLW) && int.TryParse(scanf[12], out DirectSupports) && int.TryParse(scanf[13], out CanGoW) && int.TryParse(scanf[14], out CanGoN) && int.TryParse(scanf[15], out CanGoNWS);
        if (result) Graphics[0] = (ushort)Graphic;
        return result;
    }

    public override int Contains(ushort graphic) {
        for (var i = 0; i < GRAPHICS_COUNT; i++)
            if (Graphics[i] == graphic) return i;
        return -1;
    }
}

public enum CUSTOM_HOUSE_GUMP_STATE {
    CHGS_WALL = 0,
    CHGS_DOOR,
    CHGS_FLOOR,
    CHGS_STAIR,
    CHGS_ROOF,
    CHGS_MISC,
    CHGS_MENU,
    CHGS_FIXTURE
}

public enum CUSTOM_HOUSE_FLOOR_VISION_STATE {
    CHGVS_NORMAL = 0,
    CHGVS_TRANSPARENT_CONTENT,
    CHGVS_HIDE_CONTENT,
    CHGVS_TRANSPARENT_FLOOR,
    CHGVS_HIDE_FLOOR,
    CHGVS_TRANSLUCENT_FLOOR,
    CHGVS_HIDE_ALL
}

public enum CUSTOM_HOUSE_BUILD_TYPE {
    CHBT_NORMAL = 0,
    CHBT_ROOF,
    CHBT_FLOOR,
    CHBT_STAIR
}

[Flags]
public enum CUSTOM_HOUSE_MULTI_OBJECT_FLAGS {
    CHMOF_GENERIC_INTERNAL = 0x01,
    CHMOF_FLOOR = 0x02,
    CHMOF_STAIR = 0x04,
    CHMOF_ROOF = 0x08,
    CHMOF_FIXTURE = 0x10,
    CHMOF_TRANSPARENT = 0x20,
    CHMOF_IGNORE_IN_RENDER = 0x40,
    CHMOF_VALIDATED_PLACE = 0x80,
    CHMOF_INCORRECT_PLACE = 0x100,
    CHMOF_DONT_REMOVE = 0x200,
    CHMOF_PREVIEW = 0x400
}

/// <summary>
/// ClassicUO house editor. Category lists come from the client txt files.
/// Placed pieces stay on this serial. Place and erase send 0xD7. Backup sends the same player serial the gump uses.
/// There is no House type. TryGetHouse returns this serial.
/// </summary>
public sealed class HouseCustomizationManager {
    /// <summary>One placed piece. ClassicUO Multi, stored here instead of a House.</summary>
    sealed class Component {
        public ushort Graphic;
        public ushort Hue;
        public int X, Y, Z;
        public bool IsCustom;
        public bool IsMovable;
        public bool IsDestroyed;
        public ushort AlphaHue;
        public CUSTOM_HOUSE_MULTI_OBJECT_FLAGS State;
        public void Destroy() { IsDestroyed = true; }
        public void AddToTile() { }
    }

    struct Rect {
        public int X, Y, Width, Height;
        public Rect(int x, int y, int width, int height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Contains(int x, int y) => X <= x && x < X + Width && Y <= y && y < Y + Height;
    }

    struct DesignCell {
        public ushort Floor;
        public ushort Object;
        public ushort Roof;
        public bool FloorLegal;
        public bool RoofLegal;
        public byte ObjectLegal;
        public bool Visited;
        public bool Support;
        public bool FloorSupport;
        public bool RoofSupport;
        public bool Locked;
    }

    struct MultiBlock {
        public ushort Id;
        public short X, Y, Z;
        public bool Visible;
    }

    public readonly List<CustomHouseWallCategory> Walls = new List<CustomHouseWallCategory>();
    public readonly List<CustomHouseFloor> Floors = new List<CustomHouseFloor>();
    public readonly List<CustomHouseDoor> Doors = new List<CustomHouseDoor>();
    public readonly List<CustomHouseMiscCategory> Miscs = new List<CustomHouseMiscCategory>();
    public readonly List<CustomHouseStair> Stairs = new List<CustomHouseStair>();
    public readonly List<CustomHouseTeleport> Teleports = new List<CustomHouseTeleport>();
    public readonly List<CustomHouseRoofCategory> Roofs = new List<CustomHouseRoofCategory>();
    public readonly List<CustomHousePlaceInfo> ObjectsInfo = new List<CustomHousePlaceInfo>();

    readonly World _world;
    readonly List<Component> _pieces = new List<Component>();
    Rect _bounds;
    CustomHouseHit _hit;
    bool _hitSet;
    static Binary_TileData.ItemData[] _tiles;
    static bool _tilesMiss;

    public HouseCustomizationManager(World world, uint serial) {
        _world = world;
        Serial = serial;
        ParseFileWithCategory<CustomHouseWall, CustomHouseWallCategory>(Walls, UoPath("walls.txt"));
        ParseFile(Floors, UoPath("floors.txt"));
        ParseFile(Doors, UoPath("doors.txt"));
        ParseFileWithCategory<CustomHouseMisc, CustomHouseMiscCategory>(Miscs, UoPath("misc.txt"));
        ParseFile(Stairs, UoPath("stairs.txt"));
        ParseFile(Teleports, UoPath("teleprts.txt"));
        ParseFileWithCategory<CustomHouseRoof, CustomHouseRoofCategory>(Roofs, UoPath("roof.txt"));
        ParseFile(ObjectsInfo, UoPath("suppinfo.txt"));
        InitializeHouse();
    }

    public int Category = -1, MaxPage = 1, CurrentFloor = 1, FloorCount = 4, RoofZ = 1, MinHouseZ = -120, Components, Fixtures, MaxComponets, MaxFixtures;
    public bool Erasing, SeekTile, ShowWindow, CombinedStair;
    public readonly int[] FloorVisionState = new int[4];
    public ushort SelectedGraphic;
    public readonly uint Serial;
    public PlotPoint StartPos, EndPos;
    public CUSTOM_HOUSE_GUMP_STATE State = CUSTOM_HOUSE_GUMP_STATE.CHGS_WALL;

    /// <summary>ClassicUO TryGetHouse. The house is this serial, or 0 when it is a different one.</summary>
    public uint TryGetHouse(uint serial) => serial == Serial ? Serial : 0;

    void InitializeHouse() {
        var foundation = Foundation();
        if (foundation == null) return;
        MinHouseZ = foundation.Z + 7;
        var blocks = new List<MultiBlock>();
        if (ReadMulti(foundation.Graphic, blocks)) {
            short minX = 0, minY = 0, maxX = 0, maxY = 0;
            for (var i = 0; i < blocks.Count; i++) {
                var block = blocks[i];
                if (block.X < minX) minX = block.X;
                if (block.X > maxX) maxX = block.X;
                if (block.Y < minY) minY = block.Y;
                if (block.Y > maxY) maxY = block.Y;
            }
            StartPos.X = foundation.X + minX + 1;
            StartPos.Y = foundation.Y + minY + 1;
            EndPos.X = foundation.X + maxX + 1;
            EndPos.Y = foundation.Y + maxY + 1;
            for (var i = 0; i < blocks.Count; i++) {
                var block = blocks[i];
                if (!block.Visible) continue;
                var piece = Add(block.Id, 0, (ushort)(foundation.X + block.X), (ushort)(foundation.Y + block.Y), (sbyte)(foundation.Z + block.Z), false, false);
                piece.AlphaHue = 255;
                piece.State = CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_DONT_REMOVE;
            }
        }
        var width = Math.Abs(EndPos.X - StartPos.X);
        var height = Math.Abs(EndPos.Y - StartPos.Y);
        _bounds = new Rect(StartPos.X - 1, StartPos.Y - 1, width + 1, height + 2);
        FloorCount = width >= 13 || height >= 13 ? 4 : 3;
        var plotWidth = width + 1;
        var plotHeight = height + 1;
        var componentsOnFloor = (plotWidth - 1) * (plotHeight - 1);
        MaxComponets = FloorCount * (componentsOnFloor + 2 * (plotWidth + plotHeight) - 4) - (int)(FloorCount * componentsOnFloor * -0.25) + 2 * plotWidth + 3 * plotHeight - 5;
        MaxFixtures = MaxComponets / 20;
    }

    public void GenerateFloorPlace() {
        var foundationItem = Foundation();
        if (foundationItem == null || TryGetHouse(Serial) == 0) return;
        ClearCustomHouseComponents(CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL);
        for (var n = 0; n < _pieces.Count; n++) {
            var item = _pieces[n];
            if (!item.IsCustom) continue;
            var currentFloor = -1;
            var floorZ = foundationItem.Z + 7;
            var itemZ = item.Z;
            var ignore = false;
            for (var i = 0; i < 4; i++) {
                var offset = 0;
                if (itemZ >= floorZ - offset && itemZ < floorZ + 20) {
                    currentFloor = i;
                    break;
                }
                floorZ += 20;
            }
            if (currentFloor == -1) {
                ignore = true;
                currentFloor = 0;
            }
            var state = item.State;
            var floorCheck = SeekGraphicInCustomHouseObjectList(Floors, item.Graphic);
            if (floorCheck.Item1 != -1 && floorCheck.Item2 != -1) {
                state |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR;
                if (FloorVisionState[currentFloor] == (int)CUSTOM_HOUSE_FLOOR_VISION_STATE.CHGVS_HIDE_FLOOR)
                    state |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER;
                else if (FloorVisionState[currentFloor] == (int)CUSTOM_HOUSE_FLOOR_VISION_STATE.CHGVS_TRANSPARENT_FLOOR
                         || FloorVisionState[currentFloor] == (int)CUSTOM_HOUSE_FLOOR_VISION_STATE.CHGVS_TRANSLUCENT_FLOOR)
                    state |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_TRANSPARENT;
            }
            else {
                var stairCheck = SeekGraphicInCustomHouseObjectList(Stairs, item.Graphic);
                if (stairCheck.Item1 != -1 && stairCheck.Item2 != -1) state |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_STAIR;
                else {
                    var roofCheck = SeekGraphicInCustomHouseObjectListWithCategory<CustomHouseRoof, CustomHouseRoofCategory>(Roofs, item.Graphic);
                    if (roofCheck.Item1 != -1 && roofCheck.Item2 != -1) state |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_ROOF;
                    else {
                        var fixtureCheck = SeekGraphicInCustomHouseObjectList(Doors, item.Graphic);
                        if (fixtureCheck.Item1 == -1 || fixtureCheck.Item2 == -1) {
                            fixtureCheck = SeekGraphicInCustomHouseObjectList(Teleports, item.Graphic);
                            if (fixtureCheck.Item1 != -1 && fixtureCheck.Item2 != -1)
                                state |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR;
                        }
                        else state |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FIXTURE;
                    }
                }
                if (!ignore) {
                    if (FloorVisionState[currentFloor] == (int)CUSTOM_HOUSE_FLOOR_VISION_STATE.CHGVS_HIDE_CONTENT)
                        state |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER;
                    else if (FloorVisionState[currentFloor] == (int)CUSTOM_HOUSE_FLOOR_VISION_STATE.CHGVS_TRANSPARENT_CONTENT)
                        state |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_TRANSPARENT;
                }
            }
            if (!ignore && FloorVisionState[currentFloor] == (int)CUSTOM_HOUSE_FLOOR_VISION_STATE.CHGVS_HIDE_ALL)
                state |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER;
            item.State = state;
        }
        var z = foundationItem.Z + 7;
        for (var x = StartPos.X + 1; x < EndPos.X; x++) {
            for (var y = StartPos.Y + 1; y < EndPos.Y; y++) {
                var multi = At(x, y);
                if (multi == null) continue;
                Component floorMulti = null;
                Component floorCustomMulti = null;
                for (var i = 0; i < multi.Count; i++) {
                    var item = multi[i];
                    if (item.Z != z || (item.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR) == 0) continue;
                    if (item.IsCustom) floorCustomMulti = item;
                    else floorMulti = item;
                }
                if (floorMulti != null && floorCustomMulti == null) {
                    var mo = Add(floorMulti.Graphic, 0, (ushort)(foundationItem.X + (x - foundationItem.X)), (ushort)(foundationItem.Y + (y - foundationItem.Y)), (sbyte)z, true, false);
                    mo.AlphaHue = 0xFF;
                    var state = CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL;
                    if (FloorVisionState[0] == (int)CUSTOM_HOUSE_FLOOR_VISION_STATE.CHGVS_HIDE_FLOOR)
                        state |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER;
                    else if (FloorVisionState[0] == (int)CUSTOM_HOUSE_FLOOR_VISION_STATE.CHGVS_TRANSPARENT_FLOOR
                             || FloorVisionState[0] == (int)CUSTOM_HOUSE_FLOOR_VISION_STATE.CHGVS_TRANSLUCENT_FLOOR)
                        state |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_TRANSPARENT;
                    mo.State = state;
                }
            }
        }
        ValidateDesignGrid(foundationItem, Serial);
        z = foundationItem.Z + 7;
        ushort color = 0x0051;
        for (var i = 0; i < CurrentFloor; i++) {
            for (var x = _bounds.X; x < EndPos.X; x++) {
                for (var y = _bounds.Y; y < EndPos.Y; y++) {
                    var mo = Add(0x0496, (ushort)(x == _bounds.X || y == _bounds.Y ? 0x34 : color), (ushort)(foundationItem.X + (x - foundationItem.X)), (ushort)(foundationItem.Y + (y - foundationItem.Y)), (sbyte)z, true, false);
                    mo.AlphaHue = 0xFF;
                    mo.State = CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_TRANSPARENT;
                    mo.AddToTile();
                }
            }
            color += 5;
            z += 20;
        }
    }

    public void OnTargetWorld(CustomHouseHit? place) {
        if (place == null) return;
        var at = place.Value;
        if (!_bounds.Contains(at.X, at.Y)) return;
        var zOffset = -3;
        var gump = UIManager.GetGump<HouseCustomizationGump>(Serial);
        if (CurrentFloor == 1) zOffset = -7;
        if (SeekTile) {
            if (at.IsPiece) SeekGraphic(at.Graphic);
        }
        else if (at.Z >= UOLoginNet.PlayerZ + zOffset && at.Z < UOLoginNet.PlayerZ + 20) {
            var foundationItem = Foundation();
            if (foundationItem == null || TryGetHouse(Serial) == 0) return;
            if (Erasing) {
                if (!at.IsPiece) return;
                if (CanEraseHere(at, out var type)) {
                    var multi = GetMultiAt(at.X, at.Y);
                    if (multi == null || multi.Count == 0) return;
                    var z = 7 + (CurrentFloor - 1) * 20;
                    if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR || type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_ROOF)
                        z = at.Z - (foundationItem.Z + z) + z;
                    if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR) {
                        var floorBase = foundationItem.Z;
                        var stairFloorBase = floorBase;
                        for (var f = 0; f < FloorCount; f++) {
                            var fz = floorBase + 7 + f * 20;
                            if (at.Z >= fz && at.Z < fz + 20) {
                                stairFloorBase = fz;
                                break;
                            }
                        }
                        if (at.Z < floorBase + 7) stairFloorBase = floorBase;
                        var sameXPieces = new List<Component>();
                        var sameYPieces = new List<Component>();
                        for (var i = 0; i < _pieces.Count; i++) {
                            var comp = _pieces[i];
                            if (comp.IsDestroyed || !comp.IsCustom) continue;
                            if ((comp.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_STAIR) == 0) continue;
                            if (comp.Z < stairFloorBase || comp.Z >= stairFloorBase + 20) continue;
                            if (comp.X == at.X) sameXPieces.Add(comp);
                            if (comp.Y == at.Y) sameYPieces.Add(comp);
                        }
                        var stairPieces = new List<Component>();
                        if (sameXPieces.Count >= sameYPieces.Count && sameXPieces.Count > 0) {
                            var bestCount = 0;
                            var bestStart = at.Y;
                            for (var startY = at.Y - 3; startY <= at.Y; startY++) {
                                var count = 0;
                                for (var i = 0; i < sameXPieces.Count; i++)
                                    if (sameXPieces[i].Y >= startY && sameXPieces[i].Y <= startY + 3) count++;
                                if (count > bestCount) {
                                    bestCount = count;
                                    bestStart = startY;
                                }
                            }
                            for (var i = 0; i < sameXPieces.Count; i++)
                                if (sameXPieces[i].Y >= bestStart && sameXPieces[i].Y <= bestStart + 3) stairPieces.Add(sameXPieces[i]);
                        }
                        else if (sameYPieces.Count > 0) {
                            var bestCount = 0;
                            var bestStart = at.X;
                            for (var startX = at.X - 3; startX <= at.X; startX++) {
                                var count = 0;
                                for (var i = 0; i < sameYPieces.Count; i++)
                                    if (sameYPieces[i].X >= startX && sameYPieces[i].X <= startX + 3) count++;
                                if (count > bestCount) {
                                    bestCount = count;
                                    bestStart = startX;
                                }
                            }
                            for (var i = 0; i < sameYPieces.Count; i++)
                                if (sameYPieces[i].X >= bestStart && sameYPieces[i].X <= bestStart + 3) stairPieces.Add(sameYPieces[i]);
                        }
                        var isCombined = false;
                        if (stairPieces.Count > 1) {
                            var firstZ = stairPieces[0].Z;
                            for (var i = 1; i < stairPieces.Count; i++) {
                                if (stairPieces[i].Z != firstZ) {
                                    isCombined = true;
                                    break;
                                }
                            }
                        }
                        if (isCombined) {
                            for (var i = 0; i < stairPieces.Count; i++) {
                                var piece = stairPieces[i];
                                var pz = piece.Z - (foundationItem.Z + (7 + (CurrentFloor - 1) * 20)) + (7 + (CurrentFloor - 1) * 20);
                                UOLoginNet.Send(OutgoingPackets.Send_CustomHouseDeleteItem(UOLoginNet.PlayerSerial, piece.Graphic, piece.X - foundationItem.X, piece.Y - foundationItem.Y, pz));
                                piece.Destroy();
                            }
                        }
                        else {
                            UOLoginNet.Send(OutgoingPackets.Send_CustomHouseDeleteItem(UOLoginNet.PlayerSerial, at.Graphic, at.X - foundationItem.X, at.Y - foundationItem.Y, z));
                            Find(at)?.Destroy();
                        }
                    }
                    else if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_ROOF) {
                        UOLoginNet.Send(OutgoingPackets.Send_CustomHouseDeleteRoof(UOLoginNet.PlayerSerial, at.Graphic, at.X - foundationItem.X, at.Y - foundationItem.Y, z));
                        Find(at)?.Destroy();
                    }
                    else {
                        UOLoginNet.Send(OutgoingPackets.Send_CustomHouseDeleteItem(UOLoginNet.PlayerSerial, at.Graphic, at.X - foundationItem.X, at.Y - foundationItem.Y, z));
                        Find(at)?.Destroy();
                    }
                }
            }
            else if (SelectedGraphic != 0) {
                var list = new List<CustomBuildObject>();
                _hit = at;
                _hitSet = true;
                if (CanBuildHere(list, out var type) && list.Count > 0) {
                    var placeX = at.X;
                    var placeY = at.Y;
                    if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR && CombinedStair) {
                        if (gump != null && gump.Page >= 0 && gump.Page < Stairs.Count) {
                            var stair = Stairs[gump.Page];
                            ushort graphic = 0;
                            if (SelectedGraphic == stair.North) graphic = (ushort)stair.MultiNorth;
                            else if (SelectedGraphic == stair.East) graphic = (ushort)stair.MultiEast;
                            else if (SelectedGraphic == stair.South) graphic = (ushort)stair.MultiSouth;
                            else if (SelectedGraphic == stair.West) graphic = (ushort)stair.MultiWest;
                            if (graphic != 0)
                                UOLoginNet.Send(OutgoingPackets.Send_CustomHouseAddStair(UOLoginNet.PlayerSerial, graphic, placeX - foundationItem.X, placeY - foundationItem.Y));
                        }
                    }
                    else {
                        var item = list[0];
                        var x = placeX - foundationItem.X + item.X;
                        var y = placeY - foundationItem.Y + item.Y;
                        var multi = GetMultiAt(placeX + item.X, placeY + item.Y);
                        if (multi.Count > 0 || type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR) {
                            if (!CombinedStair) {
                                var minZ = foundationItem.Z + 7 + (CurrentFloor - 1) * 20;
                                var maxZ = minZ + 20;
                                if (CurrentFloor == 1) minZ -= 7;
                                for (var i = 0; i < multi.Count; i++) {
                                    var multiObject = multi[i];
                                    var testMinZ = minZ;
                                    if ((multiObject.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_ROOF) != 0) testMinZ -= 3;
                                    if (multiObject.Z < testMinZ || multiObject.Z >= maxZ || !multiObject.IsCustom || (multiObject.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL) != 0)
                                        continue;
                                    if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR) {
                                        if ((multiObject.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_STAIR) != 0) multiObject.Destroy();
                                    }
                                    else if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_ROOF) {
                                        if ((multiObject.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_ROOF) != 0) multiObject.Destroy();
                                    }
                                    else if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_FLOOR) {
                                        if ((multiObject.State & (CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FIXTURE)) != 0) multiObject.Destroy();
                                    }
                                    else if ((multiObject.State & (CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_STAIR | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_ROOF | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_DONT_REMOVE)) == 0)
                                        multiObject.Destroy();
                                }
                            }
                            if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_ROOF)
                                UOLoginNet.Send(OutgoingPackets.Send_CustomHouseAddRoof(UOLoginNet.PlayerSerial, item.Graphic, x, y, item.Z));
                            else
                                UOLoginNet.Send(OutgoingPackets.Send_CustomHouseAddItem(UOLoginNet.PlayerSerial, item.Graphic, x, y));
                        }
                    }
                    var xx = placeX - foundationItem.X;
                    var yy = placeY - foundationItem.Y;
                    var z = foundationItem.Z + 7 + (CurrentFloor - 1) * 20;
                    if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR && !CombinedStair) z = foundationItem.Z;
                    for (var i = 0; i < list.Count; i++) {
                        var item = list[i];
                        Add(item.Graphic, 0, (ushort)(foundationItem.X + xx + item.X), (ushort)(foundationItem.Y + yy + item.Y), (sbyte)(z + item.Z), true, false);
                    }
                }
            }
            GenerateFloorPlace();
            gump?.Update();
        }
    }

    void SeekGraphic(ushort graphic) {
        CUSTOM_HOUSE_GUMP_STATE state = 0;
        var found = ExistsInList(ref state, graphic);
        if (found.Item1 == -1 || found.Item2 == -1) return;
        State = state;
        var grouped = State == CUSTOM_HOUSE_GUMP_STATE.CHGS_WALL || State == CUSTOM_HOUSE_GUMP_STATE.CHGS_ROOF || State == CUSTOM_HOUSE_GUMP_STATE.CHGS_MISC;
        if (grouped) Category = found.Item1;
        else Category = -1;
        var gump = UIManager.GetGump<HouseCustomizationGump>(Serial);
        if (gump != null) gump.Page = grouped ? found.Item2 : found.Item1;
        UpdateMaxPage();
        SetTargetMulti();
        SelectedGraphic = graphic;
        gump?.Update();
    }

    public void SetTargetMulti() {
        Erasing = false;
        SeekTile = false;
        SelectedGraphic = 0;
        CombinedStair = false;
    }

    /// <summary>0xD7 subcommand 0x02. The serial is the player, same as the gump row.</summary>
    public void Backup() => UOLoginNet.Send(OutgoingPackets.Send_CustomHouseBackup(UOLoginNet.PlayerSerial));

    public bool CanBuildHere(List<CustomBuildObject> list, out CUSTOM_HOUSE_BUILD_TYPE type) {
        type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_NORMAL;
        if (SelectedGraphic == 0) return false;
        var foundationItem = Foundation();
        if (foundationItem == null || TryGetHouse(foundationItem.Serial) == 0) return false;
        Recount();
        var result = true;
        if (CombinedStair) {
            if (Components + 10 > MaxComponets || CurrentFloor >= FloorCount) return false;
            var res = SeekGraphicInCustomHouseObjectList(Stairs, SelectedGraphic);
            if (res.Item1 == -1 || res.Item2 == -1 || res.Item1 >= Stairs.Count) {
                list.Add(new CustomBuildObject { Graphic = SelectedGraphic, X = 0, Y = 0, Z = 0 });
                return false;
            }
            var item = Stairs[res.Item1];
            if (SelectedGraphic == item.North) {
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 0, Y = -3, Z = 0 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 0, Y = -2, Z = 0 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 0, Y = -1, Z = 0 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.North, X = 0, Y = 0, Z = 0 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 0, Y = -3, Z = 5 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 0, Y = -2, Z = 5 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.North, X = 0, Y = -1, Z = 5 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 0, Y = -3, Z = 10 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.North, X = 0, Y = -2, Z = 10 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.North, X = 0, Y = -3, Z = 15 });
            }
            else if (SelectedGraphic == item.East) {
                list.Add(new CustomBuildObject { Graphic = (ushort)item.East, X = 0, Y = 0, Z = 0 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 1, Y = 0, Z = 0 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 2, Y = 0, Z = 0 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 3, Y = 0, Z = 0 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.East, X = 1, Y = 0, Z = 5 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 2, Y = 0, Z = 5 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 3, Y = 0, Z = 5 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.East, X = 2, Y = 0, Z = 10 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 3, Y = 0, Z = 10 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.East, X = 3, Y = 0, Z = 15 });
            }
            else if (SelectedGraphic == item.South) {
                list.Add(new CustomBuildObject { Graphic = (ushort)item.South, X = 0, Y = 0, Z = 0 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 0, Y = 1, Z = 0 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 0, Y = 2, Z = 0 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 0, Y = 3, Z = 0 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.South, X = 0, Y = 1, Z = 5 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 0, Y = 2, Z = 5 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 0, Y = 3, Z = 5 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.South, X = 0, Y = 2, Z = 10 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = 0, Y = 3, Z = 10 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.South, X = 0, Y = 3, Z = 15 });
            }
            else if (SelectedGraphic == item.West) {
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = -3, Y = 0, Z = 0 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = -2, Y = 0, Z = 0 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = -1, Y = 0, Z = 0 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.West, X = 0, Y = 0, Z = 0 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = -3, Y = 0, Z = 5 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = -2, Y = 0, Z = 5 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.West, X = -1, Y = 0, Z = 5 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.Block, X = -3, Y = 0, Z = 10 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.West, X = -2, Y = 0, Z = 10 });
                list.Add(new CustomBuildObject { Graphic = (ushort)item.West, X = -3, Y = 0, Z = 15 });
            }
            else list.Add(new CustomBuildObject { Graphic = SelectedGraphic, X = 0, Y = 0, Z = 0 });
            type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR;
        }
        else {
            var fixCheck = SeekGraphicInCustomHouseObjectList(Doors, SelectedGraphic);
            var isFixture = false;
            if (fixCheck.Item1 == -1 || fixCheck.Item2 == -1) {
                fixCheck = SeekGraphicInCustomHouseObjectList(Teleports, SelectedGraphic);
                isFixture = fixCheck.Item1 != -1 && fixCheck.Item2 != -1;
                if (isFixture) type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_FLOOR;
            }
            else {
                isFixture = true;
                type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_NORMAL;
            }
            if (isFixture) {
                if (Fixtures + 1 > MaxFixtures) result = false;
            }
            else if (Components + 1 > MaxComponets) result = false;
            if (State == CUSTOM_HOUSE_GUMP_STATE.CHGS_ROOF) {
                list.Add(new CustomBuildObject { Graphic = SelectedGraphic, X = 0, Y = 0, Z = (RoofZ - 2) * 3 });
                type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_ROOF;
            }
            else if (State == CUSTOM_HOUSE_GUMP_STATE.CHGS_STAIR) {
                list.Add(new CustomBuildObject { Graphic = SelectedGraphic, X = 0, Y = 1, Z = 0 });
                type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR;
            }
            else {
                if (State == CUSTOM_HOUSE_GUMP_STATE.CHGS_FLOOR) type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_FLOOR;
                list.Add(new CustomBuildObject { Graphic = SelectedGraphic, X = 0, Y = 0, Z = 0 });
            }
        }
        if (_hitSet) {
            var gobj = _hit;
            if (!_bounds.Contains(gobj.X, gobj.Y)) return false;
            var minZ = foundationItem.Z + 0 + (CurrentFloor - 1) * 20;
            var maxZ = minZ + 20;
            for (var i = 0; i < list.Count; ++i) {
                var item = list[i];
                if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR) {
                    if (CombinedStair) {
                        if (item.Z != 0) continue;
                    }
                    else {
                        if (gobj.Y + item.Y < EndPos.Y || gobj.X + item.X == _bounds.X || gobj.Z >= MinHouseZ) return false;
                        if (gobj.Y + item.Y != EndPos.Y) {
                            item.Y = 0;
                            list[0] = item;
                        }
                        continue;
                    }
                }
                if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR && CombinedStair) {
                    var tileX = gobj.X + item.X;
                    var tileY = gobj.Y + item.Y;
                    if (tileX < StartPos.X || tileX >= EndPos.X || tileY < StartPos.Y || tileY >= EndPos.Y) return false;
                }
                else if (!ValidateItemPlace(_bounds, item.Graphic, gobj.X + item.X, gobj.Y + item.Y)) return false;
                if (type != CUSTOM_HOUSE_BUILD_TYPE.CHBT_FLOOR) {
                    var multi = GetMultiAt(gobj.X + item.X, gobj.Y + item.Y);
                    for (var m = 0; m < multi.Count; m++) {
                        var piece = multi[m];
                        if (!piece.IsCustom) continue;
                        var collisionMaxZ = type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR && CombinedStair ? maxZ + 20 : maxZ;
                        if ((piece.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL) == 0 && piece.Z >= minZ && piece.Z < collisionMaxZ) {
                            if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR) {
                                if ((piece.State & (CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_DONT_REMOVE)) == 0) return false;
                            }
                            else if ((piece.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_STAIR) != 0) return false;
                        }
                    }
                }
            }
        }
        return result;
    }

    public bool CanEraseHere(CustomHouseHit? place, out CUSTOM_HOUSE_BUILD_TYPE type) {
        type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_NORMAL;
        if (place != null && place.Value.IsPiece) {
            var multi = Find(place.Value);
            if (multi != null && multi.IsCustom && (multi.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL) == 0) {
                if ((multi.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR) != 0) type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_FLOOR;
                else if ((multi.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_STAIR) != 0) type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR;
                else if ((multi.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_ROOF) != 0) type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_ROOF;
                else if (_bounds.Contains(place.Value.X, place.Value.Y) && place.Value.Z >= MinHouseZ) { }
                else return false;
                return true;
            }
        }
        return false;
    }

    CustomHousePlaceInfo GetPlaceInfo(ushort graphic) {
        if (graphic == 0) return null;
        var found = SeekGraphicInCustomHouseObjectList(ObjectsInfo, graphic);
        return found.Item1 != -1 && found.Item2 != -1 ? ObjectsInfo[found.Item1] : null;
    }

    void ValidateDesignGrid(Item foundationItem, uint serial) {
        if (serial != Serial) return;
        var ox = _bounds.X - 1;
        var oy = _bounds.Y - 1;
        var w = _bounds.Width + 1;
        var h = _bounds.Height + 1;
        var levels = FloorCount;
        if (w < 1 || w > 32 || h < 1 || h > 32 || levels < 1 || levels > 4) return;
        var baseZ = foundationItem.Z + 7;
        var grid = new DesignCell[32, 32, levels];
        int CanGoW(ushort gfx) { var i = GetPlaceInfo(gfx); return i == null ? 0 : i.CanGoW; }
        int CanGoN(ushort gfx) { var i = GetPlaceInfo(gfx); return i == null ? 0 : i.CanGoN; }
        int CanGoNWS(ushort gfx) { var i = GetPlaceInfo(gfx); return i == null ? 0 : i.CanGoNWS; }
        int DirectSup(ushort gfx) { var i = GetPlaceInfo(gfx); return i == null ? 0 : i.DirectSupports; }
        int Bottom(ushort gfx) { var i = GetPlaceInfo(gfx); return i == null ? 0 : i.Bottom; }
        static bool InGrid(int x, int y) => (uint)x < 32 && (uint)y < 32;
        int SlotOf(ushort gfx) {
            if (!TryTile(gfx, out var td)) return 0;
            if (td.Height < 2 || (td.Flags & TileFlag.Surface) != 0) return 0;
            return (td.Flags & TileFlag.Roof) != 0 ? 2 : 1;
        }
        bool AdjPair(ushort me, ushort other, int dx, int dy) {
            if (me == 0 || other == 0) return false;
            var m = GetPlaceInfo(me);
            var o = GetPlaceInfo(other);
            if (m == null || o == null) return false;
            if (dx < 0) return (m.AdjUW != 0 && o.AdjUE != 0) || (m.AdjLW != 0 && o.AdjLE != 0);
            if (dx > 0) return (m.AdjUE != 0 && o.AdjUW != 0) || (m.AdjLE != 0 && o.AdjLW != 0);
            if (dy < 0) return (m.AdjUN != 0 && o.AdjUS != 0) || (m.AdjLN != 0 && o.AdjLS != 0);
            return (m.AdjUS != 0 && o.AdjUN != 0) || (m.AdjLS != 0 && o.AdjLN != 0);
        }
        bool EdgeGuard(int x, int y, int l) =>
            (x != 0 || ((y != 0 || CanGoNWS(grid[0, 0, l].Object) != 0) && CanGoW(grid[0, y, l].Object) != 0))
            && (y != 0 || CanGoN(grid[x, 0, l].Object) != 0);
        for (var n = 0; n < _pieces.Count; n++) {
            var mm = _pieces[n];
            if (!mm.IsCustom) continue;
            if ((mm.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL) != 0) continue;
            var gx = mm.X - ox;
            var gy = mm.Y - oy;
            if (!InGrid(gx, gy)) continue;
            var l = (mm.Z - baseZ) / 20;
            if (l < 0) l = 0;
            if (l >= levels) continue;
            var gr = mm.Graphic;
            switch (SlotOf(gr)) {
                case 0:
                    grid[gx, gy, l].Floor = gr;
                    if (gr >= 0x181D && gr < 0x1829) grid[gx, gy, l].Locked = true;
                    break;
                case 2:
                    grid[gx, gy, l].Roof = gr;
                    break;
                default:
                    grid[gx, gy, l].Object = gr;
                    break;
            }
        }
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };
        bool Run(int x, int y, int l, int sx, int sy, bool roof) {
            for (var step = 1; step < 0x13; step++) {
                var cx = x + sx * step;
                var cy = y + sy * step;
                if (!InGrid(cx, cy)) return false;
                if ((roof ? grid[cx, cy, l].Roof : grid[cx, cy, l].Floor) == 0) return false;
                if (grid[cx, cy, l].Support || (roof ? grid[cx, cy, l].RoofSupport : grid[cx, cy, l].FloorSupport)) {
                    for (var k = 1; k <= step; k++) {
                        if (roof) grid[x + sx * k, y + sy * k, l].RoofSupport = true;
                        else grid[x + sx * k, y + sy * k, l].FloorSupport = true;
                    }
                    return true;
                }
            }
            return false;
        }
        bool FloorRun(int x, int y, int l) =>
            Run(x, y, l, 0, 1, false) | Run(x, y, l, 1, 0, false) | Run(x, y, l, -1, 0, false) | Run(x, y, l, 0, -1, false);
        bool RoofRun(int x, int y, int l) =>
            Run(x, y, l, 0, 1, true) | Run(x, y, l, 1, 0, true) | Run(x, y, l, -1, 0, true) | Run(x, y, l, 0, -1, true);
        void Spread(int x, int y, int l) {
            grid[x, y, l].Support = true;
            ushort below = l >= 1 ? grid[x, y, l - 1].Object : (ushort)0;
            var handled = false;
            if (InGrid(x + 1, y) && CanGoW(below) != 0) {
                grid[x + 1, y, l].FloorSupport = true;
                grid[x + 1, y, l].RoofSupport = true;
                handled = true;
            }
            if (!handled && InGrid(x, y + 1) && CanGoN(below) != 0) {
                grid[x, y + 1, l].FloorSupport = true;
                grid[x, y + 1, l].RoofSupport = true;
            }
            if (grid[x, y, l].Floor != 0) {
                if (InGrid(x + 1, y)) grid[x + 1, y, l].FloorSupport = true;
                if (InGrid(x - 1, y)) grid[x - 1, y, l].FloorSupport = true;
                if (InGrid(x, y + 1)) grid[x, y + 1, l].FloorSupport = true;
                if (InGrid(x, y - 1)) grid[x, y - 1, l].FloorSupport = true;
            }
            if (grid[x, y, l].Roof != 0) {
                for (var nx = x - 1; nx <= x + 1; nx++)
                    for (var ny = y - 1; ny <= y + 1; ny++)
                        if ((nx != x || ny != y) && InGrid(nx, ny)) grid[nx, ny, l].RoofSupport = true;
            }
        }
        for (var l = 0; l < levels; l++) {
            for (var x = 0; x < w; x++) {
                for (var y = 0; y < h; y++) {
                    grid[x, y, l].Visited = false;
                    grid[x, y, l].Support = false;
                    grid[x, y, l].FloorSupport = false;
                    grid[x, y, l].RoofSupport = false;
                    grid[x, y, l].FloorLegal = false;
                    grid[x, y, l].RoofLegal = false;
                    grid[x, y, l].ObjectLegal = 0;
                    if (l == 0) {
                        grid[x, y, l].Support = true;
                        grid[x, y, l].FloorSupport = true;
                        grid[x, y, l].RoofSupport = true;
                    }
                }
            }
            for (var x = 0; x < w; x++) {
                for (var y = 0; y < h; y++) {
                    if (l == 0) {
                        grid[x, y, l].Visited = true;
                        grid[x, y, l].Support = true;
                        grid[x, y, l].FloorSupport = true;
                        grid[x, y, l].RoofSupport = true;
                    }
                    else if (DirectSup(grid[x, y, l - 1].Object) != 0 && grid[x, y, l - 1].ObjectLegal != 0)
                        Spread(x, y, l);
                }
            }
            bool changed;
            do {
                changed = false;
                for (var x = 0; x < w; x++) {
                    for (var y = 0; y < h; y++) {
                        if (l == 0) {
                            grid[x, y, l].Visited = true;
                            grid[x, y, l].Support = true;
                            grid[x, y, l].FloorSupport = true;
                            grid[x, y, l].RoofSupport = true;
                        }
                        else if (!grid[x, y, l].Visited) {
                            if ((!grid[x, y, l].FloorSupport && !grid[x, y, l].Support) || grid[x, y, l].Floor == 0) {
                                if (grid[x, y, l].RoofSupport && grid[x, y, l].Roof != 0) {
                                    changed |= RoofRun(x, y, l);
                                    grid[x, y, l].Visited = true;
                                }
                            }
                            else {
                                changed |= FloorRun(x, y, l);
                                if (grid[x, y, l].Roof != 0) changed |= RoofRun(x, y, l);
                                grid[x, y, l].Visited = true;
                            }
                        }
                    }
                }
            }
            while (changed);
            for (var x = 1; x < w; x++) {
                for (var y = 1; y < h; y++) {
                    if (grid[x, y, l].Floor == 0) continue;
                    if (l == levels - 1 && TryTile(grid[x, y, l].Floor, out var td) && td.Height >= 2 && (td.Flags & TileFlag.Surface) != 0) continue;
                    if (l == 0 || grid[x, y, l].Support || grid[x, y, l].FloorSupport) grid[x, y, l].FloorLegal = true;
                }
            }
            for (var x = 0; x < w; x++) {
                for (var y = 0; y < h; y++) {
                    if (grid[x, y, l].Roof != 0 && (l == 0 || grid[x, y, l].Support || grid[x, y, l].RoofSupport || grid[x, y, l].FloorLegal))
                        grid[x, y, l].RoofLegal = true;
                }
            }
            for (var x = 0; x < w; x++) {
                for (var y = 0; y < h; y++) {
                    if (!EdgeGuard(x, y, l)) continue;
                    if (Bottom(grid[x, y, l].Object) == 0 || grid[x, y, l].Locked) continue;
                    var ok = grid[x, y, l].Support || grid[x, y, l].FloorLegal
                        || (CanGoN(grid[x, y, l].Object) != 0 && InGrid(x, y + 1) && grid[x, y + 1, l].FloorLegal)
                        || (CanGoW(grid[x, y, l].Object) != 0 && InGrid(x + 1, y) && grid[x + 1, y, l].FloorLegal)
                        || (CanGoNWS(grid[x, y, l].Object) != 0 && InGrid(x + 1, y + 1) && grid[x + 1, y + 1, l].FloorLegal);
                    if (ok) grid[x, y, l].ObjectLegal = 1;
                }
            }
            for (var x = 0; x < w; x++) {
                for (var y = 0; y < h; y++) {
                    if (!EdgeGuard(x, y, l)) continue;
                    if (grid[x, y, l].Object == 0 || grid[x, y, l].ObjectLegal != 0 || grid[x, y, l].Locked) continue;
                    const byte rank = 2;
                    for (var d = 0; d < 4; d++) {
                        var nx = x + dx[d];
                        var ny = y + dy[d];
                        if (!InGrid(nx, ny)) continue;
                        if (grid[nx, ny, l].ObjectLegal != 0 && grid[nx, ny, l].ObjectLegal < rank && AdjPair(grid[x, y, l].Object, grid[nx, ny, l].Object, dx[d], dy[d])) {
                            grid[x, y, l].ObjectLegal = rank;
                            break;
                        }
                    }
                }
            }
        }
        for (var n = 0; n < _pieces.Count; n++) {
            var mm = _pieces[n];
            if (!mm.IsCustom) continue;
            mm.State &= ~(CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE);
            mm.State |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE;
            if ((mm.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL) != 0) continue;
            var gx = mm.X - ox;
            var gy = mm.Y - oy;
            if (!InGrid(gx, gy)) continue;
            var level = (mm.Z - baseZ) / 20;
            if (level < 0) level = 0;
            if (level >= levels) continue;
            if (grid[gx, gy, level].Locked) continue;
            var legal = SlotOf(mm.Graphic) switch {
                0 => grid[gx, gy, level].FloorLegal,
                2 => grid[gx, gy, level].RoofLegal,
                _ => grid[gx, gy, level].ObjectLegal != 0,
            };
            if (!legal) mm.State |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE;
        }
    }

    public bool ValidateHouseForCommit() {
        var foundationItem = Foundation();
        if (foundationItem == null || TryGetHouse(Serial) == 0) return false;
        var width = Math.Abs(EndPos.X - StartPos.X) + 1;
        var height = Math.Abs(EndPos.Y - StartPos.Y) + 1;
        if (width < 1 || width > 32 || height < 1 || height > 32 || FloorCount < 1 || FloorCount > 4) return false;
        GenerateFloorPlace();
        for (var i = 0; i < _pieces.Count; i++) {
            var item = _pieces[i];
            if (!item.IsCustom) continue;
            if ((item.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL) != 0) continue;
            if ((item.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE) != 0) return false;
        }
        return true;
    }

    public (int, int) ExistsInList(ref CUSTOM_HOUSE_GUMP_STATE state, ushort graphic) {
        var res = SeekGraphicInCustomHouseObjectListWithCategory<CustomHouseWall, CustomHouseWallCategory>(Walls, graphic);
        if (res.Item1 == -1 || res.Item2 == -1) {
            res = SeekGraphicInCustomHouseObjectList(Floors, graphic);
            if (res.Item1 == -1 || res.Item2 == -1) {
                res = SeekGraphicInCustomHouseObjectList(Doors, graphic);
                if (res.Item1 == -1 || res.Item2 == -1) {
                    res = SeekGraphicInCustomHouseObjectListWithCategory<CustomHouseMisc, CustomHouseMiscCategory>(Miscs, graphic);
                    if (res.Item1 == -1 || res.Item2 == -1) {
                        res = SeekGraphicInCustomHouseObjectList(Stairs, graphic);
                        if (res.Item1 == -1 || res.Item2 == -1) {
                            res = SeekGraphicInCustomHouseObjectListWithCategory<CustomHouseRoof, CustomHouseRoofCategory>(Roofs, graphic);
                            if (res.Item1 != -1 && res.Item2 != -1) state = CUSTOM_HOUSE_GUMP_STATE.CHGS_ROOF;
                        }
                        else state = CUSTOM_HOUSE_GUMP_STATE.CHGS_STAIR;
                    }
                    else {
                        var teleport = SeekGraphicInCustomHouseObjectList(Teleports, graphic);
                        if (teleport.Item1 != -1 && teleport.Item2 != -1) {
                            state = CUSTOM_HOUSE_GUMP_STATE.CHGS_FIXTURE;
                            res = teleport;
                        }
                        else state = CUSTOM_HOUSE_GUMP_STATE.CHGS_MISC;
                    }
                }
                else state = CUSTOM_HOUSE_GUMP_STATE.CHGS_DOOR;
            }
            else state = CUSTOM_HOUSE_GUMP_STATE.CHGS_FLOOR;
        }
        else state = CUSTOM_HOUSE_GUMP_STATE.CHGS_WALL;
        return res;
    }

    bool ValidateItemPlace(Rect rect, ushort graphic, int x, int y) {
        if (!rect.Contains(x, y)) return false;
        var infoCheck = SeekGraphicInCustomHouseObjectList(ObjectsInfo, graphic);
        if (infoCheck.Item1 != -1 && infoCheck.Item2 != -1) {
            var info = ObjectsInfo[infoCheck.Item1];
            if (info.CanGoW == 0 && x == rect.X) return false;
            if (info.CanGoN == 0 && y == rect.Y) return false;
            if (info.CanGoNWS == 0 && x == rect.X && y == rect.Y) return false;
        }
        return true;
    }

    void ParseFile<T>(List<T> list, string path) where T : CustomHouseObject, new() {
        if (string.IsNullOrEmpty(path)) return;
        var file = new FileInfo(path);
        if (!file.Exists) return;
        using (var reader = File.OpenText(file.FullName)) {
            while (!reader.EndOfStream) {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var item = new T();
                if (item.Parse(line) && (item.FeatureMask == 0 || ((int)UOLoginNet.Features & item.FeatureMask) != 0))
                    list.Add(item);
            }
        }
    }

    void ParseFileWithCategory<T, U>(List<U> list, string path) where T : CustomHouseObject, new() where U : CustomHouseObjectCategory<T>, new() {
        if (string.IsNullOrEmpty(path)) return;
        var file = new FileInfo(path);
        if (!file.Exists) return;
        using (var reader = File.OpenText(file.FullName)) {
            while (!reader.EndOfStream) {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var item = new T();
                if (!item.Parse(line)) continue;
                if (item.FeatureMask != 0 && ((int)UOLoginNet.Features & item.FeatureMask) == 0) continue;
                var found = false;
                for (var i = 0; i < list.Count; i++) {
                    if (list[i].Index != item.Category) continue;
                    list[i].Items.Add(item);
                    found = true;
                    break;
                }
                if (!found) {
                    var c = new U { Index = item.Category };
                    c.Items.Add(item);
                    list.Add(c);
                }
            }
        }
    }

    static (int, int) SeekGraphicInCustomHouseObjectListWithCategory<T, U>(List<U> list, ushort graphic) where T : CustomHouseObject where U : CustomHouseObjectCategory<T> {
        for (var i = 0; i < list.Count; i++) {
            var c = list[i];
            for (var j = 0; j < c.Items.Count; j++)
                if (c.Items[j].Contains(graphic) != -1) return (i, j);
        }
        return (-1, -1);
    }

    static (int, int) SeekGraphicInCustomHouseObjectList<T>(List<T> list, ushort graphic) where T : CustomHouseObject {
        for (var i = 0; i < list.Count; i++) {
            var contains = list[i].Contains(graphic);
            if (contains != -1) return (i, contains);
        }
        return (-1, -1);
    }

    void UpdateMaxPage() {
        MaxPage = 1;
        switch (State) {
            case CUSTOM_HOUSE_GUMP_STATE.CHGS_WALL:
                if (Category == -1) MaxPage = (int)Math.Ceiling(Walls.Count / 16.0);
                else {
                    for (var i = 0; i < Walls.Count; i++)
                        if (Walls[i].Index == Category) {
                            MaxPage = Walls[i].Items.Count;
                            break;
                        }
                }
                break;
            case CUSTOM_HOUSE_GUMP_STATE.CHGS_DOOR:
                MaxPage = Doors.Count;
                break;
            case CUSTOM_HOUSE_GUMP_STATE.CHGS_FLOOR:
                MaxPage = Floors.Count;
                break;
            case CUSTOM_HOUSE_GUMP_STATE.CHGS_STAIR:
                MaxPage = Stairs.Count;
                break;
            case CUSTOM_HOUSE_GUMP_STATE.CHGS_ROOF:
                if (Category == -1) MaxPage = (int)Math.Ceiling(Roofs.Count / 16.0);
                else {
                    for (var i = 0; i < Roofs.Count; i++)
                        if (Roofs[i].Index == Category) {
                            MaxPage = Roofs[i].Items.Count;
                            break;
                        }
                }
                break;
            case CUSTOM_HOUSE_GUMP_STATE.CHGS_MISC:
                if (Category == -1) MaxPage = (int)Math.Ceiling(Miscs.Count / 16.0);
                else {
                    for (var i = 0; i < Miscs.Count; i++)
                        if (Miscs[i].Index == Category) {
                            MaxPage = Miscs[i].Items.Count;
                            break;
                        }
                }
                break;
        }
    }

    void Recount() {
        Components = 0;
        Fixtures = 0;
        if (Foundation() == null || TryGetHouse(Serial) == 0) return;
        for (var i = 0; i < _pieces.Count; i++) {
            var item = _pieces[i];
            if (!item.IsCustom || (item.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL) != 0) continue;
            CUSTOM_HOUSE_GUMP_STATE state = 0;
            var res = ExistsInList(ref state, item.Graphic);
            if (res.Item1 == -1 || res.Item2 == -1) continue;
            if (state == CUSTOM_HOUSE_GUMP_STATE.CHGS_DOOR || state == CUSTOM_HOUSE_GUMP_STATE.CHGS_FIXTURE) Fixtures++;
            else Components++;
        }
    }

    void ClearCustomHouseComponents(CUSTOM_HOUSE_MULTI_OBJECT_FLAGS state) {
        var item = Foundation();
        if (item == null) return;
        var checkZ = item.Z + 7;
        for (var i = 0; i < _pieces.Count; i++) {
            var component = _pieces[i];
            component.State &= ~(CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_TRANSPARENT | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE);
            if (component.IsCustom) {
                if (component.Z <= item.Z && (component.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_STAIR) == 0)
                    component.State |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_DONT_REMOVE;
                if (state == 0 || (component.State & state) != 0) component.Destroy();
            }
            else if (component.Z <= checkZ)
                component.State |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER;
            if (component.IsDestroyed) _pieces.RemoveAt(i--);
        }
    }

    Component Add(ushort graphic, ushort hue, ushort x, ushort y, sbyte z, bool isCustom, bool isMovable) {
        var m = new Component {
            Graphic = graphic,
            Hue = hue,
            IsCustom = isCustom,
            IsMovable = isMovable,
            X = x,
            Y = y,
            Z = z
        };
        _pieces.Add(m);
        return m;
    }

    List<Component> GetMultiAt(int x, int y) {
        var list = new List<Component>();
        for (var i = 0; i < _pieces.Count; i++) {
            var s = _pieces[i];
            if (!s.IsDestroyed && s.X == x && s.Y == y) list.Add(s);
        }
        return list;
    }

    List<Component> At(int x, int y) {
        var list = new List<Component>();
        for (var i = 0; i < _pieces.Count; i++)
            if (_pieces[i].X == x && _pieces[i].Y == y) list.Add(_pieces[i]);
        return list;
    }

    Component Find(CustomHouseHit hit) {
        if (!hit.IsPiece) return null;
        for (var i = 0; i < _pieces.Count; i++) {
            var c = _pieces[i];
            if (c.IsDestroyed || c.X != hit.X || c.Y != hit.Y || c.Z != hit.Z || c.Graphic != hit.Graphic) continue;
            return c;
        }
        return null;
    }

    Item Foundation() {
        if (_world != null && _world.Items.TryGetValue(Serial, out var obj) && obj is Item item) return item;
        return null;
    }

    static string UoPath(string name) {
        var root = UltimaOnline.Current?.ClientPath;
        return string.IsNullOrEmpty(root) ? name : Path.Combine(root, name);
    }

    static bool TryTile(ushort graphic, out Binary_TileData.ItemData data) {
        var tiles = Tiles();
        if (tiles == null || graphic >= tiles.Length) {
            data = default;
            return false;
        }
        data = tiles[graphic];
        return true;
    }

    static Binary_TileData.ItemData[] Tiles() {
        if (_tiles != null || _tilesMiss) return _tiles;
        var root = UltimaOnline.Current?.ClientPath;
        if (string.IsNullOrEmpty(root)) return null;
        _tilesMiss = true;
        try {
            var path = Path.Combine(root, "tiledata.mul");
            if (!File.Exists(path)) return null;
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var r = new BinaryReader(fs);
            _tiles = new Binary_TileData(r).ItemDatas;
        }
        catch { _tiles = null; }
        return _tiles;
    }

    static bool ReadMulti(ushort graphic, List<MultiBlock> blocks) {
        var root = UltimaOnline.Current?.ClientPath;
        if (string.IsNullOrEmpty(root)) return false;
        var idxPath = Path.Combine(root, "multi.idx");
        var mulPath = Path.Combine(root, "multi.mul");
        if (!File.Exists(idxPath) || !File.Exists(mulPath)) return false;
        var index = graphic >= 0x4000 ? graphic & 0x3FFF : graphic;
        try {
            using var idx = new FileStream(idxPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var at = (long)index * 12;
            if (at < 0 || at + 12 > idx.Length) return false;
            var hdr = new byte[12];
            idx.Position = at;
            if (idx.Read(hdr, 0, 12) < 12) return false;
            var offset = BitConverter.ToInt32(hdr, 0);
            var length = BitConverter.ToInt32(hdr, 4);
            if (offset < 0 || length <= 0) return false;
            using var mul = new FileStream(mulPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if ((long)offset + length > mul.Length) return false;
            var buf = new byte[length];
            mul.Position = offset;
            if (mul.Read(buf, 0, length) < length) return false;
            var ver = UltimaOnline.Current?.Version ?? default;
            var size = ver >= ClientVersion.CV_7090 ? 16 : 12;
            if (length < size) return false;
            var count = length / size;
            for (var i = 0; i < count; i++) {
                var o = i * size;
                var id = BitConverter.ToUInt16(buf, o);
                var x = BitConverter.ToInt16(buf, o + 2);
                var y = BitConverter.ToInt16(buf, o + 4);
                var z = BitConverter.ToInt16(buf, o + 6);
                // Mul records are MultiBlock (uint flags). The 7.0.9 tail is skipped. Visible when those flags are not 0.
                var visible = BitConverter.ToUInt32(buf, o + 8) != 0;
                blocks.Add(new MultiBlock { Id = id, X = x, Y = y, Z = z, Visible = visible });
            }
            return blocks.Count > 0;
        }
        catch { return false; }
    }
}
