using GameX.Beamdog.Formats;
using GameX.Formats.Unknown;
using GameX.Transforms;
using GameX.Unknown;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Beamdog;

/// <summary>
/// BeamdogPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class BeamdogPakFile : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="BeamdogPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public BeamdogPakFile(ArchiveState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    static ArcBinary GetPakBinary(FamilyGame game, string extension)
        => extension switch {
            ".xxx" => Binary_XXX.Current,
            _ => throw new ArgumentOutOfRangeException(nameof(extension)),
        };

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            _ => UnknownPakFile.ObjectFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}
