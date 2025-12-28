using GameX.Blizzard.Formats;
using GameX.Formats.IUnknown;
using GameX.Transforms;
using GameX.Uncore;
using Microsoft.Extensions.FileSystemGlobbing;
using OpenStack.Vfx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Blizzard;

/// <summary>
/// BlizzardFileSystem
/// </summary>
/// <seealso cref="GameX.Family" />
public class BlizzardFileSystem : FileSystem {
    public override IEnumerable<string> Glob(string path, string searchPattern) {
        var matcher = new Matcher();
        matcher.AddIncludePatterns([searchPattern]);
        return matcher.GetResultsInFullPath(searchPattern);
    }

    public override (string path, long length) FileInfo(string path) {
        throw new System.NotImplementedException();
    }

    public override Stream Open(string path, string mode) {
        throw new System.NotImplementedException();
    }

    public override bool FileExists(string path) {
        throw new System.NotImplementedException();
    }
}

/// <summary>
/// BlizzardArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class BlizzardArchive : BinaryArchive, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="BlizzardArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public BlizzardArchive(BinaryState state) : base(state, GetArcBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        AssetFactoryFunc = AssetFactory;
        UseReader = false;
    }

    #region Factories

    static ArcBinary GetArcBinary(FamilyGame game, string extension)
        => Binary_Blizzard.Current;

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            _ => UncoreArchive.AssetFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
