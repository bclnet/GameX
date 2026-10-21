from __future__ import annotations
import os
from numpy import array
from io import BytesIO
from PIL import Image
from enum import Enum
from quaternion import quaternion
from openx.core import _pathExtension, EngineX, Int3, ICellDatabase, CellManager, log, Writer
from openx.gfx import Raster, DDS_HEADER, TextureAsDds, TextureAsBytes, ITexture, TextureFormat, TexturePixel
from gamex import ArcBinary, ArcBinaryT, FileSource, BinaryArchive, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo
from .apple.plist_reader import PlistReader
from zipfile import ZipFile

#region Binary_Bik

# Binary_Bik
class Binary_Bik(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Bik(r, f.fileSize)

    def __init__(self, r: BinaryReader, fileSize: int):
        self.data = r.readBytes(fileSize)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'BIK Video'))
        ]

#endregion

#region Binary_Dds

# Binary_Dds
class Binary_Dds(IHaveMetaInfo, ITexture):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Dds(r, f)

    def __init__(self, r: BinaryReader, f: FileSource, readMagic: bool = True):
        self.capsReadDds = EngineX.Caps.ReadDds in EngineX.current.caps
        if self.capsReadDds: self.bytes = r.readBytes(f.fileSize); return
        self.header, self.headerDXT10, self.format, self.bytes = DDS_HEADER.read(r, readMagic)
        width = self.header.dwWidth; height = self.header.dwHeight; mipMaps = max(1, self.header.dwMipMapCount)
        offset = 0
        self.spans = [range(-1, 0)] * mipMaps
        for i in range(mipMaps):
            w = width >> i; h = height >> i
            if w == 0 or h == 0: self.spans[i] = range(-1, 0); continue
            size = int(((w + 3) / 4)) * int((h + 3) / 4) * self.format[1]
            remains = min(size, len(self.bytes) - offset)
            # print(f'w: {w}, h: {h}, s: {size}, r: {remains}')
            self.spans[i] = range(offset, (offset + remains)) if remains > 0 else range(-1, 0)
            offset += remains
        self.width = width
        self.height = height
        self.mipMaps = mipMaps

    #region ITexture

    format: tuple = (0, 0, 0)
    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0
    def create(self, platform: str, func: callable): return func(TextureAsDds(self.bytes) if self.capsReadDds else TextureAsBytes(self.bytes, self.format[2], self.spans))

    #endregion

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Texture', items = [
            MetaInfo(f'Format: {self.format[0]}'),
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}'),
            MetaInfo(f'Mipmaps: {self.mipMaps}')
            ])
        ]

#endregion

#region Binary_Fsb

# Binary_Fsb
class Binary_Fsb(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Fsb(r, f.fileSize)

    def __init__(self, r: BinaryReader, fileSize: int):
        self.data = r.readBytes(fileSize)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'FSB Audio'))
        ]

#endregion

#region Binary_Iif

def _i32(v: int) -> int:
    v &= 0xFFFFFFFF
    return v - 0x100000000 if v & 0x80000000 else v

def _i16(v: int) -> int:
    v &= 0xFFFF
    return v - 0x10000 if v & 0x8000 else v

def _readChunk(r: BinaryReader) -> tuple[int, int]:
    # MarshalS ">4xI": id is little-endian, size is big-endian.
    raw = r.readBytes(8)
    return int.from_bytes(raw[0:4], 'little'), int.from_bytes(raw[4:8], 'big', signed=True)

def _actualSize(size: int) -> int: return size + (size & 1)

def _vlq(r: BinaryReader) -> int:
    value = 0
    for _ in range(4):
        value = _i32(value << 7)
        b = r.readByte()
        value = _i32(value | (b & 0x7F))
        if (b & 0x80) == 0: break
    return value

def _vlq2(r: BinaryReader) -> int:
    value = 0
    i = 0
    while i < 4:
        b = r.readByte()
        if (b & 0x80) != 0: break
        value += b
        i += 1
    r.skip(-1)
    return value

def _putVlq(w: Writer, value: int) -> None:
    value = _i32(value)
    buffer = value & 0x7F
    while True:
        value = _i32(value >> 7)
        if value == 0: break
        buffer = _i32(((buffer & 0xFFFFFFFF) << 8) | ((value & 0x7F) | 0x80))
    while True:
        w.writeByte(buffer & 0xFF)
        if (buffer & 0x80) != 0: buffer = _i32(buffer >> 8)
        else: break

