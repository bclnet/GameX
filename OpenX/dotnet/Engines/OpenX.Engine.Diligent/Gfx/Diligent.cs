using System;
using System.Collections.Generic;

namespace OpenX.Gfx.Diligent;

#region Extensions

/// <summary>
/// DiligentX
/// </summary>
public static class DiligentX {
    public static Dictionary<Type, Func<object, bool, object, object>> BuildersByType = [];
}

#endregion
