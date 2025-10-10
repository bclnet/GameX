import os
from io import BytesIO
from enum import Enum, Flag
from typing import TypeVar, Generic
from openstk.poly import Reader
from gamex import FileSource, PakBinaryT, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo
from gamex.globalx import Color3, Color4

T = TypeVar('T')

# types
type Vector3 = np.ndarray
type Vector4 = np.ndarray
type Matrix2x2 = np.ndarray
type Matrix4x4 = np.ndarray
type Quaternion = np.ndarray

# typedefs
class Color: pass
class UnionBV: pass
class NiReader: pass
class NiObject: pass

#region X

class Ref(Generic[T]):
    def __init__(self, r: NiReader, v: int):
        self.v: int = v
        self.val: T
    def value() -> T: return None
class X(Generic[T]):
    @staticmethod # Refers to an object before the current one in the hierarchy.
    def ptr(r: Reader): return None if (v := r.readInt32()) < 0 else Ref(r, v)
    @staticmethod # Refers to an object after the current one in the hierarchy.
    def ref(r: Reader): return None if (v := r.readInt32()) < 0 else Ref(r, v)
class Y(Generic[T]):
    @staticmethod
    def read(type: type, r: Reader) -> object:
        if type == float: return r.readSingle()
        elif type == byte: return r.readByte()
        elif type == str: return r.readL32Encoding()
        elif type == Vector3: return r.readVector3()
        elif type == Quaternion: return r.readQuaternionWFirst()
        elif type == Color4: return Color4(r)
        else: raise NotImplementedError('Tried to read an unsupported type.')
class Z:
    @staticmethod
    def string(r: Reader) -> str: return r.readL32Encoding()
    @staticmethod
    def stringRef(r: Reader, p: int) -> str: return None
    @staticmethod
    def isVersionSupported(v: int) -> bool: return True
    @staticmethod
    def parseHeaderStr(s: str) -> tuple:
        p = s.index('Version')
        if p >= 0:
            v = s
            v = v[(p + 8):]
            for i in range(len(v)):
                if v[i].isdigit() or v[i] == '.': continue
                else: v = v[:i]
            ver = Z.ver2Num(v)
            if not Z.isVersionSupported(ver): raise Exception(f'Version {Z.ver2Str(ver)} ({ver}) is not supported.')
            return (s, ver)
        elif s.startsWith('NS'): return (s, 0x0a010000); # Dodgy version for NeoSteam
        raise Exception('Invalid header string')
    @staticmethod
    def ver2Str(v: int) -> str:
        if v == 0: return ''
        elif v < 0x0303000D:
            # this is an old-style 2-number version with one period
            s = f'{(v >> 24) & 0xff}.{(v >> 16) & 0xff}'
            sub_num1 = (v >> 8) & 0xff; sub_num2 = v & 0xff
            if sub_num1 > 0 or sub_num2 > 0: s += f'{sub_num1}'
            if sub_num2 > 0: s += f'{sub_num2}'
            return s
        # this is a new-style 4-number version with 3 periods
        else: return f'{(v >> 24) & 0xff}.{(v >> 16) & 0xff}.{(v >> 8) & 0xff}.{v & 0xff}'
    @staticmethod
    def ver2Num(s: str) -> int:
        if not s: return 0
        if '.' in s:
            l = s.split('.')
            v = 0
            if len(l) > 4: return 0 # Version # has more than 3 dots in it.
            elif len(l) == 2:
                # this is an old style version number.
                v += int(l[0]) << (3 * 8)
                if len(l[1]) >= 1: v += int(l[1][0:1]) << (2 * 8)
                if len(l[1]) >= 2: v += int(l[1][1:2]) << (1 * 8)
                if len(l[1]) >= 3: v += int(l[1][2:])
                return v
            # this is a new style version number with dots separating the digits
            for i in range(min(4, len(l))): v += int(l[i]) << ((3 - i) * 8)
            return v
        return int(s)

class Flags(Flag):
    Hidden = 0x1
    Other10 = 10
    Other34 = 34

#endregion

#region Enums

# Describes how the vertex colors are blended with the filtered texture color.
class ApplyMode(Enum): # X
    APPLY_REPLACE = 0               # Replaces existing color
    APPLY_DECAL = 1                 # For placing images on the object like stickers.
    APPLY_MODULATE = 2              # Modulates existing color. (Default)
    APPLY_HILIGHT = 3               # PS2 Only.  Function Unknown.
    APPLY_HILIGHT2 = 4              # Parallax Flag in some Oblivion meshes.

# The type of animation interpolation (blending) that will be used on the associated key frames.
class KeyType(Enum): # X
    LINEAR_KEY = 1                  # Use linear interpolation.
    QUADRATIC_KEY = 2               # Use quadratic interpolation.  Forward and back tangents will be stored.
    TBC_KEY = 3                     # Use Tension Bias Continuity interpolation.  Tension, bias, and continuity will be stored.
    XYZ_ROTATION_KEY = 4            # For use only with rotation data.  Separate X, Y, and Z keys will be stored instead of using quaternions.
    CONST_KEY = 5                   # Step function. Used for visibility keys in NiBoolData.

# Describes the pixel format used by the NiPixelData object to store a texture.
class PixelFormat(Enum): # Y
    FMT_RGB = 0                     # 24-bit RGB. 8 bits per red, blue, and green component.
    FMT_RGBA = 1                    # 32-bit RGB with alpha. 8 bits per red, blue, green, and alpha component.
    FMT_PAL = 2                     # 8-bit palette index.
    FMT_PALA = 3                    # 8-bit palette index with alpha.
    FMT_DXT1 = 4                    # DXT1 compressed texture.
    FMT_DXT3 = 5                    # DXT3 compressed texture.
    FMT_DXT5 = 6                    # DXT5 compressed texture.
    FMT_RGB24NONINT = 7             # (Deprecated) 24-bit noninterleaved texture, an old PS2 format.
    FMT_BUMP = 8                    # Uncompressed dU/dV gradient bump map.
    FMT_BUMPLUMA = 9                # Uncompressed dU/dV gradient bump map with luma channel representing shininess.
    FMT_RENDERSPEC = 10             # Generic descriptor for any renderer-specific format not described by other formats.
    FMT_1CH = 11                    # Generic descriptor for formats with 1 component.
    FMT_2CH = 12                    # Generic descriptor for formats with 2 components.
    FMT_3CH = 13                    # Generic descriptor for formats with 3 components.
    FMT_4CH = 14                    # Generic descriptor for formats with 4 components.
    FMT_DEPTH_STENCIL = 15          # Indicates the NiPixelFormat is meant to be used on a depth/stencil surface.
    FMT_UNKNOWN = 16

# Describes whether pixels have been tiled from their standard row-major format to a format optimized for a particular platform.
class PixelTiling(Enum): # Y
    TILE_NONE = 0
    TILE_XENON = 1
    TILE_WII = 2
    TILE_NV_SWIZZLED = 3

# Describes the pixel format used by the NiPixelData object to store a texture.
class PixelComponent(Enum): # Y
    COMP_RED = 0
    COMP_GREEN = 1
    COMP_BLUE = 2
    COMP_ALPHA = 3
    COMP_COMPRESSED = 4
    COMP_OFFSET_U = 5
    COMP_OFFSET_V = 6
    COMP_OFFSET_W = 7
    COMP_OFFSET_Q = 8
    COMP_LUMA = 9
    COMP_HEIGHT = 10
    COMP_VECTOR_X = 11
    COMP_VECTOR_Y = 12
    COMP_VECTOR_Z = 13
    COMP_PADDING = 14
    COMP_INTENSITY = 15
    COMP_INDEX = 16
    COMP_DEPTH = 17
    COMP_STENCIL = 18
    COMP_EMPTY = 19

# Describes how each pixel should be accessed on NiPixelFormat.
class PixelRepresentation(Enum): # Y
    REP_NORM_INT = 0
    REP_HALF = 1
    REP_FLOAT = 2
    REP_INDEX = 3
    REP_COMPRESSED = 4
    REP_UNKNOWN = 5
    REP_INT = 6

# Describes the color depth in an NiTexture.
class PixelLayout(Enum): # X
    LAY_PALETTIZED_8 = 0            # Texture is in 8-bit palettized format.
    LAY_HIGH_COLOR_16 = 1           # Texture is in 16-bit high color format.
    LAY_TRUE_COLOR_32 = 2           # Texture is in 32-bit true color format.
    LAY_COMPRESSED = 3              # Texture is compressed.
    LAY_BUMPMAP = 4                 # Texture is a grayscale bump map.
    LAY_PALETTIZED_4 = 5            # Texture is in 4-bit palettized format.
    LAY_DEFAULT = 6                 # Use default setting.
    LAY_SINGLE_COLOR_8 = 7
    LAY_SINGLE_COLOR_16 = 8
    LAY_SINGLE_COLOR_32 = 9
    LAY_DOUBLE_COLOR_32 = 10
    LAY_DOUBLE_COLOR_64 = 11
    LAY_FLOAT_COLOR_32 = 12
    LAY_FLOAT_COLOR_64 = 13
    LAY_FLOAT_COLOR_128 = 14
    LAY_SINGLE_COLOR_4 = 15
    LAY_DEPTH_24_X8 = 16

# Describes how mipmaps are handled in an NiTexture.
class MipMapFormat(Enum): # X
    MIP_FMT_NO = 0                  # Texture does not use mip maps.
    MIP_FMT_YES = 1                 # Texture uses mip maps.
    MIP_FMT_DEFAULT = 2             # Use default setting.

# Describes how transparency is handled in an NiTexture.
class AlphaFormat(Enum): # X
    ALPHA_NONE = 0                  # No alpha.
    ALPHA_BINARY = 1                # 1-bit alpha.
    ALPHA_SMOOTH = 2                # Interpolated 4- or 8-bit alpha.
    ALPHA_DEFAULT = 3               # Use default setting.

# Describes the availiable texture clamp modes, i.e. the behavior of UV mapping outside the [0,1] range.
class TexClampMode(Enum): # X
    CLAMP_S_CLAMP_T = 0             # Clamp in both directions.
    CLAMP_S_WRAP_T = 1              # Clamp in the S(U) direction but wrap in the T(V) direction.
    WRAP_S_CLAMP_T = 2              # Wrap in the S(U) direction but clamp in the T(V) direction.
    WRAP_S_WRAP_T = 3               # Wrap in both directions.

# Describes the availiable texture filter modes, i.e. the way the pixels in a texture are displayed on screen.
class TexFilterMode(Enum): # X
    FILTER_NEAREST = 0              # Nearest neighbor. Uses nearest texel with no mipmapping.
    FILTER_BILERP = 1               # Bilinear. Linear interpolation with no mipmapping.
    FILTER_TRILERP = 2              # Trilinear. Linear intepolation between 8 texels (4 nearest texels between 2 nearest mip levels).
    FILTER_NEAREST_MIPNEAREST = 3   # Nearest texel on nearest mip level.
    FILTER_NEAREST_MIPLERP = 4      # Linear interpolates nearest texel between two nearest mip levels.
    FILTER_BILERP_MIPNEAREST = 5    # Linear interpolates on nearest mip level.
    FILTER_ANISOTROPIC = 6          # Anisotropic filtering. One or many trilinear samples depending on anisotropy.

# Describes how to apply vertex colors for NiVertexColorProperty.
class VertMode(Enum): # X
    VERT_MODE_SRC_IGNORE = 0        # Emissive, ambient, and diffuse colors are all specified by the NiMaterialProperty.
    VERT_MODE_SRC_EMISSIVE = 1      # Emissive colors are specified by the source vertex colors. Ambient+Diffuse are specified by the NiMaterialProperty.
    VERT_MODE_SRC_AMB_DIF = 2       # Ambient+Diffuse colors are specified by the source vertex colors. Emissive is specified by the NiMaterialProperty. (Default)

# Describes which lighting equation components influence the final vertex color for NiVertexColorProperty.
class LightMode(Enum): # X
    LIGHT_MODE_EMISSIVE = 0         # Emissive.
    LIGHT_MODE_EMI_AMB_DIF = 1      # Emissive + Ambient + Diffuse. (Default)

# The force field type.
class FieldType(Enum): # X
    FIELD_WIND = 0                  # Wind (fixed direction)
    FIELD_POINT = 1                 # Point (fixed origin)

# Determines the way the billboard will react to the camera.
# Billboard mode is stored in lowest 3 bits although Oblivion vanilla nifs uses values higher than 7.
class BillboardMode(Enum): # Y
    ALWAYS_FACE_CAMERA = 0          # Align billboard and camera forward vector. Minimized rotation.
    ROTATE_ABOUT_UP = 1             # Align billboard and camera forward vector while allowing rotation around the up axis.
    RIGID_FACE_CAMERA = 2           # Align billboard and camera forward vector. Non-minimized rotation.
    ALWAYS_FACE_CENTER = 3          # Billboard forward vector always faces camera ceneter. Minimized rotation.
    RIGID_FACE_CENTER = 4           # Billboard forward vector always faces camera ceneter. Non-minimized rotation.
    BSROTATE_ABOUT_UP = 5           # The billboard will only rotate around its local Z axis (it always stays in its local X-Y plane).
    ROTATE_ABOUT_UP2 = 9            # The billboard will only rotate around the up axis (same as ROTATE_ABOUT_UP?).

# Describes Z-buffer test modes for NiZBufferProperty.
# "Less than" = closer to camera, "Greater than" = further from camera.
class ZCompareMode(Enum): # Y
    ZCOMP_ALWAYS = 0                # Always true. Buffer is ignored.
    ZCOMP_LESS = 1                  # VRef ‹ VBuf
    ZCOMP_EQUAL = 2                 # VRef = VBuf
    ZCOMP_LESS_EQUAL = 3            # VRef ≤ VBuf
    ZCOMP_GREATER = 4               # VRef › VBuf
    ZCOMP_NOT_EQUAL = 5             # VRef ≠ VBuf
    ZCOMP_GREATER_EQUAL = 6         # VRef ≥ VBuf
    ZCOMP_NEVER = 7                 # Always false. Ref value is ignored.

# Describes the decay function of bomb forces.
class DecayType(Enum): # X
    DECAY_NONE = 0                  # No decay.
    DECAY_LINEAR = 1                # Linear decay.
    DECAY_EXPONENTIAL = 2           # Exponential decay.

