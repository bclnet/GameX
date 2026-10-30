// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Clients.UO;
using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO WorldMapGump. This client has no facet texture, so the window is
/// MiniMapGump's radarcol land and statics on a 400 by 400 field.
/// Zoom is ClassicUO's ten steps. Index 4 is 1, which is the minimap pixel step
/// (sx - sy, sx + sy). The center is the player tile. A drag pans until the button
/// comes up, then the center returns to the player. Wheel over the gump changes the step.
/// Markers, pins, zones, and Profile are not read.
/// The sink key is 0x56100000, beside MiniMapGump's 0x55000000. DrawRaw keeps the first upload.
/// </summary>
public sealed class WorldMapGump : Gump {
    const int CacheKey = 0x56100000;
    const int PlayerDot = CacheKey | 0x00FFFFFE;
    const int MobileDot = CacheKey | 0x00FFFFFF;
    const int Size = 400;
    const int Margin = 4;
    const int Dot = 4;

    static readonly float[] Zooms = [0.125f, 0.25f, 0.5f, 0.75f, 1f, 1.5f, 2f, 4f, 6f, 8f];
    static readonly byte[] WhiteDot = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF];
    static readonly byte[] RedDot = [0x00, 0xFC, 0x00, 0xFC, 0x00, 0xFC, 0x00, 0xFC];

    readonly List<UOMobileState> _mobiles = [];
    readonly List<UOItemState> _items = [];
    readonly List<(ushort graphic, sbyte z)> _statics = [];
    readonly Dictionary<long, int> _slots = [];
    readonly Label _coords;
    readonly Label _mouse;
    byte[] _pixels;
    int _centerX, _centerY, _lastX, _lastY, _playerX, _playerY, _playerZ, _lastPaintX, _lastPaintY;
    int _facet = -1, _zoomIndex = 4, _zoomPainted = -1, _wheel, _slotCount;
    bool _hasPlayer, _centered, _scrolling, _hover, _painted;

    public WorldMapGump(World world) : base(world, 0, 0) {
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        WantUpdateSize = false;
        Width = Size;
        Height = Size;
        X = 100;
        Y = 100;
        _coords = new Label("", false, 0, 1) { X = 6, Y = 6 };
        Add(_coords);
        _mouse = new Label("", false, 0, 1) { X = 6, Y = Size - 22, IsVisible = false };
        Add(_mouse);
    }

    /// <summary>One gump. A second call disposes it and does not add another. World may be null.</summary>
    public static void Toggle(World world) {
        var open = UIManager.GetGump<WorldMapGump>();
        if (open != null) {
            open.Dispose();
            return;
        }
        UIManager.Add(new WorldMapGump(world));
    }

    public override void Update() {
        base.Update();
        SyncCenter();
        Wheel();
        Coords();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed) return false;
        var sink = GumpArt.Sink;
        if (sink != null && Build() && _pixels != null) {
            sink.DrawRaw(CacheKey | Slot(), x, y, Width, Height, _pixels, Width, Height);
            DrawDots(sink, x, y);
        }
        return base.Draw(x, y);
    }

    bool Build() {
        SyncCenter();
        var assets = UltimaOnline.Current?.Assets;
        if (assets == null || !_hasPlayer) return false;
        if (_pixels == null) _pixels = new byte[Width * Height * 2];
        var facet = UOLoginNet.MapIndex;
        if (_painted && _centerX == _lastPaintX && _centerY == _lastPaintY && _facet == facet && _zoomPainted == _zoomIndex)
            return true;
        _lastPaintX = _centerX;
        _lastPaintY = _centerY;
        _facet = facet;
        _zoomPainted = _zoomIndex;
        _painted = true;
        Fill();
        Paint(assets);
        return true;
    }

    void SyncCenter() {
        if (!Player(out var px, out var py, out var pz)) {
            _hasPlayer = false;
            return;
        }
        _hasPlayer = true;
        _playerX = px;
        _playerY = py;
        _playerZ = pz;
        var drag = Drag();
        if (drag == 0) {
            _scrolling = false;
            _centerX = px;
            _centerY = py;
            _centered = true;
            CanMove = true;
            return;
        }
        if (!_centered) {
            _centerX = px;
            _centerY = py;
            _centered = true;
        }
        if (!_scrolling) {
            _scrolling = true;
            _lastX = _centerX;
            _lastY = _centerY;
            CanMove = false;
        }
        var off = drag == 1 ? Mouse.LDragOffset : Mouse.MDragOffset;
        var zoom = Zooms[_zoomIndex];
        var u = off.X / zoom;
        var v = off.Y / zoom;
        _centerX = _lastX - (int)Math.Round((u + v) / 2.0);
        _centerY = _lastY - (int)Math.Round((v - u) / 2.0);
        if (_centerX < 0) _centerX = 0;
        if (_centerY < 0) _centerY = 0;
    }

    bool Player(out int x, out int y, out int z) {
        x = y = z = 0;
        var serial = UOLoginNet.PlayerSerial;
        if (serial == 0) return false;
        UOLoginNet.CopyActors(_mobiles, _items);
        for (var i = 0; i < _mobiles.Count; i++) {
            if (_mobiles[i].Serial != serial) continue;
            x = _mobiles[i].X;
            y = _mobiles[i].Y;
            z = _mobiles[i].Z;
            return true;
        }
        return false;
    }

    int Drag() {
        if (Mouse.LButtonPressed && UIManager.LastControlMouseDown(1) == this) return 1;
        if (Mouse.MButtonPressed && UIManager.LastControlMouseDown(2) == this) return 2;
        return 0;
    }

    void Wheel() {
        if (!Hover()) {
            _hover = false;
            return;
        }
        var w = Mouse.Current.Wheel;
        if (!_hover) {
            _hover = true;
            _wheel = w;
            return;
        }
        while (w >= _wheel + 120) {
            _wheel += 120;
            if (_zoomIndex < Zooms.Length - 1) _zoomIndex++;
        }
        while (w <= _wheel - 120) {
            _wheel -= 120;
            if (_zoomIndex > 0) _zoomIndex--;
        }
    }

    bool Hover() {
        for (var c = UIManager.MouseOverControl; c != null; c = c.Parent)
            if (c == this) return true;
        return false;
    }

    void Coords() {
        if (!_hasPlayer) {
            if (_coords.Text.Length != 0) _coords.Text = "";
            _mouse.IsVisible = false;
            return;
        }
        var text = $"{_playerX}, {_playerY} ({_playerZ}) [{_zoomIndex}]";
        if (_coords.Text != text) _coords.Text = text;
        if (!Hover()) {
            _mouse.IsVisible = false;
            return;
        }
        ScreenToWorld(UIManager.MouseX - X, UIManager.MouseY - Y, out var wx, out var wy);
        var mouse = $"{wx} {wy}";
        if (_mouse.Text != mouse) _mouse.Text = mouse;
        _mouse.Y = Height - 6 - _mouse.Height;
        _mouse.IsVisible = true;
    }

    void ScreenToWorld(int lx, int ly, out int wx, out int wy) {
        var zoom = Zooms[_zoomIndex];
        var u = (lx - (Width >> 1)) / zoom;
        var v = (ly - (Height >> 1)) / zoom;
        wx = _centerX + (int)Math.Round((u + v) / 2.0);
        wy = _centerY + (int)Math.Round((v - u) / 2.0);
    }

    void Fill() {
        var pixels = _pixels;
        for (var i = 0; i < pixels.Length; i += 2) {
            pixels[i] = 0;
            pixels[i + 1] = 0x80;
        }
    }

    void Paint(Assets assets) {
        var zoom = Zooms[_zoomIndex];
        var step = zoom < 1f ? Math.Max(1, (int)Math.Round(1.0 / zoom)) : 1;
        var footW = zoom <= 1f ? 1 : (int)Math.Ceiling(zoom);
        var footH = zoom < 1f ? 1 : zoom <= 1f ? 2 : footW;
        var lim = (int)Math.Ceiling((Width * 0.5) / zoom) + step;
        var x0 = _centerX - lim;
        var y0 = _centerY - lim;
        var x1 = _centerX + lim;
        var y1 = _centerY + lim;
        if (x0 < 0) x0 = 0;
        if (y0 < 0) y0 = 0;
        if (step > 1) {
            x0 -= x0 % step;
            y0 -= y0 % step;
        }
        var originX = Width >> 1;
        var originY = Height >> 1;
        for (var wy = y0; wy <= y1; wy += step) {
            var ry = wy - _centerY;
            for (var wx = x0; wx <= x1; wx += step) {
                var rx = wx - _centerX;
                var isoX = rx - ry;
                var isoY = rx + ry;
                if (isoX < -lim || isoX > lim || isoY < -lim || isoY > lim) continue;
                if (!assets.TryLandTile(wx, wy, out var tile, out var landZ)) continue;
                var index = (int)tile;
                sbyte topZ = sbyte.MinValue;
                ushort top = 0;
                _statics.Clear();
                assets.CopyStatics(wx, wy, _statics);
                for (var s = 0; s < _statics.Count; s++) {
                    var graphic = _statics[s].graphic;
                    var z = _statics[s].z;
                    if (graphic == 0 || graphic == 0xFFFF || !Drawn(graphic)) continue;
                    if (topZ < z) {
                        topZ = z;
                        top = graphic;
                    }
                }
                if (top != 0 && topZ >= landZ) index = top + 0x4000;
                if (!assets.TryRadarColor(index, out var color)) continue;
                Stamp(originX + (int)(isoX * zoom), originY + (int)(isoY * zoom), footW, footH, color);
            }
        }
    }

    void Stamp(int gx, int gy, int fw, int fh, ushort color) {
        var x1 = gx + fw;
        var y1 = gy + fh;
        if (gx < 0) gx = 0;
        if (gy < 0) gy = 0;
        if (x1 > Width) x1 = Width;
        if (y1 > Height) y1 = Height;
        if (gx >= x1 || gy >= y1) return;
        var lo = (byte)color;
        var hi = (byte)(color >> 8);
        var pixels = _pixels;
        var w = Width;
        for (var y = gy; y < y1; y++) {
            var i = (y * w + gx) * 2;
            for (var x = gx; x < x1; x++) {
                pixels[i++] = lo;
                pixels[i++] = hi;
            }
        }
    }

    void DrawDots(IGumpArt sink, int x, int y) {
        var serial = UOLoginNet.PlayerSerial;
        for (var i = 0; i < _mobiles.Count; i++) {
            var m = _mobiles[i];
            if (m.Serial == serial) continue;
            Stamp(sink, MobileDot, RedDot, m.X, m.Y, x, y);
        }
        Stamp(sink, PlayerDot, WhiteDot, _playerX, _playerY, x, y);
    }

    void Stamp(IGumpArt sink, int key, byte[] px, int wx, int wy, int ox, int oy) {
        var zoom = Zooms[_zoomIndex];
        var rx = wx - _centerX;
        var ry = wy - _centerY;
        var sx = ox + (Width >> 1) + (int)((rx - ry) * zoom) - (Dot >> 1);
        var sy = oy + (Height >> 1) + (int)((rx + ry) * zoom) - (Dot >> 1);
        var minX = ox + Margin;
        var minY = oy + Margin;
        var maxX = ox + Width - Margin - Dot;
        var maxY = oy + Height - Margin - Dot;
        if (sx < minX) sx = minX;
        if (sy < minY) sy = minY;
        if (sx > maxX) sx = maxX;
        if (sy > maxY) sy = maxY;
        sink.DrawRaw(key, sx, sy, Dot, Dot, px, 2, 2);
    }

    int Slot() {
        var id = ((long)_centerX << 32) | (uint)_centerY | ((long)_facet << 48) | ((long)_zoomIndex << 56);
        if (!_slots.TryGetValue(id, out var n)) {
            n = _slotCount++;
            if ((uint)n >= 0x000FFFFEu) n = 0x000FFFFD;
            _slots[id] = n;
        }
        return n;
    }

    /// <summary>Same graphic skip as MiniMapGump.Drawn.</summary>
    static bool Drawn(ushort g) {
        switch (g) {
            case 0x0001:
            case 0x21BC:
            case 0x63D3:
            case 0xA1FE:
            case 0xA1FF:
            case 0xA200:
            case 0xA201:
                return false;
        }
        return g < 0x2198 || g > 0x21A4;
    }
}
