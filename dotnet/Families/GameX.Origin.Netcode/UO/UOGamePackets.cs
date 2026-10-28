// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace GameX.Origin.Netcode.UO;

public readonly struct UOMobileState {
    public UOMobileState(uint serial, ushort body, int x, int y, int z, byte direction) {
        Serial = serial;
        Body = body;
        X = x;
        Y = y;
        Z = z;
        Direction = direction;
    }

    public uint Serial { get; }
    public ushort Body { get; }
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
    public byte Direction { get; }
}

public readonly struct UOItemState {
    public UOItemState(uint serial, ushort graphic, int x, int y, int z) {
        Serial = serial;
        Graphic = graphic;
        X = x;
        Y = y;
        Z = z;
    }

    public uint Serial { get; }
    public ushort Graphic { get; }
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
}

/// <summary>One worn item from 0x78 or 0x2E. Layer 0x15 is the backpack.</summary>
public readonly struct UOEquipState {
    public UOEquipState(uint serial, ushort graphic, byte layer, ushort hue, uint parent) {
        Serial = serial;
        Graphic = graphic;
        Layer = layer;
        Hue = hue;
        Parent = parent;
    }

    public uint Serial { get; }
    public ushort Graphic { get; }
    public byte Layer { get; }
    public ushort Hue { get; }
    public uint Parent { get; }
}

/// <summary>One item inside a container from 0x3C or 0x25. Price is 0 until 0x74.</summary>
public readonly struct UOContainedState {
    public UOContainedState(uint serial, ushort graphic, ushort amount, int x, int y, ushort hue, uint container, uint price = 0) {
        Serial = serial;
        Graphic = graphic;
        Amount = amount;
        X = x;
        Y = y;
        Hue = hue;
        Container = container;
        Price = price;
    }

    public uint Serial { get; }
    public ushort Graphic { get; }
    public ushort Amount { get; }
    public int X { get; }
    public int Y { get; }
    public ushort Hue { get; }
    public uint Container { get; }
    public uint Price { get; }

    public UOContainedState WithPrice(uint price) => new(Serial, Graphic, Amount, X, Y, Hue, Container, price);
}

/// <summary>One skill from 0x3A. Name is set only for the 0xFE name block.</summary>
public readonly struct UOSkillState {
    public UOSkillState(ushort id, ushort value, ushort baseValue, byte lockState, ushort cap, string name) {
        Id = id;
        Value = value;
        BaseValue = baseValue;
        Lock = lockState;
        Cap = cap;
        Name = name;
    }

    public ushort Id { get; }
    public ushort Value { get; }
    public ushort BaseValue { get; }
    public byte Lock { get; }
    public ushort Cap { get; }
    public string Name { get; }
}

/// <summary>One overhead line from 0x1C or 0xAE.</summary>
public readonly struct UOSpeechState {
    public UOSpeechState(uint serial, string name, string text, ushort hue) {
        Serial = serial;
        Name = name ?? "";
        Text = text ?? "";
        Hue = hue;
    }

    public uint Serial { get; }
    public string Name { get; }
    public string Text { get; }
    public ushort Hue { get; }
}

/// <summary>Hits from 0x11. The stat block is present only when the flag byte is nonzero.</summary>
public readonly struct UOStatusState {
    public UOStatusState(uint serial, string name, ushort hits, ushort hitsMax, ushort stam, ushort stamMax, ushort mana, ushort manaMax, ushort str, ushort dex, ushort intel, bool hasStats) {
        Serial = serial;
        Name = name ?? "";
        Hits = hits;
        HitsMax = hitsMax;
        Stam = stam;
        StamMax = stamMax;
        Mana = mana;
        ManaMax = manaMax;
        Str = str;
        Dex = dex;
        Int = intel;
        HasStats = hasStats;
    }

    public uint Serial { get; }
    public string Name { get; }
    public ushort Hits { get; }
    public ushort HitsMax { get; }
    public ushort Stam { get; }
    public ushort StamMax { get; }
    public ushort Mana { get; }
    public ushort ManaMax { get; }
    public ushort Str { get; }
    public ushort Dex { get; }
    public ushort Int { get; }
    public bool HasStats { get; }

    public UOStatusState WithHits(ushort hits, ushort max) => new(Serial, Name, hits, max, Stam, StamMax, Mana, ManaMax, Str, Dex, Int, HasStats);
    public UOStatusState WithMana(ushort mana, ushort max) => new(Serial, Name, Hits, HitsMax, Stam, StamMax, mana, max, Str, Dex, Int, HasStats);
    public UOStatusState WithStam(ushort stam, ushort max) => new(Serial, Name, Hits, HitsMax, stam, max, Mana, ManaMax, Str, Dex, Int, HasStats);
}

/// <summary>One generic gump from 0xB0 or 0xDD. Layout is the command string. Lines are the text table.</summary>
public sealed class UOServerGump {
    public UOServerGump(uint sender, uint id, int x, int y, string layout, string[] lines) {
        Sender = sender;
        Id = id;
        X = x;
        Y = y;
        Layout = layout ?? "";
        Lines = lines ?? Array.Empty<string>();
    }

    public uint Sender { get; }
    public uint Id { get; }
    public int X { get; }
    public int Y { get; }
    public string Layout { get; }
    public string[] Lines { get; }
}

/// <summary>Open or close, in packet order. Close uses the gump id.</summary>
public readonly struct UOGumpEvent {
    public UOGumpEvent(UOServerGump gump) {
        Open = true;
        Gump = gump;
        Id = 0;
        Button = 0;
    }

    public UOGumpEvent(uint id, int button) {
        Open = false;
        Gump = null;
        Id = id;
        Button = button;
    }

    public bool Open { get; }
    public UOServerGump Gump { get; }
    public uint Id { get; }
    public int Button { get; }
}

/// <summary>One row of a 0x7C menu. Gray rows leave the graphic and hue at 0.</summary>
public readonly struct UOMenuEntry {
    public UOMenuEntry(ushort graphic, ushort hue, string name) {
        Graphic = graphic;
        Hue = hue;
        Name = name ?? "";
    }

    public ushort Graphic { get; }
    public ushort Hue { get; }
    public string Name { get; }
}

/// <summary>0x7C. Id is the menu graphic sent back on 0x7D. Codes are 1-based.</summary>
public sealed class UOMenu {
    public UOMenu(uint serial, ushort id, string name, bool gray, UOMenuEntry[] entries) {
        Serial = serial;
        Id = id;
        Name = name ?? "";
        Gray = gray;
        Entries = entries ?? Array.Empty<UOMenuEntry>();
    }

    public uint Serial { get; }
    public ushort Id { get; }
    public string Name { get; }
    public bool Gray { get; }
    public UOMenuEntry[] Entries { get; }
}

/// <summary>One row of a 0xBF subcommand 0x0014 menu. Flag 0x01 is hue 0x0386. Otherwise the hue is 0xFFFF.</summary>
public readonly struct UOPopupEntry {
    public UOPopupEntry(int cliloc, ushort index, ushort hue, ushort replacedHue, ushort flags) {
        Cliloc = cliloc;
        Index = index;
        Hue = hue;
        ReplacedHue = replacedHue;
        Flags = flags;
    }

    public int Cliloc { get; }
    public ushort Index { get; }
    public ushort Hue { get; }
    public ushort ReplacedHue { get; }
    public ushort Flags { get; }
}

/// <summary>0xBF subcommand 0x0014. The reply is 0x0015 and sends Index.</summary>
public sealed class UOPopupMenu {
    public UOPopupMenu(uint serial, UOPopupEntry[] entries) {
        Serial = serial;
        Entries = entries ?? Array.Empty<UOPopupEntry>();
    }

    public uint Serial { get; }
    public UOPopupEntry[] Entries { get; }
}

/// <summary>0xAB. The reply is 0xAC and uses Parent and Button as the server sent them.</summary>
public sealed class UOTextEntry {
    public UOTextEntry(uint serial, byte parent, byte button, string text, string description, bool cancel, int maxLength) {
        Serial = serial;
        Parent = parent;
        Button = button;
        Text = text ?? "";
        Description = description ?? "";
        Cancel = cancel;
        MaxLength = maxLength;
    }

    public uint Serial { get; }
    public byte Parent { get; }
    public byte Button { get; }
    public string Text { get; }
    public string Description { get; }
    public bool Cancel { get; }
    public int MaxLength { get; }
}

/// <summary>0x74 buy or 0x9E sell. Close sends an empty 0x3B or 0x9F. Open is false when an incoming 0x3B closes it.</summary>
public readonly struct UOShop {
    public UOShop(uint serial, bool buy, bool open = true) {
        Serial = serial;
        Buy = buy;
        Open = open;
    }

    public uint Serial { get; }
    public bool Buy { get; }
    public bool Open { get; }
}

/// <summary>0x6F. Close sends cancel. A server close does not.</summary>
public readonly struct UOTrade {
    public UOTrade(uint serial, string name, bool open) {
        Serial = serial;
        Name = name ?? "";
        Open = open;
    }

    public uint Serial { get; }
    public string Name { get; }
    public bool Open { get; }
}

/// <summary>One 0x89 item. The packet has no graphic or hue. Layer is the byte minus one. Backpack is omitted.</summary>
public readonly struct UOCorpseEquip {
    public UOCorpseEquip(uint serial, byte layer, uint corpse) {
        Serial = serial;
        Layer = layer;
        Corpse = corpse;
    }

    public uint Serial { get; }
    public byte Layer { get; }
    public uint Corpse { get; }
}

/// <summary>One 0xDF buff. Graphic is ClassicUO's default buff table. Timer is the packet ushort.</summary>
public readonly struct UOBuffState {
    public UOBuffState(ushort type, ushort graphic, ushort timer, string text) {
        Type = type;
        Graphic = graphic;
        Timer = timer;
        Text = text ?? "";
    }

    public ushort Type { get; }
    public ushort Graphic { get; }
    public ushort Timer { get; }
    public string Text { get; }
}

/// <summary>0x95. OK sends the hue. Close does not.</summary>
public readonly struct UODye {
    public UODye(uint serial, ushort graphic) {
        Serial = serial;
        Graphic = graphic;
    }

    public uint Serial { get; }
    public ushort Graphic { get; }
}

/// <summary>One 0x38 point. Z is the ushort on the packet.</summary>
public readonly struct UOPathPoint {
    public UOPathPoint(ushort x, ushort y, ushort z) {
        X = x;
        Y = y;
        Z = z;
    }

    public ushort X { get; }
    public ushort Y { get; }
    public ushort Z { get; }
}

