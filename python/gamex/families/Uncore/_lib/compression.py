from __future__ import annotations
import sys, io, struct
from enum import Enum

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

METHOD_DEFLATE_AND_ENCRYPT = 11 # Deflate + Custom encryption (TEA)
METHOD_DEFLATE_AND_STREAMCIPHER = 12 # Deflate + stream cipher encryption on a per file basis
METHOD_STORE_AND_STREAMCIPHER_KEYTABLE = 13 # Store + Timur's encryption technique on a per file basis
METHOD_DEFLATE_AND_STREAMCIPHER_KEYTABLE = 14 # Deflate + Timur's encryption technique on a per file basis

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
        buf = self.buf
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
        input[inOff:inOff+length] = buf[self.bufOff:self.bufOff+length]
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
        print(f'doFinal2 {bufOff}: {buf.hex()}')
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

def _GetEncryptionKeyIndex(entry: object) -> int: return 0 # => (int)unchecked(~(entry.Crc32 >> 2) & 0xF)

def _GetEncryptionInitialVector(entry: object) -> bytes:
    # unchecked {
    #     var intIV = new[] {
    #         (uint)(entry.Length ^ (entry.CompressedLength << 12)),
    #         (uint)(entry.CompressedLength != 0 ? 0 : 1),
    #         (uint)(entry.Crc32 ^ (entry.CompressedLength << 12)),
    #         (uint)((entry.Length != 0 ? 0 : 1) ^ entry.CompressedLength)};
    #     iv = new byte[16];
    #     fixed (uint* ptr = intIV) Marshal.Copy((IntPtr)ptr, iv, 0, 16);
    # }
    return None

def _RsaVerifyData(data: list[bytes], sizes: list[int], numBuffers: int, signedHash: bytes, signedHashSize: int , publicKey: bytes) -> bool: return True

def _StreamCipher(data: bytes, size: int, inKey: int) -> bytes:
    pass

def _CustomRsaDecryptKeyEx(inBytes: bytes, inOff: int, inLen: int, cipher: object) -> bytes: return cipher.decrypt(inBytes[inOff:inOff+inLen])

def _GetReferenceCRCForPak() -> int: return 0

#endregion

#region TEA

TEA_DEFAULTKEY = [0xc968fb67, 0x8f9b4267, 0x85399e84, 0xf9b99dc4]
TEA_DELTA = 0x9e3779b9
TEA_DELTA2 = 0x61C88647

def btea(v: object, n: int, k: list[int]) -> None:
    y, z, sum
    p, rounds, e
    def MX() -> int: return ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z)) & 0xFFFFFFFF

    if n > 1: # Coding Part
        rounds = 6 + 52 // n
        sum = 0
        z = v[n - 1]
        for _ in range(rounds):
            sum += TEA_DELTA
            e = (sum >> 2) & 3
            for p in range(n - 1):
                y = v[p + 1]
                z = v[p] = v[p] + MX()
            y = v[0]
            z = v[n - 1] = v[n - 1] + MX()
    elif n < -1: # Decoding Part
        n = -n
        rounds = 6 + 52 // n
        sum = rounds * TEA_DELTA
        y = v[0]
        for _ in range(rounds):
            e = (sum >> 2) & 3
            for p in range(n - 1, -1):
                z = v[p - 1]
                y = v[p] = v[p] - MX()
            z = v[n - 1];
            y = v[0] = v[0] - MX()
            sum -= TEA_DELTA

# static void SwapByteOrder(uint* values, int count) {
#     for (uint* w = values, e = values + count; w != e; ++w) *w = (*w >> 24) | ((*w >> 8) & 0xff00) | ((*w & 0xff00) << 8) | (*w << 24);
# }

# internal static void XXTeaEncrypt(ref byte[] data, int size) {
#     fixed (byte* dataPtr = data) {
#         var intBuffer = (uint*)dataPtr;
#         var encryptedLen = size >> 2;
#         SwapByteOrder(intBuffer, encryptedLen);
#         Btea(intBuffer, encryptedLen, TEA_DEFAULTKEY);
#         SwapByteOrder(intBuffer, encryptedLen);
#     }
# }

# internal static void XXTeaDecrypt(ref byte[] data, int size) {
#     fixed (byte* dataPtr = data) {
#         var intBuffer = (uint*)dataPtr;
#         var encryptedLen = size >> 2;
#         SwapByteOrder(intBuffer, encryptedLen);
#         Btea(intBuffer, -encryptedLen, TEA_DEFAULTKEY);
#         SwapByteOrder(intBuffer, encryptedLen);
#     }
# }

