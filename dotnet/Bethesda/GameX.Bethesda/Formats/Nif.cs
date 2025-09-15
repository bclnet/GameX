using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using static OpenStack.Debug;
#pragma warning disable CS9113 // Parameter is unread.

namespace GameX.Bethesda.Formats.Nif;

#region X

public class Ref<T>(NifReader r, int v) where T : NiObject { public int v = v; T val; [JsonIgnore] public T Value => val ??= (T)r.Blocks[v]; }

static class Y<T> {
    public static T Read(BinaryReader r) {
        if (typeof(T) == typeof(float)) { return (T)(object)r.ReadSingle(); }
        else if (typeof(T) == typeof(byte)) { return (T)(object)r.ReadByte(); }
        else if (typeof(T) == typeof(string)) { return (T)(object)r.ReadL32Encoding(); }
        else if (typeof(T) == typeof(Vector3)) { return (T)(object)r.ReadVector3(); }
        else if (typeof(T) == typeof(Quaternion)) { return (T)(object)r.ReadQuaternionWFirst(); }
        else if (typeof(T) == typeof(Color4)) { return (T)(object)new Color4(r); }
        else throw new NotImplementedException("Tried to read an unsupported type.");
    }
}

static class X<T> where T : NiObject {
    public static Ref<T> Ptr(BinaryReader r) { int v; return (v = r.ReadInt32()) < 0 ? null : new Ref<T>((NifReader)r, v); }
    public static Ref<T> Ref(BinaryReader r) { int v; return (v = r.ReadInt32()) < 0 ? null : new Ref<T>((NifReader)r, v); }
}

static class X {
    public static string String(BinaryReader r) => r.ReadL32Encoding();
    public static string StringRef(BinaryReader r, int? p) => default;
    public static bool IsVersionSupported(uint v) => true;
    public static (string, uint) ParseHeaderStr(string s) {
        var p = s.IndexOf("Version");
        if (p >= 0) {
            var v = s;
            v = v[(p + 8)..];
            for (var i = 0; i < v.Length; i++)
                if (char.IsDigit(v[i]) || v[i] == '.') continue;
                else v = v[..i];
            var ver = Ver2Num(v);
            if (!IsVersionSupported(ver)) throw new Exception($"Version {Ver2Str(ver)} ({ver}) is not supported.");
            return (s, ver);
        }
        else if (s.StartsWith("NS")) return (s, 0x0a010000); // Dodgy version for NeoSteam
        throw new Exception("Invalid header string");
    }
    public static string Ver2Str(uint v) {
        if (v == 0) return "";
        else if (v < 0x0303000D) {
            // this is an old-style 2-number version with one period
            var s = $"{(v >> 24) & 0xff}.{(v >> 16) & 0xff}";
            uint sub_num1 = (v >> 8) & 0xff, sub_num2 = v & 0xff;
            if (sub_num1 > 0 || sub_num2 > 0) s += $"{sub_num1}";
            if (sub_num2 > 0) s += $"{sub_num2}";
            return s;
        }
        // this is a new-style 4-number version with 3 periods
        else return $"{(v >> 24) & 0xff}.{(v >> 16) & 0xff}.{(v >> 8) & 0xff}.{v & 0xff}";
    }
    public static uint Ver2Num(string s) {
        if (string.IsNullOrEmpty(s)) return 0;
        if (s.Contains('.')) {
            var l = s.Split(".");
            var v = 0U;
            if (l.Length > 4) return 0; // Version # has more than 3 dots in it.
            else if (l.Length == 2) {
                // this is an old style version number.
                v += uint.Parse(l[0]) << (3 * 8);
                if (l[1].Length >= 1) v += uint.Parse(l[1][0..1]) << (2 * 8);
                if (l[1].Length >= 2) v += uint.Parse(l[1][1..2]) << (1 * 8);
                if (l[1].Length >= 3) v += uint.Parse(l[1][2..]);
                return v;
            }
            // this is a new style version number with dots separating the digits
            for (var i = 0; i < 4 && i < l.Length; i++) v += uint.Parse(l[i]) << ((3 - i) * 8);
            return v;
        }
        return uint.Parse(s);
    }
    public static void Register() {
        DesSer.Add(new RefJsonConverter<NiImage>(),
        new RefJsonConverter<NiSourceTexture>(),
        new RefJsonConverter<NiParticleModifier>(),
        new RefJsonConverter<NiParticleSystemController>(),
        new RefJsonConverter<NiExtraData>(),
        new RefJsonConverter<NiTimeController>(),
        new RefJsonConverter<NiProperty>(),
        new RefJsonConverter<NiNode>(),
        new RefJsonConverter<NiObjectNET>(),
        new RefJsonConverter<NiMorphData>(),
        new RefJsonConverter<NiKeyframeData>(),
        new RefJsonConverter<NiFloatData>(),
        new RefJsonConverter<NiVisData>(),
        new RefJsonConverter<NiPosData>(),
        new RefJsonConverter<NiGeometryData>(),
        new RefJsonConverter<NiSkinInstance>(),
        new RefJsonConverter<NiAVObject>(),
        new RefJsonConverter<NiDynamicEffect>(),
        new RefJsonConverter<NiColorData>(),
        new RefJsonConverter<NiObject>(),
        new RefJsonConverter<NiParticleModifier>(),
        new RefJsonConverter<NiSkinData>(),
        new RefJsonConverter<NiSkinPartition>(),
        new RefJsonConverter<NiUVData>(),
        new TexCoordJsonConverter(),
        new TriangleJsonConverter());
    }
}

public enum Flags : ushort {
    Hidden = 0x1
}

public class RefJsonConverter<T> : JsonConverter<Ref<T>> where T : NiObject {
    public override Ref<T> Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Ref<T> s, JsonSerializerOptions options) => w.WriteStringValue($"{s.v}");
}

public class TexCoordJsonConverter : JsonConverter<TexCoord> {
    public override TexCoord Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, TexCoord s, JsonSerializerOptions options) => w.WriteStringValue($"{s.u} {s.v}");
}

public class TriangleJsonConverter : JsonConverter<Triangle> {
    public override Triangle Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Triangle s, JsonSerializerOptions options) => w.WriteStringValue($"{s.v1} {s.v2} {s.v3}");
}

#endregion

#region Enums

/// <summary>
/// Describes how the vertex colors are blended with the filtered texture color.
/// </summary>
public enum ApplyMode : uint { // X
    APPLY_REPLACE = 0,              // Replaces existing color
    APPLY_DECAL = 1,                // For placing images on the object like stickers.
    APPLY_MODULATE = 2,             // Modulates existing color. (Default)
    APPLY_HILIGHT = 3,              // PS2 Only.  Function Unknown.
    APPLY_HILIGHT2 = 4              // Parallax Flag in some Oblivion meshes.
}

/// <summary>
/// The type of animation interpolation (blending) that will be used on the associated key frames.
/// </summary>
public enum KeyType : uint { // X
    LINEAR_KEY = 1,                 // Use linear interpolation.
    QUADRATIC_KEY = 2,              // Use quadratic interpolation.  Forward and back tangents will be stored.
    TBC_KEY = 3,                    // Use Tension Bias Continuity interpolation.  Tension, bias, and continuity will be stored.
    XYZ_ROTATION_KEY = 4,           // For use only with rotation data.  Separate X, Y, and Z keys will be stored instead of using quaternions.
    CONST_KEY = 5                   // Step function. Used for visibility keys in NiBoolData.
}

/// <summary>
/// Describes the pixel format used by the NiPixelData object to store a texture.
/// </summary>
public enum PixelFormat : uint { // Y
    FMT_RGB = 0,                    // 24-bit RGB. 8 bits per red, blue, and green component.
    FMT_RGBA = 1,                   // 32-bit RGB with alpha. 8 bits per red, blue, green, and alpha component.
    FMT_PAL = 2,                    // 8-bit palette index.
    FMT_PALA = 3,                   // 8-bit palette index with alpha.
    FMT_DXT1 = 4,                   // DXT1 compressed texture.
    FMT_DXT3 = 5,                   // DXT3 compressed texture.
    FMT_DXT5 = 6,                   // DXT5 compressed texture.
    FMT_RGB24NONINT = 7,            // (Deprecated) 24-bit noninterleaved texture, an old PS2 format.
    FMT_BUMP = 8,                   // Uncompressed dU/dV gradient bump map.
    FMT_BUMPLUMA = 9,               // Uncompressed dU/dV gradient bump map with luma channel representing shininess.
    FMT_RENDERSPEC = 10,            // Generic descriptor for any renderer-specific format not described by other formats.
    FMT_1CH = 11,                   // Generic descriptor for formats with 1 component.
    FMT_2CH = 12,                   // Generic descriptor for formats with 2 components.
    FMT_3CH = 13,                   // Generic descriptor for formats with 3 components.
    FMT_4CH = 14,                   // Generic descriptor for formats with 4 components.
    FMT_DEPTH_STENCIL = 15,         // Indicates the NiPixelFormat is meant to be used on a depth/stencil surface.
    FMT_UNKNOWN = 16
}

/// <summary>
/// Describes whether pixels have been tiled from their standard row-major format to a format optimized for a particular platform.
/// </summary>
public enum PixelTiling : uint { // Y
    TILE_NONE = 0,
    TILE_XENON = 1,
    TILE_WII = 2,
    TILE_NV_SWIZZLED = 3
}

/// <summary>
/// Describes the pixel format used by the NiPixelData object to store a texture.
/// </summary>
public enum PixelComponent : uint { // Y
    COMP_RED = 0,
    COMP_GREEN = 1,
    COMP_BLUE = 2,
    COMP_ALPHA = 3,
    COMP_COMPRESSED = 4,
    COMP_OFFSET_U = 5,
    COMP_OFFSET_V = 6,
    COMP_OFFSET_W = 7,
    COMP_OFFSET_Q = 8,
    COMP_LUMA = 9,
    COMP_HEIGHT = 10,
    COMP_VECTOR_X = 11,
    COMP_VECTOR_Y = 12,
    COMP_VECTOR_Z = 13,
    COMP_PADDING = 14,
    COMP_INTENSITY = 15,
    COMP_INDEX = 16,
    COMP_DEPTH = 17,
    COMP_STENCIL = 18,
    COMP_EMPTY = 19
}

/// <summary>
/// Describes how each pixel should be accessed on NiPixelFormat.
/// </summary>
public enum PixelRepresentation : uint { // Y
    REP_NORM_INT = 0,
    REP_HALF = 1,
    REP_FLOAT = 2,
    REP_INDEX = 3,
    REP_COMPRESSED = 4,
    REP_UNKNOWN = 5,
    REP_INT = 6
}

/// <summary>
/// Describes the color depth in an NiTexture.
/// </summary>
public enum PixelLayout : uint { // X
    LAY_PALETTIZED_8 = 0,           // Texture is in 8-bit palettized format.
    LAY_HIGH_COLOR_16 = 1,          // Texture is in 16-bit high color format.
    LAY_TRUE_COLOR_32 = 2,          // Texture is in 32-bit true color format.
    LAY_COMPRESSED = 3,             // Texture is compressed.
    LAY_BUMPMAP = 4,                // Texture is a grayscale bump map.
    LAY_PALETTIZED_4 = 5,           // Texture is in 4-bit palettized format.
    LAY_DEFAULT = 6,                // Use default setting.
    LAY_SINGLE_COLOR_8 = 7,
    LAY_SINGLE_COLOR_16 = 8,
    LAY_SINGLE_COLOR_32 = 9,
    LAY_DOUBLE_COLOR_32 = 10,
    LAY_DOUBLE_COLOR_64 = 11,
    LAY_FLOAT_COLOR_32 = 12,
    LAY_FLOAT_COLOR_64 = 13,
    LAY_FLOAT_COLOR_128 = 14,
    LAY_SINGLE_COLOR_4 = 15,
    LAY_DEPTH_24_X8 = 16
}

/// <summary>
/// Describes how mipmaps are handled in an NiTexture.
/// </summary>
public enum MipMapFormat : uint { // X
    MIP_FMT_NO = 0,                 // Texture does not use mip maps.
    MIP_FMT_YES = 1,                // Texture uses mip maps.
    MIP_FMT_DEFAULT = 2             // Use default setting.
}

/// <summary>
/// Describes how transparency is handled in an NiTexture.
/// </summary>
public enum AlphaFormat : uint { // X
    ALPHA_NONE = 0,                 // No alpha.
    ALPHA_BINARY = 1,               // 1-bit alpha.
    ALPHA_SMOOTH = 2,               // Interpolated 4- or 8-bit alpha.
    ALPHA_DEFAULT = 3               // Use default setting.
}

/// <summary>
/// Describes the availiable texture clamp modes, i.e. the behavior of UV mapping outside the [0,1] range.
/// </summary>
public enum TexClampMode : uint { // X
    CLAMP_S_CLAMP_T = 0,            // Clamp in both directions.
    CLAMP_S_WRAP_T = 1,             // Clamp in the S(U) direction but wrap in the T(V) direction.
    WRAP_S_CLAMP_T = 2,             // Wrap in the S(U) direction but clamp in the T(V) direction.
    WRAP_S_WRAP_T = 3               // Wrap in both directions.
}

/// <summary>
/// Describes the availiable texture filter modes, i.e. the way the pixels in a texture are displayed on screen.
/// </summary>
public enum TexFilterMode : uint { // X
    FILTER_NEAREST = 0,             // Nearest neighbor. Uses nearest texel with no mipmapping.
    FILTER_BILERP = 1,              // Bilinear. Linear interpolation with no mipmapping.
    FILTER_TRILERP = 2,             // Trilinear. Linear intepolation between 8 texels (4 nearest texels between 2 nearest mip levels).
    FILTER_NEAREST_MIPNEAREST = 3,  // Nearest texel on nearest mip level.
    FILTER_NEAREST_MIPLERP = 4,     // Linear interpolates nearest texel between two nearest mip levels.
    FILTER_BILERP_MIPNEAREST = 5,   // Linear interpolates on nearest mip level.
    FILTER_ANISOTROPIC = 6          // Anisotropic filtering. One or many trilinear samples depending on anisotropy.
}

/// <summary>
/// Describes how to apply vertex colors for NiVertexColorProperty.
/// </summary>
public enum VertMode : uint { // X
    VERT_MODE_SRC_IGNORE = 0,       // Emissive, ambient, and diffuse colors are all specified by the NiMaterialProperty.
    VERT_MODE_SRC_EMISSIVE = 1,     // Emissive colors are specified by the source vertex colors. Ambient+Diffuse are specified by the NiMaterialProperty.
    VERT_MODE_SRC_AMB_DIF = 2       // Ambient+Diffuse colors are specified by the source vertex colors. Emissive is specified by the NiMaterialProperty. (Default)
}

/// <summary>
/// Describes which lighting equation components influence the final vertex color for NiVertexColorProperty.
/// </summary>
public enum LightMode : uint { // X
    LIGHT_MODE_EMISSIVE = 0,        // Emissive.
    LIGHT_MODE_EMI_AMB_DIF = 1      // Emissive + Ambient + Diffuse. (Default)
}

/// <summary>
/// The force field type.
/// </summary>
public enum FieldType : uint { // X
    FIELD_WIND = 0,                 // Wind (fixed direction)
    FIELD_POINT = 1                 // Point (fixed origin)
}

