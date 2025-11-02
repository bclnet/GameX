from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import BinaryPakFile
from gamex.families.GameX import UnknownPakFile

# DGame
class DGame(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)
        self.objectFactoryFunc = self.objectFactory
    def loaded(self):
        super().loaded()

# D2Game
class D2Game(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)
        self.objectFactoryFunc = self.objectFactory
    def loaded(self):
        super().loaded()

# VolitionPakFile
class VolitionPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))
        self.objectFactoryFunc = self.objectFactory

    #region Factories

    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> object:
        pass

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case _: return UnknownPakFile.objectFactory(source, game)

    #endregion