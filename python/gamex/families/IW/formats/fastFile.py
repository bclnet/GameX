# Twin of Families/GameX.IW/Formats/FastFile.cs (the ListFastFile readers).
# The unused members live in FastFileUnused.cs and stay commented.
# GetZoneFile writes a sibling .ff~ and is not called.
# COD4, WaW, and Black Ops share list32. MW2, MW3, and Ghosts share list64. BO2 and BO3 share listBo.
from __future__ import annotations
import hashlib, struct, zlib
from openx.core import BinaryReader

IWff = 0x66665749
S1ff = 0x66663153
TAff = 0x66664154
U100 = 0x30303175
A100 = 0x30303161
F100 = 0x30303130
F000 = 0x30303030

CO4_WWII = 0x0005
WaW = 0x0183
MW2 = 0x0114
BO = 0x01D9
MW3 = 0x0001
BO2 = 0x0093
Ghosts = 0x0235
AW = 0x072E
BO3 = 0x0251
IW = 0x0653
MW = 0x0042
COD_MW2 = 0x0017

GhostsNil = int.from_bytes(bytes.fromhex('FFFFFFFFFFFDFDFD'), 'little', signed=True)
Bo2Key = bytes.fromhex('641D8A2FE31D3AA63622BBC9CE8587229D42B0F8ED9B924130BF88B65EDC50BE')

class FileHeader:
    def __init__(self, id: int = 0, path: str = '', position: int = 0, fileSize: int = 0, data: bytes = None):
        self.id = id
        self.path = path
        self.position = position
        self.fileSize = fileSize
        self.data = data

def _i32(z: bytes, o: int) -> int: return struct.unpack_from('<i', z, o)[0]
def _i64(z: bytes, o: int) -> int: return struct.unpack_from('<q', z, o)[0]

def _streamLen(r: BinaryReader) -> int:
    pos = r.tell()
    r.f.seek(0, 2)
    n = r.tell()
    r.seek(pos)
    return n

def inflateRaw(data: bytes) -> bytes:
    d = zlib.decompressobj(-15)
    return d.decompress(data) + d.flush()

def inflateZlib(data: bytes) -> bytes: return inflateRaw(data)

def inflateAt(r: BinaryReader, offset: int) -> bytes:
    r.seek(offset)
    cmf = r.readByte()
    flg = r.readByte()
    if cmf != 0x78 or flg not in (0x01, 0x5E, 0x9C, 0xDA): raise Exception(f'Fastfile is not zlib ({cmf:02X}{flg:02X})')
    return inflateRaw(r.f.read())

def inflateAuthed(r: BinaryReader) -> bytes:
    r.seek(0)
    file = bytes(r.f.read())
    if len(file) < 0x4017 or file[0x4015] != 0x78: raise Exception('Signed fastfile has no zlib stream')
    packed = bytearray()
    pos = 0x4015
    dataChunks = 0
    while pos < len(file):
        if dataChunks == 256:
            pos += 0x2000
            dataChunks = 0
            continue
        n = min(0x2000, len(file) - pos)
        packed += file[pos:pos + n]
        pos += n
        dataChunks += 1
    return inflateRaw(bytes(packed[2:]))

def _enumName(enum, type_: int, max_: int) -> str:
    if 0 <= type_ < max_:
        try: return enum(type_).name
        except ValueError: pass
    return f'type{type_}'

def iw3Name(type_: int) -> str:
    from gamex.families.IW.formats.fastFile_assetType import IW3XAssetType
    return _enumName(IW3XAssetType, type_, int(IW3XAssetType.max))

def iw4Name(type_: int) -> str:
    from gamex.families.IW.formats.fastFile_assetType import IW4XAssetType
    return _enumName(IW4XAssetType, type_, int(IW4XAssetType.max))

def iw5Name(type_: int) -> str:
    from gamex.families.IW.formats.fastFile_assetType import IW5XAssetType
    return _enumName(IW5XAssetType, type_, int(IW5XAssetType.max))

def othName(type_: int) -> str:
    from gamex.families.IW.formats.fastFile_assetType import Oth_AssetType
    if 0 <= type_ < int(Oth_AssetType.Max):
        try: return Oth_AssetType(type_).name
        except ValueError: pass
    return f'type{type_}'

