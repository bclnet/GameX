using GameX.Uncore.Formats;
using GameX.Formats.IUnknown;
using GameX.ID.Formats;
using GameX.Transforms;
using GameX.Uncore;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameX.ID;

/// <summary>
/// QGame
/// </summary>
/// <seealso cref="GameX.FamilyGame" />
public class QGame(Family family, string id, JsonElement elem, FamilyGame dgame) : FamilyGame(family, id, elem, dgame) {
    /// <summary>
    /// Ensures this instance.
    /// </summary>
    /// <returns></returns>
    public override void Loaded() {
        base.Loaded();
        Games.Q.Database.Loaded(this);
    }
}

/// <summary>
/// IDArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class IDArchive : BinaryArchive, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="IDArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public IDArchive(BinaryState state) : base(state, GetArcBinary(state.Game, state.Path)) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    static ArcBinary GetArcBinary(FamilyGame game, string filePath)
         => Path.GetExtension(filePath).ToLowerInvariant() switch {
             "" => null,
             ".pk3" or ".pk4" or ".zip" => Binary_Zip.GetArcBinary(game),
             ".arc" => Binary_Pak.Current,
             ".wad" => Binary_Wad.Current,
             _ => throw new ArgumentOutOfRangeException(),
         };

    public static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => game.Engine.n switch {
            "idTech" => Path.GetExtension(source.Path).ToLowerInvariant() switch {
                ".lmp" or ".tex" => (0, Binary_Lmp.Factory),
                ".bsp" => (0, Binary_BspX.Factory),
                ".mdl" => (0, Binary_Mdl.Factory),
                ".spr" => (0, Binary_Spr.Factory),
                _ => UncoreArchive.AssetFactory(source, game),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(game.Engine), game.Engine.n),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
