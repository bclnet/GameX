from __future__ import annotations
from io import BytesIO
from openx.core import BinaryReader
from gamex import ArcBinaryT, FileSource, MetaInfo, IHaveMetaInfo

#region Binary_Abc

# Binary_Abc
class Binary_Abc(IHaveMetaInfo):
    def __init__(self, r: BinaryReader):
        pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo('BinaryPak', items = [
            # MetaInfo(f'Type: {self.type}'),
        ])
    ]

#endregion

#region Binary_XXX

# Binary_XXX
class Binary_XXX(ArcBinaryT):
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        source.files = []

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        raise NotImplementedError()

#endregion
