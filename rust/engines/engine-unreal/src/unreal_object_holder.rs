// PORT-SOURCE: Engines/GameX.Engine.Unreal/UnrealObjectHolder.cs
// PORT-SHA: 4ecc76b704334bc2
// PORT-STATUS: done
//
// C# `UnrealObjectHolder`. Name, isStatic, mesh, materials, groups. No GPU upload.
use std::any::Any;

use glam::Vec3;
use openx_gfx::gfx::MaterialStd2Prop;

/// C# `UnrealObjectHolder`.
pub struct UnrealObjectHolder {
    pub Name: String,
    pub IsStatic: bool,
    pub Mesh: Box<dyn Any + Send + Sync>,
    pub Materials: Vec<MaterialStd2Prop>,
    pub Groups: Vec<(String, Vec3)>,
}
