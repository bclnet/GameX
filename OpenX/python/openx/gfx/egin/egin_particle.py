from __future__ import annotations
import math, random
from enum import IntEnum

class V3:
    def __init__(self, x: float = 0., y: float = 0., z: float = 0.):
        self.x = float(x); self.y = float(y); self.z = float(z)
    def __add__(self, o): return V3(self.x + o.x, self.y + o.y, self.z + o.z)
    def __sub__(self, o): return V3(self.x - o.x, self.y - o.y, self.z - o.z)
    def __mul__(self, o):
        if isinstance(o, V3): return V3(self.x * o.x, self.y * o.y, self.z * o.z)
        return V3(self.x * o, self.y * o, self.z * o)
    def __rmul__(self, o): return self * o
    def __truediv__(self, o): return V3(self.x / o, self.y / o, self.z / o)
    def length(self) -> float: return math.sqrt(self.x * self.x + self.y * self.y + self.z * self.z)
    def cross(self, o): return V3(self.y * o.z - self.z * o.y, self.z * o.x - self.x * o.z, self.x * o.y - self.y * o.x)
    def normalize(self): return self / self.length()
    @staticmethod
    def zero(): return V3()
    @staticmethod
    def one(): return V3(1., 1., 1.)
    @staticmethod
    def unitY(): return V3(0., 1., 0.)

class Mat4:
    def __init__(self, m=None):
        import numpy as np
        self.m = np.identity(4, dtype=np.float64) if m is None else m
    def __matmul__(self, other):
        if isinstance(other, Mat4):
            r = Mat4(); r.m = self.m @ other.m; return r
        import numpy as np
        return np.asarray(other, dtype=np.float64) @ self.m
    @staticmethod
    def scale(x, y=None, z=None):
        if y is None: y = z = x
        r = Mat4(); r.m[0, 0] = x; r.m[1, 1] = y; r.m[2, 2] = z; return r
    @staticmethod
    def translation(x, y, z):
        r = Mat4(); r.m[3, 0] = x; r.m[3, 1] = y; r.m[3, 2] = z; return r
    @staticmethod
    def rotationY(a):
        c, s = math.cos(a), math.sin(a); r = Mat4()
        r.m[0, 0] = c; r.m[0, 2] = -s; r.m[2, 0] = s; r.m[2, 2] = c; return r
    @staticmethod
    def rotationZ(a):
        c, s = math.cos(a), math.sin(a); r = Mat4()
        r.m[0, 0] = c; r.m[0, 1] = s; r.m[1, 0] = -s; r.m[1, 1] = c; return r
    @staticmethod
    def fromQuaternion(x, y, z, w):
        n = x * 2.; n2 = y * 2.; n3 = z * 2.
        n4 = x * n; n5 = y * n2; n6 = z * n3
        n7 = x * n2; n8 = x * n3; n9 = y * n3
        n10 = w * n; n11 = w * n2; n12 = w * n3
        r = Mat4()
        r.m[0, 0] = 1. - (n5 + n6); r.m[0, 1] = n7 + n12; r.m[0, 2] = n8 - n11
        r.m[1, 0] = n7 - n12; r.m[1, 1] = 1. - (n4 + n6); r.m[1, 2] = n9 + n10
        r.m[2, 0] = n8 + n11; r.m[2, 1] = n9 - n10; r.m[2, 2] = 1. - (n4 + n5)
        return r
    @staticmethod
    def fromAxisAngle(axis: V3, angle: float):
        x, y, z = axis.x, axis.y, axis.z
        c, s = math.cos(angle), math.sin(angle); t = 1. - c
        r = Mat4()
        r.m[0, 0] = t * x * x + c; r.m[0, 1] = t * x * y + s * z; r.m[0, 2] = t * x * z - s * y
        r.m[1, 0] = t * x * y - s * z; r.m[1, 1] = t * y * y + c; r.m[1, 2] = t * y * z + s * x
        r.m[2, 0] = t * x * z + s * y; r.m[2, 1] = t * y * z - s * x; r.m[2, 2] = t * z * z + c
        return r

