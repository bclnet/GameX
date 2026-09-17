using System;
using System.Collections.Generic;

namespace OpenX.Gfx.Ogre;

#region Extensions

// OgreX
public static class OgreX {
    public static Dictionary<Type, Func<object, bool, object, object>> BuildersByType = [];

}

#endregion
