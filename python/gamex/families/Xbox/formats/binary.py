from __future__ import annotations
import os, itertools
from io import BytesIO
from openstk import _throw, _pathExtension, Reader, IWriteToStream
from gamex import Archive, BinaryArchive, ArcBinary, ArcBinaryT, FileSource, MetaInfo, MetaManager, MetaContent, IHaveMetaInfo, DesSer
from gamex.families.Uncore.formats.compression import decompressXbox
from gamex.families.Xbox.formats.xna import ContentReader

# types
type Vector2 = ndarray
type Vector3 = ndarray
type Vector4 = ndarray
type Matrix4x4 = ndarray
type Quaternion = ndarray

#region Binary_Xnb

# Binary_Xnb
class Binary_Xnb(IHaveMetaInfo, IWriteToStream):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: Archive): return Binary_Xnb(r, f)

    #region Headers

    MAGIC = 0x00424e58 #: XNB?

    class Header:
        _struct = ('<I2bI', 10)
        def __init__(self, tuple):
            self.magic, \
            self.version, \
            self.flags, \
            self.sizeOnDisk = tuple
        @property
        def compressed(self) -> bool: return (self.flags & 0x80) != 0
        @property
        def platform(self) -> chr: return chr(self.magic >> 24)

        def validate(self, r: Reader, path: str):
            if (self.magic & 0x00FFFFFF) != Binary_Xnb.MAGIC: raise Exception('BAD MAGIC')
            if self.version != 5 and self.version != 4: raise Exception('Invalid XNB version')
            if self.sizeOnDisk > r.f.getbuffer().nbytes: raise Exception('XNB file has been truncated.')
            if self.compressed:
                decompressedSize = r.readUInt32(); compressedSize = self.sizeOnDisk - r.tell()
                b = decompressXbox(r, compressedSize, decompressedSize)
                return ContentReader(BytesIO(b), path, self.version, None)
            return ContentReader(r.f, path, self.version, None)

    #endregion

    def __init__(self, r2: Reader, f: FileSource):
        h = r2.readS(self.Header)
        r = h.validate(r2, f.path)
        self.obj = r.readAsset()
        self.atEnd = r.atEnd()
        # r.ensureAtEnd()

    # @staticmethod
    # def addX(reader: Binary_Xnb.TypeReader) -> Binary_Xnb.TypeReader: pass
        
    def writeToStream(self, stream: object): return DesSer.serialize(self.obj, stream)

    def __repr__(self): return DesSer.serialize(self.obj)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return self.obj.getInfoNodes(resource, file, tag) if isinstance(self.obj, IHaveMetaInfo) else [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('Xnb', items = [
            MetaInfo(f'Obj: {self.obj}')
            ])
        ]


#endregion