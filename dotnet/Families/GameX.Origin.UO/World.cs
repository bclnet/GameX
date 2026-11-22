// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO World. Mobiles and Items are filled from the game socket once 0x1B arrives.
/// </summary>
public sealed class World {
    readonly EffectManager _effectManager;
    readonly AnimatedStaticsManager _animatedStatics = new();
    readonly List<UOMobileState> _actors = new();
    readonly List<UOItemState> _actorItems = new();
    readonly List<UOEquipState> _equip = new();
    readonly List<UOContainedState> _contents = new();
    readonly List<uint> _far = new();
    uint _timeToDelete;
    bool _staticsReady;

    public World() {
        WMapManager = new WorldMapEntityManager(this);
        CorpseManager = new CorpseManager(this);
        Party = new PartyManager(this);
        HouseManager = new HouseManager(this);
        WorldTextManager = new WorldTextManager(this);
        _effectManager = new EffectManager(this);
        MessageManager = new MessageManager(this);
        ContainerManager = new ContainerManager(this);
        IgnoreManager = new IgnoreManager(this);
        SkillsGroupManager = new SkillsGroupManager(this);
        ChatManager = new ChatManager(this);
        AuraManager = new AuraManager(this);
        TargetManager = new TargetManager(this);
        DelayedObjectClickManager = new DelayedObjectClickManager(this);
        BoatMovingManager = new BoatMovingManager(this);
        NameOverHeadManager = new NameOverHeadManager(this);
        Macros = new MacroManager(this);
        CommandManager = new CommandManager(this);
        InfoBars = new InfoBarManager(this);
    }

    public Dictionary<uint, object> Items { get; } = new();
    public Dictionary<uint, object> Mobiles { get; } = new();
    public string ServerName { get; set; } = "_";

    public HouseCustomizationManager CustomHouseManager;

    public WorldMapEntityManager WMapManager { get; }
    public CorpseManager CorpseManager { get; }
    public PartyManager Party { get; }
    public HouseManager HouseManager { get; }
    public WorldTextManager WorldTextManager { get; }
    public MessageManager MessageManager { get; }
    public ContainerManager ContainerManager { get; }
    public IgnoreManager IgnoreManager { get; }
    public SkillsGroupManager SkillsGroupManager { get; }
    public ChatManager ChatManager { get; }
    public AuraManager AuraManager { get; }
    public TargetManager TargetManager { get; }
    public DelayedObjectClickManager DelayedObjectClickManager { get; }
    public BoatMovingManager BoatMovingManager { get; }
    public NameOverHeadManager NameOverHeadManager { get; }
    public MacroManager Macros { get; }
    public CommandManager CommandManager { get; }
    public InfoBarManager InfoBars { get; }

    /// <summary>Copies the socket actors into Mobiles and Items. Ground rows stay container 0. Worn rows and one backpack level are added after that.</summary>
    public void RefreshActors() {
        UOLoginNet.CopyActors(_actors, _actorItems);
        Mobiles.Clear();
        Items.Clear();
        for (var i = 0; i < _actors.Count; i++) {
            var m = _actors[i];
            Mobiles[m.Serial] = new Mobile(m.Serial, m.Body, m.X, m.Y, m.Z, m.Direction, m.Notoriety, m.Flags);
        }
        for (var i = 0; i < _actorItems.Count; i++) {
            var it = _actorItems[i];
            Items[it.Serial] = new Item(it.Serial, it.Graphic, it.X, it.Y, it.Z);
        }
        for (var i = 0; i < _actors.Count; i++) {
            var m = _actors[i];
            UOLoginNet.CopyEquip(m.Serial, _equip);
            for (var e = 0; e < _equip.Count; e++) {
                var eq = _equip[e];
                if (Items.ContainsKey(eq.Serial)) continue;
                Items[eq.Serial] = new Item(eq.Serial, eq.Graphic, m.X, m.Y, m.Z, eq.Hue, 0, eq.Parent, eq.Layer);
            }
        }
        var pack = UOLoginNet.PlayerBackpack;
        if (pack != 0) {
            UOLoginNet.CopyContents(pack, _contents);
            for (var i = 0; i < _contents.Count; i++) {
                var row = _contents[i];
                if (Items.ContainsKey(row.Serial)) continue;
                Items[row.Serial] = new Item(row.Serial, row.Graphic, row.X, row.Y, 0, row.Hue, row.Amount, pack, 0);
            }
        }
    }

    /// <summary>Rebuilds the actor snapshot, drops rows past the view range, then ticks effects, world text, static animation, and stale world-map rows.</summary>
    public void Update() {
        RefreshActors();
        if (UOLoginNet.PlayerSerial != 0 && _timeToDelete < Time.Ticks) {
            _timeToDelete = Time.Ticks + 50;
            var px = UOLoginNet.PlayerX;
            var py = UOLoginNet.PlayerY;
            var range = UOLoginNet.ViewRange;
            var player = UOLoginNet.PlayerSerial;
            _far.Clear();
            foreach (var kv in Mobiles) {
                if (kv.Value is not Mobile m || m.Serial == player) continue;
                if (Math.Max(Math.Abs(m.X - px), Math.Abs(m.Y - py)) <= range) continue;
                if (Party != null && Party.Contains(m.Serial) || m.Notoriety == 1 || m.Notoriety == 2)
                    WMapManager.AddOrUpdate(m.Serial, m.X, m.Y, 0, UOLoginNet.MapIndex, false, null);
                _far.Add(m.Serial);
            }
            for (var i = 0; i < _far.Count; i++) Mobiles.Remove(_far[i]);
            _far.Clear();
            foreach (var kv in Items) {
                if (kv.Value is not Item it || !it.OnGround) continue;
                if (Math.Max(Math.Abs(it.X - px), Math.Abs(it.Y - py)) <= range) continue;
                _far.Add(it.Serial);
            }
            for (var i = 0; i < _far.Count; i++) Items.Remove(_far[i]);
        }
        _effectManager.Update();
        WorldTextManager.Update();
        WMapManager.RemoveUnupdatedWEntity();
        if (!_staticsReady) {
            _animatedStatics.Initialize();
            _staticsReady = true;
        }
        _animatedStatics.Process();
    }
}
