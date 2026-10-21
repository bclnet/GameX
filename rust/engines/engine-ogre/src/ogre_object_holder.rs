// PORT-SOURCE: Engines/GameX.Engine.Ogre/OgreObjectHolder.cs
// PORT-SHA: 4a96c9eb01abeea9
// PORT-STATUS: done
//
// C# `OgreObjectHolder`. Name, isStatic, mesh, materials, groups. No GPU upload.
use std::any::Any;

use glam::Vec3;
use openx_gfx::gfx::MaterialStd2Prop;

/// C# `OgreObjectHolder`.
pub struct OgreObjectHolder {
    pub Name: String,
    pub IsStatic: bool,
    pub Mesh: Box<dyn Any + Send + Sync>,
    pub Materials: Vec<MaterialStd2Prop>,
    pub Groups: Vec<(String, Vec3)>,
}
