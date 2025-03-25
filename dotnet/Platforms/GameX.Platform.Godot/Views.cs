using GameX;
using GameX.Platforms;
using OpenStack.Gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using SimpleEngine = System.Object;
using static OpenStack.Debug;

namespace Godot.Views;

#region ViewBase

public abstract class ViewBase(IGodotGfx gfx, object obj) : IDisposable
{
    protected readonly IGodotGfx Gfx = gfx;
    protected readonly object Obj = obj;
    public virtual void Dispose() { }
    public abstract void Start();
    public virtual void Update() { }
    public static ViewBase Create(object parent, IGodotGfx gfx, object obj, string type)
    {
        //ViewKind switch
        //{
        //    Kind.Texture => new ViewTexture(this),
        //    Kind.Object => new ViewObject(this),
        //    Kind.Cell => new ViewCell(this),
        //    Kind.Engine => new ViewEngine(this),
        //    _ => new ViewObject(this),
        //};
        return default;
    }
}

#endregion

#region ViewCell

public class ViewCell(IGodotGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override void Start() { }
}

#endregion

#region ViewEngine

public class ViewEngin(IGodotGfx gfx, object obj) : ViewBase(gfx, obj)
{
    SimpleEngine Engine;
    public override void Start() { }
}

#endregion

#region ViewObject

public class ViewObject(IGodotGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override void Start()
    {
        //if (!string.IsNullOrEmpty(View.Param1)) MakeObject(View.Param1);
    }

    void MakeObject(object path) => Gfx.ObjectManager.CreateObject(path);
}

#endregion

#region ViewTexture

public class ViewTexture(IGodotGfx gfx, object obj) : ViewBase(gfx, obj)
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

    GodotObject MakeTexture(string path)
    {
        //var obj = GeometricPrimitive.Plane.New(Game.GraphicsDevice).ToMeshDraw();
        var obj = new GodotObject();
        //var obj = new GodotObject("Name", rotation: Quaternion.CreateFromYawPitchRoll(-90f, 180f, -180f))
        //{
        //    new ModelComponent(new PlaneProceduralModel().Generate(Game.Services))
        //};
        //var obj = Content.CreatePrimitive(PrimitiveType.Plane);
        //obj.transform.rotation = ;
        //var meshRenderer = obj.GetComponent<MeshRenderer>();
        //(meshRenderer.material, _) = Gfx.MaterialManager.CreateMaterial(new FixedMaterialInfo { MainFilePath = path });
        return obj;
    }

    //void MakeCursor(string path) => Cursor.SetCursor(Gfx.TextureManager.CreateTexture(path).tex, Vector2.zero, CursorMode.Auto);
}

#endregion

#region ViewInfo

public class ViewInfo : Node
{
    static ViewInfo() => PlatformX.Activate(GodotPlatform.This);

    public enum Kind
    {
        Texture,
        TextureCursor,
        Object,
        Cell,
        Engine,
    }

    ViewBase View;

    public string FamilyId = "Bethesda";
    public string PakUri = "game:/Morrowind.bsa#Morrowind";

    public Kind ViewKind = Kind.Texture;
    public string Param1 = "bookart/boethiah_256.dds";
    //public string Param1 = "meshes/x/ex_common_balcony_01.nif";

    protected Family Family;
    protected List<PakFile> PakFiles = [];
    protected IGodotGfx Gfx;

    public override void _Ready()
    {
        Log($"_Ready {FamilyId}");
        if (string.IsNullOrEmpty(FamilyId)) return;
        Family = FamilyManager.GetFamily(FamilyId);
        Log($"Family {Family}");
        if (!string.IsNullOrEmpty(PakUri)) PakFiles.Add(Family.OpenPakFile(new Uri(PakUri)));
        var first = PakFiles.FirstOrDefault();
        Gfx = (IGodotGfx)first?.Gfx;
        View = ViewBase.Create(this, Gfx, Param1, ViewKind.ToString());
    }

    //public override void _Process(double delta)
    //{
    //}

    public override void _Notification(int what)
    {
        base._Notification(what);
        switch ((long)what)
        {
            case NotificationPredelete:
                View?.Dispose();
                foreach (var pakFile in PakFiles) pakFile.Dispose();
                PakFiles.Clear();
                break;
        }
    }

    public void Start() => View?.Start();
    public void Update() => View?.Update();
}

#endregion