/// <summary>
/// Game-stream framing. Lengths match Origin's PacketsTable. 0xFF is variable; that row is missing there.
/// </summary>
public static class UOGamePackets {
    public static readonly short[] Lengths = {
        0x0068, 0x0005, 0x0007, -1, 0x0002, 0x0005, 0x0005, 0x0007,
        0x000E, 0x0005, 0x000B, 0x010A, -1, 0x0003, -1, 0x003D,
        0x00D7, -1, -1, 0x000A, 0x0006, 0x0009, 0x0001, -1,
        -1, -1, -1, 0x0025, -1, 0x0005, 0x0004, 0x0008,
        0x0013, 0x0008, 0x0003, 0x001A, 0x0007, 0x0014, 0x0005, 0x0002,
        0x0005, 0x0001, 0x0005, 0x0002, 0x0002, 0x0011, 0x000F, 0x000A,
        0x0005, 0x0001, 0x0002, 0x0002, 0x000A, 0x028D, -1, 0x0008,
        0x0007, 0x0009, -1, -1, -1, 0x0002, 0x0025, -1,
        0x00C9, -1, -1, 0x0229, 0x02C9, 0x0005, -1, 0x000B,
        0x0049, 0x005D, 0x0005, 0x0009, -1, -1, 0x0006, 0x0002,
        -1, -1, -1, 0x0002, 0x000C, 0x0001, 0x000B, 0x006E,
        0x006A, -1, -1, 0x0004, 0x0002, 0x0049, -1, 0x0031,
        0x0005, 0x0009, 0x000F, 0x000D, 0x0001, 0x0004, -1, 0x0015,
        -1, -1, 0x0003, 0x0009, 0x0013, 0x0003, 0x000E, -1,
        0x001C, -1, 0x0005, 0x0002, -1, 0x0023, 0x0010, 0x0011,
        -1, 0x0009, -1, 0x0002, -1, 0x000D, 0x0002, -1,
        0x003E, -1, 0x0002, 0x0027, 0x0045, 0x0002, -1, -1,
        0x0042, -1, -1, -1, 0x000B, -1, -1, -1,
        0x0013, 0x0041, -1, 0x0063, -1, 0x0009, -1, 0x0002,
        -1, 0x001A, -1, 0x0102, 0x0135, 0x0033, -1, -1,
        0x0003, 0x0009, 0x0009, 0x0009, 0x0095, -1, -1, 0x0004,
        -1, -1, 0x0005, -1, -1, -1, -1, 0x000D,
        -1, -1, -1, -1, -1, 0x0040, 0x0009, -1,
        -1, 0x0003, 0x0006, 0x0009, 0x0003, -1, -1, -1,
        0x0024, -1, -1, -1, 0x0006, 0x00CB, 0x0001, 0x0031,
        0x0002, 0x0006, 0x0006, 0x0007, -1, 0x0001, -1, 0x004E,
        -1, 0x0002, 0x0019, -1, -1, -1, -1, -1,
        -1, 0x010C, -1, -1, 0x09, -1, -1, -1,
        -1, -1, 0x0A, -1, -1, -1, 0x05, 0x0C,
        0x0D, 0x4B, 0x03, -1, -1, -1, 0x0A, 0x0015,
        -1, 0x09, 0x19, 0x1A, -1, 0x15, -1, -1,
        0x6A, -1, -1, -1, -1, -1, -1, -1,
    };

    static ClientVersion _version;

    /// <summary>
    /// Same length patches as Origin's PacketsTable constructor. The static table is the unpatched list.
    /// </summary>
    public static void UseVersion(ClientVersion version) => _version = version;

    static short Adjusted(byte id) {
        var n = Lengths[id];
        var version = _version;
        if (version >= ClientVersion.CV_500A) {
            if (id == 0x0B) n = 0x07;
            else if (id == 0x16 || id == 0x31) n = -1;
        }
        if (version >= ClientVersion.CV_6017) {
            if (id == 0x08) n = 0x0F;
            else if (id == 0x25) n = 0x15;
        }
        if (version >= ClientVersion.CV_60142 && id == 0xB9) n = 0x05;
        if (version >= ClientVersion.CV_7090) {
            if (id == 0x24) n = 0x09;
            else if (id == 0x99) n = 0x1E;
            else if (id == 0xBA) n = 0x0A;
            else if (id == 0xF1) n = 0x09;
            else if (id == 0xF2) n = 0x19;
            else if (id == 0xF3) n = 0x1A;
        }
        if (version >= ClientVersion.CV_70180 && id == 0x00) n = 0x6A;
        if (version >= ClientVersion.CV_706400) {
            if (id == 0xFA) n = 0x01;
            else if (id == 0xFB) n = 0x02;
        }
        if (version >= ClientVersion.CV_7010400) {
            if (id == 0xD5) n = 0x09;
            else if (id == 0xFD) n = 2;
        }
        return n;
    }

    public static int FrameLength(ReadOnlySpan<byte> s) {
        if (s.Length < 1) return 0;
        var n = Adjusted(s[0]);
        if (n < 0) {
            if (s.Length < 3) return 0;
            var size = (s[1] << 8) | s[2];
            if (size < 3) return -1;
            if (size > s.Length) return 0;
            return size;
        }
        if (n == 0) return -1;
        if (s.Length < n) return 0;
        return n;
    }

    /// <summary>73-byte 0x5D. Big-endian fields. No password.</summary>
    public static byte[] PlayCharacter(string name, int slot, uint ip) {
        var p = new byte[73];
        p[0] = 0x5D;
        p[1] = p[2] = p[3] = p[4] = 0xED;
        WriteAscii(p, 5, name);
        WriteU32(p, 37, 0x1F);
        WriteU32(p, 65, (uint)slot);
        WriteU32(p, 69, ip);
        return p;
    }

    public static bool LoginConfirm(byte[] p, int o, out uint serial, out ushort body, out int x, out int y, out int z, out byte dir) {
        serial = body = 0;
        x = y = z = 0;
        dir = 0;
        if (o + 17 > p.Length) return false;
        serial = ReadU32(p, o + 1);
        body = ReadU16(p, o + 9);
        x = ReadU16(p, o + 11);
        y = ReadU16(p, o + 13);
        z = (sbyte)p[o + 15];
        dir = p[o + 16];
        return true;
    }

    public static bool UpdatePlayer(byte[] p, int o, int len, out uint serial, out ushort body, out int x, out int y, out int z, out byte dir) {
        serial = body = 0;
        x = y = z = 0;
        dir = 0;
        if (len < 19) return false;
        serial = ReadU32(p, o + 1);
        body = ReadU16(p, o + 5);
        x = ReadU16(p, o + 11);
        y = ReadU16(p, o + 13);
        dir = p[o + 17];
        z = (sbyte)p[o + 18];
        return true;
    }

    public static bool MoveReject(byte[] p, int o, out int x, out int y, out byte dir, out int z) {
        x = y = z = 0;
        dir = 0;
        if (o + 8 > p.Length) return false;
        x = ReadU16(p, o + 2);
        y = ReadU16(p, o + 4);
        dir = p[o + 6];
        z = (sbyte)p[o + 7];
        return true;
    }

    public static bool Moving(byte[] p, int o, int len, out uint serial, out ushort body, out int x, out int y, out int z, out byte dir) {
        serial = body = 0;
        x = y = z = 0;
        dir = 0;
        if (len < 13) return false;
        serial = ReadU32(p, o + 1);
        body = ReadU16(p, o + 5);
        x = ReadU16(p, o + 7);
        y = ReadU16(p, o + 9);
        z = (sbyte)p[o + 11];
        dir = p[o + 12];
        return true;
    }

    /// <summary>
    /// 0x78. The mobile is the same fields as before. Equipment starts after hue, flags, and notoriety.
    /// From client 7.0.33.1 every item has a hue. Older clients send the hue only when the graphic has 0x8000.
    /// </summary>
    public static bool Incoming(byte[] p, int o, int len, out uint serial, out ushort body, out int x, out int y, out int z, out byte dir, List<UOEquipState> equip) {
        serial = body = 0;
        x = y = z = 0;
        dir = 0;
        equip?.Clear();
        if (len < 15) return false;
        serial = ReadU32(p, o + 3);
        body = ReadU16(p, o + 7);
        x = ReadU16(p, o + 9);
        y = ReadU16(p, o + 11);
        z = (sbyte)p[o + 13];
        dir = p[o + 14];
        if (len < 19 || equip == null) return true;
        var i = o + 19;
        var end = o + len;
        while (i + 4 <= end) {
            var item = ReadU32(p, i);
            i += 4;
            if (item == 0) break;
            if (i + 3 > end) break;
            var graphic = ReadU16(p, i);
            i += 2;
            var layer = p[i++];
            ushort hue = 0;
            if (_version >= ClientVersion.CV_70331) {
                if (i + 2 > end) break;
                hue = ReadU16(p, i);
                i += 2;
            }
            else if ((graphic & 0x8000) != 0) {
                graphic = (ushort)(graphic & 0x7FFF);
                if (i + 2 > end) break;
                hue = ReadU16(p, i);
                i += 2;
            }
            equip.Add(new UOEquipState(item, graphic, layer, hue, serial));
        }
        return true;
    }

    public static bool WorldItem(byte[] p, int o, int len, out uint serial, out ushort graphic, out int x, out int y, out int z) {
        serial = 0;
        graphic = 0;
        x = y = z = 0;
        if (len < 9) return false;
        var i = o + 3;
        var end = o + len;
        serial = ReadU32(p, i);
        i += 4;
        if (i + 2 > end) return false;
        graphic = ReadU16(p, i);
        i += 2;
        if ((serial & 0x80000000) != 0) {
            if (i + 2 > end) return false;
            i += 2;
        }
        serial &= 0x7FFFFFFF;
        graphic = (ushort)(graphic & 0x7FFF);
        if (i + 4 > end) return false;
        var xu = ReadU16(p, i);
        i += 2;
        var yu = ReadU16(p, i);
        i += 2;
        if ((xu & 0x8000) != 0) {
            if (i >= end) return false;
            i++;
        }
        x = xu & 0x7FFF;
        y = yu & 0x3FFF;
        if (i >= end) return false;
        z = (sbyte)p[i];
        return true;
    }

    /// <summary>
    /// 0xF3, 26 bytes. ClassicUO UpdateItemSA reads past the opcode: ushort, type, serial, graphic, increment, then x at +15.
    /// Type 1 is a mobile and the graphic is the body. Any other type adds the increment and is an item.
    /// </summary>
    public static bool WorldObject(byte[] p, int o, int len, out byte type, out uint serial, out ushort graphic, out int x, out int y, out int z, out byte dir) {
        type = 0;
        serial = 0;
        graphic = 0;
        x = y = z = 0;
        dir = 0;
        if (len < 26 || o + 26 > p.Length) return false;
        type = p[o + 3];
        serial = ReadU32(p, o + 4);
        graphic = ReadU16(p, o + 8);
        if (type != 1) graphic = (ushort)(graphic + p[o + 10]);
        x = ReadU16(p, o + 15);
        y = ReadU16(p, o + 17);
        z = (sbyte)p[o + 19];
        dir = p[o + 20];
        return true;
    }

    /// <summary>0x1D. Five bytes. The serial is not removed when it is the player.</summary>
    public static bool DeleteObject(byte[] p, int o, int len, out uint serial) {
        serial = 0;
        if (len < 5 || o + 5 > p.Length) return false;
        serial = ReadU32(p, o + 1);
        return true;
    }

    /// <summary>0xBF subcommand 0x0008. The map byte follows the big-endian subcommand.</summary>
    public static bool TryFacet(byte[] p, int o, int len, out int map) {
        map = 0;
        if (len < 6) return false;
        var sub = (p[o + 3] << 8) | p[o + 4];
        if (sub != 0x0008) return false;
        map = p[o + 5];
        return true;
    }

    /// <summary>0xBF subcommand 0x0001 is the key stack. 0x0002 adds keys. Both are big-endian uints after the subcommand.</summary>
    public static void FastWalk(byte[] p, int o, int len, Queue<uint> keys) {
        if (len < 5) return;
        var sub = (p[o + 3] << 8) | p[o + 4];
        if (sub != 0x0001 && sub != 0x0002) return;
        var i = o + 5;
        var end = o + len;
        while (i + 4 <= end) {
            keys.Enqueue(ReadU32(p, i));
            i += 4;
        }
    }

