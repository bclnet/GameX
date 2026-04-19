using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.OpenGL;
using System.Collections.Generic;
using static OpenStack.Gfx.GfX;

namespace GameX.Platforms.OpenGL;

public static class OpenGLRenderer {
    public static Renderer CreateRenderer(object parent, IList<IOpenGfx> gfx, object obj, string type)
        => type switch {
            "Texture" or "VideoTexture" => new TextureRenderer(gfx[XModel] as OpenGLGfxModel, obj, 0.., false),
            "Object" => new ObjectRenderer(gfx[XModel] as OpenGLGfxModel, obj),
            "Material" => new MaterialRenderer(gfx[XModel] as OpenGLGfxModel, obj),
            "Particle" => new ParticleRenderer(gfx[XModel] as OpenGLGfxModel, obj),
            "Cell" => new CellRenderer(gfx[XModel] as OpenGLGfxModel, obj),
            //"World" => new OpenGLWorldRenderer(gfx[XModel] as OpenGLGfxModel, obj),
            "Engine" => new EngineRenderer(gfx[XModel] as OpenGLGfxModel, obj),
            "TestTri" => new TestTriRenderer(gfx[XModel] as OpenGLGfxModel, obj),
            _ => default,
        };
}


//public class OpenGLTextureRenderer(OpenGLGfxModel gfx, object obj) : TextureRenderer(gfx, obj, Level, Value0)
//{
//    static Range Level = 0..;
//    static bool Value0;
//}
