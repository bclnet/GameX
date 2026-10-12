// PORT-SOURCE: Families/GameX.Arkane/Arkane.cs
// PORT-SHA: 866f5a89aa4107c9
// PORT-STATUS: done
//
// PARTIAL PORT: the CryEngine, Unreal, Source and idTech games use Crytek's,
// Epic's, Valve's and ID's archive binaries and asset factories, which aren't
// ported, so `getArcBinary` fails for them. `ITransformAsset<IUnknownFileModel>`
// isn't wired: Rust's UnknownTransform is a stub, and C#'s `CanTransformAsset`
// always returns false, so ArkaneArchive never transforms in C# either.
//
// ArkaneArchive only sets up a BinaryArchive in its constructor, so it is a
// constructor function returning `Arc<BinaryArchive>` (binary.rs header). C#
// finds it by name through Type.GetType; `register` adds it to gamex's type
// registry under the same name, and an app calls it at startup.

use std::collections::HashMap;
use std::io::{self, Error, ErrorKind};
use std::sync::{Arc, LazyLock, Mutex};
use gamex::binary::{newArchive, ArcBinary, Archive, AssetFactory, BinaryArchive, BinaryState};
use gamex::family::{registerType, ArchiveFactory, FamilyGame};
use gamex::meta::{extension, FileSource, Tag};
use gamex_uncore::formats::binary::Binary_Txt;
use gamex_uncore::uncore::UncoreArchive;
use crate::formats::binary::{Binary_Danae, Binary_Void};
use crate::formats::danae::binary::{Binary_Ftl, Binary_Fts, Binary_Tea};

pub fn register() {
    registerType::<ArchiveFactory>("GameX.Arkane.ArkaneArchive, GameX.Arkane", |parent, state| Ok(ArkaneArchive::new(parent, state)? as Arc<dyn Archive>));
}

/// ArkaneArchive
pub struct ArkaneArchive;

impl ArkaneArchive {
    pub fn new(parent: Option<Arc<dyn Archive>>, state: BinaryState) -> io::Result<Arc<BinaryArchive>> {
        let arcBinary = Self::getArcBinary(&state.game, &extension(&state.path).to_lowercase())?;
        let engine = state.game.engine.0.clone();
        let mut s = BinaryArchive::new(parent, state, Some(arcBinary));
        s.base.assetFactoryFunc = Some(match engine.as_deref() {
            // "CryEngine" => CrytekArchive::assetFactory, "Unreal" => EpicArchive::assetFactory,
            // "Source" => ValveArchive::assetFactory, "idTech" => IDArchive::assetFactory: not ported, and getArcBinary has failed for them
            _ => Self::assetFactory,
        });
        s.useFileId = true;
        Ok(newArchive(s))
    }

    // #region Factories

    fn getArcBinary(game: &FamilyGame, extension: &str) -> io::Result<Arc<dyn ArcBinary>> {
        static ArcBinarys: LazyLock<Mutex<HashMap<String, Arc<dyn ArcBinary>>>> = LazyLock::new(Default::default);
        let mut arcBinarys = ArcBinarys.lock().unwrap();
        if let Some(z) = arcBinarys.get(&game.id) { return Ok(z.clone()); }
        let z = match game.engine.0.as_deref() {
            Some("Danae") => Binary_Danae::current(),
            Some("Void") => Binary_Void::current(),
            Some(z @ ("CryEngine" | "Unreal" | "Source" | "idTech")) => return Err(Error::new(ErrorKind::Unsupported, format!("{z}'s archive binary isn't ported to Rust yet"))),
            _ => return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException: Engine")),
        };
        arcBinarys.insert(game.id.clone(), z.clone());
        Ok(z)
    }

    pub fn assetFactory(source: &FileSource, game: &FamilyGame) -> (Tag, Option<AssetFactory>) {
        match extension(&source.path).to_lowercase().as_str() {
            ".asl" => (Tag::Int(0), Some(Binary_Txt::factory)),
            // Danae (AF)
            ".ftl" => (Tag::Int(0), Some(Binary_Ftl::factory)),
            ".fts" => (Tag::Int(0), Some(Binary_Fts::factory)),
            ".tea" => (Tag::Int(0), Some(Binary_Tea::factory)),
            //
            //".llf" => (Tag::Int(0), Some(Binary_Flt::factory)),
            //".dlf" => (Tag::Int(0), Some(Binary_Flt::factory)),
            _ => UncoreArchive::assetFactory(source, game),
        }
    }

    // #endregion
}
