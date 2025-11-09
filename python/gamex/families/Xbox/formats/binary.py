import os, numpy as np
from io import BytesIO
from openstk import _pathExtension
from openstk import Reader
from gamex import PakFile, BinaryPakFile, PakBinary, PakBinaryT, FileSource, MetaInfo, MetaManager, MetaContent, IHaveMetaInfo

#region Binary_Xnb

# Binary_Xnb
class Binary_Xnb(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Xnb(r)
        
    #region Headers

    MAGIC = 0x00424e58 #: XNB?

    class Header:
        _struct = ('<I2bI', 10)
        def __init__(self, tuple):
            self.magic, \
            self.version, \
            self.flags, \
            self.sizeOnDisk = tuple

    #endregion

    def __init__(self, r: Reader):
        pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        # MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self.data))
        ]

#endregion