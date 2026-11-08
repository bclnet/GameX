// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO HotkeyBox. Key and Mod are ints: an SDL keycode and SDL_Keymod. SDL is not referenced.
/// A left click activates the box. OK and Cancel are the check and X buttons.
/// Alt and Ctrl are not stored on GameController, so a mouse bind sees Shift only.
/// </summary>
public class HotkeyBox : Control {
    const int ModShift = 0x0003;
    const int ModCtrl = 0x00C0;
    const int ModAlt = 0x0300;
    const int FieldW = 150;

    static readonly Dictionary<int, string> Names = new Dictionary<int, string>();

    readonly Button _buttonOK;
    readonly Button _buttonCancel;
    readonly Label _label;
    bool _active;

    static HotkeyBox() {
        Add(0, "None");
        Add(8, "Backspace");
        Add(9, "Tab");
        Add(13, "Return");
        Add(27, "Esc");
        Add(32, "Space");
        Add(33, "!");
        Add(36, "$");
        Add(37, "%");
        Add(39, "'");
        Add(42, "*");
        Add(43, "+");
        Add(44, ",");
        Add(45, "-");
        Add(46, ".");
        Add(47, "/");
        for (var c = '0'; c <= '9'; c++) Add(c, c.ToString());
        Add(58, ":");
        Add(59, ";");
        Add(60, "<");
        Add(61, "=");
        Add(62, ">");
        Add(63, "?");
        Add(91, "[");
        Add(92, "\\");
        Add(93, "]");
        Add(94, "^");
        Add(95, "_");
        Add(96, "BACKQUOTE");
        for (var c = 'a'; c <= 'z'; c++) Add(c, char.ToUpperInvariant(c).ToString());
        Add(127, "DEL");
        Add(1073741881, "CAPS");
        for (var i = 1; i <= 12; i++) Add(1073741881 + i, "F" + i);
        Add(1073741894, "Print");
        Add(1073741895, "Lock");
        Add(1073741896, "Pause");
        Add(1073741897, "Ins");
        Add(1073741898, "Home");
        Add(1073741899, "PG UP");
        Add(1073741901, "END");
        Add(1073741902, "PG DOWN");
        Add(1073741903, "Right");
        Add(1073741904, "Left");
        Add(1073741905, "Down");
        Add(1073741906, "Up");
        Add(1073741908, "/");
        Add(1073741909, "*");
        Add(1073741910, "-");
        Add(1073741911, "+");
        Add(1073741912, "Enter");
        Add(1073741913, "NUM 1");
        Add(1073741914, "NUM 2");
        Add(1073741915, "NUM 3");
        Add(1073741916, "NUM 4");
        Add(1073741917, "NUM 5");
        Add(1073741918, "NUM 6");
        Add(1073741919, "NUM 7");
        Add(1073741920, "NUM 8");
        Add(1073741921, "NUM 9");
        Add(1073741922, "NUM 0");
        Add(1073741923, ".");
        Add(1073741927, "=");
    }

    public HotkeyBox() {
        CanMove = false;
        AcceptMouseInput = true;
        AcceptKeyboardInput = true;
        Width = 210;
        Height = 25;
        Key = 0;
        MouseButton = MouseButtonType.None;
        WheelScroll = false;
        WheelUp = false;
        Mod = 0;
        Add(new ResizePic(0x0BB8) { Width = FieldW, Height = Height });
        _label = new Label("", true, 0x0021, 1) { Y = 5 };
        Add(_label);
        _buttonOK = new Button((int)BoxButton.Ok, 0x0481, 0x0483, 0x0482) {
            X = 152,
            ButtonAction = ButtonAction.Activate
        };
        Add(_buttonOK);
        _buttonCancel = new Button((int)BoxButton.Cancel, 0x047E, 0x0480, 0x047F) {
            X = 182,
            ButtonAction = ButtonAction.Activate
        };
        Add(_buttonCancel);
        WantUpdateSize = false;
        IsActive = false;
    }

    public int Key { get; private set; }
    public MouseButtonType MouseButton { get; private set; }
    public bool WheelScroll { get; private set; }
    public bool WheelUp { get; private set; }
    public int Mod { get; private set; }

    public bool IsActive {
        get => _active;
        set {
            _active = value;
            _buttonOK.IsVisible = _buttonCancel.IsVisible = value;
            _buttonOK.IsEnabled = _buttonCancel.IsEnabled = value;
        }
    }

    public event EventHandler HotkeyChanged;
    public event EventHandler HotkeyCancelled;

    public override void OnMouseClick(int x, int y) {
        IsActive = true;
        SetKeyboardFocus();
    }

    public void OnKeyDown(int key, int mod) {
        if (IsActive) SetKey(key, mod);
    }

    public void SetKey(int key, int mod) {
        if (key == 0 && mod == 0) {
            ResetBinding();
            Key = key;
            Mod = mod;
            return;
        }
        if (key == 0) return;
        var name = KeyName(key, mod);
        if (name.Length == 0) return;
        ResetBinding();
        Key = key;
        Mod = mod;
        SetLabel(name);
    }

    public void OnMouseDown(int x, int y, MouseButtonType button) {
        if (button == MouseButtonType.Middle || button == MouseButtonType.XButton1 || button == MouseButtonType.XButton2)
            SetMouseButton(button, HeldMod());
    }

    public void SetMouseButton(MouseButtonType button, int mod) {
        if (button == MouseButtonType.None) return;
        var name = KeyNamePrefix(mod) + button;
        if (name.Length == 0) return;
        ResetBinding();
        MouseButton = button;
        Mod = mod;
        SetLabel(name);
    }

    public void OnMouseWheel(bool up) => SetMouseWheel(up, HeldMod());

    public void SetMouseWheel(bool wheelUp, int mod) {
        var name = KeyNamePrefix(mod) + (wheelUp ? "WheelUp" : "WheelDown");
        if (name.Length == 0) return;
        ResetBinding();
        WheelScroll = true;
        WheelUp = wheelUp;
        Mod = mod;
        SetLabel(name);
    }

    public override void OnButtonClick(int buttonID) {
        switch ((BoxButton)buttonID) {
            case BoxButton.Ok:
                HotkeyChanged?.Invoke(this, EventArgs.Empty);
                break;
            case BoxButton.Cancel:
                SetLabel("");
                HotkeyCancelled?.Invoke(this, EventArgs.Empty);
                Key = 0;
                Mod = 0;
                break;
        }
        IsActive = false;
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        return base.Draw(x, y);
    }

    void ResetBinding() {
        Key = 0;
        MouseButton = MouseButtonType.None;
        WheelScroll = false;
        Mod = 0;
        SetLabel("");
    }

    void SetLabel(string text) {
        _label.Text = text ?? "";
        _label.X = Math.Max(0, (FieldW - _label.Width) / 2);
    }

    static int HeldMod() => GameController.Shift ? ModShift : 0;

    static string KeyName(int key, int mod) {
        if (!Names.TryGetValue(key, out var name))
            name = key >= 0 && key < 256 ? ((char)key).ToString() : key.ToString();
        return KeyNamePrefix(mod) + name;
    }

    static string KeyNamePrefix(int mod) {
        var s = "";
        if ((mod & ModShift) != 0) s += "Shift ";
        if ((mod & ModCtrl) != 0) s += "Ctrl ";
        if ((mod & ModAlt) != 0) s += "Alt ";
        return s;
    }

    static void Add(int key, string name) => Names[key] = name;

    enum BoxButton { Ok, Cancel }
}
