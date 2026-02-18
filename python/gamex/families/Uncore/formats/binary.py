from __future__ import annotations
import os, numpy as np
from io import BytesIO
from PIL import Image
from enum import Enum
from openstk import _pathExtension
from openstk.gfx import Raster, DDS_HEADER, Texture_Bytes, ITexture, TextureFormat, TexturePixel
from gamex import ArcBinary, ArcBinaryT, FileSource, BinaryArchive, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo
from zipfile import ZipFile

#region Binary_Bik

# Binary_Bik
class Binary_Bik(IHaveMetaInfo):
    @staticmethod
    def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Bik(r, f.fileSize)

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
    def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Dds(r)

    def __init__(self, r: BinaryReader, readMagic: bool = True):
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

    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0
    def create(self, platform: str, func: callable): return func(Texture_Bytes(self.bytes, self.format[2], self.spans))

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
    def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Fsb(r, f.fileSize)

    def __init__(self, r: BinaryReader, fileSize: int):
        self.data = r.readBytes(fileSize)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'FSB Audio'))
        ]

#endregion

#region Binary_Img

# Binary_Img
class Binary_Img(IHaveMetaInfo, ITexture):
    @staticmethod
    def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Img(r, f)

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
                # 8-bit pixels, Grayscale
                if self.image.mode == 'L': palette = [x for xs in [[x, x, x] for x in range(255)] for x in xs]
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
    def create(self, platform: str, func: callable): return func(Texture_Bytes(self.bytes, self.format[1], None))

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
    def factory(message: str): return Binary_Msg(message)

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
    def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Pcx(r, f.fileSize)

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
        return func(Texture_Bytes(bytes, self.format, None))

    #endregion

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Binary_Pcx', items = [
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}')
            ])
        ]

#endregion

#region Binary_Snd
    
# Binary_Snd
class Binary_Snd(IHaveMetaInfo):
    @staticmethod
    def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Snd(r, f.fileSize)

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
    def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Tga(r, f)

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
            s = ColorMap()
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

    def _lambdax(self) -> Texture_Bytes:
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
        
        return Texture_Bytes(bytes, self.format, None)
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
    def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Txt(r, f.fileSize)

    def __init__(self, r: BinaryReader, fileSize: int):
        self.data = r.readBytes(fileSize).decode('utf8', 'ignore')

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self.data))
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
        arc: ZipFile
        source.tag = arc = ZipFile(r.f)
        match self.key:
            case None: pass
            case s if isinstance(key, str): arc.setpassword(s)
            case z if isinstance(key, bytes): raise NotImplementedError('Binary_Zip')
        source.files = [FileSource(
            path = s.filename, #.replace('\\', '/'),
            packedSize = s.compress_size,
            fileSize = s.file_size,
            tag = s
            ) for s in arc.infolist() if not s.is_dir()]

    # readData
    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        arc: ZipFile = source.tag
        print(arc.read(file.path))

#endregion

#region Binary_TestTri

class Binary_TestTri(IHaveMetaInfo):
    @staticmethod
    def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_TestTri(r)

    def __init__(self, r: BinaryReader): pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'TestTri', name = os.path.basename(file.path), value = self))
        ]

#endregion
