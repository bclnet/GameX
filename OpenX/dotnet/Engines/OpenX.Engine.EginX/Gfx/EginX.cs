using System;
using System.Collections.Generic;

namespace OpenX.Gfx.EginX;

#region Extensions

// EginX
public static class EginX {
    public static Dictionary<Type, Func<object, bool, object, object>> BuildersByType = [];
}

#endregion
