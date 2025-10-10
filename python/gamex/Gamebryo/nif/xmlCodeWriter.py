import re

from code_writer import fmt_camel, CodeWriter

# type forward
class Enum: pass
class Class: pass

#region Helpers

CS = 0; PY = 1
MS = 0; MN = 1; MD = 2; MI = 3; MA = 4

def fmt_cwname(s: str) -> str: return 'X' + s.replace(' ', '_') if s[0].isdigit() else s.replace(' ', '_')
def fmt_py(s: str) -> str: return fmt_camel(s)
def op_flip(s: str) -> str: return s \
    .replace(' == ', ' Z!= ') \
    .replace(' != ', ' Z== ') \
    .replace(' >= ', ' Z< ') \
    .replace(' <= ', ' Z> ') \
    .replace(' > ', ' Z<= ') \
    .replace(' < ', ' Z>= ') \
    .replace(' && ', ' Z|| ') \
    .replace(' || ', ' Z&& ') \
    .replace(' Z== ', ' == ') \
    .replace(' Z!= ', ' != ') \
    .replace(' Z>= ', ' >= ') \
    .replace(' Z<= ', ' <= ') \
    .replace(' Z> ', ' > ') \
    .replace(' Z< ', ' < ') \
    .replace(' Z&& ', ' && ') \
    .replace(' Z|| ', ' || ') if '=' in s or '>' in s or '<' in s else None \

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

#endregion

#region Writer

