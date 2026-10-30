// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO CounterBarGump. The slot list lives on this gump. No profile and no settings file.
/// Border is 0x0A8C and 0x0A8D at 4 pixels. The 0x0837 grip is drawn and does not resize.
/// Cell size is the profile default 40. An empty bar is six cells wide and shows the drop help.
/// Count walks layers 1 through 0x18 (ClassicUO OneHanded through Legs). A non-empty container,
/// and the backpack, are CopyContents trees. AmountOf supplies each row. A worn non-container counts as 1.
/// A click while Holding, whose container row is still public, records that graphic and DropHeld puts it back.
/// Double-click calls Use on the matching serial. A click with nothing held does nothing. There is no drag-off.
/// BGRA1555 has no partial alpha, so the 0.7 black fill is an opaque dark pixel. Right-click closes.
/// </summary>
public sealed class CounterBarGump : Gump {
    const int Cell = 40;
    const int Edge = 4;
    const int Pad = 2;
    const int Columns = 6;
    const ushort HBorder = 0x0A8C;
    const ushort VBorder = 0x0A8D;
    const ushort HelpHue = 0x32;
    const int FillKey = 0x57200011;
    const int GrayKey = 0x57200012;
    const int YellowKey = 0x57200013;
    const int ArtBase = 0x57300000;

    // r = g = b = 4, opaque. Stand-in for AlphaBlendControl(0.7).
    static readonly byte[] Dark = { 0x84, 0x90 };
    static readonly byte[] GrayPx = { 0x10, 0xC2 };
    static readonly byte[] YellowPx = { 0xE0, 0xFF };

    static readonly List<UOEquipState> Equip = new List<UOEquipState>();
    static readonly List<UOContainedState> Rows = new List<UOContainedState>();
    static readonly List<uint> Stack = new List<uint>();
    static readonly HashSet<uint> Seen = new HashSet<uint>();
    static readonly List<SeenItem> Bag = new List<SeenItem>();
    static readonly Dictionary<long, int> Totals = new Dictionary<long, int>();
    static readonly Dictionary<long, int> ArtSlots = new Dictionary<long, int>();
    static readonly Dictionary<long, byte[]> Hued = new Dictionary<long, byte[]>();
    static int BuiltRev = -2;

    readonly List<Slot> _slots = new List<Slot>();
    readonly Label _help1;
    readonly Label _help2;
    readonly Button _grip;

    public CounterBarGump(World world) : base(world, 0, 0) {
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        WantUpdateSize = false;
        X = 200;
        Y = 200;
        Width = Columns * Cell + Edge * 2;
        Height = Cell + Edge * 2;
        _help1 = new Label("Drop an item here to", true, HelpHue, 1) { X = Edge * 2, Y = Edge * 2 };
        _help2 = new Label("start your first counter", true, HelpHue, 1) { X = Edge * 2, Y = Edge * 2 + 16 };
        Add(_help1);
        Add(_help2);
        var grip = new Button(0, 0x0837, 0x0838, 0x0838) { AcceptMouseInput = false, CanMove = false };
        if (grip.IsDisposed) _grip = null;
        else {
            _grip = grip;
            Add(_grip);
        }
    }

    /// <summary>One gump. A second call disposes it and does not add another. World may be null.</summary>
    public static void Toggle(World world) {
        var open = UIManager.GetGump<CounterBarGump>();
        if (open != null) {
            open.Dispose();
            return;
        }
        UIManager.Add(new CounterBarGump(world));
    }

    public override void Update() {
        Layout();
        base.Update();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        var sink = GumpArt.Sink;
        var innerW = Width - Edge * 2;
        var innerH = Height - Edge * 2;
        if (sink != null && innerW > 0 && innerH > 0)
            sink.DrawRaw(FillKey, x + Edge, y + Edge, innerW, innerH, Dark, 1, 1);
        GumpArtDraw.Pic(HBorder, x, y, Width, Edge, true);
        GumpArtDraw.Pic(HBorder, x, y + Height - Edge, Width, Edge, true);
        GumpArtDraw.Pic(VBorder, x, y, Edge, Height, true);
        GumpArtDraw.Pic(VBorder, x + Width - Edge, y, Edge, Height, true);
        if (sink != null) {
            for (var i = 0; i < _slots.Count; i++) {
                var s = _slots[i];
                if (s.IsDisposed || !s.IsVisible) continue;
                var sx = x + s.X;
                var sy = y + s.Y;
                var hot = UIManager.MouseOverControl == s;
                Frame(sink, hot ? YellowKey : GrayKey, hot ? YellowPx : GrayPx, sx, sy, s.Width, s.Height);
                if (s.Graphic != 0 && ArtOf(s.Graphic, s.Hue ?? 0, out var key, out var px, out var w, out var h))
                    Blit(sink, key, px, w, h, sx, sy, s.Width, s.Height);
            }
        }
        return base.Draw(x, y);
    }

