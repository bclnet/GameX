from __future__ import annotations
import os
from openx.core import _pathExtension
from gamex import Family, FamilyGame, ArcBinary, Archive, BinaryArchive, ITransformAsset
from gamex.families.Bullfrog.formats.binary import Binary_Bullfrog, Binary_Populus, Binary_Syndicate
from gamex.families.Bullfrog.games.DK.database import Database as DK_Database
from gamex.families.Bullfrog.games.DK2.database import Database as DK2_Database
from gamex.families.Bullfrog.games.P2.database import Database as P2_Database
from gamex.families.Bullfrog.games.S.database import Database as S_Database
from gamex.families.GameX_Uncore import UncoreArchive

# DKGame
class DKGame(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)

    def loaded(self):
        super().loaded()
        DK_Database.loaded(self)

# DK2Game
class DK2Game(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)

    def loaded(self):
        super().loaded()
        DK2_Database.loaded(self)

# P2Game
class P2Game(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)

    def loaded(self):
        super().loaded()
        P2_Database.loaded(self)

# SGame
class SGame(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)

    def loaded(self):
        super().loaded()
        S_Database.loaded(self)

# BullfrogArchive
class BullfrogArchive(BinaryArchive, ITransformAsset):
    def __init__(self, parent: Archive, state: BinaryState):
        super().__init__(parent, state, self.getArcBinary(state.game, state.path))
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
    def assetFactory(source: FileSource, game: FamilyGame) -> tuple[object, callable]:
        match game.id:
            case 'DK' | 'DK2': return Binary_Bullfrog.assetFactory(source, game)
            case 'P' | 'P2' | 'P3': return Binary_Populus.assetFactory(source, game)
            case 'S' | 'S2': return Binary_Syndicate.assetFactory(source, game)
            case _: raise Exception(f'Unknown: {game.id}')

    #endregion

    #region Transforms

    def canTransformAsset(self, source: object, transformTo: Archive) -> bool: return False
    def transformAsset(self, source: object, transformTo: Archive): raise NotImplementedError()

    #endregion