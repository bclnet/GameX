using GameX.Formats.IUnknown;
using GameX.IW.Formats;
using GameX.Transforms;
using GameX.Uncore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.IW;

/// <summary>
/// IWArchive
/// </summary>
/// <seealso cref="GameX.Formats.BinaryArchive" />
public class IWArchive : BinaryArchive, ITransformAsset<IUnknownFileModel> {
    /// <summary>
    /// Initializes a new instance of the <see cref="IWArchive" /> class.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="state">The state.</param>
    public IWArchive(Archive parent, BinaryState state) : base(parent, state, Binary_IW.Current) {
        AssetFactoryFunc = AssetFactory;
    }

    #region Factories

    static (object, Func<BinaryReader, FileSource, Archive, Task<object>>) AssetFactory(FileSource source, FamilyGame game)
        => Path.GetExtension(source.Path).ToLowerInvariant() switch {
            //".roq" => (0, VIDEO.Factory),
            //".wav" => (0, BinaryWav.Factory),
            //".d3dbsp" => (0, BinaryD3dbsp.Factory),
            ".iwi" => (0, Binary_Iwi.Factory),
            ".gsc" => (0, Binary_Gsc.Factory),
            ".xmp" => (0, Binary_Xmp.Factory),
            ".phy" => (0, Binary_Phy.Factory),
            ".xan" => (0, Binary_Xan.Factory),
            ".xmd" => (0, Binary_Xmd.Factory),
            ".mat" => (0, Binary_Mat.Factory),
            ".tec" => (0, Binary_Tec.Factory),
            ".snd" => (0, Binary_Snd.Factory),
            ".crv" => (0, Binary_Crv.Factory),
            ".lds" => (0, Binary_Lds.Factory),
            ".csp" => (0, Binary_Csp.Factory),
            ".cmp" => (0, Binary_Cmp.Factory),
            ".cmm" => (0, Binary_Cmm.Factory),
            ".gsp" => (0, Binary_Gsp.Factory),
            ".gmp" => (0, Binary_Gmp.Factory),
            ".ent" => (0, Binary_Ent.Factory),
            ".gfx" => (0, Binary_Gfx.Factory),
            ".lgt" => (0, Binary_Lgt.Factory),
            ".uim" => (0, Binary_Uim.Factory),
            ".fnt" => (0, Binary_Fnt.Factory),
            ".mnf" => (0, Binary_Mnf.Factory),
            ".mnu" => (0, Binary_Mnu.Factory),
            ".loc" => (0, Binary_Loc.Factory),
            ".wep" => (0, Binary_Wep.Factory),
            ".sdg" => (0, Binary_Sdg.Factory),
            ".fx" => (0, Binary_Fx.Factory),
            ".ifx" => (0, Binary_Ifx.Factory),
            ".ait" => (0, Binary_Ait.Factory),
            ".mpt" => (0, Binary_Mpt.Factory),
            ".chr" => (0, Binary_Chr.Factory),
            ".xma" => (0, Binary_Xma.Factory),
            ".raw" => (0, Binary_Raw.Factory),
            ".tbl" => (0, Binary_Tbl.Factory),
            ".pcm" => (0, Binary_Pcm.Factory),
            ".xms" => (0, Binary_Xms.Factory),
            ".psh" => (0, Binary_Psh.Factory),
            ".vsh" => (0, Binary_Vsh.Factory),
            ".vdl" => (0, Binary_Vdl.Factory),
            ".fxm" => (0, Binary_Fxm.Factory),
            ".lbd" => (0, Binary_Lbd.Factory),
            ".sdd" => (0, Binary_Sdd.Factory),
            ".trc" => (0, Binary_Trc.Factory),
            ".veh" => (0, Binary_Veh.Factory),
            ".ame" => (0, Binary_Ame.Factory),
            ".gls" => (0, Binary_Gls.Factory),
            ".aip" => (0, Binary_Aip.Factory),
            ".vtr" => (0, Binary_Vtr.Factory),
            ".att" => (0, Binary_Att.Factory),
            ".sfx" => (0, Binary_Sfx.Factory),
            _ => UncoreArchive.AssetFactory(source, game),
        };
    #endregion

    #region Transforms

    bool ITransformAsset<IUnknownFileModel>.CanTransformAsset(object source, Archive transformTo) => UnknownTransform.CanTransformAsset(this, transformTo, source);
    Task<IUnknownFileModel> ITransformAsset<IUnknownFileModel>.TransformAsset(object source, Archive transformTo) => UnknownTransform.TransformAsset(this, transformTo, source);

    #endregion
}
