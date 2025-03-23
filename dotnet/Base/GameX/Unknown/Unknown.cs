﻿using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameX.Unknown;

/// <summary>
/// UnknownFamily
/// </summary>
/// <seealso cref="GameX.Family" />
public class UnknownFamily : Family
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownFamily"/> class.
    /// </summary>
    internal UnknownFamily() : base() { }
    public UnknownFamily(JsonElement elem) : base(elem) { }
}

/// <summary>
/// UnknownPakFile
/// </summary>
/// <seealso cref="GameX.Formats.PakFile" />
public class UnknownPakFile : PakFile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownPakFile" /> class.
    /// </summary>
    /// <param name="state">The game.</param>
    public UnknownPakFile(PakState state) : base(state)
    {
        Name = "Unknown";
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Base

    //public override void Dispose() { }
    public override int Count => 0;
    public override void Closing() { }
    public override void Opening() { }
    public override bool Contains(object path) => false;
    public override (PakFile, FileSource) GetFileSource(object path, bool throwOnError = true) => throw new NotImplementedException();
    public override Task<Stream> LoadFileData(object path, FileOption option = default, bool throwOnError = true) => throw new NotImplementedException();
    public override Task<T> LoadFileObject<T>(object path, FileOption option = default, bool throwOnError = true) => throw new NotImplementedException();

    #endregion

    #region Factories

    public static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch
        {
            ".txt" or ".ini" or ".cfg" or ".csv" or ".xml" => (0, Binary_Txt.Factory),
            ".wav" => (0, Binary_Snd.Factory),
            ".bmp" or ".jpg" or ".png" or ".gif" or ".tiff" => (0, Binary_Img.Factory), // Exif
            ".pcx" => (0, Binary_Pcx.Factory),
            ".tga" => (0, Binary_Tga.Factory),
            ".dds" => (0, Binary_Dds.Factory),
            _ => source.Path switch
            {
                "testtri.gfx" => (0, Binary_TestTri.Factory),
                _ => (0, null),
            }
        };

    #endregion

    #region Binary

    public class Binary_TestTri : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_TestTri());

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => [
                new(null, new MetaContent { Type = "TestTri", Name = Path.GetFileName(file.Path), Value = this }),
            ];
    }

    #endregion
}