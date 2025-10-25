import os
from io import BytesIO
from gamex.core.pak import PakBinaryT
from gamex.core.meta import FileSource

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

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        raise Exception('BAD MAGIC')
