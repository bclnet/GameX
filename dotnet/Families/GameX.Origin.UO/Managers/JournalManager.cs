// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.IO;

namespace GameX.Origin.UO;

/// <summary>ClassicUO JournalManager. Entries stay in memory, capped at 100. The log file is not opened.</summary>
public sealed class JournalManager {
    const int MaxJournalHistoryCount = 100;

    StreamWriter _fileWriter;
    bool _writerHasException = false;

    public static Deque<JournalEntry> Entries { get; } = new Deque<JournalEntry>(MaxJournalHistoryCount);

    public event EventHandler<JournalEntry> EntryAdded;

    public void Add(string text, ushort hue, string name, uint? serial, byte type, bool isunicode = true, MessageType messageType = MessageType.Regular) {
        JournalEntry entry = Entries.Count >= MaxJournalHistoryCount ? Entries.RemoveFromFront() : new JournalEntry();

        // There is no font or hue profile. Hue and text stay what Add received.
        byte font = (byte)(isunicode ? 0 : 9);
        DateTime timeNow = DateTime.Now;

        entry.Text = text;
        entry.Font = font;
        entry.Hue = hue;
        entry.Name = name;
        entry.IsUnicode = isunicode;
        entry.Time = timeNow;
        entry.TextType = type;
        entry.MessageType = messageType;

        Entries.AddToBack(entry);
        EntryAdded?.Invoke(null, entry);

        if (_fileWriter == null && !_writerHasException) CreateWriter();

        bool saveSerial = false;
        string serialText = saveSerial && serial.HasValue ? $"<0x{serial.Value:X8}> " : string.Empty;
        string output = $"[{timeNow:G}]  {serialText}{name}: {text}";
        if (string.IsNullOrWhiteSpace(name)) output = $"[{timeNow:G}]  {serialText}{text}";
        _fileWriter?.WriteLine(output);
    }

    private void CreateWriter() {
        // Profile path is not ported.
    }

    public void CloseWriter() {
        _fileWriter?.Flush();
        _fileWriter?.Dispose();
        _fileWriter = null;
    }

    public void Clear() {
        //Entries.Clear();
        CloseWriter();
    }
}

public class JournalEntry {
    public byte Font;
    public ushort Hue;

    public bool IsUnicode;
    public string Name;
    public string Text;

    // CLIENT 0, SYSTEM 1, OBJECT 2, GUILD_ALLY 3. Profile checkboxes are not ported.
    public byte TextType;
    public DateTime Time;

    public MessageType MessageType;
}
