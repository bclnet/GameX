using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Ex;
using System.Collections.Generic;
using static OpenStack.Gfx.GfX;
#pragma warning disable CS9113

namespace GameX.Platforms.Ex;

public static class ExRenderer {
    public static Renderer CreateRenderer(object parent, IList<IOpenGfx> gfx, object obj, string type)
        => type switch {
            "TestTri" => new ExTestTriRenderer(gfx[XSprite2D] as ExGfxSprite2D, obj),
            "Texture" => new ExSpriteRenderer(gfx[XSprite2D] as ExGfxSprite2D, obj),
            "Object" => new ExObjectRenderer(gfx[XSprite2D] as ExGfxSprite2D, obj),
            _ => new ExObjectRenderer(gfx[XSprite2D] as ExGfxSprite2D, obj),
        };
}

public class ExTestTriRenderer(ExGfxSprite2D gfx, object obj) : TestTriRenderer(gfx, obj) { }
public class ExSpriteRenderer(ExGfxSprite2D gfx, object obj) : SpriteRenderer(gfx, obj) { }
public class ExObjectRenderer(ExGfxSprite2D gfx, object obj) : Renderer { }
