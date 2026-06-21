from __future__ import annotations
import sys, os, io, struct, array, ctypes
from enum import Enum
from zstandard import ZstdDecompressor

# test
major, minor = sys.version_info.major, sys.version_info.minor
if major != 3 and minor < 12: raise Exception('Only vetted for Python 3.12+')

#region ZipEncrypt
# ref: C:\_GITHUB\_ref\CryEngine\Code\CryEngine\CrySystem\ZipDirCacheFactory.cpp

# cry
DefaultRsaKey = [
    0x30, 0x81, 0x9F, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01,
    0x05, 0x00, 0x03, 0x81, 0x8D, 0x00, 0x30, 0x81, 0x89, 0x02, 0x81, 0x81, 0x00, 0xA9, 0xD5, 0x90,
    0xA4, 0xBC, 0x92, 0xDB, 0x8C, 0xF1, 0xFC, 0x5A, 0xD5, 0x8F, 0x46, 0x05, 0x52, 0x16, 0xEE, 0xF3,
    0xC3, 0xBE, 0x86, 0xDE, 0x70, 0x1F, 0x4E, 0x2D, 0x18, 0xD3, 0x01, 0x92, 0x46, 0xBE, 0xFA, 0xAD,
    0x66, 0x04, 0x7B, 0x8C, 0xDD, 0x0D, 0x24, 0x8D, 0xA7, 0x23, 0xCA, 0x52, 0xC8, 0xE5, 0x01, 0xE0,
    0xB7, 0x2B, 0xEB, 0x55, 0xCF, 0x0D, 0xF7, 0x97, 0x77, 0xDC, 0x11, 0xE8, 0x7B, 0x18, 0xCC, 0xDB,
    0x90, 0x07, 0x2D, 0x9D, 0xC4, 0xAD, 0x80, 0x7C, 0x50, 0x23, 0x85, 0x46, 0xF3, 0xE9, 0x2C, 0x54,
    0x81, 0x11, 0x7B, 0x6D, 0xE2, 0x57, 0x87, 0x8E, 0x65, 0xE1, 0xD3, 0x16, 0xC4, 0x54, 0xED, 0x29,
    0xED, 0x51, 0xFD, 0xB1, 0xEF, 0xE4, 0x95, 0x01, 0x24, 0xAE, 0xC0, 0x6A, 0xFA, 0xE0, 0x5B, 0x19,
    0xD2, 0xE6, 0xF0, 0x22, 0x3B, 0xC3, 0xE7, 0xDD, 0x17, 0x1A, 0x8C, 0xF8, 0xE1, 0x02, 0x03, 0x01,
    0x00, 0x01 ]

BLOCK_CIPHER_NUM_KEYS = 16
BLOCK_CIPHER_KEY_LENGTH = 16
RSA_KEY_MESSAGE_LENGTH = 128         # The modulus of our private/public key pair for signing, verification, encryption and decryption

# encryption settings for zip header - stored in m_headerExtended struct
class EHeaderEncryptionType(Enum):
    HEADERS_NOT_ENCRYPTED = 0                  # (None)
    HEADERS_ENCRYPTED_STREAMCIPHER = 1         # (StreamCipher)
    HEADERS_ENCRYPTED_TEA = 2                  # (XXTEA) TEA = Tiny Encryption Algorithm
    HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE = 3 # (Twofish) Timur's technique. Encrypt each file and the CDR with one of 16 stream cipher keys. Encrypt the table of keys with an RSA key.
    HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2 = 4 # (Hunt) Hunt encryption

# Signature settings for zip header
class EHeaderSignatureType(Enum):
    HEADERS_NOT_SIGNED = 0
    HEADERS_CDR_SIGNED = 1 # Includes an RSA signature based on the hash of the archive's CDR. Verified in a console compatible way.
    HEADERS_CDR_SIGNED2 = 2 #

# Stores type of encryption and signing
CryCustomExtendedHeader = ('<I2H', 8)
_CCXH_HEADER_SIZE = 0                       # Size of the extended header.
_CCXH_ENCRYPTION = 1                        # Matches one of EHeaderEncryptionType: 0 = No encryption/extension
_CCXH_SIGNING = 2                           # Matches one of EHeaderSignatureType: 0 = No signing

# Header for HEADERS_SIGNED_CDR technique implemented on consoles. The comment section needs to contain the following in order:
# CryCustomExtendedHeader, CrySignedCDRHeader
CrySignedCDRHeader = ('<I128s', 4 + RSA_KEY_MESSAGE_LENGTH)
_CSCH_HEADER_SIZE = 0                       # Size of the extended header.
_CCEH_CDR_SIGNED = 1                        

CryCustomTeaEncryptionHeader = ('<I172s', 4 + 172)
_CCTEH_HEADER_SIZE = 0                      # Size of the extended header.
_CCTEH_UNKNOWN1 = 1    

# Header for HEADERS_ENCRYPTED_CRYCUSTOM technique. Paired with a CrySignedCDRHeader to allow for signing as well as encryption.
# i.e. the comment section for a file that uses this technique needs the following in order:
# CryCustomExtendedHeader, CrySignedCDRHeader, CryCustomEncryptionHeader
CryCustomEncryptionHeader = (['<3I128s2048s', '<I128s2048s'], 2180, 8)
_CCEH_HEADER_SIZE = 0                       # Size of the extended header.
_CCEH_UNKNOWN1 = 1                          # Hunt: Shadow
_CCEH_UNKNOWN2 = 2                          # Hunt: Shadow
_CCEH_CDR_IV = 3                            # Initial Vector is actually BLOCK_CIPHER_KEY_LENGTH bytes in length, but is encrypted as a RSA_KEY_MESSAGE_LENGTH byte message.
_CCEH_KEYS_TABLE = 4                        # As above, actually BLOCK_CIPHER_KEY_LENGTH but encrypted.

#endregion

#region StreamCipher

from Crypto.PublicKey import RSA
from Crypto.Cipher import AES, PKCS1_OAEP
from Crypto.Hash import BLAKE2b, SHA256, SHA1
from Crypto.Math.Numbers import Integer
import Crypto.Util.number
from Crypto.Util.number import ceil_div, bytes_to_long, long_to_bytes
from Crypto.Util.strxor import strxor
from Crypto.Cipher._pkcs1_oaep_decode import oaep_decode
from Crypto.Util.Padding import pad, unpad
from Crypto.Util import Counter
from TwoFish import TwoFish_encrypt, TwoFish_decrypt

class TwofishX:
    def __init__(self, key, mode): self.key = key.hex(); self.mode = mode
    def encrypt(self, plaintext): return bytes.fromhex(TwoFish_encrypt(plaintext.hex(), self.key, self.mode))
    def decrypt(self, ciphertext): return bytes.fromhex(TwoFish_decrypt(ciphertext.hex(), self.key, self.mode))

