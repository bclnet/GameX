using GameX.Uncore.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameX.Crytek.Formats.Dunia;

#region Binary_AIWorkspace

public class Binary_AIWorkspace : IHaveMetaInfo {
    public class UnknownData0 {
        public uint TypeHash;
        public Binary_Xml Xml;
    }

    public class UnknownData3 {
        public uint NameHash;
        public string Name;
        public uint IndexIntoUnknown0;
        public uint IndexIntoUnknown1;
        public uint Unknown4;
    }

    public UnknownData0[] Unknown0;
    public byte[] Unknown1;
    public uint[] VariableNameHashes;
    public UnknownData3[] Unknown3;
    public Binary_Xml Xml;

    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_AIWorkspace(r));

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = this }),
    ];

    public Binary_AIWorkspace(BinaryReader r) {
        var version = r.ReadUInt32();
        if (version < 1 || version > 4) throw new FormatException();
        uint unknownLength = r.ReadUInt32(), rmlLength = r.ReadUInt32();
        if (r.BaseStream.Position + unknownLength + rmlLength > r.BaseStream.Length) throw new FormatException();

        using (var r2 = r.ReadBytesToReader((int)unknownLength)) {
            Unknown0 = r2.ReadL32FArray(z => {
                var id = r2.ReadUInt32();
                using var r2a = r2.ReadL32BytesToReader();
                return new UnknownData0 { TypeHash = id, Xml = new Binary_Xml(r2a) };
            });
            Unknown1 = r2.ReadL32Bytes();
            VariableNameHashes = r2.ReadL32PArray<uint>("u");
            Unknown3 = r2.ReadL32FArray(z => new UnknownData3 {
                NameHash = r2.ReadUInt32(),
                Name = r2.ReadL32UString(),
                IndexIntoUnknown0 = r2.ReadUInt32(),
                IndexIntoUnknown1 = r2.ReadUInt32(),
                Unknown4 = r2.ReadUInt32()
            });
        }

        using var r3 = r.ReadBytesToReader((int)rmlLength);
        Xml = new Binary_Xml(r3);
        if (!r3.AtEnd()) throw new FormatException();

        var test_u2 = Unknown3.Max(u => u.IndexIntoUnknown0);
        var test_u3 = Unknown3.Max(u => u.IndexIntoUnknown1);
        var test_u4 = Unknown3.Max(u => u.Unknown4);
    }
}

#endregion

#region Binary_Fcb

public class Binary_Fcb : IHaveMetaInfo {
    public class Object {
        public uint TypeHash;
        public Dictionary<uint, byte[]> Values = [];
        public List<Object> Children = [];

        public static Object Deserialize(BinaryReader r, List<Object> pointers) {
            var (v, o) = r.ReadUIntV8a2();
            if (o) return pointers[(int)v];
            var child = new Object();
            pointers.Add(child);
            child.Deserialize(r, v, pointers);
            return child;
        }

        void Deserialize(BinaryReader r, uint childCount, List<Object> pointers) {
            TypeHash = r.ReadUInt32();
            var (count, c) = r.ReadUIntV8a2();
            if (c) throw new NotImplementedException();
            long position; byte[] value;
            for (var i = 0; i < count; i++) {
                var nameHash = r.ReadUInt32();
                position = r.Tell();
                var (v, o) = r.ReadUIntV8a2();
                if (o) {
                    r.Seek(position - v);
                    (v, o) = r.ReadUIntV8a2();
                    if (o) throw new FormatException();
                    value = r.ReadBytes(v);
                    r.Seek(position);
                    r.ReadUIntV8a2();
                }
                else value = r.ReadBytes(v);
                Values.Add(nameHash, value);
            }
            for (var i = 0; i < childCount; i++) Children.Add(Deserialize(r, pointers));
        }
    }

    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Fcb(r, s));

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = this }),
    ];

    public ushort Flags;
    public Object Root;

    public Binary_Fcb(BinaryReader r, Archive s) {
        var magic = r.ReadUInt32();
        if (magic != 0x4643626E) throw new FormatException("BAD MAGIC"); // FCbn
        var version = r.ReadUInt16();
        if (version != 2) throw new FormatException();
        Flags = r.ReadUInt16();
        if (Flags != 0) throw new FormatException();

        // get hashes
        //var filelist = Path.ChangeExtension(source.BinPath, ".filelist").Replace('\\', '/');
        var hashes = s.Game.Id switch {
            "FarCry2" => FarCry2.GetObjHashes("binary_classes.xml"),
            "FarCry3" or "FarCry3:BD" or "FarCry4" => FarCry2.GetObjHashes("binary_classes.xml"),
            "FarCry5" => FarCry2.GetObjHashes("binary_classes.xml"),
            "FarCry6" => FarCry2.GetObjHashes("binary_classes.xml"),
            "FarCryND" => FarCry2.GetObjHashes("binary_classes.xml"),
            "FarCryP" => FarCry2.GetObjHashes("binary_classes.xml"),
            _ => throw new NotImplementedException($"{s.Game.Id}")
        };

        // read
        var totalObjectCount = r.ReadUInt32();
        var totalValueCount = r.ReadUInt32();
        Root = Object.Deserialize(r, []);
    }

    //public void WriteToStream(Stream stream) => this.Serialize(stream);
    //public override string ToString() => this.Serialize();
}

#endregion

#region Binary_Geometry

//public class Binary_Geometry {
//    public ushort MajorVersion;
//    public ushort MinorVersion;
//    public uint Unknown08;
//    public Geometry.Root Root;

//public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Geometry(r));

//List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
//    new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = this }),
//];

