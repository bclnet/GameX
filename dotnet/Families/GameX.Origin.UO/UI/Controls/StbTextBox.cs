// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO StbTextBox without the native editor. Text, caret, selection, and max length
/// are a string. Draw asks GumpArt.Sink.DrawText. No glyph texture and no P/Invoke.
/// Arrow keys are the SDL keycode numbers HoldWalk already uses. Letters also match uppercase ASCII.
/// </summary>
public class StbTextBox : Control {
    const int KeyBackspace = 8;
    const int KeyTab = 9;
    const int KeyReturn = 13;
    const int KeyEscape = 27;
    const int KeyDelete = 127;
    const int KeyInsert = 1073741897;
    const int KeyHome = 1073741898;
    const int KeyEnd = 1073741901;
    const int KeyRight = 1073741903;
    const int KeyLeft = 1073741904;
    const int KeyDown = 1073741905;
    const int KeyUp = 1073741906;
    const int KeyKpEnter = 1073741912;
    const int ModShift = 0x0003;
    const int ModCtrl = 0x00C0;
    const int LineH = 16;

    readonly int _maxCharCount;
    readonly List<Snap> _undo = new List<Snap>();
    readonly List<Snap> _redo = new List<Snap>();
    string _text = "";
    int _caret;
    int _selectStart;
    int _selectEnd;
    int _preferredX = -1;
    byte _font;
    bool _unicode;
    bool _singleLine = true;
    bool _insert;
    bool _isWriting;
    bool _fromServer;
    // No SDL clipboard and no P/Invoke. Copy and paste stay in this process.
    static string _clip = "";

    public StbTextBox(byte font, int maxCharCount = -1, int maxWidth = 0, bool isUnicode = true, ushort hue = 0) {
        AcceptKeyboardInput = true;
        AcceptMouseInput = true;
        CanMove = false;
        IsEditable = true;
        AllowSelection = true;
        _maxCharCount = maxCharCount;
        _font = font;
        _unicode = isUnicode;
        Hue = hue;
        Height = LineH;
        if (maxWidth > 0) Width = maxWidth;
    }

    public StbTextBox(List<string> parts, string[] lines) : this(
        1,
        parts[0] == "textentrylimited" ? int.Parse(parts[8]) : byte.MaxValue,
        int.Parse(parts[3]),
        true,
        HueOf(parts[5])) {
        X = int.Parse(parts[1]);
        Y = int.Parse(parts[2]);
        Width = int.Parse(parts[3]);
        Height = int.Parse(parts[4]);
        Multiline = false;
        _fromServer = true;
        IsFromServer = true;
        LocalSerial = SerialHelper.Parse(parts[6]);
        var index = int.Parse(parts[7]);
        if (lines != null && index >= 0 && index < lines.Length) SetText(lines[index]);
    }

    public override bool AcceptKeyboardInput => base.AcceptKeyboardInput && IsEditable;

    public byte Font { get => _font; set => _font = value; }
    public bool AllowTAB { get; set; }
    public bool NoSelection { get; set; }
    public bool PassEnterToParent { get; set; }
    public bool IsEditable { get; set; }
    public bool IsFromServer { get; set; }
    public bool NumbersOnly { get; set; }
    public bool AllowSelection { get; set; }
    public bool IsUnicode => _unicode;
    public ushort Hue { get; set; }

    public bool Multiline {
        get => !_singleLine;
        set => _singleLine = !value;
    }

    public int CaretIndex {
        get => _caret;
        set {
            _caret = value;
            Clamp();
        }
    }

    public int SelectionStart {
        get => _selectStart;
        set { if (AllowSelection) _selectStart = value; }
    }

    public int SelectionEnd {
        get => _selectEnd;
        set { if (AllowSelection) _selectEnd = value; }
    }

