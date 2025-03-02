import os
from io import BytesIO
from gamex import FileSource, FileOption, PakBinaryT
from gamex.compression import decompressBlast
from gamex.util import _pathExtension

# typedefs
class Reader: pass
class BinaryPakFile: pass

#region Binary_Danae

# Binary_Danae
class Binary_Danae(PakBinaryT):
    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        source.files = files = []
        key = source.game.key.encode('ascii'); keyLength = len(key); keyIndex = 0

        # move to fat table
        r.seek(r.readUInt32())
        fatSize = r.readUInt32()
        fatBytes = bytearray(r.readBytes(fatSize)); b = 0

        # read int32
        def readInt32() -> int:
            nonlocal b, keyIndex
            p = b
            fatBytes[p + 0] = fatBytes[p + 0] ^ key[keyIndex]; keyIndex += 1
            if keyIndex >= keyLength: keyIndex = 0
            fatBytes[p + 1] = fatBytes[p + 1] ^ key[keyIndex]; keyIndex += 1
            if keyIndex >= keyLength: keyIndex = 0
            fatBytes[p + 2] = fatBytes[p + 2] ^ key[keyIndex]; keyIndex += 1
            if keyIndex >= keyLength: keyIndex = 0
            fatBytes[p + 3] = fatBytes[p + 3] ^ key[keyIndex]; keyIndex += 1
            if keyIndex >= keyLength: keyIndex = 0
            b += 4
            return int.from_bytes(fatBytes[p:p+4], 'little', signed=True)

        # read string
        def readString() -> str:
            nonlocal b, keyIndex
            p = b
            while True:
                fatBytes[p] = fatBytes[p] ^ key[keyIndex]; keyIndex += 1
                if keyIndex >= keyLength: keyIndex = 0
                if fatBytes[p] == 0: break
                p += 1
            length = p - b
            r = fatBytes[b:p].decode('ascii', 'replace')
            b = p + 1
            return r

        # while there are bytes
        while b < fatSize:
            dirPath = readString().replace('\\', '/')
            numFiles = readInt32()
            for _ in range(numFiles):
                # get file
                file = FileSource(
                    path = dirPath + readString().replace('\\', '/'),
                    offset = readInt32(),
                    compressed = readInt32(),
                    fileSize = readInt32(),
                    packedSize = readInt32()
                    )
                # special case
                if file.path.endswith('.FTL'): file.compressed = 1
                elif file.compressed == 0: file.fileSize = file.packedSize
                # add file
                files.append(file)

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource, option: FileOption = None) -> BytesIO:
        r.seek(file.offset)
        return BytesIO(
            decompressBlast(r, file.packedSize, file.fileSize) if (file.compressed & 1) != 0 else \
            r.readBytes(file.packedSize)
            )

#endregion

#region Binary_Void

# Binary_Void
class Binary_Void(PakBinaryT):

    #region Headers

    class V_File:
        struct = ('>Q4IH', 26)
        def __init__(self, tuple):
            self.offset, \
            self.fileSize, \
            self.packedSize, \
            self.unknown1, \
            self.flags, \
            self.flags2 = tuple

    #endregion

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        # must be .index file
        if _pathExtension(source.filePath) != '.index': raise Exception('must be a .index file')

        files = source.files = []

        # master.index file
        if source.filePath == 'master.index':
            MAGIC = 0x04534552
            SubMarker = 0x18000000
            EndMarker = 0x01000000
            
            magic = r.readUInt32E()
            if magic != MAGIC: raise Exception('BAD MAGIC')
            r.skip(4)
            first = True
            while True:
                pathSize = r.readUInt32()
                if pathSize == SubMarker: first = False; pathSize = r.readUInt32()
                elif pathSize == EndMarker: break
                path = r.readFAString(pathSize).replace('\\', '/')
                packId = 0 if first else r.readUInt16()
                if not path.endswith('.index'): continue
                files.append(FileSource(
                    path = path,
                    pak = self.SubPakFile(self, None, source, source.game, source.fileSystem, path)
                    ))
            return

        # find files
        fileSystem = source.fileSystem
        resourcePath = f'{source.filePath[:-6]}.resources'
        if not fileSystem.fileExists(resourcePath): raise Exception('Unable to find resources extension')
        sharedResourcePath = next((x for x in ['shared_2_3.sharedrsc',
            'shared_2_3_4.sharedrsc',
            'shared_1_2_3.sharedrsc',
            'shared_1_2_3_4.sharedrsc'] if fileSystem.fileExists(x)), None)

        # read
        r.seek(4)
        mainFileSize = r.readUInt32E()
        r.skip(24)
        numFiles = r.readUInt32E()
        files = source.files = []
        for _ in range(numFiles):
            id = r.readUInt32E()
            tag1 = r.readL32Encoding()
            tag2 = r.readL32Encoding()
            path = (r.readL32Encoding() or '').replace('\\', '/')
            file = r.readS(self.V_File)
            useSharedResources = (file.flags & 0x20) != 0 and file.flags2 == 0x8000
            if useSharedResources and not sharedResourcePath: raise Exception('sharedResourcePath not available')
            newPath = sharedResourcePath if useSharedResources else resourcePath
            files.append(FileSource(
                id = id,
                path = path,
                compressed = 1 if file.fileSize != file.packedSize else 0,
                fileSize = file.fileSize,
                packedSize = file.packedSize,
                offset = file.offset,
                tag = (newPath, tag1, tag2)
                ))

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource, option: FileOption = None) -> BytesIO:
        pass

#endregion