    public override void OnMouseClick(int x, int y) {
        if (!UOLoginNet.Holding || !TryHeld(out var graphic, out var hue, out var px, out var py, out var container)) return;
        if (graphic != 0) AddSlot(graphic, hue);
        UOLoginNet.DropHeld(px, py, 0, container);
    }

    void AddSlot(ushort graphic, ushort hue) {
        var slot = new Slot(graphic, hue);
        _slots.Add(slot);
        Add(slot);
        Layout();
    }

    void Layout() {
        var inner = Width - Edge * 2;
        var cols = inner / Cell;
        if (cols < 1) cols = 1;
        var rows = _slots.Count == 0 ? 1 : (_slots.Count + cols - 1) / cols;
        Height = rows * Cell + Edge * 2;
        var x = 0;
        var y = 0;
        for (var i = 0; i < _slots.Count; i++) {
            var s = _slots[i];
            s.X = Edge + x + Pad;
            s.Y = Edge + y + Pad;
            s.Width = Cell - Pad * 2;
            s.Height = Cell - Pad * 2;
            x += Cell;
            if (x + Cell > inner) {
                x = 0;
                y += Cell;
            }
        }
        var empty = _slots.Count == 0;
        _help1.IsVisible = empty;
        _help2.IsVisible = empty;
        if (_grip != null && !_grip.IsDisposed) {
            _grip.X = Width - _grip.Width + 2;
            _grip.Y = Height - _grip.Height + 2;
        }
    }

    static void Frame(IGumpArt sink, int key, byte[] px, int x, int y, int w, int h) {
        if (w < 1 || h < 1) return;
        sink.DrawRaw(key, x, y, w, 1, px, 1, 1);
        if (h > 1) sink.DrawRaw(key, x, y + h - 1, w, 1, px, 1, 1);
        sink.DrawRaw(key, x, y, 1, h, px, 1, 1);
        if (w > 1) sink.DrawRaw(key, x + w - 1, y, 1, h, px, 1, 1);
    }

    static void Blit(IGumpArt sink, int key, byte[] px, int w, int h, int x, int y, int cw, int ch) {
        var dw = w;
        var dh = h;
        if (dw > cw && dw > 0) {
            dh = dh * cw / dw;
            dw = cw;
        }
        if (dh > ch && dh > 0) {
            dw = dw * ch / dh;
            dh = ch;
        }
        if (dw < 1) dw = 1;
        if (dh < 1) dh = 1;
        sink.DrawRaw(key, x + (cw - dw) / 2, y + (ch - dh) / 2, dw, dh, px, w, h);
    }

    static int Total(ushort graphic, ushort? hue) {
        Ensure();
        var key = ((long)graphic << 17) | (hue == null ? 0x10000L : hue.Value);
        if (Totals.TryGetValue(key, out var n)) return n;
        n = 0;
        for (var i = 0; i < Bag.Count; i++) {
            var it = Bag[i];
            if (it.Graphic != graphic) continue;
            if (hue != null && it.Hue != hue.Value) continue;
            n += it.Amount;
        }
        Totals[key] = n;
        return n;
    }

    static uint FindSerial(ushort graphic, ushort hue) {
        Ensure();
        uint found = 0;
        var best = int.MaxValue;
        var any = hue == 0xFFFF;
        for (var i = 0; i < Bag.Count; i++) {
            var it = Bag[i];
            if (it.Serial == 0 || it.Graphic != graphic) continue;
            if (any) {
                if (it.Hue < best) {
                    best = it.Hue;
                    found = it.Serial;
                }
            }
            else if (it.Hue == hue) found = it.Serial;
        }
        return found;
    }

