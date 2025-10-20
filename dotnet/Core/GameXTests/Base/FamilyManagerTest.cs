using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameX.Base;

[TestClass]
public class FamilyManagerTest {
    [TestMethod]
    public void ShouldFamily() {
        Assert.HasCount(1, FamilyManager.Families);
    }

    [TestMethod]
    public void ShouldGetFamily() {
        Assert.Throws<ArgumentNullException>(() => FamilyManager.GetFamily(null));
        Assert.Throws<ArgumentOutOfRangeException>(() => FamilyManager.GetFamily("Missing"));
    }

    [TestMethod]
    public void ShouldParseFamily() {
        //Assert.Throws<ArgumentNullException>(() => FamilyManager.ParseFamily(null));
        //Assert.IsNotNull(FamilyManager.ParseFamily(Some.FamilyJson.Replace("'", "\"")));
    }
}
