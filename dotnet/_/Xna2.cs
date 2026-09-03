using OpenStack.Gfx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace GameX.Xbox.Formats.Xna;

#region Type Manager

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class OptionalAttribute : Attribute { public bool SharedResource = false; }

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class IgnoreAttribute : Attribute { }

public abstract class TypeReader(Type t, string name, string type, bool valueType = false, Action<TypeReader> init = null) {
    public Type T = t;
    public string Name = name;
    public string Type = type;
    public bool ValueType = valueType;
    public bool CanUseObj = false;
    public Action<TypeReader> Init = init;
    public abstract object Read(ContentReader r, object o);
}

public class TypeReader<T>(string name, string type, Func<ContentReader, T, T> read, bool valueType = false, Action<TypeReader> init = null) : TypeReader(typeof(T), name, type, valueType, init) {
    public Func<ContentReader, T, T> ReadFunc = read;
    public override object Read(ContentReader r, object o) => Read(r, (T)o);
    public virtual T Read(ContentReader r, T o) => ReadFunc(r, o);
}

public class GenericReader(string name, string type, MethodInfo ginit) : TypeReader<object>(name, type, null, false, null) {
    public MethodInfo GInit = ginit;
}

public class TypeManager {
    public static TypeReader _MakeReader<T>(string type) where T : new() => new TypeReader<T>(null, type, (r, o) => new());
    public static TypeReader _EnumReader<T>(TypeReader elem) => new TypeReader<T>(null, null, (r, o) => (T)elem.Read(r, o), init: s => { elem = GetByType(typeof(T).GetElementType()); }, valueType: true); //#t
    public static TypeReader _NullableReader<T>(TypeReader elem) where T : struct => new TypeReader<T?>(null, null, (r, o) => r.ReadBoolean() ? (T)elem.Read(r, o) : default, init: s => { elem = GetByType(typeof(T)); }, valueType: true);
    public static TypeReader _ArrayReader<T>(TypeReader elem) => new TypeReader<T[]>(null, null, (r, o) => r.ReadL32FArray(z => r.Read<T>(elem), obj: o), init: s => { elem = GetByType(typeof(T)); }); //#$"{t}[]"
    public static TypeReader _ListReader<T>(TypeReader elem) => new TypeReader<List<T>>(null, null, (r, o) => r.ReadL32FList(z => r.Read<T>(elem), obj: o), init: s => { elem = GetByType(typeof(T)); });
    public static TypeReader _DictionaryReader<TKey, TValue>(TypeReader key, TypeReader value) => new TypeReader<Dictionary<TKey, TValue>>(null, null, (r, o) => (Dictionary<TKey, TValue>)r.ReadL32FMany(z => r.Read<TKey>(key), z => r.Read<TValue>(value), obj: o), init: s => { key = GetByType(typeof(TKey)); value = GetByType(typeof(TValue)); });
    public static TypeReader _ReflectiveReader<T>(TypeReader elem) {
        var type = typeof(T);
        var baseType = type.BaseType;
        var baseReader = baseType != null && baseType != typeof(object) ? GetByType(baseType) : null;
        var constructor = Reflect.GetDefaultConstructor(type);
        var (properties, fields) = Reflect.GetAllPropertiesFields(type);
        var readers = new List<Action<ContentReader, object>>(fields.Length + properties.Length);
        foreach (var property in properties) { Action<ContentReader, object> v; if ((v = GetMemberValue(property)) != null) readers.Add(v); }
        foreach (var field in fields) { Action<ContentReader, object> v; if ((v = GetMemberValue(field)) != null) readers.Add(v); }
        return new TypeReader<T>(null, null, (r, o) => {
            var obj = o ?? (constructor == null ? (T)Activator.CreateInstance(typeof(T)) : (T)constructor.Invoke(null));
            if (baseReader != null) r.Read(baseReader, obj);
            var boxed = (object)obj; foreach (var v in readers) v(r, boxed); obj = (T)boxed;
            return obj;
        });
    }
    public static Action<ContentReader, object> GetMemberValue(MemberInfo member) {
        var (property, field) = (member as PropertyInfo, member as FieldInfo);
        if (property != null && (!property.CanRead || property.GetIndexParameters().Any())) return null;
        // ignore
        if (Attribute.GetCustomAttribute(member, typeof(IgnoreAttribute)) != null) return null;
        // optional
        var optional = (OptionalAttribute)Attribute.GetCustomAttribute(member, typeof(OptionalAttribute));
        if (optional == null) {
            if (property != null) {
                if (!property.GetGetMethod().IsPublic) return null;
                if (!property.CanWrite) {
                    var typeReader = GetByType(property.PropertyType);
                    //if (typeReader == null || !typeReader.CanDeserializeIntoExistingObject) return null;
                    throw new Exception($"CanWrite {typeReader}");
                }
            }
            else if (!field.IsPublic || field.IsInitOnly) return null;
        }
        // setter
        Action<object, object> setter; Type elementType;
        if (property != null) { elementType = property.PropertyType; setter = property.CanWrite ? (o, v) => property.SetValue(o, v, null) : (o, v) => { }; }
        else { elementType = field.FieldType; setter = field.SetValue; }
        // resources get special treatment.
        if (optional != null && optional.SharedResource) return (r, o) => r.ReadSharedResource<object>(v => setter(o, v));
        // we need to have a reader at this point.
        var reader = GetByType(elementType) ?? GetByType("Array") ?? throw new Exception($"Content reader could not be found for {elementType.FullName} T.");
        // we use the construct delegate to pick the correct existing object to be the target of deserialization.
        Func<object, object> construct = property != null && !property.CanWrite ? parent => property.GetValue(parent, null) : parent => null;
        return (r, o) => setter(o, r.Read(reader, construct(o)));
    }
    internal readonly static List<TypeReader> Readers = [
        // Primitive types
        new TypeReader<byte>("ByteReader", "Byte", (r, o) => r.ReadByte(), valueType: true),
            new TypeReader<sbyte>("SByteReader", "SByte", (r, o) => r.ReadSByte(), valueType: true),
            new TypeReader<short>("Int16Reader", "Int16", (r, o) => r.ReadInt16(), valueType: true),
            new TypeReader<ushort>("UInt16Reader", "UInt16", (r, o) => r.ReadUInt16(), valueType: true),
            new TypeReader<int>("Int32Reader", "Int32", (r, o) => r.ReadInt32(), valueType: true),
            new TypeReader<uint>("UInt32Reader", "UInt32", (r, o) => r.ReadUInt32(), valueType: true),
            new TypeReader<long>("Int64Reader", "Int64", (r, o) => r.ReadInt64(), valueType: true),
            new TypeReader<ulong>("UInt64Reader", "UInt64", (r, o) => r.ReadUInt64(), valueType: true),
            new TypeReader<float>("SingleReader", "Single", (r, o) => r.ReadSingle(), valueType: true),
            new TypeReader<double>("DoubleReader", "Double", (r, o) => r.ReadDouble(), valueType: true),
            new TypeReader<bool>("BooleanReader", "Boolean", (r, o) => r.ReadBoolean(), valueType: true),
            new TypeReader<char>("CharReader", "Char", (r, o) => r.ReadChar(), valueType: true),
            new TypeReader<string>("StringReader", "String", (r, o) => r.ReadLV7UString()),
            new TypeReader<object>("ObjectReader", "Object", (r, o) => throw new NotSupportedException()),
            // System types
            new GenericReader("EnumReader", "Enum", typeof(TypeManager).GetMethod("_EnumReader")),
            new GenericReader("NullableReader", "Nullable", typeof(TypeManager).GetMethod("_NullableReader")),
            new GenericReader("ArrayReader", "Array", typeof(TypeManager).GetMethod("_ArrayReader")),
            new GenericReader("ListReader", "Collections.Generic.List", typeof(TypeManager).GetMethod("_ListReader")),
            new GenericReader("DictionaryReader", "Collections.Generic.Dictionary", typeof(TypeManager).GetMethod("_DictionaryReader")),
            //new GenericReader("MultiArrayReader", null, typeof(TypeManager).GetMethod("_MultiArrayReader")),
            new TypeReader<TimeSpan>("TimeSpanReader", "TimeSpan", (r, o) => new TimeSpan(r.ReadInt64()), valueType: true),
            new TypeReader<DateTime>("DateTimeReader", "DateTime", (r, o) => { var v = r.ReadUInt64(); return new DateTime((long)(v & ~(3UL << 62)), (DateTimeKind)((v >> 62) & 3)); }, valueType: true),
            new TypeReader<decimal>("DecimalReader", "Decimal", (r, o) => r.ReadDecimal(), valueType: true),
            new TypeReader<string>("ExternalReferenceReader", "ExternalReference", (r, o) => r.ReadString()),
            new GenericReader("ReflectiveReader", "Object", typeof(TypeManager).GetMethod("_ReflectiveReader")),
            // Math types
            new TypeReader<Vector2>("Vector2Reader", "Vector2", (r, o) => r.ReadVector2(), valueType: true),
            new TypeReader<Vector3>("Vector3Reader", "Vector3", (r, o) => r.ReadVector3(), valueType: true),
            new TypeReader<Vector4>("Vector4Reader", "Vector4", (r, o) => r.ReadVector4(), valueType: true),
            new TypeReader<Matrix4x4>("MatrixReader", "Matrix", (r, o) => r.ReadMatrix4x4(), valueType: true),
            new TypeReader<Quaternion>("QuaternionReader", "Quaternion", (r, o) => r.ReadQuaternion(), valueType: true),
            new TypeReader<ByteColor4>("ColorReader", "Color", (r, o) => new ByteColor4(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte()), valueType: true),
            new TypeReader<Plane>("PlaneReader", "Ray", (r, o) => new Plane(r.ReadVector3(), r.ReadSingle()), valueType: true),
            new TypeReader<Point>("PointReader", "Point", (r, o) => new Point(r.ReadInt32(), r.ReadInt32()), valueType: true),
            new TypeReader<Rectangle>("RectangleReader", "Rectangle", (r, o) => new Rectangle(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32()), valueType: true),
            new TypeReader<BoundingBox>("BoundingBoxReader", "BoundingBox", (r, o) => new BoundingBox(r.ReadVector3(), r.ReadVector3()), valueType: true),
            new TypeReader<BoundingSphere>("BoundingSphereReader", "BoundingSphere", (r, o) => new BoundingSphere(r.ReadVector3(), r.ReadSingle()), valueType: true),
            new TypeReader<BoundingFrustum>("BoundingFrustumReader", "BoundingFrustum", (r, o) => new BoundingFrustum(r.ReadMatrix4x4())),
            new TypeReader<Ray>("RayReader", "Ray", (r, o) => new Ray(r.ReadVector3(), r.ReadVector3()), valueType: true),
            new TypeReader<Curve>("CurveReader", "Curve", (r, o) => new Curve(r.ReadInt32(), r.ReadInt32(), r.ReadL32FArray(z => new Curve.Key(z.ReadSingle(), z.ReadSingle(), z.ReadSingle(), z.ReadSingle(), z.ReadInt32())))),
            // Graphics types
            new TypeReader<object>("TextureReader", "Graphics.Texture", (r, o) => throw new NotSupportedException()),
            new TypeReader<Texture2D>("Texture2DReader", "Graphics.Texture2D", (r, o) => new Texture2D(r)),
            new TypeReader<Texture3D>("Texture3DReader", "Graphics.Texture3D", (r, o) => new Texture3D(r)),
            new TypeReader<TextureCube>("TextureCubeReader", "Graphics.TextureCube", (r, o) => new TextureCube(r)),
            new TypeReader<IndexBuffer>("IndexBufferReader", "Graphics.IndexBuffer", (r, o) => new IndexBuffer(r)),
            new TypeReader<VertexBuffer>("VertexBufferReader", "Graphics.VertexBuffer", (r, o) => new VertexBuffer(r)),
            new TypeReader<VertexDeclaration>("VertexDeclarationReader", "Graphics.VertexDeclaration", (r, o) => new VertexDeclaration(r)),
            new TypeReader<Effect>("EffectReader", "Graphics.Effect", (r, o) => new Effect(r)),
            new TypeReader<EffectMaterial>("EffectMaterialReader", "Graphics.EffectMaterial", (r, o) => new EffectMaterial(r)),
            new TypeReader<BasicEffect>("BasicEffectReader", "Graphics.BasicEffect", (r, o) => new BasicEffect(r)),
            new TypeReader<AlphaTestEffect>("AlphaTestEffectReader", "Graphics.AlphaTestEffect", (r, o) => new AlphaTestEffect(r)),
            new TypeReader<DualTextureEffect>("DualTextureEffectReader", "Graphics.DualTextureEffect", (r, o) => new DualTextureEffect(r)),
            new TypeReader<EnvironmentMapEffect>("EnvironmentMapEffectReader", "Graphics.EnvironmentMapEffect", (r, o) => new EnvironmentMapEffect(r)),
            new TypeReader<SkinnedEffect>("SkinnedEffectReader", "Graphics.SkinnedEffect", (r, o) => new SkinnedEffect(r)),
            new TypeReader<SpriteFont>("SpriteFontReader", "Graphics.SpriteFont", (r, o) => new SpriteFont(r)),
            new TypeReader<Model>("ModelReader", "Graphics.Model", (r, o) => new Model(r)),
            // Media types
            new TypeReader<SoundEffect>("SoundEffectReader", "Audio.SoundEffect", (r, o) => new SoundEffect(r)),
            new TypeReader<Song>("SongReader", "Media.Song", (r, o) => new Song(r)),
            new TypeReader<Video>("VideoReader", "Media.Video", (r, o) => new Video(r))
    ];

