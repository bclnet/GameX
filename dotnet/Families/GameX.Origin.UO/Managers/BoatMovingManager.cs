// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO boat step queue. Slow is 1000 ms, normal 500, fast 250, and a speed above 4 is speed * 10.
/// MoveRequest sends the existing 0xBF 0x33. House tiles are not in this tree, so the pose stays here.
/// </summary>
public sealed class BoatMovingManager {
    const int SlowInterval = 1000;
    const int NormalInterval = 500;
    const int FastInterval = 250;
    const int CharacterDelay = 80;

    readonly Dictionary<uint, Deque<BoatStep>> _steps = new Dictionary<uint, Deque<BoatStep>>();
    readonly List<uint> _toRemove = new List<uint>();
    readonly Dictionary<uint, List<ItemInside>> _items = new Dictionary<uint, List<ItemInside>>();
    readonly Dictionary<uint, Pose> _pose = new Dictionary<uint, Pose>();
    readonly Dictionary<uint, Pose> _placed = new Dictionary<uint, Pose>();
    readonly List<UOMobileState> _mobs = new List<UOMobileState>();
    readonly List<UOItemState> _live = new List<UOItemState>();
    readonly World _world;
    uint _timePacket;

    public BoatMovingManager(World world) {
        _world = world;
    }

    int GetVelocity(byte speed) {
        switch (speed) {
            case 0x02: return SlowInterval;
            default:
            case 0x03: return NormalInterval;
            case 0x04: return FastInterval;
            case > 0x04: return speed * 10;
        }
    }

    public void MoveRequest(Direction direciton, byte speed) {
        UOLoginNet.Send(OutgoingPackets.Send_MultiBoatMoveRequest(UOLoginNet.PlayerSerial, (byte)direciton, speed));
        _timePacket = Time.Ticks;
    }

    public void AddStep(uint serial, byte speed, Direction movingDir, Direction facingDir, ushort x, ushort y, sbyte z) {
        Refresh();
        if (!TryBoat(serial, out var ix, out var iy, out var iz)) return;
        if (!_pose.TryGetValue(serial, out var pose)) {
            pose = new Pose { X = ix, Y = iy, Z = iz };
            _pose[serial] = pose;
        }
        if (!_steps.TryGetValue(serial, out var deque)) {
            deque = new Deque<BoatStep>();
            _steps[serial] = deque;
        }
        var empty = deque.Count == 0;
        while (deque.Count > 5) deque.RemoveFromFront();
        if (empty) pose.LastStepTime = Time.Ticks;
        var step = new BoatStep();
        step.Serial = serial;
        step.TimeDiff = _timePacket == 0 || empty ? GetVelocity(speed) : (int)(Time.Ticks - _timePacket);
        step.Speed = speed;
        step.X = x;
        step.Y = y;
        step.Z = z;
        step.MovingDir = movingDir;
        step.FacingDir = facingDir;
        deque.AddToBack(step);
        ClearEntities(serial);
        _timePacket = Time.Ticks;
        Console.WriteLine("CURRENT PACKET TIME: {0}", _timePacket);
    }

    public void ClearSteps(uint serial) {
        if (!_steps.TryGetValue(serial, out var deque) || deque.Count == 0) return;
        if (_pose.TryGetValue(serial, out var pose)) pose.Ox = pose.Oy = pose.Oz = 0;
        if (_items.TryGetValue(serial, out var list)) {
            for (var i = 0; i < list.Count; i++) {
                if (_placed.TryGetValue(list[i].Serial, out var ent)) ent.Ox = ent.Oy = ent.Oz = 0;
            }
            list.Clear();
        }
        deque.Clear();
    }

    public void ClearEntities(uint serial) {
        _items.Remove(serial);
    }

    public void PushItemToList(uint serial, uint objSerial, int x, int y, int z) {
        if (!_items.TryGetValue(serial, out var list)) {
            list = new List<ItemInside>();
            _items[serial] = list;
        }
        for (var i = 0; i < list.Count; i++) {
            var item = list[i];
            if (!SerialHelper.IsValid(item.Serial)) break;
            if (item.Serial != objSerial) continue;
            item.X = x;
            item.Y = y;
            item.Z = z;
            list[i] = item;
            return;
        }
        list.Add(new ItemInside { Serial = objSerial, X = x, Y = y, Z = z });
    }

    public void Update() {
        Refresh();
        foreach (var deques in _steps.Values) {
            while (deques.Count != 0) {
                var step = deques.Front();
                if (!_pose.TryGetValue(step.Serial, out var pose) || !TryBoat(step.Serial, out _, out _, out _)) {
                    _toRemove.Add(step.Serial);
                    break;
                }
                var directionChange = false;
                var maxDelay = step.TimeDiff;
                var delay = (int)Time.Ticks - (int)pose.LastStepTime;
                var removeStep = delay >= maxDelay;
                if (pose.X != step.X || pose.Y != step.Y) {
                    if (maxDelay != 0) {
                        var steps = maxDelay / (float)CharacterDelay;
                        var x = delay / (float)CharacterDelay;
                        var y = x;
                        pose.Oz = (sbyte)((step.Z - pose.Z) * x * (4.0f / steps));
                        GetPixelOffset((byte)step.MovingDir, ref x, ref y, steps);
                        pose.Ox = (sbyte)x;
                        pose.Oy = (sbyte)y;
                    }
                }
                else {
                    directionChange = true;
                    removeStep = true;
                }
                if (removeStep) {
                    var serial = step.Serial;
                    var sx = step.X;
                    var sy = step.Y;
                    var sz = step.Z;
                    var dir = step.MovingDir;
                    pose.X = sx;
                    pose.Y = sy;
                    pose.Z = sz;
                    pose.Ox = 0;
                    pose.Oy = 0;
                    pose.Oz = 0;
                    pose.Direction = dir;
                    deques.RemoveFromFront();
                    UpdateEntitiesInside(serial, true, sx, sy, sz, dir);
                    pose.LastStepTime = Time.Ticks;
                }
                else {
                    UpdateEntitiesInside(step.Serial, false, pose.X, pose.Y, pose.Z, step.MovingDir);
                }
                if (!directionChange) break;
            }
        }
        if (_toRemove.Count == 0) return;
        for (var i = 0; i < _toRemove.Count; i++) {
            _steps.Remove(_toRemove[i]);
            _items.Remove(_toRemove[i]);
            _pose.Remove(_toRemove[i]);
        }
        _toRemove.Clear();
    }

