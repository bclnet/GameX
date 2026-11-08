// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO GumpType. Gump has no GumpType property, and this file does not add one.
/// A subclass that needs a value stores it itself.
/// </summary>
public enum GumpType {
    None,

    Buff,
    Container,
    CounterBar,
    HealthBar,
    InfoBar,
    Journal,
    MacroButton,
    MiniMap,
    PaperDoll,
    SkillMenu,
    SpellBook,
    StatusGump,
    TipNotice,
    AbilityButton,
    SpellButton,
    SkillButton,
    RacialButton,
    WorldMap,

    Debug,
    NetStats,

    NameOverHeadHandler
}
