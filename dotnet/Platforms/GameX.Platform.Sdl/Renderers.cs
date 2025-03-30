using OpenStack.Gfx;
using OpenStack.Sdl;
using OpenStack.Sdl.Renderers;

namespace GameX.Platforms.Sdl;

public static class SdlRenderer
{
    public static Renderer CreateRenderer(object parent, ISdlGfx2d gfx, object obj, string type)
        => type switch
        {
            "TestTri" => new SdlTestTriRenderer(gfx, obj),
            "Texture" => new SdlSpriteRenderer(gfx, obj),
            "Object" => new SdlObjectRenderer(gfx, obj),
            _ => new SdlObjectRenderer(gfx, obj),
        };
}

public class SdlTestTriRenderer(ISdlGfx2d gfx, object obj) : TestTriRenderer(gfx, obj) { }
public class SdlSpriteRenderer(ISdlGfx2d gfx, object obj) : SpriteRenderer(gfx, obj) { }
public class SdlObjectRenderer(ISdlGfx2d gfx, object obj) : Renderer { }
