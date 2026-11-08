// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>ClassicUO ChatMode. The values are the system-chat prefixes, not a grid-loot mode.</summary>
public enum ChatMode {
    Default,
    Whisper,
    Emote,
    Yell,
    Party,
    Guild,
    Alliance,
    ClientCommand,
    UOAMChat,
    Prompt,
    UOChat
}

/// <summary>
/// ClassicUO system chat. Lines with no object serial stay for 10 seconds, at most 30.
/// Enter calls UOLoginNet.Say for normal speech. Whisper, emote, yell, guild, and alliance call Send_UnicodeSpeechRequest.
/// Party uses the party packets. A prompt calls ReplyPrompt.
/// UIManager.OnKeyDown does not deliver Ctrl+Q, Ctrl+W, or Ctrl+Backspace.
/// </summary>
public sealed class SystemChatControl : Control {
    const int MaxMessage = 100;
    const int TextBoxLength = 500;
    const int ChatX = 3;
    const int ChatH = 15;
    const int LifeMs = 10000;
    const int MaxLines = 30;
    const string Lang = "ENU";
    const byte Font = 3;

    readonly WorldViewportGump _gump;
    readonly AlphaBlendControl _shade;
    readonly TextBox _box;
    readonly Label _modeLabel;
    readonly LinkedList<ChatLine> _lines = new();
    readonly List<UOSpeechState> _speech = [];
    readonly List<UOSpeechState> _seen = [];
    readonly List<(uint serial, string name)> _members = [];
    CommandManager _commands = null;
    bool _commandsReady = false;
    bool _active = false;
    ChatMode _mode = ChatMode.Default;
    bool _loot = false;

    // Profile path is not ported.
    public ushort SpeechHue { get; set; } = 0x02B2;
    public ushort WhisperHue { get; set; } = 0x0033;
    public ushort EmoteHue { get; set; } = 0x0021;
    public ushort YellHue { get; set; } = 0x0021;
    public ushort PartyHue { get; set; } = 0x0044;
    public ushort GuildHue { get; set; } = 0x0044;
    public ushort AllyHue { get; set; } = 0x0057;
    public ushort ChatHue { get; set; } = 0x0256;
    public bool ActivateChatAfterEnter { get; set; }
    public bool HideChatGradient { get; set; }

    public SystemChatControl(WorldViewportGump gump, int x, int y, int w, int h) {
        _gump = gump;
        X = x;
        Y = y;
        Width = w;
        Height = h;
        CanCloseWithRightClick = false;
        AcceptMouseInput = false;
        AcceptKeyboardInput = false;
        WantUpdateSize = false;
        _loot = UOParty.CanLoot;
        _shade = new AlphaBlendControl(0.5f) { AcceptMouseInput = true };
        Add(_shade);
        _box = new TextBox { Hue = SpeechHue, MaxLength = TextBoxLength, Width = w > ChatX ? w - ChatX : 1, Height = ChatH };
        Add(_box);
        _modeLabel = new Label("", true, SpeechHue, 1) { IsVisible = false };
        Add(_modeLabel);
        Mode = ChatMode.Default;
        _box.Hue = HueOf(Mode);
        IsActive = !ActivateChatAfterEnter;
        SetFocus();
    }

    public TextBox TextBoxControl => _box;

    public bool IsActive {
        get => _active;
        set {
            _active = value;
            _box.IsVisible = value;
            _box.AcceptKeyboardInput = value;
            if (value) {
                var span = _shade.Width - ChatX;
                _box.Width = span > 1 ? span : 1;
                _box.Text = "";
            }
            SetFocus();
        }
    }

    public ChatMode Mode {
        get => _mode;
        set {
            var prev = _mode;
            _mode = value;
            if (prev == ChatMode.Prompt && value != ChatMode.Prompt && UOLoginNet.HasPrompt)
                UOLoginNet.ReplyPrompt("", true);
            if (!_active) return;
            switch (value) {
                case ChatMode.Default:
                    _modeLabel.IsVisible = false;
                    _box.Text = "";
                    break;
                case ChatMode.Whisper: Prefix("[Whisper]: "); break;
                case ChatMode.Emote: Prefix("[Emote]: "); break;
                case ChatMode.Yell: Prefix("[Yell]: "); break;
                case ChatMode.Party: Prefix("[Party]: "); break;
                case ChatMode.Guild: Prefix("[Guild]: "); break;
                case ChatMode.Alliance: Prefix("[Alliance]: "); break;
                case ChatMode.ClientCommand: Prefix("[Command]: "); break;
                case ChatMode.UOAMChat:
                    _modeLabel.IsVisible = false;
                    Prefix("[UOAM]: ");
                    break;
                case ChatMode.UOChat:
                    _modeLabel.IsVisible = false;
                    Prefix("Chat");
                    break;
                case ChatMode.Prompt: Prefix("[Prompt]:"); break;
            }
            _box.Hue = HueOf(value);
            _modeLabel.Hue = _box.Hue;
            Resize();
        }
    }