# Describes the symmetry type of bomb forces.
class SymmetryType(Enum): # Y
    SPHERICAL_SYMMETRY = 0          # Spherical Symmetry.
    CYLINDRICAL_SYMMETRY = 1        # Cylindrical Symmetry.
    PLANAR_SYMMETRY = 2             # Planar Symmetry.

# The type of information that is stored in a texture used by an NiTextureEffect.
class TextureType(Enum): # X
    TEX_PROJECTED_LIGHT = 0         # Apply a projected light texture. Each light effect is summed before multiplying by the base texture.
    TEX_PROJECTED_SHADOW = 1        # Apply a projected shadow texture. Each shadow effect is multiplied by the base texture.
    TEX_ENVIRONMENT_MAP = 2         # Apply an environment map texture. Added to the base texture and light/shadow/decal maps.
    TEX_FOG_MAP = 3                 # Apply a fog map texture. Alpha channel is used to blend the color channel with the base texture.

# Determines the way that UV texture coordinates are generated.
class CoordGenType(Enum): # X
    CG_WORLD_PARALLEL = 0           # Use planar mapping.
    CG_WORLD_PERSPECTIVE = 1        # Use perspective mapping.
    CG_SPHERE_MAP = 2               # Use spherical mapping.
    CG_SPECULAR_CUBE_MAP = 3        # Use specular cube mapping. For NiSourceCubeMap only.
    CG_DIFFUSE_CUBE_MAP = 4         # Use diffuse cube mapping. For NiSourceCubeMap only.

class EndianType(Enum): # X
    ENDIAN_BIG = 0                  # The numbers are stored in big endian format, such as those used by PowerPC Mac processors.
    ENDIAN_LITTLE = 1               # The numbers are stored in little endian format, such as those used by Intel and AMD x86 processors.

# Used by NiMaterialColorControllers to select which type of color in the controlled object that will be animated.
class MaterialColor(Enum): # Y
    TC_AMBIENT = 0                  # Control the ambient color.
    TC_DIFFUSE = 1                  # Control the diffuse color.
    TC_SPECULAR = 2                 # Control the specular color.
    TC_SELF_ILLUM = 3               # Control the self illumination color.

# Used by NiGeometryData to control the volatility of the mesh.
# Consistency Type is masked to only the upper 4 bits (0xF000). Dirty mask is the lower 12 (0x0FFF) but only used at runtime.
class ConsistencyType(Enum): # Y
    CT_MUTABLE = 0x0000             # Mutable Mesh
    CT_STATIC = 0x4000              # Static Mesh
    CT_VOLATILE = 0x8000            # Volatile Mesh

class BoundVolumeType(Enum): # X
    BASE_BV = 0xffffffff            # Default
    SPHERE_BV = 0                   # Sphere
    BOX_BV = 1                      # Box
    CAPSULE_BV = 2                  # Capsule
    UNION_BV = 4                    # Union
    HALFSPACE_BV = 5                # Half Space

# Values for configuring the shader type in a BSLightingShaderProperty
class BSLightingShaderPropertyShaderType(Enum): # Y
    Default = 0
    Environment_Map = 1             # Enables EnvMap Mask(TS6), EnvMap Scale
    Glow_Shader = 2                 # Enables Glow(TS3)
    Parallax = 3                    # Enables Height(TS4)
    Face_Tint = 4                   # Enables Detail(TS4), Tint(TS7)
    Skin_Tint = 5                   # Enables Skin Tint Color
    Hair_Tint = 6                   # Enables Hair Tint Color
    Parallax_Occ = 7                # Enables Height(TS4), Max Passes, Scale. Unimplemented.
    Multitexture_Landscape = 8
    LOD_Landscape = 9
    Snow = 10
    MultiLayer_Parallax = 11        # Enables EnvMap Mask(TS6), Layer(TS7), Parallax Layer Thickness, Parallax Refraction Scale, Parallax Inner Layer U Scale, Parallax Inner Layer V Scale, EnvMap Scale
    Tree_Anim = 12
    LOD_Objects = 13
    Sparkle_Snow = 14               # Enables SparkleParams
    LOD_Objects_HD = 15
    Eye_Envmap = 16                 # Enables EnvMap Mask(TS6), Eye EnvMap Scale
    Cloud = 17
    LOD_Landscape_Noise = 18
    Multitexture_Landscape_LOD_Blend = 19
    FO4_Dismemberment = 20

#endregion

#region Compounds

# Color3 -> Color3(r)
# Color4 -> Color4(r)
# The NIF file footer.
class Footer: # X
    def __init__(self, r: NiReader):
        self.roots: list[Ref[NiObject]] = r.readL32FArray(X[NiObject].ref) if r.v >= 0x0303000D else None # List of root NIF objects. If there is a camera, for 1st person view, then this NIF object is referred to as well in this list, even if it is not a root object (usually we want the camera to be attached to the Bip Head node).

# Group of vertex indices of vertices that match.
class MatchGroup: # X
    def __init__(self, r: NiReader):
        self.vertexIndices: list[int] = r.readL16PArray(None, 'H') # The vertex indices.

# Description of a mipmap within an NiPixelData object.
class MipMap: # X
    struct = ('<3i', 12)
    def __init__(self, r: NiReader):
        self.width: int = r.readUInt32()                # Width of the mipmap image.
        self.height: int = r.readUInt32()               # Height of the mipmap image.
        self.offset: int = r.readUInt32()               # Offset into the pixel data array where this mipmap starts.

# NiSkinData::BoneVertData. A vertex and its weight.
class BoneVertData: # X
    struct = ('<Hf', 6)
    def __init__(self, r: NiReader, half: bool):
        self.index: int = r.readUInt16()                # The vertex index, in the mesh.
        self.weight: float = r.readSingle() if full else r.readHalf() # The vertex weight - between 0.0 and 1.0

# Information about how the file was exported
class ExportInfo: # X
    def __init__(self, r: NiReader):
        self.author: str = r.readL8AString()
        self.processScript: str = r.readL8AString()
        self.exportScript: str = r.readL8AString()

# The NIF file header.
class NiReader(Reader): # X
    headerString: str                                   # 'NetImmerse File Format x.x.x.x' (versions <= 10.0.1.2) or 'Gamebryo File Format x.x.x.x' (versions >= 10.1.0.0), with x.x.x.x the version written out. Ends with a newline character (0x0A).
    copyright: list[str]
    v: int = 0x04000002                                 # The NIF version, in hexadecimal notation: 0x04000002, 0x0401000C, 0x04020002, 0x04020100, 0x04020200, 0x0A000100, 0x0A010000, 0x0A020000, 0x14000004, ...
    endianType: EndianType = EndianType.ENDIAN_LITTLE   # Determines the endianness of the data in the file.
    uv: int                                             # An extra version number, for companies that decide to modify the file format.
    numBlocks: int                                      # Number of file objects.
    uv2: int = 0
    exportInfo: ExportInfo
    maxFilepath: str
    metadata: bytearray
    blockTypes: list[str]                               # List of all object types used in this NIF file.
    blockTypeHashes: list[int]                          # List of all object types used in this NIF file.
    blockTypeIndex: list[int]                           # Maps file objects on their corresponding type: first file object is of type object_types[object_type_index[0]], the second of object_types[object_type_index[1]], etc.
    blockSize: list[int]                                # Array of block sizes?
    numStrings: int                                     # Number of strings.
    maxStringLength: int                                # Maximum string length.
    strings: list[str]                                  # Strings.
    groups: list[int]
    # read blocks
    blocks: list[NiObject]
    roots: list[Ref[NiObject]]

    def __init__(self, b: Reader):
        super().__init__(b.f)
        (self.headerString, self.v) = Z.parseHeaderStr(b.readVAString(128, b'\x0A')); r = self
        if r.v <= 0x03010000: self.copyright = [r.readL8AString(), r.readL8AString(), r.readL8AString()]
        if r.v >= 0x03010001: self.v = r.readUInt32()
        if r.v >= 0x14000003: self.endianType = EndianType(r.readByte())
        if r.v >= 0x0A000108: self.uv = r.readUInt32()
        if r.v >= 0x03010001: self.numBlocks = r.readUInt32()
        if ((r.v == 0x14020007) or (r.v == 0x14000005) or ((r.v >= 0x0A000102) and (r.v <= 0x14000004) and (r.uv <= 11))) and (r.uv >= 3):
            self.uv2 = r.readUInt32()
            self.exportInfo = ExportInfo(r)
        if r.uv2 == 130: self.maxFilepath = r.readL8AString()
        if r.v >= 0x1E000000: self.metadata = r.readL8Bytes()
        if r.v >= 0x05000001 and r.v != 0x14030102: self.blockTypes = r.readL16FArray(lambda z: r.readL32AString())
        if r.v == 0x14030102: self.blockTypeHashes = r.readL16PArray(None, 'I')
        if r.v >= 0x05000001: self.blockTypeIndex = r.readPArray('H', self.numBlocks)
        if r.v >= 0x14020005: self.blockSize = r.readPArray(None, 'I', self.numBlocks)
        if r.v >= 0x14010001:
            self.numStrings = r.readUInt32()
            self.maxStringLength = r.readUInt32()
            self.strings = r.readFArray(lambda z: r.readL32AString(), self.numStrings)
        if r.v >= 0x05000006: self.groups = r.readL32PArray(None, 'I')
        # read blocks
        self.blocks: list[NiObject] = [None]*self.numBlocks
        if r.v >= 0x05000001:
            for i in range(self.numBlocks): self.blocks[i] = NiObject.read(r, BlockTypes[BlockTypeIndex[i]])
        else:
            for i in range(self.numBlocks): self.blocks[i] = NiObject.read(r, Z.string(r))
        self.roots = Footer(r).Roots

# Tension, bias, continuity.
class TBC: # X
    struct = ('<3f', 12)
    def __init__(self, r: NiReader):
        self.t: float = r.readSingle()                  # Tension.
        self.b: float = r.readSingle()                  # Bias.
        self.c: float = r.readSingle()                  # Continuity.

# A generic key with support for interpolation. Type 1 is normal linear interpolation, type 2 has forward and backward tangents, and type 3 has tension, bias and continuity arguments. Note that color4 and byte always seem to be of type 1.
class Key[T]: # X
    time: float                                         # Time of the key.
    value: T                                            # The key value.
    forward: T                                          # Key forward tangent.
    backward: T                                         # The key backward tangent.
    tbc: TBC                                            # The TBC of the key.

    def __init__(self, r: NiReader, keyType: KeyType):
        self.time = r.readSingle()
        self.value = Y[T].read(r)
        if keyType == KeyType.QUADRATIC_KEY:
            self.forward = Y[T].read(r)
            self.backward = Y[T].read(r)
        elif keyType == KeyType.TBC_KEY: self.tbc = r.readS(TBC)

# Array of vector keys (anything that can be interpolated, except rotations).
class KeyGroup[T]: # X
    numKeys: int                                        # Number of keys in the array.
    interpolation: KeyType                              # The key type.
    keys: list[Key[T]]                                  # The keys.

    def __init__(self, r: NiReader):
        self.numKeys = r.readUInt32()
        if self.numKeys != 0: self.interpolation = KeyType(r.readUInt32())
        self.keys = r.readFArray(lambda z: Key[T](r, self.Interpolation), self.numKeys)

# A special version of the key type used for quaternions.  Never has tangents.
class QuatKey[T]: # X
    time: float                                         # Time the key applies.
    value: T                                            # Value of the key.
    tbc: TBC                                            # The TBC of the key.

    def __init__(self, r: NiReader, keyType: KeyType):
        if r.v <= 0x0A010000: self.time = r.readSingle()
        if keyType != KeyType.XYZ_ROTATION_KEY:
            if r.v >= 0x0A01006A: self.time = r.readSingle()
            self.value = Y[T].read(r)
        if keyType == KeyType.TBC_KEY: self.tbc = r.readS(TBC)

# Texture coordinates (u,v). As in OpenGL; image origin is in the lower left corner.
class TexCoord: # X
    struct = ('<2f', 8)
    u: float                                            # First coordinate.
    v: float                                            # Second coordinate.

    def __init__(self, r: NiReader, half: bool):
        if isinstance(r, float): self.u = r; self.v = half; return
        if half: self.u = r.readHalf(); self.v = r.readHalf(); return
        self.u = r.readSingle()
        self.v = r.readSingle()

# Describes the order of scaling and rotation matrices. Translate, Scale, Rotation, Center are from TexDesc.
# Back = inverse of Center. FromMaya = inverse of the V axis with a positive translation along V of 1 unit.
class TransformMethod(Enum): # X
    Maya_Deprecated = 0             # Center * Rotation * Back * Translate * Scale
    Max = 1                         # Center * Scale * Rotation * Translate * Back
    Maya = 2                        # Center * Rotation * Back * FromMaya * Translate * Scale

# NiTexturingProperty::Map. Texture description.
class TexDesc: # X
    image: Ref[NiObject]                                # Link to the texture image.
    source: Ref[NiObject]                               # NiSourceTexture object index.
    clampMode: TexClampMode = TexClampMode.WRAP_S_WRAP_T# 0=clamp S clamp T, 1=clamp S wrap T, 2=wrap S clamp T, 3=wrap S wrap T
    filterMode: TexFilterMode = TexFilterMode.FILTER_TRILERP # 0=nearest, 1=bilinear, 2=trilinear, 3=..., 4=..., 5=...
    flags: Flags                                        # Texture mode flags; clamp and filter mode stored in upper byte with 0xYZ00 = clamp mode Y, filter mode Z.
    maxAnisotropy: int
    uvSet: int = 0                                      # The texture coordinate set in NiGeometryData that this texture slot will use.
    pS2L: int = 0                                       # L can range from 0 to 3 and are used to specify how fast a texture gets blurry.
    pS2K: int = -75                                     # K is used as an offset into the mipmap levels and can range from -2047 to 2047. Positive values push the mipmap towards being blurry and negative values make the mipmap sharper.
    unknown1: int                                       # Unknown, 0 or 0x0101?
    # NiTextureTransform
    translation: TexCoord                               # The UV translation.
    scale: TexCoord = TexCoord(1.0, 1.0)                # The UV scale.
    rotation: float = 0.0                               # The W axis rotation in texture space.
    transformMethod: TransformMethod = 0                # Depending on the source, scaling can occur before or after rotation.
    center: TexCoord                                    # The origin around which the texture rotates.

    def __init__(self, r: NiReader):
        if r.v <= 0x03010000: self.image = X[NiImage].ref(r)
        if r.v >= 0x0303000D: self.source = X[NiSourceTexture].ref(r)
        if r.v <= 0x14000005:
            self.clampMode = TexClampMode(r.readUInt32())
            self.filterMode = TexFilterMode(r.readUInt32())
        if r.v >= 0x14010003: self.flags = Flags(r.readUInt16())
        if r.v >= 0x14050004: self.maxAnisotropy = r.readUInt16()
        if r.v <= 0x14000005: self.uvSet = r.readUInt32()
        if r.v <= 0x0A040001:
            self.pS2L = r.readInt16()
            self.pS2K = r.readInt16()
        if r.v <= 0x0401000C: self.unknown1 = r.readUInt16()
        # NiTextureTransform
        if r.v >= 0x0A010000 and r.readBool32():
            self.translation = r.readS(TexCoord)
            self.scale = r.readS(TexCoord)
            self.rotation = r.readSingle()
            self.transformMethod = TransformMethod(r.readUInt32())
            self.center = r.readS(TexCoord)

