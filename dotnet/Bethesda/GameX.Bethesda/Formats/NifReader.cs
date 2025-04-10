﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using static OpenStack.Debug;

namespace GameX.Bethesda.Formats;

#region Enums

// texture enums
public enum ApplyMode : uint
{
    APPLY_REPLACE = 0,
    APPLY_DECAL = 1,
    APPLY_MODULATE = 2,
    APPLY_HILIGHT = 3,
    APPLY_HILIGHT2 = 4
}

public enum TexClampMode : uint
{
    CLAMP_S_CLAMP_T = 0,
    CLAMP_S_WRAP_T = 1,
    WRAP_S_CLAMP_T = 2,
    WRAP_S_WRAP_T = 3
}

public enum TexFilterMode : uint
{
    FILTER_NEAREST = 0,
    FILTER_BILERP = 1,
    FILTER_TRILERP = 2,
    FILTER_NEAREST_MIPNEAREST = 3,
    FILTER_NEAREST_MIPLERP = 4,
    FILTER_BILERP_MIPNEAREST = 5
}

public enum PixelLayout : uint
{
    PIX_LAY_PALETTISED = 0,
    PIX_LAY_HIGH_COLOR_16 = 1,
    PIX_LAY_TRUE_COLOR_32 = 2,
    PIX_LAY_COMPRESSED = 3,
    PIX_LAY_BUMPMAP = 4,
    PIX_LAY_PALETTISED_4 = 5,
    PIX_LAY_DEFAULT = 6
}

public enum MipMapFormat : uint
{
    MIP_FMT_NO = 0,
    MIP_FMT_YES = 1,
    MIP_FMT_DEFAULT = 2
}

public enum AlphaFormat : uint
{
    ALPHA_NONE = 0,
    ALPHA_BINARY = 1,
    ALPHA_SMOOTH = 2,
    ALPHA_DEFAULT = 3
}

// miscellaneous
public enum VertMode : uint
{
    VERT_MODE_SRC_IGNORE = 0,
    VERT_MODE_SRC_EMISSIVE = 1,
    VERT_MODE_SRC_AMB_DIF = 2
}

public enum LightMode : uint
{
    LIGHT_MODE_EMISSIVE = 0,
    LIGHT_MODE_EMI_AMB_DIF = 1
}

public enum KeyType : uint
{
    LINEAR_KEY = 1,
    QUADRATIC_KEY = 2,
    TBC_KEY = 3,
    XYZ_ROTATION_KEY = 4,
    CONST_KEY = 5
}

public enum EffectType : uint
{
    EFFECT_PROJECTED_LIGHT = 0,
    EFFECT_PROJECTED_SHADOW = 1,
    EFFECT_ENVIRONMENT_MAP = 2,
    EFFECT_FOG_MAP = 3
}

public enum CoordGenType : uint
{
    CG_WORLD_PARALLEL = 0,
    CG_WORLD_PERSPECTIVE = 1,
    CG_SPHERE_MAP = 2,
    CG_SPECULAR_CUBE_MAP = 3,
    CG_DIFFUSE_CUBE_MAP = 4
}

public enum FieldType : uint
{
    FIELD_WIND = 0,
    FIELD_POINT = 1
}

public enum DecayType : uint
{
    DECAY_NONE = 0,
    DECAY_LINEAR = 1,
    DECAY_EXPONENTIAL = 2
}

#endregion

#region Structs

// Refers to an object before the current one in the hierarchy.
public struct Ptr<T>(BinaryReader r)
{
    public int Value = r.ReadInt32();
    public bool IsNull => Value < 0;
}

// Refers to an object after the current one in the hierarchy.
public struct Ref<T>(BinaryReader r)
{
    public int Value = r.ReadInt32();
    public bool IsNull => Value < 0;
}

#endregion

#region Misc Classes

public class BoundingBox(BinaryReader r)
{
    public uint unknownInt = r.ReadUInt32();
    public Vector3 translation = r.ReadVector3();
    public Matrix4x4 rotation = NiReaderUtils.Read3x3RotationMatrix(r);
    public Vector3 radius = r.ReadVector3();
}

public struct Color3(BinaryReader r)
{
    public float r = r.ReadSingle();
    public float g = r.ReadSingle();
    public float b = r.ReadSingle();

    //public Color ToColor() => new Color(r, g, b);
}

public struct Color4(BinaryReader r)
{
    public float r = r.ReadSingle();
    public float g = r.ReadSingle();
    public float b = r.ReadSingle();
    public float a = r.ReadSingle();
}

public class TexDesc(BinaryReader r)
{
    public Ref<NiSourceTexture> source = NiReaderUtils.ReadRef<NiSourceTexture>(r);
    public TexClampMode clampMode = (TexClampMode)r.ReadUInt32();
    public TexFilterMode filterMode = (TexFilterMode)r.ReadUInt32();
    public uint UVSet = r.ReadUInt32();
    public short PS2L = r.ReadInt16();
    public short PS2K = r.ReadInt16();
    public ushort unknown1 = r.ReadUInt16();
}

public class TexCoord(BinaryReader r)
{
    public float u = r.ReadSingle();
    public float v = r.ReadSingle();
}

