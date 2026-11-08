// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO context menu. The item list and the click stay here.
/// There is no profile file. A click outside disposes the menu.
/// BGRA1555 has no partial alpha, so the 0.7 black fill is an opaque dark pixel.
/// </summary>
public class ContextMenuControl {
    static ContextMenuShowMenu _open;
    readonly List<ContextMenuItemEntry> _items = new List<ContextMenuItemEntry>();
    readonly Gump _gump;

    public ContextMenuControl(Gump gump) {
        _gump = gump;
    }

    public void Add(string text, Action action, bool canBeSelected = false, bool defaultValue = false) {
        _items.Add(new ContextMenuItemEntry(text, action, canBeSelected, defaultValue));
    }

    public void Add(ContextMenuItemEntry entry) {
        _items.Add(entry);
    }

    public void Add(string text, List<ContextMenuItemEntry> entries) {
        _items.Add(new ContextMenuItemEntry(text) { Items = entries ?? new List<ContextMenuItemEntry>() });
    }

    public void Show() {
        _open?.Dispose();
        _open = null;
        if (_items.Count == 0) return;
        _open = new ContextMenuShowMenu(_gump.World, _items);
        UIManager.Add(_open);
    }

    public void Dispose() {
        _open?.Dispose();
        _open = null;
        _items.Clear();
    }
}

/// <summary>One row, or a row that opens a nested list.</summary>
public class ContextMenuItemEntry {
    public ContextMenuItemEntry(string text, Action action = null, bool canBeSelected = false, bool defaultValue = false) {
        Text = text ?? "";
        Action = action;
        CanBeSelected = canBeSelected;
        IsSelected = defaultValue;
        Items = new List<ContextMenuItemEntry>();
    }

    public readonly Action Action;
    public readonly bool CanBeSelected;
    public bool IsSelected;
    public List<ContextMenuItemEntry> Items;
    public readonly string Text;

    public void Add(ContextMenuItemEntry subEntry) {
        Items.Add(subEntry);
    }
}

/// <summary>The open menu. Submenus are children, placed to the right of the row.</summary>
public class ContextMenuShowMenu : Gump {
    const int FillKey = 0x57200021;
    const int GrayKey = 0x57200022;

    // r = g = b = 4, opaque. Stand-in for AlphaBlendControl(0.7).
    static readonly byte[] DarkPx = { 0x84, 0x90 };
    // r = g = b = 15, opaque. Stand-in for Color.Gray.
    static readonly byte[] GrayPx = { 0xEF, 0xBD };

    readonly List<ContextMenuItem> _rows = new List<ContextMenuItem>();
    readonly List<ContextMenuShowMenu> _subMenus = new List<ContextMenuShowMenu>();
    readonly int _bgW, _bgH;
    bool _leftDown;

    public ContextMenuShowMenu(World world, List<ContextMenuItemEntry> list) : base(world, 0, 0) {
        WantUpdateSize = false;
        LayerOrder = UILayer.Over;
        CanMove = false;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        int bgW = 0, bgH = 0, y = 0;
        var entries = list ?? new List<ContextMenuItemEntry>();
        for (int i = 0; i < entries.Count; i++) {
            var item = new ContextMenuItem(this, entries[i]);
            if (i > 0) item.Y = y;
            if (bgW < item.Width) bgW = item.Width;
            bgH += item.Height;
            Add(item);
            _rows.Add(item);
            y += item.Height;
        }
        _bgW = bgW;
        _bgH = bgH;
        Width = bgW;
        Height = bgH;
        for (int i = 0; i < _rows.Count; i++)
            if (_rows[i].Width < bgW) _rows[i].Width = bgW;
        X = Mouse.Position.X + 5;
        Y = Mouse.Position.Y - 20;
        int bw = 640, bh = 480;
        if (Mouse.HasSample) {
            if (Mouse.Current.BufW > 0) bw = Mouse.Current.BufW;
            if (Mouse.Current.BufH > 0) bh = Mouse.Current.BufH;
        }
        if (X + bgW > bw) X = bw - bgW;
        if (Y + bgH > bh) Y = bh - bgH;
        _leftDown = Mouse.LButtonPressed;
    }

