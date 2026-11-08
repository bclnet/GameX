// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>ClassicUO ColorBox. Hue 0 is white. A nonzero hue is ramp index 31.</summary>
public class ColorBox : Control {
    public ColorBox(int width, int height, ushort hue) {
        CanMove = false;
        Width = width;
        Height = height;
        Hue = hue;
        WantUpdateSize = false;
    }

    public ushort Hue { get; set; }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (Width > 0 && Height > 0) {
            var key = unchecked((int)(0x5F200000u + Hue));
            GumpArt.Sink?.DrawRaw(key, x, y, Width, Height, Solid(Hue), 1, 1);
        }
        return base.Draw(x, y);
    }

    static byte[] Solid(ushort hue) {
        ushort c;
        if (hue != 0 && UltimaOnline.Current != null && UltimaOnline.Current.TryHueRamp(hue & 0x3FFF, out var ramp) && ramp != null && ramp.Length > 31)
            c = (ushort)(ramp[31] | 0x8000);
        else if (hue == 0) c = 0xFFFF;
        else {
            var g = hue & 31;
            c = (ushort)(0x8000 | (g << 10) | (g << 5) | g);
        }
        return new[] { (byte)c, (byte)(c >> 8) };
    }
}
