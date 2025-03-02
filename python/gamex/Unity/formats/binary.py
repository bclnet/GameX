import os
from io import BytesIO
from gamex.pak import PakBinaryT
from gamex.meta import FileSource

# typedefs
class Reader: pass
class BinaryPakFile: pass

#region Binary_Unity

# Binary_Unity
class Binary_Unity(PakBinaryT):

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        raise NotImplementedError()

#endregion