using OpenStack.Gfx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace GameX.Xbox.Formats.Xna;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class OptionalAttribute : Attribute { public bool SharedResource = false; }

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class IgnoreAttribute : Attribute { }

#region Type Manager

public abstract class TypeReader(Type type, bool valueType = false, bool canUseObj = false) {
    public Type Type = type;
    public bool ValueType = valueType;
    public bool CanUseObj = canUseObj;
    public virtual void Init(TypeManager manager) { }
    public abstract object Read(ContentReader r, object o);
}

public abstract class TypeReader<T>(bool valueType = false, bool canUseObj = false) : TypeReader(typeof(T), valueType, canUseObj) {
    public override object Read(ContentReader r, object o) => Read(r, (T)o);
    public abstract T Read(ContentReader r, T o);
}

// Primitive types
[RType("ByteReader")] class ByteReader() : TypeReader<byte>(valueType: true) { public override byte Read(ContentReader r, byte o) => r.ReadByte(); }
[RType("SByteReader")] class SByteReader() : TypeReader<sbyte>(valueType: true) { public override sbyte Read(ContentReader r, sbyte o) => r.ReadSByte(); }
[RType("Int16Reader")] class Int16Reader() : TypeReader<short>(valueType: true) { public override short Read(ContentReader r, short o) => r.ReadInt16(); }
[RType("UInt16Reader")] class UInt16Reader() : TypeReader<ushort>(valueType: true) { public override ushort Read(ContentReader r, ushort o) => r.ReadUInt16(); }
[RType("Int32Reader")] class Int32Reader() : TypeReader<int>(valueType: true) { public override int Read(ContentReader r, int o) => r.ReadInt32(); }
[RType("UInt32Reader")] class UInt32Reader() : TypeReader<uint>(valueType: true) { public override uint Read(ContentReader r, uint o) => r.ReadUInt32(); }
[RType("Int64Reader")] class Int64Reader() : TypeReader<long>(valueType: true) { public override long Read(ContentReader r, long o) => r.ReadInt64(); }
[RType("UInt64Reader")] class UInt64Reader() : TypeReader<ulong>(valueType: true) { public override ulong Read(ContentReader r, ulong o) => r.ReadUInt64(); }
[RType("SingleReader")] class SingleReader() : TypeReader<float>(valueType: true) { public override float Read(ContentReader r, float o) => r.ReadSingle(); }
[RType("DoubleReader")] class DoubleReader() : TypeReader<double>(valueType: true) { public override double Read(ContentReader r, double o) => r.ReadDouble(); }
[RType("BooleanReader")] class BooleanReader() : TypeReader<bool>(valueType: true) { public override bool Read(ContentReader r, bool o) => r.ReadBoolean(); }
[RType("CharReader")] class CharReader() : TypeReader<char>(valueType: true) { public override char Read(ContentReader r, char o) => r.ReadChar(); }
[RType("StringReader")] class StringReader() : TypeReader<string>() { public override string Read(ContentReader r, string o) => r.ReadLV7UString(); }