# Binary_Iif
class Binary_Iif(IHaveMetaInfo):
    class Chunk:
        FORM = 0x4d524f46
        CAT_ = 0x20544143
        XDIR = 0x52494458
        XMID = 0x44494d58
        INFO = 0x4f464e49
        TIMB = 0x424d4954
        EVNT = 0x544e5645
        _struct = ('>4xI', 8)
    class TIMB:
        _struct = ('<2x', 2)
        def __init__(self, patchNumber: int, timbreBank: int):
            self.patchNumber = patchNumber
            self.timbreBank = timbreBank
    class MidiEvent:
        def __init__(self, time: int):
            self.time = time
            self.status = 0
            self.data0 = 0
            self.data1 = 0
            self.length = 0
            self.stream = None
            self.next = None
    class EV(int):
        NOTE_OFF = 0x80
        NOTE_ON = 0x90
        POLY_PRESS = 0xa0
        CONTROL = 0xb0
        PROGRAM = 0xc0
        CHAN_PRESS = 0xd0
        PITCH = 0xe0
        SYSEX = 0xf0
        ESC = 0xf7
        META = 0xff
    class EVT(int):
        OUTPUT_CABLE = 0x21
        EOT = 0x2f
        TRACK_END = 0x2f
        TEMPO = 0x51
        SET_TEMPO = 0x51
        TIME_SIG = 0x58
        KEYSIG = 0x59

    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Iif(r, f.fileSize)

    def __init__(self, r: BinaryReader, fileSize: int):
        self.tracks = 0
        self.events = None
        self.timing = None
        self.timbres = None
        self.theList = None
        self.curTrack = 0
        self.curEvent = None
        self.curEventList = None
        self._read(r, fileSize)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self))
        ]

    def _alloc(self) -> None:
        if self.events is not None: return
        self.events = [None] * self.tracks
        self.timing = [0] * self.tracks
        self.timbres = [None] * self.tracks

    def _read(self, r: BinaryReader, fileSize: int) -> bool:
        self.tracks = 1
        while True:
            id_, size = _readChunk(r)
            position = r.tell()
            result = self._handleFORM(r, size) if id_ == self.Chunk.FORM else self._handleCAT(r, size) if id_ == self.Chunk.CAT_ else False
            if not result: return False
            newPosition = position + _actualSize(size)
            if r.tell() != newPosition: r.seek(position + _actualSize(size))
            if not r.tell() < fileSize: break
        return True

    def _handleFORM(self, r: BinaryReader, chunkSize: int) -> bool:
        kind = r.readUInt32()
        if kind == self.Chunk.XDIR: return self._handleXDIR(r, chunkSize - 4)
        if kind == self.Chunk.XMID: return self._handleXMID(r, chunkSize - 4)
        return False

    def _handleCAT(self, r: BinaryReader, chunkSize: int) -> bool:
        basePosition = r.tell()
        endPosition = r.tell() + chunkSize
        while True:
            id_, size = _readChunk(r)
            position = r.tell()
            if id_ == self.Chunk.XMID:
                r.skip(-4); position -= 4
                size = int(chunkSize - (position - basePosition))
            result = self._handleFORM(r, size) if id_ == self.Chunk.FORM else self._handleXMID(r, size) if id_ == self.Chunk.XMID else False
            if not result: return False
            newPosition = position + _actualSize(size)
            if r.tell() != newPosition: r.seek(position + _actualSize(size))
            if not r.tell() < endPosition: break
        return True

    def _handleXDIR(self, r: BinaryReader, chunkSize: int) -> bool:
        if chunkSize != 10: return False
        _id, size = _readChunk(r)
        # C# compares Size to Chunk.INFO, and rejects every size other than 2.
        if size == self.Chunk.INFO or size != 2: return False
        self.tracks = r.readUInt16()
        self._alloc()
        return True

    def _handleXMID(self, r: BinaryReader, chunkSize: int) -> bool:
        endPosition = r.tell() + chunkSize
        while True:
            id_, size = _readChunk(r)
            position = r.tell()
            result = self._handleFORM(r, size) if id_ == self.Chunk.FORM else self._handleTIMB(r, size) if id_ == self.Chunk.TIMB else self._handleEVNT(r, size) if id_ == self.Chunk.EVNT else False
            if not result: return False
            newPosition = position + _actualSize(size)
            if r.tell() != newPosition: r.seek(position + _actualSize(size))
            if not r.tell() < endPosition: break
        return True

    def _handleTIMB(self, r: BinaryReader, chunkSize: int) -> bool:
        numTimbres = r.readUInt16()
        if (numTimbres << 1) + 2 != chunkSize: return False
        timbres = []
        for _ in range(numTimbres):
            raw = r.readBytes(2)
            timbres.append(self.TIMB(raw[0], raw[1]))
        self.timbres[self.curTrack] = timbres
        return True

    def _handleEVNT(self, r: BinaryReader, chunkSize: int) -> bool:
        self._alloc()
        self.curEventList = None
        timing = self._readEventList(r)
        if timing == 0: log.info('Unable to convert data\n'); return False
        self.timing[self.curTrack] = timing
        self.events[self.curTrack] = self.curEventList
        self.curTrack += 1
        return True

    def _createNew(self, time: int):
        if self.curEventList is None:
            self.curEvent = self.curEventList = self.MidiEvent(time)
            return self.curEvent
        if self.curEvent.time > time: self.curEvent = self.curEventList
        while self.curEvent.next is not None:
            if self.curEvent.next.time > time:
                ev = self.MidiEvent(time); ev.next = self.curEvent.next
                self.curEvent.next = ev
                self.curEvent = ev
                return self.curEvent
            self.curEvent = self.curEvent.next
        self.curEvent.next = self.MidiEvent(time)
        self.curEvent = self.curEvent.next
        return self.curEvent

    def _convertEvent(self, r: BinaryReader, time: int, status: int, size: int) -> None:
        current = self._createNew(time)
        current.status = status
        current.data0 = r.readByte()
        if size == 1: return
        current.data1 = r.readByte()
        if size == 2: return
        prev = current
        delta = _vlq(r)
        current = self._createNew(time + delta * 3)
        current.status = status
        current.data0 = prev.data0
        current.data1 = 0
        self.curEvent = prev

    def _convertSystem(self, r: BinaryReader, time: int, status: int) -> None:
        current = self._createNew(time)
        current.status = status
        if current.status == self.EV.META: self.curEvent.data0 = r.readByte()
        self.curEvent.length = _vlq(r)
        if self.curEvent.length == 0: return
        self.curEvent.stream = r.readBytes(self.curEvent.length)

    def _readEventList(self, r: BinaryReader) -> int:
        time = 0
        tempo = 500000
        tempoSet = False
        while True:
            status = r.readByte()
            ev = status & 0xF0
            if ev == self.EV.NOTE_OFF: log.info('ERROR: Note off not valid in XMidiFile\n'); return 0
            elif ev == self.EV.NOTE_ON: self._convertEvent(r, time, status, 3)
            elif ev == self.EV.POLY_PRESS or ev == self.EV.CONTROL or ev == self.EV.PITCH: self._convertEvent(r, time, status, 2)
            elif ev == self.EV.PROGRAM or ev == self.EV.CHAN_PRESS: self._convertEvent(r, time, status, 1)
            elif ev == self.EV.SYSEX:
                if status == self.EV.META:
                    evt = r.readByte()
                    if evt == self.EVT.TRACK_END: return _i16((tempo * 9) // 25000)
                    if evt == self.EVT.SET_TEMPO and not tempoSet:
                        tempoSet = True
                        r.skip(1)
                        tempo = (r.readByte() << 16) | (r.readByte() << 8) | r.readByte()
                        r.skip(-4)
                    r.skip(-1)
                self._convertSystem(r, time, status)
            else:
                r.skip(-1)
                time += _vlq2(r) * 3

    def writeAll(self) -> None:
        for _i in range(self.tracks): pass

    def writeMidi(self, w: Writer, track: int) -> bool:
        if self.events is None or track > self.tracks: return False
        w.writeUInt32(0x6468544d)
        w.writeInt32E(6)
        w.writeInt16E(0)
        w.writeInt16E(1)
        w.writeInt16E(self.timing[track])
        return self._writeMidiMTrk(w, self.events[track])

    def _writeMidiMTrk(self, w: Writer, evnts) -> bool:
        XMIDI_CONTROLLER_NEXT_BREAK = 117
        time = 0
        lastStatus = 0
        sshockBreak = False
        w.writeUInt32(0x6b72544d)
        sizePosition = w.f.tell()
        w.writeUInt32(0)
        evnt = evnts
        while evnt is not None:
            delta = 0 if sshockBreak else evnt.time - time
            time = evnt.time
            _putVlq(w, delta)
            if evnt.status != lastStatus or evnt.status >= self.EV.SYSEX: w.writeByte(evnt.status)
            lastStatus = evnt.status
            kind = evnt.status & 0xF0
            if kind == self.EV.NOTE_OFF or kind == self.EV.NOTE_ON or kind == self.EV.POLY_PRESS or kind == self.EV.PITCH:
                w.writeByte(evnt.data0); w.writeByte(evnt.data1)
            elif kind == self.EV.CONTROL:
                w.writeByte(evnt.data0); w.writeByte(evnt.data1)
                if evnt.data0 == XMIDI_CONTROLLER_NEXT_BREAK: sshockBreak = True
            elif kind == self.EV.PROGRAM or kind == self.EV.CHAN_PRESS: w.writeByte(evnt.data0)
            elif kind == self.EV.SYSEX:
                if evnt.status == self.EV.META: w.writeByte(evnt.data0)
                _putVlq(w, evnt.length)
                if evnt.length != 0: w.f.write(evnt.stream)
            else: log.info('Not supposed to see this')
            evnt = evnt.next
        position = w.f.tell()
        size = position - sizePosition
        w.seek(sizePosition)
        w.writeUInt32E(size & 0xFFFFFFFF)
        w.seek(position)
        return True

#endregion

#region Binary_Img

# Binary_Img
class Binary_Img(IHaveMetaInfo, ITexture):
    class BmpHeader:
        # sizeof includes the nested BmpInfoHeader, so 54 not 14.
        _struct = ('<H3i', 54)
    class BmpInfoHeader:
        _struct = ('<3I2H6I', 40)

    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Img(r, f)

    def __init__(self, r: BinaryReader, f: FileSource):
        self.image = Image.open(r.f)
        self.width, self.height = self.image.size
        bytes = self.image.tobytes(); palette = self.image.getpalette()
        # print(f'mode: {self.image.mode}')
        match self.image.mode:
            # 1-bit pixels, black and white
            case '1':  self.format = (self.image.format, (TextureFormat.L8, TexturePixel.Unknown))
            # 8-bit pixels, mapped to any other mode using a color palette
            case 'P' | 'L':
                self.format = (self.image.format, (TextureFormat.RGB24, TexturePixel.Unknown))
                # 8-bit pixels, Grayscale. C# Format8bppIndexed is 256 entries; 255 drops index 255.
                if self.image.mode == 'L': palette = [x for xs in [[x, x, x] for x in range(256)] for x in xs]
            # 3×8-bit pixels, true color
            case 'RGB': self.format = (self.image.format, (TextureFormat.RGB24, TexturePixel.Unknown))
            # 4×8-bit pixels, true color with transparency mask
            case 'RGBA': self.format = (self.image.format, (TextureFormat.RGBA32, TexturePixel.Unknown))
            case _: raise Exception(f'Unknown format: {self.image.mode}')

        # decode
        if not palette: self.bytes = bytes
        else:
            self.bytes = bytearray(self.width * self.height * 3)
            Raster.blitByPalette(self.bytes, 3, bytes, palette, 3)

    #region ITexture

    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0
    def create(self, platform: str, func: callable): return func(TextureAsBytes(self.bytes, self.format[1], None))

    #endregion

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Texture', items = [
            MetaInfo(f'Format: {self.format[0]}'),
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}'),
            MetaInfo(f'Mipmaps: {self.mipMaps}')
            ])
        ]

