// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO SkillButtonGump. The frame is ResizePic 0x24B8, 88 by 44.
/// The name is unicode, hue 0, font 1, centered in the frame.
/// Left click calls UOLoginNet.UseSkill with the same index the skills window sends.
/// Right-click closes. CanMove is set. One button exists per index.
/// </summary>
public sealed class SkillButtonGump : Gump {
    readonly int _index;

    public SkillButtonGump(World world, int index, string name, int x, int y) : base(world, 0, 0) {
        _index = index;
        X = x;
        Y = y;
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        Width = 88;
        Height = 44;
        WantUpdateSize = false;
        Add(new ResizePic(0x24B8) { Width = Width, Height = Height, AcceptMouseInput = false, CanMove = true });
        var label = new Label(name, true, 0, 1) { CanMove = true };
        var lx = (Width - label.Width) >> 1;
        if (lx < 4) lx = 4;
        label.X = lx;
        label.Y = (Height >> 1) - (label.Height >> 1);
        Add(label);
    }

    public int Index => _index;

    public static SkillButtonGump Find(int index) {
        for (var n = UIManager.Gumps.Last; n != null; n = n.Previous)
            if (n.Value is SkillButtonGump g && !g.IsDisposed && g.Index == index) return g;
        return null;
    }

    /// <summary>The button is already open. Bring it to the front.</summary>
    public void Show() {
        if (IsDisposed) return;
        IsVisible = true;
        UIManager.Gumps.Remove(this);
        UIManager.Add(this);
    }

    public override void OnMouseClick(int x, int y) => UOLoginNet.UseSkill(_index);
}
