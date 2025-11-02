import os
from io import BytesIO
from enum import Enum
from datetime import datetime
from openstk import Reader, unsafe
from gamex import FileSource, PakBinaryT, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo, DesSer
from gamex.core.formats.compression import decompressZlibStream, decompressZlib

# typedefs
class PakFile: pass
class BinaryPakFile: pass

#region Binary_Mpk

class Binary_Crf:
    pass

#endregion

#region Binary_Mpk

# Binary_Mpk
class Binary_Mpk(PakBinaryT):

    #region Headers

    MAGIC = 0x4b41504d

    class MPK_File:
        _struct = ('<256sQ5I', 284)
        def __init__(self, tuple):
            self.fileName, \
            self.createdDate, \
            self.unknown1, \
            self.fileSize, \
            self.offset, \
            self.packedSize, \
            self.unknown2 = tuple

    #endregion

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        magic = r.readUInt32()
        if magic != self.MAGIC: raise Exception('BAD MAGIC')
        r.seek(21)
        source.tag = decompressZlibStream(r).decode('ascii') # tag: ArchiveName
        files = decompressZlibStream(r); baseOffset = r.tell()
        s = Reader(BytesIO(files))
        source.files = [FileSource(
            path = unsafe.fixedAStringScan(f.fileName, 256),
            offset = baseOffset + f.offset,
            packedSize = f.packedSize,
            fileSize = f.fileSize,
            date = datetime.fromtimestamp(f.createdDate)
        ) for f in s.readSArray(self.MPK_File, len(files) // 284)]

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource, option: object = None) -> BytesIO:
        r.seek(file.offset)
        return BytesIO(decompressZlib(r, file.packedSize, file.fileSize))

#endregion
