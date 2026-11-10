using Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using FF_FORMAT = GameX.IW.Formats.Binary_IW.FF_FORMAT;
using FF_Header = GameX.IW.Formats.Binary_IW.FF_Header;
using FF_MAGIC = GameX.IW.Formats.Binary_IW.FF_MAGIC;
using FF_VERSION = GameX.IW.Formats.Binary_IW.FF_VERSION;

namespace GameX.IW.Formats {
    public unsafe static partial class FastFile {

        internal class FileHeader {
            public int Id;
            public string Path;
            public long Position;
            public long FileSize;
            public byte[] Data;
        }

        // Titles that are not one of these stay empty. GetZoneFile is in FastFileUnused.cs and stays commented: it writes a sibling .ff~.
        internal static (byte[] zone, List<FileHeader> files)? ListFastFile(BinaryReader r, FF_Header header) {
            if (header.Magic == FF_MAGIC.IWff && header.Format == FF_FORMAT.U100 && (header.Version == FF_VERSION.CO4_WWII || header.Version == FF_VERSION.WaW || header.Version == FF_VERSION.BO)) return List32(r, header);
            if (header.Magic == FF_MAGIC.IWff && (header.Version == FF_VERSION.MW2 || header.Version == FF_VERSION.MW3 || (header.Format == FF_FORMAT.U100 && header.Version == FF_VERSION.Ghosts))) return List64(r, header);
            if (header.Magic == FF_MAGIC.TAff && header.Format == FF_FORMAT._100 && header.Version == FF_VERSION.BO2) return ListBo(r, false);
            if (header.Magic == FF_MAGIC.TAff && header.Format == FF_FORMAT._000 && header.Version == FF_VERSION.BO3) return ListBo(r, true);
            return null;
        }

        static string Iw3Name(int type) => type >= 0 && type < (int)IW3XAssetType.max ? ((IW3XAssetType)type).ToString() : $"type{type}";
        static string Iw4Name(int type) => type >= 0 && type < (int)IW4XAssetType.max ? ((IW4XAssetType)type).ToString() : $"type{type}";
        static string Iw5Name(int type) => type >= 0 && type < (int)IW5XAssetType.max ? ((IW5XAssetType)type).ToString() : $"type{type}";
        static string OthName(int type) => type >= 0 && type < (int)Oth_AssetType.Max && Enum.IsDefined(typeof(Oth_AssetType), type) ? ((Oth_AssetType)type).ToString() : $"type{type}";

        static byte[] InflateZlib(Stream stream) {
            using (var deflate = new DeflateStream(stream, CompressionMode.Decompress, true))
            using (var ms = new MemoryStream()) { deflate.CopyTo(ms); return ms.ToArray(); }
        }

        static byte[] InflateAt(BinaryReader r, int offset) {
            r.BaseStream.Position = offset;
            var cmf = r.ReadByte();
            var flg = r.ReadByte();
            if (cmf != 0x78 || (flg != 0x01 && flg != 0x5E && flg != 0x9C && flg != 0xDA)) throw new FormatException($"Fastfile is not zlib ({cmf:X2}{flg:X2})");
            return InflateZlib(r.BaseStream);
        }

        // Signed IW4/IW5 PC: IWffs100 at 0x15, then the zlib stream at 0x4015.
        // Data is 8KB chunks, 256 of them, then one hash chunk that is not part of the stream.
        static byte[] InflateAuthed(BinaryReader r) {
            r.BaseStream.Position = 0;
            var file = r.ReadBytes((int)r.BaseStream.Length);
            if (file.Length < 0x4017 || file[0x4015] != 0x78) throw new FormatException("Signed fastfile has no zlib stream");
            using (var packed = new MemoryStream()) {
                var pos = 0x4015;
                var dataChunks = 0;
                while (pos < file.Length) {
                    if (dataChunks == 256) { pos += 0x2000; dataChunks = 0; continue; }
                    var n = Math.Min(0x2000, file.Length - pos);
                    packed.Write(file, pos, n);
                    pos += n;
                    dataChunks++;
                }
                packed.Position = 2;
                return InflateZlib(packed);
            }
        }

