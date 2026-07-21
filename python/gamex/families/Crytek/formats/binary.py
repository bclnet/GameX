import io, os
from enum import Enum
# from numpy import ndarray
from dataclasses import dataclass
from openstk.core import log, BinaryReader, StreamIterators, ForwardStream
from gamex import FileSource, ArcBinaryT, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo
from gamex.families.Uncore._lib.compression import ZipFileX, ZipKind
from Crypto.Cipher import AES

# types
# type Vector3 = ndarray

# typedefs
class BinaryArchive: pass

# Binary_ArcheAge
class Binary_ArcheAge(ArcBinaryT):
    #region Headers

    MAGIC = 0x4f424957 # Magic for Archeage, the literal string "WIBO".

    # tag::Binary_ArcheAge.HDR[]
    class HDR:
        _struct = ('<8I', 32)
        def __init__(self, t):
            (self.magic, dummy1,
            self.fileCount,
            self.extraFiles, dummy2, dummy3, dummy4, dummy5) = t
    # end::Binary_Ba2.HDR[]

    #endregion

    def __init__(self, key: bytes = None):
        self.key = key

    # read - tag::Binary_ArcheAge.read[]
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        fs = r.f; fsLength = r.length
        aes = lambda: AES.new(self.key, AES.MODE_CBC, bytes(16))
        r = BinaryReader(ForwardStream(StreamIterators.StreamCipher(fs, aes())))

        # read hdr & skip
        fs.seek(fsLength - 0x200, 0)
        hdr = r.readS(self.HDR)
        if hdr.magic > self.MAGIC: raise Exception('BAD MAGIC')
        totalSize = (hdr.fileCount + hdr.extraFiles) * 0x150
        infoOffset = fsLength - 0x200 - totalSize
        while infoOffset >= 0:
            if (infoOffset % 0x200) != 0: infoOffset -= 0x10
            else: break

        # read-all files
        source.files = files = [None] * hdr.fileCount
        for i in range(hdr.fileCount):
            r = BinaryReader(ForwardStream(StreamIterators.StreamCipher(fs, aes())))
            fs.seek(infoOffset, 0)
            files[i] = FileSource(
                path = r.readFAString(0x108), #: name //.Replace('\\', '/')
                offset = r.readInt64(),       #: offset
                fileSize = r.readInt64(),     #: size
                packedSize = r.readInt64(),   #: xsize
                compressed = r.readInt32())   #: ysize
            infoOffset += 0x150
    # end::Binary_ArcheAge.read[]

    # readData - tag::Binary_ArcheAge.readData[]
    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> io.BytesIO:
        r.seek(file.offset)
        return io.BytesIO(r.readBytes(file.fileSize))
    # end::Binary_ArcheAge.readData[]

