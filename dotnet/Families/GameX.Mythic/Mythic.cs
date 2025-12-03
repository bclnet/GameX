using GameX.Bioware.Formats;
using GameX.Formats.Unknown;
using GameX.Gamebryo.Formats;
using GameX.Mythic.Formats;
using GameX.Transforms;
using GameX.Unknown;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Mythic;

/// <summary>
/// MythicArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class MythicArchive : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="MythicArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public MythicArchive(ArchiveState state) : base(state, GetArcBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    static ArcBinary GetArcBinary(FamilyGame game, string extension)
        => extension switch {
            "" => null,
            ".mpk" or ".npk" => Binary_Mpk.Current,
            ".myp" => Binary_Myp.Current,
            _ => throw new ArgumentOutOfRangeException(nameof(extension)),
        };

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".nif" => (FileOption.StreamObject, Binary_Nif.Factory),
            ".crf" => (FileOption.StreamObject, Binary_Crf.Factory),
            _ => UnknownArchive.AssetFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
