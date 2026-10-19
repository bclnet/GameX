Vulkan
===============

Engine id: `VK`. The spelling in code is Vulkan. This folder keeps the historical name Vulken.

This slice registers the engine (caps: Drawing) and creates a Vulkan instance with `vkCreateInstance`, then destroys it with `vkDestroyInstance`. C# uses Silk.NET.Vulkan and Silk.NET.Vulkan.Extensions.KHR, Rust uses the `ash` crate, and Python calls `vulkan` or `pyvulkan` only from `createInstance` (importing the engine module does not require that package). `VulkanInstance` has no window and no swapchain. `VulkanView` presents a clear on a child HWND. The loader note is [ADR 0010](../../ADR/0010-wpf-vulkan-surface-stays-on-silk.md). It is not a port of the OpenGL shader stack. Host, sprite, and model methods that Ogre leaves unimplemented still throw. The view conversion is in [View.md](View.md).
