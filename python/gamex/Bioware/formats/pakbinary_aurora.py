import os
from io import BytesIO
from gamex.pak import PakBinaryT
from gamex.meta import PakBinaryT

# typedefs
class Reader: pass
class BinaryPakFile: pass

# PakBinary_Aurora
class PakBinary_Aurora(PakBinaryT):

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        raise Exception('BAD MAGIC')
