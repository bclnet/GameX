using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.Diligent;
using GameX.Arkane.Engines;
using GameX.Arkane.Formats.Danae;
using GameX.Gamebryo.Engines;
using GameX.Gamebryo.Formats;
using GameX.Gamebryo.Formats.Nif;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameX.Engines.Diligent;

/// <summary>
/// DiligentRenderer
/// </summary>
public static class DiligentRenderer {
    static DiligentRenderer() {
        DiligentX.BuildersByType[typeof(Binary_Nif)] = BuildNif;
        DiligentX.BuildersByType[typeof(Binary_Ftl)] = BuildFtl;
        DiligentX.BuildersByType[typeof(Binary_Fts)] = BuildFts;
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
        return new DiligentObject {
            Name = name,
            IsStatic = isStatic,
            Mesh = mesh,
            Materials = new DiligentObjectMaterials { Paths = paths.ToArray(), Props = props },
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
        return new DiligentObject {
            Name = "fts",
            IsStatic = isStatic,
            Mesh = mesh,
            Materials = new DiligentObjectMaterials { Paths = paths.ToArray(), Props = props },
            Groups = FtsObjectBuilder.ScenePositions(level),
        };
    }

    /// <summary>
    /// Copies NiTriShape positions and triangle indices. Skips colliders and skinning. Does not reparse NIF bytes.
    /// </summary>
    static object BuildNif(object path, bool isStatic, object materialManager) {
        _ = materialManager;
        var nif = (Binary_Nif)path;
        var shapes = new List<DiligentNifShape>();
        var props = new List<MaterialStd2Prop>();
        if (nif.Roots != null)
            foreach (var root in nif.Roots)
                if (root != null) WalkNif(root.Value, shapes, props);
        return new DiligentObject {
            Name = string.IsNullOrEmpty(nif.Name) ? "nif" : nif.Name,
            IsStatic = isStatic,
            Mesh = shapes.ToArray(),
            Materials = new DiligentObjectMaterials { Paths = [], Props = props.ToArray() },
            Groups = Array.Empty<(string Name, Vector3 Position)>(),
        };
    }

    static void WalkNif(NiObject node, List<DiligentNifShape> shapes, List<MaterialStd2Prop> props) {
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
            shapes.Add(new DiligentNifShape { Name = shape.Name, Positions = positions, Indices = indices });
            props.Add(NifObjectBuilder.ToMaterialProp(shape));
            return;
        }
        if (node is NiNode n && n.Children != null)
            foreach (var child in n.Children)
                if (child != null) WalkNif(child.Value, shapes, props);
    }

    /// <summary>
    /// False on this build. Diligent Engine is not linked.
    /// </summary>
    public static bool DiligentLinked() => DiligentShim.Linked;

    /// <summary>
    /// This build returns "not linked".
    /// </summary>
    public static string DiligentVersion() => DiligentShim.Version;

    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, ISource source, object obj, string type) {
        if (obj is IHaveSource z) source = z.Source;
        return type switch {
            "TestTri" => new TestTriRenderer(gfx, source, obj),
            "Texture" => new TextureRenderer(gfx, source, obj, 0..),
            "Object" => new ObjectRenderer(gfx, source, obj),
            _ => default,
        };
    }
}

/// <summary>
/// Holder for an object built without a GPU upload.
/// </summary>
public class DiligentObject {
    public string Name;
    public bool IsStatic;
    public object Mesh;
    public object Materials;
    public object Groups;
}

/// <summary>
/// Texture paths from the shared builder, and one material prop per submesh or shape.
/// </summary>
public class DiligentObjectMaterials {
    public string[] Paths;
    public MaterialStd2Prop[] Props;
}

/// <summary>
/// One NiTriShape. Positions and triangle indices are copies.
/// </summary>
public class DiligentNifShape {
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
        if (Obj == null || !DiligentX.BuildersByType.TryGetValue(Obj.GetType(), out var build)) return;
        Built = build(Obj, true, null);
    }

    public override void Update(float deltaTime) { _ = deltaTime; }
}

/// <summary>
/// ViewInfo
/// </summary>
public class ViewInfo {
    static ViewInfo() => EngineX.Activate(DiligentEngine.This);
}
