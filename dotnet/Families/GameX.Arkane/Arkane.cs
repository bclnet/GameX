using GameX.Arkane.Formats;
using GameX.Arkane.Formats.Danae;
using GameX.Formats;
using GameX.Formats.IUnknown;
using GameX.Transforms;
using GameX.Uncore;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Arkane;

/// <summary>
/// ArkaneArchive
/// </summary>
/// <seealso cref="GameEstate.Formats.BinaryArchive" />
public class ArkaneArchive : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="Arkane" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public ArkaneArchive(ArchiveState state) : base(state, GetArcBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        AssetFactoryFunc = state.Game.Engine.n switch {
            "CryEngine" => Crytek.CrytekArchive.AssetFactory,
            "Unreal" => Epic.EpicArchive.AssetFactory,
            "Source" => Valve.ValveArchive.AssetFactory,
            "idTech" => ID.IDArchive.AssetFactory,
            _ => AssetFactory,
        };
        UseFileId = true;
    }

    #region Factories

    static readonly ConcurrentDictionary<string, ArcBinary> ArcBinarys = new();

    static ArcBinary GetArcBinary(FamilyGame game, string extension)
        => ArcBinarys.GetOrAdd(game.Id, _ => game.Engine.n switch {
            "Danae" => Binary_Danae.Current,
            "Void" => Binary_Void.Current,
            "CryEngine" => Crytek.Formats.Binary_Cry3.Current,
            "Unreal" => Epic.Formats.Binary_Pck.Current,
            "Source" => Valve.Formats.Binary_Vpk.Current,
            "idTech" => ID.Formats.Binary_Pak.Current,
            _ => throw new ArgumentOutOfRangeException(nameof(game.Engine)),
        });

    internal static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".asl" => (0, Binary_Txt.Factory),
            // Danae (AF)
            ".ftl" => (0, Binary_Ftl.Factory),
            ".fts" => (0, Binary_Fts.Factory),
            ".tea" => (0, Binary_Tea.Factory),
            //
            //".llf" => (0, Binary_Flt.Factory),
            //".dlf" => (0, Binary_Flt.Factory),
            _ => UncoreArchive.AssetFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
