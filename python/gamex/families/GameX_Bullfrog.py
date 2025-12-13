from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import Family, FamilyGame, BinaryArchive
from gamex.families.Bullfrog.formats.binary import Binary_Bullfrog, Binary_Populus, Binary_Syndicate
from gamex.families.GameX import UnknownArchive

# DKGame
class DKGame(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)

    def loaded(self):
        super().loaded()
        #DK_Database.loaded(self)

# DK2Game
class DK2Game(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)

    def loaded(self):
        super().loaded()
        #DK2_Database.loaded(self)

# P2Game
class P2Game(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)

    def loaded(self):
        super().loaded()
        #PK_Database.loaded(self)

# SGame
class SGame(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)

    def loaded(self):
        super().loaded()
        #S_Database.loaded(self)

# BullfrogArchive
class BullfrogArchive(BinaryArchive):
    def __init__(self, state: ArchiveState):
        super().__init__(state, self.getArcBinary(state.game, state.path))
        self.assetFactoryFunc = self.assetFactory

    #region Factories

    @staticmethod
    def getArcBinary(game: FamilyGame, filePath: str) -> ArcBinary:
        match game.id:
            case 'DK' | 'DK2': return Binary_Bullfrog()            # Keeper
            case 'P' | 'P2' | 'P3': return Binary_Populus()        # Populs
            case 'S' | 'S2': return Binary_Syndicate()             # Syndicate
            case 'MC' | 'MC2': return Binary_Bullfrog()            # Carpet
            case 'TP' | 'TH': return Binary_Bullfrog()             # Theme
            case _: raise Exception(f'Unknown: {game.id}')

    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match game.id:
            case 'DK' | 'DK2': return Binary_Bullfrog.assetFactory(source, game)
            case 'P' | 'P2' | 'P3': return Binary_Populus.assetFactory(source, game)
            case 'S' | 'S2': return Binary_Syndicate.assetFactory(source, game)
            case _: raise Exception(f'Unknown: {game.id}')

    #endregion