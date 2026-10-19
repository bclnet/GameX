// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// Hands decoded gump pixels to the host painter. This project does not upload Texture2D.
/// </summary>
internal static class GumpArtDraw {
    public static void Pic(ushort graphic, int x, int y, int destW, int destH, bool tile, ushort hue = 0) {
        var sink = GumpArt.Sink;
        var assets = UltimaOnline.Current?.Assets;
        if (sink == null || assets == null) return;
        if (graphic == 0xFFFF) return;
        if (!assets.TryGumpPixels(graphic, out var px, out var sw, out var sh)) return;
        if (destW <= 0) destW = sw;
        if (destH <= 0) destH = sh;
        sink.Draw(graphic, x, y, destW, destH, px, sw, sh, tile, hue);
    }
}