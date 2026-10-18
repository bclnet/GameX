from __future__ import annotations
import ctypes, asyncio, math
from numpy import array, ones, float32, identity, float64
from enum import Enum
from OpenGL.GL import *
from openx.core import ISource, log, CellManager
from openx.gfx import GfX, Renderer, ITextureFrames
from openx.gfx.egin import AABB, EginRenderer
from openx.engines.opengl.egin import GLRenderMaterial
from openx.engines.opengl.gfx.opengl import OpenGLCellBuilder
from openx.engines.opengl.gfx.openglopenengine import OpenGLOpenEngine

sizeof_float = ctypes.sizeof(GLfloat)

# typedefs
class OpenGLGfxModel: pass
class GfxShader: pass
class Camera: pass
class IOpenGfx: pass

#region TestTriRenderer

# TestTriRenderer
class TestTriRenderer(EginRenderer):
    def __init__(self, gfx: list[IOpenGfx], source: ISource, obj: object):
        self.gfxModel: OpenGLGfxModel = gfx[GfX.XModel]
        self.shader, self.shaderTag = asyncio.run(self.gfxModel.shaderManager.create(source, 'testtri'))
        self.vao: int = self._setupVao()
        self.boundingBox: AABB = AABB(-1., -1., -1., 1., 1., 1.)

    def _setupVao(self) -> int:
        glUseProgram(self.shader.program)

        # create and bind vao
        vao = glGenVertexArrays(1); glBindVertexArray(vao)
        vbo = glGenBuffers(1); glBindBuffer(GL_ARRAY_BUFFER, vbo)
        vertices = array([
            # xyz,           :rgb
           -0.5, -0.5, 0.0,  1.0, 0.0, 0.0,
            0.5, -0.5, 0.0,  0.0, 1.0, 0.0,
            0.0,  0.5, 0.0,  0.0, 0.0, 1.0
            ], dtype=float32)
        glBufferData(GL_ARRAY_BUFFER, vertices.nbytes, vertices, GL_STATIC_DRAW)

        # attributes
        glEnableVertexAttribArray(0); glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 24, ctypes.c_void_p(0))
        glEnableVertexAttribArray(1); glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 24, ctypes.c_void_p(12))
        glBindVertexArray(0) # unbind vao
        return vao

    def render(self, camera: Camera, passx: Pass) -> None:
        # glClear(GL_COLOR_BUFFER_BIT)
        glUseProgram(self.shader.program)
        glBindVertexArray(self.vao)
        glDrawArrays(GL_TRIANGLES, 0, 6)
        glBindVertexArray(0) # unbind vao
        glUseProgram(0) # unbind program

    def update(self, deltaTime: float) -> None: pass

#endregion

#region TextureRenderer

FACTOR = 1

