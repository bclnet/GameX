// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO appearance page. Hair, beard, and hue tables are CharacterCreationValues.
/// Language stays ENU: font 9, no profile. The paperdoll is the body gump, not a Texture2D.
/// </summary>
public class CreateCharAppearanceGump : Gump {
    const int Human = 1, Elf = 2, Gargoyle = 3;
    const int Skin = 0, Shirt = 5, Pants = 4, Hair = 0x0B, Beard = 0x10;
    const int Rows = 8;
    static readonly byte[] Marker = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF];

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

    static readonly char[] SpaceDashPeriodQuote = [' ', '-', '.', '\''];
    static readonly string[] StartDisallowed = ["seer", "counselor", "gm", "admin", "lady", "lord"];
    static readonly string[] Disallowed = [
        "jigaboo", "chigaboo", "wop", "kyke", "kike", "tit", "spic", "prick", "piss", "lezbo", "lesbo",
        "felatio", "dyke", "dildo", "chinc", "chink", "cunnilingus", "cum", "cocksucker", "cock",
        "clitoris", "clit", "ass", "hitler", "penis", "nigga", "nigger", "klit", "kunt", "jiz", "jism",
        "jerkoff", "jackoff", "goddamn", "fag", "blowjob", "bitch", "asshole", "dick", "pussy", "snatch",
        "cunt", "twat", "shit", "fuck", "tailor", "smith", "scholar", "rogue", "novice", "neophyte",
        "merchant", "medium", "master", "mage", "lb", "journeyman", "grandmaster", "fisherman", "expert",
        "chef", "carpenter", "british", "blackthorne", "blackthorn", "beggar", "archer", "apprentice",
        "adept", "gamemaster", "frozen", "squelched", "invulnerable", "osi", "origin",
    ];

    readonly CharDraft _draft = new();
    readonly Dictionary<int, (int index, ushort hue)> _colors = new();
    readonly Dictionary<int, int> _style = new() { [Hair] = 1, [Beard] = 0 };
    readonly List<Control> _live = new();
    readonly Button _human, _elf, _male, _femaleBtn;
    readonly Button _gargoyle;
    readonly Button _next;
    readonly TextBox _name;
    GumpPic _body;
    StyleList _styles;
    HueGrid _grid;
    bool _female;
    byte _race = Human;
    int[] _hairLabels, _hairGfx, _beardLabels, _beardGfx;

    public CreateCharAppearanceGump(World world) : base(world, 0, 0) {
        CanCloseWithRightClick = false;
        CanCloseWithEsc = false;
        Add(new ResizePic(0x0E10) { X = 82, Y = 125, Width = 151, Height = 310 });
        AddPic(280, 53, 0x0709);
        AddPic(240, 73, 0x070A);
        Add(new GumpPicTiled(248, 73, 215, 16, 0x070B) { AcceptMouseInput = false, CanMove = false });
        AddPic(463, 73, 0x070C);
        AddPic(238, 98, 0x0708);
        Add(new ResizePic(0x0E10) { X = 475, Y = 125, Width = 151, Height = 310 });

        _male = Radio(Buttons.MaleButton, 425, 435);
        _femaleBtn = Radio(Buttons.FemaleButton, 425, 455);
        Word((int)Buttons.MaleButton, 0x0710, 0x0712, 0x0711, 445, 435);
        Word((int)Buttons.FemaleButton, 0x070D, 0x070F, 0x070E, 445, 455);
        _name = new TextBox { X = 257, Y = 65, Width = 200, Height = 20, Hue = 1, MaxLength = 16 };
        Add(_name);

        _human = Radio(Buttons.HumanButton, 180, 435);
        Word((int)Buttons.HumanButton, 0x0702, 0x0704, 0x0703, 200, 435);
        _elf = Radio(Buttons.ElfButton, 180, 455);
        Word((int)Buttons.ElfButton, 0x0705, 0x0707, 0x0706, 200, 455);
        if (CharCreationGump.Version >= ClientVersion.CV_60144) {
            _gargoyle = Radio(Buttons.GargoyleButton, 60, 435);
            Word((int)Buttons.GargoyleButton, 0x07D3, 0x07D5, 0x07D4, 80, 435);
        }

        var prev = new Button((int)Buttons.Prev, 0x15A1, 0x15A3, 0x15A2) { X = 586, Y = 445, ButtonAction = ButtonAction.Activate };
        _next = new Button((int)Buttons.Next, 0x15A4, 0x15A6, 0x15A5) { X = 610, Y = 445, ButtonAction = ButtonAction.Activate };
        if (!prev.IsDisposed) Add(prev);
        if (_next != null && !_next.IsDisposed) Add(_next);

        Mark(_male, true);
        Mark(_human, true);
        HandleRaceChanged();
        _name.SetKeyboardFocus();
    }

    bool HasBeard => !_female && _race != Elf;

    public override void OnButtonClick(int buttonID) {
        var shell = CharCreationGump.Shell(this);
        switch ((Buttons)buttonID) {
            case Buttons.FemaleButton:
                Mark(_femaleBtn, true);
                Mark(_male, false);
                _female = true;
                HandleGenreChange();
                break;
            case Buttons.MaleButton:
                Mark(_male, true);
                Mark(_femaleBtn, false);
                _female = false;
                HandleGenreChange();
                break;
            case Buttons.HumanButton:
                _race = Human;
                if (_human != null && _human.ButtonGraphicNormal != 0x0767) {
                    Mark(_human, true);
                    Mark(_elf, false);
                    Mark(_gargoyle, false);
                    HandleRaceChanged();
                }
                break;
            case Buttons.ElfButton:
                _race = Elf;
                if (_elf != null && _elf.ButtonGraphicNormal != 0x0767) {
                    Mark(_elf, true);
                    Mark(_human, false);
                    Mark(_gargoyle, false);
                    HandleRaceChanged();
                }
                break;
            case Buttons.GargoyleButton:
                if (_gargoyle == null) break;
                _race = Gargoyle;
                if (_gargoyle.ButtonGraphicNormal != 0x0767) {
                    Mark(_gargoyle, true);
                    Mark(_elf, false);
                    Mark(_human, false);
                    HandleRaceChanged();
                }
                break;
            case Buttons.Next:
                if (_next != null && !_next.IsEnabled) break;
                _draft.Name = _name.Text ?? "";
                var invalid = ValidateName(_draft.Name);
                if (invalid > 0) {
                    shell?.ShowMessage(CharCreationGump.Cliloc(invalid, invalid == 3000612 ? "That name is too short." : "That name is not valid."));
                    break;
                }
                Remember();
                shell?.SetCharacter(_draft);
                break;
            case Buttons.Prev:
                if (UIManager.KeyboardFocusControl == _name) UIManager.KeyboardFocusControl = null;
                shell?.StepBack();
                break;
        }
    }

    public override void Dispose() {
        ClosePopups();
        base.Dispose();
    }

    void HandleRaceChanged() {
        _colors.Clear();
        HandleGenreChange();
        var allowElf = CharCreationGump.Version >= ClientVersion.CV_4011D
            && CharCreationGump.Expansion(CharCreationGump.LockedML, ClientVersion.CV_4011D);
        var allowGarg = CharCreationGump.Version >= ClientVersion.CV_60144
            && CharCreationGump.Expansion(CharCreationGump.LockedSA, ClientVersion.CV_60144);
        var ok = !(_race == Elf && !allowElf) && !(_race == Gargoyle && !allowGarg);
        if (_next != null && !_next.IsDisposed) _next.IsEnabled = ok;
    }

    void HandleGenreChange() {
        _style[Beard] = 0;
        _style[Hair] = 1;
        DropLive();
        PickTables();
        var hair = new StyleBox(this, false, 97, 155, _hairLabels);
        Live(new Label(CharCreationGump.Cliloc(_race == Gargoyle ? 1112309 : 3000121, _race == Gargoyle ? "Horns" : "Hair"), false, 0, 9) { X = 98, Y = 140 });
        Live(hair);
        if (HasBeard) {
            Live(new Label(CharCreationGump.Cliloc(_race == Gargoyle ? 1112511 : 3000122, "Facial Hair"), false, 0, 9) { X = 98, Y = 184 });
            Live(new StyleBox(this, true, 97, 199, _beardLabels));
        }
        var skin = Picker(489, 141, SkinPallet(), Skin, 3000183, "Skin Tone", Rows, Columns(SkinPallet()));
        Picker(489, 183, null, Shirt, 3000440, "Shirt Color", 10, 20);
        if (_race != Gargoyle) Picker(489, 225, null, Pants, 3000441, "Pants Color", 10, 20);
        var hairPallet = HairPallet();
        Picker(489, 267, hairPallet, Hair, _race == Gargoyle ? 1112322 : 3000184, "Hair Color", Rows, Columns(hairPallet));
        if (HasBeard)
            Picker(489, 309, hairPallet, Beard, _race == Gargoyle ? 1112512 : 3000446, "Facial Hair Color", Rows, Columns(hairPallet));
        var body = new GumpPic(262, 135, Doll(), skin) { IsPartialHue = true, AcceptMouseInput = false, CanMove = false };
        if (!body.IsDisposed) {
            _body = body;
            Live(body);
        }
        Remember();
    }

    ushort Picker(int x, int y, ushort[] pallet, int layer, int cliloc, string fallback, int rows, int columns) {
        var slot = new HueSlot(this, layer, x, y, CharCreationGump.Cliloc(cliloc, fallback), pallet, rows, columns);
        Live(slot);
        if (_colors.TryGetValue(layer, out var saved)) slot.SetHue(saved.hue);
        else _colors[layer] = (0, slot.Hue);
        return slot.Hue;
    }

    void Live(Control control) {
        _live.Add(control);
        Add(control);
    }

    void DropLive() {
        ClosePopups();
        for (var i = 0; i < _live.Count; i++) {
            var c = _live[i];
            if (c.IsDisposed) continue;
            c.Parent = null;
            c.Dispose();
        }
        _live.Clear();
        _body = null;
    }

    void PickTables() {
        if (_race == Elf) {
            _hairLabels = _female ? ElfFemaleHairLabels : ElfHairLabels;
            _hairGfx = _female ? ElfFemaleHairGraphics : ElfHairGraphics;
            _beardLabels = HumanFacialLabels;
            _beardGfx = HumanFacialGraphics;
        }
        else if (_race == Gargoyle) {
            _hairLabels = _female ? GargoyleFemaleHairLabels : GargoyleHairLabels;
            _hairGfx = _female ? GargoyleFemaleHairGraphics : GargoyleHairGraphics;
            _beardLabels = GargoyleFacialLabels;
            _beardGfx = GargoyleFacialGraphics;
        }
        else {
            _hairLabels = _female ? HumanFemaleHairLabels : HumanHairLabels;
            _hairGfx = _female ? HumanFemaleHairGraphics : HumanHairGraphics;
            _beardLabels = HumanFacialLabels;
            _beardGfx = HumanFacialGraphics;
        }
    }

    ushort[] SkinPallet() => _race == Elf ? ElfSkin : _race == Gargoyle ? GargoyleSkin : HumanSkin;
    ushort[] HairPallet() => _race == Elf ? ElfHairColor : _race == Gargoyle ? GargoyleHairColor : HumanHairColor;

    static int Columns(ushort[] pallet) {
        if (pallet == null || pallet.Length < Rows) return 1;
        var n = pallet.Length >> 3;
        return n < 1 ? 1 : n;
    }

    ushort Doll() {
        if (_race == Gargoyle) return _female ? (ushort)0x0299 : (ushort)0x029A;
        if (_race == Elf) return _female ? (ushort)0x000F : (ushort)0x000E;
        return _female ? (ushort)0x000D : (ushort)0x000C;
    }

    void Remember() {
        _draft.Female = _female;
        _draft.Race = _race;
        _draft.Hue = Hue(Skin);
        _draft.ShirtHue = Hue(Shirt);
        _draft.PantsHue = _race == Gargoyle ? (ushort)0 : Hue(Pants);
        _draft.HairHue = Hue(Hair);
        _draft.BeardHue = HasBeard ? Hue(Beard) : (ushort)0;
        _draft.HairGraphic = Graphic(_hairGfx, _style[Hair]);
        _draft.BeardGraphic = HasBeard ? Graphic(_beardGfx, _style[Beard]) : (ushort)0;
    }

    ushort Hue(int layer) => _colors.TryGetValue(layer, out var row) ? row.hue : (ushort)0;

    static ushort Graphic(int[] ids, int index) {
        if (ids == null || index < 0 || index >= ids.Length) return 0;
        return (ushort)ids[index];
    }

    internal void PickStyle(bool beard, int index) {
        _style[beard ? Beard : Hair] = index;
        Remember();
    }

    void OpenStyles(StyleBox box, bool beard, int[] labels) {
        if (_styles != null && !_styles.IsDisposed) _styles.Dispose();
        if (labels == null || labels.Length == 0) return;
        _styles = new StyleList(this, box, beard, At(box, true), At(box, false), labels);
        UIManager.Add(_styles);
    }

    void OpenGrid(HueSlot slot) {
        if (_grid != null && !_grid.IsDisposed) _grid.Dispose();
        _grid = new HueGrid(this, slot);
        UIManager.Add(_grid);
    }

    internal void PickHue(int layer, int index, ushort hue) {
        if (hue == 0xFFFF) return;
        _colors[layer] = (index, hue);
        if (layer == Skin && _body != null && !_body.IsDisposed) _body.Hue = hue;
        Remember();
    }

    void ClosePopups() {
        if (_styles != null && !_styles.IsDisposed) _styles.Dispose();
        if (_grid != null && !_grid.IsDisposed) _grid.Dispose();
        _styles = null;
        _grid = null;
    }

    Button Radio(Buttons id, int x, int y) {
        var button = new Button((int)id, 0x0768, 0x0767) { X = x, Y = y, ButtonAction = ButtonAction.Activate };
        if (button.IsDisposed) return null;
        Add(button);
        return button;
    }

    void Word(int id, ushort normal, ushort pressed, ushort over, int x, int y) {
        var button = new Button(id, normal, pressed, over) { X = x, Y = y, ButtonAction = ButtonAction.Activate };
        if (!button.IsDisposed) Add(button);
    }

    void AddPic(int x, int y, ushort graphic) {
        var pic = new GumpPic(x, y, graphic, 0) { AcceptMouseInput = false, CanMove = false };
        if (!pic.IsDisposed) Add(pic);
    }

    static void Mark(Button button, bool on) {
        if (button == null || button.IsDisposed) return;
        var graphic = on ? (ushort)0x0767 : (ushort)0x0768;
        if (button.ButtonGraphicNormal == graphic) return;
        if (!GumpMetrics.TrySize(graphic, out _, out _)) return;
        button.ButtonGraphicNormal = graphic;
    }

    public static int ValidateName(string name) {
        var blocked = CharCreationGump.Version >= ClientVersion.CV_5020 ? Disallowed : [];
        return Validate(name, 2, 16, true, false, true, 1, SpaceDashPeriodQuote, blocked, StartDisallowed);
    }

    public static int Validate(string name, int minLength, int maxLength, bool allowLetters, bool allowDigits, bool noExceptionsAtStart, int maxExceptions, char[] exceptions, string[] disallowed, string[] startDisallowed) {
        if (string.IsNullOrEmpty(name) || name.Length < minLength) return 3000612;
        if (name.Length > maxLength) return 3000611;
        if (name.Trim() != name) return 3000611;
        var exceptCount = 0;
        name = name.ToLowerInvariant();
        if (!allowLetters || !allowDigits || (exceptions.Length > 0 && (noExceptionsAtStart || maxExceptions < int.MaxValue))) {
            for (var i = 0; i < name.Length; i++) {
                var c = name[i];
                if (c >= 'a' && c <= 'z') exceptCount = 0;
                else if (c >= '0' && c <= '9') {
                    if (!allowDigits) return 3000611;
                    exceptCount = 0;
                }
                else {
                    var except = false;
                    for (var j = 0; !except && j < exceptions.Length; j++)
                        if (c == exceptions[j]) except = true;
                    if (!except || (i == 0 && noExceptionsAtStart)) return 3000611;
                    if (exceptCount++ == maxExceptions) return 3000611;
                }
            }
        }
        for (var i = 0; i < disallowed.Length; i++) {
            var indexOf = name.IndexOf(disallowed[i]);
            if (indexOf == -1) continue;
            var badPrefix = indexOf == 0;
            for (var j = 0; !badPrefix && j < exceptions.Length; j++)
                badPrefix = name[indexOf - 1] == exceptions[j];
            if (!badPrefix) continue;
            var badSuffix = indexOf + disallowed[i].Length >= name.Length;
            for (var j = 0; !badSuffix && j < exceptions.Length; j++)
                badSuffix = name[indexOf + disallowed[i].Length] == exceptions[j];
            if (badSuffix) return 3000611;
        }
        for (var i = 0; i < startDisallowed.Length; i++)
            if (name.StartsWith(startDisallowed[i])) return 3000611;
        return 0;
    }

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

    enum Buttons { MaleButton, FemaleButton, HumanButton, ElfButton, GargoyleButton, Prev, Next }

    sealed class StyleBox : Control {
        readonly CreateCharAppearanceGump _owner;
        readonly bool _beard;
        readonly int[] _labels;
        readonly Label _label;

        public StyleBox(CreateCharAppearanceGump owner, bool beard, int x, int y, int[] labels) {
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
            Add(new ResizePic(0x0BB8) { Width = 120, Height = 25, AcceptMouseInput = false, CanMove = false });
            _label = new Label(TextOf(owner._style[beard ? Beard : Hair]), false, 0x0453, 9) { X = 2, Y = 5 };
            Add(_label);
            var arrow = new GumpPic(102, 2, 0x00FC, 0) { AcceptMouseInput = false, CanMove = false };
            if (!arrow.IsDisposed) Add(arrow);
        }

        public void SetIndex(int index) => _label.Text = TextOf(index);

        public override void OnMouseClick(int x, int y) => _owner.OpenStyles(this, _beard, _labels);

        string TextOf(int index) {
            if (_labels == null || index < 0 || index >= _labels.Length) return "";
            return CharCreationGump.Cliloc(_labels[index], "#" + _labels[index]);
        }
    }

    sealed class StyleList : Gump {
        readonly CreateCharAppearanceGump _owner;
        readonly bool _beard;
        readonly StyleBox _box;

        public StyleList(CreateCharAppearanceGump owner, StyleBox box, bool beard, int x, int y, int[] labels) : base(owner.World, 0, 0) {
            _owner = owner;
            _box = box;
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
                made[i] = new Label(CharCreationGump.Cliloc(labels[i], "#" + labels[i]), false, 0x0453, 9) { X = 2, Y = 0 };
                var tw = made[i].Width + 4;
                if (tw > w) w = tw;
            }
            Add(new ResizePic(0x0BB8) { Width = w, Height = h, AcceptMouseInput = false, CanMove = false });
            Width = w;
            Height = h;
            for (var i = 0; i < n; i++) {
                var row = new Row(this, i) { Y = i * rowH, Width = w, Height = rowH };
                row.Add(made[i]);
                Add(row);
            }
        }

        void Choose(int index) {
            _owner.PickStyle(_beard, index);
            _box.SetIndex(index);
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
        readonly CreateCharAppearanceGump _owner;
        readonly int _layer;
        readonly ushort[] _pallet;
        readonly int _rows, _columns;
        readonly Swatch _box;

        public HueSlot(CreateCharAppearanceGump owner, int layer, int x, int y, string caption, ushort[] pallet, int rows, int columns) {
            _owner = owner;
            _layer = layer;
            _pallet = pallet;
            _rows = rows;
            _columns = columns < 1 ? 1 : columns;
            X = x;
            Y = y;
            Width = 121;
            Height = 38;
            AcceptMouseInput = false;
            CanMove = false;
            WantUpdateSize = false;
            Add(new Label(caption ?? "", false, 0, 9));
            _box = new Swatch(this, FirstHue(pallet));
            Add(_box);
        }

        public int Layer => _layer;
        public ushort[] Pallet => _pallet;
        public int Rows => _rows;
        public int Columns => _columns;
        public ushort Hue => _box.Hue;
        public int SelectedIndex { get; private set; }

        public void SetHue(ushort hue) => _box.SetHue(hue);

        public void Choose(int index, ushort hue) {
            SelectedIndex = index;
            _box.SetHue(hue);
        }

        public void Open() => _owner.OpenGrid(this);

        static ushort FirstHue(ushort[] pallet) => (ushort)((pallet != null && pallet.Length > 0 ? pallet[0] : 1) + 1);

        sealed class Swatch : Control {
            readonly HueSlot _slot;
            byte[] _px;
            ushort _hue;

            public Swatch(HueSlot slot, ushort hue) {
                _slot = slot;
                X = 1;
                Y = 15;
                Width = 121;
                Height = 23;
                AcceptMouseInput = true;
                CanMove = false;
                WantUpdateSize = false;
                SetHue(hue);
            }

            public ushort Hue => _hue;

            public void SetHue(ushort hue) {
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
        readonly CreateCharAppearanceGump _owner;
        readonly HueSlot _slot;
        readonly int _cellW, _cellH;
        readonly byte[][] _cells;
        readonly ushort[] _hues;

        public HueGrid(CreateCharAppearanceGump owner, HueSlot slot) : base(owner.World, 0, 0) {
            _owner = owner;
            _slot = slot;
            var columns = slot.Columns < 1 ? 1 : slot.Columns;
            var rows = slot.Rows < 1 ? 1 : slot.Rows;
            _cellW = 125 / columns;
            _cellH = 280 / rows;
            if (_cellW < 1) _cellW = 1;
            if (_cellH < 1) _cellH = 1;
            var count = rows * columns;
            _cells = new byte[count][];
            _hues = new ushort[count];
            FillHues(slot.Pallet, rows, columns);
            X = 489;
            Y = 141;
            Width = columns * _cellW;
            Height = rows * _cellH;
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
            if (column < 0 || row < 0 || column >= _slot.Columns || row >= _slot.Rows) return;
            var index = row * _slot.Columns + column;
            if (index < 0 || index >= _hues.Length) return;
            _slot.Choose(index, _hues[index]);
            _owner.PickHue(_slot.Layer, index, _hues[index]);
            Dispose();
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            var sink = GumpArt.Sink;
            if (sink != null) {
                for (var i = 0; i < _hues.Length; i++) {
                    var px = Cell(i);
                    if (px == null) continue;
                    var col = i % _slot.Columns;
                    var row = i / _slot.Columns;
                    sink.DrawRaw(unchecked((int)(0xA3200000 + _hues[i])), x + col * _cellW, y + row * _cellH, _cellW, _cellH, px, _cellW, _cellH);
                }
                if (_hues.Length > 1) {
                    var index = _slot.SelectedIndex;
                    if (index < 0 || index >= _hues.Length) index = 0;
                    var mx = (int)(x + (float)Width / _slot.Columns * (index % _slot.Columns + 0.5f) - 1);
                    var my = (int)(y + (float)Height / _slot.Rows * (index / _slot.Columns + 0.5f) - 1);
                    sink.DrawRaw(unchecked((int)0xA32FFFFE), mx, my, 2, 2, Marker, 2, 2);
                }
            }
            return base.Draw(x, y);
        }

        void FillHues(ushort[] pallet, int rows, int columns) {
            ushort start = 2;
            for (var y = 0; y < rows; y++)
                for (var x = 0; x < columns; x++) {
                    var i = y * columns + x;
                    var custom = pallet != null && i < pallet.Length ? pallet[i] : start;
                    _hues[i] = (ushort)(custom + 1);
                    start += 5;
                }
        }

        byte[] Cell(int index) {
            if (index < 0 || index >= _cells.Length) return null;
            var px = _cells[index];
            if (px != null) return px;
            px = Fill(_hues[index], _cellW, _cellH);
            _cells[index] = px;
            return px;
        }
    }
}