public class Triangle(BinaryReader r)
{
    public ushort v1 = r.ReadUInt16();
    public ushort v2 = r.ReadUInt16();
    public ushort v3 = r.ReadUInt16();
}

public class MatchGroup
{
    public ushort numVertices;
    public ushort[] vertexIndices;

    public MatchGroup(BinaryReader r)
    {
        numVertices = r.ReadUInt16();
        vertexIndices = r.ReadPArray<ushort>("h", numVertices);
    }
}

public class TBC(BinaryReader r)
{
    public float t = r.ReadSingle();
    public float b = r.ReadSingle();
    public float c = r.ReadSingle();
}

public class Key<T>
{
    public float time;
    public T value;
    public T forward;
    public T backward;
    public TBC TBC;

    public Key(BinaryReader r, KeyType keyType)
    {
        time = r.ReadSingle();
        value = NiReaderUtils.Read<T>(r);
        if (keyType == KeyType.QUADRATIC_KEY) { forward = NiReaderUtils.Read<T>(r); backward = NiReaderUtils.Read<T>(r); }
        else if (keyType == KeyType.TBC_KEY) TBC = new TBC(r);
    }
}
public class KeyGroup<T>
{
    public uint numKeys;
    public KeyType interpolation;
    public Key<T>[] keys;

    public KeyGroup(BinaryReader r)
    {
        numKeys = r.ReadUInt32();
        if (numKeys != 0) interpolation = (KeyType)r.ReadUInt32();
        keys = new Key<T>[numKeys];
        for (var i = 0; i < keys.Length; i++) keys[i] = new Key<T>(r, interpolation);
    }
}

public class QuatKey<T>
{
    public float time;
    public T value;
    public TBC TBC;

    public QuatKey(BinaryReader r, KeyType keyType)
    {
        time = r.ReadSingle();
        if (keyType != KeyType.XYZ_ROTATION_KEY) value = NiReaderUtils.Read<T>(r);
        if (keyType == KeyType.TBC_KEY) { TBC = new TBC(r); }
    }
}

public class SkinData
{
    public SkinTransform skinTransform;
    public Vector3 boundingSphereOffset;
    public float boundingSphereRadius;
    public ushort numVertices;
    public SkinWeight[] vertexWeights;

    public SkinData(BinaryReader r)
    {
        skinTransform = new SkinTransform(r);
        boundingSphereOffset = r.ReadVector3();
        boundingSphereRadius = r.ReadSingle();
        numVertices = r.ReadUInt16();
        vertexWeights = new SkinWeight[numVertices];
        for (var i = 0; i < vertexWeights.Length; i++) vertexWeights[i] = new SkinWeight(r);
    }
}

public class SkinWeight(BinaryReader r)
{
    public ushort index = r.ReadUInt16();
    public float weight = r.ReadSingle();
}

public class SkinTransform(BinaryReader r)
{
    public Matrix4x4 rotation = NiReaderUtils.Read3x3RotationMatrix(r);
    public Vector3 translation = r.ReadVector3();
    public float scale = r.ReadSingle();
}

public class Particle(BinaryReader r)
{
    public Vector3 velocity = r.ReadVector3();
    public Vector3 unknownVector = r.ReadVector3();
    public float lifetime = r.ReadSingle();
    public float lifespan = r.ReadSingle();
    public float timestamp = r.ReadSingle();
    public ushort unknownShort = r.ReadUInt16();
    public ushort vertexID = r.ReadUInt16();
}

public class Morph
{
    public uint numKeys;
    public KeyType interpolation;
    public Key<float>[] keys;
    public Vector3[] vectors;

    public Morph(BinaryReader r, uint numVertices)
    {
        numKeys = r.ReadUInt32();
        interpolation = (KeyType)r.ReadUInt32();
        keys = new Key<float>[numKeys];
        for (var i = 0; i < keys.Length; i++) keys[i] = new Key<float>(r, interpolation);
        vectors = new Vector3[numVertices];
        for (var i = 0; i < vectors.Length; i++) vectors[i] = r.ReadVector3();
    }
}

#endregion

public class NiHeader(BinaryReader r)
{
    public byte[] Str = r.ReadBytes(40); // 40 bytes (including \n)
    public uint Version = r.ReadUInt32();
    public uint NumBlocks = r.ReadUInt32();
}

public class NiFooter
{
    public uint NumRoots;
    public int[] Roots;

    public NiFooter(BinaryReader r)
    {
        NumRoots = r.ReadUInt32();
        Roots = r.ReadPArray<int>("i", (int)NumRoots);
    }
}

/// <summary>
/// These are the main units of data that NIF files are arranged in.
/// </summary>
#pragma warning disable CS9113 // Parameter is unread.
public abstract class NiObject(BinaryReader r) { }
#pragma warning restore CS9113 // Parameter is unread.

/// <summary>
/// An object that can be controlled by a controller.
/// </summary>
public abstract class NiObjectNET(BinaryReader r) : NiObject(r)
{
    public string Name = r.ReadL32Encoding();
    public Ref<NiExtraData> ExtraData = NiReaderUtils.ReadRef<NiExtraData>(r);
    public Ref<NiTimeController> Controller = NiReaderUtils.ReadRef<NiTimeController>(r);
}

