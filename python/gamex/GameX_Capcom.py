from __future__ import annotations
import os
from gamex import BinaryPakFile
from gamex.Base.formats.pakbinary import PakBinary_Zip
from gamex.Capcom.pakbinary_arc import PakBinary_Arc
from gamex.Capcom.pakbinary_big import PakBinary_Big
from gamex.Capcom.pakbinary_bundle import PakBinary_Bundle
from gamex.Capcom.pakbinary_kpka import PakBinary_Kpka
from gamex.Capcom.pakbinary_plist import PakBinary_Plist
from gamex.Unity.pakbinary_unity import PakBinary_Unity
from gamex.util import _pathExtension

# CapcomPakFile
class CapcomPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))

    #region Factories
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        if not extension: return None
        elif extension == '.pie': return PakBinary_Zip()
        match game.engine[0]:
            case 'Unity': return PakBinary_Unity()
        match extension:
            case '.pak': return PakBinary_Kpka()
            case '.arc': return PakBinary_Arc()
            case '.big': return PakBinary_Big()
            case '.bundle': return PakBinary_Bundle()
            case '.mbundle': return PakBinary_Plist()
            case _: raise Exception(f'Unknown: {extension}')

    #endregion
