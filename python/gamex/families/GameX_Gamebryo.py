from __future__ import annotations
import os
from gamex import Family, FamilyGame, BinaryPakFile, FileOption
from gamex.core.formats.binary import Binary_Dds
from gamex.families.Gamebryo.formats.binary import Binary_Nif
from gamex.families.GameX import UnknownPakFile
from gamex.util import _pathExtension

#region GamebryoPakFile

# GamebryoPakFile
class GamebryoPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))
        self.objectFactoryFunc = self.objectFactory

    #region Factories

    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        match extension:
            case _: raise Exception(f'Unknown: {extension}')

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case '.nif': return (FileOption.StreamObject, Binary_Nif.factory)
            case _: return UnknownPakFile.objectFactory(source, game)

    #endregion

#endregion