public abstract class NiAVObject : NiObjectNET
{
    [Flags] public enum NiFlags : ushort { Hidden = 0x1 }

    public NiFlags Flags; //: ushort
    public Vector3 Translation;
    public Matrix4x4 Rotation;
    public float Scale;
    public Vector3 Velocity;
    //public uint numProperties;
    public Ref<NiProperty>[] Properties;
    public bool HasBoundingBox;
    public BoundingBox BoundingBox;

    public NiAVObject(BinaryReader r) : base(r)
    {
        Flags = NiReaderUtils.ReadFlags(r);
        Translation = r.ReadVector3();
        Rotation = NiReaderUtils.Read3x3RotationMatrix(r);
        Scale = r.ReadSingle();
        Velocity = r.ReadVector3();
        Properties = NiReaderUtils.ReadLengthPrefixedRefs32<NiProperty>(r);
        HasBoundingBox = r.ReadBool32();
        if (HasBoundingBox) BoundingBox = new BoundingBox(r);
    }
}

// Nodes
public class NiNode(BinaryReader r) : NiAVObject(r)
{
    //public uint numChildren;
    public Ref<NiAVObject>[] Children = NiReaderUtils.ReadLengthPrefixedRefs32<NiAVObject>(r);
    //public uint numEffects;
    public Ref<NiDynamicEffect>[] Effects = NiReaderUtils.ReadLengthPrefixedRefs32<NiDynamicEffect>(r);
}
public class RootCollisionNode(BinaryReader r) : NiNode(r) { }
public class NiBSAnimationNode(BinaryReader r) : NiNode(r) { }
public class NiBSParticleNode(BinaryReader r) : NiNode(r) { }
public class NiBillboardNode(BinaryReader r) : NiNode(r) { }
public class AvoidNode(BinaryReader r) : NiNode(r) { }

// Geometry
public abstract class NiGeometry(BinaryReader r) : NiAVObject(r)
{
    public Ref<NiGeometryData> Data = NiReaderUtils.ReadRef<NiGeometryData>(r);
    public Ref<NiSkinInstance> SkinInstance = NiReaderUtils.ReadRef<NiSkinInstance>(r);
}

public abstract class NiGeometryData : NiObject
{
    public ushort NumVertices;
    public bool HasVertices;
    public Vector3[] Vertices;
    public bool HasNormals;
    public Vector3[] Normals;
    public Vector3 Center;
    public float Radius;
    public bool HasVertexColors;
    public Color4[] VertexColors;
    public ushort NumUVSets;
    public bool HasUV;
    public TexCoord[,] UVSets;

    public NiGeometryData(BinaryReader r) : base(r)
    {
        NumVertices = r.ReadUInt16();
        HasVertices = r.ReadBool32();
        if (HasVertices)
        {
            Vertices = new Vector3[NumVertices];
            for (var i = 0; i < Vertices.Length; i++) Vertices[i] = r.ReadVector3();
        }
        HasNormals = r.ReadBool32();
        if (HasNormals)
        {
            Normals = new Vector3[NumVertices];
            for (var i = 0; i < Normals.Length; i++) Normals[i] = r.ReadVector3();
        }
        Center = r.ReadVector3();
        Radius = r.ReadSingle();
        HasVertexColors = r.ReadBool32();
        if (HasVertexColors)
        {
            VertexColors = new Color4[NumVertices];
            for (var i = 0; i < VertexColors.Length; i++) VertexColors[i] = new Color4(r);
        }
        NumUVSets = r.ReadUInt16();
        HasUV = r.ReadBool32();
        if (HasUV)
        {
            UVSets = new TexCoord[NumUVSets, NumVertices];
            for (var i = 0; i < NumUVSets; i++)
                for (var j = 0; j < NumVertices; j++) UVSets[i, j] = new TexCoord(r);
        }
    }
}

public abstract class NiTriBasedGeom(BinaryReader r) : NiGeometry(r) { }

public abstract class NiTriBasedGeomData(BinaryReader r) : NiGeometryData(r)
{
    public ushort NumTriangles = r.ReadUInt16();
}

public class NiTriShape(BinaryReader r) : NiTriBasedGeom(r) { }

public class NiTriShapeData : NiTriBasedGeomData
{
    public uint NumTrianglePoints;
    public Triangle[] Triangles;
    public ushort NumMatchGroups;
    public MatchGroup[] MatchGroups;

    public NiTriShapeData(BinaryReader r) : base(r)
    {
        NumTrianglePoints = r.ReadUInt32();
        Triangles = new Triangle[NumTriangles];
        for (var i = 0; i < Triangles.Length; i++) Triangles[i] = new Triangle(r);
        NumMatchGroups = r.ReadUInt16();
        MatchGroups = new MatchGroup[NumMatchGroups];
        for (var i = 0; i < MatchGroups.Length; i++) MatchGroups[i] = new MatchGroup(r);
    }
}

// Properties
public abstract class NiProperty(BinaryReader r) : NiObjectNET(r) { }

