using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace GameX.Crytek;

public static class FarCryX {
    static class CRC32 {
        static readonly uint[] Table = [
            0x00000000u, 0x77073096u, 0xEE0E612Cu, 0x990951BAu,
            0x076DC419u, 0x706AF48Fu, 0xE963A535u, 0x9E6495A3u,
            0x0EDB8832u, 0x79DCB8A4u, 0xE0D5E91Eu, 0x97D2D988u,
            0x09B64C2Bu, 0x7EB17CBDu, 0xE7B82D07u, 0x90BF1D91u,
            0x1DB71064u, 0x6AB020F2u, 0xF3B97148u, 0x84BE41DEu,
            0x1ADAD47Du, 0x6DDDE4EBu, 0xF4D4B551u, 0x83D385C7u,
            0x136C9856u, 0x646BA8C0u, 0xFD62F97Au, 0x8A65C9ECu,
            0x14015C4Fu, 0x63066CD9u, 0xFA0F3D63u, 0x8D080DF5u,
            0x3B6E20C8u, 0x4C69105Eu, 0xD56041E4u, 0xA2677172u,
            0x3C03E4D1u, 0x4B04D447u, 0xD20D85FDu, 0xA50AB56Bu,
            0x35B5A8FAu, 0x42B2986Cu, 0xDBBBC9D6u, 0xACBCF940u,
            0x32D86CE3u, 0x45DF5C75u, 0xDCD60DCFu, 0xABD13D59u,
            0x26D930ACu, 0x51DE003Au, 0xC8D75180u, 0xBFD06116u,
            0x21B4F4B5u, 0x56B3C423u, 0xCFBA9599u, 0xB8BDA50Fu,
            0x2802B89Eu, 0x5F058808u, 0xC60CD9B2u, 0xB10BE924u,
            0x2F6F7C87u, 0x58684C11u, 0xC1611DABu, 0xB6662D3Du,
            0x76DC4190u, 0x01DB7106u, 0x98D220BCu, 0xEFD5102Au,
            0x71B18589u, 0x06B6B51Fu, 0x9FBFE4A5u, 0xE8B8D433u,
            0x7807C9A2u, 0x0F00F934u, 0x9609A88Eu, 0xE10E9818u,
            0x7F6A0DBBu, 0x086D3D2Du, 0x91646C97u, 0xE6635C01u,
            0x6B6B51F4u, 0x1C6C6162u, 0x856530D8u, 0xF262004Eu,
            0x6C0695EDu, 0x1B01A57Bu, 0x8208F4C1u, 0xF50FC457u,
            0x65B0D9C6u, 0x12B7E950u, 0x8BBEB8EAu, 0xFCB9887Cu,
            0x62DD1DDFu, 0x15DA2D49u, 0x8CD37CF3u, 0xFBD44C65u,
            0x4DB26158u, 0x3AB551CEu, 0xA3BC0074u, 0xD4BB30E2u,
            0x4ADFA541u, 0x3DD895D7u, 0xA4D1C46Du, 0xD3D6F4FBu,
            0x4369E96Au, 0x346ED9FCu, 0xAD678846u, 0xDA60B8D0u,
            0x44042D73u, 0x33031DE5u, 0xAA0A4C5Fu, 0xDD0D7CC9u,
            0x5005713Cu, 0x270241AAu, 0xBE0B1010u, 0xC90C2086u,
            0x5768B525u, 0x206F85B3u, 0xB966D409u, 0xCE61E49Fu,
            0x5EDEF90Eu, 0x29D9C998u, 0xB0D09822u, 0xC7D7A8B4u,
            0x59B33D17u, 0x2EB40D81u, 0xB7BD5C3Bu, 0xC0BA6CADu,
            0xEDB88320u, 0x9ABFB3B6u, 0x03B6E20Cu, 0x74B1D29Au,
            0xEAD54739u, 0x9DD277AFu, 0x04DB2615u, 0x73DC1683u,
            0xE3630B12u, 0x94643B84u, 0x0D6D6A3Eu, 0x7A6A5AA8u,
            0xE40ECF0Bu, 0x9309FF9Du, 0x0A00AE27u, 0x7D079EB1u,
            0xF00F9344u, 0x8708A3D2u, 0x1E01F268u, 0x6906C2FEu,
            0xF762575Du, 0x806567CBu, 0x196C3671u, 0x6E6B06E7u,
            0xFED41B76u, 0x89D32BE0u, 0x10DA7A5Au, 0x67DD4ACCu,
            0xF9B9DF6Fu, 0x8EBEEFF9u, 0x17B7BE43u, 0x60B08ED5u,
            0xD6D6A3E8u, 0xA1D1937Eu, 0x38D8C2C4u, 0x4FDFF252u,
            0xD1BB67F1u, 0xA6BC5767u, 0x3FB506DDu, 0x48B2364Bu,
            0xD80D2BDAu, 0xAF0A1B4Cu, 0x36034AF6u, 0x41047A60u,
            0xDF60EFC3u, 0xA867DF55u, 0x316E8EEFu, 0x4669BE79u,
            0xCB61B38Cu, 0xBC66831Au, 0x256FD2A0u, 0x5268E236u,
            0xCC0C7795u, 0xBB0B4703u, 0x220216B9u, 0x5505262Fu,
            0xC5BA3BBEu, 0xB2BD0B28u, 0x2BB45A92u, 0x5CB36A04u,
            0xC2D7FFA7u, 0xB5D0CF31u, 0x2CD99E8Bu, 0x5BDEAE1Du,
            0x9B64C2B0u, 0xEC63F226u, 0x756AA39Cu, 0x026D930Au,
            0x9C0906A9u, 0xEB0E363Fu, 0x72076785u, 0x05005713u,
            0x95BF4A82u, 0xE2B87A14u, 0x7BB12BAEu, 0x0CB61B38u,
            0x92D28E9Bu, 0xE5D5BE0Du, 0x7CDCEFB7u, 0x0BDBDF21u,
            0x86D3D2D4u, 0xF1D4E242u, 0x68DDB3F8u, 0x1FDA836Eu,
            0x81BE16CDu, 0xF6B9265Bu, 0x6FB077E1u, 0x18B74777u,
            0x88085AE6u, 0xFF0F6A70u, 0x66063BCAu, 0x11010B5Cu,
            0x8F659EFFu, 0xF862AE69u, 0x616BFFD3u, 0x166CCF45u,
            0xA00AE278u, 0xD70DD2EEu, 0x4E048354u, 0x3903B3C2u,
            0xA7672661u, 0xD06016F7u, 0x4969474Du, 0x3E6E77DBu,
            0xAED16A4Au, 0xD9D65ADCu, 0x40DF0B66u, 0x37D83BF0u,
            0xA9BCAE53u, 0xDEBB9EC5u, 0x47B2CF7Fu, 0x30B5FFE9u,
            0xBDBDF21Cu, 0xCABAC28Au, 0x53B39330u, 0x24B4A3A6u,
            0xBAD03605u, 0xCDD70693u, 0x54DE5729u, 0x23D967BFu,
            0xB3667A2Eu, 0xC4614AB8u, 0x5D681B02u, 0x2A6F2B94u,
            0xB40BBE37u, 0xC30C8EA1u, 0x5A05DF1Bu, 0x2D02EF8Du];
        public static uint Compute(string value) {
            var hash = 0xFFFFFFFFu;
            if (value != null)
                for (var i = 0; i < value.Length; i++) hash = Table[(byte)hash ^ (byte)value[i]] ^ (hash >> 8);
            return ~hash;
        }
        public static uint Compute(byte[] buffer, int offset, int count) {
            var hash = 0xFFFFFFFFu;
            for (var i = offset; i < offset + count; i++) hash = Table[(byte)hash ^ buffer[i]] ^ (hash >> 8);
            return ~hash;
        }
    }