        // COD4, WaW, and Black Ops. Zlib at byte 12. COD4 is script strings, then (type, pointer).
        // WaW and Black Ops are (pointer, type) at zone 0x30. A type outside the enum lists nothing.
        static (byte[] zone, List<FileHeader> files) List32(BinaryReader r, FF_Header header) {
            if (header.Version == FF_VERSION.CO4_WWII) {
                var cmf = r.ReadByte();
                var flg = r.ReadByte();
                if (cmf != 0x78 || (flg != 0x01 && flg != 0x5E && flg != 0x9C && flg != 0xDA)) throw new FormatException($"COD4 fastfile is not zlib ({cmf:X2}{flg:X2})");
                byte[] zone;
                using (var deflate = new DeflateStream(r.BaseStream, CompressionMode.Decompress, true))
                using (var ms = new MemoryStream()) { deflate.CopyTo(ms); zone = ms.ToArray(); }
                if (zone.Length < 0x3C) throw new FormatException($"COD4 zone is {zone.Length} bytes");
                var zr = new BinaryReader(new MemoryStream(zone));
                zr.Seek(0x2C);
                var stringCount = zr.ReadInt32();
                var stringPtr = zr.ReadInt32();
                var assetCount = zr.ReadInt32();
                var assetPtr = zr.ReadInt32();
                if (stringCount < 0 || assetCount < 0 || stringCount > 1_000_000 || assetCount > 1_000_000) throw new FormatException($"COD4 index counts {stringCount}/{assetCount}");
                var files = new List<FileHeader>(assetCount + 1);
                if (stringCount > 0) {
                    if (stringPtr != -1) throw new FormatException($"COD4 script strings are not inline ({stringPtr:X8})");
                    var ptrs = new int[stringCount];
                    for (var i = 0; i < stringCount; i++) ptrs[i] = zr.ReadInt32();
                    var text = new System.Text.StringBuilder();
                    for (var i = 0; i < stringCount; i++) {
                        var ptr = ptrs[i];
                        if (ptr == -1) {
                            var name = zr.ReadVAString();
                            if (name.Length > 0) text.Append(i).Append('\t').Append(name).Append('\n');
                        }
                        else if (ptr != 0) throw new FormatException($"COD4 script string pointer {ptr:X8}");
                    }
                    if (text.Length > 0) {
                        var data = System.Text.Encoding.ASCII.GetBytes(text.ToString());
                        files.Add(new FileHeader { Id = -1, Path = "scriptstrings.txt", FileSize = data.Length, Data = data });
                    }
                }
                if (assetCount > 0) {
                    if (assetPtr != -1) throw new FormatException($"COD4 asset list is not inline ({assetPtr:X8})");
                    for (var i = 0; i < assetCount; i++) {
                        var pos = zr.BaseStream.Position;
                        var type = zr.ReadInt32();
                        zr.ReadInt32();
                        files.Add(new FileHeader { Id = i, Path = $"{Iw3Name(type)}/{i:D4}", Position = pos, FileSize = 8 });
                    }
                }
                return (zone, files);
            }
            Func<int, string> nameOf;
            int maxType;
            if (header.Version == FF_VERSION.WaW) { nameOf = Iw3Name; maxType = (int)IW3XAssetType.max; }
            else { nameOf = OthName; maxType = (int)Oth_AssetType.Max; }
            var zone2 = InflateAt(r, 12);
            if (zone2.Length < 0x30) return (zone2, new List<FileHeader>());
            var count = BitConverter.ToInt32(zone2, 0x2C);
            if (count <= 0 || count > 1_000_000 || 0x30 + (long)count * 8 > zone2.Length) return (zone2, new List<FileHeader>());
            var rows = new List<FileHeader>(count);
            for (var i = 0; i < count; i++) {
                var pos = 0x30 + i * 8;
                var type = BitConverter.ToInt32(zone2, pos + 4);
                // code_post stores asset bodies in this array, so the "type" is not an enum value.
                if ((uint)type >= (uint)maxType) return (zone2, new List<FileHeader>());
                rows.Add(new FileHeader { Id = i, Path = $"{nameOf(type)}/{i:D4}", Position = pos, FileSize = 8 });
            }
            return (zone2, rows);
        }

