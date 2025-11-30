from __future__ import annotations
import os, re, itertools
from io import BytesIO
from enum import IntEnum
from decimal import Decimal
from numpy import ndarray, array
from openstk import _throw, Reader
from openstk.gfx import Raster, Texture_Bytes, ITexture, TextureFormat, TexturePixel
from openstk.core.drawing import Plane, Point, Rectangle, BoundingBox, BoundingSphere, BoundingFrustum, Ray, Curve
from openstk.core.typex import *
from gamex import MetaInfo, MetaManager, MetaContent, IHaveMetaInfo
from gamex.core.globalx import ByteColor4
from gamex.core.formats.compression import decompressXbox
from openstk.core.mscorlib.System import *

# types
type Vector2 = ndarray
type Vector3 = ndarray
type Vector4 = ndarray
type Matrix4x4 = ndarray
type Quaternion = ndarray

#region Type Manager

class TypeReader:
    def __init__(self, t: type, valueType: bool = False, canUseObj: bool = False):
        self.type: type = t
        self.valueType: bool = valueType
        self.canUseObj: bool = canUseObj
    def init(self, manager: TypeManager) -> None: pass
    def read(self, r: ContentReader, o: object) -> object: pass

class TypeReader[T](TypeReader):
    def __init__(self, t: type, valueType: bool = False, canUseObj: bool = False):
        super().__init__(t, valueType, canUseObj)
    def read(self, r: ContentReader, o: T) -> T: pass

# Primitive types
class ByteReader(TypeReader[int]):
    def __init__(self, t: type): super().__init__(Byte, valueType=True)
    def read(self, r: ContentReader, o: int) -> int: return r.readByte()
class SByteReader(TypeReader[int]):
    def __init__(self, t: type): super().__init__(SByte, valueType=True)
    def read(self, r: ContentReader, o: int) -> int: return r.readSByte()
class Int16Reader(TypeReader[int]):
    def __init__(self, t: type): super().__init__(Int16, valueType=True)
    def read(self, r: ContentReader, o: int) -> int: return r.readInt16()
class UInt16Reader(TypeReader[int]):
    def __init__(self, t: type): super().__init__(UInt16, valueType=True)
    def read(self, r: ContentReader, o: int) -> int: return r.readUInt16()
class Int32Reader(TypeReader[int]):
    def __init__(self, t: type): super().__init__(Int32, valueType=True)
    def read(self, r: ContentReader, o: int) -> int: return r.readInt32()
class UInt32Reader(TypeReader[int]):
    def __init__(self, t: type): super().__init__(UInt32, valueType=True)
    def read(self, r: ContentReader, o: int) -> int: return r.readUInt32()
class Int64Reader(TypeReader[int]):
    def __init__(self, t: type): super().__init__(Int64, valueType=True)
    def read(self, r: ContentReader, o: int) -> int: return r.readInt64()
class UInt64Reader(TypeReader[int]):
    def __init__(self, t: type): super().__init__(UInt64, valueType=True)
    def read(self, r: ContentReader, o: int) -> int: return r.readUInt64()
class SingleReader(TypeReader[float]):
    def __init__(self, t: type): super().__init__(Single, valueType=True)
    def read(self, r: ContentReader, o: float) -> float: return r.ReadSingle()
class DoubleReader(TypeReader[float]):
    def __init__(self, t: type): super().__init__(Double, valueType=True)
    def read(self, r: ContentReader, o: float) -> float: return r.readDouble()
class BooleanReader(TypeReader[bool]):
    def __init__(self, t: type): super().__init__(Boolean, valueType=True)
    def read(self, r: ContentReader, o: bool) -> bool: return r.readBoolean()
class CharReader(TypeReader[chr]):
    def __init__(self, t: type): print('char'); exit(0); super().__init__(Char, valueType=True)
    def read(self, r: ContentReader, o: chr) -> chr: return r.readChar()
class StringReader(TypeReader[str]):
    def __init__(self, t: type): super().__init__(String)
    def read(self, r: ContentReader, o: str) -> str: return r.readString()

# System types
class EnumReader[T](TypeReader[int]):
    elem: TypeReader 
    def __init__(self, t: type): super().__init__(typex(Elem, t), valueType=True)
    def init(self, manager: TypeManager) -> None: self.elem = manager.getTypeReader(self.type.args[0])
    def read(self, r: ContentReader, o: int) -> int: return r.readRawObject(self.elem)
