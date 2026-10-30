// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// Opens the gumps for packets that are already parsed. Each Take runs once.
/// A serial that is already open is updated. It is not opened again.
/// 0x90 or 0xF5 opens the map and 0x56 updates it. 0x93 or 0xD4 opens the book and 0x66 fills pages.
/// 0xE6 runs after 0xE5, so a remove in the same call clears the arrow that call just showed.
/// 0xBF subcommand 0x2A opens the race changer. A later packet updates that gump.
/// A window close runs after those opens. A missing gump is left alone.
/// House type 4 opens empty chrome. Type 5 disposes it. 0xD8 still fills the planes.
/// </summary>
public static class PacketWindows {
    public static void Poll() {
        var world = UIManager.GetGump<WorldGump>()?.World;
        if (PacketHandlers.TakeDisplayMap(out var map)) MapGump.Open(world, map);
        if (PacketHandlers.TakeMapData(out var pins)) MapGump.OnData(pins);
        if (PacketHandlers.TakeOpenBook(out var book)) ModernBookGump.Open(world, book);
        if (PacketHandlers.TakeBookData(out var pages)) ModernBookGump.OnPages(pages);
        if (PacketHandlers.TakeBulletin(out var board)) BulletinBoardGump.Open(world, board);
        if (PacketHandlers.TakeTipWindow(out var tip)) TipNoticeGump.Open(world, tip);
        if (PacketHandlers.TakeCharacterProfile(out var profile)) ProfileGump.Open(world, profile);
        if (PacketHandlers.TakeDisplayWaypoint(out var point)) QuestArrowGump.Open(world, point);
        if (PacketHandlers.TakeRemoveWaypoint(out var gone)) QuestArrowGump.Close(gone.Serial);
        BuffGump.Refresh(world);
        if (PacketHandlers.TakeChatMessage(out var chat)) ChatGump.OnMessage(world, chat);
        if (PacketHandlers.TakeCustomHouse(out var house)) HouseCustomizationGump.Open(world, house);
        if (PacketHandlers.TakeRaceChange(out var race)) RaceChangeGump.Open(world, race);
        PartyGump.Poll();
        while (UOLoginNet.TakeWindowClose(out var kind, out var serial)) CloseWindow(kind, serial);
        while (UOLoginNet.TakeHouseWindow(out var houseSerial, out var houseType)) HouseWindow(world, houseSerial, houseType);
    }

    /// <summary>
    /// Kind 1 is the paperdoll. Kind 2 is the hits line, which is not a gump, and the player status.
    /// Kind 8 is the profile. Kind 0x0C is the container.
    /// </summary>
    static void CloseWindow(uint kind, uint serial) {
        switch (kind) {
            case 1: UIManager.GetGump<PaperdollGump>(serial)?.Dispose(); break;
            case 2:
                if (serial == UOLoginNet.PlayerSerial) UIManager.GetGump<StatusGump>()?.Dispose();
                break;
            case 8: UIManager.GetGump<ProfileGump>(serial)?.Dispose(); break;
            case 0x0C: UIManager.GetGump<ContainerGump>(serial)?.Dispose(); break;
        }
    }

    static void HouseWindow(World world, uint serial, byte type) {
        switch (type) {
            case 4: HouseCustomizationGump.Open(world, serial); break;
            case 5: UIManager.GetGump<HouseCustomizationGump>(serial)?.Dispose(); break;
        }
    }
}
