from __future__ import annotations
import os
from io import BytesIO
from struct import pack_into
from gamex.core.binary import ArcBinaryT
from gamex.core.meta import FileSource, IHaveMetaInfo, MetaContent, MetaInfo
from gamex.families.Uncore.formats.compression import decompressLzss, decompressZlib
from openx.gfx import ITexture, ITextureFrames, ITextureSelect, TextureAsBytes, TextureFlags, TextureFormat, TexturePixel

# typedefs
class BinaryReader: pass
class BinaryArchive: pass

#region Binary_Dat - tag::Binary_Dat[]

# Binary_Dat
class Binary_Dat(ArcBinaryT):
    #region F1/F2

    F1_HEADER_FILEID = 0x000000001
    class F1_Header:
        _struct = ('>4I', 16)
        def __init__(self, t):
            (self.directoryCount,
            self.unknown1,
            self.unknown2,
            self.unknown3) = t
    class F1_Directory:
        _struct = ('>4I', 16)
        def __init__(self, t):
            (self.fileCount,
            self.unknown1,
            self.unknown2,
            self.unknown3) = t
    class F1_File:
        _struct = ('>4I', 16)
        def __init__(self, t):
            (self.attributes,
            self.offset,
            self.size,
            self.packedSize) = t

    F2_HEADER_FILEID = 0x000000011
    class F2_Header:
        _struct = ('<2I', 8)
        def __init__(self, t):
            (self.treeSize,
            self.dataSize) = t
    class F2_File:
        _struct = ('<B3I', 13)
        def __init__(self, t):
            (self.type,
            self.realSize,
            self.packedSize,
            self.offset) = t

    #endregion

    # read
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        gameId = source.game.id

        # Fallout
        if gameId == 'Fallout':
            source.magic = self.F1_HEADER_FILEID
            header = r.readS(self.F1_Header)
            directoryPaths = [r.readL8UString().replace('\\', '/') for x in range(header.directoryCount)]

            # create file metadatas
            source.files = files = []
            for i in range(header.directoryCount):
                directory = r.readS(self.F1_Directory)
                directoryPath = f'{directoryPaths[i]}/' if directoryPaths[i] != '.' else ''
                for _ in range(directory.fileCount):
                    path = directoryPath + r.readL8UString().replace('\\', '/')
                    file = r.readS(self.F1_File)
                    files.append(FileSource(
                        path = path,
                        compressed = file.attributes & 0x40,
                        offset = file.offset,
                        fileSize = file.size,
                        packedSize = file.packedSize))
        
        # Fallout2
        elif gameId == 'Fallout2':
            source.magic = self.F2_HEADER_FILEID
            r.seek(r.length - 8)
            header = r.readS(self.F2_Header)
            r.seek(header.dataSize - header.treeSize - 8)

            # create file metadatas
            source.files = files = []
            filenum = r.readInt32()
            for i in range(filenum):
                path = r.readL32UString().replace('\\', '/')
                file = r.readS(self.F2_File)
                files.append(FileSource(
                    path = path,
                    compressed = file.type,
                    fileSize = file.realSize,
                    packedSize = file.packedSize,
                    offset = file.offset))

    # readData
    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        magic = source.magic
        # F1
        if magic == self.F1_HEADER_FILEID:
            r.seek(file.offset)
            return BytesIO(
                r.readBytes(file.packedSize) if file.compressed == 0 else \
                decompressLzss(r, file.packedSize, file.fileSize))
        # F2
        elif magic == self.F2_HEADER_FILEID:
            r.seek(file.offset)
            return BytesIO(
                decompressZlib(r, file.packedSize, -1) if r.peek(lambda z : z.readUInt16()) == 0xda78 else \
                r.readBytes(file.packedSize))
        else: raise Exception('BAD MAGIC')

#endregion - end::Binary_Dat[]

