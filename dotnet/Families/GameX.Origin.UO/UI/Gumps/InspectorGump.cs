// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO InspectorGump. Rows are the fields this client stores: world actors, worn items,
/// the player backpack, status, and the property text. Land, static, and multi are not stored.
/// PriorityZ, AlphaHue, and AllowedToDraw are not on these records. Dump writes a trace, not a file.
/// SDL clipboard is not used. No profile file.
/// </summary>
public sealed class InspectorGump : Gump {
    const int WidthPx = 500;
    const int MinHeight = 400;

    readonly object _obj;
    readonly List<UOMobileState> _mobiles = [];
    readonly List<UOItemState> _items = [];
    readonly List<UOEquipState> _equip = [];
    readonly List<UOContainedState> _contents = [];

    public InspectorGump(World world, object obj) : base(world, 0, 0) {
        _obj = obj;
        X = 200;
        Y = 100;
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        WantUpdateSize = false;
        Width = WidthPx;
        Height = MinHeight;
        Build();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        GumpArtDraw.Pic(0x0A40, x + 4, y + 4, Width - 8, Height - 8, true);
        GumpArtDraw.Pic(0x0A8C, x, y, Width, 4, true);
        GumpArtDraw.Pic(0x0A8C, x, y + Height - 4, Width, 4, true);
        GumpArtDraw.Pic(0x0A8D, x, y, 4, Height, true);
        GumpArtDraw.Pic(0x0A8D, x + Width - 4, y, 4, Height, true);
        return base.Draw(x, y);
    }

