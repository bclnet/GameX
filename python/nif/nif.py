import xml.etree.ElementTree as ET
from code_writer import CodeWriter

#region CSharp : Writer

class CSharpWriter(CodeWriter):
    def __init__(self):
        super().__init__(default_delim=('{', '}'))
        self.emit_raw(
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

''')
    def region(self, name: str) -> None: self.emit(f'#region {name}'); self.emit()
    def endregion(self) -> None: self.emit(); self.emit(f'#endregion'); self.emit()
    def comment(self, comment: str) -> None:
        self.emit('/// <summary>')
        for val in comment.split('\n'): self.emit(f'/// {val}')
        self.emit('/// </summary>')
    def emitX(self) -> None:
        self.emit_raw(
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
''')

#endregion

#region Python : Writer

class PythonWriter(CodeWriter):
    def __init__(self):
        super().__init__()
        self.emit_raw(
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

''')
    def region(self, name: str) -> None: self.emit(f'#region {name}'); self.emit()
    def endregion(self) -> None: self.emit(); self.emit(f'#endregion'); self.emit()
    def comment(self, comment: str) -> None:
        for val in comment.split('\n'): self.emit(f'# {val}')
    def emitX(self) -> None:
        self.emit_raw(
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
''')


#endregion

#region Objects

class EnumX:
    comment: str
    flag: bool
    name: str
    fields: list[tuple[str, str, str]]
    def __init__(self, e: object, flag: bool):
        self.comment = e.text.strip().replace('        ', '') if e.text else None
        self.flag = flag
        self.name = e.attrib['name']
        self.fields = [(x.attrib['name'], x.attrib['value'], x.text.strip() if x.text else None) for x in e]
        # print(f'{self.name}: {self.comment} {self.fields})

    def write(self, cw: CodeWriter) -> None:
        if self.comment: cw.comment(self.comment)
        if isinstance(cw, CSharpWriter):
            if self.flag: cw.emit(f'[Flags]')
            with cw.block(before=f'public enum {self.name}'):
                for f in self.fields:
                    cw.emit(f'{f[0]} = {f[1]},')
        if isinstance(cw, PythonWriter):
            with cw.block(before=f'class {self.name}({'Flag' if self.flag else 'Enum'}):'):
                for f in self.fields:
                    cw.emit(f'{f[0]} = {f[1]},')
        cw.emit()

class ClassX:
    comment: str
    abstract: bool
    inherit: str
    name: str
    fields: list[tuple]
    def __init__(self, e: object, niobject: bool):
        self.comment = e.text.strip().replace('        ', '') if e.text else None
        if niobject:
            self.abstract = e.attrib.get('abstract') == '1'
            self.inherit = e.attrib.get('inherit')
        self.name = e.attrib['name']
        # print(f'{self.name}: {self.comment}')

    def write(self, cw: CodeWriter) -> None:
        if self.comment: cw.comment(self.comment)
        # if isinstance(cw, CSharpWriter):
        #     with cw.indent():
        #         cw.emit('print("hello, world.")')
        # if isinstance(cw, PythonWriter):
        #     with cw.indent():
        #         cw.emit('print("hello, world.")')
        cw.emit()

def parse(tree: object, cw: CodeWriter) -> None:
    root = tree.getroot()
    for e in root:
        match e.attrib.get('name'):
            case 'bool': cw.region('X'); cw.emitX()
            case 'AccumFlags': cw.endregion(); cw.region('Enums')
            case 'SizedString': cw.endregion(); cw.region('Compounds')
            case 'NiObject': cw.endregion(); cw.region('NIF Objects')
            case 'BSDistantObjectLargeRefExtraData': cw.endregion()
        match e.tag:
            case 'version' | 'basic': continue
            case 'enum' | 'bitflags': EnumX(e, e.tag == 'bitflags').write(cw)
            case 'compound' | 'niobject': ClassX(e, e.tag == 'niobject').write(cw)

#endregion

tree = ET.parse('nif.xml')
# c#
cw = CSharpWriter()
parse(tree, cw)
with open('nifx.cs', 'w') as f:
    f.write(cw.render())
# py
cw = PythonWriter()
parse(tree, cw)
with open('nifx.py', 'w') as f:
    f.write(cw.render())