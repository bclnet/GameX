from __future__ import annotations
import os
from enum import Enum
from io import BytesIO
from openx.core import unsafe, BinaryReader
from openx.core.poly.log import info as logInfo
from openx.gfx import Raster, ITextureFrames, TextureAsBytes, TextureFlags, TextureFormat, TexturePixel
from gamex import ArcBinaryT, Archive, FileSource, MetaInfo, MetaContent, IHaveMetaInfo
from gamex.families.Bullfrog.formats.utils import Rnc

def _bind(obj, names, values) -> None:
    for name, value in zip(names, values, strict=True): setattr(obj, name, value)

def _negShort(value: int) -> int:
    value &= 0xFFFF
    if value >= 0x8000: value -= 0x10000
    return -value

def _i32(n: int) -> int:
    n &= 0xFFFFFFFF
    return n - 0x100000000 if n & 0x80000000 else n

# C# UnsafeX.Atoi. The running total is kept negative and flipped at the end.
def _atoi(data: str) -> int:
    n = 0
    neg = False
    i = 0
    end = len(data)
    while i != end and data[i].isspace(): i += 1
    if i != end:
        if data[i] == '-': neg = True; i += 1
        elif data[i] == '+': i += 1
        while i != end and data[i].isdigit():
            n = _i32(10 * n - (ord(data[i]) - 48))
            i += 1
    # unchecked int negation, so -int.MinValue stays int.MinValue
    return n if neg else _i32(-n)

def _changeExtension(path: str, ext: str) -> str: return os.path.splitext(path)[0] + ext

def _bit(value: int, index: int) -> int: return (value >> index) & 1

# C# Binary_Raw.Tag is not in the Python Uncore port. Same fields, camelCase.
class RawTag:
    def __init__(self, width: int, height: int, palette: str = None):
        self.width = width
        self.height = height
        self.palette = palette

#region Binary_Bullfrog

