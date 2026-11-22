from __future__ import annotations
import os
import struct
from io import BytesIO
from openx.core import BinaryReader
from gamex import ArcBinaryT, FileSource, MetaInfo, IHaveMetaInfo
from gamex.families.Uncore.formats.compression import decompressZlib, decompressZstd

def _ascii(b, stop: bool, trim: bool) -> str:
    raw = bytes(b)
    if stop:
        n = raw.find(b'\x00')
        if n >= 0: raw = raw[:n]
    else:
        n = len(raw)
        while n > 0 and raw[n - 1] == 0: n -= 1
        raw = raw[:n]
    s = ''.join(chr(c) if c < 128 else '?' for c in raw)
    return s.strip() if trim else s

def _utf8Trim(b) -> str:
    raw = bytes(b)
    n = len(raw)
    while n > 0 and raw[n - 1] == 0: n -= 1
    return raw[:n].decode('utf-8', 'replace')

def _bifPath(binPath: str, rel: str) -> str:
    dir_ = os.path.dirname(binPath or '')
    full = os.path.abspath(os.path.normpath(os.path.join(dir_, rel)))
    if os.path.isfile(full): return full
    parent = os.path.dirname(dir_)
    folder = os.path.basename(dir_)
    slash = rel.replace('\\', '/')
    if parent and folder and slash.lower().startswith(folder.lower() + '/'):
        return os.path.abspath(os.path.normpath(os.path.join(parent, rel)))
    return full

def _i16(v: int) -> int:
    v &= 0xFFFF
    return v - 0x10000 if v & 0x8000 else v

def _i32(v: int) -> int:
    v &= 0xFFFFFFFF
    return v - 0x100000000 if v & 0x80000000 else v

def _slice(data, offset, size) -> bytes:
    o = int(offset); n = int(size)
    if o < 0 or n < 0 or o + n > len(data): raise Exception('Offset and length were out of bounds.')
    return bytes(data[o:o + n])

# BIFC V1.0. Cap the copy at the stated uncompressed size.
def _inflateBifc(r: BinaryReader) -> bytes:
    version = r.readUInt32()
    if version != 0x302E3156: raise Exception('BAD MAGIC')
    uncompressed = r.readUInt32()
    if uncompressed > 0x7FFFFFFF: raise Exception('zlib')
    output = bytearray()
    while len(output) < uncompressed:
        declen = r.readUInt32()
        complen = r.readUInt32()
        if declen == 0 or complen == 0 or declen > 0x7FFFFFFF or complen > 0x7FFFFFFF: raise Exception('zlib')
        chunk = decompressZlib(r, complen, declen)
        if len(chunk) < declen: raise Exception('zlib')
        take = declen if len(output) + declen <= uncompressed else uncompressed - len(output)
        output += chunk[:take]
    return bytes(output)

#region Binary_Aurora

