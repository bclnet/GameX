// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO chat. One window for the session: a later 0xB2 updates it.
/// The top bar shows that window. When it is not open, the button sends 0xB5 with an empty name.
/// 0x03EB asks for the chat name. 0x03ED shows the channel list and joins General.
/// 0x03E8 adds a channel and 0x03E9 removes one. 0x03EC closes and clears that list.
/// 0x03F1, 0x03F4, 0x0025, 0x0026, 0x0027, and the Chat.enu commands append a line.
/// Join, leave, create, the name, and a chat line use the 0xB3 and 0xB5 sends. Language is ENU.
/// No fill, border, scroll bar, or hover bar.
/// </summary>
public sealed class ChatGump : Gump {
    const string Language = "ENU";
    const ushort Frame = 0x0A28;
    const ushort ChatHue = 0x0256;
    const int ChannelW = 345;
    const int ChannelH = 390;
    const int ListY = 65;
    const int ListBottom = 265;

    // Chat.enu sentences ClassicUO keeps on ChatManager, index cmd - 1.
    static readonly string[] Notices = {
        "You are already ignoring the maximum number of people.",
        "You are already ignoring %1.",
        "You are now ignoring %1.",
        "You are no longer ignoring %1.",
        "You are not ignoring %1.",
        "You are no longer ignoring anyone.",
        "That is not a valid conference name.",
        "There is already a conference of that name.",
        "You must have operator status to do this.",
        "Conference %1 renamed to %2.",
        "You must be in a conference to do this. To join a conference, select one from the Conference menu.",
        "There is no player named '%1'.",
        "There is no conference named '%1'.",
        "That is not the correct password.",
        "%1 has chosen to ignore you. None of your messages to them will get through.",
        "The moderator of this conference has not given you speaking privileges.",
        "You can now receive private messages.",
        "You will no longer receive private messages. Those who send you a message will be notified that you are blocking incoming messages.",
        "You are now showing your character name to any players who inquire with the whois command.",
        "You are no longer showing your character name to any players who inquire with the whois command.",
        "%1 is remaining anonymous.",
        "%1 has chosen to not receive private messages at the moment.",
        "%1 is known in the lands of Britannia as %2.",
        "%1 has been kicked out of the conference.",
        "%1, a conference moderator, has kicked you out of the conference.",
        "You are already in the conference '%1'.",
        "%1 is no longer a conference moderator.",
        "%1 is now a conference moderator.",
        "%1 has removed you from the list of conference moderators.",
        "%1 has made you a conference moderator.",
        "%1 no longer has speaking privileges in this conference.",
        "%1 now has speaking privileges in this conference.",
        "%1, a conference moderator, has removed your speaking privileges for this conference.",
        "%1, a conference moderator, has granted you speaking privileges in this conference.",
        "From now on, everyone in the conference will have speaking privileges by default.",
        "From now on, only moderators will have speaking privileges in this conference by default.",
        "The password to the conference has been changed.",
        "Sorry--the conference named '%1' is full and no more players are allowed in.",
        "You are banning %1 from this conference.",
        "%1, a conference moderator, has banned you from the conference.",
        "You have been banned from this conference.",
    };

    static readonly List<string> _channels = [];
    static readonly List<string> _lines = [];
    static string _current = "";

    readonly ResizePic _frame;
    readonly Label _channelsTitle, _currentTitle, _currentName, _join, _leave, _create;
    readonly Button _joinBtn, _leaveBtn, _createBtn;
    readonly TextBox _say;
    readonly List<ChannelRow> _rows = [];
    readonly List<Label> _log = [];
    NamePrompt _name;
    CreatePrompt _createBox;
    string _selected;
    bool _listing;

    public ChatGump(World world) : base(world, 0, 0) {
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        WantUpdateSize = false;
        Width = ChannelW;
        Height = ChannelH;
        _frame = new ResizePic(Frame) { Width = ChannelW, Height = ChannelH };
        Keep(_frame);
        _channelsTitle = Place("Channels", 25);
        _currentTitle = Place("Your current channel:", 275);
        _currentName = Place(_current, 300);
        _joinBtn = new Button(0, 0x0845, 0x0846, 0x0845) { X = 48, Y = 342, ButtonAction = ButtonAction.Activate };
        _leaveBtn = new Button(1, 0x0845, 0x0846, 0x0845) { X = 123, Y = 342, ButtonAction = ButtonAction.Activate };
        _createBtn = new Button(2, 0x0845, 0x0846, 0x0845) { X = 216, Y = 342, ButtonAction = ButtonAction.Activate };
        Keep(_joinBtn);
        Keep(_leaveBtn);
        Keep(_createBtn);
        _join = new Label("Join", false, 0x0386, 2) { X = 65, Y = 337 };
        _leave = new Label("Leave", false, 0x0386, 2) { X = 140, Y = 337 };
        _create = new Label("Create", false, 0x0386, 2) { X = 233, Y = 337 };
        Add(_join);
        Add(_leave);
        Add(_create);
        // ClassicUO types this on the system chat after a comma. That box is not this gump.
        _say = new TextBox { X = 48, Y = 366, Width = 250, Height = 18, Hue = ChatHue, MaxLength = -1 };
        Add(_say);
    }

