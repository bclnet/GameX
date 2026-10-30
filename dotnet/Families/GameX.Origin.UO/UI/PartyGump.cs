// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO PartyGump. Poll keeps one gump while a leader or a member is stored.
/// Tell and the party line call Send_PartyMessage. Kick and leave call Send_PartyRemoveRequest.
/// Add calls Send_PartyInviteRequest. The loot button calls Send_PartyChangeLootTypeRequest.
/// A party message is a label on this gump, or a PartyNote when the party is empty.
/// </summary>
public sealed class PartyGump : Gump {
    const int OkId = 1;
    const int CancelId = 2;
    const int SendId = 3;
    const int LootId = 4;
    const int LeaveId = 5;
    const int AddId = 6;
    const int TellId = 7;
    const int KickId = 17;

    static readonly List<(uint serial, string name)> Seen = [];
    static bool _lootKnown;
    static bool _lootShown;

    readonly List<(uint serial, string name)> _members = [];
    readonly List<string> _notes = [];
    TextBox _box;
    string _draft = "";
    int _rev;

    public static void Poll() {
        var world = UIManager.GetGump<WorldGump>()?.World;
        if (world == null) return;
        var open = UIManager.GetGump<PartyGump>();
        if (!InParty()) {
            if (open != null && !open.IsDisposed) open.Dispose();
        }
        else if (open == null || open.IsDisposed) UIManager.Add(new PartyGump(world));
        else open.Apply();
        while (UOParty.TakeMessage(out var serial, out var text)) {
            var g = UIManager.GetGump<PartyGump>();
            if (g != null && !g.IsDisposed) g.ShowMessage(serial, text);
            else PartyNote.Show(world, serial, text);
        }
        if (!UOParty.TakeInvite(out var inviter)) return;
        var invite = UIManager.GetGump<PartyInviteGump>();
        if (invite != null && !invite.IsDisposed) invite.Dispose();
        UIManager.Add(new PartyInviteGump(world, inviter));
    }

    public PartyGump(World world) : base(world, 0, 0) {
        if (!_lootKnown) {
            _lootShown = UOParty.CanLoot;
            _lootKnown = true;
        }
        _rev = UOParty.Revision;
        X = 32;
        Y = 32;
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        Build();
    }

    public override void OnButtonClick(int buttonID) {
        if (buttonID == OkId || buttonID == CancelId) {
            Dispose();
            return;
        }
        if (buttonID == SendId) {
            SendText(0);
            return;
        }
        if (buttonID == LootId) {
            _lootShown = !_lootShown;
            UOLoginNet.Send(OutgoingPackets.Send_PartyChangeLootTypeRequest(_lootShown));
            Build();
            return;
        }
        if (buttonID == LeaveId) {
            if (UOParty.Leader == 0) ShowMessage(0, "You are not in a party.");
            else UOLoginNet.Send(OutgoingPackets.Send_PartyRemoveRequest(UOLoginNet.PlayerSerial));
            return;
        }
        if (buttonID == AddId) {
            var leader = UOParty.Leader;
            if (leader == 0 || leader == UOLoginNet.PlayerSerial)
                UOLoginNet.Send(OutgoingPackets.Send_PartyInviteRequest());
            return;
        }
        if (buttonID >= TellId && buttonID < TellId + 10) {
            var serial = Slot(buttonID - TellId);
            if (serial == 0) ShowMessage(0, "There is no one in that party slot.");
            else SendText(serial);
            return;
        }
        if (buttonID >= KickId && buttonID < KickId + 10) {
            var serial = Slot(buttonID - KickId);
            if (serial == 0) ShowMessage(0, "There is no one in that party slot.");
            else UOLoginNet.Send(OutgoingPackets.Send_PartyRemoveRequest(serial));
        }
    }

    public override void OnKeyboardReturn(int textID, string text) => SendText(0);

    static bool InParty() {
        if (UOParty.Leader != 0) return true;
        UOParty.CopyMembers(Seen);
        for (var i = 0; i < Seen.Count; i++)
            if (Seen[i].serial != 0) return true;
        return false;
    }

    static string Format(uint serial, string text) {
        var line = (text ?? "").Replace("\r", " ").Replace("\n", " ");
        if (serial == 0) return line;
        var who = UOLoginNet.NameOf(serial);
        return string.IsNullOrEmpty(who) ? line : who + ": " + line;
    }

    void Apply() {
        var rev = UOParty.Revision;
        if (rev == _rev) return;
        _rev = rev;
        Build();
    }

    void ShowMessage(uint serial, string text) {
        _notes.Add(Format(serial, text));
        if (_notes.Count > 8) _notes.RemoveAt(0);
        Build();
    }

    void SendText(uint to) {
        if (UOParty.Leader == 0) {
            ShowMessage(0, "You are not in a party.");
            return;
        }
        var text = _box != null && !_box.IsDisposed ? _box.Text : _draft;
        if (string.IsNullOrEmpty(text)) return;
        UOLoginNet.Send(OutgoingPackets.Send_PartyMessage(text, to));
    }

    uint Slot(int index) => index >= 0 && index < _members.Count ? _members[index].serial : 0;

