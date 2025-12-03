using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace GameX.Unity.Formats;

[TestClass]
public class UnityFileTests {
    [TestMethod]
    [DataRow("Unity:AmongUs", "Sbi/Objects/animals/fish/CleanerFish_clean_prop_animal_01.chr")]
    public async Task LoadFileObjectAsync(string pak, string sampleFile) => await LoadFileObjectAsync(Helper.Paks[pak].Value, sampleFile);

    public async Task LoadFileObjectAsync(Archive source, string sampleFile) {
        Assert.IsTrue(source.Contains(sampleFile));
        var file = await source.GetAsset<byte[]>(sampleFile);
    }
}
