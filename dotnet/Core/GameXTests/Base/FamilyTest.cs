using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameX.Base;

[TestClass]
public class FamilyTest {
    [TestMethod]
    public void ShouldGetGame() {
        var family = Some.Family;
        Assert.Throws<ArgumentOutOfRangeException>(() => family.GetGame("Wrong", out var _));
        Assert.IsNotNull(family.GetGame("Missing", out var _));
        Assert.IsNotNull(family.GetGame("Found", out var _));
    }

    [TestMethod]
    public void ShouldParseResource() {
        var family = Some.Family;
        Assert.IsNotNull(family.ParseResource(null, true));
    }

    //[TestMethod]
    //public void ShouldOpenPakFile_Paths()
    //{
    //    var family = Some.Family;
    //    Assert.Throws<ArgumentNullException>(() => family.OpenPakFile_DEL(null, null, null, null));
    //    Assert.Throws<ArgumentOutOfRangeException>(() => family.OpenPakFile_DEL(family.GetGame("Missing", out var edition), edition, null, null));
    //    Assert.Throws<ArgumentOutOfRangeException>(() => family.OpenPakFile_DEL(family.GetGame("Found", out var edition), edition, null, null));
    //    Assert.IsNull(family.OpenPakFile_DEL(family.GetGame("Missing", out var edition), edition, null, null, throwOnError: false));
    //    Assert.IsNotNull(family.OpenPakFile_DEL(family.GetGame("Found", out edition), edition, "path", null));
    //}

    [TestMethod]
    public void ShouldOpenPakFile_Resource() {
        var family = Some.Family;
        Assert.Throws<ArgumentNullException>(() => family.OpenArchive(new Resource { }));
        //Assert.IsNull(family.OpenPakFile(new Resource { Paths = null, Game = FamilyGame.Empty }, throwOnError: false));
        //Assert.IsNotNull(family.OpenPakFile(new Resource { Paths = new[] { "path" }, Game = family.GetGame("Found") }));
    }

    [TestMethod]
    public void ShouldOpenPakFile_Uri() {
        var family = Some.Family;
        Assert.IsNull(family.OpenArchive(null, throwOnError: false));
        //// game-scheme
        //Assert.IsNull(null, family.OpenPakFile(new Uri("game:/path#Found")));
        //// file-scheme
        //Assert.IsNull(null, family.OpenPakFile(new Uri("file://path#Found")));
        //// network-scheme
        //Assert.IsNull(null, family.OpenPakFile(new Uri("https://path#Found")));
    }
}
