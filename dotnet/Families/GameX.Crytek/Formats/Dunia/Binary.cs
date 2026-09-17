using GameX.Uncore.Formats;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using SharpCompress;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
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

public class Binary_Fcb : IHaveMetaInfo, IWriteToStream {
    public class Object {
        uint Id;
        public uint TypeHash;
        public Dictionary<uint, byte[]> Values = [];
        public List<Object> Children = [];

        public static Object Deserialize(BinaryReader r, List<Object> pointers, Definition defx) {
            var id = r.Tell();
            var (v, o) = r.ReadUIntV8a2();
            if (o) return pointers[(int)v];
            var child = new Object { Id = (uint)id };
            pointers.Add(child);
            child.Deserialize(r, v, pointers, defx);
            return child;
        }

        void Deserialize(BinaryReader r, uint childCount, List<Object> pointers, Definition defx) {
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
            for (var i = 0; i < childCount; i++) Children.Add(Deserialize(r, pointers, defx));
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
        var def = s.Game.Id switch {
            "FarCry3" or "FarCry3:BD" or "FarCry4" => FarCry2.GetObjDef("binary_classes.xml"),
            _ => FarCry2.GetObjDef("binary_classes.xml"),
        };

        // read
        var totalObjectCount = r.ReadUInt32();
        var totalValueCount = r.ReadUInt32();
        Root = Object.Deserialize(r, [], def);
    }

    public void WriteToStream(Stream stream) => this.Serialize(stream);
    public override string ToString() => this.Serialize();
}

#endregion

#region Binary_Xbg

public class Binary_Xbg : Binary_Xbg.IBlockFactory, IHaveMetaInfo {
    #region Blocks

    public enum BlockType : uint {
        Root = 0x00000000,
        MaterialReference = 0x524D544C, // RMTL
        Nodes = 0x4E4F4445,
        O2BM = 0x4F32424D,
        SKID = 0x534B4944,
        SKND = 0x534B4E44,
        CLUS = 0x434C5553,
        LODs = 0x04C4F4453, // LODS
        BoundingBox = 0x42424F58, // BBOX
        BSPH = 0x42535048,
        LODInfo = 0x004C4F44, // LOD\0
        PCMP = 0x50434D50,
        UCMP = 0x55434D50,
        IKDA = 0x494B4441,
        MaterialDescriptor = 0x444D544C, // DMTL
    }

    public interface IBlockFactory {
        Block CreateBlock(BlockType type);
    }

    public abstract class Block(BlockType type) : IBlockFactory {
        public BlockType Type = type;
        public abstract void Deserialize(BinaryReader r, Block parent);
        public virtual Block CreateBlock(BlockType type) => null;
        public virtual void AddChild(Block child) => throw new NotImplementedException();
        public virtual IEnumerable<Block> GetChildren() => throw new NotImplementedException();
    }

    public class BoundingBox() : Block(BlockType.BoundingBox) {
        public Vector3 Min;
        public Vector3 Max;

        public override void Deserialize(BinaryReader r, Block parent) {
            Min = r.ReadVector3();
            Max = r.ReadVector3();
        }
    }

    public class BSPH() : Block(BlockType.BSPH) {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public override void Deserialize(BinaryReader r, Block parent) {
            X = r.ReadSingle();
            Y = r.ReadSingle();
            Z = r.ReadSingle();
            W = r.ReadSingle();
        }
    }

    public class CLUS() : Block(BlockType.CLUS) {
        public class UnknownData0(BinaryReader r) {
            public byte[] Unknown0 = r.ReadBytes(108);
            public ushort Unknown1 = r.ReadUInt16();
        }

        public UnknownData0[][] Unknown0;

        public override void Deserialize(BinaryReader r, Block parent) {
            Unknown0 = r.ReadFArray(z => r.ReadL32FArray(z2 => new UnknownData0(r)), ((SKND)parent).Unknown0.Length);
        }
    }

    public class IKDA() : Block(BlockType.IKDA) {
        public byte[][] Unknown;

        public override void Deserialize(BinaryReader r, Block parent) {
            Unknown = r.ReadL32FArray(z => r.ReadBytes(52));
        }
    }

    public class LODInfo() : Block(BlockType.LODInfo) {
        public uint Count;
        public uint Unknown1;

        public override void Deserialize(BinaryReader r, Block parent) {
            Count = r.ReadUInt32();
            Unknown1 = r.ReadUInt32();
        }
    }

    public class LODs() : Block(BlockType.LODs) {
        public class LevelOfDetail {
            public float Unknown0; // seems to be a distance value for determining which LOD to use
            public Buffer[] Buffers;
            public Primitive[] Primitives;
            public byte[] VertexData;
            public short[] Indices;
            public LevelOfDetail(BinaryReader r) {
                Unknown0 = r.ReadSingle();
                Buffers = r.ReadL32FArray(z => new Buffer(r));
                Primitives = r.ReadL32FArray(z => new Primitive(r));
                var vertexDataSize = r.ReadUInt32(); VertexData = r.Align(16).ReadBytes((int)vertexDataSize); // data is aligned to 16 bytes, ugh
                var indexCount = r.ReadUInt32(); Indices = r.Align(16).ReadPArray<short>("h", indexCount); // data is aligned to 16 bytes, ugh
            }
        }

