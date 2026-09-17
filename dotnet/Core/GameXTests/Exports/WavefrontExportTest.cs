using GameX.Formats.IUnknown;
using GameX.Uncore.Formats.Wavefront;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace GameX.Exports;

[TestClass]
public class WavefrontExportTest {
    [TestMethod]
    [DataRow("Rsi:StarCitizen", "Sbi/Objects/animals/fish/CleanerFish_clean_prop_animal_01.chr")]
    //[DataRow("Rsi:StarCitizen", "Data/Objects/buildingsets/human/hightech/prop/hydroponic/hydroponic_machine_1_incubator_01x01x02_a.cgf")]
    //[DataRow("Rsi:StarCitizen", "Data/Objects/buildingsets/human/hightech/prop/hydroponic/hydroponic_machine_1_incubator_02x01x012_a.cgf")]
    //[DataRow("Rsi:StarCitizen", "Data/Objects/buildingsets/human/hightech/prop/hydroponic/hydroponic_machine_1_incubator_rotary_025x01x0225_a/cgf")]
    //[DataRow("Rsi:StarCitizen", "Data/Objects/Characters/Human/male_v7/armor/nvy/pilot_flightsuit/m_nvy_pilot_light_armor_helmet_01.skin")]
    public async Task ExportFileObjectAsync(string arc, string sampleFile) => await ExportFileObjectAsync(TestHelper.Paks[arc].Value, sampleFile);

    public async Task ExportFileObjectAsync(Archive source, string sampleFile) {
        Assert.IsTrue(source.Contains(sampleFile));
        var file = await source.GetAsset<IUnknownFileModel>(sampleFile, FamilyManager.UncoreArchive);
        var objFile = new WavefrontFileWriter(file);
        objFile.Write(@"C:\T_\Models", false);
    }
}
