from __future__ import annotations
import os
from io import BytesIO
from hashlib import md5
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.backends import default_backend
from cryptography.hazmat.primitives.asymmetric import padding
from cryptography.hazmat.primitives import serialization
from gamex import FileSource, FileOption, BinaryPakFile, PakBinaryT
from gamex.compression import decompressBlast
from gamex.util import _throw, _pathExtension
from openstk.poly import Reader, unsafe, X_LumpON

#region PakBinary_Bsp30

# PakBinary_Bsp30
class PakBinary_Bsp30(PakBinaryT):

    #region Headers

    class B_Header:
        struct = ('<31i', 124)
        def __init__(self, tuple):
            entities = self.entities = X_LumpON()
            planes = self.planes = X_LumpON()
            textures = self.textures = X_LumpON()
            vertices = self.vertices = X_LumpON()
            visibility = self.visibility = X_LumpON()
            nodes = self.nodes = X_LumpON()
            texInfo = self.texInfo = X_LumpON()
            faces = self.faces = X_LumpON()
            lighting = self.lighting = X_LumpON()
            clipNodes = self.clipNodes = X_LumpON()
            leaves = self.leaves = X_LumpON()
            markSurfaces = self.markSurfaces = X_LumpON()
            edges = self.edges = X_LumpON()
            surfEdges = self.surfEdges = X_LumpON()
            models = self.models = X_LumpON()
            self.version, \
            entities.offset, entities.length, \
            planes.offset, planes.length, \
            textures.offset, textures.length, \
            vertices.offset, vertices.length, \
            visibility.offset, visibility.length, \
            nodes.offset, nodes.length, \
            texInfo.offset, texInfo.length, \
            faces.offset, faces.length, \
            lighting.offset, lighting.length, \
            clipNodes.offset, clipNodes.length, \
            leaves.offset, leaves.length, \
            markSurfaces.offset, markSurfaces.length, \
            edges.offset, edges.length, \
            surfEdges.offset, surfEdges.length, \
            models.offset, models.length = tuple
        def forGameId(self, id: str) -> None:
            if id == 'HL:BS': (self.entities, self.planes) = (self.planes, self.entities)

    class B_Texture:
        struct = ('<16s6I', 20)
        def __init__(self, tuple):
            self.name, \
            self.width, \
            self.height, \
            self.offsets = tuple

    # MAX_MAP_HULLS = 4
    # MAX_MAP_MODELS = 400
    # MAX_MAP_BRUSHES = 4096
    # MAX_MAP_ENTITIES = 1024
    # MAX_MAP_ENTSTRING = (128 * 1024)
    # MAX_MAP_PLANES = 32767
    # MAX_MAP_NODES = 32767
    # MAX_MAP_CLIPNODES = 32767
    # MAX_MAP_LEAFS = 8192
    # MAX_MAP_VERTS = 65535
    # MAX_MAP_FACES = 65535
    # MAX_MAP_MARKSURFACES = 65535
    # MAX_MAP_TEXINFO = 8192
    # MAX_MAP_EDGES = 256000
    # MAX_MAP_SURFEDGES = 512000
    # MAX_MAP_TEXTURES = 512
    # MAX_MAP_MIPTEX = 0x200000
    # MAX_MAP_LIGHTING = 0x200000
    # MAX_MAP_VISIBILITY = 0x200000
    # MAX_MAP_PORTALS = 65536

    #endregion

    #read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        source.files = files = []

        # read file
        header = r.readS(self.B_Header)
        if header.version != 30: raise Exception('BAD VERSION')
        header.forGameId(source.game.id)
        files.append(FileSource(path = 'entities.txt', offset = header.entities.offset, fileSize = header.entities.num))
        files.append(FileSource(path = 'planes.dat', offset = header.planes.offset, fileSize = header.planes.num))
        
        files.append(FileSource(path = 'vertices.dat', offset = header.vertices.offset, fileSize = header.vertices.num))
        files.append(FileSource(path = 'visibility.dat', offset = header.visibility.offset, fileSize = header.visibility.num))
        files.append(FileSource(path = 'nodes.dat', offset = header.nodes.offset, fileSize = header.nodes.num))
        files.append(FileSource(path = 'texInfo.dat', offset = header.texInfo.offset, fileSize = header.texInfo.num))
        files.append(FileSource(path = 'faces.dat', offset = header.faces.offset, fileSize = header.faces.num))
        files.append(FileSource(path = 'lighting.dat', offset = header.lighting.offset, fileSize = header.lighting.num))
        files.append(FileSource(path = 'clipNodes.dat', offset = header.clipNodes.offset, fileSize = header.clipNodes.num))
        files.append(FileSource(path = 'leaves.dat', offset = header.leaves.offset, fileSize = header.leaves.num))
        files.append(FileSource(path = 'markSurfaces.dat', offset = header.markSurfaces.offset, fileSize = header.markSurfaces.num))
        files.append(FileSource(path = 'edges.dat', offset = header.edges.offset, fileSize = header.edges.num))
        files.append(FileSource(path = 'surfEdges.dat', offset = header.surfEdges.offset, fileSize = header.surfEdges.num))
        files.append(FileSource(path = 'markSurfaces.dat', offset = header.markSurfaces.offset, fileSize = header.markSurfaces.num))

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource, option: FileOption = None) -> BytesIO:
        r.seek(file.offset)
        return BytesIO(r.readBytes(file.fileSize))

