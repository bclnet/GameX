import re
import xml.etree.ElementTree as ET
from code_writer import fmt_camel, CodeWriter

#region Writer

class EnumX: pass
class ClassX: pass

CS = 0; PY = 1

def ver2Num(s: str) -> str:
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

def verReplace(s: str) -> str:
    if not 'Version ' in s: return s
    for x in [x for x in re.findall(r'Version.[!<=>]+.([.0-9]+)', s) if '.' in x]:
        s = s.replace(x, f'0x{ver2Num(x):08X}')
    return s

class NifCodeWriter(CodeWriter):
    def __init__(self, ex: str):
        self.ex = ex
        #region Hide
        self.members = {
            'TEMPLATE': ['', ('T', 'T'), ('??', '??')],
            'bool': ['', ('bool', 'bool'), ('r.ReadBool32()', 'r.readBool32()'), lambda c: (f'r.ReadBytes({c})', f'r.readBytes({c})')],
            'byte': ['', ('byte', 'int'), ('r.ReadByte()', 'r.readByte()'), lambda c: (f'r.ReadBytes({c})', f'r.readBytes({c})')],
            'uint': ['', ('uint', 'int'), ('r.ReadUInt32()', 'r.readUInt32()'), lambda c: (f'r.ReadPArray<uint>("I", {c})', f'r.readPArray(None, \'I\', {c})')],
            'ulittle32': ['', ('uint', 'int'), ('r.ReadUInt32()', 'r.readUInt32()'), None],
            'ushort': ['', ('ushort', 'int'), ('r.ReadUInt16()', 'r.readUInt16()'), lambda c: (f'r.ReadPArray<ushort>("H", {c})', f'r.readPArray(None, \'H\', {c})')],
            'int': ['', ('int', 'int'), ('r.ReadInt32()', 'r.readInt32()'), lambda c: (f'r.ReadPArray<uint>("i", {c})', f'r.readPArray(None, \'i\', {c})')],
            'short': ['', ('short', 'int'), ('r.ReadInt16()', 'r.readInt16()'), lambda c: (f'r.ReadPArray<short>("h", {c})', f'r.readPArray(None, \'h\', {c})')],
            'BlockTypeIndex': ['', ('ushort', 'int'), ('r.ReadUInt16()', 'r.readUInt16()'), lambda c: (f'r.ReadFArray(r => new BlockTypeIndex(r), {c})', f'r.readFArray(lambda r: BlockTypeIndex(r), {c})')],
            'char': ['', ('sbyte', 'int'), ('r.ReadSByte()', 'r.readSByte()'), lambda c: (f'r.ReadFAString({c})', f'r.readFAString({c})'), None],
            'FileVersion': ['', ('uint', 'int'), ('r.ReadUInt32()', 'r.readUInt32()'), None],
            'Flags': ['', ('Flags', 'Flags'), ('(Flags)r.ReadUInt16()', 'Flags(r.readUInt16())'), None],
            'float': ['', ('float', 'float'), ('r.ReadSingle()', 'r.readSingle()'), lambda c: (f'r.ReadPArray<float>("f", {c})', f'r.readPArray(None, \'f\', {c})')],
            'hfloat': ['', ('float', 'float'), ('r.ReadHalf()', 'r.readHalf()'), lambda c: (f'r.ReadFArray(r => r.ReadHalf(), {c})', f'r.readFArray(lambda r: r.readHalf(), {c})')],
            'HeaderString': ['', ('string', 'str'), ('??', '??'), None],
            'LineString': ['', ('string', 'str'), ('??', '??'), lambda c: (f'r.ReadFArray(r => new LineString(r), {c})', f'r.readFArray(lambda r: LineString(r), {c})')],
            'Ptr': ['', ('int?', 'int'), ('X<{T}>.Ptr(r)', 'X[{T}].ptr(r)'), lambda c: (f'r.ReadFArray(X<{{T}}>.Ptr, {c})', f'r.readFArray(X[{{T}}].ptr, {c})')],
            'Ref': ['', ('int?', 'int'), ('X<{T}>.Ref(r)', 'X[{T}].ref(r)'), lambda c: (f'r.ReadFArray(X<{{T}}>.Ref, {c})', f'r.readFArray(X[{{T}}].ref, {c})')],
            'StringOffset': ['', ('uint', 'int'), ('r.ReadUInt32()', 'r.readUInt32()'), None],
            'StringIndex': ['', ('uint', 'int'), ('r.ReadUInt32()', 'r.readUInt32()'), None],
            # Compounds
            'SizedString': ['', ('string', 'str'), ('r.ReadL32AString()', 'r.readL32AString()'), lambda c: (f'r.ReadFArray(r => new SizedString(r), {c})', f'r.readFArray(lambda r: SizedString(r), {c})')],
            'string': ['', ('string', 'str'), ('Y.String(r)', 'Y.string(r)'), lambda c: (f'r.ReadFArray(r => Y.String(r), {c})', f'r.readFArray(lambda r: Y.string(r), {c})')],
            'ByteArray': ['', ('byte[]', 'bytearray'), ('r.ReadL8Bytes()', 'r.readL8Bytes()'), None],
            'ByteMatrix': ['', ('??', '??'), ('??', '??'), None],
            'Color3': ['', ('Color3', 'Color3'), ('new Color3(r)', 'Color3(r)'), None],
            'ByteColor3': ['', ('ByteColor3', 'ByteColor3'), ('new ByteColor3(r)', 'ByteColor3(r)'), lambda c: (f'r.ReadFArray(r => new ByteColor3(r), {c})', f'r.readFArray(lambda r: ByteColor3(r), {c})')],
            'Color4': ['', ('Color4', 'Color4'), ('new Color4(r)', 'Color4(r)'), lambda c: (f'r.ReadFArray(r => new Color4(r), {c})', f'r.readFArray(lambda r: Color4(r), {c})')],
            'ByteColor4': ['', ('ByteColor4', 'ByteColor4'), ('new Color4Byte(r)', 'Color4Byte(r)'), lambda c: (f'r.ReadFArray(r => new Color4Byte(r), {c})', f'r.readFArray(lambda r: Color4Byte(r), {c})')],
            'FilePath': ['', ('??', '??'), ('??', '??'), None],
            # Compounds
            'ByteVector3': ['', ('Vector3<false>', 'Vector3'), ('new Vector3(r.ReadByte(), r.ReadByte(), r.ReadByte())', 'Vector3(r.readByte(), r.readByte(), r.readByte())'), lambda c: (f'r.ReadFArray(r => new Vector3(r.ReadByte(), r.ReadByte(), r.ReadByte()), {c})', f'r.readFArray(lambda r: Vector3(r.readByte(), r.readByte(), r.readByte()), {c})')],
            'HalfVector3': ['', ('Vector3', 'Vector3'), ('new Vector3(r.ReadHalf(), r.ReadHalf(), r.ReadHalf())', 'Vector3(r.readHalf(), r.readHalf(), r.readHalf())'), lambda c: (f'r.ReadFArray(r => new Vector3(r.ReadHalf(), r.ReadHalf(), r.ReadHalf()), {c})', f'r.readFArray(lambda r: Vector3(r.readHalf(), r.readHalf(), r.readHalf()), {c})')],
            'Vector3': ['', ('Vector3', 'Vector3'), ('r.ReadVector3()', 'r.readVector3()'), lambda c: (f'r.ReadFArray(r => r.ReadVector3(), {c})', f'r.readFArray(lambda r: r.readVector3(), {c})')],
            'Vector4': ['', ('Vector4', 'Vector4'), ('r.ReadVector4()', 'r.readVector4()'), lambda c: (f'r.ReadFArray(r => r.ReadVector4(), {c})', f'r.readFArray(lambda r: r.readVector4(), {c})')],
            'Quaternion': ['', ('Quaternion', 'Quaternion'), ('r.ReadQuaternion()', 'r.readQuaternion()'), lambda c: (f'r.ReadFArray(r => r.ReadQuaternion(), {c})', f'r.readFArray(lambda r: r.readQuaternion(), {c})')],
            'hkQuaternion': ['', ('Quaternion', 'Quaternion'), ('r.ReadQuaternionWFirst()', 'r.readQuaternionWFirst()'), None],
            'Matrix22': ['', ('Matrix2x2', 'Matrix2x2'), ('r.ReadMatrix2x2()', 'r.readMatrix2x2()'), None],
            'Matrix33': ['', ('Matrix3x3', 'Matrix3x3'), ('r.ReadMatrix3x3()', 'r.readMatrix3x3()'), None],
            'Matrix34': ['', ('Matrix3x4', 'Matrix3x4'), ('r.ReadMatrix3x4()', 'r.readMatrix3x4()'), lambda c: (f'r.ReadFArray(r => r.ReadMatrix3x4(), {c})', f'r.readFArray(lambda r: r.readMatrix3x4(), {c})')],
            'Matrix44': ['', ('Matrix4x4', 'Matrix4x4'), ('r.ReadMatrix4x4()', 'r.readMatrix4x4()'), None],
            'hkMatrix3': ['', ('Matrix3x4', 'Matrix3x4'), ('r.ReadMatrix3x4()', 'r.readMatrix3x4()'), None],
            'MipMap': ['', ('MipMap', 'MipMap'), ('new MipMap(r)', 'MipMap(r)'), lambda c: (f'r.ReadFArray(r => new MipMap(r), {c})', f'r.readFArray(lambda r: MipMap(r), {c})')],
            'NodeSet': ['', ('NodeSet', 'NodeSet'), ('new NodeSet(r)', 'NodeSet(r)'), lambda c: (f'r.ReadFArray(r => new NodeSet(r), {c})', f'r.readFArray(lambda r: NodeSet(r), {c})')],
            'ShortString': ['', ('string', 'str'), ('r.ReadL8AString()', 'r.readL8AString()'), None]
        }
        if self.ex == CS:
            super().__init__(default_delim=('{', '}'))
            self.symbolComment = '//'
            self.xbodys = [
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
''']
        elif self.ex == PY:
            super().__init__()
            self.symbolComment = '#'
            self.xbodys = [
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
''']
        #endregion
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
        # redirects
        self.members['BoneVertDataHalf'] = ['', ('BoneVertData', 'BoneVertData'), ('new BoneVertData(r, true)', 'BoneVertData(r, true)'), lambda c: (f'r.ReadFArray(r => new BoneVertData(r, true), {c})', f'r.readFArray(lambda r: BoneVertData(r, true), {c})')]