#endregion

#region Binary_Msg

# Binary_Msg
class Binary_Msg(IHaveMetaInfo):
    @staticmethod
    async def factory(message: str): return Binary_Msg(message)

    def __init__(self, message: str):
        self.message = message

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self.message))
        ]

#endregion

#region Binary_Pcx

# Binary_Pcx
class Binary_Pcx(IHaveMetaInfo, ITexture):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Pcx(r, f.fileSize)

    #region Headers

    class X_Header:
        _struct = ('<4B6H48s2B4H54s', 128)
        def __init__(self, t):
            (self.manufacturer,
            self.version,
            self.encoding,
            self.bpp,
            self.xmin,
            self.ymin,
            self.xmax,
            self.ymax,
            self.hdpi,
            self.vdpi,
            self.palette,
            self.reserved1,
            self.numPlanes,
            self.bpl,
            self.mode,
            self.hres,
            self.vres,
            self.reserved2) = t

    #endregion

    def __init__(self, r: BinaryReader, fileSize: int):
        self.header = r.readS(self.X_Header)
        if self.header.manufacturer != 0x0a: raise Exception('BAD MAGIC')
        elif self.header.encoding == 0: raise Exception('NO COMPRESSION')
        self.body = r.readToEnd()
        self.planes = self.header.numPlanes
        self.width = self.header.xmax - self.header.xmin + 1
        self.height = self.header.ymax - self.header.ymin + 1

    #region ITexture

    format: tuple = (TextureFormat.RGBA32, TexturePixel.Unknown)
    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0

    def getPalette(self) -> bytes:
        if self.header.bpp == 8 and self.body[-769] == 12: return self.body[len(self.body) - 768:]
        elif self.hHeader.bpp == 1: return self.header.palette[:48]
        else: raise Exception('Could not find 256 color palette.')

    @staticmethod
    def rle(body: bytearray, offset: int) -> bool: return (body[offset] >> 6) == 3

    @staticmethod
    def rleLength(body: bytearray, offset: int) -> int: return body[offset] & 63

    @staticmethod
    def setPixel(palette: bytearray, pixels: bytearray, pos: int, index: int) -> None:
        start = index * 3
        pixels[pos + 0] = palette[start]
        pixels[pos + 1] = palette[start + 1]
        pixels[pos + 2] = palette[start + 2]
        pixels[pos + 3] = 255 # alpha channel

    def create(self, platform: str, func: callable):
        # decodes 4bpp pixel data
        @staticmethod
        def decode4bpp():
            palette = self.getPalette()
            temp = bytearray(self.width * self.height)
            pixels = bytearray(self.width * self.height * 4)
            offset = 0; p = 0; pos = 0; length = 0; val = 0

            # Simple RLE decoding: if 2 msb == 1 then we have to mask out count and repeat following byte count times
            b = self.body
            for y in range(self.height):
                for p in range(self.planes):
                    # bpr holds the number of bytes needed to decode a row of plane: we keep on decoding until the buffer is full
                    pos = self.width * y
                    for _ in range(self.header.bpl):
                        if length == 0:
                             if self.rle(b, offset): length = self.rleLength(b, offset); val = b[offset + 1]; offset += 2
                             else: length = 1; val = b[offset]; offset += 1
                        length -= 1

                        # Since there may, or may not be blank data at the end of each scanline, we simply check we're not out of bounds
                        if (_ * 8) < self.width:
                            for i in range(8):
                                bit = (val >> (7 - i)) & 1
                                temp[pos + i] |= (bit << p) & 0xff
                                # we have all planes: we may set color using the palette
                                if p == self.planes - 1: self.setPixel(palette, pixels, (pos + i) * 4, temp[pos + i])
                            pos += 8
            return pixels

        # decodes 8bpp (depth = 8/24bit) data
        @staticmethod
        def decode8bpp():
            palette = self.getPalette() if self.planes == 1 else None
            pixels = bytearray(self.width * self.height * 4)
            offset = 0; p = 0; pos = 0; length = 0; val = 0

            # Simple RLE decoding: if 2 msb == 1 then we have to mask out count and repeat following byte count times
            b = self.body
            for y in range(self.height):
                for p in range(self.planes):
                    # bpr holds the number of bytes needed to decode a row of plane: we keep on decoding until the buffer is full
                    pos = 4 * self.width * y + p
                    for _ in range(self.header.bpl):
                        if length == 0:
                             if self.rle(b, offset): length = self.rleLength(b, offset); val = b[offset + 1]; offset += 2
                             else: length = 1; val = b[offset]; offset += 1
                        length -= 1

                        # Since there may, or may not be blank data at the end of each scanline, we simply check we're not out of bounds
                        if _ < self.width:
                            if self.planes == 3:
                                pixels[pos] = val & 0xff
                                if p == self.planes - 1: pixels[pos + 1] = 255 # add alpha channel
                            else: self.setPixel(palette, pixels, pos, val)
                            pos += 4
            return pixels

        match self.header.bpp:
            case 8: bytes = decode8bpp()
            case 1: bytes = decode4bpp()
            case _: raise Exception(f'Unknown bpp: {header.bpp}')
        return func(TextureAsBytes(bytes, self.format, None))

    #endregion

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Binary_Pcx', items = [
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}')
            ])
        ]

