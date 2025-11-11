using OpenStack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Xbox.Formats;

#region Binary_Xnb

public class Binary_Xnb : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Xnb(r));

    #region Type Reader

    public class TypeReader(string name, string target, Type type, Func<ContentReader, object> read, bool valueType = false) {
        public string Name = name;
        public string Target = target;
        public Type Type = type;
        public Func<ContentReader, object> Read = read;
        public bool ValueType = valueType;
    }

    public class GenericReader : TypeReader {
        public Func<GenericReader, object> Target2;
        public Func<ContentReader, GenericReader, object> Read2;
        public Action<GenericReader> Init;
        public List<string> Args;
        public TypeReader KeyReader;
        public TypeReader ValueReader;

        public GenericReader(string name, string target, Func<GenericReader, object> target2, Func<ContentReader, GenericReader, object> read2, Action<GenericReader> init = null, bool valueType = false, List<string> args = null) : base(name, target, null, null, valueType) {
            Target2 = target2;
            Read2 = read2; Read = r => Read2(r, this);
            Init = init;
            Args = args;
        }

        public TypeReader Create(List<string> args) {
            var suffix = $"`{args.Count}[[{string.Join("],[", args)}]]";
            return new GenericReader(Name + suffix, Target + suffix, Target2, Read2, Init, ValueType, args);
        }
    }

    public class ContentReader(Stream input) : BinaryReader(input) {
        readonly static List<TypeReader> TypeReaders = [
            // Primitive types
            new TypeReader("ByteReader", "System.Byte", typeof(byte), r => r.ReadByte(), valueType: true),
            new TypeReader("SByteReader", "System.SByte", typeof(sbyte), r => r.ReadSByte(), valueType: true),
            new TypeReader("Int16Reader", "System.Int16", typeof(short), r => r.ReadInt16(), valueType: true),
            new TypeReader("UInt16Reader", "System.UInt16", typeof(ushort), r => r.ReadUInt16(), valueType: true),
            new TypeReader("Int32Reader", "System.Int32", typeof(int), r => r.ReadInt32(), valueType: true),
            new TypeReader("UInt32Reader", "System.UInt32", typeof(uint), r => r.ReadUInt32(), valueType: true),
            new TypeReader("Int64Reader", "System.Int64", typeof(long), r => r.ReadInt64(), valueType: true),
            new TypeReader("UInt64Reader", "System.UInt64", typeof(ulong), r => r.ReadUInt64(), valueType: true),
            new TypeReader("SingleReader", "System.Single", typeof(float), r => r.ReadSingle(), valueType: true),
            new TypeReader("DoubleReader", "System.Double", typeof(double), r => r.ReadDouble(), valueType: true),
            new TypeReader("BooleanReader", "System.Boolean", typeof(bool), r => r.ReadBoolean(), valueType: true),
            new TypeReader("CharReader", "System.Char", typeof(char), r => r.ReadChar(), valueType: true),
            new TypeReader("StringReader", "System.String", typeof(string), r => r.ReadLV7UString()),
            new TypeReader("ObjectReader", "System.Object", typeof(object), r => throw new NotSupportedException()),

            // System types
            new GenericReader("EnumReader", "System.Enum", s => s.Args[0], (r, s) => r.ReadInt32()),
            new GenericReader("NullableReader", "System.Nullable", null, (r, s) => r.ReadBoolean() ? s.ValueReader.Read(r) : null, s => { s.ValueReader = GetByTarget(s.Args[0]); }, valueType: true),
            new GenericReader("ArrayReader", "System.Array", s => s.Args[0] + "[]", (r, s) => r.ReadL32FArray(z => r.ReadValueOrObject(s.ValueReader)), s => { s.ValueReader = GetByTarget(s.Args[0]); }),
            new GenericReader("ListReader", "System.Collections.Generic.List", null, (r, s) => r.ReadL32FArray(z => r.ReadValueOrObject(s.ValueReader)), s => { s.ValueReader = GetByTarget(s.Args[0]); }),
            new GenericReader("DictionaryReader", "System.Collections.Generic.Dictionary", null, (r, s) => r.ReadL32FMany(z => r.ReadValueOrObject(s), z => r.ReadValueOrObject(s)), s => { s.KeyReader = GetByTarget(s.Args[0]); s.ValueReader = GetByTarget(s.Args[1]); }),
            new TypeReader("TimeSpanReader", "System.TimeSpan", typeof(TimeSpan), r => { var v = r.ReadInt64(); return new TimeSpan(v); }, valueType: true),
            new TypeReader("DateTimeReader", "System.DateTime", typeof(DateTime), r => { var v = r.ReadInt64(); return new DateTime(v & ~(3L << 62), (DateTimeKind)(v >> 62)); }, valueType: true),
            new TypeReader("DecimalReader", "System.Decimal", typeof(decimal), r => { uint a = r.ReadUInt32(), b = r.ReadUInt32(), c = r.ReadUInt32(), d = r.ReadUInt32(); return 0; }, valueType: true),
            new TypeReader("ExternalReferenceReader", "ExternalReference", typeof(string), r => r.ReadString()),
            new GenericReader("ReflectiveReader", "System.Object", s => s.Args[0], (r, s) => throw new NotSupportedException()),

            // Math types
            new TypeReader("Vector2Reader", "Framework.Vector2", typeof(object), r => r.ReadVector2(), valueType: true),
            new TypeReader("Vector3Reader", "Framework.Vector3", typeof(object), r => r.ReadVector3(), valueType: true),
            new TypeReader("Vector4Reader", "Framework.Vector4", typeof(object), r => r.ReadVector4(), valueType: true),
            new TypeReader("MatrixReader", "Framework.Matrix", typeof(object), r => r.ReadMatrix4x4(), valueType: true),
            new TypeReader("QuaternionReader", "Framework.Quaternion", typeof(object), r => r.ReadQuaternion(), valueType: true),
            new TypeReader("ColorReader", "Framework.Color", typeof(object), r => new Vector4<byte>(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte()), valueType: true), // #rgba
            new TypeReader("PlaneReader", "Framework.Plane", typeof(object), r => (normal: r.ReadVector3(), d: r.ReadSingle()), valueType: true),
            new TypeReader("PointReader", "Framework.Point", typeof(object), r => new Vector2<int>(r.ReadInt32(), r.ReadInt32()), valueType: true),
            new TypeReader("RectangleReader", "Framework.Rectangle", typeof(object), r => new Vector4<int>(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32()), valueType: true), // #xywh
            new TypeReader("BoundingBoxReader", "Framework.BoundingBox", typeof(object), r => (min: r.ReadVector3(), max: r.ReadVector3()), valueType: true),
            new TypeReader("BoundingSphereReader", "Framework.BoundingSphere", typeof(object), r => (center: r.ReadVector3(), radius: r.ReadSingle()), valueType: true),
            new TypeReader("BoundingFrustumReader", "Framework.BoundingFrustum", typeof(object), r => r.ReadMatrix4x4()),
            new TypeReader("RayReader", "Framework.Ray", typeof(object), r => (position: r.ReadVector3(), direction: r.ReadVector3()), valueType: true),
            new TypeReader("CurveReader", "Framework.Curve", typeof(object), r => {
                var preLoop = r.ReadInt32();
                var postLoop = r.ReadInt32();
                var loops = r.ReadL32FArray(z => (position: z.ReadSingle(), value: z.ReadSingle(), tangentIn: z.ReadSingle(), tangentOut: z.ReadSingle(), continuity: z.ReadInt32()));
                return (preLoop, postLoop, loops);
            }),

            // Graphics types
            new TypeReader("TextureReader", "Framework.Graphics.Texture", typeof(object), r => throw new NotSupportedException()),
            new TypeReader("Texture2DReader", "Framework.Graphics.Texture2D", typeof(object), r => new Texture2D(r)),
            new TypeReader("Texture3DReader", "Framework.Graphics.Texture3D", typeof(object), r => new Texture3D(r)),
            new TypeReader("TextureCubeReader", "Framework.Graphics.TextureCube", typeof(object), r => new TextureCube(r)),
            new TypeReader("IndexBufferReader", "Framework.Graphics.IndexBuffer", typeof(object), r => new IndexBuffer(r)),
            new TypeReader("VertexBufferReader", "Framework.Graphics.VertexBuffer", typeof(object), r => new VertexBuffer(r)),
            new TypeReader("VertexDeclarationReader", "Framework.Graphics.VertexDeclaration", typeof(object), r => new VertexDeclaration(r)),
            new TypeReader("EffectReader", "Framework.Graphics.Effect", typeof(object), r => new Effect(r)),
            new TypeReader("EffectMaterialReader", "Framework.Graphics.EffectMaterial", typeof(object), r => new EffectMaterial(r)),
            new TypeReader("BasicEffectReader", "Framework.Graphics.BasicEffect", typeof(object), r => new BasicEffect(r)),
            new TypeReader("AlphaTestEffectReader", "Framework.Graphics.AlphaTestEffect", typeof(object), r => new AlphaTestEffect(r)),
            new TypeReader("DualTextureEffectReader", "Framework.Graphics.DualTextureEffect", typeof(object), r => new DualTextureEffect(r)),
            new TypeReader("EnvironmentMapEffectReader", "Framework.Graphics.EnvironmentMapEffect", typeof(object), r => new EnvironmentMapEffect(r)),
            new TypeReader("SkinnedEffectReader", "Framework.Graphics.SkinnedEffect", typeof(object), r => new SkinnedEffect(r)),
            new TypeReader("SpriteFontReader", "Framework.Graphics.SpriteFont", typeof(object), r => new SpriteFont(r)),
            new TypeReader("ModelReader", "Framework.Graphics.Model", typeof(object), r => new Model(r)),

            // Media types
            new TypeReader("SoundEffectReader", "Audio.SoundEffect", typeof(object), r => new SoundEffect(r)),
            new TypeReader("SongReader", "Media.Song", typeof(object), r => new Song(r)),
            new TypeReader("VideoReader", "Media.Video", typeof(object), r => new Video(r))
        ];

        readonly static Dictionary<string, TypeReader> TypeReaderMap = TypeReaders.ToDictionary(s => s.Name);
        TypeReader[] Readers;

        public void ReadTypeManifest() {
            Readers = this.ReadLV7FArray(z => GetByName(this.ReadLV7UString(), this.ReadUInt32()));
            foreach (var s in Readers.Where(x => x is GenericReader).Cast<GenericReader>()) s.Init(s);
        }
        public object ReadObject() { var reader = ReadTypeId(); return reader != null ? reader.Read(this) : null; }
        public object ReadValueOrObject(TypeReader reader) => reader.ValueType ? Convert.ChangeType(reader.Read(this), reader.Type) : ReadObject();
        public TypeReader ReadTypeId() { var typeId = this.ReadVInt7() - 1; return typeId >= 0 ? typeId < Readers.Length ? Readers[typeId] : throw new Exception("Invalid XNB file: typeId is out of range.") : null; }
        public ContentReader Validate(string type) { var reader = ReadTypeId(); return reader == null || reader.Target != type ? throw new Exception("Invalid XNB file: got an unexpected typeId.") : this; }

        public static TypeReader GetByName(string name, uint version) {
            var wanted = StripAssemblyVersion(name).Replace("Microsoft.Xna.Framework.Content.", "");
            if (TypeReaderMap.TryGetValue(wanted, out var reader)) return reader;
            // could this be a specialization of a generic reader?
            var (genericName, genericArguments) = SplitGenericTypeName(wanted);
            if (genericName == null) return default;
            // look for a generic reader factory with this name.
            if (TypeReaderMap.TryGetValue(genericName, out reader) && reader is GenericReader factory) {
                // create a specialized generic reader instance.
                reader = factory.Create(genericArguments);
                if (reader.Name != wanted) throw new Exception("ERROR");
                TypeReaders.Add(reader);
                TypeReaderMap[reader.Name] = reader;
                return reader;
            }
            throw new Exception($"Cannot find type reader '{wanted}'.");
        }

        public static TypeReader GetByTarget(string target) {
            var wanted = StripAssemblyVersion(target).Replace("Microsoft.Xna.", "");
            var reader = TypeReaders.FirstOrDefault(x => x.Target == wanted);
            if (reader != null) return reader;
            throw new Exception($"Cannot find reader for target type '{wanted}'.");
        }

        static string StripAssemblyVersion(string name) {
            var commaIndex = 0;
            while ((commaIndex = name.IndexOf(',', commaIndex)) != -1) {
                if (commaIndex + 1 < name.Length && name[commaIndex + 1] == '[') commaIndex++;
                else {
                    var closeBracket = name.IndexOf(']', commaIndex);
                    if (closeBracket != -1) name = name.Remove(commaIndex, closeBracket - commaIndex);
                    else name = name[..commaIndex];
                }
            }
            return name;
        }

        static (string, List<string>) SplitGenericTypeName(string name) {
            // look for the ` generic marker character.
            var pos = name.IndexOf('`');
            if (pos == -1) return default;
            // everything to the left of ` is the generic type name.
            var genericName = name[..pos]; var genericArguments = new List<string>();
            // advance to the start of the generic argument list.
            pos++;
            while (pos < name.Length && char.IsDigit(name[pos])) pos++;
            while (pos < name.Length && name[pos] == '[') pos++;
            // split up the list of generic type arguments.
            while (pos < name.Length && name[pos] != ']') {
                // locate the end of the current type name argument.
                int nesting = 0, end;
                for (end = pos; end < name.Length; end++) {
                    // handle nested types in case we have eg. "List`1[[List`1[[Int]]]]".
                    if (name[end] == '[') nesting++;
                    else if (name[end] == ']') {
                        if (nesting > 0) nesting--;
                        else break;
                    }
                }
                // extract the type name argument.
                genericArguments.Add(name[pos..end]);
                // skip past the type name, plus any subsequent "],[" goo.
                pos = end;
                if (pos < name.Length && name[pos] == ']') pos++;
                if (pos < name.Length && name[pos] == ',') pos++;
                if (pos < name.Length && name[pos] == '[') pos++;
            }
            return (genericName, genericArguments);
        }
    }

    #endregion

    #region Gfx Objects

    public enum SurfaceFormat {
        Color,
        Bgr565,
        Bgra5551,
        Bgra4444,
        Dxt1,
        Dxt3,
        Dxt5,
        NormalizedByte2,
        NormalizedByte4,
        Rgba1010102,
        Rg32,
        Rgba64,
        Alpha8,
        Single,
        Vector2,
        Vector4,
        HalfSingle,
        HalfVector2,
        HalfVector4,
        HdrBlendable
    }

    public enum VertexElementFormat {
        Single,
        Vector2,
        Vector3,
        Vector4,
        Color,
        Byte4,
        Short2,
        Short4,
        NormalizedShort2,
        NormalizedShort4,
        HalfVector2,
        HalfVector4
    }

    public enum VertexElementUsage {
        Position,
        Color,
        TextureCoordinate,
        Normal,
        Binormal,
        Tangent,
        BlendIndices,
        BlendWeight,
        Depth,
        Fog,
        PointSize,
        Sample,
        TessellateFactor
    }

    public enum CompareFunction { Always, Never, Less, LessEqual, Equal, GreaterEqual, Greater, NotEqual }

    public class Texture2D(ContentReader r) {
        public SurfaceFormat Format = (SurfaceFormat)r.ReadInt32();
        public uint Width = r.ReadUInt32();
        public uint Height = r.ReadUInt32();
        public byte[][] Mips = r.ReadL32FArray(z => r.ReadL32Bytes());
    }

    public class Texture3D(ContentReader r) {
        public SurfaceFormat Format = (SurfaceFormat)r.ReadInt32();
        public uint Width = r.ReadUInt32();
        public uint Height = r.ReadUInt32();
        public uint Depth = r.ReadUInt32();
        public byte[][] Mips = r.ReadL32FArray(z => r.ReadL32Bytes());
    }

    public class TextureCube(ContentReader r) {
        public SurfaceFormat Format = (SurfaceFormat)r.ReadInt32();
        public uint Size = r.ReadUInt32();
        public byte[][] Face1Mips = r.ReadL32FArray(z => r.ReadL32Bytes());
        public byte[][] Face2Mips = r.ReadL32FArray(z => r.ReadL32Bytes());
        public byte[][] Face3Mips = r.ReadL32FArray(z => r.ReadL32Bytes());
        public byte[][] Face4Mips = r.ReadL32FArray(z => r.ReadL32Bytes());
        public byte[][] Face5Mips = r.ReadL32FArray(z => r.ReadL32Bytes());
        public byte[][] Face6Mips = r.ReadL32FArray(z => r.ReadL32Bytes());
    }

    public class IndexBuffer(ContentReader r) {
        public int IndexFormat = r.ReadBoolean() ? 16 : 32;
        public byte[] IndexData = r.ReadL32Bytes();
    }

    public class VertexDeclaration(ContentReader r) {
        public class Element(ContentReader r) {
            public uint Offset = r.ReadUInt32();
            public VertexElementFormat Format = (VertexElementFormat)r.ReadInt32();
            public VertexElementUsage Usage = (VertexElementUsage)r.ReadInt32();
            public uint UsageIndex = r.ReadUInt32();
        }
        public uint VertexStride = r.ReadUInt32();
        public Element[] Elements = r.ReadL32FArray(z => new Element(r));
    }

    public class VertexBuffer : VertexDeclaration {
        public byte[][] Vertexs;
        public VertexBuffer(ContentReader r) : base(r) {
            Vertexs = r.ReadL32FArray(z => r.ReadBytes(VertexStride));
        }
    }

    public class Effect(ContentReader r) {
        public byte[] EffectBytecode = r.ReadL32Bytes();
    }

    public class EffectMaterial(ContentReader r) {
        public string EffectReference = r.ReadLV7UString();
        public object Parameters = r.ReadObject();
    }

    public class BasicEffect(ContentReader r) {
        public string TextureReference = r.ReadLV7UString();
        public Vector3 DiffuseColor = r.ReadVector3();
        public Vector3 EmissiveColor = r.ReadVector3();
        public Vector3 SpecularColor = r.ReadVector3();
        public float SpecularPower = r.ReadSingle();
        public float Alpha = r.ReadSingle();
        public bool VertexColorEnabled = r.ReadBoolean();
    }

    public class AlphaTestEffect(ContentReader r) {
        public string TextureReference = r.ReadLV7UString();
        public CompareFunction CompareFunction = (CompareFunction)r.ReadInt32();
        public uint ReferenceAlpha = r.ReadUInt32();
        public Vector3 DiffuseColor = r.ReadVector3();
        public float Alpha = r.ReadSingle();
        public bool VertexColorEnabled = r.ReadBoolean();
    }

    public class DualTextureEffect(ContentReader r) {
        public string Texture1Reference = r.ReadLV7UString();
        public string Texture2Reference = r.ReadLV7UString();
        public Vector3 DiffuseColor = r.ReadVector3();
        public float Alpha = r.ReadSingle();
        public bool VertexColorEnabled = r.ReadBoolean();
    }

    public class EnvironmentMapEffect(ContentReader r) {
        public string TextureReference = r.ReadLV7UString();
        public string EnvironmentMapReference = r.ReadLV7UString();
        public float EnvironmentMapAmount = r.ReadSingle();
        public Vector3 EnvironmentMapSpecular = r.ReadVector3();
        public float FresnelFactor = r.ReadSingle();
        public Vector3 DiffuseColor = r.ReadVector3();
        public Vector3 EmissiveColor = r.ReadVector3();
        public float Alpha = r.ReadSingle();
    }

    public class SkinnedEffect(ContentReader r) {
        public string TextureReference = r.ReadLV7UString();
        public uint WeightsPerVertex = r.ReadUInt32();
        public Vector3 DiffuseColor = r.ReadVector3();
        public Vector3 EmissiveColor = r.ReadVector3();
        public Vector3 SpecularColor = r.ReadVector3();
        public float SpecularPower = r.ReadSingle();
        public float Alpha = r.ReadSingle();
    }

    public class SpriteFont(ContentReader r) {
        public Texture2D Texture = (Texture2D)r.ReadObject();
        public Vector4<int>[] Glyphs = (Vector4<int>[])r.ReadObject();
        public object Cropping = r.ReadObject();
        public object Characters = r.ReadObject();
        public int VerticalLinespacing = r.ReadInt32();
        public float HorizontalSpacing = r.ReadSingle();
        public object Kerning = r.ReadObject();
        public char DefaultCharacter = r.ReadBoolean() ? r.ReadChar() : (char)0;
    }

    public class Model(ContentReader r) {
    }

    #endregion

    #region Media Objects

    public enum SoundtrackType {
        Music,
        Dialog,
        MusicDialog
    }

    public class SoundEffect(ContentReader r) {
        public byte[] Format = r.ReadL32Bytes();
        public byte[] Data = r.ReadL32Bytes();
        public int LoopStart = r.ReadInt32();
        public int LoopLength = r.ReadInt32();
        public int Duration = r.ReadInt32();
    }

    public class Song(ContentReader r) {
        public string Filename = r.ReadLV7UString();
        public int Duration = r.Validate("System.Int32").ReadInt32();
    }

    public class Video(ContentReader r) {
        public string Filename = r.Validate("System.String").ReadLV7UString();
        public int Duration = r.Validate("System.Int32").ReadInt32();
        public int Width = r.Validate("System.Int32").ReadInt32();
        public int Height = r.Validate("System.Int32").ReadInt32();
        public float FramesPerSecond = r.Validate("System.Single").ReadSingle();
        public SoundtrackType SoundtrackType = (SoundtrackType)r.Validate("System.Int32").ReadInt32();
    }

    #endregion

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

        public void Validate(BinaryReader r) {
            if ((Magic & 0x00FFFFFF) != MAGIC) throw new Exception("BAD MAGIC");
            if (Version != 5 && Version != 4) throw new Exception("Invalid XNB version");
            if (SizeOnDisk > r.BaseStream.Length) throw new Exception("XNB file has been truncated.");
            if (Compressed) {
                uint decompressedSize = r.ReadUInt32(), compressedSize = SizeOnDisk - (uint)r.Tell();
                Debug.Log($"{decompressedSize} bytes of asset data are compressed into {compressedSize}");
                throw new Exception("Unsupported reading of compressed XNB files.");
            }
        }
    }

    #endregion

    public object[] objs;

    public Binary_Xnb(BinaryReader r2) {
        var r = new ContentReader(r2.BaseStream);
        var h = r.ReadS<Header>();
        h.Validate(r);
        r.ReadTypeManifest();
        objs = r.ReadFArray(z => r.ReadObject(), r.ReadVInt7() + 1);
        r.EnsureAtEnd(h.SizeOnDisk);
    }

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new("BinaryPak", items: [
            //new($"Type: {Type}"),
        ])
    ];
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
