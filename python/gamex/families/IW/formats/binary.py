# Twin of Families/GameX.IW/Formats/Binary.cs for the .ff read, ReadData, and FastFile readers.
# CASC, .iwd, .pak, .ipak, and .xpak are not ported. Binary_Iwi here does not decode; the C# reader does.
from __future__ import annotations
from io import BytesIO
from openx.core import _pathExtension, BinaryReader
import hashlib, os, struct, zlib
from gamex import ArcBinaryT, Archive, BinaryArchive, FileSource, IHaveMetaInfo, MetaContent, MetaInfo, MetaManager

# FastFile header words. Readers are methods on Binary_IW.
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

class Binary_IW(ArcBinaryT):
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        source.files = []
        if source.game is not None and source.game.id in ('BO4', 'BOCW', 'Vanguard'):
            source.magic = 0
            return
        if _pathExtension(source.binPath).lower() != '.ff': return
        source.magic = 2
        magic = r.readUInt32()
        format_ = r.readUInt32()
        version = r.readUInt32()
        if magic not in (IWff, S1ff, TAff): raise Exception(f'Bad magic {magic}')
        if format_ not in (U100, F100, A100, F000): raise Exception(f'Bad format {format_}')
        listed = self.listFastFile(r, magic, format_, version)
        if listed is None: return
        zone, headers = listed
        source.tag = zone
        source.files = [FileSource(id=asset.id, path=asset.path, offset=asset.position, fileSize=asset.fileSize, data=asset.data) for asset in headers]

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        if file.data is not None: return BytesIO(bytes(file.data))
        zone = source.tag
        if isinstance(zone, (bytes, bytearray)) and file.fileSize and file.fileSize > 0:
            start = 0 if file.offset is None else file.offset
            end = start + file.fileSize
            if start < 0 or end > len(zone): raise Exception('offset')
            return BytesIO(bytes(zone[start:end]))
        return BytesIO()

    #region FastFile

    class FileHeader:
        def __init__(self, id: int = 0, path: str = '', position: int = 0, fileSize: int = 0, data: bytes = None):
            self.id = id
            self.path = path
            self.position = position
            self.fileSize = fileSize
            self.data = data

    @staticmethod
    def _i32(z: bytes, o: int) -> int: return struct.unpack_from('<i', z, o)[0]

    @staticmethod
    def _i64(z: bytes, o: int) -> int: return struct.unpack_from('<q', z, o)[0]


    @staticmethod
    def _streamLen(r: BinaryReader) -> int:
        pos = r.tell()
        r.f.seek(0, 2)
        n = r.tell()
        r.seek(pos)
        return n


    @staticmethod
    def inflateRaw(data: bytes) -> bytes:
        d = zlib.decompressobj(-15)
        return d.decompress(data) + d.flush()


    @staticmethod
    def inflateZlib(data: bytes) -> bytes: return Binary_IW.inflateRaw(data)


    @staticmethod
    def inflateAt(r: BinaryReader, offset: int) -> bytes:
        r.seek(offset)
        cmf = r.readByte()
        flg = r.readByte()
        if cmf != 0x78 or flg not in (0x01, 0x5E, 0x9C, 0xDA): raise Exception(f'Fastfile is not zlib ({cmf:02X}{flg:02X})')
        return Binary_IW.inflateRaw(r.f.read())


    @staticmethod
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
        return Binary_IW.inflateRaw(bytes(packed[2:]))


    # Index names. Same strings as the C# enums, which stay in ZoneUnused and are not ported.
    _IW3 = (
        'xmodelpieces', 'physpreset', 'xanim', 'xmodel', 'material', 'techset', 'image', 'sound',
        'sndcurve', 'loaded_sound', 'col_map_sp', 'col_map_mp', 'com_map', 'game_map_sp', 'game_map_mp',
        'map_ents', 'gfx_map', 'lightdef', 'ui_map', 'font', 'menufile', 'menu', 'localize', 'weapon',
        'snddriverglobals', 'fx', 'impactfx', 'aitype', 'mptype', 'character', 'xmodelalias', 'rawfile',
        'stringtable',
    )
    _IW4 = (
        'physpreset', 'phys_collmap', 'xanim', 'xmodelsurfs', 'xmodel', 'material', 'pixelshader',
        'vertexshader', 'vertexdecl', 'techset', 'image', 'sound', 'sndcurve', 'loaded_sound',
        'col_map_sp', 'col_map_mp', 'com_map', 'game_map_sp', 'game_map_mp', 'map_ents', 'fx_map',
        'gfx_map', 'lightdef', 'ui_map', 'font', 'menufile', 'menu', 'localize', 'weapon',
        'snddriverglobals', 'fx', 'impactfx', 'aitype', 'mptype', 'character', 'xmodelalias', 'rawfile',
        'stringtable', 'leaderboarddef', 'structureddatadef', 'tracer', 'vehicle', 'addon_map_ents',
    )
    _IW5 = (
        'physpreset', 'phys_collmap', 'xanim', 'xmodelsurfs', 'xmodel', 'material', 'pixelshader',
        'vertexshader', 'vertexdecl', 'techset', 'image', 'sound', 'sndcurve', 'loaded_sound',
        'col_map_mp', 'com_map', 'glass_map', 'ai_paths', 'vehicle_tracks', 'map_ents', 'fx_map',
        'gfx_map', 'lightdef', 'ui_map', 'font', 'menufile', 'menu', 'localize', 'attachment', 'weapon',
        'snddriverglobals', 'fx', 'impactfx', 'surfacefx', 'aitype', 'mptype', 'character', 'xmodelalias',
        'rawfile', 'scriptfile', 'stringtable', 'leaderboarddef', 'structureddatadef', 'tracer', 'vehicle',
        'addon_map_ents',
    )
    _OTH = (
        'PhysPreset', 'PhysColMap', 'XAnim', 'XModelSurfs', 'XModel', 'Material', 'PixelShader',
        'VertexShader', 'VertexDecl', 'TechSet', 'Image', 'Sound', 'SndCurve', 'LoadedSound',
        'ColMapSP', 'ColMapMP', 'ComMap', 'GameMapSP', 'GameMapMP', 'MapEnts', 'FXMap', 'GfxMap',
        'LightDef', 'UIMap', 'Font', 'MenuFile', 'Menu', 'Localize', 'Weapon', 'SndDriverGlobals',
        'FX', 'ImpactFX', 'AIType', 'MPType', 'Character', 'XModelAlias', 'RawFile', 'StringTable',
        'LeaderBoardDef', 'StructuredDataDef', 'Tracer', 'Vehical', 'AddONMapEnts',
    )

    @staticmethod
    def _typeName(names: tuple, type_: int) -> str:
        return names[type_] if 0 <= type_ < len(names) else f'type{type_}'

    @staticmethod
    def iw3Name(type_: int) -> str: return Binary_IW._typeName(Binary_IW._IW3, type_)

    @staticmethod
    def iw4Name(type_: int) -> str: return Binary_IW._typeName(Binary_IW._IW4, type_)

    @staticmethod
    def iw5Name(type_: int) -> str: return Binary_IW._typeName(Binary_IW._IW5, type_)

    @staticmethod
    def othName(type_: int) -> str: return Binary_IW._typeName(Binary_IW._OTH, type_)


    @staticmethod
    def assetExt(typeName: str) -> str:
        # The dot is part of the extension. Three letters when the name is longer.
        # image is .iwi and scriptfile is .gsc. PhysColMap and phys_collmap are both .pcm.
        key = typeName.lower().replace('_', '')
        match key:
            case 'image': return '.iwi'
            case 'scriptfile': return '.gsc'
            case 'xmodelpieces': return '.xmp'
            case 'physpreset': return '.phy'
            case 'xanim': return '.xan'
            case 'xmodel': return '.xmd'
            case 'material': return '.mat'
            case 'techset': return '.tec'
            case 'sound': return '.snd'
            case 'sndcurve': return '.crv'
            case 'loadedsound': return '.lds'
            case 'colmapsp': return '.csp'
            case 'colmapmp': return '.cmp'
            case 'commap': return '.cmm'
            case 'gamemapsp': return '.gsp'
            case 'gamemapmp': return '.gmp'
            case 'mapents': return '.ent'
            case 'gfxmap': return '.gfx'
            case 'lightdef': return '.lgt'
            case 'uimap': return '.uim'
            case 'font': return '.fnt'
            case 'menufile': return '.mnf'
            case 'menu': return '.mnu'
            case 'localize': return '.loc'
            case 'weapon': return '.wep'
            case 'snddriverglobals': return '.sdg'
            case 'fx': return '.fx'
            case 'impactfx': return '.ifx'
            case 'aitype': return '.ait'
            case 'mptype': return '.mpt'
            case 'character': return '.chr'
            case 'xmodelalias': return '.xma'
            case 'rawfile': return '.raw'
            case 'stringtable': return '.tbl'
            case 'physcolmap' | 'physcollmap': return '.pcm'
            case 'xmodelsurfs': return '.xms'
            case 'pixelshader': return '.psh'
            case 'vertexshader': return '.vsh'
            case 'vertexdecl': return '.vdl'
            case 'fxmap': return '.fxm'
            case 'leaderboarddef': return '.lbd'
            case 'structureddatadef': return '.sdd'
            case 'tracer': return '.trc'
            case 'vehicle' | 'vehical': return '.veh'
            case 'addonmapents': return '.ame'
            case 'glassmap': return '.gls'
            case 'aipaths': return '.aip'
            case 'vehicletracks': return '.vtr'
            case 'attachment': return '.att'
            case 'surfacefx': return '.sfx'
            case _: return '.' + key if len(key) <= 3 else '.' + key[:3]


    @staticmethod
    def indexPath(typeName: str, i: int) -> str:
        return f'{typeName}/{i:04d}{Binary_IW.assetExt(typeName)}'


    @staticmethod
    def listFastFile(r: BinaryReader, magic: int, format_: int, version: int):
        if magic == IWff and format_ == U100 and version in (CO4_WWII, WaW, BO): return Binary_IW.list32(r, version)
        if magic == IWff and (version in (MW2, MW3) or (format_ == U100 and version == Ghosts)): return Binary_IW.list64(r, format_, version)
        if magic == TAff and format_ == F100 and version == BO2: return Binary_IW.listBo(r, False)
        if magic == TAff and format_ == F000 and version == BO3: return Binary_IW.listBo(r, True)
        return None


    @staticmethod
    def list32(r: BinaryReader, version: int):
        if version == CO4_WWII:
            cmf = r.readByte()
            flg = r.readByte()
            if cmf != 0x78 or flg not in (0x01, 0x5E, 0x9C, 0xDA): raise Exception(f'COD4 fastfile is not zlib ({cmf:02X}{flg:02X})')
            zone = Binary_IW.inflateRaw(r.f.read())
            if len(zone) < 0x3C: raise Exception(f'COD4 zone is {len(zone)} bytes')
            stringCount = Binary_IW._i32(zone, 0x2C)
            stringPtr = Binary_IW._i32(zone, 0x30)
            assetCount = Binary_IW._i32(zone, 0x34)
            assetPtr = Binary_IW._i32(zone, 0x38)
            if stringCount < 0 or assetCount < 0 or stringCount > 1_000_000 or assetCount > 1_000_000: raise Exception(f'COD4 index counts {stringCount}/{assetCount}')
            files = []
            o = 0x3C
            if stringCount > 0:
                if stringPtr != -1: raise Exception(f'COD4 script strings are not inline ({stringPtr:08X})')
                ptrs = [Binary_IW._i32(zone, o + i * 4) for i in range(stringCount)]
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
                    files.append(Binary_IW.FileHeader(-1, 'scriptstrings.txt', 0, len(data), data))
            if assetCount > 0:
                if assetPtr != -1: raise Exception(f'COD4 asset list is not inline ({assetPtr:08X})')
                for i in range(assetCount):
                    pos = o
                    type_ = Binary_IW._i32(zone, o)
                    o += 8
                    files.append(Binary_IW.FileHeader(i, Binary_IW.indexPath(Binary_IW.iw3Name(type_), i), pos, 8))
            return zone, files
        nameOf, maxType = (Binary_IW.iw3Name, len(Binary_IW._IW3)) if version == WaW else (Binary_IW.othName, len(Binary_IW._OTH))
        zone = Binary_IW.inflateAt(r, 12)
        if len(zone) < 0x30: return zone, []
        count = Binary_IW._i32(zone, 0x2C)
        if count <= 0 or count > 1_000_000 or 0x30 + count * 8 > len(zone): return zone, []
        files = []
        for i in range(count):
            pos = 0x30 + i * 8
            type_ = Binary_IW._i32(zone, pos + 4)
            if (type_ & 0xFFFFFFFF) >= (maxType & 0xFFFFFFFF): return zone, []
            files.append(Binary_IW.FileHeader(i, Binary_IW.indexPath(nameOf(type_), i), pos, 8))
        return zone, files


    @staticmethod
    def list64(r: BinaryReader, format_: int, version: int):
        if version == Ghosts:
            zone = Binary_IW.inflateAt(r, 0x2C)
            return zone, Binary_IW.scanRaw(zone, GhostsNil)
        if version == MW2: countAt, recAt, rawType, maxType, nameOf = 0x38, 0x40, Binary_IW._IW4.index('rawfile'), len(Binary_IW._IW4), Binary_IW.iw4Name
        else: countAt, recAt, rawType, maxType, nameOf = 0x3C, 0x44, Binary_IW._IW5.index('rawfile'), len(Binary_IW._IW5), Binary_IW.iw5Name
        if format_ == U100: zone = Binary_IW.inflateAt(r, 0x15)
        elif format_ == F100: zone = Binary_IW.inflateAuthed(r)
        else: return None, []
        if len(zone) < countAt + 4: return zone, []
        count = Binary_IW._i32(zone, countAt)
        if count <= 0 or count > 1_000_000 or recAt + count * 16 > len(zone): return zone, []
        files = []
        allRaw = True
        for i in range(count):
            pos = recAt + i * 16
            type_ = Binary_IW._i64(zone, pos + 8)
            if type_ < 0 or type_ >= maxType: return zone, []
            if type_ != rawType: allRaw = False
            files.append(Binary_IW.FileHeader(i, Binary_IW.indexPath(nameOf(int(type_)), i), pos, 16))
        if allRaw:
            body = recAt + count * 16
            if body + 8 <= len(zone) and Binary_IW._i64(zone, body) == -1: body += 8
            named = Binary_IW.tryWalkAll(zone, body, count, -1)
            if named is not None: return zone, named
        return zone, files


    @staticmethod
    def tryRaw(z: bytes, pos: int, nil: int):
        if pos < 0 or pos + 24 > len(z): return None
        p1 = Binary_IW._i64(z, pos)
        cLen = Binary_IW._i32(z, pos + 8)
        length = Binary_IW._i32(z, pos + 12)
        p2 = Binary_IW._i64(z, pos + 16)
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
            try: data = Binary_IW.inflateRaw(z[p + 2:p + cLen])
            except zlib.error: return None
        return name, data, p + cLen


    @staticmethod
    def tryWalkAll(z: bytes, pos: int, count: int, nil: int):
        files = []
        for i in range(count):
            got = Binary_IW.tryRaw(z, pos, nil)
            if got is None: return None
            name, data, next_ = got
            if next_ < pos: return None
            files.append(Binary_IW.FileHeader(i, name, 0, len(data), data))
            pos = next_
        return files


    @staticmethod
    def scanRaw(z: bytes, nil: int):
        files = []
        pos = 0x40
        while pos + 24 < len(z):
            if z[pos] != (nil & 0xFF) or Binary_IW._i64(z, pos) != nil:
                pos += 1
                continue
            if Binary_IW.tryRaw(z, pos, nil) is None:
                pos += 1
                continue
            run = []
            p = pos
            guard = 0
            while guard < 100_000:
                guard += 1
                got = Binary_IW.tryRaw(z, p, nil)
                if got is None: break
                name, data, next_ = got
                if next_ < p: break
                run.append(Binary_IW.FileHeader(len(files) + len(run), name, 0, len(data), data))
                if next_ == p: break
                p = next_
            if len(run) >= 2 or (len(run) == 1 and p >= len(z) - 16):
                files.extend(run)
                if p > pos: pos = p - 1
            pos += 1
        return files


    @staticmethod
    def _rotl(v: int, c: int) -> int:
        v &= 0xFFFFFFFF
        return ((v << c) | (v >> (32 - c))) & 0xFFFFFFFF


    @staticmethod
    def _u32(b: bytes, o: int) -> int: return int.from_bytes(b[o:o + 4], 'little')


    @staticmethod
    def salsa20(key: bytes, iv: bytes, data: bytes) -> bytes:
        state = [0] * 16
        state[1] = Binary_IW._u32(key, 0)
        state[2] = Binary_IW._u32(key, 4)
        state[3] = Binary_IW._u32(key, 8)
        state[4] = Binary_IW._u32(key, 12)
        constants = b'expand 32-byte k' if len(key) == 32 else b'expand 16-byte k'
        keyIndex = len(key) - 16
        state[11] = Binary_IW._u32(key, keyIndex)
        state[12] = Binary_IW._u32(key, keyIndex + 4)
        state[13] = Binary_IW._u32(key, keyIndex + 8)
        state[14] = Binary_IW._u32(key, keyIndex + 12)
        state[0] = Binary_IW._u32(constants, 0)
        state[5] = Binary_IW._u32(constants, 4)
        state[10] = Binary_IW._u32(constants, 8)
        state[15] = Binary_IW._u32(constants, 12)
        state[6] = Binary_IW._u32(iv, 0)
        state[7] = Binary_IW._u32(iv, 4)
        out = bytearray(len(data))
        offset = 0
        while offset < len(data):
            block = Binary_IW._salsaHash(state)
            state[8] = (state[8] + 1) & 0xFFFFFFFF
            if state[8] == 0: state[9] = (state[9] + 1) & 0xFFFFFFFF
            n = min(64, len(data) - offset)
            for i in range(n): out[offset + i] = data[offset + i] ^ block[i]
            offset += n
        return bytes(out)


    @staticmethod
    def _salsaHash(input_: list) -> bytes:
        state = input_[:]
        round_ = 20
        add = lambda v, w: (v + w) & 0xFFFFFFFF
        while round_ > 0:
            state[4] ^= Binary_IW._rotl(add(state[0], state[12]), 7)
            state[8] ^= Binary_IW._rotl(add(state[4], state[0]), 9)
            state[12] ^= Binary_IW._rotl(add(state[8], state[4]), 13)
            state[0] ^= Binary_IW._rotl(add(state[12], state[8]), 18)
            state[9] ^= Binary_IW._rotl(add(state[5], state[1]), 7)
            state[13] ^= Binary_IW._rotl(add(state[9], state[5]), 9)
            state[1] ^= Binary_IW._rotl(add(state[13], state[9]), 13)
            state[5] ^= Binary_IW._rotl(add(state[1], state[13]), 18)
            state[14] ^= Binary_IW._rotl(add(state[10], state[6]), 7)
            state[2] ^= Binary_IW._rotl(add(state[14], state[10]), 9)
            state[6] ^= Binary_IW._rotl(add(state[2], state[14]), 13)
            state[10] ^= Binary_IW._rotl(add(state[6], state[2]), 18)
            state[3] ^= Binary_IW._rotl(add(state[15], state[11]), 7)
            state[7] ^= Binary_IW._rotl(add(state[3], state[15]), 9)
            state[11] ^= Binary_IW._rotl(add(state[7], state[3]), 13)
            state[15] ^= Binary_IW._rotl(add(state[11], state[7]), 18)
            state[1] ^= Binary_IW._rotl(add(state[0], state[3]), 7)
            state[2] ^= Binary_IW._rotl(add(state[1], state[0]), 9)
            state[3] ^= Binary_IW._rotl(add(state[2], state[1]), 13)
            state[0] ^= Binary_IW._rotl(add(state[3], state[2]), 18)
            state[6] ^= Binary_IW._rotl(add(state[5], state[4]), 7)
            state[7] ^= Binary_IW._rotl(add(state[6], state[5]), 9)
            state[4] ^= Binary_IW._rotl(add(state[7], state[6]), 13)
            state[5] ^= Binary_IW._rotl(add(state[4], state[7]), 18)
            state[11] ^= Binary_IW._rotl(add(state[10], state[9]), 7)
            state[8] ^= Binary_IW._rotl(add(state[11], state[10]), 9)
            state[9] ^= Binary_IW._rotl(add(state[8], state[11]), 13)
            state[10] ^= Binary_IW._rotl(add(state[9], state[8]), 18)
            state[12] ^= Binary_IW._rotl(add(state[15], state[14]), 7)
            state[13] ^= Binary_IW._rotl(add(state[12], state[15]), 9)
            state[14] ^= Binary_IW._rotl(add(state[13], state[12]), 13)
            state[15] ^= Binary_IW._rotl(add(state[14], state[13]), 18)
            round_ -= 2
        out = bytearray(64)
        for index in range(16):
            w = add(state[index], input_[index])
            out[4 * index:4 * index + 4] = w.to_bytes(4, 'little')
        return bytes(out)


    @staticmethod
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


    @staticmethod
    def updateIvTable(index: int, hash_: bytes, ivTable: bytearray, ivCounter: list) -> None:
        i = 0
        while i < 20:
            value = (index + 4 * ivCounter[index]) % 800 * 5
            for x in range(5): ivTable[4 * value + x + i] ^= hash_[i + x]
            i += 5
        ivCounter[index] += 1


    @staticmethod
    def ivAt(index: int, ivTable: bytearray, ivCounter: list) -> bytes:
        arrayIndex = (index + 4 * (ivCounter[index] - 1)) % 800 * 20
        return bytes(ivTable[arrayIndex:arrayIndex + 8])


    @staticmethod
    def listBo(r: BinaryReader, bo3: bool):
        if not bo3:
            r.seek(0x18)
            ivCount = [1, 1, 1, 1]
            ivTable = Binary_IW.createIvTable(bytes(r.readBytes(0x20)))
            r.seek(r.tell() + 0x100)
            length = Binary_IW._streamLen(r)
            zone = bytearray()
            section = 0
            while r.tell() + 4 <= length and section < 10_000:
                size = r.readInt32()
                if size == 0: break
                if size < 0 or r.tell() + size > length: raise Exception(f'BO2 section {size}')
                decrypted = Binary_IW.salsa20(Bo2Key, Binary_IW.ivAt(section % 4, ivTable, ivCount), bytes(r.readBytes(size)))
                Binary_IW.updateIvTable(section % 4, hashlib.sha1(decrypted).digest(), ivTable, ivCount)
                zone += Binary_IW.inflateRaw(decrypted)
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
            length = Binary_IW._streamLen(r)
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
                zone += Binary_IW.inflateRaw(compressed)
                consumed += decompressedSize
                r.seek(position + 16 + blockSize)
            zone = bytes(zone)
        return zone, Binary_IW.harvestNames(zone, bo3)


    @staticmethod
    def isAssetName(name: str) -> bool:
        if len(name) < 3 or len(name) > 180: return False
        for ch in name:
            if ord(ch) < 32 or ord(ch) > 126: return False
        if '/' in name or '\\' in name or name[0] == '$': return True
        return name.endswith(('.cfg', '.csv', '.gsc', '.csc', '.ddl', '.txt', '.vision', '.menu'))


    @staticmethod
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
            if Binary_IW.isAssetName(name) and name not in seen:
                seen.add(name)
                files.append(Binary_IW.FileHeader(len(files), name, start, e - start))
            i += 1
        return files

    #endregion

