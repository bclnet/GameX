using GameX.Origin.UO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenX;
using OpenX.Client;
using System;
using System.Collections.Generic;

namespace GameX.Origin.Mg.UO;

/// <summary>
/// Uploads UO gump pixels to Texture2D and draws them with SpriteBatch. Lives here because
/// GameX.Origin and GameX.Origin.UO are netstandard2.1 and do not reference MonoGame. Not an OpenX type.
/// </summary>
public sealed class UoGumpArt : IGumpArt {
    readonly GraphicsDevice _device;
    readonly SpriteBatch _batch;
    readonly Dictionary<ushort, Texture2D> _cache = [];
    readonly Dictionary<int, Texture2D> _raw = [];
    readonly Dictionary<(ushort graphic, ushort hue), Texture2D> _tinted = [];
    readonly Dictionary<(byte font, char ch, ushort hue), Texture2D> _ascii = [];
    Texture2D _glyphs;

    public UoGumpArt(GraphicsDevice device) {
        _device = device;
        _batch = new SpriteBatch(device);
    }

    public static void Attach(IClientHost host) {
        if (host is not MgClientHost mg) return;
        GumpArt.Sink = new UoGumpArt(mg.GraphicManager.GraphicsDevice);
        UoMouse.Attach(mg);
        UoKeys.Attach(mg);
    }

    public void Begin() {
        UoMusic.Attach();
        _batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
    }

    public void End() => _batch.End();

    /// <summary>
    /// hue 0 uses the graphic cache. A nonzero hue with a hues.mul ramp is keyed by graphic and hue.
    /// When the ramp is missing, RGB is scaled by hue & 31.
    /// </summary>
    public void Draw(ushort graphic, int x, int y, int destWidth, int destHeight, byte[] pixels1555, int srcWidth, int srcHeight, bool tile, ushort hue = 0) {
        if (destWidth <= 0 || destHeight <= 0) return;
        Texture2D tex;
        var color = Color.White;
        if (hue != 0 && _tinted.TryGetValue((graphic, hue), out tex)) { }
        else if (hue != 0 && TryUploadTinted(graphic, hue, pixels1555, srcWidth, srcHeight, out tex)) { }
        else {
            if (!_cache.TryGetValue(graphic, out tex)) {
                tex = Upload(pixels1555, srcWidth, srcHeight);
                if (tex == null) return;
                _cache[graphic] = tex;
            }
            color = HueColor(hue);
        }
        if (tile) DrawTiled(tex, x, y, destWidth, destHeight, color);
        else _batch.Draw(tex, new Rectangle(x, y, destWidth, destHeight), color);
    }

    /// <summary>Land and static art. A separate cache so a gump id is not reused as a tile.</summary>
    public void DrawRaw(int key, int x, int y, int destWidth, int destHeight, byte[] pixels1555, int srcWidth, int srcHeight) {
        if (destWidth <= 0 || destHeight <= 0) return;
        if (!_raw.TryGetValue(key, out var tex)) {
            tex = Upload(pixels1555, srcWidth, srcHeight);
            if (tex == null) return;
            _raw[key] = tex;
        }
        _batch.Draw(tex, new Rectangle(x, y, destWidth, destHeight), Color.White);
    }

    /// <summary>
    /// fonts.mul glyphs when loaded, else ASCII 32..126 as a 5x7 bitmap advancing 6.
    /// A missing glyph uses the bitmap. hue 0 is the glyph pixels; a ramp remaps like Draw.
    /// </summary>
    public void DrawText(int x, int y, string text, ushort hue, byte font = 1) {
        if (string.IsNullOrEmpty(text)) return;
        var uo = UltimaOnline.Current;
        if (uo == null) {
            DrawBitmapText(x, y, text, HueColor(hue));
            return;
        }
        ushort[] ramp = null;
        var useRamp = hue != 0 && uo.TryHueRamp(hue & 0x3FFF, out ramp);
        var color = useRamp ? Color.White : HueColor(hue);
        var dx = x;
        foreach (var ch in text) {
            if (!uo.TryAsciiGlyph(font, ch, out var gw, out var gh, out var px)) {
                DrawBitmapChar(dx, y, ch, HueColor(hue));
                dx += 6;
                continue;
            }
            if (px != null && gw > 0 && gh > 0) {
                var keyHue = useRamp ? hue : (ushort)0;
                if (!_ascii.TryGetValue((font, ch, keyHue), out var tex)) {
                    var src = useRamp ? RemapHue(px, gw, gh, ramp, (hue & 0x8000) != 0) : px;
                    tex = Upload(src, gw, gh);
                    if (tex != null) _ascii[(font, ch, keyHue)] = tex;
                }
                if (tex != null) _batch.Draw(tex, new Rectangle(dx, y, gw, gh), color);
            }
            dx += gw;
        }
    }

