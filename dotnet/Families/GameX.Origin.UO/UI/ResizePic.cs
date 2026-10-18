// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ResizePic. Pieces is how many of the nine graphics have an index size.
/// The nine-slice is not drawn.
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
}