# NiTexturingProperty::ShaderMap. Shader texture description.
class ShaderTexDesc: # Y
    map: TexDesc
    mapId: int                                          # Unique identifier for the Gamebryo shader system.

    def __init__(self, r: NiReader):
        if r.readBool32():
            self.map = TexDesc(r)
            self.mapId = r.readUInt32()

# List of three vertex indices.
class Triangle: # X
    struct = ('<3H', 6)
    def __init__(self, r: NiReader):
        self.v1: int = r.readUInt16()                   # First vertex index.
        self.v2: int = r.readUInt16()                   # Second vertex index.
        self.v3: int = r.readUInt16()                   # Third vertex index.

class VertexFlags(Flag): # Y
    Vertex = 1 << 4
    UVs = 1 << 5
    UVs_2 = 1 << 6
    Normals = 1 << 7
    Tangents = 1 << 8
    Vertex_Colors = 1 << 9
    Skinned = 1 << 10
    Land_Data = 1 << 11
    Eye_Data = 1 << 12
    Instance = 1 << 13
    Full_Precision = 1 << 14

class BSVertexDataSSE: # Y
    vertex: Vector3
    bitangentX: float
    unknownInt: int
    uv: TexCoord
    normal: Vector3
    bitangentY: int
    tangent: Vector3
    bitangentZ: int
    vertexColors: Color4
    boneWeights: list[float]
    boneIndices: bytearray
    eyeData: float

    def __init__(self, r: NiReader, ARG: int):
        if ((ARG & 16) != 0): self.vertex = r.readVector3()
        if ((ARG & 16) != 0) and ((ARG & 256) != 0): self.bitangentX = r.readSingle()
        if ((ARG & 16) != 0) and (ARG & 256) == 0: self.unknownInt = r.readInt32()
        if ((ARG & 32) != 0): self.uv = TexCoord(r, true)
        if (ARG & 128) != 0:
            self.normal = Vector3(r.readByte(), r.readByte(), r.readByte())
            self.bitangentY = r.readByte()
        if ((ARG & 128) != 0) and ((ARG & 256) != 0):
            self.tangent = Vector3(r.readByte(), r.readByte(), r.readByte())
            self.bitangentZ = r.readByte()
        if (ARG & 512) != 0: self.vertexColors = Color4(r.readBytes(4))
        if (ARG & 1024) != 0:
            self.boneWeights = [r.readHalf(), r.readHalf(), r.readHalf(), r.readHalf()]
            self.boneIndices = r.readBytes(4)
        if (ARG & 4096) != 0: self.eyeData = r.readSingle()

class BSVertexDesc: # Y
    struct = ('<5bHb', 8)
    def __init__(self, r: NiReader):
        self.vF1: int = r.readByte()
        self.vF2: int = r.readByte()
        self.vF3: int = r.readByte()
        self.vF4: int = r.readByte()
        self.vF5: int = r.readByte()
        self.vertexAttributes: VertexFlags = VertexFlags(r.readUInt16())
        self.vF8: int = r.readByte()

# Skinning data for a submesh, optimized for hardware skinning. Part of NiSkinPartition.
class SkinPartition: # Y
    numVertices: int                                    # Number of vertices in this submesh.
    numTriangles: int                                   # Number of triangles in this submesh.
    numBones: int                                       # Number of bones influencing this submesh.
    numStrips: int                                      # Number of strips in this submesh (zero if not stripped).
    numWeightsPerVertex: int                            # Number of weight coefficients per vertex. The Gamebryo engine seems to work well only if this number is equal to 4, even if there are less than 4 influences per vertex.
    bones: list[int]                                    # List of bones.
    vertexMap: list[int]                                # Maps the weight/influence lists in this submesh to the vertices in the shape being skinned.
    vertexWeights: list[list[float]]                    # The vertex weights.
    stripLengths: list[int]                             # The strip lengths.
    strips: list[list[int]]                             # The strips.
    triangles: list[Triangle]                           # The triangles.
    boneIndices: list[bytearray]                        # Bone indices, they index into 'Bones'.
    unknownShort: int                                   # Unknown
    vertexDesc: BSVertexDesc
    trianglesCopy: list[Triangle]

    def __init__(self, r: NiReader):
        self.numVertices = r.readUInt16()
        self.numTriangles = (self.numVertices / 3) # calculated
        self.numBones = r.readUInt16()
        self.numStrips = r.readUInt16()
        self.numWeightsPerVertex = r.readUInt16()
        self.bones = r.readPArray(None, 'H', self.numBones)
        if r.v <= 0x0A000102:
            self.vertexMap = r.readPArray(None, 'H', self.numVertices)
            self.vertexWeights = r.readFArray(lambda k: r.readPArray(None, 'f', self.numWeightsPerVertex), self.numVertices)
            self.stripLengths = r.readPArray(None, 'H', self.numStrips)
            if self.numStrips != 0: self.strips = r.readFArray(lambda k, i: r.readPArray(None, 'H', self.stripLengths), self.numStrips)
            else: self.triangles = r.readSArray<Triangle>(self.numTriangles)
        elif r.v >= 0x0A010000:
            if r.readBool32(): self.vertexMap = r.readPArray(None, 'H', self.numVertices)
            hasVertexWeights = r.readUInt32()
            if self.hasVertexWeights == 1: self.vertexWeights = r.readFArray(lambda k: r.readPArray(None, 'f', self.numWeightsPerVertex), self.numVertices)
            if self.hasVertexWeights == 15: self.vertexWeights = r.readFArray(lambda k: r.readFArray(lambda z: r.readHalf(), self.numWeightsPerVertex), self.numVertices)
            self.stripLengths = r.readPArray(None, 'H', self.numStrips)
            if r.readBool32():
                if self.numStrips != 0: self.strips = r.readFArray(lambda k, i: r.readPArray(None, 'H', self.stripLengths), self.numStrips)
                else: self.triangles = r.readSArray<Triangle>(self.numTriangles)
        if r.readBool32(): self.boneIndices = r.readFArray(lambda k: r.readBytes(self.numWeightsPerVertex), self.numVertices)
        if r.uv2 > 34: self.unknownShort = r.readUInt16()
        if r.uv2 == 100:
            self.vertexDesc = r.readS(BSVertexDesc)
            self.trianglesCopy = r.readSArray<Triangle>(self.numTriangles)

# A plane.
class NiPlane: # Y
    struct = ('<4f', 16)
    def __init__(self, r: NiReader):
        self.normal: Vector3 = r.readVector3()          # The plane normal.
        self.constant: float = r.readSingle()           # The plane constant.

# A sphere.
class NiBound: # Y
    struct = ('<4f', 16)
    def __init__(self, r: NiReader):
        self.center: Vector3 = r.readVector3()          # The sphere's center.
        self.radius: float = r.readSingle()             # The sphere's radius.

class NiTransform: # X
    def __init__(self, r: NiReader):
        self.rotation: Matrix4x4 = r.readMatrix3x3As4x4() # The rotation part of the transformation matrix.
        self.translation: Vector3 = r.readVector3()     # The translation vector.
        self.scale: float = r.readSingle()              # Scaling part (only uniform scaling is supported).

# Geometry morphing data component.
class Morph: # X
    frameName: str                                      # Name of the frame.
    interpolation: KeyType                              # Unlike most objects, the presense of this value is not conditional on there being keys.
    keys: list[Key[T]]                                  # The morph key frames.
    legacyWeight: float
    vectors: list[Vector3]                              # Morph vectors.

    def __init__(self, r: NiReader, numVertices: int):
        if r.v >= 0x0A01006A: self.frameName = Z.string(r)
        if r.v <= 0x0A010000:
            numKeys = r.readUInt32()
            self.interpolation = KeyType(r.readUInt32())
            self.keys = r.readFArray(lambda z: Key[T](r, self.Interpolation), self.numKeys)
        if r.v >= 0x0A010068 and r.v <= 0x14010002 and r.uv2 < 10: self.legacyWeight = r.readSingle()
        self.vectors = r.readPArray(None, '3f', numVertices)

# particle array entry
class Particle: # X
    struct = ('<9f2H', 40)
    def __init__(self, r: NiReader):
        self.velocity: Vector3 = r.readVector3()        # Particle velocity
        self.unknownVector: Vector3 = r.readVector3()   # Unknown
        self.lifetime: float = r.readSingle()           # The particle age.
        self.lifespan: float = r.readSingle()           # Maximum age of the particle.
        self.timestamp: float = r.readSingle()          # Timestamp of the last update.
        self.unknownShort: int = r.readUInt16()         # Unknown short
        self.vertexId: int = r.readUInt16()             # Particle/vertex index matches array index

# NiSkinData::BoneData. Skinning data component.
class BoneData: # X
    skinTransform: NiTransform                          # Offset of the skin from this bone in bind position.
    boundingSphereOffset: Vector3                       # Translation offset of a bounding sphere holding all vertices. (Note that its a Sphere Containing Axis Aligned Box not a minimum volume Sphere)
    boundingSphereRadius: float                         # Radius for bounding sphere holding all vertices.
    unknown13Shorts: list[int]                          # Unknown, always 0?
    vertexWeights: list[BoneVertData]                   # The vertex weights.

    def __init__(self, r: NiReader, arg: int):
        self.skinTransform = NiTransform(r)
        self.boundingSphereOffset = r.readVector3()
        self.boundingSphereRadius = r.readSingle()
        if r.v == 0x14030009 and (r.uv == 0x20000) or (r.uv == 0x30000): self.unknown13Shorts = r.readPArray(None, 'h', 13)
        self.vertexWeights = r.readL16SArray<BoneVertData>() if r.v <= 0x04020100 else \
            r.readL16SArray<BoneVertData>() if r.v >= 0x04020200 and arg == 1 else \
            r.readL16FArray(lambda z: BoneVertData(r, False)) if r.V >= 0x14030101 and arg == 15 else None

# Determines how the raw image data is stored in NiRawImageData.
class ImageType(Enum): # Y
    RGB = 1                         # Colors store red, blue, and green components.
    RGBA = 2                        # Colors store red, blue, green, and alpha components.

# Box Bounding Volume
class BoxBV: # X
    def __init__(self, r: NiReader):
        self.center: Vector3 = r.readVector3()
        self.axis: Matrix4x4 = r.readMatrix3x3As4x4()
        self.extent: Vector3 = r.readVector3()

# Capsule Bounding Volume
class CapsuleBV: # Y
    struct = ('<8f', 32)
    def __init__(self, r: NiReader):
        self.center: Vector3 = r.readVector3()
        self.origin: Vector3 = r.readVector3()
        self.extent: float = r.readSingle()
        self.radius: float = r.readSingle()

class HalfSpaceBV: # Y
    struct = ('<7f', 28)
    def __init__(self, r: NiReader):
        self.plane: NiPlane = r.readS(NiPlane)
        self.center: Vector3 = r.readVector3()

class BoundingVolume: # X
    collisionType: BoundVolumeType                      # Type of collision data.
    sphere: NiBound
    box: BoxBV
    capsule: CapsuleBV
    union: UnionBV
    halfSpace: HalfSpaceBV

    def __init__(self, r: NiReader):
        self.collisionType = BoundVolumeType(r.readUInt32())
        match self.collisionType:
            case BoundVolumeType.SPHERE_BV: self.sphere = r.readS(NiBound)
            case BoundVolumeType.BOX_BV: self.box = BoxBV(r)
            case BoundVolumeType.CAPSULE_BV: self.capsule = r.readS(CapsuleBV)
            case BoundVolumeType.UNION_BV: self.union = UnionBV(r)
            case BoundVolumeType.HALFSPACE_BV: self.halfSpace = r.readS(HalfSpaceBV)

class UnionBV: # Y
    def __init__(self, r: NiReader):
        self.boundingVolumes: list[BoundingVolume] = r.readL32FArray(lambda z: BoundingVolume(r))

class MorphWeight: # Y
    def __init__(self, r: NiReader):
        self.interpolator: Ref[NiObject] = X[NiInterpolator].ref(r)
        self.weight: float = r.readSingle()

#endregion

#region NIF Objects

# These are the main units of data that NIF files are arranged in.
# They are like C classes and can contain many pieces of data.
# The only differences between these and compounds is that these are treated as object types by the NIF format and can inherit from other classes.

