import os
from io import BytesIO
from itertools import groupby
from enum import Enum
from openstk import log, Int3, IWriteToStream
from gamex import FileSource, ArcBinaryT, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo, DesSer, IDatabase
from gamex.families.Uncore.formats.compression import decompressLz4, decompressZlib
from gamex.families.Bethesda.formats.records import FormType, Reader, Record, RecordGroup

# typedefs
class BinaryReader: pass
class BinaryArchive: pass

#region Binary_Ba2 - tag::Binary_Ba2[]

# Binary_Ba2
class Binary_Ba2(ArcBinaryT):
    #region Headers : TES5

    # Default header data
    F4_MAGIC = 0x58445442    # Magic for Fallout 4 BA2, the literal string "BTDX".
    F4_VERSION1 = 0x01        # Version number of a Fallout 4 BA2
    F4_VERSION2 = 0x02        # Version number of a Starfield BA2
    
    class HDR5Type(Enum): GNRL = 0x4c524e47; DX10 = 0x30315844; GNMF = 0x464d4e47

    class HDR5:
        _struct = ('<3IQ', 20)
        def __init__(self, tuple):
            (self.version,
            self.type,
            self.numFiles,
            self.nameTableOffset) = tuple
            self.type = Binary_Ba2.HDR5Type(self.type)

    class FILE5:
        _struct = ('<4IQ3I', 36)
        def __init__(self, tuple):
            (self.nameHash,
            self.ext,
            self.dirHash,
            self.flags,
            self.offset,
            self.packedSize,
            self.fileSize,
            self.align) = tuple

    class TEX5:
        _struct = ('<3I2B3H4B', 24)
        def __init__(self, tuple):
            (self.nameHash,
            self.ext,
            self.dirHash,
            self.unk0C,
            self.numChunks,
            self.chunkHeaderSize,
            self.height,
            self.width,
            self.numMips,
            self.format,
            self.isCubemap,
            self.tileMode) = tuple

    class TEXCHUNK5:
        _struct = ('<Q2I2HI', 24)
        def __init__(self, tuple):
            (self.offset,
            self.packedSize,
            self.fileSize,
            self.startMip,
            self.endMip,
            self.align) = tuple

    class GNMF5:
        _struct = ('<3I2BH32sQ4I', 72)
        def __init__(self, tuple):
            (self.nameHash,
            self.ext,
            self.dirHash,
            self.unk0C,
            self.numChunks,
            self.unk0E,
            self._header,
            self.offset,
            self.packedSize,
            self.fileSize,
            self.unk40,
            self.align) = tuple

    #endregion

    # read
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        source.magic = magic = r.readUInt32()

        # Fallout 4 - Starfield
        if magic == self.F4_MAGIC:
            header = r.readS(self.HDR5)
            if header.version > self.F4_VERSION2: raise Exception('BAD MAGIC')
            source.version = header.version
            source.files = files = [None] * header.numFiles
            # version2
            # if header.version == self.F4_VERSION2: r.skip(8)

            # General BA2 Format
            match header.type:
                # General BA2 Format
                case self.HDR5Type.GNRL:
                    headerFiles = r.readTArray(self.FILE5, header.numFiles)
                    for i in range(header.numFiles):
                        headerFile = headerFiles[i]
                        files[i] = FileSource(
                            compressed = 1 if headerFile.packedSize != 0 else 0,
                            packedSize = headerFile.packedSize,
                            fileSize = headerFile.fileSize,
                            offset = headerFile.offset)
                # Texture BA2 Format
                case self.HDR5Type.DX10:
                    for i in range(header.numFiles):
                        headerTexture = r.readS(self.TEX5)
                        headerTextureChunks = r.readTArray(self.TEXCHUNK5, headerTexture.numChunks)
                        firstChunk = headerTextureChunks[0]
                        files[i] = FileSource(
                            fileInfo = headerTexture,
                            packedSize = firstChunk.packedSize,
                            fileSize = firstChunk.fileSize,
                            offset = firstChunk.offset,
                            tag = headerTextureChunks)
                # GNMF BA2 Format
                case self.HDR5Type.GNMF:
                    for i in range(header.numFiles):
                        headerGNMF = r.readS(self.GNMF5)
                        headerTextureChunks = r.readTArray(self.TEXCHUNK5, headerGNMF.numChunks)
                        files[i] = FileSource(
                            fileInfo = headerGNMF,
                            packedSize = headerGNMF.packedSize,
                            fileSize = headerGNMF.fileSize,
                            offset = headerGNMF.offset,
                            tag = headerTextureChunks)
                case _: raise Exception(f'Unknown: {header.type}')

            # assign full names to each file
            if header.nameTableOffset > 0:
                r.seek(header.nameTableOffset)
                path = r.readL16Encoding().replace('\\', '/')
                for file in files: file.path = path

    # readData
    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        r.seek(file.offset)

        # General BA2 Format
        if file.fileInfo == None:
            return BytesIO(
                decompressZlib(r, file.packedSize, file.fileSize) if file.compressed != 0 else \
                r.readBytes(file.fileSize))

        # Texture BA2 Format
        elif file.fileInfo is self.TEX5:
            pass

        # GNMF BA2 Format
        elif file.fileInfo is self.GNMF5:
            pass

        else: raise Exception(f'Unknown fileInfo: {file.fileInfo}')

