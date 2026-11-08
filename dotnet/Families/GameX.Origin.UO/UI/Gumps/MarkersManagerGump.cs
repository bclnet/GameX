// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>One user marker. Hue is HueOf(ColorName). No icon texture is stored.</summary>
public sealed class WMapMarker {
    public string Name = "";
    public int X;
    public int Y;
    public int MapId;
    public int ZoomIndex;
    public string MarkerIconName = "";
    public string ColorName = "none";
    public ushort Hue;
}

/// <summary>One in-memory marker file. FullPath is empty. IsEditable is true for userMarkers.</summary>
public sealed class WMapMarkerFile {
    public string Name = "";
    public string FullPath = "";
    public bool IsEditable;
    public bool Hidden;
    public List<WMapMarker> Markers = new List<WMapMarker>();
}

/// <summary>
/// ClassicUO MarkersManagerGump. The rows are the in-memory list. Search filters by name.
/// Edit opens UserMarkersGump. Remove drops that row. Go stores the point and does not send 0xF0.
/// WorldMapGump has no GoToMarker, so the map is not moved.
/// Save and Load do not touch a file.
/// </summary>
public sealed class MarkersManagerGump : Gump {
    public const string UserFileName = "userMarkers";
    public const int WidthPx = 620;
    public const int HeightPx = 500;
    const int PageSize = 14;
    const ushort Hue = 0x0481;
    const int SearchId = 100;
    const int ClearId = 101;
    const int PrevId = 102;
    const int NextId = 103;
    const int EditId = 1;
    const int RemoveId = 2;
    const int GotoId = 3;

    public static readonly List<WMapMarkerFile> MarkerFiles = new List<WMapMarkerFile>();

    public static bool HasGoto;
    public static int GotoX;
    public static int GotoY;

    static MarkersManagerGump() {
        MarkerFiles.Add(new WMapMarkerFile {
            Name = UserFileName,
            FullPath = "",
            IsEditable = true,
            Markers = new List<WMapMarker>()
        });
    }

    TextBox _box;
    string _search = "";
    int _category;
    int _page;
    bool _modified;

    public MarkersManagerGump(World world) : base(world, 0, 0) {
        X = 50;
        Y = 50;
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        WantUpdateSize = false;
        Build();
    }

    /// <summary>One gump. A second call disposes it and does not add another.</summary>
    public static void Toggle(World world) {
        var open = UIManager.GetGump<MarkersManagerGump>();
        if (open != null) {
            open.Dispose();
            return;
        }
        UIManager.Add(new MarkersManagerGump(world));
    }

    /// <summary>The point a Go click stored. The world map is not moved.</summary>
    public static void GoTo(int x, int y) {
        HasGoto = true;
        GotoX = x;
        GotoY = y;
    }

    public static void Save() {
        // Profile path is not ported.
    }

    public static void Load() {
        // Profile path is not ported.
    }

    public override void OnButtonClick(int buttonID) {
        switch (buttonID) {
            case SearchId:
                _search = _box != null && !_box.IsDisposed ? _box.Text ?? "" : _search;
                _page = 0;
                Build();
                break;
            case ClearId:
                _search = "";
                _page = 0;
                Build();
                break;
            case PrevId:
                if (_page > 0) _page--;
                Build();
                break;
            case NextId:
                _page++;
                Build();
                break;
        }
    }

    public override void OnKeyboardReturn(int textID, string text) {
        _search = text ?? "";
        _page = 0;
        Build();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        return base.Draw(x, y);
    }

    public override void Dispose() {
        if (_modified) {
            Save();
            _modified = false;
            Load();
        }
        base.Dispose();
    }

    public void RemoveAt(int index) {
        var list = Current();
        if (list == null || index < 0 || index >= list.Count) return;
        list.RemoveAt(index);
        _modified = true;
        Build();
    }

    public void NoteChanged() { _modified = true; }

    public List<WMapMarker> Current() {
        if (_category < 0 || _category >= MarkerFiles.Count) return null;
        var file = MarkerFiles[_category];
        if (file.Markers == null) file.Markers = new List<WMapMarker>();
        return file.Markers;
    }

    public void Category(int index) {
        if (index < 0 || index >= MarkerFiles.Count) return;
        _category = index;
        _page = 0;
        Build();
    }

