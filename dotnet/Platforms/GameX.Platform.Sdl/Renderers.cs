using OpenStack.Gfx;
using OpenStack.Sdl;
using OpenStack.Sdl.Renderers;

namespace GameX.Platforms.Sdl;

public static class SdlRenderer
{
    public static Renderer CreateRenderer(object parent, SdlGfx2dSprite gfx, object obj, string type)
        => type switch
        {
            "TestTri" => new SdlTestTriRenderer(gfx, obj),
            "Texture" => new SdlSpriteRenderer(gfx, obj),
            "Object" => new SdlObjectRenderer(gfx, obj),
            _ => new SdlObjectRenderer(gfx, obj),
        };
}

public class SdlTestTriRenderer(SdlGfx2dSprite gfx, object obj) : TestTriRenderer(gfx, obj) { }
public class SdlSpriteRenderer(SdlGfx2dSprite gfx, object obj) : SpriteRenderer(gfx, obj) { }
public class SdlObjectRenderer(SdlGfx2dSprite gfx, object obj) : Renderer { }
