// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using Vector2 = System.Numerics.Vector2;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO AnchorManager. Control has no AnchorType and no Bounds.
/// The anchor type id and the gump rectangle are stored here and on <see cref="AnchorGroup"/>.
/// 0 is none, 1 is spell, 2 is health bar. A gump is anchorable only after <see cref="SetAnchorId"/>.
/// </summary>
public sealed class AnchorManager {
    // ClassicUO anchor-corner tables. Nothing in this file reads them.
#pragma warning disable CS0414
    static readonly Vector2[][] _anchorTriangles = {
        new[] { new Vector2(0f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 1f) },
        new[] { new Vector2(0f, 0f), new Vector2(0.5f, 0.5f), new Vector2(1f, 0f) },
        new[] { new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(1f, 1f) },
        new[] { new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(1f, 1f) }
    };

    static readonly Point[] _anchorDirectionMatrix = {
        new Point(-1, 0),
        new Point(0, -1),
        new Point(1, 0),
        new Point(0, 1)
    };

    static readonly Point[] _anchorMultiplierMatrix = {
        new Point(0, 0),
        new Point(0, 0),
        new Point(1, 0),
        new Point(0, 1)
    };
#pragma warning restore CS0414

    struct AnchorMeta {
        public bool HasId;
        public int Id;
        public int WidthMultiplier;
        public int HeightMultiplier;
        public int GroupMatrixWidth;
        public int GroupMatrixHeight;
        public int X, Y, Width, Height;
        public bool HasRect;
    }

    readonly Dictionary<Gump, AnchorGroup> reverseMap = new Dictionary<Gump, AnchorGroup>();
    readonly Dictionary<Gump, AnchorMeta> _meta = new Dictionary<Gump, AnchorMeta>();

    public AnchorGroup this[Gump control] {
        get {
            reverseMap.TryGetValue(control, out var group);
            return group;
        }
        set {
            if (value != null) {
                value._owner = this;
                if (control != null && _meta.TryGetValue(control, out var meta) && meta.HasId && value.Id == 0)
                    value.Id = meta.Id;
            }
            if (reverseMap.ContainsKey(control) && value == null)
                reverseMap.Remove(control);
            else
                reverseMap.Add(control, value);
        }
    }

    /// <summary>Anchor type id. The rectangle is stored from the gump origin and size.</summary>
    public void SetAnchorId(Gump gump, int id, int widthMultiplier = 1, int heightMultiplier = 1, int groupMatrixWidth = 0, int groupMatrixHeight = 0) {
        if (gump == null) return;
        _meta.TryGetValue(gump, out var m);
        m.HasId = true;
        m.Id = id;
        m.WidthMultiplier = widthMultiplier < 1 ? 1 : widthMultiplier;
        m.HeightMultiplier = heightMultiplier < 1 ? 1 : heightMultiplier;
        m.GroupMatrixWidth = groupMatrixWidth;
        m.GroupMatrixHeight = groupMatrixHeight;
        m.X = gump.X;
        m.Y = gump.Y;
        m.Width = gump.Width;
        m.Height = gump.Height;
        m.HasRect = true;
        _meta[gump] = m;
        if (reverseMap.TryGetValue(gump, out var group) && group != null) {
            if (group.Id == 0) group.Id = id;
            group.StoreGroupRectangle();
        }
    }

    public void Save(XmlTextWriter writer) {
        // Profile path is not ported.
        foreach (var value in reverseMap.Values.Distinct())
            value.Save(writer);
    }

    /*public void AttachControl(Gump host, Gump control) {
        if (SameAnchor(host, control) && this[control] == null) {
            if (this[host] == null)
                this[host] = new AnchorGroup(this, host);
            this[host].AnchorControlAt(control, host, new Point(control.X, control.Y));
            this[control] = this[host];
        }
    }*/

