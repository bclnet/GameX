from __future__ import annotations
import os
from gamex import Family, FamilyGame, BinaryPakFile, FileOption
from gamex.core.formats.binary import Binary_Dds
from gamex.families.Bethesda.formats.binary import Binary_Ba2, Binary_Bsa, Binary_Esm
from gamex.families.Gamebryo.formats.binary import Binary_Nif
from gamex.families.GameX import UnknownPakFile
from gamex.core.util import _pathExtension

# BethesdaFamily
class BethesdaFamily(Family):
    def __init__(self, elem: dict[str, object]):
        super().__init__(elem)

# BethesdaGame
class BethesdaGame(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)

# BethesdaPakFile
class BethesdaPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))
        self.objectFactoryFunc = self.objectFactory

    #region Factories

    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        match extension:
            case '': return None
            case '.bsa': return Binary_Bsa()
            case '.ba2': return Binary_Ba2()
            case '.esm': return Binary_Esm()
            case _: raise Exception(f'Unknown: {extension}')

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case '.nif': return (FileOption.StreamObject, Binary_Nif.factory)
            case _: return UnknownPakFile.objectFactory(source, game)

    #endregion