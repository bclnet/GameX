using GameX;
using GameX.Platforms;
using OpenStack.Gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using static OpenStack.Debug;

namespace Godot.Views;

#region ViewBase

public abstract class ViewBase(Node parent, IGodotGfx gfx, object obj) : IDisposable
{
    protected readonly Node Parent = parent;
    protected readonly IGodotGfx Gfx = gfx;
    protected readonly object Obj = obj;
    public virtual void Dispose() { }
    public abstract void Start();
    public virtual void Update(double delta) { }
    public static ViewBase Create(object parent, IGodotGfx gfx, object obj, string type)
        => type switch
        {
            "Texture" => new ViewTexture(parent as Node, gfx, obj),
            "Object" => new ViewObject(parent as Node, gfx, obj),
            "Cell" => new ViewCell(parent as Node, gfx, obj),
            //"Engine" => new ViewEngine(parent as Node, gfx, obj),
            _ => new ViewObject(parent as Node, gfx, obj),
        };
}

#endregion

#region ViewCell

public class ViewCell(Node parent, IGodotGfx gfx, object obj) : ViewBase(parent, gfx, obj)
{
    public override void Start() { }
}

#endregion

#region ViewEngine

public class ViewEngin(Node parent, IGodotGfx gfx, object obj) : ViewBase(parent, gfx, obj)
{
    object Engine;
    public override void Start() { }
}

#endregion

#region ViewObject

public class ViewObject(Node parent, IGodotGfx gfx, object obj) : ViewBase(parent, gfx, obj)
{
    public override void Start()
    {
        //if (!string.IsNullOrEmpty(View.Param1)) MakeObject(View.Param1);
    }

    void MakeObject(object path) => Gfx.ObjectManager.CreateObject(path);
}

#endregion

#region ViewTexture

public class ViewTexture(Node parent, IGodotGfx gfx, object obj) : ViewBase(parent, gfx, obj)
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
        if (!string.IsNullOrEmpty(path)) MakeTexture(path);
        //if (!string.IsNullOrEmpty(path)) MakeCursor(path);
    }

    Node MakeTexture(string path)
    {
        Log($"MakeTexture {path}");

        //var material = Parent
        var surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.TriangleStrip);
        //surfaceTool.SetSmoothGroup(-1);

//        var st = SurfaceTool.new()

//st.begin(Mesh.PRIMITIVE_TRIANGLES)

//# Prepare attributes for add_vertex.
//st.add_normal(Vector3(0, 0, 1))
//st.add_uv(Vector2(0, 0))
//# Call last for each vertex, adds the above attributes.
//st.add_vertex(Vector3(-1, -1, 0))

//st.add_normal(Vector3(0, 0, 1))
//st.add_uv(Vector2(0, 1))
//st.add_vertex(Vector3(-1, 1, 0))

//st.add_normal(Vector3(0, 0, 1))
//st.add_uv(Vector2(1, 1))
//st.add_vertex(Vector3(1, 1, 0))

//# Create indices, indices are optional.
//st.index()

//# Commit to a mesh.
//var mesh = st.commit()

        Vector3[] vertices = [
            new Vector3(-1f, -1f, +0f),
            new Vector3(-1f, +1f, +0f),
            new Vector3(+1f, -1f, +0f),
            new Vector3(+1f, +1f, +0f)
        ];
        foreach (var v in vertices) surfaceTool.AddVertex(v);
        //surfaceTool.GenerateNormals();
        surfaceTool.Index();
        //surfaceTool.SetMaterial(material);
        var mesh = surfaceTool.Commit();
        //var mesh = new ArrayMesh();
        //mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles)
        var obj = new MeshInstance3D
        {
            Name = "Texture",
            Mesh = mesh,
        };
        //obj.Transform.Rotated(new Vector3(-90f, 180f, -180f), 0f);
        //obj.AddChild
        //(meshRenderer.material, _) = Gfx.MaterialManager.CreateMaterial(new FixedMaterialInfo { MainFilePath = path });
        Parent.AddChild(obj);
        Log($"Done {obj}");
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
        if (string.IsNullOrEmpty(FamilyId)) return;
        Family = FamilyManager.GetFamily(FamilyId);
        if (!string.IsNullOrEmpty(PakUri)) PakFiles.Add(Family.OpenPakFile(new Uri(PakUri)));
        var first = PakFiles.FirstOrDefault();
        Gfx = (IGodotGfx)first?.Gfx;
        View = ViewBase.Create(this, Gfx, Param1, ViewKind.ToString());
        View?.Start();
    }

    public override void _Process(double delta) => View?.Update(delta);

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
}

#endregion