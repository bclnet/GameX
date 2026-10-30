// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// 0xD8 custom house. One gump per house serial. A later packet replaces the planes and the revision.
/// 0xBF type 4 opens this chrome with an empty plane list when that serial is not already open.
/// Each row is one component: id, x, y, z, and that plane's mode. The tile designer is not drawn.
/// Go-floor, backup, restore, sync, clear, commit, and revert send 0xD7 with the player serial.
/// Close sends the building exit. Wall, door, erase, and the other tools are drawn and do not send.
/// Commit sends. This list has no piece-legality flags.
/// </summary>
public sealed class HouseCustomizationGump : Gump {
    /// <summary>Button ids from the custom-house gump, in that order.</summary>
    enum Id {
        Wall = 1,
        Door = 2,
        Floor = 3,
        Stair = 4,
        Roof = 5,
        Misc = 6,
        Erase = 7,
        Eyedropper = 8,
        Menu = 9,
        Vis1 = 10,
        Vis2 = 11,
        Vis3 = 12,
        Vis4 = 13,
        Floor1 = 14,
        Floor2 = 15,
        Floor3 = 16,
        Floor4 = 17,
        ListLeft = 18,
        ListRight = 19,
        Backup = 20,
        Restore = 21,
        Sync = 22,
        Clear = 23,
        Commit = 24,
        Revert = 25,
        Category = 26,
        ShowWindow = 27,
        RoofUp = 28,
        RoofDown = 29,
    }

    readonly Label _components, _fixtures, _cost, _revision;
    readonly List<Label> _lines = [];
    bool _closed;

    public HouseCustomizationGump(World world, UOCustomHouse house) : base(world, house.Serial, house.Serial) {
        X = 50;
        Y = 50;
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = false;
        Pic(0, 17, 0x55F0);
        Pic(486, 17, 0x55F2);
        Tile(121, 36, 397, 120, 0x0E14);
        Tile(153, 17, 333, 154, 0x55F1);
        Tool(Id.Wall, 0x5654, 0x5656, 0x5655, 9, 41);
        Tool(Id.Door, 0x5657, 0x5659, 0x5658, 39, 40);
        Tool(Id.Floor, 0x565A, 0x565C, 0x565B, 70, 40);
        Tool(Id.Stair, 0x565D, 0x565F, 0x565E, 9, 72);
        Tool(Id.Roof, 0x5788, 0x578A, 0x5789, 39, 72);
        Tool(Id.Misc, 0x5663, 0x5665, 0x5664, 69, 72);
        Tool(Id.Erase, 0x5666, 0x5668, 0x5667, 9, 100);
        Tool(Id.Eyedropper, 0x5669, 0x566B, 0x566A, 39, 100);
        Tool(Id.Menu, 0x566C, 0x566E, 0x566D, 69, 100);
        Tool(Id.Vis1, 0x572E, 0x5730, 0x572F, 533, 108);
        Tool(Id.Floor1, 0x56CD, 0x56D1, 0x56CD, 583, 96);
        Tool(Id.Floor1, 0x56F6, 0x56F8, 0x56F7, 623, 103);
        Tool(Id.Vis2, 0x5725, 0x5727, 0x5726, 533, 86);
        Tool(Id.Floor2, 0x56CE, 0x56D2, 0x56CE, 583, 73);
        Tool(Id.Floor2, 0x56F0, 0x56F2, 0x56F1, 623, 86);
        Tool(Id.Vis3, 0x5725, 0x5727, 0x5726, 533, 64);
        Tool(Id.Floor3, 0x56CE, 0x56D2, 0x56CE, 582, 56);
        Tool(Id.Floor3, 0x56F0, 0x56F2, 0x56F1, 623, 69);
        Tool(Id.Vis4, 0x5725, 0x5727, 0x5726, 533, 42);
        Tool(Id.Floor4, 0x56D0, 0x56D4, 0x56D0, 583, 42);
        Tool(Id.Floor4, 0x56EA, 0x56EC, 0x56EB, 623, 50);
        Tool(Id.ListLeft, 0x5625, 0x5627, 0x5626, 110, 63);
        Tool(Id.ListRight, 0x5628, 0x562A, 0x5629, 510, 63);
        Tool(Id.Category, 0x5622, 0x5624, 0x5623, 167, 5);
        Tool(Id.ShowWindow, 0x562B, 0x562D, 0x562C, 228, 9);
        Tool(Id.RoofDown, 0x578B, 0x578D, 0x578C, 305, 0);
        Tool(Id.RoofUp, 0x578E, 0x5790, 0x578F, 349, 0);
        Command(Id.Backup, "Backup", 150, 50);
        Command(Id.Restore, "Restore", 150, 90);
        Command(Id.Sync, "Sync", 270, 50);
        Command(Id.Clear, "Clear", 270, 90);
        Command(Id.Commit, "Commit", 390, 50);
        Command(Id.Revert, "Revert", 390, 90);
        _components = new Label("", false, 0x0481, 9) { Y = 142 };
        _fixtures = new Label("0", false, 0x0481, 9) { X = 94, Y = 142 };
        _cost = new Label("0", false, 0x0481, 9) { X = 524, Y = 142 };
        _revision = new Label("", false, 0x0481, 9) { X = 360, Y = 142 };
        Add(new Label(":", false, 0x0481, 9) { X = 84, Y = 142 });
        Add(_components);
        Add(_fixtures);
        Add(_cost);
        Add(_revision);
        Add(new Label("id x y z mode", false, 0x0481, 9) { X = 130, Y = 176 });
        Show(house);
    }

