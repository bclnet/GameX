using GameX.Gamebryo.Formats;
using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.OpenGL;

namespace GameX.Platforms.OpenGL;

public static class OpenGLRenderer {
    static OpenGLRenderer() {
        OpenGLPlatform.BuildersByType[typeof(Binary_Nif)] = OpenGLNifObjectBuilder.BuildObject;
    }
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, ISource source, object obj, string type) {
        if (obj is IHaveSource z) source = z.Source;
        return type switch {
            "TestTri" => new TestTriRenderer(gfx, source, obj),
            "Texture" or "VideoTexture" => new TextureRenderer(gfx, source, obj, 0.., false),
            "Object" => new ObjectRenderer(gfx, source, obj),
            "Material" => new MaterialRenderer(gfx, source, obj),
            "Particle" => new ParticleRenderer(gfx, source, obj),
            //"World" => new OpenGLWorldRenderer(gfx, source,obj),
            "Engine" => new EngineRenderer(gfx, source, obj),
            _ => default,
        };
    }
}
