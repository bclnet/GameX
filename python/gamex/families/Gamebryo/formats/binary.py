import os
from io import BytesIO
from enum import Enum
from openstk.poly import IWriteToStream
from gamex import FileSource, PakBinaryT, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo, DesSer
from gamex.families.Gamebryo.formats.nif import NiReader

# typedefs
class Reader: pass
class PakFile: pass
class BinaryPakFile: pass

#region Binary_Nif

# Binary_Nif
class Binary_Nif(NiReader, IHaveMetaInfo, IWriteToStream):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Nif(r, f)

    def __init__(self, r: Reader, f: FileSource):
        super().__init__(r)
        self.name = os.path.splitext(os.path.basename(f.path))[0]

    def writeToStream(self, stream: object): return DesSer.serialize(self, stream)

    def __repr__(self): return DesSer.serialize(self)

    #region IModel

    def create(platform: str, func: callable):
        raise NotImplementedError()

    #endregion

    def isSkinnedMesh(self) -> bool: raise NotImplementedError() #return Blocks.Any(b => b is NiSkinInstance)

    def getTexturePaths(self) -> list[str]:
        raise NotImplementedError()
        # foreach (var niObject in Blocks)
        #     if (niObject is NiSourceTexture niSourceTexture && !string.IsNullOrEmpty(niSourceTexture.FileName))
        #         yield return niSourceTexture.FileName;

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('NIF', items = [
            MetaInfo(f'NumBlocks: {self.header.numBlocks}')
            ])
        ]

#endregion