class XmlCodeWriter(CodeWriter):
    def __init__(self, ex: str):
        self.ex = ex
        self.types = {
            'TEMPLATE': [None, ('T', 'T'), lambda x: (x, x), ('Y<T>.Read(r)', 'Y[T].read(r)'), None],
            'bool': [None, ('bool', 'bool'), lambda x: (x, x.replace('true', 'True').replace('false', 'False')), ('r.ReadBool32()', 'r.readBool32()'), lambda c: (f'[r.ReadBool32(), r.ReadBool32(), r.ReadBool32()]', f'[r.readBool32(), r.readBool32(), r.readBool32()]') if c == '3' else (f'r.ReadFArray(z => r.ReadBool32(), {c})', f'r.readFArray(lambda z: r.readBool32(), {c})')],
            'byte': [None, ('byte', 'int'), lambda x: (x, x), ('r.ReadByte()', 'r.readByte()'), lambda c: (f'r.ReadBytes({c})', f'r.readBytes({c})')],
            'uint': [None, ('uint', 'int'), lambda x: (x, x), ('r.ReadUInt32()', 'r.readUInt32()'), lambda c: (f'r.ReadPArray<uint>("I", {c})', f'r.readPArray(None, \'I\', {c})')],
            'ulittle32': [None, ('uint', 'int'), lambda x: (x, x), ('r.ReadUInt32()', 'r.readUInt32()'), None],
            'ushort': [None, ('ushort', 'int'), lambda x: (x, x), ('r.ReadUInt16()', 'r.readUInt16()'), lambda c: (f'r.ReadPArray<ushort>("H", {c})', f'r.readPArray(None, \'H\', {c})')],
            'int': [None, ('int', 'int'), lambda x: (x, x), ('r.ReadInt32()', 'r.readInt32()'), lambda c: (f'r.ReadPArray<int>("i", {c})', f'r.readPArray(None, \'i\', {c})')],
            'short': [None, ('short', 'int'), lambda x: (x, x), ('r.ReadInt16()', 'r.readInt16()'), lambda c: (f'r.ReadPArray<short>("h", {c})', f'r.readPArray(None, \'h\', {c})')],
            'BlockTypeIndex': [None, ('ushort', 'int'), lambda x: (x, x), ('r.ReadUInt16()', 'r.readUInt16()'), lambda c: (f'r.ReadPArray<ushort>("H", {c})', f'r.readPArray(\'H\', {c})')],
            'char': [None, ('sbyte', 'int'), lambda x: (x, x), ('r.ReadSByte()', 'r.readSByte()'), lambda c: (f'r.ReadFAString({c})', f'r.readFAString({c})'), None],
            'FileVersion': [None, ('uint', 'int'), lambda x: (x, x), ('r.ReadUInt32()', 'r.readUInt32()'), None],
            'Flags': [None, ('Flags', 'Flags'), lambda x: (f'(Flags){x}', f'{x}'), ('(Flags)r.ReadUInt16()', 'Flags(r.readUInt16())'), None],
            'float': [None, ('float', 'float'), lambda x: (f'{x}f', f'{x}'), ('r.ReadSingle()', 'r.readSingle()'), lambda c: (f'r.ReadPArray<float>("f", {c})', f'r.readPArray(None, \'f\', {c})')],
            'hfloat': [None, ('float', 'float'), lambda x: (f'{x}f', f'{x}'), ('r.ReadHalf()', 'r.readHalf()'), lambda c: (f'[r.ReadHalf(), r.ReadHalf(), r.ReadHalf(), r.ReadHalf()]', f'[r.readHalf(), r.readHalf(), r.readHalf(), r.readHalf()]') if c == '4' else (f'r.ReadFArray(z => r.ReadHalf(), {c})', f'r.readFArray(lambda z: r.readHalf(), {c})')],
            'HeaderString': [None, ('string', 'str'), lambda x: (x, x), ('Z.ParseHeaderStr(r.ReadVAString(0x80, 0xA))', 'Z.parseHeaderStr(r.readVAString(128, b\'\\x0A\'))'), None],
            'LineString': [None, ('string', 'str'), lambda x: (x, x), ('??', '??'), lambda c: (f'[r.ReadL8AString(), r.ReadL8AString(), r.ReadL8AString()]', f'[r.readL8AString(), r.readL8AString(), r.readL8AString()]') if c == '3' else (f'r.ReadFArray(z => r.ReadL8AString(), {c})', f'r.readFArray(lambda z: r.readL8AString(), {c})')],
            'Ptr': [None, ('Ref<{T}>', 'Ref[NiObject]'), lambda x: (x, x), ('X<{T}>.Ptr(r)', 'X[{T}].ptr(r)'), lambda c: (f'r.ReadFArray(X<{{T}}>.Ptr, {c})', f'r.readFArray(X[{{T}}].ptr, {c})')],
            'Ref': [None, ('Ref<{T}>', 'Ref[NiObject]'), lambda x: (x, x), ('X<{T}>.Ref(r)', 'X[{T}].ref(r)'), lambda c: (f'r.ReadFArray(X<{{T}}>.Ref, {c})', f'r.readFArray(X[{{T}}].ref, {c})')],
            'StringOffset': [None, ('uint', 'int'), lambda x: (x, x), ('r.ReadUInt32()', 'r.readUInt32()'), None],
            'StringIndex': [None, ('uint', 'int'), lambda x: (x, x), ('r.ReadUInt32()', 'r.readUInt32()'), None],
            # Compounds
            'SizedString': [None, ('string', 'str'), lambda x: (x, x), ('r.ReadL32AString()', 'r.readL32AString()'), lambda c: (f'r.ReadFArray(z => r.ReadL32AString(), {c})', f'r.readFArray(lambda z: r.readL32AString(), {c})')],
            'string': [None, ('string', 'str'), lambda x: (x, x), ('Z.String(r)', 'Z.string(r)'), lambda c: (f'r.ReadFArray(z => Z.String(r), {c})', f'r.readFArray(lambda z: Z.string(r), {c})')],
            'ByteArray': [None, ('byte[]', 'bytearray'), lambda x: (x, x), ('r.ReadL8Bytes()', 'r.readL8Bytes()'), None],
            'ByteMatrix': [None, ('??', '??'), lambda x: (x, x), ('??', '??'), None],
            'Color3': [None, ('Color3', 'Color3'), lambda x: (f'new({x})', f'Color3({x})'), ('new Color3(r)', 'Color3(r)'), None],
            'ByteColor3': [None, ('Color3', 'Color3'), lambda x: (x, x), ('new Color3(r.ReadBytes(3))', 'Color3(r.readBytes(3))'), lambda c: (f'r.ReadFArray(z => new Color3(r.ReadBytes(3)), {c})', f'r.readFArray(lambda z: Color3(r.readBytes(3)), {c})')],
            'Color4': [None, ('Color4', 'Color4'), lambda x: (f'new({x})', f'Color4({x})'), ('new Color4(r)', 'Color4(r)'), lambda c: (f'r.ReadFArray(z => new Color4(r), {c})', f'r.readFArray(lambda z: Color4(r), {c})')],
            'ByteColor4': [None, ('Color4', 'Color4'), lambda x: (x, x), ('new Color4(r.ReadBytes(4))', 'Color4(r.readBytes(4))'), lambda c: (f'r.ReadFArray(z => new Color4(r.ReadBytes(4)), {c})', f'r.readFArray(lambda z: Color4(r.readBytes(4)), {c})')],
            'FilePath': [None, ('string', 'str'), lambda x: (x, x), ('r.ReadL32Encoding()', 'r.readL32Encoding()'), None],
            # Compounds
            'ByteVector3': [None, ('Vector3<byte>', 'Vector3'), lambda x: (x, x), ('new Vector3<byte>(r.ReadByte(), r.ReadByte(), r.ReadByte())', 'Vector3(r.readByte(), r.readByte(), r.readByte())'), lambda c: (f'r.ReadFArray(z => new Vector3(r.ReadByte(), r.ReadByte(), r.ReadByte()), {c})', f'r.readFArray(lambda z: Vector3(r.readByte(), r.readByte(), r.readByte()), {c})')],
            'HalfVector3': [None, ('Vector3', 'Vector3'), lambda x: (x, x), ('new Vector3(r.ReadHalf(), r.ReadHalf(), r.ReadHalf())', 'Vector3(r.readHalf(), r.readHalf(), r.readHalf())'), lambda c: (f'r.ReadFArray(z => new Vector3(r.ReadHalf(), r.ReadHalf(), r.ReadHalf()), {c})', f'r.readFArray(lambda z: Vector3(r.readHalf(), r.readHalf(), r.readHalf()), {c})')],
            'Vector3': [None, ('Vector3', 'Vector3'), lambda x: (f'new({x})', f'Vector3({x})'), ('r.ReadVector3()', 'r.readVector3()'), lambda c: (f'r.ReadPArray<Vector3>("3f", {c})', f'r.readPArray(None, \'3f\', {c})')],
            'Vector4': [None, ('Vector4', 'Vector4'), lambda x: (f'new({x})', f'Vector4({x})'), ('r.ReadVector4()', 'r.readVector4()'), lambda c: (f'r.ReadPArray<Vector4>("4f", {c})', f'r.readPArray(None, \'4f\', {c})')],
            'Quaternion': [None, ('Quaternion', 'Quaternion'), lambda x: (x, x), ('r.ReadQuaternionWFirst()', 'r.readQuaternionWFirst()'), lambda c: (f'r.ReadFArray(z => r.ReadQuaternionWFirst(), {c})', f'r.readFArray(lambda z: r.readQuaternionWFirst(), {c})')],
            'hkQuaternion': [None, ('Quaternion', 'Quaternion'), lambda x: (x, x), ('r.ReadQuaternion()', 'r.readQuaternion()'), None],
            'Matrix22': [None, ('Matrix2x2', 'Matrix2x2'), lambda x: (x, x), ('r.ReadMatrix2x2()', 'r.readMatrix2x2()'), None],
            'Matrix33': [None, ('Matrix3x3', 'Matrix3x3'), lambda x: (x, x), ('r.ReadMatrix3x3()', 'r.readMatrix3x3()'), None],
            'Matrix34': [None, ('Matrix3x4', 'Matrix3x4'), lambda x: (x, x), ('r.ReadMatrix3x4()', 'r.readMatrix3x4()'), lambda c: (f'r.ReadFArray(z => r.ReadMatrix3x4(), {c})', f'r.readFArray(lambda z: r.readMatrix3x4(), {c})')],
            'Matrix44': [None, ('Matrix4x4', 'Matrix4x4'), lambda x: (x, x), ('r.ReadMatrix4x4()', 'r.readMatrix4x4()'), None],
            'hkMatrix3': [None, ('Matrix3x4', 'Matrix3x4'), lambda x: (x, x), ('r.ReadMatrix3x4()', 'r.readMatrix3x4()'), None],
            'Matrix33R': [None, ('Matrix4x4', 'Matrix4x4'), lambda x: (x, x), ('r.ReadMatrix3x3As4x4()', 'r.readMatrix3x3As4x4()'), lambda c: (f'r.ReadFArray(z => r.ReadMatrix3x3As4x4(), {c})', f'r.readFArray(lambda z: r.readMatrix3x3As4x4(), {c})')],
            # 'NodeSet': [None, ('NodeSet', 'NodeSet'), lambda x: (x, x), ('new NodeSet(r)', 'NodeSet(r)'), lambda c: (f'r.ReadFArray(z => new NodeSet(r), {c})', f'r.readFArray(lambda z: NodeSet(r), {c})')],
            'ShortString': [None, ('string', 'str'), lambda x: (x, x), ('r.ReadL8AString()', 'r.readL8AString()'), None]
        }
        self.struct = {}
        if self.ex == CS:
            super().__init__(default_delim=('{', '}'))
            self.symbolComment = '//'
        elif self.ex == PY:
            super().__init__()
            self.symbolComment = '#'
        self.customs = {}
    def init(self) -> None:
        for k,v in self.customs.items():
            if '_' in v: self.types[k] = v['_']
    def typeReplace(self, type: str, s: str) -> str:
        for x in self.types[type][0].values: s = s.replace(f' {x[1]}', f' {type}.{x[0]}')
        return s
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
    def writeCustom(self, s: str) -> None:
        if s in self.customs: self.emit_raw(self.customs[s][self.ex])
    def writeEnum(self, s: Enum) -> None:
        if not s.export: return
        pos = 37
        # write enum
        if s.comment: self.comment(s.comment)
        if self.ex == CS and s.flag: self.emit(f'[Flags]')
        with self.block(before=
            f'public enum {s.name} : {s.storage}' if self.ex == CS else \
            f'class {s.name}({'Flag' if s.flag else 'Enum'}):' if self.ex == PY else \
            None, delim=
            ('{' + f' // {s.tags}', '}') if s.tags and self.ex == CS else \
            (f'# {s.tags}', None) if s.tags and self.ex == PY else \
            None):
            vl = s.values[-1]
            for v in s.values:
                self.emit_with_comment(f'{v[0]} = {('1U' if self.ex == CS and s.storage == 'uint' else '1') + ' << ' if s.flag and v[1] != '0' else ''}{v[1]}{'' if self.ex == PY or v == vl else ','}', v[3], pos)
        self.emit()
    def writeClass(self, s: Class) -> None:
        if not s.export: return
        pos = 57
        # skip class
        if not self.types[s.name][MS]:
            self.emit(
                f'// {s.name} -> {self.types[s.name][MI][CS]}' if self.ex == CS else \
                f'# {s.name} -> {self.types[s.name][MI][PY]}' if self.ex == PY else \
                None)
            if s.name in ['FilePath', 'ShortString', 'BoneVertDataHalf', 'BoneVertDataHalf', 'BSVertexDataSSE']: self.emit()
            return
        # write class
        if s.comment: self.comment(s.comment)
        primary = 'P' in s.flags
        if self.ex == CS:
            if s.struct and s.struct != 'x': self.emit(f'[StructLayout(LayoutKind.Sequential, Pack = 1)]')
            for x in s.attribs: self.emit(f'[{x.body}]')
        constPre = s.custom['constPre'] if 'constPre' in s.custom else ('', '')
        with self.block(before=
            f'public{' abstract ' if s.abstract else ' '}{'struct' if s.struct else 'class'} {s.namecw[CS]}{'(' + s.init[0][CS] + constPre[CS] + ')' if primary else ''}{' : ' + s.inherit + ('(' + s.init[1][CS] + ')' if primary else '') if s.inherit else ''}' if self.ex == CS else \
            f'class {s.namecw[PY].replace('<T>', '')}{'(' + s.inherit + ')' if s.inherit else ''}:' if self.ex == PY else \
            None, delim=
            ('{' + f' // {s.tags}', '}') if s.tags and self.ex == CS else \
            (f'# {s.tags}', None) if s.tags and self.ex == PY else \
            None):
            fieldItems = s.fields.items()
            if self.ex == CS:
                if s.struct and s.struct != 'x': self.emit(f'public static (string, int) Struct = ("{s.struct[0]}", {s.struct[1]});')
                for k, v in fieldItems:
                    if v[0] or v[3]: self.emit_with_comment(f'public {v[0][CS]} {v[1][CS] if primary else k[CS] + (' = ' + v[2][CS] + ';' if v[2] else ';')}' if v[0] else '', v[3], pos if v[0] else 0)
                if not primary:
                    self.emit('')
                    def emitHeader() -> None:
                        if 'conds' in s.custom:
                            for k in s.custom['conds']: self.emit(f'var {k} = false;')
                    def emitBlock(inits: list) -> None:
                        for x in inits:
                            match x:
                                case str(): self.emit(x)
                                case Class.Comment(): self.emit(f'// {x.comment}')
                                case Class.Code(): self.emit(x.initcw[CS])
                                case Class.If():
                                    if not x.initcw[CS]: emitBlock(x.inits)
                                    else:
                                        with self.block(before=f'{x.initcw[CS]}'): emitBlock(x.inits)
                                case Class.Value(): self.emit(f'{x.initcw[CS]}{' // ' + x.tags if x.tags else ''}')
                    constArg = s.custom['constArg'][CS] if 'constArg' in s.custom else ''
                    constBase = s.custom['constBase'][CS] if 'constBase' in s.custom else 'r'
                    with self.block(before=f'public {s.namecw[CS].replace('<T>', '')}({s.init[0][CS]}{constArg}){' : base(' + constBase + ')' if s.inherit else ''}'): emitHeader(); emitBlock(s.inits)
                if 'consts' in s.custom:
                    for x in s.custom['consts']:
                        if x[CS]: self.emit(x[CS])
                for x in s.methods: self.emit_raw(x.body)
            elif self.ex == PY:
                if s.struct and s.struct != 'x': self.emit(f'struct = (\'{s.struct[0]}\', {s.struct[1]})')
                if not primary: 
                    for k, v in fieldItems:
                        if v[0] or v[3]: self.emit_with_comment(f'{k[PY]}: {v[0][PY]}{' = ' + v[2][PY] if v[2] else ''}' if v[0] else '', v[3], pos if v[0] else 0)
                    self.emit('')
                def emitHeader() -> None:
                    if 'conds' in s.custom:
                        for k in s.custom['conds']: self.emit(f'{k}: bool = False')
                def emitBlock(inits: list) -> None:
                    # if not inits: print('HERE'); return
                    for x in inits:
                        match x:
                            case str(): self.emit(x)
                            case Class.Comment(): self.emit(f'# {x.comment}')
                            case Class.Code(): self.emit(x.initcw[PY])
                            case Class.If():
                                if not x.initcw[PY]: emitBlock(x.inits)
                                else:
                                    with self.block(before=f'{x.initcw[PY]}:'): emitBlock(x.inits)
                            case Class.Value(): self.emit(f'{x.initcw[PY]}{' # ' + x.tags if x.tags else ''}')
                constArg = s.custom['constArg'][PY] if 'constArg' in s.custom else ''
                with self.block(before=f'def __init__(self, {s.init[0][PY]}{constArg}):'):
                    inheritArgs = 'b.f' if s.name == 'Header' else 'r'
                    if s.inherit: self.emit(f'super().__init__({inheritArgs})')
                    if 'consts' in s.custom:
                        for x in s.custom['consts']:
                            if x[PY]: self.emit(x[PY])
                    if primary:
                        if fieldItems or s.inherit:
                            for k, v in fieldItems:
                                self.emit_with_comment(f'{v[1][PY]}' if v[1] else '', v[3], pos)
                        else: self.emit('pass')
                    else: emitHeader(); emitBlock(s.inits)
                    for x in s.methods: self.emit_raw(x.body)
        self.emit()
    def writeBlocks(self, blocks: list[object]) -> None:
        self.writeCustom('_header')
        for s in blocks:
            if s == 'X': self.region('X'); self.writeCustom('X'); continue
            match s.name:
                case 'AccumFlags': self.endregion(); self.region('Enums')
                case 'SizedString': self.endregion(); self.region('Compounds')
                case 'NiObject':
                    self.endregion(); self.region('NIF Objects')
                    self.emit(f'{self.symbolComment} These are the main units of data that NIF files are arranged in.')
                    self.emit(f'{self.symbolComment} They are like C classes and can contain many pieces of data.')
                    self.emit(f'{self.symbolComment} The only differences between these and compounds is that these are treated as object types by the NIF format and can inherit from other classes.')
                    self.emit()
                case 'BSDistantObjectLargeRefExtraData': self.endregion()
            match s:
                case Enum(): self.writeEnum(s)
                case Class(): self.writeClass(s)
        self.writeCustom('_footer')
    def write(self, xml: object, path: str):
        print(f'build {path}')
        self.writeBlocks(self.parse(xml))
        with open(path, 'w', encoding='utf-8') as f:
            f.write(self.render())
    def parse(self, xml: object) -> None:
        root = xml.getroot()
        blocks = ['X']
        # first pass
        for e in root:
            name = e.attrib.get('name')
            match e.tag:
                case 'version' | 'basic': continue
                case 'enum' | 'bitflags': blocks.append(Enum(e, self))
                case 'compound' | 'niobject': blocks.append(Class(e, self))
        # code pass
        self.types['TexCoord'][MD] = lambda x: (f'new({x})', f'TexCoord({x})')
        for s in blocks:
            match s:
                case Class(): s.code(self)
        return blocks

#endregion

#region Objects
class Elem:
    def __init__(self, attrib: dict):
        self.text = None
        self.attrib = attrib
class Enum:
    comment: str
    flag: bool
    name: str
    namers: dict[str, str]
    storage: str
    values: list[tuple[str, str, str]]
    def __init__(self, e: object, cw: XmlCodeWriter):
        self.comment = e.text.strip().replace('        ', '') if e.text else None
        self.flag = e.tag == 'bitflags'
        self.name = e.attrib['name']
        self.export = cw.export(self.name)
        cwname = fmt_cwname(self.name)
        self.namecw = (cwname, cwname)
        self.tags = cw.tags(self.name)
        self.storage = e.attrib['storage']
        self.values = [(fmt_cwname(e.attrib['name']), e.attrib['value'], None, e.text.strip() if e.text else None) for e in e]
        self.namers = {x[1]:f'{x[0]}' if x[0][0].isdigit() else f'{cwname}.{x[0]}' for x in self.values}
        self.flags = 'E'
        self.custom = cw.customs[self.name] if self.name in cw.customs else {}
        # types
        match self.storage:
            case 'uint': self.init = [f'({self.namecw[CS]})r.ReadUInt32()', f'{self.namecw[PY]}(r.readUInt32())']
            case 'ushort': self.init = [f'({self.namecw[CS]})r.ReadUInt16()', f'{self.namecw[PY]}(r.readUInt16())']
            case 'byte': self.init = [f'({self.namecw[CS]})r.ReadByte()', f'{self.namecw[PY]}(r.readByte())']
        if self.name not in cw.types: cw.types[self.name] = [
            self, self.namecw, lambda x: (f'({self.name}){x}', f'{x}') if x[0].isdigit() else (f'{self.name}.{x}', f'{self.name}.{x}'), (self.init[CS], self.init[PY]),
            lambda c: (f'r.ReadFArray(z => {self.init[CS]}, {c})', f'r.readFArray(lambda z: {self.init[PY]}, {c})')]
    def __repr__(self): return f'enum: {self.name}'
class Class:
    class Comment:
        def __init__(self, root: Class, s: str):
            self.comment: str = s
            self.kind = None
            self.vercond = None
            self.cond = None
            self.condcw = [None, None]
            self.typecw = None
            self.initcw = None
            self.defaultcw: tuple = None
            self.name: str = s
            self.namecw: tuple = (self.name, self.name)
        def __repr__(self): return f'#: {self.comment}'
        def code(self, lx: object, root: Class, parent: object, cw: XmlCodeWriter) -> None: pass
    class Attrib:
        def __init__(self, body: str):
            self.body: str = body
        def __repr__(self): return f'Attrib'
    class Method:
        def __init__(self, body: str):
            self.body: str = body
        def __repr__(self): return f'Method'
    class Code:
        def __init__(self, root: Class, initcw: tuple):
            self.name: str = ':code:'
            self.kind = None
            self.vercond = None
            self.cond = None
            # self.condcw = [None, None]
            self.initcw: tuple = initcw
            self.namecw = self.typecw = self.defaultcw = self.comment = None
        def __repr__(self): return f'Code'
        def code(self, lx: object, root: Class, parent: object, cw: XmlCodeWriter) -> None: pass
    class If:
        def __init__(self, root: Class, comment: str, field: object, inits: list[list[object]], kind: str):
            self.comment: str = comment
            self.kind: str = kind
            self.name: str = ':if:'
            self.vercond: str = field.vercond if field else None
            self.cond: str = field.cond if field else None
            self.extcond: str = None
            self.namecw = self.typecw = self.defaultcw = self.comment = None
            self.inits: list[object] = (inits[0][inits[1]:inits[2]] if len(inits) == 3 else inits) if inits else []
            if inits and len(inits) == 3: del inits[0][inits[1]:inits[2]]
        def __repr__(self): return f'{self.name} {self.kind or ''}{'{' + (self.vercond or '') + (self.cond or '') + '}' if self.vercond or self.cond else ''}\n'
        def code(self, lx: object, root: Class, parent: object, cw: XmlCodeWriter) -> None:
            lx2 = None
            for x in self.inits:
                if not isinstance(x, str): x.code(lx2, root, self, cw); lx2 = x
            self.typecw = None
            # init
            c = self.condcw = root.addcond(parent, self, cw)
            if self.kind != 'else' and not c[0]: self.initcw = [None, None]; return
            match self.kind:
                case 'if': self.initcw = [f'if ({c[CS]})', f'if {c[PY]}']
                case 'elseif': self.initcw = [f'else if ({c[CS]})', f'elif {c[PY]}']
                case 'else': self.initcw = [f'else', f'else']
                case 'switch': self.initcw = [f'switch ({c[CS].split(' == ')[0]})', f'match {c[PY].split(' == ')[0]}']
                case _: raise Exception(f'Unknown {self.kind}')
            self.initcw = root.rename(self.initcw, cw)
    class Value:
        def __init__(self, root: Class, e: object):
            if e == None: return
            self.comment: str = e.text.strip().replace('        ', '') if e.text else None if e.text else None
            self.name: str = e.attrib['name'].replace('?', '')
            self.suffix: str = e.attrib.get('suffix') or ''
            cwname = self.name.replace('?', '').replace(' ', '')
            if self.name.replace(' ', '') == root.name or self.suffix: cwname += f'_{self.suffix}'
            if ' ' in self.name: root.namers[self.name] = cwname
            self.type: str = e.attrib['type']
            if 'type' in root.custom and self.name in root.custom['type']: self.type = root.custom['type'][self.name]
            self.namecw = (cwname, fmt_py(cwname))
            self.typecw = None
            self.initcw = None
            self.elsecw = None
            self.defaultcw = None
            self.calculated = e.attrib.get('calculated') == '1'
            self.template: str = z if (z := e.attrib.get('template')) != 'TEMPLATE' else None
            self.default: str = e.attrib.get('default')
            self.arg: str = e.attrib.get('arg')
            self.arr1: str = z if (z := e.attrib.get('arr1')) else None
            self.arr2: str = z if (z := e.attrib.get('arr2')) else None
            self.arr2x: str = None
            cond = z if (z := e.attrib.get('cond')) else ''
            cond = verReplace(cond.strip()).replace('User Version 2 ', 'ZUV2 ').replace('User Version ', 'ZUV ').replace('Version ', 'ZV ')
            self.cond: str = cond
            self.ver1: int = ver2Num(z) if (z := e.attrib.get('ver1')) else None
            self.ver2: int = ver2Num(z) if (z := e.attrib.get('ver2')) else None
            if self.ver1 and self.ver2: vercond = f'ZV == 0x{self.ver1:08X}' if self.ver1 == self.ver2 else f'ZV >= 0x{self.ver1:08X} && ZV <= 0x{self.ver2:08X}'
            elif self.ver2: vercond = f'ZV <= 0x{self.ver2:08X}'
            elif self.ver1: vercond = f'ZV >= 0x{self.ver1:08X}'
            else: vercond = ''
            vercond += f'{' && ' if vercond else ''}{z}' if (z := e.attrib.get('vercond')) else ''
            vercond += f'{' && ' if vercond else ''}(ZUV == {z})' if (z := e.attrib.get('userver')) else ''
            vercond += f'{' && ' if vercond else ''}(ZUV2 == {z})' if (z := e.attrib.get('userver2')) else ''
            vercond = verReplace(vercond).replace('User Version 2 ', 'ZUV2 ').replace('User Version ', 'ZUV ').replace('Version ', 'ZV ')
            self.vercond: str = vercond
            self.extcond: str = None
            self.kind = None
            self.tags = 'calculated' if self.calculated else ''
            # if root.name == 'Footer': print(self.template)
        def __repr__(self): return f'{self.name}{' {' + (self.cond or '') + (self.vercond or '') + (self.extcond or '') + '}' if self.cond or self.vercond or self.extcond else ''}{';' + self.kind if self.kind else ''}\n'
        def code(self, lx: object, root: Class, parent: object, cw: XmlCodeWriter) -> None:
            flags = root.flags
            primary = 'P' in flags
            # totype
            if not self.typecw:
                cs = cw.types[self.type][MN][CS].replace('<T>', f'<{self.template or 'T'}>'); py = cw.types[self.type][MN][PY].replace('<T>', f'<{self.template or 'T'}>')
                if self.template: cs = cs.replace('{T}', self.template); py = py.replace('{T}', self.template)
                if self.arr1 and self.arr2: self.typecw = [f'byte[][]', f'list[bytearray]'] if self.type == 'byte' else [f'{cs}[][]', f'list[list[{py}]]']
                elif self.arr1: self.typecw = [f'byte[]', f'bytearray'] if self.type == 'byte' else [f'{cs}[]', f'list[{py}]']
                else: self.typecw = [cs, py]
                # if root.name == 'Footer': print(self.typecw)
            # toinit
            if not self.initcw:
                # type
                if not self.calculated:
                    cs = cw.types[self.type][MI][CS].replace('<T>', f'<{self.template or 'T'}>'); py = cw.types[self.type][MI][PY].replace('<T>', f'<{self.template or 'T'}>')
                    if self.arr1:
                        arr1 = self.arr2 or self.arr1
                        if not cw.types[self.type][MA]: print(f'{self.type}: missing array func')
                        cs = cw.types[self.type][MA](root.cond(parent, arr1, cw)[CS])[CS].replace('<T>', f'<{self.template or 'T'}>'); py = cw.types[self.type][MA](root.cond(parent, arr1, cw)[PY])[PY].replace('<T>', f'<{self.template or 'T'}>')
                        if arr1.startswith('L'): cs = cs.replace('Read', f'Read{arr1}', 1).replace(f'{arr1})', ')').replace(', )', ')'); py = py.replace('read', f'read{arr1}', 1).replace(f'{arr1})', ')').replace(', )', ')')
                        if self.arr2:
                            if self.arr2x == 'i': cs = f'r.ReadFArray((k, i) => {cs[:-1]}[i]), {self.arr1})'; py = f'r.readFArray(lambda k, i: {py}, {self.arr1})'
                            else: cs = f'r.ReadFArray(k => {cs}, {self.arr1})'; py = f'r.readFArray(lambda k: {py}, {self.arr1})'
                    if self.template: cs = cs.replace('{T}', self.template); py = py.replace('{T}', self.template)
                    if self.arg:
                        arg = root.custom['arg'] if 'arg' in root.custom else root.namers[self.arg] if self.arg in root.namers else self.arg
                        cs = cs.replace('ARG', arg); py = py.replace('ARG', arg)
                elif 'calculated' in root.custom: (cs, py) = root.custom['calculated'](self.name)
                else: raise Exception(f'calculated? {root.name}')
                # if root.name == 'Footer': print(cs)

                # default
                if not self.defaultcw: self.defaultcw = cw.types[self.type][MD](self.default) if self.default else None
                # if
                c = self.condcw = root.addcond(parent, self, cw)
                # kind
                if not self.kind:
                    self.kind = 'case' if parent and parent.kind == 'switch' else \
                        'if' if self.condcw[0] else \
                        ''
                    if primary and self.kind == 'if': self.kind = '?:'
                # x
                if self.condcw[0] and lx and lx.condcw[0] and op_flip(self.condcw[0]) == lx.condcw[0]: self.kind = 'else'

                # else
                elsecw = self.elsecw or self.defaultcw or ['default', 'None']
                match self.kind:
                    case 'var': self.initcw = \
                        [f'var {self.namecw[CS]} = {c[CS]} ? {cs} : {elsecw[CS]};', f'{self.namecw[PY]} = {py} if {c[PY]} else {elsecw[PY]}'] if c[0] else \
                        [f'var {self.namecw[CS]} = {cs};', f'{self.namecw[PY]}{': ' + self.typecw[PY] if primary else ''} = {py}']
                    case '?:': self.initcw = [f'{self.namecw[CS]} = {c[CS]} ? {cs} : {elsecw[CS]};', f'self.{self.namecw[PY]}{': ' + self.typecw[PY] if primary else ''} = {py} if {c[PY]} else {elsecw[PY]}']
                    case '?+': self.initcw = [f'{self.namecw[CS]} = {c[CS]} ? {cs}', f'self.{self.namecw[PY]}{': ' + self.typecw[PY] if primary else ''} = {py} if {c[PY]} else \\']
                    case ':+': self.initcw = [f'    : {c[CS]} ? {cs}', f'    {py} if {c[PY]} else \\']
                    case ':': self.initcw = [f'    : {c[CS]} ? {cs} : {elsecw[CS]};', f'    {elsecw[PY]}']
                    case 'if': self.initcw = [f'if ({c[CS]}) {self.namecw[CS]} = {cs};', f'if {c[PY]}: self.{self.namecw[PY]} = {py}']
                    case 'elseif': self.initcw = [f'else if ({c[CS]}) {self.namecw[CS]} = {cs};', f'elif {c[PY]}: self.{self.namecw[PY]} = {py}']
                    case 'else': self.initcw = [f'else {self.namecw[CS]} = {cs};', f'else: self.{self.namecw[PY]} = {py}']
                    case 'case': self.initcw = [f'case {c[CS].split(' == ')[1]}: {self.namecw[CS]} = {cs}; break;', f'case {c[PY].split(' == ')[1]}: self.{self.namecw[PY]} = {py}']
                    case _: self.initcw = [f'{self.namecw[CS]} = {cs};', f'self.{self.namecw[PY]}{': ' + self.typecw[PY] if primary else ''} = {py}']
                self.initcw = root.rename(self.initcw, cw)

    comment: str
    abstract: bool = False
    inherit: str = None
    template: str
    name: str
    namers: dict[str, str]
    namecw: tuple[str]
    tags: str
    struct: str
    values: list[Value]
    condFlag: int
    flags: str
    fields: list[tuple]
    attribs: list[object]
    methods: list[object]
    def __init__(self, e: object, cw: XmlCodeWriter):
        niobject: bool = e.tag == 'niobject'
        self.comment = e.text.strip().replace('        ', '') if e.text else None
        if niobject:
            self.abstract = e.attrib.get('abstract') == '1'
            self.inherit = e.attrib.get('inherit')
        self.template = e.attrib.get('istemplate') == '1'
        self.name = e.attrib['name']
        self.custom = cw.customs[self.name] if self.name in cw.customs else {}
        self.export = cw.export(self.name)
        cwname = fmt_cwname(self.name)
        self.namers = cw.types[self.inherit][MS].namers.copy() if self.inherit in cw.types else {}
        self.namecw = (f'{cwname}<T>' if self.template else cwname, f'{cwname}[T]' if self.template else cwname)
        self.tags = cw.tags(self.name)
        self.struct = cw.struct.get(self.name)
        self.values = [Class.Value(self, e) for e in e]
        self.condFlag = 0
        self.flags = ''
        self.attribs = []
        self.methods = []
        self.process()
        # flags
        if not self.flags: self.flags = self.custom['flags'] if 'flags' in self.custom else \
            'C' if (self.condFlag & 2) or (self.condFlag & 4) or (niobject and self.values) or self.template else \
            'P'
        # types
        constNew = self.custom['constNew'] if 'constNew' in self.custom else ('', '')
        self.init = (
            ['NiReader r', 'r: NiReader'],
            ['r', 'r'],
            [f'new {self.namecw[CS]}(r{constNew[CS]})', f'{self.namecw[PY]}(r{constNew[PY]})'])
        manyLambda = lambda c: (f'r.ReadFArray(z => {self.init[2][CS]}, {c})', f'r.readFArray(lambda z: {self.init[2][PY]}, {c})')
        if self.name in cw.struct:
            self.init = (self.init[0], self.init[1], [f'r.ReadS<{self.namecw[CS]}>()', f'r.readS({self.namecw[PY]})'])
            manyLambda = lambda c: (f'r.ReadSArray<{self.namecw[CS]}>({c})', f'r.readSArray<{self.namecw[CS]}>({c})')
        if self.name not in cw.types: cw.types[self.name] = [
            self, self.namecw, lambda x: (x, x), (self.init[2][CS], self.init[2][PY]),
            manyLambda]
    def __repr__(self): return f'class: {self.name}'
    def addcond(self, parent: object, s: object, cw: XmlCodeWriter) -> list[str]:
        # if self.name == 'bhkRigidBody' and s.name == 'Unknown Int 2': print(s.extcond)
        if s.cond and s.vercond and s.extcond: return self.cond(parent, f'{s.vercond} && {s.cond} && {s.extcond}', cw)
        elif s.cond and s.vercond: return self.cond(parent, f'{s.vercond} && {s.cond}', cw)
        elif s.cond and s.extcond: return self.cond(parent, f'{s.cond} && {s.extcond}', cw)
        elif s.vercond and s.extcond: return self.cond(parent, f'{s.vercond} && {s.extcond}', cw)
        elif s.cond: return self.cond(parent, s.cond, cw)
        elif s.vercond: return self.cond(parent, s.vercond, cw)
        elif s.extcond: return self.cond(parent, s.extcond, cw)
        elif s.kind == 'switch': return self.cond(parent, s.inits[0].cond, cw)
        else: return [None, None]
    def cond(self, parent: object, s: str, cw: XmlCodeWriter) -> list[str]:
        if 'cond' in self.custom: s = self.custom['cond'](parent, s, cw)
        if s.startswith('(') and s.endswith(')') and '(' not in s[1:]: s = s[1:-1]
        cs = s; py = s.replace('||', 'or').replace('&&', 'and').replace('!=', 'Z=').replace('!', 'not ').replace('Z=', '!=')
        if 'condcs' in self.custom: cs = self.custom['condcs'](parent, cs, cw)
        if 'condpy' in self.custom: py = self.custom['condpy'](parent, py, cw)
        if 'B32:' in s: z = cs[cs.index('B32:'):].split(' &&')[0]; cs = cs.replace(z, 'r.ReadBool32()'); py = py.replace(z, 'r.readBool32()')
        # if 'U32:' in s: z = cs.split(' ==')[0]; cs = cs.replace(z, '(u0 = r.ReadUInt32())'); py = py.replace(z, '(u0 := r.readUInt32())')
        return [cs, py]
    def rename(self, s: list[str], cw: XmlCodeWriter) -> list[str]:
        cs = s[CS]; py = s[PY]
        for k,v in self.namers.items(): cs = cs.replace(k, v); py = py.replace(k, f'self.{fmt_py(v)}')
        cs = cs.replace('ZV ', 'r.V ').replace('ZUV2 ', 'r.UV2 ').replace('ZUV ', 'r.UV ')
        py = py.replace('ZV ', 'r.v ').replace('ZUV2 ', 'r.uv2 ').replace('ZUV ', 'r.uv ')
        return [cs, py]
    def process(self):
        values = self.values
        # if not values: self.inits = values.copy(); return

        # value-collapse
        for i in range(len(values) - 1, 0, -1):
            this = values[i]; prev = values[i-1]
            name = prev.name; type = prev.type
            if name.startswith('Num') or name.startswith('Count'):
                count = len([x for x in values if x.arr1 == name])
                arr = this.arr1 == name
                if arr and count == 1:
                    match prev.type:
                        case 'uint' | 'int': this.arr1 = 'L32'
                        case 'ushort' | 'short': this.arr1 = 'L16'
                        case 'byte': this.arr1 = 'L8'
                    del values[i-1]
            elif name.startswith('Has') and type == 'bool': # and not prev.cond:
                found = False
                vercond = prev.vercond
                prev.kind = 'var'
                for j in range(i, len(values)):
                    k = values[j]
                    if name == k.cond and k.vercond.startswith(vercond): found = True
                    else: break
                if not found: continue
                for s in values[i:j+1]: s.cond = s.cond.replace(name, f'B32:{name}')
                del values[i-1]
                    
        # condFlag
        for s in values:
            if s.vercond or 'ZV ' in s.cond or 'ZUV ' in s.cond or 'ZUV2 ' in s.cond: self.condFlag |= 1
            if 'arg ' in s.cond or 'ARG ' in s.cond: self.condFlag |= 2
            for k,v in self.namers.items():
                if k in s.vercond or k in s.cond or (s.arr1 and k in s.arr1): self.condFlag |= 2

        # custom
        if 'values' in self.custom: self.custom['values'](self, values)
        inits = self.inits = values.copy()
        if 'inits' in self.custom: self.custom['inits'](self, inits)

        # collapse multi
        ifx = None; ifc = 'A'
        def addIf(i, t, ifx, ifc):
            self.condFlag |= 4
            if i > 0 and len(ifx.inits) == 1 and self.name == 'NiObjectNET':
                k = ifx.inits[0]
                if ifc == 'X' or ifc == 'C': k.cond = t.cond
                if ifc == 'X' or ifc == 'V': k.vercond = t.vercond
                inits.insert(i+1, k)
                return
            if ifc == 'X' or ifc == 'C': t.cond = None
            if ifc == 'X' or ifc == 'V': t.vercond = None
            ifx.inits.insert(0, t); del inits[i]
            inits.insert(i, ifx)
            # if 'if' in self.custom: py = self.custom['if'](t, ifx)
        for i in range(len(inits) - 1, 0, -1):
            t = inits[i]; p = inits[i-1]; j = i
            ifn = 'X' if t.cond and t.cond == p.cond and t.vercond and t.vercond == p.vercond else 'C' if t.cond and t.cond == p.cond else 'V' if t.vercond and t.vercond == p.vercond else 'A'
            if ifx and ifc != ifn: addIf(i, t, ifx, ifc); ifx = None
            if ifn != 'A':
                if not ifx: ifx = Class.If(self, None, None, None, 'if')
                if ifn == 'X' or ifn == 'C': ifx.cond = t.cond; t.cond = None
                if ifn == 'X' or ifn == 'V': ifx.vercond = t.vercond; t.vercond = None
                ifx.inits.insert(0, t); del inits[i]
            ifc = ifn
        if ifx: addIf(0, p, ifx, ifc); ifx = None
        if 'kind' in self.custom:
            for k,v in self.custom['kind'].items(): inits[k].kind = v
        # if 'inits2' in self.custom: self.custom['inits2'](self, inits)
    def code(self, cw: XmlCodeWriter):
        lx = None
        for x in self.inits:
            if not isinstance(x, str): x.code(lx, self, None, cw); lx = x
        if 'code' in self.custom: py = self.custom['code'](self)
        values = [x for x in self.values if x.name and not isinstance(x, str) and not (x.kind or '').startswith('var')]
        fields = {x.namecw: (x.typecw, x.initcw, x.defaultcw, x.comment) for x in reversed(values)}
        self.fields = {x.namecw: fields[x.namecw] for x in values}

#endregion
