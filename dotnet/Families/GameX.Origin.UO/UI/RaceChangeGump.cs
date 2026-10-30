// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO RaceChangeGump. 0xBF subcommand 0x2A. One gump. A later packet replaces the race and the sex.
/// Race 0 or above gargoyle closes it. Right-click closes and does not send.
/// The confirm arrow is 0x15A4 at 560, 360. It sends Send_ChangeRaceRequest and disposes.
/// Skin, hair, and beard hues are the creation pallet entry plus 1. Hair and beard styles are those graphics.
/// A female or an elf sends beard style 0 and beard hue 0. Language is ENU, so the labels are font 9.
/// The body gump is 0x000C, 0x000D, 0x000E, or 0x000F. Hair and beard are not drawn.
/// </summary>
public sealed class RaceChangeGump : Gump {
    const byte Elf = 2;
    const byte Gargoyle = 3;
    const int Hair = 0x0B;
    const int Beard = 0x10;
    const int Rows = 8;

    static readonly ushort[] HumanSkin = [
        0x03E9, 0x03F1, 0x03F9, 0x0401, 0x0409, 0x0411, 0x0419, 0x0421, 0x03EA, 0x03F2, 0x03FA, 0x0402, 0x040A,
        0x0412, 0x041A, 0x0421, 0x03EB, 0x03F3, 0x03FB, 0x0403, 0x040B, 0x0413, 0x041B, 0x0421, 0x03EC, 0x03F4,
        0x03FC, 0x0404, 0x040C, 0x0414, 0x041C, 0x0421, 0x03ED, 0x03F5, 0x03FD, 0x0405, 0x040D, 0x0415, 0x041D,
        0x0421, 0x03EE, 0x03F6, 0x03FE, 0x0406, 0x040E, 0x0416, 0x041E, 0x0421, 0x03EF, 0x03F7, 0x03FF, 0x0407,
        0x040F, 0x0417, 0x041F, 0x0421, 0x03F0, 0x03F8, 0x0400, 0x0408, 0x0410, 0x0418, 0x0420, 0x0421,
    ];
    static readonly ushort[] ElfSkin = [
        0x04DD, 0x076B, 0x0834, 0x042F, 0x024C, 0x024D, 0x024E, 0x00BE, 0x04A6, 0x0360, 0x0374, 0x0366, 0x03E7,
        0x03DD, 0x0352, 0x0902, 0x076C, 0x0383, 0x0578, 0x03E8, 0x0373, 0x0388, 0x0384, 0x0375, 0x053E, 0x0380,
        0x0381, 0x0382, 0x076A, 0x03E4, 0x051C, 0x03E5,
    ];
    static readonly ushort[] GargoyleSkin = [
        0x06DA, 0x06DB, 0x06DC, 0x06DD, 0x06DE, 0x06DF, 0x06E0, 0x06E1, 0x06E2, 0x06E3, 0x06E4, 0x06E5, 0x06E6,
        0x06E7, 0x06E8, 0x06E9, 0x06EA, 0x06EB, 0x06EC, 0x06ED, 0x06EE, 0x06EF, 0x06F0, 0x06F1, 0x06F2, 0x06DA,
        0x06DB, 0x06DC,
    ];
    static readonly ushort[] HumanHairColor = [
        0x044D, 0x0455, 0x045D, 0x0465, 0x046D, 0x0475, 0x044E, 0x0456, 0x045E, 0x0466, 0x046E, 0x0476, 0x044F,
        0x0457, 0x045F, 0x0467, 0x046F, 0x0477, 0x0450, 0x0458, 0x0460, 0x0468, 0x0470, 0x0478, 0x0451, 0x0459,
        0x0461, 0x0469, 0x0471, 0x0479, 0x0452, 0x045A, 0x0462, 0x046A, 0x0472, 0x047A, 0x0453, 0x045B, 0x0463,
        0x046B, 0x0473, 0x047B, 0x0454, 0x045C, 0x0464, 0x046C, 0x0474, 0x047C,
    ];
    static readonly ushort[] ElfHairColor = [
        0x0033, 0x0034, 0x0035, 0x0036, 0x0037, 0x0038, 0x0100, 0x06B7, 0x0206, 0x0210, 0x026B, 0x02C2, 0x02C8,
        0x01E3, 0x0238, 0x0368, 0x059C, 0x0852, 0x008D, 0x008E, 0x008F, 0x0090, 0x0091, 0x0158, 0x0159, 0x015A,
        0x015B, 0x015C, 0x015D, 0x01BC, 0x0724, 0x0057, 0x0127, 0x012E, 0x01F2, 0x0250, 0x031C, 0x031D, 0x031E,
        0x031F, 0x0320, 0x0321, 0x0322, 0x0323, 0x0324, 0x0325, 0x0385, 0x0386, 0x0387, 0x0388, 0x0389, 0x0385,
        0x0386, 0x0387,
    ];
    static readonly ushort[] GargoyleHairColor = [
        0x0708, 0x070A, 0x070C, 0x070E, 0x0710, 0x0762, 0x0764, 0x0767, 0x076A, 0x06F2, 0x06F0, 0x06EE, 0x06E3,
        0x06E1, 0x06DF, 0x0708, 0x070A, 0x070C,
    ];
    static readonly int[] HumanHairLabels = [3000340, 3000341, 3000342, 3000343, 3000344, 3000345, 3000346, 3000347, 3000348, 3000349];
    static readonly int[] HumanHairGraphics = [0, 8251, 8252, 8253, 8260, 8261, 8266, 8263, 8264, 8265];
    static readonly int[] HumanFacialLabels = [3000340, 3000351, 3000352, 3000353, 3000354, 1011060, 1011061, 3000357];
    static readonly int[] HumanFacialGraphics = [0, 8256, 8254, 8255, 8257, 8267, 8268, 8269];
    static readonly int[] HumanFemaleHairLabels = [3000340, 3000341, 3000342, 3000343, 3000344, 3000345, 3000346, 3000347, 3000349, 3000350];
    static readonly int[] HumanFemaleHairGraphics = [0, 8251, 8252, 8253, 8260, 8261, 8266, 8263, 8265, 8262];
    static readonly int[] ElfHairLabels = [3000340, 1074385, 1074386, 1074387, 1074388, 1074390, 1074391, 1074392, 1074394];
    static readonly int[] ElfHairGraphics = [0, 0x2FBF, 0x2FC0, 0x2FC1, 0x2FC2, 0x2FCD, 0x2FCE, 0x2FCF, 0x2FD1];
    static readonly int[] ElfFemaleHairLabels = [3000340, 1074386, 1074387, 1074388, 1074389, 1074391, 1074392, 1074393, 1074394];
    static readonly int[] ElfFemaleHairGraphics = [0, 0x2FC0, 0x2FC1, 0x2FC2, 0x2FCC, 0x2FCE, 0x2FCF, 0x2FD0, 0x2FD1];
    static readonly int[] GargoyleHairLabels = [3000340, 1112310, 1112311, 1112312, 1112313, 1112314, 1112315, 1112316, 1112317];
    static readonly int[] GargoyleHairGraphics = [0, 0x4258, 0x4259, 0x425A, 0x425B, 0x425C, 0x425D, 0x425E, 0x425F];
    static readonly int[] GargoyleFacialLabels = [3000340, 1112310, 1112311, 1112312, 1112313];
    static readonly int[] GargoyleFacialGraphics = [0, 0x42AD, 0x42AE, 0x42AF, 0x42B0];
    static readonly int[] GargoyleFemaleHairLabels = [3000340, 1112310, 1112311, 1112312, 1112313, 1112314, 1112315, 1112316, 1112317];
    static readonly int[] GargoyleFemaleHairGraphics = [0, 0x4261, 0x4262, 0x4273, 0x4274, 0x4275, 0x42AA, 0x42AB, 0x42B1];
    static readonly byte[] Marker = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF];

    bool _female;
    byte _race;
    int[] _hairLabels, _hairGfx, _beardLabels, _beardGfx;
    ushort[] _skinPallet, _hairPallet;
    int _hair, _beard, _skinIndex, _hairHueIndex, _beardHueIndex;
    ushort _skin, _hairHue, _beardHue;
    GumpPic _body;
    HueSlot _skinSlot, _hairSlot, _beardSlot;
    StyleBox _hairBox, _beardBox;
    StyleList _styles;
    HueGrid _grid;

    public RaceChangeGump(World world, UORaceChange state) : base(world, 0, 0) {
        X = 50;
        Y = 50;
        CanMove = false;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        Width = 595;
        Height = 400;
        ActivePage = 1;
        Show(state);
    }

    /// <summary>Opens one changer. An invalid race disposes the open one and does not add another.</summary>
    public static void Open(World world, UORaceChange state) {
        var open = UIManager.GetGump<RaceChangeGump>();
        if (state.Race == 0 || state.Race > Gargoyle) {
            open?.Dispose();
            return;
        }
        if (open == null) UIManager.Add(new RaceChangeGump(world, state));
        else open.Show(state);
    }

    public override void OnButtonClick(int buttonID) {
        if (buttonID != 0 || IsDisposed) return;
        var beard = HasBeard;
        UOLoginNet.Send(OutgoingPackets.Send_ChangeRaceRequest(
            _skin,
            Graphic(_hairGfx, _hair),
            _hairHue,
            beard ? Graphic(_beardGfx, _beard) : (ushort)0,
            beard ? _beardHue : (ushort)0));
        Dispose();
    }

    public override void Dispose() {
        if (IsDisposed) return;
        ClosePopups();
        base.Dispose();
    }

    bool HasBeard => !_female && _race != Elf;

    void Show(UORaceChange state) {
        _female = state.Female;
        _race = state.Race;
        _hair = 0;
        _beard = 0;
        _skinIndex = _hairHueIndex = _beardHueIndex = 0;
        ClosePopups();
        while (Children.Count > 0) {
            var c = Children[Children.Count - 1];
            c.Parent = null;
            c.Dispose();
        }
        _body = null;
        _skinSlot = _hairSlot = _beardSlot = null;
        _hairBox = _beardBox = null;
        PickTables();
        _skin = Plus(_skinPallet);
        _hairHue = Plus(_hairPallet);
        _beardHue = HasBeard ? Plus(_hairPallet) : (ushort)0;
        Build();
    }

    void PickTables() {
        if (_race == Elf) {
            _skinPallet = ElfSkin;
            _hairPallet = ElfHairColor;
            _hairLabels = _female ? ElfFemaleHairLabels : ElfHairLabels;
            _hairGfx = _female ? ElfFemaleHairGraphics : ElfHairGraphics;
            _beardLabels = HumanFacialLabels;
            _beardGfx = HumanFacialGraphics;
        }
        else if (_race == Gargoyle) {
            _skinPallet = GargoyleSkin;
            _hairPallet = GargoyleHairColor;
            _hairLabels = _female ? GargoyleFemaleHairLabels : GargoyleHairLabels;
            _hairGfx = _female ? GargoyleFemaleHairGraphics : GargoyleHairGraphics;
            _beardLabels = GargoyleFacialLabels;
            _beardGfx = GargoyleFacialGraphics;
        }
        else {
            _skinPallet = HumanSkin;
            _hairPallet = HumanHairColor;
            _hairLabels = _female ? HumanFemaleHairLabels : HumanHairLabels;
            _hairGfx = _female ? HumanFemaleHairGraphics : HumanHairGraphics;
            _beardLabels = HumanFacialLabels;
            _beardGfx = HumanFacialGraphics;
        }
    }

    void Build() {
        Frame(0, 0, 595, 400);
        Frame(25, 45, 151, 310);
        Frame(419, 45, 151, 310);
        var raceWidth = _race == Gargoyle ? 99 : 79;
        var raceGraphic = _race == Elf ? (ushort)0x705 : _race == Gargoyle ? (ushort)0x7D4 : (ushort)0x702;
        Pic(176 - raceWidth, 360, raceGraphic);
        Pic(419, 360, _female ? (ushort)0x70D : (ushort)0x710);
        var confirm = new Button(0, 0x15A4, 0x15A6, 0x15A5) { X = 560, Y = 360, ButtonAction = ButtonAction.Activate };
        if (!confirm.IsDisposed) Add(confirm);
        Add(new Label(UOGamePackets.ClilocText(_race == Gargoyle ? 1112309 : 3000121), false, 0, 9) { X = 41, Y = 60 });
        _hairBox = new StyleBox(this, false, 40, 75, _hairLabels);
        Add(_hairBox);
        if (HasBeard) {
            Add(new Label(UOGamePackets.ClilocText(_race == Gargoyle ? 1112511 : 3000122), false, 0, 9) { X = 41, Y = 105 });
            _beardBox = new StyleBox(this, true, 40, 120, _beardLabels);
            Add(_beardBox);
        }
        _skinSlot = new HueSlot(this, 0, 434, 60, 3000183, _skinPallet, _skin);
        Add(_skinSlot);
        _hairSlot = new HueSlot(this, Hair, 434, 102, _race == Gargoyle ? 1112322 : 3000184, _hairPallet, _hairHue);
        Add(_hairSlot);
        if (HasBeard) {
            _beardSlot = new HueSlot(this, Beard, 434, 144, _race == Gargoyle ? 1112512 : 3000446, _hairPallet, _beardHue);
            Add(_beardSlot);
        }
        Pic(185, 25, 0x708);
        var body = new GumpPic(210, 75, BodyGraphic(), _skin) { IsPartialHue = true, AcceptMouseInput = false, CanMove = false };
        if (!body.IsDisposed) {
            _body = body;
            Add(body);
        }
        Pic(211, 15, 0x769);
    }

    ushort BodyGraphic() {
        if (_race == Elf) return _female ? (ushort)0x000F : (ushort)0x000E;
        return _female ? (ushort)0x000D : (ushort)0x000C;
    }

    void OpenStyles(StyleBox box, bool beard, int[] labels) {
        if (_styles != null && !_styles.IsDisposed) _styles.Dispose();
        if (labels == null || labels.Length == 0) return;
        _styles = new StyleList(this, beard, At(box, true), At(box, false), labels);
        UIManager.Add(_styles);
    }

    void PickStyle(bool beard, int index) {
        if (beard) {
            _beard = index;
            _beardBox?.SetIndex(index);
        }
        else {
            _hair = index;
            _hairBox?.SetIndex(index);
        }
    }

    void OpenGrid(int layer, ushort[] pallet) {
        if (_grid != null && !_grid.IsDisposed) _grid.Dispose();
        if (pallet == null) return;
        var columns = pallet.Length >> 3;
        if (columns < 1) return;
        var cellW = 125 / columns;
        var cellH = 280 / Rows;
        if (cellW < 1 || cellH < 1) return;
        var index = layer == Hair ? _hairHueIndex : layer == Beard ? _beardHueIndex : _skinIndex;
        _grid = new HueGrid(this, layer, pallet, columns, cellW, cellH, index);
        UIManager.Add(_grid);
    }

    void PickHue(int layer, int index, ushort hue) {
        if (hue == 0xFFFF) return;
        if (layer == Beard) {
            _beardHueIndex = index;
            _beardHue = hue;
            _beardSlot?.SetHue(hue);
        }
        else if (layer == Hair) {
            _hairHueIndex = index;
            _hairHue = hue;
            _hairSlot?.SetHue(hue);
        }
        else {
            _skinIndex = index;
            _skin = hue;
            _skinSlot?.SetHue(hue);
            if (_body != null && !_body.IsDisposed) _body.Hue = hue;
        }
    }

    void ClosePopups() {
        if (_styles != null && !_styles.IsDisposed) _styles.Dispose();
        if (_grid != null && !_grid.IsDisposed) _grid.Dispose();
        _styles = null;
        _grid = null;
    }

    void Frame(int x, int y, int width, int height) {
        Add(new ResizePic(0x0E10) { X = x, Y = y, Width = width, Height = height }, 1);
    }

    void Pic(int x, int y, ushort graphic) {
        var pic = new GumpPic(x, y, graphic, 0);
        if (!pic.IsDisposed) Add(pic);
    }

    static ushort Plus(ushort[] pallet) => (ushort)(pallet[0] + 1);

    static ushort Graphic(int[] ids, int index) => (ushort)ids[index];

    static int At(Control c, bool horizontal) {
        var n = 0;
        for (var p = c; p != null; p = p.Parent) n += horizontal ? p.X : p.Y;
        return n;
    }

    static bool TryRamp(ushort hue, out ushort[] ramp) {
        ramp = null;
        var index = hue & 0x7FFF;
        if (index != 0) index--;
        if (index == 0) return false;
        var uo = UltimaOnline.Current;
        return uo != null && uo.TryHueRamp(index, out ramp) && ramp != null && ramp.Length > 31;
    }

    static byte[] Fill(ushort hue, int w, int h) {
        if (w < 1 || h < 1 || !TryRamp(hue, out var ramp)) return null;
        var c = (ushort)(ramp[31] | 0x8000);
        var lo = (byte)c;
        var hi = (byte)(c >> 8);
        var px = new byte[w * h * 2];
        for (var i = 0; i < w * h; i++) {
            px[i * 2] = lo;
            px[i * 2 + 1] = hi;
        }
        return px;
    }

    sealed class StyleBox : Control {
        readonly RaceChangeGump _owner;
        readonly bool _beard;
        readonly int[] _labels;
        readonly Label _label;

        public StyleBox(RaceChangeGump owner, bool beard, int x, int y, int[] labels) {
            _owner = owner;
            _beard = beard;
            _labels = labels;
            X = x;
            Y = y;
            Width = 120;
            Height = 25;
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
            Add(new ResizePic(0x0BB8) { Width = 120, Height = 25, AcceptMouseInput = false });
            _label = new Label(TextOf(0), false, 0x0453, 9) { X = 2, Y = 5 };
            Add(_label);
            var arrow = new GumpPic(102, 2, 0x00FC, 0) { AcceptMouseInput = false, CanMove = false };
            if (!arrow.IsDisposed) Add(arrow);
        }

        public void SetIndex(int index) => _label.Text = TextOf(index);

        public override void OnMouseClick(int x, int y) => _owner.OpenStyles(this, _beard, _labels);

        string TextOf(int index) {
            if (_labels == null || index < 0 || index >= _labels.Length) return "";
            return UOGamePackets.ClilocText(_labels[index]);
        }
    }

    sealed class StyleList : Gump {
        readonly RaceChangeGump _owner;
        readonly bool _beard;

        public StyleList(RaceChangeGump owner, bool beard, int x, int y, int[] labels) : base(owner.World, 0, 0) {
            _owner = owner;
            _beard = beard;
            X = x;
            Y = y;
            CanMove = false;
            CanCloseWithRightClick = true;
            AcceptMouseInput = true;
            LayerOrder = UILayer.Over;
            WantUpdateSize = false;
            const int rowH = 15;
            var n = labels.Length;
            var h = n * rowH;
            if (h > 200) h = 200;
            var made = new Label[n];
            var w = 120;
            for (var i = 0; i < n; i++) {
                made[i] = new Label(UOGamePackets.ClilocText(labels[i]), false, 0x0453, 9) { X = 2, Y = 0 };
                var tw = made[i].Width + 4;
                if (tw > w) w = tw;
            }
            Add(new ResizePic(0x0BB8) { Width = w, Height = h, AcceptMouseInput = false });
            Width = w;
            Height = h;
            for (var i = 0; i < n; i++) {
                var row = new Row(this, i) { X = 0, Y = i * rowH, Width = w, Height = rowH };
                row.Add(made[i]);
                Add(row);
            }
        }

        void Choose(int index) {
            _owner.PickStyle(_beard, index);
            Dispose();
        }

        sealed class Row : Control {
            readonly StyleList _list;
            readonly int _index;

            public Row(StyleList list, int index) {
                _list = list;
                _index = index;
                AcceptMouseInput = true;
                CanMove = false;
                WantUpdateSize = false;
            }

            public override void OnMouseClick(int x, int y) => _list.Choose(_index);
        }
    }

    sealed class HueSlot : Control {
        readonly RaceChangeGump _owner;
        readonly int _layer;
        readonly ushort[] _pallet;
        readonly Swatch _box;

        public HueSlot(RaceChangeGump owner, int layer, int x, int y, int cliloc, ushort[] pallet, ushort hue) {
            _owner = owner;
            _layer = layer;
            _pallet = pallet;
            X = x;
            Y = y;
            Width = 121;
            Height = 38;
            AcceptMouseInput = false;
            CanMove = false;
            WantUpdateSize = false;
            Add(new Label(UOGamePackets.ClilocText(cliloc), false, 0, 9));
            _box = new Swatch(this);
            _box.SetHue(hue);
            Add(_box);
        }

        public void SetHue(ushort hue) => _box.SetHue(hue);

        public void Open() => _owner.OpenGrid(_layer, _pallet);

        sealed class Swatch : Control {
            readonly HueSlot _slot;
            byte[] _px;
            ushort _hue;

            public Swatch(HueSlot slot) {
                _slot = slot;
                X = 1;
                Y = 15;
                Width = 121;
                Height = 23;
                AcceptMouseInput = true;
                CanMove = false;
                WantUpdateSize = false;
            }

            public void SetHue(ushort hue) {
                if (_hue == hue && _px != null) return;
                _hue = hue;
                _px = Fill(hue, Width, Height);
            }

            public override void OnMouseClick(int x, int y) => _slot.Open();

            public override bool Draw(int x, int y) {
                if (IsDisposed || !IsVisible) return false;
                if (_px != null)
                    GumpArt.Sink?.DrawRaw(unchecked((int)(0xA3100000 + _hue)), x, y, Width, Height, _px, Width, Height);
                return base.Draw(x, y);
            }
        }
    }

    sealed class HueGrid : Gump {
        readonly RaceChangeGump _owner;
        readonly int _layer;
        readonly ushort[] _pallet;
        readonly int _columns, _cellW, _cellH, _index;
        readonly byte[][] _cells;

        public HueGrid(RaceChangeGump owner, int layer, ushort[] pallet, int columns, int cellW, int cellH, int index) : base(owner.World, 0, 0) {
            _owner = owner;
            _layer = layer;
            _pallet = pallet;
            _columns = columns;
            _cellW = cellW;
            _cellH = cellH;
            _index = index;
            _cells = new byte[Rows * columns][];
            X = 485;
            Y = 109;
            Width = columns * cellW;
            Height = Rows * cellH;
            CanMove = false;
            CanCloseWithRightClick = true;
            AcceptMouseInput = true;
            LayerOrder = UILayer.Over;
            WantUpdateSize = false;
        }

        public override void OnMouseClick(int x, int y) {
            if (_cellW < 1 || _cellH < 1) return;
            var column = (x - At(this, true)) / _cellW;
            var row = (y - At(this, false)) / _cellH;
            if (column < 0 || row < 0 || column >= _columns || row >= Rows) return;
            var index = row * _columns + column;
            _owner.PickHue(_layer, index, HueAt(index));
            Dispose();
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            var sink = GumpArt.Sink;
            if (sink != null) {
                for (var row = 0; row < Rows; row++)
                    for (var col = 0; col < _columns; col++) {
                        var index = row * _columns + col;
                        var px = Cell(index);
                        if (px == null) continue;
                        var key = unchecked((int)(0xA3000000 + ((uint)_cellW << 16) + HueAt(index)));
                        sink.DrawRaw(key, x + col * _cellW, y + row * _cellH, _cellW, _cellH, px, _cellW, _cellH);
                    }
                if (_cells.Length > 1) {
                    var mx = (int)(x + (float)Width / _columns * (_index % _columns + 0.5f) - 1);
                    var my = (int)(y + (float)Height / Rows * (_index / _columns + 0.5f) - 1);
                    sink.DrawRaw(unchecked((int)0xA3FFFFFE), mx, my, 2, 2, Marker, 2, 2);
                }
            }
            return base.Draw(x, y);
        }

        byte[] Cell(int index) {
            if (index < 0 || index >= _cells.Length) return null;
            var px = _cells[index];
            if (px != null) return px;
            var hue = HueAt(index);
            if (hue == 0xFFFF) return null;
            px = Fill(hue, _cellW, _cellH);
            _cells[index] = px;
            return px;
        }

        ushort HueAt(int index) {
            if (_pallet == null || index < 0 || index >= _pallet.Length) return 0xFFFF;
            return (ushort)(_pallet[index] + 1);
        }
    }
}
