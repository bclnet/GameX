from openx.core import ISource
from openx.gfx.gfx import MaterialManager
from gamex.families.Arkane.engines.ftlObjectBuilder import getTexturePaths as ftlGetTexturePaths, groupPositions, toGeometry as ftlToGeometry, toMaterialProp as ftlToMaterialProp
from gamex.families.Arkane.engines.ftsObjectBuilder import getTexturePaths as ftsGetTexturePaths, scenePositions, toGeometry as ftsToGeometry, toMaterialProp as ftsToMaterialProp

# An FTL kept as the shared mesh. Danae is Y-up, so positions stay as stored.
# OpenGL's material builder only accepts MaterialShaderVProp, so the props stay MaterialStd2Prop.

class OpenGLFtlObject:
    def __init__(self, name: str, isStatic: bool, data, materials, groups):
        self.name = name
        self.isStatic = isStatic
        self.data = data
        self.materials = materials
        self.groups = groups

    def drawGl(self) -> None:
        drawMesh(self.data)

def _xyz(v):
    if v is None: return 0.0, 0.0, 0.0
    if hasattr(v, 'x'): return float(v.x), float(v.y), float(v.z)
    return float(v[0]), float(v[1]), float(v[2])

def drawMesh(data) -> None:
    vertices = getattr(data, 'vertices', None) if data is not None else None
    subs = getattr(data, 'submeshes', None) if data is not None else None
    if not vertices or not subs: return
    indices = []
    for sub in subs:
        tris = getattr(sub, 'triangles', None)
        if tris: indices.extend(tris)
    if not indices: return
    from OpenGL.GL import GL_ARRAY_BUFFER, GL_ELEMENT_ARRAY_BUFFER, GL_FLOAT, GL_FRAGMENT_SHADER, GL_STATIC_DRAW, GL_TRIANGLES, GL_UNSIGNED_INT, GL_VERTEX_SHADER, glAttachShader, glBindBuffer, glBindVertexArray, glBufferData, glCompileShader, glCreateProgram, glCreateShader, glDeleteShader, glDrawElements, glEnableVertexAttribArray, glGenBuffers, glGenVertexArrays, glLinkProgram, glShaderSource, glUseProgram, glVertexAttribPointer
    import ctypes
    xs, ys, zs = [], [], []
    for v in vertices:
        x, y, z = _xyz(v)
        xs.append(x); ys.append(y); zs.append(z)
    cx = (min(xs) + max(xs)) * 0.5
    cy = (min(ys) + max(ys)) * 0.5
    cz = (min(zs) + max(zs)) * 0.5
    scale = max(max(xs) - cx, cx - min(xs), max(ys) - cy, cy - min(ys), max(zs) - cz, cz - min(zs), 1e-6)
    scale = 0.8 / scale
    packed = []
    for x, y, z in zip(xs, ys, zs):
        x = (x - cx) * scale; y = (y - cy) * scale; z = (z - cz) * scale
        packed.extend((x * 0.9 + z * 0.35, y * 0.9 - z * 0.2, 0.0))
    if not hasattr(drawMesh, 'program'):
        vs = glCreateShader(GL_VERTEX_SHADER)
        glShaderSource(vs, '#version 330 core\nlayout(location=0) in vec3 a;\nvoid main(){ gl_Position = vec4(a, 1.0); }\n')
        glCompileShader(vs)
        fs = glCreateShader(GL_FRAGMENT_SHADER)
        glShaderSource(fs, '#version 330 core\nout vec4 c;\nvoid main(){ c = vec4(0.75, 0.78, 0.82, 1.0); }\n')
        glCompileShader(fs)
        program = glCreateProgram()
        glAttachShader(program, vs); glAttachShader(program, fs); glLinkProgram(program)
        glDeleteShader(vs); glDeleteShader(fs)
        drawMesh.program = program
        drawMesh.cache = {}
    key = id(data)
    slot = drawMesh.cache.get(key)
    if slot is None:
        vao = glGenVertexArrays(1)
        glBindVertexArray(vao)
        vbo = glGenBuffers(1)
        glBindBuffer(GL_ARRAY_BUFFER, vbo)
        arr = (ctypes.c_float * len(packed))(*packed)
        glBufferData(GL_ARRAY_BUFFER, ctypes.sizeof(arr), arr, GL_STATIC_DRAW)
        glEnableVertexAttribArray(0)
        glVertexAttribPointer(0, 3, GL_FLOAT, False, 12, ctypes.c_void_p(0))
        ibo = glGenBuffers(1)
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ibo)
        idx = (ctypes.c_uint * len(indices))(*indices)
        glBufferData(GL_ELEMENT_ARRAY_BUFFER, ctypes.sizeof(idx), idx, GL_STATIC_DRAW)
        slot = (vao, len(indices))
        drawMesh.cache[key] = slot
    glUseProgram(drawMesh.program)
    glBindVertexArray(slot[0])
    glDrawElements(GL_TRIANGLES, slot[1], GL_UNSIGNED_INT, ctypes.c_void_p(0))
    glBindVertexArray(0)
    glUseProgram(0)

class OpenGLFtlObjectBuilder:
    @staticmethod
    async def buildObject(source: ISource, path: object, isStatic: bool, materialManager: MaterialManager) -> OpenGLFtlObject:
        obj = path.obj
        name = obj.file if obj.file else 'ftl'
        textureManager = materialManager._textureManager
        for texturePath in ftlGetTexturePaths(obj): textureManager.preload(source, texturePath)
        data = ftlToGeometry(obj)
        materials = []
        for sub in data.submeshes:
            tex = None
            if obj.textures and sub.texId >= 0 and sub.texId < len(obj.textures): tex = obj.textures[sub.texId]
            materials.append(ftlToMaterialProp(tex))
        return OpenGLFtlObject(name, isStatic, data, materials, groupPositions(obj))

# An FTS kept as the shared mesh. Danae is Y-up, so positions stay as stored.
# OpenGL's material builder only accepts MaterialShaderVProp, so the props stay MaterialStd2Prop.

class OpenGLFtsObject:
    def __init__(self, name: str, isStatic: bool, data, materials, groups):
        self.name = name
        self.isStatic = isStatic
        self.data = data
        self.materials = materials
        self.groups = groups

    def drawGl(self) -> None:
        drawMesh(self.data)

class OpenGLFtsObjectBuilder:
    @staticmethod
    async def buildObject(source: ISource, path: object, isStatic: bool, materialManager: MaterialManager) -> OpenGLFtsObject:
        level = path.level
        name = 'fts'
        textureManager = materialManager._textureManager
        for texturePath in ftsGetTexturePaths(level): textureManager.preload(source, texturePath)
        data = ftsToGeometry(level)
        materials = []
        for sub in data.submeshes:
            tex = None
            if level.textures and sub.texId >= 0 and sub.texId < len(level.textures): tex = level.textures[sub.texId]
            materials.append(ftsToMaterialProp(tex, sub.type))
        return OpenGLFtsObject(name, isStatic, data, materials, scenePositions(level))
