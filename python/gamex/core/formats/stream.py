from __future__ import annotations
import os
from openx.core import BinaryReader, Writer
from ..binary import ArcBinary, BinaryArchive, BinaryState
from ..meta import FileSource

def _ascii(data: bytes) -> str:
    # Encoding.ASCII uses '?' for a byte above 127.
    return ''.join(chr(b) if b < 128 else '?' for b in data) if data is not None else None

# PakBinaryCanStream
class PakBinaryCanStream(ArcBinary):
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        if tag is None: return
        if tag == 'Set':
            files = source.files = []
            data = r.readToEnd()
            lines = _ascii(data).split('\n') if data is not None else None
            if lines is not None and len(lines) == 0: return
            startIndex = len(os.path.dirname(lines[0].rstrip().replace('\\', '/'))) + 1
            for line in lines:
                if len(line) >= startIndex and (path := line[startIndex:].rstrip().replace('\\', '/')) != '.set':
                    files.append(FileSource(path = path))
            return
        if tag == 'Meta':
            source.process()
            data = r.readToEnd()
            lines = _ascii(data).split('\n') if data is not None else None
            if lines is not None and len(lines) == 0: return
            state = -1
            paramsx = source.params
            filesByPath = source.filesByPath
            for line in lines:
                path = line.rstrip().replace('\\', '/')
                if state == -1:
                    if path == 'Params:': state = 0
                    elif path == 'AllCompressed':
                        for file in source.files: file.compressed = 1
                    elif path == 'Compressed:': state = 1
                    elif path == 'Crypted:': state = 2
                else:
                    if line == '': state = -1; continue
                    files = filesByPath[line]
                    if state == 0:
                        args = line.split(':', 1)
                        paramsx[args[0]] = args[1]
                        continue
                    if state == 1:
                        if files is not None: files[0].compressed = 1
                        continue
                    if state == 2:
                        if files is not None: files[0].flags = 1
                        continue
            return
        if tag == 'Raw':
            filesRawSet = source.filesRawSet = []
            data = r.readToEnd()
            lines = _ascii(data).split('\n') if data is not None else None
            if lines is not None and len(lines) == 0: return
            for line in lines: filesRawSet.append(line.rstrip().replace('\\', '/'))
            return
        raise ValueError(str(tag))

    def write(self, source: BinaryArchive, w: Writer, tag: object = None) -> None:
        if tag is None: return
        if tag == 'Set':
            pathAsBytes = f'C:/{source.name}/'.encode('ascii', 'replace')
            w.f.write(pathAsBytes)
            w.f.write(b'.set')
            w.writeByte(ord('\n'))
            w.f.flush()
            for file in source.files:
                w.f.write(pathAsBytes)
                w.f.write(file.path.encode('ascii', 'replace'))
                w.writeByte(ord('\n'))
                w.f.flush()
            return
        if tag == 'Meta':
            params = source.params
            if len(params) > 0:
                w.f.write(b'Params:\n')
                for key, value in params.items():
                    w.f.write(f'{key}:{value}'.encode('ascii', 'replace'))
                    w.writeByte(ord('\n'))
                    w.f.flush()
                w.writeByte(ord('\n'))
                w.f.flush()
            files = source.files
            numCompressed = sum(1 for x in files if x.compressed)
            if len(files) == numCompressed: w.f.write(b'AllCompressed\n')
            elif numCompressed > 0:
                w.f.write(b'Compressed:\n')
                for file in files:
                    if not file.compressed: continue
                    w.f.write(file.path.encode('ascii', 'replace'))
                    w.writeByte(ord('\n'))
                    w.f.flush()
                w.writeByte(ord('\n'))
                w.f.flush()
            if any(x.flags for x in files):
                w.f.write(b'Crypted:\n')
                for file in files:
                    if not file.flags: continue
                    w.f.write(file.path.encode('ascii', 'replace'))
                    w.writeByte(ord('\n'))
                    w.f.flush()
                w.writeByte(ord('\n'))
                w.f.flush()
            return
        if tag == 'Raw':
            if source.filesRawSet is None: raise ValueError('filesRawSet')
            for file in source.filesRawSet:
                w.f.write(file.encode('ascii', 'replace'))
                w.writeByte(ord('\n'))
                w.f.flush()
            return
        raise ValueError(str(tag))

# StreamArchive
class StreamArchive(BinaryArchive):
    def __init__(self, parent, state: BinaryState, factory: callable = None, address: str = None):
        super().__init__(parent, state, PakBinaryCanStream())
        self.useReader = False
        self.host = None
        if factory is not None:
            if address is not None: self.host = factory(address, state.path)
        elif isinstance(parent, BinaryArchive):
            self.files = parent.files

    async def read(self, tag: object = None) -> None:
        if self.host is not None:
            files = self.files = []
            set_ = await self.host.getSetAsync()
            if set_ is None: raise Exception('.set not found')
            for item in set_: files.append(FileSource(path = item))
            return
        path = self.binPath
        if not path or not os.path.isdir(path): return
        setPath = os.path.join(path, '.set')
        if os.path.isfile(setPath):
            with open(setPath, 'rb') as f: self.arcBinary.read(self, BinaryReader(f, leaveOpen=True), 'Set')
        metaPath = os.path.join(path, '.meta')
        # C# opens setPath for the .meta read.
        if os.path.isfile(metaPath):
            with open(setPath, 'rb') as f: self.arcBinary.read(self, BinaryReader(f, leaveOpen=True), 'Meta')
        rawPath = os.path.join(path, '.raw')
        if os.path.isfile(rawPath):
            with open(rawPath, 'rb') as f: self.arcBinary.read(self, BinaryReader(f, leaveOpen=True), 'Raw')

    async def write(self, tag: object = None) -> None:
        if self.host is not None: raise Exception('NotSupported')
        path = self.binPath
        if path and not os.path.isdir(path): os.makedirs(path)
        setPath = os.path.join(path, '.set')
        with open(setPath, 'wb') as f: self.arcBinary.write(self, Writer(f), 'Set')
        metaPath = os.path.join(path, '.meta')
        with open(metaPath, 'wb') as f: self.arcBinary.write(self, Writer(f), 'Meta')
        rawPath = os.path.join(path, '.raw')
        if self.filesRawSet is not None and len(self.filesRawSet) > 0:
            with open(rawPath, 'wb') as f: self.arcBinary.write(self, Writer(f), 'Raw')
        elif os.path.isfile(rawPath): os.remove(rawPath)

    async def readData(self, file: FileSource, option: object = None):
        path = file.path
        if self.host is not None: return await self.host.getFileAsync(path)
        path = os.path.join(self.binPath, path)
        return open(path, 'rb') if os.path.isfile(path) else None

    def writeData(self, file: FileSource, data, option: object = None): raise Exception('NotSupported')
