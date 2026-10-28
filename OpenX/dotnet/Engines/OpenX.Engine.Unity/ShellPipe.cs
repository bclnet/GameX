using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using UnityEngine;

namespace OpenX;

public class ShellPipe : MonoBehaviour {
    public static int Width, Height;
    public static Action<string> OnItem;
    readonly ConcurrentQueue<string> _lines = new();
    int _pipe;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() {
        if (FindAnyObjectByType<ShellPipe>() != null) return;
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length - 1; i++)
            if (args[i] == "-parentHWND" && args[i + 1] != "0") {
                var go = new GameObject("ShellPipe");
                DontDestroyOnLoad(go);
                go.AddComponent<ShellPipe>();
                return;
            }
    }

    void Update() {
        if (_pipe == 0) Connect();
        while (_lines.TryDequeue(out var line)) Apply(line);
    }

    void Connect() {
        if (Interlocked.CompareExchange(ref _pipe, 1, 0) != 0) return;
        var thread = new Thread(Read) { IsBackground = true };
        thread.Start();
    }

    void Read() {
        NamedPipeClientStream client = null;
        try {
            client = new NamedPipeClientStream(".", "GameX.Unity.Shell", PipeDirection.In);
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

    void Apply(string line) {
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
