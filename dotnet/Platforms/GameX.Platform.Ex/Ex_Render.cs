using OpenStack.Gfx;
using OpenStack.Gfx.Ex;
#pragma warning disable CS9113

namespace GameX.Platforms.Ex;

public static class ExRenderer {
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, object obj, string type) {
        if (obj is IHaveOpenGfx z) gfx = z.Gfx;
        return type switch {
            "TestTri" => new TestTriRenderer(gfx, obj),
            "Texture" => new SpriteRenderer(gfx, obj),
            //"Object" => new ObjectRenderer(gfx, obj),
            _ => default
        };
    }
}
