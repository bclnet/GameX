using CommandLine;
using GameX.Formats.Collada;
using GameX.Formats.Unknown;
using GameX.Formats.Wavefront;
using System;
using System.IO;

namespace GameX.App.Cli {
    // test
    // list -e "Tes"
    // import -e "Tes" -u "game:/Oblivion*.bsa#Oblivion" --path "D:\T_\Test2"
    // export -e "Tes" -u "game:/Oblivion*.bsa#Oblivion" --path "D:\T_\Test2"
    // xsport -e "Tes" -u "game:/Oblivion*.bsa#Oblivion" --path "D:\T_\Test2"
    partial class Program {
        #region ProgramState

        static class ProgramState {
            public static T Load<T>(Func<byte[], T> func, T defaultValue) {
                try {
                    if (File.Exists(@".\lastChunk.txt")) {
                        using var s = File.Open(@".\lastChunk.txt", FileMode.Open);
                        var data = new byte[s.Length];
                        s.ReadExactly(data, 0, (int)s.Length);
                        return func(data);
                    }
                }
                catch { }
                return defaultValue;
            }

            public static void Store(Func<byte[]> func) {
                try {
                    var data = func();
                    using var s = new FileStream(@".\lastChunk.txt", FileMode.Create, FileAccess.Write);
                    s.Write(data, 0, data.Length);
                }
                catch { Clear(); }
            }

            public static void Clear() {
                try {
                    if (File.Exists(@".\lastChunk.txt")) File.Delete(@".\lastChunk.txt");
                }
                catch { }
            }
        }

        #endregion

        static void Register() {
            UnknownFileWriter.Factories["Collada"] = file => new ColladaFileWriter(file);
            UnknownFileWriter.Factories["Wavefront"] = file => new WavefrontFileWriter(file);
            UnknownFileWriter.Factories["default"] = UnknownFileWriter.Factories["Wavefront"];
        }

        static void Main(string[] args) {
            // TEST
            //args = ["test"];

            // LIST
            //args = ["list"];
            //args = ["list", "-f", "Red"];
            //args = ["list", "-f", "Bethesda"];
            //args = ["list", "-f", "Bethesda", "-u", "game:/#Oblivion"];
            //args = ["list", "-f", "Bethesda", "-u", "game:/Oblivion*.bsa#Oblivion"];
            //args = ["list", "-f", "Bethesda", "-u", "file:///G:/SteamLibrary/steamapps/common/Oblivion/Oblivion*.bsa#Oblivion"];
            //args = ["list", "-f", "Unity", "-u", "game:/resources.assets#Cities"];
            //args = ["list", "-f", "Unity", "-u", @"file:///C:/T_/Unity/Assets/myscene.unity#AmongUs"];
            //args = ["list", "-f", "Unity", "-u", @"game:/StreamingAssets\aa\Steam\StandaloneWindows\6fcbc56bf87ce16ef93cf7950ec3d7c9_unitybuiltinshaders_e998f854a714e8c70679af5d74e29f20.bundle#AmongUs"];
            //args = ["list", "-f", "Unity", "-u", @"game:/StreamingAssets\aa\Steam\StandaloneWindows\initialmaps_assets_all.bundle#AmongUs"];
            //args = ["list", "-f", "Unity", "-u", @"game:/resources.assets#AmongUs"]; // asset.v22
            //args = ["list", "-f", "Unity", "-u", @"game:/sharedassets2.assets#AmongUs"]; // asset.v22
            //args = ["list", "-f", "Unity", "-u", @"game:/globalgamemanagers.assets#AmongUs"]; // asset.v22
            //args = ["list", "-f", "Unity", "-u", @"game:/resources.assets#Cities"]; // asset.v17
            //args = ["list", "-f", "Unity", "-u", @"game:/resources.assets#Cities"]; // asset.v17
            //args = ["list", "-f", "Unity", "-u", @"game:/globalgamemanagers.assets#Cities"]; // asset.v17
            //args = ["list", "-f", "Unity", "-u", @"game:/resources.assets#Tabletop"]; // asset.v21
            //args = ["list", "-f", "Unity", "-u", @"game:/globalgamemanagers.assets#Tabletop"]; // asset.v21
            //args = ["list", "-f", "Lith", "-u", @"game:/FEAR_1.Arch00#FEAR"];

            // GET
            //args = ["get", "-f", "Bethesda", "-u", "game:/Morrowind.bsa#Morrowind", "-m", "*.nif", "-o", "StreamObject", "-p", @"D:\T_\MorrowindC"];
            // args = ["get", "-f", "Bethesda", "-u", "game:/Morrowind.bsa#Morrowind", "-m", "meshes/l/light_com_candle_04.nif", "-o", "StreamObject", "-p", @"D:\T_\MorrowindC"];

            //args = ["get", "-f", "Bethesda", "-u", "game:/Oblivion - Meshes.bsa#Oblivion", "-m", "*/bearskinrug01.nif", "-o", "StreamObject", "-p", @"D:\T_\OblivionC"];
            //args = ["get", "-f", "Bethesda", "-u", "game:/#Oblivion", "-m", "*.nif", "--path", @"D:\T_\Oblivion"];
            //args = ["get", "-f", "Rsi", "-u", "game:/Data.p4k#StarCitizen", "--path", @"D:\T_\StarCitizen"];
            //args = ["get", "-f", "Tes", "-u", "game:/Oblivion*.bsa#Oblivion", "--path", @"D:\T_\Oblivion"];
            //args = ["get", "-f", "Red", "-u", "game:/main.key#Witcher", "--path", @"D:\T_\Witcher"];
            //args = ["get", "-f", "Red", "-u", "game:/krbr.dzip#Witcher2", "--path", @"D:\T_\Witcher2"];

            args = ["get", "-f", "Red", "-u", "game:/main.key#Witcher", "--path", @"~/T_/Witcher"];

            //args = ["list", "-f", "Arkane"];
            //args = ["get", "-f", "Arkane", "-u", "game:/*.pak#AF", "--path", @"C:\T_\AF"];
            //args = ["list", "-f", "Blizzard"];
            //args = ["list", "-f", "Bioware"];
            //args = ["list", "-f", "Valve"];
            //args = ["list", "-f", "Valve", "-u", "game:/*_dir.vpk#L4D"];
            //args = ["get", "-f", "Valve", "-u", "game:/*_dir.vpk#L4D", "--path", @"~/T_/L4D"];
            //args = ["get", "-f", "Valve", "-u", "game:/*_dir.vpk#L4D", "--path", "null"];
            //args = ["list", "-f", "IW"];

            Register();
            Parser.Default.ParseArguments<TestOptions, ListOptions, GetOptions, SetOptions>(args)
            .MapResult(
                (TestOptions opts) => RunTestAsync(opts).GetAwaiter().GetResult(),
                (ListOptions opts) => RunListAsync(opts).GetAwaiter().GetResult(),
                (GetOptions opts) => RunGetAsync(opts).GetAwaiter().GetResult(),
                (SetOptions opts) => RunSetAsync(opts).GetAwaiter().GetResult(),
                errs => 1);
        }
    }
}