// PORT-SOURCE: Engines/OpenX.Engine.Vulkan/Gfx/Vulkan.cs
// PORT-SHA: 46a94a10a6d0d683
// PORT-STATUS: done
//
// `BuildersByType` is a `TypeId` map: Rust has no `System.Type`. The builder
// fn is the nearest equivalent of `Func<object, bool, object, object>`.

use std::any::{Any, TypeId};
use std::collections::HashMap;
use std::ffi::CString;
use std::sync::{LazyLock, Mutex};
use ash::{vk, Entry, Instance};

pub type Builder = fn(&(dyn Any + Send), bool, &(dyn Any + Send)) -> Option<Box<dyn Any + Send>>;

/// VulkanX
pub struct VulkanX;

impl VulkanX {
    /// Builders keyed by source type.
    pub fn buildersByType() -> std::sync::MutexGuard<'static, HashMap<TypeId, Builder>> {
        BuildersByType.lock().unwrap_or_else(|e| e.into_inner())
    }
}

/// C# `VulkanX.BuildersByType`.
pub static BuildersByType: LazyLock<Mutex<HashMap<TypeId, Builder>>> = LazyLock::new(|| Mutex::new(HashMap::new()));

/// A Vulkan instance. `instance` is declared first so it drops before `entry`.
/// `Drop` calls vkDestroyInstance while both fields are still alive.
pub struct VulkanInstance {
    instance: Instance,
    entry: Entry,
}

impl VulkanInstance {
    pub fn handle(&self) -> vk::Instance { self.instance.handle() }

    /// Calls vkCreateInstance. `Err` when the loader or an ICD is missing.
    pub fn createInstance() -> Result<Self, String> {
        let entry = unsafe { Entry::load() }.map_err(|e| format!("Vulkan loader is missing: {e}"))?;
        let app_name = CString::new("GameX").map_err(|e| e.to_string())?;
        let engine_name = CString::new("Vulkan").map_err(|e| e.to_string())?;
        let app_info = vk::ApplicationInfo::default()
            .application_name(&app_name)
            .application_version(vk::make_api_version(0, 1, 0, 0))
            .engine_name(&engine_name)
            .engine_version(vk::make_api_version(0, 1, 0, 0))
            .api_version(vk::API_VERSION_1_0);
        let create_info = vk::InstanceCreateInfo::default().application_info(&app_info);
        let instance = unsafe { entry.create_instance(&create_info, None) }
            .map_err(|e| format!("vkCreateInstance failed: {e}"))?;
        Ok(Self { instance, entry })
    }
}

impl Drop for VulkanInstance {
    fn drop(&mut self) {
        unsafe { self.instance.destroy_instance(None); }
    }
}
