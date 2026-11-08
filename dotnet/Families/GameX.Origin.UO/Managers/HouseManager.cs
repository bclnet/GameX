// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

public sealed class HouseManager {
    private sealed class House {
        public House(uint serial) { Serial = serial; }

        public uint Serial { get; }

        public void ClearComponents() { }
    }

    readonly Dictionary<uint, House> _houses = new Dictionary<uint, House>();
    readonly World _world;

    public HouseManager(World world) {
        _world = world;
    }

    public IReadOnlyCollection<uint> Houses => _houses.Keys;

    public void Add(uint serial) {
        _houses[serial] = new House(serial);
    }

    public bool TryGetHouse(uint serial, out uint house) {
        if (_houses.TryGetValue(serial, out House found)) {
            house = found.Serial;
            return true;
        }

        house = 0;
        return false;
    }

    public bool TryToRemove(uint serial, int distance) {
        if (!IsHouseInRange(serial, distance)) {
            if (_houses.TryGetValue(serial, out House house)) {
                house.ClearComponents();
                _houses.Remove(serial);
            }

            return true;
        }

        return false;
    }

    public bool IsHouseInRange(uint serial, int distance) {
        if (TryGetHouse(serial, out _)) {
            int currX = UOLoginNet.PlayerX;
            int currY = UOLoginNet.PlayerY;

            //if (World.Player.IsMoving)
            //{
            //    Mobile.Step step = World.Player.Steps.Back();
            //
            //    currX = step.X;
            //    currY = step.Y;
            //}
            //else
            //{
            //    currX = World.Player.X;
            //    currY = World.Player.Y;
            //}

            if (!_world.Items.TryGetValue(serial, out object raw) || raw is not Item found) {
                return true;
            }

            // no MultiDistanceBonus
            return Math.Abs(found.X - currX) <= distance && Math.Abs(found.Y - currY) <= distance;
        }

        return false;
    }

    public bool EntityIntoHouse(uint house, object obj) {
        if (obj != null && TryGetHouse(house, out _)) {
            if (!_world.Items.TryGetValue(house, out object raw) || raw is not Item) {
                return true;
            }

            // no MultiInfo
            return true;
        }

        return false;
    }

    public void Remove(uint serial) {
        if (TryGetHouse(serial, out _)) {
            if (_houses.TryGetValue(serial, out House house)) {
                house.ClearComponents();
                _houses.Remove(serial);
            }
        }
    }

    public void RemoveMultiTargetHouse() {
        if (_houses.TryGetValue(0, out House house)) {
            house.ClearComponents();
            _houses.Remove(0);
        }
    }

    public bool Exists(uint serial) {
        return _houses.ContainsKey(serial);
    }

    public void Clear() {
        foreach (KeyValuePair<uint, House> house in _houses) {
            house.Value.ClearComponents();
        }

        _houses.Clear();
    }
}
