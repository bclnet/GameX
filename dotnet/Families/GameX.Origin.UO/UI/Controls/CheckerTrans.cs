// SPDX-License-Identifier: BSD-2-Clause

using System.Collections.Generic;
using System.Globalization;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO CheckerTrans. The stencil and the blend are not used.
/// The fill is black at 0.5. BGRA1555 has no partial alpha, so that fill is an opaque dark pixel.
/// </summary>
public class CheckerTrans : Control {
    // r = g = b = 4, opaque. Same stand-in InfoBar uses for a translucent black.
    static readonly byte[] Dark = { 0x84, 0x90 };

    public CheckerTrans(List<string> parts) {
        X = int.Parse(parts[1], CultureInfo.InvariantCulture);
        Y = int.Parse(parts[2], CultureInfo.InvariantCulture);
        Width = int.Parse(parts[3], CultureInfo.InvariantCulture);
        Height = int.Parse(parts[4], CultureInfo.InvariantCulture);
        AcceptMouseInput = false;
        IsFromServer = true;
    }

    /// <summary>ClassicUO Control.IsFromServer. This tree keeps that flag on Gump, so the value stays here.</summary>
    public bool IsFromServer { get; set; }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (Width > 0 && Height > 0)
            GumpArt.Sink?.DrawRaw(0x57200002, x, y, Width, Height, Dark, 1, 1);
        return true;
    }
}
