// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>ClassicUO ButtonAction.</summary>
public enum ButtonAction { Default = 0, SwitchPage = 0, Activate = 1 }

/// <summary>
/// ClassicUO Button. The normal graphic's index size is the control size.
/// No caption texture. Mouse hits are not delivered by the host.
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
    public override ClickPriority Priority => ClickPriority.High;

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
}
