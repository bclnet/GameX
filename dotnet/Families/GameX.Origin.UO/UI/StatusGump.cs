// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO StatusGumpModern. Client 7.0.113 is past 3.0.8z, so this is not StatusGumpOld (gump 0x0802).
/// Background 0x2A6C. The gumpartLegacyMUL.uop layout: name 90,50, stats 80, values 150 minus 5.
/// Hits stay. Str, dex, int, stamina, and mana stay hidden when HasStats is false.
/// Not stored: stat locks, gold, weight, resists, luck, damage, followers, the buff window, and separator lines.
/// </summary>
public sealed class StatusGump : Gump {
    const ushort StatHue = 0x0386;
    const int BuffIcon = 0;
    readonly Label _name, _hits, _hitsMax, _str, _dex, _intel, _stam, _stamMax, _mana, _manaMax;
    int _rev = -1;

    public StatusGump(World world) : base(world, 0, 0) {
        CanCloseWithRightClick = true;
        CanMove = true;
        AcceptMouseInput = true;
        Add(new GumpPic(0, 0, 0x2A6C, 0));
        Add(new Button(BuffIcon, 0x7538, 0x7539, 0x7539) { X = 40, Y = 50, ButtonAction = ButtonAction.Activate });
        _name = Add(new Label("", false, StatHue, 1) { X = 90, Y = 50 });
        _str = Add(Slot(80, 77));
        _dex = Add(Slot(80, 105));
        _intel = Add(Slot(80, 133));
        var valueX = 150 - 5;
        _hits = Add(Slot(valueX, 70));
        _hitsMax = Add(Slot(valueX, 83));
        _stam = Add(Slot(valueX, 98));
        _stamMax = Add(Slot(valueX, 111));
        _mana = Add(Slot(valueX, 126));
        _manaMax = Add(Slot(valueX, 139));
    }

    static Label Slot(int x, int y) => new Label("", false, StatHue, 1) { X = x, Y = y, IsVisible = false };

    public override void Update() {
        var rev = UOLoginNet.WorldRevision;
        if (rev != _rev) {
            _rev = rev;
            Apply();
        }
        base.Update();
    }

    void Apply() {
        var found = UOLoginNet.TryStatus(UOLoginNet.PlayerSerial, out var status);
        var name = found && !string.IsNullOrEmpty(status.Name) ? status.Name : UOLoginNet.NameOf(UOLoginNet.PlayerSerial);
        _name.Text = name;
        if (found) {
            Show(_hits, status.Hits);
            Show(_hitsMax, status.HitsMax);
        }
        else {
            _hits.IsVisible = false;
            _hitsMax.IsVisible = false;
        }
        var extra = found && status.HasStats;
        if (extra) {
            Show(_str, status.Str);
            Show(_dex, status.Dex);
            Show(_intel, status.Int);
            Show(_stam, status.Stam);
            Show(_stamMax, status.StamMax);
            Show(_mana, status.Mana);
            Show(_manaMax, status.ManaMax);
        }
        else {
            _str.IsVisible = false;
            _dex.IsVisible = false;
            _intel.IsVisible = false;
            _stam.IsVisible = false;
            _stamMax.IsVisible = false;
            _mana.IsVisible = false;
            _manaMax.IsVisible = false;
        }
    }

    static void Show(Label label, ushort value) {
        label.Text = value.ToString();
        label.IsVisible = true;
    }
}
