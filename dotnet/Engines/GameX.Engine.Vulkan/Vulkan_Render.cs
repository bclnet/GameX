using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.Vulkan;
using GameX.Arkane.Engines;
using GameX.Arkane.Formats.Danae;
using GameX.Gamebryo.Engines;
using GameX.Gamebryo.Formats;
using GameX.Gamebryo.Formats.Nif;
using System;
using System.Collections.Generic;
using System.Numerics;
#pragma warning disable CS9113, CS0169

namespace GameX.Engines.Vulkan;

/// <summary>
/// VulkanRenderer
/// </summary>
public static class VulkanRenderer {
    static VulkanRenderer() {
        VulkanX.BuildersByType[typeof(Binary_Nif)] = BuildNif;
        VulkanX.BuildersByType[typeof(Binary_Ftl)] = BuildFtl;
        VulkanX.BuildersByType[typeof(Binary_Fts)] = BuildFts;
    }

    /// <summary>
    /// Danae is Y-up, so positions stay as stored. materialManager may be null.
    /// </summary>
    static object BuildFtl(object path, bool isStatic, object materialManager) {
        _ = materialManager;
        var obj = ((Binary_Ftl)path).Obj;
        var name = string.IsNullOrEmpty(obj.File) ? "ftl" : obj.File;
        var mesh = FtlObjectBuilder.ToGeometry(obj);
        var paths = new List<string>();
        foreach (var texturePath in FtlObjectBuilder.GetTexturePaths(obj)) paths.Add(texturePath);
        var props = new MaterialStd2Prop[mesh.Submeshes.Length];
        for (var i = 0; i < mesh.Submeshes.Length; i++) {
            var sub = mesh.Submeshes[i];
            E_TEXTURE tex = null;
            if (obj.Textures != null && sub.TexId >= 0 && sub.TexId < obj.Textures.Length) tex = obj.Textures[sub.TexId];
            props[i] = FtlObjectBuilder.ToMaterialProp(tex);
        }
        return new VulkanObject {
            Name = name,
            IsStatic = isStatic,
            Mesh = mesh,
            Materials = new VulkanObjectMaterials { Paths = paths.ToArray(), Props = props },
            Groups = FtlObjectBuilder.GroupPositions(obj),
        };
    }

    /// <summary>
    /// An FTS map. Danae is Y-up, so positions stay as stored. materialManager may be null.
    /// </summary>
    static object BuildFts(object path, bool isStatic, object materialManager) {
        _ = materialManager;
        var level = ((Binary_Fts)path).Level;
        var mesh = FtsObjectBuilder.ToGeometry(level);
        var paths = new List<string>();
        foreach (var texturePath in FtsObjectBuilder.GetTexturePaths(level)) paths.Add(texturePath);
        var props = new MaterialStd2Prop[mesh.Submeshes.Length];
        for (var i = 0; i < mesh.Submeshes.Length; i++) {
            var sub = mesh.Submeshes[i];
            E_TEXTURE tex = null;
            if (level.Textures != null && sub.TexId >= 0 && sub.TexId < level.Textures.Length) tex = level.Textures[sub.TexId];
            props[i] = FtsObjectBuilder.ToMaterialProp(tex, sub.Type);
        }
        return new VulkanObject {
            Name = "fts",
            IsStatic = isStatic,
            Mesh = mesh,
            Materials = new VulkanObjectMaterials { Paths = paths.ToArray(), Props = props },
            Groups = FtsObjectBuilder.ScenePositions(level),
        };
    }

    /// <summary>
    /// Copies NiTriShape positions and triangle indices. Skips colliders and skinning. Does not reparse NIF bytes.
    /// </summary>
    static object BuildNif(object path, bool isStatic, object materialManager) {
        _ = materialManager;
        var nif = (Binary_Nif)path;
        var shapes = new List<VulkanNifShape>();
        var props = new List<MaterialStd2Prop>();
        if (nif.Roots != null)
            foreach (var root in nif.Roots)
                if (root != null) WalkNif(root.Value, shapes, props);
        return new VulkanObject {
            Name = string.IsNullOrEmpty(nif.Name) ? "nif" : nif.Name,
            IsStatic = isStatic,
            Mesh = shapes.ToArray(),
            Materials = new VulkanObjectMaterials { Paths = [], Props = props.ToArray() },
            Groups = Array.Empty<(string Name, Vector3 Position)>(),
        };
    }

