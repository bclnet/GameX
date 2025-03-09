using OpenStack.Gfx;
using OpenStack.Gfx.Gl;
using OpenStack.Gfx.Gl.Renders;
using OpenStack.Gfx.Renders;
using OpenStack.Gfx.Textures;
using System;
using System.Collections.Generic;
using SimpleEngine = System.Object;

namespace OpenTK.Views;

#region ViewBase

public abstract class ViewBase(IOpenGLGfx gfx, object obj) : IDisposable
{
    protected readonly IOpenGLGfx Gfx = gfx;
    protected readonly object Obj = obj;
    protected IList<IRenderer> Renderers;
    public bool ToggleValue;
    public Range Level = 0..;
    protected const int FACTOR = 0;

    public virtual (int, int)? GetViewport((int, int) size) => default;
    public virtual void Dispose() { }
    public abstract void Start();
    public virtual void Update(int deltaTime) { }
    public void Render(Camera camera, float frameTime)
    {
        if (Renderers == null) return;
        foreach (var renderer in Renderers) renderer.Render(camera, RenderPass.Both);
    }

    public static ViewBase CreateView(object parent, IOpenGLGfx gfx, object obj, string type)
        => type switch
        {
            "Material" => new ViewMaterial(gfx, obj),
            "Particle" => new ViewParticle(gfx, obj),
            "TestTri" => new ViewTestTri(gfx, obj),
            "Texture" => new ViewTexture(gfx, obj),
            "VideoTexture" => new ViewVideoTexture(gfx, obj),
            "Object" => new ViewObject(gfx, obj),
            "Cell" => new ViewCell(gfx, obj),
            "World" => throw new NotImplementedException(),
            "Engine" => new ViewEngine(gfx, obj),
            _ => default,
        };
}

#endregion

#region ViewCell

public class ViewCell(IOpenGLGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override void Start() { }
}

#endregion

#region ViewParticle

public class ViewParticle(IOpenGLGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override void Start() { }
}

#endregion

#region ViewEngine

public class ViewEngine(IOpenGLGfx gfx, object obj) : ViewBase(gfx, obj)
{
    SimpleEngine Engine;
    public override void Start() { }
}

#endregion

#region ViewObject

public class ViewObject(IOpenGLGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override void Start()
    {
        //if (!string.IsNullOrEmpty(View.Param1)) MakeObject(View.Param1);
    }

    void MakeObject(object path) => Gfx.ObjectManager.CreateObject(path);
}

#endregion

#region ViewMaterial

public class ViewMaterial(IOpenGLGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override void Start()
    {
        var obj = (IMaterial)Obj;
        Gfx.TextureManager.DeleteTexture(obj);
        var (material, _) = Gfx.MaterialManager.CreateMaterial(obj);
        Renderers = [new MaterialRenderer(Gfx, material)];
    }
}

#endregion

#region ViewTexture

public class ViewTexture(IOpenGLGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override (int, int)? GetViewport((int, int) size)
        => Obj is not ITexture o ? default
        : o.Width > 1024 || o.Height > 1024 || false ? size : (o.Width << FACTOR, o.Height << FACTOR);

    public override void Start()
    {
        var obj = (ITexture)Obj;
        Gfx.TextureManager.DeleteTexture(obj);
        var (texture, _) = Gfx.TextureManager.CreateTexture(obj, Level);
        Renderers = [new TextureRenderer(Gfx, texture, ToggleValue)];
    }
}

#endregion

#region ViewVideoTexture

public class ViewVideoTexture(IOpenGLGfx gfx, object obj) : ViewBase(gfx, obj)
{
    int FrameDelay;

    public override (int, int)? GetViewport((int, int) size)
        => Obj is not ITexture o ? default
        : o.Width > 1024 || o.Height > 1024 || false ? size : (o.Width << FACTOR, o.Height << FACTOR);

    public override void Start()
    {
        var obj = (ITextureFrames)Obj;
        Gfx.TextureManager.DeleteTexture(obj);
        var (texture, _) = Gfx.TextureManager.CreateTexture(obj, Level);
        Renderers = [new TextureRenderer(Gfx, texture, ToggleValue)];
    }

    public override void Update(int deltaTime)
    {
        var obj = (ITextureFrames)Obj;
        if (Gfx == null || obj == null || !obj.HasFrames) return;
        FrameDelay += deltaTime;
        if (FrameDelay <= obj.Fps || !obj.DecodeFrame()) return;
        FrameDelay = 0; // reset delay between frames
        Gfx.TextureManager.ReloadTexture(obj, Level);
    }
}

#endregion

#region ViewTestTri

public class ViewTestTri(IOpenGLGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override void Start() => Renderers = [new TestTriRenderer(Gfx)];
}

#endregion