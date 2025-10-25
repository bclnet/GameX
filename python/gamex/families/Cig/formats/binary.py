import os
from io import BytesIO
from gamex.core.pak import PakBinaryT, BinaryPakFile
from gamex.core.meta import FileSource

# typedefs
class FamilyGame: pass
class IFileSystem: pass
class Reader: pass
class P4kFile: pass

#region Binary_P4k

# Binary_P4k
class Binary_P4k(PakBinaryT):
    Key: bytearray = bytearray([0x5E, 0x7A, 0x20, 0x02, 0x30, 0x2E, 0xEB, 0x1A, 0x3B, 0xB6, 0x17, 0xC3, 0x0F, 0xDE, 0x1E, 0x47])

    class SubPakFileP4k(BinaryPakFile):
        def __init__(self, pak: P4kFile, source: BinaryPakFile, game: FamilyGame, fileSystem: IFileSystem, filePath: str, tag: object = None):
            super().__init__(game, fileSystem, filePath, parent._instance, tag)
            self.pak = file
            self.objectFactoryFunc = source.objectFactoryFunc
            self.useReader = False
            # self.open()

        def read(self, r: Reader, tag: object = None):
            pass

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        source.useReader = False

#endregion