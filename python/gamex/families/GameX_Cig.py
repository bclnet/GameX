from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import BinaryArchive
from gamex.families.Cig.formats.binary import Binary_P4k
from gamex.families.GameX_Uncore import UncoreArchive

# CigArchive
class CigArchive(BinaryArchive):
    def __init__(self, state: BinaryState):
        super().__init__(state, Binary_P4k())
        self.assetFactoryFunc = self.assetFactory

    #region Factories

    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case _: return UncoreArchive.assetFactory(source, game)

    #endregion

class DataForgeApp:
    def __init__(self, family: Family, id: str, elem: object): pass
class StarWordsApp:
    def __init__(self, family: Family, id: str, elem: object): pass
class SubsumptionApp:
    def __init__(self, family: Family, id: str, elem: object): pass
    