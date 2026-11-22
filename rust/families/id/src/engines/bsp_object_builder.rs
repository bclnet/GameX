// PORT-SOURCE: Families/GameX.ID/Engines/BspObjectBuilder.cs
// PORT-STATUS: partial
//
// Shared Quake BSP mesh. Quake is Z-up: a point is (x, z, y) * WorldScale,
// a direction is (x, z, y). The C# file was not on disk, so PORT-SHA is not set.
// Python twin: python/gamex/families/ID/engines/bspObjectBuilder.py.

use std::collections::HashMap;
use std::sync::Arc;

use glam::{Vec2, Vec3};
use openx_gfx::gfx::{AnyArc, MaterialStd2Prop};

use crate::formats::binary::{Binary_BspX, BspLevel, BspTexture, Vector4};

/// C# `BspObjectBuilder.BspSubmesh`.
#[derive(Debug, Clone)]
pub struct BspSubmesh {
    pub TexId: i32,
    pub Triangles: Vec<i32>,
}

/// C# `BspObjectBuilder.BspMesh`.
#[derive(Debug, Clone, Default)]
pub struct BspMesh {
    pub Vertices: Vec<Vec3>,
    pub Normals: Vec<Vec3>,
    pub UVs: Vec<Vec2>,
    pub Submeshes: Vec<BspSubmesh>,
}

/// Quake units to the engine. `QuakeCoordSpace` scales points by this and not directions.
pub const WorldScale: f32 = 1.0 / 32.0;

const SKIP: [&str; 5] = ["skip", "hint", "origin", "trigger", "clip"];

/// Non-empty texture names, in lump order.
pub fn getTexturePaths(bsp: &Binary_BspX) -> Vec<String> {
    let Some(level) = bsp.level() else { return Vec::new() };
    level.Textures.iter().filter(|t| !t.Name.is_empty()).map(|t| t.Name.clone()).collect()
}

/// `Textures` key `"Main"`. An empty name stores nothing.
pub fn toMaterialProp(tex: &BspTexture) -> MaterialStd2Prop {
    let mut mp = MaterialStd2Prop::default();
    if !tex.Name.is_empty() { mp.std.textures.insert("Main".to_string(), Arc::new(tex.Name.clone()) as AnyArc); }
    mp
}

fn point(q: Vec3) -> Vec3 { Vec3::new(q.x, q.z, q.y) * WorldScale }
fn direction(q: Vec3) -> Vec3 { Vec3::new(q.x, q.z, q.y) }

fn dot3(q: Vec3, v: Vector4) -> f32 { q.x * v.X + q.y * v.Y + q.z * v.Z }

/// Model 0 when `Models` is non-empty, otherwise every face.
/// A surfedge >= 0 uses `Edges[se].0`; a negative one uses `Edges[-se].1`.
/// Faces whose texture name is skip, hint, origin, trigger, or clip are left out.
/// Winding is not flipped. A zero texture size uses 256.
pub fn toGeometry(bsp: &Binary_BspX) -> BspMesh {
    let Some(level) = bsp.level() else { return BspMesh::default() };
    geometry(level)
}

fn geometry(level: &BspLevel) -> BspMesh {
    let mut vertices = Vec::new();
    let mut normals = Vec::new();
    let mut uvs = Vec::new();
    let mut order = Vec::new();
    let mut tris: HashMap<i32, Vec<i32>> = HashMap::new();
    let faces = if level.Models.is_empty() {
        &level.Faces[..]
    } else {
        let m = &level.Models[0];
        let start = m.FirstFace as usize;
        let end = start + m.NumFaces as usize;
        &level.Faces[start..end]
    };
    for face in faces {
        let info = &level.Texinfo[face.Texinfo as usize];
        let tex = if info.Miptex >= 0 { level.Textures.get(info.Miptex as usize) } else { None };
        if tex.is_some_and(|t| !t.Name.is_empty() && SKIP.contains(&t.Name.to_ascii_lowercase().as_str())) { continue; }
        let mut normal = level.Planes[face.PlaneNum as usize].Normal;
        if face.Side != 0 { normal = -normal; }
        let normal = direction(normal);
        let width = match tex { Some(t) if t.Width != 0 => t.Width, _ => 256 } as f32;
        let height = match tex { Some(t) if t.Height != 0 => t.Height, _ => 256 } as f32;
        let mip = info.Miptex;
        if !tris.contains_key(&mip) { order.push(mip); }
        let base = vertices.len() as i32;
        for i in 0..face.NumEdges {
            let se = level.SurfEdges[(face.FirstEdge + i) as usize];
            let vi = if se >= 0 { level.Edges[se as usize].0 } else { level.Edges[(-se) as usize].1 };
            let q = level.Vertices[vi as usize];
            vertices.push(point(q));
            normals.push(normal);
            let s = dot3(q, info.Vecs0) + info.Vecs0.W;
            let t = dot3(q, info.Vecs1) + info.Vecs1.W;
            uvs.push(Vec2::new(s / width, t / height));
        }
        let list = tris.entry(mip).or_default();
        for k in 1..face.NumEdges - 1 { list.extend_from_slice(&[base, base + k, base + k + 1]); }
    }
    let sub = order.into_iter().map(|texId| BspSubmesh { TexId: texId, Triangles: tris.remove(&texId).unwrap_or_default() }).collect();
    BspMesh { Vertices: vertices, Normals: normals, UVs: uvs, Submeshes: sub }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::formats::binary::{BspFace, BspModel, BspPlane, BspTexinfo};

    fn v4(x: f32, y: f32, z: f32, w: f32) -> Vector4 { Vector4 { X: x, Y: y, Z: z, W: w } }

    #[test]
    fn bspQuad() {
        let level = BspLevel {
            Vertices: vec![Vec3::new(0.0, 0.0, 0.0), Vec3::new(32.0, 0.0, 0.0), Vec3::new(32.0, 32.0, 0.0), Vec3::new(0.0, 32.0, 0.0)],
            Edges: vec![(0, 1), (1, 2), (2, 3), (3, 0)],
            SurfEdges: vec![0, 1, 2, 3],
            Faces: vec![BspFace { PlaneNum: 0, Side: 0, FirstEdge: 0, NumEdges: 4, Texinfo: 0 }],
            Texinfo: vec![BspTexinfo { Vecs0: v4(1.0, 0.0, 0.0, 0.0), Vecs1: v4(0.0, -1.0, 0.0, 0.0), Miptex: 0 }],
            Planes: vec![BspPlane { Normal: Vec3::new(0.0, 0.0, 1.0), Dist: 0.0 }],
            Models: vec![BspModel { FirstFace: 0, NumFaces: 1 }],
            Textures: vec![BspTexture { Name: "wall".into(), Width: 32, Height: 32 }],
            Entities: String::new(),
        };
        let bsp = Binary_BspX::withLevel(level);
        let mesh = toGeometry(&bsp);
        assert_eq!(mesh.Vertices, vec![
            Vec3::new(0.0, 0.0, 0.0),
            Vec3::new(1.0, 0.0, 0.0),
            Vec3::new(1.0, 0.0, 1.0),
            Vec3::new(0.0, 0.0, 1.0),
        ]);
        assert_eq!(mesh.Submeshes[0].Triangles, vec![0, 1, 2, 0, 2, 3]);
    }
}
