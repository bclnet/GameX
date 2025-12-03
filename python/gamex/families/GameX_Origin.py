from __future__ import annotations
import os
from openstk import log, _pathExtension
from gamex import FamilyGame, BinaryArchive
from gamex.families.Origin.formats.UO.binary import Binary_Animdata, Binary_AsciiFont, Binary_BodyConverter, Binary_BodyTable, Binary_CalibrationInfo, Binary_Gump, Binary_GumpDef, Binary_Hues, Binary_Land, Binary_Light, Binary_MobType, Binary_MultiMap, Binary_MusicDef, Binary_Multi, Binary_RadarColor, Binary_SkillGroups, Binary_Skills, Binary_Sound, Binary_SpeechList, Binary_Art, Binary_StringTable, Binary_TileData, Binary_UnicodeFont, Binary_Verdata
from gamex.families.Origin.formats.UO.utility import ClientVersion, ClientVersionHelper
from gamex.families.Origin.formats.binary import Binary_U8, Binary_U9, Binary_UO
from gamex.families.Origin.clients.UO.data import ClientFlags

# U8Game
class U8Game(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)

# U9Game
class U9Game(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)

# UOGame
class UOGame(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)
    def loaded(self):
        super().loaded()
        versionText = self.options['version'] if 'version' in self.options else None
        if versionText: versionText = versionText.replace(',', '.').replace(' ', '').lower()
        version = ClientVersionHelper.validateClientVersion(versionText)
        if not version:
            log.warn(f'Client version [{versionText}] is invalid, let\'s try to read the client.exe')
            if (versionText := ClientVersionHelper.parseFromFile(os.path.join(self.found.root, 'client.exe'))) == None or (version := ClientVersionHelper.validateClientVersion(versionText)) == None:
                log.error(f'Invalid client version: {versionText}')
                raise Exception(f'Invalid client version: "{versionText}"')
            log.trace(f'Found a valid client.exe [{versionText} - {version}]')
            self.options['version'] = versionText
            self.options.dirty = True
        self.uop = version >= ClientVersion.CV_7000 and os.path.exists(os.path.join(self.found.root, 'MainMisc.uop'))
        self.version = version
        self.protocol = ClientFlags.CF_T2A
        if self.version >= ClientVersion.CV_200: self.protocol |= ClientFlags.CF_RE
        if self.version >= ClientVersion.CV_300: self.protocol |= ClientFlags.CF_TD
        if self.version >= ClientVersion.CV_308: self.protocol |= ClientFlags.CF_LBR
        if self.version >= ClientVersion.CV_308Z: self.protocol |= ClientFlags.CF_AOS
        if self.version >= ClientVersion.CV_405A: self.protocol |= ClientFlags.CF_SE
        if self.version >= ClientVersion.CV_60144: self.protocol |= ClientFlags.CF_SA
        log.trace(f'Uop: {self.uop}')
        log.trace(f'Version: {self.version}')
        log.trace(f'Protocol: {self.protocol}')

# OriginArchive
class OriginArchive(BinaryArchive):
    def __init__(self, state: ArcState):
        super().__init__(state, self.getArcBinary(state.game))
        self.assetFactoryFunc = self.assetFactory

    #region Factories

    @staticmethod
    def getArcBinary(game: FamilyGame) -> ArcBinary:
        match game.id:
            case 'U8': return Binary_U8()
            case 'UO': return Binary_UO()
            case 'U9': return Binary_U9()
            case _: raise Exception(f'Unknown: {game.id}')
        
    @staticmethod
    def assetFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match game.id:
            case 'U8': return Binary_U8.assetFactory(source, game)
            case 'UO': return Binary_UO.assetFactory(source, game)
            case 'U9': return Binary_U9.assetFactory(source, game)
            case _: raise Exception(f'Unknown: {game.id}')

    #endregion