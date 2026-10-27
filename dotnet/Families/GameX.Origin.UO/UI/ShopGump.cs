// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>0x74 or 0x9E. Close sends an empty 0x3B or 0x9F. An incoming 0x3B disposes this gump and does not send a buy.</summary>
public sealed class ShopGump : Gump {
    readonly bool _buy;
    bool _sent;

    public ShopGump(World world, uint serial, bool buy) : base(world, serial, 0) {
        _buy = buy;
        X = 200;
        Y = 80;
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        Add(new ResizePic(0x0BB8) { X = 0, Y = 0, Width = 200, Height = 64 });
        Add(new Label(buy ? "buy" : "sell", false, 0x0386) { X = 12, Y = 10 });
        Add(new CloseRow(this) { X = 12, Y = 32 });
    }

    public override void CloseWithRightClick() {
        if (!CanCloseWithRightClick || IsDisposed) return;
        Reply();
    }

    void Reply() {
        if (_sent || IsDisposed) return;
        _sent = true;
        UOLoginNet.ReplyShop(LocalSerial, _buy);
        Dispose();
    }

    sealed class CloseRow : Label {
        readonly ShopGump _owner;

        public CloseRow(ShopGump owner) : base("close", false, 0x034F) {
            _owner = owner;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) => _owner.Reply();
    }
}