    public bool IsComposing => IsActive && _box.Text.Length > 0;

    public void SetFocus() {
        _box.AcceptKeyboardInput = true;
        _box.SetKeyboardFocus();
        _box.AcceptKeyboardInput = _active;
        _shade.IsVisible = _active && !HideChatGradient;
    }

    public void ToggleChatVisibility() => IsActive = !IsActive;

    public void AddLine(string text, byte font, ushort hue, bool isunicode) {
        if (text == null) text = "";
        while (_lines.Count >= MaxLines) {
            var first = _lines.First;
            first.Value.Label.Dispose();
            _lines.RemoveFirst();
        }
        var line = new ChatLine(text, font, hue, isunicode);
        _lines.AddLast(line);
        Add(line.Label);
    }

    public void Resize() {
        var text = _box.Text ?? "";
        var lines = 1;
        for (var i = 0; i < text.Length; i++)
            if (text[i] == '\n') lines++;
        _modeLabel.X = ChatX;
        _modeLabel.Y = Height - ChatH - ChatX;
        var modeW = _modeLabel.IsVisible ? _modeLabel.Width : 0;
        _box.X = ChatX + modeW;
        _box.Y = Height - lines * ChatH - ChatX;
        var boxW = Width - ChatX - modeW;
        _box.Width = boxW > 1 ? boxW : 1;
        _box.Height = lines * ChatH + ChatX;
        _shade.X = 0;
        _shade.Y = _box.Y;
        _shade.Width = Width;
        _shade.Height = _box.Height;
        _shade.IsVisible = _active && !HideChatGradient;
        var yy = _box.Y;
        for (var node = _lines.Last; node != null; node = node.Previous) {
            yy -= ChatH;
            var row = node.Value.Label;
            row.X = 2;
            row.Y = yy;
            row.IsVisible = yy >= 0;
        }
    }

    public string ExtractSendableTextSubstring(ref string textBoxText) {
        if (textBoxText == null) textBoxText = "";
        if (textBoxText.Length <= MaxMessage) {
            var all = textBoxText;
            textBoxText = "";
            return all;
        }
        var last = textBoxText.LastIndexOf(' ', MaxMessage);
        if (last < 0) last = MaxMessage;
        var part = textBoxText.Substring(0, last);
        textBoxText = textBoxText.Substring(last).TrimStart();
        return part;
    }

    public override void Update() {
        var node = _lines.First;
        while (node != null) {
            var next = node.Next;
            if (node.Value.Dead) {
                node.Value.Label.Dispose();
                _lines.Remove(node);
            }
            node = next;
        }
        PullSpeech();
        if (UOLoginNet.HasPrompt) {
            if (Mode != ChatMode.Prompt) Mode = ChatMode.Prompt;
        }
        else if (Mode == ChatMode.Prompt) Mode = ChatMode.Default;
        WatchPrefix();
        _shade.Alpha = HideChatGradient ? 0f : 0.5f;
        Resize();
        base.Update();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        return base.Draw(x, y);
    }

    public override void OnKeyboardReturn(int textID, string text) {
        text ??= "";
        if ((!IsActive && ActivateChatAfterEnter) || text.Length == 0) {
            Reset();
            if (text.Length == 0) return;
        }
        if (TryMultipart(text, Mode, out var rest)) _box.Text = rest;
        else {
            Send(text, Mode);
            _box.Text = "";
        }
        if (_box.Text.Length == 0) Reset();
    }

    void Prefix(string label) {
        if (_modeLabel.IsVisible) return;
        _modeLabel.Text = label;
        _modeLabel.IsVisible = true;
        _box.Text = "";
    }

    void Reset() {
        _box.Text = "";
        Mode = ChatMode.Default;
    }

