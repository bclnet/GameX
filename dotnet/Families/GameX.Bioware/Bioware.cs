using GameX.Bioware.Formats;
using GameX.Uncore.Formats;
using GameX.Formats.IUnknown;
using GameX.Transforms;
using GameX.Uncore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Bioware;

/// <summary>
/// BiowareArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class BiowareArchive : BinaryArchive, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="BiowareArchive" /> class.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="state">The state.</param>
    public BiowareArchive(Archive parent, BinaryState state) : base(parent, state, GetArcBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    static ArcBinary GetArcBinary(FamilyGame game, string extension)
        => (game.Engine.n, extension) switch {
            ("Infinity", ".key" or ".bif") => Binary_Key.Current,
            ("Aurora" or "Odyssey", ".key" or ".bif") => Binary_Aurora.Current,
            ("Omen" or "Aurora", ".zip") => Binary_Zip.GetArcBinary(game),
            ("Hero", ".tor") => Binary_Myp.Current,
            _ => throw new ArgumentOutOfRangeException(nameof(extension)),
        };

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".dlg" or ".qdb" or ".qst" => (0, Binary_Gff.Factory),
            _ => UncoreArchive.AssetFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(object source, Archive transformTo) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(object source, Archive transformTo) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
