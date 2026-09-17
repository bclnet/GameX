using OpenX.Client;
using System;
using System.Collections.Generic;

namespace OpenX;

#region Client

public class UnknownClientHost : IClientHost {
    public void Dispose() => throw new NotImplementedException();
    public void Run() => throw new NotImplementedException();
}

#endregion

#region Platform

/// <summary>
/// UnknownPlatform
/// </summary>
public class UnknownEngine : Engine {
    //public static Dictionary<Type, Func<object, bool, object, object>> BuildersByType = [];
    public static readonly Engine This = new UnknownEngine();
    UnknownEngine() : base("UK", "Unknown") {
        GfxFactory = () => [null, null, null, null, null, null];
        SfxFactory = () => [null];
    }
}

#endregion
