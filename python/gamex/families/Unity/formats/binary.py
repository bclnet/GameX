import os
from io import BytesIO
from gamex.core.binary import ArcBinaryT
from gamex.core.meta import FileSource

# typedefs
class BinaryReader: pass
class BinaryArchive: pass

#region Binary_Unity

# Binary_Unity
class Binary_Unity(ArcBinaryT):

    # read
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        raise NotImplementedError('Binary_Unity')

#endregion