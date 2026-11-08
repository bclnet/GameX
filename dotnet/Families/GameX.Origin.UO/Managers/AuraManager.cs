// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO Aura. Circle opacity is kept. The last radius and hue are stored. No shader draw.
/// </summary>
public sealed class Aura : IDisposable {
    readonly int _radius;
    Pixel[] _colors;

    public Aura(int radius) {
        _radius = radius;
        LastRadius = radius;
    }

    public int LastRadius { get; private set; }
    public ushort LastHue { get; private set; }
    public float LastHueX { get; private set; }
    public float LastHueY { get; private set; }
    public float LastHueZ { get; private set; }
    public int LastX { get; private set; }
    public int LastY { get; private set; }
    public int LastLowerY { get; private set; }
    public float LastDepth { get; private set; }
    public int ColorCount => _colors == null ? 0 : _colors.Length;

    public void Draw(int x, int y, ushort hue, float depth) {
        const int yBump = -5;
        if (_colors == null) _colors = GenerateBlendedCircleColors(_radius);

        x -= _radius;
        y -= _radius;
        LastRadius = _radius;
        LastHue = hue;
        LastX = x;
        LastY = y + yBump;
        LastLowerY = y + _radius;
        LastDepth = depth + 1f;
        TranslateHue(hue, out var hx, out var hy, out var hz);
        LastHueX = hx;
        LastHueY = hy;
        LastHueZ = hz;
    }

    static void TranslateHue(ushort hue, out float x, out float y, out float z) {
        const int Spectral = 0x4000;
        var partial = false;
        var h = (int)hue;
        if ((h & 0x8000) != 0) {
            partial = true;
            h &= 0x7FFF;
        }
        if (h == 0) partial = false;

        byte type;
        if ((h & Spectral) != 0) type = 7;
        else if (h != 0) {
            h -= 1;
            type = partial ? (byte)2 : (byte)1;
        }
        else type = 0;

        x = h;
        y = type;
        z = 1f;
    }

    static Pixel[] GenerateBlendedCircleColors(int radius) {
        var width = radius * 2;
        var height = radius * 2;
        var blendedColors = new Pixel[width * height];
        for (int py = 0; py < height; py++) {
            for (int px = 0; px < width; px++) {
                var distance = (int)Math.Sqrt(Math.Pow(px - radius, 2) + Math.Pow(py - radius, 2));
                if (distance > radius) {
                    blendedColors[px + py * width] = Pixel.Transparent;
                    continue;
                }
                var opacityFactor = 1f - distance / (float)radius;
                blendedColors[px + py * width] = Pixel.White * opacityFactor;
            }
        }
        return blendedColors;
    }

    public void Dispose() => _colors = null;

    readonly struct Pixel {
        public readonly byte R, G, B, A;
        Pixel(byte r, byte g, byte b, byte a) { R = r; G = g; B = b; A = a; }
        public static Pixel Transparent => new Pixel(0, 0, 0, 0);
        public static Pixel White => new Pixel(255, 255, 255, 255);
        public static Pixel operator *(Pixel c, float scale) => new Pixel(Scale(c.R, scale), Scale(c.G, scale), Scale(c.B, scale), Scale(c.A, scale));
        static byte Scale(byte v, float scale) {
            var n = (int)(v * scale);
            if (n < 0) return 0;
            if (n > 255) return 255;
            return (byte)n;
        }
    }
}

/// <summary>
/// ClassicUO AuraManager. AuraUnderFeetType stays in memory. Ctrl is not on Mouse.
/// </summary>
public sealed class AuraManager {
    readonly World _world;
    readonly Aura _aura;
    int _saveAuraUnderFeetType;

    // ClassicUO Keyboard.Ctrl. Mouse is GameX.Origin.UO.Mouse and has no key state.
    static bool Ctrl => false;

    public AuraManager(World world) {
        _world = world;
        _aura = new Aura(40);
    }

    // Profile path is not ported.
    public int AuraUnderFeetType { get; set; }

    public bool IsEnabled {
        get {
            switch (AuraUnderFeetType) {
                default:
                case 0: return false;
                case 1 when _world != null && UOLoginNet.PlayerSerial != 0 && UOLoginNet.InWarMode: return true;
                case 2 when Ctrl && GameController.Shift: return true;
                case 3: return true;
            }
        }
    }

    public void ToggleVisibility() {
        if (!IsEnabled) {
            _saveAuraUnderFeetType = AuraUnderFeetType;
            AuraUnderFeetType = 3;
        }
        else AuraUnderFeetType = _saveAuraUnderFeetType;
    }

    public void Draw(int x, int y, ushort hue, float depth) => _aura.Draw(x, y, hue, depth);
}
