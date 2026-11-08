// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Text.RegularExpressions;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO LocationGoGump. A tile is "X, Y", "X Y", or "X:Y". A sextant is the ServUO form
/// ClassicUO parses, such as 100o25'S,40o04'E. The parse path asks ReverseLookup at 0,0, which
/// is the Britannia center 1323,1624 on a 5120 by 4096 field. Lost Lands is not that path.
/// Hue 0x40 is a point. Hue 0x33 is not. OK and Enter call the action only for a point.
/// The 0.7 fill is an opaque dark pixel. No profile file.
/// </summary>
public sealed class LocationGoGump : Gump {
    const int FillKey = 0x57500001;
    const int XCenter = 1323;
    const int YCenter = 1624;
    const int XWidth = 5120;
    const int YHeight = 4096;

    // r = g = b = 4, opaque. Stand-in for AlphaBlendControl(0.7).
    static readonly byte[] Dark = { 0x84, 0x90 };
    static readonly Regex PointCoords = new Regex(@"^(?<X>\d+)\s*[,:\s]\s*(?<Y>\d+)$", RegexOptions.CultureInvariant);
    static readonly Regex SextantCoords = new Regex(@"(?<LatDegrees>\d{1,3})[°o\s]*(?<LatMinutes>\d{2})'(?<LatDirection>[NS])[\s,]*(?<LongDegrees>\d{1,3})[°o\s]*(?<LongMinutes>\d{2})'(?<LongDirection>[EW])", RegexOptions.CultureInvariant);

    readonly Action<int, int> _goTo;
    readonly TextBox _box;
    string _seen;
    int _x = -1;
    int _y = -1;
    bool _valid;

    public LocationGoGump(World world, Action<int, int> goTo) : base(world, 0, 0) {
        _goTo = goTo;
        CanMove = true;
        CanCloseWithRightClick = true;
        CanCloseWithEsc = true;
        AcceptMouseInput = true;
        LayerOrder = UILayer.Over;
        WantUpdateSize = false;
        Width = 250;
        Height = 150;
        var caption = new Label("Enter Location", true, 0xFFFF, 1) { X = 12, Y = 12 };
        Add(caption);
        var boxW = Width - 94;
        if (boxW < 40) boxW = 40;
        var boxY = 20 + caption.Height + 5;
        Add(new ResizePic(0x0BB8) { X = 20, Y = boxY, Width = boxW + 10, Height = 25 });
        _box = new TextBox {
            X = 26,
            Y = boxY + 2,
            Width = boxW,
            Height = 25,
            Hue = 0,
            MaxLength = -1,
        };
        Add(_box);
        var ok = new Button(0, 0x0481, 0x0482, 0x0483) { ButtonAction = ButtonAction.Activate };
        if (!ok.IsDisposed) {
            ok.X = _box.X + _box.Width + 12;
            ok.Y = _box.Y;
            Add(ok);
        }
        Add(new ExampleLines { X = _box.X - 6, Y = _box.Y + 28 });
        Place();
        _box.SetKeyboardFocus();
    }

    public override void Update() {
        base.Update();
        if (_box == null || _box.IsDisposed) return;
        Apply(_box.Text);
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (Width > 0 && Height > 0)
            GumpArt.Sink?.DrawRaw(FillKey, x, y, Width, Height, Dark, 1, 1);
        return base.Draw(x, y);
    }

    public override void OnButtonClick(int buttonID) {
        if (buttonID == 0 && Go()) Dispose();
    }

    public override void OnKeyboardReturn(int textID, string text) {
        if (Go()) Dispose();
    }

    bool Go() {
        Apply(_box == null || _box.IsDisposed ? "" : _box.Text);
        if (!_valid || _goTo == null) return false;
        _goTo(_x, _y);
        return true;
    }

    void Apply(string text) {
        if (text == _seen) return;
        _seen = text;
        if (string.IsNullOrWhiteSpace(text)) return;
        _valid = TrySextant(text, out _x, out _y) || TryPoint(text, out _x, out _y);
        if (!_valid) _x = _y = -1;
        if (_box != null && !_box.IsDisposed) _box.Hue = (ushort)(_valid ? 0x40 : 0x33);
    }

    static bool TryPoint(string text, out int x, out int y) {
        x = y = -1;
        var match = PointCoords.Match(text ?? "");
        if (!match.Success) return false;
        if (!int.TryParse(match.Groups["X"].Value, out x)) return false;
        if (!int.TryParse(match.Groups["Y"].Value, out y)) return false;
        return true;
    }

    static bool TrySextant(string text, out int x, out int y) {
        x = y = -1;
        var match = SextantCoords.Match((text ?? "").Trim());
        if (!match.Success) return false;
        if (!int.TryParse(match.Groups["LatDegrees"].Value, out var latD)) return false;
        if (!int.TryParse(match.Groups["LatMinutes"].Value, out var latM)) return false;
        if (!int.TryParse(match.Groups["LongDegrees"].Value, out var longD)) return false;
        if (!int.TryParse(match.Groups["LongMinutes"].Value, out var longM)) return false;
        var south = match.Groups["LatDirection"].Value == "S";
        var east = match.Groups["LongDirection"].Value == "E";
        double absLong = longD + longM / 60.0;
        double absLat = latD + latM / 60.0;
        if (!east) absLong = 360.0 - absLong;
        if (!south) absLat = 360.0 - absLat;
        x = XCenter + (int)(absLong * XWidth / 360);
        y = YCenter + (int)(absLat * YHeight / 360);
        if (x < 0) x += XWidth;
        else if (x >= XWidth) x -= XWidth;
        if (y < 0) y += YHeight;
        else if (y >= YHeight) y -= YHeight;
        return true;
    }

    void Place() {
        var s = Mouse.Current;
        var bw = s.BufW > 0 ? s.BufW : 640;
        var bh = s.BufH > 0 ? s.BufH : 480;
        X = (bw - Width) >> 1;
        Y = (bh - Height) >> 1;
    }

    sealed class ExampleLines : Control {
        static readonly string[] Lines = {
            "Examples:",
            " 1639, 1532",
            " 100o25'S,40o04'E",
            " 9 14'N 91 37'W",
        };

        public ExampleLines() {
            AcceptMouseInput = false;
            Width = 180;
            Height = Lines.Length * 16;
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            for (var i = 0; i < Lines.Length; i++)
                GumpArt.Sink?.DrawText(x, y + i * 16, Lines[i], 0xFFFF, 1);
            return true;
        }
    }
}
