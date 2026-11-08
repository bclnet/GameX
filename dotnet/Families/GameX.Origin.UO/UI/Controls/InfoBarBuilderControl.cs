// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO InfoBarBuilderControl. The line is TextBox, not StbTextBox. The variable list is not Combobox.
/// There is no scroll area, so the list is as tall as its rows. Delete stacks the parent the way
/// DataBox.ReArrangeChildren does. No profile file.
/// </summary>
public class InfoBarBuilderControl : Control {
    readonly TextBox infoLabel;
    readonly ClickableColorBox labelColor;
    readonly Combo varStat;
    readonly Gump _gump;

    public InfoBarBuilderControl(Gump gump, InfoBarItem item) {
        _gump = gump;
        infoLabel = new TextBox { X = 5, Y = 0, Width = 130, Height = 26, MaxLength = 10 };
        infoLabel.Text = item.label;
        var dataVars = InfoBarManager.GetVars();
        varStat = new Combo(200, 0, 170, dataVars, (int)item.var, _gump.World);
        labelColor = new ClickableColorBox(_gump.World, 150, 0, 13, 14, item.hue);
        var deleteButton = new DeleteButton(this);
        Add(new ResizePic(0x0BB8) { X = infoLabel.X - 5, Y = 0, Width = infoLabel.Width + 10, Height = infoLabel.Height });
        Add(infoLabel);
        Add(varStat);
        Add(labelColor);
        if (!deleteButton.IsDisposed) Add(deleteButton);
        Width = infoLabel.Width + 10;
        Height = infoLabel.Height;
    }

    public string LabelText => infoLabel.Text;
    public InfoBarVars Var => (InfoBarVars)varStat.SelectedIndex;
    public ushort Hue => labelColor.Hue;

    static void Stack(Control parent) {
        if (parent == null) return;
        var height = 0;
        for (var i = 0; i < parent.Children.Count; i++) {
            var c = parent.Children[i];
            if (c.IsVisible && !c.IsDisposed) {
                c.Y = height;
                height += c.Height;
            }
        }
        parent.WantUpdateSize = true;
    }

    sealed class DeleteButton : NiceButton {
        readonly InfoBarBuilderControl _row;

        public DeleteButton(InfoBarBuilderControl row) : base(390, 0, 60, 25, ButtonAction.Activate, "Delete") {
            _row = row;
            ButtonParameter = 999;
        }

        public override void OnButtonClick(int buttonID) {
            var parent = _row.Parent;
            base.OnButtonClick(buttonID);
            _row.Dispose();
            Stack(parent);
        }
    }

    /// <summary>ClassicUO Combobox for one info-bar variable. A click opens the names. No modal and no scroll.</summary>
    sealed class Combo : Control {
        readonly string[] _items;
        readonly Label _label;
        readonly World _world;
        int _selectedIndex;

        public Combo(int x, int y, int width, string[] items, int selected, World world) {
            X = x;
            Y = y;
            Width = width;
            Height = 25;
            _items = items;
            _world = world;
            _selectedIndex = selected;
            CanMove = false;
            AcceptMouseInput = true;
            var initial = "";
            if (_items != null && selected > -1 && selected < _items.Length) initial = _items[selected] ?? "";
            Add(new ResizePic(0x0BB8) { Width = width, Height = 25 });
            _label = new Label(initial, false, 0x0453, 9) { X = 2, Y = 5 };
            Add(_label);
            if (width > 18) {
                var arrow = new GumpPic(width - 18, 2, 0x00FC, 0) { AcceptMouseInput = false, CanMove = false };
                if (!arrow.IsDisposed) Add(arrow);
            }
        }

        public int SelectedIndex {
            get => _selectedIndex;
            set {
                _selectedIndex = value;
                if (_label != null && _items != null && value > -1 && value < _items.Length)
                    _label.Text = _items[value] ?? "";
            }
        }

        public override void OnMouseClick(int x, int y) {
            if (_items == null || _items.Length == 0) return;
            UIManager.GetGump<ListGump>()?.Dispose();
            UIManager.Add(new ListGump(this, _world, At(this, true), At(this, false), Width, _items));
        }

        static int At(Control c, bool horizontal) {
            var n = 0;
            for (var p = c; p != null; p = p.Parent) n += horizontal ? p.X : p.Y;
            return n;
        }

        sealed class ListGump : Gump {
            readonly Combo _combo;

            public ListGump(Combo combo, World world, int x, int y, int width, string[] items) : base(world, 0, 0) {
                _combo = combo;
                X = x;
                Y = y;
                CanMove = false;
                AcceptMouseInput = true;
                CanCloseWithRightClick = true;
                LayerOrder = UILayer.Over;
                WantUpdateSize = false;
                var rows = new Row[items.Length];
                var maxW = width;
                for (var i = 0; i < items.Length; i++) {
                    var text = items[i] ?? "";
                    var row = new Row(this, i, text) { X = 2, Y = i * 15, IsVisible = text.Length != 0 };
                    if (row.X + row.Width > maxW) maxW = row.X + row.Width;
                    rows[i] = row;
                }
                var total = rows.Length == 0 ? 15 : rows[rows.Length - 1].Y + rows[rows.Length - 1].Height;
                Width = maxW;
                Height = total;
                Add(new ResizePic(0x0BB8) { Width = maxW, Height = total, AcceptMouseInput = false });
                for (var i = 0; i < rows.Length; i++) {
                    rows[i].Width = maxW > rows[i].X ? maxW - rows[i].X : 1;
                    Add(rows[i]);
                }
            }

            public void Choose(int index) {
                _combo.SelectedIndex = index;
                Dispose();
            }

            sealed class Row : HoveredLabel {
                readonly ListGump _list;
                readonly int _index;

                public Row(ListGump list, int index, string text) : base(text, false, 0x0453, 0x0453, 0x0453, font: 9) {
                    _list = list;
                    _index = index;
                    DrawBackgroundCurrentIndex = true;
                }

                public override void OnMouseClick(int x, int y) => _list.Choose(_index);
            }
        }
    }
}
