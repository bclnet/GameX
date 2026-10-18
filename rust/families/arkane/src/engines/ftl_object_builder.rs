// PORT-SOURCE: Families/GameX.Arkane/Engines/FtlObjectBuilder.cs
// PORT-SHA: 1971770a048b3a14
// PORT-STATUS: done
//
// Shared FTL mesh. Danae is Y-up, so positions stay as stored.
// `E_TEXTURE` is a C# class: `toMaterialProp` takes `Option`.
// A null `E_FACE` element is not representable (`Vec<E_FACE>`).

use std::collections::HashMap;
use std::sync::Arc;

use glam::{Vec2, Vec3};
use openx_gfx::gfx::{AnyArc, MaterialStd2Prop};

use crate::formats::danae::eerie_types::{E_3DOBJ, E_TEXTURE, POLY};

/// C# `FtlObjectBuilder.FtlSubmesh`.
#[derive(Debug, Clone)]
pub struct FtlSubmesh {
    pub TexId: i32,
    pub Triangles: Vec<i32>,
}

/// C# `FtlObjectBuilder.FtlMesh`.
#[derive(Debug, Clone, Default)]
pub struct FtlMesh {
    pub Vertices: Vec<Vec3>,
    pub Normals: Vec<Vec3>,
    pub UVs: Vec<Vec2>,
    pub Submeshes: Vec<FtlSubmesh>,
}

/// C# `GetTexturePaths`.
pub fn getTexturePaths(obj: &E_3DOBJ) -> Vec<String> {
    let mut r = Vec::new();
    let Some(textures) = obj.Textures.as_ref() else { return r };
    for t in textures {
        if !t.Path.is_empty() { r.push(t.Path.clone()); }
    }
    r
}

/// C# `ToMaterialProp`. A null texture is `None`.
pub fn toMaterialProp(tex: Option<&E_TEXTURE>) -> MaterialStd2Prop {
    let mut mp = MaterialStd2Prop::default();
    let Some(tex) = tex else { return mp };
    if !tex.Path.is_empty() { mp.std.textures.insert("Main".to_string(), Arc::new(tex.Path.clone()) as AnyArc); }
    if tex.Poly.contains(POLY::TRANS) || tex.Poly.contains(POLY::WATER) { mp.std.alphaBlended = true; mp.alpha = 1.0; }
    mp
}

/// C# `GroupPositions`. An origin outside `Vertexs` panics, as the C# index does.
pub fn groupPositions(obj: &E_3DOBJ) -> Vec<(String, Vec3)> {
    let (Some(groups), Some(vertexs)) = (obj.Groups.as_ref(), obj.Vertexs.as_ref()) else { return Vec::new() };
    groups.iter().map(|g| (g.Name.clone(), vertexs[g.Origin as usize].V)).collect()
}

/// C# `ToGeometry`. Faces whose texture has `HIDE` or `NODRAW` are skipped.
/// Each face emits three vertices. `FaceType == 2` or `DOUBLESIDED` also emits the reversed winding.
pub fn toGeometry(obj: &E_3DOBJ) -> FtlMesh {
    let mut vertices = Vec::new();
    let mut normals = Vec::new();
    let mut uvs = Vec::new();
    let mut order = Vec::new();
    let mut tris: HashMap<i32, Vec<i32>> = HashMap::new();
    if let (Some(faces), Some(vertexs)) = (obj.Faces.as_ref(), obj.Vertexs.as_ref()) {
        for face in faces {
            let tex = obj.Textures.as_ref().and_then(|ts| {
                if face.TexId >= 0 && (face.TexId as usize) < ts.len() { Some(&ts[face.TexId as usize]) } else { None }
            });
            if tex.is_some_and(|t| t.Poly.contains(POLY::HIDE) || t.Poly.contains(POLY::NODRAW)) { continue; }
            let texId = face.TexId as i32;
            if !tris.contains_key(&texId) { order.push(texId); }
            let list = tris.entry(texId).or_default();
            let ids = [face.Vid.x as usize, face.Vid.y as usize, face.Vid.z as usize];
            let us = [face.U.x, face.U.y, face.U.z];
            let vs = [face.V.x, face.V.y, face.V.z];
            let baseI = vertices.len() as i32;
            for c in 0..3 {
                let src = vertexs[ids[c]];
                vertices.push(src.V);
                let n = if face.Nrmls.as_ref().is_some_and(|n| c < n.len()) { face.Nrmls.as_ref().unwrap()[c] }
                    else if face.Norm != Vec3::ZERO { face.Norm }
                    else { src.Norm };
                normals.push(n);
                uvs.push(Vec2::new(us[c], vs[c]));
            }
            list.extend_from_slice(&[baseI, baseI + 1, baseI + 2]);
            if face.FaceType == 2 || tex.is_some_and(|t| t.Poly.contains(POLY::DOUBLESIDED)) { list.extend_from_slice(&[baseI, baseI + 2, baseI + 1]); }
        }
    }
    let sub = order.into_iter().map(|texId| FtlSubmesh { TexId: texId, Triangles: tris.remove(&texId).unwrap_or_default() }).collect();
    FtlMesh { Vertices: vertices, Normals: normals, UVs: uvs, Submeshes: sub }
}

