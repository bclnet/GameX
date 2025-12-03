using GameX.Bullfrog.Formats;
using GameX.Formats.Unknown;
using GameX.Transforms;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameX.Bullfrog;

/// <summary>
/// DKGame
/// </summary>
/// <seealso cref="GameX.FamilyGame" />
public class DKGame(Family family, string id, JsonElement elem, FamilyGame dgame) : FamilyGame(family, id, elem, dgame) {
    /// <summary>
    /// Ensures this instance.
    /// </summary>
    /// <returns></returns>
    public override void Loaded() {
        base.Loaded();
        Games.DK.Database.Loaded(this);
    }
}

/// <summary>
/// DK2Game
/// </summary>
/// <seealso cref="GameX.FamilyGame" />
public class DK2Game(Family family, string id, JsonElement elem, FamilyGame dgame) : FamilyGame(family, id, elem, dgame) {
    /// <summary>
    /// Ensures this instance.
    /// </summary>
    /// <returns></returns>
    public override void Loaded() {
        base.Loaded();
        Games.DK2.Database.Loaded(this);
    }
}

/// <summary>
/// P2Game
/// </summary>
/// <seealso cref="GameX.FamilyGame" />
public class P2Game(Family family, string id, JsonElement elem, FamilyGame dgame) : FamilyGame(family, id, elem, dgame) {
    /// <summary>
    /// Ensures this instance.
    /// </summary>
    /// <returns></returns>
    public override void Loaded() {
        base.Loaded();
        Games.P2.Database.Loaded(this);
    }
}

/// <summary>
/// SGame
/// </summary>
/// <seealso cref="GameX.FamilyGame" />
public class SGame(Family family, string id, JsonElement elem, FamilyGame dgame) : FamilyGame(family, id, elem, dgame) {
    /// <summary>
    /// Ensures this instance.
    /// </summary>
    /// <returns></returns>
    public override void Loaded() {
        base.Loaded();
        Games.S.Database.Loaded(this);
    }
}

/// <summary>
/// BullfrogArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class BullfrogArchive : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="BullfrogArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public BullfrogArchive(ArchiveState state) : base(state, GetArcBinary(state.Game, state.Path))
        => AssetFactoryFunc = AssetFactory;

    #region Factories

    static ArcBinary GetArcBinary(FamilyGame game, string filePath)
        => game.Id switch {
            "DK" or "DK2" => Binary_Bullfrog.Current,           // Keeper
            "P" or "P2" or "P3" => Binary_Populus.Current, // Populus
            "S" or "S2" => Binary_Syndicate.Current,             // Syndicate
            "MC" or "MC2" => Binary_Bullfrog.Current,           // Carpet
            "TP" or "TH" => Binary_Bullfrog.Current,            // Theme
            _ => throw new ArgumentOutOfRangeException(),
        };

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => game.Id switch {
            "DK" or "DK2" => Binary_Bullfrog.AssetFactory(source, game),
            "P" or "P2" or "P3" => Binary_Populus.AssetFactory(source, game),
            "S" or "S2" => Binary_Syndicate.AssetFactory(source, game),
            _ => throw new ArgumentOutOfRangeException(),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
