from __future__ import annotations
import os
from gamex import BinaryPakFile
from gamex.Base.formats.binary import Binary_Zip
from gamex.Bioware.formats.binary import Binary_Aurora, Binary_Myp
from gamex.GameX import UnknownPakFile
from gamex.util import _pathExtension

# BiowarePakFile
class BiowarePakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))

    #region Factories
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        if extension == '.zip': return Binary_Zip()
        match game.engine[0]:
            case 'Aurora': return Binary_Aurora()
            case 'HeroEngine': return Binary_Myp()
            case _: raise Exception(f'Unknown: {game.engine[0]}')

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (FileOption, callable):
        match _pathExtension(source.path).lower():
            case _: return UnknownPakFile.objectFactory(source, game)

    #endregion
