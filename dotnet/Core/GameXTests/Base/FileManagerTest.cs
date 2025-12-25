using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameX.Base;

[TestClass]
public class FileManagerTest {
    static readonly Family Family = FamilyManager.Uncore;

    [TestMethod]
    [DataRow(null)]
    [DataRow("game:/#APP")]
    [DataRow("file:///C:/#APP")]
    [DataRow("https://localhost#APP")]
    public void ShouldParseResource(string uri) {
        Assert.IsNotNull(Family.ParseResource(uri != null ? new Uri(uri) : null, false));
    }

    //[TestMethod]
    //public void ShouldParseFileManager()
    //{
    //    var fileManager = Family.FileManager;
    //    using var doc = JsonDocument.Parse(Some.FileManagerJson.Replace("'", "\""));
    //    var elem = doc.RootElement;
    //    //Assert.IsNotNull(fileManager.ParseFileManager(elem));
    //}
}