        // Ghosts nil pointer. The rawfile record is the same 24 bytes as MW2, with this sentinel instead of -1.
        static readonly long GhostsNil = unchecked((long)0xFDFDFDFFFFFFFFFFUL);

        // MW2 and MW3 share the 16-byte index. When every type is rawfile, the 24-byte record follows.
        // Ghosts uses that record with GhostsNil and scans for runs instead of trusting a count.
        static (byte[] zone, List<FileHeader> files) List64(BinaryReader r, FF_Header header) {
            if (header.Version == FF_VERSION.Ghosts) {
                var ghosts = InflateAt(r, 0x2C);
                return (ghosts, ScanRaw(ghosts, GhostsNil));
            }
            int countAt, recAt, rawType, maxType;
            Func<int, string> nameOf;
            if (header.Version == FF_VERSION.MW2) {
                countAt = 0x38; recAt = 0x40; rawType = (int)IW4XAssetType.rawfile; maxType = (int)IW4XAssetType.max; nameOf = Iw4Name;
            }
            else {
                countAt = 0x3C; recAt = 0x44; rawType = (int)IW5XAssetType.rawfile; maxType = (int)IW5XAssetType.max; nameOf = Iw5Name;
            }
            byte[] zone;
            if (header.Format == FF_FORMAT.U100) zone = InflateAt(r, 0x15);
            else if (header.Format == FF_FORMAT._100) zone = InflateAuthed(r);
            else return (null, new List<FileHeader>());
            if (zone.Length < countAt + 4) return (zone, new List<FileHeader>());
            var count = BitConverter.ToInt32(zone, countAt);
            if (count <= 0 || count > 1_000_000 || recAt + (long)count * 16 > zone.Length) return (zone, new List<FileHeader>());
            var files = new List<FileHeader>(count);
            var allRaw = true;
            for (var i = 0; i < count; i++) {
                var pos = recAt + i * 16;
                var type = BitConverter.ToInt64(zone, pos + 8);
                if (type < 0 || type >= maxType) return (zone, new List<FileHeader>());
                if (type != rawType) allRaw = false;
                files.Add(new FileHeader { Id = i, Path = $"{nameOf((int)type)}/{i:D4}", Position = pos, FileSize = 16 });
            }
            if (allRaw) {
                var body = recAt + count * 16;
                if (body + 8 <= zone.Length && BitConverter.ToInt64(zone, body) == -1) body += 8;
                if (TryWalkAll(zone, body, count, -1, out var named)) return (zone, named);
            }
            return (zone, files);
        }

        static bool TryRaw(byte[] z, int pos, long nil, out string name, out byte[] data, out int next) {
            name = null; data = null; next = pos;
            if (pos < 0 || pos + 24 > z.Length) return false;
            var p1 = BitConverter.ToInt64(z, pos);
            var cLen = BitConverter.ToInt32(z, pos + 8);
            var len = BitConverter.ToInt32(z, pos + 12);
            var p2 = BitConverter.ToInt64(z, pos + 16);
            if (p1 != nil || p2 != nil || cLen < 0 || len < 0 || cLen > z.Length - pos) return false;
            var s = pos + 24;
            var e = s;
            while (e < z.Length && z[e] != 0) {
                if (z[e] < 32 || z[e] > 126) return false;
                e++;
                if (e - s > 120) return false;
            }
            if (e >= z.Length || e == s) return false;
            name = System.Text.Encoding.ASCII.GetString(z, s, e - s);
            var p = e + 1;
            if ((p & 1) != 0 && p < z.Length && z[p] == 0) p++;
            else if (cLen == 0 && p + 1 < z.Length && z[p] == 0 && z[p + 1] == 0xFF) p++;
            if (p > z.Length || cLen > z.Length - p) return false;
            if (cLen > 0 && z[p] != 0x78) return false;
            if (cLen == 0) data = Array.Empty<byte>();
            else {
                try {
                    using (var ms = new MemoryStream(z, p + 2, cLen - 2, false)) data = InflateZlib(ms);
                }
                catch (InvalidDataException) { return false; }
            }
            next = p + cLen;
            return data != null;
        }

