using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameX.Origin.Formats.UO;

#region EncryptionHelper

enum EncryptionType {
    NONE,
    OLD_BFISH,
    BLOWFISH__1_25_36,
    BLOWFISH,
    BLOWFISH__2_0_3,
    TWOFISH_MD5
}

// EncryptionHelper
class EncryptionHelper {
    static readonly LoginCryptBehaviour _loginCrypt = new LoginCryptBehaviour();
    static readonly BlowfishEncryption _blowfishEncryption = new BlowfishEncryption();
    static readonly TwofishEncryption _twoFishBehaviour = new TwofishEncryption();

    readonly ClientVersion _clientVersion;
    readonly uint[] _keys;

    public EncryptionHelper(ClientVersion clientVersion) {
        _clientVersion = clientVersion;
        (EncryptionType, _keys) = CalculateEncryption(clientVersion);
    }

    public EncryptionType EncryptionType { get; }

    static (EncryptionType, uint[]) CalculateEncryption(ClientVersion version) {
        if (version == ClientVersion.CV_200X) return (EncryptionType.BLOWFISH__2_0_3, [0x2D13A5FC, 0x2D13A5FD, 0xA39D527F]);
        var a = ((int)version >> 24) & 0xFF;
        var b = ((int)version >> 16) & 0xFF;
        var c = ((int)version >> 8) & 0xFF;
        var temp = ((((a << 9) | b) << 10) | c) ^ ((c * c) << 5);
        var key2 = (uint)((temp << 4) ^ (b * b) ^ (b * 0x0B000000) ^ (c * 0x380000) ^ 0x2C13A5FD);
        temp = (((((a << 9) | c) << 10) | b) * 8) ^ (c * c * 0x0c00);
        var key3 = (uint)(temp ^ (b * b) ^ (b * 0x6800000) ^ (c * 0x1c0000) ^ 0x0A31D527F);
        var key1 = key2 - 1;
        return version switch {
            < (ClientVersion)((1 & 0xFF) << 24 | (25 & 0xFF) << 16 | (35 & 0xFF) << 8 | 0 & 0xFF) => (EncryptionType.OLD_BFISH, [key1, key2, key3]),
            (ClientVersion)((1 & 0xFF) << 24 | (25 & 0xFF) << 16 | (36 & 0xFF) << 8 | 0 & 0xFF) => (EncryptionType.BLOWFISH__1_25_36, [key1, key2, key3]),
            <= ClientVersion.CV_200 => (EncryptionType.BLOWFISH, [key1, key2, key3]),
            <= (ClientVersion)((2 & 0xFF) << 24 | (0 & 0xFF) << 16 | (3 & 0xFF) << 8 | 0 & 0xFF) => (EncryptionType.BLOWFISH__2_0_3, [key1, key2, key3]),
            _ => (EncryptionType.TWOFISH_MD5, [key1, key2, key3]),
        };
    }

    public void Initialize(bool isLogin, uint seed) {
        if (EncryptionType == EncryptionType.NONE) return;
        if (isLogin) _loginCrypt.Initialize(seed, _keys[0], _keys[1], _keys[2]);
        else {
            if (EncryptionType >= EncryptionType.OLD_BFISH && EncryptionType < EncryptionType.TWOFISH_MD5) _blowfishEncryption.Initialize();
            if (EncryptionType == EncryptionType.BLOWFISH__2_0_3 || EncryptionType == EncryptionType.TWOFISH_MD5) _twoFishBehaviour.Initialize(seed, EncryptionType == EncryptionType.TWOFISH_MD5);
        }
    }

    public void Encrypt(bool is_login, Span<byte> src, Span<byte> dst, int size) {
        if (EncryptionType == EncryptionType.NONE) return;
        if (is_login) {
            if (EncryptionType == EncryptionType.OLD_BFISH) _loginCrypt.Encrypt_OLD(src, dst, size);
            else if (EncryptionType == EncryptionType.BLOWFISH__1_25_36) _loginCrypt.Encrypt_1_25_36(src, dst, size);
            else if (EncryptionType != EncryptionType.NONE) _loginCrypt.Encrypt(src, dst, size);
        }
        else if (EncryptionType == EncryptionType.BLOWFISH__2_0_3) {
            int index_s = 0, index_d = 0;
            _blowfishEncryption.Encrypt(src, dst, size, ref index_s, ref index_d);
            _twoFishBehaviour.Encrypt(dst, dst, size);
        }
        else if (EncryptionType == EncryptionType.TWOFISH_MD5) _twoFishBehaviour.Encrypt(src, dst, size);
        else {
            int index_s = 0, index_d = 0;
            _blowfishEncryption.Encrypt(src, dst, size, ref index_s, ref index_d);
        }
    }

    public void Decrypt(Span<byte> src, Span<byte> dst, int size) {
        if (EncryptionType == EncryptionType.TWOFISH_MD5) _twoFishBehaviour.Decrypt(src, dst, size);
    }
}

#endregion

#region Blowfish

unsafe class BlowfishEncryption {
    readonly byte[] _seed = new byte[Crypt_Constants.CRYPT_GAME_SEED_LENGTH];
    int _table_index, _block_pos, _stream_pos;

    public void InitTables() {
        int i;
        for (i = 0; i < Crypt_Constants.p_table.Length; i++) Crypt_Constants.p_table[i] = new uint[18];
        for (i = 0; i < Crypt_Constants.s_table.Length; i++) Crypt_Constants.s_table[i] = new uint[1024];
        for (var key_index = 0; key_index < Crypt_Constants.CRYPT_GAME_KEY_COUNT - 1; key_index++) {
            Array.Copy(Crypt_Constants.p_box, Crypt_Constants.p_table[key_index], Crypt_Constants.p_box.Length);
            Array.Copy(Crypt_Constants.s_box, Crypt_Constants.s_table[key_index], Crypt_Constants.s_box.Length);
            fixed (byte* key_table_ptr = &Crypt_Constants.g_key_table[key_index, 0], key_table_end_ptr = &Crypt_Constants.g_key_table[key_index + 1, 0]) {
                byte* pkey = key_table_ptr;
                byte* pkey_end = key_table_end_ptr;
                for (i = 0; i < 18; i++) {
                    uint mask = *pkey++;
                    if (pkey >= pkey_end) pkey = key_table_ptr;
                    mask = (mask << 8) | *pkey++;
                    if (pkey >= pkey_end) pkey = key_table_ptr;
                    mask = (mask << 8) | *pkey++;
                    if (pkey >= pkey_end) pkey = key_table_ptr;
                    mask = (mask << 8) | *pkey++;
                    if (pkey >= pkey_end) pkey = key_table_ptr;
                    Crypt_Constants.p_table[key_index][i] ^= mask;
                }
                uint value_0 = 0, value_1 = 0;
                for (i = 0; i < 18; i += 2) {
                    RawEncrypt(ref value_0, ref value_1, key_index);
                    Crypt_Constants.p_table[key_index][i] = value_0;
                    Crypt_Constants.p_table[key_index][i + 1] = value_1;
                }
                for (i = 0; i < 1024; i += 2) {
                    RawEncrypt(ref value_0, ref value_1, key_index);
                    Crypt_Constants.s_table[key_index][i] = value_0;
                    Crypt_Constants.s_table[key_index][i + 1] = value_1;
                }
            }
        }
        Crypt_Constants.table_is_ready = true;
    }

    public void Initialize() {
        if (!Crypt_Constants.table_is_ready) InitTables();
        _table_index = Crypt_Constants.CRYPT_GAME_TABLE_START;
        Array.Copy(Crypt_Constants.g_seed_table[0][_table_index][0], _seed, Crypt_Constants.CRYPT_GAME_SEED_LENGTH);
        _stream_pos = 0;
        _block_pos = 0;
    }

    public void Encrypt(Span<byte> src, Span<byte> dst, int size, ref int index_in, ref int index_out) {
        while (_stream_pos + size > Crypt_Constants.CRYPT_GAME_TABLE_TRIGGER) {
            var len_remaining = Crypt_Constants.CRYPT_GAME_TABLE_TRIGGER - _stream_pos;
            Encrypt(src, dst, len_remaining, ref index_in, ref index_out);
            _table_index = (_table_index + Crypt_Constants.CRYPT_GAME_TABLE_STEP) % Crypt_Constants.CRYPT_GAME_TABLE_MODULO;
            Array.Copy(Crypt_Constants.g_seed_table[1][_table_index][0], _seed, Crypt_Constants.CRYPT_GAME_SEED_LENGTH);
            _stream_pos = 0;
            _block_pos = 0;
            index_in += len_remaining;
            index_out += len_remaining;
            size -= len_remaining;
        }
        uint value_0 = 0, value_1 = 0;
        for (int i = 0; i < size; i++) {
            if (_block_pos == 0) {
                value_0 = 0; value_1 = 0;
                fixed (byte* ptr = _seed) {
                    byte* p = ptr; Crypt_Constants.N2L(ref p, ref value_0); Crypt_Constants.N2L(ref p, ref value_1);
                    RawEncrypt(ref value_0, ref value_1, _table_index);
                    fixed (byte* ptr2 = _seed) {
                        p = ptr2; Crypt_Constants.L2N(ref value_0, ref p); Crypt_Constants.L2N(ref value_1, ref p);
                    }
                }
            }
            dst[i] = (byte)(src[i] ^ _seed[_block_pos]);
            _seed[_block_pos] = dst[i];
            _block_pos = (_block_pos + 1) % 8;
        }
        _stream_pos += size;
    }

    static void RawEncrypt(ref uint value_0, ref uint value_1, int table) {
        uint left = value_0, right = value_1;
        left ^= Crypt_Constants.p_table[table][0];
        for (var i = 1; i < 16; i += 2) {
            Crypt_Constants.Round(ref right, left, Crypt_Constants.s_table[table], Crypt_Constants.p_table[table][i]);
            Crypt_Constants.Round(ref left, right, Crypt_Constants.s_table[table], Crypt_Constants.p_table[table][i + 1]);
        }
        right ^= Crypt_Constants.p_table[table][17];
        value_1 = left; value_0 = right;
    }