class NullableReader[T](TypeReader[T]):
    def __init__(self, t: type): super().__init__(typex(Nullable, t), valueType=True)
    def init(self, manager: TypeManager) -> None: self.elem = manager.getTypeReader(self.type.args[0])
    def read(self, r: ContentReader, o: T) -> T: return r.readObject(self.elem, None) if r.readBoolean() else None
class ArrayReader[T](TypeReader[int]):
    def __init__(self, t: type): super().__init__(typex(Array, t))
    def init(self, manager: TypeManager) -> None: self.elem = manager.getTypeReader(self.type.args[0])
    def read(self, r: ContentReader, o: int) -> int: return r.readL32FArray(lambda z: r.readObject(self.elem, None), obj=o)
class ListReader[T](TypeReader[int]):
    def __init__(self, t: type): super().__init__(typex(Collections.Generic.List, t), canUseObj=False)
    def init(self, manager: TypeManager) -> None: self.elem = manager.getTypeReader(self.type.args[0])
    def read(self, r: ContentReader, o: int) -> int: return r.readL32FList(lambda z: r.readObject(self.elem, None), obj=o)
class DictionaryReader[TKey, TValue](TypeReader[dict[TKey, TValue]]):
    def __init__(self, t: type): super().__init__(typex(Collections.Generic.Dictionary, t))
    def init(self, manager: TypeManager) -> None: self.key = manager.getTypeReader(self.type.args[0]); self.value = manager.getTypeReader(self.type.args[1])
    def read(self, r: ContentReader, o: dict[TKey, TValue]) -> dict[TKey, TValue]: return r.readL32FMany(None, lambda z: r.readObject(self.key, None), lambda z: r.readObject(self.value, None), obj=o)
# class MultiArrayReader[T](TypeReader[T]):
#     def __init__(self, t: type): super().__init__(t)
#     def init(self, manager: TypeManager) -> None: pass
#     def read(self, r: ContentReader, o: T) -> T: raise Exception('Not Implemented')
class TimeSpanReader(TypeReader[object]):
    def __init__(self, t: type): super().__init__(TimeSpan, valueType=True)
    def read(self, r: ContentReader, o: object) -> object: raise Exception('Not Implemented')
class DateTimeReader(TypeReader[object]):
    def __init__(self, t: type): super().__init__(DateTime, valueType=True)
    def read(self, r: ContentReader, o: object) -> object: raise Exception('Not Implemented')
class DecimalReader(TypeReader[Decimal]):
    def __init__(self, t: type): super().__init__(Decimal, valueType=True)
    def read(self, r: ContentReader, o: Decimal) -> Decimal: return r.readDecimal()
class ExternalReferenceReader(TypeReader[str]):
    def __init__(self, t: type): super().__init__(String)
    def read(self, r: ContentReader, o: str) -> str: return r.readExternalReference()
class ReflectiveReader(TypeReader[object]):
    baseTypeReader: TypeReader = None
    constructor: object = None
    readers: list[object] = None
    def __init__(self, t: type): super().__init__(typex(Object, t))

    def init(self, manager: TypeManager) -> None:
        self.canUseObj = Type.isClass(self.type)
        baseType = Type.baseType(self.type)
        if baseType and baseType != object: self.baseTypeReader = manager.getTypeReader(baseType)
        self.constructor = TypeX.getDefaultConstructor(self.type)
        properties, fields = TypeX.getAllProperties(self.type), TypeX.getAllFields(self.type)
        self.readers = []
        for prop in properties:
            if (reader := TypeManager.getElementReader(manager, prop)): self.readers.append(reader)
        for field in fields:
            if (reader := TypeManager.getElementReader(manager, field)): self.readers.append(reader)

    @staticmethod
    def getElementReader(manager: TypeManager, member: MemberInfo) -> callable:
        property, field = (member if isinstance(member, PropertyInfo) else None), (member if isinstance(member, FieldInfo) else None)
        if property and (not property.canRead or property.getIndexParameters()): return None

        # ignore
        if Attribute.getCustomAttribute(member, 'Ignore'): return None

        # optional
        optional = Attribute.getCustomAttribute(member, 'Optional')
        if not optional:
            if property:
                # if not property.getGetMethod().isPublic: return None
                if not property.canWrite:
                    reader2 = manager.getTypeReader(property.propertyType)
                    if not reader2 or not reader2.canUseObj: return None
            elif not field.isPublic or field.isInitOnly: return None

        # setter
        setter: callable; elem: type
        if property: elem = property.propertyType; setter = lambda o, v: property.setValue(o, v, None) if property.canWrite else lambda o, v: None
        else: elem = field.fieldType; setter = field.setValue

        # resources get special treatment.
        if optional and optional['sharedResource']: return lambda r, p: r.readSharedResource(lambda value: setter(p, value))

        # we need to have a reader at this point.
        reader = TypeManager.getTypeReader(elem)
        if not reader:
            if elem.name == 'Array': reader = ArrayReader[list]()
            raise Exception(f'Content reader could not be found for {elem.__name__} type.')
        
        # we use the construct delegate to pick the correct existing object to be the target of deserialization.
        construct = lambda parent: property.getValue(parent, None) if property and not property.canWrite else lambda parent: None
        return lambda r, p: setter(p, r.readObject(reader, construct(p)))
        
    def read(self, r: ContentReader, o: object) -> object:
        obj = o if o != None else (TypeX.createInstance(self.type) if not constructor else constructor(None))
        if self.baseTypeReader: baseTypeReader.read(r, obj)
        for reader in self.readers: reader(r, obj)
        return obj

