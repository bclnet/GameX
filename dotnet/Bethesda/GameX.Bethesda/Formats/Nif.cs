using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using static OpenStack.Debug;
#pragma warning disable CS9113 // Parameter is unread.

namespace GameX.Bethesda.Formats.Nif;

#region Enums

// texture enums
public enum ApplyMode : uint {
    APPLY_REPLACE = 0,
    APPLY_DECAL = 1,
    APPLY_MODULATE = 2,
    APPLY_HILIGHT = 3,
    APPLY_HILIGHT2 = 4
}

public enum TexClampMode : uint {
    CLAMP_S_CLAMP_T = 0,
    CLAMP_S_WRAP_T = 1,
    WRAP_S_CLAMP_T = 2,
    WRAP_S_WRAP_T = 3
}

public enum TexFilterMode : uint {
    FILTER_NEAREST = 0,
    FILTER_BILERP = 1,
    FILTER_TRILERP = 2,
    FILTER_NEAREST_MIPNEAREST = 3,
    FILTER_NEAREST_MIPLERP = 4,
    FILTER_BILERP_MIPNEAREST = 5
}

public enum PixelLayout : uint {
    PIX_LAY_PALETTISED = 0,
    PIX_LAY_HIGH_COLOR_16 = 1,
    PIX_LAY_TRUE_COLOR_32 = 2,
    PIX_LAY_COMPRESSED = 3,
    PIX_LAY_BUMPMAP = 4,
    PIX_LAY_PALETTISED_4 = 5,
    PIX_LAY_DEFAULT = 6
}

public enum MipMapFormat : uint {
    MIP_FMT_NO = 0,
    MIP_FMT_YES = 1,
    MIP_FMT_DEFAULT = 2
}

public enum AlphaFormat : uint {
    ALPHA_NONE = 0,
    ALPHA_BINARY = 1,
    ALPHA_SMOOTH = 2,
    ALPHA_DEFAULT = 3
}

// miscellaneous
public enum VertMode : uint {
    VERT_MODE_SRC_IGNORE = 0,
    VERT_MODE_SRC_EMISSIVE = 1,
    VERT_MODE_SRC_AMB_DIF = 2
}

public enum LightMode : uint {
    LIGHT_MODE_EMISSIVE = 0,
    LIGHT_MODE_EMI_AMB_DIF = 1
}

public enum KeyType : uint {
    LINEAR_KEY = 1,
    QUADRATIC_KEY = 2,
    TBC_KEY = 3,
    XYZ_ROTATION_KEY = 4,
    CONST_KEY = 5
}

public enum EffectType : uint {
    EFFECT_PROJECTED_LIGHT = 0,
    EFFECT_PROJECTED_SHADOW = 1,
    EFFECT_ENVIRONMENT_MAP = 2,
    EFFECT_FOG_MAP = 3
}

public enum CoordGenType : uint {
    CG_WORLD_PARALLEL = 0,
    CG_WORLD_PERSPECTIVE = 1,
    CG_SPHERE_MAP = 2,
    CG_SPECULAR_CUBE_MAP = 3,
    CG_DIFFUSE_CUBE_MAP = 4
}

public enum FieldType : uint {
    FIELD_WIND = 0,
    FIELD_POINT = 1
}

public enum DecayType : uint {
    DECAY_NONE = 0,
    DECAY_LINEAR = 1,
    DECAY_EXPONENTIAL = 2
}

#endregion

#region Records

// Refers to an object before the current one in the hierarchy.
//public struct Ptr<T> {
//    public Ptr(BinaryReader r) { int v; Value = (v = r.ReadInt32()) < 0 ? null : v; }
//    public int? Value;
//    //public int Value = r.ReadInt32();
//    //public bool IsNull => Value < 0;
//}

//// Refers to an object after the current one in the hierarchy.
//public struct Ref<T> {
//    public Ref(BinaryReader r) { int v; Value = (v = r.ReadInt32()) < 0 ? null : v; }
//    public int? Value;
//}

static class X {
    // Refers to an object before the current one in the hierarchy.
    public static int? Ptr<T>(BinaryReader r) { int v; return (v = r.ReadInt32()) < 0 ? null : v; }
    // Refers to an object after the current one in the hierarchy.
    public static int? Ref<T>(BinaryReader r) { int v; return (v = r.ReadInt32()) < 0 ? null : v; }
}

public class BoundingBox(BinaryReader r) {
    public uint UnknownInt = r.ReadUInt32();
    public Vector3 Translation = r.ReadVector3();
    public Matrix4x4 Rotation = r.ReadMatrix3x3As4x4();
    public Vector3 Radius = r.ReadVector3();
}

[JsonConverter(typeof(Color3JsonConverter))]
public struct Color3(BinaryReader r) {
    public float R = r.ReadSingle();
    public float G = r.ReadSingle();
    public float B = r.ReadSingle();
    public Color ToColor() => Color.FromArgb((int)(R * 255f), (int)(G * 255f), (int)(B * 255f));
}

