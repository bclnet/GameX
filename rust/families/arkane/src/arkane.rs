// PORT-SOURCE: Families/GameX.Arkane/Arkane.cs
// PORT-SHA: 866f5a89aa4107c9
// PORT-STATUS: done
//
// PARTIAL PORT: CryEngine, Unreal, Source and idTech now dispatch to Crytek,
// Epic, Valve and ID. `Binary_Pak` (idTech) and `Binary_Vpk` (Source) read.
// `Binary_Cry3` fails in `ZipArchiveX` (`ZipKind.Cry3`). `Binary_Pck` fails in
// `UPackage`. idTech `.lmp` / `.spr` read; `.mdl` is C#'s empty reader; `.bsp`
// is still a slot. Source `.mdl` reads the `Binary_Mdl40` header. Crytek
// `.xml` / models fail when opened.
// `ITransformAsset<IUnknownFileModel>` isn't wired: Rust's UnknownTransform is
// a stub, and C#'s `CanTransformAsset` always returns false, so ArkaneArchive
// never transforms in C# either.
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
use gamex_crytek::crytek::CrytekArchive;
use gamex_crytek::formats::binary::Binary_Cry3;
use gamex_epic::epic::EpicArchive;
use gamex_epic::formats::binary::Binary_Pck;
use gamex_id::formats::binary::Binary_Pak;
use gamex_id::id::IDArchive;
use gamex_uncore::formats::binary::Binary_Txt;
use gamex_uncore::uncore::UncoreArchive;
use gamex_valve::formats::binary::Binary_Vpk;
use gamex_valve::valve::ValveArchive;
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
            Some("CryEngine") => CrytekArchive::assetFactory,
            Some("Unreal") => EpicArchive::assetFactory,
            Some("Source") => ValveArchive::assetFactory,
            Some("idTech") => IDArchive::assetFactory,
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
            Some("CryEngine") => Binary_Cry3::current(),
            Some("Unreal") => Binary_Pck::current(),
            Some("Source") => Binary_Vpk::current(),
            Some("idTech") => Binary_Pak::current(),
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

#[cfg(test)]
mod tests {
    use super::*;
    use gamex::binary::{getResult, Stream};
    use openx_vfx::vfx::{DirectoryFileSystem, VfsRead};
    use std::io::Cursor;
    use std::sync::atomic::Ordering;

    fn game(id: &str, engine: Option<&str>) -> Arc<FamilyGame> {
        Arc::new(FamilyGame { id: id.into(), engine: (engine.map(|s| s.to_string()), Some("07x".into())), ..Default::default() })
    }

    fn open(id: &str, engine: &str) -> (Arc<FamilyGame>, Arc<BinaryArchive>) {
        let game = game(id, Some(engine));
        let state = BinaryState::new(Arc::new(DirectoryFileSystem::new("", None)), game.clone(), None, Some("a.pak"), Tag::None);
        (game.clone(), ArkaneArchive::new(None, state).unwrap())
    }

    fn invoke(factory: AssetFactory) -> io::Result<Tag> {
        let empty = <dyn Archive>::empty();
        let mut stream: Stream = Box::new(Cursor::new(Vec::<u8>::new()));
        getResult(factory(&mut stream, Arc::new(FileSource::default()), &*empty))
    }

