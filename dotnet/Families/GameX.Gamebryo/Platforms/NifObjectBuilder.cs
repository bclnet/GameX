using GameX.Gamebryo.Formats.Nif;
using OpenStack.Gfx;
using System;

namespace GameX.Gamebryo.Platforms;

public static class NifObjectBuilder {
    public const int YardInMWUnits = 64;
    public const float MeterInYards = 1.09361f;
    public const float MeterInUnits = MeterInYards * YardInMWUnits;
    public const int MarkerLayer = 0;
    public const bool KinematicRigidbody = false;

    public static MaterialStd2Prop ToMaterialProp(NiAVObject s) {
        // find relevant properties.
        NiTexturingProperty tex = null;
        NiMaterialProperty mat = null;
        NiAlphaProperty alpha = null;
        foreach (var t in s.Properties) {
            var prop = t.Value;
            if (prop is NiTexturingProperty tp) tex = tp;
            else if (prop is NiMaterialProperty mp2) mat = mp2;
            else if (prop is NiAlphaProperty ap) alpha = ap;
        }

        // create the material properties.
        var mp = new MaterialStd2Prop();

        // apply alphaProperty
        if (alpha != null) {
            /*
            14 bits used:
            1 bit for alpha blend bool
            4 bits for src blend mode
            4 bits for dest blend mode
            1 bit for alpha test bool
            3 bits for alpha test mode
            1 bit for zwrite bool ( opposite value )
            Bit 0 : alpha blending enable
            Bits 1-4 : source blend mode 
            Bits 5-8 : destination blend mode
            Bit 9 : alpha test enable
            Bit 10-12 : alpha test mode
            Bit 13 : no sorter flag ( disables triangle sorting ) ( Unity ZWrite )
            */
            var flags = (ushort)alpha.Flags;
            var srcbm = (byte)(BitConverter.GetBytes(flags >> 1)[0] & 15);
            var dstbm = (byte)(BitConverter.GetBytes(flags >> 5)[0] & 15);
            mp.ZWrite = BitConverter.GetBytes(flags >> 15)[0] == 1;
            mp.AlphaBlended = (flags & 0x01) != 0;
            mp.SrcBlendMode = (GfxBlendMode)Math.Min((int)srcbm, 10);
            mp.DstBlendMode = (GfxBlendMode)Math.Min((int)dstbm, 10);
            mp.AlphaTest = (flags & 0x100) != 0;
            mp.AlphaCutoff = (float)alpha.Threshold / 255;
        }

        // apply materialProperty
        if (mat != null) {
            mp.Alpha = mat.Alpha;
            mp.DiffuseColor = mat.DiffuseColor.AsColor;
            mp.EmissiveColor = mat.EmissiveColor.AsColor;
            mp.SpecularColor = mat.SpecularColor.AsColor;
            mp.Glossiness = mat.Glossiness;
        }

        // apply texturingProperty
        if (tex != null && tex.TextureCount > 0) {
            var mt = mp.Textures;
            if (tex.BaseTexture != null) mt.Add("Main", tex.BaseTexture.Source.Value.FileName);
            if (tex.DarkTexture != null) mt.Add("Dark", tex.DarkTexture.Source.Value.FileName);
            if (tex.DetailTexture != null) mt.Add("Detail", tex.DetailTexture.Source.Value.FileName);
            if (tex.GlossTexture != null) mt.Add("Gloss", tex.GlossTexture.Source.Value.FileName);
            if (tex.GlowTexture != null) mt.Add("Glow", tex.GlowTexture.Source.Value.FileName);
            if (tex.BumpMapTexture != null) mt.Add("Bump", tex.BumpMapTexture.Source.Value.FileName);
        }
        return mp;
    }

    public static bool IsMarkerFileName(string name) => name.ToLowerInvariant() switch {
        "marker_light" or "marker_north" or "marker_error" or "marker_arrow" or "editormarker" or "marker_creature" or "marker_travel" or "marker_temple" or "marker_prison" or "marker_radius" or "marker_divine" or "editormarker_box_01" => true,
        _ => false,
    };
}
