namespace OpenX;

/// <summary>
/// DiligentShim. Reports that this build did not link Diligent Engine.
/// </summary>
public static class DiligentShim {
    /// <summary>False. Diligent Engine is not linked.</summary>
    public static bool Linked => false;

    /// <summary>This build returns "not linked".</summary>
    public static string Version => "not linked";
}
