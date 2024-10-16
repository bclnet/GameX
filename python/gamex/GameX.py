import os
from gamex.filesrc import FileSource
from gamex import Family
from gamex.pak import PakFile
from gamex.meta import MetaManager, MetaInfo, MetaContent, IHaveMetaInfo
from gamex.Base.formats.binary import Binary_Dds, Binary_Img, Binary_Snd, Binary_Tga, Binary_Txt
from gamex.util import _pathExtension

# typedefs
class Reader: pass
class FileOption: pass
class FamilyGame: pass
class PakState: pass

# UnknownFamily
class UnknownFamily(Family):
    def __init__(self, elem: dict[str, object]):
        super().__init__(elem)

# UnknownPakFile
class UnknownPakFile(PakFile):
    def __init__(self, state: PakState):
        super().__init__(state)
        self.name = 'Unknown'
        self.objectFactoryFunc = self.objectFactory

    #region Factories

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (FileOption, callable):
        match _pathExtension(source.path).lower():
            case x if x == '.txt' or x == '.ini' or x == '.cfg' or x == '.xml': return (0, Binary_Txt.factory)
            case '.wav': return (0, Binary_Snd.factory)
            case x if x == '.bmp' or x == '.jpg': return (0, Binary_Img.factory)
            case '.tga': return (0, Binary_Tga.factory)
            case '.dds': return (0, Binary_Dds.factory)
            case _:
                match source.path:
                    case 'testtri.gfx': return (0, UnknownPakFile.Binary_TestTri.factory)
                    case _: return (0, None)

    #endregion

    #region Binary

    class Binary_TestTri(IHaveMetaInfo):
        @staticmethod
        def factory(r: Reader, f: FileSource, s: PakFile): return UnknownPakFile.Binary_TestTri(r)

        def __init__(self, r: Reader):
            pass

        def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
            MetaInfo(None, MetaContent(type = 'TestTri', name = os.path.basename(file.path), value = self))
            ]

    #endregion

