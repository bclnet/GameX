from __future__ import annotations
import os
from openx.core import _pathExtension
from gamex import Archive, BinaryArchive, ITransformAsset
from gamex.families.Beamdog.formats.binary import Binary_XXX
from gamex.families.GameX_Uncore import UncoreArchive

# BeamdogArchive
class BeamdogArchive(BinaryArchive, ITransformAsset):
    def __init__(self, parent: Archive, state: BinaryState):
        super().__init__(parent, state, self.getArcBinary(state.game, _pathExtension(state.path).lower()))
        self.assetFactoryFunc = self.assetFactory

    #region Factories

    @staticmethod
    def getArcBinary(game: FamilyGame, extension: str) -> ArcBinary:
        match extension:
            case '.xxx': return Binary_XXX()
            case _: raise Exception(extension)

    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> tuple[object, callable]:
        match _pathExtension(source.path).lower():
            case _: return UncoreArchive.assetFactory(source, game)

    #endregion

    #region Transforms

    def canTransformAsset(self, source: object, transformTo: Archive) -> bool: return False
    def transformAsset(self, source: object, transformTo: Archive): raise NotImplementedError()

    #endregion