// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO LastCharacterManager. The json file stays on UOLoginSettings.
/// A -lastcharactername override is memory only and is not written.
/// </summary>
public static class LastCharacterManager {
    static string LastCharacterNameOverride;

    public static void Load() {
        // lastcharacter.json stays on UOLoginSettings
        UOLoginSettings.Load();
    }

    public static void Save(string account, string server, string name) {
        // Check to see if they passed in -lastcharactername but picked another character, clear override then
        if (!string.IsNullOrEmpty(LastCharacterNameOverride) && !LastCharacterNameOverride.Equals(name))
            LastCharacterNameOverride = string.Empty;
        // lastcharacter.json stays on UOLoginSettings
        UOLoginSettings.NoteCharacter(account, server, name);
    }

    public static string GetLastCharacter(string account, string server) {
        Load();
        // If they passed in a -lastcharactername param, ignore json value, use that value instead
        if (!string.IsNullOrEmpty(LastCharacterNameOverride))
            return LastCharacterNameOverride;
        return UOLoginSettings.LastCharacter(account, server);
    }

    public static void OverrideLastCharacter(string name) {
        LastCharacterNameOverride = name;
    }
}
