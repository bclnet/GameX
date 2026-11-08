// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

public sealed class CorpseManager {
    readonly Deque<CorpseInfo> _corpses = new Deque<CorpseInfo>();
    readonly World _world;

    public CorpseManager(World world) {
        _world = world;
    }

    public void Add(uint corpse, uint obj, Direction dir, bool run) {
        for (int i = 0; i < _corpses.Count; i++) {
            ref CorpseInfo c = ref _corpses.GetAt(i);

            if (c.CorpseSerial == corpse) {
                return;
            }
        }

        _corpses.AddToBack(new CorpseInfo(corpse, obj, dir, run));
    }

    public void Remove(uint corpse, uint obj) {
        for (int i = 0; i < _corpses.Count;) {
            ref CorpseInfo c = ref _corpses.GetAt(i);

            if (c.CorpseSerial == corpse || c.ObjectSerial == obj) {
                // Item.Layer is not written
                _corpses.RemoveAt(i);
            }
            else {
                i++;
            }
        }
    }

    public bool Exists(uint corpse, uint obj) {
        for (int i = 0; i < _corpses.Count; i++) {
            ref CorpseInfo c = ref _corpses.GetAt(i);

            if (c.CorpseSerial == corpse || c.ObjectSerial == obj) {
                return true;
            }
        }

        return false;
    }

    public uint GetCorpseObject(uint serial) {
        for (int i = 0; i < _corpses.Count; i++) {
            ref CorpseInfo c = ref _corpses.GetAt(i);

            if (c.ObjectSerial == serial) {
                if (_world.Items.TryGetValue(c.CorpseSerial, out object raw) && raw is Item) {
                    return c.CorpseSerial;
                }

                return 0;
            }
        }

        return 0;
    }

    public void Clear() {
        _corpses.Clear();
    }
}

public struct CorpseInfo {
    public CorpseInfo(uint corpseSerial, uint objectSerial, Direction direction, bool isRunning) {
        CorpseSerial = corpseSerial;
        ObjectSerial = objectSerial;
        Direction = direction;
        IsRunning = isRunning;
    }

    public uint CorpseSerial, ObjectSerial;
    public Direction Direction;
    public bool IsRunning;
}
