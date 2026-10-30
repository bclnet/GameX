// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GameX.Origin.Netcode.UO;

/// <summary>ClassicUO outgoing packets through the unicode prompt. Each method returns the bytes that would be sent.</summary>
public static partial class OutgoingPackets {
    /// <summary>0x03 speech ack. Forty bytes.</summary>
    public static byte[] Send_ACKTalk() {
        const byte ID = 0x03;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        w.Raw(new byte[] {
            0x20, 0x00, 0x34, 0x00, 0x03, 0xdb, 0x13, 0x14, 0x3f, 0x45, 0x2c, 0x58, 0x0f, 0x5d, 0x44, 0x2e,
            0x50, 0x11, 0xdf, 0x75, 0x5c, 0xe0, 0x3e, 0x71, 0x4f, 0x31, 0x34, 0x05, 0x4e, 0x18, 0x1e, 0x72,
            0x0f, 0x59, 0xad, 0xf5, 0x00,
        });
        return w.Finish(length);
    }

    /// <summary>0x73 ping.</summary>
    public static byte[] Send_Ping(byte idx) {
        const byte ID = 0x73;
        const int length = 2;
        var w = Writer.Begin(ID, length);
        w.U8(idx);
        return w.Finish(length);
    }

    /// <summary>0x06 double-click.</summary>
    public static byte[] Send_DoubleClick(uint serial) {
        const byte ID = 0x06;
        const int length = 5;
        var w = Writer.Begin(ID, length);
        w.U32(serial);
        return w.Finish(length);
    }

    /// <summary>0xEF seed. major, minor, build, and extra are written as big-endian uints.</summary>
    public static byte[] Send_Seed(uint v, byte major, byte minor, byte build, byte extra) {
        const byte ID = 0xEF;
        const int length = 0x15;
        var w = Writer.Begin(ID, length);
        w.U32(v);
        w.U32(major);
        w.U32(minor);
        w.U32(build);
        w.U32(extra);
        return w.Finish(length);
    }

    /// <summary>Four-byte big-endian seed. No packet id.</summary>
    public static byte[] Send_Seed_Old(uint v) {
        var w = new Writer();
        w.U32(v);
        return w.Finish(4);
    }

    /// <summary>0x80 first login. Names are 30 bytes, then 0xFF.</summary>
    public static byte[] Send_FirstLogin(string user, string psw) {
        const byte ID = 0x80;
        const int length = 0x3E;
        var w = Writer.Begin(ID, length);
        w.Ascii(user, 30);
        w.Ascii(psw, 30);
        w.U8(0xFF);
        return w.Finish(length);
    }

    /// <summary>0xA0 select server.</summary>
    public static byte[] Send_SelectServer(byte index) {
        const byte ID = 0xA0;
        const int length = 3;
        var w = Writer.Begin(ID, length);
        w.U8(0x00);
        w.U8(index);
        return w.Finish(length);
    }

    /// <summary>0x91 second login. seed, then 30-byte name and password.</summary>
    public static byte[] Send_SecondLogin(string user, string psw, uint seed) {
        const byte ID = 0x91;
        const int length = 0x41;
        var w = Writer.Begin(ID, length);
        w.U32(seed);
        w.Ascii(user, 30);
        w.Ascii(psw, 30);
        return w.Finish(length);
    }

