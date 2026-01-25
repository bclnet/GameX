from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import ArcBinary, BinaryArchive
from gamex.families.GameX_Uncore import UncoreArchive
from gamex.families.Mythic.formats.binary import Binary_Mpk, Binary_Crf
from gamex.families.Bioware.formats.binary import Binary_Myp
from gamex.families.Gamebryo.formats.binary import Binary_Nif
from gamex.families.GameX_Bioware import BiowareArchive
from gamex.families.GameX_Origin import OriginArchive

# MythicArchive
class MythicArchive(BinaryArchive):
    def __init__(self, state: BinaryState):
        super().__init__(state, self.getArcBinary(state.game, _pathExtension(state.path).lower()))
        match state.game.id:
            case 'UO': self.assetFactoryFunc = OriginArchive.assetFactory
            case 'DA2': self.assetFactoryFunc = BiowareArchive.assetFactory
            case _: self.assetFactoryFunc = self.assetFactory

    #region Factories

    @staticmethod
    def getArcBinary(game: FamilyGame, extension: str) -> ArcBinary:
        match game.id:
            case 'UO': return OriginArchive.getArcBinary(game, extension)
            case 'DA2': return BiowareArchive.getArcBinary(game, extension)
            case _:
                match extension:
                    case '': return None
                    case '.mpk' | '.npk': return Binary_Mpk()
                    case '.myp' | '.npk': return Binary_Myp()
                    case _: raise Exception(f'Unknown: {extension}')

    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case '.crf': return (FileOption.StreamObject, Binary_Crf.factory)
            case '.nif': return (FileOption.StreamObject, Binary_Nif.factory)
            case _: return UncoreArchive.assetFactory(source, game)

    #endregion