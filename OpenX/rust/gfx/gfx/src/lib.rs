//! `openx-gfx` — 1:1 port of .NET project `OpenX.Gfx`.
//!
//! Module layout mirrors the C# file layout. See PORT_MAP.tsv and PORTING.md.
//!
//! All 6 files ported.

pub mod gfx;
// pub mod gfx_bitmap;
// pub mod gfx_render;
pub mod gfx_texture;
// pub mod texture_helper;
// pub mod texture_sequences;

pub mod prelude {
    pub use crate::gfx::{
        GfX, GfxAlphaMode, GfxAttach, GfxBlendMode, Backend, AnyBox, AnyArc, Color,
        ObjectSpriteBuilder, ObjectSpriteManager,
        IObjectModel, ObjectModelBuilder, ObjectModelManager,
        GfxShader, ShaderBuilder, ShaderManager,
        ISprite, SpriteBuilder, SpriteManager,
        TextureAsDds, TextureAsBytes, ITexture, ITextureSelect, ITextureFrames, TextureBuilder, TextureManager,
        IMaterial, MaterialProp, MaterialPropKind, MaterialStdProp, MaterialStd2Prop, MaterialShaderProp, MaterialShaderVProp, MaterialBuilder, MaterialManager,
        IOpenGfx, IOpenGfxApi, IOpenGfxSprite, IOpenGfxModel, IOpenGfxLight, GfxTerrainLayer, IOpenGfxTerrain,
    };
    pub use crate::gfx_texture::{TextureFlags, TextureFormat, DDS_HEADER, DDS_PIXELFORMAT}; // DdsError
    // pub use crate::gfx_render::{blit_by_palette, Color32, Colorf, Pass, Renderer};
    // pub use crate::gfx_bitmap::{Color, DirectBitmap};
    // pub use crate::texture_helper::{
    //     calculate_png_size, downscale_rgba8_x2, mip_level_size, mipmap_count, mipmap_data_size,
    //     mipmap_true_data_size, BlockFormat,
    // };
    // pub use crate::texture_sequences::{Frame, Image, Sequence, TextureSequences};
}
