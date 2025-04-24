using GameX.Bohemia.Formats;
using GameX.Bohemia.Transforms;
using GameX.Formats.Unknown;
using GameX.Unknown;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Bohemia;

#region BohemiaPakFile

/// <summary>
/// BohemiaPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class BohemiaPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="BohemiaPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public BohemiaPakFile(PakState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    static PakBinary GetPakBinary(FamilyGame game, string extension)
        => extension switch {
            ".xxx" => Binary_XXX.Current,
            _ => throw new ArgumentOutOfRangeException(nameof(extension)),
        };

    static (object, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            _ => UnknownPakFile.ObjectFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}

#endregion
