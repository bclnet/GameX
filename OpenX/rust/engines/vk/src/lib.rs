//! `openx-engine-vk` — Vulkan engine provider.
//!
//! `VkEngine::This` registers id `VK` / name `Vulkan`. `VkInstance::createInstance`
//! calls `vkCreateInstance` through `ash` and destroys the instance on drop.
//! Sprite, model, and host slots are not ported.

pub mod engine_vk;
pub mod gfx;