# Binary_Dunia
class Binary_Dunia(ArcBinaryT):
    MAGIC = 0x46415432

    class Platform(Enum):
        Any = 0
        Windows = 1  # data_win32 or data_win64 or data_final
        Xenon = 2 # no platform directory (Xbox 360)
        PS3 = 3 # data_ps3
        WiiU = 4 # no platform directory (Wii U)
        Durango = 5 # data_durango (Xbox One)
        Orbis = 6 # data_orbis (PS4)
        Scarlett = 7 # data_scarlett (Xbox Series X)
        Prospero = 8 # data_prospero (PS5)
        Yeti = 9 # data_yeti (Stadia)

    class Compression(Enum):
        None_ = 0
        LZO1x = 1
        Zlib = 2
        XMemCompress = 3 # Xbox 360
        LZMA = 4
        LZ4 = 5
        LZ4LW = 6
        Oodle = 7

    @dataclass(frozen=True)
    class Version:
        fileVersion: int
        platform: 'Platform'
        compressionVersion: int

    KnownVersions = [
        # 32
        Version(5, Platform.Any, 0),       # Far Cry 2
        Version(5, Platform.Windows, 3),   # Far Cry 2
        Version(5, Platform.PS3, 4),       # Far Cry 2
        Version(9, Platform.Any, 3),       # Far Cry 3
        Version(9, Platform.Windows, 3),   # Far Cry 3
        # 64
        Version(9, Platform.Any, 0),       # Far Cry 3, Far Cry 3 Blood Dragon, Far Cry 4, Far Cry Primal
        Version(9, Platform.Windows, 3),   # Far Cry 3, Far Cry 3 Blood Dragon, Far Cry 4
        Version(9, Platform.Windows, 4),   # Far Cry Primal
        Version(10, Platform.Windows, 0),  # Far Cry 5, Far Cry New Dawn
        Version(11, Platform.Windows, 0)]   # Far Cry 6

    TEAXors = [0x76, 0x41, 0x74, 0x1E, 0x4E, 0x16, 0x1E, 0x02, 0x6A, 0x5B, 0x72, 0x0B, 0x60, 0x4F, 0x72, 0x25]
    @staticmethod
    def makeXTEAKey(value: int) -> list[int]:
        key = bytearray(16)
        for i in range(16):
            b = ((value >> i) + 0x39) & 0xFF; x = Binary_Dunia.TEAXors[i]
            key[i] = (b ^ x) & 0xFF if b != x else 0xFF
        return [int.from_bytes(key[:4], 'little', signed=False), int.from_bytes(key[4:8], 'little', signed=False), int.from_bytes(key[8:12], 'little', signed=False), int.from_bytes(key[12:], 'little', signed=False)]

    @staticmethod
    def decryptIndex(data: bytes):
        size = len(data) & ~7
        key = Binary_Dunia.makeXTEAKey(size)
        print(key, 'TODO decryptIndex')
        exit(1)
        # XTEA.Decrypt(data, 0, size, key)
        # return new BinaryReader(new MemoryStream(data, false))
        return None

    # read - tag::Binary_Dunia.read[]
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        if not source.binPath.endswith('.fat'): raise Exception('must be a .fat file')
        magic = r.readUInt32()
        if magic != Binary_Dunia.MAGIC: raise Exception('BAD MAGIC')
        fileVersionAndEncryptionFlag = r.readUInt32()
        fileVersion = fileVersionAndEncryptionFlag & ~0x80000000
        indexIsEncrypted = (fileVersionAndEncryptionFlag & 0x80000000) != 0
        if indexIsEncrypted and fileVersion < 11: raise Exception('encryption flag set when unsupported')
        if fileVersion > 11: raise Exception('unsupported version')
        flags = r.readUInt32() if fileVersion >= 3 else 0
        unknown0C = r.readUInt32() if fileVersion >= 9 else 0
        unknown10 = r.readUInt32() if fileVersion >= 9 else 0
        platform = Binary_Dunia.Platform((flags >> 0) & 0xFF)
        compressionVersion = (flags >> 8) & 0xFF
        if flags & 0xFFFF0000 != 0: raise Exception('unknown flags')
        version = Binary_Dunia.Version(fileVersion, platform, compressionVersion)
        if version not in Binary_Dunia.KnownVersions: raise Exception('unknown version/platform/CV combination')
        if unknown0C != 0 or unknown10 != 0: raise Exception('Not Implemented')

        # read files
        filelist = source.binPath.replace('.fat', '.filelist').replace('\\', '/')
        match source.game.id:
            case 'FarCry2':
                from ....resources.Crytek import FarCry2
                hashes = FarCry2.getHashes(filelist)
            case 'FarCry3' | 'FarCry3:BD' | 'FarCry4':
                from ....resources.Crytek import FarCry3
                hashes = FarCry3.getHashes(filelist)
            case 'FarCry5':
                from ....resources.Crytek import FarCry5
                hashes = FarCry5.getHashes(filelist)
            case 'FarCry6':
                from ....resources.Crytek import FarCry6
                hashes = FarCry6.getHashes(filelist)
            case 'FarCryND':
                from ....resources.Crytek import FarCryNewDawn
                hashes = FarCryNewDawn.getHashes(filelist)
            case 'FarCryP':
                from ....resources.Crytek import FarCryPrimal
                hashes = FarCryPrimal.getHashes(filelist)
            case _: raise Exception(f'Not Implemented {source.game.id}')

        # read files
        source.binPath = source.binPath.replace('.fat', '.dat')
        entryCount = r.readInt32()
        if entryCount < 0: raise Exception()
        source.files = files = [FileSource] * entryCount; hash = 0
        match fileVersion:
            case 1:  # NOTUSED
                if indexIsEncrypted: r = decryptIndex(r.readBytes(entryCount * 24))
                for i in range(len(files)):
                    a = r.readUInt32(); _ = r.readUInt32(); c = r.readUInt32(); d = r.readUInt32(); e = r.readUInt32(); _2 = r.readUInt32()
                    hash = a
                    files[i] = FileSource(
                        hash = hash,
                        path = hashes.get(hash) or str(hash),
                        fileSize = (e >> 2) & 0x3FFFFFFF,
                        compressed = (e >> 0) & 0x3,
                        offset = (d << 2) | ((c >> 30) & 0x3),
                        packedSize = (c >> 0) & 0x3FFFFFFF)
            case 6 | 8: # NOTUSED
                if indexIsEncrypted: r = decryptIndex(r.readBytes(entryCount * 20))
                for i in range(len(files)):
                    a = r.readUInt64(); c = r.readUInt32(); d = r.readUInt32(); e = r.readUInt32()
                    hash = a
                    files[i] = FileSource(
                        hash = hash,
                        path = hashes.get(hash) or str(hash),
                        fileSize = (c >> 2) & 0x3FFFFFFF,
                        compressed = (c >> 0) & 0x3,
                        offset = (d << 2) | ((e >> 30) & 0x3),
                        packedSize = (e >> 0) & 0x3FFFFFFF)
            case 7: # NOTUSED
                if indexIsEncrypted: r = decryptIndex(r.readBytes(entryCount * 24))
                for i in range(len(files)):
                    a = r.readUInt32(); b = r.readUInt32(); c = r.readUInt32(); _ = r.readUInt32(); e = r.readUInt32(); f = r.readUInt32()
                    hash = (a << 32) | b
                    files[i] = FileSource(
                        hash = hash,
                        path = hashes.get(hash) or str(hash),
                        fileSize = (c >> 2) & 0x3FFFFFFF,
                        compressed = (c >> 0) & 0x3,
                        offset = (f << 2) | ((e >> 30) & 0x3),
                        packedSize = (e >> 0) & 0x3FFFFFFF)
            case 5:
                if indexIsEncrypted: r = decryptIndex(r.readBytes(entryCount * 16))
                for i in range(len(files)):
                    a = r.readUInt32(); b = r.readUInt32(); c = r.readUInt32(); d = r.readUInt32()
                    hash = a
                    files[i] = FileSource(
                        hash = hash,
                        path = hashes.get(hash) or str(hash),
                        fileSize = (b >> 2) & 0x3FFFFFFF,
                        compressed = (b >> 0) & 0x3,
                        packedSize = (c >> 0) & 0x3FFFFFFF,
                        offset = ((c >> 30) & 0x3) | d << 2)
            case 9:
                if indexIsEncrypted: r = decryptIndex(r.readBytes(entryCount * 20))
                for i in range(len(files)):
                    a = r.readUInt32(); b = r.readUInt32(); c = r.readUInt32(); d = r.readUInt32(); e = r.readUInt32()
                    hash = (a << 32) | b
                    files[i] = FileSource(
                        hash = hash,
                        path = hashes.get(hash) or str(hash),
                        fileSize = (c >> 2) & 0x3FFFFFFF,
                        compressed = (c >> 0) & 0x3,
                        offset = (d << 2) | ((e >> 30) & 0x3),
                        packedSize = (e >> 0) & 0x3FFFFFFF)
            case 10:
                if indexIsEncrypted: r = decryptIndex(r.readBytes(entryCount * 20))
                for i in range(len(files)):
                    a = r.readUInt32(); b = r.readUInt32(); c = r.readUInt32(); d = r.readUInt32(); e = r.readUInt32()
                    hash = (a << 32) | b
                    files[i] = FileSource(
                        hash = hash,
                        path = hashes.get(hash) or str(hash),
                        fileSize = (c >> 2) & 0x3FFFFFFF,
                        flags = 1 if (c >> 0) & 0x1 != 0 else 0,
                        compressed = (c >> 1) & 0x1,
                        offset = (d << 3) | (e >> 29),
                        packedSize = (e >> 0) & 0x1FFFFFFF)
            case 11:
                if indexIsEncrypted: r = decryptIndex(r.readBytes(entryCount * 20))
                for i in range(len(files)):
                    a = r.readUInt32(); b = r.readUInt32(); c = r.readUInt32(); d = r.readUInt32(); e = r.readUInt32()
                    hash = (a << 32) | b
                    files[i] = FileSource(
                        hash = hash,
                        path = hashes.get(hash) or str(hash),
                        fileSize = (c >> 2) & 0x3FFFFFFF,
                        flags = 1 if (c >> 0) & 0x1 != 0 else 0,
                        compressed = (c >> 1) & 0x1,
                        offset = (d << 7) | ((e >> 25) & 0x70),
                        packedSize = (e >> 0) & 0x1FFFFFFF),
            case _: raise Exception(f'fileVersion: out of range {fileVersion}')

        # read localization
        localization = r.readL32FArray(lambda _: (r.readL32AString(32), r.readUInt64()))

    # end::Binary_Dunia.read[]

    # readData - tag::Binary_Dunia.readData[]
    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> io.BytesIO:
        r.seek(file.offset)
        if file.flags != 0:
            entrySize = file.packedSize if file.compressed != 0 else file.fileSize
            encryptedSize = min(0x100000, entrySize)
            encryptedSize &= ~7
            if encryptedSize > 0:
                data = r.readBytes(entrySize)
                key = Binary_Dunia.makeXTEAKey(encryptedSize)
                # XTEA.Decrypt(data, 0, encryptedSize, key)
                r = BinaryReader(io.BytesIO(data))
        match Binary_Dunia.Compression(file.compressed):
            case Binary_Dunia.Compression.None_: decompress = Binary_Dunia.decompressNone
            case Binary_Dunia.Compression.LZO1x: decompress = Binary_Dunia.decompressLZO1x
            case Binary_Dunia.Compression.Zlib: decompress = Binary_Dunia.decompressZlib
            case Binary_Dunia.Compression.LZ4: decompress = Binary_Dunia.decompressLZ4
            case _: raise Exception('unsupported compression scheme')
        s = io.BytesIO()
        decompress(r, file, s)
        s.seek(0)
        return s
    # end::Binary_Dunia.readData[]

    @staticmethod
    def decompressNone(r: BinaryReader, f: FileSource, o: io.BytesIO) -> None:
        import shutil
        shutil.copyfileobj(r.f, o, f.fileSize) # r.copyTo(o, f.fileSize)

    @staticmethod
    def decompressLZO1x(r: BinaryReader, f: FileSource, o: io.BytesIO) -> None:
        from ...._LIB.compression.lzo import Lzo1xDecompressor
        compressedBytes = bytearray(f.packedSize)
        if r.f.readinto(compressedBytes) != f.packedSize: raise Exception('could not read all compressed bytes')
        uncompressedBytes = bytearray(f.fileSize)
        result = Lzo1xDecompressor(compressedBytes, uncompressedBytes).decompress()
        if result != 0: raise Exception(f'LZO decompression failure ({result})')
        o.write(uncompressedBytes)

