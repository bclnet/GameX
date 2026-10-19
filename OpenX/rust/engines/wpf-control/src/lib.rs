//! `openx-wpf-control` — 1:1 mapping of its .NET project.
//!
//! In Rust the WPF host is egui (ADR 0005): `open_gl_control` and
//! `ui::gl_wpf_control` host OpenX's OpenGL engine in an egui paint callback.
//! Nothing else here is translated yet; each module states why at the file.

pub mod console_manager;
pub mod godot_control;
pub mod o3de_control;
pub mod ogre_control;
pub mod open_gl_control;
pub mod sdl_control;
pub mod unity_control;
pub mod unreal_control;
pub mod vulkan_control;
pub mod open_tk;
pub mod ui;
