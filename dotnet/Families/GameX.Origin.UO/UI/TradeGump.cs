// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>0x6F. Close and right-click send cancel. A server close disposes this gump and does not send again.</summary>
public sealed class TradeGump : Gump {
    bool _sent;

    public TradeGump(World world, uint serial, string name) : base(world, serial, 0) {
        X = 200;
        Y = 160;
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        var title = string.IsNullOrEmpty(name) ? "trade" : "trade " + name;
        if (title.Length > 28) title = title.Substring(0, 28);
        Add(new ResizePic(0x0BB8) { X = 0, Y = 0, Width = 220, Height = 64 });
        Add(new Label(title, false, 0x0386) { X = 12, Y = 10 });
        Add(new CloseRow(this) { X = 12, Y = 32 });
    }

    public override void CloseWithRightClick() {
        if (!CanCloseWithRightClick || IsDisposed) return;
        Reply();
    }

    void Reply() {
        if (_sent || IsDisposed) return;
        _sent = true;
        UOLoginNet.ReplyTrade(LocalSerial);
        Dispose();
    }

    sealed class CloseRow : Label {
        readonly TradeGump _owner;

        public CloseRow(TradeGump owner) : base("close", false, 0x034F) {
            _owner = owner;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) => _owner.Reply();
    }
}
