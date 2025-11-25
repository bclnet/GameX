from __future__ import annotations
import os, itertools
from io import BytesIO
from enum import Enum
from numpy import ndarray, array
from openstk import _throw, Reader
from openstk.gfx import Raster, Texture_Bytes, ITexture, TextureFormat, TexturePixel
from openstk.core.drawing import Plane, Point, Rectangle, BoundingBox, BoundingSphere, BoundingFrustum, Ray, Curve
from openstk.core.reflect import *
from gamex import MetaInfo, MetaManager, MetaContent, IHaveMetaInfo
from gamex.core.globalx import ByteColor4
from gamex.core.formats.compression import decompressXbox

# types
type Vector2 = ndarray
type Vector3 = ndarray
type Vector4 = ndarray
type Matrix4x4 = ndarray
type Quaternion = ndarray

#region Type Manager

class TypeReader:
    def __init__(self, t: type, name: str, type: str, valueType: bool = False, init: callable = None):
        self.t: type = t
        self.name: str = name
        self.type: str = type
        self.valueType: bool = valueType
        self.canUseObj: bool = False
        self.init: callable = init
    def read(self, r: ContentReader, o: object) -> object: pass
class TypeReader[T](TypeReader):
    def __init__(self, name: str, type: str, read: callable, valueType: bool = False, init: callable = None):
        super().__init__(None, name, type, valueType, init)
        self.readFunc: callable = read
    def read(self, r: ContentReader, o: T) -> T: self.readFunc(r, o)
class GenericReader(TypeReader):
    def __init__(self, name: str, type: str, ginit: callable, valueType: bool = False, args: list[str] = None):
        super().__init__(name, type, None, valueType)
        self.ginit: callable = ginit

