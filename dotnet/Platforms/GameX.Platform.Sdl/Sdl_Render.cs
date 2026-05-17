using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Sdl;
#pragma warning disable CS9113

namespace GameX.Platforms.Sdl;

public static class SdlRenderer {
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, ISource source, object obj, string type) {
        if (obj is IHaveSource z) source = z.Source;
        return type switch {
            "TestTri" => new TestTriRenderer(gfx, source, obj),
            "Texture" => new SpriteRenderer(gfx, source, obj),
            //"Object" => new ObjectRenderer(gfx, source, obj),
            _ => default,
        };
    }
}
