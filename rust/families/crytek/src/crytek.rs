// PORT-SOURCE: Families/GameX.Crytek/Crytek.cs
// PORT-SHA: abde3611233d078b
// PORT-STATUS: partial
//
// Ported: `assetFactory`. Arkane's Prey (CryEngine) uses the non-Dunia arm.
// `CrytekArchive::new` / `getArcBinary` are not ported: ArcheAge and Dunia
// still need `Binary_ArcheAge` and `Binary_Dunia`. Transforms are not wired.
// Not registered. `.xml` and the model extensions fail when opened; the
// readers are factory slots in `formats/binary.rs`.

use gamex::binary::AssetFactory;
use gamex::family::FamilyGame;
use gamex::meta::{extension, FileSource, Tag};
use gamex_uncore::uncore::UncoreArchive;
use crate::formats::binary::{Binary_CryFile, Binary_CryXml, Binary_Fcb, Binary_Map, Binary_Xbg, Binary_Xbt};

/// CrytekArchive
pub struct CrytekArchive;

impl CrytekArchive {
    // #region Factories

    pub fn assetFactory(source: &FileSource, game: &FamilyGame) -> (Tag, Option<AssetFactory>) {
        match game.engine.0.as_deref() {
            Some("Dunia") => match extension(&source.path).to_lowercase().as_str() {
                ".fc2map" => (Tag::Int(0), Some(Binary_Map::factory)),
                //".aiw" => (0, Binary_AIWorkspace::factory),
                //".lua" => (0, Binary_Lua::factory),
                ".xbt" => (Tag::Int(0), Some(Binary_Xbt::factory)),
                ".xbg" => (Tag::Int(0), Some(Binary_Xbg::factory)),
                ".fcb" => (Tag::Int(0), Some(Binary_Fcb::factory)),
                //".xml" or ".rml" => (0, Binary_Xml::factory),
                _ => UncoreArchive::assetFactory(source, game),
            },
            _ => match extension(&source.path).to_lowercase().as_str() {
                ".xml" => (Tag::Int(0), Some(Binary_CryXml::factory)),
                ".cgf" | ".cga" | ".chr" | ".skin" | ".anim" => (Tag::Int(0), Some(Binary_CryFile::factory)),
                _ => UncoreArchive::assetFactory(source, game),
            },
        }
    }

    // #endregion
}