        static bool TryWalkAll(byte[] z, int pos, int count, long nil, out List<FileHeader> files) {
            files = new List<FileHeader>(count);
            for (var i = 0; i < count; i++) {
                if (!TryRaw(z, pos, nil, out var name, out var data, out var next) || next < pos) return false;
                files.Add(new FileHeader { Id = i, Path = name, FileSize = data.Length, Data = data });
                pos = next;
            }
            return true;
        }

        static List<FileHeader> ScanRaw(byte[] z, long nil) {
            var files = new List<FileHeader>();
            for (var pos = 0x40; pos + 24 < z.Length; pos++) {
                if (BitConverter.ToInt64(z, pos) != nil) continue;
                if (!TryRaw(z, pos, nil, out var name, out var data, out var next)) continue;
                var run = new List<FileHeader>();
                var p = pos;
                var guard = 0;
                while (guard++ < 100_000 && TryRaw(z, p, nil, out name, out data, out next) && next >= p) {
                    run.Add(new FileHeader { Id = files.Count + run.Count, Path = name, FileSize = data.Length, Data = data });
                    if (next == p) break;
                    p = next;
                }
                if (run.Count >= 2 || (run.Count == 1 && p >= z.Length - 16)) {
                    files.AddRange(run);
                    if (p > pos) pos = p - 1;
                }
            }
            return files;
        }

        static readonly byte[] Bo2Key = {
            0x64, 0x1D, 0x8A, 0x2F, 0xE3, 0x1D, 0x3A, 0xA6, 0x36, 0x22, 0xBB, 0xC9, 0xCE, 0x85, 0x87, 0x22,
            0x9D, 0x42, 0xB0, 0xF8, 0xED, 0x9B, 0x92, 0x41, 0x30, 0xBF, 0x88, 0xB6, 0x5E, 0xDC, 0x50, 0xBE,
        };

        static byte[] CreateIvTable(byte[] source) {
            var ivTable = new byte[16000];
            var addDiv = 0;
            var nameKeyLength = Array.FindIndex(source, b => b == 0);
            if (nameKeyLength <= 0) return ivTable;
            for (var i = 0; i < ivTable.Length; i += nameKeyLength * 4)
                for (var x = 0; x < nameKeyLength * 4; x += 4) {
                    if (i + addDiv >= ivTable.Length || i + x >= ivTable.Length) return ivTable;
                    addDiv = x > 0 ? x / 4 : 0;
                    for (var y = 0; y < 4; y++) ivTable[i + x + y] = source[addDiv];
                }
            return ivTable;
        }

        static void UpdateIvTable(int index, byte[] hash, byte[] ivTable, int[] ivCounter) {
            for (var i = 0; i < 20; i += 5) {
                var value = (index + 4 * ivCounter[index]) % 800 * 5;
                for (var x = 0; x < 5; x++) ivTable[4 * value + x + i] ^= hash[i + x];
            }
            ivCounter[index]++;
        }

        static byte[] IvAt(int index, byte[] ivTable, int[] ivCounter) {
            var iv = new byte[8];
            var arrayIndex = (index + 4 * (ivCounter[index] - 1)) % 800 * 20;
            Array.Copy(ivTable, arrayIndex, iv, 0, 8);
            return iv;
        }

