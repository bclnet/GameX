//! `openx-gfx-egin` — 1:1 port of .NET project `OpenX.Gfx.Egin`.
//!
//! `egin_render` (AABB + Camera) and `egin_animate` (skeletal animation) are
//! both ported and **verified numerically against the C# test suite's own
//! assertions** — see their test modules. `egin_particle` is ported, including
//! the initializers, operators and `IParticleRenderer`. Nothing in this crate
//! implements `IParticleSystem`.
//!
//! The matrix-convention note at the top of `egin_render` is the important
//! reading for anyone extending this crate: System.Numerics is row-vector /
//! row-major, glam is column-vector / column-major, so every product reverses.

pub mod egin;
pub mod egin_animate;
pub mod egin_vbib;
pub mod egin_particle;
pub mod egin_render;

pub mod prelude {
    pub use crate::egin_animate::{
        Animation, AnimationController, Bone, ChannelAttribute, Frame, FrameBone, FrameCache,
        FrameIndex, Skeleton,
    };
    pub use crate::egin_render::{Aabb, Camera, CameraViewport, EginRenderer, CAMERA_SPEED, FOV};
    pub use crate::egin_vbib::{Attribute, AttributeLayout, OnDiskBufferData, RenderSlotType, Vbib};
}
