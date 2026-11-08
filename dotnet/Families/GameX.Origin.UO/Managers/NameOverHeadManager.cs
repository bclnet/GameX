// SPDX-License-Identifier: BSD-2-Clause

using System;

namespace GameX.Origin.UO;

[Flags]
public enum NameOverheadTypeAllowed {
    None = 0,
    Items = 1 << 0,
    Corpses = 1 << 1,
    Innocent = 1 << 2,
    Ally = 1 << 3,
    Gray = 1 << 4,
    Criminal = 1 << 5,
    Enemy = 1 << 6,
    Murderer = 1 << 7,
    Invulnerable = 1 << 8,
    AllMobiles = Innocent | Ally | Gray | Criminal | Enemy | Murderer | Invulnerable,
    All = Items | Corpses | AllMobiles
}

public sealed class NameOverHeadManager {
    readonly World _world;

    // Profile path is not ported.
    public NameOverheadTypeAllowed TypeAllowed;
    public bool IsToggled;

    public bool IsEnabled;
    public bool IsVisible;

    public NameOverHeadManager(World world) { _world = world; }

    public bool IsAllowed(uint serial, NotorietyFlag notoriety) {
        if (_world == null || serial == 0) {
            return false;
        }

        if (SerialHelper.IsItem(serial)) {
            // IsCorpse is not on Item
            return TypeAllowed.HasFlag(NameOverheadTypeAllowed.Items);
        }

        if (SerialHelper.IsMobile(serial)) {
            return notoriety switch {
                NotorietyFlag.Innocent => TypeAllowed.HasFlag(NameOverheadTypeAllowed.Innocent),
                NotorietyFlag.Ally => TypeAllowed.HasFlag(NameOverheadTypeAllowed.Ally),
                NotorietyFlag.Gray => TypeAllowed.HasFlag(NameOverheadTypeAllowed.Gray),
                NotorietyFlag.Criminal => TypeAllowed.HasFlag(NameOverheadTypeAllowed.Criminal),
                NotorietyFlag.Enemy => TypeAllowed.HasFlag(NameOverheadTypeAllowed.Enemy),
                NotorietyFlag.Murderer => TypeAllowed.HasFlag(NameOverheadTypeAllowed.Murderer),
                NotorietyFlag.Invulnerable => TypeAllowed.HasFlag(NameOverheadTypeAllowed.Invulnerable),
                _ => true
            };
        }

        return false;
    }

    public void Open() {
        // NameOverHeadHandlerGump does not exist
        IsEnabled = true;
        IsVisible = true;
    }

    public void SetMenuVisible(bool visible) {
        IsVisible = visible;
    }

    public void Close() {
        // NameOverHeadHandlerGump does not exist
        IsEnabled = false;
        IsVisible = false;
    }

    public void ToggleOverheads() {
        IsToggled = !IsToggled;
    }
}
