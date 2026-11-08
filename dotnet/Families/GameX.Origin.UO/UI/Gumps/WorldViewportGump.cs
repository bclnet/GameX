// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO game-window frame. Horizontal border 0x0A8C, vertical 0x0A8D, inset 5, tile thickness 4.
/// The resize button is 0x0837 / 0x0838. The inner view does not drop below 640 by 480.
/// WorldGump already draws the world. This frame does not draw a second scene.
/// </summary>
public sealed class WorldViewportGump : Gump {
    public const int BORDER_WIDTH = 5;
    const int BorderSize = 4;
    const int MinW = 640;
    const int MinH = 480;
    const ushort HBorder = 0x0A8C;
    const ushort VBorder = 0x0A8D;

    readonly GumpPicTiled _top;
    readonly GumpPicTiled _bottom;
    readonly GumpPicTiled _left;
    readonly GumpPicTiled _right;
    readonly Button _button;
    readonly SystemChatControl _chat;
    int _innerW;
    int _innerH;
    int _savedW;
    int _savedH;
    int _savedX;
    int _savedY;
    bool _drag;
    bool _resize;
    bool _move;

    // Profile path is not ported.
    public bool GameWindowLock { get; set; }

    public WorldViewportGump(World world) : base(world, 0, 0) {
        AcceptMouseInput = false;
        CanCloseWithEsc = false;
        CanCloseWithRightClick = false;
        WantUpdateSize = false;
        // ClassicUO uses UILayer.Under because the scene draws the world after that layer.
        // WorldGump is a Default gump, so Under would paint this frame under the world.
        LayerOrder = UILayer.Over;
        var sample = Mouse.Current;
        var boundsW = sample.BufW > 0 ? sample.BufW : MinW + BORDER_WIDTH * 2;
        var boundsH = sample.BufH > 0 ? sample.BufH : MinH + BORDER_WIDTH * 2;
        _innerW = boundsW - BORDER_WIDTH * 2;
        _innerH = boundsH - BORDER_WIDTH * 2;
        if (_innerW < MinW) _innerW = MinW;
        if (_innerH < MinH) _innerH = MinH;
        var maxW = boundsW - BORDER_WIDTH;
        var maxH = boundsH - BORDER_WIDTH;
        if (_innerW > maxW) _innerW = maxW;
        if (_innerH > maxH) _innerH = maxH;
        _savedW = _innerW;
        _savedH = _innerH;
        Width = _innerW + BORDER_WIDTH * 2;
        Height = _innerH + BORDER_WIDTH * 2;
        CanMove = !GameWindowLock;
        _top = new GumpPicTiled(0, 0, Width, BorderSize, HBorder);
        _bottom = new GumpPicTiled(0, 0, Width, BorderSize, HBorder);
        _left = new GumpPicTiled(0, 0, BorderSize, Height, VBorder);
        _right = new GumpPicTiled(0, 0, BorderSize, Height, VBorder);
        Keep(_top);
        Keep(_bottom);
        Keep(_left);
        Keep(_right);
        _button = new Button(0, 0x0837, 0x0838, 0x0838);
        if (_button != null && !_button.IsDisposed) Add(_button);
        _chat = new SystemChatControl(this, BORDER_WIDTH, BORDER_WIDTH, _innerW, _innerH);
        Add(_chat);
        ApplyLayout();
    }

    public SystemChatControl Chat => _chat;

    public override void Update() {
        base.Update();
        if (IsDisposed) return;
        CanMove = !GameWindowLock;
        var held = UIManager.LastControlMouseDown(1);
        if (Mouse.LButtonPressed && held != null && !GameWindowLock) {
            if (!_drag) {
                _drag = true;
                _resize = held == _button;
                _move = !_resize && IsBorder(held);
                _savedW = _innerW;
                _savedH = _innerH;
                _savedX = X;
                _savedY = Y;
            }
            var offset = Mouse.LDragOffset;
            if (_resize && (offset.X != 0 || offset.Y != 0)) ResizeBy(offset.X, offset.Y);
            else if (_move && (offset.X != 0 || offset.Y != 0)) MoveBy(offset.X, offset.Y);
        }
        else if (_drag) {
            if (_resize) SendSize();
            _drag = false;
            _resize = false;
            _move = false;
        }
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        return base.Draw(x, y);
    }

