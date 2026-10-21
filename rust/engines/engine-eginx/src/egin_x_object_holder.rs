// PORT-SOURCE: Engines/GameX.Engine.EginX/EginXObjectHolder.cs
// PORT-SHA: ae48687128bc49c2
// PORT-STATUS: done
//
// C# `EginXObjectHolder`. Name, isStatic, mesh, materials, groups. No GPU upload.
use std::any::Any;

use glam::Vec3;
use openx_gfx::gfx::MaterialStd2Prop;

/// C# `EginXObjectHolder`.
pub struct EginXObjectHolder {
    pub Name: String,
    pub IsStatic: bool,
    pub Mesh: Box<dyn Any + Send + Sync>,
    pub Materials: Vec<MaterialStd2Prop>,
    pub Groups: Vec<(String, Vec3)>,
}
