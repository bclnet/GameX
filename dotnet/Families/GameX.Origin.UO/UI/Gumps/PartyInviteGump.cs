// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO PartyInviteGump. Accept sends Send_PartyAccept. Decline sends Send_PartyDecline.
/// This client has no NiceButton, so the words are labels. The name is NameOf.
/// </summary>
public sealed class PartyInviteGump : Gump {
    readonly uint _serial;
    bool _sent;

    public PartyInviteGump(World world, uint inviter) : base(world, inviter, inviter) {
        _serial = inviter;
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        var name = UOLoginNet.NameOf(inviter);
        if (string.IsNullOrEmpty(name)) name = "No Name";
        var caption = new Label(name + " has invited you to join a party.", true, 15, 1) { X = 12, Y = 14 };
        var extra = name.Length < 10 ? 0 : name.Length * 5;
        var width = 270 + extra;
        if (caption.Width + 24 > width) width = caption.Width + 24;
        WorldGump.ViewCenter(out var cx, out _);
        X = cx - 125;
        Y = 150;
        Add(new ResizePic(0x0A28) { X = 0, Y = 0, Width = width, Height = 80 });
        Add(caption);
        Add(new Choice(this, false) { X = width - 150, Y = 48 });
        Add(new Choice(this, true) { X = width - 70, Y = 48 });
    }

    void Reply(bool accept) {
        if (_sent || IsDisposed) return;
        _sent = true;
        if (accept) UOLoginNet.Send(OutgoingPackets.Send_PartyAccept(_serial));
        else UOLoginNet.Send(OutgoingPackets.Send_PartyDecline(_serial));
        Dispose();
    }

    sealed class Choice : Label {
        readonly PartyInviteGump _owner;
        readonly bool _accept;

        public Choice(PartyInviteGump owner, bool accept) : base(accept ? "Accept" : "Decline", true, 0x0481, 1) {
            _owner = owner;
            _accept = accept;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) => _owner.Reply(_accept);
    }
}
