// PORT-SOURCE: Families/GameX.Bullfrog/Formats/Binary.cs
// PORT-SHA: unstamped
// PORT-STATUS: partial
//
// The file readers (RNC, Syndicate, Dungeon Keeper) are not ported.
// This is Binary_SyndicateX.ToSheet and the sprite fields it reads.
// C# and Python fill those fields when a tab, frame, anim, or palette file is opened.

use std::any::Any;

use openx_gfx::gfx::{GfxSpriteImage, GfxSpriteSheet, ITexture};

/// C# `Binary_SyndicateX.X_Palette`.
#[derive(Clone, Debug, Default)]
pub struct X_Palette {
    pub r: u8,
    pub g: u8,
    pub b: u8,
}

/// C# `Binary_SyndicateX.Sprite`. Indexed pixels, one byte per pixel.
#[derive(Clone, Debug, Default)]
pub struct Sprite {
    pub width: i32,
    pub height: i32,
    pub stride: i32,
    pub pixels: Vec<u8>,
}

/// C# `Binary_SyndicateX.SpriteFrame`.
#[derive(Clone, Debug, Default)]
pub struct SpriteFrame {
    pub firstElement: i32,
    pub width: i32,
    pub height: i32,
    pub flags: u32,
    pub nextElement: i32,
}

/// C# `class Binary_SyndicateX`. The readers are not in this file.
/// `sprites`, `frames`, `anims`, and `paletteRecords` are what one object holds
/// when those files have been loaded onto it. A tab open sets `sprites` only.
#[derive(Clone, Debug, Default)]
pub struct Binary_SyndicateX {
    pub sprites: Option<Vec<Sprite>>,
    pub frames: Option<Vec<SpriteFrame>>,
    pub anims: Option<Vec<u16>>,
    pub paletteRecords: Option<Vec<X_Palette>>,
}

impl Binary_SyndicateX {
    /// C# `Binary_SyndicateX.ToSheet`. One cell per sprite, duration 0.1.
    /// A palette already on this object is used. Otherwise each index is grayscale with alpha 255.
    /// SpriteFrame and SpriteAnim are separate files, so a tab open leaves them unset and `pack` names the clip `all`.
    pub fn toSheet(&self) -> GfxSpriteSheet {
        let sprites = self.sprites.as_deref().unwrap_or(&[]);
        let mut cells = Vec::with_capacity(sprites.len());
        for s in sprites {
            let rgba = expand(s, self.paletteRecords.as_deref());
            cells.push((rgba, s.width, s.height, 0.1f32));
        }
        let animList = if self.frames.is_some() && self.anims.is_some() {
            self.anims.as_ref().unwrap().iter().enumerate()
                .map(|(i, index)| (i.to_string(), vec![*index as i32]))
                .collect()
        } else {
            Vec::new()
        };
        GfxSpriteSheet::pack(&cells, &animList)
    }
}

fn expand(s: &Sprite, records: Option<&[X_Palette]>) -> Vec<u8> {
    if s.width <= 0 || s.height <= 0 || s.pixels.is_empty() { return Vec::new(); }
    let w = s.width as usize;
    let h = s.height as usize;
    let stride = if s.stride > 0 { s.stride as usize } else { w };
    let mut rgba = vec![0u8; w * h * 4];
    for y in 0..h {
        for x in 0..w {
            let src = y * stride + x;
            let index = if src < s.pixels.len() { s.pixels[src] } else { 0 };
            let o = (y * w + x) * 4;
            if let Some(records) = records {
                if (index as usize) < records.len() {
                    let c = &records[index as usize];
                    rgba[o] = c.r;
                    rgba[o + 1] = c.g;
                    rgba[o + 2] = c.b;
                    rgba[o + 3] = 255;
                    continue;
                }
            }
            rgba[o] = index;
            rgba[o + 1] = index;
            rgba[o + 2] = index;
            rgba[o + 3] = 255;
        }
    }
    rgba
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn twoIndexedSpritesPackOneRow() {
        let sheet = Binary_SyndicateX {
            sprites: Some(vec![
                Sprite { width: 2, height: 2, stride: 2, pixels: vec![1, 2, 3, 4] },
                Sprite { width: 2, height: 2, stride: 2, pixels: vec![5, 6, 7, 8] },
            ]),
            ..Default::default()
        }.toSheet();
        assert_eq!(sheet.frames.len(), 2);
        assert_eq!(sheet.texture.width(), 4);
        assert_eq!(sheet.anims.len(), 1);
        assert_eq!(sheet.anims[0].name, "all");
        assert_eq!(sheet.anims[0].frames, vec![0, 1]);
        assert_eq!(sheet.frames[0].duration, 0.1);
        let mut rgba = Vec::new();
        sheet.texture.create("", &mut |v| {
            rgba = v.downcast_ref::<GfxSpriteImage>().unwrap().rgba.clone();
        }).unwrap();
        assert_eq!(rgba.len(), 2 * 2 * 2 * 4);
        assert_eq!(&rgba[0..4], &[1, 1, 1, 255]);
        assert_eq!(&rgba[8..12], &[5, 5, 5, 255]);
    }

    #[test]
    fn paletteAndAnimIndexes() {
        let sheet = Binary_SyndicateX {
            sprites: Some(vec![Sprite { width: 1, height: 1, stride: 1, pixels: vec![1] }]),
            frames: Some(vec![SpriteFrame { firstElement: 0, width: 1, height: 1, flags: 0, nextElement: 0 }]),
            anims: Some(vec![0]),
            paletteRecords: Some(vec![
                X_Palette { r: 0, g: 0, b: 0 },
                X_Palette { r: 10, g: 20, b: 30 },
            ]),
        }.toSheet();
        assert_eq!(sheet.anims.len(), 1);
        assert_eq!(sheet.anims[0].name, "0");
        assert_eq!(sheet.anims[0].frames, vec![0]);
        let mut rgba = Vec::new();
        sheet.texture.create("", &mut |v| {
            rgba = v.downcast_ref::<GfxSpriteImage>().unwrap().rgba.clone();
        }).unwrap();
        assert_eq!(rgba, vec![10, 20, 30, 255]);
    }
}