# VGA 6-bit channels shifted by 2. FRM index 0 stays clear. RIX is opaque.
def _vgaColor(b0: int, b1: int, b2: int, alpha: int) -> int:
    return (alpha << 24) | (b2 << 18) | (b1 << 10) | (b0 << 2)

def _readVgaPalette(r: BinaryReader, clear0: bool) -> list[int]:
    rgb = r.readBytes(256 * 3)
    return [_vgaColor(rgb[i], rgb[i + 1], rgb[i + 2], 0 if clear0 and i == 0 else 0xFF) for i in range(0, 256 * 3, 3)]

def _indexedRgba(indices: bytes, rgba32: list[int], width: int, height: int) -> bytearray:
    image = bytearray(width * height * 4)
    for j, index in enumerate(indices): pack_into('<I', image, j * 4, rgba32[index])
    return image

#region Binary_Frm - tag::Binary_Frm[]

# Binary_Frm
class Binary_Frm(IHaveMetaInfo, ITextureFrames, ITextureSelect):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive) -> Binary_Frm:
        pallet = await Binary_Frm.getPalletObjAsync(f.path, s)
        if pallet is None: raise Exception('No pallet found')
        return Binary_Frm(r, pallet)

    #region Headers

    class FrmHeader:
        _struct = ('>I3H6h6h6II', 62)
        def __init__(self, t):
            self.version = t[0]
            self.fps = t[1]
            self.actionFrame = t[2]
            self.framesPerDirection = t[3]
            self.pixelShiftX = t[4:10]
            self.pixelShiftY = t[10:16]
            self.frameOffset = t[16:22]
            self.sizeOfFrame = t[22]

    class FrmFrame:
        _struct = ('>2HI2h', 12)
        def __init__(self, t):
            (self.width,
            self.height,
            self.size,
            self.pixelShiftX,
            self.pixelShiftY) = t

    #endregion

    defaultPallet: Binary_Pal2 = None

    def __init__(self, r: BinaryReader, pallet: Binary_Pal2):
        rgba32 = pallet.rgba32
        header = r.readS(self.FrmHeader)
        frames = []
        i = 0
        # Header.FrameOffset is unread here. C# reads the unset field and does not seek.
        while i < 6 * header.framesPerDirection and r.tell() < r.length:
            frame = r.readS(self.FrmFrame)
            data = r.readBytes(frame.size)
            frames.append((frame, _indexedRgba(data, rgba32, frame.width, frame.height)))
            i += 1
        self.header = header
        self.frames = frames
        self.fps = header.fps
        self.maxId = 1 if len(frames) == 1 else header.framesPerDirection
        self.select(0)

    @staticmethod
    async def getPalletObjAsync(path: str, s: Archive) -> Binary_Pal2:
        palletPath = f'{path[:-4]}.PAL'
        if s.contains(palletPath): return await s.getAsset(Binary_Pal2, palletPath)
        if Binary_Frm.defaultPallet is None and s.contains('COLOR.PAL'):
            Binary_Frm.defaultPallet = await s.getAsset(Binary_Pal2, 'COLOR.PAL')
            Binary_Frm.defaultPallet.setColors()
        return Binary_Frm.defaultPallet

    #region ITexture

    format: tuple = (TextureFormat.RGBA32, TexturePixel.Unknown)
    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0
    def create(self, platform: str, func: callable): return func(TextureAsBytes(self.bytes, self.format, None))

    def select(self, id: int) -> None:
        frame, image = self.frames[id]
        self.bytes = image
        self.width = frame.width
        self.height = frame.height

    def hasFrames(self) -> bool: return False
    def nextFrame(self) -> bool: return False

    #endregion

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Binary_Frm', items = [
            MetaInfo(f'Frames: {len(self.frames)}'),
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}'),
            ])
        ]

#endregion - end::Binary_Frm[]

#region Binary_Pal2 - tag::Binary_Pal2[]

