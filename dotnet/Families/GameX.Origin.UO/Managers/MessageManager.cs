// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>ClassicUO AffixType. None is 0xFF.</summary>
public enum AffixType : byte {
    Append = 0x00,
    Prepend = 0x01,
    System = 0x02,
    None = 0xFF
}

/// <summary>
/// ClassicUO MessageManager. Overhead text is stored here. Nothing is drawn and no gump is opened.
/// Profile reads are the in-memory defaults. Speech is not pushed into UOLoginNet.
/// </summary>
public sealed class MessageManager {
    // Profile path is not ported.
    const string Language = "ENU";
    const int MaxLabelWidth = 200;
    const int MaxSize = 5;
    const byte TextObject = 2;
    const byte PromptAscii = 1;
    const byte PromptUnicode = 2;
    const byte TargetNeutral = 0;
    const byte TargetHarmful = 1;
    const byte TargetBeneficial = 2;

    // Profile path is not ported.
    static readonly Profile CurrentProfile = new Profile();

    readonly World _world;
    readonly List<Line> _lines = new List<Line>();
    (byte Prompt, ulong Data) _prompt;

    public event EventHandler<(byte Prompt, ulong Data)> ServerPromptChanged;

    public MessageManager(World world) => _world = world;

    /// <summary>ClassicUO PromptData. Prompt 0 is none, 1 is ASCII, 2 is Unicode. Data is the server id.</summary>
    public (byte Prompt, ulong Data) PromptData {
        get => _prompt;
        set {
            _prompt = value;
            ServerPromptChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<MessageEventArgs> MessageReceived;

    public event EventHandler<MessageEventArgs> LocalizedMessageReceived;

    public void CancelServerPrompt() {
        SendServerPromptResponse(string.Empty);
    }

    public void SendServerPromptResponse(string text) {
        var cancel = text.Length < 1;
        // Profile path is not ported.
        if (UOLoginNet.HasPrompt)
            UOLoginNet.ReplyPrompt(text, cancel);
        else if (PromptData.Prompt == PromptAscii)
            UOLoginNet.Send(OutgoingPackets.Send_ASCIIPromptResponse(PromptData.Data, text, cancel));
        else if (PromptData.Prompt == PromptUnicode)
            UOLoginNet.Send(OutgoingPackets.Send_UnicodePromptResponse(PromptData.Data, text, Language, cancel));
        PromptData = default;
    }

    public void HandleMessage(
        object parent,
        string text,
        string name,
        ushort hue,
        MessageType type,
        byte font,
        byte textType,
        bool unicode = false,
        string lang = null
    ) {
        HandleMessage(parent, text, name, hue, type, font, textType, IgnoreManager.Instance.IgnoredCharsList, unicode, lang);
    }

    public void HandleMessage(
        object parent,
        string text,
        string name,
        ushort hue,
        MessageType type,
        byte font,
        byte textType,
        HashSet<string> ignored,
        bool unicode = false,
        string lang = null
    ) {
        if (string.IsNullOrEmpty(text)) return;

        // Profile path is not ported.
        if (CurrentProfile != null && CurrentProfile.OverrideAllFonts) {
            font = CurrentProfile.ChatFont;
            unicode = CurrentProfile.OverrideAllFontsIsUnicode;
        }

        switch (type) {
            case MessageType.Command:
            case MessageType.Encoded:
            case MessageType.System:
            case MessageType.Party:
                if (!ShowOverhead(type)) break;
                if (parent == null) parent = PartyParent(name);
                if (parent != null && Ignore(ignored, ParentName(parent), type)) break;
                if (parent != null) {
                    CreateMessage(text, hue, font, unicode, type, textType);
                    Bind(parent, name, 0, 0, false);
                }
                break;

            case MessageType.Guild:
                if (Spam(type)) return;
                break;

            case MessageType.Alliance:
                if (Spam(type)) return;
                break;

            case MessageType.Spell: {
                if (!string.IsNullOrEmpty(text) && (CurrentProfile.EnabledSpellFormat || CurrentProfile.EnabledSpellHue) && TrySpell(text, out var power, out var spellName, out var target)) {
                    if (CurrentProfile.EnabledSpellFormat && !string.IsNullOrWhiteSpace(CurrentProfile.SpellDisplayFormat))
                        text = CurrentProfile.SpellDisplayFormat.Replace("{power}", power).Replace("{spell}", spellName).Trim();
                    if (CurrentProfile.EnabledSpellHue) {
                        if (target == TargetBeneficial) hue = CurrentProfile.BeneficHue;
                        else if (target == TargetHarmful) hue = CurrentProfile.HarmfulHue;
                        else hue = CurrentProfile.NeutralHue;
                    }
                }
                goto case MessageType.Label;
            }

            default:
            case MessageType.Focus:
            case MessageType.Whisper:
            case MessageType.Yell:
            case MessageType.Regular:
            case MessageType.Label:
            case MessageType.Limit3Spell:
                if (parent == null) break;
                if (Ignore(ignored, ParentName(parent), type)) break;
                CreateMessage(text, hue, font, unicode, type, textType);
                // OnGround is World.Items. Contained text is stored here and no gump is opened.
                var textGump = parent is Item it && _world != null && !_world.Items.ContainsKey(it.Serial);
                var x = textGump ? Mouse.Position.X : 0;
                var y = textGump ? Mouse.Position.Y : 0;
                Bind(parent, name, x, y, textGump);
                break;
        }

        MessageReceived?.Invoke(parent, new MessageEventArgs(parent, text, name, hue, type, font, textType, unicode, lang));
    }

    public void OnLocalizedMessage(object entity, MessageEventArgs args) {
        LocalizedMessageReceived?.Invoke(entity, args);
    }

    public string CreateMessage(string msg, ushort hue, byte font, bool isunicode, MessageType type, byte textType) {
        // Profile path is not ported.
        if (CurrentProfile != null && CurrentProfile.OverrideAllFonts) {
            font = CurrentProfile.ChatFont;
            isunicode = CurrentProfile.OverrideAllFontsIsUnicode;
        }

        // Unicode and ASCII both use MeasureText. Left align, black border. A line wider than 200 is wrapped. GetWidthEx adds 4.
        var width = isunicode ? WidthOf(font, msg) : WidthOf(font, msg);
        int lines;
        if (width > MaxLabelWidth)
            width = isunicode ? WidthEx(font, msg, MaxLabelWidth, out lines) : WidthEx(font, msg, MaxLabelWidth, out lines);
        else {
            width = 0;
            lines = CountLines(msg);
        }

        var fixedColor = FixHue(hue);
        var createHue = fixedColor;
        if (!isunicode && textType == TextObject) createHue = 0x7FFF;
        createHue = fixedColor;

        _lines.Add(new Line {
            Text = msg,
            Hue = createHue,
            Font = font,
            Type = type,
            TextType = textType,
            Unicode = isunicode,
            Width = width,
            LineCount = lines,
            Time = CalculateTimeToLive(lines)
        });
        return msg;
    }

    /// <summary>The stored overhead lines, oldest first.</summary>
    public void CopyOverhead(List<(uint serial, string name, string text, ushort hue, byte font, MessageType type, bool unicode, int width, int lines, long time, int x, int y, bool textGump)> dest) {
        dest.Clear();
        foreach (var line in _lines)
            dest.Add((line.Serial, line.Name, line.Text, line.Hue, line.Font, line.Type, line.Unicode, line.Width, line.LineCount, line.Time, line.X, line.Y, line.IsTextGump));
    }

    static long CalculateTimeToLive(int linesCount) {
        // Profile path is not ported.
        var currentProfile = CurrentProfile;
        if (currentProfile == null) return 0;
        long timeToLive;
        if (currentProfile.ScaleSpeechDelay) {
            var delay = currentProfile.SpeechDelay;
            if (delay < 10) delay = 10;
            timeToLive = (long)(4000 * linesCount * delay / 100.0f);
        }
        else {
            long delay = (5497558140000 * currentProfile.SpeechDelay) >> 32 >> 5;
            timeToLive = (delay >> 31) + delay;
        }
        timeToLive += Time.Ticks;
        return timeToLive;
    }

    bool ShowOverhead(MessageType type) {
        if (type == MessageType.Command || type == MessageType.Encoded || type == MessageType.System || type == MessageType.Party)
            return CurrentProfile.OverheadPartyMessages;
        return true;
    }

    bool Spam(MessageType type) {
        if (type == MessageType.Guild) return CurrentProfile.IgnoreGuildMessages;
        if (type == MessageType.Alliance) return CurrentProfile.IgnoreAllianceMessages;
        return false;
    }

    static bool Ignore(HashSet<string> ignored, string name, MessageType type) {
        if (type == MessageType.Spell || ignored == null) return false;
        return ignored.Contains(name);
    }

    object PartyParent(string name) {
        if (_world == null || string.IsNullOrEmpty(name)) return null;
        var members = new List<(uint serial, string name)>();
        UOParty.CopyMembers(members);
        foreach (var member in members) {
            if (member.serial == 0) continue;
            var memberName = string.IsNullOrEmpty(member.name) ? UOLoginNet.NameOf(member.serial) : member.name;
            if (memberName != name) continue;
            if (_world.Mobiles.TryGetValue(member.serial, out var mob)) return mob;
        }
        return null;
    }

    void Bind(object parent, string name, int x, int y, bool textGump) {
        if (_lines.Count == 0) return;
        var i = _lines.Count - 1;
        var line = _lines[i];
        line.Serial = SerialOf(parent);
        line.Name = name;
        line.X = x;
        line.Y = y;
        line.IsTextGump = textGump;
        _lines[i] = line;
        var n = 0;
        var oldest = -1;
        for (var k = 0; k < _lines.Count; k++) {
            if (_lines[k].Serial != line.Serial) continue;
            if (oldest < 0) oldest = k;
            n++;
        }
        if (n > MaxSize && oldest >= 0) _lines.RemoveAt(oldest);
    }

    static uint SerialOf(object parent) {
        if (parent is Mobile m) return m.Serial;
        if (parent is Item it) return it.Serial;
        if (parent is uint serial) return serial;
        return 0;
    }

    static string ParentName(object parent) {
        var serial = SerialOf(parent);
        return serial == 0 ? "" : UOLoginNet.NameOf(serial);
    }

    static ushort FixHue(ushort hue) {
        var fixedColor = (ushort)(hue & 0x3FFF);
        if (fixedColor != 0) {
            if (fixedColor >= 0x0BB8) fixedColor = 1;
            fixedColor |= (ushort)(hue & 0xC000);
        }
        else fixedColor = (ushort)(hue & 0x8000);
        return fixedColor;
    }

    static int CountLines(string msg) {
        if (string.IsNullOrEmpty(msg)) return 1;
        var lines = 1;
        foreach (var c in msg)
            if (c == '\n') lines++;
        return lines;
    }

    static int WidthOf(byte font, string msg) {
        if (string.IsNullOrEmpty(msg)) return 0;
        var max = 0;
        var line = 0;
        foreach (var c in msg) {
            if (c == '\n') {
                if (line > max) max = line;
                line = 0;
                continue;
            }
            if (c == '\r') continue;
            line += Glyph(font, c);
        }
        return line > max ? line : max;
    }

    static int WidthEx(byte font, string msg, int maxWidth, out int lines) {
        lines = 0;
        var widest = 0;
        var parts = (msg ?? "").Replace("\r", "").Split('\n');
        foreach (var part in parts) {
            if (part.Length == 0) {
                lines++;
                continue;
            }
            var words = part.Split(' ');
            var width = 0;
            var count = 0;
            foreach (var word in words) {
                var ww = WidthOf(font, word);
                var gap = count == 0 ? 0 : Glyph(font, ' ');
                if (count > 0 && width + gap + ww > maxWidth) {
                    if (width > widest) widest = width;
                    lines++;
                    width = ww;
                    count = 1;
                }
                else {
                    width += gap + ww;
                    count++;
                }
            }
            if (width > widest) widest = width;
            lines++;
        }
        if (lines == 0) lines = 1;
        return widest + 4;
    }

    static int Glyph(byte font, char c) {
        var measured = GumpArt.Sink?.MeasureText(c.ToString(), font) ?? 6;
        return measured;
    }

    // SpellDefinition has no power words. EnabledSpellFormat and EnabledSpellHue default off.
    static bool TrySpell(string text, out string power, out string spell, out byte target) {
        _ = text;
        power = "";
        spell = "";
        target = TargetNeutral;
        return false;
    }

    // Profile path is not ported.
    sealed class Profile {
        public bool OverrideAllFonts = false;
        public byte ChatFont = 1;
        public bool OverrideAllFontsIsUnicode = true;
        public bool OverheadPartyMessages = false;
        public bool IgnoreGuildMessages = false;
        public bool IgnoreAllianceMessages = false;
        public ushort BeneficHue = 0x0059;
        public ushort HarmfulHue = 0x0020;
        public ushort NeutralHue = 0x03B1;
        public bool EnabledSpellHue = false;
        public bool EnabledSpellFormat = false;
        public string SpellDisplayFormat = "{power} [{spell}]";
        public bool ScaleSpeechDelay = true;
        public int SpeechDelay = 100;
    }

    struct Line {
        public uint Serial;
        public string Name;
        public string Text;
        public ushort Hue;
        public byte Font;
        public MessageType Type;
        public byte TextType;
        public bool Unicode;
        public int Width;
        public int LineCount;
        public long Time;
        public int X, Y;
        public bool IsTextGump;
    }
}
