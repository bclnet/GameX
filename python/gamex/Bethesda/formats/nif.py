import os
from io import BytesIO
from enum import Enum, Flag
from typing import TypeVar, Generic
from gamex import FileSource, PakBinaryT, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo
from gamex.compression import decompressLz4, decompressZlib
from gamex.Bethesda.formats.records import FormType, Header

T = TypeVar('T')

# typedefs
class Reader: pass
class Color: pass

#region Enums

# texture enums
class ApplyMode(Enum):
    APPLY_REPLACE = 0,
    APPLY_DECAL = 1,
    APPLY_MODULATE = 2,
    APPLY_HILIGHT = 3,
    APPLY_HILIGHT2 = 4

class TexClampMode(Enum):
    CLAMP_S_CLAMP_T = 0,
    CLAMP_S_WRAP_T = 1,
    WRAP_S_CLAMP_T = 2,
    WRAP_S_WRAP_T = 3

class TexFilterMode(Enum):
    FILTER_NEAREST = 0,
    FILTER_BILERP = 1,
    FILTER_TRILERP = 2,
    FILTER_NEAREST_MIPNEAREST = 3,
    FILTER_NEAREST_MIPLERP = 4,
    FILTER_BILERP_MIPNEAREST = 5

class PixelLayout(Enum):
    PIX_LAY_PALETTISED = 0,
    PIX_LAY_HIGH_COLOR_16 = 1,
    PIX_LAY_TRUE_COLOR_32 = 2,
    PIX_LAY_COMPRESSED = 3,
    PIX_LAY_BUMPMAP = 4,
    PIX_LAY_PALETTISED_4 = 5,
    PIX_LAY_DEFAULT = 6

class MipMapFormat(Enum):
    MIP_FMT_NO = 0,
    MIP_FMT_YES = 1,
    MIP_FMT_DEFAULT = 2

class AlphaFormat(Enum):
    ALPHA_NONE = 0,
    ALPHA_BINARY = 1,
    ALPHA_SMOOTH = 2,
    ALPHA_DEFAULT = 3

# miscellaneous
class VertMode(Enum):
    VERT_MODE_SRC_IGNORE = 0,
    VERT_MODE_SRC_EMISSIVE = 1,
    VERT_MODE_SRC_AMB_DIF = 2

class LightMode(Enum):
    LIGHT_MODE_EMISSIVE = 0,
    LIGHT_MODE_EMI_AMB_DIF = 1

class KeyType(Enum):
    LINEAR_KEY = 1,
    QUADRATIC_KEY = 2,
    TBC_KEY = 3,
    XYZ_ROTATION_KEY = 4,
    CONST_KEY = 5

class EffectType(Enum):
    EFFECT_PROJECTED_LIGHT = 0,
    EFFECT_PROJECTED_SHADOW = 1,
    EFFECT_ENVIRONMENT_MAP = 2,
    EFFECT_FOG_MAP = 3

class CoordGenType(Enum):
    CG_WORLD_PARALLEL = 0,
    CG_WORLD_PERSPECTIVE = 1,
    CG_SPHERE_MAP = 2,
    CG_SPECULAR_CUBE_MAP = 3,
    CG_DIFFUSE_CUBE_MAP = 4

class FieldType(Enum):
    FIELD_WIND = 0,
    FIELD_POINT = 1

class DecayType(Enum):
    DECAY_NONE = 0,
    DECAY_LINEAR = 1,
    DECAY_EXPONENTIAL = 2

#endregion

#region Records

# Refers to an object before the current one in the hierarchy.
class Ptr(Generic[T]):
    def __init__(self, r: Reader):
        self.value: int = r.readInt32()
        self.isNull: bool = self.value < 0

# Refers to an object after the current one in the hierarchy.
class Ref(Generic[T]):
    def __init__(self, r: Reader):
        self.value: int = r.readInt32()
        self.isNull: bool = self.value < 0

