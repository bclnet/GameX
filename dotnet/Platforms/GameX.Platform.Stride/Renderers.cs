using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Stride;
using System;

namespace GameX.Platforms.Stride;

public static class StrideRenderer
{
    //using Stride.Engine;
    //protected static Game Game;

    public static Renderer CreateRenderer(object parent, StrideGfxModel gfx, object obj, string type)
        => type switch
        {
            "TestTri" => new StrideTestTriRenderer(gfx, obj),
            "Material" => new StrideMaterialRenderer(gfx, obj),
            "Particle" => new StrideParticleRenderer(gfx, obj),
            "Texture" or "VideoTexture" => new StrideTextureRenderer(gfx, obj),
            "Object" => new StrideObjectRenderer(gfx, obj),
            "Cell" => new StrideCellRenderer(gfx, obj),
            "World" => throw new NotImplementedException(),
            "Engine" => new StrideEngineRenderer(gfx, obj),
            _ => default,
        };
}

public class StrideTestTriRenderer(StrideGfxModel gfx, object obj) : TestTriRenderer(gfx, obj) { }
public class StrideCellRenderer(StrideGfxModel gfx, object obj) : Renderer { }
public class StrideMaterialRenderer(StrideGfxModel gfx, object obj) : Renderer { }
public class StrideParticleRenderer(StrideGfxModel gfx, object obj) : Renderer { }
public class StrideEngineRenderer(StrideGfxModel gfx, object obj) : Renderer { }
public class StrideObjectRenderer(StrideGfxModel gfx, object obj) : Renderer { }
public class StrideTextureRenderer(StrideGfxModel gfx, object obj) : TextureRenderer(gfx, obj, Level)
{
    static Range Level = 0..;
}