# Math types
class Vector2Reader(TypeReader[Vector2]):
    def __init__(self, t: type): super().__init__(Vector2, valueType=True)
    def read(self, r: ContentReader, o: Vector2) -> Vector2: return r.readVector2()
class Vector3Reader(TypeReader[Vector3]):
    def __init__(self, t: type): super().__init__(Vector3, valueType=True)
    def read(self, r: ContentReader, o: Vector3) -> Vector3: return r.readVector3()
class Vector4Reader(TypeReader[Vector4]):
    def __init__(self, t: type): super().__init__(Vector4, valueType=True)
    def read(self, r: ContentReader, o: Vector4) -> Vector4: return r.readVector4()
class MatrixReader(TypeReader[Matrix4x4]):
    def __init__(self): super().__init__(Matrix4x4, valueType=True)
    def read(self, r: ContentReader, o: Matrix4x4) -> Matrix4x4: return r.readMatrix4x4()
class QuaternionReader(TypeReader[Quaternion]):
    def __init__(self, t: type): super().__init__(Quaternion, valueType=True)
    def read(self, r: ContentReader, o: Quaternion) -> Quaternion: return r.readQuaternion()
class ColorReader(TypeReader[ByteColor4]):
    def __init__(self, t: type): super().__init__(ByteColor4, valueType=True)
    def read(self, r: ContentReader, o: ByteColor4) -> ByteColor4: return ByteColor4(r.readByte(), r.readByte(), r.readByte(), r.readByte())
class PlaneReader(TypeReader[Plane]):
    def __init__(self, t: type): super().__init__(Plane, valueType=True)
    def read(self, r: ContentReader, o: Plane) -> Plane: return Plane(r.readVector3(), r.readSingle())
class PointReader(TypeReader[Point]):
    def __init__(self, t: type): super().__init__(Point, valueType=True)
    def read(self, r: ContentReader, o: Point) -> Point: return Point(r.readInt32(), r.readInt32())
class RectangleReader(TypeReader[Rectangle]):
    def __init__(self, t: type): super().__init__(Rectangle, valueType=True)
    def read(self, r: ContentReader, o: Rectangle) -> Rectangle: return Rectangle(r.readInt32(), r.readInt32(), r.readInt32(), r.readInt32())
class BoundingBoxReader(TypeReader[BoundingBox]):
    def __init__(self, t: type): super().__init__(BoundingBox, valueType=True)
    def read(self, r: ContentReader, o: BoundingBox) -> BoundingBox: return BoundingBox(r.readVector3(), r.readVector3())
class BoundingSphereReader(TypeReader[BoundingSphere]):
    def __init__(self, t: type): super().__init__(BoundingSphere, valueType=True)
    def read(self, r: ContentReader, o: BoundingSphere) -> BoundingSphere: return BoundingSphere(r.readVector3(), r.readSingle())