    /// <summary>0x1C. Name is 30 bytes at offset 14. Text is ASCII after offset 44.</summary>
    public static bool AsciiSpeech(byte[] p, int o, int len, out UOSpeechState line, out bool probe) {
        line = default;
        probe = false;
        if (len < 44 || o + 44 > p.Length) return false;
        var serial = ReadU32(p, o + 3);
        var graphic = ReadU16(p, o + 7);
        var type = p[o + 9];
        var hue = ReadU16(p, o + 10);
        var font = ReadU16(p, o + 12);
        var name = ReadAscii(p, o + 14, 30);
        var text = len > 44 ? ReadAsciiZ(p, o + 44, o + len) : "";
        probe = serial == 0 && graphic == 0 && type == 0 && hue == 0xFFFF && font == 0xFFFF && name.StartsWith("SYSTEM");
        line = new UOSpeechState(serial, name, text, hue);
        return true;
    }

    /// <summary>0xAE. Language is 4 bytes, the name is 30, and the text is UTF-16BE after offset 48.</summary>
    public static bool UnicodeSpeech(byte[] p, int o, int len, out UOSpeechState line, out bool probe) {
        line = default;
        probe = false;
        if (len < 48 || o + 48 > p.Length) return false;
        var serial = ReadU32(p, o + 3);
        var graphic = ReadU16(p, o + 7);
        var type = p[o + 9];
        var hue = ReadU16(p, o + 10);
        var font = ReadU16(p, o + 12);
        var name = ReadAscii(p, o + 18, 30);
        var text = len > 48 ? ReadUtf16(p, o + 48, o + len) : "";
        probe = serial == 0 && graphic == 0 && type == 0 && hue == 0xFFFF && font == 0xFFFF
            && name.Equals("system", System.StringComparison.OrdinalIgnoreCase);
        line = new UOSpeechState(serial, name, text, hue);
        return true;
    }

    /// <summary>ClassicUO's reply to the SYSTEM speech probe. Forty bytes, packet 0x03.</summary>
    public static byte[] SpeechAck() => new byte[] {
        0x03, 0x00, 0x28, 0x20, 0x00, 0x34, 0x00, 0x03, 0xDB, 0x13, 0x14, 0x3F, 0x45, 0x2C, 0x58, 0x0F,
        0x5D, 0x44, 0x2E, 0x50, 0x11, 0xDF, 0x75, 0x5C, 0xE0, 0x3E, 0x71, 0x4F, 0x31, 0x34, 0x05, 0x4E,
        0x18, 0x1E, 0x72, 0x0F, 0x59, 0xAD, 0xF5, 0x00,
    };

    /// <summary>0xAD. Type 0, hue 0x02B2, font 3, language ENU. Text is UTF-16BE and null terminated.</summary>
    public static byte[] UnicodeSay(string text) {
        if (text == null) text = "";
        if (text.Length > 100) text = text.Substring(0, 100);
        var len = 12 + (text.Length + 1) * 2;
        var p = new byte[len];
        p[0] = 0xAD;
        p[1] = (byte)(len >> 8);
        p[2] = (byte)len;
        p[4] = 0x02;
        p[5] = 0xB2;
        p[7] = 3;
        p[8] = (byte)'E';
        p[9] = (byte)'N';
        p[10] = (byte)'U';
        var i = 12;
        foreach (var ch in text) {
            p[i++] = (byte)(ch >> 8);
            p[i++] = (byte)(ch & 0xFF);
        }
        return p;
    }

    /// <summary>0x11. Hits always. Str, dex, int, stamina, and mana when the flag byte is nonzero.</summary>
    public static bool Status(byte[] p, int o, int len, out UOStatusState status) {
        status = default;
        if (len < 43 || o + 43 > p.Length) return false;
        var i = o + 3;
        var serial = ReadU32(p, i);
        i += 4;
        var name = ReadAscii(p, i, 30);
        i += 30;
        var hits = ReadU16(p, i);
        i += 2;
        var hitsMax = ReadU16(p, i);
        i += 2;
        i++;
        var type = p[i++];
        ushort str = 0, dex = 0, intel = 0, stam = 0, stamMax = 0, mana = 0, manaMax = 0;
        var has = false;
        if (type > 0 && i + 15 <= o + len) {
            i++;
            str = ReadU16(p, i); i += 2;
            dex = ReadU16(p, i); i += 2;
            intel = ReadU16(p, i); i += 2;
            stam = ReadU16(p, i); i += 2;
            stamMax = ReadU16(p, i); i += 2;
            mana = ReadU16(p, i); i += 2;
            manaMax = ReadU16(p, i);
            has = true;
        }
        status = new UOStatusState(serial, name, hits, hitsMax, stam, stamMax, mana, manaMax, str, dex, intel, has);
        return true;
    }

    /// <summary>0xA1 hits, 0xA2 mana, 0xA3 stamina. Nine bytes: serial, current, max.</summary>
    public static bool Vital(byte[] p, int o, int len, out uint serial, out ushort cur, out ushort max) {
        serial = 0;
        cur = max = 0;
        if (len < 9 || o + 9 > p.Length) return false;
        serial = ReadU32(p, o + 1);
        cur = ReadU16(p, o + 5);
        max = ReadU16(p, o + 7);
        return true;
    }

    /// <summary>0x88. Sixty-byte title. Flag 0x02 is the lift bit.</summary>
    public static bool OpenPaperdoll(byte[] p, int o, int len, out uint serial, out string title, out byte flags) {
        serial = 0;
        title = "";
        flags = 0;
        if (len < 66 || o + 66 > p.Length) return false;
        serial = ReadU32(p, o + 1);
        title = ReadAscii(p, o + 5, 60);
        flags = p[o + 65];
        return true;
    }

    /// <summary>0x24. Seven bytes, or nine from client 7.0.9. The extra word is ignored. 0xFFFF is a spellbook.</summary>
    public static bool OpenContainer(byte[] p, int o, int len, out uint serial, out ushort gump) {
        serial = 0;
        gump = 0;
        if (len < 7 || o + 7 > p.Length) return false;
        serial = ReadU32(p, o + 1);
        gump = ReadU16(p, o + 5);
        return true;
    }

    /// <summary>0x25. The grid byte is present from client 6.0.1.7 and sits before the container serial.</summary>
    public static bool ContainedItem(byte[] p, int o, int len, out UOContainedState item) {
        item = default;
        var grid = _version >= ClientVersion.CV_6017;
        var need = grid ? 21 : 20;
        if (len < need || o + need > p.Length) return false;
        return ReadContained(p, o + 1, grid, out item);
    }

    /// <summary>0x3C. Count is the ushort after the length. The same stride as 0x25, without the packet id.</summary>
    public static bool ContainedItems(byte[] p, int o, int len, List<UOContainedState> items) {
        items?.Clear();
        if (len < 5 || items == null) return false;
        var count = ReadU16(p, o + 3);
        var grid = _version >= ClientVersion.CV_6017;
        var stride = grid ? 20 : 19;
        var i = o + 5;
        var end = o + len;
        for (var n = 0; n < count; n++) {
            if (i + stride > end) break;
            if (!ReadContained(p, i, grid, out var item)) break;
            items.Add(item);
            i += stride;
        }
        return true;
    }

    /// <summary>0x2E. Graphic plus the signed increment byte, then layer, mobile, hue.</summary>
    public static bool WornItem(byte[] p, int o, int len, out UOEquipState item) {
        item = default;
        if (len < 15 || o + 15 > p.Length) return false;
        var serial = ReadU32(p, o + 1);
        var graphic = (ushort)(ReadU16(p, o + 5) + (sbyte)p[o + 7]);
        var layer = p[o + 8];
        var parent = ReadU32(p, o + 9);
        var hue = ReadU16(p, o + 13);
        item = new UOEquipState(serial, graphic, layer, hue, parent);
        return true;
    }

    /// <summary>0xBD. ASCII version, null terminated, with a length word.</summary>
    public static byte[] VersionPacket(string version) {
        if (string.IsNullOrEmpty(version)) version = "";
        var bytes = System.Text.Encoding.ASCII.GetBytes(version);
        var len = 4 + bytes.Length;
        var p = new byte[len];
        p[0] = 0xBD;
        p[1] = (byte)(len >> 8);
        p[2] = (byte)len;
        Buffer.BlockCopy(bytes, 0, p, 3, bytes.Length);
        return p;
    }

    /// <summary>0xBF subcommand 0x000B. Three-character language and a null. Nine bytes.</summary>
    public static byte[] Language(string lang) {
        if (string.IsNullOrEmpty(lang)) lang = "ENU";
        var p = new byte[9];
        p[0] = 0xBF;
        p[2] = 9;
        p[4] = 0x0B;
        var bytes = System.Text.Encoding.ASCII.GetBytes(lang);
        var n = bytes.Length < 3 ? bytes.Length : 3;
        Buffer.BlockCopy(bytes, 0, p, 5, n);
        return p;
    }

    /// <summary>0xBF subcommand 0x0005. Thirteen bytes. The host window is not read; the size is fixed.</summary>
    public static byte[] WindowSize(uint width, uint height) {
        var p = new byte[13];
        p[0] = 0xBF;
        p[2] = 13;
        p[4] = 0x05;
        WriteU32(p, 5, width);
        WriteU32(p, 9, height);
        return p;
    }

    /// <summary>0xBF subcommand 0x000F. Ten bytes. The flag word is ClassicUO's shift loop over the protocol bits.</summary>
    public static byte[] ClientType(ClientVersion version) {
        var p = new byte[10];
        p[0] = 0xBF;
        p[2] = 10;
        p[4] = 0x0F;
        p[5] = 0x0A;
        WriteU32(p, 6, ClientTypeFlags(version));
        return p;
    }

    /// <summary>Same bits ClassicUO ORs into the 0xBF 0x000F flag word.</summary>
    public static uint ClientTypeFlags(ClientVersion version) {
        uint protocol = 0;
        if (version >= ClientVersion.CV_200) protocol |= 0x01;
        if (version >= ClientVersion.CV_300) protocol |= 0x02;
        if (version >= ClientVersion.CV_308) protocol |= 0x04;
        if (version >= ClientVersion.CV_308Z) protocol |= 0x08;
        if (version >= ClientVersion.CV_405A) protocol |= 0x10;
        if (version >= ClientVersion.CV_60144) protocol |= 0x20;
        uint flag = 0;
        for (var i = 0; i < protocol; i++)
            flag |= 1u << (int)(i & 31);
        return flag;
    }

    /// <summary>Ten-byte 0x34, type 4.</summary>
    public static byte[] StatusRequest(uint serial) => Query34(0x04, serial);

    /// <summary>Ten-byte 0x34, type 5.</summary>
    public static byte[] SkillsRequest(uint serial) => Query34(0x05, serial);

    /// <summary>Five-byte 0x09.</summary>
    public static byte[] Click(uint serial) {
        var p = new byte[5];
        p[0] = 0x09;
        WriteU32(p, 1, serial);
        return p;
    }

    /// <summary>Two-byte 0xC8. Range is clamped to 5..24.</summary>
    public static byte[] ViewRange(byte range) {
        if (range < 5) range = 5;
        if (range > 24) range = 24;
        return new byte[] { 0xC8, range };
    }

