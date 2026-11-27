using OpenX.Gfx;

namespace GameX.Origin.Clients.UO;

/// <summary>
/// One direction of a body, packed as RGBA8 cells. Frames come from Assets.TryBodyFrame.
/// Duration is 0.08 seconds, the client's 80 ms step.
/// </summary>
public static class BodySpriteSheet {
    public const float FrameDuration = 0.08f;

    /// <summary>
    /// Packs direction's frames in order. A missing group returns false and leaves sheet null.
    /// </summary>
    public static bool TryPack(Assets assets, int body, int action, int direction, out GfxSpriteSheet sheet) {
        sheet = null;
        if (assets == null || !assets.TryFrameCount(body, action, direction, out var count) || count <= 0) return false;
        var cells = new (byte[] rgba, int width, int height, float duration)[count];
        var frames = new int[count];
        for (var i = 0; i < count; i++) {
            if (!assets.TryBodyFrame(body, action, direction, i, out var pixels, out var width, out var height, out _, out _)) return false;
            if (!ToRgba(pixels, width, height, out var rgba)) return false;
            cells[i] = (rgba, width, height, FrameDuration);
            frames[i] = i;
        }
        sheet = GfxSpriteSheet.Pack(cells, [(direction.ToString(), frames)]);
        return true;
    }

    /// <summary>
    /// UoGumpArt.Upload's split. Bits 10, 5, and 0 are R, G, and B. Bit 15 is opaque. Not a hue remap.
    /// </summary>
    static bool ToRgba(byte[] pixels1555, int width, int height, out byte[] rgba) {
        rgba = null;
        if (pixels1555 == null || width <= 0 || height <= 0) return false;
        var count = width * height;
        if (pixels1555.Length < count * 2) return false;
        rgba = new byte[count * 4];
        for (var i = 0; i < count; i++) {
            var c = (ushort)(pixels1555[i * 2] | (pixels1555[i * 2 + 1] << 8));
            var o = i * 4;
            rgba[o] = (byte)(((c >> 10) & 31) * 255 / 31);
            rgba[o + 1] = (byte)(((c >> 5) & 31) * 255 / 31);
            rgba[o + 2] = (byte)((c & 31) * 255 / 31);
            rgba[o + 3] = (c & 0x8000) != 0 ? (byte)255 : (byte)0;
        }
        return true;
    }
}