// System types
[RType("EnumReader")] class EnumReader<T>() : TypeReader<T>() { TypeReader elem; public override void Init(TypeManager manager) => elem = manager.GetTypeReader(Enum.GetUnderlyingType(typeof(T))); public override T Read(ContentReader r, T o) => r.ReadRawObject<T>(elem); }
[RType("NullableReader")] class NullableReader<T>() : TypeReader<T?>() where T : struct { TypeReader elem; public override void Init(TypeManager manager) => elem = manager.GetTypeReader(typeof(T)); public override T? Read(ContentReader r, T? o) => r.ReadBoolean() ? (T?)r.ReadObject<T>(elem, default) : default; }
[RType("ArrayReader")] class ArrayReader<T>() : TypeReader<T[]>() { TypeReader elem; public override void Init(TypeManager manager) => elem = manager.GetTypeReader(typeof(T)); public override T[] Read(ContentReader r, T[] o) => r.ReadL32FArray(z => r.ReadObject<T>(elem, default), obj: o); }
[RType("ListReader")] class ListReader<T>() : TypeReader<List<T>>(canUseObj: false) { TypeReader elem; public override void Init(TypeManager manager) => elem = manager.GetTypeReader(typeof(T)); public override List<T> Read(ContentReader r, List<T> o) => r.ReadL32FList(z => r.ReadObject<T>(elem, default), obj: o); }
[RType("DictionaryReader")] class DictionaryReader<TKey, TValue>() : TypeReader<Dictionary<TKey, TValue>>(canUseObj: true) { TypeReader key; TypeReader value; public override void Init(TypeManager manager) { key = manager.GetTypeReader(typeof(TKey)); value = manager.GetTypeReader(typeof(TValue)); } public override Dictionary<TKey, TValue> Read(ContentReader r, Dictionary<TKey, TValue> o) => (Dictionary<TKey, TValue>)r.ReadL32FMany(z => r.ReadObject<TKey>(key, default), z => r.ReadObject<TValue>(value, default), obj: o); }
//new GenericReader("MultiArrayReader", null, typeof(TypeManager).GetMethod("_MultiArrayReader")),
[RType("TimeSpanReader")] class TimeSpanReader() : TypeReader<TimeSpan>(valueType: true) { public override TimeSpan Read(ContentReader r, TimeSpan o) => new(r.ReadInt64()); }
[RType("DateTimeReader")] class DateTimeReader() : TypeReader<DateTime>(valueType: true) { public override DateTime Read(ContentReader r, DateTime o) { var v = r.ReadUInt64(); return new DateTime((long)(v & ~(3UL << 62)), (DateTimeKind)((v >> 62) & 3)); } }
[RType("DecimalReader")] class DecimalReader() : TypeReader<decimal>(valueType: true) { public override decimal Read(ContentReader r, decimal o) => r.ReadDecimal(); }
[RType("ExternalReferenceReader")] class ExternalReferenceReader() : TypeReader<string>() { public override string Read(ContentReader r, string o) => r.ReadExternalReference(); }
//new GenericReader("ReflectiveReader", "Object", typeof(TypeManager).GetMethod("_ReflectiveReader")),

[RType("ExternalReferenceReader")]
class ReflectiveReader<T> : TypeReader {
    delegate void ReadElement(ContentReader input, object parent);

    private List<ReadElement> _readers;

    private ConstructorInfo _constructor;

    private ContentTypeReader _baseTypeReader;


    public ReflectiveReader()
        : base(typeof(T)) {
    }

    public override bool CanDeserializeIntoExistingObject {
        get { return TargetType.IsClass(); }
    }

    protected internal override void Initialize(ContentTypeReaderManager manager) {
        base.Initialize(manager);

        var baseType = ReflectionHelpers.GetBaseType(TargetType);
        if (baseType != null && baseType != typeof(object))
            _baseTypeReader = manager.GetTypeReader(baseType);

        _constructor = TargetType.GetDefaultConstructor();

        var properties = TargetType.GetAllProperties();
        var fields = TargetType.GetAllFields();
        _readers = new List<ReadElement>(fields.Length + properties.Length);

        // Gather the properties.
        foreach (var property in properties) {
            var read = GetElementReader(manager, property);
            if (read != null)
                _readers.Add(read);
        }

        // Gather the fields.
        foreach (var field in fields) {
            var read = GetElementReader(manager, field);
            if (read != null)
                _readers.Add(read);
        }
    }