        public class Buffer(BinaryReader r) {
            public uint Format = r.ReadUInt32();
            public uint Size = r.ReadUInt32();
            public uint Count = r.ReadUInt32();
            public uint Offset = r.ReadUInt32();
        }

        public class Primitive(BinaryReader r) {
            public int BufferIndex = r.ReadInt32();
            public int SkeletonIndex = r.ReadInt32();
            public int MaterialIndex = r.ReadInt32();
            public int IndicesStartIndex = r.ReadInt32();
            public uint Unknown4 = r.ReadUInt32();
            public uint Unknown5 = r.ReadUInt32();
            public uint Unknown6 = r.ReadUInt32();
        }

        public LevelOfDetail[] Items;

        public override void Deserialize(BinaryReader r, Block parent) {
            Items = r.ReadL32FArray(z => new LevelOfDetail(r));
        }
    }

    // Refer to common\engine\providerdescriptors.xml
    public class MaterialDescriptor() : Block(BlockType.MaterialDescriptor) {
        public string Name;
        public string Unknown1;
        public string Unknown2;
        public IDictionary<string, string> TextureProperties;
        public IDictionary<string, float> Float1Properties;
        public IDictionary<string, Float2> Float2Properties;
        public IDictionary<string, Float3> Float3Properties;
        public IDictionary<string, Float4> Float4Properties;
        public IDictionary<string, int> IntProperties;
        public IDictionary<string, bool> BoolProperties;

