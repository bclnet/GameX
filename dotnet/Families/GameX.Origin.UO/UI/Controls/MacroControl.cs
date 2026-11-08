// SPDX-License-Identifier: BSD-2-Clause

using System;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO MacroControl. Lists this macro's MacroObject chain.
/// World has no MacroManager, so the list is not loaded from a profile and a duplicate key is not rejected.
/// Combobox is not in this project: a left click cycles the type or subtype. Shift cycles backward.
/// MacroButtonGump and the settings page are not ported. No profile write.
/// </summary>
public class MacroControl : Control {
    const int ModShift = 0x0003;
    const int ModCtrl = 0x00C0;
    const int ModAlt = 0x0300;

    static readonly string[] TypeNames = Enum.GetNames(typeof(MacroType));
    static readonly string[] SubNames = Enum.GetNames(typeof(MacroSubType));

    readonly HotkeyBox _hotkey;
    readonly MacroRows _rows;

    enum Buttons { AddBtn, RemoveBtn, CreateNewMacro, OpenMacroOptions }

    public MacroControl(Gump gump, string name, bool isFastAssign = false) {
        CanMove = true;
        Owner = gump;
        _hotkey = new HotkeyBox();
        _hotkey.HotkeyChanged += OnHotkeyChanged;
        _hotkey.HotkeyCancelled += OnHotkeyCancelled;
        Add(_hotkey);
        Add(new TextButton((int)Buttons.CreateNewMacro, 0, _hotkey.Height + 3, 170, 25, "+ Create macro button"));
        if (!isFastAssign) {
            Add(new TextButton((int)Buttons.AddBtn, 0, _hotkey.Height + 30, 50, 25, "Add"));
            Add(new TextButton((int)Buttons.RemoveBtn, 52, _hotkey.Height + 30, 50, 25, "Remove"));
        }
        else Add(new TextButton((int)Buttons.OpenMacroOptions, 0, _hotkey.Height + 30, 170, 25, "Open Macro Settings"));
        _rows = new MacroRows {
            X = 10,
            Y = _hotkey.Y + _hotkey.Height + 80,
            Width = isFastAssign ? 230 : 280
        };
        Add(_rows);
        Macro = Macro.CreateEmptyMacro(name ?? "");
        SetupKey();
        SetupRows();
    }

    public Gump Owner { get; }
    public Macro Macro { get; }

    public override void OnButtonClick(int buttonID) {
        switch (buttonID) {
            case (int)Buttons.AddBtn:
                AddEmpty();
                break;
            case (int)Buttons.RemoveBtn:
                RemoveLast();
                break;
            case (int)Buttons.CreateNewMacro:
                // MacroButtonGump is not ported.
                break;
            case (int)Buttons.OpenMacroOptions:
                // The settings gump is not ported.
                break;
        }
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        return base.Draw(x, y);
    }

    void SetupKey() {
        if (Macro == null) return;
        var mod = 0;
        if (Macro.Alt) mod |= ModAlt;
        if (Macro.Shift) mod |= ModShift;
        if (Macro.Ctrl) mod |= ModCtrl;
        if (Macro.Key != 0) _hotkey.SetKey(Macro.Key, mod);
        else if (Macro.MouseButton != MouseButtonType.None) _hotkey.SetMouseButton(Macro.MouseButton, mod);
        else if (Macro.WheelScroll) _hotkey.SetMouseWheel(Macro.WheelUp, mod);
    }

    void OnHotkeyChanged(object sender, EventArgs e) {
        var shift = (_hotkey.Mod & ModShift) != 0;
        var alt = (_hotkey.Mod & ModAlt) != 0;
        var ctrl = (_hotkey.Mod & ModCtrl) != 0;
        if (_hotkey.Key == 0 && _hotkey.MouseButton == MouseButtonType.None && !_hotkey.WheelScroll) return;
        var m = Macro;
        m.Key = _hotkey.Key;
        m.MouseButton = _hotkey.MouseButton;
        m.WheelScroll = _hotkey.WheelScroll;
        m.WheelUp = _hotkey.WheelUp;
        m.Shift = shift;
        m.Alt = alt;
        m.Ctrl = ctrl;
    }

    void OnHotkeyCancelled(object sender, EventArgs e) {
        var m = Macro;
        m.Alt = m.Ctrl = m.Shift = false;
        m.Key = 0;
        m.MouseButton = MouseButtonType.None;
        m.WheelScroll = false;
    }

    void AddEmpty() {
        var ob = (MacroObject)Macro.Items;
        if (ob == null) {
            Macro.Items = Macro.Create(MacroType.None);
            SetupRows();
            return;
        }
        if (ob.Code == MacroType.None) return;
        while (ob.Next != null) {
            var next = (MacroObject)ob.Next;
            if (next.Code == MacroType.None) return;
            ob = next;
        }
        Macro.PushToBack(Macro.Create(MacroType.None));
        SetupRows();
    }

    void RemoveLast() {
        if (_rows.Children.Count == 0) return;
        Macro.Remove(Macro.GetLast());
        SetupRows();
        if (_rows.Children.Count == 0) AddEmpty();
    }

    void SetupRows() {
        if (Macro == null) return;
        for (var i = _rows.Children.Count - 1; i >= 0; i--) {
            var c = _rows.Children[i];
            _rows.Children.RemoveAt(i);
            c.Dispose();
        }
        if (Macro.Items == null) Macro.Items = Macro.Create(MacroType.None);
        var obj = (MacroObject)Macro.Items;
        while (obj != null) {
            _rows.Add(new MacroEntry(this, obj));
            if (obj.Next != null && obj.Code == MacroType.None) break;
            obj = (MacroObject)obj.Next;
        }
        _rows.ReArrange();
    }