    // ChatManager is not constructed, so a comma does not enter UOChat.
    // Backspace on an empty box and Escape while Prompt never reach this control.
    void WatchPrefix() {
        if (!IsActive) return;
        var text = _box.Text ?? "";
        if (Mode == ChatMode.Default && text.Length > 0) {
            switch (text[0]) {
                case '/': EnterParty(text); break;
                case '\\': Mode = ChatMode.Guild; break;
                case '|': Mode = ChatMode.Alliance; break;
                case '-': Mode = ChatMode.ClientCommand; break;
                case ':' when text.Length > 1 && text[1] == ' ': Mode = ChatMode.Emote; break;
                case ';' when text.Length > 1 && text[1] == ' ': Mode = ChatMode.Whisper; break;
                case '!' when text.Length > 1 && text[1] == ' ': Mode = ChatMode.Yell; break;
            }
        }
        else if (Mode == ChatMode.ClientCommand && text.Length == 1 && text[0] == '-')
            Mode = ChatMode.UOAMChat;
    }

    void EnterParty(string text) {
        var pos = 1;
        while (pos < text.Length && text[pos] != ' ') pos++;
        if (pos < text.Length && int.TryParse(text.Substring(1, pos), out var index) && index > 0 && index < 11) {
            var name = MemberName(index - 1);
            Mode = ChatMode.Party;
            _box.Text = index + " ";
            _modeLabel.Text = name.Length > 0 ? "[Tell] [" + name + "]: " : "[Tell] []: ";
            _modeLabel.IsVisible = true;
        }
        else Mode = ChatMode.Party;
    }

    ushort HueOf(ChatMode mode) => mode switch {
        ChatMode.Default => SpeechHue,
        ChatMode.Whisper => WhisperHue,
        ChatMode.Emote => EmoteHue,
        ChatMode.Yell => YellHue,
        ChatMode.Party => PartyHue,
        ChatMode.Guild => GuildHue,
        ChatMode.Alliance => AllyHue,
        ChatMode.ClientCommand => 1161,
        ChatMode.UOAMChat => 83,
        ChatMode.UOChat => ChatHue,
        ChatMode.Prompt => 946,
        _ => 33,
    };

    void PullSpeech() {
        UOLoginNet.CopySpeech(_speech);
        for (var i = 0; i < _speech.Count; i++) {
            var line = _speech[i];
            var known = false;
            for (var j = 0; j < _seen.Count; j++)
                if (Same(line, _seen[j])) { known = true; break; }
            if (!known) Show(line);
        }
        _seen.Clear();
        for (var i = 0; i < _speech.Count; i++) _seen.Add(_speech[i]);
    }

    void Show(UOSpeechState line) {
        if (SerialHelper.IsValid(line.Serial)) return;
        var name = line.Name ?? "";
        if (name.Length > 0 && IgnoreManager.Instance.IgnoredCharsList.Contains(name) && line.Serial != UOLoginNet.PlayerSerial)
            return;
        var text = name.Length == 0 || name.Equals("system", StringComparison.OrdinalIgnoreCase)
            ? line.Text
            : name + ": " + line.Text;
        AddLine(text, Font, line.Hue, true);
    }

    static bool Same(UOSpeechState a, UOSpeechState b) =>
        a.Serial == b.Serial && a.Hue == b.Hue && a.Name == b.Name && a.Text == b.Text;

    bool TryMultipart(string text, ChatMode mode, out string remainder) {
        if (!TrySplit(text, mode, out var message, out remainder)) {
            remainder = message;
            return false;
        }
        Send(message, mode);
        if (mode == ChatMode.Party && text.Length >= 2 && int.TryParse(text.Substring(0, 2), out var index) && index > 0 && index < 11)
            remainder = index + " " + remainder;
        return true;
    }

    static bool TrySplit(string text, ChatMode mode, out string message, out string remainder) {
        if (text.Length <= MaxMessage || mode == ChatMode.ClientCommand || mode == ChatMode.Prompt) {
            message = text;
            remainder = "";
            return false;
        }
        var last = text.LastIndexOfAny(new[] { ' ', '\n' }, MaxMessage);
        if (last < 0) last = MaxMessage;
        message = text.Substring(0, last);
        remainder = text.Substring(last).TrimStart();
        return true;
    }