public class NiTexturingProperty(BinaryReader r) : NiProperty(r)
{
    public NiAVObject.NiFlags Flags = NiReaderUtils.ReadFlags(r);
    public ApplyMode ApplyMode = (ApplyMode)r.ReadUInt32();
    public uint TextureCount = r.ReadUInt32();
    //public bool HasBaseTexture;
    public TexDesc BaseTexture = r.ReadBool32() ? new TexDesc(r) : default;
    //public bool HasDarkTexture;
    public TexDesc DarkTexture = r.ReadBool32() ? new TexDesc(r) : default;
    //public bool HasDetailTexture;
    public TexDesc DetailTexture = r.ReadBool32() ? new TexDesc(r) : default;
    //public bool HasGlossTexture;
    public TexDesc GlossTexture = r.ReadBool32() ? new TexDesc(r) : default;
    //public bool HasGlowTexture;
    public TexDesc GlowTexture = r.ReadBool32() ? new TexDesc(r) : default;
    //public bool HasBumpMapTexture;
    public TexDesc BumpMapTexture = r.ReadBool32() ? new TexDesc(r) : default;
    //public bool HasDecal0Texture;
    public TexDesc Decal0Texture = r.ReadBool32() ? new TexDesc(r) : default;
}

public class NiAlphaProperty(BinaryReader r) : NiProperty(r)
{
    public ushort Flags = r.ReadUInt16();
    public byte Threshold = r.ReadByte();
}

public class NiZBufferProperty(BinaryReader r) : NiProperty(r)
{
    public ushort Flags = r.ReadUInt16();
}

public class NiVertexColorProperty(BinaryReader r) : NiProperty(r)
{
    public NiAVObject.NiFlags Flags = NiReaderUtils.ReadFlags(r);
    public VertMode VertexMode = (VertMode)r.ReadUInt32();
    public LightMode LightingMode = (LightMode)r.ReadUInt32();
}

public class NiShadeProperty(BinaryReader r) : NiProperty(r)
{
    public NiAVObject.NiFlags Flags = NiReaderUtils.ReadFlags(r);
}

public class NiWireframeProperty(BinaryReader r) : NiProperty(r)
{
    public NiAVObject.NiFlags flags = NiReaderUtils.ReadFlags(r);
}

public class NiCamera(BinaryReader r) : NiAVObject(r) { }

// Data
public class NiUVData : NiObject
{
    public KeyGroup<float>[] UVGroups;

    public NiUVData(BinaryReader r) : base(r)
    {
        UVGroups = new KeyGroup<float>[4];
        for (var i = 0; i < UVGroups.Length; i++) UVGroups[i] = new KeyGroup<float>(r);
    }
}

public class NiKeyframeData : NiObject
{
    public uint NumRotationKeys;
    public KeyType RotationType;
    public QuatKey<Quaternion>[] QuaternionKeys;
    public float UnknownFloat;
    public KeyGroup<float>[] XYZRotations;
    public KeyGroup<Vector3> Translations;
    public KeyGroup<float> Scales;

    public NiKeyframeData(BinaryReader r) : base(r)
    {
        NumRotationKeys = r.ReadUInt32();
        if (NumRotationKeys != 0)
        {
            RotationType = (KeyType)r.ReadUInt32();
            if (RotationType != KeyType.XYZ_ROTATION_KEY)
            {
                QuaternionKeys = new QuatKey<Quaternion>[NumRotationKeys];
                for (var i = 0; i < QuaternionKeys.Length; i++) QuaternionKeys[i] = new QuatKey<Quaternion>(r, RotationType);
            }
            else
            {
                UnknownFloat = r.ReadSingle();
                XYZRotations = new KeyGroup<float>[3];
                for (var i = 0; i < XYZRotations.Length; i++) XYZRotations[i] = new KeyGroup<float>(r);
            }
        }
        Translations = new KeyGroup<Vector3>(r);
        Scales = new KeyGroup<float>(r);
    }
}

public class NiColorData(BinaryReader r) : NiObject(r)
{
    public KeyGroup<Color4> Data = new KeyGroup<Color4>(r);
}

public class NiMorphData : NiObject
{
    public uint NumMorphs;
    public uint NumVertices;
    public byte RelativeTargets;
    public Morph[] Morphs;

    public NiMorphData(BinaryReader r) : base(r)
    {
        NumMorphs = r.ReadUInt32();
        NumVertices = r.ReadUInt32();
        RelativeTargets = r.ReadByte();
        Morphs = new Morph[NumMorphs];
        for (var i = 0; i < Morphs.Length; i++) Morphs[i] = new Morph(r, NumVertices);
    }
}

public class NiVisData : NiObject
{
    public uint NumKeys;
    public Key<byte>[] Keys;

    public NiVisData(BinaryReader r) : base(r)
    {
        NumKeys = r.ReadUInt32();
        Keys = new Key<byte>[NumKeys];
        for (var i = 0; i < Keys.Length; i++) Keys[i] = new Key<byte>(r, KeyType.LINEAR_KEY);
    }
}

public class NiFloatData(BinaryReader r) : NiObject(r)
{
    public KeyGroup<float> Data = new KeyGroup<float>(r);
}

