from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import BinaryPakFile
from gamex.core.formats.binary import Binary_Zip
from gamex.families.Bioware.formats.binary import Binary_Aurora, Binary_Myp
from gamex.families.GameX import UnknownPakFile

# BiowarePakFile
class BiowarePakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))
        self.objectFactoryFunc = self.objectFactory

    #region Factories

    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        if extension == '.zip': return Binary_Zip()
        # print(game.engine)
        match game.engine[0]:
            case 'Aurora': return Binary_Aurora()
            case 'Hero': return Binary_Myp()
            case 'Odyssey': return Binary_Myp()
            case _: raise Exception(f'Unknown: {game.engine[0]}')

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case _: return UnknownPakFile.objectFactory(source, game)

    #endregion