    private static ReadElement GetElementReader(ContentTypeReaderManager manager, MemberInfo member) {
        var property = member as PropertyInfo;
        var field = member as FieldInfo;
        Debug.Assert(field != null || property != null);

        if (property != null) {
            // Properties must have at least a getter.
            if (property.CanRead == false)
                return null;

            // Skip over indexer properties.
            if (property.GetIndexParameters().Any())
                return null;
        }

        // Are we explicitly asked to ignore this item?
        if (ReflectionHelpers.GetCustomAttribute<ContentSerializerIgnoreAttribute>(member) != null)
            return null;

        var contentSerializerAttribute = ReflectionHelpers.GetCustomAttribute<ContentSerializerAttribute>(member);
        if (contentSerializerAttribute == null) {
            if (property != null) {
                // There is no ContentSerializerAttribute, so non-public
                // properties cannot be deserialized.
                if (!ReflectionHelpers.PropertyIsPublic(property))
                    return null;

                // If the read-only property has a type reader,
                // and CanDeserializeIntoExistingObject is true,
                // then it is safe to deserialize into the existing object.
                if (!property.CanWrite) {
                    var typeReader = manager.GetTypeReader(property.PropertyType);
                    if (typeReader == null || !typeReader.CanDeserializeIntoExistingObject)
                        return null;
                }
            }
            else {
                // There is no ContentSerializerAttribute, so non-public
                // fields cannot be deserialized.
                if (!field.IsPublic)
                    return null;

                // evolutional: Added check to skip initialise only fields
                if (field.IsInitOnly)
                    return null;
            }
        }

        Action<object, object> setter;
        Type elementType;
        if (property != null) {
            elementType = property.PropertyType;
            if (property.CanWrite)
                setter = (o, v) => property.SetValue(o, v, null);
            else
                setter = (o, v) => { };
        }
        else {
            elementType = field.FieldType;
            setter = field.SetValue;
        }

        // Shared resources get special treatment.
        if (contentSerializerAttribute != null && contentSerializerAttribute.SharedResource) {
            return (input, parent) =>
            {
                Action<object> action = value => setter(parent, value);
                input.ReadSharedResource(action);
            };
        }

        // We need to have a reader at this point.
        var reader = manager.GetTypeReader(elementType);
        if (reader == null)
            if (elementType == typeof(System.Array))
                reader = new ArrayReader<Array>();
            else
                throw new ContentLoadException(string.Format("Content reader could not be found for {0} type.", elementType.FullName));

        // We use the construct delegate to pick the correct existing 
        // object to be the target of deserialization.
        Func<object, object> construct = parent => null;
        if (property != null && !property.CanWrite)
            construct = parent => property.GetValue(parent, null);

        return (input, parent) =>
        {
            var existing = construct(parent);
            var obj2 = input.ReadObject(reader, existing);
            setter(parent, obj2);
        };
    }

    protected internal override object Read(ContentReader input, object existingInstance) {
        T obj;
        if (existingInstance != null)
            obj = (T)existingInstance;
        else
            obj = (_constructor == null ? (T)Activator.CreateInstance(typeof(T)) : (T)_constructor.Invoke(null));

        if (_baseTypeReader != null)
            _baseTypeReader.Read(input, obj);

        // Box the type.
        var boxed = (object)obj;

        foreach (var reader in _readers)
            reader(input, boxed);

        // Unbox it... required for value types.
        obj = (T)boxed;

        return obj;
    }
}