    public static void OnMessage(World world, UOChatMessage message) {
        switch (message.Command) {
            case 0x03E8:
                AddChannel(message.Name);
                UIManager.GetGump<ChatGump>()?.Fill();
                return;
            case 0x03E9:
                RemoveChannel(message.Name);
                UIManager.GetGump<ChatGump>()?.Fill();
                return;
            case 0x03EB:
                Open(world).ShowName();
                return;
            case 0x03EC:
                _channels.Clear();
                _lines.Clear();
                _current = "";
                UIManager.GetGump<ChatGump>()?.Dispose();
                return;
            case 0x03ED:
                Open(world).ShowList();
                UOLoginNet.Send(OutgoingPackets.Send_ChatJoinCommand(Language, "General"));
                return;
            case 0x03EE:
            case 0x03EF:
            case 0x03F0:
                return;
            case 0x03F1:
                _current = message.Name ?? "";
                UIManager.GetGump<ChatGump>()?.UpdateConference();
                Append($"You have joined the '{message.Name}' channel.");
                return;
            case 0x03F4:
                Append($"You have left the '{message.Name}' channel.");
                return;
            case 0x0025:
            case 0x0026:
            case 0x0027:
                Append($"{message.Name}: {message.Text}");
                return;
            default:
                if (IsNotice(message.Command)) {
                    var msg = Format(message.Command, message.Text);
                    if (!string.IsNullOrEmpty(msg)) Append(msg);
                }
                return;
        }
    }

    public override void OnButtonClick(int buttonID) {
        switch (buttonID) {
            case 0: Join(); break;
            case 1: UOLoginNet.Send(OutgoingPackets.Send_ChatLeaveChannelCommand(Language)); break;
            case 2: OpenCreate(); break;
        }
    }

    public override void OnKeyboardReturn(int textID, string text) {
        if (string.IsNullOrEmpty(text)) return;
        UOLoginNet.Send(OutgoingPackets.Send_ChatMessageCommand(Language, text));
        if (!_say.IsDisposed) _say.Text = "";
    }

    public override void CloseWithRightClick() {
        if (!CanCloseWithRightClick || IsDisposed) return;
        if (_createBox != null && !_createBox.IsDisposed) {
            CloseCreate();
            return;
        }
        if (_name != null && !_name.IsDisposed) {
            CancelName();
            return;
        }
        base.CloseWithRightClick();
    }

    public static void Toggle(World world) {
        if (UIManager.GetGump<ChatGump>() != null) {
            Open(world);
            return;
        }
        UOLoginNet.Send(OutgoingPackets.Send_OpenChat(""));
    }

    static ChatGump Open(World world) {
        var open = UIManager.GetGump<ChatGump>();
        if (open != null) return open;
        open = new ChatGump(world);
        UIManager.Add(open);
        return open;
    }

    static void AddChannel(string name) {
        name ??= "";
        for (var i = 0; i < _channels.Count; i++)
            if (_channels[i] == name) return;
        _channels.Add(name);
    }

    static void RemoveChannel(string name) {
        for (var i = 0; i < _channels.Count; i++)
            if (_channels[i] == name) {
                _channels.RemoveAt(i);
                return;
            }
    }

    static void Append(string text) {
        _lines.Add(text ?? "");
        UIManager.GetGump<ChatGump>()?.Fill();
    }

    static bool IsNotice(ushort cmd) =>
        (cmd >= 0x0001 && cmd <= 0x0024) || (cmd >= 0x0028 && cmd <= 0x002C);

    static string Format(ushort cmd, string text) {
        var index = cmd - 1;
        if (index < 0 || index >= Notices.Length) return "";
        var msg = Notices[index];
        if (string.IsNullOrEmpty(msg) || string.IsNullOrEmpty(text)) return msg;
        if (msg.IndexOf("%1", StringComparison.Ordinal) >= 0) msg = msg.Replace("%1", text);
        if ((index == 0x000A || index == 0x0017) && msg.IndexOf("%2", StringComparison.Ordinal) >= 0)
            msg = msg.Replace("%2", text);
        return msg;
    }

