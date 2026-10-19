using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using System;
using System.IO;
using System.Runtime.InteropServices;
using VkApi = Silk.NET.Vulkan.Vk;

namespace OpenX.Gfx.Vulkan;

/// <summary>
/// A child-window Vulkan image. Clears to the OpenGL control color. One device, one swapchain.
/// </summary>
public sealed unsafe class VulkanView : IDisposable {
    const uint SubpassExternal = ~0u;

    readonly VkApi _vk;
    readonly IntPtr _hwnd;
    readonly Instance _instance;
    readonly KhrSurface _surfaceApi;
    readonly KhrWin32Surface _win32;
    readonly SurfaceKHR _surface;
    readonly PhysicalDevice _physical;
    readonly Device _device;
    readonly Queue _queue;
    readonly uint _family;
    readonly KhrSwapchain _swapchainApi;
    readonly Format _format;
    readonly ColorSpaceKHR _colorSpace;
    readonly RenderPass _renderPass;
    readonly CommandPool _pool;
    readonly CommandBuffer _command;
    readonly Silk.NET.Vulkan.Semaphore _imageAvailable;
    readonly Silk.NET.Vulkan.Semaphore _renderFinished;
    readonly Fence _fence;

    SwapchainKHR _swapchain;
    ImageView[] _views = [];
    Framebuffer[] _framebuffers = [];
    Extent2D _extent;
    int _width;
    int _height;
    bool _failed;
    bool _disposed;

    VulkanView(VkApi vk, IntPtr hwnd, Instance instance, KhrSurface surfaceApi, KhrWin32Surface win32, SurfaceKHR surface, PhysicalDevice physical, Device device, Queue queue, uint family, KhrSwapchain swapchainApi, Format format, ColorSpaceKHR colorSpace, RenderPass renderPass, CommandPool pool, CommandBuffer command, Silk.NET.Vulkan.Semaphore imageAvailable, Silk.NET.Vulkan.Semaphore renderFinished, Fence fence) {
        _vk = vk;
        _hwnd = hwnd;
        _instance = instance;
        _surfaceApi = surfaceApi;
        _win32 = win32;
        _surface = surface;
        _physical = physical;
        _device = device;
        _queue = queue;
        _family = family;
        _swapchainApi = swapchainApi;
        _format = format;
        _colorSpace = colorSpace;
        _renderPass = renderPass;
        _pool = pool;
        _command = command;
        _imageAvailable = imageAvailable;
        _renderFinished = renderFinished;
        _fence = fence;
    }

    /// <summary>
    /// Creates the instance, surface, device, and render pass for <paramref name="hwnd"/>.
    /// </summary>
    public static VulkanView Create(IntPtr hwnd, IntPtr hinstance) {
        VkApi vk;
        try { vk = VkApi.GetApi(); }
        catch (Exception e) when (e is DllNotFoundException or FileNotFoundException or EntryPointNotFoundException or TypeInitializationException) {
            throw new InvalidOperationException("Vulkan loader is missing.", e);
        }
        var instance = CreateInstance(vk);
        vk.CurrentInstance = instance;
        if (!vk.TryGetInstanceExtension(instance, out KhrSurface surfaceApi, null)) throw new InvalidOperationException("VK_KHR_surface is missing.");
        if (!vk.TryGetInstanceExtension(instance, out KhrWin32Surface win32, null)) throw new InvalidOperationException("VK_KHR_win32_surface is missing.");
        var surface = CreateSurface(win32, instance, hwnd, hinstance);
        if (!PickDevice(vk, surfaceApi, instance, surface, out var physical, out var family)) throw new InvalidOperationException("No Vulkan device can present to this window.");
        var device = CreateDevice(vk, physical, family);
        vk.CurrentDevice = device;
        if (!vk.TryGetDeviceExtension(instance, device, out KhrSwapchain swapchainApi, null)) throw new InvalidOperationException("VK_KHR_swapchain is missing.");
        vk.GetDeviceQueue(device, family, 0, out var queue);
        uint formatCount = 0;
        surfaceApi.GetPhysicalDeviceSurfaceFormats(physical, surface, ref formatCount, null);
        var formats = new SurfaceFormatKHR[formatCount];
        fixed (SurfaceFormatKHR* formatPtr = formats)
            surfaceApi.GetPhysicalDeviceSurfaceFormats(physical, surface, ref formatCount, formatPtr);
        var chosen = formats[0];
        foreach (var format in formats)
            if (format.Format == Format.B8G8R8A8Srgb || format.Format == Format.B8G8R8A8Unorm) { chosen = format; break; }
        var renderPass = CreateRenderPass(vk, device, chosen.Format);
        var pool = CreatePool(vk, device, family);
        var allocate = new CommandBufferAllocateInfo { SType = StructureType.CommandBufferAllocateInfo, CommandPool = pool, Level = CommandBufferLevel.Primary, CommandBufferCount = 1 };
        vk.AllocateCommandBuffers(device, in allocate, out var command);
        var imageAvailable = CreateSemaphore(vk, device);
        var renderFinished = CreateSemaphore(vk, device);
        var fenceInfo = new FenceCreateInfo { SType = StructureType.FenceCreateInfo, Flags = FenceCreateFlags.SignaledBit };
        Check(vk.CreateFence(device, in fenceInfo, null, out var fence), "vkCreateFence");
        return new VulkanView(vk, hwnd, instance, surfaceApi, win32, surface, physical, device, queue, family, swapchainApi, chosen.Format, chosen.ColorSpace, renderPass, pool, command, imageAvailable, renderFinished, fence);
    }

