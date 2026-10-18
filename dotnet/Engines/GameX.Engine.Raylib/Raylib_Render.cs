using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.Raylib;

namespace GameX.Engines.Raylib;

/// <summary>
/// RaylibRenderer
/// </summary>
public static class RaylibRenderer {
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, ISource source, object obj, string type) {
        if (obj is IHaveSource z) source = z.Source;
        return type switch {
            "TestTri" => new TestTriRenderer(gfx, source, obj),
            "Texture" => new TextureRenderer(gfx, source, obj, 0..),
            _ => default,
        };
    }
}

/// <summary>
/// ViewInfo
/// </summary>
public class ViewInfo {
    static ViewInfo() => EngineX.Activate(RaylibEngine.This);
}