        public override void Deserialize(BinaryReader r, Block parent) {
            Name = r.SkipAfter(r.ReadL32UString(), 1);
            Unknown1 = r.SkipAfter(r.ReadL32UString(), 1);
            Unknown2 = r.SkipAfter(r.ReadL32UString(), 1);
            TextureProperties = r.ReadL32FMany(z => r.SkipAfter(r.ReadL32UString(), 1), z => r.SkipAfter(r.ReadL32UString(), 1));
            Float1Properties = r.ReadL32FMany(z => r.SkipAfter(r.ReadL32UString(), 1), z => r.ReadSingle());
            Float2Properties = r.ReadL32FMany(z => r.SkipAfter(r.ReadL32UString(), 1), z => new Float2(r.ReadSingle(), r.ReadSingle()));
            Float3Properties = r.ReadL32FMany(z => r.SkipAfter(r.ReadL32UString(), 1), z => new Float3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()));
            Float4Properties = r.ReadL32FMany(z => r.SkipAfter(r.ReadL32UString(), 1), z => new Float4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()));
            IntProperties = r.ReadL32FMany(z => r.SkipAfter(r.ReadL32UString(), 1), z => r.ReadInt32());
            BoolProperties = r.ReadL32FMany(z => r.SkipAfter(r.ReadL32UString(), 1), z => r.ReadBoolean32());
        }
    }

    public class MaterialReference() : Block(BlockType.MaterialReference) {
        public uint Unknown00;
        public string[] Paths;

        public override void Deserialize(BinaryReader r, Block parent) {
            var count = r.ReadUInt32();
            Unknown00 = r.ReadUInt32();
            Paths = r.ReadFArray(z => r.SkipAfter(r.ReadL32UString(), 1), count);
        }
    }

    public class Nodes() : Block(BlockType.Nodes) {
        public class Node(BinaryReader r) {
            public uint NameHash = r.ReadUInt32();
            public int NextSiblingIndex = r.ReadInt32();
            public int FirstChildIndex = r.ReadInt32();
            public int PreviousSiblingIndex = r.ReadInt32();
            public float Unknown10 = r.ReadSingle();
            public float Unknown14 = r.ReadSingle();
            public float Unknown18 = r.ReadSingle();
            public float Unknown1C = r.ReadSingle();
            public float Unknown20 = r.ReadSingle();
            public float Unknown24 = r.ReadSingle();
            public float Unknown28 = r.ReadSingle();
            public float Unknown2C = r.ReadSingle();
            public float Unknown30 = r.ReadSingle();
            public float Unknown34 = r.ReadSingle();
            public int O2BMIndex = r.ReadInt32();
            public float Unknown3C = r.ReadSingle();
            public float Unknown40 = r.ReadSingle();
            public string Name = r.SkipAfter(r.ReadL32UString(), 1);
            public override string ToString() => Name ?? base.ToString();
        }

        public Node[] Items;

        public override void Deserialize(BinaryReader r, Block parent) {
            Items = r.ReadL32FArray(z => new Node(r));
        }
    }

    public class O2BM() : Block(BlockType.O2BM) {
        public Matrix4x4[] Items;

        public override void Deserialize(BinaryReader r, Block parent) {
            Items = r.ReadL32FArray(z => r.ReadMatrix4x4());
        }
    }

    public class PCMP() : Block(BlockType.PCMP) {
        public float X;
        public float Y;

        public override void Deserialize(BinaryReader r, Block parent) {
            X = r.ReadSingle();
            Y = r.ReadSingle();
        }
    }

    public class SKID() : Block(BlockType.SKID) {
        public byte[][] Unknown;

        public override void Deserialize(BinaryReader r, Block parent) {
            Unknown = r.ReadL32FArray(z => r.ReadBytes(8));
        }
    }

    public class SKND() : Block(BlockType.SKND) {
        public class UnknownData0(BinaryReader r) {
            public float Unknown00 = r.ReadSingle();
            public float Unknown04 = r.ReadSingle();
            public float Unknown08 = r.ReadSingle();
            public float Unknown0C = r.ReadSingle();
            public float Unknown10 = r.ReadSingle();
            public float Unknown14 = r.ReadSingle();
            public float Unknown18 = r.ReadSingle();
            public float Unknown1C = r.ReadSingle();
            public float Unknown20 = r.ReadSingle();
            public float Unknown24 = r.ReadSingle();
            public float Unknown28 = r.ReadSingle();
            public uint Unknown2C = r.ReadUInt32();
            public uint Unknown30 = r.ReadUInt32();
            public string Name = r.SkipAfter(r.ReadL32UString(), 1);
        }

        public UnknownData0[] Unknown0;
        public List<CLUS> Unknown1 = [];

        public override void Deserialize(BinaryReader r, Block parent) {
            Unknown0 = r.ReadL32FArray(z => new UnknownData0(r));
        }
        public override Block CreateBlock(BlockType type) => type switch {
            BlockType.CLUS => new CLUS(),
            _ => null,
        };
        public override void AddChild(Block child) => Unknown1.Add((CLUS)child);
        public override IEnumerable<Block> GetChildren() => Unknown1;
    }

    public class UCMP() : Block(BlockType.UCMP) {
        public float X;
        public float Y;

        public override void Deserialize(BinaryReader r, Block parent) {
            X = r.ReadSingle();
            Y = r.ReadSingle();
        }
    }

    public class RootX() : Block(BlockType.Root) {
        public MaterialReference MaterialReference;
        public Nodes Nodes;
        public O2BM O2BM;
        public SKID SKID;
        public SKND SKND;
        public LODs LODs;
        public BoundingBox BoundingBox;
        public BSPH BSPH;
        public LODInfo LOD;
        public PCMP PCMP;
        public UCMP UCMP;
        public IKDA IKDA;
        public List<MaterialDescriptor> MaterialDescriptors = [];

        public override void Deserialize(BinaryReader r, Block parent) { }

        public override Block CreateBlock(BlockType type) => type switch {
            BlockType.MaterialReference => new MaterialReference(),
            BlockType.Nodes => new Nodes(),
            BlockType.O2BM => new O2BM(),
            BlockType.SKID => new SKID(),
            BlockType.SKND => new SKND(),
            BlockType.LODs => new LODs(),
            BlockType.BoundingBox => new BoundingBox(),
            BlockType.BSPH => new BSPH(),
            BlockType.LODInfo => new LODInfo(),
            BlockType.PCMP => new PCMP(),
            BlockType.UCMP => new UCMP(),
            BlockType.IKDA => new IKDA(),
            BlockType.MaterialDescriptor => new MaterialDescriptor(),
            _ => throw new NotSupportedException(),
        };

        static void SetChild<TType>(Block child, ref TType value) where TType : Block {
            if (child is TType type) {
                if (value != null) throw new InvalidOperationException();
                value = type;
            }
        }
        static void GetChild(List<Block> blocks, Block value) {
            if (value != null) blocks.Add(value);
        }

        public override void AddChild(Block child) {
            SetChild(child, ref MaterialReference);
            SetChild(child, ref Nodes);
            SetChild(child, ref O2BM);
            SetChild(child, ref SKID);
            SetChild(child, ref SKND);
            SetChild(child, ref LODs);
            SetChild(child, ref BoundingBox);
            SetChild(child, ref BSPH);
            SetChild(child, ref LOD);
            SetChild(child, ref PCMP);
            SetChild(child, ref UCMP);
            SetChild(child, ref IKDA);
            if (child is MaterialDescriptor materialDescriptor) MaterialDescriptors.Add(materialDescriptor);
        }

        public override IEnumerable<Block> GetChildren() {
            var children = new List<Block>();
            GetChild(children, MaterialReference);
            GetChild(children, Nodes);
            GetChild(children, O2BM);
            GetChild(children, SKID);
            GetChild(children, SKND);
            GetChild(children, LODs);
            GetChild(children, BoundingBox);
            GetChild(children, BSPH);
            GetChild(children, LOD);
            GetChild(children, PCMP);
            GetChild(children, UCMP);
            GetChild(children, IKDA);
            children.AddRange(MaterialDescriptors);
            return children;
        }
    }

    #endregion

    public ushort MajorVersion;
    public ushort MinorVersion;
    public uint Unknown08;
    public RootX Root;

    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Xbg(r));

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = this }),
    ];

    public Binary_Xbg(BinaryReader r) {
        if (r.Tell() + 32 > r.BaseStream.Length) throw new FormatException();
        if (r.ReadUInt32() != 0x4D455348) throw new FormatException("BAD MAGIC");
        MajorVersion = r.ReadUInt16();
        if (MajorVersion != 42) throw new FormatException();
        MinorVersion = r.ReadUInt16();
        Unknown08 = r.ReadUInt32();
        Root = (RootX)DeserializeBlock(r, null, this);
    }

    public Block CreateBlock(BlockType type) => type != BlockType.Root ? null : new RootX();

    static Block DeserializeBlock(BinaryReader r, Block parent, IBlockFactory factory) {
        var baseOffset = r.Tell();
        var type = (BlockType)r.ReadUInt32();
        var block = factory.CreateBlock(type);
        if (block == null || block.Type != type) throw new FormatException();
        var unknown04 = r.ReadUInt32();
        var size = r.ReadUInt32();
        var dataSize = r.ReadUInt32();
        var childCount = r.ReadUInt32();
        if (dataSize > size) throw new FormatException();
        var childOffset = r.Tell();
        var childEnd = childOffset + (size - dataSize - 20);
        var blockOffset = childEnd;
        var blockEnd = blockOffset + dataSize;
        if (blockEnd != baseOffset + size) throw new FormatException();
        r.Seek(blockOffset);
        block.Deserialize(r, parent);
        if (!r.AtEnd(blockEnd)) throw new FormatException();
        r.Seek(childOffset);
        for (var i = 0U; i < childCount; i++) block.AddChild(DeserializeBlock(r, block, block));
        if (!r.AtEnd(childEnd)) throw new FormatException();
        r.Seek(blockEnd);
        return block;
    }
}

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

