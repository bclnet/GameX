// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Clients.UO;
using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// Opened by 0x24. Each 0x3C or 0x25 row is drawn at its stored X,Y on the ClassicUO container gump.
/// Double-click sends 0x06. Spellbook 0xFFFF and vendor 0x0030 are not opened here.
/// </summary>
public sealed class ContainerGump : Gump {
    const ushort CorpseGump = 0x0009;
    const ushort ChessGump = 0x091A;
    const ushort BackgammonGump = 0x092E;
    const int ItemGumpOffset = 11369;
    const int ArtKeyBase = 0x52000000;

    readonly ushort _gump;
    readonly GumpPic _back;
    readonly GumpPic _eye;
    readonly List<UOContainedState> _items = [];
    readonly List<Control> _added = [];
    readonly List<Stamp> _art = [];
    int _rev = -1;
    int _eyeAt;
    int _eyeFrame;
    bool _minimized;

    struct Stamp {
        public int Key, X, Y, W, H;
        public byte[] Px;
    }

    public ContainerGump(World world, uint serial, ushort gump) : base(world, serial, serial) {
        _gump = gump;
        CanCloseWithRightClick = true;
        CanMove = true;
        AcceptMouseInput = true;
        WantUpdateSize = false;
        X = 380;
        Y = 40;
        var data = Content(gump);
        _back = new BagPic(this, data.Graphic);
        Add(_back);
        if (!_back.IsDisposed) {
            Width = _back.Width;
            Height = _back.Height;
        }
        if (data.MinimizerX != 0 || data.MinimizerY != 0)
            Add(new MinHit(this, data.MinimizerX, data.MinimizerY));
        if (gump == CorpseGump) {
            _eye = new GumpPic(45, 30, 0x0045, 0);
            Add(_eye);
            _eyeAt = Environment.TickCount + 750;
        }
    }

    bool Minimized {
        get => _minimized;
        set {
            _minimized = value;
            var data = Content(_gump);
            if (_back != null && !_back.IsDisposed)
                _back.Graphic = value ? data.IconizedGraphic : data.Graphic;
            if (_back != null && !_back.IsDisposed) {
                Width = _back.Width;
                Height = _back.Height;
            }
            for (var i = 0; i < Children.Count; i++)
                Children[i].IsVisible = !value;
            if (_back != null && !_back.IsDisposed) _back.IsVisible = true;
        }
    }

    public override void Update() {
        var rev = UOLoginNet.WorldRevision;
        if (rev != _rev) {
            _rev = rev;
            Fill();
        }
        if (_eye != null && !_eye.IsDisposed && unchecked(Environment.TickCount - _eyeAt) >= 0) {
            _eyeFrame = _eyeFrame == 0 ? 1 : 0;
            _eyeAt = Environment.TickCount + 750;
            _eye.Graphic = (ushort)(0x0045 + _eyeFrame);
        }
        base.Update();
    }

    public override bool Draw(int x, int y) {
        if (!base.Draw(x, y)) return false;
        if (_minimized) return true;
        var sink = GumpArt.Sink;
        if (sink == null) return true;
        foreach (var s in _art)
            sink.DrawRaw(s.Key, x + s.X, y + s.Y, s.W, s.H, s.Px, s.W, s.H);
        return true;
    }

    void Fill() {
        for (var i = 0; i < _added.Count; i++) _added[i].Dispose();
        _added.Clear();
        _art.Clear();
        UOLoginNet.CopyContents(ServerSerial, _items);
        var chess = _gump == ChessGump;
        var board = chess || _gump == BackgammonGump;
        var assets = UltimaOnline.Current?.Assets;
        foreach (var it in _items) {
            if (it.Amount <= 0) continue;
            var shown = Shown(it.Graphic, it.Amount);
            var x = (int)(short)it.X;
            var y = (int)(short)it.Y;
            if (chess) y -= 20;
            if (board) {
                var pic = new GumpPic(x, y, (ushort)(shown - ItemGumpOffset), it.Hue);
                if (pic.IsDisposed) continue;
                pic.IsVisible = !_minimized;
                Add(pic);
                _added.Add(pic);
                AddHit(it.Serial, x, y, pic.Width, pic.Height);
                continue;
            }
            if (assets == null || !ArtPixels(assets, shown, it.Hue, out var key, out var pix, out var w, out var h)) continue;
            _art.Add(new Stamp { Key = key, X = x, Y = y, W = w, H = h, Px = pix });
            AddHit(it.Serial, x, y, w, h);
        }
    }