public class Color3JsonConverter : JsonConverter<Color3> {
    public override Color3 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Color3 s, JsonSerializerOptions options) => w.WriteStringValue($"{s.R} {s.G} {s.B}");
}

public struct Color4(BinaryReader r) {
    public float R = r.ReadSingle();
    public float G = r.ReadSingle();
    public float B = r.ReadSingle();
    public float A = r.ReadSingle();
}

public class TexDesc(BinaryReader r) {
    public int? Source = X.Ref<NiSourceTexture>(r);
    public TexClampMode ClampMode = (TexClampMode)r.ReadUInt32();
    public TexFilterMode FilterMode = (TexFilterMode)r.ReadUInt32();
    public uint UVSet = r.ReadUInt32();
    public short PS2L = r.ReadInt16();
    public short PS2K = r.ReadInt16();
    public ushort unknown1 = r.ReadUInt16();
}

public class TexCoord(BinaryReader r) {
    public float U = r.ReadSingle();
    public float V = r.ReadSingle();
}

public class Triangle(BinaryReader r) {
    public ushort V1 = r.ReadUInt16();
    public ushort V2 = r.ReadUInt16();
    public ushort V3 = r.ReadUInt16();
}

public class MatchGroup(BinaryReader r) {
    //public ushort numVertices;
    public ushort[] VertexIndices = r.ReadL16PArray<ushort>("h");
}

public class TBC(BinaryReader r) {
    public float T = r.ReadSingle();
    public float B = r.ReadSingle();
    public float C = r.ReadSingle();
}

public class Key<T> {
    public float Time;
    public T Value;
    public T Forward;
    public T Backward;
    public TBC TBC;

    public Key(BinaryReader r, KeyType keyType) {
        Time = r.ReadSingle();
        Value = NiReaderUtils.Read<T>(r);
        if (keyType == KeyType.QUADRATIC_KEY) { Forward = NiReaderUtils.Read<T>(r); Backward = NiReaderUtils.Read<T>(r); }
        else if (keyType == KeyType.TBC_KEY) TBC = new TBC(r);
    }
}

public class KeyGroup<T> {
    public uint NumKeys;
    public KeyType Interpolation;
    public Key<T>[] Keys;

    public KeyGroup(BinaryReader r) {
        NumKeys = r.ReadUInt32();
        if (NumKeys != 0) Interpolation = (KeyType)r.ReadUInt32();
        Keys = r.ReadFArray(r => new Key<T>(r, Interpolation), (int)NumKeys);
    }
}

public class QuatKey<T> {
    public float Time;
    public T Value;
    public TBC TBC;

    public QuatKey(BinaryReader r, KeyType keyType) {
        Time = r.ReadSingle();
        if (keyType != KeyType.XYZ_ROTATION_KEY) Value = NiReaderUtils.Read<T>(r);
        if (keyType == KeyType.TBC_KEY) { TBC = new TBC(r); }
    }
}

public class SkinData(BinaryReader r) {
    public SkinTransform SkinTransform = new SkinTransform(r);
    public Vector3 BoundingSphereOffset = r.ReadVector3();
    public float BoundingSphereRadius = r.ReadSingle();
    //public ushort numVertices;
    public SkinWeight[] VertexWeights = r.ReadL16FArray(r => new SkinWeight(r));
}

public class SkinWeight(BinaryReader r) {
    public ushort Index = r.ReadUInt16();
    public float Weight = r.ReadSingle();
}

public class SkinTransform(BinaryReader r) {
    public Matrix4x4 Rotation = r.ReadMatrix3x3As4x4();
    public Vector3 Translation = r.ReadVector3();
    public float Scale = r.ReadSingle();
}

public class Particle(BinaryReader r) {
    public Vector3 Velocity = r.ReadVector3();
    public Vector3 UnknownVector = r.ReadVector3();
    public float Lifetime = r.ReadSingle();
    public float Lifespan = r.ReadSingle();
    public float Timestamp = r.ReadSingle();
    public ushort UnknownShort = r.ReadUInt16();
    public ushort VertexId = r.ReadUInt16();
}

public class Morph {
    public uint NumKeys;
    public KeyType Interpolation;
    public Key<float>[] Keys;
    public Vector3[] Vectors;

    public Morph(BinaryReader r, uint numVertices) {
        NumKeys = r.ReadUInt32();
        Interpolation = (KeyType)r.ReadUInt32();
        Keys = r.ReadFArray(r => new Key<float>(r, Interpolation), (int)NumKeys);
        Vectors = r.ReadFArray(r => r.ReadVector3(), (int)numVertices);
    }
}

#endregion

#region Headers

public class NiHeader(BinaryReader r) {
    public byte[] Str = r.ReadBytes(40); // 40 bytes (including \n)
    public uint Version = r.ReadUInt32();
    public uint NumBlocks = r.ReadUInt32();
}

public class NiFooter(BinaryReader r) {
    //public uint NumRoots;
    public int[] Roots = r.ReadL32PArray<int>("i");
}

