using GameX.Platforms;
using OpenStack.Gfx;
using System;
using System.Collections.Generic;

namespace Sdl.Views;

#region ViewBase 

public abstract class ViewBase(ISdlGfx gfx, object obj) : IDisposable
{
    protected readonly ISdlGfx Gfx = gfx;
    protected readonly object Obj = obj;
    
    public virtual void Dispose() { }
    public abstract void Start();
    public virtual void Update() { }

    public static ViewBase Create(object parent, ISdlGfx gfx, object obj, string type)
        => type switch
        {
            "Texture" => new ViewTexture(gfx, obj),
            "Object" => new ViewObject(gfx, obj),
            _ => new ViewObject(gfx, obj),
        };
}

#endregion


#region ViewObject

public class ViewObject(ISdlGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override void Start()
    {
        //if (!string.IsNullOrEmpty(View.Param1)) MakeObject(View.Param1);
    }

    void MakeObject(object path) => Gfx.ObjectManager.CreateObject(path);
}

#endregion

#region ViewTexture

public class ViewTexture(ISdlGfx gfx, object obj) : ViewBase(gfx, obj)
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
        var path = Obj is string z ? z : null;
        //if (!string.IsNullOrEmpty(path)) MakeTexture(path);
    }

    //object MakeTexture(string path)
    //{
    //    var obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
    //    obj.transform.rotation = Quaternion.Euler(-90f, 180f, -180f);
    //    var meshRenderer = obj.GetComponent<MeshRenderer>();
    //    (meshRenderer.material, _) = Gfx.MaterialManager.CreateMaterial(new FixedMaterialInfo { MainFilePath = path });
    //    return obj;
    //}
}

#endregion
