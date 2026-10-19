ADR 0009: Recheck Silk.NET.Vulkan against OpenTK
===

* **Status:** Accepted
* **Date:** 2026-10-19
* **Review by:** 2027-10-19

## Context

`OpenX.Engine.Vulkan` creates a windowless instance with Silk.NET.Vulkan 2.23.0. `VulkanInstance.CreateInstance` calls `vkCreateInstance` and `vkDestroyInstance`. There is no surface and no swapchain. Rust uses `ash` 0.38. Python imports `vulkan` or `pyvulkan` only from that call. The engine id stays `VK`.

`OpenX.Engine.OpenGL` uses OpenTK 5.0.0-pre.16. Its WPF host, `OpenTK.Wpf.GLWpfControl`, builds a hidden GLFW `NativeWindow` and reads the Win32 HWND with `GLFW.GetWin32Window` (`OpenX.Wpf.Control/OpenTK/Wpf/DXGLContext.cs`). That HWND supplies the device context for `WGL_NV_DX_interop`, and the image reaches WPF through Direct3D9Ex. The WinForms `GLControl` reparents the same HWND with `SetParent`.

OpenTK 5 also binds Vulkan. `VKLoader.Init` loads `vulkan-1.dll` and `vkGetInstanceProcAddr`. `VKLoader.SetInstance` loads further commands after the instance exists. `Toolkit.Vulkan.CreateWindowSurface` takes an OpenTK `WindowHandle`, the same kind of window the OpenGL host already creates. The published loader does not call `vkGetDeviceProcAddr` and has no per-device entry points. Silk's `Vk` object keeps a current instance and a current device, and device commands go through `vkGetDeviceProcAddr`.

`AppVulkanControl` still subclasses `OpenGLControl`, so the Explorer `VK` tab is an OpenGL context. No Vulkan surface was added to `OpenX.Wpf.Control`.

## Decision

1. **Leave the C# instance on Silk.NET.Vulkan until this review.** Do not add Silk to `OpenX.Wpf.Control`. Do not move `VulkanInstance` onto OpenTK as a side effect of other work.
2. **Recheck this choice by 19 October 2027**, or sooner if a Vulkan window, surface, or swapchain is about to be written, or if OpenTK ships `vkGetDeviceProcAddr`.
3. **The recheck answers these questions:**
   * Does OpenTK load device functions, including more than one device and device-only extensions?
   * Can `Toolkit.Vulkan.CreateWindowSurface` use the same `NativeWindow` and HWND path as `DXGLContext` and `GLControl`?
   * Does one OpenTK package for OpenGL and Vulkan remove a second loader from the WPF control?
   * What does Silk.NET.Vulkan still cover that this OpenTK package does not?
   * Rust stays on `ash` and Python stays on `vulkan` unless that port's binding is the question. This review is the C# loader.
4. **The outcome is a new ADR:** keep Silk and set the next review date, or move the C# instance and any WPF surface onto OpenTK.

## Consequences

* The windowless Vulkan instance stays on Silk, and the provider keeps registering.
* A Vulkan view is not started on either loader until the review, unless the owner asks for that view.
* The engine id `VK` is unchanged. Rust and Python are outside this choice.
