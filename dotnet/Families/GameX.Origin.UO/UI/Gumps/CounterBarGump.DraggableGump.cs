// SPDX-License-Identifier: BSD-2-Clause

using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO DraggableGump. UIManager does not call OnDragBegin or OnDragEnd.
/// Update follows the pointer while the left button is down, then the cell drops.
/// </summary>
public sealed partial class CounterBarGump {
    public sealed class DraggableGump : Gump {
        bool _ended;

        public DraggableGump(World world) : base(world, 0, 0) {
            CanMove = true;
            CanCloseWithRightClick = false;
            AcceptMouseInput = true;
            WantUpdateSize = false;
        }

        public override void Update() {
            if (Mouse.LButtonPressed) {
                X = Mouse.Position.X - DragOffset;
                Y = Mouse.Position.Y - DragOffset;
            }
            else EndDrag();
            base.Update();
        }

        void EndDrag() {
            if (_ended) return;
            _ended = true;
            if (Children.Count > 0 && Children[0] is CounterItem item)
                item.FinishDrag();
        }
    }

    static List<Control> BarHits() {
        var list = new List<Control>();
        for (var n = UIManager.Gumps.First; n != null; n = n.Next) {
            if (n.Value is not CounterBarGump bar || bar.IsDisposed || !bar.IsVisible || !bar.IsEnabled) continue;
            var hit = HitIn(bar);
            if (hit != null) list.Add(hit);
        }
        return list;
    }

    static Control HitIn(CounterBarGump bar) {
        var mx = UIManager.MouseX;
        var my = UIManager.MouseY;
        var kids = bar.Children;
        for (var i = kids.Count - 1; i >= 0; i--) {
            var c = kids[i];
            if (c.IsDisposed || !c.IsVisible || !c.AcceptMouseInput) continue;
            var sx = bar.X + c.X;
            var sy = bar.Y + c.Y;
            if (mx >= sx && my >= sy && mx < sx + c.Width && my < sy + c.Height) return c;
        }
        if (bar.AcceptMouseInput && bar.IsEnabled && mx >= bar.X && my >= bar.Y && mx < bar.X + bar.Width && my < bar.Y + bar.Height)
            return bar;
        return null;
    }

    static Control RootOf(Control c) {
        if (c == null) return null;
        while (c.Parent != null) c = c.Parent;
        return c;
    }
}
