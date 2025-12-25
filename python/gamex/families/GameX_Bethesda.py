from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import Family, FamilyGame, BinaryArchive, FileOption
from gamex.families.Uncore.formats.binary import Binary_Dds
from gamex.families.Bethesda.formats.binary import Binary_Ba2, Binary_Bsa, Binary_Esm
from gamex.families.Gamebryo.formats.binary import Binary_Nif
from gamex.families.GameX_Uncore import UncoreArchive
from gamex.families.Bethesda.clients.Morrowind.client import MorrowindGameClient

# BethesdaFamily
# class BethesdaFamily(Family):
#     def __init__(self, elem: dict[str, object]):
#         super().__init__(elem)

# MorrowindGame
class MorrowindGame(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)

# BethesdaArchive
class BethesdaArchive(BinaryArchive):
    def __init__(self, state: ArchiveState):
        super().__init__(state, self.getArcBinary(state.game, _pathExtension(state.path).lower()))
        self.assetFactoryFunc = self.assetFactory

    #region Factories

    @staticmethod
    def getArcBinary(game: FamilyGame, extension: str) -> ArcBinary:
        match extension:
            case '': return None
            case '.bsa': return Binary_Bsa()
            case '.ba2': return Binary_Ba2()
            case '.esm': return Binary_Esm()
            case _: raise Exception(f'Unknown: {extension}')

    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case '.nif': return (FileOption.StreamObject, Binary_Nif.factory)
            case _: return UncoreArchive.assetFactory(source, game)

    #endregion