// Math types
[RType("Vector2Reader")] class Vector2Reader() : TypeReader<Vector2>(valueType: true) { public override Vector2 Read(ContentReader r, Vector2 o) => r.ReadVector2(); }
[RType("Vector3Reader")] class Vector3Reader() : TypeReader<Vector3>(valueType: true) { public override Vector3 Read(ContentReader r, Vector3 o) => r.ReadVector3(); }
[RType("Vector4Reader")] class Vector4Reader() : TypeReader<Vector4>(valueType: true) { public override Vector4 Read(ContentReader r, Vector4 o) => r.ReadVector4(); }
[RType("MatrixReader")] class MatrixReader() : TypeReader<Matrix4x4>(valueType: true) { public override Matrix4x4 Read(ContentReader r, Matrix4x4 o) => r.ReadMatrix4x4(); }
[RType("QuaternionReader")] class QuaternionReader() : TypeReader<Quaternion>(valueType: true) { public override Quaternion Read(ContentReader r, Quaternion o) => r.ReadQuaternion(); }
[RType("ColorReader")] class ColorReader() : TypeReader<ByteColor4>(valueType: true) { public override ByteColor4 Read(ContentReader r, ByteColor4 o) => new(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte()); }
[RType("PlaneReader")] class PlaneReader() : TypeReader<Plane>(valueType: true) { public override Plane Read(ContentReader r, Plane o) => new(r.ReadVector3(), r.ReadSingle()); }
[RType("PointReader")] class PointReader() : TypeReader<Point>(valueType: true) { public override Point Read(ContentReader r, Point o) => new(r.ReadInt32(), r.ReadInt32()); }
[RType("RectangleReader")] class RectangleReader() : TypeReader<Rectangle>(valueType: true) { public override Rectangle Read(ContentReader r, Rectangle o) => new(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32()); }
[RType("BoundingBoxReader")] class BoundingBoxReader() : TypeReader<BoundingBox>(valueType: true) { public override BoundingBox Read(ContentReader r, BoundingBox o) => new(r.ReadVector3(), r.ReadVector3()); }
[RType("BoundingSphereReader")] class BoundingSphereReader() : TypeReader<BoundingSphere>(valueType: true) { public override BoundingSphere Read(ContentReader r, BoundingSphere o) => new(r.ReadVector3(), r.ReadSingle()); }
[RType("BoundingFrustumReader")] class BoundingFrustumReader() : TypeReader<BoundingFrustum>(valueType: true) { public override BoundingFrustum Read(ContentReader r, BoundingFrustum o) => new(r.ReadMatrix4x4()); }
[RType("RayReader")] class RayReader() : TypeReader<Ray>(valueType: true) { public override Ray Read(ContentReader r, Ray o) => new(r.ReadVector3(), r.ReadVector3()); }
[RType("CurveReader")] class CurveReader() : TypeReader<Curve>(valueType: true) { public override Curve Read(ContentReader r, Curve o) => new(r.ReadInt32(), r.ReadInt32(), r.ReadL32FArray(z => new Curve.Key(z.ReadSingle(), z.ReadSingle(), z.ReadSingle(), z.ReadSingle(), z.ReadInt32()))); }

// Graphics types
[RType("TextureReader")] class TextureReader() : TypeReader<Texture>() { public override Texture Read(ContentReader r, Texture o) => o; }
[RType("Texture2DReader")] class Texture2DReader() : TypeReader<Texture2D>() { public override Texture2D Read(ContentReader r, Texture2D o) => new(r); }
[RType("Texture3DReader")] class Texture3DReader() : TypeReader<Texture3D>() { public override Texture3D Read(ContentReader r, Texture3D o) => new(r); }
[RType("TextureCubeReader")] class TextureCubeReader() : TypeReader<TextureCube>() { public override TextureCube Read(ContentReader r, TextureCube o) => new(r); }
[RType("IndexBufferReader")] class IndexBufferReader() : TypeReader<IndexBuffer>() { public override IndexBuffer Read(ContentReader r, IndexBuffer o) => new(r); }
[RType("VertexBufferReader")] class VertexBufferReader() : TypeReader<VertexBuffer>() { public override VertexBuffer Read(ContentReader r, VertexBuffer o) => new(r); }
[RType("VertexDeclarationReader")] class VertexDeclarationReader() : TypeReader<VertexDeclaration>() { public override VertexDeclaration Read(ContentReader r, VertexDeclaration o) => new(r); }
[RType("EffectReader")] class EffectReader() : TypeReader<Effect>() { public override Effect Read(ContentReader r, Effect o) => new(r); }
[RType("EffectMaterialReader")] class EffectMaterialReader() : TypeReader<EffectMaterial>() { public override EffectMaterial Read(ContentReader r, EffectMaterial o) => new(r); }
[RType("BasicEffectReader")] class BasicEffectReader() : TypeReader<BasicEffect>() { public override BasicEffect Read(ContentReader r, BasicEffect o) => new(r); }
[RType("AlphaTestEffectReader")] class AlphaTestEffectReader() : TypeReader<AlphaTestEffect>() { public override AlphaTestEffect Read(ContentReader r, AlphaTestEffect o) => new(r); }
[RType("DualTextureEffectReader")] class DualTextureEffectReader() : TypeReader<DualTextureEffect>() { public override DualTextureEffect Read(ContentReader r, DualTextureEffect o) => new(r); }
[RType("EnvironmentMapEffectReader")] class EnvironmentMapEffectReader() : TypeReader<EnvironmentMapEffect>() { public override EnvironmentMapEffect Read(ContentReader r, EnvironmentMapEffect o) => new(r); }
[RType("SkinnedEffectReader")] class SkinnedEffectReader() : TypeReader<SkinnedEffect>() { public override SkinnedEffect Read(ContentReader r, SkinnedEffect o) => new(r); }
[RType("SpriteFontReader")] class SpriteFontReader() : TypeReader<SpriteFont>() { public override SpriteFont Read(ContentReader r, SpriteFont o) => new(r); }
[RType("ModelReader")] class ModelReader() : TypeReader<Model>() { public override Model Read(ContentReader r, Model o) => new(r); }

