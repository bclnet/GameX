//using Org.BouncyCastle.Crypto;
//using Org.BouncyCastle.Crypto.Digests;
//using Org.BouncyCastle.Crypto.Encodings;
//using Org.BouncyCastle.Utilities;

//namespace System.IO.Compression;

//class Extra {
//    IAsymmetricBlockCipher engine;
//    IDigest mgf1Hash;
//    int mgf1NoMemoLimit;
//    byte[] defHash;

//    public Extra(OaepEncoding cipher) {
//        engine = cipher.UnderlyingCipher;
//        mgf1Hash = new Sha1Digest();
//        mgf1NoMemoLimit = GetMgf1NoMemoLimit(mgf1Hash);
//        defHash = new byte[mgf1Hash.GetDigestSize()];


//        //var core = (IRsa)typeof(RsaEngine).GetField("core", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(engine);
//        //var outBlockSize = engine.GetOutputBlockSize();
//        ////var m_bitSize = (int)typeof(RsaCoreEngine).GetField("m_bitSize", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(core);
//        //var output = core.ProcessBlock(new Org.BouncyCastle.Math.BigInteger(1, inBytes, inOff, inLen)).ToByteArrayUnsigned();
//        ////Console.WriteLine($"output.Hex()}\n");
//        //Console.WriteLine($"{outBlockSize} == {output.Length}");
//    }

//    int GetReducedBlockSize(int blockSize) => blockSize - 1 - 2 * defHash.Length;

//    void MaskGeneratorFunction(byte[] z, int zOff, int zLen, byte[] mask, int maskOff, int maskLen) {
//        if (mgf1Hash is IXof xof) {
//            byte[] buf = new byte[maskLen];
//            xof.BlockUpdate(z, zOff, zLen);
//            xof.OutputFinal(buf, 0, maskLen);
//            Bytes.XorTo(maskLen, buf, 0, mask, maskOff);
//        }
//        else {
//            MaskGeneratorFunction1(z, zOff, zLen, mask, maskOff, maskLen);
//        }
//    }

//    static int GetMgf1NoMemoLimit(IDigest d) {
//        if (d is IMemoable)
//            return d.GetByteLength() - 1;
//        return int.MaxValue;
//    }

//    static void UInt32_To_BE(uint n, byte[] bs) {
//        bs[0] = (byte)(n >> 24);
//        bs[1] = (byte)(n >> 16);
//        bs[2] = (byte)(n >> 8);
//        bs[3] = (byte)n;
//    }

//    void MaskGeneratorFunction1(byte[] z, int zOff, int zLen, byte[] mask, int maskOff, int maskLen) {
//        int digestSize = mgf1Hash.GetDigestSize();
//        byte[] hash = new byte[digestSize];
//        byte[] C = new byte[4];
//        int counter = 0;

//        int maskEnd = maskOff + maskLen;
//        int maskLimit = maskEnd - digestSize;
//        int maskPos = maskOff;

//        mgf1Hash.BlockUpdate(z, zOff, zLen);

//        if (zLen > mgf1NoMemoLimit) {
//            var memoable = (IMemoable)mgf1Hash;
//            var memo = memoable.Copy();
//            while (maskPos < maskLimit) {
//                UInt32_To_BE((uint)counter++, C);
//                mgf1Hash.BlockUpdate(C, 0, C.Length);
//                mgf1Hash.DoFinal(hash, 0);
//                memoable.Reset(memo);
//                Bytes.XorTo(digestSize, hash, 0, mask, maskPos);
//                maskPos += digestSize;
//            }
//        }
//        else {
//            while (maskPos < maskLimit) {
//                UInt32_To_BE((uint)counter++, C);
//                mgf1Hash.BlockUpdate(C, 0, C.Length);
//                mgf1Hash.DoFinal(hash, 0);
//                mgf1Hash.BlockUpdate(z, zOff, zLen);
//                Bytes.XorTo(digestSize, hash, 0, mask, maskPos);
//                maskPos += digestSize;
//            }
//        }

//        UInt32_To_BE((uint)counter, C);
//        mgf1Hash.BlockUpdate(C, 0, C.Length);
//        mgf1Hash.DoFinal(hash, 0);
//        Bytes.XorTo(maskEnd - maskPos, hash, 0, mask, maskPos);
//    }

//    public byte[] DecodeBlock(byte[] inBytes, int inOff, int inLen) {
//        int outBlockSize = engine.GetOutputBlockSize();

//        // i.e. wrong when block.length < (2 * defHash.length) + 1
//        int wrongMask = GetReducedBlockSize(outBlockSize) >> 31;

//        //
//        // as we may have zeros in our leading bytes for the block we produced
//        // on encryption, we need to make sure our decrypted block comes back
//        // the same size.
//        //
//        byte[] block = new byte[outBlockSize];
//        {
//            byte[] data = engine.ProcessBlock(inBytes, inOff, inLen);
//            wrongMask |= (block.Length - data.Length) >> 31;

//            int copyLen = System.Math.Min(block.Length, data.Length);
//            Array.Copy(data, 0, block, block.Length - copyLen, copyLen);
//            Array.Clear(data, 0, data.Length);
//        }

//        mgf1Hash.Reset();

//        //
//        // unmask the seed.
//        //
//        Console.WriteLine($"blockV: {block[defHash.Length..(defHash.Length + block.Length - defHash.Length)].Hex()}");
//        Console.WriteLine($"maskV: {block[0..defHash.Length].Hex()}");
//        MaskGeneratorFunction(block, defHash.Length, block.Length - defHash.Length, block, 0, defHash.Length);
//        Console.WriteLine($"maskV: {block[0..defHash.Length].Hex()}");

//        //
//        // unmask the message block.
//        //
//        MaskGeneratorFunction(block, 0, defHash.Length, block, defHash.Length, block.Length - defHash.Length);

//        //
//        // check the hash of the encoding params.
//        // long check to try to avoid this been a source of a timing attack.
//        //
//        for (int i = 0; i != defHash.Length; i++) {
//            wrongMask |= defHash[i] ^ block[defHash.Length + i];
//        }

//        //
//        // find the data block
//        //
//        int start = -1;

//        for (int index = 2 * defHash.Length; index != block.Length; index++) {
//            int octet = block[index];

//            // i.e. mask will be 0xFFFFFFFF if octet is non-zero and start is (still) negative, else 0.
//            int shouldSetMask = (-octet & start) >> 31;

//            start += index & shouldSetMask;
//        }

//        wrongMask |= start >> 31;
//        ++start;
//        wrongMask |= block[start] ^ 1;

//        if (wrongMask != 0) {
//            Array.Clear(block, 0, block.Length);
//            throw new InvalidCipherTextException("data wrong");
//        }

//        ++start;

//        //
//        // extract the data block
//        //
//        byte[] output = new byte[block.Length - start];

//        Array.Copy(block, start, output, 0, output.Length);
//        Array.Clear(block, 0, block.Length);

//        return output;
//    }

//    // ZipEncrypt::custom_rsa_decrypt_key_ex
//    internal static byte[] CustomRsaDecryptKeyEx(byte[] inBytes, int inOff, int inLen, OaepEncoding cipher, ICipherParameters key) {
//        cipher.Init(false, key);
//        var res = cipher.ProcessBlock(inBytes, inOff, inLen);
//        Console.WriteLine(res.Hex());

//        var res2 = new Extra(cipher).DecodeBlock(inBytes, inOff, inLen);
//        Console.WriteLine(res2.Hex());
//        return res;
//        //return cipher.ProcessBlock(inBytes, inOff, inLen);
//    }
//}