#         self.customs = {
#             'BoneVertDataHalf': ([], ['''Index = r.ReadUInt16();
# Weight = r.ReadHalf();''', '''Index = r.ReadUInt16();
# Weight = r.ReadHalf();''']
#         }

    def region(self, name: str) -> None: self.emit(f'#region {name}'); self.emit()
    def endregion(self) -> None: self.trim_last_line_if_empty(); self.emit(); self.emit(f'#endregion'); self.emit()
    def comment(self, comment: str) -> None:
        if self.ex == CS:
            self.emit('/// <summary>')
            for v in comment.split('\n'): self.emit(f'/// {v}')
            self.emit('/// </summary>')
        elif self.ex == PY:
            for val in comment.split('\n'): self.emit(f'# {val}')
    def emit_with_comment(self, body: str, comment: str, pos: int) -> None:
        if not comment: return self.emit(body)
        cur_indent = (self.cur_indent or 0) + 1
        for v in comment.split('\n'):
            npos = pos - (len(body) + cur_indent) if pos > 0 else 0
            self.emit(f'{body}{' ' * npos if npos >= 0 else ' '}{self.symbolComment} {v}')
            body = ''
    def writeX(self) -> None:
        self.emit_raw(self.xbodys[0])
        self.region('X')
        self.emit_raw(self.xbodys[1])
    def writeEnum(self, s: EnumX) -> None:
        pos = 37
        # write enum
        if s.comment: self.comment(s.comment)
        if self.ex == CS and s.flag: self.emit(f'[Flags]')
        with self.block(before=
            f'public enum {s.name} : {s.storage}' if self.ex == CS else \
            f'class {s.name}({'Flag' if s.flag else 'Enum'}):' if self.ex == PY else \
            None):
            vl = s.values[-1]
            for v in s.values:
                self.emit_with_comment(f'{v[0]} = {'1 << ' if s.flag and v[1] != '0' else ''}{v[1]}{'' if v == vl else ','}', v[2], pos)
        self.emit()
    def writeClass(self, s: ClassX) -> None:
        pos = 57
        # skip class
        if not self.members[s.name][0]:
            self.emit(
                f'// {s.name} -> {self.members[s.name][2][CS]}' if self.ex == CS else \
                f'# {s.name} -> {self.members[s.name][2][PY]}' if self.ex == PY else \
                None)
            if s.name in ['FilePath', 'ShortString', 'BoneVertDataHalf']: self.emit()
            return
        # write class
        if s.comment: self.comment(s.comment)
        primary = 'P' in s.init[0]
        if self.ex == CS and s.struct and s.struct != 'x': self.emit(f'[StructLayout(LayoutKind.Sequential, Pack = 1)]')
        with self.block(before=
            f'public{' abstract ' if s.abstract else ' '}{'struct' if s.struct else 'class'} {s.nameCS}{'(' + s.init[1][CS] + ')' if primary else ''}{' : ' + s.inherit + ('(' + s.init[2][CS] + ')' if primary else '') if s.inherit else ''}' if self.ex == CS else \
            f'class {s.name}{'(' + s.inherit + ')' if s.inherit else ''}:' if self.ex == PY else \
            None):
            if s.struct and s.struct != 'x': self.emit(
                f'public static (string, int) Struct = ("{s.struct[0]}", {s.struct[1]});' if self.ex == CS else \
                f'struct = (\'{s.struct[0]}\', {s.struct[1]})' if self.ex == PY else \
                None )
            if self.ex == CS:
                for k, v in s.fields.items():
                    self.emit_with_comment(f'public {v[0][CS]} {k}{' = ' + v[1][CS] if primary else ''};' if v[0] else '', v[2], pos if v[0] else 0)
                if not primary:
                    self.emit('')
                    if s.struct and s.struct != 'x': self.emit(f'public {s.name}() {{}}')
                    with self.block(before=f'public {s.name}({s.init[1][CS]})'):
                        for x in s.values:
                            match x:
                                case str(): self.emit(x)
                                case ClassX.Field(): self.emit(f'{x.name} = {x.initcw[CS]};')
            elif self.ex == PY:
                if not primary: 
                    for k, v in s.fields.items():
                        self.emit_with_comment(f'{fmt_camel(k)}: {v[0][PY]}' if v[0] else '', v[2], pos if v[0] else 0)
                    self.emit('')
                with self.block(before=f'def __init__(self, {s.init[1][PY]}):'):
                    if primary: 
                        for k, v in s.fields.items():
                            self.emit_with_comment(f'self.{fmt_camel(k)}: {v[0][PY]} = {v[1][PY]}', v[2], pos)
                    else:                            
                        for x in s.values:
                            match x:
                                case str(): self.emit(x)
                                case ClassX.Field(): self.emit(f'self.{fmt_camel(x.name)} = {x.initcw[PY]}')
        self.emit()