    static class CRC64 {
        static readonly ulong[] Table = [
            0x0000000000000000ul, 0x01B0000000000000ul, 0x0360000000000000ul, 0x02D0000000000000ul,
            0x06C0000000000000ul, 0x0770000000000000ul, 0x05A0000000000000ul, 0x0410000000000000ul,
            0x0D80000000000000ul, 0x0C30000000000000ul, 0x0EE0000000000000ul, 0x0F50000000000000ul,
            0x0B40000000000000ul, 0x0AF0000000000000ul, 0x0820000000000000ul, 0x0990000000000000ul,
            0x1B00000000000000ul, 0x1AB0000000000000ul, 0x1860000000000000ul, 0x19D0000000000000ul,
            0x1DC0000000000000ul, 0x1C70000000000000ul, 0x1EA0000000000000ul, 0x1F10000000000000ul,
            0x1680000000000000ul, 0x1730000000000000ul, 0x15E0000000000000ul, 0x1450000000000000ul,
            0x1040000000000000ul, 0x11F0000000000000ul, 0x1320000000000000ul, 0x1290000000000000ul,
            0x3600000000000000ul, 0x37B0000000000000ul, 0x3560000000000000ul, 0x34D0000000000000ul,
            0x30C0000000000000ul, 0x3170000000000000ul, 0x33A0000000000000ul, 0x3210000000000000ul,
            0x3B80000000000000ul, 0x3A30000000000000ul, 0x38E0000000000000ul, 0x3950000000000000ul,
            0x3D40000000000000ul, 0x3CF0000000000000ul, 0x3E20000000000000ul, 0x3F90000000000000ul,
            0x2D00000000000000ul, 0x2CB0000000000000ul, 0x2E60000000000000ul, 0x2FD0000000000000ul,
            0x2BC0000000000000ul, 0x2A70000000000000ul, 0x28A0000000000000ul, 0x2910000000000000ul,
            0x2080000000000000ul, 0x2130000000000000ul, 0x23E0000000000000ul, 0x2250000000000000ul,
            0x2640000000000000ul, 0x27F0000000000000ul, 0x2520000000000000ul, 0x2490000000000000ul,
            0x6C00000000000000ul, 0x6DB0000000000000ul, 0x6F60000000000000ul, 0x6ED0000000000000ul,
            0x6AC0000000000000ul, 0x6B70000000000000ul, 0x69A0000000000000ul, 0x6810000000000000ul,
            0x6180000000000000ul, 0x6030000000000000ul, 0x62E0000000000000ul, 0x6350000000000000ul,
            0x6740000000000000ul, 0x66F0000000000000ul, 0x6420000000000000ul, 0x6590000000000000ul,
            0x7700000000000000ul, 0x76B0000000000000ul, 0x7460000000000000ul, 0x75D0000000000000ul,
            0x71C0000000000000ul, 0x7070000000000000ul, 0x72A0000000000000ul, 0x7310000000000000ul,
            0x7A80000000000000ul, 0x7B30000000000000ul, 0x79E0000000000000ul, 0x7850000000000000ul,
            0x7C40000000000000ul, 0x7DF0000000000000ul, 0x7F20000000000000ul, 0x7E90000000000000ul,
            0x5A00000000000000ul, 0x5BB0000000000000ul, 0x5960000000000000ul, 0x58D0000000000000ul,
            0x5CC0000000000000ul, 0x5D70000000000000ul, 0x5FA0000000000000ul, 0x5E10000000000000ul,
            0x5780000000000000ul, 0x5630000000000000ul, 0x54E0000000000000ul, 0x5550000000000000ul,
            0x5140000000000000ul, 0x50F0000000000000ul, 0x5220000000000000ul, 0x5390000000000000ul,
            0x4100000000000000ul, 0x40B0000000000000ul, 0x4260000000000000ul, 0x43D0000000000000ul,
            0x47C0000000000000ul, 0x4670000000000000ul, 0x44A0000000000000ul, 0x4510000000000000ul,
            0x4C80000000000000ul, 0x4D30000000000000ul, 0x4FE0000000000000ul, 0x4E50000000000000ul,
            0x4A40000000000000ul, 0x4BF0000000000000ul, 0x4920000000000000ul, 0x4890000000000000ul,
            0xD800000000000000ul, 0xD9B0000000000000ul, 0xDB60000000000000ul, 0xDAD0000000000000ul,
            0xDEC0000000000000ul, 0xDF70000000000000ul, 0xDDA0000000000000ul, 0xDC10000000000000ul,
            0xD580000000000000ul, 0xD430000000000000ul, 0xD6E0000000000000ul, 0xD750000000000000ul,
            0xD340000000000000ul, 0xD2F0000000000000ul, 0xD020000000000000ul, 0xD190000000000000ul,
            0xC300000000000000ul, 0xC2B0000000000000ul, 0xC060000000000000ul, 0xC1D0000000000000ul,
            0xC5C0000000000000ul, 0xC470000000000000ul, 0xC6A0000000000000ul, 0xC710000000000000ul,
            0xCE80000000000000ul, 0xCF30000000000000ul, 0xCDE0000000000000ul, 0xCC50000000000000ul,
            0xC840000000000000ul, 0xC9F0000000000000ul, 0xCB20000000000000ul, 0xCA90000000000000ul,
            0xEE00000000000000ul, 0xEFB0000000000000ul, 0xED60000000000000ul, 0xECD0000000000000ul,
            0xE8C0000000000000ul, 0xE970000000000000ul, 0xEBA0000000000000ul, 0xEA10000000000000ul,
            0xE380000000000000ul, 0xE230000000000000ul, 0xE0E0000000000000ul, 0xE150000000000000ul,
            0xE540000000000000ul, 0xE4F0000000000000ul, 0xE620000000000000ul, 0xE790000000000000ul,
            0xF500000000000000ul, 0xF4B0000000000000ul, 0xF660000000000000ul, 0xF7D0000000000000ul,
            0xF3C0000000000000ul, 0xF270000000000000ul, 0xF0A0000000000000ul, 0xF110000000000000ul,
            0xF880000000000000ul, 0xF930000000000000ul, 0xFBE0000000000000ul, 0xFA50000000000000ul,
            0xFE40000000000000ul, 0xFFF0000000000000ul, 0xFD20000000000000ul, 0xFC90000000000000ul,
            0xB400000000000000ul, 0xB5B0000000000000ul, 0xB760000000000000ul, 0xB6D0000000000000ul,
            0xB2C0000000000000ul, 0xB370000000000000ul, 0xB1A0000000000000ul, 0xB010000000000000ul,
            0xB980000000000000ul, 0xB830000000000000ul, 0xBAE0000000000000ul, 0xBB50000000000000ul,
            0xBF40000000000000ul, 0xBEF0000000000000ul, 0xBC20000000000000ul, 0xBD90000000000000ul,
            0xAF00000000000000ul, 0xAEB0000000000000ul, 0xAC60000000000000ul, 0xADD0000000000000ul,
            0xA9C0000000000000ul, 0xA870000000000000ul, 0xAAA0000000000000ul, 0xAB10000000000000ul,
            0xA280000000000000ul, 0xA330000000000000ul, 0xA1E0000000000000ul, 0xA050000000000000ul,
            0xA440000000000000ul, 0xA5F0000000000000ul, 0xA720000000000000ul, 0xA690000000000000ul,
            0x8200000000000000ul, 0x83B0000000000000ul, 0x8160000000000000ul, 0x80D0000000000000ul,
            0x84C0000000000000ul, 0x8570000000000000ul, 0x87A0000000000000ul, 0x8610000000000000ul,
            0x8F80000000000000ul, 0x8E30000000000000ul, 0x8CE0000000000000ul, 0x8D50000000000000ul,
            0x8940000000000000ul, 0x88F0000000000000ul, 0x8A20000000000000ul, 0x8B90000000000000ul,
            0x9900000000000000ul, 0x98B0000000000000ul, 0x9A60000000000000ul, 0x9BD0000000000000ul,
            0x9FC0000000000000ul, 0x9E70000000000000ul, 0x9CA0000000000000ul, 0x9D10000000000000ul,
            0x9480000000000000ul, 0x9530000000000000ul, 0x97E0000000000000ul, 0x9650000000000000ul,
            0x9240000000000000ul, 0x93F0000000000000ul, 0x9120000000000000ul, 0x9090000000000000ul];
        public static ulong Compute(string value) {
            var hash = 0ul;
            if (value != null)
                for (var i = 0; i < value.Length; i++) hash = Table[(byte)hash ^ (byte)value[i]] ^ (hash >> 8);
            return hash;
        }
        public static ulong Compute(byte[] buffer, int offset, int count) => Hash(buffer, offset, count, 0ul);
        public static ulong Hash(byte[] buffer, int offset, int count, ulong hash) {
            for (var i = offset; i < offset + count; i++) hash = Table[(byte)hash ^ buffer[i]] ^ (hash >> 8);
            return hash;
        }
    }

