// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO RacialAbilityButton. One gump per graphic. The picture is that graphic at hue 0.
/// Right-click closes. CanMove is set. Shift-click from the book creates it 20 pixels up and left of the mouse.
/// A second Shift-click of the same graphic shows this one instead of adding another.
/// Double-click sends Send_ToggleGargoyleFlying when the graphic is 0x5DDA. It does not read the body.
/// Any other graphic does not send. The cliloc tooltip is not shown. No tooltip host is wired here.
/// </summary>
public sealed class RacialAbilityButton : Gump {
    public RacialAbilityButton(World world, ushort graphic) : base(world, 0, 0) {
        Graphic = graphic;
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        var pic = new GumpPic(0, 0, graphic, 0) { AcceptMouseInput = false, CanMove = true };
        if (pic.IsDisposed) return;
        Add(pic);
        Width = pic.Width;
        Height = pic.Height;
    }

    public ushort Graphic { get; }

    /// <summary>One button per graphic. Brings that one to the front.</summary>
    public static bool ShowExisting(ushort graphic) {
        var list = UIManager.Gumps;
        for (var n = list.First; n != null; n = n.Next) {
            if (!(n.Value is RacialAbilityButton g) || g.IsDisposed || g.Graphic != graphic) continue;
            g.IsVisible = true;
            if (n != list.First) {
                list.Remove(n);
                list.AddFirst(g);
            }
            return true;
        }
        return false;
    }

    public override bool OnMouseDoubleClick(int x, int y) {
        if (Graphic != 0x5DDA) return false;
        UOLoginNet.Send(OutgoingPackets.Send_ToggleGargoyleFlying());
        return true;
    }
}
