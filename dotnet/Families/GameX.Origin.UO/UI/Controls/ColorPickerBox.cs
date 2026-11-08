// SPDX-License-Identifier: BSD-2-Clause

using System;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ColorPickerBox. The cells store hue ids: start at Graduation + 1, each cell keeps that plus one, then start steps by 5.
/// A custom palette replaces the start. The pixel is white and the sink applies the hue. This file does not read hues.mul.
/// IsModal is stored. UIManager does not block other gumps. A later press outside closes it when that flag is set.
/// </summary>
public class ColorPickerBox : Gump {
    // Not a gump id. The sink hues this one white pixel. Hue 0 uses DrawRaw so gump 0xEFEF is not cached.
    const ushort SwatchGraphic = 0xEFEF;
    static readonly byte[] White = { 0xFF, 0xFF };
    static readonly byte[] Marker = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
    readonly int _cellHeight;
    readonly int _cellWidth;
    readonly int _columns;
    readonly ushort[] _customPallete;
    readonly int _rows;
    int _graduation, _selectedIndex;
    ushort[] _hues = new ushort[0];
    bool _needToFileeBoxes = true;
    bool _ignorePress;

    public ColorPickerBox(
        World world,
        int x,
        int y,
        int rows = 10,
        int columns = 20,
        int cellW = 8,
        int cellH = 8,
        ushort[] customPallete = null
    ) : base(world, 0, 0) {
        X = x;
        Y = y;
        Width = columns * cellW;
        Height = rows * cellH;
        _rows = rows;
        _columns = columns;
        _cellWidth = cellW;
        _cellHeight = cellH;
        _customPallete = customPallete;
        AcceptMouseInput = true;
        CanMove = false;
        WantUpdateSize = false;
        _ignorePress = Mouse.LButtonPressed;
        Graduation = 1;
        SelectedIndex = 0;
    }

    public event EventHandler ColorSelectedIndex;

    public bool ShowLivePreview { get; set; }

    /// <summary>Stored for the caller. This client does not skip non-modal gumps.</summary>
    public bool IsModal { get; set; }

    /// <summary>After the opening press ends, a left press that is not on this box disposes it.</summary>
    public bool ModalClickOutsideAreaClosesThisControl { get; set; }

    public ushort[] Hues {
        get {
            CreateTexture();
            return _hues;
        }
    }

    public int Graduation {
        get => _graduation;
        set {
            if (_graduation != value) {
                _graduation = value;
                _needToFileeBoxes = true;
                CreateTexture();
                ColorSelectedIndex?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public int SelectedIndex {
        get => _selectedIndex;
        set {
            if (value < 0 || value >= _hues.Length) return;
            if (_selectedIndex != value) {
                _selectedIndex = value;
                ColorSelectedIndex?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public ushort SelectedHue {
        get => _selectedIndex < 0 || _selectedIndex >= _hues.Length ? (ushort)0 : _hues[_selectedIndex];
        set => SelectHue(value);
    }

    public override void Update() {
        if (IsDisposed) return;
        if (_needToFileeBoxes) CreateTexture();
        if (ShowLivePreview) {
            int sx = At(this, true), sy = At(this, false);
            int mx = Mouse.Position.X, my = Mouse.Position.Y;
            if (mx >= sx && my >= sy && mx < sx + Width && my < sy + Height)
                SetSelectedIndex(mx - sx, my - sy);
        }
        if (_ignorePress) {
            if (!Mouse.LButtonPressed) _ignorePress = false;
        }
        else if (ModalClickOutsideAreaClosesThisControl && Mouse.LButtonPressed && !Owns(UIManager.LastControlMouseDown(1))) {
            Dispose();
            return;
        }
        base.Update();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        CreateTexture();
        var sink = GumpArt.Sink;
        if (sink != null && _hues.Length >= _rows * _columns && _cellWidth > 0 && _cellHeight > 0) {
            for (int i = 0; i < _rows; i++) {
                for (int j = 0; j < _columns; j++) {
                    ushort hue = _hues[i * _columns + j];
                    int px = x + j * _cellWidth;
                    int py = y + i * _cellHeight;
                    if (hue == 0) sink.DrawRaw(0x5E320000, px, py, _cellWidth, _cellHeight, White, 1, 1);
                    else sink.Draw(SwatchGraphic, px, py, _cellWidth, _cellHeight, White, 1, 1, false, hue);
                }
            }
            if (_hues.Length > 1) {
                int mx = (int)(x + Width / _columns * (_selectedIndex % _columns + .5f) - 1);
                int my = (int)(y + Height / _rows * (_selectedIndex / _columns + .5f) - 1);
                sink.DrawRaw(0x5E32FFFE, mx, my, 2, 2, Marker, 2, 2);
            }
        }
        return base.Draw(x, y);
    }

    public override void OnMouseClick(int x, int y) => OnMouseUp(x, y, MouseButtonType.Left);

    protected void OnMouseUp(int x, int y, MouseButtonType button) {
        if (button != MouseButtonType.Left) return;
        SetSelectedIndex(x - At(this, true), y - At(this, false));
    }

    void SetSelectedIndex(int x, int y) {
        int row = x / (Width / _columns);
        int column = y / (Height / _rows);
        SelectedIndex = row + column * _columns;
    }

    void CreateTexture() {
        if (!_needToFileeBoxes || IsDisposed) return;
        _needToFileeBoxes = false;
        int size = _rows * _columns;
        ushort startColor = (ushort)(Graduation + 1);
        if (_hues == null || size != _hues.Length) _hues = new ushort[size];
        for (int y = 0; y < _rows; y++) {
            for (int x = 0; x < _columns; x++) {
                ushort hue = (ushort)((_customPallete?[y * _columns + x] ?? startColor) + 1);
                _hues[y * _columns + x] = hue;
                startColor += 5;
            }
        }
    }

    void SelectHue(ushort desiredHue) {
        int previousGraduation = Graduation;
        // CreateTexture adds one twice. Adding 3 is the same as subtracting 2, and it stays non-negative.
        Graduation = (desiredHue + 3) % 5;
        for (int i = 0; i < Hues.Length; i++) {
            if (Hues[i] == desiredHue) {
                SelectedIndex = i;
                return;
            }
        }
        Graduation = previousGraduation;
    }

    bool Owns(Control c) {
        for (; c != null; c = c.Parent)
            if (c == this) return true;
        return false;
    }

    static int At(Control c, bool horizontal) {
        int n = 0;
        for (var p = c; p != null; p = p.Parent) n += horizontal ? p.X : p.Y;
        return n;
    }
}
