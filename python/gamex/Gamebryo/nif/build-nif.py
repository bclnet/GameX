import xml.etree.ElementTree as ET
from xmlCodeWriter import CS, PY, Class, Elem, XmlCodeWriter

class NifCodeWriter(XmlCodeWriter):
    def export(self, name: str): return name in self.es3 or name in self.es3x
    def tags(self, name: str): return 'X' if name in self.es3 else 'Y' if name in self.es3x else ''
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
        self.customs = {
            '_header': (
'''using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using static OpenStack.Debug;
#pragma warning disable CS9113 // Parameter is unread.

namespace GameX.Gamebryo.Formats.Nif;

''',
'''import os
from io import BytesIO
from enum import Enum, Flag, IntFlag
from typing import TypeVar, Generic
from numpy import ndarray, array
from openstk.poly import Reader
from gamex import FileSource, PakBinaryT, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo
from gamex.globalx import Color3, Color4
from gamex.desser import DesSer

T = TypeVar('T')

# types
type Vector3 = ndarray
type Vector4 = ndarray
type Matrix2x2 = ndarray
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

static class Z {
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
    public override void Write(Utf8JsonWriter w, TexCoord s, JsonSerializerOptions options) => w.WriteStringValue($"{s.u:f4} {s.v:f4}");
}

public class TriangleJsonConverter : JsonConverter<Triangle> {
    public override Triangle Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Triangle s, JsonSerializerOptions options) => w.WriteStringValue($"{s.v1} {s.v2} {s.v3}");
}
''',
'''class Ref(Generic[T]):
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

class Flags(IntFlag):
    Hidden = 0x1

def RefJsonConverter(s): return f'{s.v}'
def TexCoordJsonConverter(s): return f'{s.u:.4f} {s.v:.4f}'
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
            vx0.initcw = ('Blocks = new NiObject[NumBlocks];', 'self.blocks: list[NiObject] = [None]*self.numBlocks')
            values.insert(20, vx1 := Class.Value(s, Elem({ 'name': 'Roots', 'type': 'Ref', 'template': 'NiObject', 'arr1': 'x' })))
            vx1.initcw = ('Roots = new Footer(r).Roots;', 'self.roots = Footer(r).roots')
        def Header_inits(s, inits):
            inits.insert(20, if0 := Class.If(s, None, None, None, 'if'))
            inits.insert(21, if1 := Class.If(s, None, None, None, 'else'))
            if0.vercond = 'ZV >= 0x05000001'
            if0.inits.insert(0, Class.Code(s, ('for (var i = 0; i < NumBlocks; i++) Blocks[i] = NiObject.Read(r, BlockTypes[BlockTypeIndex[i]]);', 'for i in range(self.numBlocks): self.blocks[i] = NiObject.read(r, BlockTypes[BlockTypeIndex[i]])')))
            if1.inits.insert(0, Class.Code(s, ('for (var i = 0; i < NumBlocks; i++) Blocks[i] = NiObject.Read(r, Z.String(r));', 'for i in range(self.numBlocks): self.blocks[i] = NiObject.read(r, Z.string(r))')))
        def Header_code(s):
            s.init = (['BinaryReader b', 'b: Reader'], ['rx', 'rx'], [f'new NiReader(r)', f'NiReader(r)'])
            s.namecw = ('NiReader', 'NiReader'); s.inherit = 'BinaryReader' if self.ex == CS else 'Reader'
            s.values[0].initcw = ('(HeaderString, V) = Z.ParseHeaderStr(b.ReadVAString(0x80, 0xA)); var r = this;', '(self.headerString, self.v) = Z.parseHeaderStr(b.readVAString(128, b\'\\x0A\')); r = self')
        def StringPalette_code(s):
            s.values[0].typecw = ('string[]', 'list[str]'); s.values[0].initcw = ('Palette = r.ReadL32AString().Split((char)0)', 'self.palette: list[str] = r.readL32AString().split(\'0x00\')')
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
            ifx.inits[1].vercond = None; ifx.inits[1].type = 'uint'; ifx.inits[1].kind = 'var?'
            ifx.inits[2].vercond = None
            ifx.inits[3].vercond = None
            ifx.inits[5].vercond = None; ifx.inits[5].cond = ifx.inits[5].cond[16:-1]; ifx.inits[5].arr2x = 'i'
            ifx.inits[6].vercond = None; ifx.inits[6].cond = ifx.inits[6].cond[16:-1]
            ifx.inits.insert(5, ifx := Class.If(s, None, None, (ifx.inits, 5, 7), 'if'))
            ifx.vercond = 'B32:HasFaces'
            #
            inits.insert(6, ifx := Class.If(s, None, None, in0, 'if'))
            ifx.inits[3].arr2x = 'i'
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
                'constPre': (', bool full', ''), 'constArg': ('', ', half: bool'),
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
                'inits': Header_inits,
                'code': Header_code },
            'StringPalette': { 'x': 1508,
                'code': StringPalette_code },
            'Key': { 'x': 1521,
                'kind': {-1: 'elif'},
                'constArg': (', KeyType keyType', ', keyType: KeyType'), 'constNew': (', ARG', ', self.ARG'),
                'cond': lambda p, s, cw: cw.typeReplace('KeyType', s).replace('ARG', 'keyType') },
            'QuatKey': { 'x': 1537,
                'constArg': (', KeyType keyType', ', keyType: KeyType'), 'constNew': (', ARG', ', self.ARG'),
                'cond': lambda p, s, cw: cw.typeReplace('KeyType', s).replace('ARG', 'keyType') },
            'TexCoord': { 'x': 1545,
                'flags': 'C',
                'constArg': ('', ', half: bool=None'), 'constNew': ('', ', False'),
                'consts': [
                    ('public TexCoord(double u, double v) { this.u = (float)u; this.v = (float)v; }', 'if isinstance(r, float): self.u = r; self.v = half; return'),
                    (None, 'elif isinstance(r, tuple): self.u = r[0]; self.v = r[1]; return'),
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
                'type': ['', ('BSVertexData', 'BSVertexData'), lambda x: (x, x), ('new BSVertexData(r, true)', 'BSVertexData(r, true)'), lambda c: (f'r.ReadFArray(r => new BSVertexData(r, true), {c})', f'r.readFArray(lambda r: BSVertexData(r, true), {c})')] },
            'SkinPartition': { 'x': 1661,
                'calculated': lambda s: ('(ushort)(NumVertices / 3)', '(self.numVertices / 3)'),
                'values': SkinPartition_values, 'inits': SkinPartition_inits },
            'NiTransform': { 'x': 1728,
                'type': {'Rotation': 'Matrix33R'} },
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
            nodes = ['NiNode', 'NiTriShape', 'NiTexturingProperty', 'NiSourceTexture', 'NiMaterialProperty', 'NiMaterialColorController', 'NiTriShapeData', 'RootCollisionNode', 'NiStringExtraData', 'NiSkinInstance', 'NiSkinData', 'NiAlphaProperty', 'NiZBufferProperty', 'NiVertexColorProperty', 'NiBSAnimationNode', 'NiBSParticleNode', 'NiParticles', 'NiParticlesData', 'NiRotatingParticles', 'NiRotatingParticlesData', 'NiAutoNormalParticles', 'NiAutoNormalParticlesData', 'NiUVController', 'NiUVData', 'NiTextureEffect', 'NiTextKeyExtraData', 'NiVertWeightsExtraData', 'NiParticleSystemController', 'NiBSPArrayController', 'NiGravity', 'NiParticleBomb', 'NiParticleColorModifier', 'NiParticleGrowFade', 'NiParticleMeshModifier', 'NiParticleRotation', 'NiKeyframeController', 'NiKeyframeData', 'NiColorData', 'NiGeomMorpherController', 'NiMorphData', 'AvoidNode', 'NiVisController', 'NiVisData', 'NiAlphaController', 'NiFloatData', 'NiPosData', 'NiBillboardNode', 'NiShadeProperty', 'NiWireframeProperty', 'NiCamera', 'NiPathController', 'NiPixelData'] #, 'NiExtraData', 'NiSkinPartition']
            if self.ex == CS:
                for x in nodes: s.attribs.append(Class.Attrib(f'JsonDerivedType(typeof({x}), typeDiscriminator: nameof({x}))'))
                body = '\n'.join([f'            case "{x}": return new {x}(r);' for x in nodes])
                s.methods.append(Class.Method('''
    public static NiObject Read(NiReader r, string nodeType) {
        // Console.WriteLine(nodeType);
        switch (nodeType) {
BODY
            default: { Log($"Tried to read an unsupported NiObject type ({nodeType})."); return null; }
        }
    }
'''.replace('BODY', body)))
            elif self.ex == PY:
                body = '\n'.join([f'            case \'{x}\': n = {x}(r)' for x in nodes])
                s.methods.append(Class.Method('''
    @staticmethod
    def read(r: NiReader, nodeType: str) -> NiObject:
        # print(nodeType)
        match nodeType:
BODY
            case _: Log(f'Tried to read an unsupported NiObject type ({nodeType}).'); n = None
        setattr(n, '$type', nodeType)
        return n
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
        def InterpBlendItem_values(s, values):
            pass
        def NiBlendInterpolator_values(s, values):
            pass
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
        def NiRawImageData_values(s, values):
            values[3].cond = 'Image Type == ImageType.RGB'
            values[4].cond = 'Image Type == ImageType.RGBA'
        self.customs = self.customs | {
            'NiObject': { 'x': 2193,
                'code': NiObject_code },
            'bhkRigidBody': { 'x': 2306,
                'values': bhkRigidBody_values },
            'bhkMoppBvTreeShape': { 'x': 2511,
                'calculated': lambda s: ('0', '0') },
            'NiExtraData': { 'x': 2592,
                'conds': ['BSExtraData'] },
            'InterpBlendItem': { 'x': 2660,
                'kind': {-2: 'elif'},
                'flags': 'C',
                'values': InterpBlendItem_values },
            'NiBlendInterpolator': { 'x': 2670,
                'values': NiBlendInterpolator_values },
            'NiObjectNET': { 'x': 2712,
                'conds': ['BSLightingShaderProperty'] },
            'NiCollisionData': { 'x': 2735,
                'values': NiCollisionData_values },
            'NiAVObject': { 'x': 2787,
                'type': {'Rotation': 'Matrix33R'},
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
                'type': {'Has Vertices': 'uint', 'Has Normals': 'uint', 'Has Vertex Colors': 'uint' },
                'values': NiGeometryData_values },
            'NiKeyframeData': { 'x': 3683,
                'kind': {-3: 'else'}, 
                'values': NiKeyframeData_values },
            'NiSkinData': { 'x': 4439,
                'type': {'x': 'Matrix33R'} },
            'NiSkinPartition': { 'x': 4469,
                'arg': '(uint)VertexDesc.VertexAttributes' if self.ex == CS else 'VertexDesc.VertexAttributes' },
            'NiSourceTexture': { 'x': 4491,
                'values': NiSourceTexture_values },
            'NiTextKeyExtraData': { 'x': 4570,
                'arg': 'KeyType.LINEAR_KEY' },
            'NiTextureEffect': { 'x': 4577,
                'type': {'Model Projection Matrix': 'Matrix33R'} },
            'NiTexturingProperty': { 'x': 4620,
                'values': NiTexturingProperty_values },
            'NiTriShapeData': { 'x': 4673,
                'calculated': lambda s: ('false', 'False') },
            'NiVisData': { 'x': 4804,
                'arg': 'KeyType.LINEAR_KEY' },
            'NiRawImageData': { 'x': 4835,
                'values': NiRawImageData_values },
        }
        #endregion
        self.init()

xml = ET.parse('nif.xml')
NifCodeWriter(CS).write(xml, '../../../../dotnet/Gamebryo/GameX.Gamebryo/Formats/Nif.cs')
NifCodeWriter(PY).write(xml, '../formats/nif.py')