# TextureRenderer
class TextureRenderer(EginRenderer):
    def __init__(self, gfx: list[IOpenGfx], source: ISource, obj: object, level: range, background: bool = False):
        self.gfxModel: OpenGLGfxModel = gfx[GfX.XModel]
        self.source: ISource = source
        self.obj: object = obj
        self.level: range = level
        self.gfxModel.textureManager.delete(source, obj)
        self.tex: int = asyncio.run(self.gfxModel.textureManager.create(source, obj, self.level))[0] or -1
        self.shader, self.shaderTag = asyncio.run(self.gfxModel.shaderManager.create(source, 'plane'))
        self.vao: int = self._setupVao()
        self.background: bool = background
        self.boundingBox: AABB = AABB(-1., -1., -1., 1., 1., 1.)
        self.frameDelay: float = 0

    def getViewport(self, size: tuple) -> tuple:
        return None if not (o := self.obj) else \
            size if o.width > 1024 or o.height > 1024 or False else (o.width << FACTOR, o.height << FACTOR)

    def _setupVao(self) -> int:
        glUseProgram(self.shader.program)
        # create and bind vao
        vao = glGenVertexArrays(1); glBindVertexArray(vao) 
        vbo = glGenBuffers(1); glBindBuffer(GL_ARRAY_BUFFER, vbo)
        vertices = array([
            # position      :normal        :texcoord  :tangent
            -1., -1., +0.,  +0., +0., 1.,  +0., +1.,  +1., +0., +0.,
            -1., +1., +0.,  +0., +0., 1.,  +0., +0.,  +1., +0., +0.,
            +1., -1., +0.,  +0., +0., 1.,  +1., +1.,  +1., +0., +0.,
            +1., +1., +0.,  +0., +0., 1.,  +1., +0.,  +1., +0., +0.
            ], dtype = float32)
        glBufferData(GL_ARRAY_BUFFER, vertices.nbytes, vertices, GL_STATIC_DRAW)

        # attributes
        glEnableVertexAttribArray(0)
        attributes = [
            ('vPOSITION', 3),
            ('vNORMAL', 3),
            ('vTEXCOORD', 2),
            ('vTANGENT', 3)]
        offset = 0; stride = sizeof_float * sum([x[1] for x in attributes])
        for name, size in attributes:
            location = self.shader.attribLocation(name)
            if location > -1: glEnableVertexAttribArray(location); glVertexAttribPointer(location, size, GL_FLOAT, GL_FALSE, stride, ctypes.c_void_p(offset))
            offset += sizeof_float * size
        glBindVertexArray(0) # unbind vao
        return vao

    def render(self, camera: Camera, passx: Pass) -> None:
        if self.background: glClearColor(255, 255, 255, 255); glClear(GL_COLOR_BUFFER_BIT)
        glUseProgram(self.shader.program)
        glBindVertexArray(self.vao)
        glEnableVertexAttribArray(0)
        if self.tex > -1: glActiveTexture(GL_TEXTURE0); glBindTexture(GL_TEXTURE_2D, self.tex)
        glDrawArrays(GL_TRIANGLE_STRIP, 0, 4)
        glBindVertexArray(0) # unbind vao
        glUseProgram(0) # unbind program

    def update(self, deltaTime: float) -> None:
        obj = self.obj if isinstance(self.obj, ITextureFrames) else None
        if not obj or not obj.hasFrames(): return
        self.frameDelay += deltaTime
        if self.frameDelay <= obj.fps or not obj.nextFrame(): return
        self.frameDelay = 0 # reset delay between frames
        self.gfxModel.textureManager.reloadTexture(self.source, obj)

#endregion

#region ObjectRenderer

# ObjectRenderer
class ObjectRenderer(EginRenderer):
    def __init__(self, gfx: list[IOpenGfx], source: ISource, obj: object):
        self.gfxModel: OpenGLGfxModel = gfx[GfX.XModel]
        self.source: ISource = source
        self.obj: object = obj

    def start(self) -> None:
        asyncio.run(self.gfxModel.objectManager.createObject(self.source, self.obj, True, None))

#endregion

#region MaterialRenderer

