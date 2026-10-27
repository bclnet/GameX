// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>ClassicUO ButtonAction.</summary>
public enum ButtonAction { Default = 0, SwitchPage = 0, Activate = 1 }

/// <summary>
/// ClassicUO Button. The normal graphic's index size is the control size.
/// No caption texture. A left click calls OnButtonClick(ButtonID).
/// </summary>
public class Button : Control {
    ushort _normal, _pressed, _over;

    public Button(int buttonID, ushort normal, ushort pressed, ushort over = 0) {
        ButtonID = buttonID;
        _normal = normal;
        _pressed = pressed;
        _over = over;
        if (!GumpMetrics.TrySize(normal, out var w, out var h)) {
            Dispose();
            return;
        }
        Width = w;
        Height = h;
        CanMove = false;
        AcceptMouseInput = true;
        CanCloseWithEsc = false;
    }

    public int ButtonID { get; }
    public ButtonAction ButtonAction { get; set; }
    public int ToPage { get; set; }

    /// <summary>Action 0 on a server gump. Default stays a click, because that enum value is also 0.</summary>
    public bool IsPageButton { get; set; }
    public override ClickPriority Priority => ClickPriority.High;

    public override void OnButtonClick(int buttonID) {
        if (!IsPageButton) {
            base.OnButtonClick(buttonID);
            return;
        }
        for (var p = Parent; p != null; p = p.Parent)
            if (p is Gump g) {
                g.ActivePage = ToPage;
                return;
            }
    }

    public ushort ButtonGraphicNormal {
        get => _normal;
        set {
            _normal = value;
            if (GumpMetrics.TrySize(value, out var w, out var h)) {
                Width = w;
                Height = h;
            }
        }
    }

    public ushort ButtonGraphicPressed => _pressed;
    public ushort ButtonGraphicOver => _over;

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        GumpArtDraw.Pic(_normal, x, y, Width, Height, false);
        return base.Draw(x, y);
    }
}
