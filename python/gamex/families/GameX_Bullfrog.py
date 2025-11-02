from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import Family, FamilyGame, BinaryPakFile
from gamex.families.Bullfrog.formats.binary import Binary_Bullfrog, Binary_Populus, Binary_Syndicate
from gamex.families.GameX import UnknownPakFile

# DKGame
class DKGame(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)
        self.objectFactoryFunc = self.objectFactory

    def loaded(self):
        super().loaded()
        #DK_Database.loaded(self)

# DK2Game
class DK2Game(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)
        self.objectFactoryFunc = self.objectFactory

    def loaded(self):
        super().loaded()
        #DK2_Database.loaded(self)

# P2Game
class P2Game(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)
        self.objectFactoryFunc = self.objectFactory

    def loaded(self):
        super().loaded()
        #PK_Database.loaded(self)

# SGame
class SGame(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)
        self.objectFactoryFunc = self.objectFactory

    def loaded(self):
        super().loaded()
        #S_Database.loaded(self)

# BullfrogPakFile
class BullfrogPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, state.path))
        self.objectFactoryFunc = self.objectFactory

    #region Factories

    @staticmethod
    def getPakBinary(game: FamilyGame, filePath: str) -> PakBinary:
        match game.id:
            case 'DK' | 'DK2': return Binary_Bullfrog()            # Keeper
            case 'P' | 'P2' | 'P3': return Binary_Populus()        # Populs
            case 'S' | 'S2': return Binary_Syndicate()             # Syndicate
            case 'MC' | 'MC2': return Binary_Bullfrog()            # Carpet
            case 'TP' | 'TH': return Binary_Bullfrog()             # Theme
            case _: raise Exception(f'Unknown: {game.id}')

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match game.id:
            case 'DK' | 'DK2': return Binary_Bullfrog.objectFactory(source, game)
            case 'P' | 'P2' | 'P3': return Binary_Populus.objectFactory(source, game)
            case 'S' | 'S2': return Binary_Syndicate.objectFactory(source, game)
            case _: raise Exception(f'Unknown: {game.id}')

    #endregion