    static class Crypt_Constants {
        public const byte CRYPT_AUTO_VALUE = 0x80;
        public const byte CRYPT_GAME_KEY_LENGTH = 6;
        public const byte CRYPT_GAME_KEY_COUNT = 25;
        public const byte CRYPT_GAME_SEED_LENGTH = 8;
        public const byte CRYPT_GAME_SEED_COUNT = 25;
        public const byte CRYPT_GAME_TABLE_START = 1;
        public const byte CRYPT_GAME_TABLE_STEP = 3;
        public const byte CRYPT_GAME_TABLE_MODULO = 11;
        public const int CRYPT_GAME_TABLE_TRIGGER = 21036;
        public const byte DIR_ENCRYPT = 0;
        public const byte DIR_DECRYPT = 1;
        public const byte MODE_ECB = 1;
        public static bool table_is_ready;
        public static readonly uint[] p_box = [
            0x243f6a88, 0x85a308d3, 0x13198a2e, 0x03707344, 0xa4093822,
            0x299f31d0, 0x082efa98, 0xec4e6c89, 0x452821e6, 0x38d01377,
            0xbe5466cf, 0x34e90c6c, 0xc0ac29b7, 0xc97c50dd, 0x3f84d5b5,
            0xb5470917, 0x9216d5d9, 0x8979fb1b];
        public static readonly uint[] s_box = [
            0xd1310ba6, 0x98dfb5ac, 0x2ffd72db, 0xd01adfb7, 0xb8e1afed, 0x6a267e96, 0xba7c9045, 0xf12c7f99,
            0x24a19947, 0xb3916cf7, 0x0801f2e2, 0x858efc16, 0x636920d8, 0x71574e69, 0xa458fea3, 0xf4933d7e,
            0x0d95748f, 0x728eb658, 0x718bcd58, 0x82154aee, 0x7b54a41d, 0xc25a59b5, 0x9c30d539, 0x2af26013,
            0xc5d1b023, 0x286085f0, 0xca417918, 0xb8db38ef, 0x8e79dcb0, 0x603a180e, 0x6c9e0e8b, 0xb01e8a3e,
            0xd71577c1, 0xbd314b27, 0x78af2fda, 0x55605c60, 0xe65525f3, 0xaa55ab94, 0x57489862, 0x63e81440,
            0x55ca396a, 0x2aab10b6, 0xb4cc5c34, 0x1141e8ce, 0xa15486af, 0x7c72e993, 0xb3ee1411, 0x636fbc2a,
            0x2ba9c55d, 0x741831f6, 0xce5c3e16, 0x9b87931e, 0xafd6ba33, 0x6c24cf5c, 0x7a325381, 0x28958677,
            0x3b8f4898, 0x6b4bb9af, 0xc4bfe81b, 0x66282193, 0x61d809cc, 0xfb21a991, 0x487cac60, 0x5dec8032,
            0xef845d5d, 0xe98575b1, 0xdc262302, 0xeb651b88, 0x23893e81, 0xd396acc5, 0x0f6d6ff3, 0x83f44239,
            0x2e0b4482, 0xa4842004, 0x69c8f04a, 0x9e1f9b5e, 0x21c66842, 0xf6e96c9a, 0x670c9c61, 0xabd388f0,
            0x6a51a0d2, 0xd8542f68, 0x960fa728, 0xab5133a3, 0x6eef0b6c, 0x137a3be4, 0xba3bf050, 0x7efb2a98,
            0xa1f1651d, 0x39af0176, 0x66ca593e, 0x82430e88, 0x8cee8619, 0x456f9fb4, 0x7d84a5c3, 0x3b8b5ebe,
            0xe06f75d8, 0x85c12073, 0x401a449f, 0x56c16aa6, 0x4ed3aa62, 0x363f7706, 0x1bfedf72, 0x429b023d,
            0x37d0d724, 0xd00a1248, 0xdb0fead3, 0x49f1c09b, 0x075372c9, 0x80991b7b, 0x25d479d8, 0xf6e8def7,
            0xe3fe501a, 0xb6794c3b, 0x976ce0bd, 0x04c006ba, 0xc1a94fb6, 0x409f60c4, 0x5e5c9ec2, 0x196a2463,
            0x68fb6faf, 0x3e6c53b5, 0x1339b2eb, 0x3b52ec6f, 0x6dfc511f, 0x9b30952c, 0xcc814544, 0xaf5ebd09,
            0xbee3d004, 0xde334afd, 0x660f2807, 0x192e4bb3, 0xc0cba857, 0x45c8740f, 0xd20b5f39, 0xb9d3fbdb,
            0x5579c0bd, 0x1a60320a, 0xd6a100c6, 0x402c7279, 0x679f25fe, 0xfb1fa3cc, 0x8ea5e9f8, 0xdb3222f8,
            0x3c7516df, 0xfd616b15, 0x2f501ec8, 0xad0552ab, 0x323db5fa, 0xfd238760, 0x53317b48, 0x3e00df82,
            0x9e5c57bb, 0xca6f8ca0, 0x1a87562e, 0xdf1769db, 0xd542a8f6, 0x287effc3, 0xac6732c6, 0x8c4f5573,
            0x695b27b0, 0xbbca58c8, 0xe1ffa35d, 0xb8f011a0, 0x10fa3d98, 0xfd2183b8, 0x4afcb56c, 0x2dd1d35b,
            0x9a53e479, 0xb6f84565, 0xd28e49bc, 0x4bfb9790, 0xe1ddf2da, 0xa4cb7e33, 0x62fb1341, 0xcee4c6e8,
            0xef20cada, 0x36774c01, 0xd07e9efe, 0x2bf11fb4, 0x95dbda4d, 0xae909198, 0xeaad8e71, 0x6b93d5a0,
            0xd08ed1d0, 0xafc725e0, 0x8e3c5b2f, 0x8e7594b7, 0x8ff6e2fb, 0xf2122b64, 0x8888b812, 0x900df01c,
            0x4fad5ea0, 0x688fc31c, 0xd1cff191, 0xb3a8c1ad, 0x2f2f2218, 0xbe0e1777, 0xea752dfe, 0x8b021fa1,
            0xe5a0cc0f, 0xb56f74e8, 0x18acf3d6, 0xce89e299, 0xb4a84fe0, 0xfd13e0b7, 0x7cc43b81, 0xd2ada8d9,
            0x165fa266, 0x80957705, 0x93cc7314, 0x211a1477, 0xe6ad2065, 0x77b5fa86, 0xc75442f5, 0xfb9d35cf,
            0xebcdaf0c, 0x7b3e89a0, 0xd6411bd3, 0xae1e7e49, 0x00250e2d, 0x2071b35e, 0x226800bb, 0x57b8e0af,
            0x2464369b, 0xf009b91e, 0x5563911d, 0x59dfa6aa, 0x78c14389, 0xd95a537f, 0x207d5ba2, 0x02e5b9c5,
            0x83260376, 0x6295cfa9, 0x11c81968, 0x4e734a41, 0xb3472dca, 0x7b14a94a, 0x1b510052, 0x9a532915,
            0xd60f573f, 0xbc9bc6e4, 0x2b60a476, 0x81e67400, 0x08ba6fb5, 0x571be91f, 0xf296ec6b, 0x2a0dd915,
            0xb6636521, 0xe7b9f9b6, 0xff34052e, 0xc5855664, 0x53b02d5d, 0xa99f8fa1, 0x08ba4799, 0x6e85076a,
            0x4b7a70e9, 0xb5b32944, 0xdb75092e, 0xc4192623, 0xad6ea6b0, 0x49a7df7d, 0x9cee60b8, 0x8fedb266,
            0xecaa8c71, 0x699a17ff, 0x5664526c, 0xc2b19ee1, 0x193602a5, 0x75094c29, 0xa0591340, 0xe4183a3e,
            0x3f54989a, 0x5b429d65, 0x6b8fe4d6, 0x99f73fd6, 0xa1d29c07, 0xefe830f5, 0x4d2d38e6, 0xf0255dc1,
            0x4cdd2086, 0x8470eb26, 0x6382e9c6, 0x021ecc5e, 0x09686b3f, 0x3ebaefc9, 0x3c971814, 0x6b6a70a1,
            0x687f3584, 0x52a0e286, 0xb79c5305, 0xaa500737, 0x3e07841c, 0x7fdeae5c, 0x8e7d44ec, 0x5716f2b8,
            0xb03ada37, 0xf0500c0d, 0xf01c1f04, 0x0200b3ff, 0xae0cf51a, 0x3cb574b2, 0x25837a58, 0xdc0921bd,
            0xd19113f9, 0x7ca92ff6, 0x94324773, 0x22f54701, 0x3ae5e581, 0x37c2dadc, 0xc8b57634, 0x9af3dda7,
            0xa9446146, 0x0fd0030e, 0xecc8c73e, 0xa4751e41, 0xe238cd99, 0x3bea0e2f, 0x3280bba1, 0x183eb331,
            0x4e548b38, 0x4f6db908, 0x6f420d03, 0xf60a04bf, 0x2cb81290, 0x24977c79, 0x5679b072, 0xbcaf89af,
            0xde9a771f, 0xd9930810, 0xb38bae12, 0xdccf3f2e, 0x5512721f, 0x2e6b7124, 0x501adde6, 0x9f84cd87,
            0x7a584718, 0x7408da17, 0xbc9f9abc, 0xe94b7d8c, 0xec7aec3a, 0xdb851dfa, 0x63094366, 0xc464c3d2,
            0xef1c1847, 0x3215d908, 0xdd433b37, 0x24c2ba16, 0x12a14d43, 0x2a65c451, 0x50940002, 0x133ae4dd,
            0x71dff89e, 0x10314e55, 0x81ac77d6, 0x5f11199b, 0x043556f1, 0xd7a3c76b, 0x3c11183b, 0x5924a509,
            0xf28fe6ed, 0x97f1fbfa, 0x9ebabf2c, 0x1e153c6e, 0x86e34570, 0xeae96fb1, 0x860e5e0a, 0x5a3e2ab3,
            0x771fe71c, 0x4e3d06fa, 0x2965dcb9, 0x99e71d0f, 0x803e89d6, 0x5266c825, 0x2e4cc978, 0x9c10b36a,
            0xc6150eba, 0x94e2ea78, 0xa5fc3c53, 0x1e0a2df4, 0xf2f74ea7, 0x361d2b3d, 0x1939260f, 0x19c27960,
            0x5223a708, 0xf71312b6, 0xebadfe6e, 0xeac31f66, 0xe3bc4595, 0xa67bc883, 0xb17f37d1, 0x018cff28,
            0xc332ddef, 0xbe6c5aa5, 0x65582185, 0x68ab9802, 0xeecea50f, 0xdb2f953b, 0x2aef7dad, 0x5b6e2f84,
            0x1521b628, 0x29076170, 0xecdd4775, 0x619f1510, 0x13cca830, 0xeb61bd96, 0x0334fe1e, 0xaa0363cf,
            0xb5735c90, 0x4c70a239, 0xd59e9e0b, 0xcbaade14, 0xeecc86bc, 0x60622ca7, 0x9cab5cab, 0xb2f3846e,
            0x648b1eaf, 0x19bdf0ca, 0xa02369b9, 0x655abb50, 0x40685a32, 0x3c2ab4b3, 0x319ee9d5, 0xc021b8f7,
            0x9b540b19, 0x875fa099, 0x95f7997e, 0x623d7da8, 0xf837889a, 0x97e32d77, 0x11ed935f, 0x16681281,
            0x0e358829, 0xc7e61fd6, 0x96dedfa1, 0x7858ba99, 0x57f584a5, 0x1b227263, 0x9b83c3ff, 0x1ac24696,
            0xcdb30aeb, 0x532e3054, 0x8fd948e4, 0x6dbc3128, 0x58ebf2ef, 0x34c6ffea, 0xfe28ed61, 0xee7c3c73,
            0x5d4a14d9, 0xe864b7e3, 0x42105d14, 0x203e13e0, 0x45eee2b6, 0xa3aaabea, 0xdb6c4f15, 0xfacb4fd0,
            0xc742f442, 0xef6abbb5, 0x654f3b1d, 0x41cd2105, 0xd81e799e, 0x86854dc7, 0xe44b476a, 0x3d816250,
            0xcf62a1f2, 0x5b8d2646, 0xfc8883a0, 0xc1c7b6a3, 0x7f1524c3, 0x69cb7492, 0x47848a0b, 0x5692b285,
            0x095bbf00, 0xad19489d, 0x1462b174, 0x23820e00, 0x58428d2a, 0x0c55f5ea, 0x1dadf43e, 0x233f7061,
            0x3372f092, 0x8d937e41, 0xd65fecf1, 0x6c223bdb, 0x7cde3759, 0xcbee7460, 0x4085f2a7, 0xce77326e,
            0xa6078084, 0x19f8509e, 0xe8efd855, 0x61d99735, 0xa969a7aa, 0xc50c06c2, 0x5a04abfc, 0x800bcadc,
            0x9e447a2e, 0xc3453484, 0xfdd56705, 0x0e1e9ec9, 0xdb73dbd3, 0x105588cd, 0x675fda79, 0xe3674340,
            0xc5c43465, 0x713e38d8, 0x3d28f89e, 0xf16dff20, 0x153e21e7, 0x8fb03d4a, 0xe6e39f2b, 0xdb83adf7,
            0xe93d5a68, 0x948140f7, 0xf64c261c, 0x94692934, 0x411520f7, 0x7602d4f7, 0xbcf46b2e, 0xd4a20068,
            0xd4082471, 0x3320f46a, 0x43b7d4b7, 0x500061af, 0x1e39f62e, 0x97244546, 0x14214f74, 0xbf8b8840,
            0x4d95fc1d, 0x96b591af, 0x70f4ddd3, 0x66a02f45, 0xbfbc09ec, 0x03bd9785, 0x7fac6dd0, 0x31cb8504,
            0x96eb27b3, 0x55fd3941, 0xda2547e6, 0xabca0a9a, 0x28507825, 0x530429f4, 0x0a2c86da, 0xe9b66dfb,
            0x68dc1462, 0xd7486900, 0x680ec0a4, 0x27a18dee, 0x4f3ffea2, 0xe887ad8c, 0xb58ce006, 0x7af4d6b6,
            0xaace1e7c, 0xd3375fec, 0xce78a399, 0x406b2a42, 0x20fe9e35, 0xd9f385b9, 0xee39d7ab, 0x3b124e8b,
            0x1dc9faf7, 0x4b6d1856, 0x26a36631, 0xeae397b2, 0x3a6efa74, 0xdd5b4332, 0x6841e7f7, 0xca7820fb,
            0xfb0af54e, 0xd8feb397, 0x454056ac, 0xba489527, 0x55533a3a, 0x20838d87, 0xfe6ba9b7, 0xd096954b,
            0x55a867bc, 0xa1159a58, 0xcca92963, 0x99e1db33, 0xa62a4a56, 0x3f3125f9, 0x5ef47e1c, 0x9029317c,
            0xfdf8e802, 0x04272f70, 0x80bb155c, 0x05282ce3, 0x95c11548, 0xe4c66d22, 0x48c1133f, 0xc70f86dc,
            0x07f9c9ee, 0x41041f0f, 0x404779a4, 0x5d886e17, 0x325f51eb, 0xd59bc0d1, 0xf2bcc18f, 0x41113564,
            0x257b7834, 0x602a9c60, 0xdff8e8a3, 0x1f636c1b, 0x0e12b4c2, 0x02e1329e, 0xaf664fd1, 0xcad18115,
            0x6b2395e0, 0x333e92e1, 0x3b240b62, 0xeebeb922, 0x85b2a20e, 0xe6ba0d99, 0xde720c8c, 0x2da2f728,
            0xd0127845, 0x95b794fd, 0x647d0862, 0xe7ccf5f0, 0x5449a36f, 0x877d48fa, 0xc39dfd27, 0xf33e8d1e,
            0x0a476341, 0x992eff74, 0x3a6f6eab, 0xf4f8fd37, 0xa812dc60, 0xa1ebddf8, 0x991be14c, 0xdb6e6b0d,
            0xc67b5510, 0x6d672c37, 0x2765d43b, 0xdcd0e804, 0xf1290dc7, 0xcc00ffa3, 0xb5390f92, 0x690fed0b,
            0x667b9ffb, 0xcedb7d9c, 0xa091cf0b, 0xd9155ea3, 0xbb132f88, 0x515bad24, 0x7b9479bf, 0x763bd6eb,
            0x37392eb3, 0xcc115979, 0x8026e297, 0xf42e312d, 0x6842ada7, 0xc66a2b3b, 0x12754ccc, 0x782ef11c,
            0x6a124237, 0xb79251e7, 0x06a1bbe6, 0x4bfb6350, 0x1a6b1018, 0x11caedfa, 0x3d25bdd8, 0xe2e1c3c9,
            0x44421659, 0x0a121386, 0xd90cec6e, 0xd5abea2a, 0x64af674e, 0xda86a85f, 0xbebfe988, 0x64e4c3fe,
            0x9dbc8057, 0xf0f7c086, 0x60787bf8, 0x6003604d, 0xd1fd8346, 0xf6381fb0, 0x7745ae04, 0xd736fccc,
            0x83426b33, 0xf01eab71, 0xb0804187, 0x3c005e5f, 0x77a057be, 0xbde8ae24, 0x55464299, 0xbf582e61,
            0x4e58f48f, 0xf2ddfda2, 0xf474ef38, 0x8789bdc2, 0x5366f9c3, 0xc8b38e74, 0xb475f255, 0x46fcd9b9,
            0x7aeb2661, 0x8b1ddf84, 0x846a0e79, 0x915f95e2, 0x466e598e, 0x20b45770, 0x8cd55591, 0xc902de4c,
            0xb90bace1, 0xbb8205d0, 0x11a86248, 0x7574a99e, 0xb77f19b6, 0xe0a9dc09, 0x662d09a1, 0xc4324633,
            0xe85a1f02, 0x09f0be8c, 0x4a99a025, 0x1d6efe10, 0x1ab93d1d, 0x0ba5a4df, 0xa186f20f, 0x2868f169,
            0xdcb7da83, 0x573906fe, 0xa1e2ce9b, 0x4fcd7f52, 0x50115e01, 0xa70683fa, 0xa002b5c4, 0x0de6d027,
            0x9af88c27, 0x773f8641, 0xc3604c06, 0x61a806b5, 0xf0177a28, 0xc0f586e0, 0x006058aa, 0x30dc7d62,
            0x11e69ed7, 0x2338ea63, 0x53c2dd94, 0xc2c21634, 0xbbcbee56, 0x90bcb6de, 0xebfc7da1, 0xce591d76,
            0x6f05e409, 0x4b7c0188, 0x39720a3d, 0x7c927c24, 0x86e3725f, 0x724d9db9, 0x1ac15bb4, 0xd39eb8fc,
            0xed545578, 0x08fca5b5, 0xd83d7cd3, 0x4dad0fc4, 0x1e50ef5e, 0xb161e6f8, 0xa28514d9, 0x6c51133c,
            0x6fd5c7e7, 0x56e14ec4, 0x362abfce, 0xddc6c837, 0xd79a3234, 0x92638212, 0x670efa8e, 0x406000e0,
            0x3a39ce37, 0xd3faf5cf, 0xabc27737, 0x5ac52d1b, 0x5cb0679e, 0x4fa33742, 0xd3822740, 0x99bc9bbe,
            0xd5118e9d, 0xbf0f7315, 0xd62d1c7e, 0xc700c47b, 0xb78c1b6b, 0x21a19045, 0xb26eb1be, 0x6a366eb4,
            0x5748ab2f, 0xbc946e79, 0xc6a376d2, 0x6549c2c8, 0x530ff8ee, 0x468dde7d, 0xd5730a1d, 0x4cd04dc6,
            0x2939bbdb, 0xa9ba4650, 0xac9526e8, 0xbe5ee304, 0xa1fad5f0, 0x6a2d519a, 0x63ef8ce2, 0x9a86ee22,
            0xc089c2b8, 0x43242ef6, 0xa51e03aa, 0x9cf2d0a4, 0x83c061ba, 0x9be96a4d, 0x8fe51550, 0xba645bd6,
            0x2826a2f9, 0xa73a3ae1, 0x4ba99586, 0xef5562e9, 0xc72fefd3, 0xf752f7da, 0x3f046f69, 0x77fa0a59,
            0x80e4a915, 0x87b08601, 0x9b09e6ad, 0x3b3ee593, 0xe990fd5a, 0x9e34d797, 0x2cf0b7d9, 0x022b8b51,
            0x96d5ac3a, 0x017da67d, 0xd1cf3ed6, 0x7c7d2d28, 0x1f9f25cf, 0xadf2b89b, 0x5ad6b472, 0x5a88f54c,
            0xe029ac71, 0xe019a5e6, 0x47b0acfd, 0xed93fa9b, 0xe8d3c48d, 0x283b57cc, 0xf8d56629, 0x79132e28,
            0x785f0191, 0xed756055, 0xf7960e44, 0xe3d35e8c, 0x15056dd4, 0x88f46dba, 0x03a16125, 0x0564f0bd,
            0xc3eb9e15, 0x3c9057a2, 0x97271aec, 0xa93a072a, 0x1b3f6d9b, 0x1e6321f5, 0xf59c66fb, 0x26dcf319,
            0x7533d928, 0xb155fdf5, 0x03563482, 0x8aba3cbb, 0x28517711, 0xc20ad9f8, 0xabcc5167, 0xccad925f,
            0x4de81751, 0x3830dc8e, 0x379d5862, 0x9320f991, 0xea7a90c2, 0xfb3e7bce, 0x5121ce64, 0x774fbe32,
            0xa8b6e37e, 0xc3293d46, 0x48de5369, 0x6413e680, 0xa2ae0810, 0xdd6db224, 0x69852dfd, 0x09072166,
            0xb39a460a, 0x6445c0dd, 0x586cdecf, 0x1c20c8ae, 0x5bbef7dd, 0x1b588d40, 0xccd2017f, 0x6bb4e3bb,
            0xdda26a7e, 0x3a59ff45, 0x3e350a44, 0xbcb4cdd5, 0x72eacea8, 0xfa6484bb, 0x8d6612ae, 0xbf3c6f47,
            0xd29be463, 0x542f5d9e, 0xaec2771b, 0xf64e6370, 0x740e0d8d, 0xe75b1357, 0xf8721671, 0xaf537d5d,
            0x4040cb08, 0x4eb4e2cc, 0x34d2466a, 0x0115af84, 0xe1b00428, 0x95983a1d, 0x06b89fb4, 0xce6ea048,
            0x6f3f3b82, 0x3520ab82, 0x011a1d4b, 0x277227f8, 0x611560b1, 0xe7933fdc, 0xbb3a792b, 0x344525bd,
            0xa08839e1, 0x51ce794b, 0x2f32c9b7, 0xa01fbac9, 0xe01cc87e, 0xbcc7d1f6, 0xcf0111c3, 0xa1e8aac7,
            0x1a908749, 0xd44fbd9a, 0xd0dadecb, 0xd50ada38, 0x0339c32a, 0xc6913667, 0x8df9317c, 0xe0b12b4f,
            0xf79e59b7, 0x43f5bb3a, 0xf2d519ff, 0x27d9459c, 0xbf97222c, 0x15e6fc2a, 0x0f91fc71, 0x9b941525,
            0xfae59361, 0xceb69ceb, 0xc2a86459, 0x12baa8d1, 0xb6c1075e, 0xe3056a0c, 0x10d25065, 0xcb03a442,
            0xe0ec6e0e, 0x1698db3b, 0x4c98a0be, 0x3278e964, 0x9f1f9532, 0xe0d392df, 0xd3a0342b, 0x8971f21e,
            0x1b0a7441, 0x4ba3348c, 0xc5be7120, 0xc37632d8, 0xdf359f8d, 0x9b992f2e, 0xe60b6f47, 0x0fe3f11d,
            0xe54cda54, 0x1edad891, 0xce6279cf, 0xcd3e7e6f, 0x1618b166, 0xfd2c1d05, 0x848fd2c5, 0xf6fb2299,
            0xf523f357, 0xa6327623, 0x93a83531, 0x56cccd02, 0xacf08162, 0x5a75ebb5, 0x6e163697, 0x88d273cc,
            0xde966292, 0x81b949d0, 0x4c50901b, 0x71c65614, 0xe6c6c7bd, 0x327a140a, 0x45e1d006, 0xc3f27b9a,
            0xc9aa53fd, 0x62a80f00, 0xbb25bfe2, 0x35bdd2f6, 0x71126905, 0xb2040222, 0xb6cbcf7c, 0xcd769c2b,
            0x53113ec0, 0x1640e3d3, 0x38abbd60, 0x2547adf0, 0xba38209c, 0xf746ce76, 0x77afa1c5, 0x20756060,
            0x85cbfe4e, 0x8ae88dd8, 0x7aaaf9b0, 0x4cf9aa7e, 0x1948c25c, 0x02fb8a8c, 0x01c36ae4, 0xd6ebe1f9,
            0x90d4f869, 0xa65cdea0, 0x3f09252d, 0xc208e69f, 0xb74e6132, 0xce77e25b, 0x578fdfe3, 0x3ac372e6 ];

