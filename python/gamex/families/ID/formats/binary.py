from __future__ import annotations
import os, numpy as np
from io import BytesIO
from enum import Enum, Flag
from openx.core import _throw, _pathExtension, unsafe, BinaryReader
from openx.gfx import Raster, TextureAsBytes, ITexture, ITextureSelect, ITextureFrames, TextureFlags, TextureFormat, TexturePixel
from gamex import Archive, BinaryArchive, ArcBinary, ArcBinaryT, FileSource, MetaInfo, MetaManager, MetaContent, IHaveMetaInfo
from gamex.families.Uncore.formats.compression import decompressBlast
from hashlib import md5
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.backends import default_backend
from cryptography.hazmat.primitives.asymmetric import padding
from cryptography.hazmat.primitives import serialization

# lumps
class X_LumpON:
     offset: int
     num: int

class X_LumpNO:
    num: int
    offset: int

class X_LumpNO2:
    num: int
    offset: int
    offset2: int

class X_Lump2NO:
    offset2: int
    num: int
    offset: int

#region Binary_Lmp - tag::Binary_Lmp[]

# Binary_Lmp
class Binary_Lmp(ITexture, IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Lmp(r, f, s)

    palette: 'Binary_Lmp' = None
    colormap: 'Binary_Lmp' = None

    @staticmethod
    def toLightPixel(light: int, pixel: int) -> bytes:
        return Binary_Lmp.palette.paletteRecords[Binary_Lmp.colormap.colormapRecords[(light >> 3) & 0x1F][pixel]]

    def __init__(self, r: BinaryReader, f: FileSource, s: Archive):
        self.paletteRecords = None
        self.colormapRecords = None
        self.pixels = None
        match os.path.splitext(os.path.basename(f.path))[0]:
            case 'palette':
                self.paletteRecords = r.readFArray(lambda s: bytearray(s.readBytes(3)) + b'\x00', 256)
                Binary_Lmp.palette = self
                return
            case 'colormap':
                self.colormapRecords = r.readFArray(lambda s: s.readBytes(256), 32)
                Binary_Lmp.colormap = self
                return
            case _:
                if Binary_Lmp.palette is None or Binary_Lmp.palette.paletteRecords is None: raise NotImplementedError()
                self.width = r.readInt32()
                self.height = r.readInt32()
                indices = r.readBytes(self.width * self.height)
                pixels = bytearray()
                for index in indices: pixels += Binary_Lmp.toLightPixel(32 << 3, index)
                self.pixels = pixels
                return

    #region ITexture

    format: tuple = (TextureFormat.BGRA32, TexturePixel.Unknown)
    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0
    def create(self, platform: str, func: callable): return func(TextureAsBytes(self.pixels, self.format, None))

    #endregion

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'TextureName', name = os.path.basename(file.path), value = self)),
        MetaInfo('TextureName', items = [
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}')
            ])
        ]

#endregion - end::Binary_Lmp[]

#region Binary_Mdl - tag::Binary_Mdl[]

# Binary_Mdl
class Binary_Mdl(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Mdl(r)

    def __init__(self, r: BinaryReader):
        pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'UnknownFileModel File')),
        MetaInfo('UnknownFileModel', items = [
            MetaInfo(f'Records: {1}')
            ])
        ]

#endregion - end::Binary_Mdl[]

#region Binary_Spr - tag::Binary_Spr[]

