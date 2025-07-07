import xml.etree.ElementTree as ET
from xmlCodeWriter import CS, PY, Class, XmlCodeWriter

class NifCodeWriter(XmlCodeWriter):
    def __init__(self, ex: str):
        super().__init__(ex)
        def ControlledBlock_values(s, values):
            s.flags = 'C'
            values.insert(1, Class.Comment(s, 'NiControllerSequence::InterpArrayItem'))
            values.insert(6, Class.Comment(s, 'Bethesda-only'))
            values.insert(8, Class.Comment(s, 'NiControllerSequence::IDTag, post-10.1.0.104 only'))
        def ControlledBlock_if(s, ifx):
            if s.name == 'Interpolator ID Offset':
                ifx.kind = 'elseif'
                ifx.inits[0].name = ''; ifx.inits[0].initcw = ('var stringPalette = X<NiStringPalette>.Ref(r)', 'stringPalette = X[NiStringPalette].ref(r)')
                ifx.inits[1].name = ''; ifx.inits[1].initcw = ('NodeName = Y.StringRef(r, stringPalette)', 'self.nodeName = Y.stringRef(r, stringPalette)')
                ifx.inits[2].name = ''; ifx.inits[2].initcw = ('PropertyType = Y.StringRef(r, stringPalette)', 'self.propertyType = Y.stringRef(r, stringPalette)')
                ifx.inits[3].name = ''; ifx.inits[3].initcw = ('ControllerType = Y.StringRef(r, stringPalette)', 'self.controllerType = Y.stringRef(r, stringPalette)')
                ifx.inits[4].name = ''; ifx.inits[4].initcw = ('ControllerID = Y.StringRef(r, stringPalette)', 'self.controllerID = Y.stringRef(r, stringPalette)')
                ifx.inits[5].name = ''; ifx.inits[5].initcw = ('InterpolatorID = Y.StringRef(r, stringPalette)', 'self.interpolatorID = Y.stringRef(r, stringPalette)')
            elif s.name == 'Interpolator ID' and '&&' not in ifx.vercond: ifx.kind = 'elseif'
        def Header_code(s):
            s.values[0].initcw = ('(HeaderString, V) = Y.ParseHeaderStr(r.ReadVAString(0x80, 0xA)); var h = this', '(self.headerString, self.v) = Y.parseHeaderStr(r.readVAString(128, b\'\\x0A\')); h = self')
        def Header_values(s, values):
            s.flags = 'C'
            values[2].namecw = ('V', 'v')
            values[4].namecw = ('UV', 'uv')
            values[6].namecw = ('UV2', 'uv2')
            del values[10]
            values[10].arr1 = values[11].arr1 = 'L16'
        def StringPalette_code(s):
            s.values[0].typecw = ('string[]', 'list[str]'); s.values[0].initcw = ('Palette = r.ReadL32AString().Split((char)0)', 'self.palette: list[str] = r.readL32AString().split(\'0x00\')')
        def TexDesc_values(s, values):
            values.insert(10, Class.Comment(s, 'NiTextureTransform'))
        def MotorDescriptor_inits(s, inits):
            s.flags = 'C'
            inits.insert(1, Class.If(s, None, None, (inits, 1, 3), 'switch'))
        def BoundingVolume_inits(s, inits):
            s.flags = 'C'
            inits.insert(1, Class.If(s, None, None, (inits, 1, 6), 'switch'))
        def BSVertexData_values(s, values):
            del values[0:3]
        def BSVertexData_inits(s, inits):
            inits.insert(0, Class.Code(s, ('var full = sse || arg.HasFlag(VertexFlags.Full_Precision);', 'full = sse or VertexFlags.Full_Precision in arg')))
            inits.insert(1, Class.Code(s, ('var tangents = arg.HasFlag(VertexFlags.Tangents);', 'tangents = VertexFlags.Tangents in arg')))
            #
            inits.insert(2, ifx := Class.If(s, None, inits[2], (inits, 2, 5), 'if'))
            ifx.cond = ifx.cond[1:16]
            ifx.inits[0].kind = '?:'; ifx.inits[0].cond = ifx.inits[0].cond[22:-1]; ifx.inits[0].elsecw = ('r.ReadHalfVector3()', 'r.readHalfVector3()')
            inits.insert(6, ifx := Class.If(s, None, inits[6], (inits, 6, 8), 'if'))
            ifx.cond = ifx.cond[1:17]
            ifx.inits[0].cond = ifx.inits[0].cond[23:-1]
            ifx.inits[1].cond = ifx.inits[1].cond[23:-1]
        def SkinPartition_inits(s, inits):
            newInits = []
            {18:}
            {18}


