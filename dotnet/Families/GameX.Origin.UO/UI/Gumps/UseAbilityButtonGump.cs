// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO UseAbilityButtonGump. CombatBookGump creates one primary and one secondary.
/// UIManager does not drag a child off its parent, so the icon stays on the book.
/// A click calls CombatBookGump.Use, which sends 0xD7 UseCombatAbility.
/// Index is the 1-based ability (AbilityDefinition.Index). Hue 38 is the active bit 0x80.
/// </summary>
public sealed class UseAbilityButtonGump : Gump {
    readonly CombatBookGump _book;
    GumpPic _pic;
    ushort _graphic;

    public UseAbilityButtonGump(CombatBookGump book, bool primary) : base(book.World, 0, 0) {
        _book = book;
        IsPrimary = primary;
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = false;
    }

    public bool IsPrimary { get; }
    public int Index { get; private set; }

    public void Apply(byte ability) {
        var index = ability & 0x7F;
        var on = (ability & 0x80) != 0;
        Index = index;
        var graphic = (ushort)(index >= 1 && index <= 32 ? 0x5200 + index - 1 : 0x5200);
        var hue = on ? (ushort)38 : (ushort)0;
        if (_graphic != graphic || _pic == null || _pic.IsDisposed) {
            if (_graphic == graphic) return;
            if (_pic != null && !_pic.IsDisposed) {
                _pic.Parent = null;
                _pic.Dispose();
            }
            _graphic = graphic;
            _pic = new GumpPic(0, 0, graphic, hue) { AcceptMouseInput = false, CanMove = false };
            if (_pic.IsDisposed) _pic = null;
            else Add(_pic);
        }
        else _pic.Hue = hue;
        if (_pic != null) {
            Width = _pic.Width;
            Height = _pic.Height;
        }
    }

    public override void OnMouseClick(int x, int y) => _book.Use(IsPrimary);
}
