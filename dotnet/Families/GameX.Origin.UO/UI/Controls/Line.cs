// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>ClassicUO Line. Packed color is R, G, B, A with red in the low byte.</summary>
public class Line : Control {
    readonly uint _color;

    public Line(int x, int y, int w, int h, uint color) {
        X = x;
        Y = y;
        Width = w;
        Height = h;
        _color = color;
    }

    public uint Color => _color;

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        var a = (int)((_color >> 24) & 0xFF);
        if (a != 0 && Alpha > 0 && Width > 0 && Height > 0) {
            var r = (int)((_color & 0xFF) * 31 / 255);
            var g = (int)(((_color >> 8) & 0xFF) * 31 / 255);
            var b = (int)(((_color >> 16) & 0xFF) * 31 / 255);
            if (a < 255) {
                r = r * a / 255;
                g = g * a / 255;
                b = b * a / 255;
            }
            if (Alpha < 1f) {
                r = (int)(r * Alpha);
                g = (int)(g * Alpha);
                b = (int)(b * Alpha);
            }
            if (r < 0) r = 0;
            if (g < 0) g = 0;
            if (b < 0) b = 0;
            if (r > 31) r = 31;
            if (g > 31) g = 31;
            if (b > 31) b = 31;
            var pixel = (ushort)(0x8000 | (r << 10) | (g << 5) | b);
            var px = new[] { (byte)pixel, (byte)(pixel >> 8) };
            var key = unchecked((int)(0x5F300000u ^ _color ^ (uint)(Alpha * 255f)));
            GumpArt.Sink?.DrawRaw(key, x, y, Width, Height, px, 1, 1);
        }
        return base.Draw(x, y);
    }
}
