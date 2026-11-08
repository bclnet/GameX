// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO TradingGump. Accept sends Send_TradeResponse code 2.
/// Dispose sends code 1, including a server close. A gold edit sends Send_TradeUpdateGold.
/// Each side is a 110 by 80 box. A row is a UseLabel, the same click ContainerGump uses.
/// </summary>
public sealed class TradingGump : Gump {
    const int BoxW = 110;
    const int BoxH = 80;
    const int ArtBase = 0x57400000;
    const int BlackKey = 0x574E0000;
    const ulong PlatinumValue = 1_000_000_000;

    static readonly Dictionary<long, int> Slots = [];
    static readonly Dictionary<long, byte[]> Hued = [];
    static readonly byte[] Black = [0x00, 0x80];

    readonly string _name;
    readonly Label[] _myCoins = new Label[2];
    readonly Label[] _hisCoins = new Label[2];
    readonly TextBox[] _myCoinsEntries = new TextBox[2];
    readonly List<Control> _hits = [];
    readonly List<Stamp> _art = [];
    readonly List<UOContainedState> _rows = [];
    readonly List<UOEquipState> _equip = [];
    readonly DropPad _pad;

    uint _gold = 0;
    uint _platinum = 0;
    uint _hisGold = 0;
    uint _hisPlatinum = 0;
    uint _myGoldEntry = 0;
    uint _myPlatEntry = 0;
    bool _imAccepting = false;
    bool _heIsAccepting = false;
    bool _secure = false;
    bool _colorBoxes = false;
    bool _cancelSent = false;
    ushort _back = 0x0866;
    ushort _hisGraphic = 0x0867;
    int _hisX = 266;
    int _hisY = 160;
    int _myX = 45;
    int _myY = 70;
    int _opX = 192;
    int _opY = 70;
    int _rev = -1;
    Checkbox _myCheckbox = null;

    struct Stamp {
        public int Key, X, Y, W, H;
        public byte[] Px;
    }

    public TradingGump(World world, uint local, string name, uint id1, uint id2) : base(world, local, 0) {
        _name = name ?? "";
        ID1 = id1;
        ID2 = id2;
        _pad = new DropPad(this);
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        BuildGump();
    }

    public uint ID1 { get; }
    public uint ID2 { get; }

    public uint Gold {
        get => _gold;
        set {
            if (_gold == value) return;
            _gold = value;
            if (_secure && _myCoins[0] != null) _myCoins[0].Text = _gold.ToString("N0");
        }
    }

    public uint Platinum {
        get => _platinum;
        set {
            if (_platinum == value) return;
            _platinum = value;
            if (_secure && _myCoins[1] != null) _myCoins[1].Text = _platinum.ToString("N0");
        }
    }

    public uint HisGold {
        get => _hisGold;
        set {
            if (_hisGold == value) return;
            _hisGold = value;
            if (_secure && _hisCoins[0] != null) _hisCoins[0].Text = _hisGold.ToString("N0");
        }
    }

    public uint HisPlatinum {
        get => _hisPlatinum;
        set {
            if (_hisPlatinum == value) return;
            _hisPlatinum = value;
            if (_secure && _hisCoins[1] != null) _hisCoins[1].Text = _hisPlatinum.ToString("N0");
        }
    }

    public bool ImAccepting {
        get => _imAccepting;
        set {
            if (_imAccepting == value) return;
            _imAccepting = value;
            SetCheckboxes();
        }
    }

    public bool HeIsAccepting {
        get => _heIsAccepting;
        set {
            if (_heIsAccepting == value) return;
            _heIsAccepting = value;
            SetCheckboxes();
        }
    }

    public void RequestUpdateContents() => InvalidateContents = true;