    #[test]
    fn theFourEnginesDispatchAndTheUnknownEngineStillThrows() {
        for (id, engine, marker) in [
            ("prey", "CryEngine", "ZipArchiveX"),
            ("dishonored", "Unreal", "UPackage"),
            ("messiah", "Source", "BAD MAGIC"),
            ("wolf", "idTech", "BAD MAGIC"),
        ] {
            let (_game, arc) = open(id, engine);
            let binary = arc.arcBinary.clone().unwrap();
            let mut cursor = Cursor::new(vec![0u8; 16]);
            let err = getResult(binary.read(&arc, Some(&mut cursor as &mut dyn VfsRead), &Tag::None)).unwrap_err();
            assert!(err.to_string().contains(marker), "{engine}: {err}");
            // Source and idTech reach the real readers. CryEngine and Unreal stop on ZipArchiveX and UPackage.
            if engine == "Source" || engine == "idTech" { assert!(!err.to_string().contains("isn't ported"), "{engine}: {err}"); }
            if engine == "CryEngine" { assert!(!arc.useReader.load(Ordering::Relaxed)); }
            if engine == "Unreal" { assert!(arc.files().unwrap().is_empty()); }
        }
        let state = BinaryState::new(Arc::new(DirectoryFileSystem::new("", None)), game("danae", Some("Danae")), None, Some("a.pak"), Tag::None);
        assert!(ArkaneArchive::new(None, state).is_ok());
        for engine in [Some("Nope"), None] {
            let state = BinaryState::new(Arc::new(DirectoryFileSystem::new("", None)), game("bad", engine), None, Some("a.pak"), Tag::None);
            let err = ArkaneArchive::new(None, state).err().unwrap();
            assert!(err.to_string().contains("ArgumentOutOfRangeException"), "{err}");
        }
    }

    #[test]
    fn assetFactoriesFollowTheEngine() {
        let (idGame, idArc) = open("id-fac", "idTech");
        let lmp = FileSource { path: "a.lmp".into(), ..Default::default() };
        let (_tag, fac) = (idArc.base.assetFactoryFunc.unwrap())(&lmp, &idGame);
        let err = invoke(fac.unwrap()).unwrap_err();
        assert!(err.to_string().contains("NotImplementedException"), "{err}");
        let txt = FileSource { path: "a.txt".into(), ..Default::default() };
        assert!((idArc.base.assetFactoryFunc.unwrap())(&txt, &idGame).1.is_some());

        let (srcGame, srcArc) = open("src-fac", "Source");
        let mdl = FileSource { path: "a.mdl".into(), ..Default::default() };
        let (_tag, fac) = (srcArc.base.assetFactoryFunc.unwrap())(&mdl, &srcGame);
        let err = invoke(fac.unwrap()).unwrap_err();
        assert!(err.to_string().contains("InvalidCastException: BinaryArchive"), "{err}");

        let (cryGame, cryArc) = open("cry-fac", "CryEngine");
        let xml = FileSource { path: "a.xml".into(), ..Default::default() };
        let (_tag, fac) = (cryArc.base.assetFactoryFunc.unwrap())(&xml, &cryGame);
        let err = invoke(fac.unwrap()).unwrap_err();
        assert!(err.to_string().contains("Binary_CryXml isn't ported"), "{err}");
        let cgf = FileSource { path: "a.cgf".into(), ..Default::default() };
        let (_tag, fac) = (cryArc.base.assetFactoryFunc.unwrap())(&cgf, &cryGame);
        assert!(invoke(fac.unwrap()).unwrap_err().to_string().contains("Binary_CryFile"));

        let (unGame, unArc) = open("un-fac", "Unreal");
        let unknown = FileSource { path: "a.zzz".into(), ..Default::default() };
        assert!((unArc.base.assetFactoryFunc.unwrap())(&unknown, &unGame).1.is_none());

        let (afGame, afArc) = open("af-fac", "Danae");
        let ftl = FileSource { path: "a.ftl".into(), ..Default::default() };
        assert!((afArc.base.assetFactoryFunc.unwrap())(&ftl, &afGame).1.is_some());

        let plain = game("plain", None);
        let err = std::panic::catch_unwind(std::panic::AssertUnwindSafe(|| IDArchive::assetFactory(&txt, &plain))).unwrap_err();
        let msg = err.downcast_ref::<&str>().copied().unwrap_or("");
        assert!(msg.contains("ArgumentOutOfRangeException"), "{msg}");
        let err = std::panic::catch_unwind(std::panic::AssertUnwindSafe(|| ValveArchive::assetFactory(&txt, &plain))).unwrap_err();
        let msg = err.downcast_ref::<&str>().copied().unwrap_or("");
        assert!(msg.contains("ArgumentOutOfRangeException"), "{msg}");
    }
}
