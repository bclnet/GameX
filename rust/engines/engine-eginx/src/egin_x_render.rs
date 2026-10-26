// PORT-SOURCE: Engines/GameX.Engine.EginX/EginX_Render.cs
// PORT-SHA: 39c0ba6b081487e1
// PORT-STATUS: partial
//
// Ported: registration of Binary_Ftl and Binary_Fts, and createRenderer "Object".
// Not ported: TestTri, Texture, and the rest of the C# file. Binary_Nif is not in Rust.
// C# registers on `EginX.BuildersByType`. The OpenX gfx dictionary is not ported,
// so the map lives here. No GPU upload. A missing key does not throw.

use std::any::{Any, TypeId};
use std::collections::HashMap;
use std::sync::{Arc, LazyLock, Mutex, Once};

use gamex_arkane::formats::danae::binary::{Binary_Ftl, Binary_Fts};

use crate::egin_x_arkane::egin_x_ftl_object_builder;
use crate::egin_x_arkane::egin_x_fts_object_builder;
use crate::egin_x_object_holder::EginXObjectHolder;

pub type ObjectBuilder = fn(&dyn Any, bool, Option<&dyn Any>) -> EginXObjectHolder;

/// C# `EginX.BuildersByType`, without Binary_Nif.
pub fn BuildersByType() -> &'static Mutex<HashMap<TypeId, ObjectBuilder>> {
    static MAP: LazyLock<Mutex<HashMap<TypeId, ObjectBuilder>>> = LazyLock::new(Default::default);
    &MAP
}

fn register() {
    static ONCE: Once = Once::new();
    ONCE.call_once(|| {
        let mut map = BuildersByType().lock().unwrap();
        map.insert(TypeId::of::<Binary_Ftl>(), egin_x_ftl_object_builder::buildObject);
        map.insert(TypeId::of::<Binary_Fts>(), egin_x_fts_object_builder::buildObject);
    });
}

/// C# `ObjectRenderer`.
pub struct ObjectRenderer {
    Obj: Arc<dyn Any + Send + Sync>,
    pub Holder: Option<EginXObjectHolder>,
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

pub struct EginXRenderer;

impl EginXRenderer {
    /// C# `EginXRenderer.CreateRenderer`. Only "Object" is ported.
    pub fn createRenderer(type_: &str, obj: Arc<dyn Any + Send + Sync>) -> Option<ObjectRenderer> {
        register();
        match type_ {
            "Object" => Some(ObjectRenderer::new(obj)),
            _ => None,
        }
    }
}
