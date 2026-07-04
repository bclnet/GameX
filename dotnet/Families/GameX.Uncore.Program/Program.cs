using System.Globalization;
using System.IO.Compression;

// Zip
//DataRow("",
//    @"G:\T_\Samples64.zip", [@"Sample1.py"]);
//DataRow("", // BAD
//    @"G:\T_\SamplesPwd.zip", [@"Sample1.py"]);

// None
//DataRow("",
//    @"G:\SteamLibrary\steamapps\common\Wolcen\Game\Textures_decals.pak", [@"Textures/decals/blood/blood_decal_1.dds"]);
//DataRow("",
//    @"G:\SteamLibrary\steamapps\common\Crysis Remastered\Game\gamedata.pak", [@"entities/AdvancedDoor.ent"]);

// TEA, comments:TEA
//DataRow("hex:308189028181009B606931DCF7027A4DC0E5263B4AD0D8F4A492A16E4B5EC0850F074B4C3DA627FF96676D2379F89062DE6C917F268CBD822404D26D9D79BCB0182D4C96EEAF2B918A0300BFB81619622D1556B4E02D16FE0C7ED72C01EE429C4C849C6A786BCEC44D6C50CB914648BB662D0BA235680002D4605058D1C30DA11237822A01F2EF0203010001",
//    @"G:\SteamLibrary\steamapps\common\Warface\13_2000076\Game\GameInfo.pak", [@"paklist.txt"]);
//DataRow("hex:308189028181009B606931DCF7027A4DC0E5263B4AD0D8F4A492A16E4B5EC0850F074B4C3DA627FF96676D2379F89062DE6C917F268CBD822404D26D9D79BCB0182D4C96EEAF2B918A0300BFB81619622D1556B4E02D16FE0C7ED72C01EE429C4C849C6A786BCEC44D6C50CB914648BB662D0BA235680002D4605058D1C30DA11237822A01F2EF0203010001",
//    @"G:\SteamLibrary\steamapps\common\Warface\13_2000076\Game\Textures_Other.pak", [@"levels/africa/africa_base/africa_base.xml"]);

// Comments: NEWHUNT | NEWHUNT
DataRow("hex:30818902818100affd71ca741c1aa5895becf596e8732d290453d275cf6ff0bb214324ebab7eedd7f39deebc2708d88b6d536a58da5683137fafec478e41e6f8b0882e5eba236b9d2a150ee513ae562ce56b6aaf982c27a8c317281afa0f84f546ecb825ccf2217519c84ed0ceab179ee5ccdab0cb40a95d5442120f25a61e7da79d30c7d7d8a70203010001",
    @"G:\SteamLibrary\steamapps\common\Hunt Showdown\game_hunt\gamedata.pak", [@"difficulty/easy.cfg"]); // , @"difficulty/delta.cfg"
    //@"G:\SteamLibrary\steamapps\common\Hunt Showdown\game_hunt\gamedata.pak", [@"difficulty/easy.cfg"]); // , @"difficulty/delta.cfg"
//DataRow("hex:30818902818100affd71ca741c1aa5895becf596e8732d290453d275cf6ff0bb214324ebab7eedd7f39deebc2708d88b6d536a58da5683137fafec478e41e6f8b0882e5eba236b9d2a150ee513ae562ce56b6aaf982c27a8c317281afa0f84f546ecb825ccf2217519c84ed0ceab179ee5ccdab0cb40a95d5442120f25a61e7da79d30c7d7d8a70203010001",
//    @"G:\SteamLibrary\steamapps\common\Hunt Showdown\game_hunt\audio-part0.pak", [@"audio/initdata.xml"]);
//DataRow("hex:30818902818100affd71ca741c1aa5895becf596e8732d290453d275cf6ff0bb214324ebab7eedd7f39deebc2708d88b6d536a58da5683137fafec478e41e6f8b0882e5eba236b9d2a150ee513ae562ce56b6aaf982c27a8c317281afa0f84f546ecb825ccf2217519c84ed0ceab179ee5ccdab0cb40a95d5442120f25a61e7da79d30c7d7d8a70203010001",
//    @"G:\SteamLibrary\steamapps\common\Hunt Showdown\engine\engine.pak", [@"Config/AutoTestChain.cfg"]); //Config/durango.cfg