#region Binary_Spk

/// <summary>
/// Binary_Spk
/// </summary>
public class Binary_Spk : ArcBinary<Binary_Spk> {
    #region Scan

    static (bool, long) CheckOggChunk(BinaryReader r) {
        var b = r.ReadBytes(4);
        r.Skip(-4);
        if (Encoding.ASCII.GetString(b) != "OggS") return (false, 0);
        // walk the chain
        var first = true;
        var header = new byte[27];
        var fullSize = 0;
        while (true) {
            r.Read(header, 0, 27);
            if (Encoding.ASCII.GetString(header[..4]) != "OggS") break;
            var headerSize = 27 + header[26];
            var segments = r.ReadBytes(header[26]);
            var pageSize = 0;
            for (var i = 0; i < header[26]; i++) pageSize += segments[i];
            pageSize += headerSize;
            fullSize += pageSize;
            if (first) {
                if ((header[5] & 0x02) != 0) first = false;
                else return (false, 0);// found the middle of the stream
            }
            if ((header[5] & 0x04) != 0) break;
            r.Skip(pageSize - headerSize);
        }
        return (true, fullSize);
    }

    public static (long, long) Scan(BinaryReader r, long endOffset) {
        const int BufLen = 65535;
        if (r.Tell() >= endOffset) return (-1, -1);
        var buf = new byte[BufLen];
        var bytesLeft = (int)(endOffset - r.Tell());
        var startOffset = r.Tell();
        // scan
        bool chunkValid = false, bigEndian = false; var variant = 0; var fullSize = 0L;
        while (r.Tell() < endOffset) {
            //var position = r.Tell();
            var nextRead = bytesLeft > BufLen ? BufLen : bytesLeft;
            if (nextRead == 0) break;
            r.Read(buf, 0, nextRead);
            bytesLeft -= nextRead;
            // scan block
            var offsetReset = r.Tell();
            for (var i = 0; i < nextRead; i++) {
                var chunkStart = startOffset + i;
                r.Seek(chunkStart);
                if (buf[i] == 3 || buf[i] == 5) {
                    var b = r.ReadBytes(28);
                    // check the bytes
                    chunkValid = b[9] == 0 && b[10] == 0 && b[11] == 0 && b[18] < 89 && (b[12] == 0 || b[12] == 1) && b[22] < 89; /*&& b[23]<5*/
                    if (chunkValid && b[0] == 3) {
                        if (b[14] != 0 || b[15] != 10) chunkValid = false;
                    }
                    else if (chunkValid && b[0] == 5) {
                        if (b[14] != 10 || b[15] != 0) chunkValid = false;
                    }
                    // chunk is valid
                    if (chunkValid) { r.Seek(chunkStart); return (chunkStart - startOffset, fullSize); }
                }
                else if (buf[i] == 6) {
                    var b = r.ReadBytes(36);
                    // check the bytes
                    chunkValid = b[9] == 0 && b[10] == 0 && b[11] == 0 && b[18] < 89 && (b[12] == 0 || b[12] == 1) && b[22] < 89; /*&& b[23]<5*/
                    if (chunkValid && b[0] == 6) {
                        if (b[14] != 10 || b[15] != 0) chunkValid = false;
                    }
                    for (var j = 28; j < 36; j++)
                        if (b[j] != 0) { chunkValid = false; break; }
                    // chunk is valid
                    if (chunkValid) { r.Seek(chunkStart); return (chunkStart - startOffset, fullSize); }
                }
                else if (buf[i] == 2) {
                    var b = r.ReadBytes(24);
                    // get information
                    fullSize = BinaryPrimitives.ReadUInt32LittleEndian(b[8..11]);
                    var numberLayers = BinaryPrimitives.ReadUInt32LittleEndian(b[4..7]);
                    // check the bytes
                    chunkValid = b[0] == 2 && b[1] == 0 && b[2] == 0 && b[3] == 0 && b[5] == 0 && b[6] == 0 && b[7] == 0;
                    if (chunkValid && (fullSize < 64 || fullSize > endOffset - chunkStart)) chunkValid = false;
                    // walk the blocks
                    if (chunkValid)
                        while (r.Tell() < chunkStart + fullSize - 2) {
                            var signature = r.SkipAfter(r.ReadUInt32(), 4);
                            var totalBytes = 0U;
                            for (var j = 0; j < numberLayers; j++) totalBytes += r.ReadUInt32();
                            if (signature != i) { chunkValid = false; break; }
                            if (totalBytes >= endOffset - chunkStart || totalBytes < numberLayers * 4 + 8) { chunkValid = false; break; }
                            r.Skip(totalBytes);
                        }
                    // chunk is valid
                    if (chunkValid) { r.Seek(chunkStart); return (chunkStart - startOffset, fullSize); }
                }
                else if (buf[i] == 8) {
                    var b = r.ReadBytes(48);
                    // check the bytes
                    chunkValid = b[0] == 8 && b[1] == 0 && b[2] == 0 && b[3] == 0 && b[37] == 0 && b[38] == 0 && b[39] == 0 && (b[36] == 4 || b[36] == 6) && b[45] == 0 && b[46] == 0 && b[47] == 0 && (b[44] == 1 || b[44] == 2);
                    // walk the blocks
                    if (chunkValid) {
                        var blockHeader = new byte[52];
                        bool done = false, foundABlock = false;
                        while (!r.AtEnd()) {
                            for (var j = 0; j < b[44]; j++) {
                                r.Read(blockHeader, 0, 52);
                                if (blockHeader[0] != 2 || blockHeader[1] != 0 || blockHeader[2] != 0 || blockHeader[3] != 0) { done = true; break; }
                            }
                            r.Skip(b[36] * 384 + 2);
                            if (done) break;
                            foundABlock = true;
                        }
                        if (!foundABlock) chunkValid = false;
                    }
                    // chunk is valid
                    if (chunkValid) { r.Seek(chunkStart); return (chunkStart - startOffset, fullSize); }
                }
                else if (buf[i] == 8 || buf[i] == 7) {
                    var b = r.ReadBytes(28);
                    // check the characters
                    chunkValid = false; bigEndian = false; variant = 0;
                    if ((b[0] == 8 || b[0] == 7) && b[1] == 0 && b[3] == 0 && b[9] == 0 && b[10] == 0 && b[11] == 0) { chunkValid = true; bigEndian = false; variant = b[0] == 7 ? 2 : 0; }
                    else if (chunkStart >= 3) {
                        chunkStart -= 3; r.Seek(chunkStart); // adjust the chuck size and reread
                        b = r.ReadBytes(28);
                        if ((b[3] == 8 || b[3] == 7) && b[2] == 0 && b[0] == 0 && b[8] == 0 && b[9] == 0 && b[10] == 0) { chunkValid = true; bigEndian = true; variant = b[3] == 7 ? 2 : 0; }
                    }
                    // get information
                    var numberLayers = bigEndian ? BinaryPrimitives.ReadUInt32BigEndian(b[8..11]) : BinaryPrimitives.ReadUInt32LittleEndian(b[8..11]);
                    var numberBuffers = bigEndian ? BinaryPrimitives.ReadUInt32BigEndian(b[12..15]) : BinaryPrimitives.ReadUInt32LittleEndian(b[12..15]);
                    var offsetToHeaders = bigEndian ? BinaryPrimitives.ReadUInt32BigEndian(b[16..19]) : BinaryPrimitives.ReadUInt32LittleEndian(b[16..19]);
                    var headerSkip = bigEndian ? BinaryPrimitives.ReadUInt32BigEndian(b[20..23]) : BinaryPrimitives.ReadUInt32LittleEndian(b[20..23]);
                    if (variant == 2) {
                        numberBuffers = offsetToHeaders;
                        r.Skip(32);
                        headerSkip = !r.AtEnd() ? r.ReadUInt32X(bigEndian) : 0;
                    }
                    else if (offsetToHeaders != numberLayers * 4 + 8) {
                        variant = 1;
                        numberBuffers = offsetToHeaders;
                        r.Skip(44);
                        headerSkip = !r.AtEnd() ? r.ReadUInt32X(bigEndian) : 0;
                    }
                    // verify the information
                    if (headerSkip >= endOffset - chunkStart || headerSkip < numberLayers * 4 || numberLayers == 0 || numberBuffers < 1) chunkValid = false;
                    // walk the blocks
                    if (chunkValid) {
                        r.Skip(headerSkip);
                        for (var j = 0; i < numberBuffers; j++) {
                            var signature = 0U;
                            if (variant == 0)
                                signature = r.SkipAfter(r.ReadUInt32X(bigEndian), 4);
                            else if (variant == 1 || variant == 2) {
                                signature = r.SkipAfter(r.ReadUInt32X(bigEndian), 4);
                                if (signature != i + 1) { chunkValid = false; break; }
                                signature = r.ReadUInt32X(bigEndian);
                            }
                            var totalBytes = 0U;
                            for (var k = 0; k < numberLayers; k++) totalBytes += r.ReadUInt32X(bigEndian);
                            if (signature != 3) { chunkValid = false; break; }
                            if (totalBytes >= endOffset - chunkStart || totalBytes < numberLayers * 4 + 8) { chunkValid = false; break; }
                            r.Skip(totalBytes);
                        }
                        fullSize = r.Tell() - chunkStart;
                    }
                    // chunk is valid
                    if (chunkValid) { r.Seek(chunkStart); return (chunkStart - startOffset, fullSize); }
                }
                else if (buf[i] == 9) {
                    var b = r.ReadBytes(20);
                    // check the bytes
                    chunkValid = false; bigEndian = false;
                    if (b[0] == 9 && b[1] == 0 && b[2] == 16 && b[3] == 0 && b[4] == 0 && b[5] == 0 && b[6] == 0 && b[7] == 0) { chunkValid = true; bigEndian = false; }
                    else if (chunkStart >= 3) {
                        chunkStart -= 3; r.Seek(chunkStart); // adjust the chuck size and reread
                        b = r.ReadBytes(20);
                        if (b[0] == 0 && b[1] == 16 && b[2] == 0 && b[3] == 9 && b[4] == 0 && b[5] == 0 && b[6] == 0 && b[7] == 0) { chunkValid = true; bigEndian = true; }
                    }
                    // get information
                    var numberLayers = bigEndian ? BinaryPrimitives.ReadUInt32BigEndian(b[8..]) : BinaryPrimitives.ReadUInt32LittleEndian(b[8..]);
                    var numberBuffers = bigEndian ? BinaryPrimitives.ReadUInt32BigEndian(b[12..]) : BinaryPrimitives.ReadUInt32LittleEndian(b[12..]);
                    var totalInfoSize = bigEndian ? BinaryPrimitives.ReadUInt32BigEndian(b[16..]) : BinaryPrimitives.ReadUInt32LittleEndian(b[16..]);
                    if (numberLayers > 64 || totalInfoSize >= endOffset - chunkStart || numberLayers == 0 || numberBuffers < 1) chunkValid = false;
                    // walk the blocks
                    if (chunkValid) {
                        r.Skip(totalInfoSize + (64 - numberLayers * 4));
                        var headerSizes = 0U;
                        for (var j = 0; i < numberLayers; j++) headerSizes += r.ReadUInt32X(bigEndian);
                        if (headerSizes > endOffset - chunkStart) chunkValid = false;
                        else {
                            r.Skip(headerSizes);
                            for (var j = 0; i < numberBuffers; j++) {
                                var signature = r.SkipAfter(r.ReadUInt32X(bigEndian), 4);
                                var totalBytes = 0U;
                                for (var k = 0; k < numberLayers; k++) totalBytes += r.ReadUInt32X(bigEndian);
                                if (signature != 3) { chunkValid = false; break; }
                                if (totalBytes >= endOffset - chunkStart) { chunkValid = false; break; }
                                r.Skip(totalBytes);
                            }
                            fullSize = r.Tell() - chunkStart;
                        }
                    }
                    // chunk is valid
                    if (chunkValid) { r.Seek(chunkStart); return (chunkStart - startOffset, fullSize); }
                }
                else if (buf[i] == 79) {
                    // chunk is valid
                    (chunkValid, fullSize) = CheckOggChunk(r);
                    if (chunkValid) { r.Seek(chunkStart); return (chunkStart - startOffset, fullSize); }
                }
                // reset
                r.Seek(offsetReset);
                fullSize = 0;
            }
        }
        return (-1, -1);
    }

