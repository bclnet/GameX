// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO NameOverHeadHandlerGump. Stay Active writes NameOverHeadManager.IsToggled.
/// All sets every flag or none. Each other box sets one NameOverheadTypeAllowed bit.
/// The fill is the opaque dark pixel. The boxes are 0x00D2 and 0x00D3.
/// Right-click does not close this gump. There is no drag, so the last position is the last X and Y.
/// </summary>
public class NameOverHeadHandlerGump : Gump {
    const ushort BoxOff = 0x00D2;
    const ushort BoxOn = 0x00D3;
    const ushort Hue = 0x0481;
    const int FillKey = 0x57410010;

    // r = g = b = 4, opaque. Stand-in for AlphaBlendControl(0.7).
    static readonly byte[] Dark = { 0x84, 0x90 };

    static readonly string[] Labels = {
        "Items", "Corpses", "Innocent", "Ally", "Gray", "Criminal", "Enemy", "Murderer", "Invulnerable"
    };
    static readonly NameOverheadTypeAllowed[] Flags = {
        NameOverheadTypeAllowed.Items,
        NameOverheadTypeAllowed.Corpses,
        NameOverheadTypeAllowed.Innocent,
        NameOverheadTypeAllowed.Ally,
        NameOverheadTypeAllowed.Gray,
        NameOverheadTypeAllowed.Criminal,
        NameOverheadTypeAllowed.Enemy,
        NameOverheadTypeAllowed.Murderer,
        NameOverheadTypeAllowed.Invulnerable
    };

    static NameOverHeadManager _manager;
    static bool _hasLast;
    static int _lastX = 100;
    static int _lastY = 100;

    public NameOverHeadHandlerGump(World world) : base(world, 0, 0) {
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = false;
        WantUpdateSize = false;
        LayerOrder = UILayer.Over;
        X = _hasLast ? _lastX : 100;
        Y = _hasLast ? _lastY : 100;
        var mgr = Manager(world);
        var allowed = mgr.TypeAllowed;
        var suppress = false;
        var y = 0;
        var max = 80;
        var stay = Box("Stay Active", mgr.IsToggled, 0);
        if (stay != null) {
            y = stay.Height > 0 ? stay.Height : 16;
            if (stay.Width > max) max = stay.Width;
            stay.ValueChanged += (s, e) => mgr.IsToggled = stay.IsChecked;
            Add(stay);
        }
        var all = Box("All", allowed == NameOverheadTypeAllowed.All, y);
        var boxes = new Checkbox[Flags.Length];
        if (all != null) {
            y += all.Height > 0 ? all.Height : 16;
            if (all.Width > max) max = all.Width;
            Add(all);
        }
        for (var i = 0; i < Flags.Length; i++) {
            var cb = Box(Labels[i], allowed.HasFlag(Flags[i]), y);
            boxes[i] = cb;
            if (cb == null) continue;
            y += cb.Height > 0 ? cb.Height : 16;
            if (cb.Width > max) max = cb.Width;
            Add(cb);
        }
        Width = max + 4;
        Height = y > 16 ? y : 16;
        if (all != null) {
            all.ValueChanged += (s, e) => {
                if (suppress) return;
                suppress = true;
                var on = all.IsChecked;
                for (var i = 0; i < boxes.Length; i++)
                    if (boxes[i] != null) boxes[i].IsChecked = on;
                mgr.TypeAllowed = on ? NameOverheadTypeAllowed.All : NameOverheadTypeAllowed.None;
                suppress = false;
            };
        }
        for (var i = 0; i < boxes.Length; i++) {
            var cb = boxes[i];
            if (cb == null) continue;
            var flag = Flags[i];
            cb.ValueChanged += (s, e) => {
                if (suppress) return;
                suppress = true;
                if (cb.IsChecked) mgr.TypeAllowed |= flag;
                else mgr.TypeAllowed &= ~flag;
                if (all != null) all.IsChecked = mgr.TypeAllowed == NameOverheadTypeAllowed.All;
                suppress = false;
            };
        }
    }

    /// <summary>One manager. The profile default is All. A later change is kept.</summary>
    public static NameOverHeadManager Manager(World world) {
        if (_manager == null) {
            _manager = new NameOverHeadManager(world);
            // Profile path is not ported.
            _manager.TypeAllowed = NameOverheadTypeAllowed.All;
        }
        return _manager;
    }

    /// <summary>True while Stay Active is on, or while this gump is on screen.</summary>
    public static bool Showing() {
        if (_manager != null && _manager.IsToggled) return true;
        var open = UIManager.GetGump<NameOverHeadHandlerGump>();
        return open != null && !open.IsDisposed && open.IsVisible;
    }

    /// <summary>One gump. Stay Active hides it. Otherwise a second call disposes it.</summary>
    public static void Toggle(World world) {
        var open = UIManager.GetGump<NameOverHeadHandlerGump>();
        if (open != null && !open.IsDisposed) {
            if (!open.IsVisible) {
                open.IsVisible = true;
                return;
            }
            if (Manager(world).IsToggled) open.IsVisible = false;
            else open.Dispose();
            return;
        }
        UIManager.Add(new NameOverHeadHandlerGump(world));
    }

    public override void Update() {
        if (IsDisposed) return;
        _lastX = X;
        _lastY = Y;
        _hasLast = true;
        base.Update();
        NameOverheadGump.Sync(World);
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (Width > 0 && Height > 0)
            GumpArt.Sink?.DrawRaw(FillKey, x, y, Width, Height, Dark, 1, 1);
        return base.Draw(x, y);
    }

    static Checkbox Box(string label, bool on, int y) {
        var cb = new Checkbox(BoxOff, BoxOn, label, 1, Hue, true) { IsChecked = on, X = 2, Y = y };
        return cb.IsDisposed ? null : cb;
    }
}