class TypeManager:
    def _MakeReader[T](t: type, type: str): return TypeReader[T](None, type, lambda r, o: t.new())
    def _EnumReader[T](elem): return TypeReader[T](None, None, lambda r, o: r.readInt32(), valueType=True)
    def _NullableReader[T](elem): return TypeReader[T](None, None, lambda r, o: elem.read(r, o) if r.readBoolean() else None, valueType=True)
    def _ArrayReader[T](elem): return TypeReader[list[T]](None, None, lambda r, o: r.readL32FArray(lambda z: r.read(elem), obj=o))
    def _ListReader[T](elem): return TypeReader[list[T]](None, None, lambda r, o: r.readL32FList(lambda z: r.read(elem), obj=o))
    def _DictionaryReader[TKey, TValue](key, value): return TypeReader[dict[TKey, TValue]](None, None, lambda r, o: r.readL32FMany(None, lambda z: r.read(key), lambda z: r.read(value), obj=o))
    def _ReflectiveReader[T](elem):
        type = Reflect.getTypeByName(elem.type)
        baseType = type.__bases__[0] if hasattr(type, '__bases__') else None
        # if baseType: print(baseType)
        constructor = Reflect.getDefaultConstructor(type)
        baseReader = getByType(baseType) if baseType else None
        properties, fields = Reflect.getAllPropertiesFields(type)
        values = []
        for p in properties:
            if (v := TypeManager.getMemberValue(p)): values.append(v)
        for f in fields:
            if (v := TypeManager.getMemberValue(f)): values.append(v)
        def _Reader(r, o):
            obj = o or constructor()
            if baseReader: r.read(baseReader, obj)
            for v in values: v(r, obj)
            return obj
        return TypeReader[T](None, None, _Reader)
    @staticmethod
    def getMemberValue(member: MemberInfo) -> callable:
        property, field = (member if isinstance(member, PropertyInfo) else None), (member if isinstance(member, FieldInfo) else None)
        if property and (not property.canRead or property.getIndexParameters()): return None
        # ignore
        if Attribute.getCustomAttribute(member, 'Ignore'): return None
        # optional
        optional = Attribute.getCustomAttribute(member, 'Optional')
        if not optional:
            if property:
                # if not property.GetGetMethod().IsPublic) return null;
                if not property.canWrite:
                    typeReader = TypeManager.getByType(property.propertyType)
                    raise Exception(f'CanWrite {typeReader}')
            elif not field.isPublic: return None
        # setter
        setter: callable; elementType: object
        if property: elementType = property.propertyType; setter = lambda o, v: property.setValue(o, v, None) if property.canWrite else lambda o, v: None
        else: elementType = field.fieldType; setter = field.setValue
        # resources get special treatment.
        if optional and optional['resource']: return lambda r, parent: setter(parent, r.readResource())
        # we need to have a reader at this point.
        reader = TypeManager.getByType(elementType) or TypeManager.getByType('Array') or _throw(f'Content reader could not be found for {elementType.__name__} type.')
        construct = lambda a: None
        return lambda r, parent: setter(parent, r.read(reader, construct(parent)))
    readers: list[TypeReader] = [
        # Primitive types
        TypeReader[int]('ByteReader', 'Byte', lambda r, o: r.readByte(), valueType=True),
        TypeReader[int]('SByteReader', 'SByte', lambda r, o: r.readSByte(), valueType=True),
        TypeReader[int]('Int16Reader', 'Int16', lambda r, o: r.readInt16(), valueType=True),
        TypeReader[int]('UInt16Reader', 'UInt16', lambda r, o: r.readUInt16(), valueType=True),
        TypeReader[int]('Int32Reader', 'Int32', lambda r, o: r.readInt32(), valueType=True),
        TypeReader[int]('UInt32Reader', 'UInt32', lambda r, o: r.readUInt32(), valueType=True),
        TypeReader[int]('Int64Reader', 'Int64', lambda r, o: r.readInt64(), valueType=True),
        TypeReader[int]('UInt64Reader', 'UInt64', lambda r, o: r.readUInt64(), valueType=True),
        TypeReader[float]('SingleReader', 'Single', lambda r, o: r.readSingle(), valueType=True),
        TypeReader[float]('DoubleReader', 'Double', lambda r, o: r.readDouble(), valueType=True),
        TypeReader[bool]('BooleanReader', 'Boolean', lambda r, o: r.readBoolean(), valueType=True),
        TypeReader[chr]('CharReader', 'Char', lambda r, o: r.readChar(), valueType=True),
        TypeReader[str]('StringReader', 'String', lambda r, o: r.readLV7UString()),
        TypeReader[object]('ObjectReader', 'Object', lambda r, o: _throw('NotSupportedException')),
        # System types
        GenericReader('EnumReader', 'Enum', _EnumReader),
        GenericReader('NullableReader', 'Nullable', _NullableReader),
        GenericReader('ArrayReader', 'Array', _ArrayReader),
        GenericReader('ListReader', 'Collections.Generic.List', _ListReader),
        GenericReader('DictionaryReader', 'Collections.Generic.Dictionary', _DictionaryReader),
        # TypeReader[object]('TimeSpanReader', 'TimeSpan', lambda r, o: { var v = r.readInt64(); return new TimeSpan(v); }, valueType=True),
        # TypeReader[object]('DateTimeReader', 'DateTime', lambda r, o: { var v = r.readInt64(); return new DateTime(v & ~(3L << 62), (DateTimeKind)(v >> 62)); }, valueType=True),
        TypeReader[int]('DecimalReader', 'Decimal', lambda r, o: r.readDecimal(), valueType=True), #{ uint a = r.readUInt32(), b = r.ReadUInt32(), c = r.ReadUInt32(), d = r.ReadUInt32(); return 0; }
        TypeReader[str]('ExternalReferenceReader', 'ExternalReference', lambda r, o: r.readString()),
        GenericReader('ReflectiveReader', 'Object', _ReflectiveReader),
        # Math types
        TypeReader[Vector2]('Vector2Reader', 'Vector2', lambda r, o: r.readVector2(), valueType=True),
        TypeReader[Vector3]('Vector3Reader', 'Vector3', lambda r, o: r.readVector3(), valueType=True),
        TypeReader[Vector4]('Vector4Reader', 'Vector4', lambda r, o: r.readVector4(), valueType=True),
        TypeReader[Matrix4x4]('MatrixReader', 'Matrix', lambda r, o: r.readMatrix4x4(), valueType=True),
        TypeReader[Quaternion]('QuaternionReader', 'Quaternion', lambda r, o: r.readQuaternion(), valueType=True),
        TypeReader[ByteColor4]('ColorReader', 'Color', lambda r, o: ByteColor4(r.readByte(), r.readByte(), r.readByte(), r.readByte()), valueType=True),
        TypeReader[Plane]('PlaneReader', 'Plane', lambda r, o: Plane(r.readVector3(), r.readSingle()), valueType=True),
        TypeReader[Point]('PointReader', 'Point', lambda r, o: Point(r.readInt32(), r.readInt32()), valueType=True),
        TypeReader[Rectangle]('RectangleReader', 'Rectangle', lambda r, o: Rectangle(r.readInt32(), r.readInt32(), r.readInt32(), r.readInt32()), valueType=True),
        TypeReader[BoundingBox]('BoundingBoxReader', 'BoundingBox', lambda r, o: BoundingBox(r.readVector3(), r.readVector3()), valueType=True),
        TypeReader[BoundingSphere]('BoundingSphereReader', 'BoundingSphere', lambda r, o: BoundingSphere(r.readVector3(), r.readSingle()), valueType=True),
        TypeReader[BoundingFrustum]('BoundingFrustumReader', 'BoundingFrustum', lambda r, o: BoundingFrustum(r.readMatrix4x4())),
        TypeReader[Ray]('RayReader', 'Ray', lambda r, o: Ray(r.readVector3(), r.readVector3()), valueType=True),
        TypeReader[Curve]('CurveReader', 'Curve', lambda r, o: Curve(r.readInt32(), r.readInt32(), r.readL32FArray(lambda z: Curve.Loop(z.readSingle(), z.readSingle(), z.readSingle(), z.readSingle(), z.readInt32())))),
        # Graphics types
        TypeReader[object]('TextureReader', 'Graphics.Texture', lambda r, o: _throw('NotSupportedException')),
        TypeReader['Texture2D']('Texture2DReader', 'Graphics.Texture2D', lambda r, o: Texture2D(r)),
        TypeReader['Texture3D']('Texture3DReader', 'Graphics.Texture3D', lambda r, o: Texture3D(r)),
        TypeReader['TextureCube']('TextureCubeReader', 'Graphics.TextureCube', lambda r, o: TextureCube(r)),
        TypeReader['IndexBuffer']('IndexBufferReader', 'Graphics.IndexBuffer', lambda r, o: IndexBuffer(r)),
        TypeReader['VertexBuffer']('VertexBufferReader', 'Graphics.VertexBuffer', lambda r, o: VertexBuffer(r)),
        TypeReader['VertexDeclaration']('VertexDeclarationReader', 'Graphics.VertexDeclaration', lambda r, o: VertexDeclaration(r)),
        TypeReader['Effect']('EffectReader', 'Graphics.Effect', lambda r, o: Effect(r)),
        TypeReader['EffectMaterial']('EffectMaterialReader', 'Graphics.EffectMaterial', lambda r, o: EffectMaterial(r)),
        TypeReader['BasicEffect']('BasicEffectReader', 'Graphics.BasicEffect', lambda r, o: BasicEffect(r)),
        TypeReader['AlphaTestEffect']('AlphaTestEffectReader', 'Graphics.AlphaTestEffect', lambda r, o: AlphaTestEffect(r)),
        TypeReader['DualTextureEffect']('DualTextureEffectReader', 'Graphics.DualTextureEffect', lambda r, o: DualTextureEffect(r)),
        TypeReader['EnvironmentMapEffect']('EnvironmentMapEffectReader', 'Graphics.EnvironmentMapEffect', lambda r, o: EnvironmentMapEffect(r)),
        TypeReader['SkinnedEffect']('SkinnedEffectReader', 'Graphics.SkinnedEffect', lambda r, o: SkinnedEffect(r)),
        TypeReader['SpriteFont']('SpriteFontReader', 'Graphics.SpriteFont', lambda r, o: SpriteFont(r)),
        TypeReader['Model']('ModelReader', 'Graphics.Model', lambda r, o: Model(r)),
        # Media types
        TypeReader['SoundEffect']('SoundEffectReader', 'Audio.SoundEffect', lambda r, o: SoundEffect(r)),
        TypeReader['Song']('SongReader', 'Media.Song', lambda r, o: Song(r)),
        TypeReader['Video']('VideoReader', 'Media.Video', lambda r, o: Video(r))]
    readersByName: dict[str, TypeReader] = { x.name:x for x in readers }
    readersByType: dict[str, TypeReader] = {}
    for k, g in itertools.groupby(sorted(readers, key=lambda x: x.type), key=lambda x: x.type): v = next(g); readersByType[v.type] = v
    @staticmethod
    def add(reader: TypeReader) -> TypeReader:
        TypeManager.readers.append(reader)
        if reader.name and reader.type not in TypeManager.readersByName: TypeManager.readersByName[reader.name] = reader
        if reader.type and reader.type not in TypeManager.readersByType: TypeManager.readersByType[reader.type] = reader
        return reader
    @staticmethod
    def create(s: GenericReader, args: list[str]) -> TypeReader:
        r: TypeReader = None
        if len(args) == 1:
            value = TypeManager.getByType(args[0])
            r = s.ginit(value)
        elif len(args) == 2:
            key = TypeManager.getByType(args[0])
            value = TypeManager.getByType(args[1])
            r = s.ginit(key, value)
        else: raise Exception()
        suffix = f'`{len(args)}[[{'],['.join(args)}]]'
        r.name = s.name + suffix
        r.type = s.type + suffix
        r.valueType = s.valueType
        return r
    @staticmethod
    def getByName(name: str, version: int) -> TypeReader:
        name = Reflect.stripAssemblyVersion(name).replace('Microsoft.Xna.Framework.Content.', '')
        if name in TypeManager.readersByName: return TypeManager.readersByName[name]
        genericName, args = Reflect.splitGenericTypeName(name)
        if genericName and genericName in TypeManager.readersByName and (generic := TypeManager.readersByName[genericName]) != None:
            reader = TypeManager.create(generic, args)
            if reader.name != name: raise Exception('ERROR')
            return TypeManager.add(reader)
        _throw(f'Cannot find type reader "{name}".')
    @staticmethod
    def getByType(type: str) -> TypeReader:
        type = Reflect.stripAssemblyVersion(type).replace('Microsoft.Xna.Framework.', '').replace('System.', '')
        if type in TypeManager.readersByType: return TypeManager.readersByType[type]
        genericName, args = Reflect.splitGenericTypeName(type)
        if genericName and genericName in TypeManager.readersByType and (generic := TypeManager.readersByType[genericName]) != None:
            reader = TypeManager.create(generic, args)
            if reader.type != type: raise Exception('ERROR')
            return TypeManager.add(reader)
        t = Reflect.getTypeByName(type)
        if t:
            reader = TypeManager._MakeReader(t, type)
            return TypeManager.add(reader)
        _throw(f'Cannot find type reader "{t}".')

# ContentReader
class ContentReader(Reader):
    def __init__(self, f):
        super().__init__(f)
        self.typeReaders: list[TypeReader] = None
    def readAsset[T](self, obj: object = None) -> T: self.readTypeReaders(); s = self.readObject(obj); self.readSharedResources(); return s
    def readTypeReaders(self) -> None:
        self.typeReaders: list[TypeReader] = self.readLV7FArray(lambda z: TypeManager.getByName(self.readLV7UString(), self.readUInt32()))
        self.sharedResourceCount = self.readVInt7(); self.sharedResourceFixups = []
        for s in [x for x in self.typeReaders if x.init]: s.init(s)
    def readSharedResources(self) -> None:
        if self.sharedResourceCount <= 0: return
        self.sharedResources = r.readFArray(lambda z: r.readObject[object](), self.sharedResourceCount)
        # for fixup in self.sharedResourceFixups: fixup.value(self.sharedResourceFixups[fixup.key])
    def read[T](self, reader: TypeReader, obj: T = None) -> T: return reader.read(self, obj) if reader.valueType else self.readObject(obj)
    def readObject[T](self, obj: T = None) -> T: reader = self.readReader(); return reader.read(self, obj) if reader else None
    def readReader(self) -> TypeReader: idx = self.readVInt7() - 1; return (self.typeReaders[idx] if idx < len(self.typeReaders) else _throw('Invalid XNB file: idx is out of range.')) if idx >= 0 else None 
    def validate(self, type: str) -> ContentReader: reader = self.readReader(); return self if reader and reader.type == type else _throw('Invalid XNB file: got an unexpected reader.')
    def readSharedResource(self, fixup: callable) -> None:
        idx = self.readVInt7() - 1
        if idx >= 0: self.sharedResourceFixups.append((idx, lambda v: fixup(v)))
#endregion

#region Gfx Objects

class SurfaceFormat(Enum):
    Color = 0
    Bgr565 = 1
    Bgra5551 = 2
    Bgra4444 = 3
    Dxt1 = 4
    Dxt3 = 5
    Dxt5 = 6
    NormalizedByte2 = 7
    NormalizedByte4 = 8
    Rgba1010102 = 9
    Rg32 = 10
    Rgba64 = 11
    Alpha8 = 12
    Single = 13
    Vector2 = 14
    Vector4 = 15
    HalfSingle = 16
    HalfVector2 = 17
    HalfVector4 = 18
    HdrBlendable = 19

class VertexElementFormat(Enum):
    Single = 0
    Vector2 = 1
    Vector3 = 2
    Vector4 = 3
    Color = 4
    Byte4 = 5
    Short2 = 6
    Short4 = 7
    NormalizedShort2 = 8
    NormalizedShort4 = 9
    HalfVector2 = 10
    HalfVector4 = 11

class VertexElementUsage(Enum):
    Position = 0
    Color = 1
    TextureCoordinate = 2
    Normal = 3
    Binormal = 4
    Tangent = 5
    BlendIndices = 6
    BlendWeight = 7
    Depth = 8
    Fog = 9
    PointSize = 10
    Sample = 11
    TessellateFactor = 12

class CompareFunction(Enum):
    Always = 0
    Never = 1
    Less = 2
    LessEqual = 3
    Equal = 4
    GreaterEqual = 5
    Greater = 6
    NotEqual = 7

class Texture2D(IHaveMetaInfo, ITexture):
    def __init__(self, r: ContentReader):
        self.format: SurfaceFormat = SurfaceFormat(r.readInt32())
        self.width: int = r.readUInt32()
        self.height: int = r.readUInt32()
        self.mips: list[bytes] = r.readL32FArray(lambda z: r.readL32Bytes())
    #region ITexture
    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0
    def create(self, platform: str, func: callable):
        buf = self.mips[0]
        if len(self.mips) > 1: raise Exception('Not Supported')
        match self.format:
            case SurfaceFormat.Color: format = (TextureFormat.RGBA32, TexturePixel.Unknown)
            case SurfaceFormat.Bgr565: format = (TextureFormat.RGB565, TexturePixel.Unknown)
            case SurfaceFormat.Bgra5551: format = (TextureFormat.BGRA1555, TexturePixel.Unknown)
            # case SurfaceFormat.Bgra4444: format = (TextureFormat.X, TexturePixel.Unknown)
            case SurfaceFormat.Dxt1: format = (TextureFormat.DXT1, TexturePixel.Unknown)
            case SurfaceFormat.Dxt3: format = (TextureFormat.DXT3, TexturePixel.Unknown)
            case SurfaceFormat.Dxt5: format = (TextureFormat.DXT5, TexturePixel.Unknown)
            # case SurfaceFormat.NormalizedByte2: format = (TextureFormat.X, TexturePixel.Unknown)
            # case SurfaceFormat.NormalizedByte4: format = (TextureFormat.X, TexturePixel.Unknown)
            # case SurfaceFormat.Rgba1010102: format = (TextureFormat.X, TexturePixel.Unknown)
            # case SurfaceFormat.Rg32: format = (TextureFormat.X, TexturePixel.Unknown)
            # case SurfaceFormat.Rgba64: format = (TextureFormat.X, TexturePixel.Unknown)
            # case SurfaceFormat.Alpha8: format = (TextureFormat.X, TexturePixel.Unknown)
            # case SurfaceFormat.Single: format = (TextureFormat.X, TexturePixel.Unknown)
            # case SurfaceFormat.Vector2: format = (TextureFormat.X, TexturePixel.Unknown)
            # case SurfaceFormat.Vector4: format = (TextureFormat.X, TexturePixel.Unknown)
            # case SurfaceFormat.HalfSingle: format = (TextureFormat.X, TexturePixel.Unknown)
            # case SurfaceFormat.HalfVector2: format = (TextureFormat.X, TexturePixel.Unknown)
            # case SurfaceFormat.HalfVector4: format = (TextureFormat.X, TexturePixel.Unknown)
            # case SurfaceFormat.HdrBlendable: format = (TextureFormat.X, TexturePixel.Unknown)
            case _: raise Exception('Unknown Format: {Format}')
        return func(Texture_Bytes(buf, format, None))
    #endregion
    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Texture2D', items = [
            MetaInfo(f'Format: {self.format}'),
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}'),
            MetaInfo(f'Mips: {len(self.mips)}')
            ])
        ]

