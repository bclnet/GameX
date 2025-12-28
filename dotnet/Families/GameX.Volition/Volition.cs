using GameX.Uncore.Formats;
using GameX.Formats.IUnknown;
using GameX.Uncore;
using GameX.Volition.Formats;
using GameX.Transforms;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameX.Volition;

/// <summary>
/// DGame
/// </summary>
/// <seealso cref="GameX.FamilyGame" />
public class DGame(Family family, string id, JsonElement elem, FamilyGame dgame) : FamilyGame(family, id, elem, dgame) {
    /// <summary>
    /// Ensures this instance.
    /// </summary>
    /// <returns></returns>
    public override void Loaded() {
        base.Loaded();
        Games.D.Database.Loaded(this);
    }
}

/// <summary>
/// D2Game
/// </summary>
/// <seealso cref="GameX.FamilyGame" />
public class D2Game(Family family, string id, JsonElement elem, FamilyGame dgame) : FamilyGame(family, id, elem, dgame) {
    /// <summary>
    /// Ensures this instance.
    /// </summary>
    /// <returns></returns>
    public override void Loaded() {
        base.Loaded();
        Games.D2.Database.Loaded(this);
    }
}

/// <summary>
/// VolitionArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class VolitionArchive : BinaryArchive, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="VolitionArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public VolitionArchive(BinaryState state) : base(state, GetArcBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant())) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    static readonly ConcurrentDictionary<string, ArcBinary> ArcBinarys = new ConcurrentDictionary<string, ArcBinary>();

    static ArcBinary GetArcBinary(FamilyGame game, string extension)
        => ArcBinarys.GetOrAdd(game.Id, _ => game.Engine.n switch {
            "Descent" => Binary_Descent.Current,
            "CTG" => Binary_Ctg.Current,
            "Geo-Mod" => Binary_GeoMod.Current,
            "Geo-Mod2" => Binary_GeoMod.Current,
            _ => throw new ArgumentOutOfRangeException(nameof(game.Engine.n)),
        });

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            ".256" => (0, Binary_Pal.Factory_3),
            ".mvl" => (0, Binary_Mvl.Factory),
            _ => UncoreArchive.AssetFactory(source, game),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
