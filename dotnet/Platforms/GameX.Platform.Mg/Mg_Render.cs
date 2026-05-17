using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Mg;
#pragma warning disable CS9113

namespace GameX.Platforms.Mg;

public static class MgRenderer {
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, ISource source, object obj, string type) {
        if (obj is IHaveSource z) source = z.Source;
        return type switch {
            "TestTri" => new TestTriRenderer(gfx, source, obj),
            "Texture" => new SpriteRenderer(gfx, source, obj),
            //"Object" => new ObjectRenderer(gfx, source, obj),
            _ => default
        };
    }
}
