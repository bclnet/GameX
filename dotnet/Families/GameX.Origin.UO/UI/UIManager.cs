// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO UIManager for the login gumps. Not a second host.
/// Mouse and keyboard from the scene are forwarded here. The host does not reference this type.
/// </summary>
public static class UIManager {
    // Index is the mouse button: 1 left, 2 middle, 3 right.
    static readonly Control[] _pressed = new Control[4];
    static Control _keyboardFocus;

    public static LinkedList<Gump> Gumps { get; } = new();
    public static int MouseX { get; private set; }
    public static int MouseY { get; private set; }
    public static Control MouseOverControl => HitTest(MouseX, MouseY);

    public static Control KeyboardFocusControl {
        get => _keyboardFocus;
        set {
            if (_keyboardFocus == value) return;
            _keyboardFocus?.OnFocusLost();
            _keyboardFocus = value;
            if (value != null && value.AcceptKeyboardInput && !value.IsFocused)
                value.OnFocusEnter();
        }
    }

    public static void Add(Gump gump, bool front = true) {
        if (gump == null || gump.IsDisposed) return;
        if (front) Gumps.AddFirst(gump);
        else Gumps.AddLast(gump);
    }

    public static T GetGump<T>() where T : Gump {
        for (var n = Gumps.First; n != null; n = n.Next)
            if (!n.Value.IsDisposed && n.Value is T t) return t;
        return null;
    }

    public static T GetGump<T>(uint serial) where T : Gump {
        for (var n = Gumps.First; n != null; n = n.Next)
            if (!n.Value.IsDisposed && n.Value is T t && t.ServerSerial == serial) return t;
        return null;
    }

    public static void Update() {
        if (_keyboardFocus != null && _keyboardFocus.IsDisposed) _keyboardFocus = null;
        for (var i = 1; i <= 3; i++)
            if (_pressed[i] != null && _pressed[i].IsDisposed) _pressed[i] = null;
        var node = Gumps.First;
        while (node != null) {
            var next = node.Next;
            var g = node.Value;
            if (g.IsDisposed) Gumps.Remove(node);
            else {
                g.Update();
                if (g.IsDisposed) Gumps.Remove(node);
            }
            node = next;
        }
    }

    /// <summary>Back to front: Under, Default, Over. Returns how many gumps drew.</summary>
    public static int Draw() {
        var n = 0;
        n += DrawLayer(UILayer.Under);
        n += DrawLayer(UILayer.Default);
        n += DrawLayer(UILayer.Over);
        return n;
    }

    public static void Clear() {
        foreach (var g in Gumps) g.Dispose();
        Gumps.Clear();
        _keyboardFocus = null;
        _pressed[1] = _pressed[2] = _pressed[3] = null;
    }

    public static void OnMouseMove(int x, int y) {
        MouseX = x;
        MouseY = y;
    }

    public static bool OnMouseDown(int button, int x, int y) {
        MouseX = x;
        MouseY = y;
        var hit = HitTest(x, y);
        if ((uint)button < 4) _pressed[button] = hit;
        if (hit != null && hit.AcceptKeyboardInput) KeyboardFocusControl = hit;
        return hit != null;
    }

    public static bool OnMouseUp(int button, int x, int y) {
        MouseX = x;
        MouseY = y;
        var hit = HitTest(x, y);
        var down = TakePressed(button);
        if (button == 3) {
            var gump = GumpOf(hit);
            // LoginBackground and LoginGump set CanCloseWithRightClick false.
            if (gump != null && !gump.IsDisposed && gump.CanCloseWithRightClick && gump == GumpOf(down)) {
                gump.CloseWithRightClick();
                if (_keyboardFocus != null && _keyboardFocus.IsDisposed) _keyboardFocus = null;
                return true;
            }
        }
        else if (button == 1 && down is UseLabel src && src != hit && src.Serial != 0 && !UOLoginNet.HasTarget && !UOLoginNet.Holding) {
            UOLoginNet.Lift(src.Serial);
            if (UOLoginNet.Holding) {
                if (DropHeldOn(hit)) return true;
                return false;
            }
        }
        else if (button == 1 && hit != null && hit == down) {
            if (hit is Button b) b.OnButtonClick(b.ButtonID);
            else if (hit is Checkbox box) box.IsChecked = !box.IsChecked;
            else hit.OnMouseClick(x, y);
        }
        return hit != null;
    }

    /// <summary>A drag releases into a bag, onto a paperdoll (the backpack), or onto another label. False leaves the hold for the ground.</summary>
    static bool DropHeldOn(Control hit) {
        for (var p = hit; p != null; p = p.Parent) {
            if (p is ContainerGump bag) {
                UOLoginNet.DropHeld(40, 40, 0, bag.ServerSerial);
                return true;
            }
            if (p is PaperdollGump) {
                var pack = UOLoginNet.PlayerBackpack;
                if (pack == 0) return false;
                UOLoginNet.DropHeld(40, 40, 0, pack);
                return true;
            }
        }
        if (hit is UseLabel dest && dest.Serial != 0) {
            UOLoginNet.DropHeld(0, 0, 0, dest.Serial);
            return true;
        }
        return false;
    }

