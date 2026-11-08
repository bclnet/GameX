// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO grid loot. Cells are 50 pixels. Non-stackable items are placed, then stackable ones.
/// Stackable is tiledata Generic. Hair 11, face 15, and beard 16 are not lootable.
/// A left click lifts the slider value, or 1 when the item does not stack. The window closes past 3 tiles.
/// </summary>
public sealed class GridLootGump : Gump {
    const int MaxWidth = 300;
    const int MaxHeight = 420;
    const int Cell = 50;
    const int ArtKeyBase = 0x56000000;
    const int GrayKey = 0x568001;
    const int YellowKey = 0x568002;
    const ushort Gray = 0xC210;
    const ushort Yellow = 0xFFE0;

    static readonly byte[] GrayPx = { 0x10, 0xC2 };
    static readonly byte[] YellowPx = { 0xE0, 0xFF };
    static readonly Dictionary<long, int> Slots = [];
    static readonly Dictionary<long, byte[]> Hued = [];
    static int _lastX = int.MinValue;
    static int _lastY = 100;
    static Binary_TileData.ItemData[] _tiles;
    static bool _tilesMiss;

    readonly List<UOMobileState> _mobs = [];
    readonly List<UOItemState> _worldItems = [];
    readonly List<UOContainedState> _rows = [];
    readonly List<GridLootItem> _cells = [];
    readonly AlphaBlendControl _background;
    readonly Label _pageLabel;
    readonly Label _nameLabel;
    readonly NiceButton _prev;
    readonly NiceButton _next;
    readonly NiceButton _bag;
    int _rev = -1;
    int _page = 1;
    int _pages = 1;
    bool _live;
    bool _drag;
    int _dragX, _dragY;

    // Profile path is not ported.
    // 0 = none, 1 = only grid, 2 = both.
    public int GridLootType { get; set; }

    public GridLootGump(World world, uint local) : base(world, local, 0) {
        GridLootType = 0;
        if (!FindCorpse(out _, out _)) {
            Dispose();
            return;
        }
        _live = true;
        if (_lastX == int.MinValue) _lastX = GridLootType == 2 ? 200 : 100;
        X = _lastX;
        Y = _lastY;
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        WantUpdateSize = false;
        Width = 100;
        Height = 120;
        _background = new AlphaBlendControl { WantUpdateSize = false };
        Add(_background);
        _bag = Chrome(3, Height - 23, 100, 20, 2, "Set loot bag");
        _prev = Chrome(Width - 80, Height - 20, 40, 20, 0, "<<");
        _next = Chrome(Width - 40, Height - 20, 40, 20, 1, ">>");
        if (_prev != null) _prev.IsVisible = false;
        if (_next != null) _next.IsVisible = false;
        _pageLabel = new Label("1", true, 999, 1);
        _nameLabel = new Label(CorpseName(), true, 0x0481, 1);
        if (_bag != null) Add(_bag);
        if (_prev != null) Add(_prev);
        if (_next != null) Add(_next);
        Add(_pageLabel);
        Add(_nameLabel);
        Rebuild();
        if (!IsDisposed) PlaceChrome();
    }

    public override void Update() {
        if (IsDisposed) return;
        if (!FindCorpse(out var cx, out var cy) || TooFar(cx, cy)) {
            Dispose();
            return;
        }
        Drag();
        var rev = UOLoginNet.WorldRevision;
        if (rev != _rev) {
            _rev = rev;
            Rebuild();
            if (IsDisposed) return;
        }
        if (Width < 100) Width = 100;
        if (Height < 120) Height = 120;
        _nameLabel.Text = CorpseName();
        PlaceChrome();
        base.Update();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        return base.Draw(x, y);
    }

    public override void OnButtonClick(int buttonID) {
        if (buttonID == 0) {
            _page--;
            if (_page <= 1) {
                _page = 1;
                if (_prev != null) _prev.IsVisible = false;
            }
            if (_next != null) _next.IsVisible = true;
            ActivePage = _page;
            _pageLabel.Text = ActivePage.ToString();
        }
        else if (buttonID == 1) {
            _page++;
            if (_page >= _pages) {
                _page = _pages;
                if (_next != null) _next.IsVisible = false;
            }
            if (_prev != null) _prev.IsVisible = true;
            ActivePage = _page;
            _pageLabel.Text = ActivePage.ToString();
        }
        else if (buttonID == 2) {
            // grab bag target is not in this tree
        }
        else base.OnButtonClick(buttonID);
    }

    public override void Dispose() {
        if (_live) {
            _lastX = X;
            _lastY = Y;
        }
        base.Dispose();
    }

    void Drag() {
        var held = UIManager.LastControlMouseDown(1);
        if (Mouse.LButtonPressed && held == this) {
            if (!_drag) {
                _drag = true;
                _dragX = X;
                _dragY = Y;
            }
            var o = Mouse.LDragOffset;
            X = _dragX + o.X;
            Y = _dragY + o.Y;
        }
        else _drag = false;
    }

