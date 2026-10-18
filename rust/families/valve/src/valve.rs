// PORT-SOURCE: Families/GameX.Valve/Valve.cs
// PORT-SHA: 5804a536771f7fa0
// PORT-STATUS: partial
//
// Ported: `assetFactory`. Arkane's Dark Messiah (Source) uses the Source arm.
// `GetArcBinary`, `FindBinary` and transforms are not ported: Unity and
// GoldSrc still need `Binary_Unity` and `Binary_Wad3`. Not registered.
// GoldSrc `.bsp` / `.spr` are ID's factories. Source `.mdl` reads the
// `Binary_Mdl40` header. Source 2 and the GoldSrc texture/model factories
// fail when opened.

use gamex::binary::AssetFactory;
use gamex::family::FamilyGame;
use gamex::meta::{extension, FileSource, Tag};
use gamex_id::formats::binary::{Binary_BspX, Binary_Spr};
use gamex_uncore::uncore::UncoreArchive;
use crate::formats::binary::{Binary_Mdl10, Binary_Mdl40, Binary_Src, Binary_Wad3X};

/// ValveArchive
pub struct ValveArchive;

impl ValveArchive {
    // #region Factories

    pub fn assetFactory(source: &FileSource, game: &FamilyGame) -> (Tag, Option<AssetFactory>) {
        match game.engine.0.as_deref() {
            Some("GoldSrc") => match extension(&source.path).to_lowercase().as_str() {
                ".pic" | ".tex" | ".tex2" | ".fnt" => (Tag::Int(0), Some(Binary_Wad3X::factory)),
                ".bsp" => (Tag::Int(0), Some(Binary_BspX::factory)),
                ".spr" => (Tag::Int(0), Some(Binary_Spr::factory)),
                ".mdl" => (Tag::Int(0), Some(Binary_Mdl10::factory)),
                _ => UncoreArchive::assetFactory(source, game),
            },
            Some("Source") => match extension(&source.path).to_lowercase().as_str() {
                ".mdl" => (Tag::Int(0), Some(Binary_Mdl40::factory)),
                _ => UncoreArchive::assetFactory(source, game),
            },
            Some("Source2") => (Tag::Int(0), Some(Binary_Src::factory)),
            _ => panic!("ArgumentOutOfRangeException: game.Engine"),
        }
    }

    // #endregion
}
