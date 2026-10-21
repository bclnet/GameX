// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>One mobile from the game socket.</summary>
public sealed class Mobile {
    public Mobile(uint serial, ushort body, int x, int y, int z, byte direction) {
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

/// <summary>One world item from packet 0x1A.</summary>
public sealed class Item {
    public Item(uint serial, ushort graphic, int x, int y, int z) {
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
