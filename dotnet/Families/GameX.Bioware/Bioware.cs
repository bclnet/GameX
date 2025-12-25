using GameX.Bioware.Formats;
using GameX.Formats;
using GameX.Formats.IUnknown;
using GameX.Transforms;
using GameX.Uncore;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Bioware;

/// <summary>
/// BiowareArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class BiowareArchive : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="BiowareArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public BiowareArchive(ArchiveState state) : base(state, GetArcBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    static readonly ConcurrentDictionary<string, ArcBinary> ArcBinarys = new ConcurrentDictionary<string, ArcBinary>();

    static ArcBinary GetArcBinary(FamilyGame game, string extension)
        => extension != ".zip"
            ? ArcBinarys.GetOrAdd(game.Id, _ => PakBinaryFactory(game))
            : Binary_Zip.GetArcBinary(game);

    static ArcBinary PakBinaryFactory(FamilyGame game)
        => game.Engine.n switch {
            //"Infinity" => PakBinary_Infinity.Instance,
            "Aurora" => Binary_Aurora.Current,
            "Hero" => Binary_Myp.Current,
            "Odyssey" => Binary_Myp.Current,
            _ => throw new ArgumentOutOfRangeException(nameof(game.Engine))
        };

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".dlg" or ".qdb" or ".qst" => (0, Binary_Gff.Factory),
            _ => UncoreArchive.AssetFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
