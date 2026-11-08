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

/// <summary>ClassicUO GumpPic. Draw hands the graphic to GumpArt.Sink.</summary>
public class GumpPic : GumpPicBase {
    public GumpPic(int x, int y, ushort graphic, ushort hue) {
        X = x;
        Y = y;
        Graphic = graphic;
        Hue = hue;
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        GumpArtDraw.Pic(Graphic, x, y, Width, Height, false, Hue);
        return base.Draw(x, y);
    }
}
