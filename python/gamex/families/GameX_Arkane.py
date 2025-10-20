from __future__ import annotations
import os
from gamex import BinaryPakFile
from gamex.families.Arkane.formats.danae.binary import Binary_Ftl, Binary_Fts, Binary_Tea
from gamex.families.Arkane.formats.binary import Binary_Danae, Binary_Void
from gamex.families.Valve.formats.binary import Binary_Vpk
from gamex.families.GameX_Valve import ValvePakFile
from gamex.families.GameX import UnknownPakFile
from gamex.util import _pathExtension

#region ArkanePakFile

# ArkanePakFile
class ArkanePakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))
        match state.game.engine[0]:
            # case 'CryEngine': self.objectFactoryFunc = Crytek.CrytekPakFile.ObjectFactory
            # case 'Unreal': self.objectFactoryFunc = Epic.EpicPakFile.ObjectFactory
            case 'Valve': self.objectFactoryFunc = ValvePakFile.ObjectFactory
            # case 'idTech7': self.objectFactoryFunc = Id.IdPakFile.ObjectFactory
            case _: self.objectFactoryFunc = self.objectFactory
        self.useFileId = True

    #region Factories
        
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        match game.engine[0]:
            case 'Danae': return Binary_Danae()
            case 'Void': return Binary_Void()
            # case 'CryEngine': return Binary_Void()
            # case 'Unreal': return Binary_Void()
            case 'Valve': return Binary_Vpk()
            # case 'idTech7': return Binary_Void()
            case _: raise Exception(f'Unknown: {game.engine[0]}')

    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case '.asl': return (0, Binary_Txt.factory)
            # Danae (AF)
            case '.ftl': return (0, Binary_Ftl.factory)
            case '.fts': return (0, Binary_Fts.factory)
            case '.tea': return (0, Binary_Tea.factory)
            #
            #case ".llf": return (0, Binary_Flt.factory)
            #case ".dlf": return (0, Binary_Flt.factory)
            case _: return UnknownPakFile.objectFactory(source, game)

    #endregion

#endregion