    static uint HashPath32(string s) => s == null || s.Length == 0 ? 0xFFFFFFFFu : CRC32.Compute(s.ToLowerInvariant());
    
    static ulong HashPath64(string s) => s == null || s.Length == 0 ? 0xFFFFFFFFFFFFFFFFul : CRC64.Compute(s.ToLowerInvariant());

    public static Dictionary<ulong, string> HashFilelist32(ZipArchiveEntry entry) {
        var hashes = new Dictionary<ulong, string>();
        using var r = new StreamReader(entry.Open());
        while (true) {
            var line = r.ReadLine();
            if (line == null) break;
            else if (line.StartsWith(";") || (line = line.Trim()).Length <= 0) continue;
            var source = line;
            var hash = HashPath32(source);
            if (hashes.TryGetValue(hash, out var otherSource) && otherSource != source) throw new InvalidOperationException($"hash collision ('{source}' vs '{otherSource}')");
            hashes[hash] = source.Replace('\\', '/');
        }
        return hashes;
    }

    public static Dictionary<ulong, string> HashFilelist64(ZipArchiveEntry entry) {
        var hashes = new Dictionary<ulong, string>();
        using var r = new StreamReader(entry.Open());
        while (true) {
            var line = r.ReadLine();
            if (line == null) break;
            else if (line.StartsWith(";") || (line = line.Trim()).Length <= 0) continue;
            var source = line;
            var hash = HashPath64(line);
            if (hashes.TryGetValue(hash, out var otherSource) && otherSource != source) throw new InvalidOperationException($"hash collision ('{source}' vs '{otherSource}')");
            hashes[hash] = source.Replace('\\', '/');
        }
        return hashes;
    }
}