/// <summary>
/// Determines the way the billboard will react to the camera.
/// Billboard mode is stored in lowest 3 bits although Oblivion vanilla nifs uses values higher than 7.
/// </summary>
public enum BillboardMode : ushort { // Y
    ALWAYS_FACE_CAMERA = 0,         // Align billboard and camera forward vector. Minimized rotation.
    ROTATE_ABOUT_UP = 1,            // Align billboard and camera forward vector while allowing rotation around the up axis.
    RIGID_FACE_CAMERA = 2,          // Align billboard and camera forward vector. Non-minimized rotation.
    ALWAYS_FACE_CENTER = 3,         // Billboard forward vector always faces camera ceneter. Minimized rotation.
    RIGID_FACE_CENTER = 4,          // Billboard forward vector always faces camera ceneter. Non-minimized rotation.
    BSROTATE_ABOUT_UP = 5,          // The billboard will only rotate around its local Z axis (it always stays in its local X-Y plane).
    ROTATE_ABOUT_UP2 = 9            // The billboard will only rotate around the up axis (same as ROTATE_ABOUT_UP?).
}

/// <summary>
/// Describes Z-buffer test modes for NiZBufferProperty.
/// "Less than" = closer to camera, "Greater than" = further from camera.
/// </summary>
public enum ZCompareMode : uint { // Y
    ZCOMP_ALWAYS = 0,               // Always true. Buffer is ignored.
    ZCOMP_LESS = 1,                 // VRef ‹ VBuf
    ZCOMP_EQUAL = 2,                // VRef = VBuf
    ZCOMP_LESS_EQUAL = 3,           // VRef ≤ VBuf
    ZCOMP_GREATER = 4,              // VRef › VBuf
    ZCOMP_NOT_EQUAL = 5,            // VRef ≠ VBuf
    ZCOMP_GREATER_EQUAL = 6,        // VRef ≥ VBuf
    ZCOMP_NEVER = 7                 // Always false. Ref value is ignored.
}

/// <summary>
/// Describes the decay function of bomb forces.
/// </summary>
public enum DecayType : uint { // X
    DECAY_NONE = 0,                 // No decay.
    DECAY_LINEAR = 1,               // Linear decay.
    DECAY_EXPONENTIAL = 2           // Exponential decay.
}

/// <summary>
/// Describes the symmetry type of bomb forces.
/// </summary>
public enum SymmetryType : uint { // Y
    SPHERICAL_SYMMETRY = 0,         // Spherical Symmetry.
    CYLINDRICAL_SYMMETRY = 1,       // Cylindrical Symmetry.
    PLANAR_SYMMETRY = 2             // Planar Symmetry.
}

/// <summary>
/// The type of information that is stored in a texture used by an NiTextureEffect.
/// </summary>
public enum TextureType : uint { // X
    TEX_PROJECTED_LIGHT = 0,        // Apply a projected light texture. Each light effect is summed before multiplying by the base texture.
    TEX_PROJECTED_SHADOW = 1,       // Apply a projected shadow texture. Each shadow effect is multiplied by the base texture.
    TEX_ENVIRONMENT_MAP = 2,        // Apply an environment map texture. Added to the base texture and light/shadow/decal maps.
    TEX_FOG_MAP = 3                 // Apply a fog map texture. Alpha channel is used to blend the color channel with the base texture.
}

/// <summary>
/// Determines the way that UV texture coordinates are generated.
/// </summary>
public enum CoordGenType : uint { // X
    CG_WORLD_PARALLEL = 0,          // Use planar mapping.
    CG_WORLD_PERSPECTIVE = 1,       // Use perspective mapping.
    CG_SPHERE_MAP = 2,              // Use spherical mapping.
    CG_SPECULAR_CUBE_MAP = 3,       // Use specular cube mapping. For NiSourceCubeMap only.
    CG_DIFFUSE_CUBE_MAP = 4         // Use diffuse cube mapping. For NiSourceCubeMap only.
}

public enum EndianType : byte { // X
    ENDIAN_BIG = 0,                 // The numbers are stored in big endian format, such as those used by PowerPC Mac processors.
    ENDIAN_LITTLE = 1               // The numbers are stored in little endian format, such as those used by Intel and AMD x86 processors.
}

/// <summary>
/// Used by NiMaterialColorControllers to select which type of color in the controlled object that will be animated.
/// </summary>
public enum MaterialColor : ushort { // Y
    TC_AMBIENT = 0,                 // Control the ambient color.
    TC_DIFFUSE = 1,                 // Control the diffuse color.
    TC_SPECULAR = 2,                // Control the specular color.
    TC_SELF_ILLUM = 3               // Control the self illumination color.
}

/// <summary>
/// Used by NiGeometryData to control the volatility of the mesh.
/// Consistency Type is masked to only the upper 4 bits (0xF000). Dirty mask is the lower 12 (0x0FFF) but only used at runtime.
/// </summary>
public enum ConsistencyType : ushort { // Y
    CT_MUTABLE = 0x0000,            // Mutable Mesh
    CT_STATIC = 0x4000,             // Static Mesh
    CT_VOLATILE = 0x8000            // Volatile Mesh
}

public enum BoundVolumeType : uint { // X
    BASE_BV = 0xffffffff,           // Default
    SPHERE_BV = 0,                  // Sphere
    BOX_BV = 1,                     // Box
    CAPSULE_BV = 2,                 // Capsule
    UNION_BV = 4,                   // Union
    HALFSPACE_BV = 5                // Half Space
}

/// <summary>
/// Values for configuring the shader type in a BSLightingShaderProperty
/// </summary>
public enum BSLightingShaderPropertyShaderType : uint { // Y
    Default = 0,
    Environment_Map = 1,            // Enables EnvMap Mask(TS6), EnvMap Scale
    Glow_Shader = 2,                // Enables Glow(TS3)
    Parallax = 3,                   // Enables Height(TS4)
    Face_Tint = 4,                  // Enables Detail(TS4), Tint(TS7)
    Skin_Tint = 5,                  // Enables Skin Tint Color
    Hair_Tint = 6,                  // Enables Hair Tint Color
    Parallax_Occ = 7,               // Enables Height(TS4), Max Passes, Scale. Unimplemented.
    Multitexture_Landscape = 8,
    LOD_Landscape = 9,
    Snow = 10,
    MultiLayer_Parallax = 11,       // Enables EnvMap Mask(TS6), Layer(TS7), Parallax Layer Thickness, Parallax Refraction Scale, Parallax Inner Layer U Scale, Parallax Inner Layer V Scale, EnvMap Scale
    Tree_Anim = 12,
    LOD_Objects = 13,
    Sparkle_Snow = 14,              // Enables SparkleParams
    LOD_Objects_HD = 15,
    Eye_Envmap = 16,                // Enables EnvMap Mask(TS6), Eye EnvMap Scale
    Cloud = 17,
    LOD_Landscape_Noise = 18,
    Multitexture_Landscape_LOD_Blend = 19,
    FO4_Dismemberment = 20
}

#endregion

#region Compounds

// Color3 -> new Color3(r)
// Color4 -> new Color4(r)
/// <summary>
/// The NIF file footer.
/// </summary>
public class Footer(NifReader r) { // X
    public Ref<NiObject>[] Roots = r.V >= 0x0303000D ? r.ReadL32FArray(X<NiObject>.Ref) : default; // List of root NIF objects. If there is a camera, for 1st person view, then this NIF object is referred to as well in this list, even if it is not a root object (usually we want the camera to be attached to the Bip Head node).
}

/// <summary>
/// Group of vertex indices of vertices that match.
/// </summary>
public class MatchGroup(NifReader r) { // X
    public ushort[] VertexIndices = r.ReadL16PArray<ushort>("H"); // The vertex indices.
}

/// <summary>
/// NiSkinData::BoneVertData. A vertex and its weight.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BoneVertData(NifReader r, bool full) { // X
    public static (string, int) Struct = ("<Hf", 6);
    public ushort Index = r.ReadUInt16();               // The vertex index, in the mesh.
    public float Weight = full ? r.ReadSingle() : r.ReadHalf(); // The vertex weight - between 0.0 and 1.0
}

/// <summary>
/// Information about how the file was exported
/// </summary>
public class ExportInfo(NifReader r) { // X
    public string Author = r.ReadL8AString();
    public string ProcessScript = r.ReadL8AString();
    public string ExportScript = r.ReadL8AString();
}

/// <summary>
/// The NIF file header.
/// </summary>
public class NifReader : BinaryReader { // X
    public string HeaderString;                         // 'NetImmerse File Format x.x.x.x' (versions <= 10.0.1.2) or 'Gamebryo File Format x.x.x.x' (versions >= 10.1.0.0), with x.x.x.x the version written out. Ends with a newline character (0x0A).
    public string[] Copyright;
    public uint V = 0x04000002;                         // The NIF version, in hexadecimal notation: 0x04000002, 0x0401000C, 0x04020002, 0x04020100, 0x04020200, 0x0A000100, 0x0A010000, 0x0A020000, 0x14000004, ...
    public EndianType EndianType = EndianType.ENDIAN_LITTLE; // Determines the endianness of the data in the file.
    public uint UV;                                     // An extra version number, for companies that decide to modify the file format.
    public uint NumBlocks;                              // Number of file objects.
    public uint UV2 = 0;
    public ExportInfo ExportInfo;
    public string MaxFilepath;
    public byte[] Metadata;
    public string[] BlockTypes;                         // List of all object types used in this NIF file.
    public uint[] BlockTypeHashes;                      // List of all object types used in this NIF file.
    public ushort[] BlockTypeIndex;                     // Maps file objects on their corresponding type: first file object is of type object_types[object_type_index[0]], the second of object_types[object_type_index[1]], etc.
    public uint[] BlockSize;                            // Array of block sizes?
    public uint NumStrings;                             // Number of strings.
    public uint MaxStringLength;                        // Maximum string length.
    public string[] Strings;                            // Strings.
    public uint[] Groups;
    // read blocks
    public NiObject[] Blocks;
    public Ref<NiObject>[] Roots;

    public NifReader(BinaryReader b) : base(b.BaseStream) {
        (HeaderString, V) = X.ParseHeaderStr(b.ReadVAString(0x80, 0xA)); var r = this;
        if (r.V <= 0x03010000) Copyright = [r.ReadL8AString(), r.ReadL8AString(), r.ReadL8AString()];
        if (r.V >= 0x03010001) V = r.ReadUInt32();
        if (r.V >= 0x14000003) EndianType = (EndianType)r.ReadByte();
        if (r.V >= 0x0A000108) UV = r.ReadUInt32();
        if (r.V >= 0x03010001) NumBlocks = r.ReadUInt32();
        if (((r.V == 0x14020007) || (r.V == 0x14000005) || ((r.V >= 0x0A000102) && (r.V <= 0x14000004) && (r.UV <= 11))) && (r.UV >= 3)) {
            UV2 = r.ReadUInt32();
            ExportInfo = new ExportInfo(r);
        }
        if (r.UV2 == 130) MaxFilepath = r.ReadL8AString();
        if (r.V >= 0x1E000000) Metadata = r.ReadL8Bytes();
        if (r.V >= 0x05000001 && r.V != 0x14030102) BlockTypes = r.ReadL16FArray(z => r.ReadL32AString());
        if (r.V == 0x14030102) BlockTypeHashes = r.ReadL16PArray<uint>("I");
        if (r.V >= 0x05000001) BlockTypeIndex = r.ReadPArray<ushort>("H", NumBlocks);
        if (r.V >= 0x14020005) BlockSize = r.ReadPArray<uint>("I", NumBlocks);
        if (r.V >= 0x14010001) {
            NumStrings = r.ReadUInt32();
            MaxStringLength = r.ReadUInt32();
            Strings = r.ReadFArray(z => r.ReadL32AString(), NumStrings);
        }
        if (r.V >= 0x05000006) Groups = r.ReadL32PArray<uint>("I");
        // read blocks
        Blocks = new NiObject[NumBlocks];
        if (r.V >= 0x05000001) for (var i = 0; i < NumBlocks; i++) Blocks[i] = NiObject.Read(r, BlockTypes[BlockTypeIndex[i]]);
        else for (var i = 0; i < NumBlocks; i++) Blocks[i] = NiObject.Read(r, X.String(r));
        Roots = new Footer(r).Roots;
    }
    static NifReader() => X.Register();
}

/// <summary>
/// Tension, bias, continuity.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TBC(NifReader r) { // X
    public static (string, int) Struct = ("<3f", 12);
    public float t = r.ReadSingle();                    // Tension.
    public float b = r.ReadSingle();                    // Bias.
    public float c = r.ReadSingle();                    // Continuity.
}

/// <summary>
/// A generic key with support for interpolation. Type 1 is normal linear interpolation, type 2 has forward and backward tangents, and type 3 has tension, bias and continuity arguments. Note that color4 and byte always seem to be of type 1.
/// </summary>
public class Key<T> { // X
    public float Time;                                  // Time of the key.
    public T Value;                                     // The key value.
    public T Forward;                                   // Key forward tangent.
    public T Backward;                                  // The key backward tangent.
    public TBC TBC;                                     // The TBC of the key.

    public Key(NifReader r, KeyType keyType) {
        Time = r.ReadSingle();
        Value = Y<T>.Read(r);
        if (keyType == KeyType.QUADRATIC_KEY) {
            Forward = Y<T>.Read(r);
            Backward = Y<T>.Read(r);
        }
        else if (keyType == KeyType.TBC_KEY) TBC = r.ReadS<TBC>();
    }
}

/// <summary>
/// Array of vector keys (anything that can be interpolated, except rotations).
/// </summary>
public class KeyGroup<T> { // X
    public uint NumKeys;                                // Number of keys in the array.
    public KeyType Interpolation;                       // The key type.
    public Key<T>[] Keys;                               // The keys.

    public KeyGroup(NifReader r) {
        NumKeys = r.ReadUInt32();
        if (NumKeys != 0) Interpolation = (KeyType)r.ReadUInt32();
        Keys = r.ReadFArray(z => new Key<T>(r, Interpolation), NumKeys);
    }
}

/// <summary>
/// A special version of the key type used for quaternions.  Never has tangents.
/// </summary>
public class QuatKey<T> { // X
    public float Time;                                  // Time the key applies.
    public T Value;                                     // Value of the key.
    public TBC TBC;                                     // The TBC of the key.

    public QuatKey(NifReader r, KeyType keyType) {
        if (r.V <= 0x0A010000) Time = r.ReadSingle();
        if (r.V >= 0x0A01006A && keyType != KeyType.XYZ_ROTATION_KEY) {
            if (r.V >= 0x0A01006A) Time = r.ReadSingle();
            Value = Y<T>.Read(r);
        }
        if (keyType == KeyType.TBC_KEY) TBC = r.ReadS<TBC>();
    }
}

/// <summary>
/// Texture coordinates (u,v). As in OpenGL; image origin is in the lower left corner.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TexCoord { // X
    public static (string, int) Struct = ("<2f", 8);
    public float u;                                     // First coordinate.
    public float v;                                     // Second coordinate.

    public TexCoord(NifReader r) {
        u = r.ReadSingle();
        v = r.ReadSingle();
    }
    public TexCoord(double u, double v) { this.u = (float)u; this.v = (float)v; }
    public TexCoord(NifReader r, bool half) { u = half ? r.ReadHalf() : r.ReadSingle(); v = half ? r.ReadHalf() : r.ReadSingle(); }
}

/// <summary>
/// Describes the order of scaling and rotation matrices. Translate, Scale, Rotation, Center are from TexDesc.
/// Back = inverse of Center. FromMaya = inverse of the V axis with a positive translation along V of 1 unit.
/// </summary>
public enum TransformMethod : uint { // X
    Maya_Deprecated = 0,            // Center * Rotation * Back * Translate * Scale
    Max = 1,                        // Center * Scale * Rotation * Translate * Back
    Maya = 2                        // Center * Rotation * Back * FromMaya * Translate * Scale
}