public class NiPosData(BinaryReader r) : NiObject(r)
{
    public KeyGroup<Vector3> Data = new KeyGroup<Vector3>(r);
}

public class NiExtraData(BinaryReader r) : NiObject(r)
{
    public Ref<NiExtraData> NextExtraData = NiReaderUtils.ReadRef<NiExtraData>(r);
}

public class NiStringExtraData(BinaryReader r) : NiExtraData(r)
{
    public uint BytesRemaining = r.ReadUInt32();
    public string Str = r.ReadL32Encoding();
}

public class NiTextKeyExtraData : NiExtraData
{
    public uint UnknownInt1;
    public uint NumTextKeys;
    public Key<string>[] TextKeys;

    public NiTextKeyExtraData(BinaryReader r) : base(r)
    {
        UnknownInt1 = r.ReadUInt32();
        NumTextKeys = r.ReadUInt32();
        TextKeys = new Key<string>[NumTextKeys];
        for (var i = 0; i < TextKeys.Length; i++) TextKeys[i] = new Key<string>(r, KeyType.LINEAR_KEY);
    }
}

public class NiVertWeightsExtraData : NiExtraData
{
    public uint NumBytes;
    public ushort NumVertices;
    public float[] Weights;

    public NiVertWeightsExtraData(BinaryReader r) : base(r)
    {
        NumBytes = r.ReadUInt32();
        NumVertices = r.ReadUInt16();
        Weights = new float[NumVertices];
        for (var i = 0; i < Weights.Length; i++) Weights[i] = r.ReadSingle();
    }
}

// Particles
public class NiParticles(BinaryReader r) : NiGeometry(r) { }
public class NiParticlesData : NiGeometryData
{
    public ushort NumParticles;
    public float ParticleRadius;
    public ushort NumActive;
    public bool HasSizes;
    public float[] Sizes;

    public NiParticlesData(BinaryReader r) : base(r)
    {
        NumParticles = r.ReadUInt16();
        ParticleRadius = r.ReadSingle();
        NumActive = r.ReadUInt16();
        HasSizes = r.ReadBool32();
        if (HasSizes)
        {
            Sizes = new float[NumVertices];
            for (var i = 0; i < Sizes.Length; i++) Sizes[i] = r.ReadSingle();
        }
    }
}

public class NiRotatingParticles(BinaryReader r) : NiParticles(r) { }
public class NiRotatingParticlesData : NiParticlesData
{
    public bool HasRotations;
    public Quaternion[] Rotations;

    public NiRotatingParticlesData(BinaryReader r) : base(r)
    {
        HasRotations = r.ReadBool32();
        if (HasRotations)
        {
            Rotations = new Quaternion[NumVertices];
            for (var i = 0; i < Rotations.Length; i++) Rotations[i] = r.ReadQuaternionWFirst();
        }
    }
}

public class NiAutoNormalParticles(BinaryReader r) : NiParticles(r) { }
public class NiAutoNormalParticlesData(BinaryReader r) : NiParticlesData(r) { }

public class NiParticleSystemController : NiTimeController
{
    public float Speed;
    public float SpeedRandom;
    public float VerticalDirection;
    public float VerticalAngle;
    public float HorizontalDirection;
    public float HorizontalAngle;
    public Vector3 UnknownNormal;
    public Color4 UnknownColor;
    public float Size;
    public float EmitStartTime;
    public float EmitStopTime;
    public byte UnknownByte;
    public float EmitRate;
    public float Lifetime;
    public float LifetimeRandom;
    public ushort EmitFlags;
    public Vector3 StartRandom;
    public Ptr<NiObject> Emitter;
    public ushort UnknownShort2;
    public float UnknownFloat13;
    public uint UnknownInt1;
    public uint UnknownInt2;
    public ushort UnknownShort3;
    public ushort NumParticles;
    public ushort NumValid;
    public Particle[] Particles;
    public Ref<NiObject> UnknownLink;
    public Ref<NiParticleModifier> ParticleExtra;
    public Ref<NiObject> UnknownLink2;
    public byte Trailer;

    public NiParticleSystemController(BinaryReader r) : base(r)
    {
        Speed = r.ReadSingle();
        SpeedRandom = r.ReadSingle();
        VerticalDirection = r.ReadSingle();
        VerticalAngle = r.ReadSingle();
        HorizontalDirection = r.ReadSingle();
        HorizontalAngle = r.ReadSingle();
        UnknownNormal = r.ReadVector3();
        UnknownColor = new Color4(r);
        Size = r.ReadSingle();
        EmitStartTime = r.ReadSingle();
        EmitStopTime = r.ReadSingle();
        UnknownByte = r.ReadByte();
        EmitRate = r.ReadSingle();
        Lifetime = r.ReadSingle();
        LifetimeRandom = r.ReadSingle();
        EmitFlags = r.ReadUInt16();
        StartRandom = r.ReadVector3();
        Emitter = NiReaderUtils.ReadPtr<NiObject>(r);
        UnknownShort2 = r.ReadUInt16();
        UnknownFloat13 = r.ReadSingle();
        UnknownInt1 = r.ReadUInt32();
        UnknownInt2 = r.ReadUInt32();
        UnknownShort3 = r.ReadUInt16();
        NumParticles = r.ReadUInt16();
        NumValid = r.ReadUInt16();
        Particles = new Particle[NumParticles];
        for (var i = 0; i < Particles.Length; i++) Particles[i] = new Particle(r);
        UnknownLink = NiReaderUtils.ReadRef<NiObject>(r);
        ParticleExtra = NiReaderUtils.ReadRef<NiParticleModifier>(r);
        UnknownLink2 = NiReaderUtils.ReadRef<NiObject>(r);
        Trailer = r.ReadByte();
    }
}

