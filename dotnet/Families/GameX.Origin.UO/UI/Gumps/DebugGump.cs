// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO DebugGump. The short page is FPS. The other page adds zoom, object count, client version,
/// the player, the mouse, and the control under the pointer. FPS stays 0: this client has no refresh counter.
/// Zoom stays 1.00: SceneBase has no camera. Total Objs is the actor list, not a render pass.
/// The tile under the cursor is not stored. Double-click swaps pages. The minimized flag is the long page,
/// which is how ClassicUO named it. No profile file.
/// </summary>
public sealed class DebugGump : Gump {
    const int FillKey = 0x57400001;

    // r = g = b = 4, opaque. Stand-in for AlphaBlendControl(0.7).
    static readonly byte[] Dark = { 0x84, 0x90 };
    static int _lastX = -1;
    static int _lastY = -1;

    readonly List<UOMobileState> _mobiles = [];
    readonly List<UOItemState> _items = [];
    int _timeToUpdate;
    string _cache = "";
    bool _minimized;

    public DebugGump(World world, int x, int y) : base(world, 0, 0) {
        CanMove = true;
        CanCloseWithEsc = false;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        AcceptKeyboardInput = false;
        LayerOrder = UILayer.Over;
        WantUpdateSize = false;
        Width = 100;
        Height = 50;
        X = _lastX <= 0 ? x : _lastX;
        Y = _lastY <= 0 ? y : _lastY;
        Build();
        Remember();
        _timeToUpdate = Environment.TickCount + 100;
    }

    /// <summary>False is the FPS line. True is the long page. ClassicUO's flag has those two pages reversed in the name.</summary>
    public bool IsMinimized {
        get => _minimized;
        set {
            if (_minimized == value) return;
            _minimized = value;
            Build();
        }
    }

    public override void Update() {
        base.Update();
        if (IsDisposed) return;
        Remember();
        if (_cache.Length > 0 && unchecked(Environment.TickCount - _timeToUpdate) < 0) return;
        _timeToUpdate = Environment.TickCount + 100;
        Build();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (Width > 0 && Height > 0)
            GumpArt.Sink?.DrawRaw(FillKey, x, y, Width, Height, Dark, 1, 1);
        Paint(_cache, x + 10, y + 10);
        return base.Draw(x, y);
    }

    public override bool OnMouseDoubleClick(int x, int y) {
        IsMinimized = !IsMinimized;
        return true;
    }

    void Build() {
        if (_minimized) {
            // plugin is not ported
            UOLoginNet.CopyActors(_mobiles, _items);
            var pos = UOLoginNet.PlayerSerial == 0
                ? "0xFFFF, 0xFFFF, 0"
                : $"{UOLoginNet.PlayerX}, {UOLoginNet.PlayerY}, {UOLoginNet.PlayerZ}";
            var version = UltimaOnline.Current == null ? "" : UltimaOnline.Current.VersionText ?? "";
            _cache =
                $"- FPS: 0 (Min=0, Max=0), Zoom: 1.00, Total Objs: {_mobiles.Count + _items.Count}\r\n" +
                $"- Client version: {version}\r\n" +
                $"- CharPos: {pos}\r\n" +
                $"- Mouse: {Mouse.Position.X}, {Mouse.Position.Y}\r\n" +
                "- InGamePos: 0xFFFF, 0xFFFF, 0\r\n" +
                $"- Selected: {Selected()}\r\n";
        }
        else _cache = "FPS: 0";
        Fit(_cache);
    }

    static string Selected() {
        var hit = UIManager.MouseOverControl;
        if (hit == null) return "";
        if (hit is UseLabel label)
            return $"Label (0x{label.Serial:X8}) {UOLoginNet.NameOf(label.Serial)}";
        return hit.GetType().Name;
    }

    void Fit(string text) {
        var lines = 1;
        var max = 0;
        var start = 0;
        var src = (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n');
        for (var i = 0; i <= src.Length; i++) {
            if (i != src.Length && src[i] != '\n') continue;
            var w = WidthOf(src.Substring(start, i - start));
            if (w > max) max = w;
            if (i != src.Length) lines++;
            start = i + 1;
        }
        Width = max + 20;
        Height = lines * 16 + 20;
        if (Width < 40) Width = 40;
        if (Height < 20) Height = 20;
    }

    static void Paint(string text, int x, int y) {
        if (string.IsNullOrEmpty(text)) return;
        var line = 0;
        var start = 0;
        text = text.Replace("\r\n", "\n").Replace('\r', '\n');
        for (var i = 0; i <= text.Length; i++) {
            if (i != text.Length && text[i] != '\n') continue;
            var n = i - start;
            if (n > 0) GumpArt.Sink?.DrawText(x, y + line * 16, text.Substring(start, n), 0, 1);
            line++;
            start = i + 1;
        }
    }

    static int WidthOf(string text) =>
        GumpArt.Sink?.MeasureText(text, 1) ?? (string.IsNullOrEmpty(text) ? 0 : text.Length * 6);

    void Remember() {
        _lastX = X;
        _lastY = Y;
    }
}
