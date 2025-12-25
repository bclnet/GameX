from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import BinaryArchive
from gamex.families.Uncore.formats.binary import Binary_Zip
from gamex.families.Bioware.formats.binary import Binary_Aurora, Binary_Myp
from gamex.families.GameX_Uncore import UncoreArchive

# BiowareArchive
class BiowareArchive(BinaryArchive):
    def __init__(self, state: ArchiveState):
        super().__init__(state, self.getArcBinary(state.game, _pathExtension(state.path).lower()))
        self.assetFactoryFunc = self.assetFactory

    #region Factories

    @staticmethod
    def getArcBinary(game: FamilyGame, extension: str) -> ArcBinary:
        if extension == '.zip': return Binary_Zip()
        # print(game.engine)
        match game.engine[0]:
            case 'Aurora': return Binary_Aurora()
            case 'Hero': return Binary_Myp()
            case 'Odyssey': return Binary_Myp()
            case _: raise Exception(f'Unknown: {game.engine[0]}')

    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case _: return UncoreArchive.assetFactory(source, game)

    #endregion
