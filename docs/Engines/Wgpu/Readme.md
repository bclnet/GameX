Wgpu
====

Engine id: `WG`. This slice registers the engine (caps: Drawing) and creates a wgpu instance, requests an adapter with no window and no surface, then drops both. It does not draw. Host, sprite, and model methods throw. The Explorer does not list it.

C# uses Ahjo.Wgpu 0.7.0 (net10.0). That package ships wgpu-native. `WgpuInstance.CreateInstance` calls `Instance.Create` and `RequestAdapterBlocking`, then `Dispose` releases the adapter and the instance.

Rust uses the `wgpu` crate pinned at 22.1.0. That release's MSRV is 1.74, so it fits the workspace rust-version 1.75. Current wgpu (30.x) needs Rust 1.87 and was not used. naga 22 accepts any indexmap 2.x, and 2.12+ needs Rust 1.82. The OpenX lock keeps indexmap 2.2.6, the version wgpu 22.1.0 locked. It is not a direct pin: the GameX workspace already depends on wgpu 30 through eframe, and that needs indexmap 2.11 or newer. `WgpuInstance::createInstance` calls `Instance::new` and `enumerate_adapters` (no surface). `Drop` releases the adapter, then the instance. An empty adapter list is `Err`, not a panic.

Python imports the `wgpu` package (wgpu-py) only inside `createInstance`. Importing the engine module does not require it. The call is `wgpu.gpu.request_adapter_sync()` with no canvas, and `dispose` drops the adapter. `wgpu.gpu` is that package's instance.