class Texture3D(IHaveMetaInfo):
    def __init__(self, r: ContentReader):
        self.format: SurfaceFormat = SurfaceFormat(r.readInt32())
        self.width: int = r.readUInt32()
        self.height: int = r.readUInt32()
        self.depth: int = r.readUInt32()
        self.mips: list[bytes] = r.ReadL32FArray(lambda z: r.readL32Bytes())
    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Data', name = os.path.basename(file.path), value = self)),
        MetaInfo('Texture3D', items = [
            MetaInfo(f'Format: {self.format}'),
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}'),
            MetaInfo(f'Depth: {self.depth}'),
            MetaInfo(f'Mips: {len(self.mips)}')
            ])
        ]

class TextureCube(IHaveMetaInfo):
    def __init__(self, r: ContentReader):
        self.format: SurfaceFormat = SurfaceFormat(r.readInt32())
        self.size: int = r.readUInt32()
        self.face1Mips: list[bytes] = r.readL32FArray(lambda z: r.readL32Bytes())
        self.face2Mips: list[bytes] = r.readL32FArray(lambda z: r.readL32Bytes())
        self.face3Mips: list[bytes] = r.readL32FArray(lambda z: r.readL32Bytes())
        self.face4Mips: list[bytes] = r.readL32FArray(lambda z: r.readL32Bytes())
        self.face5Mips: list[bytes] = r.readL32FArray(lambda z: r.readL32Bytes())
        self.face6Mips: list[bytes] = r.readL32FArray(lambda z: r.readL32Bytes())
    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Data', name = os.path.basename(file.path), value = self)),
        MetaInfo('TextureCube', items = [
            MetaInfo(f'Format: {self.format}'),
            MetaInfo(f'Size: {self.size}')
            ])
        ]

