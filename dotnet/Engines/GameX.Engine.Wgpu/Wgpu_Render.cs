using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.Wgpu;

namespace GameX.Engines.Wgpu;

/// <summary>
/// WgpuRenderer
/// </summary>
public static class WgpuRenderer {
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
    static ViewInfo() => EngineX.Activate(WgpuEngine.This);
}
