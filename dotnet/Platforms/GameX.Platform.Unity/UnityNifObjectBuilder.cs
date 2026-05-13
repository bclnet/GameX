using GameX.Gamebryo.Formats;
using GameX.Gamebryo.Formats.Nif;
using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Unity;
using System;
using UnityEngine;
using static GameX.Gamebryo.Platforms.NifObjectBuilder;
using Object = UnityEngine.GameObject;

namespace GameX.Platforms.Unity;

public static class UnityNifObjectBuilder {
    public static Object BuildObject(Binary_Nif src, bool isStatic, MaterialManager<Material, Texture2D> materialManager) {
        Log.Assert(src.Name != null && src.Roots.Length > 0);

        // preload textures
        var textureManager = materialManager.TextureManager;
        foreach (var texturePath in src.GetTexturePaths()) textureManager.PreloadTexture(texturePath);

        // NIF files can have any number of root NiObjects.
        // If there is only one root, instantiate that directly.
        // If there are multiple roots, create a container Object and parent it to the roots.
        if (src.Roots.Length == 1) {
            var s = src.Roots[0].Value;
            var gobj = InstantiateRootNiObject(src, isStatic, materialManager, s);
            // If the file doesn't contain any NiObjects we are looking for, return an empty Object.
            if (gobj == null) {
                Log.Info($"{src.Name} resulted in a null Object when instantiated.");
                gobj = new Object(src.Name);
            }
            // If gobj != null and the root NiObject is an NiNode, discard any transformations (Morrowind apparently does).
            else if (s is NiNode) {
                gobj.transform.position = Vector3.zero;
                gobj.transform.rotation = Quaternion.identity;
                gobj.transform.localScale = Vector3.one;
            }
            return gobj;
        }
        else {
            Log.Info($"{src.Name} has multiple roots.");
            var gobj = new Object(src.Name);
            foreach (var s in src.Roots) {
                var child = InstantiateRootNiObject(src, isStatic, materialManager, s.Value);
                child?.transform.SetParent(gobj.transform, false);
            }
            return gobj;
        }
    }

    static void ApplyNiAVObject(Object obj, NiAVObject s) {
        obj.transform.position = s.Translation.ToUnity() / MeterInUnits;
        obj.transform.rotation = s.Rotation.ToUnityQuaternionAsRotation();
        obj.transform.localScale = s.Scale * Vector3.one;
    }

    static Object InstantiateRootNiObject(Binary_Nif src, bool isStatic, MaterialManager<Material, Texture2D> materialManager, NiObject s) {
        var gobj = InstantiateNiObject(isStatic, materialManager, s);
        var (shouldAddMissingColliders, isMarker) = ProcessExtraData(s);
        if (src.Name != null && IsMarkerFileName(src.Name)) { shouldAddMissingColliders = false; isMarker = true; }
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
    /// Creates a Object representation of an NiObject.
    /// </summary>
    /// <returns>Returns the created Object, or null if the NiObject does not need its own Object.</returns>
    static Object InstantiateNiObject(bool isStatic, MaterialManager<Material, Texture2D> materialManager, NiObject s)
        => s switch {
            NiTriShape z => InstantiateNiTriShape(isStatic, materialManager, z, true, false),
            RootCollisionNode z => InstantiateRcnNode(isStatic, materialManager, z),
            //NiBSAnimationNode z => InstantiateNiNode(isStatic, materialManager, z),
            NiNode z => InstantiateNiNode(isStatic, materialManager, z),
            NiTextureEffect _ => default,
            //NiBSParticleNode _ => default,
            NiRotatingParticles _ => default,
            NiAutoNormalParticles _ => default,
            //NiBillboardNode _ => default,
            _ => throw new NotImplementedException($"Tried to instantiate an unsupported NiObject ({s.GetType().Name}).")
        };

    static Object InstantiateNiNode(bool isStatic, MaterialManager<Material, Texture2D> materialManager, NiNode s) {
        var obj = new Object(s.Name);
        foreach (var t in s.Children)
            if (t != null) InstantiateNiObject(isStatic, materialManager, t.Value)?.transform.SetParent(obj.transform, false);
        ApplyNiAVObject(obj, s);
        return obj;
    }

    static Object InstantiateRcnNode(bool isStatic, MaterialManager<Material, Texture2D> materialManager, RootCollisionNode s) {
        var obj = new Object($"Root Collision Node{s.Name}");
        foreach (var t in s.Children)
            if (t != null)
                switch (t.Value) {
                    case NiTriShape z: InstantiateNiTriShape(isStatic, materialManager, z, false, true).transform.SetParent(obj.transform, false); break;
                    case AvoidNode: break;
                    default: Log.Info($"Unsupported collider NiObject: {t.Value.GetType().Name}"); break;
                }
        ApplyNiAVObject(obj, s);
        return obj;
    }

    //void AddMeshRenderer(Object obj, Mesh mesh, Material material, bool enabled) {
    //    obj.AddComponent<MeshFilter>().mesh = mesh;
    //    var meshRenderer = obj.AddComponent<MeshRenderer>();
    //    meshRenderer.sharedMaterial = material;
    //    meshRenderer.enabled = enabled;
    //    obj.isStatic = IsStatic;
    //}

    //void AddSkinnedMeshRenderer(Object obj, Mesh mesh, Material material, bool enabled) {
    //    var skin = obj.AddComponent<SkinnedMeshRenderer>();
    //    skin.sharedMesh = mesh;
    //    skin.bones = null;
    //    skin.rootBone = null;
    //    skin.sharedMaterial = material;
    //    skin.enabled = enabled;
    //    obj.isStatic = IsStatic;
    //}

    static Object InstantiateNiTriShape(bool isStatic, MaterialManager<Material, Texture2D> materialManager, NiTriShape s, bool visual, bool collidable) {
        var mesh = ToGeometry((NiTriShapeData)s.Data.Value);
        var obj = new Object(s.Name);
        if (visual) {
            obj.AddComponent<MeshFilter>().mesh = mesh;
            var materialProps = ToMaterialProp(s);
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = materialManager.CreateMaterial(materialProps).mat;
            if (materialProps.Textures == null || s.Flags.HasFlag(Flags.Hidden)) meshRenderer.enabled = false;
            obj.isStatic = isStatic;
        }
        else if (collidable) {
            if (!isStatic) {
                obj.AddComponent<BoxCollider>();
                obj.AddComponent<Rigidbody>().isKinematic = KinematicRigidbody;
            }
            else obj.AddComponent<MeshCollider>().sharedMesh = mesh;
        }
        ApplyNiAVObject(obj, s);
        return obj;
    }

    static Mesh ToGeometry(NiTriShapeData s) {
        var length = s.Vertices.Length;
        // vertex positions
        var vertices = new Vector3[length];
        for (var i = 0; i < vertices.Length; i++) vertices[i] = s.Vertices[i].ToUnity() / MeterInUnits;
        // vertex normals
        Vector3[] normals = null;
        if (s.Normals != null) {
            normals = new Vector3[length];
            for (var i = 0; i < normals.Length; i++) normals[i] = s.Normals[i].ToUnity();
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
        var mesh = new Mesh { vertices = vertices, normals = normals, uv = UVs, triangles = triangles };
        if (s.Normals == null) mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
