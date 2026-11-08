// SPDX-License-Identifier: BSD-2-Clause

using System;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO NiceButton. It extends Button so a left click reaches OnButtonClick.
/// The gump is only an anchor: Button disposes a graphic the archive does not have.
/// The face is the label. Selected or hovered draws the white hit fill.
/// BGRA1555 has no partial alpha, so Alpha 0.25 is an opaque light pixel.
/// </summary>
public class NiceButton : Button {
    // r = g = b = 8, opaque. White at 0.25.
    static readonly byte[] Fill = { 0x08, 0xA1 };
    readonly ButtonAction _action;
    readonly int _groupnumber;
    bool _isSelected;

    public NiceButton(
        int x,
        int y,
        int w,
        int h,
        ButtonAction action,
        string text,
        int groupnumber = 0,
        TEXT_ALIGN_TYPE align = TEXT_ALIGN_TYPE.TS_CENTER,
        ushort hue = 0xFFFF,
        bool unicode = true,
        byte font = 0xFF
    ) : base(0, Anchor(), Anchor(), 0) {
        _action = action;
        _groupnumber = groupnumber;
        _isSelected = false;
        IsSelectable = true;
        ButtonParameter = 0;
        ButtonAction = action;
        X = x;
        Y = y;
        Width = w;
        Height = h;
        WantUpdateSize = false;
        AcceptMouseInput = true;
        CanMove = false;
        Alpha = 0.25f;
        TextLabel = new Label(text, unicode, hue, font) { AcceptMouseInput = false };
        TextLabel.Y = (h - TextLabel.Height) >> 1;
        if (align == TEXT_ALIGN_TYPE.TS_CENTER) TextLabel.X = Math.Max(0, (w - TextLabel.Width) >> 1);
        else if (align == TEXT_ALIGN_TYPE.TS_RIGHT) TextLabel.X = Math.Max(0, w - TextLabel.Width);
        if (IsDisposed) TextLabel.Dispose();
        else Add(TextLabel);
    }

    public Label TextLabel { get; }
    public int ButtonParameter { get; set; }
    public bool IsSelectable { get; set; } = true;

    public bool IsSelected {
        get => _isSelected && IsSelectable;
        set {
            if (!IsSelectable) return;
            _isSelected = value;
            if (!value || Parent == null) return;
            foreach (var c in Parent.Children) {
                if (c is NiceButton b && b != this && !b.IsDisposed && b._groupnumber == _groupnumber)
                    b.IsSelected = false;
            }
        }
    }

    public static NiceButton GetSelected(Control p, int group) {
        foreach (var c in p.Children)
            if (c is NiceButton b && !b.IsDisposed && b._groupnumber == group && b.IsSelected)
                return b;
        return null;
    }

    public override void OnButtonClick(int buttonID) => OnMouseUp(0, 0, MouseButtonType.Left);

    protected void OnMouseUp(int x, int y, MouseButtonType button) {
        if (button != MouseButtonType.Left) return;
        IsSelected = true;
        if (_action == ButtonAction.SwitchPage) {
            IsPageButton = true;
            ToPage = ButtonParameter;
            base.OnButtonClick(ButtonParameter);
            return;
        }
        IsPageButton = false;
        base.OnButtonClick(ButtonParameter);
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if ((IsSelected || UIManager.MouseOverControl == this) && Width > 0 && Height > 0 && Alpha > 0)
            GumpArt.Sink?.DrawRaw(0x57200020, x, y, Width, Height, Fill, 1, 1);
        foreach (var c in Children)
            if ((c.Page == 0 || c.Page == ActivePage) && c.IsVisible)
                c.Draw(c.X + x, c.Y + y);
        return true;
    }

    // 0 stays when the archive is absent or gump 0 has a size. 0x00D4 keeps the control when 0 does not.
    static ushort Anchor() {
        if (GumpMetrics.TrySize(0, out var w, out var h) && (w > 0 || UltimaOnline.Current?.Assets == null))
            return 0;
        if (GumpMetrics.TrySize(0x00D4, out w, out h) && w > 0 && h > 0)
            return 0x00D4;
        return 0;
    }
}