    /// <summary>0x4F. One byte, capped at 30.</summary>
    public static bool OverallLight(byte[] p, int o, int len, out byte level) {
        level = 0;
        if (len < 2 || o + 2 > p.Length) return false;
        level = p[o + 1];
        if (level > 0x1E) level = 0x1E;
        return true;
    }

    /// <summary>0x4E. Serial, then one byte, capped at 30.</summary>
    public static bool PersonalLight(byte[] p, int o, int len, out uint serial, out byte level) {
        serial = 0;
        level = 0;
        if (len < 6 || o + 6 > p.Length) return false;
        serial = ReadU32(p, o + 1);
        level = p[o + 5];
        if (level > 0x1E) level = 0x1E;
        return true;
    }

    /// <summary>0xBC. Season above 4 becomes 0. The music index is stored and not played.</summary>
    public static bool Season(byte[] p, int o, int len, out byte season, out byte music) {
        season = music = 0;
        if (len < 3 || o + 3 > p.Length) return false;
        season = p[o + 1];
        music = p[o + 2];
        if (season > 4) season = 0;
        return true;
    }

    /// <summary>0xB9. A ushort before client 6.0.14.2, otherwise a uint.</summary>
    public static bool Features(byte[] p, int o, int len, out uint flags) {
        flags = 0;
        if (_version >= ClientVersion.CV_60142) {
            if (len < 5 || o + 5 > p.Length) return false;
            flags = ReadU32(p, o + 1);
            return true;
        }
        if (len < 3 || o + 3 > p.Length) return false;
        flags = ReadU16(p, o + 1);
        return true;
    }

    /// <summary>
    /// 0x3A. Type 0xFE is names. 0xFF and 0xDF are one skill. Type 0 and 2 subtract one from the id.
    /// The cap ushort is present when the type is 1, 2, 3, or 0xDF.
    /// </summary>
    public static bool Skills(byte[] p, int o, int len, List<UOSkillState> rows, out bool single) {
        rows?.Clear();
        single = false;
        if (rows == null || len < 4 || o + 4 > p.Length) return false;
        var type = p[o + 3];
        var haveCap = type != 0 && type <= 0x03 || type == 0xDF;
        single = type == 0xFF || type == 0xDF;
        var i = o + 4;
        var end = o + len;
        if (type == 0xFE) {
            if (i + 2 > end) return false;
            var count = ReadU16(p, i);
            i += 2;
            for (var n = 0; n < count && i + 2 <= end; n++) {
                i++;
                var nameLen = p[i++];
                if (i + nameLen > end) break;
                var name = nameLen == 0 ? "" : System.Text.Encoding.ASCII.GetString(p, i, nameLen);
                i += nameLen;
                rows.Add(new UOSkillState((ushort)n, 0, 0, 0, 0, name));
            }
            return true;
        }
        while (i + 2 <= end) {
            var id = (int)ReadU16(p, i);
            i += 2;
            if (i >= end) break;
            if (id == 0 && type == 0) break;
            if (type == 0 || type == 0x02) id--;
            if (id < 0 || i + 5 > end) break;
            var real = ReadU16(p, i);
            i += 2;
            var baseValue = ReadU16(p, i);
            i += 2;
            var locked = p[i++];
            ushort cap = 1000;
            if (haveCap) {
                if (i + 2 > end) break;
                cap = ReadU16(p, i);
                i += 2;
            }
            rows.Add(new UOSkillState((ushort)id, real, baseValue, locked, cap, null));
            if (single) break;
        }
        return true;
    }

    /// <summary>0xC1 is UTF-16LE after the 30-byte name. 0xCC adds a flag, an ASCII affix, and UTF-16BE arguments.</summary>
    public static bool ClilocSpeech(byte[] p, int o, int len, out UOSpeechState line) {
        line = default;
        if (len < 48 || o + 48 > p.Length || o < 0) return false;
        var cc = p[o] == 0xCC;
        if (cc && (len < 49 || o + 49 > p.Length)) return false;
        var serial = ReadU32(p, o + 3);
        var hue = ReadU16(p, o + 10);
        var cliloc = (int)ReadU32(p, o + 14);
        var flags = (byte)0;
        var nameAt = o + 18;
        if (cc) {
            flags = p[o + 18];
            nameAt = o + 19;
            if (nameAt + 30 > o + len) return false;
        }
        var name = ReadAscii(p, nameAt, 30);
        string args;
        var affix = "";
        if (cc) {
            var affixAt = nameAt + 30;
            affix = ReadAsciiZ(p, affixAt, o + len);
            var argAt = affixAt + affix.Length + 1;
            args = argAt < o + len ? ReadUtf16(p, argAt, o + len) : "";
        }
        else args = ReadUtf16Le(p, o + 48, o + len);
        var text = LookupCliloc(cliloc);
        if (string.IsNullOrEmpty(text)) text = "#" + cliloc;
        if (text.IndexOf('~') >= 0) text = ApplyArgs(text, args);
        else if (!string.IsNullOrEmpty(args) && text[0] == '#')
            text = text + " " + args.Replace('\t', ' ').Trim();
        if ((flags & 0x01) != 0) text = affix + text;
        else if (affix.Length > 0) text = text + affix;
        line = new UOSpeechState(serial, name, text, hue);
        return true;
    }

    /// <summary>Five-byte 0x06.</summary>
    public static byte[] Use(uint serial) {
        var p = new byte[5];
        p[0] = 0x06;
        WriteU32(p, 1, serial);
        return p;
    }

    /// <summary>0xBF subcommand 0x0013. Nine bytes.</summary>
    public static byte[] RequestPopup(uint serial) {
        var p = new byte[9];
        p[0] = 0xBF;
        WriteU16(p, 1, 9);
        WriteU16(p, 3, 0x0013);
        WriteU32(p, 5, serial);
        return p;
    }

    /// <summary>0xBF subcommand 0x0015. Eleven bytes.</summary>
    public static byte[] ReplyPopup(uint serial, ushort index) {
        var p = new byte[11];
        p[0] = 0xBF;
        WriteU16(p, 1, 11);
        WriteU16(p, 3, 0x0015);
        WriteU32(p, 5, serial);
        WriteU16(p, 9, index);
        return p;
    }

    /// <summary>0x12 command 0x24. The body is the index, a space, 0, and a null. The length includes that null.</summary>
    public static byte[] UseSkill(int index) {
        var body = Encoding.ASCII.GetBytes(index.ToString() + " 0");
        var p = new byte[5 + body.Length];
        p[0] = 0x12;
        p[3] = 0x24;
        Buffer.BlockCopy(body, 0, p, 4, body.Length);
        WriteU16(p, 1, (ushort)p.Length);
        return p;
    }

    /// <summary>
    /// 0xBF subcommand 0x001C from client 6.0.14.2. Nine bytes: sub, 0x0002, and the spell index.
    /// Older clients send 0x12 command 0x56 and the index as ASCII with a null.
    /// </summary>
    public static byte[] CastSpell(int index) {
        if (_version >= ClientVersion.CV_60142) {
            var p = new byte[9];
            p[0] = 0xBF;
            WriteU16(p, 1, 9);
            WriteU16(p, 3, 0x001C);
            WriteU16(p, 5, 0x0002);
            WriteU16(p, 7, (ushort)index);
            return p;
        }
        var body = Encoding.ASCII.GetBytes(index.ToString());
        var old = new byte[5 + body.Length];
        old[0] = 0x12;
        old[3] = 0x56;
        Buffer.BlockCopy(body, 0, old, 4, body.Length);
        WriteU16(old, 1, (ushort)old.Length);
        return old;
    }

    /// <summary>
    /// 0x00 is 104 bytes. Client 7.0.16 and newer sends 0xF8, 106 bytes, with a fourth skill.
    /// The layout is ServUO's reader: pattern, 30-byte name, 2 unknown, flags, 8 unknown, profession, 15 unknown, sex.
    /// Stats are 45, 10, 10. Skills are Swords 50, Tactics 50, Healing 0. City is the first index. No password.
    /// </summary>
    public static byte[] CreateCharacter(string name, bool female, int slot, uint ip) {
        var extended = _version >= ClientVersion.CV_70160;
        var p = new byte[extended ? 106 : 104];
        p[0] = extended ? (byte)0xF8 : (byte)0x00;
        p[1] = p[2] = p[3] = p[4] = 0xED;
        p[5] = p[6] = p[7] = p[8] = 0xFF;
        WriteAscii(p, 10, name);
        p[70] = female ? (byte)1 : (byte)0;
        p[71] = 45;
        p[72] = 10;
        p[73] = 10;
        p[74] = 40;
        p[75] = 50;
        p[76] = 27;
        p[77] = 50;
        p[78] = 17;
        var hueAt = 80;
        if (extended) {
            p[80] = 43;
            hueAt = 82;
        }
        WriteU16(p, hueAt, 0x03EB);
        WriteU32(p, hueAt + 12, (uint)slot);
        WriteU32(p, hueAt + 16, ip);
        WriteU16(p, hueAt + 20, 2);
        WriteU16(p, hueAt + 22, 2);
        return p;
    }

    /// <summary>Two-byte 0x73. The index is the client's, and the reply is not echoed.</summary>
    public static byte[] Ping(byte index) {
        var p = new byte[2];
        p[0] = 0x73;
        p[1] = index;
        return p;
    }

    /// <summary>0xD6 from client 5.0.9.0. Length word, then up to 15 serials. No count. Removes the serials it sends.</summary>
    public static byte[] MegaClilocRequest(List<uint> serials) {
        var count = Math.Min(15, serials.Count);
        var size = 3 + count * 4;
        var p = new byte[size];
        p[0] = 0xD6;
        WriteU16(p, 1, (ushort)size);
        for (var i = 0; i < count; i++) WriteU32(p, 3 + i * 4, serials[i]);
        serials.RemoveRange(0, count);
        return p;
    }

    /// <summary>Five-byte 0x72. The 0x32 byte is ClassicUO's constant.</summary>
    public static byte[] WarMode(bool on) {
        var p = new byte[5];
        p[0] = 0x72;
        p[1] = on ? (byte)1 : (byte)0;
        p[2] = 0x32;
        return p;
    }

    /// <summary>Five-byte 0x05.</summary>
    public static byte[] Attack(uint serial) {
        var p = new byte[5];
        p[0] = 0x05;
        WriteU32(p, 1, serial);
        return p;
    }

    /// <summary>Seven-byte 0x07.</summary>
    public static byte[] Pickup(uint serial, ushort count) {
        var p = new byte[7];
        p[0] = 0x07;
        WriteU32(p, 1, serial);
        WriteU16(p, 5, count == 0 ? (ushort)1 : count);
        return p;
    }

    /// <summary>0x08. Fourteen bytes, or fifteen from client 6.0.1.7 with the slot byte set to 0.</summary>
    public static byte[] Drop(uint serial, ushort x, ushort y, sbyte z, uint container) {
        var grid = _version >= ClientVersion.CV_6017;
        var p = new byte[grid ? 15 : 14];
        p[0] = 0x08;
        WriteU32(p, 1, serial);
        WriteU16(p, 5, x);
        WriteU16(p, 7, y);
        p[9] = (byte)z;
        var at = 10;
        if (grid) p[at++] = 0;
        WriteU32(p, at, container);
        return p;
    }

    /// <summary>Nineteen-byte 0x6C. Kind 0 is an object. Z is written as a ushort of the signed byte.</summary>
    public static byte[] TargetObject(uint cursorId, byte cursorType, uint serial, int x, int y, int z, ushort graphic)
        => Target(0, cursorId, cursorType, serial, x, y, z, graphic);

