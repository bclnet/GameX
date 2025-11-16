from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import FamilyGame, BinaryPakFile
from gamex.families.GameX import UnknownPakFile
from gamex.families.Xbox.formats.binary import Binary_Xnb

# StardewGame
class StardewGame(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)
    def loaded(self):
        super().loaded()
        Binary_Xnb.ContentReader.add(Binary_Xnb.TypeReader[str]('BmFont.XmlSourceReader', 'System.String', lambda r: r.readLV7UString()))
        Binary_Xnb.ContentReader.add(Binary_Xnb.TypeReader[str]('StardewValley.GameData.BigCraftableData', 'System.String', lambda r: r.readLV7UString()))
        Binary_Xnb.ContentReader.add(Binary_Xnb.TypeReader[str]('BmFont.XmlSourceReader', 'System.String', lambda r: r.readLV7UString()))
        Binary_Xnb.ContentReader.add(Binary_Xnb.TypeReader[str]('BmFont.XmlSourceReader', 'System.String', lambda r: r.readLV7UString()))
        Binary_Xnb.ContentReader.add(Binary_Xnb.TypeReader[str]('BmFont.XmlSourceReader', 'System.String', lambda r: r.readLV7UString()))
        Binary_Xnb.ContentReader.add(Binary_Xnb.TypeReader[str]('BmFont.XmlSourceReader', 'System.String', lambda r: r.readLV7UString()))
        Binary_Xnb.ContentReader.add(Binary_Xnb.TypeReader[str]('BmFont.XmlSourceReader', 'System.String', lambda r: r.readLV7UString()))

# XboxPakFile
class XboxPakFile(BinaryPakFile):
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
            case '.xnb': return (0, Binary_Xnb.factory)
            case _: return UnknownPakFile.objectFactory(source, game)

    #endregion