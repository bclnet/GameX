// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>ClassicUO HitBox. The white fill is drawn only while the pointer is over it.</summary>
public class HitBox : Control {
    public HitBox(int x, int y, int w, int h, string tooltip = null, float alpha = 0.25f) {
        CanMove = false;
        AcceptMouseInput = true;
        Alpha = alpha;
        X = x;
        Y = y;
        Width = w;
        Height = h;
        WantUpdateSize = false;
        SetTooltip(tooltip);
    }

    public override ClickPriority Priority { get; set; } = ClickPriority.High;
    public bool MouseIsOver => UIManager.MouseOverControl == this;
    public object Tooltip { get; private set; }
    public int TooltipMaxWidth { get; private set; }

    public void SetTooltip(string text, int maxWidth = 0) {
        if (string.IsNullOrEmpty(text)) {
            Tooltip = null;
            TooltipMaxWidth = 0;
            return;
        }
        Tooltip = text;
        TooltipMaxWidth = maxWidth;
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (MouseIsOver && Alpha > 0 && Width > 0 && Height > 0) {
            var v = (int)(Alpha * 31f + 0.5f);
            if (v < 1) v = 1;
            if (v > 31) v = 31;
            var c = (ushort)(0x8000 | (v << 10) | (v << 5) | v);
            var px = new[] { (byte)c, (byte)(c >> 8) };
            GumpArt.Sink?.DrawRaw(unchecked((int)(0x5F400000u + (uint)v)), x, y, Width, Height, px, 1, 1);
        }
        return base.Draw(x, y);
    }
}