/// <summary>
/// These are the main units of data that NIF files are arranged in.
/// </summary>
/// 
[JsonDerivedType(typeof(NiNode), typeDiscriminator: nameof(NiNode))]
[JsonDerivedType(typeof(NiTriShape), typeDiscriminator: nameof(NiTriShape))]
[JsonDerivedType(typeof(NiTexturingProperty), typeDiscriminator: nameof(NiTexturingProperty))]
[JsonDerivedType(typeof(NiSourceTexture), typeDiscriminator: nameof(NiSourceTexture))]
[JsonDerivedType(typeof(NiMaterialProperty), typeDiscriminator: nameof(NiMaterialProperty))]
[JsonDerivedType(typeof(NiMaterialColorController), typeDiscriminator: nameof(NiMaterialColorController))]
[JsonDerivedType(typeof(NiTriShapeData), typeDiscriminator: nameof(NiTriShapeData))]
[JsonDerivedType(typeof(RootCollisionNode), typeDiscriminator: nameof(RootCollisionNode))]
[JsonDerivedType(typeof(NiStringExtraData), typeDiscriminator: nameof(NiStringExtraData))]
[JsonDerivedType(typeof(NiSkinInstance), typeDiscriminator: nameof(NiSkinInstance))]
[JsonDerivedType(typeof(NiSkinData), typeDiscriminator: nameof(NiSkinData))]
[JsonDerivedType(typeof(NiAlphaProperty), typeDiscriminator: nameof(NiAlphaProperty))]
[JsonDerivedType(typeof(NiZBufferProperty), typeDiscriminator: nameof(NiZBufferProperty))]
[JsonDerivedType(typeof(NiVertexColorProperty), typeDiscriminator: nameof(NiVertexColorProperty))]
[JsonDerivedType(typeof(NiBSAnimationNode), typeDiscriminator: nameof(NiBSAnimationNode))]
[JsonDerivedType(typeof(NiBSParticleNode), typeDiscriminator: nameof(NiBSParticleNode))]
[JsonDerivedType(typeof(NiParticles), typeDiscriminator: nameof(NiParticles))]
[JsonDerivedType(typeof(NiParticlesData), typeDiscriminator: nameof(NiParticlesData))]
[JsonDerivedType(typeof(NiRotatingParticles), typeDiscriminator: nameof(NiRotatingParticles))]
[JsonDerivedType(typeof(NiRotatingParticlesData), typeDiscriminator: nameof(NiRotatingParticlesData))]
[JsonDerivedType(typeof(NiAutoNormalParticles), typeDiscriminator: nameof(NiAutoNormalParticles))]
[JsonDerivedType(typeof(NiAutoNormalParticlesData), typeDiscriminator: nameof(NiAutoNormalParticlesData))]
[JsonDerivedType(typeof(NiUVController), typeDiscriminator: nameof(NiUVController))]
[JsonDerivedType(typeof(NiUVData), typeDiscriminator: nameof(NiUVData))]
[JsonDerivedType(typeof(NiTextureEffect), typeDiscriminator: nameof(NiTextureEffect))]
[JsonDerivedType(typeof(NiTextKeyExtraData), typeDiscriminator: nameof(NiTextKeyExtraData))]
[JsonDerivedType(typeof(NiVertWeightsExtraData), typeDiscriminator: nameof(NiVertWeightsExtraData))]
[JsonDerivedType(typeof(NiParticleSystemController), typeDiscriminator: nameof(NiParticleSystemController))]
[JsonDerivedType(typeof(NiBSPArrayController), typeDiscriminator: nameof(NiBSPArrayController))]
[JsonDerivedType(typeof(NiGravity), typeDiscriminator: nameof(NiGravity))]
[JsonDerivedType(typeof(NiParticleBomb), typeDiscriminator: nameof(NiParticleBomb))]
[JsonDerivedType(typeof(NiParticleColorModifier), typeDiscriminator: nameof(NiParticleColorModifier))]
[JsonDerivedType(typeof(NiParticleGrowFade), typeDiscriminator: nameof(NiParticleGrowFade))]
[JsonDerivedType(typeof(NiParticleMeshModifier), typeDiscriminator: nameof(NiParticleMeshModifier))]
[JsonDerivedType(typeof(NiParticleRotation), typeDiscriminator: nameof(NiParticleRotation))]
[JsonDerivedType(typeof(NiKeyframeController), typeDiscriminator: nameof(NiKeyframeController))]
[JsonDerivedType(typeof(NiKeyframeData), typeDiscriminator: nameof(NiKeyframeData))]
[JsonDerivedType(typeof(NiColorData), typeDiscriminator: nameof(NiColorData))]
[JsonDerivedType(typeof(NiGeomMorpherController), typeDiscriminator: nameof(NiGeomMorpherController))]
[JsonDerivedType(typeof(NiMorphData), typeDiscriminator: nameof(NiMorphData))]
[JsonDerivedType(typeof(AvoidNode), typeDiscriminator: nameof(AvoidNode))]
[JsonDerivedType(typeof(NiVisController), typeDiscriminator: nameof(NiVisController))]
[JsonDerivedType(typeof(NiVisData), typeDiscriminator: nameof(NiVisData))]
[JsonDerivedType(typeof(NiAlphaController), typeDiscriminator: nameof(NiAlphaController))]
[JsonDerivedType(typeof(NiFloatData), typeDiscriminator: nameof(NiFloatData))]
[JsonDerivedType(typeof(NiPosData), typeDiscriminator: nameof(NiPosData))]
[JsonDerivedType(typeof(NiBillboardNode), typeDiscriminator: nameof(NiBillboardNode))]
[JsonDerivedType(typeof(NiShadeProperty), typeDiscriminator: nameof(NiShadeProperty))]
[JsonDerivedType(typeof(NiWireframeProperty), typeDiscriminator: nameof(NiWireframeProperty))]
[JsonDerivedType(typeof(NiCamera), typeDiscriminator: nameof(NiCamera))]
[JsonDerivedType(typeof(NiExtraData), typeDiscriminator: nameof(NiExtraData))]
[JsonDerivedType(typeof(NiSkinPartition), typeDiscriminator: nameof(NiSkinPartition))]
public abstract class NiObject(BinaryReader r) { }

