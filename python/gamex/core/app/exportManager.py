from __future__ import annotations
import os, io, asyncio, shutil
from openstk import parallelFor, IStream, IWriteToStream
from gamex import FileOption, Archive, MultiArchive, BinaryArchive

# ExportManager
class ExportManager:
    MaxDegreeOfParallelism = 1 #4

    @staticmethod
    async def exportAsync(family: Family, res: Resource, filePath: str, match: callable, from_: int, option: object) -> None:
        fo = option if isinstance(option, FileOption) else FileOption.Default
        with family.openArchive(res) as arc:
            # single
            if not isinstance(arc, MultiArchive): await ExportManager.exportPakAsync(filePath, match, from_, option, arc); return
            # write arcs
            if FileOption.Marker in fo:
                if filePath and not os.path.isdir(filePath): os.makedirs(filePath)
                setPath = os.path.join(filePath, '.set')
            #     using var w = new BinaryWriter(new FileStream(setPath, FileMode.Create, FileAccess.Write));
            #     await ArcBinary.Stream.Write(new StreamArchive(NetworkHost.Factory, new ArcState(null, null, null, "Root")) {
            #         Files = [.. multi.Archives.Select(x => new FileSource { Path = x.Name })]
            #     }, w, "Set");
            # multi
            for _ in arc.archives: await ExportManager.exportPakAsync(filePath, match, from_, option, _)

    @staticmethod
    async def exportPakAsync(filePath: str, match: callable, from_: int, option: object, _: Archive) -> None:
        arc = _
        if not isinstance(arc, BinaryArchive): raise Exception('s not a BinaryArchive')
        newPath = os.path.join(filePath, os.path.basename(arc.arcPath)) if filePath else None
        # write arc
        await ExportManager.exportPak2Async(arc, newPath, match, from_, option, \
            lambda file, idx: print(f'{idx:>6}> {file.path}') if (idx % 50) == 0 else None, \
            lambda file, msg: print(f'ERROR: {msg} - {file.path}'))

    @staticmethod
    async def exportPak2Async(source: BinaryArchive, filePath: str, match: callable, from_: int, option: object, next: callable, error: callable) -> None:
        fo = option if isinstance(option, FileOption) else FileOption.Default
        # create directory
        if filePath and not os.path.isdir(filePath): os.makedirs(filePath)
        # write files
        async def _lambdax(index: int):
            file = source.files[index].fix()
            # print(match(file.path))
            if match and not match(file.path): return
            newPath = os.path.join(filePath, file.path) if filePath else None
            # create directory
            directory = os.path.dirname(newPath) if newPath else None
            if directory and not os.path.isdir(directory): os.makedirs(directory)
            # recursive extract arc, and exit
            if file.arc: await exportPak2Async(file.arc, newPath, match, 0, option, next, error); return
            # ensure cached object factory
            if FileOption.Object in fo: source.ensureCachedObjectFactory(file)
            # extract file
            try:
                await ExportManager.exportFileAsync(file, source, newPath, option)
                if file.parts != None and FileOption.Raw in fo:
                    for part in file.parts: await ExportManager.exportFileAsync(part, source, os.path.join(filePath, part.path), option)
                if next: next(file, index)
            except Exception as e: error(file, repr(e)) if error else None # f'Exception: {str(e)}'
        await parallelFor(from_, len(source.files), { 'max': ExportManager.MaxDegreeOfParallelism }, _lambdax)
        # write arc-raw
        # if FileOption.Marker in fo: await StreamArchive(source, new ArcState(source.Vfx, source.Game, source.Edition, filePath)).Write(null)

    @staticmethod
    async def exportFileAsync(file: FileSource, source: BinaryArchive, newPath: str, option: object) -> None:
        fo = option if isinstance(option, FileOption) else FileOption.Default
        if file.fileSize == 0 and file.packedSize == 0: return
        oo = file.cachedObjectOption if isinstance(file.cachedObjectOption, FileOption) else FileOption.Default
        if file.cachedObjectOption and bool(fo & oo):
            if FileOption.UnknownFileModel in oo:
                model = source.getAsset(IUnknownFileModel, file, FamilyManager.UnknownArchive)
                # UnknownFileWriter.Factory('default', model).Write(newPath, false)
                return
            elif FileOption.BinaryObject in oo:
                obj = source.getAsset(object, file)
                if isinstance(obj, IStream):
                    with obj.getStream() as b2, open(newPath, 'wb') if newPath else io.BytesIO() as s2:
                        shutil.copyfileobj(b2, s2)
                    return
                ArcBinary.handleException(None, option, f'BinaryObject: {file.Path} @ {file.FileSize}')
                raise Exception()
            elif FileOption.StreamObject in oo:
                obj = source.getAsset(object, file)
                if isinstance(obj, IWriteToStream):
                    with open(newPath, 'w') if newPath else io.BytesIO() as s2:
                        obj.writeToStream(s2)
                    return
                ArcBinary.handleException(None, option, f'StreamObject: {file.Path} @ {file.FileSize}')
                raise Exception()
        with source.getData(file, option) as b, open(newPath, 'wb') if newPath else io.BytesIO() as s:
            shutil.copyfileobj(b, s)
            if file.parts and FileOption.Raw not in fo:
                for part in file.parts:
                    with await source.getData(part, option) as b2:
                        shutil.copyfileobj(b2, s)
