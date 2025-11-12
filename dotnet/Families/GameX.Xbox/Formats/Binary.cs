using OpenStack;
using SharpCompress.Readers.Rar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static GameX.Xbox.Formats.Binary_Xnb;

namespace GameX.Xbox.Formats;

#region Binary_Xnb

public class Binary_Xnb : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Xnb(r));

    #region Type Reader

    public class TypeReader(Type type, string name, string target, bool valueType = false) {
        public Type Type = type;
        public string Name = name;
        public string Target = target;
        public bool ValueType = valueType;
        public Action Init;
    }

    public class TypeReader<T>(string name, string target, Func<ContentReader, T> read, bool valueType = false) : TypeReader(typeof(TypeReader<T>), name, target, valueType) {
        public Func<ContentReader, T> Read = read;
    }

    public class GenericReader(string name, string target, Action<GenericReader> action, bool valueType = false, List<string> args = null) : TypeReader<object>(name, target, null, valueType) {
        public Action<GenericReader> Action = action;
        public List<string> Args = args;
        public string Target2;
        public TypeReader KeyReader;
        public TypeReader ValueReader;

        public TypeReader Create(List<string> args) {
            var suffix = $"`{args.Count}[[{string.Join("],[", args)}]]";
            return new GenericReader(Name + suffix, Target + suffix, Action, ValueType, args);
        }
    }

    public class GenericReader<T>(string name, string target, bool valueType = false, List<string> args = null) : TypeReader<T>(name, target, null, valueType) {
        public TypeReader<T> ValueReader;
    }

    public class ContentReader(Stream input) : BinaryReader(input) {
        readonly static List<TypeReader> TypeReaders = [
            // Primitive types
            new TypeReader<byte>("ByteReader", "System.Byte", r => r.ReadByte(), valueType: true),
            new TypeReader<sbyte>("SByteReader", "System.SByte", r => r.ReadSByte(), valueType: true),
            new TypeReader<short>("Int16Reader", "System.Int16", r => r.ReadInt16(), valueType: true),
            new TypeReader<ushort>("UInt16Reader", "System.UInt16", r => r.ReadUInt16(), valueType: true),
            new TypeReader<int>("Int32Reader", "System.Int32", r => r.ReadInt32(), valueType: true),
            new TypeReader<uint>("UInt32Reader", "System.UInt32", r => r.ReadUInt32(), valueType: true),
            new TypeReader<long>("Int64Reader", "System.Int64", r => r.ReadInt64(), valueType: true),
            new TypeReader<ulong>("UInt64Reader", "System.UInt64", r => r.ReadUInt64(), valueType: true),
            new TypeReader<float>("SingleReader", "System.Single", r => r.ReadSingle(), valueType: true),
            new TypeReader<double>("DoubleReader", "System.Double", r => r.ReadDouble(), valueType: true),
            new TypeReader<bool>("BooleanReader", "System.Boolean", r => r.ReadBoolean(), valueType: true),
            new TypeReader<char>("CharReader", "System.Char", r => r.ReadChar(), valueType: true),
            new TypeReader<string>("StringReader", "System.String", r => r.ReadLV7UString()),
            new TypeReader<object>("ObjectReader", "System.Object", r => throw new NotSupportedException()),

            // System types
            new GenericReader("EnumReader", "System.Enum", s => {
                s.Target2 = s.Args[0];
                s.Read = r => r.ReadInt32();
            }),
            new GenericReader("NullableReader", "System.Nullable", s => {
                //typeof(GenericReader<>).MakeGenericType()
                //var x = new GenericReader<int>(s.Name, s.Target, s.ValueType);
                s.ValueReader = GetByTarget(s.Args[0]);
                s.Read = r => r.ReadBoolean() ? r.Read(s.ValueReader) : null;
            }, valueType: true),
            new GenericReader("ArrayReader", "System.Array", s => {
                s.Target2 = s.Args[0] + "[]";
                s.ValueReader = GetByTarget(s.Args[0]);
                s.Read = r => r.ReadL32FArray(z => r.ReadValueOrObject(s.ValueReader));
            }),
            new GenericReader("ListReader", "System.Collections.Generic.List", s => {
                s.ValueReader = GetByTarget(s.Args[0]);
                s.Read = r => r.ReadL32FArray(z => r.ReadValueOrObject(s.ValueReader));
            }),
            new GenericReader("DictionaryReader", "System.Collections.Generic.Dictionary", s => {
                s.KeyReader = GetByTarget(s.Args[0]);
                s.ValueReader = GetByTarget(s.Args[1]);
                s.Read = r => r.ReadL32FMany(z => r.ReadValueOrObject(s.KeyReader), z => r.ReadValueOrObject(s.ValueReader));
            }),
            new TypeReader<TimeSpan>("TimeSpanReader", "System.TimeSpan", r => { var v = r.ReadInt64(); return new TimeSpan(v); }, valueType: true),
            new TypeReader<DateTime>("DateTimeReader", "System.DateTime", r => { var v = r.ReadInt64(); return new DateTime(v & ~(3L << 62), (DateTimeKind)(v >> 62)); }, valueType: true),
            new TypeReader<decimal>("DecimalReader", "System.Decimal", r => { uint a = r.ReadUInt32(), b = r.ReadUInt32(), c = r.ReadUInt32(), d = r.ReadUInt32(); return 0; }, valueType: true),
            new TypeReader<string>("ExternalReferenceReader", "ExternalReference", r => r.ReadString()),
            new GenericReader("ReflectiveReader", "System.Object", s => {
                s.Target2 = s.Args[0];
                s.Read = r => throw new NotSupportedException();
            }),

            // Math types
            new TypeReader<Vector2>("Vector2Reader", "Framework.Vector2", r => r.ReadVector2(), valueType: true),
            new TypeReader<Vector3>("Vector3Reader", "Framework.Vector3", r => r.ReadVector3(), valueType: true),
            new TypeReader<Vector4>("Vector4Reader", "Framework.Vector4", r => r.ReadVector4(), valueType: true),
            new TypeReader<Matrix4x4>("MatrixReader", "Framework.Matrix", r => r.ReadMatrix4x4(), valueType: true),
            new TypeReader<Quaternion>("QuaternionReader", "Framework.Quaternion", r => r.ReadQuaternion(), valueType: true),
            new TypeReader<Vector4<byte>>("ColorReader", "Framework.Color", r => new Vector4<byte>(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte()), valueType: true), // #rgba
            new TypeReader<Tuple<Vector3, float>>("PlaneReader", "Framework.Plane", r => new Tuple<Vector3, float>(r.ReadVector3(), r.ReadSingle()), valueType: true), //#normal,d
            new TypeReader<Vector2<int>>("PointReader", "Framework.Point", r => new Vector2<int>(r.ReadInt32(), r.ReadInt32()), valueType: true),
            new TypeReader<Vector4<int>>("RectangleReader", "Framework.Rectangle", r => new Vector4<int>(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32()), valueType: true), // #xywh
            new TypeReader<Tuple<Vector3, Vector3>>("BoundingBoxReader", "Framework.BoundingBox", r => new Tuple<Vector3, Vector3>(r.ReadVector3(), r.ReadVector3()), valueType: true), //#min,max
            new TypeReader<Tuple<Vector3, float>>("BoundingSphereReader", "Framework.BoundingSphere", r => new Tuple<Vector3, float>(r.ReadVector3(), r.ReadSingle()), valueType: true), //#center,radius
            new TypeReader<Matrix4x4>("BoundingFrustumReader", "Framework.BoundingFrustum", r => r.ReadMatrix4x4()),
            new TypeReader<Tuple<Vector3, Vector3>>("RayReader", "Framework.Ray", r => new Tuple<Vector3, Vector3>(r.ReadVector3(),  r.ReadVector3()), valueType: true), //#position,direction
            //new TypeReader<object>("CurveReader", "Framework.Curve", r => {
            //    var preLoop = r.ReadInt32();
            //    var postLoop = r.ReadInt32();
            //    var loops = r.ReadL32FArray(z => (position: z.ReadSingle(), value: z.ReadSingle(), tangentIn: z.ReadSingle(), tangentOut: z.ReadSingle(), continuity: z.ReadInt32()));
            //    return (preLoop, postLoop, loops);
            //}),

            // Graphics types
            new TypeReader<object>("TextureReader", "Framework.Graphics.Texture", r => throw new NotSupportedException()),
            new TypeReader<Texture2D>("Texture2DReader", "Framework.Graphics.Texture2D", r => new Texture2D(r)),
            new TypeReader<Texture3D>("Texture3DReader", "Framework.Graphics.Texture3D", r => new Texture3D(r)),
            new TypeReader<TextureCube>("TextureCubeReader", "Framework.Graphics.TextureCube", r => new TextureCube(r)),
            new TypeReader<IndexBuffer>("IndexBufferReader", "Framework.Graphics.IndexBuffer", r => new IndexBuffer(r)),
            new TypeReader<VertexBuffer>("VertexBufferReader", "Framework.Graphics.VertexBuffer", r => new VertexBuffer(r)),
            new TypeReader<VertexDeclaration>("VertexDeclarationReader", "Framework.Graphics.VertexDeclaration", r => new VertexDeclaration(r)),
            new TypeReader<Effect>("EffectReader", "Framework.Graphics.Effect", r => new Effect(r)),
            new TypeReader<EffectMaterial>("EffectMaterialReader", "Framework.Graphics.EffectMaterial", r => new EffectMaterial(r)),
            new TypeReader<BasicEffect>("BasicEffectReader", "Framework.Graphics.BasicEffect", r => new BasicEffect(r)),
            new TypeReader<AlphaTestEffect>("AlphaTestEffectReader", "Framework.Graphics.AlphaTestEffect", r => new AlphaTestEffect(r)),
            new TypeReader<DualTextureEffect>("DualTextureEffectReader", "Framework.Graphics.DualTextureEffect", r => new DualTextureEffect(r)),
            new TypeReader<EnvironmentMapEffect>("EnvironmentMapEffectReader", "Framework.Graphics.EnvironmentMapEffect", r => new EnvironmentMapEffect(r)),
            new TypeReader<SkinnedEffect>("SkinnedEffectReader", "Framework.Graphics.SkinnedEffect", r => new SkinnedEffect(r)),
            new TypeReader<SpriteFont>("SpriteFontReader", "Framework.Graphics.SpriteFont", r => new SpriteFont(r)),
            new TypeReader<Model>("ModelReader", "Framework.Graphics.Model", r => new Model(r)),

            // Media types
            new TypeReader<SoundEffect>("SoundEffectReader", "Audio.SoundEffect", r => new SoundEffect(r)),
            new TypeReader<Song>("SongReader", "Media.Song", r => new Song(r)),
            new TypeReader<Video>("VideoReader", "Media.Video", r => new Video(r))
        ];

        readonly static Dictionary<string, TypeReader> TypeReaderMap = TypeReaders.ToDictionary(s => s.Name);
        TypeReader[] Readers;

        public void ReadTypeManifest() {
            Readers = this.ReadLV7FArray(z => GetByName(this.ReadLV7UString(), this.ReadUInt32()));
            foreach (var s in Readers.Where(x => x.Init != null)) s.Init();
        }

        public object Read(TypeReader reader) => reader != null ? reader.Type.GetMethod("Read").Invoke(reader, [this]) : default;
        public object ReadObject() => Read(ReadTypeId());
        public object ReadValueOrObject(TypeReader reader) => reader.ValueType ? Read(reader) : ReadObject();

        public T Read<T>(TypeReader reader) => reader != null ? (T)reader.Type.GetMethod("Read").Invoke(reader, [this]) : default;
        public T ReadObject<T>() => Read<T>(ReadTypeId());
        public T ReadValueOrObject<T>(TypeReader<T> reader) => reader.ValueType ? reader.Read(this) : ReadObject<T>();

        public TypeReader ReadTypeId() { var typeId = this.ReadVInt7() - 1; return typeId >= 0 ? typeId < Readers.Length ? Readers[typeId] : throw new Exception("Invalid XNB file: typeId is out of range.") : null; }
        public ContentReader Validate(string type) { var reader = ReadTypeId(); return reader == null || reader.Target != type ? throw new Exception("Invalid XNB file: got an unexpected typeId.") : this; }

        public static TypeReader GetByName(string name, uint version) {
            var wanted = StripAssemblyVersion(name).Replace("Microsoft.Xna.Framework.Content.", "");
            if (TypeReaderMap.TryGetValue(wanted, out var reader)) return reader;
            var (genericName, genericArguments) = SplitGenericTypeName(wanted);
            if (genericName == null) return default;
            if (TypeReaderMap.TryGetValue(genericName, out reader) && reader is GenericReader factory) {
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
        public object Parameters = r.ReadObject<object>();
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
        public Texture2D Texture = r.ReadObject<Texture2D>();
        public Vector4<int>[] Glyphs = r.ReadObject<Vector4<int>[]>();
        public object Cropping = r.ReadObject<object>();
        public object Characters = r.ReadObject<object>();
        public int VerticalLinespacing = r.ReadInt32();
        public float HorizontalSpacing = r.ReadSingle();
        public object Kerning = r.ReadObject<object>();
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
        public int Duration = r.Validate<int>("System.Int32").ReadInt32();
    }

    public class Video(ContentReader r) {
        public string Filename = r.Validate<string>("System.String").ReadLV7UString();
        public int Duration = r.Validate<int>("System.Int32").ReadInt32();
        public int Width = r.Validate<int>("System.Int32").ReadInt32();
        public int Height = r.Validate<int>("System.Int32").ReadInt32();
        public float FramesPerSecond = r.Validate<float>("System.Single").ReadSingle();
        public SoundtrackType SoundtrackType = (SoundtrackType)r.Validate<int>("System.Int32").ReadInt32();
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

    public object[] Objs;

    public Binary_Xnb(BinaryReader r2) {
        var r = new ContentReader(r2.BaseStream);
        var h = r.ReadS<Header>();
        h.Validate(r);
        r.ReadTypeManifest();
        Objs = r.ReadFArray(z => r.ReadObject(), r.ReadVInt7() + 1);
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
