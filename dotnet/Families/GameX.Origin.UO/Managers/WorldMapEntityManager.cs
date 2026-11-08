// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO WorldMapEntityManager. Entities is the party and guild list.
/// AddOrUpdate and Remove keep those rows. No marker file is written.
/// </summary>
public class WMapEntity {
    public WMapEntity(uint serial) {
        Serial = serial;
    }

    public bool IsGuild;
    public uint LastUpdate;
    public string Name;
    public readonly uint Serial;
    public int X, Y, HP, Map;
}

public sealed class WorldMapEntityManager {
    const uint NewMovement = 0x4000;

    bool _ackReceived;
    uint _lastUpdate, _lastPacketSend, _lastPacketRecv;
    // Profile path is not ported.
    bool _worldMapShowParty = true;
    readonly List<WMapEntity> _toRemove = [];
    readonly List<(uint serial, string name)> _party = [];
    readonly List<UOMobileState> _mobiles = [];
    readonly List<UOItemState> _items = [];
    readonly World _world;

    public WorldMapEntityManager(World world) { _world = world; }

    public bool Enabled {
        get {
            // Profile path is not ported.
            var clear = UltimaOnline.Current == null || UltimaOnline.Current.Plain;
            return ((UOLoginNet.Features & NewMovement) == 0 || _ackReceived)
                && clear
                && _worldMapShowParty
                && UIManager.GetGump<WorldMapGump>() != null;
        }
    }

    public readonly Dictionary<uint, WMapEntity> Entities = new();

    public void SetACKReceived() {
        _ackReceived = true;
    }

    public void SetEnable(bool v) {
        var plain = UltimaOnline.Current == null || UltimaOnline.Current.Plain;
        if ((UOLoginNet.Features & NewMovement) != 0 && !_ackReceived) v = false;
        else if (!plain && !_ackReceived) v = false;
        if (v) RequestServerPartyGuildInfo(true);
    }

    public void AddOrUpdate(uint serial, int x, int y, int hp, int map, bool isguild, string name = null, bool from_packet = false) {
        if (from_packet) _lastPacketRecv = Time.Ticks + 10000;
        else if (_lastPacketRecv < Time.Ticks) return;
        if (!Enabled) return;
        if (string.IsNullOrEmpty(name)) {
            var known = UOLoginNet.NameOf(serial);
            if (!string.IsNullOrEmpty(known)) name = known;
        }
        if (!Entities.TryGetValue(serial, out var entity) || entity == null) {
            entity = new WMapEntity(serial) {
                X = x,
                Y = y,
                HP = hp,
                Map = map,
                LastUpdate = Time.Ticks + 1000,
                IsGuild = isguild,
                Name = name
            };
            Entities[serial] = entity;
        }
        else {
            entity.X = x;
            entity.Y = y;
            entity.HP = hp;
            entity.Map = map;
            entity.IsGuild = isguild;
            entity.LastUpdate = Time.Ticks + 1000;
            if (string.IsNullOrEmpty(entity.Name) && !string.IsNullOrEmpty(name)) entity.Name = name;
        }
    }

    public void Remove(uint serial) {
        if (Entities.ContainsKey(serial)) Entities.Remove(serial);
    }

    public void RemoveUnupdatedWEntity() {
        if (_lastUpdate > Time.Ticks) return;
        _lastUpdate = Time.Ticks + 1000;
        long ticks = Time.Ticks - 1000;
        foreach (var entity in Entities.Values)
            if (entity.LastUpdate < ticks) _toRemove.Add(entity);
        if (_toRemove.Count == 0) return;
        foreach (var entity in _toRemove) Entities.Remove(entity.Serial);
        _toRemove.Clear();
    }

    public WMapEntity GetEntity(uint serial) {
        Entities.TryGetValue(serial, out var entity);
        return entity;
    }

    public void RequestServerPartyGuildInfo(bool force = false) {
        if (!force && !Enabled) return;
        if (UOLoginNet.PlayerSerial == 0 || _lastPacketSend >= Time.Ticks) return;
        _lastPacketSend = Time.Ticks + 250;
        // 0xF0 is not ported.
        if (UOParty.Leader == 0) return;
        UOParty.CopyMembers(_party);
        UOLoginNet.CopyActors(_mobiles, _items);
        for (var i = 0; i < _party.Count; i++) {
            var serial = _party[i].serial;
            if (!SerialHelper.IsValid(serial)) continue;
            if (!TryActor(serial, out var actor) || Farther(actor)) {
                // 0xF0 is not ported.
                break;
            }
        }
    }

    public void Clear() {
        Entities.Clear();
        _ackReceived = false;
        SetEnable(false);
    }

    bool TryActor(uint serial, out UOMobileState actor) {
        for (var i = 0; i < _mobiles.Count; i++) {
            if (_mobiles[i].Serial != serial) continue;
            actor = _mobiles[i];
            return true;
        }
        if (_world.Mobiles.TryGetValue(serial, out var obj) && obj is Mobile mob) {
            actor = new UOMobileState(mob.Serial, mob.Body, mob.X, mob.Y, mob.Z, mob.Direction);
            return true;
        }
        actor = default;
        return false;
    }

    static bool Farther(UOMobileState actor) {
        var player = UOLoginNet.PlayerSerial;
        if (player == 0) return true;
        if (actor.Serial == player) return false;
        var dx = Math.Abs(actor.X - UOLoginNet.PlayerX);
        var dy = Math.Abs(actor.Y - UOLoginNet.PlayerY);
        var distance = dx > dy ? dx : dy;
        return distance > UOLoginNet.ViewRange;
    }
}