    enum EUFormat {
        NULL,
        UBI_V3,
        UBI_V5,
        UBI_V6,
        UBI_IV2,
        UBI_IV8,
        UBI_IV9,
        UBI_6OR4,
        UBI_RAW,
        PCM,
        RAW = PCM,
        OGG
    }

    static EUFormat DetermineFormat(BinaryReader r, long offset, long size) {
        r.Seek(offset);
        // calculate actual size
        if (size < 1) size = r.BaseStream.Length - offset;
        // read in the signature
        var magic = r.ReadBytes(4);
        var type = EUFormat.NULL;
        if (magic[0] == 3) type = EUFormat.UBI_V3;
        else if (magic[0] == 5) type = EUFormat.UBI_V5;
        else if (magic[0] == 6) type = EUFormat.UBI_V6;
        else if (magic[0] == 2) type = EUFormat.UBI_IV2;
        else if (magic[0] == 8 && magic[1] == 0 && magic[2] == 0 && magic[3] == 0) {
            // Try a version 8 interleaved stream first
            //CFileDataStream FileStream(input, beginning, size);
            //CInterleavedStream Stream(FileStream);
            //try {
            //    std::vector<unsignedlong> Layers;
            //    Layers.push_back(1);
            //    Stream.SetCurrentLayers(Layers);
            //    // Initialize
            //    if (!Stream.InitializeHeader()) type = EUFormat.UBI_6OR4; // Not a version 8 so must be a 6-Or-4
            //    else {
            //        short Buffer[1024];
            //        long NumberSamples = 1024;
            //        if (Stream.Decode(Buffer, NumberSamples)) type = EUFormat.UBI_IV8;
            //        else type = EUFormat.UBI_6OR4;
            //    }
            //}
            //catch { type = EUFormat.UBI_6OR4; } // Not a version 8 so must be a 6-Or-4
        }
        else if (magic[0] == 8 && magic[1] == 0) type = EUFormat.UBI_IV8;
        else if (magic[0] == 9 && magic[1] == 0) type = EUFormat.UBI_IV9;
        else if (magic[3] == 9 && magic[2] == 0) type = EUFormat.UBI_IV9;
        else if (magic[0] == 7 && magic[1] == 0) type = EUFormat.UBI_IV8;
        else if (magic[3] == 8 && magic[2] == 0) type = EUFormat.UBI_IV8;
        else if (magic[0] == 'O' && magic[1] == 'g' && magic[2] == 'g' && magic[3] == 'S') type = EUFormat.OGG;
        return type;
    }

