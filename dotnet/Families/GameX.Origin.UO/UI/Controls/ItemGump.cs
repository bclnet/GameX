// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using GameX.Origin.Structs.UO;
using OpenX.Client;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ItemGump. Art is the graphic plus 0x4000. A gump row is GumpArtDraw.Pic.
/// Hue stays on this control. Amount is UOLoginNet.AmountOf. There is no pixel test on Assets.
/// A drag past 5 pixels calls Lift. Shift on a count above 1 opens one split. A double-click calls Use.
/// </summary>
public class ItemGump : Control {
    const int ArtKeyBase = 0x54200000;
    const int MinPickupDrag = 5;

    static readonly Dictionary<long, int> Slots = [];
    static readonly Dictionary<long, byte[]> Hued = [];
    static Binary_TileData.ItemData[] _items;
    static bool _itemsMiss;

    readonly Gump _gump;
    readonly bool _isGump;
    ushort _graphic;
    int _w, _h, _key;
    byte[] _px;
    ushort _drawnHue;
    bool _drawnPartial, _partialKnown;

    public ItemGump(Gump gump, uint serial, ushort graphic, ushort hue, int x, int y, bool isGump = false) {
        _gump = gump;
        _isGump = isGump;
        AcceptMouseInput = true;
        X = x;
        Y = y;
        HighlightOnMouseOver = true;
        CanPickUp = true;
        LocalSerial = serial;
        WantUpdateSize = false;
        CanMove = false;
        Hue = hue;
        Graphic = graphic;
    }

    public ushort Hue { get; set; }
    public bool IsPartialHue { get; set; }
    public bool HighlightOnMouseOver { get; set; }
    public bool CanPickUp { get; set; }

    public ushort Graphic {
        get => _graphic;
        set {
            _graphic = value;
            _px = null;
            _partialKnown = false;
            var assets = UltimaOnline.Current?.Assets;
            if (_isGump) {
                if (assets == null) {
                    _w = _h = 0;
                    return;
                }
                if (!GumpMetrics.TrySize(value, out var w, out var h) || w <= 0 || h <= 0) {
                    Dispose();
                    return;
                }
                _w = w;
                _h = h;
            }
            else {
                if (assets == null) {
                    _w = _h = 0;
                    return;
                }
                var file = (value & 0x7FFF) + 0x4000;
                if (file > 0xFFFF || !assets.TryArtPixels(file, out var src, out _w, out _h) || src == null || _w <= 0 || _h <= 0) {
                    Dispose();
                    return;
                }
                NotePartial();
            }
            var pad = Stacked() ? 5 : 0;
            Width = _w + pad;
            Height = _h + pad;
        }
    }

    public override void Update() {
        if (IsDisposed) return;
        if (_w == 0 && _h == 0 && UltimaOnline.Current?.Assets != null) Graphic = _graphic;
        if (IsDisposed) return;
        NotePartial();
        var pad = _w > 0 && Stacked() ? 5 : 0;
        if (_w > 0 && (Width != _w + pad || Height != _h + pad)) {
            Width = _w + pad;
            Height = _h + pad;
        }
        base.Update();
        if (_gump.IsDisposed || !CanPickUp || UOLoginNet.Holding || UOLoginNet.HasTarget || !Mouse.LButtonPressed) return;
        if (UIManager.LastControlMouseDown((int)MouseButtonType.Left) != this) return;
        if (!Waited() && !Dragged()) return;
        if (UOLoginNet.IsDead(UOLoginNet.PlayerSerial)) return;
        if (GameController.Shift && UOLoginNet.AmountOf(LocalSerial) > 1) {
            if (UIManager.GetGump<SplitMenuGump>(LocalSerial) == null)
                UIManager.Add(new SplitMenuGump(LocalSerial));
            return;
        }
        var split = UIManager.GetGump<SplitMenuGump>(LocalSerial);
        if (split != null) {
            split.X = Mouse.Position.X - 80;
            split.Y = Mouse.Position.Y - 40;
            return;
        }
        UOLoginNet.Lift(LocalSerial);
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        var hue = Hue;
        var partial = IsPartialHue;
        if (HighlightOnMouseOver && UIManager.MouseOverControl == this) {
            hue = 0x0035;
            partial = false;
        }
        var stacked = Stacked();
        if (_isGump) {
            if (_w > 0 && _h > 0) {
                GumpArtDraw.Pic(_graphic, x, y, _w, _h, false, hue);
                if (stacked) GumpArtDraw.Pic(_graphic, x + 5, y + 5, _w, _h, false, hue);
            }
        }
        else if (TryArt(hue, partial, out var key, out var px) && GumpArt.Sink != null) {
            GumpArt.Sink.DrawRaw(key, x, y, _w, _h, px, _w, _h);
            if (stacked) GumpArt.Sink.DrawRaw(key, x + 5, y + 5, _w, _h, px, _w, _h);
        }
        return base.Draw(x, y);
    }

