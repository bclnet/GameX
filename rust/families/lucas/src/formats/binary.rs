// PORT-SOURCE: Families/GameX.Lucas/Formats/Binary.cs
// PORT-STATUS: partial
//
// Binary_Nwx cells and ToSheet only. The WAXF reader is not ported.
// SCUMM costumes and XGA do not produce pixel buffers in C#, so they have no sheet.

use std::any::Any;
use openx_gfx::gfx::{GfxSpriteImage, GfxSpriteSheet, ITexture};

/// C# `Binary_Nwx`. `cells` are `(width, height, flip, data)`. `data` is column-major indexes.
pub struct Binary_Nwx {
    palette: Option<Vec<(u8, u8, u8)>>,
    cells: Vec<(i32, i32, bool, Vec<u8>)>,
}

impl Binary_Nwx {
    pub fn new(cells: Vec<(i32, i32, bool, Vec<u8>)>, palette: Option<Vec<(u8, u8, u8)>>) -> Self {
        Self { palette, cells }
    }

    /// Column-major indexes. `flip` writes top-down; otherwise the same walk is stored backwards. Index 0 is transparent.
    fn rgba(&self, width: i32, height: i32, flip: bool, data: &[u8]) -> Vec<u8> {
        let mut bytes = vec![0u8; (width as usize) * (height as usize) * 4];
        let mut i: isize = if flip { 0 } else { bytes.len() as isize - 4 };
        let step: isize = if flip { 4 } else { -4 };
        for row in 0..height {
            for col in 0..width {
                let pixel = data[(col * height + row) as usize];
                let at = i as usize;
                if let Some(pal) = &self.palette {
                    let (r, g, b) = pal[pixel as usize];
                    bytes[at] = r;
                    bytes[at + 1] = g;
                    bytes[at + 2] = b;
                }
                else {
                    bytes[at] = pixel;
                    bytes[at + 1] = pixel;
                    bytes[at + 2] = pixel;
                }
                bytes[at + 3] = if pixel == 0 { 0 } else { 255 };
                i += step;
            }
        }
        bytes
    }

    /// One frame per cell, duration 0.1s.
    pub fn toSheet(&self) -> GfxSpriteSheet {
        let cells: Vec<(Vec<u8>, i32, i32, f32)> = self.cells.iter()
            .map(|(width, height, flip, data)| (self.rgba(*width, *height, *flip, data), *width, *height, 0.1))
            .collect();
        GfxSpriteSheet::pack(&cells, &[])
    }
}

#[cfg(test)]
mod tests {
    use super::Binary_Nwx;
    use openx_gfx::gfx::GfxSpriteImage;
    use std::any::Any;

    #[test]
    fn nwxToSheetTwoCells() {
        let pal = Some(vec![(0, 0, 0), (10, 20, 30), (40, 50, 60), (70, 80, 90)]);
        let nwx = Binary_Nwx::new(vec![
            (2, 1, true, vec![1, 2]),
            (1, 2, false, vec![3, 0]),
        ], pal);
        let sheet = nwx.toSheet();
        assert_eq!(sheet.frames.len(), 2);
        assert_eq!(sheet.frames[0].duration, 0.1f32);
        assert_eq!(sheet.frames[1].duration, 0.1f32);
        assert_eq!((sheet.frames[0].x, sheet.frames[0].width, sheet.frames[0].height), (0, 2, 1));
        assert_eq!((sheet.frames[1].x, sheet.frames[1].width, sheet.frames[1].height), (2, 1, 2));
        assert_eq!(sheet.anims.len(), 1);
        assert_eq!(sheet.anims[0].name, "all");
        assert_eq!(sheet.anims[0].frames, vec![0, 1]);
        let mut image = None;
        sheet.texture.create("", &mut |any| {
            image = any.downcast_ref::<GfxSpriteImage>().cloned();
        }).unwrap();
        let image = image.expect("image");
        assert_eq!((image.width, image.height), (3, 2));
        assert_eq!(image.rgba, vec![
            10, 20, 30, 255, 40, 50, 60, 255, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 70, 80, 90, 255,
        ]);
        let gray = Binary_Nwx::new(vec![(1, 1, true, vec![7])], None).toSheet();
        let mut g = None;
        gray.texture.create("", &mut |any| {
            g = any.downcast_ref::<GfxSpriteImage>().cloned();
        }).unwrap();
        let g = g.expect("gray");
        assert_eq!(g.rgba, vec![7, 7, 7, 255]);
    }
}
