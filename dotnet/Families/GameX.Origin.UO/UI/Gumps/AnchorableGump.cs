// SPDX-License-Identifier: BSD-2-Clause

using System.Drawing;

namespace GameX.Origin.UO;

/// <summary>ClassicUO ANCHOR_TYPE. 0 is none, 1 is spell, 2 is health bar.</summary>
public enum ANCHOR_TYPE {
    NONE,
    SPELL,
    HEALTHBAR
}

/// <summary>
/// ClassicUO AnchorableGump. UIManager has no AnchorManager, so <see cref="Anchors"/> is the one list.
/// A gump is anchorable only after SetAnchorId, and only when AnchorType is not none.
/// UIManager.OnMouseDragging is empty, so a pressed CanMove gump applies Mouse.LDragOffset here.
/// Profile path is not ported. HoldAltToMoveGumps is false. HoldDownKeyAltToCloseAnchored is true.
/// CloseAllAnchoredGumpsInGroupWithRightClick is false. Mouse has no Alt state, so the lock is not shown
/// and an anchored gump does not close or detach on right-click.
/// </summary>
public abstract class AnchorableGump : Gump {
    const ushort LockGraphic = 0x082C;
    const int PreviewFill = 0x57400005;
    const int PreviewFrame = 0x57400006;

    // Profile path is not ported. Defaults match Profile.cs.
    static readonly bool HoldAltToMoveGumps = false;
    static readonly bool HoldDownKeyAltToCloseAnchored = true;
    static readonly bool CloseAllAnchoredGumpsInGroupWithRightClick = false;

    // ClassicUO Keyboard.Alt. Mouse has no key state.
    static readonly bool Alt = false;

    // r = g = b = 192, opaque. BGRA1555 has no partial alpha, so the 0.5 silver preview is solid.
    static readonly byte[] SilverPx = { 0x18, 0xE3 };
    static readonly byte[] GrayPx = { 0x10, 0xC2 };

    /// <summary>The one anchor list. UIManager has no AnchorManager property.</summary>
    public static readonly AnchorManager Anchors = new AnchorManager();

    Gump _anchorCandidate = null;
    int _grabX, _grabY;
    bool _down;
    ANCHOR_TYPE _anchorType = ANCHOR_TYPE.NONE;
    int _groupMatrixWidth, _groupMatrixHeight;
    int _widthMultiplier = 1;
    int _heightMultiplier = 1;

    protected AnchorableGump(World world, uint local, uint server) : base(world, local, server) {
        _anchorCandidate = null;
        _grabX = 0;
        _grabY = 0;
        _down = false;
    }

    public ANCHOR_TYPE AnchorType {
        get => _anchorType;
        protected set { _anchorType = value; SyncAnchor(); }
    }

    public virtual int GroupMatrixWidth {
        get => _groupMatrixWidth;
        protected set { _groupMatrixWidth = value; SyncAnchor(); }
    }

    public virtual int GroupMatrixHeight {
        get => _groupMatrixHeight;
        protected set { _groupMatrixHeight = value; SyncAnchor(); }
    }

    public int WidthMultiplier {
        get => _widthMultiplier;
        protected set { _widthMultiplier = value < 1 ? 1 : value; SyncAnchor(); }
    }

    public int HeightMultiplier {
        get => _heightMultiplier;
        protected set { _heightMultiplier = value < 1 ? 1 : value; SyncAnchor(); }
    }

    public bool ShowLock => Alt && Anchors[this] != null;

    public override void Update() {
        if (IsDisposed) return;
        var mine = CanMove && Mouse.LButtonPressed && UIManager.LastControlMouseDown(1) == this;
        if (mine) {
            if (!_down) {
                _down = true;
                _grabX = X;
                _grabY = Y;
                Anchors[this]?.MakeTopMost();
            }
            var off = Mouse.LDragOffset;
            var nx = _grabX + off.X;
            var ny = _grabY + off.Y;
            if (nx != X || ny != Y) {
                // Profile path is not ported.
                if (Alt && !HoldAltToMoveGumps) Anchors.DetachControl(this);
                else Anchors[this]?.UpdateLocation(this, nx - X, ny - Y);
                X = nx;
                Y = ny;
                var under = Anchors.GetAnchorableControlUnder(this);
                _anchorCandidate = under == this ? null : under;
            }
        }
        else if (_down) {
            _down = false;
            Attache();
        }
        base.Update();
    }