    public override void Update() {
        if (IsDisposed) return;
        if (Width == 0 && GumpMetrics.TrySize(_back, out var w, out var h) && w > 0) {
            Width = w;
            Height = h;
        }
        var rev = UOLoginNet.WorldRevision;
        if (rev != _rev) {
            _rev = rev;
            InvalidateContents = true;
        }
        PollCoins();
        base.Update();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        GumpArtDraw.Pic(_back, x, y, 0, 0, false);
        if (_colorBoxes) {
            var sink = GumpArt.Sink;
            if (sink != null) {
                sink.DrawRaw(BlackKey, x + 45, y + 90, 110, 60, Black, 1, 1);
                sink.DrawRaw(BlackKey, x + 192, y + 70, 110, 60, Black, 1, 1);
            }
        }
        var art = GumpArt.Sink;
        if (art != null)
            for (var i = 0; i < _art.Count; i++) {
                var s = _art[i];
                art.DrawRaw(s.Key, x + s.X, y + s.Y, s.W, s.H, s.Px, s.W, s.H);
            }
        var drawn = base.Draw(x, y);
        GumpArtDraw.Pic(_hisGraphic, x + _hisX, y + _hisY, 0, 0, false);
        return drawn;
    }

    public override void Dispose() {
        if (IsDisposed) return;
        var id = ID1;
        var send = !_cancelSent;
        _cancelSent = true;
        base.Dispose();
        if (send) UOLoginNet.Send(OutgoingPackets.Send_TradeResponse(id, 1, false));
    }

    protected override void UpdateContents() {
        for (var i = 0; i < _hits.Count; i++) _hits[i].Dispose();
        _hits.Clear();
        _art.Clear();
        Fill(ID1, _myX, _myY);
        Fill(ID2, _opX, _opY);
    }

    void BuildGump() {
        var version = UltimaOnline.Current != null ? UltimaOnline.Current.Version : default;
        _secure = version >= ClientVersion.CV_704565;
        _colorBoxes = version < ClientVersion.CV_500A;
        _back = _secure ? (ushort)0x088A : (ushort)0x0866;
        WantUpdateSize = false;
        if (GumpMetrics.TrySize(_back, out var gw, out var gh) && gw > 0) {
            Width = gw;
            Height = gh;
        }
        var font = (byte)(_secure ? 3 : 1);
        var hue = (ushort)(_secure ? 0x0481 : 0x0386);
        Add(new Label(UOLoginNet.NameOf(UOLoginNet.PlayerSerial), false, hue, font) { X = _secure ? 73 : 84, Y = _secure ? 32 : 40 });
        var textW = GumpArt.Sink?.MeasureText(_name, font) ?? _name.Length * 6;
        Add(new Label(_name, false, hue, font) { X = (_secure ? 250 : 260) - textW, Y = _secure ? 244 : 170 });
        if (_secure) {
            Add(_myCoins[0] = new Label("0", false, 0x0481, 9) { X = 43, Y = 67 });
            Add(_myCoins[1] = new Label("0", false, 0x0481, 9) { X = 180, Y = 67 });
            Add(_hisCoins[0] = new Label("0", false, 0x0481, 9) { X = 180, Y = 190 });
            Add(_hisCoins[1] = new Label("0", false, 0x0481, 9) { X = 180, Y = 210 });
            Add(_myCoinsEntries[0] = new CoinBox { X = 43, Y = 190 });
            Add(_myCoinsEntries[1] = new CoinBox { X = 43, Y = 210 });
            _myX = 30;
            _myY = 110;
            _opX = 192;
            _opY = 110;
        }
        else {
            _myX = 45;
            _myY = 70;
            _opX = 192;
            _opY = 70;
        }
        _pad.X = _myX;
        _pad.Y = _myY;
        _pad.Width = BoxW;
        _pad.Height = BoxH;
        Add(_pad);
        SetCheckboxes();
        InvalidateContents = true;
    }

    void SetCheckboxes() {
        if (_myCheckbox != null) {
            _myCheckbox.ValueChanged -= OnMineChanged;
            _myCheckbox.Dispose();
            _myCheckbox = null;
        }
        if (_secure) {
            _hisX = 258;
            _hisY = 240;
        }
        else {
            _hisX = 266;
            _hisY = 160;
        }
        _hisGraphic = _heIsAccepting ? (ushort)0x0869 : (ushort)0x0867;
        var box = _imAccepting
            ? new Checkbox(0x0869, 0x086A) { X = _secure ? 37 : 52, Y = 29 }
            : new Checkbox(0x0867, 0x0868) { X = _secure ? 37 : 52, Y = 29 };
        box.ValueChanged += OnMineChanged;
        _myCheckbox = box;
        Add(box);
    }

