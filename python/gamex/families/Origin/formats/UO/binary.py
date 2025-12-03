import os, re, struct, numpy as np
# from io import BytesIO
from enum import Flag
from itertools import groupby
from openstk.gfx import Texture_Bytes, ITexture, TextureFormat, TexturePixel
from gamex import FileSource, MetaInfo, MetaContent, IHaveMetaInfo, DesSer

# typedefs
class Reader: pass
class BinaryArchive: pass
class Archive: pass
class MetaManager: pass
class TextureFlags: pass

# Binary_Anim
class Binary_Anim(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_Anim(r)

    #region Headers
    #endregion

    def __init__(self, r: Reader):
        pass

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('Anim', items = [
            # MetaInfo(f'Default: {Default.GumpID}')
            ])
        ]

# Binary_Animdata
class Binary_Animdata(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_Animdata(r)

    #region Headers

    class AnimRecord:
        _struct = ('<64s4B', 68)
        def __init__(self, tuple):
            self.frames, \
            self.unknown, \
            self.frameCount, \
            self.frameInterval, \
            self.startInterval = tuple
            self.frames = struct.unpack('<64B', self.frames)

    class Record:
        def __init__(self, record: object):
            self.frames = record.frames
            self.frameCount = record.frameCount
            self.frameInterval = record.frameInterval
            self.startInterval = record.startInterval

    records: dict[int, Record] = {}

    #endregion

    def __init__(self, r: Reader):
        id = 0
        length = int(r.length / (4 + (8 * (64 + 4))))
        for i in range(length):
            r.skip(4)
            records = r.readSArray(self.AnimRecord, 8)
            for j in range(8):
                record = records[j]
                if record.frameCount > 0:
                    self.records[id] = self.Record(record)
                id += 1

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('Animdata', items = [
            MetaInfo(f'Records: {len(self.records)}')
            ])
        ]

