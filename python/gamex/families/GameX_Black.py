from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import BinaryArchive
from gamex.families.Black.formats.binary import Binary_Dat
from gamex.families.GameX import UnknownArchive

# BlackArchive
class BlackArchive(BinaryArchive):
    def __init__(self, state: ArcState):
        super().__init__(state, self.getArcBinary(state.game, _pathExtension(state.path).lower()))
        self.assetFactoryFunc = self.assetFactory

    #region Factories

    @staticmethod
    def getArcBinary(game: FamilyGame, extension: str) -> ArcBinary:
        return Binary_Dat()

    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case _: return UnknownArchive.assetFactory(source, game)

    #endregion