def _get(d, key, default=None):
    return d[key] if key in d else default
def _getFloat(d, key, default=0.):
    return float(d[key]) if key in d else default
def _getInt64(d, key, default=0):
    return int(d[key]) if key in d else default
def _getBool(d, key, default=False):
    return bool(d[key]) if key in d else default
def _vec3(values, scale=1.):
    if not values: return V3.zero()
    return V3(float(values[0]) / scale, float(values[1]) / scale, float(values[2]) / scale)

class INumberProvider:
    def nextNumber(self) -> float: pass
    def nextInt(self) -> int: return int(self.nextNumber())

class LiteralNumberProvider(INumberProvider):
    def __init__(self, value: float): self._value = float(value)
    def nextNumber(self) -> float: return self._value

def getNumberProvider(keyValues: dict, name: str, default=None):
    if name not in keyValues: return default
    prop = keyValues[name]
    if isinstance(prop, dict):
        type_ = prop.get('m_nType')
        if type_ == 'PF_TYPE_LITERAL': return LiteralNumberProvider(float(prop['m_flLiteralValue']))
        raise Exception(f'Could not create number provider of type {type_}.')
    return LiteralNumberProvider(float(prop))

class IVectorProvider:
    def nextVector(self) -> V3: pass

class LiteralVectorProvider(IVectorProvider):
    def __init__(self, value):
        self._value = value if isinstance(value, V3) else _vec3(value)
    def nextVector(self) -> V3: return self._value

def getVectorProvider(keyValues: dict, name: str, default=None):
    if name not in keyValues: return default
    prop = keyValues[name]
    if isinstance(prop, dict) and 'm_nType' in prop:
        type_ = prop.get('m_nType')
        if type_ == 'PVEC_TYPE_LITERAL': return LiteralVectorProvider(prop.get('m_vLiteralValue'))
        raise Exception(f'Could not create vector provider of type {type_}.')
    return LiteralVectorProvider(keyValues.get(name))

class ParticleField(IntEnum):
    Position = 0
    PositionPrevious = 2
    Radius = 3
    Roll = 4
    Alpha = 7
    Yaw = 12
    AlphaAlternate = 16

class IParticleEmitter:
    def start(self, particleEmitCallback) -> None: pass
    def stop(self) -> None: pass
    def update(self, frameTime: float) -> None: pass
    @property
    def isFinished(self) -> bool: return False

class IParticleInitializer:
    def initialize(self, particle, particleSystemState): return particle

class IParticleOperator:
    def update(self, particles, frameTime: float, particleSystemState) -> None: pass

class IParticleRenderer:
    def render(self, particles, viewProjectionMatrix, modelViewMatrix) -> None: pass
    def setRenderMode(self, renderMode: str) -> None: pass
    def getSupportedRenderModes(self) -> list: return []

class Particle:
    def __init__(self, baseProperties: dict = None):
        baseProperties = baseProperties or {}
        self.particleCount = 0
        self.alpha = 1.
        self.alphaAlternate = 1.
        self.position = V3.zero()
        self.positionPrevious = V3.zero()
        self.rotation = V3.zero()
        self.rotationSpeed = V3.zero()
        self.velocity = V3.zero()
        self.constantRadius = _getFloat(baseProperties, 'm_flConstantRadius', 5.)
        self.constantAlpha = 1.
        if 'm_ConstantColor' in baseProperties: self.constantColor = _vec3(baseProperties['m_ConstantColor'], 255.)
        else: self.constantColor = V3.one()
        self.constantLifetime = _getFloat(baseProperties, 'm_flConstantLifespan', 1.)
        self.trailLength = 1.
        self.sequence = 0
        self.color = V3(self.constantColor.x, self.constantColor.y, self.constantColor.z)
        self.lifetime = self.constantLifetime
        self.radius = self.constantRadius
    def getTransformationMatrix(self) -> Mat4:
        return Mat4.scale(self.radius) @ Mat4.translation(self.position.x, self.position.y, self.position.z)
    def getRotationMatrix(self) -> Mat4:
        return Mat4.rotationZ(self.rotation.z) @ Mat4.rotationY(self.rotation.y)
    def getScalar(self, field: ParticleField) -> float:
        if field == ParticleField.Alpha: return self.alpha
        if field == ParticleField.AlphaAlternate: return self.alphaAlternate
        if field == ParticleField.Radius: return self.radius
        return 0.
    def getVector(self, field: ParticleField) -> V3:
        if field == ParticleField.Position: return self.position
        if field == ParticleField.PositionPrevious: return self.positionPrevious
        return V3.zero()