        // Set of keys
        public static readonly byte[,] g_key_table = {
            { 0x91, 0x3C, 0x2B, 0x0F, 0x44, 0xC6 },
            { 0x0C, 0x96, 0xD2, 0x40, 0x93, 0x21 },
            { 0xF2, 0x12, 0xA5, 0xAA, 0xDA, 0xE9 },
            { 0x9A, 0xD4, 0xF7, 0x14, 0x97, 0xD0 },
            { 0xFC, 0xC9, 0xC7, 0xD6, 0xA8, 0xA3 },
            { 0x7B, 0x67, 0x36, 0x9B, 0x0B, 0x1A },
            { 0x03, 0xAC, 0xF9, 0x02, 0xAE, 0x2D },
            { 0x01, 0x77, 0x79, 0x6B, 0x0C, 0x67 },
            { 0xA4, 0xB4, 0x1E, 0xD7, 0xAA, 0x51 },
            { 0xD6, 0xE1, 0xBC, 0x27, 0x15, 0x25 },
            { 0x17, 0x17, 0x47, 0x65, 0x40, 0x8B },
            { 0xB8, 0x19, 0xDB, 0x4E, 0x17, 0x74 },
            { 0xAA, 0x63, 0xAC, 0x37, 0xA0, 0x8F },
            { 0x77, 0xCD, 0x5D, 0x23, 0xEF, 0xB7 },
            { 0x13, 0x2B, 0x83, 0xBF, 0x0F, 0x8C },
            { 0xB1, 0x0B, 0xC8, 0x6F, 0x39, 0x4D },
            { 0xA1, 0xA5, 0xFA, 0x2B, 0xC6, 0xE2 },
            { 0x9C, 0x29, 0xCC, 0x26, 0xE9, 0x2D },
            { 0xCD, 0x6F, 0xD2, 0xCA, 0xBE, 0x47 },
            { 0x9B, 0x21, 0xAE, 0x3E, 0x31, 0x69 },
            { 0xE7, 0x0B, 0xE6, 0x6F, 0xCF, 0x91 },
            { 0x88, 0x59, 0xAF, 0x90, 0xC5, 0x2D },
            { 0xAE, 0xD2, 0x52, 0xB5, 0x28, 0x98 },
            { 0x3B, 0x7F, 0x65, 0xED, 0x5E, 0x93 },
            { 0x30, 0xBF, 0x0A, 0x34, 0xDB, 0x3D }};