/// <summary>
/// An object that can be controlled by a controller.
/// </summary>
public abstract class NiObjectNET : NiObject {
    public string Name;
    public int? ExtraData;
    public int? Controller;

    public NiObjectNET(BinaryReader r) : base(r) {
        Name = r.ReadL32Encoding();
        ExtraData = X.Ref<NiExtraData>(r);
        Controller = X.Ref<NiTimeController>(r);
    }
}

public abstract class NiAVObject : NiObjectNET {
    [Flags] public enum NiFlags : ushort { Hidden = 0x1 }

    public NiFlags Flags; //: ushort
    public Vector3 Translation;
    public Matrix4x4 Rotation;
    public float Scale;
    public Vector3 Velocity;
    //public uint NumProperties;
    public int?[] Properties;
    public bool HasBoundingBox;
    public BoundingBox BoundingBox;

    public NiAVObject(BinaryReader r) : base(r) {
        Flags = NiReaderUtils.ReadFlags(r);
        Translation = r.ReadVector3();
        Rotation = r.ReadMatrix3x3As4x4();
        Scale = r.ReadSingle();
        Velocity = r.ReadVector3();
        Properties = r.ReadL32FArray(r => X.Ref<NiProperty>(r));
        HasBoundingBox = r.ReadBool32();
        if (HasBoundingBox) BoundingBox = new BoundingBox(r);
    }
}

// Nodes
public class NiNode : NiAVObject {
    //public uint NumChildren;
    public int?[] Children;
    //public uint NumEffects;
    public int?[] Effects;

    public NiNode(BinaryReader r) : base(r) {
        Children = r.ReadL32FArray(r => X.Ref<NiAVObject>(r));
        Effects = r.ReadL32FArray(r => X.Ref<NiDynamicEffect>(r));
    }
}
public class RootCollisionNode(BinaryReader r) : NiNode(r) { }
public class NiBSAnimationNode(BinaryReader r) : NiNode(r) { }
public class NiBSParticleNode(BinaryReader r) : NiNode(r) { }
public class NiBillboardNode(BinaryReader r) : NiNode(r) { }
public class AvoidNode(BinaryReader r) : NiNode(r) { }

// Geometry
public abstract class NiGeometry : NiAVObject {
    public int? Data;
    public int? SkinInstance;

    public NiGeometry(BinaryReader r) : base(r) {
        Data = X.Ref<NiGeometryData>(r);
        SkinInstance = X.Ref<NiSkinInstance>(r);
    }
}

public abstract class NiGeometryData : NiObject {
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
    [JsonIgnore] public TexCoord[,] UVSets;

    public NiGeometryData(BinaryReader r) : base(r) {
        NumVertices = r.ReadUInt16();
        HasVertices = r.ReadBool32();
        if (HasVertices) Vertices = r.ReadFArray(r => r.ReadVector3(), NumVertices);
        HasNormals = r.ReadBool32();
        if (HasNormals) Normals = r.ReadFArray(r => r.ReadVector3(), NumVertices);
        Center = r.ReadVector3();
        Radius = r.ReadSingle();
        HasVertexColors = r.ReadBool32();
        if (HasVertexColors) VertexColors = r.ReadFArray(r => new Color4(r), NumVertices);
        NumUVSets = r.ReadUInt16();
        HasUV = r.ReadBool32();
        if (HasUV) {
            UVSets = new TexCoord[NumUVSets, NumVertices];
            for (var i = 0; i < NumUVSets; i++)
                for (var j = 0; j < NumVertices; j++) UVSets[i, j] = new TexCoord(r);
        }
    }
}

public abstract class NiTriBasedGeom(BinaryReader r) : NiGeometry(r) { }

public abstract class NiTriBasedGeomData : NiGeometryData {
    public ushort NumTriangles;

    public NiTriBasedGeomData(BinaryReader r) : base(r) {
        NumTriangles = r.ReadUInt16();
    }
}

public class NiTriShape(BinaryReader r) : NiTriBasedGeom(r) { }

