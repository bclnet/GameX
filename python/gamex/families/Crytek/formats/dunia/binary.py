from __future__ import annotations
import io, os
from enum import Enum
from openstk.core import log, BinaryReader, IWriteToStream
from gamex import FileSource, ArcBinaryT, Archive, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo, DesSer
from gamex.families.Uncore.formats.binary import Binary_Dds

# typedefs
class BinaryArchive: pass

# Binary_Fcb
class Binary_Fcb(IHaveMetaInfo, IWriteToStream):
    class Object:
        def __init__(self, id: int):
            self._id = id
            self.typeHash = 0
            self.values = {}
            self.children = []
        @staticmethod
        def deserialize(r: BinaryReader, pointers: list['Object'], defx: 'Definition') -> 'Object':
            id = r.tell()
            (v, o) = r.readUIntV8a2()
            if o: return pointers[v]
            child = Binary_Fcb.Object(id=id)
            pointers.append(child)
            child._deserialize(r, v, pointers, defx)
            return child
        def _deserialize(self, r: BinaryReader, childCount: int, pointers: list['Object'], defx: 'Definition') -> None:
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
            for i in range(childCount): self.children.append(Binary_Fcb.Object.deserialize(r, pointers, defx))

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
        match s.game.id:
            case 'FarCry3' | 'FarCry3:BD' | 'FarCry4':
                from .....resources.Crytek import FarCry3
                defx = FarCry3.getObjDef('binary_classes.xml')
            case _:
                from .....resources.Crytek import FarCry2
                defx = FarCry2.getObjDef('binary_classes.xml')
        # read
        totalObjectCount = r.readUInt32()
        totalValueCount = r.readUInt32()
        self.root = Binary_Fcb.Object.deserialize(r, [], defx)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self))
    ]

    def writeToStream(self, stream: object): return DesSer.serialize(self, stream)
    def __repr__(self): return DesSer.serialize(self)

