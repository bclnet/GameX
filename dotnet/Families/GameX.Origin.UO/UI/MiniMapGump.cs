// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Clients.UO;
using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO MiniMapGump. 5010 is the small gump and 5011 the large one. Double-click toggles them.
/// The center is the actor CopyActors returns for UOLoginNet.PlayerSerial.
/// Zoom matches ClassicUO: the block window is Width/4 by Height/4, and each tile is two pixels at
/// (px - py, px + py) with px = tileX - playerX + Width/2 and py = tileY - playerY.
/// The color is radarcol at the land id, or at graphic + 0x4000 when a static is at least as high.
/// The sink key is 0x55000000. DrawRaw keeps the first upload, so each cell adds a slot under that key.
/// </summary>
public sealed class MiniMapGump : Gump {
    const ushort SmallMap = 5010;
    const ushort BigMap = 5011;
    const int CacheKey = 0x55000000;
    const int PlayerDot = CacheKey | 0x00FFFFFE;
    const int MobileDot = CacheKey | 0x00FFFFFF;
    // ClassicUO's hole is 0xFF080808 (16-bit 0x8421). Binary_Gump flips bit 15, so the stored pixel is 0x0421.
    const ushort Hole = 0x0421;

    readonly List<UOMobileState> _mobiles = [];
    readonly List<UOItemState> _items = [];
    readonly List<(ushort graphic, sbyte z)> _statics = [];
    readonly Dictionary<long, int> _slots = [];
    byte[] _frame;
    byte[] _pixels;
    ushort _graphic;
    int _px, _py, _facet = -1, _lastMap = -1, _timeMs, _slotCount;
    bool _large, _dots, _painted;

    static readonly byte[] WhiteDot = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF];
    static readonly byte[] RedDot = [0x00, 0xFC, 0x00, 0xFC, 0x00, 0xFC, 0x00, 0xFC];

    public MiniMapGump(World world) : base(world, 0, 0) {
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        WantUpdateSize = false;
        CreateMap();
    }

    public override void Update() {
        if (UOLoginNet.PlayerSerial == 0) return;
        base.Update();
        var map = UOLoginNet.MapIndex;
        if (_lastMap != map) {
            _lastMap = map;
            CreateMap();
        }
        var now = Environment.TickCount;
        if (_timeMs < now) {
            _dots = !_dots;
            _timeMs = now + 500;
        }
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed) return false;
        var sink = GumpArt.Sink;
        if (sink != null && Build(false) && _pixels != null) {
            sink.DrawRaw(0x55000000 | Slot(), x, y, Width, Height, _pixels, Width, Height);
            if (_dots) DrawDots(sink, x, y);
        }
        return base.Draw(x, y);
    }

    public override bool OnMouseDoubleClick(int x, int y) {
        _large = !_large;
        CreateMap();
        return true;
    }

    protected override void UpdateContents() => CreateMap();

    void CreateMap() => Build(true);

    bool Build(bool force) {
        var graphic = _large ? BigMap : SmallMap;
        var assets = UltimaOnline.Current?.Assets;
        if (assets == null || !assets.TryGumpPixels(graphic, out var src, out var w, out var h)) return false;
        if (src == null || w <= 0 || h <= 0 || src.Length < w * h * 2) return false;
        if (_graphic != graphic || _pixels == null || Width != w || Height != h) {
            _graphic = graphic;
            Width = w;
            Height = h;
            _frame = new byte[w * h * 2];
            Buffer.BlockCopy(src, 0, _frame, 0, _frame.Length);
            _pixels = new byte[_frame.Length];
            _painted = false;
            force = true;
        }
        var facet = UOLoginNet.MapIndex;
        if (facet != _facet) {
            _facet = facet;
            force = true;
        }
        if (!Player(out var px, out var py)) return false;
        if (!force && _painted && px == _px && py == _py) return true;
        _px = px;
        _py = py;
        _painted = true;
        Buffer.BlockCopy(_frame, 0, _pixels, 0, _frame.Length);
        Paint(assets, px, py);
        return true;
    }

    bool Player(out int x, out int y) {
        x = y = 0;
        var serial = UOLoginNet.PlayerSerial;
        if (serial == 0) return false;
        UOLoginNet.CopyActors(_mobiles, _items);
        for (var i = 0; i < _mobiles.Count; i++) {
            if (_mobiles[i].Serial != serial) continue;
            x = _mobiles[i].X;
            y = _mobiles[i].Y;
            return true;
        }
        return false;
    }

    void Paint(Assets assets, int lastX, int lastY) {
        var blockOffsetX = Width >> 2;
        var blockOffsetY = Height >> 2;
        var centerX = Width >> 1;
        var minBlockX = ((lastX - blockOffsetX) >> 3) - 1;
        var minBlockY = ((lastY - blockOffsetY) >> 3) - 1;
        var maxBlockX = ((lastX + blockOffsetX) >> 3) + 1;
        var maxBlockY = ((lastY + blockOffsetY) >> 3) + 1;
        if (minBlockX < 0) minBlockX = 0;
        if (minBlockY < 0) minBlockY = 0;
        for (var bx = minBlockX; bx <= maxBlockX; bx++) {
            var ox = bx << 3;
            for (var by = minBlockY; by <= maxBlockY; by++) {
                var oy = by << 3;
                for (var tx = 0; tx < 8; tx++) {
                    var wx = ox + tx;
                    for (var ty = 0; ty < 8; ty++) {
                        var wy = oy + ty;
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
                        var px = wx - lastX + centerX;
                        var py = wy - lastY;
                        Plot(px - py, px + py, color);
                    }
                }
            }
        }
    }

    void Plot(int gx, int gy, ushort color) {
        if (MissedY(gx, gy, color)) return;
        MissedY(gx, gy + 1, color);
    }

    bool MissedY(int gx, int gy, ushort color) {
        if ((uint)gx >= (uint)Width) return false;
        if ((uint)gy >= (uint)Height) return true;
        var i = (gy * Width + gx) * 2;
        var cur = (ushort)(_pixels[i] | (_pixels[i + 1] << 8));
        if ((cur & 0x7FFF) != Hole) return false;
        _pixels[i] = (byte)color;
        _pixels[i + 1] = (byte)(color >> 8);
        return false;
    }

    void DrawDots(IGumpArt sink, int x, int y) {
        var serial = UOLoginNet.PlayerSerial;
        var cx = x + (Width >> 1);
        var cy = y + (Height >> 1);
        for (var i = 0; i < _mobiles.Count; i++) {
            var m = _mobiles[i];
            if (m.Serial == serial) continue;
            var xx = m.X - _px;
            var yy = m.Y - _py;
            sink.DrawRaw(MobileDot, cx + xx - yy, cy + xx + yy, 2, 2, RedDot, 2, 2);
        }
        sink.DrawRaw(PlayerDot, cx, cy, 2, 2, WhiteDot, 2, 2);
    }

    int Slot() {
        var id = ((long)_px << 32) | (uint)_py | ((long)_facet << 48);
        if (_large) id |= 1L << 62;
        if (!_slots.TryGetValue(id, out var n)) {
            n = _slotCount++;
            if ((uint)n >= 0x00FFFFFEu) n = 0x00FFFFFD;
            _slots[id] = n;
        }
        return n;
    }

    /// <summary>ClassicUO GameObject.CanBeDrawn cases that do not read tiledata.</summary>
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
