// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO NameOverheadGump. One plate per serial. LocalSerial is that serial and ServerSerial stays 0.
/// The plate is a solid pixel, not a gump graphic: ClassicUO uses AlphaBlendControl. BGRA1555 has no
/// partial alpha, so the 0.7 fill is the same opaque dark pixel as InfoBarGump.
/// A mobile hue is Notoriety.GetHue. An item hue is 0x0481. Text is clipped to 100 pixels.
/// The hit bar is drawn when a status has HitsMax. Profile NameOverheadShowHpBar is not read.
/// Animation height is not applied. The plate sits 36 pixels above the tile center.
/// A right-click closes that serial until the handles turn off. 0xD6 is not requested.
/// HealthBarGump already draws the hits line, so a drag does not open another bar.
/// </summary>
public class NameOverheadGump : Gump {
    const int PlateW = 104;
    const int PlateH = 22;
    const int BarH = 3;
    const int TextW = 100;
    const byte Font = 1;
    const int Above = 36;
    const int FillKey = 0x57410001;
    const int BlackKey = 0x57410002;
    const int GreenKey = 0x57410003;
    const int YellowGreenKey = 0x57410004;
    const int YellowKey = 0x57410005;
    const int RedKey = 0x57410006;

    // r = g = b = 4, opaque. Stand-in for AlphaBlendControl(0.7).
    static readonly byte[] Dark = { 0x84, 0x90 };
    static readonly byte[] Black = { 0x00, 0x80 };
    static readonly byte[] Green = { 0xE0, 0x83 };
    static readonly byte[] YellowGreen = { 0xE0, 0xC3 };
    static readonly byte[] Yellow = { 0xE0, 0xFF };
    static readonly byte[] Red = { 0x00, 0xFC };

    static readonly List<UOMobileState> Mobiles = new List<UOMobileState>();
    static readonly List<UOItemState> Items = new List<UOItemState>();
    static readonly List<NameOverheadGump> Live = new List<NameOverheadGump>();
    static readonly HashSet<uint> Want = new HashSet<uint>();
    static readonly HashSet<uint> Closed = new HashSet<uint>();
    static int _copiedTick = int.MinValue;
    static int _syncTick = int.MinValue;

    string _text = "";
    ushort _hue;
    int _hp;
    int _ax, _ay, _az;
    bool _showHp;

    public NameOverheadGump(World world, uint serial) : base(world, serial, 0) {
        CanMove = false;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        WantUpdateSize = false;
        LayerOrder = UILayer.Over;
        Width = PlateW;
        Height = PlateH;
        if (!Alive()) Dispose();
    }

    /// <summary>Plates for every allowed mobile and item while the handler is open or Stay Active is on.</summary>
    public static void Sync(World world) {
        if (world == null) return;
        var tick = Environment.TickCount;
        if (tick == _syncTick) return;
        _syncTick = tick;
        if (!NameOverHeadHandlerGump.Showing()) {
            Closed.Clear();
            Collect();
            for (var i = 0; i < Live.Count; i++) Live[i].Dispose();
            return;
        }
        var mgr = NameOverHeadHandlerGump.Manager(world);
        Copy();
        Want.Clear();
        for (var i = 0; i < Mobiles.Count; i++) {
            var m = Mobiles[i];
            if (m.Serial == 0 || Closed.Contains(m.Serial)) continue;
            if (!mgr.IsAllowed(m.Serial, (NotorietyFlag)m.Notoriety)) continue;
            Want.Add(m.Serial);
        }
        for (var i = 0; i < Items.Count; i++) {
            var it = Items[i];
            if (it.Serial == 0 || Closed.Contains(it.Serial)) continue;
            // IsAllowed treats every item as Items. A corpse is not a separate check.
            if (!mgr.IsAllowed(it.Serial, NotorietyFlag.Unknown)) continue;
            Want.Add(it.Serial);
        }
        Collect();
        for (var i = 0; i < Live.Count; i++)
            if (!Want.Contains(Live[i].LocalSerial)) Live[i].Dispose();
        foreach (var serial in Want) {
            var found = false;
            for (var i = 0; i < Live.Count; i++) {
                if (Live[i].IsDisposed || Live[i].LocalSerial != serial) continue;
                found = true;
                break;
            }
            if (!found) UIManager.Add(new NameOverheadGump(world, serial), false);
        }
    }

    public override void Update() {
        if (IsDisposed) return;
        Sync(World);
        if (IsDisposed) return;
        if (!Alive()) {
            Dispose();
            return;
        }
        IsVisible = SetName();
        if (IsVisible) Place();
        base.Update();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (!SetName()) return false;
        var sink = GumpArt.Sink;
        sink?.DrawRaw(FillKey, x, y, Width, Height, Dark, 1, 1);
        sink?.DrawRaw(BlackKey, x - 1, y - 1, Width + 2, 1, Black, 1, 1);
        sink?.DrawRaw(BlackKey, x - 1, y + Height, Width + 2, 1, Black, 1, 1);
        sink?.DrawRaw(BlackKey, x - 1, y, 1, Height, Black, 1, 1);
        sink?.DrawRaw(BlackKey, x + Width, y, 1, Height, Black, 1, 1);
        var tw = sink?.MeasureText(_text, Font) ?? _text.Length * 6;
        var tx = x + 2 + Math.Max(0, Width - tw - 4) / 2;
        sink?.DrawText(tx, y + 2, _text, _hue, Font);
        if (_showHp) {
            var by = y + Height - BarH;
            sink?.DrawRaw(BlackKey, x, by, Width, BarH, Black, 1, 1);
            var fill = Width * _hp / 100;
            if (fill > 0) {
                HpPixels(out var key, out var px);
                sink?.DrawRaw(key, x, by, fill, BarH, px, 1, 1);
            }
        }
        return true;
    }

