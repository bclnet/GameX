from __future__ import annotations
import os
from io import BytesIO
from openx.core import BinaryReader, _pathExtension
from openx.gfx import GfxSpriteSheet
from gamex import ArcBinaryT, FileSource, MetaInfo, IHaveMetaInfo
from gamex.families.Uncore.formats.binary import Binary_Zip
from gamex.families.Uncore.formats.compression import decompressZlib

#region Binary_Abc

# Binary_Abc
class Binary_Abc(IHaveMetaInfo):
    def __init__(self, r: BinaryReader):
        pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo('BinaryPak', items = [
            # MetaInfo(f'Type: {self.type}'),
        ])
    ]

#endregion

#region Binary_Key

# Binary_Key
class Binary_Key(ArcBinaryT):
    KEY_MAGIC = 0x2059454B  # KEY 
    KEY_VERSION = 0x20203156  # V1  
    BIFF_MAGIC = 0x46464942  # BIFF
    BIFF_VERSION = 0x20203156  # V1  
    BIFC_MAGIC = 0x43464942  # BIFC
    BIFC_VERSION = 0x302E3156  # V1.0
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
        if isinstance(source.tag, (bytes, bytearray)): return BytesIO(self.slice(source.tag, file.offset, file.fileSize))
        if source.magic == self.BIFC_MAGIC:
            r.seek(0)
            magic = r.readUInt32()
            if magic != self.BIFC_MAGIC: raise Exception(f'Unknown File Type {magic}')
            return BytesIO(self.slice(self.inflateBifc(r), file.offset, file.fileSize))
        r.seek(file.offset)
        return BytesIO(r.readBytes(file.fileSize))

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
            resref = self.resRef(r.readBytes(8))
            resType = r.readUInt16()
            locator = r.readUInt32()
            if ((locator >> 14) & 0x3F) != 0: continue
            bif = (locator >> 20) & 0xFFF
            index = locator & 0x3FFF
            map_ = byBif.get(bif)
            if map_ is None: map_ = byBif[bif] = {}
            if index not in map_: map_[index] = (resref, resType)

        dir_ = os.path.dirname(source.binPath or '')
        for i, rel in enumerate(bifs):
            full = os.path.abspath(os.path.normpath(os.path.join(dir_, rel)))
            if not os.path.isfile(full): continue
            self.openBif(source, files, full, rel, byBif.get(i, {}))

    def openBif(self, source: BinaryArchive, files: list, full: str, rel: str, map_: dict) -> None:
        with BinaryReader(open(full, 'rb')) as br:
            magic = br.readUInt32()
            if magic == self.BIFF_MAGIC: self.readBiff(source, br, files, rel, full, map_, False)
            elif magic == self.BIFC_MAGIC: self.readBifc(source, br, files, rel, full, map_, False)
            else: raise Exception(f'Unknown File Type {magic}')

    def readBifc(self, source: BinaryArchive, r: BinaryReader, files: list, rel, tagPath, map_, setVersion: bool) -> None:
        inflated = self.inflateBifc(r)
        if setVersion and not isinstance(source.tag, dict): source.tag = inflated
        with BinaryReader(BytesIO(inflated)) as inner:
            magic = inner.readUInt32()
            if magic != self.BIFF_MAGIC: raise Exception(f'Unknown File Type {magic}')
            self.readBiff(source, inner, files, rel, tagPath, map_, setVersion)

    def readBiff(self, source: BinaryArchive, r: BinaryReader, files: list, rel, tagPath, map_, setVersion: bool) -> None:
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
                index = locator & 0x3FFF
                res = map_.get(index)
                if res is None: continue
                resref, resType = res
                files.append(FileSource(path=f'{rel}/{resref}{self.ext(resType)}', offset=offset, fileSize=size, tag=tagPath))
            else: files.append(FileSource(path=f'{i:04d}{self.ext(resType)}', offset=offset, fileSize=size))

    # IESDP BIFC V1.0. Cap the copy at the stated uncompressed size.
    def inflateBifc(self, r: BinaryReader) -> bytes:
        version = r.readUInt32()
        if version != self.BIFC_VERSION: raise Exception('BAD MAGIC')
        uncompressed = r.readUInt32()
        output = bytearray()
        while len(output) < uncompressed:
            declen = r.readUInt32()
            complen = r.readUInt32()
            if declen == 0 or complen == 0: raise Exception('zlib')
            chunk = decompressZlib(r, complen, declen)
            if len(chunk) < declen: raise Exception('zlib')
            take = declen if len(output) + declen <= uncompressed else uncompressed - len(output)
            output += chunk[:take]
        return bytes(output)

    def readPath(self, path: str, offset: int, size: int) -> BytesIO:
        with BinaryReader(open(path, 'rb')) as br:
            magic = br.readUInt32()
            if magic == self.BIFC_MAGIC: return BytesIO(self.slice(self.inflateBifc(br), offset, size))
            br.seek(offset)
            return BytesIO(br.readBytes(size))

    def slice(self, data, offset, size) -> bytes:
        o = int(offset); n = int(size)
        if o < 0 or n < 0 or o + n > len(data): raise Exception('Offset and length were out of bounds.')
        return bytes(data[o:o + n])

    def readName(self, r: BinaryReader, offset: int, length: int) -> str:
        if length <= 0: return ''
        r.seek(offset)
        return self.bifName(r.readBytes(length))

    def bifName(self, b) -> str:
        n = bytes(b).find(b'\x00')
        if n >= 0: b = b[:n]
        return bytes(b).decode('ascii', 'replace').replace('\\', '/')

    def resRef(self, b) -> str:
        n = bytes(b).find(b'\x00')
        if n >= 0: b = b[:n]
        return bytes(b).decode('ascii', 'replace').strip()

    def ext(self, resType: int) -> str: return self.EXTS.get(resType, '')