    readonly static Dictionary<string, TypeReader> ReadersByName = Readers.ToDictionary(s => s.Name);
    readonly static Dictionary<string, TypeReader> ReadersByType = Readers.GroupBy(s => s.Type).Select(s => (s.Key, s.First())).ToDictionary(s => s.Key, s => s.Item2);
    readonly static Dictionary<Type, TypeReader> ReadersByT = Readers.GroupBy(s => s.T).Select(s => (s.Key, s.First())).ToDictionary(s => s.Key, s => s.Item2);

    public static TypeReader Add(TypeReader reader) {
        Readers.Add(reader);
        if (reader.Name != null) ReadersByName[reader.Name] = reader;
        if (reader.Type != null && !ReadersByType.ContainsKey(reader.Type)) ReadersByType[reader.Type] = reader;
        if (reader.T != null && !ReadersByT.ContainsKey(reader.T)) ReadersByT[reader.T] = reader;
        return reader;
    }

    public static TypeReader Create(GenericReader s, Type[] args) {
        TypeReader r;
        if (args.Length == 1) { var value = GetByType(args[0]); r = (TypeReader)s.GInit.MakeGenericMethod([value.T]).Invoke(null, [value]); }
        else if (args.Length == 2) { var key = GetByType(args[0]); var value = GetByType(args[1]); r = (TypeReader)s.GInit.MakeGenericMethod([key.T, value.T]).Invoke(null, [key, value]); }
        else throw new Exception();
        var suffix = $"`{args.Length}[[{string.Join("],[", args.Select(s => s.FullName))}]]";
        r.Name = s.Name + suffix;
        r.Type = s.Type + suffix;
        r.ValueType = s.ValueType;
        return r;
    }
    public static TypeReader Create(GenericReader s, string[] args) {
        TypeReader r;
        if (args.Length == 1) { var value = GetByType(args[0]); r = (TypeReader)s.GInit.MakeGenericMethod([value.T]).Invoke(null, [value]); }
        else if (args.Length == 2) { var key = GetByType(args[0]); var value = GetByType(args[1]); r = (TypeReader)s.GInit.MakeGenericMethod([key.T, value.T]).Invoke(null, [key, value]); }
        else throw new Exception();
        var suffix = $"`{args.Length}[[{string.Join("],[", args)}]]";
        r.Name = s.Name + suffix;
        r.Type = s.Type + suffix;
        r.ValueType = s.ValueType;
        return r;
    }

