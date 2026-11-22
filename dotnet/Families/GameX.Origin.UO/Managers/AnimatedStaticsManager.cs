// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO AnimatedStaticsManager. The graphic list and the frame timer stay here.
/// animdata.mul is read once from the client path. A missing file leaves the list empty.
/// </summary>
public sealed class AnimatedStaticsManager {
    StaticAnimationInfo[] _staticInfos = Array.Empty<StaticAnimationInfo>();
    Binary_Animdata _animdata;
    bool _animdataTried;
    uint _processTime;
    const uint ItemEffectAnimationDelay = 50;

    public void Initialize() {
        if (_animdataTried) return;
        var clientPath = UltimaOnline.Current?.ClientPath;
        if (string.IsNullOrEmpty(clientPath)) return;
        _animdataTried = true;
        try {
            var path = Path.Combine(clientPath, "animdata.mul");
            if (!File.Exists(path)) return;
            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);
            var data = new Binary_Animdata(reader);
            var list = new List<StaticAnimationInfo>(data.Records.Count);
            foreach (var pair in data.Records) {
                if (pair.Value == null || pair.Value.FrameCount == 0) continue;
                if ((uint)pair.Key > ushort.MaxValue) continue;
                var index = (ushort)pair.Key;
                list.Add(new StaticAnimationInfo { Index = index, IsField = IsField(index) });
            }
            _animdata = data;
            _staticInfos = list.ToArray();
        }
        catch {
            _animdata = null;
            _staticInfos = Array.Empty<StaticAnimationInfo>();
        }
    }

    public void Process() {
        if (_staticInfos.Length == 0 || _processTime >= Time.Ticks)
            return;

        uint delay = ItemEffectAnimationDelay * 2;
        uint nextTime = Time.Ticks + 250;
        // No profile is stored. FieldsType 0 keeps field animation.
        bool noAnimatedField = false;

        for (int i = 0; i < _staticInfos.Length; i++) {
            ref StaticAnimationInfo o = ref _staticInfos[i];
            o.IsField = IsField(o.Index);
            if (noAnimatedField && o.IsField) {
                o.AnimIndex = 0;
                continue;
            }
            if (o.Time < Time.Ticks) {
                uint addr = (uint)(o.Index * 68 + 4 * (o.Index / 8 + 1));
                int art = o.Index + 0x4000;
                // art read stays on Client
                byte offset = o.AnimIndex;
                byte frameInterval = 0;
                byte frameCount = 0;
                if (_animdata != null && _animdata.Records.TryGetValue(o.Index, out var record)) {
                    frameInterval = record.FrameInterval;
                    frameCount = record.FrameCount;
                }
                if (frameInterval > 0)
                    o.Time = Time.Ticks + frameInterval * delay + 1;
                else
                    o.Time = Time.Ticks + delay;
                if (offset < frameCount) {
                    _ = art;
                    _ = addr;
                    offset++;
                }
                if (offset >= frameCount)
                    offset = 0;
                o.AnimIndex = offset;
            }
            if (o.Time < nextTime)
                nextTime = o.Time;
        }
        _processTime = nextTime;
    }

    static bool IsField(ushort g) {
        return g >= 0x398C && g <= 0x399F || g >= 0x3967 && g <= 0x397A || g >= 0x3946 && g <= 0x3964 || g >= 0x3914 && g <= 0x3929;
    }

    private struct StaticAnimationInfo {
        public uint Time;
        public ushort Index;
        public byte AnimIndex;
        public bool IsField;
    }
}