/// <summary>
/// NiTexturingProperty::Map. Texture description.
/// </summary>
public class TexDesc { // X
    public Ref<NiImage> Image;                          // Link to the texture image.
    public Ref<NiSourceTexture> Source;                 // NiSourceTexture object index.
    public TexClampMode ClampMode = TexClampMode.WRAP_S_WRAP_T; // 0=clamp S clamp T, 1=clamp S wrap T, 2=wrap S clamp T, 3=wrap S wrap T
    public TexFilterMode FilterMode = TexFilterMode.FILTER_TRILERP; // 0=nearest, 1=bilinear, 2=trilinear, 3=..., 4=..., 5=...
    public Flags Flags;                                 // Texture mode flags; clamp and filter mode stored in upper byte with 0xYZ00 = clamp mode Y, filter mode Z.
    public ushort MaxAnisotropy;
    public uint UVSet = 0;                              // The texture coordinate set in NiGeometryData that this texture slot will use.
    public short PS2L = 0;                              // L can range from 0 to 3 and are used to specify how fast a texture gets blurry.
    public short PS2K = -75;                            // K is used as an offset into the mipmap levels and can range from -2047 to 2047. Positive values push the mipmap towards being blurry and negative values make the mipmap sharper.
    public ushort Unknown1;                             // Unknown, 0 or 0x0101?
    // NiTextureTransform
    public TexCoord Translation;                        // The UV translation.
    public TexCoord Scale = new(1.0, 1.0);              // The UV scale.
    public float Rotation = 0.0f;                       // The W axis rotation in texture space.
    public TransformMethod TransformMethod = (TransformMethod)0; // Depending on the source, scaling can occur before or after rotation.
    public TexCoord Center;                             // The origin around which the texture rotates.

    public TexDesc(NifReader r) {
        if (r.V <= 0x03010000) Image = X<NiImage>.Ref(r);
        if (r.V >= 0x0303000D) Source = X<NiSourceTexture>.Ref(r);
        if (r.V <= 0x14000005) {
            ClampMode = (TexClampMode)r.ReadUInt32();
            FilterMode = (TexFilterMode)r.ReadUInt32();
        }
        if (r.V >= 0x14010003) Flags = (Flags)r.ReadUInt16();
        if (r.V >= 0x14050004) MaxAnisotropy = r.ReadUInt16();
        if (r.V <= 0x14000005) UVSet = r.ReadUInt32();
        if (r.V <= 0x0A040001) {
            PS2L = r.ReadInt16();
            PS2K = r.ReadInt16();
        }
        if (r.V <= 0x0401000C) Unknown1 = r.ReadUInt16();
        // NiTextureTransform
        if (r.V >= 0x0A010000 && r.ReadBool32()) {
            Translation = r.ReadS<TexCoord>();
            Scale = r.ReadS<TexCoord>();
            Rotation = r.ReadSingle();
            TransformMethod = (TransformMethod)r.ReadUInt32();
            Center = r.ReadS<TexCoord>();
        }
    }
}

/// <summary>
/// NiTexturingProperty::ShaderMap. Shader texture description.
/// </summary>
public class ShaderTexDesc { // Y
    public TexDesc Map;
    public uint MapID;                                  // Unique identifier for the Gamebryo shader system.

    public ShaderTexDesc(NifReader r) {
        if (r.ReadBool32()) {
            Map = new TexDesc(r);
            MapID = r.ReadUInt32();
        }
    }
}

/// <summary>
/// List of three vertex indices.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Triangle(NifReader r) { // X
    public static (string, int) Struct = ("<3H", 6);
    public ushort v1 = r.ReadUInt16();                  // First vertex index.
    public ushort v2 = r.ReadUInt16();                  // Second vertex index.
    public ushort v3 = r.ReadUInt16();                  // Third vertex index.
}

[Flags]
public enum VertexFlags : ushort { // Y
    Vertex = 1 << 4,
    UVs = 1 << 5,
    UVs_2 = 1 << 6,
    Normals = 1 << 7,
    Tangents = 1 << 8,
    Vertex_Colors = 1 << 9,
    Skinned = 1 << 10,
    Land_Data = 1 << 11,
    Eye_Data = 1 << 12,
    Instance = 1 << 13,
    Full_Precision = 1 << 14
}

public class BSVertexDataSSE { // Y
    public Vector3 Vertex;
    public float BitangentX;
    public int UnknownInt;
    public TexCoord UV;
    public Vector3<byte> Normal;
    public byte BitangentY;
    public Vector3<byte> Tangent;
    public byte BitangentZ;
    public Color4 VertexColors;
    public float[] BoneWeights;
    public byte[] BoneIndices;
    public float EyeData;

    public BSVertexDataSSE(NifReader r, uint ARG) {
        if (((ARG & 16) != 0)) Vertex = r.ReadVector3();
        if (((ARG & 16) != 0) && ((ARG & 256) != 0)) BitangentX = r.ReadSingle();
        if (((ARG & 16) != 0) && (ARG & 256) == 0) UnknownInt = r.ReadInt32();
        if (((ARG & 32) != 0)) UV = new TexCoord(r, true);
        if ((ARG & 128) != 0) {
            Normal = new Vector3<byte>(r.ReadByte(), r.ReadByte(), r.ReadByte());
            BitangentY = r.ReadByte();
        }
        if (((ARG & 128) != 0) && ((ARG & 256) != 0)) {
            Tangent = new Vector3<byte>(r.ReadByte(), r.ReadByte(), r.ReadByte());
            BitangentZ = r.ReadByte();
        }
        if ((ARG & 512) != 0) VertexColors = new Color4(r.ReadBytes(4));
        if ((ARG & 1024) != 0) {
            BoneWeights = [r.ReadHalf(), r.ReadHalf(), r.ReadHalf(), r.ReadHalf()];
            BoneIndices = r.ReadBytes(4);
        }
        if ((ARG & 4096) != 0) EyeData = r.ReadSingle();
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BSVertexDesc(NifReader r) { // Y
    public static (string, int) Struct = ("<5bHb", 8);
    public byte VF1 = r.ReadByte();
    public byte VF2 = r.ReadByte();
    public byte VF3 = r.ReadByte();
    public byte VF4 = r.ReadByte();
    public byte VF5 = r.ReadByte();
    public VertexFlags VertexAttributes = (VertexFlags)r.ReadUInt16();
    public byte VF8 = r.ReadByte();
}

/// <summary>
/// Skinning data for a submesh, optimized for hardware skinning. Part of NiSkinPartition.
/// </summary>
public class SkinPartition { // Y
    public ushort NumVertices;                          // Number of vertices in this submesh.
    public ushort NumTriangles;                         // Number of triangles in this submesh.
    public ushort NumBones;                             // Number of bones influencing this submesh.
    public ushort NumStrips;                            // Number of strips in this submesh (zero if not stripped).
    public ushort NumWeightsPerVertex;                  // Number of weight coefficients per vertex. The Gamebryo engine seems to work well only if this number is equal to 4, even if there are less than 4 influences per vertex.
    public ushort[] Bones;                              // List of bones.
    public ushort[] VertexMap;                          // Maps the weight/influence lists in this submesh to the vertices in the shape being skinned.
    public float[][] VertexWeights;                     // The vertex weights.
    public ushort[] StripLengths;                       // The strip lengths.
    public ushort[][] Strips;                           // The strips.
    public Triangle[] Triangles;                        // The triangles.
    public byte[][] BoneIndices;                        // Bone indices, they index into 'Bones'.
    public ushort UnknownShort;                         // Unknown
    public BSVertexDesc VertexDesc;
    public Triangle[] TrianglesCopy;

    public SkinPartition(NifReader r) {
        NumVertices = r.ReadUInt16();
        NumTriangles = (ushort)(NumVertices / 3); // calculated
        NumBones = r.ReadUInt16();
        NumStrips = r.ReadUInt16();
        NumWeightsPerVertex = r.ReadUInt16();
        Bones = r.ReadPArray<ushort>("H", NumBones);
        if (r.V <= 0x0A000102) {
            VertexMap = r.ReadPArray<ushort>("H", NumVertices);
            VertexWeights = r.ReadFArray(k => r.ReadPArray<float>("f", NumWeightsPerVertex), NumVertices);
            StripLengths = r.ReadPArray<ushort>("H", NumStrips);
            if (NumStrips != 0) Strips = r.ReadFArray((k, i) => r.ReadPArray<ushort>("H", StripLengths[i]), NumStrips);
            else Triangles = r.ReadSArray<Triangle>(NumTriangles);
        }
        else if (r.V >= 0x0A010000) {
            if (r.ReadBool32()) VertexMap = r.ReadPArray<ushort>("H", NumVertices);
            var HasVertexWeights = r.ReadUInt32();
            if (HasVertexWeights == 1) VertexWeights = r.ReadFArray(k => r.ReadPArray<float>("f", NumWeightsPerVertex), NumVertices);
            if (HasVertexWeights == 15) VertexWeights = r.ReadFArray(k => r.ReadFArray(z => r.ReadHalf(), NumWeightsPerVertex), NumVertices);
            StripLengths = r.ReadPArray<ushort>("H", NumStrips);
            if (r.ReadBool32()) {
                if (NumStrips != 0) Strips = r.ReadFArray((k, i) => r.ReadPArray<ushort>("H", StripLengths[i]), NumStrips);
                else Triangles = r.ReadSArray<Triangle>(NumTriangles);
            }
        }
        if (r.ReadBool32()) BoneIndices = r.ReadFArray(k => r.ReadBytes(NumWeightsPerVertex), NumVertices);
        if (r.UV2 > 34) UnknownShort = r.ReadUInt16();
        if (r.UV2 == 100) {
            VertexDesc = r.ReadS<BSVertexDesc>();
            TrianglesCopy = r.ReadSArray<Triangle>(NumTriangles);
        }
    }
}

/// <summary>
/// A plane.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NiPlane(NifReader r) { // Y
    public static (string, int) Struct = ("<4f", 16);
    public Vector3 Normal = r.ReadVector3();            // The plane normal.
    public float Constant = r.ReadSingle();             // The plane constant.
}

/// <summary>
/// A sphere.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NiBound(NifReader r) { // Y
    public static (string, int) Struct = ("<4f", 16);
    public Vector3 Center = r.ReadVector3();            // The sphere's center.
    public float Radius = r.ReadSingle();               // The sphere's radius.
}

public struct NiTransform(NifReader r) { // X
    public Matrix4x4 Rotation = r.ReadMatrix3x3As4x4(); // The rotation part of the transformation matrix.
    public Vector3 Translation = r.ReadVector3();       // The translation vector.
    public float Scale = r.ReadSingle();                // Scaling part (only uniform scaling is supported).
}

/// <summary>
/// Geometry morphing data component.
/// </summary>
public class Morph { // X
    public string FrameName;                            // Name of the frame.
    public KeyType Interpolation;                       // Unlike most objects, the presense of this value is not conditional on there being keys.
    public Key<float>[] Keys;                           // The morph key frames.
    public float LegacyWeight;
    public Vector3[] Vectors;                           // Morph vectors.

    public Morph(NifReader r, uint numVertices) {
        if (r.V >= 0x0A01006A) FrameName = X.String(r);
        if (r.V <= 0x0A010000) {
            var NumKeys = r.ReadUInt32();
            Interpolation = (KeyType)r.ReadUInt32();
            Keys = r.ReadFArray(z => new Key<float>(r, Interpolation), NumKeys);
        }
        if (r.V >= 0x0A010068 && r.V <= 0x14010002 && r.UV2 < 10) LegacyWeight = r.ReadSingle();
        Vectors = r.ReadPArray<Vector3>("3f", numVertices);
    }
}

/// <summary>
/// particle array entry
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Particle(NifReader r) { // X
    public static (string, int) Struct = ("<9f2H", 40);
    public Vector3 Velocity = r.ReadVector3();          // Particle velocity
    public Vector3 UnknownVector = r.ReadVector3();     // Unknown
    public float Lifetime = r.ReadSingle();             // The particle age.
    public float Lifespan = r.ReadSingle();             // Maximum age of the particle.
    public float Timestamp = r.ReadSingle();            // Timestamp of the last update.
    public ushort UnknownShort = r.ReadUInt16();        // Unknown short
    public ushort VertexID = r.ReadUInt16();            // Particle/vertex index matches array index
}

/// <summary>
/// NiSkinData::BoneData. Skinning data component.
/// </summary>
public class BoneData { // X
    public NiTransform SkinTransform;                   // Offset of the skin from this bone in bind position.
    public Vector3 BoundingSphereOffset;                // Translation offset of a bounding sphere holding all vertices. (Note that its a Sphere Containing Axis Aligned Box not a minimum volume Sphere)
    public float BoundingSphereRadius;                  // Radius for bounding sphere holding all vertices.
    public short[] Unknown13Shorts;                     // Unknown, always 0?
    public BoneVertData[] VertexWeights;                // The vertex weights.

    public BoneData(NifReader r, int arg) {
        SkinTransform = r.ReadS<NiTransform>();
        BoundingSphereOffset = r.ReadVector3();
        BoundingSphereRadius = r.ReadSingle();
        if (r.V == 0x14030009 && (r.UV == 0x20000) || (r.UV == 0x30000)) Unknown13Shorts = r.ReadPArray<short>("h", 13);
        VertexWeights = r.V <= 0x04020100 ? r.ReadL16SArray<BoneVertData>()
            : r.V >= 0x04020200 && arg == 1 ? r.ReadL16SArray<BoneVertData>()
            : r.V >= 0x14030101 && arg == 15 ? r.ReadL16FArray(z => new BoneVertData(r, false)) : default;
    }
}

/// <summary>
/// Determines how the raw image data is stored in NiRawImageData.
/// </summary>
public enum ImageType : uint { // Y
    RGB = 1,                        // Colors store red, blue, and green components.
    RGBA = 2                        // Colors store red, blue, green, and alpha components.
}

/// <summary>
/// Box Bounding Volume
/// </summary>
public struct BoxBV(NifReader r) { // X
    public Vector3 Center = r.ReadVector3();
    public Matrix4x4 Axis = r.ReadMatrix3x3As4x4();
    public Vector3 Extent = r.ReadVector3();
}