    /// <summary>0x00, or 0xF8 from client 7.0.16. Pass 0 for an empty hair, beard, shirt, or pants slot. protocol is the client flag dword. skills are collection order; valueFixed is Skill.ValueFixed.</summary>
    public static byte[] Send_CreateCharacter(ClientVersion version, string name, uint protocol, byte profession, bool female, byte race, byte strength, byte dexterity, byte intelligence, (byte index, ushort valueFixed)[] skills, ushort hue, ushort hairGraphic, ushort hairHue, ushort beardGraphic, ushort beardHue, int cityIndex, uint slot, uint clientIP, ushort shirtHue, ushort pantsHue) {
        const byte ID = 0x00;
        var id = ID;
        var skillcount = 3;
        if (version >= ClientVersion.CV_70160) {
            id = 0xF8;
            skillcount++;
        }
        var length = version >= ClientVersion.CV_70180 ? 0x6A : 0x68;
        var w = Writer.Begin(id, length);
        w.U32(0xEDEDEDED);
        w.U32(0xFFFFFFFF);
        w.U8(0x00);
        w.Ascii(name, 30);
        w.Zero(2);
        w.U32(protocol);
        w.U32(0x01);
        w.U32(0x00);
        w.U8(profession);
        w.Zero(15);
        byte val;
        if (version < ClientVersion.CV_4011D) val = (byte)(female ? 0x01 : 0x00);
        else {
            val = race;
            if (version < ClientVersion.CV_7000) val--;
            val = (byte)(val * 2 + (female ? 0x01 : 0x00));
        }
        w.U8(val);
        w.U8(strength);
        w.U8(dexterity);
        w.U8(intelligence);
        foreach (var skill in skills.OrderByDescending(s => s.valueFixed / 10f).Take(skillcount)) {
            w.U8(skill.index);
            w.U8((byte)skill.valueFixed);
        }
        w.U16(hue);
        w.U16(hairGraphic);
        w.U16(hairHue);
        w.U16(beardGraphic);
        w.U16(beardHue);
        w.U16((ushort)cityIndex);
        w.Zero(2);
        w.U16((ushort)slot);
        w.U32(clientIP);
        w.U16(shirtHue);
        w.U16(pantsHue);
        return w.Finish(length);
    }

    /// <summary>0x83 delete character. index is a big-endian uint.</summary>
    public static byte[] Send_DeleteCharacter(byte index, uint ipclient) {
        const byte ID = 0x83;
        const int length = 0x27;
        var w = Writer.Begin(ID, length);
        w.Zero(30);
        w.U32(index);
        w.U32(ipclient);
        return w.Finish(length);
    }

    /// <summary>0x5D select character. protocol is the client flag dword.</summary>
    public static byte[] Send_SelectCharacter(string name, uint protocol, uint index, uint ipclient) {
        const byte ID = 0x5D;
        const int length = 0x49;
        var w = Writer.Begin(ID, length);
        w.U32(0xEDEDEDED);
        w.Ascii(name, 30);
        w.Zero(2);
        w.U32(protocol);
        w.Zero(24);
        w.U32(index);
        w.U32(ipclient);
        return w.Finish(length);
    }

    /// <summary>0x07 pick up.</summary>
    public static byte[] Send_PickUpRequest(uint serial, ushort count) {
        const byte ID = 0x07;
        const int length = 7;
        var w = Writer.Begin(ID, length);
        w.U32(serial);
        w.U16(count);
        return w.Finish(length);
    }

    /// <summary>0x08 drop, 14 bytes. No grid slot.</summary>
    public static byte[] Send_DropRequest_Old(uint serial, ushort x, ushort y, sbyte z, uint container) {
        const byte ID = 0x08;
        const int length = 0x0E;
        var w = Writer.Begin(ID, length);
        w.U32(serial);
        w.U16(x);
        w.U16(y);
        w.I8(z);
        w.U32(container);
        return w.Finish(length);
    }

    /// <summary>0x08 drop, 15 bytes. slot follows z.</summary>
    public static byte[] Send_DropRequest(uint serial, ushort x, ushort y, sbyte z, byte slot, uint container) {
        const byte ID = 0x08;
        const int length = 0x0E;
        var w = Writer.Begin(ID, length);
        w.U32(serial);
        w.U16(x);
        w.U16(y);
        w.I8(z);
        w.U8(slot);
        w.U32(container);
        return w.Finish(length);
    }

    /// <summary>0x13 equip. layer is the layer byte.</summary>
    public static byte[] Send_EquipRequest(uint serial, byte layer, uint container) {
        const byte ID = 0x13;
        const int length = 0x0A;
        var w = Writer.Begin(ID, length);
        w.U32(serial);
        w.U8(layer);
        w.U32(container);
        return w.Finish(length);
    }

    /// <summary>0x72 war mode. Five bytes.</summary>
    public static byte[] Send_ChangeWarMode(bool state) {
        const byte ID = 0x72;
        const int length = 5;
        var w = Writer.Begin(ID, length);
        w.Bool(state);
        w.U8(0x32);
        w.U8(0x00);
        return w.Finish(length);
    }

