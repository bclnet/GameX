using GameX.Formats;
using GameX.Formats.IUnknown;
using GameX.Monolith.Formats;
using GameX.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Monolith;

/// <summary>
/// MonolithArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class MonolithArchive : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="MonolithArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public MonolithArchive(ArchiveState state) : base(state, GetArcBinary(state.Game, state.Path)) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    static ArcBinary GetArcBinary(FamilyGame game, string filePath)
        => filePath == null || Path.GetExtension(filePath).ToLowerInvariant() != ".zip"
            ? Binary_Lith.Current
            : Binary_Zip.GetArcBinary(game);

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".dds" => (0, Binary_Dds.Factory),
            _ => (0, null),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
