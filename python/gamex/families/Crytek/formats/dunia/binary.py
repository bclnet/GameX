import io, os
from enum import Enum
from openstk.core import log, BinaryReader
from gamex import FileSource, Archive, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo
from gamex.families.Uncore.formats.binary import Binary_Dds

# typedefs
class BinaryArchive: pass

# Binary_Fcb
class Binary_Fcb(IHaveMetaInfo):
    class Object:
        def __init__(self):
            self.typeHash = 0
            self.values = {}
            self.children = []
        @staticmethod
        def deserialize(r: BinaryReader, pointers: list['Object']) -> 'Object':
            (v, o) = r.readUIntV8a2()
            if o: return pointers[v]
            child = Binary_Fcb.Object()
            pointers.append(child)
            child._deserialize(r, v, pointers)
            return child
        def _deserialize(self, r: BinaryReader, childCount: int, pointers: list['Object']) -> None:
            self.typeHash = r.readUInt32()
            (count, c) = r.readUIntV8a2()
            if c: raise Exception('Not Implemented')
            # position; value
            for i in range(count):
                nameHash = r.readUInt32()
                position = r.tell()
                (v, o) = r.readUIntV8a2()
                if o:
                    r.seek(position - v)
                    (v, o) = r.readUIntV8a2()
                    if o: raise Exception()
                    value = r.readBytes(v)
                    r.seek(position)
                    r.readUIntV8a2()
                else: value = r.readBytes(v)
                self.values[nameHash] = value
            for i in range(childCount): self.children.append(Binary_Fcb.Object.deserialize(r, pointers))

    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Fcb(r, s)

    def __init__(self, r: BinaryReader, s: Archive):
        magic = r.readUInt32()
        if magic != 0x4643626E: raise Exception('BAD MAGIC') # FCbn
        version = r.readUInt16()
        if version != 2: raise Exception()
        self.flags = r.readUInt16()
        if self.flags != 0: raise Exception()

        # get hashes

        # read
        totalObjectCount = r.readUInt32()
        totalValueCount = r.readUInt32()
        self.root = Binary_Fcb.Object.deserialize(r, [])

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self))
    ]

# Binary_Xbt
class Binary_Xbt(Binary_Dds):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Xbt(r, f)

    def __init__(self, r: BinaryReader, f: FileSource): super().__init__(Binary_Xbt.pre(r), f)

    @staticmethod
    def pre(r: BinaryReader) -> BinaryReader:
        magic = r.readUInt32() << 8
        if magic != 0x58425400: raise Exception('BAD MAGIC')
        r.seek(r.skip(4).readUInt32())
        return r