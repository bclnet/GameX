// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO split stack. OK and Enter lift the typed count.
/// Cancel and right-click dispose the gump and do not lift.
/// </summary>
public sealed class SplitMenuGump : Gump {
    readonly TextBox _box;
    bool _sent;

    public SplitMenuGump(uint serial) : base(null, serial, serial) {
        X = UIManager.MouseX - 80;
        Y = UIManager.MouseY - 40;
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = false;
        Add(new GumpPic(0, 0, 0x085C, 0));
        _box = new TextBox { X = 29, Y = 42, Width = 60, Height = 20, Hue = 0x0386, MaxLength = 6 };
        var count = UOLoginNet.AmountOf(serial);
        if (count < 1) count = 1;
        _box.Text = count.ToString();
        Add(_box);
        Add(new Button(0, 0x085D, 0x085E, 0x085F) { X = 102, Y = 37 });
        Add(new CancelRow(this) { X = 29, Y = 64 });
        _box.SetKeyboardFocus();
    }

    public override void OnButtonClick(int buttonID) => Ok();

    public override void OnKeyboardReturn(int textID, string text) => Ok();

    public override void CloseWithRightClick() {
        if (!CanCloseWithRightClick || IsDisposed) return;
        Dispose();
    }

    void Ok() {
        if (_sent || IsDisposed) return;
        var max = UOLoginNet.AmountOf(LocalSerial);
        if (!int.TryParse(_box.Text, out var amount) || amount < 1) amount = 1;
        if (amount > max) amount = max;
        if (amount < 1) {
            Dispose();
            return;
        }
        _sent = true;
        UOLoginNet.Lift(LocalSerial, amount);
        Dispose();
    }

    sealed class CancelRow : Label {
        readonly SplitMenuGump _owner;

        public CancelRow(SplitMenuGump owner) : base("cancel", false, 0x0386) {
            _owner = owner;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) {
            if (_owner._sent || _owner.IsDisposed) return;
            _owner.Dispose();
        }
    }
}
