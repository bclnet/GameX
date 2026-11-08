// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ExpandableScroll. Four gumps stack. The expander button's drag changes SpecialHeight.
/// Button has no mouse event, so Update watches the pressed expander.
/// </summary>
public class ExpandableScroll : Control {
    const int c_ExpandableScrollHeight_Min = 274;
    const int c_ExpandableScrollHeight_Max = 800;
    const int c_GumplingExpanderY_Offset = 2;
    const int c_GumplingExpander_ButtonID = 0x7FBEEF;

    readonly GumpPic _gumpBottom;
    readonly GumpPicTiled _gumpMiddle;
    readonly GumpPicTiled _gumpRight;
    readonly GumpPic _gumpTop;
    readonly bool _isResizable;
    Button _gumpExpander;
    GumpPic _gumplingTitle;
    int _gumplingTitleGumpID;
    bool _gumplingTitleGumpIDDelta;
    bool _isExpanding;
    Mouse.Point _lastExpanderPosition;

    public ExpandableScroll(int x, int y, int height, ushort graphic, bool isResizable = true) {
        X = x;
        Y = y;
        SpecialHeight = height;
        _isResizable = isResizable;
        CanMove = true;
        AcceptMouseInput = true;

        var width = 0;
        var w0 = 0;
        var w1 = 0;
        var w3 = 0;
        for (var i = 0; i < 4; i++) {
            if (!GumpMetrics.TrySize((ushort)(graphic + i), out var gw, out _)) {
                Dispose();
                return;
            }
            if (gw > width) width = gw;
            if (i == 0) w0 = gw;
            else if (i == 1) w1 = gw;
            else if (i == 3) w3 = gw;
        }

        Add(_gumpTop = new GumpPic(0, 0, graphic, 0));
        Add(_gumpRight = new GumpPicTiled(0, 0, 0, 0, (ushort)(graphic + 1)));
        Add(_gumpMiddle = new GumpPicTiled(0, 0, 0, 0, (ushort)(graphic + 2)));
        Add(_gumpBottom = new GumpPic(0, 0, (ushort)(graphic + 3), 0));

        if (_isResizable) {
            Add(_gumpExpander = new Button(c_GumplingExpander_ButtonID, 0x082E, 0x82F) {
                ButtonAction = ButtonAction.Activate,
                X = 0,
                Y = 0
            });
        }

        var off = w0 - w3;
        _gumpRight.X = _gumpMiddle.X = (width - w1) / 2;
        _gumpRight.Y = _gumpMiddle.Y = _gumplingMidY;
        _gumpRight.Height = _gumpMiddle.Height = _gumplingMidHeight;
        _gumpRight.WantUpdateSize = _gumpMiddle.WantUpdateSize = true;
        _gumpBottom.X = (off / 2) + (off / 4);
        Width = _gumpMiddle.Width;
        WantUpdateSize = true;
    }

    int _gumplingMidY => _gumpTop.Height;

    int _gumplingMidHeight => SpecialHeight - _gumpTop.Height - _gumpBottom.Height - (_gumpExpander?.Height ?? 0);

    int _gumplingBottomY => SpecialHeight - _gumpBottom.Height - (_gumpExpander?.Height ?? 0);

    int _gumplingExpanderX => (Width - (_gumpExpander?.Width ?? 0)) >> 1;

    int _gumplingExpanderY => SpecialHeight - (_gumpExpander?.Height ?? 0) - c_GumplingExpanderY_Offset;

    public int TitleGumpID {
        set {
            _gumplingTitleGumpID = value;
            _gumplingTitleGumpIDDelta = true;
        }
    }

    public int SpecialHeight { get; set; }

    public ushort Hue {
        get => _gumpTop.Hue;
        set => _gumpTop.Hue = _gumpBottom.Hue = _gumpMiddle.Hue = _gumpRight.Hue = value;
    }

    public override void Dispose() {
        _gumpExpander?.Dispose();
        _gumpExpander = null;
        base.Dispose();
    }

    public bool Contains(int x, int y) =>
        Hits(_gumpTop, x, y) || Hits(_gumpMiddle, x, y) || Hits(_gumpRight, x, y) ||
        Hits(_gumpBottom, x, y) || Hits(_gumpExpander, x, y);

    public override void Update() {
        if (IsDisposed || _gumpTop == null) return;
        var held = _gumpExpander != null && Mouse.LButtonPressed
            && UIManager.LastControlMouseDown((int)MouseButtonType.Left) == _gumpExpander;
        if (held) {
            if (!_isExpanding) {
                _isExpanding = true;
                _lastExpanderPosition = Mouse.LClickPosition;
            }
        }
        else _isExpanding = false;

        if (Mouse.LButtonPressed && _isExpanding) {
            SpecialHeight += Mouse.Position.Y - _lastExpanderPosition.Y;
            _lastExpanderPosition = Mouse.Position;
        }

        if (SpecialHeight < c_ExpandableScrollHeight_Min) {
            _lastExpanderPosition.Y += c_ExpandableScrollHeight_Min - SpecialHeight;
            SpecialHeight = c_ExpandableScrollHeight_Min;
        }
        if (SpecialHeight > c_ExpandableScrollHeight_Max) {
            _lastExpanderPosition.Y -= SpecialHeight - c_ExpandableScrollHeight_Max;
            SpecialHeight = c_ExpandableScrollHeight_Max;
        }

        if (_gumplingTitleGumpIDDelta) {
            _gumplingTitleGumpIDDelta = false;
            _gumplingTitle?.Dispose();
            Add(_gumplingTitle = new GumpPic(0, 0, (ushort)_gumplingTitleGumpID, 0));
        }

        _gumpTop.X = 0;
        _gumpTop.Y = 0;
        _gumpTop.WantUpdateSize = true;
        _gumpRight.Y = _gumpMiddle.Y = _gumplingMidY;
        _gumpRight.Height = _gumpMiddle.Height = _gumplingMidHeight;
        _gumpRight.WantUpdateSize = _gumpMiddle.WantUpdateSize = true;
        _gumpBottom.Y = _gumplingBottomY;
        _gumpBottom.WantUpdateSize = true;
        if (_isResizable) {
            _gumpExpander.X = _gumplingExpanderX;
            _gumpExpander.Y = _gumplingExpanderY;
            _gumpExpander.WantUpdateSize = true;
        }
        if (_gumplingTitle != null) {
            _gumplingTitle.X = (_gumpTop.Width - _gumplingTitle.Width) >> 1;
            _gumplingTitle.Y = (_gumpTop.Height - _gumplingTitle.Height) >> 1;
            _gumplingTitle.WantUpdateSize = true;
        }
        WantUpdateSize = true;
        base.Update();
    }

    static bool Hits(Control c, int x, int y) {
        if (c == null || c.IsDisposed || !c.IsVisible) return false;
        return x >= c.X && y >= c.Y && x < c.X + c.Width && y < c.Y + c.Height;
    }
}