#endregion

#region Binary_Pal

# Binary_Pal
class Binary_Pal(IHaveMetaInfo):
    @staticmethod
    async def factory_3(r: BinaryReader, f: FileSource, s: Archive): return Binary_Pal(r, 3)
    @staticmethod
    async def factory_4(r: BinaryReader, f: FileSource, s: Archive): return Binary_Pal(r, 4)

    def __init__(self, r: BinaryReader, bpp: int):
        self.bpp = bpp
        if bpp == 3:
            self.records = []
            for _ in range(256):
                rgb = r.readBytes(3)
                self.records.append([rgb[0], rgb[1], rgb[2], 255])
        elif bpp == 4:
            self.records = [list(r.readUInt32().to_bytes(4, 'little')) for _ in range(256)]
        else: raise ValueError(str(bpp))

    def convertVgaPalette(self):
        if self.bpp == 3:
            for p in self.records:
                p[0] = ((p[0] << 2) | (p[0] >> 4)) & 0xFF
                p[1] = ((p[1] << 2) | (p[1] >> 4)) & 0xFF
                p[2] = ((p[2] << 2) | (p[2] >> 4)) & 0xFF
        return self

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'Pallet')),
        MetaInfo('Pallet', items = [
            MetaInfo(f'Records: {len(self.records)}')
            ])
        ]