class BoundingFrustumReader(TypeReader[BoundingFrustum]):
    def __init__(self, t: type): super().__init__(BoundingFrustum, valueType=True)
    def read(self, r: ContentReader, o: BoundingFrustum) -> BoundingFrustum: return BoundingFrustum(r.readMatrix4x4())
class RayReader(TypeReader[Ray]):
    def __init__(self, t: type): super().__init__(Ray, valueType=True)
    def read(self, r: ContentReader, o: Ray) -> Ray: return Ray(r.readVector3(), r.readVector3())
class CurveReader(TypeReader[Curve]):
    def __init__(self, t: type): super().__init__(Curve, valueType=True)
    def read(self, r: ContentReader, o: Curve) -> Curve: return Curve(r.readInt32(), r.readInt32(), r.readL32FArray(lambda z: Curve.Key(z.readSingle(), z.readSingle(), z.readSingle(), z.readSingle(), z.readInt32())))

# Graphics types
class TextureReader(TypeReader['Texture']):
    def __init__(self, t: type): super().__init__(t)
    def read(self, r: ContentReader, o: Texture) -> Texture: return o
class Texture2DReader(TypeReader['Texture2D']):
    def __init__(self, t: type): super().__init__(Texture2D)
    def read(self, r: ContentReader, o: Texture2D) -> Texture2D: return Texture2D(r)
class Texture3DReader(TypeReader['Texture3D']):
    def __init__(self, t: type): super().__init__(Texture3D)
    def read(self, r: ContentReader, o: Texture3D) -> Texture3D: return Texture3D(r)
class TextureCubeReader(TypeReader['TextureCube']):
    def __init__(self, t: type): super().__init__(TextureCube)
    def read(self, r: ContentReader, o: TextureCube) -> TextureCube: return TextureCube(r)
class IndexBufferReader(TypeReader['IndexBuffer']):
    def __init__(self, t: type): super().__init__(IndexBuffer)
    def read(self, r: ContentReader, o: IndexBuffer) -> IndexBuffer: return IndexBuffer(r)
class VertexBufferReader(TypeReader['VertexBuffer']):
    def __init__(self, t: type): super().__init__(VertexBuffer)
    def read(self, r: ContentReader, o: VertexBuffer) -> VertexBuffer: return VertexBuffer(r)
class VertexDeclarationReader(TypeReader['VertexDeclaration']):
    def __init__(self, t: type): super().__init__(VertexDeclaration)
    def read(self, r: ContentReader, o: VertexDeclaration) -> VertexDeclaration: return VertexDeclaration(r)
class EffectReader(TypeReader['Effect']):
    def __init__(self, t: type): super().__init__(Effect)
    def read(self, r: ContentReader, o: Effect) -> Effect: return Effect(r)
class EffectMaterialReader(TypeReader['EffectMaterial']):
    def __init__(self, t: type): super().__init__(EffectMaterial)
    def read(self, r: ContentReader, o: EffectMaterial) -> EffectMaterial: return EffectMaterial(r)
class BasicEffectReader(TypeReader['BasicEffect']):
    def __init__(self, t: type): super().__init__(BasicEffect)
    def read(self, r: ContentReader, o: BasicEffect) -> BasicEffect: return BasicEffect(r)
class AlphaTestEffectReader(TypeReader['AlphaTestEffect']):
    def __init__(self, t: type): super().__init__(AlphaTestEffect)
    def read(self, r: ContentReader, o: AlphaTestEffect) -> AlphaTestEffect: return AlphaTestEffect(r)
class DualTextureEffectReader(TypeReader['DualTextureEffect']):
    def __init__(self, t: type): super().__init__(DualTextureEffect)
    def read(self, r: ContentReader, o: DualTextureEffect) -> DualTextureEffect: return DualTextureEffect(r)
class EnvironmentMapEffectReader(TypeReader['EnvironmentMapEffect']):
    def __init__(self, t: type): super().__init__(EnvironmentMapEffect)
    def read(self, r: ContentReader, o: EnvironmentMapEffect) -> EnvironmentMapEffect: return EnvironmentMapEffect(r)
class SkinnedEffectReader(TypeReader['SkinnedEffect']):
    def __init__(self, t: type): super().__init__(SkinnedEffect)
    def read(self, r: ContentReader, o: SkinnedEffect) -> SkinnedEffect: return SkinnedEffect(r)