#endregion

#region Binary_Dat

# Binary_Dat
class Binary_Dat(ArcBinaryT):
    ZipMagic = 0x04034B50

    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        if r.length < 4: raise Exception('Binary_Dat: no sample')
        magic = r.readUInt32()
        if magic == self.ZipMagic:
            r.seek(0)
            return self.zipBinary(source.game).read(source, r, tag)
        raise Exception(f'Binary_Dat: unknown magic {magic:08X}')

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        return self.zipBinary(source.game).readData(source, r, file, option)

    @staticmethod
    def zipBinary(game) -> Binary_Zip: return Binary_Zip() if game.key is None else Binary_Zip(game.key)

#endregion

#region Binary_Da2

# Binary_Da2
class Binary_Da2(ArcBinaryT):
    GobStride = 0x28
    LmpDaStride = 0x40
    LmpRefStride = 0x0C
    MaxFiles = 100000

    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        if r.length < 4: raise Exception('Binary_Da2: no sample')
        ext = _pathExtension(source.binPath).lower()
        files = []
        if ext == '.gob': self.readGob(r, files)
        elif ext == '.lmp': self.readLmp(r, files)
        else: raise Exception(f'Binary_Da2: unknown magic {r.readUInt32():08X}')
        source.files = files

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        r.seek(file.offset)
        return BytesIO(r.readBytes(file.fileSize))

    def readGob(self, r: BinaryReader, files: list) -> None:
        length = r.length
        pos = 0
        rows = []
        while pos + self.GobStride <= length:
            r.seek(pos)
            rec = r.readBytes(self.GobStride)
            name = self.cString(rec, 0, 0x20)
            if len(name) == 0: break
            if rec[0] < 0x20 or rec[0] > 0x7E: raise Exception(f'Binary_Da2: unknown magic {int.from_bytes(rec[0:4], "little"):08X}')
            offset = int.from_bytes(rec[0x20:0x24], 'little')
            sizeField = int.from_bytes(rec[0x24:0x28], 'little')
            rows.append((name.replace('\\', '/'), offset, sizeField))
            pos += self.GobStride
        for i, (name, offset, sizeField) in enumerate(rows):
            size = sizeField if sizeField > 0 and offset + sizeField <= length else (rows[i + 1][1] if i + 1 < len(rows) else length) - offset
            if size < 0 or offset + size > length: raise Exception('Binary_Da2: bad gob entry')
            files.append(FileSource(path = name, offset = offset, fileSize = size))

    def readLmp(self, r: BinaryReader, files: list) -> None:
        length = r.length
        r.seek(0)
        count = r.readUInt32()
        if count == 0: return
        if count > self.MaxFiles: raise Exception(f'Binary_Da2: unknown magic {count:08X}')
        if self.tryDa(r, length, count, files): return
        files.clear()
        if self.tryRef(r, length, count, files): return
        raise Exception(f'Binary_Da2: unknown magic {count:08X}')

    def tryDa(self, r: BinaryReader, length: int, count: int, files: list) -> bool:
        header = 4
        if header + count * self.LmpDaStride > length: return False
        for _ in range(count):
            r.seek(header)
            rec = r.readBytes(self.LmpDaStride)
            if not self.hasCString(rec, 0, 0x38): return False
            name = self.cString(rec, 0, 0x38).replace('\\', '/')
            offset = int.from_bytes(rec[0x38:0x3C], 'little')
            size = int.from_bytes(rec[0x3C:0x40], 'little')
            if len(name) == 0 or offset + size > length: return False
            files.append(FileSource(path = name, offset = offset, fileSize = size))
            header += self.LmpDaStride
        return True

    def tryRef(self, r: BinaryReader, length: int, count: int, files: list) -> bool:
        header = 4
        if header + count * self.LmpRefStride > length: return False
        rows = []
        for _ in range(count):
            r.seek(header)
            strOff = r.readUInt32(); offset = r.readUInt32(); size = r.readUInt32()
            if strOff >= length or offset + size > length: return False
            rows.append((strOff, offset, size))
            header += self.LmpRefStride
        for strOff, offset, size in rows:
            r.seek(strOff)
            name = self.readCString(r, length - strOff).replace('\\', '/')
            if len(name) == 0: return False
            files.append(FileSource(path = name, offset = offset, fileSize = size))
        return True

    @staticmethod
    def hasCString(rec: bytes, start: int, max: int) -> bool:
        for i in range(max):
            b = rec[start + i]
            if b == 0: return True
            if b < 0x20 or b > 0x7E: return False
        return False

    @staticmethod
    def cString(rec: bytes, start: int, max: int) -> str:
        n = 0
        while n < max and rec[start + n] != 0: n += 1
        return rec[start:start + n].decode('ascii')

    @staticmethod
    def readCString(r: BinaryReader, max: int) -> str:
        raw = bytearray()
        for _ in range(max):
            b = r.readByte()
            if b == 0: break
            raw.append(b)
        return raw.decode('ascii')

