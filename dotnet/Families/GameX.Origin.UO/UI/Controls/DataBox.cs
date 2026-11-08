// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>ClassicUO DataBox. ReArrangeChildren stacks visible children from the top.</summary>
public class DataBox : Control {
    public DataBox(int x, int y, int w, int h) {
        CanMove = false;
        AcceptMouseInput = true;
        X = x;
        Y = y;
        Width = w;
        Height = h;
        WantUpdateSize = false;
    }

    public bool ContainsByBounds { get; set; }

    public int ScreenCoordinateX {
        get {
            var x = X;
            for (var p = Parent; p != null; p = p.Parent) x += p.X;
            return x;
        }
    }

    public int ScreenCoordinateY {
        get {
            var y = Y;
            for (var p = Parent; p != null; p = p.Parent) y += p.Y;
            return y;
        }
    }

    public void ReArrangeChildren() {
        for (int i = 0, height = 0; i < Children.Count; ++i) {
            var c = Children[i];
            if (c.IsVisible && !c.IsDisposed) {
                c.Y = height;
                height += c.Height;
            }
        }
        WantUpdateSize = true;
    }

    public bool Contains(int x, int y) {
        if (ContainsByBounds) return true;
        Control t = null;
        x += ScreenCoordinateX;
        y += ScreenCoordinateY;
        for (var i = 0; i < Children.Count; i++) HitTest(Children[i], x, y, ref t);
        return t != null;
    }

    static void HitTest(Control c, int x, int y, ref Control res) {
        if (c == null || !c.IsVisible || !c.IsEnabled || c.IsDisposed) return;
        var ax = 0;
        var ay = 0;
        for (var p = c; p != null; p = p.Parent) {
            ax += p.X;
            ay += p.Y;
        }
        if (x < ax || y < ay || x >= ax + c.Width || y >= ay + c.Height) return;
        if (c.AcceptMouseInput && (res == null || res.Priority >= c.Priority)) res = c;
        var children = c.Children;
        for (var i = 0; i < children.Count; i++) {
            var child = children[i];
            if (child.Page == 0 || child.Page == c.ActivePage) HitTest(child, x, y, ref res);
        }
    }
}