        // BO2 is Salsa20 sections, then raw deflate. BO3 is the blocked zlib loop.
        // The shared step is the name harvest. A BO3 flag mismatch lists nothing.
        static (byte[] zone, List<FileHeader> files) ListBo(BinaryReader r, bool bo3) {
            byte[] zone;
            if (!bo3) {
                r.BaseStream.Position = 0x18;
                var ivCount = new[] { 1, 1, 1, 1 };
                var ivTable = CreateIvTable(r.ReadBytes(0x20));
                r.BaseStream.Position += 0x100;
                var salsa = new Salsa20 { Key = Bo2Key };
                using (var zoneMs = new MemoryStream()) {
                    var section = 0;
                    while (r.BaseStream.Position + 4 <= r.BaseStream.Length && section < 10_000) {
                        var size = r.ReadInt32();
                        if (size == 0) break;
                        if (size < 0 || r.BaseStream.Position + size > r.BaseStream.Length) throw new FormatException($"BO2 section {size}");
                        salsa.IV = IvAt(section % 4, ivTable, ivCount);
                        var decrypted = salsa.CreateDecryptor().TransformFinalBlock(r.ReadBytes(size), 0, size);
                        using (var sha1 = SHA1.Create()) UpdateIvTable(section % 4, sha1.ComputeHash(decrypted), ivTable, ivCount);
                        using (var s = new MemoryStream(decrypted))
                        using (var deflate = new DeflateStream(s, CompressionMode.Decompress))
                            deflate.CopyTo(zoneMs);
                        section++;
                    }
                    zone = zoneMs.ToArray();
                }
            }
            else {
                // Byte 12 is unknown. zlib, pc, and encrypted follow it. Installed files are 00 01 00 00.
                r.BaseStream.Position = 12;
                var unknown = r.ReadByte();
                var flagsZ = r.ReadByte();
                var flagsPc = r.ReadByte();
                var flagsEncrypted = r.ReadByte();
                if (unknown != 0 || flagsZ != 1 || flagsPc != 0 || flagsEncrypted != 0) return (null, new List<FileHeader>());
                r.BaseStream.Position = 0x90;
                var size = r.ReadInt64();
                if (size <= 0 || size > 512L * 1024 * 1024) throw new FormatException($"BO3 zone size {size}");
                r.BaseStream.Position = 0x248;
                using (var zoneMs = new MemoryStream()) {
                    long consumed = 0;
                    while (consumed < size) {
                        var pos = r.BaseStream.Position;
                        if (pos + 16 > r.BaseStream.Length) break;
                        var compressedSize = r.ReadInt32();
                        var decompressedSize = r.ReadInt32();
                        var blockSize = r.ReadInt32();
                        var position = r.ReadInt32();
                        if (position != pos) throw new FormatException($"BO3 block position {position:X}");
                        if (decompressedSize == 0) {
                            r.BaseStream.Position = (r.BaseStream.Position + 0x7FFFFF) & ~0x7FFFFFL;
                            continue;
                        }
                        if (compressedSize < 2) throw new FormatException($"BO3 block compressed size {compressedSize}");
                        r.BaseStream.Position += 2;
                        var compressed = r.ReadBytes(compressedSize - 2);
                        using (var s = new MemoryStream(compressed))
                        using (var deflate = new DeflateStream(s, CompressionMode.Decompress))
                            deflate.CopyTo(zoneMs);
                        consumed += decompressedSize;
                        r.BaseStream.Position = position + 16 + blockSize;
                    }
                    zone = zoneMs.ToArray();
                }
            }
            return (zone, HarvestNames(zone, bo3));
        }

        static bool IsAssetName(string name) {
            if (name.Length < 3 || name.Length > 180) return false;
            for (var i = 0; i < name.Length; i++) if (name[i] < 32 || name[i] > 126) return false;
            if (name.IndexOf('/') >= 0 || name.IndexOf('\\') >= 0 || name[0] == '$') return true;
            return name.EndsWith(".cfg") || name.EndsWith(".csv") || name.EndsWith(".gsc") || name.EndsWith(".csc")
                || name.EndsWith(".ddl") || name.EndsWith(".txt") || name.EndsWith(".vision") || name.EndsWith(".menu");
        }

        // commaPrefix: BO3 asset names are stored with a leading comma. BO2 names are plain paths.
        static List<FileHeader> HarvestNames(byte[] zone, bool commaPrefix) {
            var files = new List<FileHeader>();
            var seen = new HashSet<string>();
            for (var i = 0; i < zone.Length; i++) {
                var start = i;
                if (commaPrefix) {
                    if (zone[i] != (byte)',' || (i > 0 && zone[i - 1] >= 32)) continue;
                    start = i + 1;
                }
                else if (i > 0 && zone[i - 1] >= 32 && zone[i - 1] < 127) continue;
                var e = start;
                while (e < zone.Length && zone[e] >= 32 && zone[e] < 127) e++;
                if (e >= zone.Length || zone[e] != 0 || e == start) continue;
                var name = System.Text.Encoding.ASCII.GetString(zone, start, e - start);
                if (!IsAssetName(name) || !seen.Add(name)) continue;
                files.Add(new FileHeader { Id = files.Count, Path = name, Position = start, FileSize = e - start });
            }
            return files;
        }
    }
}
