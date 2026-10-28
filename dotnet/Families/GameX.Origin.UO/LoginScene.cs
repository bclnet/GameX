// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
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
    InWorld,
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
    public string ServerName { get; internal set; }
    public static string Account { get; internal set; }
    public string Password { get; private set; }
    public bool CanAutologin => _autoLogin || Reconnect;
    public (int min, int max) LoginDelay { get; private set; }

    public override void Load() {
        base.Load();
        // Login music and SetWindowSize(640, 480) stay on the host. This project has no MonoGame device.
        UIManager.Clear();
        UOLoginSettings.Load();
        _autoLogin = UOLoginSettings.AutoLogin;
        UIManager.Add(new LoginBackground(_world));
        UIManager.Add(_currentGump = new LoginGump(_world, this));
        if (CanAutologin && !string.IsNullOrEmpty(UOLoginSettings.Username)) {
            Account = UOLoginSettings.Username;
            Password = UOLoginSettings.Password;
            var uo = UltimaOnline.Current;
            if (uo != null && !string.IsNullOrEmpty(uo.Host) && uo.Port > 0) {
                CurrentLoginStep = LoginSteps.Connecting;
                UOLoginNet.Begin(uo.Host, uo.Port, Account, Password, uo.Version, uo.VersionText, uo.Plain);
            }
        }
    }

    public override void Unload() {
        if (IsDestroyed) return;
        UIManager.Clear();
        _currentGump = null;
        base.Unload();
    }

    public override void Update() {
        GameController.Update(this);
        base.Update();
        if (UOLoginNet.Take(out var servers, out var characters, out var message, out var phase)) {
            switch (phase) {
                case UOLoginPhase.ServersReady:
                    Servers = servers;
                    CurrentLoginStep = LoginSteps.ServerSelection;
                    if (CanAutologin && servers != null && servers.Length > 0) {
                        var index = UOLoginSettings.ServerIndex(servers);
                        ServerIndex = (byte)index;
                        ServerName = servers[index];
                        UOLoginSettings.NoteServer(ServerName, index);
                        UOLoginNet.SelectServer(index);
                        CurrentLoginStep = LoginSteps.LoginInToServer;
                    }
                    break;
                case UOLoginPhase.CharactersReady:
                    Characters = characters;
                    CurrentLoginStep = LoginSteps.CharacterSelection;
                    if (CanAutologin) {
                        _autoLogin = false;
                        var slot = UOLoginSettings.CharacterSlot(characters, Account, ServerName);
                        if (slot >= 0) {
                            UOLoginSettings.NoteCharacter(Account, ServerName, characters[slot]);
                            UOLoginNet.SelectCharacter(slot, characters[slot]);
                            CurrentLoginStep = LoginSteps.EnteringBritania;
                        }
                        else CurrentLoginStep = LoginSteps.CharacterCreation;
                    }
                    break;
                case UOLoginPhase.Failed:
                    PopupMessage = message;
                    CurrentLoginStep = LoginSteps.PopUpMessage;
                    break;
                case UOLoginPhase.RelayInProgress:
                    CurrentLoginStep = LoginSteps.LoginInToServer;
                    break;
                case UOLoginPhase.WorldEntered:
                    CurrentLoginStep = LoginSteps.InWorld;
                    break;
            }
        }
        if (_lastLoginStep != CurrentLoginStep) {
            if (CurrentLoginStep != LoginSteps.InWorld) {
                _world.Items.Clear();
                _world.Mobiles.Clear();
            }
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
            GameCursor.Draw(CurrentLoginStep == LoginSteps.InWorld);
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

    public override bool OnMouseUp(int button, int x, int y) {
        if (UIManager.OnMouseUp(button, x, y)) return true;
        if (CurrentLoginStep != LoginSteps.InWorld || button != 1) return base.OnMouseUp(button, x, y);
        WorldGump.GroundTile(x, y, out var wx, out var wy, out var wz);
        if (UOLoginNet.HasTarget) {
            UOLoginNet.TargetGround(wx, wy, wz);
            return true;
        }
        if (UOLoginNet.Holding) {
            UOLoginNet.DropHeld(wx, wy, wz, 0xFFFFFFFF);
            return true;
        }
        return base.OnMouseUp(button, x, y);
    }

    public override bool OnMouseDoubleClick(int button, int x, int y)
        => UIManager.OnMouseDoubleClick(button, x, y) || base.OnMouseDoubleClick(button, x, y);

    public override bool OnKeyDown(int key, int mod) {
        if (CurrentLoginStep == LoginSteps.InWorld && TryWalk(key, mod)) return true;
        if (CurrentLoginStep == LoginSteps.InWorld) {
            if (key == 9) {
                UOLoginNet.ToggleWar();
                return true;
            }
            if (key == 27) {
                if (UOLoginNet.HasTarget) {
                    UOLoginNet.CancelTarget();
                    return true;
                }
                if (UOLoginNet.HasPrompt) {
                    UOLoginNet.ReplyPrompt("", true);
                    return true;
                }
            }
        }
        return UIManager.OnKeyDown(key, mod) || base.OnKeyDown(key, mod);
    }

    /// <summary>SDL keycodes. Shift (KMOD_SHIFT, bits 0 and 1) sets the run bit.</summary>
    static bool TryWalk(int key, int mod) {
        byte? dir = key switch {
            1073741906 or 1073741920 => 0, // up, kp 8
            1073741921 => 1, // kp 9
            1073741903 or 1073741918 => 2, // right, kp 6
            1073741915 => 3, // kp 3
            1073741905 or 1073741914 => 4, // down, kp 2
            1073741913 => 5, // kp 1
            1073741904 or 1073741916 => 6, // left, kp 4
            1073741919 => 7, // kp 7
            _ => null,
        };
        if (dir is not byte d) return false;
        if ((mod & 3) != 0) d |= 0x80;
        UOLoginNet.Walk(d);
        return true;
    }

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
