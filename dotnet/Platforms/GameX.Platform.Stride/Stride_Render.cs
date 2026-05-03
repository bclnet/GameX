using OpenStack.Gfx;
using OpenStack.Gfx.Stride;
using System;
#pragma warning disable CS9113

namespace GameX.Platforms.Stride;

public static class StrideRenderer {
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, object obj, string type) {
        if (obj is IHaveOpenGfx z) gfx = z.Gfx;
        return type switch {
            "TestTri" => new TestTriRenderer(gfx, obj),
            //"Material" => new MaterialRenderer(gfx, obj),
            //"Particle" => new ParticleRenderer(gfx, obj),
            "Texture" or "VideoTexture" => new TextureRenderer(gfx, obj, 0..),
            //"Object" => new ObjectRenderer(gfx, obj),
            //"Cell" => new CellRenderer(gfx, obj),
            "World" => throw new NotImplementedException(),
            //"Engine" => new EngineRenderer(gfx, obj),
            _ => default,
        };
    }
}
