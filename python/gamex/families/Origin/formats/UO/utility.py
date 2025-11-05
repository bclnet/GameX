import os
from enum import IntFlag
from openstk import _int_tryParse

#region ClientVersion

# ClientVersion
class ClientVersion(IntFlag):
    CV_OLD = (1 << 24) | (0 << 16) | (0 << 8) | 0             # Original game
    CV_200 = (2 << 24) | (0 << 16) | (0 << 8) | 0             # T2A Introduction. Adds screen dimensions packet
    CV_200X = (2 << 24) | (0 << 16) | (0 << 8) | (ord('x') & 0xFF) # T2A Introduction. Adds screen dimensions packet
    CV_204C = (2 << 24) | (0 << 16) | (4 << 8) | (ord('c') & 0xFF) # Adds *.def files
    CV_207 = (2 << 24) | (0 << 16) | (7 << 8) | 0             # Adds *.def files
    CV_300 = (3 << 24) | (0 << 16) | (0 << 8) | 0
    CV_305D = (3 << 24) | (0 << 16) | (5 << 8) | (ord('d') & 0xFF) # Renaissance. Expanded character slots.
    CV_306E = (3 << 24) | (0 << 16) | (0 << 8) | (ord('e') & 0xFF) # Adds a packet with the client type, switches to mp3 from midi for sound files
    CV_308 = (3 << 24) | (0 << 16) | (8 << 8) | 0
    CV_308D = (3 << 24) | (0 << 16) | (8 << 8) | (ord('d') & 0xFF)   # Adds maximum stats to the status bar
    CV_308J = (3 << 24) | (0 << 16) | (8 << 8) | (ord('j') & 0xFF)   # Adds followers to the status bar
    CV_308Z = (3 << 24) | (0 << 16) | (8 << 8) | (ord('z') & 0xFF)   # Age of Shadows. Adds paladin, necromancer, custom housing, resists, profession selection window, removes save password checkbox
    CV_400B = (4 << 24) | (0 << 16) | (0 << 8) | (ord('b') & 0xFF)   # Deletes tooltips
    CV_405A = (4 << 24) | (0 << 16) | (5 << 8) | (ord('a') & 0xFF)   # Adds ninja, samurai
    CV_4011C = (4 << 24) | (0 << 16) | (11 << 8) | (ord('c') & 0xFF) # Music/* vs Music/Digital/* switchover
    CV_4011D = (4 << 24) | (0 << 16) | (11 << 8) | (ord('d') & 0xFF) # Adds elven race
    CV_500A = (5 << 24) | (0 << 16) | (0 << 8) | (ord('a') & 0xFF)   # Paperdoll buttons journal becomes quests, chat becomes guild. Use mega FileManager.Cliloc. Removes verdata.mul.
    CV_5020 = (5 << 24) | (0 << 16) | (2 << 8) | 0              # Adds buff bar
    CV_5090 = (5 << 24) | (0 << 16) | (9 << 8) | 0              #
    CV_6000 = (6 << 24) | (0 << 16) | (0 << 8) | 0              # Adds colored guild/all chat and ignore system. New targeting systems, object properties and handles.
    CV_6013 = (6 << 24) | (0 << 16) | (1 << 8) | 3              #
    CV_6017 = (6 << 24) | (0 << 16) | (1 << 8) | 8              #
    CV_6040 = (6 << 24) | (0 << 16) | (4 << 8) | 0              # Increased number of player slots
    CV_6060 = (6 << 24) | (0 << 16) | (6 << 8) | 0              #
    CV_60142 = (6 << 24) | (0 << 16) | (14 << 8) | 2            #
    CV_60143 = (6 << 24) | (0 << 16) | (14 << 8) | 3            # Stygian Abyss
    CV_60144 = (6 << 24) | (0 << 16) | (14 << 8) | 4            # Adds gargoyle race.
    CV_7000 = (7 << 24) | (0 << 16) | (0 << 8) | 0              #
    CV_7090 = (7 << 24) | (0 << 16) | (9 << 8) | 0              # high seas
    CV_70130 = (7 << 24) | (0 << 16) | (13 << 8) | 0            #
    CV_70160 = (7 << 24) | (0 << 16) | (16 << 8) | 0            #
    CV_70180 = (7 << 24) | (0 << 16) | (18 << 8) | 0            #
    CV_70240 = (7 << 24) | (0 << 16) | (24 << 8) | 0            # *.mul -> *.uop
    CV_70331 = (7 << 24) | (0 << 16) | (33 << 8) | 1            #
    CV_704565 = (7 << 24) | (0 << 16) | (45 << 8) | 65          #
    CV_705301 = (7 << 24) | (0 << 16) | (53 << 8) | 1          # Alternate backpack skins
    CV_706000 = (7 << 24) | (0 << 16) | (60 << 8) | 0
    CV_706400 = (7 << 24) | (0 << 16) | (64 << 8) | 0           # Endless Journey background
    CV_70796 = (7 << 24) | (0 << 16) | (79 << 8) | 6            # Display houses content option
    CV_7010400 = (7 << 24) | (0 << 16) | (104 << 8) | 0         # new file format