public class NiTriShapeData : NiTriBasedGeomData {
    public uint NumTrianglePoints;
    public Triangle[] Triangles;
    //public ushort NumMatchGroups;
    public MatchGroup[] MatchGroups;

    public NiTriShapeData(BinaryReader r) : base(r) {
        NumTrianglePoints = r.ReadUInt32();
        Triangles = r.ReadFArray(r => new Triangle(r), NumTriangles);
        MatchGroups = r.ReadL16FArray(r => new MatchGroup(r));
    }
}

// Properties
public abstract class NiProperty(BinaryReader r) : NiObjectNET(r) { }

public class NiTexturingProperty : NiProperty {
    public NiAVObject.NiFlags Flags;
    public ApplyMode ApplyMode;
    public uint TextureCount;
    //public bool HasBaseTexture;
    public TexDesc BaseTexture;
    //public bool HasDarkTexture;
    public TexDesc DarkTexture;
    //public bool HasDetailTexture;
    public TexDesc DetailTexture;
    //public bool HasGlossTexture;
    public TexDesc GlossTexture;
    //public bool HasGlowTexture;
    public TexDesc GlowTexture;
    //public bool HasBumpMapTexture;
    public TexDesc BumpMapTexture;
    //public bool HasDecal0Texture;
    public TexDesc Decal0Texture;

    public NiTexturingProperty(BinaryReader r) : base(r) {
        Flags = NiReaderUtils.ReadFlags(r);
        ApplyMode = (ApplyMode)r.ReadUInt32();
        TextureCount = r.ReadUInt32();
        BaseTexture = r.ReadBool32() ? new TexDesc(r) : default;
        DarkTexture = r.ReadBool32() ? new TexDesc(r) : default;
        DetailTexture = r.ReadBool32() ? new TexDesc(r) : default;
        GlossTexture = r.ReadBool32() ? new TexDesc(r) : default;
        GlowTexture = r.ReadBool32() ? new TexDesc(r) : default;
        BumpMapTexture = r.ReadBool32() ? new TexDesc(r) : default;
        Decal0Texture = r.ReadBool32() ? new TexDesc(r) : default;
    }
}

public class NiAlphaProperty : NiProperty {
    public ushort Flags;
    public byte Threshold;

    public NiAlphaProperty(BinaryReader r) : base(r) {
        Flags = r.ReadUInt16();
        Threshold = r.ReadByte();
    }
}

public class NiZBufferProperty : NiProperty {
    public ushort Flags;

    public NiZBufferProperty(BinaryReader r) : base(r) {
        Flags = r.ReadUInt16();
    }
}

public class NiVertexColorProperty : NiProperty {
    public NiAVObject.NiFlags Flags;
    public VertMode VertexMode;
    public LightMode LightingMode;

    public NiVertexColorProperty(BinaryReader r) : base(r) {
        Flags = NiReaderUtils.ReadFlags(r);
        VertexMode = (VertMode)r.ReadUInt32();
        LightingMode = (LightMode)r.ReadUInt32();
    }
}

public class NiShadeProperty : NiProperty {
    public NiAVObject.NiFlags Flags;

    public NiShadeProperty(BinaryReader r) : base(r) {
        Flags = NiReaderUtils.ReadFlags(r);
    }
}

public class NiWireframeProperty : NiProperty {
    public NiAVObject.NiFlags Flags;

    public NiWireframeProperty(BinaryReader r) : base(r) {
        Flags = NiReaderUtils.ReadFlags(r);
    }
}

public class NiCamera(BinaryReader r) : NiAVObject(r) { }

// Data
public class NiUVData : NiObject {
    public KeyGroup<float>[] UVGroups;

    public NiUVData(BinaryReader r) : base(r) {
        UVGroups = r.ReadFArray(r => new KeyGroup<float>(r), 4);
    }
}

public class NiKeyframeData : NiObject {
    public uint NumRotationKeys;
    public KeyType RotationType;
    public QuatKey<Quaternion>[] QuaternionKeys;
    public float UnknownFloat;
    public KeyGroup<float>[] XYZRotations;
    public KeyGroup<Vector3> Translations;
    public KeyGroup<float> Scales;

    public NiKeyframeData(BinaryReader r) : base(r) {
        NumRotationKeys = r.ReadUInt32();
        if (NumRotationKeys != 0) {
            RotationType = (KeyType)r.ReadUInt32();
            if (RotationType != KeyType.XYZ_ROTATION_KEY)
                QuaternionKeys = r.ReadFArray(r => new QuatKey<Quaternion>(r, RotationType), (int)NumRotationKeys);
            else {

                UnknownFloat = r.ReadSingle();
                XYZRotations = r.ReadFArray(r => new KeyGroup<float>(r), 3);
            }
        }
        Translations = new KeyGroup<Vector3>(r);
        Scales = new KeyGroup<float>(r);
    }
}

public class NiColorData : NiObject {
    public KeyGroup<Color4> Data;

    public NiColorData(BinaryReader r) : base(r) {
        Data = new KeyGroup<Color4>(r);
    }
}