    public void DropControl(Gump draggedControl, Gump host) {
        if (SameAnchor(host, draggedControl) && this[draggedControl] == null) {
            (Point? relativePosition, _) = GetAnchorDirection(draggedControl, host);
            if (relativePosition.HasValue) {
                if (this[host] == null)
                    this[host] = new AnchorGroup(this, host);
                if (this[host].IsEmptyDirection(draggedControl, host, relativePosition.Value)) {
                    this[host].AnchorControlAt(draggedControl, host, relativePosition.Value);
                    this[draggedControl] = this[host];
                }
            }
        }
    }

    public Point GetCandidateDropLocation(Gump draggedControl, Gump host) {
        if (SameAnchor(host, draggedControl) && this[draggedControl] == null) {
            (Point? relativePosition, var g) = GetAnchorDirection(draggedControl, host);
            if (relativePosition.HasValue) {
                if (this[host] == null || this[host].IsEmptyDirection(draggedControl, host, relativePosition.Value)) {
                    var offset = Mul(relativePosition.Value, new Point(GroupMatrixWidthOf(g), GroupMatrixHeightOf(g)));
                    return new Point(host.X + offset.X, host.Y + offset.Y);
                }
            }
        }
        return new Point(draggedControl.X, draggedControl.Y);
    }

    public Gump GetAnchorableControlUnder(Gump draggedControl) => ClosestOverlappingControl(draggedControl);

    public void DetachControl(Gump control) {
        if (this[control] == null) return;
        var group = reverseMap.Where(o => o.Value == this[control]).Select(o => o.Key).ToList();
        if (group.Count == 2) { // if detach 1+1 - need destroy all group
            foreach (var ctrl in group) {
                this[ctrl].DetachControl(ctrl);
                this[ctrl] = null;
            }
        }
        else {
            this[control].DetachControl(control);
            this[control] = null;
        }
    }

    public void DisposeAllControls(Gump control) {
        if (this[control] == null) return;
        foreach (var ctrl in reverseMap.Where(o => o.Value == this[control]).Select(o => o.Key).ToList()) {
            this[ctrl] = null;
            ctrl.Dispose();
        }
    }

    (Point?, Gump) GetAnchorDirection(Gump draggedControl, Gump host) {
        var xdistancescale = Math.Abs(draggedControl.X - host.X) * 100 / host.Width;
        var ydistancescale = Math.Abs(draggedControl.Y - host.Y) * 100 / host.Height;
        if (xdistancescale > ydistancescale) {
            if (draggedControl.X > host.X)
                return (new Point(WidthMultiplierOf(host), 0), host);
            return (new Point(-WidthMultiplierOf(draggedControl), 0), draggedControl);
        }
        if (draggedControl.Y > host.Y)
            return (new Point(0, HeightMultiplierOf(host)), host);
        return (new Point(0, -HeightMultiplierOf(draggedControl)), draggedControl);
    }