    public int MeasureText(string text, byte font = 1) {
        if (string.IsNullOrEmpty(text)) return 0;
        var uo = UltimaOnline.Current;
        if (uo == null) return text.Length * 6;
        var w = 0;
        foreach (var ch in text) {
            if (uo.TryAsciiGlyph(font, ch, out var gw, out _, out _)) w += gw;
            else w += 6;
        }
        return w;
    }

    Texture2D Upload(byte[] pixels1555, int srcWidth, int srcHeight) {
        if (pixels1555 == null || srcWidth <= 0 || srcHeight <= 0) return null;
        var count = srcWidth * srcHeight;
        if (pixels1555.Length < count * 2) return null;
        var colors = new Color[count];
        for (var i = 0; i < count; i++) {
            var c = (ushort)(pixels1555[i * 2] | (pixels1555[i * 2 + 1] << 8));
            colors[i] = new Color(
                ((c >> 10) & 31) * 255 / 31,
                ((c >> 5) & 31) * 255 / 31,
                (c & 31) * 255 / 31,
                (c & 0x8000) != 0 ? 255 : 0);
        }
        var tex = new Texture2D(_device, srcWidth, srcHeight, false, SurfaceFormat.Color);
        tex.SetData(colors);
        return tex;
    }

    bool TryUploadTinted(ushort graphic, ushort hue, byte[] pixels1555, int srcWidth, int srcHeight, out Texture2D tex) {
        tex = null;
        var uo = UltimaOnline.Current;
        if (uo == null || pixels1555 == null || srcWidth <= 0 || srcHeight <= 0 || pixels1555.Length < srcWidth * srcHeight * 2 || !uo.TryHueRamp(hue & 0x3FFF, out var ramp)) return false;
        tex = Upload(RemapHue(pixels1555, srcWidth, srcHeight, ramp, (hue & 0x8000) != 0), srcWidth, srcHeight);
        if (tex == null) return false;
        _tinted[(graphic, hue)] = tex;
        return true;
    }

    static byte[] RemapHue(byte[] pixels1555, int width, int height, ushort[] ramp, bool partial) {
        var count = width * height;
        var dest = new byte[count * 2];
        for (var i = 0; i < count; i++) {
            var c = (ushort)(pixels1555[i * 2] | (pixels1555[i * 2 + 1] << 8));
            if (c != 0) {
                var r = (c >> 10) & 31;
                var g = (c >> 5) & 31;
                var b = c & 31;
                if (!partial || r == g && g == b) {
                    c = ramp[Math.Max(r, Math.Max(g, b))];
                    if (c != 0) c |= 0x8000;
                }
            }
            dest[i * 2] = (byte)c;
            dest[i * 2 + 1] = (byte)(c >> 8);
        }
        return dest;
    }

    static Color HueColor(ushort hue) {
        if (hue == 0) return Color.White;
        var scale = (hue & 31) * 255 / 31;
        return new Color(scale, scale, scale, 255);
    }

    void DrawBitmapText(int x, int y, string text, Color color) {
        EnsureGlyphs();
        var dx = x;
        foreach (var ch in text) {
            DrawBitmapChar(dx, y, ch, color);
            dx += 6;
        }
    }

