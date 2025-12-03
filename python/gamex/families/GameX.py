from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import Family, Archive, FileSource, MetaManager, MetaInfo, MetaContent, IHaveMetaInfo
from gamex.core.formats.binary import Binary_Dds, Binary_Img, Binary_Pcx, Binary_Snd, Binary_Tga, Binary_Txt

# UnknownFamily
class UnknownFamily(Family):
    def __init__(self, elem: dict[str, object]):
        super().__init__(elem)

# UnknownArchive
class UnknownArchive(Archive):
    def __init__(self, state: ArcState):
        super().__init__(state)
        self.name = 'Unknown'
        self.assetFactoryFunc = self.assetFactory

    #region Factories

    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case '.txt' | '.ini' | '.cfg' | '.csv' | '.xml': return (0, Binary_Txt.factory)
            case '.wav': return (0, Binary_Snd.factory)
            case '.bmp' | '.jpg' | '.png' | '.gif' | '.tiff': return (0, Binary_Img.factory)
            case '.pcx': return (0, Binary_Pcx.factory)
            case '.tga': return (0, Binary_Tga.factory)
            case '.dds': return (0, Binary_Dds.factory)
            case _:
                match source.path:
                    case 'testtri.gfx': return (0, UnknownArchive.Binary_TestTri.factory)
                    case _: return (0, None)

    #endregion

    #region Binary

    class Binary_TestTri(IHaveMetaInfo):
        @staticmethod
        def factory(r: Reader, f: FileSource, s: Archive): return UnknownArchive.Binary_TestTri(r)

        def __init__(self, r: Reader):
            pass

        def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
            MetaInfo(None, MetaContent(type = 'TestTri', name = os.path.basename(file.path), value = self))
            ]

    #endregion