    public string Text {
        get => _text;
        set {
            var s = value ?? "";
            if (_maxCharCount > 0 && s.Length > _maxCharCount) s = s.Substring(0, _maxCharCount);
            var previous = _text;
            _text = s;
            Clamp();
            if (!_isWriting) OnTextChanged(previous);
        }
    }

    public int Length => _text.Length;

    public event EventHandler<BeforeTextChangedEventArgs> BeforeTextChanged;
    public event EventHandler TextChanged;

    public void SelectAll() {
        if (!AllowSelection) return;
        _selectStart = 0;
        _selectEnd = _caret = _text.Length;
    }

    public void SetText(string text) {
        if (string.IsNullOrEmpty(text)) {
            ClearText();
            return;
        }
        var previous = _text;
        if (_maxCharCount > 0 && !NumbersOnly && text.Length > _maxCharCount)
            text = text.Substring(0, _maxCharCount);
        _undo.Clear();
        _redo.Clear();
        _insert = false;
        _selectStart = _selectEnd = 0;
        _text = text;
        _caret = _text.Length;
        Clamp();
        OnTextChanged(previous);
    }

    public void ClearText() {
        if (_text.Length == 0) return;
        var previous = _text;
        _undo.Clear();
        _redo.Clear();
        _selectStart = _selectEnd = _caret = 0;
        _text = "";
        OnTextChanged(previous);
    }

    public void AppendText(string text) => OnTextInput(text);

    public override void OnFocusEnter() {
        base.OnFocusEnter();
        CaretIndex = _text.Length;
    }

    public override void OnFocusLost() {
        _selectStart = _selectEnd = 0;
        base.OnFocusLost();
    }

    public override void OnMouseClick(int x, int y) {
        if (!IsEditable) return;
        Place(x, y);
    }

    public override bool OnMouseDoubleClick(int x, int y) {
        if (NoSelection || _text.Length == 0) return false;
        Place(x, y);
        if (_caret < 0 || _caret >= _text.Length || char.IsWhiteSpace(_text[_caret])) return false;
        var idx = _caret;
        if (idx - 1 >= 0 && char.IsWhiteSpace(_text[idx - 1])) idx++;
        _selectStart = MovePrevWord(idx);
        _selectEnd = MoveNextWord(idx);
        if (_selectEnd < _text.Length) _selectEnd--;
        _caret = _selectEnd;
        return true;
    }

    /// <summary>SDL keycode and SDL_Keymod. The scene does not call this; UIManager still types into TextBox.</summary>
    public void OnKeyDown(int key, int mod) {
        var letter = key >= 'A' && key <= 'Z' ? key + 32 : key;
        var ctrl = (mod & ModCtrl) != 0;
        var shift = (mod & ModShift) != 0 && !NoSelection;
        if (key == KeyTab) {
            if (AllowTAB) OnTextInput("   ");
            else FocusNext();
            return;
        }
        if (key == KeyEscape) {
            _selectStart = _selectEnd = 0;
            return;
        }
        if (ctrl && letter == 'a' && !NoSelection) { SelectAll(); return; }
        if (ctrl && letter == 'c' && !NoSelection) { Copy(); return; }
        if (ctrl && letter == 'x' && !NoSelection) {
            Copy();
            if (IsEditable) Edit(DeleteSelection);
            return;
        }
        if (ctrl && letter == 'v' && IsEditable) { OnTextInput(ClipText(_clip)); return; }
        if (ctrl && letter == 'z' && IsEditable) { Undo(); return; }
        if (ctrl && letter == 'y' && IsEditable) { Redo(); return; }
        if (key == KeyInsert && IsEditable) { _insert = !_insert; return; }
        if (key == KeyReturn || key == KeyKpEnter) {
            if (!IsEditable) return;
            if (Multiline && !PassEnterToParent) {
                if (!_fromServer && !AtMax(0)) OnTextInput("\n");
            }
            else Parent?.OnKeyboardReturn(0, _text);
            // System chat is not on UIManager.
            return;
        }
        if (key == KeyLeft) MoveLeft(shift, ctrl);
        else if (key == KeyRight) MoveRight(shift, ctrl);
        else if (key == KeyUp) MoveVert(false, shift);
        else if (key == KeyDown) MoveVert(true, shift);
        else if (key == KeyHome) { if (ctrl) MoveText(true, shift); else MoveLine(true, shift); }
        else if (key == KeyEnd) { if (ctrl) MoveText(false, shift); else MoveLine(false, shift); }
        else if (key == KeyBackspace && IsEditable) Edit(Backspace);
        else if (key == KeyDelete && IsEditable) Edit(DeleteForward);
    }

