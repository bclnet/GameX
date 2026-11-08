// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO HealthLinesManager. SpriteBatch is not called. The last bar is
/// BarX, BarY, BarWidth, BarHeight, and BarHue. Alpha is BarAlpha.
/// </summary>
public sealed class HealthLinesManager {
    const int BAR_WIDTH = 34;
    const int BAR_HEIGHT = 8;
    const int BAR_WIDTH_HALF = BAR_WIDTH >> 1;
    const int BAR_HEIGHT_HALF = BAR_HEIGHT >> 1;
    const ushort BACKGROUND_GRAPHIC = 0x1068;
    const ushort HP_GRAPHIC = 0x1069;
    const int OBJECT_HANDLES_GUMP_HEIGHT = 18;
    const int OBJECT_HANDLES_HP_BAR_HEIGHT = 3;

    // Profile path is not ported. Defaults match Profile.cs.
    bool _showMobilesHP = false;
    int _mobileHPType = 0;
    int _mobileHPShowWhen = 0;
    bool _useNewTargetSystem = true;
    bool _nameOverheadShowHpBar = true;
    bool _handlesDisplaying = false;
    uint _lastTarget = 0, _selectedTarget = 0, _newTargetSerial = 0;
    readonly List<UOMobileState> _mobiles = [];
    readonly List<UOItemState> _items = [];
    readonly List<UOEquipState> _equip = [];
    readonly World _world;

    public int BarX, BarY, BarWidth, BarHeight;
    public ushort BarHue;
    public float BarAlpha = 1f;

    public HealthLinesManager(World world) { _world = world; }

    public bool IsEnabled {
        get {
            // Profile path is not ported.
            return _showMobilesHP;
        }
    }

    public void Draw(uint serial, int hits, int maxHits, byte flags, NotorietyFlag notoriety) {
        // Profile path is not ported.
        var mode = _mobileHPType;
        if (mode < 0) return;
        var showWhen = _mobileHPShowWhen;
        var useNewTargetSystem = _useNewTargetSystem;
        var isEnabled = IsEnabled;
        var version = UltimaOnline.Current?.Version ?? default;
        var sa = version >= ClientVersion.CV_7000;
        var newTargSystem = false;
        var forceDraw = false;
        var passive = serial != UOLoginNet.PlayerSerial;
        if (_lastTarget == serial || UOLoginNet.LastAttack == serial || _selectedTarget == serial || _newTargetSerial == serial) {
            newTargSystem = useNewTargetSystem && _newTargetSerial == serial;
            passive = false;
            forceDraw = true;
        }
        if (!newTargSystem) {
            if (maxHits == 0) return;
            if (showWhen == 1 && hits == maxHits) return;
        }
        Place(serial, out var sx, out var sy, out var body, out var mounted);
        var dead = body == 0x0192 || body == 0x0193 || (body >= 0x025F && body <= 0x0260) || body == 0x02B6 || body == 0x02B7 || UOLoginNet.IsDead(serial);
        var gargoyle = (sa && body == 0x029A) || body == 0x029B;
        var flying = sa && (flags & 0x04) != 0;
        var pX = sx;
        var pY = sy;
        var offsetY = 0;
        var cam = Mouse.Current;
        var camW = cam.BufW > 0 ? cam.BufW : 640;
        var camH = cam.BufH > 0 ? cam.BufH : 480;
        if (isEnabled && mode != 1 && !dead && ((showWhen == 2 && hits != maxHits) || showWhen <= 1) && HitsPercentage(hits, maxHits) != 0) {
            var centerY = 0;
            var height = 0;
            var p1X = pX;
            var p1Y = pY - (height + centerY + 8 + 22);
            if (gargoyle && flying) p1Y -= 22;
            else if (!mounted) p1Y += 22;
            const int hitsTextW = 0;
            const int hitsTextH = 0;
            p1X -= (hitsTextW >> 1) + 5;
            p1Y -= hitsTextH;
            if (_handlesDisplaying) {
                var ohHeight = OBJECT_HANDLES_GUMP_HEIGHT + (_nameOverheadShowHpBar ? OBJECT_HANDLES_HP_BAR_HEIGHT + 1 : 0);
                p1Y -= ohHeight + 5;
                offsetY += ohHeight + 5;
            }
            if (!(p1X < 0 || p1X > camW - hitsTextW || p1Y < 0 || p1Y > camH))
                Drawn(p1X, p1Y, hitsTextW, hitsTextH);
            if (newTargSystem) offsetY += hitsTextH;
        }
        pX -= 5;
        pX -= BAR_WIDTH_HALF;
        pY -= BAR_HEIGHT_HALF;
        if (pX < 0 || pX > camW - BAR_WIDTH) return;
        if (pY < 0 || pY > camH - BAR_HEIGHT) return;
        if ((isEnabled && mode >= 1) || newTargSystem || forceDraw)
            DrawHealthLine(serial, hits, maxHits, flags, notoriety, pX, pY, offsetY, passive, newTargSystem, version);
    }