class BoundingBox:
    def __init__(self, r: Reader):
        self.unknownInt: int = r.readUInt32()
        self.translation: np.ndarray = r.readVector3() #Vector3
        self.rotation: np.ndarray = r.readRowMajorMatrix3x3() #Matrix3x3
        self.radius: np.ndarray = r.readVector3() #Vector3

class Color3:
    def __init__(self, r: Reader):
        self.r: float = r.readSingle()
        self.g: float = r.readSingle()
        self.b: float = r.readSingle()
    def toColor(self) -> Color: raise NotImplementedError() # return Color.fromArgb(r * 255.0, g * 255.0, b * 255.0)

class Color4:
    def __init__(self, r: Reader):
        self.r: float = r.readSingle()
        self.g: float = r.readSingle()
        self.b: float = r.readSingle()
        self.a: float = r.readSingle()

class TexDesc:
    def __init__(self, r: Reader):
        self.source: Ref[NiSourceTexture] = Ref[NiSourceTexture](r)
        self.clampMode: TexClampMode = r.readUInt32()
        self.filterMode: TexFilterMode = r.readUInt32()
        self.uvSet: int = r.readUInt32()
        self.ps2L: int = r.readInt16()
        self.ps2K: int = r.readInt16()
        self.unknown1: int = r.readUInt16()

class TexCoord:
    def __init__(self, r: Reader):
        self.u: float = r.readSingle()
        self.v: float = r.readSingle()

class Triangle:
    def __init__(self, r: Reader):
        self.v1: int = r.readUInt16()
        self.v2: int = r.readUInt16()
        self.v3: int = r.readUInt16()

class MatchGroup:
    def __init__(self, r: Reader):
        #self.numVertices: short
        self.vertexIndices: list[int] = r.readL16PArray('h')

class TBC:
    def __init__(self, r: Reader):
        self.t: float = r.readSingle()
        self.b: float = r.readSingle()
        self.c: float = r.readSingle()

class Key:
    def __init__(self, type: type, r: Reader, keyType: KeyType):
        self.time: float = r.readSingle()
        self.value: object = NiReaderUtils.read(type, r)
        if keyType == KeyType.QUADRATIC_KEY: self.forward: object = NiReaderUtils.read(type, r); self.backward: object = NiReaderUtils.read(type, r)
        elif keyType == KeyType.TBC_KEY: self.tbc: TBC = TBC(r)

class KeyGroup:
    def __init__(self, type: type, r: Reader):
        self.numKeys: int = r.readUInt32()
        if self.numKeys != 0: self.interpolation: KeyType = r.readUInt32()
        self.keys: list[Key] = r.readFArray(lambda r: Key(r, interpolation), self.numKeys)

class QuatKey:
    def __init__(self, type: type, r: Reader):
        self.time: float = r.readSingle()
        if keyType != KeyType.XYZ_ROTATION_KEY: self.value: object = NiReaderUtils.read(type, r)
        if keyType == KeyType.TBC_KEY: self.tbc: TBC = TBC(r)

class SkinData:
    def __init__(self, r: Reader):
        self.skinTransform: SkinTransform = SkinTransform(r)
        self.boundingSphereOffset: np.ndarray = r.readVector3() #Vector3
        self.boundingSphereRadius: float = r.readSingle()
        #self.numVertices: int
        self.vertexWeights: list[SkinWeight] = r.readL16FArray(lambda r: SkinWeight(r))

class SkinWeight:
    def __init__(self, r: Reader):
        self.index: int = r.readUInt16()
        self.weight: float = r.readSingle()

class SkinTransform:
    def __init__(self, r: Reader):
        self.rotation: np.ndarray = r.readRowMajorMatrix3x3() #Matrix4x4
        self.translation: np.ndarray = r.readVector3() #Vector3
        self.scale: float = r.readSingle()

