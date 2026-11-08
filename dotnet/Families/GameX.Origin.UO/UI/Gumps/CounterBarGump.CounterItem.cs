// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using OpenX.Client;
using System;
using System.IO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO CounterItem. Item art is the same ArtOf path as the bar: it is not a gump graphic.
/// UIManager does not dispatch drag or a context menu. A held left button that moves four pixels lifts the cell.
/// CastSpellsByOneClick, abbreviated counts, and the low-amount red border stay off. Those profile flags default that way.
/// </summary>
public sealed partial class CounterBarGump {
    const int DragOffset = 22;
    const int DragStart = 4;
    const int MenuKey = 0x57200031;

    /// <summary>ClassicUO ties this to the border. This bar has no border toggle, so the flag stands alone.</summary>
    public bool ReadOnly { get; set; }

    void SetupLayout() => Layout();

    void ConfigureContextMenu(CounterMenu menu) {
        menu.Line("Add", AddPlaceholder);
        menu.Line(ReadOnly ? "Read-only mode (ON)" : "Read-only mode (OFF)", ToggleReadOnly);
    }

    void AddPlaceholder() {
        Add(new CounterItem(this, 0, 0, 0));
        SetupLayout();
    }

    void ToggleReadOnly() {
        ReadOnly = !ReadOnly;
        for (var i = 0; i < Children.Count; i++)
            if (Children[i] is CounterItem item) item.ConfigureContextMenu();
    }

    void AddCounter(CounterItem item) {
        item.Parent = this;
        item.MovedTo(this);
        SetupLayout();
    }

    void InsertCounter(CounterItem item, CounterItem before) {
        item.Parent = this;
        var kids = Children;
        kids.Remove(item);
        var index = kids.IndexOf(before);
        if (index < 0) kids.Add(item);
        else kids.Insert(index, item);
        item.MovedTo(this);
        SetupLayout();
    }

    public sealed class CounterItem : Control {
        const int UpdateMs = 100;
        const int HiMs = 5000;
        const int UpHue = 1165;
        const int DownHue = 1166;
        const int UpKey = 0x57200021;
        const int DownKey = 0x57200022;

        static byte[] _upPx;
        static byte[] _downPx;
        static Binary_TileData.ItemData[] _tiles;
        static bool _tilesTried;

        readonly ImageWithText _image;
        CounterBarGump _gump;
        CounterMenu _menu;
        int _amount;
        uint _time;
        uint _hiTime;
        int _hi;

        public CounterItem(CounterBarGump gump, ushort graphic, ushort? hue, int compareTo) {
            _gump = gump;
            CompareTo = compareTo;
            AcceptMouseInput = true;
            WantUpdateSize = false;
            CanMove = true;
            CanCloseWithRightClick = false;
            _image = new ImageWithText();
            Add(_image);
            SetGraphic(graphic, hue);
        }

        public ushort Graphic { get; private set; }
        public ushort? Hue { get; private set; }
        public int CompareTo { get; private set; }

        public void SetGraphic(ushort graphic, ushort? hue) {
            ushort draw = hue ?? 0;
            if (graphic != 0 && Partial(graphic)) draw = (ushort)(draw | 0x8000);
            _image.ChangeGraphic(graphic, draw);
            Graphic = graphic;
            Hue = hue;
            ConfigureContextMenu();
        }

        internal void MovedTo(CounterBarGump bar) {
            _gump = bar;
            ConfigureContextMenu();
        }

        public void ConfigureContextMenu() {
            if (_menu != null && !_menu.IsDisposed) _menu.Dispose();
            _menu = new CounterMenu(_gump.World);
            if (Graphic != 0) {
                _menu.Line("Use object (Double click)", Use);
                _menu.Line("Compare amount to...", CompareToSelected);
                _menu.Line(Hue != null ? "Compare Hue (ON)" : "Compare Hue (OFF)", ToggleIgnoreHue);
            }
            else _gump.ConfigureContextMenu(_menu);
        }

