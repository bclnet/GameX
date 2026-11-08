// SPDX-License-Identifier: BSD-2-Clause

using System.Collections.Generic;
using System.Globalization;

namespace GameX.Origin.UO;

/// <summary>ClassicUO GumpPicTiled. The caller sets the tiled width and height.</summary>
public class GumpPicTiled : Control {
    ushort _graphic;

    public GumpPicTiled(ushort graphic) {
        CanMove = true;
        AcceptMouseInput = true;
        Graphic = graphic;
    }

    public GumpPicTiled(int x, int y, int width, int height, ushort graphic) : this(graphic) {
        X = x;
        Y = y;
        if (width > 0) Width = width;
        if (height > 0) Height = height;
    }

    public GumpPicTiled(List<string> parts) : this(ParseUShort(parts[5])) {
        X = int.Parse(parts[1]);
        Y = int.Parse(parts[2]);
        Width = int.Parse(parts[3]);
        Height = int.Parse(parts[4]);
        IsFromServer = true;
    }

    public bool IsFromServer { get; set; }
    public ushort Hue { get; set; }

    public ushort Graphic {
        get => _graphic;
        set {
            if (_graphic == value || value == 0xFFFF) return;
            _graphic = value;
            if (!GumpMetrics.TrySize(_graphic, out var w, out var h)) {
                Dispose();
                return;
            }
            Width = w;
            Height = h;
        }
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        GumpArtDraw.Pic(Graphic, x, y, Width, Height, true, Hue);
        return base.Draw(x, y);
    }

    public bool Contains(int x, int y) {
        var width = Width;
        var height = Height;
        var assets = UltimaOnline.Current?.Assets;
        if (assets == null || !assets.TryGumpPixels(Graphic, out var px, out var gw, out var gh) || px == null || gw <= 0 || gh <= 0) return false;
        if (width == 0) width = gw;
        if (height == 0) height = gh;
        while (x > gw && width > gw) {
            x -= gw;
            width -= gw;
        }
        while (y > gh && height > gh) {
            y -= gh;
            height -= gh;
        }
        if (x > width || y > height || x < 0 || y < 0) return false;
        if (x >= gw || y >= gh) return false;
        var i = (y * gw + x) * 2;
        if (i + 1 >= px.Length) return false;
        return (px[i + 1] & 0x80) != 0;
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