    void DrawHealthLine(uint serial, int hits, int maxHits, byte flags, NotorietyFlag notoriety, int x, int y, int offsetY, bool passive, bool newTargetSystem, ClientVersion version) {
        if (serial == 0) return;
        var per = BAR_WIDTH * HitsPercentage(hits, maxHits) / 100;
        var alpha = passive && !newTargetSystem ? 0.5f : 1f;
        var hue = Notoriety.GetHue(notoriety);
        const int MULTIPLER = 1;
        if (newTargetSystem && serial != UOLoginNet.PlayerSerial) {
            var centerY = 0;
            var width = 0;
            var height = 0;
            uint gumpHue = 0x7572;
            if (notoriety == NotorietyFlag.Innocent) gumpHue = 0x7570;
            else if (notoriety == NotorietyFlag.Ally) gumpHue = 0x7571;
            else if (notoriety == NotorietyFlag.Criminal || notoriety == NotorietyFlag.Gray) gumpHue = 0x7572;
            else if (notoriety == NotorietyFlag.Enemy) gumpHue = 0x7573;
            if (notoriety == NotorietyFlag.Invulnerable) gumpHue = 0x7575;
            else if (notoriety == NotorietyFlag.Murderer) gumpHue = 0x7577;
            uint topGump, bottomGump;
            if (width >= 80) {
                topGump = 0x756D;
                bottomGump = 0x756A;
            }
            else if (width >= 40) {
                topGump = 0x756E;
                bottomGump = 0x756B;
            }
            else {
                topGump = 0x756F;
                bottomGump = 0x756C;
            }
            GumpSize(gumpHue, out var hueW, out var hueH);
            var targetX = x + BAR_WIDTH_HALF - hueW / 2f;
            var topTargetY = height + centerY + 8 + 22 + offsetY;
            GumpSize(topGump, out var topW, out var topH);
            Drawn(targetX, y - topTargetY, topW, topH);
            Drawn(targetX, y - topTargetY, hueW, hueH);
            y += 7 + topH / 2 - centerY;
            GumpSize(bottomGump, out var botW, out var botH);
            Drawn(targetX, y - 1 - botH / 2f, botW, botH);
        }
        GumpSize(BACKGROUND_GRAPHIC, out var bgW, out var bgH);
        Remember(x, y, bgW * MULTIPLER, bgH * MULTIPLER, hue, alpha);
        if (hits != maxHits || maxHits == 0) {
            var offset = 2;
            if ((per >> 2) == 0) offset = per;
            GumpSize(HP_GRAPHIC, out _, out var hpH);
            Remember(x + per * MULTIPLER - offset, y, (BAR_WIDTH - per) * MULTIPLER - offset / 2, hpH * MULTIPLER, 0x21, alpha);
        }
        hue = 90;
        if (per > 0) {
            bool poisoned;
            if (version >= ClientVersion.CV_7000) poisoned = UOLoginNet.TryHealthBar(serial, out var saPoison, out _) && saPoison;
            else poisoned = (flags & 0x04) != 0;
            if (poisoned) hue = 63;
            else if ((flags & 0x08) != 0) hue = 53;
            GumpSize(HP_GRAPHIC, out _, out var fillH);
            Remember(x, y, per * MULTIPLER, fillH * MULTIPLER, hue, alpha);
        }
    }

    void Place(uint serial, out int sx, out int sy, out ushort body, out bool mounted) {
        var px = UOLoginNet.PlayerX;
        var py = UOLoginNet.PlayerY;
        var pz = UOLoginNet.PlayerZ;
        var x = 0;
        var y = 0;
        var z = 0;
        body = 0;
        var found = false;
        UOLoginNet.CopyActors(_mobiles, _items);
        for (var i = 0; i < _mobiles.Count; i++) {
            var m = _mobiles[i];
            if (m.Serial != serial) continue;
            x = m.X;
            y = m.Y;
            z = m.Z;
            body = m.Body;
            found = true;
            break;
        }
        if (!found && _world.Mobiles.TryGetValue(serial, out var obj) && obj is Mobile mob) {
            x = mob.X;
            y = mob.Y;
            z = mob.Z;
            body = mob.Body;
            found = true;
        }
        if (!found && serial == UOLoginNet.PlayerSerial) {
            x = px;
            y = py;
            z = pz;
            body = UOLoginNet.PlayerBody;
        }
        WorldGump.ViewCenter(out var cx, out var cy);
        var dx = x - px;
        var dy = y - py;
        var dz = z - pz;
        var offX = 0;
        var offY = 0;
        var offZ = 0;
        var realX = cx + (dx - dy) * 22 - 22;
        var realY = cy + (dx + dy) * 22 - (dz << 2) - 22;
        sx = realX + offX + 22 + 5;
        sy = realY + (offY - offZ) + 22 + 5;
        mounted = false;
        UOLoginNet.CopyEquip(serial, _equip);
        for (var i = 0; i < _equip.Count; i++) {
            if (_equip[i].Layer != 0x19 || _equip[i].Graphic == 0xFFFF) continue;
            mounted = true;
            break;
        }
    }

    void Remember(int x, int y, int w, int h, ushort hue, float alpha) {
        BarX = x;
        BarY = y;
        BarWidth = w;
        BarHeight = h;
        BarHue = hue;
        BarAlpha = alpha;
    }

    static int HitsPercentage(int hits, int maxHits) {
        if (hits <= 0 || maxHits <= 0) return 0;
        var perc = hits * 100 / maxHits;
        if (perc > 100) return 100;
        if (perc < 0) return 0;
        return perc;
    }

    static void GumpSize(uint graphic, out int w, out int h) {
        w = h = 0;
        if (graphic <= ushort.MaxValue) GumpMetrics.TrySize((ushort)graphic, out w, out h);
    }

    static void Drawn(float x, float y, int w, int h) { }
}
