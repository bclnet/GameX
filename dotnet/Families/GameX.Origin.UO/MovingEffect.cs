// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Clients.UO;
using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO MovingEffect. Speed is 100 * (1 + speed) pixels per second (ITEM_EFFECT_ANIMATION_DELAY is 50), plus 22 on X.
/// Removed at screen distance 22, on the target tile, when the source is gone, or past view range. No rotation: ClassicUO stores fixedDirection and never reads it.
/// Explode is CreateExplosionEffect: graphic 0x36CB at the target for 400 ms. Type 1 is lightning, 2 and 3 stay, 5 drifts by 8,8 every 20 ms, 4 is ignored.
/// </summary>
public static class MovingEffect {
    const byte Moving = 0;
    const byte Lightning = 1;
    const byte FixedXyz = 2;
    const byte FixedFrom = 3;
    const byte Drag = 5;
    const int Delay = 50;
    const int MaxView = 24;
    const ushort BoomGraphic = 0x36CB;
    const int KeyBase = 0x53000000;

    sealed class Effect {
        public byte Kind;
        public ushort Graphic, Hue;
        public uint Watch, Target;
        public bool Follow, FollowTarget, Explode;
        public int X, Y, Z, Tx, Ty, Tz;
        public float OffX, OffY, Interval;
        public int Until, Next;
    }

    static readonly List<Effect> Live = [];
    static readonly List<UOGraphicEffect> Pending = [];
    static readonly List<UOMobileState> Mobiles = [];
    static readonly List<UOItemState> Items = [];
    static readonly Dictionary<long, int> Slots = [];
    static readonly Dictionary<int, byte[]> Hued = [];
    static int _tick;

    public static void Spawn(byte type, uint source, uint target, ushort graphic, ushort hue, ushort srcX, ushort srcY, sbyte srcZ, ushort targetX, ushort targetY, sbyte targetZ, byte speed, byte duration, bool fixedDirection, bool explode) {
        if (type > Drag || (type > FixedFrom && type != Drag)) return;
        if (type != Lightning && graphic == 0) return;
        if ((type == Moving || type == Drag) && speed == 0) speed++;
        if (hue != 0) hue++;
        Refresh();
        var fx = new Effect { Kind = type, Graphic = graphic, Hue = hue, Until = int.MinValue };
        if (type == Lightning) {
            fx.Kind = FixedXyz;
            fx.Graphic = 0x4E20;
            fx.Until = unchecked(Environment.TickCount + 400);
            Anchor(fx, source, srcX, srcY, srcZ, true);
        }
        else if (type == FixedXyz) {
            fx.Until = Deadline(duration);
            fx.X = srcX;
            fx.Y = srcY;
            fx.Z = srcZ;
        }
        else if (type == FixedFrom) {
            fx.Kind = FixedXyz;
            fx.Until = Deadline(duration);
            Anchor(fx, source, srcX, srcY, srcZ, true);
        }
        else if (type == Drag) {
            fx.Until = Deadline(duration);
            Anchor(fx, source, srcX, srcY, srcZ, false);
        }
        else {
            fx.Interval = 100f * (1 + speed);
            fx.OffX = 22;
            fx.Explode = explode;
            Anchor(fx, source, srcX, srcY, srcZ, true);
            Aim(fx, target, targetX, targetY, targetZ);
        }
        Live.Add(fx);
    }

    public static void Draw(int originX, int originY) {
        UOLoginNet.TakeGraphics(Pending);
        for (var i = 0; i < Pending.Count; i++) {
            var e = Pending[i];
            Spawn(e.Type, e.Source, e.Target, e.Graphic, e.Hue, e.SrcX, e.SrcY, e.SrcZ, e.TargetX, e.TargetY, e.TargetZ, e.Speed, e.Duration, e.FixedDirection, e.Explode);
        }
        Pending.Clear();
        var now = Environment.TickCount;
        var dt = _tick == 0 ? 0f : unchecked(now - _tick) / 1000f;
        _tick = now;
        if (Live.Count == 0) return;
        Refresh();
        var px = UOLoginNet.PlayerX;
        var py = UOLoginNet.PlayerY;
        var pz = UOLoginNet.PlayerZ;
        var range = UOLoginNet.ViewRange;
        if (range == 0) range = MaxView;
        for (var i = Live.Count - 1; i >= 0; i--) {
            var fx = Live[i];
            if (fx.Until != int.MinValue && unchecked(now - fx.Until) >= 0) {
                Live.RemoveAt(i);
                continue;
            }
            var end = Advance(fx, now, dt, px, py, pz, range);
            if (end == 0) continue;
            Live.RemoveAt(i);
            if (end == 2) Boom(fx);
        }
        var sink = GumpArt.Sink;
        var assets = UltimaOnline.Current?.Assets;
        if (sink == null || assets == null) return;
        for (var i = 0; i < Live.Count; i++) {
            var fx = Live[i];
            if (!TryPixels(assets, fx, out var key, out var pixels, out var w, out var h)) continue;
            var dx = fx.X - px;
            var dy = fx.Y - py;
            var sx = (dx - dy) * 22 + (int)fx.OffX;
            var sy = (dx + dy) * 22 - (fx.Z - pz) * 4 + (int)fx.OffY;
            sink.DrawRaw(key, originX + 320 + sx - w / 2, originY + 220 + sy - h + 22, w, h, pixels, w, h);
        }
    }