    public void OnTextInput(string c) {
        if (string.IsNullOrEmpty(c) || !IsEditable) return;
        c = ClipText(c);
        if (c.Length == 0) return;
        if (NumbersOnly)
            for (var i = 0; i < c.Length; i++)
                if (!char.IsNumber(c[i])) return;
        if (c.Length == 1 && c[0] != '\n' && Measure(c) <= 0 && _selectStart == _selectEnd) return;
        var insert = c;
        Edit(() => {
            if (_selectStart != _selectEnd) DeleteSelection();
            if (_maxCharCount > 0) {
                var remains = _maxCharCount - _text.Length;
                if (remains <= 0) return;
                if (insert.Length > remains) insert = insert.Substring(0, remains);
            }
            if (insert.Length == 0) return;
            if (_insert && insert.Length == 1 && _caret < _text.Length) {
                _text = _text.Remove(_caret, 1).Insert(_caret, insert);
                _caret++;
            }
            else {
                _text = _text.Insert(_caret, insert);
                _caret += insert.Length;
            }
            _selectStart = _selectEnd = _caret;
            _preferredX = -1;
        });
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        var text = _text;
        var lineY = y;
        var start = 0;
        while (true) {
            var nl = Multiline ? text.IndexOf('\n', start) : -1;
            var end = nl < 0 ? text.Length : nl;
            DrawLine(x, lineY, text, start, end);
            if (nl < 0) break;
            start = nl + 1;
            lineY += LineH;
        }
        if (HasKeyboardFocus) {
            CaretPos(out var cx, out var cy);
            GumpArt.Sink?.DrawText(x + cx, y + cy, "_", Hue, Font);
        }
        return base.Draw(x, y);
    }

    void OnTextChanged(string previous) {
        var args = new BeforeTextChangedEventArgs(previous ?? "", _text ?? "", _caret);
        BeforeTextChanged?.Invoke(this, args);
        var next = args.NewText ?? "";
        if (_maxCharCount > 0 && next.Length > _maxCharCount) next = next.Substring(0, _maxCharCount);
        if (next != _text) _text = next;
        _caret = args.NewCaretIndex;
        Clamp();
        TextChanged?.Invoke(this, EventArgs.Empty);
    }

    void Edit(Action apply) {
        var previous = _text;
        var snap = new Snap(_text, _caret, _selectStart, _selectEnd);
        _undo.Add(snap);
        if (_undo.Count > 50) _undo.RemoveAt(0);
        _redo.Clear();
        _isWriting = true;
        apply();
        Clamp();
        if (_text == previous && _caret == snap.Caret && _selectStart == snap.Start && _selectEnd == snap.End) {
            _undo.RemoveAt(_undo.Count - 1);
            _isWriting = false;
            return;
        }
        OnTextChanged(previous);
        _isWriting = false;
    }

    void Undo() {
        if (_undo.Count == 0) return;
        var snap = _undo[_undo.Count - 1];
        _undo.RemoveAt(_undo.Count - 1);
        _redo.Add(new Snap(_text, _caret, _selectStart, _selectEnd));
        Restore(snap);
    }

