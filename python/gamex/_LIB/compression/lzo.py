import os
from io import BytesIO

# Lzo1xDecompressor
class Lzo1xDecompressor:
    def __init__(self, input: bytes, output: bytearray):
        self.input = input
        self.output = output
        self.inputPointer = 0
        self.outputPointer = 0
        self.outputPointer2 = 0

    def read(self) -> int: z = self.input[self.inputPointer]; self.inputPointer += 1; return z
    def peek(self) -> int: return self.input[self.inputPointer]
    def readUInt16(self) -> int: z = self.input[self.inputPointer] + (self.input[self.inputPointer + 1] << 8); self.inputPointer += 2; return z

    def decompress(self) -> int:
        gotoFirst = False; gotoMatchdone = False; t = 0
        def _first():
            t = self.read()
            if t >= 16: _match()
            self.outputPointer2 = self.outputPointer - (1 + 0x0800) - (t >> 2) - (self.read() << 2)
            self.copyBytes(3, fromOutput=True)
            gotoMatchdone = True
        def _match():
            while True:
                if gotoMatchdone: gotoMatchdone = False; break
                if t >= 64:
                    self.outputPointer2 = self.outputPointer - 1 - ((t >> 2) & 7) - (self.read() << 3)
                    t = (t >> 5) - 1
                    self.copyBytes(max(3, t + 2), fromOutput=True)
                    break
                elif t >= 32:
                    t &= 31
                    if t == 0: t += 31 + self.readLength()
                    self.outputPointer2 = self.outputPointer - 1 - (self.readUInt16() >> 2)
                    self.inputPointer += 2
                elif t >= 16:
                    self.outputPointer2 = self.outputPointer - ((t & 8) << 11)
                    t &= 7
                    if t == 0: t += 7 + self.readLength()
                    self.outputPointer2 -= self.readUInt16() >> 2
                    self.inputPointer += 2
                    if self.outputPointer2 == self.outputPointer: goto _eof
                    self.outputPointer2 -= 0x4000
                else:
                    self.outputPointer2 = self.outputPointer - 1 - (t >> 2) - (self.inputRead() << 2)
                    self.copyBytes(2, fromOutput=True)
                    break
                if t >= 2 * 4 - (3 - 1) and (self.outputPointer - self.outputPointer2) >= 4: t += 4 - (3 - 1); self.copyBytes(t, fromOutput=True)
                else: self.copyBytes(max(3, t + 2), fromOutput=True)
            # _match_done:
            #     t = (uint)(Input[InputPointer - 2] & 3);
            #     if (t == 0) break;
            #     CopyBytes(t);
            #     t = InputRead;
        def _eof():
            return 0 if self.inputPointer == len(self.input) else -8 if self.inputPointer < len(self.input) else -4
        if self.peek() > 17:
            t = self.read() - 17
            if t >= 4: gotoFirst = True
            copyBytes(max(1, t))
        while True:
            if gotoFirst: gotoFirst = False; _first()
            t = self.read()
            if t >= 16: _match()
            if t == 0: t += 15 + self.readLength()
            copyBytes(4 + t - 1)
            _eof()

    def readLength(self) -> int:
        length = 0
        while True:
            b = self.inputRead
            if b == 0: length += 255
            else: length += b; break
        return length

    def copyBytes(self, size: int, fromOutput: bool = False) -> None:
        if fromOutput:
            for i in range(size): self.output[self.outputPointer + i] = self.output[self.outputPointer2 + i]
            self.outputPointer += size
            self.outputPointer2 += size
        else:
            for i in range(size): self.output[self.outputPointer + i] = self.input[self.inputPointer + i]
            self.outputPointer += size
            self.inputPointer += size
