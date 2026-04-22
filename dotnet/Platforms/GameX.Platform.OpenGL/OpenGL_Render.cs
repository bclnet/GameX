using OpenStack.Gfx;
using OpenStack.Gfx.OpenGL;

namespace GameX.Platforms.OpenGL;

public static class OpenGLRenderer {
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, object obj, string type)
        => type switch {
            "TestTri" => new TestTriRenderer(gfx, obj),
            "Texture" or "VideoTexture" => new TextureRenderer(gfx, obj, 0.., false),
            "Object" => new ObjectRenderer(gfx, obj),
            "Material" => new MaterialRenderer(gfx, obj),
            "Particle" => new ParticleRenderer(gfx, obj),
            //"World" => new OpenGLWorldRenderer(gfx, obj),
            "Engine" => new EngineRenderer(gfx, obj),
            _ => default,
        };
}
