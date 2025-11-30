from __future__ import annotations
import os
from openstk import _pathExtension, TypeX
from gamex import FamilyGame, BinaryPakFile
from gamex.families.GameX import UnknownPakFile
from gamex.families.Xbox.formats.binary import Binary_Xnb
# scan types
from gamex.families.Xbox.formats.xna import TypeReader as xna_TypeReader
from gamex.families.Xbox.formats.xtile import Map as xtile_Map
from gamex.families.Xbox.formats.AxiomVerge2.OuterBeyond import THTileMapReader as AxiomVerge2_THTileMapReader
from gamex.families.Xbox.formats.StardewValley.BmFont import XmlSourceReader as StardewValley_XmlSourceReader
from gamex.families.Xbox.formats.StardewValley.GameData import Gender as StardewValley_Gender
typesToScan = [xna_TypeReader, xtile_Map, AxiomVerge2_THTileMapReader, StardewValley_XmlSourceReader, StardewValley_Gender]

# StardewValleyGame
class StardewValleyGame(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)

# XboxPakFile
class XboxPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))
        self.objectFactoryFunc = self.objectFactory
        TypeX.scanTypes(typesToScan)
        
    #region Factories

    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> object:
        pass

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case '.xnb': return (0, Binary_Xnb.factory)
            case _: return UnknownPakFile.objectFactory(source, game)

    #endregion