    #endregion

    public override Task Read(BinaryArchive source, BinaryReader r, object tag) {
        var files = source.Files = [];
        // scan for the first chunk
        var endOffset = r.BaseStream.Length;
        var (found, sizeRead) = Scan(r, endOffset);
        // loop, until we could find no more chunks
        var bytesRead = 0L;
        while (found != -1) {
            var offset = r.Tell();
            // Seek past the current chunk, saving the current chunk size
            int fileSize;
            if (sizeRead != 0) { fileSize = (int)sizeRead; r.Skip(fileSize); }
            else { fileSize = 0; r.Skip(28); }
            // Scan for the next chunk
            (found, sizeRead) = Scan(r, endOffset);
            if (found != -1) bytesRead += 28; // we already passed the header so we don't find it again
            else bytesRead = endOffset - offset; // Assume it goes to the end of the file
            // make sure the chunk has some reasonable size
            if (fileSize == 0 && bytesRead < 48) {
                // Assume it is a simple chunk and skip to the next file; this one is too small
                // The next file cannot start at the next byte
                r.Skip(29);
                (found, sizeRead) = Scan(r, endOffset);
                continue;
            }
            // Set some variables
            if (fileSize == 0) fileSize = (int)bytesRead;
            // add
            var format = DetermineFormat(r, offset, fileSize);
            files.Add(new FileSource {
                Path = $"Sample{files.Count}.{format.ToString().ToLowerInvariant()}",
                FileSize = fileSize,
                Offset = offset,
                Tag = format,
            });
        }
        return Task.CompletedTask;
    }

