﻿using GameX.Bethesda.Formats;
using OpenStack.Gfx;
using System;
using System.Drawing.Printing;
using UnityEngine;
using UnityEngine.Diagnostics;
using static OpenStack.Debug;

namespace GameX.Platforms.Unity;

public class NifObjectBuilder(NiFile file, MaterialManager<Material, Texture> materialManager, bool isStatic)
{
    const int MarkerLayer = 0;
    const bool KinematicRigidbody = false;

    readonly NiFile _file = file;
    readonly MaterialManager<Material, Texture> _materialManager = materialManager;
    readonly bool _isStatic = isStatic;

    public GameObject BuildObject()
    {
        Debug.Assert(_file.Name != null && _file.Footer.Roots.Length > 0);

        // NIF files can have any number of root NiObjects.
        // If there is only one root, instantiate that directly.
        // If there are multiple roots, create a container GameObject and parent it to the roots.
        if (_file.Footer.Roots.Length == 1)
        {
            var rootNiObject = _file.Blocks[_file.Footer.Roots[0]];
            var gameObject = InstantiateRootNiObject(rootNiObject);
            // If the file doesn't contain any NiObjects we are looking for, return an empty GameObject.
            if (gameObject == null)
            {
                Log($"{_file.Name} resulted in a null GameObject when instantiated.");
                gameObject = new GameObject(_file.Name);
            }
            // If gameObject != null and the root NiObject is an NiNode, discard any transformations (Morrowind apparently does).
            else if (rootNiObject is NiNode)
            {
                gameObject.transform.position = Vector3.zero;
                gameObject.transform.rotation = Quaternion.identity;
                gameObject.transform.localScale = Vector3.one;
            }
            return gameObject;
        }
        else
        {
            Log(_file.Name + " has multiple roots.");
            var gameObject = new GameObject(_file.Name);
            foreach (var rootRef in _file.Footer.Roots)
            {
                var child = InstantiateRootNiObject(_file.Blocks[rootRef]);
                child?.transform.SetParent(gameObject.transform, false);
            }
            return gameObject;
        }
    }

    GameObject InstantiateRootNiObject(NiObject obj)
    {
        var gameObject = InstantiateNiObject(obj);
        ProcessExtraData(obj, out var shouldAddMissingColliders, out var isMarker);
        if (_file.Name != null && IsMarkerFileName(_file.Name)) { shouldAddMissingColliders = false; isMarker = true; }
        // Add colliders to the object if it doesn't already contain one.
        if (shouldAddMissingColliders && gameObject.GetComponentInChildren<Collider>() == null) GameObjectUtils.AddMissingMeshCollidersRecursively(gameObject, _isStatic);
        if (isMarker) GameObjectUtils.SetLayerRecursively(gameObject, MarkerLayer);
        return gameObject;
    }

    void ProcessExtraData(NiObject obj, out bool shouldAddMissingColliders, out bool isMarker)
    {
        shouldAddMissingColliders = true; isMarker = false;
        if (obj is NiObjectNET objNET)
        {
            var extraData = objNET.ExtraData.Value >= 0 ? (NiExtraData)_file.Blocks[objNET.ExtraData.Value] : null;
            while (extraData != null)
            {
                if (extraData is NiStringExtraData strExtraData)
                {
                    if (strExtraData.Str == "NCO" || strExtraData.Str == "NCC") shouldAddMissingColliders = false;
                    else if (strExtraData.Str == "MRK") { shouldAddMissingColliders = false; isMarker = true; }
                }
                // Move to the next NiExtraData.
                extraData = extraData.NextExtraData.Value >= 0 ? (NiExtraData)_file.Blocks[extraData.NextExtraData.Value] : default;
            }
        }
    }

