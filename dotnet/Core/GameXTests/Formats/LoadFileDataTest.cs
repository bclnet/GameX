using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Formats;

[TestClass]
public class LoadFileDataTest {
    [TestMethod]
    [DataRow("AC:AC")]
    [DataRow("Arkane:D2", 10000000)]
    [DataRow("Cry:MWO")]
    [DataRow("Cyanide:TheCouncil")]
    [DataRow("Origin:UltimaOnline")]
    [DataRow("Origin:UltimaIX")]
    [DataRow("Rsi:StarCitizen", 10000000)]
    [DataRow("Red:Witcher")]
    [DataRow("Red:Witcher2")]
    [DataRow("Red:Witcher3")]
    [DataRow("Tes:Morrowind")]
    [DataRow("Tes:Oblivion")]
    [DataRow("Tes:Skyrim")]
    [DataRow("Tes:SkyrimSE")]
    [DataRow("Tes:Fallout2")]
    [DataRow("Tes:Fallout3")]
    [DataRow("Tes:FalloutNV")]
    [DataRow("Tes:Fallout4")]
    [DataRow("Tes:Fallout4VR")]
    [DataRow("Tes:Fallout76", 15000000)]
    [DataRow("Valve:Dota2", 15000000)]
    public async Task LoadAllFileData(string arc, long maxFileSize = 0) {
        var source = TestHelper.Paks[arc].Value;
        if (source is MultiArchive multiPak)
            foreach (var p in multiPak.Archives) {
                if (p is not BinaryAsset z) throw new InvalidOperationException("multiPak not A BinaryAsset");
                await ExportAsync(z, maxFileSize);
            }
        else await ExportAsync(source, maxFileSize);
    }

    static Task ExportAsync(Archive source, long maxSize) {
        if (source is not BinaryAsset arc) throw new NotSupportedException();

        // write files
        Parallel.For(0, arc.Files.Count, new ParallelOptions { /*MaxDegreeOfParallelism = 1*/ }, async index => {
            var file = arc.Files[index].Fix();

            // extract arc
            if (file.Arc != null) { await ExportAsync(file.Arc, maxSize); return; }
            // skip empty file
            if (file.FileSize == 0 && file.PackedSize == 0) return;
            // skip large files
            if (maxSize != 0 && file.FileSize > maxSize) return;

            // extract file
            using var s = await arc.GetData(file);
            s.ReadAllBytes();
        });

        return Task.CompletedTask;
    }
}
