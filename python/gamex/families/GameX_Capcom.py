from __future__ import annotations
import os
from openstk.core import _pathExtension
from gamex import ArcBinary, Archive, BinaryArchive
from gamex.families.Uncore.formats.binary import Binary_Zip
from gamex.families.Capcom.formats.binary import Binary_Arc, Binary_Big, Binary_Bundle, Binary_Kpka, Binary_Plist
from gamex.families.Unity.formats.binary import Binary_Unity
from gamex.families.GameX_Uncore import UncoreArchive

# CapcomArchive
class CapcomArchive(BinaryArchive):
    def __init__(self, parent: Archive, state: BinaryState):
        super().__init__(parent, state, self.getArcBinary(state.game, _pathExtension(state.path).lower()))
        self.assetFactoryFunc = self.assetFactory

    #region Factories

    @staticmethod
    def getArcBinary(game: FamilyGame, extension: str) -> ArcBinary:
        if not extension: return None
        elif extension == '.pie': return Binary_Zip()
        match game.engine[0]:
            case 'Unity': return Binary_Unity()
        match extension:
            case '.arc': return Binary_Kpka()
            case '.arc': return Binary_Arc()
            case '.big': return Binary_Big()
            case '.bundle': return Binary_Bundle()
            case '.mbundle': return Binary_Plist()
            case _: raise Exception(f'Unknown: {extension}')

    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> tuple[object, callable]:
        match _pathExtension(source.path).lower():
            case _: return UncoreArchive.assetFactory(source, game)

    #endregion
