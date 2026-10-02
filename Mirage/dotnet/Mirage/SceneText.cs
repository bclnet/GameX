using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Mirage;

/// <summary>
/// SceneText - the text form of a scene, and the parser for it.
///
/// One node per line, children indented by two spaces, components on lines starting with '@':
///
///   Root
///     Player layer=1 pos=(1,0,0)
///       @MeshRenderer mesh=Cube material=Std
///       @MeshCollider mesh=Cube
///       Weapon rot=(0,0.7071,0,0.7071) scale=(2,2,2) hidden
///         @MeshRenderer mesh=Sword
///
/// Names and values with spaces are quoted. Vectors are (x,y,z), quaternions (x,y,z,w),
/// colours #RRGGBB or #RRGGBBAA, and booleans appear as bare flags when true.
/// Identity transforms and default values are omitted, so Write(Parse(text)) is canonical.
/// </summary>
public static class SceneText {
    public const string Indent = "  ";
    public const char ComponentMarker = '@';
    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Component kinds the parser can create. Register custom components here: SceneText.Register<MyComponent>().
    /// </summary>
    public static readonly Dictionary<string, Func<Component>> Components = new(StringComparer.Ordinal) {
        [nameof(MeshRenderer)] = () => new MeshRenderer(),
        [nameof(SkinnedMeshRenderer)] = () => new SkinnedMeshRenderer(),
        [nameof(Sprite2D)] = () => new Sprite2D(),
        [nameof(Sprite3D)] = () => new Sprite3D(),
        [nameof(Light)] = () => new Light(),
        [nameof(LightProbe)] = () => new LightProbe(),
        [nameof(Camera)] = () => new Camera(),
        [nameof(Terrain)] = () => new Terrain(),
        [nameof(AudioSource)] = () => new AudioSource(),
        [nameof(MeshCollider)] = () => new MeshCollider(),
        [nameof(BoxCollider)] = () => new BoxCollider(),
        [nameof(Rigidbody)] = () => new Rigidbody(),
        [nameof(Script)] = () => new Script(),
    };
    public static void Register<T>() where T : Component, new() => Components[typeof(T).Name] = () => new T();

    #region Write

    /// <summary>
    /// Property writer handed to Component.WriteProps and Resource.WriteProps.
    /// </summary>
    public sealed class Props {
        readonly StringBuilder _b = new();
        public void Prop(string key, string value) { if (!string.IsNullOrEmpty(value)) _b.Append(' ').Append(key).Append('=').Append(Quote(value)); }
        public void Prop(string key, float value, float default_ = float.NaN) { if (float.IsNaN(default_) || value != default_) Prop(key, Format(value)); }
        public void Prop(string key, int value, int? default_ = null) { if (default_ == null || value != default_) Prop(key, value.ToString(Inv)); }
        public void Prop(string key, Vector3 value, Vector3? default_ = null) { if (default_ == null || value != default_) Prop(key, Format(value)); }
        public void Prop(string key, Quaternion value, Quaternion? default_ = null) { if (default_ == null || value != default_) Prop(key, Format(value)); }
        public void Prop(string key, Color value) => Prop(key, Format(value));
        public void Prop(string key, object value) {
            switch (value) {
                case null: break;
                case string s: Prop(key, s); break;
                case bool b: Flag(key, b); break;
                case float f: Prop(key, f); break;
                case double d: Prop(key, (float)d); break;
                case int i: Prop(key, i); break;
                case long l: Prop(key, l.ToString(Inv)); break;
                case Vector3 v: Prop(key, v); break;
                case Vector4 v: Prop(key, $"({Format(v.X)},{Format(v.Y)},{Format(v.Z)},{Format(v.W)})"); break;
                case Quaternion q: Prop(key, q); break;
                case Color c: Prop(key, c); break;
                case Resource r: Prop(key, r.Name); break;
                case Enum e: Prop(key, e.ToString()); break;
                default: Prop(key, value.ToString()); break;
            }
        }
        public void Flag(string key, bool value) { if (value) _b.Append(' ').Append(key); }
        public override string ToString() => _b.ToString();
    }

    /// <summary>
    /// The text of a node and its subtree.
    /// </summary>
    public static string Write(Node node) {
        var b = new StringBuilder();
        Write(b, node, 0);
        return b.ToString();
    }

    /// <summary>
    /// One line for a component: "@Kind key=value ...".
    /// </summary>
    public static string Write(Component c) {
        var p = new Props();
        p.Flag("disabled", !c.Enabled);
        c.WriteProps(p);
        return $"{ComponentMarker}{c.Kind}{p}";
    }