# see: C:\Users\smorey2\AppData\Local\Python\pythoncore-3.13-64\Lib\site-packages\Crypto\PublicKey\RSA.py
class RsaKeyX(RSA.RsaKey):
    def __init__(self, base: RsaKey):
        if not self.has_private(): super().__init__(n=base.n, e=base.e)
        else: super().__init__(n=base.n, e=base.e, d=base.d, p=base.p, q=base.q, u=base.u)
    def _decrypt_to_bytes(self, ciphertext):
        if not self.has_private(): return (pow(Integer(ciphertext), self._e, self._n) % self._n).to_bytes()
        return super()._decrypt_to_bytes(ciphertext)

# see: C:\Users\smorey2\AppData\Local\Python\pythoncore-3.13-64\Lib\site-packages\Crypto\Cipher\PKCS1_OAEP.py
class PKCS1OAEP_CipherX(PKCS1_OAEP.PKCS1OAEP_Cipher):
    def __init__(self, base: PKCS1OAEP_Cipher): super().__init__(base._key, base._hashObj, base._mgf, base._label, base._randfunc)
    def decrypt(self, ciphertext):
        # See 7.1.2 in RFC3447
        modBits = Crypto.Util.number.size(self._key.n)
        k = ceil_div(modBits, 8)            # Convert from bits to bytes
        hLen = self._hashObj.digest_size
        # Step 1b and 1c
        if len(ciphertext) != k or k < hLen+2:
            raise ValueError("Ciphertext with incorrect length.")
        # Step 2a (O2SIP)
        ct_int = bytes_to_long(ciphertext)
        # Step 2b (RSADP) and step 2c (I2OSP)
        em = self._key._decrypt_to_bytes(ct_int)
        # Step 3a
        lHash = self._hashObj.new(self._label).digest()
        # y must be 0, but we MUST NOT check it here in order not to
        # allow attacks like Manger's (http://dl.acm.org/citation.cfm?id=704143)
        maskedSeed = em[0:hLen]
        maskedDB = em[hLen:]
        # Step 3c
        seedMask = self._mgf(maskedDB, hLen)
        # Step 3d
        seed = strxor(maskedSeed, seedMask)
        # Step 3e
        dbMask = self._mgf(seed, k-hLen-1)
        # Step 3f
        db = strxor(maskedDB, dbMask)
        # Step 4
        return db[-16:]

def _segmentsOverlap(aOff: int, aLen: int, bOff: int, bLen: int) -> bool: return aLen > 0 and bLen > 0 and aOff - bOff < bLen and bOff - aOff < aLen

class BufferedBlockCipher:
    def __init__(self, cipherMode):
        if not cipherMode: raise ValueError('cipherMode')
        block_size = cipherMode.block_size
        if block_size < 1: raise ValueError('cipherMode: must have a positive block size')
        self._cipherMode = cipherMode
        self.buf = bytearray(block_size)
        self.bufOff = 0
    def init(self, forEncryption: bool, parameters: dict):
        self.forEncryption = forEncryption
        self.reset()
        self._cipherMode.init(forEncryption, parameters)
    @staticmethod
    def getFullBlocksSize(totalSize: int, blockSize: int) -> int:
        assert(blockSize > 0)
        if totalSize < 0: return 0
        blockSizeMask = blockSize - 1
        return totalSize & ~blockSizeMask if (blockSize & blockSizeMask) == 0 else totalSize - totalSize % blockSize
    def getUpdateOutputSize(self, length: int) -> int: return BufferedBlockCipher.getFullBlocksSize(self.bufOff + length, len(self.buf))
    # def processBytes(self, input: bytearray, inOff: int, length: int) -> bytes:
    #     if not input: raise ValueError('input')
    #     if length < 1: return None
    #     updateOutputSize = self.getUpdateOutputSize(length)
    #     output = bytearray(updateOutputSize) if updateOutputSize > 0 else None
    #     outLen = self.processBytes2(memoryview(input), inOff, length, memoryview(output), 0)
    #     return output[:outLen] if updateOutputSize > 0 and outLen < updateOutputSize else output
    def processBytes(self, input: memoryview, inOff: int, length: int, output: memoryview, outOff: int) -> int:
        if length < 1:
            if length < 0: raise ValueError('Can\'t have a negative input length!')
            return 0
        buf = memoryview(self.buf)
        resultLen = 0
        blockSize = len(buf)
        available = blockSize - self.bufOff
        if length >= available:
            updateOutputSize = self.getUpdateOutputSize(length)
            assert(updateOutputSize >= blockSize)
            buf[self.bufOff:self.bufOff+available] = input[inOff:inOff+available]
            inOff += available
            length -= available
            # Handle destructive overlap by copying the remaining input
            if output == input and _segmentsOverlap(outOff, blockSize, inOff, length):
                input = memoryview(bytearray(length))
                input[:length] = output[inOff:inOff+length]
                inOff = 0
            resultLen = self._cipherMode.processBlock(buf, 0, output, outOff)
            self.bufOff = 0
            while length >= blockSize:
                resultLen += self._cipherMode.processBlock(input, inOff, output, outOff + resultLen)
                inOff += blockSize
                length -= blockSize
        buf[self.bufOff:self.bufOff+length] = input[inOff:inOff+length]
        self.bufOff += length
        return resultLen
    def doFinal(self, input: bytes, inOff: int, inLen: int) -> bytes:
        if not input: raise ValueError('input')
        outputSize = self.bufOff + inLen
        if outputSize < 1: self.reset(); return b''
        input = memoryview(bytearray(input))
        output = memoryview(bytearray(outputSize))
        outLen = self.processBytes(input, inOff, inLen, output, 0) if inLen > 0 else 0
        outLen += self.doFinal2(output, outLen)
        return output[:outLen] if outLen < outputSize else output
    def doFinal2(self, output: bytearray, outOff: int) -> int:
        buf = self.buf; bufOff = self.bufOff
        if bufOff != 0:
            # NB: Can't copy directly, or we may write too much output
            self._cipherMode.processBlock(buf, 0, buf, 0)
            output[outOff:outOff+bufOff] = buf[:bufOff]
        return bufOff
        self.reset()
    def reset(self) -> None:
        self.buf[:] = [0] * len(self.buf)
        self.bufOff = 0
        self._cipherMode.reset()

