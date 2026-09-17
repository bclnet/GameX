import os, ctypes
from ctypes import *
from enum import IntEnum
from io import BytesIO

class XMEMCODEC(IntEnum):
    DEFAULT = 0
    LZX = 1

class PARAMETERS_LZX(Structure):
    _fields_ = [
        ('flags', c_uint),
        ('windowSize', c_uint),
        ('chunkSize', c_uint),
    ]

# https://stackoverflow.com/questions/252417/how-can-i-use-a-dll-file-from-python
so = CDLL(os.path.abspath(__file__.replace('xcompress.py', '../../core/x64/xcompress64.dll')))

so.XMemCreateCompressionContext.argtypes = [c_int, POINTER(PARAMETERS_LZX), c_int, POINTER(c_void_p)]
so.XMemResetCompressionContext.argtypes = [c_void_p]
so.XMemDestroyCompressionContext.argtypes = [c_void_p]
so.XMemCompress.argtypes = [c_void_p, POINTER(c_ubyte), POINTER(c_long), c_char_p, c_int]

so.XMemCreateDecompressionContext.argtypes = [c_int, POINTER(PARAMETERS_LZX), c_int, POINTER(c_void_p)]
so.XMemResetDecompressionContext.argtypes = [c_void_p]
so.XMemDestroyDecompressionContext.argtypes = [c_void_p]
so.XMemDecompress.argtypes = [c_void_p, POINTER(c_ubyte), POINTER(c_long), c_char_p, c_int]

class CompressionContext:
    def __init__(self):
        self.ctx = c_void_p(None)
        param = PARAMETERS_LZX(flags=0, windowSize=64*1024, chunkSize=256*1024)
        hr = 0
        if (hr := so.XMemCreateCompressionContext(type, param, 0, self.ctx)) != 0:
            raise Exception(f'XMemCreateCompressionContext returned non-zero value {hr}.')
    def __enter__(self): return self
    def __exit__(self, type, value, traceback): so.XMemDestroyCompressionContext(self.ctx)
    def reset(self) -> None:
        hr = 0
        if (hr := so.XMemResetCompressionContext(self.ctx)) != 0:
            raise Exception(f'XMemResetCompressionContext returned non-zero value {hr}.')
    def compress(self, data: bytes, outputRef: list[bytes]) -> None:
        output = outputRef[0]
        outputLen = len(output)
        outputLenRef = c_int(outputLen)
        c_output = (c_ubyte * outputLen).from_buffer(output)
        hr = 0
        if (hr := so.XMemDecompress(self.ctx, c_output, outputLenRef, data, len(data))) != 0:
            raise Exception(f'XMemCompress returned non-zero value {hr}.')
        if outputLen != outputLenRef.value: outputRef[0] = None; raise Exception('Resize')

class DecompressionContext:
    def __init__(self, type: XMEMCODEC = XMEMCODEC.LZX):
        self.ctx = c_void_p(None)
        param = PARAMETERS_LZX(flags=0, windowSize=64*1024, chunkSize=256*1024)
        hr = 0
        if (hr := so.XMemCreateDecompressionContext(type, param, 0, self.ctx)) != 0:
            raise Exception(f'XMemCreateDecompressionContext returned non-zero value {hr}.')
    def __enter__(self): return self
    def __exit__(self, type, value, traceback): so.XMemDestroyDecompressionContext(self.ctx)
    def reset(self) -> None:
        hr = 0
        if (hr := so.XMemResetDecompressionContext(self.ctx)) != 0:
            raise Exception(f'XMemResetDecompressionContext returned non-zero value {hr}.')
    def decompress(self, data: bytes, outputRef: list[bytes]) -> None:
        output = outputRef[0]
        outputLen = len(output)
        outputLenRef = c_int(outputLen)
        c_output = (c_ubyte * outputLen).from_buffer(output)
        hr = 0
        if (hr := so.XMemDecompress(self.ctx, c_output, outputLenRef, data, len(data))) != 0:
            raise Exception(f'XMemCompress returned non-zero value {hr}.')
        if outputLen != outputLenRef.value: outputRef[0] = None; raise Exception('Resize')