    void Redo() {
        if (_redo.Count == 0) return;
        var snap = _redo[_redo.Count - 1];
        _redo.RemoveAt(_redo.Count - 1);
        _undo.Add(new Snap(_text, _caret, _selectStart, _selectEnd));
        Restore(snap);
    }

    void Restore(Snap snap) {
        var previous = _text;
        _text = snap.Text ?? "";
        _caret = snap.Caret;
        _selectStart = snap.Start;
        _selectEnd = snap.End;
        _preferredX = -1;
        OnTextChanged(previous);
    }

    void Backspace() {
        if (_selectStart != _selectEnd) DeleteSelection();
        else if (_caret > 0) {
            _caret--;
            _text = _text.Remove(_caret, 1);
            _selectStart = _selectEnd = _caret;
        }
        _preferredX = -1;
    }

    void DeleteForward() {
        if (_selectStart != _selectEnd) DeleteSelection();
        else if (_caret < _text.Length) {
            _text = _text.Remove(_caret, 1);
            _selectStart = _selectEnd = _caret;
        }
        _preferredX = -1;
    }

    void DeleteSelection() {
        var a = Math.Min(_selectStart, _selectEnd);
        var b = Math.Max(_selectStart, _selectEnd);
        if (a < 0) a = 0;
        if (b > _text.Length) b = _text.Length;
        if (a >= b) {
            _selectStart = _selectEnd = _caret;
            return;
        }
        _text = _text.Remove(a, b - a);
        _caret = _selectStart = _selectEnd = a;
    }

    void Copy() {
        var a = Math.Min(_selectStart, _selectEnd);
        var b = Math.Max(_selectStart, _selectEnd);
        if (a < 0) a = 0;
        if (b > _text.Length) b = _text.Length;
        if (a < b) _clip = _text.Substring(a, b - a);
    }

    void MoveLeft(bool shift, bool word) {
        _preferredX = -1;
        if (word) {
            if (shift) Extend(MovePrevWord(_caret));
            else if (_selectStart != _selectEnd) Collapse(false);
            else _caret = MovePrevWord(_caret);
            return;
        }
        if (shift) Extend(_caret > 0 ? _caret - 1 : 0);
        else if (_selectStart != _selectEnd) Collapse(false);
        else if (_caret > 0) _caret--;
    }

    void MoveRight(bool shift, bool word) {
        _preferredX = -1;
        if (word) {
            if (shift) Extend(MoveNextWord(_caret));
            else if (_selectStart != _selectEnd) Collapse(true);
            else _caret = MoveNextWord(_caret);
            Clamp();
            return;
        }
        if (shift) Extend(_caret < _text.Length ? _caret + 1 : _text.Length);
        else if (_selectStart != _selectEnd) Collapse(true);
        else _caret++;
        Clamp();
    }

    void MoveLine(bool start, bool shift) {
        _preferredX = -1;
        if (shift) {
            var dest = start ? (_singleLine ? 0 : LineStartIndex(_caret)) : (_singleLine ? _text.Length : LineEndIndex(_caret));
            Extend(dest);
            return;
        }
        // End collapses to the selection start first, then walks, same as the editor.
        if (_selectStart != _selectEnd) Collapse(false);
        _caret = start ? (_singleLine ? 0 : LineStartIndex(_caret)) : (_singleLine ? _text.Length : LineEndIndex(_caret));
        _selectStart = _selectEnd = _caret;
    }

    void MoveText(bool start, bool shift) {
        _preferredX = -1;
        var dest = start ? 0 : _text.Length;
        if (shift) Extend(dest);
        else _caret = _selectStart = _selectEnd = dest;
    }

