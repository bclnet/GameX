using GameX.Formats.Unknown;
using GameX.Lucas.Formats;
using GameX.Transforms;
using GameX.Unknown;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Lucas;

/// <summary>
/// LucasArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class LucasArchive : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="LucasArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public LucasArchive(ArchiveState state) : base(state, GetArcBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    static ArcBinary GetArcBinary(FamilyGame game, string extension)
        => game.Engine.n switch {
            "SPUTM" => Binary_Scumm.Current,
            "Jedi" => Binary_Jedi.Current,
            _ => null,
        };

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".nwx" => (0, Binary_Nwx.Factory),
            ".san" => (0, Binary_San.Factory),
            _ => UnknownArchive.AssetFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
