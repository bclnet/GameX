using GameX.Formats.Unknown;
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
    /// <summary>
    /// Ensures this instance.
    /// </summary>
    /// <returns></returns>
    //public override FamilyGame Ensure() {
    //    return this;
    //}
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
    //public override FamilyGame Ensure() {
    //    return Games.U9.Database.Ensure(this);
    //}
}

/// <summary>
/// OriginGame
/// </summary>
/// <seealso cref="GameX.FamilyGame" />
public class UOGame(Family family, string id, JsonElement elem, FamilyGame dgame) : FamilyGame(family, id, elem, dgame) {
    ClientVersion Version;
    ClientFlags Protocol;

    /// <summary>
    /// Ensures this instance.
    /// </summary>
    /// <returns></returns>
    public override void Loaded() {
        base.Loaded();
        var clientVersionText = Options.TryGetValue("clientVersion", out var z) ? z as string : null;
        if (!string.IsNullOrWhiteSpace(clientVersionText)) clientVersionText = clientVersionText.Replace(",", ".").Replace(" ", "").ToLowerInvariant();
        var clientVersion = ClientVersionHelper.ValidateClientVersion(clientVersionText);
        if (clientVersion == null) {
            Log.Warn($"Client version [{clientVersionText}] is invalid, let's try to read the client.exe");
            if ((clientVersionText = ClientVersionHelper.ParseFromFile(Path.Combine(Found.Root, "client.exe"))) == null || (clientVersion = ClientVersionHelper.ValidateClientVersion(clientVersionText)) == null) {
                Log.Error($"Invalid client version: {clientVersionText}");
                throw new Exception($"Invalid client version: '{clientVersionText}'");
            }
            Log.Trace($"Found a valid client.exe [{clientVersionText} - {clientVersion}]");
            Options["clientVersion"] = clientVersionText;
            Options.Dirty = true;
        }
        Version = clientVersion ?? 0;
        Protocol = ClientFlags.CF_T2A;
        if (Version >= ClientVersion.CV_200) Protocol |= ClientFlags.CF_RE;
        if (Version >= ClientVersion.CV_300) Protocol |= ClientFlags.CF_TD;
        if (Version >= ClientVersion.CV_308) Protocol |= ClientFlags.CF_LBR;
        if (Version >= ClientVersion.CV_308Z) Protocol |= ClientFlags.CF_AOS;
        if (Version >= ClientVersion.CV_405A) Protocol |= ClientFlags.CF_SE;
        if (Version >= ClientVersion.CV_60144) Protocol |= ClientFlags.CF_SA;
        Log.Trace($"Client version: {clientVersion}");
        Log.Trace($"Protocol: {Protocol}");
    }
}

/// <summary>
/// OriginPakFile
/// </summary>
/// <seealso cref="GameX.Formats.BinaryPakFile" />
public class OriginPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="OriginPakFile" /> class.
    /// </summary>
    /// <param name="state">The state.</param>
    public OriginPakFile(PakState state) : base(state, GetPakBinary(state.Game)) {
        ObjectFactoryFunc = ObjectFactory;
    }

    #region Factories

    static PakBinary GetPakBinary(FamilyGame game)
        => game.Id switch {
            "U8" => Binary_U8.Current,
            "UO" => Binary_UO.Current,
            "U9" => Binary_U9.Current,
            _ => throw new ArgumentOutOfRangeException(),
        };

    static (object, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactory(FileSource source, FamilyGame game)
        => game.Id switch {
            "U8" => Binary_U8.ObjectFactory(source, game),
            "UO" => Binary_UO.ObjectFactory(source, game),
            "U9" => Binary_U9.ObjectFactory(source, game),
            _ => throw new ArgumentOutOfRangeException(),
        };

    #endregion

    #region Transforms

    bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
    Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

    #endregion
}