class SpriteFontReader(TypeReader['SpriteFont']):
    def __init__(self, t: type): super().__init__(SpriteFont)
    def read(self, r: ContentReader, o: SpriteFont) -> SpriteFont: return SpriteFont(r)
class ModelReader(TypeReader['Model']):
    def __init__(self, t: type): super().__init__(Model)
    def read(self, r: ContentReader, o: Model) -> Model: return Model(r)

# Media types
class SoundEffectReader(TypeReader['SoundEffect']):
    def __init__(self, t: type): super().__init__(SoundEffect)
    def read(self, r: ContentReader, o: SoundEffect) -> SoundEffect: return SoundEffect(r)
class SongReader(TypeReader['Song']):
    def __init__(self, t: type): super().__init__(Song)
    def read(self, r: ContentReader, o: Song) -> Song: return Song(r)
class VideoReader(TypeReader['Video']):
    def __init__(self, t: type): super().__init__(Video)
    def read(self, r: ContentReader, o: Video) -> Video: return Video(r)

# TypeManager
@RAssembly()
class TypeManager:
    ltypes: dict[str, dict[str, type]] = {
        'MonoGame.Framework': {
            'BoundingBox': BoundingBox,
            'BoundingFrustum': BoundingFrustum,
            'BoundingSphere': BoundingSphere,
            'Color': ByteColor4,
            'Curve': Curve,
            'CurveContinuity': Curve.Continuity,
            'CurveKey': Curve.Key,
            'CurveLoopType': Curve.LoopType,
            'Matrix': Matrix4x4,
            'Plane': Plane,
            'Point': Point,
            'Quaternion': Quaternion,
            'Ray': Ray,
            'Rectangle': Rectangle,
            'Vector2': Vector2,
            'Vector3': Vector3,
            'Vector4': Vector4
            }
        }

class TypeManager:
    assembly: Assembly = Assembly.getAssembly(TypeManager)
    assemblyName: str = TypeManager.__module__
    readersCache: dict[type, TypeReader] = {}
    readers: dict[type, TypeReader] = {}
    typeFactories: dict[str, callable] = []

    def getTypeReader(self, type: type) -> TypeReader:
        # print(f'getTypeReader: {type.__name__} in {[s for s in self.readers.keys()]}')
        # if Type.isArray(type): type = Array
        return reader if type in self.readers and (reader := self.readers[type]) else _throw(f'Type reader not found for {type.__name__}.')

    def loadTypeReaders(self, r: ContentReader) -> list[TypeReader]:
        # the first content byte i read tells me the number of content readers in this XNB file
        readerCount = r.readIntV7()
        readers = [TypeReader] * readerCount
        needsInit = [False] * readerCount
        self.readers = {}
        for i in range(readerCount):
            readerName = r.readString()
            if readerName in self.typeFactories and (readerFunc := self.typeFactories[readerName]): readers[i] = readerFunc(); needsInit[i] = True
            else:
                readerType = self.getRType(readerName)
                if not (readerType in self.readersCache and (reader := self.readersCache[readerType])):
                    try: reader = Type.invoke(TypeX.getDefaultConstructor(readerType), None)
                    except Exception as e: raise Exception(f'Failed to get default constructor for TypeReader. To work around, add a creation function to ContentTypeReaderManager.AddTypeFactory() with the following failed type string: {readerName}.\n{e}')
                    needsInit[i] = True
                    self.readersCache[readerType] = reader
                readers[i] = reader
            type = readers[i].type
            if type and type not in self.readers: self.readers[type] = readers[i]
            r.readInt32()
        # initialize any new readers
        for i in range(len(self.readers)):
            if needsInit[i]: readers[i].init(self)
        return readers

    @staticmethod
    def getRType(typeName: str) -> type:
        typeName = typeName.replace('Microsoft.Xna.', 'MonoGame.').replace('MonoGame.Framework.Content.', '')
        # needed to support nested types
        count = len(typeName.split('[[')) - 1
        for i in range(count): typeName = re.sub(r'\[(.+?), Version=.+?\]', '[\\1]', typeName)
        # handle non generic types
        if 'PublicKeyToken' in typeName: typeName = re.sub(r'(.+?), Version=.+?$', '\\1', typeName)
        typeName = typeName.replace(', MonoGame.Framework.Graphics', f', {TypeManager.assemblyName}')
        typeName = typeName.replace(', MonoGame.Framework.Video', f', {TypeManager.assemblyName}')
        typeName = typeName.replace(', MonoGame.Framework', f', {TypeManager.assemblyName}')
        typeName = typeName.replace('System.Private.CoreLib', 'mscorlib')
        return TypeX.getRType(TypeManager.assembly, typeName) or _throw(f'Could not find TypeReader Type: {typeName}')
    @staticmethod
    def addTypeFactory(typeName: str, factory: callable) -> None:
        if typeName not in TypeManager.typeFactories: TypeManager.typeFactories[typeName] = factory
    @staticmethod
    def clearTypeFactories() -> None: TypeManager.typeFactories.clear()