def listFastFile(r: BinaryReader, magic: int, format_: int, version: int):
    if magic == IWff and format_ == U100 and version in (CO4_WWII, WaW, BO): return list32(r, version)
    if magic == IWff and (version in (MW2, MW3) or (format_ == U100 and version == Ghosts)): return list64(r, format_, version)
    if magic == TAff and format_ == F100 and version == BO2: return listBo(r, False)
    if magic == TAff and format_ == F000 and version == BO3: return listBo(r, True)
    return None

def iw3Max() -> int:
    from gamex.families.IW.formats.fastFile_assetType import IW3XAssetType
    return int(IW3XAssetType.max)

def iw4Max() -> int:
    from gamex.families.IW.formats.fastFile_assetType import IW4XAssetType
    return int(IW4XAssetType.max)

def iw4Raw() -> int:
    from gamex.families.IW.formats.fastFile_assetType import IW4XAssetType
    return int(IW4XAssetType.rawfile)

def iw5Max() -> int:
    from gamex.families.IW.formats.fastFile_assetType import IW5XAssetType
    return int(IW5XAssetType.max)

def iw5Raw() -> int:
    from gamex.families.IW.formats.fastFile_assetType import IW5XAssetType
    return int(IW5XAssetType.rawfile)

def othMax() -> int:
    from gamex.families.IW.formats.fastFile_assetType import Oth_AssetType
    return int(Oth_AssetType.Max)

def list32(r: BinaryReader, version: int):
    if version == CO4_WWII:
        cmf = r.readByte()
        flg = r.readByte()
        if cmf != 0x78 or flg not in (0x01, 0x5E, 0x9C, 0xDA): raise Exception(f'COD4 fastfile is not zlib ({cmf:02X}{flg:02X})')
        zone = inflateRaw(r.f.read())
        if len(zone) < 0x3C: raise Exception(f'COD4 zone is {len(zone)} bytes')
        stringCount = _i32(zone, 0x2C)
        stringPtr = _i32(zone, 0x30)
        assetCount = _i32(zone, 0x34)
        assetPtr = _i32(zone, 0x38)
        if stringCount < 0 or assetCount < 0 or stringCount > 1_000_000 or assetCount > 1_000_000: raise Exception(f'COD4 index counts {stringCount}/{assetCount}')
        files = []
        o = 0x3C
        if stringCount > 0:
            if stringPtr != -1: raise Exception(f'COD4 script strings are not inline ({stringPtr:08X})')
            ptrs = [_i32(zone, o + i * 4) for i in range(stringCount)]
            o += stringCount * 4
            text = []
            for i, ptr in enumerate(ptrs):
                if ptr == -1:
                    e = o
                    while e < len(zone) and zone[e] != 0: e += 1
                    name = zone[o:e].decode('ascii', 'replace')
                    o = e + 1 if e < len(zone) else e
                    if len(name) > 0: text.append(f'{i}\t{name}\n')
                elif ptr != 0: raise Exception(f'COD4 script string pointer {ptr:08X}')
            if text:
                data = ''.join(text).encode('ascii', 'replace')
                files.append(FileHeader(-1, 'scriptstrings.txt', 0, len(data), data))
        if assetCount > 0:
            if assetPtr != -1: raise Exception(f'COD4 asset list is not inline ({assetPtr:08X})')
            for i in range(assetCount):
                pos = o
                type_ = _i32(zone, o)
                o += 8
                files.append(FileHeader(i, f'{iw3Name(type_)}/{i:04d}', pos, 8))
        return zone, files
    nameOf, maxType = (iw3Name, iw3Max()) if version == WaW else (othName, othMax())
    zone = inflateAt(r, 12)
    if len(zone) < 0x30: return zone, []
    count = _i32(zone, 0x2C)
    if count <= 0 or count > 1_000_000 or 0x30 + count * 8 > len(zone): return zone, []
    files = []
    for i in range(count):
        pos = 0x30 + i * 8
        type_ = _i32(zone, pos + 4)
        if (type_ & 0xFFFFFFFF) >= (maxType & 0xFFFFFFFF): return zone, []
        files.append(FileHeader(i, f'{nameOf(type_)}/{i:04d}', pos, 8))
    return zone, files