        // Seed Table
        public static readonly byte[][][][] g_seed_table = [ //, CRYPT_GAME_SEED_COUNT, 2, CRYPT_GAME_SEED_LENGTH]
                new byte[CRYPT_GAME_SEED_COUNT][][] {
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x9E, 0xEC, 0x5B, 0x3C, 0x8F, 0xA8, 0x8C, 0x55 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xB6, 0x21, 0x71, 0x98, 0xA4, 0x47, 0x22, 0x58 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xF8, 0xC4, 0xD8, 0x72, 0x54, 0xFC, 0xF9, 0xDE }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x2D, 0x53, 0xDB, 0x32, 0x03, 0x10, 0x5A, 0x18 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x89, 0x9F, 0x5C, 0x53, 0x06, 0x7F, 0x44, 0x38 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x32, 0xCE, 0xAC, 0xDB, 0x91, 0x44, 0x4E, 0x1E }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x29, 0x78, 0x5A, 0xF0, 0xAB, 0x00, 0x7F, 0x91 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xE6, 0xB6, 0xD2, 0xE7, 0xA0, 0x05, 0xC2, 0xF2 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x8D, 0x46, 0xA9, 0xBB, 0x52, 0x1B, 0x41, 0xDF }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xF0, 0x4A, 0xC9, 0x14, 0x27, 0xA9, 0x6B, 0x4A }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x91, 0x4B, 0x8A, 0x80, 0xF5, 0xCF, 0xBB, 0x3C }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xBC, 0xF4, 0xC9, 0xD5, 0x42, 0x7A, 0xFA, 0xB7 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xD5, 0x8C, 0x01, 0xC0, 0xFD, 0x1E, 0xAA, 0x57 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xC1, 0x20, 0x7A, 0x38, 0x2C, 0xB7, 0xCD, 0x14 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x55, 0x9F, 0xD1, 0x5B, 0xFB, 0x70, 0xC0, 0x77 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xA4, 0x15, 0xB3, 0x9F, 0x6B, 0xBB, 0x10, 0x5A }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x80, 0x9D, 0x16, 0x54, 0x6B, 0x7C, 0x5F, 0xAD }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x35, 0xCB, 0x92, 0x24, 0x08, 0x11, 0xD9, 0x61 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x24, 0xA7, 0x75, 0xBF, 0x4D, 0x7E, 0x70, 0x0C }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x90, 0xCF, 0x9C, 0x04, 0xAC, 0x53, 0x89, 0xEF }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x99, 0x22, 0xF6, 0x89, 0x10, 0xE6, 0x72, 0x23 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x0A, 0x5C, 0xA5, 0xFF, 0x9C, 0x78, 0xDA, 0x7F }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xDF, 0xFF, 0xBB, 0x11, 0x6B, 0x75, 0xF0, 0x29 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xA5, 0x86, 0xD0, 0x53, 0x77, 0xE7, 0xB1, 0x0D }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x4C, 0x06, 0xDA, 0x55, 0x4E, 0x50, 0x1B, 0x7A }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x1C, 0x90, 0xCE, 0x64, 0xD6, 0x17, 0x52, 0xFB }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x00, 0x26, 0x75, 0x25, 0xCD, 0x95, 0x15, 0x0F }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x13, 0xD8, 0xAB, 0x30, 0xF1, 0xC5, 0xC5, 0xFA }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x0C, 0x8E, 0x86, 0x1E, 0x3F, 0xCB, 0x8B, 0xD1 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xEC, 0xCE, 0xA9, 0x96, 0x91, 0x11, 0xB4, 0x97 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x1E, 0x65, 0x5F, 0xA4, 0x55, 0xEB, 0xEC, 0xCF }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x19, 0xD9, 0x9F, 0xE0, 0x5E, 0x57, 0x45, 0x73 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x0E, 0x2D, 0x18, 0xE1, 0x55, 0x05, 0x04, 0xBF }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x5E, 0x81, 0x1F, 0xDD, 0xFF, 0x5C, 0xC3, 0xF4 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xF2, 0x06, 0x56, 0x54, 0x4D, 0xFB, 0x96, 0x54 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x33, 0x97, 0x07, 0x43, 0x4F, 0x39, 0xC4, 0xA8 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x5E, 0x02, 0x37, 0x17, 0x7B, 0x64, 0xE6, 0xA2 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x2E, 0x24, 0x13, 0x07, 0xFE, 0xA1, 0x88, 0xB7 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x60, 0xDD, 0x4C, 0xE0, 0xA1, 0xDC, 0xBA, 0x6C }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x81, 0x5C, 0x3F, 0x93, 0x7A, 0x1F, 0x2A, 0x1C }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xAE, 0x5C, 0xBE, 0x9D, 0x84, 0x6F, 0xCB, 0x51 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x4D, 0x13, 0xC6, 0x81, 0x28, 0xC3, 0x03, 0x34 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xB0, 0x5D, 0xCB, 0x8D, 0x69, 0x1C, 0xDE, 0x29 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x31, 0xF1, 0x22, 0xC3, 0x1C, 0x82, 0x8A, 0x57 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x08, 0x32, 0x8B, 0xA2, 0x1E, 0x12, 0xC9, 0xB9 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xCD, 0xA8, 0xE6, 0x1C, 0x59, 0xAC, 0x0C, 0xF6 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xA5, 0x3B, 0xE4, 0x64, 0x2F, 0x45, 0x33, 0xA2 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x4A, 0xDA, 0x39, 0xE2, 0x0E, 0x94, 0xF2, 0xAA }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xB0, 0x82, 0xB7, 0x33, 0xD2, 0x6F, 0xC0, 0x00 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xD7, 0x8D, 0x1F, 0x8E, 0x79, 0x85, 0x3E, 0x2A }]
                },
                new byte[CRYPT_GAME_SEED_COUNT][][] {
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xD2, 0xB7, 0xF6, 0x9C, 0xCF, 0x06, 0xE8, 0xC1 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xAE, 0xEB, 0x7F, 0xE9, 0x87, 0x28, 0x1C, 0x9B }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xE8, 0x8C, 0x2A, 0x97, 0xD1, 0xD2, 0xA6, 0x76 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xAD, 0x23, 0x69, 0xA0, 0xEF, 0x1F, 0x8C, 0xBA }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x24, 0x62, 0x40, 0x0B, 0x21, 0xC6, 0x07, 0x89 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xBA, 0x60, 0x9E, 0x26, 0x98, 0x18, 0xAF, 0x01 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xDF, 0x2B, 0x56, 0xC9, 0xB3, 0x72, 0x35, 0x8D }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x1D, 0x4F, 0x61, 0xAF, 0x53, 0x12, 0x6E, 0x49 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x1C, 0x87, 0x6C, 0xB1, 0xD4, 0x1B, 0xA2, 0xB2 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xD4, 0xA1, 0x2C, 0xE2, 0x2F, 0xE9, 0xA4, 0x62 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x17, 0x83, 0x1C, 0x68, 0xB3, 0xD6, 0x65, 0x2D }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x81, 0x5B, 0x4D, 0x9B, 0x15, 0x6F, 0x0B, 0xDF }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xCE, 0x91, 0xB9, 0x8A, 0x61, 0x20, 0xB1, 0xF9 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xCA, 0x0A, 0xC4, 0x76, 0x5B, 0x4B, 0xAB, 0x16 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x5B, 0xD2, 0x4A, 0xFD, 0x44, 0xB7, 0xDF, 0x1F }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x8B, 0x6F, 0xAB, 0x0C, 0xAB, 0x3D, 0x0C, 0x7A }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x35, 0x6C, 0xBD, 0xFF, 0x62, 0x53, 0x77, 0x44 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xF2, 0x44, 0x5F, 0x8C, 0x59, 0x25, 0x5F, 0x6B }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xB5, 0x27, 0x0D, 0xD2, 0x23, 0xBE, 0x40, 0xB3 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x3E, 0x8B, 0x92, 0xB1, 0x78, 0x57, 0xCB, 0xB0 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xB3, 0xB4, 0xB6, 0xD5, 0xB6, 0xA7, 0x66, 0x6E }, new byte[CRYPT_GAME_SEED_LENGTH] { 0xFB, 0xA7, 0x32, 0x93, 0xEE, 0x79, 0x61, 0x45 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x49, 0xD7, 0x93, 0x34, 0x90, 0x1A, 0xAD, 0x2C }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x84, 0x3E, 0xE9, 0x0B, 0x2C, 0xC6, 0xB3, 0xB1 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x82, 0xFB, 0x86, 0xEC, 0xA8, 0x76, 0x55, 0x98 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x7E, 0xE3, 0xA2, 0x47, 0xB6, 0x72, 0x05, 0x61 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x0B, 0xA5, 0x72, 0x17, 0xCB, 0x18, 0xAE, 0x03 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x8C, 0x61, 0x32, 0xD9, 0x2B, 0x42, 0xEF, 0xF2 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x3F, 0x0A, 0x06, 0x82, 0x09, 0xC9, 0x76, 0xF2 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x3D, 0x54, 0x50, 0xFD, 0x25, 0xA2, 0x2F, 0x2E }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xF1, 0x34, 0x64, 0x94, 0xDC, 0x90, 0x58, 0x5D }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x1E, 0x6F, 0xB4, 0xEF, 0x73, 0xE8, 0xB0, 0xED }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xC0, 0xD2, 0xE1, 0x42, 0xEC, 0x04, 0x69, 0xA8 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x27, 0x9C, 0x7C, 0x79, 0x87, 0x9A, 0xB2, 0x48 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x50, 0x73, 0xEC, 0x1E, 0x4D, 0xD0, 0x80, 0x51 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x46, 0x21, 0xC9, 0xF8, 0x93, 0xCC, 0xE8, 0x41 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x70, 0xC9, 0xE4, 0x78, 0x8F, 0x6B, 0x2C, 0x27 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x4C, 0x7E, 0x2C, 0x5A, 0x15, 0x69, 0x64, 0xDD }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x00, 0xC7, 0x09, 0xCD, 0xF6, 0x2D, 0x2D, 0x31 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x6F, 0x01, 0x01, 0x3E, 0xCD, 0x60, 0x16, 0xB4 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xE7, 0xE8, 0x76, 0xC4, 0x50, 0x4F, 0x08, 0x5B }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x62, 0x28, 0x24, 0x42, 0x7D, 0x9A, 0x19, 0x26 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x2F, 0xD4, 0x67, 0xB9, 0x24, 0x0C, 0xBB, 0x14 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x7D, 0x19, 0xC8, 0x73, 0x79, 0xA7, 0x70, 0xCF }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x2D, 0x53, 0xDC, 0x91, 0x83, 0xF2, 0x0C, 0x12 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x3B, 0xAF, 0x1B, 0x6B, 0x02, 0x99, 0x8B, 0x61 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0xE3, 0x2C, 0xA2, 0x54, 0xCD, 0x51, 0xAF, 0xE5 }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x18, 0x58, 0x11, 0x7F, 0xF0, 0x50, 0x9C, 0x15 }],
                    [new byte[CRYPT_GAME_SEED_LENGTH] { 0x6E, 0x26, 0x01, 0xE9, 0xDB, 0x50, 0x13, 0xEA }, new byte[CRYPT_GAME_SEED_LENGTH] { 0x22, 0x59, 0x30, 0x3B, 0xE4, 0x5F, 0x43, 0x1E }]
                }
        ];

        public static readonly uint[][] p_table = new uint[CRYPT_GAME_KEY_COUNT][]; // 18
        public static readonly uint[][] s_table = new uint[CRYPT_GAME_KEY_COUNT][]; // 1024

        public static void N2L(ref byte* C, ref uint LL) {
            LL = (uint)*C++ << 24;
            LL |= (uint)*C++ << 16;
            LL |= (uint)*C++ << 8;
            LL |= *C++;
        }

        public static void L2N(ref uint LL, ref byte* C) {
            (*C++) = (byte)((LL >> 24) & 0xff);
            (*C++) = (byte)((LL >> 16) & 0xff);
            (*C++) = (byte)((LL >> 8) & 0xff);
            (*C++) = (byte)(LL & 0xff);
        }

        public static void L2N(ref uint LL, uint R, uint P, byte[] S)
            => LL = (uint)(LL ^ P ^ (((S[R >> 24] + S[0x0100 + ((R >> 16) & 0xff)]) ^ S[0x0200 + ((R >> 8) & 0xff)]) + S[0x0300 + (R & 0xff)]));

        public static void Round(ref uint LL, uint R, uint[] S, uint P)
            => LL = LL ^ P ^ (((S[R >> 24] + S[0x0100 + ((R >> 16) & 0xff)]) ^ S[0x0200 + ((R >> 8) & 0xff)]) + S[0x0300 + (R & 0xff)]);
    }
}