    public override void Update() {
        base.Update();
        if (Parent != null) return;
        bool down = Mouse.LButtonPressed;
        if (down && !_leftDown && !HitInside()) Dispose();
        _leftDown = down;
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (_bgW > 0 && _bgH > 0) {
            var sink = GumpArt.Sink;
            sink?.DrawRaw(GrayKey, x - 1, y - 1, _bgW + 2, _bgH + 2, GrayPx, 1, 1);
            sink?.DrawRaw(FillKey, x, y, _bgW, _bgH, DarkPx, 1, 1);
        }
        return base.Draw(x, y);
    }

    /// <summary>A submenu is the nearest gump. Close the root.</summary>
    public override void CloseWithRightClick() {
        if (Parent is ContextMenuShowMenu parent) {
            parent.CloseWithRightClick();
            return;
        }
        base.CloseWithRightClick();
    }

    bool HitInside() {
        for (var p = UIManager.MouseOverControl; p != null; p = p.Parent)
            if (p == this) return true;
        return false;
    }

    sealed class ContextMenuItem : Control {
        readonly ContextMenuShowMenu _menu;
        readonly ContextMenuItemEntry _entry;
        readonly Label _label;
        readonly GumpPic _selectedPic;
        readonly ContextMenuShowMenu _subMenu;

        public ContextMenuItem(ContextMenuShowMenu parent, ContextMenuItemEntry entry) {
            _menu = parent;
            _entry = entry ?? new ContextMenuItemEntry("");
            CanCloseWithRightClick = false;
            CanMove = false;
            AcceptMouseInput = true;
            WantUpdateSize = false;
            _label = new Label(_entry.Text, true, 0xFFFF, 0) { X = 25 };
            Add(_label);
            Height = 25;
            _label.Y = (Height >> 1) - (_label.Height >> 1);
            _selectedPic = null;
            if (_entry.CanBeSelected) {
                var pic = new GumpPic(3, 0, 0x0838, 0) { IsVisible = _entry.IsSelected, IsEnabled = false };
                if (!pic.IsDisposed) {
                    pic.Y = (Height >> 1) - (pic.Height >> 1);
                    _selectedPic = pic;
                    Add(pic);
                }
            }
            Width = _label.X + _label.Width + 20;
            if (Width < 100) Width = 100;
            _subMenu = null;
            if (_entry.Items != null && _entry.Items.Count != 0) {
                _subMenu = new ContextMenuShowMenu(parent.World, _entry.Items) { IsVisible = false };
                parent.Add(_subMenu);
                parent._subMenus.Add(_subMenu);
            }
        }

        public override void Update() {
            base.Update();
            if (Width > _label.Width) _label.Width = Width;
            if (_subMenu == null || _subMenu.IsDisposed) return;
            _subMenu.X = Width;
            _subMenu.Y = Y;
            bool on = UIManager.MouseOverControl == this;
            if (!on) {
                for (var p = UIManager.MouseOverControl; p != null; p = p.Parent)
                    if (p == _subMenu) { on = true; break; }
            }
            _subMenu.IsVisible = on;
        }

        public override void OnMouseClick(int x, int y) {
            _entry.Action?.Invoke();
            Control root = this;
            while (root.Parent != null) root = root.Parent;
            root.Dispose();
            if (_entry.CanBeSelected) {
                _entry.IsSelected = !_entry.IsSelected;
                if (_selectedPic != null) _selectedPic.IsVisible = _entry.IsSelected;
            }
            Mouse.CancelDoubleClick = true;
            Mouse.LastLeftButtonClickTime = 0;
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            if (!string.IsNullOrWhiteSpace(_label.Text) && UIManager.MouseOverControl == this && Width > 4 && Height > 10)
                GumpArt.Sink?.DrawRaw(GrayKey, x + 2, y + 5, Width - 4, Height - 10, GrayPx, 1, 1);
            bool drawn = base.Draw(x, y);
            if (_subMenu != null) {
                const string more = ">";
                int mw = GumpArt.Sink?.MeasureText(more, 1) ?? more.Length * 6;
                GumpArt.Sink?.DrawText(x + Width - mw - 4, y + (Height >> 1) - 8, more, 0xFFFF, 1);
            }
            return drawn;
        }
    }
}