public class NiBSPArrayController(BinaryReader r) : NiParticleSystemController(r) { }

// Particle Modifiers
public abstract class NiParticleModifier(BinaryReader r) : NiObject(r)
{
    public Ref<NiParticleModifier> NextModifier = NiReaderUtils.ReadRef<NiParticleModifier>(r);
    public Ptr<NiParticleSystemController> Controller = NiReaderUtils.ReadPtr<NiParticleSystemController>(r);
}

public class NiGravity(BinaryReader r) : NiParticleModifier(r)
{
    public float UnknownFloat1 = r.ReadSingle();
    public float Force = r.ReadSingle();
    public FieldType Type = (FieldType)r.ReadUInt32();
    public Vector3 Position = r.ReadVector3();
    public Vector3 Direction = r.ReadVector3();
}

public class NiParticleBomb(BinaryReader r) : NiParticleModifier(r)
{
    public float Decay = r.ReadSingle();
    public float Duration = r.ReadSingle();
    public float DeltaV = r.ReadSingle();
    public float Start = r.ReadSingle();
    public DecayType DecayType = (DecayType)r.ReadUInt32();
    public Vector3 Position = r.ReadVector3();
    public Vector3 Direction = r.ReadVector3();
}

public class NiParticleColorModifier(BinaryReader r) : NiParticleModifier(r)
{
    public Ref<NiColorData> ColorData = NiReaderUtils.ReadRef<NiColorData>(r);
}

public class NiParticleGrowFade(BinaryReader r) : NiParticleModifier(r)
{
    public float Grow = r.ReadSingle();
    public float Fade = r.ReadSingle();
}

public class NiParticleMeshModifier : NiParticleModifier
{
    public uint NumParticleMeshes;
    public Ref<NiAVObject>[] ParticleMeshes;

    public NiParticleMeshModifier(BinaryReader r) : base(r)
    {
        NumParticleMeshes = r.ReadUInt32();
        ParticleMeshes = new Ref<NiAVObject>[NumParticleMeshes];
        for (var i = 0; i < ParticleMeshes.Length; i++) ParticleMeshes[i] = NiReaderUtils.ReadRef<NiAVObject>(r);
    }
}

public class NiParticleRotation(BinaryReader r) : NiParticleModifier(r)
{
    public byte RandomInitialAxis = r.ReadByte();
    public Vector3 InitialAxis = r.ReadVector3();
    public float RotationSpeed = r.ReadSingle();
}

// Controllers
public abstract class NiTimeController(BinaryReader r) : NiObject(r)
{
    public Ref<NiTimeController> NextController = NiReaderUtils.ReadRef<NiTimeController>(r);
    public ushort Flags = r.ReadUInt16();
    public float Frequency = r.ReadSingle();
    public float Phase = r.ReadSingle();
    public float StartTime = r.ReadSingle();
    public float StopTime = r.ReadSingle();
    public Ptr<NiObjectNET> Target = NiReaderUtils.ReadPtr<NiObjectNET>(r);
}

public class NiUVController(BinaryReader r) : NiTimeController(r)
{
    public ushort UnknownShort = r.ReadUInt16();
    public Ref<NiUVData> Data = NiReaderUtils.ReadRef<NiUVData>(r);
}

public abstract class NiInterpController(BinaryReader r) : NiTimeController(r) { }

public abstract class NiSingleInterpController(BinaryReader r) : NiInterpController(r) { }

public class NiKeyframeController(BinaryReader r) : NiSingleInterpController(r)
{
    public Ref<NiKeyframeData> Data = NiReaderUtils.ReadRef<NiKeyframeData>(r);
}

public class NiGeomMorpherController(BinaryReader r) : NiInterpController(r)
{
    public Ref<NiMorphData> Data = NiReaderUtils.ReadRef<NiMorphData>(r);
    public byte AlwaysUpdate = r.ReadByte();
}

public abstract class NiBoolInterpController(BinaryReader r) : NiSingleInterpController(r) { }

public class NiVisController(BinaryReader r) : NiBoolInterpController(r)
{
    public Ref<NiVisData> Data = NiReaderUtils.ReadRef<NiVisData>(r);
}

public abstract class NiFloatInterpController(BinaryReader r) : NiSingleInterpController(r) { }

public class NiAlphaController(BinaryReader r) : NiFloatInterpController(r)
{
    public Ref<NiFloatData> Data = NiReaderUtils.ReadRef<NiFloatData>(r);
}

