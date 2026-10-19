//! `openx-engine-vulkan` — Vulkan engine provider.
//!
//! `VulkanEngine::This` registers id `VK` / name `Vulkan`. `VulkanInstance::createInstance`
//! calls `vkCreateInstance` through `ash` and destroys the instance on drop.
//! Sprite, model, and host slots are not ported.

pub mod engine_vulkan;
pub mod gfx;