    /// <summary>Nineteen-byte 0x6C. Kind 1 is the ground. The entity is 0.</summary>
    public static byte[] TargetGround(uint cursorId, byte cursorType, int x, int y, int z)
        => Target(1, cursorId, cursorType, 0, x, y, z, 0);

    /// <summary>Nineteen-byte 0x6C cancel. The first byte is the cursor kind the server sent.</summary>
    public static byte[] TargetCancel(byte kind, uint cursorId, byte cursorType) {
        var p = new byte[19];
        p[0] = 0x6C;
        p[1] = kind;
        WriteU32(p, 2, cursorId);
        p[6] = cursorType;
        p[11] = p[12] = p[13] = p[14] = 0xFF;
        return p;
    }

    /// <summary>0x6C. After the id: cursor kind, cursor id, target type.</summary>
    public static bool TargetCursor(byte[] p, int o, int len, out byte kind, out uint id, out byte type) {
        kind = 0;
        id = 0;
        type = 0;
        if (len < 7 || o + 7 > p.Length) return false;
        kind = p[o + 1];
        id = ReadU32(p, o + 2);
        type = p[o + 6];
        return true;
    }

    /// <summary>Thirteen-byte 0x7D. Code 0 is the cancel and the rest stays zero.</summary>
    public static byte[] MenuResponse(uint serial, ushort menu, ushort code, ushort graphic, ushort hue) {
        var p = new byte[13];
        p[0] = 0x7D;
        WriteU32(p, 1, serial);
        WriteU16(p, 5, menu);
        if (code != 0) {
            WriteU16(p, 7, code);
            WriteU16(p, 9, graphic);
            WriteU16(p, 11, hue);
        }
        return p;
    }

    /// <summary>0x7C. A zero peek of the next ushort is the gray menu and those four bytes are skipped.</summary>
    public static bool Menu(byte[] p, int o, int len, out UOMenu menu) {
        menu = null;
        if (len < 9 || o + len > p.Length) return false;
        var end = o + len;
        var i = o + 3;
        if (i + 6 > end) return false;
        var serial = ReadU32(p, i);
        i += 4;
        var id = ReadU16(p, i);
        i += 2;
        var nameLen = p[i++];
        if (i + nameLen >= end) return false;
        var name = ReadAscii(p, i, nameLen);
        i += nameLen;
        if (i >= end) return false;
        var count = p[i++];
        var gray = false;
        if (count > 0 && i + 2 <= end) gray = ReadU16(p, i) == 0;
        var entries = new UOMenuEntry[count];
        for (var n = 0; n < count; n++) {
            ushort graphic = 0, hue = 0;
            if (gray) {
                if (i + 4 > end) break;
                i += 4;
            }
            else {
                if (i + 4 > end) break;
                graphic = ReadU16(p, i);
                i += 2;
                hue = ReadU16(p, i);
                i += 2;
            }
            if (i >= end) break;
            var nlen = (int)p[i++];
            if (i + nlen > end) nlen = end - i;
            entries[n] = new UOMenuEntry(graphic, hue, ReadAscii(p, i, nlen));
            i += nlen;
        }
        menu = new UOMenu(serial, id, name, gray, entries);
        return true;
    }

    /// <summary>0x9A. The id is the uint64, then 0 for cancel or 1 for ok, then ASCII and a null.</summary>
    public static byte[] PromptAscii(ulong id, string text, bool cancel) {
        if (text == null) text = "";
        if (text.Length > 128) text = text.Substring(0, 128);
        var bytes = Encoding.ASCII.GetBytes(text);
        var p = new byte[16 + bytes.Length];
        p[0] = 0x9A;
        var len = p.Length;
        p[1] = (byte)(len >> 8);
        p[2] = (byte)len;
        WriteU64(p, 3, id);
        if (!cancel) WriteU32(p, 11, 1);
        Buffer.BlockCopy(bytes, 0, p, 15, bytes.Length);
        return p;
    }

    /// <summary>0xC2. Language is ENU and a null. The text is UTF-16LE with no extra null.</summary>
    public static byte[] PromptUnicode(ulong id, string text, bool cancel) {
        if (text == null) text = "";
        if (text.Length > 128) text = text.Substring(0, 128);
        var chars = Encoding.Unicode.GetBytes(text);
        var p = new byte[19 + chars.Length];
        p[0] = 0xC2;
        var len = p.Length;
        p[1] = (byte)(len >> 8);
        p[2] = (byte)len;
        WriteU64(p, 3, id);
        if (!cancel) WriteU32(p, 11, 1);
        p[15] = (byte)'E';
        p[16] = (byte)'N';
        p[17] = (byte)'U';
        Buffer.BlockCopy(chars, 0, p, 19, chars.Length);
        return p;
    }

    /// <summary>0x9A and 0xC2. The uint64 sits after the length word.</summary>
    public static bool Prompt(byte[] p, int o, int len, out ulong id) {
        id = 0;
        if (len < 11 || o + 11 > p.Length) return false;
        id = ReadU64(p, o + 3);
        return true;
    }

    /// <summary>0xAC. The character count includes the null, and the ASCII field is that long.</summary>
    public static byte[] TextEntryReply(uint serial, byte parent, byte button, string text, bool ok) {
        if (text == null) text = "";
        if (text.Length > 239) text = text.Substring(0, 239);
        var bytes = Encoding.ASCII.GetBytes(text);
        var p = new byte[13 + bytes.Length];
        p[0] = 0xAC;
        var len = p.Length;
        p[1] = (byte)(len >> 8);
        p[2] = (byte)len;
        WriteU32(p, 3, serial);
        p[7] = parent;
        p[8] = button;
        p[9] = ok ? (byte)1 : (byte)0;
        WriteU16(p, 10, (ushort)(text.Length + 1));
        Buffer.BlockCopy(bytes, 0, p, 12, bytes.Length);
        return p;
    }

    /// <summary>0xAB. Max length is capped at 255 for the text box.</summary>
    public static bool TextEntry(byte[] p, int o, int len, out UOTextEntry entry) {
        entry = null;
        if (len < 15 || o + len > p.Length) return false;
        var end = o + len;
        var i = o + 3;
        if (i + 8 > end) return false;
        var serial = ReadU32(p, i);
        i += 4;
        var parent = p[i++];
        var button = p[i++];
        var textLen = ReadU16(p, i);
        i += 2;
        if (i + textLen > end) return false;
        var text = ReadAscii(p, i, textLen);
        i += textLen;
        if (i + 8 > end) return false;
        var cancel = p[i++] != 0;
        i++;
        var max = ReadU32(p, i);
        i += 4;
        var descLen = ReadU16(p, i);
        i += 2;
        if (i + descLen > end) descLen = (ushort)(end > i ? end - i : 0);
        var desc = ReadAscii(p, i, descLen);
        var cap = max == 0 || max > 255 ? 255 : (int)max;
        entry = new UOTextEntry(serial, parent, button, text, desc, cancel, cap);
        return true;
    }

    /// <summary>Empty 0x3B. Eight bytes: the vendor serial and a zero count.</summary>
    public static byte[] Buy(uint serial) {
        var p = new byte[8];
        p[0] = 0x3B;
        p[2] = 8;
        WriteU32(p, 3, serial);
        return p;
    }

    /// <summary>Empty 0x9F. Nine bytes: the vendor serial and a zero count.</summary>
    public static byte[] Sell(uint serial) {
        var p = new byte[9];
        p[0] = 0x9F;
        p[2] = 9;
        WriteU32(p, 3, serial);
        return p;
    }

    /// <summary>0x74. The serial is the buy container. Each row is a price then a name. Names are not stored.</summary>
    public static bool BuyList(byte[] p, int o, int len, out uint serial, List<uint> prices) {
        serial = 0;
        prices?.Clear();
        if (len < 8 || o + 8 > p.Length) return false;
        serial = ReadU32(p, o + 3);
        if (serial == 0) return false;
        var end = o + len;
        var i = o + 7;
        if (i >= end) return true;
        var count = p[i++];
        for (var n = 0; n < count; n++) {
            if (i + 4 > end) break;
            var price = ReadU32(p, i);
            i += 4;
            if (i >= end) break;
            var nameLen = p[i++];
            prices?.Add(price);
            if (i + nameLen > end) break;
            i += nameLen;
        }
        return true;
    }

    /// <summary>0x9E. A zero count does not open a gump. The serial is the vendor.</summary>
    public static bool SellList(byte[] p, int o, int len, out uint serial) {
        serial = 0;
        if (len < 9 || o + 9 > p.Length) return false;
        if (ReadU16(p, o + 7) == 0) return false;
        serial = ReadU32(p, o + 3);
        return serial != 0;
    }

    /// <summary>0x6F cancel. Eight bytes: code 1 and the container serial.</summary>
    public static byte[] TradeCancel(uint serial) {
        var p = new byte[8];
        p[0] = 0x6F;
        p[2] = 8;
        p[3] = 1;
        WriteU32(p, 4, serial);
        return p;
    }

    /// <summary>0x6F. Type 0 is open and type 1 is close. The name follows the two ids when that flag is set.</summary>
    public static bool Trade(byte[] p, int o, int len, out byte type, out uint serial, out string name) {
        type = 0;
        serial = 0;
        name = "";
        if (len < 8 || o + 8 > p.Length) return false;
        type = p[o + 3];
        serial = ReadU32(p, o + 4);
        if (serial == 0) return false;
        if (type == 0 && len >= 17 && o + 16 < p.Length && p[o + 16] != 0)
            name = ReadAscii(p, o + 17, len - 17);
        return true;
    }

    /// <summary>Nine-byte 0x95. The ushort after the serial stays 0. The graphic is not written back.</summary>
    public static byte[] Dye(uint serial, ushort hue) {
        var p = new byte[9];
        p[0] = 0x95;
        WriteU32(p, 1, serial);
        WriteU16(p, 7, hue);
        return p;
    }

    /// <summary>Nine-byte 0x95. Serial, two skipped bytes, then the graphic.</summary>
    public static bool DyeWindow(byte[] p, int o, int len, out uint serial, out ushort graphic) {
        serial = 0;
        graphic = 0;
        if (len < 9 || o + 9 > p.Length) return false;
        serial = ReadU32(p, o + 1);
        graphic = ReadU16(p, o + 7);
        return serial != 0;
    }

    /// <summary>0x3B. The serial is the vendor whose shop closes. No buy is built from this.</summary>
    public static bool VendorClose(byte[] p, int o, int len, out uint serial) {
        serial = 0;
        if (len < 7 || o + 7 > p.Length) return false;
        serial = ReadU32(p, o + 3);
        return serial != 0;
    }

    /// <summary>Seven-byte 0x0B from client 5.0.0a. A zero amount is not a hit.</summary>
    public static bool Damage(byte[] p, int o, int len, out uint serial, out ushort amount) {
        serial = 0;
        amount = 0;
        if (len != 7 || o + 7 > p.Length) return false;
        serial = ReadU32(p, o + 1);
        amount = ReadU16(p, o + 5);
        return serial != 0 && amount > 0;
    }

    /// <summary>0x98. The name runs to the end of the frame.</summary>
    public static bool UpdateName(byte[] p, int o, int len, out uint serial, out string name) {
        serial = 0;
        name = "";
        if (len < 8 || o + len > p.Length || o + 7 > p.Length) return false;
        serial = ReadU32(p, o + 3);
        name = ReadAsciiZ(p, o + 7, o + len);
        return true;
    }

