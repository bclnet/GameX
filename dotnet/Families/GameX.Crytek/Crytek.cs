using GameX.Crytek.Formats;
using GameX.Crytek.Transforms;
using GameX.Formats.Unknown;
using GameX.Unknown;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("GameX.Cig")]

namespace GameX.Crytek;

/// <summary>
/// CrytekPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class CrytekPakFile : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="CrytekPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public CrytekPakFile(ArchiveState state) : base(state, GetPakBinary(state.Game)) {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    static readonly ConcurrentDictionary<string, ArcBinary> PakBinarys = new ConcurrentDictionary<string, ArcBinary>();

    static ArcBinary GetPakBinary(FamilyGame game)
        => PakBinarys.GetOrAdd(game.Id, _ => PakBinaryFactory(game));

    static ArcBinary PakBinaryFactory(FamilyGame game)
        => game.Engine.n switch {
            "ArcheAge" => new Binary_ArcheAge((byte[])game.Key),
            _ => new Binary_Cry3((byte[])game.Key),
        };

    public static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".xml" => (0, CryXmlFile.Factory),
            ".cgf" or ".cga" or ".chr" or ".skin" or ".anim" => (0, CryFile.Factory),
            _ => UnknownPakFile.ObjectFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}
