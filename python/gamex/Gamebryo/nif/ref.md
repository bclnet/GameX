# Objects
NiObject
NiObjectNET : NiObject
NiAVObject : NiObjectNET
NiCamera : NiAVObject

# Nodes
NiNode : NiAVObject
RootCollisionNode : NiNode
NiBSAnimationNode : NiNode
NiBSParticleNode : NiNode
NiBillboardNode : NiNode
AvoidNode : NiNode

# Geometry
NiGeometry : NiAVObject
NiGeometryData : NiObject
NiTriBasedGeom : NiGeometry
NiTriBasedGeomData : NiGeometryData
NiTriShape : NiTriBasedGeom
NiTriShapeData : NiTriBasedGeomData

# Properties
NiProperty : NiObjectNET
NiTexturingProperty : NiProperty
NiAlphaProperty : NiProperty
NiZBufferProperty : NiProperty
NiVertexColorProperty : NiProperty
NiShadeProperty : NiProperty
NiWireframeProperty : NiProperty


# Data
NiUVData : NiObject
NiKeyframeData : NiObject
NiColorData : NiObject
NiMorphData : NiObject
NiVisData : NiObject
NiFloatData : NiObject
NiPosData : NiObject
NiExtraData : NiObject
NiStringExtraData : NiExtraData
NiTextKeyExtraData : NiExtraData
NiVertWeightsExtraData : NiExtraData

# Particles
NiParticles : NiGeometry
NiParticlesData : NiGeometryData
NiRotatingParticles : NiParticles
NiRotatingParticlesData : NiParticlesData
NiAutoNormalParticles : NiParticles
NiAutoNormalParticlesData : NiParticlesData
NiParticleSystemController : NiTimeController
NiBSPArrayController : NiParticleSystemController

# Particle Modifiers
NiParticleModifier : NiObject
NiGravity : NiParticleModifier
NiParticleBomb : NiParticleModifier
NiParticleColorModifier : NiParticleModifier
NiParticleGrowFade : NiParticleModifier
NiParticleMeshModifier : NiParticleModifier
NiParticleRotation : NiParticleModifier

# Controllers
NiTimeController : NiObject
NiUVController : NiTimeController
NiInterpController : NiTimeController
NiSingleInterpController : NiInterpController
NiKeyframeController : NiSingleInterpController
NiGeomMorpherController : NiInterpController
NiBoolInterpController : NiSingleInterpController
NiVisController : NiBoolInterpController
NiFloatInterpController : NiSingleInterpController
NiAlphaController : NiFloatInterpController

# Skin Stuff
NiSkinInstance : NiObject
NiSkinData : NiObject
NiSkinPartition : NiObject

# Miscellaneous
NiTexture : NiObjectNET
NiSourceTexture : NiTexture
NiPoint3InterpController : NiSingleInterpController
NiMaterialProperty : NiProperty
NiMaterialColorController : NiPoint3InterpController
NiDynamicEffect : NiAVObject
NiTextureEffect : NiDynamicEffect