# Binary_AsciiFont
class Binary_AsciiFont(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_AsciiFont(r)

    #region Headers

    class AsciiFont:
        characters: list[list[int]] = [None]*224
        height: int = 0
        def __init__(self, r: Reader):
            r.readByte()
            for i in range(224):
                width = r.readByte(); height = r.readByte()
                r.readByte()
                if width <= 0 or height <= 0: continue
                if height > self.height and i < 96: self.height = height

                length = width * height
                bd = list(np.frombuffer(r.readBytes(length << 1), dtype = np.uint16))
                for j in range(length):
                    if bd[j] != 0: bd[j] ^= 0x8000
                self.characters[i] = np.array(bd).tobytes()
    
    fonts: list[AsciiFont] = [None]*10

    #endregion

    def __init__(self, r: Reader):
        for i in range(len(self.fonts)): self.fonts[i] = self.AsciiFont(r)

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('AsciiFont', items = [
            MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_BodyConverter
class Binary_BodyConverter(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_BodyConverter(r)

    #region Headers

    table1: list[int]
    table2: list[int]
    table3: list[int]
    table4: list[int]

    def contains(body: int) -> bool:
          return True if self.table1 & body >= 0 & body < len(self.table1) & self.table1[body] != -1 else \
          True if self.table2 & body >= 0 & body < len(self.table2) & self.table2[body] != -1 else \
          True if self.table3 & body >= 0 & body < len(self.table3) & self.table3[body] != -1 else \
          True if self.table4 & body >= 0 & body < len(self.table4) & self.table4[body] != -1 else \
          False
    
    def convert(body: int) -> (int, int):
        if self.table1 & body >= 0 & body < len(self.table1):
            val = self.table1[body]
            if val != -1: return (2, val)
        if self.table2 & body >= 0 & body < len(self.table2):
            val = self.table2[body]
            if val != -1: return (3, val)
        if self.table3 & body >= 0 & body < len(self.table3):
            val = self.table3[body]
            if val != -1: return (4, val)
        if self.table4 & body >= 0 & body < len(self.table4):
            val = self.table4[body]
            if val != -1: return (5, val)
        return (1, body)

    def getTrueBody(fileType: int, index: int) -> int:
        match fileType:
            case 1: return index
            case 2:
                if self.table1 & index >= 0:
                    for i in range(len(self.table1)):
                        if self.table1[i] == index: return i
            case 3:
                if self.table2 & index >= 0:
                    for i in range(len(self.table2)):
                        if self.table2[i] == index: return i
            case 4:
                if self.table3 & index >= 0:
                    for i in range(len(self.table3)):
                        if self.table3[i] == index: return i
            case 5:
                if self.table4 & index >= 0:
                    for i in range(len(self.table4)):
                        if self.table4[i] == index: return i
            case _: return index
        return -1

    #endregion

    def __init__(self, r: Reader):
        list1 = []; list2 = []; list3 = []; list4 = []
        max1 = max2 = max3 = max4 = 0

        line: str
        while (line := r.readLine()):
            line = line.strip()
            if not line or line.startswith('#') or line.startswith('"#'): continue

            try:
                split = [x for x in re.split('\t| ', line) if x]
                hasOriginalBodyId = split[0].isdecimal()
                if not hasOriginalBodyId: continue
                original = int(split[0])

                anim2 = int(split[1]) if split[1].isdecimal() else -1
                anim3 = int(split[2]) if split[2].isdecimal() else -1
                anim4 = int(split[3]) if split[3].isdecimal() else -1
                anim5 = int(split[4]) if split[4].isdecimal() else -1

                if anim2 != -1:
                    if anim2 == 68: anim2 = 122
                    if original > max1: max1 = original
                    list1.append(original)
                    list1.append(anim2)
                if anim3 != -1:
                    if original > max2: max2 = original
                    list2.append(original)
                    list2.append(anim3)
                if anim4 != -1:
                    if original > max3: max3 = original
                    list3.append(original)
                    list3.append(anim4)
                if anim5 != -1:
                    if original > max4: max4 = original
                    list4.append(original)
                    list4.append(anim5)
            except: pass

            self.table1 = [-1]*(max1 + 1)
            for i in range(0, len(list1), 2): self.table1[list1[i]] = list1[i + 1]

            self.table2 = [-1]*(max2 + 1)
            for i in range(0, len(list2), 2): self.table2[list2[i]] = list2[i + 1]

            self.table3 = [0]*(max3 + 1)
            for i in range(0, len(list3), 2): self.table3[list3[i]] = list3[i + 1]

            self.table4 = [0]*(max4 + 1)
            for i in range(0, len(list4), 2): self.table4[list4[i]] = list4[i + 1]

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('BodyConverter', items = [
            MetaInfo(f'Table1: {len(self.table1)}'),
            MetaInfo(f'Table2: {len(self.table2)}'),
            MetaInfo(f'Table3: {len(self.table3)}'),
            MetaInfo(f'Table4: {len(self.table4)}'),
            ])
        ]

# Binary_BodyTable
class Binary_BodyTable(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_BodyTable(r)

    #region Headers

    class Record:
        def __init__(self, oldId: int, newId: int, newHue: int):
            self.oldId = oldId
            self.newId = newId
            self.newHue = newHue

    records: dict[int, Record] = {}
    
    #endregion

    def __init__(self, r: Reader):
        line: str
        while (line := r.readLine()):
            line = line.strip()
            if not line or line.startswith('#') or line.startswith('"#'): continue

            try:
                index1 = line.find('{')
                index2 = line.find('}')

                param1 = line[:index1]
                param2 = line[index1 + 1: index2]
                param3 = line[(index2 + 1):]

                indexOf = param2.find(',')
                if indexOf > -1: param2 = param2[:indexOf].strip()

                oldId = int(param1)
                newId = int(param2)
                newHue = int(param3)
                self.records[oldId] = self.Record(oldId, newId, newHue)
            except: pass

        def __repr__(self): return DesSer.serialize(self)
        
        def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('BodyTable', items = [
            MetaInfo(f'Records: {len(self.records)}')
            ])
        ]

# Binary_CalibrationInfo
class Binary_CalibrationInfo(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_CalibrationInfo(r)

    #region Headers

    class Record:
        def __init__(self, mask: bytes, vals: bytes, detX: bytes, detY: bytes, detZ: bytes, detF: bytes):
            self.mask = mask
            self.vals = vals
            self.detX = detX
            self.detY = detY
            self.detZ = detZ
            self.detF = detF

    records: list[Record] = []

    defaultRecords: list[Record] = [
        Record(
            # Post 7.0.4.0 (Andreew)
            bytes([
                0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF,
                0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF
            ]),
            bytes([
                0xFF, 0xD0, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x11, 0x8B,
                0x82, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xD0, 0x5B, 0x83, 0x00, 0x00, 0x00, 0x00, 0x00, 0xEC
            ]),
            bytes([ 0x22, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x0C ]), # x
            bytes([ 0x22, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x08 ]), # y
            bytes([ 0x22, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x04 ]), # z
            bytes([ 0x22, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x10 ])),# f
        Record(
            # (arul) 6.0.9.x+ : Calibrates both
            bytes([
                0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF,
                0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF
            ]),
            bytes([
                0xFF, 0xD0, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x11, 0x8B,
                0x82, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xD0, 0x5E, 0xE9, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x0D
            ]),
            bytes([ 0x1F, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x0C ]),
            bytes([ 0x1F, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x08 ]),
            bytes([ 0x1F, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x04 ]),
            bytes([ 0x1F, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x10 ])),
        Record(
            # Facet
            bytes([
                0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
            ]),
            bytes([
                0xA0, 0x00, 0x00, 0x00, 0x00, 0x84, 0xC0, 0x0F, 0x85, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x0D
            ]),
            bytes([]),
            bytes([]),
            bytes([]),
            bytes([ 0x01, 0x04, 0xFF, 0xFF, 0xFF, 0x01 ])),
        Record(
            # Location
            bytes([
                0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0x00, 0x00,
                0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x00
            ]),
            bytes([
                0x8B, 0x15, 0x00, 0x00, 0x00, 0x00, 0x83, 0xC4, 0x10, 0x66, 0x89, 0x5A, 0x00, 0xA1, 0x00, 0x00,
                0x00, 0x00, 0x66, 0x89, 0x78, 0x00, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x66, 0x89, 0x71, 0x00
            ]),
            bytes([ 0x02, 0x04, 0x04, 0x0C, 0x01, 0x02 ]),
            bytes([ 0x0E, 0x04, 0x04, 0x15, 0x01, 0x02 ]),
            bytes([ 0x18, 0x04, 0x04, 0x1F, 0x01, 0x02 ]),
            bytes([])),
        Record(
            # UO3D Only, calibrates both
            bytes([
                0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF,
                0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00
            ]),
            bytes([
                0xA1, 0x00, 0x00, 0x00, 0x00, 0x68, 0x40, 0x2E, 0x04, 0x01, 0x0F, 0xBF, 0x50, 0x00, 0x0F, 0xBF,
                0x48, 0x00, 0x52, 0x51, 0x0F, 0xBF, 0x50, 0x00, 0x52, 0x8D, 0x85, 0xE4, 0xFD, 0xFF, 0xFF, 0x68,
                0x00, 0x00, 0x00, 0x00, 0x50, 0xE8, 0x07, 0x44, 0x10, 0x00, 0x8A, 0x0D, 0x00, 0x00, 0x00, 0x00
            ]),
            bytes([ 0x01, 0x04, 0x04, 0x17, 0x01, 0x02 ]),
            bytes([ 0x01, 0x04, 0x04, 0x11, 0x01, 0x02 ]),
            bytes([ 0x01, 0x04, 0x04, 0x0D, 0x01, 0x02 ]),
            bytes([ 0x2C, 0x04, 0xFF, 0xFF, 0xFF, 0x01 ]))
        ]

    #endregion

    def __init__(self, r: Reader):
        line: str
        while (line := r.readLine()):
            line = line.strip()
            if line.lower() != 'begin': continue

            mask, vals, detx, dety, detz, detf
            if (mask := readBytes(r)) == None: continue
            if (vals := readBytes(r)) == None: continue
            if (detx := readBytes(r)) == None: continue
            if (dety := readBytes(r)) == None: continue
            if (detz := readBytes(r)) == None: continue
            if (detf := readBytes(r)) == None: continue
            self.records.append(self.Record(mask, vals, detx, dety, detz, detf))
        self.records += self.defaultRecords

    @staticmethod
    def readBytes(r: Reader) -> bytes:
        line = r.readLine()
        if not line: return None

        b = bytes((line.Length + 2) / 3)
        index = 0
        for i in range(0, line.length + 1, 3):
            ch = line[i + 0]
            cl = line[i + 1]

            if ch >= '0' & ch <= '9': ch -= '0'
            elif ch >= 'a' & ch <= 'f': ch -= ('a' - 10)
            elif ch >= 'A' & ch <= 'F': ch -= ('A' - 10)
            else: return None

            if cl >= '0' & cl <= '9': cl -= '0'
            elif cl >= 'a' & cl <= 'f': cl -= ('a' - 10)
            elif cl >= 'A' & cl <= 'F': cl -= ('A' - 10)
            else: return None

            b[index] = ((ch << 4) | cl) & 0xff; index += 1
        return b
    
    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('CalibrationInfo', items = [
            MetaInfo(f'Records: {len(self.records)}')
            ])
        ]

# Binary_Gump
class Binary_Gump(IHaveMetaInfo, ITexture):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_Gump(r, f.fileSize, f.compressed)

    def __init__(self, r: Reader, length: int, extra: int):
        width = self.width = (extra >> 16) & 0xFFFF
        height = self.height = extra & 0xFFFF
        self.pixels = []
        if width <= 0 | height <= 0: return
        self.load(r.readBytes(length), width, height)

    def load(self, data: bytes, width: int, height: int) -> None:
        lookup = np.frombuffer(data, dtype = np.int32); dat = np.frombuffer(data, dtype = np.uint16); lookup_ = 0; datLen = len(dat)
        bd = self.pixels = bytearray(width * height << 1)
        mv = memoryview(np.frombuffer(bd, dtype = np.uint16))
        line = 0
        for y in range(height):
            count = lookup[lookup_] << 1; lookup_ += 1
            cur = line; end = line + width
            while cur < end:
                if count < datLen: color = dat[count + 0]; next = cur + int(dat[count + 1]); count += 2
                else: color = 0
                if color == 0: cur = next
                else:
                    color ^= 0x8000
                    while cur < next: mv[cur] = color; cur += 1
            line += width

    #region ITexture

    format: tuple = (TextureFormat.BGRA1555, TexturePixel.Unknown)
    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0
    def create(self, platform: str, func: callable): return func(Texture_Bytes(self.pixels, self.format, None))

    #endregion

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Gump', items = [
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}')
            ])
        ]