#endregion

#region Binary_Plist

# Binary_Plist
class Binary_Plist(ArcBinaryT):
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        obj = PlistReader().readObject(r.f)
        source.files = [FileSource(path = str(k), fileSize = len(v), tag = v) for k, v in obj.items()]

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None):
        return BytesIO(file.tag)

#endregion

#region Binary_Raw

# Binary_Raw
class Binary_Raw(IHaveMetaInfo, ITexture):
    class Tag:
        def __init__(self, palette: str = None, width: int = 0, height: int = 0):
            self.palette = palette
            self.width = width
            self.height = height

    @staticmethod
    def factoryMethod(action: callable, palleteFunc: callable):
        async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Raw(r, s.game, f, action, palleteFunc)
        return factory

    def __init__(self, r: BinaryReader, game, source: FileSource, action: callable, palleteFunc: callable):
        self.body = None
        self.palette = None
        self.width = 0
        self.height = 0
        action(self, r, source)
        if self.body is None: self.body = r.readToEnd()
        if isinstance(source.tag, Binary_Raw.Tag):
            c = source.tag
            self.palette = c.palette
            self.width = c.width
            self.height = c.height
        self.paletteData = palleteFunc(game.id, self.palette).records

    format = (TextureFormat.RGBA32, TexturePixel.Unknown)
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0
    def create(self, platform: str, func: callable):
        pixels = bytearray()
        for s in self.body: pixels.extend(self.paletteData[s])
        return func(TextureAsBytes(bytes(pixels), self.format, None))

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Binary_Raw', items = [
            MetaInfo(f'Palette: {self.palette}'),
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}')
            ])
        ]

#endregion

#region Binary_Snd
    
# Binary_Snd
class Binary_Snd(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Snd(r, f.fileSize)

    def __init__(self, r: BinaryReader, fileSize: int):
        self.data = r.readBytes(fileSize)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'AudioPlayer', name = os.path.basename(file.path), value = self.data, tag = _pathExtension(file.path)))
        ]

#endregion

#region Binary_Tga