    public override Task<Stream> ReadData(BinaryArchive source, BinaryReader r, FileSource file, object option = default) {
        r.Seek(file.Offset);
        return Task.FromResult((Stream)new MemoryStream(r.ReadBytes((int)file.FileSize)));
    }
}

#endregion

#region Binary_Map

public class Binary_Map : IHaveMetaInfo {
    #region Map

    public enum Size : uint {
        Small = 0,
        Medium = 1,
        Large = 2,
        ExtraLarge = 3,
    }

    public enum Players : uint {
        TwoToFour = 0,
        FourToEight = 1,
        EightToTwelve = 2,
        TwelveToSixteen = 3,
    }

    public class InfoX(BinaryReader r) {
        public uint Unknown2 = r.ReadUInt32();
        public uint Unknown3 = r.ReadUInt32();
        public uint Unknown4 = r.ReadUInt32();
        public ulong Unknown5 = r.ReadUInt64();
        public string Creator = r.ReadL32UString();
        public ulong Unknown7 = r.ReadUInt64();
        public string Author = r.ReadL32UString();
        public string Name = r.ReadL32UString();
        public ulong Unknown10 = r.ReadUInt64();
        public byte[] Unknown11 = r.ReadBytes(36);
        public byte[] Unknown12 = r.ReadBytes(36);
        public Size Size = (Size)r.ReadUInt32();
        public Players Players = (Players)r.ReadUInt32();
        public uint Unknown15 = r.ReadUInt32();
    }

