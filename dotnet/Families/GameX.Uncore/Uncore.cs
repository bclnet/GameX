using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameX.Uncore;

/// <summary>
/// UncoreFamily
/// </summary>
/// <seealso cref="GameX.Family" />
public class UncoreFamily(JsonElement elem) : Family(elem) { }

/// <summary>
/// UncoreArchive
/// </summary>
/// <seealso cref="GameX.Formats.Archive" />
public class UncoreArchive : Archive {
    /// <summary>
    /// Initializes a new instance of the <see cref="UncoreArchive" /> class.
    /// </summary>
    /// <param name="state">The game.</param>
    public UncoreArchive(ArchiveState state) : base(state) {
        Name = "Uncore";
        AssetFactoryFunc = AssetFactory;
    }

    #region Base

    //public override void Dispose() { }
    public override int Count => 0;
    public override void Closing() { }
    public override void Opening() { }
    public override bool Contains(object path) => false;
    public override (Archive, FileSource) GetSource(object path, bool throwOnError = true) => throw new NotImplementedException();
    public override Task<Stream> GetData(object path, object option = default, bool throwOnError = true) => throw new NotImplementedException();
    public override Task<T> GetAsset<T>(object path, object option = default, bool throwOnError = true) => throw new NotImplementedException();

    #endregion

    #region Factories

    public static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".txt" or ".ini" or ".cfg" or ".csv" or ".xml" => (0, Binary_Txt.Factory),
            ".wav" or ".mp3" => (0, Binary_Snd.Factory),
            ".bmp" or ".jpg" or ".png" or ".gif" or ".tiff" => (0, Binary_Img.Factory), // Exif
            ".pcx" => (0, Binary_Pcx.Factory),
            ".tga" => (0, Binary_Tga.Factory),
            ".dds" => (0, Binary_Dds.Factory),
            _ => source.Path switch {
                "testtri.gfx" => (0, Binary_TestTri.Factory),
                _ => (0, null),
            }
        };

    #endregion
}