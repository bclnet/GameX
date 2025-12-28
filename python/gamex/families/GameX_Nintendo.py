from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import BinaryArchive
from gamex.families.GameX_Uncore import UncoreArchive

# NintendoArchive
class NintendoArchive(BinaryArchive):
    def __init__(self, state: BinaryState):
        super().__init__(state, self.getArcBinary(state.game, _pathExtension(state.path).lower()))
        self.assetFactoryFunc = self.assetFactory

    #region Factories

    @staticmethod
    def getArcBinary(game: FamilyGame, extension: str) -> ArcBinary:
        pass

    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case _: return UncoreArchive.assetFactory(source, game)

    #endregion