public class NiMorphData : NiObject {
    public uint NumMorphs;
    public uint NumVertices;
    public byte RelativeTargets;
    public Morph[] Morphs;

    public NiMorphData(BinaryReader r) : base(r) {
        NumMorphs = r.ReadUInt32();
        NumVertices = r.ReadUInt32();
        RelativeTargets = r.ReadByte();
        Morphs = r.ReadFArray(r => new Morph(r, NumVertices), (int)NumMorphs);
    }
}

public class NiVisData : NiObject {
    //public uint NumKeys;
    public Key<byte>[] Keys;

    public NiVisData(BinaryReader r) : base(r) {
        Keys = r.ReadL32FArray(r => new Key<byte>(r, KeyType.LINEAR_KEY));
    }
}

public class NiFloatData : NiObject {
    public KeyGroup<float> Data;

    public NiFloatData(BinaryReader r) : base(r) {
        Data = new KeyGroup<float>(r);
    }
}

public class NiPosData : NiObject {
    public KeyGroup<Vector3> Data;

    public NiPosData(BinaryReader r) : base(r) {
        Data = new KeyGroup<Vector3>(r);
    }
}

public class NiExtraData : NiObject {
    public int? NextExtraData;

    public NiExtraData(BinaryReader r) : base(r) {
        NextExtraData = X.Ref<NiExtraData>(r);
    }
}

public class NiStringExtraData : NiExtraData {
    public uint BytesRemaining;
    public string Str;

    public NiStringExtraData(BinaryReader r) : base(r) {
        BytesRemaining = r.ReadUInt32();
        Str = r.ReadL32Encoding();
    }
}

public class NiTextKeyExtraData : NiExtraData {
    public uint UnknownInt1;
    //public uint NumTextKeys;
    public Key<string>[] TextKeys;

    public NiTextKeyExtraData(BinaryReader r) : base(r) {
        UnknownInt1 = r.ReadUInt32();
        TextKeys = r.ReadL32FArray(r => new Key<string>(r, KeyType.LINEAR_KEY));
    }
}

public class NiVertWeightsExtraData : NiExtraData {
    public uint NumBytes;
    public ushort NumVertices;
    public float[] Weights;

    public NiVertWeightsExtraData(BinaryReader r) : base(r) {
        NumBytes = r.ReadUInt32();
        NumVertices = r.ReadUInt16();
        Weights = r.ReadPArray<float>("f", NumVertices);
    }
}

// Controllers
public abstract class NiTimeController : NiObject {
    public int? NextController;
    public ushort Flags;
    public float Frequency;
    public float Phase;
    public float StartTime;
    public float StopTime;
    public int? Target;

    public NiTimeController(BinaryReader r) : base(r) {
        NextController = X.Ref<NiTimeController>(r);
        Flags = r.ReadUInt16();
        Frequency = r.ReadSingle();
        Phase = r.ReadSingle();
        StartTime = r.ReadSingle();
        StopTime = r.ReadSingle();
        Target = X.Ptr<NiObjectNET>(r);
    }
}

public class NiUVController : NiTimeController {
    public ushort UnknownShort;
    public int? Data;

    public NiUVController(BinaryReader r) : base(r) {
        UnknownShort = r.ReadUInt16();
        Data = X.Ref<NiUVData>(r);
    }
}

public abstract class NiInterpController(BinaryReader r) : NiTimeController(r) { }

public abstract class NiSingleInterpController(BinaryReader r) : NiInterpController(r) { }

public class NiKeyframeController : NiSingleInterpController {
    public int? Data;

    public NiKeyframeController(BinaryReader r) : base(r) {
        Data = X.Ref<NiKeyframeData>(r);
    }
}

public class NiGeomMorpherController : NiInterpController {
    public int? Data;
    public byte AlwaysUpdate;

    public NiGeomMorpherController(BinaryReader r) : base(r) {
        Data = X.Ref<NiMorphData>(r);
        AlwaysUpdate = r.ReadByte();
    }
}

public abstract class NiBoolInterpController(BinaryReader r) : NiSingleInterpController(r) { }

public class NiVisController : NiBoolInterpController {
    public int? Data;

    public NiVisController(BinaryReader r) : base(r) {
        Data = X.Ref<NiVisData>(r);
    }
}

public abstract class NiFloatInterpController(BinaryReader r) : NiSingleInterpController(r) { }

public class NiAlphaController : NiFloatInterpController {
    public int? Data;

    public NiAlphaController(BinaryReader r) : base(r) {
        Data = X.Ref<NiFloatData>(r);
    }
}

// Particles
public class NiParticles(BinaryReader r) : NiGeometry(r) { }
public class NiParticlesData : NiGeometryData {
    public ushort NumParticles;
    public float ParticleRadius;
    public ushort NumActive;
    public bool HasSizes;
    public float[] Sizes;

    public NiParticlesData(BinaryReader r) : base(r) {
        NumParticles = r.ReadUInt16();
        ParticleRadius = r.ReadSingle();
        NumActive = r.ReadUInt16();
        HasSizes = r.ReadBool32();
        if (HasSizes) Sizes = r.ReadPArray<float>("f", NumVertices);
    }
}

