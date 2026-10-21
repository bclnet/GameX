ADR 0010: The WPF Vulkan surface stays on Silk
===

* **Status:** Accepted
* **Date:** 2026-10-19
* **Supersedes:** the deferred-view clause of [ADR 0009](0009-review-silk-vs-opentk.md)

## Context

ADR 0009 left the C# instance on Silk.NET.Vulkan and said a view is not started until the 2027-10-19 review, unless the owner asks. The owner asked for the C# image.

`VulkanInstance.CreateInstance` still enables no extensions. A surface from that instance would fail. The present path is a second instance, `VulkanView`, in `OpenX.Engine.Vulkan`. It enables `VK_KHR_surface` and `VK_KHR_win32_surface`. The extension types come from Silk.NET.Vulkan.Extensions.KHR 2.23.0, referenced by the engine project only.

`VulkanControl` in `OpenX.Wpf.Control` is an `HwndHost`. It registers the window class `GameXVulkanHost`. That window procedure returns 1 for `WM_ERASEBKGND`, because the `Static` class paints button-face gray over the client. The control passes the child HWND to `VulkanView.Create`. `Tick` calls `Draw` after `Renderer.Update`. `OpenX.Wpf.Control` has a project reference to the engine and no Silk package reference.

The chain is one device, one swapchain, one graphics queue that can present, a clear-only render pass, submit, and `QueuePresent`. One semaphore pair is in flight, so `Draw` calls `QueueWaitIdle` after present. The clear is `0.2, 0.3, 0.3, 1`. On this machine the chosen format was `B8G8R8A8Unorm` (the first of that or `B8G8R8A8Srgb`). A top-level window and a WPF window hosting `VulkanControl` both sampled BGRA 76, 76, 51, 255 from the desktop, which is that clear.

OpenTK is not the loader for this surface. ADR 0009's question, whether OpenTK later loads device functions, stays open until 2027-10-19.

Python `VulkanWidget` and Rust `vulkan_control.rs` still only start and update a renderer. The Rust glow callback stays an OpenGL framebuffer.

## Decision

1. **Keep Silk for the C# WPF surface.** `VulkanView` owns the instance, surface, device, and swapchain. The control only supplies the HWND.
2. **Do not add a Silk package reference to `OpenX.Wpf.Control`.** Do not move `VulkanInstance` onto OpenTK.
3. **`VulkanInstance.CreateInstance` stays windowless.** The view instance is the one with the surface extensions.
4. **The first picture is the clear.** `VulkanGfxModel` has its `TextureManager`. `TextureRenderer` loads through it. The view does not sample that texture yet.
5. **Python and Rust stay tick-only** until those hosts can present. Do not paint Vulkan inside the Rust glow callback.
6. **The OpenTK device-proc review date stays 2027-10-19.** That review does not remove this surface. Its outcome is still a new ADR.

## Consequences

* The Explorer `VK` tab presents the clear when the control is shown. The Explorer was not opened for this check. The proof was a short WPF window and a top-level window, then closed.
* A texture source can still throw in `CreateRenderer`. The child HWND is created first, and `Tick` still calls `Draw`.
* Rust and Python do not show this image. That missing twin is recorded, not filled in here.