# Binary_Tga
class Binary_Tga(IHaveMetaInfo, ITexture):

    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Tga(r, f)

    #region Headers

    # Image pixel format
    # The pixel data are all in little-endian. E.g. a PIXEL_ARGB32 format image, a single pixel is stored in the memory in the order of BBBBBBBB GGGGGGGG RRRRRRRR AAAAAAAA.
    class PIXEL(Enum):
        BW8 = 0     # Single channel format represents grayscale, 8-bit integer.
        BW16 = 1    # Single channel format represents grayscale, 16-bit integer.
        RGB555 = 2  # A 16-bit pixel format. The topmost bit is assumed to an attribute bit, usually ignored. Because of little-endian, this format pixel is stored in the memory in the order of GGGBBBBB ARRRRRGG.
        RGB24 = 3   # RGB color format, 8-bit per channel.
        ARGB32 = 4  # RGB color with alpha format, 8-bit per channel.

    class TYPE(Enum):
        NO_DATA = 0
        COLOR_MAPPED = 1
        TRUE_COLOR = 2
        GRAYSCALE = 3
        RLE_COLOR_MAPPED = 9
        RLE_TRUE_COLOR = 10
        RLE_GRAYSCALE = 11

    # gets the bytes per pixel by pixel format
    @staticmethod
    def pixelFormatToPixelSize(format: PIXEL) -> int:
        match format:
            case Binary_Tga.PIXEL.BW8: return 1
            case Binary_Tga.PIXEL.BW16: return 2
            case Binary_Tga.PIXEL.RGB555: return 2
            case Binary_Tga.PIXEL.RGB24: return 3
            case Binary_Tga.PIXEL.ARGB32: return 4
            case _: raise Exception('UNSUPPORTED_PIXEL_FORMAT')

    # gets the mode by pixel format
    @staticmethod
    def pixelFormatToMode(format: PIXEL) -> str:
        match format:
            case Binary_Tga.PIXEL.BW8: return 'L'
            case Binary_Tga.PIXEL.BW16: return 'I'
            # case Binary_Tga.PIXEL.RGB555: return ''
            case Binary_Tga.PIXEL.RGB24: return 'RGB'
            case Binary_Tga.PIXEL.ARGB32: return 'RGBA'
            case _: raise Exception('UNSUPPORTED_PIXEL_FORMAT')

    # convert bits to integer bytes. E.g. 8 bits to 1 byte, 9 bits to 2 bytes.
    @staticmethod
    def bitsToBytes(bits: int) -> int: return ((bits - 1) // 8 + 1) & 0xFF

    class ColorMap:
        firstIndex: int = 0
        entryCount: int = 0
        bytesPerEntry: int = 0
        pixels: bytearray = 0

    class X_Header:
        _struct = ('<3b2Hb4H2b', 18)
        def __init__(self, t):
            (self.idLength,
            self.mapType,
            self.imageType,
            self.mapFirstEntry,
            self.mapLength,
            self.mapEntrySize,
            self.imageXOrigin,
            self.imageYOrigin,
            self.imageWidth,
            self.imageHeight,
            self.pixelDepth,
            self.imageDescriptor) = t
            # remap
            self.imageType = Binary_Tga.TYPE(self.imageType)

        @property
        def IS_SUPPORTED_IMAGE_TYPE(self) -> bool: return \
            self.imageType == Binary_Tga.TYPE.COLOR_MAPPED or \
            self.imageType == Binary_Tga.TYPE.TRUE_COLOR or \
            self.imageType == Binary_Tga.TYPE.GRAYSCALE or \
            self.imageType == Binary_Tga.TYPE.RLE_COLOR_MAPPED or \
            self.imageType == Binary_Tga.TYPE.RLE_TRUE_COLOR or \
            self.imageType == Binary_Tga.TYPE.RLE_GRAYSCALE
        @property
        def IS_COLOR_MAPPED(self) -> bool: return \
            self.imageType == Binary_Tga.TYPE.COLOR_MAPPED or \
            self.imageType == Binary_Tga.TYPE.RLE_COLOR_MAPPED
        @property
        def IS_TRUE_COLOR(self) -> bool: return \
            self.imageType == Binary_Tga.TYPE.TRUE_COLOR or \
            self.imageType == Binary_Tga.TYPE.RLE_TRUE_COLOR
        @property
        def IS_GRAYSCALE(self) -> bool: return \
            self.imageType == Binary_Tga.TYPE.GRAYSCALE or \
            self.imageType == Binary_Tga.TYPE.RLE_GRAYSCALE
        @property
        def IS_RLE(self) -> bool: return \
            self.imageType == Binary_Tga.TYPE.RLE_COLOR_MAPPED or \
            self.imageType == Binary_Tga.TYPE.RLE_TRUE_COLOR or \
            self.imageType == Binary_Tga.TYPE.RLE_GRAYSCALE

        def check(self) -> None:
            MAX_IMAGE_DIMENSIONS = 65535
            if self.mapType > 1: raise Exception('UNSUPPORTED_COLOR_MAP_TYPE')
            elif self.imageType == Binary_Tga.TYPE.NO_DATA: raise Exception('NO_DATA')
            elif not self.IS_SUPPORTED_IMAGE_TYPE: raise Exception('UNSUPPORTED_IMAGE_TYPE')
            elif self.imageWidth <= 0 or self.imageWidth > MAX_IMAGE_DIMENSIONS or self.imageHeight <= 0 or self.imageHeight > MAX_IMAGE_DIMENSIONS: raise Exception('INVALID_IMAGE_DIMENSIONS')

        def getColorMap(self, r: BinaryReader) -> object: #ColorMap
            mapSize = self.mapLength * Binary_Tga.bitsToBytes(self.mapEntrySize)
            s = Binary_Tga.ColorMap()
            if self.IS_COLOR_MAPPED:
                s.firstIndex = self.mapFirstEntry
                s.entryCount = self.mapLength
                s.bytesPerEntry = Binary_Tga.bitsToBytes(self.mapEntrySize)
                s.pixels = r.readBytes(mapSize)
            elif self.mapType == 1: r.skip(mapSize)  # The image is not color mapped at this time, but contains a color map. So skips the color map data block directly.
            return s

        def getPixelFormat(self) -> PIXEL:
            if self.IS_COLOR_MAPPED:
                if self.pixelDepth == 8:
                    match self.mapEntrySize:
                        case 15 | 16: return Binary_Tga.PIXEL.RGB555
                        case 24: return Binary_Tga.PIXEL.RGB24
                        case 32: return Binary_Tga.PIXEL.ARGB32
            elif self.IS_TRUE_COLOR:
                match self.pixelDepth:
                    case 16: return Binary_Tga.PIXEL.RGB555
                    case 24: return Binary_Tga.PIXEL.RGB24
                    case 32: return Binary_Tga.PIXEL.ARGB32
            elif self.IS_GRAYSCALE:
                match self.pixelDepth:
                    case 8: return Binary_Tga.PIXEL.BW8
                    case 16: return Binary_Tga.PIXEL.BW16
            else: raise Exception('UNSUPPORTED_PIXEL_FORMAT')

    #endregion

    def __init__(self, r: BinaryReader, f: FileSource):
        header = self.header = r.readS(self.X_Header)
        header.check()
        r.skip(header.idLength)
        self.map = header.getColorMap(r)
        self.width = header.imageWidth
        self.height = header.imageHeight
        self.body = BytesIO(r.readToEnd())
        self.pixelFormat = header.getPixelFormat()
        self.pixelSize = Binary_Tga.pixelFormatToPixelSize(self.pixelFormat)
        match self.pixelFormat:
            case Binary_Tga.PIXEL.BW8: raise Exception('Not Supported')
            case Binary_Tga.PIXEL.BW16: raise Exception('Not Supported')
            case Binary_Tga.PIXEL.RGB555: self.format = (TextureFormat.RGB565, TexturePixel.Unknown)
            case Binary_Tga.PIXEL.RGB24: self.format = (TextureFormat.RGB24, TexturePixel.Unknown),
            case Binary_Tga.PIXEL.ARGB32: self.format = (TextureFormat.RGB24, TexturePixel.Unknown),
            case _: raise Exception(f'Unknown {self.pixelFormat}')

    #region ITexture

    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0

    @staticmethod
    def pixelToMapIndex(pixelPtr: bytearray, offfset: int) -> int: return pixelPtr[offset]

    @staticmethod
    def getColorFromMap(dest: bytearray, offset: int, index: int, map: ColorMap) -> None:
        index -= map.firstIndex
        if index < 0 and index >= map.entryCount: raise Exception('COLOR_MAP_INDEX_FAILED')
        # Buffer.BlockCopy(map.pixels, map.bytesPerEntry * index, dest, offset, map.bytesPerEntry)

    def _lambdax(self) -> TextureAsBytes:
        # decodeRle
        def decodeRle(data: bytearray):
            isColorMapped = self.header.IS_COLOR_MAPPED
            pixelSize = self.pixelSize
            s = self.body; o = 0
            pixelCount = self.width * self.height

            isRunLengthPacket = False
            packetCount = 0
            pixelBuffer = bytearray(map.bytesPerEntry if isColorMapped else pixelSize); mv = memoryview(pixelBuffer)
            
            for _ in range(pixelCount, 0, -1):
                if packetCount == 0:
                    repetitionCountField = int.from_bytes(s.read(1), 'little', signed=False)
                    isRunLengthPacket = (repetitionCountField & 0x80) != 0
                    packetCount = (repetitionCountField & 0x7F) + 1
                    if isRunLengthPacket:
                        s.readinto(mv[0:pixelSize])
                        # in color mapped image, the pixel as the index value of the color map. The actual pixel value is found from the color map.
                        if isColorMapped: getColorFromMap(pixelBuffer, 0, pixelToMapIndex(pixelBuffer, o), map)
                if isRunLengthPacket: data[o:o+pixelSize] = pixelBuffer[0:pixelSize]
                else:
                    s.readinto(data[o:o+pixelSize])
                    # in color mapped image, the pixel as the index value of the color map. The actual pixel value is found from the color map.
                    if isColorMapped: getColorFromMap(data, o, pixelToMapIndex(data, o), map)
                packetCount -= 1
                o += pixelSize

        # decode
        def decode(data: bytearray):
            isColorMapped = self.header.IS_COLOR_MAPPED
            pixelSize = self.pixelSize
            s = self.body; o = 0
            pixelCount = self.width * self.height

            # in color mapped image, the pixel as the index value of the color map. The actual pixel value is found from the color map
            if isColorMapped:
                for _ in range(pixelCount, 0, -1):
                    s.readinto(data[o:o+pixelSize])
                    getColorFromMap(data, o, pixelToMapIndex(data, o), map)
                    o += map.bytesPerEntry
            else: s.readinto(data[:pixelCount*pixelSize])

        header = self.header
        bytes = bytearray(self.width * self.height * self.pixelSize); mv = memoryview(bytes)
        if header.IS_RLE: decodeRle(mv)
        else: decode(mv)
        self.map.pixels = None

        # flip the image if necessary, to keep the origin in upper left corner.
        flipH = (header.imageDescriptor & 0x10) != 0
        flipV = (header.imageDescriptor & 0x20) == 0
        if flipH: self.flipH(bytes)
        if flipV: self.flipV(bytes)
        
        return TextureAsBytes(bytes, self.format, None)
    def create(self, platform: str, func: callable): return func(_lambdax)

    # returns the pixel at coordinates (x,y) for reading or writing.
    # if the pixel coordinates are out of bounds (larger than width/height or small than 0), they will be clamped.
    # def getPixel(self, x: int, y: int) -> int:
    #     if x < 0: x = 0
    #     elif x >= self.width: x = self.width - 1
    #     if y < 0: y = 0
    #     elif y >= self.height: y = self.height - 1
    #     return (y * self.width + x) * self.pixelSize

    def flipH(self, data: bytearray) -> None:
        mode = Binary_Tga.pixelFormatToMode(self.pixelFormat)
        img = Image.frombuffer(mode, (self.width, self.height), data, 'raw')
        data[0:] = img.transpose(Image.FLIP_LEFT_RIGHT).tobytes('raw')
        # pixelSize = self.pixelSize
        # temp = bytearray(pixelSize)
        # flipNum = self.width // 2
        # for i in range(flipNum):
        #     for j in range(self.height):
        #         p1 = self.getPixel(i, j)
        #         p2 = self.getPixel(self.width - 1 - i, j)
        #         # swap two pixels
        #         # Buffer.BlockCopy(data, p1, temp, 0, pixelSize)
        #         # Buffer.BlockCopy(data, p2, data, p1, pixelSize)
        #         # Buffer.BlockCopy(temp, 0, data, p2, pixelSize)
        pass

    def flipV(self, data: bytearray) -> None:
        mode = Binary_Tga.pixelFormatToMode(self.pixelFormat)
        img = Image.frombuffer(mode, (self.width, self.height), data, 'raw')
        data[0:] = img.transpose(Image.FLIP_TOP_BOTTOM).tobytes('raw')
        # pixelSize = self.pixelSize
        # temp = bytearray(pixelSize)
        # flipNum = self.height // 2
        # for i in range(flipNum):
        #     for j in range(self.width):
        #         p1 = self.getPixel(j, i)
        #         p2 = self.getPixel(j, self.height - 1 - i)
        #         # swap two pixels
        #         # Buffer.BlockCopy(data, p1, temp, 0, pixelSize)
        #         # Buffer.BlockCopy(data, p2, data, p1, pixelSize)
        #         # Buffer.BlockCopy(temp, 0, data, p2, pixelSize)
        pass

    #endregion

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Binary_Tga', items = [
            MetaInfo(f'Format: {self.pixelFormat}'),
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}')
            ])
        ]

