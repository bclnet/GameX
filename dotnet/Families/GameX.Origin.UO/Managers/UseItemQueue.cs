// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

public sealed class UseItemQueue {
    readonly Deque<uint> _actions = new Deque<uint>();
    long _timer;
    readonly World _world;

    public UseItemQueue(World world) {
        _timer = Time.Ticks + 1000;
        _world = world;
    }

    public void Update() {
        if (_timer < Time.Ticks) {
            _timer = Time.Ticks + 1000;

            if (_actions.Count == 0) {
                return;
            }

            uint serial = _actions.RemoveFromFront();

            object entity = null;
            if (SerialHelper.IsMobile(serial)) {
                if (_world.Mobiles.TryGetValue(serial, out object mobile)) {
                    entity = mobile;
                }
            }
            else if (_world.Items.TryGetValue(serial, out object item)) {
                entity = item;
            }

            if (entity != null) {
                if (SerialHelper.IsMobile(serial)) {
                    serial |= 0x8000_0000;
                }

                // no Send_* in this tree
            }
        }
    }

    public void Add(uint serial) {
        foreach (uint s in _actions) {
            if (serial == s) {
                return;
            }
        }

        _actions.AddToBack(serial);
    }

    public void Clear() {
        _actions.Clear();
    }

    public void ClearCorpses() {
        for (int i = 0; i < _actions.Count; i++) {
            if (!_world.Items.TryGetValue(_actions[i], out object entity) || entity == null) {
                continue;
            }

            // IsCorpse is not on Item
        }
    }
}