class ParticleBag:
    def __init__(self, initialCapacity: int, growable: bool):
        self._isGrowable = growable
        self._particles = [Particle() for _ in range(initialCapacity)]
        self.count = 0
    def liveParticles(self) -> list: return self._particles[:self.count]
    def add(self) -> int:
        if self.count < len(self._particles):
            self.count += 1; return self.count - 1
        if self._isGrowable:
            newSize = len(self._particles) * 2 if len(self._particles) < 1024 else len(self._particles) + 1024
            self._particles.extend(Particle() for _ in range(newSize - len(self._particles)))
            self.count += 1; return self.count - 1
        return -1
    def pruneExpired(self) -> None:
        i = 0
        while i < self.count:
            if self._particles[i].lifetime <= 0:
                self._particles[i] = self._particles[self.count - 1]
                self.count -= 1
            else: i += 1
    def clear(self) -> None: self.count = 0

class ParticleSystemRenderState:
    def __init__(self):
        self.lifetime = 0.
        self._controlPoints = {}
    def getControlPoint(self, cp: int) -> V3: return self._controlPoints.get(cp, V3.zero())
    def setControlPoint(self, cp: int, value: V3):
        self._controlPoints[cp] = value
        return self

class ContinuousEmitter(IParticleEmitter):
    def __init__(self, baseProperties: dict, keyValues: dict):
        self._baseProperties = baseProperties
        self._emissionDuration = getNumberProvider(keyValues, 'm_flEmissionDuration') or LiteralNumberProvider(0)
        self._startTime = getNumberProvider(keyValues, 'm_flStartTime') or LiteralNumberProvider(0)
        self._emitInterval = 0.01
        if 'm_flEmitRate' in keyValues:
            self._emitRate = getNumberProvider(keyValues, 'm_flEmitRate')
            self._emitInterval = 1. / float(self._emitRate.nextNumber())
        else: self._emitRate = LiteralNumberProvider(100)
        self._particleEmitCallback = None
        self._time = 0.
        self._lastEmissionTime = 0.
        self._isFinished = False
    @property
    def isFinished(self) -> bool: return self._isFinished
    def start(self, particleEmitCallback) -> None:
        self._particleEmitCallback = particleEmitCallback
        self._time = 0.
        self._lastEmissionTime = 0.
        self._isFinished = False
    def stop(self) -> None: self._isFinished = True
    def update(self, frameTime: float) -> None:
        if self._isFinished: return
        self._time += frameTime
        nextStart = self._startTime.nextNumber()
        nextDuration = self._emissionDuration.nextNumber()
        if self._time >= nextStart and (nextDuration == 0. or self._time <= nextStart + nextDuration):
            numToEmit = int(math.floor((self._time - self._lastEmissionTime) / self._emitInterval))
            emitCount = min(5 * self._emitRate.nextNumber(), numToEmit)
            for _ in range(int(emitCount)): self._particleEmitCallback()
            self._lastEmissionTime += numToEmit * self._emitInterval

