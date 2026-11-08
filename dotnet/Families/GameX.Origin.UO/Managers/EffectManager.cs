// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO effect list. Packet duration is multiplied by 50. Fixed, lightning, drag, and moving
/// effects are one private node: serial, graphic, source, target, x, y, z, duration. Screen fade is ignored.
/// </summary>
public sealed class EffectManager : LinkedObject {
    const byte Moving = 0x00;
    const byte Lightning = 0x01;
    const byte FixedXyz = 0x02;
    const byte FixedFrom = 0x03;
    const byte ScreenFade = 0x04;
    const byte Drag = 0x05;
    const int Delay = 50;
    const int MaxView = 24;
    const ushort LightningGraphic = 0x4E20;
    const ushort BoomGraphic = 0x36CB;

    readonly World _world;
    readonly List<UOMobileState> _mobs = new List<UOMobileState>();
    readonly List<UOItemState> _items = new List<UOItemState>();

    public EffectManager(World world) {
        _world = world;
    }

    public void Update() {
        Refresh();
        var range = ViewRange();
        for (var f = (Effect)Items; f != null;) {
            var next = (Effect)f.Next;
            f.Update();
            if (!f.IsDestroyed && f.Distance > range) f.Destroy();
            f = next;
        }
    }

    public void CreateEffect(byte type, uint source, uint target, ushort graphic, ushort hue, ushort srcX, ushort srcY, sbyte srcZ, ushort targetX, ushort targetY, sbyte targetZ, byte speed, int duration, bool fixedDir, bool doesExplode, bool hasparticles, byte blendmode) {
        _ = fixedDir;
        _ = blendmode;
        if (hasparticles) {
            // ClassicUO logs an unhandled particle effect and still creates the node.
        }
        if (hue != 0) hue++;
        duration *= Delay;
        Effect effect;
        switch (type) {
            case Moving:
                if (graphic == 0) return;
                if (speed == 0) speed++;
                // MovingEffect passes 0, so the packet duration does not expire it.
                effect = Begin(Moving, source, target, graphic, hue, srcX, srcY, srcZ, targetX, targetY, targetZ, 0, doesExplode, true, false);
                break;
            case Drag:
                if (graphic == 0) return;
                if (speed == 0) speed++;
                effect = Begin(Drag, source, target, graphic, hue, srcX, srcY, srcZ, targetX, targetY, targetZ, duration, doesExplode, true, false);
                break;
            case Lightning:
                // LightningEffect is graphic 0x4E20 for 400 ms, not the packet duration.
                effect = Begin(Lightning, source, target, LightningGraphic, hue, srcX, srcY, srcZ, targetX, targetY, targetZ, 400, false, true, true);
                break;
            case FixedXyz:
                if (graphic == 0) return;
                effect = Begin(FixedXyz, source, target, graphic, hue, srcX, srcY, srcZ, targetX, targetY, targetZ, duration, false, false, false);
                break;
            case FixedFrom:
                if (graphic == 0) return;
                effect = Begin(FixedFrom, source, target, graphic, hue, srcX, srcY, srcZ, targetX, targetY, targetZ, duration, false, true, true);
                break;
            case ScreenFade:
                return;
            default:
                return;
        }
        PushToBack(effect);
    }

    public new void Clear() {
        var first = (Effect)Items;
        while (first != null) {
            var n = first.Next;
            first.Destroy();
            first = (Effect)n;
        }
        Items = null;
    }

    Effect Begin(byte kind, uint source, uint target, ushort graphic, ushort hue, int x, int y, int z, int tx, int ty, int tz, int duration, bool explode, bool track, bool follow) {
        var fx = new Effect(this) {
            Kind = kind,
            Serial = source,
            Graphic = graphic,
            Source = source,
            Target = target,
            Hue = hue,
            X = x,
            Y = y,
            Z = z,
            Tx = tx,
            Ty = ty,
            Tz = tz,
            Duration = duration > 0 ? (long)Time.Ticks + duration : -1,
            Explode = explode,
        };
        if (track && SerialHelper.IsValid(source) && TryActor(source, out var ax, out var ay, out var az)) {
            fx.Latched = true;
            fx.Follow = follow;
            fx.X = ax;
            fx.Y = ay;
            fx.Z = az;
        }
        if (SerialHelper.IsValid(target) && TryActor(target, out var bx, out var by, out var bz)) {
            fx.Tx = bx;
            fx.Ty = by;
            fx.Tz = bz;
        }
        return fx;
    }

