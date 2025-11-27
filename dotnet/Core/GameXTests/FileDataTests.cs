using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack;
using static GameX.FamilyManager;

namespace GameX;

[TestClass]
public class FileDataTests {
    [TestMethod]
    [DataRow("Arkane", "game:/#AF", "sample:0")]
    public void Resource(string familyName, string file0, string file1) {
        // get family
        var family = GetFamily(familyName);
        Log.Info($"studio: {family.Studio}");

        // get pak with game:/uri
        var pakFile = family.OpenPakFile(file0);
        var sample = file1.StartsWith("sample") ? pakFile.Game.GetSample(file1[7..]).Paths[0] : file1;
        Log.Info($"pak: {pakFile}, {sample}");

        // get file
        var data = pakFile.LoadFileData(sample).Result;
        Log.Info($"dat: {data}");
    }
}