class InstantaneousEmitter(IParticleEmitter):
    def __init__(self, baseProperties: dict, keyValues: dict):
        self._baseProperties = baseProperties
        self._emitCount = getNumberProvider(keyValues, 'm_nParticlesToEmit')
        self._startTime = _getFloat(keyValues, 'm_flStartTime')
        self._time = 0.
        self._particleEmitCallback = None
        self._isFinished = False
    @property
    def isFinished(self) -> bool: return self._isFinished
    def start(self, particleEmitCallback) -> None:
        self._particleEmitCallback = particleEmitCallback
        self._isFinished = False
        self._time = 0.
    def stop(self) -> None: pass
    def update(self, frameTime: float) -> None:
        self._time += frameTime
        if not self._isFinished and self._time >= self._startTime:
            for _ in range(self._emitCount.nextInt()): self._particleEmitCallback()
            self._isFinished = True

class CreateWithinSphere(IParticleInitializer):
    def __init__(self, keyValues: dict):
        self._random = random.Random()
        self._radiusMin = _getFloat(keyValues, 'm_fRadiusMin')
        self._radiusMax = _getFloat(keyValues, 'm_fRadiusMax')
        self._speedMin = _getFloat(keyValues, 'm_fSpeedMin')
        self._speedMax = _getFloat(keyValues, 'm_fSpeedMax')
        self._speedMinLocal = _vec3(keyValues.get('m_LocalCoordinateSystemSpeedMin'))
        self._speedMaxLocal = _vec3(keyValues.get('m_LocalCoordinateSystemSpeedMax'))
    def initialize(self, particle, state):
        rv = V3(self._random.random() * 2 - 1, self._random.random() * 2 - 1, self._random.random() * 2 - 1)
        direction = rv / rv.length()
        distance = self._radiusMin + (self._random.random() * (self._radiusMax - self._radiusMin))
        speed = self._speedMin + (self._random.random() * (self._speedMax - self._speedMin))
        local = self._speedMinLocal + (self._random.random() * (self._speedMaxLocal - self._speedMinLocal))
        particle.position = particle.position + direction * distance
        particle.velocity = (direction * speed) + local
        return particle

class InitialVelocityNoise(IParticleInitializer):
    def __init__(self, keyValues: dict):
        self._outputMin = getVectorProvider(keyValues, 'm_vecOutputMin') or LiteralVectorProvider(V3.zero())
        self._outputMax = getVectorProvider(keyValues, 'm_vecOutputMax') or LiteralVectorProvider(V3.one())
        self._noiseScale = getNumberProvider(keyValues, 'm_flNoiseScale') or LiteralNumberProvider(1.)
    def initialize(self, particle, state):
        noise = float(self._noiseScale.nextNumber())
        r = V3(_simplex1d(state.lifetime * noise), _simplex1d(state.lifetime * noise + 101723), _simplex1d(state.lifetime * noise + 555557))
        mn = self._outputMin.nextVector(); mx = self._outputMax.nextVector()
        particle.velocity = mn + (r * (mx - mn))
        return particle

def _pseudoRandom(t: float) -> float: return ((1013904223517 * t) % 1664525) / 1664525.
def _cosine(start, end, mu):
    mu2 = (1 - math.cos(mu * math.pi)) / 2.
    return (start * (1 - mu2)) + (end * mu2)
def _simplex1d(t: float) -> float:
    previous = _pseudoRandom(math.floor(t))
    nxt = _pseudoRandom(math.ceil(t))
    return _cosine(previous, nxt, t % 1.)

class OffsetVectorToVector(IParticleInitializer):
    def __init__(self, keyValues: dict):
        self._random = random.Random()
        self._inputField = ParticleField(_getInt64(keyValues, 'm_nFieldInput'))
        self._outputField = ParticleField(_getInt64(keyValues, 'm_nFieldOutput'))
        self._offsetMin = _vec3(keyValues['m_vecOutputMin']) if 'm_vecOutputMin' in keyValues else V3.zero()
        self._offsetMax = _vec3(keyValues['m_vecOutputMax']) if 'm_vecOutputMax' in keyValues else V3.one()
    def initialize(self, particle, state):
        inp = particle.getVector(self._inputField)
        offset = V3(
            self._offsetMin.x + self._random.random() * (self._offsetMax.x - self._offsetMin.x),
            self._offsetMin.y + self._random.random() * (self._offsetMax.y - self._offsetMin.y),
            self._offsetMin.z + self._random.random() * (self._offsetMax.z - self._offsetMin.z))
        if self._outputField == ParticleField.Position: particle.position = particle.position + inp + offset
        elif self._outputField == ParticleField.PositionPrevious: particle.positionPrevious = inp + offset
        return particle