# Binary_Xbg
class Binary_Xbg(IHaveMetaInfo): #IBlockFactory
    #region Blocks

    class BlockType(Enum):
        Root = 0x00000000
        MaterialReference = 0x524D544C # RMTL
        Nodes = 0x4E4F4445
        O2BM = 0x4F32424D
        SKID = 0x534B4944
        SKND = 0x534B4E44
        CLUS = 0x434C5553
        LODs = 0x04C4F4453 # LODS
        BoundingBox = 0x42424F58 # BBOX
        BSPH = 0x42535048
        LODInfo = 0x004C4F44 # LOD\0
        PCMP = 0x50434D50
        UCMP = 0x55434D50
        IKDA = 0x494B4441
        MaterialDescriptor = 0x444D544C # DMTL

    class IBlockFactory:
        def createBlock(self, type: BlockType) -> Block: pass

    class Block(IBlockFactory):
        def __init__(self, type: BlockType): self.type = type
        def deserialize(self, r: BinaryReader, parent: Block) -> None: pass
        def createBlock(self, type: BlockType) -> Block: return None
        def addChild(self, child: Block) -> None: raise Exception('Not Implemented')
        def getChildren(self) -> list[Block]: raise Exception('Not Implemented')

    class BoundingBox(Block): 
        # min: Vector3 = None
        # max: Vector3 = None
        def __init__(self): super().__init__(BlockType.BoundingBox)

        def deserialize(self, r: BinaryReader, parent: Block) -> None:
            self.min = r.readVector3()
            self.max = r.readVector3()

    class BSPH(Block): 
        def __init__(self): super().__init__(BlockType.BSPH)

        def deserialize(self, r: BinaryReader, parent: Block) -> None:
            self.x = r.readSingle()
            self.y = r.readSingle()
            self.z = r.readSingle()
            self.w = r.readSingle()

    class CLUS(Block): 
        class UnknownData0:
            def __init__(self, r: BinaryReader):
                self.unknown0 = r.readBytes(108)
                self.unknown1 = r.readUInt16()

        def __init__(self): super().__init__(BlockType.CLUS)

        def deserialize(self, r: BinaryReader, parent: Block) -> None:
            self.unknown0 = r.readFArray(lambda z: r.readL32FArray(lambda z2: UnknownData0(r)), len(parent.unknown0))

    class IKDA(Block): 
        def __init__(self): super().__init__(BlockType.IKDA)

        def deserialize(self, r: BinaryReader, parent: Block) -> None:
            self.unknown = r.readL32FArray(lambda z: r.readBytes(52))

    class LODInfo(Block): 
        def __init__(self): super().__init__(BlockType.LODInfo)

        def deserialize(self, r: BinaryReader, parent: Block) -> None:
            self.count = r.readUInt32()
            self.unknown1 = r.readUInt32()

    class LODs(Block): 
        class LevelOfDetail:
            def __init__(self, r: BinaryReader):
                self.unknown0 = r.readSingle()
                self.buffers = r.readL32FArray(lambda z: Buffer(r))
                self.primitives = r.readL32FArray(lambda z: Primitive(r))
                vertexDataSize = r.readUInt32(); self.vertexData = r.align(16).readBytes(vertexDataSize) # data is aligned to 16 bytes, ugh
                indexCount = r.readUInt32(); self.indices = r.align(16).ReadPArray('h', indexCount) # data is aligned to 16 bytes, ugh

        class Buffer:
            def __init__(self, r: BinaryReader):
                self.format = r.readUInt32()
                self.size = r.readUInt32()
                self.count = r.readUInt32()
                self.offset = r.readUInt32()

        class Primitive:
            def __init__(self, r: BinaryReader):
                self.bufferIndex = r.readInt32()
                self.skeletonIndex = r.readInt32()
                self.materialIndex = r.readInt32()
                self.indicesStartIndex = r.readInt32()
                self.unknown4 = r.readUInt32()
                self.unknown5 = r.readUInt32()
                self.unknown6 = r.readUInt32()

        def __init__(self): super().__init__(BlockType.LODs)

        def deserialize(self, r: BinaryReader, parent: Block) -> None:
            self.items = r.readL32FArray(lambda z: LevelOfDetail(r))

    class MaterialDescriptor(Block): 
        def __init__(self): super().__init__(BlockType.MaterialDescriptor)

        def deserialize(self, r: BinaryReader, parent: Block) -> None:
            self.name = (r.readL32UString(), r.readByte())[0]
            self.unknown1 = (r.readL32UString(), r.readByte())[0]
            self.unknown2 = (r.readL32UString(), r.readByte())[0]
            self.textureProperties = r.readL32FMany(lambda z: r.skipAfter(r.readL32UString(), 1), lambda z: r.skipAfter(r.readL32UString(), 1))
            self.float1Properties = r.readL32FMany(lambda z: r.skipAfter(r.readL32UString(), 1), lambda z: r.readSingle())
            self.float2Properties = r.readL32FMany(lambda z: r.skipAfter(r.readL32UString(), 1), lambda z: Float2(r.readSingle(), r.readSingle()))
            self.float3Properties = r.readL32FMany(lambda z: r.skipAfter(r.readL32UString(), 1), lambda z: Float3(r.readSingle(), r.readSingle(), r.readSingle()))
            self.float4Properties = r.readL32FMany(lambda z: r.skipAfter(r.readL32UString(), 1), lambda z: Float4(r.readSingle(), r.readSingle(), r.readSingle(), r.readSingle()))
            self.intProperties = r.readL32FMany(lambda z: r.skipAfter(r.readL32UString(), 1), lambda z: r.readInt32())
            self.boolProperties = r.readL32FMany(lambda z: r.skipAfter(r.readL32UString(), 1), lambda z: r.readBoolean32())

    class MaterialReference(Block): 
        def __init__(self): super().__init__(BlockType.MaterialReference)

        def deserialize(self, r: BinaryReader, parent: Block) -> None:
            count = r.readUInt32()
            self.unknown00 = r.readUInt32()
            self.paths = r.readFArray(lambda z: r.skipAfter(r.readL32UString(), 1), count)

    class Nodes(Block): 
        class Node:
            def __init__(self, r: BinaryReader):
                self.nameHash = r.readUInt32()
                self.nextSiblingIndex = r.readInt32()
                self.firstChildIndex = r.readInt32()
                self.previousSiblingIndex = r.readInt32()
                self.unknown10 = r.readSingle()
                self.unknown14 = r.readSingle()
                self.unknown18 = r.readSingle()
                self.unknown1C = r.readSingle()
                self.unknown20 = r.readSingle()
                self.unknown24 = r.readSingle()
                self.unknown28 = r.readSingle()
                self.unknown2C = r.readSingle()
                self.unknown30 = r.readSingle()
                self.unknown34 = r.readSingle()
                self.o2bmIndex = r.readInt32()
                self.unknown3C = r.readSingle()
                self.unknown40 = r.readSingle()
                self.name = r.skipAfter(r.readL32UString(), 1)
            def __repr__(self): return self.name or 'base'

        def __init__(self): super().__init__(BlockType.Nodes)

        def deserialize(self, r: BinaryReader, parent: Block) -> None:
            self.items = r.readL32FArray(lambda z: Node(r))

    class O2BM(Block): 
        def __init__(self): super().__init__(BlockType.O2BM)

        def deserialize(self, r: BinaryReader, parent: Block) -> None:
            self.items = r.readL32FArray(lambda z: r.readMatrix4x4())

    class PCMP(Block): 
        def __init__(self): super().__init__(BlockType.PCMP)

        def deserialize(self, r: BinaryReader, parent: Block) -> None:
            self.x = r.readSingle()
            self.y = r.readSingle()

    class SKID(Block): 
        def __init__(self): super().__init__(BlockType.SKID)

        def deserialize(self, r: BinaryReader, parent: Block) -> None:
            self.unknown = r.readL32FArray(lambda z: r.readBytes(8))

    class SKND(Block): 
        class UnknownData0:
            def __init__(self, r: BinaryReader):
                self.unknown00 = r.readSingle()
                self.unknown04 = r.readSingle()
                self.unknown08 = r.readSingle()
                self.unknown0C = r.readSingle()
                self.unknown10 = r.readSingle()
                self.unknown14 = r.readSingle()
                self.unknown18 = r.readSingle()
                self.unknown1C = r.readSingle()
                self.unknown20 = r.readSingle()
                self.unknown24 = r.readSingle()
                self.unknown28 = r.readSingle()
                self.unknown2C = r.readUInt32()
                self.unknown30 = r.readUInt32()
                self.name = r.skipAfter(r.readL32UString(), 1)

        def __init__(self): super().__init__(BlockType.SKND)

        def deserialize(self, r: BinaryReader, parent: Block) -> None:
            self.unknown0 = r.readL32FArray(lambda z: UnknownData0(r))
        def createBlock(self, type: BlockType) -> Block:
            match type:
                case BlockType.CLUS: return CLUS()
                case _: return None
        def addChild(self, child: Block) -> None: self.unknown1.append(child)
        def getChildren(self)-> list[Block]: return self.unknown1

    class UCMP(Block): 
        def __init__(self): super().__init__(BlockType.UCMP)

        def deserialize(self, r: BinaryReader, parent: Block) -> None:
            self.x = r.readSingle()
            self.y = r.readSingle()

    class RootX(Block): 
        def __init__(self):
            super().__init__(BlockType.Root)
            self.materialDescriptors = []

        def deserialize(self, r: BinaryReader, parent: Block) -> None: pass
        def createBlock(self, type: BlockType) -> Block:
            match type:
                case BlockType.MaterialReference: return MaterialReference()
                case BlockType.Nodes: return Nodes()
                case BlockType.O2BM: return O2BM()
                case BlockType.SKID: return SKID()
                case BlockType.SKND: return SKND()
                case BlockType.LODs: return LODs()
                case BlockType.BoundingBox: return BoundingBox()
                case BlockType.BSPH: return BSPH()
                case BlockType.LODInfo: return LODInfo()
                case BlockType.PCMP: return PCMP()
                case BlockType.UCMP: return UCMP()
                case BlockType.IKDA: return IKDA()
                case BlockType.MaterialDescriptor: return MaterialDescriptor()
                case _: raise Exception('Not Supported')
        @staticmethod
        def setChild(t, child: Block, value) -> None:
            if isinstance(child, t):
                if getattr(): raise Exception('Invalid Operation')
                setattr('')
        @staticmethod
        def getChild(block: list[Block], value) -> None:
            if value: blocks.append(value)
        def addChild(self, child: Block) -> None:
            setChild(child, self.materialReference)
            setChild(child, self.nodes)
            setChild(child, self.O2BM)
            setChild(child, self.SKID)
            setChild(child, self.SKND)
            setChild(child, self.LODs)
            setChild(child, self.BoundingBox)
            setChild(child, self.BSPH)
            setChild(child, self.LOD)
            setChild(child, self.PCMP)
            setChild(child, self.UCMP)
            setChild(child, self.IKDA)
            if isinstance(child, MaterialDescriptor): self.materialDescriptors.append(materialDescriptor)
        
        def getChildren(self)-> list[Block]:
            children = []
            getChild(children, MaterialReference)
            getChild(children, Nodes)
            getChild(children, O2BM)
            getChild(children, SKID)
            getChild(children, SKND)
            getChild(children, LODs)
            getChild(children, BoundingBox)
            getChild(children, BSPH)
            getChild(children, LOD)
            getChild(children, PCMP)
            getChild(children, UCMP)
            getChild(children, IKDA)
            children.AddRange(MaterialDescriptors)
            return children

    #endregion

    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Xbg(r)

    def __init__(self, r: BinaryReader):
        if r.tell() + 32 > r.length: raise Exception('Format')
        if r.readUInt32() != 0x4D455348: raise Exception('BAD MAGIC')
        self.majorVersion = r.readUInt16()
        if self.majorVersion != 42: raise Exception('Format')
        self.minorVersion = r.readUInt16()
        self.unknown08 = r.readUInt32()
        self.root: RootX = Binary_Xbg.deserializeBlock(r, None, self)

    def createBlock(self, type: BlockType) -> Block: return None if type != BlockType.Root else RootX()

    @staticmethod
    def deserializeBlock(r: BinaryReader, parent: Block, factory: IBlockFactory) -> Block:
        baseOffset = r.tell()
        type = Binary_Xbg.BlockType(r.readUInt32())
        block = factory.createBlock(type)
        if not block or block.Type != type: raise Exception('Format')
        unknown04 = r.readUInt32()
        size = r.readUInt32()
        dataSize = r.readUInt32()
        childCount = r.readUInt32()
        if dataSize > size: raise Exception('Format')
        childOffset = r.tell()
        childEnd = childOffset + (size - dataSize - 20)
        blockOffset = childEnd
        blockEnd = blockOffset + dataSize
        if blockEnd != baseOffset + size: raise Exception('Format')
        r.seek(blockOffset)
        block.deserialize(r, parent)
        if not r.atEnd(blockEnd): raise Exception('Format')
        r.seek(childOffset)
        for i in range(childCount): block.addChild(deserializeBlock(r, block, block))
        if not r.atEnd(childEnd): raise Exception('Format')
        r.seek(blockEnd)
        return block

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