    void OnMineChanged(object sender, EventArgs e) {
        ImAccepting = !ImAccepting;
        UOLoginNet.Send(OutgoingPackets.Send_TradeResponse(ID1, 2, ImAccepting));
    }

    void PollCoins() {
        if (!_secure) return;
        var send = false;
        send |= Apply(_myCoinsEntries[0], true);
        send |= Apply(_myCoinsEntries[1], false);
        if (send) UOLoginNet.Send(OutgoingPackets.Send_TradeUpdateGold(ID1, _myGoldEntry, _myPlatEntry));
    }

    /// <summary>1 platinum is 1,000,000,000 gold. The offer stops at the balance minus the other field.</summary>
    bool Apply(TextBox entry, bool gold) {
        if (entry == null) return false;
        var text = entry.Text ?? "";
        var send = false;
        uint value;
        if (text.Length == 0) {
            entry.Text = "0";
            value = 0;
        }
        else if (!uint.TryParse(text.Replace(",", ""), out value)) return false;
        var wealth = _gold + (ulong)_platinum * PlatinumValue;
        if (gold) {
            var plat = (ulong)_myPlatEntry * PlatinumValue;
            var remaining = wealth > plat ? wealth - plat : 0;
            var max = remaining >= uint.MaxValue ? uint.MaxValue : (uint)remaining;
            if (value > max) {
                value = max;
                send = true;
            }
            if (_myGoldEntry != value) send = true;
            _myGoldEntry = value;
        }
        else {
            var remaining = wealth > _myGoldEntry ? wealth - _myGoldEntry : 0;
            var max = remaining / PlatinumValue;
            if (max > uint.MaxValue) max = uint.MaxValue;
            if (value > max) {
                value = (uint)max;
                send = true;
            }
            if (_myPlatEntry != value) send = true;
            _myPlatEntry = value;
        }
        if (send) entry.Text = value.ToString("N0");
        return send;
    }

    void Fill(uint container, int ox, int oy) {
        UOLoginNet.CopyContents(container, _rows);
        for (var i = 0; i < _rows.Count; i++) {
            var it = _rows[i];
            if (it.Amount <= 0 || it.Serial == 0) continue;
            var graphic = (ushort)(it.Graphic & 0x7FFF);
            var shown = Shown(graphic, it.Amount);
            var ix = (int)(short)it.X;
            var iy = (int)(short)it.Y;
            if (!ArtOf(shown, it.Hue, out var key, out var pix, out var w, out var h)) continue;
            Place(ref ix, ref iy, w, h);
            _art.Add(new Stamp { Key = key, X = ox + ix, Y = oy + iy, W = w, H = h, Px = pix });
            var hitW = w;
            var hitH = h;
            if (it.Amount > 1 && shown == graphic) {
                _art.Add(new Stamp { Key = key, X = ox + ix + 5, Y = oy + iy + 5, W = w, H = h, Px = pix });
                hitW += 5;
                hitH += 5;
            }
            var hit = new UseLabel("", 0) {
                Serial = it.Serial,
                X = ox + ix,
                Y = oy + iy,
                Width = hitW,
                Height = hitH,
                WantUpdateSize = false,
            };
            Add(hit);
            _hits.Add(hit);
        }
    }

    /// <summary>UIManager drops a drag on ContainerGump only. This runs when a hold is already up.</summary>
    void DropInMine(int screenX, int screenY) {
        if (!UOLoginNet.Holding || IsDisposed) return;
        var x = screenX - X - _myX;
        var y = screenY - Y - _myY;
        if (TryHeldGraphic(out var graphic, out var hue)) {
            var shown = Shown(graphic, UOLoginNet.AmountOf(UOLoginNet.HeldSerial));
            if (ArtOf(shown, hue, out _, out _, out var w, out var h)) {
                x -= w >> 1;
                y -= h >> 1;
                Place(ref x, ref y, w, h);
            }
            else Place(ref x, ref y, 0, 0);
        }
        else Place(ref x, ref y, 0, 0);
        UOLoginNet.DropHeld(x, y, 0, ID1);
    }