    /// <summary>0x9B help request. 258 bytes.</summary>
    public static byte[] Send_HelpRequest() {
        const byte ID = 0x9B;
        const int length = 0x102;
        var w = Writer.Begin(ID, length);
        w.Zero(257);
        return w.Finish(length);
    }

    /// <summary>0x34 status request, type 0x04.</summary>
    public static byte[] Send_StatusRequest(uint serial) {
        const byte ID = 0x34;
        const int length = 0x0A;
        var w = Writer.Begin(ID, length);
        w.U32(0xEDEDEDED);
        w.U8(0x04);
        w.U32(serial);
        return w.Finish(length);
    }

    /// <summary>0x34 skills request, type 0x05.</summary>
    public static byte[] Send_SkillsRequest(uint serial) {
        const byte ID = 0x34;
        const int length = 0x0A;
        var w = Writer.Begin(ID, length);
        w.U32(0xEDEDEDED);
        w.U8(0x05);
        w.U32(serial);
        return w.Finish(length);
    }

    /// <summary>0x3A skill lock.</summary>
    public static byte[] Send_SkillsStatusRequest(ushort skillIndex, byte lockState) {
        const byte ID = 0x3A;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        w.U16(skillIndex);
        w.U8(lockState);
        return w.Finish(length);
    }

    /// <summary>0x09 single click.</summary>
    public static byte[] Send_ClickRequest(uint serial) {
        const byte ID = 0x09;
        const int length = 5;
        var w = Writer.Begin(ID, length);
        w.U32(serial);
        return w.Finish(length);
    }

    /// <summary>0x05 attack.</summary>
    public static byte[] Send_AttackRequest(uint serial) {
        const byte ID = 0x05;
        const int length = 5;
        var w = Writer.Begin(ID, length);
        w.U32(serial);
        return w.Finish(length);
    }

    /// <summary>0xBD client version, null-terminated ASCII.</summary>
    public static byte[] Send_ClientVersion(string version) {
        const byte ID = 0xBD;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        w.AsciiZ(version);
        return w.Finish(length);
    }

    /// <summary>0x03 ASCII speech. keywords sets the 0xC0 bit; the text stays ASCII. font is a ushort on the packet.</summary>
    public static byte[] Send_ASCIISpeechRequest(string text, byte type, byte font, ushort hue, short[] keywords = null) {
        const byte ID = 0x03;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        var kind = type;
        if (keywords != null && keywords.Length != 0) kind |= 0xC0;
        w.U8(kind);
        w.U16(hue);
        w.U16(font);
        w.AsciiZ(text);
        return w.Finish(length);
    }

    /// <summary>0xAD unicode speech. keywords, when set, are speech.mul ids and replace the UTF-16 text with the encoded block.</summary>
    public static byte[] Send_UnicodeSpeechRequest(string text, byte type, byte font, ushort hue, string lang, short[] keywords = null) {
        const byte ID = 0xAD;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        var encoded = keywords != null && keywords.Length != 0;
        var kind = type;
        if (encoded) kind |= 0xC0;
        w.U8(kind);
        w.U16(hue);
        w.U16(font);
        w.Ascii(lang, 4);
        if (encoded) WriteEncoded(w, keywords, text);
        else w.UnicodeBE(text);
        return w.Finish(length);
    }

    /// <summary>0xBF sub 0x001C from client 6.0.14.2. Older clients send 0x12 command 0x56.</summary>
    public static byte[] Send_CastSpell(ClientVersion version, int idx) {
        const int length = -1;
        var id = version >= ClientVersion.CV_60142 ? (byte)0xBF : (byte)0x12;
        var w = Writer.Begin(id, length);
        if (version >= ClientVersion.CV_60142) {
            w.U16(0x1C);
            w.U16(0x02);
            w.U16((ushort)idx);
        }
        else {
            w.U8(0x56);
            w.AsciiZ(idx.ToString(CultureInfo.InvariantCulture));
        }
        return w.Finish(length);
    }