class SicRevBlockCipher:
    def __init__(self, cipher):
        self.cipher = cipher
        block_size = self.block_size = 16
        self.counter = bytearray(block_size)
        self.counterOut = bytearray(block_size)
        self.iv = bytearray(block_size)
    def init(self, forEncryption: bool, parameters: dict):
        self.iv[:] = parameters['iv']
        self.reset()
    def processBlock(self, input: memoryview, inOff: int, output: memoryview, outOff: int) -> None:
        counter = self.counter; counterOut = self.counterOut
        # print('Block')
        # print(counter.hex())
        counterOut[:] = self.cipher.encrypt(counter)
        # print(counterOut.hex())
        # XOR the counterOut with the plaintext producing the cipher text
        # print(input[inOff:].hex())
        for i in range(len(counterOut)):
            output[outOff + i] = (counterOut[i] ^ input[inOff + i]) & 0xFF
        # print(output[outOff:].hex())
        # Increment the counter
        j = 0
        while j <= len(counter):
            counter[j] = (counter[j] + 1) & 0xFF
            if counter[j] != 0: break
            j += 1
        return len(counter)
    def reset(self) -> None:
        self.counter[:] = self.iv
        # self.counter[:] = [0]*len(self.counter)
        # self.counter[:len(self.iv)] = self.iv[:len(self.iv)]

def _DecryptBufferWithStreamCipher(engineId: char, data: bytes, size: int, key: bytes, iv: bytes) -> bool:
    # try:
    cipher = BufferedBlockCipher(SicRevBlockCipher(AES.new(key, AES.MODE_ECB) if engineId == 'A' else TwofishX(key, 'ECB')))
    cipher.init(False, {'iv': iv})
    data = cipher.doFinal(data, 0, size)
    # print(data.hex())
    # except: print(sys.exc_info()[1]); return None
    return data

def _GetEncryptionKeyIndex(entry: object) -> int: return (~(entry.CRC >> 2) & 0xF) & 0xF

def _GetEncryptionInitialVector(entry: object) -> bytes:
    intIV = array.array('I', [
        (entry.file_size ^ (entry.compress_size << 12)) & 0xFFFFFFFF,
        (0 if entry.compress_size != 0 else 1) & 0xFFFFFFFF,
        (entry.CRC ^ (entry.compress_size << 12)) & 0xFFFFFFFF,
        ((0 if entry.file_size != 0 else 1) ^ entry.compress_size) & 0xFFFFFFFF])
    return intIV.tobytes()

def _RsaVerifyData(data: list[bytes], sizes: list[int], numBuffers: int, signedHash: bytes, signedHashSize: int, publicKey: bytes) -> bool: return True

def _StreamCipher(data: bytes, size: int, inKey: int) -> bytes:
    raise NotImplementedError()

def _CustomRsaDecryptKeyEx(inBytes: bytes, inOff: int, inLen: int, cipher: object) -> bytes: return cipher.decrypt(inBytes[inOff:inOff+inLen])

def _GetReferenceCRCForPak() -> int: return 0

#endregion

#region TEA

TEA_DEFAULTKEY = [0xc968fb67, 0x8f9b4267, 0x85399e84, 0xf9b99dc4]
TEA_DELTA = 0x9e3779b9
TEA_DELTA2 = 0x61C88647

def _btea(v, n: int, k: list[int]) -> None:
    y = z = sum = p = rounds = e = 0
    def MX() -> int: return (((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) & 0xFFFFFFFF ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z)) & 0xFFFFFFFF) & 0xFFFFFFFF
    if n > 1: # Coding Part
        rounds = 6 + 52 // n
        sum = 0
        z = v[n - 1]
        for _ in range(rounds):
            sum += TEA_DELTA
            e = (sum >> 2) & 3
            for p in range(0, n - 1): p = _2; y = v[p + 1]; z = v[p] = (v[p] + MX()) & 0xFFFFFFFF
            p = 0
            y = v[0]
            z = v[n - 1] = (v[n - 1] + MX()) & 0xFFFFFFFF
    elif n < -1: # Decoding Part
        n = -n
        rounds = 6 + 52 // n
        sum = (rounds * TEA_DELTA) & 0xFFFFFFFF
        y = v[0]
        for _ in range(rounds):
            e = (sum >> 2) & 3
            for p in range(n - 1, 0, -1): z = v[p - 1]; y = v[p] = (v[p] - MX()) & 0xFFFFFFFF
            p = 0
            z = v[n - 1]
            y = v[0] = (v[0] - MX()) & 0xFFFFFFFF
            sum -= TEA_DELTA

def _swapByteOrder(values, count: int) -> None:
    for i in range(count):w = values[i]; values[i] = (w >> 24) | ((w >> 8) & 0xFF00) | ((w & 0xFF00) << 8) | (w << 24)

def _XXTeaDecrypt(data: bytes, size: int) -> None:
    values = ctypes.cast(ctypes.c_char_p(data), ctypes.POINTER(ctypes.c_uint32))
    encryptedLen = size >> 2
    _swapByteOrder(values, encryptedLen)
    _btea(values, -encryptedLen, TEA_DEFAULTKEY)
    _swapByteOrder(values, encryptedLen)
    return data

#endregion

#region ZipFileX
# see: https://github.com/python/cpython/tree/main/Lib/zipfile
# local: C:\Users\smorey2\AppData\Local\Python\pythoncore-3.13-64\Lib\zipfile
# local: C:\Users\smorey01\AppData\Local\Programs\Python\Python312\Lib\zipfile\__init__.py)

import zipfile_deflate64
from zipfile import crc32, ZipInfo, ZipFile, _EndRecData, sizeCentralDir, structCentralDir, stringCentralDir, stringEndArchive, stringEndArchive64, sizeEndCentDir64, sizeEndCentDir64Locator, BadZipFile, _ECD_SIGNATURE, _ECD_DISK_NUMBER, _ECD_COMMENT, _ECD_SIZE, _ECD_OFFSET, _ECD_LOCATION, _CD_SIGNATURE, _CD_FILENAME_LENGTH, _CD_FLAG_BITS, _MASK_UTF_FILENAME, _CD_EXTRA_FIELD_LENGTH, _CD_COMMENT_LENGTH, _CD_LOCAL_HEADER_OFFSET, MAX_EXTRACT_VERSION, ZIP_MAX_COMMENT
from zipfile import compressor_names, _SharedFile, sizeFileHeader, structFileHeader, stringFileHeader, ZipExtFile, _get_decompressor, _FH_SIGNATURE, _FH_FILENAME_LENGTH, _FH_EXTRA_FIELD_LENGTH, _FH_GENERAL_PURPOSE_FLAG_BITS, _MASK_ENCRYPTED, _MASK_COMPRESSED_PATCH, _MASK_STRONG_ENCRYPTION, ZIP_DEFLATED, ZIP_STORED, ZIP_BZIP2, ZIP_LZMA

p4kStringCentralDir = b"PK\x03\x04"
p4kStringCentralDirEncrypted = b"PK\x03\x14"
ZSTD93 = 93
ZIP_DEFLATE_AND_ENCRYPT = 11 # Deflate + Custom encryption (TEA)
ZIP_DEFLATE_AND_STREAMCIPHER = 12 # Deflate + stream cipher encryption on a per file basis
ZIP_STORE_AND_STREAMCIPHER_KEYTABLE = 13 # Store + Timur's encryption technique on a per file basis
ZIP_DEFLATE_AND_STREAMCIPHER_KEYTABLE = 14 # Deflate + Timur's encryption technique on a per file basis
compressor_names[ZSTD93] = '93'