    static int Deadline(byte duration) => duration == 0 ? int.MinValue : unchecked(Environment.TickCount + duration * Delay);

    static void Refresh() => UOLoginNet.CopyActors(Mobiles, Items);

    static void Anchor(Effect fx, uint serial, int x, int y, int z, bool follow) {
        if (serial > 0 && serial < 0x80000000 && TryActor(serial, out var ax, out var ay, out var az)) {
            fx.Watch = serial;
            fx.Follow = follow;
            fx.X = ax;
            fx.Y = ay;
            fx.Z = az;
            return;
        }
        fx.X = x;
        fx.Y = y;
        fx.Z = z;
    }

    static void Aim(Effect fx, uint serial, int x, int y, int z) {
        if (serial > 0 && serial < 0x80000000 && TryActor(serial, out var ax, out var ay, out var az)) {
            fx.Target = serial;
            fx.FollowTarget = true;
            fx.Tx = ax;
            fx.Ty = ay;
            fx.Tz = az;
            return;
        }
        fx.Tx = x;
        fx.Ty = y;
        fx.Tz = z;
    }

    static bool TryActor(uint serial, out int x, out int y, out int z) {
        if (serial != 0 && serial == UOLoginNet.PlayerSerial) {
            x = UOLoginNet.PlayerX;
            y = UOLoginNet.PlayerY;
            z = UOLoginNet.PlayerZ;
            return true;
        }
        if (serial != 0 && serial < 0x40000000) {
            for (var i = 0; i < Mobiles.Count; i++)
                if (Mobiles[i].Serial == serial) {
                    x = Mobiles[i].X;
                    y = Mobiles[i].Y;
                    z = Mobiles[i].Z;
                    return true;
                }
        }
        else if (serial >= 0x40000000 && serial < 0x80000000)
            for (var i = 0; i < Items.Count; i++)
                if (Items[i].Serial == serial) {
                    x = Items[i].X;
                    y = Items[i].Y;
                    z = Items[i].Z;
                    return true;
                }
        x = y = z = 0;
        return false;
    }

    /// <summary>0 stays, 1 is removed, 2 is removed and explodes.</summary>
    static int Advance(Effect fx, int now, float dt, int px, int py, int pz, int range) {
        if (fx.Watch != 0) {
            if (!TryActor(fx.Watch, out var x, out var y, out var z)) return 1;
            if (fx.Follow) { fx.X = x; fx.Y = y; fx.Z = z; }
        }
        if (fx.Kind == Drag) {
            if (unchecked(now - fx.Next) >= 0) {
                fx.OffX += 8;
                fx.OffY += 8;
                fx.Next = unchecked(now + 20);
            }
            return Away(fx, px, py, range) ? 1 : 0;
        }
        if (fx.Kind != Moving) return Away(fx, px, py, range) ? 1 : 0;
        if (fx.FollowTarget) {
            if (TryActor(fx.Target, out var x, out var y, out var z)) { fx.Tx = x; fx.Ty = y; fx.Tz = z; }
            else fx.FollowTarget = false;
        }
        var dx = fx.X - px;
        var dy = fx.Y - py;
        var dz = fx.Z - pz;
        var srcX = (dx - dy) * 22f + fx.OffX;
        var srcY = (dx + dy) * 22f - dz * 4f + fx.OffY;
        var tdx = fx.Tx - px;
        var tdy = fx.Ty - py;
        var ox = (tdx - tdy) * 22f - srcX;
        var oy = (tdx + tdy) * 22f - (fx.Tz - pz) * 4f - srcY;
        var distance = (float)Math.Sqrt(ox * ox + oy * oy);
        var step = fx.Interval * dt;
        float s0x, s0y;
        if (distance > step) {
            var scale = step / distance;
            s0x = ox * scale;
            s0y = oy * scale;
        }
        else {
            // ClassicUO assigns the target point, not the remaining offset, once the step would pass it.
            s0x = (tdx - tdy) * 22f;
            s0y = (tdx + tdy) * 22f - (fx.Tz - pz) * 4f;
        }
        if (distance <= 22f) return fx.Explode ? 2 : 1;
        var ofsX = (int)(srcX / 22f);
        var ofsY = (int)(srcY / 22f);
        TileOffsetOnMonitorToXY(ref ofsX, ref ofsY, out var coordX, out var coordY);
        var newX = px + coordX;
        var newY = py + coordY;
        if (newX == fx.Tx && newY == fx.Ty) return fx.Explode ? 2 : 1;
        if (newX != fx.X || newY != fx.Y) {
            fx.Follow = false;
            fx.Watch = 0;
            fx.X = (ushort)newX;
            fx.Y = (ushort)newY;
            fx.Z = (sbyte)fx.Z;
            var nextX = (coordX - coordY) * 22f;
            var nextY = (coordX + coordY) * 22f - dz * 4f;
            fx.OffX = srcX - nextX;
            fx.OffY = srcY - nextY;
        }
        fx.OffX += s0x;
        fx.OffY += s0y;
        return Away(fx, px, py, range) ? 1 : 0;
    }

