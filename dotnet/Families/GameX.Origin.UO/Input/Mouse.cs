// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO Mouse. Position is filled by <see cref="Read"/>, which GameX.Origin.Mg sets
/// from the MonoGame window. This project cannot call that API.
/// </summary>
public static class Mouse {
    public const int MOUSE_DELAY_DOUBLE_CLICK = 350;

    public struct Point {
        public int X;
        public int Y;
        public static Point Zero => default;
        public static Point operator -(Point a, Point b) => new() { X = a.X - b.X, Y = a.Y - b.Y };
    }

    /// <summary>
    /// One frame of the window pointer. Coordinates are window pixels, before the backbuffer scale.
    /// </summary>
    public struct Sample {
        public int X, Y, WinW, WinH, BufW, BufH, Wheel;
        public bool Left, Middle, Right, X1, X2, InWindow;
    }

    public static System.Func<Sample> Read;
    public static Sample Current { get; private set; }
    public static bool HasSample { get; private set; }

    public static Point Position;
    public static Point LClickPosition;
    public static Point RClickPosition;
    public static Point MClickPosition;

    public static uint LastLeftButtonClickTime { get; set; }
    public static uint LastMidButtonClickTime { get; set; }
    public static uint LastRightButtonClickTime { get; set; }
    public static bool CancelDoubleClick { get; set; }
    public static bool LButtonPressed { get; set; }
    public static bool RButtonPressed { get; set; }
    public static bool MButtonPressed { get; set; }
    public static bool XButtonPressed { get; set; }
    public static bool IsDragging { get; set; }
    public static Point LDragOffset => LButtonPressed ? Position - LClickPosition : Point.Zero;
    public static Point RDragOffset => RButtonPressed ? Position - RClickPosition : Point.Zero;
    public static Point MDragOffset => MButtonPressed ? Position - MClickPosition : Point.Zero;
    public static bool MouseInWindow { get; set; }

    public static void ButtonPress(MouseButtonType type) {
        CancelDoubleClick = false;
        switch (type) {
            case MouseButtonType.Left:
                LButtonPressed = true;
                LClickPosition = Position;
                break;
            case MouseButtonType.Middle:
                MButtonPressed = true;
                MClickPosition = Position;
                break;
            case MouseButtonType.Right:
                RButtonPressed = true;
                RClickPosition = Position;
                break;
            case MouseButtonType.XButton1:
            case MouseButtonType.XButton2:
                XButtonPressed = true;
                break;
        }
    }

    public static void ButtonRelease(MouseButtonType type) {
        switch (type) {
            case MouseButtonType.Left: LButtonPressed = false; break;
            case MouseButtonType.Middle: MButtonPressed = false; break;
            case MouseButtonType.Right: RButtonPressed = false; break;
            case MouseButtonType.XButton1:
            case MouseButtonType.XButton2:
                XButtonPressed = false;
                break;
        }
    }

    /// <summary>
    /// ClassicUO reads SDL here. The sample is already in window pixels (inside or outside the client),
    /// then scaled onto the backbuffer. ClassicUO also divides by DpiScale. This client has no DpiScale.
    /// </summary>
    public static void Update() {
        if (Read == null) { HasSample = false; return; }
        var s = Current = Read();
        HasSample = true;
        MouseInWindow = s.InWindow;
        Position.X = s.X;
        Position.Y = s.Y;
        if (s.WinW > 0 && s.BufW > 0) Position.X = (int)((double)Position.X * s.BufW / s.WinW);
        if (s.WinH > 0 && s.BufH > 0) Position.Y = (int)((double)Position.Y * s.BufH / s.WinH);
        IsDragging = LButtonPressed || RButtonPressed || MButtonPressed;
    }
}
