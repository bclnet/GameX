// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO MapGump. 0x90 or 0xF5 opens it. 0x56 updates it.
/// Frame 0x1432, plot 0x1398, stop 0x1399, clear 0x139A, pin 0x139B, mark 0x139D.
/// The map picture is MultiMaps. This client does not build that texture.
/// Plot, stop, and clear change the local buttons. Send_MapMessage is not queued.
/// </summary>
public sealed class MapGump : Gump {
    const byte ActionAdd = 1;
    const byte ActionClear = 5;
    const byte ActionPlot = 7;

    readonly ResizePic _frame;
    readonly Button _plot, _stop, _clear;
    readonly GumpPic _mark;
    readonly List<Control> _pins = [];
    int _width, _height, _plotState;

    public MapGump(World world, UOMapState state) : base(world, state.Serial, state.Serial) {
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        WantUpdateSize = false;
        _frame = new ResizePic(0x1432);
        Add(_frame);
        _plot = new Button((int)MapButton.PlotCourse, 0x1398, 0x1398) { ButtonAction = ButtonAction.Activate };
        _stop = new Button((int)MapButton.StopPlotting, 0x1399, 0x1399) { ButtonAction = ButtonAction.Activate };
        _clear = new Button((int)MapButton.ClearCourse, 0x139A, 0x139A) { ButtonAction = ButtonAction.Activate };
        if (!_plot.IsDisposed) Add(_plot);
        if (!_stop.IsDisposed) Add(_stop);
        if (!_clear.IsDisposed) Add(_clear);
        _mark = new GumpPic(0, 0, 0x139D, 0);
        if (!_mark.IsDisposed) Add(_mark);
        Show(state);
    }

    /// <summary>One map for this serial. A second 0x90 replaces the size and clears the pins.</summary>
    public static void Open(World world, UOMapState state) {
        var open = UIManager.GetGump<MapGump>(state.Serial);
        if (open == null) UIManager.Add(new MapGump(world, state));
        else open.Show(state);
    }

    /// <summary>0x56. Ignored when that map is not open, which is what ClassicUO does.</summary>
    public static void OnData(UOMapData data) {
        var open = UIManager.GetGump<MapGump>(data.Serial);
        if (open == null) return;
        switch (data.Action) {
            case ActionAdd: open.AddPin(data.X, data.Y); break;
            case ActionClear: open.ClearPins(); break;
            case ActionPlot: open.SetPlotState(data.PlotState); break;
        }
    }

    public int PlotState => _plotState;

    public override void OnButtonClick(int buttonID) {
        switch ((MapButton)buttonID) {
            case MapButton.PlotCourse:
            case MapButton.StopPlotting:
                SetPlotState(_plotState == 0 ? 1 : 0);
                break;
            case MapButton.ClearCourse:
                ClearPins();
                break;
        }
    }

    void Show(UOMapState state) {
        _width = state.Width;
        _height = state.Height;
        Width = _width;
        Height = _height;
        if (!_frame.IsDisposed) {
            _frame.Width = _width + 44;
            _frame.Height = _height + 61;
        }
        Place(_plot, (_width - 100) >> 1, 5);
        Place(_stop, (_width - 70) >> 1, 5);
        Place(_clear, (_width - 66) >> 1, _height + 37);
        if (!_mark.IsDisposed) {
            _mark.X = _width - 20;
            _mark.Y = _height - 20;
        }
        ClearPins();
        SetPlotState(0);
    }

    static void Place(Button button, int x, int y) {
        if (button == null || button.IsDisposed) return;
        button.X = x;
        button.Y = y;
    }

    void AddPin(int x, int y) {
        var pin = new GumpPic(x, y, 0x139B, 0);
        if (pin.IsDisposed) return;
        pin.X += pin.Width + 5;
        pin.Y += pin.Height;
        var text = new Label((_pins.Count + 1).ToString(), false, 0, 0);
        text.X = -text.Width - 1;
        pin.Add(text);
        Add(pin);
        _pins.Add(pin);
    }

    void ClearPins() {
        foreach (var pin in _pins) {
            pin.Parent = null;
            pin.Dispose();
        }
        _pins.Clear();
    }

    void SetPlotState(int state) {
        _plotState = state;
        var plot = _plotState == 0;
        var edit = _plotState == 1;
        Show(_plot, plot);
        Show(_stop, edit);
        Show(_clear, edit);
    }

    static void Show(Button button, bool on) {
        if (button == null || button.IsDisposed) return;
        button.IsVisible = on;
        button.IsEnabled = on;
    }

    enum MapButton { PlotCourse, StopPlotting, ClearCourse }
}
