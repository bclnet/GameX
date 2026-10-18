// SPDX-License-Identifier: BSD-2-Clause

using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO World, only the lists the login scene clears.
/// Item, Mobile, and the managers World constructs are not in this project yet.
/// </summary>
public sealed class World {
    public Dictionary<uint, object> Items { get; } = new();
    public Dictionary<uint, object> Mobiles { get; } = new();
    public string ServerName { get; set; } = "_";
}