    /// <summary>0x12 command 0x27. The body is the index, a space, the serial, and a null.</summary>
    public static byte[] Send_CastSpellFromBook(int idx, uint serial) {
        const byte ID = 0x12;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        w.U8(0x27);
        w.AsciiZ(string.Format(CultureInfo.InvariantCulture, "{0} {1}", idx, serial));
        return w.Finish(length);
    }

    /// <summary>0x12 command 0x24. The body is the index, a space, 0, and a null.</summary>
    public static byte[] Send_UseSkill(int idx) {
        const byte ID = 0x12;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        w.U8(0x24);
        w.AsciiZ(string.Format(CultureInfo.InvariantCulture, "{0} 0", idx));
        return w.Finish(length);
    }

    /// <summary>0x12 command 0x58, then a null.</summary>
    public static byte[] Send_OpenDoor() {
        const byte ID = 0x12;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        w.U8(0x58);
        w.U8(0x00);
        return w.Finish(length);
    }

    /// <summary>0x12 command 0x43, then the book type.</summary>
    public static byte[] Send_OpenSpellBook(byte type) {
        const byte ID = 0x12;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        w.U8(0x43);
        w.U8(type);
        return w.Finish(length);
    }

    /// <summary>0x12 command 0xC7, then the action and a null.</summary>
    public static byte[] Send_EmoteAction(string action) {
        const byte ID = 0x12;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        w.U8(0xC7);
        w.AsciiZ(action);
        return w.Finish(length);
    }

    /// <summary>0xB1 gump reply. A text entry is capped at 239 UTF-16BE chars.</summary>
    public static byte[] Send_GumpResponse(uint local, uint server, int button, uint[] switches, Tuple<ushort, string>[] entries) {
        const byte ID = 0xB1;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        w.U32(local);
        w.U32(server);
        w.U32((uint)button);
        w.U32((uint)switches.Length);
        for (var i = 0; i < switches.Length; i++) w.U32(switches[i]);
        w.U32((uint)entries.Length);
        for (var i = 0; i < entries.Length; i++) {
            var len = Math.Min(239, entries[i].Item2.Length);
            w.U16(entries[i].Item1);
            w.U16((ushort)len);
            w.Unicode(entries[i].Item2, len, true);
        }
        return w.Finish(length);
    }

    /// <summary>0xB1 virtue gump. The gump id is 0x000001CD.</summary>
    public static byte[] Send_VirtueGumpResponse(uint serial, uint code) {
        const byte ID = 0xB1;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        w.U32(serial);
        w.U32(0x000001CD);
        w.U32(code);
        return w.Finish(length);
    }

    /// <summary>0x7D menu. code 0 omits the item and pads to 13.</summary>
    public static byte[] Send_MenuResponse(uint serial, ushort graphic, int code, ushort itemGraphic, ushort itemHue) {
        const byte ID = 0x7D;
        const int length = 0x0D;
        var w = Writer.Begin(ID, length);
        w.U32(serial);
        w.U16(graphic);
        if (code != 0) {
            w.U16((ushort)code);
            w.U16(itemGraphic);
            w.U16(itemHue);
        }
        return w.Finish(length);
    }

    /// <summary>0x7D gray menu. Nine bytes, padded to 13.</summary>
    public static byte[] Send_GrayMenuResponse(uint serial, ushort graphic, ushort code) {
        const byte ID = 0x7D;
        const int length = 0x0D;
        var w = Writer.Begin(ID, length);
        w.U32(serial);
        w.U16(graphic);
        w.U16(code);
        return w.Finish(length);
    }

    /// <summary>0x6F trade. Null when code is not 1 or 2.</summary>
    public static byte[] Send_TradeResponse(uint serial, int code, bool state) {
        const byte ID = 0x6F;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        if (code == 1) {
            w.U8(0x01);
            w.U32(serial);
        }
        else if (code == 2) {
            w.U8(0x02);
            w.U32(serial);
            w.U32((uint)(state ? 1 : 0));
        }
        else return null;
        return w.Finish(length);
    }

    /// <summary>0x6F trade gold, subcommand 0x03.</summary>
    public static byte[] Send_TradeUpdateGold(uint serial, uint gold, uint platinum) {
        const byte ID = 0x6F;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        w.U8(0x03);
        w.U32(serial);
        w.U32(gold);
        w.U32(platinum);
        return w.Finish(length);
    }