// Comments:STREAMCIPHER_KEYTABLE|CDR_SIGNED
//DataRow("hex:30818902818100D51E1D3810C4A112B2F2504B83E2F124009C0AC9CD1661913421D4E94623AD7014599DAFB0DC9F8366D164AD072B3DC5AA3D4CD24542D5F684E6A4F7473102DE2ACA11F6524015ECBD564248FC712B3A69B15B78EFAA06748259DDE77A75757E513F7AC21A0151F53C78FF45ABCC45C3F54BC6305F420981F7119AF03E6438D70203010001",
//    @"G:\SteamLibrary\steamapps\common\SNOW\Assets\GameData.pak", [@"Libs/ActionSpotSystem.xml"]);
//DataRow("hex:30818902818100D51E1D3810C4A112B2F2504B83E2F124009C0AC9CD1661913421D4E94623AD7014599DAFB0DC9F8366D164AD072B3DC5AA3D4CD24542D5F684E6A4F7473102DE2ACA11F6524015ECBD564248FC712B3A69B15B78EFAA06748259DDE77A75757E513F7AC21A0151F53C78FF45ABCC45C3F54BC6305F420981F7119AF03E6438D70203010001",
//    @"G:\SteamLibrary\steamapps\common\SNOW\Assets\Sounds.pak", [@"audio/ace/drone.xml.cryasset"]);

// TEA
//DataRow("",
//    @"G:\SteamLibrary\steamapps\common\Ryse Son of Rome\GameRyse\GameData.pak", [@"Difficulty/easy.cfg"]);
//DataRow("",
//    @"G:\SteamLibrary\steamapps\common\Ryse Son of Rome\GameRyse\Music.pak", [@"Music/colosseum_fm/07_colosseum_bossfight_stage1_action.ogg"]);

// Comments:CDR_SIGNED
//DataRow("",
//    @"G:\SteamLibrary\steamapps\common\Robinson The Journey\game_robinson\gamedata.pak", [@"libs/ai/AIInterestTypes.xml"]);
//DataRow("",
//    @"G:\SteamLibrary\steamapps\common\Robinson The Journey\game_robinson\audio.pak", [@"audio/ace/c_beetle.xml"]);

// P4K
//DataRow("hex:5E7A2002302EEB1A3BB617C30FDE1E47",
//    @"D:\Roberts Space Industries\StarCitizen\LIVE\Data.p4k", [@"Engine\EngineAssets\Textures\scratch.dds"]);
//@"D:\Roberts Space Industries\StarCitizen\LIVE\Data.p4k", [@"Data\Scripts\FeatureTests\FeatureTests.xml"]);
//    @"D:\Roberts Space Industries\StarCitizen\LIVE\Data.p4k", [@"Data\Textures\planets\surface\ground\architecture\city\city_suburbs_02_displ.dds.6", @"Data\Prefabs\shops\admin\admin.xml", @"Data\Objects\buildingsets\hangar\deluxe\deluxe_elevator_shaft_02m.cgfm"]);

static void DataRow(string key, string path, string[] files) {
    static void log(string x) => Console.WriteLine(x);
    //static void log2(byte[] x) => Console.WriteLine(Encoding.UTF8.GetString(x));
    using var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    var pak = new ZipArchiveX(fs, key: ParseKey(key));
    foreach (var ent in pak.Entries.Take(5)) log($"{ent.FullName} - {ent.Length} - {new ZipArchiveEntryX(ent).CompressionMethod}");
    log("");
    if (files != null)
        foreach (var file in files) {
            var entry = pak.GetEntry(file) ?? throw new FileNotFoundException();
            using var input2 = entry.OpenX();
            var body = new StreamReader(input2).ReadToEnd();
            log(body);
        }
    else
        foreach (var ent in pak.Entries.Take(100)) {
            try {
                // create directory
                using var input = ent.OpenX();
                if (ent.Length == 0) continue;
                var newPath = Path.Combine(@"D:\T_\X\", ent.Name);
                var directory = Path.GetDirectoryName(newPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
                using var s = new FileStream(newPath, FileMode.Create, FileAccess.Write);
                input.CopyTo(s);
            }
            catch (Exception e) { Console.WriteLine(e.Message); throw; }
        }
}

static byte[] ParseKey(string str) {
    if (string.IsNullOrEmpty(str)) return null!;
    else if (str.StartsWith("hex:", StringComparison.OrdinalIgnoreCase)) {
        var keyStr = str[4..];
        var key = keyStr.StartsWith('/')
            ? Enumerable.Range(0, keyStr.Length >> 2).Select(x => byte.Parse(keyStr.Substring((x << 2) + 2, 2), NumberStyles.HexNumber)).ToArray()
            : Enumerable.Range(0, keyStr.Length >> 1).Select(x => byte.Parse(keyStr.Substring(x << 1, 2), NumberStyles.HexNumber)).ToArray();
        return key;
    }
    else throw new ArgumentOutOfRangeException(nameof(str), str);
}