# Binary_Pal2
class Binary_Pal2(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive) -> Binary_Pal2: return Binary_Pal2(r, f)

    def __init__(self, r: BinaryReader, f: FileSource = None):
        self.rgba32 = _readVgaPalette(r, True)

    def setColors(self) -> None:
        for i in range(229, 233): self.rgba32[i] = 0xffff0000 # animated green (for radioactive waste)
        for i in range(233, 238): self.rgba32[i] = 0xff00ff00 # bright blue (computer screens)
        for i in range(238, 248): self.rgba32[i] = 0xff000000 # orange, red and yellow (for fires)
        for i in range(248, 255): self.rgba32[i] = 0xff00ff00 # bright blue (computer screens)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Null', name = os.path.basename(file.path), value = self)),
        MetaInfo('Binary_Pal2', items = [])
        ]

#endregion - end::Binary_Pal2[]

#region Binary_Rix - tag::Binary_Rix[]

# Binary_Rix
class Binary_Rix(IHaveMetaInfo, ITexture):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive) -> Binary_Rix: return Binary_Rix(r, f)

    #region Headers

    class Header:
        _struct = ('<I2H2B', 10)
        def __init__(self, t):
            (self.magic,
            self.width,
            self.height,
            self.paletteType,
            self.storageType) = t

    #endregion

    def __init__(self, r: BinaryReader, f: FileSource = None):
        header = r.readS(self.Header)
        rgba32 = _readVgaPalette(r, False)
        data = r.readBytes(header.width * header.height)
        self.bytes = _indexedRgba(data, rgba32, header.width, header.height)
        self.width = header.width
        self.height = header.height

    #region ITexture

    format: tuple = (TextureFormat.RGBA32, TexturePixel.Unknown)
    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0
    def create(self, platform: str, func: callable): return func(TextureAsBytes(self.bytes, self.format, None))

    #endregion

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Binary_Rix', items = [
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}'),
            ])
        ]

#endregion - end::Binary_Rix[]

#region Binary_Map - tag::Binary_Map[]

# MapLevel
class MapLevel:
    def __init__(self, elevation: int, floor: list[int], roof: list[int]):
        self.elevation = elevation
        self.floor = floor
        self.roof = roof

# Binary_Map. Fallout 1 and 2. Header is 0xEC bytes, big-endian.
class Binary_Map:
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive) -> Binary_Map: return Binary_Map(r, f, s)

    class Header:
        _struct = ('>i16s9iI', 60)
        def __init__(self, t):
            (self.version,
            self.name,
            self.enteringTile,
            self.enteringElevation,
            self.enteringRotation,
            self.localVarsCount,
            self.scriptIndex,
            self.flags,
            self.darkness,
            self.globalVarsCount,
            self.mapIndex,
            self.lastVisitTime) = t

    def __init__(self, r: BinaryReader, f: FileSource = None, s: Archive = None):
        header = r.readS(self.Header)
        if header.version != 19 and header.version != 20: raise Exception(header.version)
        r.skip(44 * 4)
        globalCount = header.globalVarsCount if header.globalVarsCount > 0 else 0
        localCount = header.localVarsCount if header.localVarsCount > 0 else 0
        if globalCount: r.skip(globalCount * 4)
        if localCount: r.skip(localCount * 4)
        levels = []
        for elevation, bit in ((0, 0x2), (1, 0x4), (2, 0x8)):
            if header.flags & bit: continue
            entries = r.readPArray(None, '>I', 10000)
            levels.append(MapLevel(
                elevation,
                [e & 0x0FFF for e in entries],
                [(e >> 16) & 0x0FFF for e in entries]))
        self.version = header.version
        self.name = _mapName(header.name)
        self.enteringTile = header.enteringTile
        self.enteringElevation = header.enteringElevation
        self.enteringRotation = header.enteringRotation
        self.flags = header.flags
        self.mapIndex = header.mapIndex
        self.levels = levels

def _mapName(raw: bytes) -> str:
    end = raw.find(0)
    if end >= 0: raw = raw[:end]
    return ''.join(chr(b) if b < 128 else '?' for b in raw)

#endregion - end::Binary_Map[]