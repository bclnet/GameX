// PORT-SOURCE: Engines/GameX.Engine.Sdl/Sdl_Render.cs
// PORT-SHA: 12c33c9cebddcfbf
// PORT-STATUS: partial
//
// Ported: registration of Binary_Ftl and Binary_Fts, and createRenderer "Object".
// Not ported: TestTri, Texture, and the rest of the C# file. Binary_Nif is not in Rust.
// C# registers on `SdlX.BuildersByType`. The OpenX gfx dictionary is not ported,
// so the map lives here. No GPU upload. A missing key does not throw.

use std::any::{Any, TypeId};
use std::collections::HashMap;
use std::sync::{Arc, LazyLock, Mutex, Once};

use gamex_arkane::formats::danae::binary::{Binary_Ftl, Binary_Fts};

use crate::sdl_arkane::sdl_ftl_object_builder;
use crate::sdl_arkane::sdl_fts_object_builder;
use crate::sdl_object_holder::SdlObjectHolder;

pub type ObjectBuilder = fn(&dyn Any, bool, Option<&dyn Any>) -> SdlObjectHolder;

/// C# `SdlX.BuildersByType`, without Binary_Nif.
pub fn BuildersByType() -> &'static Mutex<HashMap<TypeId, ObjectBuilder>> {
    static MAP: LazyLock<Mutex<HashMap<TypeId, ObjectBuilder>>> = LazyLock::new(Default::default);
    &MAP
}

fn register() {
    static ONCE: Once = Once::new();
    ONCE.call_once(|| {
        let mut map = BuildersByType().lock().unwrap();
        map.insert(TypeId::of::<Binary_Ftl>(), sdl_ftl_object_builder::buildObject);
        map.insert(TypeId::of::<Binary_Fts>(), sdl_fts_object_builder::buildObject);
    });
}

/// C# `ObjectRenderer`.
pub struct ObjectRenderer {
    Obj: Arc<dyn Any + Send + Sync>,
    pub Holder: Option<SdlObjectHolder>,
}

impl ObjectRenderer {
    pub fn new(obj: Arc<dyn Any + Send + Sync>) -> Self {
        Self { Obj: obj, Holder: None }
    }

    pub fn start(&mut self) {
        let id = self.Obj.as_ref().type_id();
        let build = BuildersByType().lock().unwrap().get(&id).copied();
        if let Some(build) = build {
            self.Holder = Some(build(self.Obj.as_ref(), true, None));
        }
    }

    pub fn update(&mut self, deltaTime: f32) {
        let _ = deltaTime;
    }
}

pub struct SdlRenderer;

impl SdlRenderer {
    /// C# `SdlRenderer.CreateRenderer`. Only "Object" is ported.
    pub fn createRenderer(type_: &str, obj: Arc<dyn Any + Send + Sync>) -> Option<ObjectRenderer> {
        register();
        match type_ {
            "Object" => Some(ObjectRenderer::new(obj)),
            _ => None,
        }
    }
}