    /// <summary>0x54. The index is the ushort after the mode byte.</summary>
    public static bool Sound(byte[] p, int o, int len, out ushort index) {
        index = 0;
        if (len < 5 || o + 4 > p.Length) return false;
        index = ReadU16(p, o + 2);
        return true;
    }

    /// <summary>0x6D. Three bytes. Command 0x1F and index 0xFF stop the song.</summary>
    public static bool Music(byte[] p, int o, int len, out byte index, out bool stop) {
        index = 0;
        stop = false;
        if (len < 3 || o + 3 > p.Length) return false;
        stop = p[o + 1] == 0x1F && p[o + 2] == 0xFF;
        index = p[o + 2];
        return true;
    }

    /// <summary>0x65. Type, count, and temperature. Nothing is drawn.</summary>
    public static bool Weather(byte[] p, int o, int len, out byte type, out byte count, out byte temp) {
        type = count = temp = 0;
        if (len < 4 || o + 4 > p.Length) return false;
        type = p[o + 1];
        count = p[o + 2];
        temp = p[o + 3];
        return true;
    }

    /// <summary>Five-byte 0xAA. The serial follows the id.</summary>
    public static bool Attacked(byte[] p, int o, int len, out uint serial) {
        serial = 0;
        if (len < 5 || o + 5 > p.Length) return false;
        serial = ReadU32(p, o + 1);
        return true;
    }

    static byte[] Target(byte kind, uint cursorId, byte cursorType, uint serial, int x, int y, int z, ushort graphic) {
        var p = new byte[19];
        p[0] = 0x6C;
        p[1] = kind;
        WriteU32(p, 2, cursorId);
        p[6] = cursorType;
        WriteU32(p, 7, serial);
        WriteU16(p, 11, (ushort)x);
        WriteU16(p, 13, (ushort)y);
        WriteU16(p, 15, (ushort)(sbyte)z);
        WriteU16(p, 17, graphic);
        return p;
    }

    static bool ReadContained(byte[] p, int i, bool grid, out UOContainedState item) {
        var serial = ReadU32(p, i);
        i += 4;
        var graphic = (ushort)(ReadU16(p, i) + p[i + 2]);
        i += 3;
        var amount = ReadU16(p, i);
        i += 2;
        if (amount == 0) amount = 1;
        var x = ReadU16(p, i);
        i += 2;
        var y = ReadU16(p, i);
        i += 2;
        if (grid) i++;
        var container = ReadU32(p, i);
        i += 4;
        var hue = ReadU16(p, i);
        item = new UOContainedState(serial, graphic, amount, x, y, hue, container);
        return true;
    }

    static void WriteAscii(byte[] dest, int offset, string value) {
        if (string.IsNullOrEmpty(value)) return;
        var bytes = System.Text.Encoding.ASCII.GetBytes(value);
        var n = bytes.Length < 30 ? bytes.Length : 30;
        Buffer.BlockCopy(bytes, 0, dest, offset, n);
    }

    static void WriteU32(byte[] dest, int offset, uint value) {
        dest[offset] = (byte)(value >> 24);
        dest[offset + 1] = (byte)(value >> 16);
        dest[offset + 2] = (byte)(value >> 8);
        dest[offset + 3] = (byte)value;
    }

    static void WriteU16(byte[] dest, int offset, ushort value) {
        dest[offset] = (byte)(value >> 8);
        dest[offset + 1] = (byte)value;
    }

    static void WriteU64(byte[] dest, int offset, ulong value) {
        WriteU32(dest, offset, (uint)(value >> 32));
        WriteU32(dest, offset + 4, (uint)value);
    }

    static ulong ReadU64(byte[] src, int offset)
        => ((ulong)ReadU32(src, offset) << 32) | ReadU32(src, offset + 4);

    static string ReadAscii(byte[] src, int offset, int max) {
        var n = 0;
        var end = offset + max;
        if (end > src.Length) end = src.Length;
        while (offset + n < end && src[offset + n] != 0) n++;
        return n == 0 ? "" : System.Text.Encoding.ASCII.GetString(src, offset, n);
    }

    static string ReadAsciiZ(byte[] src, int offset, int end) {
        if (end > src.Length) end = src.Length;
        var n = offset;
        while (n < end && src[n] != 0) n++;
        return n == offset ? "" : System.Text.Encoding.ASCII.GetString(src, offset, n - offset);
    }

    static byte[] Query34(byte type, uint serial) {
        var p = new byte[10];
        p[0] = 0x34;
        WriteU32(p, 1, 0xEDEDEDED);
        p[5] = type;
        WriteU32(p, 6, serial);
        return p;
    }

    /// <summary>cliloc.enu. A missing id is #id. Tilde arguments use the same replacement as 0xC1.</summary>
    public static string ClilocText(int id, string args = null) {
        var text = LookupCliloc(id);
        if (string.IsNullOrEmpty(text)) text = "#" + id;
        if (!string.IsNullOrEmpty(args) && text.IndexOf('~') >= 0) text = ApplyArgs(text, args);
        return text;
    }

    /// <summary>0xB1. Switches are big-endian uints. Each entry is an id, a character count, and UTF-16BE. Text is capped at 239 characters.</summary>
    public static byte[] GumpResponse(uint local, uint server, int button, uint[] switches, (ushort id, string text)[] entries) {
        if (switches == null) switches = Array.Empty<uint>();
        if (entries == null) entries = Array.Empty<(ushort, string)>();
        var size = 23 + switches.Length * 4;
        var texts = new string[entries.Length];
        for (var n = 0; n < entries.Length; n++) {
            var text = entries[n].text ?? "";
            if (text.Length > 239) text = text.Substring(0, 239);
            texts[n] = text;
            size += 4 + text.Length * 2;
        }
        var p = new byte[size];
        p[0] = 0xB1;
        WriteU16(p, 1, (ushort)size);
        var i = 3;
        WriteU32(p, i, local); i += 4;
        WriteU32(p, i, server); i += 4;
        WriteU32(p, i, (uint)button); i += 4;
        WriteU32(p, i, (uint)switches.Length); i += 4;
        for (var n = 0; n < switches.Length; n++) { WriteU32(p, i, switches[n]); i += 4; }
        WriteU32(p, i, (uint)entries.Length); i += 4;
        for (var n = 0; n < entries.Length; n++) {
            WriteU16(p, i, entries[n].id); i += 2;
            WriteU16(p, i, (ushort)texts[n].Length); i += 2;
            var bytes = Encoding.BigEndianUnicode.GetBytes(texts[n]);
            Buffer.BlockCopy(bytes, 0, p, i, bytes.Length);
            i += bytes.Length;
        }
        return p;
    }

    /// <summary>0xB0. Empty layout does not open. Text lines are a character count and UTF-16BE.</summary>
    public static bool PlainGump(byte[] p, int o, int len, out UOServerGump gump) {
        gump = null;
        if (p == null || len < 23 || o < 0 || o + len > p.Length) return false;
        var i = o + 3;
        var end = o + len;
        if (i + 18 > end) return false;
        var sender = ReadU32(p, i); i += 4;
        var id = ReadU32(p, i); i += 4;
        var x = (int)ReadU32(p, i); i += 4;
        var y = (int)ReadU32(p, i); i += 4;
        var cmdLen = ReadU16(p, i); i += 2;
        if (i + cmdLen > end) return false;
        var layout = cmdLen == 0 ? "" : Encoding.ASCII.GetString(p, i, cmdLen);
        i += cmdLen;
        if (i + 2 > end) return false;
        var count = ReadU16(p, i); i += 2;
        if (string.IsNullOrWhiteSpace(layout)) return false;
        gump = new UOServerGump(sender, id, x, y, layout, ReadLines(p, ref i, end, count));
        return true;
    }

    /// <summary>0xDD. The length field includes the following uncompressed-length uint. zlib, then UTF-8 layout.</summary>
    public static bool CompressedGump(byte[] p, int o, int len, out UOServerGump gump) {
        gump = null;
        if (p == null || len < 31 || o < 0 || o + len > p.Length) return false;
        var i = o + 3;
        var end = o + len;
        if (i + 24 > end) return false;
        var sender = ReadU32(p, i); i += 4;
        var id = ReadU32(p, i); i += 4;
        var x = (int)ReadU32(p, i); i += 4;
        var y = (int)ReadU32(p, i); i += 4;
        if (!InflateBlock(p, ref i, end, out var raw)) return false;
        var layout = Encoding.UTF8.GetString(raw).TrimEnd('\0');
        if (string.IsNullOrWhiteSpace(layout) || i + 4 > end) return false;
        var linesNum = ReadU32(p, i); i += 4;
        var lineCount = linesNum > 4096 ? 4096 : (int)linesNum;
        var lines = new string[lineCount];
        for (var n = 0; n < lines.Length; n++) lines[n] = "";
        if (linesNum != 0 && InflateBlock(p, ref i, end, out raw)) {
            var at = 0;
            lines = ReadLines(raw, ref at, raw.Length, lines.Length);
        }
        gump = new UOServerGump(sender, id, x, y, layout, lines);
        return true;
    }

    /// <summary>0xBF subcommand 4. The serial is the gump id. Button 0 closes without a reply.</summary>
    public static bool TryCloseGump(byte[] p, int o, int len, out uint serial, out int button) {
        serial = 0;
        button = 0;
        if (p == null || len < 13 || o < 0 || o + 13 > p.Length) return false;
        var sub = (p[o + 3] << 8) | p[o + 4];
        if (sub != 4) return false;
        serial = ReadU32(p, o + 5);
        button = (int)ReadU32(p, o + 9);
        return true;
    }

    /// <summary>
    /// 0xBF subcommand 0x0014. The subcommand is still in the packet.
    /// Mode at least 2 is a uint cliloc. The older row is a ushort cliloc plus 3000000.
    /// Flag 0x84 and flag 0x40 each skip a ushort. Flag 0x20 is the replaced graphic. Flag 0x01 sets hue 0x0386.
    /// </summary>
    public static bool TryPopupMenu(byte[] p, int o, int len, out UOPopupMenu menu) {
        menu = null;
        if (p == null || len < 12 || o < 0 || o + len > p.Length) return false;
        var sub = (p[o + 3] << 8) | p[o + 4];
        if (sub != 0x0014) return false;
        var end = o + len;
        var i = o + 5;
        if (i + 7 > end) return false;
        var mode = ReadU16(p, i);
        i += 2;
        var serial = ReadU32(p, i);
        i += 4;
        var count = p[i++];
        var modern = mode >= 2;
        var entries = new UOPopupEntry[count];
        for (var n = 0; n < count; n++) {
            ushort hue = 0xFFFF, replaced = 0;
            int cliloc;
            ushort index, flags;
            if (modern) {
                if (i + 8 > end) return false;
                cliloc = (int)ReadU32(p, i);
                i += 4;
                index = ReadU16(p, i);
                i += 2;
                flags = ReadU16(p, i);
                i += 2;
            }
            else {
                if (i + 6 > end) return false;
                index = ReadU16(p, i);
                i += 2;
                cliloc = ReadU16(p, i) + 3000000;
                i += 2;
                flags = ReadU16(p, i);
                i += 2;
                if ((flags & 0x84) != 0) {
                    if (i + 2 > end) return false;
                    i += 2;
                }
                if ((flags & 0x40) != 0) {
                    if (i + 2 > end) return false;
                    i += 2;
                }
                if ((flags & 0x20) != 0) {
                    if (i + 2 > end) return false;
                    replaced = ReadU16(p, i);
                    i += 2;
                }
            }
            if ((flags & 0x01) != 0) hue = 0x0386;
            entries[n] = new UOPopupEntry(cliloc, index, hue, replaced, flags);
        }
        menu = new UOPopupMenu(serial, entries);
        return true;
    }

