using OpenX.Gfx;
using OpenX.Sfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenX;

#region Engine

/// <summary>
/// Gets the engine.
/// </summary>
public abstract class Engine(string id, string name) {
    /// <summary>
    /// Gets the engine enabled.
    /// </summary>
    public bool Enabled = true;

    /// <summary>
    /// Gets the engine caps
    /// </summary>
    public EngineX.Caps Caps = EngineX.Caps.None_;

    /// <summary>
    /// Gets the engine id.
    /// </summary>
    public readonly string Id = id;

    /// <summary>
    /// Gets the engine name.
    /// </summary>
    public readonly string Name = name;

    /// <summary>
    /// Gets the engine name.
    /// </summary>
    public string DisplayName => Name;

    /// <summary>
    /// Gets the engine tag.
    /// </summary>
    public string Tag;

    /// <summary>
    /// Gets the engine gfx factory.
    /// </summary>
    public Func<IOpenGfx[]> GfxFactory = () => null; // throw new Exception("No GfxFactory");

    /// <summary>
    /// Gets the engine sfx factory.
    /// </summary>
    public Func<IOpenSfx[]> SfxFactory = () => null; // throw new Exception("No SfxFactory");

    /// <summary>
    /// Gets the engine log func.
    /// </summary>
    public Action<string> LogFunc = a => System.Diagnostics.Debug.Print(a);

    /// <summary>
    /// Activates the engine.
    /// </summary>
    public virtual void Activate() {
        Log.Func = LogFunc;
    }

    /// <summary>
    /// Deactivates the engine.
    /// </summary>
    public virtual void Deactivate() { }
}

/// <summary>
/// EngineX
/// </summary>
public static class EngineX {
    public static readonly float Epsilon = GetEpsilon();
    static float GetEpsilon() { float epsilon = 1f, comparison; do { epsilon *= 0.5f; comparison = 1.0f + epsilon; } while (comparison > 1.0f); return epsilon; }
    //public static Action Hook;

    /// <summary>
    /// The platform Caps.
    /// </summary>
    [Flags] public enum Caps { None_ = 0x0, ReadDds = 0x1, Drawing = 0x2 }

    /// <summary>
    /// The platform OS.
    /// </summary>
    public enum OS { Unknown, Windows, OSX, Linux, Android }

    /// <summary>
    /// Gets the platform os.
    /// </summary>
    public static readonly OS PlatformOS = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OS.Windows
        : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OS.OSX
        : RuntimeInformation.OSDescription.StartsWith("android-") ? OS.Android
        : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OS.Linux
        : OS.Unknown;
    //: throw new ArgumentOutOfRangeException(nameof(RuntimeInformation.IsOSPlatform), RuntimeInformation.OSDescription);

    /// <summary>
    /// Gets the platform startups.
    /// </summary>
    public static readonly HashSet<Engine> Engines = [UnknownEngine.This];

    /// <summary>
    /// Determines if in a test host.
    /// </summary>
    public static readonly bool InTestHost = AppDomain.CurrentDomain.GetAssemblies().Any(x => x.FullName.StartsWith("testhost,"));

    /// <summary>
    /// Gets the application Path
    /// </summary>
    public static readonly string ApplicationPath = AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// Gets the platform startups.
    /// </summary>
    public static YamlDict Options = new("~/.gamex.yaml");

    /// <summary>
    /// Gets or sets the current platform.
    /// </summary>
    public static Engine Current = Activate(InTestHost ? TestEngine.This : UnknownEngine.This);

    /// <summary>
    /// Activates an engine.
    /// </summary>
    public static Engine Activate(Engine engine) {
        //Hook?.Invoke(); Hook = null;
        if (engine == null || !engine.Enabled) engine = UnknownEngine.This;
        Engines.Add(engine);
        var current = Current;
        if (current != engine) {
            current?.Deactivate();
            engine?.Activate();
            Gfx = engine?.GfxFactory?.Invoke();
            Sfx = engine?.SfxFactory?.Invoke();
            Current = engine;
        }
        return engine;
    }

    /// <summary>
    /// Gets the gfx.
    /// </summary>
    /// <value>
    /// The gfx.
    /// </value>
    public static IOpenGfx[] Gfx { get; internal set; } = null;

    /// <summary>
    /// Gets the gfx.
    /// </summary>
    /// <value>
    /// The sfx.
    /// </value>
    public static IOpenSfx[] Sfx { get; internal set; } = null;

    ///// <summary>
    ///// Creates the matcher.
    ///// </summary>
    ///// <param name="searchPattern">The searchPattern.</param>
    ///// <returns></returns>
    //public static Func<string, bool> CreateMatcher(string searchPattern) {
    //    if (string.IsNullOrEmpty(searchPattern)) return x => true;
    //    var wildcardCount = searchPattern.Count(x => x.Equals('*'));
    //    if (wildcardCount <= 0) return x => x.Equals(searchPattern, StringComparison.CurrentCultureIgnoreCase);
    //    else if (wildcardCount == 1) {
    //        var newPattern = searchPattern.Replace("*", "");
    //        if (searchPattern.StartsWith("*")) return x => x.EndsWith(newPattern, StringComparison.CurrentCultureIgnoreCase);
    //        else if (searchPattern.EndsWith("*")) return x => x.StartsWith(newPattern, StringComparison.CurrentCultureIgnoreCase);
    //    }
    //    var regexPattern = $"^{Regex.Escape(searchPattern).Replace("\\*", ".*")}$";
    //    return x => {
    //        try { return Regex.IsMatch(x, regexPattern); }
    //        catch { return false; }
    //    };
    //}

    public static string DecodePath(string path, string rootPath = null) => Util.DecodePath(ApplicationPath, path, rootPath);
}

#endregion