    /// <summary>
    /// Creates a GameObject representation of an NiObject.
    /// </summary>
    /// <returns>Returns the created GameObject, or null if the NiObject does not need its own GameObject.</returns>
    GameObject InstantiateNiObject(NiObject obj)
    {
        if (obj.GetType() == typeof(NiNode)) return InstantiateNiNode((NiNode)obj);
        else if (obj.GetType() == typeof(NiBSAnimationNode)) return InstantiateNiNode((NiNode)obj);
        else if (obj.GetType() == typeof(NiTriShape)) return InstantiateNiTriShape((NiTriShape)obj, true, false);
        else if (obj.GetType() == typeof(RootCollisionNode)) return InstantiateRootCollisionNode((RootCollisionNode)obj);
        else if (obj.GetType() == typeof(NiTextureEffect)) return null;
        else if (obj.GetType() == typeof(NiBSAnimationNode)) return null;
        else if (obj.GetType() == typeof(NiBSParticleNode)) return null;
        else if (obj.GetType() == typeof(NiRotatingParticles)) return null;
        else if (obj.GetType() == typeof(NiAutoNormalParticles)) return null;
        else if (obj.GetType() == typeof(NiBillboardNode)) return null;
        else throw new NotImplementedException($"Tried to instantiate an unsupported NiObject ({obj.GetType().Name}).");
    }

    GameObject InstantiateNiNode(NiNode node)
    {
        var obj = new GameObject(node.Name);
        foreach (var childIndex in node.Children)
            // NiNodes can have child references < 0 meaning null.
            if (!childIndex.IsNull)
            {
                var child = InstantiateNiObject(_file.Blocks[childIndex.Value]);
                child?.transform.SetParent(obj.transform, false);
            }
        ApplyNiAVObject(node, obj);
        return obj;
    }

    void AddMeshRenderer(GameObject obj, Mesh mesh, Material material, bool enabled)
    {
        obj.AddComponent<MeshFilter>().mesh = mesh;
        var meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;
        meshRenderer.enabled = enabled;
        obj.isStatic = _isStatic;
    }

    void AddSkinnedMeshRenderer(GameObject obj, Mesh mesh, Material material, bool enabled)
    {
        var skin = obj.AddComponent<SkinnedMeshRenderer>();
        skin.sharedMesh = mesh;
        skin.bones = null;
        skin.rootBone = null;
        skin.sharedMaterial = material;
        skin.enabled = enabled;
        obj.isStatic = _isStatic;
    }

    GameObject InstantiateNiTriShape(NiTriShape triShape, bool visual, bool collidable)
    {
        //var game = TesSettings.Game;
        Debug.Assert(visual || collidable);
        var mesh = NiTriShapeDataToMesh((NiTriShapeData)_file.Blocks[triShape.Data.Value]);
        var obj = new GameObject(triShape.Name);
        if (visual)
        {
            obj.AddComponent<MeshFilter>().mesh = mesh;
            var materialProps = NiAVObjectPropertiesToMaterialProperties(triShape);
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = _materialManager.BuildMaterialFromProperties(materialProps);
            if (materialProps.textures.mainFilePath == null || triShape.Flags.HasFlag(NiAVObject.NiFlags.Hidden)) meshRenderer.enabled = false;
            obj.isStatic = true;
        }
        if (collidable)
        {
            if (!_isStatic)
            {
                var collider = obj.AddComponent<BoxCollider>();
                var rb = obj.AddComponent<Rigidbody>().isKinematic = KinematicRigidbody;
            }
            else obj.AddComponent<MeshCollider>().sharedMesh = mesh;
        }
        ApplyNiAVObject(triShape, obj);
        return obj;
    }

    GameObject InstantiateRootCollisionNode(RootCollisionNode collisionNode)
    {
        var obj = new GameObject("Root Collision Node");
        foreach (var childIndex in collisionNode.Children)
            // NiNodes can have child references < 0 meaning null.
            if (!childIndex.IsNull) AddColliderFromNiObject(_file.Blocks[childIndex.Value], obj);
        ApplyNiAVObject(collisionNode, obj);
        return obj;
    }

    void ApplyNiAVObject(NiAVObject niAVObject, GameObject obj)
    {
        obj.transform.position = NifUtils.NifPointToUnityPoint(niAVObject.Translation);
        obj.transform.rotation = NifUtils.NifRotationMatrixToUnityQuaternion(niAVObject.Rotation);
        obj.transform.localScale = niAVObject.Scale * Vector3.one;
    }

