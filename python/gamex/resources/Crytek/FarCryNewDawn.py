from zipfile import ZipFile
from importlib import resources
from .FarCryX import hashFilelist64

f = resources.files().joinpath('FarCryNewDawn.zip').open('rb')
arc: ZipFile = ZipFile(f, 'r')
files: dict[str, object] = { s.filename:s for s in arc.infolist() }
fileHashes: dict[str, dict[int, str]] = {}
@staticmethod
def getFileHashes(path: str) -> dict[int, str]:
    if path in fileHashes: return fileHashes[path]
    fileHashes[path] = hashFilelist64(arc, files[path]) if path in files else []
    return fileHashes[path]
