using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace GameX.IW;

[TestClass]
public class FormatsTest {
    static readonly Family family = FamilyManager.GetFamily("IW");

    [TestMethod]
    [DataRow(true, "game:/code_post_gfx.ff#COD4")]          // 2007 - Call of Duty 4: Modern Warfare
    [DataRow(true, "game:/code_post_gfx.ff#WaW")]           // 2008 - Call of Duty: World at War
    [DataRow(true, "game:/code_post_gfx.ff#MW2")]           // 2009 - Call of Duty: Modern Warfare 2
    [DataRow(true, "game:/code_post_gfx.ff#BO")]            // 2010 - Call of Duty: Black Ops
    [DataRow(true, "game:/code_post_gfx.ff#MW3")]           // 2011 - Call of Duty: Modern Warfare 3
    [DataRow(true, "game:/code_post_gfx.ff#BO2")]           // 2012 - Call of Duty: Black Ops II
    [DataRow(true, "game:/code_post_gfx.ff#Ghosts")]        // 2013 - Call of Duty: Ghosts
    [DataRow(true, "game:/code_post_gfx.ff#AW")]            // 2014 - Call of Duty: Advanced Warfare
    [DataRow(true, "game:/core_post_gfx.ff#BO3")]           // 2015 - Call of Duty: Black Ops III
    [DataRow(true, "game:/code_post_gfx.ff#IW")]            // 2016 - Call of Duty: Infinite Warfare
    [DataRow(true, "game:/code_post_gfx.ff#WWII")]          // 2017 - Call of Duty: WWII
    [DataRow(false, "game:/code_post_gfx.ff#BO4")]          // 2018 - Call of Duty: Black Ops 4
    [DataRow(true, "game:/code_post_gfx.ff#MW")]            // 2019 - Call of Duty: Modern Warfare
    [DataRow(false, "game:/code_post_gfx.ff#BOCW")]         // 2020 - Call of Duty: Black Ops Cold War
    [DataRow(false, "game:/code_post_gfx.ff#Vanguard")]     // 2021 - Call of Duty: Vanguard
    [DataRow(true, "game:/code_post_gfx.ff#COD:MW2")]       // 2022 - Call of Duty: Modern Warfare II
    public void DLG(bool installed, string sampleFile) {
        if (!installed) return;
        var dat = family.GetArchive(new Uri(sampleFile));
        if (sampleFile == "game:/code_post_gfx.ff#COD4") {
            var zone = IwZone(dat);
            Assert.AreEqual(1640, zone.Files.Count);
            Assert.AreEqual("scriptstrings.txt", zone.Files[0].Path);
            using var stream = zone.ReadData(zone.Files[0]).Result;
            var text = new StreamReader(stream, Encoding.ASCII).ReadToEnd();
            StringAssert.StartsWith(text, "1\tend\n2\ttag_origin\n");
        }
        else if (sampleFile == "game:/core_post_gfx.ff#BO3") {
            var zone = IwZone(dat);
            Assert.IsTrue(zone.Files.Any(f => f.Path == "language.cfg"));
        }
        else if (sampleFile == "game:/code_post_gfx.ff#Ghosts") {
            var zone = IwZone(dat);
            Assert.AreEqual(101, zone.Files.Count);
            Assert.AreEqual("options_graphics.cfg", zone.Files[0].Path);
            Assert.AreEqual("code_post_gfx", zone.Files[100].Path);
        }
        else if (sampleFile is "game:/code_post_gfx.ff#WaW" or "game:/code_post_gfx.ff#MW2" or "game:/code_post_gfx.ff#BO" or "game:/code_post_gfx.ff#MW3")
            Assert.AreEqual(0, IwZone(dat).Files.Count);
        else if (sampleFile is "game:/code_post_gfx.ff#AW" or "game:/code_post_gfx.ff#IW" or "game:/code_post_gfx.ff#WWII" or "game:/code_post_gfx.ff#MW")
            Assert.AreEqual(0, IwZone(dat).Files.Count);
    }

