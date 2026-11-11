from __future__ import annotations
import os
from openx.core import _pathExtension
from gamex import ArcBinary, Archive, BinaryArchive
from gamex.families.GameX_Uncore import UncoreArchive
from gamex.families.IW.formats import binary as iwbin

# IWArchive
class IWArchive(BinaryArchive):
    def __init__(self, parent: Archive, state: BinaryState):
        super().__init__(parent, state, self.getArcBinary(state.game, _pathExtension(state.path).lower()))
        self.assetFactoryFunc = self.assetFactory

    #region Factories

    @staticmethod
    def getArcBinary(game: FamilyGame, extension: str) -> ArcBinary:
        return iwbin.Binary_IW()

    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> tuple[object, callable]:
        match _pathExtension(source.path).lower():
            # .roq => VIDEO.factory
            # .wav => BinaryWav.factory
            # .d3dbsp => BinaryD3dbsp.factory
            case '.iwi': return (0, iwbin.Binary_Iwi.factory)
            case '.gsc': return (0, iwbin.Binary_Gsc.factory)
            case '.xmp': return (0, iwbin.Binary_Xmp.factory)
            case '.phy': return (0, iwbin.Binary_Phy.factory)
            case '.xan': return (0, iwbin.Binary_Xan.factory)
            case '.xmd': return (0, iwbin.Binary_Xmd.factory)
            case '.mat': return (0, iwbin.Binary_Mat.factory)
            case '.tec': return (0, iwbin.Binary_Tec.factory)
            case '.snd': return (0, iwbin.Binary_Snd.factory)
            case '.crv': return (0, iwbin.Binary_Crv.factory)
            case '.lds': return (0, iwbin.Binary_Lds.factory)
            case '.csp': return (0, iwbin.Binary_Csp.factory)
            case '.cmp': return (0, iwbin.Binary_Cmp.factory)
            case '.cmm': return (0, iwbin.Binary_Cmm.factory)
            case '.gsp': return (0, iwbin.Binary_Gsp.factory)
            case '.gmp': return (0, iwbin.Binary_Gmp.factory)
            case '.ent': return (0, iwbin.Binary_Ent.factory)
            case '.gfx': return (0, iwbin.Binary_Gfx.factory)
            case '.lgt': return (0, iwbin.Binary_Lgt.factory)
            case '.uim': return (0, iwbin.Binary_Uim.factory)
            case '.fnt': return (0, iwbin.Binary_Fnt.factory)
            case '.mnf': return (0, iwbin.Binary_Mnf.factory)
            case '.mnu': return (0, iwbin.Binary_Mnu.factory)
            case '.loc': return (0, iwbin.Binary_Loc.factory)
            case '.wep': return (0, iwbin.Binary_Wep.factory)
            case '.sdg': return (0, iwbin.Binary_Sdg.factory)
            case '.fx': return (0, iwbin.Binary_Fx.factory)
            case '.ifx': return (0, iwbin.Binary_Ifx.factory)
            case '.ait': return (0, iwbin.Binary_Ait.factory)
            case '.mpt': return (0, iwbin.Binary_Mpt.factory)
            case '.chr': return (0, iwbin.Binary_Chr.factory)
            case '.xma': return (0, iwbin.Binary_Xma.factory)
            case '.raw': return (0, iwbin.Binary_Raw.factory)
            case '.tbl': return (0, iwbin.Binary_Tbl.factory)
            case '.pcm': return (0, iwbin.Binary_Pcm.factory)
            case '.xms': return (0, iwbin.Binary_Xms.factory)
            case '.psh': return (0, iwbin.Binary_Psh.factory)
            case '.vsh': return (0, iwbin.Binary_Vsh.factory)
            case '.vdl': return (0, iwbin.Binary_Vdl.factory)
            case '.fxm': return (0, iwbin.Binary_Fxm.factory)
            case '.lbd': return (0, iwbin.Binary_Lbd.factory)
            case '.sdd': return (0, iwbin.Binary_Sdd.factory)
            case '.trc': return (0, iwbin.Binary_Trc.factory)
            case '.veh': return (0, iwbin.Binary_Veh.factory)
            case '.ame': return (0, iwbin.Binary_Ame.factory)
            case '.gls': return (0, iwbin.Binary_Gls.factory)
            case '.aip': return (0, iwbin.Binary_Aip.factory)
            case '.vtr': return (0, iwbin.Binary_Vtr.factory)
            case '.att': return (0, iwbin.Binary_Att.factory)
            case '.sfx': return (0, iwbin.Binary_Sfx.factory)
            case _: return UncoreArchive.assetFactory(source, game)
    #endregion
