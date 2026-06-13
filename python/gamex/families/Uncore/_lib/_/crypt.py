from __future__ import annotations
import sys, io, struct
from enum import Enum
# from cryptography.hazmat import asn1
from cryptography.hazmat.primitives import padding
from cryptography.hazmat.primitives import serialization, hashes
from cryptography.hazmat.primitives.asymmetric import rsa, padding as padding2

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

def _DecryptBufferWithStreamCipher(engineId: char, data: bytes, size: int, key: bytes, iv: bytes) -> bool:
    # try {
    #     var cipher = new BufferedBlockCipher(new SicRevBlockCipher(engineId == 'A' ? new AesEngine() : new TwofishEngine()));
    #     cipher.Init(false, new ParametersWithIV(new KeyParameter(key), iv));
    #     data = cipher.DoFinal(data, 0, size);
    # }
    # catch (CryptoException ex) { Console.WriteLine(ex.Message); return false; }
    return True

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


# see: "C:\_GITHUB\_ref\cryptography\src\cryptography\hazmat\backends\openssl\backend.py"

def _CustomRsaDecryptKeyEx(inBytes: bytes, inOff: int, inLen: int, cipher: object, key: object) -> bytes:
    mgf1HashA = cipher.algorithm; mgf = cipher.mgf; nums = key.public_numbers()
    # mgf1NoMemoLimit = GetMgf1NoMemoLimit(mgf1Hash)

    def getReducedBlockSize(blockSize: int) -> int: return blockSize - 1 - 2 * mgf1HashA.digest_size

    def xorTo(len: int, x: bytes, xOff: int, z: bytes, zOff: int) -> None:
        for i in range(len): z[zOff + i] ^= x[xOff + i]

    def mgf1(seed: bytes, length: int) -> bytes:
        """
        Mask Generation Function 1 (MGF1) as described in PKCS#1 v2.
        """
        h_len = mgf1HashA.digest_size
        t = b''
    
        # Counter must be four octets, big-endian
        for counter in range(0, (length + h_len - 1) // h_len):
            c_bytes = counter.to_bytes(4, byteorder='big')
            t += mgf1HashA(seed + c_bytes).digest()
            
        return t[:length]

    def mgf1(seed: bytes, length: int, algorithm: hashes.HashAlgorithm) -> bytes:
        """
        Mask Generation Function 1 (MGF1) as defined in PKCS #1 v2.2.
        """
        hash_len = algorithm.digest_size
        T = b""
        
        # Counter must not exceed the maximum allowed iterations
        max_count = math.ceil(length / hash_len)
        if max_count > 0xFFFFFFFF:
            raise ValueError("Mask length is too long for the chosen hash algorithm.")

        # Loop until the mask reaches or exceeds the requested length
        for counter in range(max_count):
            # Convert counter to a 4-byte big-endian binary string
            c_bytes = counter.to_bytes(4, byteorder='big')
            
            # Initialize a new hash context per iteration
            digest = hashes.Hash(algorithm)
            digest.update(seed)
            digest.update(c_bytes)
            T += digest.finalize()
            
        # Return the mask truncated to the exact requested length
        return T[:length]


    # def maskGeneratorFunction(mgf1Hash: Hash, z: bytes, zOff: int, zLen: int, mask: bytes, maskOff: int, maskLen: int) -> None:
    #     digestSize = mgf1HashA.digest_size
    #     hash: bytes = bytes(digestSize)
    #     counter = 0

    #     maskEnd = maskOff + maskLen
    #     maskLimit = maskEnd - digestSize
    #     maskPos = maskOff

    #     mgf1Hash.update(z[zOff:zOff+zLen])

    #     # if zLen > mgf1NoMemoLimit:

    #     memo = mgf1Hash.copy()
    #     while maskPos < maskLimit:
    #         C = counter.to_bytes(4, byteorder='big'); counter += 1
    #         mgf1Hash.update(C)
    #         hash = mgf1Hash.finalize()
    #         mgf1Hash = memo.copy()
    #         xorTo(digestSize, hash, 0, mask, maskPos)
    #         maskPos += digestSize

    #     # # else:
    #     # while maskPos < maskLimit:
    #     #     C = counter.to_bytes(4, byteorder='big'); counter += 1
    #     #     mgf1Hash.update(C)
    #     #     hash = mgf1Hash.finalize()
    #     #     mgf1Hash.update(z[zOff:zOff+zLen])
    #     #     xorTo(digestSize, hash, 0, mask, maskPos)
    #     #     maskPos += digestSize

    #     C = counter.to_bytes(4, byteorder='big')
    #     mgf1Hash.update(C)
    #     hash = mgf1Hash.finalize()
    #     xorTo(maskEnd - maskPos, hash, 0, mask, maskPos)

    defHash = bytearray(mgf1HashA.digest_size); defHashLength = len(defHash)

    outBlockSize = (nums.n.bit_length() - 1) // 8

    # i.e. wrong when block.length < (2 * defHash.length) + 1
    wrongMask = getReducedBlockSize(outBlockSize) >> 31

    # as we may have zeros in our leading bytes for the block we produced
    # on encryption, we need to make sure our decrypted block comes back
    # the same size.
    block = bytearray(outBlockSize); blockLength = len(block)
    
    # data = engine.processBlock(inBytes, inOff, inLen)
    i = int.from_bytes(inBytes[inOff:inOff+inLen], 'big')
    r = pow(i, nums.e, nums.n)
    data = r.to_bytes((r.bit_length() + 7) // 8, 'big')
    wrongMask |= (len(block) - len(data)) >> 31
    
    copyLen = int(min(len(block), len(data)))

    off = len(block) - copyLen
    block[off:off+copyLen] = data[:copyLen]
    
    mgf1Hash: Hash = hashes.Hash(mgf1HashA)

    # unmask the seed.
    mfg1(mgf1Hash, block, defHashLength, blockLength - defHashLength, block, 0, defHashLength)
    maskGeneratorFunction(mgf1Hash, block, defHashLength, blockLength - defHashLength, block, 0, defHashLength)

    
    mgf2 = padding2.MGF1(algorithm=mgf1HashA)
    print(f'HERE: {mgf}')
    print(f'HERE: {mgf2}')
    exit(1)

    # unmask the message block.
    maskGeneratorFunction(mgf1Hash, block, 0, defHashLength, block, defHashLength, blockLength - defHashLength)

    # check the hash of the encoding params.
    # long check to try to avoid this been a source of a timing attack.
    for i in range(defHashLength):
        wrongMask |= defHash[i] ^ block[defHashLength + i]

    # find the data block
    start = -1

    for index in range(2 * defHashLength, blockLength):
        octet = block[index]

        # i.e. mask will be 0xFFFFFFFF if octet is non-zero and start is (still) negative, else 0.
        shouldSetMask = (-octet & start) >> 31

        start += index & shouldSetMask

    wrongMask |= start >> 31
    start += 1
    wrongMask |= block[start] ^ 1

    if wrongMask != 0: raise ValueError('data wrong')

    start += 1

    # extract the data block
    output = bytearray(blockLength - start)

    # Array.Copy(block, start, output, 0, output.Length);
    # Array.Clear(block, 0, block.Length);

    return output


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

from zipfile import crc32, ZipInfo, ZipFile, _EndRecData, _ECD_SIGNATURE, _ECD_DISK_NUMBER, _ECD_COMMENT, _ECD_SIZE, _ECD_OFFSET, _ECD_LOCATION, _CD_SIGNATURE, _CD_FILENAME_LENGTH, _CD_FLAG_BITS, _MASK_UTF_FILENAME, _CD_EXTRA_FIELD_LENGTH, _CD_COMMENT_LENGTH, _CD_LOCAL_HEADER_OFFSET, MAX_EXTRACT_VERSION, sizeCentralDir, structCentralDir, stringCentralDir, stringEndArchive64, sizeEndCentDir64, sizeEndCentDir64Locator, BadZipFile

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

DEFAULT_CUSTOMIV = [0x70, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]
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
    def __init__(self, kind: ZipFileKind, file: object, name: str, key: bytes, mode: str='r'):
        self._kind = kind
        self._name = name
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
        fp.seek(self._offsetOfFirstEntry + s[_ECD_OFFSET])
        if self._encryptedHeaders != EHeaderEncryptionType.HEADERS_NOT_ENCRYPTED:
            bytes = fp.read(nSize)
            match self._encryptedHeaders:
                case EHeaderEncryptionType.HEADERS_ENCRYPTED_TEA: XXTeaDecrypt(bytes, nSize)
                case EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER: StreamCipher(bytes, nSize, GetReferenceCRCForPak())
                case EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE | EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2:
                    engineId = self._encryptedHeaders == 'A' if EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2 else '2'
                    iv = DEFAULT_CUSTOMIV if self._headerTeaEncryption[_CCTEH_HEADER_SIZE] != 0 else self._customIV
                    if not _DecryptBufferWithStreamCipher(engineId, bytes, nSize, self._customKeys[0], iv): print('Failed to decrypt pak header'); return False
                case _: print('Attempting to load encrypted pak by unsupported method'); return False
            fp = io.BytesIO(bytes)
            self.start_dir = 0
        match self._signedHeaders:
            # case EHeaderSignatureType.HEADERS_CDR_SIGNED | EHeaderSignatureType.HEADERS_CDR_SIGNED2:
            #     if self._name == None: break
            #     # Verify CDR signature & pak name
            #     pathSepIdx = Math.Max(_name.LastIndexOf('\\'), _name.LastIndexOf('/'));
            #     pathSep = _name[(pathSepIdx + 1)..];
            #     position = stream.Position; var bytes = new byte[nSize]; stream.ReadAtLeast(bytes, nSize); stream.Position = position;
            #     dataToVerify = new byte[][] { bytes, Encoding.ASCII.GetBytes(pathSep) };
            #     sizesToVerify = new int[] { nSize, pathSep.Length };
            #     # Could not verify signature
            #     if not RsaVerifyData(dataToVerify, sizesToVerify, 2, _headerSignature[_CCEH_CDR_SIGNED], 128, self._key): print('Failed to verify RSA signature of pak header'); return False
            case EHeaderSignatureType.HEADERS_NOT_SIGNED: pass
        return True

    # ZipDirCacheFactory::DecryptKeysTable
    def _decryptKeysTable(self):
        digestSize = 1 if self._headerTeaEncryption[_CCTEH_HEADER_SIZE] != 0 else 256
        rsaKey = serialization.load_der_public_key(self._key or DefaultRsaKey)
        hash = hashes.BLAKE2b() if digestSize == 257 else hashes.SHA256() if digestSize == 256 else hashes.SHA1()
        def cipher(): return padding2.OAEP(mgf=padding2.MGF1(algorithm=hash), algorithm=hash, label=None)

        # Decrypt CDR initial Vector
        self._customIV = _CustomRsaDecryptKeyEx(self._headerEncryption[_CCEH_CDR_IV], 0, RSA_KEY_MESSAGE_LENGTH, cipher(), rsaKey)
        print(self._customIV.hex())
        exit(1)

        # Decrypt the table of cipher keys.
        self._customKeys = [bytes()]*BLOCK_CIPHER_NUM_KEYS
        offset = 0
        for i in range(BLOCK_CIPHER_NUM_KEYS):
            self._customKeys[i] = _CustomRsaDecryptKeyEx(self._headerEncryption[_CCEH_KEYS_TABLE], offset, RSA_KEY_MESSAGE_LENGTH, cipher(), rsaKey)
            offset += RSA_KEY_MESSAGE_LENGTH

        # try:
        # except: print(sys.exc_info()[1])

    def _RealGetContents(self):
        """Read in the table of contents for the ZIP file."""
        self.debug = 0
        fp = self.fp
        try:
            endrec = _EndRecData(fp)
        except OSError:
            raise BadZipFile("File is not a zip file")
        if not endrec:
            raise BadZipFile("File is not a zip file")
        if self.debug > 1:
            print(endrec)
        eocdStart = fp.tell()
        self._comment = endrec[_ECD_COMMENT]    # archive comment

        if self._kind == ZipFileKind.Cry3: self._prepare(endrec)

        offset_cd, concat = _handle_prepended_data(endrec, self.debug)

        # self.start_dir:  Position of start of central directory
        self.start_dir = offset_cd + concat

        if self._kind == ZipFileKind.Cry3:
            self._trySfxEmbedded(endrec, eocdStart)
            self._readHeaderData(endrec)

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
            print(filename)
            # Create ZipInfo instance to store file information
            x = ZipInfoX(filename)
            x.extra = fp.read(centdir[_CD_EXTRA_FIELD_LENGTH])
            x.comment = fp.read(centdir[_CD_COMMENT_LENGTH])
            x.header_offset = centdir[_CD_LOCAL_HEADER_OFFSET]
            (x.create_version, x.create_system, x.extract_version, x.reserved,
             x.flag_bits, x.compress_type, t, d,
             x.CRC, x.compress_size, x.file_size) = centdir[1:12]
            print(x.file_size)
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