from __future__ import annotations
from zstandard import ZstdDecompressor

# C# Compression. BufferSize is the SharpZipLib chunk; LZMAPropsSize is the 5-byte property block.
BufferSize = 4096 * 10
LZMAPropsSize = 5

def _buf(x) -> bool: return isinstance(x, (bytes, bytearray, memoryview))

#region Doboz

def decompressDoboz(r: BinaryReader, length: int, newLength: int, targetSize: int = None): raise NotImplementedError('Doboz')

#endregion

#region Lz4

def decompressLz4(r: BinaryReader, length: int, newLength: int = None): raise NotImplementedError('LZ4')

#endregion

#region Lzo

def decompressLzo(r: BinaryReader, length: int, newLength: int = None): raise NotImplementedError('LZO')

#endregion

#region Lzf

def decompressLzf(r: BinaryReader, length: int, buffer: bytearray = None): raise NotImplementedError('LZF')

#endregion

#region Oodle

def decompressOodle(r: BinaryReader, length: int, newLength: int = None): raise NotImplementedError('Oodle')

#endregion

#region Snappy

def decompressSnappy(r: BinaryReader, length: int, newLength: int = None): raise NotImplementedError('Snappy')

#endregion

#region Zstd

# Reader form consumes `length` compressed bytes (callers pass the packed size) and returns at most newLength.
# (source, target) -> int writes into target, same as C#'s byte[] overload.
def decompressZstd(r: BinaryReader, length: int, newLength: int = None):
    if _buf(r) and _buf(length):
        source, target = r, length
        out = ZstdDecompressor().decompress(source)
        n = min(len(out), len(target))
        target[:n] = out[:n]
        return n
    out = ZstdDecompressor().decompress(r.readBytes(length))
    if isinstance(newLength, int) and newLength >= 0 and len(out) > newLength: return out[:newLength]
    return out

#endregion

#region Zlib

def decompressZlibStream(r: BinaryReader, noHeader: bool = False) -> bytes:
    import zlib
    z = zlib.decompressobj(-15 if noHeader else 0)
    res = bytearray(1024)
    outputUsed = 0
    pending = b''
    while not z.eof:
        if not pending:
            pending = r.readBytes(1024)
            if not pending: raise EOFError('Unexpected End Of File')
        if outputUsed == len(res):
            grown = bytearray(len(res) * 2)
            grown[:outputUsed] = res[:outputUsed]
            res = grown
        try: out = z.decompress(pending, len(res) - outputUsed)
        except zlib.error as e: raise OSError(str(e)) from e
        pending = z.unconsumed_tail
        res[outputUsed:outputUsed + len(out)] = out
        outputUsed += len(out)
    # Adjust reader to point to the next stream correctly.
    r.seek(r.tell() - len(z.unused_data))
    return bytes(res[:outputUsed])

def _inflate(data: bytes, noHeader: bool, full: bool) -> bytes:
    import zlib
    wbits = -15 if noHeader else 0
    if not full: return zlib.decompressobj(wbits = wbits).decompress(data)
    z = zlib.decompressobj(wbits)
    s = bytearray()
    pending = data
    while True:
        out = z.decompress(pending, BufferSize)
        pending = z.unconsumed_tail
        if out: s += out
        if z.eof or not out: break
    return bytes(s)

# (BinaryReader, length, newLength, noHeader=False, full=True) -> bytes. full=False is the partial read Capcom uses.
# (source, target) -> int. Inflater(false): zlib header, fill target.
def decompressZlib(r: BinaryReader, length: int, newLength: int = None, noHeader: bool = False, full: bool = True):
    if _buf(r) and _buf(length): return _decompressZlibInto(r, length)
    return _inflate(r.readBytes(length), noHeader, full)

def _decompressZlibInto(source: bytes, target: bytearray) -> int:
    import zlib
    z = zlib.decompressobj(15)
    pending = source
    filled = 0
    while filled < len(target):
        out = z.decompress(pending, len(target) - filled)
        pending = z.unconsumed_tail
        if not out: break
        target[filled:filled + len(out)] = out
        filled += len(out)
        if z.eof: break
    return filled

