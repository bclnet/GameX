// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO TextEntryDialogGump. Background 0x0474, field 0x0477, OK 0x047B, cancel 0x0478.
/// OK sends 0xAC with the flag set. Cancel sends it clear. Both then dispose.
/// The reply is UOLoginNet.ReplyText. No new packet.
/// Variant 2 keeps digits only. TextBox has no NumbersOnly, so Update strips the rest.
/// IsModal is stored. UIManager does not block other gumps.
/// </summary>
public class TextEntryDialogGump : Gump {
    readonly TextBox _box;
    readonly bool _numbers;

    public TextEntryDialogGump(World world, uint serial, int x, int y, byte variant, int maxlen, string text, string description, byte buttonid, byte parentid)
        : base(world, serial, 0) {
        _numbers = variant == 2;
        CanMove = false;
        IsFromServer = true;
        IsModal = true;
        X = x;
        Y = y;
        var background = new GumpPic(0, 0, 0x0474, 0);
        Add(background);
        Add(new Label(text, false, 0x0386, 2) { X = 60, Y = 50 });
        Add(new Label(description, false, 0x0386, 2) { X = 60, Y = 108 });
        Add(new GumpPic(60, 130, 0x0477, 0));
        _box = new TextBox {
            X = 71,
            Y = 137,
            Width = 250,
            Height = 20,
            Hue = 0x0386,
            MaxLength = maxlen < 0 ? 0 : maxlen,
        };
        Add(_box);
        Add(new Button((int)ButtonType.Ok, 0x047B, 0x047C, 0x047D) {
            X = 117,
            Y = 190,
            ButtonAction = ButtonAction.Activate,
        });
        Add(new Button((int)ButtonType.Cancel, 0x0478, 0x0478, 0x047A) {
            X = 204,
            Y = 190,
            ButtonAction = ButtonAction.Activate,
        });
        ButtonID = buttonid;
        ParentID = parentid;
        UIManager.KeyboardFocusControl = _box;
        _box.SetKeyboardFocus();
    }

    public byte ParentID { get; }
    public byte ButtonID { get; }

    /// <summary>ClassicUO IsModal. The host does not keep other gumps from taking input.</summary>
    public bool IsModal { get; set; }

    public override void Update() {
        if (_numbers && _box != null && !_box.IsDisposed) Digits();
        base.Update();
    }

    public override void OnButtonClick(int buttonID) {
        switch ((ButtonType)buttonID) {
            case ButtonType.Ok:
                Reply(true);
                break;
            case ButtonType.Cancel:
                Reply(false);
                break;
        }
    }

    void Reply(bool ok) {
        if (IsDisposed) return;
        UOLoginNet.ReplyText(LocalSerial, ParentID, ButtonID, _box != null ? _box.Text : "", ok);
        Dispose();
    }

    void Digits() {
        var t = _box.Text ?? "";
        var keep = true;
        for (var i = 0; i < t.Length; i++)
            if (t[i] < '0' || t[i] > '9') { keep = false; break; }
        if (keep) return;
        var sb = new char[t.Length];
        var w = 0;
        for (var i = 0; i < t.Length; i++)
            if (t[i] >= '0' && t[i] <= '9') sb[w++] = t[i];
        _box.Text = new string(sb, 0, w);
    }

    enum ButtonType { Ok, Cancel }
}
