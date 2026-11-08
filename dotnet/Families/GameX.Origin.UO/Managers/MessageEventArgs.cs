// SPDX-License-Identifier: BSD-2-Clause

using System;

namespace GameX.Origin.UO;

/// <summary>ClassicUO MessageType. Not the Origin MessageTypes packet enum.</summary>
[Flags]
public enum MessageType : byte {
    Regular = 0,
    System = 1,
    Emote = 2,
    Limit3Spell = 3, // Sphere style shards use this to limit to 3 of these message types showing overhead.
    Label = 6,
    Focus = 7,
    Whisper = 8,
    Yell = 9,
    Spell = 10,
    Guild = 13,
    Alliance = 14,
    Command = 15,
    GmChat = 16,
    Encoded = 0xC0,
    Party = 0xFF // This is a CUO assigned type, value is unimportant
}

/// <summary>ClassicUO MessageEventArgs. Parent is object because Entity is not ported. AffixType.None is 0xFF.</summary>
public class MessageEventArgs : EventArgs {
    public MessageEventArgs(
        object parent,
        string text,
        string name,
        ushort hue,
        MessageType type,
        byte font,
        byte text_type,
        bool unicode = false,
        string lang = null
    ) {
        Parent = parent;
        Text = text;
        Name = name;
        Hue = hue;
        Type = type;
        Font = font;
        Language = lang;
        AffixType = 0xFF;
        IsUnicode = unicode;
        TextType = text_type;
    }

    public object Parent { get; }

    public string Text { get; }

    public string Name { get; }

    public ushort Hue { get; }

    public MessageType Type { get; }

    public byte Font { get; }

    public string Language { get; }

    public uint Cliloc { get; }

    // ClassicUO AffixType.None. The enum is not in this file.
    public byte AffixType { get; }

    public string Affix { get; }

    public bool IsUnicode { get; }

    // CLIENT 0, SYSTEM 1, OBJECT 2, GUILD_ALLY 3.
    public byte TextType { get; }
}
