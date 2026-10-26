using GameX.Gamebryo.Formats;
using GameX.Gamebryo.Formats.Nif;
using Mirage;
using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.Mirage;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using static GameX.Gamebryo.Engines.NifObjectBuilder;

namespace GameX.Engines.Mirage;

/// <summary>
/// MirageNifObjectBuilder - a NIF becomes a Node tree: NiNodes are nodes, NiTriShapes are nodes with a MeshRenderer, RootCollisionNodes carry colliders.
/// Coordinates are the NIF's own (Z up, scaled by MeterInUnits); Mirage does not need the axis swap Unity does.
/// </summary>
public static class MirageNifObjectBuilder {
    public static async Task<Node> BuildObject(ISource source, object path, bool isStatic, MaterialManager<Material, Texture> materialManager) {
        var o = (Binary_Nif)path; var name = o.Name;
        Debug.Assert(name != null && o.Roots.Length > 0);

        // preload textures
        var textureManager = materialManager.TextureManager;
        foreach (var texturePath in o.GetTexturePaths()) textureManager.Preload(source, texturePath);

        // NIF files can have any number of root NiObjects.
        // If there is only one root, instantiate that directly.
        // If there are multiple roots, create a container Node and parent it to the roots.
        if (o.Roots.Length == 1) {
            var rootNiObject = o.Roots[0].Value;
            var gobj = await InstantiateRootNiObject(name, source, isStatic, materialManager, rootNiObject);
            // If the file doesn't contain any NiObjects we are looking for, return an empty Node.
            if (gobj == null) {
                Log.Info($"{name} resulted in a null Node when instantiated.");
                gobj = new Node(name);
            }
            // If gobj != null and the root NiObject is an NiNode, discard any transformations (Morrowind apparently does).
            else if (rootNiObject is NiNode) gobj.Transform.Reset();
            return gobj;
        }
        else {
            Log.Info($"{name} has multiple roots.");
            var gobj = new Node(name);
            foreach (var rootRef in o.Roots) {
                var child = await InstantiateRootNiObject(name, source, isStatic, materialManager, rootRef.Value);
                if (child != null) gobj.AddChild(child);
            }
            return gobj;
        }
    }

    static void ApplyNiAVObject(Node obj, NiAVObject s) {
        obj.Transform.Position = s.Translation / MeterInUnits;
        obj.Transform.Rotation = s.Rotation.ToMirageRotation();
        obj.Transform.Scale = new Vector3(s.Scale);
    }

    static async Task<Node> InstantiateRootNiObject(string name, ISource source, bool isStatic, MaterialManager<Material, Texture> materialManager, NiObject s) {
        var gobj = await InstantiateNiObject(source, isStatic, materialManager, s);
        if (gobj == null) return null;
        var (shouldAddMissingColliders, isMarker) = ProcessExtraData(s);
        if (name != null && IsMarkerFileName(name)) { shouldAddMissingColliders = false; isMarker = true; }
        // Add colliders to the object if it doesn't already contain one.
        if (shouldAddMissingColliders && gobj.GetComponentInChildren<Collider>() == null && isStatic) gobj.AddMissingMeshCollidersRecursively();
        if (isMarker) gobj.SetLayerRecursively(MarkerLayer);
        return gobj;
    }

    static (bool shouldAddMissingColliders, bool isMarker) ProcessExtraData(NiObject s) {
        bool shouldAddMissingColliders = true, isMarker = false;
        if (s is NiObjectNET objNET && objNET.ExtraData != null) {
            var extraData = objNET.ExtraData.Value;
            while (extraData != null) {
                if (extraData is NiStringExtraData z) {
                    if (z.StringData == "NCO" || z.StringData == "NCC") shouldAddMissingColliders = false;
                    else if (z.StringData == "MRK") { shouldAddMissingColliders = false; isMarker = true; }
                }
                extraData = extraData.NextExtraData?.Value;
            }
        }
        return (shouldAddMissingColliders, isMarker);
    }

