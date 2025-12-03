using GameX.Formats.Unknown;
using GameX.Transforms;
using GameX.Unknown;
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
/// XboxPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class XboxPakFile : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="XboxPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public XboxPakFile(ArchiveState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        ObjectFactoryFunc = ObjectFactory;
        TypeX.ScanTypes([typeof(XboxPakFile)]);
    }

    #region Factories

    static ArcBinary GetPakBinary(FamilyGame game, string extension)
        => extension switch {
            "" => null,
            ".xxx" => Binary_XXX.Current,
            _ => throw new ArgumentOutOfRangeException(nameof(extension)),
        };

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".xnb" => (0, Binary_Xnb.Factory),
            _ => UnknownPakFile.ObjectFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}
