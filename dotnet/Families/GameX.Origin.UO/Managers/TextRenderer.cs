// SPDX-License-Identifier: BSD-2-Clause

using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO TextRenderer. Messages are a DLeft/DRight list, not LinkedObject.
/// Time below Time.Ticks is skipped. Draw applies the gump offset and does not draw.
/// </summary>
public class TextRenderer {
    /// <summary>Screen line. X and Y are pixels (ClassicUO RealScreenPosition).</summary>
    private sealed class Node {
        public byte Alpha = 0xFF;
        public byte Shown = 0xFF;
        public Node DLeft, DRight;
        public ushort Hue, DrawHue;
        public string Text;
        public bool IsDestroyed, IsTransparent, Owned;
        public int Width, Height, X, Y;
        public long Time;

        public void Destroy() {
            if (IsDestroyed) return;
            UnlinkD();
            IsDestroyed = true;
            Text = null;
            Owned = false;
            X = 0;
            Y = 0;
        }

        public void UnlinkD() {
            if (DRight != null) DRight.DLeft = DLeft;
            if (DLeft != null) DLeft.DRight = DRight;
            DRight = null;
            DLeft = null;
        }
    }

    private struct Bounds {
        public int X, Y, Width, Height;
        public bool Intersects(Bounds b) =>
            b.X < X + Width && X < b.X + b.Width && b.Y < Y + Height && Y < b.Y + b.Height;
    }

    private readonly List<Bounds> _bounds = [];
    private readonly Node _head = new();
    private Node _draw;
    private Node _hover;

    public TextRenderer(World world) {
        World = world;
    }

    protected World World { get; }

    public virtual void Destroy() { }

    public virtual void Update() {
        ProcessWorldText(false);
    }

    /// <summary>
    /// ClassicUO Draw without a batcher. Walks from the tail. A hit is the pointer over the line.
    /// MoveToTop moves that line. Nothing is drawn.
    /// </summary>
    public virtual void Draw(int startX, int startY, float layerDepth, bool isGump = false) {
        ProcessWorldText(false);
        _hover = null;
        int mouseX = Mouse.Position.X;
        int mouseY = Mouse.Position.Y;
        for (Node o = _draw; o != null; o = o.DLeft) {
            if (o.IsDestroyed || string.IsNullOrEmpty(o.Text) || o.Time < Time.Ticks) continue;
            int x = o.X;
            int y = o.Y;
            if (isGump) {
                x += startX;
                y += startY;
            }
            bool hit = Contains(mouseX, mouseY, x, y, o.Width, o.Height);
            if (hit) _hover = o;
            int shown = o.Alpha;
            if (o.IsTransparent && shown == 0xFF) shown = 0x7F;
            o.Shown = (byte)shown;
            ushort hue = o.Hue;
            if (!isGump && o.Owned && hit) hue = 0x0035;
            o.DrawHue = hue;
        }
    }

    /// <summary>Moves the line under the pointer on the last Draw. No pointer hit leaves the list alone.</summary>
    public void MoveToTop() {
        Node obj = _hover;
        if (obj == null || obj.IsDestroyed) return;
        obj.UnlinkD();
        Node next = _head.DRight;
        _head.DRight = obj;
        obj.DLeft = _head;
        obj.DRight = next;
        if (next != null) next.DLeft = obj;
    }

    public void ProcessWorldText(bool doit) {
        if (doit && _bounds.Count != 0) _bounds.Clear();
        for (_draw = _head; _draw != null; _draw = _draw.DRight) {
            if (doit) {
                Node t = _draw;
                if (t.Time >= Time.Ticks && !string.IsNullOrEmpty(t.Text) && t.Owned) {
                    t.IsTransparent = Collides(t);
                    CalculateAlpha(t);
                }
            }
            if (_draw.DRight == null) break;
        }
    }

    /// <summary>ClassicUO gates this on Profile.TextFading, which defaults to true. There is no profile here.</summary>
    private void CalculateAlpha(Node msg) {
        int delta = (int)(msg.Time - Time.Ticks);
        if (delta >= 0 && delta <= 1000) {
            delta /= 10;
            if (delta > 100) delta = 100;
            else if (delta < 1) delta = 0;
            delta = 255 * delta / 100;
            if (!msg.IsTransparent || delta <= 0x7F) msg.Alpha = (byte)delta;
            msg.IsTransparent = true;
        }
    }

    private bool Collides(Node msg) {
        bool result = false;
        var rect = new Bounds { X = msg.X, Y = msg.Y, Width = msg.Width, Height = msg.Height };
        for (int i = 0; i < _bounds.Count; i++) {
            if (_bounds[i].Intersects(rect)) {
                result = true;
                break;
            }
        }
        _bounds.Add(rect);
        return result;
    }

    /// <summary>
    /// Inserts after the head. owned enables overlap and the fade. x and y are screen pixels.
    /// </summary>
    public void AddMessage(string text, ushort hue, int x, int y, long time, bool owned = false, byte font = 1) {
        if (text == null) return;
        Measure(text, font, out int width, out int height);
        var obj = new Node {
            Text = text,
            Hue = hue,
            X = x,
            Y = y,
            Time = time,
            Owned = owned,
            Width = width,
            Height = height,
        };
        obj.UnlinkD();
        Node item = _head;
        if (item.DRight != null) {
            Node next = item.DRight;
            item.DRight = obj;
            obj.DLeft = item;
            obj.DRight = next;
            next.DLeft = obj;
        }
        else {
            item.DRight = obj;
            obj.DLeft = item;
            obj.DRight = null;
        }
    }

    public virtual void Clear() {
        ClearChain(_head);
        ClearChain(_draw);
        _head.DLeft = null;
        _head.DRight = null;
        _draw = null;
        _hover = null;
    }

    private void ClearChain(Node first) {
        while (first?.DLeft != null) first = first.DLeft;
        while (first != null) {
            Node next = first.DRight;
            if (!ReferenceEquals(first, _head)) first.Destroy();
            first = next;
        }
    }

    protected static void Measure(string text, byte font, out int width, out int height) {
        if (string.IsNullOrEmpty(text)) {
            width = 0;
            height = 0;
            return;
        }
        width = GumpArt.Sink?.MeasureText(text, font) ?? text.Length * 6;
        height = 16;
    }

    private static bool Contains(int mx, int my, int x, int y, int w, int h) =>
        w > 0 && h > 0 && mx >= x && my >= y && mx < x + w && my < y + h;
}
