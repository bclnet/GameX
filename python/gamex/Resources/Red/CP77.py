import os, pathlib
from zipfile import ZipFile
from io import BytesIO
from importlib import resources

with resources.files().joinpath('C977.zip').open('rb') as f:
    arc: ZipFile = ZipFile(f, 'r')
hashEntries: dict[str, object] = { x.filename:x for x in arc.infolist() }

hashLookup: dict[str, dict[int, str]] = {}
@staticmethod
def getHashLookup(path: str) -> dict[int, str]:
    pass