    public override void OnMouseClick(int x, int y) {
        if (LocalSerial == 0) return;
        if (UOLoginNet.Holding) {
            UOLoginNet.DropHeld(0, 0, 0, LocalSerial);
            return;
        }
        if (UOLoginNet.HasTarget) {
            UOLoginNet.TargetSerial(LocalSerial);
            return;
        }
        UOLoginNet.RequestPopup(LocalSerial);
    }

    public override bool OnMouseDoubleClick(int x, int y) {
        if (LocalSerial == 0) return true;
        if (UOLoginNet.HasTarget || UOLoginNet.TargetedJustNow) {
            if (UOLoginNet.HasTarget) UOLoginNet.TargetSerial(LocalSerial);
            return true;
        }
        if (LocalSerial != UOLoginNet.PlayerSerial && UOLoginNet.IsMobile(LocalSerial) && UOLoginNet.InWarMode)
            UOLoginNet.Attack(LocalSerial);
        else UOLoginNet.Use(LocalSerial);
        return true;
    }

    public override void CloseWithRightClick() {
        if (!CanCloseWithRightClick || IsDisposed) return;
        Closed.Add(LocalSerial);
        base.CloseWithRightClick();
    }

    bool SetName() {
        if (!Read(out var mobile, out var graphic, out var noto)) return false;
        string t;
        if (mobile) {
            t = UOLoginNet.NameOf(LocalSerial);
            if (string.IsNullOrEmpty(t)) return false;
            _hue = Notoriety.GetHue(noto);
        }
        else {
            _hue = 0x0481;
            if (UOLoginNet.TryProperties(LocalSerial, out var prop, out _) && !string.IsNullOrEmpty(prop)) t = prop;
            else {
                t = "";
                var amount = UOLoginNet.AmountOf(LocalSerial);
                if (graphic != 0x2006 && amount > 1) t = amount + " ";
                var name = UOLoginNet.NameOf(LocalSerial);
                t += string.IsNullOrEmpty(name) ? graphic.ToString("X4") : name;
            }
            if (string.IsNullOrEmpty(t)) return false;
        }
        _text = Fit(t, TextW);
        Width = PlateW;
        // Profile path is not ported.
        _showHp = false;
        _hp = 0;
        if (mobile && UOLoginNet.TryStatus(LocalSerial, out var st) && st.HitsMax > 0) {
            _showHp = true;
            _hp = st.Hits * 100 / st.HitsMax;
            if (_hp > 100) _hp = 100;
            if (_hp < 0) _hp = 0;
        }
        Height = PlateH + (_showHp ? BarH + 1 : 0);
        WantUpdateSize = false;
        return true;
    }

    void Place() {
        if (!Read(out _, out _, out _)) return;
        var dx = _ax - UOLoginNet.PlayerX;
        var dy = _ay - UOLoginNet.PlayerY;
        var dz = _az - UOLoginNet.PlayerZ;
        WorldGump.ViewCenter(out var cx, out var cy);
        var sx = cx + (dx - dy) * 22;
        var sy = cy + (dx + dy) * 22 - dz * 4;
        X = sx - (Width >> 1);
        Y = sy - Height - Above;
    }

    bool Read(out bool mobile, out ushort graphic, out NotorietyFlag noto) {
        mobile = false;
        graphic = 0;
        noto = NotorietyFlag.Unknown;
        Copy();
        for (var i = 0; i < Mobiles.Count; i++) {
            if (Mobiles[i].Serial != LocalSerial) continue;
            mobile = true;
            graphic = Mobiles[i].Body;
            noto = (NotorietyFlag)Mobiles[i].Notoriety;
            _ax = Mobiles[i].X;
            _ay = Mobiles[i].Y;
            _az = Mobiles[i].Z;
            return true;
        }
        for (var i = 0; i < Items.Count; i++) {
            if (Items[i].Serial != LocalSerial) continue;
            graphic = Items[i].Graphic;
            _ax = Items[i].X;
            _ay = Items[i].Y;
            _az = Items[i].Z;
            return true;
        }
        return false;
    }

    bool Alive() => Read(out _, out _, out _);

    void HpPixels(out int key, out byte[] px) {
        if (_hp >= 80) { key = GreenKey; px = Green; return; }
        if (_hp >= 50) { key = YellowGreenKey; px = YellowGreen; return; }
        if (_hp >= 30) { key = YellowKey; px = Yellow; return; }
        key = RedKey;
        px = Red;
    }

    static string Fit(string text, int maxPx) {
        if (string.IsNullOrEmpty(text)) return "";
        var sink = GumpArt.Sink;
        if (sink == null) {
            var n = maxPx / 6;
            if (n < 1) n = 1;
            return text.Length <= n ? text : text.Substring(0, n);
        }
        if (sink.MeasureText(text, Font) <= maxPx) return text;
        var cut = text.Length;
        while (cut > 1 && sink.MeasureText(text.Substring(0, cut), Font) > maxPx) cut--;
        return text.Substring(0, cut);
    }

    static void Copy() {
        var tick = Environment.TickCount;
        if (tick == _copiedTick) return;
        _copiedTick = tick;
        UOLoginNet.CopyActors(Mobiles, Items);
    }

    static void Collect() {
        Live.Clear();
        foreach (var g in UIManager.Gumps)
            if (g is NameOverheadGump n && !n.IsDisposed) Live.Add(n);
    }
}