    /// <summary>0xD1 logout. Two bytes.</summary>
    public static byte[] Send_LogoutNotification() {
        const byte ID = 0xD1;
        const int length = 2;
        var w = Writer.Begin(ID, length);
        w.U8(0x00);
        return w.Finish(length);
    }

    /// <summary>0xAC text entry. The length counts the null.</summary>
    public static byte[] Send_TextEntryDialogResponse(uint serial, byte parentID, byte button, string text, bool code) {
        const byte ID = 0xAC;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        w.U32(serial);
        w.U8(parentID);
        w.U8(button);
        w.Bool(code);
        w.U16((ushort)(text.Length + 1));
        w.Ascii(text, text.Length + 1);
        return w.Finish(length);
    }

    /// <summary>0x75 rename. The name is 30 bytes.</summary>
    public static byte[] Send_RenameRequest(uint serial, string name) {
        const byte ID = 0x75;
        const int length = 0x23;
        var w = Writer.Begin(ID, length);
        w.U32(serial);
        w.Ascii(name, 30);
        return w.Finish(length);
    }

    /// <summary>0x98 name request. Seven bytes.</summary>
    public static byte[] Send_NameRequest(uint serial) {
        const byte ID = 0x98;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        w.U32(serial);
        return w.Finish(length);
    }

    /// <summary>0xA7 tip request.</summary>
    public static byte[] Send_TipRequest(ushort id, byte flag) {
        const byte ID = 0xA7;
        const int length = 4;
        var w = Writer.Begin(ID, length);
        w.U16(id);
        w.U8(flag);
        return w.Finish(length);
    }

    /// <summary>0x6C target object. z is sign-extended to a ushort.</summary>
    public static byte[] Send_TargetObject(uint entity, ushort graphic, ushort x, ushort y, sbyte z, uint cursorID, byte cursorType) {
        const byte ID = 0x6C;
        const int length = 0x13;
        var w = Writer.Begin(ID, length);
        w.U8(0x00);
        w.U32(cursorID);
        w.U8(cursorType);
        w.U32(entity);
        w.U16(x);
        w.U16(y);
        w.U16((ushort)z);
        w.U16(graphic);
        return w.Finish(length);
    }

    /// <summary>0x6C target ground. The serial is 0. z is sign-extended to a ushort.</summary>
    public static byte[] Send_TargetXYZ(ushort graphic, ushort x, ushort y, sbyte z, uint cursorID, byte cursorType) {
        const byte ID = 0x6C;
        const int length = 0x13;
        var w = Writer.Begin(ID, length);
        w.U8(0x01);
        w.U32(cursorID);
        w.U8(cursorType);
        w.U32(0x00);
        w.U16(x);
        w.U16(y);
        w.U16((ushort)z);
        w.U16(graphic);
        return w.Finish(length);
    }

    /// <summary>0x6C target cancel. type is the cursor target byte.</summary>
    public static byte[] Send_TargetCancel(byte type, uint cursorID, byte cursorType) {
        const byte ID = 0x6C;
        const int length = 0x13;
        var w = Writer.Begin(ID, length);
        w.U8(type);
        w.U32(cursorID);
        w.U8(cursorType);
        w.U32(0x00);
        w.U32(0xFFFFFFFF);
        w.U32(0x00000000);
        return w.Finish(length);
    }

    /// <summary>0x9A ASCII prompt. data is PromptData.Data. cancel writes 0.</summary>
    public static byte[] Send_ASCIIPromptResponse(ulong data, string text, bool cancel) {
        const byte ID = 0x9A;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        w.U64(data);
        w.U32((uint)(cancel ? 0 : 1));
        w.AsciiZ(text);
        return w.Finish(length);
    }

    /// <summary>0xC2 unicode prompt. Language is 3 bytes plus a null. Text is UTF-16LE with no extra null.</summary>
    public static byte[] Send_UnicodePromptResponse(ulong data, string text, string lang, bool cancel) {
        const byte ID = 0xC2;
        const int length = -1;
        var w = Writer.Begin(ID, length);
        w.U64(data);
        w.U32((uint)(cancel ? 0 : 1));
        w.Ascii(lang, 3);
        w.U8(0x00);
        w.Unicode(text, text.Length, false);
        return w.Finish(length);
    }

