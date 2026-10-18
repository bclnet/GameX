from __future__ import annotations

# Rnc — Rob Northen compression. Header sizes are big-endian; the 4-byte signature is not swapped (C# ">4x2I3H").

RNC_MAGIC = 0x01434E52

def _u32(n: int) -> int: return n & 0xFFFFFFFF

def _mask(bits: int) -> int:
    return ((1 << (bits & 31)) - 1) & 0xFFFFFFFF

class _BitStream:
    def __init__(self, source: bytes):
        self.source = source
        self.p = 0
        self.pend = len(source)
        self.bitbuf = self._lword(0)
        self.bitcount = 16

    @property
    def remain(self) -> int: return self.pend - self.p

    def _lword(self, offset: int) -> int: return (self.source[offset + 1] << 8) + self.source[offset]
    def _lbyte(self, offset: int) -> int: return self.source[offset]

    def fix(self) -> None:
        self.bitcount -= 16
        self.bitbuf = _u32(self.bitbuf & _mask(self.bitcount))
        remain = self.pend - self.p
        if remain > 1: self.bitbuf = _u32(self.bitbuf | (self._lword(self.p) << (self.bitcount & 31)))
        elif remain == 1: self.bitbuf = _u32(self.bitbuf | (self._lbyte(self.p) << (self.bitcount & 31)))
        self.bitcount += 16

    def peek(self, mask: int) -> int: return _u32(self.bitbuf & mask)

    def advance(self, n: int) -> None:
        self.bitbuf = _u32(self.bitbuf >> (n & 31))
        self.bitcount -= n
        if self.bitcount < 16:
            self.p += 2
            remain = self.pend - self.p
            if remain > 1: self.bitbuf = _u32(self.bitbuf | (self._lword(self.p) << (self.bitcount & 31)))
            elif remain == 1: self.bitbuf = _u32(self.bitbuf | (self._lbyte(self.p) << (self.bitcount & 31)))
            self.bitcount += 16

    def read(self, mask: int, n: int) -> int:
        result = self.peek(mask)
        self.advance(n)
        return result

    def readByte(self) -> int:
        b = self.source[self.p]
        self.p += 1
        return b

class _HufTable:
    def __init__(self):
        self.num = 0
        self.table = [{'code': 0, 'codelen': 0, 'value': 0} for _ in range(32)]

    def readTable(self, bs: _BitStream) -> None:
        num = bs.read(0x1F, 5)
        if num == 0: return
        leaflen = [0] * 32
        leafmax = 1
        for i in range(num):
            leaflen[i] = bs.read(0x0F, 4)
            if leafmax < leaflen[i]: leafmax = leaflen[i]
        codeb = 0
        k = 0
        for i in range(1, leafmax + 1):
            for j in range(num):
                if leaflen[j] == i:
                    self.table[k]['code'] = _HufTable._mirror(codeb, i)
                    self.table[k]['codelen'] = i
                    self.table[k]['value'] = j
                    codeb = _u32(codeb + 1)
                    k += 1
            codeb = _u32(codeb << 1)
        self.num = k

    def read(self, bs: _BitStream) -> int:
        i = 0
        while i < self.num:
            node = self.table[i]
            if bs.peek(_mask(node['codelen'])) == node['code']: break
            i += 1
        if i == self.num: return -1
        node = self.table[i]
        bs.advance(node['codelen'])
        val = node['value']
        if val >= 2:
            val = _u32(1 << ((val - 1) & 31))
            val = _u32(val | bs.read(_u32(val - 1), node['value'] - 1))
        return val

    @staticmethod
    def _mirror(x: int, n: int) -> int:
        top = 1 << (n - 1)
        bottom = 1
        while top > bottom:
            mask = top | bottom
            masked = x & mask
            if masked != 0 and masked != mask: x ^= mask
            top >>= 1
            bottom <<= 1
        return _u32(x)

class _RncCrc16:
    _crctab = None

    @staticmethod
    def _table() -> list[int]:
        if _RncCrc16._crctab == None:
            r = [0] * 256
            for i in range(256):
                val = i
                for _ in range(8):
                    val = ((val >> 1) ^ 0xA001) & 0xFFFF if (val & 1) != 0 else (val >> 1) & 0xFFFF
                r[i] = val
            _RncCrc16._crctab = r
        return _RncCrc16._crctab

    @staticmethod
    def compute(data: bytes) -> int:
        tab = _RncCrc16._table()
        val = 0
        for b in data:
            val ^= b
            val = ((val >> 8) ^ tab[val & 0xFF]) & 0xFFFF
        return val

class Rnc:
    RNC_MAGIC = RNC_MAGIC

    @staticmethod
    def read(r) -> bytes:
        signature = r.readUInt32()
        unpackedSize = r.readUInt32E()
        packedSize = r.readUInt32E()
        unpackedCrc32 = r.readUInt16E()
        packedCrc32 = r.readUInt16E()
        r.readUInt16E()
        if signature == Rnc.RNC_MAGIC: return Rnc._unpack(r, unpackedSize, packedSize, unpackedCrc32, packedCrc32)
        r.seek(0)
        return r.readToEnd()

    @staticmethod
    def _unpack(r, unpackedSize: int, packedSize: int, unpackedCrc32: int, packedCrc32: int) -> bytes:
        if unpackedSize > (1 << 30) or packedSize > (1 << 30): raise Exception('Bad Size')
        input = r.readBytes(packedSize)
        if _RncCrc16.compute(input) != packedCrc32: raise Exception('Bad CRC')
        output = Rnc._unpackBytes(input, unpackedSize)
        if _RncCrc16.compute(output) != unpackedCrc32: raise Exception('Bad CRC')
        return output

    @staticmethod
    def _unpackBytes(input: bytes, unpackedSize: int) -> bytes:
        output = bytearray(unpackedSize)
        bs = _BitStream(input)
        bs.advance(2)
        o = 0
        oend = len(output)
        raw = _HufTable()
        dist = _HufTable()
        lengthTab = _HufTable()
        while o < oend:
            if bs.remain < 6: raise Exception()
            raw.readTable(bs)
            dist.readTable(bs)
            lengthTab.readTable(bs)
            chCount = bs.read(0xFFFF, 16)
            while True:
                length = raw.read(bs)
                if length == -1: raise Exception()
                elif length != 0:
                    while length != 0:
                        length -= 1
                        output[o] = bs.readByte()
                        o += 1
                    bs.fix()
                chCount = (chCount - 1) & 0xFFFFFFFF
                if chCount == 0: break
                posn = dist.read(bs)
                if posn == -1: raise Exception()
                length = lengthTab.read(bs)
                if length == -1: raise Exception()
                posn += 1
                length += 2
                while length != 0:
                    length -= 1
                    output[o] = output[o - posn]
                    o += 1
        return bytes(output)
