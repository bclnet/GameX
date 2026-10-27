// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// Opened by 0x88. ClassicUO 7.0.113 paperdoll: background 0x07D0 or 0x07D1, the six side slots, and worn art.
/// Client 7.0 uses the quests button, not journal. Flag 0x02 is the lift bit and is shown in the title line.
/// Double-click sends 0x06. Right-click closes.
/// </summary>
public sealed class PaperdollGump : Gump {
    // PaperDollInteractable._layerOrder. A container cloak uses LayerOrderQuiver.
    static readonly byte[] WearOrder = [
        0x14, 0x05, 0x04, 0x03, 0x18, 0x13, 0x0D, 0x11, 0x08, 0x0E, 0x0F, 0x07, 0x17, 0x16, 0x0C, 0x0A, 0x0B, 0x10, 0x12, 0x06, 0x01, 0x02, 0x09,
    ];
    static readonly byte[] LayerOrderQuiver = [
        0x05, 0x04, 0x03, 0x18, 0x13, 0x0D, 0x11, 0x08, 0x0E, 0x0F, 0x07, 0x17, 0x16, 0x14, 0x0C, 0x0A, 0x0B, 0x10, 0x12, 0x06, 0x01, 0x02, 0x09,
    ];
    // EquipmentSlot order: helmet, earrings, necklace, ring, bracelet, tunic. Y is 75 + 21 * index.
    static readonly byte[] SlotLayers = [0x06, 0x12, 0x0A, 0x08, 0x0E, 0x11];
    static readonly ushort[] PeaceMode = [0x07E5, 0x07E6, 0x07E7];
    static readonly ushort[] WarMode = [0x07E8, 0x07E9, 0x07EA];
    static readonly (ushort id, string text)[] EmptyEntries = [];

    readonly bool _player;
    readonly ushort _baseGraphic;
    readonly Label _title;
    readonly DollHost _doll;
    readonly EquipSlot[] _slots;
    readonly List<UOEquipState> _equip = [];
    GumpPic _pic;
    Button _warBtn;
    string _titleText;
    byte _flags;
    bool _minimized, _war;
    int _rev = -1;

    enum Buttons {
        Help,
        Options,
        LogOut,
        Journal,
        Quests,
        Skills,
        Guild,
        PeaceWarToggle,
        Status,
    }

    public PaperdollGump(World world, uint serial, string title, byte flags) : base(world, serial, serial) {
        CanCloseWithRightClick = true;
        CanMove = true;
        AcceptMouseInput = true;
        X = 80;
        Y = 40;
        _player = serial == UOLoginNet.PlayerSerial;
        _baseGraphic = _player ? (ushort)0x07D0 : (ushort)0x07D1;
        _titleText = title;
        _flags = flags;
        // Chrome is added after the doll so the button rectangles stay above the body gump.
        _pic = _player ? new BasePic(this, _baseGraphic) : new GumpPic(0, 0, _baseGraphic, 0);
        Add(_pic);
        _doll = new DollHost();
        Add(_doll);
        _slots = new EquipSlot[SlotLayers.Length];
        for (var i = 0; i < SlotLayers.Length; i++) {
            _slots[i] = new EquipSlot(SlotLayers[i], 75 + 21 * i);
            Add(_slots[i]);
        }
        var books = _player && (UOLoginNet.Features & 0x20) != 0;
        if (_player) {
            AddButton(Buttons.Help, 0x07EF, 0x07F0, 0x07F1, 0);
            AddButton(Buttons.Options, 0x07D6, 0x07D7, 0x07D8, 1);
            AddButton(Buttons.LogOut, 0x07D9, 0x07DA, 0x07DB, 2);
            // 7.0.113 is past CV_500A, so this slot is quests, not journal 0x07DC.
            AddButton(Buttons.Quests, 0x57B5, 0x57B7, 0x57B6, 3);
            AddButton(Buttons.Skills, 0x07DF, 0x07E0, 0x07E1, 4);
            AddButton(Buttons.Guild, 0x57B2, 0x57B4, 0x57B3, 5);
            _war = UOLoginNet.InWarMode;
            var war = _war ? WarMode : PeaceMode;
            _warBtn = AddButton(Buttons.PeaceWarToggle, war[0], war[1], war[2], 6);
            var profileX = 25;
            if (books) profileX += 14;
            Add(new GumpPic(profileX, 196, 0x07D2, 0));
            profileX += 14;
            Add(new GumpPic(profileX, 196, 0x07D2, 0));
            Add(new MinBox(this));
        }
        else Add(new GumpPic(25, 196, 0x07D2, 0));
        AddButton(Buttons.Status, 0x07EB, 0x07EC, 0x07ED, 7);
        Add(new VirtuePic(serial));
        if (books) {
            Add(new GumpPic(156, 200, 0x2B34, 0));
            Add(new GumpPic(23, 200, 0x2B28, 0));
        }
        _title = new Label(Line(_titleText, _flags), false, 0x0386, 1) { X = 39, Y = 262 };
        Add(_title);
        Refresh();
        _rev = UOLoginNet.WorldRevision;
    }

