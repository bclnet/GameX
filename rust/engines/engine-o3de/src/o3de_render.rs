// PORT-SOURCE: Engines/GameX.Engine.O3de/O3de_Render.cs
// PORT-SHA: 20afab1a639413b9
// PORT-STATUS: partial
//
// Ported: registration of Binary_Ftl and Binary_Fts, and createRenderer "Object".
// Not ported: TestTri, Texture, and the rest of the C# file. Binary_Nif is not in Rust.
// C# registers on `O3deX.BuildersByType`. The OpenX gfx dictionary is not ported,
// so the map lives here. No GPU upload. A missing key does not throw.

use std::any::{Any, TypeId};
use std::collections::HashMap;
use std::sync::{Arc, LazyLock, Mutex, Once};

use gamex_arkane::formats::danae::binary::{Binary_Ftl, Binary_Fts};

use crate::o3de_ftl_object_builder;
use crate::o3de_fts_object_builder;
use crate::o3de_object_holder::O3deObjectHolder;

pub type ObjectBuilder = fn(&dyn Any, bool, Option<&dyn Any>) -> O3deObjectHolder;

/// C# `O3deX.BuildersByType`, without Binary_Nif.
pub fn BuildersByType() -> &'static Mutex<HashMap<TypeId, ObjectBuilder>> {
    static MAP: LazyLock<Mutex<HashMap<TypeId, ObjectBuilder>>> = LazyLock::new(Default::default);
    &MAP
}

fn register() {
    static ONCE: Once = Once::new();
    ONCE.call_once(|| {
        let mut map = BuildersByType().lock().unwrap();
        map.insert(TypeId::of::<Binary_Ftl>(), o3de_ftl_object_builder::buildObject);
        map.insert(TypeId::of::<Binary_Fts>(), o3de_fts_object_builder::buildObject);
    });
}

/// C# `ObjectRenderer`.
pub struct ObjectRenderer {
    Obj: Arc<dyn Any + Send + Sync>,
    pub Holder: Option<O3deObjectHolder>,
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

pub struct O3deRenderer;

impl O3deRenderer {
    /// C# `O3deRenderer.CreateRenderer`. Only "Object" is ported.
    pub fn createRenderer(type_: &str, obj: Arc<dyn Any + Send + Sync>) -> Option<ObjectRenderer> {
        register();
        match type_ {
            "Object" => Some(ObjectRenderer::new(obj)),
            _ => None,
        }
    }
}
