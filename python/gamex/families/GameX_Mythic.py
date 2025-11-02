from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import BinaryPakFile
from gamex.families.GameX import UnknownPakFile
from gamex.families.Mythic.formats.binary import Binary_Mpk, Binary_Crf
from gamex.families.Bioware.formats.binary import Binary_Myp
from gamex.families.Gamebryo.formats.binary import Binary_Nif
from gamex.families.GameX_Bioware import BiowarePakFile
from gamex.families.GameX_Origin import OriginPakFile

# MythicPakFile
class MythicPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))
        match state.game.id:
            case 'UO': self.objectFactoryFunc = OriginPakFile.objectFactory
            case 'DA2': self.objectFactoryFunc = BiowarePakFile.objectFactory
            case _: self.objectFactoryFunc = self.objectFactory

    #region Factories

    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        match game.id:
            case 'UO': return OriginPakFile.getPakBinary(game, extension)
            case 'DA2': return BiowarePakFile.getPakBinary(game, extension)
            case _:
                match extension:
                    case '': return None
                    case '.mpk' | '.npk': return Binary_Mpk()
                    case '.myp' | '.npk': return Binary_Myp()
                    case _: raise Exception(f'Unknown: {extension}')

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case '.crf': return (FileOption.StreamObject, Binary_Crf.factory)
            case '.nif': return (FileOption.StreamObject, Binary_Nif.factory)
            case _: return UnknownPakFile.objectFactory(source, game)

    #endregion