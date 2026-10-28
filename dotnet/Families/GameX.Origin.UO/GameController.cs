// SPDX-License-Identifier: BSD-2-Clause

using OpenX.Client;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO GameController mouse and text path. Not a second Game. MgClientHost stays the host.
/// LoginScene.Update calls this before UIManager.Update. No plugin calls.
/// </summary>
public static class GameController {
    static int _x, _y, _scroll;
    static bool _seen, _scrollReady, _ignoreNextText;

    public static void Update(SceneBase scene) {
        if (scene == null || !scene.IsLoaded || scene.IsDestroyed) return;
        Mouse.Update();
        if (!Mouse.HasSample) return;

        int x = Mouse.Position.X, y = Mouse.Position.Y;
        scene.OnMouseMove(x, y);
        // Motion. IsDragging is still the buttons from the previous poll.
        if (_seen && (x != _x || y != _y) && Mouse.IsDragging)
            if (!scene.OnMouseDragging()) UIManager.OnMouseDragging();
        _x = x;
        _y = y;
        _seen = true;

        var s = Mouse.Current;
        Button(scene, MouseButtonType.Left, s.Left);
        Button(scene, MouseButtonType.Middle, s.Middle);
        Button(scene, MouseButtonType.Right, s.Right);
        Button(scene, MouseButtonType.XButton1, s.X1);
        Button(scene, MouseButtonType.XButton2, s.X2);
        Wheel(scene, s.Wheel);
    }

    /// <summary>
    /// ClassicUO KEY_DOWN with no plugin. A handled key suppresses the text event that follows it.
    /// </summary>
    public static void KeyDown(SceneBase scene, int key, int mod) {
        if (scene == null || !scene.IsLoaded || scene.IsDestroyed) { _ignoreNextText = false; return; }
        _ignoreNextText = scene.OnKeyDown(key, mod);
    }

    public static void KeyUp(SceneBase scene, int key, int mod) {
        if (scene == null || !scene.IsLoaded || scene.IsDestroyed) return;
        scene.OnKeyUp(key, mod);
    }

    /// <summary>ClassicUO TEXT_INPUT. The focused box reads it through the scene.</summary>
    public static void TextInput(SceneBase scene, char c) {
        if (_ignoreNextText) { _ignoreNextText = false; return; }
        if (c == '\0' || scene == null || !scene.IsLoaded || scene.IsDestroyed) return;
        scene.OnTextInput(c.ToString());
    }

    static void Button(SceneBase scene, MouseButtonType button, bool pressed) {
        int index = (int)button;
        if ((uint)index >= 6 || pressed == _held[index]) return;
        _held[index] = pressed;
        if (pressed) ButtonDown(scene, button);
        else ButtonUp(scene, button);
    }

    static readonly bool[] _held = new bool[6];

    static void ButtonDown(SceneBase scene, MouseButtonType button) {
        uint lastClickTime = LastClick(button);
        Mouse.ButtonPress(button);
        Mouse.Update();
        int x = Mouse.Position.X, y = Mouse.Position.Y;
        uint ticks = GlobalTime.Ticks;
        if (lastClickTime + Mouse.MOUSE_DELAY_DOUBLE_CLICK >= ticks) {
            lastClickTime = 0;
            bool res = scene.OnMouseDoubleClick((int)button, x, y) || UIManager.OnMouseDoubleClick((int)button, x, y);
            if (!res) {
                if (!scene.OnMouseDown((int)button, x, y))
                    UIManager.OnMouseDown((int)button, x, y);
            }
            else lastClickTime = 0xFFFF_FFFF;
        }
        else {
            if (!scene.OnMouseDown((int)button, x, y))
                UIManager.OnMouseDown((int)button, x, y);
            lastClickTime = Mouse.CancelDoubleClick ? 0 : ticks;
        }
        SetLastClick(button, lastClickTime);
    }

    static void ButtonUp(SceneBase scene, MouseButtonType button) {
        int x = Mouse.Position.X, y = Mouse.Position.Y;
        if (LastClick(button) != 0xFFFF_FFFF) {
            if (!scene.OnMouseUp((int)button, x, y) || UIManager.LastControlMouseDown((int)button) != null)
                UIManager.OnMouseUp((int)button, x, y);
        }
        Mouse.ButtonRelease(button);
        Mouse.Update();
    }

    static void Wheel(SceneBase scene, int wheel) {
        if (!_scrollReady) { _scroll = wheel; _scrollReady = true; return; }
        while (wheel >= _scroll + 120) { _scroll += 120; OneWheel(scene, true); }
        while (wheel <= _scroll - 120) { _scroll -= 120; OneWheel(scene, false); }
    }

    static void OneWheel(SceneBase scene, bool up) {
        Mouse.Update();
        if (!scene.OnMouseWheel(up)) UIManager.OnMouseWheel(up);
    }

    static uint LastClick(MouseButtonType button) => button switch {
        MouseButtonType.Left => Mouse.LastLeftButtonClickTime,
        MouseButtonType.Middle => Mouse.LastMidButtonClickTime,
        MouseButtonType.Right => Mouse.LastRightButtonClickTime,
        _ => 0,
    };

    static void SetLastClick(MouseButtonType button, uint ticks) {
        switch (button) {
            case MouseButtonType.Left: Mouse.LastLeftButtonClickTime = ticks; break;
            case MouseButtonType.Middle: Mouse.LastMidButtonClickTime = ticks; break;
            case MouseButtonType.Right: Mouse.LastRightButtonClickTime = ticks; break;
        }
    }
}