class PositionOffset(IParticleInitializer):
    def __init__(self, keyValues: dict):
        self._random = random.Random()
        self._offsetMin = _vec3(keyValues['m_OffsetMin']) if 'm_OffsetMin' in keyValues else V3.zero()
        self._offsetMax = _vec3(keyValues['m_OffsetMax']) if 'm_OffsetMax' in keyValues else V3.zero()
    def initialize(self, particle, state):
        distance = self._offsetMax - self._offsetMin
        offset = self._offsetMin + (distance * V3(self._random.random(), self._random.random(), self._random.random()))
        particle.position = particle.position + offset
        return particle

class RandomAlpha(IParticleInitializer):
    def __init__(self, keyValues: dict):
        self._random = random.Random()
        self._alphaMin = int(_getInt64(keyValues, 'm_nAlphaMin', 255))
        self._alphaMax = int(_getInt64(keyValues, 'm_nAlphaMax', 255))
        if self._alphaMin > self._alphaMax: self._alphaMax, self._alphaMin = self._alphaMin, self._alphaMax
    def initialize(self, particle, state):
        alpha = self._random.randrange(self._alphaMin, self._alphaMax) / 255.
        particle.constantAlpha = alpha
        particle.alpha = alpha
        return particle

class RandomColor(IParticleInitializer):
    def __init__(self, keyValues: dict):
        self._random = random.Random()
        self._colorMin = _vec3(keyValues['m_ColorMin'], 255.) if 'm_ColorMin' in keyValues else V3.one()
        self._colorMax = _vec3(keyValues['m_ColorMax'], 255.) if 'm_ColorMax' in keyValues else V3.one()
    def initialize(self, particle, state):
        t = self._random.random()
        particle.constantColor = self._colorMin + (t * (self._colorMax - self._colorMin))
        particle.color = V3(particle.constantColor.x, particle.constantColor.y, particle.constantColor.z)
        return particle

class RandomLifeTime(IParticleInitializer):
    def __init__(self, keyValues: dict):
        self._random = random.Random()
        self._lifetimeMin = _getFloat(keyValues, 'm_fLifetimeMin')
        self._lifetimeMax = _getFloat(keyValues, 'm_fLifetimeMax')
    def initialize(self, particle, state):
        lifetime = self._lifetimeMin + ((self._lifetimeMax - self._lifetimeMin) * self._random.random())
        particle.constantLifetime = lifetime
        particle.lifetime = lifetime
        return particle

class RandomRadius(IParticleInitializer):
    def __init__(self, keyValues: dict):
        self._random = random.Random()
        self._radiusMin = _getFloat(keyValues, 'm_flRadiusMin')
        self._radiusMax = _getFloat(keyValues, 'm_flRadiusMax')
    def initialize(self, particle, state):
        particle.constantRadius = self._radiusMin + (self._random.random() * (self._radiusMax - self._radiusMin))
        particle.radius = particle.constantRadius
        return particle

class RandomRotation(IParticleInitializer):
    piOver180 = math.pi / 180.
    def __init__(self, keyValues: dict):
        self._random = random.Random()
        self._degreesMin = _getFloat(keyValues, 'm_flDegreesMin')
        self._degreesMax = _getFloat(keyValues, 'm_flDegreesMax', 360.)
        self._degreesOffset = _getFloat(keyValues, 'm_flDegrees')
        self._fieldOutput = _getInt64(keyValues, 'm_nFieldOutput', 4)
        self._randomlyFlipDirection = _getBool(keyValues, 'm_bRandomlyFlipDirection')
    def initialize(self, particle, state):
        degrees = self._degreesOffset + self._degreesMin + (self._random.random() * (self._degreesMax - self._degreesMin))
        if self._randomlyFlipDirection and self._random.random() > 0.5: degrees *= -1
        if self._fieldOutput == 4: particle.rotation = V3(particle.rotation.x, particle.rotation.y, degrees * RandomRotation.piOver180)
        elif self._fieldOutput == 12: particle.rotation = V3(particle.rotation.x, degrees * RandomRotation.piOver180, particle.rotation.z)
        return particle