# Binary_Spr
class Binary_Spr(ITextureFrames, IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Spr(r)

    #region Headers

    S_MAGIC = 0x50534449 #: IDSP

    class SprType(Enum):
        VP_PARALLEL_UPRIGHT = 0
        FACING_UPRIGHT = 1
        VP_PARALLEL = 2
        ORIENTED = 3
        VP_PARALLEL_ORIENTED = 4

    class SprTextFormat(Enum):
        SPR_NORMAL = 0
        SPR_ADDITIVE = 1
        SPR_INDEXALPHA = 2
        SPR_ALPHTEST = 3

    class SprSynchType(Enum):
        Synchronized = 0
        Random = 1

    class S_Header:
        _struct = ('<I3if3ifi', 40)
        def __init__(self, t):
            (self.magic,
            self.version,
            self.type,
            self.textFormat,
            self.boundingRadius,
            self.maxWidth,
            self.maxHeight,
            self.numFrames,
            self.beamLen,
            self.synchType) = t

    class S_Frame:
        _struct = ('<5i', 20)
        def __init__(self, t):
            (self.group,
            self.originX,
            self.originY,
            self.width,
            self.height) = t

    #endregion

    def __init__(self, r: BinaryReader):
        # read file
        header = r.readS(self.S_Header)
        if header.magic != self.S_MAGIC: raise Exception('BAD MAGIC')

        # load palette
        self.palette = r.readBytes(r.readUInt16() * 3)

        # load frames
        frames = self.frames = [self.S_Frame] * header.numFrames
        pixels = self.pixels = [bytearray] * header.numFrames
        for i in range(header.numFrames):
            frame = frames[i] = r.readS(self.S_Frame)
            pixels[i] = r.readBytes(frame.width * frame.height)
        self.width = frames[0].width
        self.height = frames[0].height
        self.bytes = bytearray(self.width * self.height << 2)
        self.frame = 0

    #region ITexture

    format: tuple = (TextureFormat.RGBA32, TexturePixel.Unknown)
    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0
    fps: int = 60
    def create(self, platform: str, func: callable): return func(TextureAsBytes(self.bytes, self.format, None))

    def hasFrames(self) -> bool: return self.frame < len(self.frames)

    def nextFrame(self) -> bool:
        p = self.pixels[self.frame]
        Raster.blitByPalette(self.bytes, 4, p, self.palette, 3)
        self.frame += 1
        return True

    #endregion

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'VideoTexture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Sprite', items = [
            MetaInfo(f'Frames: {len(self.frames)}'),
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}'),
            MetaInfo(f'Mipmaps: {self.mipMaps}')
            ])
        ]

#endregion - end::Binary_Spr[]


# Binary_Wad3X
class Binary_Wad3X(IHaveMetaInfo, ITexture):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Wad3X(r, f)

    #region Headers

    class CharInfo:
        _struct = ('<2H', 4)
        def __init__(self, t):
            (self.startOffset,
            self.charWidth) = t

    class Formats(Enum): Nonex = 0; Tex2 = 0x40; Pic = 0x42; Tex = 0x43; Fnt = 0x46

    #endregion

    def __init__(self, r: BinaryReader, f: FileSource):
        match _pathExtension(f.path):
            case '.pic': type = self.Formats.Pic
            case '.tex': type = self.Formats.Tex
            case '.tex2': type = self.Formats.Tex2
            case '.fnt': type = self.Formats.Fnt
            case _: type = self.Formats.Nonex
        self.transparent = os.path.basename(f.path).startswith('{')
        self.format = (type, (TextureFormat.RGBA32, TexturePixel.Unknown)) if self.transparent \
            else (type, (TextureFormat.RGB24, TexturePixel.Unknown))
        self.name = r.readFWString(16) if type == self.Formats.Tex2 or type == self.Formats.Tex else None
        self.width = r.readUInt32()
        self.height = r.readUInt32()

        # validate
        if self.width > 0x1000 or self.height > 0x1000: raise Exception('Texture width or height exceeds maximum size!')
        elif self.width == 0 or self.height == 0: raise Exception('Texture width and height must be larger than 0!')

        # read pixel offsets
        if type == self.Formats.Tex2 or type == self.Formats.Tex:
            offsets = [r.readUInt32(), r.readUInt32(), r.readUInt32(), r.readUInt32()]
            if r.tell() != offsets[0]: raise Exception('BAD OFFSET')
        elif type == self.Formats.Fnt:
            self.width = 0x100
            rowCount = r.readUInt32()
            rowHeight = r.readUInt32()
            charInfos = r.readSArray(self.CharInfo, 0x100)

        # read pixels
        pixelSize = self.width * self.height
        pixels = self.pixels = [r.readBytes(pixelSize), r.readBytes(pixelSize >> 2), r.readBytes(pixelSize >> 4), r.readBytes(pixelSize >> 6)] if type == self.Formats.Tex2 or type == self.Formats.Tex \
            else [r.readBytes(pixelSize)]
        self.mipMaps = len(pixels)

        # read pallet
        r.skip(2)
        p = self.palette = r.readBytes(0x100 * 3); j = 0
        if type == self.Formats.Tex2:
            for i in range(0x100):
                p[j + 0] = i
                p[j + 1] = i
                p[j + 2] = i
                j += 3

    #region ITexture

    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0

    def _lambdax(self) -> TextureAsBytes:
        bbp = 4 if self.transparent else 3
        buf = bytearray(sum([len(x) for x in self.pixels]) * bbp); mv = memoryview(buf)
        spans = [range(0, 0)] * len(self.pixels); offset = 0
        for i, p in enumerate(self.pixels):
            size = len(p) * bbp; span = spans[i] = range(offset, offset + size); offset += size
            Raster.blitByPalette(mv[span.start:span.stop], bbp, p, self.palette, 3, 0xFF if self.transparent else None)
        return TextureAsBytes(buf, self.format[1], spans)
    def create(self, platform: str, func: callable): return func(_lambdax)

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

