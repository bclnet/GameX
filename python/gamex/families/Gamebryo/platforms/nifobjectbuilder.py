import sys, os
from numpy import ones, zeros
from openstk.gfx.gfx import GfxBlendMode, MaterialStd2Prop
from gamex.families.Gamebryo.formats.nif import NiAVObject, NiTexturingProperty, NiMaterialProperty, NiAlphaProperty

class NifObjectBuilder:
    YardInMWUnits: int = 64
    MeterInYards: float = 1.09361
    MeterInUnits: float = MeterInYards * YardInMWUnits;
    MarkerLayer: int = 0
    KinematicRigidbody: bool = False

    @staticmethod
    def toMaterialProp(s: NiAVObject) -> MaterialStd2Prop:
        # find relevant properties.
        tex: NiTexturingProperty = None
        mat: NiMaterialProperty = None
        alpha: NiAlphaProperty = None
        for t in s.properties:
            prop = t.value
            if isinstance(prop, NiTexturingProperty): tex = prop
            elif isinstance(prop, NiMaterialProperty): mat = prop
            elif isinstance(prop, NiAlphaProperty): alpha = prop

        # create the material properties.
        mp = MaterialStd2Prop()

        # apply alphaProperty
        if alpha:
            '''
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
            '''
            flags = int(alpha.flags)
            srcbm = (flags >> 1).to_bytes(2, byteorder='little')[0] & 15
            dstbm = (flags >> 5).to_bytes(2, byteorder='little')[0] & 15
            mp.zwrite = (flags >> 15).to_bytes(2, byteorder='little')[0] == 1
            mp.alphaBlended = (flags & 0x01) != 0
            mp.srcBlendMode = GfxBlendMode(min(srcbm, 10))
            mp.dstBlendMode = GfxBlendMode(min(dstbm, 10))
            mp.alphaTest = (flags & 0x100) != 0
            mp.alphaCutoff = alpha.threshold / 255.0

        # apply materialProperty
        if mat:
            mp.alpha = mat.alpha
            mp.diffuseColor = mat.diffuseColor.asColor
            mp.emissiveColor = mat.emissiveColor.asColor
            mp.specularColor = mat.specularColor.asColor
            mp.glossiness = mat.glossiness

        # apply texturingProperty
        if tex and tex.textureCount > 0:
            mt = mp.textures
            if tex.gaseTexture: mt['Main'] = tex.baseTexture.source.value.fileName
            if tex.darkTexture: mt['Dark'] = tex.darkTexture.source.value.fileName
            if tex.detailTexture: mt['Detail'] = tex.detailTexture.source.value.fileName
            if tex.glossTexture: mt['Gloss'] = tex.glossTexture.source.value.fileName
            if tex.glowTexture: mt['Glow'] = tex.glowTexture.source.value.fileName
            if tex.bumpMapTexture: mt['Bump'] = tex.bumpMapTexture.source.value.fileName
        return mp

    @staticmethod
    def isMarkerFileName(self, name: str) -> bool:
        match name.lower():
            case 'marker_light' | 'marker_north' | 'marker_error' | 'marker_arrow' | 'editormarker' | 'marker_creature' | 'marker_travel' | 'marker_temple' | 'marker_prison' | 'marker_radius' | 'marker_divine' | 'editormarker_box_01': return True
            case _: return False
