// PORT-SOURCE: Families/GameX.Arkane/Engines/FtsObjectBuilder.cs
// PORT-SHA: ea56cc8f96725907
// PORT-STATUS: done
//
// Shared FTS mesh. Danae is Y-up, so positions stay as stored.
// A quad is a triangle strip: (0, 1, 2) then (1, 3, 2).
// `E_TEXTURE` is a C# class: `toMaterialProp` takes `Option`.
// A null `E_POLY` element is not representable (`Vec<E_POLY>`).
// Texture identity is `E_TEXTURE.Id` (the decoder clones the texture in Rust).

use std::collections::HashMap;

use glam::{Vec2, Vec3};
use openx_gfx::gfx::{AnyArc, MaterialStd2Prop};
use std::sync::Arc;

use crate::formats::danae::binary::F_LEVEL;
use crate::formats::danae::eerie_types::{E_TEXTURE, POLY};

/// C# `FtsObjectBuilder.FtsSubmesh`.
#[derive(Debug, Clone)]
pub struct FtsSubmesh {
    pub TexId: i32,
    pub Type: POLY,
    pub Triangles: Vec<i32>,
}

/// C# `FtsObjectBuilder.FtsMesh`.
#[derive(Debug, Clone, Default)]
pub struct FtsMesh {
    pub Vertices: Vec<Vec3>,
    pub Normals: Vec<Vec3>,
    pub UVs: Vec<Vec2>,
    pub Submeshes: Vec<FtsSubmesh>,
}

/// C# `GetTexturePaths`.
pub fn getTexturePaths(level: &F_LEVEL) -> Vec<String> {
    let mut r = Vec::new();
    let Some(textures) = level.Textures.as_ref() else { return r };
    for t in textures {
        if !t.Path.is_empty() { r.push(t.Path.clone()); }
    }
    r
}

/// C# `ToMaterialProp`. A null texture is `None`. `type_` is the poly flags (TRANS / WATER).
pub fn toMaterialProp(tex: Option<&E_TEXTURE>, type_: POLY) -> MaterialStd2Prop {
    let mut mp = MaterialStd2Prop::default();
    let mut flags = type_;
    if let Some(tex) = tex {
        if !tex.Path.is_empty() { mp.std.textures.insert("Main".to_string(), Arc::new(tex.Path.clone()) as AnyArc); }
        flags |= tex.Poly;
    }
    if flags.contains(POLY::TRANS) || flags.contains(POLY::WATER) { mp.std.alphaBlended = true; mp.alpha = 1.0; }
    mp
}

/// C# `ScenePositions`. Copied as stored.
pub fn scenePositions(level: &F_LEVEL) -> Vec<(String, Vec3)> {
    vec![("player".into(), level.PlayerPos), ("mscene".into(), level.MscenePos)]
}

fn texIndex(tex: Option<&E_TEXTURE>, level: &F_LEVEL) -> i32 {
    let Some(tex) = tex else { return -1 };
    if tex.Id == 0 { return -1; }
    let Some(textures) = level.Textures.as_ref() else { return -1 };
    textures.iter().position(|t| t.Id == tex.Id).map(|i| i as i32).unwrap_or(-1)
}