    sealed class MacroRows : Control {
        public MacroRows() {
            WantUpdateSize = false;
            CanMove = false;
        }

        public void ReArrange() {
            var h = 0;
            var children = Children;
            for (var i = 0; i < children.Count; i++) {
                var c = children[i];
                if (c.IsDisposed || !c.IsVisible) continue;
                c.Y = h;
                h += c.Height;
            }
            Height = h;
        }
    }

    sealed class TextButton : Control {
        public TextButton(int id, int x, int y, int w, int h, string text) {
            Id = id;
            X = x;
            Y = y;
            Width = w;
            Height = h;
            AcceptMouseInput = true;
            WantUpdateSize = false;
            CanMove = false;
            Add(new Label(text ?? "", true, 0xFFFF, 1) { X = 2, Y = Math.Max(0, (h - 16) / 2) });
        }

        public int Id { get; }

        public override void OnMouseClick(int x, int y) => Parent?.OnButtonClick(Id);
    }

    /// <summary>One macro name. A click cycles. Combobox is not in this project.</summary>
    sealed class MacroChoice : Control {
        readonly string[] _items;
        readonly Label _label;
        int _selected = -1;

        public MacroChoice(int x, int y, int width, string[] items, int selected) {
            X = x;
            Y = y;
            Width = width;
            Height = 25;
            _items = items ?? Array.Empty<string>();
            WantUpdateSize = false;
            AcceptMouseInput = true;
            CanMove = false;
            Add(new ResizePic(0x0BB8) { Width = width, Height = Height });
            var shown = "";
            if (selected >= 0 && selected < _items.Length) {
                _selected = selected;
                shown = _items[selected];
            }
            _label = new Label(shown, true, 0x0453, 9) { X = 2, Y = 5 };
            Add(_label);
            var arrow = new GumpPic(width - 18, 2, 0x00FC, 0);
            if (!arrow.IsDisposed) {
                arrow.AcceptMouseInput = false;
                Add(arrow);
            }
        }

        public event EventHandler<int> OptionSelected;

        public int SelectedIndex {
            get => _selected;
            set {
                if (_items.Length == 0) return;
                if (value < 0) value = _items.Length - 1;
                if (value >= _items.Length) value = 0;
                if (value == _selected) return;
                _selected = value;
                _label.Text = _items[value];
                OptionSelected?.Invoke(this, value);
            }
        }

        public override void OnMouseClick(int x, int y) {
            if (_items.Length == 0) return;
            SelectedIndex = _selected + (GameController.Shift ? -1 : 1);
        }
    }

    sealed class MacroEntry : Control {
        readonly MacroControl _owner;
        MacroObject _obj;

        public MacroEntry(MacroControl owner, MacroObject obj) {
            _owner = owner;
            _obj = obj;
            WantUpdateSize = false;
            CanMove = false;
            var main = new MacroChoice(0, 0, 200, TypeNames, (int)obj.Code);
            main.OptionSelected += OnPicked;
            Add(main);
            Width = main.Width;
            Height = main.Height;
            AddSub(obj);
        }

        void OnPicked(object sender, int e) {
            var current = _obj;
            if (current == null) return;
            if (e == 0) {
                _owner.Macro.Remove(current);
                _obj = null;
                Dispose();
                _owner.SetupRows();
                return;
            }
            var created = Macro.Create((MacroType)e);
            _owner.Macro.Insert(current, created);
            _owner.Macro.Remove(current);
            _obj = created;
            for (var i = Children.Count - 1; i >= 1; i--) {
                var c = Children[i];
                Children.RemoveAt(i);
                c.Dispose();
            }
            Height = Children.Count > 0 ? Children[0].Height : 25;
            AddSub(created);
        }

        void AddSub(MacroObject obj) {
            if (obj == null || obj.Code == 0) {
                _owner._rows.ReArrange();
                return;
            }
            if (obj.SubMenuType == 1) {
                var count = 0;
                var offset = 0;
                Macro.GetBoundByCode(obj.Code, ref count, ref offset);
                if (count > 0 && offset >= 0 && offset + count <= SubNames.Length) {
                    var names = new string[count];
                    for (var i = 0; i < count; i++) names[i] = SubNames[i + offset];
                    var selected = (int)obj.SubCode - offset;
                    var sub = new MacroChoice(20, Height, 180, names, selected);
                    var code = obj.Code;
                    sub.OptionSelected += (s, index) => {
                        var n = 0;
                        var off = 0;
                        Macro.GetBoundByCode(code, ref n, ref off);
                        obj.SubCode = (MacroSubType)(off + index);
                    };
                    Add(sub);
                    Height += sub.Height;
                }
            }
            else if (obj.SubMenuType == 2) {
                var background = new ResizePic(0x0BB8) {
                    X = 16,
                    Y = Height,
                    Width = 240,
                    Height = 60
                };
                Add(background);
                var box = new StbTextBox(0xFF, 80, 236, true) {
                    X = background.X + 4,
                    Y = background.Y + 4,
                    Width = background.Width - 4,
                    Height = background.Height - 4
                };
                box.SetText(obj.HasString() ? ((MacroObjectString)obj).Text : "");
                box.TextChanged += (s, ev) => {
                    if (obj.HasString()) ((MacroObjectString)obj).Text = ((StbTextBox)s).Text;
                };
                Add(box);
                Height += background.Height;
                if (background.X + background.Width > Width) Width = background.X + background.Width;
            }
            _owner._rows.ReArrange();
        }
    }
}
