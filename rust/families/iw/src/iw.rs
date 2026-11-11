// PORT-SOURCE: Families/GameX.IW/IW.cs
// PORT-SHA: 757584843dc8ca46
// PORT-STATUS: partial
//
// PARTIAL PORT: `IWArchive` constructs `Binary_IW`. `assetFactory` has one
// line per fastfile extension. Those factories are stubs. `Binary_Iwi`'s
// real decoder stays in C#. `ITransformAsset` is not ported.
// `register` is not called from the Explorer, so opening IW there still
// fails with "unknown ArchiveType".

use std::io;
use std::sync::Arc;
use gamex::binary::{newArchive, ArcBinary, Archive, AssetFactory, BinaryArchive, BinaryState};
use gamex::family::{registerType, ArchiveFactory, FamilyGame};
use gamex::meta::{extension, FileSource, Tag};
use gamex_uncore::uncore::UncoreArchive;
use crate::formats::binary::{Binary_IW, Binary_Iwi, Binary_Gsc, Binary_Xmp, Binary_Phy, Binary_Xan, Binary_Xmd, Binary_Mat, Binary_Tec, Binary_Snd, Binary_Crv, Binary_Lds, Binary_Csp, Binary_Cmp, Binary_Cmm, Binary_Gsp, Binary_Gmp, Binary_Ent, Binary_Gfx, Binary_Lgt, Binary_Uim, Binary_Fnt, Binary_Mnf, Binary_Mnu, Binary_Loc, Binary_Wep, Binary_Sdg, Binary_Fx, Binary_Ifx, Binary_Ait, Binary_Mpt, Binary_Chr, Binary_Xma, Binary_Raw, Binary_Tbl, Binary_Pcm, Binary_Xms, Binary_Psh, Binary_Vsh, Binary_Vdl, Binary_Fxm, Binary_Lbd, Binary_Sdd, Binary_Trc, Binary_Veh, Binary_Ame, Binary_Gls, Binary_Aip, Binary_Vtr, Binary_Att, Binary_Sfx};

pub fn register() {
    registerType::<ArchiveFactory>("GameX.IW.IWArchive, GameX.IW", |parent, state| Ok(IWArchive::new(parent, state)? as Arc<dyn Archive>));
}

/// IWArchive
pub struct IWArchive;

impl IWArchive {
    pub fn new(parent: Option<Arc<dyn Archive>>, state: BinaryState) -> io::Result<Arc<BinaryArchive>> {
        let arcBinary: Option<Arc<dyn ArcBinary>> = Some(Binary_IW::current());
        let mut s = BinaryArchive::new(parent, state, arcBinary);
        s.base.assetFactoryFunc = Some(Self::assetFactory);
        Ok(newArchive(s))
    }

    pub fn assetFactory(source: &FileSource, game: &FamilyGame) -> (Tag, Option<AssetFactory>) {
        match extension(&source.path).to_lowercase().as_str() {
            // ".roq" => VIDEO::factory,
            // ".wav" => BinaryWav::factory,
            // ".d3dbsp" => BinaryD3dbsp::factory,
            ".iwi" => (Tag::Int(0), Some(Binary_Iwi::factory)),
            ".gsc" => (Tag::Int(0), Some(Binary_Gsc::factory)),
            ".xmp" => (Tag::Int(0), Some(Binary_Xmp::factory)),
            ".phy" => (Tag::Int(0), Some(Binary_Phy::factory)),
            ".xan" => (Tag::Int(0), Some(Binary_Xan::factory)),
            ".xmd" => (Tag::Int(0), Some(Binary_Xmd::factory)),
            ".mat" => (Tag::Int(0), Some(Binary_Mat::factory)),
            ".tec" => (Tag::Int(0), Some(Binary_Tec::factory)),
            ".snd" => (Tag::Int(0), Some(Binary_Snd::factory)),
            ".crv" => (Tag::Int(0), Some(Binary_Crv::factory)),
            ".lds" => (Tag::Int(0), Some(Binary_Lds::factory)),
            ".csp" => (Tag::Int(0), Some(Binary_Csp::factory)),
            ".cmp" => (Tag::Int(0), Some(Binary_Cmp::factory)),
            ".cmm" => (Tag::Int(0), Some(Binary_Cmm::factory)),
            ".gsp" => (Tag::Int(0), Some(Binary_Gsp::factory)),
            ".gmp" => (Tag::Int(0), Some(Binary_Gmp::factory)),
            ".ent" => (Tag::Int(0), Some(Binary_Ent::factory)),
            ".gfx" => (Tag::Int(0), Some(Binary_Gfx::factory)),
            ".lgt" => (Tag::Int(0), Some(Binary_Lgt::factory)),
            ".uim" => (Tag::Int(0), Some(Binary_Uim::factory)),
            ".fnt" => (Tag::Int(0), Some(Binary_Fnt::factory)),
            ".mnf" => (Tag::Int(0), Some(Binary_Mnf::factory)),
            ".mnu" => (Tag::Int(0), Some(Binary_Mnu::factory)),
            ".loc" => (Tag::Int(0), Some(Binary_Loc::factory)),
            ".wep" => (Tag::Int(0), Some(Binary_Wep::factory)),
            ".sdg" => (Tag::Int(0), Some(Binary_Sdg::factory)),
            ".fx" => (Tag::Int(0), Some(Binary_Fx::factory)),
            ".ifx" => (Tag::Int(0), Some(Binary_Ifx::factory)),
            ".ait" => (Tag::Int(0), Some(Binary_Ait::factory)),
            ".mpt" => (Tag::Int(0), Some(Binary_Mpt::factory)),
            ".chr" => (Tag::Int(0), Some(Binary_Chr::factory)),
            ".xma" => (Tag::Int(0), Some(Binary_Xma::factory)),
            ".raw" => (Tag::Int(0), Some(Binary_Raw::factory)),
            ".tbl" => (Tag::Int(0), Some(Binary_Tbl::factory)),
            ".pcm" => (Tag::Int(0), Some(Binary_Pcm::factory)),
            ".xms" => (Tag::Int(0), Some(Binary_Xms::factory)),
            ".psh" => (Tag::Int(0), Some(Binary_Psh::factory)),
            ".vsh" => (Tag::Int(0), Some(Binary_Vsh::factory)),
            ".vdl" => (Tag::Int(0), Some(Binary_Vdl::factory)),
            ".fxm" => (Tag::Int(0), Some(Binary_Fxm::factory)),
            ".lbd" => (Tag::Int(0), Some(Binary_Lbd::factory)),
            ".sdd" => (Tag::Int(0), Some(Binary_Sdd::factory)),
            ".trc" => (Tag::Int(0), Some(Binary_Trc::factory)),
            ".veh" => (Tag::Int(0), Some(Binary_Veh::factory)),
            ".ame" => (Tag::Int(0), Some(Binary_Ame::factory)),
            ".gls" => (Tag::Int(0), Some(Binary_Gls::factory)),
            ".aip" => (Tag::Int(0), Some(Binary_Aip::factory)),
            ".vtr" => (Tag::Int(0), Some(Binary_Vtr::factory)),
            ".att" => (Tag::Int(0), Some(Binary_Att::factory)),
            ".sfx" => (Tag::Int(0), Some(Binary_Sfx::factory)),
            _ => UncoreArchive::assetFactory(source, game),
        }
    }
}
