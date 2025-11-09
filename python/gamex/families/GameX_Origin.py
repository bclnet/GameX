from __future__ import annotations
import os
from openstk import debug, _pathExtension
from gamex import FamilyGame, BinaryPakFile
from gamex.families.Origin.formats.UO.binary import Binary_Animdata, Binary_AsciiFont, Binary_BodyConverter, Binary_BodyTable, Binary_CalibrationInfo, Binary_Gump, Binary_GumpDef, Binary_Hues, Binary_Land, Binary_Light, Binary_MobType, Binary_MultiMap, Binary_MusicDef, Binary_Multi, Binary_RadarColor, Binary_SkillGroups, Binary_Skills, Binary_Sound, Binary_SpeechList, Binary_Static, Binary_StringTable, Binary_TileData, Binary_UnicodeFont, Binary_Verdata
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
        clientVersionText = self.options['clientVersion'] if 'clientVersion' in self.options else None
        if clientVersionText: clientVersionText = clientVersionText.replace(',', '.').replace(' ', '').lower()
        clientVersion = ClientVersionHelper.validateClientVersion(clientVersionText)
        if not clientVersion:
            debug.warn(f'Client version [{clientVersionText}] is invalid, let\'s try to read the client.exe')
            if (clientVersionText := ClientVersionHelper.parseFromFile(os.path.join(self.found.root, 'client.exe'))) == None or (clientVersion := ClientVersionHelper.validateClientVersion(clientVersionText)) == None:
                debug.error(f'Invalid client version: {clientVersionText}')
                raise Exception(f'Invalid client version: "{clientVersionText}"')
            debug.trace(f'Found a valid client.exe [{clientVersionText} - {clientVersion}]')
            self.options['clientVersion'] = clientVersionText
            self.options.dirty = True
        self.version = clientVersion
        self.protocol = ClientFlags.CF_T2A
        if self.version >= ClientVersion.CV_200: self.protocol |= ClientFlags.CF_RE
        if self.version >= ClientVersion.CV_300: self.protocol |= ClientFlags.CF_TD
        if self.version >= ClientVersion.CV_308: self.protocol |= ClientFlags.CF_LBR
        if self.version >= ClientVersion.CV_308Z: self.protocol |= ClientFlags.CF_AOS
        if self.version >= ClientVersion.CV_405A: self.protocol |= ClientFlags.CF_SE
        if self.version >= ClientVersion.CV_60144: self.protocol |= ClientFlags.CF_SA
        debug.trace(f'Client version: {clientVersion}')
        debug.trace(f'Protocol: {self.protocol}')

# OriginPakFile
class OriginPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game))
        self.objectFactoryFunc = self.objectFactory

    #region Factories

    @staticmethod
    def getPakBinary(game: FamilyGame) -> PakBinary:
        match game.id:
            case 'U8': return Binary_U8()
            case 'UO': return Binary_UO()
            case 'U9': return Binary_U9()
            case _: raise Exception(f'Unknown: {game.id}')
        
    @staticmethod
    def objectFactory(source: FileSource, game: FamilyGame) -> (object, callable):
        match game.id:
            case 'U8': return Binary_U8.objectFactory(source, game)
            case 'UO': return Binary_UO.objectFactory(source, game)
            case 'U9': return Binary_U9.objectFactory(source, game)
            case _: raise Exception(f'Unknown: {game.id}')

    #endregion