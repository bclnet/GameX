using GameX.Formats.IUnknown;
using GameX.Transforms;
using GameX.Uncore;
using GameX.Xbox.Formats;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameX.Xbox;

/// <summary>
/// StardewValleyGame
/// </summary>
/// <seealso cref="GameX.FamilyGame" />
public class StardewValleyGame(Family family, string id, JsonElement elem, FamilyGame dgame) : FamilyGame(family, id, elem, dgame) { }

/// <summary>
/// XboxArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class XboxArchive : BinaryArchive, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="XboxArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public XboxArchive(BinaryState state) : base(state, GetArcBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        AssetFactoryFunc = AssetFactory;
        TypeX.ScanTypes([typeof(XboxArchive)]);
    }

    #region Factories

    static ArcBinary GetArcBinary(FamilyGame game, string extension)
        => extension switch {
            "" => null,
            ".xxx" => Binary_XXX.Current,
            _ => throw new ArgumentOutOfRangeException(nameof(extension)),
        };

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".xnb" => (0, Binary_Xnb.Factory),
            _ => UncoreArchive.AssetFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
