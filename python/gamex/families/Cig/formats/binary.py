import os
from io import BytesIO
from gamex.core.binary import ArcBinaryT, BinaryArchive
from gamex.core.meta import FileSource
from gamex.families.Uncore._lib.compression import ZipArchiveX

# typedefs
class BinaryReader: pass
class BinaryArchive: pass

#region Binary_P4k

# Binary_P4k
class Binary_P4k(ArcBinaryT):
    key: bytearray = bytearray([0x5E, 0x7A, 0x20, 0x02, 0x30, 0x2E, 0xEB, 0x1A, 0x3B, 0xB6, 0x17, 0xC3, 0x0F, 0xDE, 0x1E, 0x47])

    # class SubArchiveP4k(BinaryArchive):
    #     def __init__(self, arc: ZipArchiveX, source: BinaryArchive, game: FamilyGame, fileSystem: IFileSystem, filePath: str, tag: object = None):
    #         super().__init__(game, fileSystem, filePath, parent._instance, tag)
    #         self.assetFactoryFunc = source.assetFactoryFunc
    #         self.arc: ZipArchiveX = file
    #         self.useReader: bool = False
    #         # self.open()

    #     def read(self, r: BinaryReader, tag: object = None):
    #         # entry = (ZipArchiveEntry)Tag
    #         # var stream = entry.OpenX()
    #         # using var r2 = new BinaryReader(stream)
    #         # await ArcBinary.Read(this, r2, tag)
    #         pass

    # read
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        source.useReader = False
        # files = source.files = []

        # arc: ZipArchiveX = source.Tag = ZipArchiveX(ZipArchiveKind.P4k, r.f, source.binPath, Key)
        # parentByPath: dict[str, FileSource] = {}
        # partsByPath: dict[str, list[FileSource]] = {}

        # for entry in arc.entries:
        #     metadata = FileSource {
        #         path = entry.fullName.replace('\\', '/'),
        #         #flags = entry.flags,
        #         packedSize = entry.compressedLength,
        #         fileSize = entry.length,
        #         tag = entry
        #     }
        # metadataPath = metadata.path.lower()
        # if metadataPath.endsWith('.arc') or metadataPath.endsWith('.socpak'): metadata.arc = SubArchiveP4k(source, arc, metadataPath, metadata.tag)
        # elif metadataPath.endsWith('.dds') or metadataPath.endsWith('.dds.a'): parentByPath.put(metadataPath, metadata)
        # elif len(metadataPath) > 8 and metadataPath[^8..].contains('.dds.'):
        #     parentPath = metadataPath[..(metadataPath.indexOf('.dds') + 4)]
        #     if metadataPath.endsWith('a'): parentPath += '.a'
        #     parts = partsByPath[parentPath]
        #     if parts: partsByPath.put(parentPath, parts = [])
        #     parts.put(metadataPath, metadata)
        #     continue
        # files.append(metadata)

        # # process links
        # if len(partsByPath) > 0:
        #     for kv in partsByPath:
        #         if parent = parentByPath[kv.key]: parent.parts = kv.value.values

#endregion