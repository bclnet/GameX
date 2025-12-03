import xml.etree.ElementTree as ET
from xmlCodeWriter import CS, PY, Class, Elem, XmlCodeWriter

class NifCodeWriter(XmlCodeWriter):
    def export(self, name: str): return name in self.es3 or name in self.es3x or name in self.es4
    def tags(self, name: str): return 'X' if name in self.es3 else 'Y' if name in self.es3x else 'Z' if name in self.es4 else ''
    def __init__(self, ex: str):
        super().__init__(ex)
        #region Header
        # EffectType->TextureType, NiHeader->NiReader, NiFooter->NiReader, SkinData->BoneData, SkinWeight->BoneVertData, BoundingBox->BoxBV, SkinTransform->NiTransform, NiCameraProperty->NiCamera
        self.es3 = [
            'ApplyMode', 'TexClampMode', 'TexFilterMode', 'PixelLayout', 'MipMapFormat', 'AlphaFormat', 'VertMode', 'LightMode', 'KeyType', 'TextureType', 'CoordGenType', 'FieldType', 'DecayType', 'BoundVolumeType', 'TransformMethod', 'EndianType',
            'BoxBV', 'BoundingVolume', 'Color3', 'Color4', 'TexDesc', 'TexCoord', 'Triangle', 'MatchGroup', 'TBC', 'Key', 'KeyGroup', 'QuatKey', 'BoneData', 'BoneVertData', 'NiTransform', 'Particle', 'Morph', 'ExportInfo', 'Header', 'Footer',
            'NiObject', 'NiObjectNET', 'NiAVObject', 'NiNode', 'RootCollisionNode', 'NiBSAnimationNode', 'NiBSParticleNode', 'NiBillboardNode', 'AvoidNode', 'NiGeometry', 'NiGeometryData', 'NiTriBasedGeom', 'NiTriBasedGeomData', 'NiTriShape', 'NiTriShapeData',
            'NiProperty', 'NiTexturingProperty', 'NiAlphaProperty', 'NiZBufferProperty', 'NiVertexColorProperty', 'NiShadeProperty', 'NiWireframeProperty', 'NiCamera', 'NiUVData', 'NiKeyframeData', 'NiColorData', 'NiMorphData', 'NiVisData', 'NiFloatData', 'NiPosData',
            'NiExtraData', 'NiStringExtraData', 'NiTextKeyExtraData', 'NiVertWeightsExtraData', 'NiParticles', 'NiParticlesData', 'NiRotatingParticles', 'NiRotatingParticlesData', 'NiAutoNormalParticles', 'NiAutoNormalParticlesData', 'NiParticleSystemController', 'NiBSPArrayController',
            'NiParticleModifier', 'NiGravity', 'NiParticleBomb', 'NiParticleColorModifier', 'NiParticleGrowFade', 'NiParticleMeshModifier', 'NiParticleRotation', 'NiTimeController', 'NiUVController', 'NiInterpController', 'NiSingleInterpController', 'NiKeyframeController', 'NiGeomMorpherController', 'NiBoolInterpController', 'NiVisController', 'NiFloatInterpController', 'NiAlphaController',
            'NiSkinInstance', 'NiSkinData', 'NiSkinPartition', 'NiTexture', 'NiSourceTexture', 'NiPoint3InterpController', 'NiMaterialProperty', 'NiMaterialColorController', 'NiDynamicEffect', 'NiTextureEffect', 'NiPathController', 'PathFlags', 'NiPixelData', 'NiPalette', 'MipMap' ]
        self.es3x = [
            'MaterialColor', 'BSLightingShaderPropertyShaderType', 'BSShaderType', 'BSShaderFlags', 'BSShaderFlags2', 'BillboardMode', 'SymmetryType', 'VertexFlags', 'ZCompareMode', 'ImageType', 'NiPixelFormat', 'PixelFormat', 'PixelComponent', 'PixelRepresentation', 'PixelTiling', 'PixelFormatComponent',
            'MaterialData', 'BSVertexDesc', 'MorphWeight', 'BSShaderProperty', 'VectorFlags', 'BSVectorFlags', 'ConsistencyType', 'AbstractAdditionalGeometryData', 'FormatPrefs', 'NiRawImageData', 'SkinPartition', 'BSVertexDataSSE',
            'NiPlane', 'NiImage', 'NiBound', 'CapsuleBV', 'UnionBV', 'HalfSpaceBV', 'ShaderTexDesc',
            'NiCollisionObject', 'NiInterpolator' ]
        self.es4 = [
            'NiBinaryExtraData', 'NiTriStrips', 'NiTriStripsData', 'NiIntegerExtraData', 'BSXFlags',
            'OblivionLayer', 'OblivionHavokMaterial', 'Fallout3Layer', 'Fallout3HavokMaterial', 'SkyrimLayer', 'SkyrimHavokMaterial', 'HavokMaterial', 'HavokFilter', 'MoppDataBuildType',
            'bhkRefObject', 'bhkSerializable', 'bhkShape', 'bhkShapeCollection', 'bhkNiTriStripsShape', 'bhkBvTreeShape', 'bhkMoppBvTreeShape',
            'hkResponseType', 'hkMotionType', 'hkDeactivatorType', 'hkSolverDeactivation', 'hkQualityType', 'BroadPhaseType', 'hkWorldObjCinfoProperty', 'bhkWorldObject', 'bhkEntity', 'bhkRigidBody', 'bhkCOFlags', 'bhkNiCollisionObject', 'bhkCollisionObject',
            'bhkRigidBodyT', 'bhkSphereRepShape', 'bhkConvexShape', 'bhkConvexVerticesShape', 'bhkListShape', 'AnimationType', 'FurnitureEntryPoints', 'FurniturePosition', 'BSFurnitureMarker', 'bhkBoxShape', 'bhkTransformShape', 'bhkConvexTransformShape',
            'NiSpecularProperty', 'AVObject', 'NiAVObjectPalette', 'NiDefaultAVObjectPalette', 'StringPalette', 'NiStringPalette', 'InterpBlendFlags', 'InterpBlendItem', 'NiBlendInterpolator', 'ControlledBlock', 'NiControllerManager',
            'AnimNoteType', 'BSAnimNote', 'BSAnimNotes', 'CycleType', 'AccumFlags', 'NiSequence', 'NiControllerSequence', 'BSVertexData', 'NiMultiTargetTransformController', 'NiKeyBasedInterpolator', 'NiQuatTransform', 'NiTransformData', 'NiTransformInterpolator',
            'NiStencilProperty', 'StencilCompareMode', 'StencilAction', 'StencilDrawMode' ]
        # nodes
        self.nodes = ['NiNode', 'NiTriShape', 'NiTexturingProperty', 'NiSourceTexture', 'NiMaterialProperty', 'NiMaterialColorController', 'NiTriShapeData', 'RootCollisionNode', 'NiStringExtraData', 'NiSkinInstance', 'NiSkinData', 'NiAlphaProperty', 'NiZBufferProperty', 'NiVertexColorProperty', 'NiBSAnimationNode', 'NiBSParticleNode', 'NiParticles', 'NiParticlesData', 'NiRotatingParticles', 'NiRotatingParticlesData', 'NiAutoNormalParticles', 'NiAutoNormalParticlesData', 'NiUVController', 'NiUVData', 'NiTextureEffect', 'NiTextKeyExtraData', 'NiVertWeightsExtraData', 'NiParticleSystemController', 'NiBSPArrayController', 'NiGravity', 'NiParticleBomb', 'NiParticleColorModifier', 'NiParticleGrowFade', 'NiParticleMeshModifier', 'NiParticleRotation', 'NiKeyframeController', 'NiKeyframeData', 'NiColorData', 'NiGeomMorpherController', 'NiMorphData', 'AvoidNode', 'NiVisController', 'NiVisData', 'NiAlphaController', 'NiFloatData', 'NiPosData', 'NiBillboardNode', 'NiShadeProperty', 'NiWireframeProperty', 'NiCamera', 'NiPathController', 'NiPixelData',
            #, 'NiExtraData', 'NiSkinPartition']
            #es4
            'NiBinaryExtraData', 'NiTriStrips', 'NiTriStripsData', 'BSXFlags', 'bhkNiTriStripsShape', 'bhkMoppBvTreeShape', 'bhkRigidBody', 'bhkCollisionObject', 'bhkRigidBodyT', 'bhkConvexVerticesShape', 'bhkListShape', 'BSFurnitureMarker', 'bhkBoxShape', 'bhkConvexTransformShape',
            'NiSpecularProperty', 'NiControllerSequence', 'NiControllerManager', 'NiMultiTargetTransformController', 'NiTransformInterpolator', 'NiTransformData', 'NiStringPalette', 'NiDefaultAVObjectPalette', 'NiStencilProperty' ]
        self.customs = {
            '_header': (
'''using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using static OpenStack;
#pragma warning disable CS9113, CS0108

namespace GameX.Gamebryo.Formats.Nif;

''',
'''import os
from io import BytesIO
from enum import Enum, Flag, IntFlag
from numpy import ndarray, array
from openstk import log, Reader
from gamex import FileSource, ArcBinaryT, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo
from gamex.core.globalx import Color3, Color4
from gamex.core.desser import DesSer

# types
type byte = int
type Vector3 = ndarray
type Vector4 = ndarray
type Matrix2x2 = ndarray
type Matrix3x4 = ndarray
type Matrix4x4 = ndarray
type Quaternion = ndarray

# typedefs
class Color: pass
class UnionBV: pass
class NiReader: pass
class NiObject: pass

'''),
            'X': (
'''public class Ref<T>(NiReader r, int v) where T : NiObject { public int v = v; T val; [JsonIgnore] public T Value => val ??= (T)r.Blocks[v]; }

static class X<T> where T : NiObject {
    public static Ref<T> Ptr(BinaryReader r) { int v; return (v = r.ReadInt32()) < 0 ? null : new Ref<T>((NiReader)r, v); }
    public static Ref<T> Ref(BinaryReader r) { int v; return (v = r.ReadInt32()) < 0 ? null : new Ref<T>((NiReader)r, v); }
}

static class Z {
    static Dictionary<uint, string> BlockHashes = new();
    public static NiObject[] ReadBlocks(NiReader r) {
        var v = r.V; var pos = r.Tell();
        var Blocks = new NiObject[r.NumBlocks];
        if (v >= 0x0303000d) {
            // block types are stored in the header for versions above 10.x.x.x
            if (v >= 0x0a000000) {
                var hasSize = v >= 0x14020000 && true; //ignoreSize
                for (var i = 0; i < r.NumBlocks; i++) {
                    if (r.AtEnd()) throw new Exception("unexpected EOF during load");
                    var size = uint.MaxValue;
                    var index = r.BlockTypeIndex[i] & 0x7FFF; // the upper bit or the blocktypeindex seems to be related to PhysX
                    var type = r.BlockTypes[index];
                    // 20.3.1.2 Custom Version
                    if (v == 0x14030102) {
                        var hash = r.BlockTypeHashes[index];
                        if (BlockHashes.ContainsKey(hash)) type = BlockHashes[hash];
                        else throw new Exception("Block Hash not found.");
                    }
                    // note: some 10.0.1.0 version nifs from Oblivion in certain distributions seem to be missing
                    // these four bytes on the havok blocks (see for instance meshes/architecture/basementsections/ungrdltraphingedoor.nif)
                    if (v < 0x0a020000 && !type.StartsWith("bhk")) {
                        var dummy = r.ReadUInt32();
                        if (dummy != 0) { var msg = $"non-zero block separator ({dummy}) preceeding block {type}"; Console.WriteLine(msg); }
                    }
                    // for version 20.2.0.? and above the block size is stored in the header
                    if (hasSize) size = r.BlockSize[index];
                    Blocks[i] = NiObject.Read(r, type);
                }
                return Blocks;
            }
            // < 0x05000001
            for (var i = 0; i < r.NumBlocks; i++) Blocks[i] = NiObject.Read(r, r.ReadL32AString());
            return Blocks;
        }
        // < 0x0303000d
        for (var i = 0; ; i++) {
            if (r.AtEnd()) throw new Exception("unexpected EOF during load");
            var type = r.ReadL32AString(80);
            if (type == "End Of File") break;
            else if (type == "Top Level Object") {
                type = r.ReadL32AString(80);
                var p = r.ReadInt32() - 1;
                //if (p != i) linkMap.insert(p, i);
                //if (isNiBlock(blockType)) {
                //    //log.info($"loading block {c}:{blockType}");
                //    insertNiBlock(blockType, -1);
                //    if (!loadItem(root->child(c + 1), stream)) throw Exception($"failed to load block number {i} ({blockType}) previous block was {root->child(c)->name()}");
                //}
                //else throw Exception($"encountered unknown block ({blockType})");
            }
        }
        return Blocks;
    }
    public static string ExtractRTTIArgs(NiReader r, string nodeType) {
        var nameAndArgs = nodeType.Split("\\x01");
        //if (nameAndArgs[0] == "NiDataStream") {
        //    metadata.usage = NiMesh::DataStreamUsage(nameAndArgs[1].toInt());
        //    metadata.access = NiMesh::DataStreamAccess(nameAndArgs[2].toInt());
        //}
        return nameAndArgs[0];
    }
    public static T Read<T>(NiReader r) {
        if (typeof(T) == typeof(float)) { return (T)(object)r.ReadSingle(); }
        else if (typeof(T) == typeof(byte)) { return (T)(object)r.ReadByte(); }
        else if (typeof(T) == typeof(string)) { return (T)(object)r.ReadL32AString(); }
        else if (typeof(T) == typeof(Vector3)) { return (T)(object)r.ReadVector3(); }
        else if (typeof(T) == typeof(Quaternion)) { return (T)(object)r.ReadQuaternionWFirst(); }
        else if (typeof(T) == typeof(Color4)) { return (T)(object)new Color4(r); }
        else throw new NotImplementedException($"Tried to read an unsupported type: {typeof(T)}");
    }
    public static byte ReadBool8(NiReader r) => r.V > 0x04000002 ? r.ReadByte() : (byte)r.ReadUInt32();
    public static bool ReadBool(NiReader r) => r.V > 0x04000002 ? r.ReadByte() != 0 : r.ReadUInt32() != 0;
    public static string String(NiReader r) => r.V < 0x14010003 ? r.ReadL32AString() : throw new Exception("HERE");
    public static string StringRef(NiReader r, int? p) => default;
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
    public override void Write(Utf8JsonWriter w, TexCoord s, JsonSerializerOptions options) => w.WriteStringValue($"{s.u:g9} {s.v:g9}");
}

public class TriangleJsonConverter : JsonConverter<Triangle> {
    public override Triangle Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Triangle s, JsonSerializerOptions options) => w.WriteStringValue($"{s.v1} {s.v2} {s.v3}");
}
''',
'''class Ref[T]:
    def __init__(self, r: NiReader, v: int): self.v: int = v; self.val: T = None
    def value() -> T: return self.val
class X[T]:
    @staticmethod # Refers to an object before the current one in the hierarchy.
    def ptr(r: Reader): return None if (v := r.readInt32()) < 0 else Ref(r, v)
    @staticmethod # Refers to an object after the current one in the hierarchy.
    def ref(r: Reader): return None if (v := r.readInt32()) < 0 else Ref(r, v)
class Z:
    @staticmethod
    def readBlocks(s, r: NiReader) -> list[object]:
        pass
    @staticmethod
    def read(s, r: NiReader) -> object:
        match s.t:
            case '[float]': return r.readSingle()
            case '[byte]': return r.readByte()
            case '[str]': return r.readL32AString()
            case '[Vector3]': return r.readVector3()
            case '[Quaternion]': return r.readQuaternionWFirst()
            case '[Color4]': return Color4(r)
            case _: raise NotImplementedError(f'Tried to read an unsupported type: {s.t}')
    @staticmethod
    def readBool8(r: NiReader) -> int: r.readByte() if r.v > 0x04000002 else r.readUInt32()
    @staticmethod
    def readBool(r: NiReader) -> bool: r.readByte() != 0 if r.v > 0x04000002 else r.readUInt32() != 0
    @staticmethod
    def string(r: NiReader) -> str: return r.readL32AString() if r.v < 0x14010003 else None
    @staticmethod
    def stringRef(r: NiReader, p: int) -> str: return None
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

class Flags(IntFlag):
    Hidden = 0x1

def RefJsonConverter(s): return f'{s.v}'
def TexCoordJsonConverter(s): return f'{s.u:.9g} {s.v:.9g}'
def TriangleJsonConverter(s): return f'{s.v1} {s.v2} {s.v3}'
DesSer.add({'Ref':RefJsonConverter, 'TexCoord':TexCoordJsonConverter, 'Triangle':TriangleJsonConverter})
''') }
        #endregion
        #region Compounds
        self.struct = {
            'BoneVertData': ('<Hf', 6, 'r'),
            'TBC': ('<3f', 12, 'r'),
            'TexCoord': ('<2f', 8, 'r'),
            'Triangle': ('<3H', 6, 'r'),
            'BSVertexDesc': ('<5bHb', 8, '(r[0],r[1],r[2],r[3],r[4],VertexFlags(r[5]),r[6])'),
            'NiPlane': ('<4f', 16, '(array(r[0:3]),r[3])'),
            'NiBound': ('<4f', 16, '(array(r[0:3]),r[3])'),
            'CapsuleBV': ('<8f', 32, '(array(r[0:3]),array(r[3:6]),r[6],r[7])'),
            'MipMap': ('<3i', 12, 'r'),
            # 'NiQuatTransform': 'x',
            # 'NiTransform': 'x',
            # 'BoxBV': 'x',
            'HalfSpaceBV': ('<7f', 28, '(NiPlane(r[0:4]),array(r[4:7]))'),
            'Particle': ('<9f2H', 40, '(array(r[0:3]),array(r[3:6]),r[6],r[7],r[8],r[9],r[10])') }
        def BoneVertData_values(s, values):
            values[1].kind = '?:'; values[1].cond = 'full'; values[1].elsecw = ('r.ReadHalf()', 'r.readHalf()')
        def ControlledBlock_values(s, values):
            values.insert(1, Class.Comment(s, 'NiControllerSequence::InterpArrayItem'))
            values.insert(6, Class.Comment(s, 'Bethesda-only'))
            values.insert(8, Class.Comment(s, 'NiControllerSequence::IDTag, post-10.1.0.104 only'))
        def ControlledBlock_if(s, ifx):
            if s.name == 'Interpolator ID Offset':
                ifx.kind = 'elif'
                ifx.inits[0].name = ''; ifx.inits[0].initcw = ('var stringPalette = X<NiStringPalette>.Ref(r)', 'stringPalette = X[NiStringPalette].ref(r)')
                ifx.inits[1].name = ''; ifx.inits[1].initcw = ('NodeName = Y.StringRef(r, stringPalette)', 'self.nodeName = Y.stringRef(r, stringPalette)')
                ifx.inits[2].name = ''; ifx.inits[2].initcw = ('PropertyType = Y.StringRef(r, stringPalette)', 'self.propertyType = Y.stringRef(r, stringPalette)')
                ifx.inits[3].name = ''; ifx.inits[3].initcw = ('ControllerType = Y.StringRef(r, stringPalette)', 'self.controllerType = Y.stringRef(r, stringPalette)')
                ifx.inits[4].name = ''; ifx.inits[4].initcw = ('ControllerID = Y.StringRef(r, stringPalette)', 'self.controllerID = Y.stringRef(r, stringPalette)')
                ifx.inits[5].name = ''; ifx.inits[5].initcw = ('InterpolatorID = Y.StringRef(r, stringPalette)', 'self.interpolatorID = Y.stringRef(r, stringPalette)')
            elif s.name == 'Interpolator ID' and '&&' not in ifx.vercond: ifx.kind = 'elif'
        def Header_values(s, values):
            values[2].namecw = ('V', 'v')
            values[4].namecw = ('UV', 'uv'); values[4].default = '0' if self.ex == PY else None
            values[6].namecw = ('UV2', 'uv2')
            del values[10]
            values[10].arr1 = values[11].arr1 = 'L16'
            # read blocks
            values.insert(18, Class.Comment(s, 'read blocks'))
            values.insert(19, vx0 := Class.Value(s, Elem({ 'name': 'Blocks', 'type': 'NiObject', 'arr1': 'x' })))
            vx0.initcw = ('Blocks = Z.ReadBlocks(r);', 'self.blocks: list[NiObject] = [None]*self.numBlocks')
            values.insert(20, vx1 := Class.Value(s, Elem({ 'name': 'Roots', 'type': 'Ref', 'template': 'NiObject', 'arr1': 'x' })))
            vx1.initcw = ('Roots = new Footer(r).Roots;', 'self.roots = Footer(r).roots')
        def Header_code(s):
            s.init = (['BinaryReader b', 'b: Reader'], ['rx', 'rx'], [f'new NiReader(r)', f'NiReader(r)'])
            s.namecw = ('NiReader', 'NiReader'); s.inherit = 'BinaryReader' if self.ex == CS else 'Reader'
            s.values[0].initcw = ('(HeaderString, V) = Z.ParseHeaderStr(b.ReadVAString(0x80, 0xA)); var r = this;', '(self.headerString, self.v) = Z.parseHeaderStr(b.readVAString(128, b\'\\x0A\')); r = self')
        def StringPalette_code(s):
            s.values[0].typecw = ('string[]', 'list[str]'); s.values[0].initcw = ('Palette = r.ReadL32AString().Split(\'\\x00\');', 'self.palette: list[str] = r.readL32AString().split(\'0x00\')')
        def TexDesc_values(s, values):
            values.insert(10, Class.Comment(s, 'NiTextureTransform'))
        def BSVertexData_values(s, values):
            del values[0:3]
        def BSVertexData_inits(s, inits):
            inits.insert(0, Class.Code(s, ('var full = sse || arg.HasFlag(VertexFlags.Full_Precision);', 'full = sse or VertexFlags.Full_Precision in arg')))
            inits.insert(1, Class.Code(s, ('var tangents = arg.HasFlag(VertexFlags.Tangents);', 'tangents = VertexFlags.Tangents in arg')))
            #
            inits.insert(2, ifx := Class.If(s, None, inits[2], (inits, 2, 5), 'if'))
            ifx.cond = ifx.cond[1:16]
            ifx.inits[0].kind = '?:'; ifx.inits[0].cond = ifx.inits[0].cond[22:-1]; ifx.inits[0].elsecw = ('r.ReadHalfVector3()', 'r.readHalfVector3()')
            ifx.inits.insert(1, ifx0 := Class.If(s, None, ifx.inits[1], (ifx.inits, 1, 2), 'if'))
            ifx.inits.insert(2, ifx1 := Class.If(s, None, ifx.inits[1], (ifx.inits, 2, 3), 'if'))
            ifx0.cond = ifx0.cond[22:38]
            ifx0.inits[0].kind = '?:'; ifx0.inits[0].cond = ifx0.inits[0].cond[44:-1]; ifx0.inits[0].elsecw = ('r.ReadHalf()', 'r.readHalf()')
            ifx1.cond = ifx1.cond[22:38]; ifx1.kind = 'else'
            ifx1.inits[0].kind = '?:'; ifx1.inits[0].cond = ifx1.inits[0].cond[44:-1]; ifx1.inits[0].elsecw = ('r.ReadUInt16()', 'r.readUInt16()')
            #
            inits.insert(6, ifx := Class.If(s, None, inits[6], (inits, 6, 8), 'if'))
            ifx.cond = ifx.cond[1:17]
            ifx.inits[0].cond = ifx.inits[0].cond[23:-1]
            ifx.inits[1].cond = ifx.inits[1].cond[23:-1]
        def SkinPartition_values(s, values):
            for i in [14, 6]: del values[i]
        def SkinPartition_inits(s, inits):
            inits.insert(6, ifx := Class.If(s, None, inits[0], (inits, 6, 17), 'elif'))
            ifx.vercond = 'ZV >= 0x0A010000'
            in0 = []
            for i in [9, 7, 3, 0]: ifx.inits[i].vercond = None; in0.insert(0, ifx.inits[i]); del ifx.inits[i]
            in0.insert(2, ifx.inits[4])
            ifx.inits[0].vercond = None; ifx.inits[0].cond = 'B32:' + ifx.inits[0].cond
            ifx.inits[1].vercond = None; ifx.inits[1].type = 'bool8'; ifx.inits[1].kind = 'var?'
            ifx.inits[2].vercond = None
            ifx.inits[3].vercond = None
            ifx.inits[5].vercond = None; ifx.inits[5].cond = ifx.inits[5].cond[16:-1]; ifx.inits[5].arr2 += '[i]'
            ifx.inits[6].vercond = None; ifx.inits[6].cond = ifx.inits[6].cond[16:-1]
            ifx.inits.insert(5, ifx := Class.If(s, None, None, (ifx.inits, 5, 7), 'if'))
            ifx.vercond = 'B32:HasFaces'
            #
            inits.insert(6, ifx := Class.If(s, None, None, in0, 'if'))
            ifx.inits[3].arr2 += '[i]'
            ifx.vercond = 'ZV <= 0x0A000102'
        def Morph_values(s, values):
            values[1].kind = 'var?'
        def BoneData_values(s, values):
            del values[4]
            values[6].kind = ':'; values[5].kind = ':+'; values[4].kind = '?+'
            values[6].arr1 = values[5].arr1 = values[4].arr1 = 'L16'
            values[6].type = 'BoneVertData'; values[6].namecw = ('VertexWeights', 'vertexWeights')
            values[6].initcw = ('    : r.V >= 0x14030101 && arg == 15 ? r.ReadL16FArray(z => new BoneVertData(r, false)) : default;', '    r.readL16FArray(lambda z: BoneVertData(r, False)) if r.V >= 0x14030101 and arg == 15 else None')
        def MotorDescriptor_inits(s, inits):
            inits.insert(1, Class.If(s, None, None, (inits, 1, 4), 'switch'))
        def RagdollDescriptor_inits(s, inits):
            inits.insert(0, Class.Comment(s, 'Oblivion and Fallout 3, Havok 550'))
            inits.insert(7, Class.Comment(s, 'Fallout 3 and later, Havok 660 and 2010'))
        def LimitedHingeDescriptor_inits(s, inits):
            inits.insert(0, Class.Comment(s, 'Oblivion and Fallout 3, Havok 550'))
            inits.insert(8, Class.Comment(s, 'Fallout 3 and later, Havok 660 and 2010'))
        def HingeDescriptor_inits(s, inits):
            inits.insert(0, Class.Comment(s, 'Oblivion'))
            inits.insert(6, Class.Comment(s, 'Fallout 3'))
        def PrismaticDescriptor_inits(s, inits):
            inits.insert(0, Class.Comment(s, 'In reality Havok loads these as Transform A and Transform B using hkTransform'))
            inits.insert(1, Class.Comment(s, 'Oblivion (Order is a guess)'))
            inits.insert(10, Class.Comment(s, 'Fallout 3'))
        def BoxBV_values(s, values):
            values[1].type = 'Matrix33R'; values[1].arr1 = None
        def BoundingVolume_inits(s, inits):
            inits.insert(1, Class.If(s, None, None, (inits, 1, 6), 'switch'))
        def MalleableDescriptor_inits(s, inits):
            inits.insert(5, Class.If(s, None, None, (inits, 5, 11), 'switch'))
            inits[6].comment = 'not in Fallout 3 or Skyrim'
            inits[7].comment = 'In TES CS described as Damping'
            inits[8].comment = 'In GECK and Creation Kit described as Strength'
        def ConstraintData_inits(s, inits):
            inits.insert(5, Class.If(s, None, None, (inits, 5, 12), 'switch'))
        self.customs = self.customs | {
            'BoneVertData': { 'x': 1421,
                'constPre': (', bool full', ''), 'constArg': ('', ', half: bool = None'),
                'consts': [(None, 'if half: self.index = r.readUInt16(); self.weight = r.readHalf(); return')],
                'values': BoneVertData_values },
            'BoneVertDataHalf': { 'x': 1427,
                '_': ['', ('BoneVertData', 'BoneVertData'), lambda x: (x, x), ('new BoneVertData(r, true)', 'BoneVertData(r, true)'), lambda c: (f'r.ReadFArray(z => new BoneVertData(r, true), {c})', f'r.readFArray(lambda z: BoneVertData(r, true), {c})')] },
            'ControlledBlock': { 'x': 1439,
                'values': ControlledBlock_values,
                'if': ControlledBlock_if },
            'Header': { 'x': 1482,
                'constBase': ('b.BaseStream', ''),
                'consts': [('static NiReader() => Z.Register();', None)],
                'values': Header_values,
                'code': Header_code },
            'StringPalette': { 'x': 1508,
                'code': StringPalette_code },
            'Key': { 'x': 1521,
                'kind': {-1: 'elif'},
                'constArg': (', KeyType keyType', ', keyType: KeyType'), 'constNew': (', ARG', ', ARG'),
                'cond': lambda p, s, cw: cw.typeReplace('KeyType', s).replace('ARG', 'keyType') },
            'QuatKey': { 'x': 1537,
                'constArg': (', KeyType keyType', ', keyType: KeyType'), 'constNew': (', ARG', ', ARG'),
                'cond': lambda p, s, cw: cw.typeReplace('KeyType', s).replace('ARG', 'keyType') },
            'TexCoord': { 'x': 1545,
                'flags': 'C',
                'constArg': ('', ', half: bool = None'), 'constNew': ('', ', False'),
                'consts': [
                    ('public TexCoord(double u, double v) { this.u = (float)u; this.v = (float)v; }', 'if isinstance(r, float): self.u = r; self.v = half; return'),
                    ('public TexCoord(NiReader r, bool half) { u = half ? r.ReadHalf() : r.ReadSingle(); v = half ? r.ReadHalf() : r.ReadSingle(); }', 'elif half: self.u = r.readHalf(); self.v = r.readHalf(); return')] },
            'HalfTexCoord': { 'x': 1551,
                '_': ['', ('TexCoord', 'TexCoord'), lambda x: (x, x), ('new TexCoord(r, true)', 'TexCoord(r, true)'), lambda c: (f'r.ReadFArray(z => new TexCoord(r, true), {c})', f'r.readFArray(lambda z: TexCoord(r, true), {c})')] },
            'TexDesc': { 'x': 1565,
                'values': TexDesc_values },
            'BSVertexData': { 'x': 1616,
                'constArg': (', VertexFlags arg, bool sse', ', arg: VertexFlags, sse: bool'),
                'condcs': lambda p, s, cw: s \
                    .replace('(ARG & 16) != 0', 'arg.HasFlag(VertexFlags.Vertex)') \
                    .replace('(ARG & 32) != 0', 'arg.HasFlag(VertexFlags.UVs)') \
                    .replace('(ARG & 128) != 0', 'arg.HasFlag(VertexFlags.Normals)') \
                    .replace('(ARG & 256) != 0', 'tangents').replace('(ARG & 256) == 0', '!tangents') \
                    .replace('(ARG & 512) != 0', 'arg.HasFlag(VertexFlags.Vertex_Colors)') \
                    .replace('(ARG & 1024) != 0', 'arg.HasFlag(VertexFlags.Skinned)') \
                    .replace('(ARG & 4096) != 0', 'arg.HasFlag(VertexFlags.Eye_Data)') \
                    .replace('(ARG & 16384) != 0', 'full').replace('(ARG & 16384) == 0', '!full'),
                'condpy': lambda p, s, cw: s \
                    .replace('(ARG & 16) != 0', 'arg.HasFlag(VertexFlags.Vertex)') \
                    .replace('(ARG & 32) != 0', 'arg.HasFlag(VertexFlags.UVs)') \
                    .replace('(ARG & 128) != 0', 'arg.HasFlag(VertexFlags.Normals)') \
                    .replace('(ARG & 256) != 0', 'tangents').replace('(ARG & 256) == 0', '!tangents') \
                    .replace('(ARG & 512) != 0', 'arg.HasFlag(VertexFlags.Vertex_Colors)') \
                    .replace('(ARG & 1024) != 0', 'arg.HasFlag(VertexFlags.Skinned)') \
                    .replace('(ARG & 4096) != 0', 'arg.HasFlag(VertexFlags.Eye_Data)') \
                    .replace('(ARG & 16384) != 0', 'full').replace('(ARG & 16384) == 0', '!full'),
                'values': BSVertexData_values, 'inits': BSVertexData_inits },
            'BSVertexDataSSE': { 'x': 1636,
                'constArg': (', uint ARG', ', ARG: int'), 'constNew': (', ARG', ', ARG'),
                '_': ['', ('BSVertexData', 'BSVertexData'), lambda x: (x, x), ('new BSVertexData(r, ARG, true)', 'BSVertexData(r, ARG, true)'), lambda c: (f'r.ReadFArray(z => new BSVertexData(r, ARG, true), {c})', f'r.readFArray(lambda z: BSVertexData(r, ARG, true), {c})')] },
            'SkinPartition': { 'x': 1661,
                'calculated': lambda s: ('(ushort)(NumVertices / 3)', '(self.numVertices / 3)'),
                'values': SkinPartition_values, 'inits': SkinPartition_inits },
            'NiTransform': { 'x': 1728,
                'types': {'Rotation': 'Matrix33R'} },
            'FurniturePosition': { 'x': 1750,
                'kind': {-1: 'else'} }, # should auto
            'Morph': { 'x': 1768,
                'constArg': (', uint numVertices', ', numVertices: int'), 'constNew': (', ARG', ', ARG'),
                'cond': lambda p, s, cw: s.replace('ARG', 'numVertices'),
                'values': Morph_values },
            'BoneData': { 'x': 1789,
                'constArg': (', int arg', ', arg: int'), 'constNew': (', ARG', ', ARG'),
                'cond': lambda p, s, cw: s.replace('ARG', 'arg'),
                'values': BoneData_values },
            'MotorDescriptor': { 'x': 1898,
                'flags': 'C',
                'cond': lambda p, s, cw: cw.typeReplace('MotorType', s),
                'inits': MotorDescriptor_inits },
            'RagdollDescriptor': { 'x': 1905,
                'kind': {3: 'else'},
                'inits': RagdollDescriptor_inits },
            'LimitedHingeDescriptor': { 'x': 1935,
                'kind': {3: 'else'},
                'inits': LimitedHingeDescriptor_inits },
            'HingeDescriptor': { 'x': 1964,
                'kind': {3: 'elif'},
                'inits': HingeDescriptor_inits },
            'PrismaticDescriptor': { 'x': 1992,
                'kind': {4: 'elif'},
                'inits': PrismaticDescriptor_inits },
            'BoxBV': { 'x': 2040,
                'values': BoxBV_values },
            'BoundingVolume': { 'x': 2060,
                'flags': 'C',
                'cond': lambda p, s, cw: cw.typeReplace('BoundVolumeType', s),
                'inits': BoundingVolume_inits },
            'MalleableDescriptor': { 'x': 2154,
                'kind': {7: 'elif'},
                'cond': lambda p, s, cw: cw.typeReplace('hkConstraintType', s) if p and p.kind == 'switch' else s,
                'inits': MalleableDescriptor_inits },
            'ConstraintData': { 'x': 2171,
                'flags': 'C',
                'cond': lambda p, s, cw: cw.typeReplace('hkConstraintType', s) if p and p.kind == 'switch' else s,
                'inits': ConstraintData_inits }
        }
        #endregion
        #region NIF Objects
        def NiObject_code(s):
            if self.ex == CS:
                for x in self.nodes: s.attribs.append(Class.Attrib(f'JsonDerivedType(typeof({x}), typeDiscriminator: nameof({x}))'))
                body = '\n'.join([f'            case "{x}": return new {x}(r);' for x in self.nodes])
                s.methods.append(Class.Method('''
    public static NiObject Read(NiReader r, string nodeType) {
        // Console.WriteLine($"{nodeType}: {r.Tell()}");
        if (nodeType.StartsWith("NiDataStream\\x01")) nodeType = Z.ExtractRTTIArgs(r, nodeType);
        switch (nodeType) {
BODY
            default: { Log.info($"Tried to read an unsupported NiObject type ({nodeType})."); return null; }
        }
    }
'''.replace('BODY', body)))
            elif self.ex == PY:
                body = '\n'.join([f'            case \'{x}\': return type({x}(r))' for x in self.nodes])
                s.methods.append(Class.Method('''
    @staticmethod
    def read(r: NiReader, nodeType: str) -> NiObject:
        # print(f'{nodeType}: {r.tell()}')
        def type(o: NiObject) -> NiObject: setattr(o, '$type', nodeType); return o;
        match nodeType:
BODY
            case _: log.info(f'Tried to read an unsupported NiObject type ({nodeType}).'); node = None
        return node
'''.replace('BODY', body)))
        def bhkRigidBody_values(s, values):
            values[6].extcond = values[6].vercond[20:]
            values[6].vercond = values[6].vercond[:16]
            values[21].extcond = values[21].vercond[15:]
            values[21].vercond = values[21].vercond[:11]
            values[27].extcond = values[27].vercond[20:]
            values[27].vercond = values[27].vercond[:16]
            values[38].kind = '?:'; values[38].elsecw = ('r.ReadUInt16()', 'r.readUInt16()')
            del values[39]
        def NiBlendInterpolator_values(s, values):
            values.insert(5, Class.Comment(s, 'Flags conds'))
            values.insert(15, Class.Comment(s, 'end Flags 1 conds'))
            for i in range(6, 15): values[i].cond = '((int)Flags & 1) == 0' if self.ex == CS else '(self.flags & 1) == 0'
        def NiCollisionData_values(s, values):
            pass
        def NiAVObject_values(s, values):
            values[0].type = 'Flags'
            # del values[0]
            # values[0].cond = None; values[0].default = '14'; values[0].kind = '?:'
             #; values[0].kind = '?:'
            # values[1].kind = ':'; values[1].default = '14'
        def NiDynamicEffect_values(s, values):
            del values[4]; del values[1]
            values[0].default = 'true'
            values[3].arr1 = values[2].arr1 = values[1].arr1 = 'L32'
        def NiGeomMorpherController_values(s, values):
            del values[3]
            values[4].arr1 = values[3].arr1 = 'L32'
        def NiFlipController_values(s, values):
            pass
        def NiGeometryData_values(s, values):
            values[1].cond = '!NiPSysData || r.UV2 >= 34'
            del values[2]
            if self.ex == PY: values[8].default = values[9].default = '0'
            values[14].cond = values[15].cond = '(HasNormals != 0) && (((int)VectorFlags | (int)BSVectorFlags) & 4096) != 0' if self.ex == CS else '(hasNormals != 0) && ((self.vectorFlags | self.bsVectorFlags) & 4096) != 0'
            values[25].arr1 = values[26].arr1 = '((NumUVSets & 63) | ((int)VectorFlags & 63) | ((int)BSVectorFlags & 1))' if self.ex == CS else '((self.numUvSets & 63) | (self.vectorFlags & 63) | (self.bsVectorFlags & 1))'
            values[20].kind = values[11].kind = values[5].kind = 'var?'
        def NiKeyframeData_values(s, values):
            values[2].cond = 'RotationType != KeyType.XYZ_ROTATION_KEY'
            values[4].cond = values[3].cond = 'RotationType == KeyType.XYZ_ROTATION_KEY'
        def NiSourceTexture_values(s, values):
            values[8].default = 'true'; values[9].default = 'false'
        def NiTexturingProperty_values(s, values):
            values[19].kind = 'var?+'; values[20].kind = 'var:'
            values[22].kind = 'var?+'; values[23].kind = 'var:'
            values[25].kind = 'var?+'; values[26].kind = 'var:'
            values[28].kind = 'var?+'; values[29].kind = 'var:'
        def NiTriStripsData_values(s, values):
            values[3].arr2 += '[i]'; values[4].arr2 += '[i]'
        def NiRawImageData_values(s, values):
            values[3].cond = 'Image Type == ImageType.RGB'
            values[4].cond = 'Image Type == ImageType.RGBA'
        def BSAnimNote_values(s, values):
            values[2].cond = 'Type == AnimNoteType.ANT_GRABIK'
            values[3].cond = values[4].cond = 'Type == AnimNoteType.ANT_LOOKIK'
        self.customs = self.customs | {
            'NiObject': { 'x': 2193,
                'code': NiObject_code },
            'bhkRigidBody': { 'x': 2306,
                'values': bhkRigidBody_values },
            'bhkMoppBvTreeShape': { 'x': 2511,
                'calculated': lambda s: ('r.ReadUInt32()', 'r.readUInt32()') },
            'NiExtraData': { 'x': 2592,
                'conds': ['BSExtraData'] },
            'InterpBlendItem': { 'x': 2660,
                'kind': {-2: 'elif'},
                'flags': 'C' },
            'NiBlendInterpolator': { 'x': 2670,
                'fields': {'Interp Count': 'ushort', 'Single Index': 'ushort', 'High Priority': 'int', 'Next High Priority': 'int' },
                'values': NiBlendInterpolator_values },
            'NiObjectNET': { 'x': 2712,
                'conds': ['BSLightingShaderProperty'] },
            'NiCollisionData': { 'x': 2735,
                'values': NiCollisionData_values },
            'NiAVObject': { 'x': 2787,
                'types': {'Rotation': 'Matrix33R'},
                'values': NiAVObject_values },
            'NiDynamicEffect': { 'x': 2804,
                'kind': {-1: 'elif', -2: 'elif'},
                'values': NiDynamicEffect_values },
            'NiTimeController': { 'x': 2860,
                'kind': {-1: 'elif'} },
            'NiGeomMorpherController': { 'x': 2892,
                'kind': {-2: 'elif'},
                'values': NiGeomMorpherController_values },
            'NiFlipController': { 'x': 2988,
                'values': NiFlipController_values },
            'NiGeometry': { 'x': 3125,
                'conds': ['NiParticleSystem'] },
            'NiGeometryData': { 'x': 3188,
                'conds': ['NiPSysData'],
                'types': {'Has Vertices': 'bool8', 'Has Normals': 'bool8', 'Has Vertex Colors': 'bool8' },
                'values': NiGeometryData_values },
            'NiKeyframeData': { 'x': 3683,
                'kind': {-3: 'else'}, 
                'values': NiKeyframeData_values },
            'NiSkinData': { 'x': 4439,
                'types': {'x': 'Matrix33R'} },
            'NiSkinPartition': { 'x': 4469,
                'arg': 'VertexDesc.VertexAttributes' if self.ex == CS else 'VertexDesc.VertexAttributes' },
            'NiSourceTexture': { 'x': 4491,
                'values': NiSourceTexture_values },
            'NiTextKeyExtraData': { 'x': 4570,
                'arg': 'KeyType.LINEAR_KEY' },
            'NiTextureEffect': { 'x': 4577,
                'types': {'Model Projection Matrix': 'Matrix33R'} },
            'NiTexturingProperty': { 'x': 4620,
                'values': NiTexturingProperty_values },
            'NiTriShapeData': { 'x': 4673,
                'calculated': lambda s: ('false', 'False') },
            'NiTriStripsData': { 'x': 4683,
                'values': NiTriStripsData_values },
            'NiVisData': { 'x': 4804,
                'arg': 'KeyType.LINEAR_KEY' },
            'NiRawImageData': { 'x': 4835,
                'values': NiRawImageData_values },
            'BSAnimNote': { 'x': 5915,
                'values': BSAnimNote_values },
        }
        #endregion
        self.init()

xml = ET.parse('nif.xml')
NifCodeWriter(CS).write(xml, '../../../../../dotnet/Families/GameX.Gamebryo/Formats/Nif.cs')
NifCodeWriter(PY).write(xml, '../formats/nif.py')


# NiSkinPartition is a problem - ver lost