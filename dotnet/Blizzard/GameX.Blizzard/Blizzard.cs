﻿using GameX.Blizzard.Formats;
using GameX.Blizzard.Transforms;
using GameX.Formats.Unknown;
using GameX.Unknown;
using Microsoft.Extensions.FileSystemGlobbing;
using OpenStack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Blizzard;

#region BlizzardFileSystem

/// <summary>
/// BlizzardFileSystem
/// </summary>
/// <seealso cref="GameX.Family" />
public class BlizzardFileSystem : IFileSystem {
    public IEnumerable<string> Glob(string path, string searchPattern) {
        var matcher = new Matcher();
        matcher.AddIncludePatterns([searchPattern]);
        return matcher.GetResultsInFullPath(searchPattern);
    }

    public (string path, long length) FileInfo(string path) {
        throw new System.NotImplementedException();
    }

    public BinaryReader OpenReader(string path) {
        throw new System.NotImplementedException();
    }

    public BinaryWriter OpenWriter(string path) {
        throw new System.NotImplementedException();
    }

    public bool FileExists(string path) {
        throw new System.NotImplementedException();
    }
}

#endregion

#region BlizzardPakFile

/// <summary>
/// BlizzardPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class BlizzardPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="BlizzardPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public BlizzardPakFile(PakState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        ObjectFactoryFunc = ObjectFactory;
        UseReader = false;
    }

    #region Factories

    static PakBinary GetPakBinary(FamilyGame game, string extension)
        => Binary_Blizzard.Current;

    static (object, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            _ => UnknownPakFile.ObjectFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}

#endregion