def list64(r: BinaryReader, format_: int, version: int):
    if version == Ghosts:
        zone = inflateAt(r, 0x2C)
        return zone, scanRaw(zone, GhostsNil)
    if version == MW2: countAt, recAt, rawType, maxType, nameOf = 0x38, 0x40, iw4Raw(), iw4Max(), iw4Name
    else: countAt, recAt, rawType, maxType, nameOf = 0x3C, 0x44, iw5Raw(), iw5Max(), iw5Name
    if format_ == U100: zone = inflateAt(r, 0x15)
    elif format_ == F100: zone = inflateAuthed(r)
    else: return None, []
    if len(zone) < countAt + 4: return zone, []
    count = _i32(zone, countAt)
    if count <= 0 or count > 1_000_000 or recAt + count * 16 > len(zone): return zone, []
    files = []
    allRaw = True
    for i in range(count):
        pos = recAt + i * 16
        type_ = _i64(zone, pos + 8)
        if type_ < 0 or type_ >= maxType: return zone, []
        if type_ != rawType: allRaw = False
        files.append(FileHeader(i, f'{nameOf(int(type_))}/{i:04d}', pos, 16))
    if allRaw:
        body = recAt + count * 16
        if body + 8 <= len(zone) and _i64(zone, body) == -1: body += 8
        named = tryWalkAll(zone, body, count, -1)
        if named is not None: return zone, named
    return zone, files

def tryRaw(z: bytes, pos: int, nil: int):
    if pos < 0 or pos + 24 > len(z): return None
    p1 = _i64(z, pos)
    cLen = _i32(z, pos + 8)
    length = _i32(z, pos + 12)
    p2 = _i64(z, pos + 16)
    if p1 != nil or p2 != nil or cLen < 0 or length < 0 or cLen > len(z) - pos: return None
    s = pos + 24
    e = s
    while e < len(z) and z[e] != 0:
        if z[e] < 32 or z[e] > 126: return None
        e += 1
        if e - s > 120: return None
    if e >= len(z) or e == s: return None
    name = z[s:e].decode('ascii')
    p = e + 1
    if (p & 1) != 0 and p < len(z) and z[p] == 0: p += 1
    elif cLen == 0 and p + 1 < len(z) and z[p] == 0 and z[p + 1] == 0xFF: p += 1
    if p > len(z) or cLen > len(z) - p: return None
    if cLen > 0 and z[p] != 0x78: return None
    if cLen == 0: data = b''
    else:
        try: data = inflateRaw(z[p + 2:p + cLen])
        except zlib.error: return None
    return name, data, p + cLen

def tryWalkAll(z: bytes, pos: int, count: int, nil: int):
    files = []
    for i in range(count):
        got = tryRaw(z, pos, nil)
        if got is None: return None
        name, data, next_ = got
        if next_ < pos: return None
        files.append(FileHeader(i, name, 0, len(data), data))
        pos = next_
    return files

def scanRaw(z: bytes, nil: int):
    files = []
    pos = 0x40
    while pos + 24 < len(z):
        if z[pos] != (nil & 0xFF) or _i64(z, pos) != nil:
            pos += 1
            continue
        if tryRaw(z, pos, nil) is None:
            pos += 1
            continue
        run = []
        p = pos
        guard = 0
        while guard < 100_000:
            guard += 1
            got = tryRaw(z, p, nil)
            if got is None: break
            name, data, next_ = got
            if next_ < p: break
            run.append(FileHeader(len(files) + len(run), name, 0, len(data), data))
            if next_ == p: break
            p = next_
        if len(run) >= 2 or (len(run) == 1 and p >= len(z) - 16):
            files.extend(run)
            if p > pos: pos = p - 1
        pos += 1
    return files

def _rotl(v: int, c: int) -> int:
    v &= 0xFFFFFFFF
    return ((v << c) | (v >> (32 - c))) & 0xFFFFFFFF

def _u32(b: bytes, o: int) -> int: return int.from_bytes(b[o:o + 4], 'little')