class IndexBuffer:
    def __init__(self, r: ContentReader):
        self.indexFormat: int = 16 if r.readBoolean() else 32
        self.indexData: bytes = r.readL32Bytes()

class VertexDeclaration:
    class Element:
        def __init__(self, r: ContentReader):
            self.offset: int = r.readUInt32()
            self.format: VertexElementFormat = VertexElementFormat(r.readInt32())
            self.usage: VertexElementUsage = VertexElementUsage(r.readInt32())
            self.usageIndex: int = r.readUInt32()
    def __init__(self, r: ContentReader):
        self.vertexStride: int = r.readUInt32()
        self.elements: list[object] = r.readL32FArray(lambda z: VertexDeclaration.Element(r))

class VertexBuffer(VertexDeclaration):
    def __init__(self, r: ContentReader):
        super().__init__(r)
        self.vertexs = r.readL32FArray(lambda z: r.readBytes(self.vertexStride))

class Effect:
    def __init__(self, r: ContentReader):
        self.effectBytecode: str = r.readL32Bytes()

class EffectMaterial:
    def __init__(self, r: ContentReader):
        self.effectReference: str = r.readLV7UString()
        self.parameters: dict[str, object] = r.readObject()

class BasicEffect:
    def __init__(self, r: ContentReader):
        self.textureReference: str = r.readLV7UString()
        self.diffuseColor: Vector3 = r.readVector3()
        self.emissiveColor: Vector3 = r.readVector3()
        self.specularColor: Vector3 = r.readVector3()
        self.specularPower: float = r.readSingle()
        self.alpha: float = r.readSingle()
        self.vertexColorEnabled: bool = r.readBoolean()