    /// <summary>
    /// Clears the window. Recreates the swapchain when the client size changes.
    /// </summary>
    public void Draw() {
        if (_disposed || _failed) return;
        try { DrawCore(); }
        catch (Exception e) {
            _failed = true;
            Console.Error.WriteLine($"Vulkan view: {e.Message}");
        }
    }

    void DrawCore() {
        GetClientRect(_hwnd, out var rect);
        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;
        if (width <= 0 || height <= 0) return;
        if (width != _width || height != _height || _swapchain.Handle == 0) {
            _vk.DeviceWaitIdle(_device);
            DestroySwapchain();
            CreateSwapchain(width, height);
            _width = width;
            _height = height;
        }
        if (_swapchain.Handle == 0) return;
        _vk.WaitForFences(_device, 1, in _fence, true, ulong.MaxValue);
        uint image = 0;
        var acquired = _swapchainApi.AcquireNextImage(_device, _swapchain, ulong.MaxValue, _imageAvailable, default, ref image);
        if (acquired == Result.ErrorOutOfDateKhr) { _width = 0; return; }
        Check(acquired, "vkAcquireNextImageKHR");
        _vk.ResetFences(_device, 1, in _fence);
        Record(image);
        var wait = _imageAvailable;
        var signal = _renderFinished;
        var stage = PipelineStageFlags.ColorAttachmentOutputBit;
        var command = _command;
        var submit = new SubmitInfo {
            SType = StructureType.SubmitInfo,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = &wait,
            PWaitDstStageMask = &stage,
            CommandBufferCount = 1,
            PCommandBuffers = &command,
            SignalSemaphoreCount = 1,
            PSignalSemaphores = &signal,
        };
        Check(_vk.QueueSubmit(_queue, 1, in submit, _fence), "vkQueueSubmit");
        var chain = _swapchain;
        var present = new PresentInfoKHR {
            SType = StructureType.PresentInfoKhr,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = &signal,
            SwapchainCount = 1,
            PSwapchains = &chain,
            PImageIndices = &image,
        };
        var presented = _swapchainApi.QueuePresent(_queue, in present);
        if (presented == Result.ErrorOutOfDateKhr || presented == Result.SuboptimalKhr) _width = 0;
        else Check(presented, "vkQueuePresentKHR");
        // One pair of semaphores. The fence covers the submit, not the present that waits on it.
        _vk.QueueWaitIdle(_queue);
    }

    void Record(uint image) {
        _vk.ResetCommandBuffer(_command, CommandBufferResetFlags.None);
        var begin = new CommandBufferBeginInfo { SType = StructureType.CommandBufferBeginInfo, Flags = CommandBufferUsageFlags.OneTimeSubmitBit };
        Check(_vk.BeginCommandBuffer(_command, in begin), "vkBeginCommandBuffer");
        var clear = new ClearValue { Color = new ClearColorValue { Float32_0 = 0.2f, Float32_1 = 0.3f, Float32_2 = 0.3f, Float32_3 = 1f } };
        var pass = new RenderPassBeginInfo {
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = _renderPass,
            Framebuffer = _framebuffers[image],
            RenderArea = new Rect2D { Extent = _extent },
            ClearValueCount = 1,
            PClearValues = &clear,
        };
        _vk.CmdBeginRenderPass(_command, in pass, SubpassContents.Inline);
        _vk.CmdEndRenderPass(_command);
        Check(_vk.EndCommandBuffer(_command), "vkEndCommandBuffer");
    }