// Media types
[RType("SoundEffectReader")] class SoundEffectReader() : TypeReader<SoundEffect>() { public override SoundEffect Read(ContentReader r, SoundEffect o) => new(r); }
[RType("SongReader")] class SongReader() : TypeReader<Song>() { public override Song Read(ContentReader r, Song o) => new(r); }
[RType("VideoReader")] class VideoReader() : TypeReader<Video>() { public override Video Read(ContentReader r, Video o) => new(r); }

public class TypeManager {
    static readonly object Locker = new();
    static readonly Dictionary<Type, TypeReader> ReadersCache = new(255);
    static readonly string AssemblyName = typeof(TypeManager).Assembly.FullName;
    static Dictionary<Type, TypeReader> Readers;
    static readonly bool IsRunningOnNetCore = typeof(object).Assembly.GetName().Name == "System.Private.CoreLib";
    static Dictionary<string, Func<TypeReader>> TypeFactories = []; // Static map of type names to creation functions. Required as iOS requires all types at compile time

    public TypeReader GetTypeReader(Type type) {
        if (type.IsArray && type.GetArrayRank() > 1) type = typeof(Array);
        return Readers.TryGetValue(type, out var reader) ? reader : null;
    }

    static bool Falseflag = false;

    internal TypeReader[] LoadTypeReaders(ContentReader r) {
#pragma warning disable 0219, 0649
        // Trick to prevent the linker removing the code, but not actually execute the code
        if (Falseflag) {
            // Dummy variables required for it to work on iDevices ** DO NOT DELETE **
            // This forces the classes not to be optimized out when deploying to iDevices
            var hByteReader = new ByteReader();
            var hSByteReader = new SByteReader();
            var hDateTimeReader = new DateTimeReader();
            var hDecimalReader = new DecimalReader();
            var hBoundingSphereReader = new BoundingSphereReader();
            var hBoundingFrustumReader = new BoundingFrustumReader();
            var hRayReader = new RayReader();
            var hCharListReader = new ListReader<char>();
            var hRectangleListReader = new ListReader<Rectangle>();
            var hRectangleArrayReader = new ArrayReader<Rectangle>();
            var hVector3ListReader = new ListReader<Vector3>();
            var hStringListReader = new ListReader<StringReader>();
            var hIntListReader = new ListReader<int>();
            var hSpriteFontReader = new SpriteFontReader();
            var hTexture2DReader = new Texture2DReader();
            var hCharReader = new CharReader();
            var hRectangleReader = new RectangleReader();
            var hStringReader = new StringReader();
            var hVector2Reader = new Vector2Reader();
            var hVector3Reader = new Vector3Reader();
            var hVector4Reader = new Vector4Reader();
            var hCurveReader = new CurveReader();
            var hIndexBufferReader = new IndexBufferReader();
            var hBoundingBoxReader = new BoundingBoxReader();
            var hMatrixReader = new MatrixReader();
            var hBasicEffectReader = new BasicEffectReader();
            var hVertexBufferReader = new VertexBufferReader();
            var hAlphaTestEffectReader = new AlphaTestEffectReader();
            //var hEnumSpriteEffectsReader = new EnumReader<SpriteEffects>();
            var hArrayFloatReader = new ArrayReader<float>();
            var hArrayVector2Reader = new ArrayReader<Vector2>();
            var hListVector2Reader = new ListReader<Vector2>();
            var hArrayMatrixReader = new ArrayReader<Matrix4x4>();
            //var hEnumBlendReader = new EnumReader<Graphics.Blend>();
            var hNullableRectReader = new NullableReader<Rectangle>();
            var hEffectMaterialReader = new EffectMaterialReader();
            var hExternalReferenceReader = new ExternalReferenceReader();
            var hSoundEffectReader = new SoundEffectReader();
            var hSongReader = new SongReader();
            var hModelReader = new ModelReader();
            var hInt32Reader = new Int32Reader();
            var hEffectReader = new EffectReader();
            var hSingleReader = new SingleReader();
            // At the moment the Video class doesn't exist on all platforms... Allow it to compile anyway.
#if ANDROID || (IOS && !TVOS) || MONOMAC || (WINDOWS && !OPENGL)
            var hVideoReader = new VideoReader();
#endif
        }
#pragma warning restore 0219, 0649

        // The first content byte i read tells me the number of content readers in this XNB file
        var readerCount = r.ReadVInt7();
        var readers = new TypeReader[readerCount];
        var needsInitialize = new BitArray(readerCount);
        Readers = new(readerCount);
        lock (Locker) {
            for (var i = 0; i < readerCount; i++) {
                var originalReaderTypeString = r.ReadString();
                if (TypeFactories.TryGetValue(originalReaderTypeString, out var readerFunc)) {
                    readers[i] = readerFunc();
                    needsInitialize[i] = true;
                }
                else {
                    var readerTypeString = PrepareType(originalReaderTypeString);
                    var readerType = Type.GetType(readerTypeString);
                    if (readerType != null) {
                        if (!ReadersCache.TryGetValue(readerType, out var reader)) {
                            try {
                                reader = Reflect.GetDefaultConstructor(readerType).Invoke(null) as TypeReader;
                            }
                            catch (TargetInvocationException ex) { throw new InvalidOperationException($"Failed to get default constructor for TypeReader. To work around, add a creation function to ContentTypeReaderManager.AddTypeFactory() with the following failed type string: {originalReaderTypeString}", ex); }
                            needsInitialize[i] = true;
                            ReadersCache.Add(readerType, reader);
                        }
                        readers[i] = reader;
                    }
                    else throw new Exception($"Could not find TypeReader Type. Please ensure the name of the Assembly that contains the Type matches the assembly in the full type name: {originalReaderTypeString} ({readerTypeString})");
                }

                var type = readers[i].Type;
                if (type != null && !Readers.ContainsKey(type)) Readers.Add(type, readers[i]);
                r.ReadInt32();
            }
            // Initialize any new readers.
            for (var i = 0; i < readers.Length; i++) if (needsInitialize.Get(i)) readers[i].Init(this);
        }
        return readers;
    }