# NWN / KotOR / Jade KEY+BIF. 16-byte resref. Index is the low 20 bits.
# Bif index is bits 31-20. Fixed (tileset) BIFF rows are not read.
class Binary_Aurora(ArcBinaryT):
    KEY_MAGIC = 0x2059454B
    KEY_VERSION = 0x20203156
    BIFF_MAGIC = 0x46464942
    BIFF_VERSION = 0x20203156
    BIFC_MAGIC = 0x43464942
    BIFC_VERSION = 0x302E3156
    EXTS = {
        0x0000: '.res', 0x0001: '.bmp', 0x0002: '.mve', 0x0003: '.tga', 0x0004: '.wav',
        0x0006: '.plt', 0x0007: '.ini', 0x0008: '.mp3', 0x0009: '.mpg', 0x000A: '.txt', 0x000B: '.xml',
        0x07D0: '.plh', 0x07D1: '.tex', 0x07D2: '.mdl', 0x07D3: '.thg',
        0x07D5: '.fnt',
        0x07D7: '.lua', 0x07D8: '.slt', 0x07D9: '.nss', 0x07DA: '.ncs', 0x07DB: '.mod', 0x07DC: '.are',
        0x07DD: '.set', 0x07DE: '.ifo', 0x07DF: '.bic', 0x07E0: '.wok', 0x07E1: '.2da', 0x07E2: '.tlk',
        0x07E6: '.txi', 0x07E7: '.git', 0x07E8: '.bti', 0x07E9: '.uti', 0x07EA: '.btc', 0x07EB: '.utc',
        0x07ED: '.dlg', 0x07EE: '.itp', 0x07EF: '.btt', 0x07F0: '.utt', 0x07F1: '.dds', 0x07F2: '.bts',
        0x07F3: '.uts', 0x07F4: '.ltr', 0x07F5: '.gff', 0x07F6: '.fac', 0x07F7: '.bte', 0x07F8: '.ute',
        0x07F9: '.btd', 0x07FA: '.utd', 0x07FB: '.btp', 0x07FC: '.utp', 0x07FD: '.dft', 0x07FE: '.gic',
        0x07FF: '.gui', 0x0800: '.css', 0x0801: '.ccs', 0x0802: '.btm', 0x0803: '.utm', 0x0804: '.dwk',
        0x0805: '.pwk', 0x0806: '.btg',
        0x0808: '.jrl', 0x0809: '.sav', 0x080A: '.utw', 0x080B: '.4pc', 0x080C: '.ssf',
        0x080F: '.bik', 0x0810: '.ndb', 0x0811: '.ptm', 0x0812: '.ptt', 0x0813: '.ncm', 0x0814: '.mfx',
        0x0815: '.mat', 0x0816: '.mdb', 0x0817: '.say', 0x0818: '.ttf', 0x0819: '.ttc', 0x081A: '.cut',
        0x081B: '.ka', 0x081C: '.jpg', 0x081D: '.ico', 0x081E: '.ogg', 0x081F: '.spt', 0x0820: '.spw',
        0x0821: '.wfx', 0x0822: '.ugm', 0x0823: '.qdb', 0x0824: '.qst', 0x0825: '.npc', 0x0826: '.spn',
        0x0827: '.utx', 0x0828: '.mmd', 0x0829: '.smm', 0x082A: '.uta', 0x082B: '.mde', 0x082C: '.mdv',
        0x082D: '.mda', 0x082E: '.mba', 0x082F: '.oct', 0x0830: '.bfx', 0x0831: '.pdb',
        0x0832: '.TheWitcherSave', 0x0833: '.pvs', 0x0834: '.cfx', 0x0835: '.luc',
        0x0837: '.prb', 0x0838: '.cam', 0x0839: '.vds', 0x083A: '.bin', 0x083B: '.wob', 0x083C: '.api',
        0x083D: '.properties', 0x083E: '.png',
        0x270B: '.big', 0x270D: '.erf', 0x270E: '.bif', 0x270F: '.key',
    }

    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        files = []
        magic = r.readUInt32()
        source.magic = magic
        if magic == self.KEY_MAGIC: self.readKey(source, r, files)
        elif magic == self.BIFF_MAGIC: self.readBiff(source, r, files, None, None, None, True)
        elif magic == self.BIFC_MAGIC: self.readBifc(source, r, files, None, None, None, True)
        else: raise Exception(f'Unknown File Type {magic:08X}')
        source.files = files

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        if isinstance(file.tag, str): return self.readPath(file.tag, file.offset, file.fileSize)
        if isinstance(source.tag, (bytes, bytearray)): return BytesIO(_slice(source.tag, file.offset, file.fileSize))
        if source.magic == self.BIFC_MAGIC:
            r.seek(0)
            magic = r.readUInt32()
            if magic != self.BIFC_MAGIC: raise Exception(f'Unknown File Type {magic:08X}')
            return BytesIO(_slice(_inflateBifc(r), file.offset, file.fileSize))
        r.seek(file.offset)
        return BytesIO(r.readBytes(file.fileSize))

    def ext(self, resType: int) -> str: return self.EXTS.get(resType, '')

    def readKey(self, source: BinaryArchive, r: BinaryReader, files: list) -> None:
        version = r.readUInt32()
        if version != self.KEY_VERSION: raise Exception('BAD MAGIC')
        source.version = version
        bifCount = r.readUInt32()
        keyCount = r.readUInt32()
        filesOffset = r.readUInt32()
        keysOffset = r.readUInt32()
        r.readUInt32(); r.readUInt32(); r.readBytes(32)

        raw = []
        r.seek(filesOffset)
        for _ in range(bifCount):
            r.readUInt32()
            nameOfs = r.readUInt32()
            nameLen = r.readUInt16()
            r.readUInt16()
            raw.append((nameOfs, nameLen))
        bifs = [self.readName(r, nameOfs, nameLen) for nameOfs, nameLen in raw]

        byBif = {}
        r.seek(keysOffset)
        for _ in range(keyCount):
            resref = _ascii(r.readBytes(16), True, True)
            resType = r.readUInt16()
            id_ = r.readUInt32()
            bif = (id_ >> 20) & 0xFFF
            index = id_ & 0xFFFFF
            map_ = byBif.get(bif)
            if map_ is None: map_ = byBif[bif] = {}
            if index not in map_: map_[index] = (resref, resType)

        for i, rel in enumerate(bifs):
            full = _bifPath(source.binPath, rel)
            if not os.path.isfile(full): continue
            self.openBif(source, files, full, rel, byBif.get(i, {}))

    def openBif(self, source, files, full, rel, map_) -> None:
        with BinaryReader(open(full, 'rb')) as br:
            magic = br.readUInt32()
            if magic == self.BIFF_MAGIC: self.readBiff(source, br, files, rel, full, map_, False)
            elif magic == self.BIFC_MAGIC: self.readBifc(source, br, files, rel, full, map_, False)
            else: raise Exception(f'Unknown File Type {magic:08X}')

    def readBifc(self, source, r, files, rel, tagPath, map_, setVersion: bool) -> None:
        inflated = _inflateBifc(r)
        if setVersion and not isinstance(source.tag, dict): source.tag = inflated
        with BinaryReader(BytesIO(inflated)) as inner:
            magic = inner.readUInt32()
            if magic != self.BIFF_MAGIC: raise Exception(f'Unknown File Type {magic:08X}')
            self.readBiff(source, inner, files, rel, tagPath, map_, setVersion)

    def readBiff(self, source, r, files, rel, tagPath, map_, setVersion: bool) -> None:
        version = r.readUInt32()
        if version != self.BIFF_VERSION: raise Exception('BAD MAGIC')
        if setVersion: source.version = version
        fileCount = r.readUInt32()
        r.readUInt32()
        fileTableOffset = r.readUInt32()
        r.seek(fileTableOffset)
        for i in range(fileCount):
            fileId = r.readUInt32()
            offset = r.readUInt32()
            size = r.readUInt32()
            resType = r.readUInt32()
            if map_ is not None:
                res = map_.get(fileId & 0xFFFFF)
                if res is None: continue
                resref, resType = res
                files.append(FileSource(path=f'{rel}/{resref}{self.ext(resType)}', offset=offset, fileSize=size, tag=tagPath))
            else: files.append(FileSource(path=f'{i:04d}{self.ext(resType)}', offset=offset, fileSize=size))

    def readPath(self, path: str, offset: int, size: int) -> BytesIO:
        with BinaryReader(open(path, 'rb')) as br:
            magic = br.readUInt32()
            if magic == self.BIFC_MAGIC: return BytesIO(_slice(_inflateBifc(br), offset, size))
            br.seek(offset)
            return BytesIO(br.readBytes(size))

    def readName(self, r: BinaryReader, offset: int, length: int) -> str:
        if length <= 0: return ''
        r.seek(offset)
        return _ascii(r.readBytes(length), True, False).replace('\\', '/')