#endregion

#region LoginCryptBehaviour

class LoginCryptBehaviour {
    uint _k1, _k2, _k3;
    readonly uint[] _key = new uint[2];
    readonly byte[] _seed = new byte[4];

    public void Initialize(uint seed, uint k1, uint k2, uint k3) {
        _seed[0] = (byte)((seed >> 24) & 0xFF);
        _seed[1] = (byte)((seed >> 16) & 0xFF);
        _seed[2] = (byte)((seed >> 8) & 0xFF);
        _seed[3] = (byte)(seed & 0xFF);
        _k1 = k1;
        _k2 = k2;
        _k3 = k3;
        const uint seed_key = 0x0000_1357;
        _key[0] = ((~seed ^ seed_key) << 16) | ((seed ^ 0xffffaaaa) & 0x0000ffff);
        _key[1] = ((seed ^ 0x43210000) >> 16) | ((~seed ^ 0xabcdffff) & 0xffff0000);
    }


    public void Encrypt(Span<byte> src, Span<byte> dst, int size) {
        for (var i = 0; i < size; i++) {
            dst[i] = (byte)(src[i] ^ (byte)_key[0]);
            uint table0 = _key[0], table1 = _key[1];
            _key[1] = (((((table1 >> 1) | (table0 << 31)) ^ _k1) >> 1) | (table0 << 31)) ^ _k2;
            _key[0] = ((table0 >> 1) | (table1 << 31)) ^ _k3;
        }
    }

    public void Encrypt_OLD(Span<byte> src, Span<byte> dst, int size) {
        for (var i = 0; i < size; i++) {
            dst[i] = (byte)(src[i] ^ (byte)_key[0]);
            uint table0 = _key[0], table1 = _key[1];
            _key[0] = ((table0 >> 1) | (table1 << 31)) ^ _k2;
            _key[1] = ((table1 >> 1) | (table0 << 31)) ^ _k1;
        }
    }

    public void Encrypt_1_25_36(Span<byte> src, Span<byte> dst, int size) {
        for (var i = 0; i < size; i++) {
            dst[i] = (byte)(src[i] ^ (byte)_key[0]);
            uint table0 = _key[0], table1 = _key[1];
            _key[0] = ((table0 >> 1) | (table1 << 31)) ^ _k2;
            _key[1] = ((table1 >> 1) | (table0 << 31)) ^ _k1;
            _key[1] = (_k1 >> (byte)((5 * table1 * table1) & 0xFF)) + table1 * _k1 + table0 * table0 * 0x35ce9581 + 0x07afcc37;
            _key[0] = (_k2 >> (byte)((3 * table0 * table0) & 0xFF)) + table0 * _k2 + _key[1] * _key[1] * 0x4c3a1353 + 0x16ef783f;
        }
    }
}

#endregion

#region MD5Behaviour