# ContentReader
class ContentReader(Reader):
    def __init__(self, f, assetName: str, version: int, recordDisposableAction: callable):
        super().__init__(f)
        self.assetName: str = assetName
        self.version: int = version
        self.recordDisposableAction: callable = recordDisposableAction
        self.typeManager: TypeManager
        self.typeReaders: list[TypeReader]
        self.sharedResourceCount: int
        self.sharedResourceFixups: list[object]

    def recordDisposable[T](self, result: T) -> None:
        if not isinstance(result, IDisposable): return
        if recordDisposableAction: recordDisposableAction(disposable)

    def readTypeReaders(self) -> None:
        self.typeManager = TypeManager()
        self.typeReaders = self.typeManager.loadTypeReaders(self)
        self.sharedResourceCount = self.readIntV7()
        self.sharedResourceFixups = []

    def readAsset[T](self, obj: object = None) -> object: self.readTypeReaders(); s = self.readObject(obj); self.readSharedResources(); return s
    
    def readExternalReference(self) -> str: return self.readString()

    def readObject[T](self, *args) -> T:
        match len(args):
            case 0: return self.innerReadObject(None)
            case 1:
                if not isinstance(args[0], TypeReader): obj = args[0]; return self.innerReadObject(None)
                else: reader = args[0]; s = reader.read(self, None); self.recordDisposable(s); return s
            case 2:
                reader = args[0]; obj = args[1]
                # if reader.valueType != reader.type.isValueType: raise Exception('valueType mismatch')
                if not reader.valueType: return self.readObject(obj)
                s = reader.read(self, obj); self.recordDisposable(s); return s
            case _: raise Exception('tomany params')

    def innerReadObject[T](self, obj: T) -> T:
        idx = self.readIntV7()
        if idx == 0: return obj
        if idx > len(self.typeReaders): raise Exception('Incorrect type reader index found.')
        s = self.typeReaders[idx - 1].read(self, obj); self.recordDisposable(s); return s

    def readRawObject[T](self, *args) -> T:
        match len(args):
            case 0: return self.readRawObject(None)
            case 1:
                if not isinstance(args[0], TypeReader):
                    obj = args[0]; type = self.type
                    for reader in self.typeReaders:
                        if reader.type == type: return self.readRawObject(reader, obj)
                    raise Exception('Not Supported')
                else: reader = args[0]; return self.readRawObject(reader, None)
            case 2: reader = args[0]; obj = args[1]; return reader.read(self, obj)
            case _: raise Exception('to many params')

    def readSharedResources(self) -> None:
        if self.sharedResourceCount <= 0: return
        self.sharedResources = r.readFArray(lambda z: self.innerReadObject(None), self.sharedResourceCount)
        # fixup shared resources by calling each registered action
        for fixup in self.sharedResourceFixups: fixup.value(self.sharedResourceFixups[fixup.key])
    def readSharedResource(self, fixup: callable) -> None:
        idx = self.readIntV7()
        if idx >= 0: self.sharedResourceFixups.append((idx - 1, lambda v: fixup(v),))

#endregion

#region Gfx Objects

class SurfaceFormat(IntEnum):
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

class VertexElementFormat(IntEnum):
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

class VertexElementUsage(IntEnum):
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

class CompareFunction(IntEnum):
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
        if r:
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
        if r:
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
        if r:
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
        if r:
            self.indexFormat: int = 16 if r.readBoolean() else 32
            self.indexData: bytes = r.readL32Bytes()