    void UpdateEntitiesInside(uint serial, bool removeStep, int x, int y, int z, Direction direction) {
        if (!_items.TryGetValue(serial, out var list)) return;
        _pose.TryGetValue(serial, out var boat);
        for (var i = 0; i < list.Count; i++) {
            var it = list[i];
            if (!TryActor(it.Serial, out var ax, out var ay, out var az)) continue;
            if (!_placed.TryGetValue(it.Serial, out var ent)) {
                ent = new Pose { X = ax, Y = ay, Z = az };
                _placed[it.Serial] = ent;
            }
            if (removeStep) {
                ent.X = (ushort)(x - it.X);
                ent.Y = (ushort)(y - it.Y);
                ent.Z = (sbyte)(z - it.Z);
                ent.Ox = 0;
                ent.Oy = 0;
                ent.Oz = 0;
                ent.Direction = direction;
            }
            else if (boat != null) {
                ent.Ox = boat.Ox;
                ent.Oy = boat.Oy;
                ent.Oz = boat.Oz;
            }
        }
    }

    void GetEndPosition(uint serial, Deque<BoatStep> deque, out ushort x, out ushort y, out sbyte z, out Direction dir) {
        if (deque.Count == 0) {
            var pose = _pose.TryGetValue(serial, out var found) ? found : null;
            x = (ushort)(pose?.X ?? 0);
            y = (ushort)(pose?.Y ?? 0);
            z = (sbyte)(pose?.Z ?? 0);
            dir = (pose?.Direction ?? 0) & Direction.Up;
            dir &= Direction.Running;
            return;
        }
        var s = deque.Back();
        x = s.X;
        y = s.Y;
        z = s.Z;
        dir = s.MovingDir;
    }

    void Refresh() => UOLoginNet.CopyActors(_mobs, _live);

    bool TryBoat(uint serial, out int x, out int y, out int z) {
        for (var i = 0; i < _live.Count; i++) {
            if (_live[i].Serial != serial) continue;
            x = _live[i].X;
            y = _live[i].Y;
            z = _live[i].Z;
            return true;
        }
        if (_world.Items.TryGetValue(serial, out var obj) && obj is Item item) {
            x = item.X;
            y = item.Y;
            z = item.Z;
            return true;
        }
        x = y = z = 0;
        return false;
    }

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
            for (var i = 0; i < _live.Count; i++) {
                if (_live[i].Serial != serial) continue;
                x = _live[i].X;
                y = _live[i].Y;
                z = _live[i].Z;
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

    /// <summary>ClassicUO MovementSpeed.GetPixelOffset. framesPerTile is the step count over one tile.</summary>
    static void GetPixelOffset(byte dir, ref float x, ref float y, float framesPerTile) {
        var stepNeswD = 44.0f / framesPerTile;
        var stepNesw = 22.0f / framesPerTile;
        var checkX = 22;
        var checkY = 22;
        switch (dir & 7) {
            case 0: // W
                x *= stepNesw;
                y *= -stepNesw;
                break;
            case 1: // NW
                x *= stepNeswD;
                checkX = 44;
                y = 0.0f;
                break;
            case 2: // N
                x *= stepNesw;
                y *= stepNesw;
                break;
            case 3: // NE
                x = 0.0f;
                y *= stepNeswD;
                checkY = 44;
                break;
            case 4: // E
                x *= -stepNesw;
                y *= stepNesw;
                break;
            case 5: // SE
                x *= -stepNeswD;
                checkX = 44;
                y = 0.0f;
                break;
            case 6: // S
                x *= -stepNesw;
                y *= -stepNesw;
                break;
            case 7: // SW
                x = 0.0f;
                y *= -stepNeswD;
                checkY = 44;
                break;
        }
        var valueX = (int)x;
        if (Math.Abs(valueX) > checkX) x = valueX < 0 ? -checkX : checkX;
        var valueY = (int)y;
        if (Math.Abs(valueY) > checkY) y = valueY < 0 ? -checkY : checkY;
    }

    sealed class Pose {
        public int X, Y, Z, Ox, Oy, Oz;
        public uint LastStepTime;
        public Direction Direction;
    }

    struct BoatStep {
        public uint Serial;
        public int TimeDiff;
        public ushort X, Y;
        public sbyte Z;
        public byte Speed;
        public Direction MovingDir, FacingDir;
    }

    struct ItemInside {
        public uint Serial;
        public int X, Y, Z;
    }
}
