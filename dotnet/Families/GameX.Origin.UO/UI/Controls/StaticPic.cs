// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace GameX.Origin.UO;

/// <summary>ClassicUO StaticPic. The art file is the graphic plus 0x4000. A missing art disposes it.</summary>
public class StaticPic : Control {
    static Binary_TileData.ItemData[] _items;
    static bool _tilesLoaded;

    ushort _graphic;
    byte[] _shown;
    int _shownKey = -1;

    public StaticPic(ushort graphic, ushort hue) {
        Hue = hue;
        Graphic = graphic;
        CanMove = true;
        WantUpdateSize = false;
    }

    public StaticPic(List<string> parts) : this(ParseUShort(parts[3]), parts.Count > 4 ? ParseUShort(parts[4]) : (ushort)0) {
        X = int.Parse(parts[1]);
        Y = int.Parse(parts[2]);
        IsFromServer = true;
    }

    public ushort Hue { get; set; }
    public bool IsPartialHue { get; set; }
    public bool IsFromServer { get; set; }

    public ushort Graphic {
        get => _graphic;
        set {
            _graphic = value;
            _shown = null;
            _shownKey = -1;
            var assets = UltimaOnline.Current?.Assets;
            if (assets == null) return;
            if (!assets.TryArtPixels(ArtFile(value), out _, out var w, out var h)) {
                Dispose();
                return;
            }
            Width = w;
            Height = h;
            IsPartialHue = Partial(value);
        }
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        var assets = UltimaOnline.Current?.Assets;
        if (assets != null && assets.TryArtPixels(ArtFile(Graphic), out var src, out var w, out var h) && src != null) {
            var partial = IsPartialHue || (Hue & 0x8000) != 0;
            var key = ArtKey(Graphic, Hue, partial);
            if (_shown == null || _shownKey != key) {
                _shown = Tint(src, w, h, Hue, partial);
                _shownKey = key;
            }
            var dw = Width > 0 ? Width : w;
            var dh = Height > 0 ? Height : h;
            GumpArt.Sink?.DrawRaw(key, x, y, dw, dh, _shown, w, h);
        }
        return base.Draw(x, y);
    }

    public bool Contains(int x, int y) {
        var assets = UltimaOnline.Current?.Assets;
        if (assets == null || !assets.TryArtPixels(ArtFile(Graphic), out var px, out var w, out var h) || px == null) return false;
        if ((uint)x >= (uint)w || (uint)y >= (uint)h) return false;
        var i = (y * w + x) * 2;
        if (i + 1 >= px.Length) return false;
        return (px[i + 1] & 0x80) != 0;
    }

    static int ArtFile(ushort graphic) => (graphic & 0x7FFF) + 0x4000;

    static int ArtKey(ushort graphic, ushort hue, bool partial) {
        uint k = 0x80000000u | graphic | ((uint)(hue & 0x3FFF) << 16);
        if (partial) k |= 0x40000000u;
        return unchecked((int)k);
    }

    static byte[] Tint(byte[] src, int w, int h, ushort hue, bool partial) {
        if (hue == 0 || src == null) return src;
        var count = w * h;
        if (w <= 0 || h <= 0 || src.Length < count * 2) return src;
        var uo = UltimaOnline.Current;
        ushort[] ramp = null;
        var useRamp = uo != null && uo.TryHueRamp(hue & 0x3FFF, out ramp) && ramp != null && ramp.Length >= 32;
        var scale = hue & 31;
        var dest = new byte[count * 2];
        for (var i = 0; i < count; i++) {
            var c = (ushort)(src[i * 2] | (src[i * 2 + 1] << 8));
            if (c != 0) {
                var r = (c >> 10) & 31;
                var g = (c >> 5) & 31;
                var b = c & 31;
                if (useRamp) {
                    if (!partial || (r == g && g == b)) {
                        var idx = r > g ? r : g;
                        if (b > idx) idx = b;
                        c = ramp[idx];
                        if (c != 0) c |= 0x8000;
                    }
                }
                else if (scale != 0 && (!partial || (r == g && g == b))) {
                    r = r * scale / 31;
                    g = g * scale / 31;
                    b = b * scale / 31;
                    c = (ushort)(((c & 0x8000) != 0 ? 0x8000 : 0) | (r << 10) | (g << 5) | b);
                }
            }
            dest[i * 2] = (byte)c;
            dest[i * 2 + 1] = (byte)(c >> 8);
        }
        return dest;
    }

    static bool Partial(ushort graphic) {
        var items = Items();
        return items != null && graphic < items.Length && items[graphic].IsPartialHue;
    }

    static Binary_TileData.ItemData[] Items() {
        if (_tilesLoaded) return _items;
        var root = UltimaOnline.Current?.ClientPath;
        if (string.IsNullOrEmpty(root)) return null;
        _tilesLoaded = true;
        var path = Path.Combine(root, "tiledata.mul");
        if (!File.Exists(path)) return _items = null;
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);
        return _items = new Binary_TileData(br).ItemDatas;
    }

    static ushort ParseUShort(string str) {
        if (string.IsNullOrEmpty(str)) return 0;
        if (str.StartsWith("0x")) {
            uint.TryParse(str.Substring(2), NumberStyles.HexNumber, null, out var hex);
            return (ushort)hex;
        }
        if (str.Length > 1 && str[0] == '-' && short.TryParse(str, out var neg)) return (ushort)neg;
        uint.TryParse(str, out var v);
        return (ushort)v;
    }
}
