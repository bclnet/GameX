Vulkan
===============

Engine id: `VK`. The spelling in code is Vulkan. This folder keeps the historical name Vulken.

This slice registers the engine (caps: Drawing) and creates a Vulkan instance with `vkCreateInstance`, then destroys it with `vkDestroyInstance`. C# uses the Silk.NET.Vulkan package, Rust uses the `ash` crate, and Python calls `vulkan` or `pyvulkan` only from `createInstance` (importing the engine module does not require that package). There is no window and no swapchain. It is not a port of the OpenGL shader stack. Host, sprite, and model methods that Ogre leaves unimplemented still throw.
