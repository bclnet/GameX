// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// Host painter for one gump graphic. pixels1555 is little-endian BGRA1555
/// (bit 15 = opaque), row-major, srcWidth * srcHeight * 2 bytes.
/// graphic is the texture cache key. When tile is false, draw the texture in
/// the dest rectangle. When tile is true, repeat the source across dest.
/// hue 0 is white. Any other hue is a SpriteBatch tint, not a cache key.
/// A null or empty buffer draws nothing. The host may ignore the buffer when
/// graphic is already uploaded. This type is UO-only; OpenX does not own it.
/// </summary>
public interface IGumpArt {
    void Begin();
    void End();
    void Draw(ushort graphic, int x, int y, int destWidth, int destHeight, byte[] pixels1555, int srcWidth, int srcHeight, bool tile, ushort hue = 0);

    /// <summary>One line of text. The default body draws nothing.</summary>
    void DrawText(int x, int y, string text, ushort hue) { }
}

/// <summary>
/// Set by GameX.Engine.Mg.UoGumpArt.Attach. Null means the scene only walks the tree.
/// </summary>
public static class GumpArt {
    public static IGumpArt Sink { get; set; }
}