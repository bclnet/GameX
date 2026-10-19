// SPDX-License-Identifier: BSD-2-Clause

using System;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ResizePic. Pieces is how many of the nine graphics have an index size.
/// Draw paints the nine-slice through GumpArt.Sink.
/// </summary>
public class ResizePic : Control {
    public ResizePic(ushort graphic) {
        CanMove = true;
        CanCloseWithRightClick = true;
        Graphic = graphic;
        for (Pieces = 0; Pieces < 9; Pieces++)
            if (!GumpMetrics.TrySize((ushort)(graphic + Pieces), out _, out _)) break;
    }

    public ushort Graphic { get; }
    public int Pieces { get; }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        Size(0, out var tlW, out var tlH);
        Size(1, out _, out var tH);
        Size(2, out var trW, out var trH);
        Size(3, out var lW, out _);
        Size(5, out var rW, out _);
        Size(6, out var blW, out var blH);
        Size(7, out _, out var bH);
        Size(8, out var brW, out var brH);
        var offsetTop = Math.Max(tlH, trH) - tH;
        var offsetBottom = Math.Max(blH, brH) - bH;
        var offsetLeft = Math.Abs(Math.Max(tlW, blW) - trW);
        var offsetRight = Math.Max(trW, brW) - rW;
        Pic(0, x, y, tlW, tlH, false);
        Pic(1, x + tlW, y, Width - tlW - trW, tH, true);
        Pic(2, x + Width - trW, y + offsetTop, trW, trH, false);
        Pic(3, x, y + tlH, lW, Height - tlH - blH, true);
        Pic(5, x + Width - rW, y + trH, rW, Height - trH - brH, true);
        Pic(6, x, y + Height - blH, blW, blH, false);
        Pic(7, x + blW, y + Height - bH - offsetBottom, Width - blW - brW, bH, true);
        Pic(8, x + Width - brW, y + Height - brH, brW, brH, false);
        Pic(4, x + tlW, y + tlH, Width - tlW - trW + offsetLeft + offsetRight, Height - trH - brH, true);
        return base.Draw(x, y);
    }

    void Size(int n, out int w, out int h) {
        w = h = 0;
        if (n >= Pieces) return;
        GumpMetrics.TrySize((ushort)(Graphic + n), out w, out h);
    }

    void Pic(int n, int x, int y, int dw, int dh, bool tile) {
        if (n >= Pieces || dw < 1 || dh < 1) return;
        GumpArtDraw.Pic((ushort)(Graphic + n), x, y, dw, dh, tile);
    }
}
