from __future__ import annotations
import os
from gamex import BinaryPakFile
from gamex.core.formats.binary import Binary_Zip
from gamex.families.Capcom.formats.binary import Binary_Arc, Binary_Big, Binary_Bundle, Binary_Kpka, Binary_Plist
from gamex.families.Unity.formats.binary import Binary_Unity
from gamex.util import _pathExtension

# CapcomPakFile
class CapcomPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))

    #region Factories
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        if not extension: return None
        elif extension == '.pie': return Binary_Zip()
        match game.engine[0]:
            case 'Unity': return Binary_Unity()
        match extension:
            case '.pak': return Binary_Kpka()
            case '.arc': return Binary_Arc()
            case '.big': return Binary_Big()
            case '.bundle': return Binary_Bundle()
            case '.mbundle': return Binary_Plist()
            case _: raise Exception(f'Unknown: {extension}')

    #endregion