    public string Title {
        get => _titleText;
        set {
            _titleText = value;
            if (_title != null) _title.Text = Line(value, _flags);
        }
    }

    public byte Flags {
        get => _flags;
        set {
            _flags = value;
            if (_title != null) _title.Text = Line(_titleText, value);
        }
    }

    public bool CanLift => (_flags & 0x02) != 0;

    public bool IsMinimized {
        get => _minimized;
        set {
            if (_minimized == value || _pic == null || _pic.IsDisposed) return;
            _minimized = value;
            var graphic = value ? (ushort)0x07EE : _baseGraphic;
            if (_pic.Graphic != graphic) _pic.Graphic = graphic;
            if (_pic.IsDisposed) return;
            foreach (var c in Children) c.IsVisible = !value;
            _pic.IsVisible = true;
            WantUpdateSize = true;
        }
    }

    public override void Update() {
        var rev = UOLoginNet.WorldRevision;
        if (rev != _rev) {
            _rev = rev;
            _title.Text = Line(_titleText, _flags);
            Refresh();
        }
        if (_player && _warBtn != null && !_warBtn.IsDisposed && _war != UOLoginNet.InWarMode) {
            _war = UOLoginNet.InWarMode;
            _warBtn.ButtonGraphicNormal = (_war ? WarMode : PeaceMode)[0];
        }
        base.Update();
    }

    public override void OnButtonClick(int buttonID) {
        switch ((Buttons)buttonID) {
            case Buttons.Skills:
                if (UIManager.GetGump<SkillsGump>() == null) UIManager.Add(new SkillsGump(World));
                break;
            case Buttons.Status:
                if (UIManager.GetGump<StatusGump>() == null) UIManager.Add(new StatusGump(World));
                break;
            case Buttons.PeaceWarToggle:
                UOLoginNet.ToggleWar();
                break;
        }
    }

    Button AddButton(Buttons id, ushort normal, ushort pressed, ushort over, int index) {
        var b = new Button((int)id, normal, pressed, over) {
            X = 185,
            Y = 44 + 27 * index,
            ButtonAction = ButtonAction.Activate,
        };
        return Add(b);
    }

    void Refresh() {
        UOLoginNet.CopyEquip(ServerSerial, _equip);
        for (var i = 0; i < _slots.Length; i++) _slots[i].Show(Find(_equip, _slots[i].Layer));
        var kids = _doll.Children;
        for (var i = kids.Count - 1; i >= 0; i--) {
            var c = kids[i];
            c.Parent = null;
            c.Dispose();
        }
        var mobile = MobileBody();
        var body = DollGump(mobile, out var hue, out var extra);
        var pic = new GumpPic(0, 0, body, hue) { IsPartialHue = true, AcceptMouseInput = false, CanMove = false };
        _doll.Add(pic);
        if (extra != 0) {
            // Mobile hue is not stored. The 0x03DB overlay stays unhued; the body uses 0x03EA.
            _doll.Add(new GumpPic(0, 0, extra, 0) { IsPartialHue = true, AcceptMouseInput = true, CanMove = false });
        }
        var swap = false;
        var arms = Find(_equip, 0x13);
        if (arms.Serial != 0 && (arms.Graphic == 0x1410 || arms.Graphic == 0x1417)) swap = true;
        // A container cloak uses LayerOrderQuiver. Tiledata IsContainer is not on Assets.
        var layers = DrawOrder(false);
        for (var i = 0; i < layers.Length; i++) {
            var layer = layers[i];
            if (swap) {
                if (layer == 0x13) layer = 0x0D;
                else if (layer == 0x0D) layer = 0x13;
            }
            var item = Find(_equip, layer);
            if (item.Serial == 0 || IsCovered(_equip, layer) || IsSlot(layer)) continue;
            if (!TryArt(item.Graphic, out var key, out var px, out var w, out var h)) continue;
            _doll.Add(new Worn(item.Serial, 0, 0, key, px, w, h));
        }
        var pack = Find(_equip, 0x15);
        if (pack.Serial != 0 && TryArt(pack.Graphic, out var pkey, out var ppx, out var pw, out var ph)) {
            var bx = (UOLoginNet.Features & 0x20) != 0 ? -6 : 0;
            _doll.Add(new Worn(pack.Serial, bx, 0, pkey, ppx, pw, ph));
        }
    }

