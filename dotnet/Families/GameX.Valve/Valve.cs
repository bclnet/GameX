using GameX.Formats.IUnknown;
using GameX.ID.Formats;
using GameX.Transforms;
using GameX.Uncore;
using GameX.Valve.Formats;
using OpenStack;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Valve;

/// <summary>
/// ValveArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class ValveArchive : BinaryArchive, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="ValveArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public ValveArchive(BinaryState state) : base(state, GetArcBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        AssetFactoryFunc = AssetFactory;
        PathFinders.Add(typeof(object), FindBinary);
    }

    #region Factories

    static readonly ConcurrentDictionary<string, ArcBinary> ArcBinarys = new();

    static ArcBinary GetArcBinary(FamilyGame game, string extension)
        => ArcBinarys.GetOrAdd(game.Id, _ => game.Engine.n switch {
            "Unity" => Unity.Formats.Binary_Unity.Current,
            "GoldSrc" => Binary_Wad3.Current,
            "Source" or "Source2" => Binary_Vpk.Current,
            _ => throw new ArgumentOutOfRangeException(nameof(game.Engine), game.Engine.n),
        });

    public static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => game.Engine.n switch {
            "GoldSrc" => Path.GetExtension(source.Path).ToLowerInvariant() switch {
                ".pic" or ".tex" or ".tex2" or ".fnt" => (0, Binary_Wad3X.Factory),
                ".bsp" => (0, Binary_BspX.Factory),
                ".spr" => (0, Binary_Spr.Factory),
                ".mdl" => (0, Binary_Mdl10.Factory),
                _ => UncoreArchive.AssetFactory(source, game),
            },
            "Source" => Path.GetExtension(source.Path).ToLowerInvariant() switch {
                ".mdl" => (0, Binary_Mdl40.Factory),
                _ => UncoreArchive.AssetFactory(source, game),
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
        Log.Info($"Could not find file '{p}' in an arc file.");
        return null;
    }

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