/// C# `ToGeometry`. Polys whose type or texture has `HIDE` or `NODRAW` are skipped.
/// A triangle emits three vertices. A quad emits four and the strip's second triangle.
/// `DOUBLESIDED` also emits the reversed winding.
pub fn toGeometry(level: &F_LEVEL) -> FtsMesh {
    let mut vertices = Vec::new();
    let mut normals = Vec::new();
    let mut uvs = Vec::new();
    let mut order = Vec::new();
    let mut tris: HashMap<i32, Vec<i32>> = HashMap::new();
    let mut types: HashMap<i32, POLY> = HashMap::new();
    if let Some(cells) = level.Backg.as_ref() {
        for cell in cells {
            let Some(polys) = cell.Polydata.as_ref() else { continue };
            for poly in polys {
                let tex = poly.Tex.as_ref();
                if poly.Type.contains(POLY::HIDE) || poly.Type.contains(POLY::NODRAW) { continue; }
                if tex.is_some_and(|t| t.Poly.contains(POLY::HIDE) || t.Poly.contains(POLY::NODRAW)) { continue; }
                let quad = poly.Type.contains(POLY::QUAD);
                let count = if quad { 4 } else { 3 };
                let Some(verts) = poly.V.as_ref() else { continue };
                if verts.len() < count { continue; }
                let texId = texIndex(tex, level);
                if !tris.contains_key(&texId) { order.push(texId); types.insert(texId, POLY::empty()); }
                if let Some(slot) = types.get_mut(&texId) { *slot |= poly.Type & (POLY::TRANS | POLY::WATER); }
                let list = tris.entry(texId).or_default();
                let baseI = vertices.len() as i32;
                for c in 0..count {
                    let src = verts[c];
                    vertices.push(src.S);
                    let n = if poly.Nrml.as_ref().is_some_and(|n| c < n.len()) { poly.Nrml.as_ref().unwrap()[c] }
                        else if poly.Norm != Vec3::ZERO { poly.Norm }
                        else { Vec3::ZERO };
                    normals.push(n);
                    uvs.push(src.T);
                }
                list.extend_from_slice(&[baseI, baseI + 1, baseI + 2]);
                if quad { list.extend_from_slice(&[baseI + 1, baseI + 3, baseI + 2]); }
                if poly.Type.contains(POLY::DOUBLESIDED) {
                    list.extend_from_slice(&[baseI, baseI + 2, baseI + 1]);
                    if quad { list.extend_from_slice(&[baseI + 1, baseI + 2, baseI + 3]); }
                }
            }
        }
    }
    let sub = order.into_iter().map(|texId| FtsSubmesh {
        TexId: texId,
        Type: types.remove(&texId).unwrap_or_default(),
        Triangles: tris.remove(&texId).unwrap_or_default(),
    }).collect();
    FtsMesh { Vertices: vertices, Normals: normals, UVs: uvs, Submeshes: sub }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::formats::danae::binary::E_BKG_INFO;
    use crate::formats::danae::eerie_types::{E_POLY, TLVERTEX};

    fn tv(x: f32, y: f32, z: f32, u: f32, v: f32) -> TLVERTEX {
        TLVERTEX { S: Vec3::new(x, y, z), T: Vec2::new(u, v), ..Default::default() }
    }

    fn sample() -> F_LEVEL {
        let floor = E_TEXTURE { Id: 5, Path: "floor.bmp".into(), Poly: POLY::empty() };
        let wall = E_TEXTURE { Id: 7, Path: "wall.bmp".into(), Poly: POLY::empty() };
        let hidden = E_TEXTURE { Id: 9, Path: "hide.bmp".into(), Poly: POLY::HIDE };
        let n = Some(vec![Vec3::Y, Vec3::Y, Vec3::Y, Vec3::Y]);
        F_LEVEL {
            PlayerPos: Vec3::new(1.0, 2.0, 3.0),
            MscenePos: Vec3::new(4.0, 5.0, 6.0),
            Textures: Some(vec![floor.clone(), wall.clone(), hidden.clone()]),
            Backg: Some(vec![
                E_BKG_INFO::default(),
                E_BKG_INFO {
                    Polydata: Some(vec![
                        E_POLY {
                            Type: POLY::TRANS,
                            V: Some(vec![tv(0.0, 2.0, 0.0, 0.0, 0.0), tv(1.0, 2.0, 0.0, 1.0, 0.0), tv(0.0, 2.0, 1.0, 0.0, 1.0), tv(0.0, 0.0, 0.0, 0.0, 0.0)]),
                            Nrml: n.clone(),
                            Tex: Some(floor),
                            ..Default::default()
                        },
                        E_POLY { Type: POLY::NODRAW, V: Some(vec![tv(0.0, 0.0, 0.0, 0.0, 0.0), tv(1.0, 0.0, 0.0, 0.0, 0.0), tv(0.0, 1.0, 0.0, 0.0, 0.0)]), Tex: Some(wall.clone()), ..Default::default() },
                        E_POLY { Type: POLY::empty(), V: Some(vec![tv(0.0, 0.0, 0.0, 0.0, 0.0), tv(1.0, 0.0, 0.0, 0.0, 0.0), tv(0.0, 1.0, 0.0, 0.0, 0.0)]), Tex: Some(hidden), ..Default::default() },
                        E_POLY {
                            Type: POLY::QUAD | POLY::DOUBLESIDED,
                            V: Some(vec![tv(0.0, 0.0, 0.0, 0.0, 0.0), tv(1.0, 0.0, 0.0, 1.0, 0.0), tv(1.0, 0.0, 1.0, 1.0, 1.0), tv(0.0, 0.0, 1.0, 0.0, 1.0)]),
                            Nrml: n,
                            Tex: Some(wall),
                            ..Default::default()
                        },
                    ]),
                    ..Default::default()
                },
            ]),
            ..Default::default()
        }
    }

    #[test]
    fn mapKeepsYAndSkipsHiddenPolys() {
        let level = sample();
        let mesh = toGeometry(&level);
        assert_eq!(mesh.Vertices.len(), 7);
        assert_eq!(mesh.Vertices[0], Vec3::new(0.0, 2.0, 0.0));
        assert_eq!(mesh.Normals[0], Vec3::Y);
        assert_eq!(mesh.UVs[1], Vec2::new(1.0, 0.0));
        assert_eq!(mesh.Submeshes.len(), 2);
        assert_eq!(mesh.Submeshes[0].TexId, 0);
        assert_eq!(mesh.Submeshes[0].Type, POLY::TRANS);
        assert_eq!(mesh.Submeshes[0].Triangles, vec![0, 1, 2]);
        assert_eq!(mesh.Submeshes[1].TexId, 1);
        assert_eq!(mesh.Submeshes[1].Type, POLY::empty());
        assert_eq!(mesh.Submeshes[1].Triangles, vec![3, 4, 5, 4, 6, 5, 3, 5, 4, 4, 5, 6]);
        assert_eq!(getTexturePaths(&level), vec!["floor.bmp".to_string(), "wall.bmp".to_string(), "hide.bmp".to_string()]);
        let mp = toMaterialProp(level.Textures.as_ref().unwrap().first(), mesh.Submeshes[0].Type);
        assert!(mp.std.alphaBlended);
        assert_eq!(mp.alpha, 1.0);
        assert_eq!(mp.std.textures.get("Main").unwrap().downcast_ref::<String>().map(String::as_str), Some("floor.bmp"));
        let wall = toMaterialProp(level.Textures.as_ref().unwrap().get(1), mesh.Submeshes[1].Type);
        assert!(!wall.std.alphaBlended);
        let water = toMaterialProp(None, POLY::WATER);
        assert!(water.std.alphaBlended);
        assert!(water.std.textures.is_empty());
        assert_eq!(scenePositions(&level), vec![("player".into(), Vec3::new(1.0, 2.0, 3.0)), ("mscene".into(), Vec3::new(4.0, 5.0, 6.0))]);
        let empty = toGeometry(&F_LEVEL::default());
        assert!(empty.Vertices.is_empty());
        assert!(empty.Submeshes.is_empty());
    }
}