#endregion

#region Binary_Txt

# Binary_Txt
class Binary_Txt(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Txt(r, f.fileSize)

    def __init__(self, r: BinaryReader, fileSize: int):
        self.data = r.readBytes(fileSize).decode('utf8', 'ignore')

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self.data))
        ]

#endregion

#region Binary_Xga

# Binary_Xga
class Binary_Xga(IHaveMetaInfo, ITexture):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Xga(r, s.tag)

    def __init__(self, r: BinaryReader, tag: object):
        self.type = 1
        self.body = r.readToEnd()

    format = (TextureFormat.RGBA32, TexturePixel.Unknown)
    width: int = 64
    height: int = 64
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0
    def create(self, platform: str, func: callable):
        def decode1(): return None
        if self.type == 1: data = decode1()
        else: raise Exception(f'Unsupported type: {self.type}')
        return func(TextureAsBytes(data, self.format, None))

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Binary_Xga', items = [
            MetaInfo(f'Type: {self.type}'),
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}')
            ])
        ]

#endregion

#region Binary_Zip

# Binary_Zip
class Binary_Zip(ArcBinaryT):
    def __init__(self, key: str | bytes = None):
        self.key = key

    # read
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        source.useReader = False
        arc = source.tag = ZipFile(r.f)
        # A null key is ZipArchive: keep a name that does not end in '/'.
        # Any other key is SharpZipLib: skip '/' and '\', and flag an encrypted entry.
        useSystem = self.key is None
        if not useSystem:
            match self.key:
                case str() as s: arc.setpassword(s.encode('utf-8'))
                case bytes() | bytearray(): pass
                case _: pass
        # zipfile rewrites '\' to '/' in filename on Windows. C# tests the raw name, which is orig_filename.
        source.files = [FileSource(
            path = s.orig_filename.replace('\\', '/'),
            flags = 0 if useSystem else (1 if s.flag_bits & 1 else 0),
            packedSize = s.compress_size,
            fileSize = s.file_size,
            tag = s) for s in arc.infolist() if not s.orig_filename.endswith('/' if useSystem else ('/', '\\'))]

    # readData
    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        arc: ZipFile = source.tag
        # zipfile cannot set SharpZipLib's ZipFile.Key, so an encrypted entry fails closed.
        if isinstance(self.key, (bytes, bytearray)) and file.flags: raise Exception('NotSupportedException: ZipFile.Key')
        return BytesIO(arc.read(file.tag))

