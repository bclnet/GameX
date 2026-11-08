// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO city page. Facet names and the old town spots are the client table.
/// Cities are LoginScene.Cities. Finish calls CharCreationGump.CreateCharacter.
/// </summary>
public class CreateCharSelectionCityGump : Gump {
    static readonly string[] Facets = ["Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno", "Ter Mur"];
    static readonly int[] TownX = [105, 245, 165, 395, 200, 335, 160, 100, 270];
    static readonly int[] TownY = [130, 90, 200, 160, 305, 250, 395, 250, 130];
    static readonly int[,] MapSize = {
        { 7168, 4096 }, { 7168, 4096 }, { 2304, 1600 }, { 2560, 2048 }, { 1448, 1448 }, { 1280, 4096 },
    };

    readonly List<CityControl> _cities = new();
    readonly Label _facetName;
    readonly HtmlControl _description;
    readonly LoginScene _scene;
    readonly byte _profession;
    CityInfo _selected;

    public bool HasCity { get; }

    public CreateCharSelectionCityGump(World world, byte profession, LoginScene scene) : base(world, 0, 0) {
        CanMove = false;
        CanCloseWithRightClick = false;
        CanCloseWithEsc = false;
        _scene = scene;
        _profession = profession;

        var modern = CharCreationGump.Version >= ClientVersion.CV_70130;
        var city = modern ? At(0) : At(3) ?? At(0);
        HasCity = city != null;

        var map = city != null && city.IsNewCity ? (int)city.Map : 0;
        if (map > 5) map = 5;
        _facetName = new Label("", true, 0x0481, 0) { X = 240, Y = 440 };
        if (modern) {
            AddPic(62, 54, (ushort)(0x15D9 + map));
            AddPic(57, 49, 0x15DF);
            _facetName.Text = Facets[map];
        }
        else {
            AddPic(57, 49, 0x1598);
            _facetName.IsVisible = false;
        }
        Add(_facetName);

        var prev = new Button((int)Buttons.PreviousScreen, 0x15A1, 0x15A3, 0x15A2) {
            X = 586, Y = 445, ButtonAction = ButtonAction.Activate,
        };
        var finish = new Button((int)Buttons.Finish, 0x15A4, 0x15A6, 0x15A5) {
            X = 610, Y = 445, ButtonAction = ButtonAction.Activate,
        };
        if (!prev.IsDisposed) Add(prev);
        if (!finish.IsDisposed) Add(finish);

        var desc = city?.Description ?? "";
        _description = new HtmlControl(452, 60, 175, 367, true, true, text: string.IsNullOrEmpty(desc) ? " " : desc, ishtml: true) {
            CanMove = false,
        };
        if (string.IsNullOrEmpty(desc)) _description.Text = "";
        Add(_description);

        var cities = scene?.Cities;
        if (cities != null) {
            for (var i = 0; i < cities.Length; i++) {
                var row = At(i);
                if (row == null) continue;
                int x = 0, y = 0;
                if (row.IsNewCity) {
                    var facet = (int)(row.Map > 5 ? 5 : row.Map);
                    x = 62 + Percent(MapSize[facet, 0] - 2048, row.X, 383);
                    y = 54 + Percent(MapSize[facet, 1], row.Y, 384);
                }
                else if (i < TownX.Length) {
                    x = TownX[i];
                    y = TownY[i];
                }
                var control = new CityControl(this, row, x, y, i);
                Add(control);
                _cities.Add(control);
            }
        }
        if (city != null) Select(city);
    }

    public override void OnButtonClick(int buttonID) {
        var shell = CharCreationGump.Shell(this);
        if (shell == null) return;
        switch ((Buttons)buttonID) {
            case Buttons.PreviousScreen:
                shell.StepBack(_profession > 0 ? 2 : 1);
                return;
            case Buttons.Finish:
                if (_selected != null) shell.SetCity(_selected.Index);
                shell.CreateCharacter(_profession);
                shell.IsVisible = false;
                return;
        }
        if (buttonID >= 2) Select(At(buttonID - 2));
    }

    void Select(CityInfo city) {
        if (city == null) return;
        _selected = city;
        if (_description != null) _description.Text = city.Description ?? "";
        if (CharCreationGump.Version < ClientVersion.CV_70130 || _facetName == null) return;
        var index = (int)city.Map;
        if (index >= Facets.Length) index = Facets.Length - 1;
        if (index < 0) index = 0;
        _facetName.Text = Facets[index];
        var pos = -1;
        var rows = _scene?.Cities;
        if (rows != null)
            for (var i = 0; i < rows.Length; i++)
                if (rows[i] == city) { pos = i; break; }
        for (var i = 0; i < _cities.Count; i++) _cities[i].IsSelected = _cities[i].Index == pos;
    }

    CityInfo At(int index) {
        var cities = _scene?.Cities;
        if (cities == null || index < 0 || index >= cities.Length) return null;
        return cities[index];
    }

    void AddPic(int x, int y, ushort graphic) {
        var pic = new GumpPic(x, y, graphic, 0) { AcceptMouseInput = false, CanMove = false };
        if (!pic.IsDisposed) Add(pic);
    }

    static int Percent(int max, int current, int maxValue) {
        if (max > 0) {
            max = current * 100 / max;
            if (max > 100) max = 100;
            if (max > 1) max = maxValue * max / 100;
        }
        return max;
    }

    enum Buttons { PreviousScreen, Finish }

    sealed class CityControl : Control {
        readonly CreateCharSelectionCityGump _owner;
        readonly Button _button;
        readonly Label _label;
        bool _selected;

        public CityControl(CreateCharSelectionCityGump owner, CityInfo city, int x, int y, int index) {
            _owner = owner;
            Index = index;
            CanMove = false;
            WantUpdateSize = false;
            AcceptMouseInput = false;
            var button = new Button(2 + index, 0x04B9, 0x04BA, 0x04BA) {
                ButtonAction = ButtonAction.Activate, X = x, Y = y,
            };
            _button = button;
            if (!button.IsDisposed) Add(button);
            var label = new CityLabel(owner, index, city.City ?? "") { X = x, Y = y - 20 };
            _label = label;
            Add(label);
            if (label.X + label.Width >= 383) label.X -= 60;
        }

        public int Index { get; }

        public bool IsSelected {
            get => _selected;
            set {
                if (_selected == value) return;
                _selected = value;
                if (_label != null) _label.Hue = value ? (ushort)0x0481 : (ushort)0x0058;
                if (_button != null && !_button.IsDisposed)
                    _button.ButtonGraphicNormal = value ? (ushort)0x04BA : (ushort)0x04B9;
            }
        }

        public override void Update() {
            base.Update();
            if (_selected || _button == null || _button.IsDisposed) return;
            var over = UIManager.MouseOverControl == _button || UIManager.MouseOverControl == _label;
            var graphic = over ? (ushort)0x04BA : (ushort)0x04B9;
            if (_button.ButtonGraphicNormal != graphic && GumpMetrics.TrySize(graphic, out _, out _))
                _button.ButtonGraphicNormal = graphic;
            if (_label != null && !_selected) _label.Hue = over ? (ushort)0x0099 : (ushort)0x0058;
        }

        sealed class CityLabel : Label {
            readonly CreateCharSelectionCityGump _owner;
            readonly int _index;

            public CityLabel(CreateCharSelectionCityGump owner, int index, string text) : base(text, false, 0x0058, 3) {
                _owner = owner;
                _index = index;
                AcceptMouseInput = true;
            }

            public override void OnMouseClick(int x, int y) => _owner.OnButtonClick(_index + 2);
        }
    }
}
