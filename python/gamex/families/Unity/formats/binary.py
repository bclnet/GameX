import os
from io import BytesIO
from gamex.core.archive import ArcBinaryT
from gamex.core.meta import FileSource

# typedefs
class Reader: pass
class BinaryArchive: pass

#region Binary_Unity

# Binary_Unity
class Binary_Unity(ArcBinaryT):

    # read
    def read(self, source: BinaryArchive, r: Reader, tag: object = None) -> None:
        raise NotImplementedError()

#endregion