# MaterialRenderer
class MaterialRenderer(EginRenderer):
    def __init__(self, gfx: list[IOpenGfx], source: ISource, obj: object):
        self.gfxModel: OpenGLGfxModel = gfx[GfX.XModel]
        self.gfxModel.textureManager.delete(source, obj)
        self.material, _ = asyncio.run(self.gfxModel.materialManager.createMaterial(source, obj)) #GLRenderMaterial
        self.shader, self.shaderTag = asyncio.run(self.gfxModel.shaderManager.create(source, material.material.shaderName, material.material.getShaderArgs()))
        self.vao: int = self._setupVao()
        self.boundingBox: AABB = AABB(-1., -1., -1., 1., 1., 1.)

    def _setupVao(self) -> int:
        glUseProgram(self.shader.program)

        # create and bind vao
        vao = glGenVertexArrays(1); glBindVertexArray(vao)
        vbo = glGenBuffers(1); glBindBuffer(GL_ARRAY_BUFFER, vbo)
        vertices = array([
            # position      :normal        :texcoord  :tangent        :blendindices        :blendweight
            -1., -1., +0.,  +0., +0., 1.,  +0., +1.,  +1., +0., +0.,  +0., +0., +0., +0.,  +0., +0., +0., +0.,
            -1., +1., +0.,  +0., +0., 1.,  +0., +0.,  +1., +0., +0.,  +0., +0., +0., +0.,  +0., +0., +0., +0.,
            +1., -1., +0.,  +0., +0., 1.,  +1., +1.,  +1., +0., +0.,  +0., +0., +0., +0.,  +0., +0., +0., +0.,
            +1., +1., +0.,  +0., +0., 1.,  +1., +0.,  +1., +0., +0.,  +0., +0., +0., +0.,  +0., +0., +0., +0.
            ], dtype=float32)
        glBufferData(GL_ARRAY_BUFFER, vertices.nbytes, vertices, GL_STATIC_DRAW)
        
        # attributes
        glEnableVertexAttribArray(0)
        attributes = [
            ('vPOSITION', 3),
            ('vNORMAL', 3),
            ('vTEXCOORD', 2),
            ('vTANGENT', 3)]
        offset = 0; stride = sizeof_float * sum([x[1] for x in attributes])
        for name, size in attributes:
            location = self.shader.attribLocation(name)
            if location > -1: glEnableVertexAttribArray(location); glVertexAttribPointer(location, size, GL_FLOAT, GL_FALSE, stride, ctypes.c_void_p(offset))
            offset += sizeof_float * size
        glBindVertexArray(0) # unbind vao
        return vao

    def render(self, camera: Camera, passx: Pass) -> None:
        identity_ = identity(4)
        glUseProgram(self.shader.program)
        glBindVertexArray(self.vao)
        glEnableVertexAttribArray(0)
        location = self.shader.uniformLocation('m_vTintColorSceneObject')
        if location > -1: glUniform4(uniformLocation, ones(4))
        location = self.shader.uniformLocation('m_vTintColorDrawCall')
        if location > -1: glUniform3(uniformLocation, ones(3))
        location = self.shader.uniformLocation('uProjectionViewMatrix')
        if location > -1: glUniformMatrix4(uniformLocation, False, identity_)
        location = self.shader.uniformLocation('transform')
        if location > -1: glUniformMatrix4(uniformLocation, False, identity_)
        self.material.render(self.shader)
        glDrawArrays(GL_TRIANGLE_STRIP, 0, 4)
        self.material.postRender()
        glBindVertexArray(0) # unbind vao
        glUseProgram(0) # unbind program

    def update(self, frameTime: float) -> None: pass

#endregion

#region GridRenderer

