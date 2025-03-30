from __future__ import annotations
import os
from gamex import Family, FamilyGame, BinaryPakFile
from gamex.Base.formats.binary import Binary_Dds
from gamex.Bethesda.formats.binary import Binary_Ba2, Binary_Bsa, Binary_Esm
from gamex.GameX import UnknownPakFile
from gamex.util import _pathExtension

#region BethesdaFamily

# BethesdaFamily
class BethesdaFamily(Family):
    def __init__(self, elem: dict[str, object]):
        super().__init__(elem)

#endregion

#region BethesdaGame

# BethesdaGame
class BethesdaGame(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)

#endregion

#region BethesdaPakFile

# BethesdaPakFile
class BethesdaPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))
        self.objectFactoryFunc = self.objectFactory

    #region Factories
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        match extension:
            case '': return Binary_Bsa()
            case '.bsa': return Binary_Bsa()
            case '.ba2': return Binary_Ba2()
            case '.esm': return Binary_Esm()
            case _: raise Exception(f'Unknown: {extension}')

    # @staticmethod
    # def NiFactory(r: Reader, f: FileSource, s: PakFile): file = NiFile(Path.GetFileNameWithoutExtension(f.Path)); file.Read(r); return file

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case '.dds': return (0, Binary_Dds.factory)
            # case '.nif': return (0, NiFactory)
            case _: return UnknownPakFile.objectFactory(source, game)

    #endregion

#endregion