using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Stride;
#pragma warning disable CS9113

namespace GameX.Platforms.Stride;

public static class StrideRenderer {
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, ISource source, object obj, string type) {
        if (obj is IHaveSource z) source = z.Source;
        return type switch {
            "TestTri" => new TestTriRenderer(gfx, source, obj),
            //"Material" => new MaterialRenderer(gfx, source, obj),
            //"Particle" => new ParticleRenderer(gfx, source, obj),
            "Texture" or "VideoTexture" => new TextureRenderer(gfx, source, obj, 0..),
            //"Object" => new ObjectRenderer(gfx, source, obj),
            //"Cell" => new CellRenderer(gfx, source, obj),
            //"World" => throw new NotImplementedException(),
            //"Engine" => new EngineRenderer(gfx, source, obj),
            _ => default,
        };
    }
}
