import os
from io import BytesIO
from enum import Enum
from datetime import datetime
from openstk import BinaryReader, unsafe
from gamex import FileSource, ArcBinaryT, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo, DesSer
from gamex.families.Uncore.formats.compression import decompressZlibStream, decompressZlib

# typedefs
class Archive: pass
class BinaryArchive: pass

#region Binary_Mpk

class Binary_Crf:
    pass

#endregion

#region Binary_Mpk

# Binary_Mpk
class Binary_Mpk(ArcBinaryT):

    #region Headers

    MAGIC = 0x4b41504d

    class MPK_File:
        _struct = ('<256sQ5I', 284)
        def __init__(self, t):
            (self.fileName,
            self.createdDate,
            self.unknown1,
            self.fileSize,
            self.offset,
            self.packedSize,
            self.unknown2) = t

    #endregion

    # read
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        magic = r.readUInt32()
        if magic != self.MAGIC: raise Exception('BAD MAGIC')
        r.seek(21)
        source.tag = decompressZlibStream(r).decode('ascii') # tag: ArchiveName
        files = decompressZlibStream(r); baseOffset = r.tell()
        s = BinaryReader(BytesIO(files))
        source.files = [FileSource(
            path = unsafe.fixedAStringScan(f.fileName, 256),
            offset = baseOffset + f.offset,
            packedSize = f.packedSize,
            fileSize = f.fileSize,
            date = datetime.fromtimestamp(f.createdDate)
        ) for f in s.readSArray(self.MPK_File, len(files) // 284)]

    # readData
    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        r.seek(file.offset)
        return BytesIO(decompressZlib(r, file.packedSize, file.fileSize))

#endregion
