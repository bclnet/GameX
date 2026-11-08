// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>ClassicUO GumpPicWithWidth. The graphic is tiled to Percent wide, not the full gump.</summary>
public class GumpPicWithWidth : GumpPic {
    public GumpPicWithWidth(int x, int y, ushort graphic, ushort hue, int perc) : base(x, y, graphic, hue) {
        Percent = perc;
        CanMove = true;
    }

    public int Percent { get; set; }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (Percent > 0 && Height > 0)
            GumpArtDraw.Pic(Graphic, x, y, Percent, Height, true, Hue);
        for (var i = 0; i < Children.Count; i++) {
            var c = Children[i];
            if ((c.Page == 0 || c.Page == ActivePage) && c.IsVisible)
                c.Draw(c.X + x, c.Y + y);
        }
        return true;
    }
}
