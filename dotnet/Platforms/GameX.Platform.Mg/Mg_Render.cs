using OpenStack.Gfx;
using OpenStack.Gfx.Mg;
using System.Collections.Generic;
#pragma warning disable CS9113

namespace GameX.Platforms.Mg;

public static class MgRenderer {
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
