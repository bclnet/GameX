// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ClickableColorBox. ColorBox is the inset. A left click disposes the open ColorPickerGump
/// and opens one whose OK sets Hue. The frame does not take the click: mouse-up does not bubble.
/// </summary>
public class ClickableColorBox : Control {
    static readonly byte[] White = { 0xFF, 0xFF };
    readonly World _world;
    ushort _hue;
    byte[] _px = White;

    public ClickableColorBox(World world, int x, int y, int w, int h, ushort hue) {
        _world = world;
        X = x;
        Y = y;
        WantUpdateSize = false;
        AcceptMouseInput = true;
        CanMove = false;
        var background = new GumpPic(0, 0, 0x00D4, 0) { AcceptMouseInput = false, CanMove = false };
        if (!background.IsDisposed) Add(background);
        Width = background.Width > 0 ? background.Width : w;
        Height = background.Height > 0 ? background.Height : h;
        Hue = hue;
    }

    public ushort Hue {
        get => _hue;
        set {
            _hue = value;
            _px = Swatch(value);
        }
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (Children.Count != 0) Children[0].Draw(x, y);
        var dw = Width - 6;
        var dh = Height - 6;
        if (dw > 0 && dh > 0 && _px != null)
            GumpArt.Sink?.DrawRaw(0x5E210000 + Hue, x + 3, y + 3, dw, dh, _px, 1, 1);
        return true;
    }

    public override void OnMouseClick(int x, int y) => OnMouseUp(x, y, MouseButtonType.Left);

    protected void OnMouseUp(int x, int y, MouseButtonType button) {
        if (button != MouseButtonType.Left) return;
        UIManager.GetGump<ColorPickerGump>()?.Dispose();
        UIManager.Add(new Picker(this, _world));
    }

    // Hue 0 stays white. A nonzero hue is hues.mul at hue - 1, the bright end of the ramp.
    static byte[] Swatch(ushort hue) {
        var index = hue & 0x7FFF;
        if (index != 0) index--;
        if (index == 0) return White;
        var uo = UltimaOnline.Current;
        if (uo == null || !uo.TryHueRamp(index, out var ramp) || ramp == null || ramp.Length <= 31) return White;
        var c = ramp[31];
        if (c == 0) return White;
        c |= 0x8000;
        return new[] { (byte)c, (byte)(c >> 8) };
    }

    sealed class Picker : ColorPickerGump {
        readonly ClickableColorBox _box;

        public Picker(ClickableColorBox box, World world) : base(world, 0, 0, 100, 100) => _box = box;

        protected override void OnOk(ushort hue) => _box.Hue = hue;
    }
}
