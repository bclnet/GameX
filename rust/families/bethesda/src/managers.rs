// PORT-SOURCE: Families/GameX.Bethesda/Managers.cs
// PORT-SHA: 4257260d9c402ed4
// PORT-STATUS: partial (TesCells only; BethesdaCellBuilder.CreateLand is not ported)
//
// Morrowind LAND rules. Twin of GameX.Bethesda.TesCells.
// C# CreateLand lives on BethesdaCellBuilder. Rust CellBuilder does not build terrain.
// `decodeHeights` writes a row-major `Side * Side` slice. C# takes `float[,]`.

use openx::manager::{Land, Query};

pub struct TesCells;

impl TesCells {
    pub const Side: usize = 65;
    pub const HeightScale: i32 = 8;
    pub const TextureRows: usize = 16;
    pub const TextureColumns: usize = 16;
    pub const SampleDivisor: usize = Self::Side - 1;
    pub const DefaultTexturePath: &'static str = "textures/_land_default.dds";

    pub fn textureIndexs(land: &dyn Land) -> Vec<u32> {
        // C# `land.Vtex ?? new uint[256]`. The trait's slice cannot be null, so empty stands in for that.
        let vtex = land.vtex();
        if vtex.is_empty() { vec![0; 256] } else { vtex.to_vec() }
    }

    pub fn decodeHeights(land: &dyn Land, dest: &mut [f32]) {
        let heights = land.heights().unwrap();
        let side = Self::Side;
        let mut rowOffset = land.heightOffset();
        for y in 0..side {
            rowOffset += heights[y * side] as f32;
            dest[y * side] = rowOffset * Self::HeightScale as f32;
            let mut colOffset = rowOffset;
            for x in 1..side {
                colOffset += heights[(y * side) + x] as f32;
                dest[(y * side) + x] = colOffset * Self::HeightScale as f32;
            }
        }
    }

    pub fn layerKey(index: u32) -> i32 { index as i32 - 1 }

    pub fn alphaKey(indexs: &[u32], x: i32, y: i32) -> i32 {
        let yMajor = y / 4;
        let yMinor = y - (yMajor * 4);
        let xMajor = x / 4;
        let xMinor = x - (xMajor * 4);
        indexs[((yMajor * 64) + (xMajor * 16) + (yMinor * 4) + xMinor) as usize] as i32 - 1
    }

    pub fn texturePath(query: &dyn Query, key: i32) -> String {
        if key >= 0 {
            query.findLtex(key).unwrap().path().to_string()
        } else {
            Self::DefaultTexturePath.to_string()
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use glam::Vec3;
    use openx::manager::{Cell, CellXref, Ltex};
    use openx_poly::poly::Int3;

    struct Flat { heights: Vec<i8>, offset: f32, vtex: Vec<u32> }
    impl Land for Flat {
        fn gridId(&self) -> Int3 { Int3::ZERO }
        fn vtex(&self) -> &[u32] { &self.vtex }
        fn heightOffset(&self) -> f32 { self.offset }
        fn heights(&self) -> Option<&[i8]> { Some(&self.heights) }
    }

    struct Tex(String);
    impl Ltex for Tex {
        fn intv(&self) -> i64 { 0 }
        fn path(&self) -> &str { &self.0 }
    }

    struct OneLtex;
    impl Cell for OneLtex {
        fn id(&self) -> u32 { 0 }
        fn isInterior(&self) -> bool { false }
        fn gridId(&self) -> Int3 { Int3::ZERO }
        fn name(&self) -> &str { "" }
        fn ambientLight(&self) -> Option<u32> { None }
        fn xrefs(&self) -> &[Box<dyn CellXref>] { &[] }
    }
    impl Query for OneLtex {
        fn meterInUnits(&self) -> f32 { 1. }
        fn cellLengthInMeters(&self) -> f32 { 1. }
        fn loadRadius(&self) -> i32 { 0 }
        fn visibleRadius(&self) -> i32 { 0 }
        fn world(&self) -> i32 { 0 }
        fn setWorld(&mut self, _world: i32) {}
        fn getCellId(&self, _point: Vec3) -> Int3 { Int3::ZERO }
        fn findCell(&self, _cell: Int3) -> Option<Box<dyn Cell>> { None }
        fn findCellByName(&self, _name: &str) -> Option<Box<dyn Cell>> { None }
        fn findLand(&self, _cell: Int3) -> Option<Box<dyn Land>> { None }
        fn findLtex(&self, index: i32) -> Option<Box<dyn Ltex>> {
            if index == 3 { Some(Box::new(Tex("textures/a.dds".to_string()))) } else { None }
        }
    }

    #[test]
    fn cumulativeHeightsScaleByEight() {
        let land = Flat { heights: vec![1; TesCells::Side * TesCells::Side], offset: 0., vtex: vec![] };
        let mut dest = vec![0f32; TesCells::Side * TesCells::Side];
        TesCells::decodeHeights(&land, &mut dest);
        assert_eq!(dest[0], 8.);
        assert_eq!(dest[1], 16.);
        assert_eq!(dest[TesCells::Side], 16.);
    }

    #[test]
    fn layerAndAlphaSubtractOne() {
        assert_eq!(TesCells::layerKey(5), 4);
        assert_eq!(TesCells::layerKey(0), -1);
        let mut indexs = vec![0u32; 256];
        indexs[17] = 5;
        assert_eq!(TesCells::alphaKey(&indexs, 5, 0), 4);
        assert_eq!(TesCells::alphaKey(&indexs, 0, 0), -1);
    }

    #[test]
    fn missingTextureUsesTheDefaultPath() {
        let query = OneLtex;
        assert_eq!(TesCells::texturePath(&query, 3), "textures/a.dds");
        assert_eq!(TesCells::texturePath(&query, -1), TesCells::DefaultTexturePath);
    }
}