    /// <summary>Opens the house, or replaces the planes and revision when that serial is already open.</summary>
    public static void Open(World world, UOCustomHouse house) {
        var open = UIManager.GetGump<HouseCustomizationGump>(house.Serial);
        if (open == null) UIManager.Add(new HouseCustomizationGump(world, house));
        else open.Show(house);
    }

    /// <summary>0xBF type 4. No planes. An open gump for this serial is left as it is.</summary>
    public static void Open(World world, uint serial) {
        if (UIManager.GetGump<HouseCustomizationGump>(serial) != null) return;
        UIManager.Add(new HouseCustomizationGump(world, new UOCustomHouse(false, false, serial, 0, [])));
    }

    public override void OnButtonClick(int buttonID) {
        var player = UOLoginNet.PlayerSerial;
        switch ((Id)buttonID) {
            case Id.Floor1: UOLoginNet.Send(OutgoingPackets.Send_CustomHouseGoToFloor(player, 1)); break;
            case Id.Floor2: UOLoginNet.Send(OutgoingPackets.Send_CustomHouseGoToFloor(player, 2)); break;
            case Id.Floor3: UOLoginNet.Send(OutgoingPackets.Send_CustomHouseGoToFloor(player, 3)); break;
            case Id.Floor4: UOLoginNet.Send(OutgoingPackets.Send_CustomHouseGoToFloor(player, 4)); break;
            case Id.Backup: UOLoginNet.Send(OutgoingPackets.Send_CustomHouseBackup(player)); break;
            case Id.Restore: UOLoginNet.Send(OutgoingPackets.Send_CustomHouseRestore(player)); break;
            case Id.Sync: UOLoginNet.Send(OutgoingPackets.Send_CustomHouseSync(player)); break;
            case Id.Clear: UOLoginNet.Send(OutgoingPackets.Send_CustomHouseClear(player)); break;
            case Id.Commit: UOLoginNet.Send(OutgoingPackets.Send_CustomHouseCommit(player)); break;
            case Id.Revert: UOLoginNet.Send(OutgoingPackets.Send_CustomHouseRevert(player)); break;
        }
    }

    /// <summary>Close sends 0xD7 building exit. The serial is the player, same as the menu row.</summary>
    public override void Dispose() {
        if (IsDisposed) return;
        if (!_closed) {
            _closed = true;
            UOLoginNet.Send(OutgoingPackets.Send_CustomHouseBuildingExit(UOLoginNet.PlayerSerial));
        }
        base.Dispose();
    }

    void Show(UOCustomHouse house) {
        var planes = house.Planes ?? [];
        foreach (var line in _lines) {
            line.Parent = null;
            line.Dispose();
        }
        _lines.Clear();
        var y = 192;
        var count = 0;
        for (var p = 0; p < planes.Length; p++) {
            var plane = planes[p];
            var parts = plane.Components;
            if (parts == null) continue;
            for (var i = 0; i < parts.Length; i++) {
                var c = parts[i];
                var row = new Label($"{c.Id:X4} {c.X} {c.Y} {c.Z} {plane.Mode}", false, 0x0481, 9) { X = 130, Y = y };
                Add(row);
                _lines.Add(row);
                y += 16;
                count++;
            }
        }
        _components.Text = count.ToString();
        _components.X = 82 - _components.Width;
        _fixtures.Text = "0";
        _cost.Text = (count * 500).ToString();
        _revision.Text = house.Revision.ToString();
        WantUpdateSize = true;
    }

    void Pic(int x, int y, ushort graphic) {
        var pic = new GumpPic(x, y, graphic, 0);
        if (!pic.IsDisposed) Add(pic);
    }

    void Tile(int x, int y, int width, int height, ushort graphic) {
        var pic = new GumpPicTiled(x, y, width, height, graphic);
        if (!pic.IsDisposed) Add(pic);
    }

    void Tool(Id id, ushort normal, ushort pressed, ushort over, int x, int y) {
        var button = new Button((int)id, normal, pressed, over) { X = x, Y = y, ButtonAction = ButtonAction.Activate };
        if (!button.IsDisposed) Add(button);
    }

    void Command(Id id, string text, int x, int y) {
        Tool(id, 0x098D, 0x098D, 0x098D, x, y);
        Add(new Label(text, false, 0x0036) { X = x, Y = y });
    }
}
