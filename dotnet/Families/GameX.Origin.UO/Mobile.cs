// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>One mobile from the game socket.</summary>
public sealed class Mobile {
    public Mobile(uint serial, ushort body, int x, int y, int z, byte direction, byte notoriety = 0, byte flags = 0) {
        Serial = serial;
        Body = body;
        X = x;
        Y = y;
        Z = z;
        Direction = direction;
        Notoriety = notoriety;
        Flags = flags;
    }

    public uint Serial { get; }
    public ushort Body { get; }
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
    public byte Direction { get; }
    public byte Notoriety { get; }
    public byte Flags { get; }
}

/// <summary>One world item from packet 0x1A.</summary>
public sealed class Item {
    public Item(uint serial, ushort graphic, int x, int y, int z, ushort hue = 0, ushort amount = 0, uint container = 0, byte layer = 0) {
        Serial = serial;
        Graphic = graphic;
        X = x;
        Y = y;
        Z = z;
        Hue = hue;
        Amount = amount;
        Container = container;
        Layer = layer;
    }

    public uint Serial { get; }
    public ushort Graphic { get; }
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
    public ushort Hue { get; }
    public ushort Amount { get; }
    public uint Container { get; }
    public byte Layer { get; }
    public bool IsCorpse => Graphic == 0x2006;
    public bool OnGround => Container == 0;
}