    void DrawBitmapChar(int x, int y, char ch, Color color) {
        EnsureGlyphs();
        var index = ch - 32;
        if ((uint)index < 95 && ch != ' ')
            _batch.Draw(_glyphs, new Rectangle(x, y, 5, 7), new Rectangle(index * 5, 0, 5, 7), color);
    }

    void DrawTiled(Texture2D tex, int x, int y, int destWidth, int destHeight, Color color) {
        var srcW = tex.Width;
        var srcH = tex.Height;
        if (srcW <= 0 || srcH <= 0) return;
        var destRight = x + destWidth;
        var destBottom = y + destHeight;
        for (var py = y; py < destBottom; py += srcH) {
            var th = Math.Min(srcH, destBottom - py);
            for (var px = x; px < destRight; px += srcW) {
                var tw = Math.Min(srcW, destRight - px);
                _batch.Draw(tex, new Rectangle(px, py, tw, th), new Rectangle(0, 0, tw, th), color);
            }
        }
    }

    // Column bitmasks, bit 0 at the top. Bit 7 is a descender; those glyphs shift up into the 7 rows.
    static readonly byte[] GlyphBits = [
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5F, 0x00, 0x00, 0x00, 0x07, 0x00, 0x07, 0x00,
        0x14, 0x7F, 0x14, 0x7F, 0x14, 0x24, 0x2A, 0x7F, 0x2A, 0x12, 0x23, 0x13, 0x08, 0x64, 0x62,
        0x36, 0x49, 0x56, 0x20, 0x50, 0x00, 0x08, 0x07, 0x03, 0x00, 0x00, 0x1C, 0x22, 0x41, 0x00,
        0x00, 0x41, 0x22, 0x1C, 0x00, 0x2A, 0x1C, 0x7F, 0x1C, 0x2A, 0x08, 0x08, 0x3E, 0x08, 0x08,
        0x00, 0x80, 0x70, 0x30, 0x00, 0x08, 0x08, 0x08, 0x08, 0x08, 0x00, 0x00, 0x60, 0x60, 0x00,
        0x20, 0x10, 0x08, 0x04, 0x02, 0x3E, 0x51, 0x49, 0x45, 0x3E, 0x00, 0x42, 0x7F, 0x40, 0x00,
        0x72, 0x49, 0x49, 0x49, 0x46, 0x21, 0x41, 0x49, 0x4D, 0x33, 0x18, 0x14, 0x12, 0x7F, 0x10,
        0x27, 0x45, 0x45, 0x45, 0x39, 0x3C, 0x4A, 0x49, 0x49, 0x31, 0x41, 0x21, 0x11, 0x09, 0x07,
        0x36, 0x49, 0x49, 0x49, 0x36, 0x46, 0x49, 0x49, 0x29, 0x1E, 0x00, 0x00, 0x14, 0x00, 0x00,
        0x00, 0x40, 0x34, 0x00, 0x00, 0x00, 0x08, 0x14, 0x22, 0x41, 0x14, 0x14, 0x14, 0x14, 0x14,
        0x00, 0x41, 0x22, 0x14, 0x08, 0x02, 0x01, 0x59, 0x09, 0x06, 0x3E, 0x41, 0x5D, 0x59, 0x4E,
        0x7C, 0x12, 0x11, 0x12, 0x7C, 0x7F, 0x49, 0x49, 0x49, 0x36, 0x3E, 0x41, 0x41, 0x41, 0x22,
        0x7F, 0x41, 0x41, 0x41, 0x3E, 0x7F, 0x49, 0x49, 0x49, 0x41, 0x7F, 0x09, 0x09, 0x09, 0x01,
        0x3E, 0x41, 0x41, 0x51, 0x73, 0x7F, 0x08, 0x08, 0x08, 0x7F, 0x00, 0x41, 0x7F, 0x41, 0x00,
        0x20, 0x40, 0x41, 0x3F, 0x01, 0x7F, 0x08, 0x14, 0x22, 0x41, 0x7F, 0x40, 0x40, 0x40, 0x40,
        0x7F, 0x02, 0x1C, 0x02, 0x7F, 0x7F, 0x04, 0x08, 0x10, 0x7F, 0x3E, 0x41, 0x41, 0x41, 0x3E,
        0x7F, 0x09, 0x09, 0x09, 0x06, 0x3E, 0x41, 0x51, 0x21, 0x5E, 0x7F, 0x09, 0x19, 0x29, 0x46,
        0x26, 0x49, 0x49, 0x49, 0x32, 0x03, 0x01, 0x7F, 0x01, 0x03, 0x3F, 0x40, 0x40, 0x40, 0x3F,
        0x1F, 0x20, 0x40, 0x20, 0x1F, 0x3F, 0x40, 0x38, 0x40, 0x3F, 0x63, 0x14, 0x08, 0x14, 0x63,
        0x03, 0x04, 0x78, 0x04, 0x03, 0x61, 0x59, 0x49, 0x4D, 0x43, 0x00, 0x7F, 0x41, 0x41, 0x41,
        0x02, 0x04, 0x08, 0x10, 0x20, 0x00, 0x41, 0x41, 0x41, 0x7F, 0x04, 0x02, 0x01, 0x02, 0x04,
        0x40, 0x40, 0x40, 0x40, 0x40, 0x00, 0x03, 0x07, 0x08, 0x00, 0x20, 0x54, 0x54, 0x78, 0x40,
        0x7F, 0x28, 0x44, 0x44, 0x38, 0x38, 0x44, 0x44, 0x44, 0x28, 0x38, 0x44, 0x44, 0x28, 0x7F,
        0x38, 0x54, 0x54, 0x54, 0x18, 0x00, 0x08, 0x7E, 0x09, 0x02, 0x18, 0xA4, 0xA4, 0x9C, 0x78,
        0x7F, 0x08, 0x04, 0x04, 0x78, 0x00, 0x44, 0x7D, 0x40, 0x00, 0x20, 0x40, 0x40, 0x3D, 0x00,
        0x7F, 0x10, 0x28, 0x44, 0x00, 0x00, 0x41, 0x7F, 0x40, 0x00, 0x7C, 0x04, 0x78, 0x04, 0x78,
        0x7C, 0x08, 0x04, 0x04, 0x78, 0x38, 0x44, 0x44, 0x44, 0x38, 0xFC, 0x18, 0x24, 0x24, 0x18,
        0x18, 0x24, 0x24, 0x18, 0xFC, 0x7C, 0x08, 0x04, 0x04, 0x08, 0x48, 0x54, 0x54, 0x54, 0x24,
        0x04, 0x04, 0x3F, 0x44, 0x24, 0x3C, 0x40, 0x40, 0x20, 0x7C, 0x1C, 0x20, 0x40, 0x20, 0x1C,
        0x3C, 0x40, 0x30, 0x40, 0x3C, 0x44, 0x28, 0x10, 0x28, 0x44, 0x4C, 0x90, 0x90, 0x90, 0x7C,
        0x44, 0x64, 0x54, 0x4C, 0x44, 0x00, 0x08, 0x36, 0x41, 0x00, 0x00, 0x00, 0x77, 0x00, 0x00,
        0x00, 0x41, 0x36, 0x08, 0x00, 0x02, 0x01, 0x02, 0x04, 0x02,
    ];

    void EnsureGlyphs() {
        if (_glyphs != null) return;
        const int count = 95, gw = 5, gh = 7;
        var width = count * gw;
        var pixels = new Color[width * gh];
        for (var i = 0; i < count; i++) {
            var shift = 0;
            for (var col = 0; col < gw; col++)
                if ((GlyphBits[i * gw + col] & 0x80) != 0) { shift = 1; break; }
            for (var col = 0; col < gw; col++) {
                var bits = GlyphBits[i * gw + col] >> shift;
                for (var row = 0; row < gh; row++) {
                    if ((bits & (1 << row)) == 0) continue;
                    pixels[row * width + i * gw + col] = Color.White;
                }
            }
        }
        _glyphs = new Texture2D(_device, width, gh, false, SurfaceFormat.Color);
        _glyphs.SetData(pixels);
    }
}