// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO UserMarkersGump. The fields are X, Y, name, color, and icon.
/// Create and Edit change the list in memory. Nothing is appended to a file.
/// Icon names are not loaded from MapIcons. There is no texture.
/// </summary>
public sealed class UserMarkersGump : Gump {
    public const int MaxCoord = 10;
    public const int MaxName = 25;

    public static readonly string[] Colors = { "none", "red", "green", "blue", "purple", "black", "yellow", "white" };
    public static readonly string[] Icons = { "exit", "home", "bank", "dungeon", "moongate", "shop", "shrine", "player" };

    const int W = 320;
    const int H = 220;
    const ushort Hue = 0x0481;
    const int AddId = 1;
    const int CancelId = 2;
    const int ColorId = 3;
    const int IconId = 4;

    readonly TextBox _textX;
    readonly TextBox _textY;
    readonly TextBox _markerName;
    readonly Label _colorLabel;
    readonly Label _iconLabel;
    readonly List<WMapMarker> _markers;
    readonly bool _edit;
    readonly int _idx;
    int _mapMaxX = 7168;
    int _mapMaxY = 4096;
    int _colorIndex;
    int _iconIndex;

    public event EventHandler EditEnd;

    public UserMarkersGump(World world, int x, int y, List<WMapMarker> markers, string color = "none", string icon = "exit", bool isEdit = false, int markerIdx = -1)
        : base(world, 0, 0) {
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        WantUpdateSize = false;
        Width = W;
        Height = H;
        _markers = markers ?? new List<WMapMarker>();
        _edit = isEdit;
        _idx = markerIdx;
        MapSize(UOLoginNet.MapIndex, out _mapMaxX, out _mapMaxY);
        _colorIndex = IndexOf(Colors, color);
        if (!isEdit) _colorIndex = Icons.Length == 0 ? 1 : 0;
        if (_colorIndex < 0) _colorIndex = 0;
        _iconIndex = IndexOf(Icons, icon);
        if (_iconIndex < 0) _iconIndex = 0;
        var markerName = _idx < 0 || _idx >= _markers.Count ? "Marker" : _markers[_idx].Name ?? "Marker";
        WorldGump.ViewCenter(out var cx, out var cy);
        X = cx - W / 2;
        Y = cy - H / 2;
        Clamp();
        Add(new ResizePic(0x0A28) { X = 0, Y = 0, Width = W, Height = H });
        Add(new Label(isEdit ? "Edit Marker" : "Add Marker", true, Hue, 1) { X = 100, Y = 8 });
        var fy = 32;
        Add(new Label("X", true, Hue, 1) { X = 8, Y = fy });
        Add(new ResizePic(0x0BB8) { X = 48, Y = fy, Width = 90, Height = 22 });
        _textX = new TextBox { X = 52, Y = fy + 2, Width = 82, Height = 18, Hue = Hue, MaxLength = MaxCoord, Text = x.ToString() };
        Add(_textX);
        fy += 30;
        Add(new Label("Y", true, Hue, 1) { X = 8, Y = fy });
        Add(new ResizePic(0x0BB8) { X = 48, Y = fy, Width = 90, Height = 22 });
        _textY = new TextBox { X = 52, Y = fy + 2, Width = 82, Height = 18, Hue = Hue, MaxLength = MaxCoord, Text = y.ToString() };
        Add(_textY);
        fy += 30;
        Add(new Label("Name", true, Hue, 1) { X = 8, Y = fy });
        Add(new ResizePic(0x0BB8) { X = 48, Y = fy, Width = 250, Height = 22 });
        _markerName = new TextBox { X = 52, Y = fy + 2, Width = 242, Height = 18, Hue = Hue, MaxLength = MaxName, Text = markerName };
        Add(_markerName);
        fy += 30;
        Add(new Label("Color", true, Hue, 1) { X = 8, Y = fy });
        Add(Cycle(ColorId, 48, fy));
        _colorLabel = new Label(Colors[_colorIndex], true, HueOf(Colors[_colorIndex]), 1) { X = 80, Y = fy };
        Add(_colorLabel);
        fy += 30;
        Add(new Label("Icon", true, Hue, 1) { X = 8, Y = fy });
        Add(Cycle(IconId, 48, fy));
        _iconLabel = new Label(Icons.Length == 0 ? "" : Icons[_iconIndex], true, Hue, 1) { X = 80, Y = fy };
        Add(_iconLabel);
        Add(Act(isEdit ? "Edit" : "Create", AddId, 16, H - 32));
        Add(Act("Cancel", CancelId, 100, H - 32));
    }