if minor >= 14: from zipfile import _handle_prepended_data
else:
    def _handle_prepended_data(endrec, debug=0):
        size_cd = endrec[_ECD_SIZE]             # bytes in central directory
        offset_cd = endrec[_ECD_OFFSET]         # offset of central directory

        # "concat" is zero, unless zip was concatenated to another file
        concat = endrec[_ECD_LOCATION] - size_cd - offset_cd
        if endrec[_ECD_SIGNATURE] == stringEndArchive64:
            # If Zip64 extension structures are present, account for them
            concat -= (sizeEndCentDir64 + sizeEndCentDir64Locator)

        if debug > 2:
            inferred = concat + offset_cd
            print("given, inferred, offset", offset_cd, inferred, concat)

        return offset_cd, concat

class ZipKind(Enum): Zip = 0; Cry3 = 1; P4k = 2

def _IsZstdStream(s: bytes) -> bool: return len(s) > 3 and s[0] == 0x28 and s[1] == 0xB5 and s[2] == 0x2F and s[3] == 0xFD
def _IsAesCrypted(s) -> bool: return False #return ExtraData != null && ExtraData.Length >= 168 && ExtraData[168] > 0x00

# def _TrySkipBlockCore(kind: ZipKind, stream: io.BytesIO, blockBytes: bytes, currPosition: int) -> bool:
#     if kind == ZipKind.P4k:
#         print(blockBytes)
#         if blockBytes != p4kStringCentralDir and blockBytes != p4kStringCentralDirEncrypted: return False
#     else:
#         if blockBytes != stringCentralDir: return False
#     if len(stream) < currPosition + 26: return False
#     stream.seek(26 - 4, 1) # Already read the signature, so make the filename length field location relative to that
#     return True

# def _TrySkipBlock(kind: ZipKind, stream: object) -> bool:
#     currPosition = stream.tell(); blockBytes = stream.read(4)
#     print(currPosition, 1054908416)
#     exit(1)
#     if not _TrySkipBlockCore(kind, stream, blockBytes, currPosition): return False
#     blockBytes = stream.read(4)
#     # return TrySkipBlockFinalize(stream, blockBytes, bytesRead);

class NoneDecompressor:
    def __init__(self): self.eof = False
    def decompress(self, data): self.eof = True; return data

class ZStdWrapDecompressor:
    def __init__(self): self._decomp = ZstdDecompressor(); self.eof = False
    def decompress(self, data): data = self._decomp.decompress(data); self.eof = True; return data

def _get_decompressorX(compress_type, stream):
    if compress_type == ZSTD93:
        if stream.shape[0] > 0:
            if _IsZstdStream(stream[0:4]): return ZStdWrapDecompressor()
            else: return NoneDecompressor()
        return ZStdWrapDecompressor()
    return _get_decompressor(compress_type)