# Binary_Cry3
class Binary_Cry3(ArcBinaryT):
    def __init__(self, key: bytes):
        self.key = key

    # read - tag::Binary_Cry3.read[]
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        source.useReader = False
        files = source.files = []
        r.leaveOpen = True

        arc = source.tag = ZipFileX(r.f, path=source.binPath, key=self.key, kind=ZipKind.Cry3)
        parentByPath: dict[str, FileSource] = {}
        partsByPath: dict[str, list[FileSource]] = {}

        for s in arc.infolist():
            if s.is_dir(): continue
            metadata = FileSource(
                path = s.filename.replace('\\', '/'),
                #flags = s.flags,
                packedSize = s.compress_size,
                fileSize = s.file_size,
                tag = s)
            metadataPath = metadata.path.lower()
            if metadataPath.endswith('.dds'): parentByPath[metadataPath] = metadata
            elif len(metadataPath) > 8 and '.dds.' in metadataPath[-8:]:
                parentPath = metadataPath[:(metadataPath.index('.dds') + 4)]
                if metadataPath.endswith('a'): parentPath += '.a'
                parts = partsByPath.get(parentPath)
                if not parts: parts = {}; partsByPath[parentPath] = parts
                parts[metadataPath] = metadata
                continue
            files.append(metadata)

        # process links
        if len(partsByPath) > 0:
            for kv in partsByPath.items():
                if parent := parentByPath.get(kv[0]): parent.parts = list(kv[1].values())
    # end::Binary_Cry3.read[]

    # readData - tag::Binary_Cry3.readData[]
    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> io.BytesIO:
        arc: ZipFileX = source.tag
        entry = file.tag
        with arc.open(entry) as input:
            return io.BytesIO(input.read())
    # end::Binary_Cry3.readData[]