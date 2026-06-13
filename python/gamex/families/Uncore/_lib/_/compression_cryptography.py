from __future__ import annotations
# from cryptography.hazmat import asn1
from cryptography.hazmat.primitives import padding
from cryptography.hazmat.primitives import serialization, hashes
from cryptography.hazmat.primitives.asymmetric import rsa, padding as padding2
# see: https://github.com/Legrandin/pycryptodome

def getReducedBlockSize(blockSize: int, algorithm: hashes.HashAlgorithm) -> int: return blockSize - 1 - 2 * algorithm.digest_size

def xorTo(len: int, x: bytes, xOff: int, z: bytes, zOff: int) -> None:
    for i in range(len): z[zOff + i] ^= x[xOff + i]

def maskGeneratorFunction(mgf1Hash: Hash, z: bytes, zOff: int, zLen: int, mask: bytes, maskOff: int, maskLen: int, algorithm: hashes.HashAlgorithm) -> None:
    digestSize = algorithm.digest_size
    hash: bytes = bytes(digestSize)
    counter = 0

    maskEnd = maskOff + maskLen
    maskLimit = maskEnd - digestSize
    maskPos = maskOff

    mgf1Hash.update(z[zOff:zOff+zLen])

    # if zLen > mgf1NoMemoLimit:

    memo = mgf1Hash.copy()
    while maskPos < maskLimit:
        C = counter.to_bytes(4, byteorder='big'); counter += 1
        mgf1Hash.update(C)
        hash = mgf1Hash.finalize()
        mgf1Hash = memo.copy()
        xorTo(digestSize, hash, 0, mask, maskPos)
        maskPos += digestSize

    # # else:
    # while maskPos < maskLimit:
    #     C = counter.to_bytes(4, byteorder='big'); counter += 1
    #     mgf1Hash.update(C)
    #     hash = mgf1Hash.finalize()
    #     mgf1Hash.update(z[zOff:zOff+zLen])
    #     xorTo(digestSize, hash, 0, mask, maskPos)
    #     maskPos += digestSize

    C = counter.to_bytes(4, byteorder='big')
    mgf1Hash.update(C)
    hash = mgf1Hash.finalize()
    xorTo(maskEnd - maskPos, hash, 0, mask, maskPos)

def _CustomRsaKeyHash(key: bytes, digestSize: int) -> tuple[object, object]: return serialization.load_der_public_key(key), hashes.BLAKE2b() if digestSize == 257 else hashes.SHA256() if digestSize == 256 else hashes.SHA1()
# def cipher(): return padding2.OAEP(mgf=padding2.MGF1(algorithm=hash), algorithm=hash, label=None)

def _CustomRsaDecryptKeyEx(inBytes: bytes, inOff: int, inLen: int, key: object, algorithm: hashes.HashAlgorithm) -> bytes:
    nums = key.public_numbers()

    defHash = bytearray(algorithm.digest_size); defHashLength = len(defHash)

    outBlockSize = (nums.n.bit_length() - 1) // 8

    # i.e. wrong when block.length < (2 * defHash.length) + 1
    wrongMask = getReducedBlockSize(outBlockSize, algorithm) >> 31

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
    
    mgf1Hash: Hash = hashes.Hash(algorithm)

    # unmask the seed.
    # maskGeneratorFunction(mgf1Hash, block, defHashLength, blockLength - defHashLength, block, 0, defHashLength)

    print(block.hex())
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



# def mgf1(seed: bytes, length: int) -> bytes:
#     """
#     Mask Generation Function 1 (MGF1) as described in PKCS#1 v2.
#     """
#     h_len = mgf1HashA.digest_size
#     t = b''

#     # Counter must be four octets, big-endian
#     for counter in range(0, (length + h_len - 1) // h_len):
#         c_bytes = counter.to_bytes(4, byteorder='big')
#         t += mgf1HashA(seed + c_bytes).digest()
        
#     return t[:length]

# def mgf1(seed: bytes, length: int, algorithm: hashes.HashAlgorithm) -> bytes:
#     """
#     Mask Generation Function 1 (MGF1) as defined in PKCS #1 v2.2.
#     """
#     hash_len = algorithm.digest_size
#     T = b""
    
#     # Counter must not exceed the maximum allowed iterations
#     max_count = math.ceil(length / hash_len)
#     if max_count > 0xFFFFFFFF:
#         raise ValueError("Mask length is too long for the chosen hash algorithm.")

#     # Loop until the mask reaches or exceeds the requested length
#     for counter in range(max_count):
#         # Convert counter to a 4-byte big-endian binary string
#         c_bytes = counter.to_bytes(4, byteorder='big')
        
#         # Initialize a new hash context per iteration
#         digest = hashes.Hash(algorithm)
#         digest.update(seed)
#         digest.update(c_bytes)
#         T += digest.finalize()
        
#     # Return the mask truncated to the exact requested length
#     return T[:length]