    void ShowName() {
        if (_name != null && !_name.IsDisposed) return;
        _name = new NamePrompt(this);
        Add(_name);
        if (!_listing) FitName();
    }

    void ShowList() {
        _listing = true;
        CanMove = true;
        Width = ChannelW;
        Height = ChannelH;
        if (!_frame.IsDisposed) {
            _frame.Width = ChannelW;
            _frame.Height = ChannelH;
        }
        CloseName();
        SetChrome(true);
        UpdateConference();
        Fill();
    }

    void FitName() {
        CanMove = false;
        X = 250;
        Y = 100;
        Width = 210;
        Height = 330;
        if (!_frame.IsDisposed) {
            _frame.Width = 210;
            _frame.Height = 330;
        }
        SetChrome(false);
    }

    void UpdateConference() {
        if (_currentName.IsDisposed) return;
        _currentName.Text = _current ?? "";
        _currentName.X = Math.Max(0, (Width - _currentName.Width) / 2);
    }

    void Fill() {
        Drop(_rows);
        Drop(_log);
        var y = ListY;
        for (var i = 0; i < _channels.Count; i++) {
            var row = new ChannelRow(this, _channels[i]) { X = 67, Y = y, Width = 195, IsVisible = _listing };
            if (row.Text == _selected) row.Hue = 0x22;
            Add(row);
            _rows.Add(row);
            y += 16;
        }
        var count = _lines.Count > 4 ? 4 : _lines.Count;
        var logY = y + 8;
        var limit = ListBottom - count * 16;
        if (logY > limit) logY = limit;
        if (logY < ListY) logY = ListY;
        var start = _lines.Count - count;
        for (var i = 0; i < count; i++) {
            var label = new Label(_lines[start + i], true, ChatHue, 1) { X = 67, Y = logY, IsVisible = _listing };
            Add(label);
            _log.Add(label);
            logY += 16;
        }
        Lift(_say);
        Lift(_createBox);
        Lift(_name);
    }

    void Select(string name) {
        _selected = name;
        foreach (var row in _rows)
            if (!row.IsDisposed) row.Hue = row.Text == name ? (ushort)0x22 : (ushort)0x49;
    }

    void Join() {
        if (!string.IsNullOrEmpty(_selected))
            UOLoginNet.Send(OutgoingPackets.Send_ChatJoinCommand(Language, _selected));
    }

    void OpenCreate() {
        if (_createBox != null && !_createBox.IsDisposed) return;
        _createBox = new CreatePrompt(this) {
            X = (Width - 200) / 2,
            Y = (Height - 60) / 2,
        };
        Add(_createBox);
    }

    void AcceptCreate(string text) {
        UOLoginNet.Send(OutgoingPackets.Send_ChatCreateChannelCommand(Language, text ?? ""));
        CloseCreate();
    }

    void CloseCreate() {
        if (_createBox == null || _createBox.IsDisposed) {
            _createBox = null;
            return;
        }
        _createBox.Parent = null;
        _createBox.Dispose();
        _createBox = null;
    }

    void AcceptName(string text) {
        if (!string.IsNullOrWhiteSpace(text))
            UOLoginNet.Send(OutgoingPackets.Send_OpenChat(text));
        CloseName();
    }

    void CancelName() => CloseName();

    void CloseName() {
        if (_name == null || _name.IsDisposed) {
            _name = null;
            if (!_listing && !IsDisposed) Dispose();
            return;
        }
        _name.Parent = null;
        _name.Dispose();
        _name = null;
        if (!_listing) Dispose();
    }

    void SetChrome(bool on) {
        Show(_channelsTitle, on);
        Show(_currentTitle, on);
        Show(_currentName, on);
        Show(_join, on);
        Show(_leave, on);
        Show(_create, on);
        Show(_joinBtn, on);
        Show(_leaveBtn, on);
        Show(_createBtn, on);
        Show(_say, on);
        foreach (var row in _rows) Show(row, on);
        foreach (var line in _log) Show(line, on);
    }

    Label Place(string text, int y) {
        var label = new Label(text ?? "", false, 0x0386, 2) { Y = y };
        label.X = Math.Max(0, (ChannelW - label.Width) / 2);
        Add(label);
        return label;
    }

    void Keep(Control c) {
        if (c != null && !c.IsDisposed) Add(c);
    }