# Abstract object type.
class NiObject: # X
    def __init__(self, r: NiReader):
        pass

    @staticmethod
    def read(r: NiReader, nodeType: str) -> NiObject:
        match nodeType:
            case 'NiNode': return NiNode(r)
            case 'NiTriShape': return NiTriShape(r)
            case 'NiTexturingProperty': return NiTexturingProperty(r)
            case 'NiSourceTexture': return NiSourceTexture(r)
            case 'NiMaterialProperty': return NiMaterialProperty(r)
            case 'NiMaterialColorController': return NiMaterialColorController(r)
            case 'NiTriShapeData': return NiTriShapeData(r)
            case 'RootCollisionNode': return RootCollisionNode(r)
            case 'NiStringExtraData': return NiStringExtraData(r)
            case 'NiSkinInstance': return NiSkinInstance(r)
            case 'NiSkinData': return NiSkinData(r)
            case 'NiAlphaProperty': return NiAlphaProperty(r)
            case 'NiZBufferProperty': return NiZBufferProperty(r)
            case 'NiVertexColorProperty': return NiVertexColorProperty(r)
            case 'NiBSAnimationNode': return NiBSAnimationNode(r)
            case 'NiBSParticleNode': return NiBSParticleNode(r)
            case 'NiParticles': return NiParticles(r)
            case 'NiParticlesData': return NiParticlesData(r)
            case 'NiRotatingParticles': return NiRotatingParticles(r)
            case 'NiRotatingParticlesData': return NiRotatingParticlesData(r)
            case 'NiAutoNormalParticles': return NiAutoNormalParticles(r)
            case 'NiAutoNormalParticlesData': return NiAutoNormalParticlesData(r)
            case 'NiUVController': return NiUVController(r)
            case 'NiUVData': return NiUVData(r)
            case 'NiTextureEffect': return NiTextureEffect(r)
            case 'NiTextKeyExtraData': return NiTextKeyExtraData(r)
            case 'NiVertWeightsExtraData': return NiVertWeightsExtraData(r)
            case 'NiParticleSystemController': return NiParticleSystemController(r)
            case 'NiBSPArrayController': return NiBSPArrayController(r)
            case 'NiGravity': return NiGravity(r)
            case 'NiParticleBomb': return NiParticleBomb(r)
            case 'NiParticleColorModifier': return NiParticleColorModifier(r)
            case 'NiParticleGrowFade': return NiParticleGrowFade(r)
            case 'NiParticleMeshModifier': return NiParticleMeshModifier(r)
            case 'NiParticleRotation': return NiParticleRotation(r)
            case 'NiKeyframeController': return NiKeyframeController(r)
            case 'NiKeyframeData': return NiKeyframeData(r)
            case 'NiColorData': return NiColorData(r)
            case 'NiGeomMorpherController': return NiGeomMorpherController(r)
            case 'NiMorphData': return NiMorphData(r)
            case 'AvoidNode': return AvoidNode(r)
            case 'NiVisController': return NiVisController(r)
            case 'NiVisData': return NiVisData(r)
            case 'NiAlphaController': return NiAlphaController(r)
            case 'NiFloatData': return NiFloatData(r)
            case 'NiPosData': return NiPosData(r)
            case 'NiBillboardNode': return NiBillboardNode(r)
            case 'NiShadeProperty': return NiShadeProperty(r)
            case 'NiWireframeProperty': return NiWireframeProperty(r)
            case 'NiCamera': return NiCamera(r)
            case 'NiPathController': return NiPathController(r)
            case 'NiPixelData': return NiPixelData(r)
            case _: Log(f'Tried to read an unsupported NiObject type ({nodeType}).'); return null

# LEGACY (pre-10.1). Abstract base class for particle system modifiers.
class NiParticleModifier(NiObject): # X
    nextModifier: Ref[NiObject]                         # Next particle modifier.
    controller: Ref[NiObject]                           # Points to the particle system controller parent.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.nextModifier = X[NiParticleModifier].ref(r)
        if r.v >= 0x04000002: self.controller = X[NiParticleSystemController].ptr(r)

# A generic extra data object.
class NiExtraData(NiObject): # X
    name: str                                           # Name of this object.
    nextExtraData: Ref[NiObject]                        # Block number of the next extra data object.

    def __init__(self, r: NiReader):
        super().__init__(r)
        BSExtraData: bool = False
        if r.v >= 0x0A000100 and not BSExtraData: self.name = Z.string(r)
        if r.v <= 0x04020200: self.nextExtraData = X[NiExtraData].ref(r)

# Abstract base class for all interpolators of bool, float, NiQuaternion, NiPoint3, NiColorA, and NiQuatTransform data.
class NiInterpolator(NiObject): # Y
    def __init__(self, r: NiReader):
        super().__init__(r)

class PathFlags(Flag): # X
    CVDataNeedsUpdate = 0
    CurveTypeOpen = 1 << 1
    AllowFlip = 1 << 2
    Bank = 1 << 3
    ConstantVelocity = 1 << 4
    Follow = 1 << 5
    Flip = 1 << 6

# Abstract base class for NiObjects that support names, extra data, and time controllers.
class NiObjectNET(NiObject): # X
    skyrimShaderType: BSLightingShaderPropertyShaderType# Configures the main shader path
    name: str                                           # Name of this controllable object, used to refer to the object in .kf files.
    oldExtraPropName: str                               # (=NiStringExtraData)
    oldExtraInternalId: int                             # ref
    oldExtraString: str                                 # Extra string data.
    unknownByte: int                                    # Always 0.
    extraData: Ref[NiObject]                            # Extra data object index. (The first in a chain)
    extraDataList: list[Ref[NiObject]]                  # List of extra data indices.
    controller: Ref[NiObject]                           # Controller object index. (The first in a chain)

    def __init__(self, r: NiReader):
        super().__init__(r)
        BSLightingShaderProperty: bool = False
        if r.uv2 >= 83 and BSLightingShaderProperty: self.skyrimShaderType = BSLightingShaderPropertyShaderType(r.readUInt32())
        self.name = Z.string(r)
        if r.v <= 0x02030000 and r.readBool32():
            self.oldExtraPropName = Z.string(r)
            self.oldExtraInternalId = r.readUInt32()
            self.oldExtraString = Z.string(r)
        if r.v <= 0x02030000: self.unknownByte = r.readByte()
        if r.v >= 0x03000000 and r.v <= 0x04020200: self.extraData = X[NiExtraData].ref(r)
        if r.v >= 0x0A000100: self.extraDataList = r.readL32FArray(X[NiExtraData].ref)
        if r.v >= 0x03000000: self.controller = X[NiTimeController].ref(r)

# This is the most common collision object found in NIF files. It acts as a real object that
# is visible and possibly (if the body allows for it) interactive. The node itself
# is simple, it only has three properties.
# For this type of collision object, bhkRigidBody or bhkRigidBodyT is generally used.
class NiCollisionObject(NiObject): # Y
    target: Ref[NiObject]                               # Index of the AV object referring to this collision object.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.target = X[NiAVObject].ptr(r)

# Abstract audio-visual base class from which all of Gamebryo's scene graph objects inherit.
class NiAVObject(NiObjectNET): # X
    flags: Flags = 14                                   # Basic flags for AV objects. For Bethesda streams above 26 only.
    translation: Vector3                                # The translation vector.
    rotation: Matrix4x4                                 # The rotation part of the transformation matrix.
    scale: float = 1.0                                  # Scaling part (only uniform scaling is supported).
    velocity: Vector3                                   # Unknown function. Always seems to be (0, 0, 0)
    properties: list[Ref[NiObject]]                     # All rendering properties attached to this object.
    unknown1: list[int]                                 # Always 2,0,2,0.
    unknown2: int                                       # 0 or 1.
    boundingVolume: BoundingVolume
    collisionObject: Ref[NiObject]

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.uv2 > 26: self.flags = Flags(r.readUInt16())
        if r.v >= 0x03000000 and (r.uv2 <= 26): self.flags = Flags(r.readUInt16())
        self.translation = r.readVector3()
        self.rotation = r.readMatrix3x3As4x4()
        self.scale = r.readSingle()
        if r.v <= 0x04020200: self.velocity = r.readVector3()
        if r.uv2 <= 34: self.properties = r.readL32FArray(X[NiProperty].ref)
        if r.v <= 0x02030000:
            self.unknown1 = r.readPArray(None, 'I', 4)
            self.unknown2 = r.readByte()
        if r.v >= 0x03000000 and r.v <= 0x04020200 and r.readBool32(): self.boundingVolume = BoundingVolume(r)
        if r.v >= 0x0A000100: self.collisionObject = X[NiCollisionObject].ref(r)

# Abstract base class for dynamic effects such as NiLights or projected texture effects.
class NiDynamicEffect(NiAVObject): # X
    switchState: bool = True                            # If true, then the dynamic effect is applied to affected nodes during rendering.
    affectedNodes: list[Ref[NiObject]]                  # If a node appears in this list, then its entire subtree will be affected by the effect.
    affectedNodePointers: list[int]                     # As of 4.0 the pointer hash is no longer stored alongside each NiObject on disk, yet this node list still refers to the pointer hashes. Cannot leave the type as Ptr because the link will be invalid.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A01006A and r.uv2 < 130: self.switchState = r.readBool32()
        if r.v <= 0x0303000D: self.affectedNodes = r.readL32FArray(X[NiNode].ptr)
        elif r.v >= 0x04000000 and r.v <= 0x04000002: self.affectedNodePointers = r.readL32PArray(None, 'I')
        elif r.v >= 0x0A010000 and r.uv2 < 130: self.affectedNodes = r.readL32FArray(X[NiNode].ptr)

# Abstract base class representing all rendering properties. Subclasses are attached to NiAVObjects to control their rendering.
class NiProperty(NiObjectNET): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# Abstract base class that provides the base timing and update functionality for all the Gamebryo animation controllers.
class NiTimeController(NiObject): # X
    nextController: Ref[NiObject]                       # Index of the next controller.
    flags: Flags                                        # Controller flags.
                                                        #     Bit 0 : Anim type, 0=APP_TIME 1=APP_INIT
                                                        #     Bit 1-2 : Cycle type, 00=Loop 01=Reverse 10=Clamp
                                                        #     Bit 3 : Active
                                                        #     Bit 4 : Play backwards
                                                        #     Bit 5 : Is manager controlled
                                                        #     Bit 6 : Always seems to be set in Skyrim and Fallout NIFs, unknown function
    frequency: float = 1.0                              # Frequency (is usually 1.0).
    phase: float                                        # Phase (usually 0.0).
    startTime: float = 3.402823466e+38                  # Controller start time.
    stopTime: float = -3.402823466e+38                  # Controller stop time.
    target: Ref[NiObject]                               # Controller target (object index of the first controllable ancestor of this object).
    unknownInteger: int                                 # Unknown integer.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.nextController = X[NiTimeController].ref(r)
        self.flags = Flags(r.readUInt16())
        self.frequency = r.readSingle()
        self.phase = r.readSingle()
        self.startTime = r.readSingle()
        self.stopTime = r.readSingle()
        if r.v >= 0x0303000D: self.target = X[NiObjectNET].ptr(r)
        elif r.v <= 0x03010000: self.unknownInteger = r.readUInt32()

# Abstract base class for all NiTimeController objects using NiInterpolator objects to animate their target objects.
class NiInterpController(NiTimeController): # X
    managerControlled: bool

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A010068 and r.v <= 0x0A01006C: self.managerControlled = r.readBool32()

# DEPRECATED (20.5), replaced by NiMorphMeshModifier.
# Time controller for geometry morphing.
class NiGeomMorpherController(NiInterpController): # X
    extraFlags: Flags                                   # 1 = UPDATE NORMALS
    data: Ref[NiObject]                                 # Geometry morphing data index.
    alwaysUpdate: int
    interpolators: list[Ref[NiObject]]
    interpolatorWeights: list[MorphWeight]
    unknownInts: list[int]                              # Unknown.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A000102: self.extraFlags = Flags(r.readUInt16())
        self.data = X[NiMorphData].ref(r)
        if r.v >= 0x04000001: self.alwaysUpdate = r.readByte()
        if r.v >= 0x0A01006A and r.v <= 0x14000005: self.interpolators = r.readL32FArray(X[NiInterpolator].ref)
        elif r.v >= 0x14010003: self.interpolatorWeights = r.readL32FArray(lambda z: MorphWeight(r))
        if r.v >= 0x0A020000 and r.v <= 0x14000005 and (r.uv2 > 9): self.unknownInts = r.readL32PArray(None, 'I')

# Uses a single NiInterpolator to animate its target value.
class NiSingleInterpController(NiInterpController): # X
    interpolator: Ref[NiObject]

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A010068: self.interpolator = X[NiInterpolator].ref(r)

# DEPRECATED (10.2), RENAMED (10.2) to NiTransformController
# A time controller object for animation key frames.
class NiKeyframeController(NiSingleInterpController): # X
    data: Ref[NiObject]

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x0A010067: self.data = X[NiKeyframeData].ref(r)

# Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target float value.
class NiFloatInterpController(NiSingleInterpController): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# Animates the alpha value of a property using an interpolator.
class NiAlphaController(NiFloatInterpController): # X
    data: Ref[NiObject]

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x0A010067: self.data = X[NiFloatData].ref(r)

# Abstract base class for all NiInterpControllers that use a NiInterpolator to animate their target boolean value.
class NiBoolInterpController(NiSingleInterpController): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# Animates the visibility of an NiAVObject.
class NiVisController(NiBoolInterpController): # X
    data: Ref[NiObject]

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x0A010067: self.data = X[NiVisData].ref(r)

# Abstract base class for all NiInterpControllers that use an NiInterpolator to animate their target NiPoint3 value.
class NiPoint3InterpController(NiSingleInterpController): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# Time controller for material color. Flags are used for color selection in versions below 10.1.0.0.
# Bits 4-5: Target Color (00 = Ambient, 01 = Diffuse, 10 = Specular, 11 = Emissive)
# NiInterpController::GetCtlrID() string formats:
#     ['AMB', 'DIFF', 'SPEC', 'SELF_ILLUM'] (Depending on "Target Color")
class NiMaterialColorController(NiPoint3InterpController): # X
    targetColor: MaterialColor                          # Selects which color to control.
    data: Ref[NiObject]

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A010000: self.targetColor = MaterialColor(r.readUInt16())
        if r.v <= 0x0A010067: self.data = X[NiPosData].ref(r)

class MaterialData: # Y
    shaderName: str                                     # The shader name.
    shaderExtraData: int                                # Extra data associated with the shader. A value of -1 means the shader is the default implementation.
    numMaterials: int
    materialName: list[str]                             # The name of the material.
    materialExtraData: list[int]                        # Extra data associated with the material. A value of -1 means the material is the default implementation.
    activeMaterial: int = -1                            # The index of the currently active material.
    unknownByte: int = 255                              # Cyanide extension (only in version 10.2.0.0?).
    unknownInteger2: int                                # Unknown.
    materialNeedsUpdate: bool                           # Whether the materials for this object always needs to be updated before rendering with them.

    def __init__(self, r: NiReader):
        if r.v >= 0x0A000100 and r.v <= 0x14010003 and r.readBool32():
            self.shaderName = Z.string(r)
            self.shaderExtraData = r.readInt32()
        if r.v >= 0x14020005:
            self.numMaterials = r.readUInt32()
            self.materialName = r.readFArray(lambda z: Z.string(r), self.numMaterials)
            self.materialExtraData = r.readPArray(None, 'i', self.numMaterials)
            self.activeMaterial = r.readInt32()
        if r.v == 0x0A020000 and (r.uv == 1): self.unknownByte = r.readByte()
        if r.v == 0x0A040001: self.unknownInteger2 = r.readInt32()
        if r.v >= 0x14020007: self.materialNeedsUpdate = r.readBool32()

