// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Clients.UO;
using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// Container graphic 0x0030. Rows are layers 0x1A then 0x1B.
/// A container whose graphic is not 0x2AF8 is walked last to first.
/// ClassicUO prints "{name} at {price}gp". The price is the 0x74 value stored on the item.
/// A row click calls ReplyShop, the empty 0x3B.
/// </summary>
public sealed class VendorGump : Gump {
    const int Rows = 16;
    readonly BuyRow[] _rows;
    readonly List<UOEquipState> _equip = [];
    readonly List<UOContainedState> _held = [];
    readonly List<Stamp> _art = [];
    int _rev = -1;
    bool _sent;

    struct Stamp {
        public int Key, X, Y, W, H;
        public byte[] Px;
    }

    public VendorGump(uint serial) : base(null, serial, serial) {
        CanCloseWithRightClick = true;
        CanMove = true;
        AcceptMouseInput = true;
        X = 150;
        Y = 5;
        Add(new ResizePic(0x0A28) { X = 0, Y = 0, Width = 260, Height = 420 });
        Add(new Label("buy", false, 0x0386, 2) { X = 16, Y = 16 });
        _rows = new BuyRow[Rows];
        for (var i = 0; i < Rows; i++) {
            _rows[i] = new BuyRow(this) { X = 70, Y = 48 + i * 22, IsVisible = false };
            Add(_rows[i]);
        }
    }

    public override void Update() {
        var rev = UOLoginNet.WorldRevision;
        if (rev != _rev) {
            _rev = rev;
            Fill();
        }
        base.Update();
    }

    public override bool Draw(int x, int y) {
        var sink = GumpArt.Sink;
        if (sink != null)
            foreach (var s in _art)
                sink.DrawRaw(s.Key, x + s.X, y + s.Y, s.W > 22 ? 22 : s.W, s.H > 22 ? 22 : s.H, s.Px, s.W, s.H);
        return base.Draw(x, y);
    }

    void Fill() {
        _art.Clear();
        var n = 0;
        UOLoginNet.CopyEquip(LocalSerial, _equip);
        for (byte layer = 0x1A; layer <= 0x1B && n < Rows; layer++) {
            uint box = 0;
            ushort graphic = 0;
            foreach (var e in _equip)
                if (e.Layer == layer) { box = e.Serial; graphic = e.Graphic; break; }
            if (box == 0) continue;
            UOLoginNet.CopyContents(box, _held);
            if (graphic != 0x2AF8) _held.Reverse();
            foreach (var it in _held) {
                if (n >= Rows) break;
                var name = UOLoginNet.NameOf(it.Serial);
                if (string.IsNullOrEmpty(name)) name = it.Graphic.ToString("X4");
                var row = _rows[n];
                row.Text = $"{Words(name)} at {it.Price}gp  x{it.Amount}";
                row.IsVisible = true;
                var id = (it.Graphic & 0x7FFF) + 0x4000;
                var assets = UltimaOnline.Current?.Assets;
                if (id <= 0xFFFF && assets != null && assets.TryArtPixels(id, out var pix, out var w, out var h))
                    _art.Add(new Stamp { Key = unchecked((int)(0x53000000 + it.Graphic)), X = 16, Y = 48 + n * 22, W = w, H = h, Px = pix });
                n++;
            }
        }
        for (; n < Rows; n++) _rows[n].IsVisible = false;
    }

    void Buy() {
        if (_sent || IsDisposed) return;
        _sent = true;
        UOLoginNet.ReplyShop(LocalSerial, true);
        Dispose();
    }

    static string Words(string text) {
        if (string.IsNullOrEmpty(text)) return "";
        var chars = text.ToCharArray();
        var cap = true;
        for (var i = 0; i < chars.Length; i++) {
            if (cap) chars[i] = char.ToUpper(chars[i]);
            if (!char.IsWhiteSpace(chars[i]))
                cap = i + 1 < chars.Length && char.IsWhiteSpace(chars[i + 1]);
        }
        return new string(chars);
    }

    sealed class BuyRow : Label {
        readonly VendorGump _owner;

        public BuyRow(VendorGump owner) : base("", false, 0x0481, 5) {
            _owner = owner;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) => _owner.Buy();

        public override bool OnMouseDoubleClick(int x, int y) => true;
    }
}
