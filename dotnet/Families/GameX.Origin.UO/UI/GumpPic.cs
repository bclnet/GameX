// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>ClassicUO GumpPicBase. Size comes from the gump index, not a texture.</summary>
public abstract class GumpPicBase : Control {
    ushort _graphic;

    protected GumpPicBase() {
        CanMove = true;
        AcceptMouseInput = true;
    }

    public ushort Graphic {
        get => _graphic;
        set {
            _graphic = value;
            if (!GumpMetrics.TrySize(_graphic, out var w, out var h)) {
                Dispose();
                return;
            }
            Width = w;
            Height = h;
        }
    }

    public ushort Hue { get; set; }
    public bool IsPartialHue { get; set; }
}

/// <summary>ClassicUO GumpPic. Draw does not upload the graphic.</summary>
public class GumpPic : GumpPicBase {
    public GumpPic(int x, int y, ushort graphic, ushort hue) {
        X = x;
        Y = y;
        Graphic = graphic;
        Hue = hue;
    }
}

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

    public ushort Hue { get; set; }
}
