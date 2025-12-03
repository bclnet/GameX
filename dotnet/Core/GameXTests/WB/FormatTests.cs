using GameX.WB.Formats;
using GameX.WB.Formats.AC.FileTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameX.WB;

[TestClass]
public class FormatTests {
    static readonly Family family = FamilyManager.GetFamily("WB");
    static readonly Archive cell = family.OpenArchive(new Uri("game:/client_cell_1.dat#AC")); const int ExpectedCellCount = 805003;
    static readonly Archive portal = family.OpenArchive(new Uri("game:/client_portal.dat#AC")); const int ExpectedPortalCount = 79694;
    static readonly Archive localEnglish = family.OpenArchive(new Uri("game:/client_local_English.dat#AC")); const int ExpectedLocalEnglishCount = 118;

    [TestMethod]
    public void LoadCellDat_NoExceptions() {
        var dat = new Database(cell);
        var count = dat.Source.Count;
        Assert.IsLessThanOrEqualTo(count, ExpectedCellCount, $"Insufficient files parsed from .dat. Expected: >= {ExpectedCellCount}, Actual: {count}");
    }

    [TestMethod]
    public void LoadPortalDat_NoExceptions() {
        var dat = new Database(portal);
        var count = dat.Source.Count;
        Assert.IsLessThanOrEqualTo(count, ExpectedPortalCount, $"Insufficient files parsed from .dat. Expected: >= {ExpectedPortalCount}, Actual: {count}");
    }

    [TestMethod]
    public void LoadLocalEnglishDat_NoExceptions() {
        var dat = new Database(localEnglish);
        var count = dat.Source.Count;
        Assert.IsLessThanOrEqualTo(count, ExpectedLocalEnglishCount, $"Insufficient files parsed from .dat. Expected: >= {ExpectedLocalEnglishCount}, Actual: {count}");
    }

    [TestMethod]
    public async Task UnpackCellDatFiles_NoExceptions() {
        var dat = new Database(cell);
        var source = dat.Source;
        foreach (var (key, file) in source.FilesById.Select(x => KeyValuePair.Create(x.Key, x.First()))) {
            if ((uint)key == Iteration.FILE_ID) continue;
            if (file.FileSize == 0) continue; // DatFileType.LandBlock files can be empty

            var (fileType, ext) = WBArchive.GetFileType(file, PakType.Cell);
            Assert.IsNotNull(ext, $"Key: 0x{key:X8}, ObjectID: 0x{file.Id:X8}, FileSize: {file.FileSize}");

            var factory = source.EnsureCachedObjectFactory(file);
            if (factory == null) throw new Exception($"Class for fileType: {fileType} does not implement an AssetFactory.");

            using var r = new BinaryReader(await source.GetData(file));
            await factory(r, file, source);
            if (r.Tell() != file.FileSize) throw new Exception($"Failed to parse all bytes for fileType: {fileType}, ObjectId: 0x{file.Id:X8}. Bytes parsed: {r.Tell()} of {file.FileSize}");
        }
    }

    [TestMethod]
    public async Task UnpackPortalDatFiles_NoExceptions() {
        var dat = new Database(portal);
        var source = dat.Source;
        foreach (var (key, file) in source.FilesById.Select(x => KeyValuePair.Create(x.Key, x.First()))) {
            if ((uint)key == Iteration.FILE_ID) continue;

            var (fileType, ext) = WBArchive.GetFileType(file, PakType.Portal);
            Assert.IsNotNull(ext, $"Key: 0x{key:X8}, ObjectID: 0x{file.Id:X8}, FileSize: {file.FileSize}");

            // These file types aren't converted yet
            if (fileType == ArchiveType.KeyMap) continue;
            if (fileType == ArchiveType.RenderMaterial) continue;
            if (fileType == ArchiveType.MaterialModifier) continue;
            if (fileType == ArchiveType.MaterialInstance) continue;
            if (fileType == ArchiveType.ActionMap) continue;
            if (fileType == ArchiveType.MasterProperty) continue;
            if (fileType == ArchiveType.DbProperties) continue;

            var factory = source.EnsureCachedObjectFactory(file);
            if (factory == null) throw new Exception($"Class for fileType: {fileType} does not implement an AssetFactory.");

            using var r = new BinaryReader(await source.GetData(file));
            await factory(r, file, source);
            if (r.Tell() != file.FileSize) throw new Exception($"Failed to parse all bytes for fileType: {fileType}, ObjectId: 0x{file.Id:X8}. Bytes parsed: {r.Tell()} of {file.FileSize}");
        }
    }

    [TestMethod]
    public async Task UnpackLocalEnglishDatFiles_NoExceptions() {
        var dat = new Database(localEnglish);
        var source = dat.Source;
        foreach (var (key, file) in source.FilesById.Select(x => KeyValuePair.Create(x.Key, x.First()))) {
            if ((uint)key == Iteration.FILE_ID) continue;

            var (fileType, ext) = WBArchive.GetFileType(file, PakType.Language);
            Assert.IsNotNull(ext, $"Key: 0x{key:X8}, ObjectID: 0x{file.Id:X8}, FileSize: {file.FileSize}");

            // These file types aren't converted yet
            if (fileType == ArchiveType.UILayout) continue;

            var factory = source.EnsureCachedObjectFactory(file);
            if (factory == null) throw new Exception($"Class for fileType: {fileType} does not implement an AssetFactory.");

            using var r = new BinaryReader(await source.GetData(file));
            await factory(r, file, source);
            if (r.Tell() != file.FileSize) throw new Exception($"Failed to parse all bytes for fileType: {fileType}, ObjectId: 0x{file.Id:X8}. Bytes parsed: {r.Tell()} of {file.FileSize}");
        }
    }

    // uncomment if you want to run this
    // [TestMethod]
    public void ExtractCellDatByLandblock() {
        //var output = @"C:\T_\cell_dat_export_by_landblock";
        var dat = new DatabaseCell(cell);
        //dat.ExtractLandblockContents(output);
    }

    // uncomment if you want to run this
    // [TestMethod]
    public void ExportPortalDatsWithTypeInfo() {
        //var output = @"C:\T_\typed_portal_dat_export";
        var dat = new DatabasePortal(portal);
        //dat.ExtractCategorizedPortalContents(output);
    }
}