    void CreateSwapchain(int width, int height) {
        _surfaceApi.GetPhysicalDeviceSurfaceCapabilities(_physical, _surface, out var caps);
        var extent = caps.CurrentExtent;
        if (extent.Width == uint.MaxValue) {
            extent.Width = (uint)Math.Clamp(width, (int)caps.MinImageExtent.Width, (int)caps.MaxImageExtent.Width);
            extent.Height = (uint)Math.Clamp(height, (int)caps.MinImageExtent.Height, (int)caps.MaxImageExtent.Height);
        }
        if (extent.Width == 0 || extent.Height == 0) return;
        var imageCount = caps.MinImageCount + 1;
        if (caps.MaxImageCount > 0 && imageCount > caps.MaxImageCount) imageCount = caps.MaxImageCount;
        var alpha = CompositeAlphaFlagsKHR.OpaqueBitKhr;
        if ((caps.SupportedCompositeAlpha & CompositeAlphaFlagsKHR.OpaqueBitKhr) == 0) alpha = CompositeAlphaFlagsKHR.InheritBitKhr;
        var info = new SwapchainCreateInfoKHR {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _surface,
            MinImageCount = imageCount,
            ImageFormat = _format,
            ImageColorSpace = _colorSpace,
            ImageExtent = extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            ImageSharingMode = SharingMode.Exclusive,
            PreTransform = caps.CurrentTransform,
            CompositeAlpha = alpha,
            PresentMode = PresentModeKHR.FifoKhr,
            Clipped = true,
        };
        Check(_swapchainApi.CreateSwapchain(_device, in info, null, out _swapchain), "vkCreateSwapchainKHR");
        uint count = 0;
        _swapchainApi.GetSwapchainImages(_device, _swapchain, ref count, null);
        var images = new Image[count];
        fixed (Image* imagePtr = images)
            _swapchainApi.GetSwapchainImages(_device, _swapchain, ref count, imagePtr);
        _extent = extent;
        _views = new ImageView[count];
        _framebuffers = new Framebuffer[count];
        for (var i = 0u; i < count; i++) {
            var view = new ImageViewCreateInfo {
                SType = StructureType.ImageViewCreateInfo,
                Image = images[i],
                ViewType = ImageViewType.Type2D,
                Format = _format,
                SubresourceRange = new ImageSubresourceRange { AspectMask = ImageAspectFlags.ColorBit, LevelCount = 1, LayerCount = 1 },
            };
            Check(_vk.CreateImageView(_device, in view, null, out _views[i]), "vkCreateImageView");
            var attachment = _views[i];
            var framebuffer = new FramebufferCreateInfo {
                SType = StructureType.FramebufferCreateInfo,
                RenderPass = _renderPass,
                AttachmentCount = 1,
                PAttachments = &attachment,
                Width = extent.Width,
                Height = extent.Height,
                Layers = 1,
            };
            Check(_vk.CreateFramebuffer(_device, in framebuffer, null, out _framebuffers[i]), "vkCreateFramebuffer");
        }
    }

    void DestroySwapchain() {
        if (_swapchain.Handle == 0) return;
        foreach (var framebuffer in _framebuffers) _vk.DestroyFramebuffer(_device, framebuffer, null);
        foreach (var view in _views) _vk.DestroyImageView(_device, view, null);
        _swapchainApi.DestroySwapchain(_device, _swapchain, null);
        _swapchain = default;
        _framebuffers = [];
        _views = [];
    }

    /// <summary>Destroys the swapchain, device, surface, and instance.</summary>
    public void Dispose() {
        if (_disposed) return;
        _disposed = true;
        _vk.DeviceWaitIdle(_device);
        DestroySwapchain();
        _vk.DestroyFence(_device, _fence, null);
        _vk.DestroySemaphore(_device, _renderFinished, null);
        _vk.DestroySemaphore(_device, _imageAvailable, null);
        _vk.DestroyCommandPool(_device, _pool, null);
        _vk.DestroyRenderPass(_device, _renderPass, null);
        _swapchainApi.Dispose();
        _vk.DestroyDevice(_device, null);
        _surfaceApi.DestroySurface(_instance, _surface, null);
        _win32.Dispose();
        _surfaceApi.Dispose();
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
    }