#endregion - end::Binary_Ba2[]

#region Binary_Bsa - tag::Binary_Bsa[]

# Binary_Bsa
class Binary_Bsa(ArcBinaryT):
    #region Headers : TES4

    OB_MAGIC = 0x00415342       # Magic for Oblivion BSA, the literal string "BSA\0".
    OB_VERSION = 0x67           # Version number of an Oblivion BSA
    F3_VERSION = 0x68           # Version number of a Fallout 3 BSA
    SE_VERSION = 0x69           # Version number of a Skyrim SE BSA

    # Archive flags
    FLAG4_PATHNAMES = 0x0001    # Whether the BSA has names for paths
    FLAG4_FILENAMES = 0x0002    # Whether the BSA has names for files
    FLAG4_COMPRESSFILES = 0x0004 # Whether the files are compressed
    FLAG4_PREFIXS = 0x0100      # Whether the name is prefixed to the data?

    # Bitmasks for the size field in the header
    FILE4_SIZEMASK = 0x3fffffff     # Bit mask with OB_HeaderFile:SizeFlags to get the compression status
    FILE4_SIZECOMPRESS = 0xC0000000 # Bit mask with OB_HeaderFile:SizeFlags to get the compression status

    class HDR4:
        _struct = ('<8I', 32)
        def __init__(self, tuple):
            (self.version,
            self.folderRecordOffset,
            self.archiveFlags,
            self.folderCount,
            self.fileCount,
            self.folderNameLength,
            self.fileNameLength,
            self.fileFlags) = tuple

    class DIR4:
        _struct = ('<Q2I', 16)
        def __init__(self, tuple):
            (self.hash,
            self.fileCount,
            self.offset) = tuple

    class DIR4SE:
        _struct = ('<Q2IQ', 24)
        def __init__(self, tuple):
            (self.hash,
            self.fileCount,
            self.unk,
            self.offset) = tuple

    class FILE4:
        _struct = ('<Q2I', 16)
        def __init__(self, tuple):
            (self.hash,
            self.size,
            self.offset) = tuple

    #endregion

    #region Headers : TES3

    MW_MAGIC = 0x00000100    # Magic for Morrowind BSA

    class HDR3:
        _struct = ('<2I', 8)
        def __init__(self, tuple):
            (self.hashOffset,
            self.fileCount) = tuple

    class FILE3:
        _struct = ('<2I', 8)
        def __init__(self, tuple):
            (self.fileSize,
            self.fileOffset) = tuple
        @property
        def size(self): return self.fileSize & 0x3FFFFFFF if self.fileSize > 0 else 0

    #endregion

    # read
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        files: list[FileSource]
        magic = source.magic = r.readUInt32()

        # Oblivion - Skyrim
        if magic == self.OB_MAGIC:
            header = r.readS(self.HDR4)
            if header.version != self.OB_VERSION and header.version != self.F3_VERSION and header.version != self.SE_VERSION: raise Exception('BAD MAGIC')
            if (header.archiveFlags & self.FLAG4_PATHNAMES) == 0 or (header.archiveFlags & self.FLAG4_FILENAMES) == 0: raise Exception('HEADER FLAGS')
            source.version = header.version

            # calculate some useful values
            compressedToggle = (header.archiveFlags & self.FLAG4_COMPRESSFILES) > 0
            if header.version == self.F3_VERSION or header.version == self.SE_VERSION:
                source.tag = (header.archiveFlags & self.FLAG4_PREFIXS) > 0

            # read-all folders
            foldersFiles = \
                [x.fileCount for x in r.readSArray(self.DIR4SE, header.folderCount)] if header.version == self.SE_VERSION else \
                [x.fileCount for x in r.readSArray(self.DIR4, header.folderCount)]

            # read-all folder files
            j = 0
            source.files = files = [None] * header.fileCount
            for i in range(header.folderCount):
                folderName = r.readFAString(r.readByte() - 1).replace('\\', '/')
                r.skip(1)
                headerFiles = r.readSArray(self.FILE4, foldersFiles[i])
                for headerFile in headerFiles:
                    compressed = (headerFile.size & self.FILE4_SIZECOMPRESS) != 0
                    packedSize = headerFile.size ^ self.FILE4_SIZECOMPRESS if compressed else headerFile.size
                    files[j] = FileSource(
                        path = folderName,
                        offset = headerFile.offset,
                        compressed = 1 if compressed ^ compressedToggle else 0,
                        packedSize = packedSize,
                        fileSize = packedSize & self.FILE4_SIZEMASK if source.version == self.SE_VERSION else packedSize)
                    j += 1

            # read-all names
            for file in files: file.path = f'{file.path}/{r.readVWString()}'

        # Morrowind
        elif magic == self.MW_MAGIC:
            header = r.readS(self.HDR3)
            dataOffset = 12 + header.hashOffset + (header.fileCount << 3)

            # create filesources
            source.files = files = [None] * header.fileCount
            headerFiles = r.readSArray(self.FILE3, header.fileCount)
            for i in range(header.fileCount):
                headerFile = headerFiles[i]
                size = headerFile.size
                files[i] = FileSource(
                    offset = dataOffset + headerFile.fileOffset,
                    compressed = 0,
                    fileSize = size,
                    packedSize = size)

            # read filename offsets
            filenameOffsets = r.readPArray(None, 'I', header.fileCount) # relative offset in filenames section

            # read filenames
            filenamesPosition = r.tell()
            for i in range(header.fileCount):
                r.seek(filenamesPosition + filenameOffsets[i])
                files[i].path = r.readVAString(1000).replace('\\', '/')
        else: raise Exception('BAD MAGIC')
    
    # readData
    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        # position
        fileSize = file.fileSize
        r.seek(file.offset)
        if source.tag:
            prefixLength = r.readByte() + 1
            if source.version == self.SE_VERSION: fileSize -= prefixLength
            r.seek(file.offset + prefixLength)

        # not compressed
        if fileSize <= 0 or file.compressed == 0:
            return BytesIO(r.readBytes(fileSize))

        # compressed
        newFileSize = r.readUInt32(); fileSize -= 4
        return BytesIO(
            decompressLz4(r, fileSize, newFileSize) if source.version == self.SE_VERSION else \
            decompressZlib(r, fileSize, newFileSize))

