from __future__ import annotations
import os, numpy as np
from io import BytesIO
from enum import Enum
from openstk import _throw, _pathExtension, Reader
from gamex import PakFile, BinaryPakFile, PakBinary, PakBinaryT, FileSource, MetaInfo, MetaManager, MetaContent, IHaveMetaInfo

# types
type Vector3 = np.ndarray

#region Binary_Xnb

# Binary_Xnb
class Binary_Xnb(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Xnb(r)

    #region Type Reader

    class TypeReader:
        def __init__(self, type: type, name: str, target: str, valueType: bool = False):
            self.type: type = type
            self.name: str = name
            self.target: str = target
            self.valueType: bool = valueType
            self.init: callable = None

    class TypeReader[T](TypeReader):
        def __init__(self, name: str, target: str, read: callable, valueType: bool = False):
            super().__init__(None, name, target, valueType)
            self.read: callable = read

    class GenericReader(TypeReader):
        def __init__(self, name: str, target: str, ginit: callable = None, valueType: bool = False, args: list[str] = None):
            super().__init__(name, target, None, valueType)
            self.ginit: callable = ginit
            self.args: list[str] = args
            self.keyReader: callable = None
            self.valueReader: callable = None

        def create(self, args: list[str]) -> Binary_Xnb.TypeReader:
            suffix = f'`{len(args)}[[{'],['.join(args)}]]'
            reader = Binary_Xnb.GenericReader(self.name + suffix, self.target + suffix, self.ginit, self.valueType, args)
            reader.init = lambda: self.ginit(reader)
            return reader

    # System types
    def _EnumReader(s):
        # s.target2 = s.args[0]
        s.read = lambda r: r.readInt32()
    def _NullableReader(s):
        # s.target2 = s.args[0]
        s.valueReader = Binary_Xnb.ContentReader.getByTarget(s.args[0])
        s.read = lambda r: r.read(s.valueReader) if r.readBoolean() else None
    def _ArrayReader(s):
        # s.target2 = s.args[0] + '[]'
        s.valueReader = Binary_Xnb.ContentReader.getByTarget(s.args[0])
        s.read = lambda r: r.readL32FArray(lambda z: r.readValueOrObject(s.valueReader))
    def _ListReader(s):
        # s.target 2= s.args[0]
        s.valueReader = Binary_Xnb.ContentReader.getByTarget(s.args[0])
        s.read = lambda r: r.readL32FArray(lambda z: r.readValueOrObject(s.valueReader))
    def _DictionaryReader(s):
        s.keyReader = Binary_Xnb.ContentReader.getByTarget(s.args[0])
        s.valueReader = Binary_Xnb.ContentReader.getByTarget(s.args[1])
        s.read = lambda r: r.readL32FMany(lambda z: r.readValueOrObject(s.keyReader), lambda z: r.readValueOrObject(s.valueReader))
    def _ReflectiveReader(s):
        # s.target2 = s.args[0]
        s.read = lambda r: _throw('NotSupportedException')
    typeReaders: list[TypeReader]  = [
        # Primitive types
        TypeReader('ByteReader', 'System.Byte', lambda r: r.readByte(), valueType=True),
        TypeReader('SByteReader', 'System.SByte', lambda r: r.readSByte(), valueType=True),
        TypeReader('Int16Reader', 'System.Int16', lambda r: r.readInt16(), valueType=True),
        TypeReader('UInt16Reader', 'System.UInt16', lambda r: r.readUInt16(), valueType=True),
        TypeReader('Int32Reader', 'System.Int32', lambda r: r.readInt32(), valueType=True),
        TypeReader('UInt32Reader', 'System.UInt32', lambda r: r.readUInt32(), valueType=True),
        TypeReader('Int64Reader', 'System.Int64', lambda r: r.readInt64(), valueType=True),
        TypeReader('UInt64Reader', 'System.UInt64', lambda r: r.readUInt64(), valueType=True),
        TypeReader('SingleReader', 'System.Single', lambda r: r.readSingle(), valueType=True),
        TypeReader('DoubleReader', 'System.Double', lambda r: r.readDouble(), valueType=True),
        TypeReader('BooleanReader', 'System.Boolean', lambda r: r.readBoolean(), valueType=True),
        TypeReader('CharReader', 'System.Char', lambda r: r.readChar(), valueType=True),
        TypeReader('StringReader', 'System.String', lambda r: r.readLV7UString()),
        TypeReader('ObjectReader', 'System.Object', lambda r: _throw('NotSupportedException')),

        # System types
        GenericReader('EnumReader', 'System.Enum', _EnumReader),
        GenericReader('NullableReader', 'System.Nullable', _NullableReader, valueType=True),
        GenericReader('ArrayReader', 'System.Array', _ArrayReader),
        GenericReader('ListReader', 'System.Collections.Generic.List', _ListReader),
        GenericReader('DictionaryReader', 'System.Collections.Generic.Dictionary', _DictionaryReader),
        # TypeReader('TimeSpanReader', 'System.TimeSpan', lambda r: { var v = r.readInt64(); return new TimeSpan(v); }, valueType=True),
        # TypeReader('DateTimeReader', 'System.DateTime', lambda r: { var v = r.readInt64(); return new DateTime(v & ~(3L << 62), (DateTimeKind)(v >> 62)); }, valueType=True),
        # TypeReader('DecimalReader', 'System.Decimal', lambda r: { uint a = r.readUInt32(), b = r.ReadUInt32(), c = r.ReadUInt32(), d = r.ReadUInt32(); return 0; }, valueType=True),
        TypeReader('ExternalReferenceReader', 'ExternalReference', lambda r: r.readString()),
 
        GenericReader('ReflectiveReader', 'System.Object', _ReflectiveReader),

        # Math types
        TypeReader('Vector2Reader', 'Framework.Vector2', lambda r: r.readVector2(), valueType=True),
        TypeReader('Vector3Reader', 'Framework.Vector3', lambda r: r.readVector3(), valueType=True),
        TypeReader('Vector4Reader', 'Framework.Vector4', lambda r: r.readVector4(), valueType=True),
        TypeReader('MatrixReader', 'Framework.Matrix', lambda r: r.readMatrix4x4(), valueType=True),
        TypeReader('QuaternionReader', 'Framework.Quaternion', lambda r: r.readQuaternion(), valueType=True),
        TypeReader('ColorReader', 'Framework.Color', lambda r: np.array([r.readByte(), r.readByte(), r.readByte(), r.readByte()]), valueType=True),
        TypeReader('PlaneReader', 'Framework.Plane', lambda r: (r.readVector3(), r.readSingle()), valueType=True),
        TypeReader('PointReader', 'Framework.Point', lambda r: np.array([r.readInt32(), r.readInt32()]), valueType=True),
        TypeReader('RectangleReader', 'Framework.Rectangle', lambda r: np.array([r.readInt32(), r.readInt32(), r.readInt32(), r.readInt32()]), valueType=True),
        TypeReader('BoundingBoxReader', 'Framework.BoundingBox', lambda r: (r.readVector3(), r.readVector3()), valueType=True),
        TypeReader('BoundingSphereReader', 'Framework.BoundingSphere', lambda r: (r.readVector3(), r.readSingle()), valueType=True),
        TypeReader('BoundingFrustumReader', 'Framework.BoundingFrustum', lambda r: r.readMatrix4x4()),
        TypeReader('RayReader', 'Framework.Ray', lambda r: (r.readVector3(), r.readVector3()), valueType=True),
        # TypeReader('CurveReader', 'Framework.Curve', lambda r: {
        #     var preLoop = r.ReadInt32();
        #     var postLoop = r.ReadInt32();
        #     var loops = r.ReadL32FArray(z => (position: z.ReadSingle(), value: z.ReadSingle(), tangentIn: z.ReadSingle(), tangentOut: z.ReadSingle(), continuity: z.ReadInt32()));
        #     return (preLoop, postLoop, loops);
        # }),

        # Graphics types
        TypeReader('TextureReader', 'Framework.Graphics.Texture', lambda r: _throw('NotSupportedException')),
        TypeReader('Texture2DReader', 'Framework.Graphics.Texture2D', lambda r: Binary_Xnb.Texture2D(r)),
        TypeReader('Texture3DReader', 'Framework.Graphics.Texture3D', lambda r: Binary_Xnb.Texture3D(r)),
        TypeReader('TextureCubeReader', 'Framework.Graphics.TextureCube', lambda r: Binary_Xnb.TextureCube(r)),
        TypeReader('IndexBufferReader', 'Framework.Graphics.IndexBuffer', lambda r: Binary_Xnb.IndexBuffer(r)),
        TypeReader('VertexBufferReader', 'Framework.Graphics.VertexBuffer', lambda r: Binary_Xnb.VertexBuffer(r)),
        TypeReader('VertexDeclarationReader', 'Framework.Graphics.VertexDeclaration', lambda r: Binary_Xnb.VertexDeclaration(r)),
        TypeReader('EffectReader', 'Framework.Graphics.Effect', lambda r: Binary_Xnb.Effect(r)),
        TypeReader('EffectMaterialReader', 'Framework.Graphics.EffectMaterial', lambda r: Binary_Xnb.EffectMaterial(r)),
        TypeReader('BasicEffectReader', 'Framework.Graphics.BasicEffect', lambda r: Binary_Xnb.BasicEffect(r)),
        TypeReader('AlphaTestEffectReader', 'Framework.Graphics.AlphaTestEffect', lambda r: Binary_Xnb.AlphaTestEffect(r)),
        TypeReader('DualTextureEffectReader', 'Framework.Graphics.DualTextureEffect', lambda r: Binary_Xnb.DualTextureEffect(r)),
        TypeReader('EnvironmentMapEffectReader', 'Framework.Graphics.EnvironmentMapEffect', lambda r: Binary_Xnb.EnvironmentMapEffect(r)),
        TypeReader('SkinnedEffectReader', 'Framework.Graphics.SkinnedEffect', lambda r: Binary_Xnb.SkinnedEffect(r)),
        TypeReader('SpriteFontReader', 'Framework.Graphics.SpriteFont', lambda r: Binary_Xnb.SpriteFont(r)),
        TypeReader('ModelReader', 'Framework.Graphics.Model', lambda r: Binary_Xnb.Model(r)),

        # Media types
        TypeReader('SoundEffectReader', 'Audio.SoundEffect', lambda r: Binary_Xnb.SoundEffect(r)),
        TypeReader('SongReader', 'Media.Song', lambda r: Binary_Xnb.Song(r)),
        TypeReader('VideoReader', 'Media.Video', lambda r: Binary_Xnb.Video(r))]
    typeReaderMap: dict[str, TypeReader] = { x.name:x for x in typeReaders }

    # ContentReader
    class ContentReader(Reader):
        def __init__(self, f): super().__init__(f); self.readers: list[Binary_Xnb.TypeReader] = None
        def readTypeManifest(self) -> None:
            self.readers = self.readLV7FArray(lambda z: Binary_Xnb.ContentReader.getByName(self.readLV7UString(), self.readUInt32()))
            for s in [x for x in self.readers if x.init]: s.init()
        def read(self, reader: Binary_Xnb.TypeReader): return reader.read(self) if reader else None
        def readObject(self) -> object: return self.read(self.readTypeId())
        def readValueOrObject(self, reader: Binary_Xnb.TypeReader) -> object: return self.read(reader) if reader.valueType else self.readObject()
        def readTypeId(self) -> Binary_Xnb.TypeReader: typeId = self.readVInt7() - 1; return self.readers[typeId] if typeId < len(self.readers) else _throw('Invalid XNB file: typeId is out of range.') if typeId >= 0 else None 
        def validate(self, type: str) -> Binary_Xnb.ContentReader: reader = self.readTypeId(); return self if reader and reader.target == type else _throw('Invalid XNB file: got an unexpected typeId.')

        @staticmethod
        def getByName(name: str, version: int) -> Binary_Xnb.TypeReader:
            wanted = Binary_Xnb.stripAssemblyVersion(name).replace('Microsoft.Xna.Framework.Content.', '')
            if wanted in Binary_Xnb.typeReaderMap: return Binary_Xnb.typeReaderMap[wanted]
            genericName, args = Binary_Xnb.splitGenericTypeName(wanted)
            if not genericName: return (None, None)
            if genericName in Binary_Xnb.typeReaderMap and (factory := Binary_Xnb.typeReaderMap[genericName]) != None:
                reader = factory.create(args)
                if reader.name != wanted: raise Exception('ERROR')
                Binary_Xnb.typeReaders.append(reader)
                Binary_Xnb.typeReaderMap[reader.name] = reader
                return reader
            _throw(f'Cannot find type reader "{wanted}".')
        @staticmethod
        def getByTarget(target: str) -> Binary_Xnb.TypeReader:
            wanted = Binary_Xnb.stripAssemblyVersion(target).replace('Microsoft.Xna.', '')
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

    class Texture2D:
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.format: Binary_Xnb.SurfaceFormat = Binary_Xnb.SurfaceFormat(r.readInt32())
            self.width: int = r.readUInt32()
            self.height: int = r.readUInt32()
            self.mips: list[bytes] = r.readL32FArray(lambda z: r.readL32Bytes())

    class Texture3D:
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.format: Binary_Xnb.SurfaceFormat = Binary_Xnb.SurfaceFormat(r.readInt32())
            self.width: int = r.readUInt32()
            self.height: int = r.readUInt32()
            self.depth: int = r.readUInt32()
            self.mips: list[bytes] = r.ReadL32FArray(lambda z: r.readL32Bytes())

    class TextureCube:
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.format: Binary_Xnb.SurfaceFormat = Binary_Xnb.SurfaceFormat(r.readInt32())
            self.size: int = r.readUInt32()
            self.face1Mips: list[bytes] = r.readL32FArray(lambda z: r.readL32Bytes())
            self.face2Mips: list[bytes] = r.readL32FArray(lambda z: r.readL32Bytes())
            self.face3Mips: list[bytes] = r.readL32FArray(lambda z: r.readL32Bytes())
            self.face4Mips: list[bytes] = r.readL32FArray(lambda z: r.readL32Bytes())
            self.face5Mips: list[bytes] = r.readL32FArray(lambda z: r.readL32Bytes())
            self.face6Mips: list[bytes] = r.readL32FArray(lambda z: r.readL32Bytes())

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

    class SpriteFont:
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.texture: Binary_Xnb.Texture2D = r.readObject()
            self.glyphs: object = r.readObject()
            self.cropping: object = r.readObject()
            self.characters: object = r.readObject()
            self.verticalLinespacing: int = r.readInt32()
            self.horizontalSpacing: float = r.readSingle()
            self.kerning: object = r.readObject()
            self.defaultCharacter: chr = r.ReadChar() if r.readBoolean() else '\x00'

    class Model:
        def __init__(self, r: Binary_Xnb.ContentReader):
            pass

    #endregion

    #region Media Objects

    class SoundtrackType(Enum):
        Music = 0
        Dialog = 1
        MusicDialog = 2

    class SoundEffect:
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.format: bytes = r.readL32Bytes()
            self.data: bytes = r.readL32Bytes()
            self.loopStart: int = r.readInt32()
            self.loopLength: int = r.readInt32()
            self.duration: int = r.readInt32()

    class Song:
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.filename: str = r.readLV7UString()
            self.duration: int = r.validate('System.Int32').readInt32()

    class Video:
        def __init__(self, r: Binary_Xnb.ContentReader):
            self.filename: str = r.validate('System.String').readLV7UString()
            self.duration: int = r.validate('System.Int32').readInt32()
            self.width: int = r.validate('System.Int32').readInt32()
            self.height: int = r.validate('System.Int32').readInt32()
            self.framesPerSecond: float = r.validate('System.Single').readSingle()
            self.soundtrackType: Binary_Xnb.SoundtrackType = Binary_Xnb.SoundtrackType(r.validate('System.Int32').readInt32())

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
                print(f'{decompressedSize} bytes of asset data are compressed into {compressedSize}')
                raise Exception('Unsupported reading of compressed XNB files.')

    #endregion

    def __init__(self, r2: Reader):
        r = self.ContentReader(r2.f)
        h = r.readS(self.Header)
        h.validate(r)
        r.readTypeManifest()
        self.objs = r.readFArray(lambda z: r.readObject(), r.readVInt7() + 1)
        #r.ensureAtEnd(h.sizeOnDisk)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        # MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self.data))
        ]

#endregion