from __future__ import annotations
import os
from gamex import BinaryPakFile
from gamex.families.Black.formats.binary import Binary_Dat
from gamex.families.GameX import UnknownPakFile
from gamex.core.util import _pathExtension

# BlackPakFile
class BlackPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))

    #region Factories
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        return Binary_Dat()

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case _: return UnknownPakFile.objectFactory(source, game)

    #endregion