# Nameless fastfile rows are the index record. These factories do not read it.

#region Binary_Iwi

# C# Binary_Iwi reads the IWi header. This stub does not.
class Binary_Iwi(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Iwi()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='IWI', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Gsc

class Binary_Gsc(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Gsc()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='GSC', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Xmp

class Binary_Xmp(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Xmp()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='XMP', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Phy

class Binary_Phy(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Phy()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='PHY', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Xan

class Binary_Xan(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Xan()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='XAN', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Xmd

class Binary_Xmd(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Xmd()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='XMD', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Mat

class Binary_Mat(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Mat()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='MAT', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Tec

class Binary_Tec(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Tec()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='TEC', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Snd

class Binary_Snd(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Snd()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='SND', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Crv

class Binary_Crv(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Crv()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='CRV', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Lds

class Binary_Lds(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Lds()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='LDS', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Csp

class Binary_Csp(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Csp()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='CSP', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Cmp

class Binary_Cmp(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Cmp()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='CMP', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Cmm

class Binary_Cmm(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Cmm()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='CMM', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Gsp

class Binary_Gsp(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Gsp()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='GSP', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Gmp

class Binary_Gmp(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Gmp()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='GMP', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Ent

class Binary_Ent(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Ent()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='ENT', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Gfx

class Binary_Gfx(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Gfx()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='GFX', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Lgt

class Binary_Lgt(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Lgt()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='LGT', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Uim

class Binary_Uim(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Uim()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='UIM', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Fnt

class Binary_Fnt(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Fnt()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='FNT', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Mnf

class Binary_Mnf(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Mnf()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='MNF', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Mnu

class Binary_Mnu(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Mnu()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='MNU', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Loc

class Binary_Loc(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Loc()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='LOC', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Wep

class Binary_Wep(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Wep()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='WEP', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Sdg

class Binary_Sdg(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Sdg()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='SDG', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Fx

class Binary_Fx(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Fx()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='FX', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Ifx

class Binary_Ifx(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Ifx()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='IFX', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Ait

class Binary_Ait(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Ait()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='AIT', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Mpt

class Binary_Mpt(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Mpt()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='MPT', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Chr

class Binary_Chr(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Chr()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='CHR', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Xma

class Binary_Xma(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Xma()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='XMA', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Raw

class Binary_Raw(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Raw()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='RAW', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Tbl

class Binary_Tbl(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Tbl()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='TBL', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Pcm

class Binary_Pcm(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Pcm()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='PCM', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Xms

class Binary_Xms(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Xms()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='XMS', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Psh

class Binary_Psh(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Psh()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='PSH', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Vsh

class Binary_Vsh(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Vsh()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='VSH', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Vdl

class Binary_Vdl(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Vdl()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='VDL', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Fxm

class Binary_Fxm(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Fxm()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='FXM', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Lbd

class Binary_Lbd(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Lbd()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='LBD', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Sdd

class Binary_Sdd(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Sdd()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='SDD', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Trc

class Binary_Trc(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Trc()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='TRC', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Veh

class Binary_Veh(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Veh()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='VEH', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Ame

class Binary_Ame(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Ame()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='AME', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Gls

class Binary_Gls(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Gls()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='GLS', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Aip

class Binary_Aip(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Aip()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='AIP', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Vtr

class Binary_Vtr(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Vtr()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='VTR', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Att

class Binary_Att(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Att()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='ATT', name=os.path.basename(file.path), value=self))
        ]

#endregion

#region Binary_Sfx

class Binary_Sfx(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Sfx()

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type='SFX', name=os.path.basename(file.path), value=self))
        ]

#endregion