# Binary_Bullfrog
class Binary_Bullfrog(ArcBinaryT):
    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> tuple:
        # C# returns default. The extension switch above it is commented out.
        return (None, None)

    @staticmethod
    def binary_RawFunc(s, r: BinaryReader, f: FileSource) -> None:
        s.body = Rnc.read(r) if r.peek(lambda x: x.readUInt32()) == Rnc.RNC_MAGIC else r.readToEnd()
        if f.tag == None:
            s.palette = '' if f.path.endswith('.bmp') else f.path[:-4]
            match len(s.body):
                case 1024: s.width = 32; s.height = 32
                case 64000: s.width = 320; s.height = 200
                case 307200: s.width = 640; s.height = 480
                case 1228800: s.width = 640; s.height = 1920
                case _: raise Exception(f'{len(s.body)}')

    #region Headers

    TEXTURE_BLOCKSA = 544
    TEXTURE_BLOCKSB = 544

    class SBK_Header:
        _struct = ('<14sI', 18)
        _names = ('unknown1', 'unknown2')
        def __init__(self, t): _bind(self, self._names, t)

    class SBK_Entry:
        _struct = ('<4I', 16)
        _names = ('entryOffset', 'bankOffset', 'size', 'unknown')
        def __init__(self, t): _bind(self, self._names, t)

    class SBK_Sample:
        _struct = ('<18sQIBB', 32)
        _names = ('path', 'offset', 'dataSize', 'sfxid', 'unknown')
        def __init__(self, t): _bind(self, self._names, t)

    class CRE_Sprite:
        _struct = ('<I8B2h', 16)
        _names = ('dataOffset', 'sWidth', 'sHeight', 'frameWidth', 'frameHeight', 'rotable', 'framesCount', 'frameOffsW', 'frameOffsH', 'offsetX', 'offsetY')
        def __init__(self, t): _bind(self, self._names, t)

    #endregion

    data = None

    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        # C# Read leaves the creature/gui/sound/texture parsers commented out.
        source.files = []

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        data = self.data
        start = int(file.offset)
        size = int(file.fileSize)
        if data == None or start < 0 or size < 0 or start + size > len(data): raise Exception('ArgumentOutOfRange')
        return BytesIO(bytes(data[start:start + size]))

    @staticmethod
    def parseCreature(r: BinaryReader, files: list) -> None:
        numSprites = int(r.length // Binary_Bullfrog.CRE_Sprite._struct[1])
        sprites = r.readSArray(Binary_Bullfrog.CRE_Sprite, numSprites)
        lastI = numSprites - 1
        for i in range(numSprites):
            s = sprites[i]
            files.append(FileSource(
                path = f'{i:03d}.tex',
                offset = s.dataOffset,
                fileSize = (sprites[i + 1].dataOffset if i != lastI else r.length) - s.dataOffset,
                tag = RawTag(width = s.sWidth, height = s.sHeight)))

    @staticmethod
    def parseGui(r: BinaryReader, files: list) -> None:
        textureBlockSize = 100 * 100
        i = 1
        o = 0
        while i < 1:
            files.append(FileSource(
                path = f'gui/{i:03d}.tex',
                offset = o,
                fileSize = textureBlockSize,
                tag = RawTag(width = 32, height = 32)))
            i += 1
            o += textureBlockSize

    @staticmethod
    def parseTexure(files: list, count: int) -> None:
        textureBlockSize = 32 * 32
        i = 1
        o = 0
        while i < count:
            files.append(FileSource(
                path = f'texs/{i:03d}.tex',
                offset = o,
                fileSize = textureBlockSize,
                tag = RawTag(width = 32, height = 32)))
            i += 1
            o += textureBlockSize

    @staticmethod
    def parseSound(r: BinaryReader, files: list, bankId: int) -> None:
        r.end(-4)
        offset = r.readUInt32()
        r.seek(offset)
        r.readS(Binary_Bullfrog.SBK_Header)
        banks = r.readSArray(Binary_Bullfrog.SBK_Entry, 9)
        bank = banks[bankId]
        if bank.entryOffset == 0 or bank.size == 0: raise Exception('BAD BANK')
        bankOffset = bank.bankOffset
        r.seek(bank.entryOffset)
        numSamples = int(bank.size // Binary_Bullfrog.SBK_Sample._struct[1])
        for s in r.readSArray(Binary_Bullfrog.SBK_Sample, numSamples):
            path = unsafe.fixedAString(s.path, 18)
            if path == None: path = 'DEFAULT.WAV'
            files.append(FileSource(
                path = path,
                offset = bankOffset + s.offset,
                fileSize = s.dataSize))

#endregion

#region Binary_Fli

# Binary_Fli
class Binary_Fli(IHaveMetaInfo, ITextureFrames):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Fli(r, f)

    #region Headers

    MAGIC = 0xAF12

    class X_Header:
        _struct = ('<I4H', 12)
        _names = ('size', 'type', 'frames', 'width', 'height')
        def __init__(self, t): _bind(self, self._names, t)

    class ChunkType(Enum):
        COLOR_256 = 0x4
        DELTA_FLC = 0x7
        BYTE_RUN = 0xF
        FRAME = 0xF1FA
        @classmethod
        def of(cls, value: int):
            try: return cls(value)
            except ValueError: return value

    class X_ChunkHeader:
        # bool Valid marshals as 4 bytes, so sizeof is 10, not the 6-byte FLI header.
        _struct = ('<IHI', 10)
        def __init__(self, t):
            self.size, type_, validRaw = t
            self.type = Binary_Fli.ChunkType.of(type_)
            self.valid = validRaw != 0
        @staticmethod
        def run(s):
            s.valid = s.type == Binary_Fli.ChunkType.COLOR_256 or s.type == Binary_Fli.ChunkType.DELTA_FLC or s.type == Binary_Fli.ChunkType.BYTE_RUN or s.type == Binary_Fli.ChunkType.FRAME
            return s

    class X_FrameHeader:
        _struct = ('<5H', 10)
        _names = ('numChunks', 'delay', 'reserved', 'widthOverride', 'heightOverride')
        def __init__(self, t): _bind(self, self._names, t)

    class OpCode(Enum):
        PACKETCOUNT = 0
        UNDEFINED = 1
        LASTPIXEL = 2
        LINESKIPCOUNT = 3

    #endregion

    def __init__(self, r: BinaryReader, f: FileSource):
        from gamex.resources.Bullfrog.S import getEvents
        self.events = getEvents(f'{os.path.splitext(os.path.basename(f.path))[0].lower()}.evt')
        header = r.readS(self.X_Header)
        if header.type != self.MAGIC: raise Exception('BAD MAGIC')
        self.width = header.width
        self.height = header.height
        self.frames = self.numFrames = header.frames
        self.r = r
        self.fps = 20 if os.path.basename(f.path).lower().startswith('mscren') else 15
        self.pixels = bytearray(self.width * self.height)
        self.palette = bytearray(256 * 3)
        self.bytes = bytearray(self.width * self.height * 3)

    def dispose(self) -> None: self.r.dispose()

    #region ITexture

    format: tuple = (TextureFormat.RGB24, TexturePixel.Unknown)
    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 0
    texFlags: TextureFlags = 0
    fps: int = 1
    def create(self, platform: str, func: callable): return func(TextureAsBytes(self.bytes, self.format, None))

    def hasFrames(self) -> bool: return self.numFrames > 0

    def nextFrame(self) -> bool:
        r = self.r
        header = self.X_ChunkHeader.run(r.readS(self.X_ChunkHeader))
        while True:
            nextPosition = r.tell() + (header.size - 6)
            match header.type:
                case self.ChunkType.COLOR_256: self.setPalette(r)
                case self.ChunkType.DELTA_FLC: self.decodeDeltaFLC(r)
                case self.ChunkType.BYTE_RUN: self.decodeByteRun(r)
                case self.ChunkType.FRAME:
                    r.readS(self.X_FrameHeader)
                    self.numFrames -= 1
                case _:
                    logInfo(f'Unknown Type: {header.type}')
                    r.skip(header.size)
            if header.type != self.ChunkType.FRAME and r.tell() != nextPosition: r.seek(nextPosition)
            header = r.readS(self.X_ChunkHeader)
            if not header.valid or header.type == self.ChunkType.FRAME: break
        Raster.blitByPalette(self.bytes, 3, self.pixels, self.palette, 3)
        if header.type == self.ChunkType.FRAME: r.skip(-self.X_ChunkHeader._struct[1])
        return header.valid

    def setPalette(self, r: BinaryReader) -> None:
        palette = self.palette
        numPackets = r.readUInt16()
        if r.readUInt16() == 0:
            data = r.readBytes(256 * 3)
            for i in range(0, len(data), 3):
                palette[i + 0] = ((data[i + 0] << 2) | (data[i + 0] & 3)) & 0xFF
                palette[i + 1] = ((data[i + 1] << 2) | (data[i + 1] & 3)) & 0xFF
                palette[i + 2] = ((data[i + 2] << 2) | (data[i + 2] & 3)) & 0xFF
            return
        r.skip(-2)
        palPos = 0
        while numPackets != 0:
            numPackets -= 1
            palPos += r.readByte() * 3
            change = r.readByte()
            data = r.readBytes(change * 3)
            for i in range(0, len(data), 3):
                palette[palPos + i + 0] = ((data[i + 0] << 2) | (data[i + 0] & 3)) & 0xFF
                palette[palPos + i + 1] = ((data[i + 1] << 2) | (data[i + 1] & 3)) & 0xFF
                palette[palPos + i + 2] = ((data[i + 2] << 2) | (data[i + 2] & 3)) & 0xFF
            palPos += change

    def decodeDeltaFLC(self, r: BinaryReader) -> None:
        linesInChunk = r.readUInt16()
        curLine = 0
        numPackets = 0
        while linesInChunk > 0:
            linesInChunk -= 1
            while True:
                value = r.readUInt16()
                opcode = self.OpCode((value >> 14) & 3)
                match opcode:
                    case self.OpCode.PACKETCOUNT: numPackets = value
                    case self.OpCode.UNDEFINED: pass
                    case self.OpCode.LASTPIXEL: self.pixels[(curLine * self.width) + (self.width - 1)] = value & 0xFF
                    case self.OpCode.LINESKIPCOUNT: curLine += _negShort(value)
                if opcode == self.OpCode.PACKETCOUNT: break
            value = 0
            while numPackets > 0:
                numPackets -= 1
                value += r.readByte()
                pos = (curLine * self.width) + value
                count = r.readSByte()
                if count > 0:
                    size = count << 1
                    self.pixels[pos:pos + size] = r.readBytes(size)
                    value += size
                elif count < 0:
                    count = -count
                    size = count << 1
                    data = r.readUInt16()
                    pair = bytes((data & 0xFF, (data >> 8) & 0xFF))
                    for i in range(count): self.pixels[pos + (i << 1):pos + (i << 1) + 2] = pair
                    value += size
                else: return
            curLine += 1

    def decodeByteRun(self, r: BinaryReader) -> None:
        pixels = self.pixels
        ptr = 0
        end = self.width * self.height
        while ptr < end:
            numChunks = r.readByte()
            while numChunks != 0:
                numChunks -= 1
                count = r.readSByte()
                if count > 0:
                    pixels[ptr:ptr + count] = bytes([r.readByte()]) * count
                    ptr += count
                else:
                    count = -count
                    pixels[ptr:ptr + count] = r.readBytes(count)
                    ptr += count

    #endregion

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'VideoTexture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Video', items = [
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}'),
            MetaInfo(f'Frames: {self.frames}')
            ])
        ]