    bool TryHeldGraphic(out ushort graphic, out ushort hue) {
        graphic = 0;
        hue = 0;
        var serial = UOLoginNet.HeldSerial;
        if (serial == 0) return false;
        if (In(ID1, serial, out graphic, out hue) || In(ID2, serial, out graphic, out hue)) return true;
        var pack = UOLoginNet.PlayerBackpack;
        if (pack != 0 && In(pack, serial, out graphic, out hue)) return true;
        UOLoginNet.CopyEquip(UOLoginNet.PlayerSerial, _equip);
        for (var i = 0; i < _equip.Count; i++) {
            if (_equip[i].Serial != serial) continue;
            graphic = (ushort)(_equip[i].Graphic & 0x7FFF);
            hue = _equip[i].Hue;
            return true;
        }
        return false;
    }

    bool In(uint container, uint serial, out ushort graphic, out ushort hue) {
        graphic = 0;
        hue = 0;
        UOLoginNet.CopyContents(container, _rows);
        for (var i = 0; i < _rows.Count; i++) {
            if (_rows[i].Serial != serial) continue;
            graphic = (ushort)(_rows[i].Graphic & 0x7FFF);
            hue = _rows[i].Hue;
            return true;
        }
        return false;
    }

    static void Place(ref int x, ref int y, int w, int h) {
        if (w > 0 && x + w > BoxW) x = BoxW - w;
        if (h > 0 && y + h > BoxH) y = BoxH - h;
        if (x < 0) x = 0;
        if (y < 0) y = 0;
    }

    static ushort Shown(ushort graphic, ushort amount) {
        if (amount <= 1 || (graphic != 0x0EEA && graphic != 0x0EED && graphic != 0x0EF0)) return graphic;
        return (ushort)(graphic + (amount > 5 ? 2 : 1));
    }

    static bool ArtOf(ushort graphic, ushort hue, out int key, out byte[] pixels, out int w, out int h) {
        key = 0;
        pixels = null;
        w = h = 0;
        var assets = UltimaOnline.Current?.Assets;
        if (assets == null || graphic == 0) return false;
        var file = graphic + 0x4000;
        if (file > 0xFFFF || !assets.TryArtPixels(file, out var src, out w, out h)) return false;
        if (src == null || w <= 0 || h <= 0 || src.Length < w * h * 2) return false;
        var index = hue & 0x3FFF;
        var onlyGray = (hue & 0x8000) != 0;
        if (index != 0 && assets.TryHueRamp(index, out var ramp)) {
            var id = (long)graphic | ((long)index << 16) | (onlyGray ? 1L << 32 : 0);
            if (!Hued.TryGetValue(id, out pixels)) Hued[id] = pixels = Tint(src, w, h, ramp, onlyGray);
            key = ArtKey(id);
            return pixels != null;
        }
        pixels = src;
        key = ArtKey(graphic);
        return true;
    }

    static int ArtKey(long id) {
        if (!Slots.TryGetValue(id, out var n)) {
            n = Slots.Count;
            Slots[id] = n;
        }
        return ArtBase + n;
    }

    /// <summary>Same remap as container art. 0x8000 recolors a gray pixel only.</summary>
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

    /// <summary>Font 9. TextBox draws font 1 and has no TextChanged.</summary>
    sealed class CoinBox : TextBox {
        public CoinBox() {
            Hue = 0x0481;
            MaxLength = -1;
            Width = 100;
            Height = 20;
            Text = "0";
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            GumpArt.Sink?.DrawText(x, y, Text, Hue, 9);
            return true;
        }
    }

    sealed class DropPad : Control {
        readonly TradingGump _owner;

        public DropPad(TradingGump owner) {
            _owner = owner;
            AcceptMouseInput = true;
            CanMove = true;
            WantUpdateSize = false;
        }

        public override void OnMouseClick(int x, int y) => _owner.DropInMine(x, y);
    }
}