    static void WalkNif(NiObject node, List<VulkanNifShape> shapes, List<MaterialStd2Prop> props) {
        if (node == null || node is RootCollisionNode) return;
        if (node is NiTriShape shape) {
            if (shape.SkinInstance != null) return;
            if (shape.Data?.Value is not NiTriShapeData data) return;
            var src = data.Vertices;
            Vector3[] positions;
            if (src == null) positions = [];
            else { positions = new Vector3[src.Length]; Array.Copy(src, positions, src.Length); }
            int[] indices;
            if (data.Triangles == null) indices = [];
            else {
                indices = new int[data.Triangles.Length * 3];
                for (var i = 0; i < data.Triangles.Length; i++) {
                    indices[i * 3] = data.Triangles[i].v1;
                    indices[i * 3 + 1] = data.Triangles[i].v2;
                    indices[i * 3 + 2] = data.Triangles[i].v3;
                }
            }
            shapes.Add(new VulkanNifShape { Name = shape.Name, Positions = positions, Indices = indices });
            props.Add(NifObjectBuilder.ToMaterialProp(shape));
            return;
        }
        if (node is NiNode n && n.Children != null)
            foreach (var child in n.Children)
                if (child != null) WalkNif(child.Value, shapes, props);
    }

    /// <summary>
    /// CreateRenderer
    /// </summary>
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, ISource source, object obj, string type) {
        if (obj is IHaveSource z) source = z.Source;
        return type switch {
            "TestTri" => new TestTriRenderer(gfx, source, obj),
            "Texture" => new TextureRenderer(gfx, source, obj, 0..),
            "Object" => new ObjectRenderer(gfx, source, obj),
            //"Cell" => new CellRenderer(gfx, source, obj),
            //"Engine" => new EngineRenderer(gfx, source, obj),
            _ => default,
        };
    }
}

/// <summary>
/// Holder for an object built without a GPU upload.
/// </summary>
public class VulkanObject {
    public string Name;
    public bool IsStatic;
    public object Mesh;
    public object Materials;
    public object Groups;
}

/// <summary>
/// Texture paths from the shared builder, and one material prop per submesh or shape.
/// </summary>
public class VulkanObjectMaterials {
    public string[] Paths;
    public MaterialStd2Prop[] Props;
}

/// <summary>
/// One NiTriShape. Positions and triangle indices are copies.
/// </summary>
public class VulkanNifShape {
    public string Name;
    public Vector3[] Positions;
    public int[] Indices;
}

/// <summary>
/// ObjectRenderer. Start invokes the builder registered for obj's type and stores the holder. An unknown type does nothing.
/// </summary>
public class ObjectRenderer : Renderer {
    readonly object Obj;
    public object Built;

    public ObjectRenderer(IOpenGfx[] gfx, ISource source, object obj) {
        _ = gfx;
        _ = source;
        Obj = obj;
    }

    public override void Start() {
        if (Obj == null || !VulkanX.BuildersByType.TryGetValue(Obj.GetType(), out var build)) return;
        Built = build(Obj, true, null);
    }

    public override void Update(float deltaTime) { _ = deltaTime; }
}

/// <summary>
/// ViewInfo
/// </summary>
public class ViewInfo {
    static ViewInfo() => EngineX.Activate(VulkanEngine.This);

    /// <summary>FamilyId</summary>
    public string FamilyId = "Bethesda";
    /// <summary>ArcUri</summary>
    public string ArcUri = "game:/Morrowind.bsa#Morrowind";
    /// <summary>Type</summary>
    public string Type = "Texture";
    /// <summary>Path</summary>
    public string Path = "bookart/boethiah_256.dds";
    //public string Path = "meshes/x/ex_common_balcony_01.nif";

    /// <summary>Family</summary>
    protected Family Family;
    /// <summary>Source</summary>
    protected Archive Source;
    /// <summary>Gfx</summary>
    protected VulkanGfxModel Gfx;
    Renderer Renderer;

    //public override void _Ready()
    //{
    //    if (string.IsNullOrEmpty(FamilyId)) return;
    //    Family = FamilyManager.GetFamily(FamilyId);
    //    if (!string.IsNullOrEmpty(ArcUri)) Archives.Add(Family.OpenArchive(new Uri(ArcUri)));
    //    var first = Archives.FirstOrDefault();
    //    Gfx = (IOgreGfx3d)first?.Gfx;
    //    Renderer = VulkanRenderer.CreateRenderer(this, Gfx, Param1, ViewKind.ToString());
    //    Renderer?.Start();
    //}

    //public override void _Process(double delta) => Renderer?.Update((float)delta);

    //public override void _Notification(int what)
    //{
    //    base._Notification(what);
    //    switch ((long)what)
    //    {
    //        case NotificationPredelete:
    //            //Renderer?.Dispose();
    //            foreach (var archive in Archives) archive.Dispose();
    //            Archives.Clear();
    //            break;
    //    }
    //}
}