//    public void Binary_Geometry(BinaryReader r) {
//        if (r.BaseStream.Position + 32 > r.BaseStream.Length) throw new FormatException();
//        if (r.ReadUInt32() != 0x4D455348) throw new FormatException();
//        MajorVersion = r.ReadUInt16();
//        if (MajorVersion != 42) throw new FormatException();
//        MinorVersion = r.ReadUInt16();
//        Unknown08 = r.ReadUInt32();
//        Root = (Geometry.Root)DeserializeBlock(r, null, this);
//    }

//    static Geometry.IBlock DeserializeBlock(BinaryReader r, Geometry.IBlock parent, Geometry.IBlockFactory factory) {
//        var baseOffset = r.Tell();
//        var type = (Geometry.BlockType)r.ReadUInt32();
//        var block = factory.CreateBlock(type);
//        if (block == null || block.Type != type) throw new FormatException();
//        var unknown04 = r.ReadUInt32();
//        var size = r.ReadUInt32();
//        var dataSize = r.ReadUInt32();
//        var childCount = r.ReadUInt32();
//        if (dataSize > size) throw new FormatException();
//        var childOffset = r.Tell();
//        var childEnd = childOffset + (size - dataSize - 20);
//        var blockOffset = childEnd;
//        var blockEnd = blockOffset + dataSize;
//        if (blockEnd != baseOffset + size) throw new FormatException();
//        r.Seek(blockOffset);
//        block.Deserialize(parent, r);
//        if (!r.AtEnd(blockEnd)) throw new FormatException();
//        r.Seek(childOffset);
//        for (var i = 0U; i < childCount; i++) {
//            block.AddChild(DeserializeBlock(block, block, r));
//        }
//        if (!r.AtEnd(childEnd)) throw new FormatException();
//        r.Seek(blockEnd);
//        return block;
//    }
//}

#endregion

#region Binary_Xbt

public class Binary_Xbt(BinaryReader r, FileSource f) : Binary_Dds(Pre(r), f) {
    public static new Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Xbt(r, f));

    static BinaryReader Pre(BinaryReader r) {
        var magic = r.ReadUInt32() << 8;
        if (magic != 0x58425400) throw new FormatException("BAD MAGIC");
        r.Seek(r.Skip(4).ReadUInt32());
        return r;
    }
}

#endregion

#region Binary_Xml

public class Binary_Xml : IHaveMetaInfo {
    public class Node {
        public string Name;
        public string Value;
        internal uint NameIndex;
        internal uint ValueIndex;
        public List<Attribute> Attributes = [];
        public List<Node> Children = [];

        public Node(BinaryReader r, ref uint totalNodeCount, ref uint totalAttributeCount) {
            NameIndex = r.ReadUIntV8a(); ValueIndex = r.ReadUIntV8a();
            uint attributeCount = r.ReadUIntV8a(), childCount = r.ReadUIntV8a();
            totalNodeCount += childCount;
            totalAttributeCount += attributeCount;
            for (var i = 0U; i < attributeCount; i++) Attributes.Add(new Attribute(r));
            for (var i = 0U; i < childCount; i++) Children.Add(new Node(r, ref totalNodeCount, ref totalAttributeCount));
        }

        internal void ReadStringTable(StringTable stringTable) {
            Name = stringTable.Read(NameIndex);
            Value = stringTable.Read(ValueIndex);
            foreach (var attribute in Attributes) attribute.ReadStringTable(stringTable);
            foreach (var child in Children) child.ReadStringTable(stringTable);
        }
    }

    public class Attribute {
        public uint Unknown;
        public string Name;
        public string Value;
        internal uint NameIndex;
        internal uint ValueIndex;

        public Attribute(BinaryReader r) {
            Unknown = r.ReadUIntV8a();
            if (Unknown != 0) throw new FormatException();
            NameIndex = r.ReadUIntV8a();
            ValueIndex = r.ReadUIntV8a();
        }

        internal void ReadStringTable(StringTable stringTable) {
            Name = stringTable.Read(NameIndex);
            Value = stringTable.Read(ValueIndex);
        }

    }

    internal class StringTable {
        readonly MemoryStream Data = new();
        readonly Dictionary<uint, string> Offsets = [];
        readonly Dictionary<string, uint> Values = [];

        public StringTable(byte[] buffer) {
            Data = new MemoryStream(buffer);
            while (Data.Position < Data.Length) {
                var offset = (uint)Data.Position;
                var value = "XX"; // Data.ReadStringZ(Encoding.UTF8);
                Offsets.Add(offset, value);
                Values.Add(value, offset);
            }
        }

        public string Read(uint index) {
            if (!Offsets.ContainsKey(index)) throw new KeyNotFoundException();
            return Offsets[index];
        }
    }

    public byte Unknown1;
    public Node Root;

    public static Task<object> Factory(BinaryReader r, FileSource m, Archive s) => Task.FromResult((object)new Binary_Xml(r));

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = this }),
    ];

    public Binary_Xml(BinaryReader r) {
        if (r.ReadByte() != 0) throw new FormatException("not an xml resource file");
        Unknown1 = r.ReadByte();
        var stringTableSize = r.ReadUIntV8a();
        var totalNodeCount = r.ReadUIntV8a();
        var totalAttributeCount = r.ReadUIntV8a();
        uint actualNodeCount = 1U, actualAttributeCount = 0U;
        Root = new Node(r, ref actualNodeCount, ref actualAttributeCount);
        if (actualNodeCount != totalNodeCount || actualAttributeCount != totalAttributeCount) throw new FormatException();
        var stringTableData = new byte[stringTableSize];
        r.Read(stringTableData, 0, stringTableData.Length);
        Root.ReadStringTable(new StringTable(stringTableData));
    }
}

#endregion
