// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ChatGumpChooseName. The wording, the name box, and the two buttons match
/// ChatGump's name prompt. OK sends 0xB5 through Send_OpenChat. Close and right-click do not.
/// The frame is ChatGump's 0x0A28 at the name-prompt size. No second chat window is opened.
/// </summary>
public sealed class ChatGumpChooseName : Gump {
    const string Choose =
        "You are about to choose your name for the Ultima Online chat system. This name will be PERMANENT and unique on this shard. It will apply to all characters using this account. Do not use your Ultima Online account name for this name as the name you choose will be public. Enter your chat name here:";

    readonly TextBox _box;
    bool _sent;

    public ChatGumpChooseName(World world) : base(world, 0, 0) {
        CanMove = false;
        AcceptKeyboardInput = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        WantUpdateSize = false;
        X = 250;
        Y = 100;
        Width = 210;
        Height = 330;
        Add(new ResizePic(0x0A28) { Width = Width, Height = Height });
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
        var boxW = Width - boxX - 17;
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

    /// <summary>One chooser. A second call returns the open gump and does not add another.</summary>
    public static ChatGumpChooseName Open(World world) {
        var open = UIManager.GetGump<ChatGumpChooseName>();
        if (open != null) return open;
        open = new ChatGumpChooseName(world);
        UIManager.Add(open);
        return open;
    }

    public override void OnButtonClick(int buttonID) => Close(buttonID == 1);

    public override void OnKeyboardReturn(int textID, string text) => Close(true);

    public override void CloseWithRightClick() {
        if (!CanCloseWithRightClick || IsDisposed) return;
        Close(false);
    }

    void Close(bool ok) {
        if (_sent || IsDisposed) return;
        _sent = true;
        if (ok && _box != null && !_box.IsDisposed && !string.IsNullOrWhiteSpace(_box.Text))
            UOLoginNet.Send(OutgoingPackets.Send_OpenChat(_box.Text));
        Dispose();
    }
}
