namespace System.Security.Cryptography;

#region XTEA

public static class XTEA {
    const uint Delta = 0x9E3779B9u;
    const uint Rounds = 32;
    const uint Sum = unchecked(Delta * Rounds);

     static void Decrypt(ref uint v0, ref uint v1, uint[] key, uint sum) {
        for (var i = 0U; i < Rounds; i++) {
            v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
            sum -= Delta;
            v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
        }
    }

    public static void Decrypt(byte[] data, int offset, int count, uint[] keys, uint sum = Sum) {
        for (var i = offset; i + 8 <= offset + count; i += 8) {
            var v0 = BitConverter.ToUInt32(data, i + 0);
            var v1 = BitConverter.ToUInt32(data, i + 4);
            Decrypt(ref v0, ref v1, keys, sum);
            Array.Copy(BitConverter.GetBytes(v0), 0, data, i + 0, 4);
            Array.Copy(BitConverter.GetBytes(v1), 0, data, i + 4, 4);
        }
    }
}

#endregion