# Describes a visible scene element with vertices like a mesh, a particle system, lines, etc.
class NiGeometry(NiAVObject): # X
    bound: NiBound
    skin: Ref[NiObject]
    data: Ref[NiObject]                                 # Data index (NiTriShapeData/NiTriStripData).
    skinInstance: Ref[NiObject]
    materialData: MaterialData
    shaderProperty: Ref[NiObject]
    alphaProperty: Ref[NiObject]

    def __init__(self, r: NiReader):
        super().__init__(r)
        NiParticleSystem: bool = False
        if (r.uv2 >= 100) and NiParticleSystem:
            self.bound = r.readS(NiBound)
            self.skin = X[NiObject].ref(r)
        if r.uv2 < 100: self.data = X[NiGeometryData].ref(r)
        if (r.uv2 >= 100) and not NiParticleSystem: self.data = X[NiGeometryData].ref(r)
        if r.v >= 0x0303000D and (r.uv2 < 100): self.skinInstance = X[NiSkinInstance].ref(r)
        if (r.uv2 >= 100) and not NiParticleSystem: self.skinInstance = X[NiSkinInstance].ref(r)
        if r.v >= 0x0A000100 and (r.uv2 < 100): self.materialData = MaterialData(r)
        if r.v >= 0x0A000100 and (r.uv2 >= 100) and not NiParticleSystem: self.materialData = MaterialData(r)
        if r.v >= 0x14020007 and (r.uv == 12):
            self.shaderProperty = X[BSShaderProperty].ref(r)
            self.alphaProperty = X[NiAlphaProperty].ref(r)

# Describes a mesh, built from triangles.
class NiTriBasedGeom(NiGeometry): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

class VectorFlags(Flag): # Y
    UV_1 = 0
    UV_2 = 1 << 1
    UV_4 = 1 << 2
    UV_8 = 1 << 3
    UV_16 = 1 << 4
    UV_32 = 1 << 5
    Unk64 = 1 << 6
    Unk128 = 1 << 7
    Unk256 = 1 << 8
    Unk512 = 1 << 9
    Unk1024 = 1 << 10
    Unk2048 = 1 << 11
    Has_Tangents = 1 << 12
    Unk8192 = 1 << 13
    Unk16384 = 1 << 14
    Unk32768 = 1 << 15

class BSVectorFlags(Flag): # Y
    Has_UV = 0
    Unk2 = 1 << 1
    Unk4 = 1 << 2
    Unk8 = 1 << 3
    Unk16 = 1 << 4
    Unk32 = 1 << 5
    Unk64 = 1 << 6
    Unk128 = 1 << 7
    Unk256 = 1 << 8
    Unk512 = 1 << 9
    Unk1024 = 1 << 10
    Unk2048 = 1 << 11
    Has_Tangents = 1 << 12
    Unk8192 = 1 << 13
    Unk16384 = 1 << 14
    Unk32768 = 1 << 15

# Mesh data: vertices, vertex normals, etc.
class NiGeometryData(NiObject): # X
    groupId: int                                        # Always zero.
    numVertices: int                                    # Number of vertices.
    bsMaxVertices: int                                  # Bethesda uses this for max number of particles in NiPSysData.
    keepFlags: int                                      # Used with NiCollision objects when OBB or TRI is set.
    compressFlags: int                                  # Unknown.
    vertices: list[Vector3]                             # The mesh vertices.
    vectorFlags: VectorFlags
    bsVectorFlags: BSVectorFlags
    materialCrc: int
    normals: list[Vector3]                              # The lighting normals.
    tangents: list[Vector3]                             # Tangent vectors.
    bitangents: list[Vector3]                           # Bitangent vectors.
    unkFloats: list[float]
    center: Vector3                                     # Center of the bounding box (smallest box that contains all vertices) of the mesh.
    radius: float                                       # Radius of the mesh: maximal Euclidean distance between the center and all vertices.
    unknown13shorts: list[int]                          # Unknown, always 0?
    vertexColors: list[Color4]                          # The vertex colors.
    numUvSets: int                                      # The lower 6 (or less?) bits of this field represent the number of UV texture sets. The other bits are probably flag bits. For versions 10.1.0.0 and up, if bit 12 is set then extra vectors are present after the normals.
    uvSets: list[list[TexCoord]]                        # The UV texture coordinates. They follow the OpenGL standard: some programs may require you to flip the second coordinate.
    consistencyFlags: ConsistencyType = ConsistencyType.CT_MUTABLE # Consistency Flags
    additionalData: Ref[NiObject]                       # Unknown.

    def __init__(self, r: NiReader):
        super().__init__(r)
        NiPSysData: bool = False
        if r.v >= 0x0A010072: self.groupId = r.readInt32()
        if not NiPSysData or r.UV2 >= 34: self.numVertices = r.readUInt16()
        if (r.uv2 >= 34) and NiPSysData: self.bsMaxVertices = r.readUInt16()
        if r.v >= 0x0A010000:
            self.keepFlags = r.readByte()
            self.compressFlags = r.readByte()
        hasVertices = r.readUInt32()
        if (self.hasVertices > 0) and (self.hasVertices != 15): self.vertices = r.readPArray(None, '3f', self.numVertices)
        if r.v >= 0x14030101 and self.hasVertices == 15: self.vertices = r.readFArray(lambda z: Vector3(r.readHalf(), r.readHalf(), r.readHalf()), self.numVertices)
        if r.v >= 0x0A000100 and not ((r.v == 0x14020007) and (r.uv2 > 0)): self.vectorFlags = VectorFlags(r.readUInt16())
        if ((r.v == 0x14020007) and (r.uv2 > 0)): self.bsVectorFlags = BSVectorFlags(r.readUInt16())
        if r.v == 0x14020007 and (r.uv == 12): self.materialCrc = r.readUInt32()
        hasNormals = r.readUInt32()
        if (self.hasNormals > 0) and (self.hasNormals != 6): self.normals = r.readPArray(None, '3f', self.numVertices)
        if r.v >= 0x14030101 and self.hasNormals == 6: self.normals = r.readFArray(lambda z: Vector3(r.readByte(), r.readByte(), r.readByte()), self.numVertices)
        if r.v >= 0x0A010000 and (HasNormals != 0) and ((VectorFlags | BSVectorFlags) & 4096) != 0:
            self.tangents = r.readPArray(None, '3f', self.numVertices)
            self.bitangents = r.readPArray(None, '3f', self.numVertices)
        if r.v == 0x14030009 and (r.uv == 0x20000) or (r.uv == 0x30000) and r.readBool32(): self.unkFloats = r.readPArray(None, 'f', self.numVertices)
        self.center = r.readVector3()
        self.radius = r.readSingle()
        if r.v == 0x14030009 and (r.uv == 0x20000) or (r.uv == 0x30000): self.unknown13shorts = r.readPArray(None, 'h', 13)
        hasVertexColors = r.readUInt32()
        if (self.hasVertexColors > 0) and (self.hasVertexColors != 7): self.vertexColors = r.readFArray(lambda z: Color4(r), self.numVertices)
        if r.v >= 0x14030101 and self.hasVertexColors == 7: self.vertexColors = r.readFArray(lambda z: Color4(r.readBytes(4)), self.numVertices)
        if r.v <= 0x04020200: self.numUvSets = r.readUInt16()
        hasUv = r.readBool32() if r.v <= 0x04000002 else None
        if (self.hasVertices > 0) and (self.hasVertices != 15): self.uvSets = r.readFArray(lambda k: r.readSArray<TexCoord>(self.numVertices), ((NumUVSets & 63) | (VectorFlags & 63) | (BSVectorFlags & 1)))
        if r.v >= 0x14030101 and self.hasVertices == 15: self.uvSets = r.readFArray(lambda k: r.readFArray(lambda z: TexCoord(r, true), self.numVertices), ((NumUVSets & 63) | (VectorFlags & 63) | (BSVectorFlags & 1)))
        if r.v >= 0x0A000100: self.consistencyFlags = ConsistencyType(r.readUInt16())
        if r.v >= 0x14000004: self.additionalData = X[AbstractAdditionalGeometryData].ref(r)

class AbstractAdditionalGeometryData(NiObject): # Y
    def __init__(self, r: NiReader):
        super().__init__(r)

# Describes a mesh, built from triangles.
class NiTriBasedGeomData(NiGeometryData): # X
    numTriangles: int                                   # Number of triangles.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.numTriangles = r.readUInt16()

# Transparency. Flags 0x00ED.
class NiAlphaProperty(NiProperty): # X
    flags: Flags = 4844                                 # Bit 0 : alpha blending enable
                                                        #     Bits 1-4 : source blend mode
                                                        #     Bits 5-8 : destination blend mode
                                                        #     Bit 9 : alpha test enable
                                                        #     Bit 10-12 : alpha test mode
                                                        #     Bit 13 : no sorter flag ( disables triangle sorting )
                                                        # 
                                                        #     blend modes (glBlendFunc):
                                                        #     0000 GL_ONE
                                                        #     0001 GL_ZERO
                                                        #     0010 GL_SRC_COLOR
                                                        #     0011 GL_ONE_MINUS_SRC_COLOR
                                                        #     0100 GL_DST_COLOR
                                                        #     0101 GL_ONE_MINUS_DST_COLOR
                                                        #     0110 GL_SRC_ALPHA
                                                        #     0111 GL_ONE_MINUS_SRC_ALPHA
                                                        #     1000 GL_DST_ALPHA
                                                        #     1001 GL_ONE_MINUS_DST_ALPHA
                                                        #     1010 GL_SRC_ALPHA_SATURATE
                                                        # 
                                                        #     test modes (glAlphaFunc):
                                                        #     000 GL_ALWAYS
                                                        #     001 GL_LESS
                                                        #     010 GL_EQUAL
                                                        #     011 GL_LEQUAL
                                                        #     100 GL_GREATER
                                                        #     101 GL_NOTEQUAL
                                                        #     110 GL_GEQUAL
                                                        #     111 GL_NEVER
    threshold: int = 128                                # Threshold for alpha testing (see: glAlphaFunc)
    unknownShort1: int                                  # Unknown
    unknownInt2: int                                    # Unknown

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.flags = Flags(r.readUInt16())
        self.threshold = r.readByte()
        if r.v <= 0x02030000: self.unknownShort1 = r.readUInt16()
        if r.v >= 0x14030101 and r.v <= 0x14030102: self.unknownShort1 = r.readUInt16()
        if r.v <= 0x02030000: self.unknownInt2 = r.readUInt32()

# Generic rotating particles data object.
class NiParticlesData(NiGeometryData): # X
    numParticles: int                                   # The maximum number of particles (matches the number of vertices).
    particleRadius: float                               # The particles' size.
    radii: list[float]                                  # The individual particle sizes.
    numActive: int                                      # The number of active particles at the time the system was saved. This is also the number of valid entries in the following arrays.
    sizes: list[float]                                  # The individual particle sizes.
    rotations: list[Quaternion]                         # The individual particle rotations.
    rotationAngles: list[float]                         # Angles of rotation
    rotationAxes: list[Vector3]                         # Axes of rotation.
    numSubtextureOffsets: int                           # How many quads to use in BSPSysSubTexModifier for texture atlasing
    subtextureOffsets: list[Vector4]                    # Defines UV offsets
    aspectRatio: float                                  # Sets aspect ratio for Subtexture Offset UV quads
    aspectFlags: int
    speedtoAspectAspect2: float
    speedtoAspectSpeed1: float
    speedtoAspectSpeed2: float

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x04000002: self.numParticles = r.readUInt16()
        if r.v <= 0x0A000100: self.particleRadius = r.readSingle()
        if r.v >= 0x0A010000 and not ((r.v == 0x14020007) and (r.uv2 > 0)) and r.readBool32(): self.radii = r.readPArray(None, 'f', self.numVertices)
        self.numActive = r.readUInt16()
        if not ((r.v == 0x14020007) and (r.uv2 > 0)) and r.readBool32(): self.sizes = r.readPArray(None, 'f', self.numVertices)
        if r.v >= 0x0A000100 and not ((r.v == 0x14020007) and (r.uv2 > 0)) and r.readBool32(): self.rotations = r.readFArray(lambda z: r.readQuaternionWFirst(), self.numVertices)
        hasRotationAngles = r.readBool32() if r.v >= 0x14000004 else None
        if not ((r.v == 0x14020007) and (r.uv2 > 0)) and self.hasRotationAngles: self.rotationAngles = r.readPArray(None, 'f', self.numVertices)
        if r.v >= 0x14000004 and not ((r.v == 0x14020007) and (r.uv2 > 0)) and r.readBool32(): self.rotationAxes = r.readPArray(None, '3f', self.numVertices)
        hasTextureIndices = r.readBool32() if ((r.v == 0x14020007) and (r.uv2 > 0)) else None
        if r.uv2 > 34: self.numSubtextureOffsets = r.readUInt32()
        if ((r.v == 0x14020007) and (r.uv2 > 0)): self.subtextureOffsets = r.readL8PArray(None, '4f')
        if r.uv2 > 34:
            self.aspectRatio = r.readSingle()
            self.aspectFlags = r.readUInt16()
            self.speedtoAspectAspect2 = r.readSingle()
            self.speedtoAspectSpeed1 = r.readSingle()
            self.speedtoAspectSpeed2 = r.readSingle()

# Rotating particles data object.
class NiRotatingParticlesData(NiParticlesData): # X
    rotations2: list[Quaternion]                        # The individual particle rotations.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x04020200 and r.readBool32(): self.rotations2 = r.readFArray(lambda z: r.readQuaternionWFirst(), self.numVertices)