#endregion - end::Binary_Bsa[]

#region Binary_Esm - tag::Binary_Esm[]

# Binary_Esm
class Binary_Esm(ArcBinaryT, IDatabase):
    format: FormType
    record: Record
    groups: dict[FormType, RecordGroup]

    @staticmethod
    def getFormat(game: str) -> FormType:
        match game:
            # tes
            case 'Morrowind': return FormType.TES3
            case 'Oblivion' | 'Oblivion:R': return FormType.TES4
            case 'Skyrim' | 'SkyrimSE' | 'SkyrimVR': return FormType.TES5
            # fallout
            case 'Fallout3' | 'FalloutNV': return FormType.TES4
            case 'Fallout4' | 'Fallout4VR': return FormType.TES5
            case 'Fallout76' : return FormType.TES5
            # starfield
            case 'Starfield': return FormType.TES6
            case _: raise Exception(f'Unknown: {game}')

    # read
    def read(self, source: BinaryArchive, b: BinaryReader, tag: object = None) -> None:
        self.format = self.getFormat(source.game.id)
        r = Reader(b, source.binPath, self.format, source.game.id in ['Fallout3', 'FalloutNV'])
        record = self.record = Record.factory(r, FormType(r.readUInt32()))
        record.readFields(r)
        files = source.files = [FileSource(path = f'{str(record.type)[9:]}', tag = record)]
        for s in RecordGroup.readAll(r):
            if s.preload: print(f'read: {s}'); s.read(r, files)
            else: print(f'skip: {s}'); r.seek(r.tell() + s.dataSize)
        print('HERE')
        # if r.format == FormType.TES3:
        #     for k, g in groupby(sorted(self.records, key=lambda s: int(s.type)), lambda s: s.type):
        #         t = RecordGroup(None); t.label = k; t.records = list(g)
        #         groups[k] = t

    def process(self, source: BinaryArchive) -> None:
        if self.format == FormType.TES3:
            pass
        pass

    #region Query

    @staticmethod
    def findTAGFactory(type: FormType, group: RecordGroup) -> object: return None #Binary_Esm.FindTAG[Record](group.records) #z = Record.factory(None, type);
    class FindTAG[T](list, IHaveMetaInfo, IWriteToStream):
        def __init__(self, source: list): super().__init__(source)
        def writeToStream(self, stream: object): return DesSer.serialize(self, stream)
        def __repr__(self): return DesSer.serialize(self)

        # IHaveMetaInfo
        def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
            MetaInfo(None, MetaContent(type = 'Data', name = os.path.basename(file.path), value = self))
        ]
    class FindLTEX:
        def __init__(self, index: int): self.index = index
        def tes3(self, _: 'Binary_Esm') -> object: return None #z if _.LTEXsById.TryGetValue(index, out var z) else None
    class FindLAND:
        def __init__(self, cell: Int3): self.cell = cell
        def tes3(self, _: 'Binary_Esm') -> object: return None #z if _.LANDsById.TryGetValue(cell, out var z) else None
        def else_(self, _: 'Binary_Esm') -> object:
            # world = _.WRLDsById[(uint)cell.Z]
            # foreach (var wrld in world.Item2)
            #     foreach (var cellBlock in wrld.EnsureWrldAndCell(cell))
            #         if (cellBlock.LANDsById.TryGetValue(cell, out var z)) return z;
            return None
    class FindCELL:
        def __init__(self, cell: Int3): self.cell = cell
        def tes3(self, _: 'Binary_Esm') -> object: return None #z if _.CELLsById.TryGetValue(cell, out var z) else None
        def else_(self, _: 'Binary_Esm') -> object:
            # var world = _.WRLDsById[(uint)cell.Z];
            # foreach (var wrld in world.Item2)
            #     foreach (var cellBlock in wrld.EnsureWrldAndCell(cell))
            #         if (cellBlock.CELLsById.TryGetValue(cell, out var z)) return z;
            return None
    class FindCELLByName:
        def __init__(self, name: str): self.name = name
        def tes3(self, _: 'Binary_Esm') -> object: return None #z if _.CELLsByName.TryGetValue(cell, out var z) else None
    def query(self, s: object) -> object:
        match s:
            case FileSource(): return self.findTAGFactory(s.flags, s.tag)
            case FindLTEX(): return s.tes3(self) if self.format == FormType.TES3 else _throw('NotImplementedError')
            case FindLAND(): return s.tes3(self) if self.format == FormType.TES3 else s.else_(self)
            case FindCELL(): return s.tes3(self) if self.format == FormType.TES3 else s.else_(self)
            case FindCELLByName(): return s.tes3(self) if self.format == FormType.TES3 else _throw('NotImplementedError')
            case _: return _throw('OutOfRange')

    #endregion
    
#endregion - end::Binary_Esm[]