#endregion

#region ZipFileX
# see: https://github.com/python/cpython/tree/main/Lib/zipfile
# local: C:\Users\smorey2\AppData\Local\Python\pythoncore-3.13-64\Lib\zipfile
# local: C:\Users\smorey01\AppData\Local\Programs\Python\Python312\Lib\zipfile\__init__.py)

from zipfile import crc32, ZipInfo, ZipFile, _EndRecData, _ECD_SIGNATURE, _ECD_DISK_NUMBER, _ECD_COMMENT, _ECD_SIZE, _ECD_OFFSET, _ECD_LOCATION, _CD_SIGNATURE, _CD_FILENAME_LENGTH, _CD_FLAG_BITS, _MASK_UTF_FILENAME, _CD_EXTRA_FIELD_LENGTH, _CD_COMMENT_LENGTH, _CD_LOCAL_HEADER_OFFSET, MAX_EXTRACT_VERSION, ZIP_MAX_COMMENT, sizeCentralDir, structCentralDir, stringCentralDir, stringEndArchive, stringEndArchive64, sizeEndCentDir64, sizeEndCentDir64Locator, BadZipFile

p4kStringCentralDir = b"PK\x03\x04"
p4kStringCentralDirEncrypted = b"PK\x03\x14"

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

class ZipFileKind(Enum): Cry3 = 0; P4k = 1

class ZipInfoX(ZipInfo):
    def __init__(self, filename='NoName', date_time=(1980,1,1,0,0,0)): super().__init__(filename, date_time)
    # def _decodeExtra(self, filename_crc): pass
    #     # super()._decodeExtra(filename_crc)