/// <summary>
/// Capsule Bounding Volume
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CapsuleBV(NifReader r) { // Y
    public static (string, int) Struct = ("<8f", 32);
    public Vector3 Center = r.ReadVector3();
    public Vector3 Origin = r.ReadVector3();
    public float Extent = r.ReadSingle();
    public float Radius = r.ReadSingle();
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HalfSpaceBV(NifReader r) { // Y
    public static (string, int) Struct = ("<7f", 28);
    public NiPlane Plane = r.ReadS<NiPlane>();
    public Vector3 Center = r.ReadVector3();
}

public class BoundingVolume { // X
    public BoundVolumeType CollisionType;               // Type of collision data.
    public NiBound Sphere;
    public BoxBV Box;
    public CapsuleBV Capsule;
    public UnionBV Union;
    public HalfSpaceBV HalfSpace;

    public BoundingVolume(NifReader r) {
        CollisionType = (BoundVolumeType)r.ReadUInt32();
        switch (CollisionType) {
            case BoundVolumeType.SPHERE_BV: Sphere = r.ReadS<NiBound>(); break;
            case BoundVolumeType.BOX_BV: Box = r.ReadS<BoxBV>(); break;
            case BoundVolumeType.CAPSULE_BV: Capsule = r.ReadS<CapsuleBV>(); break;
            case BoundVolumeType.UNION_BV: Union = new UnionBV(r); break;
            case BoundVolumeType.HALFSPACE_BV: HalfSpace = r.ReadS<HalfSpaceBV>(); break;
        }
    }
}

public class UnionBV(NifReader r) { // Y
    public BoundingVolume[] BoundingVolumes = r.ReadL32FArray(z => new BoundingVolume(r));
}

public class MorphWeight(NifReader r) { // Y
    public Ref<NiInterpolator> Interpolator = X<NiInterpolator>.Ref(r);
    public float Weight = r.ReadSingle();
}

#endregion

#region NIF Objects

// These are the main units of data that NIF files are arranged in.
// They are like C classes and can contain many pieces of data.
// The only differences between these and compounds is that these are treated as object types by the NIF format and can inherit from other classes.

/// <summary>
/// Abstract object type.
/// </summary>
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
public abstract class NiObject(NifReader r) { // X

    public static NiObject Read(NifReader r, string nodeType) {
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

/// <summary>
/// LEGACY (pre-10.1). Abstract base class for particle system modifiers.
/// </summary>
public abstract class NiParticleModifier : NiObject { // X
    public Ref<NiParticleModifier> NextModifier;        // Next particle modifier.
    public Ref<NiParticleSystemController> Controller;  // Points to the particle system controller parent.

    public NiParticleModifier(NifReader r) : base(r) {
        NextModifier = X<NiParticleModifier>.Ref(r);
        if (r.V >= 0x04000002) Controller = X<NiParticleSystemController>.Ptr(r);
    }
}

/// <summary>
/// A generic extra data object.
/// </summary>
public class NiExtraData : NiObject { // X
    public string Name;                                 // Name of this object.
    public Ref<NiExtraData> NextExtraData;              // Block number of the next extra data object.

    public NiExtraData(NifReader r) : base(r) {
        if (r.V >= 0x0A000100 && true) Name = X.String(r);
        if (r.V <= 0x04020200) NextExtraData = X<NiExtraData>.Ref(r);
    }
}

/// <summary>
/// Abstract base class for all interpolators of bool, float, NiQuaternion, NiPoint3, NiColorA, and NiQuatTransform data.
/// </summary>
public abstract class NiInterpolator(NifReader r) : NiObject(r) { // Y
}

/// <summary>
/// Abstract base class for NiObjects that support names, extra data, and time controllers.
/// </summary>
public abstract class NiObjectNET : NiObject { // X
    public BSLightingShaderPropertyShaderType SkyrimShaderType; // Configures the main shader path
    public string Name;                                 // Name of this controllable object, used to refer to the object in .kf files.
    public string OldExtraPropName;                     // (=NiStringExtraData)
    public uint OldExtraInternalId;                     // ref
    public string OldExtraString;                       // Extra string data.
    public byte UnknownByte;                            // Always 0.
    public Ref<NiExtraData> ExtraData;                  // Extra data object index. (The first in a chain)
    public Ref<NiExtraData>[] ExtraDataList;            // List of extra data indices.
    public Ref<NiTimeController> Controller;            // Controller object index. (The first in a chain)

    public NiObjectNET(NifReader r) : base(r) {
        if (r.UV2 >= 83 && false) SkyrimShaderType = (BSLightingShaderPropertyShaderType)r.ReadUInt32();
        Name = X.String(r);
        if (r.V <= 0x02030000) {
            if (r.ReadBool32()) OldExtraPropName = X.String(r);
            if (r.ReadBool32()) OldExtraInternalId = r.ReadUInt32();
            if (r.ReadBool32()) OldExtraString = X.String(r);
            UnknownByte = r.ReadByte();
        }
        if (r.V >= 0x03000000 && r.V <= 0x04020200) ExtraData = X<NiExtraData>.Ref(r);
        if (r.V >= 0x0A000100) ExtraDataList = r.ReadL32FArray(X<NiExtraData>.Ref);
        if (r.V >= 0x03000000) Controller = X<NiTimeController>.Ref(r);
    }
}

/// <summary>
/// This is the most common collision object found in NIF files. It acts as a real object that
/// is visible and possibly (if the body allows for it) interactive. The node itself
/// is simple, it only has three properties.
/// For this type of collision object, bhkRigidBody or bhkRigidBodyT is generally used.
/// </summary>
public class NiCollisionObject : NiObject { // Y
    public Ref<NiAVObject> Target;                      // Index of the AV object referring to this collision object.

    public NiCollisionObject(NifReader r) : base(r) {
        Target = X<NiAVObject>.Ptr(r);
    }
}

/// <summary>
/// Abstract audio-visual base class from which all of Gamebryo's scene graph objects inherit.
/// </summary>
public abstract class NiAVObject : NiObjectNET { // X
    public Flags Flags = (Flags)14;                     // Basic flags for AV objects. For Bethesda streams above 26 only.
    public Vector3 Translation;                         // The translation vector.
    public Matrix4x4 Rotation;                          // The rotation part of the transformation matrix.
    public float Scale = 1.0f;                          // Scaling part (only uniform scaling is supported).
    public Vector3 Velocity;                            // Unknown function. Always seems to be (0, 0, 0)
    public Ref<NiProperty>[] Properties;                // All rendering properties attached to this object.
    public uint[] Unknown1;                             // Always 2,0,2,0.
    public byte Unknown2;                               // 0 or 1.
    public BoundingVolume BoundingVolume;
    public Ref<NiCollisionObject> CollisionObject;

    public NiAVObject(NifReader r) : base(r) {
        Flags = r.UV2 > 26 ? (Flags)r.ReadUInt16()
            : r.V >= 0x03000000 && (r.UV2 <= 26) ? (Flags)r.ReadUInt16() : (Flags)14;
        Translation = r.ReadVector3();
        Rotation = r.ReadMatrix3x3As4x4();
        Scale = r.ReadSingle();
        if (r.V <= 0x04020200) Velocity = r.ReadVector3();
        if (r.UV2 <= 34) Properties = r.ReadL32FArray(X<NiProperty>.Ref);
        if (r.V <= 0x02030000) {
            Unknown1 = r.ReadPArray<uint>("I", 4);
            Unknown2 = r.ReadByte();
        }
        if (r.V >= 0x03000000 && r.V <= 0x04020200 && r.ReadBool32()) BoundingVolume = new BoundingVolume(r);
        if (r.V >= 0x0A000100) CollisionObject = X<NiCollisionObject>.Ref(r);
    }
}

/// <summary>
/// Abstract base class for dynamic effects such as NiLights or projected texture effects.
/// </summary>
public abstract class NiDynamicEffect : NiAVObject { // X
    public bool SwitchState = true;                     // If true, then the dynamic effect is applied to affected nodes during rendering.
    public Ref<NiNode>[] AffectedNodes;                 // If a node appears in this list, then its entire subtree will be affected by the effect.
    public uint[] AffectedNodePointers;                 // As of 4.0 the pointer hash is no longer stored alongside each NiObject on disk, yet this node list still refers to the pointer hashes. Cannot leave the type as Ptr because the link will be invalid.

    public NiDynamicEffect(NifReader r) : base(r) {
        if (r.V >= 0x0A01006A && r.UV2 < 130) SwitchState = r.ReadBool32();
        if (r.V <= 0x0303000D) AffectedNodes = r.ReadL32FArray(X<NiNode>.Ptr);
        else if (r.V >= 0x04000000 && r.V <= 0x04000002) AffectedNodePointers = r.ReadL32PArray<uint>("I");
        else if (r.V >= 0x0A010000 && r.UV2 < 130) AffectedNodes = r.ReadL32FArray(X<NiNode>.Ptr);
    }
}

/// <summary>
/// Abstract base class representing all rendering properties. Subclasses are attached to NiAVObjects to control their rendering.
/// </summary>
public abstract class NiProperty(NifReader r) : NiObjectNET(r) { // X
}

/// <summary>
/// Abstract base class that provides the base timing and update functionality for all the Gamebryo animation controllers.
/// </summary>
public abstract class NiTimeController : NiObject { // X
    public Ref<NiTimeController> NextController;        // Index of the next controller.
    public Flags Flags;                                 // Controller flags.
                                                        //     Bit 0 : Anim type, 0=APP_TIME 1=APP_INIT
                                                        //     Bit 1-2 : Cycle type, 00=Loop 01=Reverse 10=Clamp
                                                        //     Bit 3 : Active
                                                        //     Bit 4 : Play backwards
                                                        //     Bit 5 : Is manager controlled
                                                        //     Bit 6 : Always seems to be set in Skyrim and Fallout NIFs, unknown function
    public float Frequency = 1.0f;                      // Frequency (is usually 1.0).
    public float Phase;                                 // Phase (usually 0.0).
    public float StartTime = 3.402823466e+38f;          // Controller start time.
    public float StopTime = -3.402823466e+38f;          // Controller stop time.
    public Ref<NiObjectNET> Target;                     // Controller target (object index of the first controllable ancestor of this object).
    public uint UnknownInteger;                         // Unknown integer.

    public NiTimeController(NifReader r) : base(r) {
        NextController = X<NiTimeController>.Ref(r);
        Flags = (Flags)r.ReadUInt16();
        Frequency = r.ReadSingle();
        Phase = r.ReadSingle();
        StartTime = r.ReadSingle();
        StopTime = r.ReadSingle();
        if (r.V >= 0x0303000D) Target = X<NiObjectNET>.Ptr(r);
        else if (r.V <= 0x03010000) UnknownInteger = r.ReadUInt32();
    }
}

/// <summary>
/// Abstract base class for all NiTimeController objects using NiInterpolator objects to animate their target objects.
/// </summary>
public abstract class NiInterpController : NiTimeController { // X
    public bool ManagerControlled;

    public NiInterpController(NifReader r) : base(r) {
        if (r.V >= 0x0A010068 && r.V <= 0x0A01006C) ManagerControlled = r.ReadBool32();
    }
}

/// <summary>
/// DEPRECATED (20.5), replaced by NiMorphMeshModifier.
/// Time controller for geometry morphing.
/// </summary>
public class NiGeomMorpherController : NiInterpController { // X
    public Flags ExtraFlags;                            // 1 = UPDATE NORMALS
    public Ref<NiMorphData> Data;                       // Geometry morphing data index.
    public byte AlwaysUpdate;
    public Ref<NiInterpolator>[] Interpolators;
    public MorphWeight[] InterpolatorWeights;
    public uint[] UnknownInts;                          // Unknown.

    public NiGeomMorpherController(NifReader r) : base(r) {
        if (r.V >= 0x0A000102) ExtraFlags = (Flags)r.ReadUInt16();
        Data = X<NiMorphData>.Ref(r);
        if (r.V >= 0x04000001) AlwaysUpdate = r.ReadByte();
        if (r.V >= 0x0A01006A && r.V <= 0x14000005) Interpolators = r.ReadL32FArray(X<NiInterpolator>.Ref);
        else if (r.V >= 0x14010003) InterpolatorWeights = r.ReadL32FArray(z => new MorphWeight(r));
        if (r.V >= 0x0A020000 && r.V <= 0x14000005 && (r.UV2 > 9)) UnknownInts = r.ReadL32PArray<uint>("I");
    }
}

/// <summary>
/// Uses a single NiInterpolator to animate its target value.
/// </summary>
public abstract class NiSingleInterpController : NiInterpController { // X
    public Ref<NiInterpolator> Interpolator;

    public NiSingleInterpController(NifReader r) : base(r) {
        if (r.V >= 0x0A010068) Interpolator = X<NiInterpolator>.Ref(r);
    }
}

/// <summary>
/// DEPRECATED (10.2), RENAMED (10.2) to NiTransformController
/// A time controller object for animation key frames.
/// </summary>
public class NiKeyframeController : NiSingleInterpController { // X
    public Ref<NiKeyframeData> Data;

    public NiKeyframeController(NifReader r) : base(r) {
        if (r.V <= 0x0A010067) Data = X<NiKeyframeData>.Ref(r);
    }
}

/// <summary>
/// Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target float value.
/// </summary>
public abstract class NiFloatInterpController(NifReader r) : NiSingleInterpController(r) { // X
}

/// <summary>
/// Animates the alpha value of a property using an interpolator.
/// </summary>
public class NiAlphaController : NiFloatInterpController { // X
    public Ref<NiFloatData> Data;

    public NiAlphaController(NifReader r) : base(r) {
        if (r.V <= 0x0A010067) Data = X<NiFloatData>.Ref(r);
    }
}

/// <summary>
/// Abstract base class for all NiInterpControllers that use a NiInterpolator to animate their target boolean value.
/// </summary>
public abstract class NiBoolInterpController(NifReader r) : NiSingleInterpController(r) { // X
}

/// <summary>
/// Animates the visibility of an NiAVObject.
/// </summary>
public class NiVisController : NiBoolInterpController { // X
    public Ref<NiVisData> Data;

    public NiVisController(NifReader r) : base(r) {
        if (r.V <= 0x0A010067) Data = X<NiVisData>.Ref(r);
    }
}

/// <summary>
/// Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target NiPoint3 value.
/// </summary>
public abstract class NiPoint3InterpController(NifReader r) : NiSingleInterpController(r) { // X
}

/// <summary>
/// Time controller for material color. Flags are used for color selection in versions below 10.1.0.0.
/// Bits 4-5: Target Color (00 = Ambient, 01 = Diffuse, 10 = Specular, 11 = Emissive)
/// NiInterpController::GetCtlrID() string formats:
///     ['AMB', 'DIFF', 'SPEC', 'SELF_ILLUM'] (Depending on "Target Color")
/// </summary>
public class NiMaterialColorController : NiPoint3InterpController { // X
    public MaterialColor TargetColor;                   // Selects which color to control.
    public Ref<NiPosData> Data;

    public NiMaterialColorController(NifReader r) : base(r) {
        if (r.V >= 0x0A010000) TargetColor = (MaterialColor)r.ReadUInt16();
        if (r.V <= 0x0A010067) Data = X<NiPosData>.Ref(r);
    }
}

public class MaterialData { // Y
    public string ShaderName;                           // The shader name.
    public int ShaderExtraData;                         // Extra data associated with the shader. A value of -1 means the shader is the default implementation.
    public uint NumMaterials;
    public string[] MaterialName;                       // The name of the material.
    public int[] MaterialExtraData;                     // Extra data associated with the material. A value of -1 means the material is the default implementation.
    public int ActiveMaterial = -1;                     // The index of the currently active material.
    public byte UnknownByte = 255;                      // Cyanide extension (only in version 10.2.0.0?).
    public int UnknownInteger2;                         // Unknown.
    // Whether the materials for this object always needs to be updated before rendering with them.

    public MaterialData(NifReader r) {
        ShaderExtraData = r.ReadInt32();
        if (r.V >= 0x14020005) {
            NumMaterials = r.ReadUInt32();
            MaterialName = r.ReadFArray(z => X.String(r), NumMaterials);
            MaterialExtraData = r.ReadPArray<int>("i", NumMaterials);
            ActiveMaterial = r.ReadInt32();
        }
        if (r.V == 0x0A020000 && (r.UV == 1)) UnknownByte = r.ReadByte();
        if (r.V == 0x0A040001) UnknownInteger2 = r.ReadInt32();
        if (r.V >= 0x0A000100 && r.V <= 0x14010003 && r.ReadBool32()) {
            ShaderName = X.String(r);
            ShaderExtraData = r.ReadInt32();
        }
    }
}

/// <summary>
/// Describes a visible scene element with vertices like a mesh, a particle system, lines, etc.
/// </summary>
public abstract class NiGeometry : NiAVObject { // X
    public NiBound Bound;
    public Ref<NiObject> Skin;
    public Ref<NiGeometryData> Data;                    // Data index (NiTriShapeData/NiTriStripData).
    public Ref<NiSkinInstance> SkinInstance;
    public MaterialData MaterialData;
    public Ref<BSShaderProperty> ShaderProperty;
    public Ref<NiAlphaProperty> AlphaProperty;

    public NiGeometry(NifReader r) : base(r) {
        var NiParticleSystem = false;
        if ((r.UV2 >= 100) && NiParticleSystem) {
            Bound = r.ReadS<NiBound>();
            Skin = X<NiObject>.Ref(r);
        }
        if (r.UV2 < 100) Data = X<NiGeometryData>.Ref(r);
        if ((r.UV2 >= 100) && !NiParticleSystem) Data = X<NiGeometryData>.Ref(r);
        if (r.V >= 0x0303000D && (r.UV2 < 100)) SkinInstance = X<NiSkinInstance>.Ref(r);
        if ((r.UV2 >= 100) && !NiParticleSystem) SkinInstance = X<NiSkinInstance>.Ref(r);
        if (r.V >= 0x0A000100 && (r.UV2 < 100)) MaterialData = new MaterialData(r);
        if (r.V >= 0x0A000100 && (r.UV2 >= 100) && !NiParticleSystem) MaterialData = new MaterialData(r);
        if (r.V >= 0x14020007 && (r.UV == 12)) {
            ShaderProperty = X<BSShaderProperty>.Ref(r);
            AlphaProperty = X<NiAlphaProperty>.Ref(r);
        }
    }
}

/// <summary>
/// Describes a mesh, built from triangles.
/// </summary>
public abstract class NiTriBasedGeom(NifReader r) : NiGeometry(r) { // X
}

[Flags]
public enum VectorFlags : ushort { // Y
    UV_1 = 0,
    UV_2 = 1 << 1,
    UV_4 = 1 << 2,
    UV_8 = 1 << 3,
    UV_16 = 1 << 4,
    UV_32 = 1 << 5,
    Unk64 = 1 << 6,
    Unk128 = 1 << 7,
    Unk256 = 1 << 8,
    Unk512 = 1 << 9,
    Unk1024 = 1 << 10,
    Unk2048 = 1 << 11,
    Has_Tangents = 1 << 12,
    Unk8192 = 1 << 13,
    Unk16384 = 1 << 14,
    Unk32768 = 1 << 15
}

[Flags]
public enum BSVectorFlags : ushort { // Y
    Has_UV = 0,
    Unk2 = 1 << 1,
    Unk4 = 1 << 2,
    Unk8 = 1 << 3,
    Unk16 = 1 << 4,
    Unk32 = 1 << 5,
    Unk64 = 1 << 6,
    Unk128 = 1 << 7,
    Unk256 = 1 << 8,
    Unk512 = 1 << 9,
    Unk1024 = 1 << 10,
    Unk2048 = 1 << 11,
    Has_Tangents = 1 << 12,
    Unk8192 = 1 << 13,
    Unk16384 = 1 << 14,
    Unk32768 = 1 << 15
}

/// <summary>
/// Mesh data: vertices, vertex normals, etc.
/// </summary>
public abstract class NiGeometryData : NiObject { // X
    public int GroupID;                                 // Always zero.
    public ushort NumVertices;                          // Number of vertices.
    public ushort BSMaxVertices;                        // Bethesda uses this for max number of particles in NiPSysData.
    public byte KeepFlags;                              // Used with NiCollision objects when OBB or TRI is set.
    public byte CompressFlags;                          // Unknown.
    public Vector3[] Vertices;                          // The mesh vertices.
    public VectorFlags VectorFlags;
    public BSVectorFlags BSVectorFlags;
    public uint MaterialCRC;
    public Vector3[] Normals;                           // The lighting normals.
    public Vector3[] Tangents;                          // Tangent vectors.
    public Vector3[] Bitangents;                        // Bitangent vectors.
    public float[] UnkFloats;
    public Vector3 Center;                              // Center of the bounding box (smallest box that contains all vertices) of the mesh.
    public float Radius;                                // Radius of the mesh: maximal Euclidean distance between the center and all vertices.
    public short[] Unknown13shorts;                     // Unknown, always 0?
    public Color4[] VertexColors;                       // The vertex colors.
    public ushort NumUVSets;                            // The lower 6 (or less?) bits of this field represent the number of UV texture sets. The other bits are probably flag bits. For versions 10.1.0.0 and up, if bit 12 is set then extra vectors are present after the normals.
    public bool HasUV;                                  // Do we have UV coordinates?
                                                        // 
                                                        //     Note: for compatibility with NifTexture, set this value to either 0x00000000 or 0xFFFFFFFF.
    public TexCoord[][] UVSets;                         // The UV texture coordinates. They follow the OpenGL standard: some programs may require you to flip the second coordinate.
    public ConsistencyType ConsistencyFlags = ConsistencyType.CT_MUTABLE; // Consistency Flags
    public Ref<AbstractAdditionalGeometryData> AdditionalData; // Unknown.

    public NiGeometryData(NifReader r) : base(r) {
        if (r.V >= 0x0A010072) GroupID = r.ReadInt32();
        if (!false || r.UV2 >= 34) NumVertices = r.ReadUInt16();
        if ((r.UV2 >= 34) && false) BSMaxVertices = r.ReadUInt16();
        if (r.V >= 0x0A010000) {
            KeepFlags = r.ReadByte();
            CompressFlags = r.ReadByte();
        }
        var HasVertices = r.ReadUInt32();
        if ((HasVertices > 0) && (HasVertices != 15)) Vertices = r.ReadPArray<Vector3>("3f", NumVertices);
        if (r.V >= 0x14030101 && HasVertices == 15) Vertices = r.ReadFArray(z => new Vector3(r.ReadHalf(), r.ReadHalf(), r.ReadHalf()), NumVertices);
        if (r.V >= 0x0A000100 && !((r.V == 0x14020007) && (r.UV2 > 0))) VectorFlags = (VectorFlags)r.ReadUInt16();
        if (((r.V == 0x14020007) && (r.UV2 > 0))) BSVectorFlags = (BSVectorFlags)r.ReadUInt16();
        if (r.V == 0x14020007 && (r.UV == 12)) MaterialCRC = r.ReadUInt32();
        var HasNormals = r.ReadUInt32();
        if ((HasNormals > 0) && (HasNormals != 6)) Normals = r.ReadPArray<Vector3>("3f", NumVertices);
        if (r.V >= 0x14030101 && HasNormals == 6) Normals = r.ReadFArray(z => new Vector3(r.ReadByte(), r.ReadByte(), r.ReadByte()), NumVertices);
        if (r.V >= 0x0A010000 && (HasNormals != 0) && (((int)VectorFlags | (int)BSVectorFlags) & 4096) != 0) {
            Tangents = r.ReadPArray<Vector3>("3f", NumVertices);
            Bitangents = r.ReadPArray<Vector3>("3f", NumVertices);
        }
        if (r.V == 0x14030009 && (r.UV == 0x20000) || (r.UV == 0x30000) && r.ReadBool32()) UnkFloats = r.ReadPArray<float>("f", NumVertices);
        Center = r.ReadVector3();
        Radius = r.ReadSingle();
        if (r.V == 0x14030009 && (r.UV == 0x20000) || (r.UV == 0x30000)) Unknown13shorts = r.ReadPArray<short>("h", 13);
        var HasVertexColors = r.ReadUInt32();
        if ((HasVertexColors > 0) && (HasVertexColors != 7)) VertexColors = r.ReadFArray(z => new Color4(r), NumVertices);
        if (r.V >= 0x14030101 && HasVertexColors == 7) VertexColors = r.ReadFArray(z => new Color4(r.ReadBytes(4)), NumVertices);
        if (r.V <= 0x04020200) NumUVSets = r.ReadUInt16();
        if (r.V <= 0x04000002) HasUV = r.ReadBool32();
        if ((HasVertices > 0) && (HasVertices != 15)) UVSets = r.ReadFArray(k => r.ReadSArray<TexCoord>(NumVertices), ((NumUVSets & 63) | ((int)VectorFlags & 63) | ((int)BSVectorFlags & 1)));
        if (r.V >= 0x14030101 && HasVertices == 15) UVSets = r.ReadFArray(k => r.ReadFArray(z => new TexCoord(r, true), NumVertices), ((NumUVSets & 63) | ((int)VectorFlags & 63) | ((int)BSVectorFlags & 1)));
        if (r.V >= 0x0A000100) ConsistencyFlags = (ConsistencyType)r.ReadUInt16();
        if (r.V >= 0x14000004) AdditionalData = X<AbstractAdditionalGeometryData>.Ref(r);
    }
}

public abstract class AbstractAdditionalGeometryData(NifReader r) : NiObject(r) { // Y
}

/// <summary>
/// Describes a mesh, built from triangles.
/// </summary>
public abstract class NiTriBasedGeomData : NiGeometryData { // X
    public ushort NumTriangles;                         // Number of triangles.

    public NiTriBasedGeomData(NifReader r) : base(r) {
        NumTriangles = r.ReadUInt16();
    }
}

/// <summary>
/// Transparency. Flags 0x00ED.
/// </summary>
public class NiAlphaProperty : NiProperty { // X
    public Flags Flags = (Flags)4844;                   // Bit 0 : alpha blending enable
                                                        //     Bits 1-4 : source blend mode
                                                        //     Bits 5-8 : destination blend mode
                                                        //     Bit 9 : alpha test enable
                                                        //     Bit 10-12 : alpha test mode
                                                        //     Bit 13 : no sorter flag ( disables triangle sorting )
                                                        // 
                                                        //     blend modes (glBlendFunc):
                                                        //     0000 GL_ONE
                                                        //     0001 GL_ZERO
                                                        //     0010 GL_SRC_COLOR
                                                        //     0011 GL_ONE_MINUS_SRC_COLOR
                                                        //     0100 GL_DST_COLOR
                                                        //     0101 GL_ONE_MINUS_DST_COLOR
                                                        //     0110 GL_SRC_ALPHA
                                                        //     0111 GL_ONE_MINUS_SRC_ALPHA
                                                        //     1000 GL_DST_ALPHA
                                                        //     1001 GL_ONE_MINUS_DST_ALPHA
                                                        //     1010 GL_SRC_ALPHA_SATURATE
                                                        // 
                                                        //     test modes (glAlphaFunc):
                                                        //     000 GL_ALWAYS
                                                        //     001 GL_LESS
                                                        //     010 GL_EQUAL
                                                        //     011 GL_LEQUAL
                                                        //     100 GL_GREATER
                                                        //     101 GL_NOTEQUAL
                                                        //     110 GL_GEQUAL
                                                        //     111 GL_NEVER
    public byte Threshold = 128;                        // Threshold for alpha testing (see: glAlphaFunc)
    public ushort UnknownShort1;                        // Unknown
    public uint UnknownInt2;                            // Unknown

    public NiAlphaProperty(NifReader r) : base(r) {
        Flags = (Flags)r.ReadUInt16();
        Threshold = r.ReadByte();
        if (r.V <= 0x02030000) UnknownShort1 = r.ReadUInt16();
        if (r.V >= 0x14030101 && r.V <= 0x14030102) UnknownShort1 = r.ReadUInt16();
        if (r.V <= 0x02030000) UnknownInt2 = r.ReadUInt32();
    }
}

/// <summary>
/// Generic rotating particles data object.
/// </summary>
public class NiParticlesData : NiGeometryData { // X
    public ushort NumParticles;                         // The maximum number of particles (matches the number of vertices).
    public float ParticleRadius;                        // The particles' size.
    public float[] Radii;                               // The individual particle sizes.
    public ushort NumActive;                            // The number of active particles at the time the system was saved. This is also the number of valid entries in the following arrays.
    public float[] Sizes;                               // The individual particle sizes.
    public Quaternion[] Rotations;                      // The individual particle rotations.
    public float[] RotationAngles;                      // Angles of rotation
    public Vector3[] RotationAxes;                      // Axes of rotation.
    public bool HasTextureIndices;
    public uint NumSubtextureOffsets;                   // How many quads to use in BSPSysSubTexModifier for texture atlasing
    public Vector4[] SubtextureOffsets;                 // Defines UV offsets
    public float AspectRatio;                           // Sets aspect ratio for Subtexture Offset UV quads
    public ushort AspectFlags;
    public float SpeedtoAspectAspect2;
    public float SpeedtoAspectSpeed1;
    public float SpeedtoAspectSpeed2;

    public NiParticlesData(NifReader r) : base(r) {
        if (r.V <= 0x04000002) NumParticles = r.ReadUInt16();
        if (r.V <= 0x0A000100) ParticleRadius = r.ReadSingle();
        if (r.V >= 0x0A010000 && !((r.V == 0x14020007) && (r.UV2 > 0)) && r.ReadBool32()) Radii = r.ReadPArray<float>("f", NumVertices);
        NumActive = r.ReadUInt16();
        if (!((r.V == 0x14020007) && (r.UV2 > 0)) && r.ReadBool32()) Sizes = r.ReadPArray<float>("f", NumVertices);
        if (r.V >= 0x0A000100 && !((r.V == 0x14020007) && (r.UV2 > 0)) && r.ReadBool32()) Rotations = r.ReadFArray(z => r.ReadQuaternionWFirst(), NumVertices);
        if (!((r.V == 0x14020007) && (r.UV2 > 0)) && r.ReadBool32()) RotationAngles = r.ReadPArray<float>("f", NumVertices);
        if (r.V >= 0x14000004 && !((r.V == 0x14020007) && (r.UV2 > 0)) && r.ReadBool32()) RotationAxes = r.ReadPArray<Vector3>("3f", NumVertices);
        if (((r.V == 0x14020007) && (r.UV2 > 0))) HasTextureIndices = r.ReadBool32();
        if (r.UV2 > 34) NumSubtextureOffsets = r.ReadUInt32();
        if (((r.V == 0x14020007) && (r.UV2 > 0))) SubtextureOffsets = r.ReadL8PArray<Vector4>("4f");
        if (r.UV2 > 34) {
            AspectRatio = r.ReadSingle();
            AspectFlags = r.ReadUInt16();
            SpeedtoAspectAspect2 = r.ReadSingle();
            SpeedtoAspectSpeed1 = r.ReadSingle();
            SpeedtoAspectSpeed2 = r.ReadSingle();
        }
    }
}

/// <summary>
/// Rotating particles data object.
/// </summary>
public class NiRotatingParticlesData : NiParticlesData { // X
    public Quaternion[] Rotations2;                     // The individual particle rotations.

    public NiRotatingParticlesData(NifReader r) : base(r) {
        if (r.V <= 0x04020200 && r.ReadBool32()) Rotations2 = r.ReadFArray(z => r.ReadQuaternionWFirst(), NumVertices);
    }
}

/// <summary>
/// Particle system data object (with automatic normals?).
/// </summary>
public class NiAutoNormalParticlesData(NifReader r) : NiParticlesData(r) { // X
}

/// <summary>
/// Camera object.
/// </summary>
public class NiCamera : NiAVObject { // X
    public ushort CameraFlags;                          // Obsolete flags.
    public float FrustumLeft;                           // Frustrum left.
    public float FrustumRight;                          // Frustrum right.
    public float FrustumTop;                            // Frustrum top.
    public float FrustumBottom;                         // Frustrum bottom.
    public float FrustumNear;                           // Frustrum near.
    public float FrustumFar;                            // Frustrum far.
    public bool UseOrthographicProjection;              // Determines whether perspective is used.  Orthographic means no perspective.
    public float ViewportLeft;                          // Viewport left.
    public float ViewportRight;                         // Viewport right.
    public float ViewportTop;                           // Viewport top.
    public float ViewportBottom;                        // Viewport bottom.
    public float LODAdjust;                             // Level of detail adjust.
    public Ref<NiAVObject> Scene;
    public uint NumScreenPolygons = 0;                  // Deprecated. Array is always zero length on disk write.
    public uint NumScreenTextures = 0;                  // Deprecated. Array is always zero length on disk write.
    public uint UnknownInt3;                            // Unknown.

    public NiCamera(NifReader r) : base(r) {
        if (r.V >= 0x0A010000) CameraFlags = r.ReadUInt16();
        FrustumLeft = r.ReadSingle();
        FrustumRight = r.ReadSingle();
        FrustumTop = r.ReadSingle();
        FrustumBottom = r.ReadSingle();
        FrustumNear = r.ReadSingle();
        FrustumFar = r.ReadSingle();
        if (r.V >= 0x0A010000) UseOrthographicProjection = r.ReadBool32();
        ViewportLeft = r.ReadSingle();
        ViewportRight = r.ReadSingle();
        ViewportTop = r.ReadSingle();
        ViewportBottom = r.ReadSingle();
        LODAdjust = r.ReadSingle();
        Scene = X<NiAVObject>.Ref(r);
        NumScreenPolygons = r.ReadUInt32();
        if (r.V >= 0x04020100) NumScreenTextures = r.ReadUInt32();
        if (r.V <= 0x03010000) UnknownInt3 = r.ReadUInt32();
    }
}

/// <summary>
/// Wrapper for color animation keys.
/// </summary>
public class NiColorData : NiObject { // X
    public KeyGroup<Color4> Data;                       // The color keys.

    public NiColorData(NifReader r) : base(r) {
        Data = new KeyGroup<Color4>(r);
    }
}

/// <summary>
/// Wrapper for 1D (one-dimensional) floating point animation keys.
/// </summary>
public class NiFloatData : NiObject { // X
    public KeyGroup<float> Data;                        // The keys.

    public NiFloatData(NifReader r) : base(r) {
        Data = new KeyGroup<float>(r);
    }
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier. Applies a gravitational field on the particles.
/// </summary>
public class NiGravity : NiParticleModifier { // X
    public float UnknownFloat1;                         // Unknown.
    public float Force;                                 // The strength/force of this gravity.
    public FieldType Type;                              // The force field type.
    public Vector3 Position;                            // The position of the mass point relative to the particle system.
    public Vector3 Direction;                           // The direction of the applied acceleration.

    public NiGravity(NifReader r) : base(r) {
        if (r.V >= 0x0303000D) UnknownFloat1 = r.ReadSingle();
        Force = r.ReadSingle();
        Type = (FieldType)r.ReadUInt32();
        Position = r.ReadVector3();
        Direction = r.ReadVector3();
    }
}

/// <summary>
/// DEPRECATED (10.2), RENAMED (10.2) to NiTransformData.
/// Wrapper for transformation animation keys.
/// </summary>
public class NiKeyframeData : NiObject { // X
    public uint NumRotationKeys;                        // The number of quaternion rotation keys. If the rotation type is XYZ (type 4) then this *must* be set to 1, and in this case the actual number of keys is stored in the XYZ Rotations field.
    public KeyType RotationType;                        // The type of interpolation to use for rotation.  Can also be 4 to indicate that separate X, Y, and Z values are used for the rotation instead of Quaternions.
    public QuatKey<Quaternion>[] QuaternionKeys;        // The rotation keys if Quaternion rotation is used.
    public float Order;
    public KeyGroup<float>[] XYZRotations;              // Individual arrays of keys for rotating X, Y, and Z individually.
    public KeyGroup<Vector3> Translations;              // Translation keys.
    public KeyGroup<float> Scales;                      // Scale keys.

    public NiKeyframeData(NifReader r) : base(r) {
        NumRotationKeys = r.ReadUInt32();
        if (NumRotationKeys != 0) RotationType = (KeyType)r.ReadUInt32();
        if (RotationType != KeyType.XYZ_ROTATION_KEY) QuaternionKeys = r.ReadFArray(z => new QuatKey<Quaternion>(r, RotationType), NumRotationKeys);
        else {
            if (r.V <= 0x0A010000) Order = r.ReadSingle();
            XYZRotations = r.ReadFArray(z => new KeyGroup<float>(r), 3);
        }
        Translations = new KeyGroup<Vector3>(r);
        Scales = new KeyGroup<float>(r);
    }
}

/// <summary>
/// Describes the surface properties of an object e.g. translucency, ambient color, diffuse color, emissive color, and specular color.
/// </summary>
public class NiMaterialProperty : NiProperty { // X
    public Flags Flags;                                 // Property flags.
    public Color3 AmbientColor = new(1.0, 1.0, 1.0);    // How much the material reflects ambient light.
    public Color3 DiffuseColor = new(1.0, 1.0, 1.0);    // How much the material reflects diffuse light.
    public Color3 SpecularColor = new(1.0, 1.0, 1.0);   // How much light the material reflects in a specular manner.
    public Color3 EmissiveColor = new(0.0, 0.0, 0.0);   // How much light the material emits.
    public float Glossiness = 10.0f;                    // The material glossiness.
    public float Alpha = 1.0f;                          // The material transparency (1=non-transparant). Refer to a NiAlphaProperty object in this material's parent NiTriShape object, when alpha is not 1.
    public float EmissiveMult = 1.0f;

    public NiMaterialProperty(NifReader r) : base(r) {
        if (r.V >= 0x03000000 && r.V <= 0x0A000102) Flags = (Flags)r.ReadUInt16();
        if (r.UV2 < 26) {
            AmbientColor = new Color3(r);
            DiffuseColor = new Color3(r);
        }
        SpecularColor = new Color3(r);
        EmissiveColor = new Color3(r);
        Glossiness = r.ReadSingle();
        Alpha = r.ReadSingle();
        if (r.UV2 > 21) EmissiveMult = r.ReadSingle();
    }
}

/// <summary>
/// DEPRECATED (20.5), replaced by NiMorphMeshModifier.
/// Geometry morphing data.
/// </summary>
public class NiMorphData : NiObject { // X
    public uint NumMorphs;                              // Number of morphing object.
    public uint NumVertices;                            // Number of vertices.
    public byte RelativeTargets = 1;                    // This byte is always 1 in all official files.
    public Morph[] Morphs;                              // The geometry morphing objects.

    public NiMorphData(NifReader r) : base(r) {
        NumMorphs = r.ReadUInt32();
        NumVertices = r.ReadUInt32();
        RelativeTargets = r.ReadByte();
        Morphs = r.ReadFArray(z => new Morph(r, NumVertices), NumMorphs);
    }
}

/// <summary>
/// Generic node object for grouping.
/// </summary>
public class NiNode : NiAVObject { // X
    public Ref<NiAVObject>[] Children;                  // List of child node object indices.
    public Ref<NiDynamicEffect>[] Effects;              // List of node effects. ADynamicEffect?

    public NiNode(NifReader r) : base(r) {
        Children = r.ReadL32FArray(X<NiAVObject>.Ref);
        if (r.UV2 < 130) Effects = r.ReadL32FArray(X<NiDynamicEffect>.Ref);
    }
}

/// <summary>
/// Morrowind specific.
/// </summary>
public class AvoidNode(NifReader r) : NiNode(r) { // X
}

/// <summary>
/// These nodes will always be rotated to face the camera creating a billboard effect for any attached objects.
/// 
/// In pre-10.1.0.0 the Flags field is used for BillboardMode.
/// Bit 0: hidden
/// Bits 1-2: collision mode
/// Bit 3: unknown (set in most official meshes)
/// Bits 5-6: billboard mode
/// 
/// Collision modes:
/// 00 NONE
/// 01 USE_TRIANGLES
/// 10 USE_OBBS
/// 11 CONTINUE
/// 
/// Billboard modes:
/// 00 ALWAYS_FACE_CAMERA
/// 01 ROTATE_ABOUT_UP
/// 10 RIGID_FACE_CAMERA
/// 11 ALWAYS_FACE_CENTER
/// </summary>
public class NiBillboardNode : NiNode { // X
    public BillboardMode BillboardMode;                 // The way the billboard will react to the camera.

    public NiBillboardNode(NifReader r) : base(r) {
        if (r.V >= 0x0A010000) BillboardMode = (BillboardMode)r.ReadUInt16();
    }
}

/// <summary>
/// Bethesda-specific extension of Node with animation properties stored in the flags, often 42?
/// </summary>
public class NiBSAnimationNode(NifReader r) : NiNode(r) { // X
}

/// <summary>
/// Unknown.
/// </summary>
public class NiBSParticleNode(NifReader r) : NiNode(r) { // X
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiParticleBomb : NiParticleModifier { // X
    public float Decay;
    public float Duration;
    public float DeltaV;
    public float Start;
    public DecayType DecayType;
    public SymmetryType SymmetryType;
    public Vector3 Position;                            // The position of the mass point relative to the particle system?
    public Vector3 Direction;                           // The direction of the applied acceleration?

    public NiParticleBomb(NifReader r) : base(r) {
        Decay = r.ReadSingle();
        Duration = r.ReadSingle();
        DeltaV = r.ReadSingle();
        Start = r.ReadSingle();
        DecayType = (DecayType)r.ReadUInt32();
        if (r.V >= 0x0401000C) SymmetryType = (SymmetryType)r.ReadUInt32();
        Position = r.ReadVector3();
        Direction = r.ReadVector3();
    }
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiParticleColorModifier : NiParticleModifier { // X
    public Ref<NiColorData> ColorData;

    public NiParticleColorModifier(NifReader r) : base(r) {
        ColorData = X<NiColorData>.Ref(r);
    }
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiParticleGrowFade : NiParticleModifier { // X
    public float Grow;                                  // The time from the beginning of the particle lifetime during which the particle grows.
    public float Fade;                                  // The time from the end of the particle lifetime during which the particle fades.

    public NiParticleGrowFade(NifReader r) : base(r) {
        Grow = r.ReadSingle();
        Fade = r.ReadSingle();
    }
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiParticleMeshModifier : NiParticleModifier { // X
    public Ref<NiAVObject>[] ParticleMeshes;

    public NiParticleMeshModifier(NifReader r) : base(r) {
        ParticleMeshes = r.ReadL32FArray(X<NiAVObject>.Ref);
    }
}

/// <summary>
/// LEGACY (pre-10.1) particle modifier.
/// </summary>
public class NiParticleRotation : NiParticleModifier { // X
    public byte RandomInitialAxis;
    public Vector3 InitialAxis;
    public float RotationSpeed;

    public NiParticleRotation(NifReader r) : base(r) {
        RandomInitialAxis = r.ReadByte();
        InitialAxis = r.ReadVector3();
        RotationSpeed = r.ReadSingle();
    }
}

/// <summary>
/// Generic particle system node.
/// </summary>
public class NiParticles : NiGeometry { // X
    public BSVertexDesc VertexDesc;

    public NiParticles(NifReader r) : base(r) {
        if (r.UV2 >= 100) VertexDesc = r.ReadS<BSVertexDesc>();
    }
}

/// <summary>
/// LEGACY (pre-10.1). NiParticles which do not house normals and generate them at runtime.
/// </summary>
public class NiAutoNormalParticles(NifReader r) : NiParticles(r) { // X
}

/// <summary>
/// A generic particle system time controller object.
/// </summary>
public class NiParticleSystemController : NiTimeController { // X
    public uint OldSpeed;                               // Particle speed in old files
    public float Speed;                                 // Particle speed
    public float SpeedRandom;                           // Particle random speed modifier
    public float VerticalDirection;                     // vertical emit direction [radians]
                                                        //     0.0 : up
                                                        //     1.6 : horizontal
                                                        //     3.1416 : down
    public float VerticalAngle;                         // emitter's vertical opening angle [radians]
    public float HorizontalDirection;                   // horizontal emit direction
    public float HorizontalAngle;                       // emitter's horizontal opening angle
    public Vector3 UnknownNormal;                       // Unknown.
    public Color4 UnknownColor;                         // Unknown.
    public float Size;                                  // Particle size
    public float EmitStartTime;                         // Particle emit start time
    public float EmitStopTime;                          // Particle emit stop time
    public byte UnknownByte;                            // Unknown byte, (=0)
    public uint OldEmitRate;                            // Particle emission rate in old files
    public float EmitRate;                              // Particle emission rate (particles per second)
    public float Lifetime;                              // Particle lifetime
    public float LifetimeRandom;                        // Particle lifetime random modifier
    public ushort EmitFlags;                            // Bit 0: Emit Rate toggle bit (0 = auto adjust, 1 = use Emit Rate value)
    public Vector3 StartRandom;                         // Particle random start translation vector
    public Ref<NiObject> Emitter;                       // This index targets the particle emitter object (TODO: find out what type of object this refers to).
    public ushort UnknownShort2;                        // ? short=0 ?
    public float UnknownFloat13;                        // ? float=1.0 ?
    public uint UnknownInt1;                            // ? int=1 ?
    public uint UnknownInt2;                            // ? int=0 ?
    public ushort UnknownShort3;                        // ? short=0 ?
    public Vector3 ParticleVelocity;                    // Particle velocity
    public Vector3 ParticleUnknownVector;               // Unknown
    public float ParticleLifetime;                      // The particle's age.
    public Ref<NiObject> ParticleLink;
    public uint ParticleTimestamp;                      // Timestamp of the last update.
    public ushort ParticleUnknownShort;                 // Unknown short
    public ushort ParticleVertexId;                     // Particle/vertex index matches array index
    public ushort NumParticles;                         // Size of the following array. (Maximum number of simultaneous active particles)
    public ushort NumValid;                             // Number of valid entries in the following array. (Number of active particles at the time the system was saved)
    public Particle[] Particles;                        // Individual particle modifiers?
    public Ref<NiObject> UnknownLink;                   // unknown int (=0xffffffff)
    public Ref<NiParticleModifier> ParticleExtra;       // Link to some optional particle modifiers (NiGravity, NiParticleGrowFade, NiParticleBomb, ...)
    public Ref<NiObject> UnknownLink2;                  // Unknown int (=0xffffffff)
    public byte Trailer;                                // Trailing null byte
    public Ref<NiColorData> ColorData;
    public float UnknownFloat1;
    public float[] UnknownFloats2;

    public NiParticleSystemController(NifReader r) : base(r) {
        if (r.V <= 0x03010000) OldSpeed = r.ReadUInt32();
        if (r.V >= 0x0303000D) Speed = r.ReadSingle();
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
        if (r.V >= 0x04000002) UnknownByte = r.ReadByte();
        if (r.V <= 0x03010000) OldEmitRate = r.ReadUInt32();
        if (r.V >= 0x0303000D) EmitRate = r.ReadSingle();
        Lifetime = r.ReadSingle();
        LifetimeRandom = r.ReadSingle();
        if (r.V >= 0x04000002) EmitFlags = r.ReadUInt16();
        StartRandom = r.ReadVector3();
        Emitter = X<NiObject>.Ptr(r);
        if (r.V >= 0x04000002) {
            UnknownShort2 = r.ReadUInt16();
            UnknownFloat13 = r.ReadSingle();
            UnknownInt1 = r.ReadUInt32();
            UnknownInt2 = r.ReadUInt32();
            UnknownShort3 = r.ReadUInt16();
        }
        if (r.V <= 0x03010000) {
            ParticleVelocity = r.ReadVector3();
            ParticleUnknownVector = r.ReadVector3();
            ParticleLifetime = r.ReadSingle();
            ParticleLink = X<NiObject>.Ref(r);
            ParticleTimestamp = r.ReadUInt32();
            ParticleUnknownShort = r.ReadUInt16();
            ParticleVertexId = r.ReadUInt16();
        }
        if (r.V >= 0x04000002) {
            NumParticles = r.ReadUInt16();
            NumValid = r.ReadUInt16();
            Particles = r.ReadSArray<Particle>(NumParticles);
            UnknownLink = X<NiObject>.Ref(r);
        }
        ParticleExtra = X<NiParticleModifier>.Ref(r);
        UnknownLink2 = X<NiObject>.Ref(r);
        if (r.V >= 0x04000002) Trailer = r.ReadByte();
        if (r.V <= 0x03010000) {
            ColorData = X<NiColorData>.Ref(r);
            UnknownFloat1 = r.ReadSingle();
            UnknownFloats2 = r.ReadPArray<float>("f", ParticleUnknownShort);
        }
    }
}

/// <summary>
/// A particle system controller, used by BS in conjunction with NiBSParticleNode.
/// </summary>
public class NiBSPArrayController(NifReader r) : NiParticleSystemController(r) { // X
}

public class PixelFormatComponent(NifReader r) { // Y
    public PixelComponent Type = (PixelComponent)r.ReadUInt32(); // Component Type
    public PixelRepresentation Convention = (PixelRepresentation)r.ReadUInt32(); // Data Storage Convention
    public byte BitsPerChannel = r.ReadByte();          // Bits per component
    public bool IsSigned = r.ReadBool32();
}

public abstract class NiPixelFormat : NiObject { // Y
    public PixelFormat PixelFormat;                     // The format of the pixels in this internally stored image.
    public uint RedMask;                                // 0x000000ff (for 24bpp and 32bpp) or 0x00000000 (for 8bpp)
    public uint GreenMask;                              // 0x0000ff00 (for 24bpp and 32bpp) or 0x00000000 (for 8bpp)
    public uint BlueMask;                               // 0x00ff0000 (for 24bpp and 32bpp) or 0x00000000 (for 8bpp)
    public uint AlphaMask;                              // 0xff000000 (for 32bpp) or 0x00000000 (for 24bpp and 8bpp)
    public uint BitsPerPixel;                           // Bits per pixel, 0 (Compressed), 8, 24 or 32.
    public byte[] OldFastCompare;                       // [96,8,130,0,0,65,0,0] if 24 bits per pixel
                                                        //     [129,8,130,32,0,65,12,0] if 32 bits per pixel
                                                        //     [34,0,0,0,0,0,0,0] if 8 bits per pixel
                                                        //     [X,0,0,0,0,0,0,0] if 0 (Compressed) bits per pixel where X = PixelFormat
    public PixelTiling Tiling;                          // Seems to always be zero.
    public uint RendererHint;
    public uint ExtraData;
    public byte Flags;
    public bool sRGBSpace;
    public PixelFormatComponent[] Channels;             // Channel Data

    public NiPixelFormat(NifReader r) : base(r) {
        PixelFormat = (PixelFormat)r.ReadUInt32();
        if (r.V <= 0x0A030002) {
            RedMask = r.ReadUInt32();
            GreenMask = r.ReadUInt32();
            BlueMask = r.ReadUInt32();
            AlphaMask = r.ReadUInt32();
            BitsPerPixel = r.ReadUInt32();
            OldFastCompare = r.ReadBytes(8);
        }
        if (r.V >= 0x0A010000 && r.V <= 0x0A030002) Tiling = (PixelTiling)r.ReadUInt32();
        if (r.V >= 0x0A030003) {
            BitsPerPixel = r.ReadByte();
            RendererHint = r.ReadUInt32();
            ExtraData = r.ReadUInt32();
            Flags = r.ReadByte();
            Tiling = (PixelTiling)r.ReadUInt32();
        }
        if (r.V >= 0x14030004) sRGBSpace = r.ReadBool32();
        if (r.V >= 0x0A030003) Channels = r.ReadFArray(z => new PixelFormatComponent(r), 4);
    }
}

/// <summary>
/// Wrapper for position animation keys.
/// </summary>
public class NiPosData : NiObject { // X
    public KeyGroup<Vector3> Data;

    public NiPosData(NifReader r) : base(r) {
        Data = new KeyGroup<Vector3>(r);
    }
}

/// <summary>
/// Unknown.
/// </summary>
public class NiRotatingParticles(NifReader r) : NiParticles(r) { // X
}

/// <summary>
/// Determines whether flat shading or smooth shading is used on a shape.
/// </summary>
public class NiShadeProperty : NiProperty { // X
    public Flags Flags = (Flags)1;                      // Bit 0: Enable smooth phong shading on this shape. Otherwise, hard-edged flat shading will be used on this shape.

    public NiShadeProperty(NifReader r) : base(r) {
        if (r.UV2 <= 34) Flags = (Flags)r.ReadUInt16();
    }
}

/// <summary>
/// Skinning data.
/// </summary>
public class NiSkinData : NiObject { // X
    public NiTransform SkinTransform;                   // Offset of the skin from this bone in bind position.
    public uint NumBones;                               // Number of bones.
    public Ref<NiSkinPartition> SkinPartition;          // This optionally links a NiSkinPartition for hardware-acceleration information.
    public byte HasVertexWeights = 1;                   // Enables Vertex Weights for this NiSkinData.
    public BoneData[] BoneList;                         // Contains offset data for each node that this skin is influenced by.

    public NiSkinData(NifReader r) : base(r) {
        SkinTransform = r.ReadS<NiTransform>();
        NumBones = r.ReadUInt32();
        if (r.V >= 0x04000002 && r.V <= 0x0A010000) SkinPartition = X<NiSkinPartition>.Ref(r);
        if (r.V >= 0x04020100) HasVertexWeights = r.ReadByte();
        BoneList = r.ReadFArray(z => new BoneData(r, HasVertexWeights), NumBones);
    }
}

/// <summary>
/// Skinning instance.
/// </summary>
public class NiSkinInstance : NiObject { // X
    public Ref<NiSkinData> Data;                        // Skinning data reference.
    public Ref<NiSkinPartition> SkinPartition;          // Refers to a NiSkinPartition objects, which partitions the mesh such that every vertex is only influenced by a limited number of bones.
    public Ref<NiNode> SkeletonRoot;                    // Armature root node.
    public Ref<NiNode>[] Bones;                         // List of all armature bones.

    public NiSkinInstance(NifReader r) : base(r) {
        Data = X<NiSkinData>.Ref(r);
        if (r.V >= 0x0A010065) SkinPartition = X<NiSkinPartition>.Ref(r);
        SkeletonRoot = X<NiNode>.Ptr(r);
        Bones = r.ReadL32FArray(X<NiNode>.Ptr);
    }
}

/// <summary>
/// Skinning data, optimized for hardware skinning. The mesh is partitioned in submeshes such that each vertex of a submesh is influenced only by a limited and fixed number of bones.
/// </summary>
public class NiSkinPartition : NiObject { // X
    public uint NumSkinPartitionBlocks;
    public SkinPartition[] SkinPartitionBlocks;         // Skin partition objects.
    public uint DataSize;
    public uint VertexSize;
    public BSVertexDesc VertexDesc;
    public BSVertexDataSSE[] VertexData;
    public SkinPartition[] Partition;

    public NiSkinPartition(NifReader r) : base(r) {
        NumSkinPartitionBlocks = r.ReadUInt32();
        if (!((r.V == 0x14020007) && (r.UV2 == 100))) SkinPartitionBlocks = r.ReadFArray(z => new SkinPartition(r), NumSkinPartitionBlocks);
        if (r.UV2 == 100) {
            DataSize = r.ReadUInt32();
            VertexSize = r.ReadUInt32();
            VertexDesc = r.ReadS<BSVertexDesc>();
            if (DataSize > 0) VertexData = r.ReadFArray(z => new BSVertexDataSSE(r, (uint)VertexDesc.VertexAttributes), DataSize / VertexSize);
            Partition = r.ReadFArray(z => new SkinPartition(r), NumSkinPartitionBlocks);
        }
    }
}

/// <summary>
/// A texture.
/// </summary>
public abstract class NiTexture(NifReader r) : NiObjectNET(r) { // X
}

/// <summary>
/// NiTexture::FormatPrefs. These preferences are a request to the renderer to use a format the most closely matches the settings and may be ignored.
/// </summary>
public class FormatPrefs(NifReader r) { // Y
    public PixelLayout PixelLayout = (PixelLayout)r.ReadUInt32(); // Requests the way the image will be stored.
    public MipMapFormat UseMipmaps = (MipMapFormat)r.ReadUInt32(); // Requests if mipmaps are used or not.
    public AlphaFormat AlphaFormat = (AlphaFormat)r.ReadUInt32(); // Requests no alpha, 1-bit alpha, or
}

/// <summary>
/// Describes texture source and properties.
/// </summary>
public class NiSourceTexture : NiTexture { // X
    public byte UseExternal = 1;                        // Is the texture external?
    public string FileName;                             // The external texture file name.
    public Ref<NiObject> UnknownLink;                   // Unknown.
    public byte UnknownByte = 1;                        // Unknown. Seems to be set if Pixel Data is present?
    public Ref<NiPixelFormat> PixelData;                // NiPixelData or NiPersistentSrcTextureRendererData
    public FormatPrefs FormatPrefs;                     // A set of preferences for the texture format. They are a request only and the renderer may ignore them.
    public byte IsStatic = 1;                           // If set, then the application cannot assume that any dynamic changes to the pixel data will show in the rendered image.
    public bool DirectRender = true;                    // A hint to the renderer that the texture can be loaded directly from a texture file into a renderer-specific resource, bypassing the NiPixelData object.
    public bool PersistRenderData = false;              // Pixel Data is NiPersistentSrcTextureRendererData instead of NiPixelData.

    public NiSourceTexture(NifReader r) : base(r) {
        UseExternal = r.ReadByte();
        if (r.V >= 0x0A010000 && UseExternal == 1) {
            FileName = r.ReadL32Encoding();
            UnknownLink = X<NiObject>.Ref(r);
        }
        if (UseExternal == 0) {
            if (r.V <= 0x0A000100) UnknownByte = r.ReadByte();
            if (r.V >= 0x0A010000) FileName = r.ReadL32Encoding();
            PixelData = X<NiPixelFormat>.Ref(r);
        }
        FormatPrefs = new FormatPrefs(r);
        IsStatic = r.ReadByte();
        if (r.V >= 0x0A010067) DirectRender = r.ReadBool32();
        if (r.V >= 0x14020004) PersistRenderData = r.ReadBool32();
    }
}

/// <summary>
/// Apparently commands for an optimizer instructing it to keep things it would normally discard.
/// Also refers to NiNode objects (through their name) in animation .kf files.
/// </summary>
public class NiStringExtraData : NiExtraData { // X
    public uint BytesRemaining;                         // The number of bytes left in the record.  Equals the length of the following string + 4.
    public string StringData;                           // The string.

    public NiStringExtraData(NifReader r) : base(r) {
        if (r.V <= 0x04020200) BytesRemaining = r.ReadUInt32();
        StringData = X.String(r);
    }
}

/// <summary>
/// Extra data, used to name different animation sequences.
/// </summary>
public class NiTextKeyExtraData : NiExtraData { // X
    public uint UnknownInt1;                            // Unknown.  Always equals zero in all official files.
    public Key<string>[] TextKeys;                      // List of textual notes and at which time they take effect. Used for designating the start and stop of animations and the triggering of sounds.

    public NiTextKeyExtraData(NifReader r) : base(r) {
        if (r.V <= 0x04020200) UnknownInt1 = r.ReadUInt32();
        TextKeys = r.ReadL32FArray(z => new Key<string>(r, KeyType.LINEAR_KEY));
    }
}

/// <summary>
/// Represents an effect that uses projected textures such as projected lights (gobos), environment maps, and fog maps.
/// </summary>
public class NiTextureEffect : NiDynamicEffect { // X
    public Matrix4x4 ModelProjectionMatrix;             // Model projection matrix.  Always identity?
    public Vector3 ModelProjectionTransform;            // Model projection transform.  Always (0,0,0)?
    public TexFilterMode TextureFiltering = TexFilterMode.FILTER_TRILERP; // Texture Filtering mode.
    public ushort MaxAnisotropy;
    public TexClampMode TextureClamping = TexClampMode.WRAP_S_WRAP_T; // Texture Clamp mode.
    public TextureType TextureType = TextureType.TEX_ENVIRONMENT_MAP; // The type of effect that the texture is used for.
    public CoordGenType CoordinateGenerationType = CoordGenType.CG_SPHERE_MAP; // The method that will be used to generate UV coordinates for the texture effect.
    public Ref<NiImage> Image;                          // Image index.
    public Ref<NiSourceTexture> SourceTexture;          // Source texture index.
    public byte EnablePlane = 0;                        // Determines whether a clipping plane is used.
    public NiPlane Plane;
    public short PS2L = 0;
    public short PS2K = -75;
    public ushort UnknownShort;                         // Unknown: 0.

    public NiTextureEffect(NifReader r) : base(r) {
        ModelProjectionMatrix = r.ReadMatrix3x3As4x4();
        ModelProjectionTransform = r.ReadVector3();
        TextureFiltering = (TexFilterMode)r.ReadUInt32();
        if (r.V >= 0x14050004) MaxAnisotropy = r.ReadUInt16();
        TextureClamping = (TexClampMode)r.ReadUInt32();
        TextureType = (TextureType)r.ReadUInt32();
        CoordinateGenerationType = (CoordGenType)r.ReadUInt32();
        if (r.V <= 0x03010000) Image = X<NiImage>.Ref(r);
        if (r.V >= 0x04000000) SourceTexture = X<NiSourceTexture>.Ref(r);
        EnablePlane = r.ReadByte();
        Plane = r.ReadS<NiPlane>();
        if (r.V <= 0x0A020000) {
            PS2L = r.ReadInt16();
            PS2K = r.ReadInt16();
        }
        if (r.V <= 0x0401000C) UnknownShort = r.ReadUInt16();
    }
}

/// <summary>
/// LEGACY (pre-10.1)
/// </summary>
public class NiImage : NiObject { // Y
    public byte UseExternal;                            // 0 if the texture is internal to the NIF file.
    public string FileName;                             // The filepath to the texture.
    public Ref<NiRawImageData> ImageData;               // Link to the internally stored image data.
    public uint UnknownInt = 7;                         // Unknown.  Often seems to be 7. Perhaps m_uiMipLevels?
    public float UnknownFloat = 128.5f;                 // Unknown.  Perhaps fImageScale?

    public NiImage(NifReader r) : base(r) {
        UseExternal = r.ReadByte();
        if (UseExternal != 0) FileName = r.ReadL32Encoding();
        else ImageData = X<NiRawImageData>.Ref(r);
        UnknownInt = r.ReadUInt32();
        if (r.V >= 0x03010000) UnknownFloat = r.ReadSingle();
    }
}

/// <summary>
/// Describes how a fragment shader should be configured for a given piece of geometry.
/// </summary>
public class NiTexturingProperty : NiProperty { // X
    public Flags Flags;                                 // Property flags.
    public ApplyMode ApplyMode = ApplyMode.APPLY_MODULATE; // Determines how the texture will be applied.  Seems to have special functions in Oblivion.
    public uint TextureCount = 7;                       // Number of textures.
    public TexDesc BaseTexture;                         // The base texture.
    public TexDesc DarkTexture;                         // The dark texture.
    public TexDesc DetailTexture;                       // The detail texture.
    public TexDesc GlossTexture;                        // The gloss texture.
    public TexDesc GlowTexture;                         // The glowing texture.
    public TexDesc BumpMapTexture;                      // The bump map texture.
    public float BumpMapLumaScale;
    public float BumpMapLumaOffset;
    public Matrix2x2 BumpMapMatrix;
    public TexDesc NormalTexture;                       // Normal texture.
    public TexDesc ParallaxTexture;
    public float ParallaxOffset;
    public bool HasDecal0Texture;
    public TexDesc Decal0Texture;                       // The decal texture.
    public bool HasDecal1Texture;
    public TexDesc Decal1Texture;                       // Another decal texture.
    public bool HasDecal2Texture;
    public TexDesc Decal2Texture;                       // Another decal texture.
    public bool HasDecal3Texture;
    public TexDesc Decal3Texture;                       // Another decal texture.
    public ShaderTexDesc[] ShaderTextures;              // Shader textures.

    public NiTexturingProperty(NifReader r) : base(r) {
        if (r.V <= 0x0A000102) Flags = (Flags)r.ReadUInt16();
        if (r.V >= 0x14010002) Flags = (Flags)r.ReadUInt16();
        if (r.V >= 0x0303000D && r.V <= 0x14010001) ApplyMode = (ApplyMode)r.ReadUInt32();
        TextureCount = r.ReadUInt32();
        if (r.ReadBool32()) BaseTexture = new TexDesc(r);
        if (r.ReadBool32()) DarkTexture = new TexDesc(r);
        if (r.ReadBool32()) DetailTexture = new TexDesc(r);
        if (r.ReadBool32()) GlossTexture = new TexDesc(r);
        if (r.ReadBool32()) GlowTexture = new TexDesc(r);
        var HasBumpMapTexture = r.V >= 0x0303000D && TextureCount > 5 ? r.ReadBool32() : default;
        if (HasBumpMapTexture) {
            BumpMapTexture = new TexDesc(r);
            BumpMapLumaScale = r.ReadSingle();
            BumpMapLumaOffset = r.ReadSingle();
            BumpMapMatrix = r.ReadMatrix2x2();
        }
        var HasNormalTexture = r.V >= 0x14020005 && TextureCount > 6 ? r.ReadBool32() : default;
        if (HasNormalTexture) NormalTexture = new TexDesc(r);
        var HasParallaxTexture = r.V >= 0x14020005 && TextureCount > 7 ? r.ReadBool32() : default;
        if (HasParallaxTexture) {
            ParallaxTexture = new TexDesc(r);
            ParallaxOffset = r.ReadSingle();
        }
        if (r.V <= 0x14020004 && TextureCount > 6) HasDecal0Texture = r.ReadBool32();
        if (r.V >= 0x14020005 && TextureCount > 8) HasDecal0Texture = r.ReadBool32();
        if (HasDecal0Texture) Decal0Texture = new TexDesc(r);
        if (r.V <= 0x14020004 && TextureCount > 7) HasDecal1Texture = r.ReadBool32();
        if (r.V >= 0x14020005 && TextureCount > 9) HasDecal1Texture = r.ReadBool32();
        if (HasDecal1Texture) Decal1Texture = new TexDesc(r);
        if (r.V <= 0x14020004 && TextureCount > 8) HasDecal2Texture = r.ReadBool32();
        if (r.V >= 0x14020005 && TextureCount > 10) HasDecal2Texture = r.ReadBool32();
        if (HasDecal2Texture) Decal2Texture = new TexDesc(r);
        if (r.V <= 0x14020004 && TextureCount > 9) HasDecal3Texture = r.ReadBool32();
        if (r.V >= 0x14020005 && TextureCount > 11) HasDecal3Texture = r.ReadBool32();
        if (HasDecal3Texture) Decal3Texture = new TexDesc(r);
        if (r.V >= 0x0A000100) ShaderTextures = r.ReadL32FArray(z => new ShaderTexDesc(r));
    }
}

/// <summary>
/// A shape node that refers to singular triangle data.
/// </summary>
public class NiTriShape(NifReader r) : NiTriBasedGeom(r) { // X
}

/// <summary>
/// Holds mesh data using a list of singular triangles.
/// </summary>
public class NiTriShapeData : NiTriBasedGeomData { // X
    public uint NumTrianglePoints;                      // Num Triangles times 3.
    public bool HasTriangles;                           // Do we have triangle data?
    public Triangle[] Triangles;                        // Triangle data.
    public MatchGroup[] MatchGroups;                    // The shared normals.

    public NiTriShapeData(NifReader r) : base(r) {
        NumTrianglePoints = r.ReadUInt32();
        if (r.V >= 0x0A010000) HasTriangles = false; // calculated
        if (r.V <= 0x0A000102) Triangles = r.ReadSArray<Triangle>(NumTriangles);
        if (r.V >= 0x0A000103 && HasTriangles) Triangles = r.ReadSArray<Triangle>(NumTriangles);
        if (r.V >= 0x03010000) MatchGroups = r.ReadL16FArray(z => new MatchGroup(r));
    }
}

/// <summary>
/// DEPRECATED (pre-10.1), REMOVED (20.3).
/// Time controller for texture coordinates.
/// </summary>
public class NiUVController : NiTimeController { // X
    public ushort UnknownShort;                         // Always 0?
    public Ref<NiUVData> Data;                          // Texture coordinate controller data index.

    public NiUVController(NifReader r) : base(r) {
        UnknownShort = r.ReadUInt16();
        Data = X<NiUVData>.Ref(r);
    }
}

/// <summary>
/// DEPRECATED (pre-10.1), REMOVED (20.3)
/// Texture coordinate data.
/// </summary>
public class NiUVData : NiObject { // X
    public KeyGroup<float>[] UVGroups;                  // Four UV data groups. Appear to be U translation, V translation, U scaling/tiling, V scaling/tiling.

    public NiUVData(NifReader r) : base(r) {
        UVGroups = r.ReadFArray(z => new KeyGroup<float>(r), 4);
    }
}

/// <summary>
/// Property of vertex colors. This object is referred to by the root object of the NIF file whenever some NiTriShapeData object has vertex colors with non-default settings; if not present, vertex colors have vertex_mode=2 and lighting_mode=1.
/// </summary>
public class NiVertexColorProperty : NiProperty { // X
    public Flags Flags;                                 // Bits 0-2: Unknown
                                                        //     Bit 3: Lighting Mode
                                                        //     Bits 4-5: Vertex Mode
    public VertMode VertexMode;                         // In Flags from 20.1.0.3 on.
    public LightMode LightingMode;                      // In Flags from 20.1.0.3 on.

    public NiVertexColorProperty(NifReader r) : base(r) {
        Flags = (Flags)r.ReadUInt16();
        if (r.V <= 0x14000005) {
            VertexMode = (VertMode)r.ReadUInt32();
            LightingMode = (LightMode)r.ReadUInt32();
        }
    }
}

/// <summary>
/// DEPRECATED (10.x), REMOVED (?)
/// Not used in skinning.
/// Unsure of use - perhaps for morphing animation or gravity.
/// </summary>
public class NiVertWeightsExtraData : NiExtraData { // X
    public uint NumBytes;                               // Number of bytes in this data object.
    public float[] Weight;                              // The vertex weights.

    public NiVertWeightsExtraData(NifReader r) : base(r) {
        NumBytes = r.ReadUInt32();
        Weight = r.ReadL16PArray<float>("f");
    }
}

/// <summary>
/// DEPRECATED (10.2), REMOVED (?), Replaced by NiBoolData.
/// Visibility data for a controller.
/// </summary>
public class NiVisData : NiObject { // X
    public Key<byte>[] Keys;

    public NiVisData(NifReader r) : base(r) {
        Keys = r.ReadL32FArray(z => new Key<byte>(r, KeyType.LINEAR_KEY));
    }
}

/// <summary>
/// Allows applications to switch between drawing solid geometry or wireframe outlines.
/// </summary>
public class NiWireframeProperty : NiProperty { // X
    public Flags Flags;                                 // Property flags.
                                                        //     0 - Wireframe Mode Disabled
                                                        //     1 - Wireframe Mode Enabled

    public NiWireframeProperty(NifReader r) : base(r) {
        Flags = (Flags)r.ReadUInt16();
    }
}

/// <summary>
/// Allows applications to set the test and write modes of the renderer's Z-buffer and to set the comparison function used for the Z-buffer test.
/// </summary>
public class NiZBufferProperty : NiProperty { // X
    public Flags Flags = (Flags)3;                      // Bit 0 enables the z test
                                                        //     Bit 1 controls wether the Z buffer is read only (0) or read/write (1)
    public ZCompareMode Function = ZCompareMode.ZCOMP_LESS_EQUAL; // Z-Test function (see: glDepthFunc). In Flags from 20.1.0.3 on.

    public NiZBufferProperty(NifReader r) : base(r) {
        Flags = (Flags)r.ReadUInt16();
        if (r.V >= 0x0401000C && r.V <= 0x14000005) Function = (ZCompareMode)r.ReadUInt32();
    }
}

/// <summary>
/// Morrowind-specific node for collision mesh.
/// </summary>
public class RootCollisionNode(NifReader r) : NiNode(r) { // X
}

/// <summary>
/// LEGACY (pre-10.1)
/// Raw image data.
/// </summary>
public class NiRawImageData : NiObject { // Y
    public uint Width;                                  // Image width
    public uint Height;                                 // Image height
    public ImageType ImageType;                         // The format of the raw image data.
    public Color3[][] RGBImageData;                     // Image pixel data.
    public Color4[][] RGBAImageData;                    // Image pixel data.

    public NiRawImageData(NifReader r) : base(r) {
        Width = r.ReadUInt32();
        Height = r.ReadUInt32();
        ImageType = (ImageType)r.ReadUInt32();
        if (ImageType == ImageType.RGB) RGBImageData = r.ReadFArray(k => r.ReadFArray(z => new Color3(r.ReadBytes(3)), Height), Width);
        if (ImageType == ImageType.RGBA) RGBAImageData = r.ReadFArray(k => r.ReadFArray(z => new Color4(r.ReadBytes(4)), Height), Width);
    }
}

/// <summary>
/// The type of animation interpolation (blending) that will be used on the associated key frames.
/// </summary>
public enum BSShaderType : uint { // Y
    SHADER_TALL_GRASS = 0,          // Tall Grass Shader
    SHADER_DEFAULT = 1,             // Standard Lighting Shader
    SHADER_SKY = 10,                // Sky Shader
    SHADER_SKIN = 14,               // Skin Shader
    SHADER_WATER = 17,              // Water Shader
    SHADER_LIGHTING30 = 29,         // Lighting 3.0 Shader
    SHADER_TILE = 32,               // Tiled Shader
    SHADER_NOLIGHTING = 33          // No Lighting Shader
}

/// <summary>
/// Shader Property Flags
/// </summary>
[Flags]
public enum BSShaderFlags : uint { // Y
    Specular = 0,                   // Enables Specularity
    Skinned = 1U << 1,              // Required For Skinned Meshes
    LowDetail = 1U << 2,            // Lowddetail (seems to use standard diff/norm/spec shader)
    Vertex_Alpha = 1U << 3,         // Vertex Alpha
    Unknown_1 = 1U << 4,            // Unknown
    Single_Pass = 1U << 5,          // Single Pass
    Empty = 1U << 6,                // Unknown
    Environment_Mapping = 1U << 7,  // Environment mapping (uses Envmap Scale)
    Alpha_Texture = 1U << 8,        // Alpha Texture Requires NiAlphaProperty to Enable
    Unknown_2 = 1U << 9,            // Unknown
    FaceGen = 1U << 10,             // FaceGen
    Parallax_Shader_Index_15 = 1U << 11, // Parallax
    Unknown_3 = 1U << 12,           // Unknown/Crash
    Non_Projective_Shadows = 1U << 13, // Non-Projective Shadows
    Unknown_4 = 1U << 14,           // Unknown/Crash
    Refraction = 1U << 15,          // Refraction (switches on refraction power)
    Fire_Refraction = 1U << 16,     // Fire Refraction (switches on refraction power/period)
    Eye_Environment_Mapping = 1U << 17, // Eye Environment Mapping (does not use envmap light fade or envmap scale)
    Hair = 1U << 18,                // Hair
    Dynamic_Alpha = 1U << 19,       // Dynamic Alpha
    Localmap_Hide_Secret = 1U << 20,// Localmap Hide Secret
    Window_Environment_Mapping = 1U << 21, // Window Environment Mapping
    Tree_Billboard = 1U << 22,      // Tree Billboard
    Shadow_Frustum = 1U << 23,      // Shadow Frustum
    Multiple_Textures = 1U << 24,   // Multiple Textures (base diff/norm become null)
    Remappable_Textures = 1U << 25, // usually seen w/texture animation
    Decal_Single_Pass = 1U << 26,   // Decal
    Dynamic_Decal_Single_Pass = 1U << 27, // Dynamic Decal
    Parallax_Occulsion = 1U << 28,  // Parallax Occlusion
    External_Emittance = 1U << 29,  // External Emittance
    Shadow_Map = 1U << 30,          // Shadow Map
    ZBuffer_Test = 1U << 31         // ZBuffer Test (1=on)
}

/// <summary>
/// Shader Property Flags 2
/// </summary>
[Flags]
public enum BSShaderFlags2 : uint { // Y
    ZBuffer_Write = 0,              // ZBuffer Write
    LOD_Landscape = 1U << 1,        // LOD Landscape
    LOD_Building = 1U << 2,         // LOD Building
    No_Fade = 1U << 3,              // No Fade
    Refraction_Tint = 1U << 4,      // Refraction Tint
    Vertex_Colors = 1U << 5,        // Has Vertex Colors
    Unknown1 = 1U << 6,             // Unknown
    X1st_Light_is_Point_Light = 1U << 7, // 1st Light is Point Light
    X2nd_Light = 1U << 8,           // 2nd Light
    X3rd_Light = 1U << 9,           // 3rd Light
    Vertex_Lighting = 1U << 10,     // Vertex Lighting
    Uniform_Scale = 1U << 11,       // Uniform Scale
    Fit_Slope = 1U << 12,           // Fit Slope
    Billboard_and_Envmap_Light_Fade = 1U << 13, // Billboard and Envmap Light Fade
    No_LOD_Land_Blend = 1U << 14,   // No LOD Land Blend
    Envmap_Light_Fade = 1U << 15,   // Envmap Light Fade
    Wireframe = 1U << 16,           // Wireframe
    VATS_Selection = 1U << 17,      // VATS Selection
    Show_in_Local_Map = 1U << 18,   // Show in Local Map
    Premult_Alpha = 1U << 19,       // Premult Alpha
    Skip_Normal_Maps = 1U << 20,    // Skip Normal Maps
    Alpha_Decal = 1U << 21,         // Alpha Decal
    No_Transparecny_Multisampling = 1U << 22, // No Transparency MultiSampling
    Unknown2 = 1U << 23,            // Unknown
    Unknown3 = 1U << 24,            // Unknown
    Unknown4 = 1U << 25,            // Unknown
    Unknown5 = 1U << 26,            // Unknown
    Unknown6 = 1U << 27,            // Unknown
    Unknown7 = 1U << 28,            // Unknown
    Unknown8 = 1U << 29,            // Unknown
    Unknown9 = 1U << 30,            // Unknown
    Unknown10 = 1U << 31            // Unknown
}

/// <summary>
/// Bethesda-specific property.
/// </summary>
public class BSShaderProperty : NiShadeProperty { // Y
    public BSShaderType ShaderType = BSShaderType.SHADER_DEFAULT;
    public BSShaderFlags ShaderFlags = (BSShaderFlags)0x82000000;
    public BSShaderFlags2 ShaderFlags2 = (BSShaderFlags2)1;
    public float EnvironmentMapScale = 1.0f;            // Scales the intensity of the environment/cube map.

    public BSShaderProperty(NifReader r) : base(r) {
        if (r.UV2 <= 34) {
            ShaderType = (BSShaderType)r.ReadUInt32();
            ShaderFlags = (BSShaderFlags)r.ReadUInt32();
            ShaderFlags2 = (BSShaderFlags2)r.ReadUInt32();
            EnvironmentMapScale = r.ReadSingle();
        }
    }
}

#endregion
