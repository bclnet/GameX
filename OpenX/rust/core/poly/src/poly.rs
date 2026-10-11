// PORT-SOURCE: Core/OpenX.Poly/System.Numerics/Polyfill.cs (the small vector structs)
// PORT-STATUS: partial
//
// C#'s `Byte2`/`Int2`/`Byte3`/`Int3`/`Float2`/`Float3`/`Float4`, filed like
// Python's `openx/core/poly/poly.py`. glam already has these types, so they are
// aliases that keep the C# names (GameX ADR 0002).
//
// One difference: glam's `Display` prints `[1, 2, 3]` where C#'s `ToString`
// prints `1,2,3`.

/// C# `struct Byte2`.
pub type Byte2 = glam::U8Vec2;
/// C# `struct Int2`.
pub type Int2 = glam::IVec2;
/// C# `struct Byte3`.
pub type Byte3 = glam::U8Vec3;
/// C# `struct Int3`.
pub type Int3 = glam::IVec3;
/// C# `struct Float2`.
pub type Float2 = glam::Vec2;
/// C# `struct Float3`.
pub type Float3 = glam::Vec3;
/// C# `struct Float4`.
pub type Float4 = glam::Vec4;