class Particle:
    def __init__(self, r: Reader):
        self.velocity: np.ndarray = r.readVector3() #Vector3
        self.unknownVector: np.ndarray = r.readVector3() #Vector3
        self.lifetime: float = r.readSingle()
        self.lifespan: float = r.readSingle()
        self.timestamp: float = r.readSingle()
        self.unknownShort: int = r.readUInt16()
        self.vertexId: int = r.readUInt16()

class Morph:
    def __init__(self, r: Reader, numVertices: int):
        self.numKeys: int = r.readUInt32()
        self.interpolation: KeyType = r.readUInt32()
        self.keys: list[Key] = r.readFArray(lambda r: Key(r, interpolation), self.numKeys)
        self.vectors: list[np.ndarray] = r.readFArray(lambda r: r.readVector3(), numVertices) #Vector3[]

#endregion

#region Headers

class NiHeader:
    def __init__(self, r: Reader):
        self.str: bytes = r.readBytes(40) # 40 bytes (including \n)
        self.version: int = r.readUInt32()
        self.numBlocks: int = r.readUInt32()

class NiFooter:
    def __init__(self, r: Reader):
        #self.numRoots: int
        self.roots: list[int] = r.readL32PArray('i')

# These are the main units of data that NIF files are arranged in.
class NiObject:
    def __init__(self, r: Reader): pass