    ushort MobileBody() {
        if (ServerSerial == UOLoginNet.PlayerSerial) return UOLoginNet.PlayerBody;
        if (World != null && World.Mobiles.TryGetValue(ServerSerial, out var obj) && obj is Mobile m) return m.Body;
        return 0;
    }

    // PaperDollInteractable body gump. No IsFemale flag, so an unlisted body is the male gump 0x000C.
    static ushort DollGump(ushort mobile, out ushort hue, out ushort extra) {
        hue = 0;
        extra = 0;
        if (mobile == 0x0191 || mobile == 0x0193) return 0x000D;
        if (mobile == 0x025D) return 0x000E;
        if (mobile == 0x025E) return 0x000F;
        if (mobile == 0x029A || mobile == 0x02B6) return 0x029A;
        if (mobile == 0x029B || mobile == 0x02B7) return 0x0299;
        if (mobile == 0x04E5) return 0xC835;
        if (mobile == 0x03DB) {
            hue = 0x03EA;
            extra = 0xC72B;
            return 0x000C;
        }
        return 0x000C;
    }

    static byte[] DrawOrder(bool quiver) => quiver ? LayerOrderQuiver : WearOrder;

    static bool IsSlot(byte layer) {
        for (var i = 0; i < SlotLayers.Length; i++)
            if (SlotLayers[i] == layer) return true;
        return false;
    }

    static UOEquipState Find(List<UOEquipState> equip, byte layer) {
        for (var i = 0; i < equip.Count; i++)
            if (equip[i].Layer == layer) return equip[i];
        return default;
    }

    static bool TryArt(ushort graphic, out int key, out byte[] px, out int w, out int h) {
        key = w = h = 0;
        px = null;
        var id = (graphic & 0x7FFF) + 0x4000;
        if (id > 0xFFFF) return false;
        var assets = UltimaOnline.Current?.Assets;
        if (assets == null || !assets.TryArtPixels(id, out px, out w, out h)) return false;
        key = unchecked((int)(0x51000000 + graphic));
        return true;
    }

    static void ClickItem(uint serial) {
        if (serial != 0 && UOLoginNet.HasTarget) UOLoginNet.TargetSerial(serial);
    }

    static bool DoubleItem(uint serial) {
        if (serial == 0) return true;
        if (UOLoginNet.HasTarget || UOLoginNet.TargetedJustNow) {
            if (UOLoginNet.HasTarget) UOLoginNet.TargetSerial(serial);
            return true;
        }
        UOLoginNet.Use(serial);
        return true;
    }

    static string Line(string title, byte flags) {
        var text = string.IsNullOrEmpty(title) ? "Paperdoll" : title;
        return (flags & 0x02) != 0 ? text + "  lift" : text;
    }

    // MobileView.IsCovered. A missing layer is serial 0.
    static bool IsCovered(List<UOEquipState> equip, byte layer) {
        switch (layer) {
            case 0x03: {
                var pants = Find(equip, 0x04);
                if (Find(equip, 0x18).Serial != 0 || pants.Serial != 0 && pants.Graphic == 0x1411) return true;
                var robe = Find(equip, 0x16);
                if (pants.Serial != 0 && (pants.Graphic == 0x0513 || pants.Graphic == 0x0514) || robe.Serial != 0 && robe.Graphic == 0x0504)
                    return true;
                break;
            }
            case 0x04: {
                var robe = Find(equip, 0x16);
                var pants = Find(equip, 0x04);
                if (Find(equip, 0x18).Serial != 0 || robe.Serial != 0 && robe.Graphic == 0x0504) return true;
                if (pants.Serial != 0 && (pants.Graphic == 0x01EB || pants.Graphic == 0x03E5 || pants.Graphic == 0x03EB)) {
                    var skirt = Find(equip, 0x17);
                    if (skirt.Serial != 0 && skirt.Graphic != 0x01C7 && skirt.Graphic != 0x01E4) return true;
                    if (robe.Serial != 0 && robe.Graphic != 0x0229 && (robe.Graphic <= 0x04E7 || robe.Graphic > 0x04EB)) return true;
                }
                break;
            }
            case 0x11: {
                var robe = Find(equip, 0x16);
                var tunic = Find(equip, 0x11);
                if (tunic.Serial != 0 && tunic.Graphic == 0x0238)
                    return robe.Serial != 0 && robe.Graphic != 0x9985 && robe.Graphic != 0x9986 && robe.Graphic != 0xA412;
                break;
            }
            case 0x0D: {
                var robe = Find(equip, 0x16);
                if (robe.Serial != 0 && robe.Graphic != 0 && robe.Graphic != 0x9985 && robe.Graphic != 0x9986 && robe.Graphic != 0xA412 && robe.Graphic != 0xA2CA)
                    return true;
                var tunic = Find(equip, 0x11);
                if (tunic.Serial != 0 && tunic.Graphic != 0x1541 && tunic.Graphic != 0x1542) {
                    var torso = Find(equip, 0x0D);
                    if (torso.Serial != 0 && (torso.Graphic == 0x782A || torso.Graphic == 0x782B)) return true;
                }
                break;
            }
            case 0x13: {
                var robe = Find(equip, 0x16);
                return robe.Serial != 0 && robe.Graphic != 0 && robe.Graphic != 0x9985 && robe.Graphic != 0x9986 && robe.Graphic != 0xA412;
            }
            case 0x06:
            case 0x0B: {
                var robe = Find(equip, 0x16);
                if (robe.Serial != 0 && RobeCoversHead(robe.Graphic)) return true;
                break;
            }
        }
        return false;
    }