    public override void OnMouseClick(int x, int y) {
        if (LocalSerial == 0) return;
        if (UOLoginNet.HasTarget) UOLoginNet.TargetSerial(LocalSerial);
        else UOLoginNet.RequestPopup(LocalSerial);
    }

    /// <summary>No profile loot-inside-containers path. A double-click is Use, or the current target.</summary>
    public override bool OnMouseDoubleClick(int x, int y) {
        if (LocalSerial == 0) return false;
        if (UOLoginNet.HasTarget || UOLoginNet.TargetedJustNow) {
            if (UOLoginNet.HasTarget) UOLoginNet.TargetSerial(LocalSerial);
            return true;
        }
        UOLoginNet.Use(LocalSerial);
        return true;
    }

    bool Stacked() {
        var amount = UOLoginNet.AmountOf(LocalSerial);
        if (amount <= 1) return false;
        var tile = (ushort)(_graphic & 0x7FFF);
        if (tile == 0x0EEA || tile == 0x0EED || tile == 0x0EF0) return false;
        var items = Items();
        if (items == null || tile >= items.Length) return true;
        return (items[tile].Flags & TileFlag.Generic) != 0;
    }

    void NotePartial() {
        if (_isGump || _partialKnown) return;
        var items = Items();
        if (items == null) return;
        _partialKnown = true;
        var tile = (ushort)(_graphic & 0x7FFF);
        if (tile < items.Length) IsPartialHue = items[tile].IsPartialHue;
    }

    bool TryArt(ushort hue, bool partial, out int key, out byte[] px) {
        key = 0;
        px = null;
        if (_w <= 0 || _h <= 0) return false;
        var index = (ushort)(hue & 0x3FFF);
        var onlyGray = partial || (hue & 0x8000) != 0;
        if (_px != null && _drawnHue == index && _drawnPartial == onlyGray) {
            key = _key;
            px = _px;
            return true;
        }
        var assets = UltimaOnline.Current?.Assets;
        if (assets == null) return false;
        var file = (_graphic & 0x7FFF) + 0x4000;
        if (file > 0xFFFF || !assets.TryArtPixels(file, out var src, out var w, out var h)) return false;
        if (src == null || w <= 0 || h <= 0 || src.Length < w * h * 2) return false;
        if (index != 0 && assets.TryHueRamp(index, out var ramp)) {
            var id = (long)(_graphic & 0x7FFF) | ((long)index << 16) | (onlyGray ? 1L << 32 : 0);
            if (!Hued.TryGetValue(id, out px)) Hued[id] = px = Tint(src, w, h, ramp, onlyGray);
            key = Slot(id);
        }
        else {
            px = src;
            key = Slot(_graphic & 0x7FFF);
        }
        _px = px;
        _key = key;
        _drawnHue = index;
        _drawnPartial = onlyGray;
        _w = w;
        _h = h;
        return px != null;
    }

    static int Slot(long id) {
        if (!Slots.TryGetValue(id, out var n)) {
            n = Slots.Count;
            Slots[id] = n;
        }
        return ArtKeyBase + n;
    }

    /// <summary>Same remap as container art. Partial hue, or hue bit 0x8000, recolors a gray pixel only.</summary>
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

    static bool Waited() {
        var last = Mouse.LastLeftButtonClickTime;
        return last != 0 && last != 0xFFFF_FFFF && last + Mouse.MOUSE_DELAY_DOUBLE_CLICK < GlobalTime.Ticks;
    }

    static bool Dragged() {
        var o = Mouse.LDragOffset;
        return Math.Abs(o.X) >= MinPickupDrag || Math.Abs(o.Y) >= MinPickupDrag;
    }

    static Binary_TileData.ItemData[] Items() {
        if (_items != null || _itemsMiss) return _items;
        var root = UltimaOnline.Current?.ClientPath;
        if (string.IsNullOrEmpty(root)) return null;
        _itemsMiss = true;
        try {
            var path = Path.Combine(root, "tiledata.mul");
            if (!File.Exists(path)) return null;
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var r = new BinaryReader(fs);
            _items = new Binary_TileData(r).ItemDatas;
        }
        catch { _items = null; }
        return _items;
    }
}
