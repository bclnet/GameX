using GameX.Formats;
using OpenStack.Gfx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Xbox.Formats;

#region Binary_Xnb

public class Binary_Xnb : IHaveMetaInfo, IWriteToStream, IRedirected<object> {
    public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Xnb(r));
    object IRedirected<object>.Value => Obj;

    #region Type Reader

    public class TypeReader(Type type, Type typeRead, string name, string target, bool valueType = false) {
        public Type Type = type; public MethodInfo TypeRead = typeRead.GetMethod("Read_");
        public string Name = name;
        public string Target = target;
        public bool ValueType = valueType;
    }

    public class TypeReader<T>(string name, string target, Func<ContentReader, T> read, bool valueType = false) : TypeReader(typeof(T), typeof(TypeReader<T>), name, target, valueType) {
        public Func<ContentReader, T> Read = read; public T Read_(ContentReader r) => Read(r);
    }

    public class GenericReader(string name, string target, MethodInfo ginit, bool valueType = false) : TypeReader<object>(name, target, null, valueType) {
        public MethodInfo GInit = ginit;
    }

    public struct ResourceRef(ContentReader r) {
        public readonly uint Id = r.ReadVInt7();
        public readonly object Obj(Binary_Xnb p) => Id != 0 ? p.Resources[Id - 1] : null;
    }

    public class ContentReader(Stream input) : BinaryReader(input) {
        public static TypeReader _EnumReader<T>(TypeReader<T> value) => new TypeReader<int>(null, null, r => r.ReadInt32()); // target2: t
        public static TypeReader _NullableReader<T>(TypeReader<T> value) where T : struct => new TypeReader<T?>(null, null, r => r.ReadBoolean() ? r.Read<T?>(value) : default);
        public static TypeReader _ArrayReader<T>(TypeReader<T> value) => new TypeReader<T[]>(null, null, r => r.ReadL32FArray(z => r.ReadValueOrObject(value))); // target2: $"{t}[]"
        public static TypeReader _ListReader<T>(TypeReader<T> value) => new TypeReader<List<T>>(null, null, r => r.ReadL32FList(z => r.ReadValueOrObject(value)));
        public static TypeReader _DictionaryReader<TKey, TValue>(TypeReader<TKey> key, TypeReader<TValue> value) => new TypeReader<IDictionary<TKey, TValue>>(null, null, r => r.ReadL32FMany(z => r.ReadValueOrObject(key), z => r.ReadValueOrObject(value)));
        public static TypeReader _ReflectiveReader<T>(TypeReader<T> value) => new TypeReader<T>(null, null, r => throw new NotSupportedException()); // target2: t
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
            new GenericReader("EnumReader", "System.Enum", typeof(ContentReader).GetMethod("_EnumReader")),
            new GenericReader("NullableReader", "System.Nullable", typeof(ContentReader).GetMethod("_NullableReader"), valueType: true),
            new GenericReader("ArrayReader", "System.Array", typeof(ContentReader).GetMethod("_ArrayReader")),
            new GenericReader("ListReader", "System.Collections.Generic.List", typeof(ContentReader).GetMethod("_ListReader")),
            new GenericReader("DictionaryReader", "System.Collections.Generic.Dictionary", typeof(ContentReader).GetMethod("_DictionaryReader")),
            new TypeReader<TimeSpan>("TimeSpanReader", "System.TimeSpan", r => { var v = r.ReadInt64(); return new TimeSpan(v); }, valueType: true),
            new TypeReader<DateTime>("DateTimeReader", "System.DateTime", r => { var v = r.ReadInt64(); return new DateTime(v & ~(3L << 62), (DateTimeKind)(v >> 62)); }, valueType: true),
            new TypeReader<decimal>("DecimalReader", "System.Decimal", r => { uint a = r.ReadUInt32(), b = r.ReadUInt32(), c = r.ReadUInt32(), d = r.ReadUInt32(); return 0; }, valueType: true),
            new TypeReader<string>("ExternalReferenceReader", "ExternalReference", r => r.ReadString()),
            new GenericReader("ReflectiveReader", "System.Object", typeof(ContentReader).GetMethod("_ReflectiveReader")),

            // Math types
            new TypeReader<Vector2>("Vector2Reader", "Vector2", r => r.ReadVector2(), valueType: true),
            new TypeReader<Vector3>("Vector3Reader", "Vector3", r => r.ReadVector3(), valueType: true),
            new TypeReader<Vector4>("Vector4Reader", "Vector4", r => r.ReadVector4(), valueType: true),
            new TypeReader<Matrix4x4>("MatrixReader", "Matrix", r => r.ReadMatrix4x4(), valueType: true),
            new TypeReader<Quaternion>("QuaternionReader", "Quaternion", r => r.ReadQuaternion(), valueType: true),
            new TypeReader<ByteColor4>("ColorReader", "Color", r => new ByteColor4(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte()), valueType: true),
            new TypeReader<Plane>("PlaneReader", "Ray", r => new Plane(r.ReadVector3(), r.ReadSingle()), valueType: true),
            new TypeReader<Point>("PointReader", "Point", r => new Point(r.ReadInt32(), r.ReadInt32()), valueType: true),
            new TypeReader<Rectangle>("RectangleReader", "Rectangle", r => new Rectangle(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32()), valueType: true),
            new TypeReader<BoundingBox>("BoundingBoxReader", "BoundingBox", r => new BoundingBox(r.ReadVector3(), r.ReadVector3()), valueType: true),
            new TypeReader<BoundingSphere>("BoundingSphereReader", "BoundingSphere", r => new BoundingSphere(r.ReadVector3(), r.ReadSingle()), valueType: true),
            new TypeReader<Matrix4x4>("BoundingFrustumReader", "BoundingFrustum", r => r.ReadMatrix4x4()),
            new TypeReader<Ray>("RayReader", "Ray", r => new Ray(r.ReadVector3(), r.ReadVector3()), valueType: true),
            new TypeReader<Curve>("CurveReader", "Curve", r=> new Curve(r.ReadInt32(), r.ReadInt32(), r.ReadL32FArray(z => new Curve.Loop(z.ReadSingle(), z.ReadSingle(), z.ReadSingle(), z.ReadSingle(), z.ReadInt32())))),

            // Graphics types
            new TypeReader<object>("TextureReader", "Graphics.Texture", r => throw new NotSupportedException()),
            new TypeReader<Texture2D>("Texture2DReader", "Graphics.Texture2D", r => new Texture2D(r)),
            new TypeReader<Texture3D>("Texture3DReader", "Graphics.Texture3D", r => new Texture3D(r)),
            new TypeReader<TextureCube>("TextureCubeReader", "Graphics.TextureCube", r => new TextureCube(r)),
            new TypeReader<IndexBuffer>("IndexBufferReader", "Graphics.IndexBuffer", r => new IndexBuffer(r)),
            new TypeReader<VertexBuffer>("VertexBufferReader", "Graphics.VertexBuffer", r => new VertexBuffer(r)),
            new TypeReader<VertexDeclaration>("VertexDeclarationReader", "Graphics.VertexDeclaration", r => new VertexDeclaration(r)),
            new TypeReader<Effect>("EffectReader", "Graphics.Effect", r => new Effect(r)),
            new TypeReader<EffectMaterial>("EffectMaterialReader", "Graphics.EffectMaterial", r => new EffectMaterial(r)),
            new TypeReader<BasicEffect>("BasicEffectReader", "Graphics.BasicEffect", r => new BasicEffect(r)),
            new TypeReader<AlphaTestEffect>("AlphaTestEffectReader", "Graphics.AlphaTestEffect", r => new AlphaTestEffect(r)),
            new TypeReader<DualTextureEffect>("DualTextureEffectReader", "Graphics.DualTextureEffect", r => new DualTextureEffect(r)),
            new TypeReader<EnvironmentMapEffect>("EnvironmentMapEffectReader", "Graphics.EnvironmentMapEffect", r => new EnvironmentMapEffect(r)),
            new TypeReader<SkinnedEffect>("SkinnedEffectReader", "Graphics.SkinnedEffect", r => new SkinnedEffect(r)),
            new TypeReader<SpriteFont>("SpriteFontReader", "Graphics.SpriteFont", r => new SpriteFont(r)),
            new TypeReader<Model>("ModelReader", "Graphics.Model", r => new Model(r)),

            // Media types
            new TypeReader<SoundEffect>("SoundEffectReader", "Audio.SoundEffect", r => new SoundEffect(r)),
            new TypeReader<Song>("SongReader", "Media.Song", r => new Song(r)),
            new TypeReader<Video>("VideoReader", "Media.Video", r => new Video(r))
        ];

        readonly static Dictionary<string, TypeReader> TypeReaderMap = TypeReaders.ToDictionary(s => s.Name);
        TypeReader[] Readers;

        public static TypeReader Add(TypeReader reader) {
            TypeReaders.Add(reader);
            TypeReaderMap[reader.Name] = reader;
            return reader;
        }

        public void ReadTypeManifest() {
            Readers = this.ReadLV7FArray(z => GetByName(this.ReadLV7UString(), ReadUInt32()));
            //foreach (var s in Readers.Where(x => x.Init != null)) s.Init();
        }

        public object Read(TypeReader reader) => reader != null ? reader.TypeRead.Invoke(reader, [this]) : default;
        public object ReadObject() => Read(ReadTypeId());
        public object ReadValueOrObject(TypeReader reader) => reader.ValueType ? Read(reader) : ReadObject();
        public T Read<T>(TypeReader reader) => reader != null ? (T)reader.TypeRead.Invoke(reader, [this]) : default;
        public T ReadObject<T>() => Read<T>(ReadTypeId());
        public T ReadValueOrObject<T>(TypeReader<T> reader) => reader.ValueType ? reader.Read(this) : ReadObject<T>();
        public TypeReader ReadTypeId() { var id = (int)this.ReadVInt7() - 1; return id >= 0 ? id < Readers.Length ? Readers[id] : throw new Exception("Invalid XNB file: id is out of range.") : null; }
        public ContentReader Validate(string type) { var reader = ReadTypeId(); return reader != null && reader.Target == type ? this : throw new Exception("Invalid XNB file: got an unexpected id."); }
        public ResourceRef ReadResource() => new(this);

        public static TypeReader Create(GenericReader s, List<string> args) {
            TypeReader r;
            if (args.Count == 1) { var value = GetByTarget(args[0]); r = (TypeReader)s.GInit.MakeGenericMethod([value.Type]).Invoke(null, [value]); }
            else if (args.Count == 2) { var key = GetByTarget(args[0]); var value = GetByTarget(args[1]); r = (TypeReader)s.GInit.MakeGenericMethod([key.Type, value.Type]).Invoke(null, [key, value]); }
            else throw new Exception();
            var suffix = $"`{args.Count}[[{string.Join("],[", args)}]]";
            r.Name = s.Name + suffix;
            r.Target = s.Target + suffix;
            r.ValueType = s.ValueType;
            return r;
        }

        public static TypeReader GetByName(string name, uint version) {
            var wanted = StripAssemblyVersion(name).Replace("Microsoft.Xna.Framework.Content.", "");
            if (TypeReaderMap.TryGetValue(wanted, out var reader)) return reader;
            var (genericName, args) = SplitGenericTypeName(wanted);
            if (genericName != null && TypeReaderMap.TryGetValue(genericName, out reader) && reader is GenericReader generic) {
                reader = Create(generic, args);
                if (reader.Name != wanted) throw new Exception("ERROR");
                return Add(reader);
            }
            throw new Exception($"Cannot find codec value '{wanted}'.");
        }

        public static TypeReader GetByTarget(string target) {
            var wanted = StripAssemblyVersion(target).Replace("Microsoft.Xna.Framework.", "");
            var reader = TypeReaders.FirstOrDefault(x => x.Target == wanted);
            if (reader != null) return reader;
            throw new Exception($"Cannot find value for target codec '{wanted}'.");
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

    public class Texture2D(ContentReader r) : IHaveMetaInfo, ITexture {
        public readonly SurfaceFormat Format = (SurfaceFormat)r.ReadInt32();
        public uint Width = r.ReadUInt32();
        public uint Height = r.ReadUInt32();
        public readonly byte[][] Mips = r.ReadL32FArray(z => r.ReadL32Bytes());
        #region ITexture
        int ITexture.Width => (int)Width;
        int ITexture.Height => (int)Height;
        int ITexture.Depth => 0;
        int ITexture.MipMaps => Mips.Length;
        TextureFlags ITexture.TexFlags => 0;
        public T Create<T>(string platform, Func<object, T> func) {
            var buf = Mips[0];
            if (Mips.Length > 1) throw new NotSupportedException();
            var format = Format switch {
                SurfaceFormat.Color => (TextureFormat.RGBA32, TexturePixel.Unknown),
                SurfaceFormat.Bgr565 => (TextureFormat.RGB565, TexturePixel.Reversed),
                SurfaceFormat.Bgra5551 => (TextureFormat.BGRA1555, TexturePixel.Reversed),
                //SurfaceFormat.Bgra4444 => (TextureFormat.X, TexturePixel.Unknown),
                SurfaceFormat.Dxt1 => (TextureFormat.DXT1, TexturePixel.Unknown),
                SurfaceFormat.Dxt3 => (TextureFormat.DXT3, TexturePixel.Unknown),
                SurfaceFormat.Dxt5 => (TextureFormat.DXT5, TexturePixel.Unknown),
                //SurfaceFormat.NormalizedByte2 => (TextureFormat.X, TexturePixel.Unknown),
                //SurfaceFormat.NormalizedByte4 => (TextureFormat.X, TexturePixel.Unknown),
                //SurfaceFormat.Rgba1010102 => (TextureFormat.X, TexturePixel.Unknown),
                //SurfaceFormat.Rg32 => (TextureFormat.RG32, TexturePixel.Unknown),
                //SurfaceFormat.Rgba64 => (TextureFormat.RGBA32, TexturePixel.Unknown),
                //SurfaceFormat.Alpha8 => (TextureFormat.RGBA32, TexturePixel.Unknown),
                //SurfaceFormat.Single => (TextureFormat.RGBA32, TexturePixel.Unknown),
                //SurfaceFormat.Vector2 => (TextureFormat.RGBA32, TexturePixel.Unknown),
                //SurfaceFormat.Vector4 => (TextureFormat.RGBA32, TexturePixel.Unknown),
                //SurfaceFormat.HalfSingle => (TextureFormat.RGBA32, TexturePixel.Unknown),
                //SurfaceFormat.HalfVector2 => (TextureFormat.RGBA32, TexturePixel.Unknown),
                //SurfaceFormat.HalfVector4 => (TextureFormat.RGBA32, TexturePixel.Unknown),
                //SurfaceFormat.HdrBlendable => (TextureFormat.RGBA32, TexturePixel.Unknown),
                _ => throw new Exception($"Unknown Format: {Format}"),
            };
            return func(new Texture_Bytes(buf, format, null));
        }
        #endregion
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
            new(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new("Texture2D", items: [
                new($"Format: {Format}"),
                new($"Width: {Width}"),
                new($"Height: {Height}"),
                new($"Mips: {Mips.Length}")
            ])];
    }

    public class Texture3D(ContentReader r) : IHaveMetaInfo {
        public readonly SurfaceFormat Format = (SurfaceFormat)r.ReadInt32();
        public readonly uint Width = r.ReadUInt32();
        public readonly uint Height = r.ReadUInt32();
        public readonly uint Depth = r.ReadUInt32();
        public readonly byte[][] Mips = r.ReadL32FArray(z => r.ReadL32Bytes());
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
            new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
            new("Texture3D", items: [
                new($"Format: {Format}"),
                new($"Width: {Width}"),
                new($"Height: {Height}"),
                new($"Depth: {Depth}"),
                new($"Mips: {Mips.Length}")
            ])];
    }

    public class TextureCube(ContentReader r) : IHaveMetaInfo {
        public readonly SurfaceFormat Format = (SurfaceFormat)r.ReadInt32();
        public readonly uint Size = r.ReadUInt32();
        public readonly byte[][] Face1Mips = r.ReadL32FArray(z => r.ReadL32Bytes());
        public readonly byte[][] Face2Mips = r.ReadL32FArray(z => r.ReadL32Bytes());
        public readonly byte[][] Face3Mips = r.ReadL32FArray(z => r.ReadL32Bytes());
        public readonly byte[][] Face4Mips = r.ReadL32FArray(z => r.ReadL32Bytes());
        public readonly byte[][] Face5Mips = r.ReadL32FArray(z => r.ReadL32Bytes());
        public readonly byte[][] Face6Mips = r.ReadL32FArray(z => r.ReadL32Bytes());
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
            new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
            new("TextureCube", items: [
                new($"Format: {Format}"),
                new($"Size: {Size}")
            ])];
    }

    public class IndexBuffer(ContentReader r) {
        public readonly int IndexFormat = r.ReadBoolean() ? 16 : 32;
        public readonly byte[] IndexData = r.ReadL32Bytes();
    }

    public class VertexDeclaration(ContentReader r) {
        public class Element(ContentReader r) {
            public readonly uint Offset = r.ReadUInt32();
            public readonly VertexElementFormat Format = (VertexElementFormat)r.ReadInt32();
            public readonly VertexElementUsage Usage = (VertexElementUsage)r.ReadInt32();
            public readonly uint UsageIndex = r.ReadUInt32();
        }
        public readonly uint VertexStride = r.ReadUInt32();
        public readonly Element[] Elements = r.ReadL32FArray(z => new Element(r));
    }

    public class VertexBuffer : VertexDeclaration {
        public readonly byte[][] Vertexs;
        public VertexBuffer(ContentReader r) : base(r) {
            Vertexs = r.ReadL32FArray(z => r.ReadBytes(VertexStride));
        }
    }

    public class Effect(ContentReader r) {
        public readonly byte[] EffectBytecode = r.ReadL32Bytes();
    }

    public class EffectMaterial(ContentReader r) {
        public readonly string EffectReference = r.ReadLV7UString();
        public readonly object Parameters = r.ReadObject<object>();
    }

    public class BasicEffect(ContentReader r) {
        public readonly string TextureReference = r.ReadLV7UString();
        public readonly Vector3 DiffuseColor = r.ReadVector3();
        public readonly Vector3 EmissiveColor = r.ReadVector3();
        public readonly Vector3 SpecularColor = r.ReadVector3();
        public readonly float SpecularPower = r.ReadSingle();
        public readonly float Alpha = r.ReadSingle();
        public readonly bool VertexColorEnabled = r.ReadBoolean();
    }

    public class AlphaTestEffect(ContentReader r) {
        public readonly string TextureReference = r.ReadLV7UString();
        public readonly CompareFunction CompareFunction = (CompareFunction)r.ReadInt32();
        public readonly uint ReferenceAlpha = r.ReadUInt32();
        public readonly Vector3 DiffuseColor = r.ReadVector3();
        public readonly float Alpha = r.ReadSingle();
        public readonly bool VertexColorEnabled = r.ReadBoolean();
    }

    public class DualTextureEffect(ContentReader r) {
        public readonly string Texture1Reference = r.ReadLV7UString();
        public readonly string Texture2Reference = r.ReadLV7UString();
        public readonly Vector3 DiffuseColor = r.ReadVector3();
        public readonly float Alpha = r.ReadSingle();
        public readonly bool VertexColorEnabled = r.ReadBoolean();
    }

    public class EnvironmentMapEffect(ContentReader r) {
        public readonly string TextureReference = r.ReadLV7UString();
        public readonly string EnvironmentMapReference = r.ReadLV7UString();
        public readonly float EnvironmentMapAmount = r.ReadSingle();
        public readonly Vector3 EnvironmentMapSpecular = r.ReadVector3();
        public readonly float FresnelFactor = r.ReadSingle();
        public readonly Vector3 DiffuseColor = r.ReadVector3();
        public readonly Vector3 EmissiveColor = r.ReadVector3();
        public readonly float Alpha = r.ReadSingle();
    }

    public class SkinnedEffect(ContentReader r) {
        public readonly string TextureReference = r.ReadLV7UString();
        public readonly uint WeightsPerVertex = r.ReadUInt32();
        public readonly Vector3 DiffuseColor = r.ReadVector3();
        public readonly Vector3 EmissiveColor = r.ReadVector3();
        public readonly Vector3 SpecularColor = r.ReadVector3();
        public readonly float SpecularPower = r.ReadSingle();
        public readonly float Alpha = r.ReadSingle();
    }

    public class SpriteFont(ContentReader r) : IHaveMetaInfo {
        public readonly Texture2D Texture = r.ReadObject<Texture2D>();
        public readonly List<Rectangle> Glyphs = r.ReadObject<List<Rectangle>>();
        public readonly List<Rectangle> Cropping = r.ReadObject<List<Rectangle>>();
        public readonly string Characters = new([.. r.ReadObject<List<char>>()]);
        public readonly int VerticalLineSpacing = r.ReadInt32();
        public readonly float HorizontalSpacing = r.ReadSingle();
        public readonly List<Vector3> Kerning = r.ReadObject<List<Vector3>>();
        public readonly char? DefaultCharacter = r.ReadBoolean() ? r.ReadChar() : null;
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
            new(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = Texture }),
            new("SpriteFont", items: [
                new($"Format: {Texture.Format}"),
                new($"Width: {Texture.Width}"),
                new($"Height: {Texture.Height}")
            ])];

        public Vector2 MeasureString(ReadOnlySpan<char> text) {
            if (text == null) throw new ArgumentNullException("text");
            if (text.Length == 0) return Vector2.Zero;
            var res = Vector2.Zero;
            float curLineWidth = 0.0f, finalLineHeight = VerticalLineSpacing;
            var firstInLine = true;
            foreach (char c in text) {
                // special characters
                if (c == '\r') continue;
                if (c == '\n') {
                    res.X = Math.Max(res.X, curLineWidth);
                    res.Y += VerticalLineSpacing;
                    curLineWidth = 0.0f;
                    finalLineHeight = VerticalLineSpacing;
                    firstInLine = true;
                    continue;
                }

                // get the List index from the character map, defaulting to the DefaultCharacter if it's set.
                var index = Characters.IndexOf(c);
                if (index == -1) index = Characters.IndexOf(DefaultCharacter ?? '?');

                // for the first character in a line, always push the width rightward, even if the kerning pushes the character to the left.
                var kern = Kerning[index];
                if (firstInLine) { curLineWidth += Math.Abs(kern.X); firstInLine = false; }
                else curLineWidth += HorizontalSpacing + kern.X;

                // add the character width and right-side bearing to the line width.
                curLineWidth += kern.Y + kern.Z;

                // if a character is taller than the default line height, increase the height to that of the line's tallest character.
                var cropHeight = Cropping[index].Height;
                if (cropHeight > finalLineHeight) finalLineHeight = cropHeight;
            }

            // calculate the final width/height of the text box
            res.X = Math.Max(res.X, curLineWidth);
            res.Y += finalLineHeight;
            return res;
        }
    }

    public class Model : IHaveMetaInfo {
        public struct BoneRef(Model p, ContentReader r) {
            public readonly uint Id = p.Bones8 ? r.ReadByte() : r.ReadUInt32();
            public readonly Bone Bone => Id != 0 ? p.Bones[Id - 1] : null;
        }
        public class Bone(ContentReader r) {
            public readonly string Name = r.ReadObject<string>();
            public readonly Matrix4x4 Transform = r.ReadMatrix4x4();
            public BoneRef Parent;
            public BoneRef[] Children;
        }
        public class MeshPart(ContentReader r) {
            public readonly int VertexOffset = r.ReadInt32();
            public readonly int NumVertices = r.ReadInt32();
            public readonly int StartIndex = r.ReadInt32();
            public readonly int PrimitiveCount = r.ReadInt32();
            public readonly object Tag = r.ReadObject();
            public readonly ResourceRef VertexBuffer = r.ReadResource();
            public readonly ResourceRef IndexBuffer = r.ReadResource();
            public readonly ResourceRef Effect = r.ReadResource();
        }
        public class Mesh(Model p, ContentReader r) {
            public readonly string Name = r.ReadObject<string>();
            public readonly BoneRef Parent = new(p, r);
            public readonly BoundingSphere Bounds = new(r.ReadVector3(), r.ReadSingle());
            public readonly object Tag = r.ReadObject();
            public readonly MeshPart[] Parts = r.ReadL32FArray(z => new MeshPart(r));
        }
        public readonly Bone[] Bones; readonly bool Bones8;
        public readonly Mesh[] Meshs;
        public readonly BoneRef Root;
        public readonly object Tag;
        public Model(ContentReader r) {
            Bones = r.ReadL32FArray(z => new Bone(r)); Bones8 = Bones.Length < 255;
            foreach (var s in Bones) { s.Parent = new BoneRef(this, r); s.Children = r.ReadL32FArray(z => new BoneRef(this, r)); }
            Meshs = r.ReadL32FArray(z => new Mesh(this, r));
            Root = new BoneRef(this, r);
            Tag = r.ReadObject();
        }
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
            new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
            new("Model", items: [
                new($"Bones: {Bones.Length}"),
                new($"Meshs: {Meshs.Length}"),
                new($"Root: {Root.Bone.Name}"),
                new($"Tag: {Tag}")
            ])];
    }

    #endregion

    #region Media Objects

    public enum SoundtrackType {
        Music,
        Dialog,
        MusicDialog
    }

    public class SoundEffect(ContentReader r) : IHaveMetaInfo {
        public readonly byte[] Format = r.ReadL32Bytes();
        public readonly byte[] Data = r.ReadL32Bytes();
        public readonly int LoopStart = r.ReadInt32();
        public readonly int LoopLength = r.ReadInt32();
        public readonly int Duration = r.ReadInt32();
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
            new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
            new("SoundEffect", items: [
                new($"Duration: {Duration}")
            ])];
    }

    public class Song(ContentReader r) : IHaveMetaInfo {
        public readonly string Filename = r.ReadLV7UString();
        public readonly int Duration = r.Validate("System.Int32").ReadInt32();
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
            new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
            new("Texture", items: [
                new($"Filename: {Filename}"),
                new($"Duration: {Duration}")
            ])];
    }

    public class Video(ContentReader r) : IHaveMetaInfo {
        public readonly string Filename = r.Validate("System.String").ReadLV7UString();
        public readonly int Duration = r.Validate("System.Int32").ReadInt32();
        public readonly int Width = r.Validate("System.Int32").ReadInt32();
        public readonly int Height = r.Validate("System.Int32").ReadInt32();
        public readonly float FramesPerSecond = r.Validate("System.Single").ReadSingle();
        public readonly SoundtrackType SoundtrackType = (SoundtrackType)r.Validate("System.Int32").ReadInt32();
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
            new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
            new("Video", items: [
                new($"Filename: {Filename}"),
                new($"Duration: {Duration}"),
                new($"Width: {Width}"),
                new($"Height: {Height}")
            ])];
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

        public ContentReader Validate(BinaryReader r) {
            if ((Magic & 0x00FFFFFF) != MAGIC) throw new Exception("BAD MAGIC");
            if (Version != 5 && Version != 4) throw new Exception("Invalid XNB version");
            if (SizeOnDisk > r.BaseStream.Length) throw new Exception("XNB file has been truncated.");
            if (Compressed) {
                uint decompressedSize = r.ReadUInt32(), compressedSize = SizeOnDisk - (uint)r.Tell();
                var b = r.DecompressXmem((int)compressedSize, (int)decompressedSize);
                return new ContentReader(new MemoryStream(b));
            }
            return new ContentReader(r.BaseStream);
        }
    }

    #endregion

    public object Obj;
    public object[] Resources;

    public Binary_Xnb(BinaryReader r2) {
        var h = r2.ReadS<Header>();
        var r = h.Validate(r2);
        r.ReadTypeManifest();
        var resourceCount = r.ReadVInt7();
        Obj = r.ReadObject();
        Resources = r.ReadFArray(z => r.ReadObject(), resourceCount);
        //r.EnsureAtEnd(); // h.SizeOnDisk
    }

    public void WriteToStream(Stream stream) => Obj.Serialize(stream);

    public override string ToString() => Obj.Serialize();

    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => (Obj as IHaveMetaInfo)?.GetInfoNodes(resource, file, tag) ?? [
        new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
        new("Xnb", items: [
            new($"Obj: {Obj}")
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
