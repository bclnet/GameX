using GameX.Blizzard.Formats;
using GameX.Formats.Unknown;
using GameX.Transforms;
using GameX.Unknown;
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
/// BlizzardPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class BlizzardPakFile : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="BlizzardPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public BlizzardPakFile(ArchiveState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        ObjectFactoryFunc = ObjectFactory;
        UseReader = false;
    }

    #region Factories

    static ArcBinary GetPakBinary(FamilyGame game, string extension)
        => Binary_Blizzard.Current;

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            _ => UnknownPakFile.ObjectFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}