# Binary_GumpDef
class Binary_GumpDef(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_GumpDef(r)

    #region Headers

    #endregion

    def __init__(self, r: Reader):
        pass

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_Hues
class Binary_Hues(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_Hues(r)

    #region Headers

    #endregion

    def __init__(self, r: Reader):
        pass

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_Land
class Binary_Land(IHaveMetaInfo, ITexture):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_Land(r, f.fileSize)

    #region Headers

    #endregion

    def __init__(self, r: Reader, length: int):
        bdata = np.frombuffer(r.readBytes(length), dtype = np.uint16); bdata_ = 0
        bd = self.pixels = bytearray(44 * 44 << 1)
        mv = memoryview(np.frombuffer(bd, dtype = np.uint16))
        width = 44
        line = 0
        # run
        xOffset = 21; xRun = 2
        for y in range(22):
            cur = line + xOffset; end = cur + xRun
            while cur < end: mv[cur] = bdata[bdata_] ^ 0x8000; cur += 1; bdata_ += 1
            xOffset -= 1; xRun += 2; line += width
        # run
        xOffset = 0; xRun = 44
        for y in range(22):
            cur = line + xOffset; end = cur + xRun
            while cur < end: mv[cur] = bdata[bdata_] ^ 0x8000; cur += 1; bdata_ += 1
            xOffset += 1; xRun -= 2; line += width

    #region ITexture

    format: tuple = (TextureFormat.BGRA1555, TexturePixel.Unknown)
    width: int = 44
    height: int = 44
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0
    def create(self, platform: str, func: callable): return func(Texture_Bytes(self.pixels, self.format, None))

    #endregion

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Land', items = [
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}')
            ])
        ]