#endregion

#region Objects

class EnumX:
    comment: str
    flag: bool
    name: str
    storage: str
    values: list[tuple[str, str, str]]
    def __init__(self, e: object, cw: NifCodeWriter):
        self.comment = e.text.strip().replace('        ', '') if e.text else None
        self.flag = e.tag == 'bitflags'
        self.name = e.attrib['name'].replace(' ', '_')
        self.storage = e.attrib['storage']
        self.values = [(e.attrib['name'].replace(' ', '_'), e.attrib['value'], e.text.strip() if e.text else None) for e in e]
        # members
        match self.storage:
            case 'uint': self.init = [f'({self.name})r.ReadUInt32()', f'{self.name}(r.readUInt32())']
            case 'ushort': self.init = [f'({self.name})r.ReadUInt16()', f'{self.name}(r.readUInt16())']
            case 'byte': self.init = [f'({self.name})r.ReadByte()', f'{self.name}(r.readByte())']
        flags = 'E'
        if self.name not in cw.members: cw.members[self.name] = [
            flags,
            (self.name, self.name),
            (self.init[CS], self.init[PY]),
            lambda c: (f'r.ReadFArray(r => {self.init[CS]}, {c})', f'r.readFArray(lambda r: {self.init[PY]}, {c})')]

class ClassX:
    class Comment:
        def __init__(self, parent: ClassX, s: str):
            self.comment: str = s
            self.name: str = s
        def code(self, parent: ClassX, cw: NifCodeWriter) -> None:
            self.typecw = None
            self.initcw = None
    class Field:
        def __init__(self, parent: ClassX, e: object):
            self.comment: str = e.text.strip().replace('        ', '') if e.text else None if e.text else None
            self.name: str = e.attrib['name'].replace(' ', '')
            if self.name == parent.name: self.name += '_'
            # self.suffix: str = e.attrib.get('suffix')
            self.type: str = e.attrib['type']
            self.template: str = e.attrib.get('template')
            self.default: str = e.attrib.get('default')
            self.arg: str = e.attrib.get('arg')
            self.arr1: str = z.replace(' ', '') if (z := e.attrib.get('arr1')) else None
            self.arr2: str = z.replace(' ', '') if (z := e.attrib.get('arr2')) else None
            cond = z if (z := e.attrib.get('cond')) else ''
            cond = verReplace(cond.strip()).replace('User Version 2 ', 'UV2 ').replace('User Version ', 'UV ').replace('Version ', 'V ')
            self.cond: str = cond
            self.ver1: int = ver2Num(z) if (z := e.attrib.get('ver1')) else None
            self.ver2: int = ver2Num(z) if (z := e.attrib.get('ver2')) else None
            if self.ver1 and self.ver2: vercond = f'V >= 0x{self.ver1:08X} && V <= 0x{self.ver2:08X}'
            elif self.ver2: vercond = f'V <= 0x{self.ver2:08X}'
            elif self.ver1: vercond = f'V >= 0x{self.ver1:08X}'
            else: vercond = ''
            vercond += f'{' && ' if vercond else ''}' if (z := e.attrib.get('vercond')) else ''
            vercond += f'{' && ' if vercond else ''}(User Version == {z})' if (z := e.attrib.get('userver')) else ''
            vercond += f'{' && ' if vercond else ''}(User Version 2 == {z})' if (z := e.attrib.get('userver2')) else ''
            vercond = verReplace(vercond).replace('User Version 2 ', 'UV2 ').replace('User Version ', 'UV ').replace('Version ', 'V ')
            self.vercond: str = vercond
        def code(self, parent: ClassX, cw: NifCodeWriter) -> None:
            flags = parent.init[0]
            primary = 'P' in flags
            # totype
            cs = cw.members[self.type][1][CS]; py = cw.members[self.type][1][PY]
            if self.arr1 and self.arr2: self.typecw = [f'{cs}[][]', f'list[list[{py}]]']
            elif self.arr1: self.typecw = [f'{cs}[]', f'list[{py}]']
            else: self.typecw = [cs, py]
            # toinit
            cs = cw.members[self.type][2][CS]; py = cw.members[self.type][2][PY]
            if self.arr1:
                cs = cw.members[self.type][3](self.arr1)[CS]; py = cw.members[self.type][3](self.arr1)[PY]
                if self.arr1.startswith('L'): cs = cs.replace('Read', f'Read{self.arr1}').replace(f', {self.arr1})', ')'); py = py.replace('read', f'read{self.arr1}').replace(f', {self.arr1})', ')')
                if self.arr2: cs = f'r.ReadFArray(k => {cs}, {self.arr2})'; py = f'r.readFArray(lambda k: {py}, {self.arr2})'
            if self.template: cs = cs.replace('{T}', self.template); py = py.replace('{T}', self.template)
            c = ['', '']
            if self.cond and self.vercond: c = ClassX.Field.cond(f'{self.cond} && {self.vercond}')
            elif self.vercond: c = ClassX.Field.cond(self.vercond)
            elif self.cond: c = ClassX.Field.cond(self.cond)
            if c[0]:
                if primary: cs = f'{c[CS]} ? {cs} : default'; py = f'{py} if {c[PY]} else None'
                else: cs = f'if ({c[CS]}) {cs}'; py = f'if {c[PY]}: {py}'
            self.initcw = [cs, py]
        @staticmethod
        def cond(s: str) -> list[str]:
            return [
                s.replace('V ', 'h.V ').replace('UV2 ', 'h.UV2 ').replace('UV ', 'h.UV '),
                s.replace('V ', 'h.v ').replace('UV2 ', 'h.uv2 ').replace('UV ', 'h.uvs ').replace('||', 'or').replace('&&', 'and')]

    comment: str
    abstract: bool = False
    inherit: str = None
    name: str
    values: list[Field]
    fields: list[tuple]
    def __init__(self, e: object, cw: NifCodeWriter):
        niobject: bool = e.tag == 'niobject'
        self.comment = e.text.strip().replace('        ', '') if e.text else None
        if niobject:
            self.abstract = e.attrib.get('abstract') == '1'
            self.inherit = e.attrib.get('inherit')
        self.name = e.attrib['name']
        self.nameCS = f'{self.name}<T>' if e.attrib.get('istemplate') == '1' else self.name
        self.namePY = f'{self.name}[T]' if e.attrib.get('istemplate') == '1' else self.name
        self.struct = cw.struct.get(self.name)
        self.custom = False
        self.values = [ClassX.Field(self, e) for e in e]
        self.processValues()
        # flags
        hasVer = any(x.cond or x.vercond for x in self.values)
        flags = '' if self.name in [''] else \
            'C' if niobject or self.struct or self.custom else \
            'P'
        hasHeader = niobject or hasVer
        # members
        self.init = (flags,
            ['BinaryReader r, Header h', 'r: Reader, h: Header'] if hasHeader else ['BinaryReader r', 'r: Reader'],
            ['r, h', 'r, h'] if hasHeader else ['r', 'r'],
            [f'new {self.nameCS}(r, h)', f'{self.namePY}(r, h)'] if hasHeader else [f'new {self.nameCS}(r)', f'{self.namePY}(r)'])
        if self.name not in cw.members: cw.members[self.name] = [
            flags,
            (self.nameCS, self.namePY),
            (self.init[3][CS], self.init[3][PY]),
            lambda c: (f'r.ReadFArray(r => {self.init[3][CS]}, {c})', f'r.readFArray(lambda r: {self.init[3][PY]}, {c})')]
    def code(self, cw: NifCodeWriter):
        for x in self.values:
            if not isinstance(x, str): x.code(self, cw)
        self.fields = {x.name: (x.typecw, x.initcw, x.comment) for x in self.values if not isinstance(x, str)}
    def processValues(self):
        if not self.values: return
        for i in range(len(self.values) - 1, 0, -1):
            name = self.values[i-1].name
            if name.startswith('Num'):
                count = len([x for x in self.values if x.arr1 == name])
                arrNext = self.values[i].arr1 == name
                if arrNext and count == 1:
                    # print(f'merged: {self.name}: {self.values[i].name}: {self.values[i-1].type}')
                    match self.values[i-1].type:
                        case 'uint' | 'int': self.values[i].arr1 = 'L32'
                        case 'ushort' | 'short': self.values[i].arr1 = 'L16'
                        case 'byte': self.values[i].arr1 = 'L8'
                    del self.values[i-1]
        match self.name:
            case 'Header':
                self.custom = True
            case 'ControlledBlock':
                self.custom = True
                # del self.values[10:]
                self.values.insert(1, ClassX.Comment(self, 'NiControllerSequence::InterpArrayItem'))
                self.values.insert(6, ClassX.Comment(self, 'Bethesda-only'))
                self.values.insert(8, ClassX.Comment(self, 'NiControllerSequence::IDTag, post-10.1.0.104 only'))