    public static TypeReader GetByName(string name, uint version) {
        name = Reflect.StripAssemblyVersion(name).Replace("Microsoft.Xna.Framework.Content.", "");
        if (ReadersByName.TryGetValue(name, out var reader)) return reader;
        var (genericName, args) = Reflect.SplitGenericTypeName(name);
        if (genericName != null && ReadersByName.TryGetValue(genericName, out reader) && reader is GenericReader generic) {
            reader = Create(generic, args);
            if (reader.Name != name) throw new Exception("ERROR");
            return Add(reader);
        }
        throw new Exception($"Cannot find codec elem '{name}'.");
    }
    public static TypeReader GetByType(Type type) {
        //return GetByTarget(Reflect.TryGetName(target, out var z) ? z : target.FullName.Replace("System.Numerics.", "").Replace("System.Drawing.", ""));
        if (ReadersByT.TryGetValue(type, out var reader)) return reader;
        if (type.IsArray && type.GetArrayRank() > 1) {
            reader = Create((GenericReader)ReadersByType["Array"], [type.GetElementType()]);
            if (reader.T != type) throw new Exception("ERROR");
            return Add(reader);
        }
        var (genericName, args) = Reflect.SplitGenericType(type);
        if (genericName != null && ReadersByType.TryGetValue(genericName.Replace("System.", ""), out reader) && reader is GenericReader generic) {
            reader = Create(generic, args);
            if (reader.T != type) throw new Exception("ERROR");
            return Add(reader);
        }
        throw new Exception($"Cannot find codec elem '{type}'.");
        //reader = (TypeReader)typeof(TypeManager).GetMethod("_MakeReader").MakeGenericMethod([type]).Invoke(null, [null]);
        //return Add(reader);
    }
    public static TypeReader GetByType(string type) {
        type = Reflect.StripAssemblyVersion(type).Replace("Microsoft.Xna.Framework.", "").Replace("System.", "").Replace("+", ".");
        type = type.Replace("GameX.Xbox.Formats.", "");
        if (ReadersByType.TryGetValue(type, out var reader)) return reader;
        var (genericName, args) = Reflect.SplitGenericTypeName(type);
        if (genericName != null && ReadersByType.TryGetValue(genericName, out reader) && reader is GenericReader generic) {
            reader = Create(generic, args);
            if (reader.Type != type) throw new Exception("ERROR");
            return Add(reader);
        }
        Type t;
        if ((t = Reflect.GetTypeByName(type)) != null) {
            reader = (TypeReader)typeof(TypeManager).GetMethod("_MakeReader").MakeGenericMethod([t]).Invoke(null, [type]);
            return Add(reader);
        }
        throw new Exception($"Cannot find elem for T codec '{type}'.");
    }
}