    public class SnapshotX {
        public uint Width;
        public uint Height;
        public uint BytesPerPixel;
        public uint Unknown4;
        public byte[] Data;
        public string[] Unknown5;

        public SnapshotX(BinaryReader r) {
            Width = r.ReadUInt32();
            Height = r.ReadUInt32();
            BytesPerPixel = r.ReadUInt32();
            Unknown4 = r.ReadUInt32();
            Data = r.ReadBytes(Unknown4 * BytesPerPixel * Height * Width / 8);
            Unknown5 = r.ReadL32FArray(z => r.ReadL32UString());
        }
    }

    public class DataX(BinaryReader r) {
        public string Unknown1 = r.ReadL32UString();
        public SnapshotX Unknown2 = new(r);
        public string[] Unknown3 = r.ReadL32FArray(z => r.ReadL32UString());
    }

    public struct Block(uint virtualOffset, uint fileOffset) {
        public uint VirtualOffset = virtualOffset;
        public uint FileOffset = fileOffset & 0x7FFFFFFF;
        public bool IsCompressed = (fileOffset & 0x80000000) != 0;
    }

    public class CompressedData {
        public byte[] Data;
        public Block[] Blocks;

        public CompressedData(BinaryReader r) {
            var offset = r.ReadUInt32();
            var length = offset - 4;
            Data = new byte[length];
            if (r.Read(Data, 0, Data.Length) != Data.Length) throw new FormatException();
            Blocks = r.ReadL32FArray(z => new Block(r.ReadUInt32(), r.ReadUInt32()));
            if (Blocks.Length == 0 || Blocks.First().FileOffset != 4 || Blocks.Last().FileOffset != 4 + Data.Length) throw new FormatException();
        }

        public MemoryStream Read() {
            var s = new MemoryStream();
            using var data = new MemoryStream(Data);
            for (var i = 0; i + 1 < Blocks.Length; i++) {
                var block = Blocks[i + 0];
                var next = Blocks[i + 1];
                var size = next.VirtualOffset - block.VirtualOffset;
                data.Seek(block.FileOffset - 4, SeekOrigin.Begin);
                s.Seek(block.VirtualOffset, SeekOrigin.Begin);
                if (block.IsCompressed) new InflaterInputStream(data).CopyTo(s, size);
                else data.CopyTo(s, size);
            }
            s.Position = 0;
            return s;
        }
    }

    public class ArchiveX {
        public uint Version;
        public CompressedData DAT;
        public CompressedData FAT;
        public CompressedData XML;

        public ArchiveX(BinaryReader r) {
            var baseOffset = r.Tell();
            var magic = r.ReadUInt32();
            if (magic != 0x4D324346) throw new FormatException("BAD MAGIC");  // FC2M
            var version = r.ReadUInt32();
            if (version != 1) throw new FormatException();
            uint offsetA = r.ReadUInt32(), offsetB = r.ReadUInt32(), offsetC = r.ReadUInt32();
            if (offsetA != 20) throw new FormatException();
            DAT = new CompressedData(r);
            if (baseOffset + offsetB != r.Tell()) throw new FormatException();
            FAT = new CompressedData(r);
            if (baseOffset + offsetC != r.Tell()) throw new FormatException();
            XML = new CompressedData(r);
        }
    }

    #endregion

    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Map(r));

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = this }),
    ];

    public InfoX Info;
    public SnapshotX Snapshot;
    public DataX Data;
    public ArchiveX Archive;

    public Binary_Map(BinaryReader r) {
        var version = r.ReadUInt32();
        if (version != 11) throw new FormatException();
        var typeHash = r.ReadUInt32();
        if (typeHash != 0xD2FD0A6B) throw new FormatException();
        Info = new InfoX(r);
        Snapshot = new SnapshotX(r);
        Data = new DataX(r);
        Archive = new ArchiveX(r);
    }
}

#endregion

#region Other

//class Segment {
//    static bool ReadWhitespace(StreamReader sr) {
//        while (!sr.EndOfStream) {
//            var c = (char)sr.Peek();
//            if (c == 9 || c == 32) { sr.Read(); continue; }
//            return true;
//        }
//        return true;
//    }

//    internal static bool Parse(Stream stream) {
//        var sr = new StreamReader(stream);
//        while (!sr.EndOfStream) {
//            // Read the whitespace
//            ReadWhitespace(sr);
//            if (sr.EndOfStream) break;

//            // Get one character to figure out what kind of line this is
//            var segment = new Segment();
//            var c = (char)sr.Peek();
//            // Base our decision on whether it starts with a letter or a number
//            if (char.IsDigit(c)) {
//                //if (!ReadOffsetSizeLine(sr, Segment)) return false;
//            }
//            else if (char.IsLetter(c)) {
//                //if (!ReadKeywordLine(sr, Segment)) return false;
//            }
//            else if (c == 13 || c == 10) { } // Empty line are acceptable
//            else {
//                var message = $"Unexpected first character '{c}'. Needs to be a number or a digit.";
//                var status = false;
//                return false;
//            }
//        }
//        return true;
//    }
//}

#endregion