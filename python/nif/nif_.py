import xml.etree.ElementTree as ET
from code_writer import fmt_camel, CodeWriter

#region Writer

class EnumX: pass
class ClassX: pass

CS = 0; PY = 1

class NifCodeWriter(CodeWriter):
    #region Hide
    def __init__(self, ex: str):
        self.ex = ex
        self.members = {
            'TEMPLATE': [None, ('T', 'T'), ('??', '??')],
            'bool': [None, ('bool', 'bool'), ('r.ReadBool32()', 'r.readBool32()')],
            'byte': [None, ('byte', 'int'), ('r.ReadByte()', 'r.readByte()'), ('r.ReadL8FArray(r => {0})', 'r.readL8FArray(lambda r: {0})')],
            'uint': [None, ('uint', 'int'), ('r.ReadUInt32()', 'r.readUInt32()'), ('r.ReadL32FArray(r => {0})', 'r.readL32FArray(lambda r: {0})')],
            'ulittle32': [None, ('uint', 'int'), ('r.ReadUInt32()', 'r.readUInt32()')],
            'ushort': [None, ('ushort', 'int'), ('r.ReadUInt16()', 'r.readUInt16()'), ('r.ReadL16FArray(r => {0})', 'r.readL16FArray(lambda r: {0})')],
            'int': [None, ('int', 'int'), ('r.ReadInt32()', 'r.readInt32()'), ('r.ReadL32FArray(r => {0})', 'r.readL32FArray(lambda r: {0})')],
            'short': [None, ('short', 'int'), ('r.ReadInt16()', 'r.readInt16()'), ('r.ReadL16FArray(r => {0})', 'r.readL16FArray(lambda r: {0})')],
            'BlockTypeIndex': [None, ('ushort', 'int'), ('r.ReadUInt16()', 'r.readUInt16()')],
            'char': [None, ('sbyte', 'int'), ('r.ReadSByte()', 'r.readSByte()')],
            'FileVersion': [None, ('uint', 'int'), ('r.ReadUInt32()', 'r.readUInt32()')],
            'Flags': [None, ('Flags', 'Flags'), ('(Flags)r.ReadUInt16()', 'Flags(r.readUInt16())')],
            'float': [None, ('float', 'float'), ('r.ReadSingle()', 'r.readSingle()')],
            'hfloat': [None, ('float', 'float'), ('r.ReadHalf()', 'r.readHalf()')],
            'HeaderString': [None, ('string', 'str'), ('??', '??')],
            'LineString': [None, ('string', 'str'), ('??', '??')],
            'Ptr': [None, ('int?', 'int'), ('X<{T}>.Ptr(r)', 'X[{T}].ptr(r)')],
            'Ref': [None, ('int?', 'int'), ('X<{T}>.Ref(r)', 'X[{T}].ref(r)')],
            'StringOffset': [None, ('uint', 'int'), ('r.ReadUInt32()', 'r.readUInt32()')],
            'StringIndex': [None, ('uint', 'int'), ('r.ReadUInt32()', 'r.readUInt32()')],
            # Compounds
            'SizedString': [None, ('string', 'str'), ('r.ReadL32AString()', 'r.readL32AString()')],
            'string': [None, ('string', 'str'), ('Y.String(r)', 'Y.string(r)')],
            'ByteArray': [None, ('byte[]', 'bytearray'), ('r.ReadL8Bytes()', 'r.readL8Bytes()')],
            'ByteMatrix': [None, ('??', '??'), ('??', '??')],
            'Color3': [None, ('Color3', 'Color3'), ('new Color3(r)', 'Color3(r)')],
            'ByteColor3': [None, ('ByteColor3', 'ByteColor3'), ('new ByteColor3(r)', 'ByteColor3(r)')],
            'Color4': [None, ('Color4', 'Color4'), ('new Color4(r)', 'Color4(r)')],
            'ByteColor4': [None, ('ByteColor4', 'ByteColor4'), ('new Color4Byte(r)', 'Color4Byte(r)')],
            'FilePath': [None, ('??', '??'), ('??', '??')],
            # Compounds
            'ByteVector3': [None, ('Vector3<false>', 'Vector3'), ('new Vector3(r.ReadByte(), r.ReadByte(), r.ReadByte())', 'Vector3(r.readByte(), r.readByte(), r.readByte())')],
            'HalfVector3': [None, ('Vector3', 'Vector3'), ('r.ReadHalf()', 'r.readHalf()')],
            'Vector3': [None, ('Vector3', 'Vector3'), ('r.ReadVector3()', 'r.readVector3()')],
            'Vector4': [None, ('Vector4', 'Vector4'), ('r.ReadVector4()', 'r.readVector4()')],
            'Quaternion': [None, ('Quaternion', 'Quaternion'), ('r.ReadQuaternion()', 'r.readQuaternion()')],
            'hkQuaternion': [None, ('Quaternion', 'Quaternion'), ('r.ReadQuaternionWFirst()', 'r.readQuaternionWFirst()')],
            'Matrix22': [None, ('Matrix2x2', 'Matrix2x2'), ('r.ReadMatrix2x2()', 'r.readMatrix2x2()')],
            'Matrix33': [None, ('Matrix3x3', 'Matrix3x3'), ('r.ReadMatrix3x3()', 'r.readMatrix3x3()')],
            'Matrix34': [None, ('Matrix3x4', 'Matrix3x4'), ('r.ReadMatrix3x4()', 'r.readMatrix3x4()')],
            'Matrix44': [None, ('Matrix4x4', 'Matrix4x4'), ('r.ReadMatrix4x4()', 'r.readMatrix4x4()')],
            'hkMatrix3': [None, ('Matrix3x4', 'Matrix3x4'), ('r.ReadMatrix3x4()', 'r.readMatrix3x4()')],
            'MipMap': [None, ('??', '??'), ('??', '??')],
            'NodeSet': [None, ('??', '??'), ('??', '??')],
            'ShortString': [None, ('string', 'str'), ('r.ReadL8AString()', 'r.readL8AString()')]
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

class Flags(Flag):
    pass
''']
    #endregion
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
            npos = pos - (len(body) + cur_indent)
            self.emit(f'{body}{' ' * npos if npos > 0 else ' '}{self.symbolComment} {v}')
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
            if s.name in ['FilePath', 'SkinInfoSet']: self.emit()
            return
        # write class
        if s.comment: self.comment(s.comment)
        primary = s.init[0]
        with self.block(before=
            f'public{' abstract ' if s.abstract else ' '}class {s.name}{'(' + ', '.join(s.init[1][CS]) + ')' if primary else ''}{' : ' + s.inherit + ('(' + ', '.join(s.init[2][CS]) + ')' if primary else '') if s.inherit else ''}' if self.ex == CS else \
            f'class {s.name}{'(' + s.inherit + ')' if s.inherit else ''}:' if self.ex == PY else \
            None):
            for k, v in s.fields.items():
                self.emit_with_comment(
                    f'public {v[CS]} {k}{' = ' + v[CS+2] if primary else ''};' if self.ex == CS else \
                    f'{fmt_camel(k)}: {v[PY]}{' = ' + v[PY+2] if primary else ''}' if self.ex == PY else \
                    None, v[4], pos)
        self.emit()

#endregion

#region Objects

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
        return f'0x{v:08X}'
    return s

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
        if self.name not in cw.members: cw.members[self.name] = [self, (self.name, self.name), (self.init[CS], self.init[PY])]

class ClassX:
    class Field:
        def __init__(self, e: object):
            self.comment: str = e.text.strip().replace('        ', '') if e.text else None if e.text else None
            self.name: str = e.attrib['name'].replace(' ', '')
            # self.suffix: str = e.attrib.get('suffix')
            self.type: str = e.attrib['type']
            self.template: str = e.attrib.get('template')
            self.default: str = e.attrib.get('default')
            self.arg: str = e.attrib.get('arg')
            self.arrType: str = None
            self.arr1: str = z.replace(' ', '') if (z := e.attrib.get('arr1')) else None
            self.arr2: str = z.replace(' ', '') if (z := e.attrib.get('arr2')) else None
            self.cond: str = z if (z := e.attrib.get('cond')) else ''
            ver1: str = ver2Num(z) if (z := e.attrib.get('ver1')) else None
            ver2: str = ver2Num(z) if (z := e.attrib.get('ver2')) else None
            if ver1 and ver2: vercond = f'V >= {ver1} && V <= {ver2}'
            elif ver2: vercond = f'V <= {ver2}'
            elif ver1: vercond = f'V >= {ver1}'
            else: vercond = ''
            vercond += f'{' && ' if vercond else ''}' if (z := e.attrib.get('vercond')) else ''
            vercond += f'{' && ' if vercond else ''}(User Version == {z})' if (z := e.attrib.get('userver')) else ''
            vercond += f'{' && ' if vercond else ''}(User Version 2 == {z})' if (z := e.attrib.get('userver2')) else ''
            self.vercond: str = vercond
            self.hasVer: bool = self.cond or self.vercond
        @staticmethod
        def cond(s: str, ex: int) -> str:
            return s.replace('V ', 'h.V ').replace('User Version 2 ', 'h.UserVersion2 ').replace('User Version ', 'h.UserVersion ') if ex == CS else \
                s.replace('V ', 'h.v ').replace('User Version 2 ', 'h.userVersion2 ').replace('User Version ', 'h.userVersion ').replace('||', 'or').replace('&&', 'and') if ex == PY else \
                None
        def toType(self, s: str, ex: int) -> str:
            if self.arr1 and self.arr2: return f'{s}[][]' if ex == CS else \
                f'list[list[{s}]]' if ex == PY else \
                None
            elif self.arr1: return f'{s}[]' if ex == CS else \
                f'list[{s}]' if ex == PY else \
                None
            else: return s
        def toInit(self, s: str, ex: int) -> str:
            if self.arrType:
                s = cw.members[self.arrType][3][CS].replace('{0}', s) if ex == CS else \
                    cw.members[self.arrType][3][PY].replace('{0}', s) if ex == PY else \
                    None
                s = s.replace('r => X<{T}>.Ref(r)', 'X<{T}>.Ref') if ex == CS else \
                    s.replace('lambda r: X[{T}].ref(r)', 'X[{T}].ref') if ex == PY else \
                    None
            if self.template: s = s.replace('{T}', self.template)
            c = ''
            if self.cond and self.vercond: c = ClassX.Field.cond(f'{self.cond} && {self.vercond}', ex)
            elif self.vercond: c = ClassX.Field.cond(self.vercond, ex)
            elif self.cond: c = ClassX.Field.cond(self.cond, ex)
            if c:
                s = f'{c} ? {s} : default' if ex == CS else \
                    f'{s} if {c} else None'  if ex == PY else \
                    None
            return s
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
        self.values = [ClassX.Field(e) for e in e]
        self.processValues()
        primary = not niobject and not self.hasVer2
        hasHeader = niobject or self.hasVer
        # members
        self.init = (primary,
            [['BinaryReader r', 'Header h'], ['r: Reader', 'h: Header']] if hasHeader else [['BinaryReader r'], ['r: Reader']],
            [['r', 'h'], ['r', 'h']] if hasHeader else [['r'], ['r']],
            [f'new {self.name}(r, h)', f'{self.name}(r, h)'] if hasHeader else [f'new {self.name}(r)', f'{self.name}(r)'])
        if self.name not in cw.members: cw.members[self.name] = [self, (self.name, self.name), (self.init[3][CS], self.init[3][PY])]
    def secondPass(self):
        # print(f'pass2: {self.name}: ' + str([f'{x.name}: {cw.members[x.type][1][CS]}' for x in self.values]))
        self.fields = {x.name: (
            x.toType(cw.members[x.type][1][CS], CS), x.toType(cw.members[x.type][1][PY], PY),
            x.toInit(cw.members[x.type][2][CS], CS), x.toInit(cw.members[x.type][2][PY], PY),
            x.comment) for x in self.values}
    def processValues(self):
        if not self.values: return
        for i in range(len(self.values) - 1, 0, -1):
            name = self.values[i-1].name
            if name.startswith('Num'):
                count = len([x for x in self.values if x.arr1 == name])
                arrNext = self.values[i].arr1 == name
                if arrNext and count == 1:
                    # print(f'merged: {self.name}: {self.values[i].name}: {self.values[i-1].type}')
                    self.values[i].arrType = self.values[i-1].type
                    del self.values[i-1]
        self.hasVer = any(x.hasVer for x in self.values)
        self.hasVer2 = False

def parse(tree: object, cw: NifCodeWriter) -> None:
    root = tree.getroot()
    cs = ['X']
    # first pass
    for e in root:
        # if e.attrib.get('name') in cw.members: continue
        match e.tag:
            case 'version' | 'basic': continue
            case 'enum' | 'bitflags': cs.append(EnumX(e, cw))
            case 'compound' | 'niobject': cs.append(ClassX(e, cw))
    # second pass
    for s in cs:
        match s:
            case ClassX(): s.secondPass()
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