class AlphaTestEffect:
    def __init__(self, r: ContentReader):
        self.textureReference: str = r.readLV7UString()
        self.alphaFunction: CompareFunction = CompareFunction(r.readInt32())
        self.referenceAlpha: int = r.readUInt32()
        self.diffuseColor: Vector3 = r.readVector3()
        self.alpha: float = r.readSingle()
        self.vertexColorEnabled: bool = r.readBoolean()

class DualTextureEffect:
    def __init__(self, r: ContentReader):
        self.texture1Reference: str = r.readLV7UString()
        self.texture2Reference: str = r.readLV7UString()
        self.diffuseColor: Vector3 = r.readVector3()
        self.alpha: float = r.readSingle()
        self.vertexColorEnabled: bool = r.readBoolean()

class EnvironmentMapEffect:
    def __init__(self, r: ContentReader):
        self.textureReference: str = r.readLV7UString()
        self.environmentMapReference: str = r.readLV7UString()
        self.environmentMapAmount: float = r.readSingle()
        self.environmentMapSpecular: Vector3 = r.readVector3()
        self.fresnelFactor: float = r.readSingle()
        self.diffuseColor: Vector3 = r.readVector3()
        self.emissiveColor: Vector3 = r.readVector3()
        self.alpha: float = r.readSingle()

