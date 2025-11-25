using GameX.Formats;
using GameX.Xbox.Formats.Xna;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Xbox.Formats;

#region Binary_Xnb

public class Binary_Xnb : IHaveMetaInfo, IWriteToStream, IRedirected<object> {
    public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Xnb(r));
    object IRedirected<object>.Value => Obj;

    #region Headers

    const uint MAGIC = 0x00424e58; // XNB?

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header {
        public static (string, int) Struct = ("<I2bI", 10);
        public uint Magic;
        public byte Version;
        public byte Flags;
        public uint SizeOnDisk;
        public readonly bool Compressed => (Flags & 0x80) != 0;
        public readonly char Platform => (char)(Magic >> 24);

        public ContentReader Validate(BinaryReader r) {
            if ((Magic & 0x00FFFFFF) != MAGIC) throw new Exception("BAD MAGIC");
            if (Version != 5 && Version != 4) throw new Exception("Invalid XNB version");
            if (SizeOnDisk > r.BaseStream.Length) throw new Exception("XNB file has been truncated.");
            if (Compressed) {
                uint decompressedSize = r.ReadUInt32(), compressedSize = SizeOnDisk - (uint)r.Tell();
                var b = r.DecompressXmem((int)compressedSize, (int)decompressedSize);
                return new ContentReader(new MemoryStream(b), "name", Version, null);
            }
            return new ContentReader(r.BaseStream, "name", Version, null);
        }
    }

    #endregion

    public object Obj;
    public bool AtEnd;

    public Binary_Xnb(BinaryReader r2) {
        var h = r2.ReadS<Header>();
        var r = h.Validate(r2);
        Obj = r.ReadAsset<object>();
        AtEnd = r.AtEnd();
        //r.EnsureAtEnd(); // h.SizeOnDisk
    }

    //public static TypeReader AddX(TypeReader reader) => TypeManager.Add(reader);

    public void WriteToStream(Stream stream) => Obj.Serialize(stream);

    public override string ToString() => Obj.Serialize();

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => (Obj as IHaveMetaInfo)?.GetInfoNodes(resource, file, tag) ?? [
        new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
        new("Xnb", items: [
            new($"Obj: {Obj}"),
            new($"AtEnd: {AtEnd}")
        ])];
}

#endregion

#region Binary_XXX

public unsafe class Binary_XXX : PakBinary<Binary_XXX> {
    public override Task Read(BinaryPakFile source, BinaryReader r, object tag) {
        var files = source.Files = [];
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, object option = default) {
        throw new NotImplementedException();
    }
}

#endregion