class RandomRotationSpeed(IParticleInitializer):
    piOver180 = math.pi / 180.
    def __init__(self, keyValues: dict):
        self._random = random.Random()
        self._fieldOutput = ParticleField(_getInt64(keyValues, 'm_nFieldOutput', int(ParticleField.Roll)))
        self._randomlyFlipDirection = _getBool(keyValues, 'm_bRandomlyFlipDirection', True)
        self._degrees = _getFloat(keyValues, 'm_flDegrees')
        self._degreesMin = _getFloat(keyValues, 'm_flDegreesMin')
        self._degreesMax = _getFloat(keyValues, 'm_flDegreesMax', 360.)
    def initialize(self, particle, state):
        value = RandomRotationSpeed.piOver180 * (self._degrees + self._degreesMin + (self._random.random() * (self._degreesMax - self._degreesMin)))
        if self._randomlyFlipDirection and self._random.random() > 0.5: value *= -1
        if self._fieldOutput == ParticleField.Yaw: particle.rotationSpeed = V3(value, 0, 0)
        elif self._fieldOutput == ParticleField.Roll: particle.rotationSpeed = V3(0, 0, value)
        return particle

class RandomSequence(IParticleInitializer):
    def __init__(self, keyValues: dict):
        self._random = random.Random()
        self._sequenceMin = int(_getInt64(keyValues, 'm_nSequenceMin'))
        self._sequenceMax = int(_getInt64(keyValues, 'm_nSequenceMax'))
        self._shuffle = _getBool(keyValues, 'm_bShuffle')
        self._counter = 0
    def initialize(self, particle, state):
        if self._shuffle: particle.sequence = self._random.randrange(self._sequenceMin, self._sequenceMax + 1)
        else: particle.sequence = self._sequenceMin + ((self._counter % (self._sequenceMax - self._sequenceMin)) if self._sequenceMax > self._sequenceMin else 0); self._counter += 0 if not (self._sequenceMax > self._sequenceMin) else 1
        if not self._shuffle and self._sequenceMax > self._sequenceMin: pass
        return particle

class RandomTrailLength(IParticleInitializer):
    def __init__(self, keyValues: dict):
        self._random = random.Random()
        self._minLength = _getFloat(keyValues, 'm_flMinLength', 0.1)
        self._maxLength = _getFloat(keyValues, 'm_flMaxLength', 0.1)
    def initialize(self, particle, state):
        particle.trailLength = self._minLength + (self._random.random() * (self._maxLength - self._minLength))
        return particle

class RemapParticleCountToScalar(IParticleInitializer):
    def __init__(self, keyValues: dict):
        self._fieldOutput = _getInt64(keyValues, 'm_nFieldOutput', 3)
        self._inputMin = _getInt64(keyValues, 'm_nInputMin')
        self._inputMax = _getInt64(keyValues, 'm_nInputMax', 10)
        self._outputMin = _getFloat(keyValues, 'm_flOutputMin')
        self._outputMax = _getFloat(keyValues, 'm_flOutputMax', 1.)
        self._scaleInitialRange = _getBool(keyValues, 'm_bScaleInitialRange')
    def initialize(self, particle, state):
        particleCount = min(self._inputMax, max(self._inputMin, particle.particleCount))
        t = (particleCount - self._inputMin) / float(self._inputMax - self._inputMin)
        output = self._outputMin + (t * (self._outputMax - self._outputMin))
        if self._fieldOutput == 3: particle.radius = particle.radius * output if self._scaleInitialRange else output
        return particle