#endregion

# PST irongolm.bif GLOW3 frame 0 is 10 00 10 00 08 00 08 00 2A 04 00 00: 16x16, data offset 0x42A, bit 31 clear, 98 payload bytes. Header byte 0x0B is 0.
# pillar.bif BONE has the same 0 and a clear bit 31, and its payload length equals width*height with no 0 byte, so that byte is the RLE color, not "0 means uncompressed".
# 668 of 669 PST V1 BAMs store 0 there; one stores 28. Uncompressed is bit 31 set: irongolm.bif entry 1 frame 0 is 96 00 96 00 01 00 01 00 E0 05 00 80 (0x800005E0), 32 frames, 720000 pixels, file 721504.
# Bit 31 clear is RLE and is not decoded. Cycles follow the frames as uint16 count plus uint16 lookup start. GLOW3 at file+36 is 01 00 00 00 and the lookup at 1064 is 00 00. BONE's lookup is 32 bytes for 16 uint16 indexes.

#region Binary_Bam

# Binary_Bam
class Binary_Bam(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s): return Binary_Bam(r)

    def __init__(self, r: BinaryReader):
        sig = bytes(r.readBytes(8))
        if len(sig) != 8: raise Exception('Binary_Bam: truncated')
        if sig == b'BAMCV1  ': raise Exception('Binary_Bam: compressed BAMC')
        if sig != b'BAM V1  ': raise Exception('Binary_Bam: not BAM V1')
        self.frameCount = r.readUInt16()
        cycleCount = r.readByte()
        rleColor = r.readByte()
        frameOfs = r.readUInt32()
        paletteOfs = r.readUInt32()
        lookupOfs = r.readUInt32()
        length = r.length
        def need(at, n):
            if at < 0 or n < 0 or at + n > length: raise Exception('Binary_Bam: truncated')
        need(frameOfs, 12 * self.frameCount + 4 * cycleCount)
        r.seek(frameOfs)
        frames = []
        for _ in range(self.frameCount):
            w = r.readUInt16(); h = r.readUInt16(); r.readInt16(); r.readInt16()
            frames.append((w, h, r.readUInt32()))
        heads = [(r.readUInt16(), r.readUInt16()) for _ in range(cycleCount)]
        need(paletteOfs, 1024)
        r.seek(paletteOfs)
        pal = r.readBytes(1024)
        if len(pal) != 1024: raise Exception('Binary_Bam: truncated')
        cells = []
        for w, h, data in frames:
            if (data & 0x80000000) == 0: raise Exception(f'Binary_Bam: compressed RLE (color {rleColor})')
            n = w * h
            at = data & 0x7FFFFFFF
            need(at, n)
            r.seek(at)
            idx = r.readBytes(n)
            if len(idx) != n: raise Exception('Binary_Bam: truncated')
            rgba = bytearray(n * 4)
            for i, p in enumerate(idx):
                o = p * 4
                rgba[i * 4] = pal[o + 2]
                rgba[i * 4 + 1] = pal[o + 1]
                rgba[i * 4 + 2] = pal[o]
                rgba[i * 4 + 3] = 0 if p == 0 else (255 if pal[o + 3] == 0 else pal[o + 3])
            cells.append((bytes(rgba), w, h, 0.1))
        anims = []
        if lookupOfs != 0:
            for count, first in heads:
                need(lookupOfs + 2 * first, 2 * count)
                r.seek(lookupOfs + 2 * first)
                anims.append((str(len(anims)), [r.readUInt16() for _ in range(count)]))
        self._sheet = GfxSpriteSheet.pack(cells, anims)

    def toSheet(self): return self._sheet

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo('Binary_Bam', items = [
            MetaInfo(f'Frames: {self.frameCount}'),
        ])
    ]

