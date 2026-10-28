// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using OpenX.Client;
using System;
using System.IO;
using System.Reflection;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO LoginGump for the main login step.
/// No StbTextBox, no HTML links, no music slider.
/// Next arrow reads Login.cfg and calls UOLoginNet.Begin. Autologin is uo-settings.json.
/// </summary>
public class LoginGump : Gump {
    readonly LoginScene _scene;
    readonly ushort _buttonNormal, _buttonOver;
    readonly TextBox _account, _password;
    readonly Checkbox _auto;
    Checkbox _save;
    readonly Button _nextArrow0;
    uint _time;

    public LoginGump(World world, LoginScene scene) : base(world, 0, 0) {
        _scene = scene;
        CanCloseWithRightClick = false;
        AcceptKeyboardInput = false;

        int offsetX, offsetY, offtextY;
        byte font;
        ushort hue;
        var version = UltimaOnline.Current?.Version ?? 0;
        var versionText = UltimaOnline.Current?.VersionText;
        if (string.IsNullOrEmpty(versionText)) versionText = version.ToString();

        if (version < ClientVersion.CV_706400) {
            _buttonNormal = 0x15A4;
            _buttonOver = 0x15A5;
            const ushort HUE = 0x0386;
            if (version >= ClientVersion.CV_500A) Add(new GumpPic(0, 0, 0x2329, 0));
            Add(new GumpPic(0, 4, 0x15A0, 0) { AcceptKeyboardInput = false });
            Add(new Button((int)Buttons.Quit, 0x1589, 0x158B, 0x158A) {
                X = 555, Y = 4, ButtonAction = ButtonAction.Activate,
            });
            Add(new ResizePic(0x13BE) { X = 128, Y = 288, Width = 451, Height = 157 });
            if (version < ClientVersion.CV_500A) Add(new GumpPic(286, 45, 0x058A, 0));
            Add(new Button((int)Buttons.Credits, 0x1583, 0x1585, 0x1584) {
                X = 60, Y = 385, ButtonAction = ButtonAction.Activate,
            });
            Add(new Label("Log in to Ultima Online", false, HUE, 2) { X = 253, Y = 305 });
            Add(new Label("Account Name", false, HUE, 2) { X = 183, Y = 345 });
            Add(new Label("Password", false, HUE, 2) { X = 183, Y = 385 });
            Add(_nextArrow0 = new Button((int)Buttons.NextArrow, 0x15A4, 0x15A6, 0x15A5) {
                X = 610, Y = 445, ButtonAction = ButtonAction.Activate,
            });
            offsetX = 328;
            offsetY = 343;
            offtextY = 40;
            Add(new Label($"UO Version {versionText}.", false, 0x034E, 9) { X = 286, Y = 453 });
            Add(new Label("GameX", false, 0x034E, 9) { X = 286, Y = 465 });
            Add(_auto = new Checkbox(0x00D2, 0x00D3, "Autologin", 1, HUE, false) { X = 150, Y = 417 });
            _save = new Checkbox(0x00D2, 0x00D3, "Save Account", 1, HUE, false) {
                X = _auto.X + _auto.Width + 10, Y = 417,
            };
            Add(_save);
            font = 1;
            hue = HUE;
            AddMusic(font, hue, _save.X + _save.Width + 10);
        }
        else {
            _buttonNormal = 0x5CD;
            _buttonOver = 0x5CB;
            Add(new GumpPic(0, 0, 0x014E, 0));
            Add(new Button((int)Buttons.Quit, 0x05CA, 0x05C9, 0x05C8) {
                X = 25, Y = 240, ButtonAction = ButtonAction.Activate,
            });
            Add(new Button((int)Buttons.Credits, 0x05D0, 0x05CF, 0x5CE) {
                X = 530, Y = 125, ButtonAction = ButtonAction.Activate,
            });
            Add(_nextArrow0 = new Button((int)Buttons.NextArrow, 0x5CD, 0x5CC, 0x5CB) {
                X = 280, Y = 365, ButtonAction = ButtonAction.Activate,
            });
            offsetX = 218;
            offsetY = 283;
            offtextY = 50;
            Add(new Label($"UO Version {versionText}.", false, 0x0481, 9) { X = 286, Y = 453 });
            Add(new Label("GameX", false, 0x0481, 9) { X = 286, Y = 465 });
            Add(_auto = new Checkbox(0x00D2, 0x00D3, "Autologin", 9, 0x0481, false) { X = 150, Y = 417 });
            _save = new Checkbox(0x00D2, 0x00D3, "Save Account", 9, 0x0481, false) {
                X = _auto.X + _auto.Width + 10, Y = 417,
            };
            Add(_save);
            font = 9;
            hue = 0x0481;
            AddMusic(font, hue, _save.X + _save.Width + 10);
        }

        offsetX += 7;
        Add(new ResizePic(0x0BB8) { X = offsetX - 7, Y = offsetY, Width = 210, Height = 30 });
        Add(new ResizePic(0x0BB8) { X = offsetX - 7, Y = offsetY + offtextY, Width = 210, Height = 30 });
        Add(_account = new TextBox { X = offsetX, Y = offsetY, Width = 190, Height = 25, Hue = 0x034F, MaxLength = 16 });
        Add(_password = new TextBox {
            X = offsetX, Y = offsetY + offtextY + 2, Width = 190, Height = 25, Hue = 0x034F, MaxLength = 16, IsPassword = true,
        });
        UOLoginSettings.Load();
        if (_auto != null) _auto.IsChecked = UOLoginSettings.AutoLogin;
        if (_save != null) _save.IsChecked = UOLoginSettings.SaveAccount;
        if (!string.IsNullOrEmpty(UOLoginSettings.Username)) _account.Text = UOLoginSettings.Username;
        if (!string.IsNullOrEmpty(UOLoginSettings.Password)) _password.Text = UOLoginSettings.Password;
        if (string.IsNullOrEmpty(_account.Text)) _account.SetKeyboardFocus();
        else _password.SetKeyboardFocus();
    }

