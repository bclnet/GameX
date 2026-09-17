// PORT-SOURCE: Core/GameX/Formats/IUnknown.cs
// PORT-SHA: d536d2fcd7b2d724
// PORT-STATUS: done
//
// The abstract 3D-model interfaces every family's model reader implements:
// `IUnknownFileModel`, `IUnknownMaterial`, `IUnknownSkin`, `IUnknownBone` and
// friends. Pure declarations — 166 lines with no bodies.
//
// ===================== THREE C#-SIDE OBSERVATIONS ========================
//
//   1. **`IUnknownBone` declares two `Matrix4x4` properties that the comments
//      say are 4x3.** `WorldToBone` and `BoneToWorld` are both annotated
//      `// 4x3 matrix`, so the fourth row is padding every implementor has to
//      know to ignore — and nothing enforces that it is identity. `Mat4` is
//      kept here (glam has no 4x3) with the convention documented, because
//      changing the storage would change every family's reader.
//
//   2. **`IUnknownSkin.BoneMap.Weight` is `int[]`, commented `// Byte / 256?`.**
//      The comment is a question mark in shipped code: nothing else says
//      whether these are 0..255 or 0..1 scaled, and the two differ by a factor
//      of 256 in every skinned vertex. Left as `i32` with the ambiguity
//      recorded — **this needs someone who knows the format.**
//
//   3. **`IntVertex` has fields named `Obsolete0` and `Obsolete2`.** Two of its
//      three `Vector3`s are declared obsolete by name but still read and
//      stored, so every skinned vertex carries 24 bytes nothing consumes.
//
// Ported as traits. Rust has no property syntax, so getters become methods;
// `IEnumerable<T>` becomes a slice where the data is owned and an iterator
// where it is computed — noted per method.

use glam::{Mat4, Vec3};

/// C# `IUnknownFileObject.Source`.
#[derive(Debug, Clone, Default, PartialEq, Eq)]
pub struct Source {
    pub author: Option<String>,
    pub source_file: Option<String>,
}

/// C# `IUnknownFileObject`.
pub trait UnknownFileObject {
    fn name(&self) -> &str;
    fn path(&self) -> &str;
    fn sources(&self) -> &[Source];
}

/// C# `IUnknownBone`.
pub trait UnknownBone {
    fn name(&self) -> &str;
    /// C# `WorldToBone` — commented "4x3 matrix"; the fourth row is padding.
    fn world_to_bone(&self) -> Mat4;
    /// C# `BoneToWorld` — likewise 4x3.
    fn bone_to_world(&self) -> Mat4;
}

/// C# `IUnknownTexture`.
pub trait UnknownTexture {
    fn path(&self) -> &str;
}

/// C# `IUnknownMaterial`.
///
/// The three colours are `Vector3?` in the C# — RGB with no alpha, optional.
pub trait UnknownMaterial {
    fn name(&self) -> &str;
    fn diffuse(&self) -> Option<Vec3>;
    fn specular(&self) -> Option<Vec3>;
    fn emissive(&self) -> Option<Vec3>;
    fn shininess(&self) -> f32;
    fn opacity(&self) -> f32;
    fn textures(&self) -> Vec<&dyn UnknownTexture>;
}

/// C# `IUnknownModel`.
pub trait UnknownModel {
    fn path(&self) -> &str;
}

/// C# `IUnknownProxy.Proxy`.
#[derive(Debug, Clone, Default, PartialEq)]
pub struct Proxy {
    pub vertexs: Vec<Vec3>,
    pub indexs: Vec<i32>,
}

impl Proxy {
    /// Whether every index addresses a vertex that exists.
    ///
    /// No C# equivalent — the arrays are read from a file and handed to the
    /// renderer unchecked.
    pub fn indices_in_range(&self) -> bool {
        let n = self.vertexs.len() as i32;
        self.indexs.iter().all(|&i| i >= 0 && i < n)
    }
}

/// C# `IUnknownProxy`.
pub trait UnknownProxy {
    fn physical_proxys(&self) -> &[Proxy];
}

