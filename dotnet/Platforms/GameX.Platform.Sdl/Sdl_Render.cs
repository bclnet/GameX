using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Sdl;
using System.Collections.Generic;
using static OpenStack.Gfx.GfX;
#pragma warning disable CS9113

namespace GameX.Platforms.Sdl;

public static class SdlRenderer {
    public static Renderer CreateRenderer(object parent, IList<IOpenGfx> gfx, object obj, string type)
        => type switch {
            "TestTri" => new SdlTestTriRenderer(gfx[XSprite2D] as SdlGfxSprite2D, obj),
            "Texture" => new SdlSpriteRenderer(gfx[XSprite2D] as SdlGfxSprite2D, obj),
            "Object" => new SdlObjectRenderer(gfx[XSprite2D] as SdlGfxSprite2D, obj),
            _ => new SdlObjectRenderer(gfx[XSprite2D] as SdlGfxSprite2D, obj),
        };
}

public class SdlTestTriRenderer(SdlGfxSprite2D gfx, object obj) : TestTriRenderer(gfx, obj) { }
public class SdlSpriteRenderer(SdlGfxSprite2D gfx, object obj) : SpriteRenderer(gfx, obj) { }
public class SdlObjectRenderer(SdlGfxSprite2D gfx, object obj) : Renderer { }