def salsa20(key: bytes, iv: bytes, data: bytes) -> bytes:
    state = [0] * 16
    state[1] = _u32(key, 0)
    state[2] = _u32(key, 4)
    state[3] = _u32(key, 8)
    state[4] = _u32(key, 12)
    constants = b'expand 32-byte k' if len(key) == 32 else b'expand 16-byte k'
    keyIndex = len(key) - 16
    state[11] = _u32(key, keyIndex)
    state[12] = _u32(key, keyIndex + 4)
    state[13] = _u32(key, keyIndex + 8)
    state[14] = _u32(key, keyIndex + 12)
    state[0] = _u32(constants, 0)
    state[5] = _u32(constants, 4)
    state[10] = _u32(constants, 8)
    state[15] = _u32(constants, 12)
    state[6] = _u32(iv, 0)
    state[7] = _u32(iv, 4)
    out = bytearray(len(data))
    offset = 0
    while offset < len(data):
        block = _salsaHash(state)
        state[8] = (state[8] + 1) & 0xFFFFFFFF
        if state[8] == 0: state[9] = (state[9] + 1) & 0xFFFFFFFF
        n = min(64, len(data) - offset)
        for i in range(n): out[offset + i] = data[offset + i] ^ block[i]
        offset += n
    return bytes(out)

def _salsaHash(input_: list) -> bytes:
    state = input_[:]
    round_ = 20
    add = lambda v, w: (v + w) & 0xFFFFFFFF
    while round_ > 0:
        state[4] ^= _rotl(add(state[0], state[12]), 7)
        state[8] ^= _rotl(add(state[4], state[0]), 9)
        state[12] ^= _rotl(add(state[8], state[4]), 13)
        state[0] ^= _rotl(add(state[12], state[8]), 18)
        state[9] ^= _rotl(add(state[5], state[1]), 7)
        state[13] ^= _rotl(add(state[9], state[5]), 9)
        state[1] ^= _rotl(add(state[13], state[9]), 13)
        state[5] ^= _rotl(add(state[1], state[13]), 18)
        state[14] ^= _rotl(add(state[10], state[6]), 7)
        state[2] ^= _rotl(add(state[14], state[10]), 9)
        state[6] ^= _rotl(add(state[2], state[14]), 13)
        state[10] ^= _rotl(add(state[6], state[2]), 18)
        state[3] ^= _rotl(add(state[15], state[11]), 7)
        state[7] ^= _rotl(add(state[3], state[15]), 9)
        state[11] ^= _rotl(add(state[7], state[3]), 13)
        state[15] ^= _rotl(add(state[11], state[7]), 18)
        state[1] ^= _rotl(add(state[0], state[3]), 7)
        state[2] ^= _rotl(add(state[1], state[0]), 9)
        state[3] ^= _rotl(add(state[2], state[1]), 13)
        state[0] ^= _rotl(add(state[3], state[2]), 18)
        state[6] ^= _rotl(add(state[5], state[4]), 7)
        state[7] ^= _rotl(add(state[6], state[5]), 9)
        state[4] ^= _rotl(add(state[7], state[6]), 13)
        state[5] ^= _rotl(add(state[4], state[7]), 18)
        state[11] ^= _rotl(add(state[10], state[9]), 7)
        state[8] ^= _rotl(add(state[11], state[10]), 9)
        state[9] ^= _rotl(add(state[8], state[11]), 13)
        state[10] ^= _rotl(add(state[9], state[8]), 18)
        state[12] ^= _rotl(add(state[15], state[14]), 7)
        state[13] ^= _rotl(add(state[12], state[15]), 9)
        state[14] ^= _rotl(add(state[13], state[12]), 13)
        state[15] ^= _rotl(add(state[14], state[13]), 18)
        round_ -= 2
    out = bytearray(64)
    for index in range(16):
        w = add(state[index], input_[index])
        out[4 * index:4 * index + 4] = w.to_bytes(4, 'little')
    return bytes(out)

def createIvTable(source: bytes) -> bytearray:
    ivTable = bytearray(16000)
    addDiv = 0
    nameKeyLength = source.find(0)
    if nameKeyLength <= 0: return ivTable
    i = 0
    while i < len(ivTable):
        x = 0
        while x < nameKeyLength * 4:
            if i + addDiv >= len(ivTable) or i + x >= len(ivTable): return ivTable
            addDiv = x // 4 if x > 0 else 0
            for y in range(4): ivTable[i + x + y] = source[addDiv]
            x += 4
        i += nameKeyLength * 4
    return ivTable