    /// <summary>
    /// One line for a resource: "Kind Name key=value ...".
    /// </summary>
    public static string Write(Resource r) {
        var p = new Props();
        r.WriteProps(p);
        p.Flag("deleted", r.Deleted);
        return $"{r.Kind} {Quote(r.Name)}{p}";
    }

    static void Write(StringBuilder b, Node node, int depth) {
        var pad = Repeat(depth);
        var p = new Props();
        p.Prop("tag", node.Tag);
        p.Prop("layer", node.Layer, 0);
        p.Prop("pos", node.Transform.Position, Vector3.Zero);
        p.Prop("rot", node.Transform.Rotation, Quaternion.Identity);
        p.Prop("scale", node.Transform.Scale, Vector3.One);
        p.Flag("static", node.IsStatic);
        p.Flag("hidden", !node.Visible);
        b.Append(pad).Append(Quote(node.Name)).Append(p).Append('\n');
        var inner = Repeat(depth + 1);
        foreach (var c in node.Components) b.Append(inner).Append(Write(c)).Append('\n');
        foreach (var c in node.Children) Write(b, c, depth + 1);
    }

    static string Repeat(int depth) => depth == 0 ? "" : string.Concat(Enumerable.Repeat(Indent, depth));

    public static string Format(float v) {
        var s = v.ToString("0.####", Inv);
        return s == "-0" ? "0" : s;
    }
    public static string Format(Vector3 v) => $"({Format(v.X)},{Format(v.Y)},{Format(v.Z)})";
    public static string Format(Quaternion q) => $"({Format(q.X)},{Format(q.Y)},{Format(q.Z)},{Format(q.W)})";
    public static string Format(Color c) => c.A == 255 ? $"#{c.R:X2}{c.G:X2}{c.B:X2}" : $"#{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";

    /// <summary>
    /// Quotes a value when it has spaces, quotes or is empty.
    /// </summary>
    public static string Quote(string s) {
        if (s == null) return "\"\"";
        if (s.Length > 0 && !s.Any(c => char.IsWhiteSpace(c) || c == '"' || c == '=')) return s;
        return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }

    #endregion

    #region Parse

    /// <summary>
    /// Parses scene text into a detached node tree. Resource references are resolved (or created) in the given set.
    /// </summary>
    public static Node Parse(string text, ResourceSet resources = null) {
        resources ??= new ResourceSet();
        var lines = (text ?? "").Replace("\r\n", "\n").Split('\n');
        var stack = new List<(int depth, Node node)>();
        Node root = null;
        var lineNo = 0;
        foreach (var raw in lines) {
            lineNo++;
            if (raw.Trim().Length == 0 || raw.TrimStart().StartsWith("#")) continue;
            var depth = Depth(raw, lineNo);
            var tokens = Tokenize(raw.Trim(), lineNo);
            if (tokens.Count == 0) continue;
            var head = tokens[0];
            if (head.Length > 0 && head[0] == ComponentMarker) {
                var owner = stack.LastOrDefault(s => s.depth == depth - 1).node
                    ?? throw new FormatException($"line {lineNo}: component '{head}' has no node above it");
                var kind = head.Substring(1);
                if (!Components.TryGetValue(kind, out var factory)) throw new FormatException($"line {lineNo}: unknown component '{kind}'");
                var c = factory();
                foreach (var (k, v) in Pairs(tokens, 1))
                    if (k == "disabled") c.Enabled = !ParseBool(v);
                    else if (!c.ReadProp(k, v, resources)) throw new FormatException($"line {lineNo}: {kind} has no property '{k}'");
                owner.AddComponent(c);
                continue;
            }
            var node = new Node(head);
            foreach (var (k, v) in Pairs(tokens, 1))
                switch (k) {
                    case "tag": node.Tag = v; break;
                    case "layer": node.Layer = int.Parse(v, Inv); break;
                    case "static": node.IsStatic = ParseBool(v); break;
                    case "hidden": node.Visible = !ParseBool(v); break;
                    case "pos": node.Transform.Position = ParseVector3(v); break;
                    case "rot": node.Transform.Rotation = ParseQuaternion(v); break;
                    case "scale": node.Transform.Scale = ParseVector3(v); break;
                    default: throw new FormatException($"line {lineNo}: node has no property '{k}'");
                }
            if (depth == 0) {
                if (root != null) throw new FormatException($"line {lineNo}: a second root '{head}'");
                root = node;
                stack.Clear();
            }
            else {
                while (stack.Count > 0 && stack[stack.Count - 1].depth >= depth) stack.RemoveAt(stack.Count - 1);
                if (stack.Count == 0 || stack[stack.Count - 1].depth != depth - 1) throw new FormatException($"line {lineNo}: '{head}' is indented too far");
                stack[stack.Count - 1].node.AddChild(node);
            }
            stack.Add((depth, node));
        }
        return root ?? throw new FormatException("no root node");
    }

