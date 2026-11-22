from __future__ import annotations
import os
from openx.core import _pathExtension
from gamex import ArcBinary, Archive, BinaryArchive
from gamex.families.Black.formats.binary import Binary_Dat, Binary_Frm, Binary_Map, Binary_Pal2, Binary_Rix

# BlackArchive
class BlackArchive(BinaryArchive):
    def __init__(self, parent: Archive, state: BinaryState):
        super().__init__(parent, state, self.getArcBinary(state.game, _pathExtension(state.path).lower()))
        self.assetFactoryFunc = self.assetFactory

    #region Factories

    @staticmethod
    def getArcBinary(game: FamilyGame, extension: str) -> ArcBinary:
        match extension:
            case '.dat': return Binary_Dat()
            case _: raise Exception(extension)

    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> tuple[object, callable]:
        match _pathExtension(source.path).lower():
            case x if x.startswith('.fr'): return (0, Binary_Frm.factory)
            case '.pal': return (0, Binary_Pal2.factory)
            case '.rix': return (0, Binary_Rix.factory)
            case '.map': return (0, Binary_Map.factory)
            case _: return (0, None)

    #endregion
