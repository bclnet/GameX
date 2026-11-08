// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO AlphaBlendControl. BGRA1555 has no partial alpha, so the fill is an opaque shade.
/// Alpha 0.7 is gray 4. A hue reads hues.mul at that shade.
/// </summary>
public sealed class AlphaBlendControl : Control {
    public AlphaBlendControl(float alpha = 0.5f) {
        Alpha = alpha;
        AcceptMouseInput = false;
    }

    public ushort Hue { get; set; }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (Alpha > 0 && Width > 0 && Height > 0) {
            var shade = (int)((1f - Alpha) * 31f + 0.5f) / 2;
            if (shade < 0) shade = 0;
            if (shade > 31) shade = 31;
            var key = unchecked((int)(0x5F000000u + ((uint)Hue << 8) + (uint)shade));
            GumpArt.Sink?.DrawRaw(key, x, y, Width, Height, Solid(Hue, shade), 1, 1);
        }
        return base.Draw(x, y);
    }

    static byte[] Solid(ushort hue, int index) {
        ushort c;
        if (hue != 0 && UltimaOnline.Current != null && UltimaOnline.Current.TryHueRamp(hue & 0x3FFF, out var ramp) && ramp != null && (uint)index < (uint)ramp.Length)
            c = (ushort)(ramp[index] | 0x8000);
        else {
            var g = hue == 0 ? index : index * (hue & 31) / 31;
            c = (ushort)(0x8000 | (g << 10) | (g << 5) | g);
        }
        return new[] { (byte)c, (byte)(c >> 8) };
    }
}