public class NiRotatingParticles(BinaryReader r) : NiParticles(r) { }
public class NiRotatingParticlesData : NiParticlesData {
    public bool HasRotations;
    public Quaternion[] Rotations;

    public NiRotatingParticlesData(BinaryReader r) : base(r) {
        HasRotations = r.ReadBool32();
        if (HasRotations) Rotations = r.ReadFArray(r => r.ReadQuaternionWFirst(), NumVertices);
    }
}

public class NiAutoNormalParticles(BinaryReader r) : NiParticles(r) { }
public class NiAutoNormalParticlesData(BinaryReader r) : NiParticlesData(r) { }

public class NiParticleSystemController : NiTimeController {
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
    public int? Emitter;
    public ushort UnknownShort2;
    public float UnknownFloat13;
    public uint UnknownInt1;
    public uint UnknownInt2;
    public ushort UnknownShort3;
    public ushort NumParticles;
    public ushort NumValid;
    public Particle[] Particles;
    public int? UnknownLink;
    public int? ParticleExtra;
    public int? UnknownLink2;
    public byte Trailer;

    public NiParticleSystemController(BinaryReader r) : base(r) {
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
        Emitter = X.Ptr<NiObject>(r);
        UnknownShort2 = r.ReadUInt16();
        UnknownFloat13 = r.ReadSingle();
        UnknownInt1 = r.ReadUInt32();
        UnknownInt2 = r.ReadUInt32();
        UnknownShort3 = r.ReadUInt16();
        NumParticles = r.ReadUInt16();
        NumValid = r.ReadUInt16();
        Particles = r.ReadFArray(r => new Particle(r), NumParticles);
        UnknownLink = X.Ref<NiObject>(r);
        ParticleExtra = X.Ref<NiParticleModifier>(r);
        UnknownLink2 = X.Ref<NiObject>(r);
        Trailer = r.ReadByte();
    }
}

public class NiBSPArrayController(BinaryReader r) : NiParticleSystemController(r) { }

// Particle Modifiers
public abstract class NiParticleModifier : NiObject {
    public int? NextModifier;
    public int? Controller;

    public NiParticleModifier(BinaryReader r) : base(r) {
        NextModifier = X.Ref<NiParticleModifier>(r);
        Controller = X.Ptr<NiParticleSystemController>(r);
    }
}

public class NiGravity : NiParticleModifier {
    public float UnknownFloat1;
    public float Force;
    public FieldType Type;
    public Vector3 Position;
    public Vector3 Direction;

    public NiGravity(BinaryReader r) : base(r) {
        UnknownFloat1 = r.ReadSingle();
        Force = r.ReadSingle();
        Type = (FieldType)r.ReadUInt32();
        Position = r.ReadVector3();
        Direction = r.ReadVector3();
    }
}

public class NiParticleBomb : NiParticleModifier {
    public float Decay;
    public float Duration;
    public float DeltaV;
    public float Start;
    public DecayType DecayType;
    public Vector3 Position;
    public Vector3 Direction;

    public NiParticleBomb(BinaryReader r) : base(r) {
        Decay = r.ReadSingle();
        Duration = r.ReadSingle();
        DeltaV = r.ReadSingle();
        Start = r.ReadSingle();
        DecayType = (DecayType)r.ReadUInt32();
        Position = r.ReadVector3();
        Direction = r.ReadVector3();
    }
}

public class NiParticleColorModifier : NiParticleModifier {
    public int? ColorData;

    public NiParticleColorModifier(BinaryReader r) : base(r) {
        ColorData = X.Ref<NiColorData>(r);
    }
}

public class NiParticleGrowFade : NiParticleModifier {
    public float Grow;
    public float Fade;

    public NiParticleGrowFade(BinaryReader r) : base(r) {
        Grow = r.ReadSingle();
        Fade = r.ReadSingle();
    }
}

public class NiParticleMeshModifier : NiParticleModifier {
    //public uint NumParticleMeshes;
    public int?[] ParticleMeshes;

    public NiParticleMeshModifier(BinaryReader r) : base(r) {
        ParticleMeshes = r.ReadL32FArray(r => X.Ref<NiAVObject>(r));
    }
}

public class NiParticleRotation : NiParticleModifier {
    public byte RandomInitialAxis;
    public Vector3 InitialAxis;
    public float RotationSpeed;

    public NiParticleRotation(BinaryReader r) : base(r) {
        RandomInitialAxis = r.ReadByte();
        InitialAxis = r.ReadVector3();
        RotationSpeed = r.ReadSingle();
    }
}

// Skin Stuff
public class NiSkinInstance : NiObject {
    public int? Data;
    public int? SkeletonRoot;
    public uint NumBones;
    public int?[] Bones;

    public NiSkinInstance(BinaryReader r) : base(r) {
        Data = X.Ref<NiSkinData>(r);
        SkeletonRoot = X.Ptr<NiNode>(r);
        NumBones = r.ReadUInt32();
        Bones = r.ReadFArray(r => X.Ptr<NiNode>(r), (int)NumBones);
    }
}

