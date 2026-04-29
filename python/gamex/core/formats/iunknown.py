from __future__ import annotations
from openstk.core import Matrix4x4

# IUnknownBone
class IUnknownBone:
    name: str
    worldToBone: Matrix4x4
    boneToWorld: Matrix4x4
