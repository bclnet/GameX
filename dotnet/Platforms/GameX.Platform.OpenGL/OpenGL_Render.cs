using GameX.Gamebryo.Formats;
using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.OpenGL;

namespace GameX.Platforms.OpenGL;

public static class OpenGLRenderer {
    static OpenGLRenderer() {
        OpenGLPlatform.BuildersByType[typeof(Binary_Nif)] = (src, isStatic, materialManager) => OpenGLNifObjectBuilder.BuildObject((Binary_Nif)src, isStatic, (MaterialManager<GLRenderMaterial, int>)materialManager);
    }
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, object obj, string type) {
        if (obj is IHaveOpenGfx z) gfx = z.Gfx;
        return type switch {
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
}
