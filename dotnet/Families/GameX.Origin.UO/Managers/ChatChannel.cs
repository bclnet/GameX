// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>ClassicUO ChatChannel. Name and whether the conference has a password.</summary>
public sealed class ChatChannel {
    public ChatChannel(string name, bool hasPassword) {
        Name = name;
        HasPassword = hasPassword;
    }

    public readonly bool HasPassword;
    public readonly string Name;
}
