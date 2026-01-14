import os
from io import BytesIO
from enum import Enum
from openstk import IWriteToStream
from gamex import FileSource, ArcBinaryT, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo, DesSer
from gamex.families.Gamebryo.formats.nif import NiReader

# typedefs
class BinaryReader: pass
class Archive: pass
class BinaryArchive: pass

#region Binary_Nif

# Binary_Nif
class Binary_Nif(NiReader, IHaveMetaInfo, IWriteToStream):
    @staticmethod
    def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Nif(r, f)

    def __init__(self, r: BinaryReader, f: FileSource):
        super().__init__(r)
        self.name = os.path.splitext(os.path.basename(f.path))[0]

    def writeToStream(self, stream: object): return DesSer.serialize(self, stream)

    def __repr__(self): return DesSer.serialize(self)

    #region IModel

    def create(platform: str, func: callable):
        raise NotImplementedError('create')

    #endregion

    def isSkinnedMesh(self) -> bool: raise NotImplementedError('isSkinnedMesh') #return Blocks.Any(b => b is NiSkinInstance)

    def getTexturePaths(self) -> list[str]:
        raise NotImplementedError('getTexturePaths')
        # foreach (var niObject in Blocks)
        #     if (niObject is NiSourceTexture niSourceTexture && !string.IsNullOrEmpty(niSourceTexture.FileName))
        #         yield return niSourceTexture.FileName;

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('NIF', items = [
            MetaInfo(f'NumBlocks: {self.numBlocks}')
            ])
        ]

#endregion