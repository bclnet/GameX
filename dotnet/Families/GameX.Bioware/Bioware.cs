using GameX.Bioware.Formats;
using GameX.Formats;
using GameX.Formats.Unknown;
using GameX.Transforms;
using GameX.Unknown;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Bioware;

/// <summary>
/// BiowarePakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class BiowarePakFile : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="BiowarePakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public BiowarePakFile(ArchiveState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    static readonly ConcurrentDictionary<string, ArcBinary> PakBinarys = new ConcurrentDictionary<string, ArcBinary>();

    static ArcBinary GetPakBinary(FamilyGame game, string extension)
        => extension != ".zip"
            ? PakBinarys.GetOrAdd(game.Id, _ => PakBinaryFactory(game))
            : Binary_Zip.GetPakBinary(game);

    static ArcBinary PakBinaryFactory(FamilyGame game)
        => game.Engine.n switch {
            //"Infinity" => PakBinary_Infinity.Instance,
            "Aurora" => Binary_Aurora.Current,
            "Hero" => Binary_Myp.Current,
            "Odyssey" => Binary_Myp.Current,
            _ => throw new ArgumentOutOfRangeException(nameof(game.Engine))
        };

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".dlg" or ".qdb" or ".qst" => (0, Binary_Gff.Factory),
            _ => UnknownPakFile.ObjectFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}
