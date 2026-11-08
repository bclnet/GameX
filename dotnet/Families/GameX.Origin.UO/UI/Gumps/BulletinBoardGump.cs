// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO BulletinBoardGump. Command 0 is the board, 0x087A, name centered at 159, 36.
/// Command 1 is a summary row, 0x1523, under the list at 127, 159. A row is 18 tall.
/// Command 2 is the message. The hit at 15, 170 opens a new post.
/// Post, remove, and the row request are not sent. Reply only opens the local composer.
/// The scroll on a message is 0x0820 at height 408. Author, Date, and Title use the ClassicUO words.
/// </summary>
public sealed class BulletinBoardGump : Gump {
    readonly Label _name;
    readonly List<BoardRow> _rows = [];

    public BulletinBoardGump(World world, uint serial, string name) : base(world, serial, serial) {
        WorldGump.ViewCenter(out var cx, out var cy);
        X = cx - 245;
        Y = cy - 205;
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        Add(new GumpPic(0, 0, 0x087A, 0));
        _name = new Label("", true, 1, 1) { Y = 36 };
        Add(_name);
        SetName(name);
        Add(new PostHit(this));
    }

    public static void Open(World world, UOBulletin row) {
        switch (row.Command) {
            case 0: OpenBoard(world, row); break;
            case 1: Summary(row); break;
            case 2: Message(world, row); break;
        }
    }

    public override void Dispose() {
        var extra = new List<Gump>();
        foreach (var gump in UIManager.Gumps)
            if (gump is BulletinBoardItem) extra.Add(gump);
        foreach (var gump in extra) gump.Dispose();
        base.Dispose();
    }

    public void Compose() {
        if (UIManager.GetGump<BulletinBoardItem>(0) != null) return;
        var name = UOLoginNet.NameOf(UOLoginNet.PlayerSerial);
        UIManager.Add(new BulletinBoardItem(World, ServerSerial, 0, name, "", "Date/Time", "", 0) { X = 400, Y = 335 });
    }

    static void OpenBoard(World world, UOBulletin row) {
        var open = UIManager.GetGump<BulletinBoardGump>(row.Board);
        if (open == null) UIManager.Add(new BulletinBoardGump(world, row.Board, row.Name));
        else {
            open.SetName(row.Name);
            open.ClearRows();
        }
    }

    static void Summary(UOBulletin row) {
        var open = UIManager.GetGump<BulletinBoardGump>(row.Board);
        if (open == null) return;
        open.AddObject(row.Serial, row.Poster + " - " + row.Subject + " - " + row.Time);
    }

    static void Message(World world, UOBulletin row) {
        if (UIManager.GetGump<BulletinBoardGump>(row.Board) == null) return;
        var body = row.Lines == null || row.Lines.Length == 0 ? "" : string.Join("\n", row.Lines);
        var mine = UOLoginNet.NameOf(UOLoginNet.PlayerSerial);
        var variant = (byte)(1 + (row.Poster == mine ? 1 : 0));
        var open = UIManager.GetGump<BulletinBoardItem>(row.Serial);
        if (open == null) {
            UIManager.Add(new BulletinBoardItem(world, row.Board, row.Serial, row.Poster, row.Subject, row.Time, body, variant) { X = 40, Y = 40 });
        }
        else open.Show(row.Poster, row.Subject, row.Time, body, variant);
    }

    void SetName(string name) {
        _name.Text = name ?? "";
        _name.X = 159 + (170 - _name.Width) / 2;
        _name.Y = 36;
    }

    void ClearRows() {
        foreach (var row in _rows) {
            row.Parent = null;
            row.Dispose();
        }
        _rows.Clear();
    }

    void AddObject(uint serial, string text) {
        for (var i = 0; i < _rows.Count; i++) {
            if (_rows[i].Serial != serial) continue;
            _rows[i].Parent = null;
            _rows[i].Dispose();
            _rows.RemoveAt(i);
            break;
        }
        var row = new BoardRow(serial, text);
        _rows.Add(row);
        Add(row);
        Reflow();
    }

