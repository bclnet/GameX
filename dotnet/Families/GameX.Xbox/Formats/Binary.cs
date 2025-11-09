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
                throw new Exception("Don't support reading the contents of compressed XNB files.");
            }
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

    public class BasicEffect(ContentReader r) {
        public string TextureReference = r.ReadLV7UString();
        public Vector3 DiffuseColor = r.ReadVector3();
        public Vector3 EmissiveColor = r.ReadVector3();
        public Vector3 SpecularColor = r.ReadVector3();
        public float SpecularPower = r.ReadSingle();
        public float Alpha = r.ReadSingle();
        public bool VertexColorEnabled = r.ReadBool8();
    }

    public class AlphaTestEffect(ContentReader r) {
        public string TextureReference = r.ReadLV7UString();
        public CompareFunction CompareFunction = (CompareFunction)r.ReadInt32();
        public uint ReferenceAlpha = r.ReadUInt32();
        public Vector3 DiffuseColor = r.ReadVector3();
        public float Alpha = r.ReadSingle();
        public bool VertexColorEnabled = r.ReadBool8();
    }

    public class DualTextureEffect(ContentReader r) {
    }

    public class EnvironmentMapEffect(ContentReader r) {
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
        public object Texture = r.ReadObject();
        public object Glyphs = r.ReadObject();
        public object Cropping = r.ReadObject();
        public object Characters = r.ReadObject();
        public int VerticalLinespacing = r.ReadInt32();
        public float HorizontalSpacing = r.ReadSingle();
        public object Kerning = r.ReadObject();
        public char DefaultCharacter = r.ReadBool8() ? r.ReadChar() : (char)0;
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

    #region TypeReader

    public class ContentReader(Stream input) : BinaryReader(input) {
        public object ReadObject() => ReadTypeId()?.Read(this);
        public object ReadValueOrObject(TypeReader reader) => reader.IsValueType ? reader.Read(this) : ReadObject();
        public TypeReader ReadTypeId() { var typeId = (int)this.ReadVInt7(); return typeId > 0 ? typeId <= TypeReaders.Count ? TypeReaders[typeId - 1] : throw new Exception("Invalid XNB file: typeId is out of range.") : null; }
        public ContentReader Validate(string type) { var reader = ReadTypeId(); return reader == null || reader.Target != type ? throw new Exception("Invalid XNB file: got an unexpected typeId.") : this; }

        readonly static List<TypeReader> TypeReaders = [
            // Primitive types
            new TypeReader("ByteReader", "System.Byte", r => r.ReadByte(), isValueType: true),
            new TypeReader("SByteReader", "System.SByte", r => r.ReadSByte(), isValueType: true),
            new TypeReader("Int16Reader", "System.Int16", r => r.ReadInt16(), isValueType: true),
            new TypeReader("UInt16Reader", "System.UInt16", r => r.ReadUInt16(), isValueType: true),
            new TypeReader("Int32Reader", "System.Int32", r => r.ReadInt32(), isValueType: true),
            new TypeReader("UInt32Reader", "System.UInt32", r => r.ReadUInt32(), isValueType: true),
            new TypeReader("Int64Reader", "System.Int64", r => r.ReadInt64(), isValueType: true),
            new TypeReader("UInt64Reader", "System.UInt64", r => r.ReadUInt64(), isValueType: true),
            new TypeReader("SingleReader", "System.Single", r => r.ReadSingle(), isValueType: true),
            new TypeReader("DoubleReader", "System.Double", r => r.ReadDouble(), isValueType: true),
            new TypeReader("BooleanReader", "System.Boolean", r => r.ReadBoolean(), isValueType: true),
            new TypeReader("CharReader", "System.Char", r => r.ReadChar(), isValueType: true),
            new TypeReader("StringReader", "System.String", r => r.ReadString()),
            new TypeReader("ObjectReader", "System.Object", r => throw new NotSupportedException()),

            // System types
            new GenericReader("EnumReader", "System.Enum", r => r.ReadInt32(), targetType: x => x.GenericArgument(0)),
            new GenericReader("NullableReader", "System.Nullable", r => r.ReadByte(), isValueType: true),
            new GenericReader("ArrayReader", "System.Array", r => r.ReadByte(), targetType: x => x.GenericArgument(0) + "[]"),
            new GenericReader("ListReader", "System.Collections.Generic.List", r => r.ReadByte()),
            new GenericReader("DictionaryReader", "System.Collections.Generic.Dictionary", r => r.ReadByte()),
            new TypeReader("TimeSpanReader", "System.TimeSpan", r => { var v = r.ReadInt64(); return new TimeSpan(v); }, isValueType: true),
            new TypeReader("DateTimeReader", "System.DateTime", r => { var v = r.ReadInt64(); return new DateTime(v & ~(3L << 62), (DateTimeKind)(v >> 62)); }, isValueType: true),
            new TypeReader("DecimalReader", "System.Decimal", r => { uint a = r.ReadUInt32(), b = r.ReadUInt32(), c = r.ReadUInt32(), d = r.ReadUInt32(); return 0; }, isValueType: true),
            new TypeReader("ExternalReferenceReader", "ExternalReference", r => r.ReadString()),
            new GenericReader("ReflectiveReader", "System.Object", r => throw new NotSupportedException(), targetType: x => x.GenericArgument(0)),

            // Math types
            new TypeReader("Vector2Reader", "System.Numerics.Vector2", r => r.ReadVector2(), isValueType: true),
            new TypeReader("Vector3Reader", "System.Numerics.Vector3", r => r.ReadVector3(), isValueType: true),
            new TypeReader("Vector4Reader", "System.Numerics.Vector4", r => r.ReadVector4(), isValueType: true),
            new TypeReader("MatrixReader", "System.Numerics.Matrix4x4", r => r.ReadMatrix4x4(), isValueType: true),
            new TypeReader("QuaternionReader", "System.Numerics.Quaternion", r => r.ReadQuaternion(), isValueType: true),
            new TypeReader("ColorReader", "Framework.Color", r => (r: r.ReadByte(), g: r.ReadByte(), b: r.ReadByte(), a: r.ReadByte()), isValueType: true),
            new TypeReader("PlaneReader", "Framework.Plane", r => (normal: r.ReadVector3(), d: r.ReadSingle()), isValueType: true),
            new TypeReader("PointReader", "Framework.Point", r => (x: r.ReadInt32(), y: r.ReadInt32()), isValueType: true),
            new TypeReader("RectangleReader", "Framework.Rectangle", r => (x: r.ReadInt32(), y: r.ReadInt32(), width: r.ReadInt32(), height: r.ReadInt32()), isValueType: true),
            new TypeReader("BoundingBoxReader", "Framework.BoundingBox", r => (min: r.ReadVector3(), max: r.ReadVector3()), isValueType: true),
            new TypeReader("BoundingSphereReader", "Framework.BoundingSphere", r => (center: r.ReadVector3(), radius: r.ReadSingle()), isValueType: true),
            new TypeReader("BoundingFrustumReader", "Framework.BoundingFrustum", r => r.ReadMatrix4x4()),
            new TypeReader("RayReader", "Framework.Ray", r => (position: r.ReadVector3(), direction: r.ReadVector3()), isValueType: true),
            new TypeReader("CurveReader", "Framework.Curve", r => {
                var preLoop = r.ReadInt32();
                var postLoop = r.ReadInt32();
                var loops = r.ReadL32FArray(z => (position: z.ReadSingle(), value: z.ReadSingle(), tangentIn: z.ReadSingle(), tangentOut: z.ReadSingle(), continuity: z.ReadInt32()));
                return (preLoop, postLoop, loops);
            }),

            // Graphics types
            new TypeReader("TextureReader", "Framework.Graphics.Texture", r => throw new NotSupportedException()),
            new TypeReader("Texture2DReader", "Framework.Graphics.Texture2D", r => r.ReadByte()),
            new TypeReader("Texture3DReader", "Framework.Graphics.Texture3D", r => r.ReadByte()),
            new TypeReader("TextureCubeReader", "Framework.Graphics.TextureCube", r => r.ReadByte()),
            new TypeReader("IndexBufferReader", "Framework.Graphics.IndexBuffer", r => r.ReadByte()),
            new TypeReader("VertexBufferReader", "Framework.Graphics.VertexBuffer", r => r.ReadByte()),
            new TypeReader("VertexDeclarationReader", "Framework.Graphics.VertexDeclaration", r => r.ReadByte()),
            new TypeReader("EffectReader", "Framework.Graphics.Effect", r => r.ReadByte()),
            new TypeReader("EffectMaterialReader", "Framework.Graphics.EffectMaterial", r => r.ReadByte()),
            new TypeReader("BasicEffectReader", "Framework.Graphics.BasicEffect", r => new BasicEffect(r)),
            new TypeReader("AlphaTestEffectReader", "Framework.Graphics.AlphaTestEffect", r => new AlphaTestEffect(r)),
            new TypeReader("DualTextureEffectReader", "Framework.Graphics.DualTextureEffect", r => new DualTextureEffect(r)),
            new TypeReader("EnvironmentMapEffectReader", "Framework.Graphics.EnvironmentMapEffect", r => new EnvironmentMapEffect(r)),
            new TypeReader("SkinnedEffectReader", "Framework.Graphics.SkinnedEffect", r => new SkinnedEffect(r)),
            new TypeReader("SpriteFontReader", "Framework.Graphics.SpriteFont", r => new SpriteFont(r)),
            new TypeReader("ModelReader", "Framework.Graphics.Model", r => new Model(r)),

            // Media types
            new TypeReader("SoundEffectReader", "Audio.SoundEffect", r => new SoundEffect(r)),
            new TypeReader("SongReader", "Media.Song", r => new Song(r)),
            new TypeReader("VideoReader", "Media.Video", r => new Video(r))
        ];

        static Dictionary<string, TypeReader> TypeReaderMap;

        public static TypeReader Get(string name, uint version) {
            var wanted = StripAssemblyVersion(name).Replace("Microsoft.Xna.Framework.Content.", "");
            TypeReaderMap ??= TypeReaders.ToDictionary(x => x.Name);
            if (TypeReaderMap.TryGetValue(wanted, out var reader)) return reader;
            // Could this be a specialization of a generic reader?
            var (genericName, genericArguments) = SplitGenericTypeName(wanted);
            if (genericName == null) return default;
            // Look for a generic reader factory with this name.
            if (TypeReaderMap.TryGetValue(genericName, out reader) && reader is GenericReader factory) {
                // Create a specialized generic reader instance.
                reader = factory.Create(genericArguments);
                TypeReaders.Add(reader); TypeReaderMap = null;
                return reader;
            }
            throw new Exception($"Can't find type reader '{wanted}'.");
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
            // Look for the ` generic marker character.
            var pos = name.IndexOf('`');
            if (pos == -1) return default;

            // Everything to the left of ` is the generic type name.
            var genericName = name[..pos]; var genericArguments = new List<string>();

            // Advance to the start of the generic argument list.
            pos++;
            while (pos < name.Length && char.IsDigit(name[pos])) pos++;
            while (pos < name.Length && name[pos] == '[') pos++;

            // Split up the list of generic type arguments.
            while (pos < name.Length && name[pos] != ']') {
                // Locate the end of the current type name argument.
                int nesting = 0, end;
                for (end = pos; end < name.Length; end++) {
                    // Handle nested types in case we have eg. "List`1[[List`1[[Int]]]]".
                    if (name[end] == '[') nesting++;
                    else if (name[end] == ']') {
                        if (nesting > 0) nesting--;
                        else break;
                    }
                }

                // Extract the type name argument.
                genericArguments.Add(name[pos..end]);

                // Skip past the type name, plus any subsequent "],[" goo.
                pos = end;
                if (pos < name.Length && name[pos] == ']') pos++;
                if (pos < name.Length && name[pos] == ',') pos++;
                if (pos < name.Length && name[pos] == '[') pos++;
            }

            return (genericName, genericArguments);
        }
    }

    public class TypeReader(string name, string target, Func<ContentReader, object> read, Action<TypeReader> init = null, bool isValueType = false) {
        public string Name = name;
        public string Target = target;
        public Func<ContentReader, object> Read = read;
        public Action<TypeReader> Init = init;
        public bool IsValueType = isValueType;
    }

    public class GenericReader(string name, string target, Func<ContentReader, object> func, Action<TypeReader> init = null, bool isValueType = false, Func<GenericReader, object> targetType = null) : TypeReader(name, target, func, init, isValueType) {
        public Func<GenericReader, object> TargetType = targetType;
        public List<string> GenericArguments;
        public string GenericArgument(int i) => GenericArguments[i];

        public TypeReader Create(List<string> args) {
            return null;
        }
    }

    #endregion

    public Binary_Xnb(BinaryReader r2) {
        // header
        var r = new ContentReader(r2.BaseStream);
        var h = r.ReadS<Header>();
        h.Validate(r);
        var endPosition = h.SizeOnDisk;

        // type-manifest
        var types = r.ReadLV7FArray(z => ContentReader.Get(z.ReadLV7UString(), z.ReadUInt32()));
        foreach (var s in types) s.Init(s);

        // objects
        var objs = new object[r.ReadVInt7() + 1];
        for (var i = 0; i < objs.Length; i++) {
            objs[i] = r.ReadObject();
        }

        r.EnsureComplete(endPosition);
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