    void Boom(ushort hue, int x, int y, int z) {
        PushToBack(Begin(FixedXyz, 0, 0, BoomGraphic, hue, x, y, z, x, y, z, 400, false, false, false));
    }

    /// <summary>0 until 0xC8. ClassicUO's client view range starts at 24.</summary>
    int ViewRange() {
        var range = UOLoginNet.ViewRange;
        return range == 0 ? MaxView : range;
    }

    void Refresh() => UOLoginNet.CopyActors(_mobs, _items);

    bool TryActor(uint serial, out int x, out int y, out int z) {
        if (serial != 0 && serial == UOLoginNet.PlayerSerial) {
            x = UOLoginNet.PlayerX;
            y = UOLoginNet.PlayerY;
            z = UOLoginNet.PlayerZ;
            return true;
        }
        if (serial != 0 && serial < 0x40000000) {
            for (var i = 0; i < _mobs.Count; i++) {
                if (_mobs[i].Serial != serial) continue;
                x = _mobs[i].X;
                y = _mobs[i].Y;
                z = _mobs[i].Z;
                return true;
            }
        }
        else if (serial >= 0x40000000 && serial < 0x80000000) {
            for (var i = 0; i < _items.Count; i++) {
                if (_items[i].Serial != serial) continue;
                x = _items[i].X;
                y = _items[i].Y;
                z = _items[i].Z;
                return true;
            }
        }
        if (_world.Mobiles.TryGetValue(serial, out var mobObj) && mobObj is Mobile mob) {
            x = mob.X;
            y = mob.Y;
            z = mob.Z;
            return true;
        }
        if (_world.Items.TryGetValue(serial, out var itemObj) && itemObj is Item item) {
            x = item.X;
            y = item.Y;
            z = item.Z;
            return true;
        }
        x = y = z = 0;
        return false;
    }

    sealed class Effect : LinkedObject {
        readonly EffectManager _manager;

        public Effect(EffectManager manager) => _manager = manager;

        public uint Serial;
        public ushort Graphic;
        public uint Source;
        public uint Target;
        public int X, Y, Z;
        public long Duration = -1;
        public bool IsDestroyed;
        public byte Kind;
        public ushort Hue;
        public int Tx, Ty, Tz;
        public bool Explode, Latched, Follow;

        public int Distance {
            get {
                if (UOLoginNet.PlayerSerial == 0) return ushort.MaxValue;
                var dx = X - UOLoginNet.PlayerX;
                var dy = Y - UOLoginNet.PlayerY;
                if (dx < 0) dx = -dx;
                if (dy < 0) dy = -dy;
                return dx > dy ? dx : dy;
            }
        }

        public void Update() {
            if (IsDestroyed) return;
            if (Latched) {
                if (!_manager.TryActor(Source, out var sx, out var sy, out var sz)) {
                    Destroy();
                    return;
                }
                if (Follow) {
                    X = sx;
                    Y = sy;
                    Z = sz;
                }
            }
            if (Kind == Moving && X == Tx && Y == Ty) {
                var hue = Hue;
                var tx = Tx;
                var ty = Ty;
                var tz = Tz;
                var explode = Explode;
                Destroy();
                if (explode) _manager.Boom(hue, tx, ty, tz);
                return;
            }
            if (Duration >= 0 && Duration < Time.Ticks) Destroy();
        }

        public void Destroy() {
            if (IsDestroyed) return;
            IsDestroyed = true;
            _manager.Remove(this);
        }
    }
}
