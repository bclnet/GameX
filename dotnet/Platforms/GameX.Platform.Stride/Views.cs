using GameX.Platforms;
using OpenStack.Gfx;
using Stride.Engine;
using Stride.Rendering.ProceduralModels;
using System;
using System.Collections.Generic;
using System.Numerics;
using SimpleEngine = System.Object;

namespace Stride.Views;

#region ViewBase

public abstract class ViewBase(IStrideGfx gfx, object obj) : IDisposable
{
    protected readonly IStrideGfx Gfx = gfx;
    protected readonly object Obj = obj;
    protected Game Game;

    public virtual void Dispose() { }
    public abstract void Start();
    public virtual void Update() { }

    public static ViewBase Create(object parent, IStrideGfx gfx, object obj, string type)
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

public class ViewCell(IStrideGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override void Start() { }
}

#endregion

#region ViewMaterial

public class ViewMaterial(IStrideGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override void Start() { }
}

#endregion

#region ViewParticle

public class ViewParticle(IStrideGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override void Start() { }
}

#endregion

#region ViewEngine

public class ViewEngine(IStrideGfx gfx, object obj) : ViewBase(gfx, obj)
{
    SimpleEngine Engine;
    public override void Start() { }
}

#endregion

#region ViewObject

public class ViewObject(IStrideGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override void Start()
    {
        //if (!string.IsNullOrEmpty(View.Param1)) MakeObject(View.Param1);
    }

    void MakeObject(object path) => Gfx.ObjectManager.CreateObject(path);
}

#endregion

#region ViewTexture

public class ViewTexture(IStrideGfx gfx, object obj) : ViewBase(gfx, obj)
{
    class FixedMaterialInfo : IFixedMaterial
    {
        public string Name { get; set; }
        public string ShaderName { get; set; }
        public IDictionary<string, bool> GetShaderArgs() => null;
        public IDictionary<string, object> Data { get; set; }
        public string MainFilePath { get; set; }
        public string DarkFilePath { get; set; }
        public string DetailFilePath { get; set; }
        public string GlossFilePath { get; set; }
        public string GlowFilePath { get; set; }
        public string BumpFilePath { get; set; }
        public bool AlphaBlended { get; set; }
        public int SrcBlendMode { get; set; }
        public int DstBlendMode { get; set; }
        public bool AlphaTest { get; set; }
        public float AlphaCutoff { get; set; }
        public bool ZWrite { get; set; }
    }

    public override void Start()
    {
        //if (!string.IsNullOrEmpty(View.Param1)) MakeTexture(View.Param1);
        //if (!string.IsNullOrEmpty(View.Param2)) MakeCursor(View.Param2);
    }

    Entity MakeTexture(string path)
    {
        //var obj = GeometricPrimitive.Plane.New(Game.GraphicsDevice).ToMeshDraw();
        var obj = new Entity("Name", rotation: Quaternion.CreateFromYawPitchRoll(-90f, 180f, -180f))
        {
            new ModelComponent(new PlaneProceduralModel().Generate(Game.Services))
        };
        //var obj = Content.CreatePrimitive(PrimitiveType.Plane);
        //obj.transform.rotation = ;
        //var meshRenderer = obj.GetComponent<MeshRenderer>();
        //(meshRenderer.material, _) = Gfx.MaterialManager.CreateMaterial(new FixedMaterialInfo { MainFilePath = path });
        return obj;
    }

    //void MakeCursor(string path) => Cursor.SetCursor(Gfx.TextureManager.CreateTexture(path).tex, Vector2.zero, CursorMode.Auto);
}

#endregion

#region ViewVideoTexture

public class ViewVideoTexture(IStrideGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override void Start() { }
}

#endregion

#region ViewTestTri

public class ViewTestTri(IStrideGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override void Start() { }
}

#endregion
