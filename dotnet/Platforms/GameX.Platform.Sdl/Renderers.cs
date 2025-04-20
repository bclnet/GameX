using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Sdl;

namespace GameX.Platforms.Sdl;

public static class SdlRenderer
{
    public static Renderer CreateRenderer(object parent, SdlGfxSprite2D gfx, object obj, string type)
        => type switch
        {
            "TestTri" => new SdlTestTriRenderer(gfx, obj),
            "Texture" => new SdlSpriteRenderer(gfx, obj),
            "Object" => new SdlObjectRenderer(gfx, obj),
            _ => new SdlObjectRenderer(gfx, obj),
        };
}

public class SdlTestTriRenderer(SdlGfxSprite2D gfx, object obj) : TestTriRenderer(gfx, obj) { }
public class SdlSpriteRenderer(SdlGfxSprite2D gfx, object obj) : SpriteRenderer(gfx, obj) { }
public class SdlObjectRenderer(SdlGfxSprite2D gfx, object obj) : Renderer { }