    void Rebuild() {
        for (var i = 0; i < _cells.Count; i++) _cells[i].Dispose();
        _cells.Clear();
        UOLoginNet.CopyContents(LocalSerial, _rows);
        var x = 20;
        var y = 20;
        var count = 0;
        _pages = 1;
        var line = 1;
        for (var group = 0; group < 2; group++) {
            for (var i = 0; i < _rows.Count; i++) {
                var it = _rows[i];
                if (!Belongs(it, group) || !Lootable(it)) continue;
                var cell = new GridLootItem(it, Cell, Stackable(it));
                if (x >= MaxWidth - 20) {
                    x = 20;
                    line++;
                    y += cell.Height + 20;
                    if (y >= MaxHeight - 60) {
                        _pages++;
                        y = 20;
                    }
                }
                cell.X = x;
                cell.Y = y + 20;
                Add(cell, _pages);
                _cells.Add(cell);
                x += cell.Width + 20;
                count++;
            }
        }
        var height = 20 + 40 + (Cell + 20) * line + 40;
        if (height >= MaxHeight - 40) height = MaxHeight;
        Width = MaxWidth;
        Height = height;
        if (ActivePage <= 1) {
            ActivePage = 1;
            _page = 1;
            if (_next != null) _next.IsVisible = _pages > 1;
            if (_prev != null) _prev.IsVisible = false;
        }
        else if (ActivePage >= _pages) {
            ActivePage = _pages;
            _page = _pages;
            if (_next != null) _next.IsVisible = false;
            if (_prev != null) _prev.IsVisible = _pages > 1;
        }
        else {
            if (_next != null) _next.IsVisible = true;
            if (_prev != null) _prev.IsVisible = true;
        }
        _pageLabel.Text = ActivePage.ToString();
        if (count == 0) {
            // [GridLoot]: Corpse is empty! — no GameActions.Print in this tree
            Dispose();
        }
    }

    void PlaceChrome() {
        if (_background != null && !_background.IsDisposed) {
            _background.Width = Width;
            _background.Height = Height;
            _background.WantUpdateSize = false;
        }
        if (_prev != null) {
            _prev.X = Width - 80;
            _prev.Y = Height - 20;
        }
        if (_next != null) {
            _next.X = Width - 40;
            _next.Y = Height - 20;
        }
        if (_bag != null) {
            _bag.X = 3;
            _bag.Y = Height - 23;
        }
        _pageLabel.X = Width / 2 - 5;
        _pageLabel.Y = Height - 20;
        _nameLabel.X = Math.Max(0, (Width - _nameLabel.Width) / 2);
        _nameLabel.Y = 0;
    }

    static NiceButton Chrome(int x, int y, int w, int h, int id, string text) {
        var b = new NiceButton(x, y, w, h, ButtonAction.Activate, text) {
            ButtonParameter = id,
            IsSelectable = false
        };
        return b.IsDisposed ? null : b;
    }

    bool Belongs(UOContainedState it, int group) => Stackable(it) ? group > 0 : group == 0;

    static bool Stackable(UOContainedState it) {
        if (TryTile(it.Graphic, out var data)) return data.IsGeneric;
        return it.Amount > 1;
    }

    static bool Lootable(UOContainedState it) {
        if (it.Graphic == 0) return false;
        if (!TryTile(it.Graphic, out var data)) return true;
        var layer = data.Quality;
        return layer != 11 && layer != 15 && layer != 16;
    }

    bool FindCorpse(out int x, out int y) {
        x = y = 0;
        UOLoginNet.CopyActors(_mobs, _worldItems);
        for (var i = 0; i < _worldItems.Count; i++) {
            if (_worldItems[i].Serial != LocalSerial) continue;
            x = _worldItems[i].X;
            y = _worldItems[i].Y;
            return true;
        }
        return false;
    }

    static bool TooFar(int x, int y) {
        if (UOLoginNet.PlayerSerial == 0) return false;
        var d = Math.Max(Math.Abs(UOLoginNet.PlayerX - x), Math.Abs(UOLoginNet.PlayerY - y));
        return d > 3;
    }

    string CorpseName() {
        var name = UOLoginNet.NameOf(LocalSerial);
        return string.IsNullOrEmpty(name) ? "a corpse" : name;
    }

    static void Outline(int x, int y, int w, int h, int key, byte[] px) {
        if (w < 1 || h < 1) return;
        var sink = GumpArt.Sink;
        if (sink == null) return;
        sink.DrawRaw(key, x, y, w, 1, px, 1, 1);
        sink.DrawRaw(key, x, y + h - 1, w, 1, px, 1, 1);
        sink.DrawRaw(key, x, y, 1, h, px, 1, 1);
        sink.DrawRaw(key, x + w - 1, y, 1, h, px, 1, 1);
    }

    static bool TryTile(ushort graphic, out Binary_TileData.ItemData data) {
        var tiles = Tiles();
        if (tiles == null || graphic >= tiles.Length) {
            data = default;
            return false;
        }
        data = tiles[graphic];
        return true;
    }