    void Build() {
        var draft = _box != null && !_box.IsDisposed ? _box.Text : _search;
        _search = draft ?? "";
        ClearChildren();
        _box = null;
        Width = WidthPx;
        Height = HeightPx;
        WantUpdateSize = false;
        Add(new ResizePic(0x0A28) { X = 0, Y = 0, Width = WidthPx, Height = HeightPx });
        Add(new Label("Icon", true, Hue, 1) { X = 8, Y = 8 });
        Add(new Label("Name", true, Hue, 1) { X = 120, Y = 8 });
        Add(new Label("X", true, Hue, 1) { X = 320, Y = 8 });
        Add(new Label("Y", true, Hue, 1) { X = 380, Y = 8 });
        Add(new Label("Color", true, Hue, 1) { X = 440, Y = 8 });
        Add(new Label("Edit", true, Hue, 1) { X = 500, Y = 8 });
        Add(new Label("Remove", true, Hue, 1) { X = 540, Y = 8 });
        Add(new Label("Go", true, Hue, 1) { X = 590, Y = 8 });
        Add(new Label("Search", true, Hue, 1) { X = 8, Y = 36 });
        Add(new ResizePic(0x0BB8) { X = 70, Y = 32, Width = 200, Height = 22 });
        _box = new TextBox { X = 74, Y = 34, Width = 190, Height = 18, Hue = Hue, MaxLength = 30, Text = _search };
        Add(_box);
        Add(Act(SearchId, 0x0FB7, 0x0FB9, "Find", 280, 32));
        Add(Act(ClearId, 0x0FB1, 0x0FB2, "Clear", 320, 32));
        var list = Current();
        var editable = _category >= 0 && _category < MarkerFiles.Count && MarkerFiles[_category].IsEditable;
        var hits = new List<int>();
        if (list != null) {
            for (var i = 0; i < list.Count; i++) {
                var name = list[i] != null ? list[i].Name ?? "" : "";
                if (_search.Length != 0 && name.IndexOf(_search, StringComparison.OrdinalIgnoreCase) < 0) continue;
                hits.Add(i);
            }
        }
        var pages = (hits.Count + PageSize - 1) / PageSize;
        if (pages < 1) pages = 1;
        if (_page >= pages) _page = pages - 1;
        if (_page < 0) _page = 0;
        var start = _page * PageSize;
        var y = 68;
        for (var n = start; n < hits.Count && n < start + PageSize; n++) {
            var idx = hits[n];
            Add(new MarkerRow(this, list[idx], idx, y, editable));
            y += 25;
        }
        Add(new Label((_page + 1) + "/" + pages, true, Hue, 1) { X = 420, Y = 34 });
        Add(new MarkerAct("Prev", PrevId) { X = 470, Y = 34 });
        Add(new MarkerAct("Next", NextId) { X = 520, Y = 34 });
        var x = 8;
        for (var i = 0; i < MarkerFiles.Count; i++) {
            var name = MarkerFiles[i].Name ?? "";
            var cat = new CatLabel(this, i, name, i == _category) { X = x, Y = HeightPx - 28 };
            Add(cat);
            x += cat.Width + 12;
        }
    }

    void ClearChildren() {
        while (Children.Count > 0) {
            var c = Children[Children.Count - 1];
            c.Parent = null;
            c.Dispose();
        }
    }

    sealed class MarkerRow : Control {
        readonly MarkersManagerGump _owner;
        WMapMarker _marker;
        readonly int _index;
        readonly Label _name;
        readonly Label _x;
        readonly Label _y;
        readonly Label _color;
        readonly Label _icon;

        public MarkerRow(MarkersManagerGump owner, WMapMarker marker, int index, int y, bool editable) {
            _owner = owner;
            _marker = marker ?? new WMapMarker();
            _index = index;
            X = 8;
            Y = y;
            Width = 600;
            Height = 22;
            WantUpdateSize = false;
            CanMove = true;
            _icon = new Label(_marker.MarkerIconName ?? "", true, Hue, 1) { X = 0, Y = 0 };
            _name = new Label(_marker.Name ?? "", true, _marker.Hue == 0 ? Hue : _marker.Hue, 1) { X = 112, Y = 0 };
            _x = new Label(_marker.X.ToString(), true, Hue, 1) { X = 312, Y = 0 };
            _y = new Label(_marker.Y.ToString(), true, Hue, 1) { X = 372, Y = 0 };
            _color = new Label(_marker.ColorName ?? "", true, Hue, 1) { X = 432, Y = 0 };
            Add(_icon);
            Add(_name);
            Add(_x);
            Add(_y);
            Add(_color);
            if (editable) {
                Add(Act(EditId, 0x0FAB, 0x0FAC, "Edit", 492, 0));
                Add(Act(RemoveId, 0x0FB1, 0x0FB2, "X", 532, 0));
            }
            Add(Act(GotoId, 0x0FA5, 0x0FA7, "Go", 572, 0));
        }

        public override void OnButtonClick(int buttonID) {
            switch (buttonID) {
                case EditId:
                    Edit();
                    break;
                case RemoveId:
                    _owner.RemoveAt(_index);
                    break;
                case GotoId:
                    MarkersManagerGump.GoTo(_marker.X, _marker.Y);
                    break;
            }
        }

        void Edit() {
            var open = UIManager.GetGump<UserMarkersGump>();
            if (open != null) open.Dispose();
            var list = _owner.Current();
            var g = new UserMarkersGump(_owner.World, _marker.X, _marker.Y, list, _marker.ColorName, _marker.MarkerIconName, true, _index);
            g.EditEnd += OnEdit;
            UIManager.Add(g);
        }

        void OnEdit(object sender, EventArgs e) {
            if (!(sender is WMapMarker marker)) return;
            _marker = marker;
            _name.Text = marker.Name ?? "";
            _name.Hue = marker.Hue == 0 ? Hue : marker.Hue;
            _x.Text = marker.X.ToString();
            _y.Text = marker.Y.ToString();
            _color.Text = marker.ColorName ?? "";
            _icon.Text = marker.MarkerIconName ?? "";
            _owner.NoteChanged();
        }
    }

    sealed class CatLabel : Label {
        readonly MarkersManagerGump _owner;
        readonly int _index;

        public CatLabel(MarkersManagerGump owner, int index, string name, bool selected) : base(name, true, selected ? (ushort)0x0035 : (ushort)0xFFFF, 1) {
            _owner = owner;
            _index = index;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) => _owner.Category(_index);
    }

    static Control Act(int id, ushort normal, ushort pressed, string text, int x, int y) {
        var b = new Button(id, normal, pressed) { X = x, Y = y, ButtonAction = ButtonAction.Activate };
        if (!b.IsDisposed) {
            if (b.Width < 8) b.Width = 16;
            if (b.Height < 8) b.Height = 16;
            return b;
        }
        return new MarkerAct(text, id) { X = x, Y = y };
    }

    sealed class MarkerAct : Label {
        readonly int _id;

        public MarkerAct(string text, int id) : base(text, true, 0xFFFF, 1) {
            _id = id;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) => Parent?.OnButtonClick(_id);
    }
}
