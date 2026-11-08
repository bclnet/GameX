// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO WorldTextManager. Damage text is a private nested object (text, hue, X, Y, time).
/// Draw places those lines and does not draw. An empty damage list is removed after Update.
/// </summary>
public class WorldTextManager : TextRenderer {
    private const int DamageYMovingTime = 25;

    /// <summary>Not a public TextObject. X and Y are the overhead screen pixels.</summary>
    private sealed class TextObject {
        public string Text;
        public ushort Hue;
        public int X, Y, OffsetY, Width, Height;
        public long Time, SecondTime;
        public bool IsDestroyed;
    }

    private sealed class Overhead {
        public object Parent;
        public readonly Deque<TextObject> Messages = new();
        public int RectX, RectY;
        public bool IsEmpty => Messages.Count == 0;

        public void Add(int damage) {
            string text = damage.ToString();
            MeasureLine(text, 3, out int width, out int height);
            ushort hue = 0x0021;
            if (Parent is Mobile mobile && mobile.Serial == UOLoginNet.PlayerSerial) hue = 0x0034;
            Messages.AddToFront(new TextObject {
                Text = text,
                Hue = hue,
                Time = Time.Ticks + 1500,
                Width = width,
                Height = height,
            });
            if (Messages.Count > 10) Messages.RemoveFromBack().IsDestroyed = true;
        }

        public void Update() {
            for (int i = 0; i < Messages.Count; i++) {
                TextObject c = Messages[i];
                float delta = c.Time - Time.Ticks;
                if (c.SecondTime < Time.Ticks) {
                    c.OffsetY += 1;
                    c.SecondTime = Time.Ticks + DamageYMovingTime;
                }
                if (delta <= 0) {
                    c.IsDestroyed = true;
                    Messages.RemoveAt(i--);
                }
            }
        }

        /// <summary>ClassicUO OverheadDamage.Draw without a batcher. Writes X and Y and does not draw.</summary>
        public void Draw() {
            if (Messages.Count == 0) return;
            int offY = 0;
            int px = 0, py = 0;
            if (Parent is Mobile m) {
                // No mounted or flying flags on Mobile. ClassicUO uses -22 when the mobile is not mounted.
                // Animation height and center are not loaded, so the vertical term is 8.
                TileToScreen(m.X, m.Y, m.Z, out RectX, out RectY);
                px = RectX;
                py = RectY;
                offY = -22;
                px += 22;
                py -= 8;
            }
            else if (Parent is Item it) {
                // Corpse item. Art height is not loaded, so the UV shift is 0. A static or multi would use -44.
                TileToScreen(it.X, it.Y, it.Z, out RectX, out RectY);
                px = RectX;
                py = RectY;
                px += 22;
                int yValue = 0;
                py -= yValue;
                offY = -22;
            }
            foreach (TextObject item in Messages) {
                if (item.IsDestroyed || string.IsNullOrEmpty(item.Text)) continue;
                item.X = px - (item.Width >> 1);
                item.Y = py - offY - item.Height - item.OffsetY;
                offY += item.Height;
            }
        }
    }

    private readonly Dictionary<uint, Overhead> _damages = [];
    private readonly List<(uint From, uint To)> _subst = [];
    private readonly List<uint> _toRemoveDamages = [];

    public WorldTextManager(World world) : base(world) { }

    public override void Update() {
        base.Update();
        UpdateDamageOverhead();
        if (_toRemoveDamages.Count > 0) {
            foreach (uint s in _toRemoveDamages) _damages.Remove(s);
            _toRemoveDamages.Clear();
        }
    }

    public override void Draw(int startX, int startY, float layerDepth, bool isGump = false) {
        base.Draw(startX, startY, layerDepth, isGump);
        foreach (KeyValuePair<uint, Overhead> overheadDamage in _damages) {
            object mob = Find(overheadDamage.Key);
            if (mob == null) {
                uint ser = overheadDamage.Key | 0x80000000;
                if (World != null && World.Items.ContainsKey(ser)) {
                    if (World.Items.TryGetValue(ser, out object obj) && obj is Item item && !ReferenceEquals(item, overheadDamage.Value.Parent)) {
                        _subst.Add((overheadDamage.Key, item.Serial));
                        overheadDamage.Value.Parent = item;
                    }
                }
                else continue;
            }
            else overheadDamage.Value.Parent = mob;
            overheadDamage.Value.Draw();
        }
    }

    private void UpdateDamageOverhead() {
        if (_subst.Count != 0) {
            foreach ((uint From, uint To) tuple in _subst) {
                if (_damages.TryGetValue(tuple.From, out Overhead dmg)) {
                    _damages.Remove(tuple.From);
                    _damages[tuple.To] = dmg;
                }
            }
            _subst.Clear();
        }
        foreach (KeyValuePair<uint, Overhead> overheadDamage in _damages) {
            overheadDamage.Value.Update();
            if (overheadDamage.Value.IsEmpty) _toRemoveDamages.Add(overheadDamage.Key);
        }
    }

    public void AddDamage(uint obj, int dmg) {
        if (!_damages.TryGetValue(obj, out Overhead dm) || dm == null) {
            dm = new Overhead { Parent = Find(obj) };
            _damages[obj] = dm;
        }
        dm.Add(dmg);
    }

    public override void Clear() {
        if (_toRemoveDamages.Count > 0) {
            foreach (uint s in _toRemoveDamages) _damages.Remove(s);
            _toRemoveDamages.Clear();
        }
        _subst.Clear();
        base.Clear();
    }

    /// <summary>Nested Overhead cannot call protected Measure. This wrapper is on the derived class.</summary>
    private static void MeasureLine(string text, byte font, out int width, out int height) =>
        Measure(text, font, out width, out height);

    private object Find(uint serial) {
        if (World == null) return null;
        if (World.Mobiles.TryGetValue(serial, out object mobile)) return mobile;
        if (World.Items.TryGetValue(serial, out object item)) return item;
        return null;
    }

    /// <summary>UpdateRealScreenPosition with a zero camera. WorldToScreen is identity without a camera.</summary>
    private static void TileToScreen(int x, int y, int z, out int sx, out int sy) {
        sx = ((x - y) * 22) - 22;
        sy = ((x + y) * 22 - (z << 2)) - 22;
    }
}