#endregion

#region Binary_Myp

class Binary_Myp(ArcBinaryT):
    MYP_MAGIC = 0x0050594d

    class MYP_Header:
        _struct = ('<3IQ4I', 36)
        def __init__(self, t):
            (self.magic, self.version, self.bom, self.tableOffset, self.tableCapacity, self.totalFiles, self.unk1, self.unk2) = t
        def verify(self):
            if self.magic != Binary_Myp.MYP_MAGIC: raise Exception('Not a .tor file (Wrong file header)')
            if self.version != 5 and self.version != 6: raise Exception(f'Only versions 5 and 6 are supported, file has {self.version}')
            if self.bom != 0xfd23ec43: raise Exception('Unexpected byte order')
            if self.tableOffset == 0: raise Exception('File is empty')

    class MYP_HeaderFile:
        _struct = ('<Q3IQIH', 34)
        def __init__(self, t):
            (self.offset, self.headerSize, self.packedSize, self.fileSize, self.digest, self.crc, self.compressed) = t

    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        files = source.files = []
        match source.game.id:
            case 'SWTOR':
                from ....resources.Bioware import TOR
                hashLookup = TOR.hashLookup
            case 'WAR':
                from ....resources.Bioware import WAR
                hashLookup = WAR.hashLookup
            case _: hashLookup = {}

        header = r.readS(self.MYP_Header)
        header.verify()
        source.version = header.version

        tableOffset = header.tableOffset
        while tableOffset != 0:
            r.seek(tableOffset)
            numFiles = r.readInt32()
            if numFiles == 0: break
            tableOffset = r.readInt64()
            headerFiles = r.readSArray(self.MYP_HeaderFile, numFiles)
            for i in range(numFiles):
                headerFile = headerFiles[i]
                if headerFile.offset == 0: continue
                hash_ = headerFile.digest
                path = hashLookup[hash_].replace('\\', '/') if hash_ in hashLookup else f'{hash_:02X}.bin'
                if path.startswith('/'): path = path[1:]
                files.append(FileSource(
                    id = i,
                    path = path,
                    fileSize = headerFile.fileSize,
                    packedSize = headerFile.packedSize,
                    offset = headerFile.offset + headerFile.headerSize,
                    hash = hash_,
                    compressed = headerFile.compressed))

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        if file.fileSize == 0: return BytesIO()
        r.seek(file.offset)
        if file.compressed == 0: return BytesIO(r.readBytes(file.packedSize))
        if source.version == 6: return BytesIO(decompressZstd(r, file.packedSize, file.fileSize))
        return BytesIO(decompressZlib(r, file.packedSize, file.fileSize))

