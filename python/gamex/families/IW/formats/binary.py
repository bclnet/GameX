# Twin of Families/GameX.IW/Formats/Binary.cs for the .ff read and ReadData.
# CASC, .iwd, .pak, .ipak, .xpak, and IWI are not ported. Those extensions list nothing.
from __future__ import annotations
from io import BytesIO
from openx.core import _pathExtension, BinaryReader
from gamex import ArcBinaryT, BinaryArchive, FileSource
from gamex.families.IW.formats.fastFile import A100, F000, F100, IWff, S1ff, TAff, U100, listFastFile

class Binary_IW(ArcBinaryT):
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        source.files = []
        if source.game is not None and source.game.id in ('BO4', 'BOCW', 'Vanguard'):
            source.magic = 0
            return
        if _pathExtension(source.binPath).lower() != '.ff': return
        source.magic = 2
        magic = r.readUInt32()
        format_ = r.readUInt32()
        version = r.readUInt32()
        if magic not in (IWff, S1ff, TAff): raise Exception(f'Bad magic {magic}')
        if format_ not in (U100, F100, A100, F000): raise Exception(f'Bad format {format_}')
        listed = listFastFile(r, magic, format_, version)
        if listed is None: return
        zone, headers = listed
        source.tag = zone
        source.files = [FileSource(id=asset.id, path=asset.path, offset=asset.position, fileSize=asset.fileSize, data=asset.data) for asset in headers]

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        if file.data is not None: return BytesIO(bytes(file.data))
        zone = source.tag
        if isinstance(zone, (bytes, bytearray)) and file.fileSize and file.fileSize > 0:
            start = 0 if file.offset is None else file.offset
            end = start + file.fileSize
            if start < 0 or end > len(zone): raise Exception('offset')
            return BytesIO(bytes(zone[start:end]))
        return BytesIO()