    public override void OnButtonClick(int buttonID) {
        switch (buttonID) {
            case AddId:
                if (_edit) EditMarker();
                else AddNewMarker();
                break;
            case CancelId:
                Dispose();
                break;
            case ColorId:
                if (Colors.Length == 0) return;
                _colorIndex = (_colorIndex + 1) % Colors.Length;
                _colorLabel.Text = Colors[_colorIndex];
                _colorLabel.Hue = HueOf(Colors[_colorIndex]);
                break;
            case IconId:
                if (Icons.Length == 0) return;
                _iconIndex = (_iconIndex + 1) % Icons.Length;
                _iconLabel.Text = Icons[_iconIndex];
                break;
        }
    }

    public override void OnKeyboardReturn(int textID, string text) {
        if (_edit) EditMarker();
        else AddNewMarker();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        return base.Draw(x, y);
    }

    void AddNewMarker() {
        var marker = Prepare();
        if (marker == null) return;
        // Profile path is not ported.
        MarkersManagerGump.Save();
        _markers.Add(marker);
        Dispose();
    }

    void EditMarker() {
        if (_markers == null || _idx < 0 || _idx >= _markers.Count) return;
        var marker = Prepare();
        if (marker == null) return;
        _markers[_idx] = marker;
        EditEnd?.Invoke(marker, EventArgs.Empty);
        Dispose();
    }

    WMapMarker Prepare() {
        var xs = _textX != null && !_textX.IsDisposed ? _textX.Text : "";
        var ys = _textY != null && !_textY.IsDisposed ? _textY.Text : "";
        if (!int.TryParse(xs, out var x)) return null;
        if (!int.TryParse(ys, out var y)) return null;
        if (x > _mapMaxX || x < 0) return null;
        if (y > _mapMaxY || y < 0) return null;
        var name = _markerName != null && !_markerName.IsDisposed ? _markerName.Text : "";
        if (string.IsNullOrEmpty(name)) return null;
        if (name.Contains(",")) name = name.Replace(",", "");
        if (name.Length > MaxName) name = name.Substring(0, MaxName);
        var color = Colors.Length == 0 ? "none" : Colors[_colorIndex];
        var icon = Icons.Length == 0 ? "" : Icons[_iconIndex];
        return new WMapMarker {
            Name = name,
            X = x,
            Y = y,
            MapId = UOLoginNet.MapIndex,
            ColorName = color,
            Hue = HueOf(color),
            MarkerIconName = icon,
            ZoomIndex = 4
        };
    }

    void Clamp() {
        var s = Mouse.Current;
        var w = s.BufW > 0 ? s.BufW : 640;
        var h = s.BufH > 0 ? s.BufH : 480;
        if (X < 0) X = 0;
        if (Y < 0) Y = 0;
        if (X + Width > w) X = Math.Max(0, w - Width);
        if (Y + Height > h) Y = Math.Max(0, h - Height);
    }

    static Control Cycle(int id, int x, int y) {
        var b = new Button(id, 0x0FA8, 0x0FAA, 0x0FA9) { X = x, Y = y, ButtonAction = ButtonAction.Activate };
        if (b.IsDisposed) return new MarkerAct(">", id) { X = x, Y = y };
        if (b.Width < 8) b.Width = 16;
        if (b.Height < 8) b.Height = 16;
        return b;
    }

    static Control Act(string text, int id, int x, int y) {
        var hit = new MarkerAct(text, id) { X = x, Y = y };
        return hit;
    }

    /// <summary>ClassicUO MapsDefaultSize in tiles. Felucca and Trammel are 7168 by 4096.</summary>
    public static void MapSize(int map, out int w, out int h) {
        switch (map) {
            case 2: w = 2304; h = 1600; break;
            case 3: w = 2560; h = 2048; break;
            case 4: w = 1448; h = 1448; break;
            case 5: w = 1280; h = 4096; break;
            default: w = 7168; h = 4096; break;
        }
    }

    public static ushort HueOf(string name) {
        switch (name) {
            case "red": return 0x0023;
            case "green": return 0x0044;
            case "blue": return 0x0058;
            case "purple": return 0x000D;
            case "black": return 0x0001;
            case "yellow": return 0x0035;
            case "white": return 0x0481;
            default: return 0x0481;
        }
    }

    static int IndexOf(string[] list, string value) {
        if (list == null || value == null) return -1;
        for (var i = 0; i < list.Length; i++)
            if (string.Equals(list[i], value, StringComparison.OrdinalIgnoreCase)) return i;
        return -1;
    }

    sealed class MarkerAct : Label {
        readonly int _id;

        public MarkerAct(string text, int id) : base(text, true, 0xFFFF, 1) {
            _id = id;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) => Parent?.OnButtonClick(_id);
    }
}