#endregion

#region Binary_Gff

class Binary_Gff(IHaveMetaInfo):
    GFF_VERSION3_2 = 0x322e3356
    GFF_VERSION3_3 = 0x332e3356
    TYPES = {0x20474c44: 'DLG', 0x20424451: 'QDB', 0x20545351: 'QST'}

    class ResourceRef:
        def __init__(self, name): self.name = name

    class LocalizedRef:
        def __init__(self, dialogID, values):
            self.dialogID = dialogID
            self.values = values

    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s): return Binary_Gff(r)

    def __init__(self, r: BinaryReader):
        self.type = r.readUInt32()
        version = r.readUInt32()
        structOffset = r.readUInt32(); structCount = r.readUInt32()
        fieldOffset = r.readUInt32(); fieldCount = r.readUInt32()
        labelOffset = r.readUInt32(); labelCount = r.readUInt32()
        fieldDataOffset = r.readUInt32(); r.readUInt32()
        fieldIndicesOffset = r.readUInt32(); r.readUInt32()
        listIndicesOffset = r.readUInt32(); r.readUInt32()
        if version != self.GFF_VERSION3_2 and version != self.GFF_VERSION3_3: raise Exception('BAD MAGIC')
        r.seek(structOffset)
        structs = []
        counts = []
        dataOffs = []
        index = {}
        for _ in range(structCount):
            id_ = r.readUInt32(); data = r.readUInt32(); count = r.readUInt32()
            s = {}
            structs.append(s); counts.append(count); dataOffs.append(data)
            if id_ == 0: continue
            s['_'] = id_
            if id_ in index: raise Exception('An item with the same key has already been added.')
            index[id_] = s
        r.seek(fieldOffset)
        fields = []
        for _ in range(fieldCount):
            typ = r.readUInt32(); label = r.readUInt32(); data = r.readUInt32()
            fields.append((label, self.fieldValue(r, typ, data, structs, fieldDataOffset, listIndicesOffset)))
        r.seek(labelOffset)
        labels = [_ascii(r.readBytes(16), False, False) for _ in range(labelCount)]
        for i in range(structCount):
            count = counts[i]; data = dataOffs[i]
            if count == 1:
                label, value = fields[data]
                self.addField(structs[i], labels[label], value)
                continue
            r.seek(fieldIndicesOffset + data)
            for _ in range(count):
                label, value = fields[r.readUInt32()]
                self.addField(structs[i], labels[label], value)
        self.root = structs[0]
        self.index = index

    def fieldValue(self, r, typ, data, structs, fieldDataOffset, listIndicesOffset):
        if typ == 0: return data & 0xFF
        if typ == 1: return chr(data & 0xFFFF)
        if typ == 2: return data & 0xFFFF
        if typ == 3: return _i16(data)
        if typ == 4: return data & 0xFFFFFFFF
        if typ == 5: return _i32(data)
        if typ == 8: return struct.unpack('<f', struct.pack('<I', data & 0xFFFFFFFF))[0]
        if typ == 14: return structs[data]
        if typ == 15:
            r.seek(listIndicesOffset + data)
            n = r.readUInt32()
            lst = []
            for _ in range(n):
                idx = r.readUInt32()
                if idx >= len(structs): raise Exception()
                lst.append(structs[idx])
            return lst
        r.seek(fieldDataOffset + data)
        if typ == 6: return r.readUInt64()
        if typ == 7: return r.readInt64()
        if typ == 9: return r.readDouble()
        if typ == 10: return self.readL32(r)
        if typ == 11: return self.ResourceRef(self.readL8(r))
        if typ == 12:
            r.skip(4)
            dialogID = r.readUInt32()
            values = []
            for _ in range(r.readUInt32()): values.append((r.readUInt32(), self.readL32(r)))
            return self.LocalizedRef(dialogID, values)
        if typ == 13: return bytes(r.readBytes(r.readUInt32()))
        raise Exception(str(typ))

    def readL32(self, r: BinaryReader):
        length = r.readUInt32()
        if length == 0: return None
        return _utf8Trim(r.readBytes(length))

    def readL8(self, r: BinaryReader):
        length = r.readByte()
        if length == 0: return None
        return _utf8Trim(r.readBytes(length))

    def addField(self, target: dict, name: str, value) -> None:
        if name in target: raise Exception('An item with the same key has already been added.')
        target[name] = value

    def typeName(self) -> str: return self.TYPES.get(self.type, str(self.type))

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo('BinaryGFF', items=[MetaInfo(f'Type: {self.typeName()}')])
    ]

