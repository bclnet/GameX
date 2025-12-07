//#define HTTPTEST

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static GameX.FamilyManager;

namespace GameX;

[TestClass]
public class ResourceTests {
    const string GAME = "game:";
    const string FILE_Oblivion = "file:///G:/SteamLibrary/steamapps/common/Oblivion";
    const string DIR_Oblivion = "file:////192.168.1.3/User/_SERVE/Assets/Oblivion";
#if HTTPTEST
    const string HTTP_Oblivion = "http://192.168.1.3/Estates/Oblivion";
#endif

    [TestMethod]
    [DataRow("Tes", $"{GAME}/Oblivion*.bsa/#Oblivion")]
#if HTTPTEST
    [DataRow("Tes", $"{HTTP_Oblivion}/Oblivion*.bsa#Oblivion")]
#endif
    public void ShouldThrow(string familyName, string uri)
        => Assert.Throws<ArgumentOutOfRangeException>(() => GetFamily(familyName).ParseResource(new Uri(uri)));

    [TestMethod]
    [DataRow("Tes", $"{GAME}/Oblivion*.bsa#Oblivion", "Oblivion", 6, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 6865)]
    [DataRow("Tes", $"{FILE_Oblivion}/Sbi/Oblivion*.bsa#Oblivion", "Oblivion", 6, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 6865)]
    [DataRow("Tes", $"{FILE_Oblivion}/Sbi/Oblivion%20-%20Meshes.bsa#Oblivion", "Oblivion", 1, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 6865)]
    //[DataRow("Tes", $"{DIR_Oblivion}/Oblivion*.bsa/#Oblivion", "Oblivion", PakOption.Stream, 6, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 6865)]
    //[DataRow("Tes", $"{DIR_Oblivion}/Oblivion%20-%20Meshes.bsa/#Oblivion", "Oblivion", PakOption.Stream, 1, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 6865)]
#if HTTPTEST
    [DataRow("Tes", $"{HTTP_Oblivion}/Oblivion*.bsa/#Oblivion", "Oblivion", PakOption.Stream, 6, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 6865)]
    [DataRow("Tes", $"{HTTP_Oblivion}/Oblivion%20-%20Meshes.bsa/#Oblivion", "Oblivion", PakOption.Stream, 1, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 6865)]
#endif
    public void Resource(string familyName, string uri, string game, int pathsFound, string firstPak, string sampleFile, int sampleFileSize) {
        var family = GetFamily(familyName);
        var resource = family.ParseResource(new Uri(uri));
        Assert.AreEqual(game, resource.Game.Id);
        //Assert.AreEqual(pathsFound, resource.Paths.Length);
        var archive = family.GetArchive(new Uri(uri));
        if (archive is MultiArchive multiArchive) {
            Assert.HasCount(pathsFound, multiArchive.Archives);
            archive = multiArchive.Archives[0];
        }
        if (archive == null) throw new InvalidOperationException("arc not opened");
        Assert.AreEqual(firstPak, archive.Name);
        Assert.IsTrue(archive.Contains(sampleFile));
        Assert.AreEqual(sampleFileSize, archive.GetData(sampleFile).Result.Length);
    }
}
