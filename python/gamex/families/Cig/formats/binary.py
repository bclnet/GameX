import os
from io import BytesIO
from openstk.core import log, BinaryReader
from gamex import FileSource, ArcBinaryT #, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo
from gamex.families.Uncore._lib.compression import ZipFileX, ZipKind

# typedefs
class BinaryReader: pass
class BinaryArchive: pass

#region Binary_P4k

# Binary_P4k
class Binary_P4k(ArcBinaryT):
    key: bytearray = bytearray([0x5E, 0x7A, 0x20, 0x02, 0x30, 0x2E, 0xEB, 0x1A, 0x3B, 0xB6, 0x17, 0xC3, 0x0F, 0xDE, 0x1E, 0x47])

    class SubArchiveP4k(BinaryArchive):
        def __init__(self, arc: ZipFileX, source: BinaryArchive, game: FamilyGame, fileSystem: IFileSystem, filePath: str, tag: object = None):
            super().__init__(game, fileSystem, filePath, parent._instance, tag)
            self.assetFactoryFunc = source.assetFactoryFunc
            self.arc: ZipFileX = file
            self.useReader: bool = False
            # self.open()

        def read(self, r: BinaryReader, tag: object = None):
            # entry = (ZipArchiveEntry)Tag
            # var stream = entry.OpenX()
            # using var r2 = new BinaryReader(stream)
            # await ArcBinary.Read(this, r2, tag)
            pass

    # read
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        source.useReader = False
        files = source.files = []

        arc = source.tag = ZipFileX(r.f, path=source.binPath, key=self.key, kind=ZipKind.P4k)
        parentByPath: dict[str, FileSource] = {}
        partsByPath: dict[str, list[FileSource]] = {}

        for entry in arc.infolist():
            metadata = FileSource(
                path = entry.filename.replace('\\', '/'),
                #flags = entry.flags,
                packedSize = entry.compress_size,
                fileSize = entry.file_size,
                tag = entry)
        metadataPath = metadata.path.lower()
        if metadataPath.endswith('.pak') or metadataPath.endswith('.socpak'): metadata.arc = SubArchiveP4k(source, arc, metadataPath, metadata.tag)
        elif metadataPath.endswith('.dds') or metadataPath.endswith('.dds.a'): parentByPath[metadataPath] = metadata
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
            for kv in partsByPath:
                if parent := parentByPath[kv.key]: parent.parts = kv.value.values

#endregion