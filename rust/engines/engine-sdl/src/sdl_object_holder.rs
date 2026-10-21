// PORT-SOURCE: Engines/GameX.Engine.Sdl/SdlObjectHolder.cs
// PORT-SHA: db329665b1b9a184
// PORT-STATUS: done
//
// C# `SdlObjectHolder`. Name, isStatic, mesh, materials, groups. No GPU upload.
use std::any::Any;

use glam::Vec3;
use openx_gfx::gfx::MaterialStd2Prop;

/// C# `SdlObjectHolder`.
pub struct SdlObjectHolder {
    pub Name: String,
    pub IsStatic: bool,
    pub Mesh: Box<dyn Any + Send + Sync>,
    pub Materials: Vec<MaterialStd2Prop>,
    pub Groups: Vec<(String, Vec3)>,
}
