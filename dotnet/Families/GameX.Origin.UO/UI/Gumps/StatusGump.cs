// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO StatusGumpModern. Client 7.0.113 is past 3.0.8z, so this is not StatusGumpOld (gump 0x0802).
/// Background 0x2A6C. The gumpartLegacyMUL.uop layout: name 90,50, stats 80, values 150 minus 5.
/// Hits stay. Str, dex, int, stamina, and mana stay hidden when HasStats is false.
/// Locks sit beside those three at x 28: up, down, or locked. A click sends Send_StatLockStateRequest and cycles 0, 1, 2.
/// Not stored: gold, weight, resists, luck, damage, followers, the buff window, and separator lines.
/// </summary>
public sealed class StatusGump : Gump {
    const ushort StatHue = 0x0386;
    const int BuffIcon = 0;
    readonly Label _name, _hits, _hitsMax, _str, _dex, _intel, _stam, _stamMax, _mana, _manaMax, _buffs;
    readonly List<UOBuffState> _buffRows = [];
    readonly StatLock _strLock, _dexLock, _intLock;
    byte _strL, _dexL, _intL, _seenStr, _seenDex, _seenInt;
    bool _seenLocks, _showStats;
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
        _strLock = Add(new StatLock(this, 0, 28, 77));
        _dexLock = Add(new StatLock(this, 1, 28, 105));
        _intLock = Add(new StatLock(this, 2, 28, 133));
        var valueX = 150 - 5;
        _hits = Add(Slot(valueX, 70));
        _hitsMax = Add(Slot(valueX, 83));
        _stam = Add(Slot(valueX, 98));
        _stamMax = Add(Slot(valueX, 111));
        _mana = Add(Slot(valueX, 126));
        _manaMax = Add(Slot(valueX, 139));
        _buffs = Add(new Label("", false, StatHue, 1) { X = 40, Y = 170, IsVisible = false });
    }

    static Label Slot(int x, int y) => new Label("", false, StatHue, 1) { X = x, Y = y, IsVisible = false };

    public override void Update() {
        var rev = UOLoginNet.WorldRevision;
        if (rev != _rev) {
            _rev = rev;
            Apply();
        }
        ShowLocks();
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
        _showStats = extra;
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
        _buffRows.Clear();
        UOLoginNet.CopyBuffs(_buffRows);
        if (_buffRows.Count == 0) _buffs.IsVisible = false;
        else {
            var text = "";
            foreach (var buff in _buffRows) {
                var bit = string.IsNullOrEmpty(buff.Text) ? buff.Graphic.ToString() : buff.Text;
                text = text.Length == 0 ? bit : text + "  " + bit;
                if (text.Length > 80) break;
            }
            _buffs.Text = text;
            _buffs.IsVisible = true;
        }
    }

    static void Show(Label label, ushort value) {
        label.Text = value.ToString();
        label.IsVisible = true;
    }

    void ShowLocks() {
        UOLoginNet.StatLocks(out var str, out var dex, out var intel);
        if (!_seenLocks || str != _seenStr || dex != _seenDex || intel != _seenInt) {
            _strL = _seenStr = str;
            _dexL = _seenDex = dex;
            _intL = _seenInt = intel;
            _seenLocks = true;
        }
        if (_showStats) {
            _strLock.ShowState(_strL);
            _dexLock.ShowState(_dexL);
            _intLock.ShowState(_intL);
        }
        else {
            _strLock.IsVisible = false;
            _dexLock.IsVisible = false;
            _intLock.IsVisible = false;
        }
    }

    void CycleLock(byte stat) {
        var cur = stat == 0 ? _strL : stat == 1 ? _dexL : _intL;
        var next = (byte)((cur + 1) % 3);
        if (stat == 0) _strL = next;
        else if (stat == 1) _dexL = next;
        else _intL = next;
        UOLoginNet.Send(OutgoingPackets.Send_StatLockStateRequest(stat, next));
        if (!_showStats) return;
        if (stat == 0) _strLock.ShowState(next);
        else if (stat == 1) _dexLock.ShowState(next);
        else _intLock.ShowState(next);
    }

    static string LockWord(byte state) => state == 1 ? "down" : state == 2 ? "locked" : "up";

    sealed class StatLock : Label {
        readonly StatusGump _owner;
        readonly byte _stat;

        public StatLock(StatusGump owner, byte stat, int x, int y) : base("", false, StatHue, 1) {
            _owner = owner;
            _stat = stat;
            X = x;
            Y = y;
            AcceptMouseInput = true;
            IsVisible = false;
        }

        public void ShowState(byte state) {
            Text = LockWord(state);
            IsVisible = true;
        }

        public override void OnMouseClick(int x, int y) => _owner.CycleLock(_stat);
    }
}
