// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ScissorControl. The sink has no clip, so DoScissor is stored and children still draw.
/// </summary>
public class ScissorControl : Control {
    public ScissorControl(bool enabled, int x, int y, int width, int height) : this(enabled) {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public ScissorControl(bool enabled) {
        CanMove = false;
        AcceptMouseInput = false;
        AcceptKeyboardInput = false;
        Alpha = 1.0f;
        WantUpdateSize = false;
        DoScissor = enabled;
    }

    public bool DoScissor;

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        return base.Draw(x, y);
    }
}
