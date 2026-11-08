// SPDX-License-Identifier: BSD-2-Clause

using System;

namespace GameX.Origin.UO;

/// <summary>ClassicUO MessageButtonType. OK is the check. OK_CANCEL adds the cross.</summary>
public enum MessageButtonType {
    OK,
    OK_CANCEL,
}

/// <summary>
/// ClassicUO MessageBoxGump. OK calls the action true. Cancel calls it false. Both then dispose.
/// Enter calls OK. UIManager delivers Enter only from a TextBox, so an empty box holds the focus.
/// Right-click does not close. A null action is ignored. No profile file.
/// </summary>
public sealed class MessageBoxGump : Gump {
    readonly Action<bool> _action;
    readonly TextBox _enter;
    bool _done;

    public MessageBoxGump(World world, int w, int h, string message, Action<bool> action, bool hasBackground = false, MessageButtonType menuType = MessageButtonType.OK) : base(world, 0, 0) {
        _action = action;
        CanMove = true;
        CanCloseWithRightClick = false;
        CanCloseWithEsc = false;
        AcceptMouseInput = true;
        AcceptKeyboardInput = true;
        LayerOrder = UILayer.Over;
        WantUpdateSize = false;
        Width = w;
        Height = h;
        Add(new ResizePic(0x0A28) { Width = w, Height = h });
        if (hasBackground)
            Add(new ResizePic(3000) { X = 30, Y = 40, Width = w - 60, Height = h - 100 });
        DialogText.Add(this, message, 40, 45, w - 90, false, 0x0386, 1);
        var ok = new Button(0, 0x0481, 0x0483, 0x0482) {
            Y = h - 45,
            ButtonAction = ButtonAction.Activate,
        };
        if (!ok.IsDisposed) {
            ok.X = (w - ok.Width) >> 1;
            Add(ok);
            if (menuType == MessageButtonType.OK_CANCEL) {
                var cancel = new Button(1, 0x047E, 0x047F, 0x0480) {
                    Y = h - 45,
                    ButtonAction = ButtonAction.Activate,
                };
                if (!cancel.IsDisposed) {
                    ok.X = w / 2 - cancel.Width;
                    cancel.X = w / 2 - cancel.Width + ok.Width + 5;
                    Add(cancel);
                }
            }
        }
        _enter = new TextBox { X = 0, Y = 0, Width = 1, Height = 1, MaxLength = 0, AcceptMouseInput = false };
        Add(_enter);
        Place();
        _enter.SetKeyboardFocus();
    }

    public override void OnButtonClick(int buttonID) {
        if (buttonID == 0) Finish(true);
        else if (buttonID == 1) Finish(false);
    }

    public override void OnKeyboardReturn(int textID, string text) => Finish(true);

    void Finish(bool ok) {
        if (_done || IsDisposed) return;
        _done = true;
        _action?.Invoke(ok);
        Dispose();
    }

    void Place() {
        var s = Mouse.Current;
        var bw = s.BufW > 0 ? s.BufW : 640;
        var bh = s.BufH > 0 ? s.BufH : 480;
        X = (bw - Width) >> 1;
        Y = (bh - Height) >> 1;
    }
}

/// <summary>
/// ClassicUO EntryDialog from the same file. OK and Enter pass the box text, then dispose.
/// Right-click does not close. A null action is ignored.
/// </summary>
public sealed class EntryDialog : Gump {
    readonly Action<string> _action;
    readonly TextBox _box;
    bool _done;

    public EntryDialog(World world, int w, int h, string message, Action<string> action, string initialValue = "") : base(world, 0, 0) {
        _action = action;
        CanMove = false;
        CanCloseWithRightClick = false;
        CanCloseWithEsc = false;
        AcceptMouseInput = true;
        LayerOrder = UILayer.Over;
        WantUpdateSize = false;
        Width = w;
        Height = h;
        Add(new ResizePic(0x0A28) { Width = w, Height = h });
        var msgH = DialogText.Add(this, message, 40, 45, w - 90, false, 0x0386, 1);
        if (msgH < 16) msgH = 16;
        var boxW = w - 94;
        if (boxW < 40) boxW = 40;
        Add(new ResizePic(0x0BB8) { X = 40, Y = 45 + msgH + 5, Width = w - 90, Height = 25 });
        _box = new TextBox {
            X = 42,
            Y = 45 + msgH + 7,
            Width = boxW,
            Height = 25,
            Hue = 0x0481,
            MaxLength = -1,
        };
        _box.Text = initialValue ?? "";
        Add(_box);
        var ok = new Button(0, 0x0481, 0x0482, 0x0483) {
            Y = h - 45,
            ButtonAction = ButtonAction.Activate,
        };
        if (!ok.IsDisposed) {
            ok.X = (w - ok.Width) >> 1;
            Add(ok);
        }
        Place();
        _box.SetKeyboardFocus();
    }

    public override void OnButtonClick(int buttonID) {
        if (buttonID == 0) Finish();
    }

    public override void OnKeyboardReturn(int textID, string text) => Finish();

    void Finish() {
        if (_done || IsDisposed) return;
        _done = true;
        _action?.Invoke(_box == null || _box.IsDisposed ? "" : _box.Text);
        Dispose();
    }

    void Place() {
        var s = Mouse.Current;
        var bw = s.BufW > 0 ? s.BufW : 640;
        var bh = s.BufH > 0 ? s.BufH : 480;
        X = (bw - Width) >> 1;
        Y = (bh - Height) >> 1;
    }
}

/// <summary>Word wrap for the two dialogs. Width is MeasureText, or 6 pixels a glyph when no sink is attached.</summary>
static class DialogText {
    public static int Add(Control parent, string text, int x, int y, int maxW, bool unicode, ushort hue, byte font) {
        if (maxW < 8) maxW = 8;
        var src = (text ?? "").Replace("\r\n", "");
        var height = 0;
        var start = 0;
        while (start <= src.Length) {
            var nl = src.IndexOf('\n', start);
            var end = nl < 0 ? src.Length : nl;
            var line = src.Substring(start, end - start);
            if (line.Length == 0) height += 16;
            else {
                var i = 0;
                while (i < line.Length) {
                    var n = line.Length - i;
                    while (n > 1 && WidthOf(line.Substring(i, n), font) > maxW) {
                        var sp = line.LastIndexOf(' ', i + n - 1, n);
                        if (sp > i) n = sp - i;
                        else n--;
                    }
                    if (n <= 0) n = 1;
                    var piece = line.Substring(i, n).Trim();
                    if (piece.Length > 0)
                        parent.Add(new Label(piece, unicode, hue, font) { X = x, Y = y + height });
                    height += 16;
                    i += n;
                    while (i < line.Length && line[i] == ' ') i++;
                }
            }
            if (nl < 0) break;
            start = nl + 1;
        }
        return height;
    }

    public static int WidthOf(string text, byte font) =>
        GumpArt.Sink?.MeasureText(text, font) ?? (string.IsNullOrEmpty(text) ? 0 : text.Length * 6);
}