    void Reflow() {
        for (var i = 0; i < _rows.Count; i++) {
            _rows[i].X = 127;
            _rows[i].Y = 159 + i * 18;
        }
    }

    sealed class PostHit : Control {
        readonly BulletinBoardGump _owner;

        public PostHit(BulletinBoardGump owner) {
            _owner = owner;
            X = 15;
            Y = 170;
            Width = 80;
            Height = 80;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) => _owner.Compose();
    }

    sealed class BoardRow : Control {
        public BoardRow(uint serial, string text) {
            Serial = serial;
            CanMove = true;
            Width = 230;
            Height = 18;
            WantUpdateSize = false;
            var pic = new GumpPic(0, 0, 0x1523, 0) { AcceptMouseInput = false };
            if (!pic.IsDisposed) Add(pic);
            Add(new Label(text ?? "", true, 0, 1) { X = 23, Y = 1 });
        }

        public uint Serial { get; }
    }
}

/// <summary>
/// ClassicUO BulletinBoardItem. Variant 0 posts (0x0883, 0x0886), 1 replies (0x0884), 2 removes (0x0885).
/// ServerSerial is the message. The board serial is stored beside it, because GetGump matches ServerSerial.
/// </summary>
public sealed class BulletinBoardItem : Gump {
    const ushort Scroll = 0x0820;
    const int Special = 408;

    readonly GumpPic _top, _bottom, _head;
    readonly GumpPicTiled _right, _mid, _rule;
    readonly Button _expander, _post, _reply, _remove;
    readonly TextBox _subject;
    readonly List<Label> _lines = [];
    readonly string _time;
    string _subjectText;

    public BulletinBoardItem(World world, uint board, uint message, string poster, string subject, string time, string body, byte variant)
        : base(world, board, message) {
        BoardSerial = board;
        _time = time ?? "";
        Height = Special;
        AcceptKeyboardInput = true;
        CanMove = true;
        CanCloseWithRightClick = true;
        _top = new GumpPic(0, 0, Scroll, 0);
        _right = new GumpPicTiled((ushort)(Scroll + 1));
        _mid = new GumpPicTiled((ushort)(Scroll + 2));
        _bottom = new GumpPic(0, 0, (ushort)(Scroll + 3), 0);
        _expander = new Button(0x7FBEEF, 0x082E, 0x082F) { ButtonAction = ButtonAction.Activate };
        _head = new GumpPic(97, 12, 0x0883, 0);
        Keep(_top);
        Keep(_right);
        Keep(_mid);
        Keep(_bottom);
        Keep(_expander);
        _rule = new GumpPicTiled(30, 106, 235, 4, 0x0835);
        if (!_rule.IsDisposed) Add(_rule);
        _subject = new TextBox { Width = 150, Height = 16, MaxLength = -1 };
        Add(_subject);
        _post = new Button((int)ItemButton.Post, 0x0886, 0x0886) { X = 37, Y = Special - 50, ButtonAction = ButtonAction.Activate };
        _reply = new Button((int)ItemButton.Reply, 0x0884, 0x0884) { X = 37, Y = Special - 50, ButtonAction = ButtonAction.Activate };
        _remove = new Button((int)ItemButton.Remove, 0x0885, 0x0885) { X = 235, Y = Special - 50, ButtonAction = ButtonAction.Activate };
        Keep(_post);
        Keep(_reply);
        Keep(_remove);
        Show(poster, subject, time, body, variant);
    }

    public uint BoardSerial { get; }

    public override void Update() {
        Place();
        var y = Height > 50 ? Height - 50 : Special - 50;
        if (_post != null && !_post.IsDisposed) _post.Y = y;
        if (_reply != null && !_reply.IsDisposed) _reply.Y = y;
        if (_remove != null && !_remove.IsDisposed) _remove.Y = y;
        base.Update();
    }