    public static string PrepareType(string type) {
        // needed to support nested types
        var count = type.Split(["[["], StringSplitOptions.None).Length - 1;
        var preparedType = type;
        for (var i = 0; i < count; i++) preparedType = Regex.Replace(preparedType, @"\[(.+?), Version=.+?\]", "[$1]");
        // handle non generic types
        if (preparedType.Contains("PublicKeyToken")) preparedType = Regex.Replace(preparedType, @"(.+?), Version=.+?$", "$1");
        preparedType = preparedType.Replace(", Microsoft.Xna.Framework.Graphics", string.Format(", {0}", AssemblyName));
        preparedType = preparedType.Replace(", Microsoft.Xna.Framework.Video", string.Format(", {0}", AssemblyName));
        preparedType = preparedType.Replace(", Microsoft.Xna.Framework", string.Format(", {0}", AssemblyName));
        preparedType = IsRunningOnNetCore ? preparedType.Replace("mscorlib", "System.Private.CoreLib") : preparedType.Replace("System.Private.CoreLib", "mscorlib");
        return preparedType;
    }
    public static void AddTypeFactory(string type, Func<TypeReader> factory) { if (!TypeFactories.ContainsKey(type)) TypeFactories.Add(type, factory); }
    public static void ClearTypeFactories() => TypeFactories.Clear();
}

