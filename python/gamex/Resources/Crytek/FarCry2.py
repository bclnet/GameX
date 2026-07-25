from zipfile import ZipFile
from importlib import resources
from .FarCryX import hashFilelist32, hashObj32

f = resources.files().joinpath('FarCry2.zip').open('rb')
arc: ZipFile = ZipFile(f, 'r')
files: dict[str, object] = { s.filename:s for s in arc.infolist() }
fileHashes: dict[str, dict[int, str]] = {}
objHashes: dict[str, dict[int, str]] = {}
@staticmethod
def getFileHashes(path: str) -> dict[int, str]:
    if path in fileHashes: return fileHashes[path]
    fileHashes[path] = hashFilelist32(arc, files[path]) if path in files else []
    return fileHashes[path]
@staticmethod
def getObjHashes(path: str) -> dict[int, str]:
    if path in objHashes: return objHashes[path]
    objHashes[path] = hashObj32(arc, files[path]) if path in files else []
    return objHashes[path]
