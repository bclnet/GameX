from __future__ import annotations
from compression import ZipFileKind, ZipFileX

@staticmethod
def parseKey(value: str) -> object:
    if not value: return None
    elif value.startswith('b64:'): return base64.b64decode(value[4:].encode('ascii'))
    elif value.startswith('hex:'): return bytes.fromhex(value[4:].replace('/x', ''))
    elif value.startswith('asc:'): return value[4:].encode('ascii')
    else: return value

def dataRow(key: str, path: str, file: str) -> None:
    def log(x: str): print(x)
    with open(path, 'rb') as fs:
        pak = ZipFileX(ZipFileKind.P4k if path.endswith('.p4k') else ZipFileKind.Cry3, fs, path, parseKey(key))
        for ent in pak.infolist()[:10]: log(f'{ent.filename} - {ent.file_size} - {ent.compress_type}')
        exit(1)
        if file:
            with pak.open(file) as input2:
                body = input2.read()
                log(body)
        else:
            for ent in pak.infolist()[:100]:
                pass
        #     try {
        #         # create directory
        #         using var input = ent.OpenX();
        #         if (ent.Length == 0) continue;
        #         var newPath = Path.Combine(@'D:\T_\X\', ent.Name);
        #         var directory = Path.GetDirectoryName(newPath);
        #         if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
        #         using var s = new FileStream(newPath, FileMode.Create, FileAccess.Write);
        #         input.CopyTo(s);
        #     }
        #     catch (Exception e) { Console.WriteLine(e.Message); throw; }


# # None (OK)
# dataRow("",
#    r"G:\SteamLibrary\steamapps\common\Wolcen\Game\Textures_decals.pak", [r"Textures/decals/blood/blood_decal_1.dds"])

# TEA, comments:TEA (ERROR)
# dataRow("hex:308189028181009B606931DCF7027A4DC0E5263B4AD0D8F4A492A16E4B5EC0850F074B4C3DA627FF96676D2379F89062DE6C917F268CBD822404D26D9D79BCB0182D4C96EEAF2B918A0300BFB81619622D1556B4E02D16FE0C7ED72C01EE429C4C849C6A786BCEC44D6C50CB914648BB662D0BA235680002D4605058D1C30DA11237822A01F2EF0203010001",
#     r"C:\Program Files (x86)\Steam\steamapps\common\Warface Clutch\13_2000076\Game\GameInfo.pak", [r"paklist.txt"])
# dataRow("hex:308189028181009B606931DCF7027A4DC0E5263B4AD0D8F4A492A16E4B5EC0850F074B4C3DA627FF96676D2379F89062DE6C917F268CBD822404D26D9D79BCB0182D4C96EEAF2B918A0300BFB81619622D1556B4E02D16FE0C7ED72C01EE429C4C849C6A786BCEC44D6C50CB914648BB662D0BA235680002D4605058D1C30DA11237822A01F2EF0203010001",
#     r"G:\SteamLibrary\steamapps\common\Warface\13_2000076\Game\GameInfo.pak", [r"paklist.txt"])
#dataRow("hex:308189028181009B606931DCF7027A4DC0E5263B4AD0D8F4A492A16E4B5EC0850F074B4C3DA627FF96676D2379F89062DE6C917F268CBD822404D26D9D79BCB0182D4C96EEAF2B918A0300BFB81619622D1556B4E02D16FE0C7ED72C01EE429C4C849C6A786BCEC44D6C50CB914648BB662D0BA235680002D4605058D1C30DA11237822A01F2EF0203010001",
#    r"G:\SteamLibrary\steamapps\common\Warface\13_2000076\Game\Textures_Other.pak", [r"xxx"])

