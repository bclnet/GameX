Vulkan view conversion
===

Note from 2026-10-19. This is the on-screen view: how a picture gets from an engine onto the Explorer, starting from the OpenGL control and the Vulkan instance. It is not a video-file codec. The loader decision is [ADR 0009](../../ADR/0009-review-silk-vs-opentk.md), and the surface that is on screen is [ADR 0010](../../ADR/0010-wpf-vulkan-surface-stays-on-silk.md). The engine registration note is [Readme.md](Readme.md).

## Where it stands

`VulkanInstance.CreateInstance` is still a windowless instance. `InstanceCreateInfo` enables no extensions. Rust uses `ash` 0.38. Python imports `vulkan` or `pyvulkan` only from that call. The engine id stays `VK`. API names stay `vkCreateInstance`, `VkInstance`, and `VkApi`.

`VulkanView`, in the same engine project, is the picture. It creates a second instance with `VK_KHR_surface` and `VK_KHR_win32_surface`, a Win32 surface, one device, one swapchain, and a clear-only render pass. The extension types are Silk.NET.Vulkan.Extensions.KHR 2.23.0 on the engine project. `OpenX.Wpf.Control` does not reference Silk.

`VulkanControl` copies `OpenGLControl`'s bindings (`Source`, `Source2`, `Path`, `Value`, `Type`). On change it calls `CreateRenderer`, `Start`, and `ITextureSelect.Select`. It is an `HwndHost`. The child class is `GameXVulkanHost`, and its window procedure returns 1 for `WM_ERASEBKGND` so the client is not painted gray. `CompositionTarget.Rendering` calls `Tick`, which calls `Update` and then `VulkanView.Draw`. `AppVulkanControl` subclasses it and calls `VulkanRenderer.CreateRenderer`.

The clear is `0.2, 0.3, 0.3, 1`. This machine's swapchain format was `B8G8R8A8Unorm`, the first of that or `B8G8R8A8Srgb`. A top-level window and a WPF window hosting the control both sampled BGRA 76, 76, 51, 255 from the desktop. The Explorer was not opened. One semaphore pair is in flight, so `Draw` waits with `QueueWaitIdle` after present.

Python `VulkanWidget` (`openx/engines/vulkan/gfx/qt_widget.py`) still only ticks, and the Python Explorer selects it for id `VK`. Rust `openx-wpf-control` `vulkan_control.rs` starts and updates a renderer and does not paint. That present path is not ported.

`TestTriRenderer` and `TextureRenderer` extend `Renderer` (`Start`, `Stop`, `Update`). They do not draw. `VulkanGfxModel` leaves its managers null, so `TextureRenderer`'s constructor throws before `Start`. A game texture is not the first picture.

## What OpenGL already does

`OpenX.Engine.OpenGL` uses OpenTK 5.0.0-pre.16. `GLWpfControl` subclasses `OpenTK.Wpf.GLWpfControl`. `DXGLContext` builds a hidden GLFW `NativeWindow` and reads the Win32 HWND:

`OpenX.Wpf.Control/OpenTK/Wpf/DXGLContext.cs` calls `GLFW.GetWin32Window`, then `GetDC`, then checks `WGL_NV_DX_interop`. The image reaches WPF through Direct3D9Ex. The WinForms `GLControl` reparents that same HWND with `SetParent`.

The clear color is `0.2, 0.3, 0.3, 1`.

## Silk compared with the OpenTK window

OpenTK hides the window. Silk does not have a window type. The HWND is passed in by hand. On WPF that HWND is the one `DXGLContext` already reads.

| OpenTK | Vulkan call | Silk |
| --- | --- | --- |
| `VKLoader.Init()` | load `vulkan-1.dll` and `vkGetInstanceProcAddr` | `Vk.GetApi()`, already used by `VulkanInstance` |
| `Toolkit.Vulkan.GetRequiredInstanceExtensions()` | `VK_KHR_surface` and `VK_KHR_win32_surface` | those two names on `InstanceCreateInfo.PpEnabledExtensionNames`. The current instance enables none, so a later surface fails |
| `VKLoader.SetInstance(instance)` | later calls use that instance | `vk.CurrentInstance = instance` |
| `Toolkit.Vulkan.CreateWindowSurface(instance, window)` | `vkCreateWin32SurfaceKHR` | `KhrWin32Surface.CreateWin32Surface` with `Hwnd` and `Hinstance` |
| device commands | `vkGetDeviceProcAddr` | `vk.CurrentDevice = device`, then `TryGetDeviceExtension` for `KhrSwapchain`. OpenTK 5 has no `vkGetDeviceProcAddr` |

ADR 0009 still reviews OpenTK's device-proc loading on 2027-10-19. ADR 0010 is the surface: Silk stays, inside `OpenX.Engine.Vulkan`. Do not add Silk.NET.Vulkan to `OpenX.Wpf.Control`. Do not move `VulkanInstance` onto OpenTK. Rust stays on `ash`. Python stays on `vulkan`.

## What the clear does

`VulkanView.Draw` reads the client size, builds the swapchain when that size changes, and records a render pass with no draw calls. The steps that are in place:

1. The HWND is the `HwndHost` child, not a GLFW window and not the OpenGL D3DImage path.
2. `VkSurfaceKHR` comes from that HWND.
3. The physical device is the first one whose graphics queue supports that surface.
4. The device enables `VK_KHR_swapchain`. One queue draws and presents.
5. The swapchain uses FIFO. Each image has a view and a framebuffer.
6. The command buffer begins the render pass and ends it. Submit, present, then `QueueWaitIdle`.

`TestTriRenderer` and `TextureRenderer` still do not draw. `VulkanGfxModel` leaves its managers null, so `TextureRenderer`'s constructor throws before `Start`. A game texture is the next picture, after those managers exist. The clear does not wait on that.

## Rust has to host more than OpenGL

The Rust OpenGL view paints inside eframe's glow context. `GLWpfControl.ui` adds an `egui_glow::CallbackFn`, and `setGL` takes `painter.gl()`. That callback is an OpenGL framebuffer. Vulkan cannot present there. SDL and an embedded engine cannot either.

The viewer hands each engine a surface and a size:

| Viewer | Present target |
| --- | --- |
| OpenGL | the existing egui glow callback |
| Vulkan | a HWND or an SDL window, then the same surface and swapchain as C#. `ash` creates the Win32 surface, or SDL creates it with `SDL_Vulkan_CreateSurface` |
| SDL | the SDL window and its loop |
| Embedded engines | a child HWND. C# `ShellControl` already reparents one with `SetParent` |

`vulkan_control.rs` stays tick-only until the Explorer can host more than the glow callback. The C# present path is not ported there. The Rust Explorer does not list `VK` today. C# `App.xaml.cs` does list `VulkanEngine.This`.