    public override bool Draw(int x, int y) {
        if (!base.Draw(x, y)) return false;
        DrawLock(x, y);
        DrawPreview();
        return true;
    }

    public override void OnMouseClick(int x, int y) {
        if (!ShowLock) return;
        if (!GumpMetrics.TrySize(LockGraphic, out var w, out var h) || w <= 0 || h <= 0) return;
        // UIManager passes screen coordinates.
        var lx = x - X;
        var ly = y - Y;
        if (lx >= Width - w && lx < Width && ly >= 0 && ly <= h)
            Anchors.DetachControl(this);
    }

    public override void CloseWithRightClick() {
        // Profile path is not ported.
        var group = Anchors[this];
        if (group == null || Alt || !HoldDownKeyAltToCloseAnchored) {
            if (group != null && CloseAllAnchoredGumpsInGroupWithRightClick)
                Anchors.DisposeAllControls(this);
            base.CloseWithRightClick();
        }
    }

    public override void Dispose() {
        if (!IsDisposed) Anchors.DetachControl(this);
        base.Dispose();
    }

    public void TryAttacheToExist() {
        var under = Anchors.GetAnchorableControlUnder(this);
        _anchorCandidate = under == this ? null : under;
        Attache();
    }

    void Attache() {
        if (_anchorCandidate == null) return;
        var drop = Anchors.GetCandidateDropLocation(this, _anchorCandidate);
        X = drop.X;
        Y = drop.Y;
        Anchors.DropControl(this, _anchorCandidate);
        _anchorCandidate = null;
    }

    void SyncAnchor() {
        if (_anchorType == ANCHOR_TYPE.NONE || IsDisposed) return;
        Anchors.SetAnchorId(this, (int)_anchorType, _widthMultiplier, _heightMultiplier, _groupMatrixWidth, _groupMatrixHeight);
    }

    void DrawLock(int x, int y) {
        if (!ShowLock) return;
        if (!GumpMetrics.TrySize(LockGraphic, out var w, out var h) || w <= 0 || h <= 0) return;
        var hue = Over(this) ? (ushort)34 : (ushort)0;
        GumpArtDraw.Pic(LockGraphic, x + (Width - w), y, w, h, false, hue);
    }

    void DrawPreview() {
        if (_anchorCandidate == null || Width < 1 || Height < 1) return;
        Point drop = Anchors.GetCandidateDropLocation(this, _anchorCandidate);
        if (drop.X == X && drop.Y == Y) return;
        var sink = GumpArt.Sink;
        if (sink == null) return;
        sink.DrawRaw(PreviewFill, drop.X, drop.Y, Width, Height, SilverPx, 1, 1);
        Frame(sink, PreviewFrame, GrayPx, drop.X, drop.Y, Width, Height);
        Frame(sink, PreviewFrame, GrayPx, drop.X + 1, drop.Y + 1, Width - 2, Height - 2);
    }

    protected static bool Over(Control root) {
        if (root == null) return false;
        for (var c = UIManager.MouseOverControl; c != null; c = c.Parent)
            if (ReferenceEquals(c, root)) return true;
        return false;
    }

    static void Frame(IGumpArt sink, int key, byte[] px, int x, int y, int w, int h) {
        if (w < 1 || h < 1) return;
        sink.DrawRaw(key, x, y, w, 1, px, 1, 1);
        if (h > 1) sink.DrawRaw(key, x, y + h - 1, w, 1, px, 1, 1);
        sink.DrawRaw(key, x, y, 1, h, px, 1, 1);
        if (w > 1) sink.DrawRaw(key, x + w - 1, y, 1, h, px, 1, 1);
    }
}
