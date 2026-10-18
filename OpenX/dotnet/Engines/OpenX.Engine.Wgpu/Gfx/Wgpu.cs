using System;
using System.Collections.Generic;

namespace OpenX.Gfx.Wgpu;

#region Extensions

/// <summary>
/// WgpuX
/// </summary>
public static class WgpuX {
    /// <summary>
    /// Builders keyed by source type.
    /// </summary>
    public static Dictionary<Type, Func<object, bool, object, object>> BuildersByType = [];
}

#endregion

#region WgpuInstance

/// <summary>
/// A wgpu instance and adapter. Calls Instance.Create and RequestAdapterBlocking. No window and no surface.
/// </summary>
public sealed class WgpuInstance : IDisposable {
    readonly Ahjo.Wgpu.Instance _instance;
    readonly Ahjo.Wgpu.Adapter _adapter;
    bool _disposed;

    WgpuInstance(Ahjo.Wgpu.Instance instance, Ahjo.Wgpu.Adapter adapter) {
        _instance = instance;
        _adapter = adapter;
    }

    /// <summary>
    /// The native adapter.
    /// </summary>
    public Ahjo.Wgpu.Adapter Adapter => _adapter;

    /// <summary>
    /// Creates a wgpu instance and requests an adapter. Throws when the native library is missing or no adapter is found.
    /// </summary>
    public static WgpuInstance CreateInstance() {
        Ahjo.Wgpu.Instance instance;
        try { instance = Ahjo.Wgpu.Instance.Create(); }
        catch (Exception e) { throw new InvalidOperationException("wgpu instance create failed.", e); }
        try {
            var adapter = instance.RequestAdapterBlocking();
            if (adapter is null) throw new InvalidOperationException("wgpu found no adapter.");
            return new WgpuInstance(instance, adapter);
        }
        catch (InvalidOperationException) {
            instance.Dispose();
            throw;
        }
        catch (Exception e) {
            instance.Dispose();
            throw new InvalidOperationException("wgpu adapter request failed.", e);
        }
    }

    /// <summary>
    /// Releases the adapter, then the instance.
    /// </summary>
    public void Dispose() {
        if (_disposed) return;
        _disposed = true;
        _adapter.Dispose();
        _instance.Dispose();
    }
}

#endregion