unsafe static class MD5Behaviour {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MD5Context {
        public ulong Size;
        public fixed uint _buffer[4];
        public fixed byte _input[64];
        public fixed byte _digest[16];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref uint Buffer(int index) { fixed (uint* ptr = &_buffer[0]) return ref *(ptr + index); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref byte Input(int index) { fixed (byte* ptr = &_input[0]) return ref *(ptr + index); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref byte Digest(int index) { fixed (byte* ptr = &_digest[0]) return ref *(ptr + index); }
    }

    const uint A = 0x67452301;
    const uint B = 0xefcdab89;
    const uint C = 0x98badcfe;
    const uint D = 0x10325476;

    static uint[] _s =  {
            7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22,
            5,  9, 14, 20, 5,  9, 14, 20, 5,  9, 14, 20, 5,  9, 14, 20,
            4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23,
            6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21 };

    static uint[] _k = {
            0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee,
            0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
            0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be,
            0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
            0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa,
            0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
            0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed,
            0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
            0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c,
            0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
            0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05,
            0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
            0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039,
            0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
            0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1,
            0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391 };

    static byte[] _padding =  {
            0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint F(uint x, uint y, uint z) => ((x & y) | (~x & z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint G(uint x, uint y, uint z) => ((x & z) | (y & ~z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint H(uint x, uint y, uint z) => (x ^ y ^ z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint I(uint x, uint y, uint z) => (y ^ (x | ~z));

    public static void Initialize(ref MD5Context ctx) {
        ctx.Size = 0;
        ctx.Buffer(0) = A;
        ctx.Buffer(1) = B;
        ctx.Buffer(2) = C;
        ctx.Buffer(3) = D;
    }

    public static void Update(ref MD5Context ctx, ReadOnlySpan<byte> inputBuffer) {
        Span<uint> input = stackalloc uint[16];
        var offset = (int)(ctx.Size % 64);
        ctx.Size += (ulong)inputBuffer.Length;
        for (var i = 0; i < inputBuffer.Length; i++) {
            ctx.Input(offset++) = inputBuffer[i];
            if ((offset % 64) == 0) {
                for (int j = 0; j < 16; ++j)
                    input[j] =
                        (uint)(ctx.Input((j * 4) + 3)) << 24 |
                        (uint)(ctx.Input((j * 4) + 2)) << 16 |
                        (uint)(ctx.Input((j * 4) + 1)) << 8 |
                        (uint)(ctx.Input((j * 4)));
                Step(ref ctx, input);
                offset = 0;
            }
        }
    }

    public static void Finalize(ref MD5Context ctx) {
        Span<uint> input = stackalloc uint[16];
        var offset = (int)(ctx.Size % 64);
        var paddingLength = (uint)(offset < 56 ? 56 - offset : (56 + 64) - offset);
        Update(ref ctx, _padding.AsSpan(0, (int)paddingLength));
        ctx.Size -= (ulong)paddingLength;
        for (var j = 0; j < 14; ++j)
            input[j] =
                (uint)(ctx.Input((j * 4) + 3)) << 24 |
                (uint)(ctx.Input((j * 4) + 2)) << 16 |
                (uint)(ctx.Input((j * 4) + 1)) << 8 |
                (uint)(ctx.Input((j * 4)));
        input[14] = (uint)(ctx.Size * 8);
        input[15] = (uint)((ctx.Size * 8) >> 32);
        Step(ref ctx, input);
        for (var i = 0; i < 4; ++i) {
            ctx.Digest((i * 4) + 0) = (byte)((ctx.Buffer(i) & 0x000000FF));
            ctx.Digest((i * 4) + 1) = (byte)((ctx.Buffer(i) & 0x0000FF00) >> 8);
            ctx.Digest((i * 4) + 2) = (byte)((ctx.Buffer(i) & 0x00FF0000) >> 16);
            ctx.Digest((i * 4) + 3) = (byte)((ctx.Buffer(i) & 0xFF000000) >> 24);
        }
    }

    static void Step(ref MD5Context ctx, Span<uint> input) {
        uint AA = ctx.Buffer(0);
        uint BB = ctx.Buffer(1);
        uint CC = ctx.Buffer(2);
        uint DD = ctx.Buffer(3);
        uint E;
        int j;
        for (var i = 0; i < 64; ++i) {
            switch (i / 16) {
                case 0: E = F(BB, CC, DD); j = i; break;
                case 1: E = G(BB, CC, DD); j = ((i * 5) + 1) % 16; break;
                case 2: E = H(BB, CC, DD); j = ((i * 3) + 5) % 16; break;
                default: E = I(BB, CC, DD); j = (i * 7) % 16; break;
            }
            var temp = DD;
            DD = CC;
            CC = BB;
            BB = BB + RotateLeft(AA + E + _k[i] + input[j], _s[i]);
            AA = temp;
        }
        ctx.Buffer(0) += AA;
        ctx.Buffer(1) += BB;
        ctx.Buffer(2) += CC;
        ctx.Buffer(3) += DD;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint RotateLeft(uint x, uint n) => (uint)((x << (int)n) | (x >> (int)(32 - n)));
}

#endregion

#region  TwofishEncryption

enum CipherMode {
    CBC = 1,
    ECB,
    OFB,
    CFB,
    CTS
}

class TwofishEncryption : TwofishBase {
    byte[] _cipher_table, _xor_data;
    ushort _rect_pos;
    byte _send_pos;

    // not worked out this property yet - placing break points here just don't get caught.
    // I normally set this to false when block encrypting so that I can work on one block at a time
    // but for compression and stream type ciphers this can be set to true so that you get all the data
    EncryptionDirection encryptionDirection;

    // need to have this method due to IDisposable - just can't think of a reason to use it for in this class
    public void Dispose() { }

    /// <summary>
    /// Transform a block depending on whether we are encrypting or decrypting
    /// </summary>
    /// <param name="inputBuffer"></param>
    /// <param name="inputOffset"></param>
    /// <param name="inputCount"></param>
    /// <param name="outputBuffer"></param>
    /// <param name="outputOffset"></param>
    /// <returns></returns>
    public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) {
        Span<uint> x = stackalloc uint[4];
        for (var i = 0; i < 4; i++) x[i] = (uint)(inputBuffer[i * 4 + 3 + inputOffset] << 24) | (uint)(inputBuffer[i * 4 + 2 + inputOffset] << 16) | (uint)(inputBuffer[i * 4 + 1 + inputOffset] << 8) | inputBuffer[i * 4 + 0 + inputOffset];
        if (encryptionDirection == EncryptionDirection.Encrypting) blockEncrypt(ref x);
        else blockDecrypt(ref x);
        for (var i = 0; i < 4; i++) {
            outputBuffer[i * 4 + 0 + outputOffset] = b0(x[i]);
            outputBuffer[i * 4 + 1 + outputOffset] = b1(x[i]);
            outputBuffer[i * 4 + 2 + outputOffset] = b2(x[i]);
            outputBuffer[i * 4 + 3 + outputOffset] = b3(x[i]);
        }
        return inputCount;
    }

    public unsafe byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) {
        byte[] outputBuffer; // = new byte[0];
        if (inputCount > 0) {
            outputBuffer = new byte[16]; // blocksize
            Span<uint> x = stackalloc uint[4];
            // should be okay as we have already said to pad with zeros
            for (var i = 0; i < 4; i++) x[i] = (uint)(inputBuffer[i * 4 + 3 + inputOffset] << 24) | (uint)(inputBuffer[i * 4 + 2 + inputOffset] << 16) | (uint)(inputBuffer[i * 4 + 1 + inputOffset] << 8) | inputBuffer[i * 4 + 0 + inputOffset];
            if (encryptionDirection == EncryptionDirection.Encrypting) blockEncrypt(ref x);
            else blockDecrypt(ref x);
            for (var i = 0; i < 4; i++) {
                outputBuffer[i * 4 + 0] = b0(x[i]);
                outputBuffer[i * 4 + 1] = b1(x[i]);
                outputBuffer[i * 4 + 2] = b2(x[i]);
                outputBuffer[i * 4 + 3] = b3(x[i]);
            }
        }
        else outputBuffer = new byte[0]; // the .NET framework doesn't like it if you return null - this calms it down
        return outputBuffer;
    }

    public bool CanReuseTransform { get; } = true;

    public bool CanTransformMultipleBlocks { get; } = false;

    public int InputBlockSize => inputBlockSize;

    public int OutputBlockSize => outputBlockSize;

    public unsafe void Initialize(uint seed, bool use_md5) {
        int keyLen = 128;
        _cipher_table = new byte[0x100];
        Span<byte> key = stackalloc byte[16];
        key[0] = key[4] = key[8] = key[12] = (byte)((seed >> 24) & 0xff);
        key[1] = key[5] = key[9] = key[13] = (byte)((seed >> 16) & 0xff);
        key[2] = key[6] = key[10] = key[14] = (byte)((seed >> 8) & 0xff);
        key[3] = key[7] = key[11] = key[15] = (byte)(seed & 0xff);
        byte[] iv = new byte[0];
        // convert our key into an array of ints
        for (var i = 0; i < key.Length / 4; i++) Key[i] = (uint)(key[i * 4 + 3] << 24) | (uint)(key[i * 4 + 2] << 16) | (uint)(key[i * 4 + 1] << 8) | key[i * 4 + 0];
        cipherMode = CipherMode.ECB;
        // we only need to convert our IV if we are using CBC
        if (cipherMode == CipherMode.CBC)
            for (var i = 0; i < 4; i++) IV[i] = (uint)(iv[i * 4 + 3] << 24) | (uint)(iv[i * 4 + 2] << 16) | (uint)(iv[i * 4 + 1] << 8) | iv[i * 4 + 0];
        encryptionDirection = EncryptionDirection.Decrypting;
        reKey(keyLen, ref Key);
        for (var i = 0; i < 256; ++i) _cipher_table[i] = (byte)i;
        _send_pos = 0;
        refreshCipherTable();
        if (use_md5) {
            var ctx = new MD5Behaviour.MD5Context();
            MD5Behaviour.Initialize(ref ctx);
            MD5Behaviour.Update(ref ctx, _cipher_table.AsSpan(0, 256));
            MD5Behaviour.Finalize(ref ctx);
            _xor_data = new byte[16];
            for (var i = 0; i < 16; ++i) _xor_data[i] = ctx.Digest(i);
            //using (var md5 = MD5.Create()) _xor_data = md5.ComputeHash(_cipher_table, 0, 256);
        }
    }

    public void Encrypt(Span<byte> src, Span<byte> dst, int size) {
        for (var i = 0; i < size; ++i) {
            // Recalculate table
            if (_rect_pos >= 0x100) refreshCipherTable();
            // Simple XOR operation
            dst[i] = (byte)(src[i] ^ _cipher_table[_rect_pos++]);
        }
    }

    public void Decrypt(Span<byte> src, Span<byte> dst, int size) {
        for (var i = 0; i < size; ++i) {
            dst[i] = (byte)(src[i] ^ _xor_data[_send_pos]);
            _send_pos++;
            _send_pos &= 0x0F; // Maximum Value is 0xF = 15, then 0xF + 1 = 0 again
        }
    }

    void refreshCipherTable() {
        Span<uint> cache = stackalloc uint[4];
        Span<byte> table = _cipher_table;
        for (var i = 0; i < 256; i += 16) {
            table.Slice(i, 16).CopyTo(MemoryMarshal.AsBytes(cache));
            blockEncrypt(ref cache);
            MemoryMarshal.AsBytes(cache).CopyTo(table.Slice(i, 16));
        }
        _rect_pos = 0;
    }
}

/// <summary>
/// Summary description for TwofishBase.
/// </summary>
class TwofishBase {
    public enum EncryptionDirection {
        Encrypting,
        Decrypting
    }

    int keyLength;

    readonly int[] numRounds = { 0, ROUNDS_128, ROUNDS_192, ROUNDS_256 };
    int rounds;

    protected CipherMode cipherMode = CipherMode.ECB;

    protected int inputBlockSize = BLOCK_SIZE / 8;
    protected uint[] IV = { 0, 0, 0, 0 };              // this should be one block size
    protected uint[] Key = { 0, 0, 0, 0, 0, 0, 0, 0 }; //new int[MAX_KEY_BITS/32];
    protected int outputBlockSize = BLOCK_SIZE / 8;

    protected uint[] sboxKeys = new uint[MAX_KEY_BITS / 64]; /* key bits used for S-boxes */
    protected uint[] subKeys = new uint[TOTAL_SUBKEYS];      /* round subkeys, input/output whitening bits */

    static unsafe uint f32(uint x, ref uint[] k32, int keyLen) {
        byte* b = stackalloc byte[4];
        b[0] = b0(x);
        b[1] = b1(x);
        b[2] = b2(x);
        b[3] = b3(x);
        /* Run each byte thru 8x8 S-boxes, xoring with key byte at each stage. */
        /* Note that each byte goes through a different combination of S-boxes.*/
        //*((DWORD *)b) = Bswap(x);	/* make b[0] = LSB, b[3] = MSB */
        switch (((keyLen + 63) / 64) & 3) {
            case 0: /* 256 bits of key */
                b[0] = (byte)(P8x8[P_04, b[0]] ^ b0(k32[3]));
                b[1] = (byte)(P8x8[P_14, b[1]] ^ b1(k32[3]));
                b[2] = (byte)(P8x8[P_24, b[2]] ^ b2(k32[3]));
                b[3] = (byte)(P8x8[P_34, b[3]] ^ b3(k32[3]));
                /* fall thru, having pre-processed b[0]..b[3] with k32[3] */
                goto case 3;
            case 3: /* 192 bits of key */
                b[0] = (byte)(P8x8[P_03, b[0]] ^ b0(k32[2]));
                b[1] = (byte)(P8x8[P_13, b[1]] ^ b1(k32[2]));
                b[2] = (byte)(P8x8[P_23, b[2]] ^ b2(k32[2]));
                b[3] = (byte)(P8x8[P_33, b[3]] ^ b3(k32[2]));
                /* fall thru, having pre-processed b[0]..b[3] with k32[2] */
                goto case 2;
            case 2: /* 128 bits of key */
                b[0] = P8x8[P_00, P8x8[P_01, P8x8[P_02, b[0]] ^ b0(k32[1])] ^ b0(k32[0])];
                b[1] = P8x8[P_10, P8x8[P_11, P8x8[P_12, b[1]] ^ b1(k32[1])] ^ b1(k32[0])];
                b[2] = P8x8[P_20, P8x8[P_21, P8x8[P_22, b[2]] ^ b2(k32[1])] ^ b2(k32[0])];
                b[3] = P8x8[P_30, P8x8[P_31, P8x8[P_32, b[3]] ^ b3(k32[1])] ^ b3(k32[0])];
                break;
        }
        /* Now perform the MDS matrix multiply inline. */
        return (uint)(M00(b[0]) ^ M01(b[1]) ^ M02(b[2]) ^ M03(b[3])) ^ (uint)((M10(b[0]) ^ M11(b[1]) ^ M12(b[2]) ^ M13(b[3])) << 8) ^ (uint)((M20(b[0]) ^ M21(b[1]) ^ M22(b[2]) ^ M23(b[3])) << 16) ^ (uint)((M30(b[0]) ^ M31(b[1]) ^ M32(b[2]) ^ M33(b[3])) << 24);
    }

    protected bool reKey(int keyLen, ref uint[] key32) {
        int i, k64Cnt;
        keyLength = keyLen;
        rounds = numRounds[(keyLen - 1) / 64];
        int subkeyCnt = ROUND_SUBKEYS + 2 * rounds;
        uint A, B;
        uint[] k32e = new uint[MAX_KEY_BITS / 64];
        uint[] k32o = new uint[MAX_KEY_BITS / 64]; /* even/odd key dwords */

        k64Cnt = (keyLen + 63) / 64; /* round up to next multiple of 64 bits */

        for (i = 0; i < k64Cnt; i++) {
            /* split into even/odd key dwords */
            k32e[i] = key32[2 * i];
            k32o[i] = key32[2 * i + 1];
            /* compute S-box keys using (12,8) Reed-Solomon code over GF(256) */
            sboxKeys[k64Cnt - 1 - i] = RS_MDS_Encode(k32e[i], k32o[i]); /* reverse order */
        }

        for (i = 0; i < subkeyCnt / 2; i++) /* compute round subkeys for PHT */
        {
            A = f32((uint)(i * SK_STEP), ref k32e, keyLen);           /* A uses even key dwords */
            B = f32((uint)(i * SK_STEP + SK_BUMP), ref k32o, keyLen); /* B uses odd  key dwords */
            B = ROL(B, 8);
            subKeys[2 * i] = A + B; /* combine with a PHT */
            subKeys[2 * i + 1] = ROL(A + 2 * B, SK_ROTL);
        }

        return true;
    }

    protected unsafe void blockDecrypt(ref Span<uint> x) {
        uint t0, t1;
        Span<uint> xtemp = stackalloc uint[4];

        if (cipherMode == CipherMode.CBC) {
            x.CopyTo(xtemp);
        }

        for (int i = 0; i < BLOCK_HALF_SIZE; i++) /* copy in the block, add whitening */
        {
            x[i] ^= subKeys[OUTPUT_WHITEN + i];
        }

        for (int r = rounds - 1; r >= 0; r--) /* main Twofish decryption loop */
        {
            t0 = f32(x[0], ref sboxKeys, keyLength);
            t1 = f32(ROL(x[1], 8), ref sboxKeys, keyLength);

            x[2] = ROL(x[2], 1);
            x[2] ^= t0 + t1 + subKeys[ROUND_SUBKEYS + 2 * r]; /* PHT, round keys */
            x[3] ^= t0 + 2 * t1 + subKeys[ROUND_SUBKEYS + 2 * r + 1];
            x[3] = ROR(x[3], 1);

            if (r > 0) /* unswap, except for last round */
            {
                t0 = x[0];
                x[0] = x[2];
                x[2] = t0;
                t1 = x[1];
                x[1] = x[3];
                x[3] = t1;
            }
        }

        for (int i = 0; i < BLOCK_HALF_SIZE; i++) /* copy out, with whitening */
        {
            x[i] ^= subKeys[INPUT_WHITEN + i];

            if (cipherMode == CipherMode.CBC) {
                x[i] ^= IV[i];
                IV[i] = xtemp[i];
            }
        }
    }

    public unsafe void blockEncrypt(ref Span<uint> x) {
        uint t0, t1, tmp;

        for (int i = 0; i < BLOCK_HALF_SIZE; i++) /* copy in the block, add whitening */
        {
            x[i] ^= subKeys[INPUT_WHITEN + i];

            if (cipherMode == CipherMode.CBC) {
                x[i] ^= IV[i];
            }
        }

        for (int r = 0; r < rounds; r++) /* main Twofish encryption loop */ // 16==rounds
        {
#if FEISTEL
				t0 = f32(ROR(x[0],  (r+1)/2),ref sboxKeys,keyLength);
				t1 = f32(ROL(x[1],8+(r+1)/2),ref sboxKeys,keyLength);
											/* PHT, round keys */
				x[2] ^= ROL(t0 +   t1 + subKeys[ROUND_SUBKEYS+2*r  ], r    /2);
				x[3] ^= ROR(t0 + 2*t1 + subKeys[ROUND_SUBKEYS+2*r+1],(r+2) /2);

#else
            t0 = f32(x[0], ref sboxKeys, keyLength);
            t1 = f32(ROL(x[1], 8), ref sboxKeys, keyLength);

            x[3] = ROL(x[3], 1);
            x[2] ^= t0 + t1 + subKeys[ROUND_SUBKEYS + 2 * r]; /* PHT, round keys */
            x[3] ^= t0 + 2 * t1 + subKeys[ROUND_SUBKEYS + 2 * r + 1];
            x[2] = ROR(x[2], 1);

#endif
            if (r < rounds - 1) /* swap for next round */
            {
                tmp = x[0];
                x[0] = x[2];
                x[2] = tmp;
                tmp = x[1];
                x[1] = x[3];
                x[3] = tmp;
            }
        }
#if FEISTEL
			x[0] = ROR(x[0],8);                     /* "final permutation" */
			x[1] = ROL(x[1],8);
			x[2] = ROR(x[2],8);
			x[3] = ROL(x[3],8);
#endif
        for (int i = 0; i < BLOCK_HALF_SIZE; i++) /* copy out, with whitening */
        {
            x[i] ^= subKeys[OUTPUT_WHITEN + i];

            if (cipherMode == CipherMode.CBC) {
                IV[i] = x[i];
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint RS_MDS_Encode(uint k0, uint k1) {
        uint i, j;
        uint r;

        for (i = r = 0; i < 2; i++) {
            r ^= i > 0 ? k0 : k1; /* merge in 32 more key bits */

            for (j = 0; j < 4; j++) /* shift one byte at a time */
            {
                RS_rem(ref r);
            }
        }

        return r;
    }


    #region These are all the definitions that were found in AES.H

    static readonly int BLOCK_SIZE = 128; /* number of bits per block */
    static readonly int BLOCK_HALF_SIZE = BLOCK_SIZE >> 5;
    static readonly int MAX_ROUNDS = 16;    /* max # rounds (for allocating subkey array) */
    static readonly int ROUNDS_128 = 16;    /* default number of rounds for 128-bit keys*/
    static readonly int ROUNDS_192 = 16;    /* default number of rounds for 192-bit keys*/
    static readonly int ROUNDS_256 = 16;    /* default number of rounds for 256-bit keys*/
    static readonly int MAX_KEY_BITS = 256; /* max number of bits of key */

    //#define		VALID_SIG	 0x48534946	/* initialization signature ('FISH') */
    //#define		MCT_OUTER			400	/* MCT outer loop */
    //#define		MCT_INNER		  10000	/* MCT inner loop */
    //#define		REENTRANT			  1	/* nonzero forces reentrant code (slightly slower) */

    static readonly int INPUT_WHITEN = 0; /* subkey array indices */
    static readonly int OUTPUT_WHITEN = INPUT_WHITEN + BLOCK_SIZE / 32;
    static readonly int ROUND_SUBKEYS = OUTPUT_WHITEN + BLOCK_SIZE / 32; /* use 2 * (# rounds) */
    static readonly int TOTAL_SUBKEYS = ROUND_SUBKEYS + 2 * MAX_ROUNDS;

    #endregion

    #region These are all the definitions that were found in TABLE.H that we need

    /* for computing subkeys */
    static readonly uint SK_STEP = 0x02020202u;
    static readonly uint SK_BUMP = 0x01010101u;
    static readonly int SK_ROTL = 9;

    /* Reed-Solomon code parameters: (12,8) reversible code
    g(x) = x**4 + (a + 1/a) x**3 + a x**2 + (a + 1/a) x + 1
    where a = primitive root of field generator 0x14D */
    static readonly uint RS_GF_FDBK = 0x14D; /* field generator */

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void RS_rem(ref uint x) {
        byte b = (byte)(x >> 24);
        // TODO: maybe change g2 and g3 to bytes
        uint g2 = (uint)(((b << 1) ^ ((b & 0x80) == 0x80 ? RS_GF_FDBK : 0)) & 0xFF);
        uint g3 = (uint)(((b >> 1) & 0x7F) ^ ((b & 1) == 1 ? RS_GF_FDBK >> 1 : 0) ^ g2);
        x = (x << 8) ^ (g3 << 24) ^ (g2 << 16) ^ (g3 << 8) ^ b;
    }

    static readonly int MDS_GF_FDBK = 0x169; /* primitive polynomial for GF(256)*/

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int LFSR1(int x) => (x >> 1) ^ ((x & 0x01) == 0x01 ? MDS_GF_FDBK / 2 : 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int LFSR2(int x) => (x >> 2) ^ ((x & 0x02) == 0x02 ? MDS_GF_FDBK / 2 : 0) ^ ((x & 0x01) == 0x01 ? MDS_GF_FDBK / 4 : 0);

    // TODO: not the most efficient use of code but it allows us to update the code a lot quicker we can possibly optimize this code once we have got it all working

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Mx_1(int x) => x; /* force result to int so << will work */

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Mx_X(int x) => x ^ LFSR2(x); /* 5B */

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Mx_Y(int x) => x ^ LFSR1(x) ^ LFSR2(x); /* EF */

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int M00(int x) => Mul_1(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int M01(int x) => Mul_Y(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int M02(int x) => Mul_X(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int M03(int x) => Mul_X(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int M10(int x) => Mul_X(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int M11(int x) => Mul_Y(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int M12(int x) => Mul_Y(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int M13(int x) => Mul_1(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int M20(int x) => Mul_Y(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int M21(int x) => Mul_X(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int M22(int x) => Mul_1(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int M23(int x) => Mul_Y(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int M30(int x) => Mul_Y(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int M31(int x) => Mul_1(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int M32(int x) => Mul_Y(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int M33(int x) => Mul_X(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Mul_1(int x) => Mx_1(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Mul_X(int x) => Mx_X(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Mul_Y(int x) => Mx_Y(x);

    /*	Define the fixed p0/p1 permutations used in keyed S-box lookup.
        By changing the following constant definitions for P_ij, the S-boxes will
        automatically get changed in all the Twofish source code. Note that P_i0 is
        the "outermost" 8x8 permutation applied.  See the f32() function to see
        how these constants are to be  used.
    */
    static readonly int P_00 = 1; /* "outermost" permutation */
    static readonly int P_01 = 0;
    static readonly int P_02 = 0;
    static readonly int P_03 = P_01 ^ 1; /* "extend" to larger key sizes */
    static readonly int P_04 = 1;

    static readonly int P_10 = 0;
    static readonly int P_11 = 0;
    static readonly int P_12 = 1;
    static readonly int P_13 = P_11 ^ 1;
    static readonly int P_14 = 0;

    static readonly int P_20 = 1;
    static readonly int P_21 = 1;
    static readonly int P_22 = 0;
    static readonly int P_23 = P_21 ^ 1;
    static readonly int P_24 = 0;

    static readonly int P_30 = 0;
    static readonly int P_31 = 1;
    static readonly int P_32 = 1;
    static readonly int P_33 = P_31 ^ 1;
    static readonly int P_34 = 1;

    /* fixed 8x8 permutation S-boxes */

    /***********************************************************************
    *  07:07:14  05/30/98  [4x4]  TestCnt=256. keySize=128. CRC=4BD14D9E.
    * maxKeyed:  dpMax = 18. lpMax =100. fixPt =  8. skXor =  0. skDup =  6.
    * log2(dpMax[ 6..18])=   --- 15.42  1.33  0.89  4.05  7.98 12.05
    * log2(lpMax[ 7..12])=  9.32  1.01  1.16  4.23  8.02 12.45
    * log2(fixPt[ 0.. 8])=  1.44  1.44  2.44  4.06  6.01  8.21 11.07 14.09 17.00
    * log2(skXor[ 0.. 0])
    * log2(skDup[ 0.. 6])=   ---  2.37  0.44  3.94  8.36 13.04 17.99
    ***********************************************************************/
    static readonly byte[,] P8x8 =
   {
            /*  p0:   */
            /*  dpMax      = 10.  lpMax      = 64.  cycleCnt=   1  1  1  0.         */
            /* 817D6F320B59ECA4.ECB81235F4A6709D.BA5E6D90C8F32471.D7F4126E9B3085CA. */
            /* Karnaugh maps:
            *  0111 0001 0011 1010. 0001 1001 1100 1111. 1001 1110 0011 1110. 1101 0101 1111 1001.
            *  0101 1111 1100 0100. 1011 0101 0010 0000. 0101 1000 1100 0101. 1000 0111 0011 0010.
            *  0000 1001 1110 1101. 1011 1000 1010 0011. 0011 1001 0101 0000. 0100 0010 0101 1011.
            *  0111 0100 0001 0110. 1000 1011 1110 1001. 0011 0011 1001 1101. 1101 0101 0000 1100.
            */
            {
                0xA9, 0x67, 0xB3, 0xE8, 0x04, 0xFD, 0xA3, 0x76,
                0x9A, 0x92, 0x80, 0x78, 0xE4, 0xDD, 0xD1, 0x38,
                0x0D, 0xC6, 0x35, 0x98, 0x18, 0xF7, 0xEC, 0x6C,
                0x43, 0x75, 0x37, 0x26, 0xFA, 0x13, 0x94, 0x48,
                0xF2, 0xD0, 0x8B, 0x30, 0x84, 0x54, 0xDF, 0x23,
                0x19, 0x5B, 0x3D, 0x59, 0xF3, 0xAE, 0xA2, 0x82,
                0x63, 0x01, 0x83, 0x2E, 0xD9, 0x51, 0x9B, 0x7C,
                0xA6, 0xEB, 0xA5, 0xBE, 0x16, 0x0C, 0xE3, 0x61,
                0xC0, 0x8C, 0x3A, 0xF5, 0x73, 0x2C, 0x25, 0x0B,
                0xBB, 0x4E, 0x89, 0x6B, 0x53, 0x6A, 0xB4, 0xF1,
                0xE1, 0xE6, 0xBD, 0x45, 0xE2, 0xF4, 0xB6, 0x66,
                0xCC, 0x95, 0x03, 0x56, 0xD4, 0x1C, 0x1E, 0xD7,
                0xFB, 0xC3, 0x8E, 0xB5, 0xE9, 0xCF, 0xBF, 0xBA,
                0xEA, 0x77, 0x39, 0xAF, 0x33, 0xC9, 0x62, 0x71,
                0x81, 0x79, 0x09, 0xAD, 0x24, 0xCD, 0xF9, 0xD8,
                0xE5, 0xC5, 0xB9, 0x4D, 0x44, 0x08, 0x86, 0xE7,
                0xA1, 0x1D, 0xAA, 0xED, 0x06, 0x70, 0xB2, 0xD2,
                0x41, 0x7B, 0xA0, 0x11, 0x31, 0xC2, 0x27, 0x90,
                0x20, 0xF6, 0x60, 0xFF, 0x96, 0x5C, 0xB1, 0xAB,
                0x9E, 0x9C, 0x52, 0x1B, 0x5F, 0x93, 0x0A, 0xEF,
                0x91, 0x85, 0x49, 0xEE, 0x2D, 0x4F, 0x8F, 0x3B,
                0x47, 0x87, 0x6D, 0x46, 0xD6, 0x3E, 0x69, 0x64,
                0x2A, 0xCE, 0xCB, 0x2F, 0xFC, 0x97, 0x05, 0x7A,
                0xAC, 0x7F, 0xD5, 0x1A, 0x4B, 0x0E, 0xA7, 0x5A,
                0x28, 0x14, 0x3F, 0x29, 0x88, 0x3C, 0x4C, 0x02,
                0xB8, 0xDA, 0xB0, 0x17, 0x55, 0x1F, 0x8A, 0x7D,
                0x57, 0xC7, 0x8D, 0x74, 0xB7, 0xC4, 0x9F, 0x72,
                0x7E, 0x15, 0x22, 0x12, 0x58, 0x07, 0x99, 0x34,
                0x6E, 0x50, 0xDE, 0x68, 0x65, 0xBC, 0xDB, 0xF8,
                0xC8, 0xA8, 0x2B, 0x40, 0xDC, 0xFE, 0x32, 0xA4,
                0xCA, 0x10, 0x21, 0xF0, 0xD3, 0x5D, 0x0F, 0x00,
                0x6F, 0x9D, 0x36, 0x42, 0x4A, 0x5E, 0xC1, 0xE0
            },
            /*  p1:   */
            /*  dpMax      = 10.  lpMax      = 64.  cycleCnt=   2  0  0  1.         */
            /* 28BDF76E31940AC5.1E2B4C376DA5F908.4C75169A0ED82B3F.B951C3DE647F208A. */
            /* Karnaugh maps:
            *  0011 1001 0010 0111. 1010 0111 0100 0110. 0011 0001 1111 0100. 1111 1000 0001 1100.
            *  1100 1111 1111 1010. 0011 0011 1110 0100. 1001 0110 0100 0011. 0101 0110 1011 1011.
            *  0010 0100 0011 0101. 1100 1000 1000 1110. 0111 1111 0010 0110. 0000 1010 0000 0011.
            *  1101 1000 0010 0001. 0110 1001 1110 0101. 0001 0100 0101 0111. 0011 1011 1111 0010.
            */
            {
                0x75, 0xF3, 0xC6, 0xF4, 0xDB, 0x7B, 0xFB, 0xC8,
                0x4A, 0xD3, 0xE6, 0x6B, 0x45, 0x7D, 0xE8, 0x4B,
                0xD6, 0x32, 0xD8, 0xFD, 0x37, 0x71, 0xF1, 0xE1,
                0x30, 0x0F, 0xF8, 0x1B, 0x87, 0xFA, 0x06, 0x3F,
                0x5E, 0xBA, 0xAE, 0x5B, 0x8A, 0x00, 0xBC, 0x9D,
                0x6D, 0xC1, 0xB1, 0x0E, 0x80, 0x5D, 0xD2, 0xD5,
                0xA0, 0x84, 0x07, 0x14, 0xB5, 0x90, 0x2C, 0xA3,
                0xB2, 0x73, 0x4C, 0x54, 0x92, 0x74, 0x36, 0x51,
                0x38, 0xB0, 0xBD, 0x5A, 0xFC, 0x60, 0x62, 0x96,
                0x6C, 0x42, 0xF7, 0x10, 0x7C, 0x28, 0x27, 0x8C,
                0x13, 0x95, 0x9C, 0xC7, 0x24, 0x46, 0x3B, 0x70,
                0xCA, 0xE3, 0x85, 0xCB, 0x11, 0xD0, 0x93, 0xB8,
                0xA6, 0x83, 0x20, 0xFF, 0x9F, 0x77, 0xC3, 0xCC,
                0x03, 0x6F, 0x08, 0xBF, 0x40, 0xE7, 0x2B, 0xE2,
                0x79, 0x0C, 0xAA, 0x82, 0x41, 0x3A, 0xEA, 0xB9,
                0xE4, 0x9A, 0xA4, 0x97, 0x7E, 0xDA, 0x7A, 0x17,
                0x66, 0x94, 0xA1, 0x1D, 0x3D, 0xF0, 0xDE, 0xB3,
                0x0B, 0x72, 0xA7, 0x1C, 0xEF, 0xD1, 0x53, 0x3E,
                0x8F, 0x33, 0x26, 0x5F, 0xEC, 0x76, 0x2A, 0x49,
                0x81, 0x88, 0xEE, 0x21, 0xC4, 0x1A, 0xEB, 0xD9,
                0xC5, 0x39, 0x99, 0xCD, 0xAD, 0x31, 0x8B, 0x01,
                0x18, 0x23, 0xDD, 0x1F, 0x4E, 0x2D, 0xF9, 0x48,
                0x4F, 0xF2, 0x65, 0x8E, 0x78, 0x5C, 0x58, 0x19,
                0x8D, 0xE5, 0x98, 0x57, 0x67, 0x7F, 0x05, 0x64,
                0xAF, 0x63, 0xB6, 0xFE, 0xF5, 0xB7, 0x3C, 0xA5,
                0xCE, 0xE9, 0x68, 0x44, 0xE0, 0x4D, 0x43, 0x69,
                0x29, 0x2E, 0xAC, 0x15, 0x59, 0xA8, 0x0A, 0x9E,
                0x6E, 0x47, 0xDF, 0x34, 0x35, 0x6A, 0xCF, 0xDC,
                0x22, 0xC9, 0xC0, 0x9B, 0x89, 0xD4, 0xED, 0xAB,
                0x12, 0xA2, 0x0D, 0x52, 0xBB, 0x02, 0x2F, 0xA9,
                0xD7, 0x61, 0x1E, 0xB4, 0x50, 0x04, 0xF6, 0xC2,
                0x16, 0x25, 0x86, 0x56, 0x55, 0x09, 0xBE, 0x91
            }
        };

    #endregion

    #region These are all the definitions that were found in PLATFORM.H that we need

    // left rotation
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint ROL(uint x, int n) {
        return (x << (n & 0x1F)) | (x >> (32 - (n & 0x1F)));
    }

    // right rotation
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint ROR(uint x, int n) {
        return (x >> (n & 0x1F)) | (x << (32 - (n & 0x1F)));
    }

    // first byte
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static byte b0(uint x) {
        return (byte)x; //& 0xFF);
    }

    // second byte
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static byte b1(uint x) {
        return (byte)(x >> 8); // & (0xFF));
    }

    // third byte
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static byte b2(uint x) {
        return (byte)(x >> 16); // & (0xFF));
    }

    // fourth byte
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static byte b3(uint x) {
        return (byte)(x >> 24); // & (0xFF));
    }

    #endregion
}

#endregion