public class NiSkinData : NiObject {
    public SkinTransform SkinTransform;
    public uint NumBones;
    public int? SkinPartition;
    public SkinData[] BoneList;

    public NiSkinData(BinaryReader r) : base(r) {
        SkinTransform = new SkinTransform(r);
        NumBones = r.ReadUInt32();
        SkinPartition = X.Ref<NiSkinPartition>(r);
        BoneList = r.ReadFArray(r => new SkinData(r), (int)NumBones);
    }
}

public class NiSkinPartition(BinaryReader r) : NiObject(r) { }

// Miscellaneous
public abstract class NiTexture(BinaryReader r) : NiObjectNET(r) { }

public class NiSourceTexture : NiTexture {
    public byte UseExternal;
    public string FileName;
    public PixelLayout PixelLayout;
    public MipMapFormat UseMipMaps;
    public AlphaFormat AlphaFormat;
    public byte IsStatic;

    public NiSourceTexture(BinaryReader r) : base(r) {
        UseExternal = r.ReadByte();
        FileName = r.ReadL32Encoding();
        PixelLayout = (PixelLayout)r.ReadUInt32();
        UseMipMaps = (MipMapFormat)r.ReadUInt32();
        AlphaFormat = (AlphaFormat)r.ReadUInt32();
        IsStatic = r.ReadByte();
    }
}

public abstract class NiPoint3InterpController : NiSingleInterpController {
    public int? Data;

    public NiPoint3InterpController(BinaryReader r) : base(r) {
        Data = X.Ref<NiPosData>(r);
    }
}

public class NiMaterialProperty : NiProperty {
    public NiAVObject.NiFlags Flags;
    public Color3 AmbientColor;
    public Color3 DiffuseColor;
    public Color3 SpecularColor;
    public Color3 EmissiveColor;
    public float Glossiness;
    public float Alpha;

    public NiMaterialProperty(BinaryReader r) : base(r) {
        Flags = NiReaderUtils.ReadFlags(r);
        AmbientColor = new Color3(r);
        DiffuseColor = new Color3(r);
        SpecularColor = new Color3(r);
        EmissiveColor = new Color3(r);
        Glossiness = r.ReadSingle();
        Alpha = r.ReadSingle();
    }
}

public class NiMaterialColorController(BinaryReader r) : NiPoint3InterpController(r) { }

public abstract class NiDynamicEffect : NiAVObject {
    //uint NumAffectedNodeListPointers;
    uint[] AffectedNodeListPointers;

    public NiDynamicEffect(BinaryReader r) : base(r) {
        AffectedNodeListPointers = r.ReadL32PArray<uint>("I");
    }
}

public class NiTextureEffect : NiDynamicEffect {
    public Matrix4x4 ModelProjectionMatrix;
    public Vector3 ModelProjectionTransform;
    public TexFilterMode TextureFiltering;
    public TexClampMode TextureClamping;
    public EffectType TextureType;
    public CoordGenType CoordinateGenerationType;
    public int? SourceTexture;
    public byte ClippingPlane;
    public Vector3 UnknownVector;
    public float UnknownFloat;
    public short PS2L;
    public short PS2K;
    public ushort UnknownShort;

    public NiTextureEffect(BinaryReader r) : base(r) {
        ModelProjectionMatrix = r.ReadMatrix3x3As4x4();
        ModelProjectionTransform = r.ReadVector3();
        TextureFiltering = (TexFilterMode)r.ReadUInt32();
        TextureClamping = (TexClampMode)r.ReadUInt32();
        TextureType = (EffectType)r.ReadUInt32();
        CoordinateGenerationType = (CoordGenType)r.ReadUInt32();
        SourceTexture = X.Ref<NiSourceTexture>(r);
        ClippingPlane = r.ReadByte();
        UnknownVector = r.ReadVector3();
        UnknownFloat = r.ReadSingle();
        PS2L = r.ReadInt16();
        PS2K = r.ReadInt16();
        UnknownShort = r.ReadUInt16();
    }
}

#endregion

public class NiReaderUtils {
    public static NiAVObject.NiFlags ReadFlags(BinaryReader r) => (NiAVObject.NiFlags)r.ReadUInt16();

    public static T Read<T>(BinaryReader r) {
        if (typeof(T) == typeof(float)) { return (T)(object)r.ReadSingle(); }
        else if (typeof(T) == typeof(byte)) { return (T)(object)r.ReadByte(); }
        else if (typeof(T) == typeof(string)) { return (T)(object)r.ReadL32Encoding(); }
        else if (typeof(T) == typeof(Vector3)) { return (T)(object)r.ReadVector3(); }
        else if (typeof(T) == typeof(Quaternion)) { return (T)(object)r.ReadQuaternionWFirst(); }
        else if (typeof(T) == typeof(Color4)) { return (T)(object)new Color4(r); }
        else throw new NotImplementedException("Tried to read an unsupported type.");
    }

    public static NiObject ReadNiObject(BinaryReader r) {
        var nodeType = r.ReadL32AString();
        switch (nodeType) {
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
}