## Comments: NEWHUNT | NEWHUNT (OK)
# dataRow("hex:30818902818100affd71ca741c1aa5895becf596e8732d290453d275cf6ff0bb214324ebab7eedd7f39deebc2708d88b6d536a58da5683137fafec478e41e6f8b0882e5eba236b9d2a150ee513ae562ce56b6aaf982c27a8c317281afa0f84f546ecb825ccf2217519c84ed0ceab179ee5ccdab0cb40a95d5442120f25a61e7da79d30c7d7d8a70203010001",
#     r"G:\SteamLibrary\steamapps\common\Hunt Showdown\game_hunt\gamedata.pak", [r"xxx"])
#dataRow("hex:30818902818100affd71ca741c1aa5895becf596e8732d290453d275cf6ff0bb214324ebab7eedd7f39deebc2708d88b6d536a58da5683137fafec478e41e6f8b0882e5eba236b9d2a150ee513ae562ce56b6aaf982c27a8c317281afa0f84f546ecb825ccf2217519c84ed0ceab179ee5ccdab0cb40a95d5442120f25a61e7da79d30c7d7d8a70203010001",
#    r"G:\SteamLibrary\steamapps\common\Hunt Showdown\game_hunt\audio.pak", [r"xxx"])

## Comments:STREAMCIPHER_KEYTABLE|CDR_SIGNED (OK)
dataRow("hex:30818902818100D51E1D3810C4A112B2F2504B83E2F124009C0AC9CD1661913421D4E94623AD7014599DAFB0DC9F8366D164AD072B3DC5AA3D4CD24542D5F684E6A4F7473102DE2ACA11F6524015ECBD564248FC712B3A69B15B78EFAA06748259DDE77A75757E513F7AC21A0151F53C78FF45ABCC45C3F54BC6305F420981F7119AF03E6438D70203010001",
   r"C:\Program Files (x86)\Steam\steamapps\common\SNOW\Assets\GameData.pak", [r"xxx"])
# dataRow("hex:30818902818100D51E1D3810C4A112B2F2504B83E2F124009C0AC9CD1661913421D4E94623AD7014599DAFB0DC9F8366D164AD072B3DC5AA3D4CD24542D5F684E6A4F7473102DE2ACA11F6524015ECBD564248FC712B3A69B15B78EFAA06748259DDE77A75757E513F7AC21A0151F53C78FF45ABCC45C3F54BC6305F420981F7119AF03E6438D70203010001",
#    r"G:\SteamLibrary\steamapps\common\SNOW\Assets\GameData.pak", [r"xxx"])
#dataRow("hex:30818902818100D51E1D3810C4A112B2F2504B83E2F124009C0AC9CD1661913421D4E94623AD7014599DAFB0DC9F8366D164AD072B3DC5AA3D4CD24542D5F684E6A4F7473102DE2ACA11F6524015ECBD564248FC712B3A69B15B78EFAA06748259DDE77A75757E513F7AC21A0151F53C78FF45ABCC45C3F54BC6305F420981F7119AF03E6438D70203010001",
#    r"G:\SteamLibrary\steamapps\common\SNOW\Assets\Sounds.pak", [r"xxx"])

## TEA (OK)
#dataRow(None,
#    r"G:\SteamLibrary\steamapps\common\Ryse Son of Rome\GameRyse\GameData.pak", [r"xxx"])
#dataRow(None,
#    r"G:\SteamLibrary\steamapps\common\Ryse Son of Rome\GameRyse\Music.pak", [r"xxx"])

## Comments:CDR_SIGNED (OK)
#dataRow(None,
#    r"G:\SteamLibrary\steamapps\common\Robinson The Journey\game_robinson\gamedata.pak", [r"xxx"])
#dataRow(None,
#    r"G:\SteamLibrary\steamapps\common\Robinson The Journey\game_robinson\audio.pak", [r"xxx"])

## None (OK)
# dataRow(None,
#    r"G:\SteamLibrary\steamapps\common\Crysis Remastered\Game\gamedata.pak", [r"entities/AdvancedDoor.ent"])

# P4K (OK)
# dataRow("hex:5E7A2002302EEB1A3BB617C30FDE1E47",
#     r"D:\Roberts Space Industries\StarCitizen\LIVE\Data.p4k", [r"Data\Textures\planets\surface\ground\architecture\city\city_suburbs_02_displ.dds.6"]) #, r"Data\Prefabs\shops\admin\admin.xml", r"Data\Objects\buildingsets\hangar\deluxe\deluxe_elevator_shaft_02m.cgfm"])