class VertexDeclaration:
    class Element:
        def __init__(self, r: ContentReader):
            if r:
                self.offset: int = r.readUInt32()
                self.format: VertexElementFormat = VertexElementFormat(r.readInt32())
                self.usage: VertexElementUsage = VertexElementUsage(r.readInt32())
                self.usageIndex: int = r.readUInt32()
    def __init__(self, r: ContentReader):
        if r:
            self.vertexStride: int = r.readUInt32()
            self.elements: list[object] = r.readL32FArray(lambda z: VertexDeclaration.Element(r))

class VertexBuffer(VertexDeclaration):
    def __init__(self, r: ContentReader):
        super().__init__(r)
        if r:
            self.vertexs = r.readL32FArray(lambda z: r.readBytes(self.vertexStride))

class Effect:
    def __init__(self, r: ContentReader):
        if r:
            self.effectBytecode: str = r.readL32Bytes()

class EffectMaterial:
    def __init__(self, r: ContentReader):
        if r:
            self.effectReference: str = r.readLV7UString()
            self.parameters: dict[str, object] = r.readObject()

class BasicEffect:
    def __init__(self, r: ContentReader):
        if r:
            self.textureReference: str = r.readLV7UString()
            self.diffuseColor: Vector3 = r.readVector3()
            self.emissiveColor: Vector3 = r.readVector3()
            self.specularColor: Vector3 = r.readVector3()
            self.specularPower: float = r.readSingle()
            self.alpha: float = r.readSingle()
            self.vertexColorEnabled: bool = r.readBoolean()

class AlphaTestEffect:
    def __init__(self, r: ContentReader):
        if r:
            self.textureReference: str = r.readLV7UString()
            self.alphaFunction: CompareFunction = CompareFunction(r.readInt32())
            self.referenceAlpha: int = r.readUInt32()
            self.diffuseColor: Vector3 = r.readVector3()
            self.alpha: float = r.readSingle()
            self.vertexColorEnabled: bool = r.readBoolean()

class DualTextureEffect:
    def __init__(self, r: ContentReader):
        if r:
            self.texture1Reference: str = r.readLV7UString()
            self.texture2Reference: str = r.readLV7UString()
            self.diffuseColor: Vector3 = r.readVector3()
            self.alpha: float = r.readSingle()
            self.vertexColorEnabled: bool = r.readBoolean()

class EnvironmentMapEffect:
    def __init__(self, r: ContentReader):
        if r:
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
        if r:
            self.textureReference: str = r.readLV7UString()
            self.weightsPerVertex: int = r.readUInt32()
            self.diffuseColor: Vector3 = r.readVector3()
            self.emissiveColor: Vector3 = r.readVector3()
            self.specularColor: Vector3 = r.readVector3()
            self.specularPower: float = r.readSingle()
            self.alpha: float = r.readSingle()

class SpriteFont(IHaveMetaInfo):
    def __init__(self, r: ContentReader):
        if r:
            self.texture: Texture2D = r.readObject()
            self.glyphs: list[Rectangle] = r.readObject()
            self.cropping: list[Rectangle] = r.readObject()
            print(f'a {r.tell()}')
            self.characters: str = r.readObject()
            print(f'b {r.tell()}')
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
            if r:
                self.name: str = r.readObject()
                self.transform: Matrix4x4 = r.readMatrix4x4()
                self.parent: Bone = None
                self.children: list[Bone] = None
    class MeshPart:
        def __init__(self, p: Model, r: ContentReader):
            if r:
                self.vertexOffset: int = r.readInt32()
                self.numVertices: int = r.readInt32()
                self.startIndex: int = r.readInt32()
                self.primitiveCount: int = r.readInt32()
                self.tag: object = r.readObject()
                self.resourceRef = r.readResource()
                self.indexBuffer: ResourceRef = r.readResource()
                self.effect: ResourceRef = r.readResource()
    class Mesh:
        def __init__(self, p: Model, r: ContentReader):
            if r:
                self.name: str = r.readObject()
                self.parent: Bone = readBoneIdx(p, r)
                self.bounds: BoundingSphere = BoundingSphere(r.readVector3(), r.readSingle())
                self.tag: object = r.readObject()
                self.parts: list[MeshPart] = r.readL32FArray(lambda z: MeshPart(r))
    def __init__(self, r: ContentReader):
        if r:
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

class SoundtrackType(IntEnum):
    Music = 0
    Dialog = 1
    MusicDialog = 2

class SoundEffect(IHaveMetaInfo):
    def __init__(self, r: ContentReader):
        if r:
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
        if r:
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
        if r:
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
