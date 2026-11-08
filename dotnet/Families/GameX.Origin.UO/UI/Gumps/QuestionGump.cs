// SPDX-License-Identifier: BSD-2-Clause

using System;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO QuestionGump. Cancel calls the result false. OK calls it true. Both then dispose.
/// Right-click closes and does not call the result.
/// </summary>
public sealed class QuestionGump : Gump {
    readonly Action<bool> _result;

    public QuestionGump(World world, string message, Action<bool> result) : base(world, 0, 0) {
        _result = result;
        CanCloseWithRightClick = true;
        CanMove = false;
        AcceptMouseInput = true;
        WantUpdateSize = false;
        Add(new GumpPic(0, 0, 0x0816, 0));
        Add(new Label(message, false, 0x0386, 1) { X = 33, Y = 30 });
        Add(new Button((int)Buttons.Cancel, 0x0817, 0x0818, 0x0819) {
            X = 37, Y = 75, ButtonAction = ButtonAction.Activate,
        });
        Add(new Button((int)Buttons.Ok, 0x081A, 0x081B, 0x081C) {
            X = 100, Y = 75, ButtonAction = ButtonAction.Activate,
        });
        Place();
    }

    // 0x0816 size centers on the backbuffer. A miss sits at the middle of the 640 by 480 fallback.
    void Place() {
        var s = Mouse.Current;
        var bw = s.BufW > 0 ? s.BufW : 640;
        var bh = s.BufH > 0 ? s.BufH : 480;
        if (GumpMetrics.TrySize(0x0816, out var gw, out var gh) && gw > 0 && gh > 0) {
            Width = gw;
            Height = gh;
            X = (bw - gw) >> 1;
            Y = (bh - gh) >> 1;
            return;
        }
        X = 640 >> 1;
        Y = 480 >> 1;
    }

    public override void OnButtonClick(int buttonID) {
        switch (buttonID) {
            case (int)Buttons.Cancel:
                _result(false);
                Dispose();
                break;
            case (int)Buttons.Ok:
                _result(true);
                Dispose();
                break;
        }
    }

    enum Buttons { Cancel, Ok }
}