    /// <summary>Inner size, without the 5-pixel inset. Does not draw the world.</summary>
    public void ResizeGameWindow(int width, int height) {
        // This client has no DpiScale.
        if (width < MinW) width = MinW;
        if (height < MinH) height = MinH;
        _savedW = _innerW = width;
        _savedH = _innerH = height;
        ApplyLayout();
        SendSize();
    }

    /// <summary>Moves this frame only. WorldGump keeps its own origin.</summary>
    public void SetGameWindowPosition(int x, int y) {
        X = x;
        Y = y;
        Clamp();
    }

    void ResizeBy(int dx, int dy) {
        var w = _savedW + dx;
        var h = _savedH + dy;
        if (w < MinW) w = MinW;
        if (h < MinH) h = MinH;
        var maxW = ClientW() - BORDER_WIDTH;
        var maxH = ClientH() - BORDER_WIDTH;
        if (w > maxW) w = maxW;
        if (h > maxH) h = maxH;
        _innerW = w;
        _innerH = h;
        ApplyLayout();
    }

    void MoveBy(int dx, int dy) {
        X = _savedX + dx;
        Y = _savedY + dy;
        Clamp();
    }

    void Clamp() {
        var boundsW = ClientW();
        var boundsH = ClientH();
        var x = X;
        var y = Y;
        if (x + Width - BORDER_WIDTH > boundsW) x = boundsW - (Width - BORDER_WIDTH);
        if (x < -BORDER_WIDTH) x = -BORDER_WIDTH;
        if (y + Height - BORDER_WIDTH > boundsH) y = boundsH - (Height - BORDER_WIDTH);
        if (y < -BORDER_WIDTH) y = -BORDER_WIDTH;
        X = x;
        Y = y;
    }

    void ApplyLayout() {
        Width = _innerW + BORDER_WIDTH * 2;
        Height = _innerH + BORDER_WIDTH * 2;
        Place(_top, 0, 0, Width, BorderSize);
        Place(_bottom, 0, Height - BorderSize, Width, BorderSize);
        Place(_left, 0, 0, BorderSize, Height);
        GumpMetrics.TrySize(VBorder, out var vw, out _);
        Place(_right, Width - BorderSize, vw >> 1, BorderSize, Height - BorderSize);
        if (_button != null && !_button.IsDisposed) {
            _button.X = Width - (_button.Width >> 1);
            _button.Y = Height - (_button.Height >> 1);
        }
        if (_chat != null && !_chat.IsDisposed) {
            _chat.X = BORDER_WIDTH;
            _chat.Y = BORDER_WIDTH;
            _chat.Width = _innerW;
            _chat.Height = _innerH;
            _chat.Resize();
        }
        WantUpdateSize = false;
    }

    void SendSize() {
        var ver = UltimaOnline.Current?.Version ?? default;
        if (ver != default && ver < ClientVersion.CV_200) return;
        UOLoginNet.Send(OutgoingPackets.Send_GameWindowSize((uint)_innerW, (uint)_innerH));
    }

    bool IsBorder(Control held) => held == _top || held == _bottom || held == _left || held == _right;

    void Keep(Control c) {
        if (c != null && !c.IsDisposed) {
            c.WantUpdateSize = false;
            Add(c);
        }
    }

    static void Place(GumpPicTiled edge, int x, int y, int w, int h) {
        if (edge == null || edge.IsDisposed) return;
        edge.X = x;
        edge.Y = y;
        if (w > 0) edge.Width = w;
        if (h > 0) edge.Height = h;
        edge.WantUpdateSize = false;
    }

    static int ClientW() {
        var w = Mouse.Current.BufW;
        return w > 0 ? w : MinW + BORDER_WIDTH * 2;
    }

    static int ClientH() {
        var h = Mouse.Current.BufH;
        return h > 0 ? h : MinH + BORDER_WIDTH * 2;
    }
}
