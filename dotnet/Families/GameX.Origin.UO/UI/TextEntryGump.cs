// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>0xAB. OK and Enter send 0xAC with the flag set. Right-click sends it clear when cancel is allowed.</summary>
public sealed class TextEntryGump : Gump {
    readonly byte _parent, _button;
    readonly TextBox _box;
    bool _sent;

    public TextEntryGump(World world, UOTextEntry entry) : base(world, entry.Serial, 0) {
        _parent = entry.Parent;
        _button = entry.Button;
        X = 180;
        Y = 160;
        CanMove = true;
        CanCloseWithRightClick = entry.Cancel;
        AcceptMouseInput = true;
        Add(new ResizePic(0x0BB8) { X = 0, Y = 0, Width = 320, Height = 88 });
        var desc = string.IsNullOrEmpty(entry.Description) ? "text" : entry.Description;
        Add(new Label(desc, false, 0x0386) { X = 12, Y = 10 });
        _box = new TextBox { X = 12, Y = 32, Width = 280, Height = 20, Hue = 0x0481, MaxLength = entry.MaxLength };
        _box.Text = entry.Text;
        Add(_box);
        Add(new OkRow(this) { X = 12, Y = 58 });
        _box.SetKeyboardFocus();
    }

    public override void OnKeyboardReturn(int textID, string text) => Reply(true);

    public override void CloseWithRightClick() {
        if (!CanCloseWithRightClick || IsDisposed) return;
        Reply(false);
    }

    void Reply(bool ok) {
        if (_sent || IsDisposed) return;
        _sent = true;
        UOLoginNet.ReplyText(LocalSerial, _parent, _button, _box.Text, ok);
        Dispose();
    }

    sealed class OkRow : Label {
        readonly TextEntryGump _owner;

        public OkRow(TextEntryGump owner) : base("ok", false, 0x034F) {
            _owner = owner;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) => _owner.Reply(true);
    }
}
