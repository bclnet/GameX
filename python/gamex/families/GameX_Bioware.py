from __future__ import annotations
import os
from openx.core import _pathExtension
from gamex import Archive, BinaryArchive
from gamex.families.Uncore.formats.binary import Binary_Zip
from gamex.families.Bioware.formats.binary import Binary_Aurora, Binary_Gff, Binary_Key, Binary_Myp
from gamex.families.GameX_Uncore import UncoreArchive

# BiowareArchive
class BiowareArchive(BinaryArchive):
    def __init__(self, parent: Archive, state: BinaryState):
        super().__init__(parent, state, self.getArcBinary(state.game, _pathExtension(state.path).lower()))
        self.assetFactoryFunc = self.assetFactory

    #region Factories

    @staticmethod
    def getArcBinary(game: FamilyGame, extension: str) -> ArcBinary:
        engine = game.engine
        name = engine[0] if engine else None
        match name, extension:
            case 'Infinity', '.key' | '.bif': return Binary_Key()
            case 'Aurora' | 'Odyssey', '.key' | '.bif': return Binary_Aurora()
            case 'Omen' | 'Aurora', '.zip': return Binary_Zip() if game.key is None else Binary_Zip(game.key)
            case 'Hero', '.tor': return Binary_Myp()
            case _: raise Exception(extension)

    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> tuple[object, callable]:
        match _pathExtension(source.path).lower():
            case '.dlg' | '.qdb' | '.qst': return (0, Binary_Gff.factory)
            case _: return UncoreArchive.assetFactory(source, game)

    #endregion
