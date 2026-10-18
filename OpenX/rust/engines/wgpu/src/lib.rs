//! `openx-engine-wgpu` — wgpu engine provider.
//!
//! `WgpuEngine::This` registers id `WG` / name `Wgpu`. `WgpuInstance::createInstance`
//! creates a `wgpu` 22.1.0 instance, requests an adapter with no surface, and drops
//! both. Sprite, model, and host slots are not ported.

pub mod engine_wgpu;
pub mod gfx;
