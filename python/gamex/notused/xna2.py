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
from gamex.families.Uncore.formats.compression import decompressXbox

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