# Particle system data object (with automatic normals?).
class NiAutoNormalParticlesData(NiParticlesData): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# Camera object.
class NiCamera(NiAVObject): # X
    cameraFlags: int                                    # Obsolete flags.
    frustumLeft: float                                  # Frustrum left.
    frustumRight: float                                 # Frustrum right.
    frustumTop: float                                   # Frustrum top.
    frustumBottom: float                                # Frustrum bottom.
    frustumNear: float                                  # Frustrum near.
    frustumFar: float                                   # Frustrum far.
    useOrthographicProjection: bool                     # Determines whether perspective is used.  Orthographic means no perspective.
    viewportLeft: float                                 # Viewport left.
    viewportRight: float                                # Viewport right.
    viewportTop: float                                  # Viewport top.
    viewportBottom: float                               # Viewport bottom.
    lodAdjust: float                                    # Level of detail adjust.
    scene: Ref[NiObject]
    numScreenPolygons: int = 0                          # Deprecated. Array is always zero length on disk write.
    numScreenTextures: int = 0                          # Deprecated. Array is always zero length on disk write.
    unknownInt3: int                                    # Unknown.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A010000: self.cameraFlags = r.readUInt16()
        self.frustumLeft = r.readSingle()
        self.frustumRight = r.readSingle()
        self.frustumTop = r.readSingle()
        self.frustumBottom = r.readSingle()
        self.frustumNear = r.readSingle()
        self.frustumFar = r.readSingle()
        if r.v >= 0x0A010000: self.useOrthographicProjection = r.readBool32()
        self.viewportLeft = r.readSingle()
        self.viewportRight = r.readSingle()
        self.viewportTop = r.readSingle()
        self.viewportBottom = r.readSingle()
        self.lodAdjust = r.readSingle()
        self.scene = X[NiAVObject].ref(r)
        self.numScreenPolygons = r.readUInt32()
        if r.v >= 0x04020100: self.numScreenTextures = r.readUInt32()
        if r.v <= 0x03010000: self.unknownInt3 = r.readUInt32()

# Wrapper for color animation keys.
class NiColorData(NiObject): # X
    data: KeyGroup[T]                                   # The color keys.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.data = KeyGroup[T](r)

# Wrapper for 1D (one-dimensional) floating point animation keys.
class NiFloatData(NiObject): # X
    data: KeyGroup[T]                                   # The keys.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.data = KeyGroup[T](r)

# LEGACY (pre-10.1) particle modifier. Applies a gravitational field on the particles.
class NiGravity(NiParticleModifier): # X
    unknownFloat1: float                                # Unknown.
    force: float                                        # The strength/force of this gravity.
    type: FieldType                                     # The force field type.
    position: Vector3                                   # The position of the mass point relative to the particle system.
    direction: Vector3                                  # The direction of the applied acceleration.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0303000D: self.unknownFloat1 = r.readSingle()
        self.force = r.readSingle()
        self.type = FieldType(r.readUInt32())
        self.position = r.readVector3()
        self.direction = r.readVector3()

# DEPRECATED (10.2), RENAMED (10.2) to NiTransformData.
# Wrapper for transformation animation keys.
class NiKeyframeData(NiObject): # X
    numRotationKeys: int                                # The number of quaternion rotation keys. If the rotation type is XYZ (type 4) then this *must* be set to 1, and in this case the actual number of keys is stored in the XYZ Rotations field.
    rotationType: KeyType                               # The type of interpolation to use for rotation.  Can also be 4 to indicate that separate X, Y, and Z values are used for the rotation instead of Quaternions.
    quaternionKeys: list[QuatKey[T]]                    # The rotation keys if Quaternion rotation is used.
    order: float
    xyzRotations: list[KeyGroup[T]]                     # Individual arrays of keys for rotating X, Y, and Z individually.
    translations: KeyGroup[T]                           # Translation keys.
    scales: KeyGroup[T]                                 # Scale keys.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.numRotationKeys = r.readUInt32()
        if self.numRotationKeys != 0: self.rotationType = KeyType(r.readUInt32())
        if RotationType != KeyType.XYZ_ROTATION_KEY: self.quaternionKeys = r.readFArray(lambda z: QuatKey[T](r, self.RotationType), self.numRotationKeys)
        else:
            if r.v <= 0x0A010000: self.order = r.readSingle()
            self.xyzRotations = r.readFArray(lambda z: KeyGroup[T](r), 3)
        self.translations = KeyGroup[T](r)
        self.scales = KeyGroup[T](r)

# Describes the surface properties of an object e.g. translucency, ambient color, diffuse color, emissive color, and specular color.
class NiMaterialProperty(NiProperty): # X
    flags: Flags                                        # Property flags.
    ambientColor: Color3 = Color3(1.0, 1.0, 1.0)        # How much the material reflects ambient light.
    diffuseColor: Color3 = Color3(1.0, 1.0, 1.0)        # How much the material reflects diffuse light.
    specularColor: Color3 = Color3(1.0, 1.0, 1.0)       # How much light the material reflects in a specular manner.
    emissiveColor: Color3 = Color3(0.0, 0.0, 0.0)       # How much light the material emits.
    glossiness: float = 10.0                            # The material glossiness.
    alpha: float = 1.0                                  # The material transparency (1=non-transparant). Refer to a NiAlphaProperty object in this material's parent NiTriShape object, when alpha is not 1.
    emissiveMult: float = 1.0

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x03000000 and r.v <= 0x0A000102: self.flags = Flags(r.readUInt16())
        if r.uv2 < 26:
            self.ambientColor = Color3(r)
            self.diffuseColor = Color3(r)
        self.specularColor = Color3(r)
        self.emissiveColor = Color3(r)
        self.glossiness = r.readSingle()
        self.alpha = r.readSingle()
        if r.uv2 > 21: self.emissiveMult = r.readSingle()

# DEPRECATED (20.5), replaced by NiMorphMeshModifier.
# Geometry morphing data.
class NiMorphData(NiObject): # X
    numMorphs: int                                      # Number of morphing object.
    numVertices: int                                    # Number of vertices.
    relativeTargets: int = 1                            # This byte is always 1 in all official files.
    morphs: list[Morph]                                 # The geometry morphing objects.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.numMorphs = r.readUInt32()
        self.numVertices = r.readUInt32()
        self.relativeTargets = r.readByte()
        self.morphs = r.readFArray(lambda z: Morph(r, NumVertices), self.numMorphs)

# Generic node object for grouping.
class NiNode(NiAVObject): # X
    children: list[Ref[NiObject]]                       # List of child node object indices.
    effects: list[Ref[NiObject]]                        # List of node effects. ADynamicEffect?

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.children = r.readL32FArray(X[NiAVObject].ref)
        if r.uv2 < 130: self.effects = r.readL32FArray(X[NiDynamicEffect].ref)

# Morrowind specific.
class AvoidNode(NiNode): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# These nodes will always be rotated to face the camera creating a billboard effect for any attached objects.
# 
# In pre-10.1.0.0 the Flags field is used for BillboardMode.
# Bit 0: hidden
# Bits 1-2: collision mode
# Bit 3: unknown (set in most official meshes)
# Bits 5-6: billboard mode
# 
# Collision modes:
# 00 NONE
# 01 USE_TRIANGLES
# 10 USE_OBBS
# 11 CONTINUE
# 
# Billboard modes:
# 00 ALWAYS_FACE_CAMERA
# 01 ROTATE_ABOUT_UP
# 10 RIGID_FACE_CAMERA
# 11 ALWAYS_FACE_CENTER
class NiBillboardNode(NiNode): # X
    billboardMode: BillboardMode                        # The way the billboard will react to the camera.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A010000: self.billboardMode = BillboardMode(r.readUInt16())

# Bethesda-specific extension of Node with animation properties stored in the flags, often 42?
class NiBSAnimationNode(NiNode): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# Unknown.
class NiBSParticleNode(NiNode): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# NiPalette objects represent mappings from 8-bit indices to 24-bit RGB or 32-bit RGBA colors.
class NiPalette(NiObject): # X
    hasAlpha: int
    numEntries: int = 256                               # The number of palette entries. Always 256 but can also be 16.
    palette: list[Color4]                               # The color palette.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.hasAlpha = r.readByte()
        self.numEntries = r.readUInt32()
        if self.numEntries == 16: self.palette = r.readFArray(lambda z: Color4(r.readBytes(4)), 16)
        else: self.palette = r.readFArray(lambda z: Color4(r.readBytes(4)), 256)

# LEGACY (pre-10.1) particle modifier.
class NiParticleBomb(NiParticleModifier): # X
    decay: float
    duration: float
    deltaV: float
    start: float
    decayType: DecayType
    symmetryType: SymmetryType
    position: Vector3                                   # The position of the mass point relative to the particle system?
    direction: Vector3                                  # The direction of the applied acceleration?

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.decay = r.readSingle()
        self.duration = r.readSingle()
        self.deltaV = r.readSingle()
        self.start = r.readSingle()
        self.decayType = DecayType(r.readUInt32())
        if r.v >= 0x0401000C: self.symmetryType = SymmetryType(r.readUInt32())
        self.position = r.readVector3()
        self.direction = r.readVector3()

# LEGACY (pre-10.1) particle modifier.
class NiParticleColorModifier(NiParticleModifier): # X
    colorData: Ref[NiObject]

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.colorData = X[NiColorData].ref(r)

# LEGACY (pre-10.1) particle modifier.
class NiParticleGrowFade(NiParticleModifier): # X
    grow: float                                         # The time from the beginning of the particle lifetime during which the particle grows.
    fade: float                                         # The time from the end of the particle lifetime during which the particle fades.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.grow = r.readSingle()
        self.fade = r.readSingle()

# LEGACY (pre-10.1) particle modifier.
class NiParticleMeshModifier(NiParticleModifier): # X
    particleMeshes: list[Ref[NiObject]]

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.particleMeshes = r.readL32FArray(X[NiAVObject].ref)

# LEGACY (pre-10.1) particle modifier.
class NiParticleRotation(NiParticleModifier): # X
    randomInitialAxis: int
    initialAxis: Vector3
    rotationSpeed: float

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.randomInitialAxis = r.readByte()
        self.initialAxis = r.readVector3()
        self.rotationSpeed = r.readSingle()

# Generic particle system node.
class NiParticles(NiGeometry): # X
    vertexDesc: BSVertexDesc

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.uv2 >= 100: self.vertexDesc = r.readS(BSVertexDesc)

# LEGACY (pre-10.1). NiParticles which do not house normals and generate them at runtime.
class NiAutoNormalParticles(NiParticles): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# A generic particle system time controller object.
class NiParticleSystemController(NiTimeController): # X
    oldSpeed: int                                       # Particle speed in old files
    speed: float                                        # Particle speed
    speedRandom: float                                  # Particle random speed modifier
    verticalDirection: float                            # vertical emit direction [radians]
                                                        #     0.0 : up
                                                        #     1.6 : horizontal
                                                        #     3.1416 : down
    verticalAngle: float                                # emitter's vertical opening angle [radians]
    horizontalDirection: float                          # horizontal emit direction
    horizontalAngle: float                              # emitter's horizontal opening angle
    unknownNormal: Vector3                              # Unknown.
    unknownColor: Color4                                # Unknown.
    size: float                                         # Particle size
    emitStartTime: float                                # Particle emit start time
    emitStopTime: float                                 # Particle emit stop time
    unknownByte: int                                    # Unknown byte, (=0)
    oldEmitRate: int                                    # Particle emission rate in old files
    emitRate: float                                     # Particle emission rate (particles per second)
    lifetime: float                                     # Particle lifetime
    lifetimeRandom: float                               # Particle lifetime random modifier
    emitFlags: int                                      # Bit 0: Emit Rate toggle bit (0 = auto adjust, 1 = use Emit Rate value)
    startRandom: Vector3                                # Particle random start translation vector
    emitter: Ref[NiObject]                              # This index targets the particle emitter object (TODO: find out what type of object this refers to).
    unknownShort2: int                                  # ? short=0 ?
    unknownFloat13: float                               # ? float=1.0 ?
    unknownInt1: int                                    # ? int=1 ?
    unknownInt2: int                                    # ? int=0 ?
    unknownShort3: int                                  # ? short=0 ?
    particleVelocity: Vector3                           # Particle velocity
    particleUnknownVector: Vector3                      # Unknown
    particleLifetime: float                             # The particle's age.
    particleLink: Ref[NiObject]
    particleTimestamp: int                              # Timestamp of the last update.
    particleUnknownShort: int                           # Unknown short
    particleVertexId: int                               # Particle/vertex index matches array index
    numParticles: int                                   # Size of the following array. (Maximum number of simultaneous active particles)
    numValid: int                                       # Number of valid entries in the following array. (Number of active particles at the time the system was saved)
    particles: list[Particle]                           # Individual particle modifiers?
    unknownLink: Ref[NiObject]                          # unknown int (=0xffffffff)
    particleExtra: Ref[NiObject]                        # Link to some optional particle modifiers (NiGravity, NiParticleGrowFade, NiParticleBomb, ...)
    unknownLink2: Ref[NiObject]                         # Unknown int (=0xffffffff)
    trailer: int                                        # Trailing null byte
    colorData: Ref[NiObject]
    unknownFloat1: float
    unknownFloats2: list[float]

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x03010000: self.oldSpeed = r.readUInt32()
        if r.v >= 0x0303000D: self.speed = r.readSingle()
        self.speedRandom = r.readSingle()
        self.verticalDirection = r.readSingle()
        self.verticalAngle = r.readSingle()
        self.horizontalDirection = r.readSingle()
        self.horizontalAngle = r.readSingle()
        self.unknownNormal = r.readVector3()
        self.unknownColor = Color4(r)
        self.size = r.readSingle()
        self.emitStartTime = r.readSingle()
        self.emitStopTime = r.readSingle()
        if r.v >= 0x04000002: self.unknownByte = r.readByte()
        if r.v <= 0x03010000: self.oldEmitRate = r.readUInt32()
        if r.v >= 0x0303000D: self.emitRate = r.readSingle()
        self.lifetime = r.readSingle()
        self.lifetimeRandom = r.readSingle()
        if r.v >= 0x04000002: self.emitFlags = r.readUInt16()
        self.startRandom = r.readVector3()
        self.emitter = X[NiObject].ptr(r)
        if r.v >= 0x04000002:
            self.unknownShort2 = r.readUInt16()
            self.unknownFloat13 = r.readSingle()
            self.unknownInt1 = r.readUInt32()
            self.unknownInt2 = r.readUInt32()
            self.unknownShort3 = r.readUInt16()
        if r.v <= 0x03010000:
            self.particleVelocity = r.readVector3()
            self.particleUnknownVector = r.readVector3()
            self.particleLifetime = r.readSingle()
            self.particleLink = X[NiObject].ref(r)
            self.particleTimestamp = r.readUInt32()
            self.particleUnknownShort = r.readUInt16()
            self.particleVertexId = r.readUInt16()
        if r.v >= 0x04000002:
            self.numParticles = r.readUInt16()
            self.numValid = r.readUInt16()
            self.particles = r.readSArray<Particle>(self.numParticles)
            self.unknownLink = X[NiObject].ref(r)
        self.particleExtra = X[NiParticleModifier].ref(r)
        self.unknownLink2 = X[NiObject].ref(r)
        if r.v >= 0x04000002: self.trailer = r.readByte()
        if r.v <= 0x03010000:
            self.colorData = X[NiColorData].ref(r)
            self.unknownFloat1 = r.readSingle()
            self.unknownFloats2 = r.readPArray(None, 'f', self.particleUnknownShort)