def parse(tree: object, cw: NifCodeWriter) -> None:
    root = tree.getroot()
    cs = ['X']
    # first pass
    for e in root:
        match e.tag:
            case 'version' | 'basic': continue
            case 'enum' | 'bitflags': cs.append(EnumX(e, cw))
            case 'compound' | 'niobject': cs.append(ClassX(e, cw))
    # code pass
    for s in cs:
        match s:
            case ClassX(): s.code(cw)
    return cs

def write(cs: list[object], cw: NifCodeWriter) -> None:
    for s in cs:
        if s == 'X': cw.writeX(); continue
        match s.name:
            case 'AccumFlags': cw.endregion(); cw.region('Enums')
            case 'SizedString': cw.endregion(); cw.region('Compounds')
            case 'NiObject': cw.endregion(); cw.region('NIF Objects')
            case 'BSDistantObjectLargeRefExtraData': cw.endregion()
        match s:
            case EnumX(): cw.writeEnum(s)
            case ClassX(): cw.writeClass(s)

#endregion

tree = ET.parse('nif.xml')
# build nif.cs
print('build nif.cs')
cw = NifCodeWriter(CS)
cs = parse(tree, cw)
write(cs, cw)
with open('nif.cs', 'w', encoding='utf-8') as f:
    f.write(cw.render())
# build nif.py
print('build nif.py')
cw = NifCodeWriter(PY)
cs = parse(tree, cw)
write(cs, cw)
with open('nif.py', 'w', encoding='utf-8') as f:
    f.write(cw.render())
print('done')