# Binary_Light
class Binary_Light(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_Light(r)

    #region Headers

    #endregion

    def __init__(self, r: Reader):
        pass

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_MobType
class Binary_MobType(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_MobType(r)

    #region Headers

    #endregion

    def __init__(self, r: Reader):
        pass

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_MultiMap
class Binary_MultiMap(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_MultiMap(r)

    #region Headers

    #endregion

    def __init__(self, r: Reader):
        pass

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_MusicDef
class Binary_MusicDef(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_MusicDef(r)

    #region Headers

    #endregion

    def __init__(self, r: Reader):
        pass

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_Multi
class Binary_Multi(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_Multi(r)

    #region Headers

    #endregion

    def __init__(self, r: Reader):
        pass

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_RadarColor
class Binary_RadarColor(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_RadarColor(r)

    #region Headers

    #endregion

    def __init__(self, r: Reader):
        pass

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_SkillGroups
class Binary_SkillGroups(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_SkillGroups(r)

    #region Headers

    #endregion

    def __init__(self, r: Reader):
        pass

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_Skills
class Binary_Skills(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_Skills(r)

    #region Headers

    #endregion

    def __init__(self, r: Reader):
        pass

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_Sound
class Binary_Sound(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_Sound(r)

    #region Headers

    #endregion

    def __init__(self, r: Reader):
        pass

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_SpeechList
class Binary_SpeechList(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_SpeechList(r)

    #region Headers

    #endregion

    def __init__(self, r: Reader):
        pass

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_Art
class Binary_Art(IHaveMetaInfo, ITexture):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_Art(r, f.fileSize)

    #region Headers

    #endregion

    def __init__(self, r: Reader, length: int):
        bdata = np.frombuffer(r.readBytes(length), dtype = np.uint16); bdata_ = 0
        count = 2
        width = self.width = bdata[count]; count += 1
        height = self.height = bdata[count]; count += 1
        if width <= 0 or height <= 0: return
        start = height + 4
        lookups = [start + bdata[count + i] for i in range(height)]; count += height
        bd = self.pixels = bytearray(width * height << 1)
        mv = memoryview(np.frombuffer(bd, dtype = np.uint16))
        line = 0
        for y in range(height):
            count = lookups[y]
            cur = line
            xOffset = xRun = 0
            while (xOffset := bdata[count+0]) + (xRun := bdata[count+1]) != 0:
                count += 2
                if xOffset > width: break
                cur += xOffset
                if xOffset + xRun > width: break
                # run
                end = cur + xRun
                while cur < end: mv[cur] = bdata[count] ^ 0x8000; cur += 1; count += 1
            line += width

    #region ITexture

    format: tuple = (TextureFormat.BGRA1555, TexturePixel.Unknown)
    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0
    def create(self, platform: str, func: callable): return func(Texture_Bytes(self.pixels, self.format, None))

    #endregion

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Art', items = [
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}')
            ])
        ]

# Binary_StringTable
class Binary_StringTable(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_StringTable(r, f)
    _current: dict[str, 'Binary_StringTable'] = {}

    #region Headers

    class RecordFlag(Flag):
        Original = 0x0
        Custom = 0x1
        Modified = 0x2

    class Record:
        def __init__(self, text: str, flag: 'RecordFlag'):
            self.text = text
            self.flag = flag

    #endregion

    strings: dict[int, str] = {}
    records: dict[int, Record] = {}

    def __init__(self, r: Reader, f: FileSource):
        r.skip(6)
        while not r.atEnd():
            id = r.readInt32()
            flag = Binary_StringTable.RecordFlag(r.readByte())
            text = r.readL16UString()
            self.records[id] = Binary_StringTable.Record(text, flag)
            self.strings[id] = text
        Binary_StringTable._current[os.path.splitext(f.path[1:])[1]] = self

    def getString(self, id: int) -> str: return z if 'enu' in Binary_StringTable._current and (y := Binary_StringTable._current['enu']) and id in y.strings[id] and (z := y.strings[id]) else ''

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('StringTable', items = [
            MetaInfo(f'Count: {len(self.records)}')
            ])
        ]

# Binary_TileData
class Binary_TileData(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_TileData(r)

    #region Headers

    #endregion

    def __init__(self, r: Reader):
        pass

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_UnicodeFont
class Binary_UnicodeFont(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_UnicodeFont(r)

    #region Headers

    #endregion

    def __init__(self, r: Reader):
        pass

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_Verdata
class Binary_Verdata(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_Verdata(r, s)
    instance: object = None

    #region Headers

    class Patch:
        _struct = ('<5i ', 20)
        def __init__(self, tuple):
            self.file, \
            self.index, \
            self.offset, \
            self.fileSize, \
            self.extra = tuple

    #endregion

    def __init__(self, r: Reader, s: BinaryArchive):
        self.archive = s
        patches = r.readL32SArray(self.Patch); print(patches); patches.sort()
        self.patches = { k: list(g) for k, g in groupby(patches) }
        Binary_Verdata.instance = self

    def readData(self, offset: int, fileSize: int): return Archive.ReaderT(lambda r: BytesIO(r.seek(offset).readBytes(fileSize)))

    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('Verdata', items = [
                MetaInfo(f'Patches: {len(self.patches)}')
            ])
        ]