# ClientVersionHelper
class ClientVersionHelper:
    @staticmethod
    def parseFromFile(clientpath: str) -> str:
        if not os.path.isfile(clientpath): return None
        try:
            with open(clientpath, 'rb') as f:
                b = f.read()
                # VS_VERSION_INFO (unicode)
                vsVersionInfo = bytearray([
                    0x56, 0x00, 0x53, 0x00, 0x5F, 0x00, 0x56, \
                    0x00, 0x45, 0x00, 0x52, 0x00, 0x53, 0x00, \
                    0x49, 0x00, 0x4F, 0x00, 0x4E, 0x00, 0x5F, \
                    0x00, 0x49, 0x00, 0x4E, 0x00, 0x46, 0x00, \
                    0x4F, 0x00])
                vsVersionInfoLength = 30
                for i in range(len(b) - vsVersionInfoLength):
                    if vsVersionInfo == b[i:i + vsVersionInfoLength]:
                        offset = i + 42 # 30 + 12
                        minor = int.from_bytes(b[offset + 0:offset + 2], 'little', signed=False)
                        major = int.from_bytes(b[offset + 2:offset + 4], 'little', signed=False)
                        privatex = int.from_bytes(b[offset + 4:offset + 6], 'little', signed=False)
                        build = int.from_bytes(b[offset + 6:offset + 8], 'little', signed=False)
                        return f'{major}.{minor}.{build}.{privatex}'
            return None
        except FileNotFoundError: print(f'Error: The file "{file_path}" was not found.'); return None
        except Exception as e: print(f'An error occurred: {e}'); return None

    @staticmethod
    def validateClientVersion(versionText: str) -> ClientVersion:
        if not versionText: return None
        b = versionText.lower().split('.')
        if len(b) <= 2 or len(b) > 4: return None
        extra = 0
        if (major := _int_tryParse(b[0])) == None or major < 0 or major > 255 or \
            (minor := _int_tryParse(b[1])) == None or minor < 0 or minor > 255: print('parse'); return None
        extraIndex = 2; build = 0
        if len(b) == 4:
            if (build := _int_tryParse(b[extraIndex])) == None or build < 0 or build > 255: return None
            extraIndex += 1
        for i in range(len(b[extraIndex])):
            c = b[extraIndex][i]
            if c.isalpha(): extra = c; break
        i += 1
        if extra != 0:
            if len(b[extraIndex]) - i > 1: return None
        elif i <= 0: return None
        if (numExtra := _int_tryParse(b[extraIndex][:i])) == None or numExtra < 0 or numExtra > 255: return None
        if extra != 0:
            start = 'a'; index = 0
            while start != extra and start <= 'z': start += 1; index += 1
            extra = index
        if extraIndex == 2: build = numExtra; numExtra = extra
        return ClientVersion(((major & 0xFF) << 24) | ((minor & 0xFF) << 16) | ((build & 0xFF) << 8) | (numExtra & 0xFF))

#endregion