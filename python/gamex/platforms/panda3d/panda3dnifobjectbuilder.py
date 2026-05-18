import sys, os
from numpy import ones, zeros
from openstk.core import ISource, log
from openstk.gfx.gfx import MaterialManager
from gamex.families.Gamebryo.formats.binary import Binary_Nif
from gamex.families.Gamebryo.formats.nif import Flags, NiObject, NiObjectNET, NiStringExtraData, NiTriShape, NiTriShapeData, NiAVObject, RootCollisionNode, NiBSAnimationNode, NiNode, NiTextureEffect, NiBSParticleNode, NiRotatingParticles, NiAutoNormalParticles, NiBillboardNode, AvoidNode
from gamex.families.Gamebryo.platforms.nifobjectbuilder import NifObjectBuilder
from panda3d.core import GeomNode, NodePath, GeomVertexFormat, GeomVertexData, GeomVertexWriter, GeomTriangles, Geom

# types
type Object = object

class Panda3dNifObjectBuilder:
    @staticmethod
    async def buildObject(source: ISource, path: object, isStatic: bool, materialManager: MaterialManager) -> GeomNode:
        o: Binary_Nif = path; name: str = o.name
        assert(name and len(o.roots) > 0)

        # preload texture
        textureManager = materialManager._textureManager
        for texturePath in o.getTexturePaths(): textureManager.preloadTexture(source, texturePath)

        # NIF files can have any number of root NiObjects.
        # If there is only one root, instantiate that directly.
        # If there are multiple roots, create a container Object and parent it to the roots.
        if len(o.roots) == 1:
            s = o.roots[0].value
            gobj = await Panda3dNifObjectBuilder.instantiateRootNiObject(name, source, isStatic, materialManager, s)
            # If the file doesn't contain any NiObjects we are looking for, return an empty Object.
            if not gobj:
                log.info(f'{name} resulted in a null Object when instantiated.')
                node = GeomNode(name or '')
                gobj = render.attachNewNode(node)
            # If gobj != null and the root NiObject is an NiNode, discard any transformations (Morrowind apparently does).
            elif isinstance(s, NiNode):
                gobj.setPos(0., 0., 0.)
                gobj.setHpr(0., 0., 0.)
                gobj.setScale(1., 1., 1.)
            return gobj
        else:
            log.info(f'{name} has multiple roots.')
            node = GeomNode(name or '')
            gobj = render.attachNewNode(node)
            for s in o.roots:
                child = await Panda3dNifObjectBuilder.instantiateRootNiObject(name, source, isStatic, materialManager, s.value)
                if child: child.reparentTo(gobj)
            return gobj

    @staticmethod
    def applyNiAVObject(obj: Object, s: NiAVObject) -> None:
        z = s.translation / NifObjectBuilder.MeterInUnits; obj.setPos(z[0], z[1], z[2])
        # obj.transform.rotation = s.rotation.ToUnityQuaternionAsRotation()
        obj.setScale(s.scale, s.scale, s.scale)
        pass

    @staticmethod
    async def instantiateRootNiObject(name: str, source: ISource, isStatic: bool, materialManager: MaterialManager, s: NiObject)-> NodePath:
        gobj = await Panda3dNifObjectBuilder.instantiateNiObject(source, isStatic, materialManager, s)
        shouldAddMissingColliders, isMarker = Panda3dNifObjectBuilder.processExtraData(s)
        if name and NifObjectBuilder.isMarkerFileName(name): shouldAddMissingColliders = False; isMarker = True
        # Add colliders to the object if it doesn't already contain one.
        # if shouldAddMissingColliders and not gobj.getComponentInChildren[Collider]() and self.static: gobj.addMissingMeshCollidersRecursively()
        # if isMarker: gobj.setLayerRecursively(self.markerLayer)
        return gobj
    
    @staticmethod
    def processExtraData(s: NiObject) -> tuple[bool, bool]:
        shouldAddMissingColliders = True; isMarker = False
        if isinstance(s, NiObjectNET) and s.extraData:
            extraData = s.extraData.value
            while extraData:
                if isinstance(extraData, NiStringExtraData):
                    z = extraData
                    if z.stringData == 'NCO' or z.stringData == 'NCC': shouldAddMissingColliders = False
                    elif z.stringData == 'MRK': shouldAddMissingColliders = False; isMarker = True
                extraData = extraData.nextExtraData.value if extraData.nextExtraData else None
        return (shouldAddMissingColliders, isMarker)

    # Creates a Object representation of an NiObject.
    @staticmethod
    def instantiateNiObject(source: ISource, isStatic: bool, materialManager: MaterialManager, s: NiObject) -> NodePath:
        match s:
            case NiTriShape(): return Panda3dNifObjectBuilder.instantiateNiTriShape(source, isStatic, materialManager, s, True, False)
            case RootCollisionNode(): return Panda3dNifObjectBuilder.instantiateRcnNode(source, isStatic, materialManager, s)
            # case NiBSAnimationNode(): return instantiateNiNode(source, isStatic, materialManager, s)
            case NiNode(): return Panda3dNifObjectBuilder.instantiateNiNode(source, isStatic, materialManager, s)
            case NiTextureEffect(): return None
            # case NiBSParticleNode(): return None
            case NiRotatingParticles(): return None
            case NiAutoNormalParticles(): return None
            # case NiBillboardNode(): return None
            case _: raise Exception(f'Tried to instantiate an unsupported NiObject ({s}).')

    @staticmethod
    async def instantiateNiNode(source: ISource, isStatic: bool, materialManager: MaterialManager, s: NiNode) -> NodePath:
        node = GeomNode(s.name or '')
        obj = render.attachNewNode(node)
        for t in s.children:
            if t:
                z = Panda3dNifObjectBuilder.instantiateNiObject(source, isStatic, materialManager, t.value); child = await z if z else None
                if child: child.reparentTo(obj)
        Panda3dNifObjectBuilder.applyNiAVObject(obj, s)
        return obj

    @staticmethod
    async def instantiateRcnNode(source: ISource, isStatic: bool, materialManager: MaterialManager, s: RootCollisionNode) -> NodePath:
        node = GeomNode(f'Root Collision Node{s.name}')
        obj = render.attachNewNode(node)
        for t in s.children:
            if t != None:
                match t.value:
                    case NiTriShape(): await Panda3dNifObjectBuilder.instantiateNiTriShape(source, isStatic, materialManager, t.value, False, True) #.transform.SetParent(obj.transform, False)
                    case AvoidNode(): pass
                    case _: log.info(f'Unsupported collider NiObject: {t.value}')
        Panda3dNifObjectBuilder.applyNiAVObject(obj, s)
        return obj

    @staticmethod
    async def instantiateNiTriShape(source: ISource, isStatic: bool, materialManager: MaterialManager, s: NiTriShape, visual: bool, collidable: bool) -> NodePath:
        geom = Panda3dNifObjectBuilder.toGeometry(s.data.value)
        node = GeomNode(s.name or '')
        obj = render.attachNewNode(node)
        if visual:
            node.addGeom(geom)
            materialProps = NifObjectBuilder.toMaterialProp(s)
            mat, _ = await materialManager.createMaterial(source, materialProps)
            mat.apply(obj)
            if not materialProps.textures or Flags.Hidden in s.flags: obj.hide()
        elif collidable:
            if not isStatic:
                pass
                # obj.AddComponent<BoxCollider>()
                # obj.AddComponent<Rigidbody>().isKinematic = KinematicRigidbody
            else: pass #obj.AddComponent<MeshCollider>().sharedMesh = mesh
        Panda3dNifObjectBuilder.applyNiAVObject(obj, s)
        return obj

    @staticmethod
    def toGeometry(s: NiTriShapeData) -> object:
        length = len(s.vertices)
        vdata = GeomVertexData('name', GeomVertexFormat.getV3n3t2(), Geom.UHStatic)
        vdata.setNumRows(length)
        # vertex positions
        vertex = GeomVertexWriter(vdata, 'vertex')
        for t in s.vertices: z = t / NifObjectBuilder.MeterInUnits; vertex.addData3(z[0], z[1], z[2])
        # vertex normals
        if s.normals:
            normal = GeomVertexWriter(vdata, 'normal')
            for t in s.normals: normal.addData3(t[0], t[1], t[2])
        # vertex UV coordinates
        if s.uvSets:
            texcoord = GeomVertexWriter(vdata, 'texcoord')
            for t in s.uvSets[0]: texcoord.addData2(t.u, t.v)
        # triangle vertex indices
        prim = GeomTriangles(Geom.UHStatic)
        for t in s.triangles: prim.addVertices(t.v1, t.v3, t.v2) # Reverse triangle winding order.
        # create the mesh.
        geom = Geom(vdata)
        geom.addPrimitive(prim)
        # if not data.normals: geom.recalculateNormals()
        # geom.recalculateBounds()
        return geom
