from zipfile import ZipFile
from importlib import resources
from .FarCryX import hashFilelist64

f = resources.files().joinpath('FarCryPrimal.zip').open('rb')
arc: ZipFile = ZipFile(f, 'r')
hashFiles: dict[str, object] = { s.filename:s for s in arc.infolist() }
hashes: dict[str, dict[int, str]] = {}
@staticmethod
def getHashes(path: str) -> dict[int, str]:
    if path in hashes: return hashes[path]
    hashes[path] = hashFilelist64(arc, hashFiles[path]) if path in hashFiles else []
    return hashes[path]
