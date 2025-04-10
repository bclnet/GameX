﻿using OpenStack.Gfx;
using OpenStack.Stride;
using OpenStack.Stride.Renderers;
using System;

namespace GameX.Platforms.Stride;

public static class StrideRenderer
{
    //using Stride.Engine;
    //protected static Game Game;

    public static Renderer CreateRenderer(object parent, StrideGfx3dModel gfx, object obj, string type)
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

public class StrideTestTriRenderer(StrideGfx3dModel gfx, object obj) : TestTriRenderer(gfx, obj) { }
public class StrideCellRenderer(StrideGfx3dModel gfx, object obj) : Renderer { }
public class StrideMaterialRenderer(StrideGfx3dModel gfx, object obj) : Renderer { }
public class StrideParticleRenderer(StrideGfx3dModel gfx, object obj) : Renderer { }
public class StrideEngineRenderer(StrideGfx3dModel gfx, object obj) : Renderer { }
public class StrideObjectRenderer(StrideGfx3dModel gfx, object obj) : Renderer { }
public class StrideTextureRenderer(StrideGfx3dModel gfx, object obj) : TextureRenderer(gfx, obj, Level)
{
    static Range Level = 0..;
}
