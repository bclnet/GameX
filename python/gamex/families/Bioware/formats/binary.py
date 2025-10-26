import os
from io import BytesIO
from gamex.core.pak import PakBinaryT
from gamex.core.meta import FileSource
from ....resources.Bioware import TOR #, WAR

# typedefs
class Reader: pass
class BinaryPakFile: pass

# Binary_Aurora
class Binary_Aurora(PakBinaryT):

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        raise Exception('BAD MAGIC')


# Binary_Myp
class Binary_Myp(PakBinaryT):

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
            if self.magic != self.MYP_MAGIC: raise Exception('Not a .tor file (Wrong file header)')
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
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        files = source.files = []
        match source.game.id:
            case 'SWTOR': hashLookup = TOR.hashLookup
            case 'WAR': hashLookup = WAR.hashLookup
            case _: hashLookup = []

        header = r.readS(MYP_Header)
        header.verify()
        source.version = header.version

        hashLookup = RE.getHashLookup(f'{source.game.resource}.list') if source.game.resource else None