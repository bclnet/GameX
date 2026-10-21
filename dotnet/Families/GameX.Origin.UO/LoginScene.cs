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
/// Load builds the login gumps. It does not play music, resize to 640x480, or open a socket.
/// Draw walks the gump tree and asks GumpArt.Sink to paint gump art.
/// </summary>
public sealed class LoginScene : SceneBase {
    readonly World _world;
    Gump _currentGump;
    LoginSteps _lastLoginStep;
    bool _autoLogin;

    public LoginScene(World world) => _world = world;

    public World World => _world;
    public Gump CurrentGump => _currentGump;
    public bool Reconnect { get; set; }
    public LoginSteps CurrentLoginStep { get; set; } = LoginSteps.Main;
    public CityInfo[] Cities { get; set; }
    public string[] Servers { get; private set; }
    public string[] Characters { get; private set; }
    public string PopupMessage { get; set; }
    public byte ServerIndex { get; private set; }
    public static string Account { get; internal set; }
    public string Password { get; private set; }
    public bool CanAutologin => _autoLogin || Reconnect;
    public (int min, int max) LoginDelay { get; private set; }

    public override void Load() {
        base.Load();
        // Login music and SetWindowSize(640, 480) stay on the host. This project has no MonoGame device.
        UIManager.Clear();
        _autoLogin = false;
        UIManager.Add(new LoginBackground(_world));
        UIManager.Add(_currentGump = new LoginGump(_world, this));
    }

    public override void Unload() {
        if (IsDestroyed) return;
        UIManager.Clear();
        _currentGump = null;
        base.Unload();
    }

    public override void Update() {
        base.Update();
        if (UOLoginNet.Take(out var servers, out var characters, out var message, out var phase)) {
            switch (phase) {
                case UOLoginPhase.ServersReady:
                    Servers = servers;
                    CurrentLoginStep = LoginSteps.ServerSelection;
                    break;
                case UOLoginPhase.CharactersReady:
                    Characters = characters;
                    CurrentLoginStep = LoginSteps.CharacterSelection;
                    break;
                case UOLoginPhase.Failed:
                    PopupMessage = message;
                    CurrentLoginStep = LoginSteps.PopUpMessage;
                    break;
                case UOLoginPhase.RelayInProgress:
                    CurrentLoginStep = LoginSteps.LoginInToServer;
                    break;
            }
        }
        if (_lastLoginStep != CurrentLoginStep) {
            _world.Items.Clear();
            _world.Mobiles.Clear();
            var next = GumpForStep();
            if (next != null) {
                var prev = _currentGump;
                UIManager.Add(_currentGump = next);
                prev?.Dispose();
            }
            _lastLoginStep = CurrentLoginStep;
        }
        UIManager.Update();
    }

    public override bool Draw() {
        var sink = GumpArt.Sink;
        sink?.Begin();
        try {
            RenderedObjectsCount = UIManager.Draw();
            return base.Draw();
        }
        finally { sink?.End(); }
    }

    public override void OnMouseMove(int x, int y) {
        UIManager.OnMouseMove(x, y);
        base.OnMouseMove(x, y);
    }

    public override bool OnMouseDown(int button, int x, int y)
        => UIManager.OnMouseDown(button, x, y) || base.OnMouseDown(button, x, y);

    public override bool OnMouseUp(int button, int x, int y)
        => UIManager.OnMouseUp(button, x, y) || base.OnMouseUp(button, x, y);

    public override bool OnMouseDoubleClick(int button, int x, int y)
        => UIManager.OnMouseDoubleClick(button, x, y) || base.OnMouseDoubleClick(button, x, y);

    public override bool OnKeyDown(int key, int mod)
        => UIManager.OnKeyDown(key, mod) || base.OnKeyDown(key, mod);

    public override bool OnKeyUp(int key, int mod)
        => UIManager.OnKeyUp(key, mod) || base.OnKeyUp(key, mod);

    public override void OnTextInput(string text) {
        UIManager.OnTextInput(text);
        base.OnTextInput(text);
    }

    /// <summary>
    /// Stores the account from LoginGump. Does not open a socket.
    /// </summary>
    internal void RememberLogin(string account, string password, bool autoLogin) {
        Account = account;
        Password = password;
        _autoLogin = autoLogin;
    }

    /// <summary>
    /// Appends a character name. Ignores null or whitespace. Does not send a packet.
    /// </summary>
    internal void AddCharacter(string name) {
        if (string.IsNullOrWhiteSpace(name)) return;
        Characters = [..(Characters ?? []), name];
    }

    // Main is LoginGump.
    Gump GumpForStep() => CurrentLoginStep == LoginSteps.Main
        ? new LoginGump(_world, this)
        : LoginStepGumps.Create(CurrentLoginStep, _world, this);
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
