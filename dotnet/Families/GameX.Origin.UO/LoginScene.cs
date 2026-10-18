// SPDX-License-Identifier: BSD-2-Clause

using OpenX.Client;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO login steps.
/// </summary>
public enum LoginSteps {
    Main,
    Connecting,
    VerifyingAccount,
    ServerSelection,
    LoginInToServer,
    CharacterSelection,
    EnteringBritania,
    CharacterCreation,
    CharacterCreationDone,
    PopUpMessage
}

/// <summary>
/// ClassicUO's LoginScene. SceneBase so MgClientHost.SetScene can run it.
/// Load does not build gumps, play music, or resize the window yet.
/// </summary>
public sealed class LoginScene : SceneBase {
    readonly World _world;
    LoginSteps _lastLoginStep;
    bool _autoLogin;

    public LoginScene(World world) => _world = world;

    public World World => _world;
    public bool Reconnect { get; set; }
    public LoginSteps CurrentLoginStep { get; set; } = LoginSteps.Main;
    public CityInfo[] Cities { get; set; }
    public string[] Characters { get; private set; }
    public string PopupMessage { get; set; }
    public byte ServerIndex { get; private set; }
    public static string Account { get; internal set; }
    public string Password { get; private set; }
    public bool CanAutologin => _autoLogin || Reconnect;
    public (int min, int max) LoginDelay { get; private set; }

    public override void Load() {
        base.Load();
        // ClassicUO reads Settings.GlobalSettings.AutoLogin, adds LoginBackground and LoginGump,
        // plays login music, and sets the window to 640x480. Those types are not in this project yet.
        _autoLogin = false;
    }

    public override void Unload() {
        if (IsDestroyed) return;
        base.Unload();
    }

    public override void Update() {
        base.Update();
        if (_lastLoginStep == CurrentLoginStep) return;
        // ClassicUO removes each Item and Mobile here, then builds the gump for the step.
        _world.Items.Clear();
        _world.Mobiles.Clear();
        _lastLoginStep = CurrentLoginStep;
    }
}

/// <summary>
/// ClassicUO city row parsed from the character-list packet. No socket reader yet.
/// </summary>
public class CityInfo {
    public CityInfo(int index, string city, string building, string description, ushort x, ushort y, sbyte z, uint map, bool isNew) {
        Index = index;
        City = city;
        Building = building;
        Description = description;
        X = x;
        Y = y;
        Z = z;
        Map = map;
        IsNewCity = isNew;
    }

    public readonly string Building;
    public readonly string City;
    public readonly string Description;
    public readonly int Index;
    public readonly bool IsNewCity;
    public readonly uint Map;
    public readonly ushort X, Y;
    public readonly sbyte Z;
}
