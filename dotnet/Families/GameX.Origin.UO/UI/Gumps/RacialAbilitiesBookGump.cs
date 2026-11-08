// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO RacialAbilitiesBookGump. Book 0x2B29, corners 0x08BB at 50,8 and 0x08BC at 321,8.
/// Two index pages, three names a side. Left names are 62,52 and right names are 225,52, then 15 pixels down.
/// Ability icons are pairs. Left is 62,40 with the name at 112,34. Right is 225,40 with the name at 275,34.
/// A passive line is at the name x, 64. The rule is 0x0835, 120 by 4, at the icon x, 88.
/// The icon base follows the body, the same split ClassicUO stores as race.
/// Human, and any other body, is 0x5DD0 through 0x5DD3. Elf 0x025D through 0x0260 is 0x5DD4 through 0x5DD9.
/// Gargoyle 0x029A, 0x029B, 0x02B6, and 0x02B7 is 0x5DDA through 0x5DDE.
/// Page count is two plus half the ability count, so human and elf can turn onto an empty last page.
/// The first ability page is page 2, which is also the second index page, so that spread still draws INDEX.
/// A name click opens that ability page. A corner click steps. A corner double-click jumps to the first or last page.
/// Shift-click on an icon creates one RacialAbilityButton for that graphic, 20 pixels up and left of the mouse.
/// A second Shift-click shows that button. There is no drag that pulls a child out.
/// Double-click of 0x5DDA sends Send_ToggleGargoyleFlying and does not read the body. Other icons do not send.
/// Index names are labels. This client has no HoveredLabel, so hover hue 0x33 is not drawn.
/// Page-turn sound 0x0055 is not played. Cliloc tooltips are not shown: ClilocText exists and no tooltip host is wired.
/// </summary>
public sealed class RacialAbilitiesBookGump : Gump {
    readonly Corner _left, _right;
    readonly int _pages;

    public RacialAbilitiesBookGump(World world, int x, int y) : base(world, 0, 0) {
        X = x;
        Y = y;
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        var names = NamesFor(UOLoginNet.PlayerBody, out var icon);
        _pages = 2 + (names.Length >> 1);
        Add(new GumpPic(0, 0, 0x2B29, 0));
        _left = new Corner(this, 50, 8, 0x08BB, false);
        _right = new Corner(this, 321, 8, 0x08BC, true);
        if (!_left.IsDisposed) Add(_left);
        if (!_right.IsDisposed) Add(_right);
        BuildIndex(names);
        BuildAbilities(names, icon);
        SetActivePage(1);
    }

    void SetActivePage(int page) {
        if (page < 1) page = 1;
        else if (page > _pages) page = _pages;
        ActivePage = page;
        if (_left != null && !_left.IsDisposed) _left.Page = page != 1 ? 0 : int.MaxValue;
        if (_right != null && !_right.IsDisposed) _right.Page = page != _pages ? 0 : int.MaxValue;
    }

    void BuildIndex(string[] names) {
        var count = names.Length;
        var offs = 0;
        var toPage = 1;
        for (var page = 1; page <= 2; page++) {
            for (var side = 0; side < 2; side++) {
                var right = side % 2 != 0;
                Add(new Label("INDEX", false, 0x0288, 6) { X = right ? 269 : 106, Y = 10 }, page);
                var y = 0;
                for (var i = 0; i < 3; i++) {
                    if (offs >= count) break;
                    if (offs % 2 == 0) toPage++;
                    Add(new IndexName(this, names[offs], toPage) { X = right ? 225 : 62, Y = 52 + y }, page);
                    y += 15;
                    offs++;
                }
            }
        }
    }

    void BuildAbilities(string[] names, ushort icon) {
        var page = 1;
        for (var i = 0; i < names.Length; i++) {
            int iconX = 62, textX = 112;
            if (i > 0 && i % 2 != 0) {
                iconX = 225;
                textX = 275;
            }
            else page++;
            Add(new Label(names[i], false, 0x0288, 6) { X = textX, Y = 34 }, page);
            if (icon != 0x5DDA || i != 0)
                Add(new Label("Passive", false, 0x0288, 6) { X = textX, Y = 64 }, page);
            var pic = new AbilityIcon(this, iconX, 40, (ushort)(icon + i));
            if (!pic.IsDisposed) Add(pic, page);
            var rule = new GumpPicTiled(iconX, 88, 120, 4, 0x0835);
            if (!rule.IsDisposed) Add(rule, page);
        }
    }

    static string[] NamesFor(ushort body, out ushort icon) {
        if (body >= 0x025D && body <= 0x0260) {
            icon = 0x5DD4;
            return ElfNames;
        }
        if (body == 0x029A || body == 0x029B || body == 0x02B6 || body == 0x02B7) {
            icon = 0x5DDA;
            return GargoyleNames;
        }
        icon = 0x5DD0;
        return HumanNames;
    }

    sealed class Corner : GumpPic {
        readonly RacialAbilitiesBookGump _owner;
        readonly bool _next;

        public Corner(RacialAbilitiesBookGump owner, int x, int y, ushort graphic, bool next) : base(x, y, graphic, 0) {
            _owner = owner;
            _next = next;
        }

        public override void OnMouseClick(int x, int y) => _owner.SetActivePage(_next ? _owner.ActivePage + 1 : _owner.ActivePage - 1);

        public override bool OnMouseDoubleClick(int x, int y) {
            _owner.SetActivePage(_next ? _owner._pages : 1);
            return true;
        }
    }

    sealed class IndexName : Label {
        readonly RacialAbilitiesBookGump _owner;
        readonly int _to;

        public IndexName(RacialAbilitiesBookGump owner, string text, int to) : base(text, false, 0x0288, 9) {
            _owner = owner;
            _to = to;
            AcceptMouseInput = true;
            CanMove = true;
        }

        public override void OnMouseClick(int x, int y) => _owner.SetActivePage(_to);
    }

    sealed class AbilityIcon : GumpPic {
        readonly RacialAbilitiesBookGump _book;

        public AbilityIcon(RacialAbilitiesBookGump book, int x, int y, ushort graphic) : base(x, y, graphic, 0) => _book = book;

        public override void OnMouseClick(int x, int y) {
            if (!GameController.Shift) return;
            if (RacialAbilityButton.ShowExisting(Graphic)) return;
            UIManager.Add(new RacialAbilityButton(_book.World, Graphic) {
                X = UIManager.MouseX - 20,
                Y = UIManager.MouseY - 20,
            });
        }

        public override bool OnMouseDoubleClick(int x, int y) {
            if (Graphic != 0x5DDA) return false;
            UOLoginNet.Send(OutgoingPackets.Send_ToggleGargoyleFlying());
            return true;
        }
    }

    static readonly string[] HumanNames = { "Strong Back", "Tough", "Workhorse", "Jack of All Trades" };
    static readonly string[] ElfNames = {
        "Night Sight", "Infused with Magic", "Knowledge of Nature", "Difficult to Track", "Perception", "Wisdom",
    };
    static readonly string[] GargoyleNames = { "Flying", "Berserk", "Master Artisan", "Deadly Aim", "Mystic Insight" };
}
