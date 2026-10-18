// SPDX-License-Identifier: BSD-2-Clause

using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO UIManager for the login gumps. Not a second host.
/// Mouse and keyboard from MgClientHost are not forwarded here.
/// </summary>
public static class UIManager {
    static Control _keyboardFocus;

    public static LinkedList<Gump> Gumps { get; } = new();

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

    public static void Update() {
        if (_keyboardFocus != null && _keyboardFocus.IsDisposed) _keyboardFocus = null;
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
}