    static string[] ReadLines(byte[] p, ref int i, int end, int count) {
        if (count < 0) count = 0;
        if (count > 4096) count = 4096;
        var lines = new string[count];
        for (var n = 0; n < count; n++) lines[n] = "";
        for (var n = 0; n < count; n++) {
            if (i + 2 > end) break;
            var nchars = ReadU16(p, i);
            i += 2;
            var nbytes = nchars * 2;
            if (i + nbytes > end) break;
            if (nchars > 0) lines[n] = Encoding.BigEndianUnicode.GetString(p, i, nbytes);
            i += nbytes;
        }
        return lines;
    }

    static bool InflateBlock(byte[] src, ref int i, int end, out byte[] dest) {
        dest = null;
        if (i + 8 > end) return false;
        var clenField = ReadU32(src, i); i += 4;
        var dlen = ReadU32(src, i); i += 4;
        if (clenField < 4 || dlen == 0 || dlen > 2_000_000) return false;
        var clenLong = (long)clenField - 4;
        if (clenLong <= 0 || clenLong > int.MaxValue || i + clenLong > end) return false;
        var clen = (int)clenLong;
        var at = i;
        i += clen;
        if (src[at] == 0x78 && clen >= 2 && InflateAt(src, at + 2, clen - 2, (int)dlen, out dest)) return true;
        return InflateAt(src, at, clen, (int)dlen, out dest);
    }

    static bool InflateAt(byte[] src, int offset, int count, int dlen, out byte[] dest) {
        dest = null;
        if (count <= 0 || dlen <= 0) return false;
        try {
            var buf = new byte[dlen];
            using var ms = new MemoryStream(src, offset, count, false);
            using var z = new DeflateStream(ms, CompressionMode.Decompress);
            var got = 0;
            while (got < dlen) {
                var r = z.Read(buf, got, dlen - got);
                if (r <= 0) break;
                got += r;
            }
            if (got <= 0) return false;
            if (got == dlen) dest = buf;
            else {
                dest = new byte[got];
                Buffer.BlockCopy(buf, 0, dest, 0, got);
            }
            return true;
        }
        catch { return false; }
    }

    static string LookupCliloc(int id) {
        var tables = Binary_StringTable.Current;
        if (tables == null) return "";
        if (tables.TryGetValue("enu", out var table) && table?.Strings != null && table.Strings.TryGetValue(id, out var text)) return text;
        if (tables.TryGetValue(".enu", out table) && table?.Strings != null && table.Strings.TryGetValue(id, out text)) return text;
        foreach (var kv in tables)
            if (kv.Value?.Strings != null && kv.Value.Strings.TryGetValue(id, out text)) return text;
        return "";
    }

