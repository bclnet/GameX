// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameX.Origin.Netcode.UO;

/// <summary>ClassicUO outgoing packets from dye data through the UOLive hash response. No socket.</summary>
public static partial class OutgoingPackets {
    // 7.0.104 table. Lengths in this file do not change with version, except 0xFA and 0xFB (fixed from 7.0.64).
    static readonly PacketsTable _moreLengths = new(ClientVersion.CV_7010400);

    /// <summary>0x95 dye response. Serial, a zero word, and hue. Graphic is not written.</summary>
    public static byte[] Send_DyeDataResponse(uint serial, ushort graphic, ushort hue) {
        var w = new MoreW(0x95);
        w.U32(serial);
        w.U16(0);
        w.U16(hue);
        return w.Finish();
    }

    /// <summary>0xB8 profile request. Mode 0 and the serial.</summary>
    public static byte[] Send_ProfileRequest(uint serial) {
        var w = new MoreW(0xB8);
        w.U8(0x00);
        w.U32(serial);
        return w.Finish();
    }

    /// <summary>0xB8 profile update. Mode 1, serial, flag 1, char count, and UTF-16BE text.</summary>
    public static byte[] Send_ProfileUpdate(uint serial, string text) {
        var w = new MoreW(0xB8);
        w.U8(0x01);
        w.U32(serial);
        w.U16(0x01);
        w.U16((ushort)text.Length);
        w.Uni(text, text.Length);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x07. One byte, 1 when the quest arrow click is the right button.</summary>
    public static byte[] Send_ClickQuestArrow(bool rightClick) {
        var w = new MoreW(0xBF);
        w.U16(0x07);
        w.Bool(rightClick);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x0C. Closes the status bar for this serial.</summary>
    public static byte[] Send_CloseStatusBarGump(uint serial) {
        var w = new MoreW(0xBF);
        w.U16(0x0C);
        w.U32(serial);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x06 command 1. Party invite, target serial 0.</summary>
    public static byte[] Send_PartyInviteRequest() {
        var w = new MoreW(0xBF);
        w.U16(0x06);
        w.U8(1);
        w.U32(0);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x06 command 2. Remove this party member.</summary>
    public static byte[] Send_PartyRemoveRequest(uint serial) {
        var w = new MoreW(0xBF);
        w.U16(0x06);
        w.U8(2);
        w.U32(serial);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x06 command 0x06. Party loot type, 1 when free.</summary>
    public static byte[] Send_PartyChangeLootTypeRequest(bool type) {
        var w = new MoreW(0xBF);
        w.U16(0x06);
        w.U8(0x06);
        w.Bool(type);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x06 command 0x08. Accept a party invite from this serial.</summary>
    public static byte[] Send_PartyAccept(uint serial) {
        var w = new MoreW(0xBF);
        w.U16(0x06);
        w.U8(0x08);
        w.U32(serial);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x06 command 0x09. Decline a party invite from this serial.</summary>
    public static byte[] Send_PartyDecline(uint serial) {
        var w = new MoreW(0xBF);
        w.U16(0x06);
        w.U8(0x09);
        w.U32(serial);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x06. Command 3 plus serial when the serial is valid, otherwise command 4. UTF-16BE text.</summary>
    public static byte[] Send_PartyMessage(string text, uint serial) {
        var w = new MoreW(0xBF);
        w.U16(0x06);
        if (serial > 0 && serial < 0x80000000) {
            w.U8(0x03);
            w.U32(serial);
        }
        else w.U8(0x04);
        w.UniZ(text);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x05. Game window width and height.</summary>
    public static byte[] Send_GameWindowSize(uint w, uint h) {
        var wtr = new MoreW(0xBF);
        wtr.U16(0x05);
        wtr.U32(w);
        wtr.U32(h);
        return wtr.Finish();
    }

    /// <summary>0x71 command 3. Request one bulletin message.</summary>
    public static byte[] Send_BulletinBoardRequestMessage(uint serial, uint msgSerial) {
        var w = new MoreW(0x71);
        w.U8(0x03);
        w.U32(serial);
        w.U32(msgSerial);
        return w.Finish();
    }

    /// <summary>0x71 command 4. Request a bulletin message summary.</summary>
    public static byte[] Send_BulletinBoardRequestMessageSummary(uint serial, uint msgSerial) {
        var w = new MoreW(0x71);
        w.U8(0x04);
        w.U32(serial);
        w.U32(msgSerial);
        return w.Finish();
    }

    /// <summary>0x71 command 5. Post a bulletin. Subject and body are UTF-8, CR LF collapsed to LF.</summary>
    public static byte[] Send_BulletinBoardPostMessage(uint serial, uint msgSerial, string subject, string text) {
        subject = subject.Replace("\r\n", "\n");
        text = text.Replace("\r\n", "\n");
        var w = new MoreW(0x71);
        w.U8(0x05);
        w.U32(serial);
        w.U32(msgSerial);
        w.U8((byte)(subject.Length + 1));
        w.Raw(Encoding.UTF8.GetBytes(subject));
        w.U8(0x00);
        var lines = text.Split('\n');
        if (lines.Length > 0) {
            w.U8((byte)lines.Length);
            foreach (var line in lines) {
                var bytes = Encoding.UTF8.GetBytes(line);
                w.U8((byte)(bytes.Length + 1));
                w.Raw(bytes);
                w.U8(0x00);
            }
        }
        else {
            w.U8(0x01);
            var bytes = Encoding.UTF8.GetBytes(text);
            w.U8((byte)(bytes.Length + 1));
            w.Raw(bytes);
            w.U8(0x00);
        }
        return w.Finish();
    }

    /// <summary>0x71 command 6. Remove a bulletin message.</summary>
    public static byte[] Send_BulletinBoardRemoveMessage(uint serial, uint msgSerial) {
        var w = new MoreW(0x71);
        w.U8(0x06);
        w.U32(serial);
        w.U32(msgSerial);
        return w.Finish();
    }

    /// <summary>0xF0 command 1. Query guild member positions. The flag byte is 1.</summary>
    public static byte[] Send_QueryGuildPosition() {
        var w = new MoreW(0xF0);
        w.U8(0x01);
        w.Bool(true);
        return w.Finish();
    }

    /// <summary>0xF0 command 0. Query party member positions.</summary>
    public static byte[] Send_QueryPartyPosition() {
        var w = new MoreW(0xF0);
        w.U8(0x00);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x0B. Three CP1252 language bytes and a zero.</summary>
    public static byte[] Send_Language(string lang) {
        var w = new MoreW(0xBF);
        w.U16(0x0B);
        w.Ascii(lang, 3);
        w.U8(0x00);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x0F. Client type 0x0A, then every bit below the protocol flags value.</summary>
    public static byte[] Send_ClientType(uint protocol) {
        var w = new MoreW(0xBF);
        w.U16(0x0F);
        w.U8(0x0A);
        uint clientFlag = 0;
        for (var i = 0; i < protocol; ++i) clientFlag |= (uint)(1 << i);
        w.U32(clientFlag);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x13. Request the popup menu for this serial.</summary>
    public static byte[] Send_RequestPopupMenu(uint serial) {
        var w = new MoreW(0xBF);
        w.U16(0x13);
        w.U32(serial);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x15. Popup menu choice.</summary>
    public static byte[] Send_PopupMenuSelection(uint serial, ushort menuid) {
        var w = new MoreW(0xBF);
        w.U16(0x15);
        w.U32(serial);
        w.U16(menuid);
        return w.Finish();
    }

    /// <summary>0xB3 join chat. Four-byte language, command 0x62, quoted UTF-16BE name, and an optional password.</summary>
    public static byte[] Send_ChatJoinCommand(string language, string name, string password = null) {
        var w = new MoreW(0xB3);
        w.Ascii(language, 4);
        w.U16(0x62);
        w.U16(0x22);
        w.UniZ(name);
        w.U16(0x22);
        w.U16(0x20);
        if (!string.IsNullOrEmpty(password)) w.UniZ(password);
        return w.Finish();
    }

    /// <summary>0xB3 create chat channel. Four-byte language, command 0x63, UTF-16BE name, optional braced password.</summary>
    public static byte[] Send_ChatCreateChannelCommand(string language, string name, string password = null) {
        var w = new MoreW(0xB3);
        w.Ascii(language, 4);
        w.U16(0x63);
        w.UniZ(name);
        if (!string.IsNullOrEmpty(password)) {
            w.U16(0x7B);
            w.UniZ(password);
            w.U16(0x7D);
        }
        return w.Finish();
    }

    /// <summary>0xB3 leave chat. Four-byte language and command 0x43.</summary>
    public static byte[] Send_ChatLeaveChannelCommand(string language) {
        var w = new MoreW(0xB3);
        w.Ascii(language, 4);
        w.U16(0x43);
        return w.Finish();
    }

    /// <summary>0xB3 chat message. Four-byte language, command 0x61, and UTF-16BE text.</summary>
    public static byte[] Send_ChatMessageCommand(string language, string msg) {
        var w = new MoreW(0xB3);
        w.Ascii(language, 4);
        w.U16(0x61);
        w.UniZ(msg);
        return w.Finish();
    }

    /// <summary>0xB5 open chat. A zero, then up to 30 UTF-16BE characters. Fixed length 64.</summary>
    public static byte[] Send_OpenChat(string name) {
        var w = new MoreW(0xB5);
        w.U8(0x00);
        var len = Math.Min(name.Length, 30);
        if (len > 0) w.Uni(name, len);
        return w.Finish();
    }

    /// <summary>0x56 map pin. Serial, action, pin, x, and y. Fixed length 11.</summary>
    public static byte[] Send_MapMessage(uint serial, byte action, byte pin, ushort x, ushort y) {
        var w = new MoreW(0x56);
        w.U32(serial);
        w.U8(action);
        w.U8(pin);
        w.U16(x);
        w.U16(y);
        return w.Finish();
    }

    /// <summary>0xD7 guild menu. Player serial, subcommand 0x28, terminator 0x0A.</summary>
    public static byte[] Send_GuildMenuRequest(uint playerSerial) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x28);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0xD7 quest menu. Player serial, subcommand 0x32, terminator 0x00.</summary>
    public static byte[] Send_QuestMenuRequest(uint playerSerial) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x32);
        w.U8(0x00);
        return w.Finish();
    }

    /// <summary>0xD7 equip last weapon. Player serial, subcommand 0x1E, terminator 0x0A.</summary>
    public static byte[] Send_EquipLastWeapon(uint playerSerial) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x1E);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0x12 invoke virtue. Command 0xF4 and the id as a CP1252 decimal plus a zero.</summary>
    public static byte[] Send_InvokeVirtueRequest(byte id) {
        var w = new MoreW(0x12);
        w.U8(0xF4);
        w.AsciiZ(id.ToString());
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x10. One mega-cliloc serial, the old request.</summary>
    public static byte[] Send_MegaClilocRequest_Old(uint serial) {
        var w = new MoreW(0xBF);
        w.U16(0x10);
        w.U32(serial);
        return w.Finish();
    }

    /// <summary>0xD6 mega cliloc. Writes the first 15 serials and removes those entries from the list.</summary>
    public static byte[] Send_MegaClilocRequest(List<uint> serials) {
        var w = new MoreW(0xD6);
        var count = Math.Min(15, serials.Count);
        for (var i = 0; i < count; ++i) w.U32(serials[i]);
        serials.RemoveRange(0, count);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x1A. Stat lock. State is 0 up, 1 down, 2 locked.</summary>
    public static byte[] Send_StatLockStateRequest(byte stat, byte state) {
        var w = new MoreW(0xBF);
        w.U16(0x1A);
        w.U8(stat);
        w.U8(state);
        return w.Finish();
    }

    /// <summary>0x3A skill lock change. Skill index and lock byte.</summary>
    public static byte[] Send_SkillStatusChangeRequest(ushort skillindex, byte lockstate) {
        var w = new MoreW(0x3A);
        w.U16(skillindex);
        w.U8(lockstate);
        return w.Finish();
    }

    /// <summary>0x93 old book header. Serial, flags, and UTF-8 title (60) and author (30). Fixed length 99.</summary>
    public static byte[] Send_BookHeaderChanged_Old(uint serial, string title, string author) {
        var w = new MoreW(0x93);
        w.U32(serial);
        w.U8(0x00);
        w.U8(0x01);
        w.U16(0);
        w.Utf8(title, 60);
        w.Utf8(author, 30);
        return w.Finish();
    }

    /// <summary>0xD4 book header. Serial, flags, then UTF-8 byte counts and the title and author.</summary>
    public static byte[] Send_BookHeaderChanged(uint serial, string title, string author) {
        var w = new MoreW(0xD4);
        w.U32(serial);
        w.U8(0x00);
        w.U8(0x00);
        w.U16(0);
        var titleLength = Encoding.UTF8.GetByteCount(title);
        w.U16((ushort)titleLength);
        w.Utf8(title, titleLength);
        var authorLength = Encoding.UTF8.GetByteCount(author);
        w.U16((ushort)authorLength);
        w.Utf8(author, authorLength);
        return w.Finish();
    }

    /// <summary>0x66 book page write. One page of UTF-8 lines, each ended by 0, then a final 0.</summary>
    public static byte[] Send_BookPageData(uint serial, string[] lines, int page) {
        var w = new MoreW(0x66);
        w.U32(serial);
        w.U16(0x01);
        w.U16((ushort)page);
        w.U16((ushort)lines.Length);
        for (var i = 0; i < lines.Length; ++i) {
            if (!string.IsNullOrEmpty(lines[i])) {
                var t = lines[i].Replace("\n", "");
                if (t.Length > 0) {
                    var buffer = new byte[t.Length * 2];
                    var written = Encoding.UTF8.GetBytes(t, 0, t.Length, buffer, 0);
                    w.Raw(buffer.AsSpan(0, written));
                }
            }
            w.U8(0x00);
        }
        w.U8(0x00);
        return w.Finish();
    }

    /// <summary>0x66 book page request. One page index and a line count of 0xFFFF.</summary>
    public static byte[] Send_BookPageDataRequest(uint serial, ushort page) {
        var w = new MoreW(0x66);
        w.U32(serial);
        w.U16(0x01);
        w.U16(page);
        w.U16(0xFFFF);
        return w.Finish();
    }

    /// <summary>0x3B buy. Empty sends a zero; otherwise 0x02 and each item as 0x1A, serial, amount.</summary>
    public static byte[] Send_BuyRequest(uint serial, (uint serial, ushort amount)[] items) {
        var w = new MoreW(0x3B);
        w.U32(serial);
        if (items.Length > 0) {
            w.U8(0x02);
            for (var i = 0; i < items.Length; ++i) {
                w.U8(0x1A);
                w.U32(items[i].serial);
                w.U16(items[i].amount);
            }
        }
        else w.U8(0x00);
        return w.Finish();
    }

    /// <summary>0x9F sell. Vendor serial, count, then each item serial and amount.</summary>
    public static byte[] Send_SellRequest(uint serial, (uint serial, ushort amount)[] items) {
        var w = new MoreW(0x9F);
        w.U32(serial);
        w.U16((ushort)items.Length);
        for (var i = 0; i < items.Length; ++i) {
            w.U32(items[i].serial);
            w.U16(items[i].amount);
        }
        return w.Finish();
    }

    /// <summary>0xD7 combat ability. Player serial, subcommand 0x19, a zero word-pair, the index, terminator 0x0A.</summary>
    public static byte[] Send_UseCombatAbility(uint playerSerial, byte idx) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x19);
        w.U32(0);
        w.U8(idx);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x2C. Select a target object.</summary>
    public static byte[] Send_TargetSelectedObject(uint serial, uint targetSerial) {
        var w = new MoreW(0xBF);
        w.U16(0x2C);
        w.U32(serial);
        w.U32(targetSerial);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x32. Toggle gargoyle flying. The next word is 1 and the dword is 0.</summary>
    public static byte[] Send_ToggleGargoyleFlying() {
        var w = new MoreW(0xBF);
        w.U16(0x32);
        w.U16(0x01);
        w.U32(0);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x1E. Request custom house data for this serial.</summary>
    public static byte[] Send_CustomHouseDataRequest(uint serial) {
        var w = new MoreW(0xBF);
        w.U16(0x1E);
        w.U32(serial);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x09. Stun.</summary>
    public static byte[] Send_StunRequest() {
        var w = new MoreW(0xBF);
        w.U16(0x09);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x0A. Disarm.</summary>
    public static byte[] Send_DisarmRequest() {
        var w = new MoreW(0xBF);
        w.U16(0x0A);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x2A. Change race: skin, hair style, hair hue, beard style, beard hue.</summary>
    public static byte[] Send_ChangeRaceRequest(ushort skinHue, ushort hairStyle, ushort hairHue, ushort beardStyle, ushort beardHue) {
        var w = new MoreW(0xBF);
        w.U16(0x2A);
        w.U16(skinHue);
        w.U16(hairStyle);
        w.U16(hairHue);
        w.U16(beardStyle);
        w.U16(beardHue);
        return w.Finish();
    }

    /// <summary>0xBF subcommand 0x33. Multi boat move. Direction is written twice, then speed.</summary>
    public static byte[] Send_MultiBoatMoveRequest(uint serial, byte direction, byte speed) {
        var w = new MoreW(0xBF);
        w.U16(0x33);
        w.U32(serial);
        w.U8(direction);
        w.U8(direction);
        w.U8(speed);
        return w.Finish();
    }

    /// <summary>0x22 resync. The two bytes after the id are zero. Fixed length 3.</summary>
    public static byte[] Send_Resync() {
        var w = new MoreW(0x22);
        return w.Finish();
    }

    /// <summary>0x02 walk. Byte 1 is direction, or direction with 0x80 when running. Then sequence and fast-walk key. Fixed length 7.</summary>
    public static byte[] Send_WalkRequest(byte direction, byte seq, bool run, uint fastWalk) {
        if (run) direction |= 0x80;
        var w = new MoreW(0x02);
        w.U8(direction);
        w.U8(seq);
        w.U32(fastWalk);
        return w.Finish();
    }

    /// <summary>0xD7 custom house backup. Player serial, subcommand 0x02, terminator 0x0A.</summary>
    public static byte[] Send_CustomHouseBackup(uint playerSerial) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x02);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0xD7 custom house restore. Player serial, subcommand 0x03, terminator 0x0A.</summary>
    public static byte[] Send_CustomHouseRestore(uint playerSerial) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x03);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0xD7 custom house commit. Player serial, subcommand 0x04, terminator 0x0A.</summary>
    public static byte[] Send_CustomHouseCommit(uint playerSerial) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x04);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0xD7 custom house exit. Player serial, subcommand 0x0C, terminator 0x0A.</summary>
    public static byte[] Send_CustomHouseBuildingExit(uint playerSerial) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x0C);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0xD7 custom house floor. Player serial, subcommand 0x12, a zero dword, the floor, terminator 0x0A.</summary>
    public static byte[] Send_CustomHouseGoToFloor(uint playerSerial, byte floor) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x12);
        w.U32(0);
        w.U8(floor);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0xD7 custom house sync. Player serial, subcommand 0x0E, terminator 0x0A.</summary>
    public static byte[] Send_CustomHouseSync(uint playerSerial) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x0E);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0xD7 custom house clear. Player serial, subcommand 0x10, terminator 0x0A.</summary>
    public static byte[] Send_CustomHouseClear(uint playerSerial) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x10);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0xD7 custom house revert. Player serial, subcommand 0x1A, terminator 0x0A.</summary>
    public static byte[] Send_CustomHouseRevert(uint playerSerial) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x1A);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0xD7 custom house response. Player serial, subcommand 0x0A, terminator 0x0A.</summary>
    public static byte[] Send_CustomHouseResponse(uint playerSerial) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x0A);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0xD7 add house item. Subcommand 0x06, then 0x00, graphic, 0x00, x, 0x00, y, 0x0A.</summary>
    public static byte[] Send_CustomHouseAddItem(uint playerSerial, ushort graphic, int x, int y) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x06);
        w.U8(0x00);
        w.U32(graphic);
        w.U8(0x00);
        w.U32(x);
        w.U8(0x00);
        w.U32(y);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0xD7 delete house item. Subcommand 0x05, then 0x00, graphic, 0x00, x, 0x00, y, 0x00, z, 0x0A.</summary>
    public static byte[] Send_CustomHouseDeleteItem(uint playerSerial, ushort graphic, int x, int y, int z) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x05);
        w.U8(0x00);
        w.U32(graphic);
        w.U8(0x00);
        w.U32(x);
        w.U8(0x00);
        w.U32(y);
        w.U8(0x00);
        w.U32(z);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0xD7 add house roof. Subcommand 0x13, then 0x00, graphic, 0x00, x, 0x00, y, 0x00, z, 0x0A.</summary>
    public static byte[] Send_CustomHouseAddRoof(uint playerSerial, ushort graphic, int x, int y, int z) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x13);
        w.U8(0x00);
        w.U32(graphic);
        w.U8(0x00);
        w.U32(x);
        w.U8(0x00);
        w.U32(y);
        w.U8(0x00);
        w.U32(z);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0xD7 delete house roof. Subcommand 0x14, then 0x00, graphic, 0x00, x, 0x00, y, 0x00, z, 0x0A.</summary>
    public static byte[] Send_CustomHouseDeleteRoof(uint playerSerial, ushort graphic, int x, int y, int z) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x14);
        w.U8(0x00);
        w.U32(graphic);
        w.U8(0x00);
        w.U32(x);
        w.U8(0x00);
        w.U32(y);
        w.U8(0x00);
        w.U32(z);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0xD7 add house stair. Subcommand 0x0D, then 0x00, graphic, 0x00, x, 0x00, y, 0x0A.</summary>
    public static byte[] Send_CustomHouseAddStair(uint playerSerial, ushort graphic, int x, int y) {
        var w = new MoreW(0xD7);
        w.U32(playerSerial);
        w.U16(0x0D);
        w.U8(0x00);
        w.U32(graphic);
        w.U8(0x00);
        w.U32(x);
        w.U8(0x00);
        w.U32(y);
        w.U8(0x0A);
        return w.Finish();
    }

    /// <summary>0xC8 view range. Clamped to 5..24. Fixed length 2.</summary>
    public static byte[] Send_ClientViewRange(byte range) {
        if (range < 5) range = 5;
        else if (range > 24) range = 24;
        var w = new MoreW(0xC8);
        w.U8(range);
        return w.Finish();
    }

    /// <summary>0xFA open the store. Fixed length 1 from client 7.0.64.</summary>
    public static byte[] Send_OpenUOStore() {
        var w = new MoreW(0xFA);
        return w.Finish();
    }

    /// <summary>0xFB show public house content. Fixed length 2 from client 7.0.64.</summary>
    public static byte[] Send_ShowPublicHouseContent(bool show) {
        var w = new MoreW(0xFB);
        w.Bool(show);
        return w.Finish();
    }

    /// <summary>0x2C death screen. The action byte is 2, ghost. Fixed length 2.</summary>
    public static byte[] Send_DeathScreen() {
        var w = new MoreW(0x2C);
        w.U8(0x02);
        return w.Finish();
    }

    /// <summary>0xEC KR equip macro. A count byte, then each serial. The count is the low 8 bits of the span length.</summary>
    public static byte[] Send_EquipMacroKR(ReadOnlySpan<uint> serials) {
        var w = new MoreW(0xEC);
        w.U8((byte)serials.Length);
        for (var i = 0; i < serials.Length; ++i) w.U32(serials[i]);
        return w.Finish();
    }

    /// <summary>0xED KR unequip macro. A count byte, then each layer as a big-endian word of that byte.</summary>
    public static byte[] Send_UnequipMacroKR(ReadOnlySpan<byte> layers) {
        var w = new MoreW(0xED);
        w.U8((byte)layers.Length);
        for (var i = 0; i < layers.Length; ++i) w.U16(layers[i]);
        return w.Finish();
    }

    /// <summary>0x3F UOLive hash response. Block, six zero bytes, 0xFF, map index, then each checksum.</summary>
    public static byte[] Send_UOLive_HashResponse(uint block, byte mapIndex, Span<ushort> checksums) {
        var w = new MoreW(0x3F);
        w.U32(block);
        w.Zero(6);
        w.U8(0xFF);
        w.U8(mapIndex);
        for (var i = 0; i < checksums.Length; ++i) w.U16(checksums[i]);
        return w.Finish();
    }

    sealed class MoreW {
        byte[] _b;
        int _n;
        readonly int _len;

        public MoreW(byte id) {
            _len = _moreLengths.GetPacketLength(id);
            _b = new byte[_len < 0 ? 64 : _len];
            _b[0] = id;
            _n = _len < 0 ? 3 : 1;
        }

        public void U8(int v) {
            Need(1);
            _b[_n++] = (byte)v;
        }

        public void U16(int v) {
            Need(2);
            _b[_n++] = (byte)(v >> 8);
            _b[_n++] = (byte)v;
        }

        public void U32(long v) {
            var u = (uint)v;
            Need(4);
            _b[_n++] = (byte)(u >> 24);
            _b[_n++] = (byte)(u >> 16);
            _b[_n++] = (byte)(u >> 8);
            _b[_n++] = (byte)u;
        }

        public void Bool(bool v) => U8(v ? 1 : 0);

        public void Zero(int count) {
            if (count <= 0) return;
            Need(count);
            _n += count;
        }

        public void Raw(ReadOnlySpan<byte> s) {
            if (s.Length == 0) return;
            Need(s.Length);
            s.CopyTo(_b.AsSpan(_n));
            _n += s.Length;
        }

        public void AsciiZ(string str) {
            if (!string.IsNullOrEmpty(str)) WriteCp1252(str, -1);
            U8(0);
        }

        public void Ascii(string str, int length) {
            var start = _n;
            if (string.IsNullOrEmpty(str)) Zero(1);
            else WriteCp1252(str, length);
            if (length > -1 && _n > start) Zero(length - (_n - start));
        }

        public void UniZ(string str) {
            Uni(str, -1);
            U16(0);
        }

        public void Uni(string str, int length) {
            if (str == null) str = string.Empty;
            if (length > -1) {
                var byteCount = length * 2;
                if (byteCount == 0) return;
                var charLength = Math.Min(length, str.Length);
                var processed = 0;
                if (charLength > 0) {
                    var bytes = Encoding.BigEndianUnicode.GetBytes(str.Substring(0, charLength));
                    Raw(bytes);
                    processed = bytes.Length;
                }
                Zero(byteCount - processed);
                return;
            }
            if (str.Length == 0) return;
            Raw(Encoding.BigEndianUnicode.GetBytes(str));
        }

        public void Utf8(string str, int length) {
            if (str == null) str = string.Empty;
            var size = length > -1 ? length : Encoding.UTF8.GetByteCount(str);
            if (size == 0) return;
            var charLength = Math.Min(length > -1 ? length : str.Length, str.Length);
            var processed = 0;
            if (charLength > 0) {
                var tmp = new byte[size];
                processed = Encoding.UTF8.GetBytes(str, 0, charLength, tmp, 0);
                Raw(tmp.AsSpan(0, processed));
            }
            if (length > -1) Zero(length - processed);
        }

        public byte[] Finish() {
            if (_len < 0) {
                var n = (ushort)_n;
                _b[1] = (byte)(n >> 8);
                _b[2] = (byte)n;
            }
            else Zero(_len - _n);
            if (_b.Length == _n) return _b;
            var r = new byte[_n];
            Buffer.BlockCopy(_b, 0, r, 0, _n);
            return r;
        }

        void Need(int size) {
            if (_n + size <= _b.Length) return;
            var n = Math.Max(_n + size, _b.Length * 2);
            var g = new byte[n];
            Buffer.BlockCopy(_b, 0, g, 0, _n);
            _b = g;
        }

        void WriteCp1252(string s, int length) {
            var n = length > 0 ? Math.Min(length, s.Length) : s.Length;
            for (var i = 0; i < n; i += char.IsSurrogatePair(s, i) ? 2 : 1)
                U8(ToCp1252(char.ConvertToUtf32(s, i)));
        }

        static byte ToCp1252(int codepoint) {
            if (codepoint >= 0x80 && codepoint <= 0x9F) return (byte)'?';
            if (codepoint <= 0xFF) return (byte)codepoint;
            switch (codepoint) {
                case 0x20AC: return 128;
                case 0x201A: return 130;
                case 0x0192: return 131;
                case 0x201E: return 132;
                case 0x2026: return 133;
                case 0x2020: return 134;
                case 0x2021: return 135;
                case 0x02C6: return 136;
                case 0x2030: return 137;
                case 0x0160: return 138;
                case 0x2039: return 139;
                case 0x0152: return 140;
                case 0x017D: return 142;
                case 0x2018: return 145;
                case 0x2019: return 146;
                case 0x201C: return 147;
                case 0x201D: return 148;
                case 0x2022: return 149;
                case 0x2013: return 150;
                case 0x2014: return 151;
                case 0x02DC: return 152;
                case 0x2122: return 153;
                case 0x0161: return 154;
                case 0x203A: return 155;
                case 0x0153: return 156;
                case 0x017E: return 158;
                case 0x0178: return 159;
                default: return (byte)'?';
            }
        }
    }
}
