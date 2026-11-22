// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.IO;

namespace GameX.Origin.UO;

/// <summary>ClassicUO JournalManager. Entries stay in memory, capped at 100. The log file opens only when SaveJournalToFile is set.</summary>
public sealed class JournalManager {
    const int MaxJournalHistoryCount = 100;

    StreamWriter _fileWriter;
    bool _writerHasException = false;

    public static Deque<JournalEntry> Entries { get; } = new Deque<JournalEntry>(MaxJournalHistoryCount);

    public event EventHandler<JournalEntry> EntryAdded;

    public void Add(string text, ushort hue, string name, uint? serial, byte type, bool isunicode = true, MessageType messageType = MessageType.Regular) {
        JournalEntry entry = Entries.Count >= MaxJournalHistoryCount ? Entries.RemoveFromFront() : new JournalEntry();

        byte font = (byte)(isunicode ? 0 : 9);
        if (UOSettings.GetBool("OverrideAllFonts", false)) {
            font = (byte)UOSettings.GetInt("ChatFont", 1);
            isunicode = UOSettings.GetBool("OverrideAllFontsIsUnicode", true);
        }
        DateTime timeNow = DateTime.Now;

        entry.Text = text;
        entry.Font = font;
        entry.Hue = hue;
        entry.Name = name;
        entry.IsUnicode = isunicode;
        entry.Time = timeNow;
        entry.TextType = type;
        entry.MessageType = messageType;
        if (UOSettings.GetBool("ForceUnicodeJournal", false)) {
            entry.Font = 0;
            entry.IsUnicode = true;
        }

        Entries.AddToBack(entry);
        EntryAdded?.Invoke(null, entry);

        if (_fileWriter == null && !_writerHasException) CreateWriter();

        bool saveSerial = UOSettings.GetBool("JournalFileWithSerial", false);
        string serialText = saveSerial && serial.HasValue ? $"<0x{serial.Value:X8}> " : string.Empty;
        string output = $"[{timeNow:G}]  {serialText}{name}: {text}";
        if (string.IsNullOrWhiteSpace(name)) output = $"[{timeNow:G}]  {serialText}{text}";
        _fileWriter?.WriteLine(output);
    }

    private void CreateWriter() {
        if (_fileWriter != null || !UOSettings.GetBool("SaveJournalToFile", false)) return;
        try {
            var path = Path.Combine(AppContext.BaseDirectory, "Data", "Client", "JournalLogs");
            Directory.CreateDirectory(path);
            var maxJournalFiles = UOSettings.GetInt("MaxJournalFiles", 100);
            if (maxJournalFiles >= 0) {
                try {
                    var files = Directory.GetFiles(path, "*_journal.txt");
                    Array.Sort(files);
                    Array.Reverse(files);
                    for (var i = files.Length - 1; i >= maxJournalFiles; --i)
                        File.Delete(files[i]);
                }
                catch { }
            }
            _fileWriter = new StreamWriter(File.Open(Path.Combine(path, $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_journal.txt"), FileMode.Create, FileAccess.Write, FileShare.Read)) {
                AutoFlush = true
            };
        }
        catch {
            _writerHasException = true;
        }
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
