// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ButtonTileArt. The button is the gump. The tile is art plus 0x4000.
/// Partial hue is tiledata, or the 0x8000 bit. A missing texture disposes the control.
/// </summary>
public class ButtonTileArt : Button {
    static Binary_TileData.ItemData[] _tiles;
    static bool _tilesMiss;
    static int _slot = 1;
    readonly ushort _hue;
    readonly bool _isPartial;
    readonly int _tileX, _tileY;
    ushort _graphic;
    byte[] _px;
    int _artW, _artH, _key;

    public ButtonTileArt(List<string> gparams) : base(
        gparams.Count >= 8 ? int.Parse(gparams[7], CultureInfo.InvariantCulture) : 0,
        ParseU16(gparams[3]),
        ParseU16(gparams[4])
    ) {
        X = int.Parse(gparams[1], CultureInfo.InvariantCulture);
        Y = int.Parse(gparams[2], CultureInfo.InvariantCulture);
        _graphic = ParseU16(gparams[8]);
        _hue = ParseU16(gparams[9]);
        _tileX = int.Parse(gparams[10], CultureInfo.InvariantCulture);
        _tileY = int.Parse(gparams[11], CultureInfo.InvariantCulture);
        _isPartial = Partial(_graphic);
        _px = null;
        _artW = _artH = _key = 0;
        ContainsByBounds = true;
        IsFromServer = true;
        WantUpdateSize = false;
        ButtonAction = ButtonAction.SwitchPage;
        IsPageButton = true;
        if (gparams.Count >= 6 && int.Parse(gparams[5], CultureInfo.InvariantCulture) != 0) {
            ButtonAction = ButtonAction.Activate;
            IsPageButton = false;
        }
        if (gparams.Count >= 7) ToPage = int.Parse(gparams[6], CultureInfo.InvariantCulture);
        if (IsDisposed) return;
        if (!Load()) {
            _px = null;
            Dispose();
        }
    }

    public bool ContainsByBounds { get; set; }

    /// <summary>ClassicUO Control.IsFromServer. This tree keeps that flag on Gump, so the value stays here.</summary>
    public bool IsFromServer { get; set; }

    public override bool Draw(int x, int y) {
        if (!base.Draw(x, y)) return false;
        if (_px != null && _artW > 0 && _artH > 0)
            GumpArt.Sink?.DrawRaw(_key, x + _tileX, y + _tileY, _artW, _artH, _px, _artW, _artH);
        return true;
    }

    bool Load() {
        var assets = UltimaOnline.Current?.Assets;
        var file = _graphic + 0x4000;
        if (assets == null || file > 0xFFFF || !assets.TryArtPixels(file, out var src, out _artW, out _artH)) return false;
        if (src == null || _artW <= 0 || _artH <= 0 || src.Length < _artW * _artH * 2) return false;
        var partial = _isPartial;
        var hue = (int)_hue;
        if ((hue & 0x8000) != 0) {
            partial = true;
            hue &= 0x7FFF;
        }
        if (hue == 0) partial = false;
        if (hue != 0 && (hue & 0x4000) == 0) {
            hue--;
            if (hue != 0 && assets.TryHueRamp(hue, out var ramp) && ramp != null && ramp.Length > 31) {
                _px = Tint(src, _artW, _artH, ramp, partial);
                _key = 0x5E400000 + _slot++;
                return _px != null;
            }
        }
        _px = src;
        _key = 0x5E400000 + _slot++;
        return true;
    }

    static byte[] Tint(byte[] src, int width, int height, ushort[] ramp, bool partial) {
        var count = width * height;
        var dest = new byte[count * 2];
        for (var i = 0; i < count; i++) {
            var c = (ushort)(src[i * 2] | (src[i * 2 + 1] << 8));
            if (c != 0) {
                var r = (c >> 10) & 31;
                var g = (c >> 5) & 31;
                var b = c & 31;
                if (!partial || (r == g && g == b)) {
                    c = ramp[Math.Max(r, Math.Max(g, b))];
                    if (c != 0) c |= 0x8000;
                }
            }
            dest[i * 2] = (byte)c;
            dest[i * 2 + 1] = (byte)(c >> 8);
        }
        return dest;
    }

    static bool Partial(ushort graphic) {
        var items = Tiles();
        return items != null && graphic < items.Length && items[graphic].IsPartialHue;
    }

    static Binary_TileData.ItemData[] Tiles() {
        if (_tiles != null || _tilesMiss) return _tiles;
        var root = UltimaOnline.Current?.ClientPath;
        if (string.IsNullOrEmpty(root)) return null;
        _tilesMiss = true;
        try {
            var path = Path.Combine(root, "tiledata.mul");
            if (!File.Exists(path)) return null;
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var r = new BinaryReader(fs);
            _tiles = new Binary_TileData(r).ItemDatas;
        }
        catch { _tiles = null; }
        return _tiles;
    }

    static ushort ParseU16(string s) {
        if (string.IsNullOrEmpty(s)) return 0;
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
            uint.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex);
            return (ushort)hex;
        }
        if (s.Length > 1 && s[0] == '-' && short.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var neg))
            return (ushort)neg;
        uint.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v);
        return (ushort)v;
    }
}