6 a
# 7 b
8 a
9 a
# 10 b
11 a
12 20.3.1.1
13
14 a
# 15 b
16 a
# 17 b
18 a
18, 15, 10, 7

            ifb = Class.If(s, None, inits[6], (inits, 6, 8), 'if'))
            for x in [18, 15, 10, 7]
            newInits.append(inits[7]); del inits[7]

            # inits.insert(10, inits[7]); del inits[7]
            print(inits[6:12])
            # print(inits.remove(inits[7]))
            # print(inits.remove(inits[9]))
            # print(inits[15])

            # print(inits[12])
            # (inits[2], inits[2]) = (inits[2], inits[2])
        self.customs = {
            #region Header
            '_header': (
'''using MathNet.Numerics;
using SharpCompress.Common;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using static OpenStack.Debug;
#pragma warning disable CS9113 // Parameter is unread.

namespace GameX.Bethesda.Formats.Nif;

''',
'''import os
from io import BytesIO
from enum import Enum, Flag
from typing import TypeVar, Generic
from gamex import FileSource, PakBinaryT, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo
from gamex.compression import decompressLz4, decompressZlib
from gamex.Bethesda.formats.records import FormType, Header

T = TypeVar('T')

# types
type Vector3 = np.ndarray
type Vector4 = np.ndarray
type Matrix4x4 = np.ndarray

# typedefs
class Reader: pass
class Color: pass

#
class UnionBV: pass
class NiObject: pass

'''),
            'X': (
'''static class X<T> {
    public static T Read(BinaryReader r) {
        if (typeof(T) == typeof(float)) { return (T)(object)r.ReadSingle(); }
        else if (typeof(T) == typeof(byte)) { return (T)(object)r.ReadByte(); }
        else if (typeof(T) == typeof(string)) { return (T)(object)r.ReadL32Encoding(); }
        else if (typeof(T) == typeof(Vector3)) { return (T)(object)r.ReadVector3(); }
        else if (typeof(T) == typeof(Quaternion)) { return (T)(object)r.ReadQuaternionWFirst(); }
        else if (typeof(T) == typeof(Color4)) { return (T)(object)new Color4(r); }
        else throw new NotImplementedException("Tried to read an unsupported type.");
    }
    public static string Str(BinaryReader r) => r.ReadL32Encoding();
    // Refers to an object before the current one in the hierarchy.
    public static int? Ptr(BinaryReader r) { int v; return (v = r.ReadInt32()) < 0 ? null : v; } //:M
    // Refers to an object after the current one in the hierarchy.
    public static int? Ref(BinaryReader r) { int v; return (v = r.ReadInt32()) < 0 ? null : v; } //:M
}

static class Y {
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
}

public enum Flags : ushort { }
''',
'''class X(Generic[T]):
    @staticmethod
    def read(type: type, r: Reader) -> object:
        if type == float: return r.readSingle()
        elif type == byte: return r.readByte()
        elif type == str: return r.readL32Encoding()
        elif type == Vector3: return r.readVector3()
        elif type == Quaternion: return r.readQuaternionWFirst()
        elif type == Color4: return Color4(r)
        else: raise NotImplementedError('Tried to read an unsupported type.')
    # Refers to an object before the current one in the hierarchy.
    @staticmethod
    def ptr(r: Reader): return None if (v := r.readInt32()) < 0 else v #:M
    # Refers to an object after the current one in the hierarchy.
    @staticmethod
    def ref(r: Reader): return None if (v := r.readInt32()) < 0 else v #:M

class Y:
    @staticmethod
    def string(r: Reader) -> str: return r.readL32Encoding()
    @staticmethod
    def stringRef(r: Reader, p: int) -> str: return None
    @staticmethod
    def isVersionSupported(v: int) -> bool: return True
    @staticmethod
    def parseHeaderStr(s: str) -> tuple:
        p = s.indexOf('Version')
        if p >= 0:
            v = s
            v = v[(p + 8):]
            for i in range(len(v)):
                if v[i].isdigit() or v[i] == '.': continue
                else: v = v[:i]
            ver = Header.ver2Num(v)
            if not Header.isVersionSupported(ver): raise Exception(f'Version {Header.ver2Str(ver)} ({ver}) is not supported.')
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
    pass
'''),
            #endregion
            'BoneVertData': { 'x': 1421,
                'constArg': ('', ', half: bool'),
                'const': ('public BoneVertData(BinaryReader r, bool half) { Index = r.ReadUInt16(); Weight = r.ReadHalf(); }', 'if half: self.index = r.readUInt16(); self.weight = r.readHalf(); return') },
            'BoneVertDataHalf': { 'x': 1427,
                'type': ['', ('BoneVertData', 'BoneVertData'), ('X', 'X'), ('new BoneVertData(r, true)', 'BoneVertData(r, true)'), lambda c: (f'r.ReadFArray(r => new BoneVertData(r, true), {c})', f'r.readFArray(lambda r: BoneVertData(r, true), {c})')] },
            'ControlledBlock': { 'x': 1439,
                'values': ControlledBlock_values,
                'if': ControlledBlock_if },
            'Header': { 'x': 1482,
                'values': Header_values,
                'code': Header_code },
            'StringPalette': { 'x': 1508,
                'code': StringPalette_code },
            'Key': { 'x': 1521,
                'constArg': (', KeyType keyType', ', keyType: KeyType'), 'constNew': (', Interpolation', ', self.interpolation'),
                'cond': lambda s, cw: cw.typeReplace('KeyType', s).replace('ARG', 'keyType') },
            'QuatKey': { 'x': 1537,
                'constArg': (', KeyType keyType', ', keyType: KeyType'),
                'cond': lambda s, cw: cw.typeReplace('KeyType', s).replace('ARG', 'keyType') },
            'TexCoord': { 'x': 1545,
                'constArg': ('', ', half: bool'), 'constNew': ('', ', false'),
                'const': ('public TexCoord(BinaryReader r, bool half) { u = r.ReadHalf(); v = r.ReadHalf(); }', 'if half: self.u = r.readHalf(); self.v = r.readHalf(); return') },
            'HalfTexCoord': { 'x': 1551,
                'type': ['', ('TexCoord', 'TexCoord'), ('X', 'X'), ('new TexCoord(r, true)', 'TexCoord(r, true)'), lambda c: (f'r.ReadFArray(r => new TexCoord(r, true), {c})', f'r.readFArray(lambda r: TexCoord(r, true), {c})')] },
            'TexDesc': { 'x': 1565,
                'values': TexDesc_values },
            'BSVertexData': { 'x': 1616,
                'constArg': (', VertexFlags arg, bool sse', ', arg: VertexFlags, sse: bool'), #'constNew': (', false', ', false'),
                'condcs': lambda s, cw: s \
                    .replace('(ARG & 16) != 0', 'arg.HasFlag(VertexFlags.Vertex)') \
                    .replace('(ARG & 32) != 0', 'arg.HasFlag(VertexFlags.UVs)') \
                    .replace('(ARG & 128) != 0', 'arg.HasFlag(VertexFlags.Normals)') \
                    .replace('(ARG & 256) != 0', 'tangents').replace('(ARG & 256) == 0', '!tangents') \
                    .replace('(ARG & 512) != 0', 'arg.HasFlag(VertexFlags.Vertex_Colors)') \
                    .replace('(ARG & 1024) != 0', 'arg.HasFlag(VertexFlags.Skinned)') \
                    .replace('(ARG & 4096) != 0', 'arg.HasFlag(VertexFlags.Eye_Data)') \
                    .replace('(ARG & 16384) != 0', 'full').replace('(ARG & 16384) == 0', '!full'),
                'condpy': lambda s, cw: s \
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
                'type': ['', ('BSVertexData', 'BSVertexData'), ('X', 'X'), ('new BSVertexData(r, true)', 'BSVertexData(r, true)'), lambda c: (f'r.ReadFArray(r => new BSVertexData(r, true), {c})', f'r.readFArray(lambda r: BSVertexData(r, true), {c})')] },
            'SkinPartition': { 'x': 1661,
                'calculated': lambda s: ('(ushort)(NumVertices / 3)', '(self.numVertices / 3)'),
                'inits': SkinPartition_inits },
            'Morph': { 'x': 1768,
                'constArg': (', uint numVertices', ', numVertices: int'),
                'cond': lambda s, cw: s.replace('ARG', 'numVertices') },
            'MotorDescriptor': { 'x': 1898,
                'cond': lambda s, cw: cw.typeReplace('MotorType', s),
                'inits': MotorDescriptor_inits },
            'BoundingVolume': { 'x': 2060,
                'cond': lambda s, cw: cw.typeReplace('BoundVolumeType', s),
                'inits': BoundingVolume_inits },
            'bhkMoppBvTreeShape': { 'x': 2511,
                'calculated': lambda s: ('0', '0') },
            'NiTriShapeData': { 'x': 4673,
                'calculated': lambda s: ('0', '0') },
        }
        self.struct = {
            'BoneVertData': ('<Hf', 7),
            'TBC': ('<3f', 12),
            'TexCoord': ('<2f', 8),
            'Triangle': ('<3H', 18),
            'BSVertexDesc': ('<5bHb', 8),
            'NiPlane': ('<4f', 24),
            'NiBound': ('<4f', 24),
            'NiQuatTransform': 'x',
            'NiTransform': 'x',
            'Particle': ('<9f2H', 40) }
        self.init()

xml = ET.parse('nif.xml')
NifCodeWriter(CS).write(xml, 'nif.cs')
# NifCodeWriter(PY).write(xml, 'nif.py')
