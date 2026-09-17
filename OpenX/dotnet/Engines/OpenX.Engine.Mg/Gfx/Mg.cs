using System;
using System.Collections.Generic;

namespace OpenX.Gfx.Mg;

#region Extensions

// MgX
public static class MgX {
    public static Dictionary<Type, Func<object, bool, object, object>> BuildersByType = [];
}

#endregion
