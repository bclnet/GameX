// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO QuestArrowGump. Shown for 0xE5 and removed for 0xE6.
/// The graphic is 0x1194 plus (direction + 1) mod 8. Hue blinks 0 and 0x21.
/// Screen position is the WorldGump tile, then ClassicUO's per-direction shift, then the buffer clamp.
/// The walk glide is not on this type. Send_ClickQuestArrow is not queued.
/// The name is the 0xE5 string. ClassicUO draws no text on this gump, so the line sits under the arrow.
/// </summary>
public sealed class QuestArrowGump : Gump {
    readonly Label _name;
    GumpPic _arrow;
    int _x, _y, _z;
    int _hueAt;
    bool _needHue;
    Dir _direction;

    public QuestArrowGump(World world, UOWaypoint point) : base(world, point.Serial, point.Serial) {
        CanMove = false;
        CanCloseWithRightClick = false;
        AcceptMouseInput = true;
        WantUpdateSize = false;
        _name = new Label(point.Name, true, 0x0386, 1);
        Add(_name);
        Move(point);
        Place();
    }

    public static void Open(World world, UOWaypoint point) {
        var open = UIManager.GetGump<QuestArrowGump>(point.Serial);
        if (open == null) UIManager.Add(new QuestArrowGump(world, point));
        else open.Move(point);
    }

    public static void Close(uint serial) => UIManager.GetGump<QuestArrowGump>(serial)?.Dispose();

    public override void Update() {
        if (UOLoginNet.PlayerSerial == 0) {
            Dispose();
            return;
        }
        Place();
        base.Update();
    }

    void Move(UOWaypoint point) {
        _x = point.X;
        _y = point.Y;
        _z = point.Z;
        _name.Text = point.Name;
    }

    void Place() {
        var dir = (Dir)Facing(UOLoginNet.PlayerX, UOLoginNet.PlayerY, _x, _y, 0);
        var graphic = (ushort)(0x1194 + ((int)dir + 1) % 8);
        if (_arrow == null || _arrow.IsDisposed) {
            _arrow = new GumpPic(0, 0, graphic, 0) { AcceptMouseInput = false };
            if (!_arrow.IsDisposed) Add(_arrow);
        }
        else if (_direction != dir || _arrow.Graphic != graphic) _arrow.Graphic = graphic;
        _direction = dir;
        if (_arrow == null || _arrow.IsDisposed) return;
        var aw = _arrow.Width;
        var ah = _arrow.Height;
        Width = aw;
        Height = ah;
        WorldGump.ViewCenter(out var cx, out var cy);
        var dx = _x - UOLoginNet.PlayerX;
        var dy = _y - UOLoginNet.PlayerY;
        var x = cx + (dx - dy) * 22;
        var y = cy + (dx + dy) * 22 - (_z - UOLoginNet.PlayerZ) * 4;
        switch (dir) {
            case Dir.North: x -= aw; break;
            case Dir.South: y -= ah; break;
            case Dir.East: x -= aw; y -= ah; break;
            case Dir.West: break;
            case Dir.Right: x -= aw; y -= ah / 2; break;
            case Dir.Left: x += aw / 2; y -= ah / 2; break;
            case Dir.Up: x -= aw / 2; y += ah / 2; break;
            case Dir.Down: x -= aw / 2; y -= ah; break;
        }
        var sample = Mouse.Current;
        var bw = sample.BufW > 0 ? sample.BufW : 640;
        var bh = sample.BufH > 0 ? sample.BufH : 480;
        if (x < 0) x = 0;
        else if (aw > 0 && x > bw - aw) x = bw - aw;
        if (y < 0) y = 0;
        else if (ah > 0 && y > bh - ah) y = bh - ah;
        X = x;
        Y = y;
        _name.Y = ah;
        _name.IsVisible = _name.Text.Length > 0;
        if (unchecked(Environment.TickCount - _hueAt) >= 1000) {
            _hueAt = Environment.TickCount;
            _needHue = !_needHue;
        }
        _arrow.Hue = (ushort)(_needHue ? 0 : 0x21);
    }

    /// <summary>ClassicUO GameCursor.GetMouseDirection. Tile coordinates, not screen pixels.</summary>
    static int Facing(int x1, int y1, int toX, int toY, int current) {
        var shiftX = toX - x1;
        var shiftY = toY - y1;
        var hash = 100 * (Sgn(shiftX) + 2) + 10 * (Sgn(shiftY) + 2);
        if (shiftX != 0 && shiftY != 0) {
            shiftX = Math.Abs(shiftX);
            shiftY = Math.Abs(shiftY);
            if (shiftY * 5 <= shiftX * 2) hash += 1;
            else if (shiftY * 2 >= shiftX * 5) hash += 3;
            else hash += 2;
        }
        else if (shiftX == 0 && shiftY == 0) return current;
        switch (hash) {
            case 111: return (int)Dir.West;
            case 112: return (int)Dir.Up;
            case 113: return (int)Dir.North;
            case 120: return (int)Dir.West;
            case 131: return (int)Dir.West;
            case 132: return (int)Dir.Left;
            case 133: return (int)Dir.South;
            case 210: return (int)Dir.North;
            case 230: return (int)Dir.South;
            case 311: return (int)Dir.East;
            case 312: return (int)Dir.Right;
            case 313: return (int)Dir.North;
            case 320: return (int)Dir.East;
            case 331: return (int)Dir.East;
            case 332: return (int)Dir.Down;
            case 333: return (int)Dir.South;
            default: return current;
        }
    }

    static int Sgn(int val) {
        var a = 0 < val ? 1 : 0;
        var b = val < 0 ? 1 : 0;
        return a - b;
    }

    enum Dir { North, Right, East, Down, South, Left, West, Up }
}