    static Instance CreateInstance(VkApi vk) {
        var names = new[] { "VK_KHR_surface", "VK_KHR_win32_surface" };
        var ext = SilkMarshal.StringArrayToPtr(names);
        var appName = SilkMarshal.StringToPtr("GameX");
        var engineName = SilkMarshal.StringToPtr("Vulkan");
        try {
            var app = new ApplicationInfo {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)appName,
                ApplicationVersion = VkApi.MakeVersion(1, 0, 0),
                PEngineName = (byte*)engineName,
                EngineVersion = VkApi.MakeVersion(1, 0, 0),
                ApiVersion = VkApi.Version10,
            };
            var info = new InstanceCreateInfo {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &app,
                EnabledExtensionCount = (uint)names.Length,
                PpEnabledExtensionNames = (byte**)ext,
            };
            Check(vk.CreateInstance(in info, null, out var instance), "vkCreateInstance");
            return instance;
        }
        finally {
            SilkMarshal.Free(ext);
            SilkMarshal.Free(appName);
            SilkMarshal.Free(engineName);
        }
    }

    static SurfaceKHR CreateSurface(KhrWin32Surface win32, Instance instance, IntPtr hwnd, IntPtr hinstance) {
        var info = new Win32SurfaceCreateInfoKHR { SType = StructureType.Win32SurfaceCreateInfoKhr, Hwnd = hwnd, Hinstance = hinstance };
        Check(win32.CreateWin32Surface(instance, in info, null, out var surface), "vkCreateWin32SurfaceKHR");
        return surface;
    }

    static bool PickDevice(VkApi vk, KhrSurface surfaceApi, Instance instance, SurfaceKHR surface, out PhysicalDevice physical, out uint family) {
        uint count = 0;
        vk.EnumeratePhysicalDevices(instance, ref count, null);
        var devices = new PhysicalDevice[count];
        fixed (PhysicalDevice* devicePtr = devices)
            vk.EnumeratePhysicalDevices(instance, ref count, devicePtr);
        foreach (var device in devices) {
            uint queues = 0;
            vk.GetPhysicalDeviceQueueFamilyProperties(device, ref queues, null);
            var props = new QueueFamilyProperties[queues];
            fixed (QueueFamilyProperties* propPtr = props)
                vk.GetPhysicalDeviceQueueFamilyProperties(device, ref queues, propPtr);
            for (var i = 0u; i < queues; i++) {
                surfaceApi.GetPhysicalDeviceSurfaceSupport(device, i, surface, out var supported);
                if ((props[i].QueueFlags & QueueFlags.GraphicsBit) != 0 && supported) { physical = device; family = i; return true; }
            }
        }
        physical = default;
        family = 0;
        return false;
    }

    static Device CreateDevice(VkApi vk, PhysicalDevice physical, uint family) {
        var priority = 1f;
        var queue = new DeviceQueueCreateInfo { SType = StructureType.DeviceQueueCreateInfo, QueueFamilyIndex = family, QueueCount = 1, PQueuePriorities = &priority };
        var name = SilkMarshal.StringToPtr("VK_KHR_swapchain");
        try {
            var info = new DeviceCreateInfo { SType = StructureType.DeviceCreateInfo, QueueCreateInfoCount = 1, PQueueCreateInfos = &queue, EnabledExtensionCount = 1, PpEnabledExtensionNames = (byte**)&name };
            Check(vk.CreateDevice(physical, in info, null, out var device), "vkCreateDevice");
            return device;
        }
        finally { SilkMarshal.Free(name); }
    }

    static RenderPass CreateRenderPass(VkApi vk, Device device, Format format) {
        var attachment = new AttachmentDescription {
            Format = format,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.PresentSrcKhr,
        };
        var reference = new AttachmentReference { Attachment = 0, Layout = ImageLayout.ColorAttachmentOptimal };
        var subpass = new SubpassDescription { PipelineBindPoint = PipelineBindPoint.Graphics, ColorAttachmentCount = 1, PColorAttachments = &reference };
        var dependency = new SubpassDependency {
            SrcSubpass = SubpassExternal,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit,
        };
        var info = new RenderPassCreateInfo {
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = 1,
            PAttachments = &attachment,
            SubpassCount = 1,
            PSubpasses = &subpass,
            DependencyCount = 1,
            PDependencies = &dependency,
        };
        Check(vk.CreateRenderPass(device, in info, null, out var pass), "vkCreateRenderPass");
        return pass;
    }

    static CommandPool CreatePool(VkApi vk, Device device, uint family) {
        var info = new CommandPoolCreateInfo { SType = StructureType.CommandPoolCreateInfo, Flags = CommandPoolCreateFlags.ResetCommandBufferBit, QueueFamilyIndex = family };
        Check(vk.CreateCommandPool(device, in info, null, out var pool), "vkCreateCommandPool");
        return pool;
    }

    static Silk.NET.Vulkan.Semaphore CreateSemaphore(VkApi vk, Device device) {
        var info = new SemaphoreCreateInfo { SType = StructureType.SemaphoreCreateInfo };
        Check(vk.CreateSemaphore(device, in info, null, out var semaphore), "vkCreateSemaphore");
        return semaphore;
    }

    static void Check(Result result, string call) {
        if (result != Result.Success) throw new InvalidOperationException($"{call} failed: {result}");
    }

    [DllImport("user32.dll")]
    static extern bool GetClientRect(IntPtr hwnd, out Rect rect);

    struct Rect { public int Left, Top, Right, Bottom; }
}
