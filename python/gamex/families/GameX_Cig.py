from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import BinaryPakFile
from gamex.families.Cig.formats.binary import Binary_P4k
from gamex.families.GameX import UnknownPakFile

# CigPakFile
class CigPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, Binary_P4k())
        self.objectFactoryFunc = self.objectFactory

    #region Factories

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case _: return UnknownPakFile.objectFactory(source, game)

    #endregion
