// SPDX-License-Identifier: BSD-2-Clause

using OpenX.Client;
using System;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO Combobox. Language is ENU, so the label uses the requested font rather than the CHT/KOR/JPN font.
/// A left click opens the list. The list is modal: a press that is not on it, or on this box, closes it.
/// Rows taller than the view scroll. UIManager does not block other gumps, so IsModal is only stored.
/// </summary>
public class Combobox : Control {
    const int ELEMENT_HEIGHT = 15;
    readonly byte _font;
    readonly string[] _items;
    readonly Label _label;
    readonly int _maxHeight;
    int _selectedIndex;
    ComboboxGump _menu;

    public Combobox(
        int x,
        int y,
        int width,
        string[] items,
        int selected = -1,
        int maxHeight = 200,
        bool showArrow = true,
        string emptyString = "",
        byte font = 9
    ) {
        X = x;
        Y = y;
        Width = width;
        Height = 25;
        // SelectedIndex before _items is assigned, so the label is not written and the event does not run.
        SelectedIndex = selected;
        _font = font;
        _items = items;
        _maxHeight = maxHeight;
        WantUpdateSize = false;
        AcceptMouseInput = true;
        CanMove = false;

        Add(new ResizePic(0x0BB8) {
            Width = width,
            Height = Height,
            AcceptMouseInput = false,
            CanMove = false,
        });

        string initialText = selected > -1 ? items[selected] : emptyString;
        Add(_label = new Label(initialText, false, 0x0453, _font) { X = 2, Y = 5 });

        if (showArrow)
            Add(new GumpPic(width - 18, 2, 0x00FC, 0) { AcceptMouseInput = false, CanMove = false });
    }

    public int SelectedIndex {
        get => _selectedIndex;
        set {
            _selectedIndex = value;
            if (_items != null) {
                _label.Text = _items[value];
                OnOptionSelected?.Invoke(this, value);
            }
        }
    }

    public event EventHandler<int> OnOptionSelected;

    public override void OnMouseClick(int x, int y) => OnMouseUp(x, y, MouseButtonType.Left);

    protected void OnMouseUp(int x, int y, MouseButtonType button) {
        if (button != MouseButtonType.Left || _items == null) return;
        if (_menu != null && !_menu.IsDisposed) {
            _menu.Dispose();
            return;
        }
        int comboY = At(this, false);
        int limit = Mouse.HasSample ? Mouse.Current.BufH : 0;
        if (comboY < 0) comboY = 0;
        else if (limit > 0 && comboY + _maxHeight > limit) comboY = limit - _maxHeight;
        _menu = new ComboboxGump(WorldOf(this), At(this, true), comboY, Width, _maxHeight, _items, _font, this);
        UIManager.Add(_menu);
    }

    static World WorldOf(Control c) {
        World world = null;
        for (var p = c.Parent; p != null; p = p.Parent)
            if (p is Gump g) world = g.World;
        return world;
    }

    static int At(Control c, bool horizontal) {
        int n = 0;
        for (var p = c; p != null; p = p.Parent) n += horizontal ? p.X : p.Y;
        return n;
    }

    sealed class ComboboxGump : Gump {
        const int SCROLL_STEP = 50;
        readonly Combobox _combobox;
        readonly OptionRow[] _rows;
        readonly Button _up, _down;
        readonly GumpPic _thumb;
        readonly int _content;
        int _scroll, _maxScroll, _wheel;
        uint _barTick;
        bool _barHeld, _wheelReady, _ignorePress;

        public ComboboxGump(World world, int x, int y, int width, int maxHeight, string[] items, byte font, Combobox combobox) : base(world, 0, 0) {
            _combobox = combobox;
            _ignorePress = Mouse.LButtonPressed;
            CanMove = false;
            AcceptMouseInput = true;
            X = x;
            Y = y;
            IsModal = true;
            LayerOrder = UILayer.Over;
            ModalClickOutsideAreaClosesThisControl = true;
            WantUpdateSize = false;

            _rows = new OptionRow[items.Length];
            int maxWidth = width;
            int bottom = 0;
            for (int i = 0; i < items.Length; i++) {
                var row = new OptionRow(this, i, items[i], font);
                _rows[i] = row;
                int right = row.X + row.Width;
                int low = row.Y + row.Height;
                if (right > maxWidth) maxWidth = right;
                if (low > bottom) bottom = low;
            }
            int totalHeight = Math.Min(maxHeight, bottom);
            _content = bottom;
            _maxScroll = Math.Max(0, _content - totalHeight);
            Width = maxWidth + 15;
            Height = totalHeight;

            Add(new ResizePic(0x0BB8) {
                Width = maxWidth,
                Height = totalHeight,
                AcceptMouseInput = false,
                CanMove = false,
            });
            for (int i = 0; i < _rows.Length; i++) {
                _rows[i].Width = maxWidth;
                Add(_rows[i]);
            }

            _up = new Button(1, 251, 250) { X = Width - 14, ButtonAction = ButtonAction.Activate };
            _down = new Button(2, 253, 252) { X = Width - 14, ButtonAction = ButtonAction.Activate };
            Add(_up);
            Add(_down);
            _thumb = new GumpPic(Width - 14, 0, 254, 0) { CanMove = false };
            Add(_thumb);
            Place();
        }