    bool IsPointInPolygon(Vector2[] polygon, Vector2 point) {
        var isInside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++) {
            if (polygon[i].Y > point.Y != polygon[j].Y > point.Y && point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X)
                isInside = !isInside;
        }
        return isInside;
    }

    public Gump ClosestOverlappingControl(Gump control) {
        if (control == null || control.IsDisposed) return null;
        Gump closestControl = null;
        var closestDistance = 99999;
        foreach (var c in UIManager.Gumps) {
            if (!c.IsDisposed && SameAnchor(c, control) && IsOverlapping(control, c)) {
                var dirtyDistance = Math.Abs(control.X - c.X) + Math.Abs(control.Y - c.Y);
                if (dirtyDistance < closestDistance) {
                    closestDistance = dirtyDistance;
                    closestControl = c;
                }
            }
        }
        return closestControl;
    }

    bool IsOverlapping(Gump control, Gump host) {
        if (control == host) return false;
        var a = BoundsOf(control);
        var b = BoundsOf(host);
        if (a.Top > b.Bottom || a.Bottom < b.Top) return false;
        if (a.Right < b.Left || a.Left > b.Right) return false;
        return true;
    }

    bool SameAnchor(Gump a, Gump b) => HasAnchor(a) && HasAnchor(b) && AnchorIdOf(a) == AnchorIdOf(b);

    bool HasAnchor(Gump gump) => gump != null && _meta.TryGetValue(gump, out var m) && m.HasId;

    int AnchorIdOf(Gump gump) => HasAnchor(gump) ? _meta[gump].Id : 0;

    int WidthMultiplierOf(Gump gump) {
        if (gump != null && _meta.TryGetValue(gump, out var m) && m.WidthMultiplier > 0) return m.WidthMultiplier;
        return 1;
    }

    int HeightMultiplierOf(Gump gump) {
        if (gump != null && _meta.TryGetValue(gump, out var m) && m.HeightMultiplier > 0) return m.HeightMultiplier;
        return 1;
    }

    int GroupMatrixWidthOf(Gump gump) {
        if (gump != null && _meta.TryGetValue(gump, out var m) && m.GroupMatrixWidth > 0) return m.GroupMatrixWidth;
        return gump != null ? gump.Width : 0;
    }

    int GroupMatrixHeightOf(Gump gump) {
        if (gump != null && _meta.TryGetValue(gump, out var m) && m.GroupMatrixHeight > 0) return m.GroupMatrixHeight;
        return gump != null ? gump.Height : 0;
    }

    void StoreRectangle(Gump gump) {
        if (gump == null) return;
        _meta.TryGetValue(gump, out var m);
        if (m.WidthMultiplier < 1) m.WidthMultiplier = 1;
        if (m.HeightMultiplier < 1) m.HeightMultiplier = 1;
        m.X = gump.X;
        m.Y = gump.Y;
        m.Width = gump.Width;
        m.Height = gump.Height;
        m.HasRect = true;
        _meta[gump] = m;
    }

    Rectangle BoundsOf(Gump gump) {
        StoreRectangle(gump);
        var m = _meta[gump];
        if (!m.HasRect) return new Rectangle(gump.X, gump.Y, gump.Width, gump.Height);
        return new Rectangle(m.X, m.Y, m.Width, m.Height);
    }

    void MakeTopMostGump(Gump gump) {
        if (gump == null) return;
        var list = UIManager.Gumps;
        for (var start = list.First; start != null; start = start.Next) {
            if (start.Value != gump) continue;
            if (gump.LayerOrder == UILayer.Under) {
                if (start != list.Last) {
                    list.Remove(gump);
                    list.AddBefore(list.Last, start);
                }
            }
            else {
                list.Remove(gump);
                list.AddFirst(start);
            }
            break;
        }
    }

    static Point Add(Point a, Point b) => new Point(a.X + b.X, a.Y + b.Y);

    static Point Mul(Point a, Point b) => new Point(a.X * b.X, a.Y * b.Y);

    enum AnchorDirection {
        Left,
        Top,
        Right,
        Bottom
    }

    public class AnchorGroup {
        Gump[,] controlMatrix;
        int updateCount;
        internal AnchorManager _owner;

        public int Id;

        /// <summary>Union of the member gumps. Control has no Bounds.</summary>
        public Rectangle Rectangle { get; private set; }

        public AnchorGroup(Gump initial) : this(null, initial) { }

        public AnchorGroup() {
            controlMatrix = new Gump[0, 0];
        }

        internal AnchorGroup(AnchorManager owner, Gump initial) {
            _owner = owner;
            var wm = Math.Max(1, WidthMultiplierOf(initial));
            var hm = Math.Max(1, HeightMultiplierOf(initial));
            controlMatrix = new Gump[wm, hm];
            AddControlToMatrix(0, 0, initial);
            if (owner != null && owner.HasAnchor(initial)) Id = owner.AnchorIdOf(initial);
        }

        public void AddControlToMatrix(int xinit, int yInit, Gump control) {
            var wm = WidthMultiplierOf(control);
            var hm = HeightMultiplierOf(control);
            for (var x = 0; x < wm; x++)
                for (var y = 0; y < hm; y++)
                    controlMatrix[x + xinit, y + yInit] = control;
            StoreGroupRectangle();
        }

        public void Save(XmlTextWriter writer) {
            // Profile path is not ported.
            writer.WriteStartElement("anchored_group_gump");
            writer.WriteAttributeString("matrix_w", controlMatrix.GetLength(0).ToString());
            writer.WriteAttributeString("matrix_h", controlMatrix.GetLength(1).ToString());
            for (var y = 0; y < controlMatrix.GetLength(1); y++) {
                for (var x = 0; x < controlMatrix.GetLength(0); x++) {
                    var gump = controlMatrix[x, y];
                    if (gump == null) continue;
                    writer.WriteStartElement("gump");
                    // Gump has no Save. x, y, and serial are the fields it would write.
                    writer.WriteAttributeString("x", gump.X.ToString());
                    writer.WriteAttributeString("y", gump.Y.ToString());
                    writer.WriteAttributeString("serial", gump.LocalSerial.ToString());
                    writer.WriteAttributeString("matrix_x", x.ToString());
                    writer.WriteAttributeString("matrix_y", y.ToString());
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }

        public void MakeTopMost() {
            for (var x = 0; x < controlMatrix.GetLength(0); x++)
                for (var y = 0; y < controlMatrix.GetLength(1); y++)
                    if (controlMatrix[x, y] != null)
                        _owner?.MakeTopMostGump(controlMatrix[x, y]);
        }

        public void DetachControl(Gump control) {
            for (var x = 0; x < controlMatrix.GetLength(0); x++)
                for (var y = 0; y < controlMatrix.GetLength(1); y++)
                    if (controlMatrix[x, y] == control)
                        controlMatrix[x, y] = null;
            StoreGroupRectangle();
        }

        public void UpdateLocation(Control control, int deltaX, int deltaY) {
            if (updateCount != 0) return;
            updateCount++;
            var visited = new HashSet<Control>();
            for (var x = 0; x < controlMatrix.GetLength(0); x++) {
                for (var y = 0; y < controlMatrix.GetLength(1); y++) {
                    var cell = controlMatrix[x, y];
                    if (cell != null && cell != control && visited.Add(cell)) {
                        cell.X += deltaX;
                        cell.Y += deltaY;
                    }
                }
            }
            updateCount--;
            StoreGroupRectangle();
        }

        public void AnchorControlAt(Gump control, Gump host, Point relativePosition) {
            var hostPosition = GetControlCoordinates(host);
            if (!hostPosition.HasValue) return;
            var targetX = hostPosition.Value.X + relativePosition.X;
            var targetY = hostPosition.Value.Y + relativePosition.Y;
            if (!IsEmptyDirection(targetX, targetY)) return;
            var wm = WidthMultiplierOf(control);
            var hm = HeightMultiplierOf(control);
            if (targetX < 0) // Create new column left
                ResizeMatrix(controlMatrix.GetLength(0) + wm, controlMatrix.GetLength(1), wm, 0);
            else if (targetX > controlMatrix.GetLength(0) - wm) // Create new column right
                ResizeMatrix(controlMatrix.GetLength(0) + wm, controlMatrix.GetLength(1), 0, 0);
            if (targetY < 0) // Create new row top
                ResizeMatrix(controlMatrix.GetLength(0), controlMatrix.GetLength(1) + hm, 0, hm);
            else if (targetY > controlMatrix.GetLength(1) - 1) // Create new row bottom
                ResizeMatrix(controlMatrix.GetLength(0), controlMatrix.GetLength(1) + hm, 0, 0);
            hostPosition = GetControlCoordinates(host);
            if (!hostPosition.HasValue) return;
            targetX = hostPosition.Value.X + relativePosition.X;
            targetY = hostPosition.Value.Y + relativePosition.Y;
            AddControlToMatrix(targetX, targetY, control);
        }

        public bool IsEmptyDirection(Gump draggedControl, Gump host, Point relativePosition) {
            var hostPosition = GetControlCoordinates(host);
            var isEmpty = true;
            if (hostPosition.HasValue) {
                var targetInitPosition = AnchorManager.Add(hostPosition.Value, relativePosition);
                for (var xOffset = 0; xOffset < WidthMultiplierOf(draggedControl); xOffset++)
                    for (var yOffset = 0; yOffset < HeightMultiplierOf(draggedControl); yOffset++)
                        isEmpty &= IsEmptyDirection(targetInitPosition.X + xOffset, targetInitPosition.Y + yOffset);
                //// TODO: loop through
                //var targetX = hostPosition.Value.X + relativePosition.X;
                //var targetY = hostPosition.Value.Y + relativePosition.Y;
                //return IsEmptyDirection(targetX, targetY);
            }
            return isEmpty;
        }

        public bool IsEmptyDirection(int x, int y) {
            if (x < 0 || x > controlMatrix.GetLength(0) - 1 || y < 0 || y > controlMatrix.GetLength(1) - 1)
                return true;
            return controlMatrix[x, y] == null;
        }

        Point? GetControlCoordinates(Gump control) {
            for (var x = 0; x < controlMatrix.GetLength(0); x++)
                for (var y = 0; y < controlMatrix.GetLength(1); y++)
                    if (controlMatrix[x, y] == control)
                        return new Point(x, y);
            return null;
        }

        public void ResizeMatrix(int xCount, int yCount, int xInitial, int yInitial) {
            var newMatrix = new Gump[xCount, yCount];
            for (var x = 0; x < controlMatrix.GetLength(0); x++)
                for (var y = 0; y < controlMatrix.GetLength(1); y++)
                    newMatrix[x + xInitial, y + yInitial] = controlMatrix[x, y];
            controlMatrix = newMatrix;
        }

        void printMatrix() {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            for (var y = 0; y < controlMatrix.GetLength(1); y++) {
                for (var x = 0; x < controlMatrix.GetLength(0); x++) {
                    if (controlMatrix[x, y] != null)
                        Console.Write(" " + controlMatrix[x, y].LocalSerial + " ");
                    else
                        Console.Write(" ---------- ");
                }
                Console.WriteLine();
            }
        }

        internal void StoreGroupRectangle() {
            var any = false;
            var left = 0;
            var top = 0;
            var right = 0;
            var bottom = 0;
            var seen = new HashSet<Gump>();
            for (var x = 0; x < controlMatrix.GetLength(0); x++) {
                for (var y = 0; y < controlMatrix.GetLength(1); y++) {
                    var g = controlMatrix[x, y];
                    if (g == null || !seen.Add(g)) continue;
                    _owner?.StoreRectangle(g);
                    var rx = g.X;
                    var ry = g.Y;
                    var rr = g.X + g.Width;
                    var rb = g.Y + g.Height;
                    if (!any) {
                        left = rx;
                        top = ry;
                        right = rr;
                        bottom = rb;
                        any = true;
                    }
                    else {
                        if (rx < left) left = rx;
                        if (ry < top) top = ry;
                        if (rr > right) right = rr;
                        if (rb > bottom) bottom = rb;
                    }
                }
            }
            Rectangle = any ? new Rectangle(left, top, right - left, bottom - top) : default;
        }

        int WidthMultiplierOf(Gump gump) => _owner != null ? _owner.WidthMultiplierOf(gump) : 1;

        int HeightMultiplierOf(Gump gump) => _owner != null ? _owner.HeightMultiplierOf(gump) : 1;
    }
}