# GridRenderer
class GridRenderer(EginRenderer):
    def __init__(self, gfx: list[IOpenGfx], source: ISource, cellWidth: float, gridWidthInCells: int):
        self.gfxModel: OpenGLGfxModel = gfx[GfX.XModel]
        self.shader, self.shaderTag = asyncio.run(self.gfxModel.shaderManager.create(source, 'vrf.grid'))
        self.vao: int = self._setupVao(cellWidth, gridWidthInCells)
        self.boundingBox = AABB(
            -cellWidth * 0.5 * gridWidthInCells, -cellWidth * 0.5 * gridWidthInCells, 0,
            cellWidth * 0.5 * gridWidthInCells, cellWidth * 0.5 * gridWidthInCells, 0)
        self.vertexCount: int = 0 

    @staticmethod
    def _generateGridVertexBuffer(cellWidth: float, gridWidthInCells: int) -> list[float]:
        vertices: list[float] = []
        width = cellWidth * gridWidthInCells
        color = [1., 1., 1., 1.]
        for i in range(gridWidthInCells):
            vertices.extend([width, i * cellWidth, 0.])
            vertices.extend(color)
            vertices.extend([-width, i * cellWidth, 0.])
            vertices.extend(color)
        for i in range(gridWidthInCells):
            vertices.extend([width, -i * cellWidth, 0.])
            vertices.extend(color)
            vertices.extend([-width, -i * cellWidth, 0.])
            vertices.extend(color)
        for i in range(gridWidthInCells):
            vertices.extend([i * cellWidth, width, 0.])
            vertices.extend(color)
            vertices.extend([i * cellWidth, -width, 0.])
            vertices.extend(color)
        for i in range(gridWidthInCells):
            vertices.extend([-i * cellWidth, width, 0.])
            vertices.extend(color)
            vertices.extend([-i * cellWidth, -width, 0.])
            vertices.extend(color)
        return vertices

    def _setupVao(self, cellWidth: float, gridWidthInCells: int) -> int:
        STRIDE: int = 28
        glUseProgram(self.shader.program)
        # create and bind vao
        vao = glGenVertexArray(); glBindVertexArray(vao)
        vbo = glGenBuffer(); glBindBuffer(GL_ARRAY_BUFFER, vbo)
        vertices = _generateGridVertexBuffer(cellWidth, gridWidthInCells)
        self.vertexCount = len(vertices) / 3 # number of vertices in our buffer
        glBufferData(GL_ARRAY_BUFFER, len(vertices) * sizeof_float, vertices, GL_STATIC_DRAW)
        # attributes
        glEnableVertexAttribArray(0)
        location = self.shader.attribLocation('aVertexPosition')
        if location > -1: glEnableVertexAttribArray(location); glVertexAttribPointer(location, 3, GL_FLOAT, GL_FALSE, STRIDE, ctypes.c_void_p(0))
        location = self.shader.attribLocation('aVertexColor')
        if location > -1: glEnableVertexAttribArray(location); glVertexAttribPointer(location, 4, GL_FLOAT, GL_FALSE, STRIDE, ctypes.c_void_p(sizeof_float * 3))
        glBindVertexArray(0) # unbind vao
        glUseProgram(0) # unbind program
        return vao

    def render(self, camera: Camera, passx: Pass) -> None:
        glEnable(GL_BLEND)
        glBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)
        glUseProgram(self.shader.program)
        matrix = camera.ViewProjectionMatrix.ToOpenTK()
        glUniformMatrix4(self.shader.uniformLocation('uProjectionViewMatrix'), False, matrix)
        glBindVertexArray(self.vao)
        glEnableVertexAttribArray(0)
        glDrawArrays(GL_LINES, 0, self.vertexCount)
        glBindVertexArray(0) # unbind vao
        glUseProgram(0) # unbind program
        glDisable(GL_BLEND)

    def update(self, frameTime: float) -> None: pass

#endregion

#region ParticleRenderer

def _asMat(m):
    from openx.gfx.egin.egin_particle import Mat4
    if isinstance(m, Mat4): return m
    z = Mat4(); z.m = array(m, dtype=float64).reshape(4, 4); return z

def _quatFromMat(m):
    # System.Numerics Quaternion.CreateFromRotationMatrix, rows of Mat4.
    m11, m12, m13 = m[0, 0], m[0, 1], m[0, 2]
    m21, m22, m23 = m[1, 0], m[1, 1], m[1, 2]
    m31, m32, m33 = m[2, 0], m[2, 1], m[2, 2]
    trace = m11 + m22 + m33
    if trace > 0:
        s = math.sqrt(trace + 1.) * 2.
        return (m23 - m32) / s, (m31 - m13) / s, (m12 - m21) / s, 0.25 * s
    if m11 > m22 and m11 > m33:
        s = math.sqrt(1. + m11 - m22 - m33) * 2.
        return 0.25 * s, (m12 + m21) / s, (m13 + m31) / s, (m23 - m32) / s
    if m22 > m33:
        s = math.sqrt(1. + m22 - m11 - m33) * 2.
        return (m12 + m21) / s, 0.25 * s, (m23 + m32) / s, (m31 - m13) / s
    s = math.sqrt(1. + m33 - m11 - m22) * 2.
    return (m13 + m31) / s, (m23 + m32) / s, 0.25 * s, (m12 - m21) / s

def _quatInverse(x, y, z, w):
    n = x * x + y * y + z * z + w * w
    return -x / n, -y / n, -z / n, w / n