    public TextBox AccountBox => _account;
    public TextBox PasswordBox => _password;

    public override void OnKeyboardReturn(int textID, string text) => Submit();

    public override void Update() {
        if (IsDisposed) return;
        base.Update();
        if (_nextArrow0 != null && !_nextArrow0.IsDisposed && _time < GlobalTime.Ticks) {
            _time = GlobalTime.Ticks + 1000;
            _nextArrow0.ButtonGraphicNormal = _nextArrow0.ButtonGraphicNormal == _buttonNormal ? _buttonOver : _buttonNormal;
        }
        FocusHue(_account);
        FocusHue(_password);
    }

    public override void OnButtonClick(int buttonID) {
        switch ((Buttons)buttonID) {
            case Buttons.NextArrow: Submit(); break;
            case Buttons.Credits:
                if (UIManager.GetGump<CreditsGump>() == null)
                    UIManager.Add(new CreditsGump(World));
                break;
            // Quit would close the window. ISceneHost has no Exit.
        }
    }

    void Submit() {
        if (UIManager.GetGump<CreditsGump>() != null) return;
        if (_account == null || _account.IsDisposed) return;
        var auto = _auto != null && !_auto.IsDisposed && _auto.IsChecked;
        var save = _save != null && !_save.IsDisposed && _save.IsChecked;
        UOLoginSettings.AutoLogin = auto;
        UOLoginSettings.SaveAccount = save;
        UOLoginSettings.Username = _account.Text;
        UOLoginSettings.Password = _password?.Text ?? "";
        UOLoginSettings.Save();
        _scene.RememberLogin(_account.Text, _password?.Text, auto);
        if (!UOLoginServers.TryFirst(UltimaOnline.Current?.ClientPath, out var host, out var port)) return;
        _scene.CurrentLoginStep = LoginSteps.Connecting;
        UOLoginNet.Begin(host, port, _account.Text, _password?.Text, UltimaOnline.Current?.Version ?? default, UltimaOnline.Current?.VersionText);
    }

    static void FocusHue(TextBox box) {
        if (box == null || box.IsDisposed) return;
        var hue = box.HasKeyboardFocus ? (ushort)0x0021 : (ushort)0;
        if (box.Hue != hue) box.Hue = hue;
    }

    void AddMusic(byte font, ushort hue, int x) {
        var music = new Checkbox(0x00D2, 0x00D3, "Music", font, hue, false) { X = x, Y = 417 };
        music.IsChecked = true;
        music.ValueChanged += OnMusicChanged;
        Add(music);
    }

    static void OnMusicChanged(object sender, EventArgs e) {
        if (sender is Checkbox box) TrySetMusic(box.IsChecked);
    }

    // Origin.UO cannot reference GameX.Origin.Mg. Same lookup UOLogin uses for Attach.
    static void TrySetMusic(bool on) {
        try {
            Assembly asm = null;
            foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
                if (loaded.GetName().Name == "GameX.Origin.Mg") { asm = loaded; break; }
            if (asm == null) {
                var file = Path.Combine(AppContext.BaseDirectory, "GameX.Origin.Mg.dll");
                if (!File.Exists(file)) return;
                asm = Assembly.LoadFrom(file);
            }
            asm.GetType("GameX.Origin.Mg.UO.UoMusic")?.GetMethod("SetEnabled")?.Invoke(null, new object[] { on });
        }
        catch { }
    }

    enum Buttons { NextArrow, Quit, Credits }
}
