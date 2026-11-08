// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO MacroGump. The panel is 260 by 200, centered on the backbuffer, 150 pixels down.
/// Alpha 0.8 has no partial pixel, so the fill is the same opaque dark pixel as the 0.7 bars.
/// The action list is ClassicUO MacroControl, which is not this file, so it is not added.
/// UIManager does not drag gumps. CanMove is set and the panel stays where Place put it.
/// Profile path is not ported.
/// </summary>
public sealed class MacroGump : Gump {
    const int FillKey = 0x57400001;
    const int PanelW = 260;
    const int PanelH = 200;

    // r = g = b = 4, opaque. Stand-in for AlphaBlendControl.
    static readonly byte[] Dark = { 0x84, 0x90 };

    bool _placed;

    public MacroGump(World world, string name) : base(world, 0, 0) {
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        WantUpdateSize = false;
        Width = PanelW;
        Height = PanelH;
        _placed = false;
        Place();
        Add(new Label("Edit macro: " + (name ?? ""), true, 15, 1) { X = 20, Y = 2 });
        // Profile path is not ported.
    }

    public override void Update() {
        if (!_placed) {
            Place();
            if (Mouse.Current.BufW > 0) _placed = true;
        }
        base.Update();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (Width > 0 && Height > 0)
            GumpArt.Sink?.DrawRaw(FillKey, x, y, Width, Height, Dark, 1, 1);
        return base.Draw(x, y);
    }

    void Place() {
        var s = Mouse.Current;
        var bw = s.BufW > 0 ? s.BufW : 640;
        var bh = s.BufH > 0 ? s.BufH : 480;
        X = bw / 2 - 125;
        Y = 150;
        if (X < 0) X = 0;
        if (Y < 0) Y = 0;
        if (X > bw - Width) X = MathClamp(bw - Width);
        if (Y > bh - Height) Y = MathClamp(bh - Height);
    }

    static int MathClamp(int v) => v < 0 ? 0 : v;
}
