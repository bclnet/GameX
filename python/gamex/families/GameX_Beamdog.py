from __future__ import annotations
import os
from gamex import BinaryPakFile
from gamex.families.GameX import UnknownPakFile
from gamex.core.util import _pathExtension

# BeamdogPakFile
class BeamdogPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))
        self.objectFactoryFunc = self.objectFactory

    #region Factories
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        pass

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case _: return UnknownPakFile.objectFactory(source, game)

    #endregion