    [TestMethod]
    [DataRow(true, "game:/mp_backlot_load.ff#COD4")]        // 2007 - Call of Duty 4: Modern Warfare
    [DataRow(true, "game:/mp_makin_day_load.ff#WaW")]       // 2008 - Call of Duty: World at War
    [DataRow(true, "game:/mp_underpass_load.ff#MW2")]       // 2009 - Call of Duty: Modern Warfare 2
    [DataRow(true, "game:/ui_mp.ff#BO")]                    // 2010 - Call of Duty: Black Ops
    [DataRow(true, "game:/mp_alpha_load.ff#MW3")]           // 2011 - Call of Duty: Modern Warfare 3
    [DataRow(true, "game:/yemen_gump_outro.ff#BO2")]        // 2012 - Call of Duty: Black Ops II
    [DataRow(true, "game:/ui_install.ff#Ghosts")]           // 2013 - Call of Duty: Ghosts
    [DataRow(true, "game:/patch_sanfran.ff#AW")]            // 2014 - Call of Duty: Advanced Warfare
    [DataRow(true, "game:/core_post_gfx.ff#BO3")]           // 2015 - Call of Duty: Black Ops III
    [DataRow(true, "game:/code_pre_gfx.ff#IW")]             // 2016 - Call of Duty: Infinite Warfare
    [DataRow(true, "game:/code_post_gfx.ff#WWII")]          // 2017 - Call of Duty: WWII
    //[DataRow(true, "game:/karma_gump_checkin.ff#BO4")]    // 2018 - Call of Duty: Black Ops 4
    //[DataRow(true, "game:/karma_gump_checkin.ff#MW")]     // 2019 - Call of Duty: Modern Warfare
    //[DataRow(false, "game:/karma_gump_checkin.ff#BOCW")]   // 2020 - Call of Duty: Black Ops Cold War
    //[DataRow(false, "game:/karma_gump_checkin.ff#Vanguard")]   // 2021 - Call of Duty: Vanguard
    //[DataRow(true, "game:/karma_gump_checkin.ff#COD:MW2")]    // 2022 - Call of Duty: Modern Warfare II
    public void DLG2(bool installed, string sampleFile) {
        if (!installed) return;
        var dat = family.GetArchive(new Uri(sampleFile));
        if (sampleFile == "game:/mp_backlot_load.ff#COD4") {
            var zone = IwZone(dat);
            Assert.AreEqual(6, zone.Files.Count);
            Assert.AreEqual("techset/0000", zone.Files[0].Path);
            Assert.AreEqual("techset/0001", zone.Files[1].Path);
            Assert.AreEqual("material/0002", zone.Files[2].Path);
            Assert.AreEqual("material/0003", zone.Files[3].Path);
            Assert.AreEqual("material/0004", zone.Files[4].Path);
            Assert.AreEqual("rawfile/0005", zone.Files[5].Path);
            using var stream = zone.ReadData(zone.Files[2]).Result;
            var index = new byte[8];
            Assert.AreEqual(8, stream.Read(index, 0, 8));
            CollectionAssert.AreEqual(new byte[] { 4, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0xFF }, index);
        }
        else if (sampleFile == "game:/mp_makin_day_load.ff#WaW") {
            var zone = IwZone(dat);
            Assert.AreEqual(9, zone.Files.Count);
            Assert.AreEqual("sound/0000", zone.Files[0].Path);
            Assert.AreEqual("image/0001", zone.Files[1].Path);
            Assert.AreEqual("image/0002", zone.Files[2].Path);
            Assert.AreEqual("image/0003", zone.Files[3].Path);
            Assert.AreEqual("stringtable/0004", zone.Files[4].Path);
            Assert.AreEqual("stringtable/0008", zone.Files[8].Path);
        }
        else if (sampleFile == "game:/mp_underpass_load.ff#MW2") {
            var zone = IwZone(dat);
            Assert.AreEqual(5, zone.Files.Count);
            Assert.AreEqual("techset/0000", zone.Files[0].Path);
            Assert.AreEqual("material/0001", zone.Files[1].Path);
            Assert.AreEqual("material/0002", zone.Files[2].Path);
            Assert.AreEqual("material/0003", zone.Files[3].Path);
            Assert.AreEqual("rawfile/0004", zone.Files[4].Path);
            var pre = IwZone(family.GetArchive(new Uri("game:/code_pre_gfx.ff#MW2")));
            Assert.AreEqual(8, pre.Files.Count);
            Assert.AreEqual("codescripts/$default", pre.Files[0].Path);
            Assert.AreEqual("configure.csv", pre.Files[1].Path);
            Assert.AreEqual("code_pre_gfx", pre.Files[7].Path);
            using var csv = pre.ReadData(pre.Files[1]).Result;
            Assert.IsTrue(csv.Length > 100);
        }
        else if (sampleFile == "game:/ui_mp.ff#BO") {
            var zone = IwZone(dat);
            Assert.AreEqual(376, zone.Files.Count);
            Assert.AreEqual("VertexShader/0000", zone.Files[0].Path);
            var pre = IwZone(family.GetArchive(new Uri("game:/code_pre_gfx.ff#BO")));
            Assert.AreEqual(37, pre.Files.Count);
            Assert.AreEqual("StringTable/0000", pre.Files[0].Path);
            Assert.AreEqual("RawFile/0001", pre.Files[1].Path);
        }
        else if (sampleFile == "game:/mp_alpha_load.ff#MW3") {
            var zone = IwZone(dat);
            Assert.AreEqual(5, zone.Files.Count);
            Assert.AreEqual("techset/0000", zone.Files[0].Path);
            Assert.AreEqual("material/0001", zone.Files[1].Path);
            Assert.AreEqual("material/0003", zone.Files[3].Path);
            Assert.AreEqual("rawfile/0004", zone.Files[4].Path);
            var pre = IwZone(family.GetArchive(new Uri("game:/code_pre_gfx.ff#MW3")));
            Assert.AreEqual(7, pre.Files.Count);
            Assert.AreEqual("codescripts/$default", pre.Files[0].Path);
            Assert.AreEqual("configure.csv", pre.Files[1].Path);
            Assert.AreEqual("code_pre_gfx", pre.Files[6].Path);
            using var csv = pre.ReadData(pre.Files[1]).Result;
            Assert.IsTrue(csv.Length > 100);
        }
        else if (sampleFile == "game:/yemen_gump_outro.ff#BO2") {
            var pre = IwZone(family.GetArchive(new Uri("game:/code_pre_gfx.ff#BO2")));
            Assert.IsTrue(pre.Files.Any(f => f.Path == "configure.cfg"));
        }
        else if (sampleFile == "game:/ui_install.ff#Ghosts") {
            var zone = IwZone(dat);
            Assert.AreEqual(1, zone.Files.Count);
            Assert.AreEqual("ui_install", zone.Files[0].Path);
            var pre = IwZone(family.GetArchive(new Uri("game:/code_pre_gfx.ff#Ghosts")));
            Assert.AreEqual(6, pre.Files.Count);
            Assert.AreEqual("codescripts/$default", pre.Files[0].Path);
            Assert.AreEqual("configure.csv", pre.Files[1].Path);
            Assert.AreEqual("ragdoll.cfg", pre.Files[4].Path);
            Assert.AreEqual("code_pre_gfx", pre.Files[5].Path);
            using var csv = pre.ReadData(pre.Files[1]).Result;
            Assert.IsTrue(csv.Length > 100);
        }
        else if (sampleFile is "game:/patch_sanfran.ff#AW" or "game:/code_pre_gfx.ff#IW" or "game:/code_post_gfx.ff#WWII")
            Assert.AreEqual(0, IwZone(dat).Files.Count);
    }

    static IWArchive IwZone(Archive archive) {
        if (archive is IWArchive iw) return iw;
        var many = (BinaryArchive)archive;
        return many.Files.Select(f => f.Arc).OfType<IWArchive>().First();
    }
}

// https://github.com/mauserzjeh/iwi
// https://github.com/mauserzjeh/iwi2dds
// https://github.com/mauserzjeh/iwi2dds/blob/master/main.go
// https://github.com/mauserzjeh/cod-asset-importer
// https://wiki.zeroy.com/index.php?title=Call_of_Duty_4:_d3dbsp
// https://gist.github.com/Scobalula/a0fd08197497336f67b7ff551b2db404
// https://wiki.zeroy.com/images/b/b4/Unpack.png