#endregion

#region Binary_TestTri

class Binary_TestTri(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_TestTri(r)

    def __init__(self, r: BinaryReader): pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'TestTri', name = os.path.basename(file.path), value = self))
        ]

#endregion

#region Binary_Engine

class Binary_Engine(IHaveMetaInfo, ICellDatabase):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Engine(r, s)

    def __init__(self, r: BinaryReader, s: Archive):
        # Encoding.ASCII: a byte above 127 is '?'.
        values = ''.join(chr(b) if b < 128 else '?' for b in r.readToEnd()).split(':')
        ar = values[0]
        qa = values[1]
        ci = [int(x) for x in values[2].split(',')]
        interior = values[3]
        match interior.lower():
            case 'true': cb = True
            case 'false': cb = False
            case _: raise Exception(f'FormatException: {interior}')
        pp = [float(x) for x in values[4].split(',')]
        pr = [float(x) for x in values[5].split(',')]
        archive = s.getArchive(ar)
        queryArchive = s.getArchive(qa)
        if archive is not None and not isinstance(archive, BinaryArchive): raise Exception('InvalidCastException: BinaryArchive')
        if queryArchive is None: raise Exception('NullReferenceException: QueryArchive')
        if not isinstance(queryArchive, BinaryArchive): raise Exception('InvalidCastException: BinaryArchive')
        arcBinary = queryArchive.arcBinary
        if arcBinary is None: raise Exception('NullReferenceException: ArcBinary')
        if not isinstance(arcBinary, CellManager.IQueryFunc): raise Exception('InvalidCastException: IQueryFunc')
        self.archive = archive
        self.queryArchive = queryArchive
        self.query = arcBinary.getQuery()
        self.cellId = Int3(ci[0], ci[1], ci[2])
        self.cellInterior = cb
        self.playerPosition = array([pp[0], pp[1], pp[2]], dtype=float)
        # pr.Length <= 3 is Quaternion.Identity. A longer list is (x, y, z, w).
        self.playerRotation = quaternion(1, 0, 0, 0) if len(pr) <= 3 else quaternion(pr[3], pr[0], pr[1], pr[2])

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Engine', name = os.path.basename(file.path), value = self)),
        MetaInfo('Binary_Engine', items = [
            MetaInfo(f'Archive: {self.archive}'),
            MetaInfo(f'Query: {self.query}'),
            MetaInfo(f'CellId: {self.cellId}'),
            MetaInfo(f'PlayerPosition: {self.playerPosition}')
            ])
        ]

#endregion