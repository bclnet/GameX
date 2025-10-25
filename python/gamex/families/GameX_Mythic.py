from __future__ import annotations
import os
from gamex import BinaryPakFile
from gamex.families.GameX import UnknownPakFile
from gamex.families.Mythic.formats.binary import Binary_Mpk, Binary_Crf
from gamex.families.Gamebryo.formats.binary import Binary_Nif
from gamex.core.util import _pathExtension

# MythicPakFile
class MythicPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))

    #region Factories
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        match extension:
            case '': return None
            case '.mpk' | '.npk': return Binary_Mpk()
            case _: raise Exception(f'Unknown: {extension}')

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case '.crf': return (FileOption.StreamObject, Binary_Crf.factory)
            case '.nif': return (FileOption.StreamObject, Binary_Nif.factory)
            case _: return UnknownPakFile.objectFactory(source, game)

    #endregion