    public static bool OnMouseDoubleClick(int button, int x, int y) {
        MouseX = x;
        MouseY = y;
        var hit = HitTest(x, y);
        if (hit == null) return false;
        if (button == 1) hit.OnMouseDoubleClick(x, y);
        return true;
    }

    public static Control LastControlMouseDown(int button) => (uint)button < 4 ? _pressed[button] : null;

    public static void OnMouseDragging() { }

    public static void OnMouseWheel(bool up) { }

    /// <summary>SDL keycode. Escape does not close the process.</summary>
    public static bool OnKeyDown(int key, int mod) {
        if (key == 27) return false;
        if (KeyboardFocusControl is not TextBox box || !box.AcceptKeyboardInput) return false;
        switch (key) {
            case 8: // backspace
                if (box.Text.Length > 0) box.Text = box.Text[..^1];
                return true;
            case 13: // return
                box.Parent?.OnKeyboardReturn(0, box.Text);
                return true;
            case 9: // tab
                FocusNextTextBox(box);
                return true;
            default:
                return false;
        }
    }

    public static bool OnKeyUp(int key, int mod) => false;

    public static void OnTextInput(string text) {
        if (string.IsNullOrEmpty(text)) return;
        if (KeyboardFocusControl is not TextBox box || !box.AcceptKeyboardInput) return;
        foreach (var ch in text) {
            if (char.IsControl(ch)) continue;
            if (box.MaxLength >= 0 && box.Text.Length >= box.MaxLength) break;
            box.Text += ch;
        }
    }

    static int DrawLayer(UILayer layer) {
        var n = 0;
        for (var node = Gumps.Last; node != null; node = node.Previous) {
            var g = node.Value;
            if (g.LayerOrder != layer || g.IsDisposed || !g.IsVisible) continue;
            if (g.Draw(g.X, g.Y)) n++;
        }
        return n;
    }

    // Front to back is Draw reversed: Over, Default, Under, each list from First.
    // Children are last to first so the top control wins.
    static Control HitTest(int x, int y) {
        var hit = HitLayer(UILayer.Over, x, y);
        if (hit != null) return hit;
        hit = HitLayer(UILayer.Default, x, y);
        if (hit != null) return hit;
        return HitLayer(UILayer.Under, x, y);
    }

    static Control HitLayer(UILayer layer, int x, int y) {
        for (var node = Gumps.First; node != null; node = node.Next) {
            var g = node.Value;
            if (g.LayerOrder != layer || g.IsDisposed || !g.IsVisible) continue;
            var hit = Hit(g, g.X, g.Y, x, y);
            if (hit != null) return hit;
        }
        return null;
    }

    static Control Hit(Control c, int sx, int sy, int x, int y) {
        if (c.IsDisposed || !c.IsVisible) return null;
        var children = c.Children;
        for (var i = children.Count - 1; i >= 0; i--) {
            var child = children[i];
            if (child.Page != 0 && child.Page != c.ActivePage) continue;
            var hit = Hit(child, sx + child.X, sy + child.Y, x, y);
            if (hit != null) return hit;
        }
        if (c.AcceptMouseInput && c.IsEnabled && x >= sx && y >= sy && x < sx + c.Width && y < sy + c.Height)
            return c;
        return null;
    }

    static Control TakePressed(int button) {
        if ((uint)button > 3) return null;
        var c = _pressed[button];
        _pressed[button] = null;
        return c;
    }

    static Gump GumpOf(Control c) {
        for (var p = c; p != null; p = p.Parent)
            if (p is Gump g) return g;
        return null;
    }

    static void FocusNextTextBox(TextBox current) {
        var gump = GumpOf(current);
        if (gump == null) return;
        var boxes = new List<TextBox>();
        CollectTextBoxes(gump, boxes);
        var n = boxes.Count;
        if (n == 0) return;
        var i = boxes.IndexOf(current);
        KeyboardFocusControl = boxes[(i + 1) % n];
    }

    static void CollectTextBoxes(Control c, List<TextBox> boxes) {
        if (c.IsDisposed || !c.IsVisible) return;
        if (c is TextBox box && box.AcceptKeyboardInput) boxes.Add(box);
        var children = c.Children;
        for (var i = 0; i < children.Count; i++) {
            var child = children[i];
            if (child.Page != 0 && child.Page != c.ActivePage) continue;
            CollectTextBoxes(child, boxes);
        }
    }
}
