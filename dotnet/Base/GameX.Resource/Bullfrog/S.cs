﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GameX.Bullfrog;

public static class S {
    static readonly IDictionary<string, ZipArchiveEntry> Entries;
    static S() {
        var assembly = typeof(S).Assembly;
        var s = assembly.GetManifestResourceStream("GameX.Resource.Bullfrog.S.zip");
        var pak = new ZipArchive(s, ZipArchiveMode.Read);
        Entries = pak.Entries.ToDictionary(x => x.Name, x => x);
    }

    public struct Event {
        public int Frame;
        public decimal Music;
        public decimal Sound;
        public string Subtitle;
    }

    static readonly ConcurrentDictionary<string, Event[]> Events = new();
    public static Event[] GetEvents(string path) => Events.GetOrAdd(path, x => {
        if (!Entries.TryGetValue(path, out var entry)) return null;
        var value = new List<Event>();
        string line;
        using var r = new StreamReader(entry.Open());
        while ((line = r.ReadLine()) != null) {
            var p = line.Split(' ');
            var frame = int.Parse(p[0]);
            var evnt = new Event { Frame = frame, Music = -1M, Sound = -1M };
            for (var i = 1; i < p.Length; i++) {
                var part = p[i];
                if (part[0] == '#') { }
                else if (part[0] == '-') evnt.Music = -decimal.Parse(part);
                else if (char.IsDigit(part[0])) evnt.Sound = decimal.Parse(part);
                else evnt.Subtitle = part;
            }
            value.Add(evnt);
        }
        return value.ToArray();
    });
}