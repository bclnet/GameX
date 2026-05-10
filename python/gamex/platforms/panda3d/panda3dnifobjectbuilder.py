import sys, os
from numpy import ones, zeros
# from openstk.core import log
from openstk.gfx.gfx import MaterialManager
from gamex.families.Gamebryo.formats.binary import Binary_Nif
from gamex.families.Gamebryo.formats.nif import NiObject, NiObjectNET, NiStringExtraData, NiTriShape, NiTriShapeData, NiAVObject, RootCollisionNode, NiBSAnimationNode, NiNode, NiTextureEffect, NiBSParticleNode, NiRotatingParticles, NiAutoNormalParticles, NiBillboardNode, AvoidNode
from gamex.families.Gamebryo.platforms.nifobjectbuilder import NifObjectBuilder

# types
type Object = object

class Panda3dNifObjectBuilder:
    @staticmethod
    def buildObject(src: Binary_Nif, isStatic: bool, materialManager: MaterialManager) -> Object:
        assert(src.name and len(src.roots) > 0)

        # preload texture
        textureManager = materialManager._textureManager
        for texturePath in file.getTexturePaths(): textureManager.preloadTexture(texturePath)

        # NIF files can have any number of root NiObjects.
        # If there is only one root, instantiate that directly.
        # If there are multiple roots, create a container Object and parent it to the roots.
        if len(src.roots) == 1:
            rootNiObject = src.roots[0].value
            gobj = self.instantiateRootNiObject(rootNiObject)
            # If the file doesn't contain any NiObjects we are looking for, return an empty Object.
            if not gobj:
                log.info(f'{src.name} resulted in a null Object when instantiated.')
                gobj = Object(src.name)
            # If gobj != null and the root NiObject is an NiNode, discard any transformations (Morrowind apparently does).
            elif isinstance(rootNiObject, NiNode):
                # gobj.transform.position = zeros(3)
                # gobj.transform.rotation = quaternion()
                # gobj.transform.localScale = ones(3)
                pass
            return gobj
        else:
            log.info(f'{src.name} has multiple roots.')
            gobj = Object(src.Name)
            for rootRef in src.roots:
                child = self.instantiateRootNiObject(rootRef.Value)
                # if child: child.transform.setParent(gobj.transform, False)
            return gobj

    @staticmethod
    def applyNiAVObject(obj: Object, s: NiAVObject) -> None:
        # obj.transform.position = niAVObject.translation.ToUnity() / MeterInUnits
        # obj.transform.rotation = niAVObject.rotation.ToUnityQuaternionAsRotation()
        # obj.transform.localScale = niAVObject.scale * ones(3)
        pass

    @staticmethod
    def instantiateRootNiObject(src: Binary_Nif, isStatic: bool, materialManager: MaterialManager, s: NiObject)-> Object:
        gobj = self.instantiateNiObject(isStatic, materialManager, s)
        shouldAddMissingColliders, isMarker = processExtraData(s)
        if src.name and self.isMarkerFileName(src.name): shouldAddMissingColliders = False; isMarker = True
        # Add colliders to the object if it doesn't already contain one.
        # if shouldAddMissingColliders and not gobj.getComponentInChildren[Collider]() and self.static: gobj.addMissingMeshCollidersRecursively()
        # if isMarker: gobj.setLayerRecursively(self.markerLayer)
        return gobj
    
    @staticmethod
    def processExtraData(s: NiObject) -> tuple[bool, bool]:
        shouldAddMissingColliders = True; isMarker = False
        if isinstance(obj, NiObjectNET) and obj.extraData:
            extraData = obj.extraData.value
            while extraData:
                if isinstance(extraData, NiStringExtraData):
                    z = extraData
                    if z.stringData == 'NCO' or z.stringData == 'NCC': shouldAddMissingColliders = False
                    elif z.stringData == 'MRK': shouldAddMissingColliders = False; isMarker = True
                extraData = extraData.nextExtraData.value if extraData.nextExtraData else None
        return (shouldAddMissingColliders, isMarker)

    # Creates a Object representation of an NiObject.
    @staticmethod
    def instantiateNiObject(isStatic: bool, materialManager: MaterialManager, s: NiObject) -> Object:
        match s:
            case NiTriShape(): return instantiateNiTriShape(isStatic, materialManager, s, True, False)
            case RootCollisionNode(): return instantiateRcnNode(isStatic, materialManager, s)
            # case NiBSAnimationNode(): return instantiateNiNode(isStatic, materialManager, s)
            case NiNode(): return instantiateNiNode(isStatic, materialManager, s)
            case NiTextureEffect(): return None
            # case NiBSParticleNode(): return None
            case NiRotatingParticles(): return None
            case NiAutoNormalParticles(): return None
            # case NiBillboardNode(): return None
            case _: raise Exception(f'Tried to instantiate an unsupported NiObject ({s.type}).')

    @staticmethod
    def instantiateNiNode(isStatic: bool, materialManager: MaterialManager, s: NiNode) -> Object:
        obj = Object(s.name)
        for t in s.children:
            if t:
                child = instantiateNiObject(isStatic, materialManager, t.value)
                # if child: child.transform.setParent(obj.transform, False)
        applyNiAVObject(obj, s)
        return obj

    @staticmethod
    def instantiateRcnNode(isStatic: bool, materialManager: MaterialManager, s: RootCollisionNode) -> Object:
        obj = Object(f'Root Collision Node{s.name}')
        for t in s.children:
            if t != None:
                match t.value:
                    case NiTriShape(): instantiateNiTriShape(isStatic, materialManager, s, False, True) #.transform.SetParent(obj.transform, False)
                    case AvoidNode(): pass
                    case _: log.info(f'Unsupported collider NiObject: {t.value}')
        applyNiAVObject(obj, s)
        return obj

    @staticmethod
    def instantiateNiTriShape(isStatic: bool, materialManager: MaterialManager, s: NiTriShape, visual: bool, collidable: bool) -> Object:
        mesh = niTriShapeDataToMesh(s.data.value)
        obj = Object(s.name)
        # if visual:
        #     materialProps = NiAVObjectToMaterialProp(triShape);
        #     obj.AddComponent<MeshFilter>().mesh = mesh;
        #     meshRenderer = obj.AddComponent<MeshRenderer>();
        #     meshRenderer.material = _materialManager.CreateMaterial(materialProps).mat;
        #     if not materialProps.textures or Flags.Hidden in triShape.flags: meshRenderer.enabled = False
        #     obj.isStatic = True
        # elif collidable:
        #     if not self.isStatic:
        #         obj.AddComponent<BoxCollider>();
        #         obj.AddComponent<Rigidbody>().isKinematic = KinematicRigidbody;
        #     else: obj.AddComponent<MeshCollider>().sharedMesh = mesh
        applyNiAVObject(obj, s)
        return obj

    @staticmethod
    def niTriShapeDataToMesh(s: NiTriShapeData) -> object:
        # # vertex positions
        # vertices = Vector3[len(data.vertices)
        # for (var i = 0; i < vertices.Length; i++) vertices[i] = data.Vertices[i].ToUnity() / MeterInUnits;
        # # vertex normals
        # Vector3[] normals = null;
        # if data.normals:
        #     normals = new Vector3[vertices.Length];
        #     for (var i = 0; i < normals.Length; i++) normals[i] = data.Normals[i].ToUnity();
        # # vertex UV coordinates
        # uvs: list[Vector2] = None
        # if data.uvSets:
        #     uvs = Vector2[vertices.Length];
        #     for i in len(uvs):
        #         niTexCoord = data.uvSets[0][i]
        #         uvs[i] = array([niTexCoord.u, niTexCoord.v])
        # # triangle vertex indices
        # triangles = [0]*data.numTrianglePoints
        # for i in range(data.numTrianglePoints):
        #     baseI = 3 * i
        #     # Reverse triangle winding order.
        #     triangles[baseI] = data.triangles[i].v1
        #     triangles[baseI + 1] = data.triangles[i].v3
        #     triangles[baseI + 2] = data.triangles[i].v2;

        # # create the mesh.
        # mesh = new Mesh {
        #     vertices = vertices,
        #     normals = normals,
        #     uv = UVs,
        #     triangles = triangles
        # };
        # if (data.Normals == null) mesh.RecalculateNormals();
        # mesh.RecalculateBounds();
        # return mesh;
        return None
