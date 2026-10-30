// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO UseSpellButtonGump. The graphic is SpellDefinition.GumpIconSmallID.
/// Hue is 38 while IsSpellActive is true, and 0 otherwise. CastSpell is unchanged.
/// Magery is 0x08C0, necromancy 0x5000, chivalry 0x5100, bushido 0x5420, ninjitsu 0x5320,
/// spellweaving 0x59D8, mysticism 0x5DC0, each plus the 0-based index in that book.
/// Bushido and ninjitsu are 0x20 past the page icons. Mastery is 0x0945 for 701 through 706 and 0x9B8B after.
/// A left click calls UOLoginNet.CastSpell with that spell id, the index the book already passes.
/// UIManager does not drag a child off its parent. A Shift-click on the book icon creates one of these
/// at the mouse, 22 pixels up and left, the offset ClassicUO uses. A second Shift-click of that id
/// shows the existing one instead of adding another.
/// </summary>
public sealed class UseSpellButtonGump : Gump {
    readonly GumpPic _pic;

    public UseSpellButtonGump(World world, int spellId) : base(world, 0, 0) {
        SpellID = spellId;
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        var graphic = SmallIcon(spellId);
        if (graphic == 0) return;
        var hue = UOLoginNet.IsSpellActive(spellId) ? (ushort)38 : (ushort)0;
        _pic = new GumpPic(0, 0, graphic, hue) { AcceptMouseInput = false, CanMove = false };
        if (_pic.IsDisposed) {
            _pic = null;
            return;
        }
        Add(_pic);
        Width = _pic.Width;
        Height = _pic.Height;
    }

    public int SpellID { get; }

    public override void Update() {
        if (_pic != null && !_pic.IsDisposed) {
            var hue = UOLoginNet.IsSpellActive(SpellID) ? (ushort)38 : (ushort)0;
            if (_pic.Hue != hue) _pic.Hue = hue;
        }
        base.Update();
    }

    public override void OnMouseClick(int x, int y) {
        if (SpellID > 0) UOLoginNet.CastSpell(SpellID);
    }

    /// <summary>One button per spell id. Brings that one to the front.</summary>
    public static bool ShowExisting(int spellId) {
        var list = UIManager.Gumps;
        for (var n = list.First; n != null; n = n.Next) {
            if (!(n.Value is UseSpellButtonGump g) || g.IsDisposed || g.SpellID != spellId) continue;
            g.IsVisible = true;
            if (n != list.First) {
                list.Remove(n);
                list.AddFirst(g);
            }
            return true;
        }
        return false;
    }

    static ushort SmallIcon(int id) {
        if (id >= 1 && id <= 64) return (ushort)(0x08C0 + (id - 1));
        if (id >= 101 && id <= 117) return (ushort)(0x5000 + (id - 101));
        if (id >= 201 && id <= 210) return (ushort)(0x5100 + (id - 201));
        if (id >= 401 && id <= 406) return (ushort)(0x5420 + (id - 401));
        if (id >= 501 && id <= 508) return (ushort)(0x5320 + (id - 501));
        if (id >= 601 && id <= 616) return (ushort)(0x59D8 + (id - 601));
        if (id >= 678 && id <= 693) return (ushort)(0x5DC0 + (id - 678));
        if (id >= 701 && id <= 706) return (ushort)(0x0945 + (id - 701));
        if (id >= 707 && id <= 745) return (ushort)(0x9B8B + (id - 707));
        return 0;
    }
}
