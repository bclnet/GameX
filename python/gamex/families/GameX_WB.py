from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import FamilyGame, BinaryArchive
from gamex.families.GameX import UnknownArchive

# ACGame
class ACGame(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)
    def loaded(self):
        super().loaded()

# WBArchive
class WBArchive(BinaryArchive):
    def __init__(self, state: ArchiveState):
        super().__init__(state, self.getArcBinary(state.game, _pathExtension(state.path).lower()))
        self.assetFactoryFunc = self.assetFactory

    #region Factories

    @staticmethod
    def getArcBinary(game: FamilyGame, extension: str) -> object:
        pass

    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case _: return UnknownArchive.assetFactory(source, game)

    #endregion