    void MoveVert(bool down, bool shift) {
        if (_singleLine) {
            if (down) MoveRight(shift, false);
            else MoveLeft(shift, false);
            return;
        }
        if (!shift && _selectStart != _selectEnd) Collapse(down);
        var col = _preferredX >= 0 ? _preferredX : MeasurePrefix(_caret);
        _preferredX = col;
        int dest;
        if (!down) {
            var start = LineStartIndex(_caret);
            if (start == 0) dest = 0;
            else dest = IndexInLine(LineStartIndex(start - 1), LineEndIndex(start - 1), col);
        }
        else {
            var next = LineEndIndex(_caret);
            dest = next >= _text.Length ? _text.Length : IndexInLine(next + 1, LineEndIndex(next + 1), col);
        }
        if (shift) Extend(dest);
        else {
            _caret = dest;
            _selectStart = _selectEnd = _caret;
        }
    }

    void Extend(int dest) {
        if (_selectStart == _selectEnd) _selectStart = _selectEnd = _caret;
        _caret = _selectEnd = dest;
        Clamp();
    }

    void Collapse(bool toEnd) {
        var a = Math.Min(_selectStart, _selectEnd);
        var b = Math.Max(_selectStart, _selectEnd);
        _caret = _selectStart = _selectEnd = toEnd ? b : a;
    }

    int MovePrevWord(int c) {
        c--;
        while (c >= 0 && !WordBoundary(c)) c--;
        return c < 0 ? 0 : c;
    }

    int MoveNextWord(int c) {
        var len = _text.Length;
        c++;
        while (c < len && !WordBoundary(c)) c++;
        return c > len ? len : c;
    }

    bool WordBoundary(int idx) {
        if (idx <= 0) return true;
        if (idx >= _text.Length) return true;
        return char.IsWhiteSpace(_text[idx - 1]) && !char.IsWhiteSpace(_text[idx]);
    }

    int LineStartIndex(int index) {
        if (index > _text.Length) index = _text.Length;
        while (index > 0 && _text[index - 1] != '\n') index--;
        return index;
    }

    int LineEndIndex(int index) {
        if (index < 0) index = 0;
        while (index < _text.Length && _text[index] != '\n') index++;
        return index;
    }

    void Place(int absX, int absY) {
        Screen(out var sx, out var sy);
        _caret = _selectStart = _selectEnd = IndexAt(absX - sx, absY - sy);
        _preferredX = -1;
    }

    int IndexAt(int localX, int localY) {
        if (!Multiline) return IndexInLine(0, _text.Length, localX);
        var line = localY < 0 ? 0 : localY / LineH;
        var start = 0;
        var n = 0;
        for (var i = 0; i <= _text.Length; i++) {
            if (i != _text.Length && _text[i] != '\n') continue;
            if (n == line) return IndexInLine(start, i, localX);
            if (i == _text.Length) break;
            start = i + 1;
            n++;
        }
        return _text.Length;
    }

    int IndexInLine(int start, int end, int localX) {
        if (localX <= 0 || start >= end) return localX <= 0 ? start : end;
        var prev = 0;
        for (var i = start; i < end; i++) {
            var w = Measure(_text.Substring(start, i - start + 1));
            var mid = prev + ((w - prev) / 2);
            if (localX < mid) return i;
            prev = w;
        }
        return end;
    }

    int MeasurePrefix(int index) {
        var start = _singleLine ? 0 : LineStartIndex(index);
        if (index < start) return 0;
        return Measure(_text.Substring(start, index - start));
    }

    void CaretPos(out int cx, out int cy) {
        var index = _caret;
        if (index < 0) index = 0;
        if (index > _text.Length) index = _text.Length;
        var line = 0;
        var start = 0;
        if (Multiline) {
            for (var i = 0; i < index; i++) {
                if (_text[i] != '\n') continue;
                start = i + 1;
                line++;
            }
        }
        cx = Measure(_text.Substring(start, index - start));
        cy = line * LineH;
    }

