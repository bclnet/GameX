using GameX.Gamebryo.Formats;
using GameX.Gamebryo.Formats.Nif;
using Godot;
using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static GameX.Gamebryo.Engines.NifObjectBuilder;

namespace GameX.Engines.Godot;

/// <summary>
/// A NIF becomes a Node3D tree. NiTriShapes are mesh instances. Colliders, layers and skinning are skipped.
/// Vertices use the same Y/Z swap as Unity (`ToGodot`), scaled by MeterInUnits. NIF is Z-up; Godot is Y-up.
/// </summary>
public static class GodotNifObjectBuilder {
    public static async Task<Node> BuildObject(ISource source, object path, bool isStatic, MaterialManager<Material, Texture2D> materialManager) {
        var o = (Binary_Nif)path; var name = o.Name;
        Debug.Assert(name != null && o.Roots.Length > 0);

        var textureManager = materialManager.TextureManager;
        foreach (var texturePath in o.GetTexturePaths()) textureManager.Preload(source, texturePath);

        // NIF files can have any number of root NiObjects.
        // If there is only one root, instantiate that directly.
        // If there are multiple roots, create a container Node3D and parent the roots to it.
        if (o.Roots.Length == 1) {
            var s = o.Roots[0].Value;
            var gobj = await InstantiateNiObject(source, isStatic, materialManager, s);
            if (gobj == null) {
                Log.Info($"{name} resulted in a null Node when instantiated.");
                gobj = Named(name);
            }
            // Morrowind discards the root NiNode transform.
            else if (s is NiNode) ((Node3D)gobj).Transform = Transform3D.Identity;
            return gobj;
        }
        else {
            Log.Info($"{name} has multiple roots.");
            var gobj = Named(name);
            foreach (var rootRef in o.Roots) {
                var child = await InstantiateNiObject(source, isStatic, materialManager, rootRef.Value);
                if (child != null) gobj.AddChild(child);
            }
            return gobj;
        }
    }

    static void ApplyNiAVObject(Node3D obj, NiAVObject s) {
        var r = s.Rotation;
        // Same axis swap as UnityX.ToUnityRotation: (x, y, z) -> (x, z, y).
        var basis = new Basis(
            new Vector3(r.M11, r.M31, r.M21),
            new Vector3(r.M13, r.M33, r.M23),
            new Vector3(r.M12, r.M32, r.M22));
        obj.Transform = new Transform3D(basis.Scaled(new Vector3(s.Scale, s.Scale, s.Scale)), s.Translation.ToGodot() / MeterInUnits);
    }

    /// <summary>
    /// Creates a Node3D for an NiObject, or null if that NiObject does not need one.
    /// </summary>
    static Task<Node> InstantiateNiObject(ISource source, bool isStatic, MaterialManager<Material, Texture2D> materialManager, NiObject s)
        => s switch {
            NiTriShape z => InstantiateNiTriShape(source, isStatic, materialManager, z, true),
            RootCollisionNode z => InstantiateRcnNode(source, isStatic, materialManager, z),
            NiNode z => InstantiateNiNode(source, isStatic, materialManager, z),
            NiTextureEffect => Task.FromResult<Node>(null),
            NiRotatingParticles => Task.FromResult<Node>(null),
            NiAutoNormalParticles => Task.FromResult<Node>(null),
            _ => throw new NotImplementedException($"Tried to instantiate an unsupported NiObject ({s.GetType().Name})."),
        };

    static async Task<Node> InstantiateNiNode(ISource source, bool isStatic, MaterialManager<Material, Texture2D> materialManager, NiNode s) {
        var obj = Named(s.Name);
        foreach (var t in s.Children)
            if (t != null) {
                var child = await InstantiateNiObject(source, isStatic, materialManager, t.Value);
                if (child != null) obj.AddChild(child);
            }
        ApplyNiAVObject(obj, s);
        return obj;
    }

    static async Task<Node> InstantiateRcnNode(ISource source, bool isStatic, MaterialManager<Material, Texture2D> materialManager, RootCollisionNode s) {
        var obj = Named($"Root Collision Node{s.Name}");
        foreach (var t in s.Children)
            if (t != null)
                switch (t.Value) {
                    case NiTriShape z: obj.AddChild(await InstantiateNiTriShape(source, isStatic, materialManager, z, false)); break;
                    case AvoidNode: break;
                    default: Log.Info($"Unsupported collider NiObject: {t.Value.GetType().Name}"); break;
                }
        ApplyNiAVObject(obj, s);
        return obj;
    }

    static async Task<Node> InstantiateNiTriShape(ISource source, bool isStatic, MaterialManager<Material, Texture2D> materialManager, NiTriShape s, bool visual) {
        var obj = Named(s.Name);
        if (!visual) {
            ApplyNiAVObject(obj, s);
            return obj;
        }
        var mesh = ToGeometry((NiTriShapeData)s.Data.Value);
        var inst = new MeshInstance3D { Mesh = mesh };
        if (!string.IsNullOrEmpty(s.Name)) inst.Name = s.Name;
        var materialProps = ToMaterialProp(s);
        var (material, _) = await materialManager.Create(source, materialProps);
        if (mesh.GetSurfaceCount() > 0) inst.SetSurfaceOverrideMaterial(0, material);
        if (materialProps.Textures == null || s.Flags.HasFlag(Flags.Hidden)) inst.Visible = false;
        ApplyNiAVObject(inst, s);
        return inst;
    }

    static ArrayMesh ToGeometry(NiTriShapeData s) {
        var length = s.Vertices.Length;
        var vertices = new Vector3[length];
        for (var i = 0; i < vertices.Length; i++) vertices[i] = s.Vertices[i].ToGodot() / MeterInUnits;
        Vector3[] normals = null;
        if (s.Normals != null) {
            normals = new Vector3[length];
            for (var i = 0; i < normals.Length; i++) normals[i] = s.Normals[i].ToGodot();
        }
        Vector2[] uvs = null;
        if (s.UVSets != null && s.UVSets.Length > 0) {
            var vals = s.UVSets[0];
            uvs = new Vector2[length];
            for (var i = 0; i < uvs.Length; i++) { ref var z = ref vals[i]; uvs[i] = new Vector2(z.u, z.v); }
        }
        var triangles = new int[(int)s.NumTrianglePoints];
        for (var i = 0; i < s.Triangles.Length; i++) {
            ref var z = ref s.Triangles[i];
            var baseI = 3 * i; triangles[baseI] = z.v1; triangles[baseI + 1] = z.v3; triangles[baseI + 2] = z.v2;
        }

        var mesh = new ArrayMesh();
        if (vertices.Length == 0 || triangles.Length == 0) return mesh;
        var arrays = new global::Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices;
        if (normals != null) arrays[(int)Mesh.ArrayType.Normal] = normals;
        if (uvs != null) arrays[(int)Mesh.ArrayType.TexUV] = uvs;
        arrays[(int)Mesh.ArrayType.Index] = triangles;
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        if (s.Normals == null) {
            var st = new SurfaceTool();
            st.CreateFrom(mesh, 0);
            st.GenerateNormals(false);
            mesh = st.Commit(null, 0uL);
        }
        return mesh;
    }

    static Node3D Named(string name) {
        var n = new Node3D();
        if (!string.IsNullOrEmpty(name)) n.Name = name;
        return n;
    }
}