// Skin Stuff
public class NiSkinInstance : NiObject
{
    public Ref<NiSkinData> Data;
    public Ptr<NiNode> SkeletonRoot;
    public uint NumBones;
    public Ptr<NiNode>[] Bones;

    public NiSkinInstance(BinaryReader r) : base(r)
    {
        Data = NiReaderUtils.ReadRef<NiSkinData>(r);
        SkeletonRoot = NiReaderUtils.ReadPtr<NiNode>(r);
        NumBones = r.ReadUInt32();
        Bones = new Ptr<NiNode>[NumBones];
        for (var i = 0; i < Bones.Length; i++) Bones[i] = NiReaderUtils.ReadPtr<NiNode>(r);
    }
}

public class NiSkinData : NiObject
{
    public SkinTransform SkinTransform;
    public uint NumBones;
    public Ref<NiSkinPartition> SkinPartition;
    public SkinData[] BoneList;

    public NiSkinData(BinaryReader r) : base(r)
    {
        SkinTransform = new SkinTransform(r);
        NumBones = r.ReadUInt32();
        SkinPartition = NiReaderUtils.ReadRef<NiSkinPartition>(r);
        BoneList = new SkinData[NumBones];
        for (var i = 0; i < BoneList.Length; i++) BoneList[i] = new SkinData(r);
    }
}

public class NiSkinPartition(BinaryReader r) : NiObject(r) { }

// Miscellaneous
public abstract class NiTexture(BinaryReader r) : NiObjectNET(r) { }

public class NiSourceTexture(BinaryReader r) : NiTexture(r)
{
    public byte UseExternal = r.ReadByte();
    public string FileName = r.ReadL32Encoding();
    public PixelLayout PixelLayout = (PixelLayout)r.ReadUInt32();
    public MipMapFormat UseMipMaps = (MipMapFormat)r.ReadUInt32();
    public AlphaFormat AlphaFormat = (AlphaFormat)r.ReadUInt32();
    public byte IsStatic = r.ReadByte();
}

public abstract class NiPoint3InterpController(BinaryReader r) : NiSingleInterpController(r)
{
    public Ref<NiPosData> Data = NiReaderUtils.ReadRef<NiPosData>(r);
}

public class NiMaterialProperty(BinaryReader r) : NiProperty(r)
{
    public NiAVObject.NiFlags Flags = NiReaderUtils.ReadFlags(r);
    public Color3 AmbientColor = new Color3(r);
    public Color3 DiffuseColor = new Color3(r);
    public Color3 SpecularColor = new Color3(r);
    public Color3 EmissiveColor = new Color3(r);
    public float Glossiness = r.ReadSingle();
    public float Alpha = r.ReadSingle();
}

public class NiMaterialColorController(BinaryReader r) : NiPoint3InterpController(r) { }

public abstract class NiDynamicEffect : NiAVObject
{
    uint NumAffectedNodeListPointers;
    uint[] AffectedNodeListPointers;

    public NiDynamicEffect(BinaryReader r) : base(r)
    {
        NumAffectedNodeListPointers = r.ReadUInt32();
        AffectedNodeListPointers = r.ReadPArray<uint>("I", (int)NumAffectedNodeListPointers);
    }
}

public class NiTextureEffect(BinaryReader r) : NiDynamicEffect(r)
{
    public Matrix4x4 ModelProjectionMatrix = NiReaderUtils.Read3x3RotationMatrix(r);
    public Vector3 ModelProjectionTransform = r.ReadVector3();
    public TexFilterMode TextureFiltering = (TexFilterMode)r.ReadUInt32();
    public TexClampMode TextureClamping = (TexClampMode)r.ReadUInt32();
    public EffectType TextureType = (EffectType)r.ReadUInt32();
    public CoordGenType CoordinateGenerationType = (CoordGenType)r.ReadUInt32();
    public Ref<NiSourceTexture> SourceTexture = NiReaderUtils.ReadRef<NiSourceTexture>(r);
    public byte ClippingPlane = r.ReadByte();
    public Vector3 UnknownVector = r.ReadVector3();
    public float UnknownFloat = r.ReadSingle();
    public short PS2L = r.ReadInt16();
    public short PS2K = r.ReadInt16();
    public ushort UnknownShort = r.ReadUInt16();
}

public class NiReaderUtils
{
    public static Ptr<T> ReadPtr<T>(BinaryReader r) => new Ptr<T>(r);

    public static Ref<T> ReadRef<T>(BinaryReader r) => new Ref<T>(r);

    public static Ref<T>[] ReadLengthPrefixedRefs32<T>(BinaryReader r)
    {
        var refs = new Ref<T>[r.ReadUInt32()];
        for (var i = 0; i < refs.Length; i++) refs[i] = ReadRef<T>(r);
        return refs;
    }

    public static NiAVObject.NiFlags ReadFlags(BinaryReader r) => (NiAVObject.NiFlags)r.ReadUInt16();