    void DrawLine(int x, int y, string text, int start, int end) {
        var slice = end > start ? text.Substring(start, end - start) : "";
        var a = _selectStart;
        var b = _selectEnd;
        if (a > b) { var t = a; a = b; b = t; }
        if (!AllowSelection || a >= b || b <= start || a >= end) {
            if (slice.Length > 0) GumpArt.Sink?.DrawText(x, y, slice, Hue, Font);
            return;
        }
        var sa = Math.Max(a, start);
        var sb = Math.Min(b, end);
        var lead = text.Substring(start, sa - start);
        var mid = text.Substring(sa, sb - sa);
        var tail = text.Substring(sb, end - sb);
        var dx = x;
        if (lead.Length > 0) {
            GumpArt.Sink?.DrawText(dx, y, lead, Hue, Font);
            dx += Measure(lead);
        }
        if (mid.Length > 0) {
            GumpArt.Sink?.DrawText(dx, y, mid, 0x0035, Font);
            dx += Measure(mid);
        }
        if (tail.Length > 0) GumpArt.Sink?.DrawText(dx, y, tail, Hue, Font);
    }

    void FocusNext() {
        var root = (Control)this;
        while (root.Parent != null) root = root.Parent;
        var list = new List<StbTextBox>();
        Collect(root, list);
        if (list.Count == 0) return;
        var i = list.IndexOf(this);
        list[(i < 0 ? 0 : i + 1) % list.Count].SetKeyboardFocus();
    }

    static void Collect(Control c, List<StbTextBox> list) {
        if (c == null || c.IsDisposed || !c.IsVisible) return;
        if (c is StbTextBox box && box.AcceptKeyboardInput) list.Add(box);
        var children = c.Children;
        for (var i = 0; i < children.Count; i++) Collect(children[i], list);
    }

    void Screen(out int sx, out int sy) {
        sx = sy = 0;
        for (Control p = this; p != null; p = p.Parent) {
            sx += p.X;
            sy += p.Y;
        }
    }

    int Measure(string s) => string.IsNullOrEmpty(s) ? 0 : GumpArt.Sink?.MeasureText(s, Font) ?? s.Length * 6;

    bool AtMax(int count) => _maxCharCount >= 0 && _text.Length + count >= _maxCharCount;

    void Clamp() {
        if (_text == null) _text = "";
        if (_caret < 0) _caret = 0;
        if (_caret > _text.Length) _caret = _text.Length;
        if (_selectStart < 0) _selectStart = 0;
        if (_selectEnd < 0) _selectEnd = 0;
        if (_selectStart > _text.Length) _selectStart = _text.Length;
        if (_selectEnd > _text.Length) _selectEnd = _text.Length;
    }

    string ClipText(string s) {
        if (string.IsNullOrEmpty(s)) return "";
        s = s.Replace("\r\n", "\n").Replace('\r', '\n');
        if (!Multiline) s = s.Replace("\n", "");
        return s;
    }

    static ushort HueOf(string s) {
        if (string.IsNullOrEmpty(s)) return 1;
        var style = NumberStyles.Integer;
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
            s = s.Substring(2);
            style = NumberStyles.HexNumber;
        }
        else if (s.Length > 1 && s[0] == '-' && short.TryParse(s, out var neg))
            return (ushort)((ushort)neg + 1);
        uint.TryParse(s, style, CultureInfo.InvariantCulture, out var v);
        return (ushort)((ushort)v + 1);
    }

    readonly struct Snap {
        public readonly string Text;
        public readonly int Caret, Start, End;
        public Snap(string text, int caret, int start, int end) {
            Text = text ?? "";
            Caret = caret;
            Start = start;
            End = end;
        }
    }

    public class BeforeTextChangedEventArgs : EventArgs {
        public BeforeTextChangedEventArgs(string previousText, string newText, int newCaretIndex) {
            PreviousText = previousText;
            NewText = newText;
            NewCaretIndex = newCaretIndex;
        }

        public string PreviousText { get; }
        public string NewText { get; set; }
        public int NewCaretIndex { get; set; }
    }
}
