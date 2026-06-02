using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GameX.Uncore;

[TestClass]
public class ZipArchiveXTest {
    [TestMethod]
    //// None (OK)
    //[DataRow("",
    //    @"G:\SteamLibrary\steamapps\common\Wolcen\Game\Textures_decals.pak", [@"Textures/decals/blood/blood_decal_1.dds"])]

    //// TEA, comments:TEA (ERROR)
    //[DataRow("hex:308189028181009B606931DCF7027A4DC0E5263B4AD0D8F4A492A16E4B5EC0850F074B4C3DA627FF96676D2379F89062DE6C917F268CBD822404D26D9D79BCB0182D4C96EEAF2B918A0300BFB81619622D1556B4E02D16FE0C7ED72C01EE429C4C849C6A786BCEC44D6C50CB914648BB662D0BA235680002D4605058D1C30DA11237822A01F2EF0203010001",
    //    @"G:\SteamLibrary\steamapps\common\Warface\13_2000076\Game\GameInfo.pak", [@"paklist.txt"])]
    //[DataRow("hex:308189028181009B606931DCF7027A4DC0E5263B4AD0D8F4A492A16E4B5EC0850F074B4C3DA627FF96676D2379F89062DE6C917F268CBD822404D26D9D79BCB0182D4C96EEAF2B918A0300BFB81619622D1556B4E02D16FE0C7ED72C01EE429C4C849C6A786BCEC44D6C50CB914648BB662D0BA235680002D4605058D1C30DA11237822A01F2EF0203010001",
    //    @"G:\SteamLibrary\steamapps\common\Warface\13_2000076\Game\Textures_Other.pak", [@"xxx"])]

    //// Comments: NEWHUNT | NEWHUNT (OK)
    //[DataRow("hex:30818902818100affd71ca741c1aa5895becf596e8732d290453d275cf6ff0bb214324ebab7eedd7f39deebc2708d88b6d536a58da5683137fafec478e41e6f8b0882e5eba236b9d2a150ee513ae562ce56b6aaf982c27a8c317281afa0f84f546ecb825ccf2217519c84ed0ceab179ee5ccdab0cb40a95d5442120f25a61e7da79d30c7d7d8a70203010001",
    //    @"G:\SteamLibrary\steamapps\common\Hunt Showdown\game_hunt\gamedata.pak", [@"xxx"])]
    //[DataRow("hex:30818902818100affd71ca741c1aa5895becf596e8732d290453d275cf6ff0bb214324ebab7eedd7f39deebc2708d88b6d536a58da5683137fafec478e41e6f8b0882e5eba236b9d2a150ee513ae562ce56b6aaf982c27a8c317281afa0f84f546ecb825ccf2217519c84ed0ceab179ee5ccdab0cb40a95d5442120f25a61e7da79d30c7d7d8a70203010001",
    //    @"G:\SteamLibrary\steamapps\common\Hunt Showdown\game_hunt\audio.pak", [@"xxx"])]

    //// Comments:STREAMCIPHER_KEYTABLE|CDR_SIGNED (OK)
    //[DataRow("hex:30818902818100D51E1D3810C4A112B2F2504B83E2F124009C0AC9CD1661913421D4E94623AD7014599DAFB0DC9F8366D164AD072B3DC5AA3D4CD24542D5F684E6A4F7473102DE2ACA11F6524015ECBD564248FC712B3A69B15B78EFAA06748259DDE77A75757E513F7AC21A0151F53C78FF45ABCC45C3F54BC6305F420981F7119AF03E6438D70203010001",
    //    @"G:\SteamLibrary\steamapps\common\SNOW\Assets\GameData.pak", [@"xxx"])]
    //[DataRow("hex:30818902818100D51E1D3810C4A112B2F2504B83E2F124009C0AC9CD1661913421D4E94623AD7014599DAFB0DC9F8366D164AD072B3DC5AA3D4CD24542D5F684E6A4F7473102DE2ACA11F6524015ECBD564248FC712B3A69B15B78EFAA06748259DDE77A75757E513F7AC21A0151F53C78FF45ABCC45C3F54BC6305F420981F7119AF03E6438D70203010001",
    //    @"G:\SteamLibrary\steamapps\common\SNOW\Assets\Sounds.pak", [@"xxx"])]

    //// TEA (OK)
    //[DataRow(null,
    //    @"G:\SteamLibrary\steamapps\common\Ryse Son of Rome\GameRyse\GameData.pak", [@"xxx"])]
    //[DataRow(null,
    //    @"G:\SteamLibrary\steamapps\common\Ryse Son of Rome\GameRyse\Music.pak", [@"xxx"])]

    //// Comments:CDR_SIGNED (OK)
    //[DataRow(null,
    //    @"G:\SteamLibrary\steamapps\common\Robinson The Journey\game_robinson\gamedata.pak", [@"xxx"])]
    //[DataRow(null,
    //    @"G:\SteamLibrary\steamapps\common\Robinson The Journey\game_robinson\audio.pak", [@"xxx"])]

    //// None (OK)
    //[DataRow(null,
    //    @"G:\SteamLibrary\steamapps\common\Crysis Remastered\Game\gamedata.pak", [@"xxx"])]

    // P4K (OK)
    [DataRow("hex:5E7A2002302EEB1A3BB617C30FDE1E47",
        @"D:\Roberts Space Industries\StarCitizen\LIVE\Data.p4k", @"Data\Prefabs\shops\admin\admin.xml")]
    [DataRow("hex:5E7A2002302EEB1A3BB617C30FDE1E47",
        @"D:\Roberts Space Industries\StarCitizen\LIVE\Data.p4k", @"Data\Objects\buildingsets\hangar\deluxe\deluxe_elevator_shaft_02m.cgfm")]

    public void ShouldUnzip(string key, string path, string file) {
        static void log(string x) => Debugger.Log(0, null, x);
        using var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var pak = new ZipArchiveX(path.EndsWith(".p4k") ? ZipArchiveKind.P4k : ZipArchiveKind.Cry3, fs, fs.Name, ParseKey(key));
        foreach (var ent in pak.Entries.Take(10)) log($"{ent.FullName} - {new ZipArchiveEntryX(ent).CompressionMethod}");
        if (file != null) {
            var entry = pak.GetEntry(file) ?? throw new FileNotFoundException();
            using var input2 = entry.OpenX();
            var body = new StreamReader(input2).ReadToEnd();
            log(body);
        }
        else foreach (var ent in pak.Entries.Take(100)) {
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
        if (string.IsNullOrEmpty(str)) return null;
        else if (str.StartsWith("hex:", StringComparison.OrdinalIgnoreCase)) {
            var keyStr = str[4..];
            var key = keyStr.StartsWith('/')
                ? Enumerable.Range(0, keyStr.Length >> 2).Select(x => byte.Parse(keyStr.Substring((x << 2) + 2, 2), NumberStyles.HexNumber)).ToArray()
                : Enumerable.Range(0, keyStr.Length >> 1).Select(x => byte.Parse(keyStr.Substring(x << 1, 2), NumberStyles.HexNumber)).ToArray();
            return key;
        }
        else throw new ArgumentOutOfRangeException(nameof(str), str);
    }
}


