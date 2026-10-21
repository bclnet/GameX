// SPDX-License-Identifier: BSD-2-Clause

using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO World. Mobiles and Items are filled from the game socket once 0x1B arrives.
/// </summary>
public sealed class World {
    public Dictionary<uint, object> Items { get; } = new();
    public Dictionary<uint, object> Mobiles { get; } = new();
    public string ServerName { get; set; } = "_";
}
