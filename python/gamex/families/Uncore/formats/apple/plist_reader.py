from __future__ import annotations
import sys
from datetime import datetime, timedelta, timezone
from io import BytesIO
from struct import unpack

# PlistReader
class PlistReader:
    class ISerializable:
        def fromDictionary(self, plist: dict) -> None: pass
        def toDictionary(self) -> dict: pass

    class PlistItem:
        def __init__(self, value: object):
            self.byteValue: list[int] = []
            self.isArray: bool = False
            self.isDictionary: bool = False
            self.marker: list[int] = []
            self.value = value
        @property
        def size(self) -> int: return len(self.marker) + len(self.byteValue)
        def setByteValue(self, buffer) -> None:
            self.byteValue.clear()
            if buffer is not None: self.byteValue.extend(buffer)
        def __str__(self) -> str: return str(self.value) if self.value is not None else 'null'

    class PlistArray:
        def __init__(self, objectTable: list, size: int = 0):
            self.objectReference: list[int] = []
            self.objectTable = objectTable
        def toArray(self) -> list:
            array = [None] * len(self.objectReference)
            for i in range(len(array)):
                objectRef = self.objectReference[i]
                if objectRef >= 0 and objectRef < len(self.objectTable) and (self.objectTable[objectRef] is None or self.objectTable[objectRef].value is not self):
                    objectValue = None if self.objectTable[objectRef] is None else self.objectTable[objectRef].value
                    if isinstance(objectValue, PlistReader.PlistDictionary): objectValue = objectValue.toDictionary()
                    elif isinstance(objectValue, PlistReader.PlistArray): objectValue = objectValue.toArray()
                    array[i] = objectValue
            return array
        def __str__(self) -> str:
            b = ['[']
            for i in range(len(self.objectReference)):
                if i > 0: b.append(',')
                objectRef = self.objectReference[i]
                if len(self.objectTable) > objectRef and (self.objectTable[objectRef] is None or self.objectTable[objectRef].value is not self): b.append(str(self.objectReference[objectRef]))
                else: b.append(f'*{objectRef}')
            return ''.join(b) + ']'

    class PlistDictionary:
        def __init__(self, objectTable: list, size: int = 0):
            self.keyReference: list[int] = []
            self.objectReference: list[int] = []
            self.objectTable = objectTable
        def toDictionary(self) -> dict:
            dictionary = {}
            for i in range(len(self.keyReference)):
                keyRef = self.keyReference[i]
                objectRef = self.objectReference[i]
                if keyRef >= 0 and keyRef < len(self.objectTable) and (self.objectTable[keyRef] is None or self.objectTable[keyRef].value is not self) and \
                    objectRef >= 0 and objectRef < len(self.objectTable) and (self.objectTable[objectRef] is None or self.objectTable[objectRef].value is not self):
                    keyValue = None if self.objectTable[keyRef] is None else self.objectTable[keyRef].value
                    objectValue = None if self.objectTable[objectRef] is None else self.objectTable[objectRef].value
                    if isinstance(objectValue, PlistReader.PlistDictionary): objectValue = objectValue.toDictionary()
                    elif isinstance(objectValue, PlistReader.PlistArray): objectValue = objectValue.toArray()
                    dictionary[keyValue] = objectValue
            return dictionary
        def __str__(self) -> str:
            b = ['{']
            for i in range(len(self.keyReference)):
                if i > 0: b.append(',')
                keyRef = self.keyReference[i]
                objectRef = self.objectReference[i]
                if keyRef < 0 or keyRef >= len(self.objectTable): b.append(f'#{keyRef}')
                elif self.objectTable[keyRef] is not None and self.objectTable[keyRef].value is self: b.append(f'*{keyRef}')
                else: b.append(str(self.objectTable[keyRef]))
                b.append(':')
                if objectRef < 0 or objectRef >= len(self.objectTable): b.append(f'#{objectRef}')
                elif self.objectTable[objectRef] is not None and self.objectTable[objectRef].value is self: b.append(f'*{objectRef}')
                else: b.append(str(self.objectTable[objectRef]))
            return ''.join(b) + '}'

    headerMagicNumber = 0x62706c69
    headerVersionNumber = 0x73743030
    referenceDate = datetime(2001, 1, 1, tzinfo=timezone.utc)

    def __init__(self):
        self.objectTable = None
        self.offsetTable = None
        self.offsetIntSize = self.objectRefSize = self.objectCount = self.topLevelObjectOffset = self.offsetTableOffset = 0

    def readObject(self, s):
        if isinstance(s, str):
            with open(s, 'rb') as f: return self.readObject(f)
        if s is None: raise ValueError('stream cannot be null.')
        concrete = s
        dispose = False
        seekable = getattr(s, 'seekable', lambda: False)()
        if not seekable:
            concrete = BytesIO()
            while True:
                buf = s.read(4096)
                if not buf: break
                concrete.write(buf)
            concrete.seek(0)
            dispose = True
        try:
            dictionary = None
            self._reset()
            length = _streamLength(s) if seekable else _streamLength(concrete)
            if length > 40:
                from openx.core import BinaryReader
                r = BinaryReader(concrete, leaveOpen=True)
                s.seek(0) if seekable else concrete.seek(0)
                bpli = r.readInt32E()
                version = r.readInt32E()
                if bpli != PlistReader.headerMagicNumber or version != PlistReader.headerVersionNumber:
                    raise ValueError("The stream data does not start with required 'bplist00' header.")
                # trailer: offset 26 from the end, not 32
                (s if seekable else concrete).seek(length - 26)
                self.offsetIntSize = r.readByte()
                self.objectRefSize = r.readByte()
                self.objectCount = r.readInt64E()
                self.topLevelObjectOffset = r.readInt64E()
                self.offsetTableOffset = r.readInt64E()
                self._readOffsetTable(r)
                self._readObjectTable(r)
            else: raise ValueError('The stream is too short to be a valid binary plist.')
            root = self.objectTable[-1]
            if isinstance(root.value, PlistReader.PlistDictionary): dictionary = root.value.toDictionary()
            else: raise Exception(f'Unsupported root plist object: GameX.Uncore.Formats.Apple.PlistReader+PlistItem. A dictionary must be the root plist object.')
            return dictionary if dictionary is not None else {}
        finally:
            if dispose: concrete.close()

    def readObjectT(self, s, cls):
        # C# ReadObject<T>(string) opens the file, then calls ReadObject<T>(path) again.
        if isinstance(s, str):
            with open(s, 'rb') as f: return self.readObjectT(s, cls)
        obj = cls()
        obj.fromDictionary(self.readObject(s))
        return obj

    def _readOffsetTable(self, r) -> None:
        for i in range(self.objectCount):
            self.offsetTable.append(_readInteger(r, self.offsetTableOffset + (i * self.offsetIntSize), self.offsetIntSize))

    def _readObjectTable(self, r) -> None:
        for i in range(self.objectCount):
            r.f.seek(self.offsetTable[i])
            marker = r.readByte()
            kind = (marker & 0xf0) >> 4
            if kind == 0:
                ok, primitive = _readPrimitive(r, r.f.tell() - 1)
                if ok: self.objectTable.append(PlistReader.PlistItem(primitive))
            elif kind == 1:
                size = 1 << (marker & 0xf)
                parsed = _readInteger(r, r.f.tell(), size)
                if size < 4: self.objectTable.append(PlistReader.PlistItem(_asShort(parsed)))
                elif size < 8: self.objectTable.append(PlistReader.PlistItem(_asInt(parsed)))
                else: self.objectTable.append(PlistReader.PlistItem(parsed))
            elif kind == 2:
                size = 1 << (marker & 0xf)
                self.objectTable.append(PlistReader.PlistItem(_readReal(r, r.f.tell(), size)))
            elif kind == 3:
                size = marker & 0xf
                if size == 3: self.objectTable.append(PlistReader.PlistItem(_readDate(r, r.f.tell(), 8)))
                else: raise Exception(f'Unsupported date size: {size:b}')
            elif kind == 4:
                size = marker & 0xf
                if size == 15:
                    intSize = 1 << (r.readByte() & 0xf)
                    size = _readInteger(r, r.f.tell(), intSize)
                self.objectTable.append(PlistReader.PlistItem(_readData(r, r.f.tell(), size)))
            elif kind == 5:
                size = marker & 0xf
                if size == 15:
                    intSize = 1 << (r.readByte() & 0xf)
                    size = _readInteger(r, r.f.tell(), intSize)
                self.objectTable.append(PlistReader.PlistItem(_readAscii(r, r.f.tell(), size)))
            elif kind == 6:
                size = marker & 0xf
                if size == 15:
                    intSize = 1 << (r.readByte() & 0xf)
                    size = _readInteger(r, r.f.tell(), intSize)
                self.objectTable.append(PlistReader.PlistItem(_readUnicode(r, r.f.tell(), size)))
            elif kind == 8:
                size = (marker & 0xf) + 1
                self.objectTable.append(PlistReader.PlistItem(_readUniqueId(r, r.f.tell(), size)))
            elif kind == 10 or kind == 12:
                size = marker & 0xf
                if size == 15:
                    intSize = 1 << (r.readByte() & 0xf)
                    size = _readInteger(r, r.f.tell(), intSize)
                item = PlistReader.PlistItem(self._readArray(r, r.f.tell(), size))
                item.isArray = True
                self.objectTable.append(item)
            elif kind == 13:
                size = marker & 0xf
                if size == 15:
                    intSize = 1 << (r.readByte() & 0xf)
                    size = _readInteger(r, r.f.tell(), intSize)
                item = PlistReader.PlistItem(self._readDictionary(r, r.f.tell(), size))
                item.isDictionary = True
                self.objectTable.append(item)
            else: raise Exception(f'An invalid marker was found while reading the object table: {marker:b}')

    def _readArray(self, r, index: int, size: int):
        array = PlistReader.PlistArray(self.objectTable, size)
        for i in range(size):
            array.objectReference.append(_readInteger(r, index + (i * self.objectRefSize), self.objectRefSize))
        return array

    def _readDictionary(self, r, index: int, size: int):
        dictionary = PlistReader.PlistDictionary(self.objectTable, size)
        skip = size * self.objectRefSize
        for i in range(size):
            dictionary.keyReference.append(_readInteger(r, index + (i * self.objectRefSize), self.objectRefSize))
            dictionary.objectReference.append(_readInteger(r, skip + index + (i * self.objectRefSize), self.objectRefSize))
        return dictionary

    def _reset(self) -> None:
        self.objectRefSize = self.objectCount = self.offsetIntSize = self.offsetTableOffset = self.topLevelObjectOffset = 0
        self.objectTable = []
        self.offsetTable = []