# A particle system controller, used by BS in conjunction with NiBSParticleNode.
class NiBSPArrayController(NiParticleSystemController): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# DEPRECATED (10.2), REMOVED (20.5). Replaced by NiTransformController and NiPathInterpolator.
# Time controller for a path.
class NiPathController(NiTimeController): # X
    pathFlags: PathFlags
    bankDir: int = 1                                    # -1 = Negative, 1 = Positive
    maxBankAngle: float                                 # Max angle in radians.
    smoothing: float
    followAxis: int                                     # 0, 1, or 2 representing X, Y, or Z.
    pathData: Ref[NiObject]
    percentData: Ref[NiObject]

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v >= 0x0A010000: self.pathFlags = PathFlags(r.readUInt16())
        self.bankDir = r.readInt32()
        self.maxBankAngle = r.readSingle()
        self.smoothing = r.readSingle()
        self.followAxis = r.readInt16()
        self.pathData = X[NiPosData].ref(r)
        self.percentData = X[NiFloatData].ref(r)

class PixelFormatComponent: # Y
    def __init__(self, r: NiReader):
        self.type: PixelComponent = PixelComponent(r.readUInt32()) # Component Type
        self.convention: PixelRepresentation = PixelRepresentation(r.readUInt32()) # Data Storage Convention
        self.bitsPerChannel: int = r.readByte()         # Bits per component
        self.isSigned: bool = r.readBool32()

class NiPixelFormat(NiObject): # Y
    pixelFormat: PixelFormat                            # The format of the pixels in this internally stored image.
    redMask: int                                        # 0x000000ff (for 24bpp and 32bpp) or 0x00000000 (for 8bpp)
    greenMask: int                                      # 0x0000ff00 (for 24bpp and 32bpp) or 0x00000000 (for 8bpp)
    blueMask: int                                       # 0x00ff0000 (for 24bpp and 32bpp) or 0x00000000 (for 8bpp)
    alphaMask: int                                      # 0xff000000 (for 32bpp) or 0x00000000 (for 24bpp and 8bpp)
    bitsPerPixel: int                                   # Bits per pixel, 0 (Compressed), 8, 24 or 32.
    oldFastCompare: bytearray                           # [96,8,130,0,0,65,0,0] if 24 bits per pixel
                                                        #     [129,8,130,32,0,65,12,0] if 32 bits per pixel
                                                        #     [34,0,0,0,0,0,0,0] if 8 bits per pixel
                                                        #     [X,0,0,0,0,0,0,0] if 0 (Compressed) bits per pixel where X = PixelFormat
    tiling: PixelTiling                                 # Seems to always be zero.
    rendererHint: int
    extraData: int
    flags: int
    sRgbSpace: bool
    channels: list[PixelFormatComponent]                # Channel Data

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.pixelFormat = PixelFormat(r.readUInt32())
        if r.v <= 0x0A030002:
            self.redMask = r.readUInt32()
            self.greenMask = r.readUInt32()
            self.blueMask = r.readUInt32()
            self.alphaMask = r.readUInt32()
            self.bitsPerPixel = r.readUInt32()
            self.oldFastCompare = r.readBytes(8)
        if r.v >= 0x0A010000 and r.v <= 0x0A030002: self.tiling = PixelTiling(r.readUInt32())
        if r.v >= 0x0A030003:
            self.bitsPerPixel = r.readByte()
            self.rendererHint = r.readUInt32()
            self.extraData = r.readUInt32()
            self.flags = r.readByte()
            self.tiling = PixelTiling(r.readUInt32())
        if r.v >= 0x14030004: self.sRgbSpace = r.readBool32()
        if r.v >= 0x0A030003: self.channels = r.readFArray(lambda z: PixelFormatComponent(r), 4)

# A texture.
class NiPixelData(NiPixelFormat): # X
    palette: Ref[NiObject]
    numMipmaps: int
    bytesPerPixel: int
    mipmaps: list[MipMap]
    numPixels: int
    numFaces: int = 1
    pixelData: bytearray

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.palette = X[NiPalette].ref(r)
        self.numMipmaps = r.readUInt32()
        self.bytesPerPixel = r.readUInt32()
        self.mipmaps = r.readSArray<MipMap>(self.numMipmaps)
        self.numPixels = r.readUInt32()
        if r.v >= 0x0A030006: self.numFaces = r.readUInt32()
        if r.v <= 0x0A030005: self.pixelData = r.readBytes(self.numPixels)
        if r.v >= 0x0A030006: self.pixelData = r.readBytes(self.numPixels * self.numFaces)

# Wrapper for position animation keys.
class NiPosData(NiObject): # X
    data: KeyGroup[T]

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.data = KeyGroup[T](r)

# Unknown.
class NiRotatingParticles(NiParticles): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# Determines whether flat shading or smooth shading is used on a shape.
class NiShadeProperty(NiProperty): # X
    flags: Flags = 1                                    # Bit 0: Enable smooth phong shading on this shape. Otherwise, hard-edged flat shading will be used on this shape.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.uv2 <= 34: self.flags = Flags(r.readUInt16())

# Skinning data.
class NiSkinData(NiObject): # X
    skinTransform: NiTransform                          # Offset of the skin from this bone in bind position.
    numBones: int                                       # Number of bones.
    skinPartition: Ref[NiObject]                        # This optionally links a NiSkinPartition for hardware-acceleration information.
    hasVertexWeights: int = 1                           # Enables Vertex Weights for this NiSkinData.
    boneList: list[BoneData]                            # Contains offset data for each node that this skin is influenced by.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.skinTransform = NiTransform(r)
        self.numBones = r.readUInt32()
        if r.v >= 0x04000002 and r.v <= 0x0A010000: self.skinPartition = X[NiSkinPartition].ref(r)
        if r.v >= 0x04020100: self.hasVertexWeights = r.readByte()
        self.boneList = r.readFArray(lambda z: BoneData(r, HasVertexWeights), self.numBones)

# Skinning instance.
class NiSkinInstance(NiObject): # X
    data: Ref[NiObject]                                 # Skinning data reference.
    skinPartition: Ref[NiObject]                        # Refers to a NiSkinPartition objects, which partitions the mesh such that every vertex is only influenced by a limited number of bones.
    skeletonRoot: Ref[NiObject]                         # Armature root node.
    bones: list[Ref[NiObject]]                          # List of all armature bones.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.data = X[NiSkinData].ref(r)
        if r.v >= 0x0A010065: self.skinPartition = X[NiSkinPartition].ref(r)
        self.skeletonRoot = X[NiNode].ptr(r)
        self.bones = r.readL32FArray(X[NiNode].ptr)

# Skinning data, optimized for hardware skinning. The mesh is partitioned in submeshes such that each vertex of a submesh is influenced only by a limited and fixed number of bones.
class NiSkinPartition(NiObject): # X
    numSkinPartitionBlocks: int
    skinPartitionBlocks: list[SkinPartition]            # Skin partition objects.
    dataSize: int
    vertexSize: int
    vertexDesc: BSVertexDesc
    vertexData: list[BSVertexDataSSE]
    partition: list[SkinPartition]

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.numSkinPartitionBlocks = r.readUInt32()
        if not ((r.v == 0x14020007) and (r.uv2 == 100)): self.skinPartitionBlocks = r.readFArray(lambda z: SkinPartition(r), self.numSkinPartitionBlocks)
        if r.uv2 == 100:
            self.dataSize = r.readUInt32()
            self.vertexSize = r.readUInt32()
            self.vertexDesc = r.readS(BSVertexDesc)
            if self.dataSize > 0: self.vertexData = r.readFArray(lambda z: BSVertexDataSSE(r, VertexDesc.VertexAttributes), self.dataSize / self.vertexSize)
            self.partition = r.readFArray(lambda z: SkinPartition(r), self.numSkinPartitionBlocks)

# A texture.
class NiTexture(NiObjectNET): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# NiTexture::FormatPrefs. These preferences are a request to the renderer to use a format the most closely matches the settings and may be ignored.
class FormatPrefs: # Y
    def __init__(self, r: NiReader):
        self.pixelLayout: PixelLayout = PixelLayout(r.readUInt32()) # Requests the way the image will be stored.
        self.useMipmaps: MipMapFormat = MipMapFormat(r.readUInt32()) # Requests if mipmaps are used or not.
        self.alphaFormat: AlphaFormat = AlphaFormat(r.readUInt32()) # Requests no alpha, 1-bit alpha, or

# Describes texture source and properties.
class NiSourceTexture(NiTexture): # X
    useExternal: int = 1                                # Is the texture external?
    fileName: str                                       # The external texture file name.
    unknownLink: Ref[NiObject]                          # Unknown.
    unknownByte: int = 1                                # Unknown. Seems to be set if Pixel Data is present?
    pixelData: Ref[NiObject]                            # NiPixelData or NiPersistentSrcTextureRendererData
    formatPrefs: FormatPrefs                            # A set of preferences for the texture format. They are a request only and the renderer may ignore them.
    isStatic: int = 1                                   # If set, then the application cannot assume that any dynamic changes to the pixel data will show in the rendered image.
    directRender: bool = True                           # A hint to the renderer that the texture can be loaded directly from a texture file into a renderer-specific resource, bypassing the NiPixelData object.
    persistRenderData: bool = False                     # Pixel Data is NiPersistentSrcTextureRendererData instead of NiPixelData.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.useExternal = r.readByte()
        if self.useExternal == 1:
            self.fileName = r.readL32Encoding()
            if r.v >= 0x0A010000: self.unknownLink = X[NiObject].ref(r)
        if self.useExternal == 0:
            if r.v <= 0x0A000100: self.unknownByte = r.readByte()
            if r.v >= 0x0A010000: self.fileName = r.readL32Encoding()
            self.pixelData = X[NiPixelFormat].ref(r)
        self.formatPrefs = FormatPrefs(r)
        self.isStatic = r.readByte()
        if r.v >= 0x0A010067: self.directRender = r.readBool32()
        if r.v >= 0x14020004: self.persistRenderData = r.readBool32()

# Apparently commands for an optimizer instructing it to keep things it would normally discard.
# Also refers to NiNode objects (through their name) in animation .kf files.
class NiStringExtraData(NiExtraData): # X
    bytesRemaining: int                                 # The number of bytes left in the record.  Equals the length of the following string + 4.
    stringData: str                                     # The string.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x04020200: self.bytesRemaining = r.readUInt32()
        self.stringData = Z.string(r)

# Extra data, used to name different animation sequences.
class NiTextKeyExtraData(NiExtraData): # X
    unknownInt1: int                                    # Unknown.  Always equals zero in all official files.
    textKeys: list[Key[T]]                              # List of textual notes and at which time they take effect. Used for designating the start and stop of animations and the triggering of sounds.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x04020200: self.unknownInt1 = r.readUInt32()
        self.textKeys = r.readL32FArray(lambda z: Key[T](r, self.KeyType.LINEAR_KEY))

# Represents an effect that uses projected textures such as projected lights (gobos), environment maps, and fog maps.
class NiTextureEffect(NiDynamicEffect): # X
    modelProjectionMatrix: Matrix4x4                    # Model projection matrix.  Always identity?
    modelProjectionTransform: Vector3                   # Model projection transform.  Always (0,0,0)?
    textureFiltering: TexFilterMode = TexFilterMode.FILTER_TRILERP # Texture Filtering mode.
    maxAnisotropy: int
    textureClamping: TexClampMode = TexClampMode.WRAP_S_WRAP_T # Texture Clamp mode.
    textureType: TextureType = TextureType.TEX_ENVIRONMENT_MAP # The type of effect that the texture is used for.
    coordinateGenerationType: CoordGenType = CoordGenType.CG_SPHERE_MAP # The method that will be used to generate UV coordinates for the texture effect.
    image: Ref[NiObject]                                # Image index.
    sourceTexture: Ref[NiObject]                        # Source texture index.
    enablePlane: int = 0                                # Determines whether a clipping plane is used.
    plane: NiPlane
    pS2L: int = 0
    pS2K: int = -75
    unknownShort: int                                   # Unknown: 0.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.modelProjectionMatrix = r.readMatrix3x3As4x4()
        self.modelProjectionTransform = r.readVector3()
        self.textureFiltering = TexFilterMode(r.readUInt32())
        if r.v >= 0x14050004: self.maxAnisotropy = r.readUInt16()
        self.textureClamping = TexClampMode(r.readUInt32())
        self.textureType = TextureType(r.readUInt32())
        self.coordinateGenerationType = CoordGenType(r.readUInt32())
        if r.v <= 0x03010000: self.image = X[NiImage].ref(r)
        if r.v >= 0x04000000: self.sourceTexture = X[NiSourceTexture].ref(r)
        self.enablePlane = r.readByte()
        self.plane = r.readS(NiPlane)
        if r.v <= 0x0A020000:
            self.pS2L = r.readInt16()
            self.pS2K = r.readInt16()
        if r.v <= 0x0401000C: self.unknownShort = r.readUInt16()

# LEGACY (pre-10.1)
class NiImage(NiObject): # Y
    useExternal: int                                    # 0 if the texture is internal to the NIF file.
    fileName: str                                       # The filepath to the texture.
    imageData: Ref[NiObject]                            # Link to the internally stored image data.
    unknownInt: int = 7                                 # Unknown.  Often seems to be 7. Perhaps m_uiMipLevels?
    unknownFloat: float = 128.5                         # Unknown.  Perhaps fImageScale?

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.useExternal = r.readByte()
        if self.useExternal != 0: self.fileName = r.readL32Encoding()
        else: self.imageData = X[NiRawImageData].ref(r)
        self.unknownInt = r.readUInt32()
        if r.v >= 0x03010000: self.unknownFloat = r.readSingle()