        /// <summary>The host does not deliver a right click to the cell. A caller opens the menu.</summary>
        public void ShowMenu() {
            if (IsDisposed) return;
            if (_menu == null || _menu.IsDisposed) ConfigureContextMenu();
            _menu.Open(Mouse.Position.X, Mouse.Position.Y);
        }

        void ToggleIgnoreHue() {
            Hue = Hue != null ? null : (ushort?)0;
            SetGraphic(Graphic, Hue);
        }

        void CompareToSelected() {
            UIManager.Add(new CompareDialog(_gump.World,
                "Compare to value\n(leave empty to reset)",
                CompareToDialogClosed, _amount.ToString()));
        }

        void CompareToDialogClosed(string newValue) {
            if (string.IsNullOrEmpty(newValue)) CompareTo = 0;
            else if (int.TryParse(newValue, out var parsed)) CompareTo = parsed;
            else UIManager.Add(new CompareDialog(_gump.World,
                "Compare to value\n(leave empty to reset)\n\nThe value must be a valid number!",
                CompareToDialogClosed, newValue));
        }

        public void RemoveItem() {
            _image.ChangeGraphic(0, 0);
            _amount = 0;
            Graphic = 0;
            Dispose();
            if (RootOf(this) is CounterBarGump bar) bar.SetupLayout();
        }

        public void Use() {
            if (Graphic == 0) return;
            var serial = FindSerial(Graphic, Hue ?? 0xFFFF);
            if (serial != 0) UOLoginNet.Use(serial);
        }

        public override void Update() {
            if (!IsDisposed) {
                _image.Width = Width;
                _image.Height = Height;
            }
            base.Update();
            TryLift();
            if (Parent == null || !Parent.IsEnabled || _time >= GlobalTime.Ticks) return;
            _time = GlobalTime.Ticks + UpdateMs;
            if (Graphic == 0) {
                _image.SetAmount("");
                return;
            }
            var previous = _amount;
            _amount = Total(Graphic, Hue);
            if (_amount > previous) {
                _hi = UpHue;
                _hiTime = GlobalTime.Ticks;
            }
            else if (_amount < previous) {
                _hi = DownHue;
                _hiTime = GlobalTime.Ticks;
            }
            _image.SetAmount(DisplayText());
        }

        public override void OnMouseClick(int x, int y) {
            if (!UOLoginNet.Holding || !TryHeld(out var graphic, out var hue, out var px, out var py, out var container)) return;
            if (!_gump.ReadOnly && graphic != 0) SetGraphic(graphic, hue);
            UOLoginNet.DropHeld(px, py, 0, container);
        }

        public override bool OnMouseDoubleClick(int x, int y) {
            Use();
            return true;
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            var sink = GumpArt.Sink;
            if (sink != null && _hi != 0 && GlobalTime.Ticks - _hiTime < HiMs && Width > 0 && Height > 0) {
                byte[] px;
                int key;
                if (_hi == UpHue) {
                    px = Dot(UpHue, ref _upPx, 0xD31C);
                    key = UpKey;
                }
                else {
                    px = Dot(DownHue, ref _downPx, 0xF104);
                    key = DownKey;
                }
                sink.DrawRaw(key, x, y, Width, Height, px, 1, 1);
            }
            var drew = base.Draw(x, y);
            if (sink != null) {
                var hot = UIManager.MouseOverControl == this;
                Frame(sink, hot ? YellowKey : GrayKey, hot ? YellowPx : GrayPx, x, y, Width, Height);
            }
            return drew;
        }

        public override void Dispose() {
            if (IsDisposed) return;
            if (_menu != null && !_menu.IsDisposed) _menu.Dispose();
            _menu = null;
            base.Dispose();
        }

        internal void FinishDrag() {
            if (Parent is not DraggableGump old) return;
            if (_gump.ReadOnly) return;
            var hits = BarHits();
            if (hits.Count == 0) {
                old.Dispose();
                return;
            }
            var hit = hits[0];
            if (RootOf(hit) is not CounterBarGump bar) {
                old.Dispose();
                return;
            }
            if (hit is CounterItem at && at != this) bar.InsertCounter(this, at);
            else bar.AddCounter(this);
            if (!old.IsDisposed) old.Dispose();
        }

