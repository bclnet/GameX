// SPDX-License-Identifier: BSD-2-Clause

using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// Session ignore list. One set, the default comparer, ordinal and case-sensitive.
/// A different case is a different entry. A stored name is trimmed.
/// The names are the yaml key ignoredChars. Login keys are not written.
/// </summary>
public sealed class IgnoreManager {
    /// <summary>The one instance the ignore gump uses.</summary>
    public static readonly IgnoreManager Instance = new IgnoreManager(null);

    /// <summary>Set of char names.</summary>
    public HashSet<string> IgnoredCharsList = new HashSet<string>();

    public IgnoreManager(World world) { }

    /// <summary>Initialize the ignore list.</summary>
    public void Initialize() {
        ReadIgnoreList();
    }

    /// <summary>Adds one trimmed name. Null, empty, and a second add of the same name are rejected.</summary>
    public bool AddIgnoredTarget(string name) {
        if (name == null) return false;
        name = name.Trim();
        if (name.Length == 0) return false;
        if (!IgnoredCharsList.Add(name)) return false;
        SaveIgnoreList();
        return true;
    }

    /// <summary>Remove a char from the ignored list.</summary>
    public void RemoveIgnoredTarget(string charName) {
        if (IgnoredCharsList.Contains(charName) && IgnoredCharsList.Remove(charName)) SaveIgnoreList();
    }

    /// <summary>Load the ignored list.</summary>
    void ReadIgnoreList() {
        IgnoredCharsList.Clear();
        foreach (var name in UOSettings.GetList("ignoredChars"))
            if (!string.IsNullOrEmpty(name)) IgnoredCharsList.Add(name);
    }

    /// <summary>Save the ignored list.</summary>
    public void SaveIgnoreList() {
        UOSettings.Set("ignoredChars", new List<string>(IgnoredCharsList));
    }
}