    public override void OnButtonClick(int buttonID) {
        if (_subject == null) return;
        switch ((ItemButton)buttonID) {
            case ItemButton.Post:
            case ItemButton.Remove:
                break;
            case ItemButton.Reply:
                var name = UOLoginNet.NameOf(UOLoginNet.PlayerSerial);
                var next = new BulletinBoardItem(World, BoardSerial, ServerSerial, name, "RE: " + _subject.Text, _time, "", 0) { X = 400, Y = 335 };
                Dispose();
                UIManager.Add(next);
                break;
        }
    }

    public void Show(string poster, string subject, string time, string body, byte variant) {
        _subjectText = subject ?? "";
        WantUpdateSize = true;
        foreach (var line in _lines) {
            line.Parent = null;
            line.Dispose();
        }
        _lines.Clear();
        const ushort hue = 0;
        var author = new Label("Author:", true, hue, 1) { X = 30, Y = 40 };
        Add(author);
        _lines.Add(author);
        var who = new Label(poster ?? "", true, hue, 1) { X = 30 + author.Width, Y = 40 };
        Add(who);
        _lines.Add(who);
        var date = new Label("Date:", true, hue, 1) { X = 30, Y = 58 };
        Add(date);
        _lines.Add(date);
        var when = new Label(time ?? "", true, hue, 1) { X = 32 + date.Width, Y = 58 };
        Add(when);
        _lines.Add(when);
        var title = new Label("Title:", true, hue, 1) { X = 30, Y = 77 };
        Add(title);
        _lines.Add(title);
        _subject.Hue = variant == 0 ? (ushort)0x0008 : hue;
        _subject.X = 30 + title.Width;
        _subject.Y = 77;
        _subject.Text = _subjectText;
        _subject.AcceptKeyboardInput = variant == 0;
        var text = (body ?? "").Replace("\r", "");
        if (text.Length > 0) {
            var y = 120;
            foreach (var part in text.Split('\n')) {
                var label = new Label(part, true, hue, 1) { X = 40, Y = y };
                Add(label);
                _lines.Add(label);
                y += 16;
            }
        }
        var post = variant == 0;
        var reply = variant == 1;
        var remove = variant == 2;
        Show(_head, post);
        Show(_post, post);
        Show(_reply, reply);
        Show(_remove, remove);
        Place();
    }

    static void Show(Control c, bool on) {
        if (c == null || c.IsDisposed) return;
        c.IsVisible = on;
    }

    void Keep(Control c) {
        if (c != null && !c.IsDisposed) Add(c);
    }

    void Place() {
        GumpMetrics.TrySize(Scroll, out var w0, out var h0);
        GumpMetrics.TrySize((ushort)(Scroll + 1), out var w1, out _);
        GumpMetrics.TrySize((ushort)(Scroll + 2), out var w2, out _);
        GumpMetrics.TrySize((ushort)(Scroll + 3), out var w3, out var h3);
        GumpMetrics.TrySize(0x082E, out var ew, out var eh);
        var width = w0;
        if (w1 > width) width = w1;
        if (w2 > width) width = w2;
        if (w3 > width) width = w3;
        var midH = Special - h0 - h3 - eh;
        if (midH < 0) midH = 0;
        var midY = h0;
        if (_top != null && !_top.IsDisposed) { _top.X = 0; _top.Y = 0; }
        if (_right != null && !_right.IsDisposed) {
            _right.X = (width - w1) / 2;
            _right.Y = midY;
            if (w1 > 0) _right.Width = w1;
            _right.Height = midH;
        }
        if (_mid != null && !_mid.IsDisposed) {
            _mid.X = (width - w1) / 2;
            _mid.Y = midY;
            if (w2 > 0) _mid.Width = w2;
            _mid.Height = midH;
        }
        if (_bottom != null && !_bottom.IsDisposed) {
            var off = w0 - w3;
            _bottom.X = off / 2 + off / 4;
            _bottom.Y = Special - h3 - eh;
        }
        if (_expander != null && !_expander.IsDisposed) {
            var scrollW = w2 > 0 ? w2 : width;
            _expander.X = (scrollW - ew) >> 1;
            _expander.Y = Special - eh - 2;
        }
    }

    enum ItemButton { Post, Remove, Reply }
}
