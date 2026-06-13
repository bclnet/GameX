

# def _GetPublicKey2(keyInfoData: bytes) -> object:
#     key = DerSequence().decode(keyInfoData)
#     #key = RSA.importKey(keyInfoData)
#     return key
#     # sequence = DerSequence()
#     # if not sequence.decode(keyInfoData): raise BadZipFile("Invalid PrivateKey Data")
#     # # algId = None; keyData = None
#     # # for value in sequence:
#     # #     if not keyData: keyData = DerBitString(sequence)
#     # # if not keyData: raise BadZipFile("Invalid PrivateKey Data")
#     # # pack = (algId or DerObjectId('1.2.840.113549.1.1.1'), keyData.value)
#     # n = sequence[0]; e = sequence[1]
#     # return RSA.construct((n, e))
#     # # Assuming 'spki_der' is the raw DER bytes of a SubjectPublicKeyInfo sequence
#     # spki = DerSequence().decode(keyInfoData)
#     # # 1. Parse the AlgorithmIdentifier sequence
#     # algoIdent = DerSequence().decode(spki[1].to_bytes())
#     # algoOid = DerObjectId().decode(algoIdent[0]).value
#     # # Validate that the OID corresponds to rsaEncryption (1.2.840.113549.1.1.1)
#     # if algoOid != '1.2.840.113549.1.1.1': raise ValueError('Not an RSA key identifier!')
#     # # 2. Extract the actual RSA public key BitString
#     # subject_public_key = DerBitString().decode(spki[1]).value
#     # # 3. Parse the isolated RSAPublicKey structure (modulus 'n' and exponent 'e')
#     # rsa_components = DerSequence().decode(subject_public_key)
#     # n = rsa_components[0], e = rsa_components[1]
#     # return RSA.construct((n, e))

# def _DecryptKeysTable2(aesKey: bytes, CDR_IV: bytes, KEYS_TABLE: bytes, digestSize: int) -> tuple[bool, bytes, list[bytes]]:
#     digest = BLAKE2b if digestSize == 257 else SHA256 if digestSize == 256 else SHA1
#     try:
#         key = _GetPublicKey(aesKey or RsaKey)
#         def cipher_init() -> object: return PKCS1_OAEP.new(key, hashAlgo=digest)

#         # cdr iv
#         cipher = cipher_init()
#         cdrIV = cipher.decrypt(CDR_IV)

#         # Decrypt the table of cipher keys.
#         keysTable = [bytes()]*BLOCK_CIPHER_NUM_KEYS
#         offset = 0
#         for i in range(BLOCK_CIPHER_NUM_KEYS):
#             cipher = cipher_init()
#             keysTable[i] = cipher.decrypt(KEYS_TABLE[offset:offset+RSA_KEY_MESSAGE_LENGTH])
#             offset += RSA_KEY_MESSAGE_LENGTH
#         return (True, cdrIV, keysTable)
#     except:
#         print(sys.exc_info()[1])
#         return (False, None, None)


        # ; rsaKeyN = rsaKey.public_numbers()
        # genKey = rsa.generate_private_key(public_exponent=rsaKeyN.e, key_size=rsaKeyN.n.bit_length()); genKeyN = genKey.private_numbers()
        # priKeyN = rsa.RSAPrivateNumbers(
        #     p=genKeyN.p,
        #     q=genKeyN.q,
        #     d=genKeyN.d,
        #     dmp1=genKeyN.dmp1,
        #     dmq1=genKeyN.dmq1,
        #     iqmp=genKeyN.iqmp,
        #     public_numbers=rsaKeyN)
        # priKey = priKeyN.private_key(unsafe_skip_rsa_key_validation=True)


# def RSAPublicKey_decrypt(self, ciphertext: bytes, cipher: object) -> bytes:
#     num = self.public_numbers()
#     keySizeBytes = (num.n.bit_length() + 7) // 8
#     if keySizeBytes != len(ciphertext): raise ValueError('Ciphertext length must be equal to key size.')
#     ctx = openssl::pkey_ctx::PkeyCtx::new(&self.pkey)?;
#     pass

def _CustomRsaDecryptKeyEx(inBytes: bytes, inOff: int, inLen: int, cipher: object, key: object) -> bytes:
    # data = RSAPublicKey_decrypt(key, inBytes[inOff:inOff+inLen], cipher)
    data = key.decrypt(inBytes[inOff:inOff+inLen], cipher)
    # return cipher.encrypt(inBytes[inOff:inOff+inLen])

    # data = inBytes[inOff:inOff+inLen]
    # padder = padding.PKCS7(128).padder()
    # data = padder.update(data) + padder.finalize()
    # num = key.public_numbers()
    # i = int.from_bytes(data, 'little')
    # r = pow(i, num.e, num.n)
    # return r.to_bytes((r.bit_length() + 7) // 8, 'little')
    return data