def _billboard(modelView):
    from openx.gfx.egin.egin_particle import Mat4
    m = _asMat(modelView).m
    xlen = math.sqrt(m[0, 0] ** 2 + m[0, 1] ** 2 + m[0, 2] ** 2)
    ylen = math.sqrt(m[1, 0] ** 2 + m[1, 1] ** 2 + m[1, 2] ** 2)
    zlen = math.sqrt(m[2, 0] ** 2 + m[2, 1] ** 2 + m[2, 2] ** 2)
    det = m[0, 0] * (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1]) - m[0, 1] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0]) + m[0, 2] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0])
    sx = -xlen if det < 0 else xlen
    rot = m.copy()
    if sx != 0: rot[0, 0:3] /= sx
    if ylen != 0: rot[1, 0:3] /= ylen
    if zlen != 0: rot[2, 0:3] /= zlen
    x, y, z, w = _quatInverse(*_quatFromMat(rot))
    return Mat4.fromQuaternion(x, y, z, w)

def _seqFrame(sequence, particle, animationRate: float):
    particleTime = particle.constantLifetime - particle.lifetime
    frame = particleTime * sequence.framesPerSecond * animationRate
    current = sequence.frames[int(math.floor(frame)) % len(sequence.frames)]
    image = current.images[0]
    sub = frame % 1.0
    offset = image.croppedMin * (1 - sub) + image.uncroppedMin * sub
    scale = (image.croppedMax - image.croppedMin) * (1 - sub) + (image.uncroppedMax - image.uncroppedMin) * sub
    return offset, scale

def _textureName(keyValues: dict):
    if 'm_hTexture' in keyValues: return keyValues['m_hTexture']
    if 'm_vecTexturesInput' in keyValues:
        textures = keyValues['m_vecTexturesInput']
        if textures is not None and len(textures) > 0: return textures[0]['m_hTexture']
    return None

def _kvFloat(keyValues: dict, name: str, default: float = 0.):
    return float(keyValues[name]) if name in keyValues else default
def _kvBool(keyValues: dict, name: str) -> bool:
    return bool(keyValues[name]) if name in keyValues else False
def _kvInt(keyValues: dict, name: str) -> int:
    return int(keyValues[name]) if name in keyValues else 0

