from __future__ import annotations
import os
from openstk import _pathExtension
from gamex import BinaryArchive
from gamex.families.Arkane.formats.danae.binary import Binary_Ftl, Binary_Fts, Binary_Tea
from gamex.families.Arkane.formats.binary import Binary_Danae, Binary_Void
from gamex.families.Valve.formats.binary import Binary_Vpk
from gamex.families.GameX_Valve import ValveArchive
from gamex.families.GameX_Uncore import UncoreArchive

# ArkaneArchive
class ArkaneArchive(BinaryArchive):
    def __init__(self, state: ArchiveState):
        super().__init__(state, self.getArcBinary(state.game, _pathExtension(state.path).lower()))
        match state.game.engine[0]:
            # case 'CryEngine': self.assetFactoryFunc = Crytek.CrytekArchive.AssetFactory
            # case 'Unreal': self.assetFactoryFunc = Epic.EpicArchive.AssetFactory
            case 'Valve': self.assetFactoryFunc = ValveArchive.AssetFactory
            # case 'idTech7': self.assetFactoryFunc = Id.IdArchive.AssetFactory
            case _: self.assetFactoryFunc = self.assetFactory
        self.useFileId = True

    #region Factories
        
    @staticmethod
    def getArcBinary(game: FamilyGame, extension: str) -> ArcBinary:
        match game.engine[0]:
            case 'Danae': return Binary_Danae()
            case 'Void': return Binary_Void()
            # case 'CryEngine': return Binary_Void()
            # case 'Unreal': return Binary_Void()
            case 'Valve': return Binary_Vpk()
            # case 'idTech7': return Binary_Void()
            case _: raise Exception(f'Unknown: {game.engine[0]}')

    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match _pathExtension(source.path).lower():
            case '.asl': return (0, Binary_Txt.factory)
            # Danae (AF)
            case '.ftl': return (0, Binary_Ftl.factory)
            case '.fts': return (0, Binary_Fts.factory)
            case '.tea': return (0, Binary_Tea.factory)
            #
            #case ".llf": return (0, Binary_Flt.factory)
            #case ".dlf": return (0, Binary_Flt.factory)
            case _: return UncoreArchive.assetFactory(source, game)

    #endregion