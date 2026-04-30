import os
from io import BytesIO
from enum import Enum
from openstk.core import IWriteToStream
from gamex import FileSource, ArcBinaryT, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo, DesSer
from gamex.families.Gamebryo.formats.nif import NiReader, NiSkinInstance, NiSourceTexture

# typedefs
class BinaryReader: pass
class Archive: pass
class BinaryArchive: pass

#region Binary_Nif

# Binary_Nif
class Binary_Nif(NiReader, IHaveMetaInfo, IWriteToStream):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Nif(r, f)

    def __init__(self, r: BinaryReader, f: FileSource):
        super().__init__(r)
        self.name = os.path.splitext(os.path.basename(f.path))[0]

    def writeToStream(self, stream: object): return DesSer.serialize(self, stream)

    def __repr__(self): return DesSer.serialize(self)

    #region IModel

    def create(platform: str, func: callable):
        raise NotImplementedError('create')

    #endregion

    def isSkinnedMesh(self) -> bool: return any((s for s in self.blocks if isinstance(s, NiSkinInstance)))

    def getTexturePaths(self) -> list[str]: return (s.fileName for s in self.blocks if isinstance(s, NiSourceTexture) and s.fileName) 

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('NIF', items = [
            MetaInfo(f'NumBlocks: {self.numBlocks}')
            ])
        ]

#endregion