# ParticleRenderer
class ParticleRenderer(EginRenderer):
    def __init__(self, gfx: list[IOpenGfx], source: ISource, obj: object):
        self.gfx = gfx
        self.source = source
        self.obj = obj

    class SpritesRenderer:
        vertexSize = 9
        def __init__(self, source: ISource, keyValues: dict, gfxModel):
            self.shader, self.shaderTag = asyncio.run(gfxModel.shaderManager.create(source, 'vrf.particle.sprite'))
            self.quadIndices = gfxModel.quadIndices
            self.vao = self._makeVao()
            self.textureSequences = None
            textureName = _textureName(keyValues)
            if textureName is not None:
                self.texture, self.textureTag = asyncio.run(gfxModel.textureManager.create(source, textureName))
                if isinstance(self.textureTag, dict): self.textureSequences = self.textureTag.get('sequences')
            else:
                self.texture = gfxModel.textureManager.default
                self.textureTag = None
            self.additive = _kvBool(keyValues, 'm_bAdditive')
            self.overbrightFactor = _kvFloat(keyValues, 'm_flOverbrightFactor', 1.)
            self.orientationType = _kvInt(keyValues, 'm_nOrientationType')
            self.animationRate = _kvFloat(keyValues, 'm_flAnimationRate', 0.1)
            self.rawVertices = None

        def _makeVao(self) -> int:
            glUseProgram(self.shader.program)
            vao = glGenVertexArrays(1); glBindVertexArray(vao)
            self.vertexBufferHandle = glGenBuffers(1)
            glBindBuffer(GL_ARRAY_BUFFER, self.vertexBufferHandle)
            stride = ctypes.sizeof(ctypes.c_float) * self.vertexSize
            position = self.shader.attribLocation('aVertexPosition')
            color = self.shader.attribLocation('aVertexColor')
            uv = self.shader.attribLocation('aTexCoords')
            glVertexAttribPointer(position, 3, GL_FLOAT, GL_FALSE, stride, ctypes.c_void_p(0))
            glVertexAttribPointer(color, 4, GL_FLOAT, GL_FALSE, stride, ctypes.c_void_p(ctypes.sizeof(ctypes.c_float) * 3))
            glVertexAttribPointer(uv, 2, GL_FLOAT, GL_FALSE, stride, ctypes.c_void_p(ctypes.sizeof(ctypes.c_float) * 7))
            glEnableVertexAttribArray(position)
            glEnableVertexAttribArray(color)
            glEnableVertexAttribArray(uv)
            glBindVertexArray(0)
            return vao

        def _ensure(self, count: int) -> None:
            num = count * self.vertexSize
            if self.rawVertices is None: self.rawVertices = array([0.] * num, dtype=float32)
            elif len(self.rawVertices) < num:
                nextSize = (count // 64 + 1) * 64 * self.vertexSize
                grown = array([0.] * nextSize, dtype=float32)
                grown[:len(self.rawVertices)] = self.rawVertices
                self.rawVertices = grown

        def _updateVertices(self, particleBag, modelViewMatrix) -> None:
            particles = particleBag.liveParticles()
            billboard = _billboard(modelViewMatrix)
            self._ensure(particleBag.count * 4)
            for i in range(particleBag.count):
                p = particles[i]
                model = p.getRotationMatrix() @ billboard @ p.getTransformationMatrix() if self.orientationType == 0 else p.getRotationMatrix() @ p.getTransformationMatrix()
                tl = model @ array([-1., -1., 0., 1.])
                bl = model @ array([-1., 1., 0., 1.])
                br = model @ array([1., 1., 0., 1.])
                tr = model @ array([1., -1., 0., 1.])
                quad = i * self.vertexSize * 4
                for n, corner in enumerate((tl, bl, br, tr)):
                    self.rawVertices[quad + self.vertexSize * n + 0] = corner[0]
                    self.rawVertices[quad + self.vertexSize * n + 1] = corner[1]
                    self.rawVertices[quad + self.vertexSize * n + 2] = corner[2]
                    self.rawVertices[quad + self.vertexSize * n + 3] = p.color.x
                    self.rawVertices[quad + self.vertexSize * n + 4] = p.color.y
                    self.rawVertices[quad + self.vertexSize * n + 5] = p.color.z
                    self.rawVertices[quad + self.vertexSize * n + 6] = p.alpha
                sequences = self.textureSequences
                if sequences is not None and len(sequences) > 0 and len(sequences[0].frames) > 0:
                    sequence = sequences[p.sequence % len(sequences)]
                    offset, scale = _seqFrame(sequence, p, self.animationRate)
                    coords = ((0, 1), (0, 0), (1, 0), (1, 1))
                    for n, (u, v) in enumerate(coords):
                        self.rawVertices[quad + self.vertexSize * n + 7] = offset.x + scale.x * u
                        self.rawVertices[quad + self.vertexSize * n + 8] = offset.y + scale.y * v
                else:
                    coords = ((0, 1), (0, 0), (1, 0), (1, 1))
                    for n, (u, v) in enumerate(coords):
                        self.rawVertices[quad + self.vertexSize * n + 7] = u
                        self.rawVertices[quad + self.vertexSize * n + 8] = v
            n = particleBag.count * self.vertexSize * 4
            data = self.rawVertices[:n]
            glBindBuffer(GL_ARRAY_BUFFER, self.vertexBufferHandle)
            glBufferData(GL_ARRAY_BUFFER, data.nbytes, data, GL_DYNAMIC_DRAW)

        def render(self, particleBag, viewProjectionMatrix, modelViewMatrix) -> None:
            if particleBag.count == 0: return
            self._updateVertices(particleBag, modelViewMatrix)
            glEnable(GL_BLEND)
            glUseProgram(self.shader.program)
            if self.additive: glBlendFunc(GL_SRC_ALPHA, GL_ONE)
            else: glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA)
            glBindVertexArray(self.vao)
            glEnableVertexAttribArray(0)
            glActiveTexture(GL_TEXTURE0)
            glBindTexture(GL_TEXTURE_2D, self.texture)
            glBindBuffer(GL_ARRAY_BUFFER, self.vertexBufferHandle)
            glUniform1i(self.shader.uniformLocation('uTexture'), 0)
            otk = _asMat(viewProjectionMatrix).m
            glUniformMatrix4fv(self.shader.uniformLocation('uProjectionViewMatrix'), 1, GL_FALSE, otk)
            glUniform1f(self.shader.uniformLocation('uOverbrightFactor'), self.overbrightFactor)
            glDisable(GL_CULL_FACE)
            glEnable(GL_DEPTH_TEST)
            glDepthMask(GL_FALSE)
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, self.quadIndices.glHandle)
            glDrawElements(GL_TRIANGLES, particleBag.count * 6, GL_UNSIGNED_SHORT, ctypes.c_void_p(0))
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0)
            glBindBuffer(GL_ARRAY_BUFFER, 0)
            glEnable(GL_CULL_FACE)
            glDisable(GL_DEPTH_TEST)
            glDepthMask(GL_TRUE)
            glBindVertexArray(0)
            glUseProgram(0)
            if self.additive: glBlendEquation(GL_FUNC_ADD)
            glDisable(GL_BLEND)

        def getSupportedRenderModes(self): return self.shader.renderModes
        def setRenderMode(self, renderMode: str) -> None:
            parameters = {}
            if renderMode is not None and renderMode in self.shader.renderModes: parameters[f'renderMode_{renderMode}'] = True

    class TrailsRenderer:
        def __init__(self, source: ISource, keyValues: dict, gfxModel):
            from openx.gfx.egin.egin_particle import V3, Mat4
            self._V3 = V3; self._Mat4 = Mat4
            self.shader, self.shaderTag = asyncio.run(gfxModel.shaderManager.create(source, 'vrf.particle.trail', {}))
            self.quadVao = self._setupQuadBuffer()
            self.textureSequences = None
            textureName = _textureName(keyValues)
            if textureName is not None:
                self.texture, self.textureTag = asyncio.run(gfxModel.textureManager.create(source, textureName))
                if isinstance(self.textureTag, dict): self.textureSequences = self.textureTag.get('sequences')
            else:
                self.texture = gfxModel.textureManager.default
                self.textureTag = None
            self.additive = _kvBool(keyValues, 'm_bAdditive')
            self.overbrightFactor = _kvFloat(keyValues, 'm_flOverbrightFactor', 1.)
            self.orientationType = _kvInt(keyValues, 'm_nOrientationType')
            self.animationRate = _kvFloat(keyValues, 'm_flAnimationRate', 0.1)
            self.finalTextureScaleU = _kvFloat(keyValues, 'm_flFinalTextureScaleU', 1.)
            self.finalTextureScaleV = _kvFloat(keyValues, 'm_flFinalTextureScaleV', 1.)
            self.maxLength = _kvFloat(keyValues, 'm_flMaxLength', 2000.)
            self.lengthFadeInTime = _kvFloat(keyValues, 'm_flLengthFadeInTime')

        def _setupQuadBuffer(self) -> int:
            glUseProgram(self.shader.program)
            vao = glGenVertexArrays(1); glBindVertexArray(vao)
            vbo = glGenBuffers(1); glBindBuffer(GL_ARRAY_BUFFER, vbo)
            vertices = array([-1., -1., 0., -1., 1., 0., 1., -1., 0., 1., 1., 0.], dtype=float32)
            glBufferData(GL_ARRAY_BUFFER, vertices.nbytes, vertices, GL_STATIC_DRAW)
            glEnableVertexAttribArray(0)
            position = self.shader.attribLocation('aVertexPosition')
            glVertexAttribPointer(position, 3, GL_FLOAT, GL_FALSE, 0, ctypes.c_void_p(0))
            glBindVertexArray(0)
            return vao

        def render(self, particleBag, viewProjectionMatrix, modelViewMatrix) -> None:
            V3 = self._V3; Mat4 = self._Mat4
            particles = particleBag.liveParticles()
            glEnable(GL_BLEND)
            glUseProgram(self.shader.program)
            if self.additive: glBlendFunc(GL_SRC_ALPHA, GL_ONE)
            else: glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA)
            glBindVertexArray(self.quadVao)
            glEnableVertexAttribArray(0)
            glActiveTexture(GL_TEXTURE0)
            glBindTexture(GL_TEXTURE_2D, self.texture)
            glUniform1i(self.shader.uniformLocation('uTexture'), 0)
            otk = _asMat(viewProjectionMatrix).m
            glUniformMatrix4fv(self.shader.uniformLocation('uProjectionViewMatrix'), 1, GL_FALSE, otk)
            glUniform1f(self.shader.uniformLocation('uOverbrightFactor'), self.overbrightFactor)
            modelLocation = self.shader.uniformLocation('uModelMatrix')
            colorLocation = self.shader.uniformLocation('uColor')
            alphaLocation = self.shader.uniformLocation('uAlpha')
            uvOffsetLocation = self.shader.uniformLocation('uUvOffset')
            uvScaleLocation = self.shader.uniformLocation('uUvScale')
            _billboard(modelViewMatrix)
            for p in particles:
                position = V3(p.position.x, p.position.y, p.position.z)
                previous = V3(p.positionPrevious.x, p.positionPrevious.y, p.positionPrevious.z)
                difference = previous - position
                direction = difference.normalize()
                midPoint = position + difference * 0.5
                length = min(self.maxLength, p.trailLength * difference.length() / 2.)
                t = 1. - p.lifetime / p.constantLifetime
                animated = length if t >= self.lengthFadeInTime else t * length / self.lengthFadeInTime
                scale = Mat4.scale(p.radius, animated, 1.)
                translation = Mat4.translation(0., animated, 0.)
                axis = V3.unitY().cross(direction).normalize()
                try: angle = math.acos(direction.y)
                except ValueError: angle = float('nan')
                rotation = Mat4.fromAxisAngle(axis, angle)
                model = scale @ translation @ rotation if self.orientationType == 0 else p.getTransformationMatrix()
                # count 0: the matrix is not uploaded
                glUniformMatrix4fv(modelLocation, 0, GL_FALSE, model.m)
                sequences = self.textureSequences
                if sequences is not None and len(sequences) > 0 and len(sequences[0].frames) > 0:
                    offset, uvScale = _seqFrame(sequences[0], p, self.animationRate)
                    glUniform2f(uvOffsetLocation, offset.x, offset.y)
                    glUniform2f(uvScaleLocation, uvScale.x * self.finalTextureScaleU, uvScale.y * self.finalTextureScaleV)
                else:
                    glUniform2f(uvOffsetLocation, 1., 1.)
                    glUniform2f(uvScaleLocation, self.finalTextureScaleU, self.finalTextureScaleV)
                glUniform3f(colorLocation, p.color.x, p.color.y, p.color.z)
                glUniform1f(alphaLocation, p.alpha * p.alphaAlternate)
                glDrawArrays(GL_TRIANGLE_STRIP, 0, 4)
                _ = midPoint
            glBindVertexArray(0)
            glUseProgram(0)
            if self.additive: glBlendEquation(GL_FUNC_ADD)
            glDisable(GL_BLEND)

        def getSupportedRenderModes(self): return self.shader.renderModes
        def setRenderMode(self, renderMode: str) -> None:
            parameters = {}
            if renderMode is not None and renderMode in self.shader.renderModes: parameters[f'renderMode_{renderMode}'] = True

#endregion

#region EngineRenderer

# EngineRenderer
class EngineRenderer(EginRenderer):
    def __init__(self, gfx: list[IOpenGfx], source: ISource, obj: object):
        self.gfx: list[IOpenGfx] = gfx
        self.source: ISource = source
        self.db: ICellDatabase = obj
        self.engine = None

    def dispose(self) -> None:
        super().dispose()
        if self.engine: self.engine.dispose()

    def start(self) -> None:
        self.engine = OpenGLOpenEngine(lambda queue: CellManager(self.db.query, queue, OpenGLCellBuilder(self.db.archive, self.db.query, self.gfx)), False)
        asyncio.run(self.engine.spawnPlayer(self.db))

    def update(self, deltaTime: float) -> None:
        if self.engine: asyncio.run(self.engine.update())

    def render(self, camera: Camera, passx: Renderer.Pass) -> None:
        self.engine.camera = camera
        super().render(camera, passx)

#endregion