    void Build() {
        _draft = _box != null && !_box.IsDisposed ? _box.Text : _draft;
        ClearChildren();
        _box = null;
        UOParty.CopyMembers(_members);
        var leader = UOParty.Leader;
        var player = UOLoginNet.PlayerSerial;
        var isLeader = leader == 0 || leader == player;
        var noteY = 462;
        var height = 462 + _notes.Count * 16 + 8;
        if (height < 480) height = 480;
        Keep(new ResizePic(0x0A28) { X = 0, Y = 0, Width = 450, Height = height });
        Keep(new Label("Tell", false, 0x0386, 1) { X = 40, Y = 30 });
        Keep(new Label("Kick", false, 0x0386, 1) { X = 80, Y = 30 });
        Keep(new Label("Party Manifest", false, 0x0386, 2) { X = 153, Y = 20 });
        var y = 48;
        for (var i = 0; i < 10; i++) {
            var tell = new Button(TellId + i, 0x0FAB, 0x0FAD, 0x0FAC) { X = 40, Y = y + 2, ButtonAction = ButtonAction.Activate };
            Keep(tell);
            if (isLeader) {
                var kick = new Button(KickId + i, 0x0FB1, 0x0FB3, 0x0FB2) { X = 80, Y = y + 2, ButtonAction = ButtonAction.Activate };
                Keep(kick);
            }
            Keep(new GumpPic(130, y, 0x0475, 0));
            var serial = i < _members.Count ? _members[i].serial : 0;
            var name = "";
            if (serial != 0) {
                name = _members[i].name;
                if (string.IsNullOrEmpty(name)) name = UOLoginNet.NameOf(serial);
            }
            Keep(new Label(name, false, 0x0386, 2) { X = 140, Y = y + 1 });
            y += 25;
        }
        Keep(new Button(SendId, 0x0FAB, 0x0FAD, 0x0FAC) { X = 70, Y = 307, ButtonAction = ButtonAction.Activate });
        Keep(new Label("Send the party a message", false, 0x0386, 2) { X = 110, Y = 307 });
        Keep(new ResizePic(0x0BB8) { X = 318, Y = 303, Width = 124, Height = 22 });
        _box = new TextBox { X = 322, Y = 305, Width = 116, Height = 18, Hue = 0x0481, MaxLength = 100 };
        _box.Text = _draft ?? "";
        Keep(_box);
        var lootGraphic = (ushort)(_lootShown ? 0x0FA2 : 0x0FA9);
        Keep(new Button(LootId, lootGraphic, lootGraphic, lootGraphic) { X = 70, Y = 334, ButtonAction = ButtonAction.Activate });
        Keep(new Label(_lootShown ? "Party can loot me" : "Party CANNOT loot me", false, 0x0386, 2) { X = 110, Y = 334 });
        Keep(new Button(LeaveId, 0x0FAE, 0x0FB0, 0x0FAF) { X = 70, Y = 360, ButtonAction = ButtonAction.Activate });
        var member = leader != 0 && leader != player;
        Keep(new Label(member ? "Leave the party" : "Disband the party", false, 0x0386, 2) { X = 110, Y = 360 });
        if (isLeader) {
            Keep(new Button(AddId, 0x0FA8, 0x0FAA, 0x0FA9) { X = 70, Y = 385, ButtonAction = ButtonAction.Activate });
            Keep(new Label("Add New Member", false, 0x0386, 2) { X = 110, Y = 385 });
        }
        Keep(new Button(OkId, 0x00F9, 0x00F8, 0x00F7) { X = 130, Y = 430, ButtonAction = ButtonAction.Activate });
        Keep(new Button(CancelId, 0x00F3, 0x00F1, 0x00F2) { X = 236, Y = 430, ButtonAction = ButtonAction.Activate });
        for (var n = 0; n < _notes.Count; n++) {
            Keep(new Label(_notes[n], true, 0x0481, 1) { X = 16, Y = noteY });
            noteY += 16;
        }
        WantUpdateSize = true;
    }

    void Keep(Control c) {
        if (c != null && !c.IsDisposed) Add(c);
    }

    void ClearChildren() {
        while (Children.Count > 0) {
            var c = Children[Children.Count - 1];
            c.Parent = null;
            c.Dispose();
        }
    }

    /// <summary>A party message that arrived with no manifest. One gump, a label per line.</summary>
    sealed class PartyNote : Gump {
        readonly List<string> _lines = [];

        public static void Show(World world, uint serial, string text) {
            var open = UIManager.GetGump<PartyNote>();
            if (open == null || open.IsDisposed) UIManager.Add(open = new PartyNote(world));
            open.AddLine(Format(serial, text));
        }

        PartyNote(World world) : base(world, 0, 0) {
            X = 80;
            Y = 80;
            CanMove = true;
            CanCloseWithRightClick = true;
            AcceptMouseInput = true;
        }

        void AddLine(string line) {
            _lines.Add(line ?? "");
            if (_lines.Count > 8) _lines.RemoveAt(0);
            while (Children.Count > 0) {
                var c = Children[Children.Count - 1];
                c.Parent = null;
                c.Dispose();
            }
            var height = 16 + _lines.Count * 16;
            var back = new ResizePic(0x0A28) { X = 0, Y = 0, Width = 360, Height = height };
            if (!back.IsDisposed) Add(back);
            var y = 8;
            for (var i = 0; i < _lines.Count; i++) {
                Add(new Label(_lines[i], true, 0x0481, 1) { X = 8, Y = y });
                y += 16;
            }
            WantUpdateSize = true;
        }
    }
}
