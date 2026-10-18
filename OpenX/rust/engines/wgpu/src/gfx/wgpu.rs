// PORT-SOURCE: Engines/OpenX.Engine.Wgpu/Gfx/Wgpu.cs
// PORT-SHA: 4487cdf887dda3c7
// PORT-STATUS: done
//
// `BuildersByType` is a `TypeId` map: Rust has no `System.Type`. The builder
// fn is the nearest equivalent of `Func<object, bool, object, object>`.
// `WgpuInstance` creates a wgpu 22.1.0 instance and one adapter. No surface.
// Fields drop in declaration order. The adapter keeps its own backend
// reference, so the instance dropping first is safe.

use std::any::{Any, TypeId};
use std::collections::HashMap;
use std::sync::{LazyLock, Mutex};

pub type Builder = fn(&(dyn Any + Send), bool, &(dyn Any + Send)) -> Option<Box<dyn Any + Send>>;

/// WgpuX
pub struct WgpuX;

impl WgpuX {
    /// Builders keyed by source type.
    pub fn buildersByType() -> std::sync::MutexGuard<'static, HashMap<TypeId, Builder>> {
        BuildersByType.lock().unwrap_or_else(|e| e.into_inner())
    }
}

/// C# `WgpuX.BuildersByType`.
pub static BuildersByType: LazyLock<Mutex<HashMap<TypeId, Builder>>> = LazyLock::new(|| Mutex::new(HashMap::new()));

/// A wgpu instance and adapter. No window and no surface.
/// `instance` drops first. `adapter` keeps its own backend reference.
pub struct WgpuInstance {
    instance: wgpu::Instance,
    adapter: wgpu::Adapter,
}

impl WgpuInstance {
    pub fn adapter(&self) -> &wgpu::Adapter { &self.adapter }

    /// Creates an instance and requests an adapter. `Err` when no adapter is found.
    pub fn createInstance() -> Result<Self, String> {
        let instance = wgpu::Instance::new(wgpu::InstanceDescriptor::default());
        let Some(adapter) = instance.enumerate_adapters(wgpu::Backends::all()).into_iter().next() else {
            return Err("wgpu found no adapter.".to_string());
        };
        Ok(Self { instance, adapter })
    }
}