# An object that can be controlled by a controller.
class NiObjectNET(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.name: str = r.readL32Encoding()
        self.extraData: Ref[NiExtraData] = Ref[NiExtraData](r)
        self.controller: Ref[NiTimeController] = Ref[NiTimeController](r)

class NiAVObject(NiObjectNET):
    class NiFlags(Flag):
        Hidden = 0x1
    def __init__(self, r: Reader):
        super().__init__(r)
        self.flags: NiFlags = NiReaderUtils.readFlags(r)
        self.translation: np.ndarray = r.readVector3()  #Vector3
        self.rotation: np.ndarray = r.readMatrix3x3As4x4() #Matrix4x4
        self.scale: float = r.readSingle()
        self.velocity: np.ndarray = r.readVector3() #Vector3
        #self.numProperties: int
        self.properties: list[Ref[NiProperty]] = r.readL32FArray(lambda r: Ref[NiProperty](r))
        self.hasBoundingBox: bool = r.readBool32()
        if self.hasBoundingBox: self.boundingBox: BoundingBox = BoundingBox(r)


# Nodes
class NiNode(NiAVObject):
    def __init__(self, r: Reader):
        super().__init__(r)
        # self.numChildren: int
        self.children: list[Ref[NiAVObject]] = r.ReadL32FArray(lambda r: Ref[NiAVObject](r))
        #self.numEffects: int
        self.effects: list[Ref[NiDynamicEffect]] = r.ReadL32FArray(lambda r: Ref[NiDynamicEffect](r))
class RootCollisionNode(NiNode):
    def __init__(self, r: Reader): super().__init__(r)
class NiBSAnimationNode(NiNode):
    def __init__(self, r: Reader): super().__init__(r)
class NiBSParticleNode(NiNode):
    def __init__(self, r: Reader): super().__init__(r)
class NiBillboardNode(NiNode):
    def __init__(self, r: Reader): super().__init__(r)
class AvoidNode(NiNode):
    def __init__(self, r: Reader): super().__init__(r)

# Geometry
class NiGeometry(NiAVObject):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.data: Ref[NiGeometryData] = Ref[NiGeometryData](r)
        self.skinInstance: Ref[NiSkinInstance] = Ref[NiSkinInstance](r)

class NiGeometryData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.numVertices: int = r.readUInt16()
        self.hasVertices: bool = r.readBool32()
        if self.hasVertices: self.vertices: list[np.ndarray] = r.readFArray(lambda r: r.readVector3(), self.numVertices) #Vector3
        self.HasNormals: bool = r.readBool32()
        if self.hasNormals: self.normals: list[np.ndarray] = r.readFArray(lambda r: r.readVector3(), self.numVertices) #Vector3
        self.center: np.ndarray = r.readVector3() #Vector3
        self.radius: float = r.readSingle()
        self.hasVertexColors: bool = r.readBool32()
        if self.hasVertexColors: self.vertexColors: list[Color3] = r.readFArray(lambda r: Color4(r), self.numVertices)
        self.numUVSets: int = r.readUInt16()
        self.hasUV: bool = r.readBool32()
        if self.hasUV:
            self.uvSets: list[TexCoord] = TexCoord[self.numUVSets, self.numVertices]
            for i in range(self.numUVSets):
                for j in range(self.numVertices): self.uvSets[i, j] = TexCoord(r)

class NiTriBasedGeom(NiGeometry):
    def __init__(self, r: Reader): super().__init__(r)

class NiTriBasedGeomData(NiGeometryData):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.numTriangles: int = r.readUInt16()

class NiTriShape(NiTriBasedGeom):
    def __init__(self, r: Reader): super().__init__(r)

class NiTriShapeData(NiTriBasedGeomData):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.numTrianglePoints: int = r.readUInt32()
        self.triangles: list[Triangle] = r.readFArray(lambda r: Triangle(r), numTriangles)
        #self.numMatchGroups: int
        self.matchGroups: list[MatchGroup] = r.readL16FArray(lambda r: MatchGroup(r))

# Properties
class NiProperty(NiObjectNET):
    def __init__(self, r: Reader): super().__init__(r)

class NiTexturingProperty(NiProperty):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.flags: NiAVObject.NiFlags = NiReaderUtils.readFlags(r)
        self.applyMode: ApplyMode = r.readUInt32()
        self.textureCount: int = r.readUInt32()
        # self.hasBaseTexture: bool
        self.baseTexture: TexDesc = TexDesc(r) if r.readBool32() else None
        # self.hasDarkTexture: bool
        self.darkTexture: TexDesc = TexDesc(r) if r.readBool32() else None
        # self.hasDetailTexture: bool
        self.detailTexture: TexDesc = TexDesc(r) if r.readBool32() else None
        # self.hasGlossTexture: bool
        self.glossTexture: TexDesc = TexDesc(r) if r.readBool32() else None
        # self.hasGlowTexture: bool
        self.glowTexture: TexDesc = TexDesc(r) if r.readBool32() else None
        # self.hasBumpMapTexture: bool
        self.bumpMapTexture: TexDesc = TexDesc(r) if r.readBool32() else None
        # self.hasDecal0Texture: bool
        self.decal0Texture: TexDesc = TexDesc(r) if r.readBool32() else None

class NiAlphaProperty(NiProperty):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.flags: int = r.readUInt16()
        self.threshold: byte = r.readByte()

class NiZBufferProperty(NiProperty):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.flags: int = r.readUInt16()

class NiVertexColorProperty(NiProperty):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.flags: NiAVObject.NiFlags = NiReaderUtils.readFlags(r)
        self.vertexMode: VertMode = r.readUInt32()
        self.lightingMode: LightMode = r.readUInt32()

class NiShadeProperty(NiProperty):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.flags: NiAVObject.NiFlags = NiReaderUtils.readFlags(r)

class NiWireframeProperty(NiProperty):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.flags: NiAVObject.NiFlags = NiReaderUtils.readFlags(r)

class NiCamera(NiAVObject):
    def __init__(self, r: Reader): super().__init__(r)

# Data
class NiUVData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.uvGroups: KeyGroup = r.readFArray(lambda r: KeyGroup(float, r), 4)

class NiKeyframeData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.numRotationKeys: int = r.readUInt32()
        if self.numRotationKeys != 0:
            self.rotationType: KeyType = r.readUInt32()
            if self.rotationType != KeyType.XYZ_ROTATION_KEY:
                self.quaternionKeys: list[QuatKey] = r.readFArray(lambda r: QuatKey(Quaternion, r, rotationType), self.numRotationKeys)
            else:
                self.unknownFloat: float = r.readSingle()
                self.xyzRotations: list[KeyGroup] = r.readFArray(lambda r: KeyGroup(float, r), 3)
        self.translations: KeyGroup = KeyGroup(np.ndarray, r) #Vector3
        self.scales: KeyGroup = KeyGroup(float, r)

class NiColorData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.data: KeyGroup = KeyGroup(Color4, r)

class NiMorphData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.numMorphs: int = r.readUInt32()
        self.numVertices: int = r.readUInt32()
        self.relativeTargets: byte = r.readByte()
        self.morphs: list[Morph] = r.readFArray(lambda r: Morph(r, self.numVertices), self.numMorphs)

class NiVisData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r)
        #self.numKeys: int
        self.keys: list[Key] = r.readL32FArray(lambda r: Key(byte, r, KeyType.LINEAR_KEY))