DEFAULT_CUSTOMIV = bytes([0x70, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0])
EndOfCentralRecordBaseSize = 22
class ZipFileX(ZipFile):
    _encryptedHeaders: EHeaderEncryptionType = EHeaderEncryptionType.HEADERS_NOT_ENCRYPTED
    _signedHeaders: EHeaderSignatureType = EHeaderSignatureType.HEADERS_NOT_SIGNED
    _headerSignature = [0, None]
    _headerExtended = [0, 0, 0]
    _headerEncryption = [0, 0, 0, None, None]
    _headerTeaEncryption = [0, None]
    _customIV: bytes = None
    _customKeys: list[bytes] = None
    _offsetOfFirstEntry: int = 0
    def __init__(self, file: object, mode: str='r', path: str=None, key: bytes=None, kind: ZipKind = None):
        self._path = path; z = path.lower()
        self._kind = kind or (ZipKind.P4k if z.endswith('.p4k') else ZipKind.Cry3 if z.endswith('.pak') else ZipKind.Zip)
        self._key = key
        super().__init__(file, mode)
         

    # ZipDirCacheFactory::Prepare
    def _prepare(self, endrec):
        fp = self.fp
        # Earlier pak file encryption techniques stored the encryption type in the disk number of the CDREnd.
        # This works, but can't be used by the more recent techniques that require signed paks to be readable by 7-Zip during dev.
        headerEnc = EHeaderEncryptionType((endrec[_ECD_DISK_NUMBER] & 0xC000) >> 14)
        if headerEnc == EHeaderEncryptionType.HEADERS_ENCRYPTED_TEA or headerEnc == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER: self._encryptedHeaders = headerEnc
        endrec[_ECD_DISK_NUMBER] &= 0x3fff

        # Pak may be encrypted with CryCustom technique and/or signed. Being signed is compatible (in principle) with the earlier encryption methods.
        # The information for this exists in some custom headers at the end of the archive (in the comment section)
        commentSize = len(self._comment)
        if commentSize >= CryCustomExtendedHeader[1]:
            fp.seek(endrec[_ECD_OFFSET] + endrec[_ECD_SIZE] + EndOfCentralRecordBaseSize)
            self._headerExtended = struct.unpack(CryCustomExtendedHeader[0], fp.read(CryCustomExtendedHeader[1]))
            if self._headerExtended[_CCXH_HEADER_SIZE] != CryCustomExtendedHeader[1]: raise BadZipFile("Bad extended header")

            # We have the header, so read the encryption and signing techniques
            self._signedHeaders = EHeaderSignatureType(self._headerExtended[_CCXH_SIGNING])

            # Prepare for a quick sanity check on the size of the comment field now that we know what it should contain
            # Also check that the techniques are supported
            expectedCommentLength = CryCustomExtendedHeader[1]

            # Encryption technique has been specified in both the disk number (old technique) and the custom header (new technique).
            if self._encryptedHeaders != EHeaderEncryptionType.HEADERS_ENCRYPTED_TEA and self._headerExtended[_CCXH_ENCRYPTION] != EHeaderEncryptionType.HEADERS_NOT_ENCRYPTED.value and self._encryptedHeaders != EHeaderEncryptionType.HEADERS_NOT_ENCRYPTED: raise BadZipFile("Unexpected encryption technique in header")
            else:
                # The encryption technique has been specified only in the custom header
                self._encryptedHeaders = EHeaderEncryptionType(self._headerExtended[_CCXH_ENCRYPTION])
                match self._encryptedHeaders:
                    case EHeaderEncryptionType.HEADERS_NOT_ENCRYPTED: pass
                    case EHeaderEncryptionType.HEADERS_ENCRYPTED_TEA | EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE | EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2:
                        if self._encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_TEA: expectedCommentLength += CryCustomTeaEncryptionHeader[1]
                        hasSize2 = self._encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2
                        expectedCommentLength += CryCustomEncryptionHeader[1]
                        if hasSize2: expectedCommentLength += CryCustomEncryptionHeader[2]
                    case _: raise BadZipFile("Unexpected encryption technique in header")

            # Add the signature header to the expected size
            match self._signedHeaders:
                case EHeaderSignatureType.HEADERS_NOT_SIGNED: pass
                case EHeaderSignatureType.HEADERS_CDR_SIGNED | EHeaderSignatureType.HEADERS_CDR_SIGNED2: expectedCommentLength += CrySignedCDRHeader[1]
                case _: raise BadZipFile("Bad signing technique in header")
            
            if commentSize == expectedCommentLength:
                if self._signedHeaders == EHeaderSignatureType.HEADERS_CDR_SIGNED or self._signedHeaders == EHeaderSignatureType.HEADERS_CDR_SIGNED2:
                    self._headerSignature = struct.unpack(CrySignedCDRHeader[0], fp.read(CrySignedCDRHeader[1]))
                    if self._headerSignature[_CSCH_HEADER_SIZE] != CrySignedCDRHeader[1]: raise BadZipFile("Bad signature header")
                if self._encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_TEA:
                    self._headerTeaEncryption = struct.unpack(CryCustomTeaEncryptionHeader[0], fp.read(CryCustomTeaEncryptionHeader[1]))
                    if self._headerTeaEncryption[_CCTEH_HEADER_SIZE] != CryCustomTeaEncryptionHeader[1] + CryCustomEncryptionHeader[1]: raise BadZipFile("Bad encryption header")
                    self._encryptedHeaders = EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE
                if self._encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE or self._encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2:
                    hasSize2 = self._encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2
                    self._headerEncryption = struct.unpack(CryCustomEncryptionHeader[0][0] if hasSize2 else CryCustomEncryptionHeader[0][1], fp.read(CryCustomEncryptionHeader[1] + (CryCustomEncryptionHeader[2] if hasSize2 else 0)))
                    if not hasSize2: self._headerEncryption = (self._headerEncryption[0], 0, 0, self._headerEncryption[1], self._headerEncryption[2])
                    if self._headerEncryption[_CCEH_HEADER_SIZE] != 0 and self._headerEncryption[_CCEH_HEADER_SIZE] != CryCustomEncryptionHeader[1] + (CryCustomEncryptionHeader[2] if hasSize2 else 0): raise BadZipFile("Bad encryption header")
                    # We have a table of symmetric keys to decrypt
                    self._decryptKeysTable()
            else: raise BadZipFile("Comment field is the wrong length")

        # HACK: Hardcoded check for PAK location before enforcing encryption requirement. For C2 Mod SDK Release.
        if self._encryptedHeaders == EHeaderEncryptionType.HEADERS_NOT_ENCRYPTED:
            if _GetReferenceCRCForPak() != 0: self._encryptedHeaders = EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER

    def _trySfxEmbedded(self, s, eocdStart: int) -> None:
        # SFX/embedded support, find the offset of the first entry vis the start of the stream
        # This applies to Zip files that are appended to the end of an SFX stub.
        # Or are appended as a resource to an executable.
        # Zip files created by some archivers have the offsets altered to reflect the true offsets
        # and so dont require any adjustment here...
        # TODO: Difficulty with Zip64 and SFX offset handling needs resolution - maths?
        isZip64 = s[_CD_SIGNATURE] == stringEndArchive64
        if False and not isZip64 and (s[_ECD_OFFSET] < eocdStart - (4 + s[_ECD_SIZE])):
            self._offsetOfFirstEntry = eocdStart - (4 + s[_ECD_SIZE] + s[_ECD_OFFSET])
            if self._offsetOfFirstEntry <= 0: raise BadZipFile("Invalid embedded zip archive")

    # ZipDirCacheFactory::ReadHeaderData
    def _readHeaderData(self, s) -> bool:
        fp = self.fp
        nSize = s[_ECD_SIZE]
        fp.seek(self._offsetOfFirstEntry + s[_ECD_OFFSET], 0)
        if self._encryptedHeaders != EHeaderEncryptionType.HEADERS_NOT_ENCRYPTED:
            data = fp.read(nSize)
            match self._encryptedHeaders:
                case EHeaderEncryptionType.HEADERS_ENCRYPTED_TEA: data = _XXTeaDecrypt(data, nSize)
                case EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER: data = _StreamCipher(data, nSize, GetReferenceCRCForPak())
                case EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE | EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2:
                    engineId = 'A' if self._encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2 else '2'
                    iv = DEFAULT_CUSTOMIV if self._headerTeaEncryption[_CCTEH_HEADER_SIZE] != 0 else self._customIV
                    if not (data := _DecryptBufferWithStreamCipher(engineId, data, nSize, self._customKeys[0], iv)): print('Failed to decrypt pak header'); return False
                case _: print('Attempting to load encrypted pak by unsupported method'); return False
            fp = self.fp = io.BytesIO(data)
            self.start_dir = 0
        match self._signedHeaders:
            case EHeaderSignatureType.HEADERS_CDR_SIGNED | EHeaderSignatureType.HEADERS_CDR_SIGNED2:
                if self._path == None: return True
                # Verify CDR signature & pak name
                pathSepIdx = max(self._path.rfind('\\'), self._path.rfind('/'))
                pathSep = self._path[pathSepIdx + 1:]
                position = fp.tell(); data = bytearray(nSize); fp.readinto(data); fp.seek(position, 0)
                dataToVerify = [data, pathSep.encode('ascii')]
                sizesToVerify = [nSize, len(pathSep)]
                # Could not verify signature
                if not _RsaVerifyData(dataToVerify, sizesToVerify, 2, self._headerSignature[_CCEH_CDR_SIGNED], 128, self._key): print('Failed to verify RSA signature of pak header'); return False
            case EHeaderSignatureType.HEADERS_NOT_SIGNED: pass
        return True

    # ZipDirCacheFactory::DecryptKeysTable
    def _decryptKeysTable(self):
        digestSize = 1 if self._headerTeaEncryption[_CCTEH_HEADER_SIZE] != 0 else 256
        rsaKey = RsaKeyX(RSA.importKey(self._key or DefaultRsaKey))
        hashAlgo = BLAKE2b if digestSize == 257 else SHA256 if digestSize == 256 else SHA1
        cipher = PKCS1OAEP_CipherX(PKCS1_OAEP.new(rsaKey, hashAlgo=hashAlgo))

        # Decrypt CDR initial Vector
        self._customIV = _CustomRsaDecryptKeyEx(self._headerEncryption[_CCEH_CDR_IV], 0, RSA_KEY_MESSAGE_LENGTH, cipher)

        # Decrypt the table of cipher keys.
        self._customKeys = [bytes()]*BLOCK_CIPHER_NUM_KEYS
        offset = 0
        for i in range(BLOCK_CIPHER_NUM_KEYS):
            self._customKeys[i] = _CustomRsaDecryptKeyEx(self._headerEncryption[_CCEH_KEYS_TABLE], offset, RSA_KEY_MESSAGE_LENGTH, cipher)
            offset += RSA_KEY_MESSAGE_LENGTH

    @staticmethod
    def seekBackwardsAndRead(stream: BufferedReader, buffer: memoryview, overlap: int)-> int:
        bytesRead = 0; bufferLength = len(buffer)
        if stream.tell() >= bufferLength:
            assert(overlap <= bufferLength)
            stream.seek(-(bufferLength - overlap), 1)
            bytesRead = stream.readinto(buffer)
            stream.seek(-bufferLength, 1)
        else:
            bytesToRead = stream.tell()
            stream.seek(0, 0)
            bytesRead = stream.readinto(buffer)
            stream.seek(0, 0)
        return bytesRead

    @staticmethod
    def seekBackwardsToSignature(stream: io.BytesIO, signatureToFind: bytes, maxBytesToRead: int) -> bool:
        assert(len(signatureToFind) != 0 and maxBytesToRead > 0)

        # This method reads blocks of BackwardsSeekingBufferSize bytes, searching each block for signatureToFind.
        # A simple LastIndexOf(signatureToFind) doesn't account for cases where signatureToFind is split, starting in
        # one block and ending in another.
        # To account for this, we read blocks of BackwardsSeekingBufferSize bytes, but seek backwards by
        # [4096 - signatureToFind.Length] bytes. This guarantees that signatureToFind will not be
        # split between two consecutive blocks, at the cost of reading [signatureToFind.Length] duplicate bytes in each iteration.
        bufferPointer = 0; buffer = bytearray(4096); bufferSpan = memoryview(buffer)

        outOfBytes = False; signatureFound = False; totalBytesRead = 0
        while not signatureFound and not outOfBytes and totalBytesRead < maxBytesToRead:
            overlap = 0 if totalBytesRead == 0 else len(signatureToFind)
            if maxBytesToRead - totalBytesRead + overlap < len(bufferSpan): bufferSpan = bufferSpan[:maxBytesToRead - totalBytesRead + overlap]
            bytesRead = ZipFileX.seekBackwardsAndRead(stream, bufferSpan, overlap)
            outOfBytes = bytesRead < len(bufferSpan)
            if bytesRead < len(bufferSpan): bufferSpan = bufferSpan[:bytesRead]
            bufferPointer = bytes(bufferSpan).rfind(signatureToFind)
            assert(bufferPointer < len(bufferSpan))
            totalBytesRead += bytesRead - overlap
            if bufferPointer != -1: signatureFound = True; break;

        if not signatureFound: return False
        else: stream.seek(bufferPointer, 1); return True

    def _RealGetContents(self):
        """Read in the table of contents for the ZIP file."""
        if self._kind == ZipKind.Zip: return super()._RealGetContents()
        self.debug = 0
        fp = self.bfp = self.fp
        try: endrec = _EndRecData(fp)
        except OSError: raise BadZipFile("File is not a zip file")
        if not endrec: raise BadZipFile("File is not a zip file")
        if self.debug > 1: print(endrec)
        self._comment = endrec[_ECD_COMMENT] # archive comment

        # for trySfxEmbedded
        fp.seek(-18, 2)
        if not ZipFileX.seekBackwardsToSignature(fp, stringEndArchive, ZIP_MAX_COMMENT + 4): raise BadZipFile('File is not a zip file')
        eocdStart = fp.tell()

        if self._kind == ZipKind.Cry3: self._prepare(endrec)

        offset_cd, concat = _handle_prepended_data(endrec, self.debug)

        # position of start of central directory
        self.start_dir = offset_cd + concat

        self._trySfxEmbedded(endrec, eocdStart)
        if self._kind == ZipKind.Cry3: self._readHeaderData(endrec)
        fp = self.fp

        if self.start_dir < 0: raise BadZipFile('Bad offset for central directory')

        fp.seek(self.start_dir, 0)
        size_cd = endrec[_ECD_SIZE]
        data = fp.read(size_cd)
        fp = io.BytesIO(data)
        total = 0
        while total < size_cd:
            centdir = fp.read(sizeCentralDir)
            if len(centdir) != sizeCentralDir: raise BadZipFile('Truncated central directory')
            centdir = struct.unpack(structCentralDir, centdir)
            if centdir[_CD_SIGNATURE] != stringCentralDir: raise BadZipFile('Bad magic number for central directory')
            if self.debug > 2: print(centdir)
            filename = fp.read(centdir[_CD_FILENAME_LENGTH])
            orig_filename_crc = crc32(filename)
            flags = centdir[_CD_FLAG_BITS]
            # UTF-8 file names extension
            if flags & _MASK_UTF_FILENAME: filename = filename.decode('utf-8')
            # Historical ZIP filename encoding
            else: filename = filename.decode(self.metadata_encoding or 'cp437')
            # Create ZipInfo instance to store file information
            x = ZipInfo(filename)
            x.extra = fp.read(centdir[_CD_EXTRA_FIELD_LENGTH])
            x.comment = fp.read(centdir[_CD_COMMENT_LENGTH])
            x.header_offset = centdir[_CD_LOCAL_HEADER_OFFSET]
            (x.create_version, x.create_system, x.extract_version, x.reserved,
             x.flag_bits, x.compress_type, t, d,
             x.CRC, x.compress_size, x.file_size) = centdir[1:12]
            if x.extract_version > MAX_EXTRACT_VERSION: raise NotImplementedError('zip file version %.1f' % (x.extract_version / 10))
            x.volume, x.internal_attr, x.external_attr = centdir[15:18]
            # Convert date/time code to (year, month, day, hour, min, sec)
            x._raw_time = t
            x.date_time = ((d>>9)+1980, (d>>5)&0xF, d&0x1F, t>>11, (t>>5)&0x3F, (t&0x1F) * 2)
            if self._kind != ZipKind.P4k: x._decodeExtra(orig_filename_crc)
            x.header_offset = x.header_offset + concat
            self.filelist.append(x)
            self.NameToInfo[x.filename] = x
            # update total bytes read from central directory
            total = (total + sizeCentralDir + centdir[_CD_FILENAME_LENGTH] + centdir[_CD_EXTRA_FIELD_LENGTH] + centdir[_CD_COMMENT_LENGTH])
            if self.debug > 2: print("total", total)

        end_offset = self.start_dir
        for zinfo in reversed(sorted(self.filelist, key=lambda zinfo: zinfo.header_offset)): zinfo._end_offset = end_offset; end_offset = zinfo.header_offset

    def open(self, name, mode="r", pwd=None, *, force_zip64=False):
        """Return file-like object for 'name'.

        name is a string for the file name within the ZIP file, or a ZipInfo
        object.

        mode should be 'r' to read a file already in the ZIP file, or 'w' to
        write to a file newly added to the archive.

        pwd is the password to decrypt files (only used for reading).

        When writing, if the file size is not known in advance but may
        exceed 2 GiB, pass force_zip64 to use the ZIP64 format, which can
        handle large files.  If the size is known in advance, it is best to
        pass a ZipInfo instance for name, with zinfo.file_size set.
        """
        kind = self._kind
        if kind == ZipKind.Zip: return super().open(name, mode=mode, pwd=pwd, force_zip64=force_zip64) # , allowZip64=True
        if mode not in {'r', 'w'}: raise ValueError('open() requires mode "r" or "w"')
        if pwd and (mode == 'w'): raise ValueError('pwd is only supported for reading files')
        if not self.fp: raise ValueError('Attempt to use ZIP archive that was already closed')

        # Make sure we have an info object
        if isinstance(name, ZipInfo): zinfo = name # 'name' is already an info object
        elif mode == 'w': zinfo = ZipInfo(name); zinfo.compress_type = self.compression; zinfo.compress_level = self.compresslevel
        else: zinfo = self.getinfo(name) # Get info object for name

        if mode == 'w': return self._open_to_write(zinfo, force_zip64=force_zip64)

        if self._writing: raise ValueError('Can\'t read from the ZIP file while there is an open writing handle on it. Close the writing handle before trying to read.')

        # pre-flags
        fp = self.bfp; fileOffset = self._offsetOfFirstEntry + zinfo.header_offset
        compress_type = zinfo.compress_type; compressed = None
        zipTest = True

        # pre-decompress
        if self._encryptedHeaders != EHeaderEncryptionType.HEADERS_NOT_ENCRYPTED:
            fileOffset += sizeFileHeader + len(zinfo.filename)
            zipTest = False
            if kind == ZipKind.Cry3 and ((compress_type >= ZIP_DEFLATE_AND_ENCRYPT and compress_type <= ZIP_DEFLATE_AND_STREAMCIPHER_KEYTABLE) or self._encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2):
                teaEncryption = self._headerTeaEncryption[_CCTEH_HEADER_SIZE] != 0
                fp.seek(fileOffset, 0)
                compressed = fp.read(zinfo.compress_size)
                if compress_type == ZIP_STORE_AND_STREAMCIPHER_KEYTABLE:
                    compressed = _StreamCipher(compressed, 0)
                    zinfo.compress_type = ZIP_STORED
                elif compress_type == ZIP_DEFLATE_AND_ENCRYPT or compress_type == ZIP_DEFLATE_AND_STREAMCIPHER or compress_type == ZIP_DEFLATE_AND_STREAMCIPHER_KEYTABLE:
                    if compress_type == ZIP_DEFLATE_AND_ENCRYPT and not teaEncryption:
                        compressed = _XXTeaDecrypt(compressed, zinfo.compress_size)
                        zinfo.compress_type = ZIP_DEFLATED
                    else:
                        keyIndex = _GetEncryptionKeyIndex(zinfo)
                        iv = _GetEncryptionInitialVector(zinfo)
                        if not (compressed := _DecryptBufferWithStreamCipher('2', compressed, zinfo.compress_size, self._customKeys[keyIndex], iv)): raise BadZipFile('Data is corrupt')
                        zinfo.compress_type = ZIP_STORED if teaEncryption and compress_type == ZIP_DEFLATE_AND_STREAMCIPHER else ZIP_DEFLATED
                else:
                    if self._encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2:
                        keyIndex = _GetEncryptionKeyIndex(zinfo)
                        iv = _GetEncryptionInitialVector(zinfo)
                        if not (compressed := _DecryptBufferWithStreamCipher('A', compressed, zinfo.compress_size, self._customKeys[keyIndex], iv)): raise BadZipFile('Data is corrupt')
                fp = io.BytesIO(compressed)
                fileOffset = 0

        # Open for reading:
        self._fileRefCnt += 1
        zef_file = _SharedFile(fp, fileOffset, self._fpclose, self._lock, lambda: self._writing)
        if zipTest:
            try:
                # Skip the file header:
                fheader = zef_file.read(sizeFileHeader)
                print(fheader)
                if len(fheader) != sizeFileHeader: raise BadZipFile('Truncated file header')
                fheader = struct.unpack(structFileHeader, fheader)
                if kind == ZipKind.P4k:
                    if fheader[_FH_SIGNATURE] != p4kStringCentralDir and fheader[_FH_SIGNATURE] != p4kStringCentralDirEncrypted: raise BadZipFile('Bad magic number for central directory')
                else:
                    if fheader[_FH_SIGNATURE] != stringFileHeader: raise BadZipFile('Bad magic number for file header')
                fname = zef_file.read(fheader[_FH_FILENAME_LENGTH])
                if self._encryptedHeaders != EHeaderEncryptionType.HEADERS_NOT_ENCRYPTED and fheader[_FH_EXTRA_FIELD_LENGTH]: zef_file.seek(fheader[_FH_EXTRA_FIELD_LENGTH], whence=1)
                if zinfo.flag_bits & _MASK_COMPRESSED_PATCH: raise NotImplementedError('compressed patched data (flag bit 5)') # Zip 2.7: compressed patched data
                if zinfo.flag_bits & _MASK_STRONG_ENCRYPTION: raise NotImplementedError('strong encryption (flag bit 6)') # strong encryption
                if fheader[_FH_GENERAL_PURPOSE_FLAG_BITS] & _MASK_UTF_FILENAME: fname_str = fname.decode('utf-8') # UTF-8 filename
                else: fname_str = fname.decode(self.metadata_encoding or 'cp437')
                
                # if fname_str != zinfo.orig_filename: raise BadZipFile('File name in directory %r and header %r differ.' % (zinfo.orig_filename, fname))
                # if (zinfo._end_offset is not None and zef_file.tell() + zinfo.compress_size > zinfo._end_offset):
                #     if zinfo._end_offset == zinfo.header_offset:
                #         import warnings
                #         warnings.warn(f'Overlapped entries: {zinfo.orig_filename!r} (possible zip bomb)', skip_file_prefixes=(os.path.dirname(__file__),))
                #     else: raise BadZipFile(f'Overlapped entries: {zinfo.orig_filename!r} (possible zip bomb)')

                # check for encrypted flag & handle password
                is_encrypted = zinfo.flag_bits & _MASK_ENCRYPTED
                if is_encrypted:
                    if not pwd: pwd = self.pwd
                    if pwd and not isinstance(pwd, bytes): raise TypeError('pwd: expected bytes, got %s' % type(pwd).__name__)
                    if not pwd: raise RuntimeError('File %r is encrypted, password required for extraction' % name)
                else: pwd = None
            except:
                zef_file.close()
                raise

        # check for encrypted flag & handle password
        is_encrypted = zinfo.flag_bits & _MASK_ENCRYPTED
        # if is_encrypted: compressedStream = _archive.CreateAndInitDecryptionStream(compressedStream, this_) #or raise BadZipFile('Unable to decrypt this entry')
        # if kind == ZipKind.P4k and _IsAesCrypted(zinfo.extra): compressedStream = _archive.CreateAndInitAesDecryptionStream(compressedStream, this_) #or raise('Unable to decrypt this entry')

        # ZipExtFile
        if kind == ZipKind.P4k and compress_type == 100: zef_file.seek(4096); compress_type = ZSTD93; compressed = memoryview(zef_file._file.read(4)) #; zef_file.seek(-4, whence=1)
        if compress_type in { ZIP_STORED, ZIP_DEFLATED, ZIP_BZIP2, ZIP_LZMA }: res = ZipExtFile(zef_file, mode + 'b', zinfo, self.pwd, True)
        else:
            zinfo.compress_type = ZIP_STORED
            res = ZipExtFile(zef_file, mode + 'b', zinfo, self.pwd, True)
            res._compress_type = zinfo.compress_type = compress_type
            res._decompressor = _get_decompressorX(compress_type, compressed)
        res._expected_crc = None
        return res

