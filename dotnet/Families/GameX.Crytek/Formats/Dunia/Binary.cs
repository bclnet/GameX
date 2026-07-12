using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GameX.Crytek.Formats.Dunia;


#region Binary_AIWorkspace

//public class AIWorkspaceResourceFile {
//    public List<UnknownData0> Unknown0 = new List<UnknownData0>();
//    public byte[] Unknown1;
//    public List<uint> VariableNameHashes = new List<uint>();
//    public List<UnknownData3> Unknown3 = new List<UnknownData3>();
//    public XmlResourceFile XmlResource;

//    public void Deserialize(Stream input) {
//        var version = input.ReadValueU32(Endian.Little);
//        if (version < 1 || version > 4) {
//            throw new FormatException();
//        }
//        var endian = Endian.Little;

//        var unknownLength = input.ReadValueU32(endian);
//        var rmlLength = input.ReadValueU32(endian);

//        if (input.Position + unknownLength + rmlLength > input.Length) {
//            throw new FormatException();
//        }

//        using (var data = input.ReadToMemoryStream((int)unknownLength)) {
//            var unk0count = data.ReadValueU32(endian);
//            this.Unknown0.Clear();
//            for (uint i = 0; i < unk0count; i++) {
//                var id = data.ReadValueU32(endian);
//                var length = data.ReadValueU32(endian);
//                var xml = new XmlResourceFile();
//                using (var data2 = data.ReadToMemoryStream((int)length)) {
//                    xml.Deserialize(data2);
//                }
//                this.Unknown0.Add(new UnknownData0() {
//                    TypeHash = id,
//                    XmlResource = xml,
//                });
//            }

//            var unk1length = data.ReadValueU32(endian);
//            this.Unknown1 = new byte[unk1length];
//            if (data.Read(this.Unknown1, 0, this.Unknown1.Length) != this.Unknown1.Length) {
//                throw new FormatException();
//            }

//            this.VariableNameHashes.Clear();
//            var variableNameCount = data.ReadValueU32(endian);
//            for (uint i = 0; i < variableNameCount; i++) {
//                this.VariableNameHashes.Add(data.ReadValueU32(endian));
//            }

//            this.Unknown3.Clear();
//            var unk3count = data.ReadValueU32(endian);
//            for (uint i = 0; i < unk3count; i++) {
//                var unknown3 = new UnknownData3();
//                unknown3.NameHash = data.ReadValueU32(endian);
//                var unk1 = data.ReadValueU32(endian);
//                unknown3.Name = data.ReadString((int)unk1, Encoding.UTF8);
//                unknown3.IndexIntoUnknown0 = data.ReadValueU32(endian);
//                unknown3.IndexIntoUnknown1 = data.ReadValueU32(endian);
//                unknown3.Unknown4 = data.ReadValueU32(endian);
//                this.Unknown3.Add(unknown3);
//            }
//        }

//        this.XmlResource = new XmlResourceFile();
//        using (var data = input.ReadToMemoryStream((int)rmlLength)) {
//            this.XmlResource.Deserialize(data);
//            if (data.Position != data.Length) {
//                throw new FormatException();
//            }
//        }

//        var test_u2 = this.Unknown3.Max(u => u.IndexIntoUnknown0);
//        var test_u3 = this.Unknown3.Max(u => u.IndexIntoUnknown1);
//        var test_u4 = this.Unknown3.Max(u => u.Unknown4);
//    }

//    public class UnknownData0 {
//        public uint TypeHash;
//        public XmlResourceFile XmlResource;
//    }

//    public class UnknownData3 {
//        public uint NameHash;
//        public string Name;
//        public uint IndexIntoUnknown0;
//        public uint IndexIntoUnknown1;
//        public uint Unknown4;
//    }
//}

#endregion

#region Binary_Resource

#endregion

#region Binary_Geometry

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
