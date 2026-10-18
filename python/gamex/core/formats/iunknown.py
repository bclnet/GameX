from __future__ import annotations
import os
from enum import Flag, IntFlag
from openx.core import Matrix4x4

#region IUnknownBone

class IUnknownBone:
    name: str
    worldToBone: Matrix4x4
    boneToWorld: Matrix4x4

#endregion

#region IUnknownFileObject

class IUnknownFileObject:
    class Source:
        def __init__(self, author: str = None, sourceFile: str = None):
            self.author = author
            self.sourceFile = sourceFile
    name: str
    path: str
    sources: list

#endregion

#region IUnknownFileModel

class IUnknownFileModel(IUnknownFileObject):
    models: list
    meshes: list
    materials: list
    proxies: list
    skinningInfo: object
    rootNodes: list

#endregion

#region IUnknownMaterial

class IUnknownMaterial:
    name: str
    diffuse: object
    specular: object
    emissive: object
    shininess: float
    opacity: float
    textures: list

#endregion

#region IUnknownModel

class IUnknownModel:
    path: str

#endregion

#region IUnknownProxy

class IUnknownProxy:
    class Proxy:
        def __init__(self, vertexs=None, indexs=None):
            self.vertexs = vertexs
            self.indexs = indexs
    physicalProxys: list

#endregion

#region IUnknownSkin

class IUnknownSkin:
    class BoneMap:
        def __init__(self, boneIndex=None, weight=None):
            self.boneIndex = boneIndex
            self.weight = weight
    class IntVertex:
        def __init__(self):
            self.obsolete0 = None
            self.position = None
            self.obsolete2 = None
            self.boneIDs = None
            self.weights = None
            self.color = None
    hasSkinningInfo: bool
    compiledBones: list
    intVertexs: list
    boneMaps: list
    ext2IntMaps: list

#endregion

#region IUnknownTexture

class IUnknownTexture:
    class Map(IntFlag):
        Diffuse = 1 << 0
        Bumpmap = 1 << 1
        Specular = 1 << 2
        Environment = 1 << 3
        Decal = 1 << 4
        SubSurface = 1 << 5
        Opacity = 1 << 6
        Detail = 1 << 7
        Heightmap = 1 << 8
        BlendDetail = 1 << 9
        Custom = 1 << 10
    path: str
    maps: Map

#endregion

#region IUnknownUnknown

# ChunkType, IGenericNode and IChunk stay commented, as in C#.

#endregion

#region UnknownFileWriter

class _IgnoreCase(dict):
    def _k(self, key): return key.lower() if isinstance(key, str) else key
    def __getitem__(self, key): return super().__getitem__(self._k(key))
    def __setitem__(self, key, value): super().__setitem__(self._k(key), value)
    def __contains__(self, key): return super().__contains__(self._k(key))
    def get(self, key, default=None): return super().get(self._k(key), default)

class UnknownFileWriter:
    factories = _IgnoreCase()
    noConflicts = False
    tiffTextures = False
    skipShieldNodes = False
    skipStreamNodes = False
    groupMeshes = True
    smooth = True

    def __init__(self, file: IUnknownFileModel):
        self.file = file

    def write(self, outputDir: str = None, preservePath: bool = True) -> None: raise NotImplementedError()

    def getFileInfo(self, extension: str, outputDir: str = None, preservePath: bool = True) -> str:
        fileName = f'temp.{extension}'
        if outputDir is None or outputDir.strip() == '':
            directory = os.path.dirname(self.file.path)
            stem = os.path.splitext(os.path.basename(self.file.path))[0]
            suffix = '_out' if UnknownFileWriter.noConflicts else ''
            fileName = os.path.join(directory, f'{stem}{suffix}.{extension}')
        else:
            preserveDir = os.path.dirname(self.file.path) if preservePath else ''
            root = os.path.splitdrive(preserveDir)[0]
            if preserveDir and preserveDir.strip() and root and root.strip(): preserveDir = preserveDir.replace(root, '', 1)
            fileName = os.path.join(outputDir, preserveDir, os.path.splitext(os.path.basename(self.file.path))[0] + (extension if extension.startswith('.') else '.' + extension))
        return fileName

    @staticmethod
    def factory(name: str, model: IUnknownFileModel):
        found = UnknownFileWriter.factories.get(name)
        if found is None: raise ValueError(name)
        return found(model)

#endregion

#region UnknownMesh

class UnknownMesh:
    class Effect(Flag):
        ScaleOffset = 0x1
    class Subset:
        def __init__(self, vertexs=None, indexs=None, matId: int = 0):
            self.vertexs = vertexs
            self.indexs = indexs
            self.matId = matId
    def __init__(self):
        self.name = None
        self.minBound = None
        self.maxBound = None
        self.subsets = None
        self.vertexs = None
        self.uvs = None
        self.normals = None
        self.indexs = None
        self.effects = UnknownMesh.Effect(0)
        self.scaleOffset3 = None
        self.scaleOffset4 = None
    def __getitem__(self, i: Subset):
        v = i.vertexs; ix = i.indexs
        return type('SubsetMesh', (), {
            'vertexs': self.vertexs[v.start:v.stop] if self.vertexs is not None else None,
            'uvs': self.uvs[v.start:v.stop] if self.uvs is not None else None,
            'normals': self.normals[v.start:v.stop] if self.normals is not None else None,
            'indexs': self.indexs[ix.start:ix.stop] if self.indexs is not None else None,
        })()
    def getTransform(self, vertex): return vertex

#endregion