    static bool TryHeld(out ushort graphic, out ushort hue, out int x, out int y, out uint container) {
        graphic = 0;
        hue = 0;
        x = 0;
        y = 0;
        container = 0;
        var serial = UOLoginNet.HeldSerial;
        if (serial == 0) return false;
        Ensure();
        for (var i = 0; i < Bag.Count; i++) {
            var it = Bag[i];
            if (it.Serial != serial || it.Container == 0) continue;
            graphic = it.Graphic;
            hue = it.Hue;
            x = it.X;
            y = it.Y;
            container = it.Container;
            return true;
        }
        return false;
    }

    /// <summary>Layers 1..0x18. Backpack is 0x15. Shop and mount layers are above Legs and stay out.</summary>
    static void Ensure() {
        var rev = UOLoginNet.WorldRevision;
        if (rev == BuiltRev) return;
        BuiltRev = rev;
        Bag.Clear();
        Totals.Clear();
        var pack = UOLoginNet.PlayerBackpack;
        var sawPack = pack == 0;
        UOLoginNet.CopyEquip(UOLoginNet.PlayerSerial, Equip);
        var equip = Equip.ToArray();
        for (var i = 0; i < equip.Length; i++) {
            var e = equip[i];
            if (e.Layer < 1 || e.Layer > 0x18) continue;
            if (e.Serial == pack) sawPack = true;
            UOLoginNet.CopyContents(e.Serial, Rows);
            if (Rows.Count == 0 && e.Layer != 0x15) {
                Bag.Add(new SeenItem {
                    Serial = e.Serial,
                    Graphic = e.Graphic,
                    Hue = e.Hue,
                    Amount = 1,
                });
                continue;
            }
            Collect(e.Serial);
        }
        if (!sawPack) Collect(pack);
    }

    static void Collect(uint root) {
        Seen.Clear();
        Stack.Clear();
        if (root == 0 || !Seen.Add(root)) return;
        Stack.Add(root);
        while (Stack.Count > 0) {
            var c = Stack[Stack.Count - 1];
            Stack.RemoveAt(Stack.Count - 1);
            UOLoginNet.CopyContents(c, Rows);
            var batch = Rows.ToArray();
            for (var i = 0; i < batch.Length; i++) {
                var it = batch[i];
                Bag.Add(new SeenItem {
                    Serial = it.Serial,
                    Graphic = it.Graphic,
                    Hue = it.Hue,
                    Amount = UOLoginNet.AmountOf(it.Serial),
                    X = it.X,
                    Y = it.Y,
                    Container = it.Container,
                });
                if (it.Serial != 0 && Seen.Add(it.Serial)) Stack.Add(it.Serial);
            }
        }
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
        if (!ArtSlots.TryGetValue(id, out var n)) {
            n = ArtSlots.Count;
            ArtSlots[id] = n;
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
                    c = ramp[r > g ? (r > b ? r : b) : (g > b ? g : b)];
                    if (c != 0) c |= 0x8000;
                }
            }
            dest[i * 2] = (byte)c;
            dest[i * 2 + 1] = (byte)(c >> 8);
        }
        return dest;
    }

    struct SeenItem {
        public uint Serial;
        public ushort Graphic;
        public ushort Hue;
        public int Amount;
        public int X;
        public int Y;
        public uint Container;
    }

    sealed class Slot : Control {
        readonly Label _count;

        public Slot(ushort graphic, ushort hue) {
            Graphic = graphic;
            Hue = hue;
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
            _count = new Label("", true, 0x35, 1) { X = 2, AcceptMouseInput = false };
            Add(_count);
        }

        public ushort Graphic { get; private set; }
        public ushort? Hue { get; private set; }

        public override void Update() {
            var text = "";
            if (Graphic != 0) {
                var n = Total(Graphic, Hue);
                if (n != 1) text = n.ToString();
            }
            if (_count.Text != text) _count.Text = text;
            _count.X = 2;
            _count.Y = Height > 15 ? Height - 15 : 0;
            base.Update();
        }

        public override void OnMouseClick(int x, int y) {
            if (!UOLoginNet.Holding || !TryHeld(out var graphic, out var hue, out var px, out var py, out var container)) return;
            if (graphic != 0) {
                Graphic = graphic;
                Hue = hue;
            }
            UOLoginNet.DropHeld(px, py, 0, container);
        }

        public override bool OnMouseDoubleClick(int x, int y) {
            if (Graphic != 0) {
                var serial = FindSerial(Graphic, Hue ?? 0xFFFF);
                if (serial != 0) UOLoginNet.Use(serial);
            }
            return true;
        }
    }
}