    static bool RobeCoversHead(ushort graphic) {
        if (graphic > 0x3173) return graphic == 0x4B9D || graphic == 0x7816;
        if (graphic <= 0x2687) {
            if (graphic < 0x2683) return graphic >= 0x204E && graphic <= 0x204F;
            return true;
        }
        return graphic == 0x2FB9 || graphic == 0x3173;
    }

    sealed class DollHost : Control {
        public DollHost() {
            X = 8;
            Y = 19;
            AcceptMouseInput = false;
            CanMove = false;
            WantUpdateSize = false;
        }
    }

    sealed class BasePic : GumpPic {
        readonly PaperdollGump _owner;

        public BasePic(PaperdollGump owner, ushort graphic) : base(0, 0, graphic, 0) => _owner = owner;

        public override bool OnMouseDoubleClick(int x, int y) {
            if (_owner.IsMinimized) _owner.IsMinimized = false;
            return true;
        }
    }

    sealed class VirtuePic : GumpPic {
        readonly uint _serial;

        public VirtuePic(uint serial) : base(80, 4, 0x0071, 0) => _serial = serial;

        public override bool OnMouseDoubleClick(int x, int y) {
            UOLoginNet.ReplyGump(UOLoginNet.PlayerSerial, 0x000001CD, 0x00000001, new[] { _serial }, EmptyEntries);
            return true;
        }
    }

    sealed class MinBox : Control {
        readonly PaperdollGump _owner;

        public MinBox(PaperdollGump owner) {
            _owner = owner;
            X = 228;
            Y = 260;
            Width = 16;
            Height = 16;
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
        }

        public override void OnMouseClick(int x, int y) {
            if (!_owner.IsMinimized) _owner.IsMinimized = true;
        }
    }

    sealed class EquipSlot : Control {
        int _key, _w, _h;
        byte[] _px;

        public EquipSlot(byte layer, int y) {
            Layer = layer;
            X = 2;
            Y = y;
            Width = 19;
            Height = 20;
            WantUpdateSize = false;
            CanMove = false;
            AcceptMouseInput = true;
            Add(new GumpPicTiled(0, 0, 19, 20, 0x243A) { AcceptMouseInput = false, CanMove = false });
            Add(new GumpPic(0, 0, 0x2344, 0) { AcceptMouseInput = false, CanMove = false });
        }

        public byte Layer { get; }

        public void Show(UOEquipState item) {
            LocalSerial = item.Serial;
            _px = null;
            if (item.Serial != 0 && TryArt(item.Graphic, out _key, out _px, out _w, out _h)) return;
            _px = null;
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            var ok = base.Draw(x, y);
            if (_px != null && GumpArt.Sink != null) {
                var dw = _w < 18 ? _w : 18;
                var dh = _h < 18 ? _h : 18;
                GumpArt.Sink.DrawRaw(_key, x + (18 >> 1) - (dw >> 1), y + (18 >> 1) - (dh >> 1), dw, dh, _px, _w, _h);
            }
            return ok;
        }

        public override void OnMouseClick(int x, int y) => ClickItem(LocalSerial);

        public override bool OnMouseDoubleClick(int x, int y) => DoubleItem(LocalSerial);
    }

    sealed class Worn : Control {
        readonly int _key, _w, _h;
        readonly byte[] _px;

        public Worn(uint serial, int x, int y, int key, byte[] px, int w, int h) {
            LocalSerial = serial;
            X = x;
            Y = y;
            Width = w;
            Height = h;
            _key = key;
            _px = px;
            _w = w;
            _h = h;
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            GumpArt.Sink?.DrawRaw(_key, x, y, _w, _h, _px, _w, _h);
            return true;
        }

        public override void OnMouseClick(int x, int y) => ClickItem(LocalSerial);

        public override bool OnMouseDoubleClick(int x, int y) => DoubleItem(LocalSerial);
    }
}