    static Binary_TileData.ItemData[] Tiles() {
        if (_tiles != null || _tilesMiss) return _tiles;
        var root = UltimaOnline.Current?.ClientPath;
        if (string.IsNullOrEmpty(root)) return null;
        _tilesMiss = true;
        try {
            var path = Path.Combine(root, "tiledata.mul");
            if (!File.Exists(path)) return null;
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var r = new BinaryReader(fs);
            _tiles = new Binary_TileData(r).ItemDatas;
        }
        catch { _tiles = null; }
        return _tiles;
    }

    static bool ArtPixels(ushort graphic, ushort hue, bool partial, out int key, out byte[] pixels, out int w, out int h) {
        key = 0;
        pixels = null;
        w = h = 0;
        var assets = UltimaOnline.Current?.Assets;
        var file = graphic + 0x4000;
        if (assets == null || file > 0xFFFF || !assets.TryArtPixels(file, out var src, out w, out h)) return false;
        if (src == null || w <= 0 || h <= 0 || src.Length < w * h * 2) return false;
        var index = hue & 0x3FFF;
        if (index != 0 && assets.TryHueRamp(index, out var ramp)) {
            var id = (long)graphic | ((long)index << 16) | (partial ? 1L << 32 : 0);
            if (!Hued.TryGetValue(id, out pixels)) Hued[id] = pixels = Tint(src, w, h, ramp, partial);
            key = Slot(id);
            return pixels != null;
        }
        pixels = src;
        key = Slot(graphic);
        return true;
    }

    static int Slot(long id) {
        if (!Slots.TryGetValue(id, out var n)) {
            n = Slots.Count;
            Slots[id] = n;
        }
        return ArtKeyBase + n;
    }

    static byte[] Tint(byte[] src, int width, int height, ushort[] ramp, bool onlyGray) {
        var count = width * height;
        var dest = new byte[count * 2];
        for (var i = 0; i < count; i++) {
            var c = (ushort)(src[i * 2] | (src[i * 2 + 1] << 8));
            if (c != 0) {
                var r = (c >> 10) & 31;
                var g = (c >> 5) & 31;
                var b = c & 31;
                if (!onlyGray || (r == g && g == b)) {
                    c = ramp[Math.Max(r, Math.Max(g, b))];
                    if (c != 0) c |= 0x8000;
                }
            }
            dest[i * 2] = (byte)c;
            dest[i * 2 + 1] = (byte)(c >> 8);
        }
        return dest;
    }

    static ushort Shown(ushort graphic, ushort amount) {
        if (graphic != 0x0EEA && graphic != 0x0EED && graphic != 0x0EF0) return graphic;
        if (amount > 5) return (ushort)(graphic + 2);
        if (amount > 1) return (ushort)(graphic + 1);
        return graphic;
    }

    sealed class GridLootItem : Control {
        readonly uint _serial;
        readonly int _size;
        readonly int _max;
        readonly HSliderBar _slider;
        readonly AlphaBlendControl _blend;
        readonly int _key;
        readonly byte[] _px;
        readonly int _sw, _sh, _dw, _dh, _ox, _oy;

        public GridLootItem(UOContainedState item, int size, bool stack) {
            _serial = item.Serial;
            _size = size;
            _max = stack && item.Amount > 1 ? item.Amount : 1;
            LocalSerial = item.Serial;
            CanMove = false;
            AcceptMouseInput = true;
            WantUpdateSize = false;
            Width = size;
            Height = size + 15;
            _slider = new HSliderBar(0, 0, size, 1, _max, _max, HSliderBarStyle.MetalWidgetRecessedBar, true, 0, 0xFFFF, true, true) {
                IsVisible = _max > 1,
                IsEnabled = _max > 1
            };
            Add(_slider);
            _blend = new AlphaBlendControl { Y = 15, Width = size, Height = size, WantUpdateSize = false };
            Add(_blend);
            var partial = (item.Hue & 0x8000) != 0;
            if (TryTile(item.Graphic, out var data)) partial = data.IsPartialHue;
            var shown = Shown(item.Graphic, item.Amount);
            if (!ArtPixels(shown, item.Hue, partial, out _key, out _px, out _sw, out _sh)) return;
            if (_sw < size) { _dw = _sw; _ox = (size - _sw) / 2; }
            else _dw = size;
            if (_sh < size) { _dh = _sh; _oy = (size - _sh) / 2; }
            else _dh = size;
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            var top = y + 15;
            if (_blend != null && !_blend.IsDisposed) _blend.Draw(x, top);
            if (_key != 0 && _px != null && _dw > 0 && _dh > 0)
                GumpArt.Sink?.DrawRaw(_key, x + _ox, top + _oy, _dw, _dh, _px, _sw, _sh);
            Outline(x, top, _size, _size, GrayKey, GrayPx);
            if (UIManager.MouseOverControl == this) Outline(x, top, _size, _size, YellowKey, YellowPx);
            if (_slider != null && !_slider.IsDisposed && _slider.IsVisible)
                _slider.Draw(x + _slider.X, y + _slider.Y);
            return true;
        }

        public override void OnMouseClick(int x, int y) {
            var count = _slider != null && !_slider.IsDisposed ? _slider.Value : 1;
            if (count < 1) count = 1;
            if (count > _max) count = _max;
            UOLoginNet.Lift(_serial, count);
        }
    }
}