DEFAULT_CUSTOMIV = bytes([0x70, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0])
EndOfCentralRecordBaseSize = 22
class ZipFileX(ZipFile):
    _encryptedHeaders: EHeaderEncryptionType = 0
    _signedHeaders: EHeaderSignatureType = 0
    _headerSignature = [0, None]
    _headerExtended = [0, 0, 0]
    _headerEncryption = [0, 0, 0, None, None]
    _headerTeaEncryption = [0, None]
    _customIV: bytes = None
    _customKeys: list[bytes] = None
    _offsetOfFirstEntry: int = 0
    def __init__(self, kind: ZipFileKind, file: object, name: str, key: bytes, mode: str='r'): self._kind = kind; self._name = name; self._key = key; super().__init__(file, mode)

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
            if self._encryptedHeaders != EHeaderEncryptionType.HEADERS_ENCRYPTED_TEA and self._headerExtended[_CCXH_ENCRYPTION] != EHeaderEncryptionType.HEADERS_NOT_ENCRYPTED.value and self._encryptedHeaders != 0: raise BadZipFile("Unexpected encryption technique in header")
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
        isZip64 = False
        if not isZip64 and (s[_ECD_OFFSET] < eocdStart - (4 + s[_ECD_SIZE])):
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
                case EHeaderEncryptionType.HEADERS_ENCRYPTED_TEA: data = XXTeaDecrypt(data, nSize)
                case EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER: data = StreamCipher(data, nSize, GetReferenceCRCForPak())
                case EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE | EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2:
                    engineId = 'A' if self._encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2 else '2'
                    iv = DEFAULT_CUSTOMIV if self._headerTeaEncryption[_CCTEH_HEADER_SIZE] != 0 else self._customIV
                    if not (data := _DecryptBufferWithStreamCipher(engineId, data, nSize, self._customKeys[0], iv)): print('Failed to decrypt pak header'); return False
                case _: print('Attempting to load encrypted pak by unsupported method'); return False
            fp = self.fp = io.BytesIO(data)
            self.start_dir = 0
        match self._signedHeaders:
            case EHeaderSignatureType.HEADERS_CDR_SIGNED | EHeaderSignatureType.HEADERS_CDR_SIGNED2:
                if self._name == None: return True
                # Verify CDR signature & pak name
                pathSepIdx = max(self._name.rfind('\\'), self._name.rfind('/'))
                pathSep = self._name[pathSepIdx + 1:]
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
        # print(self._customIV.hex())

        # Decrypt the table of cipher keys.
        self._customKeys = [bytes()]*BLOCK_CIPHER_NUM_KEYS
        offset = 0
        for i in range(BLOCK_CIPHER_NUM_KEYS):
            self._customKeys[i] = _CustomRsaDecryptKeyEx(self._headerEncryption[_CCEH_KEYS_TABLE], offset, RSA_KEY_MESSAGE_LENGTH, cipher)
            # print(self._customKeys[i].hex())
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
        self.debug = 1
        fp = self.fp
        try:
            endrec = _EndRecData(fp)
        except OSError:
            raise BadZipFile("File is not a zip file")
        if not endrec:
            raise BadZipFile("File is not a zip file")
        if self.debug > 1:
            print(endrec)
        self._comment = endrec[_ECD_COMMENT]    # archive comment

        if self._kind == ZipFileKind.Cry3:
            fp.seek(-18, 2)
            if not ZipFileX.seekBackwardsToSignature(fp, stringEndArchive, ZIP_MAX_COMMENT + 4): raise BadZipFile("File is not a zip file")
            eocdStart = fp.tell()
            self._prepare(endrec)

        offset_cd, concat = _handle_prepended_data(endrec, self.debug)

        # self.start_dir:  Position of start of central directory
        self.start_dir = offset_cd + concat
        # print(f'start: {self.start_dir}')

        if self._kind == ZipFileKind.Cry3:
            self._trySfxEmbedded(endrec, eocdStart)
            self._readHeaderData(endrec)
        fp = self.fp

        if self.start_dir < 0:
            raise BadZipFile("Bad offset for central directory")

        fp.seek(self.start_dir, 0)
        size_cd = endrec[_ECD_SIZE]
        data = fp.read(size_cd)
        fp = io.BytesIO(data)
        total = 0
        while total < size_cd:
            centdir = fp.read(sizeCentralDir)
            if len(centdir) != sizeCentralDir:
                raise BadZipFile("Truncated central directory")
            centdir = struct.unpack(structCentralDir, centdir)
            if self._kind == ZipFileKind.P4k:
                if centdir[_CD_SIGNATURE] != p4kStringCentralDir and centdir[_CD_SIGNATURE] != p4kStringCentralDirEncrypted:
                    raise BadZipFile("Bad magic number for central directory")
            else:
                if centdir[_CD_SIGNATURE] != stringCentralDir:
                    raise BadZipFile("Bad magic number for central directory")
            if self.debug > 2:
                print(centdir)
            filename = fp.read(centdir[_CD_FILENAME_LENGTH])
            orig_filename_crc = crc32(filename)
            flags = centdir[_CD_FLAG_BITS]
            if flags & _MASK_UTF_FILENAME:
                # UTF-8 file names extension
                filename = filename.decode('utf-8')
            else:
                # Historical ZIP filename encoding
                filename = filename.decode(self.metadata_encoding or 'cp437')
            # print(filename)
            # Create ZipInfo instance to store file information
            x = ZipInfoX(filename)
            x.extra = fp.read(centdir[_CD_EXTRA_FIELD_LENGTH])
            x.comment = fp.read(centdir[_CD_COMMENT_LENGTH])
            x.header_offset = centdir[_CD_LOCAL_HEADER_OFFSET]
            (x.create_version, x.create_system, x.extract_version, x.reserved,
             x.flag_bits, x.compress_type, t, d,
             x.CRC, x.compress_size, x.file_size) = centdir[1:12]
            # print(x.file_size)
            if x.extract_version > MAX_EXTRACT_VERSION:
                raise NotImplementedError("zip file version %.1f" %
                                          (x.extract_version / 10))
            x.volume, x.internal_attr, x.external_attr = centdir[15:18]
            # Convert date/time code to (year, month, day, hour, min, sec)
            x._raw_time = t
            x.date_time = ( (d>>9)+1980, (d>>5)&0xF, d&0x1F,
                            t>>11, (t>>5)&0x3F, (t&0x1F) * 2 )
            x._decodeExtra(orig_filename_crc)
            x.header_offset = x.header_offset + concat
            self.filelist.append(x)
            self.NameToInfo[x.filename] = x

            # update total bytes read from central directory
            total = (total + sizeCentralDir + centdir[_CD_FILENAME_LENGTH]
                     + centdir[_CD_EXTRA_FIELD_LENGTH]
                     + centdir[_CD_COMMENT_LENGTH])

            if self.debug > 2:
                print("total", total)

        end_offset = self.start_dir
        for zinfo in reversed(sorted(self.filelist,
                                     key=lambda zinfo: zinfo.header_offset)):
            zinfo._end_offset = end_offset
            end_offset = zinfo.header_offset

#endregion