        public bool IsModal { get; set; }
        public bool ModalClickOutsideAreaClosesThisControl { get; set; }

        public override void Update() {
            if (IsDisposed) return;
            if (_ignorePress) {
                if (!Mouse.LButtonPressed) _ignorePress = false;
            }
            else if (ModalClickOutsideAreaClosesThisControl && Mouse.LButtonPressed && !Owns(UIManager.LastControlMouseDown(1))) {
                Dispose();
                return;
            }
            HoldScroll();
            DragThumb();
            Wheel();
            base.Update();
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            if (_maxScroll > 0) {
                int top = _up != null && !_up.IsDisposed ? _up.Height : 0;
                int bot = _down != null && !_down.IsDisposed ? _down.Height : 0;
                int h = Height - top - bot;
                if (h > 0) GumpArtDraw.Pic(256, x + Width - 14, y + top, 0, h, true);
            }
            return base.Draw(x, y);
        }

        public void Choose(int index) {
            if (!_combobox.IsDisposed) _combobox.SelectedIndex = index;
            Dispose();
        }

        void HoldScroll() {
            var held = Mouse.LButtonPressed ? UIManager.LastControlMouseDown(1) : null;
            bool up = held == _up;
            bool down = held == _down;
            if (!up && !down) { _barHeld = false; return; }
            if (!_barHeld || GlobalTime.Ticks > _barTick) {
                bool first = !_barHeld;
                _barHeld = true;
                _barTick = GlobalTime.Ticks + (first ? 500u : 250u);
                SetScroll(_scroll + (down ? SCROLL_STEP : -SCROLL_STEP));
            }
        }

        void DragThumb() {
            if (_thumb == null || _thumb.IsDisposed || _maxScroll < 1 || !Mouse.LButtonPressed) return;
            if (UIManager.LastControlMouseDown(1) != _thumb) return;
            int top = _up != null && !_up.IsDisposed ? _up.Height : 0;
            int bot = _down != null && !_down.IsDisposed ? _down.Height : 0;
            int track = Height - top - bot - _thumb.Height;
            if (track < 1) return;
            int y = Mouse.Position.Y - At(this, false) - top - _thumb.Height / 2;
            if (y < 0) y = 0;
            if (y > track) y = track;
            SetScroll((int)(y / (float)track * _maxScroll));
        }

        void Wheel() {
            if (!Owns(UIManager.MouseOverControl) || !Mouse.HasSample) return;
            int w = Mouse.Current.Wheel;
            if (!_wheelReady) { _wheel = w; _wheelReady = true; return; }
            while (w >= _wheel + 120) { _wheel += 120; SetScroll(_scroll - SCROLL_STEP); }
            while (w <= _wheel - 120) { _wheel -= 120; SetScroll(_scroll + SCROLL_STEP); }
        }

        void SetScroll(int value) {
            if (value < 0) value = 0;
            if (value > _maxScroll) value = _maxScroll;
            if (_scroll == value) return;
            _scroll = value;
            Place();
        }

        void Place() {
            for (int i = 0; i < _rows.Length; i++) {
                var row = _rows[i];
                row.Y = i * ELEMENT_HEIGHT - _scroll;
                row.IsVisible = row.HasText && row.Y >= 0 && row.Y < Height;
            }
            bool bar = _maxScroll > 0;
            if (_up != null && !_up.IsDisposed) _up.IsVisible = bar;
            if (_down != null && !_down.IsDisposed) {
                _down.IsVisible = bar;
                _down.Y = _down.Height > 0 ? Math.Max(0, Height - _down.Height) : Height;
            }
            if (_thumb != null && !_thumb.IsDisposed) {
                _thumb.IsVisible = bar;
                int top = _up != null && !_up.IsDisposed ? _up.Height : 0;
                int bot = _down != null && !_down.IsDisposed ? _down.Height : 0;
                int track = Height - top - bot - _thumb.Height;
                int y = top;
                if (track > 0 && _maxScroll > 0) y += (int)(_scroll / (float)_maxScroll * track);
                _thumb.Y = y;
            }
        }

        bool Owns(Control c) {
            for (; c != null; c = c.Parent)
                if (c == this || c == _combobox) return true;
            return false;
        }

        static int At(Control c, bool horizontal) {
            int n = 0;
            for (var p = c; p != null; p = p.Parent) n += horizontal ? p.X : p.Y;
            return n;
        }
    }

    sealed class OptionRow : HoveredLabel {
        readonly ComboboxGump _menu;

        public OptionRow(ComboboxGump menu, int index, string item, byte font)
            : base(item ?? "", false, 0x0453, 0x0453, 0x0453, font: font) {
            _menu = menu;
            Index = index;
            HasText = !string.IsNullOrEmpty(item);
            X = 2;
            Y = index * ELEMENT_HEIGHT;
            IsVisible = HasText;
            DrawBackgroundCurrentIndex = true;
        }

        public int Index { get; }
        public bool HasText { get; }

        public override void OnMouseClick(int x, int y) => _menu.Choose(Index);
    }
}