class RingWave(IParticleInitializer):
    def __init__(self, keyValues: dict):
        self._random = random.Random()
        self._evenDistribution = _getBool(keyValues, 'm_bEvenDistribution')
        self._initialRadius = _getFloat(keyValues, 'm_flInitialRadius')
        self._thickness = _getFloat(keyValues, 'm_flThickness')
        self._particlesPerOrbit = _getFloat(keyValues, 'm_flParticlesPerOrbit', -1.)
        self._orbitCount = 0.
    def initialize(self, particle, state):
        radius = self._initialRadius + (self._random.random() * self._thickness)
        angle = self._nextAngle()
        particle.position = particle.position + radius * V3(math.cos(angle), math.sin(angle), 0)
        return particle
    def _nextAngle(self) -> float:
        if self._evenDistribution:
            offset = self._orbitCount / self._particlesPerOrbit
            self._orbitCount = (self._orbitCount + 1) % self._particlesPerOrbit
            return offset * 2 * math.pi
        return 2 * math.pi * self._random.random()

class Decay(IParticleOperator):
    def __init__(self, keyValues: dict = None): pass
    def update(self, particles, frameTime, state) -> None:
        for p in particles: p.lifetime -= frameTime

class BasicMovement(IParticleOperator):
    def __init__(self, keyValues: dict):
        self._gravity = _vec3(keyValues['m_Gravity']) if 'm_Gravity' in keyValues else V3.zero()
        self._drag = _getFloat(keyValues, 'm_fDrag')
    def update(self, particles, frameTime, state) -> None:
        acceleration = self._gravity * frameTime
        for p in particles:
            p.velocity = p.velocity + acceleration
            p.velocity = p.velocity * (1 - (self._drag * 30. * frameTime))
            p.position = p.position + p.velocity * frameTime

class ColorInterpolate(IParticleOperator):
    def __init__(self, keyValues: dict):
        self._colorFade = _vec3(keyValues['m_ColorFade'], 255.) if 'm_ColorFade' in keyValues else V3.one()
        self._fadeStartTime = _getFloat(keyValues, 'm_flFadeStartTime')
        self._fadeEndTime = _getFloat(keyValues, 'm_flFadeEndTime', 1.)
    def update(self, particles, frameTime, state) -> None:
        for p in particles:
            time = 1 - (p.lifetime / p.constantLifetime)
            if self._fadeStartTime <= time <= self._fadeEndTime:
                t = (time - self._fadeStartTime) / (self._fadeEndTime - self._fadeStartTime)
                p.color = ((1 - t) * p.constantColor) + (t * self._colorFade)

class InterpolateRadius(IParticleOperator):
    def __init__(self, keyValues: dict):
        self._startTime = _getFloat(keyValues, 'm_flStartTime')
        self._endTime = _getFloat(keyValues, 'm_flEndTime', 1.)
        self._startScale = _getFloat(keyValues, 'm_flStartScale', 1.)
        self._endScale = _getFloat(keyValues, 'm_flEndScale', 1.)
    def update(self, particles, frameTime, state) -> None:
        for p in particles:
            time = 1 - (p.lifetime / p.constantLifetime)
            if self._startTime <= time <= self._endTime:
                t = (time - self._startTime) / (self._endTime - self._startTime)
                p.radius = p.constantRadius * ((self._startScale * (1 - t)) + (self._endScale * t))

