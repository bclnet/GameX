using GameX.Formats.Unknown;
using GameX.Lucas.Formats;
using GameX.Transforms;
using GameX.Unknown;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Lucas;

/// <summary>
/// LucasPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class LucasPakFile : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="LucasPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public LucasPakFile(ArchiveState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    static ArcBinary GetPakBinary(FamilyGame game, string extension)
        => game.Engine.n switch {
            "SPUTM" => Binary_Scumm.Current,
            "Jedi" => Binary_Jedi.Current,
            _ => null,
        };

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".nwx" => (0, Binary_Nwx.Factory),
            ".san" => (0, Binary_San.Factory),
            _ => UnknownPakFile.ObjectFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}
