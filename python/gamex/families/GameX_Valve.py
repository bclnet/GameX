from __future__ import annotations
import os
from gamex import BinaryPakFile
from gamex.families.Valve.formats.binary import Binary_Bsp30, Binary_Src, Binary_Spr, Binary_Mdl10, Binary_Mdl40, Binary_Vpk, Binary_Wad3, Binary_Wad3X
from gamex.families.GameX import UnknownPakFile
from gamex.core.util import _pathExtension

#region ValvePakFile

# ValvePakFile
class ValvePakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))
        self.objectFactoryFunc = self.objectFactory
        # self.pathFinders.add(typeof(object), FindBinary)

    #region Factories
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> object:
        if extension == '.bsp': return PakBinary_Bsp30()
        match game.engine[0]:
            # case 'Unity': return Binary_Unity()
            case 'GoldSrc': return Binary_Wad3()
            case 'Source' | 'Source2': return Binary_Vpk()
            case _: raise Exception(f'Unknown: {game.engine[0]}')

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match game.engine[0]:
            case 'GoldSrc':
                match _pathExtension(source.path).lower():
                    case '.pic' | '.tex' | '.tex2' | '.fnt': return (0, Binary_Wad3X.factory)
                    case '.spr': return (0, Binary_Spr.factory)
                    case '.mdl': return (0, Binary_Mdl10.factory)
                    case _: return UnknownPakFile.objectFactory(source, game)
            case 'Source':
                match _pathExtension(source.path).lower():
                    case '.mdl': return (0, Binary_Mdl40.factory)
                    case _: return UnknownPakFile.objectFactory(source, game)
            case 'Source2': return (0, Binary_Src.factory)
            case _: raise Exception(f'Unknown: {game.engine[0]}')

    #endregion

#endregion