    public static T Read<T>(BinaryReader r)
    {
        if (typeof(T) == typeof(float)) { return (T)(object)r.ReadSingle(); }
        else if (typeof(T) == typeof(byte)) { return (T)(object)r.ReadByte(); }
        else if (typeof(T) == typeof(string)) { return (T)(object)r.ReadL32Encoding(); }
        else if (typeof(T) == typeof(Vector3)) { return (T)(object)r.ReadVector3(); }
        else if (typeof(T) == typeof(Quaternion)) { return (T)(object)r.ReadQuaternionWFirst(); }
        else if (typeof(T) == typeof(Color4)) { return (T)(object)new Color4(r); }
        else throw new NotImplementedException("Tried to read an unsupported type.");
    }

    public static NiObject ReadNiObject(BinaryReader r)
    {
        var nodeType = r.ReadL32AString();
        switch (nodeType)
        {
            case "NiNode": return new NiNode(r);
            case "NiTriShape": return new NiTriShape(r);
            case "NiTexturingProperty": return new NiTexturingProperty(r);
            case "NiSourceTexture": return new NiSourceTexture(r);
            case "NiMaterialProperty": return new NiMaterialProperty(r);
            case "NiMaterialColorController": return new NiMaterialColorController(r);
            case "NiTriShapeData": return new NiTriShapeData(r);
            case "RootCollisionNode": return new RootCollisionNode(r);
            case "NiStringExtraData": return new NiStringExtraData(r);
            case "NiSkinInstance": return new NiSkinInstance(r);
            case "NiSkinData": return new NiSkinData(r);
            case "NiAlphaProperty": return new NiAlphaProperty(r);
            case "NiZBufferProperty": return new NiZBufferProperty(r);
            case "NiVertexColorProperty": return new NiVertexColorProperty(r);
            case "NiBSAnimationNode": return new NiBSAnimationNode(r);
            case "NiBSParticleNode": return new NiBSParticleNode(r);
            case "NiParticles": return new NiParticles(r);
            case "NiParticlesData": return new NiParticlesData(r);
            case "NiRotatingParticles": return new NiRotatingParticles(r);
            case "NiRotatingParticlesData": return new NiRotatingParticlesData(r);
            case "NiAutoNormalParticles": return new NiAutoNormalParticles(r);
            case "NiAutoNormalParticlesData": return new NiAutoNormalParticlesData(r);
            case "NiUVController": return new NiUVController(r);
            case "NiUVData": return new NiUVData(r);
            case "NiTextureEffect": return new NiTextureEffect(r);
            case "NiTextKeyExtraData": return new NiTextKeyExtraData(r);
            case "NiVertWeightsExtraData": return new NiVertWeightsExtraData(r);
            case "NiParticleSystemController": return new NiParticleSystemController(r);
            case "NiBSPArrayController": return new NiBSPArrayController(r);
            case "NiGravity": return new NiGravity(r);
            case "NiParticleBomb": return new NiParticleBomb(r);
            case "NiParticleColorModifier": return new NiParticleColorModifier(r);
            case "NiParticleGrowFade": return new NiParticleGrowFade(r);
            case "NiParticleMeshModifier": return new NiParticleMeshModifier(r);
            case "NiParticleRotation": return new NiParticleRotation(r);
            case "NiKeyframeController": return new NiKeyframeController(r);
            case "NiKeyframeData": return new NiKeyframeData(r);
            case "NiColorData": return new NiColorData(r);
            case "NiGeomMorpherController": return new NiGeomMorpherController(r);
            case "NiMorphData": return new NiMorphData(r);
            case "AvoidNode": return new AvoidNode(r);
            case "NiVisController": return new NiVisController(r);
            case "NiVisData": return new NiVisData(r);
            case "NiAlphaController": return new NiAlphaController(r);
            case "NiFloatData": return new NiFloatData(r);
            case "NiPosData": return new NiPosData(r);
            case "NiBillboardNode": return new NiBillboardNode(r);
            case "NiShadeProperty": return new NiShadeProperty(r);
            case "NiWireframeProperty": return new NiWireframeProperty(r);
            case "NiCamera": return new NiCamera(r);
            default: { Log($"Tried to read an unsupported NiObject type ({nodeType})."); return null; }
        }
    }

    public static Matrix4x4 Read3x3RotationMatrix(BinaryReader r) => r.ReadRowMajorMatrix3x3();
}

public class NiFile(string name) : IHaveMetaInfo
{
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Engine", Name = Name, Value = this }),
        new("Nif", items: [
            new($"NumBlocks: {Header.NumBlocks}"),
        ]),
    ];

    public string Name = name;
    public NiHeader Header;
    public NiObject[] Blocks;
    public NiFooter Footer;

    public void Read(BinaryReader r)
    {
        Header = new NiHeader(r);
        Blocks = new NiObject[Header.NumBlocks];
        for (var i = 0; i < Header.NumBlocks; i++) Blocks[i] = NiReaderUtils.ReadNiObject(r);
        Footer = new NiFooter(r);
    }

    public bool IsSkinnedMesh() => Blocks.Any(b => b is NiSkinInstance);

    public IEnumerable<string> GetTexturePaths()
    {
        foreach (var niObject in Blocks)
            if (niObject is NiSourceTexture niSourceTexture && !string.IsNullOrEmpty(niSourceTexture.FileName))
                yield return niSourceTexture.FileName;
    }
}
