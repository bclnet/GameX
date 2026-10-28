using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace OpenX;

// ViewInfoX ticks this. Godot does not call a static class.
public static class ShellPipe {
    public static int Width, Height;
    public static Action<string> OnItem;
    static readonly ConcurrentQueue<string> _lines = new();
    static int _pipe;

    public static void Start() {
        if (Interlocked.CompareExchange(ref _pipe, 1, 0) != 0) return;
        var thread = new Thread(Read) { IsBackground = true };
        thread.Start();
    }

    public static void Tick() {
        if (_pipe == 0) Start();
        while (_lines.TryDequeue(out var line)) Apply(line);
        ShellSize.Apply();
    }

    static void Read() {
        NamedPipeClientStream client = null;
        try {
            client = new NamedPipeClientStream(".", "GameX.Godot.Shell", PipeDirection.In);
            client.Connect(200);
            Interlocked.Exchange(ref _pipe, 2);
            using var reader = new StreamReader(client, Encoding.UTF8, false, 256, true);
            string line;
            while ((line = reader.ReadLine()) != null) _lines.Enqueue(line);
        }
        catch { }
        finally {
            try { client?.Dispose(); } catch { }
            Interlocked.Exchange(ref _pipe, 0);
        }
    }

    static void Apply(string line) {
        if (line != null && line.StartsWith("SZ|", StringComparison.Ordinal)) {
            var p = line.Split('|');
            if (p.Length == 3 && int.TryParse(p[1], out var w) && int.TryParse(p[2], out var h) && w > 1 && h > 1) {
                Width = w;
                Height = h;
            }
            return;
        }
        OnItem?.Invoke(line);
    }
}
