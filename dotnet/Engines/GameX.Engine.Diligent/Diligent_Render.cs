using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.Diligent;

namespace GameX.Engines.Diligent;

/// <summary>
/// DiligentRenderer
/// </summary>
public static class DiligentRenderer {
    /// <summary>
    /// False on this build. Diligent Engine is not linked.
    /// </summary>
    public static bool DiligentLinked() => DiligentShim.Linked;

    /// <summary>
    /// This build returns "not linked".
    /// </summary>
    public static string DiligentVersion() => DiligentShim.Version;

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
    static ViewInfo() => EngineX.Activate(DiligentEngine.This);
}