        void TryLift() {
            if (IsDisposed || _gump.ReadOnly || Parent is DraggableGump || UOLoginNet.Holding) return;
            if (!Mouse.LButtonPressed || UIManager.LastControlMouseDown(1) != this) return;
            var d = Mouse.LDragOffset;
            if (d.X > -DragStart && d.X < DragStart && d.Y > -DragStart && d.Y < DragStart) return;
            var w = Width > 0 ? Width : Cell - Pad * 2;
            var h = Height > 0 ? Height : Cell - Pad * 2;
            var host = new DraggableGump(_gump.World) {
                X = Mouse.LClickPosition.X - DragOffset,
                Y = Mouse.LClickPosition.Y - DragOffset,
                Width = w,
                Height = h,
            };
            host.Add(this);
            X = 0;
            Y = 0;
            Width = w;
            Height = h;
            UIManager.Add(host);
            _gump.SetupLayout();
        }

        // A lone 1 is blank. ± is the equal mark. Abbreviated counts stay off (profile default).
        string DisplayText() {
            var n = _amount - CompareTo;
            var prefix = "";
            if (CompareTo != 0) {
                if (n == 0) prefix = "\u00B1";
                else if (n > 0) prefix = "+";
            }
            if (prefix.Length == 0 && n == 1) return "";
            return prefix + n.ToString();
        }

        static bool Partial(ushort graphic) {
            if (!_tilesTried) {
                _tilesTried = true;
                var root = UltimaOnline.Current?.ClientPath;
                if (!string.IsNullOrEmpty(root)) {
                    var tile = Path.Combine(root, "tiledata.mul");
                    if (File.Exists(tile)) {
                        using var fs = File.OpenRead(tile);
                        using var br = new BinaryReader(fs);
                        _tiles = new Binary_TileData(br).ItemDatas;
                    }
                }
            }
            return _tiles != null && graphic < _tiles.Length && _tiles[graphic].IsPartialHue;
        }

        static byte[] Dot(int hue, ref byte[] slot, ushort fallback) {
            if (slot != null) return slot;
            var c = fallback;
            var assets = UltimaOnline.Current?.Assets;
            if (assets != null && assets.TryHueRamp(hue, out var ramp) && ramp != null && ramp.Length > 0) {
                var i = ramp.Length > 16 ? 16 : ramp.Length - 1;
                if (ramp[i] != 0) c = (ushort)(ramp[i] | 0x8000);
            }
            slot = new byte[] { (byte)c, (byte)(c >> 8) };
            return slot;
        }

        sealed class ImageWithText : Control {
            readonly Label _label;
            ushort _graphic;
            ushort _hue;

            public ImageWithText() {
                CanMove = true;
                WantUpdateSize = false;
                AcceptMouseInput = false;
                _label = new Label("", true, 0x35, 1) { X = 2, AcceptMouseInput = false };
                Add(_label);
            }

            public void ChangeGraphic(ushort graphic, ushort hue) {
                if (graphic != 0) {
                    _graphic = graphic;
                    _hue = hue;
                }
                else _graphic = 0;
            }

            public override void Update() {
                base.Update();
                if (Parent == null) return;
                Width = Parent.Width;
                Height = Parent.Height;
                _label.Y = Parent.Height > 15 ? Parent.Height - 15 : 0;
            }

            public override bool Draw(int x, int y) {
                if (IsDisposed || !IsVisible) return false;
                if (_graphic != 0) {
                    var sink = GumpArt.Sink;
                    if (sink != null && ArtOf(_graphic, _hue, out var key, out var px, out var w, out var h))
                        Blit(sink, key, px, w, h, x, y, Width, Height);
                }
                return base.Draw(x, y);
            }

            public void SetAmount(string amount) {
                if (_label.Text != amount) _label.Text = amount;
            }
        }
    }

