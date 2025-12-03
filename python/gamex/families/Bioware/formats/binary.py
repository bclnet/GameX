import os
from io import BytesIO
from gamex.core.archive import ArcBinaryT
from gamex.core.meta import FileSource
from gamex.core.formats.compression import decompressZstd, decompressZlib
from ....resources.Bioware import TOR, WAR

# typedefs
class Reader: pass
class BinaryArchive: pass

# Binary_Aurora
class Binary_Aurora(ArcBinaryT):

    # read
    def read(self, source: BinaryArchive, r: Reader, tag: object = None) -> None:
        raise Exception('BAD MAGIC')


# Binary_Myp
class Binary_Myp(ArcBinaryT):

    #region Headers

    MYP_MAGIC = 0x0050594d

    class MYP_Header:
        _struct = ('<3IQ4I', 36)
        def __init__(self, tuple):
            self.magic, \
            self.version, \
            self.bom, \
            self.tableOffset, \
            self.tableCapacity, \
            self.totalFiles, \
            self.unk1, \
            self.unk2 = tuple
        def verify(self):
            if self.magic != Binary_Myp.MYP_MAGIC: raise Exception('Not a .tor file (Wrong file header)')
            if self.version != 5 and self.version != 6: raise Exception(f'Only versions 5 and 6 are supported, file has {self.version}')
            if self.bom != 0xfd23ec43: raise Exception('Unexpected byte order')
            if self.tableOffset == 0: raise Exception('File is empty')

    class MYP_HeaderFile:
        _struct = ('<Q3IQIH', 34)
        def __init__(self, tuple):
            self.offset, \
            self.headerSize, \
            self.packedSize, \
            self.fileSize, \
            self.digest, \
            self.crc, \
            self.compressed = tuple

    #endregion

    # read
    def read(self, source: BinaryArchive, r: Reader, tag: object = None) -> None:
        files = source.files = []
        match source.game.id:
            case 'SWTOR': hashLookup = TOR.hashLookup
            case 'WAR': hashLookup = WAR.hashLookup
            case _: hashLookup = {}

        header = r.readS(self.MYP_Header)
        header.verify()
        source.version = header.version

        tableOffset = header.tableOffset
        while tableOffset != 0:
            r.seek(tableOffset)

            numFiles = r.readInt32()
            if numFiles == 0: break
            tableOffset = r.readInt64()

            headerFiles = r.readSArray(self.MYP_HeaderFile, numFiles)
            for i in range(numFiles):
                headerFile = headerFiles[i]
                if headerFile.offset == 0: continue
                hash = headerFile.digest
                print(hash)
                exit(0)
                path = hashLookup[hash].replace('\\', '/') if hash in hashLookup else f'{hash:02X}.bin'
                files.append(FileSource(
                    id = i,
                    path = path[1:] if path.startswith('/') else path,
                    fileSize = headerFile.fileSize,
                    packedSize = headerFile.packedSize,
                    offset = headerFile.offset + headerFile.headerSize,
                    hash = hash,
                    compressed = headerFile.compressed))

    # readData
    def readData(self, source: BinaryArchive, r: Reader, file: FileSource, option: object = None) -> BytesIO:
        if file.fileSize == 0: return BytesIO()
        r.seek(file.offset)
        return BytesIO(
                r.readBytes(file.fileSize) if file.compressed == 0 else \
                decompressZstd(r, file.packedSize, file.fileSize) if source.version == 6 else \
                decompressZlib(r, file.packedSize, file.fileSize))