    void AddHit(uint serial, int x, int y, int w, int h) {
        if (w <= 0 || h <= 0) return;
        var hit = new UseLabel("", 0) { Serial = serial, X = x, Y = y, Width = w, Height = h, IsVisible = !_minimized };
        Add(hit);
        _added.Add(hit);
    }

    static ushort Shown(ushort graphic, ushort amount) {
        if (graphic != 0x0EEA && graphic != 0x0EED && graphic != 0x0EF0) return graphic;
        if (amount > 5) return (ushort)(graphic + 2);
        if (amount > 1) return (ushort)(graphic + 1);
        return graphic;
    }

    static readonly Dictionary<long, int> Slots = [];
    static readonly Dictionary<long, byte[]> Hued = [];

    static bool ArtPixels(Assets assets, ushort graphic, ushort hue, out int key, out byte[] pixels, out int w, out int h) {
        key = 0;
        pixels = null;
        w = h = 0;
        var file = (graphic & 0x7FFF) + 0x4000;
        if (file > 0xFFFF || !assets.TryArtPixels(file, out var src, out w, out h)) return false;
        if (src == null || w <= 0 || h <= 0 || src.Length < w * h * 2) return false;
        var index = hue & 0x3FFF;
        var onlyGray = (hue & 0x8000) != 0;
        if (index != 0 && assets.TryHueRamp(index, out var ramp)) {
            var id = (long)graphic | ((long)index << 16) | (onlyGray ? 1L << 32 : 0);
            if (!Hued.TryGetValue(id, out pixels)) Hued[id] = pixels = Tint(src, w, h, ramp, onlyGray);
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

    /// <summary>Same remap as the cursor and gump art. 0x8000 recolors a gray pixel only. The ramp is hues.mul.</summary>
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

    sealed class BagPic : GumpPic {
        readonly ContainerGump _owner;

        public BagPic(ContainerGump owner, ushort graphic) : base(0, 0, graphic, 0) => _owner = owner;

        public override bool OnMouseDoubleClick(int x, int y) {
            if (!_owner.Minimized) return false;
            _owner.Minimized = false;
            return true;
        }
    }

    sealed class MinHit : Control {
        readonly ContainerGump _owner;

        public MinHit(ContainerGump owner, int x, int y) {
            _owner = owner;
            X = x;
            Y = y;
            Width = 16;
            Height = 16;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) {
            if (!_owner.Minimized && !UOLoginNet.Holding) _owner.Minimized = true;
        }
    }

    /// <summary>
    /// ClassicUO ContainerData. X and Y are the left and top.
    /// Width and Height are the right and bottom edges, not a size.
    /// </summary>
    readonly struct ContainerData {
        public readonly ushort Graphic;
        public readonly ushort OpenSound;
        public readonly ushort ClosedSound;
        public readonly int X, Y, Width, Height;
        public readonly ushort IconizedGraphic;
        public readonly int MinimizerX, MinimizerY;

        public ContainerData(ushort graphic, ushort openSound, ushort closedSound, int x, int y, int width, int height, ushort iconizedGraphic = 0, int minimizerX = 0, int minimizerY = 0) {
            Graphic = graphic;
            OpenSound = openSound;
            ClosedSound = closedSound;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IconizedGraphic = iconizedGraphic;
            MinimizerX = minimizerX;
            MinimizerY = minimizerY;
        }
    }

    static ContainerData Content(ushort graphic) {
        if (Table.TryGetValue(graphic, out var data)) return data;
        return new ContainerData(graphic, 0, 0, 44, 65, 186, 159);
    }

    /// <summary>ClassicUO ContainerManager.MakeDefault.</summary>
    static readonly Dictionary<ushort, ContainerData> Table = new Dictionary<ushort, ContainerData> {
        [0x0007] = new ContainerData(0x0007, 0x0000, 0x0000, 30, 30, 270, 170),
        [0x0009] = new ContainerData(0x0009, 0x0000, 0x0000, 20, 85, 124, 196),
        [0x003C] = new ContainerData(0x003C, 0x0048, 0x0058, 44, 65, 186, 159, 0x0050, 105, 162),
        [0x003D] = new ContainerData(0x003D, 0x0048, 0x0058, 29, 34, 137, 128),
        [0x003E] = new ContainerData(0x003E, 0x002F, 0x002E, 33, 36, 142, 148),
        [0x003F] = new ContainerData(0x003F, 0x004F, 0x0058, 19, 47, 182, 123),
        [0x0040] = new ContainerData(0x0040, 0x002D, 0x002C, 16, 38, 152, 125),
        [0x0041] = new ContainerData(0x0041, 0x004F, 0x0058, 40, 30, 139, 123),
        [0x0042] = new ContainerData(0x0042, 0x002D, 0x002C, 18, 105, 162, 178),
        [0x0043] = new ContainerData(0x0043, 0x002D, 0x002C, 16, 51, 184, 124),
        [0x0044] = new ContainerData(0x0044, 0x002D, 0x002C, 20, 10, 170, 100),
        [0x0047] = new ContainerData(0x0047, 0x0000, 0x0000, 16, 10, 148, 138),
        [0x0048] = new ContainerData(0x0048, 0x002F, 0x002E, 16, 10, 154, 94),
        [0x0049] = new ContainerData(0x0049, 0x002D, 0x002C, 18, 105, 162, 178),
        [0x004A] = new ContainerData(0x004A, 0x002D, 0x002C, 18, 105, 162, 178),
        [0x004B] = new ContainerData(0x004B, 0x002D, 0x002C, 16, 51, 184, 124),
        [0x004C] = new ContainerData(0x004C, 0x002D, 0x002C, 46, 74, 196, 184),
        [0x004D] = new ContainerData(0x004D, 0x002F, 0x002E, 76, 12, 140, 68),
        [0x004E] = new ContainerData(0x004E, 0x002D, 0x002C, 24, 18, 100, 152),
        [0x004F] = new ContainerData(0x004F, 0x002D, 0x002C, 24, 18, 100, 152),
        [0x0051] = new ContainerData(0x0051, 0x002F, 0x002E, 16, 10, 154, 94),
        [0x0052] = new ContainerData(0x0052, 0x0000, 0x0000, 0, 0, 110, 62),
        [0x0102] = new ContainerData(0x0102, 0x004F, 0x0058, 35, 10, 190, 95),
        [0x0103] = new ContainerData(0x0103, 0x0048, 0x0058, 41, 21, 173, 104),
        [0x0104] = new ContainerData(0x0104, 0x002F, 0x002E, 10, 10, 160, 105),
        [0x0105] = new ContainerData(0x0105, 0x002F, 0x002E, 10, 10, 160, 105),
        [0x0106] = new ContainerData(0x0106, 0x002F, 0x002E, 10, 10, 160, 105),
        [0x0107] = new ContainerData(0x0107, 0x002F, 0x002E, 10, 10, 160, 105),
        [0x0108] = new ContainerData(0x0108, 0x004F, 0x0058, 10, 10, 160, 105),
        [0x0109] = new ContainerData(0x0109, 0x002D, 0x002C, 10, 10, 160, 105),
        [0x010A] = new ContainerData(0x010A, 0x002D, 0x002C, 10, 10, 160, 105),
        [0x010B] = new ContainerData(0x010B, 0x002D, 0x002C, 10, 10, 160, 105),
        [0x010C] = new ContainerData(0x010C, 0x002F, 0x002E, 10, 10, 160, 105),
        [0x010D] = new ContainerData(0x010D, 0x002F, 0x002E, 10, 10, 160, 105),
        [0x010E] = new ContainerData(0x010E, 0x002F, 0x002E, 10, 10, 160, 105),
        [0x0116] = new ContainerData(0x0116, 0x0000, 0x0000, 40, 25, 140, 110),
        [0x011A] = new ContainerData(0x011A, 0x0000, 0x0000, 10, 65, 125, 160),
        [0x011B] = new ContainerData(0x011B, 0x0000, 0x0000, 45, 10, 175, 95),
        [0x011C] = new ContainerData(0x011C, 0x0000, 0x0000, 37, 10, 175, 105),
        [0x011D] = new ContainerData(0x011D, 0x0000, 0x0000, 43, 10, 165, 110),
        [0x011E] = new ContainerData(0x011E, 0x0000, 0x0000, 30, 22, 263, 106),
        [0x011F] = new ContainerData(0x011F, 0x0000, 0x0000, 45, 10, 175, 95),
        [0x0120] = new ContainerData(0x0120, 0x0000, 0x0000, 56, 30, 160, 107),
        [0x0121] = new ContainerData(0x0121, 0x0000, 0x0000, 77, 32, 162, 107),
        [0x0123] = new ContainerData(0x0123, 0x0000, 0x0000, 36, 19, 111, 157),
        [0x0484] = new ContainerData(0x0484, 0x0000, 0x0000, 0, 45, 175, 125),
        [0x058E] = new ContainerData(0x058E, 0x0000, 0x0000, 50, 150, 348, 250),
        [0x06D3] = new ContainerData(0x06D3, 0x0000, 0x0000, 10, 65, 125, 160),
        [0x06D4] = new ContainerData(0x06D4, 0x0000, 0x0000, 10, 65, 125, 160),
        [0x06D5] = new ContainerData(0x06D5, 0x0000, 0x0000, 10, 65, 125, 160),
        [0x06D6] = new ContainerData(0x06D6, 0x0000, 0x0000, 10, 65, 125, 160),
        [0x06E5] = new ContainerData(0x06E5, 0x0000, 0x0000, 66, 74, 306, 520),
        [0x06E6] = new ContainerData(0x06E6, 0x0000, 0x0000, 66, 74, 306, 520),
        [0x06E7] = new ContainerData(0x06E7, 0x0000, 0x0000, 50, 60, 548, 308),
        [0x06E8] = new ContainerData(0x06E8, 0x0000, 0x0000, 50, 60, 548, 308),
        [0x06E9] = new ContainerData(0x06E9, 0x0000, 0x0000, 60, 80, 318, 324),
        [0x06EA] = new ContainerData(0x06EA, 0x0000, 0x0000, 50, 60, 548, 308),
        [0x091A] = new ContainerData(0x091A, 0x0000, 0x0000, 0, 0, 282, 230),
        [0x092E] = new ContainerData(0x092E, 0x0000, 0x0000, 0, 0, 282, 210),
        [0x266A] = new ContainerData(0x266A, 0x0000, 0x0000, 16, 51, 184, 124),
        [0x266B] = new ContainerData(0x266B, 0x0000, 0x0000, 16, 51, 184, 124),
        [0x2A63] = new ContainerData(0x2A63, 0x0187, 0x01C9, 60, 33, 460, 348),
        [0x4D0C] = new ContainerData(0x4D0C, 0x0000, 0x0000, 25, 65, 220, 155),
        [0x775E] = new ContainerData(0x775E, 0x0048, 0x0058, 44, 65, 186, 159, 0x775F, 105, 178),
        [0x7760] = new ContainerData(0x7760, 0x0048, 0x0058, 44, 65, 186, 159, 0x7761, 105, 178),
        [0x7762] = new ContainerData(0x7762, 0x0048, 0x0058, 44, 65, 186, 159, 0x7763, 105, 178),
        [0x777A] = new ContainerData(0x777A, 0x0000, 0x0000, 32, 40, 184, 116),
        [0x9CD9] = new ContainerData(0x9CD9, 0x0000, 0x0000, 10, 10, 160, 105),
        [0x9CDB] = new ContainerData(0x9CDB, 0x0000, 0x0000, 50, 60, 548, 308),
        [0x9CDD] = new ContainerData(0x9CDD, 0x0000, 0x0000, 50, 60, 548, 308),
        [0x9CDF] = new ContainerData(0x9CDF, 0x0000, 0x0000, 50, 60, 548, 308),
        [0x9CE3] = new ContainerData(0x9CE3, 0x0000, 0x0000, 50, 60, 548, 308),
        [0x9CE4] = new ContainerData(0x9CE4, 0x0000, 0x0000, 44, 65, 186, 159),
        [0x9CE5] = new ContainerData(0x9CE5, 0x0000, 0x0000, 44, 65, 186, 159),
        [0x9CE7] = new ContainerData(0x9CE7, 0x0000, 0x0000, 44, 65, 186, 159),
    };
}