    /// <summary>ClassicUO EntryDialog for the compare value. OK and Enter apply it. No packet.</summary>
    sealed class CompareDialog : Gump {
        readonly Action<string> _action;
        readonly TextBox _box;

        public CompareDialog(World world, string message, Action<string> action, string initial) : base(world, 0, 0) {
            _action = action;
            CanMove = false;
            CanCloseWithRightClick = false;
            CanCloseWithEsc = false;
            AcceptMouseInput = true;
            WantUpdateSize = false;
            LayerOrder = UILayer.Over;
            Width = 250;
            var lines = (message ?? "").Split('\n');
            var boxY = 45 + lines.Length * 16 + 7;
            Height = boxY + 80;
            if (Height < 160) Height = 160;
            var sample = Mouse.Current;
            var bw = sample.BufW > 0 ? sample.BufW : 640;
            var bh = sample.BufH > 0 ? sample.BufH : 480;
            X = (bw - Width) >> 1;
            Y = (bh - Height) >> 1;
            Add(new ResizePic(0x0A28) { Width = Width, Height = Height, AcceptMouseInput = false, CanMove = false });
            for (var i = 0; i < lines.Length; i++)
                Add(new Label(lines[i], false, 0x0386, 1) { X = 40, Y = 45 + i * 16, AcceptMouseInput = false });
            Add(new ResizePic(0x0BB8) { X = 40, Y = boxY - 2, Width = Width - 90, Height = 25, AcceptMouseInput = false, CanMove = false });
            _box = new TextBox { X = 42, Y = boxY, Width = Width - 94, Height = 25, Hue = 0x0481, MaxLength = 12 };
            _box.Text = initial ?? "";
            Add(_box);
            var ok = new Button(0, 0x0481, 0x0482, 0x0483) { ButtonAction = ButtonAction.Activate };
            if (!ok.IsDisposed) {
                ok.X = (Width - ok.Width) >> 1;
                ok.Y = Height - 45;
                Add(ok);
            }
            _box.SetKeyboardFocus();
        }

        public override void OnButtonClick(int buttonID) {
            if (buttonID != 0 || IsDisposed) return;
            _action?.Invoke(_box.Text);
            Dispose();
        }

        public override void OnKeyboardReturn(int textID, string text) {
            if (IsDisposed) return;
            _action?.Invoke(_box.Text);
            Dispose();
        }
    }

    sealed class CounterMenu : Gump {
        int _rows;

        public CounterMenu(World world) : base(world, 0, 0) {
            CanCloseWithRightClick = true;
            CanMove = false;
            AcceptMouseInput = true;
            WantUpdateSize = false;
            LayerOrder = UILayer.Over;
            Width = 40;
            Height = 8;
        }

        public void Line(string text, Action action) {
            var row = new MenuRow(text, action, this) { X = 8, Y = 6 + _rows * 16 };
            Add(row);
            _rows++;
            var w = row.Width + 16;
            if (w > Width) Width = w;
            Height = 8 + _rows * 16;
        }

        public void Open(int x, int y) {
            if (IsDisposed) return;
            X = x;
            Y = y;
            UIManager.Gumps.Remove(this);
            UIManager.Add(this);
        }

        public override bool Draw(int x, int y) {
            if (IsDisposed || !IsVisible) return false;
            var sink = GumpArt.Sink;
            if (sink != null && Width > 0 && Height > 0) {
                sink.DrawRaw(MenuKey, x, y, Width, Height, Dark, 1, 1);
                Frame(sink, GrayKey, GrayPx, x, y, Width, Height);
            }
            return base.Draw(x, y);
        }

        sealed class MenuRow : Label {
            readonly Action _action;
            readonly CounterMenu _menu;

            public MenuRow(string text, Action action, CounterMenu menu) : base(text, true, 0x0481, 1) {
                _action = action;
                _menu = menu;
                AcceptMouseInput = true;
            }

            public override void OnMouseClick(int x, int y) {
                var act = _action;
                _menu.Dispose();
                act?.Invoke();
            }
        }
    }
}