# Binary_Xml
class Binary_Xml:
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Xml(r)

    def __init__(self, r: BinaryReader): pass

# Binary_Spk
class Binary_Spk(ArcBinaryT):
    #region Scan

    @staticmethod
    def checkOggChunk(r: BinaryReader) -> tuple[bool, int]:
        b = r.readBytes(4)
        r.skip(-4)
        if b.decode('ascii') != 'OggS': return (False, 0)
        # walk the chain
        first = True
        header = bytes(27)
        fullSize = 0
        while True:
            r.read(header, 0, 27)
            if header[:4].decode('ascii') != 'OggS': break
            headerSize = 27 + header[26]
            segments = r.readBytes(header[26])
            pageSize = 0
            for i in range(header[26]): pageSize += segments[i]
            pageSize += headerSize
            fullSize += pageSize
            if first:
                if (header[5] & 0x02) != 0: first = False
                else: return (False, 0) # found the middle of the stream
            if (header[5] & 0x04) != 0: break
            r.skip(pageSize - headerSize)
        return True, fullSize

    @staticmethod
    def scan(r: BinaryReader, endOffset: int) -> tuple[int, int]:
        BufLen = 65535
        if r.tell() >= endOffset: return (-1, -1)
        buf = bytes(BufLen)
        bytesLeft = endOffset - r.tell()
        startOffset = r.tell()
        fullSize = 0
        # scan
        chunkValid = False; bigEndian = False; variant = 0; fullSize = 0 
        while r.tell() < endOffset:
            #position = r.tell()
            nextRead = BufLen if bytesLeft > BufLen else bytesLeft
            if nextRead == 0: break
            r.read(buf, 0, nextRead)
            bytesLeft -= nextRead
            # scan block
            offsetReset = r.tell()
            for i in range(nextRead):
                chunkStart = startOffset + i
                r.seek(chunkStart)
                if buf[i] == 3 or buf[i] == 5:
                    b = r.readBytes(28)
                    # check the bytes
                    chunkValid = b[9] == 0 and b[10] == 0 and b[11] == 0 and b[18] < 89 and (b[12] == 0 or b[12] == 1) and b[22] < 89 #&& b[23]<5
                    if chunkValid and b[0] == 3:
                        if b[14] != 0 or b[15] != 10: chunkValid = False
                    elif chunkValid and b[0] == 5:
                        if b[14] != 10 or b[15] != 0: chunkValid = False
                    # chunk is valid
                    if chunkValid: r.seek(chunkStart); return (chunkStart - startOffset, fullSize)
                elif buf[i] == 6:
                    b = r.readBytes(36)
                    # check the bytes
                    chunkValid = b[9] == 0 and b[10] == 0 and b[11] == 0 and b[18] < 89 and (b[12] == 0 or b[12] == 1) and b[22] < 89 #*&& b[23]<5
                    if chunkValid and b[0] == 6:
                        if b[14] != 10 or b[15] != 0: chunkValid = False
                    for j in range(28, 36):
                        if b[j] != 0: chunkValid = False; break
                    # chunk is valid
                    if chunkValid: r.seek(chunkStart); return (chunkStart - startOffset, fullSize)
                elif buf[i] == 2:
                    b = r.readBytes(24)
                    # get information
                    fullSize = int.from_bytes(b[8:11], 'little', signed=False)
                    numberLayers = int.from_bytes(b[4:7], 'little', signed=False)
                    # check the bytes
                    chunkValid = b[0] == 2 and b[1] == 0 and b[2] == 0 and b[3] == 0 and b[5] == 0 and b[6] == 0 and b[7] == 0
                    if chunkValid and (fullSize < 64 or fullSize > endOffset - chunkStart): chunkValid = False
                    # walk the blocks
                    if chunkValid:
                        while r.tell() < chunkStart + fullSize - 2:
                            signature = r.skipAfter(r.readUInt32(), 4)
                            totalBytes = 0
                            for j in range(numberLayers): totalBytes += r.readUInt32()
                            if signature != i: chunkValid = False; break
                            if totalBytes >= endOffset - chunkStart or totalBytes < numberLayers * 4 + 8: chunkValid = False; break
                            r.skip(totalBytes)
                    # chunk is valid
                    if chunkValid: r.seek(chunkStart); return (chunkStart - startOffset, fullSize)
                elif buf[i] == 8:
                    b = r.readBytes(48)
                    # check the bytes
                    chunkValid = b[0] == 8 and b[1] == 0 and b[2] == 0 and b[3] == 0 and b[37] == 0 and b[38] == 0 and b[39] == 0 and (b[36] == 4 or b[36] == 6) and b[45] == 0 and b[46] == 0 and b[47] == 0 and (b[44] == 1 or b[44] == 2)
                    # walk the blocks
                    if chunkValid:
                        blockHeader = bytes(52)
                        done = False; foundABlock = False
                        while not r.atEnd():
                            for j in range(b[44]):
                                r.read(blockHeader, 0, 52)
                                if blockHeader[0] != 2 or blockHeader[1] != 0 or blockHeader[2] != 0 or blockHeader[3] != 0: done = True; break
                            r.skip(b[36] * 384 + 2)
                            if done: break
                            foundABlock = True
                        if not foundABlock: chunkValid = False
                    # chunk is valid
                    if chunkValid: r.seek(chunkStart); return (chunkStart - startOffset, fullSize)
                elif buf[i] == 8 or buf[i] == 7:
                    b = r.readBytes(28)
                    # check the characters
                    chunkValid = False; bigEndian = False; variant = 0
                    if (b[0] == 8 or b[0] == 7) and b[1] == 0 and b[3] == 0 and b[9] == 0 and b[10] == 0 and b[11] == 0: chunkValid = True; bigEndian = False; variant = 2 if b[0] == 7 else 0
                    elif chunkStart >= 3:
                        chunkStart -= 3; r.seek(chunkStart) # adjust the chuck size and reread
                        b = r.readBytes(28)
                        if (b[3] == 8 or b[3] == 7) and b[2] == 0 and b[0] == 0 and b[8] == 0 and b[9] == 0 and b[10] == 0: chunkValid = True; bigEndian = True; variant = 2 if b[3] == 7 else 0
                    # get information
                    numberLayers = int.from_bytes(b[8:11], 'big' if bigEndian else 'little', signed=False)
                    numberBuffers = int.from_bytes(b[12:15], 'big' if bigEndian else 'little', signed=False)
                    offsetToHeaders = int.from_bytes(b[16:19], 'big' if bigEndian else 'little', signed=False)
                    headerSkip = int.from_bytes(b[20:23], 'big' if bigEndian else 'little', signed=False)
                    if variant == 2:
                        numberBuffers = offsetToHeaders
                        r.skip(32)
                        headerSkip = r.readUInt32X(bigEndian) if not r.atEnd() else 0
                    elif offsetToHeaders != numberLayers * 4 + 8:
                        variant = 1
                        numberBuffers = offsetToHeaders
                        r.skip(44)
                        headerSkip = r.readUInt32X(bigEndian) if not r.atEnd() else 0
                    # verify the information
                    if headerSkip >= endOffset - chunkStart or headerSkip < numberLayers * 4 or numberLayers == 0 or numberBuffers < 1: chunkValid = False
                    # walk the blocks
                    if chunkValid:
                        r.skip(headerSkip)
                        for j in range(numberBuffers):
                            signature = 0
                            if variant == 0:
                                signature = r.skipAfter(r.readUInt32X(bigEndian), 4)
                            elif variant == 1 or variant == 2:
                                signature = r.skipAfter(r.readUInt32X(bigEndian), 4)
                                if signature != i + 1: chunkValid = False; break
                                signature = r.readUInt32X(bigEndian)
                            totalBytes = 0
                            for k in range(numberLayers): totalBytes += r.readUInt32X(bigEndian)
                            if signature != 3: chunkValid = False; break
                            if totalBytes >= endOffset - chunkStart or totalBytes < numberLayers * 4 + 8: chunkValid = False; break
                            r.skip(totalBytes)
                        fullSize = r.tell() - chunkStart
                    # chunk is valid
                    if chunkValid: r.Seek(chunkStart); return (chunkStart - startOffset, fullSize)
                elif buf[i] == 9:
                    b = r.readBytes(20)
                    # check the bytes
                    chunkValid = False; bigEndian = False
                    if b[0] == 9 and b[1] == 0 and b[2] == 16 and b[3] == 0 and b[4] == 0 and b[5] == 0 and b[6] == 0 and b[7] == 0: chunkValid = True; bigEndian = False
                    elif chunkStart >= 3:
                        chunkStart -= 3; r.seek(chunkStart) # adjust the chuck size and reread
                        b = r.readBytes(20)
                        if b[0] == 0 and b[1] == 16 and b[2] == 0 and b[3] == 9 and b[4] == 0 and b[5] == 0 and b[6] == 0 and b[7] == 0: chunkValid = True; bigEndian = True
                    # get information
                    numberLayers = int.from_bytes(b[8:11], 'big' if bigEndian else 'little', signed=False)
                    numberBuffers = int.from_bytes(b[12:15], 'big' if bigEndian else 'little', signed=False)
                    totalInfoSize = int.from_bytes(b[16:19], 'big' if bigEndian else 'little', signed=False)
                    if numberLayers > 64 or totalInfoSize >= endOffset - chunkStart or numberLayers == 0 or numberBuffers < 1: chunkValid = False
                    # walk the blocks
                    if chunkValid:
                        r.skip(totalInfoSize + (64 - numberLayers * 4))
                        headerSizes = 0
                        for j in range(numberLayers): headerSizes += r.ReadUInt32X(bigEndian)
                        if headerSizes > endOffset - chunkStart: chunkValid = False
                        else:
                            r.skip(headerSizes)
                            for j in range(numberBuffers):
                                signature = r.skipAfter(r.readUInt32X(bigEndian), 4)
                                totalBytes = 0
                                for k in range(numberLayers): totalBytes += r.readUInt32X(bigEndian)
                                if signature != 3: chunkValid = False; break
                                if totalBytes >= endOffset - chunkStart: chunkValid = False; break
                                r.skip(totalBytes)
                            fullSize = r.tell() - chunkStart
                    # chunk is valid
                    if chunkValid: r.seek(chunkStart); return (chunkStart - startOffset, fullSize)
                elif buf[i] == 79:
                    # chunk is valid
                    chunkValid, fullSize = checkOggChunk(r)
                    if chunkValid: r.seek(chunkStart); return (chunkStart - startOffset, fullSize)
                # reset
                r.seek(offsetReset)
                fullSize = 0
        return (-1, -1)
    
    class EUFormat(Enum):
        NULL = 0
        UBI_V3 = 1
        UBI_V5 = 2
        UBI_V6 = 3
        UBI_IV2 = 4
        UBI_IV8 = 5
        UBI_IV9 = 6
        UBI_6OR4 = 7
        UBI_RAW = 8
        PCM = 9
        RAW = PCM,
        OGG = 10

    @staticmethod
    def determineFormat(r: BinaryReader, offset: int, size: int) -> EUFormat:
        r.seek(offset)
        # calculate actual size
        if size < 1: size = r.length - offset
        # read in the signature
        magic = r.readBytes(4)
        type = EUFormat.NULL
        if magic[0] == 3: type = EUFormat.UBI_V3
        elif magic[0] == 5: type = EUFormat.UBI_V5
        elif magic[0] == 6: type = EUFormat.UBI_V6
        elif magic[0] == 2: type = EUFormat.UBI_IV2
        elif magic[0] == 8 and magic[1] == 0 and magic[2] == 0 and magic[3] == 0:
            pass
            # Try a version 8 interleaved stream first
            #CFileDataStream FileStream(input, beginning, size);
            #CInterleavedStream Stream(FileStream);
            #try {
            #    std::vector<unsignedlong> Layers;
            #    Layers.push_back(1);
            #    Stream.SetCurrentLayers(Layers);
            #    // Initialize
            #    if (!Stream.InitializeHeader()) type = EUFormat.UBI_6OR4; // Not a version 8 so must be a 6-Or-4
            #    else {
            #        short Buffer[1024];
            #        long NumberSamples = 1024;
            #        if (Stream.Decode(Buffer, NumberSamples)) type = EUFormat.UBI_IV8;
            #        else type = EUFormat.UBI_6OR4;
            #    }
            #}
            #catch { type = EUFormat.UBI_6OR4; } // Not a version 8 so must be a 6-Or-4
        elif magic[0] == 8 and magic[1] == 0: type = EUFormat.UBI_IV8
        elif magic[0] == 9 and magic[1] == 0: type = EUFormat.UBI_IV9
        elif magic[3] == 9 and magic[2] == 0: type = EUFormat.UBI_IV9
        elif magic[0] == 7 and magic[1] == 0: type = EUFormat.UBI_IV8
        elif magic[3] == 8 and magic[2] == 0: type = EUFormat.UBI_IV8
        elif magic[0] == 'O' and magic[1] == 'g' and magic[2] == 'g' and magic[3] == 'S': type = EUFormat.OGG
        return type

    #endregion

    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Spk(r)

    # read - tag::Binary_Spk.read[]
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        files = source.files = []
        # scan for the first chunk
        endOffset = r.length
        found, sizeRead = scan(r, endOffset)
        # loop, until we could find no more chunks
        bytesRead = 0
        while found != -1:
            offset = r.tell()
            # Seek past the current chunk, saving the current chunk size
            if sizeRead != 0: fileSize = sizeRead; r.skip(fileSize)
            else: fileSize = 0; r.skip(28)
            # Scan for the next chunk
            found, sizeRead = scan(r, endOffset)
            if found != -1: bytesRead += 28 # we already passed the header so we don't find it again
            else: bytesRead = endOffset - offset # Assume it goes to the end of the file
            # make sure the chunk has some reasonable size
            if fileSize == 0 and bytesRead < 48:
                # Assume it is a simple chunk and skip to the next file; this one is too small
                # The next file cannot start at the next byte
                r.skip(29)
                found, sizeRead = scan(r, endOffset)
                continue
            # Set some variables
            if fileSize == 0: fileSize = bytesRead
            # add
            format = determineFormat(r, offset, fileSize)
            files.append(FileSource(
                path = f'Sample{len(files)}.{format}',
                fileSize = fileSize,
                offset = offset,
                tag = format))
    # end::Binary_Spk.read[]

    # readData - tag::Binary_Spk.readData[]
    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> io.BytesIO:
        r.seek(file.offset)
        return io.BytesIO(r.readBytes(file.fileSize))
    # end::Binary_Dunia.readData[]