#endregion

#region Not Used

    # def _GetOffsetOfCompressedData(self, zinfo) -> int:
    #     if self._kind == ZipKind.P4k:
    #         fp = self.bfp
    #         fp.seek(zinfo.header_offset, 0)
    #         # by calling this, we are using local header _storedEntryNameBytes.Length and extraFieldLength
    #         # to find start of data, but still using central directory size information
    #         if not _TrySkipBlock(self._kind, fp): raise BadZipFile('Local File Header Corrupt')
    #         return fp.tell()
    #     elif self._encryptedHeaders == EHeaderEncryptionType.HEADERS_NOT_ENCRYPTED: return zinfo.header_offset + sizeFileHeader + len(zinfo.filename) + len(zinfo.extra)
    #     # use CDR instead of local header
    #     # The pak encryption tool asserts that there is no extra data at the end of the local file header, so don't add any extra data from the CDR header.
    #     else: return self._offsetOfFirstEntry + zinfo.header_offset + sizeFileHeader + len(zinfo.filename)


        # Open for reading:
        # fileOffset = self._GetOffsetOfCompressedData(zinfo)
        # print(fileOffset, 1054912512)
        # exit(1)

        # # pre-decompress
        # fileOffset = self.bfp.tell()
        # method = zinfo.compress_type; fp = self.bfp; compressed = None
        # if (method >= ZIP_DEFLATE_AND_ENCRYPT and method <= ZIP_DEFLATE_AND_STREAMCIPHER_KEYTABLE) or self._encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2:
        #     teaEncryption = self._headerTeaEncryption[_CCTEH_HEADER_SIZE] != 0
        #     # self.bfp.seek(fileOffset, 0)
        #     compressed = zef_file.read(zinfo.compress_size)
        #     print(compressed)
        #     if method == ZIP_STORE_AND_STREAMCIPHER_KEYTABLE:
        #         compressed = _StreamCipher(compressed, 0)
        #         zinfo.compress_type = ZIP_STORED
        #     elif method == ZIP_DEFLATE_AND_ENCRYPT or method == ZIP_DEFLATE_AND_STREAMCIPHER or method == ZIP_DEFLATE_AND_STREAMCIPHER_KEYTABLE:
        #         if method == ZIP_DEFLATE_AND_ENCRYPT and not teaEncryption:
        #             compressed = _XXTeaDecrypt(compressed, zinfo.compress_size)
        #             zinfo.compress_type = ZIP_DEFLATED
        #         else:
        #             keyIndex = _GetEncryptionKeyIndex(zinfo)
        #             iv = _GetEncryptionInitialVector(zinfo)
        #             if not (compressed := _DecryptBufferWithStreamCipher('2', compressed, zinfo.compress_size, self._customKeys[keyIndex], iv)): raise BadZipFile('Data is corrupt')
        #             zinfo.compress_type = ZIP_STORED if teaEncryption and method == ZIP_DEFLATE_AND_STREAMCIPHER else ZIP_DEFLATED
        #     else:
        #         if self._encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2:
        #             keyIndex = _GetEncryptionKeyIndex(zinfo)
        #             iv = _GetEncryptionInitialVector(zinfo)
        #             if not (compressed := _DecryptBufferWithStreamCipher('A', compressed, zinfo.compress_size, self._customKeys[keyIndex], iv)): raise BadZipFile('Data is corrupt')
        #     fp = io.BytesIO(compressed)
        #     fileOffset = 0
        # elif self._kind == ZipKind.P4k and method == 100: method = zinfo.compress_type = ZSTD93; compressed = memoryview(fp.read(4)); fp.seek(-4, 1)
        
        # # Open for reading:
        # # self._fileRefCnt += 1
        # # zef_file = _SharedFile(fp, fileOffset,
        # #                     self._fpclose, self._lock, lambda: self._writing)
        