/// C# `IUnknownSkin.BoneMap`.
#[derive(Debug, Clone, Default, PartialEq, Eq)]
pub struct BoneMap {
    pub bone_index: Vec<i32>,
    /// C# comment: `// Byte / 256?` — see observation 2. Scale is unresolved.
    pub weight: Vec<i32>,
}

/// C# `IUnknownSkin.IntVertex`.
#[derive(Debug, Clone, Default, PartialEq)]
pub struct IntVertex {
    /// C# `Obsolete0` — read and stored, consumed by nothing.
    pub obsolete0: Vec3,
    pub position: Vec3,
    /// C# `Obsolete2` — likewise.
    pub obsolete2: Vec3,
    /// C# comment: "4 bone IDs".
    pub bone_ids: [u16; 4],
    /// C# comment: "Should be 4 of these" — a `float[]` with an expectation
    /// rather than a fixed size, so nothing rejects a vertex with three.
    pub weights: [f32; 4],
    pub color: Option<u32>,
}

impl IntVertex {
    /// Whether the four weights sum to 1 within tolerance.
    ///
    /// No C# equivalent, and worth having: a mis-scaled weight array (see
    /// observation 2) shows up here rather than as silently wrong deformation.
    pub fn weights_normalised(&self, tol: f32) -> bool {
        (self.weights.iter().sum::<f32>() - 1.0).abs() <= tol
    }
}

/// C# `IUnknownSkin`.
pub trait UnknownSkin {
    fn has_skinning_info(&self) -> bool;
    fn compiled_bones(&self) -> Vec<&dyn UnknownBone>;
    fn int_vertexs(&self) -> &[IntVertex];
}

/// C# `IUnknownFileModel`.
pub trait UnknownFileModel: UnknownFileObject {
    fn models(&self) -> Vec<&dyn UnknownModel>;
    fn materials(&self) -> Vec<&dyn UnknownMaterial>;
    fn proxies(&self) -> Vec<&dyn UnknownProxy>;
    fn skinning_info(&self) -> Option<&dyn UnknownSkin>;
    fn root_nodes(&self) -> Vec<&str>;
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn proxy_indices_are_range_checked() {
        let p = Proxy { vertexs: vec![Vec3::ZERO; 3], indexs: vec![0, 1, 2] };
        assert!(p.indices_in_range());
        let bad = Proxy { vertexs: vec![Vec3::ZERO; 3], indexs: vec![0, 3] };
        assert!(!bad.indices_in_range(), "3 is past the end of 3 vertices");
        let neg = Proxy { vertexs: vec![Vec3::ZERO], indexs: vec![-1] };
        assert!(!neg.indices_in_range());
    }

    #[test]
    fn an_empty_proxy_is_trivially_in_range() {
        assert!(Proxy::default().indices_in_range());
    }

    #[test]
    fn normalised_weights_are_detectable() {
        let v = IntVertex { weights: [0.5, 0.25, 0.25, 0.0], ..Default::default() };
        assert!(v.weights_normalised(1e-6));
        // The `Byte / 256?` ambiguity looks like this if read at the wrong scale.
        let unscaled = IntVertex { weights: [128.0, 64.0, 64.0, 0.0], ..Default::default() };
        assert!(!unscaled.weights_normalised(1e-6));
        assert!(
            (unscaled.weights.iter().sum::<f32>() / 256.0 - 1.0).abs() < 1e-6,
            "and /256 is what makes it normalise, which is the open question"
        );
    }

    #[test]
    fn bone_ids_and_weights_are_fixed_at_four() {
        // The C# uses ushort[] and float[] with the count only in a comment.
        let v = IntVertex::default();
        assert_eq!(v.bone_ids.len(), 4);
        assert_eq!(v.weights.len(), 4);
    }

    #[test]
    fn obsolete_fields_are_present_as_the_c_sharp_has_them() {
        // Named obsolete, still read, consumed by nothing - 24 bytes per vertex.
        let v = IntVertex::default();
        assert_eq!(v.obsolete0, Vec3::ZERO);
        assert_eq!(v.obsolete2, Vec3::ZERO);
    }
}
