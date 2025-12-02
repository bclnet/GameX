using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Mg;
using System.Collections.Generic;
using static OpenStack.Gfx.GfX;

namespace GameX.Platforms.Mg;

public static class MgRenderer {
    public static Renderer CreateRenderer(object parent, IList<IOpenGfx> gfx, object obj, string type)
        => type switch {
            "TestTri" => new MgTestTriRenderer(gfx[XSprite2D] as MgGfxSprite2D, obj),
            "Texture" => new MgSpriteRenderer(gfx[XSprite2D] as MgGfxSprite2D, obj),
            "Object" => new MgObjectRenderer(gfx[XSprite2D] as MgGfxSprite2D, obj),
            _ => new MgObjectRenderer(gfx[XSprite2D] as MgGfxSprite2D, obj),
        };
}

public class MgTestTriRenderer(MgGfxSprite2D gfx, object obj) : TestTriRenderer(gfx, obj) { }
public class MgSpriteRenderer(MgGfxSprite2D gfx, object obj) : SpriteRenderer(gfx, obj) { }
public class MgObjectRenderer(MgGfxSprite2D gfx, object obj) : Renderer { }