#endregion

#region PakBinary_Vpk

# PakBinary_Vpk
class PakBinary_Vpk(PakBinaryT):

    #region Headers

    MAGIC = 0x55AA1234

    class V_HeaderV2:
        struct = ('<4I', 16)
        def __init__(self, tuple):
            self.fileDataSectionSize, \
            self.archiveMd5SectionSize, \
            self.otherMd5SectionSize, \
            self.signatureSectionSize = tuple

    class V_ArchiveMd5:
        struct = ('<3I16s', 28)
        def __init__(self, tuple):
            self.archiveIndex, \
            self.offset, \
            self.length, \
            self.checksum = tuple

    class Verification:
        archiveMd5s: tuple = (0, bytearray())                  # Gets the archive MD5 checksum section entries. Also known as cache line hashes.
        treeChecksum: bytearray = bytearray()                  # Gets the MD5 checksum of the file tree.
        archiveMd5EntriesChecksum: bytearray = bytearray()     # Gets the MD5 checksum of the archive MD5 checksum section entries.
        wholeFileChecksum: tuple = (0, bytearray())            # Gets the MD5 checksum of the complete package until the signature structure.
        publicKey: bytearray = bytearray()                     # Gets the public key.
        signature: tuple = (0, bytearray())                    # Gets the signature.

        def __init__(self, r: Reader, h: V_HeaderV2):
            # archive md5
            if h.archiveMd5SectionSize != 0:
                self.archiveMd5s = (r.tell(), r.readSArray(PakBinary_Vpk.V_ArchiveMd5, h.archiveMd5SectionSize // 28))
            # other md5
            if h.otherMd5SectionSize != 0:
                self.treeChecksum = r.readBytes(16)
                self.archiveMd5EntriesChecksum = r.readBytes(16)
                self.wholeFileChecksum = (r.tell(), r.readBytes(16))
            # signature
            if h.signatureSectionSize != 0:
                position = r.tell()
                publicKeySize = r.readInt32()
                if h.signatureSectionSize == 20 and publicKeySize == self.MAGIC: return; # CS2 has this
                self.publicKey = r.readBytes(publicKeySize)
                self.signature = (position, r.readBytes(r.readInt32()))

        # Verify checksums and signatures provided in the VPK
        def verifyHashes(self, r: Reader, treeSize: int, h: V_HeaderV2, headerPosition: int) -> None:
            # treeChecksum
            r.seek(headerPosition)
            hash = md5(r.readBytes(treeSize)).digest()
            if hash != self.treeChecksum: raise Exception(f'File tree checksum mismatch ({hash.hex()} != expected {self.treeChecksum.hex()})')
            # archiveMd5SectionSize
            r.seek(self.archiveMd5s[0])
            hash = md5(r.readBytes(h.archiveMd5SectionSize)).digest()
            if hash != self.archiveMd5EntriesChecksum: raise Exception(f'Archive MD5 checksum mismatch ({hash.hex()} != expected {self.archiveMd5EntriesChecksum.hex()})')
            # wholeFileChecksum
            r.seek(0)
            hash = md5(r.readBytes(self.wholeFileChecksum[0])).digest()
            if hash != self.wholeFileChecksum[1]: raise Exception(f'Package checksum mismatch ({hash.hex()} != expected {self.wholeFileChecksum[1].hex()})')

        # Verifies the RSA signature
        def verifySignature(self, r: Reader) -> None:
            if not self.publicKey or not self.signature[1]: return
            publicKey = serialization.load_der_public_key(self.publicKey, backend = default_backend())
            r.seek(0)
            data = r.readBytes(self.signature[0])
            publicKey.verify(
                self.signature[1],
                data,
                padding.PKCS1v15(),
                hashes.SHA256()
            )

    #endregion

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        source.files = files = []

        # file mask
        def fileMask(path: str) -> str:
            extension = _pathExtension(path)
            if extension.endswith('_c'): extension = extension[:-2]
            if extension.startswith('.v'): extension = extension[2:]
            return f'{os.path.splitext(os.path.basename(path))[0]}{extension}'
        source.fileMask = fileMask

        # pakPath
        pakPath = source.pakPath
        dirVpk = pakPath.endswith('_dir.vpk')
        if dirVpk: pakPath = pakPath[:-8]

        # read header
        if r.readUInt32() != self.MAGIC: raise Exception('BAD MAGIC')
        version = r.readUInt32()
        treeSize = r.readUInt32()
        if version == 0x00030002: raise Exception('Unsupported VPK: Apex Legends, Titanfall')
        elif version > 2: raise Exception(f'Bad VPK version. ({version})')
        headerV2 = r.readS(self.V_HeaderV2) if version == 2 else None
        headerPosition = r.tell()
        
        # read entires
        ms = BytesIO()
        while True:
            typeName = r.readVUString(ms=ms)
            if not typeName: break
            while True:
                directoryName = r.readVUString(ms=ms)
                if not directoryName: break
                while True:
                    fileName = r.readVUString(ms=ms)
                    if not fileName: break
                    # get file
                    file = FileSource(
                        path = f'{f'{directoryName}/' if directoryName[0] != ' ' else ''}{fileName}.{typeName}',
                        hash = r.readUInt32(),
                        data = bytearray(r.readUInt16()),
                        id = r.readUInt16(),
                        offset = r.readUInt32(),
                        fileSize = r.readUInt32()
                        )
                    terminator = r.readUInt16()
                    if terminator != 0xFFFF: raise Exception(f'Invalid terminator, was 0x{terminator:X} but expected 0x{0xFFFF:X}')
                    if len(file.data) > 0: r.read(file.data, 0, len(file.data))
                    if file.id != 0x7FFF:
                        if not dirVpk: raise Exception('Given VPK is not a _dir, but entry is referencing an external archive.')
                        file.tag = f'{pakPath}_{file.id:03d}.vpk'
                    else: file.tag = headerPosition + treeSize
                    # add file
                    files.append(file)

        # verification
        if version == 2:
            # skip over file data, if any
            r.skip(headerV2.fileDataSectionSize)
            v = self.Verification(r, headerV2)
            v.verifyHashes(r, treeSize, headerV2, headerPosition)
            v.verifySignature(r)

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource, option: FileOption = None) -> BytesIO:
        fileDataLength = len(file.data)
        data = bytearray(fileDataLength + file.fileSize); mv = memoryview(data)
        if fileDataLength > 0: data[0:] = file.data
        def _str(r2: Reader): r2.seek(file.offset); r2.read(mv, fileDataLength, file.fileSize)
        if file.fileSize == 0: pass
        elif isinstance(file.tag, int): r.seek(file.offset + file.tag); r.read(mv, fileDataLength, file.fileSize)
        elif isinstance(file.tag, str): source.reader(_str, file.tag)
        return BytesIO(data)

#endregion

#region PakBinary_Wad3

# PakBinary_Wad3
class PakBinary_Wad3(PakBinaryT):

    #region Headers

    W_MAGIC = 0x33444157 #: WAD3

    class W_Header:
        struct = ('<3I', 12)
        def __init__(self, tuple):
            self.magic, \
            self.lumpCount, \
            self.lumpOffset = tuple

    class W_Lump:
        struct = ('<3I2bH16s', 32)
        def __init__(self, tuple):
            self.offset, \
            self.diskSize, \
            self.size, \
            self.type, \
            self.compression, \
            self.padding, \
            self.name = tuple

    class W_LumpInfo:
        struct = ('<3I', 12)
        def __init__(self, tuple):
            self.width, \
            self.height, \
            self.paletteSize = tuple

    #endregion

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        source.files = files = []

        # read file
        header = r.readS(self.W_Header)
        if header.magic != self.W_MAGIC: raise Exception('BAD MAGIC')
        r.seek(header.lumpOffset)
        lumps = r.readSArray(self.W_Lump, header.lumpCount)
        for lump in lumps:
            name = unsafe.fixedAString(lump.name, 16)
            path = None
            match lump.type:
                case 0x40: path = f'{name}.tex2'
                case 0x42: path = f'{name}.pic'
                case 0x43: path = f'{name}.tex'
                case 0x46: path = f'{name}.fnt'
                case _: path = f'{name}.{lump.type:x}'
            files.append(FileSource(
                path = path,
                offset = lump.offset,
                compressed = lump.compression,
                fileSize = lump.diskSize,
                packedSize = lump.size,
                ))

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource, option: FileOption = None) -> BytesIO:
        r.seek(file.offset)
        return BytesIO(
            r.readBytes(file.fileSize) if file.compressed == 0 else \
            _throw('NotSupportedException')
            )

#endregion