def decompressZlib2(r: BinaryReader, length: int, newLength: int) -> bytes:
    import zlib
    data = r.readBytes(length)
    res = bytearray(newLength)
    z = zlib.decompressobj(15)
    pending = data
    filled = 0
    while filled < newLength:
        out = z.decompress(pending, min(4096, newLength - filled))
        pending = z.unconsumed_tail
        if not out: break
        res[filled:filled + len(out)] = out
        filled += len(out)
        if z.eof: break
    return bytes(res)

# Level 9, zlib wrapper, BufferSize input pieces. Flushed so the stream is complete (C# never calls Finish()).
def compressZlib(source: bytes, length: int) -> bytes:
    import zlib
    z = zlib.compressobj(zlib.Z_BEST_COMPRESSION)
    view = memoryview(source)[:length]
    parts = []
    off = 0
    while off < len(view):
        parts.append(z.compress(view[off:off + BufferSize]))
        off += BufferSize
    parts.append(z.flush())
    return b''.join(parts)

#endregion

#region Lzma

# No end marker (C#'s default): a fake LZMA Alone header carries newLength.
# An end marker makes that header fail on this liblzma; the raw decoder takes that stream.
def decompressLzma(r: BinaryReader, length: int, newLength: int = None):
    import lzma
    if _buf(r): raise NotImplementedError('LZMA')
    props = bytes(r.readBytes(LZMAPropsSize))
    data = bytes(r.readBytes(length))
    size = int(newLength) if isinstance(newLength, int) and newLength >= 0 else None
    if size is not None:
        try: return lzma.decompress(props + size.to_bytes(8, 'little') + data, format = lzma.FORMAT_ALONE)
        except lzma.LZMAError: pass
    d = props[0]
    lc = d % 9
    d //= 9
    lp = d % 5
    pb = d // 5
    dictSize = int.from_bytes(props[1:5], 'little') or 1
    dec = lzma.LZMADecompressor(format = lzma.FORMAT_RAW, filters = [{
        'id': lzma.FILTER_LZMA1, 'dict_size': dictSize, 'lc': lc, 'lp': lp, 'pb': pb,
    }])
    limit = size if size is not None else -1
    out = dec.decompress(data, limit)
    if limit >= 0:
        while len(out) < limit and not dec.needs_input and not dec.eof:
            more = dec.decompress(b'', limit - len(out))
            if not more: break
            out += more
    return out

#endregion

#region Blast

def decompressBlast(r: BinaryReader, length: int, newLength: int = None):
    if _buf(r): raise NotImplementedError('Blast')
    from ...._LIB.compression.blast import Blast
    z = Blast()
    data = r.readBytes(length)
    res = bytearray(newLength)
    z.decompress(data, res)
    return res

#endregion

#region Lzss

def decompressLzss(r: BinaryReader, length: int, newLength: int = None):
    if _buf(r): raise NotImplementedError('LZSS')
    from io import BytesIO
    from ...._LIB.compression.lzss import Lzss
    return Lzss(BytesIO(r.readBytes(length)), newLength).decompress()

#endregion

#region Xmem

def decompressXmem(r: BinaryReader, length: int, newLength: int, codec: int = 1): raise NotImplementedError('XCompress')

# Not a C# method. Xbox callers import this; it still calls xcompress.
def decompressXbox(r: BinaryReader, length: int, newLength: int, codec: int = 1) -> bytes:
    from ...._LIB.compression.xcompress import XMEMCODEC, DecompressionContext
    res = [bytearray(newLength)]
    with DecompressionContext(XMEMCODEC(codec)) as ctx:
        ctx.decompress(r.readBytes(length), res)
    return res[0]

#endregion

def decompressUnknown(r: BinaryReader, length: int, newLength: int) -> bytes: raise NotImplementedError('decompressUnknown')
