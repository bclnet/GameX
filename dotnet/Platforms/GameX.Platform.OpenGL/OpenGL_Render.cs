using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.OpenGL;
using System.Collections.Generic;
using static OpenStack.Gfx.GfX;

namespace GameX.Platforms.OpenGL;

public static class OpenGLRenderer {
    public static Renderer CreateRenderer(object parent, IList<IOpenGfx> gfx, object obj, string type)
        => type switch {
            "Texture" or "VideoTexture" => new OpenGLTextureRenderer(gfx[XModel] as OpenGLGfxModel, obj, 0.., false),
            "Object" => new OpenGLObjectRenderer(gfx[XModel] as OpenGLGfxModel, obj),
            "Material" => new OpenGLMaterialRenderer(gfx[XModel] as OpenGLGfxModel, obj),
            "Particle" => new OpenGLParticleRenderer(gfx[XModel] as OpenGLGfxModel, obj),
            "Cell" => new OpenGLCellRenderer(gfx[XModel] as OpenGLGfxModel, obj),
            "World" => new OpenGLWorldRenderer(gfx[XModel] as OpenGLGfxModel, obj),
            "TestTri" => new OpenGLTestTriRenderer(gfx[XModel] as OpenGLGfxModel, obj),
            _ => default,
        };
}


//public class OpenGLTextureRenderer(OpenGLGfxModel gfx, object obj) : TextureRenderer(gfx, obj, Level, Value0)
//{
//    static Range Level = 0..;
//    static bool Value0;
//}