    static bool Away(Effect fx, int px, int py, int range) => Math.Max(Math.Abs(fx.X - px), Math.Abs(fx.Y - py)) > range;

    static void Boom(Effect fx) {
        Live.Add(new Effect {
            Kind = FixedXyz,
            Graphic = BoomGraphic,
            Hue = fx.Hue,
            X = fx.Tx,
            Y = fx.Ty,
            Z = fx.Tz,
            Until = unchecked(Environment.TickCount + 400),
        });
    }

    static void TileOffsetOnMonitorToXY(ref int ofsX, ref int ofsY, out int x, out int y) {
        y = 0;
        if (ofsX == 0) x = y = ofsY >> 1;
        else if (ofsY == 0) {
            x = ofsX >> 1;
            y = -x;
        }
        else {
            var absX = Math.Abs(ofsX);
            var absY = Math.Abs(ofsY);
            x = ofsX;
            if (ofsY > ofsX) {
                if (ofsX < 0 && ofsY < 0) y = absX - absY;
                else if (ofsX > 0 && ofsY > 0) y = absY - absX;
            }
            else if (ofsX > ofsY) {
                if (ofsX < 0 && ofsY < 0) y = -(absY - absX);
                else if (ofsX > 0 && ofsY > 0) y = -(absX - absY);
            }
            if (y == 0 && ofsY != ofsX) y = ofsY < 0 ? -(absX + absY) : absX + absY;
            y /= 2;
            x += y;
        }
    }

    static bool TryPixels(Assets assets, Effect fx, out int key, out byte[] pixels, out int w, out int h) {
        key = 0;
        pixels = null;
        w = h = 0;
        var id = fx.Graphic + 0x4000;
        if (id > 0xFFFF || !assets.TryArtPixels(id, out var src, out w, out h) || w <= 0 || h <= 0) return false;
        if (src == null || src.Length < w * h * 2) return false;
        var partial = (fx.Hue & 0x8000) != 0;
        var index = fx.Hue & 0x7FFF;
        if (index != 0) index--;
        if (index != 0 && assets.TryHueRamp(index, out var ramp)) {
            key = ArtKey(fx.Graphic, index, partial);
            if (!Hued.TryGetValue(key, out pixels)) Hued[key] = pixels = Paint(src, w, h, ramp, partial);
            return pixels != null;
        }
        key = ArtKey(fx.Graphic, 0, false);
        pixels = src;
        return true;
    }

    /// <summary>0x53000000 plus a stable slot. Land, static, body, and cursor keys stay clear of it.</summary>
    static int ArtKey(ushort graphic, int hueIndex, bool partial) {
        var id = (long)graphic | ((long)hueIndex << 16) | (partial ? 1L << 32 : 0);
        if (!Slots.TryGetValue(id, out var n)) {
            n = Slots.Count;
            Slots[id] = n;
        }
        return KeyBase + n;
    }

    /// <summary>ClassicUO's effect shader. Not partial: the green channel indexes the ramp. Partial: only a gray pixel, and it uses red.</summary>
    static byte[] Paint(byte[] src, int w, int h, ushort[] ramp, bool partial) {
        var count = w * h;
        var dest = new byte[count * 2];
        for (var i = 0; i < count; i++) {
            var c = (ushort)(src[i * 2] | (src[i * 2 + 1] << 8));
            if (c != 0) {
                var r = (c >> 10) & 31;
                var g = (c >> 5) & 31;
                var b = c & 31;
                if (!partial) {
                    c = ramp[g];
                    if (c != 0) c |= 0x8000;
                }
                else if (r == g && g == b) {
                    c = ramp[r];
                    if (c != 0) c |= 0x8000;
                }
            }
            dest[i * 2] = (byte)c;
            dest[i * 2 + 1] = (byte)(c >> 8);
        }
        return dest;
    }
}