    static void WriteEncoded(Writer w, short[] keywords, string text) {
        var ids = (short[])keywords.Clone();
        Array.Sort(ids);
        var code = new byte[ids.Length * 2 + 2];
        var n = 0;
        var len = ids.Length;
        code[n++] = (byte)(len >> 4);
        var num3 = len & 15;
        var flag = false;
        var index = 0;
        while (index < len) {
            var keywordID = (int)ids[index];
            if (flag) {
                code[n++] = (byte)(keywordID >> 4);
                num3 = keywordID & 15;
            }
            else {
                code[n++] = (byte)((num3 << 4) | ((keywordID >> 8) & 15));
                code[n++] = (byte)keywordID;
            }
            index++;
            flag = !flag;
        }
        if (!flag) code[n++] = (byte)(num3 << 4);
        w.Raw(code, n);
        w.Raw(Encoding.UTF8.GetBytes(text));
        w.U8(0x00);
    }

    sealed class Writer {
        readonly System.Collections.Generic.List<byte> _b = new System.Collections.Generic.List<byte>();

        public static Writer Begin(byte id, int length) {
            var w = new Writer();
            w.U8(id);
            if (length < 0) w.Zero(2);
            return w;
        }

        public void U8(byte v) => _b.Add(v);

        public void I8(sbyte v) => _b.Add((byte)v);

        public void Bool(bool v) => _b.Add(v ? (byte)0x01 : (byte)0x00);

        public void U16(ushort v) {
            _b.Add((byte)(v >> 8));
            _b.Add((byte)v);
        }

        public void U32(uint v) {
            _b.Add((byte)(v >> 24));
            _b.Add((byte)(v >> 16));
            _b.Add((byte)(v >> 8));
            _b.Add((byte)v);
        }

        public void U64(ulong v) {
            U32((uint)(v >> 32));
            U32((uint)v);
        }

        public void Zero(int count) {
            if (count <= 0) return;
            for (var i = 0; i < count; i++) _b.Add(0);
        }

        public void Raw(byte[] data) => _b.AddRange(data);

        public void Raw(byte[] data, int count) {
            for (var i = 0; i < count; i++) _b.Add(data[i]);
        }

        public void AsciiZ(string str) {
            if (!string.IsNullOrEmpty(str)) Cp1252(str, -1);
            U8(0x00);
        }

        public void Ascii(string str, int length) {
            var start = _b.Count;
            if (string.IsNullOrEmpty(str)) Zero(1);
            else Cp1252(str, length);
            if (length > -1 && _b.Count > start) Zero(length - (_b.Count - start));
        }

        public void UnicodeBE(string str) {
            Unicode(str, -1, true);
            U16(0);
        }

        public void Unicode(string str, int length, bool big) {
            if (str == null) str = string.Empty;
            var enc = big ? Encoding.BigEndianUnicode : Encoding.Unicode;
            var byteCount = length > -1 ? length * 2 : enc.GetByteCount(str);
            if (byteCount == 0) return;
            var charLength = Math.Min(length > -1 ? length : str.Length, str.Length);
            var processed = 0;
            if (charLength > 0) {
                var raw = new byte[byteCount];
                processed = enc.GetBytes(str, 0, charLength, raw, 0);
                for (var i = 0; i < processed; i++) _b.Add(raw[i]);
            }
            if (length > -1) Zero(length * 2 - processed);
        }

        public byte[] Finish(int length) {
            if (length < 0) {
                var n = (ushort)_b.Count;
                _b[1] = (byte)(n >> 8);
                _b[2] = (byte)n;
            }
            else if (_b.Count < length) Zero(length - _b.Count);
            return _b.ToArray();
        }

        void Cp1252(string s, int length) {
            var n = length > 0 ? Math.Min(length, s.Length) : s.Length;
            for (var i = 0; i < n; i += char.IsSurrogatePair(s, i) ? 2 : 1)
                U8(Cp1252Byte(char.ConvertToUtf32(s, i)));
        }

        static byte Cp1252Byte(int codepoint) {
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
