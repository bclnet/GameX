// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.Netcode.UO;

public static partial class PacketHandlers {
    /// <summary>Handlers ClassicUO registers and UOLoginNet does not already apply.</summary>
    public static void Incoming(byte[] p, int o, int len) {
        if (p == null || len < 1 || o < 0 || o >= p.Length) return;
        switch (p[o]) {
            case 0x03: ClientTalk(p, o, len); break;
            case 0x15: FollowR(p, o, len); break;
            case 0x32: Unknown_0x32(p, o, len); break;
            case 0x56: MapData(p, o, len); break;
            case 0x66: BookData(p, o, len); break;
            case 0x71: BulletinBoardData(p, o, len); break;
            case 0x86: UpdateCharacterList(p, o, len); break;
            case 0x90:
            case 0xF5: DisplayMap(p, o, len); break;
            case 0x93:
            case 0xD4: OpenBook(p, o, len); break;
            case 0x99: MultiPlacement(p, o, len); break;
            case 0xA5: OpenUrl(p, o, len); break;
            case 0xA6: TipWindow(p, o, len); break;
            case 0xAF: DisplayDeath(p, o, len); break;
            case 0xB2: ChatMessage(p, o, len); break;
            case 0xB7: Help(p, o, len); break;
            case 0xB8: CharacterProfile(p, o, len); break;
            case 0xBF: RaceChange(p, o, len); break;
            case 0xBB: UltimaMessengerR(p, o, len); break;
            case 0xC4: Semivisible(p, o, len); break;
            case 0xC6: InvalidMapEnable(p, o, len); break;
            case 0xCA: GetUserServerPingGodClientR(p, o, len); break;
            case 0xCB: GlobalQueCount(p, o, len); break;
            case 0xD0: ConfigurationFileR(p, o, len); break;
            case 0xD3: UpdateObject(p, o, len); break;
            case 0xD7: GenericAOSCommandsR(p, o, len); break;
            case 0xD8: CustomHouse(p, o, len); break;
            case 0xDB: CharacterTransferLog(p, o, len); break;
            case 0xE3: KREncryptionResponse(p, o, len); break;
            case 0xE5: DisplayWaypoint(p, o, len); break;
            case 0xE6: RemoveWaypoint(p, o, len); break;
            case 0xF6: BoatMoving(p, o, len); break;
            case 0xF7: PacketList(p, o, len); break;
            case 0xFD: LoginDelay(p, o, len); break;
        }
    }
}