class SkinnedEffect:
    def __init__(self, r: ContentReader):
        self.textureReference: str = r.readLV7UString()
        self.weightsPerVertex: int = r.readUInt32()
        self.diffuseColor: Vector3 = r.readVector3()
        self.emissiveColor: Vector3 = r.readVector3()
        self.specularColor: Vector3 = r.readVector3()
        self.specularPower: float = r.readSingle()
        self.alpha: float = r.readSingle()

class SpriteFont(IHaveMetaInfo):
    def __init__(self, r: ContentReader):
        self.texture: Texture2D = r.readObject()
        self.glyphs: list[Rectangle] = r.readObject()
        self.cropping: list[Rectangle] = r.readObject()
        self.characters: str = r.readObject()
        self.verticalLinespacing: int = r.readInt32()
        self.horizontalSpacing: float = r.readSingle()
        self.kerning: list[Vector3] = r.readObject()
        self.defaultCharacter: chr = r.readChar() if r.readBoolean() else '\x00'
    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self.texture)),
        MetaInfo('SpriteFont', items = [
            MetaInfo(f'Format: {self.texture.format}'),
            MetaInfo(f'Width: {self.texture.width}'),
            MetaInfo(f'Height: {self.texture.height}')
            ])
        ]

    def measureString(text: str) -> Vector2:
        if not text: raise Exception('text')
        if len(text) == 0: return array([0.,0.,0.])
        res = array([0.,0.,0.])
        curLineWidth = 0.; finalLineHeight = self.verticalLineSpacing
        firstInLine = True
        for c in text:
            # special characters
            if c == '\r': continue
            if c == '\n':
                res.x = max(result.x, curLineWidth)
                res.y += self.verticalLineSpacing
                curLineWidth = 0.
                finalLineHeight = self.verticalLineSpacing
                firstInLine = True
                continue

            # get the List index from the character map, defaulting to the DefaultCharacter if it's set.
            index = self.characters.find(c)
            if index == -1: index = self.characters.find(self.defaultCharacter or '?')

            # for the first character in a line, always push the width rightward, even if the kerning pushes the character to the left.
            kern = self.kerning[index]
            if firstInLine: curLineWidth += abs(kern.x); firstInLine = False
            else: curLineWidth += HorizontalSpacing + kern.x

            # add the character width and right-side bearing to the line width.
            curLineWidth += kern.y + kern.z

            # if a character is taller than the default line height, increase the height to that of the line's tallest character.
            cropHeight = Cropping[index].height
            if cropHeight > finalLineHeight: finalLineHeight = cropHeight

        # calculate the final width/height of the text box
        res.x = max(res.x, curLineWidth)
        res.y += finalLineHeight
        return res

