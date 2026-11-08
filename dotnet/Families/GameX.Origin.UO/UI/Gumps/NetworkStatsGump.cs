// SPDX-License-Identifier: BSD-2-Clause

using System;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO NetworkStatsGump. The labels are ping, bytes in, and bytes out.
/// Those counters are not public on UOLoginNet, so the numbers stay 0.
/// Double-click swaps the one-line ping and the two-line form. The minimized flag is the two-line form,
/// which is how ClassicUO named it. Hue follows the ping: 0x44, 0x34, 0x31, then 0x20.
/// The position is remembered in this process. No profile file.
/// </summary>
public sealed class NetworkStatsGump : Gump {
    const int FillKey = 0x57300001;

    // r = g = b = 4, opaque. Stand-in for AlphaBlendControl(0.7).
    static readonly byte[] Dark = { 0x84, 0x90 };
    static int _lastX = -1;
    static int _lastY = -1;

    uint _ping;
    uint _deltaBytesReceived;
    uint _deltaBytesSent;
    int _timeToUpdate;
    string _cache = "";
    bool _minimized;

    public NetworkStatsGump(World world, int x, int y) : base(world, 0, 0) {
        CanMove = true;
        CanCloseWithEsc = false;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        AcceptKeyboardInput = false;
        LayerOrder = UILayer.Over;
        WantUpdateSize = false;
        _ping = 0;
        _deltaBytesReceived = 0;
        _deltaBytesSent = 0;
        X = _lastX <= 0 ? x : _lastX;
        Y = _lastY <= 0 ? y : _lastY;
        Width = 100;
        Height = 30;
        Build();
        Remember();
        _timeToUpdate = Environment.TickCount + 100;
    }

    /// <summary>False is the ping line. True is ping plus in and out. ClassicUO's flag has those two pages reversed in the name.</summary>
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
        // UOLoginNet does not expose ping or byte counters.
        _ping = 0;
        _deltaBytesReceived = 0;
        _deltaBytesSent = 0;
        Build();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (Width > 0 && Height > 0)
            GumpArt.Sink?.DrawRaw(FillKey, x, y, Width, Height, Dark, 1, 1);
        Paint(_cache, x + 10, y + 10, HueFor(_ping));
        return base.Draw(x, y);
    }

    public override bool OnMouseDoubleClick(int x, int y) {
        IsMinimized = !IsMinimized;
        return true;
    }

    void Build() {
        if (_minimized)
            _cache = $"Ping: {_ping} ms\r\nIn: {SizeOf(_deltaBytesReceived),-6} Out: {SizeOf(_deltaBytesSent),-6}";
        else
            _cache = $"Ping: {_ping} ms";
        Fit(_cache);
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

    static void Paint(string text, int x, int y, ushort hue) {
        if (string.IsNullOrEmpty(text)) return;
        var line = 0;
        var start = 0;
        text = text.Replace("\r\n", "\n").Replace('\r', '\n');
        for (var i = 0; i <= text.Length; i++) {
            if (i != text.Length && text[i] != '\n') continue;
            var n = i - start;
            if (n > 0) GumpArt.Sink?.DrawText(x, y + line * 16, text.Substring(start, n), hue, 1);
            line++;
            start = i + 1;
        }
    }

    static int WidthOf(string text) =>
        GumpArt.Sink?.MeasureText(text, 1) ?? (string.IsNullOrEmpty(text) ? 0 : text.Length * 6);

    static ushort HueFor(uint ping) {
        if (ping < 150) return 0x44;
        if (ping < 200) return 0x34;
        if (ping < 300) return 0x31;
        return 0x20;
    }

    static string SizeOf(uint bytes) {
        decimal num = bytes;
        var arg = "B";
        if (num >= 1024m) {
            arg = "KB";
            num /= 1024m;
            if (num >= 1024m) {
                arg = "MB";
                num /= 1024m;
                if (num >= 1024m) {
                    arg = "GB";
                    num /= 1024m;
                }
            }
        }
        return $"{Math.Round(num, 2):0.##} {arg}";
    }

    void Remember() {
        _lastX = X;
        _lastY = Y;
    }
}
