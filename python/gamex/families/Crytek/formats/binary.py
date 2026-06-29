import io, os
# from enum import Enum
# from numpy import ndarray
from openstk.core import log, BinaryReader, StreamIterators, ForwardStream
from gamex import FileSource, ArcBinaryT, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo
from Crypto.Cipher import AES

# types
# type Vector3 = ndarray

# typedefs
class BinaryArchive: pass

# Binary_ArcheAge
class Binary_ArcheAge(ArcBinaryT):
    #region Headers

    MAGIC = 0x4f424957 # Magic for Archeage, the literal string "WIBO".

    # tag::Binary_ArcheAge.HDR[]
    class HDR:
        _struct = ('<8I', 32)
        def __init__(self, t):
            (self.magic, dummy1,
            self.fileCount,
            self.extraFiles, dummy2, dummy3, dummy4, dummy5) = t
    # end::Binary_Ba2.HDR[]

    #endregion

    def __init__(self, key: bytes = None):
        self.key = key

    # read - tag::Binary_ArcheAge.read[]
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        fs = r.f; fsLength = r.length
        aes = lambda: AES.new(self.key, AES.MODE_CBC, bytes(16))
        r = BinaryReader(ForwardStream(StreamIterators.streamCipherIter(fs, aes())))
        fs.seek(fsLength - 0x200, 0)

        hdr = r.readS(self.HDR)
        if hdr.magic > self.MAGIC: raise Exception('BAD MAGIC')

        totalSize = (hdr.fileCount + hdr.extraFiles) * 0x150
        infoOffset = fsLength - 0x200 - totalSize
        while infoOffset >= 0:
            if (infoOffset % 0x200) != 0: infoOffset -= 0x10
            else: break

        # read-all files
        source.files = files = [None] * hdr.fileCount
        for i in range(hdr.fileCount):
            fs.seek(infoOffset, 0)
            r = BinaryReader(ForwardStream(StreamIterators.streamCipherIter(fs, aes())))
            files[i] = FileSource(
                path = r.readFAString(0x108), #: name //.Replace('\\', '/')
                offset = r.readInt64(),       #: offset
                fileSize = r.readInt64(),     #: size
                packedSize = r.readInt64(),   #: xsize
                compressed = r.readInt32())   #: ysize
            infoOffset += 0x150
    # end::Binary_ArcheAge.read[]

    # readData - tag::Binary_ArcheAge.readData[]
    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> io.BytesIO:
        r.seek(file.offset)
        return BytesIO(r.readBytes(file.fileSize))
    # end::Binary_ArcheAge.readData[]

# Binary_Cry3
class Binary_Cry3(ArcBinaryT):
    pass
  