class NiFloatData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.data: KeyGroup = KeyGroup(float, r)

class NiPosData(NiObject): 
    def __init__(self, r: Reader):
        super().__init__(r)
        self.data: KeyGroup = KeyGroup(np.ndarray, r)

class NiExtraData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.nextExtraData: Ref[NiExtraData] = Ref[NiExtraData](r)

class NiStringExtraData(NiExtraData):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.bytesRemaining: int = r.readUInt32()
        self.str: str = r.readL32Encoding()

class NiTextKeyExtraData(NiExtraData):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.unknownInt1: int = r.readUInt32()
        # self.numTextKeys: int
        self.textKeys: list[Key] = r.readL32FArray(lambda r: Key(string, r, KeyType.LINEAR_KEY))

class NiVertWeightsExtraData(NiExtraData):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.numBytes: int = r.readUInt32()
        self.numVertices: int = r.readUInt16()
        self.weights: list[float] = r.readPArray('f', self.numVertices)

# Controllers
class NiTimeController(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.nextController: Ref[NiTimeController] = Ref[NiTimeController](r)
        self.flags: int = r.readUInt16()
        self.frequency: float = r.readSingle()
        self.phase: float = r.readSingle()
        self.startTime: float = r.readSingle()
        self.stopTime: float = r.readSingle()
        self.target: Ptr[NiObjectNET] = Ptr[NiObjectNET](r)

class NiUVController(NiTimeController):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.unknownShort: int = r.readUInt16()
        self.data: Ref[NiUVData] = Ref[NiUVData](r)

class NiInterpController(NiTimeController):
    def __init__(self, r: Reader): super().__init__(r)

class NiSingleInterpController(NiInterpController):
    def __init__(self, r: Reader): super().__init__(r)

class NiKeyframeController(NiSingleInterpController):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.data: Ref[NiKeyframeData] = Ref[NiKeyframeData](r)

class NiGeomMorpherController(NiInterpController):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.data: Ref[NiMorphData] = Ref[NiMorphData](r)
        self.alwaysUpdate: byte = r.readByte()

class NiBoolInterpController(NiSingleInterpController):
    def __init__(self, r: Reader): super().__init__(r)

class NiVisController(NiBoolInterpController):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.data: Ref[NiVisData] = Ref[NiVisData](r)

class NiFloatInterpController(NiSingleInterpController):
    def __init__(self, r: Reader): super().__init__(r)

class NiAlphaController(NiFloatInterpController):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.data: Ref[NiFloatData] = Ref[NiFloatData](r)

# Particles
class NiParticles(NiGeometry):
    def __init__(self, r: Reader): super().__init__(r)
class NiParticlesData(NiGeometryData):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.numParticles: int = r.readUInt16()
        self.particleRadius: float = r.readSingle()
        self.numActive: int = r.readUInt16()
        self.hasSizes: bool = r.readBool32()
        if self.hasSizes: self.sizes: list[float] = r.readPArray('f', self.numVertices)

class NiRotatingParticles(NiParticles):
    def __init__(self, r: Reader): super().__init__(r)
class NiRotatingParticlesData(NiParticlesData):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.hasRotations: bool = r.readBool32()
        if self.hHasRotations: self.rotations: list[Quaternion] = r.readFArray(lambda r: r.readQuaternionWFirst(), self.numVertices)

class NiAutoNormalParticles(NiParticles):
    def __init__(self, r: Reader): super().__init__(r)
class NiAutoNormalParticlesData(NiParticlesData):
    def __init__(self, r: Reader): super().__init__(r)

class NiParticleSystemController(NiTimeController):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.speed: float = r.readSingle()
        self.speedRandom: float = r.readSingle()
        self.verticalDirection: float = r.readSingle()
        self.verticalAngle: float = r.readSingle()
        self.horizontalDirection: float = r.readSingle()
        self.horizontalAngle: float = r.readSingle()
        self.unknownNormal: np.ndarray = r.readVector3() #Vector3
        self.unknownColor: Color4 = Color4(r)
        self.size: float = r.readSingle()
        self.emitStartTime: float = r.readSingle()
        self.emitStopTime: float = r.readSingle()
        self.unknownByte: byte = r.readByte()
        self.emitRate: float = r.readSingle()
        self.lifetime: float = r.readSingle()
        self.lifetimeRandom: float = r.readSingle()
        self.emitFlags: int = r.readUInt16()
        self.startRandom: np.ndarray = r.readVector3() #Vector3
        self.emitter: Ptr[NiObject] = Ptr[NiObject](r)
        self.unknownShort2: int = r.readUInt16()
        self.unknownFloat13: float = r.readSingle()
        self.unknownInt1: int = r.readUInt32()
        self.unknownInt2: int = r.readUInt32()
        self.unknownShort3: int = r.readUInt16()
        self.numParticles: int = r.readUInt16()
        self.numValid: int = r.readUInt16()
        self.particles: list[Particle] = r.ReadFArray(lambda r: Particle(r), self.numParticles)
        self.unknownLink: Ref[NiObject] = Ref[NiObject](r)
        self.particleExtra: Ref[NiParticleModifier] = Ref[NiParticleModifier](r)
        self.unknownLink2: Ref[NiObject] = Ref[NiObject](r)
        self.trailer: byte = r.readByte()

class NiBSPArrayController(NiParticleSystemController):
    def __init__(self, r: Reader): super().__init__(r)

# Particle Modifiers
class NiParticleModifier(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.nextModifier: Ref[NiParticleModifier] = Ref[NiParticleModifier](r)
        self.controller: Ptr[NiParticleSystemController] = Ptr[NiParticleSystemController](r)

class NiGravity(NiParticleModifier):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.unknownFloat1: float = r.readSingle()
        self.force: float = r.readSingle()
        self.type: FieldType = r.readUInt32()
        self.position: np.ndarray = r.ReadVector3() #Vector3
        self.direction: np.ndarray = r.ReadVector3() #Vector3

class NiParticleBomb(NiParticleModifier):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.decay: float = r.readSingle()
        self.duration: float = r.readSingle()
        self.deltaV: float = r.readSingle()
        self.start: float = r.readSingle()
        self.decayType: DecayType = r.readUInt32()
        self.position: np.ndarray = r.readVector3() #Vector3
        self.direction: np.ndarray = r.readVector3() #Vector3

class NiParticleColorModifier(NiParticleModifier):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.colorData: Ref[NiColorData] = Ref[NiColorData](r)

class NiParticleGrowFade(NiParticleModifier):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.grow: float = r.readSingle()
        self.fade: float = r.readSingle()

class NiParticleMeshModifier(NiParticleModifier):
    def __init__(self, r: Reader):
        super().__init__(r)
        # self.numParticleMeshes: int
        self.particleMeshes: list[Ref[NiAVObject]] = r.readL32FArray(lambda r: Ref[NiAVObject](r))

class NiParticleRotation(NiParticleModifier):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.randomInitialAxis: byte = r.readByte()
        self.initialAxis: np.ndarray = r.readVector3() #Vector3
        self.rotationSpeed: float = r.readSingle()


# Skin Stuff
class NiSkinInstance(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.data: Ref[NiSkinData] = Ref[NiSkinData](r)
        self.skeletonRoot: Ptr[NiNode] = Ptr[NiNode](r)
        self.numBones: int = r.readUInt32()
        self.bones: list[Ptr[NiNode]] = r.readFArray(lambda r: Ptr[NiNode](r), self.numBones)

class NiSkinData(NiObject):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.skinTransform: SkinTransform = SkinTransform(r)
        self.numBones: int = r.readUInt32()
        self.skinPartition: Ref[NiSkinPartition] = Ref[NiSkinPartition](r)
        self.boneList: list[SkinData] = r.readFArray(lambda r: SkinData(r), self.numBones)

class NiSkinPartition(NiObject):
    def __init__(self, r: Reader): super().__init__(r)

# Miscellaneous
class NiTexture(NiObjectNET):
    def __init__(self, r: Reader): super().__init__(r)

class NiSourceTexture(NiTexture):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.useExternal: byte = r.readByte()
        self.fileName: str = r.readL32Encoding()
        self.pixelLayout: PixelLayout = r.readUInt32()
        self.useMipMaps: MipMapFormat = r.readUInt32()
        self.alphaFormat: AlphaFormat = r.readUInt32()
        self.isStatic: byte = r.readByte()

class NiPoint3InterpController(NiSingleInterpController):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.data: Ref[NiPosData] = Ref[NiPosData](r)

class NiMaterialProperty(NiProperty):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.flags: NiAVObject.NiFlags = NiReaderUtils.readFlags(r)
        self.ambientColor: Color3 = Color3(r)
        self.diffuseColor: Color3 = Color3(r)
        self.specularColor: Color3 = Color3(r)
        self.emissiveColor: Color3 = Color3(r)
        self.glossiness: float = r.readSingle()
        self.alpha: float = r.readSingle()

class NiMaterialColorController(NiPoint3InterpController):
    def __init__(self, r: Reader): super().__init__(r)

class NiDynamicEffect(NiAVObject):
    def __init__(self, r: Reader):
        super().__init__(r)
        # self.numAffectedNodeListPointers: int
        self.affectedNodeListPointers: list[int] = r.readL32PArray('I')

class NiTextureEffect(NiDynamicEffect):
    def __init__(self, r: Reader):
        super().__init__(r)
        self.modelProjectionMatrix: np.ndarray = r.readRowMajorMatrix3x3() #Matrix3x3
        self.modelProjectionTransform: np.ndarray = r.readVector3() #Vector3
        self.textureFiltering: TexFilterMode = r.ReadUInt32()
        self.textureClamping: TexClampMode = r.readUInt32()
        self.textureType: EffectType = r.readUInt32()
        self.coordinateGenerationType: CoordGenType = r.readUInt32()
        self.sourceTexture: Ref[NiSourceTexture] = Ref[NiSourceTexture](r)
        self.clippingPlane: byte = r.readByte()
        self.unknownVector: np.ndarray = r.readVector3() #Vector3
        self.unknownFloat: float = r.readSingle()
        self.ps2L: int = r.readInt16()
        self.ps2K: int = r.readInt16()
        self.unknownShort: int = r.readUInt16()

#endregion

class NiReaderUtils:
    @staticmethod
    def readFlags(r: Reader) -> NiAVObject.NiFlags: return r.readUInt16()

    @staticmethod
    def read(type: type, r: Reader) -> object:
        if type == float: return r.readSingle()
        elif type == byte: return r.readByte()
        elif type == str: return r.readL32Encoding()
        elif type == np.ndarray: return r.readVector3()
        elif type == Quaternion: return r.readQuaternionWFirst()
        elif type == Color4: return Color4(r)
        else: raise NotImplementedError('Tried to read an unsupported type.')

    @staticmethod
    def readNiObject(r: Reader) -> NiObject:
        nodeType: str = r.readL32AString()
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
            case _: print(f'Tried to read an unsupported NiObject type ({nodeType}).'); return None