class Model(IHaveMetaInfo):
    @staticmethod
    def readBoneIdx(p: Model, r: ContentReader): id = r.readByte() if p.bones8 else r.readUInt32(); return p.bones[id - 1] if id != 0 else None
    class Bone:
        def __init__(self, r: ContentReader):
            self.name: str = r.readObject[string]()
            self.transform: Matrix4x4 = r.readMatrix4x4()
            self.parent: Bone = None
            self.children: list[Bone] = None
    class MeshPart:
        def __init__(self, p: Model, r: ContentReader):
            self.vertexOffset: int = r.readInt32()
            self.numVertices: int = r.readInt32()
            self.startIndex: int = r.readInt32()
            self.primitiveCount: int = r.readInt32()
            self.tag: object = r.readObject[object]()
            self.resourceRef = r.readResource()
            self.indexBuffer: ResourceRef = r.readResource()
            self.effect: ResourceRef = r.readResource()
    class Mesh:
        def __init__(self, p: Model, r: ContentReader):
            self.name: str = r.readObject[str]()
            self.parent: Bone = readBoneIdx(p, r)
            self.bounds: BoundingSphere = BoundingSphere(r.readVector3(), r.readSingle())
            self.tag: object = r.readObject[object]()
            self.parts: list[MeshPart] = r.readL32FArray(lambda z: MeshPart(r))
    def __init__(self, r: ContentReader):
        self.bones: list[Bone] = r.readL32FArray(lambda z: Bone(r)); self.bones8: bool = len(self.bones) < 255
        for s in self.bones:
            s.parent = readBoneIdx(p, r)
            s.children = r.readL32FArray(lambda z: readBoneIdx(p, r))
        self.meshs: list[Mesh] = r.readL32FArray(lambda z: Mesh(self, r))
        self.root: Bone = readBoneIdx(p, r)
        self.tag: object = r.readObject[object]()
    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Data', name = os.path.basename(file.path), value = self)),
        MetaInfo('Model', items = [
            MetaInfo(f'Bones: {len(self.bones)}'),
            MetaInfo(f'Meshs: {len(self.meshs)}'),
            MetaInfo(f'Root: {self.root.name}'),
            MetaInfo(f'Tag: {self.tag}')
            ])
        ]