    static string ApplyArgs(string text, string args) {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(args)) return text;
        var parts = SplitArgs(args);
        var sb = new System.Text.StringBuilder(text);
        var pos = 0;
        while (pos < sb.Length) {
            var start = IndexOf(sb, '~', pos);
            if (start < 0) break;
            var end = IndexOf(sb, '~', start + 1);
            if (end < 0) break;
            var token = sb.ToString(start + 1, end - start - 1);
            var under = token.IndexOf('_');
            var num = under < 0 ? token : token.Substring(0, under);
            if (!int.TryParse(num, out var index)) break;
            index--;
            var arg = index >= 0 && index < parts.Count ? parts[index] : "";
            if (arg.Length > 1 && arg[0] == '#' && int.TryParse(arg.Substring(1), out var id)) {
                var inner = LookupCliloc(id);
                if (!string.IsNullOrEmpty(inner)) arg = inner;
            }
            sb.Remove(start, end - start + 1);
            sb.Insert(start, arg);
            pos = start + arg.Length;
        }
        return sb.ToString();
    }

    static List<string> SplitArgs(string args) {
        var parts = new List<string>();
        var i = 0;
        while (i < args.Length && args[i] == '\t') i++;
        var start = i;
        for (; i <= args.Length; i++) {
            if (i == args.Length || args[i] == '\t') {
                parts.Add(i > start ? args.Substring(start, i - start) : "");
                start = i + 1;
                if (i == args.Length) break;
            }
        }
        return parts;
    }

    static int IndexOf(System.Text.StringBuilder sb, char c, int from) {
        for (var i = from; i < sb.Length; i++)
            if (sb[i] == c) return i;
        return -1;
    }

    static string ReadUtf16(byte[] src, int offset, int end) {
        if (end > src.Length) end = src.Length;
        var n = offset;
        while (n + 1 < end && (src[n] != 0 || src[n + 1] != 0)) n += 2;
        return n == offset ? "" : System.Text.Encoding.BigEndianUnicode.GetString(src, offset, n - offset);
    }

    static string ReadUtf16Le(byte[] src, int offset, int end) {
        if (end > src.Length) end = src.Length;
        var n = offset;
        while (n + 1 < end && (src[n] != 0 || src[n + 1] != 0)) n += 2;
        return n == offset ? "" : System.Text.Encoding.Unicode.GetString(src, offset, n - offset);
    }

    /// <summary>0xE2. Ten bytes. serial, type, action, mode.</summary>
    public static bool NewCharacterAnimation(byte[] p, int o, int len, out uint serial, out ushort type, out ushort action, out byte mode) {
        serial = 0;
        type = action = 0;
        mode = 0;
        if (len < 10 || o + 10 > p.Length) return false;
        serial = ReadU32(p, o + 1);
        type = ReadU16(p, o + 5);
        action = ReadU16(p, o + 7);
        mode = p[o + 9];
        return true;
    }

    /// <summary>28 bytes through explode. Hue is the next dword, except 0x23 and 0x70. A shorter frame fails closed.</summary>
    public static bool GraphicEffect(byte[] p, int o, int len, out byte type, out uint source, out uint target, out ushort graphic, out ushort hue, out ushort srcX, out ushort srcY, out sbyte srcZ, out ushort targetX, out ushort targetY, out sbyte targetZ, out byte speed, out byte duration, out bool fixedDirection, out bool explode) {
        type = speed = duration = 0;
        source = target = 0;
        graphic = hue = srcX = srcY = targetX = targetY = 0;
        srcZ = targetZ = 0;
        fixedDirection = explode = false;
        if (len < 28 || o + 28 > p.Length) return false;
        type = p[o + 1];
        source = ReadU32(p, o + 2);
        target = ReadU32(p, o + 6);
        graphic = ReadU16(p, o + 10);
        srcX = ReadU16(p, o + 12);
        srcY = ReadU16(p, o + 14);
        srcZ = (sbyte)p[o + 16];
        targetX = ReadU16(p, o + 17);
        targetY = ReadU16(p, o + 19);
        targetZ = (sbyte)p[o + 21];
        speed = p[o + 22];
        duration = p[o + 23];
        _ = ReadU16(p, o + 24);
        fixedDirection = p[o + 26] != 0;
        explode = p[o + 27] != 0;
        if (p[o] != 0x23 && p[o] != 0x70 && len >= 32 && o + 32 <= p.Length)
            hue = (ushort)ReadU32(p, o + 28);
        return true;
    }

    /// <summary>0x6E. Fourteen bytes. forward is the inverse of the bool, matching ClassicUO.</summary>
    public static bool CharacterAnimation(byte[] p, int o, int len, out uint serial, out ushort action, out ushort frames, out ushort repeat, out bool forward, out bool again, out byte delay) {
        serial = 0;
        action = frames = repeat = 0;
        forward = again = false;
        delay = 0;
        if (len < 14) return false;
        serial = ReadU32(p, o + 1);
        action = ReadU16(p, o + 5);
        frames = ReadU16(p, o + 7);
        repeat = ReadU16(p, o + 9);
        forward = p[o + 11] == 0;
        again = p[o + 12] != 0;
        delay = p[o + 13];
        return true;
    }

    static uint ReadU32(byte[] src, int offset)
        => ((uint)src[offset] << 24) | ((uint)src[offset + 1] << 16) | ((uint)src[offset + 2] << 8) | src[offset + 3];

    static ushort ReadU16(byte[] src, int offset)
        => (ushort)((src[offset] << 8) | src[offset + 1]);

    /// <summary>0x2F. Ten bytes. The byte after the id is skipped. This is not an animation.</summary>
    public static bool Swing(byte[] p, int o, int len, out uint attacker, out uint defender) {
        attacker = defender = 0;
        if (len < 10 || o + 10 > p.Length) return false;
        attacker = ReadU32(p, o + 2);
        defender = ReadU32(p, o + 6);
        return true;
    }

    /// <summary>0x16 and 0x17. Before 5.0.0a, 0x16 is one byte and updates nothing. Each row is a type and a bool.</summary>
    public static bool HealthBar(byte[] p, int o, int len, out uint serial, List<(ushort type, bool enabled)> rows) {
        serial = 0;
        rows?.Clear();
        if (rows == null || o < 0 || o >= p.Length) return false;
        if (p[o] == 0x16 && _version < ClientVersion.CV_500A) return false;
        if (len < 9 || o + 9 > p.Length) return false;
        serial = ReadU32(p, o + 3);
        if (serial == 0) return false;
        var count = ReadU16(p, o + 7);
        var i = o + 9;
        var end = o + len;
        for (var n = 0; n < count; n++) {
            if (i + 3 > end) break;
            var type = ReadU16(p, i);
            i += 2;
            rows.Add((type, p[i++] != 0));
        }
        return true;
    }

    /// <summary>0xBA. Six bytes, or ten from 7.0.9.0 when the serial follows the point.</summary>
    public static bool QuestArrow(byte[] p, int o, int len, out bool display, out ushort x, out ushort y, out uint serial) {
        display = false;
        x = y = 0;
        serial = 0;
        if (len < 6 || o + 6 > p.Length) return false;
        display = p[o + 1] != 0;
        x = ReadU16(p, o + 2);
        y = ReadU16(p, o + 4);
        if (_version >= ClientVersion.CV_7090) {
            if (len < 10 || o + 10 > p.Length) return false;
            serial = ReadU32(p, o + 6);
        }
        return true;
    }

    /// <summary>0x89. Layer 0 ends the list. A layer whose value minus one is the backpack is skipped.</summary>
    public static bool CorpseEquipment(byte[] p, int o, int len, out uint serial, List<UOCorpseEquip> rows) {
        serial = 0;
        rows?.Clear();
        if (rows == null || len < 8 || o + len > p.Length) return false;
        var end = o + len;
        var i = o + 3;
        if (i + 4 > end) return false;
        serial = ReadU32(p, i);
        i += 4;
        if (serial == 0 || i >= end) return false;
        var layer = p[i++];
        while (layer != 0 && i + 4 <= end) {
            var item = ReadU32(p, i);
            i += 4;
            var worn = (byte)(layer - 1);
            if (worn != 0x15) rows.Add(new UOCorpseEquip(item, worn, serial));
            if (i >= end) break;
            layer = p[i++];
        }
        return true;
    }

    /// <summary>0x2D. Seventeen bytes. Hits, then mana, then stamina. Each pair is max, then current.</summary>
    public static bool MobileAttributes(byte[] p, int o, int len, out uint serial, out ushort hits, out ushort hitsMax, out ushort mana, out ushort manaMax, out ushort stam, out ushort stamMax) {
        serial = 0;
        hits = hitsMax = mana = manaMax = stam = stamMax = 0;
        if (len < 17 || o + 17 > p.Length) return false;
        serial = ReadU32(p, o + 1);
        hitsMax = ReadU16(p, o + 5);
        hits = ReadU16(p, o + 7);
        manaMax = ReadU16(p, o + 9);
        mana = ReadU16(p, o + 11);
        stamMax = ReadU16(p, o + 13);
        stam = ReadU16(p, o + 15);
        return true;
    }

    /// <summary>0xDE. Serial, then a status byte. Status 1 is followed by the attacker serial.</summary>
    public static bool UpdateMobileStatus(byte[] p, int o, int len, out uint serial, out byte status, out uint attacker) {
        serial = attacker = 0;
        status = 0;
        if (len < 8 || o + 8 > p.Length) return false;
        serial = ReadU32(p, o + 3);
        status = p[o + 7];
        if (status == 1) {
            if (len < 12 || o + 12 > p.Length) return false;
            attacker = ReadU32(p, o + 8);
        }
        return true;
    }

    /// <summary>0xDC. Nine bytes. Serial, then the revision compared with the last 0xD6.</summary>
    public static bool OplInfo(byte[] p, int o, int len, out uint serial, out uint revision) {
        serial = revision = 0;
        if (len < 9 || o + 9 > p.Length) return false;
        serial = ReadU32(p, o + 1);
        revision = ReadU32(p, o + 5);
        return true;
    }

    /// <summary>0x38. Seven bytes. One point: x, y, then z.</summary>
    public static bool Pathfinding(byte[] p, int o, int len, out ushort x, out ushort y, out ushort z) {
        x = y = z = 0;
        if (len < 7 || o + 7 > p.Length) return false;
        x = ReadU16(p, o + 1);
        y = ReadU16(p, o + 3);
        z = ReadU16(p, o + 5);
        return true;
    }

    /// <summary>0xD6. Name is the first cliloc line. Data is the rest, separated by newlines.</summary>
    public static bool MegaCliloc(byte[] p, int o, int len, int playerStr, out uint serial, out string name, out string data) {
        serial = 0;
        name = "";
        data = "";
        if (len < 15 || o + 15 > p.Length || o + len > p.Length) return false;
        if (ReadU16(p, o + 3) > 1) return false;
        serial = ReadU32(p, o + 5);
        var end = o + len;
        var i = o + 15;
        var list = new List<(int cliloc, string text)>();
        while (i + 4 <= end) {
            var cliloc = (int)ReadU32(p, i);
            i += 4;
            if (cliloc == 0) break;
            if (i + 2 > end) break;
            var length = ReadU16(p, i);
            i += 2;
            var argument = "";
            if (length != 0) argument = ReadUtf16LeCount(p, ref i, end, length / 2);
            var str = Capitalize(ClilocText(cliloc, argument));
            if (cliloc == 1080418 && _version >= ClientVersion.CV_60143)
                str = "<basefont color=#40a4fe>" + str + "</basefont>";
            else if (cliloc == 1061170 && int.TryParse(argument, out var need) && playerStr < need)
                str = "<basefont color=#FF0000>" + str + "</basefont>";
            else if (cliloc == 1062613)
                str = "<basefont color=#FFCC33>" + str + "</basefont>";
            else if (cliloc == 1159561)
                str = "<basefont color=#b66dff>" + str + "</basefont>";
            for (var n = 0; n < list.Count; n++) {
                if (list[n].cliloc == cliloc && string.Equals(list[n].text, str, StringComparison.Ordinal)) {
                    list.RemoveAt(n);
                    break;
                }
            }
            list.Add((cliloc, str));
        }
        if (list.Count == 0) return true;
        name = list[0].text;
        if (list.Count == 1) return true;
        var sb = new StringBuilder();
        for (var n = 1; n < list.Count; n++) {
            if (sb.Length != 0) sb.Append('\n');
            sb.Append(list[n].text);
        }
        data = sb.ToString();
        return true;
    }

    /// <summary>0xDF. Count 0 removes the icon. The graphic is the buff table, not the packet's icon word.</summary>
    public static bool BuffDebuff(byte[] p, int o, int len, out ushort type, out bool remove, List<UOBuffState> rows) {
        type = 0;
        remove = false;
        rows?.Clear();
        if (rows == null || len < 11 || o + 11 > p.Length || o + len > p.Length) return false;
        var end = o + len;
        var i = o + 7;
        var ic = ReadU16(p, i);
        i += 2;
        type = ic;
        const ushort start = 0x03E9;
        const ushort startNew = 0x466;
        var iconID = ic >= startNew ? (ushort)(ic - (startNew - 125)) : (ushort)(ic - start);
        if (iconID >= BuffGraphics.Length) return false;
        if (i + 2 > end) return false;
        var count = ReadU16(p, i);
        i += 2;
        if (count == 0) {
            remove = true;
            return true;
        }
        var graphic = BuffGraphics[iconID];
        for (var n = 0; n < count; n++) {
            if (!ReadBuff(p, ref i, end, type, graphic, out var row)) break;
            rows.Add(row);
        }
        return rows.Count > 0;
    }

    static bool ReadBuff(byte[] p, ref int i, int end, ushort type, ushort graphic, out UOBuffState row) {
        row = default;
        if (i + 31 > end) return false;
        i += 12;
        var timer = ReadU16(p, i);
        i += 2;
        i += 3;
        var titleId = ReadU32(p, i);
        i += 4;
        var descId = ReadU32(p, i);
        i += 4;
        var wtfId = ReadU32(p, i);
        i += 4;
        i += 2;
        var args = ReadUtf16LeCount(p, ref i, end, 2) + ReadUtf16LeZ(p, ref i, end);
        if (i + 2 > end) return false;
        i += 2;
        var args2 = ReadUtf16LeZ(p, ref i, end);
        var description = "";
        if (descId != 0) {
            description = "\n" + Capitalize(ClilocText((int)descId, string.IsNullOrEmpty(args2) ? args : args2));
            if (description.Length < 2) description = "";
        }
        if (i + 2 > end) return false;
        i += 2;
        var args3 = ReadUtf16LeZ(p, ref i, end);
        var wtf = "";
        if (wtfId != 0) {
            wtf = Capitalize(ClilocText((int)wtfId, string.IsNullOrEmpty(args3) ? args : args3));
            if (!string.IsNullOrWhiteSpace(wtf)) wtf = "\n" + wtf;
        }
        var title = Capitalize(ClilocText((int)titleId, args));
        row = new UOBuffState(type, graphic, timer, "<left>" + title + description + wtf + "</left>");
        return true;
    }

    static string ReadUtf16LeCount(byte[] p, ref int i, int end, int chars) {
        if (chars <= 0 || i + 2 > end) return "";
        var want = chars * 2;
        var size = i + want <= end ? want : (end - i) & ~1;
        var n = 0;
        while (n + 1 < size && (p[i + n] != 0 || p[i + n + 1] != 0)) n += 2;
        var s = n == 0 ? "" : Encoding.Unicode.GetString(p, i, n);
        i = i + want <= end ? i + want : end;
        return s;
    }

    static string ReadUtf16LeZ(byte[] p, ref int i, int end) {
        if (i + 2 > end) return "";
        var n = i;
        while (n + 1 < end && (p[n] != 0 || p[n + 1] != 0)) n += 2;
        var s = n == i ? "" : Encoding.Unicode.GetString(p, i, n - i);
        i = n + 1 < end ? n + 2 : n;
        return s;
    }

    static string Capitalize(string str) {
        if (string.IsNullOrEmpty(str)) return "";
        var chars = str.ToCharArray();
        var next = true;
        for (var i = 0; i < chars.Length; i++) {
            if (next) chars[i] = char.ToUpper(chars[i]);
            if (!char.IsWhiteSpace(chars[i]))
                next = i + 1 < chars.Length && char.IsWhiteSpace(chars[i + 1]);
        }
        return new string(chars);
    }

    /// <summary>ClassicUO BuffTable default graphics. Index 0 is icon 0x03E9. Index 125 is icon 0x0466.</summary>
    static readonly ushort[] BuffGraphics = {
        0x754C, 0x754A, 0x0000, 0x0000, 0x755E, 0x7549, 0x7551, 0x7556, 0x753A, 0x754D, 0x754E, 0x7565, 0x753B, 0x7543, 0x7544, 0x7546,
        0x755C, 0x755F, 0x7566, 0x7554, 0x7540, 0x7568, 0x754F, 0x7550, 0x7553, 0x753E, 0x755D, 0x7563, 0x7562, 0x753F, 0x7559, 0x7557,
        0x754B, 0x753D, 0x7561, 0x7558, 0x755B, 0x7560, 0x7541, 0x7545, 0x7552, 0x7569, 0x7548, 0x755A, 0x753C, 0x7547, 0x7567, 0x7542,
        0x758A, 0x758B, 0x758C, 0x758D, 0x0000, 0x758E, 0x094B, 0x094C, 0x094D, 0x094E, 0x094F, 0x0950, 0x753E, 0x5011, 0x7590, 0x7591,
        0x7592, 0x7593, 0x7594, 0x7595, 0x7596, 0x7598, 0x7599, 0x759B, 0x759C, 0x759E, 0x759F, 0x75A0, 0x75A1, 0x75A3, 0x75A4, 0x75A5,
        0x75A6, 0x75A7, 0x75C0, 0x75C1, 0x75C2, 0x75C3, 0x75C4, 0x75F2, 0x75F3, 0x75F4, 0x75F5, 0x75F6, 0x75F7, 0x75F8, 0x75F9, 0x75FA,
        0x75FB, 0x75FC, 0x75FD, 0x75FE, 0x75FF, 0x7600, 0x7601, 0x7602, 0x7603, 0x7604, 0x7605, 0x7606, 0x7607, 0x7608, 0x7609, 0x760A,
        0x760B, 0x760C, 0x760D, 0x760E, 0x760F, 0x7610, 0x7611, 0x7612, 0x7613, 0x7614, 0x7615, 0x75C5, 0x75F6, 0x761B, 0x9bc9, 0x9bb5,
        0x9bdd, 0x9bc6, 0x9bcc, 0x9bbe, 0x9bbd, 0x9bcb, 0x9bc8, 0x9bbf, 0x9bcd, 0x9bc0, 0x9bce, 0x9bc1, 0x9bc7, 0x9bc2, 0x9bb7, 0x9bca,
        0x9bb6, 0x9bb8, 0x9bb9, 0x9bba, 0x9bbb, 0x9bbc, 0x9bc3, 0x9bc4, 0x9bc5, 0x9bd2, 0x9bd3, 0x9bd4, 0x9bd5, 0x9bd1, 0x9bd6, 0x9bd7,
        0x9bcf, 0x9bd8, 0x9bd9, 0x9bdb, 0x9bdc, 0x9bda, 0x9bd0, 0x9bde, 0x9bdf, 0xC349, 0xC34D, 0xC34E, 0xC34C, 0xC34B, 0xC34A, 0xC343,
        0xC345, 0xC346, 0xC347, 0xC348, 0x9CDE, 0x5DE1, 0x5DDF, 0x5DE3, 0x5DE5, 0x5DE4, 0x5DE6, 0x5D51, 0x0951,
    };
}
