using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.IO;
using VkApi = Silk.NET.Vulkan.Vk;

namespace OpenX.Gfx.Vk;

#region Extensions

/// <summary>
/// VkX
/// </summary>
public static class VkX {
    /// <summary>
    /// Builders keyed by source type.
    /// </summary>
    public static Dictionary<Type, Func<object, bool, object, object>> BuildersByType = [];
}

#endregion

#region VkInstance

/// <summary>
/// A Vulkan instance. Calls vkCreateInstance and vkDestroyInstance. No window and no swapchain.
/// </summary>
public sealed class VkInstance : IDisposable {
    readonly VkApi _vk;
    readonly Instance _instance;
    bool _disposed;

    VkInstance(VkApi vk, Instance instance) {
        _vk = vk;
        _instance = instance;
    }

    /// <summary>
    /// The native VkInstance handle.
    /// </summary>
    public Instance Handle => _instance;

    /// <summary>
    /// Calls vkCreateInstance. Throws when the loader or an ICD is missing.
    /// </summary>
    public static unsafe VkInstance CreateInstance(string applicationName = "GameX") {
        VkApi vk;
        try { vk = VkApi.GetApi(); }
        catch (Exception e) when (e is DllNotFoundException or FileNotFoundException or EntryPointNotFoundException or TypeInitializationException) {
            throw new InvalidOperationException("Vulkan loader is missing.", e);
        }
        var appName = SilkMarshal.StringToPtr(string.IsNullOrEmpty(applicationName) ? "GameX" : applicationName);
        var engineName = SilkMarshal.StringToPtr("Vulkan");
        try {
            var appInfo = new ApplicationInfo {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)appName,
                ApplicationVersion = VkApi.MakeVersion(1, 0, 0),
                PEngineName = (byte*)engineName,
                EngineVersion = VkApi.MakeVersion(1, 0, 0),
                ApiVersion = VkApi.Version10,
            };
            var createInfo = new InstanceCreateInfo {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo,
            };
            var result = vk.CreateInstance(in createInfo, null, out Instance instance);
            if (result != Result.Success) {
                vk.Dispose();
                throw new InvalidOperationException($"vkCreateInstance failed: {result}");
            }
            return new VkInstance(vk, instance);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception e) {
            vk.Dispose();
            throw new InvalidOperationException("vkCreateInstance failed.", e);
        }
        finally {
            SilkMarshal.Free(appName);
            SilkMarshal.Free(engineName);
        }
    }

    /// <summary>
    /// Calls vkDestroyInstance.
    /// </summary>
    public unsafe void Dispose() {
        if (_disposed) return;
        _disposed = true;
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
    }
}

#endregion