def updateIvTable(index: int, hash_: bytes, ivTable: bytearray, ivCounter: list) -> None:
    i = 0
    while i < 20:
        value = (index + 4 * ivCounter[index]) % 800 * 5
        for x in range(5): ivTable[4 * value + x + i] ^= hash_[i + x]
        i += 5
    ivCounter[index] += 1

def ivAt(index: int, ivTable: bytearray, ivCounter: list) -> bytes:
    arrayIndex = (index + 4 * (ivCounter[index] - 1)) % 800 * 20
    return bytes(ivTable[arrayIndex:arrayIndex + 8])

def listBo(r: BinaryReader, bo3: bool):
    if not bo3:
        r.seek(0x18)
        ivCount = [1, 1, 1, 1]
        ivTable = createIvTable(bytes(r.readBytes(0x20)))
        r.seek(r.tell() + 0x100)
        length = _streamLen(r)
        zone = bytearray()
        section = 0
        while r.tell() + 4 <= length and section < 10_000:
            size = r.readInt32()
            if size == 0: break
            if size < 0 or r.tell() + size > length: raise Exception(f'BO2 section {size}')
            decrypted = salsa20(Bo2Key, ivAt(section % 4, ivTable, ivCount), bytes(r.readBytes(size)))
            updateIvTable(section % 4, hashlib.sha1(decrypted).digest(), ivTable, ivCount)
            zone += inflateRaw(decrypted)
            section += 1
        zone = bytes(zone)
    else:
        r.seek(12)
        unknown = r.readByte()
        flagsZ = r.readByte()
        flagsPc = r.readByte()
        flagsEncrypted = r.readByte()
        if unknown != 0 or flagsZ != 1 or flagsPc != 0 or flagsEncrypted != 0: return None, []
        r.seek(0x90)
        size = r.readInt64()
        if size <= 0 or size > 512 * 1024 * 1024: raise Exception(f'BO3 zone size {size}')
        r.seek(0x248)
        length = _streamLen(r)
        zone = bytearray()
        consumed = 0
        while consumed < size:
            pos = r.tell()
            if pos + 16 > length: break
            compressedSize = r.readInt32()
            decompressedSize = r.readInt32()
            blockSize = r.readInt32()
            position = r.readInt32()
            if position != pos: raise Exception(f'BO3 block position {position:X}')
            if decompressedSize == 0:
                r.seek((r.tell() + 0x7FFFFF) & ~0x7FFFFF)
                continue
            if compressedSize < 2: raise Exception(f'BO3 block compressed size {compressedSize}')
            r.seek(r.tell() + 2)
            compressed = bytes(r.readBytes(compressedSize - 2))
            zone += inflateRaw(compressed)
            consumed += decompressedSize
            r.seek(position + 16 + blockSize)
        zone = bytes(zone)
    return zone, harvestNames(zone, bo3)

def isAssetName(name: str) -> bool:
    if len(name) < 3 or len(name) > 180: return False
    for ch in name:
        if ord(ch) < 32 or ord(ch) > 126: return False
    if '/' in name or '\\' in name or name[0] == '$': return True
    return name.endswith(('.cfg', '.csv', '.gsc', '.csc', '.ddl', '.txt', '.vision', '.menu'))

def harvestNames(zone: bytes, commaPrefix: bool):
    files = []
    seen = set()
    i = 0
    n = len(zone)
    while i < n:
        start = i
        if commaPrefix:
            if zone[i] != ord(',') or (i > 0 and zone[i - 1] >= 32):
                i += 1
                continue
            start = i + 1
        elif i > 0 and 32 <= zone[i - 1] < 127:
            i += 1
            continue
        e = start
        while e < n and 32 <= zone[e] < 127: e += 1
        if e >= n or zone[e] != 0 or e == start:
            i += 1
            continue
        name = zone[start:e].decode('ascii')
        if isAssetName(name) and name not in seen:
            seen.add(name)
            files.append(FileHeader(len(files), name, start, e - start))
        i += 1
    return files
