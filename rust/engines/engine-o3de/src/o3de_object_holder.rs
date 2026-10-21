// PORT-SOURCE: Engines/GameX.Engine.O3de/O3deObjectHolder.cs
// PORT-SHA: c2dbb9bbf3185d8e
// PORT-STATUS: done
//
// C# `O3deObjectHolder`. Name, isStatic, mesh, materials, groups. No GPU upload.
use std::any::Any;

use glam::Vec3;
use openx_gfx::gfx::MaterialStd2Prop;

/// C# `O3deObjectHolder`.
pub struct O3deObjectHolder {
    pub Name: String,
    pub IsStatic: bool,
    pub Mesh: Box<dyn Any + Send + Sync>,
    pub Materials: Vec<MaterialStd2Prop>,
    pub Groups: Vec<(String, Vec3)>,
}