#endregion

#region Binary_Populus

# Binary_Populus
class Binary_Populus(ArcBinaryT):
    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> tuple:
        match os.path.splitext(source.path)[1].lower():
            case '.pal':
                from gamex.families.Uncore.formats.binary import Binary_Pal
                return (0, Binary_Pal.factory_3)
            case '.wav':
                from gamex.families.Uncore.formats.binary import Binary_Snd
                return (0, Binary_Snd.factory)
            case '.raw':
                from gamex.families.Uncore.formats.binary import Binary_Raw
                from gamex.families.Bullfrog.games.P2.database import Database as P2_Database
                def palette(id, value, game=game):
                    match id:
                        case 'P2': return P2_Database.getPalette(value, 'DATA/MQAZ')
                        case _: raise Exception(game.id)
                return (0, Binary_Raw.factoryMethod(Binary_Populus.binary_RawFunc, palette))
            case _: return (0, None)

    @staticmethod
    def binary_RawFunc(s, r: BinaryReader, f: FileSource) -> None:
        if f.tag == None:
            s.body = r.readToEnd()
            s.palette = f.path[:-4]
            match len(s.body):
                case 64000: s.width = 320; s.height = 200
                case _: raise Exception(f'{len(s.body)}')

    #region Headers

    MAGIC_SPR = 0x42465350

    class SPR_Record:
        _struct = ('<2HI', 8)
        _names = ('width', 'height', 'offset')
        def __init__(self, t): _bind(self, self._names, t)

    class DAT_Sprite:
        # C# format string says "<2bI" but the marshalled layout is 6 bytes: byte, byte, ushort, ushort.
        _struct = ('<BB2H', 6)
        _names = ('width', 'height', 'unknown', 'offset')
        def __init__(self, t): _bind(self, self._names, t)

    #endregion

    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        source.files = files = []
        tabPath = f'{source.binPath[:-4]}.TAB'
        fileName = os.path.basename(source.binPath)
        match source.game.id:
            case 'P2':
                if fileName.startswith('EMSCBLK'): source.readerT(lambda s: self.parseSprite(s, files), tabPath)
                elif fileName.startswith('EMSFACES') or fileName.startswith('MEMSFACE'): source.readerT(lambda s: self.parseSprite(s, files), tabPath)
                elif fileName.startswith('EMSICON') or fileName.startswith('MEMICON'): source.readerT(lambda s: self.parseSprite(s, files), tabPath)
                elif fileName.startswith('EMSSPR') or fileName.startswith('MEMSPR'): source.readerT(lambda s: self.parseSprite(s, files), tabPath)
                elif fileName.startswith('HPOINTER') or fileName.startswith('MPOINTER'): source.readerT(lambda s: self.parseSprite(s, files), tabPath)
                elif fileName.startswith('MBLOCK'): source.readerT(lambda s: self.parseSprite(s, files), tabPath)
                elif fileName.startswith('LAND'): self.parseOther(files, 10)
                return
            case 'P3':
                magic = r.readUInt32()
                match magic:
                    case self.MAGIC_SPR:
                        count = r.readUInt32()
                        i = 0
                        l = None
                        lastOffset = r.tell()
                        for s in r.readSArray(self.SPR_Record, int(count)):
                            n = FileSource(
                                path = f'sprs/spr{i}.spr',
                                offset = s.offset,
                                tag = (s.width, s.height))
                            i += 1
                            files.append(n)
                            if l != None:
                                lastOffset = l.offset
                                l.fileSize = n.offset - lastOffset
                            l = n
                        l.fileSize = r.length - lastOffset
                        return
                    case _: raise Exception('BAD MAGIC')
            case _: return

    @staticmethod
    def parseSprite(r: BinaryReader, files: list) -> None:
        r.skip(4)
        # C#: Length - 4 / sizeof(DAT_Sprite). Integer division binds first, sizeof is 6, so this is Length - 0.
        numSprites = int(r.length - 4 // Binary_Populus.DAT_Sprite._struct[1])
        sprites = r.readSArray(Binary_Populus.DAT_Sprite, numSprites)
        lastI = numSprites - 1
        for i in range(numSprites):
            s = sprites[i]
            files.append(FileSource(
                path = f'{i:03d}.raw',
                offset = s.offset,
                fileSize = (sprites[i + 1].offset if i != lastI else r.length) - s.offset,
                tag = RawTag(width = s.width, height = s.height)))

    @staticmethod
    def parseOther(files: list, count: int) -> None:
        textureBlockSize = 32 * 32
        i = 1
        o = 0
        while i < count:
            files.append(FileSource(
                path = f'sprs/{i:03d}.raw',
                offset = o,
                fileSize = textureBlockSize,
                tag = RawTag(width = 32, height = 32)))
            i += 1
            o += textureBlockSize

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        r.seek(file.offset)
        return BytesIO(r.readBytes(int(file.fileSize)))

#endregion

#region Binary_Syndicate

S_FLIFILES = ['INTRO.DAT', 'MBRIEF.DAT', 'MBRIEOUT.DAT', 'MCONFOUT.DAT', 'MCONFUP.DAT', 'MDEBRIEF.DAT', 'MDEOUT.DAT', 'MENDLOSE.DAT', 'MENDWIN.DAT', 'MGAMEWIN.DAT', 'MLOSA.DAT', 'MLOSAOUT.DAT', 'MLOSEGAM.DAT', 'MMAP.DAT', 'MMAPOUT.DAT', 'MOPTION.DAT', 'MOPTOUT.DAT', 'MRESOUT.DAT', 'MRESRCH.DAT', 'MSCRENUP.DAT', 'MSELECT.DAT', 'MSELOUT.DAT', 'MTITLE.DAT', 'MMULTI.DAT', 'MMULTOUT.DAT']

# Nested class bodies cannot see Binary_SyndicateX's class scope.
_SYNDICATE_OBJECT_NAMES = ('offsetNext', 'offsetPrev', 'tileX', 'tileY', 'tileZ', 'location', 'state', 'unkn3', 'indexBaseAnim', 'indexCurrentFrame', 'indexCurrentAnim', 'health', 'offsetLastEnemy', 'type', 'status_SubType', 'orientation', 'unkn4')

# Binary_Syndicate
class Binary_Syndicate(ArcBinaryT):
    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> tuple:
        name = os.path.basename(source.path).upper()
        if name in S_FLIFILES: return (0, Binary_Fli.factory)
        if name == 'HFNT01.DAT': return (0, Binary_SyndicateX.factory_Font)
        if name.startswith('GAME') and name.endswith('.DAT'): return (0, Binary_SyndicateX.factory_Game)
        if name == 'COL01.DAT': return (0, Binary_SyndicateX.factory_MapColumn)
        if name.startswith('MAP') and name.endswith('.DAT'): return (0, Binary_SyndicateX.factory_MapData)
        if name == 'HBLK01.DAT': return (0, Binary_SyndicateX.factory_MapTile)
        if name.startswith('MISS') and name.endswith('.DAT'): return (0, Binary_SyndicateX.factory_Mission)
        if name == 'INTRO.XMI' or name == 'SYNGAME.XMI':
            from gamex.families.Uncore.formats.binary import Binary_Iif
            return (0, Binary_Iif.factory)
        if (name.startswith('HPAL') and name.endswith('.DAT')) or name == 'HPALETTE.DAT' or name == 'MSELECT.PAL': return (0, Binary_SyndicateX.factory_Palette)
        if name == 'MLOGOS.DAT' or name == 'MMAPBLK.DAT' or name == 'MMINLOGO.PAL': return (0, Binary_SyndicateX.factory_Raw)
        if name == 'HREQ.DAT': return (0, Binary_SyndicateX.factory_Req)
        if (name.startswith('ISNDS-') and name.endswith('.DAT')) or (name.startswith('SOUND-') and name.endswith('.DAT')): return (0, Binary_SyndicateX.factory_SoundData)
        if (name.startswith('ISNDS-') and name.endswith('.TAB')) or (name.startswith('SOUND-') and name.endswith('.TAB')): return (0, Binary_SyndicateX.factory_SoundTab)
        if name == 'HSTA-0.ANI': return (0, Binary_SyndicateX.factory_SpriteAnim)
        if name == 'HFRA-0.ANI': return (0, Binary_SyndicateX.factory_SpriteFrame)
        if name == 'HELE-0.ANI': return (0, Binary_SyndicateX.factory_SpriteElement)
        if name == 'HPOINTER.TAB' or name == 'HSPR-0.TAB' or name == 'MFNT-0.TAB' or name == 'MSPR-0.TAB': return (0, Binary_SyndicateX.factory_SpriteTab)
        if name == 'HPOINTER.DAT' or name == 'HSPR-0.DAT' or name == 'MFNT-0.DAT' or name == 'MSPR-0.DAT': return (0, Binary_SyndicateX.factory_SpriteData)
        raise Exception(name)

    # C# does not override Read/ReadData, so ArcBinary throws NotSupportedException.
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None: raise Exception('NotSupported')
    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO: raise Exception('NotSupported')

#endregion

#region Binary_SyndicateX

# Binary_SyndicateX
class Binary_SyndicateX(IHaveMetaInfo):
    class Kind(Enum):
        Font = 0
        Game = 1
        MapColumn = 2
        MapData = 3
        MapTile = 4
        Mission = 5
        Palette = 6
        Raw = 7
        Req = 8
        SoundData = 9
        SoundTab = 10
        SpriteAnim = 11
        SpriteFrame = 12
        SpriteElement = 13
        SpriteTab = 14
        SpriteData = 15

    @staticmethod
    async def factory_Font(r: BinaryReader, f: FileSource, s: Archive): return Binary_SyndicateX(r, f, s, Binary_SyndicateX.Kind.Font)
    @staticmethod
    async def factory_Game(r: BinaryReader, f: FileSource, s: Archive): return Binary_SyndicateX(r, f, s, Binary_SyndicateX.Kind.Game)
    @staticmethod
    async def factory_MapColumn(r: BinaryReader, f: FileSource, s: Archive): return Binary_SyndicateX(r, f, s, Binary_SyndicateX.Kind.MapColumn)
    @staticmethod
    async def factory_MapData(r: BinaryReader, f: FileSource, s: Archive): return Binary_SyndicateX(r, f, s, Binary_SyndicateX.Kind.MapData)
    @staticmethod
    async def factory_MapTile(r: BinaryReader, f: FileSource, s: Archive): return Binary_SyndicateX(r, f, s, Binary_SyndicateX.Kind.MapTile)
    @staticmethod
    async def factory_Mission(r: BinaryReader, f: FileSource, s: Archive): return Binary_SyndicateX(r, f, s, Binary_SyndicateX.Kind.Mission)
    @staticmethod
    async def factory_Palette(r: BinaryReader, f: FileSource, s: Archive): return Binary_SyndicateX(r, f, s, Binary_SyndicateX.Kind.Palette)
    @staticmethod
    async def factory_Raw(r: BinaryReader, f: FileSource, s: Archive): return Binary_SyndicateX(r, f, s, Binary_SyndicateX.Kind.Raw)
    @staticmethod
    async def factory_Req(r: BinaryReader, f: FileSource, s: Archive): return Binary_SyndicateX(r, f, s, Binary_SyndicateX.Kind.Req)
    @staticmethod
    async def factory_SoundData(r: BinaryReader, f: FileSource, s: Archive): return Binary_SyndicateX(r, f, s, Binary_SyndicateX.Kind.SoundData)
    @staticmethod
    async def factory_SoundTab(r: BinaryReader, f: FileSource, s: Archive): return Binary_SyndicateX(r, f, s, Binary_SyndicateX.Kind.SoundTab)
    @staticmethod
    async def factory_SpriteAnim(r: BinaryReader, f: FileSource, s: Archive): return Binary_SyndicateX(r, f, s, Binary_SyndicateX.Kind.SpriteAnim)
    @staticmethod
    async def factory_SpriteFrame(r: BinaryReader, f: FileSource, s: Archive): return Binary_SyndicateX(r, f, s, Binary_SyndicateX.Kind.SpriteFrame)
    @staticmethod
    async def factory_SpriteElement(r: BinaryReader, f: FileSource, s: Archive): return Binary_SyndicateX(r, f, s, Binary_SyndicateX.Kind.SpriteElement)
    @staticmethod
    async def factory_SpriteTab(r: BinaryReader, f: FileSource, s: Archive): return Binary_SyndicateX(r, f, s, Binary_SyndicateX.Kind.SpriteTab)
    @staticmethod
    async def factory_SpriteData(r: BinaryReader, f: FileSource, s: Archive): return Binary_SyndicateX(r, f, s, Binary_SyndicateX.Kind.SpriteData)

    #region Headers

    class X_Palette:
        _struct = ('<3B', 3)
        _names = ('r', 'g', 'b')
        def __init__(self, t): _bind(self, self._names, t)

    class X_Font:
        _struct = ('<H3B', 5)
        _names = ('offset', 'width', 'height', 'lineOffset')
        def __init__(self, t): _bind(self, self._names, t)

    class X_MapData:
        _struct = ('<3I', 12)
        _names = ('maxX', 'maxY', 'maxZ')
        def __init__(self, t): _bind(self, self._names, t)

    class X_SoundTab:
        _struct = ('<I28s', 32)
        _names = ('size', 'unknown')
        def __init__(self, t): _bind(self, self._names, t)
        @staticmethod
        def patch(i: int, sample: bytearray) -> None:
            if i == 13: sample[0x1e] = 0x9c
            elif i == 24: sample[0x1e] = 0x9c
            elif i == 25: sample[0x1e] = 0x38

    class X_MapTile:
        _struct = ('<6I', 24)
        _names = ('offsets0', 'offsets1', 'offsets2', 'offsets3', 'offsets4', 'offsets5')
        def __init__(self, t): _bind(self, self._names, t)

    class X_SpriteTab:
        _struct = ('<IBB', 6)
        _names = ('offset', 'width', 'height')
        def __init__(self, t): _bind(self, self._names, t)
        def stride(self) -> int: return 0 if self.width <= 0 else ((self.width // 8) + 1) * 8 if (self.width % 8) != 0 else self.width

    class X_SpriteElement:
        _struct = ('<5H', 10)
        _names = ('sprite', 'offsetX', 'offsetY', 'flipped', 'nextElement')
        def __init__(self, t): _bind(self, self._names, t)

    class X_SpriteFrame:
        _struct = ('<HBB2H', 8)
        _names = ('firstElement', 'width', 'height', 'flags', 'nextElement')
        def __init__(self, t): _bind(self, self._names, t)

    class X_GameHeader:
        _struct = ('<6s16384HH', 32776)
        def __init__(self, t):
            self.header = t[0]
            self.offsets = t[1:16385]
            self.offsetRef = t[16385]

    class ObjectLocation(Enum):
        OnMap = 0x04
        NotOnMap = 0x05
        OnMap2 = 0x06
        OnMap3 = 0x07
        AboveWalkSurf = 0x0C
        NotVisible = 0x0D

    class ObjectState(Enum):
        Ped_Standing = 0x0
        Ped_Walking = 0x10
        Ped_Dead = 0x11

    class ObjectType(Enum):
        Ped = 0x01
        Vehicle = 0x02
        Weapon = 0x04
        Object = 0x05

    class X_GameObject:
        _struct = ('<5HBB4HhHBBBB', 28)
        _names = ('offsetNext', 'offsetPrev', 'tileX', 'tileY', 'tileZ', 'location', 'state', 'unkn3', 'indexBaseAnim', 'indexCurrentFrame', 'indexCurrentAnim', 'health', 'offsetLastEnemy', 'type', 'status_SubType', 'orientation', 'unkn4')
        def __init__(self, t): _bind(self, self._names, t)

    class X_GamePerson:
        _struct = ('<5HBB4HhHBBBBB3s7H3H6s2H6sH13B9s', 92)
        _names = _SYNDICATE_OBJECT_NAMES + ('typePed', 'unkn5', 'offsetOfPersuader', 'unkn6', 'offsetOfVehicle', 'offsetScenarioCurr', 'offsetScenarioStart', 'unkn7', 'offsetOfVehicle2', 'gotoMapPosX', 'gotoMapPosY', 'gotoMapPosZ', 'unkn8', 'offsetEquipment', 'modsInfo', 'unkn9', 'offsetCurWeapon', 'unkn10', 'adrena_amount', 'adrena_dependency', 'adrena_effect', 'unkn11', 'inteliAmount', 'inteliDependency', 'inteliEffect', 'unkn12', 'percepAmount', 'percepDependency', 'percepEffect', 'unkn13', 'unkn14')
        def __init__(self, t): _bind(self, self._names, t)

    class X_GameVehicle:
        _struct = ('<5HBB4HhHBBBBB11sBB', 42)
        _names = _SYNDICATE_OBJECT_NAMES + ('offsetOfDriver', 'unkn5', 'speed', 'unkn6')
        def __init__(self, t): _bind(self, self._names, t)

    class X_GameStatic:
        _struct = ('<5HBB4HhHBBBBH', 30)
        _names = _SYNDICATE_OBJECT_NAMES + ('unkn27',)
        def __init__(self, t): _bind(self, self._names, t)

    class GameWeaponSubType(Enum):
        Persuadertron = 0x01
        Pistol = 0x02
        GaussGun = 0x03
        Shotgun = 0x04
        Uzi = 0x05
        Minigun = 0x06
        Laser = 0x07
        Flamer = 0x08
        LongRange = 0x09
        Scanner = 0x0A
        Medikit = 0x0B
        TimeBomb = 0x05
        AccessCard = 0x05
        Invalid1 = 0x0E
        Invalid2 = 0x0F
        Invalid3 = 0x10
        EnergyShield = 0x11

    class X_GameWeapon:
        _struct = ('<5HBB4HhHBBBB4H', 36)
        _names = _SYNDICATE_OBJECT_NAMES + ('offsetNextInventory', 'offsetPrevInventory', 'offsetOwner', 'unkn7')
        def __init__(self, t): _bind(self, self._names, t)

    class X_GameSfx:
        _struct = ('<5HBB4HhHBBBBH', 30)
        _names = _SYNDICATE_OBJECT_NAMES + ('offsetOwner',)
        def __init__(self, t): _bind(self, self._names, t)

    class GameScenarioType(Enum):
        Scn0 = 0x00
        Scn1 = 0x01
        Scn2 = 0x02
        Scn3 = 0x03
        Scn5 = 0x05
        Scn6 = 0x06
        Scn7 = 0x07
        Scn8 = 0x08
        Scn9 = 0x09
        ScnA = 0x0A
        ScnB = 0x0B

    class X_GameScenario:
        _struct = ('<2H4B', 8)
        _names = ('next', 'offsetObject', 'tileX', 'tileY', 'tileZ', 'type')
        def __init__(self, t): _bind(self, self._names, t)

    class X_GameMapinfo:
        _struct = ('<5HB3s', 14)
        _names = ('map', 'minX', 'minY', 'maxX', 'maxY', 'status', 'unkn1')
        def __init__(self, t): _bind(self, self._names, t)

    class X_GameObjective:
        _struct = ('<5HB3s', 14)
        _names = ('type', 'offset', 'tileX', 'tileY', 'tileZ', 'status', 'unkn1')
        def __init__(self, t): _bind(self, self._names, t)

    #endregion

    #region Objects

    class Palette:
        def __init__(self, records): self.records = records

    class Font:
        def __init__(self, records, data): self.records = records; self.data = data

    class MapData:
        def __init__(self, mapId, maxX, maxY, maxZ, tiles, width, height):
            self.mapId = mapId
            self.maxX = maxX
            self.maxY = maxY
            self.maxZ = maxZ
            self.tiles = tiles
            self.width = width
            self.height = height
        def patchTile(self, x, y, z, tile) -> None: self.tiles[(y * self.maxX + x) * self.maxZ + z] = tile
        def patch(self):
            if self.mapId == 0x27:
                for y in (63, 64, 65, 66, 67):
                    for x in (60, 61, 62): self.patchTile(x, y, 1, 0x27)
            elif self.mapId == 0x5B:
                self.patchTile(49, 27, 2, 0)
                self.patchTile(49, 28, 2, 0)
                self.patchTile(49, 29, 2, 0)
            return self

    class ColType(Enum):
        None_ = 0
        SlopeSN = 1
        SlopeNS = 2
        SlopeEW = 3
        SlopeWE = 4
        Ground = 5
        RoadSideEW = 6
        RoadSideWE = 7
        RoadSideSN = 8
        RoadSideNS = 9
        Wall = 10
        RoadCurve = 11
        HandrailLight = 12
        Roof = 13
        RoadPedCross = 14
        RoadMark = 15
        Unknown = 16

    class MapTile:
        def __init__(self, tiles, types): self.tiles = tiles; self.types = types

    class Sprite:
        def __init__(self, width = 0, height = 0, stride = 0, pixels = None):
            self.width = width
            self.height = height
            self.stride = stride
            self.pixels = pixels

    class SpriteElement:
        def __init__(self, sprite, offsetX, offsetY, flipped, nextElement):
            self.sprite = sprite
            self.offsetX = offsetX
            self.offsetY = offsetY
            self.flipped = flipped
            self.nextElement = nextElement

    class SpriteFrame:
        def __init__(self, firstElement, width, height, flags, nextElement):
            self.firstElement = firstElement
            self.width = width
            self.height = height
            self.flags = flags
            self.nextElement = nextElement

    class Mission:
        def __init__(self, infoCosts, enhtsCosts, briefings):
            self.infoCosts = infoCosts
            self.enhtsCosts = enhtsCosts
            self.briefings = briefings

    class Game:
        def __init__(self, header, people, vehicles, statics, weapons, sfxs, scenarios, unknown9, mapinfo, objectives, unknown11):
            self.header = header
            self.people = people
            self.vehicles = vehicles
            self.statics = statics
            self.weapons = weapons
            self.sfxs = sfxs
            self.scenarios = scenarios
            self.unknown9 = unknown9
            self.mapinfo = mapinfo
            self.objectives = objectives
            self.unknown11 = unknown11

    #endregion

    def __init__(self, r: BinaryReader, f: FileSource, s: Archive, kind: Kind):
        from openx.core import runSync
        NUMOFTILES = 256
        PIXELS_PER_BLOCK = 8
        BLOCK_LENGTH = PIXELS_PER_BLOCK // 2 + PIXELS_PER_BLOCK // 8
        TILE_WIDTH = 64
        TILE_HEIGHT = 48
        SUBTILE_WIDTH = 32
        SUBTILE_HEIGHT = 16
        SUBTILE_ROW_LENGTH = BLOCK_LENGTH * SUBTILE_WIDTH // PIXELS_PER_BLOCK
        self.obj = None
        data = Rnc.read(r)
        with BinaryReader(BytesIO(data)) as r2:
            streamSize = r2.length
            if kind == self.Kind.SoundData: raise Exception('Please load the .TAB')
            elif kind == self.Kind.SoundTab:
                got = runSync(s.getData(_changeExtension(f.path, '.DAT')))
                blob = got.getvalue() if isinstance(got, BytesIO) else bytes(got)
                offset = 0
                samples = []
                r2.seek(58)
                span = streamSize - 58
                size = self.X_SoundTab._struct[1]
                # C# integer division truncates toward zero, unlike Python //.
                count = (span // size if span >= 0 else -((-span) // size)) + 1
                for i, t in enumerate(r2.readSArray(self.X_SoundTab, count)):
                    if t.size <= 144: continue
                    end = offset + int(t.size)
                    # C# range indexer throws when the slice runs past the buffer.
                    if offset < 0 or end > len(blob): raise Exception('ArgumentOutOfRange')
                    sample = bytearray(blob[offset:end])
                    offset += t.size
                    self.X_SoundTab.patch(i, sample)
                    samples.append(bytes(sample))
                self.obj = samples
            elif kind == self.Kind.Palette:
                self.obj = self.Palette(r2.readSArray(self.X_Palette, 256))
            elif kind == self.Kind.Font:
                self.obj = self.Font(r2.readSArray(self.X_Font, 128), r2.readToEnd())
            elif kind == self.Kind.Req:
                self.obj = None
            elif kind == self.Kind.MapData:
                mapId = int(os.path.splitext(os.path.basename(f.path))[0][3:])
                t = r2.readS(self.X_MapData)
                maxX, maxY, maxZ = int(t.maxX), int(t.maxY), int(t.maxZ)
                width = (maxX + maxY) * (TILE_WIDTH // 2)
                height = (maxX + maxY + maxZ) * TILE_HEIGHT // 3
                payload = r2.readToEnd()
                r2.seek(12)
                lookups = r2.readPArray(None, '<I', maxX * maxY)
                zr = maxZ + 1
                tiles = bytearray(maxX * maxY * zr)
                for h in range(maxY):
                    for w in range(maxX):
                        idx = h * maxX + w
                        for z in range(maxZ):
                            tiles[idx * zr + z] = payload[lookups[idx] + z]
                maxZ += 1
                z = maxZ - 1
                for h in range(maxY):
                    for w in range(maxX):
                        tiles[(h * maxX + w) * maxZ + z] = 0
                self.obj = self.MapData(mapId, maxX, maxY, maxZ, tiles, width, height).patch()
            elif kind == self.Kind.MapColumn:
                # LINQ Cast<ColType>() boxes each byte and castclass throws. An empty buffer becomes an empty array.
                raw = r2.readToEnd()
                if len(raw) != 0: raise Exception('InvalidCastException')
                self.obj = []
            elif kind == self.Kind.MapTile:
                types = runSync(s.getAsset(Binary_SyndicateX, f.path.replace('HBLK', 'COL'))).obj
                def unpackBlock(block, pixels, index):
                    for j in range(4):
                        for i in range(8):
                            pixels[index + j * 8 + i] = 0xFF if _bit(block[j], 7 - i) != 0 else (
                                (_bit(block[4 + j], 7 - i) << 0) |
                                (_bit(block[8 + j], 7 - i) << 1) |
                                (_bit(block[12 + j], 7 - i) << 2) |
                                (_bit(block[16 + j], 7 - i) << 3)) & 0xFF
                def loadSubTile(offset, index, stride, pixels):
                    r2.seek(offset)
                    for i in range(SUBTILE_HEIGHT):
                        unpackBlock(r2.readBytes(SUBTILE_ROW_LENGTH), pixels, index + (SUBTILE_HEIGHT - 1 - i) * stride)
                tiles = []
                for t in r2.readSArray(self.X_MapTile, NUMOFTILES):
                    b = bytearray(TILE_WIDTH * TILE_HEIGHT)
                    loadSubTile(t.offsets0, 2 * SUBTILE_HEIGHT * TILE_WIDTH + 0 * SUBTILE_WIDTH, TILE_WIDTH, b)
                    loadSubTile(t.offsets1, 1 * SUBTILE_HEIGHT * TILE_WIDTH + 0 * SUBTILE_WIDTH, TILE_WIDTH, b)
                    loadSubTile(t.offsets2, 0 * SUBTILE_HEIGHT * TILE_WIDTH + 0 * SUBTILE_WIDTH, TILE_WIDTH, b)
                    loadSubTile(t.offsets3, 2 * SUBTILE_HEIGHT * TILE_WIDTH + 1 * SUBTILE_WIDTH, TILE_WIDTH, b)
                    loadSubTile(t.offsets4, 1 * SUBTILE_HEIGHT * TILE_WIDTH + 1 * SUBTILE_WIDTH, TILE_WIDTH, b)
                    loadSubTile(t.offsets5, 0 * SUBTILE_HEIGHT * TILE_WIDTH + 1 * SUBTILE_WIDTH, TILE_WIDTH, b)
                    tiles.append(b)
                self.obj = self.MapTile(tiles, types)
            elif kind == self.Kind.SpriteData:
                self.obj = BytesIO(r2.readToEnd())
            elif kind == self.Kind.SpriteTab:
                got = runSync(s.getAsset(Binary_SyndicateX, _changeExtension(f.path, '.DAT'))).obj
                with BinaryReader(got) as r3:
                    def unpackBlock(block, pixels, index):
                        for i in range(8):
                            pixels[index + i] = 0xFF if _bit(block[0], 7 - i) != 0 else (
                                (_bit(block[1], 7 - i) << 0) |
                                (_bit(block[2], 7 - i) << 1) |
                                (_bit(block[3], 7 - i) << 2) |
                                (_bit(block[4], 7 - i) << 3)) & 0xFF
                    def loadSprite(t, rle: bool):
                        width, height, stride = t.width, t.height, t.stride()
                        if width == 0 or height == 0: return self.Sprite()
                        pixels = bytearray(stride * height)
                        r3.seek(t.offset)
                        if rle:
                            for i in range(height):
                                spriteWidth = width
                                currentPixel = i * stride
                                b = r3.readByte()
                                runLength = b if b < 128 else -(256 - b)
                                while runLength != 0:
                                    spriteWidth -= runLength
                                    if runLength > 0:
                                        if currentPixel < 0: currentPixel = 0
                                        if currentPixel + runLength > height * stride: runLength = height * stride - currentPixel
                                        for _j in range(runLength):
                                            pixels[currentPixel] = r3.readByte()
                                            currentPixel += 1
                                    elif runLength < 0:
                                        runLength = -runLength
                                        if currentPixel < 0: currentPixel = 0
                                        if currentPixel + runLength > height * stride: runLength = height * stride - currentPixel
                                        for _j in range(runLength):
                                            pixels[currentPixel] = 0xFF
                                            currentPixel += 1
                                        pixels[currentPixel:currentPixel + runLength] = b'\xff' * runLength
                                        currentPixel += runLength
                                    elif runLength == 0:
                                        spriteWidth = 0
                                    b = r3.readByte()
                                    runLength = b if b < 128 else -(256 - b)
                        else:
                            for j in range(height):
                                currentPixel = j * stride
                                i = 0
                                while i < width:
                                    unpackBlock(r3.readBytes(BLOCK_LENGTH), pixels, currentPixel)
                                    currentPixel += PIXELS_PER_BLOCK
                                    i += PIXELS_PER_BLOCK
                        return self.Sprite(width, height, stride, pixels)
                    count = int(streamSize // self.X_SpriteTab._struct[1])
                    self.obj = [loadSprite(t, False) for t in r2.readSArray(self.X_SpriteTab, count)]
            elif kind == self.Kind.SpriteElement:
                def element(t):
                    return self.SpriteElement(
                        t.sprite // 6,
                        t.offsetX - 65536 if (t.offsetX & (1 << 15)) != 0 else t.offsetX,
                        t.offsetY - 65536 if (t.offsetY & (1 << 15)) != 0 else t.offsetY,
                        t.flipped != 0,
                        t.nextElement)
                count = int(streamSize // self.X_SpriteElement._struct[1])
                self.obj = [element(t) for t in r2.readSArray(self.X_SpriteElement, count)]
            elif kind == self.Kind.SpriteFrame:
                count = int(streamSize // self.X_SpriteFrame._struct[1])
                self.obj = [self.SpriteFrame(t.firstElement, t.width, t.height, t.flags, t.nextElement) for t in r2.readSArray(self.X_SpriteFrame, count)]
            elif kind == self.Kind.SpriteAnim:
                self.obj = r2.readPArray(None, '<H', int(streamSize // 2))
            elif kind == self.Kind.Mission:
                # Encoding.ASCII replaces each byte above 127 with '?'.
                text = ''.join(chr(b) if b < 128 else '?' for b in r2.readToEnd())
                parts = text.split('\n|\n')
                self.obj = self.Mission(
                    [_atoi(x) for x in parts[0].split('\n')],
                    [_atoi(x) for x in parts[1].split('\n')],
                    parts[2:])
            elif kind == self.Kind.Game:
                self.obj = self.Game(
                    r2.readS(self.X_GameHeader),
                    r2.readSArray(self.X_GamePerson, 256),
                    r2.readSArray(self.X_GameVehicle, 64),
                    r2.readSArray(self.X_GameStatic, 400),
                    r2.readSArray(self.X_GameWeapon, 512),
                    r2.readSArray(self.X_GameSfx, 256),
                    r2.readSArray(self.X_GameScenario, 2048),
                    r2.readBytes(448),
                    r2.readS(self.X_GameMapinfo),
                    r2.readSArray(self.X_GameObjective, 10),
                    r2.readBytes(1896))

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'Bullfrog')),
        MetaInfo('Bullfrog', items = [
            ])
        ]

#endregion