public class ContentReader(Stream input) : BinaryReader(input) {
    TypeReader[] TypeReaders;
    public int SharedResourceCount;
    List<KeyValuePair<int, Action<object>>> SharedResourceFixups;
    public object[] SharedResources;

    public T ReadAsset<T>(T obj = default) { ReadTypeReaders(); var s = ReadObject<T>(obj); ReadSharedResources(); return s; }
    public void ReadTypeReaders() {
        TypeReaders = this.ReadLV7FArray(z => TypeManager.GetByName(this.ReadLV7UString(), ReadUInt32()));
        SharedResourceCount = this.ReadVInt7(); SharedResourceFixups = [];
        foreach (var s in TypeReaders.Where(x => x.Init != null)) s.Init(s);
    }
    public void ReadSharedResources() {
        if (SharedResourceCount <= 0) return;
        SharedResources = this.ReadFArray(z => ReadObject<object>(), SharedResourceCount);
        foreach (var fixup in SharedResourceFixups) fixup.Value(SharedResourceFixups[fixup.Key]);
    }
    public T Read<T>(TypeReader reader, T obj = default) => reader.ValueType ? (T)reader.Read(this, obj) : ReadObject(obj);
    public T Read<T>(TypeReader<T> reader, T obj = default) => reader.ValueType ? reader.Read(this, obj) : ReadObject(obj);
    public T ReadObject<T>(T obj = default) { var reader = ReadReader(); return reader != null ? (T)reader.Read(this, obj) : default; }
    public TypeReader ReadReader() { var idx = this.ReadVInt7() - 1; return idx >= 0 ? idx < TypeReaders.Length ? TypeReaders[idx] : throw new Exception("Invalid XNB file: idx is out of range.") : null; }
    public ContentReader Validate(string type) { var reader = ReadReader(); return reader != null && reader.Type == type ? this : throw new Exception("Invalid XNB file: got an unexpected idx."); }
    public void ReadSharedResource<T>(Action<T> fixup) {
        var idx = this.ReadVInt7() - 1;
        if (idx >= 0) SharedResourceFixups.Add(new KeyValuePair<int, Action<object>>(idx, v => {
            if (v is T t) fixup(t);
            else throw new Exception($"Error loading shared resource. Expected t {typeof(T).Name}, received t {v.GetType().Name}");
        }));
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
    public readonly Dictionary<string, object> Parameters = r.ReadObject<Dictionary<string, object>>();
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
    public readonly CompareFunction AlphaFunction = (CompareFunction)r.ReadInt32();
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
    static Bone ReadBoneIdx(Model p, ContentReader r) { var id = p.Bones8 ? r.ReadByte() : r.ReadUInt32(); return id != 0 ? p.Bones[(int)(id - 1)] : null; }
    public class Bone(ContentReader r) {
        public readonly string Name = r.ReadObject<string>();
        public readonly Matrix4x4 Transform = r.ReadMatrix4x4();
        public Bone Parent;
        public Bone[] Children;
    }
    public class MeshPart(ContentReader r) {
        public readonly int VertexOffset = r.ReadInt32();
        public readonly int NumVertices = r.ReadInt32();
        public readonly int StartIndex = r.ReadInt32();
        public readonly int PrimitiveCount = r.ReadInt32();
        public readonly object Tag = r.ReadObject<object>();
        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;
        public Effect Effect;
        public MeshPart Apply(ContentReader r) {
            r.ReadSharedResource<VertexBuffer>(v => VertexBuffer = v);
            r.ReadSharedResource<IndexBuffer>(v => IndexBuffer = v);
            r.ReadSharedResource<Effect>(v => Effect = v);
            return this;
        }
    }
    public class Mesh(Model p, ContentReader r) {
        public readonly string Name = r.ReadObject<string>();
        public readonly Bone Parent = ReadBoneIdx(p, r);
        public readonly BoundingSphere Bounds = new(r.ReadVector3(), r.ReadSingle());
        public readonly object Tag = r.ReadObject<object>();
        public readonly MeshPart[] Parts = r.ReadL32FArray(z => new MeshPart(r).Apply(r));
    }
    public readonly Bone[] Bones; readonly bool Bones8;
    public readonly Mesh[] Meshs;
    public readonly Bone Root;
    public readonly object Tag;
    public Model(ContentReader r) {
        Bones = r.ReadL32FArray(z => new Bone(r)); Bones8 = Bones.Length < 255;
        foreach (var s in Bones) {
            s.Parent = ReadBoneIdx(this, r);
            s.Children = r.ReadL32FArray(z => ReadBoneIdx(this, r));
        }
        Meshs = r.ReadL32FArray(z => new Mesh(this, r));
        Root = ReadBoneIdx(this, r);
        Tag = r.ReadObject<object>();
    }
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
            new("Model", items: [
                new($"Bones: {Bones.Length}"),
                new($"Meshs: {Meshs.Length}"),
                new($"Root: {Root.Name}"),
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
    public readonly int Duration = r.Validate("Int32").ReadInt32();
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
            new("Texture", items: [
                new($"Filename: {Filename}"),
                new($"Duration: {Duration}")
            ])];
}

public class Video(ContentReader r) : IHaveMetaInfo {
    public readonly string Filename = r.Validate("String").ReadLV7UString();
    public readonly int Duration = r.Validate("Int32").ReadInt32();
    public readonly int Width = r.Validate("Int32").ReadInt32();
    public readonly int Height = r.Validate("Int32").ReadInt32();
    public readonly float FramesPerSecond = r.Validate("Single").ReadSingle();
    public readonly SoundtrackType SoundtrackType = (SoundtrackType)r.Validate("Int32").ReadInt32();
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
