using System;
using System.Collections.Generic;

namespace OpenX.Gfx.Bgfx;

#region Extensions

/// <summary>
/// BgfxX
/// </summary>
public static class BgfxX {
    public static Dictionary<Type, Func<object, bool, object, object>> BuildersByType = [];
}

#endregion
