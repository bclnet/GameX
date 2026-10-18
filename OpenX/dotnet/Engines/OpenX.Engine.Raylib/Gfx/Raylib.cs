using System;
using System.Collections.Generic;

namespace OpenX.Gfx.Raylib;

#region Extensions

/// <summary>
/// RaylibX
/// </summary>
public static class RaylibX {
    public static Dictionary<Type, Func<object, bool, object, object>> BuildersByType = [];

    /// <summary>
    /// Calls IsWindowReady. Does not open a window. Returns the binding version string.
    /// </summary>
    public static string Version() {
        Raylib_cs.Raylib.IsWindowReady();
        return Raylib_cs.Raylib.RAYLIB_VERSION;
    }
}

#endregion
