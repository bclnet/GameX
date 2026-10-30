// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ModernBookGump. 0x93 or 0xD4 opens it at 100, 100. 0x66 fills the lines.
/// Cover 0x01FE, back 0x01FF, forward 0x0200. Title at 40, 60. The word is "by" at 40, 130. Author at 40, 160.
/// Ten lines a page. Left x is 38, right x is 223, top margin is 26. Page numbers sit at y 220.
/// A second open updates the title and the author and does not read the page count again.
/// Page turns stay local. Send_BookPageDataRequest is not queued, so a page that never arrives stays blank.
/// </summary>
public sealed class ModernBookGump : Gump {
    const int MaxLines = 10;
    const int LeftX = 38;
    const int RightX = 223;
    const int Upper = 26;

    readonly PageTurn _back, _forward;
    readonly TextBox _title, _author;
    readonly Label[] _left, _right;
    readonly List<Control> _numbers = [];
    string[] _lines = [];
    bool _editable;

    public ModernBookGump(World world, UOBookState state) : base(world, state.Serial, state.Serial) {
        X = 100;
        Y = 100;
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        Add(new GumpPic(0, 0, 0x01FE, 0) { CanMove = true });
        _back = new PageTurn(this, 0, 0, 0x01FF, false);
        _forward = new PageTurn(this, 356, 0, 0x0200, true);
        if (!_back.IsDisposed) Add(_back);
        if (!_forward.IsDisposed) Add(_forward);
        _title = new TextBox { X = 40, Y = 60, Width = 155, Height = 50, MaxLength = -1 };
        _author = new TextBox { X = 40, Y = 160, Width = 155, Height = 25, MaxLength = -1 };
        Add(_title, 1);
        Add(new Label("by", true, 1) { X = 40, Y = 130 }, 1);
        Add(_author, 1);
        _left = new Label[MaxLines];
        _right = new Label[MaxLines];
        for (var i = 0; i < MaxLines; i++) {
            _left[i] = new Label("", true, 0x01B5, 1) { X = LeftX, Y = Upper + i * 16 };
            _right[i] = new Label("", true, 0x01B5, 1) { X = RightX, Y = Upper + i * 16 };
            Add(_left[i]);
            Add(_right[i]);
        }
        Apply(state, true);
    }

    public ushort BookPageCount { get; private set; }

    int MaxPage => (BookPageCount >> 1) + 1;

    public static void Open(World world, UOBookState state) {
        var open = UIManager.GetGump<ModernBookGump>(state.Serial);
        if (open == null) UIManager.Add(new ModernBookGump(world, state));
        else open.Apply(state, false);
    }

    public static void OnPages(UOBookData data) => UIManager.GetGump<ModernBookGump>(data.Serial)?.SetPages(data.Pages);

    public override void Update() {
        ShowTurns();
        ShowSpread();
        base.Update();
    }

    void Apply(UOBookState state, bool fresh) {
        _editable = state.Editable;
        _title.Text = state.Title ?? "";
        _author.Text = state.Author ?? "";
        _title.AcceptKeyboardInput = _editable;
        _author.AcceptKeyboardInput = _editable;
        if (!fresh) return;
        BookPageCount = state.Pages;
        _lines = new string[BookPageCount * MaxLines];
        BuildNumbers();
        ActivePage = 1;
        ShowTurns();
        ShowSpread();
    }

    void SetPages(UOBookPage[] pages) {
        if (pages == null || _lines == null) return;
        foreach (var page in pages) {
            if (page.Page < 0 || page.Page >= BookPageCount) continue;
            var src = page.Lines;
            var count = src == null ? 0 : src.Length;
            for (var line = 0; line < MaxLines; line++) {
                var index = page.Page * MaxLines + line;
                if ((uint)index >= (uint)_lines.Length) break;
                var text = line < count ? src[line] ?? "" : "";
                _lines[index] = text.Replace("\n", "");
            }
        }
        ShowSpread();
    }

    void BuildNumbers() {
        foreach (var number in _numbers) {
            number.Parent = null;
            number.Dispose();
        }
        _numbers.Clear();
        for (var k = 1; k <= BookPageCount; k++) {
            var x = k % 2 == 1 ? RightX : LeftX;
            var page = k + 1;
            if (page % 2 == 1) page += 1;
            page >>= 1;
            var label = new Label(k.ToString(), true, 1) { X = x + 80, Y = 220 };
            Add(label, page);
            _numbers.Add(label);
        }
    }

    void Turn(int page) {
        if (page < 1) page = 1;
        var max = MaxPage;
        if (page > max) page = max;
        ActivePage = page;
        ShowTurns();
        ShowSpread();
    }

    void ShowTurns() {
        // Page 1 keeps the forward corner even when it is the only page. The last page hides it.
        var first = ActivePage <= 1;
        var last = ActivePage >= MaxPage;
        if (_back != null && !_back.IsDisposed) _back.IsVisible = !first;
        if (_forward != null && !_forward.IsDisposed) _forward.IsVisible = first || !last;
    }

    void ShowSpread() {
        var right = (ActivePage - 1) * 2;
        var left = right - 1;
        for (var i = 0; i < MaxLines; i++) {
            Set(_left[i], left, i);
            Set(_right[i], right, i);
        }
    }

    void Set(Label label, int page, int line) {
        if (label == null || label.IsDisposed) return;
        if (page < 0 || page >= BookPageCount) {
            label.IsVisible = false;
            return;
        }
        label.Text = Line(page, line);
        label.IsVisible = true;
    }

    string Line(int page, int line) {
        var index = page * MaxLines + line;
        if (_lines == null || (uint)index >= (uint)_lines.Length || _lines[index] == null) return "";
        return _lines[index];
    }

    sealed class PageTurn : GumpPic {
        readonly ModernBookGump _owner;
        readonly bool _next;

        public PageTurn(ModernBookGump owner, int x, int y, ushort graphic, bool next) : base(x, y, graphic, 0) {
            _owner = owner;
            _next = next;
        }

        public override void OnMouseClick(int x, int y) => _owner.Turn(_next ? _owner.ActivePage + 1 : _owner.ActivePage - 1);

        public override bool OnMouseDoubleClick(int x, int y) {
            _owner.Turn(_next ? _owner.MaxPage : 1);
            return true;
        }
    }
}
