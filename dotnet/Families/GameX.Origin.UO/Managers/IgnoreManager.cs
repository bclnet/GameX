// SPDX-License-Identifier: BSD-2-Clause

using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// Session ignore list. One set, the default comparer, ordinal and case-sensitive.
/// A different case is a different entry. A stored name is trimmed.
/// Nothing is written to a profile or a settings file.
/// </summary>
public sealed class IgnoreManager {
    /// <summary>The one instance the ignore gump uses.</summary>
    public static readonly IgnoreManager Instance = new IgnoreManager(null);

    /// <summary>Set of char names.</summary>
    public HashSet<string> IgnoredCharsList = new HashSet<string>();

    public IgnoreManager(World world) { }

    /// <summary>Initialize the ignore list.</summary>
    public void Initialize() {
        // Profile path is not ported.
        ReadIgnoreList();
    }

    /// <summary>Adds one trimmed name. Null, empty, and a second add of the same name are rejected.</summary>
    public bool AddIgnoredTarget(string name) {
        if (name == null) return false;
        name = name.Trim();
        if (name.Length == 0) return false;
        return IgnoredCharsList.Add(name);
    }

    /// <summary>Remove a char from the ignored list.</summary>
    public void RemoveIgnoredTarget(string charName) {
        if (IgnoredCharsList.Contains(charName)) IgnoredCharsList.Remove(charName);
    }

    /// <summary>Load the ignored list.</summary>
    void ReadIgnoreList() {
        // Profile path is not ported.
    }

    /// <summary>Save the ignored list.</summary>
    public void SaveIgnoreList() {
        // Profile path is not ported.
    }
}