#endregion

#region Media Objects

class SoundtrackType(Enum):
    Music = 0
    Dialog = 1
    MusicDialog = 2

class SoundEffect(IHaveMetaInfo):
    def __init__(self, r: ContentReader):
        self.format: bytes = r.readL32Bytes()
        self.data: bytes = r.readL32Bytes()
        self.loopStart: int = r.readInt32()
        self.loopLength: int = r.readInt32()
        self.duration: int = r.readInt32()
    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Data', name = os.path.basename(file.path), value = self)),
        MetaInfo('SoundEffect', items = [
            MetaInfo(f'Duration: {self.duration}')
            ])
        ]

class Song(IHaveMetaInfo):
    def __init__(self, r: ContentReader):
        self.filename: str = r.readLV7UString()
        self.duration: int = r.validate('Int32').readInt32()
    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Data', name = os.path.basename(file.path), value = self)),
        MetaInfo('Song', items = [
            MetaInfo(f'Filename: {self.filename}'),
            MetaInfo(f'Duration: {self.duration}')
            ])
        ]

class Video(IHaveMetaInfo):
    def __init__(self, r: ContentReader):
        self.filename: str = r.validate('String').readLV7UString()
        self.duration: int = r.validate('Int32').readInt32()
        self.width: int = r.validate('Int32').readInt32()
        self.height: int = r.validate('Int32').readInt32()
        self.framesPerSecond: float = r.validate('Single').readSingle()
        self.soundtrackType: SoundtrackType = SoundtrackType(r.validate('Int32').readInt32())
    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Data', name = os.path.basename(file.path), value = self)),
        MetaInfo('Video', items = [
            MetaInfo(f'Filename: {self.filename}'),
            MetaInfo(f'Duration: {self.duration}'),
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}')
            ])
        ]

#endregion
