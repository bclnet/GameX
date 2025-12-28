using GameX.Crytek.Formats;
using GameX.Crytek.Transforms;
using GameX.Formats.IUnknown;
using GameX.Uncore;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("GameX.Cig")]

namespace GameX.Crytek;

/// <summary>
/// CrytekArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class CrytekArchive : BinaryArchive, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="CrytekArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public CrytekArchive(BinaryState state) : base(state, GetArcBinary(state.Game)) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    static readonly ConcurrentDictionary<string, ArcBinary> ArcBinarys = new ConcurrentDictionary<string, ArcBinary>();

    static ArcBinary GetArcBinary(FamilyGame game)
        => ArcBinarys.GetOrAdd(game.Id, _ => PakBinaryFactory(game));

    static ArcBinary PakBinaryFactory(FamilyGame game)
        => game.Engine.n switch {
            "ArcheAge" => new Binary_ArcheAge((byte[])game.Key),
            _ => new Binary_Cry3((byte[])game.Key),
        };

    public static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".xml" => (0, CryXmlFile.Factory),
            ".cgf" or ".cga" or ".chr" or ".skin" or ".anim" => (0, CryFile.Factory),
            _ => UncoreArchive.AssetFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
