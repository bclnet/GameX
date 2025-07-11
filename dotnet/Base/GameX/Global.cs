﻿using System.Drawing;
using System.IO;

namespace GameX;

/// <summary>
/// A color without alpha (red, green, blue).
/// </summary>
public struct Color3 { //:M
    public float R; // Red color component.
    public float G; // Green color component.
    public float B; // Blue color component.

    public Color3() { }
    public Color3(BinaryReader r) {
        R = r.ReadSingle();
        G = r.ReadSingle();
        B = r.ReadSingle();
    }

    public Color ToColor() => Color.FromArgb((int)(R * 255f), (int)(G * 255f), (int)(B * 255f));
}

/// <summary>
/// A color without alpha (red, green, blue).
/// </summary>
public struct ByteColor3 { //:X
    public byte R;   // Red color component.
    public byte G;   // Green color component.
    public byte B;   // Blue color component.

    public ByteColor3() { }
    public ByteColor3(BinaryReader r) {
        R = r.ReadByte();
        G = r.ReadByte();
        B = r.ReadByte();
    }

    public Color ToColor() => Color.FromArgb(R, G, B);
}

/// <summary>
/// A color with alpha (red, green, blue, alpha).
/// </summary>
public struct Color4 { //:M
    public float R; // Red component.
    public float G; // Green component.
    public float B; // Blue component.
    public float A; // Alpha.

    public Color4() { }
    public Color4(BinaryReader r) {
        R = r.ReadSingle();
        G = r.ReadSingle();
        B = r.ReadSingle();
        A = r.ReadSingle();
    }
}

/// <summary>
/// A color with alpha (red, green, blue, alpha).
/// </summary>
public struct ByteColor4 { //:X
    public byte R; // Red component.
    public byte G; // Green component.
    public byte B; // Blue component.
    public byte A; // Alpha.

    public ByteColor4() { }
    public ByteColor4(BinaryReader r) {
        R = r.ReadByte();
        G = r.ReadByte();
        B = r.ReadByte();
        A = r.ReadByte();
    }
}