#endregion

#region Binary_Key

# Infinity Engine chitin.key. 8-byte resref. Low 14 bits are the file index.
# Bits 19-14 are tileset rows and are skipped. Bits 31-20 are the bif index.
class Binary_Key(ArcBinaryT):
    KEY_MAGIC = 0x2059454B
    KEY_VERSION = 0x20203156
    BIFF_MAGIC = 0x46464942
    BIFF_VERSION = 0x20203156
    BIFC_MAGIC = 0x43464942
    BIFC_VERSION = 0x302E3156
    EXTS = {
        0x0001: '.bmp', 0x0002: '.mve', 0x0004: '.wav', 0x0005: '.wfx', 0x0006: '.plt',
        0x03e8: '.bam', 0x03e9: '.wed', 0x03ea: '.chu', 0x03eb: '.tis', 0x03ec: '.mos',
        0x03ed: '.itm', 0x03ee: '.spl', 0x03ef: '.bcs', 0x03f0: '.ids', 0x03f1: '.cre',
        0x03f2: '.are', 0x03f3: '.dlg', 0x03f4: '.2da', 0x03f5: '.gam', 0x03f6: '.sto',
        0x03f7: '.wmp', 0x03f8: '.eff', 0x03f9: '.bs', 0x03fa: '.chr', 0x03fb: '.vvc',
        0x03fc: '.vef', 0x03fd: '.pro', 0x03fe: '.bio', 0x03ff: '.wbm', 0x0400: '.fnt',
        0x0402: '.gui', 0x0403: '.sql', 0x0404: '.pvrz', 0x0405: '.glsl', 0x0406: '.tot',
        0x0407: '.toh', 0x0408: '.menu', 0x0409: '.lua', 0x040a: '.ttf', 0x040b: '.png',
        0x044c: '.bah', 0x0802: '.ini', 0x0803: '.src', 0x0804: '.maze',
    }

    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        files = []
        magic = r.readUInt32()
        source.magic = magic
        if magic == self.KEY_MAGIC: self.readKey(source, r, files)
        elif magic == self.BIFF_MAGIC: self.readBiff(source, r, files, None, None, None, True)
        elif magic == self.BIFC_MAGIC: self.readBifc(source, r, files, None, None, None, True)
        else: raise Exception(f'Unknown File Type {magic}')
        source.files = files

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        if isinstance(file.tag, str): return self.readPath(file.tag, file.offset, file.fileSize)
        if isinstance(source.tag, (bytes, bytearray)): return BytesIO(_slice(source.tag, file.offset, file.fileSize))
        if source.magic == self.BIFC_MAGIC:
            r.seek(0)
            magic = r.readUInt32()
            if magic != self.BIFC_MAGIC: raise Exception(f'Unknown File Type {magic}')
            return BytesIO(_slice(_inflateBifc(r), file.offset, file.fileSize))
        r.seek(file.offset)
        return BytesIO(r.readBytes(file.fileSize))

    def ext(self, resType: int) -> str: return self.EXTS.get(resType, '')

    def readKey(self, source: BinaryArchive, r: BinaryReader, files: list) -> None:
        version = r.readUInt32()
        if version != self.KEY_VERSION: raise Exception('BAD MAGIC')
        source.version = version
        bifCount = r.readUInt32()
        resourceCount = r.readUInt32()
        bifOffset = r.readUInt32()
        resourceOffset = r.readUInt32()

        raw = []
        r.seek(bifOffset)
        for _ in range(bifCount):
            r.readUInt32()
            nameOfs = r.readUInt32()
            nameLen = r.readUInt16()
            r.readUInt16()
            raw.append((nameOfs, nameLen))
        bifs = [self.readName(r, nameOfs, nameLen) for nameOfs, nameLen in raw]

        byBif = {}
        r.seek(resourceOffset)
        for _ in range(resourceCount):
            resref = _ascii(r.readBytes(8), True, True)
            resType = r.readUInt16()
            locator = r.readUInt32()
            if ((locator >> 14) & 0x3F) != 0: continue
            bif = (locator >> 20) & 0xFFF
            index = locator & 0x3FFF
            map_ = byBif.get(bif)
            if map_ is None: map_ = byBif[bif] = {}
            if index not in map_: map_[index] = (resref, resType)

        for i, rel in enumerate(bifs):
            full = _bifPath(source.binPath, rel)
            if not os.path.isfile(full): continue
            self.openBif(source, files, full, rel, byBif.get(i, {}))

    def openBif(self, source, files, full, rel, map_) -> None:
        with BinaryReader(open(full, 'rb')) as br:
            magic = br.readUInt32()
            if magic == self.BIFF_MAGIC: self.readBiff(source, br, files, rel, full, map_, False)
            elif magic == self.BIFC_MAGIC: self.readBifc(source, br, files, rel, full, map_, False)
            else: raise Exception(f'Unknown File Type {magic}')

    def readBifc(self, source, r, files, rel, tagPath, map_, setVersion: bool) -> None:
        inflated = _inflateBifc(r)
        if setVersion and not isinstance(source.tag, dict): source.tag = inflated
        with BinaryReader(BytesIO(inflated)) as inner:
            magic = inner.readUInt32()
            if magic != self.BIFF_MAGIC: raise Exception(f'Unknown File Type {magic}')
            self.readBiff(source, inner, files, rel, tagPath, map_, setVersion)

    def readBiff(self, source, r, files, rel, tagPath, map_, setVersion: bool) -> None:
        version = r.readUInt32()
        if version != self.BIFF_VERSION: raise Exception('BAD MAGIC')
        if setVersion: source.version = version
        fileCount = r.readUInt32()
        r.readUInt32()
        fileTableOffset = r.readUInt32()
        r.seek(fileTableOffset)
        for i in range(fileCount):
            locator = r.readUInt32()
            offset = r.readUInt32()
            size = r.readUInt32()
            resType = r.readUInt16()
            r.readUInt16()
            if map_ is not None:
                res = map_.get(locator & 0x3FFF)
                if res is None: continue
                resref, resType = res
                files.append(FileSource(path=f'{rel}/{resref}{self.ext(resType)}', offset=offset, fileSize=size, tag=tagPath))
            else: files.append(FileSource(path=f'{i:04d}{self.ext(resType)}', offset=offset, fileSize=size))

    def readPath(self, path: str, offset: int, size: int) -> BytesIO:
        with BinaryReader(open(path, 'rb')) as br:
            magic = br.readUInt32()
            if magic == self.BIFC_MAGIC: return BytesIO(_slice(_inflateBifc(br), offset, size))
            br.seek(offset)
            return BytesIO(br.readBytes(size))

    def readName(self, r: BinaryReader, offset: int, length: int) -> str:
        if length <= 0: return ''
        r.seek(offset)
        return _ascii(r.readBytes(length), True, False).replace('\\', '/')

#endregion