def _streamLength(s) -> int:
    pos = s.tell()
    n = s.seek(0, 2)
    s.seek(pos)
    return n

def _readData(r, index: int, size: int) -> bytes:
    r.f.seek(index)
    buf = bytearray(size)
    filled = 0
    while filled < size:
        got = r.f.read(size - filled)
        if not got: break
        buf[filled:filled + len(got)] = got
        filled += len(got)
    return bytes(buf)

def _readAscii(r, index: int, size: int) -> str:
    buf = _readData(r, index, size)
    if not buf: return ''
    return bytes(b if b < 128 else ord('?') for b in buf).decode('ascii')

def _readInteger(r, index: int, size: int) -> int:
    buf = bytearray(_readData(r, index, size))
    if len(buf) > 1 and sys.byteorder == 'little': buf.reverse()
    if size == 1: return buf[0]
    if size == 2: return int.from_bytes(buf, sys.byteorder, signed=False)
    if size == 4: return int.from_bytes(buf, sys.byteorder, signed=False)
    if size == 8: return int.from_bytes(buf, sys.byteorder, signed=True)
    raise Exception(f'Unsupported variable-length integer size: {size}')

def _asShort(v: int) -> int:
    v &= 0xFFFF
    return v - 0x10000 if v & 0x8000 else v