# Describes how a fragment shader should be configured for a given piece of geometry.
class NiTexturingProperty(NiProperty): # X
    flags: Flags                                        # Property flags.
    applyMode: ApplyMode = ApplyMode.APPLY_MODULATE     # Determines how the texture will be applied.  Seems to have special functions in Oblivion.
    textureCount: int = 7                               # Number of textures.
    baseTexture: TexDesc                                # The base texture.
    darkTexture: TexDesc                                # The dark texture.
    detailTexture: TexDesc                              # The detail texture.
    glossTexture: TexDesc                               # The gloss texture.
    glowTexture: TexDesc                                # The glowing texture.
    bumpMapTexture: TexDesc                             # The bump map texture.
    bumpMapLumaScale: float
    bumpMapLumaOffset: float
    bumpMapMatrix: Matrix2x2
    normalTexture: TexDesc                              # Normal texture.
    parallaxTexture: TexDesc
    parallaxOffset: float
    decal0Texture: TexDesc                              # The decal texture.
    decal1Texture: TexDesc                              # Another decal texture.
    decal2Texture: TexDesc                              # Another decal texture.
    decal3Texture: TexDesc                              # Another decal texture.
    shaderTextures: list[ShaderTexDesc]                 # Shader textures.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.v <= 0x0A000102: self.flags = Flags(r.readUInt16())
        if r.v >= 0x14010002: self.flags = Flags(r.readUInt16())
        if r.v >= 0x0303000D and r.v <= 0x14010001: self.applyMode = ApplyMode(r.readUInt32())
        self.textureCount = r.readUInt32()
        if r.readBool32(): self.baseTexture = TexDesc(r)
        if r.readBool32(): self.darkTexture = TexDesc(r)
        if r.readBool32(): self.detailTexture = TexDesc(r)
        if r.readBool32(): self.glossTexture = TexDesc(r)
        if r.readBool32(): self.glowTexture = TexDesc(r)
        hasBumpMapTexture = r.readBool32() if r.v >= 0x0303000D and self.textureCount > 5 else None
        if self.hasBumpMapTexture:
            self.bumpMapTexture = TexDesc(r)
            self.bumpMapLumaScale = r.readSingle()
            self.bumpMapLumaOffset = r.readSingle()
            self.bumpMapMatrix = r.readMatrix2x2()
        hasNormalTexture = r.readBool32() if r.v >= 0x14020005 and self.textureCount > 6 else None
        if self.hasNormalTexture: self.normalTexture = TexDesc(r)
        hasParallaxTexture = r.readBool32() if r.v >= 0x14020005 and self.textureCount > 7 else None
        if self.hasParallaxTexture:
            self.parallaxTexture = TexDesc(r)
            self.parallaxOffset = r.readSingle()
        hasDecal0Texture = r.readBool32() if r.v <= 0x14020004 and self.textureCount > 6 else None
        hasDecal0Texture = r.readBool32() if r.v >= 0x14020005 and self.textureCount > 8 else None
        if self.hasDecal0Texture: self.decal0Texture = TexDesc(r)
        hasDecal1Texture = r.readBool32() if r.v <= 0x14020004 and self.textureCount > 7 else None
        hasDecal1Texture = r.readBool32() if r.v >= 0x14020005 and self.textureCount > 9 else None
        if self.hasDecal1Texture: self.decal1Texture = TexDesc(r)
        hasDecal2Texture = r.readBool32() if r.v <= 0x14020004 and self.textureCount > 8 else None
        hasDecal2Texture = r.readBool32() if r.v >= 0x14020005 and self.textureCount > 10 else None
        if self.hasDecal2Texture: self.decal2Texture = TexDesc(r)
        hasDecal3Texture = r.readBool32() if r.v <= 0x14020004 and self.textureCount > 9 else None
        hasDecal3Texture = r.readBool32() if r.v >= 0x14020005 and self.textureCount > 11 else None
        if self.hasDecal3Texture: self.decal3Texture = TexDesc(r)
        if r.v >= 0x0A000100: self.shaderTextures = r.readL32FArray(lambda z: ShaderTexDesc(r))

# A shape node that refers to singular triangle data.
class NiTriShape(NiTriBasedGeom): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# Holds mesh data using a list of singular triangles.
class NiTriShapeData(NiTriBasedGeomData): # X
    numTrianglePoints: int                              # Num Triangles times 3.
    triangles: list[Triangle]                           # Triangle data.
    matchGroups: list[MatchGroup]                       # The shared normals.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.numTrianglePoints = r.readUInt32()
        hasTriangles = False if r.v >= 0x0A010000 else None # calculated
        if r.v <= 0x0A000102: self.triangles = r.readSArray<Triangle>(self.numTriangles)
        if r.v >= 0x0A000103 and self.hasTriangles: self.triangles = r.readSArray<Triangle>(self.numTriangles)
        if r.v >= 0x03010000: self.matchGroups = r.readL16FArray(lambda z: MatchGroup(r))

# DEPRECATED (pre-10.1), REMOVED (20.3).
# Time controller for texture coordinates.
class NiUVController(NiTimeController): # X
    unknownShort: int                                   # Always 0?
    data: Ref[NiObject]                                 # Texture coordinate controller data index.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.unknownShort = r.readUInt16()
        self.data = X[NiUVData].ref(r)

# DEPRECATED (pre-10.1), REMOVED (20.3)
# Texture coordinate data.
class NiUVData(NiObject): # X
    uvGroups: list[KeyGroup[T]]                         # Four UV data groups. Appear to be U translation, V translation, U scaling/tiling, V scaling/tiling.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.uvGroups = r.readFArray(lambda z: KeyGroup[T](r), 4)

# Property of vertex colors. This object is referred to by the root object of the NIF file whenever some NiTriShapeData object has vertex colors with non-default settings; if not present, vertex colors have vertex_mode=2 and lighting_mode=1.
class NiVertexColorProperty(NiProperty): # X
    flags: Flags                                        # Bits 0-2: Unknown
                                                        #     Bit 3: Lighting Mode
                                                        #     Bits 4-5: Vertex Mode
    vertexMode: VertMode                                # In Flags from 20.1.0.3 on.
    lightingMode: LightMode                             # In Flags from 20.1.0.3 on.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.flags = Flags(r.readUInt16())
        if r.v <= 0x14000005:
            self.vertexMode = VertMode(r.readUInt32())
            self.lightingMode = LightMode(r.readUInt32())

# DEPRECATED (10.x), REMOVED (?)
# Not used in skinning.
# Unsure of use - perhaps for morphing animation or gravity.
class NiVertWeightsExtraData(NiExtraData): # X
    numBytes: int                                       # Number of bytes in this data object.
    weight: list[float]                                 # The vertex weights.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.numBytes = r.readUInt32()
        self.weight = r.readL16PArray(None, 'f')

# DEPRECATED (10.2), REMOVED (?), Replaced by NiBoolData.
# Visibility data for a controller.
class NiVisData(NiObject): # X
    keys: list[Key[T]]

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.keys = r.readL32FArray(lambda z: Key[T](r, self.KeyType.LINEAR_KEY))

# Allows applications to switch between drawing solid geometry or wireframe outlines.
class NiWireframeProperty(NiProperty): # X
    flags: Flags                                        # Property flags.
                                                        #     0 - Wireframe Mode Disabled
                                                        #     1 - Wireframe Mode Enabled

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.flags = Flags(r.readUInt16())

# Allows applications to set the test and write modes of the renderer's Z-buffer and to set the comparison function used for the Z-buffer test.
class NiZBufferProperty(NiProperty): # X
    flags: Flags = 3                                    # Bit 0 enables the z test
                                                        #     Bit 1 controls wether the Z buffer is read only (0) or read/write (1)
    function: ZCompareMode = ZCompareMode.ZCOMP_LESS_EQUAL # Z-Test function (see: glDepthFunc). In Flags from 20.1.0.3 on.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.flags = Flags(r.readUInt16())
        if r.v >= 0x0401000C and r.v <= 0x14000005: self.function = ZCompareMode(r.readUInt32())

# Morrowind-specific node for collision mesh.
class RootCollisionNode(NiNode): # X
    def __init__(self, r: NiReader):
        super().__init__(r)

# LEGACY (pre-10.1)
# Raw image data.
class NiRawImageData(NiObject): # Y
    width: int                                          # Image width
    height: int                                         # Image height
    imageType: ImageType                                # The format of the raw image data.
    rgbImageData: list[list[Color3]]                    # Image pixel data.
    rgbaImageData: list[list[Color4]]                   # Image pixel data.

    def __init__(self, r: NiReader):
        super().__init__(r)
        self.width = r.readUInt32()
        self.height = r.readUInt32()
        self.imageType = ImageType(r.readUInt32())
        if self.imageType == ImageType.RGB: self.rgbImageData = r.readFArray(lambda k: r.readFArray(lambda z: Color3(r.readBytes(3)), Height), Width)
        if self.imageType == ImageType.RGBA: self.rgbaImageData = r.readFArray(lambda k: r.readFArray(lambda z: Color4(r.readBytes(4)), Height), Width)

# The type of animation interpolation (blending) that will be used on the associated key frames.
class BSShaderType(Enum): # Y
    SHADER_TALL_GRASS = 0           # Tall Grass Shader
    SHADER_DEFAULT = 1              # Standard Lighting Shader
    SHADER_SKY = 10                 # Sky Shader
    SHADER_SKIN = 14                # Skin Shader
    SHADER_WATER = 17               # Water Shader
    SHADER_LIGHTING30 = 29          # Lighting 3.0 Shader
    SHADER_TILE = 32                # Tiled Shader
    SHADER_NOLIGHTING = 33          # No Lighting Shader

# Shader Property Flags
class BSShaderFlags(Flag): # Y
    Specular = 0                    # Enables Specularity
    Skinned = 1 << 1                # Required For Skinned Meshes
    LowDetail = 1 << 2              # Lowddetail (seems to use standard diff/norm/spec shader)
    Vertex_Alpha = 1 << 3           # Vertex Alpha
    Unknown_1 = 1 << 4              # Unknown
    Single_Pass = 1 << 5            # Single Pass
    Empty = 1 << 6                  # Unknown
    Environment_Mapping = 1 << 7    # Environment mapping (uses Envmap Scale)
    Alpha_Texture = 1 << 8          # Alpha Texture Requires NiAlphaProperty to Enable
    Unknown_2 = 1 << 9              # Unknown
    FaceGen = 1 << 10               # FaceGen
    Parallax_Shader_Index_15 = 1 << 11 # Parallax
    Unknown_3 = 1 << 12             # Unknown/Crash
    Non_Projective_Shadows = 1 << 13# Non-Projective Shadows
    Unknown_4 = 1 << 14             # Unknown/Crash
    Refraction = 1 << 15            # Refraction (switches on refraction power)
    Fire_Refraction = 1 << 16       # Fire Refraction (switches on refraction power/period)
    Eye_Environment_Mapping = 1 << 17 # Eye Environment Mapping (does not use envmap light fade or envmap scale)
    Hair = 1 << 18                  # Hair
    Dynamic_Alpha = 1 << 19         # Dynamic Alpha
    Localmap_Hide_Secret = 1 << 20  # Localmap Hide Secret
    Window_Environment_Mapping = 1 << 21 # Window Environment Mapping
    Tree_Billboard = 1 << 22        # Tree Billboard
    Shadow_Frustum = 1 << 23        # Shadow Frustum
    Multiple_Textures = 1 << 24     # Multiple Textures (base diff/norm become null)
    Remappable_Textures = 1 << 25   # usually seen w/texture animation
    Decal_Single_Pass = 1 << 26     # Decal
    Dynamic_Decal_Single_Pass = 1 << 27 # Dynamic Decal
    Parallax_Occulsion = 1 << 28    # Parallax Occlusion
    External_Emittance = 1 << 29    # External Emittance
    Shadow_Map = 1 << 30            # Shadow Map
    ZBuffer_Test = 1 << 31          # ZBuffer Test (1=on)

# Shader Property Flags 2
class BSShaderFlags2(Flag): # Y
    ZBuffer_Write = 0               # ZBuffer Write
    LOD_Landscape = 1 << 1          # LOD Landscape
    LOD_Building = 1 << 2           # LOD Building
    No_Fade = 1 << 3                # No Fade
    Refraction_Tint = 1 << 4        # Refraction Tint
    Vertex_Colors = 1 << 5          # Has Vertex Colors
    Unknown1 = 1 << 6               # Unknown
    X1st_Light_is_Point_Light = 1 << 7 # 1st Light is Point Light
    X2nd_Light = 1 << 8             # 2nd Light
    X3rd_Light = 1 << 9             # 3rd Light
    Vertex_Lighting = 1 << 10       # Vertex Lighting
    Uniform_Scale = 1 << 11         # Uniform Scale
    Fit_Slope = 1 << 12             # Fit Slope
    Billboard_and_Envmap_Light_Fade = 1 << 13 # Billboard and Envmap Light Fade
    No_LOD_Land_Blend = 1 << 14     # No LOD Land Blend
    Envmap_Light_Fade = 1 << 15     # Envmap Light Fade
    Wireframe = 1 << 16             # Wireframe
    VATS_Selection = 1 << 17        # VATS Selection
    Show_in_Local_Map = 1 << 18     # Show in Local Map
    Premult_Alpha = 1 << 19         # Premult Alpha
    Skip_Normal_Maps = 1 << 20      # Skip Normal Maps
    Alpha_Decal = 1 << 21           # Alpha Decal
    No_Transparecny_Multisampling = 1 << 22 # No Transparency MultiSampling
    Unknown2 = 1 << 23              # Unknown
    Unknown3 = 1 << 24              # Unknown
    Unknown4 = 1 << 25              # Unknown
    Unknown5 = 1 << 26              # Unknown
    Unknown6 = 1 << 27              # Unknown
    Unknown7 = 1 << 28              # Unknown
    Unknown8 = 1 << 29              # Unknown
    Unknown9 = 1 << 30              # Unknown
    Unknown10 = 1 << 31             # Unknown

# Bethesda-specific property.
class BSShaderProperty(NiShadeProperty): # Y
    shaderType: BSShaderType = BSShaderType.SHADER_DEFAULT
    shaderFlags: BSShaderFlags = 0x82000000
    shaderFlags2: BSShaderFlags2 = 1
    environmentMapScale: float = 1.0                    # Scales the intensity of the environment/cube map.

    def __init__(self, r: NiReader):
        super().__init__(r)
        if r.uv2 <= 34:
            self.shaderType = BSShaderType(r.readUInt32())
            self.shaderFlags = BSShaderFlags(r.readUInt32())
            self.shaderFlags2 = BSShaderFlags2(r.readUInt32())
            self.environmentMapScale = r.readSingle()

#endregion

