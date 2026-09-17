using GameX.Valve.Formats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameX.Valve;

[TestClass]
public class PakBinaryTest {
    static readonly Family family = FamilyManager.GetFamily("Valve");
    //static readonly Archive dota2 = family.OpenArchive(new Uri("game:/platform_misc_dir.vpk#H2"));

    //[TestMethod]
    //public void AGRP()
    //{
    //    var pak1 = family.OpenArchive(new Uri("game:/platform_misc_dir.vpk#H2"));
    //    var pak2 = family.OpenArchive(new Uri("vpk_file_not_ending_in_vpk.vpk.0123456789abc"));
    //    pak2.Contains("kitten.jpg");
    //}

    //[TestMethod]
    //[DataRow("kitten.jpg")]
    //[DataRow("addons\\chess\\chess.vdf")]
    //[DataRow("addons/chess\\chess.vdf")]
    //[DataRow("addons/chess/chess.vdf")]
    //[DataRow("\\addons/chess/chess.vdf")]
    //[DataRow("\\addons/chess/chess.vdf")]
    //[DataRow("/addons/chess/chess.vdf")]
    //[DataRow("\\addons/chess/hello_github_reader.vdf")]
    //[DataRow("\\addons/hello_github_reader/chess.vdf")]
    //public void DATATexture(string sampleFile) => LoadObject<Binary_Src>(dota2, sampleFile);
}
