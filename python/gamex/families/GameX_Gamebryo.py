from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import Family, FamilyGame, BinaryArchive, FileOption
from gamex.families.Uncore.formats.binary import Binary_Dds
from gamex.families.Gamebryo.formats.binary import Binary_Nif
from gamex.families.GameX_Uncore import UncoreArchive

# GamebryoArchive
class GamebryoArchive(BinaryArchive):
    def __init__(self, state: BinaryState):
        super().__init__(state, self.getArcBinary(state.game, _pathExtension(state.path).lower()))
        self.assetFactoryFunc = self.assetFactory

    #region Factories

    @staticmethod
    def getArcBinary(game: FamilyGame, extension: str) -> ArcBinary:
        match extension:
            case _: raise Exception(f'Unknown: {extension}')

    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case '.nif': return (FileOption.StreamObject, Binary_Nif.factory)
            case _: return UncoreArchive.assetFactory(source, game)

    #endregion