    Mesh NiTriShapeDataToMesh(NiTriShapeData data)
    {
        // vertex positions
        var vertices = new Vector3[data.Vertices.Length];
        for (var i = 0; i < vertices.Length; i++) vertices[i] = NifUtils.NifPointToUnityPoint(data.Vertices[i]);
        // vertex normals
        Vector3[] normals = null;
        if (data.HasNormals)
        {
            normals = new Vector3[vertices.Length];
            for (var i = 0; i < normals.Length; i++) normals[i] = NifUtils.NifVectorToUnityVector(data.Normals[i]);
        }
        // vertex UV coordinates
        Vector2[] UVs = null;
        if (data.HasUV)
        {
            UVs = new Vector2[vertices.Length];
            for (var i = 0; i < UVs.Length; i++)
            {
                var NiTexCoord = data.UVSets[0, i];
                UVs[i] = new Vector2(NiTexCoord.u, NiTexCoord.v);
            }
        }
        // triangle vertex indices
        var triangles = new int[data.NumTrianglePoints];
        for (var i = 0; i < data.Triangles.Length; i++)
        {
            var baseI = 3 * i;
            // Reverse triangle winding order.
            triangles[baseI] = data.Triangles[i].v1;
            triangles[baseI + 1] = data.Triangles[i].v3;
            triangles[baseI + 2] = data.Triangles[i].v2;
        }

        // Create the mesh.
        var mesh = new Mesh
        {
            vertices = vertices,
            normals = normals,
            uv = UVs,
            triangles = triangles
        };
        if (!data.HasNormals) mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    MaterialProps NiAVObjectPropertiesToMaterialProperties(NiAVObject obj)
    {
        // Find relevant properties.
        NiTexturingProperty texturingProperty = null;
        NiMaterialProperty materialProperty = null;
        NiAlphaProperty alphaProperty = null;


        foreach (var propRef in obj.Properties)
        {
            var prop = _file.Blocks[propRef.Value];
            if (prop is NiTexturingProperty tp) texturingProperty = tp;
            else if (prop is NiMaterialProperty mp2) materialProperty = mp2;
            else if (prop is NiAlphaProperty ap) alphaProperty = ap;
        }

        // Create the material properties.
        var mp = new MaterialProps();

        // apply alphaProperty
        if (alphaProperty != null)
        {
            #region AlphaProperty Cheat Sheet
            /*
            14 bits used:

            1 bit for alpha blend bool
            4 bits for src blend mode
            4 bits for dest blend mode
            1 bit for alpha test bool
            3 bits for alpha test mode
            1 bit for zwrite bool ( opposite value )

            Bit 0 : alpha blending enable
            Bits 1-4 : source blend mode 
            Bits 5-8 : destination blend mode
            Bit 9 : alpha test enable
            Bit 10-12 : alpha test mode
            Bit 13 : no sorter flag ( disables triangle sorting ) ( Unity ZWrite )

            blend modes (glBlendFunc):
            0000 GL_ONE
            0001 GL_ZERO
            0010 GL_SRC_COLOR
            0011 GL_ONE_MINUS_SRC_COLOR
            0100 GL_DST_COLOR
            0101 GL_ONE_MINUS_DST_COLOR
            0110 GL_SRC_ALPHA
            0111 GL_ONE_MINUS_SRC_ALPHA
            1000 GL_DST_ALPHA
            1001 GL_ONE_MINUS_DST_ALPHA
            1010 GL_SRC_ALPHA_SATURATE

            test modes (glAlphaFunc):
            000 GL_ALWAYS
            001 GL_LESS
            010 GL_EQUAL
            011 GL_LEQUAL
            100 GL_GREATER
            101 GL_NOTEQUAL
            110 GL_GEQUAL
            111 GL_NEVER
            */
            #endregion

            var flags = alphaProperty.Flags;
            var oldflags = flags;
            var srcbm = (byte)(BitConverter.GetBytes(flags >> 1)[0] & 15);
            var dstbm = (byte)(BitConverter.GetBytes(flags >> 5)[0] & 15);
            mp.zWrite = BitConverter.GetBytes(flags >> 15)[0] == 1;//smush
            mp.alphaBlended = (flags & 0x01) != 0;
            mp.srcBlendMode = FigureBlendMode(srcbm);
            mp.dstBlendMode = FigureBlendMode(dstbm);
            mp.alphaTest = (flags & 0x100) != 0;
            mp.alphaCutoff = (float)alphaProperty.Threshold / 255;

            //mp.ZWrite = BitConverter.GetBytes(flags >> 15)[0] == 1; // smush
            //if ((flags & 0x01) != 0) // if flags contain the alpha blend flag at bit 0 in byte 0
            //{
            //    mp.AlphaBlended = true;
            //    mp.SrcBlendMode = FigureBlendMode(srcbm);
            //    mp.DstBlendMode = FigureBlendMode(dstbm);
            //}
            //else if ((flags & 0x100) != 0) // if flags contain the alpha test flag
            //{
            //    mp.AlphaTest = true;
            //    mp.AlphaCutoff = (float)alphaProperty.Threshold / 255;
            //}
        }
        else
        {
            mp.AlphaBlended = false;
            mp.AlphaTest = false;
        }

        // apply materialProperty
        if (materialProperty != null)
        {
            mp.alpha = materialProperty.Alpha;
            mp.diffuseColor = materialProperty.DiffuseColor.ToColor();
            mp.emissiveColor = materialProperty.EmissiveColor.ToColor();
            mp.specularColor = materialProperty.SpecularColor.ToColor();
            mp.glossiness = materialProperty.Glossiness;
        }

        // apply texturingProperty
        if (texturingProperty != null) mp.Textures = ConfigureTextureProperties(texturingProperty);
        return mp;
    }

    MaterialTextures ConfigureTextureProperties(NiTexturingProperty ntp)
    {
        var tp = new MaterialTextures();
        if (ntp.TextureCount < 1) return tp;
        if (ntp.BaseTexture != null) tp.MainFilePath = ((NiSourceTexture)_file.Blocks[ntp.BaseTexture.source.Value]).FileName;
        if (ntp.DarkTexture != null) tp.DarkFilePath = ((NiSourceTexture)_file.Blocks[ntp.DarkTexture.source.Value]).FileName;
        if (ntp.DetailTexture != null) tp.DetailFilePath = ((NiSourceTexture)_file.Blocks[ntp.DetailTexture.source.Value]).FileName;
        if (ntp.GlossTexture != null) tp.GlossFilePath = ((NiSourceTexture)_file.Blocks[ntp.GlossTexture.source.Value]).FileName;
        if (ntp.GlowTexture != null) tp.GlowFilePath = ((NiSourceTexture)_file.Blocks[ntp.GlowTexture.source.Value]).FileName;
        if (ntp.BumpMapTexture != null) tp.BumpFilePath = ((NiSourceTexture)_file.Blocks[ntp.BumpMapTexture.source.Value]).FileName;
        return tp;
    }

    UnityEngine.Rendering.BlendMode FigureBlendMode(byte b) => (UnityEngine.Rendering.BlendMode)Mathf.Min(b, 10);

    MatTestMode FigureTestMode(byte b) => (MatTestMode)Mathf.Min(b, 7);

    void AddColliderFromNiObject(NiObject niObject, GameObject gameObject)
    {
        if (niObject.GetType() == typeof(NiTriShape)) InstantiateNiTriShape((NiTriShape)niObject, false, true).transform.SetParent(gameObject.transform, false);
        else if (niObject.GetType() == typeof(AvoidNode)) { }
        else Log($"Unsupported collider NiObject: {niObject.GetType().Name}");
    }

    bool IsMarkerFileName(string name) => name.ToLowerInvariant() switch
    {
        "marker_light" or "marker_north" or "marker_error" or "marker_arrow" or "editormarker" or "marker_creature" or
        "marker_travel" or "marker_temple" or "marker_prison" or "marker_radius" or "marker_divine" or "editormarker_box_01" => true,
        _ => false,
    };
}