#[cfg(test)]
mod tests {
    use super::*;
    use glam::U16Vec3;
    use crate::formats::danae::eerie_types::{E_FACE, E_GROUPLIST, E_VERTEX};

    fn vert(x: f32, y: f32, z: f32) -> E_VERTEX {
        E_VERTEX { V: Vec3::new(x, y, z), Norm: Vec3::Y, ..Default::default() }
    }

    fn sample() -> E_3DOBJ {
        E_3DOBJ {
            Vertexs: Some(vec![vert(0.0, 0.0, 0.0), vert(1.0, 0.0, 0.0), vert(0.0, 1.0, 0.0)]),
            Textures: Some(vec![
                E_TEXTURE { Path: "a.tex".into(), Poly: POLY::TRANS, ..Default::default() },
                E_TEXTURE { Path: "hidden.tex".into(), Poly: POLY::NODRAW, ..Default::default() },
            ]),
            Faces: Some(vec![
                E_FACE { FaceType: 0, TexId: 0, Vid: U16Vec3::new(0, 1, 2), U: Vec3::new(0.0, 1.0, 0.5), V: Vec3::new(0.0, 0.0, 1.0), ..Default::default() },
                E_FACE { FaceType: 0, TexId: 1, Vid: U16Vec3::new(0, 1, 2), ..Default::default() },
                E_FACE { FaceType: 2, TexId: 0, Vid: U16Vec3::new(0, 1, 2), ..Default::default() },
            ]),
            Groups: Some(vec![E_GROUPLIST { Name: "head".into(), Origin: 1, ..Default::default() }]),
            ..Default::default()
        }
    }

    #[test]
    fn oneTexturedTriangleSkipsNodrawAndDoublesAFaceType2() {
        let obj = sample();
        let mesh = toGeometry(&obj);
        assert_eq!(mesh.Vertices.len(), 6);
        assert_eq!(mesh.Submeshes.len(), 1);
        assert_eq!(mesh.Submeshes[0].TexId, 0);
        assert_eq!(mesh.Submeshes[0].Triangles, vec![0, 1, 2, 3, 4, 5, 3, 5, 4]);
        assert_eq!(mesh.UVs[1], Vec2::new(1.0, 0.0));
        assert_eq!(mesh.Normals[0], Vec3::Y);
        assert_eq!(getTexturePaths(&obj), vec!["a.tex".to_string(), "hidden.tex".to_string()]);
        let mp = toMaterialProp(obj.Textures.as_ref().unwrap().first());
        assert!(mp.std.alphaBlended);
        assert_eq!(mp.alpha, 1.0);
        assert_eq!(mp.std.textures.get("Main").unwrap().downcast_ref::<String>().map(String::as_str), Some("a.tex"));
        assert_eq!(groupPositions(&obj), vec![("head".into(), Vec3::new(1.0, 0.0, 0.0))]);
    }
}