#endregion - end::Binary_Wad3X[]

#region Binary_Pak - tag::Binary_Pak[]

# Binary_Pak
class Binary_Pak(ArcBinaryT):
    #region Headers

    P_MAGIC = 0x4b434150 #: PACK

    class P_Header:
        _struct = ('<I2i', 12)
        def __init__(self, t):
            (self.magic,
            self.dirOffset,
            self.dirLength) = t

    class P_File:
        _struct = ('<56s2i', 64)
        def __init__(self, t):
            (self.path,
            self.offset,
            self.fileSize) = t

    #endregion

    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        header = r.readS(self.P_Header)
        if header.magic != self.P_MAGIC: raise Exception('BAD MAGIC')
        numFiles = header.dirLength // self.P_File._struct[1]
        r.seek(header.dirOffset)
        files = []
        for s in r.readSArray(self.P_File, numFiles):
            path = unsafe.fixedAString(s.path, 56).replace('\\', '/')
            file = FileSource(path = path, offset = s.offset, fileSize = s.fileSize)
            if path.lower().endswith('.wad'): file.arc = Binary_Wad.SubArchive(source, file, path)
            files.append(file)
        source.files = files

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        r.seek(file.offset)
        return BytesIO(r.readBytes(file.fileSize))

#endregion - end::Binary_Pak[]

#region Binary_Wad - tag::Binary_Wad[]

# Binary_Wad
class Binary_Wad(ArcBinaryT):
    #region Headers

    W_MAGIC = 0x32444157 #: WAD2

    class W_Header:
        _struct = ('<I2i', 12)
        def __init__(self, t):
            (self.magic,
            self.lumpCount,
            self.lumpOffset) = t

    class W_Lump:
        _struct = ('<3i4b16s', 32)
        def __init__(self, t):
            (self.offset,
            self.packedSize,
            self.fileSize,
            self.type,
            self.compression,
            self.padding0,
            self.padding1,
            self.path) = t

    #endregion

    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        header = r.readS(self.W_Header)
        if header.magic != self.W_MAGIC: raise Exception('BAD MAGIC')
        r.seek(header.lumpOffset)
        source.files = [FileSource(
            path = f'{unsafe.fixedAString(s.path, 16).replace("\\", "/")}.tex',
            hash = s.type,
            offset = s.offset,
            packedSize = s.packedSize,
            fileSize = s.fileSize,
            compressed = s.compression) for s in r.readSArray(self.W_Lump, header.lumpCount)]

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        r.seek(file.offset)
        return BytesIO(r.readBytes(file.fileSize))

#endregion - end::Binary_Wad[]