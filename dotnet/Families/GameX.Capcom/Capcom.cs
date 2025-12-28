using GameX.Capcom.Formats;
using GameX.Uncore.Formats;
using GameX.Formats.IUnknown;
using GameX.Transforms;
using GameX.Uncore;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Capcom;

/// <summary>
/// CapcomArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class CapcomArchive : BinaryArchive, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="CapcomArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public CapcomArchive(BinaryState state) : base(state, GetArcBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        AssetFactoryFunc = state.Game.Engine.n switch {
            "Unity" => Unity.UnityArchive.AssetFactory,
            _ => AssetFactory,
        };
    }

    #region Factories

    static readonly ConcurrentDictionary<string, ArcBinary> ArcBinarys = new();

    static ArcBinary GetArcBinary(FamilyGame game, string extension) => ArcBinarys.GetOrAdd(game.Id, _ => PakBinaryFactory(game, extension));

    static ArcBinary PakBinaryFactory(FamilyGame game, string extension)
        => game.Engine.n switch {
            "Zip" => Binary_Zip.GetArcBinary(game),
            "Unity" => Unity.Formats.Binary_Unity.Current,
            _ => extension switch {
                ".kpka" => Binary_Kpka.Current,
                ".arc" => Binary_Arc.Current,
                ".big" => Binary_Big.Current,
                ".bundle" => Binary_Bundle.Current,
                ".mbundle" => Binary_Plist.Current,
                _ => null, //throw new ArgumentOutOfRangeException(nameof(extension)),
            },
        };

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
