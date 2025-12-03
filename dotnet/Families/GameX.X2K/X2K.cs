using GameX.Formats.Unknown;
using GameX.Transforms;
using GameX.Unknown;
using GameX.X2K.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.X2K;

/// <summary>
/// X2KPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class X2KPakFile : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="X2KPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public X2KPakFile(ArchiveState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
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
