using OpenStack.Gfx;
using OpenStack.Gfx.Sdl;
#pragma warning disable CS9113

namespace GameX.Platforms.Sdl;

public static class SdlRenderer {
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, object obj, string type)
        => type switch {
            "TestTri" => new TestTriRenderer(gfx, obj),
            "Texture" => new SpriteRenderer(gfx, obj),
            //"Object" => new ObjectRenderer(gfx, obj),
            _ => default,
        };
}
