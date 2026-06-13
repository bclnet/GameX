from __future__ import annotations
# from Crypto.Util.asn1 import DerObjectId, DerSequence, DerInteger, DerBitString
from Crypto.PublicKey import RSA
from Crypto.Cipher import AES, PKCS1_OAEP
from Crypto.Hash import BLAKE2b, SHA256, SHA1
# see: https://github.com/Legrandin/pycryptodome

def getReducedBlockSize(blockSize: int, algorithm: Hash) -> int: return blockSize - 1 - 2 * algorithm.digest_size

def _CustomRsaKeyHash(key: bytes, digestSize: int) -> tuple[object, object]: return RSA.importKey(key), BLAKE2b if digestSize == 257 else SHA256 if digestSize == 256 else SHA1
# def cipher2(): return PKCS1_OAEP.new(None, hashAlgo=hash)

def _CustomRsaDecryptKeyEx(inBytes: bytes, inOff: int, inLen: int, key: object, algorithm: hashes.HashAlgorithm) -> bytes:
    defHash = bytearray(algorithm.digest_size); defHashLength = len(defHash)

    outBlockSize = (key.n.bit_length() - 1) // 8

    # i.e. wrong when block.length < (2 * defHash.length) + 1
    wrongMask = getReducedBlockSize(outBlockSize, algorithm) >> 31

    # as we may have zeros in our leading bytes for the block we produced
    # on encryption, we need to make sure our decrypted block comes back
    # the same size.
    block = bytearray(outBlockSize); blockLength = len(block)
    
    # data = engine.processBlock(inBytes, inOff, inLen)
    i = int.from_bytes(inBytes[inOff:inOff+inLen], 'big')
    r = pow(i, key.e, key.n)
    data = r.to_bytes((r.bit_length() + 7) // 8, 'big')
    wrongMask |= (len(block) - len(data)) >> 31
    
    copyLen = int(min(len(block), len(data)))

    off = len(block) - copyLen
    block[off:off+copyLen] = data[:copyLen]
    # print(block.hex())

    mgf1 = PKCS1_OAEP.new(key, hashAlgo=algorithm)
    mgf1.encrypt(block)

    mgf1Hash: Hash = algorithm.new()
    exit(1)

    # unmask the seed.
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