    void Lift(Control c) {
        if (c == null || c.IsDisposed) return;
        c.Parent = null;
        Add(c);
    }

    static void Show(Control c, bool on) {
        if (c != null && !c.IsDisposed) c.IsVisible = on;
    }

    static void Drop<T>(List<T> list) where T : Control {
        foreach (var c in list) {
            c.Parent = null;
            c.Dispose();
        }
        list.Clear();
    }

    sealed class ChannelRow : Label {
        readonly ChatGump _owner;

        public ChannelRow(ChatGump owner, string text) : base(text, false, 0x49, 3) {
            _owner = owner;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) => _owner.Select(Text);

        public override bool OnMouseDoubleClick(int x, int y) {
            _owner.Select(Text);
            _owner.Join();
            return true;
        }
    }

    sealed class NamePrompt : Control {
        const string Choose =
            "You are about to choose your name for the Ultima Online chat system. This name will be PERMANENT and unique on this shard. It will apply to all characters using this account. Do not use your Ultima Online account name for this name as the name you choose will be public. Enter your chat name here:";

        readonly ChatGump _owner;
        readonly TextBox _box;

        public NamePrompt(ChatGump owner) {
            _owner = owner;
            AcceptMouseInput = true;
            Width = 210;
            Height = 330;
            var y = 6;
            var i = 0;
            const int max = 34;
            while (i < Choose.Length) {
                var n = Math.Min(max, Choose.Length - i);
                if (i + n < Choose.Length) {
                    var sp = Choose.LastIndexOf(' ', i + n - 1, n);
                    if (sp > i) n = sp - i;
                }
                if (n <= 0) n = 1;
                Add(new Label(Choose.Substring(i, n).Trim(), true, 23, 3) { X = 6, Y = y });
                y += 16;
                i += n;
                while (i < Choose.Length && Choose[i] == ' ') i++;
            }
            y += 8;
            var name = new Label("Name", true, 0x033, 3) { X = 6, Y = y };
            Add(name);
            var boxX = name.X + name.Width + 2;
            var boxW = 210 - boxX - 17;
            if (boxW < 40) boxW = 40;
            _box = new TextBox { X = boxX, Y = y, Width = boxW, Height = 20, Hue = 0x0481, MaxLength = 30 };
            Add(_box);
            var close = new Button(0, 0x0A94, 0x0A95, 0x0A94) {
                X = Width - 19 - 4,
                Y = Height - 19 - 4,
                ButtonAction = ButtonAction.Activate,
            };
            var ok = new Button(1, 0x0A9A, 0x0A9B, 0x0A9A) {
                X = Width - 19 * 2 - 4,
                Y = Height - 19 - 4,
                ButtonAction = ButtonAction.Activate,
            };
            if (!close.IsDisposed) Add(close);
            if (!ok.IsDisposed) Add(ok);
            _box.SetKeyboardFocus();
        }

        public override void OnButtonClick(int buttonID) {
            if (buttonID == 1) _owner.AcceptName(_box.Text);
            else _owner.CancelName();
        }

        public override void OnKeyboardReturn(int textID, string text) => _owner.AcceptName(text);
    }

    sealed class CreatePrompt : Control {
        readonly ChatGump _owner;
        readonly TextBox _box;

        public CreatePrompt(ChatGump owner) {
            _owner = owner;
            CanMove = true;
            AcceptMouseInput = true;
            Width = 200;
            Height = 60;
            Add(new Label("Create a channel:", true, 0x23, 1) { X = 6, Y = 3 });
            Add(new Label("Name", true, 0x23, 1) { X = 6, Y = 25 });
            _box = new TextBox { X = 45, Y = 25, Width = 150, Height = 16, Hue = 0x0481, MaxLength = -1 };
            Add(_box);
            var close = new Button(0, 0x0A94, 0x0A95, 0x0A94) {
                X = Width - 19 - 3,
                Y = Height - 19 + 6,
                ButtonAction = ButtonAction.Activate,
            };
            var ok = new Button(1, 0x0A9A, 0x0A9B, 0x0A9A) {
                X = Width - 19 * 2 - 3,
                Y = Height - 19 + 6,
                ButtonAction = ButtonAction.Activate,
            };
            if (!close.IsDisposed) Add(close);
            if (!ok.IsDisposed) Add(ok);
            _box.SetKeyboardFocus();
        }

        public override void OnButtonClick(int buttonID) {
            if (buttonID == 1) _owner.AcceptCreate(_box.Text);
            else _owner.CloseCreate();
        }

        public override void OnKeyboardReturn(int textID, string text) => _owner.AcceptCreate(text);
    }
}
