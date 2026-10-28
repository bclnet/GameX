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
    bool _rightWalk;
    bool _continueRun;
    bool _hasGoal;
    int _goalX, _goalY;
    int _stuckX, _stuckY, _stuckPx, _stuckPy, _stuckAt;

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
        if (CurrentLoginStep == LoginSteps.InWorld) PropTip.Poll(_world);
        DriveMouse();
    }

    /// <summary>
    /// ClassicUO right-button walk. Direction is GetMouseDirection minus one, and North wraps through 8.
    /// The run bit is set at 190 pixels from the player anchor. Left-up while right is held keeps walking.
    /// A left-up on empty ground steps toward that tile until the player arrives.
    /// </summary>
    void DriveMouse() {
        if (CurrentLoginStep != LoginSteps.InWorld) {
            _rightWalk = false;
            _continueRun = false;
            _hasGoal = false;
            return;
        }
        if (Mouse.RButtonPressed) {
            if (_rightWalk || _continueRun) {
                _hasGoal = false;
                WalkFacing();
            }
            return;
        }
        _rightWalk = false;
        if (_continueRun) {
            _hasGoal = false;
            WalkFacing();
            return;
        }
        if (!_hasGoal) return;
        var px = UOLoginNet.PlayerX;
        var py = UOLoginNet.PlayerY;
        if (_stuckX != _goalX || _stuckY != _goalY || _stuckPx != px || _stuckPy != py) {
            _stuckX = _goalX;
            _stuckY = _goalY;
            _stuckPx = px;
            _stuckPy = py;
            _stuckAt = System.Environment.TickCount;
        }
        else if (unchecked(System.Environment.TickCount - _stuckAt) > 1000) {
            _hasGoal = false;
            return;
        }
        var gx = _goalX - px;
        var gy = _goalY - py;
        if (gx == 0 && gy == 0) {
            _hasGoal = false;
            return;
        }
        var step = StepDir(Sign(gx), Sign(gy));
        if (Abs(gx) + Abs(gy) > 2) step |= 0x80;
        UOLoginNet.Walk(step);
    }

    static void WalkFacing() {
        var facing = GameCursor.GetMouseDirection(320, 220, Mouse.Position.X, Mouse.Position.Y, 1);
        if (facing == 0) facing = 8;
        var walk = (byte)((facing - 1) & 7);
        var dx = Mouse.Position.X - 320;
        var dy = Mouse.Position.Y - 220;
        if ((long)dx * dx + (long)dy * dy >= 190L * 190L) walk |= 0x80;
        UOLoginNet.Walk(walk);
    }

    static bool OverWorld() {
        var over = UIManager.MouseOverControl;
        return over == null || over is WorldGump || over is UseLabel;
    }

    static int Sign(int n) => n < 0 ? -1 : n > 0 ? 1 : 0;
    static int Abs(int n) => n < 0 ? -n : n;

    static byte StepDir(int sx, int sy) => (sx, sy) switch {
        (0, -1) => 0,
        (1, -1) => 1,
        (1, 0) => 2,
        (1, 1) => 3,
        (0, 1) => 4,
        (-1, 1) => 5,
        (-1, 0) => 6,
        _ => 7,
    };

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

    public override bool OnMouseDown(int button, int x, int y) {
        if (CurrentLoginStep == LoginSteps.InWorld && button == (int)MouseButtonType.Right) {
            _continueRun = false;
            if (OverWorld()) _rightWalk = true;
        }
        return UIManager.OnMouseDown(button, x, y) || base.OnMouseDown(button, x, y);
    }

    public override bool OnMouseUp(int button, int x, int y) {
        if (CurrentLoginStep == LoginSteps.InWorld && button == (int)MouseButtonType.Left && Mouse.RButtonPressed && _rightWalk)
            _continueRun = true;
        var over = UIManager.MouseOverControl;
        var ui = UIManager.OnMouseUp(button, x, y);
        if (CurrentLoginStep == LoginSteps.InWorld && button == (int)MouseButtonType.Left) {
            WorldGump.GroundTile(x, y, out var wx, out var wy, out var wz);
            if (UOLoginNet.HasTarget) {
                if (WorldPick.TryActor(_world, x, y, out var serial)) UOLoginNet.TargetSerial(serial);
                else UOLoginNet.TargetGround(wx, wy, wz);
                return true;
            }
            if (UOLoginNet.Holding) {
                var drop = WorldPick.TryActor(_world, x, y, out var onto) ? onto : 0xFFFFFFFFu;
                UOLoginNet.DropHeld(wx, wy, wz, drop);
                return true;
            }
            if (!ui && (over is WorldGump || over == null)) {
                _continueRun = false;
                if (WorldPick.TryActor(_world, x, y, out var picked)) {
                    _hasGoal = false;
                    UOLoginNet.RequestPopup(picked);
                }
                else {
                    _goalX = wx;
                    _goalY = wy;
                    _hasGoal = true;
                }
                return true;
            }
        }
        if (ui) return true;
        return base.OnMouseUp(button, x, y);
    }

    public override bool OnMouseDoubleClick(int button, int x, int y) {
        if (UIManager.OnMouseDoubleClick(button, x, y)) return true;
        if (CurrentLoginStep == LoginSteps.InWorld && button == (int)MouseButtonType.Left
            && WorldPick.TryActor(_world, x, y, out var serial)) {
            _hasGoal = false;
            if (serial != UOLoginNet.PlayerSerial && UOLoginNet.IsMobile(serial) && UOLoginNet.InWarMode)
                UOLoginNet.Attack(serial);
            else UOLoginNet.Use(serial);
            return true;
        }
        return base.OnMouseDoubleClick(button, x, y);
    }

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
            if (TryWorldKey(key, mod)) return true;
        }
        return UIManager.OnKeyDown(key, mod) || base.OnKeyDown(key, mod);
    }

    /// <summary>
    /// ClassicUO CreateDefaultMacros. Alt is SDL_KMOD_ALT (0x0300). Shift and Ctrl stay up.
    /// UoKeys.Map leaves letters as XNA Keys, which are uppercase ASCII, not SDLK.
    /// Paperdoll and backpack call Use. Journal and the radar map are the gumps TopBar opens.
    /// </summary>
    bool TryWorldKey(int key, int mod) {
        if ((mod & 0x0300) == 0 || (mod & 0x00C3) != 0) return false;
        switch (key) {
            case 80: // Keys.P, GameActions.OpenPaperdoll
                var player = UOLoginNet.PlayerSerial;
                if (player != 0) UOLoginNet.Use(player);
                return true;
            case 73: // Keys.I, GameActions.OpenBackpack
                var pack = UOLoginNet.PlayerBackpack;
                if (pack != 0) UOLoginNet.Use(pack);
                return true;
            case 74: // Keys.J, GameActions.OpenJournal
                if (UIManager.GetGump<JournalGump>() == null) UIManager.Add(new JournalGump(_world));
                return true;
            case 82: // Keys.R, GameActions.OpenMiniMap
                if (UIManager.GetGump<MiniMapGump>() == null) UIManager.Add(new MiniMapGump(_world));
                return true;
            default:
                return false;
        }
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