#endregion

#region Binary_XXX

# Binary_XXX
class Binary_XXX(ArcBinaryT):
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        source.files = []

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        raise NotImplementedError()

#endregion

def _sampleBam() -> bytes:
    import struct
    pal = bytearray(1024)
    pal[0:4] = bytes([9, 8, 7, 4])
    pal[4:8] = bytes([0, 0, 255, 0])
    buf = bytearray(b'BAM V1  ')
    buf += struct.pack('<HBBIII', 2, 1, 0, 24, 52, 1076)
    buf += struct.pack('<HHhhI', 2, 2, 0, 0, 1080 | 0x80000000)
    buf += struct.pack('<HHhhI', 2, 2, 1, 1, 1084 | 0x80000000)
    buf += struct.pack('<HH', 2, 0)
    buf += pal
    buf += struct.pack('<HH', 0, 1)
    buf += bytes([0, 1, 1, 1, 1, 1, 1, 0])
    return bytes(buf)

def _checkBam() -> None:
    bam = Binary_Bam(BinaryReader(BytesIO(_sampleBam())))
    sheet = bam.toSheet()
    assert len(sheet.frames) == 2
    assert sheet.frames[0].duration == 0.1
    assert len(sheet.anims) == 1 and list(sheet.anims[0].frames) == [0, 1]
    rgba = sheet.texture.rgba
    assert rgba[3] == 0 and bytes(rgba[0:4]) == bytes([7, 8, 9, 0])
    assert rgba[7] == 255
    raw = bytearray(_sampleBam())
    assert int.from_bytes(raw[32:36], 'little') == 1080 | 0x80000000
    raw[35] = 0
    try: Binary_Bam(BinaryReader(BytesIO(bytes(raw))))
    except Exception as e:
        if 'compressed RLE' not in str(e): raise
    else: raise Exception('compressed BAM did not throw')

if __name__ == '__main__': _checkBam()