class Binary_Map(IHaveMetaInfo):
    #region Map

    class Size(Enum):
        Small = 0
        Medium = 1
        Large = 2
        ExtraLarge = 3

    class Players(Enum):
        TwoToFour = 0
        FourToEight = 1
        EightToTwelve = 2
        TwelveToSixteen = 3

    class InfoX:
        def __init__(self, r: BinaryReader):
            self.unknown2 = r.readUInt32()
            self.unknown3 = r.readUInt32()
            self.unknown4 = r.readUInt32()
            self.unknown5 = r.readUInt64()
            self.creator = r.readL32UString()
            self.unknown7 = r.readUInt64()
            self.author = r.readL32UString()
            self.name = r.readL32UString()
            self.unknown10 = r.readUInt64()
            self.unknown11 = r.readBytes(36)
            self.unknown12 = r.readBytes(36)
            self.size = Size(r.readUInt32())
            self.players = Players(r.readUInt32())
            self.unknown15 = r.readUInt32()

    class SnapshotX:
        def __init__(self, r: BinaryReader):
            self.width = r.readUInt32()
            self.height = r.readUInt32()
            self.bytesPerPixel = r.readUInt32()
            self.unknown4 = r.readUInt32()
            self.data = r.readBytes(self.unknown4 * self.bytesPerPixel * self.height * self.width // 8)
            self.unknown5 = r.readL32FArray(lambda z: r.readL32UString())

    class DataX:
        def __init__(self, r: BinaryReader):
            self.unknown1 = r.ReadL32UString()
            self.unknown2 = SnapshotX(r)
            self.unknown3 = r.readL32FArray(lambda z: r.readL32UString())

    class Block:
        def __init__(self, virtualOffset: int, fileOffset: int):
            self.virtualOffset = virtualOffset
            self.fileOffset = fileOffset & 0x7FFFFFFF
            self.isCompressed = (fileOffset & 0x80000000) != 0

    class CompressedData:
        def __init__(self, r: BinaryReader):
            offset = r.readUInt32()
            length = offset - 4
            self.data = bytes(length)
            if r.read(self.data, 0, len(self.data)) != len(self.data): raise Exception('Format')
            self.blocks = r.readL32FArray(lambda z: Block(r.readUInt32(), r.readUInt32()))
            if len(self.blocks) == 0 or self.blocks[0].fileOffset != 4 or self.blocks[-1].fileOffset != 4 + len(self.data): raise Exception('Format')

        def read(self) -> io.BytesIO:
            s = io.BytesIO()
            # var data = new MemoryStream(self.data)
            # for (var i = 0; i + 1 < Blocks.Length; i++) {
            #     var block = Blocks[i + 0];
            #     var next = Blocks[i + 1];
            #     var size = next.VirtualOffset - block.VirtualOffset;
            #     data.Seek(block.FileOffset - 4, SeekOrigin.Begin);
            #     s.Seek(block.VirtualOffset, SeekOrigin.Begin);
            #     if (block.IsCompressed) new InflaterInputStream(data).CopyTo(s, size);
            #     else data.CopyTo(s, size);
            # }
            # s.Position = 0;
            return s;

    class ArchiveX:
        def __init__(self, r: BinaryReader):
            baseOffset = r.tell()
            magic = r.readUInt32()
            if magic != 0x4D324346: raise Exception('BAD MAGIC') # FC2M
            version = r.readUInt32()
            if version != 1: raise Exception('Format')
            offsetA = r.readUInt32(); offsetB = r.readUInt32(); offsetC = r.readUInt32()
            if offsetA != 20: raise Exception('Format')
            self.DAT = CompressedData(r)
            if baseOffset + offsetB != r.tell(): raise Exception('Format')
            self.FAT = CompressedData(r)
            if baseOffset + offsetC != r.tell(): raise Exception('Format')
            self.XML = CompressedData(r)

    #endregion

    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Map(r)

    def __init__(self, r: BinaryReader):
        version = r.readUInt32()
        if version != 11: raise Exception('Format')
        typeHash = r.readUInt32()
        if typeHash != 0xD2FD0A6B: raise Exception('Format')
        self.info = InfoX(r)
        self.snapshot = SnapshotX(r)
        self.data = DataX(r)
        self.archive = ArchiveX(r)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self))
    ]