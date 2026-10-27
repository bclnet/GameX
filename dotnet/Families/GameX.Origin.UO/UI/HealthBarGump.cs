// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO HealthBarGump hits line. Red is 0x0805 at its own size. Blue is 0x0806,
/// tiled to CalculatePercents(HitsMax, Hits, 109), the same clip as GumpPicWithWidth.
/// A percent of 0 or 1 is not scaled. HitsMax 0 hides the line. No status is skipped.
/// Poison 0x0808 and yellow 0x0809 are not read. Screen position is WorldGump's player anchor.
/// </summary>
public static class HealthBarGump {
    const ushort LineRed = 0x0805;
    const ushort LineBlue = 0x0806;
    const int BarWidth = 109;

    static readonly List<UOMobileState> Mobiles = [];
    static readonly List<UOItemState> Items = [];

    public static void Draw(int originX, int originY) {
        if (GumpArt.Sink == null) return;
        UOLoginNet.CopyActors(Mobiles, Items);
        var px = UOLoginNet.PlayerX;
        var py = UOLoginNet.PlayerY;
        var pz = UOLoginNet.PlayerZ;
        for (var i = 0; i < Mobiles.Count; i++) {
            var m = Mobiles[i];
            if (!UOLoginNet.TryStatus(m.Serial, out var st) || st.HitsMax == 0) continue;
            var dx = m.X - px;
            var dy = m.Y - py;
            var x = originX + 320 + (dx - dy) * 22 - BarWidth / 2;
            var y = originY + 220 + (dx + dy) * 22 - (m.Z - pz) * 4;
            var hits = CalculatePercents(st.HitsMax, st.Hits, BarWidth);
            GumpArtDraw.Pic(LineRed, x, y, 0, 0, false);
            if (hits > 0) GumpArtDraw.Pic(LineBlue, x, y, hits, 0, true);
        }
    }

    static int CalculatePercents(int max, int current, int maxValue) {
        if (max > 0) {
            max = current * 100 / max;
            if (max > 100) max = 100;
            if (max > 1) max = maxValue * max / 100;
        }
        return max;
    }
}