    /// <summary>
    /// Creates a Node representation of an NiObject.
    /// </summary>
    /// <returns>Returns the created Node, or null if the NiObject does not need its own Node.</returns>
    static Task<Node> InstantiateNiObject(ISource source, bool isStatic, MaterialManager<Material, Texture> materialManager, NiObject s)
        => s switch {
            NiTriShape z => InstantiateNiTriShape(source, isStatic, materialManager, z, true, false),
            RootCollisionNode z => InstantiateRcnNode(source, isStatic, materialManager, z),
            NiNode z => InstantiateNiNode(source, isStatic, materialManager, z),
            NiTextureEffect _ => Task.FromResult<Node>(null),
            NiRotatingParticles _ => Task.FromResult<Node>(null),
            NiAutoNormalParticles _ => Task.FromResult<Node>(null),
            _ => throw new NotImplementedException($"Tried to instantiate an unsupported NiObject ({s.GetType().Name})."),
        };

    static async Task<Node> InstantiateNiNode(ISource source, bool isStatic, MaterialManager<Material, Texture> materialManager, NiNode s) {
        var obj = new Node(s.Name);
        foreach (var t in s.Children)
            if (t != null) {
                var child = await InstantiateNiObject(source, isStatic, materialManager, t.Value);
                if (child != null) obj.AddChild(child);
            }
        ApplyNiAVObject(obj, s);
        return obj;
    }

    static async Task<Node> InstantiateRcnNode(ISource source, bool isStatic, MaterialManager<Material, Texture> materialManager, RootCollisionNode s) {
        var obj = new Node($"Root Collision Node{s.Name}");
        foreach (var t in s.Children)
            if (t != null)
                switch (t.Value) {
                    case NiTriShape z: obj.AddChild(await InstantiateNiTriShape(source, isStatic, materialManager, z, false, true)); break;
                    case AvoidNode: break;
                    default: Log.Info($"Unsupported collider NiObject: {t.Value.GetType().Name}"); break;
                }
        ApplyNiAVObject(obj, s);
        return obj;
    }

    static async Task<Node> InstantiateNiTriShape(ISource source, bool isStatic, MaterialManager<Material, Texture> materialManager, NiTriShape s, bool visual, bool collidable) {
        var mesh = ToGeometry((NiTriShapeData)s.Data.Value, s.Name);
        var obj = new Node(s.Name);
        if (visual) {
            var materialProps = ToMaterialProp(s);
            var meshRenderer = obj.AddComponent(new MeshRenderer { Mesh = mesh });
            (meshRenderer.Material, _) = await materialManager.Create(source, materialProps);
            if (materialProps.Textures == null || s.Flags.HasFlag(Flags.Hidden)) meshRenderer.Enabled = false;
            obj.IsStatic = isStatic;
        }
        else if (collidable) {
            if (!isStatic) {
                obj.AddComponent<BoxCollider>();
                obj.AddComponent(new Rigidbody { IsKinematic = KinematicRigidbody });
            }
            else obj.AddComponent(new MeshCollider { Mesh = mesh });
        }
        ApplyNiAVObject(obj, s);
        return obj;
    }

    static Mesh ToGeometry(NiTriShapeData s, string name) {
        var length = s.Vertices.Length;
        // vertex positions
        var vertices = new Vector3[length];
        for (var i = 0; i < vertices.Length; i++) vertices[i] = s.Vertices[i] / MeterInUnits;
        // vertex normals
        Vector3[] normals = null;
        if (s.Normals != null) {
            normals = new Vector3[length];
            for (var i = 0; i < normals.Length; i++) normals[i] = s.Normals[i];
        }
        // vertex UV coordinates
        Vector2[] UVs = null;
        if (s.UVSets != null && s.UVSets.Length > 0) {
            var vals = s.UVSets[0];
            UVs = new Vector2[length];
            for (var i = 0; i < UVs.Length; i++) { ref var z = ref vals[i]; UVs[i] = new Vector2(z.u, z.v); }
        }
        // triangle vertex indices
        var triangles = new int[s.NumTrianglePoints];
        for (var i = 0; i < s.Triangles.Length; i++) {
            ref var z = ref s.Triangles[i];
            var baseI = 3 * i; triangles[baseI] = z.v1; triangles[baseI + 1] = z.v3; triangles[baseI + 2] = z.v2; // Reverse triangle winding order.
        }

        // create the mesh.
        var mesh = MirageEngine.Tree.Resources.Add(new Mesh { Name = string.IsNullOrEmpty(name) ? $"mesh#{s.GetHashCode():x}" : name, Vertices = vertices, Normals = normals, UVs = UVs, Triangles = triangles });
        if (s.Normals == null) mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