public class ContentReader(Stream stream, string assetName, int version, Action<IDisposable> recordDisposableAction) : BinaryReader(stream) {
    readonly string AssetName = assetName;
    readonly int Version = version;
    readonly Action<IDisposable> RecordDisposableAction = recordDisposableAction;
    TypeManager TypeManager;
    TypeReader[] TypeReaders;
    int SharedResourceCount;
    List<KeyValuePair<int, Action<object>>> SharedResourceFixups;

    void RecordDisposable<T>(T result) {
        if (result is not IDisposable disposable) return;
        RecordDisposableAction?.Invoke(disposable);
    }

    public void ReadTypeReaders() {
        TypeManager = new TypeManager();
        TypeReaders = TypeManager.LoadTypeReaders(this);
        SharedResourceCount = this.ReadVInt7();
        SharedResourceFixups = [];
    }

    public object ReadAsset<T>() { ReadTypeReaders(); var s = ReadObject<T>(); ReadSharedResources(); return s; }
    public object ReadAsset<T>(T obj) { ReadTypeReaders(); var s = ReadObject<T>(obj); ReadSharedResources(); return s; }

    public string ReadExternalReference() => this.ReadLV7UString();

    public T ReadObject<T>() => InnerReadObject(default(T));
    public T ReadObject<T>(T obj) => InnerReadObject(obj);
    public T ReadObject<T>(TypeReader reader) { var s = (T)reader.Read(this, default(T)); RecordDisposable(s); return s; }
    public T ReadObject<T>(TypeReader reader, T obj) {
        if (!reader.ValueType) return ReadObject(obj);
        var s = (T)reader.Read(this, obj); RecordDisposable(s); return s;
    }
    T InnerReadObject<T>(T obj) {
        var idx = this.ReadVInt7();
        if (idx == 0) return obj;
        if (idx > TypeReaders.Length) throw new Exception("Incorrect type reader index found.");
        var s = (T)TypeReaders[idx - 1].Read(this, obj); RecordDisposable(s); return s;
    }

    public T ReadRawObject<T>() => ReadRawObject(default(T));
    public T ReadRawObject<T>(TypeReader reader) => ReadRawObject<T>(reader, default);
    public T ReadRawObject<T>(T obj) {
        var type = typeof(T);
        foreach (var reader in TypeReaders) if (reader.Type == type) return ReadRawObject(reader, obj);
        throw new NotSupportedException();
    }
    public T ReadRawObject<T>(TypeReader reader, T obj) => (T)reader.Read(this, obj);

    public void ReadSharedResources() {
        if (SharedResourceCount <= 0) return;
        var sharedResources = this.ReadFArray(z => InnerReadObject<object>(null), SharedResourceCount);
        // fixup shared resources by calling each registered action
        foreach (var fixup in SharedResourceFixups) fixup.Value(SharedResourceFixups[fixup.Key]);
    }
    public void ReadSharedResource<T>(Action<T> fixup) {
        var idx = this.ReadVInt7();
        if (idx > 0) SharedResourceFixups.Add(new KeyValuePair<int, Action<object>>(idx - 1, v => {
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

public class Texture {
}

public class Texture2D(ContentReader r) : Texture, IHaveMetaInfo, ITexture {
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

public class Texture3D(ContentReader r) : Texture, IHaveMetaInfo {
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

public class TextureCube(ContentReader r) : Texture, IHaveMetaInfo {
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
    public readonly int Duration = r.ReadObject<int>();
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
            new("Texture", items: [
                new($"Filename: {Filename}"),
                new($"Duration: {Duration}")
            ])];
}

public class Video(ContentReader r) : IHaveMetaInfo {
    public readonly string Filename = r.ReadObject<string>();
    public readonly int Duration = r.ReadObject<int>();
    public readonly int Width = r.ReadObject<int>();
    public readonly int Height = r.ReadObject<int>();
    public readonly float FramesPerSecond = r.ReadObject<float>();
    public readonly SoundtrackType SoundtrackType = (SoundtrackType)r.ReadObject<int>();
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