class FadeAndKill(IParticleOperator):
    def __init__(self, keyValues: dict):
        self._startFadeInTime = _getFloat(keyValues, 'm_flStartFadeInTime')
        self._endFadeInTime = _getFloat(keyValues, 'm_flEndFadeInTime', .5)
        self._startFadeOutTime = _getFloat(keyValues, 'm_flStartFadeOutTime', .5)
        self._endFadeOutTime = _getFloat(keyValues, 'm_flEndFadeOutTime', 1.)
        self._startAlpha = _getFloat(keyValues, 'm_flStartAlpha', 1.)
        self._endAlpha = _getFloat(keyValues, 'm_flEndAlpha')
    def update(self, particles, frameTime, state) -> None:
        for p in particles:
            time = 1 - (p.lifetime / p.constantLifetime)
            if self._startFadeInTime <= time <= self._endFadeInTime:
                t = (time - self._startFadeInTime) / (self._endFadeInTime - self._startFadeInTime)
                p.alpha = ((1 - t) * self._startAlpha) + (t * p.constantAlpha)
            if self._startFadeOutTime <= time <= self._endFadeOutTime:
                t = (time - self._startFadeOutTime) / (self._endFadeOutTime - self._startFadeOutTime)
                p.alpha = ((1 - t) * p.constantAlpha) + (t * self._endAlpha)
            p.lifetime -= frameTime

class FadeInSimple(IParticleOperator):
    def __init__(self, keyValues: dict):
        self._fadeInTime = _getFloat(keyValues, 'm_flFadeInTime', .25)
    def update(self, particles, frameTime, state) -> None:
        for p in particles:
            time = 1 - (p.lifetime / p.constantLifetime)
            if time <= self._fadeInTime: p.alpha = (time / self._fadeInTime) * p.constantAlpha

class FadeOutSimple(IParticleOperator):
    def __init__(self, keyValues: dict):
        self._fadeOutTime = _getFloat(keyValues, 'm_flFadeOutTime', .25)
    def update(self, particles, frameTime, state) -> None:
        for p in particles:
            timeLeft = p.lifetime / p.constantLifetime
            if timeLeft <= self._fadeOutTime: p.alpha = (timeLeft / self._fadeOutTime) * p.constantAlpha

class OscillateScalar(IParticleOperator):
    def __init__(self, keyValues: dict):
        self._random = random.Random()
        self._outputField = ParticleField(_getInt64(keyValues, 'm_nField', int(ParticleField.Alpha)))
        self._rateMin = _getFloat(keyValues, 'm_RateMin')
        self._rateMax = _getFloat(keyValues, 'm_RateMax')
        self._frequencyMin = _getFloat(keyValues, 'm_FrequencyMin', 1.)
        self._frequencyMax = _getFloat(keyValues, 'm_FrequencyMax', 1.)
        self._oscillationMultiplier = _getFloat(keyValues, 'm_flOscMult', 2.)
        self._oscillationOffset = _getFloat(keyValues, 'm_flOscAdd', .5)
        self._proportional = _getBool(keyValues, 'm_bProportionalOp', True)
        self._particleRates = {}
        self._particleFrequencies = {}
    def update(self, particles, frameTime, state) -> None:
        for p in particles:
            rate = self._rate(p.particleCount)
            frequency = self._frequency(p.particleCount)
            t = 1 - (p.lifetime / p.constantLifetime) if self._proportional else p.lifetime
            delta = math.sin(((t * frequency * self._oscillationMultiplier) + self._oscillationOffset) * math.pi)
            if self._outputField == ParticleField.Radius: p.radius += delta * rate * frameTime
            elif self._outputField == ParticleField.Alpha: p.alpha += delta * rate * frameTime
            elif self._outputField == ParticleField.AlphaAlternate: p.alphaAlternate += delta * rate * frameTime
    def _rate(self, particleId: int) -> float:
        if particleId in self._particleRates: return self._particleRates[particleId]
        value = self._rateMin + (self._random.random() * (self._rateMax - self._rateMin))
        self._particleRates[particleId] = value
        return value
    def _frequency(self, particleId: int) -> float:
        if particleId in self._particleFrequencies: return self._particleFrequencies[particleId]
        value = self._frequencyMin + (self._random.random() * (self._frequencyMax - self._frequencyMin))
        self._particleFrequencies[particleId] = value
        return value

class SpinUpdate(IParticleOperator):
    def __init__(self, keyValues: dict = None): pass
    def update(self, particles, frameTime, state) -> None:
        for p in particles: p.rotation = p.rotation + p.rotationSpeed * frameTime
