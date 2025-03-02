from __future__ import annotations
import os
from gamex import BinaryPakFile
from gamex.Cryptic.formats.binary import Binary_Hogg
from gamex.GameX import UnknownPakFile
from gamex.util import _pathExtension

# CrypticPakFile
class CrypticPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))

    #region Factories
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        return Binary_Hogg()

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (FileOption, callable):
        match _pathExtension(source.path).lower():
            case _: return UnknownPakFile.objectFactory(source, game)

    #endregion