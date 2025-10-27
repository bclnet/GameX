from __future__ import annotations
from io import BytesIO
from zstandard import ZstdDecompressor

def decompressUnknown(r: Reader, length: int, newLength: int) -> bytes: raise NotImplementedError()
def decompressZlibStream(r: Reader, noHeader: bool = False) -> bytes: 
    import zlib
    z = zlib.decompressobj(wbits = (-15 if noHeader else 0))
    outs = []
    while True:
        if not (buf := r.f.read(1024)): break # Read in chunks (e.g., 4KB) #4096
        out = z.decompress(buf)
        if out: outs.append(out)
        if z.eof: break
    r.seek(r.tell() - len(z.unused_data))
    return b''.join(outs)
def decompressZlib(r: Reader, length: int, newLength: int, noHeader: bool = False, full: bool = True) -> bytes: 
    import zlib
    return \
        zlib.decompress(r.readBytes(length), wbits = (-15 if noHeader else 0)) if full else \
        zlib.decompressobj(wbits = (-15 if noHeader else 0)).decompress(r.readBytes(length))
def decompressZstd(r: Reader, length: int, newLength: int) -> bytes:
    return ZstdDecompressor().decompress(r.readBytes(length))
def decompressLzss(r: Reader, length: int, newLength: int) -> bytes:
    from ._LIB.compression.lzss import Lzss
    return Lzss(BytesIO(r.readBytes(length)), newLength).decompress()
def decompressBlast(r: Reader, length: int, newLength: int) -> bytes:
    from ._LIB.compression.blast import Blast
    z = Blast()
    fs = r.readBytes(length)
    os = bytearray(newLength)
    z.decompress(fs, os)
    return os
def decompressLz4(r: Reader, length: int, newLength: int) -> bytes: raise NotImplementedError()
# def decompressZlib2(r: Reader, length: int, newLength: int) -> bytes: raise NotImplementedError()