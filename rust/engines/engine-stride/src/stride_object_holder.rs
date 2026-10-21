// PORT-SOURCE: Engines/GameX.Engine.Stride/StrideObjectHolder.cs
// PORT-SHA: 330ce09537f6af76
// PORT-STATUS: partial
//
// Stride.Engine.Entity is not in Rust. The holder is what C# stores on the entity.
// C# `StrideObjectHolder`. Name, isStatic, mesh, materials, groups. No GPU upload.
use std::any::Any;

use glam::Vec3;
use openx_gfx::gfx::MaterialStd2Prop;

/// C# `StrideObjectHolder`.
pub struct StrideObjectHolder {
    pub Name: String,
    pub IsStatic: bool,
    pub Mesh: Box<dyn Any + Send + Sync>,
    pub Materials: Vec<MaterialStd2Prop>,
    pub Groups: Vec<(String, Vec3)>,
}
