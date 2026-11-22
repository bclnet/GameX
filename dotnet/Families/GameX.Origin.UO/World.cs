// SPDX-License-Identifier: BSD-2-Clause

using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO World. Mobiles and Items are filled from the game socket once 0x1B arrives.
/// </summary>
public sealed class World {
    readonly EffectManager _effectManager;

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

    /// <summary>Ticks effects, world text, and stale world-map rows.</summary>
    public void Update() {
        _effectManager.Update();
        WorldTextManager.Update();
        WMapManager.RemoveUnupdatedWEntity();
    }
}