    void Build() {
        var rows = new List<(string Key, string Value)>();
        Read(_obj, rows);
        rows.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));
        Add(new Label("Object Information", true, 1153, 3) { X = 20, Y = 10 });
        Add(new DumpHit(this) { X = WidthPx - 115, Y = 5 });
        var y = 35;
        for (var i = 0; i < rows.Count; i++) {
            var row = new Row(rows[i].Key, rows[i].Value) { X = 25, Y = y };
            Add(row);
            y += row.Height;
        }
        Height = y + 16;
        if (Height < MinHeight) Height = MinHeight;
    }

    void Dump() {
        // dump_gameobject.txt is not written.
        var rows = new List<(string Key, string Value)>();
        Read(_obj, rows);
        rows.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));
        Trace.WriteLine("###################################################");
        Trace.WriteLine("OBJECT TYPE: " + (_obj == null ? "null" : _obj.GetType().ToString()));
        for (var i = 0; i < rows.Count; i++)
            Trace.WriteLine(rows[i].Key + " = " + rows[i].Value);
        Trace.WriteLine("###################################################");
    }

    void Read(object obj, List<(string Key, string Value)> rows) {
        if (obj == null) {
            Put(rows, "Object", "null");
            return;
        }
        var seen = new Seen();
        switch (obj) {
            case Mobile mobile:
                seen.Mobile = true;
                seen.HasSerial = true;
                seen.Serial = mobile.Serial;
                seen.HasGraphic = true;
                seen.Graphic = mobile.Body;
                seen.HasPos = true;
                seen.X = mobile.X;
                seen.Y = mobile.Y;
                seen.Z = mobile.Z;
                seen.HasDirection = true;
                seen.Direction = mobile.Direction;
                break;
            case Item item:
                seen.HasSerial = true;
                seen.Serial = item.Serial;
                seen.HasGraphic = true;
                seen.Graphic = item.Graphic;
                seen.HasPos = true;
                seen.X = item.X;
                seen.Y = item.Y;
                seen.Z = item.Z;
                break;
            case UOMobileState mob:
                seen.Mobile = true;
                seen.HasSerial = true;
                seen.Serial = mob.Serial;
                seen.HasGraphic = true;
                seen.Graphic = mob.Body;
                seen.HasPos = true;
                seen.X = mob.X;
                seen.Y = mob.Y;
                seen.Z = mob.Z;
                seen.HasDirection = true;
                seen.Direction = mob.Direction;
                seen.HasFlags = true;
                seen.Flags = mob.Flags;
                seen.HasNoto = true;
                seen.Noto = mob.Notoriety;
                break;
            case UOItemState it:
                seen.HasSerial = true;
                seen.Serial = it.Serial;
                seen.HasGraphic = true;
                seen.Graphic = it.Graphic;
                seen.HasPos = true;
                seen.X = it.X;
                seen.Y = it.Y;
                seen.Z = it.Z;
                break;
            case UOContainedState contained:
                seen.HasSerial = true;
                seen.Serial = contained.Serial;
                seen.HasGraphic = true;
                seen.Graphic = contained.Graphic;
                seen.HasHue = true;
                seen.Hue = contained.Hue;
                seen.HasAmount = true;
                seen.Amount = contained.Amount;
                seen.HasContainer = true;
                seen.Container = contained.Container;
                seen.HasPrice = true;
                seen.Price = contained.Price;
                seen.HasGrid = true;
                seen.GridX = contained.X;
                seen.GridY = contained.Y;
                break;
            case UOEquipState equip:
                seen.HasSerial = true;
                seen.Serial = equip.Serial;
                seen.HasGraphic = true;
                seen.Graphic = equip.Graphic;
                seen.HasHue = true;
                seen.Hue = equip.Hue;
                seen.HasLayer = true;
                seen.Layer = equip.Layer;
                seen.HasContainer = true;
                seen.Container = equip.Parent;
                break;
            case UseLabel label:
                seen.HasSerial = true;
                seen.Serial = label.Serial;
                break;
            case uint serial:
                seen.HasSerial = true;
                seen.Serial = serial;
                break;
            default:
                Put(rows, "Type", obj.GetType().Name);
                Put(rows, "Value", obj.ToString());
                return;
        }
        if (seen.HasSerial) {
            LocalSerial = seen.Serial;
            Fill(seen);
        }
        Emit(obj, seen, rows);
    }

    void Fill(Seen seen) {
        if (seen.Serial == 0) return;
        UOLoginNet.CopyActors(_mobiles, _items);
        for (var i = 0; i < _mobiles.Count; i++) {
            var mob = _mobiles[i];
            if (mob.Serial != seen.Serial) continue;
            seen.Mobile = true;
            if (!seen.HasGraphic) { seen.HasGraphic = true; seen.Graphic = mob.Body; }
            if (!seen.HasPos) { seen.HasPos = true; seen.X = mob.X; seen.Y = mob.Y; seen.Z = mob.Z; }
            if (!seen.HasDirection) { seen.HasDirection = true; seen.Direction = mob.Direction; }
            if (!seen.HasFlags) { seen.HasFlags = true; seen.Flags = mob.Flags; }
            if (!seen.HasNoto) { seen.HasNoto = true; seen.Noto = mob.Notoriety; }
            break;
        }
        if (!seen.HasGraphic) {
            for (var i = 0; i < _items.Count; i++) {
                var it = _items[i];
                if (it.Serial != seen.Serial) continue;
                seen.HasGraphic = true;
                seen.Graphic = it.Graphic;
                if (!seen.HasPos) { seen.HasPos = true; seen.X = it.X; seen.Y = it.Y; seen.Z = it.Z; }
                break;
            }
        }
        if (World != null && !seen.Mobile && World.Mobiles.TryGetValue(seen.Serial, out var rawMob) && rawMob is Mobile mobile) {
            seen.Mobile = true;
            if (!seen.HasGraphic) { seen.HasGraphic = true; seen.Graphic = mobile.Body; }
            if (!seen.HasPos) { seen.HasPos = true; seen.X = mobile.X; seen.Y = mobile.Y; seen.Z = mobile.Z; }
            if (!seen.HasDirection) { seen.HasDirection = true; seen.Direction = mobile.Direction; }
        }
        if (World != null && !seen.HasGraphic && World.Items.TryGetValue(seen.Serial, out var rawItem) && rawItem is Item item) {
            seen.HasGraphic = true;
            seen.Graphic = item.Graphic;
            if (!seen.HasPos) { seen.HasPos = true; seen.X = item.X; seen.Y = item.Y; seen.Z = item.Z; }
        }
        if (!seen.HasGraphic) {
            for (var i = 0; i < _mobiles.Count; i++) {
                UOLoginNet.CopyEquip(_mobiles[i].Serial, _equip);
                for (var e = 0; e < _equip.Count; e++) {
                    var worn = _equip[e];
                    if (worn.Serial != seen.Serial) continue;
                    seen.HasGraphic = true;
                    seen.Graphic = worn.Graphic;
                    if (!seen.HasHue) { seen.HasHue = true; seen.Hue = worn.Hue; }
                    seen.HasLayer = true;
                    seen.Layer = worn.Layer;
                    seen.HasContainer = true;
                    seen.Container = worn.Parent;
                    break;
                }
                if (seen.HasGraphic) break;
            }
        }
        var pack = UOLoginNet.PlayerBackpack;
        if (!seen.HasGraphic && pack != 0) {
            UOLoginNet.CopyContents(pack, _contents);
            for (var i = 0; i < _contents.Count; i++) {
                var row = _contents[i];
                if (row.Serial != seen.Serial) continue;
                seen.HasGraphic = true;
                seen.Graphic = row.Graphic;
                if (!seen.HasHue) { seen.HasHue = true; seen.Hue = row.Hue; }
                if (!seen.HasAmount) { seen.HasAmount = true; seen.Amount = row.Amount; }
                seen.HasContainer = true;
                seen.Container = row.Container;
                seen.HasPrice = true;
                seen.Price = row.Price;
                seen.HasGrid = true;
                seen.GridX = row.X;
                seen.GridY = row.Y;
                break;
            }
        }
    }

    static void Emit(object obj, Seen seen, List<(string Key, string Value)> rows) {
        Put(rows, "Type", obj.GetType().Name);
        if (seen.HasGraphic) Put(rows, "Graphics", $"0x{seen.Graphic:X4}");
        if (seen.HasHue) Put(rows, "Hue", $"0x{seen.Hue:X4}");
        if (seen.HasPos) Put(rows, "Position", $"X={seen.X}, Y={seen.Y}, Z={seen.Z}");
        if (seen.HasGrid) Put(rows, "Grid", $"X={seen.GridX}, Y={seen.GridY}");
        if (seen.HasDirection) Put(rows, "Direction", Dir(seen.Direction));
        if (!seen.HasSerial) return;
        Put(rows, "Serial", $"0x{seen.Serial:X8}");
        var named = UOLoginNet.NameOf(seen.Serial);
        if (UOLoginNet.TryStatus(seen.Serial, out var st)) {
            if (!string.IsNullOrEmpty(st.Name)) named = st.Name;
            Put(rows, "HP", $"{st.Hits}/{st.HitsMax}");
            if (st.HasStats) {
                Put(rows, "Mana", $"{st.Mana}/{st.ManaMax}");
                Put(rows, "Stamina", $"{st.Stam}/{st.StamMax}");
            }
        }
        if (UOLoginNet.TryProperties(seen.Serial, out var propName, out var data)) {
            if (!string.IsNullOrEmpty(propName) && string.IsNullOrEmpty(named)) named = propName;
            if (!string.IsNullOrEmpty(data))
                Put(rows, "Properties", data.Replace("\r\n", " ").Replace("\r\n", " | "));
        }
        if (!string.IsNullOrEmpty(named)) Put(rows, "Name", named);
        if (seen.Mobile) {
            if (!seen.HasFlags) { seen.HasFlags = true; seen.Flags = UOLoginNet.MobileFlags(seen.Serial); }
            if (!seen.HasNoto) { seen.HasNoto = true; seen.Noto = UOLoginNet.MobileNotoriety(seen.Serial); }
            Put(rows, "IsDead", UOLoginNet.IsDead(seen.Serial).ToString());
            if (seen.Serial == UOLoginNet.PlayerSerial) Put(rows, "SpeedMode", UOLoginNet.SpeedMode.ToString());
        }
        if (seen.HasFlags) Put(rows, "Flags", $"0x{seen.Flags:X2}");
        if (seen.HasNoto) Put(rows, "Notoriety", seen.Noto.ToString());
        if (UOLoginNet.TryHealthBar(seen.Serial, out var poisoned, out var yellow)) {
            Put(rows, "Poisoned", poisoned.ToString());
            Put(rows, "YellowHits", yellow.ToString());
        }
        if (!seen.HasAmount) {
            var amount = UOLoginNet.AmountOf(seen.Serial);
            if (amount != 0) { seen.HasAmount = true; seen.Amount = amount; }
        }
        if (seen.HasAmount) Put(rows, "Amount", seen.Amount.ToString());
        if (seen.HasContainer) Put(rows, "Container", $"0x{seen.Container:X8}");
        if (seen.HasLayer) Put(rows, "Layer", $"0x{seen.Layer:X2}");
        if (seen.HasPrice) Put(rows, "Price", seen.Price.ToString());
        if (seen.HasPos && UOLoginNet.PlayerSerial != 0) {
            var dx = seen.X - UOLoginNet.PlayerX;
            var dy = seen.Y - UOLoginNet.PlayerY;
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;
            Put(rows, "Distance", (dx > dy ? dx : dy).ToString());
        }
    }

    static void Put(List<(string Key, string Value)> rows, string key, string value) {
        for (var i = 0; i < rows.Count; i++)
            if (rows[i].Key == key) {
                rows[i] = (key, value ?? "");
                return;
            }
        rows.Add((key, value ?? ""));
    }

    static string Dir(byte direction) {
        var name = ((Direction)(direction & 7)).ToString();
        if ((direction & 0x80) != 0) return name + " Running";
        return name;
    }

    sealed class Seen {
        public uint Serial, Container, Price;
        public ushort Graphic, Hue, Amount;
        public int X, Y, Z, GridX, GridY;
        public byte Direction, Flags, Noto, Layer;
        public bool HasSerial, HasGraphic, HasPos, HasDirection, HasHue, HasAmount, HasContainer, HasLayer, HasPrice, HasFlags, HasNoto, HasGrid, Mobile;
    }

    sealed class DumpHit : Label {
        readonly InspectorGump _owner;

        public DumpHit(InspectorGump owner) : base("Dump", true, 0xFFFF, 1) {
            _owner = owner;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) => _owner.Dump();
    }

    sealed class Row : Control {
        public Row(string key, string value) {
            AcceptMouseInput = false;
            var caption = new Label((key ?? "") + ":", true, 33, 1);
            Add(caption);
            var h = AddValue(value, 200, 230);
            if (caption.Height > h) h = caption.Height;
            Width = 435;
            Height = h + 4;
        }

        public override bool Draw(int x, int y) {
            if (!base.Draw(x, y)) return false;
            GumpArtDraw.Pic(0x0A8C, x, y + Height - 2, Width, 1, true);
            return true;
        }

        int AddValue(string text, int x, int maxW) {
            var src = text ?? "";
            if (src.Length == 0) {
                Add(new Label("", true, 1153, 1) { X = x, Y = 0 });
                return 16;
            }
            var y = 0;
            var i = 0;
            while (i < src.Length) {
                var n = src.Length - i;
                while (n > 1 && WidthOf(src.Substring(i, n)) > maxW) {
                    var sp = src.LastIndexOf(' ', i + n - 1, n);
                    if (sp > i) n = sp - i;
                    else n--;
                }
                if (n <= 0) n = 1;
                var piece = src.Substring(i, n).Trim();
                if (piece.Length > 0)
                    Add(new Label(piece, true, 1153, 1) { X = x, Y = y });
                y += 16;
                i += n;
                while (i < src.Length && src[i] == ' ') i++;
            }
            return y;
        }

        static int WidthOf(string text) =>
            GumpArt.Sink?.MeasureText(text, 1) ?? (string.IsNullOrEmpty(text) ? 0 : text.Length * 6);
    }
}
