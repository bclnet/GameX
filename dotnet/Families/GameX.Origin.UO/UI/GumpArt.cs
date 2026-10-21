// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// Host painter for one gump graphic. pixels1555 is little-endian BGRA1555
/// (bit 15 = opaque), row-major, srcWidth * srcHeight * 2 bytes.
/// graphic is the texture cache key. When tile is false, draw the texture in
/// the dest rectangle. When tile is true, repeat the source across dest.
/// hue 0 is unhued. A nonzero hue with a hues.mul ramp is a cache key.
/// Otherwise RGB is scaled by hue & 31. A null or empty buffer draws nothing.
/// The host may ignore the buffer when graphic is already uploaded.
/// This type is UO-only; OpenX does not own it.
/// </summary>
public interface IGumpArt {
    void Begin();
    void End();
    void Draw(ushort graphic, int x, int y, int destWidth, int destHeight, byte[] pixels1555, int srcWidth, int srcHeight, bool tile, ushort hue = 0);

    /// <summary>
    /// Same pixels as Draw. key is not a gump id, so the host must not reuse the gump cache.
    /// The default body keeps the low 16 bits and calls Draw.
    /// </summary>
    void DrawRaw(int key, int x, int y, int destWidth, int destHeight, byte[] pixels1555, int srcWidth, int srcHeight) {
        Draw((ushort)(key & 0xFFFF), x, y, destWidth, destHeight, pixels1555, srcWidth, srcHeight, false, 0);
    }

    /// <summary>One line of text. font is fonts.mul 0..9. The default body draws nothing.</summary>
    void DrawText(int x, int y, string text, ushort hue, byte font = 1) { }

    /// <summary>Pixel width of one line. Null or empty is 0. The default body is length * 6.</summary>
    int MeasureText(string text, byte font = 1) => string.IsNullOrEmpty(text) ? 0 : text.Length * 6;
}

/// <summary>
/// Set by GameX.Origin.Mg.UO.UoGumpArt.Attach. Null means the scene only walks the tree.
/// </summary>
public static class GumpArt {
    public static IGumpArt Sink { get; set; }
}