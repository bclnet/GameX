from __future__ import annotations
import os
from io import BytesIO
from enum import Enum
from numpy import ndarray, array
from openstk import _throw, _pathExtension, Reader, IWriteToStream
from openstk.gfx import Raster, Texture_Bytes, ITexture, TextureFormat, TexturePixel
from openstk.core.drawing import Plane, Point, Rectangle, BoundingBox, BoundingSphere, Ray, Curve
from gamex import PakFile, BinaryPakFile, PakBinary, PakBinaryT, FileSource, MetaInfo, MetaManager, MetaContent, IHaveMetaInfo, DesSer
from gamex.core.globalx import ByteColor4
from gamex.core.formats.compression import decompressXbox

# types
type Vector2 = ndarray
type Vector3 = ndarray
type Vector4 = ndarray
type Matrix4x4 = ndarray
type Quaternion = ndarray

#region Binary_Xnb

# Binary_Xnb
class Binary_Xnb(IHaveMetaInfo, IWriteToStream):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Xnb(r)

    #region Type Reader

    class TypeReader:
        def __init__(self, type: type, name: str, target: str, valueType: bool = False):
            self.type: type = type
            self.name: str = name
            self.target: str = target
            self.valueType: bool = valueType

    class TypeReader[T](TypeReader):
        def __init__(self, name: str, target: str, read: callable, valueType: bool = False):
            super().__init__(None, name, target, valueType)
            self.read: callable = read

    class GenericReader(TypeReader):
        def __init__(self, name: str, target: str, ginit: callable, valueType: bool = False, args: list[str] = None):
            super().__init__(name, target, None, valueType)
            self.ginit: callable = ginit

    class ResourceRef:
        def __init__(self, r: ContentReader): self.id = r.readVInt7()
        def obj(p: Binary_Xnb) -> object: return p.resources[self.id - 1] if self.id != 0 else None

    # System types
    def _EnumReader[T](value): return Binary_Xnb.TypeReader[T](None, None, lambda r: r.readInt32())
    def _NullableReader[T](value): return Binary_Xnb.TypeReader[T](None, None, lambda r: r.read(value) if r.readBoolean() else None)
    def _ArrayReader[T](value): return Binary_Xnb.TypeReader[list[T]](None, None, lambda r: r.readL32FArray(lambda z: r.readValueOrObject(value)))
    def _ListReader[T](value): return Binary_Xnb.TypeReader[list[T]](None, None, lambda r: r.readL32FList(lambda z: r.readValueOrObject(value)))
    def _DictionaryReader[TKey, TValue](key, value): return Binary_Xnb.TypeReader[dict[TKey, TValue]](None, None, lambda r: r.readL32FMany(None, lambda z: r.readValueOrObject(key), lambda z: r.readValueOrObject(value)))
    def _ReflectiveReader[T](value): return Binary_Xnb.TypeReader[T](None, None, lambda r: _throw('NotSupportedException'))
    typeReaders: list[TypeReader]  = [
        # Primitive types
        TypeReader[int]('ByteReader', 'System.Byte', lambda r: r.readByte(), valueType=True),
        TypeReader[int]('SByteReader', 'System.SByte', lambda r: r.readSByte(), valueType=True),
        TypeReader[int]('Int16Reader', 'System.Int16', lambda r: r.readInt16(), valueType=True),
        TypeReader[int]('UInt16Reader', 'System.UInt16', lambda r: r.readUInt16(), valueType=True),
        TypeReader[int]('Int32Reader', 'System.Int32', lambda r: r.readInt32(), valueType=True),
        TypeReader[int]('UInt32Reader', 'System.UInt32', lambda r: r.readUInt32(), valueType=True),
        TypeReader[int]('Int64Reader', 'System.Int64', lambda r: r.readInt64(), valueType=True),
        TypeReader[int]('UInt64Reader', 'System.UInt64', lambda r: r.readUInt64(), valueType=True),
        TypeReader[float]('SingleReader', 'System.Single', lambda r: r.readSingle(), valueType=True),
        TypeReader[float]('DoubleReader', 'System.Double', lambda r: r.readDouble(), valueType=True),
        TypeReader[bool]('BooleanReader', 'System.Boolean', lambda r: r.readBoolean(), valueType=True),
        TypeReader[chr]('CharReader', 'System.Char', lambda r: r.readChar(), valueType=True),
        TypeReader[str]('StringReader', 'System.String', lambda r: r.readLV7UString()),
        TypeReader[object]('ObjectReader', 'System.Object', lambda r: _throw('NotSupportedException')),

        # System types
        GenericReader('EnumReader', 'System.Enum', _EnumReader),
        GenericReader('NullableReader', 'System.Nullable', _NullableReader, valueType=True),
        GenericReader('ArrayReader', 'System.Array', _ArrayReader),
        GenericReader('ListReader', 'System.Collections.Generic.List', _ListReader),
        GenericReader('DictionaryReader', 'System.Collections.Generic.Dictionary', _DictionaryReader),
        # TypeReader[object]('TimeSpanReader', 'System.TimeSpan', lambda r: { var v = r.readInt64(); return new TimeSpan(v); }, valueType=True),
        # TypeReader[object]('DateTimeReader', 'System.DateTime', lambda r: { var v = r.readInt64(); return new DateTime(v & ~(3L << 62), (DateTimeKind)(v >> 62)); }, valueType=True),
        # TypeReader[object]('DecimalReader', 'System.Decimal', lambda r: { uint a = r.readUInt32(), b = r.ReadUInt32(), c = r.ReadUInt32(), d = r.ReadUInt32(); return 0; }, valueType=True),
        TypeReader[str]('ExternalReferenceReader', 'ExternalReference', lambda r: r.readString()),
        GenericReader('ReflectiveReader', 'System.Object', _ReflectiveReader),

        # Math types
        TypeReader[Vector2]('Vector2Reader', 'Vector2', lambda r: r.readVector2(), valueType=True),
        TypeReader[Vector3]('Vector3Reader', 'Vector3', lambda r: r.readVector3(), valueType=True),
        TypeReader[Vector4]('Vector4Reader', 'Vector4', lambda r: r.readVector4(), valueType=True),
        TypeReader[Matrix4x4]('MatrixReader', 'Matrix', lambda r: r.readMatrix4x4(), valueType=True),
        TypeReader[Quaternion]('QuaternionReader', 'Quaternion', lambda r: r.readQuaternion(), valueType=True),
        TypeReader[ByteColor4]('ColorReader', 'Color', lambda r: ByteColor4(r.readByte(), r.readByte(), r.readByte(), r.readByte()), valueType=True),
        TypeReader[Plane]('PlaneReader', 'Plane', lambda r: Plane(r.readVector3(), r.readSingle()), valueType=True),
        TypeReader[Point]('PointReader', 'Point', lambda r: Point(r.readInt32(), r.readInt32()), valueType=True),
        TypeReader[Rectangle]('RectangleReader', 'Rectangle', lambda r: Rectangle(r.readInt32(), r.readInt32(), r.readInt32(), r.readInt32()), valueType=True),
        TypeReader[BoundingBox]('BoundingBoxReader', 'BoundingBox', lambda r: BoundingBox(r.readVector3(), r.readVector3()), valueType=True),
        TypeReader[BoundingSphere]('BoundingSphereReader', 'BoundingSphere', lambda r: BoundingSphere(r.readVector3(), r.readSingle()), valueType=True),
        TypeReader[Matrix4x4]('BoundingFrustumReader', 'BoundingFrustum', lambda r: r.readMatrix4x4()),
        TypeReader[Ray]('RayReader', 'Ray', lambda r: Ray(r.readVector3(), r.readVector3()), valueType=True),
        TypeReader[Curve]('CurveReader', 'Curve', lambda r: Curve(r.readInt32(), r.readInt32(), r.readL32FArray(lambda z: Curve.Loop(z.readSingle(), z.readSingle(), z.readSingle(), z.readSingle(), z.readInt32())))),

        # Graphics types
        TypeReader[object]('TextureReader', 'Graphics.Texture', lambda r: _throw('NotSupportedException')),
        TypeReader['Binary_Xnb.Texture2D']('Texture2DReader', 'Graphics.Texture2D', lambda r: Binary_Xnb.Texture2D(r)),
        TypeReader['Binary_Xnb.Texture3D']('Texture3DReader', 'Graphics.Texture3D', lambda r: Binary_Xnb.Texture3D(r)),
        TypeReader['Binary_Xnb.TextureCube']('TextureCubeReader', 'Graphics.TextureCube', lambda r: Binary_Xnb.TextureCube(r)),
        TypeReader['Binary_Xnb.IndexBuffer']('IndexBufferReader', 'Graphics.IndexBuffer', lambda r: Binary_Xnb.IndexBuffer(r)),
        TypeReader['Binary_Xnb.VertexBuffer']('VertexBufferReader', 'Graphics.VertexBuffer', lambda r: Binary_Xnb.VertexBuffer(r)),
        TypeReader['Binary_Xnb.VertexDeclaration']('VertexDeclarationReader', 'Graphics.VertexDeclaration', lambda r: Binary_Xnb.VertexDeclaration(r)),
        TypeReader['Binary_Xnb.Effect']('EffectReader', 'Graphics.Effect', lambda r: Binary_Xnb.Effect(r)),
        TypeReader['Binary_Xnb.EffectMaterial']('EffectMaterialReader', 'Graphics.EffectMaterial', lambda r: Binary_Xnb.EffectMaterial(r)),
        TypeReader['Binary_Xnb.BasicEffect']('BasicEffectReader', 'Graphics.BasicEffect', lambda r: Binary_Xnb.BasicEffect(r)),
        TypeReader['Binary_Xnb.AlphaTestEffect']('AlphaTestEffectReader', 'Graphics.AlphaTestEffect', lambda r: Binary_Xnb.AlphaTestEffect(r)),
        TypeReader['Binary_Xnb.DualTextureEffect']('DualTextureEffectReader', 'Graphics.DualTextureEffect', lambda r: Binary_Xnb.DualTextureEffect(r)),
        TypeReader['Binary_Xnb.EnvironmentMapEffect']('EnvironmentMapEffectReader', 'Graphics.EnvironmentMapEffect', lambda r: Binary_Xnb.EnvironmentMapEffect(r)),
        TypeReader['Binary_Xnb.SkinnedEffect']('SkinnedEffectReader', 'Graphics.SkinnedEffect', lambda r: Binary_Xnb.SkinnedEffect(r)),
        TypeReader['Binary_Xnb.SpriteFont']('SpriteFontReader', 'Graphics.SpriteFont', lambda r: Binary_Xnb.SpriteFont(r)),
        TypeReader['Binary_Xnb.Model']('ModelReader', 'Graphics.Model', lambda r: Binary_Xnb.Model(r)),

        # Media types
        TypeReader['Binary_Xnb.SoundEffect']('SoundEffectReader', 'Audio.SoundEffect', lambda r: Binary_Xnb.SoundEffect(r)),
        TypeReader['Binary_Xnb.Song']('SongReader', 'Media.Song', lambda r: Binary_Xnb.Song(r)),
        TypeReader['Binary_Xnb.Video']('VideoReader', 'Media.Video', lambda r: Binary_Xnb.Video(r))]
    typeReaderMap: dict[str, TypeReader] = { x.name:x for x in typeReaders }

    # ContentReader
    class ContentReader(Reader):
        def __init__(self, f): super().__init__(f); self.readers: list[Binary_Xnb.TypeReader] = None
        @staticmethod
        def add(reader: Binary_Xnb.TypeReader) -> Binary_Xnb.TypeReader:
            Binary_Xnb.typeReaders.append(reader)
            Binary_Xnb.typeReaderMap[reader.name] = reader
            return reader
        def readTypeManifest(self) -> None:
            self.readers: list[Binary_Xnb.TypeReader] = self.readLV7FArray(lambda z: Binary_Xnb.ContentReader.getByName(self.readLV7UString(), self.readUInt32()))
            # for s in [x for x in self.readers if x.init]: s.init()

        def read[T](self, reader: Binary_Xnb.TypeReader) -> T: return reader.read(self) if reader else None
        def readObject[T](self) -> T: return self.read(self.readTypeId())
        def readValueOrObject[T](self, reader: Binary_Xnb.TypeReader) -> T: return self.read(reader) if reader.valueType else self.readObject()
        def readTypeId(self) -> Binary_Xnb.TypeReader: typeId = self.readVInt7() - 1; return self.readers[typeId] if typeId < len(self.readers) else _throw('Invalid XNB file: typeId is out of range.') if typeId >= 0 else None 
        def validate(self, type: str) -> Binary_Xnb.ContentReader: reader = self.readTypeId(); return self if reader and reader.target == type else _throw('Invalid XNB file: got an unexpected typeId.')
        def readResource(self) -> Binary_Xnb.ResourceRef: return ResourceRef(self)

        @staticmethod
        def create(s: Binary_Xnb.GenericReader, args: list[str]) -> Binary_Xnb.TypeReader:
            r: Binary_Xnb.TypeReader = None
            if len(args) == 1:
                value = Binary_Xnb.ContentReader.getByTarget(args[0])
                r = s.ginit(value)
            elif len(args) == 2:
                key = Binary_Xnb.ContentReader.getByTarget(args[0])
                value = Binary_Xnb.ContentReader.getByTarget(args[1])
                r = s.ginit(key, value)
            else: raise Exception()
            suffix = f'`{len(args)}[[{'],['.join(args)}]]'
            r.name = s.name + suffix
            r.target = s.target + suffix
            r.valueType = s.valueType
            return r

        @staticmethod
        def getByName(name: str, version: int) -> Binary_Xnb.TypeReader:
            wanted = Binary_Xnb.stripAssemblyVersion(name).replace('Microsoft.Xna.Framework.Content.', '')
            if wanted in Binary_Xnb.typeReaderMap: return Binary_Xnb.typeReaderMap[wanted]
            genericName, args = Binary_Xnb.splitGenericTypeName(wanted)
            if not genericName: return (None, None)
            if genericName in Binary_Xnb.typeReaderMap and (generic := Binary_Xnb.typeReaderMap[genericName]) != None:
                reader = Binary_Xnb.ContentReader.create(generic, args)
                if reader.name != wanted: raise Exception('ERROR')
                return Binary_Xnb.ContentReader.add(reader)
            _throw(f'Cannot find type reader "{wanted}".')
        @staticmethod
        def getByTarget(target: str) -> Binary_Xnb.TypeReader:
            wanted = Binary_Xnb.stripAssemblyVersion(target).replace('Microsoft.Xna.Framework.', '')
            reader = next(iter([x for x in Binary_Xnb.typeReaders if x.target == wanted]), None)
            if reader: return reader
            _throw(f'Cannot find type reader "{wanted}".')

    @staticmethod
    def stripAssemblyVersion(name: str) -> str:
        commaIndex = 0
        while (commaIndex := name.find(',', commaIndex)) != -1:
            if commaIndex + 1 < len(name) and name[commaIndex + 1] == '[': commaIndex+=1
            else:
                closeBracket = name.find(']', commaIndex)
                if closeBracket != -1: name = name[:commaIndex] + name[closeBracket:]
                else: name = name[:commaIndex]
        return name

    @staticmethod
    def splitGenericTypeName(name: str) -> tuple[str, list[str]]:
        # look for the ` generic marker character.
        pos = name.find('`')
        if pos == -1: return (None, None)
        # everything to the left of ` is the generic type name.
        genericName = name[:pos]; args = []
        # advance to the start of the generic argument list.
        pos+=1
        while pos < len(name) and name[pos].isdigit(): pos+=1
        while pos < len(name) and name[pos] == '[': pos+=1
        # split up the list of generic type arguments.
        while pos < len(name) and name[pos] != ']':
            # locate the end of the current type name argument.
            nesting = 0; end = 0
            for end in range(pos, len(name)):
                # handle nested types in case we have eg. "List`1[[List`1[[Int]]]]".
                if name[end] == '[': nesting+=1
                elif name[end] == ']':
                    if nesting > 0: nesting-=1
                    else: break
            # extract the type name argument.
            args.append(name[pos:end])
            # skip past the type name, plus any subsequent "],[" goo.
            pos = end
            if pos < len(name) and name[pos] == ']': pos+=1
            if pos < len(name) and name[pos] == ',': pos+=1
            if pos < len(name) and name[pos] == '[': pos+=1
        return (genericName, args)

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
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.format: Binary_Xnb.SurfaceFormat = Binary_Xnb.SurfaceFormat(r.readInt32())
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
                case Binary_Xnb.SurfaceFormat.Color: format = (TextureFormat.RGBA32, TexturePixel.Unknown)
                case Binary_Xnb.SurfaceFormat.Bgr565: format = (TextureFormat.RGB565, TexturePixel.Unknown)
                case Binary_Xnb.SurfaceFormat.Bgra5551: format = (TextureFormat.BGRA1555, TexturePixel.Unknown)
                # case Binary_Xnb.SurfaceFormat.Bgra4444: format = (TextureFormat.X, TexturePixel.Unknown)
                case Binary_Xnb.SurfaceFormat.Dxt1: format = (TextureFormat.DXT1, TexturePixel.Unknown)
                case Binary_Xnb.SurfaceFormat.Dxt3: format = (TextureFormat.DXT3, TexturePixel.Unknown)
                case Binary_Xnb.SurfaceFormat.Dxt5: format = (TextureFormat.DXT5, TexturePixel.Unknown)
                # case Binary_Xnb.SurfaceFormat.NormalizedByte2: format = (TextureFormat.X, TexturePixel.Unknown)
                # case Binary_Xnb.SurfaceFormat.NormalizedByte4: format = (TextureFormat.X, TexturePixel.Unknown)
                # case Binary_Xnb.SurfaceFormat.Rgba1010102: format = (TextureFormat.X, TexturePixel.Unknown)
                # case Binary_Xnb.SurfaceFormat.Rg32: format = (TextureFormat.X, TexturePixel.Unknown)
                # case Binary_Xnb.SurfaceFormat.Rgba64: format = (TextureFormat.X, TexturePixel.Unknown)
                # case Binary_Xnb.SurfaceFormat.Alpha8: format = (TextureFormat.X, TexturePixel.Unknown)
                # case Binary_Xnb.SurfaceFormat.Single: format = (TextureFormat.X, TexturePixel.Unknown)
                # case Binary_Xnb.SurfaceFormat.Vector2: format = (TextureFormat.X, TexturePixel.Unknown)
                # case Binary_Xnb.SurfaceFormat.Vector4: format = (TextureFormat.X, TexturePixel.Unknown)
                # case Binary_Xnb.SurfaceFormat.HalfSingle: format = (TextureFormat.X, TexturePixel.Unknown)
                # case Binary_Xnb.SurfaceFormat.HalfVector2: format = (TextureFormat.X, TexturePixel.Unknown)
                # case Binary_Xnb.SurfaceFormat.HalfVector4: format = (TextureFormat.X, TexturePixel.Unknown)
                # case Binary_Xnb.SurfaceFormat.HdrBlendable: format = (TextureFormat.X, TexturePixel.Unknown)
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
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.format: Binary_Xnb.SurfaceFormat = Binary_Xnb.SurfaceFormat(r.readInt32())
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
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.format: Binary_Xnb.SurfaceFormat = Binary_Xnb.SurfaceFormat(r.readInt32())
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
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.indexFormat: int = 16 if r.readBoolean() else 32
            self.indexData: bytes = r.readL32Bytes()

    class VertexDeclaration:
        class Element:
            def __init__(self, r: Binary_Xnb.ContentReader):
                self.offset: int = r.readUInt32()
                self.format: Binary_Xnb.VertexElementFormat = Binary_Xnb.VertexElementFormat(r.readInt32())
                self.usage: Binary_Xnb.VertexElementUsage = Binary_Xnb.VertexElementUsage(r.readInt32())
                self.usageIndex: int = r.readUInt32()
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.vertexStride: int = r.readUInt32()
            self.elements: list[object] = r.readL32FArray(lambda z: Binary_Xnb.VertexDeclaration.Element(r))

    class VertexBuffer(VertexDeclaration):
        def __init__(self, r: Binary_Xnb.ContentReader):
            super().__init__(r)
            self.vertexs = r.readL32FArray(lambda z: r.readBytes(self.vertexStride))

    class Effect:
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.effectBytecode: str = r.readL32Bytes()

    class EffectMaterial:
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.effectReference: str = r.readLV7UString()
            self.Parameters: object = r.readObject()

    class BasicEffect:
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.textureReference: str = r.readLV7UString()
            self.diffuseColor: Vector3 = r.readVector3()
            self.emissiveColor: Vector3 = r.readVector3()
            self.specularColor: Vector3 = r.readVector3()
            self.specularPower: float = r.readSingle()
            self.alpha: float = r.readSingle()
            self.vertexColorEnabled: bool = r.readBoolean()

    class AlphaTestEffect:
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.textureReference: str = r.readLV7UString()
            self.compareFunction: Binary_Xnb.CompareFunction = Binary_Xnb.CompareFunction(r.readInt32())
            self.referenceAlpha: int = r.readUInt32()
            self.diffuseColor: Vector3 = r.readVector3()
            self.alpha: float = r.readSingle()
            self.vertexColorEnabled: bool = r.readBoolean()

    class DualTextureEffect:
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.texture1Reference: str = r.readLV7UString()
            self.texture2Reference: str = r.readLV7UString()
            self.diffuseColor: Vector3 = r.readVector3()
            self.alpha: float = r.readSingle()
            self.vertexColorEnabled: bool = r.readBoolean()

    class EnvironmentMapEffect:
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.textureReference: str = r.readLV7UString()
            self.environmentMapReference: str = r.readLV7UString()
            self.environmentMapAmount: float = r.readSingle()
            self.environmentMapSpecular: Vector3 = r.readVector3()
            self.fresnelFactor: float = r.readSingle()
            self.diffuseColor: Vector3 = r.readVector3()
            self.emissiveColor: Vector3 = r.readVector3()
            self.alpha: float = r.readSingle()

    class SkinnedEffect:
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.textureReference: str = r.readLV7UString()
            self.weightsPerVertex: int = r.readUInt32()
            self.diffuseColor: Vector3 = r.readVector3()
            self.emissiveColor: Vector3 = r.readVector3()
            self.specularColor: Vector3 = r.readVector3()
            self.specularPower: float = r.readSingle()
            self.alpha: float = r.readSingle()

    class SpriteFont(IHaveMetaInfo):
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.texture: Binary_Xnb.Texture2D = r.readObject()
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
        class BoneRef:
            def __init__(self, p: Model, r: Binary_Xnb.ContentReader):
                self.id: int = r.readByte() if p.bones8 else r.readUInt32()
            def done(self) -> Bone: return p.Bones[Id - 1] if self.id != 0 else None
        class Bone:
            def __init__(self, r: Binary_Xnb.ContentReader):
                self.name: str = r.readObject()
                self.transform: Matrix4x4 = r.readMatrix4x4()
                self.parent: BoneRef
                self.children: list[BoneRef]
        class MeshPart:
            def __init__(self, p: Model, r: Binary_Xnb.ContentReader):
                self.vertexOffset: int = r.readInt32()
                self.numVertices: int = r.readInt32()
                self.startIndex: int = r.readInt32()
                self.primitiveCount: int = r.readInt32()
                self.tag: object = r.readObject()
                self.resourceRef = r.readResource()
                self.indexBuffer: ResourceRef = r.readResource()
                self.effect: ResourceRef = r.readResource()
        class Mesh:
            def __init__(self, p: Model, r: Binary_Xnb.ContentReader):
                self.name: str = r.readObject()
                self.parent: BoneRef = BoneRef(p, r)
                self.bounds: BoundingSphere = BoundingSphere(r.readVector3(), r.readSingle())
                self.tag: object = r.readObject()
                self.parts: list[MeshPart] = r.readL32FArray(lambda z: MeshPart(r))
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.bones: list[Bone] = r.readL32FArray(lambda z: Bone(r)); self.bones8: bool = len(self.bones) < 255
            for s in self.bones: s.parent = BoneRef(self, r); s.children = r.readL32FArray(lambda z: BoneRef(self, r))
            self.meshs: list[Mesh] = r.readL32FArray(lambda z: Mesh(self, r))
            self.root: BoneRef = BoneRef(self, r)
            self.tag: object = r.readObject()
        def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
            MetaInfo(None, MetaContent(type = 'Data', name = os.path.basename(file.path), value = self)),
            MetaInfo('Model', items = [
                MetaInfo(f'Bones: {len(self.bones)}'),
                MetaInfo(f'Meshs: {len(self.meshs)}'),
                MetaInfo(f'Root: {self.root.bone.name}'),
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
        def __init__(self, r: Binary_Xnb.ContentReader):
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
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.filename: str = r.readLV7UString()
            self.duration: int = r.validate('System.Int32').readInt32()
        def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
            MetaInfo(None, MetaContent(type = 'Data', name = os.path.basename(file.path), value = self)),
            MetaInfo('Song', items = [
                MetaInfo(f'Filename: {self.filename}'),
                MetaInfo(f'Duration: {self.duration}')
                ])
            ]

    class Video(IHaveMetaInfo):
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.filename: str = r.validate('System.String').readLV7UString()
            self.duration: int = r.validate('System.Int32').readInt32()
            self.width: int = r.validate('System.Int32').readInt32()
            self.height: int = r.validate('System.Int32').readInt32()
            self.framesPerSecond: float = r.validate('System.Single').readSingle()
            self.soundtrackType: Binary_Xnb.SoundtrackType = Binary_Xnb.SoundtrackType(r.validate('System.Int32').readInt32())
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
        
    #region Headers

    MAGIC = 0x00424e58 #: XNB?

    class Header:
        _struct = ('<I2bI', 10)
        def __init__(self, tuple):
            self.magic, \
            self.version, \
            self.flags, \
            self.sizeOnDisk = tuple
        @property
        def compressed(self) -> bool: return (self.flags & 0x80) != 0
        @property
        def platform(self) -> chr: return chr(self.magic >> 24)

        def validate(self, r: Reader):
            if (self.magic & 0x00FFFFFF) != Binary_Xnb.MAGIC: raise Exception('BAD MAGIC')
            if self.version != 5 and self.version != 4: raise Exception('Invalid XNB version')
            if self.sizeOnDisk > r.f.getbuffer().nbytes: raise Exception('XNB file has been truncated.')
            if self.compressed:
                decompressedSize = r.readUInt32(); compressedSize = self.sizeOnDisk - r.tell()
                b = decompressXbox(r, compressedSize, decompressedSize)
                return Binary_Xnb.ContentReader(BytesIO(b))
            return Binary_Xnb.ContentReader(r.f)

    #endregion

    def __init__(self, r2: Reader):
        h = r2.readS(self.Header)
        r = h.validate(r2)
        r.readTypeManifest()
        resourceCount = r.readVInt7()
        self.obj = r.readObject()
        self.resources = r.readFArray(lambda z: r.readObject(), resourceCount)
        # r.ensureAtEnd() #h.sizeOnDisk

    def writeToStream(self, stream: object): return DesSer.serialize(self.obj, stream)

    def __repr__(self): return DesSer.serialize(self.obj)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return self.obj.getInfoNodes(resource, file, tag) if isinstance(self.obj, IHaveMetaInfo) else [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('Xnb', items = [
            MetaInfo(f'Obj: {self.obj}')
            ])
        ]


#endregion