# class ZipExtFileX(ZipExtFile):
#     def __init__(self, fileobj, mode, zipinfo, pwd=None, close_fileobj=False): super().__init__(fileobj, mode, zipinfo, pwd, close_fileobj)

    # def read(self, n=-1):
    #     """Read and return up to n bytes.
    #     If the argument is omitted, None, or negative, data is read and returned until EOF is reached.
    #     """
    #     super().read(n)

    # def _read1x(self, n):
    #     # Read up to n compressed bytes with at most one read() system call,
    #     # decrypt and decompress them.
    #     if self._eof or n <= 0:
    #         return b''

    #     # Read from file.
    #     # print(compressor_names.get(self._compress_type))
    #     # print(self._decompressor)
    #     if self._compress_type == ZIP_DEFLATED:
    #         ## Handle unconsumed data.
    #         data = self._decompressor.unconsumed_tail
    #         if n > len(data):
    #             data += self._read2(n - len(data))
    #     else:
    #         data = self._read2(n)

    #     if self._compress_type == ZIP_STORED:
    #         self._eof = self._compress_left <= 0
    #     elif self._compress_type == ZIP_DEFLATED:
    #         n = max(n, self.MIN_READ_SIZE)
    #         data = self._decompressor.decompress(data, n)
    #         self._eof = (self._decompressor.eof or
    #                      self._compress_left <= 0 and
    #                      not self._decompressor.unconsumed_tail)
    #         if self._eof:
    #             data += self._decompressor.flush()
    #     else:
    #         data = self._decompressor.decompress(data)
    #         self._eof = self._decompressor.eof or self._compress_left <= 0

    #     data = data[:self._left]
    #     self._left -= len(data)
    #     if self._left <= 0:
    #         self._eof = True
    #     self._update_crc(data)
    #     return data

#endregion