using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mirage;

/// <summary>
/// SceneTree - the root node plus a simulated game loop. Nothing here waits on
/// a clock: Step() advances exactly one frame, so tests are deterministic.
/// </summary>
public class SceneTree {
    public const float DefaultDelta = 1f / 60f;

    public readonly Node Root;
    public readonly ResourceSet Resources = new();

    /// <summary>
    /// Frames stepped so far; the frame being processed is Frame during Process().
    /// </summary>
    public int Frame { get; private set; }
    public float Time { get; private set; }
    public float Delta { get; private set; }
    public bool Running { get; private set; }

    /// <summary>
    /// Everything that happened, in order: node adds and removes, component readies, frames, and log lines. Assert on it.
    /// </summary>
    public readonly List<string> Journal = [];

    /// <summary>
    /// Where Print() and, with Trace on, every journal line goes. Console.Out by default.
    /// </summary>
    public TextWriter Out = Console.Out;
    public bool Trace;

    public SceneTree(string rootName = "Root") {
        Root = new Node(rootName) { Tree = this };
    }

    #region Loop

    /// <summary>
    /// Advances one frame: readies any component not yet readied, then processes every enabled component, depth first.
    /// </summary>
    public void Step(float? delta = null) {
        Delta = delta ?? DefaultDelta;
        Frame++;
        Time += Delta;
        Running = true;
        Record($"frame {Frame}");
        var components = Root.DescendantsAndSelf().SelectMany(n => n.Components.ToArray()).ToArray();
        foreach (var c in components)
            if (!c.IsReady && c.Node != null && c.Node.InTree) { c.IsReady = true; Record($"ready {c.Node.Path}:{c.Kind}"); c.Ready(); }
        foreach (var c in components)
            if (c.Enabled && c.IsReady && c.Node != null && c.Node.InTree) c.Process(Delta);
    }

    /// <summary>
    /// Steps a number of frames.
    /// </summary>
    public void Run(int frames, float? delta = null) { for (var i = 0; i < frames; i++) Step(delta); }

    /// <summary>
    /// Runs until the predicate holds or maxFrames is reached. Returns whether it held.
    /// </summary>
    public bool RunUntil(Func<bool> done, int maxFrames = 1000, float? delta = null) {
        for (var i = 0; i < maxFrames; i++) { if (done()) return true; Step(delta); }
        return done();
    }

    /// <summary>
    /// Removes every node under the root, all resources, and the journal; the frame counter restarts.
    /// </summary>
    public void Clear() {
        foreach (var c in Root.Children.ToArray()) Root.RemoveChild(c);
        Resources.Clear();
        Journal.Clear();
        Frame = 0; Time = 0; Delta = 0; Running = false;
    }

    #endregion

    #region Output

    internal void Record(string line) {
        Journal.Add(line);
        if (Trace) Out.WriteLine(line);
    }

    /// <summary>
    /// Adds a log line to the journal ("log ...").
    /// </summary>
    public void Log(string message) => Record($"log {message}");

    /// <summary>
    /// Journal lines that start with a prefix, e.g. "ready ".
    /// </summary>
    public IEnumerable<string> JournalOf(string prefix) => Journal.Where(s => s.StartsWith(prefix, StringComparison.Ordinal));

    /// <summary>
    /// The whole scene as text.
    /// </summary>
    public string ToText() => SceneText.Write(Root);

    /// <summary>
    /// Writes the scene text to Out.
    /// </summary>
    public void Print() => Out.Write(ToText());

    public IEnumerable<Node> Nodes => Root.DescendantsAndSelf();
    public Node Find(string path) => Root.Find(path);

    /// <summary>
    /// Replaces the scene with one parsed from text. The first line becomes the root's name.
    /// </summary>
    public void Load(string text) {
        Clear();
        var parsed = SceneText.Parse(text, Resources);
        Root.Name = parsed.Name;
        Root.Tag = parsed.Tag; Root.Layer = parsed.Layer; Root.Visible = parsed.Visible; Root.IsStatic = parsed.IsStatic;
        Root.Transform.Position = parsed.Transform.Position; Root.Transform.Rotation = parsed.Transform.Rotation; Root.Transform.Scale = parsed.Transform.Scale;
        foreach (var c in parsed.Components.ToArray()) { parsed.RemoveComponent(c); Root.AddComponent(c); }
        foreach (var c in parsed.Children.ToArray()) Root.AddChild(c);
    }

    public override string ToString() => ToText();

    #endregion
}
