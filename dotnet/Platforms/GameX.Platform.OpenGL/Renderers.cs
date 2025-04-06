using OpenStack.Gfx;
using OpenStack.Gl;
using OpenStack.Gl.Renderers;
using System;

namespace GameX.Platforms.OpenGL;

public static class OpenGLRenderer
{
    public static Renderer CreateRenderer(object parent, OpenGLGfx3dModel gfx, object obj, string type)
        => type switch
        {
            "TestTri" => new OpenGLTestTriRenderer(gfx, obj),
            "Material" => new OpenGLMaterialRenderer(gfx, obj),
            "Particle" => new OpenGLParticleRenderer(gfx, obj),
            "Texture" or "VideoTexture" => new OpenGLTextureRenderer(gfx, obj),
            "Object" => new OpenGLObjectRenderer(gfx, obj),
            "Cell" => new OpenGLCellRenderer(gfx, obj),
            "World" => throw new NotImplementedException(),
            "Engine" => new OpenGLEngineRenderer(gfx, obj),
            _ => default,
        };
}

public class OpenGLTestTriRenderer(OpenGLGfx3dModel gfx, object obj) : TestTriRenderer(gfx, obj) { }
public class OpenGLCellRenderer(OpenGLGfx3dModel gfx, object obj) : Renderer { }
public class OpenGLParticleRenderer(OpenGLGfx3dModel gfx, object obj) : Renderer { }
public class OpenGLEngineRenderer(OpenGLGfx3dModel gfx, object obj) : Renderer { }
public class OpenGLObjectRenderer(OpenGLGfx3dModel gfx, object obj) : Renderer { }
public class OpenGLMaterialRenderer(OpenGLGfx3dModel gfx, object obj) : MaterialRenderer(gfx, obj) { }
public class OpenGLTextureRenderer(OpenGLGfx3dModel gfx, object obj) : TextureRenderer(gfx, obj, Level, Value0)
{
    static Range Level = 0..;
    static bool Value0;
}

//    public override void Start()
//    {
//        //if (!string.IsNullOrEmpty(View.Param1)) MakeObject(View.Param1);
//    }
//    void MakeObject(object path) => Gfx.ObjectManager.CreateObject(path);
