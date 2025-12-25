using GameX.Formats.IUnknown;
using GameX.Origin.Clients.UO.Data;
using GameX.Origin.Formats;
using GameX.Origin.Formats.UO;
using GameX.Transforms;
using OpenStack;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameX.Origin;

/// <summary>
/// U8Game
/// </summary>
/// <seealso cref="GameX.FamilyGame" />
public class U8Game(Family family, string id, JsonElement elem, FamilyGame dgame) : FamilyGame(family, id, elem, dgame) {
    public override string ToString() => $"U8";
}

/// <summary>
/// U9Game
/// </summary>
/// <seealso cref="GameX.FamilyGame" />
public class U9Game(Family family, string id, JsonElement elem, FamilyGame dgame) : FamilyGame(family, id, elem, dgame) {
    /// <summary>
    /// Ensures this instance.
    /// </summary>
    /// <returns></returns>
    //public override FamilyGame Ensure() => Games.U9.Database.Ensure(this);
    public override string ToString() => $"U9";
}

/// <summary>
/// UOGame
/// </summary>
/// <seealso cref="GameX.FamilyGame" />
public class UOGame(Family family, string id, JsonElement elem, FamilyGame dgame) : FamilyGame(family, id, elem, dgame) {
    internal bool Uop;
    internal ClientVersion Version;
    internal ClientFlags Protocol;

    /// <summary>
    /// Ensures this instance.
    /// </summary>
    /// <returns></returns>
    public override void Loaded() {
        base.Loaded();
        var versionText = Options.TryGetValue("version", out var z) ? z as string : null;
        if (!string.IsNullOrWhiteSpace(versionText)) versionText = versionText.Replace(",", ".").Replace(" ", "").ToLowerInvariant();
        var version = ClientVersionHelper.ValidateClientVersion(versionText);
        if (version == null) {
            Log.Warn($"Client version [{versionText}] is invalid, let's try to read the client.exe");
            if ((versionText = ClientVersionHelper.ParseFromFile(Path.Combine(Found.Root, "client.exe"))) == null || (version = ClientVersionHelper.ValidateClientVersion(versionText)) == null) {
                Log.Error($"Invalid client version: {versionText}");
                throw new Exception($"Invalid client version: '{versionText}'");
            }
            Log.Trace($"Found a valid client.exe [{versionText} - {version}]");
            Options["version"] = versionText;
            Options.Dirty = true;
        }
        Uop = version >= ClientVersion.CV_7000 && File.Exists(Path.Combine(Found.Root, "MainMisc.uop"));
        Version = version ?? 0;
        Protocol = ClientFlags.CF_T2A;
        if (Version >= ClientVersion.CV_200) Protocol |= ClientFlags.CF_RE;
        if (Version >= ClientVersion.CV_300) Protocol |= ClientFlags.CF_TD;
        if (Version >= ClientVersion.CV_308) Protocol |= ClientFlags.CF_LBR;
        if (Version >= ClientVersion.CV_308Z) Protocol |= ClientFlags.CF_AOS;
        if (Version >= ClientVersion.CV_405A) Protocol |= ClientFlags.CF_SE;
        if (Version >= ClientVersion.CV_60144) Protocol |= ClientFlags.CF_SA;
        Log.Trace($"Uop: {Uop}");
        Log.Trace($"Version: {Version}");
        Log.Trace($"Protocol: {Protocol}");
    }

    public override string ToString() => $"UO - {Version}";
}

/// <summary>
/// OriginArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class OriginArchive : BinaryAsset, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="OriginArchive" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public OriginArchive(ArchiveState state) : base(state, GetArcBinary(state.Game)) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    static ArcBinary GetArcBinary(FamilyGame game)
        => game.Id switch {
            "U8" => Binary_U8.Current,
            "UO" => Binary_UO.Current,
            "U9" => Binary_U9.Current,
            _ => throw new ArgumentOutOfRangeException(),
        };

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => game.Id switch {
            "U8" => Binary_U8.AssetFactory(source, game),
            "UO" => Binary_UO.AssetFactory(source, game),
            "U9" => Binary_U9.AssetFactory(source, game),
            _ => throw new ArgumentOutOfRangeException(),
        };

    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(Archive transformTo, object source) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(Archive transformTo, object source) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