def _asInt(v: int) -> int:
    v &= 0xFFFFFFFF
    return v - 0x100000000 if v & 0x80000000 else v

def _readPrimitive(r, index: int):
    r.f.seek(index)
    value = r.readByte()
    low = value & 0xf
    if low == 0: return True, None
    if low == 8: return True, False
    if low == 9: return True, True
    if low == 15: return False, None
    raise Exception(f'Illegal primitive: {value:b}')

def _readReal(r, index: int, size: int) -> float:
    buf = _readData(r, index, size)
    if size == 4: return float(unpack('>f', buf)[0])
    if size == 8: return float(unpack('>d', buf)[0])
    raise Exception(f'Unsupported floating point number size: {size}')

def _readDate(r, index: int, size: int) -> datetime:
    return (PlistReader.referenceDate + timedelta(seconds=_readReal(r, index, size))).astimezone()

def _readUnicode(r, index: int, size: int) -> str:
    r.f.seek(index)
    size *= 2
    buf = bytearray(size)
    i = 0
    while i < size:
        one = r.readByte(); two = r.readByte()
        if sys.byteorder == 'little': buf[i] = two; i += 1; buf[i] = one
        else: buf[i] = one; i += 1; buf[i] = two
        i += 1
    return buf.decode('utf-16-le')

def _readUniqueId(r, index: int, size: int) -> dict:
    return { 'CF$UID': _readInteger(r, index, size) & 0xFFFFFFFFFFFFFFFF }