    void Send(string text, ChatMode mode) {
        switch (mode) {
            case ChatMode.Default:
                UOLoginNet.Say(text);
                break;
            case ChatMode.Whisper:
                Speech(text, MessageType.Whisper, WhisperHue);
                break;
            case ChatMode.Emote:
                Speech("*" + text + "*", MessageType.Emote, EmoteHue);
                break;
            case ChatMode.Yell:
                Speech(text, MessageType.Yell, YellHue);
                break;
            case ChatMode.Prompt:
                UOLoginNet.ReplyPrompt(text, false);
                break;
            case ChatMode.Party:
                Party(text);
                break;
            case ChatMode.Guild:
                Speech(text, MessageType.Guild, GuildHue);
                break;
            case ChatMode.Alliance:
                Speech(text, MessageType.Alliance, AllyHue);
                break;
            case ChatMode.ClientCommand:
                Command(text);
                break;
            case ChatMode.UOAMChat:
                // no UoAssist in this tree
                break;
            case ChatMode.UOChat:
                UOLoginNet.Send(OutgoingPackets.Send_ChatMessageCommand(Lang, text));
                break;
        }
    }

    static void Speech(string text, MessageType type, ushort hue) =>
        UOLoginNet.Send(OutgoingPackets.Send_UnicodeSpeechRequest(text, (byte)type, Font, hue, Lang));

    void Party(string text) {
        switch ((text ?? "").ToLower()) {
            case "add":
                if (UOParty.Leader == 0 || UOParty.Leader == UOLoginNet.PlayerSerial)
                    UOLoginNet.Send(OutgoingPackets.Send_PartyInviteRequest());
                else AddLine("You are not party leader.", Font, 0xFFFF, true);
                break;
            case "loot":
                if (UOParty.Leader != 0) {
                    _loot = !_loot;
                    UOLoginNet.Send(OutgoingPackets.Send_PartyChangeLootTypeRequest(_loot));
                }
                else AddLine("You are not in a party.", Font, 0xFFFF, true);
                break;
            case "quit":
                if (UOParty.Leader == 0) AddLine("You are not in a party.", Font, 0xFFFF, true);
                else UOLoginNet.Send(OutgoingPackets.Send_PartyRemoveRequest(UOLoginNet.PlayerSerial));
                break;
            case "accept":
                ReplyInvite(true);
                break;
            case "decline":
                ReplyInvite(false);
                break;
            case "rem":
                if (UOParty.Leader != 0 && UOParty.Leader == UOLoginNet.PlayerSerial) {
                    // remove target is not in this tree
                }
                else AddLine("You are not party leader.", Font, 0xFFFF, true);
                break;
            default:
                if (UOParty.Leader != 0)
                    UOLoginNet.Send(OutgoingPackets.Send_PartyMessage(text, PartyTarget(text)));
                else AddLine("Note to self: " + text, Font, 0xFFFF, true);
                break;
        }
    }

    void ReplyInvite(bool accept) {
        var invite = UIManager.GetGump<PartyInviteGump>();
        if (UOParty.Leader == 0 && invite != null && !invite.IsDisposed && invite.ServerSerial != 0) {
            var packet = accept
                ? OutgoingPackets.Send_PartyAccept(invite.ServerSerial)
                : OutgoingPackets.Send_PartyDecline(invite.ServerSerial);
            UOLoginNet.Send(packet);
        }
        else AddLine("No one has invited you to be in a party.", Font, 0xFFFF, true);
    }

    uint PartyTarget(string text) {
        if (string.IsNullOrEmpty(text)) return 0;
        var pos = 0;
        while (pos < text.Length && text[pos] != ' ') pos++;
        if (pos >= text.Length || pos == 0) return 0;
        if (!int.TryParse(text.Substring(0, pos), out var index) || index < 1 || index > 10) return 0;
        UOParty.CopyMembers(_members);
        var slot = index - 1;
        return slot < _members.Count ? _members[slot].serial : 0;
    }

    string MemberName(int index) {
        UOParty.CopyMembers(_members);
        if (index < 0 || index >= _members.Count) return "";
        var serial = _members[index].serial;
        if (serial == 0) return "";
        var name = _members[index].name;
        if (string.IsNullOrEmpty(name)) name = UOLoginNet.NameOf(serial);
        return name ?? "";
    }

    void Command(string text) {
        var world = _gump != null ? _gump.World : null;
        _commands ??= new CommandManager(world);
        if (!_commandsReady) {
            _commands.Initialize();
            _commandsReady = true;
        }
        var parts = (text ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 0) _commands.Execute(parts[0], parts);
    }

    sealed class ChatLine {
        public readonly Label Label;
        public readonly int Until;

        public ChatLine(string text, byte font, ushort hue, bool unicode) {
            Label = new Label(text, unicode, hue, font) { AcceptMouseInput = false };
            Until = Environment.TickCount + LifeMs;
        }

        public bool Dead => Label.IsDisposed || unchecked(Environment.TickCount - Until) >= 0;
    }
}
