﻿using GameX.Formats.Unknown;
using GameX.ID.Formats;
using GameX.Transforms;
using GameX.Unknown;
using GameX.Valve.Formats;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameX.Valve;

#region ValvePakFile

/// <summary>
/// ValvePakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class ValvePakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="ValvePakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public ValvePakFile(PakState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        ObjectFactoryFunc = ObjectFactory;
        PathFinders.Add(typeof(object), FindBinary);
    }

    #region Factories

    static readonly ConcurrentDictionary<string, PakBinary> PakBinarys = new();

    static PakBinary GetPakBinary(FamilyGame game, string extension)
        => PakBinarys.GetOrAdd(game.Id, _ => game.Engine.n switch {
            "Unity" => Unity.Formats.Binary_Unity.Current,
            "GoldSrc" => Binary_Wad3.Current,
            "Source" or "Source2" => Binary_Vpk.Current,
            _ => throw new ArgumentOutOfRangeException(nameof(game.Engine), game.Engine.n),
        });

    public static (object, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => game.Engine.n switch {
            "GoldSrc" => Path.GetExtension(source.Path).ToLowerInvariant() switch {
                ".pic" or ".tex" or ".tex2" or ".fnt" => (0, Binary_Wad3X.Factory),
                ".bsp" => (0, Binary_BspX.Factory),
                ".spr" => (0, Binary_Spr.Factory),
                ".mdl" => (0, Binary_Mdl10.Factory),
                _ => UnknownPakFile.ObjectFactory(source, game),
            },
            "Source" => Path.GetExtension(source.Path).ToLowerInvariant() switch {
                ".mdl" => (0, Binary_Mdl40.Factory),
                _ => UnknownPakFile.ObjectFactory(source, game),
            },
            "Source2" => (0, Binary_Src.Factory),
            _ => throw new ArgumentOutOfRangeException(nameof(game.Engine), game.Engine.n),
        };

    #endregion

    #region PathFinders

    /// <summary>
    /// Finds the actual path of a texture.
    /// </summary>
    public object FindBinary(object path) {
        if (path is not string p) return path;
        if (Contains(p)) return p;
        if (!p.EndsWith("_c", StringComparison.Ordinal)) path = $"{p}_c";
        if (Contains(p)) return p;
        Log($"Could not find file '{p}' in a PAK file.");
        return null;
    }

    #endregion

    #region Transforms

    bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}

#endregion