    /// <summary>
    /// Write(Parse(text)) - the canonical form, for comparing expected text written by hand.
    /// </summary>
    public static string Normalize(string text) => Write(Parse(text));

    /// <summary>
    /// Whether two scene texts describe the same scene, ignoring indentation style and blank lines.
    /// </summary>
    public static bool AreEqual(string a, string b) => Normalize(a) == Normalize(b);

    static int Depth(string line, int lineNo) {
        var spaces = 0;
        foreach (var c in line) { if (c == ' ') spaces++; else if (c == '\t') spaces += Indent.Length; else break; }
        if (spaces % Indent.Length != 0) throw new FormatException($"line {lineNo}: indent must be a multiple of {Indent.Length} spaces");
        return spaces / Indent.Length;
    }

    static IEnumerable<(string key, string value)> Pairs(List<string> tokens, int from) {
        for (var i = from; i < tokens.Count; i++) {
            var t = tokens[i];
            var eq = t.IndexOf('=');
            if (eq < 0) yield return (t, "true");
            else yield return (t.Substring(0, eq), t.Substring(eq + 1));
        }
    }

    /// <summary>
    /// Splits on whitespace, keeping quoted strings and parenthesised tuples together. Quotes are removed.
    /// </summary>
    static List<string> Tokenize(string line, int lineNo) {
        var tokens = new List<string>();
        var b = new StringBuilder();
        var quoted = false; var parens = 0; var any = false;
        for (var i = 0; i < line.Length; i++) {
            var c = line[i];
            if (quoted) {
                if (c == '\\' && i + 1 < line.Length) { b.Append(line[++i]); }
                else if (c == '"') quoted = false;
                else b.Append(c);
                continue;
            }
            switch (c) {
                case '"': quoted = true; any = true; break;
                case '(': parens++; b.Append(c); break;
                case ')': parens--; b.Append(c); break;
                case ' ' or '\t' when parens == 0:
                    if (b.Length > 0 || any) { tokens.Add(b.ToString()); b.Clear(); any = false; }
                    break;
                default: b.Append(c); break;
            }
        }
        if (quoted) throw new FormatException($"line {lineNo}: unterminated quote");
        if (parens != 0) throw new FormatException($"line {lineNo}: unbalanced parentheses");
        if (b.Length > 0 || any) tokens.Add(b.ToString());
        return tokens;
    }

    public static float ParseFloat(string s) => float.Parse(s, NumberStyles.Float, Inv);
    public static bool ParseBool(string s) => s == "true" || s == "1" || s == "yes";
    public static float[] ParseTuple(string s, int count) {
        var t = s.Trim().TrimStart('(').TrimEnd(')').Split(',');
        if (t.Length != count) throw new FormatException($"expected {count} values in '{s}'");
        return t.Select(ParseFloat).ToArray();
    }
    public static Vector3 ParseVector3(string s) { var t = ParseTuple(s, 3); return new(t[0], t[1], t[2]); }
    public static Vector4 ParseVector4(string s) { var t = ParseTuple(s, 4); return new(t[0], t[1], t[2], t[3]); }
    public static Quaternion ParseQuaternion(string s) { var t = ParseTuple(s, 4); return new(t[0], t[1], t[2], t[3]); }
    public static Color ParseColor(string s) {
        var h = s.TrimStart('#');
        if (h.Length != 6 && h.Length != 8) throw new FormatException($"expected #RRGGBB or #RRGGBBAA, got '{s}'");
        int r = Convert.ToInt32(h.Substring(0, 2), 16), g = Convert.ToInt32(h.Substring(2, 2), 16), b = Convert.ToInt32(h.Substring(4, 2), 16);
        var a = h.Length == 8 ? Convert.ToInt32(h.Substring(6, 2), 16) : 255;
        return Color.FromArgb(a, r, g, b);
    }

    #endregion
}
