// PORT-SOURCE: Engines/GameX.Engine.Mg/MgObjectHolder.cs
// PORT-SHA: 9dc082e80b240618
// PORT-STATUS: done
//
// C# `MgObjectHolder`. Name, isStatic, mesh, materials, groups. No GPU upload.
use std::any::Any;

use glam::Vec3;
use openx_gfx::gfx::MaterialStd2Prop;

/// C# `MgObjectHolder`.
pub struct MgObjectHolder {
    pub Name: String,
    pub IsStatic: bool,
    pub Mesh: Box<dyn Any + Send + Sync>,
    pub Materials: Vec<MaterialStd2Prop>,
    pub Groups: Vec<(String, Vec3)>,
}
